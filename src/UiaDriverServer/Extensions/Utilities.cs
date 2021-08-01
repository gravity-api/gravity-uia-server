using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

using UIAutomationClient;

namespace UiaDriverServer.Extensions
{
    internal static class Utilities
    {
        /// <summary>
        /// web-driver element reference key - must be returned with element object value
        /// </summary>
        public const string EelementReference = "element-6066-11e4-a52e-4f735466cecf";

        /// <summary>
        /// gets the local IP address of the host machine
        /// </summary>
        /// <returns>local ip address if exists</returns>
        public static string GetLocalEndpoint()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
                {
                    if (string.IsNullOrEmpty(ip.ToString())) continue;
                    return ip.ToString();
                }
                throw new KeyNotFoundException("local IPvP address not found");
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// gets the json response settings and formatting
        /// </summary>
        /// <returns>settings for Newtonsoft.Json.JsonSerializer object</returns>
        public static JsonSerializerSettings GetJsonSettings() => new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// get element runtime-id based on it's COM runtime property
        /// </summary>
        /// <param name="domRuntime"></param>
        /// <returns>automation element runtime id</returns>
        public static int[] GetRuntime(string domRuntime) => JArray.Parse(domRuntime).ToObject<int[]>();

        /// <summary>
        /// create global cache request for elements properties
        /// </summary>
        /// <param name="automation">automation to get request for</param>
        /// <returns>cache request</returns>
        public static IUIAutomationCacheRequest GetCacheRequest(this CUIAutomation8 automation)
        {
            // create request
            var r = automation.CreateCacheRequest();

            // add patterns
            r.AddPattern(UIA_PatternIds.UIA_TextChildPatternId);
            r.AddPattern(UIA_PatternIds.UIA_TextEditPatternId);
            r.AddPattern(UIA_PatternIds.UIA_TextPattern2Id);
            r.AddPattern(UIA_PatternIds.UIA_TextPatternId);

            // add properties
            r.AddProperty(UIA_PropertyIds.UIA_AcceleratorKeyPropertyId);
            r.AddProperty(UIA_PropertyIds.UIA_AccessKeyPropertyId);

            // tree scope
            r.TreeScope = TreeScope.TreeScope_Descendants;
            r.TreeFilter = automation.CreateTrueCondition();
            return r;
        }

        /// <summary>
        /// Gets the primary screen full resolution.
        /// </summary>
        /// <returns>Primary screen full resolution.</returns>
        public static (int Width, int Height) GetScreenResultion()
        {
            // setup
            var query = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            var queryCollection = query.Get();

            // build
            var videoModeDescription = GetVideoModeDescription(queryCollection);
            var x = Regex.Match(videoModeDescription, @"^\d+").Value;
            var y = Regex.Match(videoModeDescription, @"(?<=x\s+)\d+(?=\s+x)").Value;

            // parse
            _ = int.TryParse(x, out int xOut);
            _ = int.TryParse(y, out int yOut);

            // get
            return (xOut, yOut);
        }

        private static string GetVideoModeDescription(ManagementObjectCollection queryCollection)
        {
            foreach (var managementObject in queryCollection)
            {
                var propertyDataCollection = managementObject.Properties;
                foreach (var propertyData in propertyDataCollection)
                {
                    if (propertyData.Name.Equals("VideoModeDescription", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"{propertyData.Value}";
                    }
                }
            }
            return string.Empty;
        }
    }
}
