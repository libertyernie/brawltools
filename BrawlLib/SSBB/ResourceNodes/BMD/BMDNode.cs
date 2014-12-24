using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using BrawlLib.SSBBTypes;
using BrawlLib.OpenGL;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Imaging;
using BrawlLib.Modeling;
using BrawlLib.Wii.Models;
using BrawlLib.Wii.Animations;
using System.Windows.Forms;
using BrawlLib.Wii.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class BMDNode : J3DNode, IModel
    {
        public override ResourceType ResourceType { get { return ResourceType.BMD; } }

        public BMDNode()
        {
            _version = 3;
        }

        [Category("BMD Model")]
        public uint VertexCount { get { return _vertexCount; } }
        [Category("BMD Model")]
        public uint Unknown { get { return _unknown; } }

        uint _unknown;
        uint _vertexCount;
        public UnsafeBuffer[] _assets = new UnsafeBuffer[12];
        public InfluenceManager _influences = new InfluenceManager();
        BMDBoneNode[] _boneCache;
        IMatrixNode[] _nodes;

        public BMDGroupNode _boneGroup, _matGroup, _objGroup, _texGroup, _regGroup;
        public List<ResourceNode> _boneList, _matList, _objList, _texList, _regList;

        #region Immediate Accessors
        [Browsable(false)]
        public ResourceNode BoneGroup { get { return _boneGroup; } }
        [Browsable(false)]
        public ResourceNode MaterialGroup { get { return _matGroup; } }
        [Browsable(false)]
        public ResourceNode RegisterGroup { get { return _regGroup; } }
        [Browsable(false)]
        public ResourceNode PolygonGroup { get { return _objGroup; } }
        [Browsable(false)]
        public ResourceNode TextureGroup { get { return _texGroup; } }
        [Browsable(false)]
        public List<ResourceNode> BoneList { get { return _boneList; } }
        [Browsable(false)]
        public List<ResourceNode> MaterialList { get { return _matList; } }
        [Browsable(false)]
        public List<ResourceNode> RegisterList { get { return _regList; } }
        [Browsable(false)]
        public List<ResourceNode> PolygonList { get { return _objList; } }
        [Browsable(false)]
        public List<ResourceNode> TextureList { get { return _texList; } }
        #endregion

        #region Linking
        public void LinkGroup(BMDGroupNode group)
        {
            switch (group.GroupType)
            {
                case BMDResourceType.Bones: { _boneGroup = group; _boneList = group._children; break; }
                case BMDResourceType.Materials: { _matGroup = group; _matList = group._children; break; }
                case BMDResourceType.Objects: { _objGroup = group; _objList = group._children; break; }
                case BMDResourceType.Registers: { _regGroup = group; _regList = group._children; break; }
                case BMDResourceType.Textures: { _texGroup = group; _texList = group._children; break; }
            }
        }
        public void UnlinkGroup(BMDGroupNode group)
        {
            if (group != null)
                switch (group.GroupType)
                {
                    case BMDResourceType.Bones: { _boneGroup = null; _boneList = null; break; }
                    case BMDResourceType.Materials: { _matGroup = null; _matList = null; break; }
                    case BMDResourceType.Objects: { _objGroup = null; _objList = null; break; }
                    case BMDResourceType.Registers: { _regGroup = null; _regList = null; break; }
                    case BMDResourceType.Textures: { _texGroup = null; _texList = null; break; }
                }
        }
        internal void InitGroups()
        {
            LinkGroup(new BMDGroupNode(BMDResourceType.Bones));
            LinkGroup(new BMDGroupNode(BMDResourceType.Materials));
            LinkGroup(new BMDGroupNode(BMDResourceType.Registers));
            LinkGroup(new BMDGroupNode(BMDResourceType.Objects));
            LinkGroup(new BMDGroupNode(BMDResourceType.Textures));

            _boneGroup._parent = this;
            _matGroup._parent = this;
            _regGroup._parent = this;
            _objGroup._parent = this;
            _texGroup._parent = this;
        }
        internal void CleanGroups()
        {
            if (_boneList.Count > 0)
                _children.Add(_boneGroup);
            else
                UnlinkGroup(_boneGroup);

            if (_matList.Count > 0)
                _children.Add(_matGroup);
            else
                UnlinkGroup(_matGroup);

            if (_regList.Count > 0)
                _children.Add(_regGroup);
            else
                UnlinkGroup(_regGroup);

            if (_objList.Count > 0)
                _children.Add(_objGroup);
            else
                UnlinkGroup(_objGroup);

            if (_texList.Count > 0)
                _children.Add(_texGroup);
            else
                UnlinkGroup(_texGroup);
        }
        #endregion

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _influences = new InfluenceManager();
            
            return Header->_sectionCount > 0;
        }

        public override void OnPopulate()
        {
            InitGroups();

            BMDInfoHeader* infHeader = null;
            BMDJointsHeader* jntHeader = null;
            BMDObjectsHeader* shpHeader = null;
            BMDMaterialsHeader* matHeader = null;
            BMDDrawHeader* drwHeader = null;
            BMDEnvelopesHeader* evpHeader = null;
            BMDVertexArrayHeader* vtxHeader = null;
            BMDRegistersHeader* mdlHeader = null;
            BMDTexturesHeader* texHeader = null;
            
            //Retrieve all the section headers
            BMDCommonHeader* cmnHdr = (BMDCommonHeader*)((VoidPtr)Header + J3DCommonHeader.Size);
            for (int i = 0; i < Header->_sectionCount; i++, cmnHdr = (BMDCommonHeader*)((VoidPtr)cmnHdr + cmnHdr->_sectionSize))
            {
                string tag = cmnHdr->_tag.ToString().Substring(0, 3);
                switch (tag)
                {
                    case "INF": infHeader = (BMDInfoHeader*)cmnHdr; break;
                    case "VTX": vtxHeader = (BMDVertexArrayHeader*)cmnHdr; break;
                    case "EVP": evpHeader = (BMDEnvelopesHeader*)cmnHdr; break;
                    case "DRW": drwHeader = (BMDDrawHeader*)cmnHdr; break;
                    case "JNT": jntHeader = (BMDJointsHeader*)cmnHdr; break;
                    case "SHP": shpHeader = (BMDObjectsHeader*)cmnHdr; break;
                    case "MAT": matHeader = (BMDMaterialsHeader*)cmnHdr; break;
                    case "MDL": mdlHeader = (BMDRegistersHeader*)cmnHdr; break;
                    case "TEX": texHeader = (BMDTexturesHeader*)cmnHdr; break;
                }
            }

            //Parse material related information
            if (texHeader != null)
                ParseTEX(texHeader);
            if (mdlHeader != null)
                ParseMDL(mdlHeader);
            if (matHeader != null)
                ParseMAT(matHeader);

            //Parse object headers (not data)
            if (shpHeader != null)
                ParseSHP(shpHeader);

            //Parse info tree to link bones and assign materials and bones to objects
            ParseInfoTree(infHeader, jntHeader);

            //Finally, parse mesh data
            ParseVTX(vtxHeader);
            ParsePrimitives(shpHeader, drwHeader, evpHeader);

            CleanGroups();
        }

        #region Parsing

        private void ParseSHP(BMDObjectsHeader* shp)
        {
            for (int i = 0; i < shp->_count; i++)
                new BMDObjectNode().Initialize(_objGroup, &shp->Objects[i], BMDObjectEntry.Size);
        }

        private void ParsePrimitives(BMDObjectsHeader* shp, BMDDrawHeader* drw, BMDEnvelopesHeader* evp)
        {
            _nodes = new IMatrixNode[drw->_count];
            
            for (int i = 0; i < _nodes.Length; i++)
            {
                ushort index = drw->Data[i];
                if (drw->GetIsWeighted(i))
                {
                    Influence inf = new Influence();
                    ushort[] boneIndices = evp->GetIndices(index);
                    float[] weights = evp->GetWeights(index);
                    int z = 0;
                    foreach (ushort x in boneIndices)
                        inf.AddWeight(new BoneWeight(_boneCache[x], weights[z++]));

                    ((Influence)(_nodes[i] = _influences.FindOrCreate(inf, true)))._index = i;
                }
                else
                {
                    BMDBoneNode b = _boneCache[index];
                    b._index = i;
                    b._bindMatrix = b._frameMatrix = evp->GetMatrix(index);

                    _nodes[i] = b;
                }
            }

            foreach (BMDObjectNode o in _objList)
                o._manager = new PrimitiveManager(shp, o.Header, _assets, _nodes);
        }

        private void ParseMAT(BMDMaterialsHeader* mat)
        {
            for (int i = 0; i < mat->_count; i++)
                new BMDMaterialNode().Initialize(_matGroup, mat, 0);
        }

        private void ParseMDL(BMDRegistersHeader* mdl)
        {

        }

        public List<BMDObjectNode> _objects;

        private void RecursiveParseTree(
            BMDEntryNode parent,
            ref BMDInfoEntry* entry,
            BMDJointsHeader* jnt,
            BMDBoneNode currentBone,
            BMDMaterialNode currentMaterial)
        {
            while (entry->Type != INFType.Terminator)
            {
                switch (entry->Type)
                {
                    case INFType.HierarchyDown:
                        entry++;
                        RecursiveParseTree(currentBone, ref entry, jnt, currentBone, currentMaterial);
                        continue;
                    case INFType.HierarchyUp:
                        entry++;
                        return;
                    case INFType.Bone:
                    case INFType.Object:
                    case INFType.Material:

                        int index = entry->_index;
                        int type = entry->_type & 0xF;

                        switch (type)
                        {
                            case 0: //Bone

                                if (parent == null)
                                    throw new Exception("Null bone parent when parsing INF");

                                (currentBone = _boneCache[index] = new BMDBoneNode()
                                {
                                    _index = index,
                                    _name = GetString(jnt->StringIDs[index], jnt->StringTable)
                                }
                                ).Initialize(parent, &jnt->Joints[index], BMDJointEntry.Size);
                                break;
                            case 1: //Material
                                if (index >= 0 && index < _matList.Count)
                                    currentMaterial = _matList[index] as BMDMaterialNode;
                                break;
                            case 2: //Object
                                if (index >= 0 && index < _objList.Count)
                                {
                                    BMDObjectNode obj = _objList[index] as BMDObjectNode;
                                    if (obj != null)
                                    {
                                        obj.BoneNode = currentBone;
                                        obj.MaterialNode = currentMaterial;
                                    }
                                }
                                break;
                        }

                        break;
                }
                entry++;
            }
        }

        private void ParseInfoTree(BMDInfoHeader* inf, BMDJointsHeader* jnt)
        {
            _unknown = inf->_unknown;
            _vertexCount = inf->_numVertices;

            _boneCache = new BMDBoneNode[jnt->_count];

            BMDInfoEntry* entry = inf->Entries;
            RecursiveParseTree(_boneGroup, ref entry, jnt, null, null);
        }
        private void ParseVTX(BMDVertexArrayHeader* vtx)
        {
            BMDArrayFormat* fmt = vtx->Formats;

            for (int x = 0; x < 13; x++)
            {
                VoidPtr addr = vtx->GetAddress(x);

                if (!addr)
                    continue;

                uint length = 0;
                uint startOffset = vtx->Offsets[x];
                for (int i = x + 1; i < 13; ++i)
                {
                    uint offset = vtx->Offsets[i];
                    if (offset != 0)
                    {
                        length = offset - startOffset;
                        break;
                    }
                }
                if (length == 0)
                    length = vtx->_header._sectionSize - startOffset;

                GXAttribute attr = fmt->ArrayType;
                UnsafeBuffer buffer = null;

                switch (attr)
                {
                    case GXAttribute.Position:
                    case GXAttribute.Normal:
                    case GXAttribute.Tex0:
                    case GXAttribute.Tex1:
                    case GXAttribute.Tex2:
                    case GXAttribute.Tex3:
                    case GXAttribute.Tex4:
                    case GXAttribute.Tex5:
                    case GXAttribute.Tex6:
                    case GXAttribute.Tex7:
                        buffer = VertexCodec.Decode(addr, fmt->_divisor, length, fmt->VertexComponentType, attr > GXAttribute.Normal, fmt->_isSpecial);
                        break;
                    case GXAttribute.Color0:
                    case GXAttribute.Color1:
                        buffer = ColorCodec.Decode(addr, length, fmt->ColorComponentType);
                        break;
                }
                if (buffer != null)
                    if (attr >= GXAttribute.Position && attr <= GXAttribute.Tex7)
                    {
                        int id = (int)(attr - GXAttribute.Position);
                        _assets[id] = buffer;
                    }
                    else
                        throw new Exception("Non-asset array type");
                fmt++;
            }
        }

        public static string GetString(int index, VoidPtr stringTable) { return new String((sbyte*)(stringTable + *((bushort*)stringTable + 2 + index * 2 + 1))); }

        private void ParseTEX(BMDTexturesHeader* tex)
        {
            for (int x = 0; x < tex->_count; x++)
            {
                new BTINode() { _name = GetString(x, tex->StringTable) }.Initialize(_texGroup, new DataSource(&tex->TextureHeaders[x], 0));
            }
        }

        #endregion

        public static BMDNode FromFile(string path)
        {
            if (path.EndsWith(".bmd", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".bdl", StringComparison.OrdinalIgnoreCase))
                return NodeFactory.FromFile(null, path) as BMDNode;

            throw new NotSupportedException("The file extension specified is not of a supported model type.");
        }

        public override unsafe void Export(string outPath)
        {
            //if (outPath.ToUpper().EndsWith(".DAE"))
            //    Collada.Serialize(this, outPath);
            //else
                base.Export(outPath);
        }

        public override int OnCalculateSize(bool force)
        {
            return 0;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {

        }

        #region Rendering

        public void GetBox(out Vector3 min, out Vector3 max)
        {
            min = max = new Vector3(0);
        }

        public ModelRenderAttributes _renderAttribs = new ModelRenderAttributes();
        private bool _attached = false;
        private int _objectIndex = -1;

        [Browsable(false)]
        public bool Attached
        {
            get { return _attached; }
        }

        public void Attach()
        {
            _attached = true;
            foreach (BMDGroupNode g in Children)
                g.Bind();

            ResetToBindState();
        }

        public void Detach()
        {
            _attached = false;
            foreach (BMDGroupNode g in Children)
                g.Unbind();
        }

        public void Refesh()
        {
            //if (_texList != null)
            //    foreach (MDL0TextureNode t in _texList)
            //        t.Reload();
        }

        public ModelPanel _mainWindow;

        public void Render(params object[] args)
        {
            if (!_render)
                return;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            ModelRenderAttributes attrib;
            if (args.Length > 0 && args[0] is ModelRenderAttributes)
                attrib = args[0] as ModelRenderAttributes;
            else
                attrib = _renderAttribs;

            if (attrib._renderPolygons || attrib._renderWireframe)
            {
                GL.Enable(EnableCap.Lighting);
                GL.Enable(EnableCap.DepthTest);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                foreach (BMDObjectNode p in _objGroup.Children)
                    RenderObject(p, attrib._renderPolygons, attrib._renderWireframe);
            }

            if (attrib._renderBones)
            {
                GL.Enable(EnableCap.Blend);
                GL.Disable(EnableCap.Lighting);
                GL.Disable(EnableCap.DepthTest);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

                if (_boneList != null)
                    foreach (BMDBoneNode bone in _boneList)
                        bone.Render();
            }
        }

        public static void RenderObject(
            BMDObjectNode p,
            bool renderPolygons,
            bool renderWireframe)
        {
            if (p.IsRendering)
            {
                if (renderPolygons)
                {
                    float polyOffset = 0.0f;
                    //polyOffset -= p.DrawPriority;
                    //polyOffset += maxDrawPriority;
                    if (renderWireframe)
                        polyOffset += 1.0f;
                    if (polyOffset != 0)
                    {
                        GL.Enable(EnableCap.PolygonOffsetFill);
                        GL.PolygonOffset(1.0f, polyOffset);
                    }
                    else
                        GL.Disable(EnableCap.PolygonOffsetFill);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    p.Render(false);
                }
                if (renderWireframe)
                {
                    GL.Disable(EnableCap.PolygonOffsetFill);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.LineWidth(0.5f);
                    p.Render(true);
                }
            }
        }

        [Browsable(false)]
        public InfluenceManager Influences
        {
            get { return _influences; }
            set { _influences = value; }
        }
        [Browsable(false)]
        public IBoneNode[] BoneCache { get { return new IBoneNode[0]; } }
        [Browsable(false)]
        public IBoneNode[] RootBones { get { return new IBoneNode[0]; } }

        public void ApplyCHR(CHR0Node node, float index)
        {

        }

        public void ApplySRT(SRT0Node node, float index)
        {

        }

        public void ApplySHP(SHP0Node node, float index)
        {

        }

        public void ApplyPAT(PAT0Node node, float index)
        {

        }

        public void ApplyVIS(VIS0Node node, float index)
        {

        }

        public void ApplyCLR(CLR0Node node, float index)
        {

        }

        public void SetSCN0(SCN0Node node)
        {

        }

        public void SetSCN0Frame(float index)
        {

        }

        [Browsable(false)]
        public bool IsRendering { get { return _render; } set { _render = value; } }
        bool _render = true;

        public void ResetToBindState()
        {
            //Transform bones
            if (_boneList != null)
            {
                foreach (BMDBoneNode b in _boneList)
                    b.ApplyCHR0(null, 0);
                foreach (BMDBoneNode b in _boneList)
                    b.RecalcFrameState();
            }

            //Transform nodes
            foreach (Influence inf in _influences._influences)
                inf.CalcMatrix();

            //Weight Vertices
            if (_objList != null)
                foreach (BMDObjectNode poly in _objList)
                    poly.WeightVertices();
        }

        [Browsable(false)]
        public int SelectedObjectIndex
        {
            get { return _objectIndex; }
            set { _objectIndex = value; }
        }

        [Browsable(false)]
        public IObject[] Objects { get { return new IObject[0]; } }

        public void RenderVertices(bool depthPass, IBoneNode weightTarget)
        {

        }
        public void RenderNormals()
        {

        }

        #endregion

        internal static ResourceNode TryParse(DataSource source)
        {
            J3DCommonHeader* hdr = (J3DCommonHeader*)source.Address;
            string j3dTag = hdr->_j3dTag;
            string nodeTag = hdr->_nodeTag;
            return j3dTag.StartsWith(J3DCommonHeader.J3DTag) && (nodeTag.StartsWith(J3DCommonHeader.BMDTag) || nodeTag.StartsWith(J3DCommonHeader.BDLTag)) ? new BMDNode() : null;
        }


        public bool IsTargetModel
        {
            get
            {
                return true;
            }
            set
            {
                
            }
        }


        public void Refresh()
        {
            
        }
    }
}