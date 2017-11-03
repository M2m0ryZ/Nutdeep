using System;
using System.Diagnostics;

namespace Nutdeep.Tools
{
    public class ProcessAccess
    {
        public IntPtr Handle { get; set; }
        public Process Process { get; set; }
    }
}
