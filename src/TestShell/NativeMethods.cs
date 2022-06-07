using System.Runtime.InteropServices;

namespace TestShell
{
    public static class NativeMethods
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();
    }
}