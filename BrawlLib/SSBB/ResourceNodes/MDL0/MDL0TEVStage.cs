using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Wii.Graphics;
using BrawlLib.IO;
using BrawlLib.Imaging;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe partial class TEVStage : MDL0EntryNode
    {
        public TEVStage() { }
        public TEVStage(ColorEnv colEnv, AlphaEnv alphaEnv, CMD cmd, TevKColorSel kc, TevKAlphaSel ka, TexMapID id, TexCoordID coord, ColorSelChan col, bool useTex) 
        {
            _colorEnv = colEnv;
            _alphaEnv = alphaEnv;
            _cmd = cmd;
            _kcSel = kc;
            _kaSel = ka;
            _texMapID = id;
            _texCoord = coord;
            _colorChan = col;
            _texEnabled = useTex;
        }

        public override ResourceType ResourceType { get { return ResourceType.TEVStage; } }
        public override string Name
        {
            get { return String.Format("Stage{0}", Index); }
            set { base.Name = value; }
        }

        public ColorEnv _colorEnv = new ColorEnv();
        public AlphaEnv _alphaEnv = new AlphaEnv();
        public CMD _cmd = new CMD();
        public TevKColorSel _kcSel;
        public TevKAlphaSel _kaSel;
        public TexMapID _texMapID;
        public TexCoordID _texCoord;
        public ColorSelChan _colorChan;
        public bool _texEnabled;

        //public string ColorEnv { get { return "0x" + ((uint)(int)_colorEnv).ToString("X"); } }
        //public string AlphaEnv { get { return "0x" + ((uint)(int)_alphaEnv).ToString("X"); } }
        //public string CMD { get { return "0x" + ((uint)(int)_cmd).ToString("X"); } }
        
        [Category("c TEV Color Env")]
        public string ColorOutput { get { return (ColorClamp ? "clamp(" : "") + "(d " + (ColorSubtract ? "-" : "+") + " ((1 - c) * a + c * b)" + ((int)ColorBias == 1 ? " + 0.5" : (int)ColorBias == 2 ? " - 0.5" : "") + ") * " + ((int)ColorScale == 3 ? "0.5" : (int)ColorScale == 0 ? "1" : ((int)ColorScale * 2).ToString()) + (ColorClamp ? ");" : ";"); } }
        [Category("d TEV Alpha Env")]
        public string AlphaOutput { get { return (AlphaClamp ? "clamp(" : "") + "(d " + (AlphaSubtract ? "-" : "+") + " ((1 - c) * a + c * b)" + ((int)AlphaBias == 1 ? " + 0.5" : (int)AlphaBias == 2 ? " - 0.5" : "") + ") * " + ((int)AlphaScale == 3 ? "0.5" : (int)AlphaScale == 0 ? "1" : ((int)AlphaScale * 2).ToString()) + (AlphaClamp ? ");" : ";"); } }

        [Category("a TEV KSel")]
        public TevKColorSel KonstantColorSelection { get { return _kcSel; } set { _kcSel = value; SignalPropertyChange(); } }
        [Category("a TEV KSel")]
        public TevKAlphaSel KonstantAlphaSelection { get { return _kaSel; } set { _kaSel = value; SignalPropertyChange(); } }
        
        [Category("b TEV RAS1 TRef")]
        public TexMapID TextureMapID { get { return _texMapID; } set { _texMapID = value; SignalPropertyChange(); } }
        [Category("b TEV RAS1 TRef")]
        public TexCoordID TextureCoord { get { return _texCoord; } set { _texCoord = value; SignalPropertyChange(); } }
        [Category("b TEV RAS1 TRef")]
        public bool TextureEnabled { get { return _texEnabled; } set { _texEnabled = value; SignalPropertyChange(); } }
        [Category("b TEV RAS1 TRef")]
        public ColorSelChan ColorChannel { get { return _colorChan; } set { _colorChan = value; SignalPropertyChange(); } }
        
        [Category("c TEV Color Env")]
        public ColorArg ColorSelectionA { get { return _colorEnv.SelA; } set { _colorEnv.SelA = value; SignalPropertyChange(); } }
        [Category("c TEV Color Env")]
        public ColorArg ColorSelectionB { get { return _colorEnv.SelB; } set { _colorEnv.SelB = value; SignalPropertyChange(); } }
        [Category("c TEV Color Env")]
        public ColorArg ColorSelectionC { get { return _colorEnv.SelC; } set { _colorEnv.SelC = value; SignalPropertyChange(); } }
        [Category("c TEV Color Env")]
        public ColorArg ColorSelectionD { get { return _colorEnv.SelD; } set { _colorEnv.SelD = value; SignalPropertyChange(); } }

        [Category("c TEV Color Env")]
        public Bias ColorBias { get { return _colorEnv.Bias; } set { _colorEnv.Bias = value; SignalPropertyChange(); } }

        [Category("c TEV Color Env")]
        public bool ColorSubtract { get { return _colorEnv.Sub; } set { _colorEnv.Sub = value; SignalPropertyChange(); } }
        [Category("c TEV Color Env")]
        public bool ColorClamp { get { return _colorEnv.Clamp; } set { _colorEnv.Clamp = value; SignalPropertyChange(); } }

        [Category("c TEV Color Env")]
        public TevScale ColorScale { get { return _colorEnv.Shift; } set { _colorEnv.Shift = value; SignalPropertyChange(); } }
        [Category("c TEV Color Env")]
        public TevRegID ColorRegister { get { return _colorEnv.Dest; } set { _colorEnv.Dest = value; SignalPropertyChange(); } }
        
        [Category("d TEV Alpha Env")]
        public TevSwapSel AlphaRasterSwap { get { return _alphaEnv.RSwap; } set { _alphaEnv.RSwap = value; SignalPropertyChange(); } }
        [Category("d TEV Alpha Env")]
        public TevSwapSel AlphaTextureSwap { get { return _alphaEnv.TSwap; } set { _alphaEnv.TSwap = value; SignalPropertyChange(); } }

        [Category("d TEV Alpha Env")]
        public AlphaArg AlphaSelectionA { get { return _alphaEnv.SelA; } set { _alphaEnv.SelA = value; SignalPropertyChange(); } }
        [Category("d TEV Alpha Env")]
        public AlphaArg AlphaSelectionB { get { return _alphaEnv.SelB; } set { _alphaEnv.SelB = value; SignalPropertyChange(); } }
        [Category("d TEV Alpha Env")]
        public AlphaArg AlphaSelectionC { get { return _alphaEnv.SelC; } set { _alphaEnv.SelC = value; SignalPropertyChange(); } }
        [Category("d TEV Alpha Env")]
        public AlphaArg AlphaSelectionD { get { return _alphaEnv.SelD; } set { _alphaEnv.SelD = value; SignalPropertyChange(); } }

        [Category("d TEV Alpha Env")]
        public Bias AlphaBias { get { return _alphaEnv.Bias; } set { _alphaEnv.Bias = value; SignalPropertyChange(); } }

        [Category("d TEV Alpha Env")]
        public bool AlphaSubtract { get { return _alphaEnv.Sub; } set { _alphaEnv.Sub = value; SignalPropertyChange(); } }
        [Category("d TEV Alpha Env")]
        public bool AlphaClamp { get { return _alphaEnv.Clamp; } set { _alphaEnv.Clamp = value; SignalPropertyChange(); } }

        [Category("d TEV Alpha Env")]
        public TevScale AlphaScale { get { return _alphaEnv.Shift; } set { _alphaEnv.Shift = value; SignalPropertyChange(); } }
        [Category("d TEV Alpha Env")]
        public TevRegID AlphaRegister { get { return _alphaEnv.Dest; } set { _alphaEnv.Dest = value; SignalPropertyChange(); } }
        
        [Category("e TEV Ind CMD")]
        public IndTexStageID TexStage { get { return _cmd.StageID; } set { _cmd.StageID = value; SignalPropertyChange(); } }
        [Category("e TEV Ind CMD")]
        public IndTexFormat TexFormat { get { return _cmd.Format; } set { _cmd.Format = value; SignalPropertyChange(); } }
        [Category("e TEV Ind CMD")]
        public IndTexBiasSel Bias { get { return _cmd.Bias; } set { _cmd.Bias = value; SignalPropertyChange(); } }
        [Category("e TEV Ind CMD")]
        public IndTexAlphaSel Alpha { get { return _cmd.Alpha; } set { _cmd.Alpha = value; SignalPropertyChange(); } }
        [Category("e TEV Ind CMD")]
        public IndTexMtxID Matrix { get { return _cmd.Matrix; } set { _cmd.Matrix = value; SignalPropertyChange(); } }
        
        [Category("e TEV Ind CMD")]
        public IndTexWrap SWrap { get { return _cmd.SWrap; } set { _cmd.SWrap = value; SignalPropertyChange(); } }
        [Category("e TEV Ind CMD")]
        public IndTexWrap TWrap { get { return _cmd.TWrap; } set { _cmd.TWrap = value; SignalPropertyChange(); } }
        [Category("e TEV Ind CMD")]
        public bool UsePrevStage { get { return _cmd.UsePrevStage; } set { _cmd.UsePrevStage = value; SignalPropertyChange(); } }
        [Category("e TEV Ind CMD")]
        public bool UnmodifiedLOD { get { return _cmd.UnmodifiedLOD; } set { _cmd.UnmodifiedLOD = value; SignalPropertyChange(); } }

        public void Default()
        {
            AlphaSelectionA = AlphaArg.Zero;
            AlphaSelectionB = AlphaArg.Zero;
            AlphaSelectionC = AlphaArg.Zero;
            AlphaSelectionD = AlphaArg.Zero;
            AlphaBias = Wii.Graphics.Bias.Zero;
            AlphaClamp = true;

            ColorSelectionA = ColorArg.Zero;
            ColorSelectionB = ColorArg.Zero;
            ColorSelectionC = ColorArg.Zero;
            ColorSelectionD = ColorArg.Zero;
            ColorBias = Wii.Graphics.Bias.Zero;
            ColorClamp = true;

            TextureMapID = TexMapID.TexMap7;
            TextureCoord = TexCoordID.TexCoord7;
            ColorChannel = ColorSelChan.Zero;
        }

        public void DefaultAsMetal(int texIndex)
        {
            if (Index == 0)
            {
                _colorEnv = 0x28F8AF;
                _alphaEnv = 0x08F2F0;
                KonstantColorSelection = TevKColorSel.KSel_0_Value;
                KonstantAlphaSelection = TevKAlphaSel.KSel_0_Alpha;
                _colorChan = (ColorSelChan)0;
                TextureCoord = TexCoordID.TexCoord0 + texIndex;
                TextureMapID = TexMapID.TexMap0 + texIndex;
                TextureEnabled = true;
            }
            else if (Index == 1)
            {
                _colorEnv = 0x08AFF0;
                _alphaEnv = 0x08FF80;
                KonstantColorSelection = TevKColorSel.KSel_0_Value;
                KonstantAlphaSelection = TevKAlphaSel.KSel_0_Alpha;
                _colorChan = (ColorSelChan)1;
                TextureCoord = TexCoordID.TexCoord7;
                TextureMapID = TexMapID.TexMap7;
                TextureEnabled = false;
            }
            else if (Index == 2)
            {
                _colorEnv = 0x08FEB0;
                _alphaEnv = 0x081FF0;
                KonstantColorSelection = TevKColorSel.KSel_1_Value;
                KonstantAlphaSelection = TevKAlphaSel.KSel_0_Alpha;
                _colorChan = (ColorSelChan)0;
                TextureCoord = TexCoordID.TexCoord7;
                TextureMapID = TexMapID.TexMap7;
                TextureEnabled = false;
            }
            else if (Index == 3)
            {
                _colorEnv = 0x0806EF;
                _alphaEnv = 0x081FF0;
                KonstantColorSelection = TevKColorSel.KSel_0_Value;
                KonstantAlphaSelection = TevKAlphaSel.KSel_0_Alpha;
                _colorChan = (ColorSelChan)7;
                TextureCoord = TexCoordID.TexCoord7;
                TextureMapID = TexMapID.TexMap7;
                TextureEnabled = false;
            }
        }

        public override void Remove()
        {
            if (_parent == null)
                return;

            ((MDL0ShaderNode)Parent).STGs = (byte)(Parent.Children.Count - 1);
            base.Remove();
        }

        internal override void GetStrings(StringTable table) { }

        public new void SignalPropertyChange()
        {
            ((MDL0ShaderNode)Parent)._renderUpdate = true;
            base.SignalPropertyChange();
        }
    }
}