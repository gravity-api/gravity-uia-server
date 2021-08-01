/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-input
 */
using System;

namespace UiaDriverServer.Contracts
{
    /// <summary>
    /// The type of the input event.
    /// </summary>
    [Flags]
    public enum InputType
    {
        /// <summary>
        /// The event is a mouse event.
        /// </summary>
        Mouse = 0,

        /// <summary>
        /// The event is a keyboard event.
        /// </summary>
        Keyboard = 1,

        /// <summary>
        /// The event is a hardware event.
        /// </summary>
        Hardware = 2
    }
}