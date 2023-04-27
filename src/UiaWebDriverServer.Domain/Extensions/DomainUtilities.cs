using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading;

using UiaWebDriverServer.Contracts;

namespace UiaWebDriverServer.Domain.Extensions
{
    public static class DomainUtilities
    {
        /// <summary>
        /// start an interactive process.
        /// </summary>
        /// <param name="fileName">The application or document to start.</param>
        /// <param name="arguments">The set of command-line arguments to use when starting the application.</param>
        /// <returns>A new instance of the <see cref="Process"/> class.</returns>
        public static Process StartProcess(string fileName, string arguments)
        {
            return StartProcess(impersonation: default, fileName, arguments);
        }

        /// <summary>
        /// start an interactive process.
        /// </summary>
        /// <param name="fileName">The application or document to start.</param>
        /// <param name="arguments">The set of command-line arguments to use when starting the application.</param>
        /// <returns>A new instance of the <see cref="Process"/> class.</returns>
        public static Process StartProcess(string fileName, string arguments, ImpersonationModel impersonation)
        {
            return StartProcess(impersonation, fileName, arguments);
        }

        // TODO: refactor
        private static Process StartProcess(ImpersonationModel impersonation, string fileName, string arguments)
        {
            // setup conditions
            var isDirectory = Directory.Exists(fileName);

            // is directory (opens files explorer)
            var startInfo = isDirectory
                ? new ProcessStartInfo { FileName = "explorer.exe", Arguments = fileName, WindowStyle = ProcessWindowStyle.Maximized }
                : new ProcessStartInfo { FileName = fileName, Arguments = arguments, WindowStyle = ProcessWindowStyle.Maximized };

            // impersonate
            if (impersonation?.Enabled == true)
            {
                var password = new SecureString();
                foreach (var character in impersonation.Password)
                {
                    password.AppendChar(character);
                }
                var iProcess = new Process();
                iProcess.StartInfo.UseShellExecute = false;
                iProcess.StartInfo.FileName = fileName;
                iProcess.StartInfo.Arguments = arguments;
                iProcess.StartInfo.Domain = impersonation.Domain;
                iProcess.StartInfo.Password = password;
                iProcess.StartInfo.UserName = impersonation.Username;
                iProcess.Start();
                Thread.Sleep(3000);
                return iProcess;
            }

            var process = new Process
            {
                StartInfo = startInfo
            };
            process.Start();
            Thread.Sleep(3000);
            return process;
        }
    }
}
