namespace Nutdeep.Tools
{
    //TODO: change nullable with enums, pls m3n
    public class ScanSettings
    {
        /// <summary>
        /// True: Only scan writable memory
        /// False: Do not scan writable memory
        /// Null: Do not care if it's writable memory or not
        /// </summary>
        public bool? Writable { get; set; } = true;
        /// <summary>
        /// True: Only scan executable memory
        /// False: Do not scan executable memory
        /// Null: Do not care if it's executable memory or not
        /// </summary>
        public bool? Executable { get; set; } = null;
        /// <summary>
        /// True: Only scan copy-on-write memory
        /// False: Do not scan copy-on-write memory
        /// Null: Do not care if it's copy-on-write memory or not
        /// </summary>
        public bool? CopyOnWrite { get; set; } = null;

        /// <summary>
        /// Memory Region Type
        /// If it's true, it will scan memory that's mapped into the view of an image section
        /// </summary>
        public bool MemImage { get; set; } = true;
        /// <summary>
        /// Memory Region Type
        /// If it's true, it will scan memory that's private
        /// </summary>
        public bool MemPrivate { get; set; } = true;
        /// <summary>
        /// Memory Region Type
        /// If it's true, it will scan memory that's mapped into the view of a section
        /// </summary>
        public bool MemMapped { get; set; } = false;

        /// <summary>
        /// Speeds up the scan and decreases useless result by skipping unaligned memory
        /// or only scan addresses ending with specific digits
        /// </summary>
        public bool FastScan { get; set; } = true;
        /// <summary>
        /// Pause the process while scanning
        /// </summary>
        public bool PauseWhileScanning { get; set; } = false;

        public int FastScan_Digit { get; set; } = 1;
        public FastScanType FastScanType { get; set; } = FastScanType.ALIGNMENT;
    }
    
    public enum FastScanType
    {
        ALIGNMENT,
        LAST_DIGIT
    }
}
