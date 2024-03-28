/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-input
 */
using System;

namespace UiaWebDriverServer.Contracts
{
    public static class NativeEnums
    {
        /// <summary>
        /// The type of the input event.
        /// </summary>
        [Flags]
        public enum SendInputEventType
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

        [Flags]
        public enum MouseEvent : uint
        {
            None = 0x0000,
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            Absolute = 0x8000,
        }

        [Flags]
        public enum KeyEvent : uint
        {
            KeyDown = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            Scancode = 0x0008
        }
    }
}
