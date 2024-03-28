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
    public partial class User32Controller : ControllerBase
    {
        // Imports the GetMessageExtraInfo function from user32.dll.
        [LibraryImport("user32.dll")]
        private static partial IntPtr GetMessageExtraInfo();

        // Represents the UI automation domain service.
        private readonly IUiaDomain _domain;

        /// <summary>
        /// Initializes a new instance of the <see cref="User32Controller"/> class.
        /// </summary>
        /// <param name="domain">The UI automation domain service.</param>
        public User32Controller(IUiaDomain domain)
        {
            _domain = domain;
        }

        // POST wd/hub/user32/session/{s}/element/{e}/copy
        // POST user32/session/{s}/element/{e}/copy
        [Route("wd/hub/user32/session/{s}/element/{e}/copy")]
        [Route("user32/session/{s}/element/{e}/copy")]
        [HttpPost]
        public IActionResult InvokeCopy(string s, string e)
        {
            // Retrieve the session based on the provided ID
            var session = _domain.GetSession(s);

            // Retrieve the element based on the provided session ID and element ID
            var element = _domain.ElementsRepository.GetElement(s, e);

            // Select the UI automation element
            element.UIAutomationElement.Select();

            // Send the copy command (Ctrl + C) using the session's automation
            session.Automation.SendModifiedKey("Ctrl", "C");

            // Return HTTP status code indicating success
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/element/{e}/click
        // POST user32/session/{s}/element/{e}/click
        [Route("wd/hub/user32/session/{s}/element/{e}/click")]
        [Route("user32/session/{s}/element/{e}/click")]
        [HttpPost]
        public IActionResult InvokeNativeClick(string s, string e)
        {
            // Retrieve the session based on the provided ID
            var session = _domain.GetSession(s);

            // Retrieve the element based on the provided session ID and element ID
            var element = _domain.ElementsRepository.GetElement(s, e);

            // Perform a native click on the UI automation element with the session's scale ratio
            element.UIAutomationElement.NativeClick(session.ScaleRatio);

            // Return HTTP status code indicating success
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/dclick
        // POST user32/session/{s}/dclick
        [Route("wd/hub/user32/session/{s}/dclick")]
        [Route("user32/session/{s}/dclick")]
        [HttpPost]
        public IActionResult InvokeNativeDoubleClick(string s, IDictionary<string, object> data)
        {
            // Retrieve the session based on the provided ID
            var session = _domain.GetSession(s);

            // Extract coordinates of the double click from the provided data
            var x = ((JsonElement)data["x"]).GetInt32();
            var y = ((JsonElement)data["y"]).GetInt32();

            // Perform a native double click at the specified coordinates using the session's automation
            session.Automation.NativeClick(x, y, repeat: 2);

            // Return HTTP status code indicating success
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/native/paste
        // POST user32/session/{s}/paste
        [Route("wd/hub/user32/session/{s}/native/paste")]
        [Route("user32/session/{s}/paste")]
        [HttpPost]
        public IActionResult InvokePaste(string s, string e)
        {
            // Retrieve the session based on the provided ID
            var session = _domain.GetSession(s);

            // Retrieve the element based on the provided session ID and element ID
            var element = _domain.ElementsRepository.GetElement(s, e);

            // Set focus on the UI automation element
            element.UIAutomationElement.SetFocus();

            // Send the paste command (Ctrl + V) using the session's automation
            session.Automation.SendModifiedKey("Ctrl", "V");

            // Return HTTP status code indicating success
            return Ok();
        }

        // POST wd/hub/session/user32/{s}/element/{e}/select
        // POST user32/session/{s}/element/{e}/select
        [Route("wd/hub/user32/session/{s}/element/{e}/select")]
        [Route("user32/session/{s}/element/{e}/select")]
        [HttpPost]
        public IActionResult SelectElement(string s, string e)
        {
            // Retrieve the element based on the provided session ID and element ID
            var element = _domain.ElementsRepository.GetElement(s, e);

            // Select the UI automation element
            element.UIAutomationElement.Select();

            // Return HTTP status code indicating success
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/value
        // POST user32/session/{s}/value
        [Route("wd/hub/user32/session/{s}/value")]
        [Route("user32/session/{s}/value")]
        [HttpPost]
        public IActionResult SendInput(string s, [FromBody] IDictionary<string, object> data)
        {
            // Retrieve the session based on the provided ID
            var session = _domain.GetSession(s);

            // Extract text input from the provided data and convert it to an array of inputs
            var inputs = $"{data["text"]}".GetInputs().ToArray();

            // Send the input to the session's automation
            session.Automation.SendInput(inputs);

            // Return HTTP status code indicating success
            return Ok();
        }

        // POST wd/hub/user32/session/{session}/inputs
        // POST user32/session/{session}/inputs
        [Route("wd/hub/user32/session/{s}/inputs")]
        [Route("user32/session/{s}/inputs")]
        [HttpPost]
        public IActionResult SendKeyboardInputs(string s, [FromBody] IDictionary<string, object> data)
        {
            // TODO: Create as an extension method
            // Function to create keyboard input
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

            // Deserialize scan codes from the provided data
            var wScans = JsonSerializer.Deserialize<string[]>($"{data["wScans"]}").Select(i => i.GetScanCode());

            // Retrieve the session based on the provided ID
            var session = _domain.GetSession(s);

            // Send keyboard inputs for each scan code
            foreach (var wScan in wScans)
            {
                var down = GetKeyboardInput(wScan, KeyEvent.KeyDown | KeyEvent.Scancode);
                var up = GetKeyboardInput(wScan, KeyEvent.KeyUp | KeyEvent.Scancode);
                session.Automation.SendInput(down, up);
            }

            // Return HTTP status code indicating success
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/modified
        // POST user32/session/{s}/modified
        [Route("wd/hub/user32/session/{s}/modified")]
        [Route("user32/session/{s}/modified")]
        [HttpPost]
        public IActionResult SendModifiedKey(string s, [FromBody] IDictionary<string, object> data)
        {
            // Retrieve the session based on the provided ID
            var session = _domain.GetSession(s);

            // Extract modifier and key from the provided data
            string modifier = $"{data["modifier"]}";
            var key = $"{data["key"]}";

            // Send the modified key to the session's automation
            session.Automation.SendModifiedKey(modifier, key);

            // Return HTTP status code indicating success
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/element/{e}/focus
        // POST user32/session/{s}/element/{e}/focus
        [Route("wd/hub/user32/session/{s}/element/{e}/focus")]
        [Route("user32/session/{s}/element/{e}/focus")]
        [HttpGet]
        public IActionResult SetFocus(string s, string e)
        {
            // Retrieve the element based on the provided session ID and element ID
            var element = _domain.ElementsRepository.GetElement(s, e);

            // Set focus on the UI automation element
            element.UIAutomationElement.SetFocus();

            // Return HTTP status code indicating success
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/mouse/move
        // POST user32/session/{s}/mouse/move
        [Route("wd/hub/user32/session/{s}/mouse/move")]
        [Route("user32/session/{s}/mouse/move")]
        [HttpPost]
        public IActionResult SetMousePosition(string s, [FromBody] IDictionary<string, object> data)
        {
            // Retrieve the session based on the provided ID
            var session = _domain.GetSession(s);

            // Extract mouse coordinates from the provided data, defaulting to (0, 0) if not provided
            var x = data.TryGetValue("x", out object xOut) ? int.Parse($"{xOut}") : 0;
            var y = data.TryGetValue("y", out object yOut) ? int.Parse($"{yOut}") : 0;

            // Set the cursor position using the session's automation
            session.Automation.SetCursorPosition(x, y);

            // Return HTTP status code indicating success
            return Ok();
        }

        [Route("wd/hub/user32/session/{s}/element/{e}/mouse/move")]
        [Route("user32/session/{s}/element/{e}/mouse/move")]
        [HttpPost]
        public IActionResult SetMousePosition(string s, string e, [FromBody] IDictionary<string, object> data)
        {
            // Retrieve the session based on the provided ID
            var session = _domain.GetSession(s);

            // Retrieve the element based on the provided session ID and element ID
            var element = _domain.ElementsRepository.GetElement(s, e);

            // Determine alignment of the mouse pointer (default: MiddleCenter)
            var align = data.TryGetValue("align", out object alignOut)
                ? $"{alignOut}"
                : "MiddleCenter";

            // Determine top offset of the mouse pointer (default: 0)
            var topOffset = data.TryGetValue("topOffset", out object topOffsetOut)
                ? int.Parse($"{topOffsetOut}")
                : 0;

            // Determine left offset of the mouse pointer (default: 0)
            var leftOffset = data.TryGetValue("leftOffset", out object leftOffsetOut)
                ? int.Parse($"{leftOffsetOut}")
                : 0;

            // Retrieve the scale ratio of the session
            var scaleRatio = session.ScaleRatio;

            // Get the clickable point on the element with the specified alignment and offsets
            var point = element.UIAutomationElement.GetClickablePoint(align, topOffset, leftOffset, scaleRatio);

            // Set the cursor position to the calculated point
            session.Automation.SetCursorPosition(point.XPos, point.YPos);

            // Return HTTP status code indicating success
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/click
        // POST user32/session/{s}/click
        [Route("wd/hub/user32/session/{s}/click")]
        [Route("user32/session/{s}/click")]
        [HttpPost]
        public IActionResult NativeClick(string s, IDictionary<string, object> data)
        {
            // Retrieve the session based on the provided ID
            var session = _domain.GetSession(s);

            // Extract coordinates of the click from the provided data
            var x = ((JsonElement)data["x"]).GetInt32();
            var y = ((JsonElement)data["y"]).GetInt32();

            // Perform a native click at the specified coordinates using the session's automation
            session.Automation.NativeClick(x, y);

            // Return HTTP status code indicating success
            return Ok();
        }
    }
}
