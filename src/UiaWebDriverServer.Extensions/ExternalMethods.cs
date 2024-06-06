using System;
using System.Runtime.InteropServices;

using UIAutomationClient;

using static UiaWebDriverServer.Contracts.NativeStructs;
/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
namespace UiaWebDriverServer.Extensions
{
    internal static class ExternalMethods
    {
        // constants
        internal const int MouseEventLeftDown = 0x02;
        internal const int MouseEventLeftUp = 0x04;

        [DllImport("user32.dll")]
        internal static extern bool SetProcessDpiAwarenessContext(int value);

        [DllImport("user32.dll")]
        internal static extern bool EnumDisplaySettings(string? deviceName, int modeNum, ref DevMode devMode);

        [DllImport("gdi32.dll")]
        internal static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        internal static extern IntPtr GetPhysicalCursorPos(out tagPOINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        internal static extern bool SetPhysicalCursorPos(int x, int y);

        // native calls: obsolete
        [Obsolete("This function has been superseded. Use SendInput instead.")]
        [DllImport("user32.dll")]
        internal static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [StructLayout(LayoutKind.Sequential)]
        internal struct DevMode
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }
    }
}