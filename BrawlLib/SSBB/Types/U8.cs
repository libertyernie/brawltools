using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using BrawlLib.Wii.Graphics;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct YAZ0
    {
        public const int Size = 0x10;
        public const uint Tag = 0x307A6159;

        public uint _tag;
        public buint _unCompDataLen;
        public fixed int padding[2];

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public VoidPtr Data { get { return Address + Size; } }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct U8
    {
        public const int Size = 0x20;
        public const uint Tag = 0x2D38AA55;

        public uint _tag;
        public buint _entriesOffset;
        public buint _entriesLength;
        public buint _firstOffset;
        
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public U8Entry* Entries { get { return (U8Entry*)(Address + _entriesOffset); } }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct U8Entry
    {
        public const int Size = 0xC;

        public byte _type; //1 == Folder, 0 == Node
        public BUInt24 _stringOffset; //Base is string table
        public buint _dataOffset; //Folder == Parent entry index
        public buint _dataLength; //Folder == Index of first entry that's not a child
        
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public bool isFolder { get { return _type == 1; } }

        //Align string table to 0x20
    }
}