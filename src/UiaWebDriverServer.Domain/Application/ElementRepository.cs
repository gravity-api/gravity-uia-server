/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

using UIAutomationClient;

using UiaWebDriverServer.Contracts;
using UiaWebDriverServer.Domain.Extensions;
using UiaWebDriverServer.Extensions;

namespace UiaWebDriverServer.Domain.Application
{
    public partial class ElementRepository : IElementRepository
    {
        #region *** Expressions  ***
        [GeneratedRegex(@"(?<=((\/)+)?)\w+(?=\)?\[)")]
        private static partial Regex GetTypeSegmentPattern();

        [GeneratedRegex(@"(?<=@)\w+")]
        private static partial Regex GetPropertyTypeSegmentPattern();

        [GeneratedRegex(@"(?<=\[@\w+=('|"")).*(?=('|"")])")]
        private static partial Regex GetPropertyValueSegmentPattern();

        [GeneratedRegex(@"(?<=\[)\d+(?=])")]
        private static partial Regex GetElementIndexPattern();
        #endregion

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
            var segments = locationStrategy.Value.Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            // bad request
            if (segments == null || segments.Length == 0)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            foreach (var segment in segments)
            {
                var (statusCode, elements) = FindElements(session: s, string.Empty, locationStrategy: new()
                {
                    Using = "xpath",
                    Value = segment
                });

                if (statusCode == StatusCodes.Status200OK && elements.Any())
                {
                    return (statusCode, elements.First());
                }
            }

            // get
            return (StatusCodes.Status404NotFound, default);
        }

        public (int Status, Element Element) FindElement(string session, string element, LocationStrategy locationStrategy)
        {
            throw new NotImplementedException();
        }

        public (int Status, IEnumerable< Element> Elements) FindElements(string session, LocationStrategy locationStrategy)
        {
            // not found
            if (!_sessions.ContainsKey(session))
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // setup
            var s = _sessions[session];
            var segments = locationStrategy.Value.Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            // bad request
            if (segments == null || segments.Length == 0)
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            foreach (var segment in segments)
            {
                var (statusCode, element) = FindElements(session: s, string.Empty, locationStrategy: new()
                {
                    Using = "xpath",
                    Value = segment
                });

                if (statusCode == StatusCodes.Status200OK)
                {
                    return (statusCode, element);
                }
            }

            // get
            return (StatusCodes.Status200OK, Array.Empty<Element>());
        }

        private static (int Status, IEnumerable<Element> Elements) FindElements(Session session, string element, LocationStrategy locationStrategy)
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

            // get by Cords
            var (status, driverElement) = GetByCords(session, locationStrategy);
            if (status == StatusCodes.Status200OK)
            {
                return (status, new[] { driverElement });
            }

            // get by path
            return GetByProperty(session, locationStrategy);
        }

        private static (int Status, IEnumerable<Element> Elements) GetByProperty(Session session, LocationStrategy locationStrategy)
        {
            // setup
            var (isRoot, hierarchy) = GetLocatorHierarchy(locationStrategy);

            // bad request
            if (!hierarchy.Any())
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            // find element
            var rootElement = isRoot ? new CUIAutomation8().GetRootElement() : session.ApplicationRoot;
            var automationElements = GetElementBySegment(new CUIAutomation8(), rootElement, hierarchy.First());

            // not found
            if (!automationElements.Any())
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // find
            foreach (var pathSegment in hierarchy.Skip(1))
            {
                var collection = new List<IUIAutomationElement>();
                foreach (var automationElement in automationElements)
                {
                    var range = GetElementBySegment(new CUIAutomation8(), automationElement, pathSegment);
                    if (range == null || !range.Any())
                    {
                        continue;
                    }
                    collection.AddRange(range);
                }
                automationElements = collection;
            }

            // not found
            if (automationElements == null || !automationElements.Any())
            {
                return (StatusCodes.Status404NotFound, Array.Empty<Element>());
            }

            // OK
            var elements = automationElements.Select(i => i.ConvertToElement()).ToArray();
            foreach (var element in elements)
            {
                session.Elements[element.Id] = element;
            }

            // get
            return (StatusCodes.Status200OK, elements);
        }

        [SuppressMessage("GeneratedRegex", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.", Justification = "Keep it simple.")]
        private static (bool FromDesktop, IEnumerable<string> Hierarchy) GetLocatorHierarchy(LocationStrategy locationStrategy)
        {
            // constants
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;
            const RegexOptions RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline;

            // setup conditions
            var values = Regex.Matches(locationStrategy.Value, @"(?<==').+?(?=')").Select(i => i.Value).ToArray();
            var fromDesktop = Regex.IsMatch(locationStrategy.Value, @"^(\(+)?\/root", RegexOptions);
            var xpath = fromDesktop
                ? locationStrategy.Value.Replace("/root", string.Empty, Compare)
                : locationStrategy.Value;

            // normalize tokens
            var tokens = new Dictionary<string, string>();
            for (int i = 0; i < values.Length; i++)
            {
                tokens[$"value_token_{i}"] = values[i];
                xpath = xpath.Replace(values[i], $"value_token_{i}");
            }

            // setup
            var hierarchy = Regex
                .Split(xpath, @"(?<![\/\.])\(?\/")
                .Where(i => !string.IsNullOrEmpty(i))
                .ToArray();

            // restore tokens
            for (int i = 0; i < hierarchy.Length; i++)
            {
                foreach (var token in tokens)
                {
                    hierarchy[i] = hierarchy[i].Replace(token.Key, token.Value);
                }
            }

            // get
            return (fromDesktop, hierarchy);
        }

        private static IEnumerable<IUIAutomationElement> GetElementBySegment(CUIAutomation8 session,
            IUIAutomationElement rootElement,
            string pathSegment)
        {
            // setup conditions
            var controlTypeCondition = GetControlTypeCondition(session, pathSegment);
            var propertyCondition = GetPropertyCondition(session, pathSegment);
            var isDescendants = pathSegment.StartsWith("/");

            // setup condition
            var scope = isDescendants ? TreeScope.TreeScope_Descendants : TreeScope.TreeScope_Children;
            IUIAutomationCondition condition;

            if (controlTypeCondition == default && propertyCondition != default)
            {
                condition = propertyCondition;
            }
            else if (controlTypeCondition != default && propertyCondition == default)
            {
                condition = controlTypeCondition;
            }
            else if (controlTypeCondition != default && propertyCondition != default)
            {
                condition = session.CreateAndCondition(controlTypeCondition, propertyCondition);
            }
            else
            {
                return default;
            }

            // setup find all
            var index = GetElementIndexPattern().Match(input: pathSegment).Value;
            var isIndex = int.TryParse(index, out int indexOut);

            // get single
            if (!isIndex)
            {
                return rootElement.FindElements(condition, scope);
            }

            // get by index
            var elements = rootElement.FindElements(condition, scope);

            // get
            return elements?.Any() == false
                ? Array.Empty<IUIAutomationElement>()
                : new[] { elements.ElementAt(indexOut - 1 < 0 ? 0 : indexOut - 1) };
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

        private static IUIAutomationCondition GetControlTypeCondition(CUIAutomation8 session, string pathSegment)
        {
            // constants
            const BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.Static;
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // local
            static int GetControlTypeId(string propertyName)
            {
                var fields = typeof(UIA_ControlTypeIds).GetFields(BindingFlags);
                var id = fields
                    .FirstOrDefault(i => i.Name.Equals($"UIA_{propertyName}ControlTypeId", Compare))?
                    .GetValue(null);
                return id == default ? -1 : (int)id;
            }

            // setup
            pathSegment = pathSegment.LastIndexOf('[') == -1 ? $"{pathSegment}[]" : pathSegment;
            var typeSegment = GetTypeSegmentPattern().Match(pathSegment).Value;

            // setup
            var isPartial = typeSegment.StartsWith("partial", Compare);
            typeSegment = typeSegment.Replace("partial", string.Empty, Compare);
            var controlTypeId = GetControlTypeId(typeSegment);

            // not found
            if (string.IsNullOrEmpty(typeSegment))
            {
                return default;
            }

            // setup
            var condition = isPartial
                ? session.CreatePropertyConditionEx(UIA_PropertyIds.UIA_ControlTypePropertyId, controlTypeId, PropertyConditionFlags.PropertyConditionFlags_MatchSubstring)
                : session.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, controlTypeId);

            // get
            return condition;
        }

        [SuppressMessage("GeneratedRegex", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.", Justification = "Keep it simple.")]
        private static IUIAutomationCondition GetPropertyCondition(CUIAutomation8 session, string pathSegment)
        {
            // constants
            const BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.Static;
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // local
            static int GetPropertyId(string propertyName)
            {
                var fields = typeof(UIA_PropertyIds).GetFields(BindingFlags);
                var id = fields
                    .FirstOrDefault(i => i.Name.Equals($"UIA_{propertyName}PropertyId", Compare))?
                    .GetValue(null);
                return id == default ? -1 : (int)id;
            }

            // setup
            // TODO: replace with fully functional logical parser.
            var conditions = new List<IUIAutomationCondition>();
            var segments = Regex.Match(pathSegment, @"(?<=\[).*(?=\])").Value.Split(" and ").Select(i => $"[{i}]");

            // build
            foreach (var segment in segments)
            {
                var typeSegment = GetPropertyTypeSegmentPattern().Match(segment).Value;

                // setup
                var isPartial = typeSegment.StartsWith("partial", Compare);
                typeSegment = typeSegment.Replace("partial", string.Empty, Compare);
                var valueSegment = GetPropertyValueSegmentPattern().Match(segment).Value;
                var propertyId = GetPropertyId(typeSegment);

                // not found
                if (propertyId == -1)
                {
                    continue;
                }

                // get
                var condition = isPartial
                    ? session.CreatePropertyConditionEx(propertyId, valueSegment, PropertyConditionFlags.PropertyConditionFlags_MatchSubstring)
                    : session.CreatePropertyCondition(propertyId, valueSegment);

                // set
                conditions.Add(condition);
            }

            // not found
            if(conditions.Count == 0)
            {
                return default;
            }

            // no logical operators
            return conditions.Count == 1
                ? conditions.First()
                : session.CreateAndConditionFromArray(conditions.ToArray());
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
            var xml = DomFactory.Create(elementObject.UIAutomationElement, TreeScope.TreeScope_Children);
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
