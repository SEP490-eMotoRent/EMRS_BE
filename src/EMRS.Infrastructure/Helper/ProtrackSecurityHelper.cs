using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Helper
{
    public static class SecurityHelper
    {


        public static string Encrypt(string plainText, string key, string iv)
        {
            try
            {
                var keyBytes = Encoding.UTF8.GetBytes(key);
                var ivBytes = Encoding.UTF8.GetBytes(iv);
                Console.WriteLine($"Key length: {keyBytes.Length} bytes");
                Console.WriteLine($"IV length: {ivBytes.Length} bytes");
                using var aes = Aes.Create();
              
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor();
                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    var bytes = Encoding.UTF8.GetBytes(plainText);
                    cs.Write(bytes, 0, bytes.Length);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Encrypt: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        public static string Decrypt(string cipherText, string key, string iv)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                var cipherBytes = Convert.FromBase64String(cipherText);
                using var ms = new MemoryStream(cipherBytes);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Decrypt: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        public static string GetMd5Hash(string input)
        {
            try
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(input);
                data = System.Security.Cryptography.MD5.Create().ComputeHash(data);
                return BitConverter.ToString(data).Replace("-", "").ToLower();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetMd5Hash: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }
    }
}
