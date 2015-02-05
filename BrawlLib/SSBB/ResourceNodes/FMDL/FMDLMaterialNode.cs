using BrawlLib.SSBBTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class FMDLMaterialNode : FMDLEntryNode
    {
        internal FMAT* Header { get { return (FMAT*)WorkingUncompressed.Address; } }

        public override bool OnInitialize()
        {
            _name = Header->Name;

            return false;
        }
    }
}
