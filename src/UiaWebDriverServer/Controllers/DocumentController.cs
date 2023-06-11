using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Text.Json;

using UiaWebDriverServer.Domain;

namespace UiaWebDriverServer.Controllers
{
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IUiaDomain _domain;

        public DocumentController(IUiaDomain domain)
        {
            _domain = domain;
        }

        // POST wd/hub/session/[id]/execute/sync
        // POST session/[id]/execute/sync        
        [Route("wd/hub/session/{id}/execute/sync")]
        [Route("session/{id}/execute/sync")]
        [HttpPost]
        public IActionResult ExecuteScript(string id, [FromBody] IDictionary<string, object> data)
        {
            // get session
            var src = data["script"].ToString();
            var session = _domain.GetSession(id);

            // invoke
            var (statusCode, result) = _domain
                .DocumentRepository
                .InvokeScript(session.SessionId, src, "powershell");
            
            // setup
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
            };
            var content = JsonSerializer.Serialize( new
            {
                Data = result
            }, options);

            // get
            return new ContentResult
            {
                StatusCode = statusCode,
                Content = content
            };
        }
    }
}
