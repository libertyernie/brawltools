using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.Runtime.InteropServices;
using BrawlLib.Imaging;
using BrawlLib.Wii;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class ItmFreqNode : ARCEntryNode
    {
        public override ResourceType ResourceType { get { return ResourceType.U8Folder; } }
        internal ItmFreqHeader* Header { get { return (ItmFreqHeader*)WorkingUncompressed.Address; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            return true;
        }
        public override void OnPopulate()
        {
            ItmFreqTableList* tList = (ItmFreqTableList*)(WorkingUncompressed.Address + Header->_DataLength - 0x08);
            DataSource source = new DataSource(tList, 0x28);
            new TableListNode().Initialize(this, source);
        }
        //public override int OnCalculateSize(bool force)
        //{
        //    int size = ItmFreqHeader.Size;
        //    foreach (ResourceNode node in Children)
        //        size += node.CalculateSize(force);
        //    return size;
        //}
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            BLOC* header = (BLOC*)address;
            *header = new BLOC(Children.Count);
            uint offset = (uint)(0x10 + (Children.Count * 4));
            for (int i = 0; i < Children.Count; i++)
            {
                if (i > 0) { offset += (uint)(Children[i - 1].CalculateSize(false)); }
                *(buint*)((VoidPtr)address + 0x10 + i * 4) = offset;
                _children[i].Rebuild((VoidPtr)address + offset, _children[i].CalculateSize(false), true);
            }
        }

        internal static ResourceNode TryParse(DataSource source) { return ((ItmFreqHeader*)source.Address)->_Length == source.Length &&  ((ItmFreqHeader*)source.Address)->_DataTable == 1 ? new ItmFreqNode() : null; }
    }

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
                    DataSource _tEntry = new DataSource(entry, 0x08);
                    new TableNode().Initialize(this, _tEntry);
                }
            }
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

                ItmFreqGroupNode* entry = (ItmFreqGroupNode*)(addr + off + (i*0x14));
                    DataSource _group = new DataSource(entry, 0x14);
                    new TableGroupNode().Initialize(this, _group);
            }
        }
    }

    public unsafe class TableGroupNode : ResourceNode
    {
        internal ItmFreqGroupNode* Header { get { return (ItmFreqGroupNode*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.U8Folder; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            Name = "Group["+Parent.Children.IndexOf(this)+"]";
            return true;
        }
        public override void OnPopulate()
        {
            for (int i = 0; i < Header->_entryCount; i++)
            {
                VoidPtr addr = Parent.Parent.Parent.WorkingUncompressed.Address;

                ItmFreqEntry* Entry = (ItmFreqEntry*)(addr + Header->_entryOffset + 0x20 + (i*0x10));
                DataSource source = new DataSource(Entry, 0x10);
                new ItmFreq().Initialize(this, source);
            }
        }
    }
    public unsafe class ItmFreq : ResourceNode
    {
        internal ItmFreqEntry* Header { get { return (ItmFreqEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        internal int _id;
        [DisplayName("Item ID")]
        [Category("Item")]
        public int ID { get { return _id; } set { _id = value; SignalPropertyChange(); } }

        internal int _subID;
        [DisplayName("Sub Item ID")]
        [Category("Item")]
        public int SubID { get { return _subID; } set { _subID = value; SignalPropertyChange(); } }

        internal float _frequency;
        [DisplayName("Frequency")]
        [Category("Item Frequency")]
        public float Frequency { get { return _frequency; } set { _frequency = value; SignalPropertyChange(); } }

        internal short _action;
        [DisplayName("Start Action")]
        [Category("Item")]
        public short Action { get { return _action; } set { _action = value; SignalPropertyChange(); } }

        internal short _subaction;
        [DisplayName("Start Subaction")]
        [Category("Item")]
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
        public override void OnPopulate()
        {
        }
    }
}
