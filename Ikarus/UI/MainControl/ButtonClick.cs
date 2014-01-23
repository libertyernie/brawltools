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
using System.Windows.Forms;

namespace Ikarus.UI
{
    public partial class MainControl : UserControl, IMainWindow
    {
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
                ScreenCapBgLocText.Text = Application.StartupPath;
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
        private void SaveBitmap(Bitmap bmp)
        {
            if (!String.IsNullOrEmpty(ScreenCapBgLocText.Text) && !String.IsNullOrEmpty(_imgExt))
            {
                try
                {
                    string outPath = ScreenCapBgLocText.Text;
                    DirectoryInfo dir = new DirectoryInfo(outPath);
                    FileInfo[] files = dir.GetFiles();
                    int i = 0;
                    string name = "BrawlboxScreencap";
                    Top:
                    foreach (FileInfo f in files)
                        if (f.Name == name + i + _imgExt)
                        {
                            i++;
                            goto Top;
                        }
                    outPath += "\\" + name + i + _imgExt;
                    bool okay = true;
                    if (_imgExt.Equals(".png"))
                        bmp.Save(outPath, ImageFormat.Png);
                    else if (_imgExt.Equals(".tga"))
                        bmp.SaveTGA(outPath);
                    else if (_imgExt.Equals(".tiff") || _imgExt.Equals(".tif"))
                        bmp.Save(outPath, ImageFormat.Tiff);
                    else if (_imgExt.Equals(".bmp"))
                        bmp.Save(outPath, ImageFormat.Bmp);
                    else if (_imgExt.Equals(".jpg") || outPath.EndsWith(".jpeg"))
                        bmp.Save(outPath, ImageFormat.Jpeg);
                    else if (_imgExt.Equals(".gif"))
                        bmp.Save(outPath, ImageFormat.Gif);
                    else { okay = false; }
                    if (okay)
                        MessageBox.Show("Screenshot successfully saved to " + outPath.Replace("\\", "/"));
                }
                catch { }
            }
            bmp.Dispose();
        }
        private void btnExportToImgWithTransparency_Click(object sender, EventArgs e)
        {
            //Make sure the background alpha value is 0.
            //GL.ClearColor(Color.Transparent);
            //Image i = null;
            //if (BackgroundImage != null)
            //{
            //    i = BackgroundImage;
            //    BackgroundImage = null;
            //}
            //Invalidate();
            SaveBitmap(modelPanel.GrabScreenshot(true));
            //GL.ClearColor(BackColor);
            //if (i != null)
            //    BackgroundImage = i;
        }
        private void btnExportToImgNoTransparency_Click(object sender, EventArgs e)
        {
            SaveBitmap(modelPanel.GrabScreenshot(false));
        }

        private void showMoveset_Click_1(object sender, EventArgs e)
        {
            if (Manager.Moveset != null)
                showRight.Checked = !showRight.Checked;
            else
                showRight.Checked = false;
        }

        public bool _capture = false;
        public List<Image> images = new List<Image>();
        public void btnExportToAnimatedGIF_Click(object sender, EventArgs e)
        {
            SetFrame(1);
            images = new List<Image>();
            RunTime._loop = false;
            _capture = true;
            Enabled = false;
            ModelPanel.Enabled = false;
            if (InterpolationEditor != null)
                InterpolationEditor.Enabled = false;
            btnPlay_Click(null, null);
        }

        public void RenderToGIF(Image[] images)
        {
            string outPath = "";
        Start:
            if (!String.IsNullOrEmpty(ScreenCapBgLocText.Text))
            {
                try
                {
                    outPath = ScreenCapBgLocText.Text;
                    if (!Directory.Exists(outPath))
                        Directory.CreateDirectory(outPath);

                    DirectoryInfo dir = new DirectoryInfo(outPath);
                    FileInfo[] files = dir.GetFiles();
                    int i = 0;
                    string name = "Animation";
                Top:
                    foreach (FileInfo f in files)
                        if (f.Name == name + i + ".gif")
                        {
                            i++;
                            goto Top;
                        }
                    outPath += "\\" + name + i + ".gif";
                }
                catch { }
            }
            else
            {
                ScreenCapBgLocText.Text = Application.StartupPath + "\\ScreenCaptures";
                goto Start;
            }

            AnimatedGifEncoder e = new AnimatedGifEncoder();
            e.Start(outPath);
            e.SetDelay(1000 / (int)pnlPlayback.numFPS.Value);
            e.SetRepeat(0);
            e.SetQuality(1);
            using (ProgressWindow progress = new ProgressWindow(this, "GIF Encoder", "Encoding, please wait...", true))
            {
                progress.TopMost = true;
                progress.Begin(0, images.Length, 0);
                for (int i = 0, count = images.Length; i < count; i++)
                {
                    if (progress.Cancelled)
                        break;

                    e.AddFrame(images[i]);
                    progress.Update(progress.CurrentValue + 1);
                }
                progress.Finish();
                e.Finish();
            }

            if (InterpolationEditor != null)
                InterpolationEditor.Enabled = true;
            ModelPanel.Enabled = true;
            Enabled = true;

            MessageBox.Show("GIF successfully saved to " + outPath.Replace("\\", "/"));
        }

        private void btnSaveCam_Click(object sender, EventArgs e)
        {
            if (btnSaveCam.Text == "Save Camera")
            {
                modelPanel.DefaultRotate = new Vector2(modelPanel._camera._rotation._x, modelPanel._camera._rotation._y);
                modelPanel.DefaultTranslate = modelPanel._camera._matrixInverse.Multiply(new Vector3());

                btnSaveCam.Text = "Clear Camera";
            }
            else
            {
                modelPanel.DefaultRotate = new Vector2();
                modelPanel.DefaultTranslate = new Vector3();

                btnSaveCam.Text = "Save Camera";
            }
        }
        private void helpToolStripMenuItem_Click(object sender, EventArgs e) 
        { 
            //new ModelViewerHelp().Show(this); 
        }
        //Form popoutForm;
        private void detachViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (detachViewerToolStripMenuItem.Text == "Detach Viewer")
            //{
            //    //modelPanel1.Popout();
            //    popoutForm = new Form();
            //    Controls.Remove(modelPanel);
            //    popoutForm.Controls.Add(modelPanel);
            //    modelPanel.Dock = DockStyle.Fill;
            //    popoutForm.Show();
            //    detachViewerToolStripMenuItem.Text = "Attach Viewer";
            //    btnLeftToggle.Visible = false;
            //    btnRightToggle.Visible = false;
            //    btnOptionToggle.Visible = false;
            //    btnPlaybackToggle.Visible = false;
            //    spltRight.Visible = false;
            //    controlPanel.Visible = true;
            //    animEditors.Visible = true;
            //    leftPanel.Visible = true;
            //    pnlKeyframes.Visible = true;
            //    pnlPlayback.Parent = this;
            //    pnlPlayback.SendToBack();
            //    animEditors.SendToBack();
            //    pnlPlayback.Dock = DockStyle.Bottom;
            //    pnlKeyframes.Dock = DockStyle.Fill;
            //}
            //else
            //{
            //    modelPanel.Popin();
            //    detachViewerToolStripMenuItem.Text = "Detach Viewer";
            //}
        }

        #region Settings

        public unsafe void SaveSettings(bool maximize)
        {            
            try
            {
                BrawlBoxViewerSettings settings = new BrawlBoxViewerSettings();
                settings._tag = BrawlBoxViewerSettings.Tag;
                settings._version = 2;
                settings._defaultCam = modelPanel.DefaultTranslate;
                settings._defaultRot = modelPanel.DefaultRotate;
                settings._amb = modelPanel.Ambient;
                settings._pos = modelPanel.LightPosition;
                settings._diff = modelPanel.Diffuse;
                settings._spec = modelPanel.Specular;
                settings._yFov = modelPanel._fovY;
                settings._nearZ = modelPanel._nearZ;
                settings._farz = modelPanel._farZ;
                settings._tScale = modelPanel.TranslationScale;
                settings._rScale = modelPanel.RotationScale;
                settings._zScale = modelPanel.ZoomScale;
                settings._orbColor = (ARGBPixel)MDL0BoneNode.DefaultNodeColor;
                settings._lineColor = (ARGBPixel)MDL0BoneNode.DefaultBoneColor;
                settings._floorColor = (ARGBPixel)StaticMainWindow._floorHue;
                //settings.SetFlags1(
                //    syncAnimationsTogetherToolStripMenuItem.Checked,
                //    true,
                //    syncLoopToAnimationToolStripMenuItem.Checked,
                //    syncTexObjToolStripMenuItem.Checked,
                //    syncObjectsListToVIS0ToolStripMenuItem.Checked,
                //    disableBonesWhenPlayingToolStripMenuItem.Checked,
                //    maximize,
                //    btnSaveCam.Text == "Clear Camera");
                settings._undoCount = (uint)_allowedUndos;
                settings._shaderCount = 0;
                settings._matCount = 0;
                settings._emis = modelPanel.Emission;
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

                //if (BrawlLib.Properties.Settings.Default.External)
                //{
                //    using (FileStream stream = new FileStream(Application.StartupPath + "/brawlbox.settings", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 8, FileOptions.SequentialScan))
                //    {
                //        CompactStringTable s = new CompactStringTable();
                //        s.Add(ScreenCapBgLocText.Text);
                //        stream.SetLength((long)BrawlBoxViewerSettings.Size + s.TotalSize);
                //        using (FileMap map = FileMap.FromStream(stream))
                //        {
                //            *(BrawlBoxViewerSettings*)map.Address = settings;
                //            s.WriteTable(map.Address + BrawlBoxViewerSettings.Size);
                //            ((BrawlBoxViewerSettings*)map.Address)->_screenCapPathOffset = (uint)s[ScreenCapBgLocText.Text] - (uint)map.Address;
                //        }
                //    }
                //}
                //else
                //{
                //    BrawlLib.Properties.Settings.Default.ViewerSettings = settings;
                //    BrawlLib.Properties.Settings.Default.ScreenCapBgLocText = ScreenCapBgLocText.Text;
                //    BrawlLib.Properties.Settings.Default.Save();
                //}

                clearSavedSettingsToolStripMenuItem.Enabled = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private unsafe void saveCurrentSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool maximize = false;
            if (MessageBox.Show("When the viewer is opened, do you want it to automatically maximize?", "Maximize Viewer?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                maximize = true;

            SaveSettings(maximize);
        }

        #endregion

        private void alwaysSyncFrameCountsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (!_updating && alwaysSyncFrameCountsToolStripMenuItem.Checked == true)
            {
                _updating = true;
                displayFrameCountDifferencesToolStripMenuItem.Checked = false;
                _updating = false;
            }
        }

        private void clearSavedSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    if (BrawlLib.Properties.Settings.Default.External)
            //    {
            //        if (File.Exists(Application.StartupPath + "/brawlbox.settings"))
            //            File.Delete(Application.StartupPath + "/brawlbox.settings");
            //    }
            //    else
            //    {
            //        BrawlBoxViewerSettings v = BrawlLib.Properties.Settings.Default.ViewerSettings;
            //        v.UseModelViewerSettings = false;
            //        BrawlLib.Properties.Settings.Default.ViewerSettings = v;
            //    }
            //    clearSavedSettingsToolStripMenuItem.Enabled = false;
            //}
            //catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void chkPolygons_Click(object sender, EventArgs e)
        {
            chkPolygons.CheckState = chkPolygons.CheckState == CheckState.Checked ? CheckState.Indeterminate :
                                     chkPolygons.CheckState == CheckState.Indeterminate ? CheckState.Unchecked :
                                     CheckState.Checked;
        }

        //private void displayBRRESRelativeAnimationsToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    displayBRRESRelativeAnimationsToolStripMenuItem.CheckState = displayBRRESRelativeAnimationsToolStripMenuItem.CheckState == CheckState.Checked ? CheckState.Indeterminate :
        //                                                                 displayBRRESRelativeAnimationsToolStripMenuItem.CheckState == CheckState.Indeterminate ? CheckState.Unchecked :
        //                                                                 CheckState.Checked;
        //}
        private void syncTexObjToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            modelListsPanel1._syncObjTex = syncTexObjToolStripMenuItem.Checked;
            modelListsPanel1.UpdateTextures();
        }
        private void pnlOptions_CamResetClicked(object sender, EventArgs e) { modelPanel.ResetCamera(); }
        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loadImageToolStripMenuItem.Text == "Load Image")
            {
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "All Image Formats (*.png,*.tga,*.tif,*.tiff,*.bmp,*.jpg,*.jpeg,*.gif)|*.png;*.tga;*.tif;*.tiff;*.bmp;*.jpg;*.jpeg,*.gif|" +
                "Portable Network Graphics (*.png)|*.png|" +
                "Truevision TARGA (*.tga)|*.tga|" +
                "Tagged Image File Format (*.tif, *.tiff)|*.tif;*.tiff|" +
                "Bitmap (*.bmp)|*.bmp|" +
                "Jpeg (*.jpg,*.jpeg)|*.jpg;*.jpeg|" +
                "Gif (*.gif)|*.gif";
                d.Title = "Select an image to load";

                if (d.ShowDialog() == DialogResult.OK)
                    BGImage = Image.FromFile(d.FileName);

                loadImageToolStripMenuItem.Text = "Clear Image";
            }
            else
            {
                BGImage = null;
                loadImageToolStripMenuItem.Text = "Load Image";
            }
        }
        private void btnLeftToggle_Click(object sender, EventArgs e)
        {
            showLeft.Checked = !showLeft.Checked;
        }
        private void btnOptionToggle_Click(object sender, EventArgs e) 
        {
            showOptions.Checked = !showOptions.Checked;
        }
        private void btnPlaybackToggle_Click(object sender, EventArgs e) { showAnim.Checked = !showAnim.Checked; CheckDimensions(); }
        private void btnRightToggle_Click(object sender, EventArgs e)
        {
            showRight.Checked = !showRight.Checked;
        }

        #region Animation

        public void btnPrevFrame_Click(object sender, EventArgs e) { pnlPlayback.numFrameIndex.Value--; }
        public void btnNextFrame_Click(object sender, EventArgs e) { pnlPlayback.numFrameIndex.Value++; }
        public void btnPlay_Click(object sender, EventArgs e) { MovesetPanel.btnRunScript_Click(sender, e); }

        #endregion

        private void setColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dlgColor.ShowDialog(this) == DialogResult.OK)
                modelPanel.BackColor = ClearColor = dlgColor.Color;
        }
        protected override bool ProcessKeyPreview(ref Message m)
        {
            if (!modelPanel.Focused)
                return base.ProcessKeyPreview(ref m);

            if (m.Msg == 0x100)
            {
                Keys key = (Keys)m.WParam;
                if (key == Keys.PageUp)
                {
                    if (Ctrl)
                        pnlPlayback.btnLast_Click(this, null);
                    else
                        pnlPlayback.btnNextFrame_Click(this, null);
                    return true; 
                }
                else if (key == Keys.PageDown)
                {
                    if (Ctrl)
                        pnlPlayback.btnFirst_Click(this, null);
                    else
                        pnlPlayback.btnPrevFrame_Click(this, null);
                    return true;
                }
                else if (key == Keys.U)
                {
                    if (Ctrl)
                    {
                        modelPanel.ResetCamera();
                        return true;
                    }
                }
                else if (key == Keys.A)
                {
                    if (Ctrl)
                    {
                        ResetVertexColors();
                        _selectedVertices.Clear();
                        if (_targetModels != null)
                            foreach (MDL0Node mdl in _targetModels)
                                if (mdl._objList != null)
                                    foreach (MDL0ObjectNode o in mdl._objList)
                                        if (o._render)
                                            foreach (Vertex3 v in o._manager._vertices)
                                            {
                                                _selectedVertices.Add(v);
                                                v._selected = true;
                                                v._highlightColor = Color.Orange;
                                            }
                        modelPanel.Invalidate();
                    }
                    else
                    {
                        btnLeftToggle_Click(null, null);
                        return true;
                    }
                }
                else if (key == Keys.D)
                {
                    if (Control.ModifierKeys == (Keys.Control | Keys.Alt))
                        if (listPanel.Visible || animEditors.Visible || MovesetPanel.Visible || controlPanel.Visible)
                            showAnim.Checked = showLeft.Checked = showRight.Checked = showOptions.Checked = false;
                        else
                            showAnim.Checked = showLeft.Checked = showRight.Checked = showOptions.Checked = true;
                    else
                        btnRightToggle_Click(null, null);
                    return true;
                }
                else if (key == Keys.W)
                {
                    btnOptionToggle_Click(null, null);
                    return true;
                }
                else if (key == Keys.S)
                {
                    btnPlaybackToggle_Click(null, null);
                    return true;
                }
                else if (key == Keys.E)
                {
                    scaleToolStripMenuItem.PerformClick();
                    return true;
                }
                else if (key == Keys.R)
                {
                    rotationToolStripMenuItem.PerformClick();
                    return true;
                }
                else if (key == Keys.G)
                {
                    modelPanel.RefreshReferences();
                    return true;
                }
                else if (key == Keys.T)
                {
                    translationToolStripMenuItem.PerformClick();
                    return true;
                }
                else if (key == Keys.C)
                {
                    //Copy frame
                    if (Ctrl)
                        if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                        {
                            //We're copying the whole frame
                            if (_currentControl is CHR0Editor)
                            {
                                chr0Editor.btnCopyAll.PerformClick();
                                return true;
                            }
                        }
                        else
                        {
                            //We're copying the entry frame
                            if (_currentControl is CHR0Editor)
                            {
                                chr0Editor.btnCopy.PerformClick();
                                return true;
                            }
                        }
                }
                else if (key == Keys.V)
                {
                    //Paste frame
                    if (Ctrl)
                        if (Shift)
                            if (Alt)
                            {
                                //We're pasting only keyframes of the whole frame
                                if (_currentControl is CHR0Editor)
                                {
                                    chr0Editor._onlyKeys = true;
                                    chr0Editor.btnPasteAll.PerformClick(); 
                                    return true;
                                }
                            }
                            else
                            {
                                //We're pasting the whole frame
                                if (_currentControl is CHR0Editor)
                                {
                                    chr0Editor._onlyKeys = false;
                                    chr0Editor.btnPasteAll.PerformClick();
                                    return true;
                                }
                            }
                        else
                            if (Alt)
                            {
                                //We're pasting only keyframes of the entry frame
                                if (_currentControl is CHR0Editor)
                                {
                                    chr0Editor._onlyKeys = true;
                                    chr0Editor.btnPaste.PerformClick();
                                    return true;
                                }
                            }
                            else
                            {
                                //We're pasting the entry frame
                                if (_currentControl is CHR0Editor)
                                {
                                    chr0Editor._onlyKeys = false;
                                    chr0Editor.btnPaste.PerformClick();
                                    return true;
                                }
                            }
                }
                else if (key == Keys.Back)
                {
                    if (Ctrl)
                    {
                        //Clear all keyframes from frame
                        if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                        {
                            //We're removing the whole frame
                            if (_currentControl is CHR0Editor)
                            {
                                chr0Editor.btnClearAll.PerformClick();
                                return true;
                            }
                        }
                        else
                        {
                            //We're removing the entry frame
                            if (_currentControl is CHR0Editor)
                            {
                                chr0Editor.ClearEntry();
                                return true;
                            }
                        }
                    }
                    else if (ModifierKeys == Keys.Shift)
                    {
                        //Delete frame
                        if (_currentControl is CHR0Editor)
                        {
                            chr0Editor.btnDelete.PerformClick();
                            return true;
                        }
                    }
                }
                else if (key == Keys.P)
                {
                    chkPolygons.PerformClick();
                    return true;
                }
                else if (key == Keys.B)
                {
                    chkBones.PerformClick();
                    return true;
                }
                else if (key == Keys.F)
                {
                    chkFloor.PerformClick();
                    return true;
                }
                else if (key == Keys.I)
                {
                    if ((ModifierKeys & (Keys.Alt | Keys.Control)) == (Keys.Alt | Keys.Control))
                    {
                        btnExportToImgWithTransparency_Click(null, null);
                        return true;
                    }
                    else if ((ModifierKeys & (Keys.Shift | Keys.Control)) == (Keys.Shift | Keys.Control))
                    {
                        btnExportToImgNoTransparency_Click(null, null);
                        return true;
                    }
                }
                if (key == Keys.Z)
                {
                    if (Ctrl)
                    {
                        if (btnUndo.Enabled)
                            btnUndo_Click(null, null);

                        return true;
                    }
                }
                else if (key == Keys.Y)
                {
                    if (Ctrl)
                    {
                        if (btnRedo.Enabled)
                            btnRedo_Click(null, null);

                        return true;
                    }
                }
                else if (key == Keys.Escape)
                {
                    //Undo transformations, make sure to reset keyframes
                    if (_rotating)
                    {
                        _rotating = false;
                        chr0Editor.numRotX.Value = _oldAngles._x;
                        chr0Editor.numRotY.Value = _oldAngles._y;
                        chr0Editor.numRotZ.Value = _oldAngles._z;
                        chr0Editor.BoxChanged(chr0Editor.numRotX, null);
                        chr0Editor.BoxChanged(chr0Editor.numRotY, null);
                        chr0Editor.BoxChanged(chr0Editor.numRotZ, null);
                        modelPanel._forceNoSelection = false;
                    }
                    if (_translating)
                    {
                        _translating = false;
                        chr0Editor.numTransX.Value = _oldPosition._x;
                        chr0Editor.numTransY.Value = _oldPosition._y;
                        chr0Editor.numTransZ.Value = _oldPosition._z;
                        chr0Editor.BoxChanged(chr0Editor.numTransX, null);
                        chr0Editor.BoxChanged(chr0Editor.numTransY, null);
                        chr0Editor.BoxChanged(chr0Editor.numTransZ, null);
                        modelPanel._forceNoSelection = false;
                    }
                    if (_scaling)
                    {
                        _scaling = false;
                        chr0Editor.numScaleX.Value = _oldScale._x;
                        chr0Editor.numScaleY.Value = _oldScale._y;
                        chr0Editor.numScaleZ.Value = _oldScale._z;
                        chr0Editor.BoxChanged(chr0Editor.numScaleX, null);
                        chr0Editor.BoxChanged(chr0Editor.numScaleY, null);
                        chr0Editor.BoxChanged(chr0Editor.numScaleZ, null);
                        modelPanel._forceNoSelection = false;
                    }
                }
                else if (key == Keys.Space)
                {
                    btnPlay_Click(null, null);
                    //return true;
                }
            }
            return base.ProcessKeyPreview(ref m);
        }
        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "All Compatible Files (*.pac, *.pcs, *.brres, *.mrg, *.arc, *.szs,  *.mdl0)|*.pac;*.pcs;*.brres;*.mrg;*.arc;*.szs;*.mdl0";
            d.Title = "Select a file to open";
            if (d.ShowDialog() == DialogResult.OK)
                OpenFile(d.FileName);
        }

        private void toggleBonesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenderBones = !RenderBones;
        }

        private void togglePolygonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (togglePolygons.CheckState == CheckState.Checked)
            {
                togglePolygons.Checked = false;
                chkPolygons.CheckState = CheckState.Unchecked;
            }
            else
            {
                togglePolygons.Checked = true;
                chkPolygons.CheckState = CheckState.Checked;
            }
        }

        private void renderWireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chkPolygons.CheckState = CheckState.Indeterminate;
        }

        //private void openModelSwitherToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    new ModelSwitcher().ShowDialog(this, _targetModels);
        //}

        private void hideFromSceneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _resetCam = false;

            modelPanel.RemoveTarget(TargetModel);

            if (_targetModels != null && _targetModels.Count != 0)
                TargetModel = _targetModels[0];

            modelPanel.Invalidate();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _resetCam = false;

            modelPanel.RemoveTarget(TargetModel);
            _targetModels.Remove(TargetModel);
            comboCharacters.Items.Remove(TargetModel);

            if (_targetModels != null && _targetModels.Count != 0)
                TargetModel = _targetModels[0];

            modelPanel.Invalidate();
        }

        private void hideAllOtherModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (MDL0Node node in _targetModels)
                if (node != TargetModel)
                    modelPanel.RemoveTarget(node);

            modelPanel.Invalidate();
        }

        private void deleteAllOtherModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (MDL0Node node in _targetModels)
                if (node != TargetModel)
                {
                    _targetModels.Remove(node);
                    modelPanel.RemoveTarget(node);
                    comboCharacters.Items.Remove(node);
                }

            modelPanel.Invalidate();
        }
        private void modifyLightingToolStripMenuItem_Click(object sender, EventArgs e) { new ModelViewerSettingsDialog().Show(this); }
        private void hitboxesOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chkHitboxes.Checked = !chkHitboxes.Checked;

            hitboxesOffToolStripMenuItem.Checked = chkHitboxes.Checked;

            modelPanel.Invalidate();
        }
        private void hurtboxesOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chkHurtboxes.Checked = !chkHurtboxes.Checked;

            hurtboxesOffToolStripMenuItem.Checked = chkHurtboxes.Checked;

            modelPanel.Invalidate();
        }
        private void toggleFloor_Click(object sender, EventArgs e)
        {
            RenderFloor = !RenderFloor;
        }
        private void resetCameraToolStripMenuItem_Click_1(object sender, EventArgs e) { modelPanel.ResetCamera(); }
    }
}
