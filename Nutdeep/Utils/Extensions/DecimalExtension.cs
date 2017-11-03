using System;
using System.Linq;

namespace Nutdeep.Utils.Extensions
{
    public static class DecimalExtension
    {
        public static byte[] ToByteArray(this decimal n)
            => decimal.GetBits(n).SelectMany(x =>
            BitConverter.GetBytes(x)).ToArray();
    }
}
