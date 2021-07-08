using System.Collections.Generic;

namespace UiaDriverServer.Dto
{
    /// <summary>
    /// class to create the capabilities of the browser you require for IWebDriver
    /// </summary>
    internal class Capabilities
    {
        /// <summary>
        /// desired-capabilities dictionary
        /// </summary>
        public Dictionary<string, object> DesiredCapabilities { get; set; }
    }
}
