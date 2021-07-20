/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments & document reference
 * 
 * 2019-02-09
 *    - modify: change to automation COM instead of System.Windows.Automation
 *    - modify: remove DOM refreshing after action
 *
 * 2019-02-10
 *    - modify: better set-value fall-back
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.valuepattern.valuepatterninformation?view=netframework-4.7.2
 */
using Newtonsoft.Json.Linq;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Web.Http;
using System.Windows.Forms;

using UiaDriverServer.Dto;
using UiaDriverServer.Extensions;
using UIAutomationClient;

namespace UiaDriverServer.Controllers
{
    public class InteractionController : Api
    {
        // native calls
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        // constants
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        // POST wd/hub/session/[s]/element/[e]/value
        // POST session/[s]/element/[e]/value
        [Route("wd/hub/session/{s}/element/{e}/value")]
        [Route("session/{s}/element/{e}/value")]
        [HttpPost]
        public IHttpActionResult Value(string s, string e, [FromBody] object dto)
        {
            // load action information
            var session = GetSession(s);
            var element = GetElement(session, e).UIAutomationElement;
            var text = $"{((JToken)dto)["text"]}";
            var isNative = IsNativeEvents(session);

            // evaluate action compliance
            Evaluate(element, text);

            // get value pattern
            var pattern = element.GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId);

            // setup conditions
            var isValue = pattern != null;

            // set value
            if (!isValue || isNative)
            {
                Simulate(element, text);
            }
            if (isValue && !isNative)
            {
                try
                {
                    ((IUIAutomationValuePattern)pattern).SetValue(text);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning(ex.Message);
                    Simulate(element, text);
                }
            }
            DevMode(session);
            return Ok();
        }

        // POST wd/hub/session/[s]/element/[e]/value
        // POST session/[s]/element/[e]/value
        [Route("wd/hub/session/{s}/element/{e}/click")]
        [Route("session/{s}/element/{e}/click")]
        [HttpPost]
        public IHttpActionResult Click(string s, string e)
        {
            // load action information
            var session = GetSession(s);
            var element = GetElement(session, e);

            // native
            if (IsNativeEvents(session))
            {
                NativeClick(element);
                DevMode(session);
                return Ok();
            }

            // flat click pipeline
            var isClick = element.ClickablePoint != null;
            var isNotUiElement = element.UIAutomationElement == null;
            var isNotXnode = element.Node == null;

            if (isClick && isNotUiElement && isNotXnode)
            {
                Simulate(element.ClickablePoint.XPos, element.ClickablePoint.YPos);
                return Ok();
            }

            // evaluate action compliance
            Evaluate(element.UIAutomationElement);

            // setup conditions
            var isInvoke = element.UIAutomationElement.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId) != null;
            var isExpandCollapse = !isInvoke && element.UIAutomationElement.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId) != null;
            var isSelectable = !isInvoke && !isExpandCollapse && element.UIAutomationElement.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId) != null;
            var isFocus = element.UIAutomationElement.CurrentIsKeyboardFocusable == 1;
            var isNative = IsNativeEvents(session);

            // native
            if (isNative && isFocus)
            {
                NativeClick(element);
                DevMode(session);
                return Ok();
            }

            // action factory
            if (isInvoke)
            {
                var p = (IUIAutomationInvokePattern)element
                    .UIAutomationElement.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId);
                p.Invoke();
            }
            if (isExpandCollapse)
            {
                ExpandCollapse(element.UIAutomationElement);
            }
            if (isSelectable)
            {
                var p = (IUIAutomationSelectionItemPattern)element
                    .UIAutomationElement.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId);
                p.Select();
            }
            if (!isInvoke && !isExpandCollapse && !isSelectable && isFocus)
            {
                NativeClick(element);
            }
            DevMode(session);
            return Ok();
        }

        private void Evaluate(IUIAutomationElement element, string text)
        {
            // validate arguments & initial setup
            if (text == null)
            {
                const string message = "string parameter must not be null";
                throw new ArgumentNullException(nameof(text), message);
            }
            if (element == null)
            {
                const string message = "automation-element parameter must not be null";
                throw new ArgumentNullException(nameof(element), message);
            }

            // check #1: is control enabled?
            if (element.CurrentIsEnabled == 0)
            {
                throw new InvalidOperationException("the control is not enabled");
            }

            // check #2: are there styles that prohibit us from sending text to this control?
            if (element.CurrentIsKeyboardFocusable == 0)
            {
                throw new InvalidOperationException("the control is not focusable");
            }
        }

        private void Evaluate(IUIAutomationElement element)
        {
            if (element == null)
            {
                const string message = "automation-element parameter must not be null";
                throw new ArgumentNullException(nameof(element), message);
            }
        }

        private void Simulate(IUIAutomationElement element, string text)
        {
            element.SetFocus();
            SendKeys.SendWait(text);
        }

        private void ExpandCollapse(IUIAutomationElement element)
        {
            // get current pattern
            var p = element.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId);
            var pattern = (IUIAutomationExpandCollapsePattern)p;

            // toggle
            if (pattern.CurrentExpandCollapseState != ExpandCollapseState.ExpandCollapseState_Collapsed)
            {
                pattern.Collapse();
                return;
            }
            try
            {
                pattern.Expand();
            }
            catch (InvalidOperationException ex)
            {
                Trace.TraceWarning(ex.Message);
                element.SetFocus();
                pattern.Expand();
            }
        }

        private void DevMode(Session session)
        {
            // constants
            const string DEV_MODE = "devMode";

            // setup conditions
            var isKey = session.Capabilities.ContainsKey(DEV_MODE);
            var isDev = isKey && (bool)session.Capabilities[DEV_MODE];
            if (!isDev)
            {
                return;
            }
            session.RefreshDom();
        }

        private bool IsNativeEvents(Session session)
        {
            // members
            var key = UiaCapability.UseNativeEvents;
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // setup
            var capabilites = session.Capabilities;
            var isNative = capabilites.ContainsKey(key);

            // get
            return isNative && $"{capabilites[key]}".Equals("true", Compare);
        }

        // initiate native click
        private void NativeClick(Element element)
        {
            // build
            var point = element.GetClickablePoint();

            // get
            Simulate(point.XPos, point.YPos);
        }

        private void Simulate(int xpos, int ypos)
        {
            SetCursorPos(xpos, xpos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, xpos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, xpos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, xpos, 0, 0);
        }
    }
}