using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct GSND
    {
        public const uint Tag = 0x444E5347;
        public const int Size = 92;
        public uint _tag;
        public bint _count;
        public bint _DataOffset;

        public VoidPtr this[int index] { get { return (VoidPtr)((byte*)Address + Offsets(index)); } }
        public uint Offsets(int index) { return *(buint*)((byte*)Address + 0x08 + (index * 4)); }
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
    public unsafe struct GSNDEntry
    {
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
}