using System;
using System.Text;

using Nutdeep.Exceptions;
using Nutdeep.Utils.Extensions;

/**
 * MemoryDumper - Written by Jeremi Martini (Aka Adversities)
 */
namespace Nutdeep.Tools
{
    public class MemoryDumper : MemoryHelper
    {
        private IntPtr _handle { get; set; }
        private ProcessAccess _access { get; set; }

        public MemoryDumper() { }

        internal MemoryDumper(ProcessAccess access)
        {
            SetAccess(access);
        }

        public void SetAccess(ProcessAccess access)
        {
            _access = access;
            _handle = _access.Handle;
        }

        public T Read<T>(IntPtr address, int byteOrStringLenght = 16)
        {
            var type = typeof(T);
            try
            {
                return Parse<T>(GetByteArray(address, byteOrStringLenght));
            }
            catch (Exception)
            {
                throw new NotImplementedException();
            }
        }

        public byte[] GetByteArray(IntPtr address, int length = 16)
        {
            ProcessAccess.CheckAccess();

            var buff = new byte[length];

            uint read = 0;
            if (!Pinvoke.ReadProcessMemory(_handle, address, buff,
                (uint)length, ref read) || read != length)
                throw new UnreadableMemoryException(address, _access);

            return buff;
        }
    }
}
