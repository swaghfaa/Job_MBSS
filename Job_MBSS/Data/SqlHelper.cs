using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Job_MBSS.Data
{
    public static class SqlHelper
    {
        private static string ConnStr
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["JobBoxDb"];
                if (cs != null)
                    return cs.ConnectionString;

                // fallback default (Windows Authentication)
                return @"Server=LAPTOP-REQAU0QU\Acer;Database=JobBoxDB;Trusted_Connection=True;";
            }
        }

        public static SqlConnection GetOpenConnection()
        {
            var con = new SqlConnection(ConnStr);
            con.Open();
            return con;
        }

        public static object ExecuteScalar(string spName, params SqlParameter[] parameters)
        {
            using (var con = GetOpenConnection())
            using (var cmd = new SqlCommand(spName, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteScalar();
            }
        }

        public static int ExecuteNonQuery(string spName, params SqlParameter[] parameters)
        {
            using (var con = GetOpenConnection())
            using (var cmd = new SqlCommand(spName, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteNonQuery();
            }
        }

        public static SqlDataReader ExecuteReader(string spName, params SqlParameter[] parameters)
        {
            var con = GetOpenConnection();
            var cmd = new SqlCommand(spName, con) { CommandType = CommandType.StoredProcedure };

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            // close connection otomatis saat reader ditutup
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }
}
