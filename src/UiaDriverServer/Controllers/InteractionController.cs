using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Linq;

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
        public IActionResult Value(string s, string e, [FromBody] IDictionary<string, object> data)
        {
            // setup
            var session = GetSession(s);
            var element = GetElement(session, e).UIAutomationElement;
            var text = $"{data["text"]}";

            // evaluate action compliance
            element.ApproveReadyForValue(text);

            // invoke
            element.SendKeys(text, isNative: session.GetIsNative());

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
            element.UIAutomationElement.Click(session.GetIsNative());

            // sync
            session.RevokeVirtualDom();

            // get
            return Ok();
        }
    }
}