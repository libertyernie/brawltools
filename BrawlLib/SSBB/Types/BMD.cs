using BrawlLib.Imaging;
using BrawlLib.Wii.Graphics;
using BrawlLib.Wii.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct J3DCommonHeader
    {
        public const int Size = 0x20;

        public const string J3DTag = "J3D"; //1, 2
        public const string SVRTag = "SVR3";

        public const string BMDTag = "bmd"; //3
        public const string BDLTag = "bdl"; //4

        public BinTag _j3dTag;
        public BinTag _nodeTag;
        public buint _dataSize;
        public buint _sectionCount;
        public BinTag _tag3; //0xFF or SVR3
        public buint _pad1; //0xFF
        public buint _pad2; //0xFF
        public buint _unknown; //Usually 0xFF

        public J3DCommonHeader(
            string nodeTag,
            uint size,
            uint sectionCount,
            int j3dVersion,
            int nodeVersion,
            bool SVR3)
        {
            _j3dTag = J3DTag;
            _nodeTag = nodeTag;
            _dataSize = size;
            _sectionCount = sectionCount;

            if (SVR3)
                _tag3 = SVRTag;
            else
                _tag3 = 0xFFFFFFFF;

            _pad1 = _pad2 = 0xFFFFFFFF;
            _unknown = 0;

            J3DVersion = j3dVersion;
            NodeVersion = nodeVersion;
        }

        public int J3DVersion
        {
            get { return ((byte*)_j3dTag.Address)[3] & 0xF; }
            set { ((byte*)_j3dTag.Address)[3] = (byte)(0x30 | value.Clamp(0, 9)); }
        }
        public int NodeVersion
        {
            get { return ((byte*)_nodeTag.Address)[3] & 0xF; }
            set { ((byte*)_nodeTag.Address)[3] = (byte)(0x30 | value.Clamp(0, 9)); }
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        //Align all sections to 0x20 using AlignPadding instead of zerobytes
        public const string AlignPadding = "This is padding data to align";
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDCommonHeader
    {
        public const int Size = 8;

        public BinTag _tag;
        public buint _sectionSize;

        public int Version
        {
            get { return ((byte*)_tag.Address)[3] & 0xF; }
            set { ((byte*)_tag.Address)[3] = (byte)(0x30 | value.Clamp(0, 9)); }
        }

        public BMDCommonHeader(string tag, uint size, int version)
        {
            _tag = tag;
            _sectionSize = size;
            Version = version;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDInfoHeader
    {
        public const int Size = 0x18;
        public const string Tag = "INF"; //1

        public BMDCommonHeader _header;
        public bushort _count;
        public bushort _pad; //0xFFFF
        public buint _unknown;
        public buint _numVertices; //number of coords in VTX section
        public buint _dataOffset; //0x18

        public BMDInfoEntry* Entries { get { return (BMDInfoEntry*)(Address + _dataOffset); } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDInfoEntry
    {
        public const int Size = 4;
        
        public bushort _type;
        public bushort _index;

        public INFType Type
        {
            get { return (INFType)(ushort)_type; }
            set { _type = (ushort)value; }
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    public enum INFType
    {
        Terminator = 0x0,

        HierarchyDown = 0x01,
        HierarchyUp = 0x02,

        Bone = 0x10,
        Material = 0x11,
        Object = 0x12,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDVertexArrayHeader
    {
        public const int Size = 0x40;
        public const string Tag = "VTX"; //1

        public BMDCommonHeader _header;
        public buint _arrayFormatOffset; //0x40
        private fixed uint _offsets[13];

        public buint* Offsets { get { return (buint*)Address + 3; } }

        public VoidPtr GetAddress(int index)
        {
            uint offset = Offsets[index];
            if (offset > 0)
                return Address + offset;
            else
                return null;
        }

        public BMDArrayFormat* Formats { get { return (BMDArrayFormat*)(Address + _arrayFormatOffset); } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDArrayFormat
    {
        public const int Size = 0x10;

        public buint _arrayType;
        public bint _isSpecial;
        public buint _componentType;
        public byte _divisor;
        public BUInt24 _pad;

        public GXAttribute ArrayType
        {
            get { return (GXAttribute)(uint)_arrayType; }
            set { _arrayType = (uint)value; }
        }
        public WiiVertexComponentType VertexComponentType
        {
            get { return (WiiVertexComponentType)(uint)_componentType; }
            set { _componentType = (uint)value; }
        }
        public WiiColorComponentType ColorComponentType
        {
            get { return (WiiColorComponentType)(uint)_componentType; }
            set { _componentType = (uint)value; }
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDEnvelopesHeader
    {
        public const int Size = 0x1C;
        public const string Tag = "EVP"; //1

        public BMDCommonHeader _header;
        public bushort _count;
        public bushort _pad; //0xFFFF
        public buint _countsOffset;
        public buint _indicesOffset;
        public buint _weightsOffset;
        public buint _matricesOffset;

        private byte* Counts { get { return (byte*)(Address + _countsOffset); } }
        private bfloat* Weights { get { return (bfloat*)(Address + _weightsOffset); } }
        private bushort* Indices { get { return (bushort*)(Address + _indicesOffset); } }

        public ushort[] GetIndices(int index)
        {
            int count = Counts[index], offset = 0;
            for (int i = 0; i < index; i++)
                offset += Counts[i];
            ushort[] array = new ushort[count];
            bushort* data = &Indices[offset];
            for (int i = 0; i < count; i++)
                array[i] = data[i];
            return array;
        }
        public float[] GetWeights(int index)
        {
            int count = Counts[index], offset = 0;
            for (int i = 0; i < index; i++)
                offset += Counts[i];
            float[] array = new float[count];
            bfloat* data = &Weights[offset];
            for (int i = 0; i < count; i++)
                array[i] = data[i];
            return array;
        }
        public Matrix GetMatrix(int index)
        {
            return ((bMatrix43*)(Address + _matricesOffset))[index];
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDDrawHeader
    {
        public const int Size = 0x14;
        public const string Tag = "DRW"; //1

        public BMDCommonHeader _header;
        public bushort _count;
        public bushort _pad; //0xFFFF
        public buint _isWeightedOffset; //Align data to 0x2
        public buint _dataOffset;

        private byte* Weighted { get { return (byte*)(Address + _isWeightedOffset); } }
        
        //Index into global matrix for unweighted
        public bushort* Data { get { return (bushort*)(Address + _dataOffset); } }

        public bool GetIsWeighted(int index)
        {
            return Weighted[index] != 0;
        }
        public void SetIsWeighted(int index, bool value)
        {
            Weighted[index] = (byte)(value ? 1 : 0);
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDJointsHeader
    {
        public const int Size = 0x18;
        public const string Tag = "JNT"; //1
        
        public BMDCommonHeader _header;
        public bushort _count;
        public bushort _pad; //0xFFFF
        public buint _jointsOffset;
        public buint _stringIDTableOffset;
        public buint _stringTableOffset;

        public BMDJointEntry* Joints { get { return (BMDJointEntry*)(Address + _jointsOffset); } }
        
        //Pad to 4 bytes
        public bushort* StringIDs { get { return (bushort*)(Address + _stringIDTableOffset); } }
        
        public VoidPtr StringTable { get { return Address + _stringTableOffset; } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    public unsafe class BMDStringTable
    {
        SortedList<string, int> _table = new SortedList<string, int>(StringComparer.Ordinal);
        
        public void Add(string s)
        {
            if ((!String.IsNullOrEmpty(s)) && (!_table.ContainsKey(s)))
                _table.Add(s, 0);
        }

        public int GetTotalSize()
        {
            int len = 4;
            foreach (string s in _table.Keys)
                len += (s.Length + 5);
            return len;
        }

        public void Clear() { _table.Clear(); }

        public VoidPtr this[string s]
        {
            get
            {
                if ((!String.IsNullOrEmpty(s)) && (_table.ContainsKey(s)))
                    return _table[s];
                return _table.Values[0];
            }
        }
        private ushort Hash(string value)
        {
            ushort hash = 0;
            foreach (char c in value)
            {
                hash *= 3;
                hash += (ushort)c;
            }

            return hash;
        }

        public void WriteTable(VoidPtr address)
        {
            bushort* data = (bushort*)address;
            *data++ = (ushort)_table.Count;
            *data++ = 0xFFFF;
            sbyte* strings = (sbyte*)data + _table.Count * 4;
            for (int i = 0; i < _table.Count; i++, data++)
            {
                string s = _table.Keys[i];
                *data++ = Hash(s);
                *data = (ushort)((int)(data + 1) - (int)address);
                _table[s] = i;
                s.Write(ref strings);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDJointEntry
    {
        public const int Size = 0x40;

        public bushort _unk1; //0, 1, 2
        public byte _unk2; //0, 1, 0xFF
        public byte _pad1; //0xFF
        public BVec3 _scale;
        public bshort _rotX; //-32768 = -180 deg, 32767 = 180 deg
        public bshort _rotY;
        public bshort _rotZ;
        public bshort _pad2; //0xFFFF
        public BVec3 _trans;
        public bfloat _unk3;
        public BVec3 _boxMin;
        public BVec3 _boxMax;

        public Vector3 Rotation
        {
            get
            {
                return new Vector3(
                    (float)(short)_rotX / 32768.0f * 180.0f,
                    (float)(short)_rotY / 32768.0f * 180.0f,
                    (float)(short)_rotZ / 32768.0f * 180.0f);
            }
            set
            {
                _rotX = (short)(value._x.RemapToRange(-180.0f, 180.0f) / 180.0f * 32768.0f);
                _rotY = (short)(value._y.RemapToRange(-180.0f, 180.0f) / 180.0f * 32768.0f);
                _rotZ = (short)(value._z.RemapToRange(-180.0f, 180.0f) / 180.0f * 32768.0f);
            }
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDObjectsHeader
    {
        public const int Size = 0x2C;
        public const string Tag = "SHP"; //1

        public BMDCommonHeader _header;
        public bushort _count;
        public bushort _padding; //0xFFFF
        public buint _objectsOffset;
        public buint _indexTableOffset;
        public buint _unk1; //0
        public buint _attributesOffset;
        public buint _matrixTableOffset; //Offset to MatrixTable
        public buint _primitiveDataOffset; //Offset to the actual primitive data
        public buint _matrixDataOffset; //Offset to MatrixData
        public buint _groupsOffset;

        public VoidPtr Primitives { get { return Address + _primitiveDataOffset; } }
        public BMDObjectEntry* Objects { get { return (BMDObjectEntry*)(Address + _objectsOffset); } }
        public PacketLocation* Groups { get { return (PacketLocation*)(Address + _groupsOffset); } }
        public MatrixData* MatrixData { get { return (MatrixData*)(Address + _matrixDataOffset); } }
        public bushort* IndexTable { get { return (bushort*)(Address + _indexTableOffset); } }
        public bushort* MatrixTable { get { return (bushort*)(Address + _matrixTableOffset); } }

        public BMDObjectAttrib* GetAttrib(int offset)
        {
            return (BMDObjectAttrib*)(Address + _attributesOffset + offset);
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDObjectEntry
    {
        public const int Size = 0x28;
        
        public byte _billboardType; //0 = none, 1 = STD, 2 = Y, 3 = ROT
        public byte _pad1; //0xFF
        public bushort _groupCount;
        public bushort _attribOffset; //Relative to AttributesOffset
        public bushort _firstMatrixDataIndex; //Index to 'PacketCount' consecutive indexes
        public bushort _firstGroupIndex;
        public bushort _pad2;
        public bfloat _unknown2;
        public BVec3 _boxMin;
        public BVec3 _boxMax;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    public struct PacketLocation
    {
        public buint _size; //number of bytes in group
        public buint _offset; //Relative to PrimitiveDataOffset
    }

    public struct MatrixData
    {
        public bushort _unknown;
        public bushort _count; //count many consecutive indices into matrixTable
        public buint _firstIndex; //first index into matrix table
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDObjectAttrib
    {
        public buint _arrayType;
        public buint _dataType;

        public GXAttribute ArrayType
        {
            get { return (GXAttribute)(uint)_arrayType; }
            set { _arrayType = (uint)value; }
        }

        public WiiVertexComponentType DataFormat
        {
            get { return (WiiVertexComponentType)(uint)_dataType; }
            set { _dataType = (uint)value; }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDRegistersHeader
    {
        public const int Size = 0;
        public const string Tag = "MDL"; //1
        
        public BMDCommonHeader _header;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDMaterialsHeader
    {
        public const int Size3 = 0x84;
        public const int Size2 = 0x40;
        public const string Tag = "MAT"; //2, 3

        public BMDCommonHeader _header;
        public bushort _count;
        public bushort _padding; //0xFFFF

        private buint _value1;
        
        private buint* Values { get { fixed (buint* ptr = &_value1)return ptr; } }

        BMDMaterialEntry3* MaterialEntries { get { return (BMDMaterialEntry3*)(GetAddress(MatOffsets.MaterialEntries)); } }
        BMDMaterialEntry3* MaterialIndices { get { return (BMDMaterialEntry3*)(GetAddress(MatOffsets.MaterialIndices)); } }
        BMDMaterialEntry3* StringTable { get { return (BMDMaterialEntry3*)(GetAddress(MatOffsets.StringTable)); } }
        BMDMaterialEntry3* IndirectTexturing { get { return (BMDMaterialEntry3*)(GetAddress(MatOffsets.IndirectTexturing)); } }
        BMDMaterialEntry3* CullMode { get { return (BMDMaterialEntry3*)(GetAddress(MatOffsets.CullMode)); } }
        BMDMaterialEntry3* AmbientColor { get { return (BMDMaterialEntry3*)(GetAddress(MatOffsets.AmbientColor)); } }
        BMDMaterialEntry3* ColorChanNum { get { return (BMDMaterialEntry3*)(GetAddress(MatOffsets.ColorChanNum)); } }
        BMDMaterialEntry3* MaterialColor { get { return (BMDMaterialEntry3*)(GetAddress(MatOffsets.MaterialColor)); } }
        BMDMaterialEntry3* LightInfo { get { return (BMDMaterialEntry3*)(GetAddress(MatOffsets.LightInfo)); } }
        BMDMaterialEntry3* TexGenNum { get { return (BMDMaterialEntry3*)(GetAddress(MatOffsets.TexGenNum)); } }
        BMDMaterialEntry3* TexCoordInfo { get { return (BMDMaterialEntry3*)(GetAddress(MatOffsets.TexCoordInfo)); } }
        BMDMaterialEntry3* TexCoordInfo2 { get { return (BMDMaterialEntry3*)(GetAddress(MatOffsets.TexCoordInfo2)); } }

        public VoidPtr GetAddress(MatOffsets value)
        {
            //Use version to remap value
            //int version = _header.Version;

            uint v = Values[(int)value];
            return v > 0 ? Address + v : null;
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    public enum MatOffsets
    {
        MaterialEntries = 0,
        MaterialIndices = 1,
        StringTable = 2,
        IndirectTexturing = 3,
        CullMode = 4,
        AmbientColor = 5,
        ColorChanNum = 6,
        ColorChanInfo = 7,
        MaterialColor = 8,
        LightInfo = 9,
        TexGenNum = 10,
        TexCoordInfo = 11,
        TexCoordInfo2 = 12,
        TexMatrixInfo = 13,
        TexMatrix2Info = 14,
        TexTable = 15,
        TevOrderInfo = 16,
        ColorRegisters = 17,
        KonstRegisters = 18,
        TEVCounts = 19,
        TEVStages = 20,
        TEVSwapModeInfo = 21,
        TEVSwapModeTable = 22,
        FogInfo = 23,
        AlphaCompare = 24,
        Blend = 25,
        ZMode = 26,
        Unk1 = 27,
        Unk2 = 28,
        Unk3 = 29,
        NBTScale = 30
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDMaterialEntry3
    {
        public const int Size = 0x130;
        
        public byte _unknown1; //Read by patched material, always 1?
        public byte _unknown2; //Mostly 0, sometimes 2
        public bushort _padding1; //Always 0?
        public bushort _indirectTexturingIndex;
        public bushort _cullModeIndex;
        public fixed ushort _ambColorIndex[2];
        public fixed ushort _chanControlIndex[4];
        public fixed ushort _matColorIndex[2];
        public fixed ushort _lightingIndex[8];
        public fixed ushort _texCoordIndex[8];
        public fixed ushort _texCoord2Index[8];
        public fixed ushort _texMatrixIndex[8];
        public fixed ushort _texMatrix2Index[16];
        public fixed ushort _texIndex[8];
        public fixed ushort _tevConstantColorIndex[4];
        public fixed byte _constColorSel[16];
        public fixed byte _constAlphaSel[16];
        public fixed ushort _tevOrderIndex[16];
        public fixed ushort _tevColorIndex[4];
        public fixed ushort _tevStageInfoIndex[16];
        public fixed ushort _tevSwapModeInfoIndex[16];
        public fixed ushort _tevSwapModeTableIndex[4];
        public fixed ushort _unknownIndices[12];
        public bushort _fogIndex;
        public bushort _alphaCompareIndex;
        public bushort _blendInfoIndex;
        public bushort _nbtScaleIndex;
        
        public bushort* AmbColorIndices { get { fixed (ushort* b = _ambColorIndex) return (bushort*)b; } }
        public bushort* ChanControlIndices { get { fixed (ushort* b = _chanControlIndex) return (bushort*)b; } }
        public bushort* MatColorIndices { get { fixed (ushort* b = _matColorIndex) return (bushort*)b; } }
        public bushort* LightingIndices { get { fixed (ushort* b = _lightingIndex) return (bushort*)b; } }
        public bushort* TexCoordIndices { get { fixed (ushort* b = _texCoordIndex) return (bushort*)b; } }
        public bushort* TexCoord2Indices { get { fixed (ushort* b = _texCoord2Index) return (bushort*)b; } }
        public bushort* TexMatrixIndices { get { fixed (ushort* b = _texMatrixIndex) return (bushort*)b; } }
        public bushort* TexMatrix2Indices { get { fixed (ushort* b = _texMatrix2Index) return (bushort*)b; } }
        public bushort* TextureIndices { get { fixed (ushort* b = _texIndex) return (bushort*)b; } }
        public bushort* TevConstantColorIndices { get { fixed (ushort* b = _tevConstantColorIndex) return (bushort*)b; } }
        public bushort* ConstColorSel { get { fixed (byte* b = _constColorSel) return (bushort*)b; } }
        public bushort* ConstAlphaSel { get { fixed (byte* b = _constAlphaSel) return (bushort*)b; } }
        public bushort* TevOrderIndices { get { fixed (ushort* b = _tevOrderIndex) return (bushort*)b; } }
        public bushort* TevColorIndices { get { fixed (ushort* b = _tevColorIndex) return (bushort*)b; } }
        public bushort* TevStageInfoIndices { get { fixed (ushort* b = _tevStageInfoIndex) return (bushort*)b; } }
        public bushort* TevSwapModeInfoIndices { get { fixed (ushort* b = _tevSwapModeInfoIndex) return (bushort*)b; } }
        public bushort* TevSwapModeTableInfoIndices { get { fixed (ushort* b = _tevSwapModeTableIndex) return (bushort*)b; } }
        public bushort* UnknownIndices { get { fixed (ushort* b = _unknownIndices) return (bushort*)b; } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDIndirectTexturingEntry
    {
        //size = 312 = 0x138
        //(not always...see default.bmd <- but there 3 and 2 point to the same
        //location in file (string table). but default.bmd is the only file i know
        //where number of ind tex entries doesn't match number of mats)
        //this could be arguments to GX_SetIndTexOrder() plus some dummy values
        public fixed ushort unk[10];
        struct unk2
        {
            public fixed float f[6]; //3x2 matrix? texmatrix?
            public fixed byte b[4];
        }
        public fixed byte unk2data[(6 * 4 + 4) * 3];
        //probably the arguments to GX_SetIndTexOrder()
        //or GX_SetIndtexCoordScale() (index is first param)
        public fixed uint unk3[4];
        struct unk4
        {
            //the first 9 bytes of this array are probably the arguments to
            //GX_SetTevIndirect (index is the first argument), the
            //other three bytes are padding
            public fixed ushort unk[6];
        }
        public fixed byte unk4data[6 * 2 * 16];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDColorChanInfo
    {
        public const int Size = 8;

        public byte _enable;
        public byte _matColorSource;
        public byte _litMask;
        public byte _diffuseAttenuationFunc;
        public byte _attenuationFracFunc;
        public byte _ambColorSource;
        public fixed byte _pad[2];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDTexGenInfo
    {
        public const int Size = 4;

        public byte _texGenType;
        public byte _texGenSrc;
        public byte _matrix;
        public byte _pad;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDTexMtxInfo
    {
        public const int Size = 8;

        byte _projection;
        byte _type;
        bushort _pad0; // 0xFFFF
        bfloat _center_s;
        bfloat _center_t;
        bfloat _unknown0;
        bfloat _scale_s;
        bfloat _scale_t;
        bshort _rotate; // -32768 = -180 deg, 32768 = 180 deg
        bushort _pad1; // 0xFFFF
        bfloat _translate_s;
        bfloat _translate_t;
        Matrix _preMatrix;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDTevOrderInfo
    {
        public const int Size = 4;

        public byte _texCoordId;
        public byte _texMap;
        public byte _chanId;
        public byte _pad;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDTevStageInfo
    {
        public const int Size = 20;

        public byte unk; //always 0xff
        //GX_SetTevColorIn() arguments
        public fixed byte colorIn[4]; //GX_CC_*
        //GX_SetTevColorOp() arguments
        public byte colorOp;
        public byte colorBias;
        public byte colorScale;
        public byte colorClamp;
        public byte colorRegId;
        //GX_SetTevAlphaIn() arguments
        public fixed byte alphaIn[4]; //GC_CA_*
        //GX_SetTevAlphaOp() arguments
        public byte alphaOp;
        public byte alphaBias;
        public byte alphaScale;
        public byte alphaClamp;
        public byte alphaRegId;
        public byte unk2; //always 0xff
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    public unsafe struct BMDTevSwapModeInfo
    {
        public const int Size = 4;

        public byte _rasSel;
        public byte _texSel;
        public fixed byte _pad[2];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    public unsafe struct BMDTevSwapModeTable
    {
        public const int Size = 4;

        public RGBAPixel _color;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    public unsafe struct BMDAlphaCompare
    {
        public const int Size = 8;

        public byte _comp0;
        public byte _ref0;
        public byte _alphaOp;
        public byte _comp1;
        public byte _ref1;
        public fixed byte _pad[3];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    public unsafe struct BMDBlendInfo
    {
        public const int Size = 4;

        public byte _blendMode;
        public byte _srcFactor;
        public byte _dstFactor;
        public byte _logicOp;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    public unsafe struct BMDZModeInfo
    {
        public const int Size = 4;

        public byte _enable;
        public byte _func;
        public byte _updateEnable;
        public byte _pad; //(ref val?)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    public unsafe struct BMDFogInfo
    {
        public const int Size = 44;

        public byte _fogType;
        public byte _enable;
        public bushort _center;
        public bfloat _startZ;
        public bfloat _endZ;
        public bfloat _nearZ;
        public bfloat _farZ;
        public RGBAPixel _color;
        public fixed ushort _adjTable[10]; //????
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BMDTexturesHeader
    {
        public const int Size = 0x130;
        public const string Tag = "TEX"; //1

        public BMDCommonHeader _header;
        public bushort _count;
        public bushort _padding; //0xFFFF
        public buint _textureHeadersOffset;
        public buint _stringTableOffset;

        public BTIHeader* TextureHeaders { get { return (BTIHeader*)(Address + _textureHeadersOffset); } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public VoidPtr StringTable { get { return Address + _stringTableOffset; } }
    }
}
