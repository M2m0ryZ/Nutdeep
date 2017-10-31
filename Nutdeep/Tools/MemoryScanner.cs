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
using Nutdeep.Utils.EventArguments;
using static Nutdeep.Utils.Delegates.Delegates;

/**
 * MemoryScanner - Written by Jeremi Martini (Aka Adversities)
 * Date: 10/30/2017
 */
namespace Nutdeep.Tools
{
    public class MemoryScanner
    {
        public event ScanEndsEventHandler ScanEnds;

        private IntPtr _handle { get; set; }
        private ProcessAccess _access { get; set; }

        private MemoryDumper _dumper;
        private Collection<MemoryInformation> _memoryRegions;

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
            _handle = _access.Handle;
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
                int memDump = Pinvoke.VirtualQueryEx(_handle, addy,
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

        //This is not organize yet but im on it
        //TODO: Organize this shit adv, wtf...
        private void Scan(IntPtr baseAddress, byte[] region, byte[] pattern,
            ref Collection<IntPtr> addresses, bool caseSensitive = true)
        {
            if (!caseSensitive)
            {
                var regionString = _dumper.GetString(baseAddress,
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

        //This is to allow a wildcards/nonwildcards scan from the same method
        //TODO: Wildcard niggeh
        private IEnumerable<IntPtr> General(byte[] buff = null, string pattern = null, bool caseSensitive = true)
        {
            ProcessAccess.CheckAccess();

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
                    region = _dumper.GetByteArray(current.BaseAddress,
                    (int)current.RegionSize);
                }
                catch (UnreadableMemoryException) { continue; }

                if (buff != null)
                    Scan(current.BaseAddress, region, buff, ref addresses, caseSensitive);
                //else Scan(current.BaseAddress, region, pattern, ref addresses); this is for wildcards
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

            ProcessAccess.CheckAccess();

            if (Settings.PauseWhileScanning)
                _access.Process.Pause();

            Stopwatch benchmark = null;
            if (ScanEnds != null)
                benchmark = Stopwatch.StartNew();

            if (!caseSensitive)
            {
                for (int i = 0; i < addresses.Length; i++)
                {
                    var current = _dumper.GetString(addresses[i], buff.Length).ToLower();
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
                        _dumper.GetByteArray(addresses[i], buff.Length);

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

        public IntPtr[] GetAddressesByInt32(int obj)
        => General(BitConverter.GetBytes(obj)).ToArray();
        public IntPtr[] GetAddressesByInt32(IntPtr[] addresses, int obj)
        => General(addresses, BitConverter.GetBytes(obj)).ToArray();

        public IntPtr[] GetAddressesByUInt32(uint obj)
        => General(BitConverter.GetBytes(obj)).ToArray();
        public IntPtr[] GetAddressesByUInt32(IntPtr[] addresses, uint obj)
        => General(addresses, BitConverter.GetBytes(obj)).ToArray();

        public IntPtr[] GetAddressesByChar(char obj)
        => General(BitConverter.GetBytes(obj)).ToArray();
        public IntPtr[] GetAddressesByChar(IntPtr[] addresses, char obj)
        => General(addresses, BitConverter.GetBytes(obj)).ToArray();

        public IntPtr[] GetAddressesByBoolean(bool obj)
        => General(BitConverter.GetBytes(obj)).ToArray();
        public IntPtr[] GetAddressesByBoolean(IntPtr[] addresses, bool obj)
        => General(addresses, BitConverter.GetBytes(obj)).ToArray();

        public IntPtr[] GetAddressesByInt64(long obj)
        => General(BitConverter.GetBytes(obj)).ToArray();
        public IntPtr[] GetAddressesByInt64(IntPtr[] addresses, long obj)
        => General(addresses, BitConverter.GetBytes(obj)).ToArray();

        public IntPtr[] GetAddressesByUInt64(ulong obj)
        => General(BitConverter.GetBytes(obj)).ToArray();
        public IntPtr[] GetAddressesByUInt64(IntPtr[] addresses, ulong obj)
        => General(addresses, BitConverter.GetBytes(obj)).ToArray();

        public IntPtr[] GetAddressesByFloat(float obj)
        => General(BitConverter.GetBytes(obj)).ToArray();
        public IntPtr[] GetAddressesByFloat(IntPtr[] addresses, float obj)
        => General(addresses, BitConverter.GetBytes(obj)).ToArray();

        public IntPtr[] GetAddressesByInt16(short obj)
        => General(BitConverter.GetBytes(obj)).ToArray();
        public IntPtr[] GetAddressesByInt16(IntPtr[] addresses, short obj)
        => General(addresses, BitConverter.GetBytes(obj)).ToArray();

        public IntPtr[] GetAddressesByUInt16(ushort obj)
        => General(BitConverter.GetBytes(obj)).ToArray();
        public IntPtr[] GetAddressesByUInt16(IntPtr[] addresses, ushort obj)
        => General(addresses, BitConverter.GetBytes(obj)).ToArray();

        public IntPtr[] GetAddressesByDouble(double obj)
        => General(BitConverter.GetBytes(obj)).ToArray();
        public IntPtr[] GetAddressesByDouble(IntPtr[] addresses, double obj)
        => General(addresses, BitConverter.GetBytes(obj)).ToArray();

        public IntPtr[] GetAddressesByDecimal(decimal obj)
        {
            var bytes = decimal.GetBits(obj).SelectMany(
                x => BitConverter.GetBytes(x)).ToArray();

            return General(bytes).ToArray();
        }
        public IntPtr[] GetAddressesByDecimal(IntPtr[] addresses, decimal obj)
        {
            var bytes = decimal.GetBits(obj).SelectMany(
                x => BitConverter.GetBytes(x)).ToArray();

            return General(addresses, bytes).ToArray();
        }

        public IntPtr[] GetAddressesByString(string obj, bool caseSensitive = true)
        => General(Encoding.UTF8.GetBytes(obj), caseSensitive: caseSensitive).ToArray();
        public IntPtr[] GetAddressesByString(IntPtr[] addresses, string obj, bool caseSensitive = true)
        => General(addresses, Encoding.UTF8.GetBytes(obj), caseSensitive).ToArray();

        public IntPtr[] GetAddressesByByteArray(byte[] obj)
        => General(obj).ToArray();
        public IntPtr[] GetAddressesByByteArray(IntPtr[] addresses, byte[] obj)
        => General(addresses, obj).ToArray();

        
    }
}
