/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments & document reference
 *    - modify: clean code
 *    - modify: add support for tree-scope
 *    
 * 2019-02-09
 *    - modify: change to automation COM instead of System.Windows.Automation
 *    - modify: add parent container before trying to find the element (improve performance)
 *    
 * 2019-02-10
 *    - modify: add not-found response to comply with element-not-found exception
 * 
 * docs.w3c.web-driver
 * https://www.w3.org/TR/webdriver1/#find-element
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.treescope?view=netframework-4.7.2
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.propertycondition?view=netframework-4.7.2
 */
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Xml.XPath;

using UiaDriverServer.Attributes;
using UiaDriverServer.Components;
using UiaDriverServer.Dto;
using UiaDriverServer.Extensions;

using UIAutomationClient;

namespace UiaDriverServer.Controllers
{
    public class ElementController : UiaController
    {
        // members: state
        private Session session;

        // POST wd/hub/session/[id]/element
        // POST session/[id]/element
        [Route("wd/hub/session/{s}/element")]
        [Route("session/{s}/element")]
        [HttpPost]
        public IHttpActionResult Element(string s, [FromBody] object dto)
        {
            // exit conditions
            var sessionFound = GetSession(s) != null;
            if (!sessionFound)
            {
                return NotFound();
            }

            // initialize & refresh session
            session = GetSession(s).RevokeVirtualDom();
            var locationStrategy = ((JToken)dto).ToObject<LocationStrategy>();

            // flat point element (element map by x, y for not discoverable element or flat action)
            var e = GetFlatPointElement(locationStrategy);
            if (e != null)
            {
                // update state
                var reference = $"{Guid.NewGuid()}";
                session.Elements[reference] = e;

                // value response
                var v = new Dictionary<string, string> { [Utilities.ELEMENT_REFERENCE] = reference };
                return Json(new { Value = v }, jsonSettings);
            }

            // parse runtime-id
            var domRuntime = GetDomRuntime(locationStrategy);
            if (domRuntime == null)
            {
                return NotFound();
            }

            // get element
            var dElement = session.Dom.XPathSelectElement($"//*[@id='{domRuntime}']");
            var aElement = Get(domRuntime);

            // update state
            session.Elements[domRuntime] = new Element
            {
                UIAutomationElement = aElement,
                Node = dElement
            };

            // value response
            var value = new Dictionary<string, string> { [Utilities.ELEMENT_REFERENCE] = domRuntime };
            return Json(new { Value = value }, jsonSettings);
        }

        // GET wd/hub/session/{session id}/element/{element id}/text
        // GET /session/[id]/element/[id]/text
        [Route("wd/hub/session/{s}/element/{e}/text")]
        [Route("session/{s}/element/{e}/text")]
        [HttpGet]
        public IHttpActionResult Text(string s, string e)
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
                return Json(new { Value = string.Empty }, jsonSettings);
            }

            var constant = attribute.Constant;
            var pattern = element.GetCurrentPattern(constant);

            // execute method
            var instance = Activator.CreateInstance(method.DeclaringType);
            var text = method.Invoke(instance, new object[] { pattern });
            return Json(new { Value = text }, jsonSettings);
        }

        private Element GetFlatPointElement(LocationStrategy locationStrategy)
        {
            const string P1 = @"(?i)//cords\[\d+,\d+]";
            const string P2 = @"\[\d+,\d+]";

            // setup conditions
            var isCords = Regex.IsMatch(locationStrategy.Value, P1);
            if (!isCords)
            {
                return null;
            }

            // load cords
            var cords = JsonConvert.DeserializeObject<int[]>(Regex.Match(locationStrategy.Value, P2).Value);
            return new Element { ClickablePoint = new ClickablePoint(xpos: cords[0], ypos: cords[1]) };
        }

        private string GetDomRuntime(LocationStrategy locationStrategy)
        {
            var domElement = session.Dom.XPathSelectElement(locationStrategy.Value);
            return domElement?.Attribute("id").Value;
        }

        private IUIAutomationElement Get(string domRuntime)
        {
            // get container
            var containerElement = session.GetApplicationRoot();

            // create finding condition
            var c = GetRuntimeCondition(domRuntime);
            return containerElement.FindFirst(TreeScope.TreeScope_Descendants, c);
        }

        private IUIAutomationCondition GetRuntimeCondition(string domRuntime)
        {
            // shortcuts
            var runtime = Utilities.GetRuntime(domRuntime);
            const int pid = UIA_PropertyIds.UIA_RuntimeIdPropertyId;

            // get condition
            return session.Automation.CreatePropertyCondition(pid, runtime);
        }
    }
}