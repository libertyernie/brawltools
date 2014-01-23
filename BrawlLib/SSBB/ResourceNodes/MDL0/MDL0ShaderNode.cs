using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Wii.Graphics;
using BrawlLib.Imaging;
using BrawlLib.OpenGL;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using BrawlLib.Modeling;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class MDL0ShaderNode : MDL0EntryNode
    {
        internal MDL0Shader* Header { get { return (MDL0Shader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.MDL0Shader; } }

        public KSelSwapBlock _swapBlock = KSelSwapBlock.Default;

        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap0Red { get { return (ColorChannel)_swapBlock._Value01.XRB; } set { _swapBlock._Value01.XRB = value; SignalPropertyChange(); } }
        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap0Green { get { return (ColorChannel)_swapBlock._Value01.XGA; } set { _swapBlock._Value01.XGA = value; SignalPropertyChange(); } }

        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap0Blue { get { return (ColorChannel)_swapBlock._Value03.XRB; } set { _swapBlock._Value03.XRB = value; SignalPropertyChange(); } }
        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap0Alpha { get { return (ColorChannel)_swapBlock._Value03.XGA; } set { _swapBlock._Value03.XGA = value; SignalPropertyChange(); } }

        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap1Red { get { return (ColorChannel)_swapBlock._Value05.XRB; } set { _swapBlock._Value05.XRB = value; SignalPropertyChange(); } }
        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap1Green { get { return (ColorChannel)_swapBlock._Value05.XGA; } set { _swapBlock._Value05.XGA = value; SignalPropertyChange(); } }

        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap1Blue { get { return (ColorChannel)_swapBlock._Value07.XRB; } set { _swapBlock._Value07.XRB = value; SignalPropertyChange(); } }
        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap1Alpha { get { return (ColorChannel)_swapBlock._Value07.XGA; } set { _swapBlock._Value07.XGA = value; SignalPropertyChange(); } }

        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap2Red { get { return (ColorChannel)_swapBlock._Value09.XRB; } set { _swapBlock._Value09.XRB = value; SignalPropertyChange(); } }
        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap2Green { get { return (ColorChannel)_swapBlock._Value09.XGA; } set { _swapBlock._Value09.XGA = value; SignalPropertyChange(); } }

        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap2Blue { get { return (ColorChannel)_swapBlock._Value11.XRB; } set { _swapBlock._Value11.XRB = value; SignalPropertyChange(); } }
        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap2Alpha { get { return (ColorChannel)_swapBlock._Value11.XGA; } set { _swapBlock._Value11.XGA = value; SignalPropertyChange(); } }

        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap3Red { get { return (ColorChannel)_swapBlock._Value13.XRB; } set { _swapBlock._Value13.XRB = value; SignalPropertyChange(); } }
        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap3Green { get { return (ColorChannel)_swapBlock._Value13.XGA; } set { _swapBlock._Value13.XGA = value; SignalPropertyChange(); } }

        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap3Blue { get { return (ColorChannel)_swapBlock._Value15.XRB; } set { _swapBlock._Value15.XRB = value; SignalPropertyChange(); } }
        [Category("Swap Mode Table"), Browsable(true)]
        public ColorChannel Swap3Alpha { get { return (ColorChannel)_swapBlock._Value15.XGA; } set { _swapBlock._Value15.XGA = value; SignalPropertyChange(); } }

        //Used by Alpha Env to retrieve what values to swap
        public string[] swapModeTable = new string[4];

        private void BuildSwapModeTable()
        {
	        string swapColors = "rgba";

            //Iterate through the swaps
	        for (int i = 0; i < 4; i++)
	        {
                switch (i)
                {
                    case 0:
                        swapModeTable[i] = new string(new char[] {
                        swapColors[(int)Swap0Red],
                        swapColors[(int)Swap0Green],
                        swapColors[(int)Swap0Blue],
                        swapColors[(int)Swap0Alpha]});
                        break;
                    case 1:
                        swapModeTable[i] = new string(new char[] {
                        swapColors[(int)Swap1Red],
                        swapColors[(int)Swap1Green],
                        swapColors[(int)Swap1Blue],
                        swapColors[(int)Swap1Alpha]});
                        break;
                    case 2:
                        swapModeTable[i] = new string(new char[] {
                        swapColors[(int)Swap2Red],
                        swapColors[(int)Swap2Green],
                        swapColors[(int)Swap2Blue],
                        swapColors[(int)Swap2Alpha]});
                        break;
                    case 3:
                        swapModeTable[i] = new string(new char[] {
                        swapColors[(int)Swap3Red],
                        swapColors[(int)Swap3Green],
                        swapColors[(int)Swap3Blue],
                        swapColors[(int)Swap3Alpha]});
                        break;
                }
	        }
        }

        [Category("TEV RAS1 IRef"), Browsable(true)]
        public TexMapID IndTex0MapID { get { return _swapBlock._Value16.TexMap0; } set { _swapBlock._Value16.TexMap0 = value; SignalPropertyChange(); } }
        [Category("TEV RAS1 IRef"), Browsable(true)]
        public TexCoordID IndTex0Coord { get { return _swapBlock._Value16.TexCoord0; } set { _swapBlock._Value16.TexCoord0 = value; SignalPropertyChange(); } }
        [Category("TEV RAS1 IRef"), Browsable(true)]
        public TexMapID IndTex1MapID { get { return _swapBlock._Value16.TexMap1; } set { _swapBlock._Value16.TexMap1 = value; SignalPropertyChange(); } }
        [Category("TEV RAS1 IRef"), Browsable(true)]
        public TexCoordID IndTex1Coord { get { return _swapBlock._Value16.TexCoord1; } set { _swapBlock._Value16.TexCoord1 = value; SignalPropertyChange(); } }
        [Category("TEV RAS1 IRef"), Browsable(true)]
        public TexMapID IndTex2MapID { get { return _swapBlock._Value16.TexMap2; } set { _swapBlock._Value16.TexMap2 = value; SignalPropertyChange(); } }
        [Category("TEV RAS1 IRef"), Browsable(true)]
        public TexCoordID IndTex2Coord { get { return _swapBlock._Value16.TexCoord2; } set { _swapBlock._Value16.TexCoord2 = value; SignalPropertyChange(); } }
        [Category("TEV RAS1 IRef"), Browsable(true)]
        public TexMapID IndTex3MapID { get { return _swapBlock._Value16.TexMap3; } set { _swapBlock._Value16.TexMap3 = value; SignalPropertyChange(); } }
        [Category("TEV RAS1 IRef"), Browsable(true)]
        public TexCoordID IndTex3Coord { get { return _swapBlock._Value16.TexCoord3; } set { _swapBlock._Value16.TexCoord3 = value; SignalPropertyChange(); } }

        public MDL0MaterialNode[] Materials { get { return _materials.ToArray(); } }
        public List<MDL0MaterialNode> _materials = new List<MDL0MaterialNode>();

        public sbyte _ref0 = -1, _ref1 = -1, _ref2 = -1, _ref3 = -1, _ref4 = -1, _ref5 = -1, _ref6 = -1, _ref7 = -1;
        public byte _stages, _res0, _res1, _res2;
        int _datalen, _mdl0offset, _pad0, _pad1;

        [Category("Shader Data"), Browsable(false)]
        public int DataLength { get { return _datalen; } }
        [Category("Shader Data"), Browsable(false)]
        public int MDL0Offset { get { return _mdl0offset; } }

        [Category("Shader Data"), Browsable(false)]
        public byte Stages { get { return _stages; } } //Max 16 (2 stages per group - 8 groups)
        [Browsable(false)]
        public byte STGs 
        {
            get { return _stages; } 
            set 
            {
                _stages = value; 
                SignalPropertyChange();

                foreach (MDL0MaterialNode m in Materials)
                {
                    m._updating = true;
                    m.ActiveShaderStages = value;
                    m._updating = false;
                }
            }
        }
        
        //[Category("Shader Data"), Browsable(true)]
        //public byte Res0 { get { return res0; } set { res0 = value; SignalPropertyChange(); } }
        //[Category("Shader Data"), Browsable(true)]
        //public byte Res1 { get { return res1; } set { res1 = value; SignalPropertyChange(); } }
        //[Category("Shader Data"), Browsable(true)]
        //public byte Res2 { get { return res2; } set { res2 = value; SignalPropertyChange(); } }

        [Category("Shader Data"), Browsable(true)]
        public bool TextureRef0 { get { return _ref0 != -1; } set { _ref0 = (sbyte)(value ? 0 : -1); SignalPropertyChange(); } }
        [Category("Shader Data"), Browsable(true)]
        public bool TextureRef1 { get { return _ref1 != -1; } set { _ref1 = (sbyte)(value ? 1 : -1); SignalPropertyChange(); } }
        [Category("Shader Data"), Browsable(true)]
        public bool TextureRef2 { get { return _ref2 != -1; } set { _ref2 = (sbyte)(value ? 2 : -1); SignalPropertyChange(); } }
        [Category("Shader Data"), Browsable(true)]
        public bool TextureRef3 { get { return _ref3 != -1; } set { _ref3 = (sbyte)(value ? 3 : -1); SignalPropertyChange(); } }
        [Category("Shader Data"), Browsable(true)]
        public bool TextureRef4 { get { return _ref4 != -1; } set { _ref4 = (sbyte)(value ? 4 : -1); SignalPropertyChange(); } }
        [Category("Shader Data"), Browsable(true)]
        public bool TextureRef5 { get { return _ref5 != -1; } set { _ref5 = (sbyte)(value ? 5 : -1); SignalPropertyChange(); } }
        [Category("Shader Data"), Browsable(true)]
        public bool TextureRef6 { get { return _ref6 != -1; } set { _ref6 = (sbyte)(value ? 6 : -1); SignalPropertyChange(); } }
        [Category("Shader Data"), Browsable(true)]
        public bool TextureRef7 { get { return _ref7 != -1; } set { _ref7 = (sbyte)(value ? 7 : -1); SignalPropertyChange(); } }

        //[Category("Shader Data"), Browsable(true)]
        //public int Pad0 { get { return pad0; } }
        //[Category("Shader Data"), Browsable(true)]
        //public int Pad1 { get { return pad1; } }

        public bool _renderUpdate = false;
        public new void SignalPropertyChange()
        {
            _renderUpdate = true;
            base.SignalPropertyChange();
        }

        public bool _enabled = true;

        public bool _autoMetal = false;
        public int texCount = -1;
        public bool rendered = false;

        public override string Name
        {
            get
            {
                return String.Format("Shader{0}", Index);
            }
            set
            {
                base.Name = value;
            }
        }

        public void Default()
        {
            _datalen = 512;
            _ref0 =
            _ref1 =
            _ref2 =
            _ref3 =
            _ref4 =
            _ref5 =
            _ref6 =
            _ref7 = -1;

            _stages = 1;

            TEVStage stage = new TEVStage();
            AddChild(stage, true);
            stage.Default();
        }

        public void DefaultAsMetal(int texcount)
        {
            _datalen = 512;
            _autoMetal = true;

            _ref0 =
            _ref1 =
            _ref2 =
            _ref3 =
            _ref4 =
            _ref5 =
            _ref6 =
            _ref7 = -1;

            switch ((texCount = texcount) - 1)
            {
                case 0: _ref0 = 0; break;
                case 1: _ref1 = 1; break;
                case 2: _ref2 = 2; break;
                case 3: _ref3 = 3; break;
                case 4: _ref4 = 4; break;
                case 5: _ref5 = 5; break;
                case 6: _ref6 = 6; break;
                case 7: _ref7 = 7; break;
            }

            _stages = 4;

            Children.Clear();

            int i = 0;
            TEVStage s;
            while (i++ < 4)
            {
                AddChild(s = new TEVStage());
                s.DefaultAsMetal(texcount - 1);
            }
        }

        internal override void GetStrings(StringTable table)
        {
            //We DO NOT want to add the name to the string table!
        }

        public override bool OnInitialize()
        {
            MDL0Shader* header = Header;

            _datalen = header->_dataLength;
            _mdl0offset = header->_mdl0Offset;

            _stages = header->_stages;

            _res0 = header->_res0;
            _res1 = header->_res1;
            _res2 = header->_res2;

            _ref0 = header->_ref0;
            _ref1 = header->_ref1;
            _ref2 = header->_ref2;
            _ref3 = header->_ref3;
            _ref4 = header->_ref4;
            _ref5 = header->_ref5;
            _ref6 = header->_ref6;
            _ref7 = header->_ref7;

            _pad0 = header->_pad0;
            _pad1 = header->_pad1;
            
            //Attach to materials
            byte* pHeader = (byte*)Header;
            if ((Model != null) && (Model._matList != null))
                foreach (MDL0MaterialNode mat in Model._matList)
                {
                    MDL0Material* mHeader = mat.Header;
                    if (((byte*)mHeader + mHeader->_shaderOffset) == pHeader)
                    {
                        mat._shader = this;
                        _materials.Add(mat);
                    }
                }

            _swapBlock = *header->SwapBlock;

            Populate();
            return true;
        }

        public override void OnPopulate()
        {
            StageGroup* grp = Header->First;
            for (int r = 0; r < 8; r++, grp = grp->Next)
                if (grp->mask.Reg == 0x61)
                {
                    TEVStage s0 = new TEVStage();

                    KSel ksel = new KSel(grp->ksel.Data.Value);
                    RAS1_TRef tref = new RAS1_TRef(grp->tref.Data.Value);

                    s0._colorEnv = grp->eClrEnv.Data;
                    s0._alphaEnv = grp->eAlpEnv.Data;
                    s0._cmd = grp->eCMD.Data;

                    s0._kcSel = ksel.KCSel0;
                    s0._kaSel = ksel.KASel0;

                    s0._texMapID = tref.TexMapID0;
                    s0._texCoord = tref.TexCoord0;
                    s0._colorChan = tref.ColorChannel0;
                    s0._texEnabled = tref.TexEnabled0;

                    AddChild(s0, false);

                    if (grp->oClrEnv.Reg == 0x61 && grp->oAlpEnv.Reg == 0x61 && grp->oCMD.Reg == 0x61)
                    {
                        TEVStage s1 = new TEVStage();

                        s1._colorEnv = grp->oClrEnv.Data;
                        s1._alphaEnv = grp->oAlpEnv.Data;
                        s1._cmd = grp->oCMD.Data;

                        s1._kcSel = ksel.KCSel1;
                        s1._kaSel = ksel.KASel1;

                        s1._texMapID = tref.TexMapID1;
                        s1._texCoord = tref.TexCoord1;
                        s1._colorChan = tref.ColorChannel1;
                        s1._texEnabled = tref.TexEnabled1;

                        AddChild(s1, false);
                    }
                }
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            MDL0Shader* header = (MDL0Shader*)address;

            if (Model._isImport)
            {
                _ref1 =
                _ref2 =
                _ref3 =
                _ref4 =
                _ref5 =
                _ref6 =
                _ref7 = -1;

                if (Collada._importOptions._mdlType == 0)
                    _stages = 3;
                else
                    _stages = 1;
            }

            header->_dataLength = length;
            header->_index = Index;

            header->_stages = Model._isImport ? _stages : (byte)Children.Count;

            header->_res0 = 0;
            header->_res1 = 0;
            header->_res2 = 0;

            header->_ref0 = _ref0;
            header->_ref1 = _ref1;
            header->_ref2 = _ref2;
            header->_ref3 = _ref3;
            header->_ref4 = _ref4;
            header->_ref5 = _ref5;
            header->_ref6 = _ref6;
            header->_ref7 = _ref7;

            header->_pad0 = 0;
            header->_pad1 = 0;

            *header->SwapBlock = _swapBlock;

            StageGroup* grp = (StageGroup*)(address + 0x80);
            for (int i = 0; i < Children.Count; i++)
            {
                TEVStage c = (TEVStage)Children[i]; //Current Stage

                if (i % 2 == 0) //Even Stage
                {
                    *grp = StageGroup.Default;

                    grp->SetGroup(i / 2);
                    grp->SetStage(i);

                    grp->eClrEnv.Data = c._colorEnv;
                    grp->eAlpEnv.Data = c._alphaEnv;
                    grp->eCMD.Data = c._cmd;

                    if (i == Children.Count - 1) //Last stage is even, odd stage isn't used
                    {
                        grp->ksel.Data = new KSel(0, 0, c._kcSel, c._kaSel, 0, 0);
                        grp->tref.Data = new RAS1_TRef(c._texMapID, c._texCoord, c._texEnabled, c._colorChan, TexMapID.TexMap7, TexCoordID.TexCoord7, false, ColorSelChan.Zero);
                    }
                }
                else //Odd Stage
                {
                    TEVStage p = (TEVStage)Children[i - 1]; //Previous Stage

                    grp->SetStage(i);

                    grp->oClrEnv.Data = c._colorEnv;
                    grp->oAlpEnv.Data = c._alphaEnv;
                    grp->oCMD.Data = c._cmd;

                    grp->ksel.Data = new KSel(0, 0, p._kcSel, p._kaSel, c._kcSel, c._kaSel);
                    grp->tref.Data = new RAS1_TRef(p._texMapID, p._texCoord, p._texEnabled, p._colorChan, c._texMapID, c._texCoord, c._texEnabled, c._colorChan);

                    grp = grp->Next;
                }
            }

            if (Model._isImport)
            {
                StageGroup* struct0 = header->First;
                *struct0 = StageGroup.Default;
                struct0->SetGroup(0);

                switch (Collada._importOptions._mdlType)
                {
                    case Modeling.Collada.ImportOptions.MDLType.Character:

                        struct0->SetStage(0);
                        struct0->SetStage(1);

                        struct0->mask.Data.Value = 0xFFFFF0;
                        struct0->ksel.Data.Value = 0xE378C0;
                        struct0->tref.Data.Value = 0x03F040;
                        struct0->eClrEnv.Data.Value = 0x28F8AF;
                        struct0->oClrEnv.Data.Value = 0x08FEB0;
                        struct0->eAlpEnv.Data.Value = 0x08F2F0;
                        struct0->oAlpEnv.Data.Value = 0x081FF0;

                        StageGroup* struct1 = struct0->Next;
                        *struct1 = StageGroup.Default;

                        struct1->SetGroup(1);
                        struct1->SetStage(2);

                        struct1->mask.Data.Value = 0xFFFFF0;
                        struct1->ksel.Data.Value = 0x0038C0;
                        struct1->tref.Data.Value = 0x3BF3BF;
                        struct1->eClrEnv.Data.Value = 0x0806EF;
                        struct1->eAlpEnv.Data.Value = 0x081FF0;

                        break;

                    case Modeling.Collada.ImportOptions.MDLType.Stage:

                        struct0->SetStage(0);

                        struct0->mask.Data.Value = 0xFFFFF0;
                        struct0->ksel.Data.Value = 0x0038C0;
                        struct0->tref.Data.Value = 0x3BF040;
                        struct0->eClrEnv.Data.Value = 0x28F8AF;
                        struct0->eAlpEnv.Data.Value = 0x08F2F0;

                        break;
                }
            }
        }

        public override int OnCalculateSize(bool force)
        {
            return 512;
        }

        internal override void Bind(TKContext ctx)
        {
            BuildSwapModeTable();
        }
    }
}
