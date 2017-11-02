using System;
using System.Globalization;

using Nutdeep.Tools;

namespace Nutdeep.Utils.EventArguments
{
    public class SearchResultEventArgs : EventArgs
    {
        public IntPtr[] Addresses { get; }
        public double Milliseconds { get; }
        public ProcessAccess Access { get; }

        internal SearchResultEventArgs(IntPtr[] addresses, double milliseconds, ProcessAccess access)
        {
            Access = access;
            Addresses = addresses;
            Milliseconds = milliseconds;
        }

        public override string ToString()
        {
            if (Addresses.Length != 0)
                return $"{Addresses.Length.ToString("#,#", CultureInfo.InvariantCulture)} results -" +
                    $" {Access.Process.Id.ToString("x8").ToUpper()}-" +
                    $"{Access.Process.MainModule.ModuleName} " +
                    $"({Milliseconds.ToString("0.000 ms")})";
            else
                return $"we've not got results -" +
                    $" {Access.Process.Id.ToString("x8").ToUpper()}-" +
                    $"{Access.Process.MainModule.ModuleName} " +
                    $"({Milliseconds.ToString("0.000 ms")})";
        }
    }
}
