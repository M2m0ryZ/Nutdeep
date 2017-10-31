using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using Nutdeep.Exceptions;

/**
 * MemoryEditor - Written by Jeremi Martini (Aka Adversities)
 * Date: 10/30/2017
 */
namespace Nutdeep.Tools
{
    public class MemoryEditor
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
            return ((Pinvoke.VirtualProtectEx(_handle, address,
                buff.Length, Protection.PEReadWrite,
                out uint old)) && Pinvoke.WriteProcessMemory(_handle, address,
                buff, buff.Length, ref written) && buff.Length == written);
        }

        public void ReplaceBoolean(IntPtr address, bool obj)
        {
            ReplaceByteArray(address, BitConverter.GetBytes(obj));
        }
        public void ReplaceChar(IntPtr address, char obj)
        {
            ReplaceByteArray(address, BitConverter.GetBytes(obj));
        }
        public void ReplaceInt16(IntPtr address, short obj)
        {
            ReplaceByteArray(address, BitConverter.GetBytes(obj));
        }
        public void ReplaceUInt16(IntPtr address, ushort obj)
        {
            ReplaceByteArray(address, BitConverter.GetBytes(obj));
        }
        public void ReplaceInt32(IntPtr address, int obj)
        {
            ReplaceByteArray(address, BitConverter.GetBytes(obj));
        }
        public void ReplaceUInt32(IntPtr address, uint obj)
        {
            ReplaceByteArray(address, BitConverter.GetBytes(obj));
        }
        public void ReplaceInt64(IntPtr address, long obj)
        {
            ReplaceByteArray(address, BitConverter.GetBytes(obj));
        }
        public void ReplaceUInt64(IntPtr address, ulong obj)
        {
            ReplaceByteArray(address, BitConverter.GetBytes(obj));
        }
        public void ReplaceSingle(IntPtr address, float obj)
        {
            ReplaceByteArray(address, BitConverter.GetBytes(obj));
        }
        public void ReplaceDouble(IntPtr address, double obj)
        {
            ReplaceByteArray(address, BitConverter.GetBytes(obj));
        }
        public void ReplaceDecimal(IntPtr address, decimal replacement)
        {
            var bytes = decimal.GetBits(replacement)
                .SelectMany(x => BitConverter.GetBytes(x))
                .ToArray();

            ReplaceByteArray(address, bytes);
        }
        public void ReplaceString(IntPtr address, string obj)
        {
            ReplaceByteArray(address, Encoding.UTF8.GetBytes(obj));
        }
        public void ReplaceByteArray(IntPtr address, byte[] buff)
        {
            ProcessAccess.CheckAccess();

            if (!TryWrite(address, buff))
                throw new UnwritableMemoryException(address, _access);
        }

        
    }

    [Flags]
    public enum Protection
    {
        PEReadWrite = 0x40,
        PReadWrite = 0x04
    }
}
