/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Xml.Linq;

using UIAutomationClient;

namespace UiaWebDriverServer.Contracts
{
    public class Element
    {
        public string Id { get; set; }
        public IUIAutomationElement UIAutomationElement { get; set; }
        public XNode Node { get; set; }
        public ClickablePoint ClickablePoint { get; set; }
        public Location Location { get; set; }
    }
}
