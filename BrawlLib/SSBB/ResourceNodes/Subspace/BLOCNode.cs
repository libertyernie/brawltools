using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.Runtime.InteropServices;
using BrawlLib.Imaging;
using BrawlLib.Wii;

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
                //source decleration
                DataSource source;

                //Enumerate datasources for each child node
                if (i == Header->_count - 1)
                source = new DataSource((*Header)[i], WorkingUncompressed.Address+WorkingUncompressed.Length - (*Header)[i]);
                else {source = new DataSource((*Header)[i], (*Header)[i + 1] - (*Header)[i]);}

                //Call NodeFactory on datasource to initiate various files
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
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }
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
