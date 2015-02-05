using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Animations;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.OpenGL;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using Ikarus;
using BrawlLib.Modeling;
using System.Windows.Forms;

namespace Ikarus.MovesetFile
{
    public unsafe class MiscSectionNode : MovesetEntryNode
    {
        sDataMisc _hdr;

        [Category("Misc Offsets")]
        public int UnknownSection1Offset { get { return _hdr.UnknownSection1Offset; } }
        [Category("Misc Offsets")]
        public int FinalSmashAuraOffset { get { return _hdr.FinalSmashAuraOffset; } }
        [Category("Misc Offsets")]
        public int FinalSmashAuraCount { get { return _hdr.FinalSmashAuraCount; } }
        [Category("Misc Offsets")]
        public int HurtBoxOffset { get { return _hdr.HurtBoxOffset; } }
        [Category("Misc Offsets")]
        public int HurtBoxCount { get { return _hdr.HurtBoxCount; } }
        [Category("Misc Offsets")]
        public int LedgegrabOffset { get { return _hdr.LedgegrabOffset; } }
        [Category("Misc Offsets")]
        public int LedgegrabCount { get { return _hdr.LedgegrabCount; } }
        [Category("Misc Offsets")]
        public int UnknownSection2Offset { get { return _hdr.UnknownSection2Offset; } }
        [Category("Misc Offsets")]
        public int UnknownSection2Count { get { return _hdr.UnknownSection2Count; } }
        [Category("Misc Offsets")]
        public int BoneRefOffset { get { return _hdr.BoneRef2Offset; } }
        [Category("Misc Offsets")]
        public int UnknownSection3Offset { get { return _hdr.UnknownSection3Offset; } }
        [Category("Misc Offsets")]
        public int SoundDataOffset { get { return _hdr.SoundDataOffset; } }
        [Category("Misc Offsets")]
        public int UnkSection5Offset { get { return _hdr.UnknownSection5Offset; } }
        [Category("Misc Offsets")]
        public int MultiJumpOffset { get { return _hdr.MultiJumpOffset; } }
        [Category("Misc Offsets")]
        public int GlideOffset { get { return _hdr.GlideOffset; } }
        [Category("Misc Offsets")]
        public int CrawlOffset { get { return _hdr.CrawlOffset; } }
        [Category("Misc Offsets")]
        public int CollisionData { get { return _hdr.CollisionDataOffset; } }
        [Category("Misc Offsets")]
        public int TetherOffset { get { return _hdr.TetherOffset; } }
        [Category("Misc Offsets")]
        public int UnknownSection12Offset { get { return _hdr.UnknownSection12Offset; } }

        public CollisionData _collisionData;
        public EntryListOffset<IndexValue> _unknown18;
        public MiscTether _tether;
        public EntryList<IndexValue> _unknown1;
        public EntryList<MiscFinalSmashAura> _finalSmashAura;
        public EntryList<MiscHurtBox> _hurtBoxes;
        public EntryList<MiscLedgeGrab> _ledgeGrabs;
        public EntryList<MiscUnknown7Entry> _unknown7;
        public EntryList<BoneIndexValue> _boneRefs;
        public MiscUnknown10 _unknown10;
        public MiscSoundData _soundData;
        public MiscUnknown12 _unkSection5;
        public MiscMultiJump _multiJump;
        public MiscGlide _glide;
        public MiscCrawl _crawl;

        protected override void OnParse(VoidPtr address)
        {
            //Get header values
            _hdr = *(sDataMisc*)address;

            //Parse all misc entries.
            //If an offset is 0, the entry will be set to null.
            _unknown1 = Parse<EntryList<IndexValue>>(_hdr[0], 4);
            _finalSmashAura = Parse<EntryList<MiscFinalSmashAura>>(_hdr[1], 0x14, _hdr[2]);
            _hurtBoxes = Parse<EntryList<MiscHurtBox>>(_hdr[3], 0x20, _hdr[4]);
            _ledgeGrabs = Parse<EntryList<MiscLedgeGrab>>(_hdr[5], 0x10, _hdr[6]);
            _unknown7 = Parse<EntryList<MiscUnknown7Entry>>(_hdr[7], 0x20, _hdr[8]);
            _boneRefs = Parse<EntryList<BoneIndexValue>>(_hdr[9], 4, 10);
            _unknown10 = Parse<MiscUnknown10>(_hdr[10]);
            _soundData = Parse<MiscSoundData>(_hdr[11]);
            _unkSection5 = Parse<MiscUnknown12>(_hdr[12]);
            _multiJump = Parse<MiscMultiJump>(_hdr[13]);
            _glide = Parse<MiscGlide>(_hdr[14]);
            _crawl = Parse<MiscCrawl>(_hdr[15]);
            _collisionData = Parse<CollisionData>(_hdr[16]);
            _tether = Parse<MiscTether>(_hdr[17]);
            _unknown18 = Parse<EntryListOffset<IndexValue>>(_hdr[18], 4);
        }
    }

    public unsafe class MiscUnknown10 : MovesetEntryNode
    {
        public List<MiscUnknown10Entry> _entries;

        int _haveNBoneIndex1, _haveNBoneIndex2, _haveNBoneIndex3, _throwNBoneIndex, _unkCount, _unkOffset, _pad;

        [Category("Bone References"), Browsable(true), TypeConverter(typeof(DropDownListBonesMDef))]
        public string HaveNBone1 
        {
            get { return HaveNBoneNode1 == null ? _haveNBoneIndex1.ToString() : HaveNBoneNode1.Name; } 
            set 
            {
                if (Model == null) 
                    _haveNBoneIndex1 = Convert.ToInt32(value); 
                else 
                    HaveNBoneNode1 = String.IsNullOrEmpty(value) ? HaveNBoneNode1 : Model.FindBone(value);
                SignalPropertyChange();
            }
        }
        [Category("Bone References"), Browsable(true), TypeConverter(typeof(DropDownListBonesMDef))]
        public string HaveNBone2
        {
            get { return HaveNBoneNode2 == null ? _haveNBoneIndex2.ToString() : HaveNBoneNode2.Name; }
            set
            {
                if (Model == null)
                    _haveNBoneIndex2 = Convert.ToInt32(value);
                else
                    HaveNBoneNode2 = String.IsNullOrEmpty(value) ? HaveNBoneNode2 : Model.FindBone(value);
                SignalPropertyChange();
            }
        }
        [Category("Bone References"), Browsable(true), TypeConverter(typeof(DropDownListBonesMDef))]
        public string ThrowNBone
        {
            get { return ThrowNBoneNode == null ? _throwNBoneIndex.ToString() : ThrowNBoneNode.Name; }
            set
            {
                if (Model == null)
                    _throwNBoneIndex = Convert.ToInt32(value);
                else
                    ThrowNBoneNode = String.IsNullOrEmpty(value) ? ThrowNBoneNode : Model.FindBone(value);
                SignalPropertyChange();
            }
        }
        [Category("Bone References"), Browsable(true), TypeConverter(typeof(DropDownListBonesMDef))]
        public string HaveNBone3
        {
            get { return HaveNBoneNode3 == null ? _haveNBoneIndex3.ToString() : HaveNBoneNode3.Name; }
            set
            {
                if (Model == null)
                    _haveNBoneIndex3 = Convert.ToInt32(value);
                else
                    HaveNBoneNode3 = String.IsNullOrEmpty(value) ? HaveNBoneNode3 : Model.FindBone(value);
                SignalPropertyChange();
            }
        }

        [Browsable(false)]
        public MDL0BoneNode HaveNBoneNode1
        {
            get { if (Model == null) return null; if (_haveNBoneIndex1 >= Model._linker.BoneCache.Length || _haveNBoneIndex1 < 0) return null; return (MDL0BoneNode)Model._linker.BoneCache[_haveNBoneIndex1]; }
            set { _haveNBoneIndex1 = value.BoneIndex; }
        }
        [Browsable(false)]
        public MDL0BoneNode HaveNBoneNode2
        {
            get { if (Model == null) return null; if (_haveNBoneIndex2 >= Model._linker.BoneCache.Length || _haveNBoneIndex2 < 0) return null; return (MDL0BoneNode)Model._linker.BoneCache[_haveNBoneIndex2]; }
            set { _haveNBoneIndex2 = value.BoneIndex; }
        }
        [Browsable(false)]
        public MDL0BoneNode ThrowNBoneNode
        {
            get { if (Model == null) return null; if (_throwNBoneIndex >= Model._linker.BoneCache.Length || _throwNBoneIndex < 0) return null; return (MDL0BoneNode)Model._linker.BoneCache[_throwNBoneIndex]; }
            set { _throwNBoneIndex = value.BoneIndex; }
        }
        [Browsable(false)]
        public MDL0BoneNode HaveNBoneNode3
        {
            get { if (Model == null) return null; if (_haveNBoneIndex3 >= Model._linker.BoneCache.Length || _haveNBoneIndex3 < 0) return null; return (MDL0BoneNode)Model._linker.BoneCache[_haveNBoneIndex3]; }
            set { _haveNBoneIndex3 = value.BoneIndex; }
        }

        protected override void OnParse(VoidPtr address)
        {
            _entries = new List<MiscUnknown10Entry>();
            sMiscUnknown10* hdr = (sMiscUnknown10*)address;

            _haveNBoneIndex1 = hdr->_haveNBoneIndex1;
            _haveNBoneIndex2 = hdr->_haveNBoneIndex2;
            _throwNBoneIndex = hdr->_throwNBoneIndex;
            _unkCount = hdr->_list._startOffset;
            _unkOffset = hdr->_list._listCount;
            _pad = hdr->_pad;
            _haveNBoneIndex3 = hdr->_haveNBoneIndex3;

            //if ((_offset - _unkOffset) / 16 != _unkCount)
            //    throw new Exception("Count is incorrect");

            for (int i = 0; i < _unkCount; i++)
                _entries.Add(Parse<MiscUnknown10Entry>(_unkOffset + i * 0x10));
        }

        protected override int OnGetSize()
        {
            _lookupCount = (_entries.Count > 0 ? 1 : 0);
            return 28 + _entries.Count * 0x10;
        }

        protected override void OnWrite(VoidPtr address)
        {
            sMiscUnknown10Entry* data = (sMiscUnknown10Entry*)address;
            foreach (MiscUnknown10Entry e in _entries)
                e.Write(data++);

            RebuildAddress = data;

            sMiscUnknown10* header = (sMiscUnknown10*)data;
            header->_haveNBoneIndex1 = _haveNBoneIndex1;
            header->_haveNBoneIndex2 = _haveNBoneIndex2;
            header->_throwNBoneIndex = _throwNBoneIndex;
            header->_pad = _pad;
            header->_haveNBoneIndex3 = _haveNBoneIndex3;

            //Values are switched on purpose
            header->_list._startOffset = _entries.Count;
            header->_list._listCount = (_entries.Count > 0 ? Offset(address) : 0);

            if (header->_list._listCount > 0)
                _lookupOffsets.Add(&header->_list._listCount);
        }
    }

    public unsafe class MiscUnknown10Entry : MovesetEntryNode
    {
        int _unk1, _unk2, _pad1, _pad2;
        
        [Category("Unk Section 3 Entry")]
        public int Unk1 { get { return _unk1; } set { _unk1 = value; SignalPropertyChange(); } }
        [Category("Unk Section 3 Entry")]
        public int Unk2 { get { return _unk2; } set { _unk2 = value; SignalPropertyChange(); } }
        [Category("Unk Section 3 Entry")]
        public int Pad1 { get { return _pad1; } }
        [Category("Unk Section 3 Entry")]
        public int Pad2 { get { return _pad2; } }

        protected override void OnParse(VoidPtr address)
        {
            sMiscUnknown10Entry* hdr = (sMiscUnknown10Entry*)address;
            _unk1 = hdr->_unk1;
            _unk2 = hdr->_unk2;
            _pad1 = hdr->_pad1;
            _pad2 = hdr->_pad2;
        }

        protected override int OnGetSize()
        {
            _lookupCount = 0;
            return 16;
        }

        protected override void OnWrite(VoidPtr address)
        {
            RebuildAddress = address;
            sMiscUnknown10Entry* header = (sMiscUnknown10Entry*)address;
            header->_unk1 = _unk1;
            header->_unk2 = _unk2;
            header->_pad1 = _pad1;
            header->_pad2 = _pad2;
        }
    }

    public unsafe class MiscHurtBox : MovesetEntryNode
    {
        internal Vector3 _posOffset, _stretch;
        internal float _radius;
        internal sHurtBoxFlags _flags = new sHurtBoxFlags();

        [Browsable(false)]
        public MDL0BoneNode BoneNode
        {
            get { if (Model == null) return null; if (_flags.BoneIndex >= Model._linker.BoneCache.Length || _flags.BoneIndex < 0) return null; return (MDL0BoneNode)Model._linker.BoneCache[_flags.BoneIndex]; }
            set { _flags.BoneIndex = value.BoneIndex; _name = value.Name; }
        }
        
        [Category("HurtBox"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 PosOffset { get { return _posOffset; } set { _posOffset = value; SignalPropertyChange(); } }
        [Category("HurtBox"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Stretch { get { return _stretch; } set { _stretch = value; SignalPropertyChange(); } }
        [Category("HurtBox")]
        public float Radius { get { return _radius; } set { _radius = value; SignalPropertyChange(); } }
        [Category("HurtBox"), Browsable(true), TypeConverter(typeof(DropDownListBonesMDef))]
        public string Bone { get { return BoneNode == null ? _flags.BoneIndex.ToString() : BoneNode.Name; } set { if (Model == null) { _flags.BoneIndex = Convert.ToInt32(value); _name = _flags.BoneIndex.ToString(); } else { BoneNode = String.IsNullOrEmpty(value) ? BoneNode : Model.FindBone(value); } SignalPropertyChange(); } }
        [Category("HurtBox")]
        public bool Enabled { get { return _flags.Enabled; } set { _flags.Enabled = value; SignalPropertyChange(); } }
        [Category("HurtBox")]
        public HurtBoxZone Zone { get { return _flags.Zone; } set { _flags.Zone = value; SignalPropertyChange(); } }
        [Category("HurtBox")]
        public int Region { get { return _flags.Region; } set { _flags.Region = value; SignalPropertyChange(); } }
        [Category("HurtBox")]
        public int Unknown { get { return _flags.Unk; } set { _flags.Unk = value; SignalPropertyChange(); } }

        public override string Name
        {
            get { return Bone; }
        }

        protected override void OnParse(VoidPtr address)
        {
            sHurtBox* hdr = (sHurtBox*)address;
            _posOffset = hdr->_offset;
            _stretch = hdr->_stretch;
            _radius = hdr->_radius;
            _flags = hdr->_flags;
        }

        protected override int OnGetSize()
        {
            _lookupCount = 0;
            return 0x20;
        }

        protected override void OnWrite(VoidPtr address)
        {
            sHurtBox* header = (sHurtBox*)(RebuildAddress = address);
            header->_offset = _posOffset;
            header->_stretch = _stretch;
            header->_radius = _radius;
            header->_flags = _flags;
        }

        #region Rendering
        public unsafe void Render(bool selected, int type) 
        {
            if (BoneNode == null)
                return;

            //Disable all things that could be enabled
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            switch (type)
            {
                case 0: //normal - yellow
                    switch ((int)Zone)
                    {
                        case 0:
                            GL.Color4(selected ? 0.0f : 0.5f, 0.5f, 0.0f, 0.5f);
                            break;
                        default:
                            GL.Color4(selected ? 0.0f : 1.0f, 1.0f, 0.0f, 0.5f);
                            break;
                        case 2:
                            GL.Color4(selected ? 0.0f : 1.0f, 1.0f, 0.25f, 0.5f);
                            break;
                    }
                    break;
                case 1: //invincible - green
                    switch ((int)Zone)
                    {
                        case 0:
                            GL.Color4(selected ? 0.0f : 0.0f, 0.5f, 0.0f, 0.5f);
                            break;
                        default:
                            GL.Color4(selected ? 0.0f : 0.0f, 1.0f, 0.0f, 0.5f);
                            break;
                        case 2:
                            GL.Color4(selected ? 0.0f : 0.0f, 1.0f, 0.25f, 0.5f);
                            break;
                    }
                    break;
                default: //intangible - blue
                    switch ((int)Zone)
                    {
                        case 0:
                            GL.Color4(0.0f, selected ? 0.5f : 0.0f, selected ? 0.0f : 0.5f, 0.5f);
                            break;
                        default:
                            GL.Color4(0.0f, selected ? 1.0f : 0.0f, selected ? 0.0f : 1.0f, 0.5f);
                            break;
                        case 2:
                            GL.Color4(0.0f, selected ? 1.0f : 0.25f, selected ? 0.25f : 1.0f, 0.5f);
                            break;
                    }
                    break;
            }

            FrameState frame = BoneNode.Matrix.Derive();
            Vector3 bonescl = frame._scale * _radius;

            Matrix m = Matrix.TransformMatrix(bonescl, frame._rotate, frame._translate);
            
            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            Vector3 stretchfac = _stretch / bonescl;
            GL.Translate(_posOffset._x / bonescl._x, _posOffset._y / bonescl._y, _posOffset._z / bonescl._z);

            int res = 16;
            double angle = 360.0 / res;

            // eight corners: XYZ, XYz, XyZ, Xyz, xYZ, xYz, xyZ, xyz
            for (int quadrant = 0; quadrant < 8; quadrant++)
            {
                for (double i = 0; i < 180 / angle; i++)
                {
                    double ringang1 = (i * angle) / 180 * Math.PI;
                    double ringang2 = ((i + 1) * angle) / 180 * Math.PI;

                    for (double j = 0; j < 360 / angle; j++)
                    {
                        double ang1 = (j * angle) / 180 * Math.PI;
                        double ang2 = ((j + 1) * angle) / 180 * Math.PI;

                        int q = 0;
                        Vector3 stretch = new Vector3(0, 0, 0);

                        if (Math.Cos(ang2) >= 0) // X
                        {
                            q += 4;
                            if (_stretch._x > 0)
                                stretch._x = stretchfac._x;
                        }
                        else
                        {
                            if (_stretch._x < 0)
                                stretch._x = stretchfac._x;
                        }
                        if (Math.Sin(ang2) >= 0) // Y
                        {
                            q += 2;
                            if (_stretch._y > 0)
                                stretch._y = stretchfac._y;
                        }
                        else
                        {
                            if (_stretch._y < 0)
                                stretch._y = stretchfac._y;
                        }
                        if (Math.Cos(ringang2) >= 0) // Z
                        {
                            q += 1;
                            if (_stretch._z > 0)
                                stretch._z = stretchfac._z;
                        }
                        else
                        {
                            if (_stretch._z < 0)
                                stretch._z = stretchfac._z;
                        }
                        if (quadrant == q)
                        {
                            GL.Translate(stretch._x,stretch._y,stretch._z);
                            GL.Begin(PrimitiveType.Quads);
                            GL.Vertex3(Math.Cos(ang1) * Math.Sin(ringang2), Math.Sin(ang1) * Math.Sin(ringang2), Math.Cos(ringang2));
                            GL.Vertex3(Math.Cos(ang2) * Math.Sin(ringang2), Math.Sin(ang2) * Math.Sin(ringang2), Math.Cos(ringang2));
                            GL.Vertex3(Math.Cos(ang2) * Math.Sin(ringang1), Math.Sin(ang2) * Math.Sin(ringang1), Math.Cos(ringang1));
                            GL.Vertex3(Math.Cos(ang1) * Math.Sin(ringang1), Math.Sin(ang1) * Math.Sin(ringang1), Math.Cos(ringang1));
                            GL.End();
                            GL.Translate(-stretch._x, -stretch._y, -stretch._z);
                        }
                    }
                }
            }

            // twelve edges
            double x1, x2, y1, y2, z1, z2;

            // x-axis edges
            for (double i = 0; i < 360 / angle; i++)
            {
                double ang1 = (i * angle) / 180 * Math.PI;
                double ang2 = ((i + 1) * angle) / 180 * Math.PI;

                z1 = Math.Cos(ang1);
                z2 = Math.Cos(ang2);
                y1 = Math.Sin(ang1);
                y2 = Math.Sin(ang2);

                x1 = _stretch._x < 0 ? stretchfac._x : 0;
                x2 = _stretch._x > 0 ? stretchfac._x : 0;

                if (y2 >= 0 && _stretch._y > 0)
                {
                    y1 += stretchfac._y;
                    y2 += stretchfac._y;
                }
                if (y2 <= 0 && _stretch._y < 0)
                {
                    y1 += stretchfac._y;
                    y2 += stretchfac._y;
                }
                if (z2 >= 0 && _stretch._z > 0)
                {
                    z1 += stretchfac._z;
                    z2 += stretchfac._z;
                }
                if (z2 <= 0 && _stretch._z < 0)
                {
                    z1 += stretchfac._z;
                    z2 += stretchfac._z;
                }

                GL.Begin(PrimitiveType.Quads);
                GL.Vertex3(x1, y1, z1);
                GL.Vertex3(x2, y1, z1);
                GL.Vertex3(x2, y2, z2);
                GL.Vertex3(x1, y2, z2);
                GL.End();
            }

            // y-axis edges
            for (double i = 0; i < 360 / angle; i++)
            {
                double ang1 = (i * angle) / 180 * Math.PI;
                double ang2 = ((i + 1) * angle) / 180 * Math.PI;

                x1 = Math.Cos(ang1);
                x2 = Math.Cos(ang2);
                z1 = Math.Sin(ang1);
                z2 = Math.Sin(ang2);

                y1 = _stretch._y < 0 ? stretchfac._y : 0;
                y2 = _stretch._y > 0 ? stretchfac._y : 0;

                if (x2 >= 0 && _stretch._x > 0)
                {
                    x1 += stretchfac._x;
                    x2 += stretchfac._x;
                }
                if (x2 <= 0 && _stretch._x < 0)
                {
                    x1 += stretchfac._x;
                    x2 += stretchfac._x;
                }
                if (z2 >= 0 && _stretch._z > 0)
                {
                    z1 += stretchfac._z;
                    z2 += stretchfac._z;
                }
                if (z2 <= 0 && _stretch._z < 0)
                {
                    z1 += stretchfac._z;
                    z2 += stretchfac._z;
                }

                GL.Begin(PrimitiveType.Quads);
                GL.Vertex3(x1, y1, z1);
                GL.Vertex3(x1, y2, z1);
                GL.Vertex3(x2, y2, z2);
                GL.Vertex3(x2, y1, z2);
                GL.End();
            }

            // z-axis edges
            for (double i = 0; i < 360 / angle; i++)
            {
                double ang1 = (i * angle) / 180 * Math.PI;
                double ang2 = ((i + 1) * angle) / 180 * Math.PI;

                x1 = Math.Cos(ang1);
                x2 = Math.Cos(ang2);
                y1 = Math.Sin(ang1);
                y2 = Math.Sin(ang2);

                z1 = _stretch._z < 0 ? stretchfac._z : 0;
                z2 = _stretch._z > 0 ? stretchfac._z : 0;

                if (x2 >= 0 && _stretch._x > 0)
                {
                    x1 += stretchfac._x;
                    x2 += stretchfac._x;
                }
                if (x2 <= 0 && _stretch._x < 0)
                {
                    x1 += stretchfac._x;
                    x2 += stretchfac._x;
                }
                if (y2 >= 0 && _stretch._y > 0)
                {
                    y1 += stretchfac._y;
                    y2 += stretchfac._y;
                }
                if (y2 <= 0 && _stretch._y < 0)
                {
                    y1 += stretchfac._y;
                    y2 += stretchfac._y;
                }

                GL.Begin(PrimitiveType.Quads);
                GL.Vertex3(x2, y2, z1);
                GL.Vertex3(x2, y2, z2);
                GL.Vertex3(x1, y1, z2);
                GL.Vertex3(x1, y1, z1);
                GL.End();
            }

            Vector3 scale = frame._scale;

            // six faces
            GL.Begin(PrimitiveType.Quads);
            float outpos;

            // left face
            outpos = _radius / bonescl._x * scale._x;
            if (_stretch._x > 0)
                outpos = (_stretch._x + _radius) / bonescl._x;
            
            GL.Vertex3(outpos, 0, 0);
            GL.Vertex3(outpos, stretchfac._y, 0);
            GL.Vertex3(outpos, stretchfac._y, stretchfac._z);
            GL.Vertex3(outpos, 0, stretchfac._z);

            // right face
            outpos = -_radius / bonescl._x * scale._x;
            if (_stretch._x < 0)
                outpos = (_stretch._x - _radius) / bonescl._x;
            
            GL.Vertex3(outpos, 0, 0);
            GL.Vertex3(outpos, 0, stretchfac._z);
            GL.Vertex3(outpos, stretchfac._y, stretchfac._z);
            GL.Vertex3(outpos, stretchfac._y, 0);

            // top face
            outpos = _radius / bonescl._y * scale._y;
            if (_stretch._y > 0)
                outpos = (_stretch._y + _radius) / bonescl._y;
            
            GL.Vertex3(0, outpos, 0);
            GL.Vertex3(0, outpos, stretchfac._z);
            GL.Vertex3(stretchfac._x, outpos, stretchfac._z);
            GL.Vertex3(stretchfac._x, outpos, 0);

            // bottom face
            outpos = -_radius / bonescl._y * scale._y;
            if (_stretch._y < 0)
                outpos = (_stretch._y - _radius) / bonescl._y;
            
            GL.Vertex3(0, outpos, 0);
            GL.Vertex3(stretchfac._x, outpos, 0);
            GL.Vertex3(stretchfac._x, outpos, stretchfac._z);
            GL.Vertex3(0, outpos, stretchfac._z);

            // front face
            outpos = _radius / bonescl._z * scale._z;
            if (_stretch._z > 0)
                outpos = (_stretch._z + _radius) / bonescl._z;
            
            GL.Vertex3(0, 0, outpos);
            GL.Vertex3(stretchfac._x, 0, outpos);
            GL.Vertex3(stretchfac._x, stretchfac._y, outpos);
            GL.Vertex3(0, stretchfac._y, outpos);

            // right face
            outpos = -_radius / bonescl._z * scale._z;
            if (_stretch._z < 0)
                outpos = (_stretch._z - _radius) / bonescl._z;
            
            GL.Vertex3(0, 0, outpos);
            GL.Vertex3(0, stretchfac._y, outpos);
            GL.Vertex3(stretchfac._x, stretchfac._y, outpos);
            GL.Vertex3(stretchfac._x, 0, outpos);
            GL.End();

            GL.PopMatrix();
        }
        #endregion
    }

    public unsafe class MiscFinalSmashAura : MovesetEntryNode
    {
        internal int _boneIndex = 0;
        internal float _x, _y, _width, _height;

        [Browsable(false)]
        public MDL0BoneNode BoneNode
        {
            get { if (Model == null) return null; if (_boneIndex >= Model._linker.BoneCache.Length || _boneIndex < 0) return null; return (MDL0BoneNode)Model._linker.BoneCache[_boneIndex]; }
            set { _boneIndex = value.BoneIndex; _name = value.Name; }
        }

        [Category("Final Smash Aura"), Browsable(true), TypeConverter(typeof(DropDownListBonesMDef))]
        public string Bone { get { return BoneNode == null ? _boneIndex.ToString() : BoneNode.Name; } set { if (Model == null) { _boneIndex = Convert.ToInt32(value); _name = _boneIndex.ToString(); } else { BoneNode = String.IsNullOrEmpty(value) ? BoneNode : Model.FindBone(value); } SignalPropertyChange(); } }
        [Category("Final Smash Aura"), TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 XY { get { return new Vector2(_x, _y); } set { _x = value._x; _y = value._y; SignalPropertyChange(); } }
        [Category("Final Smash Aura")]
        public float Height { get { return _width; } set { _height = value; SignalPropertyChange(); } }
        [Category("Final Smash Aura")]
        public float Width { get { return _height; } set { _width = value; SignalPropertyChange(); } }

        public override string Name
        {
            get { return Bone; }
        }

        protected override void OnParse(VoidPtr address)
        {
            sMiscFSAura* hdr = (sMiscFSAura*)address;

            _boneIndex = hdr->_boneIndex;
            _x = hdr->_x;
            _y = hdr->_y;
            _width = hdr->_width;
            _height = hdr->_height;
        }

        protected override int OnGetSize()
        {
            _lookupCount = 0;
            return 0x14;
        }

        protected override void OnWrite(VoidPtr address)
        {
            RebuildAddress = address;
            sMiscFSAura* header = (sMiscFSAura*)address;
            header->_boneIndex = _boneIndex;
            header->_height = _height;
            header->_width = _width;
            header->_x = _x;
            header->_y = _y;
        }
    }

    public unsafe class MiscLedgeGrab : MovesetEntryNode
    {
        internal float _width, height;
        Vector2 _xy;

        [Category("LedgeGrab"), TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 XY { get { return _xy; } set { _xy = value; SignalPropertyChange(); } }
        [Category("LedgeGrab")]
        public float Height { get { return height; } set { height = value; SignalPropertyChange(); } }
        [Category("LedgeGrab")]
        public float Width { get { return _width; } set { _width = value; SignalPropertyChange(); } }

        protected override void OnParse(VoidPtr address)
        {
            sLedgegrab* hdr = (sLedgegrab*)address;
            _xy = hdr->_xy;
            _width = hdr->_width;
            height = hdr->_height;
        }

        protected override int OnGetSize()
        {
            _lookupCount = 0;
            return 0x10;
        }

        protected override void OnWrite(VoidPtr address)
        {
            RebuildAddress = address;
            sLedgegrab* header = (sLedgegrab*)address;
            header->_height = height;
            header->_width = _width;
            header->_xy = _xy;
        }
    }

    public unsafe class MiscMultiJump : MovesetEntryNode
    {
        internal float _unk1, _unk2, _unk3, _horizontalBoost, _turnFrames;
        internal List<float> _hops, _unks;

        [Category("MultiJump Attribute")]
        public float Unk1 { get { return _unk1; } set { _unk1 = value; SignalPropertyChange(); } }
        [Category("MultiJump Attribute")]
        public float Unk2 { get { return _unk2; } set { _unk2 = value; SignalPropertyChange(); } }
        [Category("MultiJump Attribute")]
        public float Unk3 { get { return _unk3; } set { _unk3 = value; SignalPropertyChange(); } }
        [Category("MultiJump Attribute")]
        public float HorizontalBoost { get { return _horizontalBoost; } set { _horizontalBoost = value; SignalPropertyChange(); } }
        [Category("MultiJump Attribute")]
        public float TurnFrames { get { return _turnFrames; } set { _turnFrames = value; SignalPropertyChange(); } }
        [Category("MultiJump Attribute")]
        public float[] Hops { get { return _hops.ToArray(); } set { _hops = value.ToList<float>(); SignalPropertyChange(); } }
        [Category("MultiJump Attribute")]
        public float[] Unks { get { return _unks.ToArray(); } set { _unks = value.ToList<float>(); SignalPropertyChange(); } }

        protected override void OnParse(VoidPtr address)
        {
            _unks = new List<float>();
            _hops = new List<float>();

            sMultiJump* hdr = (sMultiJump*)address;
            _unk1 = hdr->_unk1;
            _unk2 = hdr->_unk2;
            _unk3 = hdr->_unk3;
            _horizontalBoost = hdr->_horizontalBoost;

            if (hdr->hopFixed)
                _hops.Add(*(bfloat*)hdr->_hopListOffset.Address);
            else
            {
                bfloat* addr = (bfloat*)(BaseAddress + hdr->_hopListOffset);
                for (int i = 0; i < (_offset - hdr->_hopListOffset) / 4; i++)
                    _hops.Add(*addr++);
            }
            if (hdr->unkFixed)
                _unks.Add(*(bfloat*)hdr->_unkListOffset.Address);
            else
            {
                bfloat* addr = (bfloat*)(BaseAddress + hdr->_unkListOffset);
                for (int i = 0; i < ((hdr->hopFixed ? _offset : (int)hdr->_hopListOffset) - hdr->_unkListOffset) / 4; i++)
                    _unks.Add(*addr++);
            }
            if (hdr->turnFixed)
                _turnFrames = *(bfloat*)hdr->_turnFrameOffset.Address;
            else
                _turnFrames = hdr->_turnFrameOffset;
        }

        protected override int OnGetSize()
        {
            int size = 28;
            _lookupCount = 0;
            if (_hops.Count > 1)
            {
                _lookupCount++;
                size += _hops.Count * 4;
            }
            if (_unks.Count > 1)
            {
                _lookupCount++;
                size += _unks.Count * 4;
            }
            
            return size;
        }

        protected override void OnWrite(VoidPtr address)
        {
            int off = 0;
            if (_hops.Count > 1)
                off += _hops.Count * 4;
            if (_unks.Count > 1)
                off += _unks.Count * 4;

            sMultiJump* header = (sMultiJump*)(address + off);
            RebuildAddress = header;

            bfloat* addr = (bfloat*)address;

            if (_unks.Count > 1)
            {
                header->_unkListOffset = Offset(addr);
                if (header->_unkListOffset > 0)
                    _lookupOffsets.Add(&header->_unkListOffset);

                foreach (float f in _unks)
                    *addr++ = f;
            }
            else if (_unks.Count == 1)
                *((bfloat*)&header->_unkListOffset) = _unks[0];
            else
                *((bfloat*)&header->_unkListOffset) = 0;

            if (_hops.Count > 1)
            {
                header->_hopListOffset = Offset(addr);
                if (header->_hopListOffset > 0)
                    _lookupOffsets.Add(&header->_hopListOffset);
                
                foreach (float f in _hops)
                    *addr++ = f;
            }
            else if (_hops.Count == 1)
                *((bfloat*)&header->_hopListOffset) = _hops[0];
            else
                *((bfloat*)&header->_hopListOffset) = 0;

            header->_unk1 = _unk1;
            header->_unk2 = _unk2;
            header->_unk3 = _unk3;
            header->_horizontalBoost = _horizontalBoost;

            if (header->turnFixed)
                *(bfloat*)&header->_turnFrameOffset = _turnFrames;
            else
                header->_turnFrameOffset = (int)_turnFrames;
        }
    }

    public unsafe class MiscGlide : MovesetEntryNode
    {
        internal float[] floatEntries;
        internal int intEntry1 = 0;

        [Category("Glide")]
        public float[] Entries { get { return floatEntries; } set { floatEntries = value; SignalPropertyChange(); } }
        [Category("Glide")]
        public int Unknown { get { return intEntry1; } set { intEntry1 = value; SignalPropertyChange(); } }

        protected override void OnParse(VoidPtr address)
        {
            bfloat* floatval = (bfloat*)address;
            bint intval1 = *(bint*)(address + 80);

            floatEntries = new float[20];
            for (int i = 0; i < floatEntries.Length; i++)
                floatEntries[i] = floatval[i];
            intEntry1 = intval1;
        }

        protected override int OnGetSize()
        {
            _lookupCount = 0;
            return 84;
        }

        protected override void OnWrite(VoidPtr address)
        {
            RebuildAddress = address;
            for (int i = 0; i < 20; i++)
                if (i < floatEntries.Length)
                    *(bfloat*)(address + i * 4) = floatEntries[i];
                else
                    *(bfloat*)(address + i * 4) = 0;
            *(bint*)(address + 80) = intEntry1;
        }
    }

    public unsafe class MiscCrawl : MovesetEntryNode
    {
        internal float forward, backward;

        [Category("Crawl Acceleration")]
        public float Forward { get { return forward; } set { forward = value; SignalPropertyChange(); } }
        [Category("Crawl Acceleration")]
        public float Backward { get { return backward; } set { backward = value; SignalPropertyChange(); } }

        protected override void OnParse(VoidPtr address)
        {
            sCrawl* hdr = (sCrawl*)address;
            forward = hdr->_forward;
            backward = hdr->_backward;
        }

        protected override int OnGetSize()
        {
            _lookupCount = 0;
            return 8;
        }

        protected override void OnWrite(VoidPtr address)
        {
            RebuildAddress = address;
            sCrawl* header = (sCrawl*)address;
            header->_forward = forward;
            header->_backward = backward;
        }
    }

    public unsafe class MiscTether : MovesetEntryNode
    {
        internal int _numHangFrame = 0;
        internal float _unknown;

        [Category("Tether")]
        public int HangFrameCount { get { return _numHangFrame; } set { _numHangFrame = value; SignalPropertyChange(); } }
        [Category("Tether")]
        public float Unknown { get { return _unknown; } set { _unknown = value; SignalPropertyChange(); } }
        
        protected override void OnParse(VoidPtr address)
        {
            sTether* hdr = (sTether*)address;
            _numHangFrame = hdr->_numHangFrame;
            _unknown = hdr->_unk1;
        }

        protected override int OnGetSize()
        {
            _lookupCount = 0;
            return 8;
        }

        protected override void OnWrite(VoidPtr address)
        {
            sTether* header = (sTether*)(RebuildAddress = address);
            header->_numHangFrame = _numHangFrame;
            header->_unk1 = _unknown;
        }
    }

    public unsafe class MiscSoundData : ListOffset
    {
        public List<EntryListOffset<IndexValue>> _entries;
        protected override void OnParse(VoidPtr address)
        {
            base.OnParse(address);
            _entries = new List<EntryListOffset<IndexValue>>();
            for (int i = 0; i < Count; i++)
                _entries.Add(Parse<EntryListOffset<IndexValue>>(DataOffset + i * 8, 4));
        }

        protected override int OnGetSize()
        {
            int size = 8;
            _lookupCount = (_entries.Count > 0 ? 1 : 0);
            foreach (EntryListOffset<IndexValue> r in _entries)
            {
                _lookupCount += (r._entries.Count > 0 ? 1 : 0);
                size += 8 + r._entries.Count * 4;
            }
            return size;
        }
        
        protected override void OnWrite(VoidPtr address)
        {
            int sndOff = 0, mainOff = 0;
            foreach (EntryListOffset<IndexValue> r in _entries)
            {
                mainOff += 8;
                sndOff += r._entries.Count * 4;
            }

            //indices
            //sound list offsets
            //header
            
            bint* indices = (bint*)address;
            sListOffset* sndLists = (sListOffset*)(address + sndOff);
            sListOffset* header = (sListOffset*)((VoidPtr)sndLists + mainOff);

            RebuildAddress = header;

            if (_entries.Count > 0)
            {
                header->_startOffset = Offset(sndLists);
               _lookupOffsets.Add(&header->_startOffset);
            }

            header->_listCount = _entries.Count;

            foreach (EntryListOffset<IndexValue> r in _entries)
            {
                if (r._entries.Count > 0)
                {
                    sndLists->_startOffset = Offset(indices);
                    _lookupOffsets.Add(&sndLists->_startOffset);
                }

                (sndLists++)->_listCount = r._entries.Count;
                foreach (IndexValue b in r._entries)
                {
                    b.RebuildAddress = indices;
                    *indices++ = b.ItemIndex;
                }
            }
        }
    }
}