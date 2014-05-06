using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
    //Alot of this was reused from STPM
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BLOC
    {
        public const uint Tag = 0x434F4C42;
        public const int Size = 20;
        public uint _tag;
        public bint _count;
        public int unk0;
        public int pad1;

        public BLOC(int count)
        {
            _tag = Tag;
            _count = count;
            unk0 = 0x80;
            pad1 = 0;                 
        }
        public VoidPtr this[int index] { get { return (VoidPtr)((byte*)Address + Offsets(index)); } }
        public uint Offsets(int index) { return *(buint*)((byte*)Address + 0x10 + (index * 4)); }
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BLOCEntry
    {
        //public bushort _id;
        //public byte _id2;
        //public byte _echo;

        //public fixed int _values[64];

        //public BLOCEntry(ushort id, byte echo, byte id2)
        //{
        //    _id = id;
        //    _echo = echo;
        //    _id2 = id2;
        //}

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
}