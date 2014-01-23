using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace BrawlLib.Wii.Graphics
{
    // This is for sending immediate XF (transform unit) commands.
    // All immediate XF commands have the form:
    //
    // | 8 bits     | 16 bits    | 16 bits    | 32 bits * length |
    // | cmd. token | length - 1 | 1st addr.  | reg. value(s)    |
    //
    // Length (the number of values being sent) can be up to 16 only.
    //
    // XF has a different register address space than BP or CP.

    public struct XFCommand
    {
        public bushort _length; //Reads one 32bit value for each (length + 1). 0 means one value follows
        public bushort _address;
    }

    public class XFData
    {
        public XFMemoryAddr addr;
        public List<uint> values = new List<uint>();

        public XFData() { }
        public XFData(XFMemoryAddr mem) { addr = mem; }

        public int Size { get { return 5 + values.Count * 4; } }

        public override string ToString()
        {
            string str = "Address: " + addr;
            if (values != null)
            for (int i = 0; i < values.Count; i++)
            {
                if (addr >= XFMemoryAddr.XF_TEX0_ID && addr <= XFMemoryAddr.XF_TEX7_ID)
                    str += " | Value " + i + ": (" + new XFTexMtxInfo(values[i]) + ")";
                else if (addr >= XFMemoryAddr.XF_DUALTEX0_ID && addr <= XFMemoryAddr.XF_DUALTEX7_ID)
                    str += " | Value " + i + ": (" + new XFDualTex(values[i]) + ")";
            }
            return str;
        }
    }

    //TexMtxInfo
    //0000 0000 0000 0001   Unk (None)
    //0000 0000 0000 0010   Projection
    //0000 0000 0000 1100   Input Form
    //0000 0000 0111 0000   Texgen Type
    //0000 0011 1000 0000   Source Row
    //0001 1100 0000 0000   Emboss Source
    //1110 0000 0000 0000   Emboss Light

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct XFTexMtxInfo
    {
        internal uint _data;

        public TexProjection Projection { get { return (TexProjection)((_data >> 1) & 1); } }
        //Normal enable is true when projection is XF_TEX_STQ
        public TexInputForm InputForm { get { return (TexInputForm)((_data >> 2) & 3); } }
        public TexTexgenType TexGenType { get { return (TexTexgenType)((_data >> 4) & 7); } }
        public TexSourceRow SourceRow { get { return (TexSourceRow)((_data >> 7) & 7); } }
        public int EmbossSource { get { return (int)((_data >> 10) & 7); } }
        public int EmbossLight { get { return (int)((_data >> 13) & 7); } }

        public XFTexMtxInfo(uint value) { _data = value; }

        public override string ToString()
        {
            return String.Format("Projection: {0} | Input Form: {1} | Texgen Type: {2} | Source Row: {3} | Emboss Source: {4} | Emboss Light: {5}", Projection, InputForm, TexGenType, SourceRow, EmbossSource, EmbossLight);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct XFDualTex
    {
        //0000 0000 1111 1111   Dual Mtx
        //1111 1111 0000 0000   Normal Enable
        
        public byte pad0, pad1, NormalEnable, DualMtx;
        //Normal enable is true when projection is XF_TEX_STQ
        //DualMtx starts at 0 and increases by 3 for each texture (for every 3 matrix rows)

        public XFDualTex(uint value) 
        {
            pad0 = pad1 = 0;
            DualMtx = (byte)(value & 0xFF); 
            NormalEnable = (byte)((value >> 8) & 0xFF);
        }

        public XFDualTex(int mtx, int norm)
        {
            pad0 = pad1 = 0;
            DualMtx = (byte)mtx;
            NormalEnable = (byte)norm;
        }

        public uint Value { get { return (uint)(((ushort)NormalEnable << 8) | DualMtx); } }

        public override string ToString()
        {
            return String.Format("Normal Enable: {0} | Dual Matrix: {1}", NormalEnable != 0 ? "True" : "False", DualMtx);
        }
    }

    public enum XFMemoryAddr : ushort
    {
        PosMatrices = 0x0000,

        Size = 0x8000,
        Error = 0x1000,
        Diag = 0x1001,
        State0 = 0x1002,
        State1 = 0x1003,
        Clock = 0x1004,
        ClipDisable = 0x1005,
        SetGPMetric = 0x1006,

        //VTXSpecs = 0x1008,
        //SetNumChan = 0x1009,
        SetChan0AmbColor = 0x100A,
        SetChan1AmbColor = 0x100B,
        SetChan0MatColor = 0x100C,
        SetChan1MatColor = 0x100D,
        SetChan0Color = 0x100E,
        SetChan1Color = 0x100F,
        SetChan0Alpha = 0x1010,
        SetChan1Alpha = 0x1011,
        DualTex = 0x1012,
        SetMatrixIndA = 0x1018,
        SetMatrixIndB = 0x1019,
        SetViewport = 0x101A,
        SetZScale = 0x101C,
        SetZOffset = 0x101F,
        SetProjection = 0x1020,
        //SetNumTexGens = 0x103F,
        //SetTexMtxInfo = 0x1040,
        //SetPosMtxInfo = 0x1050,

         XF_INVTXSPEC_ID = 0x1008,
         XF_NUMCOLORS_ID = 0x1009,
         XF_NUMTEX_ID    = 0x103f,

         XF_TEX0_ID      = 0x1040,
         XF_TEX1_ID      = 0x1041,
         XF_TEX2_ID      = 0x1042,
         XF_TEX3_ID      = 0x1043,
         XF_TEX4_ID      = 0x1044,
         XF_TEX5_ID      = 0x1045,
         XF_TEX6_ID      = 0x1046,
         XF_TEX7_ID      = 0x1047,

         XF_DUALTEX0_ID  = 0x1050,
         XF_DUALTEX1_ID  = 0x1051,
         XF_DUALTEX2_ID  = 0x1052,
         XF_DUALTEX3_ID  = 0x1053,
         XF_DUALTEX4_ID  = 0x1054,
         XF_DUALTEX5_ID  = 0x1055,
         XF_DUALTEX6_ID  = 0x1056,
         XF_DUALTEX7_ID  = 0x1057
    }

    public enum XFNormalFormat
    {
        None = 0, //I've seen models with no normals.
        XYZ = 1,
        NBT = 2
    }

    //VTXSpecs
    //0000 0000 0000 0011   - Num colors
    //0000 0000 0000 1100   - Normal type (0 = none, 1 = normals, 2 = normals + binormals)
    //0000 0000 1111 0000   - Num textures
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct XFVertexSpecs
    {
        internal buint _data;

        public int ColorCount { get { return (int)(_data & 3); } set { _data = _data & 0xFFFFFFFC | ((uint)value & 3); } }
        public int TextureCount { get { return (int)(_data >> 4 & 0xF); } set { _data = _data & 0xFFFFFF0F | (((uint)value & 0xF) << 4); } }
        public XFNormalFormat NormalFormat { get { return (XFNormalFormat)(_data >> 2 & 3); } set { _data = _data & 0xFFFFFFF3 | (((uint)value & 3) << 2); } }

        public XFVertexSpecs(uint raw) { _data = raw; }
        public XFVertexSpecs(int colors, int textures, XFNormalFormat normalFormat)
        { _data = (((uint)textures & 0xF) << 4) | (((uint)normalFormat & 3) << 2) | ((uint)colors & 3); }

        public override string ToString()
        {
            return String.Format("ColorCount: {0} | TextureCount: {1} | Normal Format: {2} [Data: {3}] ", ColorCount.ToString(), TextureCount.ToString(), NormalFormat.ToString(), (int)_data);
        }
    }

    //This is used by polygons to enable element arrays (I believe)
    //There doesn't seem to be a native spec for this
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct XFArrayFlags
    {
        internal buint _data;

        //0000 0000 0000 0000 0000 0001 Pos Matrix
        //0000 0000 0000 0000 0000 0010 TexMtx0
        //0000 0000 0000 0000 0000 0100 TexMtx1
        //0000 0000 0000 0000 0000 1000 TexMtx2
        //0000 0000 0000 0000 0001 0000 TexMtx3
        //0000 0000 0000 0000 0010 0000 TexMtx4
        //0000 0000 0000 0000 0100 0000 TexMtx5
        //0000 0000 0000 0000 1000 0000 TexMtx6
        //0000 0000 0000 0001 0000 0000 TexMtx7
        //0000 0000 0000 0010 0000 0000 Positions
        //0000 0000 0000 0100 0000 0000 Normals
        //0000 0000 0000 1000 0000 0000 Color0
        //0000 0000 0001 0000 0000 0000 Color1
        //0000 0000 0010 0000 0000 0000 Tex0
        //0000 0000 0100 0000 0000 0000 Tex1
        //0000 0000 1000 0000 0000 0000 Tex2
        //0000 0001 0000 0000 0000 0000 Tex3
        //0000 0010 0000 0000 0000 0000 Tex4
        //0000 0100 0000 0000 0000 0000 Tex5
        //0000 1000 0000 0000 0000 0000 Tex6
        //0001 0000 0000 0000 0000 0000 Tex7

        public bool HasPosMatrix { get { return (_data & 1) != 0; } set { _data = _data & 0xFFFFFFFE | (uint)(value ? 1 : 0); } }
        public bool HasPositions { get { return (_data & 0x200) != 0; } set { _data = _data & 0xFFFFFDFF | (uint)(value ? 0x200 : 0); } }
        public bool HasNormals { get { return (_data & 0x400) != 0; } set { _data = _data & 0xFFFFFBFF | (uint)(value ? 0x400 : 0); } }

        public bool GetHasTexMatrix(int index) { return (_data & 2 << index) != 0; }
        public void SetHasTexMatrix(int index, bool exists) { _data = _data & ~((uint)2 << index) | ((uint)(exists ? 2 : 0) << index); }

        public bool GetHasColor(int index) { return (_data & 0x800 << index) != 0; }
        public void SetHasColor(int index, bool exists) { _data = _data & ~((uint)0x800 << index) | ((uint)(exists ? 0x800 : 0) << index); }

        public bool GetHasUVs(int index) { return (_data & 0x2000 << index) != 0; }
        public void SetHasUVs(int index, bool exists) { _data = _data & ~((uint)0x2000 << index) | ((uint)(exists ? 0x2000 : 0) << index); }

        public override string ToString()
        {
            string texmtx = "";
            bool hasTex = false;
            for (int i = 0; i < 8; i++)
                if (GetHasTexMatrix(i))
                {
                    hasTex = true;
                    texmtx += i.ToString() + " ";
                }

            string uvs = "";
            bool hasUVs = false;
            for (int i = 0; i < 8; i++)
                if (GetHasUVs(i) == true)
                {
                    hasUVs = true;
                    uvs += i.ToString() + " ";
                }

            string colors = "";
            bool hasColors = false;
            for (int i = 0; i < 2; i++)
                if (GetHasUVs(i) == true)
                {
                    hasColors = true;
                    colors += i.ToString() + " ";
                }

            return String.Format("PosMtx: {0} | TexMtx: {1}| Positions: {2} | Normals: {3} | Colors: {5} | UVs: {4}", HasPosMatrix ? "True" : "False", hasTex ? texmtx : "False ", HasPositions ? "True" : "False", HasNormals ? "True" : "False", hasUVs ? uvs : "False ", hasColors ? colors : "False ");
        }
    }

    public enum XFDataFormat : byte
    {
        None = 0,
        Direct = 1,
        Index8 = 2,
        Index16 = 3
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FacepointAttribute
    {
        public FacepointAttribute(GXAttribute attr, XFDataFormat fmt)
        {
            _attr = attr;
            _type = fmt;
        }

        public GXAttribute _attr;
        public XFDataFormat _type;
    }

    public enum GXAttribute
    {
        PosNrmMtxId = 0,
        Tex0MtxId,
        Tex1MtxId,
        Tex2MtxId,
        Tex3MtxId,
        Tex4MtxId,
        Tex5MtxId,
        Tex6MtxId,
        Tex7MtxId,
        Position = 9,    
        Normal,
        Color0,
        Color1,
        Tex0,
        Tex1,
        Tex2,
        Tex3,
        Tex4,
        Tex5,
        Tex6,
        Tex7,

        PosMtxArray,
        NrmMtxArray,
        TexMtxArray,
        LightArray,
        NBT // normal, bi-normal, tangent 
    }
}
