using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrawlLib.OpenGL;

namespace System.Windows.Forms
{
    public class DynamicModelPanel : UserControl
    {
        private ModelPanel[] _panels;

        public event GLRenderEventHandler PreRender;
        public event GLRenderEventHandler PostRender;

        public DynamicModelPanel()
        {
            InitializeComponent();
        }

        void p_PreRender(GLPanel sender)
        {
            if (PreRender != null)
                PreRender(sender);
        }

        void p_PostRender(GLPanel sender)
        {
            if (PostRender != null)
                PostRender(sender);
        }

        ~DynamicModelPanel()
        {
            foreach (ModelPanel p in _panels)
            {
                p.GotFocus -= panel_GotFocus;
                p.LostFocus -= panel_LostFocus;
                p.PreRender -= p_PreRender;
                p.PostRender -= p_PostRender;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _front = new ModelPanel();
            _perspective = new ModelPanel();
            _left = new ModelPanel();
            _top = new ModelPanel();

            _panels = new ModelPanel[]
            {
                _perspective,
                _top,
                _left,
                _front
            };

            foreach (ModelPanel p in _panels)
            {
                p.GotFocus += panel_GotFocus;
                p.LostFocus += panel_LostFocus;
                p.PreRender += p_PreRender;
                p.PostRender += p_PostRender;

                p.Dock = DockStyle.Fill;
            }

            _front.Name = "Front";
            _perspective.Name = "Perspective";
            _left.Name = "Left";
            _top.Name = "Top";

            BottomLeft.Controls.Add(_left);
            BottomRight.Controls.Add(_perspective);
            TopLeft.Controls.Add(_top);
            TopRight.Controls.Add(_front);
        }

        void panel_LostFocus(object sender, EventArgs e)
        {
            //Don't ever want the selected panel to be null
            //if (_selectedPanel == sender as ModelPanel)
            //    SetSelectedPanel(null);
        }

        void panel_GotFocus(object sender, EventArgs e)
        {
            SetSelectedPanel(sender as ModelPanel);
        }

        #region Designer

        private Panel BottomRight;
        private Panel BottomLeft;
        private Panel TopPanel;
        private Splitter splitVerticalTop;
        private Panel BottomPanel;
        private Splitter splitVerticalBottom;
        private Splitter splitHorizontal;
        private ModelPanel _perspective;
        private ModelPanel _front;
        private ModelPanel _top;
        private ModelPanel _left;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DynamicModelPanel));
            this.TopRight = new System.Windows.Forms.Panel();
            
            this.TopLeft = new System.Windows.Forms.Panel();
            
            this.BottomRight = new System.Windows.Forms.Panel();
          
            this.BottomLeft = new System.Windows.Forms.Panel();

            this.TopPanel = new System.Windows.Forms.Panel();
            this.splitVerticalTop = new System.Windows.Forms.Splitter();
            this.BottomPanel = new System.Windows.Forms.Panel();
            this.splitVerticalBottom = new System.Windows.Forms.Splitter();
            this.splitHorizontal = new System.Windows.Forms.Splitter();
            this.TopRight.SuspendLayout();
            this.TopLeft.SuspendLayout();
            this.BottomRight.SuspendLayout();
            this.BottomLeft.SuspendLayout();
            this.TopPanel.SuspendLayout();
            this.BottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // TopRight
            // 

            this.TopRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TopRight.Location = new System.Drawing.Point(253, 0);
            this.TopRight.Name = "TopRight";
            this.TopRight.Size = new System.Drawing.Size(247, 250);
            this.TopRight.TabIndex = 1;
            
            // 
            // TopLeft
            // 

            this.TopLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.TopLeft.Location = new System.Drawing.Point(0, 0);
            this.TopLeft.Name = "TopLeft";
            this.TopLeft.Size = new System.Drawing.Size(250, 250);
            this.TopLeft.TabIndex = 2;
            // 
            // _top
            // 

            // 
            // BottomRight
            // 
            this.BottomRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BottomRight.Location = new System.Drawing.Point(253, 0);
            this.BottomRight.Name = "BottomRight";
            this.BottomRight.Size = new System.Drawing.Size(247, 247);
            this.BottomRight.TabIndex = 2;
            // 
            // _perspective
            // 
            
            // BottomLeft
            // 
            this.BottomLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.BottomLeft.Location = new System.Drawing.Point(0, 0);
            this.BottomLeft.Name = "BottomLeft";
            this.BottomLeft.Size = new System.Drawing.Size(250, 247);
            this.BottomLeft.TabIndex = 2;
            // 
            // _left
            // 

            // 
            // Top
            // 
            this.TopPanel.Controls.Add(this.TopRight);
            this.TopPanel.Controls.Add(this.splitVerticalTop);
            this.TopPanel.Controls.Add(this.TopLeft);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopPanel.Location = new System.Drawing.Point(0, 0);
            this.TopPanel.Name = "Top";
            this.TopPanel.Size = new System.Drawing.Size(500, 250);
            this.TopPanel.TabIndex = 0;
            // 
            // splitVerticalTop
            // 
            this.splitVerticalTop.Location = new System.Drawing.Point(250, 0);
            this.splitVerticalTop.Name = "splitVerticalTop";
            this.splitVerticalTop.Size = new System.Drawing.Size(3, 250);
            this.splitVerticalTop.TabIndex = 3;
            this.splitVerticalTop.TabStop = false;
            this.splitVerticalTop.SplitterMoving += new System.Windows.Forms.SplitterEventHandler(this.splitVerticalTop_SplitterMoving);
            // 
            // Bottom
            // 
            this.BottomPanel.Controls.Add(this.BottomRight);
            this.BottomPanel.Controls.Add(this.splitVerticalBottom);
            this.BottomPanel.Controls.Add(this.BottomLeft);
            this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BottomPanel.Location = new System.Drawing.Point(0, 253);
            this.BottomPanel.Name = "Bottom";
            this.BottomPanel.Size = new System.Drawing.Size(500, 247);
            this.BottomPanel.TabIndex = 1;
            // 
            // splitVerticalBottom
            // 
            this.splitVerticalBottom.Location = new System.Drawing.Point(250, 0);
            this.splitVerticalBottom.Name = "splitVerticalBottom";
            this.splitVerticalBottom.Size = new System.Drawing.Size(3, 247);
            this.splitVerticalBottom.TabIndex = 2;
            this.splitVerticalBottom.TabStop = false;
            this.splitVerticalBottom.SplitterMoving += new System.Windows.Forms.SplitterEventHandler(this.splitVerticalBottom_SplitterMoving);
            // 
            // splitHorizontal
            // 
            this.splitHorizontal.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitHorizontal.Location = new System.Drawing.Point(0, 250);
            this.splitHorizontal.Name = "splitHorizontal";
            this.splitHorizontal.Size = new System.Drawing.Size(500, 3);
            this.splitHorizontal.TabIndex = 0;
            this.splitHorizontal.TabStop = false;
            // 
            // DynamicModelPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BottomPanel);
            this.Controls.Add(this.splitHorizontal);
            this.Controls.Add(this.TopPanel);
            this.Name = "DynamicModelPanel";
            this.Size = new System.Drawing.Size(500, 500);
            this.Resize += new System.EventHandler(this.DynamicModelPanel_Resize);
            this.TopRight.ResumeLayout(false);
            this.TopLeft.ResumeLayout(false);
            this.BottomRight.ResumeLayout(false);
            this.BottomLeft.ResumeLayout(false);
            this.TopPanel.ResumeLayout(false);
            this.BottomPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel TopRight;
        private Panel TopLeft;

        #endregion

        public ModelPanel SelectedPanel { get { return _selectedPanel == null ? _selectedPanel = _perspective : _selectedPanel; } }

        public event EventHandler SelectedPanelChanged;

        public void SetSelectedPanel(ModelPanel p)
        {
            _selectedPanel = p;

            if (SelectedPanelChanged != null)
                SelectedPanelChanged(this, EventArgs.Empty);
        }

        ModelPanel _selectedPanel = null;

        Size _previousSize = new Size(500, 500);

        private void DynamicModelPanel_Resize(object sender, EventArgs e)
        {


            _previousSize = Size;
        }

        bool _updating = false;

        private void splitVerticalTop_SplitterMoving(object sender, SplitterEventArgs e)
        {
            if (_updating)
                return;

            _updating = true;

            splitVerticalBottom.SplitPosition = e.SplitX;

            _updating = false;
        }

        private void splitVerticalBottom_SplitterMoving(object sender, SplitterEventArgs e)
        {
            if (_updating)
                return;

            _updating = true;

            splitVerticalTop.SplitPosition = e.SplitX;

            _updating = false;
        }

        //new void Invalidate()
        //{
        //    base.Invalidate();

            
        //}

        internal void RefreshReferences()
        {
            foreach (ModelPanel p in _panels)
                p.RefreshReferences();
        }
    }
}
