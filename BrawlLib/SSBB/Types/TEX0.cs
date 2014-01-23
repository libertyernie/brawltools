using System;
using System.Runtime.InteropServices;
using BrawlLib.Wii.Textures;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct TEX0v1
    {
        public const int Size = 0x40;
        public const uint Tag = 0x30584554;

        public BRESCommonHeader _header;
        public bint _headerLen;
        public bint _stringOffset;
        public bint _hasPalette;
        public bshort _width;
        public bshort _height;
        public bint _pixelFormat;
        public bint _levelOfDetail;
        public bfloat _minLod;
        public bfloat _maxLod;
        public bint _origPathOffset;

        internal VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public string OrigPath { get { return new String((sbyte*)OrigPathAddress); } }
        public VoidPtr OrigPathAddress
        {
            get { return Address + _origPathOffset; }
            set { _origPathOffset = (int)value - (int)Address; }
        }
        public string ResourceString { get { return new String((sbyte*)this.ResourceStringAddress); } }
        public VoidPtr ResourceStringAddress
        {
            get { return (VoidPtr)this.Address + _stringOffset; }
            set { _stringOffset = (int)value - (int)Address; }
        }
        public VoidPtr PixelData { get { return (VoidPtr)Address + _headerLen; } }
        public WiiPixelFormat PixelFormat
        {
            get { return (WiiPixelFormat)(int)_pixelFormat; }
            set { _pixelFormat = (int)value; }
        }
        public bool HasPalette
        {
            get { return _hasPalette != 0; }
            set { _hasPalette = (value) ? 1 : 0; }
        }

        public TEX0v1(int width, int height, WiiPixelFormat format, int mipLevels)
        {
            _header._tag = Tag;
            _header._size = TextureConverter.Get(format).GetMipOffset(width, height, mipLevels + 1) + Size;
            _header._version = 1;
            _header._bresOffset = 0;

            _headerLen = Size;
            _stringOffset = 0;
            _hasPalette = ((format == WiiPixelFormat.CI4) || (format == WiiPixelFormat.CI8)) ? 1 : 0;
            _width = (short)width;
            _height = (short)height;
            _pixelFormat = (int)format;
            _levelOfDetail = mipLevels;
            _minLod = 0;
            _maxLod = mipLevels - 1.0f;
            _origPathOffset = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct TEX0v3
    {
        public const int Size = 0x44;
        public const uint Tag = 0x30584554;

        public BRESCommonHeader _header;
        public bint _headerLen;
        public bint _stringOffset;
        public bint _hasPalette;
        public bshort _width;
        public bshort _height;
        public bint _pixelFormat;
        public bint _levelOfDetail;
        public bfloat _minLod;
        public bfloat _maxLod;
        public bint _origPathOffset;
        public bint _userDataOffset;

        //User Data comes before texture data. Align to 0x20

        internal VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public string OrigPath { get { return new String((sbyte*)OrigPathAddress); } }
        public VoidPtr OrigPathAddress
        {
            get { return Address + _origPathOffset; }
            set { _origPathOffset = (int)value - (int)Address; }
        }
        public UserData* UserData
        {
            get { return (UserData*)(Address + _userDataOffset); }
            set { _userDataOffset = (int)(VoidPtr)value - (int)Address; }
        }

        public string ResourceString { get { return new String((sbyte*)this.ResourceStringAddress); } }
        public VoidPtr ResourceStringAddress
        {
            get { return (VoidPtr)this.Address + _stringOffset; }
            set { _stringOffset = (int)value - (int)Address; }
        }
        public VoidPtr PixelData { get { return (VoidPtr)Address + _headerLen; } }
        public WiiPixelFormat PixelFormat
        {
            get { return (WiiPixelFormat)(int)_pixelFormat; }
            set { _pixelFormat = (int)value; }
        }
        public bool HasPalette
        {
            get { return _hasPalette != 0; }
            set { _hasPalette = (value) ? 1 : 0; }
        }

        public TEX0v3(int width, int height, WiiPixelFormat format, int mipLevels)
        {
            _header._tag = Tag;
            _header._size = TextureConverter.Get(format).GetMipOffset(width, height, mipLevels + 1) + Size;
            _header._version = 3;
            _header._bresOffset = 0;

            _headerLen = Size;
            _stringOffset = 0;
            _hasPalette = ((format == WiiPixelFormat.CI4) || (format == WiiPixelFormat.CI8)) ? 1 : 0;
            _width = (short)width;
            _height = (short)height;
            _pixelFormat = (int)format;
            _levelOfDetail = mipLevels;
            _minLod = 0;
            _maxLod = mipLevels - 1.0f;
            _origPathOffset = 0;
            _userDataOffset = 0;
        }
    }
}
