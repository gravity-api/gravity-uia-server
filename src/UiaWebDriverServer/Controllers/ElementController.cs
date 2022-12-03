/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Net.Mime;
using System.Text.Json;
using System.Xml.Linq;

using UiaWebDriverServer.Contracts;
using UiaWebDriverServer.Domain;

namespace UiaWebDriverServer.Controllers
{
    [ApiController]
    public class ElementController : ControllerBase
    {
        // members
        private readonly IUiaDomain _domain;

        public ElementController(IUiaDomain domain)
        {
            _domain= domain;
        }

        // POST /wd/hub/session/{session}/element
        // POST /session/{session}/element
        [Route("wd/hub/session/{s}/element")]
        [Route("session/{s}/element")]
        [HttpPost]
        public IActionResult FindElement(string s, LocationStrategy locationStrategy)
        {
            // setup
            var (statusCode, element) = _domain.ElementsRepostiroy.FindElement(session: s, locationStrategy);

            // bad request
            if (statusCode == StatusCodes.Status400BadRequest)
            {
                return BadRequest();
            }

            // not found
            if (statusCode == StatusCodes.Status404NotFound || element == null)
            {
                return NotFound();
            }

            // get
            var value = new Dictionary<string, string> { [ElementData.EelementReference] = element.Id };
            return Ok(new { Value = value });
        }

        // GET /wd/hub/session/{session}/element/{element}/text
        // GET /session/{session}/element/{element}/text
        [Route("wd/hub/session/{s}/element/{e}/text")]
        [Route("session/{s}/element/{e}/text")]
        [HttpGet]
        public IActionResult GetElementText(string s, string e)
        {
            // setup
            var (statusCode, text) = _domain.ElementsRepostiroy.GetElementText(session: s, element: e);
            var contentBody = new { Value = text };
            var content = JsonSerializer.Serialize(contentBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // get
            return new ContentResult
            {
                Content = content,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode,
            };
        }

        // GET /wd/hub/session/{s}/element/{e}/attribute/{name}
        // GET /session/{s}/element/{e}/attribute/{name}
        [Route("wd/hub/session/{s}/element/{e}/attribute/{name}")]
        [Route("session/{s}/element/{e}/attribute/{name}")]
        public IActionResult GetElementAttribute(string s, string e, string name)
        {
            // setup
            var (statusCode, text) = _domain
                .ElementsRepostiroy
                .GetElementAttribute(session: s, element: e, attribute: name);
            var contentBody = new { Value = text };
            var content = JsonSerializer.Serialize(contentBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // get
            return new ContentResult
            {
                Content = content,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode,
            };
        }
    }
}
