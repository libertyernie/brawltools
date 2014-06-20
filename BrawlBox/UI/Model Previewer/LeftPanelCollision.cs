using System;
using BrawlLib.SSBB.ResourceNodes;
using System.Drawing;
using BrawlLib.Modeling;
using System.ComponentModel;
using BrawlLib.OpenGL;
using BrawlLib;
using System.IO;
using System.Collections.Generic;

namespace System.Windows.Forms
{
    public class LeftPanelCollision : UserControl
    {
        #region Designer

        public CheckedListBox lstObjects;
        private CheckBox chkAllObj;
        private Button btnObjects;
        private ProxySplitter spltAnimObj;
        private Panel pnlAnims;
        private Button btnAnims;
        private Panel pnlTextures;
        private CheckedListBox lstTextures;
        private CheckBox chkAllTextures;
        private Button btnTextures;
        private ProxySplitter spltObjTex;
        private ContextMenuStrip ctxTextures;
        private ToolStripMenuItem sourceToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem replaceTextureToolStripMenuItem;
        private ToolStripMenuItem sizeToolStripMenuItem;
        private ToolStripMenuItem resetToolStripMenuItem;
        private ToolStripMenuItem renameTextureTextureToolStripMenuItem;
        private ToolStripMenuItem exportTextureToolStripMenuItem;
        private IContainer components;
        public CheckBox chkSyncVis;
        public ListView listAnims;
        private ColumnHeader nameColumn;
        public ComboBox fileType;
        private Panel panel1;
        private ContextMenuStrip ctxAnim;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripMenuItem renameToolStripMenuItem;
        public Button SaveAnims;
        public Button Load;
        private TransparentPanel overObjPnl;
        private TransparentPanel overTexPnl;
        private ToolStripMenuItem createNewToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem chkLoop;
        private Panel pnlObjects;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Animations", System.Windows.Forms.HorizontalAlignment.Left);
            this.pnlObjects = new System.Windows.Forms.Panel();
            this.overObjPnl = new System.Windows.Forms.TransparentPanel();
            this.lstObjects = new System.Windows.Forms.CheckedListBox();
            this.chkAllObj = new System.Windows.Forms.CheckBox();
            this.chkSyncVis = new System.Windows.Forms.CheckBox();
            this.btnObjects = new System.Windows.Forms.Button();
            this.pnlAnims = new System.Windows.Forms.Panel();
            this.listAnims = new System.Windows.Forms.ListView();
            this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.SaveAnims = new System.Windows.Forms.Button();
            this.Load = new System.Windows.Forms.Button();
            this.fileType = new System.Windows.Forms.ComboBox();
            this.btnAnims = new System.Windows.Forms.Button();
            this.ctxTextures = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.sourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameTextureTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlTextures = new System.Windows.Forms.Panel();
            this.overTexPnl = new System.Windows.Forms.TransparentPanel();
            this.lstTextures = new System.Windows.Forms.CheckedListBox();
            this.chkAllTextures = new System.Windows.Forms.CheckBox();
            this.btnTextures = new System.Windows.Forms.Button();
            this.spltObjTex = new System.Windows.Forms.ProxySplitter();
            this.spltAnimObj = new System.Windows.Forms.ProxySplitter();
            this.ctxAnim = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkLoop = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlObjects.SuspendLayout();
            this.pnlAnims.SuspendLayout();
            this.panel1.SuspendLayout();
            this.ctxTextures.SuspendLayout();
            this.pnlTextures.SuspendLayout();
            this.ctxAnim.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlObjects
            // 
            this.pnlObjects.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlObjects.Controls.Add(this.overObjPnl);
            this.pnlObjects.Controls.Add(this.lstObjects);
            this.pnlObjects.Controls.Add(this.chkAllObj);
            this.pnlObjects.Controls.Add(this.chkSyncVis);
            this.pnlObjects.Controls.Add(this.btnObjects);
            this.pnlObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlObjects.Location = new System.Drawing.Point(0, 182);
            this.pnlObjects.MinimumSize = new System.Drawing.Size(0, 21);
            this.pnlObjects.Name = "pnlObjects";
            this.pnlObjects.Size = new System.Drawing.Size(137, 150);
            this.pnlObjects.TabIndex = 0;
            // 
            // overObjPnl
            // 
            this.overObjPnl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.overObjPnl.Location = new System.Drawing.Point(0, 61);
            this.overObjPnl.Name = "overObjPnl";
            this.overObjPnl.Size = new System.Drawing.Size(135, 87);
            this.overObjPnl.TabIndex = 8;
            // 
            // lstObjects
            // 
            this.lstObjects.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstObjects.CausesValidation = false;
            this.lstObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstObjects.IntegralHeight = false;
            this.lstObjects.Location = new System.Drawing.Point(0, 61);
            this.lstObjects.Margin = new System.Windows.Forms.Padding(0);
            this.lstObjects.Name = "lstObjects";
            this.lstObjects.Size = new System.Drawing.Size(135, 87);
            this.lstObjects.TabIndex = 4;
            this.lstObjects.Leave += new System.EventHandler(this.lstObjects_Leave);
            // 
            // chkAllObj
            // 
            this.chkAllObj.Checked = true;
            this.chkAllObj.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllObj.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkAllObj.Location = new System.Drawing.Point(0, 41);
            this.chkAllObj.Margin = new System.Windows.Forms.Padding(0);
            this.chkAllObj.Name = "chkAllObj";
            this.chkAllObj.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
            this.chkAllObj.Size = new System.Drawing.Size(135, 20);
            this.chkAllObj.TabIndex = 5;
            this.chkAllObj.Text = "All";
            this.chkAllObj.UseVisualStyleBackColor = false;
            // 
            // chkSyncVis
            // 
            this.chkSyncVis.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkSyncVis.Location = new System.Drawing.Point(0, 21);
            this.chkSyncVis.Margin = new System.Windows.Forms.Padding(0);
            this.chkSyncVis.Name = "chkSyncVis";
            this.chkSyncVis.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
            this.chkSyncVis.Size = new System.Drawing.Size(135, 20);
            this.chkSyncVis.TabIndex = 7;
            this.chkSyncVis.Text = "Sync VIS0";
            this.chkSyncVis.UseVisualStyleBackColor = false;
            // 
            // btnObjects
            // 
            this.btnObjects.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnObjects.Location = new System.Drawing.Point(0, 0);
            this.btnObjects.Name = "btnObjects";
            this.btnObjects.Size = new System.Drawing.Size(135, 21);
            this.btnObjects.TabIndex = 6;
            this.btnObjects.Text = "Objects";
            this.btnObjects.UseVisualStyleBackColor = true;
            // 
            // pnlAnims
            // 
            this.pnlAnims.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAnims.Controls.Add(this.listAnims);
            this.pnlAnims.Controls.Add(this.panel1);
            this.pnlAnims.Controls.Add(this.btnAnims);
            this.pnlAnims.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAnims.Location = new System.Drawing.Point(0, 0);
            this.pnlAnims.MinimumSize = new System.Drawing.Size(0, 21);
            this.pnlAnims.Name = "pnlAnims";
            this.pnlAnims.Size = new System.Drawing.Size(137, 178);
            this.pnlAnims.TabIndex = 2;
            // 
            // listAnims
            // 
            this.listAnims.AutoArrange = false;
            this.listAnims.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn});
            this.listAnims.Cursor = System.Windows.Forms.Cursors.Default;
            this.listAnims.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup2.Header = "Animations";
            listViewGroup2.Name = "grpAnims";
            this.listAnims.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup2});
            this.listAnims.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listAnims.HideSelection = false;
            this.listAnims.Location = new System.Drawing.Point(0, 42);
            this.listAnims.MultiSelect = false;
            this.listAnims.Name = "listAnims";
            this.listAnims.Size = new System.Drawing.Size(135, 134);
            this.listAnims.TabIndex = 25;
            this.listAnims.UseCompatibleStateImageBehavior = false;
            this.listAnims.View = System.Windows.Forms.View.Details;
            // 
            // nameColumn
            // 
            this.nameColumn.Text = "Name";
            this.nameColumn.Width = 160;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.SaveAnims);
            this.panel1.Controls.Add(this.Load);
            this.panel1.Controls.Add(this.fileType);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 21);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(135, 21);
            this.panel1.TabIndex = 27;
            // 
            // SaveAnims
            // 
            this.SaveAnims.Location = new System.Drawing.Point(41, 0);
            this.SaveAnims.Name = "SaveAnims";
            this.SaveAnims.Size = new System.Drawing.Size(41, 21);
            this.SaveAnims.TabIndex = 28;
            this.SaveAnims.Text = "Save";
            this.SaveAnims.UseVisualStyleBackColor = true;
            // 
            // Load
            // 
            this.Load.Location = new System.Drawing.Point(1, 0);
            this.Load.Name = "Load";
            this.Load.Size = new System.Drawing.Size(41, 21);
            this.Load.TabIndex = 27;
            this.Load.Text = "Load";
            this.Load.UseVisualStyleBackColor = true;
            // 
            // fileType
            // 
            this.fileType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fileType.FormattingEnabled = true;
            this.fileType.Location = new System.Drawing.Point(82, 0);
            this.fileType.Name = "fileType";
            this.fileType.Size = new System.Drawing.Size(53, 21);
            this.fileType.TabIndex = 26;
            // 
            // btnAnims
            // 
            this.btnAnims.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAnims.Location = new System.Drawing.Point(0, 0);
            this.btnAnims.Name = "btnAnims";
            this.btnAnims.Size = new System.Drawing.Size(135, 21);
            this.btnAnims.TabIndex = 7;
            this.btnAnims.Text = "Animations";
            this.btnAnims.UseVisualStyleBackColor = true;
            // 
            // ctxTextures
            // 
            this.ctxTextures.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sourceToolStripMenuItem,
            this.sizeToolStripMenuItem,
            this.toolStripMenuItem1,
            this.viewToolStripMenuItem,
            this.exportTextureToolStripMenuItem,
            this.replaceTextureToolStripMenuItem,
            this.renameTextureTextureToolStripMenuItem,
            this.resetToolStripMenuItem});
            this.ctxTextures.Name = "ctxTextures";
            this.ctxTextures.Size = new System.Drawing.Size(125, 164);
            // 
            // sourceToolStripMenuItem
            // 
            this.sourceToolStripMenuItem.Enabled = false;
            this.sourceToolStripMenuItem.Name = "sourceToolStripMenuItem";
            this.sourceToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.sourceToolStripMenuItem.Text = "Source";
            // 
            // sizeToolStripMenuItem
            // 
            this.sizeToolStripMenuItem.Enabled = false;
            this.sizeToolStripMenuItem.Name = "sizeToolStripMenuItem";
            this.sizeToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.sizeToolStripMenuItem.Text = "Size";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(121, 6);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.viewToolStripMenuItem.Text = "View...";
            // 
            // exportTextureToolStripMenuItem
            // 
            this.exportTextureToolStripMenuItem.Name = "exportTextureToolStripMenuItem";
            this.exportTextureToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.exportTextureToolStripMenuItem.Text = "Export...";
            // 
            // replaceTextureToolStripMenuItem
            // 
            this.replaceTextureToolStripMenuItem.Name = "replaceTextureToolStripMenuItem";
            this.replaceTextureToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.replaceTextureToolStripMenuItem.Text = "Replace...";
            // 
            // renameTextureTextureToolStripMenuItem
            // 
            this.renameTextureTextureToolStripMenuItem.Name = "renameTextureTextureToolStripMenuItem";
            this.renameTextureTextureToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.renameTextureTextureToolStripMenuItem.Text = "Rename";
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.resetToolStripMenuItem.Text = "Reload";
            // 
            // pnlTextures
            // 
            this.pnlTextures.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTextures.Controls.Add(this.overTexPnl);
            this.pnlTextures.Controls.Add(this.lstTextures);
            this.pnlTextures.Controls.Add(this.chkAllTextures);
            this.pnlTextures.Controls.Add(this.btnTextures);
            this.pnlTextures.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlTextures.Location = new System.Drawing.Point(0, 336);
            this.pnlTextures.MinimumSize = new System.Drawing.Size(0, 21);
            this.pnlTextures.Name = "pnlTextures";
            this.pnlTextures.Size = new System.Drawing.Size(137, 164);
            this.pnlTextures.TabIndex = 3;
            // 
            // overTexPnl
            // 
            this.overTexPnl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.overTexPnl.Location = new System.Drawing.Point(0, 41);
            this.overTexPnl.Name = "overTexPnl";
            this.overTexPnl.Size = new System.Drawing.Size(135, 121);
            this.overTexPnl.TabIndex = 9;
            // 
            // lstTextures
            // 
            this.lstTextures.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstTextures.CausesValidation = false;
            this.lstTextures.ContextMenuStrip = this.ctxTextures;
            this.lstTextures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstTextures.IntegralHeight = false;
            this.lstTextures.Location = new System.Drawing.Point(0, 41);
            this.lstTextures.Margin = new System.Windows.Forms.Padding(0);
            this.lstTextures.Name = "lstTextures";
            this.lstTextures.Size = new System.Drawing.Size(135, 121);
            this.lstTextures.TabIndex = 7;
            // 
            // chkAllTextures
            // 
            this.chkAllTextures.Checked = true;
            this.chkAllTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllTextures.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkAllTextures.Location = new System.Drawing.Point(0, 21);
            this.chkAllTextures.Margin = new System.Windows.Forms.Padding(0);
            this.chkAllTextures.Name = "chkAllTextures";
            this.chkAllTextures.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
            this.chkAllTextures.Size = new System.Drawing.Size(135, 20);
            this.chkAllTextures.TabIndex = 8;
            this.chkAllTextures.Text = "All";
            this.chkAllTextures.UseVisualStyleBackColor = false;
            // 
            // btnTextures
            // 
            this.btnTextures.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnTextures.Location = new System.Drawing.Point(0, 0);
            this.btnTextures.Name = "btnTextures";
            this.btnTextures.Size = new System.Drawing.Size(135, 21);
            this.btnTextures.TabIndex = 9;
            this.btnTextures.Text = "Textures";
            this.btnTextures.UseVisualStyleBackColor = true;
            // 
            // spltObjTex
            // 
            this.spltObjTex.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.spltObjTex.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.spltObjTex.Location = new System.Drawing.Point(0, 332);
            this.spltObjTex.Name = "spltObjTex";
            this.spltObjTex.Size = new System.Drawing.Size(137, 4);
            this.spltObjTex.TabIndex = 4;
            // 
            // spltAnimObj
            // 
            this.spltAnimObj.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.spltAnimObj.Dock = System.Windows.Forms.DockStyle.Top;
            this.spltAnimObj.Location = new System.Drawing.Point(0, 178);
            this.spltAnimObj.Name = "spltAnimObj";
            this.spltAnimObj.Size = new System.Drawing.Size(137, 4);
            this.spltAnimObj.TabIndex = 1;
            // 
            // ctxAnim
            // 
            this.ctxAnim.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripSeparator1,
            this.chkLoop,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.renameToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.createNewToolStripMenuItem});
            this.ctxAnim.Name = "ctxAnim";
            this.ctxAnim.Size = new System.Drawing.Size(195, 186);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Enabled = false;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(194, 22);
            this.toolStripMenuItem2.Text = "Source";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(191, 6);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(194, 22);
            this.toolStripMenuItem3.Text = "Export...";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(194, 22);
            this.toolStripMenuItem4.Text = "Replace...";
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // createNewToolStripMenuItem
            // 
            this.createNewToolStripMenuItem.Name = "createNewToolStripMenuItem";
            this.createNewToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.createNewToolStripMenuItem.Text = "Create New Animation";
            // 
            // chkLoop
            // 
            this.chkLoop.CheckOnClick = true;
            this.chkLoop.Name = "chkLoop";
            this.chkLoop.Size = new System.Drawing.Size(194, 22);
            this.chkLoop.Text = "Loop";
            // 
            // LeftPanel
            // 
            this.Controls.Add(this.pnlObjects);
            this.Controls.Add(this.spltObjTex);
            this.Controls.Add(this.spltAnimObj);
            this.Controls.Add(this.pnlAnims);
            this.Controls.Add(this.pnlTextures);
            this.Name = "LeftPanel";
            this.Size = new System.Drawing.Size(137, 500);
            this.pnlObjects.ResumeLayout(false);
            this.pnlAnims.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ctxTextures.ResumeLayout(false);
            this.pnlTextures.ResumeLayout(false);
            this.ctxAnim.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public bool _closing = false;

        public ModelEditControl _mainWindow;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModelEditControl MainWindow
        {
            get { return _mainWindow; }
            set { _mainWindow = value; }
        }

        private bool _updating = false;
        private CollisionObject _targetObject;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CollisionObject TargetObject
        {
            get { return _targetObject; }
            set { _targetObject = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CollisionNode TargetCollision
        {
            get { return _mainWindow.TargetCollision; }
            set { _mainWindow.TargetCollision = value; }
        }

        public LeftPanelCollision()
        {
            InitializeComponent();
        }

        public void Reset()
        {
            lstObjects.BeginUpdate();
            lstObjects.Items.Clear();

            _targetObject = null;

            chkAllObj.CheckState = CheckState.Checked;
            chkAllTextures.CheckState = CheckState.Checked;

            if (TargetCollision != null)
            {
				lstObjects.Items.AddRange(TargetCollision._objects.ToArray());
            }

            lstObjects.EndUpdate();
        }

        private void lstObjects_Leave(object sender, EventArgs e)
        {
            overObjPnl.Invalidate();
        }
    }
}
