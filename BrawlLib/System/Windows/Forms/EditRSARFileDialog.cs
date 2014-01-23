using System;
using System.Windows.Forms;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Wii.Animations;
using System.Audio;
using BrawlLib;

namespace System.Windows.Forms
{
    public class EditRSARFileDialog : Form
    {
        public EditRSARFileDialog() { InitializeComponent(); }

        private AudioPlaybackPanel audioPlaybackPanel1;
        private ContextMenuStrip ctxData;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem replaceToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ContextMenuStrip ctxSounds;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem2;
        private Button button1;

        private RSARFileNode _targetNode;
        public RSARFileNode TargetNode { get { return _targetNode; } set { _targetNode = value; TargetChanged(); } }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            audioPlaybackPanel1.TargetSource = null;
            base.OnClosing(e);
        }

        public DialogResult ShowDialog(IWin32Window owner, RSARFileNode node)
        {
            TargetNode = node;
            TargetNode.UpdateCurrControl += OnUpdateCurrControl;
            return base.ShowDialog();
        }

        private unsafe void btnOkay_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            audioPlaybackPanel1.TargetSource = null;
            TargetNode.UpdateCurrControl -= OnUpdateCurrControl;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) { DialogResult = DialogResult.Cancel; Close(); }

        internal protected virtual void OnUpdateCurrControl(object sender, EventArgs e)
        {
            soundsListBox_SelectedIndexChanged(this, null);
        }

        #region Designer

        private Panel panel1;
        private Panel panel2;
        private Panel panel4;
        private Splitter splitter2;
        private Panel panel3;
        private Splitter splitter1;
        private PropertyGrid propertyGrid;
        private ListBox soundsListBox;
        private Label label2;
        private ListBox dataListBox;
        private Label label1;
        private Button btnOkay;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnOkay = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dataListBox = new System.Windows.Forms.ListBox();
            this.ctxData = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.panel4 = new System.Windows.Forms.Panel();
            this.soundsListBox = new System.Windows.Forms.ListBox();
            this.ctxSounds = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.audioPlaybackPanel1 = new System.Windows.Forms.AudioPlaybackPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.ctxData.SuspendLayout();
            this.panel4.SuspendLayout();
            this.ctxSounds.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOkay
            // 
            this.btnOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOkay.Location = new System.Drawing.Point(369, 3);
            this.btnOkay.Name = "btnOkay";
            this.btnOkay.Size = new System.Drawing.Size(75, 23);
            this.btnOkay.TabIndex = 1;
            this.btnOkay.Text = "&Done";
            this.btnOkay.UseVisualStyleBackColor = true;
            this.btnOkay.Click += new System.EventHandler(this.btnOkay_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnOkay);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 320);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(447, 31);
            this.panel1.TabIndex = 3;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.splitter2);
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.MinimumSize = new System.Drawing.Size(54, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(107, 320);
            this.panel2.TabIndex = 4;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.dataListBox);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.MinimumSize = new System.Drawing.Size(0, 15);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(107, 160);
            this.panel3.TabIndex = 1;
            // 
            // dataListBox
            // 
            this.dataListBox.ContextMenuStrip = this.ctxData;
            this.dataListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataListBox.FormattingEnabled = true;
            this.dataListBox.IntegralHeight = false;
            this.dataListBox.Location = new System.Drawing.Point(0, 15);
            this.dataListBox.Name = "dataListBox";
            this.dataListBox.Size = new System.Drawing.Size(107, 145);
            this.dataListBox.TabIndex = 3;
            this.dataListBox.SelectedIndexChanged += new System.EventHandler(this.dataListBox_SelectedIndexChanged);
            // 
            // ctxData
            // 
            this.ctxData.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.replaceToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.ctxData.Name = "contextMenuStrip1";
            this.ctxData.Size = new System.Drawing.Size(116, 48);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.replaceToolStripMenuItem.Text = "Replace";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.replaceToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Data";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter2.Location = new System.Drawing.Point(0, 160);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(107, 3);
            this.splitter2.TabIndex = 0;
            this.splitter2.TabStop = false;
            this.splitter2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitter2_SplitterMoved);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.soundsListBox);
            this.panel4.Controls.Add(this.label2);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 163);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.MinimumSize = new System.Drawing.Size(0, 15);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(107, 157);
            this.panel4.TabIndex = 2;
            // 
            // soundsListBox
            // 
            this.soundsListBox.ContextMenuStrip = this.ctxSounds;
            this.soundsListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.soundsListBox.FormattingEnabled = true;
            this.soundsListBox.IntegralHeight = false;
            this.soundsListBox.Location = new System.Drawing.Point(0, 15);
            this.soundsListBox.Name = "soundsListBox";
            this.soundsListBox.Size = new System.Drawing.Size(107, 142);
            this.soundsListBox.TabIndex = 2;
            this.soundsListBox.SelectedIndexChanged += new System.EventHandler(this.soundsListBox_SelectedIndexChanged);
            // 
            // ctxSounds
            // 
            this.ctxSounds.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2});
            this.ctxSounds.Name = "contextMenuStrip1";
            this.ctxSounds.Size = new System.Drawing.Size(116, 48);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            this.toolStripMenuItem1.Text = "Replace";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(115, 22);
            this.toolStripMenuItem2.Text = "Export";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Sounds";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(107, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 320);
            this.splitter1.TabIndex = 0;
            this.splitter1.TabStop = false;
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.HelpVisible = false;
            this.propertyGrid.Location = new System.Drawing.Point(110, 21);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(337, 188);
            this.propertyGrid.TabIndex = 5;
            // 
            // audioPlaybackPanel1
            // 
            this.audioPlaybackPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.audioPlaybackPanel1.Location = new System.Drawing.Point(110, 209);
            this.audioPlaybackPanel1.Name = "audioPlaybackPanel1";
            this.audioPlaybackPanel1.Size = new System.Drawing.Size(337, 111);
            this.audioPlaybackPanel1.TabIndex = 6;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Top;
            this.button1.Location = new System.Drawing.Point(110, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(337, 21);
            this.button1.TabIndex = 8;
            this.button1.Text = "View Entries";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // EditRSARFileDialog
            // 
            this.AcceptButton = this.btnOkay;
            this.ClientSize = new System.Drawing.Size(447, 351);
            this.Controls.Add(this.propertyGrid);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.audioPlaybackPanel1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "EditRSARFileDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit RSAR File";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ctxData.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ctxSounds.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        public void TargetChanged()
        {
            dataListBox.Items.Clear();
            soundsListBox.Items.Clear();

            if (TargetNode is RSEQNode)
            {
                splitter2.Visible = panel4.Visible = audioPlaybackPanel1.Visible = false;

                dataListBox.Items.AddRange(TargetNode.Children.ToArray());
                if (dataListBox.Items.Count > 0) dataListBox.SelectedIndex = 0;
            }
            else
            {
                splitter2.Visible = panel4.Visible = audioPlaybackPanel1.Visible = true;

                dataListBox.Items.AddRange(TargetNode.Children[0].Children.ToArray());
                if (dataListBox.Items.Count > 0) 
                    dataListBox.SelectedIndex = 0;

                if (TargetNode.Children.Count > 1)
                    soundsListBox.Items.AddRange(TargetNode.Children[1].Children.ToArray());
                if (soundsListBox.Items.Count > 0) 
                    soundsListBox.SelectedIndex = 0;
            }
            button1.Visible = TargetNode is RBNKNode;

            if (TargetNode != null)
                Text = "Edit RSAR File - " + TargetNode.Name;
            else
                Text = "Edit RSAR File";
        }

        private void splitter2_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void dataListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dataListBox.SelectedIndex < 0) return;
            ResourceNode r = dataListBox.Items[dataListBox.SelectedIndex] as ResourceNode;
            propertyGrid.SelectedObject = r;
            int w;
            if (TargetNode is RWSDNode)
            {
                w = (r as RWSDDataNode)._part3._waveIndex;
                if (w < soundsListBox.Items.Count)
                    soundsListBox.SelectedIndex = w;
            } 
            else if (TargetNode is RBNKNode && r is RBNKDataInstParamNode)
            {
                w = (r as RBNKDataInstParamNode).hdr._waveIndex;
                if (w < soundsListBox.Items.Count)
                    soundsListBox.SelectedIndex = w;
            }
            button1.Enabled = !(button1.Text != "Back" && r is RBNKDataEntryNode);
        }

        private void soundsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (soundsListBox.SelectedIndex < 0) return;
            ResourceNode r = soundsListBox.Items[soundsListBox.SelectedIndex] as ResourceNode;
            if (r is IAudioSource)
                audioPlaybackPanel1.TargetSource = r as IAudioSource;
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataListBox.SelectedIndex < 0) return;
            ResourceNode r = dataListBox.Items[dataListBox.SelectedIndex] as ResourceNode;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = FileFilters.Raw;
                if (dlg.ShowDialog() == DialogResult.OK)
                    r.Replace(dlg.FileName);
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataListBox.SelectedIndex < 0) return;
            ResourceNode r = dataListBox.Items[dataListBox.SelectedIndex] as ResourceNode;
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.FileName = r.Name;
                dlg.Filter = FileFilters.Raw;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    r.Export(dlg.FileName);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "View Entries")
            {
                button1.Text = "Back";
                if (dataListBox.SelectedIndex < 0) return;
                RBNKEntryNode entry = dataListBox.Items[dataListBox.SelectedIndex] as RBNKEntryNode;
                label1.Text = entry.Name;
                dataListBox.Items.Clear();
                dataListBox.Items.AddRange(entry.Children.ToArray());
                if (dataListBox.Items.Count > 0) dataListBox.SelectedIndex = 0;
            }
            else
            {
                button1.Text = "View Entries";
                dataListBox.Items.Clear();
                dataListBox.Items.AddRange(TargetNode.Children[0].Children.ToArray());
                if (dataListBox.Items.Count > 0) dataListBox.SelectedIndex = 0;
                label1.Text = "Data";
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (soundsListBox.SelectedIndex < 0) return;
            ResourceNode r = soundsListBox.Items[soundsListBox.SelectedIndex] as ResourceNode;
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.FileName = r.Name;
                dlg.Filter = FileFilters.WAV + "|" + FileFilters.Raw;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    r.Export(dlg.FileName);
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (soundsListBox.SelectedIndex < 0) return;
            ResourceNode r = soundsListBox.Items[soundsListBox.SelectedIndex] as ResourceNode;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = FileFilters.WAV + "|" + FileFilters.Raw;
                if (dlg.ShowDialog() == DialogResult.OK)
                    r.Replace(dlg.FileName);
            }
        }
    }
}
