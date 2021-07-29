﻿/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments & document reference
 *    - modify: clean code
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/owin?view=aspnetcore-2.2
 */
using Microsoft.Owin.Hosting;
using System;
using System.Diagnostics;

using UiaDriverServer.Extensions;
using UiaDriverServer.Setup;

namespace UiaDriverServer.Components
{
    internal class OwinFactory
    {
        // constants
        private const string CmptMsg = "[{0}] for [{1}] initialized";

        // members: state
        private StartOptions options;

        // members: information
        private readonly string serviceFullName = nameof(UiaDriverServer);

        public int Port { get; private set; }
        public string Address => Utilities.GetLocalEndpoint();

        /// <summary>
        /// creates a wcf-hose instance
        /// </summary>
        /// <param name="port">port to which the service will listen</param>
        /// <returns>a host for the service</returns>
        public OwinFactory Create(int port)
        {
            // get options
            options = new StartOptions();
            Port = port;

            // initialize service-host
            options.Urls.Add($"http://+:{port}");
            Trace.TraceInformation(CmptMsg, "ServiceEndpoint", serviceFullName);

            // expose service-host
            return this;
        }

        /// <summary>
        /// opens the current wcf-host
        /// </summary>
        public void Open()
        {
            // exit conditions
            if (options == null)
            {
                return;
            }

            try
            {
                // logo
                RednerLogo();

                // open wcf-service
                WebApp.Start<DriverServiceStartup>(options);

                // output information
                Trace.TraceInformation($"Web API listening on    - http://{Address}:{Port}/wd/hub");
                Trace.TraceInformation($"virtual DOM information - http://{Address}:{Port}/wd/hub/session/?id={{session-id}}");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to open {serviceFullName} Web API due to --- {ex} ---");
                throw;
            }
        }

        private void RednerLogo()
        {
            Console.WriteLine("  ▄▄▄▄▄▄▄     ▄▄▄▄▄▄   ▄▄▄▄▄         ▄▄▄▄▄         ");
            Console.WriteLine(" ████████     ██████  █████▀        ███████▄       ");
            Console.WriteLine("  ██████       ████     ▄▄▄▄        ████████▄      ");
            Console.WriteLine("  ██████       ████  ▄██████       ▄███▀██████     ");
            Console.WriteLine("  ██████       ████   ██████      ▄███▀ ▀██████    ");
            Console.WriteLine("  ██████       ████   ██████     ▄██████████████   ");
            Console.WriteLine("  ███████▄   ▄▄████   ██████    ▄███▀▀▀▀▀▀▀██████  ");
            Console.WriteLine("   ▀█████████████▀   ████████ ████████   ██████████");
            Console.WriteLine("      ▀▀▀▀▀▀▀▀▀      ▀▀▀▀▀▀▀  ▀▀▀▀▀▀▀     ▀▀▀▀▀▀▀▀ ");
            Console.WriteLine(" WebDriver implementation for Windows native.      ");
            Console.WriteLine();
            Console.WriteLine(" Powered by IUIAutomation: https://docs.microsoft.com/en-us/windows/win32/api/_winauto/");
            Console.WriteLine(" GitHub Project URL:       https://github.com/gravity-api/gravity-uia-server");
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
