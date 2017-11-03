using System;
using System.Diagnostics;
using System.Collections.Generic;

using Nutdeep;
using Nutdeep.Tools;
using Nutdeep.Utils;
using Nutdeep.Utils.Extensions;
using Nutdeep.Utils.CustomTypes;
using Nutdeep.Utils.EventArguments;

namespace ConsoleExample
{
    class Program : ConsoleHandler
    {
        private ScanSettings _settings;
        private MemoryScanner _scanner;
        private IList<Process> _processesPaused;
        private Program()
        {
            OnClose(Console_OnClose);

            _settings = new ScanSettings()
            {
                Writable = ScanType.ONLY,
                PauseWhileScanning = true
            };
            _scanner = new MemoryScanner();
            _processesPaused = new List<Process>();

            _scanner.SearchResult += Search_Result;
        }

        static void Main(string[] args) => new Program().Run();

        private void Run()
        {
            OpenNotepadHandle();

            Console.ReadLine();
        }

        private void OpenNotepadHandle()
        {
            //Automatically get the shockwave task from Chrome, this way:
            using (var handler = new ProcessHandler("chrome&flash"))
            {
                SetTitle(handler.ToString());

                _scanner.SetAccess(handler);
                _scanner.SetSettings(_settings);

                _scanner.SearchFor<Signature>("00");
            }
        }

        //TODO: Optimizar "PauseWhileScan"
        private void Search_Result(object sender, SearchResultEventArgs args)
        {
            Console.WriteLine(args.ToString());

            MemoryDumper dumper = args.Access;

            for (int i = 0; i < args.Addresses.Length; i++)
            {
                var address = args.Addresses[i];

                Console.Write($"[({i}) - {address.ToString("x8").ToUpper()}]: ");

                foreach (var b in dumper.Read<byte[]>(address, 32))
                    Console.Write($"{b.ToString("x2").ToUpper()} ");

                Console.WriteLine($": {dumper.Read<string>(address, 42)}");
            }
        }

        private void Console_OnClose()
        {
            if (_processesPaused.Count > 0)
                foreach (var process in _processesPaused)
                    process.Resume();
        }

        private void SetTitle(string inf)
        {
            Console.Title = $"Nutdeep - {inf}";
        }
    }
}