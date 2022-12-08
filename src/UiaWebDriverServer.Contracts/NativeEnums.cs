using System;

namespace UiaWebDriverServer.Contracts
{
    public static class NativeEnums
    {
        public enum SendInputEventType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2,
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
