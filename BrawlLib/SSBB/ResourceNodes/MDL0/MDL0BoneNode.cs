using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using BrawlLib.Wii.Models;
using BrawlLib.SSBBTypes;
using BrawlLib.Modeling;
using BrawlLib.OpenGL;
using System.Drawing;
using BrawlLib.Wii.Animations;
using BrawlLib.Wii.Compression;
using System.Windows;
using BrawlLib.IO;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class MDL0BoneNode : MDL0EntryNode, IMatrixNode
    {
        internal MDL0Bone* Header { get { return (MDL0Bone*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.MDL0Bone; } }
        public override bool AllowDuplicateNames { get { return true; } }
        
        private MDL0BoneNode _overrideBone;
        [Browsable(false)]
        public MDL0BoneNode OverrideBone
        {
            get { return _overrideBone; }
            set { _overrideBone = value; }
        }
        //private List<MDL0BoneNode> _overriding = new List<MDL0BoneNode>();
        //[Browsable(false)]
        //public List<MDL0BoneNode> Overriding
        //{
        //    get { return _overriding; }
        //    set { _overriding = value; }
        //}
        
        public MDL0BoneNode Clone() 
        {
            MDL0BoneNode b = new MDL0BoneNode();
            b._name = _name;
            b._bindState = new FrameState(_bindState._scale, _bindState._rotate, _bindState._translate);
            return b;
        }

        public bool _locked; //For the weight editor

        public bool _moved = false;
        [Browsable(false)]
        public bool Moved
        {
            get { return _moved;  } 
            set 
            {
                _moved = true;
                Model.SignalPropertyChange();
            } 
        }

        public BoneFlags _flags1 = (BoneFlags)0x100;
        public BillboardFlags _flags2;
        public uint _bbNodeId;

        public List<MDL0ObjectNode> _infPolys = new List<MDL0ObjectNode>();
        public List<MDL0ObjectNode> _manPolys = new List<MDL0ObjectNode>();
        [Category("Bone")]
        public MDL0ObjectNode[] AttachedObjects { get { return _manPolys.ToArray(); } }
        [Category("Bone")]
        public MDL0ObjectNode[] InfluencedObjects { get { return _infPolys.ToArray(); } }

        public FrameState _bindState = FrameState.Neutral;
        public Matrix _bindMatrix = Matrix.Identity, _inverseBindMatrix = Matrix.Identity;
        public FrameState _frameState = FrameState.Neutral;
        public Matrix _frameMatrix = Matrix.Identity, _inverseFrameMatrix = Matrix.Identity;

        private Vector3 _bMin, _bMax;
        internal int _nodeIndex, _weightCount, _refCount, _headerLen, _mdl0Offset, _stringOffset, _parentOffset, _firstChildOffset, _prevOffset, _nextOffset, _userDataOffset;

        [Category("Bone"), Browsable(false)]
        public Matrix Matrix { get { return _frameMatrix; } }
        [Category("Bone"), Browsable(false)]
        public Matrix InverseMatrix { get { return _inverseFrameMatrix; } }
        [Category("Bone"), Browsable(false), TypeConverter(typeof(MatrixStringConverter))]
        public Matrix BindMatrix { get { return _bindMatrix; } set { _bindMatrix = value; SignalPropertyChange(); } }
        [Category("Bone"), Browsable(false), TypeConverter(typeof(MatrixStringConverter))]
        public Matrix InverseBindMatrix { get { return _inverseBindMatrix; } set { _inverseBindMatrix = value; SignalPropertyChange(); } }

        [Browsable(false)]
        public int NodeIndex { get { return _nodeIndex; } }
        [Browsable(false)]
        public int ReferenceCount { get { return _refCount; } set { _refCount = value; } }
        [Browsable(false)]
        public bool IsPrimaryNode { get { return true; } }

        private List<BoneWeight> _weightRef;
        [Browsable(false)]
        public List<BoneWeight> Weights { get { return _weightRef == null ? _weightRef = new List<BoneWeight> { new BoneWeight(this, 1.0f) } : _weightRef; } }

        [Browsable(false)]
        public List<IMatrixNodeUser> Users { get { return _users; } set { _users = value; } }
        internal List<IMatrixNodeUser> _users = new List<IMatrixNodeUser>();

//#if DEBUG
        //[Category("Bone")]
        //public int HeaderLen { get { return _headerLen; } }
        //[Category("Bone")]
        //public int MDL0Offset { get { return _mdl0Offset; } }
        //[Category("Bone")]
        //public int StringOffset { get { return _stringOffset; } }
        //[Category("Bone")]
        //public int ParentOffset { get { return _parentOffset / 0xD0; } }
        //[Category("Bone")]
        //public int FirstChildOffset { get { return _firstChildOffset / 0xD0; } }
        //[Category("Bone")]
        //public int NextOffset { get { return _nextOffset / 0xD0; } }
        //[Category("Bone")]
        //public int PrevOffset { get { return _prevOffset / 0xD0; } }
        //[Category("Bone")]
        //public int UserDataOffset { get { return _userDataOffset; } }
//#endif
        
        [Category("Bone")]
        public bool Visible
        {
            get { return _flags1.HasFlag(BoneFlags.Visible); }
            set
            {
                if (value)
                    _flags1 |= BoneFlags.Visible;
                else
                    _flags1 &= ~BoneFlags.Visible;
            }
        }
        [Category("Bone")]
        public bool SegScaleCompApply
        {
            get { return _flags1.HasFlag(BoneFlags.SegScaleCompApply); }
            set
            {
                if (value)
                    _flags1 |= BoneFlags.SegScaleCompApply;
                else
                    _flags1 &= ~BoneFlags.SegScaleCompApply;
            }
        }
        [Category("Bone")]
        public bool SegScaleCompParent
        {
            get { return _flags1.HasFlag(BoneFlags.SegScaleCompParent); }
            set
            {
                if (value)
                    _flags1 |= BoneFlags.SegScaleCompParent;
                else
                    _flags1 &= ~BoneFlags.SegScaleCompParent;
            }
        }
        [Category("Bone")]
        public bool ClassicScale
        {
            get { return !_flags1.HasFlag(BoneFlags.ClassicScaleOff); }
            set
            {
                if (!value)
                    _flags1 |= BoneFlags.ClassicScaleOff;
                else
                    _flags1 &= ~BoneFlags.ClassicScaleOff;
            }
        }
        public int _boneIndex;
        [Category("Bone")]
        public int BoneIndex { get { return _boneIndex; } }
        [Category("Bone"), Browsable(false)]
        public int NodeId { get { return _nodeIndex; } }
        [Category("Bone"), Browsable(false)]
        public BoneFlags Flags { get { return _flags1; } set { _flags1 = (BoneFlags)(int)value; SignalPropertyChange(); } }

        [Category("Bone")]
        public bool HasBillboardParent
        {
            get { return _flags1.HasFlag(BoneFlags.HasBillboardParent); }
            set
            {
                if (value)
                    _flags1 |= BoneFlags.HasBillboardParent;
                else
                    _flags1 &= ~BoneFlags.HasBillboardParent;
            }
        }
        [Category("Bone")]
        public BillboardFlags BillboardSetting 
        {
            get { return _flags2; } 
            set 
            {
                if (_flags2 != 0 && Model._billboardBones.Contains(this))
                    Model._billboardBones.Remove(this);
                _flags2 = (BillboardFlags)(int)value;
                if (_flags2 != 0 && _flags1.HasFlag(BoneFlags.HasGeometry) && !Model._billboardBones.Contains(this))
                    Model._billboardBones.Add(this);
                SignalPropertyChange();
            }
        }
        [Category("Bone")]
        public uint BillboardRefNodeId { get { return _bbNodeId; } set { _bbNodeId = value; SignalPropertyChange(); } }
        
        [Category("Bone"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Scale 
        {
            get { return _bindState._scale; } 
            set 
            {
                _bindState.Scale = value;

                if (value == new Vector3(1))
                    _flags1 |= BoneFlags.FixedScale;
                else
                    _flags1 &= ~BoneFlags.FixedScale;

                if (value._x == value._y && value._y == value._z)
                    _flags1 |= BoneFlags.ScaleEqual;
                else
                    _flags1 &= ~BoneFlags.ScaleEqual;

                //RecalcBindState();
                //Model.CalcBindMatrices();
                
                if (Parent is MDL0BoneNode)
                {
                    if ((BindMatrix == ((MDL0BoneNode)Parent).BindMatrix) && (InverseBindMatrix == ((MDL0BoneNode)Parent).InverseBindMatrix))
                        _flags1 |= BoneFlags.NoTransform;
                    else
                        _flags1 &= ~BoneFlags.NoTransform;
                }
                else if (BindMatrix == Matrix.Identity && InverseBindMatrix == Matrix.Identity)
                    _flags1 |= BoneFlags.NoTransform;
                else
                    _flags1 &= ~BoneFlags.NoTransform;

                SignalPropertyChange();
            }
        }

        //[Category("Bone"), TypeConverter(typeof(Vector3StringConverter))]
        //public Quaternion QuaternionRotation { get { return _bindState._quaternion; } set { _bindState.QuaternionRotate = value; flagsChanged = true; SignalPropertyChange(); } }
        
        [Category("Bone"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Rotation 
        {
            get { return _bindState._rotate; }
            set
            {
                _bindState.Rotate = value;

                if (value == new Vector3())
                    _flags1 |= BoneFlags.FixedRotation;
                else
                    _flags1 &= ~BoneFlags.FixedRotation;

                //RecalcBindState();
                //Model.CalcBindMatrices();

                if (Parent is MDL0BoneNode)
                {
                    if ((BindMatrix == ((MDL0BoneNode)Parent).BindMatrix) && (InverseBindMatrix == ((MDL0BoneNode)Parent).InverseBindMatrix))
                        _flags1 |= BoneFlags.NoTransform;
                    else
                        _flags1 &= ~BoneFlags.NoTransform;
                }
                else if (BindMatrix == Matrix.Identity && InverseBindMatrix == Matrix.Identity)
                    _flags1 |= BoneFlags.NoTransform;
                else
                    _flags1 &= ~BoneFlags.NoTransform;

                SignalPropertyChange();
            }
        }
        [Category("Bone"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Translation 
        {
            get { return _bindState._translate; }
            set
            {
                _bindState.Translate = value;

                if (value == new Vector3())
                    _flags1 |= BoneFlags.FixedTranslation;
                else
                    _flags1 &= ~BoneFlags.FixedTranslation;

                //RecalcBindState();
                //Model.CalcBindMatrices();

                if (Parent is MDL0BoneNode)
                {
                    if ((BindMatrix == ((MDL0BoneNode)Parent).BindMatrix) && (InverseBindMatrix == ((MDL0BoneNode)Parent).InverseBindMatrix))
                        _flags1 |= BoneFlags.NoTransform;
                    else
                        _flags1 &= ~BoneFlags.NoTransform;
                }
                else if (BindMatrix == Matrix.Identity && InverseBindMatrix == Matrix.Identity)
                    _flags1 |= BoneFlags.NoTransform;
                else
                    _flags1 &= ~BoneFlags.NoTransform;

                SignalPropertyChange(); 
            }
        }

        [Category("Bone"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 BoxMin { get { return _bMin; } set { _bMin = value; SignalPropertyChange(); } }
        [Category("Bone"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 BoxMax { get { return _bMax; } set { _bMax = value; SignalPropertyChange(); } }

        //[Category("Kinect Settings"), Browsable(true)]
        //public SkeletonJoint Joint
        //{
        //    get { return _joint; }
        //    set { _joint = value; }
        //}
        //public SkeletonJoint _joint;

        [Category("User Data"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public UserDataCollection UserEntries { get { return _userEntries; } set { _userEntries = value; SignalPropertyChange(); } }
        internal UserDataCollection _userEntries = new UserDataCollection();
        
        internal override void GetStrings(StringTable table)
        {
            table.Add(Name);

            foreach (MDL0BoneNode n in Children)
                n.GetStrings(table);

            foreach (UserDataClass s in _userEntries)
            {
                table.Add(s._name);
                if (s._type == UserValueType.String && s._entries.Count > 0)
                    table.Add(s._entries[0]);
            }
        }

        //Initialize should only be called from parent group during parse.
        //Bones need not be imported/exported anyways
        public override bool OnInitialize()
        {
            MDL0Bone* header = Header;

            SetSizeInternal(header->_headerLen);

            //Assign true parent using parent header offset
            int offset = header->_parentOffset;
            //Offsets are always < 0, because parent entries are listed before children
            if (offset < 0)
            {
                //Get address of parent header
                MDL0Bone* pHeader = (MDL0Bone*)((byte*)header + offset);
                //Search bone list for matching header
                foreach (MDL0BoneNode bone in Parent._children)
                    if (pHeader == bone.Header)
                    {
                        _parent = bone;
                        break;
                    }
            }

            //Conditional name assignment
            if ((_name == null) && (header->_stringOffset != 0))
                _name = header->ResourceString;

            //Assign fields
            _flags1 = (BoneFlags)(uint)header->_flags;
            _flags2 = (BillboardFlags)(uint)header->_bbFlags;
            _bbNodeId = header->_bbNodeId;
            _nodeIndex = header->_nodeId;
            _boneIndex = header->_index;
            _headerLen = header->_headerLen;
            _mdl0Offset = header->_mdl0Offset;
            _stringOffset = header->_stringOffset;
            _parentOffset = header->_parentOffset;
            _firstChildOffset = header->_firstChildOffset;
            _nextOffset = header->_nextOffset;
            _prevOffset = header->_prevOffset;
            _userDataOffset = header->_userDataOffset;

            if (_flags2 != 0 && _flags1.HasFlag(BoneFlags.HasGeometry))
                Model._billboardBones.Add(this); //Update mesh in T-Pose

            _bindState = _frameState = new FrameState(header->_scale, (Vector3)header->_rotation, header->_translation);
            _bindMatrix = _frameMatrix = header->_transform;
            _inverseBindMatrix = _inverseFrameMatrix = header->_transformInv;

            _bMin = header->_boxMin;
            _bMax = header->_boxMax;

            (_userEntries = new UserDataCollection()).Read(header->UserDataAddress);
            
            //We don't want to process children because not all have been parsed yet.
            //Child assigning will be handled by the parent group.
            return false;
        }

        //Use MoveRaw without processing children.
        //Prevents addresses from changing before completion.
        //internal override void MoveRaw(VoidPtr address, int length)
        //{
        //    Memory.Move(address, WorkingSource.Address, (uint)length);
        //    DataSource newsrc = new DataSource(address, length);
        //    if (_compression == CompressionType.None)
        //    {
        //        _replSrc.Close();
        //        _replUncompSrc.Close();
        //        _replSrc = _replUncompSrc = newsrc;
        //    }
        //    else
        //    {
        //        _replSrc.Close();
        //        _replSrc = newsrc;
        //    }
        //}

        public override int OnCalculateSize(bool force)
        {
            int len = 0xD0;
            len += _userEntries.GetSize();
            return len;
        }

        public override void RemoveChild(ResourceNode child)
        {
            base.RemoveChild(child);
            Moved = true;
        }

        public void RecalcOffsets(MDL0Bone* header, VoidPtr address, int length)
        {
            MDL0BoneNode bone;
            int index = 0, offset;
            
            //Sub-entries
            if (_userEntries.Count > 0)
            {
                header->_userDataOffset = 0xD0;
                _userEntries.Write(address + 0xD0);
            }
            else
                header->_userDataOffset = 0;

            //Set first child
            if (_children.Count > 0)
                header->_firstChildOffset = length;
            else
                header->_firstChildOffset = 0;

            if (_parent != null)
            {
                index = Parent._children.IndexOf(this);

                //Parent
                if (Parent is MDL0BoneNode)
                    header->_parentOffset = (int)Parent.WorkingUncompressed.Address - (int)address;
                else
                    header->_parentOffset = 0;

                //Prev
                if (index == 0)
                    header->_prevOffset = 0;
                else
                {
                    //Link to prev
                    bone = Parent._children[index - 1] as MDL0BoneNode;
                    offset = (int)bone.Header - (int)address;
                    header->_prevOffset = offset;
                    bone.Header->_nextOffset = -offset;
                }

                //Next
                if (index == (Parent._children.Count - 1))
                    header->_nextOffset = 0;
            }
        }

        public void CalcFlags()
        {
            _flags1 = BoneFlags.Visible;

            if ((Scale._x == Scale._y) && (Scale._y == Scale._z))
                _flags1 += (int)BoneFlags.ScaleEqual;
            if (_refCount > 0)
                _flags1 += (int)BoneFlags.HasGeometry;
            if (Scale == new Vector3(1))
                _flags1 += (int)BoneFlags.FixedScale;
            if (Rotation == new Vector3(0))
                _flags1 += (int)BoneFlags.FixedRotation;
            if (Translation == new Vector3(0))
                _flags1 += (int)BoneFlags.FixedTranslation;

            if (Parent is MDL0BoneNode)
            {
                if ((BindMatrix == ((MDL0BoneNode)Parent).BindMatrix) && (InverseBindMatrix == ((MDL0BoneNode)Parent).InverseBindMatrix))
                    _flags1 += (int)BoneFlags.NoTransform;
            }
            else if (BindMatrix == Matrix.Identity && InverseBindMatrix == Matrix.Identity)
                _flags1 += (int)BoneFlags.NoTransform;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            MDL0Bone* header = (MDL0Bone*)address;

            RecalcOffsets(header, address, length);

            if (_refCount > 0 || _weightCount > 0 || InfluencedObjects.Length > 0)
                _flags1 |= BoneFlags.HasGeometry;
            else
                _flags1 &= ~BoneFlags.HasGeometry;

            header->_headerLen = length;
            header->_index = _boneIndex = _entryIndex;
            header->_nodeId = _nodeIndex;
            header->_flags = (uint)_flags1;
            header->_bbFlags = (uint)_flags2;
            header->_bbNodeId = _bbNodeId;
            header->_scale = _bindState._scale;
            header->_rotation = _bindState._rotate;
            header->_translation = _bindState._translate;
            header->_boxMin = _bMin;
            header->_boxMax = _bMax;
            header->_transform = (bMatrix43)_bindMatrix;
            header->_transformInv = (bMatrix43)_inverseBindMatrix;

            _moved = false;
        }

        protected internal override void PostProcess(VoidPtr mdlAddress, VoidPtr dataAddress, StringTable stringTable)
        {
            MDL0Bone* header = (MDL0Bone*)dataAddress;
            header->MDL0Address = mdlAddress;
            header->ResourceStringAddress = stringTable[Name] + 4;

            _userEntries.PostProcess(dataAddress + 0xD0, stringTable);
        }

        //Change has been made to bind state, need to recalculate matrices
        public void RecalcBindState()
        {
            if (_parent is MDL0BoneNode)
            {
                _bindMatrix = ((MDL0BoneNode)_parent)._bindMatrix * _bindState._transform;
                _inverseBindMatrix = _bindState._iTransform * ((MDL0BoneNode)_parent)._inverseBindMatrix;
            }
            else
            {
                _bindMatrix = _bindState._transform;
                _inverseBindMatrix = _bindState._iTransform;
            }
            
            foreach (MDL0BoneNode bone in Children)
                bone.RecalcBindState();

            SignalPropertyChange();
        }
        public void RecalcFrameState()
        {
            if (_overrideBone != null)
            {
                _frameMatrix = _overrideBone._frameMatrix;
                _inverseFrameMatrix = _overrideBone._inverseFrameMatrix;
            }
            else
            {
                if (_overrideTranslate != new Vector3())
                    _frameState = new FrameState(_frameState.Scale, _frameState.Rotate, _frameState.Translate + _overrideTranslate);

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
            }

            //if (_overriding.Count != 0)
            //    foreach (MDL0BoneNode b in _overriding)
            //        b.RecalcFrameState();

            if (BillboardSetting == BillboardFlags.PerspectiveSTD)
                MuliplyRotation();

            foreach (MDL0BoneNode bone in Children)
                bone.RecalcFrameState();
        }

        public void MuliplyRotation()
        {
            if (Model._mainWindow != null)
            {
                Vector3 center = _frameMatrix.GetPoint();
                Vector3 cam = Model._mainWindow._camera.GetPoint();
                Vector3 scale = new Vector3(1);
                Vector3 rot = new Vector3();
                Vector3 trans = new Vector3();

                if (BillboardSetting == BillboardFlags.PerspectiveSTD)
                    rot = center.LookatAngles(cam) * Maths._rad2degf;

                _frameMatrix *= Matrix.TransformMatrix(scale, rot, trans);
                _inverseFrameMatrix *= Matrix.ReverseTransformMatrix(scale, rot, trans);
            }

            //foreach (MDL0BoneNode bone in Children)
            //    bone.MuliplyRotation();
        }

        public unsafe List<MDL0BoneNode> ChildTree(List<MDL0BoneNode> list)
        {
            list.Add(this);
            foreach (MDL0BoneNode c in _children)
                c.ChildTree(list);
            
            return list;
        }

        #region Rendering

        public static Color DefaultBoneColor = Color.FromArgb(0, 0, 128);
        public static Color DefaultNodeColor = Color.FromArgb(0, 128, 0);

        public Color _boneColor = Color.Transparent;
        public Color _nodeColor = Color.Transparent;

        public const float _nodeRadius = 0.20f;

        public bool _render = true;
        internal unsafe void Render(TKContext ctx, ModelPanel mainWindow)
        {
            if (!_render)
                return;

            if (_boneColor != Color.Transparent)
                GL.Color4(_boneColor.R / 255.0f, _boneColor.G / 255.0f, _boneColor.B / 255.0f, 1.0f);
            else
                GL.Color4(DefaultBoneColor.R / 255.0f, DefaultBoneColor.G / 255.0f, DefaultBoneColor.B / 255.0f, 1.0f);

            //GL.LineWidth(1.0f);

            //Draw name if selected
            if (mainWindow != null && _nodeColor != Color.Transparent)
            {
                Vector3 pt = _frameMatrix.GetPoint();
                Vector3 v2 = mainWindow.Project(pt);
                mainWindow.ScreenText[Name] = new Vector3(v2._x, v2._y - 9.0f, v2._z);
            }

            Vector3 v1 = (_parent == null || !(_parent is MDL0BoneNode)) ? new Vector3(0.0f) : ((MDL0BoneNode)_parent)._frameMatrix.GetPoint();
            Vector3 v = _frameMatrix.GetPoint();

            GL.Begin(BeginMode.Lines);

            GL.Vertex3((float*)&v1);
            GL.Vertex3((float*)&v);

            GL.End();

            GL.PushMatrix();

            fixed (Matrix* m = &_frameMatrix)
                GL.MultMatrix((float*)m);

            //Render node
            GLDisplayList ndl = ctx.FindOrCreate<GLDisplayList>("BoneNodeOrb", CreateNodeOrb);
            if (_nodeColor != Color.Transparent)
                GL.Color4(_nodeColor.R / 255.0f, _nodeColor.G / 255.0f, _nodeColor.B / 255.0f, 1.0f);
            else
                GL.Color4(DefaultNodeColor.R / 255.0f, DefaultNodeColor.G / 255.0f, DefaultNodeColor.B / 255.0f, 1.0f);
            
            ndl.Call();
            
            DrawNodeOrients();

            if (BillboardSetting != 0 && mainWindow != null)
            {
                Vector3 center = _frameMatrix.GetPoint();
                Vector3 cam = mainWindow._camera.GetPoint();
                Matrix m2 = new Matrix();
                Vector3 scale = new Vector3(1);
                Vector3 rot = new Vector3();
                Vector3 trans = new Vector3();

                if (BillboardSetting == BillboardFlags.PerspectiveSTD)
                    rot = center.LookatAngles(cam) * Maths._rad2degf;

                m2 = Matrix.TransformMatrix(scale, rot, trans);
                GL.PushMatrix();
                GL.MultMatrix((float*)&m2);
            }

            if (BillboardSetting != 0 && mainWindow != null)
                GL.PopMatrix();

            GL.PopMatrix();

            //Render children
            foreach (MDL0BoneNode n in Children)
                n.Render(ctx, mainWindow);
        }

        internal void ApplyCHR0(CHR0Node node, int index, bool linear)
        {
            CHR0EntryNode e;

            _frameState = _bindState;

            if (node != null && index > 0 && (e = node.FindChild(Name, false) as CHR0EntryNode) != null) //Set to anim pose
                fixed (FrameState* v = &_frameState)
                {
                    float* f = (float*)v;
                    for (int i = 0; i < 9; i++)
                        if (e.Keyframes[(KeyFrameMode)(i + 0x10)] > 0)
                            f[i] = e.GetFrameValue((KeyFrameMode)(i + 0x10), index - 1, linear, node.Loop);

                    _frameState.CalcTransforms();
                }

            foreach (MDL0BoneNode b in Children)
                b.ApplyCHR0(node, index, linear);
        }

        public static GLDisplayList CreateNodeOrb(TKContext ctx)
        {
            GLDisplayList circle = ctx.GetRingList();
            GLDisplayList orb = new GLDisplayList();

            orb.Begin();
            GL.PushMatrix();

            GL.Scale(_nodeRadius, _nodeRadius, _nodeRadius);
            circle.Call();
            GL.Rotate(90.0f, 0.0f, 1.0f, 0.0f);
            circle.Call();
            GL.Rotate(90.0f, 1.0f, 0.0f, 0.0f);
            circle.Call();

            GL.PopMatrix();
            orb.End();
            return orb;
        }

        public static void DrawNodeOrients()
        {
            GL.Begin(BeginMode.Lines);

            GL.Color4(1.0f, 0.0f, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(_nodeRadius * 2, 0.0f, 0.0f);

            GL.Color4(0.0f, 1.0f, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, _nodeRadius * 2, 0.0f);

            GL.Color4(0.0f, 0.0f, 1.0f, 1.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, _nodeRadius * 2);

            GL.End();
        }

        internal override void Bind(TKContext ctx)
        {
            _render = true;
            _boneColor = Color.Transparent;
            _nodeColor = Color.Transparent;
        }

        #endregion

        public Vector3 _overrideTranslate;

        //public FrameState BindState 
        //{
        //    get { return _bindState; }
        //    set
        //    {
        //        _bindState = value;
        //        _bindState.CalcTransforms();
        //        RecalcBindState();
        //        SignalPropertyChange();

        //        //Apply bindmatrix difference to vertex positions bound to only this bone
        //    }
        //}
    }
}
