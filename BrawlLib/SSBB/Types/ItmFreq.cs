using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
    //Alot of this was reused from STPM
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ItmFreqHeader
    {
        public const int Size = 0x20;

        public bint _Length;
        public bint _DataLength;
        public bint _Offsets;
        public int _DataTable;


        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public ItmFreqHeader(int offsets, int length)
        {
            _Length = length;
            _DataLength = length - (offsets*4)+(0x20+0x8)+0xC;
            _Offsets = offsets;
            _DataTable = 1;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ItmFreqEntry
    {
        //public bushort _id;
        //public byte _id2;
        //public byte _echo;

        //public fixed int _values[64];

        //public ItmFreqEntry(ushort id, byte echo, byte id2)
        //{
        //    _id = id;
        //    _echo = echo;
        //    _id2 = id2;
        //}

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
}