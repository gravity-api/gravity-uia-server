using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;

using UiaDriverServer.Attributes;
using UiaDriverServer.Dto;
using UiaDriverServer.Extensions;

using UIAutomationClient;

namespace UiaDriverServer.Controllers
{
    [ApiController]
    public class ElementController : UiaController
    {
        // POST wd/hub/session/{session}/element
        // POST session/{session}/element
        [Route("wd/hub/session/{s}/element")]
        [Route("session/{s}/element")]
        [HttpPost]
        public IActionResult Element(string s, LocationStrategy locationStrategy)
        {
            // exit conditions
            var session = GetSession(s);
            if (session == null)
            {
                return NotFound();
            }

            // flat point element (element map by x, y for not discoverable element or flat action)
            var e = locationStrategy.GetFlatPointElement();
            if (e != null)
            {
                // update state
                var reference = $"{Guid.NewGuid()}";
                session.Elements[reference] = e;

                // value response
                var v = new Dictionary<string, string> { [Utilities.EelementReference] = reference };
                return Ok(new { Value = v });
            }

            // parse runtime-id
            var domRuntime = session.GetRuntime(locationStrategy);
            if (domRuntime == null)
            {
                return NotFound();
            }

            // get element
            var dElement = session.Dom.XPathSelectElement($"//*[@id='{domRuntime}']");
            var aElement = session.GetElementById(domRuntime);

            // update state
            session.Elements[domRuntime] = new Element
            {
                UIAutomationElement = aElement,
                Node = dElement
            };

            // value response
            var value = new Dictionary<string, string> { [Utilities.EelementReference] = domRuntime };
            return Ok(new { Value = value });
        }

        // GET wd/hub/session/{session}/element/{element}/text
        // GET /session/{session}/element/{element}/text
        [Route("wd/hub/session/{s}/element/{e}/text")]
        [Route("session/{s}/element/{e}/text")]
        [HttpGet]
        public IActionResult Text(string s, string e)
        {
            // exit conditions
            var elementFound = GetSession(s)?.Elements?.ContainsKey(e) == true;
            if (!elementFound)
            {
                return NotFound();
            }
            var element = GetSession(s).Elements[e].UIAutomationElement;

            // supported text-patterns
            // filtering
            var textPatterns = new[]
            {
                UIA_PatternIds.UIA_TextChildPatternId,
                UIA_PatternIds.UIA_TextEditPatternId,
                UIA_PatternIds.UIA_TextPattern2Id,
                UIA_PatternIds.UIA_TextPatternId,
                UIA_PatternIds.UIA_ValuePatternId
            };

            // load methods
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var methods = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .SelectMany(t => t.GetMethods(FLAGS));

            var patterns = element.GetPatterns().Where(p => textPatterns.Contains(p));
            var afterCa = methods.Where(m => m.GetCustomAttribute<UiaConstantAttribute>() != null);
            var method = afterCa.FirstOrDefault(m => patterns.Contains(m.GetCustomAttribute<UiaConstantAttribute>().Constant));
            var attribute = method?.GetCustomAttribute<UiaConstantAttribute>();

            // not found
            if (attribute == null)
            {
                return Ok(new { Value = string.Empty });
            }

            var constant = attribute.Constant;
            var pattern = element.GetCurrentPattern(constant);

            // execute method
            var instance = Activator.CreateInstance(method.DeclaringType);
            var text = method.Invoke(instance, new object[] { pattern });
            return Ok(new { Value = text });
        }
    }
}