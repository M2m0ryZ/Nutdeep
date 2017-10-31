using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Nutdeep.Utils.Extensions
{
    internal static class ObjectExtension
    {
        internal static byte[] ToByteArray(this object obj)
        {
            if (obj == null) return null;
            var binFor = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                binFor.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
