using System.Collections.Generic;
using System.Data.SqlClient;
using Job_MBSS.Models;

namespace Job_MBSS.Data
{
    public class PathRepository
    {
        public List<PathItem> GetActivePaths()
        {
            var list = new List<PathItem>();
            using (var rd = SqlHelper.ExecuteReader("dbo.sp_Paths_GetActive"))
            {
                while (rd.Read())
                {
                    var p = new PathItem();
                    p.Id = rd.GetInt32(0);
                    p.SourcePath = rd.GetString(1);
                    p.TargetFolderId = rd.GetString(2);
                    list.Add(p);
                }
            }
            return list;
        }
    }
}
