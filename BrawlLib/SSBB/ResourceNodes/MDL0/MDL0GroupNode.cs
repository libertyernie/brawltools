using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.OpenGL;
using BrawlLib.Wii.Models;
using BrawlLib.Modeling;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe abstract class MDL0EntryNode : ResourceNode
    {
        internal virtual void GetStrings(StringTable table) { table.Add(Name); }

        internal int _entryIndex;

        [Browsable(false)]
        public MDL0Node Model
        {
            get
            {
                ResourceNode n = _parent;
                while (!(n is MDL0Node) && (n != null))
                    n = n._parent;
                return n as MDL0Node;
            }
        }

        [Browsable(false)]
        public BRESNode BRESNode
        {
            get
            {
                ResourceNode n = _parent;
                while (!(n is BRESNode) && (n != null))
                    n = n._parent;
                return n as BRESNode;
            }
        }

        internal virtual void Bind(TKContext ctx) { }
        internal virtual void Unbind() { }

        protected internal virtual void PostProcess(VoidPtr mdlAddress, VoidPtr dataAddress, StringTable stringTable) { }
    }

    public unsafe class MDL0GroupNode : ResourceNode
    {
        internal ResourceGroup* Header { get { return (ResourceGroup*)WorkingUncompressed.Address; } }

        public override ResourceType ResourceType { get { return ResourceType.MDL0Group; } }

        public MDLResourceType _type;
        internal int _index;
        //internal List<ResourceNode> _nodeCache;

        public MDL0GroupNode(MDLResourceType type)
        {
            _type = type;
            _name = _type.ToString("g");
        }

        internal void GetStrings(StringTable table)
        {
            foreach (MDL0EntryNode n in Children)
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

        internal void Parse(MDL0Node model)
        {
            Influence inf;
            ModelLinker linker = model._linker;
            switch (_type)
            {
                case MDLResourceType.Definitions:
                    if (linker.Defs != null)
                        ExtractGroup(linker.Defs, typeof(MDL0DefNode));
                    break;

                case MDLResourceType.Bones:
                    //Break if there are no bones defined
                    if (linker.Bones == null)
                        break;

                    //Parse bones from raw data (flat list).
                    //Bones re-assign parents in their Initialize block, so parents are true.
                    //Parents must be assigned now as bones will be moved in memory when assigned as children.
                    ExtractGroup(linker.Bones, typeof(MDL0BoneNode));

                    //Cache flat list
                    linker.BoneCache = _children.ToArray();

                    //Make sure the node cache is the correct size
                    int highest = 0;
                    foreach (MDL0BoneNode b in linker.BoneCache)
                        if (b._nodeIndex >= linker.NodeCache.Length && b._nodeIndex > highest)
                            highest = b._nodeIndex;
                    if (highest >= linker.NodeCache.Length)
                        linker.NodeCache = new IMatrixNode[highest + 1];

                    //Reset children so we can rebuild
                    _children.Clear();

                    //Assign children using each bones' parent offset in case NodeTree is corrupted
                    foreach (MDL0BoneNode b in linker.BoneCache)
                        b._parent._children.Add(b);

                    //Populate node cache
                    MDL0BoneNode bone = null;
                    int index;
                    int count = linker.BoneCache.Length;
                    for (int i = 0; i < count; i++)
                        linker.NodeCache[(bone = linker.BoneCache[i] as MDL0BoneNode)._nodeIndex] = bone;

                    int nullCount = 0;

                    bool nodeTreeError = false;

                    //Now that bones and primary influences have been cached, we can create weighted influences.
                    foreach (ResourcePair p in *linker.Defs)
                        if (p.Name == "NodeTree")
                        {
                            //Double check bone tree using the NodeTree definition.
                            //If the NodeTree is corrupt, the user will be informed that it needs to be rebuilt.
                            byte* pData = (byte*)p.Data;

                        Top:
                            if (*pData == 2)
                            {
                                bone = linker.BoneCache[*(bushort*)(pData + 1)] as MDL0BoneNode;
                                index = *(bushort*)(pData + 3);

                                if (bone.Header->_parentOffset == 0)
                                {
                                    if (!_children.Contains(bone))
                                    {
                                        nodeTreeError = true;
                                        continue;
                                    }
                                }
                                else
                                {
                                    ResourceNode n = linker.NodeCache[index] as ResourceNode;
                                    if (n == null || bone._parent != n || !n._children.Contains(bone))
                                    {
                                        nodeTreeError = true;
                                        continue;
                                    }
                                }
                                pData += 5;
                                goto Top;
                            }
                        }
                        else 
                            if (p.Name == "NodeMix")
                        {
                            //Use node mix to create weight groups
                            byte* pData = (byte*)p.Data;

                        Top:
                            switch (*pData)
                            {
                                //Type 3 is for weighted influences
                                case 3:
                                    //Get index/count fields
                                    index = *(bushort*)(pData + 1);
                                    count = pData[3];
                                    //Get data pointer (offset of 4)
                                    MDL0NodeType3Entry* nEntry = (MDL0NodeType3Entry*)(pData + 4);
                                    //Create influence with specified count
                                    inf = new Influence();
                                    //Iterate through weights, adding each to the influence
                                    //Here, we are referring back to the NodeCache to grab the bone.
                                    //Note that the weights do not reference other influences, only bones. There is a good reason for this.
                                    MDL0BoneNode b = null;
                                    List<int> nullIndices = new List<int>();
                                    for (int i = 0; i < count; i++, nEntry++)
                                        if (nEntry->_id < linker.NodeCache.Length && (b = (linker.NodeCache[nEntry->_id] as MDL0BoneNode)) != null)
                                            inf._weights.Add(new BoneWeight(b, nEntry->_value));
                                        else
                                        {
                                            nullIndices.Add(i);
                                            nullCount++;
                                        }

                                    bool d = false;
                                    if (nullIndices.Count > 0)
                                    {
                                        List<BoneWeight> newWeights = new List<BoneWeight>();
                                        for (int i = 0; i < inf._weights.Count; i++)
                                            if (!nullIndices.Contains(i))
                                                newWeights.Add(inf._weights[i]);
                                        if (newWeights.Count == 0)
                                            d = true;
                                        else
                                            inf._weights = newWeights;
                                    }

                                    //Add influence to model object, while adding it to the cache.
                                    if (!d) ((Influence)(linker.NodeCache[index] = model._influences.FindOrCreate(inf, true)))._index = index;

                                    //Move data pointer to next entry
                                    pData = (byte*)nEntry;
                                    goto Top;

                                //Type 5 is for primary influences
                                case 5:
                                    pData += 5;
                                    goto Top;
                            }
                        }

                    if (nullCount > 0)
                    {
                        model._errors.Add("There were " + nullCount + " null weights in NodeMix.");
                        SignalPropertyChange();
                    }

                    if (nodeTreeError)
                    {
                        model._errors.Add("The NodeTree definition did not match the bone tree.");
                        SignalPropertyChange();
                    }

                    break;

                case MDLResourceType.Materials:
                    if (linker.Materials != null)
                        ExtractGroup(linker.Materials, typeof(MDL0MaterialNode));
                    break;

                case MDLResourceType.Shaders:
                    if (linker.Shaders != null)
                        ExtractGroup(linker.Shaders, typeof(MDL0ShaderNode));
                    break;

                case MDLResourceType.Vertices:
                    if (linker.Vertices != null)
                        ExtractGroup(linker.Vertices, typeof(MDL0VertexNode));
                    break;

                case MDLResourceType.Normals:
                    if (linker.Normals != null)
                        ExtractGroup(linker.Normals, typeof(MDL0NormalNode));
                    break;

                case MDLResourceType.UVs:
                    if (linker.UVs != null)
                        ExtractGroup(linker.UVs, typeof(MDL0UVNode));
                    break;

                case MDLResourceType.FurLayerCoords:
                    if (linker.FurLayerCoords != null)
                        ExtractGroup(linker.FurLayerCoords, typeof(MDL0FurPosNode));
                    break;

                case MDLResourceType.FurVectors:
                    if (linker.FurVectors != null)
                        ExtractGroup(linker.FurVectors, typeof(MDL0FurVecNode));
                    break;

                case MDLResourceType.Objects:
                    //Break if no polygons defined
                    if (linker.Polygons == null)
                        break;

                    //Extract
                    ExtractGroup(linker.Polygons, typeof(MDL0ObjectNode));

                    //Attach materials to polygons.
                    //This assumes that materials have already been parsed.

                    List<ResourceNode> matList = ((MDL0Node)_parent)._matList;
                    MDL0ObjectNode poly;
                    MDL0MaterialNode mat;
                    
                    //Find DrawOpa or DrawXlu entry in Definition list
                    foreach (ResourcePair p in *linker.Defs)
                        if ((p.Name == "DrawOpa") || (p.Name == "DrawXlu"))
                        {
                            bool opa = p.Name == "DrawOpa";
                            ushort dIndex = 0;
                            byte* pData = (byte*)p.Data;
                            while (*pData++ == 4)
                            {
                                //Get polygon from index
                                dIndex = *(bushort*)(pData + 2);
                                if (dIndex >= _children.Count || dIndex < 0)
                                {
                                    model._errors.Add("Object index was greater than the actual object count.");
                                    SignalPropertyChange();
                                    dIndex = 0;
                                }
                                poly = _children[dIndex] as MDL0ObjectNode;
                                poly._drawIndex = pData[6];
                                //Get material from index
                                mat = matList[*(bushort*)pData] as MDL0MaterialNode;
                                //Get bone from index and assign
                                int boneIndex = *(bushort*)(pData + 4);
                                if (linker.BoneCache != null && boneIndex >= 0 && boneIndex < linker.BoneCache.Length)
                                    poly.BoneNode = linker.BoneCache[boneIndex] as MDL0BoneNode;
                                //Assign material to polygon
                                if (opa)
                                    poly.OpaMaterialNode = mat;
                                else
                                    poly.XluMaterialNode = mat;
                                //Increment pointer
                                pData += 7;
                            }
                        }

                    foreach (MDL0ObjectNode m in _children)
                    {
                        int max = Maths.Max(
                        m.OpaMaterialNode != null ? m.OpaMaterialNode.Children.Count : 0,
                        m.XluMaterialNode != null ? m.XluMaterialNode.Children.Count : 0,
                        m.OpaMaterialNode != null && m.OpaMaterialNode.MetalMaterial != null ? m.OpaMaterialNode.MetalMaterial.Children.Count : 0,
                        m.XluMaterialNode != null && m.XluMaterialNode.MetalMaterial != null ? m.XluMaterialNode.MetalMaterial.Children.Count : 0);

                        bool hasUnused = false;
                        for (int i = max; i < 8; i++)
                            if (m.HasTextureMatrix[i])
                            {
                                m.HasTextureMatrix[i] = false;
                                m._rebuild = true;
                                hasUnused = true;
                            }
                        if (hasUnused)
                        {
                            ((MDL0Node)Parent)._errors.Add("Object " + m.Index + " has unused texture matrices.");
                            m.SignalPropertyChange();
                        }

                        if (m.HasTexMtx && m.HasNonFloatVertices)
                        {
                            ((MDL0Node)Parent)._errors.Add("Object " + m.Index + " has texture matrices and non-float vertices, meaning it will explode in-game.");
                            m.SignalPropertyChange();
                        }
                    }
                    break;

                case MDLResourceType.Colors:
                    if (linker.Colors != null)
                        ExtractGroup(linker.Colors, typeof(MDL0ColorNode));
                    break;

                case MDLResourceType.Textures:
                    if (linker.Textures != null)
                        ExtractGroup(linker.Textures, typeof(MDL0TextureNode));
                    break;

                case MDLResourceType.Palettes:
                    if (linker.Palettes != null)
                        ExtractGroup(linker.Palettes, typeof(MDL0TextureNode));
                    break;
            }
        }

        //Extracts resources from a group, using the specified type
        private void ExtractGroup(ResourceGroup* pGroup, Type t)
        {
            //If using shaders, cache results instead of unique entries
            //This is because shaders can appear multiple times, but with different names
            bool useCache = t == typeof(MDL0ShaderNode);

            MDL0CommonHeader* pHeader;
            ResourceNode node;
            int* offsetCache = stackalloc int[128];
            int offsetCount = 0, offset, x;

            foreach (ResourcePair p in *pGroup)
            {
                //Get data offset
                offset = (int)p.Data;
                if (useCache)
                {
                    //search for entry within offset cache
                    for (x = 0; (x < offsetCount) && (offsetCache[x] != offset); x++);
                    //If found, skip to next entry
                    if (x < offsetCount) continue;
                    //Otherwise, store offset
                    offsetCache[offsetCount++] = offset;
                }

                //Create resource instance
                pHeader = (MDL0CommonHeader*)p.Data;
                node = Activator.CreateInstance(t) as ResourceNode;

                //Initialize
                node.Initialize(this, pHeader, pHeader->_size);

                //Set the name of the node. This is necessary for defs.
                //Make sure we're not naming the shaders,
                //or it will name it the name of the first material it's linked to.
                if (t != typeof(MDL0ShaderNode))
                    node._name = (string)p.Name;
            }
        }
        
        protected internal virtual void PostProcess(VoidPtr mdlAddress, VoidPtr dataAddress, StringTable stringTable)
        {
            ResourceGroup* pGroup = (ResourceGroup*)dataAddress;
            ResourceEntry* rEntry = &pGroup->_first;
            int index = 1;
            (*rEntry++) = new ResourceEntry(0xFFFF, 0, 0, 0, 0);

            if (_name == "Bones")
                foreach (MDL0EntryNode n in _children)
                    PostProcessBone(mdlAddress, n, pGroup, ref index, stringTable);
            else if (_name != "Definitions")
                foreach (MDL0EntryNode n in _children)
                {
                    dataAddress = (VoidPtr)pGroup + (rEntry++)->_dataOffset;
                    ResourceEntry.Build(pGroup, index++, dataAddress, (BRESString*)stringTable[n.Name]);
                    n.PostProcess(mdlAddress, dataAddress, stringTable);
                }
        }

        private void PostProcessBone(VoidPtr mdlAddress, MDL0EntryNode node, ResourceGroup* group, ref int index, StringTable stringTable)
        {
            VoidPtr dataAddress = (VoidPtr)group + (&group->_first)[index]._dataOffset;
            ResourceEntry.Build(group, index++, dataAddress, (BRESString*)stringTable[node.Name]);
            node.PostProcess(mdlAddress, dataAddress, stringTable);

            foreach (MDL0EntryNode n in node.Children)
                PostProcessBone(mdlAddress, n, group, ref index, stringTable);
        }

        internal void Bind(TKContext ctx)
        {
            foreach (MDL0EntryNode e in Children)
                e.Bind(ctx);
        }
        internal void Unbind()
        {
            foreach (MDL0EntryNode e in Children)
                e.Unbind();
        }
    }
}
