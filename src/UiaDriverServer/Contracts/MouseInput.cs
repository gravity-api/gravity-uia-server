/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput
 */
using System;
using System.Runtime.InteropServices;

namespace UiaDriverServer.Contracts
{
    /// <summary>
    /// Contains information about a simulated mouse event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        /// <summary>
        /// The absolute position of the mouse, or the amount of motion since the 
        /// last mouse event was generated, depending on the value of the dwFlags member.
        /// Absolute data is specified as the x coordinate of the mouse; 
        /// relative data is specified as the number of pixels moved.
        /// </summary>
        public int dx;

        /// <summary>
        /// The absolute position of the mouse, or the amount of motion since the 
        /// last mouse event was generated, depending on the value of the dwFlags member.
        /// Absolute data is specified as the y coordinate of the mouse;
        /// relative data is specified as the number of pixels moved.
        /// </summary>
        public int dy;

        /// <summary>
        /// If dwFlags contains MOUSEEVENTF_WHEEL, then mouseData specifies the amount of wheel movement.
        /// </summary>
        public uint mouseData;

        /// <summary>
        /// A set of bit flags that specify various aspects of mouse motion and button clicks.
        /// The bits in this member can be any reasonable combination of the following values.
        /// </summary>
        public uint dwFlags;

        /// <summary>
        /// The time stamp for the event, in milliseconds.
        /// If this parameter is 0, the system will provide its own time stamp.
        /// </summary>
        public uint time;

        /// <summary>
        /// An additional value associated with the mouse event.
        /// An application calls GetMessageExtraInfo to obtain this extra information.
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmessageextrainfo"/>
        public IntPtr dwExtraInfo;
    }
}