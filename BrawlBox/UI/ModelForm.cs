using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrawlLib.OpenGL;
using BrawlLib.Modeling;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.IO;
using System.IO;
using System.Drawing;
using BrawlLib.Properties;
using System.Runtime.InteropServices;

namespace BrawlBox
{
    class ModelForm : Form
    {
        #region Designer

        private ModelEditControl modelEditControl1;
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelForm));
            this.modelEditControl1 = new System.Windows.Forms.ModelEditControl();
            this.SuspendLayout();
            // 
            // modelEditControl1
            // 
            this.modelEditControl1.AllowDrop = true;
            this.modelEditControl1.BackColor = System.Drawing.Color.Lavender;
            this.modelEditControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modelEditControl1.ImgExtIndex = 0;
            this.modelEditControl1.Location = new System.Drawing.Point(0, 0);
            this.modelEditControl1.Name = "modelEditControl1";
            this.modelEditControl1.Size = new System.Drawing.Size(639, 528);
            this.modelEditControl1.TabIndex = 0;
            this.modelEditControl1.TargetAnimation = null;
            this.modelEditControl1.TargetAnimType = System.Windows.Forms.AnimType.CHR;
            this.modelEditControl1.TargetModelChanged += new System.EventHandler(this.TargetModelChanged);
            this.modelEditControl1.ModelViewerChanged += new System.EventHandler(this.ModelViewerChanged);
            // 
            // ModelForm
            // 
            this.BackColor = System.Drawing.Color.PowderBlue;
            this.ClientSize = new System.Drawing.Size(639, 528);
            this.Controls.Add(this.modelEditControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ModelForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ModelForm_FormClosing);
            this.ResumeLayout(false);

        }
        #endregion

        public ModelForm() { InitializeComponent(); }

        private List<MDL0Node> _models = new List<MDL0Node>();

        public DialogResult ShowDialog(List<MDL0Node> models) { return ShowDialog(null, models); }
        public DialogResult ShowDialog(IWin32Window owner, List<MDL0Node> models)
        {
            _models = models;
            try { return base.ShowDialog(owner); }
            finally { _models = null; }
        }
        public DialogResult ShowDialog(MDL0Node model) { return ShowDialog(null, model); }
        public DialogResult ShowDialog(IWin32Window owner, MDL0Node model)
        {
            _models.Add(model);
            try { return base.ShowDialog(owner); }
            finally { _models = null; }
        }

        public void Show(List<MDL0Node> models) { Show(null, models); }
        public void Show(IWin32Window owner, List<MDL0Node> models)
        {
            _models = models;
            base.Show(owner);
        }
        public void Show(MDL0Node model) { Show(null, model); }
        public void Show(IWin32Window owner, MDL0Node model)
        {
            _models.Add(model);
            base.Show(owner);
        }

        public unsafe void ReadSettings()
        {
            string t = BrawlBox.Properties.Settings.Default.ScreenCapBgLocText;
            if (!String.IsNullOrEmpty(t))
                modelEditControl1.ScreenCapBgLocText.Text = t;
            else
                modelEditControl1.ScreenCapBgLocText.Text = Application.StartupPath + "\\ScreenCaptures";

            BrawlBoxViewerSettings? s;

            if (!BrawlBox.Properties.Settings.Default.ViewerSettingsSet)
            {
                s = BrawlBoxViewerSettings.Default;
                BrawlBox.Properties.Settings.Default.ViewerSettingsSet = true;
            }
            else
                s = BrawlBox.Properties.Settings.Default.ViewerSettings;

            if (s == null)
                return;

            BrawlBoxViewerSettings settings = (BrawlBoxViewerSettings)s;
            modelEditControl1.DistributeSettings(settings);
            modelEditControl1.ModelPanel.ResetCamera();

            if (settings.Maximize)
                WindowState = FormWindowState.Maximized;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_models.Count != 0)
            {
                for (int i = 0; i < _models.Count; i++)
                    if (_models[i] != null)
                        modelEditControl1.AppendTarget(_models[i]);
                modelEditControl1.TargetModel = _models[0];
                modelEditControl1.ResetBoneColors();
            }
            else
                modelEditControl1.TargetModel = null;

            ReadSettings();
            modelEditControl1.ModelPanel.Capture();

            GenericWrapper._modelViewerOpen = true;
            MainForm.Instance.Visible = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            MainForm.Instance.Visible = true;
            GenericWrapper._modelViewerOpen = false;
            MainForm.Instance.modelPanel1.Capture();
            MainForm.Instance.resourceTree_SelectionChanged(this, null);
            MainForm.Instance.Refresh();
        }

        private void ModelForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!(e.Cancel = !modelEditControl1.Close()))
            {
                try
                {
                    if (modelEditControl1.TargetModel != null)
                        modelEditControl1.TargetModel = null;

                    if (modelEditControl1._targetModels != null)
                        modelEditControl1._targetModels = null;

                    modelEditControl1.ModelPanel.ClearAll();
                    modelEditControl1.models.Items.Clear();
                }
                catch { }
            }
        }

        private void ModelViewerChanged(object sender, EventArgs e)
        {
            //Ain't nobody got time fo dis fancy mdi stuff
            if (modelEditControl1.ModelViewerForm != null)
            {
                //IsMdiContainer = true;
                //modelEditControl1.ModelViewerForm.MdiParent = this;
                Application.AddMessageFilter(mouseMessageFilter = new MouseMoveMessageFilter() { TargetForm = this });
            }
            else
            {
                //IsMdiContainer = false;
                Application.RemoveMessageFilter(mouseMessageFilter);
            }
        }

        private void TargetModelChanged(object sender, EventArgs e)
        {
            if (modelEditControl1.TargetModel != null)
                Text = String.Format("{1} - Advanced Model Editor - {0}", modelEditControl1.TargetModel.Name, Program.AssemblyTitle);
            else
                Text = Text = Program.AssemblyTitle + " - Advanced Model Editor";
        }

        private MouseMoveMessageFilter mouseMessageFilter;
        class MouseMoveMessageFilter : IMessageFilter
        {
            public ModelForm TargetForm { get; set; }
            bool _mainWindowFocused = false;
            public bool PreFilterMessage(ref Message m)
            {
                int numMsg = m.Msg;
                if (numMsg == 0x0200) //WM_MOUSEMOVE
                {
                    if (TargetForm.modelEditControl1.ModelViewerForm != null)
                    if (InForm(TargetForm.modelEditControl1.ModelViewerForm, Control.MousePosition))
                    {
                        if (_mainWindowFocused)
                        {
                            TargetForm.modelEditControl1.ModelViewerForm.Focus();
                            _mainWindowFocused = false;
                        }
                    }
                    else if (InForm(TargetForm, Control.MousePosition))
                    {
                        if (!_mainWindowFocused)
                        {
                            TargetForm.Focus();
                            _mainWindowFocused = true;
                        }
                    }
                }
                return false;
            }
            private bool InForm(Form f, Point screenPoint)
            {
                Point p = f.PointToClient(screenPoint);
                return p.X > 0 && p.X < f.Size.Width && p.Y > 0 && p.Y < f.Size.Height;
            }
        }
    }
}
