using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using System.Net;

using UiaDriverServer.Extensions;

namespace UiaDriverServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Utilities.RednerLogo();
            CreateWebHostBuilder(args).Build().Run();
        }

        // creates web service host container
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .UseUrls()
            .ConfigureKestrel(o => o.Listen(IPAddress.Any, 4444))
            .UseStartup<Startup>();
    }
}