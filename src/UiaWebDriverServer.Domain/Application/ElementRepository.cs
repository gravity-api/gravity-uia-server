/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

using UIAutomationClient;

using UiaWebDriverServer.Contracts;
using UiaWebDriverServer.Extensions;

namespace UiaWebDriverServer.Domain.Application
{
    public partial class ElementRepository : IElementRepository
    {
        #region *** Expressions  ***
        [GeneratedRegex(@"(?<=\/(\/)?)\w+(?=\)?\[)")]
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

        // TODO: refactor to support XPATH pipe
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

            // get by Cords
            var elementByCords = GetByCords(session, locationStrategy);
            if (elementByCords.Status == StatusCodes.Status200OK)
            {
                return elementByCords;
            }

            // get by path
            return GetByProperty(session, locationStrategy);
        }

        private static (int Status, Element Element) GetByProperty(Session session, LocationStrategy locationStrategy)
        {
            // constants
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // setup conditions
            var isRoot = locationStrategy.Value.StartsWith("/root", Compare);
            var xpath = isRoot
                ? locationStrategy.Value.Replace("/root", string.Empty, Compare)
                : locationStrategy.Value;
            
            // setup
            var hierarchy = GetHierarchyPattern()
                .Split(xpath)
                .Where(i => !string.IsNullOrEmpty(i))
                .ToArray();

            // bad request
            if (hierarchy.Length == 0)
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            // find element
            var rootElement = isRoot ? new CUIAutomation8().GetRootElement() : session.ApplicationRoot;
            var automationElement = GetElementBySegment(new CUIAutomation8(), rootElement, hierarchy[0]);

            // not found
            if(automationElement == default)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // iterate
            foreach (var pathSegment in hierarchy.Skip(1))
            {
                automationElement = GetElementBySegment(new CUIAutomation8(), automationElement, pathSegment);
                if(automationElement == default)
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

            if(controlTypeCondition == default && propertyCondition != default)
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
            return elements.Length == 0 ? default : elements.GetElement(indexOut - 1 < 0 ? 0 : indexOut - 1);
        }

        private static IUIAutomationCondition GetControlTypeCondition(CUIAutomation8 session, string pathSegment)
        {
            // constants
            const PropertyConditionFlags ConditionFlags = PropertyConditionFlags.PropertyConditionFlags_IgnoreCase;
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
            var typeSegment = GetTypeSegmentPattern().Match(pathSegment).Value;

            // setup
            var conditionFlag = typeSegment.StartsWith("partial", Compare)
                ? ConditionFlags | PropertyConditionFlags.PropertyConditionFlags_MatchSubstring
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
            const PropertyConditionFlags ConditionFlags = PropertyConditionFlags.PropertyConditionFlags_IgnoreCase;
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
            var typeSegment = GetPropertyTypeSegmentPattern().Match(pathSegment).Value;

            // setup
            var conditionFlag = typeSegment.StartsWith("partial", Compare)
                ? ConditionFlags | PropertyConditionFlags.PropertyConditionFlags_MatchSubstring
                : ConditionFlags;
            typeSegment = typeSegment.Replace("partial", string.Empty, Compare);
            var valueSegment = GetPropertyValueSegmentPattern().Match(pathSegment).Value;
            var propertyId = GetPropertyId(typeSegment);

            // not found
            if(propertyId == -1)
            {
                return default;
            }

            // get
            return session.CreatePropertyConditionEx(propertyId, valueSegment, conditionFlag);
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
