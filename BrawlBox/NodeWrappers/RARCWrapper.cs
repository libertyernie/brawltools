using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using System.ComponentModel;
using BrawlLib.SSBBTypes;
using BrawlLib;

namespace BrawlBox
{
    [NodeWrapper(ResourceType.RARC)]
    class RARCWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static RARCWrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("Ne&w", null,
                new ToolStripMenuItem("Folder", null, NewFolderAction)
                ));
            _menu.Items.Add(new ToolStripMenuItem("&Import", null,
                new ToolStripMenuItem("RARChive", null, ImportRARCAction)
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
        protected static void NewFolderAction(object sender, EventArgs e) { GetInstance<RARCWrapper>().NewFolder(); }
        protected static void ImportRARCAction(object sender, EventArgs e) { GetInstance<RARCWrapper>().ImportRARC(); }
        protected static void PreviewAllAction(object sender, EventArgs e) { GetInstance<RARCWrapper>().PreviewAll(); }
        protected static void ExportAllAction(object sender, EventArgs e) { GetInstance<RARCWrapper>().ExportAll(); }
        protected static void ReplaceAllAction(object sender, EventArgs e) { GetInstance<RARCWrapper>().ReplaceAll(); }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[8].Enabled = _menu.Items[9].Enabled = _menu.Items[11].Enabled = _menu.Items[12].Enabled = _menu.Items[15].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            RARCWrapper w = GetInstance<RARCWrapper>();
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
                return "RARC Archive (*.arc,*.rarc)|*.arc;*.rarc|" +
                    "Compressed RARC Archive (*.szs,*.szp)|*.szs;*.szp";
            }
        }

        public RARCWrapper() { ContextMenuStrip = _menu; }

        public RARCNode NewFolder()
        {
            //RARCNode node = new RARCNode() { Name = _resource.FindName("NewRARChive") };
            //_resource.AddChild(node);

            //BaseWrapper w = this.FindResource(node, false);
            //w.EnsureVisible();
            //w.TreeView.SelectedNode = w;
            //return node;

            return null;
        }
        public void ImportRARC()
        {
            //string path;
            //if (Program.OpenFile("RARChive (*.arc,*.szs,*.szp)|*.arc;*.szs;*.szp", out path) > 0)
            //    NewRARC().Replace(path);
        }
        
        //public override void OnExport(string outPath, int filterIndex)
        //{
        //    switch (filterIndex)
        //    {
        //        case 1: ((RARCNode)_resource).Export(outPath); break;
        //        case 2: ((RARCNode)_resource).ExportSZS(outPath); break;
        //        case 3: ((RARCNode)_resource).ExportPair(outPath); break;
        //        case 4: ((RARCNode)_resource).ExportAsMRG(outPath); break;
        //    }
        //}

        public void PreviewAll()
        {
            //List<BMDNode> models = new List<BMDNode>();
            //LoadModels(_resource, models);
            //using (ModelForm form = new ModelForm())
            //{
            //    form.ShowDialog(_owner, models);
            //}
        }

        public void ExportAll()
        {
            string path = Program.ChooseFolder();
            if (path == null)
                return;

            //((RARCNode)_resource).ExtractToFolder(path);
        }

        public void ReplaceAll()
        {
            string path = Program.ChooseFolder();
            if (path == null)
                return;

            //((RARCNode)_resource).ReplaceFromFolder(path);
        }
    }
}
