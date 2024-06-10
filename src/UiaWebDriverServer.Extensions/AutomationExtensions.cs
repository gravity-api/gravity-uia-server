/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
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
using System.Xml.XPath;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using UIAutomationClient;

using UiaWebDriverServer.Contracts;

using static UiaWebDriverServer.Contracts.NativeEnums;
using static UiaWebDriverServer.Contracts.NativeStructs;

namespace UiaWebDriverServer.Extensions
{
    public static partial class AutomationExtensions
    {
        

        #region *** Expressions         ***
        [GeneratedRegex("(?<=UIA_).*(?=ControlTypeId)")]
        private static partial Regex GetControlTypePattern();

        #endregion

        #region *** Screen Size         ***
        /// <summary>
        /// Gets the primary screen resultion.
        /// </summary>
        /// <param name="_">Automation session.</param>
        /// <returns>A rectangular region with an ordered pair of width and height.</returns>
        public static Size GetScreenResultion(this CUIAutomation8 _)
        {
            // setup
            var graphics = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = graphics.GetHdc();

            // build
            int physicalScreenHeight = ExternalMethods.GetDeviceCaps(desktop, (int)DeviceCap.Desktopvertres);
            int physicalScreenWidth = ExternalMethods.GetDeviceCaps(desktop, (int)DeviceCap.Desktophorzres);

            // get
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

        #region *** Element: Rectangle  ***
        // TODO: add support for element
        // TODO: add support for window
        public static RectangleModel GetRectangle(this IUIAutomationElement element)
        {
            // setup
            var pattern = element.GetCurrentPattern(UIA_PatternIds.UIA_WindowPatternId) as IUIAutomationWindowPattern;
            var isWindow = pattern != null;

            // TODO: add support for element
            // bad request
            if (!isWindow)
            {
                return new RectangleModel();
            }

            // get
            return new RectangleModel();
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
            while (DateTime.Now < expires)
            {
                try
                {
                    var controlType = typeof(UIA_ControlTypeIds).GetFields()
                        .Where(f => f.FieldType == typeof(int))
                        .FirstOrDefault(f => (int)f.GetValue(null) == element.CurrentControlType)?.Name;

                    return GetControlTypePattern().Match(controlType).Value;
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
                ["AcceleratorKey"] = info.CurrentAcceleratorKey.ParseForXml(),
                ["AccessKey"] = info.CurrentAccessKey.ParseForXml(),
                ["AriaProperties"] = info.CurrentAriaProperties.ParseForXml(),
                ["AriaRole"] = info.CurrentAriaRole.ParseForXml(),
                ["AutomationId"] = info.CurrentAutomationId.ParseForXml(),
                ["Bottom"] = $"{info.CurrentBoundingRectangle.bottom}",
                ["Left"] = $"{info.CurrentBoundingRectangle.left}",
                ["Right"] = $"{info.CurrentBoundingRectangle.right}",
                ["Top"] = $"{info.CurrentBoundingRectangle.top}",
                ["ClassName"] = info.CurrentClassName.ParseForXml(),
                ["FrameworkId"] = info.CurrentFrameworkId.ParseForXml(),
                ["HelpText"] = info.CurrentHelpText.ParseForXml(),
                ["IsContentElement"] = info.CurrentIsContentElement == 1 ? "true" : "false",
                ["IsControlElement"] = info.CurrentIsControlElement == 1 ? "true" : "false",
                ["IsEnabled"] = info.CurrentIsEnabled == 1 ? "true" : "false",
                ["IsKeyboardFocusable"] = info.CurrentIsKeyboardFocusable == 1 ? "true" : "false",
                ["IsPassword"] = info.CurrentIsPassword == 1 ? "true" : "false",
                ["IsRequiredForForm"] = info.CurrentIsRequiredForForm == 1 ? "true" : "false",
                ["ItemStatus"] = info.CurrentItemStatus.ParseForXml(),
                ["ItemType"] = info.CurrentItemType.ParseForXml(),
                ["Name"] = info.CurrentName.ParseForXml(),
                ["NativeWindowHandle"] = $"{info.CurrentNativeWindowHandle}",
                ["Orientation"] = $"{info.CurrentOrientation}",
                ["ProcessId"] = $"{info.CurrentProcessId}"
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
        /// <summary>
        /// Gets the inner text of the element.
        /// </summary>
        /// <param name="element">The element to get text from.</param>
        /// <returns>The inner text of the element.</returns>
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
            var patterns = GetPatterns(element).Where(i => textPatterns.Contains(i));

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

        public static void NativeClick(this CUIAutomation8 automation, int x, int y)
        {
            ExternalMethods.SetPhysicalCursorPos(x, y);
            InvokeNativeClick();
        }

        public static void NativeClick(this CUIAutomation8 automation, int x, int y, int repeat)
        {
            ExternalMethods.SetPhysicalCursorPos(x, y);
            ExternalMethods.GetPhysicalCursorPos(out tagPOINT position);

            for (int i = 0; i < repeat; i++)
            {
                ExternalMethods.mouse_event(ExternalMethods.MouseEventLeftDown, position.x, position.y, 0, 0);
                ExternalMethods.mouse_event(ExternalMethods.MouseEventLeftUp, position.x, position.y, 0, 0);
            }
        }

        /// <summary>
        /// Invokes a pattern based MouseClick action on the element. If not possible to use
        /// pattern, it will use native click.
        /// </summary>
        /// <param name="element">The element to click on.</param>
        /// <remarks>This action will attempt to evaluate the center of the element and click on it.</remarks>
        public static IUIAutomationElement Click(this IUIAutomationElement element)
        {
            return Click(scaleRatio: 1.0D, element);
        }

        /// <summary>
        /// Invokes a pattern based MouseClick action on the element. If not possible to use
        /// pattern, it will use native click.
        /// </summary>
        /// <param name="element">The element to click on.</param>
        /// <remarks>This action will attempt to evaluate the center of the element and click on it.</remarks>
        public static IUIAutomationElement Click(this IUIAutomationElement element, double scaleRatio)
        {
            return Click(scaleRatio, element);
        }

        private static IUIAutomationElement Click(double scaleRatio, IUIAutomationElement uiElement)
        {
            // setup conditions
            var pattern = GetElementPattern(uiElement);

            // perform based on pattern type
            switch (pattern)
            {
                case IUIAutomationInvokePattern:
                    uiElement.Invoke();
                    break;

                case IUIAutomationExpandCollapsePattern:
                    uiElement.ExpandCollapse();
                    break;

                case IUIAutomationSelectionItemPattern:
                    uiElement.Select();
                    break;

                default:
                    var point = InvokeGetClickablePoint(scaleRatio, uiElement);
                    ExternalMethods.SetPhysicalCursorPos(point.XPos, point.YPos);
                    InvokeNativeClick();
                    break;
            }


            // get
            return uiElement;
        }

        private static object GetElementPattern(IUIAutomationElement uiElement)
        {
            if (uiElement is null)
            {
                return null;
            }
            var patterns = new[]
            {
                UIA_PatternIds.UIA_InvokePatternId,
                UIA_PatternIds.UIA_ExpandCollapsePatternId,
                UIA_PatternIds.UIA_SelectionItemPatternId
            };
            object patternObject = null;
            foreach (var pattern in patterns)
            {
                patternObject = uiElement.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId);
                if (patternObject != null)
                {
                    break;
                }
            }
            return patternObject;
        }
        #endregion

        #region *** Element: Select     ***
        /// <summary>
        /// Select the element if possible.
        /// </summary>
        /// <param name="element">The element to select on.</param>
        public static IUIAutomationElement Select(this IUIAutomationElement element)
        {
            return SelectElement(element);
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
            var dom = DomFactory.New(session.ApplicationRoot);

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

        #region *** Session: Inputs     ***
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
            ExternalMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
            Marshal.GetLastWin32Error();
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
            var numberOfEvents = ExternalMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
            var errorCode = Marshal.GetLastWin32Error();

            // get
            return (numberOfEvents, errorCode);
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
            ExternalMethods.SetPhysicalCursorPos(x, y);
            return automation;
        }

        /// <summary>
        /// Gets a collection of KeyboardInput based on an input string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A collection of KeyboardInput.</returns>
        public static IEnumerable<Input> GetInputs(this string input)
        {
            // setup
            var map = GetScanCodeMap();

            // build: buttons
            var isButton = map.Any(i => i.Value.Equals(input, StringComparison.OrdinalIgnoreCase));

            if (isButton)
            {
                var wScan = map.First(i => i.Value.Equals(input, StringComparison.OrdinalIgnoreCase)).Key;
                return new[]
                {
                    InvokeGetKeyboardInput(wScan, KeyEvent.KeyDown | KeyEvent.Scancode),
                    InvokeGetKeyboardInput(wScan, KeyEvent.KeyUp | KeyEvent.Scancode)
                };
            }

            // build: inputs
            var inputs = new List<Input>();
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
                    InvokeGetKeyboardInput(wScan, KeyEvent.KeyDown | KeyEvent.Scancode),
                    InvokeGetKeyboardInput(wScan, KeyEvent.KeyUp | KeyEvent.Scancode)
                });
            }

            // get
            return inputs;
        }

        private static (bool Modified, ushort Modifier, ushort ModifiedKeyCode) GetModifiedInforamtion(string input)
        {
            // setup
            var info = new List<(string Input, ushort Modifier, ushort ModifiedKeyCode)>();
            info.AddRange(new (string Input, ushort Modifier, ushort ModifiedKeyCode)[]
            {
                (Input: ":", Modifier: 0x2A, ModifiedKeyCode: 0x27),
                (Input: "@", Modifier: 0x2A, ModifiedKeyCode: 0x03),
                (Input: "_", Modifier: 0x2A, ModifiedKeyCode: 0x0C)
            });

            // build
            var isModified = info.Any(i => i.Input.Equals(input));
            var modifier = isModified ? info.First(i => i.Input.Equals(input)).Modifier : (ushort)0x00;
            var modifiedKeyCode = isModified ? info.First(i => i.Input.Equals(input)).ModifiedKeyCode : (ushort)0x00;

            // get
            return (isModified, modifier, modifiedKeyCode);
        }
        #endregion

        public static ushort GetScanCode(this string key)
        {
            // get
            var codes = GetScanCodeMap().Where(i => i.Value.Equals(key, StringComparison.OrdinalIgnoreCase));

            // get
            return codes.Any() ? codes.First().Key : ushort.MaxValue;
        }

        public static Element ConvertToElement(this IUIAutomationElement automationElement)
        {
            // setup
            var automationId = automationElement.CurrentAutomationId;
            var id = string.IsNullOrEmpty(automationId)
                ? $"{Guid.NewGuid()}"
                : automationElement.CurrentAutomationId;
            
            var location = new Location
            {
                Bottom = automationElement.CurrentBoundingRectangle.bottom,
                Left = automationElement.CurrentBoundingRectangle.left,
                Right = automationElement.CurrentBoundingRectangle.right,
                Top = automationElement.CurrentBoundingRectangle.top
            };

            // build
            var element = new Element
            {
                Id = id,
                UIAutomationElement = automationElement,
                Location = location
            };

            // get
            return element;
        }

        /// <summary>
        /// Gets an element by id.
        /// </summary>
        /// <param name="session">The session to get the element from.</param>
        /// <param name="id">The element id.</param>
        /// <returns>Element information object.</returns>
        public static Element GetElement(this Session session, string id)
        {
            if (!session.Elements.TryGetValue(id, out Element value))
            {
                return null;
            }
            return value;
        }

        /// <summary>
        /// Gets the runtime id from the virtual DOM.
        /// </summary>
        /// <param name="session">Session DOM to search.</param>
        /// <param name="locationStrategy">W3C WebDriver location strategy.</param>
        /// <returns>Serialized runtime id.</returns>
        public static string GetRuntime(this Session session, LocationStrategy locationStrategy)
        {
            // refresh children
            session.Dom = DomFactory.New(session.ApplicationRoot);

            // find
            var domElement = session.Dom.XPathSelectElement(locationStrategy.Value);
            
            // get
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

        private static IEnumerable<int> GetPatterns(IUIAutomationElement element)
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

        private static IUIAutomationElement InvokeExpanCollapse(this IUIAutomationElement element)
        {
            // get current pattern
            var currentPattern = element.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId);
            var pattern = (IUIAutomationExpandCollapsePattern)currentPattern;

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

        private static IUIAutomationElement InvokeElement(this IUIAutomationElement element)
        {
            // get current pattern
            var currentPattern = element.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId);
            var pattern = (IUIAutomationInvokePattern)currentPattern;

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

        private static ClickablePoint InvokeGetClickablePoint(double scaleRatio, IUIAutomationElement element)
        {
            scaleRatio = scaleRatio <= 0 ? 1 : scaleRatio;

            // range
            var hDelta = (element.CurrentBoundingRectangle.right - element.CurrentBoundingRectangle.left) / 2;
            var vDelta = (element.CurrentBoundingRectangle.bottom - element.CurrentBoundingRectangle.top) / 2;

            // setup
            var x = (int)((element.CurrentBoundingRectangle.left + hDelta) / scaleRatio);
            var y = (int)((element.CurrentBoundingRectangle.top + vDelta) / scaleRatio);

            // get
            return new(xpos: x, ypos: y);
        }


        private static IUIAutomationElement SelectElement(this IUIAutomationElement element)
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

        private static void InvokeNativeClick()
        {
            // get current mouse position
            ExternalMethods.GetPhysicalCursorPos(out tagPOINT position);

            // invoke click sequence
            ExternalMethods.mouse_event(ExternalMethods.MouseEventLeftDown, position.x, position.y, 0, 0);
            ExternalMethods.mouse_event(ExternalMethods.MouseEventLeftUp, position.x, position.y, 0, 0);
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

        private static IEnumerable<Input> Modify(ushort modifierCode, ushort keyCode) => new[]
        {
            InvokeGetKeyboardInput(modifierCode, KeyEvent.KeyDown | KeyEvent.Scancode),
            InvokeGetKeyboardInput(keyCode, KeyEvent.KeyDown | KeyEvent.Scancode),
            InvokeGetKeyboardInput(keyCode, KeyEvent.KeyUp | KeyEvent.Scancode),
            InvokeGetKeyboardInput(modifierCode, KeyEvent.KeyUp | KeyEvent.Scancode),
        };

        private static Input InvokeGetKeyboardInput(ushort wScan, KeyEvent flags) => new()
        {
            type = (int)SendInputEventType.Keyboard,
            union = new InputUnion
            {
                ki = new KeyInput
                {
                    wVk = 0,
                    wScan = wScan,
                    dwFlags = (uint)(flags),
                    dwExtraInfo = ExternalMethods.GetMessageExtraInfo()
                }
            }
        };
    }
}
