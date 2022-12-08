using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;

using UiaWebDriverServer.Domain;
using UiaWebDriverServer.Extensions;

namespace UiaWebDriverServer.Controllers
{
    [ApiController]
    public class InteractionController : ControllerBase
    {
        // members
        private readonly IUiaDomain _domain;

        public InteractionController(IUiaDomain domain)
        {
            _domain = domain;
        }

        // POST wd/hub/session/{session}/element/{element}/value
        // POST session/{session}/element/{element}/value
        [Route("wd/hub/session/{s}/element/{e}/value")]
        [Route("session/{s}/element/{e}/value")]
        [HttpPost]
        public IActionResult SetValue(string s, string e, [FromBody] IDictionary<string, object> data)
        {
            // setup
            var (statusCode, _) = _domain.SessionsRepository.GetSession(id: s);
            var element = _domain.ElementsRepostiroy.GetElement(session: s, element: e);
            var text = $"{data["text"]}";

            // not found
            if(statusCode == StatusCodes.Status404NotFound)
            {
                return NotFound();
            }
            if(element == null || element.UIAutomationElement == null)
            {
                return NotFound();
            }

            // evaluate action compliance
            var canHaveValue = element.UIAutomationElement?.AssertCanHaveValue(text) == true;

            // bad request
            if(!canHaveValue)
            {
                return BadRequest();
            }

            // invoke
            element.UIAutomationElement.SendKeys(text);

            // get
            return Ok();
        }

        // POST wd/hub/session/{session}/element/{element}/click
        // POST session/{session}/element/{element}/click
        [Route("wd/hub/session/{s}/element/{e}/click")]
        [Route("session/{s}/element/{e}/click")]
        [HttpPost]
        public IActionResult InvokeClick(string s, string e)
        {
            // setup
            var (statusCode, session) = _domain.SessionsRepository.GetSession(id: s);
            var element = _domain.ElementsRepostiroy.GetElement(session: s, element: e);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return NotFound();
            }
            if (element == null || element.UIAutomationElement == null)
            {
                return NotFound();
            }

            // invoke
            element.UIAutomationElement.Click(session.ScaleRatio);

            // get
            return Ok();
        }
    }
}
