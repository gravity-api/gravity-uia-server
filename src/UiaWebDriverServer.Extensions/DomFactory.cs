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
        /// <param name="root">Root element to start building the DOM from.</param>
        public DomFactory(IUIAutomationElement root)
            :this(root, TreeScope.TreeScope_Children)
        { }

        /// <summary>
        /// generates a new DOM-factory instance to create a DOM based on automation-element
        /// </summary>
        /// <param name="rootElement">Root element to start building the DOM from.</param>
        /// <param name="treeScope">The scope by which to search for elements.</param>
        public DomFactory(IUIAutomationElement rootElement, TreeScope treeScope)
        {
            _rootElement = rootElement;
            _treeScope = treeScope;
        }

        /// <summary>
        /// create virtual DOM for the current application
        /// </summary>
        /// <returns>virtual DOM</returns>
        public XDocument Create()
        {
            return Create(_treeScope, _rootElement);
        }

        /// <summary>
        /// create virtual DOM for the current application
        /// </summary>
        /// <returns>virtual DOM</returns>
        public static XDocument Create(IUIAutomationElement element)
        {
            return Create(TreeScope.TreeScope_Children, element);
        }

        /// <summary>
        /// create virtual DOM for the current application
        /// </summary>
        /// <returns>virtual DOM</returns>
        public static XDocument Create(IUIAutomationElement element, TreeScope treeScope)
        {
            return Create(treeScope, element);
        }

        private static XDocument Create(TreeScope treeScope, IUIAutomationElement element)
        {
            // setup
            var xml = CreateXml(element, treeScope);
            var xmlBody = string.Join("\n", xml);

            // build
            var xdocument = $"<root>{xmlBody}</root>";

            // get
            return XDocument.Parse(xdocument);
        }

        // Utilities
        private static IEnumerable<string> CreateXml(IUIAutomationElement element, TreeScope treeScope)
        {
            // load attributes dictionary
            var attributes = GetElementAttributes(element);
            var condition = new CUIAutomation8().CreateTrueCondition();
            var xml = new List<string>();

            // get tagName
            var tagName = element.GetTagName();

            // add current element row
            xml.Add($"<{tagName} {attributes}>");

            // exit routine
            var elements = element.FindAll(treeScope, condition);
            if (elements.Length == 0)
            {
                xml.Add($"</{tagName}>");
                return xml;
            }

            // iterate
            for (int i = 0; i < elements.Length; i++)
            {
                var nodes = CreateXml(elements.GetElement(i), treeScope);
                xml.AddRange(nodes);
            }
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
