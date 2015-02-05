using BrawlLib.Modeling;
using BrawlLib.SSBBTypes;
using BrawlLib.Wii.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class FMDLNode : BFRESEntryNode, IModel
    {
        internal FMDL* Header { get { return (FMDL*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.FMDL; } }

        private InfluenceManager _influenceManager;

        public List<FMDLBoneNode> _boneCache = new List<FMDLBoneNode>();

        public override bool OnInitialize()
        {
            if (_name == null)
                _name = Header->Name;

            return true;
        }

        public override void OnPopulate()
        {
            InitGroups();

            BFRESNode bfres = BFRESNode;

            FVTX* vertices = Header->VTXArray;
            ResourceGroup* objects = Header->SHPGroup;
            ResourceGroup* materials = Header->MATGroup;
            ResourceGroup* uniforms = Header->UniformGroup;
            FSKL* skeleton = Header->Skeleton;

            for (int i = 0; i < Header->_countVTX; i++, vertices++)
                new FMDLVertexNode().Initialize(BufferGroup, vertices, 0x20);
            VoidPtr bones = skeleton->Bones;
            bushort* indices = skeleton->InverseMtxIndices;
            
            bool oldVersion = bfres.VersionMinor == 0;

            Matrix?[] matrixArray = new Matrix?[skeleton->_boneCount];

            bMatrix43* matrices = skeleton->InverseMatrices;
            if (oldVersion)
                for (int i = 0; i < skeleton->_extraIndexCount; i++, matrices++)
                    matrixArray[*indices++] = (Matrix)(*matrices);
            else
                for (int i = 0; i < skeleton->_inverseMtxIndexCount; i++)
                    matrixArray[*indices++] = Matrix.Identity;

            int size = FSKLBone.Size;
            for (int i = 0; i < skeleton->_boneCount; i++, bones += size)
            {
                size = FSKLBone.Size;
                Matrix inverse = Matrix.Identity;
                if (matrixArray[i].HasValue)
                    if (oldVersion)
                    {
                        inverse = *(bMatrix43*)(bones + size);
                        size += 0x30;
                    }
                    else
                        inverse = matrixArray[i].Value;

                new FMDLBoneNode() {
                    _inverseBindMatrix = inverse,
                    _inverseMatrix = inverse,
                }.Initialize(BoneGroup, bones, size);
            }

            _boneCache = BoneGroup.Children.Select(x => (FMDLBoneNode)x).ToList();
            foreach (FMDLBoneNode bone in _boneCache)
                if (bone.Header->_parentIndex1 >= 0)
                {
                    bone._parent._children.Remove(bone);
                    bone._parent = _boneCache[bone.Header->_parentIndex1];
                    bone._parent._children.Add(bone);
                }

            for (int i = 0; i < Header->_countMAT; i++)
                new FMDLMaterialNode().Initialize(MaterialGroup, materials->First[i].DataAddressRelative, FMAT.Size);
            for (int i = 0; i < Header->_countSHP; i++)
                new FMDLObjectNode().Initialize(ObjectGroup, objects->First[i].DataAddressRelative, FSHP.Size);
            for (int i = 0; i < Header->_countUniforms; i++)
                new FMDLUniformNode().Initialize(UniformsGroup, uniforms->First[i].DataAddressRelative, FMDLUniform.Size);

            CleanGroups();
        }

        public override void AddChild(ResourceNode child, bool change)
        {
            if (child is FMDLGroupNode)
                LinkGroup(child as FMDLGroupNode);
            base.AddChild(child, change);
        }

        public override void RemoveChild(ResourceNode child)
        {
            if (child is FMDLGroupNode)
                UnlinkGroup(child as FMDLGroupNode);
            base.RemoveChild(child);
        }

        [Browsable(false)]
        public FMDLGroupNode BoneGroup { get { return _groups[0]; } }
        [Browsable(false)]
        public FMDLGroupNode ObjectGroup { get { return _groups[1]; } }
        [Browsable(false)]
        public FMDLGroupNode MaterialGroup { get { return _groups[2]; } }
        [Browsable(false)]
        public FMDLGroupNode UniformsGroup { get { return _groups[3]; } }
        [Browsable(false)]
        public FMDLGroupNode BufferGroup { get { return _groups[4]; } }
        [Browsable(false)]
        public List<ResourceNode> BoneList { get { return _lists[0]; } }
        [Browsable(false)]
        public List<ResourceNode> ObjectList { get { return _lists[1]; } }
        [Browsable(false)]
        public List<ResourceNode> MaterialList { get { return _lists[2]; } }
        [Browsable(false)]
        public List<ResourceNode> UniformList { get { return _lists[3]; } }
        [Browsable(false)]
        public List<ResourceNode> BufferList { get { return _lists[4]; } }

        public FMDLGroupNode[] _groups = new FMDLGroupNode[(int)FMDLResourceType.Max];
        public List<ResourceNode>[] _lists = new List<ResourceNode>[(int)FMDLResourceType.Max];

        #region Linking
        public void LinkGroup(FMDLGroupNode group)
        {
            if (group == null)
                return;

            int index = (int)group._type;
            _groups[index] = group;
            _lists[index] = group._children;
        }
        public void UnlinkGroup(FMDLGroupNode group)
        {
            if (group == null)
                return;

            int index = (int)group._type;
            _groups[index] = null;
            _lists[index] = null;
        }
        internal void InitGroups()
        {
            for (int i = 0; i < (int)FMDLResourceType.Max; i++)
                LinkGroup(new FMDLGroupNode((FMDLResourceType)i) { _parent = this });
        }
        internal void CleanGroups()
        {
            for (int i = 0; i < (int)FMDLResourceType.Max; i++)
                if (_lists[i].Count > 0)
                    _children.Add(_groups[i]);
                else
                    UnlinkGroup(_groups[i]);
        }
        #endregion

        private bool _render, _isTargetModel, _attached;
        private int _selectedObjectIndex = -1;

        [Browsable(false)]
        public InfluenceManager Influences { get { return _influenceManager; } }
        [Browsable(false)]
        public IBoneNode[] BoneCache { get { return _boneCache.Select(x => (IBoneNode)x).ToArray(); } }
        [Browsable(false)]
        public IBoneNode[] RootBones { get { return new IBoneNode[0]; } }
        [Browsable(false)]
        public bool IsTargetModel
        {
            get { return _isTargetModel; }
            set { _isTargetModel = value; }
        }
        [Browsable(false)]
        public int SelectedObjectIndex
        {
            get { return _selectedObjectIndex; }
            set { _selectedObjectIndex = value; }
        }
        [Browsable(false)]
        public IObject[] Objects { get { return new IObject[0]; } }
        [Browsable(false)]
        public bool IsRendering
        {
            get { return _render; }
            set { _render = value; }
        }
        [Browsable(false)]
        public bool Attached { get { return _attached; } }

        public void ResetToBindState()
        {
            
        }

        public void ApplyCHR(CHR0Node node, float index) { }
        public void ApplySRT(SRT0Node node, float index) { }
        public void ApplySHP(SHP0Node node, float index) { }
        public void ApplyPAT(PAT0Node node, float index) { }
        public void ApplyVIS(VIS0Node node, float index) { }
        public void ApplyCLR(CLR0Node node, float index) { }
        public void ApplySCN(SCN0Node node, float index) { }

        public void RenderVertices(bool depthPass, IBoneNode weightTarget)
        {

        }

        public void RenderNormals()
        {
            
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

        ModelRenderAttributes _attributes = null;

        public void Render(params object[] args)
        {
            ModelRenderAttributes attrib;
            if (args.Length > 0 && args[0] is ModelRenderAttributes)
                attrib = (ModelRenderAttributes)args[0];
            else
                attrib = _attributes;

            if (attrib._renderBones)
            {
                foreach (FMDLBoneNode b in BoneList)
                {
                    b.RecalcFrameState();
                    b.Render(_isTargetModel);
                }
            }
        }

        public Box GetBox()
        {
            return new Box();
        }

        internal static ResourceNode TryParse(DataSource source) { return ((FMDL*)source.Address)->_tag == FMDL.Tag ? new FMDLNode() : null; }
    }
}
