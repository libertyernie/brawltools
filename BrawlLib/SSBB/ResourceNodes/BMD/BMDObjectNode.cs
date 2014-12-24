using BrawlLib.Modeling;
using BrawlLib.SSBBTypes;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class BMDObjectNode : BMDEntryNode, IObject, IMatrixNodeUser
    {
        internal BMDObjectEntry* Header { get { return (BMDObjectEntry*)WorkingUncompressed.Address; } }

        public PrimitiveManager _manager;

        public BMDBoneNode BoneNode { get { return _bone; } set { _bone = value; } }
        public BMDMaterialNode MaterialNode { get { return _material; } set { _material = value; } }
        public List<Vertex3> Vertices { get { return _manager != null ? _manager._vertices : null; } }

        [Browsable(false)]
        public IMatrixNode MatrixNode
        {
            get
            {
                return _bone;
            }
            set
            {
                
            }
        }

        private BMDBoneNode _bone;
        private BMDMaterialNode _material;
        private Vector3 _min, _max;
        private float _unk;

        public Vector3 BoxMin { get { return _min; } }
        public Vector3 BoxMax { get { return _max; } }
        public float Unknown { get { return _unk; } }

        public override bool OnInitialize()
        {
            _min = Header->_boxMin;
            _max = Header->_boxMax;
            _unk = Header->_unknown2;

            return false;
        }

        [Browsable(false)]
        public PrimitiveManager PrimitiveManager
        {
            get { return _manager; }
        }

        bool _render = true;

        [Browsable(false)]
        public bool IsRendering
        {
            get
            {
                return _render;
            }
            set
            {
                _render = value;
            }
        }

        bool _attached = false;

        [Browsable(false)]
        public bool Attached
        {
            get { return _attached; }
        }

        public void Attach()
        {
            _attached = true;
            _render = true;
        }

        public void Detach()
        {
            _attached = false;
        }

        public void Refesh()
        {

        }

        public void Render(params object[] args)
        {
            if (!_render || _manager == null)
                return;

            //bool useShaders = TKContext.CurrentContext._shadersEnabled;
            //BMDMaterialNode material = UsableMaterialNode;

            GL.Disable(EnableCap.CullFace);

            if (args.Length > 0 && args[0] is bool && (bool)args[0])
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
                _manager.PrepareStream();
                _manager.DisableTextures();
                GL.Color4(Color.Black);
                _manager.RenderMesh();
                _manager.DetachStreams();
                return;
            }

            GL.Enable(EnableCap.Texture2D);
            _manager.PrepareStream();

            GL.MatrixMode(MatrixMode.Modelview);
            //if (material != null)
            //{
            //    switch ((int)material.CullMode)
            //    {
            //        case 0: //None
            //            GL.Disable(EnableCap.CullFace);
            //            break;
            //        case 1: //Outside
            //            GL.Enable(EnableCap.CullFace);
            //            GL.CullFace(CullFaceMode.Front);
            //            break;
            //        case 2: //Inside
            //            GL.Enable(EnableCap.CullFace);
            //            GL.CullFace(CullFaceMode.Back);
            //            break;
            //        case 3: //Double
            //            GL.Enable(EnableCap.CullFace);
            //            GL.CullFace(CullFaceMode.FrontAndBack);
            //            break;
            //    }

            //    //if (material.EnableBlend)
            //    //{
            //    //    GL.Enable(EnableCap.Blend);
            //    //}
            //    //else
            //    //    GL.Disable(EnableCap.Blend);

            //    if (material.Children.Count == 0)
            //    {
            //        _manager.ApplyTexture(null);
            //        _manager.RenderMesh();
            //    }
            //    else
            //    {
            //        foreach (MDL0MaterialRefNode mr in material.Children)
            //        {
            //            if (mr._texture != null && (!mr._texture.Enabled || mr._texture.Rendered))
            //                continue;

            //            GL.MatrixMode(MatrixMode.Texture);

            //            GL.PushMatrix();

            //            //Add bind transform
            //            GL.Scale(mr.Scale._x, mr.Scale._y, 0);
            //            GL.Rotate(mr.Rotation, 1, 0, 0);
            //            GL.Translate(-mr.Translation._x, mr.Translation._y - ((mr.Scale._y - 1) / 2), 0);

            //            //Now add frame transform
            //            GL.Scale(mr._frameState._scale._x, mr._frameState._scale._y, 1);
            //            GL.Rotate(mr._frameState._rotate._x, 1, 0, 0);
            //            GL.Translate(-mr._frameState._translate._x, mr._frameState._translate._y - ((mr._frameState._scale._y - 1) / 2), 0);

            //            GL.MatrixMode(MatrixMode.Modelview);

            //            mr.Bind(-1);

            //            _manager.ApplyTexture(mr);
            //            _manager.RenderMesh();

            //            GL.MatrixMode(MatrixMode.Texture);
            //            GL.PopMatrix();
            //            GL.MatrixMode(MatrixMode.Modelview);
            //        }
            //    }
            //}
            //else
            //{
            _manager.DisableTextures();
            _manager.RenderMesh();
            //}

            _manager.DetachStreams();
        }

        public void GetBox(out Vector3 min, out Vector3 max)
        {
            min = new Vector3();
            max = new Vector3();
        }

        internal void WeightVertices()
        {
            if (_manager != null)
                _manager.Weight();
        }

        public void Refresh()
        {
            
        }
    }
}
