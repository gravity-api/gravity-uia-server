/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.XPath;

using UIAutomationClient;

using UiaWebDriverServer.Contracts;

namespace UiaWebDriverServer.Extensions
{
    public static partial class AutomationExtensions
    {
        // constants
        private const int MouseEventLeftDown = 0x02;
        private const int MouseEventLeftUp = 0x04;

        #region *** Expressions         ***
        [GeneratedRegex("(?<=UIA_).*(?=ControlTypeId)")]
        private static partial Regex GetControlTypePattern();

        [GeneratedRegex("(?i)//cords\\[\\d+,\\d+]", RegexOptions.None, "en-US")]
        private static partial Regex GetCordsPattern();

        [GeneratedRegex("\\[\\d+,\\d+]")]
        private static partial Regex GetCordsNumberPattern();
        #endregion

        #region *** Externals           ***
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out tagPOINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetPhysicalCursorPos(int x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        // native calls: obsolete
        [Obsolete("This function has been superseded. Use SendInput instead.")]
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        #endregion

        #region *** Screen Size         ***
        public static Size GetScreenResultion(this CUIAutomation8 _)
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();

            int physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.Desktopvertres);
            int physicalScreenWidth = GetDeviceCaps(desktop, (int)DeviceCap.Desktophorzres);

            return new Size(physicalScreenWidth, physicalScreenHeight);
        }
        #endregion

        #region *** Application Root    ***
        /// <summary>
        /// Gets root element from file explorer instance.
        /// </summary>
        /// <param name="session">The session to use.</param>
        /// <returns>An <see cref="IUIAutomationElement"/> interface.</returns>
        public static IUIAutomationElement GetApplicationRoot(this Session session)
        {
            return GetRoot(session);
        }
        #endregion

        #region *** Element: Tag Name   ***
        /// <summary>
        /// Generates XML tag-name for this automation element
        /// </summary>
        /// <param name="element">element to generate tag-name for</param>
        /// <returns>element tag-name</returns>
        public static string GetTagName(this IUIAutomationElement element)
        {
            return GetTagName(timeout: TimeSpan.FromSeconds(5), element);
        }

        /// <summary>
        /// Generates XML tag-name for this automation element
        /// </summary>
        /// <param name="element">element to generate tag-name for</param>
        /// <returns>element tag-name</returns>
        public static string GetTagName(this IUIAutomationElement element, TimeSpan timeout)
        {
            return GetTagName(timeout, element);
        }

        private static string GetTagName(TimeSpan timeout, IUIAutomationElement element)
        {
            // setup
            var expires = DateTime.Now.Add(timeout);

            // iterate
            while (DateTime.Now<expires)
            {
                try
                {
                    var controlType = typeof(UIA_ControlTypeIds).GetFields()
                        .Where(f => f.FieldType == typeof(int))
                        .FirstOrDefault(f => (int)f.GetValue(null) == element.CurrentControlType)?.Name;

                    return GetControlTypePattern().Match(controlType).Value.ToCamelCase();
                }
                catch (COMException e) when (e != null)
                {
                    // ignore exception
                }
            }

            // get
            return string.Empty;
        }
        #endregion

        #region *** Element: Attributes ***
        /// <summary>
        /// Gets the <see cref="IUIAutomationElement"/> information as a collection of key/value.
        /// </summary>
        /// <param name="info"><see cref="IUIAutomationElement"/> information.</param>
        /// <returns>A collection of key/value.</returns>
        public static IDictionary<string, string> GetAttributes(this IUIAutomationElement info)
        {
            return GetAttributes(timeout: TimeSpan.FromSeconds(5), info);
        }

        /// <summary>
        /// Gets the <see cref="IUIAutomationElement"/> information as a collection of key/value.
        /// </summary>
        /// <param name="info"><see cref="IUIAutomationElement"/> information.</param>
        /// <returns>A collection of key/value.</returns>
        public static IDictionary<string, string> GetAttributes(this IUIAutomationElement info, TimeSpan timeout)
        {
            return GetAttributes(timeout, info);
        }

        private static IDictionary<string, string> GetAttributes(TimeSpan timeout, IUIAutomationElement info)
        {
            // local
            static IDictionary<string, string> Get(IUIAutomationElement info) => new Dictionary<string, string>
            {
                ["AcceleratorKey".ToCamelCase()] = info.CurrentAcceleratorKey.ParseForXml(),
                ["AccessKey".ToCamelCase()] = info.CurrentAccessKey.ParseForXml(),
                ["AriaProperties".ToCamelCase()] = info.CurrentAriaProperties.ParseForXml(),
                ["AriaRole".ToCamelCase()] = info.CurrentAriaRole.ParseForXml(),
                ["AutomationId".ToCamelCase()] = info.CurrentAutomationId.ParseForXml(),
                ["Bottom".ToCamelCase()] = $"{info.CurrentBoundingRectangle.bottom}",
                ["Left".ToCamelCase()] = $"{info.CurrentBoundingRectangle.left}",
                ["Right".ToCamelCase()] = $"{info.CurrentBoundingRectangle.right}",
                ["Top".ToCamelCase()] = $"{info.CurrentBoundingRectangle.top}",
                ["ClassName".ToCamelCase()] = info.CurrentClassName.ParseForXml(),
                ["FrameworkId".ToCamelCase()] = info.CurrentFrameworkId.ParseForXml(),
                ["HelpText".ToCamelCase()] = info.CurrentHelpText.ParseForXml(),
                ["IsContentElement".ToCamelCase()] = info.CurrentIsContentElement == 1 ? "true" : "false",
                ["IsControlElement".ToCamelCase()] = info.CurrentIsControlElement == 1 ? "true" : "false",
                ["IsEnabled".ToCamelCase()] = info.CurrentIsEnabled == 1 ? "true" : "false",
                ["IsKeyboardFocusable".ToCamelCase()] = info.CurrentIsKeyboardFocusable == 1 ? "true" : "false",
                ["IsPassword".ToCamelCase()] = info.CurrentIsPassword == 1 ? "true" : "false",
                ["IsRequiredForForm".ToCamelCase()] = info.CurrentIsRequiredForForm == 1 ? "true" : "false",
                ["ItemStatus".ToCamelCase()] = info.CurrentItemStatus.ParseForXml(),
                ["ItemType".ToCamelCase()] = info.CurrentItemType.ParseForXml(),
                ["Name".ToCamelCase()] = info.CurrentName.ParseForXml(),
                ["NativeWindowHandle".ToCamelCase()] = $"{info.CurrentNativeWindowHandle}",
                ["Orientation".ToCamelCase()] = $"{info.CurrentOrientation}",
                ["ProcessId".ToCamelCase()] = $"{info.CurrentProcessId}"
            };

            // setup
            var expire = DateTime.Now.Add(timeout);

            // get
            while (DateTime.Now < expire)
            {
                try
                {
                    return Get(info);
                }
                catch (COMException e) when (e != null)
                {
                    // ignore exceptions
                }
            }
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region *** Element: Text       ***
        public static string GetText(this IUIAutomationElement element)
        {
            // setup
            var textPatterns = new[]
            {
                UIA_PatternIds.UIA_TextChildPatternId,
                UIA_PatternIds.UIA_TextEditPatternId,
                UIA_PatternIds.UIA_TextPattern2Id,
                UIA_PatternIds.UIA_TextPatternId,
                UIA_PatternIds.UIA_ValuePatternId
            };
            var patterns = GetElementPatterns(element).Where(i => textPatterns.Contains(i));

            // not supported
            if (!patterns.Any())
            {
                return string.Empty;
            }

            // setup
            var id = patterns.First();
            var pattern = element.GetCurrentPattern(id);

            // get
            return new TextPatternFactory().GetText(id, pattern);
        }
        #endregion

        #region *** Element: Send Keys  ***
        /// <summary>
        /// Sends the given keys to the active application, and then waits for the messages
        /// to be processed.
        /// </summary>
        /// <param name="element">The element into which to send the keys.</param>
        /// <param name="text">Text (keys) to send.</param>
        public static IUIAutomationElement SendKeys(this IUIAutomationElement element, string text)
        {
            return SendKeys(text, isNative: false, element);
        }

        /// <summary>
        /// Sends the given keys to the active application, and then waits for the messages
        /// to be processed.
        /// </summary>
        /// <param name="element">The element into which to send the keys.</param>
        /// <param name="text">Text (keys) to send.</param>
        public static IUIAutomationElement SendKeys(this IUIAutomationElement element, string text, bool isNative)
        {
            return SendKeys(text, isNative, element);
        }

        // TODO: implement unmanaged!
        private static IUIAutomationElement SendKeys(string text, bool isNative, IUIAutomationElement element)
        {
            // local functions
            void SetValue()
            {
                element.SetFocus();
            }

            // get value pattern
            var pattern = element.GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId);

            // setup conditions
            var isValue = pattern != null;
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
        #endregion

        #region *** Element: Invoke     ***
        /// <summary>
        /// Try to get a mouse click-able point of the element.
        /// </summary>
        /// <param name="element">The Element to get click-able point for.</param>
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
        /// <remarks>This action will attempt to evaluate the center of the element and click on it.</remarks>
        public static IUIAutomationElement Click(this IUIAutomationElement element)
        {
            return InvokeClick(element, isNative: false);
        }

        /// <summary>
        /// Invokes a MouseClick action on the element.
        /// </summary>
        /// <param name="element">The element to click on.</param>
        /// <param name="isNative">Set to <see cref="true"/> to invoke native click.</param>
        /// <remarks>This action will attempt to evaluate the center of the element and click on it.</remarks>
        public static IUIAutomationElement Click(this IUIAutomationElement element, bool isNative)
        {
            return InvokeClick(element, isNative);
        }

        private static IUIAutomationElement InvokeClick(IUIAutomationElement element, bool isNative)
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

            // setup conditions
            var isInvoke = element?.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId) != null;
            var isExpandCollapse = !isInvoke && element.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId) != null;
            var isSelectable = !isInvoke && !isExpandCollapse && element.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId) != null;

            // invoke
            if (isInvoke)
            {
                InvokeElement(element);
            }
            else if (isExpandCollapse)
            {
                InvokeExpanCollapse(element);
            }
            else if (isSelectable)
            {
                InvokeSelectionItem(element);
            }

            // get
            return element;
        }

        private static void InvokeNativeClick(IUIAutomationElement element)
        {
            // build
            var point = InvokeGetClickablePoint(element);

            // invoke
            SetPhysicalCursorPos(point.XPos, point.YPos);
            mouse_event(MouseEventLeftDown, point.XPos, point.YPos, 0, 0);
            mouse_event(MouseEventLeftUp, point.XPos, point.YPos, 0, 0);
        }

        private static void InvokeFlatClick()
        {
            GetCursorPos(out tagPOINT point);
            mouse_event(MouseEventLeftDown, point.x, point.y, 0, 0);
            mouse_event(MouseEventLeftDown, point.x, point.y, 0, 0);
        }
        #endregion

        #region *** Session: Revoke XML ***
        /// <summary>
        /// Revoke the virtual DOM for the Session under automation.
        /// </summary>
        /// <param name="session">The session to revoke DOM for.</param>
        /// <remarks>Using this option will reduce the automation performance.</remarks>
        public static Session RevokeXml(this Session session)
        {
            // constants
            const string DevMode = "devMode";

            // setup conditions
            var isKey = session.Capabilities.ContainsKey(DevMode);
            var isDev = isKey && ((JsonElement)session.Capabilities[DevMode]).GetBoolean();

            // invoke
            return RevokeXml(isDev, session);
        }

        /// <summary>
        /// Revoke the virtual DOM for the Session under automation.
        /// </summary>
        /// <param name="session">The session to revoke DOM for.</param>
        /// <param name="force">Set to <see cref="true"/> in order to force a refresh not under DevMode.</param>
        /// <remarks>Using this option will reduce the automation performance.</remarks>
        public static Session RevokeXml(this Session session, bool force)
        {
            return RevokeXml(force, session);
        }

        private static Session RevokeXml(bool isDev, Session session)
        {
            // bad request
            if (!isDev)
            {
                return session;
            }

            // setup
            Thread.Sleep(1000);
            var dom = DomFactory.Create(session.ApplicationRoot, TreeScope.TreeScope_Descendants);

            // not created
            if (dom == null)
            {
                return session;
            }

            // refresh DOM
            session.Dom = dom;
            return session;
        }
        #endregion

        /// <summary>
        /// Gets an element by id.
        /// </summary>
        /// <param name="session">The session to get the element from.</param>
        /// <param name="id">The element id.</param>
        /// <returns>Element information object.</returns>
        public static Element GetElement(this Session session, string id)
        {
            if (!session.Elements.ContainsKey(id))
            {
                return null;
            }
            return session.Elements[id];
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
        /// <param name="runtime">Serialized runtime id to find by.</param>
        /// <returns>A <see cref="IUIAutomationElement"/>.</returns>
        public static IUIAutomationElement GetElementByRuntime(this Session session, string runtime)
        {
            // get container
            var containerElement = GetRoot(session);

            // create finding condition
            var domRuntime = JsonSerializer.Deserialize<IEnumerable<int>>(runtime);
            var c = session.Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_RuntimeIdPropertyId, domRuntime);

            // get element
            return containerElement.FindFirst(TreeScope.TreeScope_Descendants, c);
        }

        /// <summary>
        /// Gets a flat (x, y) click-able point (//cords[x, y]) wrapped in an Element.
        /// </summary>
        /// <param name="locationStrategy">LocationStrategy object to get cords from.</param>
        /// <returns>An Element object with a flat click-able point.</returns>
        public static Element GetFlatPointElement(this LocationStrategy locationStrategy)
        {
            // setup conditions
            var isCords = GetCordsPattern().IsMatch(locationStrategy.Value);

            // not found
            if (!isCords)
            {
                return null;
            }

            // load cords
            var cords = JsonSerializer.Deserialize<int[]>(GetCordsNumberPattern().Match(locationStrategy.Value).Value);
            return new Element { ClickablePoint = new ClickablePoint(xpos: cords[0], ypos: cords[1]) };
        }

        /// <summary>
        /// Assert if the capabilities are compliant with UiA Driver.
        /// </summary>
        /// <param name="capabilities">The capabilities to assert.</param>
        /// <returns><see cref="true"/> if compliant; <see cref="false"/> if not.</returns>
        public static (IActionResult Response, bool Result) AssertCapabilities(this Capabilities capabilities)
        {
            // setup
            const string message = "You must provide [{0}] capability";
            var response = new ContentResult
            {
                ContentType = MediaTypeNames.Text.Plain,
                StatusCode = StatusCodes.Status500InternalServerError
            };

            // shortcuts
            var c = capabilities.DesiredCapabilities;

            // evaluate
            if (!c.ContainsKey(UiaCapability.Application))
            {
                response.Content = string.Format(message, UiaCapability.Application);
                return (response, false);
            }

            // setup
            response.StatusCode = StatusCodes.Status200OK;
            response.Content = string.Empty;
            response.ContentType = MediaTypeNames.Application.Json;

            // get
            return (response, true);
        }

        public static TreeScope ConvertToTreeScope(this string treeScope) => treeScope.ToUpper() switch
        {
            "NONE" => TreeScope.TreeScope_None,
            "ANCESTORS" => TreeScope.TreeScope_Ancestors,
            "CHILDREN" => TreeScope.TreeScope_Children,
            "DESCENDANTS" => TreeScope.TreeScope_Descendants,
            "ELEMENT" => TreeScope.TreeScope_Element,
            "PARENT" => TreeScope.TreeScope_Parent,
            "SUBTREE" => TreeScope.TreeScope_Subtree,
            _ => TreeScope.TreeScope_Descendants
        };

        /// <summary>
        /// Assert if element is interact-able and can accept a given text.
        /// </summary>
        /// <param name="element">The element to evaluate</param>
        /// <param name="text">The string to accept into the element</param>
        /// <returns><see cref="true"/> if the element can have a value; <see cref="false"/> if not.</returns>
        public static bool AssertCanHaveValue(this IUIAutomationElement element, string text)
        {
            // validate arguments & initial setup
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            // check #1: is control enabled?
            if (element.CurrentIsEnabled == 0)
            {
                return false;
            }

            // get
            return true;
        }

        // Utilities
        private static IUIAutomationElement GetRoot(Session session)
        {
            // constants
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;
            const string Explorer = "EXPLORER.EXE";

            // get
            return session.Application.GetNameOrFile().Contains(Explorer, Compare)
                ? GetRootFromFileExplorer(session)
                : GetRootFromApplication(session);
        }

        private static IUIAutomationElement GetRootFromFileExplorer(Session session)
        {
            // bad request
            if (!Directory.Exists(session.Application.StartInfo.Arguments))
            {
                return null;
            }

            // iteration
            IUIAutomationElement rootElement = null;
            var timeout = DateTime.Now.Add(session.Timeout);

            while (DateTime.Now < timeout)
            {
                rootElement = GetFromFileExplorer(session);

                if (rootElement != null)
                {
                    session.Runtime = rootElement.GetRuntimeId().Cast<int>().ToArray();
                    return rootElement;
                }

                Thread.Sleep(100);
            }

            // get
            return rootElement;
        }

        private static IUIAutomationElement GetFromFileExplorer(Session session)
        {
            // constants
            const int NameProperty = UIA_PropertyIds.UIA_NamePropertyId;
            const int ControlTypeProperty = UIA_PropertyIds.UIA_ControlTypePropertyId;
            const int ControlTypeWindow = UIA_ControlTypeIds.UIA_WindowControlTypeId;
            const int ControlTypeToolBar = UIA_ControlTypeIds.UIA_ToolBarControlTypeId;

            // setup
            var folder = session.Application.StartInfo.Arguments;
            var folderLast = new DirectoryInfo(folder).Name;

            // setup: windows - chain condition
            var windowCondition = session.Automation.CreatePropertyCondition(ControlTypeProperty, ControlTypeWindow);
            var windowPartialNameCondition = session.Automation.CreatePropertyCondition(NameProperty, folderLast);
            var windowFullNameCondition = session.Automation.CreatePropertyCondition(NameProperty, folder);
            var windowNameCondition = session.Automation.CreateOrCondition(windowFullNameCondition, windowPartialNameCondition);
            var windowRootCondition = session.Automation.CreateAndCondition(windowCondition, windowNameCondition);

            // setup: tool-bar - chain condition
            var toolBarCondition = session.Automation.CreatePropertyCondition(ControlTypeProperty, ControlTypeToolBar);

            // collect: windows
            var window = session.Automation.GetRootElement().FindFirst(TreeScope.TreeScope_Descendants, windowRootCondition);

            // find the first explorer window based on the application
            var toolBars = window.FindAll(TreeScope.TreeScope_Descendants, toolBarCondition);
            var names = new List<string>();

            for (int toolBar = 0; toolBar < toolBars.Length; toolBar++)
            {
                names.Add($"{toolBars.GetElement(toolBar)?.GetCurrentPropertyValue(NameProperty)}");
            }

            var isWindow = names.Any(i => i.Contains(folder, StringComparison.OrdinalIgnoreCase));

            // not found
            return isWindow ? window : null;
        }

        private static IUIAutomationElement GetRootFromApplication(Session session)
        {
            // iteration
            IUIAutomationElement rootElement;
            var timeout = DateTime.Now.Add(session.Timeout);

            while (DateTime.Now < timeout)
            {
                var condition = GetCondition(session);
                var root = session.Automation.GetRootElement();
                rootElement = root.FindFirst(TreeScope.TreeScope_Descendants, condition);

                if (rootElement != null)
                {
                    session.Runtime = rootElement.GetRuntimeId().Cast<int>().ToArray();
                    return rootElement;
                }

                Thread.Sleep(100);
            }

            // get
            return null;
        }

        private static IUIAutomationCondition GetCondition(Session session)
        {
            // setup conditions
            var isRuntime = session.Runtime?.Any() == true;
            var isHandle = session.Application.MainWindowHandle != default;

            // setup
            var id = isRuntime
                ? UIA_PropertyIds.UIA_RuntimeIdPropertyId
                : UIA_PropertyIds.UIA_NativeWindowHandlePropertyId;
            if (!isRuntime && !isHandle)
            {
                id = UIA_PropertyIds.UIA_ProcessIdPropertyId;
            }

            // get condition
            if (isRuntime)
            {
                return session.Automation.CreatePropertyCondition(id, session.Runtime.ToArray());
            }
            return isHandle
                ? session.Automation.CreatePropertyCondition(id, session.Application.MainWindowHandle)
                : session.Automation.CreatePropertyCondition(id, session.Application.Id);
        }

        private static IEnumerable<int> GetElementPatterns(IUIAutomationElement element)
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

        private static ClickablePoint InvokeGetClickablePoint(IUIAutomationElement element)
        {
            static Size GetScreenResultion()
            {
                var graphics = Graphics.FromHwnd(IntPtr.Zero);
                IntPtr desktop = graphics.GetHdc();

                int physicalScreenHeight = GetDeviceCaps(desktop, 117);
                int physicalScreenWidth = GetDeviceCaps(desktop, 118);

                return new Size(physicalScreenWidth, physicalScreenHeight);
            }

            // setup
            element.GetClickablePoint(out tagPOINT point);
            var x = point.x;
            var y = point.y;

            // OK
            if ((point.x == 0 && point.y != 0) || (point.x != 0 && point.y == 0) || (point.x != 0 && point.y != 0))
            {
                return new ClickablePoint(x, y);
            }

            // setup
            var p = element.CurrentBoundingRectangle;
            var input = new NativeStructs.Input
            {
                type = NativeEnums.SendInputEventType.Mouse,
                mouseInput = new NativeStructs.MouseInput
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = NativeEnums.MouseEvent.Absolute | NativeEnums.MouseEvent.RightDown | NativeEnums.MouseEvent.Move,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero,
                },
            };

            var primaryScreen = Screen.PrimaryScreen;
            input.mouseInput.dx = Convert.ToInt32((p.left + 1 - primaryScreen.Bounds.Left) * 65536 / primaryScreen.Bounds.Width);
            input.mouseInput.dy = Convert.ToInt32((p.top + 1 - primaryScreen.Bounds.Top) * 65536 / primaryScreen.Bounds.Height);

            // get
            return new ClickablePoint(input.mouseInput.dx, input.mouseInput.dy);
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
            var p = element?.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId);

            // invoke
            if (p is not IUIAutomationSelectionItemPattern pattern)
            {
                return element;
            }
            try
            {
                element.SetFocus();
                pattern.Select();
            }
            catch (Exception e)
            {
                throw e.GetBaseException();
            }

            // get
            return element;
        }










        private static class NativeStructs
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Input
            {
                public NativeEnums.SendInputEventType type;
                public MouseInput mouseInput;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MouseInput
            {
                public int dx;
                public int dy;
                public uint mouseData;
                public NativeEnums.MouseEvent dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }
        }

        private static class NativeEnums
        {
            internal enum SendInputEventType
            {
                Mouse = 0,
                Keyboard = 1,
                Hardware = 2,
            }

            [Flags]
            internal enum MouseEvent : uint
            {
                None = 0x0000,
                Move = 0x0001,
                LeftDown = 0x0002,
                LeftUp = 0x0004,
                RightDown = 0x0008,
                RightUp = 0x0010,
                MiddleDown = 0x0020,
                MiddleUp = 0x0040,
                XDown = 0x0080,
                XUp = 0x0100,
                Wheel = 0x0800,
                Absolute = 0x8000,
            }
        }
    }
}
