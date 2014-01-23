using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Imaging;
using System.Windows.Forms;
using BrawlLib.Wii.Graphics;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class REFFNode : NW4RArcEntryNode
    {
        internal REFF* Header { get { return (REFF*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.REFF; } }

        private int _unk1, _unk2, _unk3, _dataLen, _dataOff;
        private int _TableLen;
        private short _TableEntries;
        private short _TableUnk1;

        //[Category("REFF Data")]
        //public int DataLength { get { return _dataLen; } }
        //[Category("REFF Data")]
        //public int DataOffset { get { return _dataOff; } }

        //[Category("REFF Object Table")]
        //public int Length { get { return _TableLen; } }
        //[Category("REFF Object Table")]
        //public short NumEntries { get { return _TableEntries; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            REFF* header = Header;

            if (_name == null)
                _name = header->IdString;

            _dataLen = header->_dataLength;
            _dataOff = header->_dataOffset;
            _unk1 = header->_linkPrev;
            _unk2 = header->_linkNext;
            _unk3 = header->_padding;

            REFTypeObjectTable* objTable = header->Table;
            _TableLen = (int)objTable->_length;
            _TableEntries = (short)objTable->_entries;
            _TableUnk1 = (short)objTable->_unk1;

            return header->Table->_entries > 0;
        }

        public override void OnPopulate()
        {
            REFTypeObjectTable* table = Header->Table;
            REFTypeObjectEntry* Entry = table->First;
            for (int i = 0; i < table->_entries; i++, Entry = Entry->Next)
                new REFFEntryNode() { _name = Entry->Name, _offset = (int)Entry->DataOffset, _length = (int)Entry->DataLength }.Initialize(this, new DataSource((byte*)table->Address + Entry->DataOffset, (int)Entry->DataLength));
        }
        int tableLen = 0;
        public override int OnCalculateSize(bool force)
        {
            int size = 0x28 + (Name.Length + 1).Align(4);
            tableLen = 0x8;
            foreach (ResourceNode n in Children)
            {
                tableLen += n.Name.Length + 11;
                size += n.CalculateSize(force);
            }
            return size + (tableLen = tableLen.Align(4));
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            REFF* header = (REFF*)address;
            header->_linkPrev = 0;
            header->_linkNext = 0;
            header->_padding = 0;
            header->_dataLength = length - 0x18;
            header->_header._tag = header->_tag = REFF.Tag;
            header->_header.Endian = Endian.Big;
            header->_header._version = 7;
            header->_header._length = length;
            header->_header._firstOffset = 0x10;
            header->_header._numEntries = 1;
            header->IdString = Name;

            REFTypeObjectTable* table = (REFTypeObjectTable*)((byte*)header + header->_dataOffset + 0x18);
            table->_entries = (short)Children.Count;
            table->_unk1 = 0;
            table->_length = tableLen;

            REFTypeObjectEntry* entry = table->First;
            int offset = tableLen;
            foreach (ResourceNode n in Children)
            {
                entry->Name = n.Name;
                entry->DataOffset = offset;
                entry->DataLength = n._calcSize;
                n.Rebuild((VoidPtr)table + offset, n._calcSize, force);
                offset += n._calcSize;
                entry = entry->Next;
            }
        }

        internal static ResourceNode TryParse(DataSource source) { return ((REFF*)source.Address)->_tag == REFF.Tag ? new REFFNode() : null; }
    }
    public unsafe class REFFEntryNode : ResourceNode
    {
        internal REFFDataHeader* Header { get { return (REFFDataHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType
        {
            get
            {
                return ResourceType.REFFEntry;
            }
        }
        [Category("REFF Entry")]
        public int REFFOffset { get { return _offset; } }
        [Category("REFF Entry")]
        public int DataLength { get { return _length; } }

        public int _offset;
        public int _length;

        public override bool OnInitialize()
        {
            base.OnInitialize();

            return true;
        }

        public override void OnPopulate()
        {
            if (((REFFNode)Parent).VersionMinor == 7)
                new REFFEmitterNode7().Initialize(this, (VoidPtr)Header + 8, (int)Header->_headerSize);
            else
                new REFFEmitterNode9().Initialize(this, (VoidPtr)Header + 8, (int)Header->_headerSize);
            new REFFParticleNode().Initialize(this, (VoidPtr)Header->_params, (int)Header->_params->headersize);
            new REFFAnimationListNode()
            {
                _ptclTrackCount = *Header->_ptclTrackCount,
                _ptclInitTrackCount = *Header->_ptclInitTrackCount,
                _emitTrackCount = *Header->_emitTrackCount,
                _emitInitTrackCount = *Header->_emitInitTrackCount,
                _ptclTrackAddr = Header->_ptclTrack,
                _emitTrackAddr = Header->_emitTrack,
            }
            .Initialize(this, Header->_animations, WorkingUncompressed.Length - ((int)Header->_animations - (int)Header));
        }

        public override int OnCalculateSize(bool force)
        {
            int size = 8;
            foreach (ResourceNode r in Children)
                size += r.CalculateSize(true);
            return size;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            REFFDataHeader* d = (REFFDataHeader*)address;
            d->_headerSize = (uint)Children[0]._calcSize;
            Children[0].Rebuild(d->_descriptor.Address, Children[0]._calcSize, true);
            Children[1].Rebuild(d->_params, Children[1]._calcSize, true);
            Children[2].Rebuild(d->_ptclTrackCount, Children[2]._calcSize, true);
        }
    }

    public unsafe class REFFAnimationListNode : ResourceNode
    {
        internal VoidPtr First { get { return (VoidPtr)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType
        {
            get
            {
                return ResourceType.Container;
            }
        }
        public ushort _ptclTrackCount, _ptclInitTrackCount, _emitTrackCount, _emitInitTrackCount;
        public buint* _ptclTrackAddr, _emitTrackAddr;
        public List<uint> _ptclTrack, _emitTrack;

        [Category("Animation Table")]
        public ushort PtclTrackCount { get { return _ptclTrackCount; } }
        [Category("Animation Table")]
        public ushort PtclInitTrackCount { get { return _ptclInitTrackCount; } }
        [Category("Animation Table")]
        public ushort EmitTrackCount { get { return _emitTrackCount; } }
        [Category("Animation Table")]
        public ushort EmitInitTrackCount { get { return _emitInitTrackCount; } }

        public override bool OnInitialize()
        {
            _name = "Animations";

            return PtclTrackCount > 0 || EmitTrackCount > 0;
        }

        public override void OnPopulate()
        {
            int offset = 0;
            buint* addr = _ptclTrackAddr;
            addr += PtclTrackCount; //skip nulled pointers to size list
            for (int i = 0; i < PtclTrackCount; i++)
            {
                new REFFAnimationNode() { _isPtcl = true }.Initialize(this, First + offset, (int)*addr);
                offset += (int)*addr++;
            }
            addr = _emitTrackAddr;
            addr += EmitTrackCount; //skip nulled pointers to size list
            for (int i = 0; i < EmitTrackCount; i++)
            {
                new REFFAnimationNode().Initialize(this, First + offset, (int)*addr);
                offset += (int)*addr++;
            }
        }

        public ushort ptcl, emit;
        public override int OnCalculateSize(bool force)
        {
            ptcl = 0;
            emit = 0;
            int size = 8;
            size += Children.Count * 8;
            foreach (REFFAnimationNode e in Children)
            {
                if (e._isPtcl) ptcl++; else emit++;
                size += e.CalculateSize(true);
            }
            return size;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            buint* addr = (buint*)address;
            ((bushort*)addr)[0] = ptcl;
            ((bushort*)addr)[1] = (ushort)_ptclInitTrackCount; 
            addr += ptcl + 1;
            foreach (REFFAnimationNode e in Children)
                if (e._isPtcl)
                    *addr++ = (uint)e._calcSize;
            ((bushort*)addr)[0] = emit;
            ((bushort*)addr)[1] = (ushort)_emitInitTrackCount;
            addr += emit + 1;
            foreach (REFFAnimationNode e in Children)
                if (!e._isPtcl)
                    *addr++ = (uint)e._calcSize;
            VoidPtr ptr = addr;
            foreach (REFFAnimationNode e in Children)
                if (e._isPtcl)
                {
                    e.Rebuild(ptr, e._calcSize, true);
                    ptr += e._calcSize;
                }
            foreach (REFFAnimationNode e in Children)
                if (!e._isPtcl)
                {
                    e.Rebuild(ptr, e._calcSize, true);
                    ptr += e._calcSize;
                }
        }
    }

    public unsafe class REFFAnimationNode : ResourceNode
    {
        internal AnimCurveHeader* Header { get { return (AnimCurveHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.REFFAnimationList; } }
        
        internal AnimCurveHeader _hdr = new AnimCurveHeader();
        
        public bool _isPtcl = false;

        public enum AnimType
        {
            Particle,
            Emitter
        }

        [Category("Animation")]
        public AnimType Type { get { return _isPtcl ? AnimType.Particle : AnimType.Emitter; } set { _isPtcl = value == AnimType.Particle; SignalPropertyChange(); } }
        [Category("Animation")]
        public byte Magic { get { return _hdr.magic; } }
        [Category("Animation"), TypeConverter(typeof(DropDownListReffAnimType))]
        public string KindType 
        {
            get 
            {
                if (((REFFNode)Parent.Parent.Parent).VersionMinor == 9)
                    switch (CurveFlag)
                    {
                        case AnimCurveType.ParticleByte:
                        case AnimCurveType.ParticleFloat:
                            return ((v9AnimCurveTargetByteFloat)_hdr.kindType).ToString();
                        case AnimCurveType.ParticleRotate:
                            return ((v9AnimCurveTargetRotateFloat)_hdr.kindType).ToString();
                        case AnimCurveType.ParticleTexture:
                            return ((v9AnimCurveTargetPtclTex)_hdr.kindType).ToString();
                        case AnimCurveType.Child:
                            return ((v9AnimCurveTargetChild)_hdr.kindType).ToString();
                        case AnimCurveType.Field:
                            return ((v9AnimCurveTargetField)_hdr.kindType).ToString();
                        case AnimCurveType.PostField:
                            return ((v9AnimCurveTargetPostField)_hdr.kindType).ToString();
                        case AnimCurveType.EmitterFloat:
                            return ((v9AnimCurveTargetEmitterFloat)_hdr.kindType).ToString();
                    }
                else
                    return _hdr.kindType.ToString();
                    //switch (CurveFlag)
                    //{
                    //    case AnimCurveType.ParticleByte:
                    //    case AnimCurveType.ParticleFloat:
                    //        return ((v7AnimCurveTargetByteFloat)_hdr.kindType).ToString();
                    //    case AnimCurveType.ParticleRotate:
                    //        return ((v7AnimCurveTargetRotateFloat)_hdr.kindType).ToString();
                    //    case AnimCurveType.ParticleTexture:
                    //        return ((v7AnimCurveTargetPtclTex)_hdr.kindType).ToString();
                    //    case AnimCurveType.Child:
                    //        return ((v7AnimCurveTargetChild)_hdr.kindType).ToString();
                    //    case AnimCurveType.Field:
                    //        return ((v7AnimCurveTargetField)_hdr.kindType).ToString();
                    //    case AnimCurveType.PostField:
                    //        return ((v7AnimCurveTargetPostField)_hdr.kindType).ToString();
                    //    case AnimCurveType.EmitterFloat:
                    //        return ((v7AnimCurveTargetEmitterFloat)_hdr.kindType).ToString();
                    //}
                return null;
            }
            set
            {
                int i = 0;
                if (((REFFNode)Parent.Parent.Parent).VersionMinor == 9)
                    switch (CurveFlag)
                    {
                        case AnimCurveType.ParticleByte:
                        case AnimCurveType.ParticleFloat:
                            v9AnimCurveTargetByteFloat a;
                            if (Enum.TryParse<v9AnimCurveTargetByteFloat>(value, true, out a))
                                _hdr.kindType = (byte)a;
                            else if (int.TryParse(value, out i))
                                _hdr.kindType = (byte)i;
                            break;
                        case AnimCurveType.ParticleRotate:
                            v9AnimCurveTargetRotateFloat b;
                            if (Enum.TryParse<v9AnimCurveTargetRotateFloat>(value, true, out b))
                                _hdr.kindType = (byte)b;
                            else if (int.TryParse(value, out i))
                                _hdr.kindType = (byte)i;
                            break;
                        case AnimCurveType.ParticleTexture:
                            v9AnimCurveTargetPtclTex c;
                            if (Enum.TryParse<v9AnimCurveTargetPtclTex>(value, true, out c))
                                _hdr.kindType = (byte)c;
                            else if (int.TryParse(value, out i))
                                _hdr.kindType = (byte)i;
                            break;
                        case AnimCurveType.Child:
                            v9AnimCurveTargetChild d;
                            if (Enum.TryParse<v9AnimCurveTargetChild>(value, true, out d))
                                _hdr.kindType = (byte)d;
                            else if (int.TryParse(value, out i))
                                _hdr.kindType = (byte)i;
                            break;
                        case AnimCurveType.Field:
                            v9AnimCurveTargetField e;
                            if (Enum.TryParse<v9AnimCurveTargetField>(value, true, out e))
                                _hdr.kindType = (byte)e;
                            else if (int.TryParse(value, out i))
                                _hdr.kindType = (byte)i;
                            break;
                        case AnimCurveType.PostField:
                            v9AnimCurveTargetPostField f;
                            if (Enum.TryParse<v9AnimCurveTargetPostField>(value, true, out f))
                                _hdr.kindType = (byte)f;
                            else if (int.TryParse(value, out i))
                                _hdr.kindType = (byte)i;
                            break;
                        case AnimCurveType.EmitterFloat:
                            v9AnimCurveTargetEmitterFloat g;
                            if (Enum.TryParse<v9AnimCurveTargetEmitterFloat>(value, true, out g))
                                _hdr.kindType = (byte)g;
                            else if (int.TryParse(value, out i))
                                _hdr.kindType = (byte)i;
                            break;
                    }
                else
                {
                    if (int.TryParse(value, out i))
                        _hdr.kindType = (byte)i;
                }
            }
        }
        [Category("Animation")]
        public AnimCurveType CurveFlag { get { return (AnimCurveType)_hdr.curveFlag; } }//set { hdr.curveFlag = (byte)value; SignalPropertyChange(); } }
        [Category("Animation")]
        public byte KindEnable { get { return _hdr.kindEnable; } }
        [Category("Animation")]
        public AnimCurveHeaderProcessFlagType ProcessFlag { get { return (AnimCurveHeaderProcessFlagType)_hdr.processFlag; } }
        [Category("Animation")]
        public byte LoopCount { get { return _hdr.loopCount; } }

        [Category("Animation")]
        public ushort RandomSeed { get { return _hdr.randomSeed; } }
        [Category("Animation")]
        public ushort FrameLength { get { return _hdr.frameLength; } }
        [Category("Animation")]
        public ushort Padding { get { return _hdr.padding; } }

        [Category("Animation")]
        public uint KeyTableSize { get { return _hdr.keyTable; } }
        [Category("Animation")]
        public uint RangeTableSize { get { return _hdr.rangeTable; } }
        [Category("Animation")]
        public uint RandomTableSize { get { return _hdr.randomTable; } }
        [Category("Animation")]
        public uint NameTableSize { get { return _hdr.nameTable; } }
        [Category("Animation")]
        public uint InfoTableSize { get { return _hdr.infoTable; } }

        Random random = null;

        public override bool OnInitialize()
        {
            _hdr = *Header;
            _name = "AnimCurve" + Index;
            random = new Random(RandomSeed);
            //if (CurveFlag == AnimCurveType.EmitterFloat || CurveFlag == AnimCurveType.Field || CurveFlag == AnimCurveType.PostField)
            //    MessageBox.Show(TreePath);
            return KeyTableSize > 4 || RangeTableSize > 4 || RandomTableSize > 4 || NameTableSize > 4 || InfoTableSize > 4;
        }

        public override void OnPopulate()
        {
            if (KeyTableSize > 4)
                new REFFAnimCurveTableNode() { _name = "Key Table" }.Initialize(this, (VoidPtr)Header + 0x20, (int)KeyTableSize);
            if (RangeTableSize > 4)
                new REFFAnimCurveTableNode() { _name = "Range Table" }.Initialize(this, (VoidPtr)Header + 0x20 + KeyTableSize, (int)RangeTableSize);
            if (RandomTableSize > 4)
                new REFFAnimCurveTableNode() { _name = "Random Table" }.Initialize(this, (VoidPtr)Header + 0x20 + KeyTableSize + RangeTableSize, (int)RandomTableSize);
            if (NameTableSize > 4)
                new REFFAnimCurveNameTableNode() { _name = "Name Table" }.Initialize(this, (VoidPtr)Header + 0x20 + KeyTableSize + RangeTableSize + RandomTableSize, (int)NameTableSize);
            if (InfoTableSize > 4)
                new REFFAnimCurveTableNode() { _name = "Info Table" }.Initialize(this, (VoidPtr)Header + 0x20 + KeyTableSize + RangeTableSize + RandomTableSize + NameTableSize, (int)InfoTableSize);
        }

        public void ParseCommands()
        {
            //KindEnable enables the amount of values and where they affect
            Bin8 kind = KindEnable;
            List<int> off = new List<int>();
            for (int i = 0; i < 8; i++)
                if (kind[i])
                    off.Add(i);

            //Child KeyTable:
            //00000000 00000000 00000000 00000000 0080 0100 0102 0000
            //00000000 00000000 00000000 00000000 0080 0100 0102 0001
            //00000000 00000000 00000000 00000000 0080 0100 0102 0002


        }

        public override int OnCalculateSize(bool force)
        {
            return base.OnCalculateSize(force);
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            base.OnRebuild(address, length, force);
        }
    }

    public unsafe class REFFAnimCurveNameTableNode : ResourceNode
    {
        internal AnimCurveTableHeader* Header { get { return (AnimCurveTableHeader*)WorkingUncompressed.Address; } }

        [Category("Name Table")]
        public string[] Names { get { return _names.ToArray(); } set { _names = value.ToList<string>(); SignalPropertyChange(); } }
        public List<string> _names = new List<string>();

        public override bool OnInitialize()
        {
            _name = "Name Table";
            _names = new List<string>();
            bushort* addr = (bushort*)((VoidPtr)Header + 4 + Header->count * 4);
            for (int i = 0; i < Header->count; i++)
            {
                _names.Add(new String((sbyte*)addr + 2));
                addr = (bushort*)((VoidPtr)addr + 2 + *addr);
            }

            return false;
        }
    }

    public unsafe class REFFAnimCurveTableNode : ResourceNode
    {
        internal AnimCurveTableHeader* Header { get { return (AnimCurveTableHeader*)WorkingUncompressed.Address; } }

        [Category("AnimCurve Table")]
        public int Size { get { return WorkingUncompressed.Length; } }
        [Category("AnimCurve Table")]
        public int Count { get { return Header->count; } }
        [Category("AnimCurve Table")]
        public int Pad { get { return Header->pad; } }
        
        public override bool OnInitialize()
        {
            if (_name == null)
                _name = "Table" + Index;
            //return Count > 0;
            return false;
        }

        public override void OnPopulate()
        {
            //VoidPtr addr = (VoidPtr)Header + 4;
            //int s = (WorkingUncompressed.Length - 4) / Count;
            //for (int i = 0; i < Count; i++)
            //    new MoveDefSectionParamNode() { _name = "Entry" + i }.Initialize(this, (VoidPtr)Header + 4 + i * s, s);
        }
    }

    public unsafe class REFFPostFieldInfoNode : ResourceNode
    {
        internal PostFieldInfo* Header { get { return (PostFieldInfo*)WorkingUncompressed.Address; } }

        PostFieldInfo hdr;

        [Category("Post Field Info")]
        public Vector3 Scale { get { return hdr.mAnimatableParams.mSize; } }
        [Category("Post Field Info")]
        public Vector3 Rotation { get { return hdr.mAnimatableParams.mRotate; } }
        [Category("Post Field Info")]
        public Vector3 Translation { get { return hdr.mAnimatableParams.mTranslate; } }
        [Category("Post Field Info")]
        public float ReferenceSpeed { get { return hdr.mReferenceSpeed; } }
        [Category("Post Field Info")]
        public PostFieldInfo.ControlSpeedType ControlSpeedType { get { return (PostFieldInfo.ControlSpeedType)hdr.mControlSpeedType; } }
        [Category("Post Field Info")]
        public PostFieldInfo.CollisionShapeType CollisionShapeType { get { return (PostFieldInfo.CollisionShapeType)hdr.mCollisionShapeType; } }
        [Category("Post Field Info")]
        public PostFieldInfo.ShapeOption ShapeOption { get { return CollisionShapeType == PostFieldInfo.CollisionShapeType.Sphere || CollisionShapeType == PostFieldInfo.CollisionShapeType.Plane ? (PostFieldInfo.ShapeOption)(((int)CollisionShapeType << 2) | hdr.mCollisionShapeOption) : PostFieldInfo.ShapeOption.None; } }
        [Category("Post Field Info")]
        public PostFieldInfo.CollisionType CollisionType { get { return (PostFieldInfo.CollisionType)hdr.mCollisionType; } }
        [Category("Post Field Info")]
        public PostFieldInfo.CollisionOption CollisionOption { get { return (PostFieldInfo.CollisionOption)(short)hdr.mCollisionOption; } }
        [Category("Post Field Info")]
        public ushort StartFrame { get { return hdr.mStartFrame; } }
        [Category("Post Field Info")]
        public Vector3 SpeedFactor { get { return hdr.mSpeedFactor; } }

        public override bool OnInitialize()
        {
            _name = "Entry" + Index;
            hdr = *Header;
            return false;
        }

        public override void OnPopulate()
        {
            base.OnPopulate();
        }
    }

    public unsafe class REFFParticleNode : ResourceNode
    {
        internal ParticleParameterHeader* Params { get { return (ParticleParameterHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType
        {
            get
            {
                return ResourceType.Unknown;
            }
        }
        ParticleParameterHeader hdr = new ParticleParameterHeader();
        ParticleParameterDesc desc = new ParticleParameterDesc();

        //[Category("Particle Parameters")]
        //public uint HeaderSize { get { return hdr.headersize; } }

        [Category("Particle Parameters"), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel Color1Primary { get { return desc.mColor11; } set { desc.mColor11 = value; SignalPropertyChange(); } }
        [Category("Particle Parameters"), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel Color1Secondary { get { return desc.mColor12; } set { desc.mColor12 = value; SignalPropertyChange(); } }
        [Category("Particle Parameters"), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel Color2Primary { get { return desc.mColor21; } set { desc.mColor21 = value; SignalPropertyChange(); } }
        [Category("Particle Parameters"), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel Color2Secondary { get { return desc.mColor22; } set { desc.mColor22 = value; SignalPropertyChange(); } }
        
        [Category("Particle Parameters"), TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 Size { get { return desc.size; } set { desc.size = value; SignalPropertyChange(); } }
        [Category("Particle Parameters"), TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 Scale { get { return desc.scale; } set { desc.scale = value; SignalPropertyChange(); } }
        [Category("Particle Parameters"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Rotate { get { return desc.rotate; } set { desc.rotate = value; SignalPropertyChange(); } }

        [Category("Particle Parameters"), TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 TextureScale1 { get { return desc.textureScale1; } set { desc.textureScale1 = value; SignalPropertyChange(); } }
        [Category("Particle Parameters"), TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 TextureScale2 { get { return desc.textureScale2; } set { desc.textureScale2 = value; SignalPropertyChange(); } }
        [Category("Particle Parameters"), TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 TextureScale3 { get { return desc.textureScale3; } set { desc.textureScale3 = value; SignalPropertyChange(); } }

        [Category("Particle Parameters"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 TextureRotate { get { return desc.textureRotate; } set { desc.textureRotate = value; SignalPropertyChange(); } }

        [Category("Particle Parameters"), TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 TextureTranslate1 { get { return desc.textureTranslate1; } set { desc.textureTranslate1 = value; SignalPropertyChange(); } }
        [Category("Particle Parameters"), TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 TextureTranslate2 { get { return desc.textureTranslate2; } set { desc.textureTranslate2 = value; SignalPropertyChange(); } }
        [Category("Particle Parameters"), TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 TextureTranslate3 { get { return desc.textureTranslate3; } set { desc.textureTranslate3 = value; SignalPropertyChange(); } }

        [Category("Particle Parameters")]
        public ushort TextureWrap { get { return desc.textureWrap; } set { desc.textureWrap = value; SignalPropertyChange(); } }
        [Category("Particle Parameters")]
        public byte TextureReverse { get { return desc.textureReverse; } set { desc.textureReverse = value; SignalPropertyChange(); } }

        [Category("Particle Parameters")]
        public byte AlphaCompareRef0 { get { return desc.mACmpRef0; } set { desc.mACmpRef0 = value; SignalPropertyChange(); } }
        [Category("Particle Parameters")]
        public byte AlphaCompareRef1 { get { return desc.mACmpRef1; } set { desc.mACmpRef1 = value; SignalPropertyChange(); } }

        [Category("Particle Parameters")]
        public byte RotateOffsetRandom1 { get { return desc.rotateOffsetRandomX; } set { desc.rotateOffsetRandomX = value; SignalPropertyChange(); } }
        [Category("Particle Parameters")]
        public byte RotateOffsetRandom2 { get { return desc.rotateOffsetRandomY; } set { desc.rotateOffsetRandomY = value; SignalPropertyChange(); } }
        [Category("Particle Parameters")]
        public byte RotateOffsetRandom3 { get { return desc.rotateOffsetRandomZ; } set { desc.rotateOffsetRandomZ = value; SignalPropertyChange(); } }

        [Category("Particle Parameters"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 RotateOffset { get { return desc.rotateOffset; } set { desc.rotateOffset = value; SignalPropertyChange(); } }

        [Category("Particle Parameters")]
        public string Texture1Name { get { return _textureNames[0]; } set { _textureNames[0] = value; SignalPropertyChange(); } }
        [Category("Particle Parameters")]
        public string Texture2Name { get { return _textureNames[1]; } set { _textureNames[1] = value; SignalPropertyChange(); } }
        [Category("Particle Parameters")]
        public string Texture3Name { get { return _textureNames[2]; } set { _textureNames[2] = value; SignalPropertyChange(); } }

        public string[] _textureNames = new string[3] { "", "", "" };

        public override bool OnInitialize()
        {
            _name = "Particle";
            hdr = *Params;
            desc = hdr.paramDesc;

            VoidPtr addr = Params->paramDesc.textureNames.Address;
            for (int i = 0; i < 3; i++)
            {
                if (*(bushort*)addr > 1)
                    _textureNames[i] = new String((sbyte*)(addr + 2));
                else
                    _textureNames[i] = null;
                
                addr += 2 + *(bushort*)addr;
            }

            return false;
        }

        public override int OnCalculateSize(bool force)
        {
            int size = 0x8C;
            foreach (string s in _textureNames)
            {
                size += 3;
                if (s != null && s.Length > 0)
                    size += s.Length;
            }
            return size.Align(4);
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            ParticleParameterHeader* p = (ParticleParameterHeader*)address;
            p->headersize = (uint)length - 4;
            p->paramDesc = desc;
            sbyte* ptr = (sbyte*)p->paramDesc.textureNames.Address;
            foreach (string s in _textureNames)
                if (s != null && s.Length > 0)
                {
                    *(bushort*)ptr = (ushort)(s.Length + 1); 
                    ptr += 2;
                    s.Write(ref ptr);
                }
                else
                {
                    *(bushort*)ptr = 1;
                    ptr += 3;
                }
        }
    }
    public unsafe class REFFEmitterNode7 : ResourceNode
    {
        internal EmitterDesc* Descriptor { get { return (EmitterDesc*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType
        {
            get
            {
                return ResourceType.Container;
            }
        }
        EmitterDesc desc;

        [Category("Emitter Descriptor")]
        public EmitterDesc.EmitterCommonFlag CommonFlag { get { return (EmitterDesc.EmitterCommonFlag)(uint)desc.commonFlag; } set { desc.commonFlag = (uint)value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public uint EmitFlag { get { return desc.emitFlag; } set { desc.emitFlag = value; SignalPropertyChange(); } } // EmitFormType - value & 0xFF
        [Category("Emitter Descriptor")]
        public ushort EmitLife { get { return desc.emitLife; } set { desc.emitLife = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public ushort PtclLife { get { return desc.ptclLife; } set { desc.ptclLife = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public sbyte PtclLifeRandom { get { return desc.ptclLifeRandom; } set { desc.ptclLifeRandom = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public sbyte InheritChildPtclTranslate { get { return desc.inheritChildPtclTranslate; } set { desc.inheritChildPtclTranslate = value; SignalPropertyChange(); } }

        [Category("Emitter Descriptor")]
        public sbyte EmitIntervalRandom { get { return desc.emitEmitIntervalRandom; } set { desc.emitEmitIntervalRandom = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public sbyte EmitRandom { get { return desc.emitEmitRandom; } set { desc.emitEmitRandom = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float Emit { get { return desc.emitEmit; } set { desc.emitEmit = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public ushort EmitStart { get { return desc.emitEmitStart; } set { desc.emitEmitStart = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public ushort EmitPast { get { return desc.emitEmitPast; } set { desc.emitEmitPast = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public ushort EmitInterval { get { return desc.emitEmitInterval; } set { desc.emitEmitInterval = value; SignalPropertyChange(); } }

        [Category("Emitter Descriptor")]
        public sbyte InheritPtclTranslate { get { return desc.inheritPtclTranslate; } set { desc.inheritPtclTranslate = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public sbyte InheritChildEmitTranslate { get { return desc.inheritChildEmitTranslate; } set { desc.inheritChildEmitTranslate = value; SignalPropertyChange(); } }

        [Category("Emitter Descriptor")]
        public float CommonParam1 { get { return desc.commonParam1; } set { desc.commonParam1 = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float CommonParam2 { get { return desc.commonParam2; } set { desc.commonParam2 = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float CommonParam3 { get { return desc.commonParam3; } set { desc.commonParam3 = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float CommonParam4 { get { return desc.commonParam4; } set { desc.commonParam4 = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float CommonParam5 { get { return desc.commonParam5; } set { desc.commonParam5 = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float CommonParam6 { get { return desc.commonParam6; } set { desc.commonParam6 = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public ushort EmitEmitDiv { get { return desc.emitEmitDiv; } set { desc.emitEmitDiv = value; SignalPropertyChange(); } } //aka orig tick

        [Category("Emitter Descriptor")]
        public sbyte VelInitVelocityRandom { get { return desc.velInitVelocityRandom; } set { desc.velInitVelocityRandom = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public sbyte VelMomentumRandom { get { return desc.velMomentumRandom; } set { desc.velMomentumRandom = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelPowerRadiationDir { get { return desc.velPowerRadiationDir; } set { desc.velPowerRadiationDir = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelPowerYAxis { get { return desc.velPowerYAxis; } set { desc.velPowerYAxis = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelPowerRandomDir { get { return desc.velPowerRandomDir; } set { desc.velPowerRandomDir = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelPowerNormalDir { get { return desc.velPowerNormalDir; } set { desc.velPowerNormalDir = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelDiffusionEmitterNormal { get { return desc.velDiffusionEmitterNormal; } set { desc.velDiffusionEmitterNormal = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelPowerSpecDir { get { return desc.velPowerSpecDir; } set { desc.velPowerSpecDir = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelDiffusionSpecDir { get { return desc.velDiffusionSpecDir; } set { desc.velDiffusionSpecDir = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 VelSpecDir { get { return desc.velSpecDir; } set { desc.velSpecDir = value; SignalPropertyChange(); } }

        [Category("Emitter Descriptor"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Scale { get { return desc.scale; } set { desc.scale = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Rotate { get { return desc.rotate; } set { desc.rotate = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Translate { get { return desc.translate; } set { desc.translate = value; SignalPropertyChange(); } }

        [Category("Emitter Descriptor")]
        public byte LodNear { get { return desc.lodNear; } set { desc.lodNear = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public byte LodFar { get { return desc.lodFar; } set { desc.lodFar = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public byte LodMinEmit { get { return desc.lodMinEmit; } set { desc.lodMinEmit = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public byte LodAlpha { get { return desc.lodAlpha; } set { desc.lodAlpha = value; SignalPropertyChange(); } }
        
        [Category("Emitter Descriptor")]
        public uint RandomSeed { get { return desc.randomSeed; } set { desc.randomSeed = value; SignalPropertyChange(); } }

        //[Category("Emitter Descriptor")]
        //public byte userdata1 { get { fixed (byte* dat = desc.userdata) return dat[0]; } set { fixed (byte* dat = desc.userdata) dat[0] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata2 { get { fixed (byte* dat = desc.userdata) return dat[1]; } set { fixed (byte* dat = desc.userdata) dat[1] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata3 { get { fixed (byte* dat = desc.userdata) return dat[2]; } set { fixed (byte* dat = desc.userdata) dat[2] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata4 { get { fixed (byte* dat = desc.userdata) return dat[3]; } set { fixed (byte* dat = desc.userdata) dat[3] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata5 { get { fixed (byte* dat = desc.userdata) return dat[4]; } set { fixed (byte* dat = desc.userdata) dat[4] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata6 { get { fixed (byte* dat = desc.userdata) return dat[5]; } set { fixed (byte* dat = desc.userdata) dat[5] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata7 { get { fixed (byte* dat = desc.userdata) return dat[6]; } set { fixed (byte* dat = desc.userdata) dat[6] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata8 { get { fixed (byte* dat = desc.userdata) return dat[7]; } set { fixed (byte* dat = desc.userdata) dat[7] = value; SignalPropertyChange(); } }

        #region Draw Settings

        [Category("Draw Settings")]
        public DrawFlag mFlags { get { return (SSBBTypes.DrawFlag)(ushort)drawSetting.mFlags; } set { drawSetting.mFlags = (ushort)value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public AlphaCompare AlphaComparison0 { get { return (AlphaCompare)drawSetting.mACmpComp0; } set { drawSetting.mACmpComp0 = (byte)value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public AlphaCompare AlphaComparison1 { get { return (AlphaCompare)drawSetting.mACmpComp1; } set { drawSetting.mACmpComp1 = (byte)value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public AlphaOp AlphaCompareOperation { get { return (AlphaOp)drawSetting.mACmpOp; } set { drawSetting.mACmpOp = (byte)value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public byte TevStageCount { get { return drawSetting.mNumTevs; } set { drawSetting.mNumTevs = value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public bool FlagClamp { get { return drawSetting.mFlagClamp != 0; } set { drawSetting.mFlagClamp = (byte)(value ? 1: 0); SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public IndirectTargetStage IndirectTargetStage { get { return (SSBBTypes.IndirectTargetStage)drawSetting.mIndirectTargetStage; } set { drawSetting.mIndirectTargetStage = (byte)value; SignalPropertyChange(); } }

        #region Old

        //public byte mTevTexture1 { get { return drawSetting.mTevTexture1; } set { drawSetting.mTevTexture1 = value; SignalPropertyChange(); } }
        //public byte mTevTexture2 { get { return drawSetting.mTevTexture2; } set { drawSetting.mTevTexture2 = value; SignalPropertyChange(); } }
        //public byte mTevTexture3 { get { return drawSetting.mTevTexture3; } set { drawSetting.mTevTexture3 = value; SignalPropertyChange(); } }
        //public byte mTevTexture4 { get { return drawSetting.mTevTexture4; } set { drawSetting.mTevTexture4 = value; SignalPropertyChange(); } }

        //#region Color

        //[Category("TEV Color 1")]
        //public ColorArg c1mA { get { return (ColorArg)drawSetting.mTevColor1.mA; } set { drawSetting.mTevColor1.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 1")]
        //public ColorArg c1mB { get { return (ColorArg)drawSetting.mTevColor1.mB; } set { drawSetting.mTevColor1.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 1")]
        //public ColorArg c1mC { get { return (ColorArg)drawSetting.mTevColor1.mC; } set { drawSetting.mTevColor1.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 1")]
        //public ColorArg c1mD { get { return (ColorArg)drawSetting.mTevColor1.mD; } set { drawSetting.mTevColor1.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 2")]
        //public ColorArg c2mA { get { return (ColorArg)drawSetting.mTevColor2.mA; } set { drawSetting.mTevColor2.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 2")]
        //public ColorArg c2mB { get { return (ColorArg)drawSetting.mTevColor2.mB; } set { drawSetting.mTevColor2.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 2")]
        //public ColorArg c2mC { get { return (ColorArg)drawSetting.mTevColor2.mC; } set { drawSetting.mTevColor2.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 2")]
        //public ColorArg c2mD { get { return (ColorArg)drawSetting.mTevColor2.mD; } set { drawSetting.mTevColor2.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 3")]
        //public ColorArg c3mA { get { return (ColorArg)drawSetting.mTevColor3.mA; } set { drawSetting.mTevColor3.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 3")]
        //public ColorArg c3mB { get { return (ColorArg)drawSetting.mTevColor3.mB; } set { drawSetting.mTevColor3.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 3")]
        //public ColorArg c3mC { get { return (ColorArg)drawSetting.mTevColor3.mC; } set { drawSetting.mTevColor3.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 3")]
        //public ColorArg c3mD { get { return (ColorArg)drawSetting.mTevColor3.mD; } set { drawSetting.mTevColor3.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 4")]
        //public ColorArg c4mA { get { return (ColorArg)drawSetting.mTevColor4.mA; } set { drawSetting.mTevColor4.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 4")]
        //public ColorArg c4mB { get { return (ColorArg)drawSetting.mTevColor4.mB; } set { drawSetting.mTevColor4.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 4")]
        //public ColorArg c4mC { get { return (ColorArg)drawSetting.mTevColor4.mC; } set { drawSetting.mTevColor4.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 4")]
        //public ColorArg c4mD { get { return (ColorArg)drawSetting.mTevColor4.mD; } set { drawSetting.mTevColor4.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 1 Operation")]
        //public TevOp c1mOp { get { return (TevOp)drawSetting.mTevColorOp1.mOp; } set { drawSetting.mTevColorOp1.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 1 Operation")]
        //public Bias c1mBias { get { return (Bias)drawSetting.mTevColorOp1.mBias; } set { drawSetting.mTevColorOp1.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 1 Operation")]
        //public TevScale c1mScale { get { return (TevScale)drawSetting.mTevColorOp1.mScale; } set { drawSetting.mTevColorOp1.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 1 Operation")]
        //public bool c1mClamp { get { return drawSetting.mTevColorOp1.mClamp != 0; } set { drawSetting.mTevColorOp1.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Color 1 Operation")]
        //public TevRegID c1mOutReg { get { return (TevRegID)drawSetting.mTevColorOp1.mOutReg; } set { drawSetting.mTevColorOp1.mOutReg = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 2 Operation")]
        //public TevOp c2mOp { get { return (TevOp)drawSetting.mTevColorOp2.mOp; } set { drawSetting.mTevColorOp2.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 2 Operation")]
        //public Bias c2mBias { get { return (Bias)drawSetting.mTevColorOp2.mBias; } set { drawSetting.mTevColorOp2.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 2 Operation")]
        //public TevScale c2mScale { get { return (TevScale)drawSetting.mTevColorOp2.mScale; } set { drawSetting.mTevColorOp2.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 2 Operation")]
        //public bool c2mClamp { get { return drawSetting.mTevColorOp2.mClamp != 0; } set { drawSetting.mTevColorOp2.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Color 2 Operation")]
        //public TevRegID c2mOutReg { get { return (TevRegID)drawSetting.mTevColorOp2.mOutReg; } set { drawSetting.mTevColorOp2.mOutReg = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 3 Operation")]
        //public TevOp c3mOp { get { return (TevOp)drawSetting.mTevColorOp3.mOp; } set { drawSetting.mTevColorOp3.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 3 Operation")]
        //public Bias c3mBias { get { return (Bias)drawSetting.mTevColorOp3.mBias; } set { drawSetting.mTevColorOp3.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 3 Operation")]
        //public TevScale c3mScale { get { return (TevScale)drawSetting.mTevColorOp3.mScale; } set { drawSetting.mTevColorOp3.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 3 Operation")]
        //public bool c3mClamp { get { return drawSetting.mTevColorOp3.mClamp != 0; } set { drawSetting.mTevColorOp3.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Color 3 Operation")]
        //public TevRegID c3mOutReg { get { return (TevRegID)drawSetting.mTevColorOp3.mOutReg; } set { drawSetting.mTevColorOp3.mOutReg = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 4 Operation")]
        //public TevOp c4mOp { get { return (TevOp)drawSetting.mTevColorOp4.mOp; } set { drawSetting.mTevColorOp4.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 4 Operation")]
        //public Bias c4mBias { get { return (Bias)drawSetting.mTevColorOp4.mBias; } set { drawSetting.mTevColorOp4.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 4 Operation")]
        //public TevScale c4mScale { get { return (TevScale)drawSetting.mTevColorOp4.mScale; } set { drawSetting.mTevColorOp4.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 4 Operation")]
        //public bool c4mClamp { get { return drawSetting.mTevColorOp4.mClamp != 0; } set { drawSetting.mTevColorOp4.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Color 4 Operation")]
        //public TevRegID c4mOutReg { get { return (TevRegID)drawSetting.mTevColorOp4.mOutReg; } set { drawSetting.mTevColorOp4.mOutReg = (byte)value; SignalPropertyChange(); } }
        
        //#endregion  

        //#region Alpha

        //[Category("TEV Alpha 1")]
        //public ColorArg a1mA { get { return (ColorArg)drawSetting.mTevAlpha1.mA; } set { drawSetting.mTevAlpha1.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 1")]
        //public ColorArg a1mB { get { return (ColorArg)drawSetting.mTevAlpha1.mB; } set { drawSetting.mTevAlpha1.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 1")]
        //public ColorArg a1mC { get { return (ColorArg)drawSetting.mTevAlpha1.mC; } set { drawSetting.mTevAlpha1.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 1")]
        //public ColorArg a1mD { get { return (ColorArg)drawSetting.mTevAlpha1.mD; } set { drawSetting.mTevAlpha1.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 2")]
        //public ColorArg a2mA { get { return (ColorArg)drawSetting.mTevAlpha2.mA; } set { drawSetting.mTevAlpha2.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 2")]
        //public ColorArg a2mB { get { return (ColorArg)drawSetting.mTevAlpha2.mB; } set { drawSetting.mTevAlpha2.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 2")]
        //public ColorArg a2mC { get { return (ColorArg)drawSetting.mTevAlpha2.mC; } set { drawSetting.mTevAlpha2.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 2")]
        //public ColorArg a2mD { get { return (ColorArg)drawSetting.mTevAlpha2.mD; } set { drawSetting.mTevAlpha2.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 3")]
        //public ColorArg a3mA { get { return (ColorArg)drawSetting.mTevAlpha3.mA; } set { drawSetting.mTevAlpha3.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 3")]
        //public ColorArg a3mB { get { return (ColorArg)drawSetting.mTevAlpha3.mB; } set { drawSetting.mTevAlpha3.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 3")]
        //public ColorArg a3mC { get { return (ColorArg)drawSetting.mTevAlpha3.mC; } set { drawSetting.mTevAlpha3.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 3")]
        //public ColorArg a3mD { get { return (ColorArg)drawSetting.mTevAlpha3.mD; } set { drawSetting.mTevAlpha3.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 4")]
        //public ColorArg a4mA { get { return (ColorArg)drawSetting.mTevAlpha4.mA; } set { drawSetting.mTevAlpha4.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 4")]
        //public ColorArg a4mB { get { return (ColorArg)drawSetting.mTevAlpha4.mB; } set { drawSetting.mTevAlpha4.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 4")]
        //public ColorArg a4mC { get { return (ColorArg)drawSetting.mTevAlpha4.mC; } set { drawSetting.mTevAlpha4.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 4")]
        //public ColorArg a4mD { get { return (ColorArg)drawSetting.mTevAlpha4.mD; } set { drawSetting.mTevAlpha4.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 1 Operation")]
        //public TevOp a1mOp { get { return (TevOp)drawSetting.mTevAlphaOp1.mOp; } set { drawSetting.mTevAlphaOp1.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 1 Operation")]
        //public Bias a1mBias { get { return (Bias)drawSetting.mTevAlphaOp1.mBias; } set { drawSetting.mTevAlphaOp1.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 1 Operation")]
        //public TevScale a1mScale { get { return (TevScale)drawSetting.mTevAlphaOp1.mScale; } set { drawSetting.mTevAlphaOp1.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 1 Operation")]
        //public bool a1mClamp { get { return drawSetting.mTevAlphaOp1.mClamp != 0; } set { drawSetting.mTevAlphaOp1.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Alpha 1 Operation")]
        //public TevRegID a1mOutReg { get { return (TevRegID)drawSetting.mTevAlphaOp1.mOutReg; } set { drawSetting.mTevAlphaOp1.mOutReg = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 2 Operation")]
        //public TevOp a2mOp { get { return (TevOp)drawSetting.mTevAlphaOp2.mOp; } set { drawSetting.mTevAlphaOp2.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 2 Operation")]
        //public Bias a2mBias { get { return (Bias)drawSetting.mTevAlphaOp2.mBias; } set { drawSetting.mTevAlphaOp2.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 2 Operation")]
        //public TevScale a2mScale { get { return (TevScale)drawSetting.mTevAlphaOp2.mScale; } set { drawSetting.mTevAlphaOp2.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 2 Operation")]
        //public bool a2mClamp { get { return drawSetting.mTevAlphaOp2.mClamp != 0; } set { drawSetting.mTevAlphaOp2.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Alpha 2 Operation")]
        //public TevRegID a2mOutReg { get { return (TevRegID)drawSetting.mTevAlphaOp2.mOutReg; } set { drawSetting.mTevAlphaOp2.mOutReg = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 3 Operation")]
        //public TevOp a3mOp { get { return (TevOp)drawSetting.mTevAlphaOp3.mOp; } set { drawSetting.mTevAlphaOp3.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 3 Operation")]
        //public Bias a3mBias { get { return (Bias)drawSetting.mTevAlphaOp3.mBias; } set { drawSetting.mTevAlphaOp3.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 3 Operation")]
        //public TevScale a3mScale { get { return (TevScale)drawSetting.mTevAlphaOp3.mScale; } set { drawSetting.mTevAlphaOp3.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 3 Operation")]
        //public bool a3mClamp { get { return drawSetting.mTevAlphaOp3.mClamp != 0; } set { drawSetting.mTevAlphaOp3.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Alpha 3 Operation")]
        //public TevRegID a3mOutReg { get { return (TevRegID)drawSetting.mTevAlphaOp3.mOutReg; } set { drawSetting.mTevAlphaOp3.mOutReg = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 4 Operation")]
        //public TevOp a4mOp { get { return (TevOp)drawSetting.mTevAlphaOp4.mOp; } set { drawSetting.mTevAlphaOp4.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 4 Operation")]
        //public Bias a4mBias { get { return (Bias)drawSetting.mTevAlphaOp4.mBias; } set { drawSetting.mTevAlphaOp4.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 4 Operation")]
        //public TevScale a4mScale { get { return (TevScale)drawSetting.mTevAlphaOp4.mScale; } set { drawSetting.mTevAlphaOp4.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 4 Operation")]
        //public bool a4mClamp { get { return drawSetting.mTevAlphaOp4.mClamp != 0; } set { drawSetting.mTevAlphaOp4.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Alpha 4 Operation")]
        //public TevRegID a4mOutReg { get { return (TevRegID)drawSetting.mTevAlphaOp4.mOutReg; } set { drawSetting.mTevAlphaOp4.mOutReg = (byte)value; SignalPropertyChange(); } }

        //#endregion

        //[Category("Constant Register Selection")]
        //public TevKColorSel mTevKColorSel1 { get { return (TevKColorSel)drawSetting.mTevKColorSel1; } set { drawSetting.mTevKColorSel1 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKAlphaSel mTevKAlphaSel1 { get { return (TevKAlphaSel)drawSetting.mTevKAlphaSel1; } set { drawSetting.mTevKAlphaSel1 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKColorSel mTevKColorSel2 { get { return (TevKColorSel)drawSetting.mTevKColorSel2; } set { drawSetting.mTevKColorSel2 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKAlphaSel mTevKAlphaSel2 { get { return (TevKAlphaSel)drawSetting.mTevKAlphaSel2; } set { drawSetting.mTevKAlphaSel2 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKColorSel mTevKColorSel3 { get { return (TevKColorSel)drawSetting.mTevKColorSel3; } set { drawSetting.mTevKColorSel3 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKAlphaSel mTevKAlphaSel3 { get { return (TevKAlphaSel)drawSetting.mTevKAlphaSel3; } set { drawSetting.mTevKAlphaSel3 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKColorSel mTevKColorSel4 { get { return (TevKColorSel)drawSetting.mTevKColorSel4; } set { drawSetting.mTevKColorSel4 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKAlphaSel mTevKAlphaSel4 { get { return (TevKAlphaSel)drawSetting.mTevKAlphaSel4; } set { drawSetting.mTevKAlphaSel4 = (byte)value; SignalPropertyChange(); } }

        #endregion

        //BlendMode
        [Category("Blend Mode")]
        public GXBlendMode BlendType { get { return (GXBlendMode)drawSetting.mBlendMode.mType; } set { drawSetting.mBlendMode.mType = (byte)value; SignalPropertyChange(); } }
        [Category("Blend Mode")]
        public BlendFactor SrcFactor { get { return (BlendFactor)drawSetting.mBlendMode.mSrcFactor; } set { drawSetting.mBlendMode.mSrcFactor = (byte)value; SignalPropertyChange(); } }
        [Category("Blend Mode")]
        public BlendFactor DstFactor { get { return (BlendFactor)drawSetting.mBlendMode.mDstFactor; } set { drawSetting.mBlendMode.mDstFactor = (byte)value; SignalPropertyChange(); } }
        [Category("Blend Mode")]
        public GXLogicOp Operation { get { return (GXLogicOp)drawSetting.mBlendMode.mOp; } set { drawSetting.mBlendMode.mOp = (byte)value; SignalPropertyChange(); } }

        //Color
        [Category("Color Input")]
        public SSBBTypes.ColorInput.RasColor RasterColor { get { return (SSBBTypes.ColorInput.RasColor)drawSetting.mColorInput.mRasColor; } set { drawSetting.mColorInput.mRasColor = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevColor1 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevColor1; } set { drawSetting.mColorInput.mTevColor1 = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevColor2 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevColor2; } set { drawSetting.mColorInput.mTevColor2 = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevColor3 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevColor3; } set { drawSetting.mColorInput.mTevColor3 = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevKColor1 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevKColor1; } set { drawSetting.mColorInput.mTevKColor1 = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevKColor2 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevKColor2; } set { drawSetting.mColorInput.mTevKColor2 = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevKColor3 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevKColor3; } set { drawSetting.mColorInput.mTevKColor3 = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevKColor4 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevKColor4; } set { drawSetting.mColorInput.mTevKColor4 = (byte)value; SignalPropertyChange(); } }
        
        ////Alpha
        //[Category("Alpha Input")]
        //public EmitterDrawSetting.ColorInput.RasColor amRasColor { get { return (EmitterDrawSetting.ColorInput.RasColor)drawSetting.mAlphaInput.mRasColor; } set { drawSetting.mAlphaInput.mRasColor = (byte)value; SignalPropertyChange(); } }
        //[Category("Alpha Input")]
        //public EmitterDrawSetting.ColorInput.TevColor amTevColor1 { get { return (EmitterDrawSetting.ColorInput.TevColor)drawSetting.mAlphaInput.mTevColor1; } set { drawSetting.mAlphaInput.mTevColor1 = (byte)value; SignalPropertyChange(); } }
        //[Category("Alpha Input")]
        //public EmitterDrawSetting.ColorInput.TevColor amTevColor2 { get { return (EmitterDrawSetting.ColorInput.TevColor)drawSetting.mAlphaInput.mTevColor2; } set { drawSetting.mAlphaInput.mTevColor2 = (byte)value; SignalPropertyChange(); } }
        //[Category("Alpha Input")]
        //public EmitterDrawSetting.ColorInput.TevColor amTevColor3 { get { return (EmitterDrawSetting.ColorInput.TevColor)drawSetting.mAlphaInput.mTevColor3; } set { drawSetting.mAlphaInput.mTevColor3 = (byte)value; SignalPropertyChange(); } }
        //[Category("Alpha Input")]
        //public EmitterDrawSetting.ColorInput.TevColor amTevKColor1 { get { return (EmitterDrawSetting.ColorInput.TevColor)drawSetting.mAlphaInput.mTevKColor1; } set { drawSetting.mAlphaInput.mTevKColor1 = (byte)value; SignalPropertyChange(); } }
        //[Category("Alpha Input")]
        //public EmitterDrawSetting.ColorInput.TevColor amTevKColor2 { get { return (EmitterDrawSetting.ColorInput.TevColor)drawSetting.mAlphaInput.mTevKColor2; } set { drawSetting.mAlphaInput.mTevKColor2 = (byte)value; SignalPropertyChange(); } }
        //[Category("Alpha Input")]
        //public EmitterDrawSetting.ColorInput.TevColor amTevKColor3 { get { return (EmitterDrawSetting.ColorInput.TevColor)drawSetting.mAlphaInput.mTevKColor3; } set { drawSetting.mAlphaInput.mTevKColor3 = (byte)value; SignalPropertyChange(); } }
        //[Category("Alpha Input")]
        //public EmitterDrawSetting.ColorInput.TevColor amTevKColor4 { get { return (EmitterDrawSetting.ColorInput.TevColor)drawSetting.mAlphaInput.mTevKColor4; } set { drawSetting.mAlphaInput.mTevKColor4 = (byte)value; SignalPropertyChange(); } }

        [Category("Draw Settings")]
        public GXCompare ZCompareFunc { get { return (GXCompare)drawSetting.mZCompareFunc; } set { drawSetting.mZCompareFunc = (byte)value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public SSBBTypes.AlphaFlickType AlphaFlickType { get { return (SSBBTypes.AlphaFlickType)drawSetting.mAlphaFlickType; } set { drawSetting.mAlphaFlickType = (byte)value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public ushort AlphaFlickCycle { get { return drawSetting.mAlphaFlickCycle; } set { drawSetting.mAlphaFlickCycle = value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public byte AlphaFlickRandom { get { return drawSetting.mAlphaFlickRandom; } set { drawSetting.mAlphaFlickRandom = value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public byte AlphaFlickAmplitude { get { return drawSetting.mAlphaFlickAmplitude; } set { drawSetting.mAlphaFlickAmplitude = value; SignalPropertyChange(); } }

        //mLighting 
        [Category("Lighting")]
        public SSBBTypes.Lighting.Mode Mode { get { return (SSBBTypes.Lighting.Mode)drawSetting.mLighting.mMode; } set { drawSetting.mLighting.mMode = (byte)value; SignalPropertyChange(); } }
        [Category("Lighting")]
        public SSBBTypes.Lighting.Type LightType { get { return (SSBBTypes.Lighting.Type)drawSetting.mLighting.mType; } set { drawSetting.mLighting.mMode = (byte)value; SignalPropertyChange(); } }
        [Category("Lighting"), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel Ambient { get { return drawSetting.mLighting.mAmbient; } set { drawSetting.mLighting.mAmbient = value; SignalPropertyChange(); } }
        [Category("Lighting"), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel Diffuse { get { return drawSetting.mLighting.mDiffuse; } set { drawSetting.mLighting.mDiffuse = value; SignalPropertyChange(); } }
        [Category("Lighting")]
        public float Radius { get { return drawSetting.mLighting.mRadius; } set { drawSetting.mLighting.mRadius = value; SignalPropertyChange(); } }
        [Category("Lighting"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Position { get { return drawSetting.mLighting.mPosition; } set { drawSetting.mLighting.mPosition = value; SignalPropertyChange(); } }
        
        //[Category("Draw Settings")]
        //public fixed float mIndTexOffsetMtx[6] { get { return drawSetting.mFlags; } set { drawSetting.mFlags = value; SignalPropertyChange(); } } //2x3 Matrix
        [Category("Draw Settings")]
        public sbyte IndTexScaleExp { get { return drawSetting.mIndTexScaleExp; } set { drawSetting.mIndTexScaleExp = value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public sbyte PivotX { get { return drawSetting.pivotX; } set { drawSetting.pivotX = value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public sbyte PivotY { get { return drawSetting.pivotY; } set { drawSetting.pivotY = value; SignalPropertyChange(); } }
        //[Category("Draw Settings")]
        //public byte padding { get { return drawSetting.padding; } set { drawSetting.padding = value; SignalPropertyChange(); } }
        [Category("Particle Settings")]
        public SSBBTypes.ReffType ParticleType 
        {
            get { return (SSBBTypes.ReffType)drawSetting.ptcltype; }
            set
            {
                if (!(ParticleType >= SSBBTypes.ReffType.Stripe && value >= SSBBTypes.ReffType.Stripe))
                    typeOption2._data = 0;

                drawSetting.ptcltype = (byte)value;

                SignalPropertyChange();
                UpdateProperties();
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffBillboardAssist))]
        public string BillboardAssist 
        {
            get
            {
                if (ParticleType == SSBBTypes.ReffType.Billboard)
                    return ((SSBBTypes.BillboardAssist)drawSetting.typeOption).ToString();
                else
                    return "";
            } 
            set
            {
                if (ParticleType == SSBBTypes.ReffType.Billboard && !String.IsNullOrEmpty(value))
                {
                    drawSetting.typeOption = (byte)(SSBBTypes.BillboardAssist)Enum.Parse(typeof(SSBBTypes.BillboardAssist), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffStripeAssist))]
        public string StripeAssist
        {
            get
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe)
                    return ((SSBBTypes.StripeAssist)drawSetting.typeOption).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe && !String.IsNullOrEmpty(value))
                {
                    drawSetting.typeOption = (byte)(SSBBTypes.StripeAssist)Enum.Parse(typeof(SSBBTypes.StripeAssist), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffAssist))]
        public string Assist
        {
            get
            {
                if (ParticleType != SSBBTypes.ReffType.Billboard)
                    return ((SSBBTypes.Assist)drawSetting.typeOption).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType != SSBBTypes.ReffType.Billboard && !String.IsNullOrEmpty(value))
                {
                    drawSetting.typeOption = (byte)(SSBBTypes.Assist)Enum.Parse(typeof(SSBBTypes.Assist), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffBillboardDirection))]
        public string BillboardDirection
        {
            get
            {
                if (ParticleType == SSBBTypes.ReffType.Billboard)
                    return ((SSBBTypes.BillboardAhead)drawSetting.typeDir).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType == SSBBTypes.ReffType.Billboard && !String.IsNullOrEmpty(value))
                {
                    drawSetting.typeDir = (byte)(SSBBTypes.BillboardAhead)Enum.Parse(typeof(SSBBTypes.BillboardAhead), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffDirection))]
        public string Direction
        {
            get
            {
                if (ParticleType != SSBBTypes.ReffType.Billboard)
                    return ((SSBBTypes.Ahead)drawSetting.typeOption).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType != SSBBTypes.ReffType.Billboard && !String.IsNullOrEmpty(value))
                {
                    drawSetting.typeOption = (byte)(SSBBTypes.Ahead)Enum.Parse(typeof(SSBBTypes.Ahead), value);
                    SignalPropertyChange();
                }
            }
        }

        [Category("Particle Settings")]
        public SSBBTypes.RotateAxis TypeAxis { get { return (SSBBTypes.RotateAxis)drawSetting.typeAxis; } set { drawSetting.typeAxis = (byte)value; SignalPropertyChange(); } }

        private Bin8 typeOption2;

        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffStripeConnect))]
        public string StripeConnect
        {
            get
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe)
                    return ((SSBBTypes.StripeConnect)typeOption2[0, 3]).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe && !String.IsNullOrEmpty(value))
                {
                    typeOption2[0, 3] = (byte)(SSBBTypes.StripeConnect)Enum.Parse(typeof(SSBBTypes.StripeConnect), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffStripeInitialPrevAxis))]
        public string StripeInitialPrevAxis
        {
            get
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe)
                    return ((SSBBTypes.StripeInitialPrevAxis)typeOption2[3, 3]).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe && !String.IsNullOrEmpty(value))
                {
                    typeOption2[3, 3] = (byte)(SSBBTypes.StripeInitialPrevAxis)Enum.Parse(typeof(SSBBTypes.StripeInitialPrevAxis), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffStripeTexmapType))]
        public string StripeTexmapType
        {
            get
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe)
                    return ((SSBBTypes.StripeTexmapType)typeOption2[6, 1]).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe && !String.IsNullOrEmpty(value))
                {
                    typeOption2[6, 1] = (byte)(SSBBTypes.StripeTexmapType)Enum.Parse(typeof(SSBBTypes.StripeTexmapType), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffDirectionalPivot))]
        public string DirectionalPivot
        {
            get
            {
                if (ParticleType == SSBBTypes.ReffType.Directional)
                    return ((SSBBTypes.DirectionalPivot)typeOption2._data).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType == SSBBTypes.ReffType.Directional && !String.IsNullOrEmpty(value))
                {
                    typeOption2._data = (byte)(SSBBTypes.DirectionalPivot)Enum.Parse(typeof(SSBBTypes.StripeTexmapType), value);
                    SignalPropertyChange();
                }
            }
        }

        [Category("Particle Settings")]
        public string DirectionalChangeYBySpeed
        {
            get
            {
                if (ParticleType == SSBBTypes.ReffType.Directional)
                    return (drawSetting.typeOption0 != 0).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType == SSBBTypes.ReffType.Directional && !String.IsNullOrEmpty(value))
                {
                    bool b;
                    bool.TryParse(value, out b);
                    drawSetting.typeOption0 = (byte)(b ? 1 : 0);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings")]
        public string StripeTubeVertexCount
        {
            get
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe)
                    return drawSetting.typeOption0.ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe && !String.IsNullOrEmpty(value))
                {
                    byte b;
                    byte.TryParse(value, out b);
                    if (b >= 3)
                        drawSetting.typeOption0 = b;
                    SignalPropertyChange();
                }
            }
        }

        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffDirectionalFace))]
        public string DirectionalFace
        {
            get
            {
                if (ParticleType == SSBBTypes.ReffType.Directional)
                    return ((SSBBTypes.Face)drawSetting.typeOption1).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType == SSBBTypes.ReffType.Directional && !String.IsNullOrEmpty(value))
                {
                    drawSetting.typeOption1 = (byte)(SSBBTypes.Face)Enum.Parse(typeof(SSBBTypes.Face), value);
                    SignalPropertyChange();
                }
            }
        }

        [Category("Particle Settings")]
        public string StripeInterpDivisionCount
        {
            get
            {
                if (ParticleType == SSBBTypes.ReffType.SmoothStripe)
                    return drawSetting.typeOption1.ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType == SSBBTypes.ReffType.SmoothStripe && !String.IsNullOrEmpty(value))
                {
                    byte b;
                    byte.TryParse(value, out b);
                    if (b >= 1)
                        drawSetting.typeOption1 = b;
                    SignalPropertyChange();
                }
            }
        }
        
        //[Category("Draw Settings")]
        //public byte padding4 { get { return drawSetting.padding4; } set { drawSetting.padding4 = value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public float ZOffset { get { return drawSetting.zOffset; } set { drawSetting.zOffset = value; SignalPropertyChange(); } }
        
        #endregion

        EmitterDrawSetting7 drawSetting;

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _name = "Emitter";

            desc = *Descriptor;
            drawSetting = *(EmitterDrawSetting7*)&Descriptor->drawSetting;
            typeOption2 = new Bin8(drawSetting.typeOption2);
            
            return TevStageCount > 0;
        }

        public override void OnPopulate()
        {
            int col1 = 0;
            int colop1 = col1 + 16;
            int alpha1 = colop1 + 20;
            int alphaop1 = alpha1 + 16;
            int csel1 = alphaop1 + 20;
            for (int i = 0; i < 4; i++)
            {
                REFFTEVStage s = new REFFTEVStage(i);

                byte* p = (byte*)drawSetting.mTevColor1.Address;
                {
                    s.kcsel = p[csel1 + i];
                    s.kasel = p[csel1 + 4 + i];

                    s.cseld = p[col1 + 4 * i + 3];
                    s.cselc = p[col1 + 4 * i + 2];
                    s.cselb = p[col1 + 4 * i + 1];
                    s.csela = p[col1 + 4 * i + 0];

                    s.cop = p[colop1 + 5 * i + 0];
                    s.cbias = p[colop1 + 5 * i + 1];
                    s.cshift = p[colop1 + 5 * i + 2];
                    s.cclamp = p[colop1 + 5 * i + 3];
                    s.cdest = p[colop1 + 5 * i + 4];

                    s.aseld = p[alpha1 + 4 * i + 3];
                    s.aselc = p[alpha1 + 4 * i + 2];
                    s.aselb = p[alpha1 + 4 * i + 1];
                    s.asela = p[alpha1 + 4 * i + 0];

                    s.aop = p[alphaop1 + 5 * i + 0];
                    s.abias = p[alphaop1 + 5 * i + 1];
                    s.ashift = p[alphaop1 + 5 * i + 2];
                    s.aclamp = p[alphaop1 + 5 * i + 3];
                    s.adest = p[alphaop1 + 5 * i + 4];
                }

                s.ti = 0; 
                s.tc = 0;
                s.cc = 0;
                s.te = false;

                s.Parent = this;
            }
        }

        public override int OnCalculateSize(bool force)
        {
            return 0x140;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            EmitterDesc* hdr = (EmitterDesc*)address;
            *hdr = desc;
            *(EmitterDrawSetting7*)&hdr->drawSetting = drawSetting;
            hdr->drawSetting.typeOption2 = typeOption2._data;
            int col1 = 0;
            int colop1 = col1 + 16;
            int alpha1 = colop1 + 20;
            int alphaop1 = alpha1 + 16;
            int csel1 = alphaop1 + 20;
            for (int i = 0; i < 4; i++)
            {
                REFFTEVStage s = (REFFTEVStage)Children[i];

                byte* p = (byte*)(*(EmitterDrawSetting7*)&hdr->drawSetting).mTevColor1.Address;
                {
                    p[csel1 + i] = (byte)s.kcsel;
                    p[csel1 + 4 + i] = (byte)s.kasel;

                    p[col1 + 4 * i + 3] = (byte)s.cseld;
                    p[col1 + 4 * i + 2] = (byte)s.cselc;
                    p[col1 + 4 * i + 1] = (byte)s.cselb;
                    p[col1 + 4 * i + 0] = (byte)s.csela;

                    p[colop1 + 5 * i + 0] = (byte)s.cop;
                    p[colop1 + 5 * i + 1] = (byte)s.cbias;
                    p[colop1 + 5 * i + 2] = (byte)s.cshift;
                    p[colop1 + 5 * i + 3] = (byte)s.cclamp;
                    p[colop1 + 5 * i + 4] = (byte)s.cdest;

                    p[alpha1 + 4 * i + 3] = (byte)s.aseld;
                    p[alpha1 + 4 * i + 2] = (byte)s.aselc;
                    p[alpha1 + 4 * i + 1] = (byte)s.aselb;
                    p[alpha1 + 4 * i + 0] = (byte)s.asela;

                    p[alphaop1 + 5 * i + 0] = (byte)s.aop;
                    p[alphaop1 + 5 * i + 1] = (byte)s.abias;
                    p[alphaop1 + 5 * i + 2] = (byte)s.ashift;
                    p[alphaop1 + 5 * i + 3] = (byte)s.aclamp;
                    p[alphaop1 + 5 * i + 4] = (byte)s.adest;
                }
            }
        }
    }

    public unsafe class REFFEmitterNode9 : ResourceNode
    {
        internal EmitterDesc* Descriptor { get { return (EmitterDesc*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType
        {
            get
            {
                return ResourceType.Container;
            }
        }

        EmitterDesc desc;

        [Category("Emitter Descriptor")]
        public EmitterDesc.EmitterCommonFlag CommonFlag { get { return (EmitterDesc.EmitterCommonFlag)(uint)desc.commonFlag; } set { desc.commonFlag = (uint)value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public uint EmitFlag { get { return desc.emitFlag; } set { desc.emitFlag = value; SignalPropertyChange(); } } // EmitFormType - value & 0xFF
        [Category("Emitter Descriptor")]
        public ushort EmitLife { get { return desc.emitLife; } set { desc.emitLife = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public ushort PtclLife { get { return desc.ptclLife; } set { desc.ptclLife = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public sbyte PtclLifeRandom { get { return desc.ptclLifeRandom; } set { desc.ptclLifeRandom = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public sbyte InheritChildPtclTranslate { get { return desc.inheritChildPtclTranslate; } set { desc.inheritChildPtclTranslate = value; SignalPropertyChange(); } }

        [Category("Emitter Descriptor")]
        public sbyte EmitIntervalRandom { get { return desc.emitEmitIntervalRandom; } set { desc.emitEmitIntervalRandom = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public sbyte EmitRandom { get { return desc.emitEmitRandom; } set { desc.emitEmitRandom = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float Emit { get { return desc.emitEmit; } set { desc.emitEmit = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public ushort EmitStart { get { return desc.emitEmitStart; } set { desc.emitEmitStart = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public ushort EmitPast { get { return desc.emitEmitPast; } set { desc.emitEmitPast = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public ushort EmitInterval { get { return desc.emitEmitInterval; } set { desc.emitEmitInterval = value; SignalPropertyChange(); } }

        [Category("Emitter Descriptor")]
        public sbyte InheritPtclTranslate { get { return desc.inheritPtclTranslate; } set { desc.inheritPtclTranslate = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public sbyte InheritChildEmitTranslate { get { return desc.inheritChildEmitTranslate; } set { desc.inheritChildEmitTranslate = value; SignalPropertyChange(); } }

        [Category("Emitter Descriptor")]
        public float CommonParam1 { get { return desc.commonParam1; } set { desc.commonParam1 = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float CommonParam2 { get { return desc.commonParam2; } set { desc.commonParam2 = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float CommonParam3 { get { return desc.commonParam3; } set { desc.commonParam3 = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float CommonParam4 { get { return desc.commonParam4; } set { desc.commonParam4 = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float CommonParam5 { get { return desc.commonParam5; } set { desc.commonParam5 = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float CommonParam6 { get { return desc.commonParam6; } set { desc.commonParam6 = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public ushort EmitEmitDiv { get { return desc.emitEmitDiv; } set { desc.emitEmitDiv = value; SignalPropertyChange(); } } //aka orig tick

        [Category("Emitter Descriptor")]
        public sbyte VelInitVelocityRandom { get { return desc.velInitVelocityRandom; } set { desc.velInitVelocityRandom = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public sbyte VelMomentumRandom { get { return desc.velMomentumRandom; } set { desc.velMomentumRandom = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelPowerRadiationDir { get { return desc.velPowerRadiationDir; } set { desc.velPowerRadiationDir = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelPowerYAxis { get { return desc.velPowerYAxis; } set { desc.velPowerYAxis = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelPowerRandomDir { get { return desc.velPowerRandomDir; } set { desc.velPowerRandomDir = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelPowerNormalDir { get { return desc.velPowerNormalDir; } set { desc.velPowerNormalDir = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelDiffusionEmitterNormal { get { return desc.velDiffusionEmitterNormal; } set { desc.velDiffusionEmitterNormal = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelPowerSpecDir { get { return desc.velPowerSpecDir; } set { desc.velPowerSpecDir = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public float VelDiffusionSpecDir { get { return desc.velDiffusionSpecDir; } set { desc.velDiffusionSpecDir = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 VelSpecDir { get { return desc.velSpecDir; } set { desc.velSpecDir = value; SignalPropertyChange(); } }

        [Category("Emitter Descriptor"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Scale { get { return desc.scale; } set { desc.scale = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Rotate { get { return desc.rotate; } set { desc.rotate = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Translate { get { return desc.translate; } set { desc.translate = value; SignalPropertyChange(); } }

        [Category("Emitter Descriptor")]
        public byte LodNear { get { return desc.lodNear; } set { desc.lodNear = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public byte LodFar { get { return desc.lodFar; } set { desc.lodFar = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public byte LodMinEmit { get { return desc.lodMinEmit; } set { desc.lodMinEmit = value; SignalPropertyChange(); } }
        [Category("Emitter Descriptor")]
        public byte LodAlpha { get { return desc.lodAlpha; } set { desc.lodAlpha = value; SignalPropertyChange(); } }

        [Category("Emitter Descriptor")]
        public uint RandomSeed { get { return desc.randomSeed; } set { desc.randomSeed = value; SignalPropertyChange(); } }

        //[Category("Emitter Descriptor")]
        //public byte userdata1 { get { fixed (byte* dat = desc.userdata) return dat[0]; } set { fixed (byte* dat = desc.userdata) dat[0] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata2 { get { fixed (byte* dat = desc.userdata) return dat[1]; } set { fixed (byte* dat = desc.userdata) dat[1] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata3 { get { fixed (byte* dat = desc.userdata) return dat[2]; } set { fixed (byte* dat = desc.userdata) dat[2] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata4 { get { fixed (byte* dat = desc.userdata) return dat[3]; } set { fixed (byte* dat = desc.userdata) dat[3] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata5 { get { fixed (byte* dat = desc.userdata) return dat[4]; } set { fixed (byte* dat = desc.userdata) dat[4] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata6 { get { fixed (byte* dat = desc.userdata) return dat[5]; } set { fixed (byte* dat = desc.userdata) dat[5] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata7 { get { fixed (byte* dat = desc.userdata) return dat[6]; } set { fixed (byte* dat = desc.userdata) dat[6] = value; SignalPropertyChange(); } }
        //[Category("Emitter Descriptor")]
        //public byte userdata8 { get { fixed (byte* dat = desc.userdata) return dat[7]; } set { fixed (byte* dat = desc.userdata) dat[7] = value; SignalPropertyChange(); } }

        #region Draw Settings

        [Category("Draw Settings")]
        public DrawFlag mFlags { get { return (SSBBTypes.DrawFlag)(ushort)drawSetting.mFlags; } set { drawSetting.mFlags = (ushort)value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public AlphaCompare AlphaComparison0 { get { return (AlphaCompare)drawSetting.mACmpComp0; } set { drawSetting.mACmpComp0 = (byte)value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public AlphaCompare AlphaComparison1 { get { return (AlphaCompare)drawSetting.mACmpComp1; } set { drawSetting.mACmpComp1 = (byte)value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public AlphaOp AlphaCompareOperation { get { return (AlphaOp)drawSetting.mACmpOp; } set { drawSetting.mACmpOp = (byte)value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public byte TevStageCount { get { return drawSetting.mNumTevs; } set { drawSetting.mNumTevs = value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public bool FlagClamp { get { return drawSetting.mFlagClamp != 0; } set { drawSetting.mFlagClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public IndirectTargetStage IndirectTargetStage { get { return (SSBBTypes.IndirectTargetStage)drawSetting.mIndirectTargetStage; } set { drawSetting.mIndirectTargetStage = (byte)value; SignalPropertyChange(); } }

        [Category("TEV")]
        public byte mTevTexture1 { get { return drawSetting.mTevTexture1; } set { drawSetting.mTevTexture1 = value; SignalPropertyChange(); } }
        [Category("TEV")]
        public byte mTevTexture2 { get { return drawSetting.mTevTexture2; } set { drawSetting.mTevTexture2 = value; SignalPropertyChange(); } }
        [Category("TEV")]
        public byte mTevTexture3 { get { return drawSetting.mTevTexture3; } set { drawSetting.mTevTexture3 = value; SignalPropertyChange(); } }
        [Category("TEV")]
        public byte mTevTexture4 { get { return drawSetting.mTevTexture4; } set { drawSetting.mTevTexture4 = value; SignalPropertyChange(); } }

        #region Old

        //#region Color

        //[Category("TEV Color 1")]
        //public ColorArg c1mA { get { return (ColorArg)drawSetting.mTevColor1.mA; } set { drawSetting.mTevColor1.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 1")]
        //public ColorArg c1mB { get { return (ColorArg)drawSetting.mTevColor1.mB; } set { drawSetting.mTevColor1.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 1")]
        //public ColorArg c1mC { get { return (ColorArg)drawSetting.mTevColor1.mC; } set { drawSetting.mTevColor1.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 1")]
        //public ColorArg c1mD { get { return (ColorArg)drawSetting.mTevColor1.mD; } set { drawSetting.mTevColor1.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 2")]
        //public ColorArg c2mA { get { return (ColorArg)drawSetting.mTevColor2.mA; } set { drawSetting.mTevColor2.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 2")]
        //public ColorArg c2mB { get { return (ColorArg)drawSetting.mTevColor2.mB; } set { drawSetting.mTevColor2.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 2")]
        //public ColorArg c2mC { get { return (ColorArg)drawSetting.mTevColor2.mC; } set { drawSetting.mTevColor2.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 2")]
        //public ColorArg c2mD { get { return (ColorArg)drawSetting.mTevColor2.mD; } set { drawSetting.mTevColor2.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 3")]
        //public ColorArg c3mA { get { return (ColorArg)drawSetting.mTevColor3.mA; } set { drawSetting.mTevColor3.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 3")]
        //public ColorArg c3mB { get { return (ColorArg)drawSetting.mTevColor3.mB; } set { drawSetting.mTevColor3.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 3")]
        //public ColorArg c3mC { get { return (ColorArg)drawSetting.mTevColor3.mC; } set { drawSetting.mTevColor3.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 3")]
        //public ColorArg c3mD { get { return (ColorArg)drawSetting.mTevColor3.mD; } set { drawSetting.mTevColor3.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 4")]
        //public ColorArg c4mA { get { return (ColorArg)drawSetting.mTevColor4.mA; } set { drawSetting.mTevColor4.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 4")]
        //public ColorArg c4mB { get { return (ColorArg)drawSetting.mTevColor4.mB; } set { drawSetting.mTevColor4.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 4")]
        //public ColorArg c4mC { get { return (ColorArg)drawSetting.mTevColor4.mC; } set { drawSetting.mTevColor4.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 4")]
        //public ColorArg c4mD { get { return (ColorArg)drawSetting.mTevColor4.mD; } set { drawSetting.mTevColor4.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 1 Operation")]
        //public TevOp c1mOp { get { return (TevOp)drawSetting.mTevColorOp1.mOp; } set { drawSetting.mTevColorOp1.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 1 Operation")]
        //public Bias c1mBias { get { return (Bias)drawSetting.mTevColorOp1.mBias; } set { drawSetting.mTevColorOp1.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 1 Operation")]
        //public TevScale c1mScale { get { return (TevScale)drawSetting.mTevColorOp1.mScale; } set { drawSetting.mTevColorOp1.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 1 Operation")]
        //public bool c1mClamp { get { return drawSetting.mTevColorOp1.mClamp != 0; } set { drawSetting.mTevColorOp1.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Color 1 Operation")]
        //public TevRegID c1mOutReg { get { return (TevRegID)drawSetting.mTevColorOp1.mOutReg; } set { drawSetting.mTevColorOp1.mOutReg = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 2 Operation")]
        //public TevOp c2mOp { get { return (TevOp)drawSetting.mTevColorOp2.mOp; } set { drawSetting.mTevColorOp2.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 2 Operation")]
        //public Bias c2mBias { get { return (Bias)drawSetting.mTevColorOp2.mBias; } set { drawSetting.mTevColorOp2.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 2 Operation")]
        //public TevScale c2mScale { get { return (TevScale)drawSetting.mTevColorOp2.mScale; } set { drawSetting.mTevColorOp2.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 2 Operation")]
        //public bool c2mClamp { get { return drawSetting.mTevColorOp2.mClamp != 0; } set { drawSetting.mTevColorOp2.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Color 2 Operation")]
        //public TevRegID c2mOutReg { get { return (TevRegID)drawSetting.mTevColorOp2.mOutReg; } set { drawSetting.mTevColorOp2.mOutReg = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 3 Operation")]
        //public TevOp c3mOp { get { return (TevOp)drawSetting.mTevColorOp3.mOp; } set { drawSetting.mTevColorOp3.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 3 Operation")]
        //public Bias c3mBias { get { return (Bias)drawSetting.mTevColorOp3.mBias; } set { drawSetting.mTevColorOp3.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 3 Operation")]
        //public TevScale c3mScale { get { return (TevScale)drawSetting.mTevColorOp3.mScale; } set { drawSetting.mTevColorOp3.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 3 Operation")]
        //public bool c3mClamp { get { return drawSetting.mTevColorOp3.mClamp != 0; } set { drawSetting.mTevColorOp3.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Color 3 Operation")]
        //public TevRegID c3mOutReg { get { return (TevRegID)drawSetting.mTevColorOp3.mOutReg; } set { drawSetting.mTevColorOp3.mOutReg = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Color 4 Operation")]
        //public TevOp c4mOp { get { return (TevOp)drawSetting.mTevColorOp4.mOp; } set { drawSetting.mTevColorOp4.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 4 Operation")]
        //public Bias c4mBias { get { return (Bias)drawSetting.mTevColorOp4.mBias; } set { drawSetting.mTevColorOp4.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 4 Operation")]
        //public TevScale c4mScale { get { return (TevScale)drawSetting.mTevColorOp4.mScale; } set { drawSetting.mTevColorOp4.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Color 4 Operation")]
        //public bool c4mClamp { get { return drawSetting.mTevColorOp4.mClamp != 0; } set { drawSetting.mTevColorOp4.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Color 4 Operation")]
        //public TevRegID c4mOutReg { get { return (TevRegID)drawSetting.mTevColorOp4.mOutReg; } set { drawSetting.mTevColorOp4.mOutReg = (byte)value; SignalPropertyChange(); } }

        //#endregion  

        //#region Alpha

        //[Category("TEV Alpha 1")]
        //public ColorArg a1mA { get { return (ColorArg)drawSetting.mTevAlpha1.mA; } set { drawSetting.mTevAlpha1.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 1")]
        //public ColorArg a1mB { get { return (ColorArg)drawSetting.mTevAlpha1.mB; } set { drawSetting.mTevAlpha1.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 1")]
        //public ColorArg a1mC { get { return (ColorArg)drawSetting.mTevAlpha1.mC; } set { drawSetting.mTevAlpha1.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 1")]
        //public ColorArg a1mD { get { return (ColorArg)drawSetting.mTevAlpha1.mD; } set { drawSetting.mTevAlpha1.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 2")]
        //public ColorArg a2mA { get { return (ColorArg)drawSetting.mTevAlpha2.mA; } set { drawSetting.mTevAlpha2.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 2")]
        //public ColorArg a2mB { get { return (ColorArg)drawSetting.mTevAlpha2.mB; } set { drawSetting.mTevAlpha2.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 2")]
        //public ColorArg a2mC { get { return (ColorArg)drawSetting.mTevAlpha2.mC; } set { drawSetting.mTevAlpha2.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 2")]
        //public ColorArg a2mD { get { return (ColorArg)drawSetting.mTevAlpha2.mD; } set { drawSetting.mTevAlpha2.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 3")]
        //public ColorArg a3mA { get { return (ColorArg)drawSetting.mTevAlpha3.mA; } set { drawSetting.mTevAlpha3.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 3")]
        //public ColorArg a3mB { get { return (ColorArg)drawSetting.mTevAlpha3.mB; } set { drawSetting.mTevAlpha3.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 3")]
        //public ColorArg a3mC { get { return (ColorArg)drawSetting.mTevAlpha3.mC; } set { drawSetting.mTevAlpha3.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 3")]
        //public ColorArg a3mD { get { return (ColorArg)drawSetting.mTevAlpha3.mD; } set { drawSetting.mTevAlpha3.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 4")]
        //public ColorArg a4mA { get { return (ColorArg)drawSetting.mTevAlpha4.mA; } set { drawSetting.mTevAlpha4.mA = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 4")]
        //public ColorArg a4mB { get { return (ColorArg)drawSetting.mTevAlpha4.mB; } set { drawSetting.mTevAlpha4.mB = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 4")]
        //public ColorArg a4mC { get { return (ColorArg)drawSetting.mTevAlpha4.mC; } set { drawSetting.mTevAlpha4.mC = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 4")]
        //public ColorArg a4mD { get { return (ColorArg)drawSetting.mTevAlpha4.mD; } set { drawSetting.mTevAlpha4.mD = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 1 Operation")]
        //public TevOp a1mOp { get { return (TevOp)drawSetting.mTevAlphaOp1.mOp; } set { drawSetting.mTevAlphaOp1.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 1 Operation")]
        //public Bias a1mBias { get { return (Bias)drawSetting.mTevAlphaOp1.mBias; } set { drawSetting.mTevAlphaOp1.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 1 Operation")]
        //public TevScale a1mScale { get { return (TevScale)drawSetting.mTevAlphaOp1.mScale; } set { drawSetting.mTevAlphaOp1.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 1 Operation")]
        //public bool a1mClamp { get { return drawSetting.mTevAlphaOp1.mClamp != 0; } set { drawSetting.mTevAlphaOp1.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Alpha 1 Operation")]
        //public TevRegID a1mOutReg { get { return (TevRegID)drawSetting.mTevAlphaOp1.mOutReg; } set { drawSetting.mTevAlphaOp1.mOutReg = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 2 Operation")]
        //public TevOp a2mOp { get { return (TevOp)drawSetting.mTevAlphaOp2.mOp; } set { drawSetting.mTevAlphaOp2.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 2 Operation")]
        //public Bias a2mBias { get { return (Bias)drawSetting.mTevAlphaOp2.mBias; } set { drawSetting.mTevAlphaOp2.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 2 Operation")]
        //public TevScale a2mScale { get { return (TevScale)drawSetting.mTevAlphaOp2.mScale; } set { drawSetting.mTevAlphaOp2.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 2 Operation")]
        //public bool a2mClamp { get { return drawSetting.mTevAlphaOp2.mClamp != 0; } set { drawSetting.mTevAlphaOp2.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Alpha 2 Operation")]
        //public TevRegID a2mOutReg { get { return (TevRegID)drawSetting.mTevAlphaOp2.mOutReg; } set { drawSetting.mTevAlphaOp2.mOutReg = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 3 Operation")]
        //public TevOp a3mOp { get { return (TevOp)drawSetting.mTevAlphaOp3.mOp; } set { drawSetting.mTevAlphaOp3.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 3 Operation")]
        //public Bias a3mBias { get { return (Bias)drawSetting.mTevAlphaOp3.mBias; } set { drawSetting.mTevAlphaOp3.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 3 Operation")]
        //public TevScale a3mScale { get { return (TevScale)drawSetting.mTevAlphaOp3.mScale; } set { drawSetting.mTevAlphaOp3.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 3 Operation")]
        //public bool a3mClamp { get { return drawSetting.mTevAlphaOp3.mClamp != 0; } set { drawSetting.mTevAlphaOp3.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Alpha 3 Operation")]
        //public TevRegID a3mOutReg { get { return (TevRegID)drawSetting.mTevAlphaOp3.mOutReg; } set { drawSetting.mTevAlphaOp3.mOutReg = (byte)value; SignalPropertyChange(); } }

        //[Category("TEV Alpha 4 Operation")]
        //public TevOp a4mOp { get { return (TevOp)drawSetting.mTevAlphaOp4.mOp; } set { drawSetting.mTevAlphaOp4.mOp = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 4 Operation")]
        //public Bias a4mBias { get { return (Bias)drawSetting.mTevAlphaOp4.mBias; } set { drawSetting.mTevAlphaOp4.mBias = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 4 Operation")]
        //public TevScale a4mScale { get { return (TevScale)drawSetting.mTevAlphaOp4.mScale; } set { drawSetting.mTevAlphaOp4.mScale = (byte)value; SignalPropertyChange(); } }
        //[Category("TEV Alpha 4 Operation")]
        //public bool a4mClamp { get { return drawSetting.mTevAlphaOp4.mClamp != 0; } set { drawSetting.mTevAlphaOp4.mClamp = (byte)(value ? 1 : 0); SignalPropertyChange(); } }
        //[Category("TEV Alpha 4 Operation")]
        //public TevRegID a4mOutReg { get { return (TevRegID)drawSetting.mTevAlphaOp4.mOutReg; } set { drawSetting.mTevAlphaOp4.mOutReg = (byte)value; SignalPropertyChange(); } }

        //#endregion

        //[Category("Constant Register Selection")]
        //public TevKColorSel mTevKColorSel1 { get { return (TevKColorSel)drawSetting.mTevKColorSel1; } set { drawSetting.mTevKColorSel1 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKAlphaSel mTevKAlphaSel1 { get { return (TevKAlphaSel)drawSetting.mTevKAlphaSel1; } set { drawSetting.mTevKAlphaSel1 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKColorSel mTevKColorSel2 { get { return (TevKColorSel)drawSetting.mTevKColorSel2; } set { drawSetting.mTevKColorSel2 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKAlphaSel mTevKAlphaSel2 { get { return (TevKAlphaSel)drawSetting.mTevKAlphaSel2; } set { drawSetting.mTevKAlphaSel2 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKColorSel mTevKColorSel3 { get { return (TevKColorSel)drawSetting.mTevKColorSel3; } set { drawSetting.mTevKColorSel3 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKAlphaSel mTevKAlphaSel3 { get { return (TevKAlphaSel)drawSetting.mTevKAlphaSel3; } set { drawSetting.mTevKAlphaSel3 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKColorSel mTevKColorSel4 { get { return (TevKColorSel)drawSetting.mTevKColorSel4; } set { drawSetting.mTevKColorSel4 = (byte)value; SignalPropertyChange(); } }
        //[Category("Constant Register Selection")]
        //public TevKAlphaSel mTevKAlphaSel4 { get { return (TevKAlphaSel)drawSetting.mTevKAlphaSel4; } set { drawSetting.mTevKAlphaSel4 = (byte)value; SignalPropertyChange(); } }

        #endregion

        //BlendMode
        [Category("Blend Mode")]
        public GXBlendMode BlendType { get { return (GXBlendMode)drawSetting.mBlendMode.mType; } set { drawSetting.mBlendMode.mType = (byte)value; SignalPropertyChange(); } }
        [Category("Blend Mode")]
        public BlendFactor SrcFactor { get { return (BlendFactor)drawSetting.mBlendMode.mSrcFactor; } set { drawSetting.mBlendMode.mSrcFactor = (byte)value; SignalPropertyChange(); } }
        [Category("Blend Mode")]
        public BlendFactor DstFactor { get { return (BlendFactor)drawSetting.mBlendMode.mDstFactor; } set { drawSetting.mBlendMode.mDstFactor = (byte)value; SignalPropertyChange(); } }
        [Category("Blend Mode")]
        public GXLogicOp Operation { get { return (GXLogicOp)drawSetting.mBlendMode.mOp; } set { drawSetting.mBlendMode.mOp = (byte)value; SignalPropertyChange(); } }

        //Color
        [Category("Color Input")]
        public SSBBTypes.ColorInput.RasColor RasterColor { get { return (SSBBTypes.ColorInput.RasColor)drawSetting.mColorInput.mRasColor; } set { drawSetting.mColorInput.mRasColor = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevColor1 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevColor1; } set { drawSetting.mColorInput.mTevColor1 = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevColor2 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevColor2; } set { drawSetting.mColorInput.mTevColor2 = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevColor3 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevColor3; } set { drawSetting.mColorInput.mTevColor3 = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevKColor1 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevKColor1; } set { drawSetting.mColorInput.mTevKColor1 = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevKColor2 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevKColor2; } set { drawSetting.mColorInput.mTevKColor2 = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevKColor3 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevKColor3; } set { drawSetting.mColorInput.mTevKColor3 = (byte)value; SignalPropertyChange(); } }
        [Category("Color Input")]
        public SSBBTypes.ColorInput.TevColor TevKColor4 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mColorInput.mTevKColor4; } set { drawSetting.mColorInput.mTevKColor4 = (byte)value; SignalPropertyChange(); } }

        //Alpha
        [Category("Alpha Input")]
        public SSBBTypes.ColorInput.RasColor AlphaRasColor { get { return (SSBBTypes.ColorInput.RasColor)drawSetting.mAlphaInput.mRasColor; } set { drawSetting.mAlphaInput.mRasColor = (byte)value; SignalPropertyChange(); } }
        [Category("Alpha Input")]
        public SSBBTypes.ColorInput.TevColor AlphaTevColor1 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mAlphaInput.mTevColor1; } set { drawSetting.mAlphaInput.mTevColor1 = (byte)value; SignalPropertyChange(); } }
        [Category("Alpha Input")]
        public SSBBTypes.ColorInput.TevColor AlphaTevColor2 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mAlphaInput.mTevColor2; } set { drawSetting.mAlphaInput.mTevColor2 = (byte)value; SignalPropertyChange(); } }
        [Category("Alpha Input")]
        public SSBBTypes.ColorInput.TevColor AlphaTevColor3 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mAlphaInput.mTevColor3; } set { drawSetting.mAlphaInput.mTevColor3 = (byte)value; SignalPropertyChange(); } }
        [Category("Alpha Input")]
        public SSBBTypes.ColorInput.TevColor AlphaTevKColor1 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mAlphaInput.mTevKColor1; } set { drawSetting.mAlphaInput.mTevKColor1 = (byte)value; SignalPropertyChange(); } }
        [Category("Alpha Input")]
        public SSBBTypes.ColorInput.TevColor AlphaTevKColor2 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mAlphaInput.mTevKColor2; } set { drawSetting.mAlphaInput.mTevKColor2 = (byte)value; SignalPropertyChange(); } }
        [Category("Alpha Input")]
        public SSBBTypes.ColorInput.TevColor AlphaTevKColor3 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mAlphaInput.mTevKColor3; } set { drawSetting.mAlphaInput.mTevKColor3 = (byte)value; SignalPropertyChange(); } }
        [Category("Alpha Input")]
        public SSBBTypes.ColorInput.TevColor AlphaTevKColor4 { get { return (SSBBTypes.ColorInput.TevColor)drawSetting.mAlphaInput.mTevKColor4; } set { drawSetting.mAlphaInput.mTevKColor4 = (byte)value; SignalPropertyChange(); } }

        [Category("Draw Settings")]
        public GXCompare ZCompareFunc { get { return (GXCompare)drawSetting.mZCompareFunc; } set { drawSetting.mZCompareFunc = (byte)value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public SSBBTypes.AlphaFlickType AlphaFlickType { get { return (SSBBTypes.AlphaFlickType)drawSetting.mAlphaFlickType; } set { drawSetting.mAlphaFlickType = (byte)value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public ushort AlphaFlickCycle { get { return drawSetting.mAlphaFlickCycle; } set { drawSetting.mAlphaFlickCycle = value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public byte AlphaFlickRandom { get { return drawSetting.mAlphaFlickRandom; } set { drawSetting.mAlphaFlickRandom = value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public byte AlphaFlickAmplitude { get { return drawSetting.mAlphaFlickAmplitude; } set { drawSetting.mAlphaFlickAmplitude = value; SignalPropertyChange(); } }

        //mLighting 
        [Category("Lighting")]
        public SSBBTypes.Lighting.Mode Mode { get { return (SSBBTypes.Lighting.Mode)drawSetting.mLighting.mMode; } set { drawSetting.mLighting.mMode = (byte)value; SignalPropertyChange(); } }
        [Category("Lighting")]
        public SSBBTypes.Lighting.Type LightType { get { return (SSBBTypes.Lighting.Type)drawSetting.mLighting.mType; } set { drawSetting.mLighting.mMode = (byte)value; SignalPropertyChange(); } }
        [Category("Lighting"), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel Ambient { get { return drawSetting.mLighting.mAmbient; } set { drawSetting.mLighting.mAmbient = value; SignalPropertyChange(); } }
        [Category("Lighting"), TypeConverter(typeof(RGBAStringConverter))]
        public RGBAPixel Diffuse { get { return drawSetting.mLighting.mDiffuse; } set { drawSetting.mLighting.mDiffuse = value; SignalPropertyChange(); } }
        [Category("Lighting")]
        public float Radius { get { return drawSetting.mLighting.mRadius; } set { drawSetting.mLighting.mRadius = value; SignalPropertyChange(); } }
        [Category("Lighting"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Position { get { return drawSetting.mLighting.mPosition; } set { drawSetting.mLighting.mPosition = value; SignalPropertyChange(); } }

        //[Category("Draw Settings")]
        //public fixed float mIndTexOffsetMtx[6] { get { return drawSetting.mFlags; } set { drawSetting.mFlags = value; SignalPropertyChange(); } } //2x3 Matrix
        [Category("Draw Settings")]
        public sbyte IndTexScaleExp { get { return drawSetting.mIndTexScaleExp; } set { drawSetting.mIndTexScaleExp = value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public sbyte PivotX { get { return drawSetting.pivotX; } set { drawSetting.pivotX = value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public sbyte PivotY { get { return drawSetting.pivotY; } set { drawSetting.pivotY = value; SignalPropertyChange(); } }
        //[Category("Draw Settings")]
        //public byte padding { get { return drawSetting.padding; } set { drawSetting.padding = value; SignalPropertyChange(); } }
        [Category("Particle Settings")]
        public SSBBTypes.ReffType ParticleType
        {
            get { return (SSBBTypes.ReffType)drawSetting.ptcltype; }
            set
            {
                if (!(ParticleType >= SSBBTypes.ReffType.Stripe && value >= SSBBTypes.ReffType.Stripe))
                    typeOption2._data = 0;

                drawSetting.ptcltype = (byte)value;

                SignalPropertyChange();
                UpdateProperties();
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffBillboardAssist))]
        public string BillboardAssist
        {
            get
            {
                if (ParticleType == SSBBTypes.ReffType.Billboard)
                    return ((SSBBTypes.BillboardAssist)drawSetting.typeOption).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType == SSBBTypes.ReffType.Billboard && !String.IsNullOrEmpty(value))
                {
                    drawSetting.typeOption = (byte)(SSBBTypes.BillboardAssist)Enum.Parse(typeof(SSBBTypes.BillboardAssist), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffStripeAssist))]
        public string StripeAssist
        {
            get
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe)
                    return ((SSBBTypes.StripeAssist)drawSetting.typeOption).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe && !String.IsNullOrEmpty(value))
                {
                    drawSetting.typeOption = (byte)(SSBBTypes.StripeAssist)Enum.Parse(typeof(SSBBTypes.StripeAssist), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffAssist))]
        public string Assist
        {
            get
            {
                if (ParticleType != SSBBTypes.ReffType.Billboard)
                    return ((SSBBTypes.Assist)drawSetting.typeOption).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType != SSBBTypes.ReffType.Billboard && !String.IsNullOrEmpty(value))
                {
                    drawSetting.typeOption = (byte)(SSBBTypes.Assist)Enum.Parse(typeof(SSBBTypes.Assist), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffBillboardDirection))]
        public string BillboardDirection
        {
            get
            {
                if (ParticleType == SSBBTypes.ReffType.Billboard)
                    return ((SSBBTypes.BillboardAhead)drawSetting.typeDir).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType == SSBBTypes.ReffType.Billboard && !String.IsNullOrEmpty(value))
                {
                    drawSetting.typeDir = (byte)(SSBBTypes.BillboardAhead)Enum.Parse(typeof(SSBBTypes.BillboardAhead), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffDirection))]
        public string Direction
        {
            get
            {
                if (ParticleType != SSBBTypes.ReffType.Billboard)
                    return ((SSBBTypes.Ahead)drawSetting.typeOption).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType != SSBBTypes.ReffType.Billboard && !String.IsNullOrEmpty(value))
                {
                    drawSetting.typeOption = (byte)(SSBBTypes.Ahead)Enum.Parse(typeof(SSBBTypes.Ahead), value);
                    SignalPropertyChange();
                }
            }
        }

        [Category("Particle Settings")]
        public SSBBTypes.RotateAxis TypeAxis { get { return (SSBBTypes.RotateAxis)drawSetting.typeAxis; } set { drawSetting.typeAxis = (byte)value; SignalPropertyChange(); } }

        private Bin8 typeOption2;

        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffStripeConnect))]
        public string StripeConnect
        {
            get
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe)
                    return ((SSBBTypes.StripeConnect)typeOption2[0, 3]).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe && !String.IsNullOrEmpty(value))
                {
                    typeOption2[0, 3] = (byte)(SSBBTypes.StripeConnect)Enum.Parse(typeof(SSBBTypes.StripeConnect), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffStripeInitialPrevAxis))]
        public string StripeInitialPrevAxis
        {
            get
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe)
                    return ((SSBBTypes.StripeInitialPrevAxis)typeOption2[3, 3]).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe && !String.IsNullOrEmpty(value))
                {
                    typeOption2[3, 3] = (byte)(SSBBTypes.StripeInitialPrevAxis)Enum.Parse(typeof(SSBBTypes.StripeInitialPrevAxis), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffStripeTexmapType))]
        public string StripeTexmapType
        {
            get
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe)
                    return ((SSBBTypes.StripeTexmapType)typeOption2[6, 1]).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe && !String.IsNullOrEmpty(value))
                {
                    typeOption2[6, 1] = (byte)(SSBBTypes.StripeTexmapType)Enum.Parse(typeof(SSBBTypes.StripeTexmapType), value);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffDirectionalPivot))]
        public string DirectionalPivot
        {
            get
            {
                if (ParticleType == SSBBTypes.ReffType.Directional)
                    return ((SSBBTypes.DirectionalPivot)typeOption2._data).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType == SSBBTypes.ReffType.Directional && !String.IsNullOrEmpty(value))
                {
                    typeOption2._data = (byte)(SSBBTypes.DirectionalPivot)Enum.Parse(typeof(SSBBTypes.StripeTexmapType), value);
                    SignalPropertyChange();
                }
            }
        }

        [Category("Particle Settings")]
        public string DirectionalChangeYBySpeed
        {
            get
            {
                if (ParticleType == SSBBTypes.ReffType.Directional)
                    return (drawSetting.typeOption0 != 0).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType == SSBBTypes.ReffType.Directional && !String.IsNullOrEmpty(value))
                {
                    bool b;
                    bool.TryParse(value, out b);
                    drawSetting.typeOption0 = (byte)(b ? 1 : 0);
                    SignalPropertyChange();
                }
            }
        }
        [Category("Particle Settings")]
        public string StripeTubeVertexCount
        {
            get
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe)
                    return drawSetting.typeOption0.ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType >= SSBBTypes.ReffType.Stripe && !String.IsNullOrEmpty(value))
                {
                    byte b;
                    byte.TryParse(value, out b);
                    if (b >= 3)
                        drawSetting.typeOption0 = b;
                    SignalPropertyChange();
                }
            }
        }

        [Category("Particle Settings"), TypeConverter(typeof(DropDownListReffDirectionalFace))]
        public string DirectionalFace
        {
            get
            {
                if (ParticleType == SSBBTypes.ReffType.Directional)
                    return ((SSBBTypes.Face)drawSetting.typeOption1).ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType == SSBBTypes.ReffType.Directional && !String.IsNullOrEmpty(value))
                {
                    drawSetting.typeOption1 = (byte)(SSBBTypes.Face)Enum.Parse(typeof(SSBBTypes.Face), value);
                    SignalPropertyChange();
                }
            }
        }

        [Category("Particle Settings")]
        public string StripeInterpDivisionCount
        {
            get
            {
                if (ParticleType == SSBBTypes.ReffType.SmoothStripe)
                    return drawSetting.typeOption1.ToString();
                else
                    return "";
            }
            set
            {
                if (ParticleType == SSBBTypes.ReffType.SmoothStripe && !String.IsNullOrEmpty(value))
                {
                    byte b;
                    byte.TryParse(value, out b);
                    if (b >= 1)
                        drawSetting.typeOption1 = b;
                    SignalPropertyChange();
                }
            }
        }

        //[Category("Draw Settings")]
        //public byte padding4 { get { return drawSetting.padding4; } set { drawSetting.padding4 = value; SignalPropertyChange(); } }
        [Category("Draw Settings")]
        public float ZOffset { get { return drawSetting.zOffset; } set { drawSetting.zOffset = value; SignalPropertyChange(); } }

        #endregion

        EmitterDrawSetting9 drawSetting;

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _name = "Emitter";

            desc = *Descriptor;
            drawSetting = desc.drawSetting;
            typeOption2 = new Bin8(drawSetting.typeOption2);

            return TevStageCount > 0;
        }

        public override void OnPopulate()
        {
            int col1 = 0;
            int colop1 = col1 + 16;
            int alpha1 = colop1 + 20;
            int alphaop1 = alpha1 + 16;
            int csel1 = alphaop1 + 20;
            for (int i = 0; i < 4; i++)
            {
                REFFTEVStage s = new REFFTEVStage(i);

                byte* p = (byte*)drawSetting.mTevColor1.Address;
                {
                    s.kcsel = p[csel1 + i];
                    s.kasel = p[csel1 + 4 + i];

                    s.cseld = p[col1 + 4 * i + 3];
                    s.cselc = p[col1 + 4 * i + 2];
                    s.cselb = p[col1 + 4 * i + 1];
                    s.csela = p[col1 + 4 * i + 0];

                    s.cop = p[colop1 + 5 * i + 0];
                    s.cbias = p[colop1 + 5 * i + 1];
                    s.cshift = p[colop1 + 5 * i + 2];
                    s.cclamp = p[colop1 + 5 * i + 3];
                    s.cdest = p[colop1 + 5 * i + 4];

                    s.aseld = p[alpha1 + 4 * i + 3];
                    s.aselc = p[alpha1 + 4 * i + 2];
                    s.aselb = p[alpha1 + 4 * i + 1];
                    s.asela = p[alpha1 + 4 * i + 0];

                    s.aop = p[alphaop1 + 5 * i + 0];
                    s.abias = p[alphaop1 + 5 * i + 1];
                    s.ashift = p[alphaop1 + 5 * i + 2];
                    s.aclamp = p[alphaop1 + 5 * i + 3];
                    s.adest = p[alphaop1 + 5 * i + 4];
                }

                s.ti = 0;
                s.tc = 0;
                s.cc = 0;
                s.te = false;

                s.Parent = this;
            }
        }

        public override int OnCalculateSize(bool force)
        {
            return 0x14C;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            EmitterDesc* hdr = (EmitterDesc*)address;
            *hdr = desc;
            hdr->drawSetting = drawSetting;
            hdr->drawSetting.typeOption2 = typeOption2._data;
            int col1 = 0;
            int colop1 = col1 + 16;
            int alpha1 = colop1 + 20;
            int alphaop1 = alpha1 + 16;
            int csel1 = alphaop1 + 20;
            for (int i = 0; i < 4; i++)
            {
                REFFTEVStage s = (REFFTEVStage)Children[i];

                byte* p = (byte*)hdr->drawSetting.mTevColor1.Address;
                {
                    p[csel1 + i] = (byte)s.kcsel;
                    p[csel1 + 4 + i] = (byte)s.kasel;

                    p[col1 + 4 * i + 3] = (byte)s.cseld;
                    p[col1 + 4 * i + 2] = (byte)s.cselc;
                    p[col1 + 4 * i + 1] = (byte)s.cselb;
                    p[col1 + 4 * i + 0] = (byte)s.csela;

                    p[colop1 + 5 * i + 0] = (byte)s.cop;
                    p[colop1 + 5 * i + 1] = (byte)s.cbias;
                    p[colop1 + 5 * i + 2] = (byte)s.cshift;
                    p[colop1 + 5 * i + 3] = (byte)s.cclamp;
                    p[colop1 + 5 * i + 4] = (byte)s.cdest;

                    p[alpha1 + 4 * i + 3] = (byte)s.aseld;
                    p[alpha1 + 4 * i + 2] = (byte)s.aselc;
                    p[alpha1 + 4 * i + 1] = (byte)s.aselb;
                    p[alpha1 + 4 * i + 0] = (byte)s.asela;

                    p[alphaop1 + 5 * i + 0] = (byte)s.aop;
                    p[alphaop1 + 5 * i + 1] = (byte)s.abias;
                    p[alphaop1 + 5 * i + 2] = (byte)s.ashift;
                    p[alphaop1 + 5 * i + 3] = (byte)s.aclamp;
                    p[alphaop1 + 5 * i + 4] = (byte)s.adest;
                }
            }
        }
    }
}
