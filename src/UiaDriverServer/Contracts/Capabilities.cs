using System.Collections.Generic;

namespace UiaDriverServer.Contracts
{
    /// <summary>
    /// class to create the capabilities of the browser you require for IWebDriver
    /// </summary>
    public class Capabilities
    {
        /// <summary>
        /// desired-capabilities dictionary
        /// </summary>
        public Dictionary<string, object> DesiredCapabilities { get; set; }
    }
}
