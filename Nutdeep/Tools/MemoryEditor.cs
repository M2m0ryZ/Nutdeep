using System;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Nutdeep.Exceptions;
using Nutdeep.Tools.Flags;
using Nutdeep.Utils.Extensions;

/**
 * MemoryEditor - Written by Jeremi Martini (Aka Adversities)
 */
namespace Nutdeep.Tools
{
    public class MemoryEditor : MemoryHelper
    {
        private ProcessAccess _access { get; set; }

        public MemoryEditor() { }

        internal MemoryEditor(ProcessAccess access)
        {
            SetAccess(access);
        }

        public void SetAccess(ProcessAccess access)
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
                ReplaceByteArray(address, (byte[])(object)obj);

            try
            {
                ReplaceByteArray(address, Parse(obj));
            }
            catch { throw new TypeNotSupportedException(type); }
        }

        private void ReplaceByteArray(IntPtr address, byte[] buff)
        {
            ProcessHandler.CheckAccess();

            if (!TryWrite(address, buff))
                throw new UnwritableMemoryException(address, _access);
        }
    }
}
