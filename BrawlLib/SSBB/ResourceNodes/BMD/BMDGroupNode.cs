using BrawlLib.SSBBTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    public enum BMDResourceType
    {
        Bones,
        Objects,
        Materials,
        Registers,
        Textures,
        None
    }

    public unsafe class BMDGroupNode : BMDEntryNode
    {
        internal BMDCommonHeader* CommonHeader { get { return (BMDCommonHeader*)WorkingUncompressed.Address; } }

        public override ResourceType ResourceType { get { return ResourceType.BMDGroup; } }

        public BMDResourceType GroupType { get { return _type; } }
        BMDResourceType _type = BMDResourceType.None;

        public BMDGroupNode(BMDResourceType type)
        {
            _name = (_type = type).ToString("g");
        }

        public override bool OnInitialize()
        {
            return true;
        }

        public override void RemoveChild(ResourceNode child)
        {
            if ((_children != null) && (_children.Count == 1) && (_children.Contains(child)))
                _parent.RemoveChild(this);
            else
                base.RemoveChild(child);
        }
    }

    public unsafe abstract class BMDEntryNode : ResourceNode
    {
        [Browsable(false)]
        public BMDNode Model
        {
            get
            {
                ResourceNode n = _parent;
                while (!(n is BMDNode) && (n != null))
                    n = n._parent;
                return n as BMDNode;
            }
        }

        internal virtual void Bind() { }
        internal virtual void Unbind() { }
    }
}
