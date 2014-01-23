using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using BrawlLib.Imaging;

namespace BrawlLib.Wii.Textures
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wRGBXPixel
    {
        public byte R, G, B, X;

        public static explicit operator wRGBXPixel(ARGBPixel p) { return new wRGBXPixel() { R = p.R, G = p.G, B = p.B, X = 0 }; }
        public static explicit operator ARGBPixel(wRGBXPixel p) { return new ARGBPixel() { A = 0xFF, R = p.R, G = p.G, B = p.B }; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wRGBAPixel
    {
        public byte R, G, B, A;

        public static explicit operator wRGBAPixel(ARGBPixel p) { return new wRGBAPixel() { A = p.A, R = p.R, G = p.G, B = p.B }; }
        public static explicit operator ARGBPixel(wRGBAPixel p) { return new ARGBPixel() { A = p.A, R = p.R, G = p.G, B = p.B }; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wRGBA4Pixel
    {
        bushort _data;

        public static explicit operator ARGBPixel(wRGBA4Pixel p)
        {
            int val = p._data;
            int r = val & 0xF000; r = (r >> 8) | (r >> 12);
            int g = val & 0x0F00; g = (g >> 4) | (g >> 8);
            int b = val & 0x00F0; b |= (b >> 4);
            int a = val & 0x000F; a |= (a << 4);
            return new ARGBPixel((byte)a, (byte)r, (byte)g, (byte)b);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wRGBA6Pixel
    {
        byte _b1, _b2, _b3;

        public static explicit operator ARGBPixel(wRGBA6Pixel p)
        {
            int val = (p._b1 << 16) | (p._b2 << 8) | p._b3;
            int r = val & 0xFC0000; r = (r >> 16) | (r >> 22);
            int g = val & 0x3F000; g = (g >> 10) | (g >> 16);
            int b = val & 0xFC0; b = (b >> 4) | (b >> 10);
            int a = val & 0x3F; a = (a << 2) | (a >> 4);
            return new ARGBPixel((byte)a, (byte)r, (byte)g, (byte)b);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wRGBPixel
    {
        public byte R, G, B;

        public static explicit operator wRGBPixel(ARGBPixel p) { return new wRGBPixel() { R = p.R, G = p.G, B = p.B }; }
        public static explicit operator ARGBPixel(wRGBPixel p) { return new ARGBPixel() { A = 0xFF, R = p.R, G = p.G, B = p.B }; }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wRGB565Pixel
    {
        public bushort _data;

        public wRGB565Pixel(bushort data) { _data = data; }

        public static bool operator >(wRGB565Pixel p1, wRGB565Pixel p2) { return (ushort)p1._data > (ushort)p2._data; }
        public static bool operator <(wRGB565Pixel p1, wRGB565Pixel p2) { return (ushort)p1._data < (ushort)p2._data; }
        public static bool operator >=(wRGB565Pixel p1, wRGB565Pixel p2) { return (ushort)p1._data >= (ushort)p2._data; }
        public static bool operator <=(wRGB565Pixel p1, wRGB565Pixel p2) { return (ushort)p1._data <= (ushort)p2._data; }
        public static bool operator ==(wRGB565Pixel p1, wRGB565Pixel p2) { return p1._data._data == p2._data._data; }
        public static bool operator !=(wRGB565Pixel p1, wRGB565Pixel p2) { return p1._data._data != p2._data._data; }


        public static explicit operator ARGBPixel(wRGB565Pixel p)
        {
            int r, g, b;
            ushort val = p._data;
            r = (val & 0xF800); r = (r >> 8) | (r >> 13);
            g = (val & 0x7E0); g = (g >> 3) | (g >> 9);
            b = (val & 0x1F); b = (b << 3) | (b >> 2);
            return new ARGBPixel(0xFF, (byte)r, (byte)g, (byte)b);
        }
        public static explicit operator RGBPixel(wRGB565Pixel p)
        {
            int r, g, b;
            ushort val = p._data;
            r = (val & 0xF800); r = (r >> 8) | (r >> 13);
            g = (val & 0x7E0); g = (g >> 3) | (g >> 9);
            b = (val & 0x1F); b = (b << 3) | (b >> 2);
            return new RGBPixel() { R = (byte)r, G = (byte)g, B = (byte)b };
        }
        public static explicit operator wRGB565Pixel(ARGBPixel p)
        { return new wRGB565Pixel() { _data = (ushort)(((p.R >> 3) << 11) | ((p.G >> 2) << 5) | (p.B >> 3)) }; }
        public static explicit operator wRGB565Pixel(RGBPixel p)
        {
            return new wRGB565Pixel() { _data = (ushort)(((p.R >> 3) << 11) | ((p.G >> 2) << 5) | (p.B >> 3)) };
        }
        public static explicit operator wRGB565Pixel(Color p)
        {
            return new wRGB565Pixel() { _data = (ushort)(((p.R >> 3) << 11) | ((p.G >> 2) << 5) | (p.B >> 3)) };
        }
        public static explicit operator Color(wRGB565Pixel p)
        {
            int r, g, b;
            ushort val = p._data;
            r = val & 0xF800; r = (r >> 8) | (r >> 13);
            g = (val & 0x7E0); g = (g >> 3) | (g >> 9);
            b = (val & 0x1F); b = (b << 3) | (b >> 2);
            return Color.FromArgb(0xFF, r, g, b);
        }

        public static explicit operator wRGB565Pixel(Vector3 v) 
        {
            int r = Math.Max(Math.Min((int)(v._x * (31.0f + 0.5f)), 31), 0);
            int g = Math.Max(Math.Min((int)(v._y * (63.0f + 0.5f)), 63), 0);
            int b = Math.Max(Math.Min((int)(v._z * (31.0f + 0.5f)), 31), 0);
            return new wRGB565Pixel((ushort)((r << 11) | (g << 5) | b));
        }

        //public uint ColorData()
        //{
        //    int r, g, b;
        //    ushort val = _data;
        //    r = val & 0xF800; r = (r >> 8) | (r >> 13);
        //    g = (val & 0x7E0); g = (g >> 3) | (g >> 9);
        //    b = (val & 0x1F); b = (b << 3) | (b >> 2);
        //    return (uint)((r << 16) | (g << 8) | b);
        //}
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wRGB5A3Pixel
    {
        public bushort _data;
        public static explicit operator ARGBPixel(wRGB5A3Pixel p)
        {
            int a, r, g, b;
            ushort val = p._data;
            if ((val & 0x8000) != 0)
            {
                a = 0xFF;
                r = val & 0x7C00; r = (r >> 7) | (r >> 12);
                g = (val & 0x3E0); g = (g >> 2) | (g >> 7);
                b = (val & 0x1F); b = (b << 3) | (b >> 2);
            }
            else
            {
                a = val & 0x7000; a = (a >> 7) | (a >> 10) | (a >> 13);
                r = val & 0xF00; r = (r >> 4) | (r >> 8);
                g = val & 0xF0; g |= (g >> 4);
                b = val & 0x0F; b |= (b << 4);
            }
            return new ARGBPixel() { A = (byte)a, R = (byte)r, G = (byte)g, B = (byte)b };
        }
        public static explicit operator wRGB5A3Pixel(ARGBPixel p)
        {
            if ((p.A & 0xE0) != 0xE0) return new wRGB5A3Pixel() { _data = (ushort)(((p.A >> 5) << 12) | ((p.R >> 4) << 8) | ((p.G >> 4) << 4) | (p.B >> 4)) };
            else return new wRGB5A3Pixel() { _data = (ushort)(0x8000 | ((p.R >> 3) << 10) | ((p.G >> 3) << 5) | (p.B >> 3)) };
        }

        public static explicit operator Color(wRGB5A3Pixel p)
        {
            int a, r, g, b;
            ushort val = p._data;
            if ((val & 0x8000) != 0)
            {
                a = 0xFF;
                r = val & 0x7C00; r = (r >> 7) | (r >> 12);
                g = (val & 0x3E0); g = (g >> 2) | (g >> 7);
                b = (val & 0x1F); b = (b << 3) | (b >> 2);
            }
            else
            {
                a = val & 0x7000; a = (a >> 7) | (a >> 10) | (a >> 13);
                r = val & 0xF00; r = (r >> 4) | (r >> 8);
                g = val & 0xF0; g |= (g >> 4);
                b = val & 0x0F; b |= (b << 4);
            }
            return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
        }
        public static explicit operator wRGB5A3Pixel(Color p)
        {
            if ((p.A & 0xE0) != 0xE0) return new wRGB5A3Pixel() { _data = (ushort)(((p.A >> 5) << 12) | ((p.R >> 4) << 8) | ((p.G >> 4) << 4) | (p.B >> 4)) };
            else return new wRGB5A3Pixel() { _data = (ushort)(0x8000 | ((p.R >> 3) << 10) | ((p.G >> 3) << 5) | (p.B >> 3)) };
        }
    }
}
