/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-hardwareinput
 */
using System.Runtime.InteropServices;

namespace UiaDriverServer.Contracts
{
    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)]
        public MouseInput mi;
        
        [FieldOffset(0)]
        public KeyboardInput ki;
        
        [FieldOffset(0)]
        public HardwareInput hi;
    }
}
