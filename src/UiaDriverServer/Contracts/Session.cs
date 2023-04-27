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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Xml.Linq;

using UiaDriverServer.Extensions;

using UIAutomationClient;

// TODO: fix all cross references and spaghetti code.
namespace UiaDriverServer.Contracts
{
    internal class Session
    {
        public Session()
        { }

        public Session(CUIAutomation8 automation)
            :this(automation, default)
        { }
        
        public Session(CUIAutomation8 automation, Process application)
        {
            Automation = automation;
            Application = application;
            Elements = new ConcurrentDictionary<string, Element>();
            ScreenResolution = Utilities.GetScreenResultion();
        }

        /// <summary>
        /// current session-id, this property will be passed by command-executor
        /// </summary>
        public string SessionId { get; set; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// response value data (holds all capabilities settings)
        /// </summary>
        public Dictionary<string, object> Capabilities { get; set; }

        /// <summary>
        /// gets the current application run-time id
        /// </summary>
        public IEnumerable<int> Runtime { get; set; }

        /// <summary>
        /// Gets the primary screen Width and Height.
        /// </summary>
        public (int Width, int Height) ScreenResolution { get; }

        /// <summary>
        /// the application which is under the current session
        /// </summary>
        [JsonIgnore]
        public Process Application { get; set; }

        /// <summary>
        /// the DOM of the application which is under the current session
        /// </summary>
        [JsonIgnore]
        public XDocument Dom { get; set; }

        /// <summary>
        /// gets or sets a collection of cached elements across a given session
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, Element> Elements { get; set; }

        /// <summary>
        /// main automation object for this session
        /// </summary>
        [JsonIgnore]
        public CUIAutomation8 Automation { get; }

        /// <summary>
        /// contains values that specify the scope of elements within the UI Automation tree
        /// </summary>
        [JsonIgnore]
        public TreeScope TreeScope { get; set; } = TreeScope.TreeScope_Descendants;
    }
}