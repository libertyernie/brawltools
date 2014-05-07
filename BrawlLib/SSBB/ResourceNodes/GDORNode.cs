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
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }


        [Category("BLOC Archive")]
        public int SubFiles { get { return Header->_count; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            if (_name == null)
                _name = "GDOR";
            return Header->_count > 0;
        }

        internal static ResourceNode TryParse(DataSource source) { return ((GDOR*)source.Address)->_tag == GDOR.Tag ? new GDORNode() : null; }
    }
}
