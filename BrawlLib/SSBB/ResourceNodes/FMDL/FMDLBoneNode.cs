using BrawlLib.Modeling;
using BrawlLib.OpenGL;
using BrawlLib.SSBBTypes;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class FMDLBoneNode : FMDLEntryNode, IBoneNode
    {
        internal FSKLBone* Header { get { return (FSKLBone*)WorkingUncompressed.Address; } }

        public string ParentBone1 { get { return Header->_parentIndex1 < 0 ? null : Model._boneCache[Header->_parentIndex1].Name; } }
        public string ParentBone2 { get { return Header->_parentIndex2 < 0 ? null : Model._boneCache[Header->_parentIndex2].Name; } }
        public string ParentBone3 { get { return Header->_parentIndex3 < 0 ? null : Model._boneCache[Header->_parentIndex3].Name; } }
        public string ParentBone4 { get { return Header->_parentIndex4 < 0 ? null : Model._boneCache[Header->_parentIndex4].Name; } }
        public Vector3 Scale { get { return Header->_scale; } }
        public Vector4 Rotation { get { return Header->_rotate; } }
        public Vector3 Translation { get { return Header->_translate; } }
        public Bin32 Flags { get { return Header->_flags; } }
        public ushort BoneIndex { get { return Header->_boneIndex; } }

        public override bool OnInitialize()
        {
            _name = Header->Name;

            _frameState = _bindState = new FrameState(Scale, new Vector3(Rotation._x * Maths._rad2degf, Rotation._y * Maths._rad2degf, Rotation._z * Maths._rad2degf), Translation);

            return false;
        }

        int _weightCount, _refCount;
        bool _locked, _moved, _render;
        FrameState _bindState, _frameState;        
        internal Matrix _bindMatrix, _inverseBindMatrix, _matrix, _inverseMatrix;
        public static Color DefaultLineColor = Color.FromArgb(0, 0, 128);
        public static Color DefaultLineDeselectedColor = Color.FromArgb(128, 0, 0);
        public static Color DefaultNodeColor = Color.FromArgb(0, 128, 0);
        public Color _boneColor = Color.Transparent;
        public Color _nodeColor = Color.Transparent;

        [Browsable(false)]
        public bool Locked
        {
            get { return _locked; }
            set { _locked = value; }
        }
        [Browsable(false)]
        public bool Moved
        {
            get { return _moved; }
            set { _moved = value; }
        }
        [Browsable(false)]
        public int WeightCount
        {
            get { return _weightCount; }
            set { _weightCount = value; }
        }
        [Browsable(true)]
        public FrameState BindState
        {
            get { return _bindState; }
            set { _bindState = value; }
        }
        [Browsable(true)]
        public FrameState FrameState
        {
            get { return _frameState; }
            set { _frameState = value; }
        }
        [Browsable(false)]
        public System.Drawing.Color NodeColor
        {
            get { return Color.Green; }
            set { }
        }
        [Browsable(false)]
        public System.Drawing.Color BoneColor
        {
            get { return Color.Blue; }
            set { }
        }
        [Browsable(false)]
        int IBoneNode.BoneIndex
        {
            get { return BoneIndex; }
        }
        [Browsable(false)]
        public IModel IModel
        {
            get { return (IModel)Model; }
        }
        [Browsable(false)]
        public List<Wii.Models.Influence> LinkedInfluences
        {
            get { return new List<Wii.Models.Influence>(); }
        }
        [Browsable(false)]
        public bool IsRendering
        {
            get { return _render; }
            set { _render = value; }
        }
        internal override void Bind()
        {
            _render = true;
            _boneColor = Color.Transparent;
            _nodeColor = Color.Transparent;
        }
        public void Render(params object[] args)
        {
            bool Foreground = (args.Length > 0 && args[0] is bool ? (bool)args[0] : false);

            Color c = Foreground ? DefaultLineColor : DefaultLineDeselectedColor;

            if (_boneColor != Color.Transparent)
                GL.Color4(_boneColor.R / 255.0f, _boneColor.G / 255.0f, _boneColor.B / 255.0f, Foreground ? 1.0f : 0.45f);
            else
                GL.Color4(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, Foreground ? 1.0f : 0.45f);

            //Draw name if selected
            if (GLPanel.Current != null && GLPanel.Current is ModelPanel && _nodeColor != Color.Transparent)
            {
                Vector3 pt = _matrix.GetPoint();
                Vector3 v2 = GLPanel.Current.CurrentViewport.Camera.Project(pt);
                ((ModelPanel)GLPanel.Current).CurrentViewport.ScreenText[Name] = new Vector3(v2._x, v2._y - 9.0f, v2._z);
            }

            Vector3 v1 = (_parent == null || !(_parent is FMDLBoneNode)) ? new Vector3(0.0f) : ((FMDLBoneNode)_parent)._matrix.GetPoint();
            Vector3 v = _matrix.GetPoint();

            GL.Begin(BeginMode.Lines);

            GL.Vertex3((float*)&v1);
            GL.Vertex3((float*)&v);

            GL.End();

            GL.PushMatrix();
            {
                fixed (Matrix* m = &_matrix)
                    GL.MultMatrix((float*)m);

                //Render node
                GLDisplayList ndl = TKContext.FindOrCreate<GLDisplayList>("BoneNodeOrb", MDL0BoneNode.CreateNodeOrb);
                if (_nodeColor != Color.Transparent)
                    GL.Color4(_nodeColor.R / 255.0f, _nodeColor.G / 255.0f, _nodeColor.B / 255.0f, Foreground ? 1.0f : 0.45f);
                else
                    GL.Color4(DefaultNodeColor.R / 255.0f, DefaultNodeColor.G / 255.0f, DefaultNodeColor.B / 255.0f, Foreground ? 1.0f : 0.45f);

                ndl.Call();

                MDL0BoneNode.DrawNodeOrients(Foreground ? 1.0f : 0.5f);
            }
            GL.PopMatrix();

            //Render children
            foreach (FMDLBoneNode n in Children)
                n.Render(Foreground);
        }
        [Browsable(false)]
        public List<IMatrixNodeUser> Users
        {
            get { return new List<IMatrixNodeUser>(); }
            set { }
        }
        [Browsable(false)]
        public int ReferenceCount
        {
            get { return _refCount; }
            set { _refCount = value; }
        }
        [Browsable(false)]
        public int NodeIndex
        {
            get { return BoneIndex; }
        }
        [Browsable(true)]
        public Matrix BindMatrix
        {
            get { return _bindMatrix; }
        }
        [Browsable(true)]
        public Matrix InverseBindMatrix
        {
            get { return _inverseBindMatrix; }
        }
        [Browsable(true)]
        public Matrix Matrix
        {
            get { return _matrix; }
        }
        [Browsable(true)]
        public Matrix InverseMatrix
        {
            get { return _inverseMatrix; }
        }
        [Browsable(false)]
        public bool IsPrimaryNode
        {
            get { return true; }
        }
        [Browsable(false)]
        public List<Wii.Models.BoneWeight> Weights
        {
            get { return new List<Wii.Models.BoneWeight>(); }
        }


        public void Render(bool targetModel, GLViewport viewport)
        {
            
        }

        public void Render(bool targetModel, ModelPanelViewport viewport, Vector3 position = default(Vector3))
        {
            
        }

        public void RecalcBindState(bool updateMesh, bool moveMeshWithBone, bool updateAssetLists = true)
        {
            
        }

        public void RecalcFrameState(ModelPanelViewport v = null)
        {
            _frameState.CalcTransforms();
            if (_parent is FMDLBoneNode)
            {
                _matrix = ((FMDLBoneNode)_parent)._matrix * _frameState._transform;
                _inverseMatrix = _frameState._iTransform * ((FMDLBoneNode)_parent)._inverseMatrix;
            }
            else
            {
                _matrix = _frameState._transform;
                _inverseMatrix = _frameState._iTransform;
            }

            foreach (FMDLBoneNode bone in Children)
                bone.RecalcFrameState();
        }
    }
}
