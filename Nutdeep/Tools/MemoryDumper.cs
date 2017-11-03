using System;

using Nutdeep.Exceptions;

namespace Nutdeep.Tools
{
    public class MemoryDumper : MemoryHelper
    {
        private ProcessAccess _access { get; set; }

        public MemoryDumper() { }

        public MemoryDumper(ProcessAccess access)
        {
            SetProcess(access);
        }

        public void SetProcess(ProcessAccess access)
        {
            _access = access;
        }

        public T Read<T>(IntPtr address, int byteOrStringLenght = 16)
        {
            var type = typeof(T);

            if (type == typeof(byte[]))
                return (T)(object)GetByteArray(address, byteOrStringLenght);

            try
            {
                return Parse<T>(GetByteArray(address, byteOrStringLenght));
            }
            catch { throw new TypeNotSupportedException(type); }
        }

        private byte[] GetByteArray(IntPtr address, int length = 16)
        {
            ProcessHandler.CheckAccess();

            var buff = new byte[length];

            uint read = 0;
            if (!Pinvoke.ReadProcessMemory(_access.Handle, address, buff,
                (uint)length, ref read) || read != length)
                throw new UnreadableMemoryException(address, _access);

            return buff;
        }
    }
}
