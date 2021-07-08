using OpenQA.Selenium.Remote;
using System.Collections.Generic;

namespace OpenQA.Selenium.Extensions
{
    internal static class CapabilitiesExtensions
    {
        public static Dictionary<string, object> ToDictionary<T>(this ICapabilities capabilities, bool checkName) where T : ICapabilities
        {
            // initialize dictionary
            var capabilitiesDictionary = new Dictionary<string, object>();

            // extract nuder-layer capabilities object
            dynamic capabilitiesObject = (T)capabilities;

            // add capabilities
            foreach (KeyValuePair<string, object> entry in capabilitiesObject.CapabilitiesDictionary)
            {
                // exit conditions
                if (!CapabilityType.IsSpecCompliantCapabilityName(entry.Key) && checkName) continue;

                // append capability
                capabilitiesDictionary.Add(entry.Key, entry.Value);
            }

            // complete pipeline
            return capabilitiesDictionary;
        }
    }
}
