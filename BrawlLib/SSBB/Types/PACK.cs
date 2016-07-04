using System;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct PACKHeader
    {
        public const string Tag = "KCAP"; //PACK in little endian
        public const int Size = 0x10;

        public BinTag _tag;
        public buint _pad1;
        public buint _fileCount;
        public buint _pad2;

        public bint* StringOffsets { get { return (bint*)(Address + Size); } }
        public bint* DataOffsets { get { return (bint*)(Address + Size + _fileCount * 4); } }
        public bint* Lengths { get { return (bint*)(Address + Size + _fileCount * 8); } }

        private VoidPtr Address { get { fixed (void* p = &this)return p; } }
    }
}
