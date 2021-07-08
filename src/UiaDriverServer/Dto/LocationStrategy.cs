/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments & document reference
 * 
 * docs.w3c.web-driver
 * https://www.w3.org/TR/webdriver1/#locator-strategies
 */
namespace UiaDriverServer.Dto
{
    /// <summary>
    /// an enumerated attribute deciding what technique should be used to
    /// search for elements in the current browsing context
    /// </summary>
    internal class LocationStrategy
    {
        /// <summary>
        /// not implemented (not supported for UiA)
        /// </summary>
        public const string CSS = "css selector";

        /// <summary>
        /// not implemented (not supported for UiA)
        /// </summary>
        public const string LINK_TEXT = "link text";

        /// <summary>
        /// not implemented (not supported for UiA)
        /// </summary>
        public const string PARTIAL_LINK_TEXT = "partial link text";

        /// <summary>
        /// find a web element with the Tag Name strategy
        /// </summary>
        public const string TAG_NAME = "tag name";

        /// <summary>
        /// find a web element with the XPath Selector strategy
        /// </summary>
        public const string XPATH = "xpath";

        /// <summary>
        /// location strategy
        /// </summary>
        public string Using { get; set; }

        /// <summary>
        /// selector
        /// </summary>
        public string Value { get; set; }
    }
}
