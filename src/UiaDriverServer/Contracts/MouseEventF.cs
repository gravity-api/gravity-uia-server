/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-mouse_event
 * https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput
 */
using System;

namespace UiaDriverServer.Contracts
{
    /// <summary>
    /// The mouse_event function synthesizes mouse motion and button clicks.
    /// </summary>
    /// <remarks>This function has been superseded. Use SendInput instead.</remarks>
    [Flags]
    public enum MouseEventF
    {
        /// <summary>
        /// The dx and dy parameters contain normalized absolute coordinates.
        /// </summary>
        Absolute = 0x8000,

        /// <summary>
        /// The wheel button is tilted.
        /// </summary>
        HWheel = 0x01000,

        /// <summary>
        /// Movement occurred.
        /// </summary>
        Move = 0x0001,

        /// <summary>
        /// The left button is down.
        /// </summary>
        LeftDown = 0x0002,

        /// <summary>
        /// The left button is up.
        /// </summary>
        LeftUp = 0x0004,

        /// <summary>
        /// The right button is down.
        /// </summary>
        RightDown = 0x0008,

        /// <summary>
        /// The right button is up.
        /// </summary>
        RightUp = 0x0010,

        /// <summary>
        /// The middle button is down.
        /// </summary>
        MiddleDown = 0x0020,

        /// <summary>
        /// The middle button is up.
        /// </summary>
        MiddleUp = 0x0040,

        /// <summary>
        /// The wheel has been moved, if the mouse has a wheel. The amount of movement is specified in dwData
        /// </summary>
        Wheel = 0x0800,

        /// <summary>
        /// An X button was pressed.
        /// </summary>
        XDown = 0x0080,

        /// <summary>
        /// An X button was released.
        /// </summary>
        XUp = 0x0100,

        /// <summary>
        /// Maps coordinates to the entire desktop. Must be used with MOUSEEVENTF_ABSOLUTE.
        /// </summary>
        VirtualDesk = 0x4000,

        /// <summary>
        /// The WM_MOUSEMOVE messages will not be coalesced. The default behavior is to coalesce WM_MOUSEMOVE messages.
        /// </summary>
        /// <remarks>Windows XP/2000: This value is not supported.</remarks>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousemove"/>
        MoveNoCoalesce = 0x2000,
    }
}