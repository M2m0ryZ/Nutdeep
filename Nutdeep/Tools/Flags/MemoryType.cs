using System;

namespace Nutdeep.Tools.Flags
{
    [Flags]
    internal enum MemoryType
    {
        /// <summary>
        /// Non-official flag that is present when the page does not fall into another category
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Indicates that the memory pages within the region are mapped into the view of an image section
        /// </summary>
        Image = 0x1000000,

        /// <summary>
        /// Indicates that the memory pages within the region are mapped into the view of a section
        /// </summary>
        Mapped = 0x40000,

        /// <summary>
        /// Indicates that the memory pages within the region are private (that is, not shared by other processes)
        /// </summary>
        Private = 0x20000
    }
}
