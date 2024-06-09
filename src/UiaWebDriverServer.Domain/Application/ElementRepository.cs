/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

using Microsoft.AspNetCore.Http;

using UIAutomationClient;

using UiaWebDriverServer.Contracts;
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

        [GeneratedRegex(@"(?<![\/\.])\(?\/")]
        private static partial Regex GetHierarchyPattern();

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
            return FindElement(locationStrategy, session);
        }

        public (int Status, Element Element) FindElement(string session, string element, LocationStrategy locationStrategy)
        {
            throw new NotImplementedException();
        }

        // Linear Search
        private (int Status, Element Element) FindElement(LocationStrategy locationStrategy, string session)
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
            if (segments.Length == 0)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // dom
            if (locationStrategy.Value.Contains("/DOM/", StringComparison.OrdinalIgnoreCase))
            {
                return FindElement(s, locationStrategy);
            }

            foreach (var segment in segments)
            {
                var (statusCode, element) = FindElement(session: s, string.Empty, locationStrategy: new()
                {
                    Using = locationStrategy.Using,
                    Value = segment
                });

                if (statusCode == StatusCodes.Status200OK)
                {
                    return (statusCode, element);
                }
            }

            // get
            return (StatusCodes.Status404NotFound, default);
        }

        // Binary Search
        private (int Status, Element Element) FindElement(Session session, LocationStrategy locationStrategy)
        {
            // setup
            var (isRoot, hierarchy) = GetLocatorHierarchy(locationStrategy);

            // bad request
            if (!hierarchy.Any())
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            // setup
            var segments = hierarchy
                .Where(i => !string.IsNullOrEmpty(i) && !i.Equals("dom", StringComparison.OrdinalIgnoreCase))
                .Select(i => $"/{i}")
                .ToArray();

            var uiElementSegment = segments[0];
            var elementSegment = string.Join(string.Empty, segments.Skip(1));

            // TODO: allow first element to found in the dom
            // find element
            var automation = new CUIAutomation8();
            var rootElement = isRoot ? automation.GetRootElement() : session.ApplicationRoot;
            var (statusCode, element) = FindElement(new LocationStrategy
            {
                Using = "xpath",
                Value = (isRoot ? $"/root{uiElementSegment}" : uiElementSegment)
            }, session.SessionId);

            // not found
            if(statusCode == StatusCodes.Status404NotFound)
            {
                return (statusCode, element);
            }

            // only one segment
            if (segments.Length == 1)
            {
                return (StatusCodes.Status200OK, element.UIAutomationElement.ConvertToElement());
            }

            // find
            var (status, automationElement) = GetElementFromDom(element, elementSegment);

            // not found
            if(automationElement == default)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // add to session state
            session.Elements[automationElement.Id] = automationElement;

            // get
            return (status, automationElement);
        }

        private static (int Status, Element Element) FindElement(Session session, string element, LocationStrategy locationStrategy)
        {
            // not found
            if (session == null)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            switch (locationStrategy.Using)
            {
                case LocationStrategy.Xpath:
                    // get by Cords
                    var elementByCords = GetByCords(session, locationStrategy);
                    if (elementByCords.Status == StatusCodes.Status200OK)
                    {
                        return elementByCords;
                    }

                    // get by path
                    return GetByProperty(session, locationStrategy);

                case LocationStrategy.CssSelector:
                    return GetByText(session, locationStrategy);

                default:
                    return (StatusCodes.Status501NotImplemented, default);
            }

        }

        private static (int Status, Element Element) GetElementFromDom(Element rootElement, string xpath)
        {
            // find
            try
            {
                var automation = new CUIAutomation8();
                var dom = new DomFactory(rootElement.UIAutomationElement).New();
                var idAttribute = dom.XPathSelectElement(xpath)?.Attribute("id")?.Value;
                var id = JsonSerializer.Deserialize<int[]>(idAttribute);
                var condition = automation.CreatePropertyCondition(UIA_PropertyIds.UIA_RuntimeIdPropertyId, id);
                var treeScope = TreeScope.TreeScope_Descendants;

                rootElement.UIAutomationElement = rootElement.UIAutomationElement.FindFirst(treeScope, condition);

                var statusCode = rootElement.UIAutomationElement == null
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status200OK;
                rootElement = rootElement.UIAutomationElement == null
                    ? default
                    : rootElement.UIAutomationElement.ConvertToElement();

                if (rootElement == default)
                {
                    return (StatusCodes.Status404NotFound, default);
                }

                return (statusCode, rootElement);
            }
            catch (Exception e) when (e != null)
            {
                return (StatusCodes.Status404NotFound, default);
            }
        }

        private static (int Status, Element Element) GetByProperty(Session session, LocationStrategy locationStrategy)
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
            var automationElement = GetElementBySegment(new CUIAutomation8(), rootElement, hierarchy.First());

            // not found
            if (automationElement == default)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // iterate
            foreach (var pathSegment in hierarchy.Skip(1))
            {
                automationElement = GetElementBySegment(new CUIAutomation8(), automationElement, pathSegment);
                if (automationElement == default)
                {
                    return (StatusCodes.Status404NotFound, default);
                }
            }

            // OK
            var element = automationElement.ConvertToElement();
            session.Elements[element.Id] = element;

            // get
            return (StatusCodes.Status200OK, element);
        }

        [SuppressMessage("GeneratedRegex", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.", Justification = "Keep it simple.")]
        private static (bool FromDesktop, IEnumerable<string> Hierarchy) GetLocatorHierarchy(LocationStrategy locationStrategy)
        {
            // constants
            const RegexOptions RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline;

            // setup conditions
            var values = Regex.Matches(locationStrategy.Value, @"(?<==').+?(?=')").Select(i => i.Value).ToArray();
            var fromDesktop = Regex.IsMatch(locationStrategy.Value, @"^(\(+)?\/(root|dom)", RegexOptions);
            var xpath = fromDesktop
                ? Regex.Replace(locationStrategy.Value, @"^(\(+)?\/(root|dom)", string.Empty, RegexOptions)
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
                .Split(xpath, @"\/(?=\w+|\*)(?![^\[]*\])")
                .Where(i => !string.IsNullOrEmpty(i))
                .ToArray();

            // normalize
            for (int i = 0; i < hierarchy.Length; i++)
            {
                var segment = hierarchy[i];
                if (!segment.Equals("/") && !segment.EndsWith("/"))
                {
                    continue;
                }
                hierarchy[i + 1] = $"/{hierarchy[i + 1]}";
            }
            hierarchy = hierarchy
                .Where(i => !string.IsNullOrEmpty(i) && !i.Equals("/"))
                .Select(i=>i.TrimEnd('/'))
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

        private static IUIAutomationElement GetElementBySegment(
            CUIAutomation8 session,
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
                return rootElement.FindFirst(scope, condition);
            }

            // get by index
            var elements = rootElement.FindAll(scope, condition);

            // get
            return elements.Length == 0
                ? default
                : elements.GetElement(indexOut - 1 < 0 ? 0 : indexOut - 1);
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

        private static (int Status, Element Element) GetByText(Session session, LocationStrategy locationStrategy)
        {
            // find
            var element = locationStrategy.GetElementByText();

            // not found
            if (element == null)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // setup
            var id = element.Id;
            
            // update
            session.Elements[id] = element;

            // get
            return (StatusCodes.Status200OK, element);
        }

        private static IUIAutomationCondition GetControlTypeCondition(CUIAutomation8 session, string pathSegment)
        {
            // constants
            const PropertyConditionFlags ConditionFlags = PropertyConditionFlags.PropertyConditionFlags_None;
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
            var conditionFlag = typeSegment.StartsWith("partial", Compare)
                ? PropertyConditionFlags.PropertyConditionFlags_MatchSubstring
                : ConditionFlags;
            typeSegment = typeSegment.Replace("partial", string.Empty, Compare);
            var controlTypeId = GetControlTypeId(typeSegment);

            // not found
            if (string.IsNullOrEmpty(typeSegment))
            {
                return default;
            }

            // get
            return session
                .CreatePropertyConditionEx(UIA_PropertyIds.UIA_ControlTypePropertyId, controlTypeId, conditionFlag);
        }

        private static IUIAutomationCondition GetPropertyCondition(CUIAutomation8 session, string pathSegment)
        {
            // constants
            const PropertyConditionFlags ConditionFlags = PropertyConditionFlags.PropertyConditionFlags_None;
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
                var conditionFlag = typeSegment.StartsWith("partial", Compare)
                    ? PropertyConditionFlags.PropertyConditionFlags_MatchSubstring
                    : ConditionFlags;
                typeSegment = typeSegment.Replace("partial", string.Empty, Compare);
                var valueSegment = GetPropertyValueSegmentPattern().Match(segment).Value;
                var propertyId = GetPropertyId(typeSegment);

                // not found
                if (propertyId == -1)
                {
                    continue;
                }

                // get
                var condition = session.CreatePropertyConditionEx(propertyId, valueSegment, conditionFlag);

                // set
                conditions.Add(condition);
            }

            // not found
            if (conditions.Count == 0)
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
            var xml = DomFactory.New(elementObject.UIAutomationElement);
            var isRoot = xml.Document.Root.Name.LocalName.Equals("Root", StringComparison.OrdinalIgnoreCase);
            var value = isRoot
                ? XElement.Parse($"{xml.XPathSelectElement("/Root/*")}")?.Attribute(attribute)?.Value
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