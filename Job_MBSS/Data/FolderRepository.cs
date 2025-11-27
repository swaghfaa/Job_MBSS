using System.Data.SqlClient;

namespace Job_MBSS.Data
{
    public class FolderRepository
    {
        public void Upsert(string folderId, string name = null)
        {
            SqlHelper.ExecuteNonQuery("dbo.sp_Folder_Upsert",
                new SqlParameter("@BoxFolderId", folderId),
                new SqlParameter("@Name", (object)name ?? System.DBNull.Value));
        }
    }
}
