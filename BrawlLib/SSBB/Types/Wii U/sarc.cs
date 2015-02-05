using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using BrawlLib.Wii.Graphics;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SARC
    {
        public const int Size = 0x14;
        public const string Tag = "SARC";
        
        public BinTag _tag;
        public bushort _headerLength;
        public bushort _endian;
        public buint _length;
        public buint _dataOffset;
        public uint _unk1; //1

        public SFAT* Entries { get { return (SFAT*)(Address + _headerLength); } }
        public VoidPtr Data { get { return Address + _dataOffset; } }
        public Endian Endian { get { return (Endian)(short)_endian; } set { _endian = (ushort)value; } }
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SFAT
    {
        public const int Size = 0xC;
        public const string Tag = "SFAT";

        public BinTag _tag;
        public bushort _headerLength; //0xC
        public bushort _entryCount;
        public buint _hashMultiplier; //0x65

        public SFATEntry* Entries { get { return (SFATEntry*)(Address + _headerLength); } }
        public SFNT* Strings { get { return (SFNT*)(Entries + _entryCount); } }
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SFATEntry
    {
        public const int Size = 0x10;

        public buint _hash;
        public byte _unknown; //1
        public BUInt24 _stringOffset; //Multiply by 4, relative to SFNT Data address
        public buint _dataOffset; //Relative to SARC Data address
        public buint _endOffset; //Does not include alignment
        
        public void CalcHash(string name, uint multiplier)
        {
            uint hash = 0;
            for (int i = 0; i < name.Length; i++)
                hash = name[i] + hash * multiplier;
            _hash = hash;
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SFNT
    {
        public const int Size = 8;
        public const string Tag = "SFNT";
        
        public BinTag _tag;
        public bushort _version; //8 - assuming
        public bushort _pad;

        public sbyte* Data { get { return (sbyte*)Address + Size; } }

        public string GetString(uint stringOffset) { return new string(Data + stringOffset * 4); }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
}