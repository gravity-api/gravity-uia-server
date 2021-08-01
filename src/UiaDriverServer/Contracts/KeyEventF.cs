/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-keybd_event
 */
using System;

namespace UiaDriverServer.Contracts
{
    /// <summary>
    /// Synthesizes a keystroke.
    /// The system can use such a synthesized keystroke to generate a WM_KEYUP or WM_KEYDOWN message.
    /// The keyboard driver's interrupt handler calls the keybd_event function.
    /// </summary>
    [Flags]
    public enum KeyEventF
    {
        /// <summary>
        /// If specified, the key is being pressed.
        /// </summary>
        KeyDown = 0x0000,

        /// <summary>
        /// If specified, the scan code was preceded by a prefix byte having the value 0xE0 (224).
        /// </summary>
        ExtendedKey = 0x0001,

        /// <summary>
        /// If specified, the key is being released. If not specified, the key is being depressed.
        /// </summary>
        KeyUp = 0x0002,

        Unicode = 0x0004,
        Scancode = 0x0008
    }
}
