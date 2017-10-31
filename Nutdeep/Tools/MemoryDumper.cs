using System;
using System.Text;

using Nutdeep.Exceptions;
using Nutdeep.Utils.Extensions;

/**
 * MemoryDumper - Written by Jeremi Martini (Aka Adversities)
 */
namespace Nutdeep.Tools
{
    public class MemoryDumper
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
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    var bytesToBoolean = GetByteArray(address, sizeof(bool));
                    return (T)(object)BitConverter.ToBoolean(bytesToBoolean, 0);
                case TypeCode.Char:
                    var bytesToChar = GetByteArray(address, sizeof(char));
                    return (T)(object)BitConverter.ToChar(bytesToChar, 0);
                case TypeCode.SByte:
                    var bytesToSByte = GetByteArray(address, sizeof(sbyte));
                    return (T)(object)Convert.ToSByte(bytesToSByte);
                case TypeCode.Byte:
                    var bytesToByte = GetByteArray(address, sizeof(byte));
                    return (T)(object)BitConverter.ToBoolean(bytesToByte, 0);
                case TypeCode.Int16:
                    var bytesToInt16 = GetByteArray(address, sizeof(short));
                    return (T)(object)BitConverter.ToInt16(bytesToInt16, 0);
                case TypeCode.UInt16:
                    var bytesToUInt16 = GetByteArray(address, sizeof(ushort));
                    return (T)(object)BitConverter.ToUInt16(bytesToUInt16, 0);
                case TypeCode.Int32:
                    var bytesToInt32 = GetByteArray(address, sizeof(int));
                    return (T)(object)BitConverter.ToInt32(bytesToInt32, 0);
                case TypeCode.UInt32:
                    var bytesToUInt32 = GetByteArray(address, sizeof(uint));
                    return (T)(object)BitConverter.ToUInt32(bytesToUInt32, 0);
                case TypeCode.Int64:
                    var bytesToInt64 = GetByteArray(address, sizeof(long));
                    return (T)(object)BitConverter.ToInt64(bytesToInt64, 0);
                case TypeCode.UInt64:
                    var bytesToUInt64 = GetByteArray(address, sizeof(ulong));
                    return (T)(object)BitConverter.ToUInt64(bytesToUInt64, 0);
                case TypeCode.Single:
                    var bytesToSingle = GetByteArray(address, sizeof(float));
                    return (T)(object)BitConverter.ToSingle(bytesToSingle, 0);
                case TypeCode.Double:
                    var bytesToDouble = GetByteArray(address, sizeof(double));
                    return (T)(object)BitConverter.ToDouble(bytesToDouble, 0);
                case TypeCode.Decimal:
                    var bytesToDecimal = GetByteArray(address, sizeof(decimal));
                    return (T)(object)bytesToDecimal.ToDecimal();
                case TypeCode.String:
                    var bytesToString = GetByteArray(address, byteOrStringLenght);
                    return (T)(object)Encoding.ASCII.GetString(bytesToString);
                default:
                    if (type == typeof(byte[]))
                        return (T)(object)GetByteArray(address, byteOrStringLenght);
                    else throw new TypeNotSupportedException(type);
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
