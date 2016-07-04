using System;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using System.ComponentModel;
using BrawlLib.SSBBTypes;
using BrawlLib;

namespace BrawlBox
{
    [NodeWrapper(ResourceType.PACK)]
    class PACKWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static PACKWrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("Ne&w", null,
                new ToolStripMenuItem("OMO Animation", null, NewOMOAction)
                ));
            _menu.Items.Add(new ToolStripMenuItem("&Import", null,
                new ToolStripMenuItem("OMO Animation", null, ImportOMOAction)
                ));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Export All", null, ExportAllAction));
            _menu.Items.Add(new ToolStripMenuItem("Replace All", null, ReplaceAllAction));
            _menu.Items.Add(new ToolStripMenuItem("Import All", null, ImportAllAction));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Export", null, ExportAction, Keys.Control | Keys.E));
            _menu.Items.Add(new ToolStripMenuItem("&Replace", null, ReplaceAction, Keys.Control | Keys.R));
            _menu.Items.Add(new ToolStripMenuItem("Res&tore", null, RestoreAction, Keys.Control | Keys.T));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUpAction, Keys.Control | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Move D&own", null, MoveDownAction, Keys.Control | Keys.Down));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteAction, Keys.Control | Keys.Delete));
            _menu.Opening += MenuOpening;
            _menu.Closing += MenuClosing;
        }
        protected static void NewOMOAction(object sender, EventArgs e) { GetInstance<PACKWrapper>().NewOMO(); }
        protected static void ImportOMOAction(object sender, EventArgs e) { GetInstance<PACKWrapper>().ImportOMO(); }
        protected static void ExportAllAction(object sender, EventArgs e) { GetInstance<PACKWrapper>().ExportAll(); }
        protected static void ReplaceAllAction(object sender, EventArgs e) { GetInstance<PACKWrapper>().ReplaceAll(); }
        protected static void ImportAllAction(object sender, EventArgs e) { GetInstance<PACKWrapper>().ImportAll(); }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[8].Enabled = _menu.Items[9].Enabled = _menu.Items[11].Enabled = _menu.Items[12].Enabled = _menu.Items[14].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            PACKWrapper w = GetInstance<PACKWrapper>();

            _menu.Items[8].Enabled = _menu.Items[14].Enabled = w.Parent != null;
            _menu.Items[9].Enabled = ((w._resource.IsDirty) || (w._resource.IsBranch));
            _menu.Items[11].Enabled = w.PrevNode != null;
            _menu.Items[12].Enabled = w.NextNode != null;
        }
        #endregion

        public override string ExportFilter { get { return "Pack File (*.pac)|*.pac"; } }

        public PACKWrapper() { ContextMenuStrip = _menu; }

        public OMONode NewOMO()
        {
            OMONode node = new OMONode() { Name = _resource.FindName("NewOMO") };
            _resource.AddChild(node);

            BaseWrapper w = this.FindResource(node, false);
            w.EnsureVisible();
            w.TreeView.SelectedNode = w;
            return node;
        }

        public void ImportOMO()
        {
            string path;
            if (Program.OpenFile("OMO Animation (*.omo)|*.omo", out path) > 0)
                NewOMO().Replace(path);
        }

        public void ExportAll()
        {
            string path = Program.ChooseFolder();
            if (path == null)
                return;

            ((PACKNode)_resource).ExtractToFolder(path);
        }

        public void ReplaceAll()
        {
            string path = Program.ChooseFolder();
            if (path == null)
                return;

            ((PACKNode)_resource).ReplaceFromFolder(path);
        }

        public void ImportAll()
        {
            string path = Program.ChooseFolder();
            if (path == null)
                return;

            ((PACKNode)_resource).ImportFromFolder(path);
        }
    }
}
