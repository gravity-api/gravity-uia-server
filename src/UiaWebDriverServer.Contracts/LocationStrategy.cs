/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 * https://www.w3.org/TR/webdriver1/#locator-strategies
 */
using System.Runtime.Serialization;

namespace UiaWebDriverServer.Contracts
{
    /// <summary>
    /// an enumerated attribute deciding what technique should be used to
    /// search for elements in the current browsing context
    /// </summary>
    [DataContract]
    public class LocationStrategy
    {
        /// <summary>
        /// not implemented (not supported for UiA)
        /// </summary>
        public const string CssSelector = "css selector";

        /// <summary>
        /// not implemented (not supported for UiA)
        /// </summary>
        public const string LinkText = "link text";

        /// <summary>
        /// not implemented (not supported for UiA)
        /// </summary>
        public const string PartialLinkText = "partial link text";

        /// <summary>
        /// find a web element with the Tag Name strategy
        /// </summary>
        public const string TagName = "tag name";

        /// <summary>
        /// find a web element with the XPath Selector strategy
        /// </summary>
        public const string Xpath = "xpath";

        /// <summary>
        /// location strategy
        /// </summary>
        [DataMember]
        public string Using { get; set; }

        /// <summary>
        /// selector
        /// </summary>
        [DataMember]
        public string Value { get; set; }
    }
}
