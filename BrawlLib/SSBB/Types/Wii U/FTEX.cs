using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FTEX
    {
        public const int Size = 0xC0;
        public const string Tag = "FTEX";

        public BinTag _tag;
        public buint _unk1; //1
        public buint _width;
        public buint _height;

        public fixed uint _unk[40];

        public buint _dataOffset1;
        public buint _dataOffset2; //LOD textures?
        public buint _pad1; //0
        public buint _pad2; //0

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
}
