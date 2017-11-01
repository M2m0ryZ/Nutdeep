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
        private ScanSettings _settings;
        private MemoryScanner _scanner;
        private IList<Process> _processesPaused;
        private Program()
        {
            OnClose(Console_OnClose);

            _settings = new ScanSettings()
            {
                Writable = true
            };
            _scanner = new MemoryScanner();
            _processesPaused = new List<Process>();

            _scanner.ScanEnds += Scan_Ends;
        }

        static void Main(string[] args) => new Program().Run();

        private void Run()
        {
            OpenNotepadHandle();

            Console.ReadLine();
        }

        private void OpenNotepadHandle()
        {
            //If you wanna get the flash player task from chrome, use &flash
            using (var handler = new ProcessHandler("chrome&flash"))
            {
                SetTitle(handler.ToString());

                _scanner.SetAccess(handler);
                _scanner.SetSettings(_settings);

                _scanner.SearchFor("flashplayer");
            }
        }

        private void Scan_Ends(object sender, ScanEndsEventArgs args)
        {
            Console.WriteLine(args.ToString());

            for (int i = 0; i < args.Addresses.Length; i++)
            {
                var address = args.Addresses[i];

                Console.WriteLine($"[{i}] {address.ToString("x8").ToUpper()}");
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