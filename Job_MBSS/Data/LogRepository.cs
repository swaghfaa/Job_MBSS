using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Job_MBSS.Models;

namespace Job_MBSS.Data
{
    public class LogRepository
    {
        public void QueueIfChanged(string fullPath, string boxFolderId, DateTime localModifiedUtc)
        {
            SqlHelper.ExecuteNonQuery("dbo.sp_Log_QueueIfChanged",
                new SqlParameter("@FullPath", fullPath),
                new SqlParameter("@BoxFolderId", boxFolderId),
                new SqlParameter("@LocalModifiedAt", localModifiedUtc));
        }

        public List<UploadItem> LoadPending(string boxFolderId)
        {
            var list = new List<UploadItem>();
            using (var rd = SqlHelper.ExecuteReader("dbo.sp_Log_LoadPending",
                new SqlParameter("@BoxFolderId", (object)boxFolderId ?? (object)DBNull.Value)))
            {
                while (rd.Read())
                {
                    var item = new UploadItem
                    {
                        Id = rd.GetInt32(0),
                        FileName = rd.GetString(1),
                        FullPath = rd.GetString(2),
                        BoxFolderId = rd.GetString(3),
                        LocalModifiedAt = rd.IsDBNull(4) ? (DateTime?)null : rd.GetDateTime(4),
                        BoxFileId = rd.IsDBNull(5) ? null : rd.GetString(5)
                    };
                    list.Add(item);
                }
            }
            return list;
        }

        public void UpdateStatus(int id, string status, string message, string boxFileId, DateTime? localModifiedUtc)
        {
            SqlHelper.ExecuteNonQuery("dbo.sp_Log_UpdateStatus",
                new SqlParameter("@Id", id),
                new SqlParameter("@Status", status),
                new SqlParameter("@ResponseMessage", (object)message ?? (object)DBNull.Value),
                new SqlParameter("@BoxFileId", (object)boxFileId ?? (object)DBNull.Value),
                new SqlParameter("@LocalModifiedAt", (object)localModifiedUtc ?? (object)DBNull.Value)
            );
        }
    }
}
