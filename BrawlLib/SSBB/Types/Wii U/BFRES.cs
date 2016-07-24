using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BFRESHeader
    {
        public const string Tag = "FRES";
        public const int Size = 16;

        public BinTag _tag;
        public byte _versionMajor; //2, 3
        public byte _versionMinor; //0, 2, 3, 4, 5
        public bushort _flags; //0, 1, 2, 4, 6
        public bushort _endian;
        public bushort _rootOffset; //0x10
        public buint _fileSize; //Total size of resource package file

        public buint _dataAlign; //0x2000 or 0x1000 usually
        public buint _stringOffset;
        public buint _unk5;
        public buint _stringTableOffset; //Offset to entries of 8 bytes each

        public buint _offsetGroupMDL;
        public buint _offsetGroupTEX;
        public buint _offsetGroupSKA;
        public buint _offsetGroupSHU1;
        public buint _offsetGroupSHU2;
        public buint _offsetGroupSHU3;
        public buint _offsetGroupTXP;
        public buint _offsetGroupUNK8;
        public buint _offsetGroupVIS;
        public buint _offsetGroupSHA;
        public buint _offsetGroupSCN;
        public buint _offsetGroupEMB;

        public bushort _countGroupMDL;
        public bushort _countGroupTEX;
        public bushort _countGroupSKA;
        public bushort _countGroupSHU1;
        public bushort _countGroupSHU2;
        public bushort _countGroupSHU3;
        public bushort _countGroupTXP;
        public bushort _countGroupUNK8;
        public bushort _countGroupVIS;
        public bushort _countGroupSHA;
        public bushort _countGroupSCN;
        public bushort _countGroupEMB;
        
        public buint _pad; //0

        public buint* Offsets { get { return (buint*)_offsetGroupMDL.Address; } }
        public bushort* Counts { get { return (bushort*)_countGroupMDL.Address; } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public Endian Endian { get { return (Endian)(short)_endian; } set { _endian = (ushort)value; } }
    }
}
