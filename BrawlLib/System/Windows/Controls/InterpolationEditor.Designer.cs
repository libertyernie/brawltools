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
            this.nibTanLen = new System.Windows.Forms.NumericInputBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.nibKeyFrame = new System.Windows.Forms.NumericInputBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nibTangent = new System.Windows.Forms.NumericInputBox();
            this.nibKeyValue = new System.Windows.Forms.NumericInputBox();
            this.cbTransform = new System.Windows.Forms.ComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.displayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkViewAll = new System.Windows.Forms.ToolStripMenuItem();
            this.chkLinear = new System.Windows.Forms.ToolStripMenuItem();
            this.mItem_display_showEditValsAsLinear = new System.Windows.Forms.ToolStripMenuItem();
            this.chkRenderTans = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkSetFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.chkSyncStartEnd = new System.Windows.Forms.ToolStripMenuItem();
            this.chkKeyDrag = new System.Windows.Forms.ToolStripMenuItem();
            this.chkGenTans = new System.Windows.Forms.ToolStripMenuItem();
            this.mItem_genTan_alterSelTanOnDrag = new System.Windows.Forms.ToolStripMenuItem();
            this.mItem_genTan_alterAdjTan = new System.Windows.Forms.ToolStripMenuItem();
            this.mItem_genTan_alterAdjTan_OnSet = new System.Windows.Forms.ToolStripMenuItem();
            this.mItem_genTan_alterAdjTan_OnDel = new System.Windows.Forms.ToolStripMenuItem();
            this.mItem_genTan_alterAdjTan_OnDrag = new System.Windows.Forms.ToolStripMenuItem();
            this.interpolationViewer = new System.Windows.Forms.InterpolationViewer();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.nibTanLen);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.cbTransform);
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
            // nibTanLen
            // 
            this.nibTanLen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nibTanLen.Integral = false;
            this.nibTanLen.Location = new System.Drawing.Point(309, 2);
            this.nibTanLen.MaximumValue = 3.402823E+38F;
            this.nibTanLen.MinimumValue = 0F;
            this.nibTanLen.Name = "nibTanLen";
            this.nibTanLen.Size = new System.Drawing.Size(43, 20);
            this.nibTanLen.TabIndex = 6;
            this.nibTanLen.Text = "0";
            this.nibTanLen.ValueChanged += new System.EventHandler(this.numTanLen_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.nibKeyFrame);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.nibTangent);
            this.groupBox1.Controls.Add(this.nibKeyValue);
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
            // nibKeyFrame
            // 
            this.nibKeyFrame.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nibKeyFrame.Integral = false;
            this.nibKeyFrame.Location = new System.Drawing.Point(62, 16);
            this.nibKeyFrame.MaximumValue = 3.402823E+38F;
            this.nibKeyFrame.MinimumValue = 0F;
            this.nibKeyFrame.Name = "nibKeyFrame";
            this.nibKeyFrame.Size = new System.Drawing.Size(60, 20);
            this.nibKeyFrame.TabIndex = 4;
            this.nibKeyFrame.Text = "0";
            this.nibKeyFrame.ValueChanged += new System.EventHandler(this.numericInputBox3_ValueChanged);
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
            // nibTangent
            // 
            this.nibTangent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nibTangent.Integral = false;
            this.nibTangent.Location = new System.Drawing.Point(292, 16);
            this.nibTangent.MaximumValue = 3.402823E+38F;
            this.nibTangent.MinimumValue = -3.402823E+38F;
            this.nibTangent.Name = "nibTangent";
            this.nibTangent.Size = new System.Drawing.Size(60, 20);
            this.nibTangent.TabIndex = 0;
            this.nibTangent.Text = "0";
            this.nibTangent.ValueChanged += new System.EventHandler(this.numericInputBox1_ValueChanged);
            // 
            // nibKeyValue
            // 
            this.nibKeyValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nibKeyValue.Integral = false;
            this.nibKeyValue.Location = new System.Drawing.Point(177, 16);
            this.nibKeyValue.MaximumValue = 3.402823E+38F;
            this.nibKeyValue.MinimumValue = -3.402823E+38F;
            this.nibKeyValue.Name = "nibKeyValue";
            this.nibKeyValue.Size = new System.Drawing.Size(60, 20);
            this.nibKeyValue.TabIndex = 2;
            this.nibKeyValue.Text = "0";
            this.nibKeyValue.ValueChanged += new System.EventHandler(this.numericInputBox2_ValueChanged);
            // 
            // cbTransform
            // 
            this.cbTransform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTransform.FormattingEnabled = true;
            this.cbTransform.Items.AddRange(new object[] {
            "Scale X",
            "Scale Y",
            "Scale Z",
            "Rotation X",
            "Rotation Y",
            "Rotation Z",
            "Translation X",
            "Translation Y",
            "Translation Z"});
            this.cbTransform.Location = new System.Drawing.Point(127, 1);
            this.cbTransform.Name = "cbTransform";
            this.cbTransform.Size = new System.Drawing.Size(110, 21);
            this.cbTransform.TabIndex = 6;
            this.cbTransform.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
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
            this.chkLinear.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mItem_display_showEditValsAsLinear});
            this.chkLinear.Name = "chkLinear";
            this.chkLinear.Size = new System.Drawing.Size(177, 22);
            this.chkLinear.Text = "Linear";
            this.chkLinear.CheckedChanged += new System.EventHandler(this.chkLinear_CheckedChanged);
            // 
            // mItem_display_showEditValsAsLinear
            // 
            this.mItem_display_showEditValsAsLinear.Checked = true;
            this.mItem_display_showEditValsAsLinear.CheckOnClick = true;
            this.mItem_display_showEditValsAsLinear.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mItem_display_showEditValsAsLinear.Name = "mItem_display_showEditValsAsLinear";
            this.mItem_display_showEditValsAsLinear.Size = new System.Drawing.Size(216, 22);
            this.mItem_display_showEditValsAsLinear.Text = "Show/Edit Values As Linear";
            this.mItem_display_showEditValsAsLinear.CheckedChanged += new System.EventHandler(this.mItem_display_showEditValsAsLinear_CheckedChanged);
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
            this.chkGenTans.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mItem_genTan_alterSelTanOnDrag,
            this.mItem_genTan_alterAdjTan});
            this.chkGenTans.Name = "chkGenTans";
            this.chkGenTans.Size = new System.Drawing.Size(228, 22);
            this.chkGenTans.Text = "Generate tangents";
            this.chkGenTans.CheckedChanged += new System.EventHandler(this.chkGenTans_CheckedChanged);
            // 
            // mItem_genTan_alterSelTanOnDrag
            // 
            this.mItem_genTan_alterSelTanOnDrag.Checked = true;
            this.mItem_genTan_alterSelTanOnDrag.CheckOnClick = true;
            this.mItem_genTan_alterSelTanOnDrag.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mItem_genTan_alterSelTanOnDrag.Name = "mItem_genTan_alterSelTanOnDrag";
            this.mItem_genTan_alterSelTanOnDrag.Size = new System.Drawing.Size(240, 22);
            this.mItem_genTan_alterSelTanOnDrag.Text = "Alter Selected Tangent On Drag";
            this.mItem_genTan_alterSelTanOnDrag.CheckedChanged += new System.EventHandler(this.mItem_genTan_alterSelTanOnDrag_CheckedChanged);
            // 
            // mItem_genTan_alterAdjTan
            // 
            this.mItem_genTan_alterAdjTan.Checked = true;
            this.mItem_genTan_alterAdjTan.CheckOnClick = true;
            this.mItem_genTan_alterAdjTan.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mItem_genTan_alterAdjTan.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mItem_genTan_alterAdjTan_OnSet,
            this.mItem_genTan_alterAdjTan_OnDel,
            this.mItem_genTan_alterAdjTan_OnDrag});
            this.mItem_genTan_alterAdjTan.Name = "mItem_genTan_alterAdjTan";
            this.mItem_genTan_alterAdjTan.Size = new System.Drawing.Size(240, 22);
            this.mItem_genTan_alterAdjTan.Text = "Alter Adjacent Tangents";
            this.mItem_genTan_alterAdjTan.CheckedChanged += new System.EventHandler(this.mItem_genTan_alterAdjTan_CheckedChanged);
            // 
            // mItem_genTan_alterAdjTan_OnSet
            // 
            this.mItem_genTan_alterAdjTan_OnSet.Checked = true;
            this.mItem_genTan_alterAdjTan_OnSet.CheckOnClick = true;
            this.mItem_genTan_alterAdjTan_OnSet.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mItem_genTan_alterAdjTan_OnSet.Name = "mItem_genTan_alterAdjTan_OnSet";
            this.mItem_genTan_alterAdjTan_OnSet.Size = new System.Drawing.Size(163, 22);
            this.mItem_genTan_alterAdjTan_OnSet.Text = "KeyFrame Create";
            this.mItem_genTan_alterAdjTan_OnSet.CheckedChanged += new System.EventHandler(this.mItem_genTan_alterAdjTan_OnSet_CheckedChanged);
            // 
            // mItem_genTan_alterAdjTan_OnDel
            // 
            this.mItem_genTan_alterAdjTan_OnDel.Checked = true;
            this.mItem_genTan_alterAdjTan_OnDel.CheckOnClick = true;
            this.mItem_genTan_alterAdjTan_OnDel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mItem_genTan_alterAdjTan_OnDel.Name = "mItem_genTan_alterAdjTan_OnDel";
            this.mItem_genTan_alterAdjTan_OnDel.Size = new System.Drawing.Size(163, 22);
            this.mItem_genTan_alterAdjTan_OnDel.Text = "KeyFrame Delete";
            this.mItem_genTan_alterAdjTan_OnDel.CheckedChanged += new System.EventHandler(this.mItem_genTan_alterAdjTan_OnDel_CheckedChanged);
            // 
            // mItem_genTan_alterAdjTan_OnDrag
            // 
            this.mItem_genTan_alterAdjTan_OnDrag.Checked = true;
            this.mItem_genTan_alterAdjTan_OnDrag.CheckOnClick = true;
            this.mItem_genTan_alterAdjTan_OnDrag.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mItem_genTan_alterAdjTan_OnDrag.Name = "mItem_genTan_alterAdjTan_OnDrag";
            this.mItem_genTan_alterAdjTan_OnDrag.Size = new System.Drawing.Size(163, 22);
            this.mItem_genTan_alterAdjTan_OnDrag.Text = "KeyFrame Drag";
            this.mItem_genTan_alterAdjTan_OnDrag.CheckedChanged += new System.EventHandler(this.mItem_genTan_alterAdjTan_OnDrag_CheckedChanged);
            // 
            // interpolationViewer
            // 
            this.interpolationViewer.AllKeyframes = true;
            this.interpolationViewer.AlterAdjTangent_OnSelectedDrag = true;
            this.interpolationViewer.AlterSelectedTangent_OnDrag = true;
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
        private global::System.Windows.Forms.NumericInputBox nibKeyFrame;
        private global::System.Windows.Forms.Label label3;
        private global::System.Windows.Forms.Label label2;
        private global::System.Windows.Forms.NumericInputBox nibTangent;
        private global::System.Windows.Forms.NumericInputBox nibKeyValue;
        private global::System.Windows.Forms.ComboBox cbTransform;
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
        private NumericInputBox nibTanLen;
        private ToolStripMenuItem mItem_display_showEditValsAsLinear;
        private ToolStripMenuItem mItem_genTan_alterSelTanOnDrag;
        private ToolStripMenuItem mItem_genTan_alterAdjTan;
        private ToolStripMenuItem mItem_genTan_alterAdjTan_OnSet;
        private ToolStripMenuItem mItem_genTan_alterAdjTan_OnDel;
        private ToolStripMenuItem mItem_genTan_alterAdjTan_OnDrag;
    }
}
