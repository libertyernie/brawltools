using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using BrawlLib.Imaging;
using BrawlLib.Wii.Graphics;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SCN0v4
    {
        public const uint Tag = 0x304E4353;
        public const int Size = 0x44;

        public BRESCommonHeader _header;
        public bint _dataOffset;
        public bint _lightSetOffset;
        public bint _ambLightOffset;
        public bint _lightOffset;
        public bint _fogOffset;
        public bint _cameraOffset;
        public bint _stringOffset;
        public bint _origPathOffset;
        public bshort _frameCount;
        public bshort _specLightCount;
        public bint _loop;
        public bshort _lightSetCount;
        public bshort _ambientCount;
        public bshort _lightCount;
        public bshort _fogCount;
        public bshort _cameraCount;
        public bshort _pad;

        public void Set(int groupLen, int lightSetLen, int ambLightLen, int lightLen, int fogLen, int cameraLen)
        {
            _dataOffset = Size;

            _header._tag = Tag;
            _header._version = 4;
            _header._bresOffset = 0;

            _lightSetOffset = _dataOffset + groupLen;
            _ambLightOffset = _lightSetOffset + lightSetLen;
            _lightOffset = _ambLightOffset + ambLightLen;
            _fogOffset = _lightOffset + lightLen;
            _cameraOffset = _fogOffset + fogLen;
            _header._size = _cameraOffset + cameraLen;

            if (lightSetLen == 0) _lightSetOffset = 0;
            if (ambLightLen == 0) _ambLightOffset = 0;
            if (lightLen == 0) _lightOffset = 0;
            if (fogLen == 0) _fogOffset = 0;
            if (cameraLen == 0) _cameraOffset = 0;
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public ResourceGroup* Group { get { return (ResourceGroup*)(Address + _dataOffset); } }

        public SCN0LightSet* LightSets { get { return (SCN0LightSet*)(Address + _lightSetOffset); } }
        public SCN0AmbientLight* AmbientLights { get { return (SCN0AmbientLight*)(Address + _ambLightOffset); } }
        public SCN0Light* Lights { get { return (SCN0Light*)(Address + _lightOffset); } }
        public SCN0Fog* Fogs { get { return (SCN0Fog*)(Address + _fogOffset); } }
        public SCN0Camera* Cameras { get { return (SCN0Camera*)(Address + _cameraOffset); } }

        public string ResourceString { get { return new String((sbyte*)this.ResourceStringAddress); } }
        public VoidPtr ResourceStringAddress
        {
            get { return (VoidPtr)this.Address + _stringOffset; }
            set { _stringOffset = (int)value - (int)Address; }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SCN0v5
    {
        public const uint Tag = 0x304E4353;
        public const int Size = 0x44;

        public BRESCommonHeader _header;
        public bint _dataOffset;
        public bint _lightSetOffset;
        public bint _ambLightOffset;
        public bint _lightOffset;
        public bint _fogOffset;
        public bint _cameraOffset;
        public bint _userDataOffset;
        public bint _stringOffset;
        public bint _origPathOffset;
        public bshort _frameCount;
        public bshort _specLightCount;
        public bint _loop;
        public bshort _lightSetCount;
        public bshort _ambientCount;
        public bshort _lightCount;
        public bshort _fogCount;
        public bshort _cameraCount;
        public bshort _pad;

        public void Set(int groupLen, int lightSetLen, int ambLightLen, int lightLen, int fogLen, int cameraLen)
        {
            _dataOffset = Size;

            _header._tag = Tag;
            _header._version = 5;
            _header._bresOffset = 0;

            _lightSetOffset = _dataOffset + groupLen;
            _ambLightOffset = _lightSetOffset + lightSetLen;
            _lightOffset = _ambLightOffset + ambLightLen;
            _fogOffset = _lightOffset + lightLen;
            _cameraOffset = _fogOffset + fogLen;
            _header._size = _cameraOffset + cameraLen;

            if (lightSetLen == 0) _lightSetOffset = 0;
            if (ambLightLen == 0) _ambLightOffset = 0;
            if (lightLen == 0) _lightOffset = 0;
            if (fogLen == 0) _fogOffset = 0;
            if (cameraLen == 0) _cameraOffset = 0;
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public ResourceGroup* Group { get { return (ResourceGroup*)(Address + _dataOffset); } }
        
        public VoidPtr UserData
        {
            get { return _userDataOffset == 0 ? null : Address + _userDataOffset; }
            set { _userDataOffset = (int)value - (int)Address; }
        }

        public SCN0LightSet* LightSets { get { return (SCN0LightSet*)(Address + _lightSetOffset); } }
        public SCN0AmbientLight* AmbientLights { get { return (SCN0AmbientLight*)(Address + _ambLightOffset); } }
        public SCN0Light* Lights { get { return (SCN0Light*)(Address + _lightOffset); } }
        public SCN0Fog* Fogs { get { return (SCN0Fog*)(Address + _fogOffset); } }
        public SCN0Camera* Cameras { get { return (SCN0Camera*)(Address + _cameraOffset); } }

        public string ResourceString { get { return new String((sbyte*)this.ResourceStringAddress); } }
        public VoidPtr ResourceStringAddress
        {
            get { return (VoidPtr)this.Address + _stringOffset; }
            set { _stringOffset = (int)value - (int)Address; }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SCN0CommonHeader
    {
        public const int Size = 0x14;

        public bint _length;
        public bint _scn0Offset;
        public bint _stringOffset;
        public bint _nodeIndex;
        public bint _realIndex;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public string ResourceString { get { return new String((sbyte*)ResourceStringAddress); } }
        public VoidPtr ResourceStringAddress
        {
            get { return (VoidPtr)Address + _stringOffset; }
            set { _stringOffset = (int)value - (int)Address; }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SCN0LightSet
    {
        public const int Size = 76;

        public SCN0CommonHeader _header;

        public bint _ambNameOffset;
        public bshort _id; //ambient set here as id at runtime
        public byte _numLights;
        public byte _pad;
        public fixed int _entries[8]; //string offsets
        public fixed short _lightIds[8]; //entry ids are set here at runtime
        
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
        public bint* Offsets { get { fixed (void* ptr = _entries)return (bint*)ptr; } }

        public string AmbientString { get { return new String((sbyte*)AmbientStringAddress); } }
        public VoidPtr AmbientStringAddress
        {
            get { return (VoidPtr)Address + _ambNameOffset; }
            set { _ambNameOffset = (int)value - (int)Address; }
        }

        public bint* StringOffsets { get { return (bint*)(Address + 0x1C); } }
        public bshort* IDs { get { return (bshort*)(Address + 0x3C); } }
    }

    [Flags]
    public enum SCN0AmbLightFixedFlags
    {
        None = 0,
        FixedLighting = 128,
    }

    [Flags]
    public enum SCN0AmbLightFlags
    {
        None = 0,
        ColorEnabled = 1,
        AlphaEnabled = 2
    }

    [Flags]
    public enum SCN0LightsKeyframes
    {
        FixedX = 0,
        FixedY = 1,
        FixedZ = 2
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SCN0AmbientLight
    {
        public const int Size = 28;

        public SCN0CommonHeader _header;

        public byte _fixedFlags;
        public byte _pad1;
        public byte _pad2;
        public byte _flags;

        public RGBAPixel _lighting;

        public RGBAPixel* lightEntries { get { return (RGBAPixel*)((byte*)Address + 24 + *(bint*)((byte*)Address + 24)); } }
        
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [Flags]
    public enum UsageFlags : ushort
    {
        //Enabled = 0x1,
        SpecularEnabled = 0x2, //Use NonSpecLightId, SpecularColor, Brightness
        ColorEnabled = 0x4,
        AlphaEnabled = 0x8,
    }

    [Flags]
    public enum LightType : ushort
    {
        Point = 0x0,
        Directional = 0x1, //Don't use distFunc, refDistance, refBrightness
        Spotlight = 0x2, //Use cutoff
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SCN0Light
    {
        public const int Size = 92;

        public SCN0CommonHeader _header;

        public bint _nonSpecLightId;
        public bint _part2Offset;

        public bushort _fixedFlags;
        public bushort _usageFlags;

        public bint _visOffset; 
        public byte* visBitEntries { get { return (byte*)_visOffset.Address + _visOffset; } }

        public BVec3 _startPoint;
        public SCN0KeyframesHeader* xStartKeyframes { get { return (SCN0KeyframesHeader*)(_startPoint._x.Address + *(bint*)_startPoint._x.Address); } }
        public SCN0KeyframesHeader* yStartKeyframes { get { return (SCN0KeyframesHeader*)(_startPoint._y.Address + *(bint*)_startPoint._y.Address); } }
        public SCN0KeyframesHeader* zStartKeyframes { get { return (SCN0KeyframesHeader*)(_startPoint._z.Address + *(bint*)_startPoint._z.Address); } }

        public RGBAPixel _lightColor;
        public RGBAPixel* lightColorEntries { get { return (RGBAPixel*)(_lightColor.Address + *(bint*)_lightColor.Address); } }

        public BVec3 _endPoint;
        public SCN0KeyframesHeader* xEndKeyframes { get { return (SCN0KeyframesHeader*)(_endPoint._x.Address + *(bint*)_endPoint._x.Address); } }
        public SCN0KeyframesHeader* yEndKeyframes { get { return (SCN0KeyframesHeader*)(_endPoint._y.Address + *(bint*)_endPoint._y.Address); } }
        public SCN0KeyframesHeader* zEndKeyframes { get { return (SCN0KeyframesHeader*)(_endPoint._z.Address + *(bint*)_endPoint._z.Address); } }

        public bint _distFunc;

        public bfloat _refDistance;
        public SCN0KeyframesHeader* refDistanceKeyframes { get { return (SCN0KeyframesHeader*)(_refDistance.Address + *(bint*)_refDistance.Address); } }

        public bfloat _refBrightness;
        public SCN0KeyframesHeader* refBrightnessKeyframes { get { return (SCN0KeyframesHeader*)(_refBrightness.Address + *(bint*)_refBrightness.Address); } }
        
        public bint _spotFunc;

        public bfloat _cutoff;
        public SCN0KeyframesHeader* cutoffKeyframes { get { return (SCN0KeyframesHeader*)(_cutoff.Address + *(bint*)_cutoff.Address); } }

        public RGBAPixel _specularColor;
        public RGBAPixel* specColorEntries { get { return (RGBAPixel*)(_specularColor.Address + *(bint*)_specularColor.Address); } }
        
        public bfloat _shininess;
        public SCN0KeyframesHeader* shininessKeyframes { get { return (SCN0KeyframesHeader*)(_shininess.Address + *(bint*)_shininess.Address); } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SCN0Fog
    {
        public const int Size = 40;

        public SCN0CommonHeader _header;
        
        public byte _flags;
        public BUInt24 _pad;
        public bint _type;

        public bfloat _start;
        public SCN0KeyframesHeader* startKeyframes { get { return (SCN0KeyframesHeader*)((byte*)Address + 28 + *(bint*)((byte*)Address + 28)); } }

        public bfloat _end;
        public SCN0KeyframesHeader* endKeyframes { get { return (SCN0KeyframesHeader*)((byte*)Address + 32 + *(bint*)((byte*)Address + 32)); } }

        public RGBAPixel _color;
        public RGBAPixel* colorEntries { get { return (RGBAPixel*)((byte*)Address + 36 + *(bint*)((byte*)Address + 36)); } }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [Flags]
    public enum SCN0FogFlags
    {
        None = 0,
        FixedStart = 0x20,
        FixedEnd = 0x40,
        FixedColor = 0x80
    }

    [Flags]
    public enum SCN0CameraFlags
    {
        PosXConstant = 0x2,
        PosYConstant = 0x4,
        PosZConstant = 0x8,
        AspectConstant = 0x10,
        NearConstant = 0x20,
        FarConstant = 0x40,
        PerspFovYConstant = 0x80,
        OrthoHeightConstant = 0x100,
        AimXConstant = 0x200,
        AimYConstant = 0x400,
        AimZConstant = 0x800,
        TwistConstant = 0x1000,
        RotXConstant = 0x2000,
        RotYConstant = 0x4000,
        RotZConstant = 0x8000,
    }

    [Flags]
    public enum SCN0CameraFlags2
    {
        None = 0,
        AlwaysOn = 2,
        CameraTypeMask = 1
    }

    public enum SCN0CameraType
    {
        Rotate = 0,
        Aim = 1,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SCN0Camera
    {
        public const int Size = 92;

        public SCN0CommonHeader _header;

        public bint _projType;
        public bushort _flags1;
        public bushort _flags2;
        public bint _userDataOffset;

        public VoidPtr UserData
        {
            get { return _userDataOffset == 0 ? null : Address + _userDataOffset; }
            set { _userDataOffset = (int)value - (int)Address; }
        }

        public BVec3 _position;
        public SCN0KeyframesHeader* posXKeyframes { get { return (SCN0KeyframesHeader*)(_position._x.Address + *(bint*)_position._x.Address); } }
        public SCN0KeyframesHeader* posYKeyframes { get { return (SCN0KeyframesHeader*)(_position._y.Address + *(bint*)_position._y.Address); } }
        public SCN0KeyframesHeader* posZKeyframes { get { return (SCN0KeyframesHeader*)(_position._z.Address + *(bint*)_position._z.Address); } }

        public bfloat _aspect;
        public bfloat _nearZ;
        public bfloat _farZ;
        public SCN0KeyframesHeader* aspectKeyframes { get { return (SCN0KeyframesHeader*)(_aspect.Address + *(bint*)_aspect.Address); } }
        public SCN0KeyframesHeader* nearZKeyframes { get { return (SCN0KeyframesHeader*)(_nearZ.Address + *(bint*)_nearZ.Address); } }
        public SCN0KeyframesHeader* farZKeyframes { get { return (SCN0KeyframesHeader*)(_farZ.Address + *(bint*)_farZ.Address); } }
        
        public BVec3 _rotate;
        public SCN0KeyframesHeader* rotXKeyframes { get { return (SCN0KeyframesHeader*)(_rotate._x.Address + *(bint*)_rotate._x.Address); } }
        public SCN0KeyframesHeader* rotYKeyframes { get { return (SCN0KeyframesHeader*)(_rotate._y.Address + *(bint*)_rotate._y.Address); } }
        public SCN0KeyframesHeader* rotZKeyframes { get { return (SCN0KeyframesHeader*)(_rotate._z.Address + *(bint*)_rotate._z.Address); } }

        public BVec3 _aim;
        public SCN0KeyframesHeader* aimXKeyframes { get { return (SCN0KeyframesHeader*)(_aim._x.Address + *(bint*)_aim._x.Address); } }
        public SCN0KeyframesHeader* aimYKeyframes { get { return (SCN0KeyframesHeader*)(_aim._y.Address + *(bint*)_aim._y.Address); } }
        public SCN0KeyframesHeader* aimZKeyframes { get { return (SCN0KeyframesHeader*)(_aim._z.Address + *(bint*)_aim._z.Address); } }

        public bfloat _twist;
        public bfloat _perspFovY;
        public bfloat _orthoHeight;
        public SCN0KeyframesHeader* twistKeyframes { get { return (SCN0KeyframesHeader*)(_twist.Address + *(bint*)_twist.Address); } }
        public SCN0KeyframesHeader* fovYKeyframes { get { return (SCN0KeyframesHeader*)(_perspFovY.Address + *(bint*)_perspFovY.Address); } }
        public SCN0KeyframesHeader* heightKeyframes { get { return (SCN0KeyframesHeader*)(_orthoHeight.Address + *(bint*)_orthoHeight.Address); } }
        
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SCN0KeyframesHeader
    {
        public const int Size = 4;

        public bushort _numFrames;
        public bushort _unk;

        public SCN0KeyframesHeader(int entries)
        {
            _numFrames = (ushort)entries;
            _unk = 0;
        }

        private VoidPtr Address { get { fixed (void* p = &this)return p; } }
        public SCN0KeyframeStruct* Data { get { return (SCN0KeyframeStruct*)(Address + Size); } }
    }

    public struct SCN0KeyframeStruct
    {
        public bfloat _tangent, _index, _value;

        public static implicit operator SCN0Keyframe(SCN0KeyframeStruct v) { return new SCN0Keyframe(v._tangent, v._index, v._value); }
        public static implicit operator SCN0KeyframeStruct(SCN0Keyframe v) { return new SCN0KeyframeStruct(v._tangent, v._index, v._value); }

        public SCN0KeyframeStruct(float tan, float index, float value) { _index = index; _value = value; _tangent = tan; }

        public float Index { get { return _index; } set { _index = value; } }
        public float Value { get { return _value; } set { _value = value; } }
        public float Tangent { get { return _tangent; } set { _tangent = value; } }

        public override string ToString()
        {
            return String.Format("Tangent={0}, Index={1}, Value={2}", _tangent, _index, _value);
        }
    }

    public class SCN0Keyframe
    {
        public float _tangent, _index, _value;
        
        public static implicit operator SCN0Keyframe(Vector3 v) { return new SCN0Keyframe(v._x, v._y, v._z); }
        public static implicit operator Vector3(SCN0Keyframe v) { return new Vector3(v._tangent, v._index, v._value); }

        public SCN0Keyframe(float tan, float index, float value) { _index = index; _value = value; _tangent = tan; }
        public SCN0Keyframe() { }

        public float Index { get { return _index; } set { _index = value; } }
        public float Value { get { return _value; } set { _value = value; } }
        public float Tangent { get { return _tangent; } set { _tangent = value; } }

        public override string ToString()
        {
            return String.Format("Tangent={0}, Index={1}, Value={2}", _tangent, _index, _value);
        }
    }
}
