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
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement.boundingrectangleproperty?view=net-5.0
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.XPath;

using UiaDriverServer.Attributes;
using UiaDriverServer.Components;
using UiaDriverServer.Dto;

using UIAutomationClient;

using static System.Windows.Automation.AutomationElement;

namespace UiaDriverServer.Extensions
{
    internal static class AutomationExtensions
    {
        // constants
        private const int MouseEventLeftDown = 0x02;
        private const int MouseEventLeftUp = 0x04;

        // native calls
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out tagPOINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        #region *** Session     ***
        /// <summary>
        /// Gets child element from root
        /// </summary>
        /// <param name="automation">automation to get root from</param>
        /// <param name="runtime">element runtime id</param>
        /// <returns>automation element</returns>
        public static IUIAutomationElement GetApplicationRoot(this CUIAutomation8 automation, int[] runtime)
        {
            var conditions = automation.CreatePropertyCondition(UIA_PropertyIds.UIA_RuntimeIdPropertyId, runtime);
            return automation.GetRootElement().FindFirst(TreeScope.TreeScope_Children, conditions);
        }

        /// <summary>
        /// Set the mouse position cursor.
        /// </summary>
        /// <param name="automation">Main <see cref="CUIAutomation8"/> object.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <returns>Self reference.</returns>
        public static CUIAutomation8 SetCursorPosition(this CUIAutomation8 automation, int x, int y)
        {
            SetCursorPos(x, y);
            return automation;
        }

        /// <summary>
        /// Revoke the virtual DOM for the Session under automation.
        /// </summary>
        /// <param name="session">The session to revoke DOM for.</param>
        /// <remarks>Using this option will reduce the automation performance.</remarks>
        public static Session RevokeVirtualDom(this Session session)
        {
            // constants
            const string DevMode = "devMode";

            // setup conditions
            var isKey = session.Capabilities.ContainsKey(DevMode);
            var isDev = isKey && (bool)session.Capabilities[DevMode];

            // invoke
            return InvokeRefreshDom(session, isDev);
        }

        /// <summary>
        /// Revoke the virtual DOM for the Session under automation.
        /// </summary>
        /// <param name="session">The session to revoke DOM for.</param>
        /// <param name="force">Set to <see cref="true"/> in order to force a refresh not under DevMode.</param>
        /// <remarks>Using this option will reduce the automation performance.</remarks>
        public static Session RevokeVirtualDom(this Session session, bool force)
        {
            return InvokeRefreshDom(session, force);
        }

        private static Session InvokeRefreshDom(Session session, bool isDev)
        {
            // bad request
            if (!isDev)
            {
                return session;
            }

            // setup
            var dom = new DomFactory(session).Create();
            
            // not created
            if (dom == null)
            {
                return session;
            }

            // refresh DOM
            session.Dom = dom;
            return session;
        }

        /// <summary>
        /// Gets the runtime id from the virtual DOM.
        /// </summary>
        /// <param name="session">Session DOM to search.</param>
        /// <param name="locationStrategy">W3C WebDriver location strategy.</param>
        /// <returns>Serialized runtime id.</returns>
        public static string GetRuntime(this Session session, LocationStrategy locationStrategy)
        {
            var domElement = session.Dom.XPathSelectElement(locationStrategy.Value);
            return domElement?.Attribute("id").Value;
        }

        /// <summary>
        /// Gets an <see cref="IUIAutomationElement"/> by a serialized runtime id.
        /// </summary>
        /// <param name="session">Session to search.</param>
        /// <param name="domRuntime">Serialized runtime id to find by.</param>
        /// <returns>A <see cref="IUIAutomationElement"/>.</returns>
        public static IUIAutomationElement GetElementById(this Session session, string domRuntime)
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
        #endregion

        #region *** Validation  ***
        /// <summary>
        /// Assert if element in interactable and can accept a given text.
        /// </summary>
        /// <param name="element">The element to evaluate</param>
        /// <param name="text">The string to accept into the element</param>
        public static bool ApproveReadyForValue(this IUIAutomationElement element, string text)
        {
            // validate arguments & initial setup
            if (text == null)
            {
                const string message = "String parameter must not be null";
                throw new ArgumentNullException(nameof(text), message);
            }
            if (element == null)
            {
                const string message = "AutomationElement parameter must not be null";
                throw new ArgumentNullException(nameof(element), message);
            }

            // check #1: is control enabled?
            if (element.CurrentIsEnabled == 0)
            {
                throw new InvalidOperationException("The controller is not enabled");
            }

            // check #2: are there styles that prohibit us from sending text to this control?
            if (element.CurrentIsKeyboardFocusable == 0)
            {
                throw new InvalidOperationException("The controller is not focusable");
            }

            // get
            return true;
        }

        /// <summary>
        /// Assert if element is valid and can be used.
        /// </summary>
        /// <param name="element">The element to evaluate</param>
        public static bool ApproveElement(this IUIAutomationElement element)
        {
            return InvokeApproveElement(element);
        }
        #endregion

        #region *** Element     ***
        /// <summary>
        /// Try to get a mouse clickable point of the element.
        /// </summary>
        /// <param name="element">The Element to get clickable point for.</param>
        /// <returns>A ClickablePoint object.</returns>
        public static ClickablePoint GetClickablePoint(this IUIAutomationElement element)
        {
            return InvokeGetClickablePoint(element);
        }

        /// <summary>
        /// Expand or collapse the element if ExpandCollapse pattern is supported.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to expand/collapse.</param>
        public static IUIAutomationElement ExpandCollapse(this IUIAutomationElement element)
        {
            return InvokeExpanCollapse(element);
        }

        /// <summary>
        /// Trigger the <see cref="IUIAutomationInvokePattern"/> of the element.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to invoke.</param>
        /// <remarks>If the <see cref="IUIAutomationElement"/> cannot be invoked, the action is ignored.</remarks>
        public static IUIAutomationElement Invoke(this IUIAutomationElement element)
        {
            return InvokeElement(element);
        }

        /// <summary>
        /// Invokes a pattern based MouseClick action on the element. If not possible to use
        /// pattern, it will use native click. 
        /// </summary>
        /// <param name="element">The element to click on.</param>
        /// <remarks>This action wll attempt to evaluate the center of the element and click on it.</remarks>
        public static IUIAutomationElement Click(this IUIAutomationElement element)
        {
            return InvokeClick(element, isNative: false);
        }

        /// <summary>
        /// Invokes a MouseClick action on the element.
        /// </summary>
        /// <param name="element">The element to click on.</param>
        /// <param name="isNative">Set to <see cref="true"/> to invoke native click.</param>
        /// <remarks>This action wll attempt to evaluate the center of the element and click on it.</remarks>
        public static IUIAutomationElement Click(this IUIAutomationElement element, bool isNative)
        {
            return InvokeClick(element, isNative);
        }

        public static IUIAutomationElement InvokeClick(IUIAutomationElement element, bool isNative)
        {
            // native
            if (isNative)
            {
                InvokeNativeClick(element);
                return element;
            }

            // flat click pipeline
            var isFlat = element == null;
            if (isFlat)
            {
                InvokeFlatClick();
            }

            // evaluate action compliance
            InvokeApproveElement(element);

            // setup conditions
            var isInvoke = element.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId) != null;
            var isExpandCollapse = !isInvoke && element.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId) != null;
            var isSelectable = !isInvoke && !isExpandCollapse && element.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId) != null;
            var isFocus = element.CurrentIsKeyboardFocusable == 1;

            // action factory
            if ((isNative && isFocus) || (!isInvoke && !isExpandCollapse && !isSelectable && isFocus))
            {
                return InvokeNativeClick(element);
            }
            if (isInvoke)
            {
                return InvokeElement(element);
            }
            if (isExpandCollapse)
            {
                return InvokeExpanCollapse(element);
            }
            if (isSelectable)
            {
                return InvokeSelectionItem(element);
            }

            // get
            return element;
        }

        private static IUIAutomationElement InvokeNativeClick(IUIAutomationElement element)
        {
            // build
            var point = InvokeGetClickablePoint(element);

            // invoke
            SetCursorPos(point.XPos, point.YPos);
            mouse_event(MouseEventLeftDown, point.XPos, point.YPos, 0, 0);
            mouse_event(MouseEventLeftUp, point.XPos, point.YPos, 0, 0);

            // get
            return element;
        }

        private static void InvokeFlatClick()
        {
            GetCursorPos(out tagPOINT point);
            mouse_event(MouseEventLeftDown, point.x, point.y, 0, 0);
            mouse_event(MouseEventLeftDown, point.x, point.y, 0, 0);
        }

        /// <summary>
        /// Sends the given keys to the active application, and then waits for the messages
        /// to be processed.
        /// </summary>
        /// <param name="element">The element into which to send the keys.</param>
        /// <param name="text">Text (keys) to send.</param>
        public static IUIAutomationElement SendKeys(this IUIAutomationElement element, string text)
        {
            return InvokeSendKeys(element, text, isNative: false);
        }

        /// <summary>
        /// Sends the given keys to the active application, and then waits for the messages
        /// to be processed.
        /// </summary>
        /// <param name="element">The element into which to send the keys.</param>
        /// <param name="text">Text (keys) to send.</param>
        public static IUIAutomationElement SendKeys(this IUIAutomationElement element, string text, bool isNative)
        {
            return InvokeSendKeys(element, text, isNative);
        }

        private static IUIAutomationElement InvokeSendKeys(
            IUIAutomationElement element, string text, bool isNative)
        {
            // local functions
            void SetValue()
            {
                element.SetFocus();
                System.Windows.Forms.SendKeys.SendWait(text);
            }

            // get value pattern
            var pattern = element.GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId);

            // setup conditions
            var isValue = pattern != null;

            // set value
            if (!isValue || isNative)
            {
                SetValue();
            }
            if (isValue && !isNative)
            {
                try
                {
                    ((IUIAutomationValuePattern)pattern).SetValue(text);
                }
                catch (Exception e) when (e != null)
                {
                    SetValue();
                }
            }

            // get
            return element;
        }

        /// <summary>
        /// Gets the <see cref="IUIAutomationElement"/> information as a collection of key/value.
        /// </summary>
        /// <param name="info"><see cref="IUIAutomationElement"/> information.</param>
        /// <returns>A collection of key/value.</returns>
        public static IDictionary<string, string> GetAttributes(this IUIAutomationElement info)
        {
            while (true)
            {
                try
                {
                    return InvokeGetAttributes(info);
                }
                catch (COMException e) when (e != null)
                {
                    // ignore exceptions
                }
            }
        }

        private static IDictionary<string, string> InvokeGetAttributes(IUIAutomationElement info) => new Dictionary<string, string>
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
                catch (COMException e) when (e != null)
                {
                    // ignore exception
                }
            }
        }

        /// <summary>
        /// Gets the innerText of the <see cref="IUIAutomationElement"/>, without any leading or trailing whitespace,
        /// and with other whitespace collapsed.
        /// </summary>
        /// <param name="element"><see cref="IUIAutomationElement"/> to get text from.</param>
        /// <returns>The innerText of <see cref="IUIAutomationElement"/>.</returns>
        public static string GetText(this IUIAutomationElement element)
        {
            // supported text-patterns
            var textPatterns = new[]
            {
                UIA_PatternIds.UIA_TextChildPatternId,
                UIA_PatternIds.UIA_TextEditPatternId,
                UIA_PatternIds.UIA_TextPattern2Id,
                UIA_PatternIds.UIA_TextPatternId,
                UIA_PatternIds.UIA_ValuePatternId
            };

            // load methods
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var methods = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .SelectMany(t => t.GetMethods(FLAGS));

            var patterns = InvokeGetPatterns(element).Where(p => textPatterns.Contains(p));
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
            return InvokeGetPatterns(element);
        }
        #endregion

        #region *** Information ***
        /// <summary>
        /// gets the element information as key/value pair
        /// </summary>
        /// <param name="info">element information</param>
        /// <returns>element information as key/value pair</returns>
        public static IDictionary<string, string> GetAttributes(this AutomationElementInformation info)
        {
            try
            {
                return InvokeGetAttributes(info);
            }
            catch (Exception e) when (e != null)
            {
                throw;
            }
        }

        private static IDictionary<string, string> InvokeGetAttributes(AutomationElementInformation info) => new Dictionary<string, string>
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
        #endregion

        // Utilities
        private static ClickablePoint InvokeGetClickablePoint(IUIAutomationElement element)
        {
            // setup
            element.GetClickablePoint(out tagPOINT point);
            var x = point.x;
            var y = point.y;

            // ok
            if ((point.x == 0 && point.y != 0) || (point.x != 0 && point.y == 0) || (point.x != 0 && point.y != 0))
            {
                return new ClickablePoint(x, y);
            }

            // setup
            var width = Screen.PrimaryScreen.WorkingArea.Width;
            var height = Screen.PrimaryScreen.WorkingArea.Height;
            var rect = element.CurrentBoundingRectangle;

            // build
            x = (width - (rect.left + rect.right));
            y = (height - (rect.top + rect.bottom));

            // get
            return new ClickablePoint(x, y);
        }

        private static IEnumerable<int> InvokeGetPatterns(IUIAutomationElement element)
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

        private static bool InvokeApproveElement(this IUIAutomationElement element)
        {
            // assert: null
            if (element == null)
            {
                const string message = "AutomationElement parameter must not be null";
                throw new ArgumentNullException(nameof(element), message);
            }

            // get
            return true;
        }

        private static IUIAutomationElement InvokeElement(this IUIAutomationElement element)
        {
            // get current pattern
            var p = element.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId);
            var pattern = (IUIAutomationInvokePattern)p;

            // invoke
            if (pattern == null)
            {
                return element;
            }
            try
            {
                pattern.Invoke();
            }
            catch (Exception e)
            {
                throw e.GetBaseException();
            }

            // get
            return element;
        }

        private static IUIAutomationElement InvokeExpanCollapse(this IUIAutomationElement element)
        {
            // get current pattern
            var p = element.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId);
            var pattern = (IUIAutomationExpandCollapsePattern)p;

            // invoke
            if (pattern.CurrentExpandCollapseState != ExpandCollapseState.ExpandCollapseState_Collapsed)
            {
                pattern.Collapse();
                return element;
            }
            try
            {
                pattern.Expand();
            }
            catch (InvalidOperationException)
            {
                element.SetFocus();
                pattern.Expand();
            }

            // get
            return element;
        }

        private static IUIAutomationElement InvokeSelectionItem(this IUIAutomationElement element)
        {
            // get current pattern
            var p = element.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId);
            var pattern = (IUIAutomationSelectionItemPattern)p;

            // invoke
            if (pattern == null)
            {
                return element;
            }
            try
            {
                pattern.Select();
            }
            catch (Exception e)
            {
                throw e.GetBaseException();
            }

            // get
            return element;
        }
    }
}