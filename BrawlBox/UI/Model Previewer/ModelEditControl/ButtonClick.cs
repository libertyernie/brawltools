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
    public partial class ModelEditControl : UserControl, IMainWindow
    {
        private void chkBones_Click(object sender, EventArgs e) { RenderBones = !RenderBones; }
        private void toggleBones_Click(object sender, EventArgs e) { RenderBones = !RenderBones; }

        private void chkPolygons_Click(object sender, EventArgs e) { RenderPolygons = !RenderPolygons; }
        private void togglePolygons_Click(object sender, EventArgs e) { RenderPolygons = !RenderPolygons; }

        private void chkVertices_Click(object sender, EventArgs e) { RenderVertices = !RenderVertices; }
        private void toggleVertices_Click(object sender, EventArgs e) { RenderVertices = !RenderVertices; }

        private void chkCollisions_Click(object sender, EventArgs e) { RenderCollisions = !RenderCollisions; }
        private void toggleCollisions_Click(object sender, EventArgs e) { RenderCollisions = !RenderCollisions; }

        private void chkFloor_Click(object sender, EventArgs e) { RenderFloor = !RenderFloor; }
        private void toggleFloor_Click(object sender, EventArgs e) { RenderFloor = !RenderFloor; }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e) { RenderWireframe = !RenderWireframe; }
        private void toggleNormals_Click(object sender, EventArgs e) { RenderNormals = !RenderNormals; }
        private void boundingBoxToolStripMenuItem_Click(object sender, EventArgs e) { RenderBox = !RenderBox; }

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
        private void SaveBitmap(Bitmap bmp)
        {
            Begin:
            if (!String.IsNullOrEmpty(ScreenCapBgLocText.Text) && !String.IsNullOrEmpty(_imgExt))
            {
                try
                {
                    string outPath = ScreenCapBgLocText.Text;
                    if (!Directory.Exists(outPath))
                        Directory.CreateDirectory(outPath);

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
            else
            {
                if (String.IsNullOrEmpty(ScreenCapBgLocText.Text))
                    ScreenCapBgLocText.Text = Application.StartupPath + "\\ScreenCaptures";
                if (String.IsNullOrEmpty(_imgExt))
                    _imgExt = ".png";
                goto Begin;
            }
            bmp.Dispose();
        }
        private void btnExportToImgWithTransparency_Click(object sender, EventArgs e)
        {
            SaveBitmap(ModelPanel.GrabScreenshot(true));
        }
        private void btnExportToImgNoTransparency_Click(object sender, EventArgs e)
        {
            SaveBitmap(ModelPanel.GrabScreenshot(false));
        }

        bool _capture = false;
        List<Image> images = new List<Image>();
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

        private void RenderToGIF(Image[] images)
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
                    string name = "BrawlboxAnimation";
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

                //_viewerForm.modelPanel1._settings = modelPanel._settings;
                //_viewerForm.modelPanel1._camera = modelPanel._camera;

                _viewerForm.FormClosed += _viewerForm_FormClosed;

                modelPanel.PreRender -= EventPreRender;
                modelPanel.PostRender -= EventPostRender;
                modelPanel.MouseDown -= EventMouseDown;
                modelPanel.MouseMove -= EventMouseMove;
                modelPanel.MouseUp -= EventMouseUp;

                _viewerForm.modelPanel1.PreRender += EventPreRender;
                _viewerForm.modelPanel1.PostRender += EventPostRender;
                _viewerForm.modelPanel1.MouseDown += EventMouseDown;
                _viewerForm.modelPanel1.MouseMove += EventMouseMove;
                _viewerForm.modelPanel1.MouseUp += EventMouseUp;
                _viewerForm.modelPanel1.EventProcessKeyMessage += ProcessKeyPreview;

                if (ModelViewerChanged != null)
                    ModelViewerChanged(this, null);

                _viewerForm.Show();
                _viewerForm.modelPanel1.Invalidate();

                _interpolationEditor.Visible = true;
                interpolationEditorToolStripMenuItem.Checked = false;
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

            _viewerForm.modelPanel1.PreRender -= EventPreRender;
            _viewerForm.modelPanel1.PostRender -= EventPostRender;
            _viewerForm.modelPanel1.MouseDown -= EventMouseDown;
            _viewerForm.modelPanel1.MouseMove -= EventMouseMove;
            _viewerForm.modelPanel1.MouseUp -= EventMouseUp;
            _viewerForm.modelPanel1.EventProcessKeyMessage -= ProcessKeyPreview;

            modelPanel.PreRender += EventPreRender;
            modelPanel.PostRender += EventPostRender;
            modelPanel.MouseDown += EventMouseDown;
            modelPanel.MouseMove += EventMouseMove;
            modelPanel.MouseUp += EventMouseUp;

            _viewerForm = null;
            _interpolationEditor.Visible = false;
            interpolationEditorToolStripMenuItem.Enabled = true;

            if (ModelViewerChanged != null)
                ModelViewerChanged(this, null);
        }

        #endregion

        #region Settings Management

        private void clearSavedSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrawlBox.Properties.Settings.Default.ViewerSettings = null;
            BrawlBox.Properties.Settings.Default.ScreenCapBgLocText = null;
            BrawlBox.Properties.Settings.Default.ViewerSettingsSet = false;
            SetDefaultSettings();
        }

        private unsafe void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrawlBoxViewerSettings settings = CollectSettings();

            SaveFileDialog sd = new SaveFileDialog();
            sd.Filter = "Brawlbox Settings (*.settings)|*.settings";
            sd.FileName = Application.StartupPath;
            if (sd.ShowDialog() == DialogResult.OK)
            {
                string path = sd.FileName;
                using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 8, FileOptions.SequentialScan))
                {
                    CompactStringTable s = new CompactStringTable();
                    s.Add(ScreenCapBgLocText.Text);
                    stream.SetLength((long)BrawlBoxViewerSettings.Size + s.TotalSize);
                    using (FileMap map = FileMap.FromStream(stream))
                    {
                        *(BrawlBoxViewerSettings*)map.Address = settings;
                        s.WriteTable(map.Address + BrawlBoxViewerSettings.Size);
                        ((BrawlBoxViewerSettings*)map.Address)->_screenCapPathOffset = (uint)s[ScreenCapBgLocText.Text] - (uint)map.Address;
                    }
                }
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
                using (FileMap map = FileMap.FromFile(path, FileMapProtect.Read))
                {
                    if (*(uint*)map.Address == BrawlBoxViewerSettings.Tag)
                    {
                        BrawlBoxViewerSettings* settings = (BrawlBoxViewerSettings*)map.Address;
                        DistributeSettings(*settings);
                        ScreenCapBgLocText.Text = new String((sbyte*)map.Address + settings->_screenCapPathOffset);
                    }
                }
            }
        }

        #endregion

        #region Menu Buttons
        private void btnSaveCam_Click(object sender, EventArgs e)
        {
            if (btnSaveCam.Text == "Save Camera")
            {
                ModelPanel._settings._defaultRotate = new Vector2(ModelPanel.Camera._rotation._x, ModelPanel.Camera._rotation._y);
                ModelPanel._settings._defaultTranslate = ModelPanel.Camera._matrixInverse.Multiply(new Vector3());

                btnSaveCam.Text = "Clear Camera";
            }
            else
            {
                ModelPanel._settings._defaultRotate = new Vector2();
                ModelPanel._settings._defaultTranslate = new Vector3();

                btnSaveCam.Text = "Save Camera";
            }
        }

        #endregion

        #region Viewer Background

        private void setColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dlgColor.ShowDialog(this) == DialogResult.OK)
                ModelPanel.BackColor = ClearColor = dlgColor.Color;
        }

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

        #endregion

        #region Panel Toggles

        private void btnLeftToggle_Click(object sender, EventArgs e) { showLeft.Checked = !showLeft.Checked; }
        private void btnTopToggle_Click(object sender, EventArgs e) { showTop.Checked = !showTop.Checked; }
        private void btnBottomToggle_Click(object sender, EventArgs e) { showBottom.Checked = !showBottom.Checked; CheckDimensions(); }
        private void btnRightToggle_Click(object sender, EventArgs e) { showRight.Checked = !showRight.Checked; }

        #endregion

        #region Animation Buttons

        public void btnPrevFrame_Click(object sender, EventArgs e) { pnlPlayback.numFrameIndex.Value--; }
        public void btnNextFrame_Click(object sender, EventArgs e) { pnlPlayback.numFrameIndex.Value++; }
        public void btnPlay_Click(object sender, EventArgs e)
        {
            if (_timer.IsRunning)
                StopAnim();
            else
                PlayAnim();
        }

        #endregion

        #region Shortcut Keys

        protected override bool ProcessKeyPreview(ref Message m)
        {
            if (m.Msg == 0x100)
            {
                bool focused = ModelPanel.ContainsFocus;

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
                        ModelPanel.ResetCamera();
                        return true;
                    }
                }
                else if (key == Keys.A)
                {
                    if (Ctrl)
                    {
                        ResetVertexColors();
                        if (_targetModels != null)
                            foreach (IModel mdl in _targetModels)
                                if (mdl.SelectedObjectIndex >= 0 && mdl.SelectedObjectIndex < mdl.Objects.Length)
                                    foreach (Vertex3 v in ((IObject)mdl.Objects[mdl.SelectedObjectIndex]).PrimitiveManager._vertices)
                                    {
                                        _selectedVertices.Add(v);
                                        v._selected = true;
                                        v._highlightColor = Color.Orange;
                                    }
                                else
                                    foreach (IObject o in mdl.Objects)
                                        foreach (Vertex3 v in o.PrimitiveManager._vertices)
                                        {
                                            _selectedVertices.Add(v);
                                            v._selected = true;
                                            v._highlightColor = Color.Orange;
                                        }

                        //weightEditor.TargetVertices = _selectedVertices;
                        vertexEditor.TargetVertices = _selectedVertices;
                        ModelPanel.Invalidate();
                    }
                    else if (focused)
                    {
                        btnLeftToggle_Click(null, null);
                        return true;
                    }
                }
                else if (key == Keys.D)
                {
                    if (focused)
                    {
                        if (Control.ModifierKeys == (Keys.Control | Keys.Alt))
                            if (leftPanel.Visible || rightPanel.Visible || animEditors.Visible || controlPanel.Visible)
                                showBottom.Checked = showRight.Checked = showLeft.Checked = showTop.Checked = false;
                            else
                                showBottom.Checked = showRight.Checked = showLeft.Checked = showTop.Checked = true;
                        else
                            btnRightToggle_Click(null, null);
                        return true;
                    }
                }
                else if (key == Keys.W)
                {
                    if (focused)
                    {
                        btnTopToggle_Click(null, null);
                        return true;
                    }
                }
                else if (key == Keys.S)
                {
                    btnBottomToggle_Click(null, null);
                    return true;
                }
                else if (key == Keys.E)
                {
                    if (focused)
                    {
                        scaleToolStripMenuItem.PerformClick();
                        return true;
                    }
                }
                else if (key == Keys.R)
                {
                    if (focused)
                    {
                        rotationToolStripMenuItem.PerformClick();
                        return true;
                    }
                }
                else if (key == Keys.G)
                {
                    if (focused)
                    {
                        ModelPanel.RefreshReferences();
                        return true;
                    }
                }
                else if (key == Keys.T)
                {
                    if (focused)
                    {
                        translationToolStripMenuItem.PerformClick();
                        return true;
                    }
                }
                else if (key == Keys.C)
                {
                    if (focused)
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
                }
                else if (key == Keys.V)
                {
                    if (focused)
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
                        else
                        {
                            chkVertices.PerformClick();
                            return true;
                        }
                    }
                }
                else if (key == Keys.Back)
                {
                    if (focused)
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
                }
                else if (key == Keys.P)
                {
                    if (focused)
                    {
                        chkPolygons.PerformClick();
                        return true;
                    }
                }
                else if (key == Keys.B)
                {
                    if (focused)
                    {
                        chkBones.PerformClick();
                        return true;
                    }
                }
                else if (key == Keys.F)
                {
                    if (focused)
                    {
                        chkFloor.PerformClick();
                        return true;
                    }
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
                    }
                    ModelPanel.AllowSelection = true;
                }
                else if (key == Keys.Space)
                {
                    if (focused)
                    {
                        btnPlay_Click(null, null);
                        return true;
                    }
                }
                //Weight editor has been disabled due to the necessity 
                //of re-encoding objects after making influence changes.
                //else if (key == Keys.H)
                //{
                //    ToggleWeightEditor();
                //    return true;
                //}
                else if (key == Keys.J)
                {
                    if (focused)
                    {
                        ToggleVertexEditor();
                        return true;
                    }
                }
            }
            return base.ProcessKeyPreview(ref m);
        }

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
            if (SelectedBone != null)
            {
                ModelPanel.Camera.Reset();
                ModelPanel.Camera.Translate(SelectedBone.Matrix.GetPoint() + new Vector3(0.0f, 0.0f, 27.0f));
                ModelPanel.Invalidate();
            }
            else
                MessageBox.Show("Select a bone!");
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
                interpolationEditorToolStripMenuItem.Checked = true;
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
            else if(node is PAT0Node)
                (node as PAT0Node).Append();
            else if(node is VIS0Node)
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
