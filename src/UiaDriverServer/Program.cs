using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using UiaDriverServer.Extensions;

namespace UiaDriverServer
{
    public static class Program
    {
        // constatns
        private const string Hub = "hub";
        private const string HubPort = "hubPort";
        private const string Host = "host";
        private const string Port = "port";
        private const string Register = "register";
        
        public static void Main(string[] args)
        {
            // cute
            Utilities.RednerLogo();

            // setup
            var arguments = GetArguments(args);

            // register
            if (arguments.ContainsKey(Register))
            {
                RegisterNode(arguments);
            }

            // invoke
            CreateWebHostBuilder(arguments).Build().Run();
        }

        // creates web service host container
        private static IWebHostBuilder CreateWebHostBuilder(IDictionary<string, string> arguments) => WebHost
            .CreateDefaultBuilder()
            .UseUrls()
            .ConfigureKestrel(o => o.Listen(IPAddress.Any, GetPort(arguments)))
            .UseStartup<Startup>();

        // register
        private static void RegisterNode(IDictionary<string, string> arguments)
        {
            // setup
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // build
            var host = arguments.ContainsKey(Host) ? arguments[Host] : "localhost";
            var port = arguments.ContainsKey(Port) && int.TryParse(arguments[Port], out int portOut)
                ? portOut
                : 5555;
            var hub = arguments.ContainsKey(Hub) ? arguments[Hub] : "localhost";
            var hubPort = arguments.ContainsKey(HubPort) && int.TryParse(arguments[HubPort], out int hubPortOut)
                ? hubPortOut
                : 4444;
            var nodeConfiguration = GetNodeConfiguration(port, hubPort, host);

            // setup
            var content = JsonSerializer.Serialize(nodeConfiguration, options);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Content = stringContent,
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{hub}:{hubPort}/grid/register/")
            };

            // invoke
            using var clinet = new HttpClient();
            var response = clinet.SendAsync(request).GetAwaiter().GetResult();

            // assert
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Register-Node -Host {host}:{port} -Hub {hub}:{hubPort} = Ok");
                return;
            }
            var message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            throw new ArgumentException(message, nameof(arguments));
        }

        private static object GetNodeConfiguration(int port, int hubPort, string host)
        {
            return new
            {
                Capabilities = new[]
                {
                    new
                    {
                        BrowserName = "UiAutomation",
                        BrowserVersion = "1.0",
                        Platform = "WINDOWS",
                        MaxInstances = 1,
                        Role = "WebDriver"
                    }
                },
                Configuration = new
                {
                    _comment = "Configuration for Windows, UIAutomation based Node.",
                    CleanUpCycle = 2000,
                    Timeout = 30000,
                    Port = port,
                    Host = host,
                    Register = true,
                    HubPort = hubPort,
                    MaxSessions = 1
                }
            };
        }

        private static int GetPort(IDictionary<string, string> arguments)
        {
            // setup
            var port = arguments.ContainsKey(Port) && int.TryParse(arguments[Port], out int portOut)
                ? portOut
                : 4444;

            // assert
            if (arguments.ContainsKey(Register) && port == 4444)
            {
                port = 5555;
            }

            // get
            return port;
        }

        private static IDictionary<string, string> GetArguments(IEnumerable<string> args)
        {
            // setup
            var arguments = new Dictionary<string, string>();

            // build
            foreach (var arg in args)
            {
                var key = Regex.Match(input: arg.Trim(), pattern: "(?<=--)[^:]*").Value;
                var value = Regex.Match(input: arg.Trim(), pattern: "(?<=--.*:).*").Value;

                if (!string.IsNullOrEmpty(key))
                {
                    arguments[key] = value;
                }
            }

            // get
            return arguments;
        } 
    }
}
