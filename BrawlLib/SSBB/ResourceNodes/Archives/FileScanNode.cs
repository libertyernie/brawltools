using System;
using BrawlLib.SSBBTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Compression;
using System.Windows;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class FileScanNode : ResourceNode
    {
        internal byte* Data { get { return (byte*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }
        
        public List<ResourceNode> _list;
        public override void OnPopulate()
        {
            foreach (ResourceNode r in _list)
            {
                r._parent = this;
                _children.Add(r);
            }
        }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            _name = Path.GetFileNameWithoutExtension(_origPath);
            return true;
        }
    }
}
