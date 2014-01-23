namespace System.Windows.Forms
{
    partial class InterpolationEditor
    {
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.numTanLen = new System.Windows.Forms.NumericInputBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numericInputBox3 = new System.Windows.Forms.NumericInputBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericInputBox1 = new System.Windows.Forms.NumericInputBox();
            this.numericInputBox2 = new System.Windows.Forms.NumericInputBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.displayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkViewAll = new System.Windows.Forms.ToolStripMenuItem();
            this.chkLinear = new System.Windows.Forms.ToolStripMenuItem();
            this.chkRenderTans = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkSetFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.chkSyncStartEnd = new System.Windows.Forms.ToolStripMenuItem();
            this.chkKeyDrag = new System.Windows.Forms.ToolStripMenuItem();
            this.chkGenTans = new System.Windows.Forms.ToolStripMenuItem();
            this.interpolationViewer = new System.Windows.Forms.InterpolationViewer();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.numTanLen);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.comboBox1);
            this.panel1.Controls.Add(this.menuStrip1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Enabled = false;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(360, 66);
            this.panel1.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label4.Location = new System.Drawing.Point(242, 2);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 20);
            this.label4.TabIndex = 7;
            this.label4.Text = "Tan Length:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numTanLen
            // 
            this.numTanLen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numTanLen.Integral = false;
            this.numTanLen.Location = new System.Drawing.Point(309, 2);
            this.numTanLen.MaximumValue = 3.402823E+38F;
            this.numTanLen.MinimumValue = 0F;
            this.numTanLen.Name = "numTanLen";
            this.numTanLen.Size = new System.Drawing.Size(43, 20);
            this.numTanLen.TabIndex = 6;
            this.numTanLen.Text = "0";
            this.numTanLen.ValueChanged += new System.EventHandler(this.numTanLen_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.numericInputBox3);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.numericInputBox1);
            this.groupBox1.Controls.Add(this.numericInputBox2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(0, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(360, 42);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Edit Keyframe";
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Frame:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericInputBox3
            // 
            this.numericInputBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericInputBox3.Integral = false;
            this.numericInputBox3.Location = new System.Drawing.Point(62, 16);
            this.numericInputBox3.MaximumValue = 3.402823E+38F;
            this.numericInputBox3.MinimumValue = 0F;
            this.numericInputBox3.Name = "numericInputBox3";
            this.numericInputBox3.Size = new System.Drawing.Size(60, 20);
            this.numericInputBox3.TabIndex = 4;
            this.numericInputBox3.Text = "0";
            this.numericInputBox3.ValueChanged += new System.EventHandler(this.numericInputBox3_ValueChanged);
            // 
            // label3
            // 
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Location = new System.Drawing.Point(236, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Tangent:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Location = new System.Drawing.Point(121, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Value:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericInputBox1
            // 
            this.numericInputBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericInputBox1.Integral = false;
            this.numericInputBox1.Location = new System.Drawing.Point(292, 16);
            this.numericInputBox1.MaximumValue = 3.402823E+38F;
            this.numericInputBox1.MinimumValue = -3.402823E+38F;
            this.numericInputBox1.Name = "numericInputBox1";
            this.numericInputBox1.Size = new System.Drawing.Size(60, 20);
            this.numericInputBox1.TabIndex = 0;
            this.numericInputBox1.Text = "0";
            this.numericInputBox1.ValueChanged += new System.EventHandler(this.numericInputBox1_ValueChanged);
            // 
            // numericInputBox2
            // 
            this.numericInputBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericInputBox2.Integral = false;
            this.numericInputBox2.Location = new System.Drawing.Point(177, 16);
            this.numericInputBox2.MaximumValue = 3.402823E+38F;
            this.numericInputBox2.MinimumValue = -3.402823E+38F;
            this.numericInputBox2.Name = "numericInputBox2";
            this.numericInputBox2.Size = new System.Drawing.Size(60, 20);
            this.numericInputBox2.TabIndex = 2;
            this.numericInputBox2.Text = "0";
            this.numericInputBox2.ValueChanged += new System.EventHandler(this.numericInputBox2_ValueChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Scale X",
            "Scale Y",
            "Scale Z",
            "Rotation X",
            "Rotation Y",
            "Rotation Z",
            "Translation X",
            "Translation Y",
            "Translation Z"});
            this.comboBox1.Location = new System.Drawing.Point(127, 1);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(110, 21);
            this.comboBox1.TabIndex = 6;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displayToolStripMenuItem,
            this.optionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(360, 24);
            this.menuStrip1.TabIndex = 16;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // displayToolStripMenuItem
            // 
            this.displayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chkViewAll,
            this.chkLinear,
            this.chkRenderTans});
            this.displayToolStripMenuItem.Name = "displayToolStripMenuItem";
            this.displayToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.displayToolStripMenuItem.Text = "Display";
            // 
            // chkViewAll
            // 
            this.chkViewAll.CheckOnClick = true;
            this.chkViewAll.Name = "chkViewAll";
            this.chkViewAll.Size = new System.Drawing.Size(177, 22);
            this.chkViewAll.Text = "View keyframe only";
            this.chkViewAll.CheckedChanged += new System.EventHandler(this.chkAllKeys_CheckedChanged);
            // 
            // chkLinear
            // 
            this.chkLinear.CheckOnClick = true;
            this.chkLinear.Name = "chkLinear";
            this.chkLinear.Size = new System.Drawing.Size(177, 22);
            this.chkLinear.Text = "Linear";
            this.chkLinear.CheckedChanged += new System.EventHandler(this.chkLinear_CheckedChanged);
            // 
            // chkRenderTans
            // 
            this.chkRenderTans.Checked = true;
            this.chkRenderTans.CheckOnClick = true;
            this.chkRenderTans.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRenderTans.Name = "chkRenderTans";
            this.chkRenderTans.Size = new System.Drawing.Size(177, 22);
            this.chkRenderTans.Text = "Show tangents";
            this.chkRenderTans.CheckedChanged += new System.EventHandler(this.chkRenderTans_CheckedChanged);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chkSetFrame,
            this.chkSyncStartEnd,
            this.chkKeyDrag,
            this.chkGenTans});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // chkSetFrame
            // 
            this.chkSetFrame.Checked = true;
            this.chkSetFrame.CheckOnClick = true;
            this.chkSetFrame.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSetFrame.Name = "chkSetFrame";
            this.chkSetFrame.Size = new System.Drawing.Size(228, 22);
            this.chkSetFrame.Text = "Set frame on keyframe select";
            // 
            // chkSyncStartEnd
            // 
            this.chkSyncStartEnd.CheckOnClick = true;
            this.chkSyncStartEnd.Name = "chkSyncStartEnd";
            this.chkSyncStartEnd.Size = new System.Drawing.Size(228, 22);
            this.chkSyncStartEnd.Text = "Sync start and end keyframes";
            this.chkSyncStartEnd.CheckedChanged += new System.EventHandler(this.chkSyncStartEnd_CheckedChanged);
            // 
            // chkKeyDrag
            // 
            this.chkKeyDrag.CheckOnClick = true;
            this.chkKeyDrag.Name = "chkKeyDrag";
            this.chkKeyDrag.Size = new System.Drawing.Size(228, 22);
            this.chkKeyDrag.Text = "Draggable keyframes";
            this.chkKeyDrag.CheckedChanged += new System.EventHandler(this.chkKeyDrag_CheckedChanged);
            // 
            // chkGenTans
            // 
            this.chkGenTans.CheckOnClick = true;
            this.chkGenTans.Name = "chkGenTans";
            this.chkGenTans.Size = new System.Drawing.Size(228, 22);
            this.chkGenTans.Text = "Generate tangents";
            this.chkGenTans.CheckedChanged += new System.EventHandler(this.chkGenTans_CheckedChanged);
            // 
            // interpolationViewer
            // 
            this.interpolationViewer.AllKeyframes = true;
            this.interpolationViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.interpolationViewer.DrawTangents = true;
            this.interpolationViewer.FrameIndex = 0;
            this.interpolationViewer.FrameLimit = 0;
            this.interpolationViewer.GenerateTangents = false;
            this.interpolationViewer.KeyDraggingAllowed = false;
            this.interpolationViewer.Linear = false;
            this.interpolationViewer.Location = new System.Drawing.Point(0, 66);
            this.interpolationViewer.Name = "interpolationViewer";
            this.interpolationViewer.Precision = 3.75F;
            this.interpolationViewer.Size = new System.Drawing.Size(360, 180);
            this.interpolationViewer.SyncStartEnd = false;
            this.interpolationViewer.TabIndex = 3;
            this.interpolationViewer.TangentLength = 5F;
            // 
            // InterpolationEditor
            // 
            this.Controls.Add(this.interpolationViewer);
            this.Controls.Add(this.panel1);
            this.Name = "InterpolationEditor";
            this.Size = new System.Drawing.Size(360, 246);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private global::System.Windows.Forms.Panel panel1;
        private global::System.Windows.Forms.GroupBox groupBox1;
        private global::System.Windows.Forms.Label label1;
        private global::System.Windows.Forms.NumericInputBox numericInputBox3;
        private global::System.Windows.Forms.Label label3;
        private global::System.Windows.Forms.Label label2;
        private global::System.Windows.Forms.NumericInputBox numericInputBox1;
        private global::System.Windows.Forms.NumericInputBox numericInputBox2;
        private global::System.Windows.Forms.ComboBox comboBox1;
        public InterpolationViewer interpolationViewer;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem displayToolStripMenuItem;
        private ToolStripMenuItem chkViewAll;
        private ToolStripMenuItem chkLinear;
        private ToolStripMenuItem chkRenderTans;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem chkSetFrame;
        private ToolStripMenuItem chkSyncStartEnd;
        private ToolStripMenuItem chkKeyDrag;
        private ToolStripMenuItem chkGenTans;
        private Label label4;
        private NumericInputBox numTanLen;
    }
}
