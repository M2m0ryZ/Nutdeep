using System;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;

using Nutdeep.Utils;
using Nutdeep.Exceptions;
using Nutdeep.Utils.Extensions;
using Nutdeep.Utils.CustomTypes;
using Nutdeep.Utils.EventArguments;
using static Nutdeep.Utils.Delegates.Delegates;

namespace Nutdeep.Tools
{
    //TODO: multi-thread scan
    public class MemoryScanner : MemoryHelper
    {
        private ProcessAccess _access { get; set; }

        private MemoryDumper _dumper;
        private Collection<MemoryInformation> _memoryRegions;

        public event ScanEndsEventHandler ScanEnds;

        public ScanSettings Settings { get; set; } = new ScanSettings();

        public MemoryScanner(ProcessAccess access)
        {
            SetAccess(access);
            _memoryRegions = new Collection<MemoryInformation>();
        }

        public MemoryScanner()
        {
            _memoryRegions = new Collection<MemoryInformation>();
        }

        public void SetAccess(ProcessAccess access)
        {
            _access = access;
            _dumper = access;
        }

        public void SetSettings(ScanSettings settings)
        {
            Settings = settings;
        }

        private void GetRegions()
        {
            var addy = new IntPtr();

            while (true)
            {
                var memInf = new MemoryInformation();
                int memDump = Pinvoke.VirtualQueryEx(_access.Handle, addy,
                    out memInf, Marshal.SizeOf(memInf));

                if (memDump == 0) return;

                var memProps = MemoryProperties.Parse(memInf);

                if (!memProps.State.IsCommited &&
                    !memProps.Protect.IsGuard)
                    goto ADDY;

                if (!Settings.MemPrivate)
                {
                    if (memProps.Type.IsPrivate)
                        goto ADDY;
                }

                if (!Settings.MemImage)
                {
                    if (memProps.Type.IsImageMapped)
                        goto ADDY;
                }
                if (!Settings.MemMapped)
                {
                    if (memProps.Type.IsMapped)
                        goto ADDY;
                }

                if (Settings.Writable != null)
                {
                    if ((bool)Settings.Writable)
                    {
                        if (!memProps.Protect.IsWritable)
                            goto ADDY;
                    }
                    else if (memProps.Protect.IsWritable)
                        goto ADDY;
                }
                if (Settings.CopyOnWrite != null)
                {
                    if ((bool)Settings.CopyOnWrite)
                    {
                        if (!memProps.Protect.IsCopyOnWrite)
                            goto ADDY;
                    }
                    else if (memProps.Protect.IsCopyOnWrite)
                        goto ADDY;
                }
                if (Settings.Executable != null)
                {
                    if ((bool)Settings.Executable)
                    {
                        if (!memProps.Protect.IsExecutable)
                            goto ADDY;
                    }
                    else if (memProps.Protect.IsExecutable)
                        goto ADDY;
                }

                _memoryRegions.Add(memInf);

                ADDY:
                addy = new IntPtr(memInf.BaseAddress.ToInt32()
                    + (int)memInf.RegionSize);
            }
        }

        //TODO: Step the scan
        private void Scan(IntPtr baseAddress, byte[] region, byte[] pattern,
            ref Collection<IntPtr> addresses, bool caseSensitive = true)
        {
            if (!caseSensitive)
            {
                var regionString = _dumper.Read<string>(baseAddress,
                    region.Length).ToLower();
                var regionFixed = Encoding.ASCII.GetBytes(regionString);

                var patternString = Encoding.ASCII.GetString(pattern).ToLower();
                var patternFixed = Encoding.ASCII.GetBytes(patternString);

                int offSet = 0;
                while ((offSet = Array.IndexOf(regionFixed, patternFixed[0], offSet)) != -1)
                {
                    var address = new IntPtr((int)baseAddress + offSet);

                    if (Settings.FastScan)
                    {
                        if (Settings.FastScanType == FastScanType.ALIGNMENT)
                        {
                            if (((uint)address % Settings.FastScan_Digit) != 0)
                                goto CONTINUE;
                        }
                        else
                        {
                            if (((uint)address & 0xf) != Settings.FastScan_Digit)
                                goto CONTINUE;
                        }
                    }


                    if (pattern.Length > 1)
                    {
                        for (int i = 1; i < pattern.Length; i++)
                        {
                            if (region.Length <= offSet + pattern.Length
                                || patternFixed[i] != regionFixed[offSet + i]) break;

                            if (i == pattern.Length - 1)
                                addresses.Add(address);
                        }
                    }
                    else addresses.Add(address);

                    CONTINUE:
                    offSet++;
                }

            }
            else
            {
                int offSet = 0;
                while ((offSet = Array.IndexOf(region, pattern[0], offSet)) != -1)
                {
                    var address = new IntPtr((int)baseAddress + offSet);

                    if (Settings.FastScan)
                    {
                        if (Settings.FastScanType == FastScanType.ALIGNMENT)
                        {
                            if (((uint)address % Settings.FastScan_Digit) != 0)
                                goto CONTINUE;
                        }
                        else
                        {
                            if (((uint)address & 0xf) != Settings.FastScan_Digit)
                                goto CONTINUE;
                        }
                    }


                    if (pattern.Length > 1)
                    {
                        for (int i = 1; i < pattern.Length; i++)
                        {
                            if (region.Length <= offSet + pattern.Length
                                || pattern[i] != region[offSet + i]) break;

                            if (i == pattern.Length - 1)
                                addresses.Add(address);
                        }
                    }
                    else addresses.Add(address);

                    CONTINUE:
                    offSet++;
                }
            }
        }

        private void WildCardScan(IntPtr baseAddress, byte[] region, Signature aobString,
            ref Collection<IntPtr> addresses)
        {
            if (aobString.IsUniqueWildCard)
            {
                for (int i = 0; i < region.Length; i++)
                {
                    var address = new IntPtr((int)baseAddress + i);

                    if (Settings.FastScan)
                    {
                        if (Settings.FastScanType == FastScanType.ALIGNMENT)
                        {
                            if (((uint)address % Settings.FastScan_Digit) != 0)
                                continue;
                        }
                        else
                        {
                            if (((uint)address & 0xf) != Settings.FastScan_Digit)
                                continue;
                        }
                    }
                    addresses.Add(address);
                }
            }
            else
            {
                var offSet = 0;
                var pattern = aobString.ToWildCardBytes();
                while ((offSet = Array.IndexOf(region, pattern[0], offSet)) != -1)
                {
                    var address = new IntPtr(((int)baseAddress + offSet) - aobString.AmountToSubtract);

                    if (Settings.FastScan)
                    {
                        if (Settings.FastScanType == FastScanType.ALIGNMENT)
                        {
                            if (((uint)address % Settings.FastScan_Digit) != 0)
                                goto CONTINUE;
                        }
                        else
                        {
                            if (((uint)address & 0xF) != Settings.FastScan_Digit)
                                goto CONTINUE;
                        }
                    }

                    if (offSet < aobString.AmountToSubtract)
                        goto CONTINUE;

                    if (pattern.Length > 1)
                    {
                        for (int i = 1; i < pattern.Length; i++)
                        {
                            if (region.Length <= offSet + pattern.Length)
                                break;

                            if (pattern[i] == null)
                            {
                                if (i == pattern.Length - 1)
                                {
                                    addresses.Add(address);
                                    break;
                                }
                                continue;
                            }

                            if (pattern[i] != region[offSet + i]) break;

                            if (i == pattern.Length - 1)
                                addresses.Add(address);
                        }
                    }
                    else addresses.Add(address);

                    CONTINUE: offSet++;
                }
            }
        }

        private IEnumerable<IntPtr> General(Signature aobString)
        {
            string pattern = aobString;

            if (!aobString.IsWildCard)
                return General(aobString.ToBytes());

            ProcessHandler.CheckAccess();

            if (Settings.PauseWhileScanning)
                _access.Process.Pause();

            Stopwatch benchmark = null;
            if (ScanEnds != null)
                benchmark = Stopwatch.StartNew();

            GetRegions();

            var addresses = new Collection<IntPtr>();
            var memoryRegions = _memoryRegions.ToArray();

            for (int i = 0; i < memoryRegions.Length; i++)
            {
                byte[] region = null;
                var current = _memoryRegions[i];

                try
                {
                    region = _dumper.Read<byte[]>(current.BaseAddress,
                        (int)current.RegionSize);
                }
                catch (UnreadableMemoryException) { continue; }

                WildCardScan(current.BaseAddress, region, aobString, ref addresses);
            }

            if (ScanEnds != null)
            {
                benchmark.Stop();
                ScanEnds.Invoke(this,
                    new ScanEndsEventArgs(addresses.ToArray(),
                    benchmark.Elapsed.TotalMilliseconds, _access));
            }

            if (Settings.PauseWhileScanning)
                _access.Process.Resume();

            return addresses;
        }
        private IEnumerable<IntPtr> General(IntPtr[] addresses, byte[] buff, bool caseSensitive = true)
        {
            Collection<IntPtr> nextAddresses
                = new Collection<IntPtr>();

            ProcessHandler.CheckAccess();

            if (Settings.PauseWhileScanning)
                _access.Process.Pause();

            Stopwatch benchmark = null;
            if (ScanEnds != null)
                benchmark = Stopwatch.StartNew();

            if (!caseSensitive)
            {
                for (int i = 0; i < addresses.Length; i++)
                {
                    var current = _dumper.Read<string>(addresses[i], buff.Length).ToLower();
                    var fixedCurrent = Encoding.ASCII.GetBytes(current);

                    var buffStr = Encoding.ASCII.GetString(buff).ToLower();
                    var fixedBuff = Encoding.ASCII.GetBytes(buffStr);

                    if (fixedBuff[0] != fixedCurrent[0]) continue;

                    for (int x = 1; x < fixedCurrent.Length; x++)
                        if (fixedBuff[x] != fixedCurrent[x]) goto NEXT;

                    nextAddresses.Add(addresses[i]);
                    NEXT:;
                }
            }
            else
            {
                for (int i = 0; i < addresses.Length; i++)
                {
                    var current =
                        _dumper.Read<byte[]>(addresses[i], buff.Length);

                    if (buff[0] != current[0]) continue;

                    for (int x = 1; x < current.Length; x++)
                        if (buff[x] != current[x]) goto NEXT;

                    nextAddresses.Add(addresses[i]);
                    NEXT:;
                }
            }


            if (ScanEnds != null)
            {
                benchmark.Stop();
                ScanEnds.Invoke(this,
                    new ScanEndsEventArgs(nextAddresses.ToArray(),
                    benchmark.Elapsed.TotalMilliseconds, _access));
            }

            if (Settings.PauseWhileScanning)
                _access.Process.Resume();

            return nextAddresses;
        }
        private IEnumerable<IntPtr> General(ObjectSearch obj, bool caseSensitive = true)
        {
            ProcessHandler.CheckAccess();

            if (Settings.PauseWhileScanning)
                _access.Process.Pause();

            Stopwatch benchmark = null;
            if (ScanEnds != null)
                benchmark = Stopwatch.StartNew();

            GetRegions();

            var addresses = new Collection<IntPtr>();
            var memoryRegions = _memoryRegions.ToArray();

            for (int i = 0; i < memoryRegions.Length; i++)
            {
                byte[] region = null;
                var current = _memoryRegions[i];

                try
                {
                    region = _dumper.Read<byte[]>(current.BaseAddress,
                        (int)current.RegionSize);
                }
                catch (UnreadableMemoryException) { continue; }

                Scan(current.BaseAddress, region, obj, ref addresses, caseSensitive);
            }

            if (ScanEnds != null)
            {
                benchmark.Stop();
                ScanEnds.Invoke(this,
                    new ScanEndsEventArgs(addresses.ToArray(),
                    benchmark.Elapsed.TotalMilliseconds, _access));
            }

            if (Settings.PauseWhileScanning)
                _access.Process.Resume();

            return addresses;
        }

        public IntPtr[] SearchFor<T>(T obj)
        {
            var type = typeof(T);

            if (type == typeof(Signature))
                return General((Signature)(object)obj)
                    .ToArray();

            try
            {
                return General(Parse(obj)).ToArray();
            }
            catch { throw new TypeNotSupportedException(type); }
        }
        public IntPtr[] SearchFor(string str, bool caseSensitive = true)
        {
            return General(Encoding.ASCII.GetBytes(str), caseSensitive:
                caseSensitive).ToArray();
        }

        public IntPtr[] NextSearchFor<T>(IntPtr[] addresses, T obj)
        {
            var type = typeof(T);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return General(addresses, BitConverter.GetBytes(
                         (bool)(object)obj)).ToArray();
                case TypeCode.Char:
                    return General(addresses, BitConverter.GetBytes(
                        (char)(object)obj)).ToArray();
                case TypeCode.SByte:
                    return General(addresses, BitConverter.GetBytes(
                        (sbyte)(object)obj)).ToArray();
                case TypeCode.Byte:
                    return General(addresses, BitConverter.GetBytes(
                        (byte)(object)obj)).ToArray();
                case TypeCode.Int16:
                    return General(addresses, BitConverter.GetBytes(
                        (short)(object)obj)).ToArray();
                case TypeCode.UInt16:
                    return General(addresses, BitConverter.GetBytes(
                        (ushort)(object)obj)).ToArray();
                case TypeCode.Int32:
                    return General(addresses, BitConverter.GetBytes(
                        (int)(object)obj)).ToArray();
                case TypeCode.UInt32:
                    return General(addresses, BitConverter.GetBytes(
                        (uint)(object)obj)).ToArray();
                case TypeCode.Int64:
                    return General(addresses, BitConverter.GetBytes(
                        (long)(object)obj)).ToArray();
                case TypeCode.UInt64:
                    return General(addresses, BitConverter.GetBytes(
                        (ulong)(object)obj)).ToArray();
                case TypeCode.Single:
                    return General(addresses, BitConverter.GetBytes(
                        (float)(object)obj)).ToArray();
                case TypeCode.Double:
                    return General(addresses, BitConverter.GetBytes(
                        (double)(object)obj)).ToArray();
                case TypeCode.Decimal:
                    var bytes = decimal.GetBits(
                        (decimal)(object)obj).SelectMany(
                        x => BitConverter.GetBytes(x)).ToArray();
                    return General(addresses, bytes).ToArray();
                case TypeCode.String:
                    return General(addresses, Encoding.ASCII.GetBytes(
                        (string)(object)obj)).ToArray();
                default:
                    if (type == typeof(byte[]))
                        return General(addresses, (byte[])(object)obj).ToArray();
                    else if (type == typeof(Signature))
                        return General((Signature)(object)obj).ToArray();
                    else throw new TypeNotSupportedException(type);
            }
        }
        public IntPtr[] NextSearchFor(IntPtr[] addresses, string str, bool caseSensitive = true)
        {
            return General(addresses, Encoding.ASCII.GetBytes(str),
                caseSensitive: caseSensitive).ToArray();
        }
    }
}
