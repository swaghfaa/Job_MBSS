using System;
using System.Collections.Generic;
using System.IO;
using Job_MBSS.Data;
using Job_MBSS.Models;
using Job_MBSS.Services;

namespace Job_MBSS
{
    class Program
    {
        static string clientId = "xzsj9vqi91v7tn5dsre48ugtobbazfae";
        static string clientSecret = "LUOFdxo2sqLyttxPX9XhEv0kz8Qw4P9n";
        static string redirectUri = "http://localhost:5000/callback";

        static void Main(string[] args)
        {
            try { Run().GetAwaiter().GetResult(); }
            catch (Exception ex) { Console.WriteLine("FATAL: " + ex); }
        }

        static async System.Threading.Tasks.Task Run()
        {
            var tokenRepo = new TokenRepository();
            var oauth = new OAuthService(clientId, clientSecret, redirectUri, tokenRepo);
            var uploadSvc = new UploadService();
            var pathRepo = new PathRepository();
            var logRepo = new LogRepository();
            var fileRepo = new BoxFileRepository();

            var accessToken = await oauth.GetOrRefreshAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("Gagal mendapatkan access token.");
                return;
            }

            Console.WriteLine("=== MULAI PROSES ===");
            var paths = pathRepo.GetActivePaths();

            foreach (var p in paths)
            {
                Console.WriteLine("Scan: " + p.SourcePath);

                if (!Directory.Exists(p.SourcePath))
                {
                    Console.WriteLine("Skip. Folder tidak ada: " + p.SourcePath);
                    continue;
                }

                // queue only if changed
                foreach (var f in Directory.GetFiles(p.SourcePath))
                {
                    var mtime = File.GetLastWriteTimeUtc(f);
                    logRepo.QueueIfChanged(f, p.TargetFolderId, mtime);
                }

                // load pending and upload
                var pending = logRepo.LoadPending(p.TargetFolderId);

                foreach (var item in pending)
                {
                    var localModUtc = item.LocalModifiedAt ?? File.GetLastWriteTimeUtc(item.FullPath);
                    var existing = fileRepo.GetByFullPath(item.FullPath);

                    UploadResult res;
                    if (existing == null)
                    {
                        // first time upload
                        res = await uploadSvc.UploadNew(accessToken, item.FullPath, item.BoxFolderId);
                    }
                    else
                    {
                        // only version if truly newer
                        if (localModUtc <= existing.LocalModifiedAt)
                        {
                            res = new UploadResult
                            {
                                Success = true,
                                Status = "SkipNotModified",
                                Message = "Local file not newer than BoxFiles.LocalModifiedAt.",
                                BoxFileId = existing.BoxFileId,
                                LocalModifiedAt = localModUtc
                            };
                        }
                        else
                        {
                            res = await uploadSvc.UploadNewVersion(accessToken, item.FullPath, existing.BoxFileId, existing.ETag);
                        }
                    }
                    var folderRepo = new FolderRepository();
                    // Upsert BoxFiles if we know file id (Success/Exists/Versioned)
                    if (res.Success && !string.IsNullOrEmpty(res.BoxFileId))
                    {
                        // Pastikan folder ada di BoxFolders
                        folderRepo.Upsert(item.BoxFolderId, Path.GetFileName(p.SourcePath));

                        // Baru upsert file
                        fileRepo.UpsertAfterUpload(
                            item.FullPath,
                            item.BoxFolderId,
                            Path.GetFileName(item.FullPath),
                            res.BoxFileId,
                            res.LocalModifiedAt ?? File.GetLastWriteTimeUtc(item.FullPath),
                            res.ETag,
                            res.Sha1,
                            res.VersionNumber
                        );
                    }

                    // log status
                    logRepo.UpdateStatus(item.Id, res.Status, res.Message, res.BoxFileId, res.LocalModifiedAt);
                    Console.WriteLine($"{item.FileName} → {res.Status}");
                }
            }

            Console.WriteLine("=== SELESAI ===");
        }
    }
}
