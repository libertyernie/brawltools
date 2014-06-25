using BrawlLib.Imaging;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace System.Windows.Forms
{
    public partial class ModelEditControl : UserControl, IMainWindow
    {
        private void VISEntryChanged(object sender, EventArgs e)
        {
            UpdateModel();
        }

        private void VISIndexChanged(object sender, EventArgs e)
        {
            int i = KeyframePanel.visEditor.listBox1.SelectedIndex;
            if (i >= 0 && i <= MaxFrame && i != CurrentFrame - 1)
                SetFrame(i + 1);
        }

        private void models_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            _resetCamera = false;

            TargetCollision = null;
            if ((models.SelectedItem is MDL0Node) && models.SelectedItem.ToString() != "All")
                TargetModel = (MDL0Node)models.SelectedItem;
            else if (models.SelectedItem is CollisionNode)
                TargetCollision = (CollisionNode)models.SelectedItem;
            else
                TargetModel = _targetModels != null && _targetModels.Count > 0 ? _targetModels[0] : null;

            _undoSaves.Clear();
            _redoSaves.Clear();
            _saveIndex = -1;
        }

        private void _interpolationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _interpolationForm = null;
            interpolationEditorToolStripMenuItem.Checked = false;
        }

        private void pnlPlayback_Resize(object sender, EventArgs e)
        {
            if (pnlPlayback.Width <= pnlPlayback.MinimumSize.Width)
            {
                pnlPlayback.Dock = DockStyle.Left;
                pnlPlayback.Width = pnlPlayback.MinimumSize.Width;
            }
            else
                pnlPlayback.Dock = DockStyle.Fill;
        }

        bool addedHeight = false;
        private void ModelEditControl_SizeChanged(object sender, EventArgs e)
        {
            CheckDimensions();
        }

        public void CheckDimensions()
        {
            if (pnlPlayback.Width <= pnlPlayback.MinimumSize.Width)
            {
                pnlPlayback.Dock = DockStyle.Left;
                pnlPlayback.Width = pnlPlayback.MinimumSize.Width;
            }
            else
                pnlPlayback.Dock = DockStyle.Fill;

            if (_updating)
                return;

            if (animEditors.Width - animCtrlPnl.Width >= pnlPlayback.MinimumSize.Width)
            {
                pnlPlayback.Width += animEditors.Width - animCtrlPnl.Width - pnlPlayback.MinimumSize.Width;
                pnlPlayback.Dock = DockStyle.Fill;
            }
            else pnlPlayback.Dock = DockStyle.Left;

            if (animCtrlPnl.Width + pnlPlayback.Width <= animEditors.Width)
            {
                if (addedHeight)
                {
                    _updating = true;
                    animEditors.Height -= 17;
                    _updating = false;
                    animEditors.HorizontalScroll.Visible = addedHeight = false;
                }
            }
            else
            {
                if (!addedHeight)
                {
                    _updating = true;
                    animEditors.Height += 17;
                    _updating = false;
                    animEditors.HorizontalScroll.Visible = addedHeight = true;
                }
            }
        }

        public bool Close()
        {
            try
            {
                if (!CloseExternal())
                    return false;

                BrawlBox.Properties.Settings.Default.ViewerSettings = CollectSettings();
                BrawlBox.Properties.Settings.Default.ScreenCapBgLocText = ScreenCapBgLocText.Text;
                BrawlBox.Properties.Settings.Default.ViewerSettingsSet = true;
                BrawlBox.Properties.Settings.Default.Save();

                StopAnim();

                if (_viewerForm != null)
                    _viewerForm.Close();
                if (_interpolationForm != null)
                    _interpolationForm.Close();

                if (TargetModel != null)
                {
                    TargetModel.ApplyCHR(null, 0);
                    TargetModel.ApplySRT(null, 0);
                }
                ResetBoneColors();
            }
            catch { }
            return true;
        }
        private void ModelChanged(MDL0Node model)
        {
            if (model != null && !_targetModels.Contains(model))
                _targetModels.Add(model);

            if (_targetModel != null)
                _targetModel._isTargetModel = false;

            if (model == null)
                ModelPanel.RemoveTarget(_targetModel);

            if ((_targetModel = model) != null)
            {
                ModelPanel.AddTarget(_targetModel);
                leftPanel.VIS0Indices = _targetModel.VIS0Indices;
                _targetModel._isTargetModel = true;
                ResetVertexColors();
            }
            else
                models.SelectedIndex = 0;

            if (_resetCamera)
            {
                ModelPanel.ResetCamera();
                SetFrame(0);
            }
            else
                _resetCamera = true;

            leftPanel.Reset();
            rightPanel.Reset();

            if (TargetModelChanged != null)
                TargetModelChanged(this, null);

            _updating = true;
            if (_targetModel != null && !_editingAll && TargetCollision == null)
                models.SelectedItem = _targetModel;
            _updating = false;

            if (_targetModel != null)
                RenderBones = _targetModel._renderBones;
        }

        private bool PointCollides(Vector3 point) {
            float f;
            return PointCollides(point, out f);
        }
        private bool PointCollides(Vector3 point, out float y_result) {
            Vector2 v2 = new Vector2(point._x, point._y);
            foreach (CollisionNode coll in _collisions) {
                foreach (CollisionObject obj in coll._objects) {
                    if (obj._render) {
                        foreach (CollisionPlane plane in obj._planes) {
                            if (plane._type == BrawlLib.SSBBTypes.CollisionPlaneType.Floor) {
                                if (plane.PointLeft._x < v2._x && plane.PointRight._x > v2._x) {
                                    float x = v2._x;
                                    float m = (plane.PointLeft._y - plane.PointRight._y)
                                        / (plane.PointLeft._x - plane.PointRight._x);
                                    float b = plane.PointRight._y - m * plane.PointRight._x;
                                    y_result = m * x + b;
                                    if (Math.Abs(y_result - v2._y) <= 5) {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            y_result = 0;
            return false;
        }
        private void SnapYIfClose() {
            float f;
            if (PointCollides(new Vector3(chr0Editor._transBoxes[6].Value, chr0Editor._transBoxes[7].Value, chr0Editor._transBoxes[8].Value), out f)) {
                ApplyTranslation(1, f - chr0Editor._transBoxes[7].Value);
            }
        }

        #region Settings
        public BrawlBoxViewerSettings CollectSettings()
        {
            BrawlBoxViewerSettings settings = new BrawlBoxViewerSettings();
            settings._tag = BrawlBoxViewerSettings.Tag;
            settings._version = 5;

            settings.RetrieveCorrAnims = syncAnimationsTogetherToolStripMenuItem.Checked;
            settings.DisplayExternalAnims = chkExternalAnims.Checked;
            settings.DisplayBRRESAnims = chkBRRESAnims.Checked;
            settings.DisplayNonBRRESAnims = chkNonBRRESAnims.Checked;
            settings.SyncTexToObj = syncTexObjToolStripMenuItem.Checked;
            settings.SyncObjToVIS0 = syncObjectsListToVIS0ToolStripMenuItem.Checked;
            settings.DisableBonesOnPlay = disableBonesWhenPlayingToolStripMenuItem.Checked;

            settings._defaultCam = ModelPanel.DefaultTranslate;
            settings._defaultRot = ModelPanel.DefaultRotate;
            settings._amb = ModelPanel.Ambient;
            settings._pos = ModelPanel.LightPosition;
            settings._diff = ModelPanel.Diffuse;
            settings._spec = ModelPanel.Specular;
            settings._yFov = ModelPanel._fovY;
            settings._nearZ = ModelPanel._nearZ;
            settings._farz = ModelPanel._farZ;
            settings._tScale = ModelPanel.TranslationScale;
            settings._rScale = ModelPanel.RotationScale;
            settings._zScale = ModelPanel.ZoomScale;
            settings._orbColor = (ARGBPixel)MDL0BoneNode.DefaultNodeColor;
            settings._lineColor = (ARGBPixel)MDL0BoneNode.DefaultBoneColor;
            settings._floorColor = (ARGBPixel)StaticMainWindow._floorHue;
            settings._undoCount = (uint)_allowedUndos;
            settings._shaderCount = 0;
            settings._matCount = 0;
            settings._emis = ModelPanel.Emission;

            settings.SnapToColl = chkSnapToColl.Checked;
            settings.Maximize = chkMaximize.Checked;
            settings.CameraSet = btnSaveCam.Text == "Clear Camera";
            settings.ImageCapFmt = _imgExtIndex;
            settings.Bones = _renderBones;
            settings.Polys = _renderPolygons;
            settings.Wireframe = _renderWireframe;
            settings.Vertices = _renderVertices;
            settings.Normals = _renderNormals;
            settings.HideOffscreen = _dontRenderOffscreen;
            settings.BoundingBox = _renderBox;
            settings.ShowCamCoords = showCameraCoordinatesToolStripMenuItem.Checked;
            settings.Floor = _renderFloor;
            settings.OrthoCam = orthographicToolStripMenuItem.Checked;
            settings.EnableSmoothing = enablePointAndLineSmoothingToolStripMenuItem.Checked;
            settings.EnableText = enableTextOverlaysToolStripMenuItem.Checked;

            settings.LinearCHR = chkLinearCHR.Checked;
            settings.LinearSRT = chkLinearSRT.Checked;
            settings.LinearSHP = chkLinearSHP.Checked;
            settings.LinearLight = chkLinearLight.Checked;
            settings.LinearFog = chkLinearFog.Checked;
            settings.LinearCam = chkLinearCamera.Checked;

            settings.GenTansCHR = chkGenTansCHR.Checked;
            settings.GenTansSRT = chkGenTansSRT.Checked;
            settings.GenTansSHP = chkGenTansSHP.Checked;
            settings.GenTansLight = chkGenTansLight.Checked;
            settings.GenTansFog = chkGenTansFog.Checked;
            settings.GenTansCam = chkGenTansCamera.Checked;

            return settings;
        }
        public void SetDefaultSettings() { DistributeSettings(BrawlBoxViewerSettings.Default); }
        public void DistributeSettings(BrawlBoxViewerSettings settings)
        {
            _updating = true;
            ModelPanel.BeginUpdate();

            syncAnimationsTogetherToolStripMenuItem.Checked = settings.RetrieveCorrAnims;
            syncTexObjToolStripMenuItem.Checked = settings.SyncTexToObj;
            syncObjectsListToVIS0ToolStripMenuItem.Checked = settings.SyncObjToVIS0;
            disableBonesWhenPlayingToolStripMenuItem.Checked = settings.DisableBonesOnPlay;
            chkSnapToColl.Checked = settings.SnapToColl;
            chkMaximize.Checked = settings.Maximize;
            chkExternalAnims.Checked = settings.DisplayExternalAnims;
            chkBRRESAnims.Checked = settings.DisplayBRRESAnims;
            chkNonBRRESAnims.Checked = settings.DisplayNonBRRESAnims;

            ModelPanel.Ambient = settings._amb;
            ModelPanel.LightPosition = settings._pos;
            ModelPanel.Diffuse = settings._diff;
            ModelPanel.Specular = settings._spec;
            ModelPanel.Emission = settings._emis;

            ModelPanel._fovY = settings._yFov;
            ModelPanel._nearZ = settings._nearZ;
            ModelPanel._farZ = settings._farz;

            ModelPanel.ZoomScale = settings._zScale;
            ModelPanel.TranslationScale = settings._tScale;
            ModelPanel.RotationScale = settings._rScale;

            MDL0BoneNode.DefaultNodeColor = (Color)settings._orbColor;
            MDL0BoneNode.DefaultBoneColor = (Color)settings._lineColor;
            StaticMainWindow._floorHue = (Color)settings._floorColor;
            if (settings.CameraSet)
            {
                btnSaveCam.Text = "Clear Camera";
                ModelPanel.DefaultTranslate = settings._defaultCam;
                ModelPanel.DefaultRotate = settings._defaultRot;
            }
            else
            {
                btnSaveCam.Text = "Save Camera";
                ModelPanel.DefaultTranslate = new Vector3();
                ModelPanel.DefaultRotate = new Vector2();
            }

            _allowedUndos = settings._undoCount;
            ImgExtIndex = settings.ImageCapFmt;

            RenderBones = settings.Bones;
            RenderWireframe = settings.Wireframe;
            RenderPolygons = settings.Polys;
            RenderVertices = settings.Vertices;
            RenderBox = settings.BoundingBox;
            RenderNormals = settings.Normals;
            DontRenderOffscreen = settings.HideOffscreen;
            showCameraCoordinatesToolStripMenuItem.Checked = settings.ShowCamCoords;
            RenderFloor = settings.Floor;
            enablePointAndLineSmoothingToolStripMenuItem.Checked = settings.EnableSmoothing;
            enableTextOverlaysToolStripMenuItem.Checked = settings.EnableText;

            chkLinearCHR.Checked = settings.LinearCHR;
            chkLinearSRT.Checked = settings.LinearSRT;
            chkLinearSHP.Checked = settings.LinearSHP;
            chkLinearLight.Checked = settings.LinearLight;
            chkLinearFog.Checked = settings.LinearFog;
            chkLinearCamera.Checked = settings.LinearCam;

            chkGenTansCHR.Checked = settings.GenTansCHR;
            chkGenTansSRT.Checked = settings.GenTansSRT;
            chkGenTansSHP.Checked = settings.GenTansSHP;
            chkGenTansLight.Checked = settings.GenTansLight;
            chkGenTansFog.Checked = settings.GenTansFog;
            chkGenTansCamera.Checked = settings.GenTansCam;

            ModelPanel.EndUpdate();
            ModelPanel.Invalidate();
            _updating = false;
        }
        #endregion
    }
}
