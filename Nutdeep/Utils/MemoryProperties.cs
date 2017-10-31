using Nutdeep.Tools;
using Nutdeep.Tools.Flags;

namespace Nutdeep.Utils
{
    internal class MemoryProperties
    {
        internal MType Type { get; set; }
        internal MState State { get; set; }
        internal MProtect Protect { get; set; }

        internal static MemoryProperties Parse(MemoryInformation inf)
        {
            return new MemoryProperties()
            {
                State = new MState()
                {
                    IsFree = (inf.State & (uint)MemoryState.Free) != 0,
                    IsCommited = (inf.State & (uint)MemoryState.Commit) != 0,
                    IsReserved = (inf.State & (uint)MemoryState.Reserve) != 0
                },
                Type = new MType()
                {
                    IsNone = (inf.Type & (uint)MemoryType.None) != 0,
                    IsMapped = (inf.Type & (uint)MemoryType.Mapped) != 0,
                    IsPrivate = (inf.Type & (uint)MemoryType.Private) != 0,
                    IsImageMapped = (inf.Type & (uint)MemoryType.Image) != 0
                },
                Protect = new MProtect()
                {
                    IsGuard = (inf.Protect & (uint)MemoryProtection.Guard) != 0,
                    NoCache = (inf.Protect & (uint)MemoryProtection.NoCache) != 0,
                    HasAccess = (inf.Protect & (uint)MemoryProtection.NoAccess) == 0,
                    IsReadOnly = (inf.Protect & (uint)MemoryProtection.ReadOnly) != 0,
                    IsWritable = (inf.Protect & (uint)MemoryProtection.Writable) != 0,
                    IsReadWrite = (inf.Protect & (uint)MemoryProtection.ReadWrite) != 0,
                    IsZeroAcces = (inf.Protect & (uint)MemoryProtection.ZeroAccess) != 0,
                    IsCopyOnWrite = (inf.Protect & (uint)MemoryProtection.WriteCopy) != 0,
                    IsExecutable = (inf.Protect & (uint)MemoryProtection.Executable) != 0,
                    WriteCombine = (inf.Protect & (uint)MemoryProtection.WriteCombine) != 0
                }
            };
        }
    }

    internal class MProtect
    {
        internal bool IsGuard { get; set; }
        internal bool NoCache { get; set; }
        internal bool HasAccess { get; set; }
        internal bool IsReadOnly { get; set; }
        internal bool IsWritable { get; set; }
        internal bool IsReadWrite { get; set; }
        internal bool IsZeroAcces { get; set; }
        internal bool WriteCombine { get; set; }
        internal bool IsExecutable { get; set; }
        internal bool IsCopyOnWrite { get; set; }
    }

    internal class MState
    {
        internal bool IsFree { get; set; }
        internal bool IsCommited { get; set; }
        internal bool IsReserved { get; set; }
    }

    internal class MType
    {
        internal bool IsNone { get; set; }
        internal bool IsMapped { get; set; }
        internal bool IsPrivate { get; set; }
        internal bool IsImageMapped { get; set; }
    }
}
