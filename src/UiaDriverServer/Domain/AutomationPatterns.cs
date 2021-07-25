using UiaDriverServer.Attributes;
using UIAutomationClient;

namespace UiaDriverServer.Domain
{
    internal class AutomationPatterns
    {
        [UiaConstant(UIA_PatternIds.UIA_TextChildPatternId)]
        public string G10029(object pattern)
        {
            var p = (IUIAutomationTextChildPattern)pattern;
            return p.TextRange.GetText(-1);
        }

        [UiaConstant(UIA_PatternIds.UIA_TextEditPatternId)]
        public string G10032(object pattern)
        {
            var p = (IUIAutomationTextEditPattern)pattern;
            return p.DocumentRange.GetText(-1);
        }

        [UiaConstant(UIA_PatternIds.UIA_TextPattern2Id)]
        public string G10024(object pattern)
        {
            var p = (IUIAutomationTextPattern2)pattern;
            return p.DocumentRange.GetText(-1);
        }

        [UiaConstant(UIA_PatternIds.UIA_TextPatternId)]
        public string G10014(object pattern)
        {
            var p = (IUIAutomationTextPattern)pattern;
            return p.DocumentRange.GetText(-1);
        }

        [UiaConstant(UIA_PatternIds.UIA_ValuePatternId)]
        public string G10002(object pattern)
        {
            var p = (IUIAutomationValuePattern)pattern;
            return p.CurrentValue;
        }
    }
}