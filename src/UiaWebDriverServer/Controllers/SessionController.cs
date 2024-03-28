/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Net.Mime;
using System.Text.Json;

using UiaWebDriverServer.Contracts;
using UiaWebDriverServer.Domain;

namespace UiaWebDriverServer.Controllers
{
    [ApiController]
    public class SessionController(IUiaDomain domain) : ControllerBase
    {
        private readonly IUiaDomain _domain = domain;

        // POST wd/hub/session
        // POST session
        [Route("wd/hub/session")]
        [Route("session")]
        [HttpPost]
        public IActionResult CreateSession([FromBody] Capabilities capabilities)
        {
            // setup
            var session = _domain.SessionsRepository.CreateSession(capabilities);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var content = session.Entity == null
                ? "{}"
                : JsonSerializer.Serialize(session.Entity, options);

            // get
            return new ContentResult
            {
                Content = content,
                StatusCode = session.StatusCode,
                ContentType = MediaTypeNames.Application.Json
            };
        }

        // GET wd/hub/session/:id
        // GET session/:id
        [Route("wd/hub/session/{id}")]
        [Route("session/{id}")]
        [HttpGet]
        public IActionResult GetSessionXml([FromRoute] string id)
        {
            // setup
            var (statusCode, xml) = _domain.SessionsRepository.CreateSessionXml(id);
            var contentType = statusCode == StatusCodes.Status200OK
                ? MediaTypeNames.Application.Xml
                : MediaTypeNames.Text.Plain;
            var content = statusCode == StatusCodes.Status200OK
                ? xml.ToString()
                : $"Get-Session -Session [{id}] = NotFound";

            // get
            return new ContentResult
            {
                Content = content,
                ContentType = contentType,
                StatusCode = statusCode
            };
        }

        // GET wd/hub/status
        // GET status
        [Route("wd/hub/status")]
        [Route("status")]
        [HttpGet]
        public IActionResult GetStatus()
        {
            // setup conditions
            var isFull = _domain.SessionsRepository.Sessions.Count > 0;

            // setup message
            var message = isFull
                ? "Current sessions stack is full, the maximum allowed sessions number is 1"
                : "No sessions in stack, can create new session";

            // compose status
            return Ok(new { Ready = !isFull, Message = message });
        }

        // DELETE wd/hub/session/:id
        // DELETE session/:id
        [Route("wd/hub/session/{id}")]
        [Route("session/{id}")]
        [HttpDelete]
        public IActionResult DeleteSession(string id)
        {
            // get session
            var response = _domain.SessionsRepository.DeleteSession(id);

            // get
            return new ContentResult
            {
                StatusCode = response
            };
        }

        // DELETE wd/hub/session
        // DELETE session
        [Route("wd/hub/session")]
        [Route("session")]
        [HttpDelete]
        public IActionResult DeleteSession()
        {
            // get session
            var sessions = _domain.SessionsRepository.Sessions.Keys;
            foreach (var id in sessions)
            {
                try
                {
                    _domain.SessionsRepository.DeleteSession(id);
                }
                catch (Exception)
                {
                    // ignore exceptions
                }
            }

            // get
            return new ContentResult
            {
                StatusCode = 204
            };
        }

        // POST wd/hub/session/:id/screenshot
        // POST session/:id/screenshot
        [Route("wd/hub/session/{id}/screenshot")]
        [Route("session/{id}/screenshot")]
        [HttpGet]
        public IActionResult GetScreenshot()
        {
            // get session
            var (statusCode, entity) = _domain.SessionsRepository.GetScreenshot();

            // setup
            var responseObject = new
            {
                Value = entity
            };
            var content = JsonSerializer.Serialize(responseObject, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // get
            return new ContentResult
            {
                Content = content,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = statusCode
            };
        }
    }
}
