using System.Data.SqlClient;
using Job_MBSS.Models;

namespace Job_MBSS.Data
{
    public class TokenRepository
    {
        public TokenPair GetTokens()
        {
            using (var rd = SqlHelper.ExecuteReader("dbo.sp_Tokens_Get"))
            {
                if (rd.Read())
                {
                    var pair = new TokenPair
                    {
                        AccessToken = rd.IsDBNull(0) ? null : rd.GetString(0),
                        RefreshToken = rd.IsDBNull(1) ? null : rd.GetString(1)
                    };

                    return pair;
                }
            }

            return new TokenPair();
        }

        public void SaveTokens(TokenPair pair)
        {
            SqlHelper.ExecuteNonQuery(
                "dbo.sp_Tokens_Save",
                new SqlParameter("@AccessToken",
                    (object)pair.AccessToken ?? (object)System.DBNull.Value),
                new SqlParameter("@RefreshToken",
                    (object)pair.RefreshToken ?? (object)System.DBNull.Value)
            );
        }
    }
}
