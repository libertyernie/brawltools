using BrawlLib.Modeling;
using BrawlLib.OpenGL;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBBTypes;
using Gif.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace System.Windows.Forms
{
    public partial class ModelEditorBase : UserControl
    {
        #region Constructor

        protected void PreConstruct()
        {
            srt0Editor._mainWindow =
            shp0Editor._mainWindow =
            pat0Editor._mainWindow =
            vis0Editor._mainWindow =
            scn0Editor._mainWindow =
            clr0Editor._mainWindow =
            chr0Editor._mainWindow =
            pnlPlayback._mainWindow =
            this;

            _interpolationEditor = new Forms.InterpolationEditor(this);
        }

        protected void PostConstruct()
        {
            if (pnlPlayback.Width <= pnlPlayback.MinimumSize.Width)
            {
                pnlPlayback.Dock = DockStyle.Left;
                pnlPlayback.Width = pnlPlayback.MinimumSize.Width;
            }
            else
                pnlPlayback.Dock = DockStyle.Fill;

            _timer = new CoolTimer();
            _timer.RenderFrame += _timer_RenderFrame;

            modelPanel.PreRender += (EventPreRender = new GLRenderEventHandler(this.modelPanel1_PreRender));
            modelPanel.PostRender += (EventPostRender = new GLRenderEventHandler(this.modelPanel1_PostRender));
            modelPanel.MouseDown += (EventMouseDown = new System.Windows.Forms.MouseEventHandler(this.modelPanel1_MouseDown));
            modelPanel.MouseMove += (EventMouseMove = new System.Windows.Forms.MouseEventHandler(this.modelPanel1_MouseMove));
            modelPanel.MouseUp += (EventMouseUp = new System.Windows.Forms.MouseEventHandler(this.modelPanel1_MouseUp));

            KeyframePanel.visEditor.EntryChanged += new EventHandler(VISEntryChanged);
            KeyframePanel.visEditor.IndexChanged += new EventHandler(VISIndexChanged);

            InitHotkeyList();

            _hotKeys = new Dictionary<Keys, Func<bool>>();
            foreach (HotKeyInfo key in _hotkeyList)
                _hotKeys.Add(key.KeyCode, key._function);
        }

        public virtual void LinkModelPanel(ModelPanel p)
        {
            p.PreRender += EventPreRender;
            p.PostRender += EventPostRender;
            p.MouseDown += EventMouseDown;
            p.MouseMove += EventMouseMove;
            p.MouseUp += EventMouseUp;
        }
        public virtual void UnlinkModelPanel(ModelPanel p)
        {
            p.PreRender -= EventPreRender;
            p.PostRender -= EventPostRender;
            p.MouseDown -= EventMouseDown;
            p.MouseMove -= EventMouseMove;
            p.MouseUp -= EventMouseUp;
        }

        public virtual void OnModelPanelChanged()
        {
            if (ModelViewerChanged != null)
                ModelViewerChanged(this, null);
        }

        #endregion

        #region Models
        public virtual void AppendTarget(IModel model)
        {
            if (!_targetModels.Contains(model))
                _targetModels.Add(model);

            ModelPanel.AddTarget(model);
            model.ResetToBindState();
        }

        protected virtual void ModelChanged(IModel newModel)
        {
            if (newModel != null && !_targetModels.Contains(newModel))
                _targetModels.Add(newModel);

            if (_targetModel != null)
                _targetModel.IsTargetModel = false;

            if ((_targetModel = newModel) != null)
            {
                ModelPanel.AddTarget(_targetModel);
                _targetModel.IsTargetModel = true;
                ResetVertexColors();
            }
            else
                EditingAll = true; //No target model so all is the only option

            if (_resetCamera)
            {
                ModelPanel.ResetCamera();
                SetFrame(0);
            }
            else
                _resetCamera = true;

            OnModelChanged();

            if (TargetModelChanged != null)
                TargetModelChanged(this, null);
        }

        protected virtual void OnModelChanged() { }
        protected virtual void OnSelectedVerticesChanged()
        {
            //Force the average vertex location to be recalculated
            _vertexLoc = null;
        }
        protected virtual void OnSelectedBoneChanged() { }

        #endregion

        #region Viewer Background

        public void setColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dlgColor.ShowDialog(this) == DialogResult.OK)
                ModelPanel.BackColor = ClearColor = dlgColor.Color;
        }

        public virtual bool BackgroundImageLoaded { get { return BGImage != null; } set { if (!value) BGImage = null; } }

        public void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BGImage == null)
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
            }
            else
                BGImage = null;
        }

        #endregion

        public virtual void SaveSettings() { }
        public virtual void SetDefaultSettings() { }

        private void RenderToGIF(List<Image> images, string path)
        {
            if (String.IsNullOrEmpty(path))
                return;

            string outPath = "";
            try
            {
                outPath = path;
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

            AnimatedGifEncoder e = new AnimatedGifEncoder();
            e.Start(outPath);
            e.SetDelay(1000 / (int)pnlPlayback.numFPS.Value);
            e.SetRepeat(0);
            e.SetQuality(1);
            using (ProgressWindow progress = new ProgressWindow(this, "GIF Encoder", "Encoding, please wait...", true))
            {
                progress.TopMost = true;
                progress.Begin(0, images.Count, 0);
                for (int i = 0, count = images.Count; i < count; i++)
                {
                    if (progress.Cancelled)
                        break;

                    e.AddFrame(images[i]);
                    progress.Update(progress.CurrentValue + 1);
                }
                progress.Finish();
                e.Finish();
            }

            MessageBox.Show("Animated GIF successfully saved to " + outPath.Replace("\\", "/"));
        }
        protected void SaveBitmap(Bitmap bmp, string path, string extension)
        {
            if (String.IsNullOrEmpty(path))
                path = Application.StartupPath + "\\ScreenCaptures";

            if (String.IsNullOrEmpty(extension))
                extension = ".png";

            if (!String.IsNullOrEmpty(path) && !String.IsNullOrEmpty(extension))
            {
                try
                {
                    string outPath = path;
                    if (!Directory.Exists(outPath))
                        Directory.CreateDirectory(outPath);

                    DirectoryInfo dir = new DirectoryInfo(outPath);
                    FileInfo[] files = dir.GetFiles();
                    int i = 0;
                    string name = "ScreenCapture";
                Top:
                    foreach (FileInfo f in files)
                        if (f.Name == name + i + extension)
                        {
                            i++;
                            goto Top;
                        }
                outPath += "\\" + name + i + extension;
                    bool okay = true;
                    if (extension.Equals(".png"))
                        bmp.Save(outPath, ImageFormat.Png);
                    else if (extension.Equals(".tga"))
                        bmp.SaveTGA(outPath);
                    else if (extension.Equals(".tiff") || extension.Equals(".tif"))
                        bmp.Save(outPath, ImageFormat.Tiff);
                    else if (extension.Equals(".bmp"))
                        bmp.Save(outPath, ImageFormat.Bmp);
                    else if (extension.Equals(".jpg") || outPath.EndsWith(".jpeg"))
                        bmp.Save(outPath, ImageFormat.Jpeg);
                    else if (extension.Equals(".gif"))
                        bmp.Save(outPath, ImageFormat.Gif);
                    else { okay = false; }
                    if (okay)
                        MessageBox.Show("Screenshot successfully saved to " + outPath.Replace("\\", "/"));
                }
                catch { }
            }
            bmp.Dispose();
        }

        public void OnDragEnter(object sender, DragEventArgs e)
        {
            if (_openFileDelegate == null)
                return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        public void OnDragDrop(object sender, DragEventArgs e)
        {
            if (_openFileDelegate == null)
                return;

            Array a = (Array)e.Data.GetData(DataFormats.FileDrop);
            if (a != null)
            {
                string s = null;
                for (int i = 0; i < a.Length; i++)
                {
                    s = a.GetValue(i).ToString();
                    this.BeginInvoke(_openFileDelegate, new Object[] { s });
                }
            }
        }
    }
}