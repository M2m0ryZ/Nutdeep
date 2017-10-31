using System;

namespace Nutdeep.Exceptions
{
    public class AccessNotFoundException : Exception
    {
        public AccessNotFoundException() : base("ProcessAccess is null, make sure you gave it") { }
        public AccessNotFoundException(string message) : base(message) { }
    }
}
