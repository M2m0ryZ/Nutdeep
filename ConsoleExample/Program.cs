using System;
using System.Diagnostics;
using System.Collections.Generic;

using Nutdeep;
using Nutdeep.Tools;
using Nutdeep.Utils;
using Nutdeep.Utils.Extensions;
using Nutdeep.Utils.EventArguments;
using Nutdeep.Utils.CustomTypes;

namespace ConsoleExample
{
    //Should i leave something simpler or is this fine?
    class Program : ConsoleHandler
    {
        ScanSettings settings;
        MemoryScanner _scanner;
        IList<Process> _processesPaused;
        Program()
        {
            OnClose(Console_OnClose);

            settings = new ScanSettings()
            {
                Writable = null
            };
            _scanner = new MemoryScanner();
            _processesPaused = new List<Process>();

            _scanner.ScanEnds += Scan_Ends;
        }

        static void Main(string[] args) => new Program().Run();

        private void Run()
        {
            ScanAllProcess();
            OpenNotepadHandle();

            Console.ReadLine();
        }



        private void OpenNotepadHandle()
        {
            using (var handler = new ProcessHandler("notepad"))
            {
                SetTitle(handler.ToString());

                var addresses = ScanInNotepad(handler);
                var writed = WriteInNotepad(handler, addresses);
                var newAddresses = NextScanInNotepad(handler, addresses, writed);

                PrintMemoryView(newAddresses, handler);
            }
        }

        private IntPtr[] ScanInNotepad(ProcessHandler access)
        {
            MemoryDumper dumper = access;

            _scanner.SetSettings(new ScanSettings()
            {
                Writable = true
            });

            CheckProcessPaused(access.Process);

            SEARCH:
            Console.Write("Gimme a string to search throw notepad: ");

            var str = Console.ReadLine();
            var addresses = _scanner.SearchFor(str);

            //oh btw you can perform a wildcards scarns doing the following:
            if (false) //Ignore this if, it's just to show you guys how wildcards work...
                _scanner.SearchFor<Signature>("?? 00 ?? 00 00 00 ?? 0A");

                Console.Beep();

            if (addresses.Length == 0)
            {
                Console.WriteLine("Found nothing (Pedo*)");
                goto SEARCH;
            }

            //as all the scans were done without problems (like unexpected exit)
            CheckProcessPaused(access.Process, true);

            return addresses;
        }

        private string WriteInNotepad(MemoryEditor editor, IntPtr[] addresses)
        {
            Console.WriteLine($"We got {addresses.Length} results");

            GIMME:
            Console.Write("Gimme an index to edit an address: ");
            if (!int.TryParse(Console.ReadLine(), out int index)) goto GIMME;

            Console.Write($"Gimme a value to write it on {addresses[index].ToString("x8").ToUpper()}: ");
            var value = Console.ReadLine();

            editor.Write(addresses[index], value);

            return value;
        }

        private IntPtr[] NextScanInNotepad(ProcessHandler handler, IntPtr[] addresses, string nextValue)
        {
            CheckProcessPaused(handler.Process);

            MemoryScanner scanner = handler;

            var changed = scanner.NextSearchFor(addresses, nextValue);

            Console.WriteLine($"{changed.Length} from the last scan has the new value: {nextValue}");
            Console.Write("Press enter to show them/it...");
            Console.ReadLine();

            CheckProcessPaused(handler.Process, true);

            return changed;
        }


        private void SetTitle(string inf)
        {
            Console.Title = $"Nutdeep - {inf}";
        }

        private void ScanAllProcess()
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    using (var access = new ProcessHandler(process.Id))
                    {
                        SetTitle(access.ToString());

                        _scanner.SetAccess(access);

                        if (_scanner.Settings.PauseWhileScanning)
                            _processesPaused.Add(access.Process);

                        _scanner.SearchFor("comm");

                        if (_scanner.Settings.PauseWhileScanning)
                            _processesPaused.Remove(access.Process);
                    }
                }
                catch { }
            }
        }

        private void Console_OnClose()
        {
            if (_processesPaused.Count > 0)
                foreach (var process in _processesPaused)
                    process.Resume();
        }

        private void Scan_Ends(object sender, ScanEndsEventArgs args)
        {
            Console.WriteLine(args.ToString());
            Console.Beep();
            Console.WriteLine("Press enter to show the results...");
            Console.ReadLine();

            PrintMemoryView(args.Addresses, args.Access);

            Console.WriteLine();
        }

        private void PrintMemoryView(IntPtr[] addresses, MemoryDumper dumper)
        {
            for (int i = 0; i < addresses.Length; i++)
            {
                var address = addresses[i];
                Console.Write($"([{i}] - {address.ToString("x8").ToUpper()}) => ");

                foreach (var b in dumper.Read<byte[]>(address))
                    Console.Write($"{b.ToString("x2").ToUpper()} ");

                Console.WriteLine($": {dumper.Read<string>(address, 32)}");
            }
        }

        private void CheckProcessPaused(Process process, bool done = false)
        {
            //if (!_isPauseWhileScanning) return;

            if (!done) _processesPaused.Add(process);
            else _processesPaused.Remove(process);
        }
    }
}