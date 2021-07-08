using OpenQA.Selenium.Extensions;
using System;
using System.IO;

namespace OpenQA.Selenium.Uia
{
    public class UiaDriverService : DriverService
    {
        // constants
        private const string DEFAULT_NAME = "UiaDriverServer.exe";
        private const string UIA_DRIVER = "https://1drv.ms/f/s!AmRY2pH37JevgW4abckY7qlNdorq";
        private static readonly Uri DRIVER_URI = new Uri(UIA_DRIVER);

        /// <summary>
        /// initializes a new instance of the OpenQA.Selenium.DriverService class
        /// </summary>
        /// <param name="servicePath">the full path to the directory containing the executable providing the service to drive the application</param>
        /// <param name="port">the port on which the driver executable should listen</param>
        public UiaDriverService(string servicePath, int port) : base(servicePath, port, DEFAULT_NAME, DRIVER_URI)
        {
            Evaluate();
        }

        /// <summary>
        /// creates a default instance of the UiaDriverService
        /// </summary>
        /// <returns>a UiaDriverService that implements default settings</returns>
        public static UiaDriverService CreateDefault()
        {
            var f = FindDriverServiceExecutable(DEFAULT_NAME, DRIVER_URI);
            var d = Path.GetDirectoryName(f);
            return new UiaDriverService(d, Utilities.FindFreePort());
        }

        /// <summary>
        /// creates a default instance of the UiaDriverService
        /// </summary>
        /// <param name="servicePath">the full path to the directory containing the executable providing the service to drive the application</param>
        /// <returns>a UiaDriverService that implements default settings</returns>
        public static UiaDriverService CreateDefault(string servicePath)
        {
            return new UiaDriverService(servicePath, Utilities.FindFreePort());
        }

        private void Evaluate()
        {
            var isCompliantPlatform = Environment.OSVersion.Platform == PlatformID.Win32NT;
            if (!isCompliantPlatform)
            {
                var message = $"platform [{Environment.OSVersion.Platform}] in not supported. " +
                    $"make sure your platform is [{PlatformID.Win32NT}]";
                throw new InvalidOperationException(message);
            }
        }
    }
}