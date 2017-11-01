using System;

using Nutdeep.Tools;

namespace Nutdeep.Exceptions
{
    public class UnwritableMemoryException : Exception
    {
        public UnwritableMemoryException(IntPtr address, ProcessAccess access) : 
            base($"({access.Process.MainModule.ModuleName}) - The address " +
                $"[{address.ToString("x8").ToUpper()}] is not writable") { }

        public UnwritableMemoryException(string message) : base(message) { }
    }
}
