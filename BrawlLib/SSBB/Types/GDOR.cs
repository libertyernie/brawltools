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
        public bint _DataOffset;

        //private GDOR* Address { get { fixed (GDOR* ptr = &this)return ptr; } }
        //public byte* Data { get { return (byte*)(Address + _DataOffset); } }

        public VoidPtr this[int index] { get { return (VoidPtr)((byte*)Address + Offsets(index)); } }
        public uint Offsets(int index) { return *(buint*)((byte*)Address + 0x08 + (index * 4)); }
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
   [StructLayout(LayoutKind.Sequential, Pack = 1)]
   public unsafe struct GDOREntry
   {
       private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
   }
}
