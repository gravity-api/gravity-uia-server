﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            // setup conditions
            var isDirectory = Directory.Exists(fileName);

            // build process
            var startInfo = isDirectory
                ? new ProcessStartInfo { FileName = "explorer.exe", Arguments = fileName }
                : new ProcessStartInfo { FileName = fileName, Arguments = arguments };

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