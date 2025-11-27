using System;
using System.Data.SqlClient;
using Job_MBSS.Models;

namespace Job_MBSS.Data
{
    public class BoxFileRepository
    {
        public BoxFileInfo GetByFullPath(string fullPath)
        {
            using (var rd = SqlHelper.ExecuteReader("dbo.sp_File_GetByFullPath",
                new SqlParameter("@FullPath", fullPath)))
            {
                if (rd.Read())
                {
                    return new BoxFileInfo
                    {
                        Id = rd.GetInt32(0),
                        BoxFileId = rd.GetString(1),
                        BoxFolderId = rd.GetString(2),
                        FileName = rd.GetString(3),
                        FullPath = rd.GetString(4),
                        LocalModifiedAt = rd.GetDateTime(5),
                        ETag = rd.IsDBNull(6) ? null : rd.GetString(6),
                        Sha1 = rd.IsDBNull(7) ? null : rd.GetString(7),
                        VersionNumber = rd.IsDBNull(8) ? (int?)null : rd.GetInt32(8)
                    };
                }
            }
            return null;
        }

        public void UpsertAfterUpload(string fullPath, string boxFolderId, string fileName,
                                      string boxFileId, DateTime localModUtc,
                                      string etag, string sha1, int? versionNumber)
        {
            SqlHelper.ExecuteNonQuery("dbo.sp_File_UpsertAfterUpload",
                new SqlParameter("@FullPath", fullPath),
                new SqlParameter("@BoxFolderId", boxFolderId),
                new SqlParameter("@FileName", fileName),
                new SqlParameter("@BoxFileId", boxFileId),
                new SqlParameter("@LocalModifiedAt", localModUtc),
                new SqlParameter("@ETag", (object)etag ?? DBNull.Value),
                new SqlParameter("@Sha1", (object)sha1 ?? DBNull.Value),
                new SqlParameter("@VersionNumber", (object)versionNumber ?? DBNull.Value)
            );
        }
    }
}
