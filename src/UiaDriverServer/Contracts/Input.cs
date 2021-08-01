/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-hardwareinput
 */
namespace UiaDriverServer.Contracts
{
    public struct Input
    {
        public int type;
        public InputUnion u;
    }
}
