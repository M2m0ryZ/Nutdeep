using System;
using System.Runtime.InteropServices;

using Nutdeep.Tools.Flags;

namespace Nutdeep.Tools
{
    internal abstract class Pinvoke
    {
        [DllImport("KERNEL32")]
        internal static extern int CloseHandle(IntPtr handle);
        [DllImport("KERNEL32")]
        internal static extern int ResumeThread(IntPtr handle);
        [DllImport("KERNEL32")]
        internal static extern uint SuspendThread(IntPtr handle);
        [DllImport("KERNEL32")]
        internal static extern bool SetConsoleCtrlHandler(Delegate handler, bool add);
        [DllImport("KERNEL32", SetLastError = true)]
        internal static extern IntPtr OpenThread(ThreadAccess access, bool inheritHandler, uint threadId);
        [DllImport("KERNEL32", SetLastError = true)]
        internal static extern IntPtr OpenProcess(ProcessAccessLevel processAccess, bool inheritHandler, int processId);
        [DllImport("KERNEL32")]
        internal static extern int VirtualQueryEx(IntPtr process,IntPtr address, out MemoryInformation memoryInf, int size);
        [DllImport("KERNEL32", SetLastError = true)]
        internal static extern bool ReadProcessMemory(IntPtr process, IntPtr address, byte[] buffer, uint size, ref uint read);
        [DllImport("KERNEL32", SetLastError = true)]
        internal static extern bool WriteProcessMemory(IntPtr process, IntPtr address, byte[] buffer, int size, ref uint written);
        [DllImport("KERNEL32")]
        internal static extern bool VirtualProtectEx(IntPtr process, IntPtr address, int size, Protection protection, out uint oldProtect);
    }
}
