using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct EmbeddedEntry
    {
        public const int Size = 8;

        public buint _dataOffset;
        public buint _dataLength; //Not including alignment

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    #region Embedded types

    //Align start to 0x100, align end to 0x20
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FRESEmb1
    {
        public const int Size = 8;

        public bushort _count;
        public byte _unk1; //0xD
        public byte _unk2; //0
        public buint _unk3; //0

        public FRESEmb1Entry* Entries { get { return (FRESEmb1Entry*)(Address + Size); } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FRESEmb1Entry
    {
        public const int Size = 0x20;

        public buint _unk1; //flags? 0x3827 or 0x3826
        public bfloat _unk2;
        public buint _unk3; //0
        public bfloat _unk4; //1
        public buint _unk5; //0
        public byte _unk6; //0x28
        public byte _unk7; //0
        public bushort _unk8; //0x40
        public buint _unk9; //0
        public buint _unk10; //0

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    //Align start to 0x100
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct STB
    {
        public const int Size = 8;
        public const uint Tag = 0x00425453;

        public BinTag _tag;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    #endregion
}
