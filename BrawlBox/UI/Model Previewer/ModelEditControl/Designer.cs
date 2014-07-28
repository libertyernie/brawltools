using System;
using BrawlLib.OpenGL;
using System.ComponentModel;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;
using BrawlLib.Modeling;
using System.Drawing;
using BrawlLib.Wii.Animations;
using System.Collections.Generic;
using BrawlLib.SSBBTypes;
using BrawlLib.IO;
using BrawlLib;
using System.Drawing.Imaging;
using Gif.Components;
using OpenTK.Graphics.OpenGL;
using BrawlLib.Imaging;
using System.Threading;

namespace System.Windows.Forms
{
    public partial class ModelEditControl : UserControl, IMainWindow
    {
        #region Designer
        private ModelPanel modelPanel;
        private ColorDialog dlgColor;
        private Button btnLeftToggle;
        private Button btnRightToggle;
        private Button btnBottomToggle;
        private Splitter spltLeft;
        private Button btnTopToggle;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem openModelsToolStripMenuItem;
        private ToolStripMenuItem kinectToolStripMenuItem;
        private ToolStripMenuItem notYetImplementedToolStripMenuItem;
        private ToolStripMenuItem newSceneToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem1;
        private ToolStripMenuItem btnUndo;
        private ToolStripMenuItem btnRedo;
        private ToolStripMenuItem backColorToolStripMenuItem;
        private ToolStripMenuItem startTrackingToolStripMenuItem;
        private Label label1;
        private ToolStripMenuItem syncKinectToolStripMenuItem;
        private ToolStripMenuItem targetModelToolStripMenuItem;
        private ToolStripMenuItem hideFromSceneToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem hideAllOtherModelsToolStripMenuItem;
        private ToolStripMenuItem deleteAllOtherModelsToolStripMenuItem;
        private ToolStripMenuItem modelToolStripMenuItem;
        private ToolStripMenuItem toggleBones;
        private ToolStripMenuItem togglePolygons;
        private ToolStripMenuItem toggleVertices;
        private ToolStripMenuItem toggleCollisions;
        private ToolStripMenuItem modifyLightingToolStripMenuItem;
        private ToolStripMenuItem toggleFloor;
        private ToolStripMenuItem resetCameraToolStripMenuItem;
        private ToolStripMenuItem editorsToolStripMenuItem;
        private ToolStripMenuItem showLeft;
        private ToolStripMenuItem showBottom;
        private ToolStripMenuItem showTop;
        public CHR0Editor chr0Editor;
        public ComboBox models;
        private Panel controlPanel;
        private Splitter spltRight;
        private Panel panel1;
        public SRT0Editor srt0Editor;
        private ToolStripMenuItem fileTypesToolStripMenuItem;
        private ToolStripMenuItem playCHR0ToolStripMenuItem;
        private ToolStripMenuItem playSRT0ToolStripMenuItem;
        private ToolStripMenuItem playSHP0ToolStripMenuItem;
        private ToolStripMenuItem playPAT0ToolStripMenuItem;
        private ToolStripMenuItem playVIS0ToolStripMenuItem;
        private ToolStripMenuItem openAnimationsToolStripMenuItem;
        private ToolStripMenuItem openMovesetToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem btnOpenClose;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        public VIS0Editor vis0Editor;
        public PAT0Editor pat0Editor;
        public SHP0Editor shp0Editor;
        public Panel animEditors;
        private ToolStrip toolStrip1;
        private Panel panel2;
        private ToolStripButton chkBones;
        private ToolStripButton chkPolygons;
        private ToolStripButton chkVertices;
        private ToolStripButton chkFloor;
        private ToolStripButton button1;
        private ToolStripSeparator toolStripSeparator1;
        public ModelPlaybackPanel pnlPlayback;
        public ToolStripMenuItem chkExternalAnims;
        private Splitter splitter1;
        public Panel animCtrlPnl;
        private ToolStripButton chkCollisions;
        public ToolStripButton btnSaveCam;
        private SCN0Editor scn0Editor;
        private Splitter splitter2;
        private ToolStripMenuItem showRight;
        public ToolStripMenuItem showCameraCoordinatesToolStripMenuItem;
        private ToolStripMenuItem sCN0ToolStripMenuItem;
        private ToolStripMenuItem displayAmbienceToolStripMenuItem;
        private ToolStripMenuItem displayLightsToolStripMenuItem;
        private ToolStripMenuItem displayFogToolStripMenuItem;
        private ToolStripMenuItem displayCameraToolStripMenuItem;
        private ToolStripMenuItem displayToolStripMenuItem;
        private ToolStripMenuItem stPersonToolStripMenuItem;
        private ToolStripMenuItem editControlToolStripMenuItem;
        private ToolStripMenuItem rotationToolStripMenuItem;
        private ToolStripMenuItem translationToolStripMenuItem;
        private ToolStripMenuItem scaleToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private CLR0Editor clr0Editor;
        private ToolStripMenuItem playCLR0ToolStripMenuItem;
        private WeightEditor weightEditor;
        private ToolStripMenuItem backgroundToolStripMenuItem;
        private ToolStripMenuItem setColorToolStripMenuItem;
        private ToolStripMenuItem loadImageToolStripMenuItem;
        private ToolStripMenuItem takeScreenshotToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        public ToolStripMenuItem displayFrameCountDifferencesToolStripMenuItem;
        public ToolStripMenuItem alwaysSyncFrameCountsToolStripMenuItem;
        public ToolStripMenuItem syncAnimationsTogetherToolStripMenuItem;
        public ToolStripMenuItem syncTexObjToolStripMenuItem;
        public ToolStripMenuItem syncObjectsListToVIS0ToolStripMenuItem;
        public ToolStripMenuItem disableBonesWhenPlayingToolStripMenuItem;
        private ToolStripMenuItem btnExportToImgNoTransparency;
        private ToolStripMenuItem btnExportToImgWithTransparency;
        private ToolStripMenuItem btnExportToAnimatedGIF;
        private ToolStripMenuItem saveLocationToolStripMenuItem;
        public ToolStripMenuItem ScreenCapBgLocText;
        private ToolStripMenuItem displaySettingToolStripMenuItem;
        private ToolStripMenuItem stretchToolStripMenuItem1;
        private ToolStripMenuItem centerToolStripMenuItem1;
        private ToolStripMenuItem resizeToolStripMenuItem1;
        private ToolStripMenuItem imageFormatToolStripMenuItem;
        private ToolStripMenuItem projectionToolStripMenuItem;
        private ToolStripMenuItem perspectiveToolStripMenuItem;
        public ToolStripMenuItem orthographicToolStripMenuItem;
        private VertexEditor vertexEditor;
        private ToolStripMenuItem boundingBoxToolStripMenuItem;
        private ToolStripMenuItem chkDontRenderOffscreen;
        private ToolStripMenuItem toggleNormals;
        private ToolStripMenuItem dontHighlightBonesAndVerticesToolStripMenuItem;
        public ToolStripMenuItem enablePointAndLineSmoothingToolStripMenuItem;
        public ToolStripMenuItem enableTextOverlaysToolStripMenuItem;
        private RightPanel rightPanel;
        private ToolStripMenuItem wireframeToolStripMenuItem;
        private ToolStripMenuItem interpolationEditorToolStripMenuItem;
        private ToolStripMenuItem linearInterpolationToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem selectedAnimationToolStripMenuItem;
        private ToolStripMenuItem portToolStripMenuItem;
        private ToolStripMenuItem mergeToolStripMenuItem;
        private ToolStripMenuItem appendToolStripMenuItem;
        private ToolStripMenuItem resizeToolStripMenuItem;
        private ToolStripMenuItem playToolStripMenuItem;
        private ToolStripMenuItem chkLinearCHR;
        private ToolStripMenuItem chkLinearSRT;
        private ToolStripMenuItem chkLinearSHP;
        private ToolStripMenuItem interpolationToolStripMenuItem;
        private ToolStripMenuItem averageAllStartEndTangentsToolStripMenuItem;
        private ToolStripMenuItem averageboneStartendTangentsToolStripMenuItem;
        private ToolStripMenuItem SparentLocalToolStripMenuItem;
        private ToolStripMenuItem SworldToolStripMenuItem;
        private ToolStripMenuItem RparentLocalToolStripMenuItem;
        private ToolStripMenuItem RworldToolStripMenuItem;
        private ToolStripMenuItem TparentLocalToolStripMenuItem;
        private ToolStripMenuItem TworldToolStripMenuItem;
        private ToolStripMenuItem syncStartendTangentsToolStripMenuItem;
        public ToolStripMenuItem chkNonBRRESAnims;
        private ToolStripMenuItem allSettingsToolStripMenuItem;
        private ToolStripMenuItem resetToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem importToolStripMenuItem;
        private ToolStripMenuItem chkSnapToColl;
        private ToolStripMenuItem chkMaximize;
        private ToolStripMenuItem generateTangentsToolStripMenuItem;
        private ToolStripMenuItem chkGenTansCHR;
        private ToolStripMenuItem chkGenTansSRT;
        private ToolStripMenuItem chkGenTansSHP;
        private ToolStripMenuItem chkGenTansLight;
        private ToolStripMenuItem chkGenTansFog;
        private ToolStripMenuItem chkGenTansCamera;
        private ToolStripMenuItem chkLinearLight;
        private ToolStripMenuItem chkLinearFog;
        private ToolStripMenuItem chkLinearCamera;
        public ToolStripMenuItem chkBRRESAnims;
        private ToolStripMenuItem detachViewerToolStripMenuItem;
        private ToolStripButton chkZoomExtents;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripComboBox cboToolSelect;
        private ToolStripDropDownButton dropdownOverlays;
        private ToolStripMenuItem chkBoundaries;
        private ToolStripMenuItem chkSpawns;
        private ToolStripMenuItem chkItems;
        public LeftPanel leftPanel;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelEditControl));
            this.dlgColor = new System.Windows.Forms.ColorDialog();
            this.btnLeftToggle = new System.Windows.Forms.Button();
            this.btnRightToggle = new System.Windows.Forms.Button();
            this.btnBottomToggle = new System.Windows.Forms.Button();
            this.spltLeft = new System.Windows.Forms.Splitter();
            this.btnTopToggle = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newSceneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openAnimationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnOpenClose = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMovesetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.btnRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.takeScreenshotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExportToImgNoTransparency = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExportToImgWithTransparency = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExportToAnimatedGIF = new System.Windows.Forms.ToolStripMenuItem();
            this.saveLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ScreenCapBgLocText = new System.Windows.Forms.ToolStripMenuItem();
            this.imageFormatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modifyLightingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateTangentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkGenTansCHR = new System.Windows.Forms.ToolStripMenuItem();
            this.chkGenTansSRT = new System.Windows.Forms.ToolStripMenuItem();
            this.chkGenTansSHP = new System.Windows.Forms.ToolStripMenuItem();
            this.chkGenTansLight = new System.Windows.Forms.ToolStripMenuItem();
            this.chkGenTansFog = new System.Windows.Forms.ToolStripMenuItem();
            this.chkGenTansCamera = new System.Windows.Forms.ToolStripMenuItem();
            this.displayFrameCountDifferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysSyncFrameCountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syncAnimationsTogetherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syncTexObjToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syncObjectsListToVIS0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableBonesWhenPlayingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkDontRenderOffscreen = new System.Windows.Forms.ToolStripMenuItem();
            this.dontHighlightBonesAndVerticesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enablePointAndLineSmoothingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableTextOverlaysToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syncStartendTangentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkSnapToColl = new System.Windows.Forms.ToolStripMenuItem();
            this.chkMaximize = new System.Windows.Forms.ToolStripMenuItem();
            this.allSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTop = new System.Windows.Forms.ToolStripMenuItem();
            this.showLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.showBottom = new System.Windows.Forms.ToolStripMenuItem();
            this.showRight = new System.Windows.Forms.ToolStripMenuItem();
            this.backColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displaySettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stretchToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.centerToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.resizeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scaleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SparentLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SworldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RparentLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RworldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.translationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TparentLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TworldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.perspectiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.orthographicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleFloor = new System.Windows.Forms.ToolStripMenuItem();
            this.resetCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCameraCoordinatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.detachViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleBones = new System.Windows.Forms.ToolStripMenuItem();
            this.togglePolygons = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleVertices = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleCollisions = new System.Windows.Forms.ToolStripMenuItem();
            this.wireframeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleNormals = new System.Windows.Forms.ToolStripMenuItem();
            this.boundingBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playCHR0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playSRT0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playSHP0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playPAT0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playVIS0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playCLR0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.linearInterpolationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkLinearCHR = new System.Windows.Forms.ToolStripMenuItem();
            this.chkLinearSRT = new System.Windows.Forms.ToolStripMenuItem();
            this.chkLinearSHP = new System.Windows.Forms.ToolStripMenuItem();
            this.chkLinearLight = new System.Windows.Forms.ToolStripMenuItem();
            this.chkLinearFog = new System.Windows.Forms.ToolStripMenuItem();
            this.chkLinearCamera = new System.Windows.Forms.ToolStripMenuItem();
            this.sCN0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayAmbienceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayLightsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayFogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stPersonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.interpolationEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.portToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.appendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.interpolationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.averageAllStartEndTangentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.averageboneStartendTangentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.targetModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideFromSceneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideAllOtherModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllOtherModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkExternalAnims = new System.Windows.Forms.ToolStripMenuItem();
            this.chkBRRESAnims = new System.Windows.Forms.ToolStripMenuItem();
            this.chkNonBRRESAnims = new System.Windows.Forms.ToolStripMenuItem();
            this.kinectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syncKinectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notYetImplementedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startTrackingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.models = new System.Windows.Forms.ComboBox();
            this.controlPanel = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.chkBones = new System.Windows.Forms.ToolStripButton();
            this.chkPolygons = new System.Windows.Forms.ToolStripButton();
            this.chkVertices = new System.Windows.Forms.ToolStripButton();
            this.chkCollisions = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.chkFloor = new System.Windows.Forms.ToolStripButton();
            this.button1 = new System.Windows.Forms.ToolStripButton();
            this.chkZoomExtents = new System.Windows.Forms.ToolStripButton();
            this.btnSaveCam = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cboToolSelect = new System.Windows.Forms.ToolStripComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.spltRight = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.modelPanel = new System.Windows.Forms.ModelPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.animEditors = new System.Windows.Forms.Panel();
            this.pnlPlayback = new System.Windows.Forms.ModelPlaybackPanel();
            this.animCtrlPnl = new System.Windows.Forms.Panel();
            this.vis0Editor = new System.Windows.Forms.VIS0Editor();
            this.pat0Editor = new System.Windows.Forms.PAT0Editor();
            this.shp0Editor = new System.Windows.Forms.SHP0Editor();
            this.srt0Editor = new System.Windows.Forms.SRT0Editor();
            this.chr0Editor = new System.Windows.Forms.CHR0Editor();
            this.scn0Editor = new System.Windows.Forms.SCN0Editor();
            this.clr0Editor = new System.Windows.Forms.CLR0Editor();
            this.weightEditor = new System.Windows.Forms.WeightEditor();
            this.vertexEditor = new System.Windows.Forms.VertexEditor();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.rightPanel = new System.Windows.Forms.RightPanel();
            this.leftPanel = new System.Windows.Forms.LeftPanel();
            this.dropdownOverlays = new System.Windows.Forms.ToolStripDropDownButton();
            this.chkBoundaries = new System.Windows.Forms.ToolStripMenuItem();
            this.chkSpawns = new System.Windows.Forms.ToolStripMenuItem();
            this.chkItems = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.controlPanel.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.animEditors.SuspendLayout();
            this.animCtrlPnl.SuspendLayout();
            this.SuspendLayout();
            // 
            // dlgColor
            // 
            this.dlgColor.AnyColor = true;
            this.dlgColor.FullOpen = true;
            // 
            // btnLeftToggle
            // 
            this.btnLeftToggle.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnLeftToggle.Location = new System.Drawing.Point(142, 24);
            this.btnLeftToggle.Name = "btnLeftToggle";
            this.btnLeftToggle.Size = new System.Drawing.Size(15, 391);
            this.btnLeftToggle.TabIndex = 5;
            this.btnLeftToggle.TabStop = false;
            this.btnLeftToggle.Text = ">";
            this.btnLeftToggle.UseVisualStyleBackColor = false;
            this.btnLeftToggle.Click += new System.EventHandler(this.btnLeftToggle_Click);
            // 
            // btnRightToggle
            // 
            this.btnRightToggle.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnRightToggle.Location = new System.Drawing.Point(657, 24);
            this.btnRightToggle.Name = "btnRightToggle";
            this.btnRightToggle.Size = new System.Drawing.Size(15, 391);
            this.btnRightToggle.TabIndex = 6;
            this.btnRightToggle.TabStop = false;
            this.btnRightToggle.Text = "<";
            this.btnRightToggle.UseVisualStyleBackColor = false;
            this.btnRightToggle.Click += new System.EventHandler(this.btnRightToggle_Click);
            // 
            // btnBottomToggle
            // 
            this.btnBottomToggle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnBottomToggle.Location = new System.Drawing.Point(157, 400);
            this.btnBottomToggle.Name = "btnBottomToggle";
            this.btnBottomToggle.Size = new System.Drawing.Size(500, 15);
            this.btnBottomToggle.TabIndex = 8;
            this.btnBottomToggle.TabStop = false;
            this.btnBottomToggle.UseVisualStyleBackColor = false;
            this.btnBottomToggle.Click += new System.EventHandler(this.btnBottomToggle_Click);
            // 
            // spltLeft
            // 
            this.spltLeft.BackColor = System.Drawing.SystemColors.Control;
            this.spltLeft.Location = new System.Drawing.Point(138, 24);
            this.spltLeft.Name = "spltLeft";
            this.spltLeft.Size = new System.Drawing.Size(4, 391);
            this.spltLeft.TabIndex = 9;
            this.spltLeft.TabStop = false;
            this.spltLeft.Visible = false;
            // 
            // btnTopToggle
            // 
            this.btnTopToggle.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnTopToggle.Location = new System.Drawing.Point(157, 24);
            this.btnTopToggle.Name = "btnTopToggle";
            this.btnTopToggle.Size = new System.Drawing.Size(500, 15);
            this.btnTopToggle.TabIndex = 11;
            this.btnTopToggle.TabStop = false;
            this.btnTopToggle.UseVisualStyleBackColor = false;
            this.btnTopToggle.Click += new System.EventHandler(this.btnTopToggle_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem1,
            this.toolsToolStripMenuItem,
            this.targetModelToolStripMenuItem,
            this.kinectToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(303, 24);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newSceneToolStripMenuItem,
            this.openModelsToolStripMenuItem,
            this.openAnimationsToolStripMenuItem,
            this.openMovesetToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newSceneToolStripMenuItem
            // 
            this.newSceneToolStripMenuItem.Name = "newSceneToolStripMenuItem";
            this.newSceneToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newSceneToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.newSceneToolStripMenuItem.Text = "New Scene";
            this.newSceneToolStripMenuItem.Click += new System.EventHandler(this.newSceneToolStripMenuItem_Click);
            // 
            // openModelsToolStripMenuItem
            // 
            this.openModelsToolStripMenuItem.Name = "openModelsToolStripMenuItem";
            this.openModelsToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.openModelsToolStripMenuItem.Text = "Load Models";
            this.openModelsToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // openAnimationsToolStripMenuItem
            // 
            this.openAnimationsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpenClose,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem});
            this.openAnimationsToolStripMenuItem.Name = "openAnimationsToolStripMenuItem";
            this.openAnimationsToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.openAnimationsToolStripMenuItem.Text = "Animations";
            // 
            // btnOpenClose
            // 
            this.btnOpenClose.Name = "btnOpenClose";
            this.btnOpenClose.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.btnOpenClose.Size = new System.Drawing.Size(186, 22);
            this.btnOpenClose.Text = "Load";
            this.btnOpenClose.Click += new System.EventHandler(this.btnOpenClose_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.saveToolStripMenuItem.Text = "Save ";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // openMovesetToolStripMenuItem
            // 
            this.openMovesetToolStripMenuItem.Name = "openMovesetToolStripMenuItem";
            this.openMovesetToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.openMovesetToolStripMenuItem.Text = "Load Moveset";
            this.openMovesetToolStripMenuItem.Visible = false;
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.closeToolStripMenuItem.Text = "Close Window";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnUndo,
            this.btnRedo,
            this.takeScreenshotToolStripMenuItem,
            this.modifyLightingToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.allSettingsToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.editToolStripMenuItem.Text = "Options";
            // 
            // btnUndo
            // 
            this.btnUndo.Enabled = false;
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.btnUndo.Size = new System.Drawing.Size(187, 22);
            this.btnUndo.Text = "Undo";
            this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // btnRedo
            // 
            this.btnRedo.Enabled = false;
            this.btnRedo.Name = "btnRedo";
            this.btnRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.btnRedo.Size = new System.Drawing.Size(187, 22);
            this.btnRedo.Text = "Redo";
            this.btnRedo.Click += new System.EventHandler(this.btnRedo_Click);
            // 
            // takeScreenshotToolStripMenuItem
            // 
            this.takeScreenshotToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnExportToImgNoTransparency,
            this.btnExportToImgWithTransparency,
            this.btnExportToAnimatedGIF,
            this.saveLocationToolStripMenuItem,
            this.imageFormatToolStripMenuItem});
            this.takeScreenshotToolStripMenuItem.Name = "takeScreenshotToolStripMenuItem";
            this.takeScreenshotToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.takeScreenshotToolStripMenuItem.Text = "Take Screenshot";
            // 
            // btnExportToImgNoTransparency
            // 
            this.btnExportToImgNoTransparency.Name = "btnExportToImgNoTransparency";
            this.btnExportToImgNoTransparency.ShortcutKeyDisplayString = "Ctrl+Shift+I";
            this.btnExportToImgNoTransparency.Size = new System.Drawing.Size(292, 22);
            this.btnExportToImgNoTransparency.Text = "With Background";
            this.btnExportToImgNoTransparency.Click += new System.EventHandler(this.btnExportToImgNoTransparency_Click);
            // 
            // btnExportToImgWithTransparency
            // 
            this.btnExportToImgWithTransparency.Name = "btnExportToImgWithTransparency";
            this.btnExportToImgWithTransparency.ShortcutKeyDisplayString = "Ctrl+Alt+I";
            this.btnExportToImgWithTransparency.Size = new System.Drawing.Size(292, 22);
            this.btnExportToImgWithTransparency.Text = "With Transparent Background";
            this.btnExportToImgWithTransparency.Click += new System.EventHandler(this.btnExportToImgWithTransparency_Click);
            // 
            // btnExportToAnimatedGIF
            // 
            this.btnExportToAnimatedGIF.Name = "btnExportToAnimatedGIF";
            this.btnExportToAnimatedGIF.Size = new System.Drawing.Size(292, 22);
            this.btnExportToAnimatedGIF.Text = "To Animated GIF";
            this.btnExportToAnimatedGIF.Click += new System.EventHandler(this.btnExportToAnimatedGIF_Click);
            // 
            // saveLocationToolStripMenuItem
            // 
            this.saveLocationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ScreenCapBgLocText});
            this.saveLocationToolStripMenuItem.Name = "saveLocationToolStripMenuItem";
            this.saveLocationToolStripMenuItem.Size = new System.Drawing.Size(292, 22);
            this.saveLocationToolStripMenuItem.Text = "Save Location";
            // 
            // ScreenCapBgLocText
            // 
            this.ScreenCapBgLocText.Name = "ScreenCapBgLocText";
            this.ScreenCapBgLocText.Size = new System.Drawing.Size(110, 22);
            this.ScreenCapBgLocText.Text = "<null>";
            this.ScreenCapBgLocText.Click += new System.EventHandler(this.ScreenCapBgLocText_Click);
            // 
            // imageFormatToolStripMenuItem
            // 
            this.imageFormatToolStripMenuItem.Name = "imageFormatToolStripMenuItem";
            this.imageFormatToolStripMenuItem.Size = new System.Drawing.Size(292, 22);
            this.imageFormatToolStripMenuItem.Text = "Image Format: PNG";
            this.imageFormatToolStripMenuItem.Click += new System.EventHandler(this.imageFormatToolStripMenuItem_Click);
            // 
            // modifyLightingToolStripMenuItem
            // 
            this.modifyLightingToolStripMenuItem.Name = "modifyLightingToolStripMenuItem";
            this.modifyLightingToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.modifyLightingToolStripMenuItem.Text = "Environment Settings";
            this.modifyLightingToolStripMenuItem.Click += new System.EventHandler(this.modifyLightingToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generateTangentsToolStripMenuItem,
            this.displayFrameCountDifferencesToolStripMenuItem,
            this.alwaysSyncFrameCountsToolStripMenuItem,
            this.syncAnimationsTogetherToolStripMenuItem,
            this.syncTexObjToolStripMenuItem,
            this.syncObjectsListToVIS0ToolStripMenuItem,
            this.disableBonesWhenPlayingToolStripMenuItem,
            this.chkDontRenderOffscreen,
            this.dontHighlightBonesAndVerticesToolStripMenuItem,
            this.enablePointAndLineSmoothingToolStripMenuItem,
            this.enableTextOverlaysToolStripMenuItem,
            this.syncStartendTangentsToolStripMenuItem,
            this.chkSnapToColl,
            this.chkMaximize});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // generateTangentsToolStripMenuItem
            // 
            this.generateTangentsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chkGenTansCHR,
            this.chkGenTansSRT,
            this.chkGenTansSHP,
            this.chkGenTansLight,
            this.chkGenTansFog,
            this.chkGenTansCamera});
            this.generateTangentsToolStripMenuItem.Name = "generateTangentsToolStripMenuItem";
            this.generateTangentsToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this.generateTangentsToolStripMenuItem.Text = "Generate tangents";
            // 
            // chkGenTansCHR
            // 
            this.chkGenTansCHR.Checked = true;
            this.chkGenTansCHR.CheckOnClick = true;
            this.chkGenTansCHR.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenTansCHR.Name = "chkGenTansCHR";
            this.chkGenTansCHR.Size = new System.Drawing.Size(147, 22);
            this.chkGenTansCHR.Text = "CHR0";
            this.chkGenTansCHR.CheckedChanged += new System.EventHandler(this.cHR0ToolStripMenuItem1_CheckedChanged);
            // 
            // chkGenTansSRT
            // 
            this.chkGenTansSRT.Checked = true;
            this.chkGenTansSRT.CheckOnClick = true;
            this.chkGenTansSRT.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenTansSRT.Name = "chkGenTansSRT";
            this.chkGenTansSRT.Size = new System.Drawing.Size(147, 22);
            this.chkGenTansSRT.Text = "SRT0";
            this.chkGenTansSRT.CheckedChanged += new System.EventHandler(this.sRT0ToolStripMenuItem1_CheckedChanged);
            // 
            // chkGenTansSHP
            // 
            this.chkGenTansSHP.Checked = true;
            this.chkGenTansSHP.CheckOnClick = true;
            this.chkGenTansSHP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenTansSHP.Name = "chkGenTansSHP";
            this.chkGenTansSHP.Size = new System.Drawing.Size(147, 22);
            this.chkGenTansSHP.Text = "SHP0";
            this.chkGenTansSHP.CheckedChanged += new System.EventHandler(this.sHP0ToolStripMenuItem1_CheckedChanged);
            // 
            // chkGenTansLight
            // 
            this.chkGenTansLight.Checked = true;
            this.chkGenTansLight.CheckOnClick = true;
            this.chkGenTansLight.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenTansLight.Name = "chkGenTansLight";
            this.chkGenTansLight.Size = new System.Drawing.Size(147, 22);
            this.chkGenTansLight.Text = "SCN0 Light";
            this.chkGenTansLight.CheckedChanged += new System.EventHandler(this.sCN0LightToolStripMenuItem_CheckedChanged);
            // 
            // chkGenTansFog
            // 
            this.chkGenTansFog.Checked = true;
            this.chkGenTansFog.CheckOnClick = true;
            this.chkGenTansFog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenTansFog.Name = "chkGenTansFog";
            this.chkGenTansFog.Size = new System.Drawing.Size(147, 22);
            this.chkGenTansFog.Text = "SCN0 Fog";
            this.chkGenTansFog.CheckedChanged += new System.EventHandler(this.sCN0FogToolStripMenuItem_CheckedChanged);
            // 
            // chkGenTansCamera
            // 
            this.chkGenTansCamera.Checked = true;
            this.chkGenTansCamera.CheckOnClick = true;
            this.chkGenTansCamera.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenTansCamera.Name = "chkGenTansCamera";
            this.chkGenTansCamera.Size = new System.Drawing.Size(147, 22);
            this.chkGenTansCamera.Text = "SCN0 Camera";
            this.chkGenTansCamera.CheckedChanged += new System.EventHandler(this.sCN0CameraToolStripMenuItem_CheckedChanged);
            // 
            // displayFrameCountDifferencesToolStripMenuItem
            // 
            this.displayFrameCountDifferencesToolStripMenuItem.CheckOnClick = true;
            this.displayFrameCountDifferencesToolStripMenuItem.Enabled = false;
            this.displayFrameCountDifferencesToolStripMenuItem.Name = "displayFrameCountDifferencesToolStripMenuItem";
            this.displayFrameCountDifferencesToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this.displayFrameCountDifferencesToolStripMenuItem.Text = "Warn if frame counts differ";
            this.displayFrameCountDifferencesToolStripMenuItem.Visible = false;
            // 
            // alwaysSyncFrameCountsToolStripMenuItem
            // 
            this.alwaysSyncFrameCountsToolStripMenuItem.CheckOnClick = true;
            this.alwaysSyncFrameCountsToolStripMenuItem.Enabled = false;
            this.alwaysSyncFrameCountsToolStripMenuItem.Name = "alwaysSyncFrameCountsToolStripMenuItem";
            this.alwaysSyncFrameCountsToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this.alwaysSyncFrameCountsToolStripMenuItem.Text = "Always sync frame counts";
            this.alwaysSyncFrameCountsToolStripMenuItem.Visible = false;
            // 
            // syncAnimationsTogetherToolStripMenuItem
            // 
            this.syncAnimationsTogetherToolStripMenuItem.Checked = true;
            this.syncAnimationsTogetherToolStripMenuItem.CheckOnClick = true;
            this.syncAnimationsTogetherToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.syncAnimationsTogetherToolStripMenuItem.Name = "syncAnimationsTogetherToolStripMenuItem";
            this.syncAnimationsTogetherToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this.syncAnimationsTogetherToolStripMenuItem.Text = "Retrieve corresponding animations";
            this.syncAnimationsTogetherToolStripMenuItem.CheckedChanged += new System.EventHandler(this.syncAnimationsTogetherToolStripMenuItem_CheckedChanged);
            // 
            // syncTexObjToolStripMenuItem
            // 
            this.syncTexObjToolStripMenuItem.CheckOnClick = true;
            this.syncTexObjToolStripMenuItem.Name = "syncTexObjToolStripMenuItem";
            this.syncTexObjToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this.syncTexObjToolStripMenuItem.Text = "Sync texture list with object list";
            this.syncTexObjToolStripMenuItem.CheckedChanged += new System.EventHandler(this.syncTexObjToolStripMenuItem_CheckedChanged);
            // 
            // syncObjectsListToVIS0ToolStripMenuItem
            // 
            this.syncObjectsListToVIS0ToolStripMenuItem.CheckOnClick = true;
            this.syncObjectsListToVIS0ToolStripMenuItem.Name = "syncObjectsListToVIS0ToolStripMenuItem";
            this.syncObjectsListToVIS0ToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this.syncObjectsListToVIS0ToolStripMenuItem.Text = "Sync objects list edits to VIS0";
            this.syncObjectsListToVIS0ToolStripMenuItem.CheckedChanged += new System.EventHandler(this.syncObjectsListToVIS0ToolStripMenuItem_CheckedChanged);
            // 
            // disableBonesWhenPlayingToolStripMenuItem
            // 
            this.disableBonesWhenPlayingToolStripMenuItem.Checked = true;
            this.disableBonesWhenPlayingToolStripMenuItem.CheckOnClick = true;
            this.disableBonesWhenPlayingToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.disableBonesWhenPlayingToolStripMenuItem.Name = "disableBonesWhenPlayingToolStripMenuItem";
            this.disableBonesWhenPlayingToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this.disableBonesWhenPlayingToolStripMenuItem.Text = "Disable bones when playing";
            // 
            // chkDontRenderOffscreen
            // 
            this.chkDontRenderOffscreen.Enabled = false;
            this.chkDontRenderOffscreen.Name = "chkDontRenderOffscreen";
            this.chkDontRenderOffscreen.Size = new System.Drawing.Size(276, 22);
            this.chkDontRenderOffscreen.Text = "Don\'t render offscreen objects";
            this.chkDontRenderOffscreen.Visible = false;
            // 
            // dontHighlightBonesAndVerticesToolStripMenuItem
            // 
            this.dontHighlightBonesAndVerticesToolStripMenuItem.CheckOnClick = true;
            this.dontHighlightBonesAndVerticesToolStripMenuItem.Name = "dontHighlightBonesAndVerticesToolStripMenuItem";
            this.dontHighlightBonesAndVerticesToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this.dontHighlightBonesAndVerticesToolStripMenuItem.Text = "Don\'t highlight bones and vertices";
            // 
            // enablePointAndLineSmoothingToolStripMenuItem
            // 
            this.enablePointAndLineSmoothingToolStripMenuItem.CheckOnClick = true;
            this.enablePointAndLineSmoothingToolStripMenuItem.Name = "enablePointAndLineSmoothingToolStripMenuItem";
            this.enablePointAndLineSmoothingToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this.enablePointAndLineSmoothingToolStripMenuItem.Text = "Enable point and line smoothing";
            this.enablePointAndLineSmoothingToolStripMenuItem.CheckedChanged += new System.EventHandler(this.enablePointAndLineSmoothingToolStripMenuItem_CheckedChanged);
            // 
            // enableTextOverlaysToolStripMenuItem
            // 
            this.enableTextOverlaysToolStripMenuItem.CheckOnClick = true;
            this.enableTextOverlaysToolStripMenuItem.Name = "enableTextOverlaysToolStripMenuItem";
            this.enableTextOverlaysToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this.enableTextOverlaysToolStripMenuItem.Text = "Enable text overlays";
            this.enableTextOverlaysToolStripMenuItem.CheckedChanged += new System.EventHandler(this.enableTextOverlaysToolStripMenuItem_CheckedChanged);
            // 
            // syncStartendTangentsToolStripMenuItem
            // 
            this.syncStartendTangentsToolStripMenuItem.CheckOnClick = true;
            this.syncStartendTangentsToolStripMenuItem.Enabled = false;
            this.syncStartendTangentsToolStripMenuItem.Name = "syncStartendTangentsToolStripMenuItem";
            this.syncStartendTangentsToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
            this.syncStartendTangentsToolStripMenuItem.Text = "Sync first && last frame tangents";
            this.syncStartendTangentsToolStripMenuItem.Visible = false;
            // 
            // chkSnapToColl
            // 
            this.chkSnapToColl.CheckOnClick = true;
            this.chkSnapToColl.Name = "chkSnapToColl";
            this.chkSnapToColl.Size = new System.Drawing.Size(276, 22);
            this.chkSnapToColl.Text = "Snap dragged bones to floor collisions";
            // 
            // chkMaximize
            // 
            this.chkMaximize.CheckOnClick = true;
            this.chkMaximize.Name = "chkMaximize";
            this.chkMaximize.Size = new System.Drawing.Size(276, 22);
            this.chkMaximize.Text = "Maximize upon opening";
            // 
            // allSettingsToolStripMenuItem
            // 
            this.allSettingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.importToolStripMenuItem});
            this.allSettingsToolStripMenuItem.Name = "allSettingsToolStripMenuItem";
            this.allSettingsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.allSettingsToolStripMenuItem.Text = "All Settings";
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.resetToolStripMenuItem.Text = "Reset";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.clearSavedSettingsToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem1
            // 
            this.viewToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editorsToolStripMenuItem,
            this.backColorToolStripMenuItem,
            this.modelToolStripMenuItem,
            this.fileTypesToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.viewToolStripMenuItem1.Name = "viewToolStripMenuItem1";
            this.viewToolStripMenuItem1.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem1.Text = "View";
            // 
            // editorsToolStripMenuItem
            // 
            this.editorsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showTop,
            this.showLeft,
            this.showBottom,
            this.showRight});
            this.editorsToolStripMenuItem.Name = "editorsToolStripMenuItem";
            this.editorsToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.editorsToolStripMenuItem.Text = "Panels";
            // 
            // showTop
            // 
            this.showTop.CheckOnClick = true;
            this.showTop.Name = "showTop";
            this.showTop.Size = new System.Drawing.Size(162, 22);
            this.showTop.Text = "Menu Bar";
            this.showTop.CheckedChanged += new System.EventHandler(this.showTop_CheckedChanged);
            // 
            // showLeft
            // 
            this.showLeft.CheckOnClick = true;
            this.showLeft.Name = "showLeft";
            this.showLeft.Size = new System.Drawing.Size(162, 22);
            this.showLeft.Text = "Left Panel";
            this.showLeft.CheckedChanged += new System.EventHandler(this.showLeft_CheckedChanged);
            // 
            // showBottom
            // 
            this.showBottom.CheckOnClick = true;
            this.showBottom.Name = "showBottom";
            this.showBottom.Size = new System.Drawing.Size(162, 22);
            this.showBottom.Text = "Animation Panel";
            this.showBottom.CheckedChanged += new System.EventHandler(this.showBottom_CheckedChanged);
            // 
            // showRight
            // 
            this.showRight.CheckOnClick = true;
            this.showRight.Name = "showRight";
            this.showRight.Size = new System.Drawing.Size(162, 22);
            this.showRight.Text = "Right Panel";
            this.showRight.CheckedChanged += new System.EventHandler(this.showRight_CheckedChanged);
            // 
            // backColorToolStripMenuItem
            // 
            this.backColorToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backgroundToolStripMenuItem,
            this.editControlToolStripMenuItem,
            this.projectionToolStripMenuItem,
            this.toggleFloor,
            this.resetCameraToolStripMenuItem,
            this.showCameraCoordinatesToolStripMenuItem,
            this.detachViewerToolStripMenuItem});
            this.backColorToolStripMenuItem.Name = "backColorToolStripMenuItem";
            this.backColorToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.backColorToolStripMenuItem.Text = "Viewer";
            // 
            // backgroundToolStripMenuItem
            // 
            this.backgroundToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setColorToolStripMenuItem,
            this.loadImageToolStripMenuItem,
            this.displaySettingToolStripMenuItem});
            this.backgroundToolStripMenuItem.Name = "backgroundToolStripMenuItem";
            this.backgroundToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.backgroundToolStripMenuItem.Text = "Background";
            // 
            // setColorToolStripMenuItem
            // 
            this.setColorToolStripMenuItem.Name = "setColorToolStripMenuItem";
            this.setColorToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.setColorToolStripMenuItem.Text = "Set Color";
            this.setColorToolStripMenuItem.Click += new System.EventHandler(this.setColorToolStripMenuItem_Click);
            // 
            // loadImageToolStripMenuItem
            // 
            this.loadImageToolStripMenuItem.Name = "loadImageToolStripMenuItem";
            this.loadImageToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.loadImageToolStripMenuItem.Text = "Load Image";
            this.loadImageToolStripMenuItem.Click += new System.EventHandler(this.loadImageToolStripMenuItem_Click);
            // 
            // displaySettingToolStripMenuItem
            // 
            this.displaySettingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stretchToolStripMenuItem1,
            this.centerToolStripMenuItem1,
            this.resizeToolStripMenuItem1});
            this.displaySettingToolStripMenuItem.Name = "displaySettingToolStripMenuItem";
            this.displaySettingToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.displaySettingToolStripMenuItem.Text = "Display Setting";
            // 
            // stretchToolStripMenuItem1
            // 
            this.stretchToolStripMenuItem1.Checked = true;
            this.stretchToolStripMenuItem1.CheckOnClick = true;
            this.stretchToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.stretchToolStripMenuItem1.Name = "stretchToolStripMenuItem1";
            this.stretchToolStripMenuItem1.Size = new System.Drawing.Size(111, 22);
            this.stretchToolStripMenuItem1.Text = "Stretch";
            this.stretchToolStripMenuItem1.CheckedChanged += new System.EventHandler(this.stretchToolStripMenuItem_CheckedChanged);
            // 
            // centerToolStripMenuItem1
            // 
            this.centerToolStripMenuItem1.CheckOnClick = true;
            this.centerToolStripMenuItem1.Name = "centerToolStripMenuItem1";
            this.centerToolStripMenuItem1.Size = new System.Drawing.Size(111, 22);
            this.centerToolStripMenuItem1.Text = "Center";
            this.centerToolStripMenuItem1.CheckedChanged += new System.EventHandler(this.centerToolStripMenuItem_CheckedChanged);
            // 
            // resizeToolStripMenuItem1
            // 
            this.resizeToolStripMenuItem1.CheckOnClick = true;
            this.resizeToolStripMenuItem1.Name = "resizeToolStripMenuItem1";
            this.resizeToolStripMenuItem1.Size = new System.Drawing.Size(111, 22);
            this.resizeToolStripMenuItem1.Text = "Resize";
            this.resizeToolStripMenuItem1.CheckedChanged += new System.EventHandler(this.resizeToolStripMenuItem_CheckedChanged);
            // 
            // editControlToolStripMenuItem
            // 
            this.editControlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scaleToolStripMenuItem,
            this.rotationToolStripMenuItem,
            this.translationToolStripMenuItem});
            this.editControlToolStripMenuItem.Name = "editControlToolStripMenuItem";
            this.editControlToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.editControlToolStripMenuItem.Text = "Bone Control";
            // 
            // scaleToolStripMenuItem
            // 
            this.scaleToolStripMenuItem.CheckOnClick = true;
            this.scaleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SparentLocalToolStripMenuItem,
            this.SworldToolStripMenuItem});
            this.scaleToolStripMenuItem.Name = "scaleToolStripMenuItem";
            this.scaleToolStripMenuItem.ShortcutKeyDisplayString = "E Key";
            this.scaleToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.scaleToolStripMenuItem.Text = "Scale";
            this.scaleToolStripMenuItem.CheckedChanged += new System.EventHandler(this.scaleToolStripMenuItem_CheckedChanged);
            // 
            // SparentLocalToolStripMenuItem
            // 
            this.SparentLocalToolStripMenuItem.CheckOnClick = true;
            this.SparentLocalToolStripMenuItem.Enabled = false;
            this.SparentLocalToolStripMenuItem.Name = "SparentLocalToolStripMenuItem";
            this.SparentLocalToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.SparentLocalToolStripMenuItem.Text = "Parent Local";
            this.SparentLocalToolStripMenuItem.Visible = false;
            this.SparentLocalToolStripMenuItem.CheckedChanged += new System.EventHandler(this.SparentLocalToolStripMenuItem_CheckedChanged);
            // 
            // SworldToolStripMenuItem
            // 
            this.SworldToolStripMenuItem.Checked = true;
            this.SworldToolStripMenuItem.CheckOnClick = true;
            this.SworldToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SworldToolStripMenuItem.Enabled = false;
            this.SworldToolStripMenuItem.Name = "SworldToolStripMenuItem";
            this.SworldToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.SworldToolStripMenuItem.Text = "World";
            this.SworldToolStripMenuItem.Visible = false;
            this.SworldToolStripMenuItem.CheckedChanged += new System.EventHandler(this.SworldToolStripMenuItem_CheckedChanged);
            // 
            // rotationToolStripMenuItem
            // 
            this.rotationToolStripMenuItem.Checked = true;
            this.rotationToolStripMenuItem.CheckOnClick = true;
            this.rotationToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rotationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RparentLocalToolStripMenuItem,
            this.RworldToolStripMenuItem});
            this.rotationToolStripMenuItem.Name = "rotationToolStripMenuItem";
            this.rotationToolStripMenuItem.ShortcutKeyDisplayString = "R Key";
            this.rotationToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.rotationToolStripMenuItem.Text = "Rotation";
            this.rotationToolStripMenuItem.CheckedChanged += new System.EventHandler(this.rotationToolStripMenuItem_CheckedChanged);
            // 
            // RparentLocalToolStripMenuItem
            // 
            this.RparentLocalToolStripMenuItem.Checked = true;
            this.RparentLocalToolStripMenuItem.CheckOnClick = true;
            this.RparentLocalToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RparentLocalToolStripMenuItem.Enabled = false;
            this.RparentLocalToolStripMenuItem.Name = "RparentLocalToolStripMenuItem";
            this.RparentLocalToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.RparentLocalToolStripMenuItem.Text = "Parent Local";
            this.RparentLocalToolStripMenuItem.Visible = false;
            this.RparentLocalToolStripMenuItem.CheckedChanged += new System.EventHandler(this.RparentLocalToolStripMenuItem_CheckedChanged);
            // 
            // RworldToolStripMenuItem
            // 
            this.RworldToolStripMenuItem.CheckOnClick = true;
            this.RworldToolStripMenuItem.Enabled = false;
            this.RworldToolStripMenuItem.Name = "RworldToolStripMenuItem";
            this.RworldToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.RworldToolStripMenuItem.Text = "World";
            this.RworldToolStripMenuItem.Visible = false;
            this.RworldToolStripMenuItem.CheckedChanged += new System.EventHandler(this.RworldToolStripMenuItem_CheckedChanged);
            // 
            // translationToolStripMenuItem
            // 
            this.translationToolStripMenuItem.CheckOnClick = true;
            this.translationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TparentLocalToolStripMenuItem,
            this.TworldToolStripMenuItem});
            this.translationToolStripMenuItem.Name = "translationToolStripMenuItem";
            this.translationToolStripMenuItem.ShortcutKeyDisplayString = "T Key";
            this.translationToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.translationToolStripMenuItem.Text = "Translation";
            this.translationToolStripMenuItem.CheckedChanged += new System.EventHandler(this.translationToolStripMenuItem_CheckedChanged);
            // 
            // TparentLocalToolStripMenuItem
            // 
            this.TparentLocalToolStripMenuItem.CheckOnClick = true;
            this.TparentLocalToolStripMenuItem.Enabled = false;
            this.TparentLocalToolStripMenuItem.Name = "TparentLocalToolStripMenuItem";
            this.TparentLocalToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.TparentLocalToolStripMenuItem.Text = "Parent Local";
            this.TparentLocalToolStripMenuItem.Visible = false;
            this.TparentLocalToolStripMenuItem.CheckedChanged += new System.EventHandler(this.TparentLocalToolStripMenuItem_CheckedChanged);
            // 
            // TworldToolStripMenuItem
            // 
            this.TworldToolStripMenuItem.Checked = true;
            this.TworldToolStripMenuItem.CheckOnClick = true;
            this.TworldToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TworldToolStripMenuItem.Enabled = false;
            this.TworldToolStripMenuItem.Name = "TworldToolStripMenuItem";
            this.TworldToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.TworldToolStripMenuItem.Text = "World";
            this.TworldToolStripMenuItem.Visible = false;
            this.TworldToolStripMenuItem.CheckedChanged += new System.EventHandler(this.TworldToolStripMenuItem_CheckedChanged);
            // 
            // projectionToolStripMenuItem
            // 
            this.projectionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.perspectiveToolStripMenuItem,
            this.orthographicToolStripMenuItem});
            this.projectionToolStripMenuItem.Enabled = false;
            this.projectionToolStripMenuItem.Name = "projectionToolStripMenuItem";
            this.projectionToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.projectionToolStripMenuItem.Text = "Projection";
            this.projectionToolStripMenuItem.Visible = false;
            // 
            // perspectiveToolStripMenuItem
            // 
            this.perspectiveToolStripMenuItem.Checked = true;
            this.perspectiveToolStripMenuItem.CheckOnClick = true;
            this.perspectiveToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.perspectiveToolStripMenuItem.Name = "perspectiveToolStripMenuItem";
            this.perspectiveToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.perspectiveToolStripMenuItem.Text = "Perspective";
            this.perspectiveToolStripMenuItem.CheckedChanged += new System.EventHandler(this.perspectiveToolStripMenuItem_CheckedChanged);
            // 
            // orthographicToolStripMenuItem
            // 
            this.orthographicToolStripMenuItem.CheckOnClick = true;
            this.orthographicToolStripMenuItem.Name = "orthographicToolStripMenuItem";
            this.orthographicToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.orthographicToolStripMenuItem.Text = "Orthographic";
            this.orthographicToolStripMenuItem.CheckedChanged += new System.EventHandler(this.orthographicToolStripMenuItem_CheckedChanged);
            // 
            // toggleFloor
            // 
            this.toggleFloor.Name = "toggleFloor";
            this.toggleFloor.ShortcutKeyDisplayString = "F Key";
            this.toggleFloor.Size = new System.Drawing.Size(214, 22);
            this.toggleFloor.Text = "Floor";
            this.toggleFloor.Click += new System.EventHandler(this.toggleFloor_Click);
            // 
            // resetCameraToolStripMenuItem
            // 
            this.resetCameraToolStripMenuItem.Name = "resetCameraToolStripMenuItem";
            this.resetCameraToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+R";
            this.resetCameraToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.resetCameraToolStripMenuItem.Text = "Reset Camera";
            this.resetCameraToolStripMenuItem.Click += new System.EventHandler(this.resetCameraToolStripMenuItem_Click_1);
            // 
            // showCameraCoordinatesToolStripMenuItem
            // 
            this.showCameraCoordinatesToolStripMenuItem.CheckOnClick = true;
            this.showCameraCoordinatesToolStripMenuItem.Name = "showCameraCoordinatesToolStripMenuItem";
            this.showCameraCoordinatesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.showCameraCoordinatesToolStripMenuItem.Text = "Show Camera Coordinates";
            this.showCameraCoordinatesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showCameraCoordinatesToolStripMenuItem_CheckedChanged);
            // 
            // detachViewerToolStripMenuItem
            // 
            this.detachViewerToolStripMenuItem.Name = "detachViewerToolStripMenuItem";
            this.detachViewerToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.detachViewerToolStripMenuItem.Text = "Detach";
            this.detachViewerToolStripMenuItem.Click += new System.EventHandler(this.detachViewerToolStripMenuItem_Click);
            // 
            // modelToolStripMenuItem
            // 
            this.modelToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toggleBones,
            this.togglePolygons,
            this.toggleVertices,
            this.toggleCollisions,
            this.wireframeToolStripMenuItem,
            this.toggleNormals,
            this.boundingBoxToolStripMenuItem});
            this.modelToolStripMenuItem.Name = "modelToolStripMenuItem";
            this.modelToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.modelToolStripMenuItem.Text = "Model";
            // 
            // toggleBones
            // 
            this.toggleBones.Checked = true;
            this.toggleBones.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toggleBones.Name = "toggleBones";
            this.toggleBones.ShortcutKeyDisplayString = "B Key";
            this.toggleBones.Size = new System.Drawing.Size(159, 22);
            this.toggleBones.Text = "Bones";
            this.toggleBones.Click += new System.EventHandler(this.toggleBones_Click);
            // 
            // togglePolygons
            // 
            this.togglePolygons.Checked = true;
            this.togglePolygons.CheckState = System.Windows.Forms.CheckState.Checked;
            this.togglePolygons.Name = "togglePolygons";
            this.togglePolygons.ShortcutKeyDisplayString = "P Key";
            this.togglePolygons.Size = new System.Drawing.Size(159, 22);
            this.togglePolygons.Text = "Polygons";
            this.togglePolygons.Click += new System.EventHandler(this.togglePolygons_Click);
            // 
            // toggleVertices
            // 
            this.toggleVertices.Checked = true;
            this.toggleVertices.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toggleVertices.Name = "toggleVertices";
            this.toggleVertices.ShortcutKeyDisplayString = "V Key";
            this.toggleVertices.Size = new System.Drawing.Size(159, 22);
            this.toggleVertices.Text = "Vertices";
            this.toggleVertices.Click += new System.EventHandler(this.toggleVertices_Click);
            // 
            // toggleCollisions
            // 
            this.toggleCollisions.Name = "toggleCollisions";
            this.toggleCollisions.Size = new System.Drawing.Size(159, 22);
            this.toggleCollisions.Text = "Collisions";
            this.toggleCollisions.Click += new System.EventHandler(this.toggleCollisions_Click);
            // 
            // wireframeToolStripMenuItem
            // 
            this.wireframeToolStripMenuItem.Name = "wireframeToolStripMenuItem";
            this.wireframeToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.wireframeToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.wireframeToolStripMenuItem.Text = "Wireframe";
            this.wireframeToolStripMenuItem.Click += new System.EventHandler(this.wireframeToolStripMenuItem_Click);
            // 
            // toggleNormals
            // 
            this.toggleNormals.Name = "toggleNormals";
            this.toggleNormals.Size = new System.Drawing.Size(159, 22);
            this.toggleNormals.Text = "Normals";
            this.toggleNormals.Click += new System.EventHandler(this.toggleNormals_Click);
            // 
            // boundingBoxToolStripMenuItem
            // 
            this.boundingBoxToolStripMenuItem.Name = "boundingBoxToolStripMenuItem";
            this.boundingBoxToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.boundingBoxToolStripMenuItem.Text = "Bounding Box";
            this.boundingBoxToolStripMenuItem.Click += new System.EventHandler(this.boundingBoxToolStripMenuItem_Click);
            // 
            // fileTypesToolStripMenuItem
            // 
            this.fileTypesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playToolStripMenuItem,
            this.linearInterpolationToolStripMenuItem,
            this.sCN0ToolStripMenuItem});
            this.fileTypesToolStripMenuItem.Name = "fileTypesToolStripMenuItem";
            this.fileTypesToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.fileTypesToolStripMenuItem.Text = "Animations";
            // 
            // playToolStripMenuItem
            // 
            this.playToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playCHR0ToolStripMenuItem,
            this.playSRT0ToolStripMenuItem,
            this.playSHP0ToolStripMenuItem,
            this.playPAT0ToolStripMenuItem,
            this.playVIS0ToolStripMenuItem,
            this.playCLR0ToolStripMenuItem});
            this.playToolStripMenuItem.Name = "playToolStripMenuItem";
            this.playToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.playToolStripMenuItem.Text = "Play";
            // 
            // playCHR0ToolStripMenuItem
            // 
            this.playCHR0ToolStripMenuItem.Checked = true;
            this.playCHR0ToolStripMenuItem.CheckOnClick = true;
            this.playCHR0ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.playCHR0ToolStripMenuItem.Name = "playCHR0ToolStripMenuItem";
            this.playCHR0ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.playCHR0ToolStripMenuItem.Text = "CHR0";
            // 
            // playSRT0ToolStripMenuItem
            // 
            this.playSRT0ToolStripMenuItem.Checked = true;
            this.playSRT0ToolStripMenuItem.CheckOnClick = true;
            this.playSRT0ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.playSRT0ToolStripMenuItem.Name = "playSRT0ToolStripMenuItem";
            this.playSRT0ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.playSRT0ToolStripMenuItem.Text = "SRT0";
            // 
            // playSHP0ToolStripMenuItem
            // 
            this.playSHP0ToolStripMenuItem.Checked = true;
            this.playSHP0ToolStripMenuItem.CheckOnClick = true;
            this.playSHP0ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.playSHP0ToolStripMenuItem.Name = "playSHP0ToolStripMenuItem";
            this.playSHP0ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.playSHP0ToolStripMenuItem.Text = "SHP0";
            // 
            // playPAT0ToolStripMenuItem
            // 
            this.playPAT0ToolStripMenuItem.Checked = true;
            this.playPAT0ToolStripMenuItem.CheckOnClick = true;
            this.playPAT0ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.playPAT0ToolStripMenuItem.Name = "playPAT0ToolStripMenuItem";
            this.playPAT0ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.playPAT0ToolStripMenuItem.Text = "PAT0";
            // 
            // playVIS0ToolStripMenuItem
            // 
            this.playVIS0ToolStripMenuItem.Checked = true;
            this.playVIS0ToolStripMenuItem.CheckOnClick = true;
            this.playVIS0ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.playVIS0ToolStripMenuItem.Name = "playVIS0ToolStripMenuItem";
            this.playVIS0ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.playVIS0ToolStripMenuItem.Text = "VIS0";
            // 
            // playCLR0ToolStripMenuItem
            // 
            this.playCLR0ToolStripMenuItem.Checked = true;
            this.playCLR0ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.playCLR0ToolStripMenuItem.Name = "playCLR0ToolStripMenuItem";
            this.playCLR0ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.playCLR0ToolStripMenuItem.Text = "CLR0";
            // 
            // linearInterpolationToolStripMenuItem
            // 
            this.linearInterpolationToolStripMenuItem.CheckOnClick = true;
            this.linearInterpolationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chkLinearCHR,
            this.chkLinearSRT,
            this.chkLinearSHP,
            this.chkLinearLight,
            this.chkLinearFog,
            this.chkLinearCamera});
            this.linearInterpolationToolStripMenuItem.Name = "linearInterpolationToolStripMenuItem";
            this.linearInterpolationToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.linearInterpolationToolStripMenuItem.Text = "Linear Interpolation";
            // 
            // chkLinearCHR
            // 
            this.chkLinearCHR.CheckOnClick = true;
            this.chkLinearCHR.Name = "chkLinearCHR";
            this.chkLinearCHR.Size = new System.Drawing.Size(147, 22);
            this.chkLinearCHR.Text = "CHR0";
            this.chkLinearCHR.CheckedChanged += new System.EventHandler(this.cHR0ToolStripMenuItem_CheckedChanged);
            // 
            // chkLinearSRT
            // 
            this.chkLinearSRT.CheckOnClick = true;
            this.chkLinearSRT.Name = "chkLinearSRT";
            this.chkLinearSRT.Size = new System.Drawing.Size(147, 22);
            this.chkLinearSRT.Text = "SRT0";
            this.chkLinearSRT.CheckedChanged += new System.EventHandler(this.sRT0ToolStripMenuItem_CheckedChanged);
            // 
            // chkLinearSHP
            // 
            this.chkLinearSHP.CheckOnClick = true;
            this.chkLinearSHP.Name = "chkLinearSHP";
            this.chkLinearSHP.Size = new System.Drawing.Size(147, 22);
            this.chkLinearSHP.Text = "SHP0";
            this.chkLinearSHP.CheckedChanged += new System.EventHandler(this.sHP0ToolStripMenuItem_CheckedChanged);
            // 
            // chkLinearLight
            // 
            this.chkLinearLight.Name = "chkLinearLight";
            this.chkLinearLight.Size = new System.Drawing.Size(147, 22);
            this.chkLinearLight.Text = "SCN0 Light";
            this.chkLinearLight.CheckedChanged += new System.EventHandler(this.sCN0LightsToolStripMenuItem_CheckedChanged);
            // 
            // chkLinearFog
            // 
            this.chkLinearFog.Name = "chkLinearFog";
            this.chkLinearFog.Size = new System.Drawing.Size(147, 22);
            this.chkLinearFog.Text = "SCN0 Fog";
            this.chkLinearFog.CheckedChanged += new System.EventHandler(this.sCN0FogToolStripMenuItem1_CheckedChanged);
            // 
            // chkLinearCamera
            // 
            this.chkLinearCamera.Name = "chkLinearCamera";
            this.chkLinearCamera.Size = new System.Drawing.Size(147, 22);
            this.chkLinearCamera.Text = "SCN0 Camera";
            this.chkLinearCamera.CheckedChanged += new System.EventHandler(this.sCN0CameraToolStripMenuItem1_CheckedChanged);
            // 
            // sCN0ToolStripMenuItem
            // 
            this.sCN0ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displayAmbienceToolStripMenuItem,
            this.displayLightsToolStripMenuItem,
            this.displayFogToolStripMenuItem,
            this.displayCameraToolStripMenuItem});
            this.sCN0ToolStripMenuItem.Name = "sCN0ToolStripMenuItem";
            this.sCN0ToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.sCN0ToolStripMenuItem.Text = "SCN0";
            // 
            // displayAmbienceToolStripMenuItem
            // 
            this.displayAmbienceToolStripMenuItem.Name = "displayAmbienceToolStripMenuItem";
            this.displayAmbienceToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.displayAmbienceToolStripMenuItem.Text = "Display Ambience";
            this.displayAmbienceToolStripMenuItem.Visible = false;
            // 
            // displayLightsToolStripMenuItem
            // 
            this.displayLightsToolStripMenuItem.Name = "displayLightsToolStripMenuItem";
            this.displayLightsToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.displayLightsToolStripMenuItem.Text = "Display Light";
            this.displayLightsToolStripMenuItem.Visible = false;
            // 
            // displayFogToolStripMenuItem
            // 
            this.displayFogToolStripMenuItem.Name = "displayFogToolStripMenuItem";
            this.displayFogToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.displayFogToolStripMenuItem.Text = "Display Fog";
            this.displayFogToolStripMenuItem.Visible = false;
            // 
            // displayCameraToolStripMenuItem
            // 
            this.displayCameraToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displayToolStripMenuItem,
            this.stPersonToolStripMenuItem});
            this.displayCameraToolStripMenuItem.Name = "displayCameraToolStripMenuItem";
            this.displayCameraToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.displayCameraToolStripMenuItem.Text = "Camera";
            // 
            // displayToolStripMenuItem
            // 
            this.displayToolStripMenuItem.Name = "displayToolStripMenuItem";
            this.displayToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.displayToolStripMenuItem.Text = "Display";
            this.displayToolStripMenuItem.Visible = false;
            // 
            // stPersonToolStripMenuItem
            // 
            this.stPersonToolStripMenuItem.CheckOnClick = true;
            this.stPersonToolStripMenuItem.Name = "stPersonToolStripMenuItem";
            this.stPersonToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.stPersonToolStripMenuItem.Text = "1st Person";
            this.stPersonToolStripMenuItem.CheckedChanged += new System.EventHandler(this.stPersonToolStripMenuItem_CheckedChanged);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.interpolationEditorToolStripMenuItem,
            this.selectedAnimationToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // interpolationEditorToolStripMenuItem
            // 
            this.interpolationEditorToolStripMenuItem.Name = "interpolationEditorToolStripMenuItem";
            this.interpolationEditorToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.interpolationEditorToolStripMenuItem.Text = "Interpolation Editor";
            this.interpolationEditorToolStripMenuItem.Click += new System.EventHandler(this.interpolationEditorToolStripMenuItem_Click);
            // 
            // selectedAnimationToolStripMenuItem
            // 
            this.selectedAnimationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.portToolStripMenuItem,
            this.mergeToolStripMenuItem,
            this.appendToolStripMenuItem,
            this.resizeToolStripMenuItem,
            this.interpolationToolStripMenuItem});
            this.selectedAnimationToolStripMenuItem.Enabled = false;
            this.selectedAnimationToolStripMenuItem.Name = "selectedAnimationToolStripMenuItem";
            this.selectedAnimationToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.selectedAnimationToolStripMenuItem.Text = "Selected Animation";
            // 
            // portToolStripMenuItem
            // 
            this.portToolStripMenuItem.Enabled = false;
            this.portToolStripMenuItem.Name = "portToolStripMenuItem";
            this.portToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.portToolStripMenuItem.Text = "Port";
            this.portToolStripMenuItem.Click += new System.EventHandler(this.portToolStripMenuItem_Click);
            // 
            // mergeToolStripMenuItem
            // 
            this.mergeToolStripMenuItem.Enabled = false;
            this.mergeToolStripMenuItem.Name = "mergeToolStripMenuItem";
            this.mergeToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.mergeToolStripMenuItem.Text = "Merge";
            this.mergeToolStripMenuItem.Click += new System.EventHandler(this.mergeToolStripMenuItem_Click);
            // 
            // appendToolStripMenuItem
            // 
            this.appendToolStripMenuItem.Enabled = false;
            this.appendToolStripMenuItem.Name = "appendToolStripMenuItem";
            this.appendToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.appendToolStripMenuItem.Text = "Append";
            this.appendToolStripMenuItem.Click += new System.EventHandler(this.appendToolStripMenuItem_Click);
            // 
            // resizeToolStripMenuItem
            // 
            this.resizeToolStripMenuItem.Enabled = false;
            this.resizeToolStripMenuItem.Name = "resizeToolStripMenuItem";
            this.resizeToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.resizeToolStripMenuItem.Text = "Resize";
            this.resizeToolStripMenuItem.Click += new System.EventHandler(this.resizeToolStripMenuItem_Click);
            // 
            // interpolationToolStripMenuItem
            // 
            this.interpolationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.averageAllStartEndTangentsToolStripMenuItem,
            this.averageboneStartendTangentsToolStripMenuItem});
            this.interpolationToolStripMenuItem.Name = "interpolationToolStripMenuItem";
            this.interpolationToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.interpolationToolStripMenuItem.Text = "Interpolation";
            // 
            // averageAllStartEndTangentsToolStripMenuItem
            // 
            this.averageAllStartEndTangentsToolStripMenuItem.Name = "averageAllStartEndTangentsToolStripMenuItem";
            this.averageAllStartEndTangentsToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.averageAllStartEndTangentsToolStripMenuItem.Text = "Average all start/end keyframes";
            this.averageAllStartEndTangentsToolStripMenuItem.Click += new System.EventHandler(this.averageAllStartEndTangentsToolStripMenuItem_Click);
            // 
            // averageboneStartendTangentsToolStripMenuItem
            // 
            this.averageboneStartendTangentsToolStripMenuItem.Enabled = false;
            this.averageboneStartendTangentsToolStripMenuItem.Name = "averageboneStartendTangentsToolStripMenuItem";
            this.averageboneStartendTangentsToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.averageboneStartendTangentsToolStripMenuItem.Text = "Average entry start/end keyframes";
            this.averageboneStartendTangentsToolStripMenuItem.Click += new System.EventHandler(this.averageboneStartendTangentsToolStripMenuItem_Click);
            // 
            // targetModelToolStripMenuItem
            // 
            this.targetModelToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hideFromSceneToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.hideAllOtherModelsToolStripMenuItem,
            this.deleteAllOtherModelsToolStripMenuItem,
            this.chkExternalAnims,
            this.chkBRRESAnims,
            this.chkNonBRRESAnims});
            this.targetModelToolStripMenuItem.Name = "targetModelToolStripMenuItem";
            this.targetModelToolStripMenuItem.Size = new System.Drawing.Size(93, 20);
            this.targetModelToolStripMenuItem.Text = "Target Model:";
            // 
            // hideFromSceneToolStripMenuItem
            // 
            this.hideFromSceneToolStripMenuItem.Name = "hideFromSceneToolStripMenuItem";
            this.hideFromSceneToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.hideFromSceneToolStripMenuItem.Text = "Hide from scene";
            this.hideFromSceneToolStripMenuItem.Click += new System.EventHandler(this.hideFromSceneToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.deleteToolStripMenuItem.Text = "Delete from scene";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // hideAllOtherModelsToolStripMenuItem
            // 
            this.hideAllOtherModelsToolStripMenuItem.Name = "hideAllOtherModelsToolStripMenuItem";
            this.hideAllOtherModelsToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.hideAllOtherModelsToolStripMenuItem.Text = "Hide all other models";
            this.hideAllOtherModelsToolStripMenuItem.Click += new System.EventHandler(this.hideAllOtherModelsToolStripMenuItem_Click);
            // 
            // deleteAllOtherModelsToolStripMenuItem
            // 
            this.deleteAllOtherModelsToolStripMenuItem.Name = "deleteAllOtherModelsToolStripMenuItem";
            this.deleteAllOtherModelsToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.deleteAllOtherModelsToolStripMenuItem.Text = "Delete all other models";
            this.deleteAllOtherModelsToolStripMenuItem.Click += new System.EventHandler(this.deleteAllOtherModelsToolStripMenuItem_Click);
            // 
            // chkExternalAnims
            // 
            this.chkExternalAnims.Checked = true;
            this.chkExternalAnims.CheckOnClick = true;
            this.chkExternalAnims.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExternalAnims.Name = "chkExternalAnims";
            this.chkExternalAnims.Size = new System.Drawing.Size(244, 22);
            this.chkExternalAnims.Text = "Display external animations";
            this.chkExternalAnims.CheckedChanged += new System.EventHandler(this.chkExternalAnims_CheckedChanged);
            // 
            // chkBRRESAnims
            // 
            this.chkBRRESAnims.Checked = true;
            this.chkBRRESAnims.CheckOnClick = true;
            this.chkBRRESAnims.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBRRESAnims.Name = "chkBRRESAnims";
            this.chkBRRESAnims.Size = new System.Drawing.Size(244, 22);
            this.chkBRRESAnims.Text = "Display animations in BRRES";
            this.chkBRRESAnims.CheckedChanged += new System.EventHandler(this.chkBRRESAnims_CheckedChanged);
            // 
            // chkNonBRRESAnims
            // 
            this.chkNonBRRESAnims.Checked = true;
            this.chkNonBRRESAnims.CheckOnClick = true;
            this.chkNonBRRESAnims.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNonBRRESAnims.Name = "chkNonBRRESAnims";
            this.chkNonBRRESAnims.Size = new System.Drawing.Size(244, 22);
            this.chkNonBRRESAnims.Text = "Display animations not in BRRES";
            this.chkNonBRRESAnims.CheckedChanged += new System.EventHandler(this.chkNonBRRESAnims_CheckedChanged);
            // 
            // kinectToolStripMenuItem
            // 
            this.kinectToolStripMenuItem.Name = "kinectToolStripMenuItem";
            this.kinectToolStripMenuItem.Size = new System.Drawing.Size(12, 20);
            // 
            // syncKinectToolStripMenuItem
            // 
            this.syncKinectToolStripMenuItem.Name = "syncKinectToolStripMenuItem";
            this.syncKinectToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // notYetImplementedToolStripMenuItem
            // 
            this.notYetImplementedToolStripMenuItem.Name = "notYetImplementedToolStripMenuItem";
            this.notYetImplementedToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // startTrackingToolStripMenuItem
            // 
            this.startTrackingToolStripMenuItem.Name = "startTrackingToolStripMenuItem";
            this.startTrackingToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // models
            // 
            this.models.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.models.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.models.FormattingEnabled = true;
            this.models.Items.AddRange(new object[] {
            "All"});
            this.models.Location = new System.Drawing.Point(294, 1);
            this.models.Name = "models";
            this.models.Size = new System.Drawing.Size(137, 21);
            this.models.TabIndex = 21;
            this.models.SelectedIndexChanged += new System.EventHandler(this.models_SelectedIndexChanged);
            // 
            // controlPanel
            // 
            this.controlPanel.Controls.Add(this.splitter1);
            this.controlPanel.Controls.Add(this.toolStrip1);
            this.controlPanel.Controls.Add(this.panel2);
            this.controlPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.controlPanel.Location = new System.Drawing.Point(0, 0);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(805, 24);
            this.controlPanel.TabIndex = 22;
            this.controlPanel.Visible = false;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(431, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 24);
            this.splitter1.TabIndex = 31;
            this.splitter1.TabStop = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chkBones,
            this.chkPolygons,
            this.chkVertices,
            this.chkCollisions,
            this.dropdownOverlays,
            this.toolStripSeparator1,
            this.chkFloor,
            this.button1,
            this.chkZoomExtents,
            this.btnSaveCam,
            this.toolStripSeparator2,
            this.cboToolSelect});
            this.toolStrip1.Location = new System.Drawing.Point(431, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.toolStrip1.Size = new System.Drawing.Size(374, 24);
            this.toolStrip1.TabIndex = 30;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // chkBones
            // 
            this.chkBones.Checked = true;
            this.chkBones.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBones.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.chkBones.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.chkBones.Name = "chkBones";
            this.chkBones.Size = new System.Drawing.Size(43, 21);
            this.chkBones.Text = "Bones";
            this.chkBones.Click += new System.EventHandler(this.chkBones_Click);
            // 
            // chkPolygons
            // 
            this.chkPolygons.Checked = true;
            this.chkPolygons.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPolygons.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.chkPolygons.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.chkPolygons.Name = "chkPolygons";
            this.chkPolygons.Size = new System.Drawing.Size(60, 21);
            this.chkPolygons.Text = "Polygons";
            this.chkPolygons.Click += new System.EventHandler(this.chkPolygons_Click);
            // 
            // chkVertices
            // 
            this.chkVertices.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.chkVertices.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.chkVertices.Name = "chkVertices";
            this.chkVertices.Size = new System.Drawing.Size(52, 21);
            this.chkVertices.Text = "Vertices";
            this.chkVertices.Click += new System.EventHandler(this.chkVertices_Click);
            // 
            // chkCollisions
            // 
            this.chkCollisions.Checked = true;
            this.chkCollisions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCollisions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.chkCollisions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.chkCollisions.Name = "chkCollisions";
            this.chkCollisions.Size = new System.Drawing.Size(62, 21);
            this.chkCollisions.Text = "Collisions";
            this.chkCollisions.Click += new System.EventHandler(this.chkCollisions_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 24);
            // 
            // chkFloor
            // 
            this.chkFloor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.chkFloor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.chkFloor.Name = "chkFloor";
            this.chkFloor.Size = new System.Drawing.Size(38, 21);
            this.chkFloor.Text = "Floor";
            this.chkFloor.Click += new System.EventHandler(this.chkFloor_Click);
            // 
            // button1
            // 
            this.button1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.button1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(83, 19);
            this.button1.Text = "Reset Camera";
            this.button1.Click += new System.EventHandler(this.resetCameraToolStripMenuItem_Click_1);
            // 
            // chkZoomExtents
            // 
            this.chkZoomExtents.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.chkZoomExtents.Image = ((System.Drawing.Image)(resources.GetObject("chkZoomExtents.Image")));
            this.chkZoomExtents.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.chkZoomExtents.Name = "chkZoomExtents";
            this.chkZoomExtents.Size = new System.Drawing.Size(83, 19);
            this.chkZoomExtents.Text = "Zoom Extents";
            this.chkZoomExtents.Click += new System.EventHandler(this.chkZoomExtents_Click);
            // 
            // btnSaveCam
            // 
            this.btnSaveCam.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSaveCam.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveCam.Name = "btnSaveCam";
            this.btnSaveCam.Size = new System.Drawing.Size(79, 19);
            this.btnSaveCam.Text = "Save Camera";
            this.btnSaveCam.Click += new System.EventHandler(this.btnSaveCam_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 6);
            // 
            // cboToolSelect
            // 
            this.cboToolSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboToolSelect.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.cboToolSelect.Items.AddRange(new object[] {
            "Translation",
            "Rotation",
            "Scale"});
            this.cboToolSelect.Name = "cboToolSelect";
            this.cboToolSelect.Size = new System.Drawing.Size(121, 23);
            this.cboToolSelect.Text = "Rotation";
            this.cboToolSelect.SelectedIndexChanged += new System.EventHandler(this.cboToolSelect_SelectedIndexChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.models);
            this.panel2.Controls.Add(this.menuStrip1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(431, 24);
            this.panel2.TabIndex = 29;
            // 
            // spltRight
            // 
            this.spltRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.spltRight.Location = new System.Drawing.Point(672, 24);
            this.spltRight.Name = "spltRight";
            this.spltRight.Size = new System.Drawing.Size(4, 391);
            this.spltRight.TabIndex = 23;
            this.spltRight.TabStop = false;
            this.spltRight.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.modelPanel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(157, 39);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(500, 361);
            this.panel1.TabIndex = 25;
            // 
            // modelPanel
            // 
            this.modelPanel.DefaultTranslate = new Vector3(0,0,0);
            this.modelPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modelPanel.InitialYFactor = 100;
            this.modelPanel.InitialZoomFactor = 5;
            this.modelPanel.Location = new System.Drawing.Point(0, 0);
            this.modelPanel.Name = "modelPanel";
            this.modelPanel.RotationScale = 0.4F;
            this.modelPanel.Size = new System.Drawing.Size(500, 361);
            this.modelPanel.TabIndex = 0;
            this.modelPanel.TranslationScale = 0.05F;
            this.modelPanel.ZoomScale = 2.5F;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 157);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Open";
            // 
            // animEditors
            // 
            this.animEditors.AutoScroll = true;
            this.animEditors.Controls.Add(this.pnlPlayback);
            this.animEditors.Controls.Add(this.animCtrlPnl);
            this.animEditors.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.animEditors.Location = new System.Drawing.Point(0, 415);
            this.animEditors.Name = "animEditors";
            this.animEditors.Size = new System.Drawing.Size(805, 60);
            this.animEditors.TabIndex = 29;
            this.animEditors.Visible = false;
            // 
            // pnlPlayback
            // 
            this.pnlPlayback.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPlayback.Enabled = false;
            this.pnlPlayback.Location = new System.Drawing.Point(264, 0);
            this.pnlPlayback.MinimumSize = new System.Drawing.Size(290, 54);
            this.pnlPlayback.Name = "pnlPlayback";
            this.pnlPlayback.Size = new System.Drawing.Size(541, 60);
            this.pnlPlayback.TabIndex = 15;
            this.pnlPlayback.Resize += new System.EventHandler(this.pnlPlayback_Resize);
            // 
            // animCtrlPnl
            // 
            this.animCtrlPnl.AutoScroll = true;
            this.animCtrlPnl.Controls.Add(this.vis0Editor);
            this.animCtrlPnl.Controls.Add(this.pat0Editor);
            this.animCtrlPnl.Controls.Add(this.shp0Editor);
            this.animCtrlPnl.Controls.Add(this.srt0Editor);
            this.animCtrlPnl.Controls.Add(this.chr0Editor);
            this.animCtrlPnl.Controls.Add(this.scn0Editor);
            this.animCtrlPnl.Controls.Add(this.clr0Editor);
            this.animCtrlPnl.Controls.Add(this.weightEditor);
            this.animCtrlPnl.Controls.Add(this.vertexEditor);
            this.animCtrlPnl.Dock = System.Windows.Forms.DockStyle.Left;
            this.animCtrlPnl.Location = new System.Drawing.Point(0, 0);
            this.animCtrlPnl.Name = "animCtrlPnl";
            this.animCtrlPnl.Size = new System.Drawing.Size(264, 60);
            this.animCtrlPnl.TabIndex = 29;
            // 
            // vis0Editor
            // 
            this.vis0Editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vis0Editor.Location = new System.Drawing.Point(0, 0);
            this.vis0Editor.Name = "vis0Editor";
            this.vis0Editor.Padding = new System.Windows.Forms.Padding(4);
            this.vis0Editor.Size = new System.Drawing.Size(264, 60);
            this.vis0Editor.TabIndex = 26;
            this.vis0Editor.Visible = false;
            // 
            // pat0Editor
            // 
            this.pat0Editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pat0Editor.Location = new System.Drawing.Point(0, 0);
            this.pat0Editor.Name = "pat0Editor";
            this.pat0Editor.Size = new System.Drawing.Size(264, 60);
            this.pat0Editor.TabIndex = 27;
            this.pat0Editor.Visible = false;
            // 
            // shp0Editor
            // 
            this.shp0Editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shp0Editor.Location = new System.Drawing.Point(0, 0);
            this.shp0Editor.Name = "shp0Editor";
            this.shp0Editor.Size = new System.Drawing.Size(264, 60);
            this.shp0Editor.TabIndex = 28;
            this.shp0Editor.Visible = false;
            // 
            // srt0Editor
            // 
            this.srt0Editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.srt0Editor.Location = new System.Drawing.Point(0, 0);
            this.srt0Editor.Name = "srt0Editor";
            this.srt0Editor.Size = new System.Drawing.Size(264, 60);
            this.srt0Editor.TabIndex = 20;
            this.srt0Editor.Visible = false;
            // 
            // chr0Editor
            // 
            this.chr0Editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chr0Editor.Location = new System.Drawing.Point(0, 0);
            this.chr0Editor.Name = "chr0Editor";
            this.chr0Editor.Size = new System.Drawing.Size(264, 60);
            this.chr0Editor.TabIndex = 19;
            this.chr0Editor.Visible = false;
            // 
            // scn0Editor
            // 
            this.scn0Editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scn0Editor.Location = new System.Drawing.Point(0, 0);
            this.scn0Editor.Name = "scn0Editor";
            this.scn0Editor.Size = new System.Drawing.Size(264, 60);
            this.scn0Editor.TabIndex = 30;
            this.scn0Editor.Visible = false;
            // 
            // clr0Editor
            // 
            this.clr0Editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clr0Editor.Location = new System.Drawing.Point(0, 0);
            this.clr0Editor.Name = "clr0Editor";
            this.clr0Editor.Size = new System.Drawing.Size(264, 60);
            this.clr0Editor.TabIndex = 30;
            this.clr0Editor.Visible = false;
            // 
            // weightEditor
            // 
            this.weightEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.weightEditor.Location = new System.Drawing.Point(0, 0);
            this.weightEditor.Name = "weightEditor";
            this.weightEditor.Size = new System.Drawing.Size(264, 60);
            this.weightEditor.TabIndex = 31;
            this.weightEditor.Visible = false;
            this.weightEditor.WeightIncrement = 0.1F;
            // 
            // vertexEditor
            // 
            this.vertexEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vertexEditor.Enabled = false;
            this.vertexEditor.Location = new System.Drawing.Point(0, 0);
            this.vertexEditor.Name = "vertexEditor";
            this.vertexEditor.Size = new System.Drawing.Size(264, 60);
            this.vertexEditor.TabIndex = 32;
            this.vertexEditor.Visible = false;
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter2.Location = new System.Drawing.Point(801, 24);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(4, 391);
            this.splitter2.TabIndex = 31;
            this.splitter2.TabStop = false;
            this.splitter2.Visible = false;
            // 
            // rightPanel
            // 
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.rightPanel.Location = new System.Drawing.Point(676, 24);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Size = new System.Drawing.Size(125, 391);
            this.rightPanel.TabIndex = 32;
            this.rightPanel.Visible = false;
            // 
            // leftPanel
            // 
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Location = new System.Drawing.Point(0, 24);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Size = new System.Drawing.Size(138, 391);
            this.leftPanel.TabIndex = 4;
            this.leftPanel.TargetAnimType = System.Windows.Forms.AnimType.None;
            this.leftPanel.Visible = false;
            // 
            // dropdownOverlays
            // 
            this.dropdownOverlays.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.dropdownOverlays.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chkBoundaries,
            this.chkSpawns,
            this.chkItems});
            this.dropdownOverlays.Image = ((System.Drawing.Image)(resources.GetObject("dropdownOverlays.Image")));
            this.dropdownOverlays.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.dropdownOverlays.Name = "dropdownOverlays";
            this.dropdownOverlays.Size = new System.Drawing.Size(65, 21);
            this.dropdownOverlays.Text = "Overlays";
            // 
            // chkBoundaries
            // 
            this.chkBoundaries.CheckOnClick = true;
            this.chkBoundaries.Name = "chkBoundaries";
            this.chkBoundaries.Size = new System.Drawing.Size(171, 22);
            this.chkBoundaries.Text = "Boundaries";
            this.chkBoundaries.Click += new System.EventHandler(this.chkBoundaries_Click);
            // 
            // chkSpawns
            // 
            this.chkSpawns.CheckOnClick = true;
            this.chkSpawns.Name = "chkSpawns";
            this.chkSpawns.Size = new System.Drawing.Size(171, 22);
            this.chkSpawns.Text = "Spawn/Respawns";
            this.chkSpawns.Click += new System.EventHandler(this.chkBoundaries_Click);
            // 
            // chkItems
            // 
            this.chkItems.CheckOnClick = true;
            this.chkItems.Name = "chkItems";
            this.chkItems.Size = new System.Drawing.Size(171, 22);
            this.chkItems.Text = "Item Spawn Zones";
            this.chkItems.Click += new System.EventHandler(this.chkBoundaries_Click);
            // 
            // ModelEditControl
            // 
            this.AllowDrop = true;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnTopToggle);
            this.Controls.Add(this.btnBottomToggle);
            this.Controls.Add(this.btnRightToggle);
            this.Controls.Add(this.spltRight);
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.btnLeftToggle);
            this.Controls.Add(this.spltLeft);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.controlPanel);
            this.Controls.Add(this.animEditors);
            this.Name = "ModelEditControl";
            this.Size = new System.Drawing.Size(805, 475);
            this.SizeChanged += new System.EventHandler(this.ModelEditControl_SizeChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ModelEditControl_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ModelEditControl_DragEnter);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.controlPanel.ResumeLayout(false);
            this.controlPanel.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.animEditors.ResumeLayout(false);
            this.animCtrlPnl.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        #region Initialization

        public ModelEditControl()
        {
            InitializeComponent();
            leftPanel._mainWindow = this;
            rightPanel.pnlKeyframes._mainWindow =
            rightPanel.pnlBones._mainWindow =
            srt0Editor._mainWindow =
            shp0Editor._mainWindow =
            pat0Editor._mainWindow =
            vis0Editor._mainWindow =
            scn0Editor._mainWindow =
            clr0Editor._mainWindow =
            weightEditor._mainWindow =
            vertexEditor._mainWindow =
            pnlPlayback._mainWindow =
            chr0Editor._mainWindow =
            this;

            animEditors.HorizontalScroll.Enabled = addedHeight = (!(animEditors.Width - animCtrlPnl.Width >= pnlPlayback.MinimumSize.Width));
            if (pnlPlayback.Width <= pnlPlayback.MinimumSize.Width)
            {
                pnlPlayback.Dock = DockStyle.Left;
                pnlPlayback.Width = pnlPlayback.MinimumSize.Width;
            }
            else
                pnlPlayback.Dock = DockStyle.Fill;

            _interpolationEditor = new Forms.InterpolationEditor(this);

            leftPanel.fileType.DataSource = _editableAnimTypes;

            TargetAnimType = AnimType.CHR;
            m_DelegateOpenFile = new DelegateOpenFile(OpenFile);
            ScreenCapBgLocText.Text = Application.StartupPath + "\\ScreenCaptures";

            _timer = new CoolTimer();
            _timer.RenderFrame += _timer_RenderFrame;

            modelPanel.PreRender += (EventPreRender = new System.Windows.Forms.GLRenderEventHandler(this.modelPanel1_PreRender));
            modelPanel.PostRender += (EventPostRender = new System.Windows.Forms.GLRenderEventHandler(this.modelPanel1_PostRender));
            modelPanel.MouseDown += (EventMouseDown = new System.Windows.Forms.MouseEventHandler(this.modelPanel1_MouseDown));
            modelPanel.MouseMove += (EventMouseMove = new System.Windows.Forms.MouseEventHandler(this.modelPanel1_MouseMove));
            modelPanel.MouseUp += (EventMouseUp = new System.Windows.Forms.MouseEventHandler(this.modelPanel1_MouseUp));

            KeyframePanel.visEditor.EntryChanged += new EventHandler(this.VISEntryChanged);
            KeyframePanel.visEditor.IndexChanged += new EventHandler(this.VISIndexChanged);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            panel1.Controls.Add(_interpolationEditor);
            _interpolationEditor.SendToBack();
            _interpolationEditor.Dock = DockStyle.Fill;
            _interpolationEditor.Visible = false;
        }

        #endregion


        private void chkZoomExtents_Click(object sender, EventArgs e)
        {
            if (SelectedBone != null)
            {
                modelPanel._camera.Reset();
                modelPanel._camera.Translate(_selectedBone._frameMatrix.GetPoint() + new Vector3(0.0f,0.0f,27.0f));
                ModelPanel.Invalidate();
            }
            else
                MessageBox.Show("Select a bone!");
        }

        private void cboToolSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboToolSelect.SelectedIndex == 0) { _editType = TransformType.Translation; }
            else if (cboToolSelect.SelectedIndex == 1) { _editType = TransformType.Rotation; }
            else if (cboToolSelect.SelectedIndex == 2) { _editType = TransformType.Scale; }
            ModelPanel.Invalidate();
        }

        private void chkBoundaries_Click(object sender, EventArgs e)
        {
            ModelPanel.Invalidate();
        }

    }
}
