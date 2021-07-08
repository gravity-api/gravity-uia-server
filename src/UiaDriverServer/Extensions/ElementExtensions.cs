/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments & document reference
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement.automationelementinformation?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.treescope?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.propertycondition?view=netframework-4.7.2
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UiaDriverServer.Attributes;
using UiaDriverServer.Components;
using UIAutomationClient;
using static System.Windows.Automation.AutomationElement;

namespace UiaDriverServer.Extensions
{
    internal static class ElementExtensions
    {
        /// <summary>
        /// gets the element information as key/value pair
        /// </summary>
        /// <param name="info">element information</param>
        /// <returns>element information as key/value pair</returns>
        public static IDictionary<string, string> AsAttributes(this AutomationElementInformation info)
        {
            try
            {
                return Get(info);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
                throw;
            }
        }

        private static IDictionary<string, string> Get(AutomationElementInformation info) => new Dictionary<string, string>
        {
            [nameof(info.AcceleratorKey).ToCamelCase()] = info.AcceleratorKey,
            [nameof(info.AccessKey).ToCamelCase()] = info.AccessKey,
            [nameof(info.AutomationId).ToCamelCase()] = info.AutomationId,
            [nameof(info.BoundingRectangle.Bottom).ToCamelCase()] = $"{info.BoundingRectangle.Bottom}",
            [nameof(info.BoundingRectangle.Height).ToCamelCase()] = $"{info.BoundingRectangle.Height}",
            [nameof(info.BoundingRectangle.IsEmpty).ToCamelCase()] = $"{info.BoundingRectangle.IsEmpty}",
            [nameof(info.BoundingRectangle.Left).ToCamelCase()] = $"{info.BoundingRectangle.Left}",
            [nameof(info.BoundingRectangle.Right).ToCamelCase()] = $"{info.BoundingRectangle.Right}",
            [nameof(info.BoundingRectangle.Top).ToCamelCase()] = $"{info.BoundingRectangle.Top}",
            [nameof(info.BoundingRectangle.Width).ToCamelCase()] = $"{info.BoundingRectangle.Width}",
            [nameof(info.BoundingRectangle.X).ToCamelCase()] = $"{info.BoundingRectangle.X}",
            [nameof(info.BoundingRectangle.Y).ToCamelCase()] = $"{info.BoundingRectangle.Y}",
            [nameof(info.ClassName).ToCamelCase()] = info.ClassName,
            [nameof(info.ControlType).ToCamelCase()] = info.ControlType.LocalizedControlType,
            [nameof(info.FrameworkId).ToCamelCase()] = info.FrameworkId,
            [nameof(info.HelpText).ToCamelCase()] = info.HelpText.ParseForXml(),
            [nameof(info.IsContentElement).ToCamelCase()] = $"{info.IsContentElement}",
            [nameof(info.IsControlElement).ToCamelCase()] = $"{info.IsControlElement}",
            [nameof(info.IsEnabled).ToCamelCase()] = $"{info.IsEnabled}",
            [nameof(info.IsKeyboardFocusable).ToCamelCase()] = $"{info.IsKeyboardFocusable}",
            [nameof(info.IsPassword).ToCamelCase()] = $"{info.IsPassword}",
            [nameof(info.IsRequiredForForm).ToCamelCase()] = $"{info.IsRequiredForForm}",
            [nameof(info.ItemStatus).ToCamelCase()] = info.ItemStatus,
            [nameof(info.ItemType).ToCamelCase()] = info.ItemType,
            [nameof(info.Name).ToCamelCase()] = info.Name.ParseForXml(),
            [nameof(info.NativeWindowHandle).ToCamelCase()] = $"{info.NativeWindowHandle}",
            [nameof(info.Orientation).ToCamelCase()] = $"{info.Orientation}",
            [nameof(info.ProcessId).ToCamelCase()] = $"{info.ProcessId}"
        };

        /// <summary>
        /// gets the element information as key/value pair
        /// </summary>
        /// <param name="info">element information</param>
        /// <returns>element information as key/value pair</returns>
        public static IDictionary<string, string> AsAttributes(this IUIAutomationElement info)
        {
            while (true)
            {
                try
                {
                    return Get(info);
                }
                catch (COMException ex)
                {
                    Trace.TraceWarning(ex.Message);
                }
            }
        }

        private static IDictionary<string, string> Get(IUIAutomationElement info) => new Dictionary<string, string>
        {
            [nameof(AutomationElementInformation.AcceleratorKey).ToCamelCase()] = info.CurrentAcceleratorKey,
            [nameof(AutomationElementInformation.AccessKey).ToCamelCase()] = info.CurrentAccessKey,
            ["AriaProperties".ToCamelCase()] = info.CurrentAriaProperties,
            ["AriaRole".ToCamelCase()] = info.CurrentAriaRole,
            [nameof(AutomationElementInformation.AutomationId).ToCamelCase()] = info.CurrentAutomationId,
            [nameof(AutomationElementInformation.BoundingRectangle.Bottom).ToCamelCase()] = $"{info.CurrentBoundingRectangle.bottom}",
            [nameof(AutomationElementInformation.BoundingRectangle.Left).ToCamelCase()] = $"{info.CurrentBoundingRectangle.left}",
            [nameof(AutomationElementInformation.BoundingRectangle.Right).ToCamelCase()] = $"{info.CurrentBoundingRectangle.right}",
            [nameof(AutomationElementInformation.BoundingRectangle.Top).ToCamelCase()] = $"{info.CurrentBoundingRectangle.top}",
            [nameof(AutomationElementInformation.ClassName).ToCamelCase()] = info.CurrentClassName,
            [nameof(AutomationElementInformation.FrameworkId).ToCamelCase()] = info.CurrentFrameworkId,
            [nameof(AutomationElementInformation.HelpText).ToCamelCase()] = info.CurrentHelpText.ParseForXml(),
            [nameof(AutomationElementInformation.IsContentElement).ToCamelCase()] = info.CurrentIsContentElement == 1 ? "true" : "false",
            [nameof(AutomationElementInformation.IsControlElement).ToCamelCase()] = info.CurrentIsControlElement == 1 ? "true" : "false",
            [nameof(AutomationElementInformation.IsEnabled).ToCamelCase()] = info.CurrentIsEnabled == 1 ? "true" : "false",
            [nameof(AutomationElementInformation.IsKeyboardFocusable).ToCamelCase()] = info.CurrentIsKeyboardFocusable == 1 ? "true" : "false",
            [nameof(AutomationElementInformation.IsPassword).ToCamelCase()] = info.CurrentIsPassword == 1 ? "true" : "false",
            [nameof(AutomationElementInformation.IsRequiredForForm).ToCamelCase()] = info.CurrentIsRequiredForForm == 1 ? "true" : "false",
            [nameof(AutomationElementInformation.ItemStatus).ToCamelCase()] = info.CurrentItemStatus,
            [nameof(AutomationElementInformation.ItemType).ToCamelCase()] = info.CurrentItemType,
            [nameof(AutomationElementInformation.Name).ToCamelCase()] = info.CurrentName.ParseForXml(),
            [nameof(AutomationElementInformation.NativeWindowHandle).ToCamelCase()] = $"{info.CurrentNativeWindowHandle}",
            [nameof(AutomationElementInformation.Orientation).ToCamelCase()] = $"{info.CurrentOrientation}",
            [nameof(AutomationElementInformation.ProcessId).ToCamelCase()] = $"{info.CurrentProcessId}"
        };

        /// <summary>
        /// generates xml tag-name for this automation element
        /// </summary>
        /// <param name="element">element to generate tag-name for</param>
        /// <returns>element tag-name</returns>
        public static string GetTagName(this IUIAutomationElement element)
        {
            while (true)
            {
                try
                {
                    // get tagName
                    const string PATTERN = "(?<=UIA_).*(?=ControlTypeId)";

                    var controlType = typeof(UIA_ControlTypeIds).GetFields()
                        .Where(f => f.FieldType == typeof(int))
                        .FirstOrDefault(f => (int)f.GetValue(null) == element.CurrentControlType)?.Name;

                    return Regex.Match(controlType, PATTERN).Value.ToCamelCase();
                }
                catch (COMException ex)
                {
                    Trace.TraceWarning(ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets the innerText of this element, without any leading or trailing whitespace,
        /// and with other whitespace collapsed.
        /// </summary>
        /// <param name="element">element to get text from</param>
        /// <returns>the innerText of this element</returns>
        public static string GetText(this IUIAutomationElement element)
        {
            // supported text-patterns
            var textPatterns = new[]
            {
                UIA_PatternIds.UIA_TextChildPatternId,
                UIA_PatternIds.UIA_TextEditPatternId,
                UIA_PatternIds.UIA_TextPattern2Id,
                UIA_PatternIds.UIA_TextPatternId
            };

            // load methods
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var methods = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .SelectMany(t => t.GetMethods(FLAGS));

            var patterns = element.GetPatterns().Where(p => textPatterns.Contains(p));
            var afterCa = methods.Where(m => m.GetCustomAttribute<UiaConstantAttribute>() != null);
            var method = afterCa.FirstOrDefault(m => patterns.Contains(m.GetCustomAttribute<UiaConstantAttribute>().Constant));

            // exit conditions
            if (method == null)
            {
                return string.Empty;
            }
            var constant = method.GetCustomAttribute<UiaConstantAttribute>().Constant;
            var pattern = element.GetCurrentPattern(constant);

            // execute method
            var instance = Activator.CreateInstance(method.DeclaringType);
            return (string)method.Invoke(instance, new object[] { pattern });
        }

        /// <summary>
        /// gets all supported patterns for this element
        /// </summary>
        /// <param name="element">element to get patterns from</param>
        /// <returns>patterns id's</returns>
        public static IEnumerable<int> GetPatterns(this IUIAutomationElement element)
        {
            // patterns result
            var results = new List<int>();

            // pattern evaluation
            foreach (var pattern in typeof(UIA_PatternIds).GetFields().Where(f => f.FieldType == typeof(int)))
            {
                var id = (int)pattern.GetValue(null);
                if (element.GetCurrentPattern(id) == null)
                {
                    continue;
                }
                results.Add(id);
            }
            return results;
        }
    }
}