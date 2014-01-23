using System;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using BrawlLib;
using System.ComponentModel;
using BrawlLib.IO;
using BrawlLib.SSBBTypes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.MDL0Material)]
    class MDL0MaterialWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static MDL0MaterialWrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("&Export", null, ExportAction, Keys.Control | Keys.E));
            _menu.Items.Add(new ToolStripMenuItem("&Replace", null, ReplaceAction, Keys.Control | Keys.R));
            _menu.Items.Add(new ToolStripMenuItem("Re&name", null, RenameAction, Keys.Control | Keys.N));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUpAction, Keys.Control | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Move D&own", null, MoveDownAction, Keys.Control | Keys.Down));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Add New Reference", null, CreateAction, Keys.Control | Keys.Alt | Keys.N));
            _menu.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteAction, Keys.Control | Keys.Delete));
            _menu.Opening += MenuOpening;
            _menu.Closing += MenuClosing;
        }
        protected static void CreateAction(object sender, EventArgs e) { GetInstance<MDL0MaterialWrapper>().CreateRef(); }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[4].Enabled = _menu.Items[5].Enabled = _menu.Items[7].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            MDL0MaterialWrapper w = GetInstance<MDL0MaterialWrapper>();
            _menu.Items[4].Enabled = w.PrevNode != null;
            _menu.Items[5].Enabled = w.NextNode != null;
            _menu.Items[7].Enabled = w._resource.Children.Count < 8; //8 mat refs max!
        }
        private void CreateRef()
        {
            if (_resource.Children.Count < 8)
            {
                MDL0MaterialRefNode node = new MDL0MaterialRefNode();
                _resource.AddChild(node);
                node.Default();
                _resource.SignalPropertyChange();

                if (node.Model.AutoMetalMaterials && ((MDL0MaterialNode)node.Parent).MetalMaterial != null)
                    ((MDL0MaterialNode)node.Parent).MetalMaterial.UpdateAsMetal();

                Nodes[Nodes.Count - 1].EnsureVisible();
                //TreeView.SelectedNode = Nodes[Nodes.Count - 1];
            }
        }
        #endregion

        public MDL0MaterialWrapper() { ContextMenuStrip = _menu; }
    }
}
