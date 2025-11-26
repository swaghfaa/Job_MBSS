using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Job_MBSS.Models;

namespace Job_MBSS.Data
{
    public class LogRepository
    {
        public void InsertPendingIfNeeded(string fullPath, string boxFolderId)
        {
            SqlHelper.ExecuteNonQuery("dbo.sp_Log_InsertPendingIfNeeded",
                new SqlParameter("@FullPath", fullPath),
                new SqlParameter("@BoxFolderId", boxFolderId)
            );
        }

        public List<UploadItem> LoadPending(string boxFolderId)
        {
            var list = new List<UploadItem>();
            using (var rd = SqlHelper.ExecuteReader("dbo.sp_Log_LoadPending",
                new SqlParameter("@BoxFolderId", (object)boxFolderId ?? (object)System.DBNull.Value)))
            {
                while (rd.Read())
                {
                    var item = new UploadItem();
                    item.Id = rd.GetInt32(0);
                    item.FileName = rd.GetString(1);
                    item.FullPath = rd.GetString(2);
                    item.BoxFolderId = rd.GetString(3);
                    list.Add(item);
                }
            }
            return list;
        }

        public void UpdateStatus(int id, string status, string message)
        {
            SqlHelper.ExecuteNonQuery("dbo.sp_Log_UpdateStatus",
                new SqlParameter("@Id", id),
                new SqlParameter("@Status", status),
                new SqlParameter("@ResponseMessage", (object)message ?? (object)System.DBNull.Value)
            );
        }
    }
}
