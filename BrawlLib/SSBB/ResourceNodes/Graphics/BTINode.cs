using BrawlLib.Imaging;
using BrawlLib.IO;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBBTypes;
using BrawlLib.Wii.Textures;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public unsafe class BTINode : RARCEntryNode, IImageSource, IColorSource
{
    internal BTIHeader* Header { get { return (BTIHeader*)WorkingUncompressed.Address; } }
    public override ResourceType ResourceType { get { return ResourceType.BTI; } }

    BTIHeader hdr;

    [Browsable(false)]
    public bool HasPlt { get { return Header->_paletteEntryCount > 0; } }
    
    [Category("BTI Image")]
    public string FileOffset { get { return "0x" + ((uint)hdr._dataOffset).ToString("X"); } }
    [Category("BTI Image")]
    public uint Un1 { get { return hdr._unk1; } }
    [Category("BTI Image")]
    public uint Unk2 { get { return hdr._unk2; } }
    [Category("BTI Image")]
    public uint Unk3 { get { return hdr._unk3; } }
    [Category("BTI Image")]
    public uint Unk4 { get { return hdr._unk4; } }
    [Category("BTI Image")]
    public uint Unk5 { get { return hdr._unk5; } }
    [Category("BTI Image")]
    public WiiPixelFormat TextureFormat { get { return hdr.PixelFormat; } }
    [Category("BTI Image")]
    public WiiPaletteFormat PaletteFormat { get { return hdr.PaletteFormat; } }
    [Category("BTI Image")]
    public int Colors { get { return hdr._paletteEntryCount; } }
    [Category("BTI Image")]
    public int Width { get { return hdr._width; } }
    [Category("BTI Image")]
    public int Height { get { return hdr._height; } }
    [Category("BTI Image")]
    public int LevelOfDetail { get { return hdr._mipmapCount; } }
    [Category("BTI Image")]
    public MatTextureMinFilter MinFilter { get { return (MatTextureMinFilter)hdr._minFilter; } }
    [Category("BTI Image")]
    public MatTextureMagFilter MagFilter { get { return (MatTextureMagFilter)hdr._magFilter; } }

    [Browsable(false)]
    public int ImageCount { get { return LevelOfDetail; } }
    public Bitmap GetImage(int index)
    {
        try
        {
            if (HasPlt == true)
                return TextureConverter.DecodeIndexed((VoidPtr)Header + Header->_dataOffset, Width, Height, Palette, index + 1, TextureFormat);
            else
                return TextureConverter.Decode((VoidPtr)Header + Header->_dataOffset, Width, Height, index + 1, TextureFormat);
        }
        catch
        {
            return null;
        }
    }

    private ColorPalette _palette;
    [Browsable(false)]
    public ColorPalette Palette
    {
        get { return HasPlt ? _palette == null ? _palette = TextureConverter.DecodePalette((VoidPtr)Header + Header->_paletteOffset, Colors, PaletteFormat) : _palette : null; }
        set { _palette = value; SignalPropertyChange(); }
    }

    #region IColorSource Members

    public bool HasPrimary(int id) { return false; }
    public ARGBPixel GetPrimaryColor(int id) { return new ARGBPixel(); }
    public void SetPrimaryColor(int id, ARGBPixel color) { }
    [Browsable(false)]
    public string PrimaryColorName(int id) { return null; }
    [Browsable(false)]
    public int TypeCount { get { return 1; } }
    [Browsable(false)]
    public int ColorCount(int id) { return Palette != null ? Palette.Entries.Length : 0; }
    public ARGBPixel GetColor(int index, int id) { return Palette != null ? (ARGBPixel)Palette.Entries[index] : new ARGBPixel(); }
    public void SetColor(int index, int id, ARGBPixel color) { if (Palette != null) { Palette.Entries[index] = (Color)color; SignalPropertyChange(); } }
    public bool GetColorConstant(int id)
    {
        return false;
    }
    public void SetColorConstant(int id, bool constant)
    {
    }

    #endregion

    public override bool OnInitialize()
    {
        base.OnInitialize();

        hdr = *Header;

        return false;
    }

    public override void OnRebuild(VoidPtr address, int length, bool force)
    {
        
    }

    public void Replace(Bitmap bmp)
    {
        //FileMap tMap;
        //if (HasPlt)
        //    tMap = TextureConverter.Get(_format).EncodeREFTTextureIndexed(bmp, LevelOfDetail, Palette.Entries.Length, PaletteFormat, QuantizationAlgorithm.MedianCut);
        //else
        //    tMap = TextureConverter.Get(_format).EncodeREFTTexture(bmp, LevelOfDetail, WiiPaletteFormat.IA8);
        //ReplaceRaw(tMap);
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

    internal static ResourceNode TryParse(DataSource source)
    {
        BTIHeader* hdr = (BTIHeader*)source.Address;
        if (hdr->_dataOffset == 0x20 &&
            hdr->_textureFormat < 0x14 &&
            hdr->_paletteFormat < 3 &&
            hdr->_wrapS < 3 &&
            hdr->_wrapT < 3 &&
            hdr->_magFilter <= 5 &&
            hdr->_minFilter <= 5)
            return new BTINode();
        return null;
    }

    //internal unsafe void Prepare(BMDMaterialRefNode mRef, int shaderProgramHandle)
    //{
    //    if (mRef.PaletteNode != null && palette == null)
    //        palette = mRef.RootNode.FindChild("Palettes(NW4R)/" + mRef.Palette, true) as PLT0Node;

    //    try
    //    {
    //        if (Texture != null)
    //            Texture.Bind(mRef.Index, shaderProgramHandle, TKContext.CurrentContext);
    //        else
    //            Load(mRef.Index, shaderProgramHandle, palette);
    //    }
    //    catch { }

    //    int filter = 0;
    //    switch (mRef.MagFilter)
    //    {
    //        case MDL0MaterialRefNode.TextureMagFilter.Nearest:
    //            filter = (int)TextureMagFilter.Nearest; break;
    //        case MDL0MaterialRefNode.TextureMagFilter.Linear:
    //            filter = (int)TextureMagFilter.Linear; break;
    //    }
    //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, filter);
    //    switch (mRef.MinFilter)
    //    {
    //        case MDL0MaterialRefNode.TextureMinFilter.Nearest:
    //            filter = (int)TextureMinFilter.Nearest; break;
    //        case MDL0MaterialRefNode.TextureMinFilter.Linear:
    //            filter = (int)TextureMinFilter.Linear; break;
    //        case MDL0MaterialRefNode.TextureMinFilter.Nearest_Mipmap_Nearest:
    //            filter = (int)TextureMinFilter.NearestMipmapNearest; break;
    //        case MDL0MaterialRefNode.TextureMinFilter.Nearest_Mipmap_Linear:
    //            filter = (int)TextureMinFilter.NearestMipmapLinear; break;
    //        case MDL0MaterialRefNode.TextureMinFilter.Linear_Mipmap_Nearest:
    //            filter = (int)TextureMinFilter.LinearMipmapNearest; break;
    //        case MDL0MaterialRefNode.TextureMinFilter.Linear_Mipmap_Linear:
    //            filter = (int)TextureMinFilter.LinearMipmapLinear; break;
    //    }
    //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, filter);
    //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, mRef.LODBias);

    //    switch ((int)mRef.UWrapMode)
    //    {
    //        case 0: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge); break;
    //        case 1: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); break;
    //        case 2: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat); break;
    //    }

    //    switch ((int)mRef.VWrapMode)
    //    {
    //        case 0: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge); break;
    //        case 1: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat); break;
    //        case 2: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat); break;
    //    }

    //    float* p = stackalloc float[4];
    //    p[0] = p[1] = p[2] = p[3] = 1.0f;
    //    if (Selected && !ObjOnly)
    //        p[0] = -1.0f;

    //    GL.Light(LightName.Light0, LightParameter.Specular, p);
    //    GL.Light(LightName.Light0, LightParameter.Diffuse, p);
    //}

    //public void GetSource()
    //{
    //    Source = BRESNode.FindChild("Textures(NW4R)/" + Name, true) as TEX0Node;
    //}

    //public void Reload()
    //{
    //    if (TKContext.CurrentContext == null)
    //        return;

    //    TKContext.CurrentContext.Capture();
    //    Load();
    //}

    //private unsafe void Load() { Load(-1, -1, null); }
    //private unsafe void Load(int index, int program, PLT0Node palette)
    //{
    //    if (TKContext.CurrentContext == null)
    //        return;

    //    Source = null;

    //    if (Texture != null)
    //        Texture.Delete();
    //    Texture = new GLTexture();
    //    Texture.Bind(index, program, TKContext.CurrentContext);

    //    //ctx._states[String.Format("{0}_TexRef", Name)] = Texture;

    //    Bitmap bmp = null;

    //    if (_folderWatcher.EnableRaisingEvents && !String.IsNullOrEmpty(_folderWatcher.Path))
    //        bmp = SearchDirectory(_folderWatcher.Path + Name);

    //    if (bmp == null && TKContext.CurrentContext._states.ContainsKey("_Node_Refs"))
    //    {
    //        List<ResourceNode> nodes = TKContext.CurrentContext._states["_Node_Refs"] as List<ResourceNode>;
    //        List<ResourceNode> searched = new List<ResourceNode>(nodes.Count);
    //        TEX0Node tNode = null;

    //        foreach (ResourceNode n in nodes)
    //        {
    //            ResourceNode node = n.RootNode;
    //            if (searched.Contains(node))
    //                continue;
    //            searched.Add(node);

    //            //Search node itself first
    //            if ((tNode = node.FindChild("Textures(NW4R)/" + Name, true) as TEX0Node) != null)
    //            {
    //                Source = tNode;
    //                if (palette != null)
    //                    Texture.Attach(tNode, palette);
    //                else
    //                    Texture.Attach(tNode);
    //                return;
    //            }
    //            else
    //                bmp = SearchDirectory(node._origPath);

    //            if (bmp != null)
    //                break;
    //        }
    //        searched.Clear();
    //    }

    //    if (bmp != null)
    //        Texture.Attach(bmp);
    //}

    //private Bitmap SearchDirectory(string path)
    //{
    //    Bitmap bmp = null;
    //    if (!String.IsNullOrEmpty(path))
    //    {
    //        DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(path));
    //        if (dir.Exists && Name != "<null>")
    //            foreach (FileInfo file in dir.GetFiles(Name + ".*"))
    //            {
    //                if (file.Name.EndsWith(".tga"))
    //                {
    //                    Source = file.FullName;
    //                    bmp = TGA.FromFile(file.FullName);
    //                    break;
    //                }
    //                else if (file.Name.EndsWith(".png") || file.Name.EndsWith(".tiff") || file.Name.EndsWith(".tif"))
    //                {
    //                    Source = file.FullName;
    //                    bmp = (Bitmap)Bitmap.FromFile(file.FullName);
    //                    break;
    //                }
    //            }
    //    }
    //    return bmp;
    //}

    //public static int Compare(MDL0TextureNode t1, MDL0TextureNode t2)
    //{
    //    return String.Compare(t1.Name, t2.Name, false);
    //}

    //public int CompareTo(object obj)
    //{
    //    if (obj is MDL0TextureNode)
    //        return this.Name.CompareTo(((MDL0TextureNode)obj).Name);
    //    else return 1;
    //}

    //internal override void Bind()
    //{
    //    if (Name == "TShadow1")
    //        Enabled = false;

    //    Selected = false;
    //    //Enabled = true;
    //}
    //internal override void Unbind()
    //{
    //    if (Texture != null)
    //    {
    //        Texture.Delete();
    //        Texture = null;
    //    }
    //    Rendered = false;
    //}
}