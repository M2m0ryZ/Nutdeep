namespace Nutdeep.Tools
{
    public class FastScanSettings
    {
        public int Digit { get; set; }
        public FastScanType FastScanType { get; set; }

        public FastScanSettings(int digit = 1, FastScanType fastScanType = FastScanType.ALIGNMENT)
        {
            Digit = digit;
            FastScanType = fastScanType;
        }
    }

    public enum FastScanType
    {
        ALIGNMENT,
        LAST_DIGIT
    }
}
