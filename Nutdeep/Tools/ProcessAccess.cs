using System;
using System.Diagnostics;

namespace Nutdeep.Tools
{
    public class ProcessAccess
    {
        public IntPtr Handle { get; set; }
        public Process Process { get; set; }

        public static implicit operator MemoryDumper(ProcessAccess access)
        => new MemoryDumper(access);

        public static implicit operator MemoryScanner(ProcessAccess access)
        => new MemoryScanner(access);

        public static implicit operator MemoryEditor(ProcessAccess access)
        => new MemoryEditor(access);
    }
}
