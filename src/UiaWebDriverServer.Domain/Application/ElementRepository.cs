/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;

using UIAutomationClient;

using UiaWebDriverServer.Contracts;
using UiaWebDriverServer.Extensions;

namespace UiaWebDriverServer.Domain.Application
{
    public partial class ElementRepository : IElementRepository
    {
        [GeneratedRegex(@"(?<![\/\.])\/")]
        private static partial Regex GetHierarchyPattern();

        [GeneratedRegex(@"(?<=(\()?(\/\/\*\[\@name=')).*(?=('])(\))?(\[\d+])?)")]
        private static partial Regex GetXpathNamePattern();

        [GeneratedRegex(@"(?<=(\()?(\/\/\*\[\@type=')).*(?=('])(\))?(\[\d+])?)")]
        private static partial Regex GetXpathTypePattern();

        [GeneratedRegex(@"(?<=\[)\d+(?=])")]
        private static partial Regex GetElementIndexPattern();

        // members
        private readonly IDictionary<string, Session> _sessions;

        public ElementRepository(IDictionary<string, Session> sessions)
        {
            _sessions = sessions;
        }

        #region *** Find Element ***
        public (int Status, Element Element) FindElement(string session, LocationStrategy locationStrategy)
        {
            // not found
            if (!_sessions.ContainsKey(session))
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // setup
            var s = _sessions[session];

            // get
            return FindElement(session: s, string.Empty, locationStrategy);
        }

        public (int Status, Element Element) FindElement(string session, string element, LocationStrategy locationStrategy)
        {
            throw new NotImplementedException();
        }

        private static (int Status, Element Element) FindElement(Session session, string element, LocationStrategy locationStrategy)
        {
            // not implemented
            if (locationStrategy.Using != LocationStrategy.Xpath)
            {
                return (StatusCodes.Status501NotImplemented, default);
            }

            // not found
            if (session == null)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // get by name
            if (GetXpathNamePattern().IsMatch(input: locationStrategy.Value))
            {
                return GetByName(session, locationStrategy);
            }

            // get by control type
            if(GetXpathTypePattern().IsMatch(input: locationStrategy.Value))
            {
                return GetByControlType(session, locationStrategy);
            }

            // get by Cords
            var elementByCords = GetByCords(session, locationStrategy);
            if (elementByCords.Status == StatusCodes.Status200OK)
            {
                return elementByCords;
            }

            // get by path
            return GetByPath(session, locationStrategy);
        }

        private static (int Status, Element Element) GetByName(Session session, LocationStrategy locationStrategy)
        {
            // setup
            var name = GetXpathNamePattern().Match(input: locationStrategy.Value).Value;
            var condition = session.Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_NamePropertyId, name);
            var index = GetElementIndexPattern().Match(input: locationStrategy.Value).Value;
            var isIndex = int.TryParse(index, out int indexOut);
            indexOut = isIndex ? indexOut : 1;

            // find
            var elements = session.ApplicationRoot.FindAll(TreeScope.TreeScope_Descendants, condition);
            var automationElement = elements.Length > 0 ? elements.GetElement(indexOut - 1) : default;

            // not found
            if (automationElement == default)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // OK
            var node = DomFactory.Create(automationElement, TreeScope.TreeScope_Children);
            var xmlElement = XElement.Parse($"{node.Document.XPathSelectElement("(//root/*)[1]")}");
            var id = xmlElement.Attribute("id").Value;
            var location = new Location
            {
                Left = int.Parse(xmlElement.Attribute("left").Value),
                Top = int.Parse(xmlElement.Attribute("top").Value)
            };
            var element = new Element
            {
                Id = id,
                UIAutomationElement = automationElement,
                Location = location,
                Node = node
            };
            session.Elements[id] = element;
            return (StatusCodes.Status200OK, element);
        }

        private static (int Status, Element Element) GetByControlType(Session session, LocationStrategy locationStrategy)
        {
            // setup
            var controlType = GetXpathTypePattern().Match(input: locationStrategy.Value).Value;
            var condition = session.Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_LocalizedControlTypePropertyId, controlType);
            var index = GetElementIndexPattern().Match(input: locationStrategy.Value).Value;
            var isIndex = int.TryParse(index, out int indexOut);
            indexOut = isIndex ? indexOut : 1;

            // find
            var elements = session.ApplicationRoot.FindAll(TreeScope.TreeScope_Descendants, condition);
            var automationElement = elements.Length > 0 ? elements.GetElement(indexOut - 1) : default;

            // not found
            if (automationElement == default)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // OK
            var node = DomFactory.Create(automationElement, TreeScope.TreeScope_Children);
            var xmlElement = XElement.Parse($"{node.Document.XPathSelectElement("(//root/*)[1]")}");
            var id = xmlElement.Attribute("id").Value;
            var location = new Location
            {
                Left = int.Parse(xmlElement.Attribute("left").Value),
                Top = int.Parse(xmlElement.Attribute("top").Value)
            };
            var element = new Element
            {
                Id = id,
                UIAutomationElement = automationElement,
                Location = location,
                Node = node
            };
            session.Elements[id] = element;
            return (StatusCodes.Status200OK, element);
        }

        private static (int Status, Element Element) GetByCords(Session session, LocationStrategy locationStrategy)
        {
            // find
            var element = locationStrategy.GetFlatPointElement();

            // not found
            if (element == null)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // setup
            var id = $"{Guid.NewGuid()}";

            // update
            session.Elements[id] = element;

            // get
            return (StatusCodes.Status200OK, element);
        }

        private static (int Status, Element Element) GetByPath(Session session, LocationStrategy locationStrategy)
        {
            // local
            static Element GetElementByRuntime(Session session, string path) => GetByRuntime(session, new LocationStrategy
            {
                Using = LocationStrategy.Xpath,
                Value = $"/{path}"
            });

            // setup
            var hierarchy = GetHierarchyPattern()
                .Split(locationStrategy.Value)
                .Where(i => !string.IsNullOrEmpty(i))
                .ToArray();

            // bad request
            if(hierarchy.Length == 0)
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            // find first in hierarchy
            var element = GetElementByRuntime(session, path: hierarchy[0]);
            var root = hierarchy[0].Contains("root", StringComparison.OrdinalIgnoreCase)
                ? session.Automation.GetRootElement()
                : element.UIAutomationElement;

            // not found
            if(root == default)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // setup: cache DOM
            var originalDom = session.Dom;
            var origApplicationRoot = session.ApplicationRoot;
            session.Dom = DomFactory.Create(root, TreeScope.TreeScope_Children);

            // iterate
            foreach (var path in hierarchy.Skip(1))
            {
                locationStrategy.Value = $"/root/{path}";
                element = GetElementByRuntime(session, locationStrategy.Value);

                var domRuntime = session.GetRuntime(locationStrategy);
                if (string.IsNullOrEmpty(domRuntime))
                {
                    return (StatusCodes.Status404NotFound, default);
                }

                root = element.UIAutomationElement;
                if (root == null)
                {
                    return (StatusCodes.Status404NotFound, default);
                }
                var children = DomFactory
                    .Create(root, TreeScope.TreeScope_Children)
                    .Document
                    .XPathSelectElement(locationStrategy.Value)
                    .Elements();
                session.ApplicationRoot = root;
                session.Dom = XDocument.Parse($"<root>{string.Join("", children.Select(i => $"{i}"))}</root>");
            }

            // restore DOM
            session.Dom = originalDom;
            session.ApplicationRoot = origApplicationRoot;

            // get
            return (StatusCodes.Status200OK, element);
        }

        private static Element GetByRuntime(Session session, LocationStrategy locationStrategy)
        {
            // setup
            var domRuntime = session.GetRuntime(locationStrategy);

            // not found
            if(domRuntime == null)
            {
                return default;
            }

            var runtime = JsonSerializer.Deserialize<int[]>(domRuntime);
            var condition = session.Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_RuntimeIdPropertyId, runtime);

            // find
            var element = session.ApplicationRoot.FindFirst(TreeScope.TreeScope_Children, condition);

            // not found
            if(element == null)
            {
                return default;
            }

            // update
            session.Elements[domRuntime] = new()
            {
                Id = domRuntime,
                UIAutomationElement = element,
                Node = session.Dom.XPathSelectElement($"//*[@id='{domRuntime}']")
            };

            // get
            return session.Elements[domRuntime];
        }
        #endregion

        #region *** Element Data ***
        public (int StatusCode, string Text) GetElementText(string session, string element)
        {
            // setup
            var elementObject = GetElement(_sessions, session, element);

            // notFound
            if (elementObject == null)
            {
                return (StatusCodes.Status404NotFound, string.Empty);
            }

            // setup
            var automationElement = elementObject.UIAutomationElement;
            var text = automationElement.GetText();

            // get
            return (StatusCodes.Status200OK, text);
        }

        public (int StatusCode, string Value) GetElementAttribute(string session, string element, string attribute)
        {
            // setup
            var elementObject = GetElement(_sessions, session, element);

            // notFound
            if (elementObject == null)
            {
                return (StatusCodes.Status404NotFound, string.Empty);
            }

            // find attribute
            var xml = elementObject.Node;
            var isRoot = xml.Document.Root.Name.LocalName.Equals("root", StringComparison.OrdinalIgnoreCase);
            var value = isRoot
                ? XElement.Parse($"{xml.XPathSelectElement("/root/*")}")?.Attribute(attribute).Value
                : XElement.Parse($"{xml}").Attribute(attribute)?.Value;

            // get
            return string.IsNullOrEmpty(value)
                ? (StatusCodes.Status404NotFound, string.Empty)
                : (StatusCodes.Status200OK, value);
        }

        public Element GetElement(string session, string element)
        {
            return GetElement(_sessions, session, element);
        }

        private static Element GetElement(IDictionary<string, Session> sessions, string session, string element)
        {
            // notFound
            if (!sessions.ContainsKey(session))
            {
                return default;
            }
            if (sessions[session].Elements?.ContainsKey(element) != true)
            {
                return default;
            }

            // get
            return sessions[session].Elements[element];
        }
        #endregion
    }
}
