using BrawlLib.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FMDL
    {
        public const int Size = 0x30;
        public const string Tag = "FMDL";

        public BinTag _tag;
        public buint _stringOffset;
        public buint _unk1Offset;
        public buint _skeletonOffset;
        public buint _offsetVTX;
        public buint _offsetGroupSHP;
        public buint _offsetGroupMAT;
        public buint _offsetGroupUniforms;
        public bushort _countVTX;
        public bushort _countSHP;
        public bushort _countMAT;
        public bushort _countUniforms;
        public buint _unk4; //offset?

        public String Name { get { return new String((sbyte*)StringAddress); } }
        public VoidPtr StringAddress
        {
            get { return _stringOffset.OffsetAddress; }
            set { _stringOffset.OffsetAddress = value; }
        }
        public FSKL* Skeleton
        {
            get { return (FSKL*)_skeletonOffset.OffsetAddress; }
            set { _skeletonOffset.OffsetAddress = value; }
        }
        public FVTX* VTXArray
        {
            get { return (FVTX*)_offsetVTX.OffsetAddress; }
            set { _offsetVTX.OffsetAddress = value; }
        }
        public ResourceGroup* SHPGroup
        {
            get { return (ResourceGroup*)_offsetGroupSHP.OffsetAddress; }
            set { _offsetGroupSHP.OffsetAddress = value; }
        }
        public ResourceGroup* MATGroup
        {
            get { return (ResourceGroup*)_offsetGroupMAT.OffsetAddress; }
            set { _offsetGroupMAT.OffsetAddress = value; }
        }
        public ResourceGroup* UniformGroup
        {
            get { return (ResourceGroup*)_offsetGroupUniforms.OffsetAddress; }
            set { _offsetGroupUniforms.OffsetAddress = value; }
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        /*Data Order: 
        FMDL
        FVTX array
        FSKL
        Bone array
        Bone resource group
        SHP Group, MAT Group, SHPs, MATs
        */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FVTX
    {
        public const int Size = 0x20;
        public const string Tag = "FVTX";

        public BinTag _tag;
        public byte _attributeCount;
        public byte _bufferCount;
        public bushort _index;
        public buint _elementCount;
        public byte _unk1;
        public byte _unk2;
        public bushort _unk3;
        public buint _attributeArrayOffset;
        public buint _attributeGroupOffset;
        public buint _bufferArrayOffset;
        public buint _pad; //0

        public FVTXAttribute* AttributeArray
        {
            get { return (FVTXAttribute*)_attributeArrayOffset.OffsetAddress; }
            set { _attributeArrayOffset.OffsetAddress = value; }
        }
        public ResourceGroup* AttributeGroup
        {
            get { return (ResourceGroup*)_attributeGroupOffset.OffsetAddress; }
            set { _attributeGroupOffset.OffsetAddress = value; }
        }
        public FVTXBuffer* BufferArray
        {
            get { return (FVTXBuffer*)_bufferArrayOffset.OffsetAddress; }
            set { _bufferArrayOffset.OffsetAddress = value; }
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FVTXAttribute
    {
        public const int Size = 0xC;

        public buint _attributeNameOffset;
        public byte _bufferIndex;
        public BUInt24 _bufferOffset;
        public bushort _unknown; //0
        public byte _type; //0, 2, 8 - flags?
        public byte _format;

        public FVTXAttributeFormat Format { get { return (FVTXAttributeFormat)_format; } set { _format = (byte)value; } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
        
        public String Name { get { return new String((sbyte*)StringAddress); } }
        public VoidPtr StringAddress
        {
            get { return _attributeNameOffset.OffsetAddress; }
            set { _attributeNameOffset.OffsetAddress = value; }
        }
    }

    public enum FVTXAttributeFormat
    {
        Unk0   = 0x00,
        Unk1   = 0x01,
        Unk2   = 0x02,
        Unk3   = 0x03,
        XY8    = 0x04,
        Unk4   = 0x05,
        Unk5   = 0x06, 
        XY16   = 0x07,
        Unk8   = 0x08,
        Unk9   = 0x09,
        XYZW8  = 0x0A,
        XYZ10  = 0x0B,
        Unk12  = 0x0C,
        XY32   = 0x0D,
        Unk14  = 0x0E,
        XYZW16 = 0x0F,
        Unk16  = 0x10,
        XYZ32  = 0x11,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FVTXBuffer
    {
        public const int Size = 0x18;

        public buint _unk1; //0
        public buint _dataSize;
        public buint _unk2; //0
        public bushort _stride;
        public bushort _unk3; //1
        public buint _unk4; //0
        public buint _dataOffset;

        public VoidPtr BufferAddress
        {
            get { return _dataOffset.OffsetAddress; }
            set { _dataOffset.OffsetAddress = value; }
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FSKL
    {
        public const int Size = 0x20;
        public const string Tag = "FSKL";

        public BinTag _tag;
        public bushort _unk1; //0
        public byte _unk2; //0x11, 0x12
        public byte _unk3; //0
        public bushort _boneCount;
        public bushort _inverseMtxIndexCount;
        public bushort _extraIndexCount;
        public bushort _pad1; //0
        public buint _boneGroupOffset;
        public buint _boneArrayOffset;
        public buint _inverseIndexArrayOffset; //array of bone indices
        public buint _inverseMatrixArrayOffset; //array of 4x3 matrices, only here in v3.4 bfres
        public buint _unk4; //0

        public bMatrix43* InverseMatrices
        {
            get { return (bMatrix43*)_inverseMatrixArrayOffset.OffsetAddress; }
            set { _inverseMatrixArrayOffset.OffsetAddress = value; }
        }
        public bushort* InverseMtxIndices
        {
            get { return (bushort*)_inverseIndexArrayOffset.OffsetAddress; }
            set { _inverseIndexArrayOffset.OffsetAddress = value; }
        }
        public FSKLBone* Bones
        {
            get { return (FSKLBone*)_boneArrayOffset.OffsetAddress; }
            set { _boneArrayOffset.OffsetAddress = value; }
        }
        public ResourceGroup* BoneGroup
        {
            get { return (ResourceGroup*)_boneGroupOffset.OffsetAddress; }
            set { _boneGroupOffset.OffsetAddress = value; }
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FSKLBone
    {
        public const int Size = 0x40;

        public buint _stringOffset;
        public bushort _boneIndex;
        public bshort _parentIndex1;
        public bshort _parentIndex2;
        public bshort _parentIndex3;
        public bshort _parentIndex4;
        public bushort _pad1; //0
        public Bin32 _flags; //0x ???? 1001
        public BVec3 _scale;
        public BVec4 _rotate;
        public BVec3 _translate;
        public buint _pad2;

        public VoidPtr StringAddress
        {
            get { return _stringOffset.OffsetAddress; }
            set { _stringOffset.OffsetAddress = value; }
        }
        public String Name { get { return new String((sbyte*)StringAddress); } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FMAT
    {
        public const int Size = 0x48;
        public const string Tag = "FMAT";

        public BinTag _tag;
        public buint _stringOffset;
        public buint _unk1;
        public bushort _sectionIndex;
        public bushort _renderInfoParamCount;
        public byte _texSelectorCount;
        public byte _texAttributeSelectorCount;
        public bushort _matParamCount;
        public buint _matParamDataSize;
        public buint _unk2; //0, 1, 2
        public buint _renderInfoParamGroupOffset;
        public buint _unk3Offset;
        public buint _shaderControlOffset;
        public buint _texSelectorOffset;
        public buint _texAttribSelectorOffset;
        public buint _texAttribSelectorGroupOffset;
        public buint _matParamArrayOffset;
        public buint _matParamGroupOffset;
        public buint _matParamDataOffset;
        public buint _shadowParamGroupOffset;
        public buint _unk4Offset;

        public VoidPtr StringAddress
        {
            get { return _stringOffset.OffsetAddress; }
            set { _stringOffset.OffsetAddress = value; }
        }
        public String Name { get { return new String((sbyte*)StringAddress); } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FSHP
    {
        public const int Size = 0x40;
        public const string Tag = "FSHP";

        public BinTag _tag;
        public buint _stringOffset;
        public buint _unk1;
        public bushort _fvtxIndex1;
        public bushort _fmatIndex;
        public bushort _fsklIndex;
        public bushort _fvtxIndex2;
        public bushort _fsklIndexArrayCount;
        public byte _unk2; //used when fskl index array is present
        public byte _lodCount;
        public buint _visGroupTreeNodeCount;
        public bfloat _unk3;
        public buint _fvtxOffset; //Offset to fvtx header
        public buint _lodArrayOffset;
        public buint _fsklIndexArrayOffset; //to bushort array
        public buint _unk4; //0
        public buint _visGroupTreeNodesOffset;
        public buint _visGroupTreeRangesOffset;
        public buint _visGroupTreeIndicesOffset;
        public buint _unk5; //0



        public VoidPtr StringAddress
        {
            get { return _stringOffset.OffsetAddress; }
            set { _stringOffset.OffsetAddress = value; }
        }
        public String Name { get { return new String((sbyte*)StringAddress); } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FSHPLodEntry
    {
        public const int Size = 0x1C;

        public buint _unk1; //4
        public buint _unk2; //4
        public buint _count;
        public bushort _visGroupCount;
        public bushort _unk3; //0
        public buint _visArrayOffset;
        public buint _indexArrayOffset;
        public buint _fvtxBufferStartIndex; //number of elements into the buffer

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FSHPVisGroup
    {
        public const int Size = 8;

        public buint _indexBufferOffset; //in bytes
        public buint _count;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FSHPIndexBuffer
    {
        public const int Size = 0x18;

        public buint _unk1; //0
        public buint _size; //in bytes
        public buint _unk2;
        public buint _unk3;
        public buint _unk4;
        public buint _unk5;
        public buint _indexDataOffset;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FSHPVisTreeNode
    {
        public const int Size = 0xC;
        
        public bushort _leftIndex;
        public bushort _rightIndex;
        public bushort _unknown; //same as left index
        public bushort _nextSiblingIndex;
        public bushort _visGroupIndex;
        public bushort _visGroupCount;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FSHPVisTreeRange
    {
        public const int Size = 0x18;

        public BVec3 _unk1;
        public BVec3 _unk2;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FMDLUniform
    {
        public const int Size = 0xC;

        public buint _variableStringOffset;
        public bushort _unk1; //1
        public bushort _unk2; //0
        public bfloat _value;

        public String VariableName { get { return new String((sbyte*)_variableStringOffset.OffsetAddress); } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
}
