using System;

using Nutdeep.Exceptions;

namespace Nutdeep.Tools
{
    public class MemoryEditor : MemoryHelper
    {
        private ProcessAccess _access { get; set; }

        public MemoryEditor() { }

        public MemoryEditor(ProcessAccess access)
        {
            SetProcess(access);
        }

        public void SetProcess(ProcessAccess access)
        {
            _access = access;
        }

        bool TryWrite(IntPtr address, byte[] buff)
        {
            uint written = 0;
            return (Pinvoke.WriteProcessMemory(_access.Handle, address,
                buff, buff.Length, ref written) && buff.Length == written);
        }

        public void Write<T>(IntPtr address, T obj)
        {
            var type = typeof(T);

            if(type == typeof(byte[]))
                WriteByteArray(address, (byte[])(object)obj);

            try
            {
                WriteByteArray(address, Parse(obj));
            }
            catch { throw new TypeNotSupportedException(type); }
        }

        private void WriteByteArray(IntPtr address, byte[] buff)
        {
            ProcessHandler.CheckAccess();

            if (!TryWrite(address, buff))
                throw new UnwritableMemoryException(address, _access);
        }
    }
}
