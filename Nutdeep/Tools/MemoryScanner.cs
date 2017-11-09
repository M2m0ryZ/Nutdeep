using System;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
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
    //TODO: try to optimize dude...
    public class MemoryScanner : MemoryHelper
    {
        public ScanSettings Settings { get; private set; }
            = new ScanSettings();

        private FastScanSettings FastScanSettings
            => Settings.FastScanSettings;

        private MemoryDumper _dumper;
        private ProcessAccess _access;

        public event SearchResultEventHandler SearchResult;

        public MemoryScanner() { }

        public MemoryScanner(ProcessAccess access)
        {
            SetProcess(access);
        }

        public void SetProcess(ProcessAccess access)
        {
            _access = access;
            _dumper = new MemoryDumper(access);
        }

        public void SetSettings(ScanSettings settings)
        {
            Settings = settings;
        }

        private int _tasksAmount = 12;
        private MemoryInformation[][] GetRegions()
        {
            var addy = new IntPtr();
            IList<MemoryInformation> regions
                = new List<MemoryInformation>();

            while (true)
            {
                var memInf = new MemoryInformation();
                int memDump = Pinvoke.VirtualQueryEx(_access.Handle, addy,
                    out memInf, Marshal.SizeOf(memInf));

                if (memDump == 0)
                    return DivideRegions(regions);

                var memProps = MemoryProperties.Parse(memInf);

                if (!memProps.State.IsCommited &&
                    !memProps.Protect.IsGuard &&
                    memProps.Protect.IsReadOnly)
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

                if (Settings.Writable != ScanType.BOTH)
                {
                    if (Settings.Writable == ScanType.ONLY)
                    {
                        if (!memProps.Protect.IsWritable)
                            goto ADDY;
                    }
                    else if (memProps.Protect.IsWritable)
                        goto ADDY;
                }
                if (Settings.CopyOnWrite != ScanType.BOTH)
                {
                    if (Settings.CopyOnWrite == ScanType.ONLY)
                    {
                        if (!memProps.Protect.IsCopyOnWrite)
                            goto ADDY;
                    }
                    else if (memProps.Protect.IsCopyOnWrite)
                        goto ADDY;
                }
                if (Settings.Executable != ScanType.BOTH)
                {
                    if (Settings.Executable == ScanType.ONLY)
                    {
                        if (!memProps.Protect.IsExecutable)
                            goto ADDY;
                    }
                    else if (memProps.Protect.IsExecutable)
                        goto ADDY;
                }

                regions.Add(memInf);

                ADDY:
                addy = new IntPtr(memInf.BaseAddress.ToInt32()
                    + (int)memInf.RegionSize);
            }
        }
        private MemoryInformation[][] DivideRegions(IEnumerable<MemoryInformation> regions)
        {
            var regionsArray = regions.ToArray();
            var result = regionsArray.Length / _tasksAmount;
            var rest = regionsArray.Length - (result * _tasksAmount);

            var count = 0;
            IList<MemoryInformation> subList
                = new List<MemoryInformation>();
            IList<MemoryInformation[]> list
                = new List<MemoryInformation[]>();

            for (int i = 0; i < _tasksAmount; i++)
            {
                subList = new List<MemoryInformation>();
                for (int x = 0; x < result; x++)
                    subList.Add(regionsArray[count++]);

                list.Add(subList.ToArray());
            }

            if (rest != 0)
            {
                subList = new List<MemoryInformation>();
                for (int i = 0; i < rest; i++)
                    subList.Add(regionsArray[count++]);
                list.Add(subList.ToArray());
            }

            return list.ToArray();
        }

        private bool FastScanChecker(IntPtr address)
        {
            if (Settings.FastScan)
            {
                if (FastScanSettings.FastScanType == FastScanType.ALIGNMENT)
                {
                    if (((uint)address % FastScanSettings.Digit) != 0)
                        return false;
                }
                else
                {
                    if (((uint)address & 0xF) != FastScanSettings.Digit)
                        return false;
                }
            }

            return true;
        }
        private Task<IEnumerable<IntPtr>> Scan(MemoryInformation[] regions, byte[] pattern, bool caseSensitive = true)
        {
            return Task<IEnumerable<IntPtr>>.Factory.StartNew(() =>
            {
                IList<IntPtr> addresses = new List<IntPtr>();

                foreach (var region in regions)
                {
                    int offSet = 0;
                    byte[] regionByteArray = null;
                    try
                    {
                        regionByteArray = _dumper.Read<byte[]>
                            (region.BaseAddress, (int)region.RegionSize);
                    }
                    catch (UnreadableMemoryException) { continue; }

                    if (!caseSensitive)
                    {
                        var strRegion = Encoding.ASCII.GetString(regionByteArray);
                        regionByteArray = Encoding.ASCII.GetBytes(strRegion.ToLower());

                        var strPattern = Encoding.ASCII.GetString(pattern);
                        pattern = Encoding.ASCII.GetBytes(strPattern.ToLower());
                    }

                    while ((offSet = Array.IndexOf(regionByteArray, pattern[0], offSet)) != -1)
                    {
                        var address = new IntPtr((int)region.BaseAddress + offSet);

                        if (!FastScanChecker(address))
                        {
                            offSet++;
                            continue;
                        }

                        if (pattern.Length > 1)
                        {
                            for (int i = 1; i < pattern.Length; i++)
                            {
                                if (regionByteArray.Length <= offSet + pattern.Length
                                    || pattern[i] != regionByteArray[offSet + i]) break;

                                if (i == pattern.Length - 1)
                                    addresses.Add(address);
                            }
                        }
                        else addresses.Add(address);

                        offSet++;
                    }

                }
                return addresses;
            });
        }
        private Task<IEnumerable<IntPtr>> WildCardScan(MemoryInformation[] regions, Signature aobString)
        {
            return Task<IEnumerable<IntPtr>>.Factory.StartNew(() =>
            {
                IList<IntPtr> addresses = new List<IntPtr>();

                foreach (var region in regions)
                {
                    byte[] regionByteArray;
                    try
                    {
                        regionByteArray = _dumper.Read<byte[]>
                            (region.BaseAddress, (int)region.RegionSize);
                    }
                    catch (UnreadableMemoryException) { continue; }
                    if (aobString.IsUniqueWildCard)
                    {
                        for (int i = 0; i < regionByteArray.Length; i++)
                        {
                            var address = new IntPtr((int)region.BaseAddress + i);

                            if (!FastScanChecker(address))
                                continue;

                            addresses.Add(address);
                        }
                    }
                    else
                    {
                        var offSet = 0;
                        var pattern = aobString.ToWildCardBytes();
                        while ((offSet = Array.IndexOf(regionByteArray, pattern[0], offSet)) != -1)
                        {
                            var address = new IntPtr(((int)region.BaseAddress + offSet)
                                - aobString.AmountToSubtract);

                            if (!FastScanChecker(address) ||
                                offSet < aobString.AmountToSubtract)
                            {
                                offSet++;
                                continue;
                            }

                            if (pattern.Length > 1)
                            {
                                for (int i = 1; i < pattern.Length; i++)
                                {
                                    if (regionByteArray.Length <= offSet + pattern.Length)
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

                                    if (pattern[i] != regionByteArray[offSet + i]) break;

                                    if (i == pattern.Length - 1)
                                        addresses.Add(address);
                                }
                            }
                            else addresses.Add(address);

                            offSet++;
                        }
                    }
                }

                return addresses;
            });
        }

        private Task<IEnumerable<IntPtr>>[] _tasks;
        private IEnumerable<IntPtr> General(Signature aobString)
        {
            string pattern = aobString;

            if (!aobString.IsWildCard)
                return General(aobString.ToBytes());

            ProcessHandler.CheckAccess();

            Stopwatch benchmark = null;
            if (SearchResult != null)
                benchmark = Stopwatch.StartNew();

            if (Settings.PauseWhileScanning)
                _access.Process.Pause();

            IEnumerable<IntPtr> addresses = new IntPtr[] { };

            var regions = GetRegions();
            _tasks = new Task<IEnumerable
                <IntPtr>>[regions.Length];

            for (int i = 0; i < _tasks.Length; i++)
                _tasks[i] = WildCardScan(regions[i], aobString);

            Task.WaitAll(_tasks);

            foreach (var task in _tasks)
                addresses = addresses.Concat(task.Result);

            if (Settings.PauseWhileScanning)
                _access.Process.Resume();

            if (SearchResult != null)
            {
                benchmark.Stop();
                SearchResult.Invoke(this,
                    new SearchResultEventArgs(addresses.ToArray(),
                    benchmark.Elapsed.TotalMilliseconds, _access));
            }

            return addresses;
        }
        private IEnumerable<IntPtr> General(ObjectSearch obj, bool caseSensitive = true)
        {
            ProcessHandler.CheckAccess();

            Stopwatch benchmark = null;
            if (SearchResult != null)
                benchmark = Stopwatch.StartNew();

            if (Settings.PauseWhileScanning)
                _access.Process.Pause();

            IEnumerable<IntPtr> addresses = new IntPtr[] { };

            var regions = GetRegions();
            _tasks = new Task<IEnumerable
                <IntPtr>>[regions.Length];

            for (int i = 0; i < _tasks.Length; i++)
                _tasks[i] = Scan(regions[i], obj, caseSensitive);

            Task.WaitAll(_tasks);

            foreach (var task in _tasks)
                addresses = addresses.Concat(task.Result);

            if (Settings.PauseWhileScanning)
                _access.Process.Resume();

            if (SearchResult != null)
            {
                benchmark.Stop();
                SearchResult.Invoke(this,
                    new SearchResultEventArgs(addresses.ToArray(),
                    benchmark.Elapsed.TotalMilliseconds, _access));
            }

            return addresses;
        }

        private IEnumerable<IntPtr> General(IntPtr[] addresses, byte[] buff, bool caseSensitive = true)
        {
            Collection<IntPtr> nextAddresses
                = new Collection<IntPtr>();

            ProcessHandler.CheckAccess();

            Stopwatch benchmark = null;
            if (SearchResult != null)
                benchmark = Stopwatch.StartNew();

            if (Settings.PauseWhileScanning)
                _access.Process.Pause();

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
                        if (fixedBuff[x] != fixedCurrent[x]) continue;

                    nextAddresses.Add(addresses[i]);
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
                        if (buff[x] != current[x]) continue;

                    nextAddresses.Add(addresses[i]);
                }
            }


            if (Settings.PauseWhileScanning)
                _access.Process.Resume();

            if (SearchResult != null)
            {
                benchmark.Stop();
                SearchResult.Invoke(this,
                    new SearchResultEventArgs(nextAddresses.ToArray(),
                    benchmark.Elapsed.TotalMilliseconds, _access));
            }

            return nextAddresses;
        }
        private IEnumerable<IntPtr> General(IntPtr[] addresses, Signature signature, bool caseSensitive = true)
        {
            Collection<IntPtr> nextAddresses
                = new Collection<IntPtr>();

            ProcessHandler.CheckAccess();

            if (Settings.PauseWhileScanning)
                _access.Process.Pause();

            Stopwatch benchmark = null;
            if (SearchResult != null)
                benchmark = Stopwatch.StartNew();

            if (!signature.IsWildCard)
            {
                var bytes = signature.ToBytes();
                for (int i = 0; i < addresses.Length; i++)
                {
                    var current =
                        _dumper.Read<byte[]>(addresses[i], bytes.Length);

                    if (bytes[0] != current[0]) continue;

                    for (int x = 0; x < bytes.Length; x++)
                    {
                        if (bytes[x] == current[x])
                        {
                            if (x == bytes.Length - 1)
                                nextAddresses.Add(addresses[i]);
                        }
                    }
                }
            }
            else
            {
                var bytes = signature.ToWildCardBytes();

                if (signature.IsUniqueWildCard)
                    return addresses;

                for (int i = 0; i < addresses.Length; i++)
                {
                    var current =
                       _dumper.Read<byte[]>(addresses[i], bytes.Length);

                    for (int x = 0; x < bytes.Length; x++)
                    {
                        if (bytes[x] == null)
                        {
                            if (x == bytes.Length - 1)
                            {
                                nextAddresses.Add(addresses[i]);
                                break;
                            }
                            continue;
                        }

                        if (bytes[x] == current[x])
                        {
                            if (x == bytes.Length - 1)
                                nextAddresses.Add(addresses[i]);
                        }
                    }
                }
            }

            if (SearchResult != null)
            {
                benchmark.Stop();
                SearchResult.Invoke(this,
                    new SearchResultEventArgs(nextAddresses.ToArray(),
                    benchmark.Elapsed.TotalMilliseconds, _access));
            }

            if (Settings.PauseWhileScanning)
                _access.Process.Resume();

            return nextAddresses;
        }

        public IntPtr[] SearchFor<T>(T obj)
        {
            var type = typeof(T);

            if (type == typeof(Signature))
                return General((Signature)(object)obj)
                    .ToArray();
            else if (type == typeof(byte[]))
                return General((byte[])(object)obj)
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

            if (type == typeof(Signature))
                return General(addresses, (Signature)(object)obj)
                    .ToArray();
            else if (type == typeof(byte[]))
                return General((byte[])(object)obj)
                    .ToArray();

            try
            {
                return General(addresses, Parse(obj)).ToArray();
            }
            catch { throw new TypeNotSupportedException(type); }
        }
        public IntPtr[] NextSearchFor(IntPtr[] addresses, string str, bool caseSensitive = true)
        {
            return General(addresses, Encoding.ASCII.GetBytes(str),
                caseSensitive: caseSensitive).ToArray();
        }
    }
}
