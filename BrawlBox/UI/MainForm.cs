using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Imaging;
using System.Reflection;
using BrawlLib.IO;
using System.Audio;
using BrawlLib.Wii.Audio;
using BrawlLib.OpenGL;
using System.Diagnostics;
using BrawlBox.Properties;
using System.Collections.Specialized;
using BrawlLib.SSBB;
using BrawlLib.Modeling;
using Octokit;

namespace BrawlBox
{
    public partial class MainForm : Form
    {
        private static MainForm _instance;
        public static MainForm Instance { get { return _instance == null ? _instance = new MainForm() : _instance; } }

        private BaseWrapper _root;
        public BaseWrapper RootNode { get { return _root; } }

        private SettingsDialog _settings;
        private SettingsDialog Settings { get { return _settings == null ? _settings = new SettingsDialog() : _settings; } }

        RecentFileHandler RecentFileHandler;

        private InterpolationForm _interpolationForm = null;
        public InterpolationForm InterpolationForm
        {
            get
            {
                if (_interpolationForm == null)
                {
                    _interpolationForm = new InterpolationForm(null);
                    _interpolationForm.FormClosed += _interpolationForm_FormClosed;
                    _interpolationForm.Show();
                }
                return _interpolationForm;
            }
        }

        void _interpolationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _interpolationForm = null;
        }

        public MainForm()
        {
            InitializeComponent();
            Text = Program.AssemblyTitle;
            if (CheckUpdatesOnStartup)
                CheckUpdates();
//#if _DEBUG
//            Text += " - DEBUG";
//#endif
            soundPackControl1._grid = propertyGrid1;
            soundPackControl1.lstSets.SmallImageList = ResourceTree.Images;
            msBinEditor1.Dock =
            animEditControl.Dock =
            texAnimEditControl.Dock =
            shpAnimEditControl.Dock =
            soundPackControl1.Dock =
            audioPlaybackPanel1.Dock =
            clrControl.Dock =
            visEditor.Dock =
            scN0CameraEditControl1.Dock =
            scN0LightEditControl1.Dock =
            scN0FogEditControl1.Dock =
            modelPanel1.Dock =
            previewPanel2.Dock =
            videoPlaybackPanel1.Dock =
			attributeGrid1.Dock =
            texCoordRenderer1.Dock =
            DockStyle.Fill;
            m_DelegateOpenFile = new DelegateOpenFile(Program.Open);
            _instance = this;
            modelPanel1.AllowSelection = false;
            _currentControl = modelPanel1;

            RecentFileHandler = new RecentFileHandler(this.components);
            RecentFileHandler.RecentFileToolStripItem = this.recentFilesToolStripMenuItem;

            _displayPropertyDescription = BrawlBox.Properties.Settings.Default.DisplayPropertyDescriptionWhenAvailable;
        }

        private delegate bool DelegateOpenFile(String s);
        private DelegateOpenFile m_DelegateOpenFile;

        private async void CheckUpdates()
        {
            const string version = "v0.74"; // <---- Replace with latest release tag

            var github = new GitHubClient(new Octokit.ProductHeaderValue("Brawltools"));
            var release = await github.Release.GetAll("libertyernie", "brawltools");

            if (release[0].TagName != version)
            {
                DialogResult UpdateResult = MessageBox.Show("Brawlbox update " + release[0].TagName + " is available! Update now?", "Update", MessageBoxButtons.YesNo);

                if (UpdateResult == DialogResult.Yes)
                {

                    DialogResult OverwriteResult = MessageBox.Show("Overwrite current installation?", "", MessageBoxButtons.YesNoCancel);
                    if (OverwriteResult == DialogResult.Yes)
                    {
                        Process.Start(System.Windows.Forms.Application.StartupPath + "/Updater.exe", "-r");
                        this.Close();
                    }
                    else if (OverwriteResult == DialogResult.No)
                        Process.Start(System.Windows.Forms.Application.StartupPath + "/Updater.exe");

                }
            }
        }
        public void Reset()
        {
            _root = null;
            resourceTree.SelectedNode = null;
            resourceTree.Clear();

            if (Program.RootNode != null)
            {
                _root = BaseWrapper.Wrap(this, Program.RootNode);
                resourceTree.BeginUpdate();
                resourceTree.Nodes.Add(_root);
                resourceTree.SelectedNode = _root;
                _root.Expand();
                resourceTree.EndUpdate();

                closeToolStripMenuItem.Enabled = true;
                saveAsToolStripMenuItem.Enabled = true;
                saveToolStripMenuItem.Enabled = true;

                Program.RootNode._mainForm = this;
            }
            else
            {
                closeToolStripMenuItem.Enabled = false;
                saveAsToolStripMenuItem.Enabled = false;
                saveToolStripMenuItem.Enabled = false;
            }

            UpdateName();
        }

        public void UpdateName()
        {
            if (Program.RootPath != null)
                Text = String.Format("{0} - {1}", Program.AssemblyTitle, Program.RootPath);
            else
                Text = Program.AssemblyTitle;

//#if DEBUG
//            Text += " - DEBUG";
//#endif
        }

        public void TargetResource(ResourceNode n)
        {
            if (_root != null)
                resourceTree.SelectedNode = _root.FindResource(n, true);
        }

        public Control _currentControl = null;
        private Control _secondaryControl = null;
        public void resourceTree_SelectionChanged(object sender, EventArgs e)
        {
            audioPlaybackPanel1.TargetSource = null;
            animEditControl.TargetSequence = null;
            texAnimEditControl.TargetSequence = null;
            shpAnimEditControl.TargetSequence = null;
            msBinEditor1.CurrentNode = null;
            //soundPackControl1.TargetNode = null;
            clrControl.ColorSource = null;
            visEditor.TargetNode = null;
            scN0CameraEditControl1.TargetSequence = null;
            scN0LightEditControl1.TargetSequence = null;
            scN0FogEditControl1.TargetSequence = null;
            modelPanel1.ClearAll();
            
            Control newControl = null;
            Control newControl2 = null;

            BaseWrapper w;
            ResourceNode node = null;
            bool disable2nd = false;
            if ((resourceTree.SelectedNode is BaseWrapper) && ((node = (w = resourceTree.SelectedNode as BaseWrapper).ResourceNode) != null))
            {
                propertyGrid1.SelectedObject = node;

                if (node is IVideo)
                {
                    videoPlaybackPanel1.TargetSource = node as IVideo;
                    newControl = videoPlaybackPanel1;
                }
                else if (node is MDL0TextureNode || node is MDL0MaterialRefNode)
                {
                    newControl = texCoordRenderer1;
                }
                else if (node is MSBinNode)
                {
                    msBinEditor1.CurrentNode = node as MSBinNode;
                    newControl = msBinEditor1;
                }
                else if (node is CHR0EntryNode)
                {
                    animEditControl.TargetSequence = node as CHR0EntryNode;
                    newControl = animEditControl;
                }
                else if (node is SRT0TextureNode)
                {
                    texAnimEditControl.TargetSequence = node as SRT0TextureNode;
                    newControl = texAnimEditControl;
                }
                else if (node is SHP0VertexSetNode)
                {
                    shpAnimEditControl.TargetSequence = node as SHP0VertexSetNode;
                    newControl = shpAnimEditControl;
                }
                else if (node is RSARNode)
                {
                    soundPackControl1.TargetNode = node as RSARNode;
                    newControl = soundPackControl1;
                }
                else if (node is VIS0EntryNode)
                {
                    visEditor.TargetNode = node as VIS0EntryNode;
                    newControl = visEditor;
                }
                else if (node is SCN0CameraNode)
                {
                    scN0CameraEditControl1.TargetSequence = node as SCN0CameraNode;
                    newControl = scN0CameraEditControl1;
                }
                else if (node is SCN0LightNode)
                {
                    scN0LightEditControl1.TargetSequence = node as SCN0LightNode;
                    newControl = scN0LightEditControl1;
                    disable2nd = true;
                }
                else if (node is SCN0FogNode)
                {
                    scN0FogEditControl1.TargetSequence = node as SCN0FogNode;
                    newControl = scN0FogEditControl1;
                    disable2nd = true;
                }
                else if (node is IAudioSource)
                {
                    audioPlaybackPanel1.TargetSource = node as IAudioSource;
                    IAudioStream[] sources = audioPlaybackPanel1.TargetSource.CreateStreams();
                    if (sources != null && sources.Length > 0 && sources[0] != null)
                        newControl = audioPlaybackPanel1;
                }
                else if (node is IImageSource)
                {
                    previewPanel2.RenderingTarget = ((IImageSource)node);
                    newControl = previewPanel2;
                }
                else if (node is IRenderedObject)
				{
                    newControl = modelPanel1;
				}
				else if (node is STDTNode)
				{
					STDTNode stdt = (STDTNode)node;

					attributeGrid1.Clear();
					attributeGrid1.AddRange(stdt.GetPossibleInterpretations());
					attributeGrid1.TargetNode = stdt;
					newControl = attributeGrid1;
				}

                if (node is IColorSource && !disable2nd)
                {
                    clrControl.ColorSource = node as IColorSource;
                    if (((IColorSource)node).ColorCount(0) > 0)
                        if (newControl != null)
                            newControl2 = clrControl;
                        else
                            newControl = clrControl;
                }

                if ((editToolStripMenuItem.DropDown = w.ContextMenuStrip) != null)
                    editToolStripMenuItem.Enabled = true;
                else
                    editToolStripMenuItem.Enabled = false;
            }
            else
            {
                propertyGrid1.SelectedObject = null;
                try
                {
                    editToolStripMenuItem.DropDown = null;
                }
                catch
                {

                }
                editToolStripMenuItem.Enabled = false;
            }

            if (_secondaryControl != newControl2)
            {
                if (_secondaryControl != null)
                {
                    _secondaryControl.Dock = DockStyle.Fill;
                    _secondaryControl.Visible = false;
                }
                _secondaryControl = newControl2;
                if (_secondaryControl != null)
                {
                    _secondaryControl.Dock = DockStyle.Right;
                    _secondaryControl.Visible = true;
                    _secondaryControl.Width = 340;
                }
            }
            if (_currentControl != newControl)
            {
                if (_currentControl != null)
                    _currentControl.Visible = false;
                _currentControl = newControl;
                if (_currentControl != null)
                    _currentControl.Visible = true;
            }
            if (_currentControl != null)
            {
                if (_secondaryControl != null)
                    _currentControl.Width = splitContainer2.Panel2.Width - _secondaryControl.Width;
                _currentControl.Dock = DockStyle.Fill;
            }

            //Model panel has to be loaded first to display model correctly
            if (_currentControl is ModelPanel)
            {
                if (node._children == null)
                    node.Populate(0);
                
                if (node is IModel)
                {
                    IModel m = node as IModel;
                    m.ResetToBindState();
                }

                modelPanel1.AddTarget((IRenderedObject)node);

                Vector3 min, max;
                ((IRenderedObject)node).GetBox(out min, out max);
                modelPanel1.SetCamWithBox(min, max);
            }
            else if (_currentControl is TexCoordRenderer)
            {
                if (node is MDL0TextureNode)
                    texCoordRenderer1.SetTarget((MDL0TextureNode)node);
                else
                    texCoordRenderer1.SetTarget(((MDL0MaterialRefNode)node).TextureNode);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!Program.Close()) 
                e.Cancel = true;

            base.OnClosing(e);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string inFile;
            int i = Program.OpenFile(SuppertedFilesHandler.CompleteFilter, out inFile);
            if (i != 0)
            {
                //if (i >= SupportedFilesHandler.Files.Length)
                //{
                //    FileMap map = FileMap.FromFile(inFile, FileMapProtect.Read);
                //    FileScanNode node = new FileScanNode();
                //    Program.Scan(map, node);
                //    if (node.Children.Count == 0)
                //        MessageBox.Show("No formats recognized.");
                //    else
                //    {
                //        Program._rootNode = node;
                //        Program._rootPath = inFile;
                //        node._list = node._children;
                //        node.Initialize(null, new DataSource(map));
                //        Reset();
                //    }
                //}
                //else 
                    if (Program.Open(inFile))
                        RecentFileHandler.AddFile(inFile);
            }
        }

        #region File Menu
        private void aRCArchiveToolStripMenuItem_Click(object sender, EventArgs e) { Program.New<ARCNode>(); }
        private void u8FileArchiveToolStripMenuItem_Click(object sender, EventArgs e) { Program.New<U8Node>(); }
        private void brresPackToolStripMenuItem_Click(object sender, EventArgs e) { Program.New<BRESNode>(); }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) { Program.Save(); }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) { Program.SaveAs(); }
        private void closeToolStripMenuItem_Click(object sender, EventArgs e) { Program.Close(); }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) { this.Close(); }
        #endregion

        private void fileResizerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //using (FileResizer res = new FileResizer())
            //    res.ShowDialog();
        }
        private void settingsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Settings.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) { AboutForm.Instance.ShowDialog(this); }

        private void bRStmAudioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path;
            if (Program.OpenFile("PCM Audio (*.wav)|*.wav", out path) > 0)
            {
                if (Program.New<RSTMNode>())
                {
                    using (BrstmConverterDialog dlg = new BrstmConverterDialog())
                    {
                        dlg.AudioSource = path;
                        if (dlg.ShowDialog(this) == DialogResult.OK)
                        {
                            Program.RootNode.Name = Path.GetFileNameWithoutExtension(dlg.AudioSource);
                            Program.RootNode.ReplaceRaw(dlg.AudioData);
                        }
                        else
                            Program.Close(true);
                    }
                }
            }
        }

        public bool DisplayPropertyDescription 
        {
            get { return _displayPropertyDescription; }
            set
            {
                _displayPropertyDescription = value;

                BrawlBox.Properties.Settings.Default.DisplayPropertyDescriptionWhenAvailable = _displayPropertyDescription;
                BrawlBox.Properties.Settings.Default.Save();
                UpdatePropertyDescriptionBox(propertyGrid1.SelectedGridItem);
            }
        }
        bool _displayPropertyDescription;

        public bool CheckUpdatesOnStartup
        {
            get { return _updatesOnStartup; }
            set 
            {
                _updatesOnStartup = value;

                BrawlBox.Properties.Settings.Default.CheckUpdatesAtStartup = _updatesOnStartup;
                BrawlBox.Properties.Settings.Default.Save();
            }
        }
        bool _updatesOnStartup;
        
        private void UpdatePropertyDescriptionBox(GridItem item)
        {
            if (!DisplayPropertyDescription)
            {
                if (propertyGrid1.HelpVisible != false)
                    propertyGrid1.HelpVisible = false;
            }
            else
                propertyGrid1.HelpVisible = item != null && item.PropertyDescriptor != null && !String.IsNullOrEmpty(item.PropertyDescriptor.Description);
        }

        private void propertyGrid1_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            if (DisplayPropertyDescription)
                UpdatePropertyDescriptionBox(e.NewSelection);
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            Array a = (Array)e.Data.GetData(DataFormats.FileDrop);
            if (a != null)
            {
                string s = null;
                for (int i = 0; i < a.Length; i++)
                {
                    s = a.GetValue(i).ToString();
                    this.BeginInvoke(m_DelegateOpenFile, new Object[] { s });
                }
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void gCTEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCTEditor c = new GCTEditor();
            c.Show();
        }

        private void recentFilesToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            RecentFileHandler.FileMenuItem fmi = (RecentFileHandler.FileMenuItem)e.ClickedItem;
            Program.Open(fmi.FileName);
        }

        private void checkForUpdatesToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            CheckUpdates();
        }
    }

    public class RecentFileHandler : Component
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
        }
        public class FileMenuItem : ToolStripMenuItem
        {
            string fileName;

            public string FileName
            {
                get { return fileName; }
                set { fileName = value; }
            }

            public FileMenuItem(string fileName)
            {
                this.fileName = fileName;
            }

            public override string Text
            {
                get
                {
                    ToolStripMenuItem parent = (ToolStripMenuItem)this.OwnerItem;
                    int index = parent.DropDownItems.IndexOf(this);
                    return string.Format("{0} {1}", index + 1, fileName);
                }
                set
                {
                }
            }
        }

        public const int MaxRecentFiles = 24;

        public RecentFileHandler()
        {
            InitializeComponent();

            Init();
        }

        public RecentFileHandler(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            Init();
        }

        void Init()
        {
            Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
        }

        public void AddFile(string fileName)
        {
            try
            {
                if (this.recentFileToolStripItem == null)
                    throw new OperationCanceledException("recentFileToolStripItem can not be null!");

                // check if the file is already in the collection
                int alreadyIn = GetIndexOfRecentFile(fileName);
                if (alreadyIn != -1) // remove it
                {
                    Settings.Default.RecentFiles.RemoveAt(alreadyIn);
                    if (recentFileToolStripItem.DropDownItems.Count > alreadyIn)
                        recentFileToolStripItem.DropDownItems.RemoveAt(alreadyIn);
                }
                else if (alreadyIn == 0) // it´s the latest file so return
                    return;

                // insert the file on top of the list
                Settings.Default.RecentFiles.Insert(0, fileName);
                recentFileToolStripItem.DropDownItems.Insert(0, new FileMenuItem(fileName));

                // remove the last one, if max size is reached
                if (Settings.Default.RecentFiles.Count > MaxRecentFiles)
                    Settings.Default.RecentFiles.RemoveAt(MaxRecentFiles);
                if (Settings.Default.RecentFiles.Count > Settings.Default.RecentFilesMax)
                    recentFileToolStripItem.DropDownItems.RemoveAt(Settings.Default.RecentFilesMax);

                // enable the menu item if it´s disabled
                if (!recentFileToolStripItem.Enabled)
                    recentFileToolStripItem.Enabled = true;

                // save the changes
                Settings.Default.Save();
            }
            catch { }
        }

        int GetIndexOfRecentFile(string filename)
        {
            for (int i = 0; i < Settings.Default.RecentFiles.Count; i++)
            {
                string currentFile = Settings.Default.RecentFiles[i];
                if (string.Equals(currentFile, filename, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        ToolStripMenuItem recentFileToolStripItem;

        public ToolStripMenuItem RecentFileToolStripItem
        {
            get { return recentFileToolStripItem; }
            set
            {
                if (recentFileToolStripItem == value)
                    return;

                recentFileToolStripItem = value;

                ReCreateItems();
            }
        }

        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RecentFilesMax")
            {
                ReCreateItems();
            }
        }

        void ReCreateItems()
        {
            if (recentFileToolStripItem == null)
                return;

            if (Settings.Default.RecentFiles == null)
                Settings.Default.RecentFiles = new StringCollection();

            recentFileToolStripItem.DropDownItems.Clear();
            recentFileToolStripItem.Enabled = (Settings.Default.RecentFiles.Count > 0);

            int fileItemCount = Math.Min(Settings.Default.RecentFilesMax, Settings.Default.RecentFiles.Count);
            for (int i = 0; i < fileItemCount; i++)
            {
                string file = Settings.Default.RecentFiles[i];
                recentFileToolStripItem.DropDownItems.Add(new FileMenuItem(file));
            }
        }

        public void Clear()
        {
            Settings.Default.RecentFiles.Clear();
            ReCreateItems();
        }
    }
}
