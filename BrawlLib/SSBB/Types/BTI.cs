using BrawlLib.Wii.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BTIHeader
    {
        public const int Size = 0x20;

        public byte _textureFormat;
        public byte _enableAlpha;
        public bushort _height;
        public bushort _width;
        public byte _wrapS;
        public byte _wrapT;
        public byte _unk1;
        public byte _paletteFormat;
        public bushort _paletteEntryCount;
        public buint _paletteOffset;

        public buint _unk2;
        public byte _magFilter;
        public byte _minFilter;
        public bushort _unk3; //0 most of the time, sometimes 0x10, 0x18, 0x20, 0x28
        public byte _mipmapCount;
        public byte _unk4;
        public bushort _unk5;
        public buint _dataOffset;

        public WiiPixelFormat PixelFormat { get { return (WiiPixelFormat)_textureFormat; } set { _textureFormat = (byte)value; } }
        public WiiPaletteFormat PaletteFormat { get { return (WiiPaletteFormat)_paletteFormat; } set { _paletteFormat = (byte)value; } }
        
        private VoidPtr Address { get { fixed (void* p = &this)return p; } }
    }
}
