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
using System.Collections.Generic;
using System.Web.Http;

using UiaDriverServer.Extensions;

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

        // POST wd/hub/session/[s]/element/[e]/value
        // POST session/[s]/element/[e]/value
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

        // POST wd/hub/session/[s]/mouse/move
        // POST session/[s]/mouse/move
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

        // POST wd/hub/session/[s]/mouse/move
        // POST session/[s]/mouse/move
        [Route("wd/hub/session/native/{s}/element/{e}/native/click")]
        [Route("session/native/{s}/element/{e}/click")]
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
    }
}