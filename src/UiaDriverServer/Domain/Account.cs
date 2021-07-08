using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UiaDriverServer.Extensions;

namespace UiaDriverServer.Domain
{
    internal static class Account
    {
        /// <summary>
        /// gets a list of available features
        /// </summary>
        /// <param name="user">user name</param>
        /// <param name="password">password</param>
        /// <returns>list of available feature-code for the provided user</returns>
        public static IEnumerable<string> GetFeatures(string user, string password)
        {
            // DO NOT EXPOSE AS CONST OR CLASS MEMEBER
            const string KEY = "FeatureSuperSecretEncryptionKey";
            const string ENDPOINT = "https://gravityapi.azurewebsites.net/api/Account/GetFeatures";
            const string MEDIA_TYPE = "application/json";

            var json = JsonConvert.SerializeObject(new { Email = user, Password = password, RememberMe = false });
            var requestBody = json.Encrypt(KEY);

            using (var client = new HttpClient())
            {
                var content = new StringContent(requestBody, Encoding.UTF8, MEDIA_TYPE);
                var response = client.PostAsync(ENDPOINT, content).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    return new string[0];
                }
                var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var jtoken = JToken.Parse(responseBody)["value"];
                return jtoken.ToObject<string[]>();
            }
        }
    }
}
