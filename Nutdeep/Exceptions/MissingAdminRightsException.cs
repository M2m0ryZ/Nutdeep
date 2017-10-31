using System;

namespace Nutdeep.Exceptions
{
    public class MissingAdminRightsException : Exception
    {
        public MissingAdminRightsException() : base("Nutdeep needs for administrator permission. " +
            "Please add an app.manifest and setup requestedExecutionLevel to requireAdministrator") { }
        public MissingAdminRightsException(string message) : base(message) { }
    }
}
