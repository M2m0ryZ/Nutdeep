using System.Text;

namespace Nutdeep.Utils.CustomTypes
{
    internal class ObjectSearch
    {
        private bool _isString => _patternString != null;

        private string _patternString { get; set; }
        private byte[] _patternByteArray { get; set; }

        ObjectSearch(string obj)
        {
            _patternString = obj;
        }
        ObjectSearch(byte[] obj)
        {
            _patternByteArray = obj;
        }

        public static implicit operator ObjectSearch(string obj)
            => new ObjectSearch(obj);
        public static implicit operator ObjectSearch(byte[] obj)
           => new ObjectSearch(obj);

        public static implicit operator string(ObjectSearch obj)
            => obj._patternString;
        public static implicit operator byte[] (ObjectSearch obj)
        {
            if (!obj._isString)
                return obj._patternByteArray;

            return Encoding.ASCII.GetBytes(obj._patternString);
        }
    }
}
