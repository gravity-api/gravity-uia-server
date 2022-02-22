using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text.Json;

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
        public IActionResult Session([FromBody]Capabilities capabilities)
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
                ? (TreeScope)((JsonElement) caps[UiaCapability.TreeScope]).GetInt32()
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
    }
}
