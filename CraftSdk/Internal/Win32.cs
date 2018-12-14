using System.Runtime.InteropServices;

namespace CraftSdk.Internal
{
    internal class Win32
    {
        [DllImport("kernel32.dll")]
        public static extern bool ProcessIdToSessionId(uint dwProcessID, int pSessionID);

        [DllImport("Kernel32.dll", EntryPoint = "WTSGetActiveConsoleSessionId")]
        public static extern int WTSGetActiveConsoleSessionId();
    }
}
