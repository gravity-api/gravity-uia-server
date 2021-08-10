/*
 * CHANGE LOG - keep only last 5 threads
 */
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UiaDriverServer.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// converts to camel case
        /// Location_ID => LocationId, and testLEFTSide => TestLeftSide
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <returns>converted string</returns>
        public static string ToCamelCase(this string s)
        {
            return (char.ToLowerInvariant(s[0]) + s[1..]).Replace("_", string.Empty);
        }

        /// <summary>
        /// parse illegal xml chars
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <returns>converted string</returns>
        public static string ParseForXml(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return s.Replace("&", "&amp;").Replace("\"", "&quot;");
        }

        /// <summary>
        /// Encrypts a string using the provided encryption key
        /// </summary>
        /// <param name="clearText">string to encrypt</param>
        /// <param name="key">encryption key to use for encryption</param>
        /// <returns>encrypted string</returns>
        public static string Encrypt(this string clearText, string key)
        {
            var encryptionKey = key;
            var clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using var memoryStream = new MemoryStream();
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(clearBytes, 0, clearBytes.Length);
                }
                clearText = Convert.ToBase64String(memoryStream.ToArray());
            }
            return clearText;
        }
    }
}