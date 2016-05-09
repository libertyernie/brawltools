using BrawlLib.Modeling;
using BrawlLib.SSBBTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrawlLib.OpenGL;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class FMDLObjectNode : FMDLEntryNode, IObject
    {
        internal FSHP* Header { get { return (FSHP*)WorkingUncompressed.Address; } }

        public string Material { get { return Model.MaterialList[Header->_fmatIndex].Name; } }
        public string Bone { get { return Model.BoneCache[Header->_fsklIndex].Name; } }

        public byte LODCount { get { return Header->_lodCount; } }
        public uint VisGroupTreeNodeCount { get { return Header->_visGroupTreeNodeCount; } }
        public byte Unknown2 { get { return Header->_unk2; } }
        public float Unknown3 { get { return Header->_unk3; } }

        public ushort BufferIndex1 { get { return Header->_fvtxIndex1; } }
        public ushort BufferIndex2 { get { return Header->_fvtxIndex2; } }
        public ushort BoneIndexArrayCount { get { return Header->_fsklIndexArrayCount; } }
        public uint BoneIndexArrayOffset { get { return Header->_fsklIndexArrayOffset; } }

        List<Vertex3> _vertices = new List<Vertex3>();

        public override bool OnInitialize()
        {
            _name = Header->Name;

            return false;
        }

        bool _attached, _render;

        public event EventHandler DrawCallsChanged;

        public List<Vertex3> Vertices { get { return _vertices; } }

        [Browsable(false)]
        public bool IsRendering
        {
            get { return _render; }
            set { _render = value; }
        }
        [Browsable(false)]
        public bool Attached { get { return _attached; } }

        public List<DrawCallBase> DrawCalls
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Attach()
        {
            _attached = true;
        }

        public void Detach()
        {
            _attached = false;
        }

        public void Refresh()
        {
            
        }

        public void Render(params object[] args)
        {
            
        }

        public Box GetBox()
        {
            if (_vertices == null)
                return new Box();

            Box box = Box.ExpandableVolume;
            foreach (Vertex3 vertex in _vertices)
                box.ExpandVolume(vertex.WeightedPosition);

            return box;
        }

        public void PreRender(ModelPanelViewport v)
        {
            
        }
    }
}
