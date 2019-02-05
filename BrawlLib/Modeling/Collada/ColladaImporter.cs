﻿using System;
using System.Collections.Generic;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;
using System.Windows.Forms;
using BrawlLib.Wii.Models;
using BrawlLib.Imaging;
using BrawlLib.SSBBTypes;
using BrawlLib.Wii.Graphics;
using System.ComponentModel;

namespace BrawlLib.Modeling
{
    public unsafe partial class Collada : Form
    {
        public static string Error;
        public static IModel CurrentModel;
        public static ImportType ModelType;
        private static Type BoneType;
        private static ResourceNode TempRootBone;
        public static Matrix TransformMatrix = Matrix.Identity;
        
        public enum ImportType
        {
            MDL0, //Wii SDK
            //BMD, //GameCube SDK
            //LM, //Luigi's Mansion
            //FMDL, //Wii U SDK
            //NP3D, //Namco (Smash 4)
        }

        public IModel ImportModel(string filePath, ImportType type)
        {
            IModel model = null;
            ModelType = type;

            BoneType = ModelType == ImportType.MDL0 ? typeof(MDL0BoneNode) : null;

            //TransformMatrix = Matrix.TransformMatrix(_importOptions._modifyScale, _importOptions._modifyRotation, new Vector3());

            switch (type)
            {
                case ImportType.MDL0:
                    MDL0Node m = new MDL0Node()
                    {
                        _name = Path.GetFileNameWithoutExtension(filePath),
                        _version = _importOptions._modelVersion.Clamp(8, 11)
                    };
                    if (_importOptions._setOrigPath)
                        m._originalPath = filePath;
                    m.BeginImport();
                    model = m;
                    break;
            }

            CurrentModel = model;

            Error = "There was a problem reading the model.";
            using (DecoderShell shell = DecoderShell.Import(filePath))
                try
                {
                    Error = "There was a problem reading texture entries.";

                    //Extract images, removing duplicates
                    foreach (ImageEntry img in shell._images)
                    {
                        string name = img._path != null ? 
                            Path.GetFileNameWithoutExtension(img._path) :
                            img._name != null ? img._name : img._id;

                        switch (type)
                        {
                            case ImportType.MDL0:
                                img._node = ((MDL0Node)model).FindOrCreateTexture(name);
                                break;
                        }
                    }

                    Error = "There was a problem creating a default shader.";

                    //Create a shader
                    ResourceNode shader = null;
                    switch (type)
                    {
                        case ImportType.MDL0:
                            MDL0Node m = (MDL0Node)model;
                            MDL0ShaderNode shadNode = new MDL0ShaderNode()
                            {
                                _ref0 = 0,
                                _ref1 = -1,
                                _ref2 = -1,
                                _ref3 = -1,
                                _ref4 = -1,
                                _ref5 = -1,
                                _ref6 = -1,
                                _ref7 = -1,
                            };

                            shadNode._parent = m._shadGroup;
                            m._shadList.Add(shadNode);

                            switch (_importOptions._mdlType)
                            {
                                case ImportOptions.MDLType.Character:
                                    for (int i = 0; i < 3; i++)
                                    {
                                        switch (i)
                                        {
                                            case 0:
                                                shadNode.AddChild(new MDL0TEVStageNode(0x28F8AF, 0x08F2F0, 0, TevKColorSel.ConstantColor0_RGB, TevKAlphaSel.ConstantColor0_Alpha, TexMapID.TexMap0, TexCoordID.TexCoord0, ColorSelChan.LightChannel0, true));
                                                break;
                                            case 1:
                                                shadNode.AddChild(new MDL0TEVStageNode(0x08FEB0, 0x081FF0, 0, TevKColorSel.ConstantColor1_RGB, TevKAlphaSel.ConstantColor0_Alpha, TexMapID.TexMap7, TexCoordID.TexCoord7, ColorSelChan.LightChannel0, false));
                                                break;
                                            case 2:
                                                shadNode.AddChild(new MDL0TEVStageNode(0x0806EF, 0x081FF0, 0, TevKColorSel.ConstantColor0_RGB, TevKAlphaSel.ConstantColor0_Alpha, TexMapID.TexMap7, TexCoordID.TexCoord7, ColorSelChan.Zero, false));
                                                break;
                                        }
                                    }
                                    break;
                                case ImportOptions.MDLType.Stage:
                                    shadNode.AddChild(new MDL0TEVStageNode(0x28F8AF, 0x08F2F0, 0, TevKColorSel.ConstantColor0_RGB, TevKAlphaSel.ConstantColor0_Alpha, TexMapID.TexMap0, TexCoordID.TexCoord0, ColorSelChan.LightChannel0, true));
                                    break;
                            }

                            shader = shadNode;

                            break;
                    }

                    Error = "There was a problem extracting materials.";
                    
                    //Extract materials
                    foreach (MaterialEntry mat in shell._materials)
                    {
                        List<int> uwrap = new List<int>();
                        List<int> vwrap = new List<int>();
                        List<int> minfilter = new List<int>();
                        List<int> magfilter = new List<int>();
                        List<ImageEntry> imgEntries = new List<ImageEntry>();

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
                                                {
                                                    if (p._sid == l._texture)
                                                    {
                                                        path = p._sampler2D._url;
                                                        if (!String.IsNullOrEmpty(p._sampler2D._source))
                                                            foreach (EffectNewParam p2 in eff._newParams)
                                                                if (p2._sid == p._sampler2D._source)
                                                                    path = p2._path;
                                                        switch (p._sampler2D._wrapS)
                                                        {
                                                            case "CLAMP":
                                                                uwrap.Add(0);
                                                                break;
                                                            case "WRAP":
                                                                uwrap.Add(1);
                                                                break;
                                                            case "MIRROR":
                                                                uwrap.Add(2);
                                                                break;
                                                            default:
                                                                uwrap.Add((int)_importOptions.TextureWrap);   // Default to user-defined
                                                                break;
                                                        }
                                                        switch (p._sampler2D._wrapT)
                                                        {
                                                            case "CLAMP":
                                                                vwrap.Add(0);
                                                                break;
                                                            case "WRAP":
                                                                vwrap.Add(1);
                                                                break;
                                                            case "MIRROR":
                                                                vwrap.Add(2);
                                                                break;
                                                            default:
                                                                vwrap.Add((int)_importOptions.TextureWrap);   // Default to user-defined
                                                                break;
                                                        }
                                                        switch (p._sampler2D._minFilter)
                                                        {
                                                            case "NEAREST":
                                                                minfilter.Add(0);
                                                                break;
                                                            case "LINEAR":
                                                                minfilter.Add(1);
                                                                break;
                                                            case "NEAREST_MIPMAP_NEAREST":
                                                                minfilter.Add(2);
                                                                break;
                                                            case "LINEAR_MIPMAP_NEAREST":
                                                                minfilter.Add(3);
                                                                break;
                                                            case "NEAREST_MIPMAP_LINEAR":
                                                                minfilter.Add(4);
                                                                break;
                                                            case "LINEAR_MIPMAP_LINEAR":
                                                                minfilter.Add(5);
                                                                break;
                                                            default:
                                                                minfilter.Add((int)_importOptions.MinFilter);   // Default to user-defined
                                                                break;
                                                        }
                                                        switch (p._sampler2D._magFilter)
                                                        {
                                                            case "NEAREST":
                                                                magfilter.Add(0);
                                                                break;
                                                            case "LINEAR":
                                                                magfilter.Add(1);
                                                                break;
                                                            case "NEAREST_MIPMAP_NEAREST":
                                                                magfilter.Add(0);
                                                                //magfilter.Add(2);   // Unused. Use Nearest instead.
                                                                break;
                                                            case "LINEAR_MIPMAP_NEAREST":
                                                                magfilter.Add(1);
                                                                //magfilter.Add(3);   // Unused. Use Linear instead.
                                                                break;
                                                            case "NEAREST_MIPMAP_LINEAR":
                                                                magfilter.Add(0);
                                                                //magfilter.Add(4);   // Unused. Use Nearest instead.
                                                                break;
                                                            case "LINEAR_MIPMAP_LINEAR":
                                                                magfilter.Add(1);
                                                                //magfilter.Add(5);   // Unused. Use Linear instead.
                                                                break;
                                                            default:
                                                                magfilter.Add((int)_importOptions.MagFilter);   // Default to user-defined
                                                                break;
                                                        }
                                                    }
                                                }
                                                foreach (ImageEntry img in shell._images)
                                                    if (img._id == path)
                                                    {
                                                        imgEntries.Add(img);
                                                        break;
                                                    }
                                            }
                        switch (type)
                        {
                            case ImportType.MDL0:
                                MDL0MaterialNode matNode = new MDL0MaterialNode();

                                MDL0Node m = (MDL0Node)model;
                                matNode._parent = m._matGroup;
                                m._matList.Add(matNode);

                                matNode._name = mat._name != null ? mat._name : mat._id;
                                matNode.ShaderNode = shader as MDL0ShaderNode;

                                mat._node = matNode;
                                matNode._cull = _importOptions._culling;

                                int i = 0;
                                foreach (ImageEntry img in imgEntries)
                                {
                                    MDL0MaterialRefNode mr = new MDL0MaterialRefNode();
                                    (mr._texture = img._node as MDL0TextureNode)._references.Add(mr);
                                    mr._name = mr._texture.Name;
                                    matNode._children.Add(mr);
                                    mr._parent = matNode;
                                    mr._minFltr = minfilter.Count > i ? minfilter[i] : (int)_importOptions.MinFilter;
                                    mr._magFltr = magfilter.Count > i ? magfilter[i] : (int)_importOptions.MagFilter;
                                    mr._uWrap = uwrap.Count > i ? uwrap[i] : (int)_importOptions.TextureWrap;
                                    mr._vWrap = vwrap.Count > i ? vwrap[i] : (int)_importOptions.TextureWrap;
                                    i++;
                                }
                                break;
                        }
                    }

                    Say("Extracting scenes...");

                    List<ObjectInfo> _objects = new List<ObjectInfo>();
                    ResourceNode boneGroup = null;
                    switch (type)
                    {
                        case ImportType.MDL0:
                            boneGroup = ((MDL0Node)model)._boneGroup;
                            break;
                    }

                    //Extract bones and objects and create bone tree
                    foreach (SceneEntry scene in shell._scenes)
                        foreach (NodeEntry node in scene._nodes)
                            EnumNode(node, boneGroup, scene, model, shell, _objects, TransformMatrix, Matrix.Identity);

                    //Add root bone if there are no bones
                    if (boneGroup.Children.Count == 0)
                        switch (type)
                        {
                            case ImportType.MDL0:
                                MDL0BoneNode bone = new MDL0BoneNode();
                                bone.Scale = new Vector3(1);
                                bone.RecalcBindState(false, false);
                                bone._name = "TopN";
                                TempRootBone = bone;
                                break;
                        }

                    //Create objects
                    foreach (ObjectInfo obj in _objects)
                    {
                        NodeEntry node = obj._node;
                        string w = obj._weighted ? "" : "un";
                        string w2 = obj._weighted ? "\nOne or more vertices may not be weighted correctly." : "";
                        string n = node._name != null ? node._name : node._id;

                        Error = String.Format("There was a problem decoding {0}weighted primitives for the object {1}.{2}", w, n, w2);

                        Say(String.Format("Decoding {0}weighted primitives for {1}...", w, n));

                        obj.Initialize(model, shell);
                    }

                    //Finish
                    switch (type)
                    {
                        case ImportType.MDL0:
                            MDL0Node mdl0 = (MDL0Node)model;
                            if (TempRootBone != null)
                            {
                                mdl0._boneGroup._children.Add(TempRootBone);
                                TempRootBone._parent = mdl0._boneGroup;
                            }
                            FinishMDL0(mdl0);
                            break;
                    }
                }
    #if !DEBUG
                catch (Exception x)
                {
                    MessageBox.Show("Cannot continue importing this model.\n" + Error + "\n\nException:\n" + x.ToString());
                    model = null;
                    Close();
                }
    #endif
                finally
                {
                    //Clean up the mess we've made
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                }

            CurrentModel = null;
            Error = null;

            return model;
        }

        private void EnumNode(
            NodeEntry node, 
            ResourceNode parent,
            SceneEntry scene,
            IModel model,
            DecoderShell shell,
            List<ObjectInfo> objects,
            Matrix bindMatrix,
            Matrix parentInvMatrix)
        {
            bindMatrix *= node._matrix;

            if (node._type == NodeType.JOINT || 
                (node._type == NodeType.NONE && node._instances.Count == 0))
            {
                Error = "There was a problem creating a new bone.";

                Influence inf = null;

                switch (ModelType)
                {
                    case ImportType.MDL0:
                        MDL0BoneNode bone = new MDL0BoneNode();
                        bone._name = node._name != null ? node._name : node._id;

                        bone._bindState = node._matrix.Derive();
                        node._node = bone;

                        parent._children.Add(bone);
                        bone._parent = parent;

                        bone.RecalcBindState(false, false);
                        bone.CalcFlags();

                        parent = bone;

                        inf = new Influence(bone);
                        break;
                }

                if (inf != null)
                    model.Influences._influences.Add(inf);
            }

            //parentInvMatrix *= node._matrix.Invert();
            foreach (NodeEntry e in node._children)
                EnumNode(e, parent, scene, model, shell, objects, bindMatrix, parentInvMatrix);
            
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
                                    objects.Add(new ObjectInfo(true, g, bindMatrix, skin, scene, inst, parent, node));
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
                            objects.Add(new ObjectInfo(false, g, bindMatrix, null, null, inst, parent, node));
                            break;
                        }
                }
                else
                    foreach (NodeEntry e in shell._nodes)
                        if (e._id == inst._url)
                            EnumNode(e, parent, scene, model, shell, objects, bindMatrix, parentInvMatrix);
            }
        }

        private class ObjectInfo
        {
            public bool _weighted;
            public GeometryEntry _g;
            public Matrix _bindMatrix;
            public SkinEntry _skin;
            public InstanceEntry _inst;
            public SceneEntry _scene;
            public ResourceNode _parent;
            public NodeEntry _node;

            public ObjectInfo(
                bool weighted,
                GeometryEntry g,
                Matrix bindMatrix,
                SkinEntry skin,
                SceneEntry scene,
                InstanceEntry inst,
                ResourceNode parent,
                NodeEntry node)
            {
                _weighted = weighted;
                _g = g;
                _bindMatrix = bindMatrix;
                _skin = skin;
                _scene = scene;
                _parent = parent;
                _node = node;
                _inst = inst;
            }

            public void Initialize(IModel model, DecoderShell shell)
            {
                PrimitiveManager m;
                if (_weighted)
                    m = DecodePrimitivesWeighted(_bindMatrix, _g, _skin, _scene, model.Influences, BoneType);
                else
                    m = DecodePrimitivesUnweighted(_bindMatrix, _g);

                switch (ModelType)
                {
                    case ImportType.MDL0:
                        CreateMDL0Object(_inst, _node, _parent, m, (MDL0Node)model, shell);
                        break;
                }
            }
        }

        private static void CreateMDL0Object(
            InstanceEntry inst,
            NodeEntry node,
            ResourceNode parent,
            PrimitiveManager manager,
            MDL0Node model,
            DecoderShell shell)
        {
            if (manager != null)
            {
                Error = "There was a problem creating a new object for " + (node._name != null ? node._name : node._id);

                MDL0ObjectNode poly = new MDL0ObjectNode()
                {
                    _manager = manager,
                    _name = node._name != null ? node._name : node._id,
                    _drawCalls = new BindingList<DrawCall>()
                };

                //Attach material
                if (inst._material != null)
                    foreach (MaterialEntry mat in shell._materials)
                        if (mat._id == inst._material._target)
                            poly._drawCalls.Add(new DrawCall(poly) { MaterialNode = mat._node as MDL0MaterialNode });

                model._numTriangles += poly._numFaces = manager._faceCount = manager._pointCount / 3;
                model._numFacepoints += poly._numFacepoints = manager._pointCount;

                poly._parent = model._objGroup;
                model._objList.Add(poly);

                model.ResetToBindState();

                //Attach single-bind
                if (parent != null && parent is MDL0BoneNode)
                {
                    MDL0BoneNode bone = (MDL0BoneNode)parent;
                    poly.DeferUpdateAssets();
                    poly.MatrixNode = bone;

                    foreach (DrawCall c in poly._drawCalls)
                        c.VisibilityBoneNode = bone;
                }
                else if (model._boneList.Count == 0)
                {
                    Error = String.Format("There was a problem rigging {0} to a single bone.", poly._name);

                    Box box = poly.GetBox();
                    MDL0BoneNode bone = new MDL0BoneNode()
                    {
                        Scale = Vector3.One,
                        Translation = (box.Max + box.Min) / 2.0f,
                        _name = "TransN_" + poly.Name,
                        Parent = TempRootBone,
                    };

                    poly.DeferUpdateAssets();
                    poly.MatrixNode = bone;
                    ((MDL0BoneNode)TempRootBone).RecalcBindState(true, false, false);

                    foreach (DrawCall c in poly._drawCalls)
                        c.VisibilityBoneNode = bone;
                }
                else
                {
                    Error = String.Format("There was a problem checking if {0} is rigged to a single bone.", poly._name);

                    foreach (DrawCall c in poly._drawCalls)
                        c.VisibilityBoneNode = model._boneList[0] as MDL0BoneNode;

                    IMatrixNode mtxNode = null;
                    bool singlebind = true;

                    foreach (Vertex3 v in poly._manager._vertices)
                        if (v.MatrixNode != null)
                        {
                            if (mtxNode == null)
                                mtxNode = v.MatrixNode;

                            if (v.MatrixNode != mtxNode)
                            {
                                singlebind = false;
                                break;
                            }
                        }

                    if (singlebind && poly._matrixNode == null)
                    {
                        //Reassign reference entries
                        if (poly._manager._vertices[0].MatrixNode != null)
                            poly._manager._vertices[0].MatrixNode.Users.Add(poly);

                        foreach (Vertex3 v in poly._manager._vertices)
                            if (v.MatrixNode != null)
                                v.MatrixNode.Users.Remove(v);

                        poly._nodeId = -2; //Continued on polygon rebuild
                    }
                }
            }
        }

        private void FinishMDL0(MDL0Node model)
        {
            Error = "There was a problem creating a default material and shader.";
            if (model._matList.Count == 0 && model._objList.Count != 0)
            {
                MDL0MaterialNode mat = new MDL0MaterialNode() { _name = "Default", };
                (mat.ShaderNode = new MDL0ShaderNode()).AddChild(new MDL0TEVStageNode()
                {
                    RasterColor = ColorSelChan.LightChannel0,
                    AlphaSelectionD = AlphaArg.RasterAlpha,
                    ColorSelectionD = ColorArg.RasterColor,
                });

                model._shadGroup.AddChild(mat.ShaderNode);
                model._matGroup.AddChild(mat);

                foreach (MDL0ObjectNode obj in model._objList)
                {
                    if (obj._drawCalls.Count == 0)
                        obj._drawCalls.Add(new DrawCall(obj));
                    
                    obj._drawCalls[0].MaterialNode = mat;
                }
            }

            Error = "There was a problem removing original color buffers.";

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

            Error = "There was a problem adding default color values.";

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

            Error = "There was a problem initializing materials.";

            //Apply defaults to materials
            if (model._matList != null)
                foreach (MDL0MaterialNode mat in model._matList)
                {
                    mat._activeStages = mat.ShaderNode.Stages;
                    if (_importOptions._mdlType == ImportOptions.MDLType.Stage)
                    {
                        mat._lightSetIndex = 0;
                        mat._fogIndex = 0;
                    }
                }

            Error = "There was a problem remapping materials.";

            //Remap materials if option set
            if (_importOptions._rmpMats && model._matList != null && model._objList != null)
            {
                foreach (MDL0ObjectNode obj3 in model._objList)
                {
                    if (obj3.DrawCalls.Count == 0)
                        continue;

                    MDL0MaterialNode mat = obj3._drawCalls[0].MaterialNode;
                    foreach (MDL0MaterialNode m in model._matList)
                        if (m.Children.Count > 0 &&
                            m.Children[0] != null &&
                            mat != null &&
                            mat.Children.Count > 0 &&
                            mat.Children[0] != null &&
                            m.Children[0].Name == mat.Children[0].Name &&
                            m.C1ColorMaterialSource == mat.C1ColorMaterialSource)
                        {
                            obj3._drawCalls[0].MaterialNode = m;
                            break;
                        }
                }

                //Remove unused materials
                for (int i = 0; i < model._matList.Count; i++)
                    if (((MDL0MaterialNode)model._matList[i])._objects.Count == 0)
                        model._matList.RemoveAt(i--);
            }

            Error = "There was a problem writing the model.";

            //Clean the model and then build it!
            if (model != null)
                model.FinishImport();
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
            //[Category("Model"), TypeConverter(typeof(Vector3StringConverter)), Description("Rotates the entire model before importing. This can be used to fix a model's up axis, as BrawlCrate uses Y-up while some other 3D programs use Z-up.")]
            //public Vector3 ModifyRotation { get { return _modifyRotation; } set { _modifyRotation = value; } }
            //[Category("Model"), TypeConverter(typeof(Vector3StringConverter)), Description("Scales the entire model before importing. This can be used to fix a model's units, as BrawlCrate uses centimeters while other 3D programs uses units such as meters or inches.")]
            //public Vector3 ModifyScale { get { return _modifyScale; } set { _modifyScale = value; } }
            
            [Category("Materials"), Description("The default texture wrap for material texture references.")]
            public MatWrapMode TextureWrap { get { return _wrap; } set { _wrap = value; } }
            [Category("Materials"), Description("The default min filter for material texture references.")]
            public MatTextureMinFilter MinFilter { get { return _minFilter; } set { _minFilter = value; } }
            [Category("Materials"), Description("The default magnification filter for material texture references.")]
            public MatTextureMagFilter MagFilter { get { return _magFilter; } set { _magFilter = value; } }
            [Category("Materials"), Description("If true, materials will be remapped. This means there will be no redundant materials with the same settings, saving file space.")]
            public bool RemapMaterials { get { return _rmpMats; } set { _rmpMats = value; } }
            [Category("Materials"), Description("The default setting to use for material culling. Culling determines what side of the mesh is invisible.")]
            public CullMode MaterialCulling { get { return _culling; } set { _culling = value; } }

            [Category("Assets"), Description("If true, vertex arrays will be written in float format. This means that the data size will be larger, but more precise. Float arrays for vertices must be used if any object is rigged to multiple bones (no single bind) or animated by an SHP0 animation.")]
            public bool ForceFloatVertices { get { return _fltVerts; } set { _fltVerts = value; } }
            [Category("Assets"), Description("If true, normal arrays will be written in float format. This means that the data size will be larger, but more precise. Float arrays for normals must be used if any object is rigged to multiple bones (no single bind) or animated by an SHP0 animation.")]
            public bool ForceFloatNormals { get { return _fltNrms; } set { _fltNrms = value; } }
            [Category("Assets"), Description("If true, texture coordinate arrays will be written in float format. This means that the data size will be larger, but more precise. Float arrays for texture coordinates must be used if any object uses at least one texture matrix.")]
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
            [Category("Color Nodes"), Description("This will make all colors be written in one color node. This will save file space for models with lots of different colors.")]
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

            //public Vector3 _modifyRotation;
            //public Vector3 _modifyScale = new Vector3(1.0f);
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
            public RGBAPixel _dfltClr = new RGBAPixel(128, 128, 128, 255);
            public uint _cacheSize = 52;
            public uint _minStripLen = 2;
            public bool _pushCacheHits = true;
            public bool _useTristrips = true;
            public bool _setOrigPath = false;
            public float _weightPrecision = 0.0001f;
            public int _modelVersion = 9;
            public bool _useOneNode = true;
            public MatWrapMode _wrap = MatWrapMode.Repeat;
            public MatTextureMinFilter _minFilter = MatTextureMinFilter.Linear;
            public MatTextureMagFilter _magFilter = MatTextureMagFilter.Linear;

            //This doesn't work, but it's optional and not efficient with the cache on anyway
            public bool _backwardSearch = false;

            internal RGBAPixel[] _singleColorNodeEntries;
        }
    }
}
