/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-keybdinput
 * https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
 */
using System;
using System.Runtime.InteropServices;

namespace UiaDriverServer.Contracts
{
    /// <summary>
    /// Contains information about a simulated keyboard event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        /// <summary>
        /// A virtual-key code. The code must be a value in the range 1 to 254.
        /// If the dwFlags member specifies KEYEVENTF_UNICODE, wVk must be 0.
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes"/>
        public ushort wVk;

        /// <summary>
        /// A hardware scan code for the key.
        /// If dwFlags specifies KEYEVENTF_UNICODE, wScan specifies a Unicode character 
        /// which is to be sent to the foreground application.
        /// </summary>
        public ushort wScan;

        /// <summary>
        /// Specifies various aspects of a keystroke.
        /// </summary>
        public uint dwFlags;

        /// <summary>
        /// The time stamp for the event, in milliseconds.
        /// If this parameter is zero, the system will provide its own time stamp.
        /// </summary>
        public uint time;

        /// <summary>
        /// An additional value associated with the keystroke.
        /// Use the GetMessageExtraInfo function to obtain this information.
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmessageextrainfo"/>
        public IntPtr dwExtraInfo;
    }
}