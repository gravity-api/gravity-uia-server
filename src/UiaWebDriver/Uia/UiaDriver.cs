using OpenQA.Selenium.Extensions;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;

namespace OpenQA.Selenium.Uia
{
    public class UiaDriver : RemoteWebDriver
    {
        public UiaDriver() : this(new UiaOptions()) { }

        /// <summary>
        /// initializes a new instance of the OpenQA.Selenium.Remote.RemoteWebDriver class.
        /// this constructor defaults proxy to http://127.0.0.1:4444/wd/hub
        /// </summary>
        /// <param name="options">an OpenQA.Selenium.DriverOptions object containing the desired capabilities of
        /// the browser.</param>
        public UiaDriver(UiaOptions options)
            : this(UiaDriverService.CreateDefault(options.DriverPath), options, DefaultCommandTimeout) { }

        /// <summary>
        /// initializes a new instance of the OpenQA.Selenium.Remote.RemoteWebDriver class.
        /// this constructor defaults proxy to http://127.0.0.1:4444/wd/hub
        /// </summary>
        /// <param name="desiredCapabilities">an OpenQA.Selenium.ICapabilities object containing the desired capabilities of
        /// the browser</param>
        public UiaDriver(ICapabilities desiredCapabilities)
            : base(new DriverServiceCommandExecutor(UiaDriverService.CreateDefault($"{desiredCapabilities["driverPath"]}"), DefaultCommandTimeout), desiredCapabilities) { }

        /// <summary>
        /// initializes a new instance of the OpenQA.Selenium.Remote.RemoteWebDriver class.
        /// this constructor defaults proxy to http://127.0.0.1:4444/wd/hub
        /// </summary>
        /// <param name="uiaDriverDirectory">the full path to the directory containing UiaWebDriver.exe</param>
        /// <param name="application">the full path of the application execution file</param>
        public UiaDriver(string uiaDriverDirectory, string application)
            : this(UiaDriverService.CreateDefault(uiaDriverDirectory), new UiaOptions { Application = application }, DefaultCommandTimeout) { }

        /// <summary>
        /// initializes a new instance of the OpenQA.Selenium.Remote.RemoteWebDriver class.
        /// this constructor defaults proxy to http://127.0.0.1:4444/wd/hub
        /// </summary>
        /// <param name="remoteAddress">uri containing the address of the WebDriver remote server (e.g. http://127.0.0.1:4444/wd/hub)</param>)
        /// <param name="options">an OpenQA.Selenium.DriverOptions object containing the desired capabilities of
        /// the browser</param>
        public UiaDriver(Uri remoteAddress, DriverOptions options) 
            : base(remoteAddress, options) { }

        /// <summary>
        /// initializes a new instance of the <see cref="UiDriver"/> class using the specified <see cref="UiaDriverService"/>
        /// </summary>
        /// <param name="service">the <see cref="ChromeDriverService"/> to use.</param>
        /// <param name="options">The <see cref="ChromeOptions"/> to be used with the Chrome driver.</param>
        /// <param name="commandTimeout">The maximum amount of time to wait for each command.</param>
        public UiaDriver(UiaDriverService service, UiaOptions options, TimeSpan commandTimeout)
            : base(new DriverServiceCommandExecutor(service, commandTimeout), options.ToCapabilities())
        {
            // add the custom commands unique to UIA (if needed)
        }

        /// <summary>
        /// gets the capabilities as a dictionary
        /// </summary>
        /// <param name="capabilitiesToConvert">the dictionary to return</param>
        /// <returns>a dictionary consisting of the capabilities requested</returns>
        /// <remarks>this method is only transitional. do not rely on it. it will be removed once
        /// browser driver capability formats stabilize</remarks>
        protected override Dictionary<string, object> GetCapabilitiesDictionary(ICapabilities capabilitiesToConvert)
        {
            return capabilitiesToConvert.ToDictionary<UiaCapabilities>(true);
        }
        #pragma warning restore
    }
}