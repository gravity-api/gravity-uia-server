/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-24
 *    - modify: better xml comments & document reference
 */
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
    }
}
