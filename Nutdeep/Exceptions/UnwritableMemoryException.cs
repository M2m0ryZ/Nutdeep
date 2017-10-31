using System;

namespace Nutdeep.Exceptions
{
    public class UnwritableMemoryException : Exception
    {
        public UnwritableMemoryException(IntPtr address, ProcessAccess access) : 
            base($"({access.Process.MainModule.ModuleName}) - The address " +
                $"[{address.ToString("x8")}] could not be read") { }

        public UnwritableMemoryException(string message) : base(message) { }
    }
}
