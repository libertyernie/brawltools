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
    public unsafe class BLOCNode : ARCEntryNode
    {
        public override ResourceType ResourceType { get { return ResourceType.BLOC; } }
        internal BLOC* Header { get { return (BLOC*)WorkingUncompressed.Address; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            return Header->_count > 0;
        }

        public override void OnPopulate()
        {
            for (int i = 0; i < Header->_count; i++)
            {

                DataSource source;
                if (i == Header->_count - 1)
                source = new DataSource((*Header)[i], WorkingUncompressed.Address+WorkingUncompressed.Length - (*Header)[i]);
                else {source = new DataSource((*Header)[i], (*Header)[i + 1] - (*Header)[i]); }
                if ((NodeFactory.FromSource(this, source) == null))
                {
                    new BLOCEntryNode().Initialize(this, source);
                }
            }  
    }

        internal static ResourceNode TryParse(DataSource source) { return ((BLOC*)source.Address)->_tag == BLOC.Tag ? new BLOCNode() : null; }
    }

    public unsafe class BLOCEntryNode : ResourceNode
    {
        internal BLOCEntry* Header { get { return (BLOCEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.BLOCEntry; } }
        public int Entries { get; private set; }



        public override bool OnInitialize()
        {
            base.OnInitialize();
            byte* _NumFiles = (byte*)WorkingUncompressed.Address + 0x07;  
            if (_name == null)
                _name = new String((sbyte*)Header);
            this.Entries = *(int*)_NumFiles;
            return false;
        }
    }
}
