using System;

namespace Nutdeep.Tools.Flags
{
    [Flags]
    public enum ControlType
    {
        C = 0,
        BREAK,
        CLOSE,
        LOGOFF = 5,
        SHUTDOWN
    }
}
