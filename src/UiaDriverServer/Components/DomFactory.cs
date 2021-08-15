/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments & document reference
 *    - modify: clean code
 *    - modify: add application waiter mechanism
 *    
 * 2019-02-07
 *    - modify: change to automation COM instead of System.Windows.Automation
 *    
 * 2019-02-10
 *    - modify: move element tag-name generation to element extensions
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.propertycondition?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement.rootelement?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement.findfirst?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.xml.linq.xdocument?view=netframework-4.7.2
 */
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using UIAutomationClient;
using UiaDriverServer.Contracts;
using UiaDriverServer.Extensions;
using System.Text.Json;

namespace UiaDriverServer.Components
{
    internal class DomFactory
    {
        // members: state
        private readonly StringBuilder domWriter;
        private readonly IUIAutomationCondition allCondition;
        internal readonly Session session;

        /// <summary>
        /// generates a new DOM-factory instance to create a DOM based on automation-element
        /// </summary>
        /// <param name="session">session by which to generate V-DOM</param>
        public DomFactory(Session session)
        {
            // set automation api
            this.session = session;

            // set DOM-writer string-builder
            domWriter = new StringBuilder();

            // get all conditions
            allCondition = session.Automation.CreateTrueCondition();
        }

        /// <summary>
        /// create virtual DOM for the current application
        /// </summary>
        /// <returns>virtual DOM</returns>
        public XDocument Create()
        {
            return InvokeCreate(session.GetApplicationRoot());
        }

        /// <summary>
        /// create virtual DOM for the current application
        /// </summary>
        /// <returns>virtual DOM</returns>
        public XDocument Create(IUIAutomationElement element)
        {
            return InvokeCreate(element);
        }

        private XDocument InvokeCreate(IUIAutomationElement element)
        {
            GenerateDOM(element);
            var xdocument = $"<root>{domWriter}</root>";
            return XDocument.Parse(xdocument);
        }

        private void GenerateDOM(IUIAutomationElement element)
        {
            // load attributes dictionary
            var attributes = ElementAttributes(element);

            // get tagName
            var tagName = element.GetTagName();

            // add current element row
            domWriter.Append('<').Append(tagName).Append(' ').Append(attributes).AppendLine("> ");

            // exit routine
            var elements = element.FindAll(session.TreeScope, allCondition);
            if (elements.Length == 0)
            {
                domWriter.Append("</").Append(tagName).AppendLine(">");
                return;
            }

            // recursive call
            for (int i = 0; i < elements.Length; i++)
            {
                GenerateDOM(elements.GetElement(i));
            }
            domWriter.Append("</").Append(tagName).AppendLine(">");
        }

        private static string ElementAttributes(IUIAutomationElement element)
        {
            // load attributes
            var attributes = element.GetAttributes();
            var runtime = element.GetRuntimeId().OfType<int>();
            var id = JsonSerializer.Serialize(runtime);
            attributes.Add("id", id);

            // initialize xml row
            var xmlNode = new List<string>();
            foreach (var item in attributes)
            {
                if (string.IsNullOrEmpty(item.Key) || Regex.IsMatch(item.Key, "$\\s+^"))
                {
                    continue;
                }
                if (string.IsNullOrEmpty(item.Value))
                {
                    continue;
                }
                xmlNode.Add($"{item.Key}=\"{item.Value}\"");
            }
            return string.Join(" ", xmlNode);
        }
    }
}