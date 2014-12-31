using System;
using BrawlLib.OpenGL;
using System.ComponentModel;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;
using BrawlLib.Modeling;
using System.Drawing;
using BrawlLib.Wii.Animations;
using System.Collections.Generic;
using BrawlLib.SSBBTypes;
using BrawlLib.IO;
using BrawlLib;
using System.Drawing.Imaging;
using Gif.Components;
using OpenTK.Graphics.OpenGL;
using BrawlLib.Imaging;
using System.Windows;
using System.Threading;

namespace System.Windows.Forms
{
    public partial class ModelEditControl : ModelEditorBase
    {
        private void chkBones_Click(object sender, EventArgs e) { if (!_updating) RenderBones = !RenderBones; }
        private void toggleBones_Click(object sender, EventArgs e) { if (!_updating) RenderBones = !RenderBones; }

        private void chkPolygons_Click(object sender, EventArgs e) { if (!_updating) RenderPolygons = !RenderPolygons; }
        private void togglePolygons_Click(object sender, EventArgs e) { if (!_updating) RenderPolygons = !RenderPolygons; }

        private void chkVertices_Click(object sender, EventArgs e) { if (!_updating) RenderVertices = !RenderVertices; }
        private void toggleVertices_Click(object sender, EventArgs e) { if (!_updating) RenderVertices = !RenderVertices; }

        private void chkCollisions_Click(object sender, EventArgs e) { if (!_updating) RenderCollisions = !RenderCollisions; }
        private void toggleCollisions_Click(object sender, EventArgs e) { if (!_updating) RenderCollisions = !RenderCollisions; }

        private void chkFloor_Click(object sender, EventArgs e) { if (!_updating) RenderFloor = !RenderFloor; }
        private void toggleFloor_Click(object sender, EventArgs e) { if (!_updating) RenderFloor = !RenderFloor; }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e) { if (!_updating) RenderWireframe = !RenderWireframe; }
        private void toggleNormals_Click(object sender, EventArgs e) { if (!_updating) RenderNormals = !RenderNormals; }
        private void boundingBoxToolStripMenuItem_Click(object sender, EventArgs e) { if (!_updating) RenderBox = !RenderBox; }

        #region Screen Capture

        private void ScreenCapBgLocText_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog d = new FolderBrowserDialog())
            {
                d.SelectedPath = ScreenCapBgLocText.Text;
                d.Description = "Choose a place to automatically save screen captures.";
                if (d.ShowDialog(this) == DialogResult.OK)
                    ScreenCapBgLocText.Text = d.SelectedPath;
            }
            if (String.IsNullOrEmpty(ScreenCapBgLocText.Text))
                ScreenCapBgLocText.Text = Application.StartupPath + "\\ScreenCaptures";
        }
        private string _imgExt = ".png";
        private int _imgExtIndex = 0;
        public int ImgExtIndex
        {
            get { return _imgExtIndex; }
            set
            {
                switch (_imgExtIndex = value)
                {
                    case 0: _imgExt = ".png"; break;
                    case 1: _imgExt = ".tga"; break;
                    case 2: _imgExt = ".tif"; break;
                    case 3: _imgExt = ".bmp"; break;
                    case 4: _imgExt = ".jpg"; break;
                    case 5: _imgExt = ".gif"; break;
                }
                imageFormatToolStripMenuItem.Text = "Image Format: " + _imgExt.Substring(1).ToUpper();
            }
        }
        private void imageFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Just use an existing dialog with the same basic function
            using (ExportAllFormatDialog d = new ExportAllFormatDialog())
            {
                d.Text = "Choose texture format";
                d.comboBox1.Items.RemoveAt(6); //TEX0
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    _imgExtIndex = d.comboBox1.SelectedIndex;
                    _imgExt = d.SelectedExtension;
                    imageFormatToolStripMenuItem.Text = "Image Format: " + _imgExt.Substring(1).ToUpper();
                }
            }
        }

        private void btnExportToImgWithTransparency_Click(object sender, EventArgs e)
        {
            SaveBitmap(ModelPanel.GetScreenshot(true), ScreenCapBgLocText.Text, _imgExt);
        }
        private void btnExportToImgNoTransparency_Click(object sender, EventArgs e)
        {
            SaveBitmap(ModelPanel.GetScreenshot(false), ScreenCapBgLocText.Text, _imgExt);
        }

        private void btnExportToAnimatedGIF_Click(object sender, EventArgs e)
        {
            SetFrame(1);
            images = new List<Image>();
            _loop = false;
            _capture = true;
            Enabled = false;
            ModelPanel.Enabled = false;
            if (InterpolationEditor != null)
                InterpolationEditor.Enabled = false;
            if (disableBonesWhenPlayingToolStripMenuItem.Checked)
            {
                if (RenderBones == false)
                    _bonesWereOff = true;
                RenderBones = false;
            }
            btnPlay_Click(null, null);
        }

        #endregion

        #region Model Viewer Detaching

        private void detachViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_updating)
                return;

            if (_viewerForm == null)
            {
                modelPanel.Visible = false;
                modelPanel.Enabled = false;
                detachViewerToolStripMenuItem.Text = "Attach";

                _viewerForm = new ModelViewerForm(this);

                _viewerForm.modelPanel1._settings = modelPanel._settings;
                _viewerForm.modelPanel1._camera = modelPanel._camera;

                _viewerForm.FormClosed += _viewerForm_FormClosed;

                UnlinkModelPanel(modelPanel);
                LinkModelPanel(_viewerForm.modelPanel1);

                _viewerForm.modelPanel1.EventProcessKeyMessage += ProcessKeyPreview;

                OnModelPanelChanged();

                _viewerForm.Show();
                _viewerForm.modelPanel1.Invalidate();

                _interpolationEditor.Visible = true;
                InterpolationFormOpen = false;
                interpolationEditorToolStripMenuItem.Enabled = false;

                if (_interpolationForm != null)
                    _interpolationForm.Close();
            }
            else
                _viewerForm.Close();
        }

        void _viewerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            modelPanel.Visible = true;
            modelPanel.Enabled = true;
            detachViewerToolStripMenuItem.Text = "Detach";
            
            _viewerForm.modelPanel1.EventProcessKeyMessage -= ProcessKeyPreview;

            UnlinkModelPanel(_viewerForm.modelPanel1);
            LinkModelPanel(modelPanel);

            _viewerForm = null;
            _interpolationEditor.Visible = false;
            interpolationEditorToolStripMenuItem.Enabled = true;

            OnModelPanelChanged();
        }

        #endregion

        #region Settings Management

        private void clearSavedSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrawlBox.Properties.Settings.Default.ViewerSettings = null;
            BrawlBox.Properties.Settings.Default.ViewerSettingsSet = false;
            SetDefaultSettings();
        }

        private unsafe void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sd = new SaveFileDialog();
            sd.Filter = "Brawlbox Settings (*.settings)|*.settings";
            sd.FileName = Application.StartupPath;
            if (sd.ShowDialog() == DialogResult.OK)
            {
                string path = sd.FileName;
                ModelEditorSettings settings = CollectSettings();

                Serializer.SerializeObject(path, settings);
                MessageBox.Show("Settings successfully saved to " + path);
            }
        }

        private unsafe void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
            od.Filter = "Brawlbox Settings (*.settings)|*.settings";
            od.FileName = Application.StartupPath;
            if (od.ShowDialog() == DialogResult.OK)
            {
                string path = od.FileName;
                ModelEditorSettings settings = Serializer.DeserializeObject(path) as ModelEditorSettings;
                DistributeSettings(settings);
            }
        }

        #endregion

        #region Menu Buttons
        private void btnSaveCam_Click(object sender, EventArgs e)
        {
            if (btnSaveCam.Text == "Save Camera")
            {
                ModelPanel._settings._defaultRotate = ModelPanel.Camera._rotation;
                ModelPanel._settings._defaultTranslate = ModelPanel.Camera._matrixInverse.Multiply(new Vector3());

                btnSaveCam.Text = "Clear Camera";
            }
            else
            {
                ModelPanel._settings._defaultRotate = new Vector3();
                ModelPanel._settings._defaultTranslate = new Vector3();

                btnSaveCam.Text = "Save Camera";
            }
        }

        #endregion

        #region Panel Toggles

        private void btnLeftToggle_Click(object sender, EventArgs e) { showLeft.Checked = !showLeft.Checked; }
        private void btnTopToggle_Click(object sender, EventArgs e) { showTop.Checked = !showTop.Checked; }
        private void btnBottomToggle_Click(object sender, EventArgs e) { showBottom.Checked = !showBottom.Checked; CheckDimensions(); }
        private void btnRightToggle_Click(object sender, EventArgs e) { showRight.Checked = !showRight.Checked; }

        #endregion

        private void LiveTextureFolderPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog d = new FolderBrowserDialog())
            {
                d.SelectedPath = LiveTextureFolderPath.Text;
                d.Description = "Choose a place to automatically scan for textures to apply when modified.";
                if (d.ShowDialog(this) == DialogResult.OK)
                    LiveTextureFolderPath.Text = MDL0TextureNode.TextureOverrideDirectory = d.SelectedPath;
            }
            if (String.IsNullOrEmpty(LiveTextureFolderPath.Text))
                LiveTextureFolderPath.Text = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            modelPanel.RefreshReferences();
        }

        private void chkZoomExtents_Click(object sender, EventArgs e)
        {
            //TODO: different handling based on if viewport is perspective, front, side, or top
            ModelPanel.Camera.Reset();
            ModelPanel.Camera.Translate(SelectedBone.Matrix.GetPoint() + new Vector3(0.0f, 0.0f, 27.0f));
            ModelPanel.Invalidate();

        }
        private void chkBoundaries_Click(object sender, EventArgs e)
        {
            ModelPanel.Invalidate();
        }

        private void syncTexObjToolStripMenuItem_CheckedChanged(object sender, EventArgs e) { leftPanel.UpdateTextures(); }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "All Compatible Files (*.pac, *.pcs, *.brres, *.mrg, *.arc, *.szs,  *.mdl0)|*.pac;*.pcs;*.brres;*.mrg;*.arc;*.szs;*.mdl0";
            d.Title = "Select a file to open";
            if (d.ShowDialog() == DialogResult.OK)
                OpenFile(d.FileName);
        }

        private void newSceneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to clear the current scene?\nYou will lose any unsaved data.", "Continue?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return;

            TargetModel = null;
            _targetModels.Clear();

            ModelPanel.ClearAll();

            models.Items.Clear();
            models.Items.Add("All");
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Close()) this.ParentForm.Close();
        }

        #region Rendered Models

        private void hideFromSceneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _resetCamera = false;

            ModelPanel.RemoveTarget(TargetModel);

            if (_targetModels != null && _targetModels.Count != 0)
                TargetModel = _targetModels[0];

            ModelPanel.Invalidate();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _resetCamera = false;

            ModelPanel.RemoveTarget(TargetModel);
            _targetModels.Remove(TargetModel);
            models.Items.Remove(TargetModel);

            if (_targetModels != null && _targetModels.Count != 0)
                TargetModel = _targetModels[0];

            ModelPanel.Invalidate();
        }

        private void hideAllOtherModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (IModel node in _targetModels)
                if (node != TargetModel)
                    ModelPanel.RemoveTarget(node);

            ModelPanel.Invalidate();
        }

        private void deleteAllOtherModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (IModel node in _targetModels)
                if (node != TargetModel)
                {
                    _targetModels.Remove(node);
                    ModelPanel.RemoveTarget(node);
                    models.Items.Remove(node);
                }

            ModelPanel.Invalidate();
        }

        #endregion

        private void helpToolStripMenuItem_Click(object sender, EventArgs e) { new ModelViewerHelp().Show(this, false); }
        private void modifyLightingToolStripMenuItem_Click(object sender, EventArgs e) { new ModelViewerSettingsDialog().Show(this); }
        private void resetCameraToolStripMenuItem_Click_1(object sender, EventArgs e) { ModelPanel.ResetCamera(); }

        private void interpolationEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_interpolationForm == null)
            {
                _interpolationForm = new InterpolationForm(this);
                _interpolationForm.FormClosed += _interpolationForm_FormClosed;
                _interpolationForm.Show();
                InterpolationFormOpen = true;
                UpdatePropDisplay();
            }
            else
                _interpolationForm.Close();
        }

        private void portToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TargetModel == null || !(TargetModel is MDL0Node))
                return;

            AnimationNode node = TargetAnimation;
            if (node is CHR0Node)
                (node as CHR0Node).Port((MDL0Node)TargetModel);

            AnimChanged(TargetAnimType);
        }

        private void mergeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TargetModel == null)
                return;

            AnimationNode node = TargetAnimation;
            if (node is CHR0Node)
                (node as CHR0Node).MergeWith();

            AnimChanged(TargetAnimType);
        }

        private void appendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TargetModel == null)
                return;

            AnimationNode node = TargetAnimation;
            if (node is CHR0Node)
                (node as CHR0Node).Append();
            else if (node is SRT0Node)
                (node as SRT0Node).Append();
            else if (node is SHP0Node)
                (node as SHP0Node).Append();
            else if (node is PAT0Node)
                (node as PAT0Node).Append();
            else if (node is VIS0Node)
                (node as VIS0Node).Append();

            AnimChanged(TargetAnimType);
        }

        private void resizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TargetModel == null)
                return;

            AnimationNode node = TargetAnimation;
            if (node is CHR0Node)
                (node as CHR0Node).Resize();
            else if (node is SRT0Node)
                (node as SRT0Node).Resize();
            else if (node is SHP0Node)
                (node as SHP0Node).Resize();
            else if (node is PAT0Node)
                (node as PAT0Node).Resize();
            else if (node is VIS0Node)
                (node as VIS0Node).Resize();

            AnimChanged(TargetAnimType);
        }

        private void averageAllStartEndTangentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnimationNode n = TargetAnimation;
            if (n is CHR0Node)
                ((CHR0Node)n).AverageKeys();
            if (n is SRT0Node)
                ((SRT0Node)n).AverageKeys();
            if (n is SHP0Node)
                ((SHP0Node)n).AverageKeys();
        }

        private void averageboneStartendTangentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnimationNode n = TargetAnimation;
            if (n is CHR0Node && SelectedBone != null)
                ((CHR0Node)n).AverageKeys(SelectedBone.Name);
            if (n is SRT0Node && TargetTexRef != null)
                ((SRT0Node)n).AverageKeys(TargetTexRef.Parent.Name, TargetTexRef.Index);
            if (n is SHP0Node && SHP0Editor.VertexSet != null && SHP0Editor.VertexSetDest != null)
                ((SHP0Node)n).AverageKeys(SHP0Editor.VertexSet.Name, SHP0Editor.VertexSetDest.Name);
        }
    }
}
