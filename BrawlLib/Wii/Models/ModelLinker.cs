using System;
using System.Runtime.InteropServices;
using BrawlLib.SSBBTypes;
using BrawlLib.SSBB.ResourceNodes;
using System.Collections.Generic;
using MR = BrawlLib.Wii.Models.MDLResourceType;
using BrawlLib.Modeling;
using System.Windows.Forms;

namespace BrawlLib.Wii.Models
{
    public enum MDLResourceType : int
    {
        Definitions,
        Bones,
        Vertices,
        Normals,
        Colors,
        UVs,
        Materials,
        Shaders,
        Objects,
        Textures,
        Palettes,
        FurVectors,
        FurLayerCoords
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe class ModelLinker// : IDisposable
    {
        #region Linker lists
        internal const int BankLen = 13;

        internal static readonly System.Type[] TypeBank = new System.Type[] 
        {
            null,
            typeof(MDL0BoneNode),
            typeof(MDL0VertexNode),
            typeof(MDL0NormalNode),
            typeof(MDL0ColorNode),
            typeof(MDL0UVNode),
            typeof(MDL0MaterialNode),
            typeof(MDL0ShaderNode),
            typeof(MDL0ObjectNode),
            null,
            null,
            typeof(MDL0FurVecNode),
            typeof(MDL0FurPosNode),
        };

        internal static readonly MR[] OrderBank = new MR[]
        {
            MR.Textures,
            MR.Palettes,
            MR.Definitions,
            MR.Bones,
            MR.FurVectors,
            MR.FurLayerCoords,
            MR.Materials,
            MR.Shaders,
            MR.Objects,
            MR.Vertices,
            MR.Normals,
            MR.Colors,
            MR.UVs,
        };

        internal static readonly List<MR>[] IndexBank = new List<MR>[]{
            null, //0
            null, //1
            null, //2
            null, //3
            null, //4
            null, //5
            null, //6
            null, //7
            new List<MR>(new MR[] { MR.Definitions, MR.Bones, MR.Vertices, MR.Normals, MR.Colors, MR.UVs, MR.Materials, MR.Shaders, MR.Objects, MR.Textures, MR.Palettes }),
            new List<MR>(new MR[] { MR.Definitions, MR.Bones, MR.Vertices, MR.Normals, MR.Colors, MR.UVs, MR.Materials, MR.Shaders, MR.Objects, MR.Textures, MR.Palettes }),
            new List<MR>(new MR[] { MR.Definitions, MR.Bones, MR.Vertices, MR.Normals, MR.Colors, MR.UVs, MR.FurVectors, MR.FurLayerCoords, MR.Materials, MR.Shaders, MR.Objects, MR.Textures, MR.Palettes }),
            new List<MR>(new MR[] { MR.Definitions, MR.Bones, MR.Vertices, MR.Normals, MR.Colors, MR.UVs, MR.FurVectors, MR.FurLayerCoords, MR.Materials, MR.Shaders, MR.Objects, MR.Textures, MR.Palettes }) 
        };
        #endregion

        public MDL0Header* Header;
        public int Version = 9;
        //Build relocation offsets in this order:
        public ResourceGroup* Defs; //1
        public ResourceGroup* Bones; //2
        public ResourceGroup* Vertices; //6
        public ResourceGroup* Normals; //7
        public ResourceGroup* Colors; //8
        public ResourceGroup* UVs; //9
        public ResourceGroup* Materials; //3
        public ResourceGroup* Shaders; //4
        public ResourceGroup* Polygons; //5
        public ResourceGroup* Textures; //10
        public ResourceGroup* Palettes; //11
        public ResourceGroup* FurVectors;
        public ResourceGroup* FurLayerCoords;

        public MDL0GroupNode[] Groups = new MDL0GroupNode[BankLen];

        public MDL0Node Model;
        public int _headerLen, _tableLen, _groupLen, _texLen, _boneLen, _dataLen, _defLen, _assetLen;
        public int _texCount, _palCount, _nodeCount;
        public ResourceNode[] BoneCache;
        public IMatrixNode[] NodeCache;

        public List<VertexCodec> _vertices;
        public List<VertexCodec> _normals;
        public List<ColorCodec> _colors;
        public List<VertexCodec> _uvs;

        public bool[] _forceDirectAssets = new bool[12];

        private ModelLinker() { }

        public ModelLinker(MDL0Header* pModel)
        {
            Header = pModel;
            Version = pModel->_header._version;
            NodeCache = new IMatrixNode[pModel->Properties->_numNodes];

            bint* offsets = (bint*)((byte*)pModel + 0x10);
            List<MDLResourceType> iList = IndexBank[Version];
            int groupCount = iList.Count;
            int offset;

            //Extract resource addresses
            fixed (ResourceGroup** gList = &Defs)
                for (int i = 0; i < groupCount; i++)
                    if ((offset = offsets[i]) > 0)
                        gList[(int)iList[i]] = (ResourceGroup*)((byte*)pModel + offset);
                    //else if (offset > 0 && (iList[i] == MR.FurLayerCoords || iList[i] == MR.FurVectors))
                    //    MessageBox.Show("Unsupported Fur Data is used!");
        }

        public static ModelLinker Prepare(MDL0Node model)
        {
            ModelLinker linker = new ModelLinker();

            linker.Model = model;
            linker.Version = model._version;

            linker.BoneCache = new ResourceNode[0];

            MDLResourceType resType;
            int index;
            List<MDLResourceType> iList = IndexBank[model._version];

            foreach (MDL0GroupNode group in model.Children)
            {
                resType = (MDLResourceType)Enum.Parse(typeof(MDLResourceType), group.Name);

                //Get flattened bone list and assign it to bone cache
                if (resType == MDLResourceType.Bones)
                    linker.BoneCache = group.FindChildrenByType(null, ResourceType.MDL0Bone);

                //If version contains resource type, add it to group list
                if ((index = iList.IndexOf(resType)) >= 0)
                    linker.Groups[(int)resType] = group;
            }

            return linker;
        }

        public void Write(Collada form, ref byte* pGroup, ref byte* pData, bool force)
        {
            MDL0GroupNode group;
            ResourceGroup* pGrp;
            ResourceEntry* pEntry;
            int len;

            //Write data in the order it appears
            foreach (MDLResourceType resType in OrderBank)
            {
                if (((group = Groups[(int)resType]) == null) || (TypeBank[(int)resType] == null))
                    continue;

                if (resType == MDLResourceType.Bones)
                {
                    foreach (ResourceNode e in BoneCache)
                    {
                        if (form != null)
                            form.Say("Writing the Bones - " + e.Name);

                        len = e._calcSize;
                        e.Rebuild(pData, len, true);
                        pData += len;
                    }
                }
                else if (resType == MDLResourceType.Shaders)
                {
                    MDL0GroupNode mats = Groups[(int)MDLResourceType.Materials];
                    MDL0Material* mHeader;

                    //Write data without headers
                    foreach (ResourceNode e in group.Children)
                    {
                        if (((MDL0ShaderNode)e)._materials.Count > 0)
                        {
                            if (form != null)
                                form.Say("Writing the Shaders - " + e.Name);

                            len = e._calcSize;
                            e.Rebuild(pData, len, force);
                            pData += len;
                        }
                    }
                    //Write one header for each material, using same order.
                    if (mats != null)
                    foreach (MDL0MaterialNode mat in mats.Children)
                    {
                        mHeader = mat.Header;
                        if (mat._shader != null)
                        {
                            len = (int)mat._shader.Header;
                            mHeader->_shaderOffset = len - (int)mHeader;
                        }
                        else
                            mHeader->_shaderOffset = 0;
                    }
                }
                else if (resType == MDLResourceType.Objects || resType == MDLResourceType.Materials)
                {
                    foreach (ResourceNode r in group.Children)
                    {
                        if (form != null)
                            form.Say("Writing the " + resType.ToString() + " - " + r.Name);

                        len = r._calcSize;
                        r.Rebuild(pData, len, true); //Forced to fix object node ids and align materials
                        pData += len;
                    }
                }
                else
                {
                    bool rebuild = true;

                    if (Model._isImport)
                        if (group._name == "Vertices" ||
                            group._name == "Normals" ||
                            group._name == "UVs" ||
                            group._name == "Colors")
                            rebuild = false; //The data has already been written!

                    if (rebuild)
                        foreach (ResourceNode e in group.Children)
                        {
                            //Console.WriteLine("Rebuilding the " + group.Name);

                            if (form != null)
                                form.Say("Writing the " + resType.ToString() + " - " + e.Name);

                            len = e._calcSize;
                            e.Rebuild(pData, len, true); //Forced just in case we need to convert to float.
                            pData += len;
                        }
                }
            }

            //Write relocation offsets in the order of the header
            fixed (ResourceGroup** pOut = &Defs)
                foreach (MDLResourceType resType in IndexBank[Version])
                {
                    if (((group = Groups[(int)resType]) == null) || (TypeBank[(int)resType] == null))
                        continue;

                    pOut[(int)resType] = pGrp = (ResourceGroup*)pGroup;
                    pEntry = &pGrp->_first + 1;
                    if (resType == MDLResourceType.Bones)
                    {
                        *pGrp = new ResourceGroup(BoneCache.Length);
                        foreach (ResourceNode e in BoneCache)
                            (pEntry++)->_dataOffset = (int)(e.WorkingUncompressed.Address - pGroup);
                    }
                    else if (resType == MDLResourceType.Shaders)
                    {
                        MDL0GroupNode mats = Groups[(int)MDLResourceType.Materials];

                        if (mats != null)
                        {
                            //Create a material group with the amount of entries
                            *pGrp = new ResourceGroup(mats.Children.Count);

                            foreach (MDL0MaterialNode mat in mats.Children)
                                (pEntry++)->_dataOffset = (int)mat._shader.Header - (int)pGrp;
                        }
                    }
                    else
                    {
                        *pGrp = new ResourceGroup(group.Children.Count);
                        foreach (ResourceNode e in group.Children)
                            (pEntry++)->_dataOffset = (int)(e.WorkingUncompressed.Address - pGroup);
                    }
                    pGroup += pGrp->_totalSize;
                }
        }

        //Write stored offsets to MDL header
        public void Finish()
        {
            List<MDLResourceType> iList = IndexBank[Version];
            MDLResourceType resType;
            bint* pOffset = (bint*)Header + 4;
            int count = iList.Count, offset;

            fixed (ResourceGroup** pGroup = &Defs)
                for (int i = 0; i < count; i++)
                {
                    resType = iList[i];
                    if ((offset = (int)pGroup[(int)resType]) > 0)
                        offset -= (int)Header;
                    pOffset[i] = offset;
                }
        }
    }
}
