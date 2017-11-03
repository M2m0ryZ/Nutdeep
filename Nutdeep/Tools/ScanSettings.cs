﻿namespace Nutdeep.Tools
{
    public class ScanSettings
    {
        
        //LUULL a constructor what an improvment

        public ScanSettings(ScanType writableMemory = ScanType.BOTH,ScanType executableMemory = ScanType.BOTH, ScanType copyOnWriteMemory = ScanType.BOTH)
        {
            Writable = writableMemory;
            Executable = executableMemory;
            CopyOnWrite = copyOnWriteMemory;
        }

        public ScanSettings()
        {
            
        }
        
        
        /// <summary>
        /// ONLY: Only scan writable memory
        /// NOT: Do not scan writable memory
        /// BOTH: Do not care if it's writable memory or not
        /// </summary>
        public ScanType Writable { get; set; } = ScanType.ONLY;
        /// <summary>
        /// ONLY: Only scan executable memory
        /// NOT: Do not scan executable memory
        /// BOTH: Do not care if it's executable memory or not
        /// </summary>
        public ScanType Executable { get; set; } = ScanType.BOTH;
        /// <summary>
        /// ONLY: Only scan copy-on-write memory
        /// NOT: Do not scan copy-on-write memory
        /// BOTH: Do not care if it's copy-on-write memory or not
        /// </summary>
        public ScanType CopyOnWrite { get; set; } = ScanType.BOTH;
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

    public enum ScanType
    {
        ONLY,
        NOT,
        BOTH
    }
}
