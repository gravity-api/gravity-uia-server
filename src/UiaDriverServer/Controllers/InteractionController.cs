using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using UiaDriverServer.Contracts;
using UiaDriverServer.Extensions;

namespace UiaDriverServer.Controllers
{
    [ApiController]
    public class InteractionController : UiaController
    {
        // POST wd/hub/session/[s]/element/[e]/value
        // POST session/[s]/element/[e]/value
        [Route("wd/hub/session/{s}/element/{e}/value")]
        [Route("session/{s}/element/{e}/value")]
        [HttpPost]
        public IActionResult Value(string s, string e, [FromBody]IDictionary<string, object> data)
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
        public IActionResult Click(string s, string e)
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
        public IActionResult Select(string s, string e)
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
        public IActionResult SetMousePosition(string s, [FromBody] IDictionary<string, object> data)
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
        public IActionResult NativeClick(string s, string e)
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

        // POST wd/hub/session/[s]/element/[e]/native/copy
        // POST session/[s]/element/[e]/native/copy
        [Route("wd/hub/session/{s}/element/{e}/native/copy")]
        [Route("session/{s}/element/{e}/native/copy")]
        [HttpPost]
        public IActionResult InvokeCopy(string s, string e)
        {
            // load action information
            var session = GetSession(s);
            var element = GetElement(session, e);

            // invoke
            element.UIAutomationElement.SetFocus();

            // inputs           
            var inputs = new[]
            {
                // press CTRL+C
                GetKeyboardInput(0x1D, KeyEventF.KeyDown | KeyEventF.Scancode),
                GetKeyboardInput(0x2E, KeyEventF.KeyDown | KeyEventF.Scancode),
                // release CTRL+C
                GetKeyboardInput(0x2E, KeyEventF.KeyUp | KeyEventF.Scancode),
                GetKeyboardInput(0x1D, KeyEventF.KeyUp | KeyEventF.Scancode)
            };

            // invoke
            session.Automation.SendInput(inputs);

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
        public IActionResult InvokePaste(string s, string e)
        {
            // load action information
            var session = GetSession(s);
            var element = GetElement(session, e);

            // invoke
            element.UIAutomationElement.SetFocus();

            // inputs
            var inputs = new[]
            {
                // press CTRL+V
                GetKeyboardInput(0x1D, KeyEventF.KeyDown | KeyEventF.Scancode),
                GetKeyboardInput(0x2F, KeyEventF.KeyDown | KeyEventF.Scancode),
                // release CTRL+V
                GetKeyboardInput(0x2F, KeyEventF.KeyUp | KeyEventF.Scancode),
                GetKeyboardInput(0x1D, KeyEventF.KeyUp | KeyEventF.Scancode)
            };

            // invoke
            session.Automation.SendInput(inputs);

            // sync
            session.RevokeVirtualDom();

            // get
            return Ok();
        }

        // Utilities
        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();
        private static Input GetKeyboardInput(ushort wScan, KeyEventF flags) => new()
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
}
