using System;
using System.Collections.Generic;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using System.ComponentModel;
using BrawlLib.SSBBTypes;
using BrawlLib;
using BrawlLib.Modeling;

namespace BrawlBox
{
    [NodeWrapper(ResourceType.ARC)]
    class ARCWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static ARCWrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("Ne&w", null,
                new ToolStripMenuItem("ARChive", null, NewARCAction),
                new ToolStripMenuItem("BRResource Pack", null, NewBRESAction),
                new ToolStripMenuItem("Collision", null, NewCollisionAction),
                new ToolStripMenuItem("BLOC", null, NewBLOCAction),
                new ToolStripMenuItem("MSBin", null, NewMSBinAction),
                new ToolStripMenuItem("SCLA (Empty)", null, NewSCLAAction),
                new ToolStripMenuItem("SCLA (Full)", null, NewSCLAFullAction),
#if DEBUG
                new ToolStripMenuItem("SCLA (Expanded)", null, NewSCLAExpandedAction),
#endif
                new ToolStripMenuItem("STDT", null, NewSTDTAction),
                new ToolStripMenuItem("STPM", null, NewSTPMAction)
                ));
            _menu.Items.Add(new ToolStripMenuItem("&Import", null,
                new ToolStripMenuItem("ARChive", null, ImportARCAction),
                new ToolStripMenuItem("BRResource Pack", null, ImportBRESAction),
                new ToolStripMenuItem("BLOC", null, ImportBLOCAction),
                new ToolStripMenuItem("MSBin", null, ImportMSBinAction),
                new ToolStripMenuItem("SCLA", null, ImportSCLAAction),
                new ToolStripMenuItem("STDT", null, ImportSTDTAction),
                new ToolStripMenuItem("STPM", null, ImportSTPMAction),
                new ToolStripMenuItem("Stage Table", null,
                    new ToolStripMenuItem("TBCL", null, ImportTBCLAction),
                    new ToolStripMenuItem("TBGC", null, ImportTBGCAction),
                    new ToolStripMenuItem("TBGD", null, ImportTBGDAction),
                    new ToolStripMenuItem("TBGM", null, ImportTBGMAction),
                    new ToolStripMenuItem("TBLV", null, ImportTBLVAction),
                    new ToolStripMenuItem("TBRM", null, ImportTBRMAction),
                    new ToolStripMenuItem("TBST", null, ImportTBSTAction)
                    )
                ));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Preview All Models", null, PreviewAllAction));
            _menu.Items.Add(new ToolStripMenuItem("Export All", null, ExportAllAction));
            _menu.Items.Add(new ToolStripMenuItem("Replace All", null, ReplaceAllAction));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Export", null, ExportAction, Keys.Control | Keys.E));
            _menu.Items.Add(new ToolStripMenuItem("&Replace", null, ReplaceAction, Keys.Control | Keys.R));
            _menu.Items.Add(new ToolStripMenuItem("Res&tore", null, RestoreAction, Keys.Control | Keys.T));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUpAction, Keys.Control | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Move D&own", null, MoveDownAction, Keys.Control | Keys.Down));
            _menu.Items.Add(new ToolStripMenuItem("Re&name", null, RenameAction, Keys.Control | Keys.N));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteAction, Keys.Control | Keys.Delete));
            _menu.Opening += MenuOpening;
            _menu.Closing += MenuClosing;
        }
        protected static void NewBRESAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().NewBRES(); }
        protected static void NewARCAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().NewARC(); }
        protected static void NewMSBinAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().NewMSBin(); }
        protected static void NewCollisionAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().NewCollision(); }
        protected static void NewBLOCAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().NewBLOC(); }
        protected static void NewSCLAAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().NewSCLA(0); }
        protected static void NewSCLAFullAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().NewSCLA(32); }
        protected static void NewSCLAExpandedAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().NewSCLA(256); }
        protected static void NewSTDTAction(object sender, EventArgs e)
        {
            STDTCreator entryCount = new STDTCreator();
            if (entryCount.ShowDialog() == DialogResult.OK)
                GetInstance<ARCWrapper>().NewSTDT(entryCount.NewValue);
        }
        protected static void NewSTPMAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().NewSTPM(); }
        protected static void ImportBRESAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportBRES(); }
        protected static void ImportARCAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportARC(); }
        protected static void ImportBLOCAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportBLOC(); }
        protected static void ImportCollisionAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportCollision(); }
        protected static void ImportMSBinAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportMSBin(); }
        protected static void ImportSCLAAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportSCLA(); }
        protected static void ImportSTDTAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportSTDT(); }
        protected static void ImportSTPMAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportSTPM(); }

        protected static void ImportTBCLAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportTBCL(); }
        protected static void ImportTBGCAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportTBGC(); }
        protected static void ImportTBGDAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportTBGD(); }
        protected static void ImportTBGMAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportTBGM(); }
        protected static void ImportTBLVAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportTBLV(); }
        protected static void ImportTBRMAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportTBRM(); }
        protected static void ImportTBSTAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ImportTBST(); }

        protected static void PreviewAllAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().PreviewAll(); }
        protected static void ExportAllAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ExportAll(); }
        protected static void ReplaceAllAction(object sender, EventArgs e) { GetInstance<ARCWrapper>().ReplaceAll(); }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[8].Enabled = _menu.Items[9].Enabled = _menu.Items[11].Enabled = _menu.Items[12].Enabled = _menu.Items[15].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            ARCWrapper w = GetInstance<ARCWrapper>();
                _menu.Items[8].Enabled = _menu.Items[15].Enabled = w.Parent != null;
                _menu.Items[9].Enabled = ((w._resource.IsDirty) || (w._resource.IsBranch));
                _menu.Items[11].Enabled = w.PrevNode != null;
                _menu.Items[12].Enabled = w.NextNode != null;
        }
        #endregion

        public override string ExportFilter
        {
            get
            {
                return "PAC Archive (*.pac)|*.pac|" +
                    "Compressed PAC Archive (*.pcs)|*.pcs|" +
                    "Archive Pair (*.pair)|*.pair|" +
                    "Multiple Resource Group (*.mrg)|*.mrg|" +
                    "Compressed MRG (*.mrgc)|*.mrgc";
            }
        }

        public ARCWrapper() { ContextMenuStrip = _menu; }

        public ARCNode NewARC()
        {
            ARCNode node = new ARCNode() { Name = _resource.FindName("NewARChive"), FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            w.TreeView.SelectedNode = w;
            return node;
        }
        public BRRESNode NewBRES()
        {
            BRRESNode node = new BRRESNode() { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            w.TreeView.SelectedNode = w;
            return node;
        }
        public CollisionNode NewCollision()
        {
            CollisionNode node = new CollisionNode() { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            w.TreeView.SelectedNode = w;
            return node;
        }
        public BLOCNode NewBLOC()
        {
            BLOCNode node = new BLOCNode() { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            w.TreeView.SelectedNode = w;
            return node;
        }
        public MSBinNode NewMSBin()
        {
            MSBinNode node = new MSBinNode() { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            w.TreeView.SelectedNode = w;
            return node;
        }
        
        // StageBox create SCLA
        public SCLANode NewSCLA(uint index)
        {
            SCLANode node = new SCLANode(index) { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            w.TreeView.SelectedNode = w;
            return node;
        }
        
        // StageBox create STDT
        public STDTNode NewSTDT(int numEntries)
        {
            STDTNode node = new STDTNode(null, numEntries) { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            // Viewing a STDT immediately after creation causes a crash
            w.TreeView.SelectedNode = w;
            return node;
        }
        
        // StageBox create STPM
        public STPMNode NewSTPM()
        {
            STPMNode node = new STPMNode() { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            w.TreeView.SelectedNode = w;
            return node;
        }
        
        // StageBox create TBCL
        public TBCLNode NewTBCL()
        {
            TBCLNode node = new TBCLNode() { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            // Viewing a TBCL immediately after creation causes a crash
            // w.TreeView.SelectedNode = w;
            return node;
        }
        
        // StageBox create TBGC
        public TBGCNode NewTBGC()
        {
            TBGCNode node = new TBGCNode() { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            // Viewing a TBGC immediately after creation causes a crash
            // w.TreeView.SelectedNode = w;
            return node;
        }
        
        // StageBox create TBGD
        public TBGDNode NewTBGD()
        {
            TBGDNode node = new TBGDNode() { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            // Viewing a TBGD immediately after creation causes a crash
            // w.TreeView.SelectedNode = w;
            return node;
        }
        
        // StageBox create TBGM
        public TBGMNode NewTBGM()
        {
            TBGMNode node = new TBGMNode() { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            // Viewing a TBGM immediately after creation causes a crash
            // w.TreeView.SelectedNode = w;
            return node;
        }
        
        // StageBox create TBLV
        public TBLVNode NewTBLV()
        {
            TBLVNode node = new TBLVNode() { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            // Viewing a TBLV immediately after creation causes a crash
            // w.TreeView.SelectedNode = w;
            return node;
        }
        
        // StageBox create TBRM
        public TBRMNode NewTBRM()
        {
            TBRMNode node = new TBRMNode() { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            // Viewing a TBRM immediately after creation causes a crash
            // w.TreeView.SelectedNode = w;
            return node;
        }
        
        // StageBox create TBST
        public TBSTNode NewTBST()
        {
            TBSTNode node = new TBSTNode() { FileType = ARCFileType.MiscData };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            // Viewing a TBST immediately after creation causes a crash
            // w.TreeView.SelectedNode = w;
            return node;
        }

        public void ImportARC()
        {
            string path;
            if (Program.OpenFile("ARChive (*.pac,*.pcs)|*.pac;*.pcs", out path) > 0)
                NewARC().Replace(path);
        }
        public void ImportBRES()
        {
            string path;
            if (Program.OpenFile(FileFilters.BRES, out path) > 0)
                NewBRES().Replace(path);
        }
        public void ImportBLOC()
        {
            string path;
            if (Program.OpenFile(FileFilters.BLOC, out path) > 0)
                NewBLOC().Replace(path);
        }
        public void ImportCollision()
        {
            string path;
            if (Program.OpenFile(FileFilters.CollisionDef, out path) > 0)
                NewBRES().Replace(path);
        }
        public void ImportMSBin()
        {
            string path;
            if (Program.OpenFile(FileFilters.MSBin, out path) > 0)
                NewMSBin().Replace(path);
        }
        
        // StageBox import SCLA
        public void ImportSCLA()
        {
            string path;
            if (Program.OpenFile(FileFilters.SCLA, out path) > 0)
                NewSCLA(0).Replace(path);
        }
        
        // StageBox import STDT
        public void ImportSTDT()
        {
            string path;
            if (Program.OpenFile(FileFilters.STDT, out path) > 0)
            {
                STDTNode node = NewSTDT(1);
                node.Replace(path);
                BaseWrapper w = this.FindResource(node, false);
                w.EnsureVisible();
                w.TreeView.SelectedNode = w;
            }
        }
        
        // StageBox import STPM
        public void ImportSTPM()
        {
            string path;
            if (Program.OpenFile(FileFilters.STPM, out path) > 0)
                NewSTPM().Replace(path);
        }
        
        // StageBox import TBCL
        public void ImportTBCL()
        {
            string path;
            if (Program.OpenFile(FileFilters.TBCL, out path) > 0)
            {
                TBCLNode node = NewTBCL();
                node.Replace(path);
                BaseWrapper w = this.FindResource(node, false);
                w.EnsureVisible();
                w.TreeView.SelectedNode = w;
            }
        }
        
        // StageBox import TBGC
        public void ImportTBGC()
        {
            string path;
            if (Program.OpenFile(FileFilters.TBGC, out path) > 0)
            {
                TBGCNode node = NewTBGC();
                node.Replace(path);
                BaseWrapper w = this.FindResource(node, false);
                w.EnsureVisible();
                w.TreeView.SelectedNode = w;
            }
        }
        
        // StageBox import TBGD
        public void ImportTBGD()
        {
            string path;
            if (Program.OpenFile(FileFilters.TBGD, out path) > 0)
            {
                TBGDNode node = NewTBGD();
                node.Replace(path);
                BaseWrapper w = this.FindResource(node, false);
                w.EnsureVisible();
                w.TreeView.SelectedNode = w;
            }
        }
        
        // StageBox import TBGM
        public void ImportTBGM()
        {
            string path;
            if (Program.OpenFile(FileFilters.TBGM, out path) > 0)
            {
                TBGMNode node = NewTBGM();
                node.Replace(path);
                BaseWrapper w = this.FindResource(node, false);
                w.EnsureVisible();
                w.TreeView.SelectedNode = w;
            }
        }
        
        // StageBox import TBLV
        public void ImportTBLV()
        {
            string path;
            if (Program.OpenFile(FileFilters.TBLV, out path) > 0)
            {
                TBLVNode node = NewTBLV();
                node.Replace(path);
                BaseWrapper w = this.FindResource(node, false);
                w.EnsureVisible();
                w.TreeView.SelectedNode = w;
            }
        }
        
        // StageBox import TBRM
        public void ImportTBRM()
        {
            string path;
            if (Program.OpenFile(FileFilters.TBRM, out path) > 0)
            {
                TBRMNode node = NewTBRM();
                node.Replace(path);
                BaseWrapper w = this.FindResource(node, false);
                w.EnsureVisible();
                w.TreeView.SelectedNode = w;
            }
        }
        
        // StageBox import TBST
        public void ImportTBST()
        {
            string path;
            if (Program.OpenFile(FileFilters.TBST, out path) > 0)
            {
                TBSTNode node = NewTBST();
                node.Replace(path);
                BaseWrapper w = this.FindResource(node, false);
                w.EnsureVisible();
                w.TreeView.SelectedNode = w;
            }
        }
        
        public override void OnExport(string outPath, int filterIndex)
        {
            switch (filterIndex)
            {
                case 1: ((ARCNode)_resource).Export(outPath); break;
                case 2: ((ARCNode)_resource).ExportPCS(outPath); break;
                case 3: ((ARCNode)_resource).ExportPair(outPath); break;
                case 4: ((ARCNode)_resource).ExportAsMRG(outPath); break;
            }
        }

        private void LoadModels(ResourceNode node, List<IModel> models, List<CollisionNode> collisions)
        {
            switch (node.ResourceType)
            {
                case ResourceType.ARC:
                case ResourceType.MRG:
                case ResourceType.U8:
                case ResourceType.U8Folder:
                case ResourceType.BRES:
                case ResourceType.BRESGroup:
                    foreach (ResourceNode n in node.Children)
                        LoadModels(n, models, collisions);
                    break;
                case ResourceType.MDL0:
                    models.Add((IModel)node);
                    break;
                case ResourceType.CollisionDef:
                    collisions.Add((CollisionNode)node);
                    break;
            }
        }

        public void PreviewAll()
        {
            List<IModel> models = new List<IModel>();
            List<CollisionNode> collisions = new List<CollisionNode>();
            LoadModels(_resource, models, collisions);
            new ModelForm().Show(_owner, models, collisions);
        }

        public void ExportAll()
        {
            string path = Program.ChooseFolder();
            if (path == null)
                return;

            ((ARCNode)_resource).ExtractToFolder(path);
        }

        public void ReplaceAll()
        {
            string path = Program.ChooseFolder();
            if (path == null)
                return;

            ((ARCNode)_resource).ReplaceFromFolder(path);
        }
    }
}
