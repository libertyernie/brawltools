using System;
using System.Collections.Generic;
using BrawlLib.SSBBTypes;
using BrawlLib.Wii.Models;
using BrawlLib.Imaging;
using BrawlLib.Wii.Graphics;
using BrawlLib.OpenGL;
using System.Drawing;
using BrawlLib.SSBB.ResourceNodes;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Linq;

namespace BrawlLib.Modeling
{
    public unsafe class PrimitiveManager : IDisposable
    {
        #region Variables

        public List<Vertex3> _vertices;
        public UnsafeBuffer _indices;

        public int _pointCount, _faceCount, _stride;
        public MDL0ObjectNode _polygon;

        //Face Data:
        //0 is Vertices
        //1 is Normals
        //2-3 is Colors
        //4-12 is UVs
        //The primitives indices match up to these values
        public UnsafeBuffer[] _faceData = new UnsafeBuffer[12];
        public bool[] _dirty = new bool[12];

        //Graphics buffer is a combination of the _faceData streams
        //Vertex (Vertex3 - 12 bytes), Normal (Vertex3 - 12 bytes),
        //Color1 & Color2 (4 bytes each), UVs 0 - 7 (Vertex2 - 8 bytes each),
        //Repeat 
        public UnsafeBuffer _graphicsBuffer;

        internal NewPrimitive _triangles, _lines, _points;

        #endregion

        #region Asset Lists
        /// <summary>
        /// Returns vertex positions from each vertex.
        /// </summary>
        /// <param name="force">If not forced, will return values created by a previous call (if there was one).</param>
        /// <returns></returns>
        public Vector3[] GetVertices(bool force)
        {
            if (_rawVertices != null && _rawVertices.Length != 0 && !force)
                return _rawVertices;

            int i = 0;
            _rawVertices = new Vector3[_vertices.Count];
            foreach (Vertex3 v in _vertices)
                _rawVertices[i++] = v._position;

            return _rawVertices;
        }
        private Vector3[] _rawVertices;
        /// <summary>
        /// Retrieves normals from raw facedata in a remapped array.
        /// </summary>
        /// <param name="force">If not forced, will return values created by a previous call (if there was one).</param>
        /// <returns></returns>
        public Vector3[] GetNormals(bool force)
        {
            if (_rawNormals != null && _rawNormals.Length != 0 && !force)
                return _rawNormals;

            HashSet<Vector3> list = new HashSet<Vector3>();
            if (_faceData[1] != null)
            {
                Vector3* pIn = (Vector3*)_faceData[1].Address;
                for (int i = 0; i < _pointCount; i++)
                    list.Add(*pIn++);
            }

            _rawNormals = new Vector3[list.Count];
            list.CopyTo(_rawNormals);

            return _rawNormals;
        }
        private Vector3[] _rawNormals;
        /// <summary>
        /// Retrieves texture coordinates from raw facedata in a remapped array.
        /// </summary>
        /// <param name="index">The UV set to retrieve. Values 0 - 7</param>
        /// <param name="force">If not forced, will return values created by a previous call (if there was one).</param>
        /// <returns></returns>
        public Vector2[] GetUVs(int index, bool force)
        {
            index.Clamp(0, 7);

            if (_uvs[index] != null && _uvs[index].Length != 0 && !force) 
                return _uvs[index];

            HashSet<Vector2> list = new HashSet<Vector2>();
            if (_faceData[index + 4] != null)
            {
                Vector2* pIn = (Vector2*)_faceData[index + 4].Address;
                for (int i = 0; i < _pointCount; i++)
                    list.Add(*pIn++);
            }

            _uvs[index] = new Vector2[list.Count];
            list.CopyTo(_uvs[index]);

            return _uvs[index];
        }
        private Vector2[][] _uvs = new Vector2[8][];
        /// <summary>
        /// Retrieves color values from raw facedata in a remapped array.
        /// </summary>
        /// <param name="index">The color set to retrieve. Values 0 - 1</param>
        /// <param name="force">If not forced, will return values created by a previous call (if there was one).</param>
        /// <returns></returns>
        public RGBAPixel[] GetColors(int index, bool force)
        {
            index.Clamp(0, 1);

            if (_colors[index] != null && _colors[index].Length != 0 && !force) 
                return _colors[index];

            HashSet<RGBAPixel> list = new HashSet<RGBAPixel>();
            if (_faceData[index + 2] != null)
            {
                RGBAPixel* pIn = (RGBAPixel*)_faceData[index + 2].Address;
                for (int i = 0; i < _pointCount; i++)
                    list.Add(*pIn++);
            }

            _colors[index] = new RGBAPixel[list.Count];
            list.CopyTo(_colors[index]);

            return _colors[index];
        }
        private RGBAPixel[][] _colors = new RGBAPixel[2][];
        #endregion

        #region Reading

        public PrimitiveManager() { }
        public PrimitiveManager(MDL0Object* polygon, AssetStorage assets, IMatrixNode[] nodes, MDL0ObjectNode p)
        {
            _polygon = p;

            byte*[] pAssetList = new byte*[12];
            byte*[] pOutList = new byte*[12];
            int id;

            //This relies on the header being accurate!
            _indices = new UnsafeBuffer(2 * (_pointCount = polygon->_numVertices));
            _faceCount = polygon->_numFaces;

            //Compile decode script by reading the polygon def list
            //This sets how to read the facepoints
            ElementDescriptor desc = new ElementDescriptor(polygon);

            //Grab asset lists in sequential order.
            if ((id = polygon->_vertexId) >= 0 && desc.HasData[0] && assets.Assets[0] != null)
            {
                pOutList[0] = (byte*)(_faceData[0] = new UnsafeBuffer(12 * _pointCount)).Address;
                pAssetList[0] = (byte*)assets.Assets[0][id].Address;
            }
            if ((id = polygon->_normalId) >= 0 && desc.HasData[1] && assets.Assets[1] != null)
            {
                pOutList[1] = (byte*)(_faceData[1] = new UnsafeBuffer(12 * _pointCount)).Address;
                pAssetList[1] = (byte*)assets.Assets[1][id].Address;
            }
            for (int i = 0, x = 2; i < 2; i++, x++)
                if ((id = ((bshort*)polygon->_colorIds)[i]) >= 0 && desc.HasData[x] && assets.Assets[2] != null)
                {
                    pOutList[x] = (byte*)(_faceData[x] = new UnsafeBuffer(4 * _pointCount)).Address;
                    pAssetList[x] = (byte*)assets.Assets[2][id].Address;
                }
            for (int i = 0, x = 4; i < 8; i++, x++)
                if ((id = ((bshort*)polygon->_uids)[i]) >= 0 && desc.HasData[x] && assets.Assets[3] != null)
                {
                    pOutList[x] = (byte*)(_faceData[x] = new UnsafeBuffer(8 * _pointCount)).Address;
                    pAssetList[x] = (byte*)assets.Assets[3][id].Address;
                }

            //Extract primitives, using our descriptor and asset lists
            fixed (byte** pOut = pOutList)
            fixed (byte** pAssets = pAssetList)
                ExtractPrimitives(polygon, ref desc, pOut, pAssets);

            //Compile merged vertex list
            _vertices = desc.Finish((Vector3*)pAssetList[0], nodes);

            ushort* pIndex = (ushort*)_indices.Address;
            for (int x = 0; x < _pointCount; x++)
                if (pIndex[x] >= 0 && pIndex[x] < _vertices.Count)
                    _vertices[pIndex[x]]._faceDataIndices.Add(x);
        }
        
        ~PrimitiveManager() { Dispose(); }
        public void Dispose()
        {
            if (_graphicsBuffer != null)
            {
                _graphicsBuffer.Dispose(); 
                _graphicsBuffer = null; 
            }

            if (_faceData != null)
            {
                for (int i = 0; i < _faceData.Length; i++)
                    if (_faceData[i] != null)
                    {
                        _faceData[i].Dispose();
                        _faceData[i] = null;
                    }
                _faceData = null;
            }

            if (_indices != null)
            {
                _indices.Dispose();
                _indices = null;
            }
        }
        internal void ExtractPrimitives(MDL0Object* header, ref ElementDescriptor desc, byte** pOut, byte** pAssets)
        {
            int count;
            uint index = 0, temp;
            byte* pData = (byte*)header->PrimitiveData;
            byte* pTemp = (byte*)pData;
            ushort* indices = (ushort*)_indices.Address;

            IMatrixNode[] cache = _polygon.Model._linker.NodeCache;

            //Get element count for each primitive type
            int d3 = 0, d2 = 0, d1 = 0;
            uint p3 = 0, p2 = 0, p1 = 0;
            uint[] p1arr = null, p2arr = null, p3arr = null;

            bool newGroup = true;
            PrimitiveGroup group = new PrimitiveGroup();
            ushort id;

            //Get counts for each primitive type, and assign face points
        NextPrimitive:
            byte cmd = *pTemp++;
            if (cmd <= 0x38 && cmd >= 0x20)
            {
                if (newGroup == false)
                {
                    _polygon._primGroups.Add(group);
                    group = new PrimitiveGroup() { _offset = (uint)((pTemp - 1) - pData) };
                    newGroup = true;
                }
                if (!group._nodes.Contains(id = *(bushort*)pTemp) && id != ushort.MaxValue)
                    group._nodes.Add(id);
            }
            //Switch by primitive type and increment as well so we can read the count.
            switch ((GXListCommand)cmd)
            {
                //Fill weight cache
                case GXListCommand.LoadIndexA: //Positions

                    group._nodeOffsets.Add(new NodeOffset((uint)(pTemp - pData) - group._offset, cache[*(bushort*)pTemp]));

                    //Set weight node for facepoint extraction
                    desc.SetNode(ref pTemp, (byte*)pData);

                    goto NextPrimitive;

                case GXListCommand.LoadIndexB: //Normals
                case GXListCommand.LoadIndexC: //UVs

                    group._nodeOffsets.Add(new NodeOffset((uint)(pTemp - pData) - group._offset, cache[*(bushort*)pTemp]));
                    pTemp += 4; //Skip
                    goto NextPrimitive;

                case GXListCommand.LoadIndexD: //Lights

                    Console.WriteLine("There are lights in here!");
                    pTemp += 4; //Skip
                    goto NextPrimitive;

                case GXListCommand.DrawQuads:

                    if (newGroup == true) newGroup = false;
                    d3 += (count = *(bushort*)pTemp) / 2 * 3;
                    group._headers.Add(new PrimitiveHeader() { Type = WiiPrimitiveType.Quads, Entries = (ushort)count });
                    break;

                case GXListCommand.DrawTriangles:

                    if (newGroup == true) newGroup = false;
                    d3 += (count = *(bushort*)pTemp);
                    group._headers.Add(new PrimitiveHeader() { Type = WiiPrimitiveType.TriangleList, Entries = (ushort)count });
                    break;

                case GXListCommand.DrawTriangleFan:

                    if (newGroup == true) newGroup = false;
                    d3 += ((count = *(bushort*)pTemp) - 2) * 3;
                    group._headers.Add(new PrimitiveHeader() { Type = WiiPrimitiveType.TriangleFan, Entries = (ushort)count });
                    break;

                case GXListCommand.DrawTriangleStrip:

                    if (newGroup == true) newGroup = false;
                    d3 += ((count = *(bushort*)pTemp) - 2) * 3;
                    group._headers.Add(new PrimitiveHeader() { Type = WiiPrimitiveType.TriangleStrip, Entries = (ushort)count });
                    break;

                case GXListCommand.DrawLines:

                    if (newGroup == true) newGroup = false;
                    d2 += (count = *(bushort*)pTemp);
                    group._headers.Add(new PrimitiveHeader() { Type = WiiPrimitiveType.Lines, Entries = (ushort)count });
                    break;

                case GXListCommand.DrawLineStrip:

                    if (newGroup == true) newGroup = false;
                    d2 += ((count = *(bushort*)pTemp) - 1) * 2;
                    group._headers.Add(new PrimitiveHeader() { Type = WiiPrimitiveType.LineStrip, Entries = (ushort)count });
                    break;

                case GXListCommand.DrawPoints:

                    if (newGroup == true) newGroup = false;
                    d1 += (count = *(bushort*)pTemp);
                    group._headers.Add(new PrimitiveHeader() { Type = WiiPrimitiveType.Points, Entries = (ushort)count });
                    break;

                default:
                    _polygon._primGroups.Add(group);
                    goto Next; //No more primitives.
            }

            pTemp += 2;

            //Extract facepoints here!
            desc.Run(ref pTemp, pAssets, pOut, count, group, ref indices, _polygon.Model._linker.NodeCache);

            goto NextPrimitive;

        Next: //Create primitives
            if (d3 > 0)
            { _triangles = new NewPrimitive(d3, BeginMode.Triangles); p3arr = _triangles._indices; }
            else _triangles = null;

            if (d2 > 0)
            { _lines = new NewPrimitive(d2, BeginMode.Lines); p2arr = _lines._indices; }
            else _lines = null;

            if (d1 > 0)
            { _points = new NewPrimitive(d1, BeginMode.Points); p1arr = _points._indices; }
            else _points = null;

            //Extract indices in reverse order, this way we get CCW winding.
        Top:
            switch ((GXListCommand)(*pData++))
            {
                case GXListCommand.LoadIndexA:
                case GXListCommand.LoadIndexB:
                case GXListCommand.LoadIndexC:
                case GXListCommand.LoadIndexD:
                    pData += 4; //Skip
                    goto Top;

                case GXListCommand.DrawQuads:
                    count = *(bushort*)pData;
                    for (int i = 0; i < count; i += 4)
                    {
                        p3arr[p3++] = index;
                        p3arr[p3++] = (uint)(index + 2);
                        p3arr[p3++] = (uint)(index + 1);
                        p3arr[p3++] = index;
                        p3arr[p3++] = (uint)(index + 3);
                        p3arr[p3++] = (uint)(index + 2);
                        index += 4;
                    }
                    break;
                case GXListCommand.DrawTriangles:
                    count = *(bushort*)pData;
                    for (int i = 0; i < count; i += 3)
                    {
                        p3arr[p3++] = (uint)(index + 2);
                        p3arr[p3++] = (uint)(index + 1);
                        p3arr[p3++] = index;
                        index += 3;
                    }
                    break;
                case GXListCommand.DrawTriangleFan:
                    count = *(bushort*)pData;
                    temp = index++;
                    for (int i = 2; i < count; i++)
                    {
                        p3arr[p3++] = temp;
                        p3arr[p3++] = (uint)(index + 1);
                        p3arr[p3++] = index++;
                    }
                    index++;
                    break;
                case GXListCommand.DrawTriangleStrip:
                    count = *(bushort*)pData;
                    index += 2;
                    for (int i = 2; i < count; i++)
                    {
                        p3arr[p3++] = index;
                        p3arr[p3++] = (uint)(index - 1 - (i & 1));
                        p3arr[p3++] = (uint)((index++) - 2 + (i & 1));
                    }
                    break;
                case GXListCommand.DrawLines:
                    count = *(bushort*)pData;
                    for (int i = 0; i < count; i++)
                        p2arr[p2++] = index++;
                    break;
                case GXListCommand.DrawLineStrip:
                    count = *(bushort*)pData;
                    for (int i = 1; i < count; i++)
                    {
                        p2arr[p2++] = index++;
                        p2arr[p2++] = index;
                    }
                    index++;
                    break;
                case GXListCommand.DrawPoints:
                    count = *(bushort*)pData;
                    for (int i = 0; i < count; i++)
                        p1arr[p1++] = index++;
                    break;
                default: return;
            }
            pData += 2 + count * desc.Stride;
            goto Top;
        }

        #endregion

        #region Writing

        internal void WritePrimitives(MDL0ObjectNode poly, MDL0Object* header)
        {
            _polygon = poly;

            VoidPtr start = header->PrimitiveData;
            VoidPtr address = header->PrimitiveData;
            FacepointAttribute[] desc = poly._descList;
            IMatrixNode[] cache = poly.Model._linker.NodeCache;

            foreach (PrimitiveGroup g in poly._primGroups)
            {
                if (!poly.Model._isImport && !poly._reOptimized)
                    g.RegroupNodes();

                g._nodes.Sort();
                g._offset = (uint)address - (uint)start;
                g._nodeOffsets.Clear();

                //Write matrix headers for linking weighted influences to points
                if (poly.Weighted)
                {
                    int index = 0;

                    //Texture Matrices
                    if (poly.HasTexMtx)
                        for (int i = 0; i < g._nodes.Count; i++)
                        {
                            *(byte*)address++ = 0x30;

                            g._nodeOffsets.Add(new NodeOffset((uint)(address - start) - g._offset, cache[g._nodes[i]]));

                            *(bushort*)address = g._nodes[i]; address += 2;
                            *(byte*)address++ = 0xB0;
                            *(byte*)address++ = (byte)(0x78 + (12 * index++));
                        }

                    index = 0;

                    //Position Matrices
                    for (int i = 0; i < g._nodes.Count; i++)
                    {
                        *(byte*)address++ = 0x20;

                        g._nodeOffsets.Add(new NodeOffset((uint)(address - start) - g._offset, cache[g._nodes[i]]));

                        *(bushort*)address = g._nodes[i]; address += 2;
                        *(byte*)address++ = 0xB0;
                        *(byte*)address++ = (byte)(12 * index++);
                    }

                    index = 0;

                    //Normal Matrices
                    for (int i = 0; i < g._nodes.Count; i++)
                    {
                        *(byte*)address++ = 0x28;

                        g._nodeOffsets.Add(new NodeOffset((uint)(address - start) - g._offset, cache[g._nodes[i]]));

                        *(bushort*)address = g._nodes[i]; address += 2;
                        *(byte*)address++ = 0x84;
                        *(byte*)address++ = (byte)(9 * index++);
                    }
                }

                if (poly.Model._isImport || poly._reOptimized)
                {
                    //Write strips first
                    if (g._tristrips.Count != 0)
                        foreach (PointTriangleStrip strip in g._tristrips)
                        {
                            *(PrimitiveHeader*)address = strip.Header; address += 3;
                            foreach (Facepoint f in strip._points)
                                WriteFacepoint(f, g, desc, ref address, poly);
                        }
                    
                    //Write remaining triangles under a single list header
                    if (g._triangles.Count != 0)
                    {
                        *(PrimitiveHeader*)address = g.TriangleHeader; address += 3;
                        foreach (PointTriangle tri in g._triangles)
                        {
                            WriteFacepoint(tri._x, g, desc, ref address, poly);
                            WriteFacepoint(tri._y, g, desc, ref address, poly);
                            WriteFacepoint(tri._z, g, desc, ref address, poly);
                        }
                    }
                }
                else //Write the original primitives read from the model
                    for (int i = 0; i < g._headers.Count; i++)
                    {
                        *(PrimitiveHeader*)address = g._headers[i];
                        address += 3;
                        foreach (Facepoint point in g._points[i])
                            WriteFacepoint(point, g, desc, ref address, poly);
                    }
            }
        }
        internal void WriteFacepoint(Facepoint f, PrimitiveGroup g, FacepointAttribute[] desc, ref VoidPtr address, MDL0ObjectNode node)
        {
            foreach (FacepointAttribute d in desc)
                switch (d._attr)
                {
                    case GXAttribute.PosNrmMtxId:
                        if (d._type == XFDataFormat.Direct)
                            *(byte*)address++ = (byte)(3 * g._nodes.IndexOf(f.NodeID));
                        break;
                    case GXAttribute.Tex0MtxId:
                    case GXAttribute.Tex1MtxId:
                    case GXAttribute.Tex2MtxId:
                    case GXAttribute.Tex3MtxId:
                    case GXAttribute.Tex4MtxId:
                    case GXAttribute.Tex5MtxId:
                    case GXAttribute.Tex6MtxId:
                    case GXAttribute.Tex7MtxId:
                        if (d._type == XFDataFormat.Direct)
                            *(byte*)address++ = (byte)(30 + (3 * g._nodes.IndexOf(f.NodeID)));
                        break;
                    case GXAttribute.Position:
                        switch (d._type)
                        {
                            case XFDataFormat.Direct:
                                byte* addr = (byte*)address;
                                node.Model._linker._vertices[node._elementIndices[0]].Write(ref addr, f._vertexIndex);
                                address = addr;
                                break;
                            case XFDataFormat.Index8:
                                *(byte*)address++ = (byte)f._vertexIndex;
                                break;
                            case XFDataFormat.Index16:
                                *(bushort*)address = (ushort)f._vertexIndex;
                                address += 2;
                                break;
                        }
                        break;
                    case GXAttribute.Normal:
                        switch (d._type)
                        {
                            case XFDataFormat.Direct:
                                byte* addr = (byte*)address;
                                node.Model._linker._normals[node._elementIndices[1]].Write(ref addr, f._normalIndex);
                                address = addr;
                                break;
                            case XFDataFormat.Index8:
                                *(byte*)address++ = (byte)f._normalIndex;
                                break;
                            case XFDataFormat.Index16:
                                *(bushort*)address = (ushort)f._normalIndex;
                                address += 2;
                                break;
                        }
                        break;
                    case GXAttribute.Color0:
                    case GXAttribute.Color1:
                        switch (d._type)
                        {
                            case XFDataFormat.Direct:
                                int index = (int)d._attr - 11;
                                byte* addr = (byte*)address;
                                node.Model._linker._colors[node._elementIndices[index + 2]].Write(ref addr, f._colorIndices[index]);
                                address = addr;
                                break;
                            case XFDataFormat.Index8:
                                if (_polygon._colorChanged[(int)d._attr - (int)GXAttribute.Color0] && _polygon._colorSet[(int)d._attr - (int)GXAttribute.Color0] != null)
                                    *(byte*)address++ = 0;
                                else
                                    *(byte*)address++ = (byte)f._colorIndices[(int)d._attr - 11];
                                break;
                            case XFDataFormat.Index16:
                                if (_polygon._colorChanged[(int)d._attr - (int)GXAttribute.Color0] && _polygon._colorSet[(int)d._attr - (int)GXAttribute.Color0] != null)
                                    *(bushort*)address = 0;
                                else
                                    *(bushort*)address = (ushort)f._colorIndices[(int)d._attr - 11];
                                address += 2;
                                break;
                        }
                        break;
                    case GXAttribute.Tex0:
                    case GXAttribute.Tex1:
                    case GXAttribute.Tex2:
                    case GXAttribute.Tex3:
                    case GXAttribute.Tex4:
                    case GXAttribute.Tex5:
                    case GXAttribute.Tex6:
                    case GXAttribute.Tex7:
                        switch (d._type)
                        {
                            case XFDataFormat.Direct:
                                int index = (int)d._attr - 13;
                                byte* addr = (byte*)address;
                                node.Model._linker._uvs[node._elementIndices[index + 4]].Write(ref addr, f._UVIndices[index]);
                                address = addr;
                                break;
                            case XFDataFormat.Index8:
                                *(byte*)address++ = (byte)f._UVIndices[(int)d._attr - 13];
                                break;
                            case XFDataFormat.Index16:
                                *(bushort*)address = (ushort)f._UVIndices[(int)d._attr - 13];
                                address += 2;
                                break;
                        }
                        break;
                }
        }

        #endregion

        #region Flags

        public FacepointAttribute[] SetVertexDescList(MDL0ObjectNode obj, params bool[] forceDirect)
        {
            //Everything is set in the order the facepoint is written!

            ModelLinker linker = obj.Model._linker;
            short[] indices = obj._elementIndices;
            XFDataFormat fmt;

            //Create new command list
            List<FacepointAttribute> list = new List<FacepointAttribute>();

            obj._arrayFlags._data = 0;

            if (obj.Weighted)
            {
                list.Add(new FacepointAttribute(GXAttribute.PosNrmMtxId, XFDataFormat.Direct));
                obj._fpStride++;
                obj._arrayFlags.HasPosMatrix = true;

                //There are no texture matrices without a position/normal matrix also
                for (int i = 0; i < 8; i++)
                    if (obj.HasTextureMatrix[i])
                    {
                        list.Add(new FacepointAttribute((GXAttribute)(i + 1), XFDataFormat.Direct));
                        obj._fpStride++;
                        obj._arrayFlags.SetHasTexMatrix(i, true);
                    }
            }
            if (indices[0] > -1 && _faceData[0] != null) //Positions
            {
                obj._arrayFlags.HasPositions = true;
                fmt = (forceDirect != null && forceDirect.Length > 0 && forceDirect[0] == true) ? XFDataFormat.Direct : (XFDataFormat)(GetVertices(false).Length > byte.MaxValue ? 3 : 2);

                list.Add(new FacepointAttribute(GXAttribute.Position, fmt));
                obj._fpStride += fmt == XFDataFormat.Direct ? linker._vertices[obj._elementIndices[0]]._dstStride : (int)fmt - 1;
            }
            if (indices[1] > -1 && _faceData[1] != null) //Normals
            {
                obj._arrayFlags.HasNormals = true;

                fmt = (forceDirect != null && forceDirect.Length > 1 && forceDirect[1] == true) ? XFDataFormat.Direct : (XFDataFormat)(GetNormals(false).Length > byte.MaxValue ? 3 : 2);

                list.Add(new FacepointAttribute(GXAttribute.Normal, fmt));
                obj._fpStride += fmt == XFDataFormat.Direct ? linker._normals[obj._elementIndices[1]]._dstStride : (int)fmt - 1;
            }
            for (int i = 2; i < 4; i++)
                if (indices[i] > -1 && (_faceData[i] != null || obj._colorChanged[i - 2])) //Colors
                {
                    obj._arrayFlags.SetHasColor(i - 2, true);

                    if (obj._colorChanged[i - 2])
                        fmt = XFDataFormat.Index8;
                    else
                        fmt = (forceDirect != null && forceDirect.Length > i && forceDirect[i] == true) ? XFDataFormat.Direct : (XFDataFormat)(GetColors(i - 2, false).Length > byte.MaxValue ? 3 : 2);

                    list.Add(new FacepointAttribute((GXAttribute)(i + 9), fmt));
                    obj._fpStride += fmt == XFDataFormat.Direct ? linker._colors[obj._elementIndices[i]]._dstStride : (int)fmt - 1;
                }
            for (int i = 4; i < 12; i++)
                if (indices[i] > -1 && _faceData[i] != null) //UVs
                {
                    obj._arrayFlags.SetHasUVs(i - 4, true);

                    fmt = (forceDirect != null && forceDirect.Length > i && forceDirect[i] == true) ? XFDataFormat.Direct : (XFDataFormat)(GetUVs(i - 4, false).Length > byte.MaxValue ? 3 : 2);

                    list.Add(new FacepointAttribute((GXAttribute)(i + 9), fmt));
                    obj._fpStride += fmt == XFDataFormat.Direct ? linker._uvs[obj._elementIndices[i]]._dstStride : (int)fmt - 1;
                }

            return list.ToArray();
        }

        //This sets up how to read the facepoints that are going to be written.
        public void WriteVertexDescriptor(FacepointAttribute[] attributes, MDL0ObjectNode obj)
        {
            XFNormalFormat nrmFmt = XFNormalFormat.None;
            int numColors = 0;
            int numUVs = 0;

            uint posNrmMtxId = 0;
            uint texMtxIdMask = 0;
            uint pos = 0;
            uint nrm = 0;
            uint col0 = 0;
            uint col1 = 0;
            uint tex0 = 0;
            uint tex1 = 0;
            uint tex2 = 0;
            uint tex3 = 0;
            uint tex4 = 0;
            uint tex5 = 0;
            uint tex6 = 0;
            uint tex7 = 0;

            foreach (FacepointAttribute attrPtr in attributes)
                switch (attrPtr._attr)
                {
                    case GXAttribute.PosNrmMtxId:
                        posNrmMtxId = (uint)attrPtr._type;
                        break;

                    case GXAttribute.Tex0MtxId:
                        texMtxIdMask = (uint)(texMtxIdMask & ~1) | ((uint)attrPtr._type << 0);
                        break;
                    case GXAttribute.Tex1MtxId:
                        texMtxIdMask = (uint)(texMtxIdMask & ~2) | ((uint)attrPtr._type << 1);
                        break;
                    case GXAttribute.Tex2MtxId:
                        texMtxIdMask = (uint)(texMtxIdMask & ~4) | ((uint)attrPtr._type << 2);
                        break;
                    case GXAttribute.Tex3MtxId:
                        texMtxIdMask = (uint)(texMtxIdMask & ~8) | ((uint)attrPtr._type << 3);
                        break;
                    case GXAttribute.Tex4MtxId:
                        texMtxIdMask = (uint)(texMtxIdMask & ~16) | ((uint)attrPtr._type << 4);
                        break;
                    case GXAttribute.Tex5MtxId:
                        texMtxIdMask = (uint)(texMtxIdMask & ~32) | ((uint)attrPtr._type << 5);
                        break;
                    case GXAttribute.Tex6MtxId:
                        texMtxIdMask = (uint)(texMtxIdMask & ~64) | ((uint)attrPtr._type << 6);
                        break;
                    case GXAttribute.Tex7MtxId:
                        texMtxIdMask = (uint)(texMtxIdMask & ~128) | ((uint)attrPtr._type << 7);
                        break;

                    case GXAttribute.Position:
                        pos = (uint)attrPtr._type;
                        break;

                    case GXAttribute.Normal:
                        if (attrPtr._type != XFDataFormat.None)
                        { 
                            nrm = (uint)attrPtr._type;
                            nrmFmt = XFNormalFormat.XYZ; 
                        } 
                        break;

                    case GXAttribute.NBT:
                        if (attrPtr._type != XFDataFormat.None)
                        {
                            nrm = (uint)attrPtr._type; 
                            nrmFmt = XFNormalFormat.NBT; 
                        } 
                        break;

                    case GXAttribute.Color0: col0 = (uint)attrPtr._type; numColors += (col0 != 0 ? 1 : 0); break;
                    case GXAttribute.Color1: col1 = (uint)attrPtr._type; numColors += (col1 != 0 ? 1 : 0); break;

                    case GXAttribute.Tex0: tex0 = (uint)attrPtr._type; numUVs += (tex0 != 0 ? 1 : 0); break;
                    case GXAttribute.Tex1: tex1 = (uint)attrPtr._type; numUVs += (tex1 != 0 ? 1 : 0); break;
                    case GXAttribute.Tex2: tex2 = (uint)attrPtr._type; numUVs += (tex2 != 0 ? 1 : 0); break;
                    case GXAttribute.Tex3: tex3 = (uint)attrPtr._type; numUVs += (tex3 != 0 ? 1 : 0); break;
                    case GXAttribute.Tex4: tex4 = (uint)attrPtr._type; numUVs += (tex4 != 0 ? 1 : 0); break;
                    case GXAttribute.Tex5: tex5 = (uint)attrPtr._type; numUVs += (tex5 != 0 ? 1 : 0); break;
                    case GXAttribute.Tex6: tex6 = (uint)attrPtr._type; numUVs += (tex6 != 0 ? 1 : 0); break;
                    case GXAttribute.Tex7: tex7 = (uint)attrPtr._type; numUVs += (tex7 != 0 ? 1 : 0); break;
                }

            obj._vertexFormat._lo = ShiftVtxLo(posNrmMtxId, texMtxIdMask, pos, nrm, col0, col1);
            obj._vertexFormat._hi = ShiftVtxHi(tex0, tex1, tex2, tex3, tex4, tex5, tex6, tex7);
            obj._vertexSpecs = new XFVertexSpecs(numColors, numUVs, nrmFmt);
        }

        public VertexAttributeFormat[] SetFormatList(MDL0ObjectNode polygon, ModelLinker linker)
        {
            List<VertexAttributeFormat> list = new List<VertexAttributeFormat>();
            VertexCodec vert = null;
            ColorCodec col = null;

            for (int i = 0; i < 12; i++)
            {
                if (polygon._manager._faceData[i] != null)
                    switch (i)
                    {
                        case 0: //Positions
                            if (linker._vertices != null && linker._vertices.Count != 0 && polygon._elementIndices[0] != -1)
                                if ((vert = linker._vertices[polygon._elementIndices[0]]) != null)
                                    list.Add(new VertexAttributeFormat(
                                        GXAttribute.Position,
                                        (GXCompType)vert._type,
                                        (GXCompCnt)(vert._hasZ ? 1 : 0),
                                        (byte)vert._scale));
                            break;
                        case 1: //Normals
                            vert = null;
                            if (linker._normals != null && linker._normals.Count != 0 && polygon._elementIndices[1] != -1)
                                if ((vert = linker._normals[polygon._elementIndices[1]]) != null)
                                    list.Add(new VertexAttributeFormat(
                                        GXAttribute.Normal,
                                        (GXCompType)vert._type,
                                        GXCompCnt.NrmXYZ,
                                        (byte)vert._scale));
                            break;
                        case 2: //Color 1
                        case 3: //Color 2
                            col = null;
                            if (linker._colors != null && linker._colors.Count != 0 && polygon._elementIndices[i] != -1 && (col = linker._colors[polygon._elementIndices[i]]) != null)
                                list.Add(new VertexAttributeFormat(
                                    (GXAttribute)((int)GXAttribute.Color0 + (i - 2)),
                                    (GXCompType)col._outType,
                                    (GXCompCnt)(col._hasAlpha ? 1 : 0),
                                    0));
                            break;
                        case 4: //Tex 1
                        case 5: //Tex 2
                        case 6: //Tex 3
                        case 7: //Tex 4
                        case 8: //Tex 5
                        case 9: //Tex 6
                        case 10: //Tex 7
                        case 11: //Tex 8
                            vert = null;
                            if (linker._uvs != null && linker._uvs.Count != 0 && polygon._elementIndices[i] != -1)
                                if ((vert = linker._uvs[polygon._elementIndices[i]]) != null)
                                    list.Add(new VertexAttributeFormat(
                                        (GXAttribute)((int)GXAttribute.Tex0 + (i - 4)),
                                        (GXCompType)vert._type,
                                        GXCompCnt.TexST,
                                        (byte)vert._scale));
                            break;
                    }
            }
            return list.ToArray();
        }

        public void WriteVertexFormat(VertexAttributeFormat[] fmtList, MDL0Object* polygon)
        {
            //These are default values.

            uint posCnt = (int)GXCompCnt.PosXYZ;
            uint posType = (int)GXCompType.Float;
            uint posFrac = 0;

            uint nrmCnt = (int)GXCompCnt.NrmXYZ;
            uint nrmType = (int)GXCompType.Float;
            uint nrmId3 = 0;

            uint c0Cnt = (int)GXCompCnt.ClrRGBA;
            uint c0Type = (int)GXCompType.RGBA8;
            uint c1Cnt = (int)GXCompCnt.ClrRGBA;
            uint c1Type = (int)GXCompType.RGBA8;

            uint tx0Cnt = (int)GXCompCnt.TexST;
            uint tx0Type = (int)GXCompType.Float;
            uint tx0Frac = 0;
            uint tx1Cnt = (int)GXCompCnt.TexST;
            uint tx1Type = (int)GXCompType.Float;
            uint tx1Frac = 0;
            uint tx2Cnt = (int)GXCompCnt.TexST;
            uint tx2Type = (int)GXCompType.Float;
            uint tx2Frac = 0;
            uint tx3Cnt = (int)GXCompCnt.TexST;
            uint tx3Type = (int)GXCompType.Float;
            uint tx3Frac = 0;
            uint tx4Cnt = (int)GXCompCnt.TexST;
            uint tx4Type = (int)GXCompType.Float;
            uint tx4Frac = 0;
            uint tx5Cnt = (int)GXCompCnt.TexST;
            uint tx5Type = (int)GXCompType.Float;
            uint tx5Frac = 0;
            uint tx6Cnt = (int)GXCompCnt.TexST;
            uint tx6Type = (int)GXCompType.Float;
            uint tx6Frac = 0;
            uint tx7Cnt = (int)GXCompCnt.TexST;
            uint tx7Type = (int)GXCompType.Float;
            uint tx7Frac = 0;

            foreach (VertexAttributeFormat list in fmtList)
                switch (list._attr)
                {
                    case GXAttribute.Position:
                        posCnt = (uint)list._cnt;
                        posType = (uint)list._type;
                        posFrac = list._frac;
                        break;
                    case GXAttribute.Normal:
                    case GXAttribute.NBT:
                        nrmType = (uint)list._type;
                        if (list._cnt == GXCompCnt.NrmNBT3)
                        {
                            nrmCnt = (uint)GXCompCnt.NrmNBT;
                            nrmId3 = 1;
                        }
                        else
                        {
                            nrmCnt = (uint)list._cnt;
                            nrmId3 = 0;
                        }
                        break;
                    case GXAttribute.Color0:
                        c0Cnt = (uint)list._cnt;
                        c0Type = (uint)list._type;
                        break;
                    case GXAttribute.Color1:
                        c1Cnt = (uint)list._cnt;
                        c1Type = (uint)list._type;
                        break;
                    case GXAttribute.Tex0:
                        tx0Cnt = (uint)list._cnt;
                        tx0Type = (uint)list._type;
                        tx0Frac = list._frac;
                        break;
                    case GXAttribute.Tex1:
                        tx1Cnt = (uint)list._cnt;
                        tx1Type = (uint)list._type;
                        tx1Frac = list._frac;
                        break;
                    case GXAttribute.Tex2:
                        tx2Cnt = (uint)list._cnt;
                        tx2Type = (uint)list._type;
                        tx2Frac = list._frac;
                        break;
                    case GXAttribute.Tex3:
                        tx3Cnt = (uint)list._cnt;
                        tx3Type = (uint)list._type;
                        tx3Frac = list._frac;
                        break;
                    case GXAttribute.Tex4:
                        tx4Cnt = (uint)list._cnt;
                        tx4Type = (uint)list._type;
                        tx4Frac = list._frac;
                        break;
                    case GXAttribute.Tex5:
                        tx5Cnt = (uint)list._cnt;
                        tx5Type = (uint)list._type;
                        tx5Frac = list._frac;
                        break;
                    case GXAttribute.Tex6:
                        tx6Cnt = (uint)list._cnt;
                        tx6Type = (uint)list._type;
                        tx6Frac = list._frac;
                        break;
                    case GXAttribute.Tex7:
                        tx7Cnt = (uint)list._cnt;
                        tx7Type = (uint)list._type;
                        tx7Frac = list._frac;
                        break;
                }
            
            MDL0PolygonDefs* Defs = (MDL0PolygonDefs*)polygon->DefList;
            Defs->UVATA = (uint)ShiftUVATA(posCnt, posType, posFrac, nrmCnt, nrmType, c0Cnt, c0Type, c1Cnt, c1Type, tx0Cnt, tx0Type, tx0Frac, nrmId3);
            Defs->UVATB = (uint)ShiftUVATB(tx1Cnt, tx1Type, tx1Frac, tx2Cnt, tx2Type, tx2Frac, tx3Cnt, tx3Type, tx3Frac, tx4Cnt, tx4Type);
            Defs->UVATC = (uint)ShiftUVATC(tx4Frac, tx5Cnt, tx5Type, tx5Frac, tx6Cnt, tx6Type, tx6Frac, tx7Cnt, tx7Type, tx7Frac);
        }

        #endregion

        #region Shifts

        //Vertex Format Lo Shift
        public uint ShiftVtxLo(uint pmidx, uint t76543210midx, uint pos, uint nrm, uint col0, uint col1)
        {
            return ((((uint)(pmidx)) << 0) |
                    (((uint)(t76543210midx)) << 1) |
                    (((uint)(pos)) << 9) |
                    (((uint)(nrm)) << 11) |
                    (((uint)(col0)) << 13) |
                    (((uint)(col1)) << 15));
        }

        //Vertex Format Hi Shift
        public uint ShiftVtxHi(uint tex0, uint tex1, uint tex2, uint tex3, uint tex4, uint tex5, uint tex6, uint tex7)
        {
            return ((((uint)(tex0)) << 0) |
                    (((uint)(tex1)) << 2) |
                    (((uint)(tex2)) << 4) |
                    (((uint)(tex3)) << 6) |
                    (((uint)(tex4)) << 8) |
                    (((uint)(tex5)) << 10) |
                    (((uint)(tex6)) << 12) |
                    (((uint)(tex7)) << 14));
        }

        //XF Specs Shift
        public uint ShiftXFSpecs(uint host_colors, uint host_normal, uint host_textures)
        {
            return ((((uint)(host_colors)) << 0) |
                    (((uint)(host_normal)) << 2) |
                    (((uint)(host_textures)) << 4));
        }

        //UVAT Group A Shift
        public uint ShiftUVATA(uint posCnt, uint posFmt, uint posShft, uint nrmCnt, uint nrmFmt, uint Col0Cnt, uint Col0Fmt, uint Col1Cnt, uint Col1Fmt, uint tex0Cnt, uint tex0Fmt, uint tex0Shft, uint normalIndex3)
        {
            return ((((uint)(posCnt)) << 0) |
                    (((uint)(posFmt)) << 1) |
                    (((uint)(posShft)) << 4) |
                    (((uint)(nrmCnt)) << 9) |
                    (((uint)(nrmFmt)) << 10) |
                    (((uint)(Col0Cnt)) << 13) |
                    (((uint)(Col0Fmt)) << 14) |
                    (((uint)(Col1Cnt)) << 17) |
                    (((uint)(Col1Fmt)) << 18) |
                    (((uint)(tex0Cnt)) << 21) |
                    (((uint)(tex0Fmt)) << 22) |
                    (((uint)(tex0Shft)) << 25) |
                    (((uint)(1)) << 30) | //Should always be 1
                    (((uint)(normalIndex3)) << 31));
        }

        //UVAT Group B Shift
        public uint ShiftUVATB(uint tex1Cnt, uint tex1Fmt, uint tex1Shft, uint tex2Cnt, uint tex2Fmt, uint tex2Shft, uint tex3Cnt, uint tex3Fmt, uint tex3Shft, uint tex4Cnt, uint tex4Fmt)
        {
            return ((((uint)(tex1Cnt)) << 0) |
                    (((uint)(tex1Fmt)) << 1) |
                    (((uint)(tex1Shft)) << 4) |
                    (((uint)(tex2Cnt)) << 9) |
                    (((uint)(tex2Fmt)) << 10) |
                    (((uint)(tex2Shft)) << 13) |
                    (((uint)(tex3Cnt)) << 18) |
                    (((uint)(tex3Fmt)) << 19) |
                    (((uint)(tex3Shft)) << 22) |
                    (((uint)(tex4Cnt)) << 27) |
                    (((uint)(tex4Fmt)) << 28) |
                    (((uint)(1)) << 31)); //Should always be 1
        }

        //UVAT Group C Shift
        public uint ShiftUVATC(uint tex4Shft, uint tex5Cnt, uint tex5Fmt, uint tex5Shft, uint tex6Cnt, uint tex6Fmt, uint tex6Shft, uint tex7Cnt, uint tex7Fmt, uint tex7Shft)
        {
            return ((((uint)(tex4Shft)) << 0) |
                    (((uint)(tex5Cnt)) << 5) |
                    (((uint)(tex5Fmt)) << 6) |
                    (((uint)(tex5Shft)) << 9) |
                    (((uint)(tex6Cnt)) << 14) |
                    (((uint)(tex6Fmt)) << 15) |
                    (((uint)(tex6Shft)) << 18) |
                    (((uint)(tex7Cnt)) << 23) |
                    (((uint)(tex7Fmt)) << 24) |
                    (((uint)(tex7Shft)) << 27));
        }
        #endregion

        #region Rendering

        private void CalcStride()
        {
            _stride = 0;
            for (int i = 0; i < 2; i++)
                if (_faceData[i] != null)
                    _stride += 12;
            for (int i = 2; i < 4; i++)
                if (_faceData[i] != null)
                    _stride += 4;
            for (int i = 4; i < 12; i++)
                if (_faceData[i] != null)
                    _stride += 8;
        }

        internal Facepoint[] MergeExternalFaceData(MDL0ObjectNode poly)
        {
            Facepoint[] _facepoints = new Facepoint[_pointCount];

            ushort* pIndex = (ushort*)_indices.Address;
            for (int x = 0; x < 12; x++)
            {
                if (poly._elementIndices[x] < 0 && x != 0)
                    continue;

                switch (x)
                {
                    case 0:
                        Vector3* pIn0 = (Vector3*)_faceData[x].Address;
                        for (int i = 0; i < _pointCount; i++)
                        {
                            Facepoint f = _facepoints[i] = new Facepoint() { _index = i };
                            if (_vertices.Count != 0)
                            {
                                ushort id = *pIndex++;
                                if (id < _vertices.Count && id >= 0)
                                    f._vertex = _vertices[id];
                                f._vertexIndex = f._vertex._facepoints[0]._vertexIndex;
                            }
                        }
                        break;
                    case 1:
                        Vector3* pIn1 = (Vector3*)_faceData[x].Address;
                        for (int i = 0; i < _pointCount; i++)
                            _facepoints[i]._normalIndex = Array.IndexOf(poly._normalNode.Normals, *pIn1++);
                        break;
                    case 2:
                    case 3:
                        RGBAPixel* pIn2 = (RGBAPixel*)_faceData[x].Address;
                        for (int i = 0; i < _pointCount; i++)
                            _facepoints[i]._colorIndices[x - 2] = Array.IndexOf(poly._colorSet[x - 2].Colors, *pIn2++);
                        break;
                    default:
                        Vector2* pIn3 = (Vector2*)_faceData[x].Address;
                        for (int i = 0; i < _pointCount; i++)
                            _facepoints[i]._UVIndices[x - 4] = Array.IndexOf(poly._uvSet[x - 4].Points, *pIn3++);
                        break;
                }
            }
            return _facepoints;
        }

        internal int[] _newClrObj = new int[2];
        internal Facepoint[] MergeInternalFaceData(MDL0ObjectNode poly)
        {
            Facepoint[] _facepoints = new Facepoint[_pointCount];

            ushort* pIndex = (ushort*)_indices.Address;
            for (int x = 0; x < 12; x++)
            {
                if (_faceData[x] == null && x != 0)
                    continue;

                switch (x)
                {
                    case 0:
                        for (int i = 0; i < _pointCount; i++)
                            if (_vertices.Count != 0)
                            {
                                Facepoint f = _facepoints[i] = new Facepoint() { _index = i };
                                f._vertexIndex = *pIndex++;
                                if (f._vertexIndex < _vertices.Count && f._vertexIndex >= 0)
                                    f._vertex = _vertices[f._vertexIndex];
                            }
                        break;
                    case 1:
                        Vector3* pIn1 = (Vector3*)_faceData[x].Address;
                        for (int i = 0; i < _pointCount; i++)
                            _facepoints[i]._normalIndex = Array.IndexOf(GetNormals(false), *pIn1++);
                        break;
                    case 2:
                    case 3:
                        RGBAPixel* pIn2 = (RGBAPixel*)_faceData[x].Address;
                        if (Collada._importOptions._useOneNode)
                            for (int i = 0; i < _pointCount; i++)
                                _facepoints[i]._colorIndices[x - 2] = Array.IndexOf(Collada._importOptions._singleColorNodeEntries, *pIn2++);
                        else
                            for (int i = 0; i < _pointCount; i++)
                                _facepoints[i]._colorIndices[x - 2] = Array.IndexOf(((MDL0ObjectNode)_polygon.Model._objList[_newClrObj[x - 2]])._manager.GetColors(x - 2, false), *pIn2++);
                        break;
                    default:
                        Vector2* pIn3 = (Vector2*)_faceData[x].Address;
                        for (int i = 0; i < _pointCount; i++)
                            _facepoints[i]._UVIndices[x - 4] = Array.IndexOf(GetUVs(x - 4, false), *pIn3++);
                        break;
                }
            }
            return _facepoints;
        }

        internal void Weight()
        {
            if (_vertices != null)
                foreach (Vertex3 v in _vertices)
                    v.Weight();
            _dirty[0] = true;
            _dirty[1] = true;
        }

        public void Bind()
        {
            //GL.GenBuffers(1, out _arrayBufferHandle);
            //GL.GenBuffers(1, out _elementArrayBufferHandle);

            //GL.GenVertexArrays(1, out _arrayHandle);
            //GL.BindVertexArray(_arrayHandle);

            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementArrayBufferHandle);
            //VoidPtr ptr = _triangles._indices.Address;
            //GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(_triangles._indices.Length),
            //    ptr, BufferUsageHint.StaticDraw);
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Unbind()
        {
            //if (_arrayHandle != -1) 
            //    GL.DeleteVertexArrays(1, ref _arrayHandle);
            //if (_arrayBufferHandle != -1) 
            //    GL.DeleteBuffers(1, ref _arrayBufferHandle);
            //if (_elementArrayBufferHandle != -1) 
            //    GL.DeleteBuffers(1, ref _elementArrayBufferHandle);
        }

        internal void UpdateStream(int index)
        {
            _dirty[index] = false;

            if (_faceData[index] == null || _vertices == null || _vertices.Count == 0)
                return;

            //Set starting address
            byte* pOut = (byte*)_graphicsBuffer.Address;
            for (int i = 0; i < index; i++)
                if (_faceData[i] != null)
                    if (i < 2)
                        pOut += 12;
                    else if (i < 4)
                        pOut += 4;
                    else
                        pOut += 8;

            int v;
            ushort* pIndex = (ushort*)_indices.Address;
            if (index == 0) //Vertices
            {
                for (int i = 0; i < _pointCount; i++, pOut += _stride)
                    if ((v = *pIndex++) < _vertices.Count && v >= 0)
                        *(Vector3*)pOut = _vertices[v].WeightedPosition;
            }
            else if (index == 1) //Normals
            {
                Vector3* pIn = (Vector3*)_faceData[index].Address;
                for (int i = 0; i < _pointCount; i++, pOut += _stride)
                    if ((v = *pIndex++) < _vertices.Count && v >= 0)
                        *(Vector3*)pOut = *pIn++ * _vertices[v].GetMatrix().GetRotationMatrix();
                    else
                        *(Vector3*)pOut = *pIn++;
            }
            else if (index < 4) //Colors
            {
                RGBAPixel* pIn = (RGBAPixel*)_faceData[index].Address;
                for (int i = 0; i < _pointCount; i++, pOut += _stride)
                    *(RGBAPixel*)pOut = *pIn++;
            }
            else //UVs
            {
                Vector2* pIn = (Vector2*)_faceData[index].Address;
                for (int i = 0; i < _pointCount; i++, pOut += _stride)
                    *(Vector2*)pOut = *pIn++;
            }
        }

        internal unsafe void PrepareStream()
        {
            CalcStride();
            int bufferSize = _stride * _pointCount;

            //Dispose of buffer if size doesn't match
            if ((_graphicsBuffer != null) && (_graphicsBuffer.Length != bufferSize))
            {
                _graphicsBuffer.Dispose();
                _graphicsBuffer = null;
            }

            //Create data buffer
            if (_graphicsBuffer == null)
            {
                _graphicsBuffer = new UnsafeBuffer(bufferSize);
                for (int i = 0; i < 12; i++)
                    _dirty[i] = true;
            }

            //Update streams before binding
            for (int i = 0; i < 12; i++)
                if (_dirty[i])
                    UpdateStream(i);

            byte* pData = (byte*)_graphicsBuffer.Address;
            for (int i = 0; i < 12; i++)
                if (_faceData[i] != null)
                    switch (i)
                    {
                        case 0:
                            GL.EnableClientState(ArrayCap.VertexArray);
                            GL.VertexPointer(3, VertexPointerType.Float, _stride, (IntPtr)pData);
                            pData += 12;
                            break;
                        case 1:
                            GL.EnableClientState(ArrayCap.NormalArray);
                            GL.NormalPointer(NormalPointerType.Float, _stride, (IntPtr)pData);
                            pData += 12;
                            break;
                        case 2:
                            GL.EnableClientState(ArrayCap.ColorArray);
                            GL.ColorPointer(4, ColorPointerType.Byte, _stride, (IntPtr)pData);
                            pData += 4;
                            break;
                        case 3:
                            GL.EnableClientState(ArrayCap.SecondaryColorArray);
                            GL.SecondaryColorPointer(4, ColorPointerType.Byte, _stride, (IntPtr)pData);
                            pData += 4;
                            break;
                        default:
                            pData += 8;
                            break;
                    }
        }

        internal unsafe void PrepareStreamNew(int programHandle)
        {
            CalcStride();
            int bufferSize = _stride * _pointCount;

            //Dispose of buffer if size doesn't match
            if ((_graphicsBuffer != null) && (_graphicsBuffer.Length != bufferSize))
            {
                _graphicsBuffer.Dispose();
                _graphicsBuffer = null;
            }

            //Create data buffer
            if (_graphicsBuffer == null)
            {
                _graphicsBuffer = new UnsafeBuffer(bufferSize);
                for (int i = 0; i < 12; i++)
                    _dirty[i] = true;
            }

            //Update streams before binding
            for (int i = 0; i < 12; i++)
                if (_dirty[i])
                    UpdateStream(i);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _arrayBufferHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementArrayBufferHandle);

            VoidPtr ptr = _graphicsBuffer.Address;
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_graphicsBuffer.Length),
                ptr, BufferUsageHint.StreamCopy);

            GL.BindVertexArray(_arrayHandle);

            int offset = 0;
            int index = 0;
            for (int i = 0; i < 12; i++)
                if (_faceData[i] != null)
                {
                    GL.EnableVertexAttribArray(index);
                    switch (i)
                    {
                        case 0:
                        case 1:
                            GL.VertexAttribPointer(index, 3, VertexAttribPointerType.Float, false, _stride, offset);
                            GL.BindAttribLocation(programHandle, index++, i == 0 ? "Position" : "Normal");
                            offset += 12;
                            break;
                        case 2:
                        case 3:
                            GL.VertexAttribPointer(index, 4, VertexAttribPointerType.UnsignedByte, false, _stride, offset);
                            GL.BindAttribLocation(programHandle, index++, String.Format("Color{0}", i - 2));
                            offset += 4;
                            break;
                        default:
                            GL.VertexAttribPointer(index, 2, VertexAttribPointerType.Float, false, _stride, offset);
                            GL.BindAttribLocation(programHandle, index++, String.Format("UV{0}", i - 4));
                            offset += 8;
                            break;
                    }
                }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        internal unsafe void DetachStreamsNew()
        {
            int index = 0;
            for (int i = 0; i < 12; i++)
                if (_faceData[i] != null)
                    GL.DisableVertexAttribArray(index++);

            GL.Disable(EnableCap.Texture2D);
        }

        public int _arrayHandle;
        public int _arrayBufferHandle;
        public int _elementArrayBufferHandle;
        internal unsafe void DetachStreams()
        {
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.Disable(EnableCap.Texture2D);
        }

        internal void ApplyTexture(MDL0MaterialRefNode texgen)
        {
            if (texgen != null)
            {
                int texId = texgen.TextureCoordId;
                texId = texId < 0 ? 0 : texId;
                if ((texId >= 0) && (_faceData[texId += 4] != null))
                {
                    byte* pData = (byte*)_graphicsBuffer.Address;
                    //int pData = 0;
                    for (int i = 0; i < texId; i++)
                        if (_faceData[i] != null)
                            if (i < 2)
                                pData += 12;
                            else if (i < 4)
                                pData += 4;
                            else
                                pData += 8;

                    GL.Enable(EnableCap.Texture2D);
                    GL.EnableClientState(ArrayCap.TextureCoordArray);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, _stride, (IntPtr)pData);
                    //GL.EnableVertexAttribArray(texId);
                    //GL.VertexAttribPointer(texId, 2, VertexAttribPointerType.Float, true, _stride, pData);
                    //GL.BindAttribLocation(_polygon.shaderProgramHandle, texId - 4, "tex" + (texId - 4));
                }
                else
                {
                    if (texId < 0)
                    {
                        switch (texId)
                        {
                            case -1: //Vertex coords 
                            case -2: //Normal coords 
                            case -3: //Color coords 	
                            case -4: //Binormal B coords 			
                            case -5: //Binormal T coords 		
                            default:
                                GL.DisableClientState(ArrayCap.TextureCoordArray);
                                GL.Disable(EnableCap.Texture2D);
                                break;
                        }
                    }
                    else
                    {
                        GL.DisableClientState(ArrayCap.TextureCoordArray);
                        GL.Disable(EnableCap.Texture2D);
                    }
                }
            }
            else
            {
                GL.DisableClientState(ArrayCap.TextureCoordArray);
                GL.Disable(EnableCap.Texture2D);
            }
        }

        public void RenderMesh()
        {
            if (_triangles != null)
                _triangles.Render();
            if (_lines != null)
                _lines.Render();
            if (_points != null)
                _points.Render();
        }

        public static Color DefaultVertColor = Color.FromArgb(0, 128, 0);
        internal Color _vertColor = Color.Transparent;

        public static Color DefaultNormColor = Color.FromArgb(0, 0, 128);
        internal Color _normColor = Color.Transparent;

        public static float NormalLength = 0.5f;

        public const float _nodeRadius = 0.05f;
        const float _nodeAdj = 0.01f;

        public bool _render = true;
        public bool _renderNormals = true;
        
        internal unsafe void RenderVerts(TKContext ctx, IMatrixNode _singleBind, MDL0BoneNode selectedBone, Vector3 cam, bool pass2)
        {
            if (!_render)
                return;

            foreach (Vertex3 v in _vertices)
            {
                Color w = v._highlightColor != Color.Transparent ? v._highlightColor : (_singleBind != null && _singleBind == selectedBone) ? Color.Red : v.GetWeightColor(selectedBone);
                if (w != Color.Transparent)
                    GL.Color4(w);
                else
                    GL.Color4(DefaultVertColor);

                float d = cam.DistanceTo(_singleBind == null ? v.WeightedPosition : _singleBind.Matrix * v.WeightedPosition);
                if (d == 0) d = 0.000000000001f;
                GL.PointSize((5000 / d).Clamp(1.0f, !pass2 ? 5.0f : 8.0f));

                GL.Begin(BeginMode.Points);
                GL.Vertex3(v.WeightedPosition._x, v.WeightedPosition._y, v.WeightedPosition._z);
                GL.End();
            }
        }
        internal unsafe void RenderNormals(TKContext ctx, ModelPanel mainWindow)
        {
            if (!_render || _faceData[1] == null)
                return;

            ushort* indices = (ushort*)_indices.Address;
            Vector3* normals = (Vector3*)_faceData[1].Address;

            if (_normColor != Color.Transparent)
                GL.Color4(_normColor);
            else
                GL.Color4(DefaultNormColor);

            for (int i = 0; i < _pointCount; i++)
            {
                GL.PushMatrix();

                GL.Color4(Color.Blue);

                Vertex3 n = _vertices[indices[i]];
                Vector3 w = normals[i] * n.GetMatrix().GetRotationMatrix();
                
                Matrix m = Matrix.TransformMatrix(new Vector3(NormalLength), new Vector3(), n.WeightedPosition);
                GL.MultMatrix((float*)&m);

                GL.Begin(BeginMode.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(w._x, w._y, w._z);
                GL.End();

                GL.PopMatrix();
            }
        }

        #endregion

        internal unsafe PrimitiveManager Clone() { return MemberwiseClone() as PrimitiveManager; }
    }

    public unsafe class NewPrimitive// : IDisposable
    {
        public BeginMode _type;
        public int _elementCount;
        public uint[] _indices;

        public NewPrimitive(int elements, BeginMode type)
        {
            _elementCount = elements;
            _type = type;
            _indices = new uint[_elementCount];
        }

        //~NewPrimitive() { Dispose(); }
        //public void Dispose()
        //{
        //    if (_indices != null)
        //    {
        //        _indices.Dispose();
        //        _indices = null;
        //    }
        //}

        public unsafe void Render()
        {
            GL.DrawElements(_type, _elementCount, DrawElementsType.UnsignedInt, _indices);
        }
    }
}
