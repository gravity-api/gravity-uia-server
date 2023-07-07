using System;

namespace UiaWebDriverServer.Extensions
{
    internal static class ControllerUtilities
    {
        /// <summary>
        /// Render UiA Driver logo.
        /// </summary>
        public static void RednerLogo()
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
