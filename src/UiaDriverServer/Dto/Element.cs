using System.Collections;
using System.Xml.Linq;
using UIAutomationClient;

namespace UiaDriverServer.Dto
{
    internal class Element
    {
        public IUIAutomationElement UIAutomationElement { get; set; }
        public XNode Node { get; set; }
        public ClickablePoint ClickablePoint { get; set; }
        public Location Location { get; set; }
    }
}
