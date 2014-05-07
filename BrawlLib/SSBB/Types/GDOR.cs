using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
   [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct GDOR
    {
        public const uint Tag = 0x524F4447;
        public const int Size = 12;
        public uint _tag;
        public bint _count;

        public GDOR(int count)
        {
            _tag = Tag;
            _count = count;
        }
        private GDOR* Address { get { fixed (GDOR* ptr = &this)return ptr; } }
    }
}
