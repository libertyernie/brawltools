using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PhysicsHeader
    {
        public const uint Tag1 = 0x57E0E057;
        public const uint Tag2 = 0x10C0C010;

        public uint _tag1; //0x57E0E057
        public uint _tag2; //0x10C0C010
        public bint _unk2; //0
        public bint _unk3; //4

        public byte _unk4; //4
        public bshort _unk5; //1
        public byte _unk6; //1
        public bint _unk7; //3
        public bint _unk8; //1
        public bint _unk9; //0

        public bint _unk10; //0
        public buint _unk11; //Size/Offset

        public fixed byte _name[0x18]; 

        //Three sections of offsets:
        //__classnames__
        //__data__
        //__types__

        public PhysicsOffsetSection ClassNames;
        public PhysicsOffsetSection Data;
        public PhysicsOffsetSection Types;

        private VoidPtr Address { get { fixed (void* p = &this)return p; } }

        public String Name { get { return new String((sbyte*)Address + 0x28); } }

        public VoidPtr ClassNamesData { get { return Address + *(buint*)(Address + 0x54); } }
        public VoidPtr DataData { get { return Address + *(buint*)(Address + 0x84); } }
        public VoidPtr TypesData { get { return Address + *(buint*)(Address + 0xB4); } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PhysicsOffsetSection
    {
        public fixed byte name[0x10];

        public bint _unk0; //-1
        public buint _dataOffset; //Main header is the base

        //Offsets to indices struct. _dataOffset is the base for everything.
        public buint _dataOffset1;
        public buint _dataOffset2;
        public buint _dataOffset3;
        public buint _dataOffset4;
        public buint _dataOffset5;
        public buint _dataLength;

        //When offsets begin to repeat, stop reading
        //Indices count is ((next data offset) - (current data offset)) / 2
        //Indices are padded to 0x10 with 0xFF

        public String Name { get { return new String((sbyte*)Address); } }

        public VoidPtr Address { get { fixed (void* p = &this)return p; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PhysicsClassName
    {
        public buint _unk1;
        public byte _unk2; //9
        public String _value { get { return new String((sbyte*)Address + 5); } }

        private VoidPtr Address { get { fixed (void* p = &this)return p; } }

        //Align string table to 0x10 with 0xFF
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PhysicsIndexGroup
    {
        //Base is the offset sections's _dataOffset

        public buint _dataOffset;
        public buint _stringOffset;

        private VoidPtr Address { get { fixed (void* p = &this)return p; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PhysicsVector
    {
        public bint _x;
        public bint _y;
        public bint _z;
        
        private VoidPtr Address { get { fixed (void* p = &this)return p; } }
    }
}
