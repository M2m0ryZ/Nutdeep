using System;
using System.Collections.Generic;
using System.Linq;

namespace Nutdeep.Utils.CustomTypes
{
    public class Signature
    {
        internal int AmountToSubtract = 0;
        internal bool IsWildCard => _pattern.Contains("??");
        internal bool IsUniqueWildCard => IsWildCard && _pattern.Length == 2;

        private string _pattern { get; set; }

        Signature(string pattern)
        {
            _pattern = pattern;
        }

        public static implicit operator Signature(string pattern)
            => new Signature(pattern);
        public static implicit operator string(Signature aobString)
            => aobString.ToString();

        internal byte[] ToBytes()
        {
            if (_pattern.Length < 2)
                throw new FormatException();

            byte[] bytes = new byte[_pattern.Split(' ').Length];
            for (int i = 0, x = 0; i < _pattern.Length; i += 3)
            {
                var hex = _pattern.Substring(i, 2);

                if (hex.Length != 2)
                    throw new FormatException();

                try
                {
                    bytes[x++] = Convert.ToByte(hex, 16);
                }
                catch (Exception e)
                {
                    throw new Exception(e.InnerException.Message);
                }
            }

            return bytes;
        }

        internal byte?[] ToWildCardBytes()
        {
            AmountToSubtract = 0;

            if (!IsWildCard) return null;

            if (_pattern.Length < 2)
                throw new FormatException();

            IList<byte?> bytes = new List<byte?>();
            for (int i = 0; i < _pattern.Length; i += 3)
            {
                var hex = _pattern.Substring(i, 2);

                if (hex.Length != 2)
                    throw new FormatException();

                if (hex == "??")
                {
                    if (bytes.Count != 0)
                    {
                        bytes.Add(null);
                        continue;
                    }
                    AmountToSubtract++;
                    continue;
                }

                try
                {
                    bytes.Add(Convert.ToByte(hex, 16));
                }
                catch (Exception e)
                {
                    throw new Exception(e.InnerException.Message);
                }
            }

            return bytes.ToArray();
        }

        public override string ToString() => _pattern;
    }
}
