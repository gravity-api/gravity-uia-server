using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;

namespace OpenQA.Selenium.Uia
{
    public class UiaCapabilities : ICapabilities
    {
        public Dictionary<string, object> CapabilitiesDictionary { get; } = new Dictionary<string, object>();

        /// <summary>
        /// initializes a new instance of the <see cref="DesiredCapabilities"/> class
        /// </summary>
        /// <param name="browser">name of the browser e.g. firefox, internet explorer, safari</param>
        /// <param name="version">version of the browser</param>
        /// <param name="platform">the platform it works on</param>
        public UiaCapabilities(string browser, string version, Platform platform)
        {
            SetCapability(CapabilityType.BrowserName, browser);
            SetCapability(CapabilityType.Version, version);
            SetCapability(CapabilityType.Platform, platform);
        }

        /// <summary>
        /// initializes a new instance of the <see cref="DesiredCapabilities"/> class
        /// </summary>
        public UiaCapabilities() { }

        /// <summary>
        /// initializes a new instance of the <see cref="Capabilities"/> class
        /// </summary>
        /// <param name="rawMap">dictionary of items for the remote driver</param>
        public UiaCapabilities(IDictionary<string, object> rawMap)
        {
            // exit conditions
            if (rawMap == null) return;

            // iterate row-map dictionary
            foreach (KeyValuePair<string, object> keyValue in rawMap)
            {
                ProcessCapability(keyValue);
            }
        }

        /// <summary>
        /// set a single capability from rowMap constructor
        /// </summary>
        /// <param name="keyValue">capability to process</param>
        private void ProcessCapability(KeyValuePair<string, object> keyValue)
        {
            // non-platform condition
            if (keyValue.Key != CapabilityType.Platform)
            {
                SetCapability(keyValue.Key, keyValue.Value);
                return;
            }

            // platform handler
            var raw = keyValue.Value;
            var rawAsPlatform = raw as Platform;

            //---- parse from string
            if (raw is string rawAsString)
            {
                SetCapability(CapabilityType.Platform, FromString(rawAsString));
            }
            //---- parse from platform
            else if (rawAsPlatform != null)
            {
                SetCapability(CapabilityType.Platform, rawAsPlatform);
            }
        }

        /// <summary>
        /// gets the capability value with the specified name
        /// </summary>
        /// <param name="capabilityName">the name of the capability to get</param>
        /// <returns>the value of the capability</returns>
        public object this[string capabilityName]
        {
            get => FetchCapability(capabilityName);
            set => CapabilitiesDictionary[capabilityName] = value;
        }

        private object FetchCapability(string capabilityName)
        {
            // exit conditions
            if (CapabilitiesDictionary.ContainsKey(capabilityName))
            {
                return CapabilitiesDictionary[capabilityName];
            }

            // error message
            var errorMessage = $"the capability [{capabilityName}] is not present in this set of capabilities";
            throw new ArgumentException(errorMessage, capabilityName);
        }

        /// <summary>
        /// gets a value indicating whether the browser has a given capability
        /// </summary>
        /// <param name="capability">the capability to get</param>
        /// <returns>returns <see langword="true"/> if the browser has the capability; otherwise, <see langword="false"/></returns>
        public bool HasCapability(string capability) => CapabilitiesDictionary.ContainsKey(capability);

        /// <summary>
        /// gets a capability of the browser.
        /// </summary>
        /// <param name="capability">the capability to get</param>
        /// <returns>An object associated with the capability, or <see langword="null"/>
        /// if the capability is not set on the browser</returns>
        public object GetCapability(string capability)
        {
            // exit conditions
            if (!CapabilitiesDictionary.ContainsKey(capability)) return null;

            // fetch capability value
            var capabilityValue = CapabilitiesDictionary[capability];

            // safe cast capability value as string
            var capabilityValueString = CapabilitiesDictionary[capability] as string;

            // platform handler
            if (capability == CapabilityType.Platform && capabilityValueString != null)
            {
                capabilityValue = FromString(capabilityValue.ToString());
            }

            // complete pipeline
            return capabilityValue;
        }

        /// <summary>
        /// Sets a capability of the browser
        /// </summary>
        /// <param name="capability">The capability to get</param>
        /// <param name="capabilityValue">The value for the capability</param>
        public void SetCapability(string capability, object capabilityValue)
        {
            // handle the special case of platform objects. these should
            // be stored in the underlying dictionary as their protocol
            // string representation.
            if (capabilityValue is Platform platformCapabilityValue)
            {
                CapabilitiesDictionary[capability] = platformCapabilityValue.ProtocolPlatformType;
                return;
            }

            // non-platform
            CapabilitiesDictionary[capability] = capabilityValue;
        }

        /// <summary>
        /// creates a <see cref="Platform"/> object from a string name of the platform
        /// </summary>
        /// <param name="platformName">the name of the platform to create</param>
        /// <returns>the Platform object represented by the string name</returns>
        internal Platform FromString(string platformName)
        {
            PlatformType platformTypeFromString = PlatformType.Any;
            try
            {
                platformTypeFromString = (PlatformType)Enum.Parse(typeof(PlatformType), platformName, true);
            }
            catch (ArgumentException)
            {
                // if the requested platform string is not a valid platform type,
                // ignore it and use PlatformType.Any.
            }
            return new Platform(platformTypeFromString);
        }
    }
}