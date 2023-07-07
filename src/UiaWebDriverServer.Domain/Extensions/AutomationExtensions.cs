using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;

using UIAutomationClient;
using UiaWebDriverServer.Contracts;
using UiaWebDriverServer.Extensions;

namespace UiaWebDriverServer.Domain.Extensions
{
    public static class AutomationExtensions
    {
        /// <summary>
        /// Gets an <see cref="IUIAutomationElement"/> from root scope (desktop).
        /// </summary>
        /// <param name="session">Session to search.</param>
        /// <param name="locationStrategy"></param>
        /// <returns>A <see cref="IUIAutomationElement"/>.</returns>
        public static (IUIAutomationElement, XNode, string) GetFromRoot(this Session session, LocationStrategy locationStrategy)
        {
            // bad request
            if (!locationStrategy.Value.StartsWith("//root"))
            {
                return (null, null, null);
            }
            locationStrategy.Value = locationStrategy.Value.Replace("//root", string.Empty);

            // build
            var dom = DomFactory.New(session.ApplicationRoot);
            var domElement = dom.XPathSelectElement(locationStrategy.Value);
            var domRuntime = domElement?.Attribute("id").Value;

            // get container
            var containerElement = session.Automation.GetRootElement();

            // create finding condition
            var runtime = JsonSerializer.Deserialize<IEnumerable<int>>(domRuntime).ToArray();
            var c = session.Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_RuntimeIdPropertyId, runtime);

            // get element
            return (containerElement.FindFirst(TreeScope.TreeScope_Descendants, c), domElement, domRuntime);
        }

        public static IEnumerable<IUIAutomationElement> FindElements(this IUIAutomationElement element, IUIAutomationCondition condition, TreeScope scope)
        {
            // setup
            var elementsArray = element.FindAll(scope, condition);

            // not found
            if(elementsArray == null || elementsArray.Length == 0)
            {
                return Array.Empty<IUIAutomationElement>();
            }

            // build
            var elements = new List<IUIAutomationElement>();
            for (int i = 0; i < elementsArray.Length; i++)
            {
                elements.Add(elementsArray.GetElement(i));
            }

            // get
            return elements;
        }
    }
}
