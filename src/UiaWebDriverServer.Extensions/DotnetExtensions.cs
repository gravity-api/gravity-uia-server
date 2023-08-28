/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;

using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text.Json;
using System.Threading.Tasks;

namespace UiaWebDriverServer.Extensions
{
    public static class DotnetExtensions
    {
        #region *** Read Request ***
        /// <summary>
        /// Deserialize a <see cref="HttpRequest.Body"/> into an object of a type.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to deserialize the <see cref="HttpRequest"/> to.</typeparam>
        /// <param name="request">The <see cref="HttpRequest"/> to deserialize.</param>
        /// <returns>An object of the given type.</returns>
        public static async Task<T> ReadAsAsync<T>(this HttpRequest request)
        {
            // read content
            var requestBody = await Read(request).ConfigureAwait(false);

            // exit conditions
            if (!AssertJson(requestBody))
            {
                throw new NotSupportedException("The request body must be JSON formatted.");
            }

            // deserialize into object
            return JsonSerializer.Deserialize<T>(requestBody);
        }

        /// <summary>
        /// Reads a <see cref="HttpRequest.Body"/> object.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> to read.</param>
        /// <returns>The <see cref="HttpRequest.Body"/> as <see cref="string"/>.</returns>
        public static Task<string> ReadAsync(this HttpRequest request)
        {
            return Read(request);
        }

        private static async Task<string> Read(HttpRequest request)
        {
            using var streamReader = new StreamReader(request.Body);
            return await streamReader.ReadToEndAsync().ConfigureAwait(false);
        }
        #endregion

        public static string GetNameOrFile(this Process process)
        {
            try
            {
                return process.StartInfo.FileName;
            }
            catch (Exception e) when (e != null)
            {
                return process.ProcessName;
            }
        }

        /// <summary>
        /// converts to camel case
        /// Location_ID => LocationId, and testLEFTSide => TestLeftSide
        /// </summary>
        /// <param name="input">string to convert</param>
        /// <returns>converted string</returns>
        public static string ToCamelCase(this string input)
        {
            return (char.ToLowerInvariant(input[0]) + input[1..]).Replace("_", string.Empty);
        }

        /// <summary>
        /// parse illegal XML chars
        /// </summary>
        /// <param name="input">string to convert</param>
        /// <returns>converted string</returns>
        public static string ParseForXml(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            return input
                .Replace("&", "&amp;")
                .Replace("\"", "&quot;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        /// <summary>
        /// Converts a stream into a Base64 string.
        /// </summary>
        /// <param name="stream">The stream to convert.</param>
        /// <returns>Base64 string.</returns>
        public static string ConvertToBase64(this Stream stream)
        {
            if (stream is MemoryStream memoryStream)
            {
                return Convert.ToBase64String(memoryStream.ToArray());
            }

            var bytes = new byte[(int)stream.Length];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, (int)stream.Length);

            return Convert.ToBase64String(bytes);
        }

        public static SecureString ConvertToSecureString(this string str)
        {
            // normalize
            str = string.IsNullOrEmpty(str) ? string.Empty : str;

            // setup
            var securePassword = new SecureString();

            // build
            foreach (var character in str)
            {
                securePassword.AppendChar(character);
            }

            // get
            return securePassword;
        }

        // Utilities
        private static bool AssertJson(string json)
        {
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch (Exception e) when(e!=null)
            {
                return false;
            }
        }
    }
}
