using System;
using System.Diagnostics;
using System.Collections.Generic;

using Nutdeep;
using Nutdeep.Tools;
using Nutdeep.Utils;
using Nutdeep.Utils.Extensions;
using Nutdeep.Utils.EventArguments;

namespace ConsoleExample
{
    class Program : ConsoleHandler
    {
        MemoryScanner _scanner;
        IList<Process> _processesPaused;
        Program()
        {
            OnClose(On_Close);

            _processesPaused = new List<Process>();

            _scanner = new MemoryScanner()
            {
                //You have here all the settings as CheatEngine 
                //please feel free to use any of them
                Settings = new ScanSettings()
                {
                    Writable = false //I'm just showing it, it's like this be default
                }
            };

            //This is most for when you are going to scan throw many process
            _scanner.ScanEnds += Scan_Ends;
        }

        static void Main(string[] args) => new Program().Run();

        void Run()
        {
            //ScanAllProcess();
            ScanNotepad();

            Console.ReadLine();
        }

        void ScanNotepad()
        {
            using (var access = new ProcessAccess("notepad"))
            {
                SetTitle(access.ToString());

                MemoryScanner scanner = access;
                scanner.SetSettings(new ScanSettings()
                {
                    Writable = false
                });
                //scanner.SetSettings(ScanSettings); you got this if you want any special shit
                //as i dont care about events for this, i made this local instance

                //if you scan from here, and you enabled 
                //"PauseWhileScanning" please dont forgot to do this
                if (scanner.Settings.PauseWhileScanning)
                    _processesPaused.Add(access.Process);

                SEARCH:
                Console.Write("Gimme a string to search throw notepad: ");

                var str = Console.ReadLine();
                var addresses = scanner.GetAddresses(str);

                if (addresses.Length == 0)
                {
                    Console.WriteLine("Found nothing (Pedo*)");
                    goto SEARCH;
                }

                GIMME:
                Console.Write($"Got {addresses.Length} results, gimme an address index to edit its memory: ");

                if (!int.TryParse(Console.ReadLine(), out int index)) goto GIMME;

                Console.Write($"Gimme a value to write it on {addresses[index].ToString("x8").ToUpper()}: ");

                var value = Console.ReadLine();

                MemoryEditor editor = access;
                editor.Write<string>(addresses[index], value);

                //This is "Next Scan"
                var changed = scanner.GetAddresses(addresses, value);

                Console.WriteLine($"{changed.Length} from the last scan has the new value: {value}");
                Console.Write("Press enter to show them/it...");
                Console.ReadLine();

                PrintMemoryView(changed, access);

                //as all the scans were done without problems (like unexpected exit)
                if (scanner.Settings.PauseWhileScanning)
                    _processesPaused.Remove(access.Process);
            }
        }

        void ScanAllProcess()
        {
            foreach (var process in Process.GetProcesses())
            {
                using (var access = new ProcessAccess(process.Id))
                {
                    if (_scanner.Settings.PauseWhileScanning)
                        _processesPaused.Add(access.Process);

                    _scanner.SetAccess(access);
                    _scanner.GetAddresses("comm");

                    if (_scanner.Settings.PauseWhileScanning)
                        _processesPaused.Remove(access.Process);
                }
            }
        }

        void SetTitle(string inf)
        {
            Console.Title = $"Nutdeep - {inf}";
        }

        void PrintMemoryView(IntPtr[] addresses, MemoryDumper dumper)
        {
            for (int i = 0; i < addresses.Length; i++)
            {
                var address = addresses[i];
                Console.Write($"([{i}] - {address.ToString("x8").ToUpper()}) => ");

                foreach (var b in dumper.GetByteArray(address))
                    Console.Write($"{b.ToString("x2").ToUpper()} ");

                Console.WriteLine($": {dumper.Read<string>(address)}");
            }
        }

        private void On_Close()
        {
            if (_processesPaused.Count > 0)
                foreach (var process in _processesPaused)
                    process.Resume();
        }

        private void Scan_Ends(object sender, ScanEndsEventArgs args)
        {
            Console.WriteLine(args.ToString());
            Console.WriteLine("Press enter to show the results...");
            Console.ReadLine();

            PrintMemoryView(args.Addresses, args.Access);

            Console.WriteLine();
        }
    }
}
