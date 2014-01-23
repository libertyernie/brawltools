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
using System.Drawing.Imaging;
using BrawlLib.Modeling;
using BrawlLib.Wii.Graphics;
using System.Windows.Forms;
using BrawlLib.Wii.Models;
using BrawlLib.IO;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class MDL0MaterialRefNode : MDL0EntryNode
    {
        internal MDL0TextureRef* Header { get { return (MDL0TextureRef*)_origSource.Address; } set { _origSource.Address = value; } }
        public override bool AllowDuplicateNames { get { return true; } }

        [Browsable(false)]
        public MDL0MaterialNode Material { get { return Parent as MDL0MaterialNode; } }

        public MDL0MaterialRefNode()
        {
            _uWrap = (int)WrapMode.Repeat;
            _vWrap = (int)WrapMode.Repeat;
            _texFlags = TextureSRT.Default;
            _texMatrix = TexMtxEffect.Default;
            _minFltr = 1;
            _magFltr = 1;
        }

        public TextureSRT _texFlags;
        public TexMtxEffect _texMatrix;

        [Browsable(false)]
        public int TextureCoordId 
        {
            get 
            {
                if ((int)Coordinates >= (int)TexSourceRow.TexCoord0)
                    return (int)Coordinates - (int)TexSourceRow.TexCoord0;
                else
                    return -1 - (int)Coordinates;
            } 
        }

        [Category("Texture Coordinates"), TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 Scale { get { return _texFlags.TexScale; } set { if (!CheckIfMetal()) { _texFlags.TexScale = value; _bindState._scale = new Vector3(value._x, value._y, 1); } } }
        [Category("Texture Coordinates")]
        public float Rotation { get { return _texFlags.TexRotation; } set { if (!CheckIfMetal()) { _texFlags.TexRotation = value; _bindState._rotate = new Vector3(value, 0, 0); } } }
        [Category("Texture Coordinates"), TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 Translation { get { return _texFlags.TexTranslation; } set { if (!CheckIfMetal()) { _texFlags.TexTranslation = value; _bindState._translate = new Vector3(value._x, value._y, 0); } } }

        public TexFlags _flags;

        public enum MappingMethod
        {
            TexCoord = 0x00,
            EnvCamera = 0x01,
            Projection = 0x02,
            EnvLight = 0x03,
            EnvSpec = 0x04
        }

        [Category("Texture Matrix Effect")]
        public sbyte SCN0RefCamera { get { return _texMatrix.SCNCamera; } set { if (!CheckIfMetal()) _texMatrix.SCNCamera = value; } }
        [Category("Texture Matrix Effect")]
        public sbyte SCN0RefLight { get { return _texMatrix.SCNLight; } set { if (!CheckIfMetal()) _texMatrix.SCNLight = value; } }
        [Category("Texture Matrix Effect")]
        public MappingMethod MapMode { get { return (MappingMethod)_texMatrix.MapMode; } set { if (!CheckIfMetal()) _texMatrix.MapMode = (byte)value; } }
        [Category("Texture Matrix Effect")]
        public bool IdentityMatrix { get { return _texMatrix.Identity != 0; } set { if (!CheckIfMetal()) _texMatrix.Identity = (byte)(value ? 1 : 0); } }
        [Category("Texture Matrix Effect"), TypeConverter(typeof(Matrix43StringConverter))]
        public Matrix43 EffectMatrix { get { return _texMatrix.TexMtx; } set { if (!CheckIfMetal()) _texMatrix.TexMtx = value; } }
        
        public XFDualTex DualTexFlags;
        public XFTexMtxInfo TexMtxFlags;

        internal int _projection; //Normal enable is true when projection is XF_TEX_STQ
        internal int _inputForm;
        internal int _texGenType;
        internal int _sourceRow;
        internal int _embossSource;
        internal int _embossLight;

        public bool HasTextureMatrix
        {
            get
            {
                bool allsinglebinds = true;
                if (((MDL0MaterialNode)Parent).Objects != null)
                {
                    foreach (MDL0ObjectNode n in ((MDL0MaterialNode)Parent).Objects)
                        if (n.Weighted)
                        {
                            allsinglebinds = false;
                            if (!n.HasTextureMatrix[Index])
                                return false;
                        }
                }
                else return false;

                if (allsinglebinds)
                    return false;

                return true;
            }
            set
            {
                foreach (MDL0ObjectNode n in ((MDL0MaterialNode)Parent).Objects)
                    if (n.Weighted)
                    {
                        n.HasTextureMatrix[Index] = value;
                        n._rebuild = true;
                        Model.SignalPropertyChange();

                        if (n._vertexNode.Format != WiiVertexComponentType.Float)
                            n._vertexNode._forceRebuild = n._vertexNode._forceFloat = value;
                    }
            }
        }
        
        [Category("XF TexGen Flags")]
        public TexProjection Projection { get { return (TexProjection)_projection; } set { if (!CheckIfMetal()) { _projection = (int)value; getTexMtxVal(); } } }
        [Category("XF TexGen Flags")]
        public TexInputForm InputForm { get { return (TexInputForm)_inputForm; } set { if (!CheckIfMetal()) { _inputForm = (int)value; getTexMtxVal(); } } }
        [Category("XF TexGen Flags")]
        public TexTexgenType Type { get { return (TexTexgenType)_texGenType; } set { if (!CheckIfMetal()) { _texGenType = (int)value; getTexMtxVal(); } } }
        [Category("XF TexGen Flags")]
        public TexSourceRow Coordinates { get { return (TexSourceRow)_sourceRow; } set { if (!CheckIfMetal()) { _sourceRow = (int)value; getTexMtxVal(); } } }
        [Category("XF TexGen Flags")]
        public int EmbossSource { get { return _embossSource; } set { if (!CheckIfMetal()) { _embossSource = value; getTexMtxVal(); } } }
        [Category("XF TexGen Flags")]
        public int EmbossLight { get { return _embossLight; } set { if (!CheckIfMetal()) { _embossLight = value; getTexMtxVal(); } } }
        [Category("XF TexGen Flags")]
        public bool Normalize { get { return DualTexFlags.NormalEnable != 0; } set { if (!CheckIfMetal()) { DualTexFlags.NormalEnable = (byte)(value ? 1 : 0); } } }
        
        public void getTexMtxVal()
        {
            TexMtxFlags._data = (uint)(0 |
            (_projection << 1) |
            (_inputForm << 2) |
            (_texGenType << 4) |
            (_sourceRow << 7) |
            (_embossSource << 10) |
            (_embossLight << 13));

            SignalPropertyChange();
        }

        public void getValues()
        {
            _projection = (int)TexMtxFlags.Projection;
            _inputForm = (int)TexMtxFlags.InputForm;
            _texGenType = (int)TexMtxFlags.TexGenType;
            _sourceRow = (int)TexMtxFlags.SourceRow;
            _embossSource = (int)TexMtxFlags.EmbossSource;
            _embossLight = (int)TexMtxFlags.EmbossLight;
        }

        internal int _texPtr;
        internal int _pltPtr;
        internal int _index1;
        internal int _index2;
        internal int _uWrap; 
        internal int _vWrap;
        internal int _minFltr;
        internal int _magFltr;
        internal float _lodBias;
        internal int _maxAniso;
        internal bool _clampBias;
        internal bool _texelInterp;
        internal int _pad;
        
        public enum WrapMode
        {
            Clamp,
            Repeat,
            Mirror
        }

        #region Texture linkage
        internal MDL0TextureNode _texture;
        [Browsable(false)]
        public MDL0TextureNode TextureNode
        {
            get { return _texture; }
            set
            {
                if (_texture == value)
                    return;
                if (_texture != null)
                {
                    _texture._references.Remove(this);
                    if (_texture._references.Count == 0)
                        _texture.Remove();
                }
                if ((_texture = value) != null)
                {
                    _texture._references.Add(this);

                    Name = _texture.Name;

                    if (_texture.Source == null)
                        _texture.GetSource();

                    if (_texture.Source is TEX0Node && ((TEX0Node)_texture.Source).HasPalette)
                        PaletteNode = Model.FindOrCreatePalette(_texture.Name);
                    else
                        PaletteNode = null;
                }
            }
        }
        [Browsable(true), TypeConverter(typeof(DropDownListTextures))]
        public string Texture
        {
            get { return _texture == null ? null : _texture.Name; }
            set { TextureNode = String.IsNullOrEmpty(value) ? null : Model.FindOrCreateTexture(value); SignalPropertyChange(); }
        }
        #endregion

        #region Palette linkage
        internal MDL0TextureNode _palette;
        [Browsable(false)]
        public MDL0TextureNode PaletteNode
        {
            get { return _palette; }
            set
            {
                if (_palette == value)
                    return;
                if (_palette != null)
                {
                    _palette._references.Remove(this);
                    if (_palette._references.Count == 0)
                        _palette.Remove();
                }
                if ((_palette = value) != null)
                    _palette._references.Add(this);
            }
        }
        [Browsable(true), TypeConverter(typeof(DropDownListTextures))]
        public string Palette
        {
            get { return _palette == null ? null : _palette.Name; }
            set { PaletteNode = String.IsNullOrEmpty(value) ? null : Model.FindOrCreatePalette(value); SignalPropertyChange(); }
        }
        #endregion

        public override string Name
        {
            get { return _texture != null ? _texture.Name : base.Name; }
            set { if (_texture != null) Texture = value; base.Name = value; }
        }

        public enum TextureMinFilter : uint
        {
            Nearest = 0,
            Linear,
            Nearest_Mipmap_Nearest,
            Linear_Mipmap_Nearest,
            Nearest_Mipmap_Linear,
            Linear_Mipmap_Linear
        }

        public enum TextureMagFilter : uint
        {
            Nearest = 0,
            Linear,
        }

        //[Category("Texture Reference")]
        //public int TexMapID { get { return _index1; } set { if (!CheckIfMetal()) _index1 = value; } }
        //[Category("Texture Reference")]
        //public int PaletteID { get { return _index2; } set { if (!CheckIfMetal()) _index2 = value; } }
        [Category("Texture Reference")]
        public WrapMode UWrapMode { get { return (WrapMode)_uWrap; } set { if (!CheckIfMetal()) _uWrap = (int)value; } }
        [Category("Texture Reference")]
        public WrapMode VWrapMode { get { return (WrapMode)_vWrap; } set { if (!CheckIfMetal()) _vWrap = (int)value; } }
        [Category("Texture Reference")]
        public TextureMinFilter MinFilter { get { return (TextureMinFilter)_minFltr; } set { if (!CheckIfMetal()) _minFltr = (int)value; } }
        [Category("Texture Reference")]
        public TextureMagFilter MagFilter { get { return (TextureMagFilter)_magFltr; } set { if (!CheckIfMetal()) _magFltr = (int)value; } }
        [Category("Texture Reference")]
        public float LODBias { get { return _lodBias; } set { if (!CheckIfMetal()) _lodBias = value; } }
        [Category("Texture Reference")]
        public Anisotropy MaxAnisotropy { get { return (Anisotropy)_maxAniso; } set { if (!CheckIfMetal()) _maxAniso = (int)value; } }
        [Category("Texture Reference")]
        public bool ClampBias { get { return _clampBias; } set { if (!CheckIfMetal()) _clampBias = value; } }
        [Category("Texture Reference")]
        public bool TexelInterpolate { get { return _texelInterp; } set { if (!CheckIfMetal()) _texelInterp = value; } }
        
        public enum Anisotropy
        {
            One,//GX_ANISO_1, //No anisotropic filter.
            Two,//GX_ANISO_2, //Filters a maximum of two samples.
            Four//GX_ANISO_4  //Filters a maximum of four samples.
        }

        public bool CheckIfMetal()
        {
            if (Material != null && Material.CheckIfMetal())
                return true;

            SignalPropertyChange();
            return false;
        }

        public override bool OnInitialize()
        {
            MDL0TextureRef* header = Header;

            _texPtr = header->_texPtr;
            _pltPtr = header->_pltPtr;
            _index1 = header->_index1;
            _index2 = header->_index2;
            _uWrap = header->_uWrap;
            _vWrap = header->_vWrap;
            _minFltr = header->_minFltr;
            _magFltr = header->_magFltr;
            _lodBias = header->_lodBias;
            _maxAniso = header->_maxAniso;
            _clampBias = header->_clampBias == 1;
            _texelInterp = header->_texelInterp == 1;
            _pad = header->_pad;

            if (header->_texOffset != 0)
            {
                if (_replaced && header->_texOffset >= Parent.WorkingUncompressed.Length)
                    Name = null;
                else
                {
                    if (_replaced)
                        Name = header->TextureName;
                    else
                        _name = header->TextureName;
                    _texture = Model.FindOrCreateTexture(_name);
                    _texture._references.Add(this);
                }
            }
            if (header->_pltOffset != 0)
            {
                if (_replaced && header->_pltOffset >= Parent.WorkingUncompressed.Length)
                    _palette = null;
                else
                {
                    string name = header->PaletteName;
                    _palette = Model.FindOrCreatePalette(name);
                    _palette._references.Add(this);
                }
            }

            int len = ((MDL0MaterialNode)Parent).XFCommands.Length;
            if (len != 0 && Index * 2 < len)
            {
                TexMtxFlags = new XFTexMtxInfo(((MDL0MaterialNode)Parent).XFCommands[Index * 2].values[0]);
                DualTexFlags = new XFDualTex(((MDL0MaterialNode)Parent).XFCommands[Index * 2 + 1].values[0]);
                getValues();
            }

            //if (PaletteNode == null && TextureNode != null)
            //{
            //    if (TextureNode.Source == null)
            //        TextureNode.GetSource();

            //    if (TextureNode.Source is TEX0Node && ((TEX0Node)TextureNode.Source).HasPalette)
            //    {
            //        Model._errors.Add("A palette was not set to texture reference " + Index + " in material " + Parent.Index + " (" + Parent.Name + ").");
            //        PaletteNode = Model.FindOrCreatePalette(TextureNode.Name);

            //        SignalPropertyChange();
            //    }
            //}

            MDL0TexSRTData* TexSettings = ((MDL0MaterialNode)Parent).Header->TexMatrices(((MDL0MaterialNode)Parent)._initVersion);
            
            _texFlags = TexSettings->GetTexFlags(Index);
            _texMatrix = TexSettings->GetTexMatrices(Index);

            _flags = (TexFlags)((((MDL0MaterialNode)Parent)._layerFlags >> (4 * Index)) & 0xF);

            _bindState = new FrameState(
                new Vector3(_texFlags.TexScale._x, _texFlags.TexScale._y, 1),
                new Vector3(_texFlags.TexRotation, 0, 0),
                new Vector3(_texFlags.TexTranslation._x, _texFlags.TexTranslation._y, 0));

            return false;
        }

        internal override void GetStrings(StringTable table)
        {
            table.Add(Name);
            if (_palette != null)
                table.Add(_palette.Name);
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            MDL0TextureRef* header = (MDL0TextureRef*)address;
            header->_texPtr = 0;
            header->_pltPtr = 0;
            header->_index1 = Index;
            header->_index2 = Index;
            header->_uWrap = _uWrap;
            header->_vWrap = _vWrap;
            header->_minFltr = _minFltr;
            header->_magFltr = _magFltr;
            header->_lodBias = _lodBias;
            header->_maxAniso = _maxAniso;
            header->_clampBias = (byte)(_clampBias ? 1 : 0);
            header->_texelInterp = (byte)(_texelInterp ? 1 : 0);
            header->_pad = 0;
        }

        protected internal override void PostProcess(VoidPtr mdlAddress, VoidPtr dataAddress, StringTable stringTable)
        {
            MDL0TextureRef* header = (MDL0TextureRef*)dataAddress;
            header->_texOffset = (int)stringTable[Name] + 4 - (int)dataAddress;

            if (_palette != null)
                header->_pltOffset = (int)stringTable[_palette.Name] + 4 - (int)dataAddress;
            else
                header->_pltOffset = 0;

            header->_texPtr = 0;
            header->_pltPtr = 0;
            header->_index1 = Index;
            header->_index2 = Index;
            header->_uWrap = _uWrap;
            header->_vWrap = _vWrap;
            header->_minFltr = _minFltr;
            header->_magFltr = _magFltr;
            header->_lodBias = _lodBias;
            header->_maxAniso = _maxAniso;
            header->_clampBias = (byte)(_clampBias ? 1 : 0);
            header->_texelInterp = (byte)(_texelInterp ? 1 : 0);
            header->_pad = (short)0;
        }

        internal void Bind(TKContext ctx, int prog)
        {
            if (!String.IsNullOrEmpty(PAT0Texture))
            {
                if (!PAT0Textures.ContainsKey(PAT0Texture))
                    PAT0Textures[PAT0Texture] = new MDL0TextureNode(PAT0Texture) { Source = null, palette = !String.IsNullOrEmpty(PAT0Palette) ? RootNode.FindChildByType(PAT0Palette, true, ResourceNodes.ResourceType.PLT0) as PLT0Node : null };
                MDL0TextureNode t = PAT0Textures[PAT0Texture];
                t.Bind(ctx);
                t.Prepare(this, prog);
            }
            else if (_texture != null)
                _texture.Prepare(this, prog);
        }

        internal override void Unbind()
        {
            if (_texture != null)
                _texture.Unbind();
            
            foreach (MDL0TextureNode t in PAT0Textures.Values)
                t.Unbind();
        }

        public FrameState _frameState, _bindState;
        internal void ApplySRT0Texture(SRT0TextureNode node, int index, bool linear)
        {
            if ((node == null) || (index == 0)) //Reset to identity
                _frameState = new FrameState() { _scale = new Vector3(1) };
            else
                _frameState = new FrameState(node.GetAnimFrame(index - 1, linear));
        }

        public Dictionary<string, MDL0TextureNode> PAT0Textures = new Dictionary<string, MDL0TextureNode>(); 
        public string PAT0Texture, PAT0Palette;
        internal void ApplyPAT0Texture(PAT0TextureNode node, int index)
        {
            PAT0TextureEntryNode prev = null;
            if (node != null && index != 0 && node.Children.Count > 0)
            {
                foreach (PAT0TextureEntryNode next in node.Children)
                {
                    if (next.Index == 0)
                    {
                        prev = next;
                        continue;
                    }
                    if (prev._frame <= index - 1 && next._frame > index - 1)
                        break;
                    prev = next;
                }

                PAT0Texture = prev.Texture;
                PAT0Palette = prev.Palette;
                if (!PAT0Textures.ContainsKey(PAT0Texture))
                {
                    TEX0Node texture = RootNode.FindChildByType(PAT0Texture, true, ResourceNodes.ResourceType.TEX0) as TEX0Node;
                    if (texture != null)
                        PAT0Textures[PAT0Texture] = new MDL0TextureNode(texture.Name) { Source = texture, palette = !String.IsNullOrEmpty(PAT0Palette) ? RootNode.FindChildByType(PAT0Palette, true, ResourceNodes.ResourceType.PLT0) as PLT0Node : null };
                }
                return;
            }
            else PAT0Texture = PAT0Palette = null;
        }

        public void Default()
        {
            Name = "NewRef";
            _minFltr = 1;
            _magFltr = 1;
            UWrapMode = WrapMode.Repeat;
            VWrapMode = WrapMode.Repeat;

            _flags = (TexFlags)0xF;
            _texFlags.TexScale = new Vector2(1);
            _bindState._scale = new Vector3(1);
            _texMatrix.TexMtx = Matrix43.Identity;
            _texMatrix.SCNCamera = -1;
            _texMatrix.SCNLight = -1;
            _texMatrix.Identity = 1;

            _projection = (int)TexProjection.ST;
            _inputForm = (int)TexInputForm.AB11;
            _texGenType = (int)TexTexgenType.Regular;
            _sourceRow = (int)TexSourceRow.TexCoord0;
            _embossSource = 4;
            _embossLight = 2;

            getTexMtxVal();

            _texture = Model.FindOrCreateTexture(_name);
            _texture._references.Add(this);
        }

        public override void Remove()
        {
            if (_parent != null && !CheckIfMetal())
            {
                TextureNode = null;
                PaletteNode = null;
                base.Remove();
            }
        }

        public override bool MoveUp()
        {
            if (Parent == null)
                return false;

            if (CheckIfMetal())
                return false;

            int index = Index - 1;
            if (index < 0)
                return false;

            Parent.Children.Remove(this);
            Parent.Children.Insert(index, this);
            Parent._changed = true;

            _index1 = _index2 = index;

            return true;
        }

        public override bool MoveDown()
        {
            if (Parent == null)
                return false;

            if (CheckIfMetal())
                return false;

            int index = Index + 1;
            if (index >= Parent.Children.Count)
                return false;

            Parent.Children.Remove(this);
            Parent.Children.Insert(index, this);
            Parent._changed = true;

            _index1 = _index2 = index;

            return true;
        }

        public override unsafe void Replace(string fileName)
        {
            base.Replace(fileName);

            Model.CheckTextures();
        }

        public override unsafe void Export(string outPath)
        {
            StringTable table = new StringTable();
            GetStrings(table);
            int dataLen = OnCalculateSize(true);
            int totalLen = dataLen + table.GetTotalSize();

            using (FileStream stream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.RandomAccess))
            {
                stream.SetLength(totalLen);
                using (FileMap map = FileMap.FromStream(stream))
                {
                    Rebuild(map.Address, dataLen, false);
                    table.WriteTable(map.Address + dataLen);
                    PostProcess(map.Address, map.Address, table);
                }
            }
        }
    }
}
