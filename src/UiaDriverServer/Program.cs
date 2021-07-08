using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UiaDriverServer.Components;

namespace UiaDriverServer
{
    internal static class Program
    {
        // cancellation token setup
        private static CancellationTokenSource Source { get; } = new CancellationTokenSource();

        [STAThread]
        internal static void Main(string[] args)
        {
            // register cancellation event
            Console.CancelKeyPress += Console_CancelKeyPress;

            // register unexpected exception
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // run services container
            var a = Regex.Split(string.Join(" ", args), "--").Where(i => !string.IsNullOrEmpty(i));
            var p = a.FirstOrDefault(i => i.Contains("port="));
            p = p ?? "port=4444";
            var port = Regex.Match(p, @"(?<=port=)\d+").Value;
            int.TryParse(port, out int portOut);
            Run(portOut);
        }

        // graceful shutdown
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CloseApplication();
        }

        // graceful shutdown
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            CloseApplication();
        }

        // graceful shutdown
        private static void CloseApplication()
        {
            Trace.TraceInformation("shutting down application, please wait...");

            // cancel async operation
            Source?.Cancel();
            Trace.TraceInformation("async operation was canceled successfully");
            Environment.Exit(0);
        }

        // run services hosts (each service on a new thread with shared cancellation)
        private static void Run(int port)
        {
            // start OwinWebApi host
            OwinHost(port).Start();

            // stop main thread until cancel
            Thread.Sleep(Timeout.Infinite);
        }

        // owin-host instance (web-api REST endpoint)
        private static Task OwinHost(int port) => new Task(() 
            => new OwinFactory().Create(port).Open(), Source.Token);
    }
}