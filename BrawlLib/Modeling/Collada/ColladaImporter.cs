using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;
using BrawlLib.IO;
using System.Windows.Forms;
using BrawlLib.Wii.Models;
using BrawlLib.Imaging;
using BrawlLib.SSBBTypes;
using BrawlLib.Wii.Graphics;
using System.Globalization;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;

namespace BrawlLib.Modeling
{
    public unsafe partial class Collada : Form
    {
        public Collada() { InitializeComponent(); }
        public Collada(Form owner, string title) : this()
        {
            Owner = owner;
            Text = title;
        }
        private Button button1;
        private Button button2;
        private Panel panel1;

        private Label Status;
        private PropertyGrid propertyGrid1;
        private Panel panel2;
        public string _filePath;

        public void Say(string text)
        {
            Status.Text = text;
            Update();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var info = propertyGrid1.GetType().GetProperty("Controls");
            var collection = (Control.ControlCollection)info.GetValue(propertyGrid1, null);

            foreach (var control in collection)
            {
                var type = control.GetType();
                if ("DocComment" == type.Name)
                {
                    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
                    var field = type.BaseType.GetField("userSized", Flags);
                    field.SetValue(control, true);

                    info = type.GetProperty("Lines");
                    info.SetValue(control, 5, null);

                    propertyGrid1.HelpVisible = true;
                    break;
                }
            }
        }

        public MDL0Node ShowDialog(string filePath)
        {
            _importOptions = BrawlLib.Properties.Settings.Default.ColladaImportOptions;
            propertyGrid1.SelectedObject = _importOptions;

            if (base.ShowDialog() == DialogResult.OK)
            {
                panel1.Visible = false;
                Height = 70;
                UseWaitCursor = true;
                Text = "Please wait...";
                Show();
                Update();
                MDL0Node model = ImportModel(filePath);
                BrawlLib.Properties.Settings.Default.Save();
                Close();
                _importOptions = new ImportOptions();
                return model;
            }
            _importOptions = new ImportOptions();
            return null;
        }

        public static ImportOptions _importOptions = new ImportOptions();
        public class ImportOptions
        {
            public enum MDLType
            {
                Character,
                Stage
            }
            
            [Category("Model"), Description("Determines the default settings for materials and shaders.")]
            public MDLType ModelType { get { return _mdlType; } set { _mdlType = value; } }
            [Category("Model"), Description("If true, object primitives will be culled in reverse. This means the outside of the object will be the inside, and the inside will be the outside. It is not recommended to change this to true as you can change the culling later using the object's material.")]
            public bool ForceCounterClockwisePrimitives { get { return _forceCCW; } set { _forceCCW = value; } }
            [Category("Model"), Description("If true, the file path of the imported model will be written to the model's header.")]
            public bool SetOriginalPath { get { return _setOrigPath; } set { _setOrigPath = value; } }
            [Category("Model"), Description("Determines how precise weights will be compared. A smaller value means more accuracy but also more influences, resulting in a larger file size. A larger value means the weighting will be less accurate but there will be less influences.")]
            public float WeightPrecision { get { return _weightPrecision; } set { _weightPrecision = value.Clamp(0.0000001f, 0.999999f); } }
            [Category("Model"), Description("Sets the model version number, which affects how some parts of the model are written. Only versions 8, 9, 10 and 11 are supported.")]
            public int ModelVersion { get { return _modelVersion; } set { _modelVersion = value.Clamp(8, 11); } }

            [Category("Materials"), Description("The default texture wrap for material texture references.")]
            public MDL0MaterialRefNode.WrapMode TextureWrap { get { return _wrap; } set { _wrap = value; } }
            [Category("Materials"), Description("If true, materials will be remapped. This means there will be no redundant materials with the same settings, saving file space.")]
            public bool RemapMaterials { get { return _rmpMats; } set { _rmpMats = value; } }
            [Category("Materials"), Description("The default setting to use for material culling. Culling determines what side of the mesh is invisible.")]
            public CullMode MaterialCulling { get { return _culling; } set { _culling = value; } }

            [Category("Assets"), Description("If true, vertex arrays will be written in float format. This means that the data size will be larger, but more precise. Float arrays for vertices must be used if the model uses texture matrices, tristripped primitives or SHP0 morph animations; otherwise the model will explode in-game.")]
            public bool ForceFloatVertices { get { return _fltVerts; } set { _fltVerts = value; } }
            [Category("Assets"), Description("If true, normal arrays will be written in float format. This means that the data size will be larger, but more precise.")]
            public bool ForceFloatNormals { get { return _fltNrms; } set { _fltNrms = value; } }
            [Category("Assets"), Description("If true, texture coordinate arrays will be written in float format. This means that the data size will be larger, but more precise.")]
            public bool ForceFloatUVs { get { return _fltUVs; } set { _fltUVs = value; } }
            
            [Category("Color Nodes"), Description("If true, color arrays read from the file will be ignored.")]
            public bool IgnoreOriginalColors { get { return _ignoreColors; } set { _ignoreColors = value; } }
            [Category("Color Nodes"), Description("If true, color arrays will be added to objects that do not have any. The array will be filled with only the default color.")]
            public bool AddColors { get { return _addClrs; } set { _addClrs = value; } }
            [Category("Color Nodes"), Description("If true, color arrays will be remapped. This means there will not be any color nodes that have the same entries as another, saving file space.")]
            public bool RemapColors { get { return _rmpClrs; } set { _rmpClrs = value; } }
            [Category("Color Nodes"), Description("If true, objects without color arrays will use a constant color (set to the default color) in its material for the whole mesh instead of a color node that specifies a color for every vertex. This saves a lot of file space.")]
            public bool UseRegisterColor { get { return _useReg; } set { _useReg = value; } }
            [Category("Color Nodes"), TypeConverter(typeof(RGBAStringConverter)), Description("The default color to use for generated color arrays.")]
            public RGBAPixel DefaultColor { get { return _dfltClr; } set { _dfltClr = value; } }
            [Category("Color Nodes"), TypeConverter(typeof(RGBAStringConverter)), Description("This will make all colors be written in one color node. This will save file space for models with lots of different colors.")]
            public bool UseOneNode { get { return _useOneNode; } set { _useOneNode = value; } }

            [Category("Tristripper"), Description("Determines whether the model will be optimized to use tristrips along with triangles or not. Tristrips can greatly reduce in-game lag, so it is highly recommended that you leave this as true.")]
            public bool UseTristrips { get { return _useTristrips; } set { _useTristrips = value; } }
            [Category("Tristripper"), Description("The size of the cache optimizer which affects the final amount of face points. Set to 0 to disable.")]
            public uint CacheSize { get { return _cacheSize; } set { _cacheSize = value; } }
            [Category("Tristripper"), Description("The minimum amount of triangles that must be included in a strip. This cannot be lower than two triangles and should not be a large number. Two is highly recommended.")]
            public uint MinimumStripLength { get { return _minStripLen; } set { _minStripLen = value < 2 ? 2 : value; } }
            [Category("Tristripper"), Description("When enabled, pushes cache hits into a simple FIFO structure to simulate GPUs that don't duplicate cache entries, affecting the final face point count. Does nothing if the cache is disabled.")]
            public bool PushCacheHits { get { return _pushCacheHits; } set { _pushCacheHits = value; } }
            //[Category("Tristripper"), Description("If true, the tristripper will search for strips backwards as well as forwards.")]
            //public bool BackwardSearch { get { return _backwardSearch; } set { _backwardSearch = value; } }

            public MDLType _mdlType = MDLType.Character;
            public bool _fltVerts = true;
            public bool _fltNrms = true;
            public bool _fltUVs = true;
            public bool _addClrs = false;
            public bool _rmpClrs = true;
            public bool _rmpMats = true;
            public bool _forceCCW = false;
            public bool _useReg = true;
            public bool _ignoreColors = false;
            public CullMode _culling = CullMode.Cull_None;
            public RGBAPixel _dfltClr = new RGBAPixel(100, 100, 100, 255);
            public uint _cacheSize = 52;
            public uint _minStripLen = 2;
            public bool _pushCacheHits = true;
            public bool _useTristrips = true;
            public bool _setOrigPath = false;
            public float _weightPrecision = 0.0001f;
            public int _modelVersion = 9;
            public bool _useOneNode = true;
            public MDL0MaterialRefNode.WrapMode _wrap = MDL0MaterialRefNode.WrapMode.Repeat;
            
            //This doesn't work, but it's optional and not efficient with the cache on anyway
            public bool _backwardSearch = false;

            internal RGBAPixel[] _singleColorNodeEntries;
        }

        public static string Error;
        public MDL0Node ImportModel(string filePath)
        {
            MDL0Node model = new MDL0Node() { _name = Path.GetFileNameWithoutExtension(filePath) };
            model.InitGroups();
            if (_importOptions._setOrigPath)
                model._originalPath = filePath;

            //Parse the collada file and use the data to create an MDL0
            using (DecoderShell shell = DecoderShell.Import(filePath))
            try
            {
                model._version = _importOptions._modelVersion;

                Error = "There was a problem reading the model.";

                //Extract images, removing duplicates
                foreach (ImageEntry img in shell._images)
                {
                    string name;
                    MDL0TextureNode tex;

                    if (img._path != null)
                        name = Path.GetFileNameWithoutExtension(img._path);
                    else
                        name = img._name != null ? img._name : img._id;

                    tex = model.FindOrCreateTexture(name);
                    img._node = tex;
                }

                //Extract materials and create shaders
                int tempNo = -1;
                foreach (MaterialEntry mat in shell._materials)
                {
                    tempNo += 1;
                    MDL0MaterialNode matNode = new MDL0MaterialNode();

                    matNode._parent = model._matGroup;
                    matNode._name = mat._name != null ? mat._name : mat._id;

                    if (tempNo == 0)
                    {
                        MDL0ShaderNode shadNode = new MDL0ShaderNode();
                        shadNode._parent = model._shadGroup;
                        shadNode._name = "Shader" + tempNo;
                        model._shadList.Add(shadNode);
                    }
                    matNode.Shader = "Shader0";
                    matNode.ShaderNode = (MDL0ShaderNode)model._shadGroup.Children[0];

                    mat._node = matNode;
                    matNode._cull = _importOptions._culling;
                    
                    //Find effect
                    if (mat._effect != null)
                        foreach (EffectEntry eff in shell._effects)
                            if (eff._id == mat._effect) //Attach textures and effects to material
                                if (eff._shader != null)
                                    foreach (LightEffectEntry l in eff._shader._effects)
                                        if (l._type == LightEffectType.diffuse && l._texture != null)
                                        {
                                            string path = l._texture;
                                            foreach (EffectNewParam p in eff._newParams)
                                                if (p._sid == l._texture)
                                                {
                                                    path = p._sampler2D._url;
                                                    if (!String.IsNullOrEmpty(p._sampler2D._source))
                                                    {
                                                        foreach (EffectNewParam p2 in eff._newParams)
                                                            if (p2._sid == p._sampler2D._source)
                                                                path = p2._path;
                                                    }
                                                }

                                            foreach (ImageEntry img in shell._images)
                                                if (img._id == path)
                                                {
                                                    MDL0MaterialRefNode mr = new MDL0MaterialRefNode();
                                                    (mr._texture = img._node as MDL0TextureNode)._references.Add(mr);
                                                    mr._name = mr._texture.Name;
                                                    matNode._children.Add(mr);
                                                    mr._parent = matNode;
                                                    mr._minFltr = mr._magFltr = 1;
                                                    mr._index1 = mr._index2 = mr.Index;
                                                    mr._uWrap = mr._vWrap = (int)_importOptions._wrap;
                                                    break;
                                                }
                                        }

                    matNode._numTextures = (byte)matNode.Children.Count;
                    model._matList.Add(matNode);
                }

                Say("Extracting scenes...");

                //Extract scenes
                foreach (SceneEntry scene in shell._scenes)
                {
                    //Parse joints first
                    NodeEntry[] joints = scene._nodes.Where(x => x._type == NodeType.JOINT).ToArray();
                    NodeEntry[] nodes = scene._nodes.Where(x => x._type != NodeType.JOINT).ToArray();
                    foreach (NodeEntry node in joints)
                        EnumNode(node, model._boneGroup, scene, model, shell);
                    foreach (NodeEntry node in nodes)
                        EnumNode(node, model._boneGroup, scene, model, shell);
                }

                //If there are no bones, rig all objects to a single bind.
                if (model._boneGroup._children.Count == 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        MDL0BoneNode bone = new MDL0BoneNode();
                        bone.Scale = new Vector3(1);

                        bone._bindMatrix =
                        bone._inverseBindMatrix =
                        Matrix.Identity;

                        switch (i)
                        {
                            case 0:
                                bone._name = "TopN";
                                model._boneGroup._children.Add(bone);
                                bone._parent = model._boneGroup;
                                break;
                            case 1:
                                bone._name = "TransN";
                                model._boneGroup._children[0]._children.Add(bone);
                                bone._parent = model._boneGroup._children[0];
                                bone.ReferenceCount = model._objList.Count;
                                break;
                        }
                    }
                    if (model._objList != null && model._objList.Count != 0)
                        foreach (MDL0ObjectNode poly in model._objList)
                        {
                            poly._nodeId = 0;
                            poly.MatrixNode = (MDL0BoneNode)model._boneGroup._children[0]._children[0];
                        }
                }
                else
                {
                    //Check each polygon to see if it can be rigged to a single influence
                    if (model._objList != null && model._objList.Count != 0)
                        foreach (MDL0ObjectNode p in model._objList)
                        {
                            IMatrixNode node = null; 
                            bool singlebind = true;

                            foreach (Vertex3 v in p._manager._vertices)
                                if (v._matrixNode != null)
                                {
                                    if (node == null)
                                        node = v._matrixNode;

                                    if (v._matrixNode != node)
                                    {
                                        singlebind = false;
                                        break;
                                    }
                                }

                            if (singlebind && p._matrixNode == null)
                            {
                                //Increase reference count ahead of time for rebuild
                                if (p._manager._vertices[0]._matrixNode != null)
                                    p._manager._vertices[0]._matrixNode.ReferenceCount++;

                                foreach (Vertex3 v in p._manager._vertices)
                                    if (v._matrixNode != null)
                                        v._matrixNode.ReferenceCount--;
                                
                                p._nodeId = -2; //Continued on polygon rebuild
                            }
                        }
                }

                //Remove original color buffers if option set
                if (_importOptions._ignoreColors)
                {
                    if (model._objList != null && model._objList.Count != 0)
                        foreach (MDL0ObjectNode p in model._objList)
                            for (int x = 2; x < 4; x++)
                                if (p._manager._faceData[x] != null)
                                {
                                    p._manager._faceData[x].Dispose();
                                    p._manager._faceData[x] = null;
                                }
                }

                //Add color buffers if option set
                if (_importOptions._addClrs)
                {
                    RGBAPixel pixel = _importOptions._dfltClr;

                    //Add a color buffer to objects that don't have one
                    if (model._objList != null && model._objList.Count != 0)
                        foreach (MDL0ObjectNode p in model._objList)
                            if (p._manager._faceData[2] == null)
                            {
                                RGBAPixel* pIn = (RGBAPixel*)(p._manager._faceData[2] = new UnsafeBuffer(4 * p._manager._pointCount)).Address;
                                for (int i = 0; i < p._manager._pointCount; i++)
                                    *pIn++ = pixel;
                            }
                }

                //Apply defaults to materials
                if (model._matList != null)
                    foreach (MDL0MaterialNode p in model._matList)
                    {
                        if (_importOptions._mdlType == 0)
                        {
                            p._lSet = 20;
                            p._fSet = 4;
                            p._ssc = 3;

                            p.C1ColorEnabled = true;
                            p.C1AlphaMaterialSource = GXColorSrc.Vertex;
                            p.C1ColorMaterialSource = GXColorSrc.Vertex;
                            p.C1ColorDiffuseFunction = GXDiffuseFn.Clamped;
                            p.C1ColorAttenuation = GXAttnFn.Spotlight;
                            p.C1AlphaEnabled = true;
                            p.C1AlphaDiffuseFunction = GXDiffuseFn.Clamped;
                            p.C1AlphaAttenuation = GXAttnFn.Spotlight;

                            p.C2ColorDiffuseFunction = GXDiffuseFn.Disabled;
                            p.C2ColorAttenuation = GXAttnFn.None;
                            p.C2AlphaDiffuseFunction = GXDiffuseFn.Disabled;
                            p.C2AlphaAttenuation = GXAttnFn.None;
                        }
                        else
                        {
                            p._lSet = 0;
                            p._fSet = 0;
                            p._ssc = 1;

                            p._chan1.Color = new LightChannelControl(1795);
                            p._chan1.Alpha = new LightChannelControl(1795);
                            p._chan2.Color = new LightChannelControl(1795);
                            p._chan2.Alpha = new LightChannelControl(1795);
                        }
                    }

                //Set materials to use register color if option set
                if (_importOptions._useReg && model._objList != null)
                    foreach (MDL0ObjectNode p in model._objList)
                    {
                        MDL0MaterialNode m = p.OpaMaterialNode;
                        if (m != null && p._manager._faceData[2] == null && p._manager._faceData[3] == null)
                        {
                            m.C1MaterialColor = _importOptions._dfltClr;
                            m.C1ColorMaterialSource = GXColorSrc.Register;
                            m.C1AlphaMaterialSource = GXColorSrc.Register;
                        }
                    }

                //Remap materials if option set
                if (_importOptions._rmpMats && model._matList != null && model._objList != null)
                {
                    foreach (MDL0ObjectNode p in model._objList)
                        foreach (MDL0MaterialNode m in model._matList)
                            if (m.Children.Count > 0 &&
                                m.Children[0] != null &&
                                p.OpaMaterialNode != null &&
                                p.OpaMaterialNode.Children.Count > 0 &&
                                p.OpaMaterialNode.Children[0] != null &&
                                m.Children[0].Name == p.OpaMaterialNode.Children[0].Name &&
                                m.C1ColorMaterialSource == p.OpaMaterialNode.C1ColorMaterialSource)
                            {
                                p.OpaMaterialNode = m;
                                break;
                            }

                    //Remove unused materials
                    for (int i = 0; i < model._matList.Count; i++)
                        if (((MDL0MaterialNode)model._matList[i])._objects.Count == 0)
                            model._matList.RemoveAt(i--);
                }

                Error = "There was a problem writing the model.";

                //Clean the model and then build it!
                if (model != null)
                {
                    model.CleanGroups();
                    model.BuildFromScratch(this);
                }
            }
            catch (Exception x)
            {
                MessageBox.Show("Cannot continue importing this model.\n" + Error + "\n\nException:\n" + x.ToString());
                model = null;
                Close();
            }
            finally
            {
                //Clean up the mess we've made
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }
            return model;
        }
        private void EnumNode(NodeEntry node, ResourceNode parent, SceneEntry scene, MDL0Node model, DecoderShell shell)
        {
            MDL0BoneNode bone = null;
            Influence inf = null;

            if (node._type == NodeType.JOINT)
            {
                Error = "There was a problem creating a new bone.";

                bone = new MDL0BoneNode();
                bone._name = node._name != null ? node._name : node._id;

                bone._bindState = node._transform;
                node._node = bone;

                parent._children.Add(bone);
                bone._parent = parent;

                bone.RecalcBindState();
                bone.CalcFlags();

                foreach (NodeEntry e in node._children)
                    EnumNode(e, bone, scene, model, shell);

                inf = new Influence(bone);
                model._influences._influences.Add(inf);
            }
            else
                foreach (NodeEntry e in node._children)
                    EnumNode(e, parent, scene, model, shell);

            foreach (InstanceEntry inst in node._instances)
            {
                if (inst._type == InstanceType.Controller)
                {
                    foreach (SkinEntry skin in shell._skins)
                        if (skin._id == inst._url)
                        {
                            foreach (GeometryEntry g in shell._geometry)
                                if (g._id == skin._skinSource)
                                {
                                    Error = @"
                                    There was a problem decoding weighted primitives for the object " + (node._name != null ? node._name : node._id) + 
                                    ".\nOne or more vertices may not be weighted correctly.";
                                    Say("Decoding weighted primitives for " + (g._name != null ? g._name : g._id) + "...");
                                    CreateObject(inst, node, parent, DecodePrimitivesWeighted(node, g, skin, scene, model._influences, ref Error), model, shell);
                                    break;
                                }
                            break;
                        }
                }
                else if (inst._type == InstanceType.Geometry)
                {
                    foreach (GeometryEntry g in shell._geometry)
                        if (g._id == inst._url)
                        {
                            Error = "There was a problem decoding unweighted primitives for the object " + (node._name != null ? node._name : node._id) + ".";
                            Say("Decoding unweighted primitives for " + (g._name != null ? g._name : g._id) + "...");
                            CreateObject(inst, node, parent, DecodePrimitivesUnweighted(node, g), model, shell);
                            break;
                        }
                }
                else
                {
                    foreach (NodeEntry e in shell._nodes)
                        if (e._id == inst._url)
                            EnumNode(e, parent, scene, model, shell);
                }
            }
        }

        private void CreateObject(InstanceEntry inst, NodeEntry node, ResourceNode parent, PrimitiveManager manager, MDL0Node model, DecoderShell shell)
        {
            if (manager != null)
            {
                Error = "There was a problem creating a new object for " + (node._name != null ? node._name : node._id);
                int i = 0;
                foreach (Vertex3 v in manager._vertices)
                    v._index = i++;

                MDL0ObjectNode poly = new MDL0ObjectNode() { _manager = manager };
                poly._manager._polygon = poly;
                poly._name = node._name != null ? node._name : node._id;

                //Attach single-bind
                if (parent != null && parent is MDL0BoneNode)
                    poly.MatrixNode = (MDL0BoneNode)parent;

                //Attach material
                if (inst._material != null)
                    foreach (MaterialEntry mat in shell._materials)
                        if (mat._id == inst._material._target)
                        {
                            (poly._opaMaterial = (mat._node as MDL0MaterialNode))._objects.Add(poly);
                            break;
                        }

                model._numFaces += poly._numFaces = manager._faceCount = manager._pointCount / 3;
                model._numFacepoints += poly._numFacepoints = manager._pointCount;
                
                poly._parent = model._objGroup;
                model._objList.Add(poly);
            }
        }

        private class ColladaEntry : IDisposable
        {
            internal string _id, _name, _sid;
            internal object _node;

            ~ColladaEntry() { Dispose(); }
            public virtual void Dispose() { GC.SuppressFinalize(this); }
        }
        private class ImageEntry : ColladaEntry
        {
            internal string _path;
        }
        private class MaterialEntry : ColladaEntry
        {
            internal string _effect;
        }
        private class EffectEntry : ColladaEntry
        {
            internal EffectShaderEntry _shader;
            internal List<EffectNewParam> _newParams = new List<EffectNewParam>();
        }
        private class GeometryEntry : ColladaEntry
        {
            internal List<SourceEntry> _sources = new List<SourceEntry>();
            internal List<PrimitiveEntry> _primitives = new List<PrimitiveEntry>();

            internal int _faces, _lines;

            internal string _verticesId;
            internal InputEntry _verticesInput;

            public override void Dispose()
            {
                foreach (SourceEntry p in _sources)
                    p.Dispose();
                GC.SuppressFinalize(this);
            }
        }
        private class SourceEntry : ColladaEntry
        {
            internal SourceType _arrayType;
            internal string _arrayId;
            internal int _arrayCount;
            internal object _arrayData; //Parser must dispose!

            internal string _accessorSource;
            internal int _accessorCount;
            internal int _accessorStride;

            public override void Dispose()
            {
                if (_arrayData is UnsafeBuffer)
                    ((UnsafeBuffer)_arrayData).Dispose();
                _arrayData = null;
                GC.SuppressFinalize(this);
            }
        }
        private class InputEntry : ColladaEntry
        {
            internal SemanticType _semantic;
            internal int _set;
            internal int _offset;
            internal string _source;
            internal int _outputOffset;
        }
        private class PrimitiveEntry
        {
            internal PrimitiveType _type;

            internal string _material;
            internal int _entryCount;
            internal int _entryStride;
            internal int _faceCount, _pointCount;

            internal List<InputEntry> _inputs = new List<InputEntry>();

            internal List<PrimitiveFace> _faces = new List<PrimitiveFace>();
        }
        private class PrimitiveFace
        {
            internal int _pointCount;
            internal int _faceCount;
            internal ushort[] _pointIndices;
        }
        private class SkinEntry : ColladaEntry
        {
            internal string _skinSource;
            internal Matrix _bindMatrix = Matrix.Identity;

            //internal Matrix _bindShape;
            internal List<SourceEntry> _sources = new List<SourceEntry>();
            internal List<InputEntry> _jointInputs = new List<InputEntry>();

            internal List<InputEntry> _weightInputs = new List<InputEntry>();
            internal int _weightCount;
            internal int[][] _weights;

            public override void Dispose()
            {
                foreach (SourceEntry src in _sources)
                    src.Dispose();
                GC.SuppressFinalize(this);
            }
        }
        private class SceneEntry : ColladaEntry
        {
            internal List<NodeEntry> _nodes = new List<NodeEntry>();

            public NodeEntry FindNode(string name)
            {
                NodeEntry n;
                foreach (NodeEntry node in _nodes)
                    if ((n = DecoderShell.FindNodeInternal(name, node)) != null)
                        return n;
                return null;
            }
        }
        private class NodeEntry : ColladaEntry
        {
            internal NodeType _type = NodeType.NONE;
            internal FrameState _transform;
            internal Matrix _matrix = Matrix.Identity;
            internal List<NodeEntry> _children = new List<NodeEntry>();
            internal List<InstanceEntry> _instances = new List<InstanceEntry>();

            public static int Compare(NodeEntry n1, NodeEntry n2)
            {
                if ((n1._type == NodeType.NODE || n1._type == NodeType.NONE) && n2._type == NodeType.JOINT)
                    return 1;
                if (n1._type == NodeType.JOINT && (n2._type == NodeType.NODE || n2._type == NodeType.NONE))
                    return -1;

                return 0;
            }
        }
        private enum InstanceType
        {
            Controller,
            Geometry,
            Node
        }
        private class InstanceEntry : ColladaEntry
        {
            internal InstanceType _type;
            internal string _url;
            internal InstanceMaterial _material;
            internal List<string> skeletons = new List<string>();
        }
        private class InstanceMaterial : ColladaEntry
        {
            internal string _symbol, _target;
            internal List<VertexBind> _vertexBinds = new List<VertexBind>();
        }
        private class VertexBind : ColladaEntry
        {
            internal string _semantic;
            internal string _inputSemantic;
            internal int _inputSet;
        }
        private class EffectSampler2D
        {
            public string _source;
            public string _url;
            public string _wrapS, _wrapT;
            public string _minFilter, _magFilter;
        }
        private class EffectNewParam : ColladaEntry
        {
            public string _path;
            public EffectSampler2D _sampler2D;
        }
        private class EffectShaderEntry : ColladaEntry
        {
            internal ShaderType _type;
            internal float _shininess, _reflectivity, _transparency;
            internal List<LightEffectEntry> _effects = new List<LightEffectEntry>();
        }
        private class LightEffectEntry : ColladaEntry
        {
            internal LightEffectType _type;
            internal RGBAPixel _color;

            internal string _texture;
            internal string _texCoord;
        }
        private enum ShaderType
        {
            None,
            phong,
            lambert,
            blinn
        }
        private enum LightEffectType
        {
            None,
            ambient,
            diffuse,
            emission,
            reflective,
            specular,
            transparent
        }
        private enum PrimitiveType
        {
            None,
            polygons,
            polylist,
            triangles,
            trifans,
            tristrips,
            lines,
            linestrips
        }
        private enum SemanticType
        {
            None,
            POSITION,
            VERTEX,
            NORMAL,
            TEXCOORD,
            COLOR,
            WEIGHT,
            JOINT,
            INV_BIND_MATRIX,
            TEXTANGENT,
            TEXBINORMAL
        }
        private enum SourceType
        {
            None,
            Float,
            Int,
            Name
        }
        private enum NodeType
        {
            NODE,
            JOINT,
            NONE
        }

        private class DecoderShell : IDisposable
        {
            internal List<ImageEntry> _images = new List<ImageEntry>();
            internal List<MaterialEntry> _materials = new List<MaterialEntry>();
            internal List<EffectEntry> _effects = new List<EffectEntry>();
            internal List<GeometryEntry> _geometry = new List<GeometryEntry>();
            internal List<SkinEntry> _skins = new List<SkinEntry>();
            internal List<NodeEntry> _nodes = new List<NodeEntry>();
            internal List<SceneEntry> _scenes = new List<SceneEntry>();
            internal XmlReader _reader;
            internal int _v1, _v2, _v3;

            public static DecoderShell Import(string path)
            {
                using (FileMap map = FileMap.FromFile(path))
                using (XmlReader reader = new XmlReader(map.Address, map.Length))
                    return new DecoderShell(reader);
            }
            ~DecoderShell() { Dispose(); }
            public void Dispose()
            {
                foreach (GeometryEntry geo in _geometry)
                    geo.Dispose();
            }

            private void Output(string message)
            {
                MessageBox.Show(message);
            }

            private DecoderShell(XmlReader reader)
            {
                _reader = reader;

                while (reader.BeginElement())
                {
                    if (reader.Name.Equals("COLLADA", true))
                        ParseMain();

                    reader.EndElement();
                }

                _reader = null;
            }

            public NodeEntry FindNode(string name)
            {
                NodeEntry n;
                foreach (SceneEntry scene in _scenes)
                    foreach (NodeEntry node in scene._nodes)
                        if ((n = FindNodeInternal(name, node)) != null)
                            return n;
                return null;
            }
            internal static NodeEntry FindNodeInternal(string name, NodeEntry node)
            {
                NodeEntry e;

                if (node._name == name || node._sid == name || node._id == name)
                    return node;

                foreach (NodeEntry n in node._children)
                    if ((e = FindNodeInternal(name, n)) != null)
                        return e;

                return null;
            }

            private void ParseMain()
            {
                while (_reader.ReadAttribute())
                    if (_reader.Name.Equals("version", true))
                    {
                        string v = (string)_reader.Value;
                        string[] s = v.Split('.');
                        int.TryParse(s[0], NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out _v1);
                        int.TryParse(s[1], NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out _v2);
                        int.TryParse(s[2], NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out _v3);
                    }
                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("asset", true))
                        ParseAsset();
                    else if (_reader.Name.Equals("library_images", true))
                        ParseLibImages();
                    else if (_reader.Name.Equals("library_materials", true))
                        ParseLibMaterials();
                    else if (_reader.Name.Equals("library_effects", true))
                        ParseLibEffects();
                    else if (_reader.Name.Equals("library_geometries", true))
                        ParseLibGeometry();
                    else if (_reader.Name.Equals("library_controllers", true))
                        ParseLibControllers();
                    else if (_reader.Name.Equals("library_visual_scenes", true))
                        ParseLibScenes();
                    else if (_reader.Name.Equals("library_nodes", true))
                        ParseLibNodes();

                    _reader.EndElement();
                }
            }
            
            public float _scale = 1;
            private void ParseAsset()
            {
                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("unit", true))
                        while (_reader.ReadAttribute())
                            if (_reader.Name.Equals("meter", true))
                                float.TryParse((string)_reader.Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _scale);
                    _reader.EndElement();
                }
            }

            private void ParseLibImages()
            {
                ImageEntry img;
                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("image", true))
                    {
                        img = new ImageEntry();
                        while (_reader.ReadAttribute())
                        {
                            if (_reader.Name.Equals("id", true))
                                img._id = (string)_reader.Value;
                            else if (_reader.Name.Equals("name", true))
                                img._name = (string)_reader.Value;
                        }

                        while (_reader.BeginElement())
                        {
                            img._path = null;
                            if (_reader.Name.Equals("init_from", true))
                            {
                                if (_v2 < 5)
                                    img._path = _reader.ReadElementString();
                                else
                                    while (_reader.BeginElement())
                                    {
                                        if (_reader.Name.Equals("ref", true))
                                            img._path = _reader.ReadElementString();
                                        _reader.EndElement();
                                    }
                            }

                            _reader.EndElement();
                        }

                        _images.Add(img);
                    }
                    _reader.EndElement();
                }
            }
            private void ParseLibMaterials()
            {
                MaterialEntry mat;
                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("material", true))
                    {
                        mat = new MaterialEntry();
                        while (_reader.ReadAttribute())
                        {
                            if (_reader.Name.Equals("id", true))
                                mat._id = (string)_reader.Value;
                            else if (_reader.Name.Equals("name", true))
                                mat._name = (string)_reader.Value;
                        }

                        while (_reader.BeginElement())
                        {
                            if (_reader.Name.Equals("instance_effect", true))
                                while (_reader.ReadAttribute())
                                    if (_reader.Name.Equals("url", true))
                                        mat._effect = _reader.Value[0] == '#' ? (string)(_reader.Value + 1) : (string)_reader.Value;

                            _reader.EndElement();
                        }

                        _materials.Add(mat);
                    }

                    _reader.EndElement();
                }
            }
            private void ParseLibEffects()
            {
                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("effect", true))
                        _effects.Add(ParseEffect());
                    _reader.EndElement();
                }
            }
            private EffectEntry ParseEffect()
            {
                EffectEntry eff = new EffectEntry();

                while (_reader.ReadAttribute())
                {
                    if (_reader.Name.Equals("id", true))
                        eff._id = (string)_reader.Value;
                    else if (_reader.Name.Equals("name", true))
                        eff._name = (string)_reader.Value;
                }

                while (_reader.BeginElement())
                {
                    //Only common is supported
                    if (_reader.Name.Equals("profile_COMMON", true))
                        while (_reader.BeginElement())
                        {
                            if (_reader.Name.Equals("newparam", true))
                                eff._newParams.Add(ParseNewParam());
                            else if (_reader.Name.Equals("technique", true))
                            {
                                while (_reader.BeginElement())
                                {
                                    if (_reader.Name.Equals("phong", true))
                                        eff._shader = ParseShader(ShaderType.phong);
                                    else if (_reader.Name.Equals("lambert", true))
                                        eff._shader = ParseShader(ShaderType.lambert);
                                    else if (_reader.Name.Equals("blinn", true))
                                        eff._shader = ParseShader(ShaderType.blinn);

                                    _reader.EndElement();
                                }
                            }
                            _reader.EndElement();
                        }

                    _reader.EndElement();
                }
                return eff;
            }
            private EffectNewParam ParseNewParam()
            {
                EffectNewParam p = new EffectNewParam();

                while (_reader.ReadAttribute())
                {
                    if (_reader.Name.Equals("sid", true))
                        p._sid = (string)_reader.Value;
                    else if (_reader.Name.Equals("id", true))
                        p._id = (string)_reader.Value;
                }
                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("surface", true))
                    {
                        while (_reader.BeginElement())
                        {
                            p._path = null;
                            if (_reader.Name.Equals("init_from", true))
                            {
                                if (_v2 < 5)
                                    p._path = _reader.ReadElementString();
                                else
                                    while (_reader.BeginElement())
                                    {
                                        if (_reader.Name.Equals("ref", true))
                                            p._path = _reader.ReadElementString();
                                        _reader.EndElement();
                                    }
                            }
                            _reader.EndElement();
                        }
                    }
                    else if (_reader.Name.Equals("sampler2D", true))
                        p._sampler2D = ParseSampler2D();
                    
                    _reader.EndElement();
                }

                return p;
            }
            private EffectSampler2D ParseSampler2D()
            {
                EffectSampler2D s = new EffectSampler2D();

                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("source", true))
                        s._source = _reader.ReadElementString();
                    else if (_reader.Name.Equals("instance_image", true))
                    {
                        while (_reader.ReadAttribute())
                        {
                            if (_reader.Name.Equals("url", true))
                                s._url = _reader.Value[0] == '#' ? (string)(_reader.Value + 1) : (string)_reader.Value;
                        }
                    }
                    else if (_reader.Name.Equals("wrap_s", true))
                        s._wrapS = _reader.ReadElementString();
                    else if (_reader.Name.Equals("wrap_t", true))
                        s._wrapT = _reader.ReadElementString();
                    else if (_reader.Name.Equals("minfilter", true))
                        s._minFilter = _reader.ReadElementString();
                    else if (_reader.Name.Equals("magfilter", true))
                        s._magFilter = _reader.ReadElementString();

                    _reader.EndElement();
                }

                return s;
            }
            private EffectShaderEntry ParseShader(ShaderType type)
            {
                EffectShaderEntry s = new EffectShaderEntry();
                s._type = type;
                float v;

                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("ambient", true))
                        s._effects.Add(ParseLightEffect(LightEffectType.ambient));
                    else if (_reader.Name.Equals("diffuse", true))
                        s._effects.Add(ParseLightEffect(LightEffectType.diffuse));
                    else if (_reader.Name.Equals("emission", true))
                        s._effects.Add(ParseLightEffect(LightEffectType.emission));
                    else if (_reader.Name.Equals("reflective", true))
                        s._effects.Add(ParseLightEffect(LightEffectType.reflective));
                    else if (_reader.Name.Equals("specular", true))
                        s._effects.Add(ParseLightEffect(LightEffectType.specular));
                    else if (_reader.Name.Equals("transparent", true))
                        s._effects.Add(ParseLightEffect(LightEffectType.transparent));
                    else if (_reader.Name.Equals("shininess", true))
                    {
                        while (_reader.BeginElement())
                        {
                            if (_reader.Name.Equals("float", true))
                                if (_reader.ReadValue(&v))
                                    s._shininess = v;
                            _reader.EndElement();
                        }
                    }
                    else if (_reader.Name.Equals("reflectivity", true))
                    {
                        while (_reader.BeginElement())
                        {
                            if (_reader.Name.Equals("float", true))
                                if (_reader.ReadValue(&v))
                                    s._reflectivity = v;
                            _reader.EndElement();
                        }
                    }
                    else if (_reader.Name.Equals("transparency", true))
                    {
                        while (_reader.BeginElement())
                        {
                            if (_reader.Name.Equals("float", true))
                                if (_reader.ReadValue(&v))
                                    s._transparency = v;
                            _reader.EndElement();
                        }
                    }

                    _reader.EndElement();
                }

                return s;
            }
            private LightEffectEntry ParseLightEffect(LightEffectType type)
            {
                LightEffectEntry eff = new LightEffectEntry();
                eff._type = type;

                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("color", true))
                        eff._color = ParseColor();
                    else if (_reader.Name.Equals("texture", true))
                    {
                        while (_reader.ReadAttribute())
                            if (_reader.Name.Equals("texture", true))
                                eff._texture = (string)_reader.Value;
                            else if (_reader.Name.Equals("texcoord", true))
                                eff._texCoord = (string)_reader.Value;
                    }

                    _reader.EndElement();
                }

                return eff;
            }
            private void ParseLibGeometry()
            {
                GeometryEntry geo;
                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("geometry", true))
                    {
                        geo = new GeometryEntry();
                        while (_reader.ReadAttribute())
                        {
                            if (_reader.Name.Equals("id", true))
                                geo._id = (string)_reader.Value;
                            else if (_reader.Name.Equals("name", true))
                                geo._name = (string)_reader.Value;
                        }

                        while (_reader.BeginElement())
                        {
                            if (_reader.Name.Equals("mesh", true))
                            {
                                while (_reader.BeginElement())
                                {
                                    if (_reader.Name.Equals("source", true))
                                        geo._sources.Add(ParseSource());
                                    else if (_reader.Name.Equals("vertices", true))
                                    {
                                        while (_reader.ReadAttribute())
                                            if (_reader.Name.Equals("id", true))
                                                geo._verticesId = (string)_reader.Value;

                                        while (_reader.BeginElement())
                                        {
                                            if (_reader.Name.Equals("input", true))
                                                geo._verticesInput = ParseInput();

                                            _reader.EndElement();
                                        }
                                    }
                                    else if (_reader.Name.Equals("polygons", true))
                                        geo._primitives.Add(ParsePrimitive(PrimitiveType.polygons));
                                    else if (_reader.Name.Equals("polylist", true))
                                        geo._primitives.Add(ParsePrimitive(PrimitiveType.polylist));
                                    else if (_reader.Name.Equals("triangles", true))
                                        geo._primitives.Add(ParsePrimitive(PrimitiveType.triangles));
                                    else if (_reader.Name.Equals("tristrips", true))
                                        geo._primitives.Add(ParsePrimitive(PrimitiveType.tristrips));
                                    else if (_reader.Name.Equals("trifans", true))
                                        geo._primitives.Add(ParsePrimitive(PrimitiveType.trifans));
                                    else if (_reader.Name.Equals("lines", true))
                                        geo._primitives.Add(ParsePrimitive(PrimitiveType.lines));
                                    else if (_reader.Name.Equals("linestrips", true))
                                        geo._primitives.Add(ParsePrimitive(PrimitiveType.linestrips));

                                    _reader.EndElement();
                                }
                            }
                            _reader.EndElement();
                        }

                        _geometry.Add(geo);
                    }
                    _reader.EndElement();
                }
            }
            private PrimitiveEntry ParsePrimitive(PrimitiveType type)
            {
                PrimitiveEntry prim = new PrimitiveEntry() { _type = type };
                PrimitiveFace p;
                int val;
                int stride = 0, elements = 0;

                switch (type)
                {
                    case PrimitiveType.trifans:
                    case PrimitiveType.tristrips:
                    case PrimitiveType.triangles:
                        stride = 3;
                        break;
                    case PrimitiveType.lines:
                    case PrimitiveType.linestrips:
                        stride = 2;
                        break;
                    case PrimitiveType.polygons:
                    case PrimitiveType.polylist:
                        stride = 4;
                        break;
                }

                while (_reader.ReadAttribute())
                    if (_reader.Name.Equals("material", true))
                        prim._material = (string)_reader.Value;
                    else if (_reader.Name.Equals("count", true))
                        prim._entryCount = int.Parse((string)_reader.Value);

                prim._faces.Capacity = prim._entryCount;

                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("input", true))
                    {
                        prim._inputs.Add(ParseInput());
                        elements++;
                    }
                    else if (_reader.Name.Equals("p", true))
                    {
                        List<ushort> indices = new List<ushort>(stride * elements);

                        p = new PrimitiveFace();
                        //p._pointIndices.Capacity = stride * elements;
                        while (_reader.ReadValue(&val))
                            indices.Add((ushort)val);

                        p._pointCount = indices.Count / elements;
                        p._pointIndices = indices.ToArray();

                        switch (type)
                        {
                            case PrimitiveType.trifans:
                            case PrimitiveType.tristrips:
                            case PrimitiveType.polygons:
                            case PrimitiveType.polylist:
                                p._faceCount = p._pointCount - 2;
                                break;

                            case PrimitiveType.triangles:
                                p._faceCount = p._pointCount / 3;
                                break;

                            case PrimitiveType.lines:
                                p._faceCount = p._pointCount / 2;
                                break;

                            case PrimitiveType.linestrips:
                                p._faceCount = p._pointCount - 1;
                                break;
                        }

                        prim._faceCount += p._faceCount;
                        prim._pointCount += p._pointCount;
                        prim._faces.Add(p);
                    }

                    _reader.EndElement();
                }

                prim._entryStride = elements;

                return prim;
            }
            private InputEntry ParseInput()
            {
                InputEntry inp = new InputEntry();

                while (_reader.ReadAttribute())
                    if (_reader.Name.Equals("id", true))
                        inp._id = (string)_reader.Value;
                    else if (_reader.Name.Equals("name", true))
                        inp._name = (string)_reader.Value;
                    else if (_reader.Name.Equals("semantic", true))
                        inp._semantic = (SemanticType)Enum.Parse(typeof(SemanticType), (string)_reader.Value, true);
                    else if (_reader.Name.Equals("set", true))
                        inp._set = int.Parse((string)_reader.Value);
                    else if (_reader.Name.Equals("offset", true))
                        inp._offset = int.Parse((string)_reader.Value);
                    else if (_reader.Name.Equals("source", true))
                        inp._source = _reader.Value[0] == '#' ? (string)(_reader.Value + 1) : (string)_reader.Value;

                return inp;
            }
            private SourceEntry ParseSource()
            {
                SourceEntry src = new SourceEntry();

                while (_reader.ReadAttribute())
                    if (_reader.Name.Equals("id", true))
                        src._id = (string)_reader.Value;

                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("float_array", true))
                    {
                        if (src._arrayType == SourceType.None)
                        {
                            src._arrayType = SourceType.Float;

                            while (_reader.ReadAttribute())
                                if (_reader.Name.Equals("id", true))
                                    src._arrayId = (string)_reader.Value;
                                else if (_reader.Name.Equals("count", true))
                                    src._arrayCount = int.Parse((string)_reader.Value);

                            UnsafeBuffer buffer = new UnsafeBuffer(src._arrayCount * 4);
                            src._arrayData = buffer;

                            float* pOut = (float*)buffer.Address;
                            for (int i = 0; i < src._arrayCount; i++)
                                if (!_reader.ReadValue(pOut++))
                                    break;
                        }
                    }
                    else if (_reader.Name.Equals("int_array", true))
                    {
                        if (src._arrayType == SourceType.None)
                        {
                            src._arrayType = SourceType.Int;

                            while (_reader.ReadAttribute())
                                if (_reader.Name.Equals("id", true))
                                    src._arrayId = (string)_reader.Value;
                                else if (_reader.Name.Equals("count", true))
                                    src._arrayCount = int.Parse((string)_reader.Value);

                            UnsafeBuffer buffer = new UnsafeBuffer(src._arrayCount * 4);
                            src._arrayData = buffer;

                            int* pOut = (int*)buffer.Address;
                            for (int i = 0; i < src._arrayCount; i++)
                                if (!_reader.ReadValue(pOut++))
                                    break;
                        }
                    }
                    else if (_reader.Name.Equals("Name_array", true))
                    {
                        if (src._arrayType == SourceType.None)
                        {
                            src._arrayType = SourceType.Name;

                            while (_reader.ReadAttribute())
                                if (_reader.Name.Equals("id", true))
                                    src._arrayId = (string)_reader.Value;
                                else if (_reader.Name.Equals("count", true))
                                    src._arrayCount = int.Parse((string)_reader.Value);

                            string[] list = new string[src._arrayCount];
                            src._arrayData = list;

                            for (int i = 0; i < src._arrayCount; i++)
                                if (!_reader.ReadStringSingle())
                                    break;
                                else
                                    list[i] = (string)_reader.Value;
                        }
                    }
                    else if (_reader.Name.Equals("technique_common", true))
                    {
                        while (_reader.BeginElement())
                        {
                            if (_reader.Name.Equals("accessor", true))
                            {
                                while (_reader.ReadAttribute())
                                    if (_reader.Name.Equals("source", true))
                                        src._accessorSource = _reader.Value[0] == '#' ? (string)(_reader.Value + 1) : (string)_reader.Value;
                                    else if (_reader.Name.Equals("count", true))
                                        src._accessorCount = int.Parse((string)_reader.Value);
                                    else if (_reader.Name.Equals("stride", true))
                                        src._accessorStride = int.Parse((string)_reader.Value);

                                //Ignore params
                            }

                            _reader.EndElement();
                        }
                    }

                    _reader.EndElement();
                }

                return src;
            }

            private void ParseLibControllers()
            {
                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("controller", false))
                    {
                        string id = null;
                        while (_reader.ReadAttribute())
                            if (_reader.Name.Equals("id", false))
                                id = (string)_reader.Value;

                        while (_reader.BeginElement())
                        {
                            if (_reader.Name.Equals("skin", false))
                                _skins.Add(ParseSkin(id));

                            _reader.EndElement();
                        }
                    }
                    _reader.EndElement();
                }
            }

            private SkinEntry ParseSkin(string id)
            {
                SkinEntry skin = new SkinEntry();
                skin._id = id;

                while (_reader.ReadAttribute())
                    if (_reader.Name.Equals("source", false))
                        skin._skinSource = _reader.Value[0] == '#' ? (string)(_reader.Value + 1) : (string)_reader.Value;

                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("bind_shape_matrix", false))
                        skin._bindMatrix = ParseMatrix();
                    else if (_reader.Name.Equals("source", false))
                        skin._sources.Add(ParseSource());
                    else if (_reader.Name.Equals("joints", false))
                        while (_reader.BeginElement())
                        {
                            if (_reader.Name.Equals("input", false))
                                skin._jointInputs.Add(ParseInput());

                            _reader.EndElement();
                        }
                    else if (_reader.Name.Equals("vertex_weights", false))
                    {
                        while (_reader.ReadAttribute())
                            if (_reader.Name.Equals("count", false))
                                skin._weightCount = int.Parse((string)_reader.Value);

                        skin._weights = new int[skin._weightCount][];

                        while (_reader.BeginElement())
                        {
                            if (_reader.Name.Equals("input", false))
                                skin._weightInputs.Add(ParseInput());
                            else if (_reader.Name.Equals("vcount", false))
                            {
                                for (int i = 0; i < skin._weightCount; i++)
                                {
                                    int val;
                                    _reader.ReadValue(&val);
                                    skin._weights[i] = new int[val * skin._weightInputs.Count];
                                }
                            }
                            else if (_reader.Name.Equals("v", false))
                            {
                                for (int i = 0; i < skin._weightCount; i++)
                                {
                                    int[] weights = skin._weights[i];
                                    fixed (int* p = weights)
                                        for (int x = 0; x < weights.Length; x++)
                                            _reader.ReadValue(&p[x]);
                                }
                            }
                            _reader.EndElement();
                        }
                    }

                    _reader.EndElement();
                }

                return skin;
            }

            private void ParseLibNodes()
            {
                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("node", true))
                        _nodes.Add(ParseNode());

                    _reader.EndElement();
                }
            }

            private void ParseLibScenes()
            {
                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("visual_scene", true))
                        _scenes.Add(ParseScene());

                    _reader.EndElement();
                }
            }

            private SceneEntry ParseScene()
            {
                SceneEntry sc = new SceneEntry();

                while (_reader.ReadAttribute())
                    if (_reader.Name.Equals("id", true))
                        sc._id = (string)_reader.Value;

                while (_reader.ReadAttribute())
                    if (_reader.Name.Equals("name", true))
                        sc._name = (string)_reader.Value;

                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("node", true))
                        sc._nodes.Add(ParseNode());

                    _reader.EndElement();
                }

                return sc;
            }

            private NodeEntry ParseNode()
            {
                NodeEntry node = new NodeEntry();

                while (_reader.ReadAttribute())
                    if (_reader.Name.Equals("id", true))
                        node._id = (string)_reader.Value;
                    else if (_reader.Name.Equals("name", true))
                        node._name = (string)_reader.Value;
                    else if (_reader.Name.Equals("sid", true))
                        node._sid = (string)_reader.Value;
                    else if (_reader.Name.Equals("type", true))
                        node._type = (NodeType)Enum.Parse(typeof(NodeType), (string)_reader.Value, true);

                Matrix m = Matrix.Identity;
                //if (node._type != NodeType.JOINT)
                //    node._type = NodeType.JOINT;
                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("matrix", true))
                        m *= ParseMatrix();
                    else if (_reader.Name.Equals("rotate", true))
                    {
                        Vector4 v = ParseVec4();
                        m *= Matrix.RotationMatrix(v._x * v._w, v._y * v._w, v._z * v._w);
                    }
                    else if (_reader.Name.Equals("scale", true))
                        m.Scale(ParseVec3());
                    else if (_reader.Name.Equals("translate", true))
                        m.Translate(ParseVec3());
                    else if (_reader.Name.Equals("node", true))
                        node._children.Add(ParseNode());
                    else if (_reader.Name.Equals("instance_controller", true))
                        node._instances.Add(ParseInstance(InstanceType.Controller));
                    else if (_reader.Name.Equals("instance_geometry", true))
                        node._instances.Add(ParseInstance(InstanceType.Geometry));
                    else if (_reader.Name.Equals("instance_node", true))
                        node._instances.Add(ParseInstance(InstanceType.Node));

                    _reader.EndElement();
                }
                node._transform = (node._matrix = m).Derive();
                return node;
            }

            private InstanceEntry ParseInstance(InstanceType type)
            {
                InstanceEntry c = new InstanceEntry();
                c._type = type;

                while (_reader.ReadAttribute())
                    if (_reader.Name.Equals("url", true))
                        c._url = _reader.Value[0] == '#' ? (string)(_reader.Value + 1) : (string)_reader.Value;

                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("skeleton", true))
                        c.skeletons.Add(_reader.Value[0] == '#' ? (string)(_reader.Value + 1) : (string)_reader.Value);

                    if (_reader.Name.Equals("bind_material", true))
                        while (_reader.BeginElement())
                        {
                            if (_reader.Name.Equals("technique_common", true))
                                while (_reader.BeginElement())
                                {
                                    if (_reader.Name.Equals("instance_material", true))
                                        c._material = ParseMatInstance();
                                    _reader.EndElement();
                                }
                            _reader.EndElement();
                        }

                    _reader.EndElement();
                }

                return c;
            }

            private InstanceMaterial ParseMatInstance()
            {
                InstanceMaterial mat = new InstanceMaterial();

                while (_reader.ReadAttribute())
                    if (_reader.Name.Equals("symbol", true))
                        mat._symbol = (string)_reader.Value;
                    else if (_reader.Name.Equals("target", true))
                        mat._target = _reader.Value[0] == '#' ? (string)(_reader.Value + 1) : (string)_reader.Value;

                while (_reader.BeginElement())
                {
                    if (_reader.Name.Equals("bind_vertex_input", true))
                        mat._vertexBinds.Add(ParseVertexInput());
                    _reader.EndElement();
                }
                return mat;
            }
            private VertexBind ParseVertexInput()
            {
                VertexBind v = new VertexBind();

                while (_reader.ReadAttribute())
                    if (_reader.Name.Equals("semantic", true))
                        v._semantic = (string)_reader.Value;
                    else if (_reader.Name.Equals("input_semantic", true))
                        v._inputSemantic = (string)_reader.Value;
                    else if (_reader.Name.Equals("input_set", true))
                        v._inputSet = int.Parse((string)_reader.Value);

                return v;
            }

            private Matrix ParseMatrix()
            {
                Matrix m;
                float* pM = (float*)&m;
                for (int y = 0; y < 4; y++)
                    for (int x = 0; x < 4; x++)
                        _reader.ReadValue(&pM[x * 4 + y]);
                return m;
            }
            private RGBAPixel ParseColor()
            {
                float f;
                RGBAPixel c;
                byte* p = (byte*)&c;
                for (int i = 0; i < 4; i++)
                {
                    if (!_reader.ReadValue(&f))
                        p[i] = 255;
                    else
                        p[i] = (byte)(f * 255.0f + 0.5f);
                }
                return c;
            }
            private Vector3 ParseVec3()
            {
                float f;
                Vector3 c;
                float* p = (float*)&c;
                for (int i = 0; i < 3; i++)
                {
                    if (!_reader.ReadValue(&f))
                        p[i] = 0;
                    else
                        p[i] = f;
                }
                return c;
            }
            private Vector4 ParseVec4()
            {
                float f;
                Vector4 c;
                float* p = (float*)&c;
                for (int i = 0; i < 4; i++)
                {
                    if (!_reader.ReadValue(&f))
                        p[i] = 0;
                    else
                        p[i] = f;
                }
                return c;
            }
        }

        private void InitializeComponent()
        {
            this.Status = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // Status
            // 
            this.Status.AutoSize = true;
            this.Status.Location = new System.Drawing.Point(12, 9);
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(107, 13);
            this.Status.TabIndex = 0;
            this.Status.Text = "Parsing DAE model...";
            this.Status.UseWaitCursor = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(231, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(65, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "Okay";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(302, 6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(65, 23);
            this.button2.TabIndex = 10;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.propertyGrid1);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(379, 464);
            this.panel1.TabIndex = 11;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(379, 429);
            this.propertyGrid1.TabIndex = 11;
            this.propertyGrid1.ToolbarVisible = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.button2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 429);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(379, 35);
            this.panel2.TabIndex = 12;
            // 
            // Collada
            // 
            this.AcceptButton = this.button1;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(379, 464);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.Status);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.Name = "Collada";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Settings";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel; 
            Close();
        }
    }
}
