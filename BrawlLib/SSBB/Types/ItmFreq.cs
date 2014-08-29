using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ItmFreqHeader
    {
        public const int Size = 0x20;

        public bint _Length;
        public bint _DataLength;
        public bint _OffCount;
        public bint _DataTable;
        public int _pad0;
        public int _pad1;
        public int _pad2;
        public int _pad3;


        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public ItmFreqHeader(int offCount, int length)
        {
            _Length = length;
            _DataLength = length - (offCount*4)+(0x20+0x8)+0xC;
            _OffCount = offCount;
            _DataTable = 1;
            _pad0 = _pad1 =
            _pad2 = _pad3 = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ItmFreqEntry
    {
        public const int Size = 0x10;

        public bint _ID;
        public bint _subItem;
        public bfloat _frequency;
        public short _action;
        public short _subaction;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ItmFreqTableList
    {
        public const int Size = 0x28;

        public bint _table1;
        public bint _t1_count;
        public bint _table2;
        public bint _t2_count;
        public bint _table3;
        public bint _t3_count;
        public bint _table4;
        public bint _t4_count;
        public bint _table5;
        public bint _t5_count;


        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ItmFreqGroupNode
    {
        public const int Size = 0x14;

        public bint _unknown0;
        public bint _unknown1;
        public bint _unknown2;
        public bint _entryOffset;
        public bint _entryCount;
        public bint _unknown3;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ItmFreqOffPair
    {
        public const int Size = 0x08;

        public bint _offset;
        public bint _count;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
}