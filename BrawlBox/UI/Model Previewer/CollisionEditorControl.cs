using BrawlLib.Wii.Animations;
using BrawlLib.SSBB.ResourceNodes;
using System.ComponentModel;
using System.Collections.Generic;
using BrawlLib.Modeling;
using System.Drawing;
using BrawlLib.SSBBTypes;

namespace System.Windows.Forms
{
    public class CollisionEditorControl : UserControl
    {
        #region Designer
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.subtract = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.add = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.Source = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxBox = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addCustomAmountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlPlaneProps = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkTypeUnk2 = new System.Windows.Forms.CheckBox();
            this.chkTypeUnk1 = new System.Windows.Forms.CheckBox();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkNoWalljump = new System.Windows.Forms.CheckBox();
            this.chkRightLedge = new System.Windows.Forms.CheckBox();
            this.chkLeftLedge = new System.Windows.Forms.CheckBox();
            this.chkFallThrough = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cboMaterial = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.pnlPointProps = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.numY = new System.Windows.Forms.NumericInputBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numX = new System.Windows.Forms.NumericInputBox();
            this.pnlObjProps = new System.Windows.Forms.Panel();
            this.chkObjSSEUnk = new System.Windows.Forms.CheckBox();
            this.chkObjModule = new System.Windows.Forms.CheckBox();
            this.chkObjUnk = new System.Windows.Forms.CheckBox();
            this.btnUnlink = new System.Windows.Forms.Button();
            this.btnRelink = new System.Windows.Forms.Button();
            this.txtBone = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtModel = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ctxBox.SuspendLayout();
            this.pnlPlaneProps.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnlPointProps.SuspendLayout();
            this.pnlObjProps.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(118, 26);
            this.toolStripMenuItem7.Text = "+45";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(118, 26);
            this.toolStripMenuItem4.Text = "+90";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(118, 26);
            this.toolStripMenuItem3.Text = "+180";
            // 
            // subtract
            // 
            this.subtract.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem5,
            this.toolStripMenuItem6,
            this.toolStripMenuItem8});
            this.subtract.Name = "subtract";
            this.subtract.Size = new System.Drawing.Size(199, 26);
            this.subtract.Text = "Subtract From All";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(114, 26);
            this.toolStripMenuItem5.Text = "-180";
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(114, 26);
            this.toolStripMenuItem6.Text = "-90";
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(114, 26);
            this.toolStripMenuItem8.Text = "-45";
            // 
            // add
            // 
            this.add.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripMenuItem7});
            this.add.Name = "add";
            this.add.Size = new System.Drawing.Size(199, 26);
            this.add.Text = "Add To All";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(196, 6);
            // 
            // Source
            // 
            this.Source.Enabled = false;
            this.Source.Name = "Source";
            this.Source.Size = new System.Drawing.Size(199, 26);
            this.Source.Text = "Source";
            // 
            // removeAllToolStripMenuItem
            // 
            this.removeAllToolStripMenuItem.Name = "removeAllToolStripMenuItem";
            this.removeAllToolStripMenuItem.Size = new System.Drawing.Size(199, 26);
            this.removeAllToolStripMenuItem.Text = "Remove All";
            // 
            // ctxBox
            // 
            this.ctxBox.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctxBox.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Source,
            this.toolStripSeparator1,
            this.add,
            this.subtract,
            this.removeAllToolStripMenuItem,
            this.addCustomAmountToolStripMenuItem});
            this.ctxBox.Name = "ctxBox";
            this.ctxBox.Size = new System.Drawing.Size(200, 140);
            // 
            // addCustomAmountToolStripMenuItem
            // 
            this.addCustomAmountToolStripMenuItem.Name = "addCustomAmountToolStripMenuItem";
            this.addCustomAmountToolStripMenuItem.Size = new System.Drawing.Size(199, 26);
            // 
            // pnlPlaneProps
            // 
            this.pnlPlaneProps.Controls.Add(this.groupBox2);
            this.pnlPlaneProps.Controls.Add(this.groupBox1);
            this.pnlPlaneProps.Controls.Add(this.panel1);
            this.pnlPlaneProps.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlPlaneProps.Location = new System.Drawing.Point(0, 0);
            this.pnlPlaneProps.MinimumSize = new System.Drawing.Size(265, 134);
            this.pnlPlaneProps.Name = "pnlPlaneProps";
            this.pnlPlaneProps.Size = new System.Drawing.Size(265, 134);
            this.pnlPlaneProps.TabIndex = 0;
            this.pnlPlaneProps.Visible = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkTypeUnk2);
            this.groupBox2.Controls.Add(this.chkTypeUnk1);
            this.groupBox2.Controls.Add(this.cboType);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(135, 25);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(130, 109);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Types";
            // 
            // chkTypeUnk2
            // 
            this.chkTypeUnk2.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkTypeUnk2.Location = new System.Drawing.Point(4, 61);
            this.chkTypeUnk2.Margin = new System.Windows.Forms.Padding(0);
            this.chkTypeUnk2.Name = "chkTypeUnk2";
            this.chkTypeUnk2.Size = new System.Drawing.Size(122, 18);
            this.chkTypeUnk2.TabIndex = 3;
            this.chkTypeUnk2.Text = "Items";
            this.chkTypeUnk2.UseVisualStyleBackColor = true;
            this.chkTypeUnk2.CheckedChanged += new System.EventHandler(this.chkTypeUnk2_CheckedChanged);
            // 
            // chkTypeUnk1
            // 
            this.chkTypeUnk1.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkTypeUnk1.Location = new System.Drawing.Point(4, 43);
            this.chkTypeUnk1.Margin = new System.Windows.Forms.Padding(0);
            this.chkTypeUnk1.Name = "chkTypeUnk1";
            this.chkTypeUnk1.Size = new System.Drawing.Size(122, 18);
            this.chkTypeUnk1.TabIndex = 4;
            this.chkTypeUnk1.Text = "Characters";
            this.chkTypeUnk1.UseVisualStyleBackColor = true;
            this.chkTypeUnk1.CheckedChanged += new System.EventHandler(this.chkTypeUnk1_CheckedChanged);
            // 
            // cboType
            // 
            this.cboType.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(4, 19);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(122, 24);
            this.cboType.TabIndex = 5;
            this.cboType.SelectedIndexChanged += new System.EventHandler(this.cboType_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkNoWalljump);
            this.groupBox1.Controls.Add(this.chkRightLedge);
            this.groupBox1.Controls.Add(this.chkLeftLedge);
            this.groupBox1.Controls.Add(this.chkFallThrough);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(0, 25);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(135, 109);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Flags";
            // 
            // chkNoWalljump
            // 
            this.chkNoWalljump.AutoSize = true;
            this.chkNoWalljump.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkNoWalljump.Location = new System.Drawing.Point(4, 82);
            this.chkNoWalljump.Margin = new System.Windows.Forms.Padding(0);
            this.chkNoWalljump.Name = "chkNoWalljump";
            this.chkNoWalljump.Size = new System.Drawing.Size(127, 21);
            this.chkNoWalljump.TabIndex = 2;
            this.chkNoWalljump.Text = "No Walljump";
            this.chkNoWalljump.UseVisualStyleBackColor = true;
            this.chkNoWalljump.CheckedChanged += new System.EventHandler(this.chkNoWalljump_CheckedChanged);
            // 
            // chkRightLedge
            // 
            this.chkRightLedge.AutoSize = true;
            this.chkRightLedge.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkRightLedge.Location = new System.Drawing.Point(4, 61);
            this.chkRightLedge.Margin = new System.Windows.Forms.Padding(0);
            this.chkRightLedge.Name = "chkRightLedge";
            this.chkRightLedge.Size = new System.Drawing.Size(127, 21);
            this.chkRightLedge.TabIndex = 1;
            this.chkRightLedge.Text = "Right Ledge";
            this.chkRightLedge.UseVisualStyleBackColor = true;
            this.chkRightLedge.CheckedChanged += new System.EventHandler(this.chkRightLedge_CheckedChanged);
            // 
            // chkLeftLedge
            // 
            this.chkLeftLedge.AutoSize = true;
            this.chkLeftLedge.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkLeftLedge.Location = new System.Drawing.Point(4, 40);
            this.chkLeftLedge.Margin = new System.Windows.Forms.Padding(0);
            this.chkLeftLedge.Name = "chkLeftLedge";
            this.chkLeftLedge.Size = new System.Drawing.Size(127, 21);
            this.chkLeftLedge.TabIndex = 4;
            this.chkLeftLedge.Text = "Left Ledge";
            this.chkLeftLedge.UseVisualStyleBackColor = true;
            this.chkLeftLedge.CheckedChanged += new System.EventHandler(this.chkLeftLedge_CheckedChanged);
            // 
            // chkFallThrough
            // 
            this.chkFallThrough.AutoSize = true;
            this.chkFallThrough.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkFallThrough.Location = new System.Drawing.Point(4, 19);
            this.chkFallThrough.Margin = new System.Windows.Forms.Padding(0);
            this.chkFallThrough.Name = "chkFallThrough";
            this.chkFallThrough.Size = new System.Drawing.Size(127, 21);
            this.chkFallThrough.TabIndex = 0;
            this.chkFallThrough.Text = "Fall-Through";
            this.chkFallThrough.UseVisualStyleBackColor = true;
            this.chkFallThrough.CheckedChanged += new System.EventHandler(this.chkFallThrough_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cboMaterial);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(265, 25);
            this.panel1.TabIndex = 15;
            // 
            // cboMaterial
            // 
            this.cboMaterial.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboMaterial.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMaterial.FormattingEnabled = true;
            this.cboMaterial.Location = new System.Drawing.Point(75, 0);
            this.cboMaterial.Name = "cboMaterial";
            this.cboMaterial.Size = new System.Drawing.Size(190, 24);
            this.cboMaterial.TabIndex = 12;
            this.cboMaterial.SelectedIndexChanged += new System.EventHandler(this.cboMaterial_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.Dock = System.Windows.Forms.DockStyle.Left;
            this.label5.Location = new System.Drawing.Point(0, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 25);
            this.label5.TabIndex = 8;
            this.label5.Text = "Material:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlPointProps
            // 
            this.pnlPointProps.Controls.Add(this.label2);
            this.pnlPointProps.Controls.Add(this.numY);
            this.pnlPointProps.Controls.Add(this.label1);
            this.pnlPointProps.Controls.Add(this.numX);
            this.pnlPointProps.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlPointProps.Location = new System.Drawing.Point(265, 0);
            this.pnlPointProps.MinimumSize = new System.Drawing.Size(173, 134);
            this.pnlPointProps.Name = "pnlPointProps";
            this.pnlPointProps.Size = new System.Drawing.Size(173, 134);
            this.pnlPointProps.TabIndex = 15;
            this.pnlPointProps.Visible = false;
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Location = new System.Drawing.Point(17, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 22);
            this.label2.TabIndex = 3;
            this.label2.Text = "Y";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numY
            // 
            this.numY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numY.Integral = false;
            this.numY.Location = new System.Drawing.Point(58, 63);
            this.numY.MaximumValue = 3.402823E+38F;
            this.numY.MinimumValue = -3.402823E+38F;
            this.numY.Name = "numY";
            this.numY.Size = new System.Drawing.Size(100, 22);
            this.numY.TabIndex = 2;
            this.numY.Text = "0";
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(17, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 22);
            this.label1.TabIndex = 1;
            this.label1.Text = "X";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numX
            // 
            this.numX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numX.Integral = false;
            this.numX.Location = new System.Drawing.Point(58, 42);
            this.numX.MaximumValue = 3.402823E+38F;
            this.numX.MinimumValue = -3.402823E+38F;
            this.numX.Name = "numX";
            this.numX.Size = new System.Drawing.Size(100, 22);
            this.numX.TabIndex = 0;
            this.numX.Text = "0";
            // 
            // pnlObjProps
            // 
            this.pnlObjProps.Controls.Add(this.chkObjSSEUnk);
            this.pnlObjProps.Controls.Add(this.chkObjModule);
            this.pnlObjProps.Controls.Add(this.chkObjUnk);
            this.pnlObjProps.Controls.Add(this.btnUnlink);
            this.pnlObjProps.Controls.Add(this.btnRelink);
            this.pnlObjProps.Controls.Add(this.txtBone);
            this.pnlObjProps.Controls.Add(this.label4);
            this.pnlObjProps.Controls.Add(this.txtModel);
            this.pnlObjProps.Controls.Add(this.label3);
            this.pnlObjProps.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlObjProps.Location = new System.Drawing.Point(438, 0);
            this.pnlObjProps.MinimumSize = new System.Drawing.Size(218, 134);
            this.pnlObjProps.Name = "pnlObjProps";
            this.pnlObjProps.Size = new System.Drawing.Size(218, 134);
            this.pnlObjProps.TabIndex = 1;
            this.pnlObjProps.Visible = false;
            // 
            // chkObjSSEUnk
            // 
            this.chkObjSSEUnk.AutoSize = true;
            this.chkObjSSEUnk.Location = new System.Drawing.Point(10, 102);
            this.chkObjSSEUnk.Name = "chkObjSSEUnk";
            this.chkObjSSEUnk.Size = new System.Drawing.Size(119, 21);
            this.chkObjSSEUnk.TabIndex = 15;
            this.chkObjSSEUnk.Text = "SSE Unknown";
            this.chkObjSSEUnk.UseVisualStyleBackColor = true;
            this.chkObjSSEUnk.CheckedChanged += new System.EventHandler(this.chkObjSSEUnk_CheckedChanged);
            // 
            // chkObjModule
            // 
            this.chkObjModule.AutoSize = true;
            this.chkObjModule.Location = new System.Drawing.Point(10, 79);
            this.chkObjModule.Name = "chkObjModule";
            this.chkObjModule.Size = new System.Drawing.Size(144, 21);
            this.chkObjModule.TabIndex = 14;
            this.chkObjModule.Text = "Module Controlled";
            this.chkObjModule.UseVisualStyleBackColor = true;
            this.chkObjModule.CheckedChanged += new System.EventHandler(this.chkObjModule_CheckedChanged);
            // 
            // chkObjUnk
            // 
            this.chkObjUnk.AutoSize = true;
            this.chkObjUnk.Location = new System.Drawing.Point(10, 56);
            this.chkObjUnk.Name = "chkObjUnk";
            this.chkObjUnk.Size = new System.Drawing.Size(88, 21);
            this.chkObjUnk.TabIndex = 13;
            this.chkObjUnk.Text = "Unknown";
            this.chkObjUnk.UseVisualStyleBackColor = true;
            this.chkObjUnk.CheckedChanged += new System.EventHandler(this.chkObjUnk_CheckedChanged);
            // 
            // btnUnlink
            // 
            this.btnUnlink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUnlink.Location = new System.Drawing.Point(186, 24);
            this.btnUnlink.Name = "btnUnlink";
            this.btnUnlink.Size = new System.Drawing.Size(28, 21);
            this.btnUnlink.TabIndex = 12;
            this.btnUnlink.Text = "-";
            this.btnUnlink.UseVisualStyleBackColor = true;
            this.btnUnlink.Click += new System.EventHandler(this.btnUnlink_Click);
            // 
            // btnRelink
            // 
            this.btnRelink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRelink.Location = new System.Drawing.Point(186, 3);
            this.btnRelink.Name = "btnRelink";
            this.btnRelink.Size = new System.Drawing.Size(28, 21);
            this.btnRelink.TabIndex = 4;
            this.btnRelink.Text = "+";
            this.btnRelink.UseVisualStyleBackColor = true;
            this.btnRelink.Click += new System.EventHandler(this.btnRelink_Click);
            // 
            // txtBone
            // 
            this.txtBone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBone.Location = new System.Drawing.Point(73, 23);
            this.txtBone.Name = "txtBone";
            this.txtBone.ReadOnly = true;
            this.txtBone.Size = new System.Drawing.Size(111, 22);
            this.txtBone.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(4, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 22);
            this.label4.TabIndex = 2;
            this.label4.Text = "Bone:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtModel
            // 
            this.txtModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtModel.Location = new System.Drawing.Point(73, 3);
            this.txtModel.Name = "txtModel";
            this.txtModel.ReadOnly = true;
            this.txtModel.Size = new System.Drawing.Size(111, 22);
            this.txtModel.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(4, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 22);
            this.label3.TabIndex = 0;
            this.label3.Text = "Model:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CollisionEditorControl
            // 
            this.AutoSize = true;
            this.Controls.Add(this.pnlObjProps);
            this.Controls.Add(this.pnlPointProps);
            this.Controls.Add(this.pnlPlaneProps);
            this.Name = "CollisionEditorControl";
            this.Size = new System.Drawing.Size(656, 134);
            this.ctxBox.ResumeLayout(false);
            this.pnlPlaneProps.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.pnlPointProps.ResumeLayout(false);
            this.pnlPointProps.PerformLayout();
            this.pnlObjProps.ResumeLayout(false);
            this.pnlObjProps.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private ToolStripMenuItem toolStripMenuItem7;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem subtract;
        private ToolStripMenuItem toolStripMenuItem5;
        private ToolStripMenuItem toolStripMenuItem6;
        private ToolStripMenuItem toolStripMenuItem8;
        private ToolStripMenuItem add;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem Source;
        private ToolStripMenuItem removeAllToolStripMenuItem;
        private ContextMenuStrip ctxBox;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem addCustomAmountToolStripMenuItem;
        Panel pnlPlaneProps;
        GroupBox groupBox2;
        ComboBox cboType;
        CheckBox chkTypeUnk2;
        CheckBox chkTypeUnk1;
        GroupBox groupBox1;
        CheckBox chkLeftLedge;
        CheckBox chkNoWalljump;
        CheckBox chkRightLedge;
        CheckBox chkFallThrough;
        ComboBox cboMaterial;
        Label label5;
        Panel pnlPointProps;
        Label label2;
        NumericInputBox numY;
        Label label1;
        NumericInputBox numX;
        Panel pnlObjProps;
        public CheckBox chkObjSSEUnk;
        public CheckBox chkObjModule;
        public CheckBox chkObjUnk;
        Button btnUnlink;
        Button btnRelink;
        TextBox txtBone;
        Label label4;
        TextBox txtModel;
        Label label3;
        Panel panel1;

        public ModelEditControl _mainWindow;

        public event EventHandler CreateUndo;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IBoneNode TargetBone { get { return _mainWindow.SelectedBone; } set { _mainWindow.SelectedBone = value; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentFrame
        {
            get { return _mainWindow.CurrentFrame; }
            set { _mainWindow.CurrentFrame = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IModel TargetModel
        {
            get { return _mainWindow.TargetModel; }
            set { _mainWindow.TargetModel = value; }
        }
        private bool _updating;
        public CollisionEditorControl()
        {
            InitializeComponent();

            _updating = true;
            cboMaterial.DataSource = Enum.GetValues(typeof(CollisionPlaneMaterial));
            cboType.DataSource = Enum.GetValues(typeof(CollisionPlaneType));
            _updating = false;
        }

        public void SelectionModified()
        {
            pnlPlaneProps.Visible = false;
            pnlObjProps.Visible = false;
            pnlPointProps.Visible = false;

            if (_mainWindow._selectedPlanes.Count > 0)
            {
                pnlPlaneProps.Visible = true;
            }
            else if (_mainWindow._selectedLinks.Count == 1)
            {
                pnlPointProps.Visible = true;
            }
            _mainWindow.UpdateAnimationPanelDimensions();
        }
        public void UpdatePropPanels()
        {
            _updating = true;

            if (pnlPlaneProps.Visible)
            {
                CollisionPlane p = _mainWindow._selectedPlanes[0];

                //Material
                cboMaterial.SelectedItem = p._material;
                //Flags
                chkFallThrough.Checked = p.IsFallThrough;
                chkLeftLedge.Checked = p.IsLeftLedge;
                chkRightLedge.Checked = p.IsRightLedge;
                chkNoWalljump.Checked = p.IsNoWalljump;
                //Type
                cboType.SelectedItem = p.Type;
                chkTypeUnk1.Checked = p.IsType1;
                chkTypeUnk2.Checked = p.IsType2;
            }
            else if (pnlPointProps.Visible)
            {
                numX.Value = _mainWindow._selectedLinks[0].Value._x;
                numY.Value = _mainWindow._selectedLinks[0].Value._y;
            }
            else if (pnlObjProps.Visible)
            {
                txtModel.Text = _mainWindow._selectedCollisionObject._modelName;
                txtBone.Text = _mainWindow._selectedCollisionObject._boneName;
                chkObjUnk.Checked = _mainWindow._selectedCollisionObject._flags[0];
                chkObjModule.Checked = _mainWindow._selectedCollisionObject._flags[2];
                chkObjSSEUnk.Checked = _mainWindow._selectedCollisionObject._flags[3];
            }

            _updating = false;

            _mainWindow.UpdateAnimationPanelDimensions();
        }
        public void ObjectSelected()
        {
            pnlPlaneProps.Visible = false;
            pnlPointProps.Visible = false;
            pnlObjProps.Visible = false;
            if (_mainWindow._selectedCollisionObject != null)
            {
                pnlObjProps.Visible = true;
                UpdatePropPanels();
            }
            _mainWindow.UpdateAnimationPanelDimensions();
        }

        #region Plane Properties

        private void cboMaterial_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;
            foreach (CollisionPlane plane in _mainWindow._selectedPlanes)
                plane._material = (CollisionPlaneMaterial)cboMaterial.SelectedItem;
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
        }
        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;
            foreach (CollisionPlane plane in _mainWindow._selectedPlanes)
                plane.Type = (CollisionPlaneType)cboType.SelectedItem;
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
        }
        private void chkTypeUnk1_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
            foreach (CollisionPlane p in _mainWindow._selectedPlanes)
                p.IsType1 = chkTypeUnk1.Checked;
        }
        private void chkTypeUnk2_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
            foreach (CollisionPlane p in _mainWindow._selectedPlanes)
                p.IsType2 = chkTypeUnk2.Checked;
        }
        private void chkFallThrough_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
            foreach (CollisionPlane p in _mainWindow._selectedPlanes)
                p.IsFallThrough = chkFallThrough.Checked;
        }
        private void chkLeftLedge_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
            foreach (CollisionPlane p in _mainWindow._selectedPlanes)
                p.IsLeftLedge = chkLeftLedge.Checked;
        }
        private void chkRightLedge_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
            foreach (CollisionPlane p in _mainWindow._selectedPlanes)
                p.IsRightLedge = chkRightLedge.Checked;
        }
        private void chkNoWalljump_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
            foreach (CollisionPlane p in _mainWindow._selectedPlanes)
                p.IsNoWalljump = chkNoWalljump.Checked;
        }

        #endregion

        #region Point Properties

        private void numX_ValueChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;
            foreach (CollisionLink link in _mainWindow._selectedLinks)
                link._rawValue._x = numX.Value;
            _mainWindow.ModelPanel.Invalidate();
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
        }

        private void numY_ValueChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;
            foreach (CollisionLink link in _mainWindow._selectedLinks)
                link._rawValue._y = numY.Value;
            _mainWindow.ModelPanel.Invalidate();
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
        }

        #endregion

        private void btnRelink_Click(object sender, EventArgs e)
        {
            IBoneNode node = _mainWindow.SelectedBone;
            if (_mainWindow._selectedCollisionObject == null ||
                node == null || !(node is MDL0BoneNode))
                return;

            _mainWindow.LinkBoneChange();
            txtBone.Text = _mainWindow._selectedCollisionObject._boneName = node.Name;
            _mainWindow._selectedCollisionObject.LinkedBone = ((MDL0BoneNode)node);
            txtModel.Text = _mainWindow._selectedCollisionObject._modelName = ((ResourceNode)node.IModel).Name;
            _mainWindow.LinkBoneChange();
        }
        private void btnUnlink_Click(object sender, EventArgs e)
        {
            _mainWindow.LinkBoneChange();
            txtBone.Text = "";
            txtModel.Text = "";
            _mainWindow._selectedCollisionObject.LinkedBone = null;
            _mainWindow.LinkBoneChange();
        }
        private void chkObjUnk_CheckedChanged(object sender, EventArgs e)
        {
            if (_mainWindow._selectedCollisionObject == null || _updating)
                return;
            _mainWindow._selectedCollisionObject._flags[0] = chkObjUnk.Checked;
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
        }
        private void chkObjIndep_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void chkObjModule_CheckedChanged(object sender, EventArgs e)
        {
            if (_mainWindow._selectedCollisionObject == null || _updating)
                return;
            _mainWindow._selectedCollisionObject._flags[2] = chkObjModule.Checked;
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
        }
        private void chkObjSSEUnk_CheckedChanged(object sender, EventArgs e)
        {
            if (_mainWindow._selectedCollisionObject == null || _updating)
                return;
            _mainWindow._selectedCollisionObject._flags[3] = chkObjSSEUnk.Checked;
            _mainWindow.TargetCollisionNode.SignalPropertyChange();
        }
        internal Size GetDimensions()
        {
            if (pnlObjProps.Visible)
                return pnlObjProps.MinimumSize;
            else if (pnlPlaneProps.Visible)
                return pnlPlaneProps.MinimumSize;
            else if (pnlPointProps.Visible)
                return pnlPointProps.MinimumSize;
            return new Size(0,134);
        }
    }
}
