/*
 * CHANGE LOG - keep only last 5 threads
 */
using System;
using System.Diagnostics;
using System.Threading;

namespace UiaDriverServer.Extensions
{
    internal static class ProcessExtensions
    {
        /// <summary>
        /// Wait for <see cref="Process"/> main window handle to be available.
        /// </summary>
        /// <param name="p"><see cref="Process"/> to wait for.</param>
        /// <param name="timeout">Timeout until return false.</param>
        /// <returns>Self reference.</returns>
        public static Process WaitForHandle(this Process p, TimeSpan timeout)
        {
            // setup            
            var timeoutCounter = TimeSpan.Zero;

            // iterate
            while (timeoutCounter < timeout)
            {
                if (p.MainWindowHandle != default)
                {
                    return p;
                }
                Thread.Sleep(100);
                timeoutCounter = timeoutCounter.Add(TimeSpan.FromMilliseconds(100));
            }
            return p;
        }

        public static string GetNameOrFile(this Process p)
        {
            try
            {
                return p.StartInfo.FileName;
            }
            catch (Exception e) when (e != null)
            {
                return p.ProcessName;
            }
        }
    }
}
