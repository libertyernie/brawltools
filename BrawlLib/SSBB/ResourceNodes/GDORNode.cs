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
    public unsafe class GDORNode : ResourceNode
    {
        internal GDOR* Header { get { return (GDOR*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.GDOR; } }
        [Category("BLOC Archive")]
        public int Doors { get { return Header->_count; } }

        public override void OnPopulate()
        {
            for (int i = 0; i < Header->_count; i++)
            {
                DataSource source;
                if (i == Header->_count - 1)
                { source = new DataSource((*Header)[i], WorkingUncompressed.Address + WorkingUncompressed.Length - (*Header)[i]); }
                else { source = new DataSource((*Header)[i], (*Header)[i + 1] - (*Header)[i]); }
                new GDOREntryNode().Initialize(this, source);

            }
        }
        public override bool OnInitialize()
        {
            base.OnInitialize();
            if (_name == null)
                _name = "GDOR";
            return Header->_count > 0;
        }

        internal static ResourceNode TryParse(DataSource source) { return ((GDOR*)source.Address)->_tag == GDOR.Tag ? new GDORNode() : null; }
    }
    public unsafe class GDOREntryNode : ResourceNode
    {
        internal GDOREntry* Header { get { return (GDOREntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            if (_name == null)
                _name = "ENTRY["+Index+']';
            return false;
        }
    }
}
