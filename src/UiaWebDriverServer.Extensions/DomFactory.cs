/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using UIAutomationClient;

namespace UiaWebDriverServer.Extensions
{
    public partial class DomFactory
    {
        [GeneratedRegex("$\\s+^")]
        private static partial Regex GetXmlAttributePattern();

        // members: state
        private readonly IUIAutomationElement _rootElement;
        private readonly TreeScope _treeScope;

        /// <summary>
        /// generates a new DOM-factory instance to create a DOM based on automation-element
        /// </summary>
        public DomFactory()
            : this(new CUIAutomation8().GetRootElement())
        { }

        /// <summary>
        /// generates a new DOM-factory instance to create a DOM based on automation-element
        /// </summary>
        /// <param name="rootElement">Root element to start building the DOM from.</param>
        public DomFactory(IUIAutomationElement rootElement)
        {
            _rootElement = rootElement;
        }

        /// <summary>
        /// create virtual DOM for the current application
        /// </summary>
        /// <returns>virtual DOM</returns>
        public XDocument New()
        {
            // setup
            var automation = new CUIAutomation8();
            var element = _rootElement ?? automation.GetRootElement();

            // get
            return New(automation, element);
        }

        /// <summary>
        /// create virtual DOM for the current application
        /// </summary>
        /// <returns>virtual DOM</returns>
        public static XDocument New(IUIAutomationElement element)
        {
            // setup
            var automation = new CUIAutomation8();

            // get
            return New(automation, element);
        }

        private static XDocument New(CUIAutomation8 automation, IUIAutomationElement element)
        {
            // setup
            var xmlData = RegisterNewDom(automation, element);
            var xml = "<Root>" + string.Join("\n", xmlData) + "</Root>";

            // get
            try
            {
                return XDocument.Parse(xml);
            }
            catch (Exception e) when (e != null)
            {
                return XDocument.Parse($"<Root><Error>{e.GetBaseException().Message}</Error></Root>");
            }
        }

        // Utilites
        private static IEnumerable<string> RegisterNewDom(CUIAutomation8 automation, IUIAutomationElement element)
        {
            // setup
            var xml = new List<string>();

            // setup: open tag
            var tagName = element.GetTagName();
            var attributes = GetElementAttributes(element);

            // apply open tag
            xml.Add($"<{tagName} {attributes}>");

            // iterate
            var condition = automation.CreateTrueCondition();
            var treeWalker = automation.CreateTreeWalker(condition);
            var childElement = treeWalker.GetFirstChildElement(element);
            while (childElement != null)
            {
                var nodeXml = RegisterNewDom(automation, childElement);
                xml.AddRange(nodeXml);
                childElement = treeWalker.GetNextSiblingElement(childElement);
            }

            // setup: close tag
            xml.Add($"</{tagName}>");

            // get
            return xml;
        }

        private static string GetElementAttributes(IUIAutomationElement element)
        {
            // load attributes
            var attributes = element.GetAttributes();
            var runtime = element.GetRuntimeId().OfType<int>();
            var id = JsonSerializer.Serialize(runtime);
            attributes.Add("id", id);

            // initialize XML row
            var xmlNode = new List<string>();
            foreach (var item in attributes)
            {
                if (string.IsNullOrEmpty(item.Key) || GetXmlAttributePattern().IsMatch(item.Key))
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
