using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

using UiaWebDriverServer.Domain;
using UiaWebDriverServer.Extensions;

using static UiaWebDriverServer.Contracts.NativeEnums;
using static UiaWebDriverServer.Contracts.NativeStructs;

namespace UiaWebDriverServer.Controllers
{
    [ApiController]
    public class User32Controller : ControllerBase
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        // members
        private readonly IUiaDomain _domain;

        public User32Controller(IUiaDomain domain)
        {
            _domain = domain;
        }

        // POST wd/hub/user32/session/{s}/modified
        // POST user32/session/{s}/modified
        [Route("wd/hub/user32/session/{s}/modified")]
        [Route("user32/session/{s}/modified")]
        [HttpPost]
        public IActionResult Modified(string s, [FromBody] IDictionary<string, object> data)
        {
            // setup
            var session = _domain.GetSession(s);
            string modifier = $"{data["modifier"]}";
            var key = $"{data["key"]}";

            // invoke
            session.Automation.SendModifiedKey(modifier, key);

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
            var session = _domain.GetSession(s);
            var inputs = $"{data["text"]}".GetInputs().ToArray();

            // invoke
            session.Automation.SendInput(inputs);

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
            var element = _domain.ElementsRepostiroy.GetElement(s, e);

            // invoke
            element.UIAutomationElement.Select();

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
            var session = _domain.GetSession(s);
            var x = data.TryGetValue("x", out object xOut) ? int.Parse($"{xOut}") : 0;
            var y = data.TryGetValue("y", out object yOut) ? int.Parse($"{yOut}") : 0;

            // invoke
            session.Automation.SetCursorPosition(x, y);

            // get
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
            var session = _domain.GetSession(s);
            var element = _domain.ElementsRepostiroy.GetElement(s, e);

            // invoke
            element.UIAutomationElement.NativeClick(session.ScaleRatio);

            // get
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/element/{e}/focus
        // POST user32/session/{s}/element/{e}/focus
        [Route("wd/hub/user32/session/{s}/element/{e}/focus")]
        [Route("user32/session/{s}/element/{e}/focus")]
        [HttpGet]
        public IActionResult SetFocus(string s, string e)
        {
            // load action information
            var element = _domain.ElementsRepostiroy.GetElement(s, e);

            // invoke
            element.UIAutomationElement.SetFocus();

            // get
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/click
        // POST user32/session/{s}/click
        [Route("wd/hub/user32/session/{s}/click")]
        [Route("user32/session/{s}/click")]
        [HttpPost]
        public IActionResult NativeClick(string s, IDictionary<string, object> data)
        {
            // load action information
            var session = _domain.GetSession(s);
            var x = ((JsonElement)data["x"]).GetInt32();
            var y = ((JsonElement)data["y"]).GetInt32();

            // invoke
            session.Automation.NativeClick(x, y);

            // get
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/dclick
        // POST user32/session/{s}/dclick
        [Route("wd/hub/user32/session/{s}/dclick")]
        [Route("user32/session/{s}/dclick")]
        [HttpPost]
        public IActionResult NativeDoubleClick(string s, IDictionary<string, object> data)
        {
            // load action information
            var session = _domain.GetSession(s);
            var x = ((JsonElement)data["x"]).GetInt32();
            var y = ((JsonElement)data["y"]).GetInt32();

            // invoke
            session.Automation.NativeClick(x, y, repeat: 2);

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
            var session = _domain.GetSession(s);
            var element = _domain.ElementsRepostiroy.GetElement(s, e);

            // invoke
            element.UIAutomationElement.Select();
            session.Automation.SendModifiedKey("Ctrl", "C");

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
            var session = _domain.GetSession(s);
            var element = _domain.ElementsRepostiroy.GetElement(s, e);

            // invoke
            element.UIAutomationElement.SetFocus();
            session.Automation.SendModifiedKey("Ctrl", "V");

            // get
            return Ok();
        }

        // POST wd/hub/user32/session/{session}/inputs
        // POST user32/session/{session}/inputs
        [Route("wd/hub/user32/session/{s}/inputs")]
        [Route("user32/session/{s}/inputs")]
        [HttpPost]
        public IActionResult SendKeyboardInputs(string s, [FromBody] IDictionary<string, object> data)
        {
            // local
            // TODO: move to extension
            static Input GetKeyboardInput(ushort wScan, KeyEvent flags) => new()
            {
                type = (int)SendInputEventType.Keyboard,
                union = new InputUnion
                {
                    ki = new KeyInput
                    {
                        wVk = 0,
                        wScan = wScan,
                        dwFlags = (uint)(flags),
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };

            // setup
            var wScans = JsonSerializer.Deserialize<string[]>($"{data["wScans"]}").Select(i => i.GetScanCode());
            var session = _domain.GetSession(s);

            // invoke (one by one)
            foreach (var wScan in wScans)
            {
                var down = GetKeyboardInput(wScan, KeyEvent.KeyDown | KeyEvent.Scancode);
                var up = GetKeyboardInput(wScan, KeyEvent.KeyUp | KeyEvent.Scancode);
                session.Automation.SendInput(new[] { down, up });
            }

            // get
            return Ok();
        }
    }
}
