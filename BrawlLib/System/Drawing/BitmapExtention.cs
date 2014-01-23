using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.IO;
using BrawlLib.Imaging;
using BrawlLib.Wii.Textures;
using System.Windows.Forms;

namespace System.Drawing
{
    public struct ColorInformation
    {
        private ARGBPixel[] _colors;
        private int _alphaColors;
        private bool _isGreyscale;

        public ARGBPixel[] UniqueColors { get { return _colors; } }
        public int ColorCount { get { return _colors.Length; } }
        public int AlphaColors { get { return _alphaColors; } }
        public bool IsGreyscale { get { return _isGreyscale; } }

        public ColorInformation(ARGBPixel[] colors)
        {
            _colors = colors;
            _alphaColors = 0;
            _isGreyscale = true;
            foreach (ARGBPixel p in colors)
            {
                if (p.A != 0xFF) 
                    _alphaColors++;
                if ((_isGreyscale) && (!p.IsGreyscale()))
                    _isGreyscale = false;
            }
        }
    }
    public static class BitmapExtention
    {
        public static bool IsIndexed(this Bitmap bmp) { return (bmp.PixelFormat & PixelFormat.Indexed) != 0; }
        //public unsafe static int GetColorCount(this Bitmap bmp) { int alpha; bool grey; return GetColorCount(bmp, out alpha, out grey); }
        //public unsafe static int GetColorCount(this Bitmap bmp, out int alphaColors) { bool grey; return GetColorCount(bmp, out alphaColors, out grey); }
        //public unsafe static int GetColorCount(this Bitmap bmp, out int alphaColors, out bool isGreyscale)
        //{
        //    return GetUniqueColors(bmp, out alphaColors, out isGreyscale).Count;
        //}

        public static unsafe ColorInformation GetColorInformation(this Bitmap bmp)
        {
            HashSet<ARGBPixel> colors = new HashSet<ARGBPixel>();

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            for (ARGBPixel* ptr = (ARGBPixel*)data.Scan0, c = ptr + (bmp.Width * bmp.Height); ptr < c; )
                colors.Add(*ptr++);

            bmp.UnlockBits(data);

            return new ColorInformation(colors.ToArray());
        }

        public static void GreyscalePalette(this Bitmap bmp)
        {
            ColorPalette pal = bmp.Palette;

            float inc = 255.0f / pal.Entries.Length, c = 0;
            for (int i = 0, z = 0; i < pal.Entries.Length; i++, c += inc, z = (int)c)
                pal.Entries[i] = Color.FromArgb(z, z, z);

            bmp.Palette = pal;
        }
        //public static unsafe ColorPalette GeneratePalette(this Bitmap bmp, QuantizationAlgorithm mode, int numColors)
        //{
        //    ColorInformation info = bmp.GetColorInformation();

        //    ColorPalette pal = ColorPaletteExtension.CreatePalette(ColorPaletteFlags.None, numColors);
        //    if (info.UniqueColors.Length <= numColors)
        //    {
        //        //Use original colors
        //        for (int i = 0; i < info.UniqueColors.Length; i++)
        //            pal.Entries[i] = (Color)info.UniqueColors[i];
        //    }
        //    else
        //    {
        //        switch (mode)
        //        {
        //            case QuantizationAlgorithm.WeightedAverage:
        //                {
        //                    pal = WeightedAverage.Process(bmp, numColors);
        //                    break;
        //                }
        //            //case QuantizationAlgorithm.MedianCut:
        //            //    {
        //            //        MedianCut.Quantize(bmp, numColors);
        //            //        break;
        //            //    }
        //        }
        //    }
        //    return pal;
        //}

        public static Bitmap Quantize(this Bitmap bmp, QuantizationAlgorithm algorithm, int numColors, WiiPixelFormat texFormat, WiiPaletteFormat palFormat, IProgressTracker progress)
        {
            return MedianCut.Quantize(bmp, numColors, texFormat, palFormat, progress);
        }

        public static unsafe Bitmap IndexColors(this Bitmap src, ColorPalette palette, PixelFormat format)
        {
            int w = src.Width, h = src.Height;

            int entries = palette.Entries.Length;
            switch (format)
            {
                case PixelFormat.Format4bppIndexed: { entries = Math.Min(entries, 16); break; }
                case PixelFormat.Format8bppIndexed: { entries = Math.Min(entries, 256); break; }
                default: { throw new ArgumentException("Pixel format is not an indexed format."); }
            }

            Bitmap dst = new Bitmap(w, h, format);
            dst.Palette = palette;

            BitmapData sData = src.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dData = dst.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, format);

            ARGBPixel* sPtr = (ARGBPixel*)sData.Scan0;
            for (int y = 0; y < h; y++)
            {
                byte* dPtr = (byte*)dData.Scan0 + (y * dData.Stride);
                for (int x = 0; x < w; x++)
                {
                    ARGBPixel p = *sPtr++;

                    int bestDist = Int32.MaxValue, bestIndex = 0;
                    for (int z = 0; z < entries; z++)
                    {
                        int dist = p.DistanceTo(palette.Entries[z]);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestIndex = z;
                        }
                    }

                    if (format == PixelFormat.Format4bppIndexed)
                    {
                        byte val = *dPtr;
                        if ((x % 2) == 0)
                            *dPtr = (byte)((bestIndex << 4) | (val & 0x0F));
                        else
                            *dPtr++ = (byte)((val & 0xF0) | (bestIndex & 0x0F));
                    }
                    else
                        *dPtr++ = (byte)bestIndex;
                }
            }


            dst.UnlockBits(dData);
            src.UnlockBits(sData);

            return dst;
        }
        public static unsafe void Clamp(this Bitmap bmp, ColorPalette palette)
        {
            int w = bmp.Width, h = bmp.Height, e = palette.Entries.Length;
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            for (ARGBPixel* ptr = (ARGBPixel*)data.Scan0, ceil = ptr + (w * h); ptr < ceil; )
            {
                *ptr = (ARGBPixel)palette.Entries[palette.FindMatch(*ptr++)];

                //ARGBPixel p = *ptr;
                //int bestDist = Int32.MaxValue, bestIndex = 0;
                //for(int i = 0 ; i < e ; i++)
                //{
                //    int dist = p.DistanceTo(palette.Entries[i]);
                //    if(dist < bestDist)
                //    {
                //        bestDist = dist;
                //        bestIndex = i;
                //        if (dist == 0) break;
                //    }
                //}
                //*ptr++ = (ARGBPixel)palette.Entries[bestIndex];
            }
            bmp.UnlockBits(data);
        }

        private unsafe delegate void PixelClamper(ARGBPixel* src);
        public static unsafe void Clamp(this Bitmap bmp, WiiPixelFormat format)
        {
            int w = bmp.Width, h = bmp.Height;
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            PixelClamper clmp;
            switch (format)
            {
                case WiiPixelFormat.I4: { clmp = p => { *p = ((I4Pixel)(*p))[0]; }; break; }
                case WiiPixelFormat.I8: { clmp = p => { *p = (ARGBPixel)(I8Pixel)(*p); }; break; }
                case WiiPixelFormat.IA4: { clmp = p => { *p = (ARGBPixel)(IA4Pixel)(*p); }; break; }
                case WiiPixelFormat.IA8: { clmp = p => { *p = (ARGBPixel)(IA8Pixel)(*p); }; break; }
                case WiiPixelFormat.RGB565: { clmp = p => { *p = (ARGBPixel)(wRGB565Pixel)(*p); }; break; }
                case WiiPixelFormat.RGB5A3: { clmp = p => { *p = (ARGBPixel)(wRGB5A3Pixel)(*p); }; break; }
                case WiiPixelFormat.RGBA8:
                default: { clmp = p => { }; break; }
            }

            for (ARGBPixel* ptr = (ARGBPixel*)data.Scan0, ceil = ptr + (w * h); ptr < ceil; )
                clmp(ptr++);

            bmp.UnlockBits(data);
        }

        public static unsafe void CopyTo(this Bitmap bmp, Bitmap dest)
        {
            int w = Math.Min(bmp.Width, dest.Width);
            int h = Math.Min(bmp.Height, dest.Height);
            using (DIB dib = new DIB(w, h, dest.PixelFormat))
            {
                dib.ReadBitmap(bmp, w, h);
                dib.WriteBitmap(dest, w, h);
            }
        }

        public static unsafe Bitmap Clone(this Bitmap src, int width, int height, int skip)
        {
            int sw = src.Width, sh = src.Height;
            PixelFormat format = src.PixelFormat;
            Bitmap dst = new Bitmap(width, height, format);

            BitmapData srcData = src.LockBits(new Rectangle(0, 0, sw, sh), ImageLockMode.ReadOnly, format);
            BitmapData dstData = dst.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, format);

            switch (format)
            {
                case PixelFormat.Format4bppIndexed:
                    {
                        for (int sy = skip / 2, dy = 0; (sy < sh) && (dy < height); dy++, sy += skip)
                        {
                            byte* sPtr = (byte*)srcData.Scan0 + (sy * srcData.Stride);
                            byte* dPtr = (byte*)dstData.Scan0 + (dy * dstData.Stride);
                            for (int sx = skip / 2, dx = 0; (sx < sw) && (dx < width); dx++, sx += skip)
                            {
                                byte value = (sx % 2 == 0) ? (byte)(sPtr[sx >> 1] >> 4) : (byte)(sPtr[sx >> 1] & 0x0F);
                                dPtr[dx >> 1] = (dx % 2 == 0) ? (byte)((value << 4) | (dPtr[dx >> 1] & 0x0F)) : (byte)((dPtr[dx >> 1] & 0xF0) | value);
                            }
                        }
                        break;
                    }
            }

            dst.UnlockBits(dstData);
            src.UnlockBits(srcData);

            return dst;
        }
        public static unsafe Bitmap GenerateMip(this Bitmap bmp, int level)
        {
            if (level <= 1)
                return (Bitmap)bmp.Clone();

            int scale = 1 << (level - 1);
            int w = bmp.Width / scale, h = bmp.Height / scale;

            Bitmap dst = new Bitmap(w, h, bmp.PixelFormat);

            //Step-scale indexed elements
            if (bmp.IsIndexed())
            {
                BitmapData srcData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                BitmapData dstData = dst.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, bmp.PixelFormat);

                float xStep = (float)bmp.Width / w;
                float yStep = (float)bmp.Height / h;
                int x, y;
                float fx, fy;

                byte* sPtr, dPtr = (byte*)dstData.Scan0;
                if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    for (y = 0, fy = 0.5f; y < h; y++, fy += yStep, dPtr += dstData.Stride)
                    {
                        sPtr = (byte*)srcData.Scan0 + ((int)fy * srcData.Stride);
                        for (x = 0, fx = 0.5f; x < w; x++, fx += xStep)
                            dPtr[x] = sPtr[(int)fx];
                    }
                }
                else
                {
                    for (y = 0, fy = 0.5f; y < h; y++, fy += yStep, dPtr += dstData.Stride)
                    {
                        sPtr = (byte*)srcData.Scan0 + ((int)fy * srcData.Stride);
                        int b = 0, ind;
                        for (x = 0, fx = 0.5f; x < w; x++, fx += xStep)
                        {
                            ind = (int)fx;
                            if ((x & 1) == 0)
                            {
                                if ((ind & 1) == 0)
                                    b = sPtr[ind >> 1] & 0xF0;
                                else
                                    b = sPtr[ind >> 1] << 4;
                            }
                            else
                            {
                                if ((ind & 1) == 0)
                                    b |= sPtr[ind >> 1] >> 4;
                                else
                                    b |= sPtr[ind >> 1] & 0xF;
                                dPtr[x >> 1] = (byte)b;
                            }
                        }
                        if ((x & 1) != 0)
                            dPtr[x >> 1] = (byte)b;
                    }

                }

                bmp.UnlockBits(srcData);
                dst.UnlockBits(dstData);
            }
            else
            {
                using (Graphics g = Graphics.FromImage(dst))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    g.DrawImage(bmp, new Rectangle(0, 0, w, h));
                }
            }
            return dst;
        }

        public static void SaveTGA(this Bitmap bmp, string path)
        {
            TGA.ToFile(bmp, path);
        }
        public static void SaveTGA(this Bitmap bmp, FileStream stream)
        {
            TGA.ToStream(bmp, stream);
        }
    }
    public enum QuantizationAlgorithm
    {
        //WeightedAverage
        MedianCut
    }
}
