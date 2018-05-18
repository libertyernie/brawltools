using System;
using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using System.ComponentModel;


namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.MSBin)]
    class MSBinWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static MSBinWrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("&Export", null, ExportAction, Keys.Control | Keys.E));
            _menu.Items.Add(new ToolStripMenuItem("&Replace", null, ReplaceAction, Keys.Control | Keys.R));
            _menu.Items.Add(new ToolStripMenuItem("Res&tore", null, RestoreAction, Keys.Control | Keys.T));
            _menu.Items.Add(new ToolStripMenuItem("&Duplicate", null, DuplicateAction, Keys.Control | Keys.D));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUpAction, Keys.Control | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Move D&own", null, MoveDownAction, Keys.Control | Keys.Down));
            _menu.Items.Add(new ToolStripMenuItem("Re&name", null, RenameAction, Keys.Control | Keys.N));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteAction, Keys.Control | Keys.Delete));
            _menu.Opening += MenuOpening;
            _menu.Closing += MenuClosing;
        }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[1].Enabled = _menu.Items[2].Enabled = _menu.Items[5].Enabled = _menu.Items[6].Enabled = _menu.Items[9].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            MSBinWrapper w = GetInstance<MSBinWrapper>();
            _menu.Items[1].Enabled = _menu.Items[4].Enabled = _menu.Items[9].Enabled = w.Parent != null;
            _menu.Items[2].Enabled = ((w._resource.IsDirty) || (w._resource.IsBranch));
            _menu.Items[5].Enabled = w.PrevNode != null;
            _menu.Items[6].Enabled = w.NextNode != null;
        }

        #endregion

        public override string ExportFilter { get { return FileFilters.MSBin; } }

        public override ResourceNode Duplicate()
        {
            if(_resource._parent == null)
            {
                return null;
            }
            _resource.Rebuild();
            MSBinNode newNode = NodeFactory.FromAddress(null, _resource.WorkingUncompressed.Address, _resource.WorkingUncompressed.Length) as MSBinNode;
            int newIndex = _resource.Index + 1;
            _resource._parent.InsertChild(newNode, true, newIndex);
            newNode.Populate();
            newNode.FileType = ((MSBinNode)_resource).FileType;
            newNode.FileIndex = ((MSBinNode)_resource).FileIndex;
            newNode.RedirectIndex = ((MSBinNode)_resource).RedirectIndex;
            newNode.Name = _resource.Name;
            return newNode;
        }

        public MSBinWrapper() { ContextMenuStrip = _menu; }
    }
}
