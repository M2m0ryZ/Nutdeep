using System;
using System.Text;

using Nutdeep.Exceptions;
using Nutdeep.Utils.Extensions;

/**
 * MemoryDumper - Written by Jeremi Martini (Aka Adversities)
 * Date: 10/30/2017
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

        public bool GetBoolean(IntPtr address)
        {
            var bytes = GetByteArray(address, sizeof(uint));
            return BitConverter.ToBoolean(bytes, 0);
        }

        public char GetChar(IntPtr address)
        {
            var bytes = GetByteArray(address, sizeof(char));
            return BitConverter.ToChar(bytes, 0);
        }

        public short GetInt16(IntPtr address)
        {
            var bytes = GetByteArray(address, sizeof(short));
            return BitConverter.ToInt16(bytes, 0);
        }

        public ushort GetUInt16(IntPtr address)
        {
            var bytes = GetByteArray(address, sizeof(ushort));
            return BitConverter.ToUInt16(bytes, 0);
        }

        public int GetInt32(IntPtr address)
        {
            var bytes = GetByteArray(address, sizeof(int));
            return BitConverter.ToInt32(bytes, 0);
        }

        public uint GetUInt32(IntPtr address)
        {
            var bytes = GetByteArray(address, sizeof(uint));
            return BitConverter.ToUInt32(bytes, 0);
        }

        public long GetInt64(IntPtr address)
        {
            var bytes = GetByteArray(address, sizeof(long));
            return BitConverter.ToInt64(bytes, 0);
        }

        public ulong GetUInt64(IntPtr address)
        {
            var bytes = GetByteArray(address, sizeof(ulong));
            return BitConverter.ToUInt64(bytes, 0);
        }

        public float GetFloat(IntPtr address)
        {
            var bytes = GetByteArray(address, sizeof(float));
            return BitConverter.ToSingle(bytes, 0);
        }

        public double GetDouble(IntPtr address)
        {
            var bytes = GetByteArray(address, sizeof(double));
            return BitConverter.ToDouble(bytes, 0);
        }

        public decimal GetDecimal(IntPtr address)
        {
            var bytes = GetByteArray(address, sizeof(decimal));
            return bytes.ToDecimal();
        }

        public string GetString(IntPtr address, int length = 16)
        {
            var bytes = GetByteArray(address, length);
            return Encoding.ASCII.GetString(bytes);
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

    public struct MemoryInformation
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public uint RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }
}
