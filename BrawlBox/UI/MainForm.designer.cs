using System.Windows.Forms;
namespace BrawlBox
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ModelPanelViewport modelPanelViewport1 = new System.Windows.Forms.ModelPanelViewport();
            BrawlLib.OpenGL.GLCamera glCamera1 = new BrawlLib.OpenGL.GLCamera();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.resourceTree = new BrawlBox.ResourceTree();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.archivesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aRCFileArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bRRESResourcePackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.u8FileArchiveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tPLTextureArchiveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.audioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bRSTMAudioStreamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.effectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eFLSEffectListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rEFFParticlesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rEFTParticleTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.recentFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gCTEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.hexBox1 = new Be.Windows.Forms.HexBox();
            this.mdL0ObjectControl1 = new System.Windows.Forms.MDL0ObjectControl();
            this.ppcDisassembler1 = new System.Windows.Forms.PPCDisassembler();
            this.texCoordControl1 = new System.Windows.Forms.TexCoordControl();
            this.attributeGrid1 = new System.Windows.Forms.MultipleInterpretationAttributeGrid();
            this.videoPlaybackPanel1 = new System.Windows.Forms.VideoPlaybackPanel();
            this.modelPanel1 = new System.Windows.Forms.ModelPanel();
            this.previewPanel2 = new System.Windows.Forms.PreviewPanel();
            this.scN0FogEditControl1 = new System.Windows.Forms.SCN0FogEditControl();
            this.scN0LightEditControl1 = new System.Windows.Forms.SCN0LightEditControl();
            this.scN0CameraEditControl1 = new System.Windows.Forms.SCN0CameraEditControl();
            this.animEditControl = new System.Windows.Forms.AnimEditControl();
            this.shpAnimEditControl = new System.Windows.Forms.ShpAnimEditControl();
            this.texAnimEditControl = new System.Windows.Forms.TexAnimEditControl();
            this.audioPlaybackPanel1 = new System.Windows.Forms.AudioPlaybackPanel();
            this.visEditor = new System.Windows.Forms.VisEditor();
            this.clrControl = new System.Windows.Forms.CLRControl();
            this.rsarGroupEditor = new RSARGroupEditor();
            this.soundPackControl1 = new System.Windows.Forms.SoundPackControl();
            this.msBinEditor1 = new System.Windows.Forms.MSBinEditor();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.resourceTree);
            this.splitContainer1.Panel1.Controls.Add(this.menuStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(823, 506);
            this.splitContainer1.SplitterDistance = 282;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            this.splitContainer1.TabStop = false;
            this.splitContainer1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.splitContainer_MouseDown);
            this.splitContainer1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.splitContainer_MouseMove);
            this.splitContainer1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.splitContainer_MouseUp);
            // 
            // resourceTree
            // 
            this.resourceTree.AllowDrop = true;
            this.resourceTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resourceTree.HideSelection = false;
            this.resourceTree.ImageIndex = 0;
            this.resourceTree.Indent = 20;
            this.resourceTree.Location = new System.Drawing.Point(0, 28);
            this.resourceTree.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.resourceTree.Name = "resourceTree";
            this.resourceTree.SelectedImageIndex = 0;
            this.resourceTree.ShowIcons = true;
            this.resourceTree.Size = new System.Drawing.Size(282, 478);
            this.resourceTree.TabIndex = 0;
            this.resourceTree.SelectionChanged += new System.EventHandler(this.resourceTree_SelectionChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(282, 28);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.toolStripMenuItem1,
            this.recentFilesToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.archivesToolStripMenuItem,
            this.audioToolStripMenuItem,
            this.effectsToolStripMenuItem});
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
            this.newToolStripMenuItem.Text = "&New";
            // 
            // archivesToolStripMenuItem
            // 
            this.archivesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aRCFileArchiveToolStripMenuItem,
            this.bRRESResourcePackToolStripMenuItem,
            this.u8FileArchiveToolStripMenuItem1,
            this.tPLTextureArchiveToolStripMenuItem1});
            this.archivesToolStripMenuItem.Name = "archivesToolStripMenuItem";
            this.archivesToolStripMenuItem.Size = new System.Drawing.Size(180, 26);
            this.archivesToolStripMenuItem.Text = "Archives";
            // 
            // aRCFileArchiveToolStripMenuItem
            // 
            this.aRCFileArchiveToolStripMenuItem.Name = "aRCFileArchiveToolStripMenuItem";
            this.aRCFileArchiveToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.aRCFileArchiveToolStripMenuItem.Text = "ARC File Archive";
            this.aRCFileArchiveToolStripMenuItem.Click += new System.EventHandler(this.aRCArchiveToolStripMenuItem_Click);
            // 
            // bRRESResourcePackToolStripMenuItem
            // 
            this.bRRESResourcePackToolStripMenuItem.Name = "bRRESResourcePackToolStripMenuItem";
            this.bRRESResourcePackToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.bRRESResourcePackToolStripMenuItem.Text = "BRRES Resource Pack";
            this.bRRESResourcePackToolStripMenuItem.Click += new System.EventHandler(this.brresPackToolStripMenuItem_Click);
            // 
            // u8FileArchiveToolStripMenuItem1
            // 
            this.u8FileArchiveToolStripMenuItem1.Name = "u8FileArchiveToolStripMenuItem1";
            this.u8FileArchiveToolStripMenuItem1.Size = new System.Drawing.Size(224, 26);
            this.u8FileArchiveToolStripMenuItem1.Text = "U8 File Archive";
            this.u8FileArchiveToolStripMenuItem1.Click += new System.EventHandler(this.u8FileArchiveToolStripMenuItem_Click);
            // 
            // tPLTextureArchiveToolStripMenuItem1
            // 
            this.tPLTextureArchiveToolStripMenuItem1.Name = "tPLTextureArchiveToolStripMenuItem1";
            this.tPLTextureArchiveToolStripMenuItem1.Size = new System.Drawing.Size(224, 26);
            this.tPLTextureArchiveToolStripMenuItem1.Text = "TPL Texture Archive";
            this.tPLTextureArchiveToolStripMenuItem1.Click += new System.EventHandler(this.tPLTextureArchiveToolStripMenuItem_Click);
            // 
            // audioToolStripMenuItem
            // 
            this.audioToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bRSTMAudioStreamToolStripMenuItem});
            this.audioToolStripMenuItem.Name = "audioToolStripMenuItem";
            this.audioToolStripMenuItem.Size = new System.Drawing.Size(180, 26);
            this.audioToolStripMenuItem.Text = "Audio";
            // 
            // bRSTMAudioStreamToolStripMenuItem
            // 
            this.bRSTMAudioStreamToolStripMenuItem.Name = "bRSTMAudioStreamToolStripMenuItem";
            this.bRSTMAudioStreamToolStripMenuItem.Size = new System.Drawing.Size(226, 26);
            this.bRSTMAudioStreamToolStripMenuItem.Text = "BRSTM Audio Stream";
            this.bRSTMAudioStreamToolStripMenuItem.Click += new System.EventHandler(this.bRStmAudioToolStripMenuItem_Click);
            // 
            // effectsToolStripMenuItem
            // 
            this.effectsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.eFLSEffectListToolStripMenuItem,
            this.rEFFParticlesToolStripMenuItem,
            this.rEFTParticleTexturesToolStripMenuItem});
            this.effectsToolStripMenuItem.Name = "effectsToolStripMenuItem";
            this.effectsToolStripMenuItem.Size = new System.Drawing.Size(180, 26);
            this.effectsToolStripMenuItem.Text = "Particle Effects";
            // 
            // eFLSEffectListToolStripMenuItem
            // 
            this.eFLSEffectListToolStripMenuItem.Name = "eFLSEffectListToolStripMenuItem";
            this.eFLSEffectListToolStripMenuItem.Size = new System.Drawing.Size(226, 26);
            this.eFLSEffectListToolStripMenuItem.Text = "EFLS Effect List";
            this.eFLSEffectListToolStripMenuItem.Click += new System.EventHandler(this.eFLSEffectListToolStripMenuItem_Click);
            // 
            // rEFFParticlesToolStripMenuItem
            // 
            this.rEFFParticlesToolStripMenuItem.Name = "rEFFParticlesToolStripMenuItem";
            this.rEFFParticlesToolStripMenuItem.Size = new System.Drawing.Size(226, 26);
            this.rEFFParticlesToolStripMenuItem.Text = "REFF Particles";
            this.rEFFParticlesToolStripMenuItem.Click += new System.EventHandler(this.rEFFParticlesToolStripMenuItem_Click);
            // 
            // rEFTParticleTexturesToolStripMenuItem
            // 
            this.rEFTParticleTexturesToolStripMenuItem.Name = "rEFTParticleTexturesToolStripMenuItem";
            this.rEFTParticleTexturesToolStripMenuItem.Size = new System.Drawing.Size(226, 26);
            this.rEFTParticleTexturesToolStripMenuItem.Text = "REFT Particle Textures";
            this.rEFTParticleTexturesToolStripMenuItem.Click += new System.EventHandler(this.rEFTParticleTexturesToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
            this.openToolStripMenuItem.Text = "&Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
            this.saveAsToolStripMenuItem.Text = "Save &As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Enabled = false;
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
            this.closeToolStripMenuItem.Text = "&Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(193, 6);
            // 
            // recentFilesToolStripMenuItem
            // 
            this.recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
            this.recentFilesToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
            this.recentFilesToolStripMenuItem.Text = "Recent Files";
            this.recentFilesToolStripMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.recentFilesToolStripMenuItem_DropDownItemClicked);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(193, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Enabled = false;
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(47, 24);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.gCTEditorToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(56, 24);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(182, 26);
            this.settingsToolStripMenuItem.Text = "&Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click_1);
            // 
            // gCTEditorToolStripMenuItem
            // 
            this.gCTEditorToolStripMenuItem.Name = "gCTEditorToolStripMenuItem";
            this.gCTEditorToolStripMenuItem.Size = new System.Drawing.Size(182, 26);
            this.gCTEditorToolStripMenuItem.Text = "Code Manager";
            this.gCTEditorToolStripMenuItem.Click += new System.EventHandler(this.gCTEditorToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.checkForUpdatesToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(203, 26);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(203, 26);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for updates";
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click_1);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.propertyGrid1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.hexBox1);
            this.splitContainer2.Panel2.Controls.Add(this.mdL0ObjectControl1);
            this.splitContainer2.Panel2.Controls.Add(this.ppcDisassembler1);
            this.splitContainer2.Panel2.Controls.Add(this.texCoordControl1);
            this.splitContainer2.Panel2.Controls.Add(this.attributeGrid1);
            this.splitContainer2.Panel2.Controls.Add(this.videoPlaybackPanel1);
            this.splitContainer2.Panel2.Controls.Add(this.modelPanel1);
            this.splitContainer2.Panel2.Controls.Add(this.previewPanel2);
            this.splitContainer2.Panel2.Controls.Add(this.scN0FogEditControl1);
            this.splitContainer2.Panel2.Controls.Add(this.scN0LightEditControl1);
            this.splitContainer2.Panel2.Controls.Add(this.scN0CameraEditControl1);
            this.splitContainer2.Panel2.Controls.Add(this.animEditControl);
            this.splitContainer2.Panel2.Controls.Add(this.shpAnimEditControl);
            this.splitContainer2.Panel2.Controls.Add(this.texAnimEditControl);
            this.splitContainer2.Panel2.Controls.Add(this.audioPlaybackPanel1);
            this.splitContainer2.Panel2.Controls.Add(this.visEditor);
            this.splitContainer2.Panel2.Controls.Add(this.clrControl);
            this.splitContainer2.Panel2.Controls.Add(this.rsarGroupEditor);
            this.splitContainer2.Panel2.Controls.Add(this.soundPackControl1);
            this.splitContainer2.Panel2.Controls.Add(this.msBinEditor1);
            this.splitContainer2.Size = new System.Drawing.Size(536, 506);
            this.splitContainer2.SplitterDistance = 252;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 3;
            this.splitContainer2.TabStop = false;
            this.splitContainer2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.splitContainer_MouseDown);
            this.splitContainer2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.splitContainer_MouseMove);
            this.splitContainer2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.splitContainer_MouseUp);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.HelpVisible = false;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGrid1.Size = new System.Drawing.Size(536, 252);
            this.propertyGrid1.TabIndex = 2;
            this.propertyGrid1.SelectedGridItemChanged += new System.Windows.Forms.SelectedGridItemChangedEventHandler(this.propertyGrid1_SelectedGridItemChanged);
            // 
            // hexBox1
            // 
            this.hexBox1.BlrColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(100)))));
            this.hexBox1.BranchOffsetColor = System.Drawing.Color.Plum;
            this.hexBox1.ColumnDividerColor = System.Drawing.Color.Empty;
            this.hexBox1.CommandColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(255)))), ((int)(((byte)(200)))));
            this.hexBox1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hexBox1.InfoForeColor = System.Drawing.Color.Empty;
            this.hexBox1.LinkedBranchColor = System.Drawing.Color.Orange;
            this.hexBox1.Location = new System.Drawing.Point(113, 33);
            this.hexBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.hexBox1.Name = "hexBox1";
            this.hexBox1.ReadOnly = true;
            this.hexBox1.SectionEditor = null;
            this.hexBox1.SelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.hexBox1.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hexBox1.Size = new System.Drawing.Size(287, 160);
            this.hexBox1.StringViewVisible = true;
            this.hexBox1.TabIndex = 22;
            this.hexBox1.UseFixedBytesPerLine = true;
            this.hexBox1.Visible = false;
            this.hexBox1.VScrollBarVisible = true;
            // 
            // mdL0ObjectControl1
            // 
            this.mdL0ObjectControl1.Location = new System.Drawing.Point(185, 69);
            this.mdL0ObjectControl1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.mdL0ObjectControl1.Name = "mdL0ObjectControl1";
            this.mdL0ObjectControl1.Size = new System.Drawing.Size(869, 489);
            this.mdL0ObjectControl1.TabIndex = 21;
            // 
            // ppcDisassembler1
            // 
            this.ppcDisassembler1.Location = new System.Drawing.Point(0, 0);
            this.ppcDisassembler1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.ppcDisassembler1.Name = "ppcDisassembler1";
            this.ppcDisassembler1.Size = new System.Drawing.Size(531, 245);
            this.ppcDisassembler1.TabIndex = 20;
            this.ppcDisassembler1.Visible = false;
            // 
            // texCoordControl1
            // 
            this.texCoordControl1.Location = new System.Drawing.Point(0, 0);
            this.texCoordControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.texCoordControl1.Name = "texCoordControl1";
            this.texCoordControl1.Size = new System.Drawing.Size(528, 245);
            this.texCoordControl1.TabIndex = 19;
            this.texCoordControl1.TargetNode = null;
            this.texCoordControl1.Visible = false;
            // 
            // attributeGrid1
            // 
            this.attributeGrid1.AttributeArray = null;
            this.attributeGrid1.Location = new System.Drawing.Point(61, 69);
            this.attributeGrid1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.attributeGrid1.Name = "attributeGrid1";
            this.attributeGrid1.Size = new System.Drawing.Size(639, 375);
            this.attributeGrid1.TabIndex = 18;
            this.attributeGrid1.Visible = false;
            // 
            // videoPlaybackPanel1
            // 
            this.videoPlaybackPanel1.Location = new System.Drawing.Point(113, -20);
            this.videoPlaybackPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.videoPlaybackPanel1.Name = "videoPlaybackPanel1";
            this.videoPlaybackPanel1.Size = new System.Drawing.Size(715, 137);
            this.videoPlaybackPanel1.TabIndex = 17;
            this.videoPlaybackPanel1.Visible = false;
            // 
            // modelPanel1
            // 
            modelPanelViewport1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            modelPanelViewport1.BackgroundImage = null;
            modelPanelViewport1.BackgroundImageType = BrawlLib.OpenGL.BGImageType.Stretch;
            glCamera1.Aspect = 2.442308F;
            glCamera1.FarDepth = 200000F;
            glCamera1.Height = 208F;
            glCamera1.NearDepth = 1F;
            glCamera1.Orthographic = false;
            glCamera1.VerticalFieldOfView = 45F;
            glCamera1.Width = 508F;
            modelPanelViewport1.Camera = glCamera1;
            modelPanelViewport1.Enabled = true;
            modelPanelViewport1.Region = new System.Drawing.Rectangle(0, 0, 508, 208);
            modelPanelViewport1.RotationScale = 0.4F;
            modelPanelViewport1.TranslationScale = 0.05F;
            modelPanelViewport1.ViewType = BrawlLib.OpenGL.ViewportProjection.Perspective;
            modelPanelViewport1.ZoomScale = 2.5F;
            this.modelPanel1.CurrentViewport = modelPanelViewport1;
            this.modelPanel1.Location = new System.Drawing.Point(0, 0);
            this.modelPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.modelPanel1.Name = "modelPanel1";
            this.modelPanel1.Size = new System.Drawing.Size(508, 208);
            this.modelPanel1.TabIndex = 15;
            this.modelPanel1.TabStop = false;
            this.modelPanel1.Visible = false;
            // 
            // previewPanel2
            // 
            this.previewPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.previewPanel2.CurrentIndex = 0;
            this.previewPanel2.DisposeImage = true;
            this.previewPanel2.Location = new System.Drawing.Point(0, 0);
            this.previewPanel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.previewPanel2.Name = "previewPanel2";
            this.previewPanel2.RenderingTarget = null;
            this.previewPanel2.Size = new System.Drawing.Size(494, 165);
            this.previewPanel2.TabIndex = 16;
            this.previewPanel2.Visible = false;
            // 
            // scN0FogEditControl1
            // 
            this.scN0FogEditControl1.Location = new System.Drawing.Point(-148, -146);
            this.scN0FogEditControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.scN0FogEditControl1.Name = "scN0FogEditControl1";
            this.scN0FogEditControl1.Size = new System.Drawing.Size(391, 340);
            this.scN0FogEditControl1.TabIndex = 13;
            this.scN0FogEditControl1.Visible = false;
            // 
            // scN0LightEditControl1
            // 
            this.scN0LightEditControl1.Location = new System.Drawing.Point(185, -234);
            this.scN0LightEditControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.scN0LightEditControl1.Name = "scN0LightEditControl1";
            this.scN0LightEditControl1.Size = new System.Drawing.Size(391, 340);
            this.scN0LightEditControl1.TabIndex = 12;
            this.scN0LightEditControl1.Visible = false;
            // 
            // scN0CameraEditControl1
            // 
            this.scN0CameraEditControl1.Location = new System.Drawing.Point(139, -235);
            this.scN0CameraEditControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.scN0CameraEditControl1.Name = "scN0CameraEditControl1";
            this.scN0CameraEditControl1.Size = new System.Drawing.Size(381, 340);
            this.scN0CameraEditControl1.TabIndex = 11;
            this.scN0CameraEditControl1.Visible = false;
            // 
            // animEditControl
            // 
            this.animEditControl.Location = new System.Drawing.Point(0, 0);
            this.animEditControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.animEditControl.Name = "animEditControl";
            this.animEditControl.Size = new System.Drawing.Size(512, 208);
            this.animEditControl.TabIndex = 1;
            this.animEditControl.Visible = false;
            // 
            // shpAnimEditControl
            // 
            this.shpAnimEditControl.Location = new System.Drawing.Point(0, 0);
            this.shpAnimEditControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.shpAnimEditControl.Name = "shpAnimEditControl";
            this.shpAnimEditControl.Size = new System.Drawing.Size(512, 208);
            this.shpAnimEditControl.TabIndex = 7;
            this.shpAnimEditControl.Visible = false;
            // 
            // texAnimEditControl
            // 
            this.texAnimEditControl.Location = new System.Drawing.Point(0, 0);
            this.texAnimEditControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.texAnimEditControl.Name = "texAnimEditControl";
            this.texAnimEditControl.Size = new System.Drawing.Size(400, 261);
            this.texAnimEditControl.TabIndex = 7;
            this.texAnimEditControl.Visible = false;
            // 
            // audioPlaybackPanel1
            // 
            this.audioPlaybackPanel1.AutoScaleMode = AutoScaleMode.None;
            this.audioPlaybackPanel1.Location = new System.Drawing.Point(199, 113);
            this.audioPlaybackPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.audioPlaybackPanel1.Name = "audioPlaybackPanel1";
            this.audioPlaybackPanel1.Size = new System.Drawing.Size(93, 137);
            this.audioPlaybackPanel1.TabIndex = 4;
            this.audioPlaybackPanel1.TargetStreams = null;
            this.audioPlaybackPanel1.Visible = false;
            this.audioPlaybackPanel1.Volume = null;
            // 
            // visEditor
            // 
            this.visEditor.Location = new System.Drawing.Point(0, 0);
            this.visEditor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.visEditor.Name = "visEditor";
            this.visEditor.Size = new System.Drawing.Size(104, 107);
            this.visEditor.TabIndex = 6;
            this.visEditor.Visible = false;
            // 
            // clrControl
            // 
            this.clrControl.ColorID = 0;
            this.clrControl.Location = new System.Drawing.Point(0, 0);
            this.clrControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.clrControl.Name = "clrControl";
            this.clrControl.Size = new System.Drawing.Size(131, 58);
            this.clrControl.TabIndex = 5;
            this.clrControl.Visible = false;
            // 
            // rsarGroupEditor
            // 
            this.rsarGroupEditor.Location = new System.Drawing.Point(0, 0);
            this.rsarGroupEditor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rsarGroupEditor.Name = "rsarGroupEditor";
            this.rsarGroupEditor.Size = new System.Drawing.Size(131, 58);
            this.rsarGroupEditor.TabIndex = 5;
            this.rsarGroupEditor.Visible = false;
            // 
            // soundPackControl1
            // 
            this.soundPackControl1.Location = new System.Drawing.Point(17, 124);
            this.soundPackControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.soundPackControl1.Name = "soundPackControl1";
            this.soundPackControl1.Size = new System.Drawing.Size(173, 80);
            this.soundPackControl1.TabIndex = 3;
            this.soundPackControl1.TargetNode = null;
            this.soundPackControl1.Visible = false;
            // 
            // msBinEditor1
            // 
            this.msBinEditor1.Location = new System.Drawing.Point(139, 5);
            this.msBinEditor1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.msBinEditor1.Name = "msBinEditor1";
            this.msBinEditor1.Size = new System.Drawing.Size(195, 101);
            this.msBinEditor1.TabIndex = 2;
            this.msBinEditor1.Visible = false;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 506);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "MainForm";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public ResourceTree resourceTree;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        public System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        public System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private AnimEditControl animEditControl;
        private MSBinEditor msBinEditor1;
        private MultipleInterpretationAttributeGrid attributeGrid1;
        private SoundPackControl soundPackControl1;
        private AudioPlaybackPanel audioPlaybackPanel1;
        private CLRControl clrControl;
        private RSARGroupEditor rsarGroupEditor;
        private VisEditor visEditor;
        //private ScriptEditor movesetEditor1;
        //private EventDescription eventDescription1;
        private TexAnimEditControl texAnimEditControl;
        private ShpAnimEditControl shpAnimEditControl;
        //private AttributeGrid attributeControl;
        //private OffsetEditor offsetEditor1;
        //private ArticleAttributeGrid articleAttributeGrid;
        private SCN0LightEditControl scN0LightEditControl1;
        private SCN0CameraEditControl scN0CameraEditControl1;
        private SCN0FogEditControl scN0FogEditControl1;
        public ModelPanel modelPanel1;
        private PreviewPanel previewPanel2;
        public ToolStripMenuItem editToolStripMenuItem;
        private VideoPlaybackPanel videoPlaybackPanel1;
        private ToolStripMenuItem gCTEditorToolStripMenuItem;
        private ToolStripMenuItem recentFilesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        public ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private ToolStripMenuItem archivesToolStripMenuItem;
        private ToolStripMenuItem aRCFileArchiveToolStripMenuItem;
        private ToolStripMenuItem bRRESResourcePackToolStripMenuItem;
        private ToolStripMenuItem u8FileArchiveToolStripMenuItem1;
        private ToolStripMenuItem tPLTextureArchiveToolStripMenuItem1;
        private ToolStripMenuItem audioToolStripMenuItem;
        private ToolStripMenuItem bRSTMAudioStreamToolStripMenuItem;
        private ToolStripMenuItem effectsToolStripMenuItem;
        private ToolStripMenuItem eFLSEffectListToolStripMenuItem;
        private ToolStripMenuItem rEFFParticlesToolStripMenuItem;
        private ToolStripMenuItem rEFTParticleTexturesToolStripMenuItem;
        private TexCoordControl texCoordControl1;
        private PPCDisassembler ppcDisassembler1;
        private MDL0ObjectControl mdL0ObjectControl1;
        private Be.Windows.Forms.HexBox hexBox1;
    }
}

