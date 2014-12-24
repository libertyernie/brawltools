using BrawlLib.SSBBTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class J3DNode : RARCEntryNode
    {
        internal J3DCommonHeader* Header { get { return (J3DCommonHeader*)WorkingUncompressed.Address; } }

        [Category("J3D Node")]
        public virtual int Version { get { return _version; } }
        protected int _version = 1;

        public override bool OnInitialize()
        {
            return base.OnInitialize();
        }
    }
}
