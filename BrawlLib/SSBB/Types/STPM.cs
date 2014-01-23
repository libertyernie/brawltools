using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct STPM
    {
        public const uint Tag = 0x4D505453;
        public const int Size = 20;

        public uint _tag;
        public bint _count;
        public int pad0;
        public int pad1;

        public STPM(int count)
        {
            _tag = Tag;
            _count = count;
            pad0 = pad1 = 0;
        }

        public VoidPtr this[int index] { get { return (VoidPtr)((byte*)Address + Offsets(index)); } }
        public uint Offsets(int index) { return *(buint*)((byte*)Address + 0x10 + (index * 4)); }
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct STPMEntry
    {
        public bushort _id;
        public byte _id2;
        public byte _echo;
        
        public fixed int _values[64];

        public STPMEntry(ushort id, byte echo, byte id2)
        {
            _id = id;
            _echo = echo;
            _id2 = id2;
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
}