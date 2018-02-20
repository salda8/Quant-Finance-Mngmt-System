using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Server.Utils
{
    public static class EncryptionUtils
    {
        /// <summary>
        ///     Used for decrypting db passwords.
        /// </summary>
        public static string Unprotect(string encryptedString)
        {
            byte[] buffer;
            try
            {
                buffer = ProtectedData.Unprotect(Convert.FromBase64String(encryptedString), null,
                    DataProtectionScope.CurrentUser);
            }
            catch (Exception)
            {
                return "";
            }
            return Encoding.Unicode.GetString(buffer);
        }

        /// <summary>
        ///     Used for encrypting db passwords.
        /// </summary>
        public static string Protect(string unprotectedString)
        {
            var buffer = ProtectedData.Protect(Encoding.Unicode.GetBytes(unprotectedString), null,
                DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(buffer);
        }
    }

    public static class ServerUtils
    {
        public static bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    if (!success)
                    {
                        return false;
                    }

                    client.EndConnect(result);
                }

            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}