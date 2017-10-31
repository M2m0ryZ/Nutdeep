using System;

namespace Nutdeep.Exceptions
{
    public class TypeNotSupportedException : Exception
    {
        public TypeNotSupportedException(Type type) : base($"The type {type.Name} is not supported") { }
        public TypeNotSupportedException(string message) : base(message) { }
    }
}
