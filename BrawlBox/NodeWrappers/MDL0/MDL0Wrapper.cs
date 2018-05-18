using System;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using System.ComponentModel;
using BrawlLib;
using BrawlLib.Imaging;
using BrawlLib.Wii.Models;
using BrawlLib.SSBB;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.MDL0)]
    class MDL0Wrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static MDL0Wrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("&Preview", null, PreviewAction, Keys.Control | Keys.P));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Export", null, ExportAction, Keys.Control | Keys.E));
            _menu.Items.Add(new ToolStripMenuItem("&Replace", null, ReplaceAction, Keys.Control | Keys.R));
            _menu.Items.Add(new ToolStripMenuItem("Res&tore", null, RestoreAction, Keys.Control | Keys.T));
            _menu.Items.Add(new ToolStripMenuItem("&Duplicate", null, DuplicateAction, Keys.Control | Keys.D));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUpAction, Keys.Control | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Move D&own", null, MoveDownAction, Keys.Control | Keys.Down));
            _menu.Items.Add(new ToolStripMenuItem("Re&name", null, RenameAction, Keys.Control | Keys.N));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Add New S&hader", null, NewShaderAction, Keys.Control | Keys.H));
            _menu.Items.Add(new ToolStripMenuItem("Add New &Material", null, NewMaterialAction, Keys.Control | Keys.M));
            _menu.Items.Add(new ToolStripMenuItem("Ne&w Asset", null,
                new ToolStripMenuItem("Vertices", null, NewVertexAction),
                new ToolStripMenuItem("Normals", null, NewNormalAction),
                new ToolStripMenuItem("Colors", null, NewColorAction),
                new ToolStripMenuItem("UVs", null, NewUVAction)
                ));
            _menu.Items.Add(new ToolStripMenuItem("&Import Asset", null,
                new ToolStripMenuItem("Vertices", null, ImportVertexAction),
                new ToolStripMenuItem("Normals", null, ImportNormalAction),
                new ToolStripMenuItem("Colors", null, ImportColorAction),
                new ToolStripMenuItem("UVs", null, ImportUVAction)
                ));
            _menu.Items.Add(new ToolStripMenuItem("&Auto Name Assets", null,
                new ToolStripMenuItem("Vertices", null, NameVertexAction),
                new ToolStripMenuItem("Normals", null, NameNormalAction),
                new ToolStripMenuItem("Colors", null, NameColorAction),
                new ToolStripMenuItem("UVs", null, NameUVAction),
                new ToolStripMenuItem("Objects", null, NameObjectAction)
                ));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Mirror Model", null,
                new ToolStripMenuItem("X-Axis (Scale)", null, MirrorXAction),
                new ToolStripMenuItem("Y-Axis (Scale)", null, MirrorYAction),
                new ToolStripMenuItem("Z-Axis (Scale)", null, MirrorZAction),
                new ToolStripMenuItem("X-Axis (Translation/Z-Rotation)", null, FlipXAction),
                new ToolStripMenuItem("Y-Axis (Translation/Z-Rotation)", null, FlipYAction)
                //new ToolStripMenuItem("Z-Axis (Translation/Rotation)", null, FlipZAction)
                ));
            _menu.Items.Add(new ToolStripMenuItem("&Edit all Materials", null,
                new ToolStripMenuItem("Invert Materials", null, InvertMaterialsAction),
                new ToolStripMenuItem("Set all (Cull None)", null, CullNoneAction),
                new ToolStripMenuItem("Set all (Cull Outside)", null, CullOutsideAction),
                new ToolStripMenuItem("Set all (Cull Inside)", null, CullInsideAction),
                new ToolStripMenuItem("Set all (Cull All)", null, CullAllAction)
                ));
            _menu.Items.Add(new ToolStripMenuItem("&Reimport Meshes", null, ReimportAction));
            _menu.Items.Add(new ToolStripMenuItem("&Import Existing Object", null, ImportObjectAction));
            _menu.Items.Add(new ToolStripMenuItem("&Optimize Meshes", null, OptimizeAction));
            _menu.Items.Add(new ToolStripMenuItem("&Recalculate Bounding Boxes", null, RecalcBBsOption));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Fix Transparency With Characters", null, TransparencyFixAction));
            _menu.Items.Add(new ToolStripMenuItem("&Convert To Shadow Model", null, ShadowConvertAction));
            _menu.Items.Add(new ToolStripMenuItem("&Regenerate Metal Materials", null, MetalAction));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteAction, Keys.Control | Keys.Delete));
            _menu.Opening += MenuOpening;
            _menu.Closing += MenuClosing;
        }

        // StageBox model mirroring
        protected static void MirrorXAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().MirrorX(); }
        protected static void MirrorYAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().MirrorY(); }
        protected static void MirrorZAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().MirrorZ(); }
        protected static void FlipXAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().FlipX(true); }
        protected static void FlipYAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().FlipY(true); }
        //protected static void FlipZAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().FlipZ(true); }

        // StageBox Material settings
        protected static void InvertMaterialsAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().InvertMaterials(); }
        protected static void CullNoneAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().CullMaterials(0); }
        protected static void CullOutsideAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().CullMaterials(1); }
        protected static void CullInsideAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().CullMaterials(2); }
        protected static void CullAllAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().CullMaterials(3); }
        protected static void ShadowConvertAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().ShadowConvert(); }

        protected static void TransparencyFixAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().StartTransparencyFix(); }

        private static void ReimportAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().ReimportMeshes(); }
        private static void OptimizeAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().Optimize(); }
        protected static void PreviewAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().Preview(); }
        protected static void ImportObjectAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().ImportObject(); }
        protected static void RecalcBBsOption(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().RecalcBoundingBoxes(); }
        protected static void MetalAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().AutoMetal(); }
        
        protected static void NewShaderAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().NewShader(); }
        protected static void NewMaterialAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().NewMaterial(); }

        protected static void NewVertexAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().NewVertex(); }
        protected static void NewNormalAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().NewNormal(); }
        protected static void NewColorAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().NewColor(); }
        protected static void NewUVAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().NewUV(); }

        protected static void NameVertexAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().NameVertex(); }
        protected static void NameNormalAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().NameNormal(); }
        protected static void NameColorAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().NameColor(); }
        protected static void NameUVAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().NameUV(); }
        protected static void NameObjectAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().NameObject(); }

        protected static void ImportVertexAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().ImportVertex(); }
        protected static void ImportNormalAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().ImportNormal(); }
        protected static void ImportColorAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().ImportColor(); }
        protected static void ImportUVAction(object sender, EventArgs e) { GetInstance<MDL0Wrapper>().ImportUV(); }
        
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[3].Enabled = _menu.Items[4].Enabled = _menu.Items[5].Enabled = _menu.Items[7].Enabled = _menu.Items[8].Enabled = _menu.Items[11].Enabled = _menu.Items[28].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            MDL0Wrapper w = GetInstance<MDL0Wrapper>();
            _menu.Items[3].Enabled = _menu.Items[5].Enabled = _menu.Items[28].Enabled = w.Parent != null;
            _menu.Items[4].Enabled = ((w._resource.IsDirty) || (w._resource.IsBranch));
            _menu.Items[7].Enabled = w.PrevNode != null;
            _menu.Items[8].Enabled = w.NextNode != null;
            if (((MDL0Node)w._resource)._shadList != null && ((MDL0Node)w._resource)._matList != null)
                _menu.Items[11].Enabled = (((MDL0Node)w._resource)._shadList.Count < ((MDL0Node)w._resource)._matList.Count);
            else
                _menu.Items[11].Enabled = false;
        }
        #endregion

        public override string ExportFilter { get { return FileFilters.MDL0Export; } }
        public override string ImportFilter { get { return FileFilters.MDL0Import; } }

        public MDL0Wrapper() { ContextMenuStrip = _menu; }

        public void StartTransparencyFix()
        {
            if (MessageBox.Show(null, "This option will fix a transparent model to render correctly relative to characters.\nThis merely renames the model. Proper transparency must be set up for this to work fully.\nThis option will fail if the rename would cause a model name to be more than 255 characters.", "", MessageBoxButtons.OKCancel) == DialogResult.OK)
                ((MDL0Node)_resource).GR2Fix();
            _resource.Rebuild(true);
        }

        public void ShadowConvert()
        {
            ((MDL0Node)_resource).ConvertToShadowModel();
        }

        public void InvertMaterials()
        {
            ((MDL0Node)_resource).FlipAllMaterials();
        }

        public void CullMaterials(int cullMode)
        {
            ((MDL0Node)_resource).SetAllMaterialCulling(cullMode);
        }

        public void MirrorX()
        {
            MDL0Node model = ((MDL0Node)_resource);
            
            if(model.FindBoneByIndex(0) != null) {
                float newScale = 0 - model.FindBoneByIndex(0).getManualScale('X');
                // Console.WriteLine("NEW SCALE: " + newScale);
                model.FindBoneByIndex(0).setManualScale('X', newScale);
                model.FlipAllMaterials();
            }
        }
        
        public void MirrorY()
        {
            MDL0Node model = ((MDL0Node)_resource);

            if (model.FindBoneByIndex(0) != null)
            {
                float newScale = 0 - model.FindBoneByIndex(0).getManualScale('Y');
                // Console.WriteLine("NEW SCALE: " + newScale);
                model.FindBoneByIndex(0).setManualScale('Y', newScale);
                model.FlipAllMaterials();
            }
        }

        public void MirrorZ()
        {
            MDL0Node model = ((MDL0Node)_resource);

            if (model.FindBoneByIndex(0) != null)
            {
                float newScale = 0 - model.FindBoneByIndex(0).getManualScale('Z');
                // Console.WriteLine("NEW SCALE: " + newScale);
                model.FindBoneByIndex(0).setManualScale('Z', newScale);
                model.FlipAllMaterials();
            }
        }

        public void FlipX(bool allowBoundaryFix)
        {
            MDL0Node model = ((MDL0Node)_resource);
            bool boundaryFixActive = false;
            if (allowBoundaryFix)
            {
                if (model.Name == "StgPosition" || model.Name == "Stgposition" || model.Name == "stgPosition" || model.Name == "stgposition" || model.Name == "StagePosition" || model.Name == "Stageposition" || model.Name == "stagePosition" || model.Name == "stageposition")
                {
                    boundaryFixActive = true;
                }
                else if (model.Name.Length == 13)
                {
                    //Console.WriteLine(model + " was found to be 13 characters long");
                    if (model.Name.Substring(0, 11) == "PokeTrainer" || model.Name.Substring(0, 11) == "Poketrainer" || model.Name.Substring(0, 11) == "pokeTrainer" || model.Name.Substring(0, 11) == "poketrainer")
                    {
                        //Console.WriteLine("It's a pokemon trainer");
                        boundaryFixActive = true;
                    }
                }
            }

            if (model.FindBoneByIndex(0) != null)
            {
                MDL0BoneNode b = model.FindBoneByIndex(0);
                MDL0BoneNode b2 = null;
                int j = 0;
                float camPositionSaverX = 0;
                float deathPositionSaverX = 0;
                bool camPositionSaved = false;
                bool deathPositionSaved = false;
                bool camPositionSet = false;
                bool deathPositionSet = false;

                bool isRestrictedName = false;

                while (b != null)
                {
                    j++;
                    if (boundaryFixActive)
                    {
                        //
                        if (b.Name == "CamLimit0N" && !camPositionSet)
                        {
                            isRestrictedName = true;
                            if (!camPositionSaved)
                            {
                                camPositionSaverX = b.getManualTranslation('X');
                                camPositionSaved = true;
                            }
                            else
                            {
                                b2 = model.FindBone("CamLimit1N");
                                if (b2 != null)
                                {
                                    float tempCam0 = b.getManualTranslation('X');
                                    b.setManualTranslation('X', 0 - camPositionSaverX);
                                    b2.setManualTranslation('X', 0 - tempCam0);
                                    camPositionSet = true;
                                }
                            }
                        }
                        else if (b.Name == "CamLimit1N" && !camPositionSet)
                        {
                            isRestrictedName = true;
                            if (!camPositionSaved)
                            {
                                camPositionSaverX = b.getManualTranslation('X');
                                camPositionSaved = true;
                            }
                            else
                            {
                                b2 = model.FindBone("CamLimit0N");
                                if (b2 != null)
                                {
                                    float tempCam1 = b.getManualTranslation('X');
                                    b.setManualTranslation('X', 0 - camPositionSaverX);
                                    b2.setManualTranslation('X', 0 - tempCam1);
                                    camPositionSet = true;
                                }
                            }
                        }
                        else if (b.Name == "Dead0N" && !deathPositionSet)
                        {
                            isRestrictedName = true;
                            if (!deathPositionSaved)
                            {
                                deathPositionSaverX = b.getManualTranslation('X');
                                deathPositionSaved = true;
                            }
                            else
                            {
                                b2 = model.FindBone("Dead1N");
                                if (b2 != null)
                                {
                                    float tempDead0 = b.getManualTranslation('X');
                                    b.setManualTranslation('X', 0 - deathPositionSaverX);
                                    b2.setManualTranslation('X', 0 - tempDead0);
                                    deathPositionSet = true;
                                }
                            }
                        }
                        else if (b.Name == "Dead1N" && !deathPositionSet)
                        {
                            isRestrictedName = true;
                            if (!deathPositionSaved)
                            {
                                deathPositionSaverX = b.getManualTranslation('X');
                                deathPositionSaved = true;
                            }
                            else
                            {
                                b2 = model.FindBone("Dead0N");
                                if (b2 != null)
                                {
                                    float tempDead1 = b.getManualTranslation('X');
                                    b.setManualTranslation('X', 0 - deathPositionSaverX);
                                    b2.setManualTranslation('X', 0 - tempDead1);
                                    deathPositionSet = true;
                                }
                            }
                        }
                        else if (b.Name.EndsWith("E"))
                        {
                            //Console.WriteLine("Found E Bone: " + b);
                            string b2Finder = b.Name.Remove(b.Name.Length - 1, 1) + "N";
                            //Console.WriteLine("Searching for " + b2Finder);
                            b2 = model.FindBone(b2Finder);
                            if (b2 != null)
                            {
                                //Console.WriteLine("Found! " + b2);
                                float tempE = b.getManualTranslation('X');
                                float tempN = b2.getManualTranslation('X');
                                b.setManualTranslation('X', 0 - tempN);
                                b2.setManualTranslation('X', 0 - tempE);
                                isRestrictedName = true;
                            }
                            else
                            {
                                isRestrictedName = false;
                            }
                        }
                        else if (b.Name.EndsWith("N"))
                        {
                            //Console.WriteLine("Found N Bone: " + b);
                            string b2Finder = b.Name.Remove(b.Name.Length - 1, 1) + "E";
                            //Console.WriteLine("Searching for " + b2Finder);
                            b2 = model.FindBone(b2Finder);
                            if (b2 != null)
                            {
                                //Console.WriteLine("Found! Will not regenerate as was set by " + b2);
                                isRestrictedName = true;
                            }
                            else
                            {
                                isRestrictedName = false;
                            }
                        }
                    }

                    if (!boundaryFixActive || !isRestrictedName)
                    {
                        b.setManualTranslation('X', 0 - b.getManualTranslation('X'));
                        b.setManualRotation('Z', 0 - b.getManualRotation('Z'));
                    }
                    b = model.FindBoneByIndex(j);
                    isRestrictedName = false;
                }
            }
        }

        public void FlipY(bool allowBoundaryFix)
        {
            MDL0Node model = ((MDL0Node)_resource);
            bool boundaryFixActive = false;
            if (allowBoundaryFix)
            {
                if (model.Name == "StgPosition" || model.Name == "Stgposition" || model.Name == "stgPosition" || model.Name == "stgposition" || model.Name == "StagePosition" || model.Name == "Stageposition" || model.Name == "stagePosition" || model.Name == "stageposition")
                {
                    boundaryFixActive = true;
                }
                else if (model.Name.Length == 13)
                {
                    //Console.WriteLine(model + " was found to be 13 characters long");
                    if (model.Name.Substring(0, 11) == "PokeTrainer" || model.Name.Substring(0, 11) == "Poketrainer" || model.Name.Substring(0, 11) == "pokeTrainer" || model.Name.Substring(0, 11) == "poketrainer")
                    {
                        //Console.WriteLine("It's a pokemon trainer");
                        boundaryFixActive = true;
                    }
                }
            }

            if (model.FindBoneByIndex(0) != null)
            {
                MDL0BoneNode b = model.FindBoneByIndex(0);
                MDL0BoneNode b2 = null;
                int j = 0;
                float camPositionSaverY = 0;
                float deathPositionSaverY = 0;
                bool camPositionSaved = false;
                bool deathPositionSaved = false;
                bool camPositionSet = false;
                bool deathPositionSet = false;

                bool isRestrictedName = false;

                while (b != null)
                {
                    j++;
                    if (boundaryFixActive)
                    {
                        //
                        if (b.Name == "CamLimit0N" && !camPositionSet)
                        {
                            isRestrictedName = true;
                            if (!camPositionSaved)
                            {
                                camPositionSaverY = b.getManualTranslation('Y');
                                camPositionSaved = true;
                            }
                            else
                            {
                                b2 = model.FindBone("CamLimit1N");
                                if (b2 != null)
                                {
                                    float tempCam0 = b.getManualTranslation('Y');
                                    b.setManualTranslation('Y', 0 - camPositionSaverY);
                                    b2.setManualTranslation('Y', 0 - tempCam0);
                                    camPositionSet = true;
                                }
                            }
                        }
                        else if (b.Name == "CamLimit1N" && !camPositionSet)
                        {
                            isRestrictedName = true;
                            if (!camPositionSaved)
                            {
                                camPositionSaverY = b.getManualTranslation('Y');
                                camPositionSaved = true;
                            }
                            else
                            {
                                b2 = model.FindBone("CamLimit0N");
                                if (b2 != null)
                                {
                                    float tempCam1 = b.getManualTranslation('Y');
                                    b.setManualTranslation('Y', 0 - camPositionSaverY);
                                    b2.setManualTranslation('Y', 0 - tempCam1);
                                    camPositionSet = true;
                                }
                            }
                        }
                        else if (b.Name == "Dead0N" && !deathPositionSet)
                        {
                            isRestrictedName = true;
                            if (!deathPositionSaved)
                            {
                                deathPositionSaverY = b.getManualTranslation('Y');
                                deathPositionSaved = true;
                            }
                            else
                            {
                                b2 = model.FindBone("Dead1N");
                                if (b2 != null)
                                {
                                    float tempDead0 = b.getManualTranslation('Y');
                                    b.setManualTranslation('Y', 0 - deathPositionSaverY);
                                    b2.setManualTranslation('Y', 0 - tempDead0);
                                    deathPositionSet = true;
                                }
                            }
                        }
                        else if (b.Name == "Dead1N" && !deathPositionSet)
                        {
                            isRestrictedName = true;
                            if (!deathPositionSaved)
                            {
                                deathPositionSaverY = b.getManualTranslation('Y');
                                deathPositionSaved = true;
                            }
                            else
                            {
                                b2 = model.FindBone("Dead0N");
                                if (b2 != null)
                                {
                                    float tempDead1 = b.getManualTranslation('Y');
                                    b.setManualTranslation('Y', 0 - deathPositionSaverY);
                                    b2.setManualTranslation('Y', 0 - tempDead1);
                                    deathPositionSet = true;
                                }
                            }
                        }
                        else if (b.Name.EndsWith("E"))
                        {
                            //Console.WriteLine("Found E Bone: " + b);
                            string b2Finder = b.Name.Remove(b.Name.Length - 1, 1) + "N";
                            //Console.WriteLine("Searching for " + b2Finder);
                            b2 = model.FindBone(b2Finder);
                            if (b2 != null)
                            {
                                //Console.WriteLine("Found! " + b2);
                                float tempE = b.getManualTranslation('Y');
                                float tempN = b2.getManualTranslation('Y');
                                b.setManualTranslation('Y', 0 - tempN);
                                b2.setManualTranslation('Y', 0 - tempE);
                                isRestrictedName = true;
                            }
                            else
                            {
                                isRestrictedName = false;
                            }
                        }
                        else if (b.Name.EndsWith("N"))
                        {
                            //Console.WriteLine("Found N Bone: " + b);
                            string b2Finder = b.Name.Remove(b.Name.Length - 1, 1) + "E";
                            //Console.WriteLine("Searching for " + b2Finder);
                            b2 = model.FindBone(b2Finder);
                            if (b2 != null)
                            {
                                //Console.WriteLine("Found! Will not regenerate as was set by " + b2);
                                isRestrictedName = true;
                            }
                            else
                            {
                                isRestrictedName = false;
                            }
                        }
                    }

                    if (!boundaryFixActive || !isRestrictedName)
                    {
                        b.setManualTranslation('Y', 0 - b.getManualTranslation('Y'));
                        b.setManualRotation('Z', 0 - b.getManualRotation('Z'));
                    }
                    b = model.FindBoneByIndex(j);
                    isRestrictedName = false;
                }
            }
        }

        public void ReimportMeshes()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = SupportedFilesHandler.GetCompleteFilter("mdl0", "dae");
            ofd.Title = "Please select a model to reimport meshes from.";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                MDL0Node replacement = MDL0Node.FromFile(ofd.FileName);
                if (replacement != null)
                {
                    ((MDL0Node)_resource).ReplaceMeshes(replacement, true, true, true);
                    replacement.Dispose();
                    _resource.UpdateCurrentControl();
                }
            }
        }

        public void Preview()
        {
            new ModelForm().Show(_owner, (MDL0Node)_resource);
        }

        private void Optimize()
        {
            new ObjectOptimizerForm().ShowDialog(_resource);
        }

        public void NewShader()
        {
            MDL0Node model = ((MDL0Node)_resource);

            if (model._shadGroup == null)
            {
                MDL0GroupNode g = model._shadGroup;
                if (g == null)
                {
                    model.AddChild(g = new MDL0GroupNode(MDLResourceType.Shaders), true);
                    model._shadGroup = g; model._shadList = g.Children;
                }
            }

            if (model._shadList != null &&
                model._matList != null &&
                model._shadList.Count < model._matList.Count)
            {
                MDL0ShaderNode shader = new MDL0ShaderNode();
                model._shadGroup.AddChild(shader);
                shader.Default();
                shader.Rebuild(true);

                BaseWrapper b = FindResource(shader, true);
                if (b != null)
                    b.EnsureVisible();
            }
        }

        public void NewMaterial()
        {
            MDL0Node model = ((MDL0Node)_resource);

            if (model._matGroup == null)
            {
                MDL0GroupNode g = model._matGroup;
                if (g == null)
                {
                    model.AddChild(g = new MDL0GroupNode(MDLResourceType.Materials), true);
                    model._matGroup = g; model._matList = g.Children;
                }
            }

            MDL0MaterialNode mat = new MDL0MaterialNode();
            model._matGroup.AddChild(mat);
            mat.Name = "Material" + mat.Index;

            if (model._shadGroup == null)
            {
                MDL0GroupNode g = model._shadGroup;
                if (g == null)
                {
                    model.AddChild(g = new MDL0GroupNode(MDLResourceType.Shaders), true);
                    model._shadGroup = g; model._shadList = g.Children;
                }
            }
            if (model._shadList.Count == 0)
                NewShader();
            
            mat.ShaderNode = (MDL0ShaderNode)model._shadList[0];
            MDL0MaterialRefNode mr = new MDL0MaterialRefNode();
            mat.AddChild(mr);
            mr.Name = "MatRef0";
            mat.Rebuild(true);

            BaseWrapper b = FindResource(mat, true);
            if (b != null)
                b.EnsureVisible();
        }

        public void AutoMetal()
        {
            if (MessageBox.Show(null, "Are you sure you want to regenerate metal materials for Brawl?\nAll existing metal materials and shaders will be reset.", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                ((MDL0Node)_resource).GenerateMetalMaterials();
        }

        public void NameVertex()
        {
            MDL0Node model = ((MDL0Node)_resource);
            MDL0GroupNode g = model._vertGroup;
            if (g != null)
                foreach (MDL0VertexNode v in g.Children)
                {
                    string name = model.Name + "_";
                    if (v._objects.Count > 0)
                    {
                        MDL0ObjectNode o = v._objects[0];
                        name += o.Name;
                        if (o._drawCalls.Count > 0)
                        {
                            DrawCall c = o._drawCalls[0];
                            if (c.MaterialNode != null && c.VisibilityBoneNode != null)
                                name += "_" + c.Material + "_" + c.VisibilityBone;
                        }
                    }
                    else
                        name += "VertexArray";

                    v.Name = g.FindName(name);
                }
        }
        public void NameNormal()
        {
            MDL0Node model = ((MDL0Node)_resource);
            MDL0GroupNode g = model._normGroup;
            if (g != null)
                foreach (MDL0NormalNode v in g.Children)
                {
                    string name = model.Name + "_";
                    if (v._objects.Count > 0)
                    {
                        MDL0ObjectNode o = v._objects[0];
                        name += o.Name;
                        if (o._drawCalls.Count > 0)
                        {
                            DrawCall c = o._drawCalls[0];
                            if (c.MaterialNode != null && c.VisibilityBoneNode != null)
                                name += "_" + c.Material + "_" + c.VisibilityBone;
                        }
                    }
                    else
                        name += "NormalArray";

                    v.Name = g.FindName(name);
                }
        }
        public void NameColor()
        {
            MDL0Node model = ((MDL0Node)_resource);
            MDL0GroupNode g = model._colorGroup;
            if (g != null)
                foreach (MDL0ColorNode v in g.Children)
                {
                    string name = model.Name + "_";
                    if (v._objects.Count > 0)
                    {
                        MDL0ObjectNode o = v._objects[0];
                        name += o.Name;
                        if (o._drawCalls.Count > 0)
                        {
                            DrawCall c = o._drawCalls[0];
                            if (c.MaterialNode != null && c.VisibilityBoneNode != null)
                                name += "_" + c.Material + "_" + c.VisibilityBone;
                        }
                    }
                    else
                        name += "ColorArray";

                    v.Name = g.FindName(name);
                }
        }
        public void NameUV()
        {
            MDL0Node model = ((MDL0Node)_resource);
            MDL0GroupNode g = model._uvGroup;
            int i = 0;
            if (g != null)
                foreach (MDL0UVNode v in g.Children)
                    v.Name = "#" + i++;
        }
        public void NameObject()
        {
            MDL0Node model = ((MDL0Node)_resource);
            MDL0GroupNode g = model._objGroup;
            int i = 0;
            if (g != null)
                foreach (MDL0ObjectNode o in g.Children)
                    o.Name = "polygon" + i++;
        }

        public MDL0VertexNode NewVertex()
        {
            MDL0Node model = ((MDL0Node)_resource);

            MDL0GroupNode g = model._vertGroup;
            if (g == null)
            {
                model.AddChild(g = new MDL0GroupNode(MDLResourceType.Vertices), true);
                model._vertGroup = g; model._vertList = g.Children;
            }

            MDL0VertexNode node = new MDL0VertexNode() { Name = "VertexSet" + ((MDL0Node)_resource)._vertList.Count };
            node.Vertices = new Vector3[] { new Vector3(0) };
            g.AddChild(node, true);
            node._forceRebuild = true;
            node.Rebuild(true);
            node.SignalPropertyChange();

            FindResource(node, true).EnsureVisible();

            return node;
        }

        public MDL0NormalNode NewNormal()
        {
            MDL0Node model = ((MDL0Node)_resource);

            MDL0GroupNode g = model._normGroup;
            if (g == null)
            {
                model.AddChild(g = new MDL0GroupNode(MDLResourceType.Normals), true);
                model._normGroup = g; model._normList = g.Children;
            }

            MDL0NormalNode node = new MDL0NormalNode() { Name = "NormalSet" + ((MDL0Node)_resource)._normList.Count };
            node.Normals = new Vector3[] { new Vector3(0) };
            g.AddChild(node, true);
            node._forceRebuild = true;
            node.Rebuild(true);
            node.SignalPropertyChange();

            FindResource(node, true).EnsureVisible();

            return node;
        }

        public MDL0ColorNode NewColor()
        {
            MDL0Node model = ((MDL0Node)_resource);

            MDL0GroupNode g = model._colorGroup;
            if (g == null)
            {
                model.AddChild(g = new MDL0GroupNode(MDLResourceType.Colors), true);
                model._colorGroup = g; model._colorList = g.Children;
            }

            MDL0ColorNode node = new MDL0ColorNode() { Name = "ColorSet" + ((MDL0Node)_resource)._colorList.Count };
            node.Colors = new RGBAPixel[] { new RGBAPixel() { A = 255, R = 128, G = 128, B = 128 } };
            g.AddChild(node, true);

            node.Rebuild(true);
            node.SignalPropertyChange();

            FindResource(node, true).EnsureVisible();

            return node;
        }

        public MDL0UVNode NewUV()
        {
            MDL0Node model = ((MDL0Node)_resource);

            MDL0GroupNode g = model._uvGroup;
            if (g == null)
            {
                model.AddChild(g = new MDL0GroupNode(MDLResourceType.UVs), true);
                model._uvGroup = g; model._uvList = g.Children;
            }

            MDL0UVNode node = new MDL0UVNode() { Name = "#" + ((MDL0Node)_resource)._uvList.Count };
            node.Points = new Vector2[] { new Vector2(0) };
            g.AddChild(node, true);
            node._forceRebuild = true;
            node.Rebuild(true);
            node.SignalPropertyChange();

            FindResource(node, true).EnsureVisible();

            return node;
        }

        public void ImportVertex()
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Raw Vertex Set (*.*)|*.*";
            o.Title = "Please select a vertex set to import.";
            if (o.ShowDialog() == DialogResult.OK)
                NewVertex().Replace(o.FileName);
        }

        public void ImportNormal()
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Raw Normal Set (*.*)|*.*";
            o.Title = "Please select a normal set to import.";
            if (o.ShowDialog() == DialogResult.OK)
                NewNormal().Replace(o.FileName);
        }

        public void ImportColor()
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Raw Color Set (*.*)|*.*";
            o.Title = "Please select a color set to import.";
            if (o.ShowDialog() == DialogResult.OK)
                NewColor().Replace(o.FileName);
        }

        public void ImportUV()
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Raw Vertex Set (*.*)|*.*";
            o.Title = "Please select a vertex set to import.";
            if (o.ShowDialog() == DialogResult.OK)
                NewUV().Replace(o.FileName);
        }

        public void ImportObject()
        {
            MDL0Node external = null;
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "MDL0 Raw Model (*.mdl0)|*.mdl0";
            o.Title = "Please select a model to import an object from.";
            if (o.ShowDialog() == DialogResult.OK)
                if ((external = (MDL0Node)NodeFactory.FromFile(null, o.FileName)) != null)
                    new ObjectImporter().ShowDialog((MDL0Node)_resource, external);
        }

        private void RecalcBoundingBoxes()
        {
            MDL0Node model = _resource as MDL0Node;
            if (model != null)
                model.CalculateBoundingBoxes();
        }
    }
}
