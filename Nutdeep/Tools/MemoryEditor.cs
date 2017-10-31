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
            return (Pinvoke.WriteProcessMemory(_handle, address,
                buff, buff.Length, ref written) && buff.Length == written);
        }

        public void Write<T>(IntPtr address, T obj)
        {
            var type = typeof(T);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    ReplaceByteArray(address, BitConverter.GetBytes(
                         (bool)(object)obj));
                    break;
                case TypeCode.Char:
                    ReplaceByteArray(address, BitConverter.GetBytes(
                        (char)(object)obj));
                    break;
                case TypeCode.SByte:
                    ReplaceByteArray(address, BitConverter.GetBytes(
                        (sbyte)(object)obj));
                    break;
                case TypeCode.Byte:
                    ReplaceByteArray(address, BitConverter.GetBytes(
                        (byte)(object)obj));
                    break;
                case TypeCode.Int16:
                    ReplaceByteArray(address, BitConverter.GetBytes(
                        (short)(object)obj));
                    break;
                case TypeCode.UInt16:
                    ReplaceByteArray(address, BitConverter.GetBytes(
                        (ushort)(object)obj));
                    break;
                case TypeCode.Int32:
                    ReplaceByteArray(address, BitConverter.GetBytes(
                        (int)(object)obj));
                    break;
                case TypeCode.UInt32:
                    ReplaceByteArray(address, BitConverter.GetBytes(
                        (uint)(object)obj));
                    break;
                case TypeCode.Int64:
                    ReplaceByteArray(address, BitConverter.GetBytes(
                        (long)(object)obj));
                    break;
                case TypeCode.UInt64:
                    ReplaceByteArray(address, BitConverter.GetBytes(
                        (ulong)(object)obj));
                    break;
                case TypeCode.Single:
                    ReplaceByteArray(address, BitConverter.GetBytes(
                        (float)(object)obj));
                    break;
                case TypeCode.Double:
                    ReplaceByteArray(address, BitConverter.GetBytes(
                        (double)(object)obj));
                    break;
                case TypeCode.Decimal:
                    ReplaceByteArray(address, decimal.GetBits((decimal)(object)obj)
                        .SelectMany(x => BitConverter.GetBytes(x)).ToArray());
                    break;
                case TypeCode.String:
                    ReplaceByteArray(address, Encoding.UTF8.GetBytes(
                        (string)(object)obj));
                    break;
                default:
                    if (type == typeof(byte[]))
                        ReplaceByteArray(address, (byte[])(object)obj);
                    else throw new TypeNotSupportedException(type);
                    break;
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
