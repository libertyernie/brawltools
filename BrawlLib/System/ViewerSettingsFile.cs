using System;
using BrawlLib.Imaging;
using BrawlLib.SSBBTypes;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System
{
    public unsafe struct BrawlBoxModelViewerEntry
    {
        public Bin32 _flags;
        public bfloat _tScale, _rScale, _zScale, _nearZ, _farz, _yFov;
        public BVec4 _amb, _pos, _diff, _spec, _emis;
        public BVec3 _defaultCam;
        public BVec2 _defaultRot;
        public ARGBPixel _orbColor;
        public ARGBPixel _lineColor;
        public ARGBPixel _floorColor;

        public bool Bones { get { return _flags[0]; } set { _flags[0] = value; } }
        public bool Polys { get { return _flags[1]; } set { _flags[1] = value; } }
        public bool Wireframe { get { return _flags[2]; } set { _flags[2] = value; } }
        public bool Floor { get { return _flags[3]; } set { _flags[3] = value; } }
        public bool Vertices { get { return _flags[4]; } set { _flags[4] = value; } }
        public bool Normals { get { return _flags[5]; } set { _flags[5] = value; } }
        public bool ShowCamCoords { get { return _flags[6]; } set { _flags[6] = value; } }
        public bool OrthoCam { get { return _flags[7]; } set { _flags[7] = value; } }
        public bool BoundingBox { get { return _flags[8]; } set { _flags[8] = value; } }
        public bool HideOffscreen { get { return _flags[9]; } set { _flags[9] = value; } }
        public bool CameraSet { get { return _flags[10]; } set { _flags[10] = value; } }
        public bool EnableSmoothing { get { return _flags[11]; } set { _flags[11] = value; } }
        public bool EnableText { get { return _flags[12]; } set { _flags[12] = value; } }
    }

    public unsafe struct BrawlBoxViewerSettings
    {
        public const uint Tag = 0x53564242;
        public const uint Size = 0xA4;

        public uint _tag;
        public byte _version;
        public Bin8 _flags1;
        public Bin16 _flags2;
        public Bin32 _flags3;
        public buint _screenCapPathOffset;
        public buint _undoCount;
        public bint _shaderCount;
        public bint _matCount;
        public buint _modelPanelCount;
        public buint _modelPanelOffset;
        public buint _shaderOffset;
        public buint _materialOffset;

        public MDL0Shader* GetShader(int index) { return (MDL0Shader*)(Address + ((buint*)(Address + _shaderOffset))[index]); }
        public MDL0Material* GetMaterial(int index) { return (MDL0Material*)(Address + ((buint*)(Address + _materialOffset))[index]); }

        public bool RetrieveCorrAnims { get { return _flags1[0]; } set { _flags1[0] = value; } }
        public bool DisplayExternalAnims { get { return _flags1[1]; } set { _flags1[1] = value; } }
        public bool DisplayNonBRRESAnims { get { return _flags1[2]; } set { _flags1[2] = value; } }
        public bool SyncTexToObj { get { return _flags1[3]; } set { _flags1[3] = value; } }
        public bool SyncObjToVIS0 { get { return _flags1[4]; } set { _flags1[4] = value; } }
        public bool DisableBonesOnPlay { get { return _flags1[5]; } set { _flags1[5] = value; } }
        public bool Maximize { get { return _flags1[6]; } set { _flags1[6] = value; } }

        //flags1 7 unused

        public bool UseDataTable { get { return _flags2[0]; } set { _flags2[0] = value; } }
        public bool HasShaders { get { return _flags2[1]; } set { _flags2[1] = value; } }
        public bool HasMaterials { get { return _flags2[2]; } set { _flags2[2] = value; } }
        public int ImageCapFmt { get { return _flags2[3, 3]; } set { _flags2[3, 3] = (ushort)value; } }

        //flags2 6 - 15 unused

        //flags3 0, 1 unused

        public bool GenTansCHR { get { return _flags3[2]; } set { _flags3[2] = value; } }
        public bool GenTansSRT { get { return _flags3[3]; } set { _flags3[3] = value; } }
        public bool GenTansSHP { get { return _flags3[4]; } set { _flags3[4] = value; } }
        public bool GenTansLight { get { return _flags3[5]; } set { _flags3[5] = value; } }
        public bool GenTansFog { get { return _flags3[6]; } set { _flags3[6] = value; } }
        public bool GenTansCam { get { return _flags3[7]; } set { _flags3[7] = value; } }
        //public bool LinearCHR { get { return _flags3[8]; } set { _flags3[8] = value; } }
        //public bool LinearSRT { get { return _flags3[9]; } set { _flags3[9] = value; } }
        //public bool LinearSHP { get { return _flags3[10]; } set { _flags3[10] = value; } }
        //public bool LinearLight { get { return _flags3[11]; } set { _flags3[11] = value; } }
        //public bool LinearFog { get { return _flags3[12]; } set { _flags3[12] = value; } }
        //public bool LinearCam { get { return _flags3[13]; } set { _flags3[13] = value; } }
        public bool DisplayBRRESAnims { get { return _flags3[14]; } set { _flags3[14] = value; } }
        public bool SnapToColl { get { return _flags3[15]; } set { _flags3[15] = value; } }
        public bool FlatBoneList { get { return _flags3[16]; } set { _flags3[16] = value; } }
        public bool BoneListContains { get { return _flags3[17]; } set { _flags3[17] = value; } }
        public uint RightPanelWidth { get { return _flags3[18, 10]; } set { _flags3[18, 10] = value; } }

        //flags3 28 - 31 unused

        private VoidPtr Address { get { fixed (void* p = &this)return p; } }

        public static readonly BrawlBoxViewerSettings Default = new BrawlBoxViewerSettings()
        {
            RetrieveCorrAnims = true,
            SyncTexToObj = false,
            SyncObjToVIS0 = false,
            DisableBonesOnPlay = true,
            Maximize = false,
            //_amb = new Vector4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 1.0f),
            //_diff = new Vector4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 1.0f),
            //_pos = new Vector4(100.0f, 45.0f, 45.0f, 1.0f),
            //_spec = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            //_emis = new Vector4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 1.0f),
            //_yFov = 45.0f,
            //_nearZ = 1.0f,
            //_farz = 200000.0f,
            //_zScale = 2.5f,
            //_tScale = 0.05f,
            //_rScale = 0.4f,
            //_orbColor = new ARGBPixel(255, 0, 128, 0),
            //_lineColor = new ARGBPixel(255, 0, 0, 128),
            //_floorColor = new ARGBPixel(255, 128, 128, 191),
            //_defaultCam = new BVec3(),
            //_defaultRot = new BVec2(),
            _undoCount = 50,
            ImageCapFmt = 0,
            //Bones = true,
            //Wireframe = false,
            //Polys = true,
            //Vertices = false,
            //BoundingBox = false,
            //Normals = false,
            //HideOffscreen = false,
            //ShowCamCoords = false,
            //Floor = false,
            //EnableText = false,
            //EnableSmoothing = false,
            GenTansCHR = true,
            GenTansSRT = true,
            GenTansSHP = true,
            GenTansLight = true,
            GenTansFog = true,
            GenTansCam = true,
            //OrthoCam = false,
            DisplayNonBRRESAnims = true,
            DisplayExternalAnims = true,
            DisplayBRRESAnims = true,
            //CameraSet = false,
            SnapToColl = false,
            FlatBoneList = true,
            BoneListContains = false,
        };
    }
}
