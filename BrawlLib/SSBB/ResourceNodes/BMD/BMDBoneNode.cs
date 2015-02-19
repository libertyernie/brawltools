using BrawlLib.Modeling;
using BrawlLib.OpenGL;
using BrawlLib.SSBBTypes;
using BrawlLib.Wii.Animations;
using BrawlLib.Wii.Models;
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
    public unsafe class BMDBoneNode : BMDEntryNode, IBoneNode
    {
        BMDJointEntry* Header { get { return (BMDJointEntry*)WorkingUncompressed.Address; } }

        public ushort Unknown1 { get { return _unk1; } set { _unk1 = value; SignalPropertyChange(); } }
        public ushort Unknown2 { get { return _unk2; } set { _unk2 = value; SignalPropertyChange(); } }
        public Vector3 Scale { get { return _scale; } set { _scale = value; SignalPropertyChange(); } }
        public Vector3 Rotation { get { return _rot; } set { _rot = value; SignalPropertyChange(); } }
        public Vector3 Translation { get { return _trans; } set { _trans = value; SignalPropertyChange(); } }
        public float Unknown3 { get { return _unk3; } set { _unk3 = value; SignalPropertyChange(); } }
        public Vector3 BoxMin { get { return _boxMin; } set { _boxMin = value; SignalPropertyChange(); } }
        public Vector3 BoxMax { get { return _boxMax; } set { _boxMax = value; SignalPropertyChange(); } }

        private ushort _unk1;
        private ushort _unk2;
        private Vector3 _scale;
        private Vector3 _rot;
        private Vector3 _trans;
        private float _unk3;
        private Vector3 _boxMin;
        private Vector3 _boxMax;
        public int _index;

        public override bool OnInitialize()
        {
            _unk1 = Header->_unk1;
            _unk2 = Header->_unk2;
            _unk3 = Header->_unk3;
            _scale = Header->_scale;
            _rot = Header->Rotation;
            _trans = Header->_trans;
            _boxMin = Header->_boxMin;
            _boxMax = Header->_boxMax;

            _bindState = _frameState = new FrameState(Scale, Rotation, Translation);

            return false;
        }

        bool _locked = false;
        public Matrix _frameMatrix, _inverseFrameMatrix, _bindMatrix, _invBindMatrix;
        List<IMatrixNodeUser> _users = new List<IMatrixNodeUser>();
        int _refCount = 0;
        int _nodeIndex;
        List<BoneWeight> _weights;
        bool _moved = false;
        int _weightCount = 0;
        List<Influence> _linkedInfluences = new List<Influence>();
        FrameState _frameState, _bindState;

        [Browsable(false)]
        public bool Locked { get { return _locked; } set { _locked = value; } }
        [Browsable(false)]
        public Matrix BindMatrix { get { return _bindMatrix; } }
        [Browsable(false)]
        public Matrix InverseBindMatrix { get { return _invBindMatrix; } }
        [Browsable(false)]
        public List<IMatrixNodeUser> Users { get { return _users; } set { _users = value; } }
        [Browsable(false)]
        public int ReferenceCount { get { return _refCount; } set { _refCount = value; } }
        [Browsable(false)]
        public int NodeIndex { get { return _nodeIndex; } }
        [Browsable(false)]
        public Matrix Matrix { get { return _frameMatrix; } }
        [Browsable(false)]
        public Matrix InverseMatrix { get { return _inverseFrameMatrix; } }
        [Browsable(false)]
        public bool IsPrimaryNode { get { return true; } }
        [Browsable(false)]
        public List<BoneWeight> Weights { get { return _weights; } }
        [Browsable(false)]
        public bool Moved { get { return _moved; } set { _moved = value; } }
        [Browsable(false)]
        public int WeightCount { get { return _weightCount; } set { _weightCount = value; } }
        [Browsable(false)]
        public FrameState FrameState { get { return _frameState; } set { _frameState = value; } }
        [Browsable(false)]
        public FrameState BindState { get { return _bindState; } set { _bindState = value; } }
        [Browsable(false)]
        public Color BoneColor { get { return _boneColor; } set { _boneColor = value; } }
        [Browsable(false)]
        public Color NodeColor { get { return _nodeColor; } set { _nodeColor = value; } }
        [Browsable(false)]
        public int BoneIndex { get { return _index; } }
        [Browsable(false)]
        public IModel IModel { get { return Model; } }
        [Browsable(false)]
        public List<Influence> LinkedInfluences { get { return _linkedInfluences; } }

        public void RecalcFrameState()
        {
            _frameState.CalcTransforms();

            if (_parent is MDL0BoneNode)
            {
                _frameMatrix = ((MDL0BoneNode)_parent)._frameMatrix * _frameState._transform;
                _inverseFrameMatrix = _frameState._iTransform * ((MDL0BoneNode)_parent)._inverseFrameMatrix;
            }
            else
            {
                _frameMatrix = _frameState._transform;
                _inverseFrameMatrix = _frameState._iTransform;
            }
            
            foreach (BMDBoneNode bone in Children)
                bone.RecalcFrameState();
        }

        internal void ApplyCHR0(CHR0Node node, int index)
        {
            CHR0EntryNode e;

            _frameState = _bindState;

            if (node != null && index > 0 && (e = node.FindChild(Name, false) as CHR0EntryNode) != null) //Set to anim pose
                fixed (FrameState* v = &_frameState)
                {
                    float* f = (float*)v;
                    for (int i = 0; i < 9; i++)
                        if (e.Keyframes[i]._keyCount > 0)
                            f[i] = e.GetFrameValue(i, index - 1);

                    _frameState.CalcTransforms();
                }

            foreach (BMDBoneNode b in Children)
                b.ApplyCHR0(node, index);
        }

        public static Color DefaultBoneColor = Color.FromArgb(0, 0, 128);
        public static Color DefaultNodeColor = Color.FromArgb(0, 128, 0);

        public Color _boneColor = Color.Transparent;
        public Color _nodeColor = Color.Transparent;

        public const float _nodeRadius = 0.20f;

        public bool _render = true;
        internal unsafe void Render()
        {
            if (!_render)
                return;

            if (_boneColor != Color.Transparent)
                GL.Color4(_boneColor.R / 255.0f, _boneColor.G / 255.0f, _boneColor.B / 255.0f, 1.0f);
            else
                GL.Color4(DefaultBoneColor.R / 255.0f, DefaultBoneColor.G / 255.0f, DefaultBoneColor.B / 255.0f, 1.0f);

            //Draw name if selected
            if (GLPanel.Current != null && GLPanel.Current is ModelPanel && _nodeColor != Color.Transparent)
            {
                Vector3 pt = _frameMatrix.GetPoint();
                Vector3 v2 = GLPanel.Current.CurrentViewport.Project(pt);
                ((ModelPanel)GLPanel.Current).CurrentViewport.ScreenText[Name] = new Vector3(v2._x, v2._y - 9.0f, v2._z);
            }

            Vector3 v1 = (_parent == null || !(_parent is BMDBoneNode)) ? new Vector3(0.0f) : ((BMDBoneNode)_parent)._frameMatrix.GetPoint();
            Vector3 v = _frameMatrix.GetPoint();

            GL.Begin(PrimitiveType.Lines);

            GL.Vertex3((float*)&v1);
            GL.Vertex3((float*)&v);

            GL.End();

            GL.PushMatrix();

            fixed (Matrix* m = &_frameMatrix)
                GL.MultMatrix((float*)m);

            //Render node
            GLDisplayList ndl = TKContext.FindOrCreate<GLDisplayList>("BoneNodeOrb", MDL0BoneNode.CreateNodeOrb);
            if (_nodeColor != Color.Transparent)
                GL.Color4(_nodeColor.R / 255.0f, _nodeColor.G / 255.0f, _nodeColor.B / 255.0f, 1.0f);
            else
                GL.Color4(DefaultNodeColor.R / 255.0f, DefaultNodeColor.G / 255.0f, DefaultNodeColor.B / 255.0f, 1.0f);

            ndl.Call();

            MDL0BoneNode.DrawNodeOrients(true);

            GL.PopMatrix();

            //Render children
            foreach (BMDBoneNode n in Children)
                n.Render();
        }

        public bool IsRendering
        {
            get
            {
                return true;
            }
            set
            {
                
            }
        }

        public void Render(bool targetModel, GLViewport viewport)
        {
            
        }
    }
}
