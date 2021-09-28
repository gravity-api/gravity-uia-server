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
        private const string Configuration = "config";
        private const string Hub = "hub";
        private const string HubPort = "hubPort";
        private const string Host = "host";
        private const string Port = "port";
        private const string Register = "register";
        private const string Tags = "tags";
        private const string BrowserName = "browserName";

        public static void Main(string[] args)
        {
            // cute
            Utilities.RednerLogo();

            // setup
            var arguments = GetArguments(args);

            // register
            if (arguments.ContainsKey(Register) || arguments.ContainsKey(Configuration))
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
            var tags = arguments.ContainsKey(Tags) ? GetTags(arguments[Tags]) : new Dictionary<string, string>();
            var browserName = arguments.ContainsKey(BrowserName) ? arguments[BrowserName] : "UiAutomation";
            var nodeConfiguration = GetNodeConfiguration(port, hubPort, host, browserName, tags);

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

        private static IDictionary<string, string> GetTags(string tags)
        {
            // setup
            var _tags = tags.Split(";");
            var outcome = new Dictionary<string, string>();

            // build
            foreach (var tag in _tags)
            {
                var _tag = tag.Split("=");
                outcome[_tag[0].Trim()] = _tag[1].Trim();
            }

            // get
            return outcome;
        }

        private static object GetNodeConfiguration(int port, int hubPort, string host, string browserName, IDictionary<string, string> tags)
        {
            // setup
            var capabilities = new Dictionary<string, object>
            {
                ["browserName"] = browserName,
                ["browserVersion"] = "1.0",
                ["platform"] = "WINDOWS",
                ["maxInstances"] = 1,
                ["role"] = "WebDriver"
            };
            foreach (var tag in tags)
            {
                capabilities[tag.Key] = tag.Value;
            }

            // get
            return new
            {
                Capabilities = new[]
                {
                    capabilities
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
