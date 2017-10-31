using System;

namespace Nutdeep.Exceptions
{
    public class UnreadableMemoryException : Exception
    {
        public UnreadableMemoryException(IntPtr address, ProcessAccess access) : 
            base($"({access.Process.MainModule.ModuleName}) - The address " +
                $"[{address.ToString("x8").ToUpper()}] could not be read") { }

        public UnreadableMemoryException(string message) : base(message) { }
    }
}
