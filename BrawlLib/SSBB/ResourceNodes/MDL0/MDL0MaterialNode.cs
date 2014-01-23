using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.OpenGL;
using System.Drawing;
using System.IO;
using BrawlLib.Imaging;
using BrawlLib.Wii.Graphics;
using BrawlLib.Wii.Models;
using System.Windows.Forms;
using BrawlLib.IO;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe partial class MDL0MaterialNode : MDL0EntryNode
    {
        internal MDL0Material* Header { get { return (MDL0Material*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.MDL0Material; } }
        public override bool AllowDuplicateNames { get { return true; } }

        public MDL0MaterialNode()
        {
            _normMapRefLight1 =
            _normMapRefLight2 =
            _normMapRefLight3 =
            _normMapRefLight4 = -1;
            _numLights = 1;

            _chan1 = new LightChannel(63, new RGBAPixel(255, 255, 255, 255), new RGBAPixel(255, 255, 255, 255), 0, 0, this);
            _chan2 = new LightChannel(15, new RGBAPixel(0, 0, 0, 255), new RGBAPixel(), 0, 0, this);
        }

        #region Variables

        //Use these for streaming values into the shader
        public RGBAPixel amb1, amb2, clr1, clr2;
        public GXColorS10 k1, k2, k3, k4, c1, c2, c3;

        MatModeBlock* mode;

        [Category("User Data"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public UserDataCollection UserEntries { get { return _userEntries; } set { _userEntries = value; SignalPropertyChange(); } }
        internal UserDataCollection _userEntries = new UserDataCollection();

        internal int _dataLen, _mdl0Offset, _stringOffset, _shaderOffset, _matRefOffset, _userDataOffset, _furDataOffset, _dlOffset, _pad;
        internal byte _numTextures, _numLights, _indirectMethod1, _indirectMethod2, _indirectMethod3, _indirectMethod4;
        public sbyte _normMapRefLight1, _normMapRefLight2, _normMapRefLight3, _normMapRefLight4, _lSet, _fSet;
        internal Bin32 _usageFlags = new Bin32();
        public byte _ssc;
        internal byte _clip;
        public byte _transp;
        internal CullMode _cull;

        //In order of appearance in display list:
        //Mode block
        internal GXAlphaFunction _alphaFunc = GXAlphaFunction.Default;
        internal ZMode _zMode = ZMode.Default;
        //Mask, does not allow changing the dither/update bits
        internal Wii.Graphics.BlendMode _blendMode = Wii.Graphics.BlendMode.Default;
        internal ConstantAlpha _constantAlpha = ConstantAlpha.Default;
        //Tev Color Block
        internal MatTevColorBlock _tevColorBlock = MatTevColorBlock.Default;
        //Pad 4
        //Tev Konstant Block
        internal MatTevKonstBlock _tevKonstBlock = MatTevKonstBlock.Default;
        //Pad 24
        //Indirect texture scale for CMD stages
        internal MatIndMtxBlock _indMtx = MatIndMtxBlock.Default;
        //XF Texture matrix info

        #endregion

        #region Attributes

        public MDL0ObjectNode[] Objects { get { if (!isMetal) return _objects.ToArray(); else return MetalMaterial == null ? null : MetalMaterial._objects.ToArray(); } }
        internal List<MDL0ObjectNode> _objects = new List<MDL0ObjectNode>();

        [Browsable(false)]
        public XFData[] XFCommands { get { return XFCmds.ToArray(); } }
        internal List<XFData> XFCmds = new List<XFData>();

        [Category("TEV Konstant Block"), TypeConverter(typeof(GXColorS10StringConverter))]
        public GXColorS10 KReg0Color 
        { 
            get { return new GXColorS10() { R = _tevKonstBlock.TevReg0Lo.RB, A = _tevKonstBlock.TevReg0Lo.AG, B = _tevKonstBlock.TevReg0Hi.RB, G = _tevKonstBlock.TevReg0Hi.AG }; }
            set { if (!CheckIfMetal()) { _tevKonstBlock.TevReg0Lo.RB = value.R; _tevKonstBlock.TevReg0Lo.AG = value.A; _tevKonstBlock.TevReg0Hi.RB = value.B; _tevKonstBlock.TevReg0Hi.AG = value.G; k1 = value; } } 
        }
        [Category("TEV Konstant Block"), TypeConverter(typeof(GXColorS10StringConverter))]
        public GXColorS10 KReg1Color 
        { 
            get { return new GXColorS10() { R = _tevKonstBlock.TevReg1Lo.RB, A = _tevKonstBlock.TevReg1Lo.AG, B = _tevKonstBlock.TevReg1Hi.RB, G = _tevKonstBlock.TevReg1Hi.AG }; }
            set { if (!CheckIfMetal()) { _tevKonstBlock.TevReg1Lo.RB = value.R; _tevKonstBlock.TevReg1Lo.AG = value.A; _tevKonstBlock.TevReg1Hi.RB = value.B; _tevKonstBlock.TevReg1Hi.AG = value.G; k2 = value; } } 
        }
        [Category("TEV Konstant Block"), TypeConverter(typeof(GXColorS10StringConverter))]
        public GXColorS10 KReg2Color 
        { 
            get { return new GXColorS10() { R = _tevKonstBlock.TevReg2Lo.RB, A = _tevKonstBlock.TevReg2Lo.AG, B = _tevKonstBlock.TevReg2Hi.RB, G = _tevKonstBlock.TevReg2Hi.AG }; }
            set { if (!CheckIfMetal()) { _tevKonstBlock.TevReg2Lo.RB = value.R; _tevKonstBlock.TevReg2Lo.AG = value.A; _tevKonstBlock.TevReg2Hi.RB = value.B; _tevKonstBlock.TevReg2Hi.AG = value.G; k3 = value; } } 
        }
        [Category("TEV Konstant Block"), TypeConverter(typeof(GXColorS10StringConverter))]
        public GXColorS10 KReg3Color 
        { 
            get { return new GXColorS10() { R = _tevKonstBlock.TevReg3Lo.RB, A = _tevKonstBlock.TevReg3Lo.AG, B = _tevKonstBlock.TevReg3Hi.RB, G = _tevKonstBlock.TevReg3Hi.AG }; }
            set { if (!CheckIfMetal()) { _tevKonstBlock.TevReg3Lo.RB = value.R; _tevKonstBlock.TevReg3Lo.AG = value.A; _tevKonstBlock.TevReg3Hi.RB = value.B; _tevKonstBlock.TevReg3Hi.AG = value.G; k4 = value; } } 
        }

        [Category("TEV Color Block"), TypeConverter(typeof(GXColorS10StringConverter))]
        public GXColorS10 CReg0Color 
        {
            get { return new GXColorS10() { R = _tevColorBlock.TevReg1Lo.RB, A = _tevColorBlock.TevReg1Lo.AG, B = _tevColorBlock.TevReg1Hi0.RB, G = _tevColorBlock.TevReg1Hi0.AG }; }
            set
            {
                if (!CheckIfMetal())
                {
                    _tevColorBlock.TevReg1Lo.RB = value.R;
                    _tevColorBlock.TevReg1Lo.AG = value.A;

                    //Hi values are always the same...
                    _tevColorBlock.TevReg1Hi0.RB =
                    _tevColorBlock.TevReg1Hi1.RB =
                    _tevColorBlock.TevReg1Hi2.RB = value.B;
                    _tevColorBlock.TevReg1Hi0.AG =
                    _tevColorBlock.TevReg1Hi1.AG =
                    _tevColorBlock.TevReg1Hi2.AG = value.G;

                    c1 = value;
                }
            }
        }
        [Category("TEV Color Block"), TypeConverter(typeof(GXColorS10StringConverter))]
        public GXColorS10 CReg1Color 
        {
            get { return new GXColorS10() { R = _tevColorBlock.TevReg2Lo.RB, A = _tevColorBlock.TevReg2Lo.AG, B = _tevColorBlock.TevReg2Hi0.RB, G = _tevColorBlock.TevReg2Hi0.AG }; }
            set
            {
                if (!CheckIfMetal())
                {
                    _tevColorBlock.TevReg2Lo.RB = value.R;
                    _tevColorBlock.TevReg2Lo.AG = value.A;

                    //Hi values are always the same...
                    _tevColorBlock.TevReg2Hi0.RB =
                    _tevColorBlock.TevReg2Hi1.RB =
                    _tevColorBlock.TevReg2Hi2.RB = value.B;
                    _tevColorBlock.TevReg2Hi0.AG =
                    _tevColorBlock.TevReg2Hi1.AG =
                    _tevColorBlock.TevReg2Hi2.AG = value.G;

                    c2 = value;
                }
            }
        }
        [Category("TEV Color Block"), TypeConverter(typeof(GXColorS10StringConverter))]
        public GXColorS10 CReg2Color 
        {
            get { return new GXColorS10() { R = _tevColorBlock.TevReg3Lo.RB, A = _tevColorBlock.TevReg3Lo.AG, B = _tevColorBlock.TevReg3Hi0.RB, G = _tevColorBlock.TevReg3Hi0.AG }; } 
            set
            {
                if (!CheckIfMetal()) 
                {

                    _tevColorBlock.TevReg3Lo.RB = value.R; 
                    _tevColorBlock.TevReg3Lo.AG = value.A; 

                    //Hi values are always the same...
                    _tevColorBlock.TevReg3Hi0.RB =
                    _tevColorBlock.TevReg3Hi1.RB =
                    _tevColorBlock.TevReg3Hi2.RB = value.B; 
                    _tevColorBlock.TevReg3Hi0.AG =
                    _tevColorBlock.TevReg3Hi1.AG =
                    _tevColorBlock.TevReg3Hi2.AG = value.G;

                    c3 = value;
                }
            }
        }

        //[Category("TEV Color Block")]
        //public ColorReg Reg1Lo { get { return _tevColorBlock.TevReg1Lo; } }
        //[Category("TEV Color Block")]
        //public ColorReg Reg1Hi0 { get { return _tevColorBlock.TevReg1Hi0; } }
        //[Category("TEV Color Block")]
        //public ColorReg Reg1Hi1 { get { return _tevColorBlock.TevReg1Hi1; } }
        //[Category("TEV Color Block")]
        //public ColorReg Reg1Hi2 { get { return _tevColorBlock.TevReg1Hi2; } }

        //[Category("TEV Color Block")]
        //public ColorReg Reg2Lo { get { return _tevColorBlock.TevReg2Lo; } }
        //[Category("TEV Color Block")]
        //public ColorReg Reg2Hi0 { get { return _tevColorBlock.TevReg2Hi0; } }
        //[Category("TEV Color Block")]
        //public ColorReg Reg2Hi1 { get { return _tevColorBlock.TevReg2Hi1; } }
        //[Category("TEV Color Block")]
        //public ColorReg Reg2Hi2 { get { return _tevColorBlock.TevReg2Hi2; } }

        //[Category("TEV Color Block")]
        //public ColorReg Reg3Lo { get { return _tevColorBlock.TevReg3Lo; } }
        //[Category("TEV Color Block")]
        //public ColorReg Reg3Hi0 { get { return _tevColorBlock.TevReg3Hi0; } }
        //[Category("TEV Color Block")]
        //public ColorReg Reg3Hi1 { get { return _tevColorBlock.TevReg3Hi1; } }
        //[Category("TEV Color Block")]
        //public ColorReg Reg3Hi2 { get { return _tevColorBlock.TevReg3Hi2; } }
        
        #region Shader linkage
        internal MDL0ShaderNode _shader;
        [Browsable(false)]
        public MDL0ShaderNode ShaderNode
        {
            get { return _shader; }
            set
            {
                if (_shader == value)
                    return;
                if (_shader != null)
                    _shader._materials.Remove(this);
                if ((_shader = value) != null)
                    _shader._materials.Add(this);
                if (_shader != null)
                    ActiveShaderStages = _shader._stages;
            }
        }
        [Browsable(true), TypeConverter(typeof(DropDownListShaders))]
        public string Shader
        {
            get { return _shader == null ? null : _shader.Name; }
            set
            {
                if (CheckIfMetal())
                    return;

                if (String.IsNullOrEmpty(value))
                    ShaderNode = null;
                else
                {
                    MDL0ShaderNode node = Model.FindChild(String.Format("Shaders/{0}", value), false) as MDL0ShaderNode;
                    if (node != null)
                        ShaderNode = node;
                }
            }
        }
        #endregion

        [Category("Alpha Function")]
        public byte Ref0 { get { return _alphaFunc.ref0; } set { if (!CheckIfMetal()) _alphaFunc.ref0 = value; } }
        [Category("Alpha Function")]
        public AlphaCompare Comp0 { get { return _alphaFunc.Comp0; } set { if (!CheckIfMetal()) _alphaFunc.Comp0 = value;  } }
        [Category("Alpha Function")]
        public AlphaOp Logic { get { return _alphaFunc.Logic; } set { if (!CheckIfMetal()) _alphaFunc.Logic = value;  } }
        [Category("Alpha Function")]
        public byte Ref1 { get { return _alphaFunc.ref1; } set { if (!CheckIfMetal()) _alphaFunc.ref1 = value;  } }
        [Category("Alpha Function")]
        public AlphaCompare Comp1 { get { return _alphaFunc.Comp1; } set { if (!CheckIfMetal()) _alphaFunc.Comp1 = value;  } }

        [Category("Z Mode")]
        public bool EnableDepthTest { get { return _zMode.EnableDepthTest; } set { if (!CheckIfMetal()) _zMode.EnableDepthTest = value;  } }
        [Category("Z Mode")]
        public bool EnableDepthUpdate { get { return _zMode.EnableDepthUpdate; } set { if (!CheckIfMetal()) _zMode.EnableDepthUpdate = value;  } }
        [Category("Z Mode")]
        public GXCompare DepthFunction { get { return _zMode.DepthFunction; } set { if (!CheckIfMetal()) _zMode.DepthFunction = value;  } }

        [Category("Blend Mode")] //Allows textures to be opaque. Cannot be used with Alpha Function
        public bool EnableBlend { get { return _blendMode.EnableBlend; } set { if (!CheckIfMetal()) { _blendMode.EnableBlend = value; } } }
        [Category("Blend Mode")]
        public bool EnableBlendLogic { get { return _blendMode.EnableLogicOp; } set { if (!CheckIfMetal()) _blendMode.EnableLogicOp = value;  } }
        
        //These are disabled via mask
        //[Category("Blend Mode")]
        //public bool EnableDither { get { return _blendMode.EnableDither; } }
        //[Category("Blend Mode")]
        //public bool EnableColorUpdate { get { return _blendMode.EnableColorUpdate; } }
        //[Category("Blend Mode")]
        //public bool EnableAlphaUpdate { get { return _blendMode.EnableAlphaUpdate; } }

        [Category("Blend Mode")]
        public BlendFactor SrcFactor { get { return _blendMode.SrcFactor; } set { if (!CheckIfMetal()) _blendMode.SrcFactor = value;  } }
        [Category("Blend Mode")]
        public GXLogicOp BlendLogicOp { get { return _blendMode.LogicOp; } set { if (!CheckIfMetal()) _blendMode.LogicOp = value;  } }
        [Category("Blend Mode")]
        public BlendFactor DstFactor { get { return _blendMode.DstFactor; } set { if (!CheckIfMetal()) _blendMode.DstFactor = value;  } }

        [Category("Blend Mode")]
        public bool Subtract { get { return _blendMode.Subtract; } set { if (!CheckIfMetal()) _blendMode.Subtract = value;  } }

        [Category("Constant Alpha")]
        public bool Enabled { get { return _constantAlpha.Enable != 0; } set { if (!CheckIfMetal()) _constantAlpha.Enable = (byte)(value ? 1 : 0); } }
        [Category("Constant Alpha")]
        public byte Value { get { return _constantAlpha.Value; } set { if (!CheckIfMetal()) _constantAlpha.Value = value; } }

        [Category("Material"), Browsable(false)]
        public int TotalLen { get { return _dataLen; } }
        [Category("Material"), Browsable(false)]
        public int MDL0Offset { get { return _mdl0Offset; } }
        [Category("Material"), Browsable(false)]
        public int StringOffset { get { return _stringOffset; } }
        [Category("Material")]
        public int ID { get { return Index; } }

        [Category("Indirect Texture Scale"), Browsable(true)]
        public IndTexScale IndirectTex0ScaleS { get { return (IndTexScale)_indMtx.SS0val.S_Scale0; } set { if (!CheckIfMetal()) _indMtx.SS0val.S_Scale0 = value; } }
        [Category("Indirect Texture Scale"), Browsable(true)]
        public IndTexScale IndirectTex0ScaleT { get { return (IndTexScale)_indMtx.SS0val.T_Scale0; } set { if (!CheckIfMetal()) _indMtx.SS0val.T_Scale0 = value; } }
        [Category("Indirect Texture Scale"), Browsable(true)]
        public IndTexScale IndirectTex1ScaleS { get { return (IndTexScale)_indMtx.SS0val.S_Scale1; } set { if (!CheckIfMetal()) _indMtx.SS0val.S_Scale1 = value; } }
        [Category("Indirect Texture Scale"), Browsable(true)]
        public IndTexScale IndirectTex1ScaleT { get { return (IndTexScale)_indMtx.SS0val.T_Scale1; } set { if (!CheckIfMetal()) _indMtx.SS0val.T_Scale1 = value; } }
        
        [Category("Indirect Texture Scale"), Browsable(true)]
        public IndTexScale IndirectTex2ScaleS { get { return (IndTexScale)_indMtx.SS1val.S_Scale0; } set { if (!CheckIfMetal()) _indMtx.SS1val.S_Scale0 = value; } }
        [Category("Indirect Texture Scale"), Browsable(true)]
        public IndTexScale IndirectTex2ScaleT { get { return (IndTexScale)_indMtx.SS1val.T_Scale0; } set { if (!CheckIfMetal()) _indMtx.SS1val.T_Scale0 = value; } }
        [Category("Indirect Texture Scale"), Browsable(true)]
        public IndTexScale IndirectTex3ScaleS { get { return (IndTexScale)_indMtx.SS1val.S_Scale1; } set { if (!CheckIfMetal()) _indMtx.SS1val.S_Scale1 = value; } }
        [Category("Indirect Texture Scale"), Browsable(true)]
        public IndTexScale IndirectTex3ScaleT { get { return (IndTexScale)_indMtx.SS1val.T_Scale1; } set { if (!CheckIfMetal()) _indMtx.SS1val.T_Scale1 = value; } }
        
        //Usage flags. Each set of 4 bits represents one texture layer.
        [Category("Texture Flags"), Browsable(false)]
        public string LayerFlags { get { return _layerFlags.ToString("X"); } }//set { if (!CheckIfMetal()) _layerFlags = UInt32.Parse(value, System.Globalization.NumberStyles.HexNumber);  } }
        public uint _layerFlags;
        [Category("Texture Flags")]
        public TexMatrixMode TexMatrixFlags { get { return (TexMatrixMode)_texMtxFlags; } set { if (!CheckIfMetal()) _texMtxFlags = (uint)value; } }
        public uint _texMtxFlags;

        [Category("Lighting Channels"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public LightChannel LightChannel1 { get { return _chan1; } set { if (!CheckIfMetal()) _chan1 = value; } }
        [Category("Lighting Channels"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public LightChannel LightChannel2 { get { return _chan2; } set { if (!CheckIfMetal()) _chan2 = value; } }

        [Category("Lighting Channel 1"), Browsable(false)]
        public LightingChannelFlags C1Flags { get { return _chan1.Flags; } set { if (!CheckIfMetal()) _chan1.Flags = value; } }
        [Category("Lighting Channel 1"), Browsable(false), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel C1MaterialColor { get { return _chan1.MaterialColor; } set { if (!CheckIfMetal()) clr1 = _chan1.MaterialColor = value; } }
        [Category("Lighting Channel 1"), Browsable(false), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel C1AmbientColor { get { return _chan1.AmbientColor; } set { if (!CheckIfMetal()) amb1 = _chan1.AmbientColor = value; } }

        [Category("Lighting Channel 1"), Browsable(false)]
        public GXColorSrc C1ColorMaterialSource
        {
            get { return _chan1.ColorMaterialSource; }
            set { if (!CheckIfMetal()) _chan1.ColorMaterialSource = value; }
        }
        [Category("Lighting Channel 1"), Browsable(false)]
        public bool C1ColorEnabled
        {
            get { return _chan1.ColorEnabled; }
            set { if (!CheckIfMetal()) _chan1.ColorEnabled = value; }
        }
        [Category("Lighting Channel 1"), Browsable(false)]
        public GXColorSrc C1ColorAmbientSource
        {
            get { return _chan1.ColorAmbientSource; }
            set { if (!CheckIfMetal()) _chan1.ColorAmbientSource = value; }
        }
        [Category("Lighting Channel 1"), Browsable(false)]
        public GXDiffuseFn C1ColorDiffuseFunction
        {
            get { return _chan1.ColorDiffuseFunction; }
            set { if (!CheckIfMetal()) _chan1.ColorDiffuseFunction = value; }
        }
        [Category("Lighting Channel 1"), Browsable(false)]
        public GXAttnFn C1ColorAttenuation
        {
            get { return _chan1.ColorAttenuation; }
            set { if (!CheckIfMetal()) _chan1.ColorAttenuation = value; }
        }
        [Category("Lighting Channel 1"), Browsable(false)]
        public MatChanLights C1ColorLights
        {
            get { return _chan1.ColorLights; }
            set { if (!CheckIfMetal()) _chan1.ColorLights = value; }
        }

        [Category("Lighting Channel 1"), Browsable(false)]
        public GXColorSrc C1AlphaMaterialSource
        {
            get { return _chan1.AlphaMaterialSource; }
            set { if (!CheckIfMetal()) _chan1.AlphaMaterialSource = value; }
        }
        [Category("Lighting Channel 1"), Browsable(false)]
        public bool C1AlphaEnabled
        {
            get { return _chan1.AlphaEnabled; }
            set { if (!CheckIfMetal()) _chan1.AlphaEnabled = value; }
        }
        [Category("Lighting Channel 1"), Browsable(false)]
        public GXColorSrc C1AlphaAmbientSource
        {
            get { return _chan1.AlphaAmbientSource; }
            set { if (!CheckIfMetal()) _chan1.AlphaAmbientSource = value; }
        }
        [Category("Lighting Channel 1"), Browsable(false)]
        public GXDiffuseFn C1AlphaDiffuseFunction
        {
            get { return _chan1.AlphaDiffuseFunction; }
            set { if (!CheckIfMetal()) _chan1.AlphaDiffuseFunction = value; }
        }
        [Category("Lighting Channel 1"), Browsable(false)]
        public GXAttnFn C1AlphaAttenuation
        {
            get { return _chan1.AlphaAttenuation; }
            set { if (!CheckIfMetal()) _chan1.AlphaAttenuation = value; }
        }
        [Category("Lighting Channel 1"), Browsable(false)]
        public MatChanLights C1AlphaLights
        {
            get { return _chan1.AlphaLights; }
            set { if (!CheckIfMetal()) _chan1.AlphaLights = value; }
        }

        [Category("Lighting Channel 2"), Browsable(false)]
        public LightingChannelFlags C2Flags { get { return _chan2.Flags; } set { if (!CheckIfMetal()) _chan2.Flags = value; } }
        [Category("Lighting Channel 2"), Browsable(false), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel C2MaterialColor { get { return _chan2.MaterialColor; } set { if (!CheckIfMetal()) clr2 = _chan2.MaterialColor = value; } }
        [Category("Lighting Channel 2"), Browsable(false), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel C2AmbientColor { get { return _chan2.AmbientColor; } set { if (!CheckIfMetal()) amb2 = _chan2.AmbientColor = value; } }

        [Category("Lighting Channel 2"), Browsable(false)]
        public GXColorSrc C2ColorMaterialSource
        {
            get { return _chan2.ColorMaterialSource; }
            set { if (!CheckIfMetal()) _chan2.ColorMaterialSource = value; }
        }
        [Category("Lighting Channel 2"), Browsable(false)]
        public bool C2ColorEnabled
        {
            get { return _chan2.ColorEnabled; }
            set { if (!CheckIfMetal()) _chan2.ColorEnabled = value; }
        }
        [Category("Lighting Channel 2"), Browsable(false)]
        public GXColorSrc C2ColorAmbientSource
        {
            get { return _chan2.ColorAmbientSource; }
            set { if (!CheckIfMetal()) _chan2.ColorAmbientSource = value; }
        }
        [Category("Lighting Channel 2"), Browsable(false)]
        public GXDiffuseFn C2ColorDiffuseFunction
        {
            get { return _chan2.ColorDiffuseFunction; }
            set { if (!CheckIfMetal()) _chan2.ColorDiffuseFunction = value; }
        }
        [Category("Lighting Channel 2"), Browsable(false)]
        public GXAttnFn C2ColorAttenuation
        {
            get { return _chan2.ColorAttenuation; }
            set { if (!CheckIfMetal()) _chan2.ColorAttenuation = value; }
        }
        [Category("Lighting Channel 2"), Browsable(false)]
        public MatChanLights C2ColorLights
        {
            get { return _chan2.ColorLights; }
            set { if (!CheckIfMetal()) _chan2.ColorLights = value; }
        }

        [Category("Lighting Channel 2"), Browsable(false)]
        public GXColorSrc C2AlphaMaterialSource
        {
            get { return _chan2.AlphaMaterialSource; }
            set { if (!CheckIfMetal()) _chan2.AlphaMaterialSource = value; }
        }
        [Category("Lighting Channel 2"), Browsable(false)]
        public bool C2AlphaEnabled
        {
            get { return _chan2.AlphaEnabled; }
            set { if (!CheckIfMetal()) _chan2.AlphaEnabled = value; }
        }
        [Category("Lighting Channel 2"), Browsable(false)]
        public GXColorSrc C2AlphaAmbientSource
        {
            get { return _chan2.AlphaAmbientSource; }
            set { if (!CheckIfMetal()) _chan2.AlphaAmbientSource = value; }
        }
        [Category("Lighting Channel 2"), Browsable(false)]
        public GXDiffuseFn C2AlphaDiffuseFunction
        {
            get { return _chan2.AlphaDiffuseFunction; }
            set { if (!CheckIfMetal()) _chan2.AlphaDiffuseFunction = value; }
        }
        [Category("Lighting Channel 2"), Browsable(false)]
        public GXAttnFn C2AlphaAttenuation
        {
            get { return _chan2.AlphaAttenuation; }
            set { if (!CheckIfMetal()) _chan2.AlphaAttenuation = value; }
        }
        [Category("Lighting Channel 2"), Browsable(false)]
        public MatChanLights C2AlphaLights
        {
            get { return _chan2.AlphaLights; }
            set { if (!CheckIfMetal()) _chan2.AlphaLights = value; }
        }

        public LightChannel _chan1 = new LightChannel(63, new RGBAPixel(255, 255, 255, 255), new RGBAPixel(255, 255, 255, 255), 0, 0), _chan2 = new LightChannel(15, new RGBAPixel(0, 0, 0, 255), new RGBAPixel(), 0, 0);

        [Category("Material")]
        public bool XLUMaterial 
        {
            get { return _usageFlags[31]; }
            set
            {
                if (!CheckIfMetal())
                {
                    bool prev = _usageFlags[31];
                    _usageFlags[31] = value;

                    string message = "";
                    for (int i = 0; i < _objects.Count; i++)
                        _objects[i].EvalMaterials(ref message);

                    if (message.Length != 0)
                        if (MessageBox.Show(null, "Are you sure you want to continue?\nThe following objects will no longer use this material:\n" + message, "Continue?", MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            _changed = false;
                            _usageFlags[31] = prev;
                            return;
                        }

                    message = "";
                    for (int i = 0; i < _objects.Count; i++)
                        _objects[i].FixMaterials(ref message);

                    if (message.Length != 0)
                        MessageBox.Show("The following objects no longer use this material:\n" + message);
                }
            }
        }
        
        //[Category("Material")]
        //public byte Texgens { get { return _numTextures; } }//set { if (!CheckIfMetal()) _numTextures = value;  } }
        [Category("Material")]
        public byte LightChannels { get { return _numLights; } set { if (!CheckIfMetal()) _numLights = value.Clamp(0, 2); } }
        [Category("Material")]
        public byte ActiveShaderStages { get { return _ssc; } set { if (!CheckIfMetal()) _ssc = (value > ShaderNode._stages ? (byte)ShaderNode._stages : value < 1 ? (byte)1 : value); } }
        [Category("Material")]
        public byte IndirectShaderStages { get { return _clip; } set { if (!CheckIfMetal()) _clip = (value > 4 ? (byte)4 : value < 0 ? (byte)0 : value); } }
        [Category("Material")]
        public CullMode CullMode { get { return _cull; } set { if (!CheckIfMetal()) _cull = value;  } }
        [Category("Material")]
        public bool ZCompareLoc { get { return _transp != 1; } set { if (!CheckIfMetal()) _transp = (byte)(value ? 0 : 1); } }
        [Category("SCN0 References")]
        public sbyte LightSetIndex { get { return _lSet; } set { if (!CheckIfMetal()) { _lSet = value; if (MetalMaterial != null) MetalMaterial.UpdateAsMetal(); } } }
        [Category("SCN0 References")]
        public sbyte FogIndex { get { return _fSet; } set { if (!CheckIfMetal()) { _fSet = value; if (MetalMaterial != null) MetalMaterial.UpdateAsMetal(); } } }

        public enum IndirectMethod
        {
            Warp = 0,
            NormalMap,
            NormalMapSpecular,
            Fur,
            Reserved0,
            Reserved1,
            User0,
            User1
        }
        
        [Category("Material")]
        public IndirectMethod IndirectMethod1 { get { return (IndirectMethod)_indirectMethod1; } set { if (!CheckIfMetal()) _indirectMethod1 = (byte)value; } }
        [Category("Material")]
        public IndirectMethod IndirectMethod2 { get { return (IndirectMethod)_indirectMethod2; } set { if (!CheckIfMetal()) _indirectMethod2 = (byte)value; } }
        [Category("Material")]
        public IndirectMethod IndirectMethod3 { get { return (IndirectMethod)_indirectMethod3; } set { if (!CheckIfMetal()) _indirectMethod3 = (byte)value; } }
        [Category("Material")]
        public IndirectMethod IndirectMethod4 { get { return (IndirectMethod)_indirectMethod4; } set { if (!CheckIfMetal()) _indirectMethod4 = (byte)value; } }

        [Category("SCN0 References")]
        public sbyte NormMapRefLight1 { get { return _normMapRefLight1; } set { if (!CheckIfMetal()) _normMapRefLight1 = value; } }
        [Category("SCN0 References")]
        public sbyte NormMapRefLight2 { get { return _normMapRefLight2; } set { if (!CheckIfMetal()) _normMapRefLight1 = value; } }
        [Category("SCN0 References")]
        public sbyte NormMapRefLight3 { get { return _normMapRefLight3; } set { if (!CheckIfMetal()) _normMapRefLight1 = value; } }
        [Category("SCN0 References")]
        public sbyte NormMapRefLight4 { get { return _normMapRefLight4; } set { if (!CheckIfMetal()) _normMapRefLight1 = value; } }
        
//#if DEBUG
//        [Category("Material")]
//        public int NumTextures { get { return _numTextures; } }
//        [Category("Material")]
//        public int ShaderOffset { get { return _shaderOffset; } }
//        [Category("Material")]
//        public int MaterialRefOffset { get { return _matRefOffset; } }
//        [Category("Material")]
//        public int UserDataOffset { get { return _userDataOffset; } }
//        [Category("Material")]
//        public int DisplayListOffset { get { return _dlOffset; } }
//        [Category("Material")]
//        public int FurDataOffset { get { return _furDataOffset; } }
//        [Category("Material")]
//        public int Pad { get { return _pad; } }
//#endif
        #endregion

        #region Metal

        public bool _updating = false;
        public void UpdateAsMetal()
        {
            if (!isMetal)
                return;

            _updating = true;
            if (ShaderNode != null && ShaderNode._autoMetal && ShaderNode.texCount == Children.Count)
            {
                //ShaderNode.DefaultAsMetal(Children.Count); 
            }
            else
            {
                bool found = false;
                foreach (MDL0ShaderNode s in Model._shadGroup.Children)
                {
                    if (s._autoMetal && s.texCount == Children.Count)
                    {
                        ShaderNode = s;
                        found = true;
                    }
                    else
                    {
                        if (s._stages == 4)
                        {
                            foreach (MDL0MaterialNode y in s._materials)
                                if (!y.isMetal || y.Children.Count != Children.Count)
                                    goto NotFound;
                            ShaderNode = s;
                            found = true;
                            goto End;
                        NotFound:
                            continue;
                        }
                    }
                }
            End:
                if (!found)
                {
                    MDL0ShaderNode shader = new MDL0ShaderNode();
                    Model._shadGroup.AddChild(shader);
                    shader.DefaultAsMetal(Children.Count);
                    ShaderNode = shader;
                }
            }

            if (MetalMaterial != null)
            {
                Name = MetalMaterial.Name + "_ExtMtl";
                _ssc = 4;

                if (Children.Count - 1 != MetalMaterial.Children.Count)
                {
                    //Remove all children
                    for (int i = 0; i < Children.Count; i++)
                    {
                        ((MDL0MaterialRefNode)Children[i]).TextureNode = null;
                        ((MDL0MaterialRefNode)Children[i]).PaletteNode = null;
                        RemoveChild(Children[i--]);
                    }

                    //Start over
                    for (int i = 0; i <= MetalMaterial.Children.Count; i++)
                    {
                        MDL0MaterialRefNode mr = new MDL0MaterialRefNode();

                        AddChild(mr);
                        mr.Texture = "metal00";
                        mr._index1 = mr._index2 = i;

                        mr._texFlags.TexScale = new Vector2(1);
                        mr._bindState._scale = new Vector3(1);
                        mr._texMatrix.TexMtx = Matrix43.Identity;
                        mr._texMatrix.SCNCamera = -1;
                        mr._texMatrix.SCNLight = -1;
                        mr._texMatrix.Identity = 1;

                        if (i == MetalMaterial.Children.Count)
                        {
                            mr._minFltr = 5;
                            mr._magFltr = 1;
                            mr._lodBias = -2;
                            mr.HasTextureMatrix = true;
                            mr._projection = (int)TexProjection.STQ;
                            mr._inputForm = (int)TexInputForm.ABC1;
                            mr._sourceRow = (int)TexSourceRow.Normals;
                            mr.Normalize = true;
                            mr.MapMode = (MDL0MaterialRefNode.MappingMethod)1;
                        }
                        else
                        {
                            mr._projection = (int)TexProjection.ST;
                            mr._inputForm = (int)TexInputForm.AB11;
                            mr._sourceRow = (int)TexSourceRow.TexCoord0 + i;
                            mr.Normalize = false;
                            mr.MapMode = (MDL0MaterialRefNode.MappingMethod)0;
                        }

                        mr._texGenType = (int)TexTexgenType.Regular;
                        mr._embossSource = 4;
                        mr._embossLight = 2;

                        mr.getTexMtxVal();
                    }

                    _chan1 = new LightChannel(63, new RGBAPixel(128, 128, 128, 255), new RGBAPixel(255, 255, 255, 255), 0, 0, this);
                    C1ColorEnabled = true;
                    C1ColorDiffuseFunction = GXDiffuseFn.Clamped;
                    C1ColorAttenuation = GXAttnFn.Spotlight;
                    C1AlphaEnabled = true;
                    C1AlphaDiffuseFunction = GXDiffuseFn.Clamped;
                    C1AlphaAttenuation = GXAttnFn.Spotlight;

                    _chan2 = new LightChannel(63, new RGBAPixel(255, 255, 255, 255), new RGBAPixel(), 0, 0, this);
                    C2ColorEnabled = true;
                    C2ColorDiffuseFunction = GXDiffuseFn.Disabled;
                    C2ColorAttenuation = GXAttnFn.Specular;
                    C2AlphaDiffuseFunction = GXDiffuseFn.Disabled;
                    C2AlphaAttenuation = GXAttnFn.Specular;

                    _lSet = MetalMaterial._lSet;
                    _fSet = MetalMaterial._fSet;

                    _cull = MetalMaterial._cull;
                    _numLights = 2;
                    ZCompareLoc = false;
                    _normMapRefLight1 =
                    _normMapRefLight2 =
                    _normMapRefLight3 =
                    _normMapRefLight4 = -1;

                    SignalPropertyChange();
                }
            }
            _updating = false;
        }

        public bool CheckIfMetal()
        {
            if (Model != null && Model._autoMetal)
            {
                if (!_updating)
                {
                    if (isMetal)
                        if (MessageBox.Show(null, "This model is currently set to automatically modify metal materials.\nYou cannot make changes unless you turn it off.\nDo you want to turn it off?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            Model._autoMetal = false;
                        else
                            return true;
                }
            }

            SignalPropertyChange();
            return false;
        }

        [Browsable(false)]
        public bool isMetal { get { return Name.EndsWith("_ExtMtl"); } }

        [Browsable(false)]
        public MDL0MaterialNode MetalMaterial
        {
            get
            {
                foreach (MDL0MaterialNode t in Model._matList)
                {
                    if (!isMetal)
                    {
                        if (t.Name.StartsWith(Name) && t.isMetal && (Name.Length + 7 == t.Name.Length))
                            return t;
                    }
                    else if (Name.StartsWith(t.Name) && !t.isMetal && (t.Name.Length + 7 == Name.Length)) return t;
                }
                return null;
            }
        }

        #endregion

        #region Reading & Writing
        internal int _initVersion;
        public override bool OnInitialize()
        {
            MDL0Material* header = Header;

            _initVersion = header->_pad != 0 && _replaced ? header->_pad : Model._version;

            if ((_name == null) && (header->_stringOffset != 0))
                _name = header->ResourceString;

            XFCmds.Clear();

            //Get XF Commands
            byte* pData = (byte*)header->DisplayLists(Model._version) + 0xE0;
        Top:
            if (*pData++ == 0x10)
            {
                XFData dat = new XFData();
                int count = (ushort)*(bushort*)pData; pData += 2;
                dat.addr = (XFMemoryAddr)(ushort)*(bushort*)pData; pData += 2;
                dat.values = new List<uint>();
                for (int i = 0; i < count + 1; i++) 
                {
                    dat.values.Add(*(buint*)pData); 
                    pData += 4; 
                }
                XFCmds.Add(dat);
                goto Top;
            }

            _mdl0Offset = header->_mdl0Offset;
            _stringOffset = header->_stringOffset;
            _userDataOffset = header->UserDataOffset(_initVersion);
            _shaderOffset = header->_shaderOffset;
            _dlOffset = header->DisplayListOffset(_initVersion);
            _furDataOffset = header->FurDataOffset(_initVersion);
            _matRefOffset = header->_matRefOffset;
            _pad = header->_pad;

            _dataLen = header->_dataLen;
            _numTextures = header->_numTexGens;
            _numLights = header->_numLightChans;
            _usageFlags = new Bin32(header->_usageFlags);

            _indirectMethod1 = header->_indirectMethod1;
            _indirectMethod2 = header->_indirectMethod2;
            _indirectMethod3 = header->_indirectMethod3;
            _indirectMethod4 = header->_indirectMethod4;
            
            _normMapRefLight1 = header->_normMapRefLight1;
            _normMapRefLight2 = header->_normMapRefLight2;
            _normMapRefLight3 = header->_normMapRefLight3;
            _normMapRefLight4 = header->_normMapRefLight4;

            _ssc = header->_activeTEVStages;
            _clip = header->_numIndTexStages;
            _transp = header->_enableAlphaTest;

            _lSet = header->_lightSet;
            _fSet = header->_fogSet;

            _cull = (CullMode)(int)header->_cull;

            if ((-header->_mdl0Offset + (int)header->DisplayListOffset(_initVersion)) % 0x20 != 0)
            {
                Model._errors.Add("Material " + Index + " has an improper align offset.");
                SignalPropertyChange();
            }

            mode = header->DisplayLists(_initVersion);
            _alphaFunc = mode->AlphaFunction;
            _zMode = mode->ZMode;
            _blendMode = mode->BlendMode;
            _constantAlpha = mode->ConstantAlpha;

            _tevColorBlock = *header->TevColorBlock(_initVersion);
            _tevKonstBlock = *header->TevKonstBlock(_initVersion);
            _indMtx = *header->IndMtxBlock(_initVersion);

            MDL0TexSRTData* TexMatrices = header->TexMatrices(_initVersion);

            _layerFlags = TexMatrices->_layerFlags;
            _texMtxFlags = TexMatrices->_mtxFlags;

            MDL0MaterialLighting* Light = header->Light(_initVersion);

            (_chan1 = Light->Channel1)._parent = this;
            (_chan2 = Light->Channel2)._parent = this;

            c1 = CReg2Color;
            c2 = CReg2Color;
            c3 = CReg2Color;

            k1 = KReg0Color;
            k2 = KReg1Color;
            k3 = KReg2Color;
            k3 = KReg3Color;

            clr1 = C1MaterialColor;
            clr2 = C2MaterialColor;

            amb1 = C1AmbientColor;
            amb2 = C2AmbientColor;

            (_userEntries = new UserDataCollection()).Read(header->UserData(_initVersion));
            
            return true;
        }

        public override void OnPopulate()
        {
            MDL0TextureRef* first = Header->First;
            for (int i = 0; i < Header->_numTextures; i++)
                new MDL0MaterialRefNode().Initialize(this, first++, MDL0TextureRef.Size);
        }

        internal override void GetStrings(StringTable table)
        {
            table.Add(Name);

            foreach (UserDataClass s in _userEntries)
            {
                table.Add(s._name);
                if (s._type == UserValueType.String && s._entries.Count > 0)
                    table.Add(s._entries[0]);
            }

            foreach (MDL0MaterialRefNode n in Children)
                n.GetStrings(table);
        }

        public int _dataAlign = 0, _mdlOffset = 0;
        public override int OnCalculateSize(bool force)
        {
            int temp, size;

            //Add header and tex matrices size at start
            size = 0x414 + (Model._version >= 10 ? 4 : 0);

            //Add children size
            size += Children.Count * MDL0TextureRef.Size;

            //Add user entries, if there are any
            size += _userEntries.GetSize();

            //Set temp align offset
            temp = size;

            //Align data to an offset divisible by 0x20 using data length.
            size = size.Align(0x10) + _dataAlign;
            if ((size + _mdlOffset) % 0x20 != 0)
                size += ((size - 0x10 >= temp) ? -0x10 : 0x10);

            //Reset data alignment
            _dataAlign = 0;

            //Add display list and XF flags
            size += 0x180;

            return size;
        }

        public bool New = false;
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            MDL0Material* header = (MDL0Material*)address;

            ushort i1 = 0x1040, i2 = 0x1050; int mtx = 0;

            //Set offsets
            header->_dataLen = _dataLen = length;

            int addr = 0;
            if (Model._version >= 10)
            {
                header->_dlOffset = 0; //Fur Data not supported
                header->_dlOffsetv10p = length - 0x180;
                if (Children.Count > 0)
                    header->_matRefOffset = addr = 1048;
                else
                    header->_matRefOffset = 0;
            }
            else
            {
                header->_dlOffset = length - 0x180;
                if (Children.Count > 0)
                    header->_matRefOffset = addr = 1044;
                else
                    header->_matRefOffset = 0;
            }

            //Check for user entries
            if (_userEntries.Count > 0)
            {
                addr += Children.Count * 0x34;
                if (Model._version == 11 || Model._version == 10)
                    header->_dlOffset = addr;
                else
                    header->_userDataOffset = addr;
                
                _userEntries.Write(header->UserData(Model._version));
            }
            else
                addr = header->_userDataOffset = 0;

            //Set defaults if the model is an import or the material was created
            if (Model._isImport || New)
            {
                if (New)
                {
                    _lSet = 20;
                    _fSet = 4;
                    _ssc = 3;

                    C1ColorEnabled = true;
                    C1AlphaMaterialSource = GXColorSrc.Vertex;
                    C1ColorMaterialSource = GXColorSrc.Vertex;
                    C1ColorDiffuseFunction = GXDiffuseFn.Clamped;
                    C1ColorAttenuation = GXAttnFn.Spotlight;
                    C1AlphaEnabled = true;
                    C1AlphaDiffuseFunction = GXDiffuseFn.Clamped;
                    C1AlphaAttenuation = GXAttnFn.Spotlight;

                    C2ColorDiffuseFunction = GXDiffuseFn.Disabled;
                    C2ColorAttenuation = GXAttnFn.None;
                    C2AlphaDiffuseFunction = GXDiffuseFn.Disabled;
                    C2AlphaAttenuation = GXAttnFn.None;
                }

                //Set default texgen flags
                for (int i = 0; i < Children.Count; i++)
                {
                    MDL0MaterialRefNode node = ((MDL0MaterialRefNode)Children[i]);

                    //Tex Mtx
                    XFData dat = new XFData();
                    dat.addr = (XFMemoryAddr)i1++;
                    XFTexMtxInfo tex = new XFTexMtxInfo();
                    tex._data = (uint)(0 | 
                        ((int)TexProjection.ST << 1) |
                        ((int)TexInputForm.AB11 << 2) |
                        ((int)TexTexgenType.Regular << 4) |
                        ((int)(0x5) << 7) |
                        (4 << 10) | 
                        (2 << 13));
                    dat.values.Add(tex._data); 
                    XFCmds.Add(dat);
                    node.TexMtxFlags = tex;

                    //Dual Tex
                    dat = new XFData();
                    dat.addr = (XFMemoryAddr)i2++;
                    XFDualTex dtex = new XFDualTex(mtx, 0); mtx += 3;
                    dat.values.Add(dtex.Value);
                    XFCmds.Add(dat);
                    node.DualTexFlags = dtex;
                    node.getValues();
                    node._texFlags.TexScale = new Vector2(1);
                    node._bindState._scale = new Vector3(1);
                    node._texMatrix.TexMtx = Matrix43.Identity;
                    node._texMatrix.SCNCamera = -1;
                    node._texMatrix.SCNLight = -1;
                    node._texMatrix.MapMode = 0;
                    node._texMatrix.Identity = 1;
                }
            }

            //Set header values
            header->_numTextures = Children.Count;
            header->_numTexGens = _numTextures = (byte)Children.Count;
            header->_index = Index;
            header->_numLightChans = _numLights;
            header->_activeTEVStages = (byte)_ssc;
            header->_numIndTexStages = _clip;
            header->_enableAlphaTest = _transp;

            header->_lightSet = _lSet;
            header->_fogSet = _fSet;
            header->_pad = 0;

            header->_cull = (int)_cull;
            header->_usageFlags = _usageFlags._data;

            header->_indirectMethod1 = _indirectMethod1;
            header->_indirectMethod2 = _indirectMethod2;
            header->_indirectMethod3 = _indirectMethod3;
            header->_indirectMethod4 = _indirectMethod4;

            header->_normMapRefLight1 = _normMapRefLight1;
            header->_normMapRefLight2 = _normMapRefLight2;
            header->_normMapRefLight3 = _normMapRefLight3;
            header->_normMapRefLight4 = _normMapRefLight4;

            //Generate layer flags and write texture matrices
            MDL0TexSRTData* TexSettings = header->TexMatrices(Model._version);
            *TexSettings = MDL0TexSRTData.Default;

            _layerFlags = 0;
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                MDL0MaterialRefNode node = (MDL0MaterialRefNode)Children[i];

                node._flags |= TexFlags.Enabled;

                node._texFlags.TexScale = new Vector2(node._bindState._scale._x, node._bindState._scale._y);
                node._texFlags.TexRotation = node._bindState._rotate._x;
                node._texFlags.TexTranslation = new Vector2(node._bindState._translate._x, node._bindState._translate._y);

                //Check for non-default values
                if (node._texFlags.TexScale != new Vector2(1))
                    node._flags &= 0xF - TexFlags.FixedScale;
                else
                    node._flags |= TexFlags.FixedScale;

                if (node._texFlags.TexRotation != 0)
                    node._flags &= 0xF - TexFlags.FixedRot;
                else
                    node._flags |= TexFlags.FixedRot;

                if (node._texFlags.TexTranslation != new Vector2(0))
                    node._flags &= 0xF - TexFlags.FixedTrans;
                else
                    node._flags |= TexFlags.FixedTrans;

                TexSettings->SetTexFlags(node._texFlags, node.Index);
                TexSettings->SetTexMatrices(node._texMatrix, node.Index);

                _layerFlags = ((_layerFlags << 4) | (byte)node._flags);
            }

            TexSettings->_layerFlags = _layerFlags;
            TexSettings->_mtxFlags = _texMtxFlags;

            //Write lighting flags
            MDL0MaterialLighting* Light = header->Light(Model._version);

            Light->Channel1 = _chan1;
            Light->Channel2 = _chan2;

            //The shader offset will be written later

            //Rebuild references
            MDL0TextureRef* mRefs = header->First;
            foreach (MDL0MaterialRefNode n in Children)
                n.Rebuild(mRefs++, 0x34, force);
            
            //Set Display Lists
            *header->TevKonstBlock(Model._version) = _tevKonstBlock;
            *header->TevColorBlock(Model._version) = _tevColorBlock;
            *header->IndMtxBlock(Model._version) = _indMtx;

            mode = header->DisplayLists(Model._version);
            *mode = MatModeBlock.Default;
            if (Model._isImport)
            {
                _alphaFunc = mode->AlphaFunction;
                _zMode = mode->ZMode;
                _blendMode = mode->BlendMode;
                _constantAlpha = mode->ConstantAlpha;
            }
            else
            {
                mode->AlphaFunction = _alphaFunc;
                mode->ZMode = _zMode;
                mode->BlendMode = _blendMode;
                mode->ConstantAlpha = _constantAlpha;
            }

            //Write XF flags
            byte* xfData = (byte*)header->DisplayLists(Model._version) + 0xE0;
            i1 = 0x1040; i2 = 0x1050; mtx = 0;
            foreach (MDL0MaterialRefNode mr in Children)
            {
                //Tex Mtx
                *xfData++ = 0x10;
                *(bushort*)xfData = 0; xfData += 2;
                *(bushort*)xfData = (ushort)i1++;  xfData += 2;
                *(buint*)xfData = mr.TexMtxFlags._data; xfData += 4;

                //Dual Tex
                *xfData++ = 0x10;
                *(bushort*)xfData = 0; xfData += 2;
                *(bushort*)xfData = (ushort)i2++; xfData += 2;
                *(buint*)xfData = new XFDualTex(mtx, mr.DualTexFlags.NormalEnable).Value; xfData += 4;
                
                mtx += 3;
            }
            
            New = false;
        }

        protected internal override void PostProcess(VoidPtr mdlAddress, VoidPtr dataAddress, StringTable stringTable)
        {
            MDL0Material* header = (MDL0Material*)dataAddress;
            header->_mdl0Offset = (int)mdlAddress - (int)dataAddress;
            header->_stringOffset = (int)stringTable[Name] + 4 - (int)dataAddress;
            header->_index = Index;

            _userEntries.PostProcess(header->UserData(Model._version), stringTable);

            MDL0TextureRef* first = header->First;
            foreach (MDL0MaterialRefNode n in Children)
                n.PostProcess(mdlAddress, first++, stringTable);
        }
        
        #endregion

        #region Rendering

        public bool _renderUpdate = false;
        public new void SignalPropertyChange()
        {
            _renderUpdate = true;
            base.SignalPropertyChange();
        }

        public bool written = false;

        public SCN0FogNode _fog;
        public SCN0LightSetNode _lightSet;
        public int _animFrame;

        public void Render(TKContext ctx, ModelPanel mainWindow)
        {
            foreach (MDL0ObjectNode p in _objects)
                p.Render(ctx, false, mainWindow);

            #region Old

            //if (Model._mainWindow != null && Model._mainWindow._scn0 != null && (LightSet >= 0 || FogSet >= 0))
            //{
            //    ModelEditControl m = Model._mainWindow;
            //    SCN0Node scn = m._scn0;
            //    int animFrame = m._animFrame;
            //    SCN0GroupNode fog;
            //    if (FogSet >= 0 && (fog = scn.GetFolder<SCN0FogNode>()) != null && fog.Children.Count > FogSet)
            //    {
            //        SCN0FogNode f = fog.Children[FogSet] as SCN0FogNode;
            //        GL.Enable(EnableCap.Fog);
            //        uint mode = 0;
            //        switch (f.Type)
            //        {
            //            case FogType.OrthographicExp:
            //            case FogType.PerspectiveExp:
            //                mode = (uint)OpenTK.Graphics.OpenGL.FogMode.Exp;
            //                break;
            //            case FogType.OrthographicExp2:
            //            case FogType.PerspectiveExp2:
            //                mode = (uint)OpenTK.Graphics.OpenGL.FogMode.Exp2;
            //                break;
            //            case FogType.OrthographicLinear:
            //            case FogType.PerspectiveLinear:
            //                mode = (uint)OpenTK.Graphics.OpenGL.FogMode.Linear;
            //                break;
            //            case FogType.OrthographicRevExp:
            //            case FogType.PerspectiveRevExp:
            //                mode = (uint)OpenTK.Graphics.OpenGL.FogMode.Linear;
            //                break;
            //            case FogType.OrthographicRevExp2:
            //            case FogType.PerspectiveRevExp2:
            //                mode = (uint)OpenTK.Graphics.OpenGL.FogMode.Linear;
            //                break;
            //        }
            //        GL.Fog(OpenTK.Graphics.OpenGL.FogParameter.FogMode, mode);
            //        float* l = stackalloc float[4];
            //        if (f.Colors.Count == 1)
            //        {
            //            l[0] = (float)f.Colors[0].R / 255f;
            //            l[1] = (float)f.Colors[0].G / 255f;
            //            l[2] = (float)f.Colors[0].B / 255f;
            //            l[3] = (float)f.Colors[0].A / 255f;
            //        }
            //        else if (animFrame - 1 < f.Colors.Count)
            //        {
            //            l[0] = (float)f.Colors[animFrame - 1].R / 255f;
            //            l[1] = (float)f.Colors[animFrame - 1].G / 255f;
            //            l[2] = (float)f.Colors[animFrame - 1].B / 255f;
            //            l[3] = (float)f.Colors[animFrame - 1].A / 255f;
            //        }
            //        GL.Fog(OpenTK.Graphics.OpenGL.FogParameter.FogColor, l);
            //        //ctx.glFog(FogParameter.FogDensity, 0.05f);
            //        GL.Hint(HintTarget.FogHint, HintMode.Nicest);
            //        GL.Fog(OpenTK.Graphics.OpenGL.FogParameter.FogStart, f._startKeys.GetFrameValue(animFrame - 1));
            //        GL.Fog(OpenTK.Graphics.OpenGL.FogParameter.FogEnd, f._endKeys.GetFrameValue(animFrame - 1));
            //    }
            //    else
            //        GL.Disable(EnableCap.Fog);
            //}
            //else
            //    GL.Disable(EnableCap.Fog);

            #endregion
        }

        internal override void Bind(TKContext ctx)
        {
            _renderUpdate = true;
        }

        internal override void Unbind() 
        {
            foreach (MDL0MaterialRefNode m in Children) 
                m.Unbind();
        }

        internal void ApplySRT0(SRT0Node node, int index, bool linear)
        {
            SRT0EntryNode e;

            if (node == null || index == 0)
                foreach (MDL0MaterialRefNode r in Children)
                    r.ApplySRT0Texture(null, 0, linear);
            else if ((e = node.FindChild(Name, false) as SRT0EntryNode) != null)
            {
                foreach (SRT0TextureNode t in e.Children)
                    if (t._textureIndex < Children.Count)
                        ((MDL0MaterialRefNode)Children[t._textureIndex]).ApplySRT0Texture(t, index, linear);
            }
            else
                foreach (MDL0MaterialRefNode r in Children)
                    r.ApplySRT0Texture(null, 0, linear);
        }
        
        internal void ApplyCLR0(CLR0Node node, int index)
        {
            if (node == null) return;
            CLR0MaterialNode mat = node.FindChild(Name, false) as CLR0MaterialNode;
            if (mat != null)
                foreach (CLR0MaterialEntryNode e in mat.Children)
                {
                    ARGBPixel p = e.Colors.Count > index ? e.Colors[index] : new ARGBPixel();
                    switch (e.Target)
                    {
                        case EntryTarget.Ambient0: amb1 = (RGBAPixel)p; break;
                        case EntryTarget.Ambient1: amb2 = (RGBAPixel)p; break;
                        case EntryTarget.Color0: clr1 = (RGBAPixel)p; break;
                        case EntryTarget.Color1: clr2 = (RGBAPixel)p; break;
                        case EntryTarget.TevColorReg0: c1 = (GXColorS10)p; break;
                        case EntryTarget.TevColorReg1: c2 = (GXColorS10)p; break;
                        case EntryTarget.TevColorReg2: c3 = (GXColorS10)p; break;
                        case EntryTarget.TevKonstReg0: k1 = (GXColorS10)p; break;
                        case EntryTarget.TevKonstReg1: k2 = (GXColorS10)p; break;
                        case EntryTarget.TevKonstReg2: k3 = (GXColorS10)p; break;
                        case EntryTarget.TevKonstReg3: k4 = (GXColorS10)p; break;
                    }
                }
        }

        internal unsafe void ApplyPAT0(PAT0Node node, int index)
        {
            PAT0EntryNode e;

            if (node == null || index == 0)
                foreach (MDL0MaterialRefNode r in Children)
                    r.ApplyPAT0Texture(null, 0);
            else if ((e = node.FindChild(Name, false) as PAT0EntryNode) != null)
            {
                foreach (PAT0TextureNode t in e.Children)
                    if (t._textureIndex < Children.Count)
                        ((MDL0MaterialRefNode)Children[t._textureIndex]).ApplyPAT0Texture(t, index);
            }
            else
                foreach (MDL0MaterialRefNode r in Children)
                    r.ApplyPAT0Texture(null, 0);
        }
        public int renderFrame = 0;
        internal unsafe void SetSCN0(SCN0Node node)
        {
            if (node == null)
            {
                _lightSet = null;
                _fog = null;
            }
            else
            {
                SCN0GroupNode g = node.GetFolder<SCN0LightSetNode>();
                _lightSet = LightSetIndex < g.Children.Count && LightSetIndex >= 0 ? g.Children[LightSetIndex] as SCN0LightSetNode : null;
                g = node.GetFolder<SCN0FogNode>();
                _fog = FogIndex < g.Children.Count && FogIndex >= 0 ? g.Children[FogIndex] as SCN0FogNode : null;
            }
        }

        public void SetSCN0Frame(int frame)
        {
            renderFrame = frame;
        }

        internal unsafe void ApplySCN0(int index)
        {
            
        }

        #endregion

        public override unsafe void Replace(string fileName)
        {
            base.Replace(fileName);

            Model.CheckTextures();
        }

        public override void Remove()
        {
            if (Parent != null)
            {
                ShaderNode = null;

                foreach (MDL0MaterialRefNode r in Children)
                {
                    r.TextureNode = null;
                    r.PaletteNode = null;
                }

                Model.CheckTextures();
            }
            base.Remove();
        }

        public override void RemoveChild(ResourceNode child)
        {
            base.RemoveChild(child);

            if (!_updating && Model._autoMetal && MetalMaterial != null && !this.isMetal)
                MetalMaterial.UpdateAsMetal();
        }

        public override unsafe void Export(string outPath)
        {
            StringTable table = new StringTable();
            GetStrings(table);
            int dataLen = CalculateSize(false);
            int totalLen = dataLen + table.GetTotalSize();

            using (FileStream stream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.RandomAccess))
            {
                stream.SetLength(totalLen);
                using (FileMap map = FileMap.FromStream(stream))
                {
                    Rebuild(map.Address, dataLen, false);
                    table.WriteTable(map.Address + dataLen);
                    PostProcess(map.Address, map.Address, table);
                    ((MDL0Material*)map.Address)->_pad = (byte)Model._version;
                }
            }
        }
    }

    public class LightChannel
    {
        public MDL0MaterialNode _parent = null;

        public LightChannel(uint flags, RGBAPixel mat, RGBAPixel amb, uint color, uint alpha, MDL0MaterialNode material) : this(flags, mat, amb, color, alpha) { _parent = material; }
        public LightChannel(uint flags, RGBAPixel mat, RGBAPixel amb, uint color, uint alpha)
        {
            _flags = flags;
            _matColor = mat;
            _ambColor = amb;
            _color = new LightChannelControl(color) { _parent = this };
            _alpha = new LightChannelControl(alpha) { _parent = this };
        }

        [Category("Lighting Channel")]
        public LightingChannelFlags Flags { get { return (LightingChannelFlags)_flags; } set { _flags = (uint)value; _parent.SignalPropertyChange(); } }

        [Category("Lighting Channel"), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel MaterialColor { get { return _matColor; } set { _matColor = value; _parent.SignalPropertyChange(); } }

        [Category("Lighting Channel"), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel AmbientColor { get { return _ambColor; } set { _ambColor = value; _parent.SignalPropertyChange(); } }

        [Category("Lighting Channel"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public LightChannelControl Color
        {
            get { return _color; }
            set { _color = value; _color._parent = this; _parent.SignalPropertyChange(); }
        }

        [Category("Lighting Channel"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public LightChannelControl Alpha
        {
            get { return _alpha; }
            set { _alpha = value; _alpha._parent = this; _parent.SignalPropertyChange(); }
        }

        [Category("Lighting Channel"), Browsable(false)]
        public GXColorSrc ColorMaterialSource
        {
            get { return _color.MaterialSource; }
            set { _color.MaterialSource = value; _parent.SignalPropertyChange(); }
        }
        [Category("Lighting Channel"), Browsable(false)]
        public bool ColorEnabled
        {
            get { return _color.Enabled; }
            set { _color.Enabled = value; _parent.SignalPropertyChange(); }
        }
        [Category("Lighting Channel"), Browsable(false)]
        public GXColorSrc ColorAmbientSource
        {
            get { return _color.AmbientSource; }
            set { _color.AmbientSource = value; _parent.SignalPropertyChange(); }
        }
        [Category("Lighting Channel"), Browsable(false)]
        public GXDiffuseFn ColorDiffuseFunction
        {
            get { return _color.DiffuseFunction; }
            set { _color.DiffuseFunction = value; _parent.SignalPropertyChange(); }
        }
        [Category("Lighting Channel"), Browsable(false)]
        public GXAttnFn ColorAttenuation
        {
            get { return _color.Attenuation; }
            set { _color.Attenuation = value; _parent.SignalPropertyChange(); }
        }
        [Category("Lighting Channel"), Browsable(false)]
        public MatChanLights ColorLights
        {
            get { return _color.Lights; }
            set { _color.Lights = value; _parent.SignalPropertyChange(); }
        }
        [Category("Lighting Channel"), Browsable(false)]
        public GXColorSrc AlphaMaterialSource
        {
            get { return _alpha.MaterialSource; }
            set { _alpha.MaterialSource = value; _parent.SignalPropertyChange(); }
        }
        [Category("Lighting Channel"), Browsable(false)]
        public bool AlphaEnabled
        {
            get { return _alpha.Enabled; }
            set { _alpha.Enabled = value; _parent.SignalPropertyChange(); }
        }
        [Category("Lighting Channel"), Browsable(false)]
        public GXColorSrc AlphaAmbientSource
        {
            get { return _alpha.AmbientSource; }
            set { _alpha.AmbientSource = value; _parent.SignalPropertyChange(); }
        }
        [Category("Lighting Channel"), Browsable(false)]
        public GXDiffuseFn AlphaDiffuseFunction
        {
            get { return _alpha.DiffuseFunction; }
            set { _alpha.DiffuseFunction = value; _parent.SignalPropertyChange(); }
        }
        [Category("Lighting Channel"), Browsable(false)]
        public GXAttnFn AlphaAttenuation
        {
            get { return _alpha.Attenuation; }
            set { _alpha.Attenuation = value; _parent.SignalPropertyChange(); }
        }
        [Category("Lighting Channel"), Browsable(false)]
        public MatChanLights AlphaLights
        {
            get { return _alpha.Lights; }
            set { _alpha.Lights = value; _parent.SignalPropertyChange(); }
        }

        public uint _flags;
        public RGBAPixel _matColor, _ambColor;
        public LightChannelControl _color, _alpha;
    }

    public enum GXColorSrc
    {
        Register,
        Vertex
    }

    [Flags]
    public enum MatChanLights
    {
        None = 0x0,
        Light0 = 0x1,
        Light1 = 0x2,
        Light2 = 0x4,
        Light3 = 0x8,
        Light4 = 0x10,
        Light5 = 0x20,
        Light6 = 0x40,
        Light7 = 0x80,
    }

    public enum GXDiffuseFn
    {
        Disabled,
        Enabled,
        Clamped
    }

    public enum GXAttnFn
    {
        Specular,
        Spotlight,
        None
    }

    [Flags]
    public enum LightingChannelFlags
    {
        UseMatColor = 0x1,
        UseMatAlpha = 0x2,
        UseAmbColor = 0x4,
        UseAmbAlpha = 0x8,
        UseChanColor = 0x10,
        UseChanAlpha = 0x20
    }

    public class LightChannelControl
    {
        public LightChannel _parent;

        public LightChannelControl(uint ctrl) { _binary = new Bin32(ctrl); }

        public Bin32 _binary;

        //0000 0000 0000 0000 0000 0000 0000 0001   Material Source (GXColorSrc)
        //0000 0000 0000 0000 0000 0000 0000 0010   Light Enabled
        //0000 0000 0000 0000 0000 0000 0011 1100   Light 0123
        //0000 0000 0000 0000 0000 0000 0100 0000   Ambient Source (GXColorSrc)
        //0000 0000 0000 0000 0000 0001 1000 0000   Diffuse Func
        //0000 0000 0000 0000 0000 0010 0000 0000   Attenuation Enable
        //0000 0000 0000 0000 0000 0100 0000 0000   Attenuation Function (0 = Specular)
        //0000 0000 0000 0000 0111 1000 0000 0000   Light 4567

        [Category("Lighting Control")]
        public bool Enabled { get { return _binary[1]; } set { _binary[1] = value; if (_parent != null) _parent._parent.SignalPropertyChange(); } }
        [Category("Lighting Control")]
        public GXColorSrc MaterialSource { get { return (GXColorSrc)(_binary[0] ? 1 : 0); } set { _binary[0] = ((int)value != 0); if (_parent != null) _parent._parent.SignalPropertyChange(); } }
        [Category("Lighting Control")]
        public GXColorSrc AmbientSource { get { return (GXColorSrc)(_binary[6] ? 1 : 0); } set { _binary[6] = ((int)value != 0); if (_parent != null) _parent._parent.SignalPropertyChange(); } }
        [Category("Lighting Control")]
        public GXDiffuseFn DiffuseFunction { get { return (GXDiffuseFn)(_binary[7, 2]); } set { _binary[7, 2] = ((uint)value); if (_parent != null) _parent._parent.SignalPropertyChange(); } }
        [Category("Lighting Control")]
        public GXAttnFn Attenuation
        {
            get
            {
                if (!_binary[9])
                    return GXAttnFn.None;
                else
                    return (GXAttnFn)(_binary[10] ? 1 : 0);
            }
            set
            {
                if (value != GXAttnFn.None)
                {
                    _binary[9] = true;
                    _binary[10] = ((int)value) != 0;
                }
                else
                {
                    _binary[9] = false;
                    _binary[10] = false;
                }

                if (_parent != null)
                    _parent._parent.SignalPropertyChange();
            }
        }
        [Category("Lighting Control")]
        public MatChanLights Lights
        {
            get { return (MatChanLights)(_binary[2, 4] | (_binary[11, 4] << 4)); }
            set
            {
                uint val = (uint)value;
                _binary[2, 4] = (val & 0xF);
                _binary[11, 4] = ((val >> 4) & 0xF);

                if (_parent != null)
                    _parent._parent.SignalPropertyChange();
            }
        }
    }
}
