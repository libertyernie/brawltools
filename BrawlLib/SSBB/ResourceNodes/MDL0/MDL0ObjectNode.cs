using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.OpenGL;
using System.Collections.Generic;
using BrawlLib.Modeling;
using BrawlLib.Wii.Models;
using BrawlLib.Wii.Graphics;
using System.Windows.Forms;
using BrawlLib.Imaging;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using BrawlLib.Modeling.Triangle_Converter;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe partial class MDL0ObjectNode : MDL0EntryNode, IMatrixNodeUser, IRenderedObject
    {
        internal MDL0Object* Header { get { return (MDL0Object*)WorkingUncompressed.Address; } }

        public override ResourceType ResourceType { get { return ResourceType.MDL0Object; } }

        #region Attributes

        public List<Vertex3> Vertices { get { return _manager != null ? _manager._vertices : null; } }

        internal bool Weighted { get { return _nodeId == -1 || _matrixNode == null; } }
        internal bool HasTexMtx
        {
            get
            {
                for (int i = 0; i < 8; i++)
                    if (HasTextureMatrix[i])
                        return true;
                return false;
            }
        }

        public byte _drawIndex;
        public byte DrawPriority
        {
            get { return _drawIndex; }
            set
            {
                _drawIndex = value;
                SignalPropertyChange();
            }
        }

        [Category("Object Data"), Browsable(false)]
        public int TotalLen { get { return _totalLength; } }
        [Category("Object Data"), Browsable(false)]
        public int MDL0Offset { get { return _mdl0Offset; } }

        internal CPVertexFormat _vertexFormat;
        internal XFArrayFlags _arrayFlags;
        internal XFVertexSpecs _vertexSpecs;

        internal CPElementSpec UVATGroups;

        public bool[] HasTextureMatrix = new bool[8];
        public bool HasPosMatrix = false;

        [Category("Texture Matrices")]
        public bool HasTextureMatrix0 { get { return HasTextureMatrix[0]; } set { if (!Weighted) return; HasTextureMatrix[0] = value; SignalPropertyChange(); _rebuild = true; } }
        [Category("Texture Matrices")]
        public bool HasTextureMatrix1 { get { return HasTextureMatrix[1]; } set { if (!Weighted) return; HasTextureMatrix[1] = value; SignalPropertyChange(); _rebuild = true; } }
        [Category("Texture Matrices")]
        public bool HasTextureMatrix2 { get { return HasTextureMatrix[2]; } set { if (!Weighted) return; HasTextureMatrix[2] = value; SignalPropertyChange(); _rebuild = true; } }
        [Category("Texture Matrices")]
        public bool HasTextureMatrix3 { get { return HasTextureMatrix[3]; } set { if (!Weighted) return; HasTextureMatrix[3] = value; SignalPropertyChange(); _rebuild = true; } }
        [Category("Texture Matrices")]
        public bool HasTextureMatrix4 { get { return HasTextureMatrix[4]; } set { if (!Weighted) return; HasTextureMatrix[4] = value; SignalPropertyChange(); _rebuild = true; } }
        [Category("Texture Matrices")]
        public bool HasTextureMatrix5 { get { return HasTextureMatrix[5]; } set { if (!Weighted) return; HasTextureMatrix[5] = value; SignalPropertyChange(); _rebuild = true; } }
        [Category("Texture Matrices")]
        public bool HasTextureMatrix6 { get { return HasTextureMatrix[6]; } set { if (!Weighted) return; HasTextureMatrix[6] = value; SignalPropertyChange(); _rebuild = true; } }
        [Category("Texture Matrices")]
        public bool HasTextureMatrix7 { get { return HasTextureMatrix[7]; } set { if (!Weighted) return; HasTextureMatrix[7] = value; SignalPropertyChange(); _rebuild = true; } }

        [Category("Object Data")]
        public ObjFlag Flags { get { return (ObjFlag)_flag; } set { _flag = (int)value; SignalPropertyChange(); } }
        [Category("Object Data")]
        public int ID { get { return _entryIndex; } }
        [Category("Object Data")]
        public int FacepointCount { get { return _numFacepoints; } }
        [Category("Object Data")]
        public int VertexCount { get { return _manager == null || _manager._vertices == null ? 0 : _manager._vertices.Count; } }
        [Category("Object Data")]
        public int FaceCount { get { return _numFaces; } }

        internal List<IMatrixNode> _influences;
        [Browsable(false)]
        public List<IMatrixNode> Influences { get { return _influences; } }

        #endregion

        #region Linked Sets

        #region Vertices & Normals

        internal MDL0VertexNode _vertexNode;
        internal MDL0NormalNode _normalNode;

        [TypeConverter(typeof(DropDownListVertices))]
        public string VertexNode
        {
            get { return _vertexNode == null ? null : _vertexNode._name; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    MDL0VertexNode node = Model.FindChild(String.Format("Vertices/{0}", value), false) as MDL0VertexNode;
                    if (node != null && _vertexNode != null && node.NumVertices >= _vertexNode.NumVertices)
                    {
                        if (_vertexNode != null && _vertexNode._objects.Contains(this))
                            _vertexNode._objects.Remove(this);

                        (_vertexNode = node)._objects.Add(this);
                        _elementIndices[0] = (short)node.Index;
                    }
                }
                SignalPropertyChange();
            }
        }

        [TypeConverter(typeof(DropDownListNormals))]
        public string NormalNode
        {
            get { return _normalNode == null ? null : _normalNode._name; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    MDL0NormalNode node = Model.FindChild(String.Format("Normals/{0}", value), false) as MDL0NormalNode;
                    if (node != null && _normalNode != null && node.NumEntries >= _normalNode.NumEntries)
                    {
                        if (_normalNode != null && _normalNode._objects.Contains(this))
                            _normalNode._objects.Remove(this);

                        (_normalNode = node)._objects.Add(this);
                        _elementIndices[1] = (short)node.Index;
                    }
                }
                SignalPropertyChange();
            }
        }

        #endregion

        #region Colors

        internal MDL0ColorNode[] _colorSet = new MDL0ColorNode[2];
        private void SetColors(int id, string value)
        {
            if (String.IsNullOrEmpty(value))
                if (_colorSet[id] != null)
                {
                    if (_colorSet[id] != null && _colorSet[id]._objects.Contains(this))
                        _colorSet[id]._objects.Remove(this);

                    _colorChanged[id] = true;
                    _colorSet[id] = null;
                    _elementIndices[id + 2] = -1;
                    _rebuild = true;
                }
                else return;
            else
            {
                MDL0ColorNode node = Model.FindChild(String.Format("Colors/{0}", value), false) as MDL0ColorNode;
                if (node != null && node.NumEntries != 0)
                {
                    if (_colorSet[id] != null)
                    {
                        if (node.NumEntries == _colorSet[id].NumEntries)
                        {
                            if (_colorSet[id] != null && _colorSet[id]._objects.Contains(this))
                                _colorSet[id]._objects.Remove(this);

                            (_colorSet[id] = node)._objects.Add(this);
                            _elementIndices[id + 2] = (short)node.Index;
                        }
                        else if (node.NumEntries > 0)
                        {
                            if (MessageBox.Show(null, "All vertices will only use the first color entry.\nIs this okay?", "", MessageBoxButtons.YesNo) == DialogResult.No)
                                return;

                            if (_colorSet[id] != null && _colorSet[id]._objects.Contains(this))
                                _colorSet[id]._objects.Remove(this);

                            (_colorSet[id] = node)._objects.Add(this);
                            _elementIndices[id + 2] = (short)node.Index;
                            _rebuild = true;
                            _colorChanged[id] = true;
                        }
                    }
                    else
                    {
                        if (node.NumEntries > 1)
                            if (MessageBox.Show(null, "All vertices will only use the first color entry.\nIs this okay?", "", MessageBoxButtons.YesNo) == DialogResult.No)
                                return;

                        if (_colorSet[id] != null && _colorSet[id]._objects.Contains(this))
                            _colorSet[id]._objects.Remove(this);

                        (_colorSet[id] = node)._objects.Add(this);
                        _elementIndices[id + 2] = (short)node.Index;
                        _rebuild = true;
                        _colorChanged[id] = true;
                    }
                }
                else return;
            }
            SignalPropertyChange();
        }

        public bool[] _colorChanged = new bool[2] { false, false };
        [TypeConverter(typeof(DropDownListColors))]
        public string ColorNode0
        {
            get { return _colorSet[0] == null ? null : _colorSet[0]._name; }
            set { SetColors(0, value); }
        }
        [TypeConverter(typeof(DropDownListColors))]
        public string ColorNode1
        {
            get { return _colorSet[1] == null ? null : _colorSet[1]._name; }
            set { SetColors(1, value); }
        }

        #endregion

        #region UVs

        internal MDL0UVNode[] _uvSet = new MDL0UVNode[8];
        private void SetUVs(int id, string value)
        {
            if (String.IsNullOrEmpty(value))
                if (MessageBox.Show(RootNode._mainForm, "Do you want to remove this reference?", "Continue?", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    if (_uvSet[id] != null && _uvSet[id]._objects.Contains(this))
                        _uvSet[id]._objects.Remove(this);

                    _uvSet[id] = null;
                    _elementIndices[id + 4] = -1;
                    _rebuild = true;
                }
                else return;
            else
            {
                MDL0UVNode node = Model.FindChild(String.Format("UVs/{0}", value), false) as MDL0UVNode;
                if (node != null && _uvSet[id] != null)
                {
                    if (node.NumEntries != _uvSet[id].NumEntries && MessageBox.Show(RootNode._mainForm, "Entry counts are not equal.\nThis might cause problems.\nContinue anyway?", "Continue?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                        return;

                    if (_uvSet[id] != null && _uvSet[id]._objects.Contains(this))
                        _uvSet[id]._objects.Remove(this);

                    (_uvSet[id] = node)._objects.Add(this);
                    _elementIndices[id + 4] = (short)node.Index;
                }
                else return;
            }
            SignalPropertyChange();
        }

        [TypeConverter(typeof(DropDownListUVs))]
        public string TexCoord0
        {
            get { return _uvSet[0] == null ? null : _uvSet[0]._name; }
            set { SetUVs(0, value); }
        }
        [TypeConverter(typeof(DropDownListUVs))]
        public string TexCoord1
        {
            get { return _uvSet[1] == null ? null : _uvSet[1]._name; }
            set { SetUVs(1, value); }
        }
        [TypeConverter(typeof(DropDownListUVs))]
        public string TexCoord2
        {
            get { return _uvSet[2] == null ? null : _uvSet[2]._name; }
            set { SetUVs(2, value); }
        }
        [TypeConverter(typeof(DropDownListUVs))]
        public string TexCoord3
        {
            get { return _uvSet[3] == null ? null : _uvSet[3]._name; }
            set { SetUVs(3, value); }
        }
        [TypeConverter(typeof(DropDownListUVs))]
        public string TexCoord4
        {
            get { return _uvSet[4] == null ? null : _uvSet[4]._name; }
            set { SetUVs(4, value); }
        }
        [TypeConverter(typeof(DropDownListUVs))]
        public string TexCoord5
        {
            get { return _uvSet[5] == null ? null : _uvSet[5]._name; }
            set { SetUVs(5, value); }
        }
        [TypeConverter(typeof(DropDownListUVs))]
        public string TexCoord6
        {
            get { return _uvSet[6] == null ? null : _uvSet[6]._name; }
            set { SetUVs(6, value); }
        }
        [TypeConverter(typeof(DropDownListUVs))]
        public string TexCoord7
        {
            get { return _uvSet[7] == null ? null : _uvSet[7]._name; }
            set { SetUVs(7, value); }
        }

        #endregion

        #region Fur

        internal MDL0FurPosNode _furPosNode;
        internal MDL0FurVecNode _furVecNode;

        [TypeConverter(typeof(DropDownListFurPos))]
        public string FurLayerCoordNode
        {
            get { return _furPosNode == null ? null : _furPosNode._name; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    MDL0FurPosNode node = Model.FindChild(String.Format("FurLayerCoords/{0}", value), false) as MDL0FurPosNode;
                    if (node != null && _furPosNode != null && node.NumVertices >= _furPosNode.NumVertices)
                    {
                        if (_furPosNode != null && _furPosNode._objects.Contains(this))
                            _furPosNode._objects.Remove(this);

                        (_furPosNode = node)._objects.Add(this);
                        _elementIndices[12] = (short)node.Index;
                    }
                }
                SignalPropertyChange();
            }
        }

        [TypeConverter(typeof(DropDownListFurVec))]
        public string FurVectorNode
        {
            get { return _furVecNode == null ? null : _furVecNode._name; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    MDL0FurVecNode node = Model.FindChild(String.Format("FurVectors/{0}", value), false) as MDL0FurVecNode;
                    if (node != null && _furVecNode != null && node.NumEntries >= _furVecNode.NumEntries)
                    {
                        if (_furVecNode != null && _furVecNode._objects.Contains(this))
                            _furVecNode._objects.Remove(this);

                        (_furVecNode = node)._objects.Add(this);
                        _elementIndices[13] = (short)node.Index;
                    }
                }
                SignalPropertyChange();
            }
        }

        #endregion

        #endregion

        #region Variables

        int _totalLength, _mdl0Offset, _stringOffset;

        public int _numFacepoints;
        public int _numFaces;
        public int _nodeId;
        public int _defBufferSize = 0xE0;
        public int _defSize = 0x80;
        public int _defOffset;
        public int _primBufferSize;
        public int _primSize;
        public int _primOffset;
        public int _flag = 0;

        internal short[] _elementIndices = new short[14];

        public int[] _nodeCache;
        private int _tableLen = 0;
        private int _primitiveStart = 0;
        private int _primitiveSize = 0;
        public FacepointAttribute[] _descList;
        public VertexAttributeFormat[] _fmtList;
        public int _fpStride = 0;
        public List<PrimitiveGroup> _primGroups = new List<PrimitiveGroup>();

        public bool _rebuild = false, _reOptimized = false;

        #endregion

        #region Single Bind linkage
        [Browsable(true), TypeConverter(typeof(DropDownListBones))]
        public string SingleBind
        {
            get { return _matrixNode == null ? "(none)" : _matrixNode.IsPrimaryNode ? ((MDL0BoneNode)_matrixNode)._name : "(multiple)"; }
            set
            {
                MatrixNode = String.IsNullOrEmpty(value) ? null : Model.FindBone(value); 
                Model.SignalPropertyChange();
            }
        }
        internal IMatrixNode _matrixNode;
        [Browsable(false)]
        public IMatrixNode MatrixNode
        {
            get { return _matrixNode; }
            set
            {
                if (_matrixNode == value)
                    return;

                if (value is MDL0BoneNode && _matrixNode is Influence)
                {
                    foreach (Vertex3 v in _manager._vertices)
                    {
                        v._position *= ((MDL0BoneNode)value).InverseMatrix;
                        //v._normal *= ((MDL0BoneNode)value).InverseMatrix.GetRotationMatrix();
                    }
                    SetEditedVertices();
                    //SetEditedNormals();
                }
                else if (value is Influence && _matrixNode is MDL0BoneNode)
                {
                    foreach (Vertex3 v in _manager._vertices)
                    {
                        v._position *= ((MDL0BoneNode)_matrixNode).Matrix;
                        //v._normal *= ((MDL0BoneNode)_matrixNode).Matrix.GetRotationMatrix();
                    }
                    SetEditedVertices();
                    //SetEditedNormals();
                }

                if (_matrixNode != null)
                {
                    if (_matrixNode is MDL0BoneNode)
                        ((MDL0BoneNode)_matrixNode)._infPolys.Remove(this);
                    else
                    {
                        _matrixNode.ReferenceCount--;
                        _matrixNode.Users.Remove(this);
                    }
                }
                if ((_matrixNode = value) != null)
                {
                    //Singlebind bones aren't added to NodeMix, but its node id is still built as influenced
                    //_singleBind.ReferenceCount++;
                    if (_matrixNode is MDL0BoneNode)
                        ((MDL0BoneNode)_matrixNode)._infPolys.Add(this);
                    else
                    {
                        _matrixNode.ReferenceCount++;
                        _matrixNode.Users.Add(this);
                    }
                }
            }
        }
        #endregion

        #region Material linkage
        public void EvalMaterials(ref string message)
        {
            if (XluMaterialNode != null && !XluMaterialNode.XLUMaterial)
                if (OpaMaterialNode != null)
                    message += Name + "\n";
            if (OpaMaterialNode != null && OpaMaterialNode.XLUMaterial)
                if (XluMaterialNode != null)
                    message += Name + "\n";
        }
        public void FixMaterials(ref string message)
        {
            if (XluMaterialNode != null && !XluMaterialNode.XLUMaterial)
            {
                if (OpaMaterialNode == null)
                    OpaMaterialNode = XluMaterialNode;
                else
                    message += Name + "\n";
                XluMaterialNode = null;
            }
            if (OpaMaterialNode != null && OpaMaterialNode.XLUMaterial)
            {
                if (XluMaterialNode == null)
                    XluMaterialNode = OpaMaterialNode;
                else
                    message += Name + "\n";
                OpaMaterialNode = null;
            }
        }
        [Browsable(false)]
        public MDL0MaterialNode UsableMaterialNode
        {
            get
            {
                if (OpaMaterialNode != null)
                    return OpaMaterialNode;
                else
                    return XluMaterialNode;
            }
            set
            {
                if (value.XLUMaterial)
                    XluMaterialNode = value;
                else 
                    OpaMaterialNode = value;
            }
        }

        internal MDL0MaterialNode _opaMaterial, _xluMaterial;
        [Browsable(false)]
        public MDL0MaterialNode OpaMaterialNode
        {
            get { return _opaMaterial; }
            set
            {
                if (_opaMaterial == value)
                    return;
                if (_opaMaterial != null)
                    _opaMaterial._objects.Remove(this);
                if ((_opaMaterial = value) != null)
                    _opaMaterial._objects.Add(this);
            }
        }
        [Browsable(false)]
        public MDL0MaterialNode XluMaterialNode
        {
            get { return _xluMaterial; }
            set
            {
                if (_xluMaterial == value)
                    return;
                if (_xluMaterial != null)
                    _xluMaterial._objects.Remove(this);
                if ((_xluMaterial = value) != null)
                    _xluMaterial._objects.Add(this);
            }
        }
        [Browsable(true), TypeConverter(typeof(DropDownListOpaMaterials))]
        public string OpaMaterial
        {
            get { return _opaMaterial == null ? null : _opaMaterial._name; }
            set { if (String.IsNullOrEmpty(value)) OpaMaterialNode = null; else { OpaMaterialNode = Model.FindOrCreateOpaMaterial(value); Model.SignalPropertyChange(); } }
        }
        [Browsable(true), TypeConverter(typeof(DropDownListXluMaterials))]
        public string XluMaterial
        {
            get { return _xluMaterial == null ? null : _xluMaterial._name; }
            set { if (String.IsNullOrEmpty(value)) XluMaterialNode = null; else { XluMaterialNode = Model.FindOrCreateXluMaterial(value); Model.SignalPropertyChange(); } }
        }
        #endregion

        #region Bone linkage
        public MDL0BoneNode _bone;
        [Browsable(false)]
        public MDL0BoneNode BoneNode
        {
            get { return _bone; }
            set
            {
                if (_bone == value)
                    return;
                if (_bone != null)
                    _bone._manPolys.Remove(this);
                if ((_bone = value) != null)
                {
                    _bone._manPolys.Add(this);
                    _render = _bone._flags1.HasFlag(BoneFlags.Visible);
                }
            }
        }
        [Browsable(true), TypeConverter(typeof(DropDownListBones))]
        public string VisibilityBone //This attaches the object to a bone controlled by a VIS0
        {
            get { return _bone == null ? null : _bone._name; }
            set { BoneNode = String.IsNullOrEmpty(value) ? null : Model.FindBone(value); Model.SignalPropertyChange(); }
        }
        #endregion

        #region Reading & Writing

        public PrimitiveManager _manager;

        public override void Dispose()
        {
            if (_manager != null)
            {
                _manager.Dispose();
                _manager = null;
            }
            base.Dispose();
        }

        public override bool OnInitialize()
        {
            MDL0Object* header = Header;
            _nodeId = header->_nodeId;

            SetSizeInternal(_totalLength = header->_totalLength);
            _mdl0Offset = header->_mdl0Offset;
            _stringOffset = header->_stringOffset;

            ModelLinker linker = Model._linker;

            MatrixNode = (_nodeId >= 0 && _nodeId < Model._linker.NodeCache.Length) ? Model._linker.NodeCache[_nodeId] : null;

            _vertexFormat = header->_vertexFormat;
            _vertexSpecs = header->_vertexSpecs;
            _arrayFlags = header->_arrayFlags;

            HasPosMatrix = _arrayFlags.HasPosMatrix;
            for (int i = 0; i < 8; i++)
                HasTextureMatrix[i] = _arrayFlags.GetHasTexMatrix(i);

            _numFacepoints = header->_numVertices;
            _numFaces = header->_numFaces;

            _flag = header->_flag;

            _primBufferSize = header->_primitives._bufferSize;
            _primSize = header->_primitives._size;
            _primOffset = header->_primitives._offset;

            _defBufferSize = header->_defintions._bufferSize;
            _defSize = header->_defintions._size;
            _defOffset = header->_defintions._offset;

            _entryIndex = header->_index;

            //Conditional name assignment
            if ((_name == null) && (header->_stringOffset != 0))
                if (!_replaced)
                    _name = header->ResourceString;
                else
                    _name = "polygon" + Index;

            //Link nodes
            if (header->_vertexId >= 0 && Model._vertList != null)
                foreach (MDL0VertexNode v in Model._vertList)
                    if (header->_vertexId == v.ID)
                    {
                        (_vertexNode = v)._objects.Add(this);
                        break;
                    }

            if (header->_normalId >= 0 && Model._normList != null)
                foreach (MDL0NormalNode n in Model._normList)
                    if (header->_normalId == n.ID)
                    {
                        (_normalNode = n)._objects.Add(this);
                        break;
                    }

            int id;
            for (int i = 0; i < 2; i++)
                if ((id = ((bshort*)header->_colorIds)[i]) >= 0 && Model._colorList != null)
                    foreach (MDL0ColorNode c in Model._colorList)
                        if (id == c.ID)
                        {
                            (_colorSet[i] = c)._objects.Add(this);
                            break;
                        }

            for (int i = 0; i < 8; i++)
                if ((id = ((bshort*)header->_uids)[i]) >= 0 && Model._uvList != null)
                    foreach (MDL0UVNode u in Model._uvList)
                        if (id == u.ID)
                        {
                            (_uvSet[i] = u)._objects.Add(this);
                            break;
                        }

            if (Model._version > 9)
            {
                if (header->_furVectorId >= 0)
                    foreach (MDL0FurVecNode v in Model._furVecList)
                        if (header->_furVectorId == v.ID)
                        {
                            (_furVecNode = v)._objects.Add(this);
                            break;
                        }

                if (header->_furLayerCoordId >= 0)
                    foreach (MDL0FurPosNode n in Model._furPosList)
                        if (header->_furLayerCoordId == n.ID)
                        {
                            (_furPosNode = n)._objects.Add(this);
                            break;
                        }
            }

            //Link element indices for rebuild
            _elementIndices[0] = (short)(_vertexNode != null ? _vertexNode.Index : -1);
            _elementIndices[1] = (short)(_normalNode != null ? _normalNode.Index : -1);
            for (int i = 2; i < 4; i++)
                _elementIndices[i] = (short)(_colorSet[i - 2] != null ? _colorSet[i - 2].Index : -1);
            for (int i = 4; i < 12; i++)
                _elementIndices[i] = (short)(_uvSet[i - 4] != null ? _uvSet[i - 4].Index : -1);
            _elementIndices[12] = (short)(_furVecNode != null ? _furVecNode.Index : -1);
            _elementIndices[13] = (short)(_furPosNode != null ? _furPosNode.Index : -1);

            //Create primitive manager
            if (_parent != null)
            {
                int i = 0;
                _manager = new PrimitiveManager(header, Model._assets, linker.NodeCache, this);
                if (_manager._vertices != null)
                    foreach (Vertex3 v in _manager._vertices)
                    {
                        v._index = i++;
                        v._object = this;
                    }
            }

            //Get polygon UVAT groups
            MDL0PolygonDefs* Defs = (MDL0PolygonDefs*)header->DefList;
            UVATGroups = new CPElementSpec(
                (uint)Defs->UVATA,
                (uint)Defs->UVATB,
                (uint)Defs->UVATC);

            //Read internal object node cache and read influence list
            if (Model._linker.NodeCache != null)
            {
                if (_matrixNode == null)
                {
                    _influences = new List<IMatrixNode>();
                    bushort* weights = header->WeightIndices(Model._version);
                    int count = *(bint*)weights; weights += 2;
                    for (int i = 0; i < count; i++)
                        if (*weights < Model._linker.NodeCache.Length)
                            _influences.Add(Model._linker.NodeCache[*weights++]);
                        else
                            weights++;
                }
            }

            //Check for errors
            if (header->_totalLength % 0x20 != 0)
            {
                Model._errors.Add("Object " + Index + " has an improper data length.");
                SignalPropertyChange(); _rebuild = true;
            }
            if ((int)(0x24 + header->_primitives._offset) % 0x20 != 0)
            {
                Model._errors.Add("Object " + Index + " has an improper primitives start offset.");
                SignalPropertyChange(); _rebuild = true;
            }
            if (CheckVertexFormat())
            {
                Model._errors.Add("Object " + Index + " has a facepoint descriptor that does not match its linked nodes.");
                SignalPropertyChange(); _rebuild = true;

                for (int i = 0; i < 2; i++)
                    if (_colorSet[i] != null && _manager._faceData[i + 2] == null)
                    {
                        _manager._faceData[i + 2] = new UnsafeBuffer(_manager._pointCount * 4);
                        _colorChanged[i] = true;
                    }
            }
            if (HasTexMtx && !Weighted)
            {
                Model._errors.Add("Object " + Index + " has texture matrices but is not weighted.");
                for (int i = 0; i < 8; i++)
                    HasTextureMatrix[i] = false;
                SignalPropertyChange(); 
                _rebuild = true;
            }

            //if (!Weighted)
            //{
            //    bool notFloat = HasANonFloatAsset;
            //    foreach (PrimitiveGroup p in _primGroups)
            //    {
            //        bool o = false;
            //        foreach (PrimitiveHeader ph in p._headers)
            //            if (ph.Type != WiiPrimitiveType.TriangleList && notFloat)
            //            {
            //                Model._errors.Add("Object " + Index + " will explode in-game due to assets that are not written as float.");
            //                SignalPropertyChange();

            //                if (_vertexNode.Format != WiiVertexComponentType.Float)
            //                    _vertexNode._forceRebuild = _vertexNode._forceFloat = true;

            //                if (_normalNode != null && _normalNode.Format != WiiVertexComponentType.Float)
            //                    _normalNode._forceRebuild = _normalNode._forceFloat = true;

            //                for (int i = 4; i < 12; i++)
            //                    if (_uvSet[i - 4] != null && _uvSet[i - 4].Format != WiiVertexComponentType.Float)
            //                        _uvSet[i - 4]._forceRebuild = _uvSet[i - 4]._forceFloat = true;

            //                o = true;
            //                break;
            //            }
            //        if (o)
            //            break;
            //    }
            //}

            return false;
        }

        #region Rebuilding

        [Browsable(false)]
        public bool HasNonFloatVertices { get { return _vertexNode !=  null && _vertexNode.Format != WiiVertexComponentType.Float; } }

        public void RecalcIndices()
        {
            _elementIndices[0] = (short)(_vertexNode != null ? _vertexNode.Index : _elementIndices[0]);
            _elementIndices[1] = (short)(_normalNode != null ? _normalNode.Index : _elementIndices[1]);
            for (int i = 2; i < 4; i++)
                _elementIndices[i] = (short)(_colorSet[i - 2] != null ? _colorSet[i - 2].Index : _elementIndices[i]);
            for (int i = 4; i < 12; i++)
                _elementIndices[i] = (short)(_uvSet[i - 4] != null ? _uvSet[i - 4].Index : _elementIndices[i]);
            _elementIndices[12] = (short)(_furVecNode != null ? _furVecNode.Index : _elementIndices[0]);
            _elementIndices[13] = (short)(_furPosNode != null ? _furPosNode.Index : _elementIndices[1]);
        }

        /// <summary>
        /// Returns true if the facepoint descriptor does not match the linked nodes, meaning this object must be rebuilt.
        /// </summary>
        public bool CheckVertexFormat()
        {
            bool b1 = _vertexFormat.PosFormat != XFDataFormat.None;
            bool b2 = _elementIndices[0] != -1;
            if (b1 != b2)
                return true;

            b1 = _vertexFormat.NormalFormat != XFDataFormat.None;
            b2 = _elementIndices[1] != -1;
            if (b1 != b2)
                return true;
            
            for (int i = 0; i < 2; i++)
            {
                b1 = _vertexFormat.GetColorFormat(i) != XFDataFormat.None;
                b2 = _elementIndices[i + 2] != -1;
                if (b1 != b2)
                    return true;
            }

            for (int i = 0; i < 8; i++)
            {
                b1 = _vertexFormat.GetUVFormat(i) != XFDataFormat.None;
                b2 = _elementIndices[i + 4] != -1;
                if (b1 != b2)
                    return true;
            }

            return false;
        }

        public void GenerateNodeCache()
        {
            if (MatrixNode != null)
            {
                _nodeCache = new int[0];
                return;
            }

            //Create node table
            HashSet<int> nodes = new HashSet<int>();
            foreach (Vertex3 v in _manager._vertices)
                if (v._matrixNode != null)
                    nodes.Add(v._matrixNode.NodeIndex);

            //Copy to array and sort
            _nodeCache = new int[nodes.Count];
            nodes.CopyTo(_nodeCache);
            Array.Sort(_nodeCache);
        }

        public TriangleConverter _triConverter = new TriangleConverter(true, 30, 2, false, true);

        //This should be done after node indices have been assigned
        public override int OnCalculateSize(bool force)
        {
            //Reset everything!
            _tableLen =
            _primitiveStart =
            _primitiveSize =
            _fpStride = 0;

            MDL0Node model = Model;
            if (model._isImport)
            {
                //Continue checking for single bind
                if (_nodeId == -2 && _matrixNode == null)
                {
                    bool first = true;
                    foreach (Vertex3 v in _manager._vertices)
                    {
                        if (first)
                        {
                            if (v._matrixNode != null)
                                MatrixNode = model._linker.NodeCache[v._matrixNode.NodeIndex];

                            first = false;
                        }
                        v.MatrixNode = null;
                    }
                }
            }

            //Collect vertex node ids
            GenerateNodeCache();
            
            //Recollect indices of linked nodes
            RecalcIndices();

            //Check that the facepoint descriptor matches the linked nodes
            if (!_rebuild)
                _rebuild = CheckVertexFormat();

            //Rebuild only under certain circumstances
            if (model._rebuildAllObj || model._isImport || _rebuild || _reOptimized)
            {
                int size = (int)MDL0Object.Size;

                if (model._version >= 10)
                    size += 4; //Add extra -1 value

                //Set vertex descriptor
                _descList = _manager.SetVertexDescList(this, model._linker._forceDirectAssets);

                //Add table length
                size += _nodeCache.Length * 2 + 4;
                _tableLen = ((size.Align(0x10) + 0xE0) % 0x20 == 0) ? size.Align(0x10) : size.Align(0x20);

                //Add def length
                size = _primitiveStart = _tableLen + 0xE0;

                //Need to group facepoint data if creating a new model
                if (model._isImport)
                {
                    _triConverter._useStrips = Collada._importOptions._useTristrips;
                    _triConverter._cacheSize = Collada._importOptions._cacheSize;
                    _triConverter._minStripLen = Collada._importOptions._minStripLen;
                    _triConverter._pushCacheHits = Collada._importOptions._pushCacheHits;
                    _triConverter._backwardSearch = Collada._importOptions._backwardSearch;
                    
                    _primGroups.Clear();

                    //Merge vertices and assets into facepoints
                    Facepoint[] facepoints = _manager.MergeInternalFaceData(this);
                    if (_manager._triangles != null)
                    {
                        Facepoint[] points = new Facepoint[_manager._triangles._elementCount];
                        uint[] indices = _manager._triangles._indices;
                        bool ccw = Collada._importOptions._forceCCW;

                        //Indices are written in reverse for each triangle, 
                        //so they need to be set to a triangle in reverse if not CCW
                        for (int t = 0; t < _manager._triangles._elementCount; t++)
                            points[ccw ? t : (t - (t % 3)) + (2 - (t % 3))] = facepoints[indices[t]];

                        _primGroups = _triConverter.GroupPrimitives(points, out _numFacepoints, out _numFaces);
                    }
                }

                //Build display list
                foreach (PrimitiveGroup g in _primGroups)
                {
                    if (model._isImport || _reOptimized)
                    {
                        if (g._tristrips.Count != 0)
                            foreach (PointTriangleStrip strip in g._tristrips)
                                _primitiveSize += 3 + strip._points.Count * _fpStride;

                        if (g._triangles.Count != 0)
                            _primitiveSize += 3 + g._triangles.Count * 3 * _fpStride;
                    }
                    else
                        for (int i = 0; i < g._headers.Count; i++)
                            _primitiveSize += 3 + g._points[i].Count * _fpStride;

                    if (Weighted)
                        _primitiveSize += 5 * g._nodes.Count * (HasTexMtx ? 3 : 2); //Add total matrices size
                }

                size += _primitiveSize;
                int align = ((size.Align(0x10)) % 0x20 == 0) ? 0x10 : 0x20;
                size = size.Align(align);
                _primitiveSize = _primitiveSize.Align(align);
                
                //Texture matrices (0x30) start at 0x00, max 11
                //Pos matrices (0x20) start at 0x78, max 10
                //Normal matrices (0x28) start at 0x400, max 10

                return size;
            }
            else
                return base.OnCalculateSize(force);
        }
        
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            MDL0Object* header = (MDL0Object*)address;

            MDL0Node model = Model;
            
            if (_uvSet[7] != null)
                _defSize = 224;
            else if (_uvSet[5] != null)
                _defSize = 192;
            else if (_uvSet[2] != null)
                _defSize = 160;
            else
                _defSize = 128;

            if (model._rebuildAllObj || model._isImport || _rebuild || _reOptimized)
            {
                //Set Header
                header->_totalLength = length;

                header->_numVertices = _numFacepoints;
                header->_numFaces = _numFaces;

                _primBufferSize = header->_primitives._bufferSize = _primitiveSize;
                _primSize = header->_primitives._size = _primitiveSize;
                _primOffset = header->_primitives._offset = _tableLen + 0xBC;

                _defOffset = _tableLen - 0x18;

                header->_defintions._bufferSize = _defBufferSize;
                header->_defintions._size = _defSize;
                header->_defintions._offset = _defOffset;

                header->_flag = _flag;
                header->_index = _entryIndex;

                if (model._version < 10)
                    header->_nodeTableOffset = 0x64;
                else
                {
                    *(bshort*)((byte*)header + 0x60) = _elementIndices[12];
                    *(bshort*)((byte*)header + 0x62) = _elementIndices[13];

                    //Table offset
                    *(byte*)((byte*)header + 0x67) = 0x68;
                }

                //Set the node id
                if (_matrixNode != null)
                    header->_nodeId = _nodeId = (ushort)_matrixNode.NodeIndex;
                else
                    header->_nodeId = _nodeId = -1;

                //Set asset ids
                header->_vertexId = model._isImport && model._linker._forceDirectAssets[0] ? (short)-1 : (short)(_elementIndices[0] >= 0 ? _elementIndices[0] : -1);
                header->_normalId = model._isImport && model._linker._forceDirectAssets[1] ? (short)-1 : (short)(_elementIndices[1] >= 0 ? _elementIndices[1] : -1);
                for (int i = 2; i < 4; i++)
                    *(bshort*)&header->_colorIds[i - 2] = model._isImport && model._linker._forceDirectAssets[i] ? (short)-1 : (short)(_elementIndices[i] >= 0 ? _elementIndices[i] : -1);
                for (int i = 4; i < 12; i++)
                    *(bshort*)&header->_uids[i - 4] = model._isImport && model._linker._forceDirectAssets[i] ? (short)-1 : (short)(_elementIndices[i] >= 0 ? _elementIndices[i] : -1);

                //Write def list
                MDL0PolygonDefs* Defs = (MDL0PolygonDefs*)header->DefList;
                *Defs = MDL0PolygonDefs.Default;

                //Array flags are already set
                header->_arrayFlags = _arrayFlags;

                //Set vertex flags using descriptor list (sets the flags to this object)
                _manager.WriteVertexDescriptor(_descList, this);

                //Set UVAT groups using format list (writes directly to header)
                _manager.WriteVertexFormat(_fmtList, header);

                //Write newly set flags
                header->_vertexFormat._lo = Defs->VtxFmtLo = _vertexFormat._lo;
                header->_vertexFormat._hi = Defs->VtxFmtHi = _vertexFormat._hi;
                header->_vertexSpecs = Defs->VtxSpecs = _vertexSpecs;

                //Display UVAT groups that were written
                UVATGroups = new CPElementSpec(
                    (uint)Defs->UVATA,
                    (uint)Defs->UVATB,
                    (uint)Defs->UVATC);

                //Write weight table only if the object is weighted
                if (_matrixNode == null)
                    WriteWeightTable(header->WeightIndices(Model._version));

                //Write primitives
                _manager.WritePrimitives(this, header);
            }
            else
            {
                //Move raw data over
                base.OnRebuild(address, length, force);

                CorrectNodeIds(header);
                
                header->_vertexId = _elementIndices[0];
                header->_normalId = _elementIndices[1];
                for (int i = 2; i < 4; i++)
                    *(bshort*)&header->_colorIds[i - 2] = (short)(_elementIndices[i] >= 0 ? _elementIndices[i] : -1);
                for (int i = 4; i < 12; i++)
                    *(bshort*)&header->_uids[i - 4] = (short)(_elementIndices[i] >= 0 ? _elementIndices[i] : -1);
                if (model._version >= 10)
                {
                    *(bshort*)((byte*)header + 0x60) = _elementIndices[12];
                    *(bshort*)((byte*)header + 0x62) = _elementIndices[13];
                }
                header->_defintions._size = _defSize;
            }
            _rebuild = _reOptimized = false;
        }

        private void WriteWeightTable(VoidPtr addr)
        {
            if (_nodeCache == null)
                GenerateNodeCache();

            bushort* ptr = (bushort*)addr;
            *(buint*)ptr = (uint)_nodeCache.Length; ptr += 2;
            foreach (int n in _nodeCache)
                *ptr++ = (ushort)n;
        }

        private void CorrectNodeIds(MDL0Object* header)
        {
            WriteWeightTable(header->WeightIndices(Model._version));

            if (_matrixNode != null)
                header->_nodeId = _nodeId = (ushort)_matrixNode.NodeIndex;
            else
            {
                header->_nodeId = _nodeId = -1;

                VoidPtr primAddr = header->PrimitiveData;
                foreach (PrimitiveGroup p in _primGroups)
                    p.SetNodeIds(primAddr);
            }
        }

        public override unsafe void Export(string outPath)
        {
            if (outPath.EndsWith(".obj"))
                Wavefront.Serialize(outPath, this);
            else
                base.Export(outPath);
        }

        protected internal override void PostProcess(VoidPtr mdlAddress, VoidPtr dataAddress, StringTable stringTable)
        {
            MDL0Object* header = (MDL0Object*)dataAddress;
            header->_mdl0Offset = (int)mdlAddress - (int)dataAddress;
            header->_stringOffset = (int)stringTable[Name] + 4 - (int)dataAddress;
            header->_index = Index;
        }

        #endregion
        
        #endregion

        #region Rendering

        public void GetBox(out Vector3 min, out Vector3 max)
        {
            min = new Vector3(float.MaxValue);
            max = new Vector3(float.MinValue);

            if (_manager != null && _manager._vertices != null)
                foreach (Vertex3 vertex in _manager._vertices)
                {
                    Vector3 v = vertex.WeightedPosition;

                    min.Min(v);
                    max.Max(v);
                }
        }

        //public void UpdateProgram(ModelPanel mainWindow)
        //{
        //    bool temp = false;
        //    bool force = true;
        //    bool updateProgram = force || _renderUpdate || UsableMaterialNode._renderUpdate || UsableMaterialNode.ShaderNode._renderUpdate;
        //    if (updateProgram)
        //    {
        //        temp = true;

        //        if (_programHandle > 0)
        //            GL.DeleteProgram(_programHandle);

        //        _programHandle = GL.CreateProgram();

        //        int status;
        //        string info;

        //        if (_renderUpdate)
        //        {
        //            vertexShaderSource = ShaderGenerator.GenerateVertexShader(this);

        //            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
        //            GL.CompileShader(vertexShaderHandle);

        //            GL.GetShaderInfoLog(vertexShaderHandle, out info);
        //            GL.GetShader(vertexShaderHandle, OpenTK.Graphics.OpenGL.ShaderParameter.CompileStatus, out status);

        //            if (status != 1)
        //                Console.WriteLine(info + "\n\n" + vertexShaderSource + "\n\n");
        //            else
        //                GL.AttachShader(_programHandle, vertexShaderHandle);
        //        }
        //        if (_renderUpdate || UsableMaterialNode._renderUpdate || UsableMaterialNode.ShaderNode._renderUpdate)
        //        {
        //            _fragShaderSource = ShaderGenerator.GeneratePixelShader(this);

        //            GL.ShaderSource(_fragShaderHandle, _fragShaderSource);
        //            GL.CompileShader(_fragShaderHandle);

        //            GL.GetShaderInfoLog(_fragShaderHandle, out info);
        //            GL.GetShader(_fragShaderHandle, OpenTK.Graphics.OpenGL.ShaderParameter.CompileStatus, out status);

        //            if (status != 1)
        //                Console.WriteLine(info + "\n\n" + _fragShaderSource + "\n\n");
        //            else
        //                GL.AttachShader(_programHandle, _fragShaderHandle);

        //            UsableMaterialNode._renderUpdate = UsableMaterialNode.ShaderNode._renderUpdate = false;
        //        }

        //        _renderUpdate = false;

        //        GL.LinkProgram(_programHandle);
        //    }

        //    GL.UseProgram(_programHandle);

        //    if (temp)
        //    {
        //        SetUniforms(_programHandle, mainWindow);
        //        //UsableMaterialNode.SetUniforms(_programHandle);
        //    }
        //    //if (UsableMaterialNode._lightSet != null)
        //    //    SetLightUniforms(_programHandle);
        //}

        public bool _render = true;
        internal void Render(TKContext ctx, bool wireframe, ModelPanel mainWindow)
        {
            if (!_render || _manager == null)
                return;

            bool useShaders = ctx._shadersEnabled;
            MDL0MaterialNode material = UsableMaterialNode;

            if (wireframe)
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
                _manager.PrepareStream();
                _manager.ApplyTexture(null);
                GL.Color4(Color.Black);
                _manager.RenderMesh();
                _manager.DetachStreams();
                return;
            }

            //if (useShaders)
            //{
            //    UpdateProgram(mainWindow);

            //    _manager.PrepareStreamNew(_programHandle);

            //    if (material != null)
            //    {
            //        switch ((int)material.CullMode)
            //        {
            //            case 0: //None
            //                GL.Disable(EnableCap.CullFace);
            //                break;
            //            case 1: //Outside
            //                GL.Enable(EnableCap.CullFace);
            //                GL.CullFace(CullFaceMode.Front);
            //                break;
            //            case 2: //Inside
            //                GL.Enable(EnableCap.CullFace);
            //                GL.CullFace(CullFaceMode.Back);
            //                break;
            //            case 3: //Double
            //                GL.Enable(EnableCap.CullFace);
            //                GL.CullFace(CullFaceMode.FrontAndBack);
            //                break;
            //        }

            //        if (material.Children.Count == 0)
            //            _manager.RenderMesh();
            //        else
            //        {
            //            foreach (MDL0MaterialRefNode mr in material.Children)
            //            {
            //                if (mr._texture != null && (!mr._texture.Enabled || mr._texture.Rendered))
            //                    continue;

            //                mr.Bind(ctx, _programHandle);
            //            }
            //            GL.BindVertexArray(_manager._arrayHandle);
            //            _manager.RenderMesh();
            //        }
            //    }
            //    else
            //        _manager.RenderMesh();

            //    _manager.DetachStreamsNew();
            //}
            //else
            {
                GL.Enable(EnableCap.Texture2D);
                _manager.PrepareStream();

                GL.MatrixMode(MatrixMode.Modelview);
                if (material != null)
                {
                    switch ((int)material.CullMode)
                    {
                        case 0: //None
                            GL.Disable(EnableCap.CullFace);
                            break;
                        case 1: //Outside
                            GL.Enable(EnableCap.CullFace);
                            GL.CullFace(CullFaceMode.Front);
                            break;
                        case 2: //Inside
                            GL.Enable(EnableCap.CullFace);
                            GL.CullFace(CullFaceMode.Back);
                            break;
                        case 3: //Double
                            GL.Enable(EnableCap.CullFace);
                            GL.CullFace(CullFaceMode.FrontAndBack);
                            break;
                    }

                    //if (material.EnableBlend)
                    //{
                    //    GL.Enable(EnableCap.Blend);
                    //}
                    //else
                    //    GL.Disable(EnableCap.Blend);

                    if (material.Children.Count == 0)
                    {
                        _manager.ApplyTexture(null);
                        _manager.RenderMesh();
                    }
                    else
                    {
                        foreach (MDL0MaterialRefNode mr in material.Children)
                        {
                            if (mr._texture != null && (!mr._texture.Enabled || mr._texture.Rendered))
                                continue;

                            GL.MatrixMode(MatrixMode.Texture);

                            GL.PushMatrix();

                            //Add bind transform
                            GL.Scale(mr.Scale._x, mr.Scale._y, 0);
                            GL.Rotate(mr.Rotation, 1, 0, 0);
                            GL.Translate(-mr.Translation._x, mr.Translation._y - ((mr.Scale._y - 1) / 2), 0);

                            //Now add frame transform
                            GL.Scale(mr._frameState._scale._x, mr._frameState._scale._y, 1);
                            GL.Rotate(mr._frameState._rotate._x, 1, 0, 0);
                            GL.Translate(-mr._frameState._translate._x, mr._frameState._translate._y - ((mr._frameState._scale._y - 1) / 2), 0);

                            GL.MatrixMode(MatrixMode.Modelview);

                            mr.Bind(ctx, -1);

                            _manager.ApplyTexture(mr);
                            _manager.RenderMesh();

                            GL.MatrixMode(MatrixMode.Texture);
                            GL.PopMatrix();
                            GL.MatrixMode(MatrixMode.Modelview);
                        }
                    }
                }
                else
                {
                    _manager.ApplyTexture(null);
                    _manager.RenderMesh();
                }

                _manager.DetachStreams();
            }
        }

        public void DrawBox()
        {
            Vector3 min, max;
            GetBox(out min, out max);

            GL.Begin(BeginMode.LineStrip);
            GL.Vertex3(max._x, max._y, max._z);
            GL.Vertex3(max._x, max._y, min._z);
            GL.Vertex3(min._x, max._y, min._z);
            GL.Vertex3(min._x, min._y, min._z);
            GL.Vertex3(min._x, min._y, max._z);
            GL.Vertex3(max._x, min._y, max._z);
            GL.Vertex3(max._x, max._y, max._z);
            GL.End();
            GL.Begin(BeginMode.Lines);
            GL.Vertex3(min._x, max._y, max._z);
            GL.Vertex3(max._x, max._y, max._z);
            GL.Vertex3(min._x, max._y, max._z);
            GL.Vertex3(min._x, min._y, max._z);
            GL.Vertex3(min._x, max._y, max._z);
            GL.Vertex3(min._x, max._y, min._z);
            GL.Vertex3(max._x, min._y, min._z);
            GL.Vertex3(min._x, min._y, min._z);
            GL.Vertex3(max._x, min._y, min._z);
            GL.Vertex3(max._x, max._y, min._z);
            GL.Vertex3(max._x, min._y, min._z);
            GL.Vertex3(max._x, min._y, max._z);
            GL.End();
        }

        public bool _renderUpdate = false;

        public string vertexShaderSource;
        public int vertexShaderHandle;

        internal void WeightVertices() 
        {
            if (_manager != null) 
                _manager.Weight();
        }

        internal override void Bind(TKContext ctx) 
        {
            _render = (_bone != null ? _bone._flags1.HasFlag(BoneFlags.Visible) ? true : false : true);

            //if (ctx != null && ctx._shadersEnabled)
            //{
            //    vertexShaderHandle = GL.CreateShader(OpenTK.Graphics.OpenGL.ShaderType.VertexShader);
            //    _fragShaderHandle = GL.CreateShader(OpenTK.Graphics.OpenGL.ShaderType.FragmentShader);

            //    _renderUpdate = true;

            //    if (_manager != null)
            //        _manager.Bind();
            //}
        }
        internal override void Unbind() 
        {
            _render = false;

            //if (vertexShaderHandle != 0)
            //    GL.DeleteShader(vertexShaderHandle);

            //if (_fragShaderHandle != 0)
            //    GL.DeleteShader(_fragShaderHandle);

            //if (_programHandle != 0)
            //    GL.DeleteProgram(_programHandle);

            if (_manager != null)
                _manager.Unbind();
        }

        public void Attach(TKContext ctx) { Model.Attach(ctx); _render = true; }
        public void Detach() { if (Model == null) return; Model.Detach(); _render = false; }
        public void Refesh() { if (Model == null) return; Model.Refesh(); }

        public void Render(TKContext ctx, ModelPanel mainWindow)
        {
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.DepthTest);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            Render(ctx, false, mainWindow);
        }

        #endregion

        #region Etc
        public void ConvertInf()
        {
            if (_matrixNode == null)
            {
                IMatrixNode inf = null;
                bool ok = true;
                foreach (Vertex3 v in _manager._vertices)
                {
                    if (inf == null)
                        inf = v.MatrixNode;
                    else if (inf != v.MatrixNode)
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    MatrixNode = inf;
                    foreach (Vertex3 v in _manager._vertices)
                        v.MatrixNode = null;
                }
            }
            else
            {
                foreach (Vertex3 v in _manager._vertices)
                    v.MatrixNode = MatrixNode;
                MatrixNode = null;
            }
        }
        public void SetVerticesFromWeighted()
        {
            for (int i = 0; i < _manager._vertices.Count; i++)
            {
                Vertex3 vec = _manager._vertices[i];
                //if (vec._moved)
                    _vertexNode.Vertices[_manager._vertices[i]._facepoints[0]._vertexIndex] = vec.UnweightPos(vec._weightedPosition);
            }

            _vertexNode.ForceRebuild = true; 
            if (_vertexNode.Format == WiiVertexComponentType.Float)
                _vertexNode.ForceFloat = true;
        }
        //public void SetEditedNormals()
        //{
        //    for (int i = 0; i < _manager._vertices.Count; i++)
        //    {
        //        Vertex3 vec = _manager._vertices[i];
        //        if (vec._moved)
        //            _vertexNode.Vertices[_manager._vertices[i]._facepoints[0].NormalIndex] = vec._normal;
        //    }

        //    _normalNode.ForceRebuild = true;
        //    if (_normalNode.Format == WiiVertexComponentType.Float)
        //        _normalNode.ForceFloat = true;
        //}
        public void SetEditedVertices()
        {
            for (int i = 0; i < _manager._vertices.Count; i++)
            {
                Vertex3 vec = _manager._vertices[i];
                //if (vec._moved)
                    _vertexNode.Vertices[_manager._vertices[i]._facepoints[0]._vertexIndex] = vec._position;
            }
            
            _vertexNode.ForceRebuild = true;
            if (_vertexNode.Format == WiiVertexComponentType.Float)
                _vertexNode.ForceFloat = true;
        }

        public MDL0ObjectNode HardCopy() 
        {
            MDL0ObjectNode o = new MDL0ObjectNode() { _manager = _manager, Name = Name };
            o._vertexNode = _vertexNode;
            o._normalNode = _normalNode;
            for (int i = 0; i < 2; i++)
                o._colorSet[i] = _colorSet[i];
            for (int i = 0; i < 8; i++)
                o._uvSet[i] = _uvSet[i];
            //o.Nodes = Nodes;
            o._opaMaterial = _opaMaterial;
            o._xluMaterial = _xluMaterial;
            o._furVecNode = _furVecNode;
            o._furPosNode = _furPosNode;
            o._bone = _bone;
            o._primGroups = _primGroups;
            o._matrixNode = _matrixNode;
            o._elementIndices = _elementIndices;
            o._uncompSource = o._origSource = new DataSource(WorkingUncompressed.Address, WorkingUncompressed.Length, Wii.Compression.CompressionType.None);
            return o;
        }

        public MDL0ObjectNode SoftCopy()
        {
            return MemberwiseClone() as MDL0ObjectNode; 
        }

        public override void Remove()
        {
            MDL0Node node = Model;

            if (node == null)
            {
                base.Remove();
                return;
            }

            if (_vertexNode != null)
                if (_vertexNode._objects.Count == 1)
                    if (MessageBox.Show("Do you want to remove this object's vertex node?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        _vertexNode.Remove();
                    else _vertexNode._objects.Remove(this);
                else _vertexNode._objects.Remove(this);

            if (_normalNode != null)
                if (_normalNode._objects.Count == 1)
                    _normalNode.Remove();
                else _normalNode._objects.Remove(this);

            for (int i = 0; i < 2; i++)
                if (_colorSet[i] != null)
                    if (_colorSet[i]._objects.Count == 1)
                        _colorSet[i].Remove();
                    else _colorSet[i]._objects.Remove(this);

            for (int i = 0; i < 8; i++)
                if (_uvSet[i] != null)
                    if (_uvSet[i]._objects.Count == 1)
                        _uvSet[i].Remove();
                    else _uvSet[i]._objects.Remove(this);

            MatrixNode = null;
            BoneNode = null;
            OpaMaterialNode = null;
            XluMaterialNode = null;

            if (_manager != null)
                foreach (Vertex3 v in _manager._vertices)
                    if (v._matrixNode != null)
                    {
                        v._matrixNode.Users.Remove(v);
                        v._matrixNode.ReferenceCount--;
                    }

            base.Remove();

            Dispose();

            if (node._objList != null)
                foreach (MDL0ObjectNode p in node._objList)
                    p.RecalcIndices();
        }

        public static int DrawCompareOpa(ResourceNode n1, ResourceNode n2)
        {
            //First compare draw priorities
            if (((MDL0ObjectNode)n1).DrawPriority > ((MDL0ObjectNode)n2).DrawPriority)
                return 1;
            if (((MDL0ObjectNode)n1).DrawPriority < ((MDL0ObjectNode)n2).DrawPriority)
                return -1;

            //Make sure the node isn't null
            if (((MDL0ObjectNode)n1).OpaMaterialNode != null && ((MDL0ObjectNode)n2).OpaMaterialNode == null)
                return 1;
            if (((MDL0ObjectNode)n1).OpaMaterialNode == null && ((MDL0ObjectNode)n2).OpaMaterialNode != null)
                return -1;
            if (((MDL0ObjectNode)n1).OpaMaterialNode == null && ((MDL0ObjectNode)n2).OpaMaterialNode == null)
                return 0;

            //Now check material draw priority
            if (((MDL0ObjectNode)n1).OpaMaterialNode.Index > ((MDL0ObjectNode)n2).OpaMaterialNode.Index)
                return 1;
            if (((MDL0ObjectNode)n1).OpaMaterialNode.Index < ((MDL0ObjectNode)n2).OpaMaterialNode.Index)
                return -1;

            //Finally compare the object index
            if (((MDL0ObjectNode)n1).Index > ((MDL0ObjectNode)n2).Index)
                return 1;
            if (((MDL0ObjectNode)n1).Index < ((MDL0ObjectNode)n2).Index)
                return -1;

            //Should never return equal
            return 0;
        }
        public static int DrawCompareXlu(ResourceNode n1, ResourceNode n2)
        {
            //First compare draw priorities
            if (((MDL0ObjectNode)n1).DrawPriority > ((MDL0ObjectNode)n2).DrawPriority)
                return 1;
            if (((MDL0ObjectNode)n1).DrawPriority < ((MDL0ObjectNode)n2).DrawPriority)
                return -1;

            //Make sure the node isn't null
            if (((MDL0ObjectNode)n1).XluMaterialNode != null && ((MDL0ObjectNode)n2).XluMaterialNode == null)
                return 1;
            if (((MDL0ObjectNode)n1).XluMaterialNode == null && ((MDL0ObjectNode)n2).XluMaterialNode != null)
                return -1;
            if (((MDL0ObjectNode)n1).XluMaterialNode == null && ((MDL0ObjectNode)n2).XluMaterialNode == null)
                return 0;

            //Now check material draw priority
            if (((MDL0ObjectNode)n1).XluMaterialNode.Index > ((MDL0ObjectNode)n2).XluMaterialNode.Index)
                return 1;
            if (((MDL0ObjectNode)n1).XluMaterialNode.Index < ((MDL0ObjectNode)n2).XluMaterialNode.Index)
                return -1;

            //Finally compare the object index
            if (((MDL0ObjectNode)n1).Index > ((MDL0ObjectNode)n2).Index)
                return 1;
            if (((MDL0ObjectNode)n1).Index < ((MDL0ObjectNode)n2).Index)
                return -1;

            //Should never return equal
            return 0;
        }
        #endregion
    }

    public interface IMatrixNodeUser //Objects and Vertices
    {
        IMatrixNode MatrixNode { get; set; }
    }
}
