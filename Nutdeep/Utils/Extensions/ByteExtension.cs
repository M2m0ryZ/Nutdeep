using System;

namespace Nutdeep.Utils.Extensions
{
    public static class ByteExtension
    {
        public static bool EqualsIgnoreCase(this byte byteA, byte byteB)
        {
            var result = byteA - byteB;
            return (result == 32 || result == -32 || result == 0);
        }

        public static char ToLowerChar(this byte b)
        {
            var c = Convert.ToChar(b);
            return char.ToLower(c); 
        }
    }
}
