/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-24
 *    - modify: better xml comments & document reference
 */
using System.Xml.Linq;

using UiaDriverServer.Dto;

using UIAutomationClient;

namespace UiaDriverServer.Extensions
{
    internal static class AutomationExtensions
    {
        /// <summary>
        /// gets child element from root
        /// </summary>
        /// <param name="automation">automation to get root from</param>
        /// <param name="runtime">element runtime id</param>
        /// <returns>automation element</returns>
        public static IUIAutomationElement GetApplicationRoot(this CUIAutomation8 automation, int[] runtime)
        {
            var conditions = automation.CreatePropertyCondition(UIA_PropertyIds.UIA_RuntimeIdPropertyId, runtime);
            return automation.GetRootElement().FindFirst(TreeScope.TreeScope_Children, conditions);
        }

        public static ClickablePoint GetClickablePoint(this Element element)
        {
            element.UIAutomationElement.GetClickablePoint(out tagPOINT point);

            // setup
            var x = point.x;
            var y = point.y;

            // fallback
            if (point.x == 0 && point.y == 0)
            {
                var document = XDocument.Parse($"{element.Node}");
                var xValue = document.Root.Attribute("left").Value;
                var yValue = document.Root.Attribute("top").Value;

                int.TryParse(xValue, out x);
                int.TryParse(yValue, out y);
            }

            // get
            return new ClickablePoint
            {
                XPos = x,
                YPos = y
            };
        }
    }
}
