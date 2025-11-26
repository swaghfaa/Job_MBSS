using System.Data.SqlClient;
using Job_MBSS.Security;
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
                    var accessEnc = rd.IsDBNull(0) ? null : rd.GetString(0);
                    var refreshEnc = rd.IsDBNull(1) ? null : rd.GetString(1);

                    var pair = new TokenPair();
                    pair.AccessToken = string.IsNullOrEmpty(accessEnc) ? null : TokenCrypto.UnprotectToString(accessEnc);
                    pair.RefreshToken = string.IsNullOrEmpty(refreshEnc) ? null : TokenCrypto.UnprotectToString(refreshEnc);
                    return pair;
                }
            }
            return new TokenPair();
        }

        public void SaveTokens(TokenPair pair)
        {
            var accessEnc = string.IsNullOrEmpty(pair.AccessToken) ? null : TokenCrypto.ProtectToBase64(pair.AccessToken);
            var refreshEnc = string.IsNullOrEmpty(pair.RefreshToken) ? null : TokenCrypto.ProtectToBase64(pair.RefreshToken);

            SqlHelper.ExecuteNonQuery("dbo.sp_Tokens_Save",
                new SqlParameter("@AccessTokenEnc", (object)accessEnc ?? (object)System.DBNull.Value),
                new SqlParameter("@RefreshTokenEnc", (object)refreshEnc ?? (object)System.DBNull.Value)
            );
        }
    }
}
