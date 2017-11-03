namespace Nutdeep.Tools
{
    public class ScanSettings
    {
        /// <summary>
        /// ONLY: Only scan writable memory
        /// NOT: Do not scan writable memory
        /// BOTH: Do not care if it's writable memory or not
        /// </summary>
        public ScanType Writable { get; set; } 
        /// <summary>
        /// ONLY: Only scan executable memory
        /// NOT: Do not scan executable memory
        /// BOTH: Do not care if it's executable memory or not
        /// </summary>
        public ScanType Executable { get; set; } 
        /// <summary>
        /// ONLY: Only scan copy-on-write memory
        /// NOT: Do not scan copy-on-write memory
        /// BOTH: Do not care if it's copy-on-write memory or not
        /// </summary>
        public ScanType CopyOnWrite { get; set; }
        /// <summary>
        /// Memory Region Type
        /// If it's true, it will scan memory that's mapped into the view of an image section
        /// </summary>
        public bool MemImage { get; set; }
        /// <summary>
        /// Memory Region Type
        /// If it's true, it will scan memory that's private
        /// </summary>
        public bool MemPrivate { get; set; }
        /// <summary>
        /// Memory Region Type
        /// If it's true, it will scan memory that's mapped into the view of a section
        /// </summary>
        public bool MemMapped { get; set; }
        /// <summary>
        /// Pause the process while scanning
        /// </summary>
        public bool PauseWhileScanning { get; set; }


        /// <summary>
        /// Speeds up the scan and decreases useless result by skipping unaligned memory
        /// or only scan addresses ending with specific digits
        /// </summary>
        public bool FastScan { get; set; }

        /// <summary>
        /// Set up the settings for fast scan such as digit and types (alignment/last digit)
        /// </summary>
        public FastScanSettings FastScanSettings { get; private set; }

        public ScanSettings(ScanType writable = ScanType.ONLY, ScanType executable = ScanType.BOTH, 
            ScanType copyOnWrite = ScanType.BOTH, bool memImage = true, bool memPrivate = true, 
            bool memMapped = false, bool fastScan = true, FastScanSettings fastScanSetttings = null)
        {
            Writable = writable;
            Executable = executable;
            CopyOnWrite = copyOnWrite;

            MemImage = memImage;
            MemPrivate = memPrivate;
            MemMapped = memMapped;

            FastScan = fastScan;
            FastScanSettings = fastScanSetttings ?? new FastScanSettings();
        }
    }

    public enum ScanType
    {
        ONLY,
        NOT,
        BOTH
    }
}
