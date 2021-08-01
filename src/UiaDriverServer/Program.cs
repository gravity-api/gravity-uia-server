using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using System;

namespace UiaDriverServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RednerLogo();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(i => i.UseStartup<Startup>());

        private static void RednerLogo()
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