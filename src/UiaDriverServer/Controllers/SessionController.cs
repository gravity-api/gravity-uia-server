using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using UiaDriverServer.Attributes;
using UiaDriverServer.Components;
using UiaDriverServer.Contracts;
using UiaDriverServer.Extensions;

using UIAutomationClient;

namespace UiaDriverServer.Controllers
{
    [ApiController]
    public class SessionController : UiaController
    {
        private readonly ILogger<SessionController> logger;
        public string _sessionId;


        public SessionController(ILogger<SessionController> logger)
        {
            this.logger = logger;
        }

        // native iterop       
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // GET wd/hub/status
        // GET status        
        [Route("wd/hub/session/{id}")]
        [Route("session/{id}")]
        [HttpGet]
        public IActionResult Dom([FromRoute] string id)
        {
            // setup
            var notFound = new ContentResult
            {
                StatusCode = StatusCodes.Status404NotFound,
                Content = $"Get-Session -Session [{id}] = NotFound",
                ContentType = MediaTypeNames.Text.Plain
            };
            var ok = new ContentResult
            {
                StatusCode = StatusCodes.Status200OK,
                Content = $"{sessions[id].Dom}",
                ContentType = MediaTypeNames.Application.Xml
            };

            // get
            return sessions.ContainsKey(id) ? ok : notFound;
        }

        // GET wd/hub/status
        // GET status        
        [Route("wd/hub/status")]
        [Route("status")]
        [HttpGet]
        public IActionResult Status()
        {
            // setup conditions
            var isFull = sessions.Count > 0;

            // setup message
            var message = isFull
                ? "Current sessions stack is full, the maximum allowed sessions number is 1"
                : "No sessions in stack, can create new session";

            // compose status
            return Ok(new { Ready = !isFull, Message = message });
        }

        // GET wd/hub/status
        // GET status        
        [Route("wd/hub/shutdown")]
        [Route("shutdown")]
        [HttpGet]
        public IActionResult Shutdown()
        {
            Utilities.CloseDriver();
            return Ok();
        }

        // POST wd/hub/session
        // POST session        
        [Route("wd/hub/session")]
        [Route("session")]
        [HttpPost]
        public IActionResult Session([FromBody] Capabilities capabilities)
        {
            // internal server error
            var (response, assertion) = capabilities.AssertCapabilities();
            if (!assertion)
            {
                return response;
            }

            // setup
            var caps = capabilities.DesiredCapabilities;

            // build
            var args = caps.ContainsKey(UiaCapability.Arguments)
                ? JsonSerializer.Deserialize<IEnumerable<string>>($"{caps[UiaCapability.Arguments]}")
                : Array.Empty<string>();
            var mount = caps.ContainsKey(UiaCapability.Mount) && ((JsonElement)caps[UiaCapability.Mount]).GetBoolean();
            var executeable = $"{capabilities.DesiredCapabilities[UiaCapability.Application]}";

            // get session
            var process = mount
                ? Process.GetProcesses().FirstOrDefault(i => executeable.ToUpper().Contains(i.ProcessName.ToUpper()))
                : Utilities.StartProcess(executeable, string.Join(" ", args));

            // exit conditions
            if (process.MainWindowHandle == default && process.Handle == default && (process.SafeHandle.IsInvalid || process.SafeHandle.IsClosed))
            {
                return new ContentResult
                {
                    StatusCode = 500
                };
            }

            // compose session
            var treeScope = caps.ContainsKey(UiaCapability.TreeScope)
                ? (TreeScope)((JsonElement)caps[UiaCapability.TreeScope]).GetInt32()
                : TreeScope.TreeScope_Descendants;
            var session = new Session(new CUIAutomation8(), process)
            {
                Capabilities = capabilities.DesiredCapabilities,
                TreeScope = treeScope
            };
            
            // generate virtual DOM
            var domFactory = new DomFactory(session);

            // apply session
            session.Dom = domFactory.Create();
            session.SessionId = process.MainWindowHandle == default ? $"{process.Handle}" : $"{process.MainWindowHandle}";
            sessions[session.SessionId] = session;

            // put to screen
            var message = $"Create-Session " +
                $"-Session {session.SessionId} " +
                $"-Application {session.Application.GetNameOrFile()} = Created";
            logger.LogInformation(message);
            logger.LogInformation($"Get-VirtualDom = /session/{session.SessionId}");

            // set response
            return Ok(new { Value = session });
        }

        // POST wd/hub/session/[id]
        // POST session        
        [Route("wd/hub/session/{id}")]
        [Route("session/{id}")]
        [HttpDelete]
        public IActionResult Delete(string id)
        {
            // get session
            var session = GetSession(id);

            // delete remove from state
            session.Delete();
            sessions.Remove(id);

            // put to screen
            var message = $"Delete-Session -Session [{id}] -Application [{session?.Application.StartInfo.FileName}] = NoContent";
            Trace.TraceInformation(message);

            return Ok();
        }

        // POST wd/hub/session/[id]
        // POST session
        [Route("wd/hub/session/{id}/window/maximize")]
        [Route("session/{id}/window/maximize")]
        [HttpPost]
        public IActionResult Maximize(string id)
        {
            // get session
            var session = GetSession(id);

            // delete
            ShowWindow(session.Application.MainWindowHandle, 3);

            // put to screen
            var message = $"Invoke-Maximize -Handle {session.Application.MainWindowHandle} = OK";
            Trace.TraceInformation(message);

            // get
            return Ok();
        }

        // POST wd/hub/session/[id]/actions
        // POST session/[id]/actions
        [Route("wd/hub/session/{id}/actions")]
        [Route("session/{id}/actions")]
        [HttpPost]
        public IActionResult Actions([FromRoute] string id, [FromBody] W3ActionsContract data)
        {
            //setup
            var session = GetSession(id);
            

            var origin = data
                .Actions
                .SelectMany(i => i.Actions)
                .FirstOrDefault(i => i.Origin != null && i.Origin.ContainsKey("element-6066-11e4-a52e-4f735466cecf"));
            var elementId = origin == default ? string.Empty : origin.Origin["element-6066-11e4-a52e-4f735466cecf"];
            var element = GetElement(session, elementId).UIAutomationElement;

            //get coords
            // TODO: fix here to get real cords
            var c = element.GetClickablePoint();
           
            // constants
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

            //setup
            var actions = data.Actions.SelectMany(i => i.Actions).Select(i => i.Type).ToList();
            var methods = GetType()
                .GetMethods(Flags)
                .Where(i => i.GetCustomAttribute<W3ActionAttribute>() != null);

            //iterate
            foreach (var action in actions)
            {
                var method = methods
                    .FirstOrDefault(i => i.GetCustomAttribute<W3ActionAttribute>().Type.Equals(action));

                if (method == null)
                {
                    
                    //throw new InvalidOperationException("The method not exists" + method);

                    //TODO: error handling
                    continue;
                }
                var parameters = new object[] { element };
                if (method.GetParameters().First().ParameterType == typeof (int)) {
                    parameters = new object[] { 0 };
                }
                method.Invoke(null, parameters);
            }

            //get
            return Ok();
        }


        [W3Action(type: "pointerMove")]
        private static void PointerMove(IUIAutomationElement element)
        {


          var position = element.CurrentBoundingRectangle; 
            var input = new NativeStructs.Input
            {
                type = NativeEnums.SendInputEventType.Mouse,
                mouseInput = new NativeStructs.MouseInput
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = NativeEnums.MouseEventFlags.Move,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero,
                },
            };
            var primaryScreen = Screen.PrimaryScreen;
            input.mouseInput.dx = Convert.ToInt32((position.left + 1 - primaryScreen.Bounds.Left) * 65536 / primaryScreen.Bounds.Width);
            input.mouseInput.dy = Convert.ToInt32((position.top + 1 - primaryScreen.Bounds.Top) * 65536 / primaryScreen.Bounds.Height);
            NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));

        }

        [W3Action(type: "pointerUp")]
        private static void PointerUp(IUIAutomationElement element)
        {
            var position = element.CurrentBoundingRectangle; 
            var input = new NativeStructs.Input
            {
                type = NativeEnums.SendInputEventType.Mouse,
                mouseInput = new NativeStructs.MouseInput
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = NativeEnums.MouseEventFlags.LeftUp,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero,
                },
            };
            var primaryScreen = Screen.PrimaryScreen;
            input.mouseInput.dx = Convert.ToInt32((position.left + 1 - primaryScreen.Bounds.Left) * 65536 / primaryScreen.Bounds.Width);
            input.mouseInput.dy = Convert.ToInt32((position.top + 1 - primaryScreen.Bounds.Top) * 65536 / primaryScreen.Bounds.Height);
            NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));

        }

        [W3Action(type: "pointerDown")]
        private static void PointerDown(IUIAutomationElement element)
        {
            var position = element.CurrentBoundingRectangle;
            var input = new NativeStructs.Input
            {
                type = NativeEnums.SendInputEventType.Mouse,
                mouseInput = new NativeStructs.MouseInput
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = NativeEnums.MouseEventFlags.LeftDown,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero,
                },
            };
            var primaryScreen = Screen.PrimaryScreen;
            input.mouseInput.dx = Convert.ToInt32((position.left + 1 - primaryScreen.Bounds.Left) * 65536 / primaryScreen.Bounds.Width);
            input.mouseInput.dy = Convert.ToInt32((position.top + 1 - primaryScreen.Bounds.Top) * 65536 / primaryScreen.Bounds.Height);
            NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
        }

        [W3Action(type: "pause")]
        private static void Pause(int time)
        {
            Thread.Sleep(time);
        }

        [W3Action(type: "keyDown")]
        private static void KeyDown(IUIAutomationElement element, object key)
        {
            var position = element.CurrentBoundingRectangle;
            var input = new NativeStructs.Input
            {
                type = NativeEnums.SendInputEventType.Keyboard,
                keyInput = new NativeStructs.KeyInput
                {
                    wVk = 0,
                    wScan = 0x11, // W
                    dwFlags = NativeEnums.KeyEventFlags.KeyDown | NativeEnums.KeyEventFlags.Scancode,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero,
                },
            };
            var primaryScreen = Screen.PrimaryScreen;
            input.mouseInput.dx = Convert.ToInt32((position.left + 1 - primaryScreen.Bounds.Left) * 65536 / primaryScreen.Bounds.Width);
            input.mouseInput.dy = Convert.ToInt32((position.top + 1 - primaryScreen.Bounds.Top) * 65536 / primaryScreen.Bounds.Height);
            NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));

        }

        [W3Action(type: "keyUp")]
        private static void KeyUp(IUIAutomationElement element, object key)
        {
            var position = element.CurrentBoundingRectangle;
            var input = new NativeStructs.Input
            {
                type = NativeEnums.SendInputEventType.Keyboard,
                keyInput = new NativeStructs.KeyInput
                {

                    wVk = 0,
                    wScan = 0x11, // W
                    dwFlags = NativeEnums.KeyEventFlags.KeyUp | NativeEnums.KeyEventFlags.Scancode,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero,
                },
            };
            var primaryScreen = Screen.PrimaryScreen;
            input.mouseInput.dx = Convert.ToInt32((position.left + 1 - primaryScreen.Bounds.Left) * 65536 / primaryScreen.Bounds.Width);
            input.mouseInput.dy = Convert.ToInt32((position.top + 1 - primaryScreen.Bounds.Top) * 65536 / primaryScreen.Bounds.Height);
            NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));


        }
        private static class NativeStructs
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Input
            {
                public NativeEnums.SendInputEventType type;
                public MouseInput mouseInput;
                public KeyInput keyInput;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MouseInput
            {
                public int dx;
                public int dy;
                public uint mouseData;
                public NativeEnums.MouseEventFlags dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }
            public struct KeyInput
            {
                public ushort wVk;
                public ushort wScan;
                public NativeEnums.KeyEventFlags dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }
        }

        private static class NativeEnums
        {
            internal enum SendInputEventType : int
            {
                Mouse = 0,
                Keyboard = 1,
                Hardware = 2,
            }

            [Flags]
            internal enum MouseEventFlags : uint
            {
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
            internal enum KeyEventFlags : uint
            {
                KeyDown = 0x0000,
                ExtendedKey = 0x0001,
                KeyUp = 0x0002,
                Unicode = 0x0004,
                Scancode = 0x0008
            }
        }
        private static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            internal static extern uint SendInput(uint nInputs, ref NativeStructs.Input pInputs, int cbSize);
        }
    }

}