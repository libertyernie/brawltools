using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class GSNDNode : ResourceNode
    {
        internal GSND* Header { get { return (GSND*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.GSND; } }

        [Category("GSND")]
        [DisplayName("Entries")]
        public int count { get { return Header->_count; } }
        public override void OnPopulate()
        {
            for (int i = 0; i < Header->_count; i++)
            {
                DataSource source;
                if (i == Header->_count - 1)
                { source = new DataSource((*Header)[i], WorkingUncompressed.Address + WorkingUncompressed.Length - (*Header)[i]); }
                else { source = new DataSource((*Header)[i], (*Header)[i + 1] - (*Header)[i]); }
                new GSNDEntryNode().Initialize(this, source);
            }
        }
        public override bool OnInitialize()
        {
            base.OnInitialize();
            if (_name == null)
                _name = "Sound Effects";
            return Header->_count > 0;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {

            GSND* header = (GSND*)address;
            *header = new GSND(Children.Count);
            uint offset = (uint)(0x08 + (Children.Count * 4));
            for (int i = 0; i < Children.Count; i++)
            {            
                if (i > 0){offset += (uint)(Children[i - 1].CalculateSize(false));}
                *(buint*)((VoidPtr)address + 0x08 + i * 4) = offset;
                _children[i].Rebuild((VoidPtr)address + offset, _children[i].CalculateSize(false), true);
            }
        }
        

        internal static ResourceNode TryParse(DataSource source) { return ((GSND*)source.Address)->_tag == GSND.Tag ? new GSNDNode() : null; }
    }

    public unsafe class GSNDEntryNode : ResourceNode
    {
        internal GSNDEntry* Header { get { return (GSNDEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }
        [Category("Sound")]
        [DisplayName("Bone Name")]
        public string BName { get { return Header->Name; } set { Header->Name = value; SignalPropertyChange(); } }

        [Category("Sound")]
        [DisplayName("Info Index")]
        public int infoIndex { get { return Header->_infoIndex; } set { Header->_infoIndex = value; SignalPropertyChange(); } }

        [Category("Misc")]
        [DisplayName("unknown")]
        public float unkFloat0 { get { return Header->_unkFloat0; } set { Header->_unkFloat0 = value; SignalPropertyChange(); } }

        [Category("Misc")]
        [DisplayName("unknown")]
        public float unkFloat1 { get { return Header->_unkFloat1; } set { Header->_unkFloat1 = value; SignalPropertyChange(); } }

        [Category("Misc")]
        [DisplayName("Trigger ID?")]
        public string _trigger { get { return Header->Trigger; } set { Header->Trigger = value; SignalPropertyChange(); } }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            if (_name == null)
                _name = "Sound[" + Index + ']';
            return false;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            GSNDEntry* header = (GSNDEntry*)address;
            *header = new GSNDEntry();
            header->_infoIndex = infoIndex;
            header->_pad0 = header->_pad1 =
            header->_pad2 = header->_pad3 = header->Pad4 = 0;
            header->_unkFloat0 = unkFloat0;
            header->_unkFloat1 = unkFloat1;
            header->Name = BName;
            header->Trigger = _trigger;
        }
    }
}