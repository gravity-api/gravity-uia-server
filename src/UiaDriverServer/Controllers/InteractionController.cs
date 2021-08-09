using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

using UiaDriverServer.Contracts;
using UiaDriverServer.Extensions;

namespace UiaDriverServer.Controllers
{
    [ApiController]
    public class InteractionController : UiaController
    {
        // POST wd/hub/session/{session}/element/{element}/value
        // POST session/{session}/element/{element}/value
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

        // POST wd/hub/session/{session}/element/{element}/click
        // POST session/{session}/element/{element}/click
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

        // POST wd/hub/session/{session}/element/{element}/select
        // POST session/{session}/element/{element}/select
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

        // POST wd/hub/session/{session}/native/mouse/move
        // POST session/{session}/native/mouse/move
        [Route("wd/hub/session/{s}/native/mouse/move")]
        [Route("session/{s}/native/mouse/move")]
        [HttpPost]
        public IActionResult SetMousePosition(string s, [FromBody]IDictionary<string, object> data)
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

        // POST wd/hub/session/{session}/element/{element}/native/click
        // POST session/{session}/element/{element}/native/click
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

        // POST wd/hub/session/{session}/element/{element}/native/copy
        // POST session/{session}/element/{element}/native/copy
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

        // POST wd/hub/session/{session}/native/paste
        // POST session/{session}/native/paste
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

        // POST wd/hub/session/{session}/native/inputs
        // POST session/{session}/native/inputs
        [Route("wd/hub/session/{s}/native/inputs")]
        [Route("session/{s}/native/inputs")]
        [HttpPost]
        public IActionResult InvokeKeyboardInputs(string s, [FromBody] IDictionary<string, object> data)
        {
            // setup
            var wScans = JsonSerializer.Deserialize<ushort[]>($"{data["wScans"]}");
            var session = GetSession(s);

            // invoke (one by one)
            foreach (var wScan in wScans)
            {
                var down = GetKeyboardInput(wScan, KeyEventF.KeyDown | KeyEventF.Scancode);
                var up = GetKeyboardInput(wScan, KeyEventF.KeyUp | KeyEventF.Scancode);
                session.Automation.SendInput(new[] { down, up });
            }

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
