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
            try
            {
                Run().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine("FATAL: " + ex);
            }
        }

        static async System.Threading.Tasks.Task Run()
        {
            var tokenRepo = new TokenRepository();
            var oauth = new OAuthService(clientId, clientSecret, redirectUri, tokenRepo);
            var uploadSvc = new UploadService();
            var pathRepo = new PathRepository();
            var logRepo = new LogRepository();

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
                Console.WriteLine("Scan folder: " + p.SourcePath);

                if (!Directory.Exists(p.SourcePath))
                {
                    Console.WriteLine("Skip. Folder tidak ada: " + p.SourcePath);
                    continue;
                }

                var files = Directory.GetFiles(p.SourcePath);
                foreach (var f in files)
                {
                    logRepo.InsertPendingIfNeeded(f, p.TargetFolderId);
                }

                var pending = logRepo.LoadPending(p.TargetFolderId);

                foreach (var item in pending)
                {
                    var res = await uploadSvc.UploadFile(accessToken, item.FullPath, item.BoxFolderId);
                    logRepo.UpdateStatus(item.Id, res.Status, res.Message);
                    Console.WriteLine($"{item.FileName} → {res.Status}");
                }
            }

            Console.WriteLine("=== SELESAI ===");
        }
    }
}
