using System;
using System.Windows.Forms;
using System.IO;
using BrawlLib.SSBB;
using System.Collections.Generic;
namespace BrawlBox
{
    class SettingsDialog : Form
    {
        private bool _updating;

        static SettingsDialog()
        {
            foreach (SupportedFileInfo info in SupportedFilesHandler.Files)
                if (info._forEditing)
                    foreach (string s in info._extensions)
                    {
                        _assocList.Add(FileAssociation.Get("." + s));
                        _typeList.Add(FileType.Get("SSBB." + s.ToUpper()));
                    }
        }

        private static List<FileAssociation> _assocList = new List<FileAssociation>();
        private static List<FileType> _typeList = new List<FileType>();

        private CheckBox chkShowPropDesc;
        private CheckBox chkUpdatesOnStartup;

        public SettingsDialog()
        {
            InitializeComponent();

            listView1.Items.Clear();
            foreach (SupportedFileInfo info in SupportedFilesHandler.Files)
                foreach (string s in info._extensions)
                    listView1.Items.Add(new ListViewItem() { Text = String.Format("{0} (*.{1})", info._name, s) });
        }

        private void Apply()
        {
            try
            {
                bool check;
                int index = 0;
                foreach (ListViewItem i in listView1.Items)
                {
                    if ((check = i.Checked) != (bool)i.Tag)
                    {
                        if (check)
                        {
                            _assocList[index].FileType = _typeList[index];
                            _typeList[index].SetCommand("open", String.Format("\"{0}\" \"%1\"", Program.FullPath));
                        }
                        else
                        {
                            _typeList[index].Delete();
                            _assocList[index].Delete();
                        }
                        i.Tag = check;
                    }
                    index++;
                }
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show(null, "Unable to access the registry to set file associations.\nRun the program as administrator and try again.", "Insufficient Privileges", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnApply.Enabled = false;
            }
        }
        private void SettingsDialog_Shown(object sender, EventArgs e)
        {
            int index = 0;
            string cmd;
            foreach (ListViewItem i in listView1.Items)
            {
                try
                {
                    if ((_typeList[index] == _assocList[index].FileType) &&
                        (!String.IsNullOrEmpty(cmd = _typeList[index].GetCommand("open"))) &&
                        (cmd.IndexOf(Program.FullPath, StringComparison.OrdinalIgnoreCase) >= 0))
                        i.Tag = i.Checked = true;
                    else
                        i.Tag = i.Checked = false;
                }
                catch
                {

                }
                index++;
            }

            _updating = true;
            chkUpdatesOnStartup.Checked = MainForm.Instance.CheckUpdatesOnStartup;
            chkShowPropDesc.Checked = MainForm.Instance.DisplayPropertyDescriptionsWhenAvailable;
            _updating = false;
            btnApply.Enabled = false;      
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            btnApply.Enabled = true;
        }
        private void btnApply_Click(object sender, EventArgs e)
        {
            Apply();
        }
        private void btnOkay_Click(object sender, EventArgs e)
        {
            if (btnApply.Enabled)
                Apply();

            this.DialogResult = DialogResult.OK;
            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
        
        #region Designer

        private GroupBox groupBox1;
        private ListView listView1;
        private CheckBox checkBox1;
        private Button btnOkay;
        private Button btnCancel;
        private Button btnApply;
        private ColumnHeader columnHeader1;
    
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("File Types", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Resource Types", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem30 = new System.Windows.Forms.ListViewItem("ARChive Pack (*.pac)");
            System.Windows.Forms.ListViewItem listViewItem31 = new System.Windows.Forms.ListViewItem("Compressed ARChive Pack (*.pcs)");
            System.Windows.Forms.ListViewItem listViewItem32 = new System.Windows.Forms.ListViewItem("ARChive (*.arc)");
            System.Windows.Forms.ListViewItem listViewItem33 = new System.Windows.Forms.ListViewItem("Compressed ARChive (*.szs)");
            System.Windows.Forms.ListViewItem listViewItem34 = new System.Windows.Forms.ListViewItem("Resource Pack (*.brres)");
            System.Windows.Forms.ListViewItem listViewItem35 = new System.Windows.Forms.ListViewItem("Model Pack (*.brmdl)");
            System.Windows.Forms.ListViewItem listViewItem36 = new System.Windows.Forms.ListViewItem("Texture Pack (*.brtex)");
            System.Windows.Forms.ListViewItem listViewItem37 = new System.Windows.Forms.ListViewItem("MSBin Message List (*.msbin)");
            System.Windows.Forms.ListViewItem listViewItem38 = new System.Windows.Forms.ListViewItem("Sound Archive (*.brsar)");
            System.Windows.Forms.ListViewItem listViewItem39 = new System.Windows.Forms.ListViewItem("Sound Stream (*.brstm)");
            System.Windows.Forms.ListViewItem listViewItem40 = new System.Windows.Forms.ListViewItem("Texture (*.tex0)");
            System.Windows.Forms.ListViewItem listViewItem41 = new System.Windows.Forms.ListViewItem("Palette (*.plt0)");
            System.Windows.Forms.ListViewItem listViewItem42 = new System.Windows.Forms.ListViewItem("Model (*.mdl0)");
            System.Windows.Forms.ListViewItem listViewItem43 = new System.Windows.Forms.ListViewItem("Model Animation (*.chr0)");
            System.Windows.Forms.ListViewItem listViewItem44 = new System.Windows.Forms.ListViewItem("Texture Animation (*.srt0)");
            System.Windows.Forms.ListViewItem listViewItem45 = new System.Windows.Forms.ListViewItem("Vertex Morph (*.shp0)");
            System.Windows.Forms.ListViewItem listViewItem46 = new System.Windows.Forms.ListViewItem("Texture Pattern (*.pat0)");
            System.Windows.Forms.ListViewItem listViewItem47 = new System.Windows.Forms.ListViewItem("Bone Visibility (*.vis0)");
            System.Windows.Forms.ListViewItem listViewItem48 = new System.Windows.Forms.ListViewItem("Scene Settings (*.scn0)");
            System.Windows.Forms.ListViewItem listViewItem49 = new System.Windows.Forms.ListViewItem("Color Sequence (*.clr0)");
            System.Windows.Forms.ListViewItem listViewItem50 = new System.Windows.Forms.ListViewItem("Effect List (*.efls)");
            System.Windows.Forms.ListViewItem listViewItem51 = new System.Windows.Forms.ListViewItem("Effect Parameters (*.breff)");
            System.Windows.Forms.ListViewItem listViewItem52 = new System.Windows.Forms.ListViewItem("Effect Textures (*.breft)");
            System.Windows.Forms.ListViewItem listViewItem53 = new System.Windows.Forms.ListViewItem("Sound Stream (*.brwsd)");
            System.Windows.Forms.ListViewItem listViewItem54 = new System.Windows.Forms.ListViewItem("Sound Bank (*.brbnk)");
            System.Windows.Forms.ListViewItem listViewItem55 = new System.Windows.Forms.ListViewItem("Sound Sequence (*.brseq)");
            System.Windows.Forms.ListViewItem listViewItem56 = new System.Windows.Forms.ListViewItem("Static Module (*.dol)");
            System.Windows.Forms.ListViewItem listViewItem57 = new System.Windows.Forms.ListViewItem("Relocatable Module (*.rel)");
            System.Windows.Forms.ListViewItem listViewItem58 = new System.Windows.Forms.ListViewItem("Texture Archive (*.tpl)");
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnOkay = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.chkShowPropDesc = new System.Windows.Forms.CheckBox();
            this.chkUpdatesOnStartup = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.listView1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(268, 262);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "File Associations";
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox1.Location = new System.Drawing.Point(151, 13);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBox1.Size = new System.Drawing.Size(104, 20);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "Check All";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.AutoArrange = false;
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView1.CheckBoxes = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            listViewGroup3.Header = "File Types";
            listViewGroup3.Name = "grpFileTypes";
            listViewGroup4.Header = "Resource Types";
            listViewGroup4.Name = "grpResTypes";
            this.listView1.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup3,
            listViewGroup4});
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView1.HideSelection = false;
            listViewItem30.StateImageIndex = 0;
            listViewItem30.Tag = "";
            listViewItem31.StateImageIndex = 0;
            listViewItem31.Tag = "";
            listViewItem32.StateImageIndex = 0;
            listViewItem33.StateImageIndex = 0;
            listViewItem34.StateImageIndex = 0;
            listViewItem34.Tag = "";
            listViewItem35.StateImageIndex = 0;
            listViewItem36.StateImageIndex = 0;
            listViewItem37.StateImageIndex = 0;
            listViewItem37.Tag = "";
            listViewItem38.StateImageIndex = 0;
            listViewItem39.StateImageIndex = 0;
            listViewItem40.StateImageIndex = 0;
            listViewItem41.StateImageIndex = 0;
            listViewItem42.StateImageIndex = 0;
            listViewItem43.StateImageIndex = 0;
            listViewItem44.StateImageIndex = 0;
            listViewItem45.StateImageIndex = 0;
            listViewItem46.StateImageIndex = 0;
            listViewItem47.StateImageIndex = 0;
            listViewItem48.StateImageIndex = 0;
            listViewItem49.StateImageIndex = 0;
            listViewItem50.StateImageIndex = 0;
            listViewItem51.StateImageIndex = 0;
            listViewItem52.StateImageIndex = 0;
            listViewItem53.StateImageIndex = 0;
            listViewItem54.StateImageIndex = 0;
            listViewItem55.StateImageIndex = 0;
            listViewItem56.StateImageIndex = 0;
            listViewItem57.StateImageIndex = 0;
            listViewItem58.StateImageIndex = 0;
            this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem30,
            listViewItem31,
            listViewItem32,
            listViewItem33,
            listViewItem34,
            listViewItem35,
            listViewItem36,
            listViewItem37,
            listViewItem38,
            listViewItem39,
            listViewItem40,
            listViewItem41,
            listViewItem42,
            listViewItem43,
            listViewItem44,
            listViewItem45,
            listViewItem46,
            listViewItem47,
            listViewItem48,
            listViewItem49,
            listViewItem50,
            listViewItem51,
            listViewItem52,
            listViewItem53,
            listViewItem54,
            listViewItem55,
            listViewItem56,
            listViewItem57,
            listViewItem58});
            this.listView1.Location = new System.Drawing.Point(3, 37);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(262, 219);
            this.listView1.TabIndex = 6;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView1_ItemChecked);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 300;
            // 
            // btnOkay
            // 
            this.btnOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOkay.Location = new System.Drawing.Point(30, 328);
            this.btnOkay.Name = "btnOkay";
            this.btnOkay.Size = new System.Drawing.Size(75, 23);
            this.btnOkay.TabIndex = 1;
            this.btnOkay.Text = "Okay";
            this.btnOkay.UseVisualStyleBackColor = true;
            this.btnOkay.Click += new System.EventHandler(this.btnOkay_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(111, 328);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(192, 328);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 23);
            this.btnApply.TabIndex = 3;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // chkShowPropDesc
            // 
            this.chkShowPropDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkShowPropDesc.AutoSize = true;
            this.chkShowPropDesc.Location = new System.Drawing.Point(25, 305);
            this.chkShowPropDesc.Name = "chkShowPropDesc";
            this.chkShowPropDesc.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkShowPropDesc.Size = new System.Drawing.Size(242, 17);
            this.chkShowPropDesc.TabIndex = 7;
            this.chkShowPropDesc.Text = "Show property description box when available";
            this.chkShowPropDesc.UseVisualStyleBackColor = true;
            this.chkShowPropDesc.CheckedChanged += new System.EventHandler(this.chkShowPropDesc_CheckedChanged);
            // 
            // chkUpdatesOnStartup
            // 
            this.chkUpdatesOnStartup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUpdatesOnStartup.AutoSize = true;
            this.chkUpdatesOnStartup.Location = new System.Drawing.Point(104, 282);
            this.chkUpdatesOnStartup.Name = "chkUpdatesOnStartup";
            this.chkUpdatesOnStartup.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkUpdatesOnStartup.Size = new System.Drawing.Size(163, 17);
            this.chkUpdatesOnStartup.TabIndex = 7;
            this.chkUpdatesOnStartup.Text = "Check for updates on startup";
            this.chkUpdatesOnStartup.UseVisualStyleBackColor = true;
            this.chkUpdatesOnStartup.CheckedChanged += new System.EventHandler(this.chkUpdatesOnStartup_CheckedChanged);
            // 
            // SettingsDialog
            // 
            this.ClientSize = new System.Drawing.Size(292, 363);
            this.Controls.Add(this.chkUpdatesOnStartup);
            this.Controls.Add(this.chkShowPropDesc);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOkay);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "SettingsDialog";
            this.Text = "Settings";
            this.Shown += new System.EventHandler(this.SettingsDialog_Shown);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                bool check = checkBox1.Checked;
                foreach (ListViewItem i in listView1.Items)
                    i.Checked = check;
            }
        }

        private void chkShowPropDesc_CheckedChanged(object sender, EventArgs e)
        {
            if (!_updating)
                MainForm.Instance.DisplayPropertyDescriptionsWhenAvailable = chkShowPropDesc.Checked;
        }

        private void chkUpdatesOnStartup_CheckedChanged(object sender, EventArgs e)
        {
            if (!_updating)
                MainForm.Instance.CheckUpdatesOnStartup = chkUpdatesOnStartup.Checked;
        }
    }
}