using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Wii.Textures;
using BrawlLib.Imaging;
using System.Drawing;
using System.Collections.Generic;
using BrawlLib.IO;
using System.Drawing.Imaging;
using System.IO;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class TEX0Node : BRESEntryNode, IImageSource
    {
        internal TEX0v1* Header1 { get { return (TEX0v1*)WorkingUncompressed.Address; } }
        internal TEX0v2* Header2 { get { return (TEX0v2*)WorkingUncompressed.Address; } }
        internal TEX0v3* Header3 { get { return (TEX0v3*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.TEX0; } }
        public override int DataAlign { get { return 0x20; } }
        public override int[] SupportedVersions { get { return new int[] { 1, 2, 3 }; } }

        public TEX0Node() { _version = 1; }

        int _width, _height;
        WiiPixelFormat _format;
        int _lod;
        bool _hasPalette;

        [Category("G3D Texture")]
        public int Width { get { return _width; } }
        [Category("G3D Texture")]
        public int Height { get { return _height; } }
        [Category("G3D Texture")]
        public WiiPixelFormat Format { get { return _format; } }
        [Category("G3D Texture")]
        public int LevelOfDetail { get { return _lod; } }
        [Category("G3D Texture")]
        public bool HasPalette { get { return _hasPalette; } }

        public PLT0Node GetPaletteNode() { return ((_parent == null) || (!HasPalette)) ? null : Parent._parent.FindChild("Palettes(NW4R)/" + this.Name, false) as PLT0Node; }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            if (_version == 2)
            {
                if ((_name == null) && (Header2->_stringOffset != 0))
                    _name = Header2->ResourceString;

                _width = Header2->_width;
                _height = Header2->_height;
                _format = Header2->PixelFormat;
                _lod = Header2->_levelOfDetail;
                _hasPalette = Header2->HasPalette;
            }
            else
            {
                if ((_name == null) && (Header1->_stringOffset != 0))
                    _name = Header1->ResourceString;

                _width = Header1->_width;
                _height = Header1->_height;
                _format = Header1->PixelFormat;
                _lod = Header1->_levelOfDetail;
                _hasPalette = Header1->HasPalette;
            }

            if (_version == 3)
                (_userEntries = new UserDataCollection()).Read(Header3->UserData);

            return false;
        }

        internal override void GetStrings(StringTable table)
        {
            table.Add(Name);

            if (_version == 3)
                _userEntries.GetStrings(table);

            if (!String.IsNullOrEmpty(_originalPath))
                table.Add(_originalPath);
        }

        [Browsable(false)]
        public int ImageCount { get { return _lod; } }
        public Bitmap GetImage(int index)
        {
            PLT0Node plt = GetPaletteNode();
            return GetImage(index, plt);
        }

        public Bitmap GetImage(int index, PLT0Node plt)
        {
            try
            {
                if (WorkingUncompressed != DataSource.Empty)
                    if (plt != null)
                        return TextureConverter.DecodeIndexed(
                            (VoidPtr)CommonHeader + 0x40, _width, _height, plt.Palette, index + 1, _format);
                    else
                        return TextureConverter.Decode(
                            (VoidPtr)CommonHeader + 0x40, _width, _height, index + 1, _format);
                else
                    return null;
            }
            catch { return null; }
        }

        protected internal override void PostProcess(VoidPtr bresAddress, VoidPtr dataAddress, int dataLength, StringTable stringTable)
        {
            base.PostProcess(bresAddress, dataAddress, dataLength, stringTable);

            if (_version == 2)
            {
                TEX0v2* header = (TEX0v2*)dataAddress;
                header->ResourceStringAddress = stringTable[Name] + 4;
                if (!String.IsNullOrEmpty(_originalPath))
                    header->OrigPathAddress = stringTable[_originalPath] + 4;
            }
            else
            {
                TEX0v1* header = (TEX0v1*)dataAddress;
                header->ResourceStringAddress = stringTable[Name] + 4;
                if (!String.IsNullOrEmpty(_originalPath))
                    header->OrigPathAddress = stringTable[_originalPath] + 4;
            }
        }

        public void Replace(Bitmap bmp)
        {
            FileMap tMap, pMap;
            if (HasPalette)
            {
                PLT0Node pn = GetPaletteNode();
                tMap = TextureConverter.Get(Format).EncodeTextureIndexed(bmp, LevelOfDetail, pn.Colors, pn.Format, QuantizationAlgorithm.MedianCut, out pMap);
                pn.ReplaceRaw(pMap);
            }
            else
                tMap = TextureConverter.Get(Format).EncodeTEX0Texture(bmp, LevelOfDetail);
            ReplaceRaw(tMap);
        }

        public override unsafe void Replace(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            Bitmap bmp;
            if (String.Equals(ext, ".tga", StringComparison.OrdinalIgnoreCase))
                bmp = TGA.FromFile(fileName);
            else if (
                String.Equals(ext, ".png", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(ext, ".tif", StringComparison.OrdinalIgnoreCase) || 
                String.Equals(ext, ".tiff", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(ext, ".bmp", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(ext, ".jpg", StringComparison.OrdinalIgnoreCase) || 
                String.Equals(ext, ".jpeg", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(ext, ".gif", StringComparison.OrdinalIgnoreCase))
                bmp = (Bitmap)Bitmap.FromFile(fileName);
            else
            {
                base.Replace(fileName);
                return;
            }

            using (Bitmap b = bmp)
                Replace(b);
        }

        public override void Export(string outPath)
        {
            if (outPath.EndsWith(".png"))
                using (Bitmap bmp = GetImage(0)) bmp.Save(outPath, ImageFormat.Png);
            else if (outPath.EndsWith(".tga"))
                using (Bitmap bmp = GetImage(0)) bmp.SaveTGA(outPath);
            else if (outPath.EndsWith(".tiff") || outPath.EndsWith(".tif"))
                using (Bitmap bmp = GetImage(0)) bmp.Save(outPath, ImageFormat.Tiff);
            else if (outPath.EndsWith(".bmp"))
                using (Bitmap bmp = GetImage(0)) bmp.Save(outPath, ImageFormat.Bmp);
            else if (outPath.EndsWith(".jpg") || outPath.EndsWith(".jpeg"))
                using (Bitmap bmp = GetImage(0)) bmp.Save(outPath, ImageFormat.Jpeg);
            else if (outPath.EndsWith(".gif"))
                using (Bitmap bmp = GetImage(0)) bmp.Save(outPath, ImageFormat.Gif);
            else
                base.Export(outPath);
        }

        internal static ResourceNode TryParse(DataSource source) { return ((TEX0v1*)source.Address)->_header._tag == TEX0v1.Tag ? new TEX0Node() : null; }
    }
}
