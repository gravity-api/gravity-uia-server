/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Reflection;

using UIAutomationClient;
using UiaWebDriverServer.Contracts.Attributes;

#pragma warning disable IDE0051 // Remove unused private members (used by reflection do not remove)
namespace UiaWebDriverServer.Extensions
{
    public class TextPatternFactory
    {
        public string GetText(int id, object pattern)
        {
            // constants
            const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            // setup
            var methods = typeof(TextPatternFactory).GetMethods(Flags);
            var method = Array
                .Find(methods, i => i.GetCustomAttribute<UiaConstantAttribute>() != null && i.GetCustomAttribute<UiaConstantAttribute>().Constant == id);

            // not supported
            if (method == null)
            {
                return string.Empty;
            }

            // create
            var instance = method.IsStatic ? null : this;

            // get
            return method.Invoke(instance, new object[] { pattern }).ToString();
        }

        #region *** Patterns ***
        [UiaConstant(UIA_PatternIds.UIA_TextChildPatternId)]
        private static string GetByTextChildPattern(object pattern)
        {
            var textChildPattern = (IUIAutomationTextChildPattern)pattern;
            return textChildPattern.TextRange.GetText(-1);
        }

        [UiaConstant(UIA_PatternIds.UIA_TextEditPatternId)]
        private static string GetByTextEditPattern(object pattern)
        {
            var textEditPattern = (IUIAutomationTextEditPattern)pattern;
            return textEditPattern.DocumentRange.GetText(-1);
        }

        [UiaConstant(UIA_PatternIds.UIA_TextPattern2Id)]
        private static string GetByTextPattern2(object pattern)
        {
            var textPattern2 = (IUIAutomationTextPattern2)pattern;
            return textPattern2.DocumentRange.GetText(-1);
        }

        [UiaConstant(UIA_PatternIds.UIA_TextPatternId)]
        private static string GetByTextPattern(object pattern)
        {
            var textPattern = (IUIAutomationTextPattern)pattern;
            return textPattern.DocumentRange.GetText(-1);
        }

        [UiaConstant(UIA_PatternIds.UIA_ValuePatternId)]
        private static string GetByValuePattern(object pattern)
        {
            var valuePattern = (IUIAutomationValuePattern)pattern;
            return valuePattern.CurrentValue;
        }
        #endregion
    }
}
