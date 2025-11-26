using System;
using System.Text;
using System.Security.Cryptography;

namespace Job_MBSS.Security
{
    public static class TokenCrypto
    {
        private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("Job_MBSS-DPAPI-Entropy-2025");

        public static string ProtectToBase64(string plain)
        {
            var data = Encoding.UTF8.GetBytes(plain);
            var enc = ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(enc);
        }

        public static string UnprotectToString(string base64)
        {
            var enc = Convert.FromBase64String(base64);
            var dec = ProtectedData.Unprotect(enc, _entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(dec);
        }
    }
}
