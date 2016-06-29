using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Animations;
using System.Windows.Forms;
using BrawlLib.Modeling;
using BrawlLib.OpenGL;
using BrawlLib.Wii.Models;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class VBNNode : ResourceNode, IModel
    {
        internal VBNHeader* Header { get { return (VBNHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.VBN; } }
        public override Type[] AllowedChildTypes { get { return new Type[] { typeof(OMOBoneEntryNode) }; } }

        public Endian _endian = Endian.Little;

        public VBNNode() { }
        const string _category = "Skeleton";

        public Dictionary<uint, VBNBoneNode> _boneDictionary;

        public VBNBoneNode[] _boneCache = new VBNBoneNode[0];
        public VBNBoneNode[] _rootBones = new VBNBoneNode[0];

        public ushort _versionMin, _versionMax;

        uint _unk1;
        uint _unk2;
        uint _unk3;
        uint _unk4;

        public event EventHandler DrawCallsChanged;

        [Category(_category)]
        public Endian Endian
        {
            get { return _endian; }
            set { _endian = value; SignalPropertyChange(); }
        }
        [Category(_category)]
        public ushort VersionMin
        {
            get { return _versionMin; }
            set { _versionMin = value; SignalPropertyChange(); }
        }
        [Category(_category)]
        public ushort VersionMax
        {
            get { return _versionMax; }
            set { _versionMax = value; SignalPropertyChange(); }
        }
        [Category(_category)]
        public uint Unk1
        {
            get { return _unk1; }
            set { _unk1 = value; SignalPropertyChange(); }
        }
        [Category(_category)]
        public uint Unk2
        {
            get { return _unk2; }
            set { _unk2 = value; SignalPropertyChange(); }
        }
        [Category(_category)]
        public uint Unk3
        {
            get { return _unk3; }
            set { _unk3 = value; SignalPropertyChange(); }
        }
        [Category(_category)]
        public uint Unk4
        {
            get { return _unk4; }
            set { _unk4 = value; SignalPropertyChange(); }
        }

        public string[] Bones
        {
            get { return _boneCache.Select(x => x.Name).ToArray(); }
        }

        private InfluenceManager _manager = new InfluenceManager();
        [Browsable(false)]
        public InfluenceManager Influences { get { return _manager; } }
        [Browsable(false)]
        public IBoneNode[] BoneCache { get { return _boneCache.Select(x => x as IBoneNode).ToArray(); } }
        [Browsable(false)]
        public IBoneNode[] RootBones { get { return _children.Select(x => x as IBoneNode).ToArray(); } }
        [Browsable(false)]
        public IObject[] Objects { get { return new IObject[0]; } }
        [Browsable(false)]
        public int SelectedObjectIndex { get { return -1; } set { } }
        [Browsable(false)]
        public bool IsTargetModel { get { return true; } set { } }
        [Browsable(false)]
        public List<DrawCallBase> DrawCalls { get { return new List<DrawCallBase>(); } }
        [Browsable(false)]
        public bool IsRendering { get { return false; } set { } }
        [Browsable(false)]
        public bool Attached { get { return true; } }

        public override bool OnInitialize()
        {
            _name = "Skeleton";
            _endian = *(byte*)Header->_tag.Address == 0x20 ? Endian.Little : Endian.Big;
            EndianMode.SetEndian(_endian);
            _unk1 = Header->_unk1;
            _unk2 = Header->_unk2;
            _unk3 = Header->_unk3;
            _unk4 = Header->_unk4;
            _versionMin = Header->_versionMin;
            _versionMax = Header->_versionMax;
            uint boneCount = Header->_boneCount;
            EndianMode.SetEndian(Endian.Big);
            _boneDictionary = new Dictionary<uint, VBNBoneNode>();
            return boneCount > 0;
        }

        public override void OnPopulate()
        {
            EndianMode.SetEndian(_endian);
            uint count = Header->_boneCount;
            VBNFrameState* frames = Header->FrameData;
            for (int i = 0; i < count; ++i)
                new VBNBoneNode(*frames++, i).Initialize(this, (VoidPtr)Header + VBNHeader.Size + i * VBNBone.Size, VBNBone.Size);
            EndianMode.SetEndian(Endian.Big);
            _boneCache = _children.Select(x => x as VBNBoneNode).ToArray();
            foreach (VBNBoneNode bone in _boneCache)
            {
                _boneDictionary.Add(bone.HashValue, bone);

                uint id = bone._parentID;
                ResourceNode parent;
                if (id == 0x0FFFFFFF)
                    parent = this;
                else
                    parent = _boneCache[(int)id];
                if (parent != bone)
                {
                    bone._parent.Children.Remove(bone);
                    bone._parent = parent;
                    parent.Children.Add(bone);
                }
            }
            foreach (VBNBoneNode b in Children)
            {
                b.RecalcBindState(false, false);
                b.RecalcFrameState();
            }
            _boneCache = _boneCache.OrderBy(x => ((VBNBoneNode)x)._hash).ToArray();
        }

        internal static ResourceNode TryParse(DataSource source)
        {
            string tag = *(BinTag*)source.Address;
            if (tag == VBNHeader.Tag)
                return new VBNNode() { _endian = Endian.Big };
            if (tag.Reverse() == VBNHeader.Tag)
                return new VBNNode() { _endian = Endian.Little };
            return null;
        }

        public void ResetToBindState() { }
        public void ApplyCHR(CHR0Node node, float index) { }
        public void ApplySRT(SRT0Node node, float index) { }
        public void ApplySHP(SHP0Node node, float index) { }
        public void ApplyPAT(PAT0Node node, float index) { }
        public void ApplyVIS(VIS0Node node, float index) { }
        public void ApplyCLR(CLR0Node node, float index) { }
        public void ApplySCN(SCN0Node node, float index) { }
        public void RenderVertices(bool depthPass, IBoneNode weightTarget, GLCamera camera) { }
        public void RenderNormals() { }
        public void RenderBoxes(bool model, bool obj, bool bone, bool bindState) { }
        public void RenderBones(ModelPanelViewport v)
        {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.LineWidth(1.5f);

            foreach (VBNBoneNode bone in Children)
                bone.Render(IsTargetModel, v);

            GL.PopAttrib();
        }
        public void Attach() { }
        public void Detach() { }
        public void Refresh() { }
        public void PreRender(ModelPanelViewport v) { }
        public Box GetBox()
        {
            return new Box();
        }

        public void ApplyOMO(OMONode omo, float frame)
        {
            foreach (VBNBoneNode b in _boneCache)
                b._frameState = b._bindState;
            if (omo != null && frame > 0)
                foreach (OMOBoneEntryNode b in omo.Children)
                    if (_boneDictionary.ContainsKey(b._boneHash))
                    {
                        VBNBoneNode bone = _boneDictionary[b._boneHash];
                        FrameState newState = b._frameStates[(int)frame - 1];
                        if (b.HasScale)
                            bone._frameState._scale = newState._scale;
                        if (b.HasRotation)
                            bone._frameState._rotate = newState._rotate;
                        if (b.HasTranslation)
                            bone._frameState._translate = newState._translate;
                        bone._frameState.CalcTransforms();
                    }
            foreach (VBNBoneNode b in Children)
                b.RecalcFrameState();
        }
    }

    public unsafe class VBNBoneNode : ResourceNode, IBoneNode
    {
        internal VBNBone* Header { get { return (VBNBone*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.VBNBone; } }
        
        public VBNBoneNode(FrameState state, int boneIndex)
        {
            _bindState = _frameState = state;
            _boneIndex = boneIndex;
        }

        public bool _render = true;
        public uint _parentID;
        public uint _hash;
        public int _boneIndex;
        
        const string _category = "Bone";

        [Category(_category)]
        public FrameState BindState
        {
            get { return _bindState; }
            set { _bindState = value; SignalPropertyChange(); }
        }
        [Category(_category), Browsable(false)]
        public uint HashValue
        {
            get { return _hash; }
            set { _hash = value; SignalPropertyChange(); }
        }
        [Category(_category)]
        public string HashHex
        {
            get { return _hash.ToString("X8"); }
        }

        public Matrix _bindMatrix, _invBindMatrix, _frameMatrix, _invFrameMatrix;
        public FrameState _bindState, _frameState;

        [Browsable(false)]
        public bool Locked { get { return false; } set { } }
        [Browsable(false)]
        public Matrix BindMatrix { get { return _bindMatrix; } }
        [Browsable(false)]
        public Matrix InverseBindMatrix { get { return _invBindMatrix; } }
        [Browsable(false)]
        public int WeightCount { get { return 0; } set { } }
        [Browsable(false)]
        public FrameState FrameState { get { return _frameState; } set { } }
        [Browsable(false)]
        public Color NodeColor { get { return Color.Black; } set { } }
        [Browsable(false)]
        public Color BoneColor { get { return Color.Red; } set { } }
        [Browsable(false)]
        public int BoneIndex { get { return _boneIndex; } }
        [Browsable(false)]
        public List<Influence> LinkedInfluences { get { return new List<Influence>(); } }
        [Browsable(false)]
        public bool IsRendering { get { return _render; } set { _render = value; } }
        [Browsable(false)]
        public List<IMatrixNodeUser> Users { get { return new List<IMatrixNodeUser>(); } set { } }
        [Browsable(false)]
        public int NodeIndex { get { return 0; } }
        [Browsable(false)]
        public Matrix Matrix { get { return _frameMatrix; } }
        [Browsable(false)]
        public Matrix InverseMatrix { get { return _invFrameMatrix; } }
        [Browsable(false)]
        public bool IsPrimaryNode { get { return true; } }
        [Browsable(false)]
        public List<BoneWeight> Weights { get { return new List<BoneWeight>(); } }
        [Browsable(false)]
        public IModel IModel
        {
            get
            {
                ResourceNode r = Parent;
                while (r != null && r is VBNBoneNode)
                    r = r.Parent;
                return r as IModel;
            }
        }

        public override bool OnInitialize()
        {
            _parentID = Header->_parentIndex;
            _hash = Header->_hash;
            _name = Header->GetString();

            return false;
        }

        public void Render(bool targetModel, ModelPanelViewport viewport, Vector3 parentPos = default(Vector3))
        {
            if (!_render)
                return;

            //Draw name if selected
            //if (NodeColor != Color.Transparent && viewport != null)
            //{
            //    Vector3 screenPos = viewport.Camera.Project(_frameMatrix.GetPoint());
            //    viewport.ScreenText[Name] = new Vector3(screenPos._x, screenPos._y - 9.0f, screenPos._z);
            //}

            float alpha = targetModel ? 1.0f : 0.45f;

            //Set bone line color
            if (BoneColor != Color.Transparent)
                GL.Color4(BoneColor.R / 255.0f, BoneColor.G / 255.0f, BoneColor.B / 255.0f, alpha);
            //else
            //    GL.Color4(targetModel ? DefaultLineColor : DefaultLineDeselectedColor);

            //Draw bone line
            Vector3 currentPos = _frameMatrix.GetPoint();
            GL.Begin(BeginMode.Lines);
            GL.Vertex3((float*)&parentPos);
            GL.Vertex3((float*)&currentPos);
            GL.End();

            //Set bone orb color
            if (NodeColor != Color.Transparent)
                GL.Color4(NodeColor.R / 255.0f, NodeColor.G / 255.0f, NodeColor.B / 255.0f, alpha);
            //else
            //    GL.Color4(DefaultNodeColor.R / 255.0f, DefaultNodeColor.G / 255.0f, DefaultNodeColor.B / 255.0f, alpha);

            //Draw bone orb
            GL.PushMatrix();
            
            Matrix transform = _frameMatrix;

            GL.MultMatrix((float*)&transform);

            //Orb
            TKContext.FindOrCreate<GLDisplayList>("BoneNodeOrb", MDL0BoneNode.CreateNodeOrb).Call();

            //Axes
            MDL0BoneNode.DrawNodeOrients(alpha);

            GL.PopMatrix();

            //Render children
            foreach (VBNBoneNode n in Children)
                n.Render(targetModel, viewport, currentPos);
        }
        public void RecalcBindState(bool updateMesh, bool moveMeshWithBone, bool updateAssetLists = true)
        {
            if (_parent is VBNBoneNode)
            {
                _bindMatrix = ((VBNBoneNode)_parent)._bindMatrix * _bindState._transform;
                _invBindMatrix = _bindState._iTransform * ((VBNBoneNode)_parent)._invBindMatrix;
            }
            else
            {
                _bindMatrix = _bindState._transform;
                _invBindMatrix = _bindState._iTransform;
            }
            foreach (VBNBoneNode bone in Children)
                bone.RecalcBindState(updateMesh, moveMeshWithBone, updateAssetLists);
        }
        public void RecalcFrameState(ModelPanelViewport v = null)
        {
            if (_parent is VBNBoneNode)
            {
                _frameMatrix = ((VBNBoneNode)_parent)._frameMatrix * _frameState._transform;
                _invFrameMatrix = _frameState._iTransform * ((VBNBoneNode)_parent)._invFrameMatrix;
            }
            else
            {
                _frameMatrix = _frameState._transform;
                _invFrameMatrix = _frameState._iTransform;
            }
            foreach (VBNBoneNode bone in Children)
                bone.RecalcFrameState(v);
        }
    }
}