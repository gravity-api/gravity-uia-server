using System.Collections.Generic;

namespace UiaWebDriverServer.Contracts
{
    /// <summary>
    /// class to create the capabilities of the browser you require for IWebDriver
    /// </summary>
    public class Capabilities
    {
        /// <summary>
        /// desired-capabilities dictionary
        /// </summary>
        public IDictionary<string, object> DesiredCapabilities { get; set; }
    }
}
