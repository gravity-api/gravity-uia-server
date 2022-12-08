using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UiaWebDriverServer.Contracts
{
    public static class NativeStructs
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Input
        {
            public NativeEnums.SendInputEventType type;
            public MouseInput mouseInput;
            public KeyInput keyInput;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public NativeEnums.MouseEvent dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public struct KeyInput
        {
            public ushort wVk;
            public ushort wScan;
            public NativeEnums.KeyEvent dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}
