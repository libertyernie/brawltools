using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class SCLANode : ARCEntryNode
    {
        internal SCLA* Header { get { return (SCLA*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.SCLA; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            //if (_name == null)
                //_name = "Stage Collision Attributes";

            return Header->_count > 0;
        }

        const int _entrySize = 0x54;

        public override void OnPopulate()
        {
            for (int i = 0; i < Header->_count; i++)
                new SCLAEntryNode().Initialize(this, new DataSource((*Header)[i], _entrySize));
        }
        
        protected override string GetName() {
            return base.GetName("Stage Collision Attributes");
        }

        public override int OnCalculateSize(bool force)
        {
            return 0x10 + Children.Count * 4 + Children.Count * _entrySize;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            SCLA* header = (SCLA*)address;
            *header = new SCLA(Children.Count);
            uint offset = (uint)(0x10 + (Children.Count * 4));
            for (int i = 0; i < Children.Count; i++)
            {
                ResourceNode r = Children[i];
                *(buint*)((VoidPtr)address + 0x10 + i * 4) = offset;
                r.Rebuild((VoidPtr)address + offset, _entrySize, true);
                offset += _entrySize;
            }
        }

        internal static ResourceNode TryParse(DataSource source) { return ((SCLA*)source.Address)->_tag == SCLA.Tag ? new SCLANode() : null; }
    }

    public unsafe class SCLAEntryNode : ResourceNode
    {
        internal SCLAEntry* Header { get { return (SCLAEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        [Category("SCLA Entry")]
        public uint ID { get { return _index; } set { _index = value; SignalPropertyChange(); } }
        [Category("SCLA Entry")]
        public float Traction { get { return _unk1; } set { _unk1 = value; SignalPropertyChange(); } }
        [Category("SCLA Entry")]
        public uint HitDataSet { get { return _unk2; } set { _unk2 = value; SignalPropertyChange(); } }
        [Category("SCLA Entry"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public SCLASubEntryClass SubEntry1 { get { return _sub1; } set { _sub1 = value; SignalPropertyChange(); } }
        [Category("SCLA Entry"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public SCLASubEntryClass SubEntry2 { get { return _sub2; } set { _sub2 = value; SignalPropertyChange(); } }
        [Category("SCLA Entry"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public SCLASubEntryClass SubEntry3 { get { return _sub3; } set { _sub3 = value; SignalPropertyChange(); } }
        
        public uint _index;
        public float _unk1;
        public uint _unk2;
        SCLASubEntryClass _sub1, _sub2, _sub3;

        public override bool OnInitialize()
        {
            //_name = "Entry" + Index;
            generateSCLAEntryName();
            _index = Header->_index;
            _unk1 = Header->_unk1;
            _unk2 = Header->_unk2;
            _sub1 = Header->_entry1;
            _sub2 = Header->_entry2;
            _sub3 = Header->_entry3;

            _sub1._parent =
            _sub2._parent =
            _sub3._parent = this;

            return false;
        }
        
        private void generateSCLAEntryName() {
            switch (Index)
            {
                case 0:
                    _name = "Basic";
                    break;
                case 1:
                    _name = "Rock";
                    break;
                case 2:
                    _name = "Grass";
                    break;
                case 3:
                    _name = "Soil";
                    break;
                case 4:
                    _name = "Wood";
                    break;
                case 5:
                    _name = "LightIron";
                    break;
                case 6:
                    _name = "Iron";
                    break;
                case 7:
                    _name = "Carpet";
                    break;
                case 8:
                    _name = "Alien";
                    break;
                case 9:
                    _name = "Bulborb";
                    break;
                case 10:
                    _name = "Water";
                    break;
                case 11:
                    _name = "Rubber";
                    break;
                case 12:
                    _name = "Ice";
                    break;
                case 13:
                    _name = "Snow";
                    break;
                case 14:
                    _name = "SnowIce";
                    break;
                case 15:
                    _name = "GameWatch";
                    break;
                case 16:
                    _name = "Ice2";
                    break;
                case 17:
                    _name = "Checkered";
                    break;
                case 18:
                    _name = "SpikesTargetTestOnly";
                    break;
                case 19:
                    _name = "Crash2";
                    break;
                case 20:
                    _name = "Crash3";
                    break;
                case 21:
                    _name = "LargeBubbles";
                    break;
                case 22:
                    _name = "Cloud";
                    break;
                case 23:
                    _name = "Subspace";
                    break;
                case 24:
                    _name = "Stone";
                    break;
                case 25:
                    _name = "Unknown1";
                    break;
                case 26:
                    _name = "NES8Bit";
                    break;
                case 27:
                    _name = "Grate";
                    break;
                case 28:
                    _name = "Sand";
                    break;
                case 29:
                    _name = "Homerun";
                    break;
                case 30:
                    _name = "WaterNoSplash";
                    break;
                case 31:
                    _name = "Unknown2";
                    break;
                default:
                    _name = "Entry" + Index;
                    break;
            }

        }

        public override int OnCalculateSize(bool force)
        {
            return 0x54;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            SCLAEntry* hdr = (SCLAEntry*)address;
            hdr->_index = _index;
            hdr->_unk1 = _unk1;
            hdr->_unk2 = _unk2;
            hdr->_entry1 = _sub1;
            hdr->_entry2 = _sub2;
            hdr->_entry3 = _sub3;
        }

        public unsafe class SCLASubEntryClass
        {
            public SCLAEntryNode _parent;

            [Category("SCLA Sub Entry")]
            public byte Unk1 { get { return _unk1; } set { _unk1 = value; _parent.SignalPropertyChange(); } }
            [Category("SCLA Sub Entry")]
            public byte Unk2 { get { return _unk2; } set { _unk2 = value; _parent.SignalPropertyChange(); } }
            [Category("SCLA Sub Entry")]
            public ushort Unk3 { get { return _unk3; } set { _unk3 = value; _parent.SignalPropertyChange(); } }
            [Category("SCLA Sub Entry")]
            public uint Unk4 { get { return _unk4; } set { _unk4 = value; _parent.SignalPropertyChange(); } }
            [Category("SCLA Sub Entry")]
            public int Index1 { get { return _index1; } set { _index1 = value; _parent.SignalPropertyChange(); } }
            [Category("SCLA Sub Entry")]
            public int Index2 { get { return _index2; } set { _index2 = value; _parent.SignalPropertyChange(); } }
            [Category("SCLA Sub Entry")]
            public int Index3 { get { return _index3; } set { _index3 = value; _parent.SignalPropertyChange(); } }
            [Category("SCLA Sub Entry")]
            public int Index4 { get { return _index4; } set { _index4 = value; _parent.SignalPropertyChange(); } }

            public byte _unk1;
            public byte _unk2;
            public ushort _unk3;
            public uint _unk4;
            public int _index1;
            public int _index2;
            public int _index3;
            public int _index4;

            public SCLASubEntryClass(SCLASubEntry e)
            {
                _unk1 = e._unk1;
                _unk2 = e._unk2;
                _unk3 = e._unk3;
                _unk4 = e._unk4;
                _index1 = e._index1;
                _index2 = e._index2;
                _index3 = e._index3;
                _index4 = e._index4;
            }

            public override string ToString()
            {
                return string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", _unk1, _unk2, _unk3, _unk4, _index1, _index2, _index3, _index4);
            }

            public static implicit operator SCLASubEntry(SCLASubEntryClass val) { return new SCLASubEntry(val); }
            public static implicit operator SCLASubEntryClass(SCLASubEntry val) { return new SCLASubEntryClass(val); }
        }
    }
}
