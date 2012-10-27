using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Utilities.Security
{
    public class ProtectedData
    {
        public static string Unprotect(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            byte[] encryptedBytes = Convert.FromBase64String(text);
            byte[] decryptedBytes = System.Security.Cryptography.ProtectedData.Unprotect(encryptedBytes, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            string result = Encoding.UTF8.GetString(decryptedBytes);
            return result;
        }

        public static string Protect(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            byte[] plainBytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedBytes = System.Security.Cryptography.ProtectedData.Protect(plainBytes, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            string result = Convert.ToBase64String(encryptedBytes);
            return result;
        }
    }
}
