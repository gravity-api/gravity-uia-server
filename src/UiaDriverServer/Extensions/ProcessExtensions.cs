using System;
using System.Diagnostics;
using System.Threading;

namespace UiaDriverServer.Extensions
{
    internal static class ProcessExtensions
    {
        /// <summary>
        /// wait for process main window handle to be available
        /// </summary>
        /// <param name="p">process to wait</param>
        /// <param name="timeout">timeout until return false</param>
        /// <returns>update process state</returns>
        public static Process WaitForHandle(this Process p, TimeSpan timeout)
        {
            // setup            
            var timeoutCounter = TimeSpan.Zero;

            // iterate
            while (timeoutCounter < timeout)
            {
                if(p.MainWindowHandle != default(IntPtr))
                {
                    return p;
                }
                Thread.Sleep(100);
                timeoutCounter = timeoutCounter.Add(TimeSpan.FromMilliseconds(100));
            }
            return p;
        }
    }
}