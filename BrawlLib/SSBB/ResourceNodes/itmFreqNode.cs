using BrawlLib.SSBBTypes;
using System;
using System.ComponentModel;
using System.Collections.Generic;
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


        public List<ItmFreq> _entries;
        public List<TableGroupNode> _groups;
        public List<TableNode> _tables;
        public List<bint> _pointerList;

        public TableListNode _tablelist;

        public override bool OnInitialize()
        {
            base.OnInitialize();
            Name = "Item Generation";
            _tablelist = new TableListNode();

            for (int i = 0; i < 5; i++)
                _tables.Add(new TableNode());
            for (int i = 0; i < _tables.Count; i++)
                _groups.Add(new TableGroupNode());
            foreach (TableGroupNode g in _groups)
                for (int i = 0; i < g.Count; i++)
                    _entries.Add(new ItmFreq());

            foreach (ItmFreq itm in _entries)
                itm.Initialize(null, new DataSource());

                return _tables.Count > 0;
        }
    }

    //public unsafe class ItmFreqNode : ARCEntryNode
    //{
    //    public override ResourceType ResourceType { get { return ResourceType.U8Folder; } }
    //    internal ItmFreqHeader* Header { get { return (ItmFreqHeader*)WorkingUncompressed.Address; } }

    //    public override bool OnInitialize()
    //    {
    //        base.OnInitialize();
    //        Name = "Item Generation";
    //        return true;
    //    }
    //    public override void OnPopulate()
    //    {
    //        List<DataSource> GroupSources = new List<DataSource>();
    //        List<DataSource> FreqEntrySources = new List<DataSource>();
    //        List<DataSource> TableSources = new List<DataSource>();

    //        ItmFreqTableList* _TList = (ItmFreqTableList*)(WorkingUncompressed.Address + Header->_DataLength - 0x08);
    //        DataSource _s_TableList = new DataSource(_TList, 0x28);
    //        TableListNode TableList = new TableListNode();
    //        TableList.Initialize(this, _s_TableList);

    //    }
    //    public override int OnCalculateSize(bool force)
    //    {
    //        int size = ItmFreqHeader.Size;
    //        foreach (ResourceNode node in Children)
    //            size += node.CalculateSize(force);
    //        return size;
    //    }

    //    internal static ResourceNode TryParse(DataSource source) { return ((ItmFreqHeader*)source.Address)->_Length == source.Length && ((ItmFreqHeader*)source.Address)->Str == "genParamSet" ? new ItmFreqNode() : null; }
    //}
    public unsafe class TableListNode : ResourceNode
    {
        internal ItmFreqTableList* Header { get { return (ItmFreqTableList*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.U8Folder; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            Name = "Frequency Tables";
            return true;
        }
        public override void OnPopulate()
        {
            for (int i = 0; i < 5; i++)
            {
                ItmFreqOffPair* entry = (ItmFreqOffPair*)WorkingUncompressed.Address + i;
                if (entry->_offset != 0 && entry->_count != 0)
                {
                    DataSource source = new DataSource(entry, 0x08);
                    new TableNode().Initialize(this, source);
                }
            }
        }
        public override int OnCalculateSize(bool force)
        {
            int size = ItmFreqTableList.Size;
            foreach (ResourceNode node in Children)
                size += node.CalculateSize(force);
            return size;
        }
    }
    public unsafe class TableNode : ResourceNode
    {
        internal ItmFreqOffPair* Header { get { return (ItmFreqOffPair*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.U8Folder; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            Name = "Table[" + Parent.Children.IndexOf(this) + "]";
            return true;
        }
        public override void OnPopulate()
        {
            for (int i = 0; i < Header->_count; i++)
            {
                VoidPtr addr = Parent.Parent.WorkingUncompressed.Address;
                VoidPtr off = Header->_offset + 0x20;

                ItmFreqGroupNode* entry = (ItmFreqGroupNode*)(addr + off + (i * 0x14));
                DataSource source = new DataSource(entry, 0x14);
                new TableGroupNode().Initialize(this, source);
            }
        }
        public override int OnCalculateSize(bool force)
        {
            int size = ItmFreqOffPair.Size;
            foreach (ResourceNode node in Children)
                size += node.CalculateSize(force);
            return size;
        }
    }
    public unsafe class TableGroupNode : ResourceNode
    {
        internal ItmFreqGroupNode* Header { get { return (ItmFreqGroupNode*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.U8Folder; } }
        private bint _offset;
        public bint Ofsset { get { return _offset; } set { _offset = value; SignalPropertyChange(); } }

        private bint _count;
        public bint Count { get { return _count; } set { _count = value; SignalPropertyChange(); } }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            Name = "Group[" + Parent.Children.IndexOf(this) + "]";
            _offset = Header->_entryOffset;
            _count = Header->_entryCount;
            return true;
        }
        public override void OnPopulate()
        {
            for (int i = 0; i < Header->_entryCount; i++)
            {
                VoidPtr addr = Parent.Parent.Parent.WorkingUncompressed.Address;

                ItmFreqEntry* Entry = (ItmFreqEntry*)(addr + Header->_entryOffset + 0x20 + (i * 0x10));
                DataSource source = new DataSource(Entry, 0x10);
                new ItmFreq().Initialize(this, source);
            }
        }
        public override int OnCalculateSize(bool force)
        {
            int size = ItmFreqGroupNode.Size;
            foreach (ResourceNode node in Children)
                size += node.CalculateSize(force);
            return size;
        }
    }
    public unsafe class ItmFreq : ResourceNode
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

            Name = "Item[" + Parent.Children.IndexOf(this) + "]";
            return false;
        }
        public override int OnCalculateSize(bool force)
        {
            int size = ItmFreqEntry.Size;
            return size;
        }

    }
}
