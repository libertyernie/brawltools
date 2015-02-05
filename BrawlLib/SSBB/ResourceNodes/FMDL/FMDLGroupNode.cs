using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class FMDLGroupNode : FMDLEntryNode
    {
        public override ResourceType ResourceType { get { return ResourceType.Container; } }

        public FMDLGroupNode(FMDLResourceType type)
        {
            _type = type;
            _name = _type.ToString("g");
        }

        public int _index;
        public FMDLResourceType _type;

        internal override void GetStrings(StringTable table)
        {
            foreach (FMDLEntryNode n in Children)
                n.GetStrings(table);
        }

        internal void Initialize(ResourceNode parent, DataSource source, int index)
        {
            _index = index;
            base.Initialize(parent, source);
        }

        public override void RemoveChild(ResourceNode child)
        {
            if ((_children != null) && (_children.Count == 1) && (_children.Contains(child)))
                _parent.RemoveChild(this);
            else
                base.RemoveChild(child);
        }

        internal override void Bind()
        {
            foreach (FMDLEntryNode e in Children)
                e.Bind();
        }
        internal override void Unbind()
        {
            foreach (FMDLEntryNode e in Children)
                e.Unbind();
        }
    }

    public enum FMDLResourceType : int
    {
        Bones,
        Objects,
        Materials,
        Uniforms,
        VertexGroups,
        Max,
    }

    public unsafe abstract class FMDLEntryNode : ResourceNode
    {
        internal virtual void GetStrings(StringTable table) { table.Add(_name); }

        internal int _entryIndex;

        [Browsable(false)]
        public FMDLNode Model
        {
            get
            {
                ResourceNode n = _parent;
                while (!(n is FMDLNode) && (n != null))
                    n = n._parent;
                return n as FMDLNode;
            }
        }

        [Browsable(false)]
        public BFRESNode BFRESNode
        {
            get
            {
                ResourceNode n = _parent;
                while (!(n is BFRESNode) && (n != null))
                    n = n._parent;
                return n as BFRESNode;
            }
        }

        internal virtual void Bind() { }
        internal virtual void Unbind() { }

        protected internal virtual void PostProcess(VoidPtr mdlAddress, VoidPtr dataAddress, StringTable stringTable) { }
    }
}
