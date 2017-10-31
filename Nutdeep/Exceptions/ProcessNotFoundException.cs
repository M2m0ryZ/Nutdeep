using System;

namespace Nutdeep.Exceptions
{
    public class ProcessNotFoundException : Exception
    {
        public ProcessNotFoundException() : base("We didn't find a process, please check it out") { }
        public ProcessNotFoundException(string message) : base(message) { }
    }
}
