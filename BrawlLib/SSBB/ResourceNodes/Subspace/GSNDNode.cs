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

        internal static ResourceNode TryParse(DataSource source) { return ((GSND*)source.Address)->_tag == GSND.Tag ? new GSNDNode() : null; }
    }

    public unsafe class GSNDEntryNode : ResourceNode
    {
        internal GSNDEntry* Header { get { return (GSNDEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }
        [Category("Sound")]
        [DisplayName("Bone Name")]
        public string BName { get { return new String((sbyte*)(Header + 0x1C)); ; } }
        
        [Category("Sound")]
        [DisplayName("Info Index")]
        public bint InfoIndex { get { return *(bint*)(WorkingUncompressed.Address); } }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            if (_name == null)
                _name = "Sound[" + Index + ']';
            return false;
        }
    }
}