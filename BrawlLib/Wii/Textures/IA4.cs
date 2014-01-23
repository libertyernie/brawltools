using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using BrawlLib.Imaging;

namespace BrawlLib.Wii.Textures
{
    public unsafe class IA4 : TextureConverter
    {
        public override int BitsPerPixel { get { return 8; } }
        public override int BlockWidth { get { return 8; } }
        public override int BlockHeight { get { return 4; } }
        //public override PixelFormat DecodedFormat { get { return PixelFormat.Format32bppArgb; } }
        public override WiiPixelFormat RawFormat { get { return WiiPixelFormat.IA4; } }

        protected override void DecodeBlock(VoidPtr blockAddr, ARGBPixel* dPtr, int width)
        {
            IA4Pixel* sPtr = (IA4Pixel*)blockAddr;
            //ARGBPixel* dPtr = (ARGBPixel*)destAddr;
            for (int y = 0; y < BlockHeight; y++, dPtr += width)
                for (int x = 0; x < BlockWidth; )
                    dPtr[x++] = *sPtr++;
        }

        protected override void EncodeBlock(ARGBPixel* sPtr, VoidPtr blockAddr, int width)
        {
            IA4Pixel* dPtr = (IA4Pixel*)blockAddr;
            for (int y = 0; y < BlockHeight; y++, sPtr += width)
                for (int x = 0; x < BlockWidth; )
                    *dPtr++ = sPtr[x++];
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct IA4Pixel
    {
        public byte data;

        public byte Intensity
        {
            get { return (byte)((data >> 4) | (data & 0xF0)); }
            set { data = (byte)((data & 0x0F) | (value << 4)); }
        }

        public byte Alpha
        {
            get { return (byte)((data << 4) | (data & 0x0F)); }
            set { data = (byte)((data & 0xF0) | (value & 0x0F)); }
        }

        public static implicit operator ARGBPixel(IA4Pixel p)
        {
            byte i = p.Intensity;
            return new ARGBPixel() { A = p.Alpha, R = i, G = i, B = i };
        }
        public static implicit operator IA4Pixel(ARGBPixel p)
        {
            return new IA4Pixel() { data = (byte)((((p.R + p.G + p.B) / 3) & 0xF0) | (p.A >> 4)) };
        }
    }
}
