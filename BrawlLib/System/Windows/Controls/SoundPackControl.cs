using System;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;
using System.Diagnostics;
using System.Collections;
using BrawlLib;
using System.ComponentModel;
using System.Threading;

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
            this.contextMenuStrip1.SuspendLayout();
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
            this.lstSets.Location = new System.Drawing.Point(0, 0);
            this.lstSets.MultiSelect = false;
            this.lstSets.Name = "lstSets";
            this.lstSets.Size = new System.Drawing.Size(506, 253);
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
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuPath,
            this.mnuExport,
            this.mnuReplace,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(116, 92);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // mnuPath
            // 
            this.mnuPath.Name = "mnuPath";
            this.mnuPath.Size = new System.Drawing.Size(115, 22);
            this.mnuPath.Text = "Path...";
            this.mnuPath.Click += new System.EventHandler(this.mnuPath_Click);
            // 
            // mnuExport
            // 
            this.mnuExport.Name = "mnuExport";
            this.mnuExport.Size = new System.Drawing.Size(115, 22);
            this.mnuExport.Text = "Export";
            this.mnuExport.Click += new System.EventHandler(this.mnuExport_Click);
            // 
            // mnuReplace
            // 
            this.mnuReplace.Name = "mnuReplace";
            this.mnuReplace.Size = new System.Drawing.Size(115, 22);
            this.mnuReplace.Text = "Replace";
            this.mnuReplace.Click += new System.EventHandler(this.mnuReplace_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // SoundPackControl
            // 
            this.Controls.Add(this.lstSets);
            this.Name = "SoundPackControl";
            this.Size = new System.Drawing.Size(506, 253);
            this.DoubleClick += new System.EventHandler(this.lstSets_DoubleClick);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

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
            {
                if (_selectedItem._node is RSARExtFileNode)
                    mnuExport.Enabled = false;
                else
                    mnuExport.Enabled = true;
            }
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
                }
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    _selectedItem._node.Export(dlg.FileName);
            }
        }

        private void lstSets_DoubleClick(object sender, EventArgs e)
        {
            if (_selectedItem._node is RSARExtFileNode)
            {
                if (File.Exists(_selectedItem._node.FullExtPath)) Process.Start(_selectedItem._node.FullExtPath);
                else mnuPath_Click(this, null);
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
                dlg.Filter =
                    "All File Types (*.brwsd, *.brbnk, *.brseq, *.brstm)|*.brwsd;*.brbnk;*.brseq;*.brstm|" +
                    "Sound Stream (*.brwsd)|*.brwsd|" +
                    "Sound Bank (*.brbnk)|*.brbnk|" +
                    "Sound Sequence (*.brseq)|*.brseq|" +
                    "Audio Stream (*.brstm)|*.brstm";
                if (dlg.ShowDialog() == DialogResult.OK)
                    _selectedItem._node.Replace(dlg.FileName);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _selectedItem._node.Remove();
            lstSets.Items.Remove(_selectedItem);
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
