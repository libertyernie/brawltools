using BrawlLib.SSBBTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class ItmFreqNode : ARCEntryNode
    {
        public override ResourceType ResourceType { get { return ResourceType.U8Folder; } }
        internal ItmFreqHeader* Header { get { return (ItmFreqHeader*)WorkingUncompressed.Address; } }
        internal ItmFreqTableList* TList
        {
            get
            {
                return (ItmFreqTableList*)(WorkingUncompressed.Address + Header->_DataLength - 0x08);
            }
        }
        ItmFreqOffPair _t1, _t2, _t3, _t4, _t5;

        // Node cache
        List<TableNode> _tables = new List<TableNode>();
        List<bint> _pointerList = new List<bint>();

        // Source cache
        List<DataSource> _s_tables = new List<DataSource>();
        // Header variables
        public bint
            _dataLength,
            _fileSize,
            _DTableCount,
            _offCount = 0;

        // Public variables
        public VoidPtr BaseAddress;
        public int _numTables;

        public override bool OnInitialize()
        {
            base.OnInitialize();

            BaseAddress = (VoidPtr)Header + 0x20;
            _dataLength = Header->_DataLength;
            _fileSize = Header->_Length;
            _DTableCount = Header->_DataTable;
            _offCount = Header->_OffCount;

            _t1 = TList->_table1;
            _t2 = TList->_table2;
            _t3 = TList->_table3;
            _t4 = TList->_table4;
            _t5 = TList->_table5;

            for (int i = 0; i < 5; i++)
                if (TList->Entries[i]._count > 0)
                    _numTables++;
            
                Name = "Item Generation";
            return _numTables > 0;
        }
        public override void OnPopulate()
        {
            for (int i = 0; i < 5; i++)
            {
                if (TList->Entries[i]._count > 0)
                {
                    ItmFreqOffPair* table = (ItmFreqOffPair*)(TList + (i * 8));
                    _tables.Add(new TableNode());
                    _s_tables.Add(new DataSource(table, 0x08));
                    _tables[i].Initialize(this, _s_tables[i]);
                }
            }

            foreach (TableNode t in _tables)
                for (int i = 0; i < t.Count; i++)
                {
                    ItmFreqGroup* group = (ItmFreqGroup*)(BaseAddress + t.Offset + (i * 0x14));
                    DataSource GroupSource = new DataSource(group, 0x14);
                    TableGroupNode GroupNode = new TableGroupNode();
                    GroupNode.Initialize(t, GroupSource);

                    for (int b = 0; b < group->_entryCount; b++)
                    {
                        ItmFreqEntry* entry = (ItmFreqEntry*)(BaseAddress + (group->_entryOffset + (b * 0x10)));
                        ItmFreqEntryNode EntryNode = new ItmFreqEntryNode();
                        DataSource EntrySource = new DataSource(entry, 0x10);
                        EntryNode.Initialize(GroupNode, EntrySource);
                    }
                }
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            BaseAddress = (VoidPtr)address + 0x20;

            ItmFreqHeader* Header = (ItmFreqHeader*)address;               
            *Header = new ItmFreqHeader();
            Header->_Length = _fileSize;
            Header->_DataTable = _DTableCount;
            Header->_OffCount = _offCount;
            Header->_DataLength = _dataLength;
            Header->_pad0 = Header->_pad1 =
            Header->_pad2 = Header->_pad3 = 0;

            ItmFreqTableList* TList = (ItmFreqTableList*)(address + Header->_DataLength - 0x08);

            for (int i = 0; i < Children.Count; i++)
                Children[i].Rebuild(TList + (i*8), 0x08, force);
        }
        public override int OnCalculateSize(bool force)
        {
            int size = ItmFreqHeader.Size;
            foreach (TableNode node in Children)
                size += node.CalculateSize(force);
            return size;
        }

        internal static ResourceNode TryParse(DataSource source) { return ((ItmFreqHeader*)source.Address)->_Length == source.Length && ((ItmFreqHeader*)source.Address)->Str == "genParamSet" ? new ItmFreqNode() : null; }
    }

    public unsafe class TableNode : ItmFreqBaseNode
    {
        internal ItmFreqOffPair* Header { get { return (ItmFreqOffPair*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.U8Folder; } }
        private int _entryOffset;
        public int Offset { get { return _entryOffset; } set { _entryOffset = value; SignalPropertyChange(); } }

        private int _count;
        public int Count { get { return _count; } set { _count = value; SignalPropertyChange(); } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _entryOffset = Header->_offset;
            _count = Header->_count;
            Name = "Table[" + Parent.Children.IndexOf(this) + "]";
            return true;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            ItmFreqOffPair* Header = (ItmFreqOffPair*)address;
            *Header = new ItmFreqOffPair();
            Header->_offset = _entryOffset;
            Header->_count = _count;

            for (int i = 0; i < Children.Count; i++)
                Children[i].Rebuild(BaseAddress + Header->_offset + (i*0x14), 0x14, force);
        }
        public override int OnCalculateSize(bool force)
        {
            int size = ItmFreqOffPair.Size;
            foreach (TableGroupNode node in Children)
                size += node.CalculateSize(force);
            return size;
        }
    }
    public unsafe class TableGroupNode : ItmFreqBaseNode
    {
        internal ItmFreqGroup* Header { get { return (ItmFreqGroup*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.U8Folder; } }

        private bint _unk3;
        public bint Unknown3 { get { return _unk3; } set { _unk3 = value; SignalPropertyChange(); } }

        private bint _unk2;
        public bint Unknown2 { get { return _unk2; } set { _unk2 = value; SignalPropertyChange(); } }

        private bint _unk1;
        public bint Unknown1 { get { return _unk1; } set { _unk1 = value; SignalPropertyChange(); } }

        private bint _unk0;
        public bint Unknown0 { get { return _unk0; } set { _unk0 = value; SignalPropertyChange(); } }

        private bint _entryOffset;
        public bint Offset { get { return _entryOffset; } set { _entryOffset = value; SignalPropertyChange(); } }

        private bint _count;
        public bint Count { get { return _count; } set { _count = value; SignalPropertyChange(); } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            Name = "Group[" + Parent.Children.IndexOf(this) + "]";
            _unk0 = Header->_unknown0;
            _unk1 = Header->_unknown1;
            _unk2 = Header->_unknown2;
            _unk3 = Header->_unknown3;
            _entryOffset = Header->_entryOffset;
            _count = Header->_entryCount;

            return Header->_entryCount > 0;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            ItmFreqGroup* Header = (ItmFreqGroup*)address;
            *Header = new ItmFreqGroup();
            Header->_entryCount = _count;
            Header->_entryOffset = _entryOffset;
            Header->_unknown0 = _unk0;
            Header->_unknown1 = _unk1;
            Header->_unknown2 = _unk2;
            Header->_unknown3 = _unk3;

            for (int i = 0; i < Children.Count; i++)
                Children[i].Rebuild(BaseAddress + Header->_entryOffset + (i*0x10), 0x10, force);
        }
        public override int OnCalculateSize(bool force)
        {
            int size = ItmFreqGroup.Size;
            foreach (ItmFreqEntryNode node in Children)
                size += node.CalculateSize(force);
            return size;
        }
    }
    public unsafe class ItmFreqEntryNode : ItmFreqBaseNode
    {
        internal ItmFreqEntry* Header { get { return (ItmFreqEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        //internal int _id;
        //[DisplayName("Item ID")]
        //[Category("Item")]
        //public int ID { get { return _id; } set { _id = value; SignalPropertyChange(); } }
        internal int _id;
        [Category("Item")]
        [DisplayName("Item ID")]
        [Description("The ID of the item to spawn.")]
        [TypeConverter(typeof(DropDownListItemIDs))]
        public string ItemID
        {
            get
            {
                if (Header->_ID < 0) return "N/A";
                Item item = Item.Items.Where(s => s.ID == Header->_ID).FirstOrDefault();
                return _id.ToString("X2") + (item == null ? "" : (" - " + item.Name));
            }
            set
            {
                // Don't try to set the stage ID if it's not a stage module
                if (ItemID == null) return;
                if (value.Length < 3) return;
                _id = int.Parse(value.Substring(0, 3), System.Globalization.NumberStyles.HexNumber);
                SignalPropertyChange();
            }
        }

        internal int _subID;
        [DisplayName("Sub Item ID")]
        [Category("Item")]
        [Description("Seems to be sub-item to spawn from initial item. (e.x Barrel/Crate skin)")]
        public int SubID { get { return _subID; } set { _subID = value; SignalPropertyChange(); } }

        internal float _frequency;
        [DisplayName("Frequency")]
        [Category("Item Frequency")]
        [Description("The spawn frequency of the selected item. Higher values mean a higher spawn rate.")]
        public string Frequency { get { return _frequency.ToString("0.00"); } set { _frequency = float.Parse(value); SignalPropertyChange(); } }

        internal short _action;
        [DisplayName("Start Action")]
        [Category("Item")]
        [Description("Possible the spawning action of the item.")]
        public short Action { get { return _action; } set { _action = value; SignalPropertyChange(); } }

        internal short _subaction;
        [DisplayName("Start Subaction")]
        [Category("Item")]
        [Description("Possible the spawning subaction of the item.")]
        public short Subaction { get { return _subaction; } set { _subaction = value; SignalPropertyChange(); } }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            _id = Header->_ID;
            _subID = Header->_subItem;
            _frequency = Header->_frequency;
            _action = Header->_action;
            _subaction = Header->_subaction;

            Name = ItemID;
            return false;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            ItmFreqEntry* Header = (ItmFreqEntry*)address;
            *Header = new ItmFreqEntry();
            Header->_action = _action;
            Header->_frequency = _frequency;
            Header->_ID = _id;
            Header->_subaction = _subaction;
            Header->_subItem = _subID;
        }
        public override int OnCalculateSize(bool force)
        {
            int size = ItmFreqEntry.Size;
            return size;
        }

    }

    public unsafe class ItmFreqBaseNode : ResourceNode
    {
        [Browsable(false)]
        public ItmFreqNode Root
        {
            get
            {
                ResourceNode n = _parent;
                while (!(n is ItmFreqNode) && (n != null))
                    n = n._parent;
                return n as ItmFreqNode;
            }
        }
        [Browsable(false)]
        public VoidPtr Data { get { return (VoidPtr)WorkingUncompressed.Address; } }
        [Browsable(false)]
        public VoidPtr BaseAddress
        {
            get
            {
                if (Root == null)
                    return 0;
                return Root.BaseAddress;
            }
        }
        [Browsable(false)]
        public int _offset { get { if (Data != null) return (int)Data - (int)BaseAddress; else return 0; } }
    }
}
