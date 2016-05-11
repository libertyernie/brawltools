using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct OMOHeader
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

        public VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct OMOBoneEntry
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
        //0000 0000 0000 0000 0000 0000 0000 0001   enabled (always on?)

        //02087081
        //0000 0010 0000 1000 0111 0000 1000 0001

        public Bin32 _flags;
        public buint _boneHash; //matches id in vbn (2 bytes after bone parent index in vbn)
        public buint _offsetInFixedData;
        public buint _offsetInFrame;

        public VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct VBNHeader
    {
        public const int Size = 0x10;
        public static readonly string Tag = "VBN ";

        public BinTag _tag;
        public bushort _versionMin;
        public bushort _versionMax;
        public buint _boneCount;
        public buint _boneCount2Unk;

        public VBNFrameHeader* FrameData { get { return (VBNFrameHeader*)(Address + Size + _boneCount * VBNBone.Size); } }

        public VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct VBNBone
    {
        public const int Size = 0x4C;

        public const uint RootID = 0x0FFFFFFF;

        public buint _unk1; //0
        public buint _parentIndex;
        public buint _hash;

        public string GetString() { return new String((sbyte*)Address + 12); }

        public VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct VBNFrameHeader
    {
        public const int Size = 0xC;

        public buint _unk1; //0, 3
        public buint _unk2; //count?
        public buint _unk3; //flags? hash?

        public VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct VBNBoneFrame
    {
        public const int Size = 0x24;

        public BVec3 _translation;
        public BVec3 _rotation;
        public BVec3 _scale;

        public VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }
}
