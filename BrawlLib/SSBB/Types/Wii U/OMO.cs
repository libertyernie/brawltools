using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OMOHeader
    {
        public const int Size = 0x20;
        public static readonly string Tag = "OMO ";
        
        public BinTag _tag;
        public bushort _versionMin; //1
        public bushort _versionMax; //3
        public buint _unknown; //0x091B152E
        public buint _boneCount;
        public bushort _frameCount;
        public bushort _frameSize;
        public buint _boneTableOffset;
        public buint _fixedDataOffset;
        public buint _frameDataOffset;

        public OMOHeader(int frameCount, int boneCount)
        {
            _tag = Tag;
            _versionMin = 1;
            _versionMax = 3;
            _unknown = 0x091B152E;
            _boneCount = (uint)boneCount;
            _frameCount = (ushort)frameCount;
            _frameSize = 0;
            _boneTableOffset = 0x20;
            _fixedDataOffset = _boneTableOffset + _boneCount * OMOBoneEntry.Size;
            _frameDataOffset = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OMOBoneEntry
    {
        public const int Size = 0x10;

        //0000 0100 0000 0000 0000 0000 0000 0000   has scale
        //0000 0010 0000 0000 0000 0000 0000 0000   has rot
        //0000 0001 0000 0000 0000 0000 0000 0000   has trans
        //0000 0000 0010 0000 0000 0000 0000 0000   trans const
        //0000 0000 0000 1000 0000 0000 0000 0000   trans interp
        //0000 0000 0000 0000 0111 0000 0000 0000   rot const
        //0000 0000 0000 0000 0101 0000 0000 0000   rot interp
        //0000 0000 0000 0000 0000 0010 0000 0000   scale const
        //0000 0000 0000 0000 0000 0000 1000 0000   scale interp

        //02087081
        //0000 0010 0000 1000 0111 0000 1000 0001

        public buint _flags;
        public buint _boneId; //matches id in vbn (2 bytes after bone parent index in vbn)
        public buint _offsetInFixedData;
        public buint _offsetInFrame;
    }
}
