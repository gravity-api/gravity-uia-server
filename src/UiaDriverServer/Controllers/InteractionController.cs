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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Http;

using UiaDriverServer.Extensions;

using UIAutomationClient;

namespace UiaDriverServer.Controllers
{
    public class InteractionController : UiaController
    {
        // POST wd/hub/session/[s]/element/[e]/value
        // POST session/[s]/element/[e]/value
        [Route("wd/hub/session/{s}/element/{e}/value")]
        [Route("session/{s}/element/{e}/value")]
        [HttpPost]
        public IHttpActionResult Value(string s, string e, [FromBody] IDictionary<string, object> data)
        {
            // setup
            var session = GetSession(s);
            var element = GetElement(session, e).UIAutomationElement;
            var text = $"{data["text"]}";

            // evaluate action compliance
            element.ApproveReadyForValue(text);

            // invoke
            element.SendKeys(text, isNative: session.IsNative);

            // sync
            session.RevokeVirtualDom();
            
            // get
            return Ok();
        }

        // POST wd/hub/session/[s]/element/[e]/click
        // POST session/[s]/element/[e]/click
        [Route("wd/hub/session/{s}/element/{e}/click")]
        [Route("session/{s}/element/{e}/click")]
        [HttpPost]
        public IHttpActionResult Click(string s, string e)
        {
            // load action information
            var session = GetSession(s);
            var element = GetElement(session, e);

            // invoke
            element.UIAutomationElement.Click(session.IsNative);

            // sync
            session.RevokeVirtualDom();
            
            // get
            return Ok();
        }

        // POST wd/hub/session/[s]/element/[e]/select
        // POST session/[s]/element/[e]/select
        [Route("wd/hub/session/{s}/element/{e}/select")]
        [Route("session/{s}/element/{e}/select")]
        [HttpPost]
        public IHttpActionResult Select(string s, string e)
        {
            // load action information
            var session = GetSession(s);
            var element = GetElement(session, e);

            // invoke
            element.UIAutomationElement.Select();

            // sync
            session.RevokeVirtualDom();

            // get
            return Ok();
        }

        // POST wd/hub/session/[s]/native/mouse/move
        // POST session/[s]/native/mouse/move
        [Route("wd/hub/session/{s}/native/mouse/move")]
        [Route("session/{s}/native/mouse/move")]
        [HttpPost]
        public IHttpActionResult SetMousePosition(string s, [FromBody] IDictionary<string, object> data)
        {
            // load action information
            var session = GetSession(s);
            var x = data.ContainsKey("x") ? int.Parse($"{data["x"]}") : 0;
            var y = data.ContainsKey("y") ? int.Parse($"{data["y"]}") : 0;

            // invoke
            session.Automation.SetCursorPosition(x, y);

            // sync
            session.RevokeVirtualDom();
            return Ok();
        }

        // POST wd/hub/session/[s]/element/[e]/native/click
        // POST session/[s]/element/[e]/native/click
        [Route("wd/hub/session/{s}/element/{e}/native/click")]
        [Route("session/{s}/element/{e}/native/click")]
        [HttpPost]
        public IHttpActionResult NativeClick(string s, string e)
        {
            // load action information
            var session = GetSession(s);
            var element = GetElement(session, e);

            // invoke
            element.UIAutomationElement.Click(isNative: true);

            // sync
            session.RevokeVirtualDom();

            // get
            return Ok();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        // POST wd/hub/session/[s]/element/[e]/native/copy
        // POST session/[s]/element/[e]/native/copy
        [Route("wd/hub/session/{s}/element/{e}/native/copy")]
        [Route("session/{s}/element/{e}/native/copy")]
        [HttpPost]
        public IHttpActionResult CopyElement(string s, string e)
        {
            // load action information
            var session = GetSession(s);
            var element = GetElement(session, e);

            // invoke
            element.UIAutomationElement.SetFocus();

            // inputs
            var ctrlDown = GetInput(0x1D, KeyEventF.KeyDown | KeyEventF.Scancode);
            var ctrlUp = GetInput(0x1D, KeyEventF.KeyUp | KeyEventF.Scancode);
            var cDown = GetInput(0x2E, KeyEventF.KeyDown | KeyEventF.Scancode);
            var cUp = GetInput(0x2E, KeyEventF.KeyUp | KeyEventF.Scancode);
            
            var inputs = new[]
            {
                ctrlDown,
                cDown,
                cUp,
                ctrlUp
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));

            // sync
            session.RevokeVirtualDom();

            // get
            return Ok();
        }

        // POST wd/hub/session/[s]/native/paste
        // POST session/[s]/native/paste
        [Route("wd/hub/session/{s}/native/paste")]
        [Route("session/{s}/native/paste")]
        [HttpPost]
        public IHttpActionResult InvokePaste(string s, string e)
        {
            // load action information
            var session = GetSession(s);
            var element = GetElement(session, e);

            // invoke
            element.UIAutomationElement.SetFocus();

            // inputs
            var ctrlDown = GetInput(0x1D, KeyEventF.KeyDown | KeyEventF.Scancode);
            var ctrlUp = GetInput(0x1D, KeyEventF.KeyUp | KeyEventF.Scancode);
            var fDown = GetInput(0x2F, KeyEventF.KeyDown | KeyEventF.Scancode);
            var fUp = GetInput(0x2F, KeyEventF.KeyUp | KeyEventF.Scancode);

            var inputs = new[]
            {
                ctrlDown,
                fDown,
                fUp,
                ctrlUp
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));

            // sync
            session.RevokeVirtualDom();

            // get
            return Ok();
        }

        private Input GetInput(ushort wScan, KeyEventF flags) => new Input
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
    }

    [Flags]
    public enum InputType
    {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }

    [Flags]
    public enum KeyEventF
    {
        KeyDown = 0x0000,
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        Unicode = 0x0004,
        Scancode = 0x0008
    }

    [Flags]
    public enum MouseEventF
    {
        Absolute = 0x8000,
        HWheel = 0x01000,
        Move = 0x0001,
        MoveNoCoalesce = 0x2000,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        VirtualDesk = 0x4000,
        Wheel = 0x0800,
        XDown = 0x0080,
        XUp = 0x0100
    }

    public struct Input
    {
        public int type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)] public MouseInput mi;
        [FieldOffset(0)] public KeyboardInput ki;
        [FieldOffset(0)] public HardwareInput hi;
    }
}