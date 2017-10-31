using System;
using System.Linq;

namespace Nutdeep.Utils.Extensions
{
    internal static class ByteArrayExtension
    {
        internal static decimal ToDecimal(this byte[] bytes)
        {
            if (bytes.Count() != 16)
                throw new Exception("A decimal must be created from exactly 16 bytes");

            Int32[] bits = new Int32[4];

            for (int i = 0; i <= 15; i += 4)
                bits[i / 4] = BitConverter.ToInt32(bytes, i);

            return new decimal(bits);
        }
    }
}
