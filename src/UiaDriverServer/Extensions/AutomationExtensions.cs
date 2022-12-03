/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better XML comments & document reference
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement.automationelementinformation?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.treescope?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.propertycondition?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement.boundingrectangleproperty?view=net-5.0
 * 
 * codemag.com
 * https://www.codemag.com/article/0810122/Creating-UI-Automation-Client-Applications
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;

using UiaDriverServer.Attributes;
using UiaDriverServer.Components;
using UiaDriverServer.Contracts;

using UIAutomationClient;

namespace UiaDriverServer.Extensions
{
    internal static partial class AutomationExtensions
    {
        // constants
        private const int MouseEventLeftDown = 0x02;
        private const int MouseEventLeftUp = 0x04;

        // native calls
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out tagPOINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetPhysicalCursorPos(int x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        // native calls: obsolete
        [Obsolete("This function has been superseded. Use SendInput instead.")]
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        #region *** Session      ***
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
        /// Gets root element from file explorer instance.
        /// </summary>
        /// <param name="session">The session to use.</param>
        /// <returns>An <see cref="IUIAutomationElement"/> interface.</returns>
        public static IUIAutomationElement GetApplicationRoot(this Session session)
        {
            return session.Application.GetNameOrFile().Contains("EXPLORER.EXE", StringComparison.OrdinalIgnoreCase)
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

            while(DateTime.Now < timeout)
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
            IUIAutomationElement rootElement = null;
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

        /// <summary>
        /// Gets the native status indicator.
        /// </summary>
        /// <returns><see cref="true"/> if native; <see cref="false"/> if not.</returns>
        public static bool GetIsNative(this Session session)
        {
            // members
            const string Key = UiaCapability.UseNativeEvents;
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // setup
            var capabilites = session.Capabilities;
            var isNative = capabilites.ContainsKey(Key);

            // get
            return isNative && $"{capabilites[Key]}".Equals("true", Compare);
        }

        /// <summary>
        /// Gets the file explorer indicator.
        /// </summary>
        /// <returns><see cref="true"/> if FileExplorer; <see cref="false"/> if not.</returns>
        public static bool GetIsFileExplorer(this Session session)
        {
            try
            {
                return session
                    .Application?
                    .StartInfo?
                    .FileName?
                    .Equals("explorer.exe", StringComparison.OrdinalIgnoreCase) != false;
            }
            catch (Exception e) when (e != null)
            {
                return false;
            }
        }

        /// <summary>
        /// Synthesizes keystrokes, mouse motions, and button clicks.
        /// </summary>
        /// <param name="inputs">A collection of input objects.</param>
        /// <returns>The number of events that it successfully inserted into the keyboard or mouse input stream.</returns>
        /// <remarks>If the function returns zero, the input was already blocked by another thread.</remarks>
        public static (uint NumberOfEvents, int ErrorCode) SendInput(this CUIAutomation8 _, params Input[] inputs)
        {
            // invoke
            var numberOfevents = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
            var errorCode = Marshal.GetLastWin32Error();

            // get
            return (numberOfevents, errorCode);
        }

        /// <summary>
        /// Send a key stroke using a a modifier (e.g. alt, shift, CTRL).
        /// </summary>
        /// <param name="modifier">The modifier (e.g. alt, shift, CTRL).</param>
        /// <param name="key">The key.</param>
        public static void SendModifiedKey(this CUIAutomation8 _, string modifier, string key)
        {
            // locals
            ushort GetKeyCode(string key)
            {
                // setup
                var isCode = GetScanCodeMap().Any(i => i.Value.Equals(key, StringComparison.OrdinalIgnoreCase));

                // get
                return isCode
                    ? GetScanCodeMap().First(i => i.Value.Equals(key, StringComparison.OrdinalIgnoreCase)).Key
                    : (ushort)0x00;
            }

            // build
            var modifierCode = GetKeyCode(modifier);
            var keyCode = GetKeyCode(key);
            var inputs = Modify(modifierCode, keyCode).ToArray();

            // invoke
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
            Marshal.GetLastWin32Error();
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
            SetPhysicalCursorPos(x, y);
            return automation;
        }

        /// <summary>
        /// Delete a Session, disposing all the resources bound to ti.
        /// </summary>
        /// <param name="session">The session to delete.</param>
        public static void Delete(this Session session)
        {
            // file explorer child session
            if (session.GetIsFileExplorer())
            {
                var appRoot = session.GetApplicationRoot();
                var pattern = appRoot.GetCurrentPattern(UIA_PatternIds.UIA_WindowPatternId);
                (pattern != null ? (IUIAutomationWindowPattern)pattern : null)?.Close();
            }

            // dispose
            session?.Application?.Kill();
            session?.Application?.Dispose();
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
            var isDev = isKey && ((JsonElement)session.Capabilities[DevMode]).GetBoolean();

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
        /// <param name="runtime">Serialized runtime id to find by.</param>
        /// <returns>A <see cref="IUIAutomationElement"/>.</returns>
        public static IUIAutomationElement GetElementById(this Session session, string runtime)
        {
            // get container
            var containerElement = session.GetApplicationRoot();

            // create finding condition
            var domRuntime = Utilities.GetRuntime(runtime).ToArray();
            var c = session.Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_RuntimeIdPropertyId, domRuntime);

            // get element
            return containerElement.FindFirst(TreeScope.TreeScope_Descendants, c);
        }

        // TODO: build cache
        /// <summary>
        /// Gets an <see cref="IUIAutomationElement"/> from root scope (desktop).
        /// </summary>
        /// <param name="session">Session to search.</param>
        /// <param name="locationStrategy"></param>
        /// <returns>A <see cref="IUIAutomationElement"/>.</returns>
        public static (IUIAutomationElement, XNode, string) GetFromRoot(this Session session, LocationStrategy locationStrategy)
        {
            // bad request
            if (!locationStrategy.Value.StartsWith("//root"))
            {
                return (null, null, null);
            }
            locationStrategy.Value = locationStrategy.Value.Replace("//root", string.Empty);

            // build
            var dom = new DomFactory(session).Create(session.Automation.GetRootElement());
            var domElement = dom.XPathSelectElement(locationStrategy.Value);
            var domRuntime = domElement?.Attribute("id").Value;

            // get container
            var containerElement = session.Automation.GetRootElement();

            // create finding condition
            var runtime = Utilities.GetRuntime(domRuntime).ToArray();
            var c = session.Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_RuntimeIdPropertyId, runtime);

            // get element
            return (containerElement.FindFirst(TreeScope.TreeScope_Descendants, c), domElement, domRuntime);
        }
        #endregion

        #region *** Validation   ***
        /// <summary>
        /// Assert if element in interact-able and can accept a given text.
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

        #region *** Element      ***
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

            // evaluate action compliance
            InvokeApproveElement(element);

            // setup conditions
            var isInvoke = element?.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId) != null;
            var isExpandCollapse = !isInvoke && element.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId) != null;
            var isSelectable = !isInvoke && !isExpandCollapse && element.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId) != null;
            var isFocus = element.CurrentIsKeyboardFocusable == 1;

            // action factory
            if (isFocus || (!isInvoke && !isExpandCollapse && !isSelectable && isFocus))
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
            SetPhysicalCursorPos(point.XPos, point.YPos);
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
        /// Select the element if possible.
        /// </summary>
        /// <param name="element">The element to select on.</param>
        public static IUIAutomationElement Select(this IUIAutomationElement element)
        {
            return InvokeSelectionItem(element);
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
            // TODO: implement unmanaged!
            // local functions
            void SetValue()
            {
                element.SetFocus();
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
        /// Generates XML tag-name for this automation element
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
        [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "As design. This method have access to internal resource.")]
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

        /// <summary>
        /// Gets a flat (x, y) clickable point (//cords[x, y]) wrapped in an Element.
        /// </summary>
        /// <param name="locationStrategy">LocationStrategy object to get cords from.</param>
        /// <returns>An Element object with a flat clickable point.</returns>
        public static Element GetFlatPointElement(this LocationStrategy locationStrategy)
        {
            const string P1 = @"(?i)//cords\[\d+,\d+]";
            const string P2 = @"\[\d+,\d+]";

            // setup conditions
            var isCords = Regex.IsMatch(locationStrategy.Value, P1);
            if (!isCords)
            {
                return null;
            }

            // load cords
            var cords = JsonSerializer.Deserialize<int[]>(Regex.Match(locationStrategy.Value, P2).Value);
            return new Element { ClickablePoint = new ClickablePoint(xpos: cords[0], ypos: cords[1]) };
        }
        #endregion

        #region *** Information  ***
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
            ["AcceleratorKey".ToCamelCase()] = info.CurrentAcceleratorKey,
            ["AccessKey".ToCamelCase()] = info.CurrentAccessKey,
            ["AriaProperties".ToCamelCase()] = info.CurrentAriaProperties,
            ["AriaRole".ToCamelCase()] = info.CurrentAriaRole,
            ["AutomationId".ToCamelCase()] = info.CurrentAutomationId,
            ["Bottom".ToCamelCase()] = $"{info.CurrentBoundingRectangle.bottom}",
            ["Left".ToCamelCase()] = $"{info.CurrentBoundingRectangle.left}",
            ["Right".ToCamelCase()] = $"{info.CurrentBoundingRectangle.right}",
            ["Top".ToCamelCase()] = $"{info.CurrentBoundingRectangle.top}",
            ["ClassName".ToCamelCase()] = info.CurrentClassName,
            ["FrameworkId".ToCamelCase()] = info.CurrentFrameworkId,
            ["HelpText".ToCamelCase()] = info.CurrentHelpText.ParseForXml(),
            ["IsContentElement".ToCamelCase()] = info.CurrentIsContentElement == 1 ? "true" : "false",
            ["IsControlElement".ToCamelCase()] = info.CurrentIsControlElement == 1 ? "true" : "false",
            ["IsEnabled".ToCamelCase()] = info.CurrentIsEnabled == 1 ? "true" : "false",
            ["IsKeyboardFocusable".ToCamelCase()] = info.CurrentIsKeyboardFocusable == 1 ? "true" : "false",
            ["IsPassword".ToCamelCase()] = info.CurrentIsPassword == 1 ? "true" : "false",
            ["IsRequiredForForm".ToCamelCase()] = info.CurrentIsRequiredForForm == 1 ? "true" : "false",
            ["ItemStatus".ToCamelCase()] = info.CurrentItemStatus,
            ["ItemType".ToCamelCase()] = info.CurrentItemType,
            ["Name".ToCamelCase()] = info.CurrentName.ParseForXml(),
            ["NativeWindowHandle".ToCamelCase()] = $"{info.CurrentNativeWindowHandle}",
            ["Orientation".ToCamelCase()] = $"{info.CurrentOrientation}",
            ["ProcessId".ToCamelCase()] = $"{info.CurrentProcessId}"
        };

        /// <summary>
        /// Gets a keyboard input object.
        /// </summary>
        /// <param name="wScan">The key scan code.</param>
        /// <param name="flags">A collection of flags to use.</param>
        /// <returns>A keyboard input object.</returns>
        public static Input GetKeyboardInput(ushort wScan, KeyEventF flags) => InvokeGetKeyboardInput(wScan, flags);

        /// <summary>
        /// Gets a collection of KeyboradInput based on an input string.
        /// </summary>
        /// <param name="input">The intput string.</param>
        /// <returns>A collection of KeyboradInput.</returns>
        public static IEnumerable<Input> GetInputs(this string input)
        {
            // setup
            var map = GetScanCodeMap();
            var inputs = new List<Input>();

            // build: inputs
            foreach (var item in input)
            {
                var (modified, modifier, keyCode) = GetModifiedInforamtion($"{item}");
                if (modified)
                {
                    inputs.AddRange(Modify(modifier, keyCode));
                    continue;
                }

                var wScan = map.Any(i => i.Value.Equals($"{item}", StringComparison.OrdinalIgnoreCase))
                    ? map.First(i => i.Value.Equals($"{item}", StringComparison.OrdinalIgnoreCase)).Key
                    : (ushort)0x00;
                inputs.AddRange(new[]
                {
                    InvokeGetKeyboardInput(wScan, KeyEventF.KeyDown | KeyEventF.Scancode),
                    InvokeGetKeyboardInput(wScan, KeyEventF.KeyUp | KeyEventF.Scancode)
                });
            }

            // get
            return inputs;
        }
        #endregion

        #region *** Capabilities ***
        /// <summary>
        /// Assert if the capabilities are compliant with UiA Driver.
        /// </summary>
        /// <param name="capabilities">The capabilites to assert.</param>
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
        #endregion

        // Utilities
        private static ClickablePoint InvokeGetClickablePoint(IUIAutomationElement element)
        {
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
            //input.mouseInput.dwFlags = NativeEnums.MouseEventFlags.Absolute | NativeEnums.MouseEventFlags.LeftUp | NativeEnums.MouseEventFlags.Move;
            //NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));

            //var left = element.CurrentBoundingRectangle.left;
            //var right = element.CurrentBoundingRectangle.right;
            //var top = element.CurrentBoundingRectangle.top;
            //var bottom = element.CurrentBoundingRectangle.bottom;
            //x = (left + right) / 2;
            //y = (top + bottom) / 2;

            // get
            return new ClickablePoint(input.mouseInput.dx, input.mouseInput.dy);
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

        private static Input InvokeGetKeyboardInput(ushort wScan, KeyEventF flags) => new()
        {
            type = (int)InputType.Keyboard,
            u = new InputUnion
            {
                ki = new KeyboardInput
                {
                    wVk = 0,
                    wScan = wScan,
                    dwFlags = (uint)(flags),
                    dwExtraInfo = GetMessageExtraInfo()
                }
            }
        };

        private static (bool Modified, ushort Modifier, ushort ModifiedKeyCode) GetModifiedInforamtion(string input)
        {
            // setup
            var info = new List<(string Input, ushort Modifier, ushort ModifiedKeyCode)>();
            info.AddRange(new (string Input, ushort Modifier, ushort ModifiedKeyCode)[]
            {
                (Input: ":", Modifier: 0x2A, ModifiedKeyCode: 0x27),
                (Input: "@", Modifier: 0x2A, ModifiedKeyCode: 0x03)
            });

            // build
            var isModified = info.Any(i => i.Input.Equals(input));
            var modifier = isModified ? info.First(i => i.Input.Equals(input)).Modifier : (ushort)0x00;
            var modifiedKeyCode = isModified ? info.First(i => i.Input.Equals(input)).ModifiedKeyCode : (ushort)0x00;

            // get
            return (isModified, modifier, modifiedKeyCode);
        }

        private static IEnumerable<Input> Modify(ushort modifierCode, ushort keyCode)
        {
            return new[]
            {
                InvokeGetKeyboardInput(modifierCode, KeyEventF.KeyDown | KeyEventF.Scancode),
                InvokeGetKeyboardInput(keyCode, KeyEventF.KeyDown | KeyEventF.Scancode),
                InvokeGetKeyboardInput(keyCode, KeyEventF.KeyUp | KeyEventF.Scancode),
                InvokeGetKeyboardInput(modifierCode, KeyEventF.KeyUp | KeyEventF.Scancode),
            };
        }

        private static IDictionary<ushort, string> GetScanCodeMap() => new Dictionary<ushort, string>
        {
            [0x01] = "Esc",
            [0x02] = "1",
            [0x03] = "2",
            [0x04] = "3",
            [0x05] = "4",
            [0x06] = "5",
            [0x07] = "6",
            [0x08] = "7",
            [0x09] = "8",
            [0x0A] = "9",
            [0x0B] = "0",
            [0x0C] = "-",
            [0x0D] = "=",
            [0x0E] = "Backspace",
            [0x0F] = "Tab",
            [0x10] = "Q",
            [0x11] = "W",
            [0x12] = "E",
            [0x13] = "R",
            [0x14] = "T",
            [0x15] = "Y",
            [0x16] = "U",
            [0x17] = "I",
            [0x18] = "O",
            [0x19] = "P",
            [0x1A] = "[",
            [0x1B] = "]",
            [0x1C] = "Enter",
            [0x1D] = "Ctrl",
            [0x1E] = "A",
            [0x1F] = "S",
            [0x20] = "D",
            [0x21] = "F",
            [0x22] = "G",
            [0x23] = "H",
            [0x24] = "J",
            [0x25] = "K",
            [0x26] = "L",
            [0x27] = ";",
            [0x28] = "'",
            [0x29] = "`",
            [0x2A] = "LShift",
            [0x2B] = @"\",
            [0x2C] = "Z",
            [0x2D] = "X",
            [0x2E] = "C",
            [0x2F] = "V",
            [0x30] = "B",
            [0x31] = "N",
            [0x32] = "M",
            [0x33] = ",",
            [0x34] = ".",
            [0x35] = "/",
            [0x36] = "RShift",
            [0x37] = "PrtSc",
            [0x38] = "Alt",
            [0x39] = " ",
            [0x3A] = "CapsLock",
            [0x3B] = "F1",
            [0x3C] = "F2",
            [0x3D] = "F3",
            [0x3E] = "F4",
            [0x3F] = "F5",
            [0x40] = "F6",
            [0x41] = "F7",
            [0x42] = "F8",
            [0x43] = "F9",
            [0x44] = "F10",
            [0x45] = "Num",
            [0x46] = "Scroll",
            [0x47] = "Home",
            [0x48] = "Up",
            [0x49] = "PgUp",
            [0x4A] = "-",
            [0x4B] = "Left",
            [0x4C] = "Center",
            [0x4D] = "Right",
            [0x4E] = "+",
            [0x4F] = "End",
            [0x50] = "Down",
            [0x51] = "PgDn",
            [0x52] = "Ins",
            [0x53] = "Del"
        };

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

        private static partial class NativeMethods
        {
            [LibraryImport("user32.dll", SetLastError = true)]
            internal static partial uint SendInput(uint nInputs, ref NativeStructs.Input pInputs, int cbSize);
        }
    }
}
