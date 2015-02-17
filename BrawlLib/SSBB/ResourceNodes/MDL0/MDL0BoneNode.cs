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
    public unsafe class MDL0BoneNode : MDL0EntryNode, IBoneNode
    {
        internal MDL0Bone* Header { get { return (MDL0Bone*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.MDL0Bone; } }
        public override bool AllowDuplicateNames { get { return true; } }
        public override bool RetainChildrenOnReplace { get { return true; } }

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

        public BoneFlags _boneFlags = (BoneFlags)0x11F;
        public BillboardFlags _billboardFlags;
        //public uint _bbNodeId;

        public List<MDL0ObjectNode> _infPolys = new List<MDL0ObjectNode>();
        public List<MDL0ObjectNode> _manPolys = new List<MDL0ObjectNode>();
        public MDL0BoneNode _bbRefNode;

        public FrameState _bindState = FrameState.Neutral;
        public Matrix _bindMatrix = Matrix.Identity, _inverseBindMatrix = Matrix.Identity;
        public FrameState _frameState = FrameState.Neutral;
        public Matrix _frameMatrix = Matrix.Identity, _inverseFrameMatrix = Matrix.Identity;

        private Box _extents = new Box();
        public int _nodeIndex, _weightCount, _refCount;

        #region IBoneNode Implementation

        [Browsable(false)]
        public IModel IModel { get { return Model; } }
        [Browsable(false)]
        public FrameState FrameState { get { return _frameState; } set { _frameState = value; } }
        [Browsable(false)]
        public FrameState BindState { get { return _bindState; } set { _bindState = value; } }

        [Browsable(false)]
        public Color BoneColor { get { return _boneColor; } set { _boneColor = value; } }
        [Browsable(false)]
        public Color NodeColor { get { return _nodeColor; } set { _nodeColor = value; } }

        [Browsable(false)]
        public int WeightCount { get { return _weightCount; } set { _weightCount = value; } }

        [Browsable(false)]
        public bool Locked
        {
            get { return _locked; }
            set { _locked = value; }
        }

        [Category("Bone"), Browsable(false), TypeConverter(typeof(MatrixStringConverter))]
        public Matrix BindMatrix { get { return _bindMatrix; } set { _bindMatrix = value; SignalPropertyChange(); } }
        [Category("Bone"), Browsable(false), TypeConverter(typeof(MatrixStringConverter))]
        public Matrix InverseBindMatrix { get { return _inverseBindMatrix; } set { _inverseBindMatrix = value; SignalPropertyChange(); } }

        #region IMatrixNode Implementation

        [Category("Bone"), Browsable(false)]
        public Matrix Matrix { get { return _frameMatrix; } }
        [Category("Bone"), Browsable(false)]
        public Matrix InverseMatrix { get { return _inverseFrameMatrix; } }
        
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

        #endregion

        #endregion

        #region Properties

        [Category("Bone")]
        public MDL0ObjectNode[] AttachedObjects { get { return _manPolys.ToArray(); } }
        [Category("Bone")]
        public MDL0ObjectNode[] InfluencedObjects { get { return _infPolys.ToArray(); } }

        [Category("Bone")]
        public bool Visible
        {
            get { return _boneFlags.HasFlag(BoneFlags.Visible); }
            set
            {
                if (value)
                    _boneFlags |= BoneFlags.Visible;
                else
                    _boneFlags &= ~BoneFlags.Visible;
            }
        }
        [Category("Bone")]
        public bool SegScaleCompApply
        {
            get { return _boneFlags.HasFlag(BoneFlags.SegScaleCompApply); }
            set
            {
                if (value)
                    _boneFlags |= BoneFlags.SegScaleCompApply;
                else
                    _boneFlags &= ~BoneFlags.SegScaleCompApply;
            }
        }
        [Category("Bone")]
        public bool SegScaleCompParent
        {
            get { return _boneFlags.HasFlag(BoneFlags.SegScaleCompParent); }
            set
            {
                if (value)
                    _boneFlags |= BoneFlags.SegScaleCompParent;
                else
                    _boneFlags &= ~BoneFlags.SegScaleCompParent;
            }
        }
        [Category("Bone")]
        public bool ClassicScale
        {
            get { return !_boneFlags.HasFlag(BoneFlags.ClassicScaleOff); }
            set
            {
                if (!value)
                    _boneFlags |= BoneFlags.ClassicScaleOff;
                else
                    _boneFlags &= ~BoneFlags.ClassicScaleOff;
            }
        }
        [Category("Bone")]
        public int BoneIndex { get { return _entryIndex; } }

        [Category("Bone"), Description(@"This setting will rotate the bone and all influenced geometry in relation to the camera.
If the setting is 'Perspective', the bone's Z axis points at the camera's position.
Otherwise, the bone's Z axis is parallel to the camera's Z axis.

Standard: Default; no rotation restrictions. Is affected by the parent bone's rotation.
Rotation: The bone's Y axis is parallel to the camera's Y axis. Is NOT affected by the parent bone's rotation.
Y: Only the Y axis is allowed to rotate. Is affected by the parent bone's rotation.")]
        public BillboardFlags BillboardSetting 
        {
            get { return _billboardFlags; } 
            set 
            {
                if (_billboardFlags == value)
                    return;

                MDL0Node model = Model;
                if (_billboardFlags != BillboardFlags.Off && model._billboardBones.Contains(this))
                    model._billboardBones.Remove(this);

                if ((_billboardFlags = value) != BillboardFlags.Off && !model._billboardBones.Contains(this))
                    model._billboardBones.Add(this);

                OnBillboardModeChanged();
                SignalPropertyChange();
            }
        }

        private void OnBillboardModeChanged()
        {
            if (BillboardSetting == BillboardFlags.Off)
            {
                MDL0BoneNode n = this;
                while ((n = n.Parent as MDL0BoneNode) != null)
                    if (n.BillboardSetting != BillboardFlags.Off)
                        break;
                    
                if (n != null)
                {
                    BBRefNode = n;
                    foreach (MDL0BoneNode b in Children)
                        b.RecursiveSetBillboard(BBRefNode);
                }
                else
                {
                    BBRefNode = null;
                    foreach (MDL0BoneNode b in Children)
                        b.RecursiveSetBillboard(null);
                }
            }
            else
            {
                BBRefNode = null;
                foreach (MDL0BoneNode b in Children)
                    b.RecursiveSetBillboard(this);
            }
        }

        private void RecursiveSetBillboard(MDL0BoneNode node)
        {
            if (BillboardSetting == BillboardFlags.Off)
            {
                BBRefNode = node;
                foreach (MDL0BoneNode b in Children)
                    b.RecursiveSetBillboard(node);
            }
        }

        [Category("Bone"), TypeConverter(typeof(DropDownListBones))]
        public string BillboardRefNode
        {
            get { return _bbRefNode == null ? String.Empty : _bbRefNode.Name; }
            set
            {
                BBRefNode = String.IsNullOrEmpty(value) ? null : Model.FindBone(value);
                SignalPropertyChange();
            }
        }
        
        [Browsable(false)]
        public MDL0BoneNode BBRefNode
        {
            get { return _bbRefNode; }
            set
            {
                _bbRefNode = value;

                if (_bbRefNode != null)
                    _boneFlags |= BoneFlags.HasBillboardParent;
                else
                    _boneFlags &= ~BoneFlags.HasBillboardParent;
            }
        }

        [Category("Bone"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Scale 
        {
            get { return _bindState._scale; } 
            set 
            {
                _bindState.Scale = value;

                if (value == new Vector3(1))
                    _boneFlags |= BoneFlags.FixedScale;
                else
                    _boneFlags &= ~BoneFlags.FixedScale;

                if (value._x == value._y && value._y == value._z)
                    _boneFlags |= BoneFlags.ScaleEqual;
                else
                    _boneFlags &= ~BoneFlags.ScaleEqual;

                //RecalcBindState();
                //Model.CalcBindMatrices();
                
                if (Parent is MDL0BoneNode)
                {
                    if ((BindMatrix == ((MDL0BoneNode)Parent).BindMatrix) && (InverseBindMatrix == ((MDL0BoneNode)Parent).InverseBindMatrix))
                        _boneFlags |= BoneFlags.NoTransform;
                    else
                        _boneFlags &= ~BoneFlags.NoTransform;
                }
                else if (BindMatrix == Matrix.Identity && InverseBindMatrix == Matrix.Identity)
                    _boneFlags |= BoneFlags.NoTransform;
                else
                    _boneFlags &= ~BoneFlags.NoTransform;

                SignalPropertyChange();
            }
        }
        [Category("Bone"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Rotation 
        {
            get { return _bindState._rotate; }
            set
            {
                _bindState.Rotate = value;

                if (value == new Vector3())
                    _boneFlags |= BoneFlags.FixedRotation;
                else
                    _boneFlags &= ~BoneFlags.FixedRotation;

                //RecalcBindState();
                //Model.CalcBindMatrices();

                if (Parent is MDL0BoneNode)
                {
                    if ((BindMatrix == ((MDL0BoneNode)Parent).BindMatrix) && (InverseBindMatrix == ((MDL0BoneNode)Parent).InverseBindMatrix))
                        _boneFlags |= BoneFlags.NoTransform;
                    else
                        _boneFlags &= ~BoneFlags.NoTransform;
                }
                else if (BindMatrix == Matrix.Identity && InverseBindMatrix == Matrix.Identity)
                    _boneFlags |= BoneFlags.NoTransform;
                else
                    _boneFlags &= ~BoneFlags.NoTransform;

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
                    _boneFlags |= BoneFlags.FixedTranslation;
                else
                    _boneFlags &= ~BoneFlags.FixedTranslation;

                //RecalcBindState();
                //Model.CalcBindMatrices();

                if (Parent is MDL0BoneNode)
                {
                    if ((BindMatrix == ((MDL0BoneNode)Parent).BindMatrix) && (InverseBindMatrix == ((MDL0BoneNode)Parent).InverseBindMatrix))
                        _boneFlags |= BoneFlags.NoTransform;
                    else
                        _boneFlags &= ~BoneFlags.NoTransform;
                }
                else if (BindMatrix == Matrix.Identity && InverseBindMatrix == Matrix.Identity)
                    _boneFlags |= BoneFlags.NoTransform;
                else
                    _boneFlags &= ~BoneFlags.NoTransform;

                SignalPropertyChange(); 
            }
        }

        [Category("Bone"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 BoxMin { get { return _extents.Min; } set { _extents.Min = value; SignalPropertyChange(); } }
        [Category("Bone"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 BoxMax { get { return _extents.Max; } set { _extents.Max = value; SignalPropertyChange(); } }

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

        #endregion

        public override void OnMoved()
        {
            MDL0Node model = Model;
            model._linker = ModelLinker.Prepare(model);
            SignalPropertyChange();
        }

        internal override void GetStrings(StringTable table)
        {
            table.Add(Name);

            foreach (MDL0BoneNode n in Children)
                n.GetStrings(table);

            _userEntries.GetStrings(table);
        }

        //Initialize should only be called from parent group during parse.
        //Bones need not be imported/exported anyways
        public override bool OnInitialize()
        {
            MDL0Bone* header = Header;

            SetSizeInternal(header->_headerLen);

            if (!_replaced)
            {
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
            }

            //Conditional name assignment
            if (_name == null && header->_stringOffset != 0)
                _name = header->ResourceString;

            //Assign fields
            _boneFlags = (BoneFlags)(uint)header->_flags;
            _billboardFlags = (BillboardFlags)(uint)header->_bbFlags;
            _nodeIndex = header->_nodeId;
            _entryIndex = header->_index;

            _bbRefNode = !_replaced && _boneFlags.HasFlag(BoneFlags.HasBillboardParent) ?
                Model._linker.NodeCache[header->_bbNodeId] as MDL0BoneNode : null;

            if (_billboardFlags != BillboardFlags.Off)
                Model._billboardBones.Add(this); //Update mesh in T-Pose

            _bindState = _frameState = new FrameState(header->_scale, (Vector3)header->_rotation, header->_translation);
            _bindMatrix = _frameMatrix = header->_transform;
            _inverseBindMatrix = _inverseFrameMatrix = header->_transformInv;

            _extents = header->_extents;

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
            OnMoved();
        }

        private void RecalcOffsets(MDL0Bone* header, VoidPtr address, int length)
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
            _boneFlags = BoneFlags.Visible;

            if ((Scale._x == Scale._y) && (Scale._y == Scale._z))
                _boneFlags |= BoneFlags.ScaleEqual;
            if (_refCount > 0)
                _boneFlags |= BoneFlags.HasGeometry;
            if (Scale == new Vector3(1))
                _boneFlags |= BoneFlags.FixedScale;
            if (Rotation == new Vector3(0))
                _boneFlags |= BoneFlags.FixedRotation;
            if (Translation == new Vector3(0))
                _boneFlags |= BoneFlags.FixedTranslation;

            if (Parent is MDL0BoneNode)
            {
                if ((BindMatrix == ((MDL0BoneNode)Parent).BindMatrix) && (InverseBindMatrix == ((MDL0BoneNode)Parent).InverseBindMatrix))
                    _boneFlags |= BoneFlags.NoTransform;
            }
            else if (BindMatrix == Matrix.Identity && InverseBindMatrix == Matrix.Identity)
                _boneFlags |= BoneFlags.NoTransform;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            MDL0Bone* header = (MDL0Bone*)address;

            RecalcOffsets(header, address, length);

            if (_refCount > 0 || _weightCount > 0 || InfluencedObjects.Length > 0)
                _boneFlags |= BoneFlags.HasGeometry;
            else
                _boneFlags &= ~BoneFlags.HasGeometry;

            header->_headerLen = length;
            header->_index = _entryIndex;
            header->_nodeId = _nodeIndex;
            header->_flags = (uint)_boneFlags;
            header->_bbFlags = (uint)_billboardFlags;
            header->_bbNodeId = _bbRefNode == null ? 0 : (uint)_bbRefNode.NodeIndex;
            header->_scale = _bindState._scale;
            header->_rotation = _bindState._rotate;
            header->_translation = _bindState._translate;
            header->_extents = _extents;
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
        public void RecalcFrameState(ModelPanelViewport v = null)
        {
            if (_overrideBone != null)
            {
                _frameMatrix = _overrideBone._frameMatrix;
                _inverseFrameMatrix = _overrideBone._inverseFrameMatrix;
            }
            else
            {
                if (_overrideLocalTranslate != new Vector3())
                    _frameState = new FrameState(_frameState.Scale, _frameState.Rotate, _frameState.Translate + _overrideLocalTranslate);

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

            if (BillboardSetting != BillboardFlags.Off && 
                v != null && 
                v.ApplyBillboardBones)
                ApplyBillboard(v.Camera);

            foreach (MDL0BoneNode bone in Children)
                bone.RecalcFrameState(v);
        }

        public void ApplyBillboard(GLCamera camera)
        {
            if (BillboardSetting == BillboardFlags.Off)
                return;

            Vector3 camPoint = camera.GetPoint();
            Vector3 camRot = camera._rotation;

            FrameState worldState = _frameMatrix.Derive();

            Matrix m = Matrix.Identity, mInv = Matrix.Identity;

            Vector3 rot = ((int)BillboardSetting & 1) == 0 ? //If perspective
                worldState.Translate.LookatAngles(camPoint) * Maths._rad2degf : //Point at camera position
                camRot; //Set parallel to the camera

            switch (BillboardSetting)
            {
                case BillboardFlags.Standard:
                case BillboardFlags.StandardPerspective:

                    //Is affected by parent rotation
                    //m = Matrix.RotationMatrix(worldState.Rotate);
                    //mInv = Matrix.ReverseRotationMatrix(worldState.Rotate);

                    //No restrictions to apply
                    break;

                case BillboardFlags.Rotation:
                case BillboardFlags.RotationPerspective:
                    
                    //Is not affected by parent rotation
                    m = Matrix.RotationMatrix(_frameState.Rotate);
                    mInv = Matrix.ReverseRotationMatrix(_frameState.Rotate);

                    //TODO: apply restrictions?
                    break;

                case BillboardFlags.Y:
                case BillboardFlags.YPerspective:

                    //Is affected by parent rotation
                    m = Matrix.RotationMatrix(worldState.Rotate);
                    mInv = Matrix.ReverseRotationMatrix(worldState.Rotate);

                    //Only Y is allowed to rotate automatically
                    rot._x = 0;
                    rot._z = 0;
                    
                    break;

                default: //Not a valid billboard type
                    return;
            }

            worldState.Rotate = rot;

            _frameMatrix = worldState._transform * m;
            _inverseFrameMatrix = worldState._iTransform * mInv;
        }

        public unsafe void DrawBox(bool drawChildren, bool bindBox)
        {
            Box box = bindBox ? _extents : GetBox();

            if (bindBox)
            {
                GL.MatrixMode(MatrixMode.Modelview);
                GL.PushMatrix();
                fixed (Matrix* m = &_frameMatrix)
                    GL.MultMatrix((float*)m);
            }
            
            TKContext.DrawWireframeBox(box);

            if (bindBox)
                GL.PopMatrix();
            
            if (drawChildren)
                foreach (MDL0BoneNode b in Children)
                    b.DrawBox(true, bindBox);
        }

        public Box GetBox()
        {
            if (AttachedObjects.Length == 0)
                return new Box();

            Box box = Box.ExpandableVolume;
            foreach (MDL0ObjectNode o in AttachedObjects)
                box.ExpandVolume(o.GetBox());

            return box;
        }

        internal void SetBox()
        {
            _extents = GetBox();
            foreach (MDL0BoneNode b in Children)
                b.SetBox();
        }

        public unsafe List<MDL0BoneNode> ChildTree(List<MDL0BoneNode> list)
        {
            list.Add(this);
            foreach (MDL0BoneNode c in _children)
                c.ChildTree(list);
            
            return list;
        }

        [Browsable(false)]
        public List<Influence> LinkedInfluences { get { return _linkedInfluences; } }
        List<Influence> _linkedInfluences = new List<Influence>();

        #region Rendering

        public static Color DefaultLineColor = Color.FromArgb(0, 0, 128);
        public static Color DefaultLineDeselectedColor = Color.FromArgb(128, 0, 0);
        public static Color DefaultNodeColor = Color.FromArgb(0, 128, 0);

        public Color _boneColor = Color.Transparent;
        public Color _nodeColor = Color.Transparent;

        public const float _nodeRadius = 0.20f;

        internal void ApplyCHR0(CHR0Node node, float index)
        {
            CHR0EntryNode e;

            _frameState = _bindState;
            
            if (node != null && index >= 1 && (e = node.FindChild(Name, false) as CHR0EntryNode) != null) //Set to anim pose
                fixed (FrameState* v = &_frameState)
                {
                    float* f = (float*)v;
                    for (int i = 0; i < 9; i++)
                        if (e.Keyframes[i]._keyCount > 0)
                            f[i] = e.GetFrameValue(i, index - 1);

                    _frameState.CalcTransforms();
                }

            foreach (MDL0BoneNode b in Children)
                b.ApplyCHR0(node, index);
        }

        internal override void Bind()
        {
            _render = true;
            _boneColor = Color.Transparent;
            _nodeColor = Color.Transparent;
        }
        
        //public void Attach() { }

        //[Browsable(false)]
        //public bool Attached { get { return _attached; } }
        //private bool _attached = false;

        [Browsable(false)]
        public bool IsRendering { get { return _render; } set { _render = value; } }
        public bool _render = true;

        //public void Detach() { }

        //public void GetBox(out Vector3 min, out Vector3 max)
        //{
        //    min = max = new Vector3(0);
        //}

        //public void Refresh() { }

        public void Render(bool targetModel, GLViewport viewport)
        {
            if (!_render)
                return;

            Color c = targetModel ? DefaultLineColor : DefaultLineDeselectedColor;

            if (_boneColor != Color.Transparent)
                GL.Color4(_boneColor.R / 255.0f, _boneColor.G / 255.0f, _boneColor.B / 255.0f, targetModel ? 1.0f : 0.45f);
            else
                GL.Color4(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, targetModel ? 1.0f : 0.45f);

            //Draw name if selected
            if (_nodeColor != Color.Transparent && viewport != null && viewport is ModelPanelViewport)
            {
                Vector3 pt = _frameMatrix.GetPoint();
                Vector3 v2 = viewport.Camera.Project(pt);
                ((ModelPanelViewport)viewport).ScreenText[Name] = new Vector3(v2._x, v2._y - 9.0f, v2._z);
            }

            Vector3 v1 = (_parent == null || !(_parent is MDL0BoneNode)) ? new Vector3(0.0f) : ((MDL0BoneNode)_parent)._frameMatrix.GetPoint();
            Vector3 v = _frameMatrix.GetPoint();

            GL.Begin(PrimitiveType.Lines);

            GL.Vertex3((float*)&v1);
            GL.Vertex3((float*)&v);

            GL.End();

            GL.PushMatrix();
            {
                fixed (Matrix* m = &_frameMatrix)
                    GL.MultMatrix((float*)m);

                //Render node
                GLDisplayList ndl = TKContext.FindOrCreate<GLDisplayList>("BoneNodeOrb", CreateNodeOrb);
                if (_nodeColor != Color.Transparent)
                    GL.Color4(_nodeColor.R / 255.0f, _nodeColor.G / 255.0f, _nodeColor.B / 255.0f, targetModel ? 1.0f : 0.45f);
                else
                    GL.Color4(DefaultNodeColor.R / 255.0f, DefaultNodeColor.G / 255.0f, DefaultNodeColor.B / 255.0f, targetModel ? 1.0f : 0.45f);

                ndl.Call();

                DrawNodeOrients(targetModel);
            }
            GL.PopMatrix();

            //Render children
            foreach (MDL0BoneNode n in Children)
                n.Render(targetModel, viewport);
        }

        public static GLDisplayList CreateNodeOrb(TKContext ctx)
        {
            GLDisplayList circle = TKContext.GetRingList();
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

        public static void DrawNodeOrients(bool Strong)
        {
            GL.Begin(PrimitiveType.Lines);

            GL.Color4(1.0f, 0.0f, 0.0f, Strong ? 1.0f : 0.35f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(_nodeRadius * 2, 0.0f, 0.0f);

            GL.Color4(0.0f, 1.0f, 0.0f, Strong ? 1.0f : 0.35f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, _nodeRadius * 2, 0.0f);

            GL.Color4(0.0f, 0.0f, 1.0f, Strong ? 1.0f : 0.35f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, _nodeRadius * 2);

            GL.End();
        }

        #endregion

        public Vector3 _overrideLocalTranslate;

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
