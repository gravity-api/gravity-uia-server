using System.Xml.XPath;
using UiaDriverServer.Components;
using UiaDriverServer.Dto;
using UIAutomationClient;

namespace UiaDriverServer.Extensions
{
    internal static class SessionExtensions
    {
        /// <summary>
        /// refresh a session DOM
        /// </summary>
        /// <param name="session">session DOM to refresh</param>
        /// <returns>refreshed session</returns>
        public static Session RefreshDom(this Session session)
        {
            // exit conditions
            var dom = new DomFactory(session).Create();
            if (dom == null)
            {
                return null;
            }

            // refresh DOM
            session.Dom = dom;
            return session;
        }

        /// <summary>
        /// check if session have devMode capability and refresh DOM if dose
        /// </summary>
        /// <param name="session"></param>
        public static void RefreshForDevMode(this Session session)
        {
            // setup conditions
            var isKey = session.Capabilities.ContainsKey("devMode");
            var isDevMode = isKey && bool.TryParse($"{session.Capabilities["devMode"]}", out bool devOut);

            // exit conditions
            if (!isDevMode)
            {
                return;
            }
            session.RefreshDom();
        }

        /// <summary>
        /// gets the runtime-id from the current DOM snapshot
        /// </summary>
        /// <param name="session">session DOM to search</param>
        /// <param name="locationStrategy">w3c web-driver location strategy</param>
        /// <returns>serialized runtime-id</returns>
        public static string GetDomRuntime(this Session session, LocationStrategy locationStrategy)
        {
            var domElement = session.Dom.XPathSelectElement(locationStrategy.Value);
            return domElement?.Attribute("id").Value;
        }

        /// <summary>
        /// gets automation-element based on a given serialized runtime-id
        /// </summary>
        /// <param name="session">session to search</param>
        /// <param name="domRuntime">serialized runtime-id to use</param>
        /// <returns>automation-element</returns>
        public static IUIAutomationElement Get(this Session session, string domRuntime)
        {
            // get container
            var containerElement = session.GetApplicationRoot();

            // create finding condition
            var runtime = Utilities.GetRuntime(domRuntime);
            const int pid = UIA_PropertyIds.UIA_RuntimeIdPropertyId;
            var c = session.Automation.CreatePropertyCondition(pid, runtime);

            // get element
            return containerElement.FindFirst(TreeScope.TreeScope_Descendants, c);
        }
    }
}