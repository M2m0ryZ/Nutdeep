using System;
using System.Linq;
using System.Text;

using Nutdeep.Exceptions;
using Nutdeep.Tools.Flags;

/**
 * MemoryEditor - Written by Jeremi Martini (Aka Adversities)
 */
namespace Nutdeep.Tools
{
    public class MemoryEditor : MemoryHelper
    {
        private IntPtr _handle { get; set; }
        private ProcessAccess _access { get; set; }

        public MemoryEditor() { }

        internal MemoryEditor(ProcessAccess access)
        {
            SetAccess(access);
        }

        public void SetAccess(ProcessAccess access)
        {
            _access = access;
            _handle = _access.Handle;
        }

        bool TryWrite(IntPtr address, byte[] buff)
        {
            uint written = 0;
            return (Pinvoke.WriteProcessMemory(_handle, address,
                buff, buff.Length, ref written) && buff.Length == written);
        }

        public void Write<T>(IntPtr address, T obj)
        {
            try
            {
                ReplaceByteArray(address,Parse(obj));
            }
            catch (Exception)
            {
                throw new NotImplementedException();
            }
        }

        private void ReplaceByteArray(IntPtr address, byte[] buff)
        {
            ProcessAccess.CheckAccess();

            if (!TryWrite(address, buff))
                throw new UnwritableMemoryException(address, _access);
        }
    }
}
