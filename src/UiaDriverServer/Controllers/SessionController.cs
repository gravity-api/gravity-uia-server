/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments & document reference
 *    
 * 2019-02-12
 *    - modify: align with standard capabilities (app, arguments & platformName)
 *    - modify: add capabilities validation
 * 
 * docs.w3c.web-driver
 * https://www.w3.org/TR/webdriver1/#new-session
 * https://www.w3.org/TR/webdriver1/#delete-session
 * 
 * docs.c-sharpcorner
 * https://www.c-sharpcorner.com/uploadfile/puranindia/windows-management-instrumentation-in-C-Sharp/
 */
using Newtonsoft.Json.Linq;

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

using UiaDriverServer.Components;
using UiaDriverServer.Domain;
using UiaDriverServer.Dto;
using UiaDriverServer.Extensions;

using UIAutomationClient;

namespace UiaDriverServer.Controllers
{
    // TODO: implement timeouts
    /// <summary>
    /// a single instantiation of a particular user agent,
    /// including all its child browsers/windows
    /// </summary>
    public class SessionController : UiaController
    {
        // native iterop       
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // GET wd/hub/status
        // GET status        
        [Route("wd/hub/session")]
        [Route("session")]
        [HttpGet]
        public HttpResponseMessage Dom([FromUri] string id)
        {
            // setup conditions
            var haveSession = sessions.ContainsKey(id);
            if (!haveSession)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    ReasonPhrase = $"Get-Session -Session [{id}] = NotFound"
                };
            }

            // return xml
            var content = new StringContent($"{sessions[id].Dom}", Encoding.UTF8, "application/xml");
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = content
            };
        }

        // GET wd/hub/status
        // GET status        
        [Route("wd/hub/status")]
        [Route("status")]
        [HttpGet]
        public IHttpActionResult Status()
        {
            // setup conditions
            var isFull = sessions.Count > 0;

            // setup message
            var message = isFull
                ? "Current sessions stack is full, the maximum allowed sessions number is 1"
                : "No sessions in stack, can create new session";

            // compose status
            return Json(new { Ready = !isFull, Message = message }, jsonSettings);
        }

        // GET wd/hub/status
        // GET status        
        [Route("wd/hub/shutdown")]
        [Route("shutdown")]
        [HttpGet]
        public IHttpActionResult Shutdown()
        {
            Exit();
            return Ok();
        }

        // POST wd/hub/session
        // POST session        
        [Route("wd/hub/session")]
        [Route("session")]
        [HttpPost]
        public IHttpActionResult Session([FromBody] object dto)
        {
            // evaluate capabilities
            var capabilities = ((JToken)dto).ToObject<Capabilities>();

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
                return InternalServerError();
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
            return Json(new { Value = session }, jsonSettings);
        }

        // POST wd/hub/session/[id]
        // POST session        
        [Route("wd/hub/session/{id}")]
        [Route("session/{id}")]
        [HttpDelete]
        public IHttpActionResult Delete(string id)
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
        public IHttpActionResult Maximize(string id)
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

        private IHttpActionResult Evaluate(Capabilities capabilities, out bool passed)
        {
            // shortcuts
            var c = capabilities.DesiredCapabilities;
            passed = false;

            // evaluate
            if (!c.ContainsKey(UiaCapability.Application))
            {
                var exception = Get(UiaCapability.Application);
                return InternalServerError(exception);
            }
            if (!c.ContainsKey(UiaCapability.PlatformName))
            {
                var exception = Get(UiaCapability.PlatformName);
                return InternalServerError(exception);
            }
            if (!$"{c[UiaCapability.PlatformName]}".Equals("windows", StringComparison.OrdinalIgnoreCase))
            {
                var exception =
                    new ArgumentException("Platform name must be 'windows'", nameof(capabilities));
                return InternalServerError(exception);
            }
            passed = true;
            return Ok();
        }

        private ArgumentException Get(string capabilities)
        {
            const string m = "You must provide [{0}] capability";
            var message = string.Format(m, capabilities);
            return new ArgumentException(message, nameof(capabilities));
        }

        private Process Get(string app, string args)
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

        private void Exit() => Task.Run(() =>
        {
            Trace.TraceInformation("Shutting down...");
            Thread.Sleep(1000);
            Environment.Exit(0);
        });

        // loads the current connector-feature
        private void LoadFeatures(Capabilities c, string feature)
        {
            // constants: logging
            const string M1 = "you are not allowed to use [{0}] feature, please contact customers support at gravity.customer-services@outlook.com";
            const string M2 = "you must provide [gUser] & [gPassword] capabilities";

            // constants            
            const StringComparison COMPARE = StringComparison.OrdinalIgnoreCase;
            const string USER = "gUser";
            const string PASSWORD = "gPassword";

            // setup conditions
            var isUser = c.DesiredCapabilities.ContainsKey(USER);
            var isPassword = c.DesiredCapabilities.ContainsKey(PASSWORD);
            var isComplient = isUser && isPassword;

            // exit conditions
            if (!isComplient)
            {
                throw new NotSupportedException(M2);
            }

            // shortcut           
            var u = $"{c.DesiredCapabilities[USER]}";
            var p = $"{c.DesiredCapabilities[PASSWORD]}";

            // set conditions
            var isAllowed = Account.GetFeatures(u, p).Any(f => f.Equals(feature, COMPARE));
            if (isAllowed)
            {
                return;
            }

            // terminate connector
            throw new NotSupportedException(string.Format(M1, feature));
        }
    }
}