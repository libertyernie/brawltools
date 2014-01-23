using System;
using BrawlLib.Imaging;
using BrawlLib.SSBBTypes;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System
{
    public unsafe struct BrawlBoxViewerSettings
    {
        public const uint Tag = 0x53564242;
        public const uint Size = 0xA4;

        public uint _tag;
        public byte _version;
        public Bin8 _flags1;
        public Bin16 _flags2;
        public Bin32 _flags3;
        public bfloat _tScale, _rScale, _zScale, _nearZ, _farz, _yFov;
        public BVec4 _amb, _pos, _diff, _spec, _emis; 
        public BVec3 _defaultCam;
        public BVec2 _defaultRot;
        public ARGBPixel _orbColor;
        public ARGBPixel _lineColor;
        public ARGBPixel _floorColor;
        public buint _screenCapPathOffset;
        public buint _undoCount;
        public bint _shaderCount;
        public bint _matCount;

        public bint* ShaderOffsets { get { return (bint*)_matCount.Address + 1; } }
        public bint* MaterialOffsets { get { return ShaderOffsets + _shaderCount; } }

        public MDL0Shader* GetShader(int index) { return (MDL0Shader*)(Address + ShaderOffsets[index]); }
        public MDL0Material* GetMaterial(int index) { return (MDL0Material*)(Address + MaterialOffsets[index]); }

        public bool RetrieveCorrAnims { get { return _flags1[0]; } set { _flags1[0] = value; } }
        public bool DisplayExternalAnims { get { return _flags1[1]; } set { _flags1[1] = value; } }
        public bool DisplayNonBRRESAnims { get { return _flags1[2]; } set { _flags1[2] = value; } }
        public bool SyncTexToObj { get { return _flags1[3]; } set { _flags1[3] = value; } }
        public bool SyncObjToVIS0 { get { return _flags1[4]; } set { _flags1[4] = value; } }
        public bool DisableBonesOnPlay { get { return _flags1[5]; } set { _flags1[5] = value; } }
        public bool Maximize { get { return _flags1[6]; } set { _flags1[6] = value; } }
        public bool CameraSet { get { return _flags1[7]; } set { _flags1[7] = value; } }

        public bool UseDataTable { get { return _flags2[0]; } set { _flags2[0] = value; } }
        public bool HasShaders { get { return _flags2[1]; } set { _flags2[1] = value; } }
        public bool HasMaterials { get { return _flags2[2]; } set { _flags2[2] = value; } }
        public int ImageCapFmt { get { return _flags2[3, 3]; } set { _flags2[3, 3] = (ushort)value; } }
        public bool Bones { get { return _flags2[6]; } set { _flags2[6] = value; } }
        public bool Polys { get { return _flags2[7]; } set { _flags2[7] = value; } }

        public bool Wireframe { get { return _flags2[8]; } set { _flags2[8] = value; } }
        public bool Floor { get { return _flags2[9]; } set { _flags2[9] = value; } }
        public bool Vertices { get { return _flags2[10]; } set { _flags2[10] = value; } }
        public bool Normals { get { return _flags2[11]; } set { _flags2[11] = value; } }
        public bool ShowCamCoords { get { return _flags2[12]; } set { _flags2[12] = value; } }
        public bool OrthoCam { get { return _flags2[13]; } set { _flags2[13] = value; } }
        public bool BoundingBox { get { return _flags2[14]; } set { _flags2[14] = value; } }
        public bool HideOffscreen { get { return _flags2[15]; } set { _flags2[15] = value; } }

        public bool EnableSmoothing { get { return _flags3[0]; } set { _flags3[0] = value; } }
        public bool EnableText { get { return _flags3[1]; } set { _flags3[1] = value; } }

        public bool GenTansCHR { get { return _flags3[2]; } set { _flags3[2] = value; } }
        public bool GenTansSRT { get { return _flags3[3]; } set { _flags3[3] = value; } }
        public bool GenTansSHP { get { return _flags3[4]; } set { _flags3[4] = value; } }
        public bool GenTansLight { get { return _flags3[5]; } set { _flags3[5] = value; } }
        public bool GenTansFog { get { return _flags3[6]; } set { _flags3[6] = value; } }
        public bool GenTansCam { get { return _flags3[7]; } set { _flags3[7] = value; } }
        
        public bool LinearCHR { get { return _flags3[8]; } set { _flags3[8] = value; } }
        public bool LinearSRT { get { return _flags3[9]; } set { _flags3[9] = value; } }
        public bool LinearSHP { get { return _flags3[10]; } set { _flags3[10] = value; } }
        public bool LinearLight { get { return _flags3[11]; } set { _flags3[11] = value; } }
        public bool LinearFog { get { return _flags3[12]; } set { _flags3[12] = value; } }
        public bool LinearCam { get { return _flags3[13]; } set { _flags3[13] = value; } }

        public bool DisplayBRRESAnims { get { return _flags3[14]; } set { _flags3[14] = value; } }
        
        private VoidPtr Address { get { fixed (void* p = &this)return p; } }

        public static readonly BrawlBoxViewerSettings Default = new BrawlBoxViewerSettings()
        {
            RetrieveCorrAnims = true,
            SyncTexToObj = false,
            SyncObjToVIS0 = false,
            DisableBonesOnPlay = true,
            Maximize = false,
            _amb = new Vector4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 1.0f),
            _diff = new Vector4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 1.0f),
            _pos = new Vector4(100.0f, 45.0f, 45.0f, 1.0f),
            _spec =  new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            _emis = new Vector4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 1.0f),
            _yFov = 45.0f,
            _nearZ = 1.0f,
            _farz = 200000.0f,
            _zScale = 2.5f,
            _tScale = 0.05f,
            _rScale = 0.4f,
            _orbColor = new ARGBPixel(255, 0, 128, 0),
            _lineColor = new ARGBPixel(255, 0, 0, 128),
            _floorColor = new ARGBPixel(255, 128, 128, 191),
            _defaultCam = new BVec3(),
            _defaultRot = new BVec2(),
            _undoCount = 50,
            ImageCapFmt = 0,
            Bones = true,
            Wireframe = false,
            Polys = true,
            Vertices = false,
            BoundingBox = false,
            Normals = false,
            HideOffscreen = false,
            ShowCamCoords = false,
            Floor = false,
            EnableText = false,
            EnableSmoothing = false,
            LinearCHR = false,
            LinearSRT = false,
            LinearSHP = false,
            LinearLight = true,
            LinearFog = true,
            LinearCam = true,
            GenTansCHR = true,
            GenTansSRT = true,
            GenTansSHP = true,
            GenTansLight = true,
            GenTansFog = true,
            GenTansCam = true,
            OrthoCam = false,
            DisplayNonBRRESAnims = true,
            DisplayExternalAnims = true,
            DisplayBRRESAnims = true,
            CameraSet = false,
        };
    }
}
