using Microsoft.AspNetCore.Mvc;

using System.Net.Mime;
using System.Text.Json;

using UIAutomationClient;

using UiaWebDriverServer.Domain;

namespace UiaWebDriverServer.Controllers
{
    [ApiController]
    public class ContextController : ControllerBase
    {
        // members
        private readonly IUiaDomain _domain;

        public ContextController(IUiaDomain domain)
        {
            _domain = domain;
        }

        // POST wd/hub/session/:id/window/maximize
        // POST session/:id/window/maximize
        [Route("wd/hub/session/{id}/window/maximize")]
        [Route("session/{id}/window/maximize")]
        [HttpPost]
        public IActionResult WindowMaximize([FromRoute] string id)
        {
            // setup
            var session = _domain.SessionsRepository.SetWindowVisualState(id, WindowVisualState.WindowVisualState_Maximized);
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
    }
}
