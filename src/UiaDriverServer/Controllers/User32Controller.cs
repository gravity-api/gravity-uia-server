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
    public class User32Controller : UiaController
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        // POST wd/hub/user32/session/{s}/modified
        // POST user32/session/{s}/modified
        [Route("wd/hub/user32/session/{s}/modified")]
        [Route("user32/session/{s}/modified")]
        [HttpPost]
        public IActionResult Modified(string s, [FromBody] IDictionary<string, object> data)
        {
            // setup
            var session = GetSession(s);
            string modifier = $"{data["modifier"]}";
            var key = $"{data["key"]}";

            // invoke
            session.Automation.SendModifiedKey(modifier, key);

            // sync
            session.RevokeVirtualDom();

            // get
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/value
        // POST user32/session/{s}/value
        [Route("wd/hub/user32/session/{s}/value")]
        [Route("user32/session/{s}/value")]
        [HttpPost]
        public IActionResult Value(string s, [FromBody] IDictionary<string, object> data)
        {
            // setup
            var session = GetSession(s);
            var inputs = $"{data["text"]}".GetInputs().ToArray();

            // invoke
            session.Automation.SendInput(inputs);

            // sync
            session.RevokeVirtualDom();

            // get
            return Ok();
        }

        // POST wd/hub/session/user32/{s}/element/{e}/select
        // POST user32/session/{s}/element/{e}/select
        [Route("wd/hub/user32/session/{s}/element/{e}/select")]
        [Route("user32/session/{s}/element/{e}/select")]
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

        // POST wd/hub/user32/session/{s}/mouse/move
        // POST user32/session/{s}/mouse/move
        [Route("wd/hub/user32/session/{s}/mouse/move")]
        [Route("user32/session/{s}/mouse/move")]
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

        // POST wd/hub/user32/session/{s}/element/{e}/click
        // POST user32/session/{s}/element/{e}/click
        [Route("wd/hub/user32/session/{s}/element/{e}/click")]
        [Route("user32/session/{s}/element/{e}/click")]
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

        // POST wd/hub/user32/session/{s}/element/{e}/copy
        // POST user32/session/{s}/element/{e}/copy
        [Route("wd/hub/user32/session/{s}/element/{e}/copy")]
        [Route("user32/session/{s}/element/{e}/copy")]
        [HttpPost]
        public IActionResult InvokeCopy(string s, string e)
        {
            // load action information
            var session = GetSession(s);
            var element = GetElement(session, e);

            // invoke
            element.UIAutomationElement.Select();

            // invoke
            session.Automation.SendModifiedKey("Ctrl", "C");

            // sync
            session.RevokeVirtualDom();

            // get
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/native/paste
        // POST user32/session/{s}/paste
        [Route("wd/hub/user32/session/{s}/native/paste")]
        [Route("user32/session/{s}/paste")]
        [HttpPost]
        public IActionResult InvokePaste(string s, string e)
        {
            // load action information
            var session = GetSession(s);
            var element = GetElement(session, e);

            // invoke
            element.UIAutomationElement.SetFocus();

            // inputs
            // invoke
            session.Automation.SendModifiedKey("Ctrl", "V");

            // sync
            session.RevokeVirtualDom();

            // get
            return Ok();
        }

        // POST wd/hub/user32/session/{session}/inputs
        // POST user32/session/{session}/inputs
        [Route("wd/hub/user32/session/{s}/inputs")]
        [Route("user32/session/{s}/inputs")]
        [HttpPost]
        public IActionResult InvokeKeyboardInputs(string s, [FromBody] IDictionary<string, object> data)
        {
            // local
            // TODO: move to extension
            static Input GetKeyboardInput(ushort wScan, KeyEventF flags) => new()
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
    }
}