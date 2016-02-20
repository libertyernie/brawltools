using System;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;
using System.Diagnostics;
using System.Collections;
using BrawlLib;
using System.ComponentModel;
using System.Threading;
using BrawlLib.SSBB;

namespace System.Windows.Forms
{
    public class SoundPackControl : UserControl
    {
        #region Designer

        public ListView lstSets;
        private ColumnHeader clmIndex;
        private ColumnHeader clmName;
        private ContextMenuStrip contextMenuStrip1;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem mnuExport;
        private ToolStripMenuItem mnuReplace;
        private ToolStripMenuItem mnuPath;
        private ColumnHeader clmType;
        private ColumnHeader clmDataOffset;
        private ColumnHeader clmAudioOffset;
        private ColumnHeader clmEntryOffset;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem newFileToolStripMenuItem;
        private ToolStripMenuItem rWSDToolStripMenuItem;
        private ToolStripMenuItem rSEQToolStripMenuItem;
        private ToolStripMenuItem rBNKToolStripMenuItem;
        private ToolStripMenuItem externalReferenceToolStripMenuItem;
        private ToolStripMenuItem rSTMToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.clmIndex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lstSets = new System.Windows.Forms.ListView();
            this.clmType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmDataOffset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmAudioOffset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmEntryOffset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuPath = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExport = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuReplace = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.newFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rWSDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rSEQToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rBNKToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.externalReferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rSTMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // clmIndex
            // 
            this.clmIndex.Text = "Index";
            this.clmIndex.Width = 40;
            // 
            // clmName
            // 
            this.clmName.Text = "Name";
            this.clmName.Width = 40;
            // 
            // lstSets
            // 
            this.lstSets.AutoArrange = false;
            this.lstSets.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstSets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmIndex,
            this.clmType,
            this.clmName,
            this.clmDataOffset,
            this.clmAudioOffset,
            this.clmEntryOffset});
            this.lstSets.ContextMenuStrip = this.contextMenuStrip1;
            this.lstSets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstSets.FullRowSelect = true;
            this.lstSets.GridLines = true;
            this.lstSets.HideSelection = false;
            this.lstSets.LabelWrap = false;
            this.lstSets.Location = new System.Drawing.Point(0, 28);
            this.lstSets.MultiSelect = false;
            this.lstSets.Name = "lstSets";
            this.lstSets.Size = new System.Drawing.Size(389, 225);
            this.lstSets.TabIndex = 0;
            this.lstSets.UseCompatibleStateImageBehavior = false;
            this.lstSets.View = System.Windows.Forms.View.Details;
            this.lstSets.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstSets_ColumnClick);
            this.lstSets.SelectedIndexChanged += new System.EventHandler(this.lstSets_SelectedIndexChanged);
            this.lstSets.DoubleClick += new System.EventHandler(this.lstSets_DoubleClick);
            this.lstSets.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstSets_KeyDown);
            // 
            // clmType
            // 
            this.clmType.Text = "Type";
            // 
            // clmDataOffset
            // 
            this.clmDataOffset.Text = "Data Offset";
            this.clmDataOffset.Width = 70;
            // 
            // clmAudioOffset
            // 
            this.clmAudioOffset.Text = "Audio Offset";
            this.clmAudioOffset.Width = 70;
            // 
            // clmEntryOffset
            // 
            this.clmEntryOffset.Text = "Entry Offset";
            this.clmEntryOffset.Width = 80;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuPath,
            this.mnuExport,
            this.mnuReplace,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(138, 108);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // mnuPath
            // 
            this.mnuPath.Name = "mnuPath";
            this.mnuPath.Size = new System.Drawing.Size(137, 26);
            this.mnuPath.Text = "Path...";
            this.mnuPath.Click += new System.EventHandler(this.mnuPath_Click);
            // 
            // mnuExport
            // 
            this.mnuExport.Name = "mnuExport";
            this.mnuExport.Size = new System.Drawing.Size(137, 26);
            this.mnuExport.Text = "Export";
            this.mnuExport.Click += new System.EventHandler(this.mnuExport_Click);
            // 
            // mnuReplace
            // 
            this.mnuReplace.Name = "mnuReplace";
            this.mnuReplace.Size = new System.Drawing.Size(137, 26);
            this.mnuReplace.Text = "Replace";
            this.mnuReplace.Click += new System.EventHandler(this.mnuReplace_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(137, 26);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newFileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(389, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // newFileToolStripMenuItem
            // 
            this.newFileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rWSDToolStripMenuItem,
            this.rSEQToolStripMenuItem,
            this.rBNKToolStripMenuItem,
            this.rSTMToolStripMenuItem,
            this.externalReferenceToolStripMenuItem});
            this.newFileToolStripMenuItem.Name = "newFileToolStripMenuItem";
            this.newFileToolStripMenuItem.Size = new System.Drawing.Size(78, 24);
            this.newFileToolStripMenuItem.Text = "New File";
            // 
            // rWSDToolStripMenuItem
            // 
            this.rWSDToolStripMenuItem.Name = "rWSDToolStripMenuItem";
            this.rWSDToolStripMenuItem.Size = new System.Drawing.Size(207, 26);
            this.rWSDToolStripMenuItem.Text = "RWSD";
            this.rWSDToolStripMenuItem.Click += new System.EventHandler(this.rWSDToolStripMenuItem_Click);
            // 
            // rSEQToolStripMenuItem
            // 
            this.rSEQToolStripMenuItem.Name = "rSEQToolStripMenuItem";
            this.rSEQToolStripMenuItem.Size = new System.Drawing.Size(207, 26);
            this.rSEQToolStripMenuItem.Text = "RSEQ";
            this.rSEQToolStripMenuItem.Click += new System.EventHandler(this.rSEQToolStripMenuItem_Click);
            // 
            // rBNKToolStripMenuItem
            // 
            this.rBNKToolStripMenuItem.Name = "rBNKToolStripMenuItem";
            this.rBNKToolStripMenuItem.Size = new System.Drawing.Size(207, 26);
            this.rBNKToolStripMenuItem.Text = "RBNK";
            this.rBNKToolStripMenuItem.Click += new System.EventHandler(this.rBNKToolStripMenuItem_Click);
            // 
            // externalReferenceToolStripMenuItem
            // 
            this.externalReferenceToolStripMenuItem.Name = "externalReferenceToolStripMenuItem";
            this.externalReferenceToolStripMenuItem.Size = new System.Drawing.Size(207, 26);
            this.externalReferenceToolStripMenuItem.Text = "External Reference";
            this.externalReferenceToolStripMenuItem.Click += new System.EventHandler(this.externalReferenceToolStripMenuItem_Click);
            // 
            // rSTMToolStripMenuItem
            // 
            this.rSTMToolStripMenuItem.Name = "rSTMToolStripMenuItem";
            this.rSTMToolStripMenuItem.Size = new System.Drawing.Size(207, 26);
            this.rSTMToolStripMenuItem.Text = "RSTM";
            this.rSTMToolStripMenuItem.Click += new System.EventHandler(this.rSTMToolStripMenuItem_Click);
            // 
            // SoundPackControl
            // 
            this.Controls.Add(this.lstSets);
            this.Controls.Add(this.menuStrip1);
            this.Name = "SoundPackControl";
            this.Size = new System.Drawing.Size(389, 253);
            this.DoubleClick += new System.EventHandler(this.lstSets_DoubleClick);
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public PropertyGrid _grid = null;

        private RSARNode _targetNode;
        public RSARNode TargetNode
        {
            get { return _targetNode; }
            set { if (value == _targetNode) return; _targetNode = value; NodeChanged(); }
        }

        private SoundPackItem _selectedItem;
        private ListViewColumnSorter lvwColumnSorter;

        public SoundPackControl()
        {
            InitializeComponent();

            lvwColumnSorter = new ListViewColumnSorter();
            lstSets.ListViewItemSorter = lvwColumnSorter;

            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.WorkerReportsProgress = false;
            backgroundWorker1.WorkerSupportsCancellation = false;
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
        }

        private void Update(ListView list)
        {
            list.BeginUpdate();
            list.Items.Clear();
            if (_targetNode != null)
                foreach (RSARFileNode file in _targetNode.Files)
                    list.Items.Add(new SoundPackItem(file));

            list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            list.EndUpdate();
        }

        delegate void delUpdate(ListView list);
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (lstSets.InvokeRequired)
            {
                delUpdate callbackMethod = new delUpdate(Update);
                this.Invoke(callbackMethod, lstSets);
            }
            else
                Update(lstSets);
        }

        BackgroundWorker backgroundWorker1;
        private void NodeChanged()
        {
            //if (backgroundWorker1.IsBusy != true && _targetNode != null)
            //    backgroundWorker1.RunWorkerAsync();
            Update(lstSets);
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_selectedItem == null)
                e.Cancel = true;
            else
                mnuExport.Enabled = !(_selectedItem._node is RSARExtFileNode);
        }

        private void mnuPath_Click(object sender, EventArgs e)
        {
            using (SoundPathChanger dlg = new SoundPathChanger())
            {
                dlg.FilePath = _selectedItem._node.FullExtPath;
                dlg.dlg.InitialDirectory = TargetNode._origPath.Substring(0, TargetNode._origPath.LastIndexOf('\\'));
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _selectedItem._node.FullExtPath = dlg.FilePath;
                    _selectedItem.SubItems[2].Text = _selectedItem._node._extPath;
                }
            }
        }

        private void lstSets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSets.SelectedIndices.Count == 0)
                _selectedItem = null;
            else
                _selectedItem = lstSets.SelectedItems[0] as SoundPackItem;

            if (_grid != null && _selectedItem != null)
                _grid.SelectedObject = _selectedItem._node;
        }

        private void mnuExport_Click(object sender, EventArgs e)
        {
            if (_selectedItem.SubItems[1].Text != "External")
                using (SaveFileDialog dlg = new SaveFileDialog())
                {
                    dlg.FileName = _selectedItem.SubItems[2].Text.Replace('/', '_');
                    switch (_selectedItem.SubItems[1].Text)
                    {
                        case "RWSD": dlg.Filter = FileFilters.RWSD; break;
                        case "RBNK": dlg.Filter = FileFilters.RBNK; break;
                        case "RSEQ": dlg.Filter = FileFilters.RSEQ; break;
                        case "RSAR": dlg.Filter = FileFilters.RSAR; break;
                    }
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                        _selectedItem._node.Export(dlg.FileName);
                }
        }

        private void lstSets_DoubleClick(object sender, EventArgs e)
        {
            if (_selectedItem._node is RSARExtFileNode)
            {
                if (File.Exists(_selectedItem._node.FullExtPath))
                    Process.Start(_selectedItem._node.FullExtPath);
                else
                    mnuPath_Click(this, null);
            }
            else
                new EditRSARFileDialog().ShowDialog(this, _selectedItem._node);
        }

        private void lstSets_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                    lvwColumnSorter.Order = SortOrder.Descending;
                else
                    lvwColumnSorter.Order = SortOrder.Ascending;
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            lstSets.Sort();
        }

        private void lstSets_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                lstSets.SelectedItems.Clear();
                if (_grid != null)
                    _grid.SelectedObject = _targetNode;
            }
        }

        private void mnuReplace_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = SupportedFilesHandler.GetCompleteFilter("brwsd", "brbnk", "brseq", "brstm");
                if (dlg.ShowDialog() == DialogResult.OK)
                    _selectedItem._node.Replace(dlg.FileName);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _selectedItem._node.Remove();
            lstSets.Items.Remove(_selectedItem);
        }

        private void rWSDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RWSDNode node = new RWSDNode()
            {
                _name = String.Format("[{0}] RWSD", _targetNode.Files.Count),
                _fileIndex = _targetNode.Files.Count
            };
            node.InitGroups();
            node._parent = _targetNode;
            _targetNode.Files.Add(node);
            Update(lstSets);
        }

        private void rSEQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RSEQNode node = new RSEQNode()
            {
                _name = String.Format("[{0}] RSEQ", _targetNode.Files.Count),
                _fileIndex = _targetNode.Files.Count
            };
            node._parent = _targetNode;
            _targetNode.Files.Add(node);
            Update(lstSets);
        }

        private void rBNKToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RBNKNode node = new RBNKNode()
            {
                _name = String.Format("[{0}] RBNK", _targetNode.Files.Count),
                _fileIndex = _targetNode.Files.Count
            };
            node.InitGroups();
            node._parent = _targetNode;
            _targetNode.Files.Add(node);
            Update(lstSets);
        }

        private void externalReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RSARExtFileNode node = new RSARExtFileNode()
            {
                _name = String.Format("[{0}] External", _targetNode.Files.Count),
                _fileIndex = _targetNode.Files.Count
            };
            node._parent = _targetNode;
            _targetNode.Files.Add(node);
            Update(lstSets);
        }

        private void rSTMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "PCM Audio (*.wav)|*.wav" })
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    RSTMNode r = new RSTMNode() { _fileIndex = _targetNode.Files.Count };
                    using (BrstmConverterDialog dlg = new BrstmConverterDialog())
                    {
                        dlg.AudioSource = ofd.FileName;
                        if (dlg.ShowDialog(this) == DialogResult.OK)
                        {
                            r.Name = String.Format("[{0}] {1}",
                                _targetNode.Files.Count,
                                Path.GetFileNameWithoutExtension(dlg.AudioSource));
                            r.ReplaceRaw(dlg.AudioData);
                        }
                    }
                    r._parent = _targetNode;
                    _targetNode.Files.Add(r);
                    Update(lstSets);
                }
        }
    }

    public class SoundPackItem : ListViewItem
    {
        public RSARFileNode _node;

        public SoundPackItem(RSARFileNode file)
        {
            ImageIndex = (byte)file.ResourceType;

            Text = file.FileNodeIndex.ToString();

            _node = file;

            string s = file.ResourceType.ToString();
            if (s == "Unknown") s = "External";
            SubItems.Add(s);
            int i = Helpers.FindFirst(file.Name, 0, ']');
            SubItems.Add(file.Name.Substring(i + 1));
            //SubItems.Add(file.ExtPath);
            SubItems.Add("0x" + file.DataOffset);
            SubItems.Add("0x" + file.AudioOffset);
            SubItems.Add("0x" + file.InfoHeaderOffset);
        }
    }

    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
	    /// <summary>
	    /// Specifies the column to be sorted
	    /// </summary>
	    private int ColumnToSort;
	    /// <summary>
	    /// Specifies the order in which to sort (i.e. 'Ascending').
	    /// </summary>
	    private SortOrder OrderOfSort;
	    /// <summary>
	    /// Case insensitive comparer object
	    /// </summary>
	    private CaseInsensitiveComparer ObjectCompare;

	    /// <summary>
	    /// Class constructor.  Initializes various elements
	    /// </summary>
	    public ListViewColumnSorter()
	    {
		    // Initialize the column to '0'
		    ColumnToSort = 0;

		    // Initialize the sort order to 'none'
		    OrderOfSort = SortOrder.None;

		    // Initialize the CaseInsensitiveComparer object
		    ObjectCompare = new CaseInsensitiveComparer();
	    }

	    /// <summary>
	    /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
	    /// </summary>
	    /// <param name="x">First object to be compared</param>
	    /// <param name="y">Second object to be compared</param>
	    /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
	    public int Compare(object x, object y)
	    {
		    int compareResult;
		    ListViewItem listviewX, listviewY;

		    // Cast the objects to be compared to ListViewItem objects
		    listviewX = (ListViewItem)x;
		    listviewY = (ListViewItem)y;

		    // Compare the two items
            if (ColumnToSort == 0)
                compareResult = ObjectCompare.Compare(int.Parse(listviewX.SubItems[ColumnToSort].Text), int.Parse(listviewY.SubItems[ColumnToSort].Text));
            else if (ColumnToSort >= 4)
                compareResult = ObjectCompare.Compare(int.Parse(listviewX.SubItems[ColumnToSort].Text.Substring(2), Globalization.NumberStyles.HexNumber), int.Parse(listviewY.SubItems[ColumnToSort].Text.Substring(2), Globalization.NumberStyles.HexNumber));
            else 
		        compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text,listviewY.SubItems[ColumnToSort].Text);
			
		    // Calculate correct return value based on object comparison
		    if (OrderOfSort == SortOrder.Ascending)
			    // Ascending sort is selected, return normal result of compare operation
			    return compareResult;
		    else if (OrderOfSort == SortOrder.Descending)
			    // Descending sort is selected, return negative result of compare operation
			    return (-compareResult);
		    else
			    // Return '0' to indicate they are equal
			    return 0;
		    
	    }
    
	    /// <summary>
	    /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
	    /// </summary>
	    public int SortColumn
	    {
		    set { ColumnToSort = value; }
		    get { return ColumnToSort; }
	    }

	    /// <summary>
	    /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
	    /// </summary>
	    public SortOrder Order
	    {
		    set { OrderOfSort = value; }
		    get { return OrderOfSort; }
	    }
    }
}
