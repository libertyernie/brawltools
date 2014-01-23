using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using BrawlLib.IO;
using System.PowerPcAssembly;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RELDeConStructorNode : RELMethodNode
    {
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        public bool _destruct;
        public int _index;

        public override bool OnInitialize()
        {
            _name = String.Format("{0}[{1}]", _destruct ? "Destructor" : "Constructor", Index);
            
            base.OnInitialize();

            return false;
        }
    }
}
