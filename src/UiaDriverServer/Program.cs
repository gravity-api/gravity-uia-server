using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using UiaDriverServer.Extensions;

namespace UiaDriverServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Utilities.RednerLogo();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(i => i.UseStartup<Startup>());
    }
}