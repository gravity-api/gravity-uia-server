using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using UiaDriverServer.Components;
using UiaDriverServer.Dto;
using UiaDriverServer.Extensions;

using UIAutomationClient;

namespace UiaDriverServer.Controllers
{
    [ApiController]
    public class SessionController : UiaController
    {
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
            // setup conditions
            var haveSession = sessions.ContainsKey(id);
            if (!haveSession)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Content = $"Get-Session -Session [{id}] = NotFound",
                    ContentType = MediaTypeNames.Text.Plain
                };
            }

            // return xml
            return new ContentResult
            {
                StatusCode = StatusCodes.Status200OK,
                Content = $"{sessions[id].Dom}",
                ContentType = MediaTypeNames.Application.Xml
            };
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
            Exit();
            return Ok();
        }

        // POST wd/hub/session
        // POST session        
        [Route("wd/hub/session")]
        [Route("session")]
        [HttpPost]
        public IActionResult Session([FromBody] Capabilities capabilities)
        {
            // evaluate
            var eval = Evaluate(capabilities, out bool passed);
            if (!passed)
            {
                return eval;
            }

            // get session initialization information
            var args = string.Empty;
            var executeable = $"{capabilities.DesiredCapabilities[UiaCapability.Application]}";
            if (capabilities.DesiredCapabilities.ContainsKey(UiaCapability.Arguments))
            {
                args = $"{capabilities.DesiredCapabilities[UiaCapability.Arguments]}";
            }
            var process = Get(executeable, args).WaitForHandle(TimeSpan.FromSeconds(60));

            // exit conditions
            if (process.MainWindowHandle == default)
            {
                return new ContentResult
                {
                    StatusCode = 500
                };
            }

            // compose session
            var session = new Session(new CUIAutomation8())
            {
                Application = process,
                Capabilities = capabilities.DesiredCapabilities
            };

            // generate virtual DOM
            var domFactory = new DomFactory(session);

            // apply session
            session.Dom = domFactory.Create();
            session.SessionId = $"{process.MainWindowHandle}";
            sessions[session.SessionId] = session;

            // put to screen
            var message = $"Create-Session -Session {session.SessionId} -Application {session.Application.StartInfo.FileName} = Created";
            Trace.TraceInformation(message);

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

        private IActionResult Evaluate(Capabilities capabilities, out bool passed)
        {
            // shortcuts
            var c = capabilities.DesiredCapabilities;
            passed = false;

            // evaluate
            if (!c.ContainsKey(UiaCapability.Application))
            {
                var exception = Get(UiaCapability.Application);
                return new ContentResult
                {
                    Content = exception.Message,
                    ContentType = MediaTypeNames.Text.Plain,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            if (!c.ContainsKey(UiaCapability.PlatformName))
            {
                var exception = Get(UiaCapability.PlatformName);
                return new ContentResult
                {
                    Content = exception.Message,
                    ContentType = MediaTypeNames.Text.Plain,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            if (!$"{c[UiaCapability.PlatformName]}".Equals("windows", StringComparison.OrdinalIgnoreCase))
            {
                var exception =
                    new ArgumentException("Platform name must be 'windows'", nameof(capabilities));
                return new ContentResult
                {
                    Content = exception.Message,
                    ContentType = MediaTypeNames.Text.Plain,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            passed = true;
            return Ok();
        }

        private static ArgumentException Get(string capabilities)
        {
            const string m = "You must provide [{0}] capability";
            var message = string.Format(m, capabilities);
            return new ArgumentException(message, nameof(capabilities));
        }

        private static Process Get(string app, string args)
        {
            // initialize notepad process
            var process = new Process
            {
                StartInfo = new ProcessStartInfo { FileName = app, Arguments = args }
            };
            process.Start();
            process.WaitForInputIdle();
            return process;
        }

        private static void Exit() => Task.Run(() =>
        {
            Trace.TraceInformation("Shutting down...");
            Thread.Sleep(1000);
            Environment.Exit(0);
        });
    }
}