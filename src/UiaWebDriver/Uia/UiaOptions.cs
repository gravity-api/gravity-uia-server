/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments & document reference
 *    - modify: add support for arguments
 *    - modify: add support for tree-scope
 */
using OpenQA.Selenium.Extensions;
using OpenQA.Selenium.Remote;
using System;
using System.IO;

namespace OpenQA.Selenium.Uia
{
    public class UiaOptions : DriverOptions
    {
        // the dictionary of capabilities
        private readonly UiaCapabilities capabilities = new();

        /// <summary>
        /// initialize an object for managing options specific to a browser driver
        /// </summary>
        public UiaOptions()
        {
            PlatformName = "windows";
        }

        /// <summary>
        /// gets or sets path to application file
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        /// gets or sets command line arguments to pass to the application
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// gets or sets OS version
        /// </summary>
        public string PlatfromVersion => Environment.OSVersion.VersionString;

        /// <summary>
        /// sets or sets value that specify the scope of elements within the UI Automation tree
        /// </summary>
        public TreeScope TreeScope { get; set; }

        /// <summary>
        /// gets or sets the option to refresh application DOM
        /// on element interactions. when set to true, this option will provide
        /// more updated and accurate DOM, but will reduce performance. use it for
        /// development and set it to false when deploy to production
        /// </summary>
        public bool DevelopmentMode { get; set; }

        /// <summary>
        /// the full path to the directory containing UiaWebDriver.exe
        /// </summary>
        public string DriverPath { get; set; } = Environment.CurrentDirectory;

        /// <summary>
        /// Prefer native events over patterns when possible.
        /// </summary>
        public bool UseNativeEvents { get; set; } = false;

        /// <summary>
        /// Turn the capabilities into an desired capability
        /// </summary>
        /// <returns>A desired capability</returns>
        public override ICapabilities ToCapabilities()
        {
            Evaluate();

            // shortcuts
            var dictionary = capabilities.CapabilitiesDictionary;

            // apply known capabilities
            var args = string.IsNullOrEmpty(Arguments) ? string.Empty : Arguments;
            var scope = Enum.GetName(typeof(TreeScope), TreeScope);

            dictionary.AddOrReplace(CapabilityType.BrowserName, BrowserName);
            dictionary.AddOrReplace(CapabilityType.BrowserVersion, BrowserVersion);
            dictionary.AddOrReplace(CapabilityType.PlatformName, PlatformName);
            dictionary.AddOrReplace(CapabilityType.Proxy, Proxy);
            dictionary.AddOrReplace(CapabilityType.UnhandledPromptBehavior, UnhandledPromptBehavior);
            dictionary.AddOrReplace(CapabilityType.PageLoadStrategy, PageLoadStrategy);
            dictionary.AddOrReplace(UiaCapability.PlatformVersion, PlatfromVersion);
            dictionary.AddOrReplace(UiaCapability.Application, Application);
            dictionary.AddOrReplace(UiaCapability.Arguments, args);
            dictionary.AddOrReplace(UiaCapability.TreeScope, scope);
            dictionary.AddOrReplace(UiaCapability.DevMode, DevelopmentMode);
            dictionary.AddOrReplace(UiaCapability.DriverPath, DriverPath);
            dictionary.AddOrReplace(UiaCapability.UseNativeEvents, UseNativeEvents);
            return capabilities;
        }

        private void Evaluate()
        {
            if (string.IsNullOrEmpty(Application))
            {
                const string message = "application property cannot " +
                    "be null or empty, please provide valid application path";
                throw new FileNotFoundException(message);
            }
        }
    }
}