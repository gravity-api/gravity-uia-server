/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments & document reference
 *    
 * 2019-02-07
 *    - modify: change to automation COM instead of System.Windows.Automation
 * 
 * docs.w3c.web-driver
 * https://www.w3.org/TR/webdriver1/#sessions
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement.rootelement?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement.findfirst?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.treescope?view=netframework-4.7.2
 */
using Newtonsoft.Json;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

using UIAutomationClient;

namespace UiaDriverServer.Dto
{
    internal class Session
    {
        public Session(CUIAutomation8 automation)
        {
            Automation = automation;
            Elements = new ConcurrentDictionary<string, Element>();
        }

        /// <summary>
        /// current session-id, this property will be passed by command-executor
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// response value data (holds all capabilities settings)
        /// </summary>
        public Dictionary<string, object> Capabilities { get; set; }

        /// <summary>
        /// gets the current application run-time id
        /// </summary>
        public int[] Runtime { get; private set; }

        /// <summary>
        /// Gets or set a value indicates if this session is native or not.
        /// </summary>
        public bool IsNative => GetIsNative();

        /// <summary>
        /// the application which is under the current session
        /// </summary>
        [JsonIgnore]
        internal Process Application { get; set; }

        /// <summary>
        /// the DOM of the application which is under the current session
        /// </summary>
        [JsonIgnore]
        internal XDocument Dom { get; set; }

        /// <summary>
        /// gets or sets a collection of cached elements across a given session
        /// </summary>
        [JsonIgnore]
        internal IDictionary<string, Element> Elements { get; set; }

        /// <summary>
        /// main automation object for this session
        /// </summary>
        [JsonIgnore]
        internal CUIAutomation8 Automation { get; }

        /// <summary>
        /// contains values that specify the scope of elements within the UI Automation tree
        /// </summary>
        [JsonIgnore]
        internal TreeScope TreeScope { get; set; } = TreeScope.TreeScope_Descendants;

        // TODO: implement timeouts
        /// <summary>
        /// gets the application main window element
        /// </summary>
        /// <returns>application main window element/returns>
        internal IUIAutomationElement GetApplicationRoot()
        {
            // setup            
            var timeout = TimeSpan.FromSeconds(60);
            var timeoutCounter = TimeSpan.Zero;

            // iterate
            while (timeoutCounter < timeout)
            {
                // shortcuts
                var condition = GetCondition();

                // iterate
                var root = Automation.GetRootElement();
                var application = root.FindFirst(TreeScope.TreeScope_Descendants, condition);

                var isReady = application != null && application.CurrentNativeWindowHandle != default;
                if (isReady)
                {
                    if (Runtime?.Length <= 0)
                    {
                        Runtime = application.GetRuntimeId().Cast<int>().ToArray();
                    }
                    return application;
                }

                Thread.Sleep(100);
                timeoutCounter = timeoutCounter.Add(TimeSpan.FromMilliseconds(100));
            }
            return null;
        }

        private IUIAutomationCondition GetCondition()
        {
            // setup conditions
            var isRuntime = Runtime?.Length > 0;

            // get automation condition-id
            var id = isRuntime
                ? UIA_PropertyIds.UIA_RuntimeIdPropertyId
                : UIA_PropertyIds.UIA_NativeWindowHandlePropertyId;

            // get condition
            return isRuntime
                ? Automation.CreatePropertyCondition(id, Runtime)
                : Automation.CreatePropertyCondition(id, Application.MainWindowHandle);
        }

        private bool GetIsNative()
        {
            // members
            var key = UiaCapability.UseNativeEvents;
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // setup
            var capabilites = Capabilities;
            var isNative = capabilites.ContainsKey(key);

            // get
            return isNative && $"{capabilites[key]}".Equals("true", Compare);
        }
    }
}