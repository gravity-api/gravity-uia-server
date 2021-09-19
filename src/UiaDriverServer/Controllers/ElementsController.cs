using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

using UiaDriverServer.Attributes;
using UiaDriverServer.Contracts;
using UiaDriverServer.Extensions;

using UIAutomationClient;

namespace UiaDriverServer.Controllers
{
    [ApiController]
    public class ElementController : UiaController
    {
        // POST /wd/hub/session/{session}/element
        // POST /session/{session}/element
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

            // root
            var (element, node, runtime) = session.GetFromRoot(locationStrategy);
            if (element != null && node != null)
            {
                // get
                return Get(session, runtime, element, node);
            }

            // parse runtime-id
            var domRuntime = session.GetRuntime(locationStrategy);
            if (domRuntime == null)
            {
                return NotFound();
            }

            // get element
            element = session.GetElementById(domRuntime);
            node = session.Dom.XPathSelectElement($"//*[@id='{domRuntime}']");

            // get
            return Get(session, domRuntime, element, node);
        }

        private IActionResult Get(Session session, string domRuntime, IUIAutomationElement element, XNode node)
        {
            // update state
            session.Elements[domRuntime] = new Element
            {
                UIAutomationElement = element,
                Node = node
            };

            // value response
            var value = new Dictionary<string, string> { [Utilities.EelementReference] = domRuntime };
            return Ok(new { Value = value });
        }

        // GET /wd/hub/session/{session}/element/{element}/text
        // GET /session/{session}/element/{element}/text
        [Route("wd/hub/session/{s}/element/{e}/text")]
        [Route("session/{s}/element/{e}/text")]
        [HttpGet]
        [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "As design. This method have access to internal resource.")]
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

        // GET /wd/hub/session/{s}/element/{e}/attribute/{name}
        // GET /session/{s}/element/{e}/attribute/{name}
        [Route("wd/hub/session/{s}/element/{e}/attribute/{name}")]
        [Route("session/{s}/element/{e}/attribute/{name}")]
        public IActionResult Attribute(string s, string e, string name)
        {
            // exit conditions
            var elementFound = GetSession(s)?.Elements?.ContainsKey(e) == true;
            if (!elementFound)
            {
                return NotFound();
            }

            // setup
            var element = GetSession(s).Elements[e].Node;
            var attribute = XElement.Parse($"{element}").Attribute(name)?.Value;

            // get
            return Ok(new { Value = attribute });
        }
    }
}
