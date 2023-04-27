/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Xml.Linq;

using UIAutomationClient;

namespace UiaWebDriverServer.Contracts
{
    public class Session
    {
        public Session()
            : this(new CUIAutomation8())
        { }

        public Session(CUIAutomation8 automation)
            : this(automation, default)
        { }

        public Session(CUIAutomation8 automation, Process application)
            : this(automation, application, TreeScope.TreeScope_Children)
        { }

        public Session(CUIAutomation8 automation, Process application, TreeScope treeScope)
        {
            Automation = automation;
            Application = application;
            Elements = new ConcurrentDictionary<string, Element>();

            // root element
            var id = application?.Id == null ? -1 : application.Id;
            var condition = automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ProcessIdPropertyId, id);
            var applicationRoot = automation.GetRootElement().FindFirst(treeScope, condition);

            ApplicationRoot = applicationRoot ?? automation.GetRootElement();
            SessionId = $"{id}";
        }

        /// <summary>
        /// current session-id, this property will be passed by command-executor
        /// </summary>
        [DataMember]
        public string SessionId { get; set; }

        [DataMember]
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// response value data (holds all capabilities settings)
        /// </summary>
        [DataMember]
        public IDictionary<string, object> Capabilities { get; set; }

        /// <summary>
        /// gets the current application run-time id
        /// </summary>
        [DataMember]
        public IEnumerable<int> Runtime { get; set; }

        [DataMember]
        public double ScaleRatio { get; set; }

        [JsonIgnore]
        public IUIAutomationElement ApplicationRoot { get; set; }

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
