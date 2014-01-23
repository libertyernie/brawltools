using System;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using BrawlLib;
using System.ComponentModel;
using BrawlLib.IO;
using BrawlLib.SSBBTypes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TEVStage)]
    class TEVStageWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static TEVStageWrapper()
        {
            _menu = new ContextMenuStrip();
            
            _menu.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUpAction, Keys.Control | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Move D&own", null, MoveDownAction, Keys.Control | Keys.Down));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteAction, Keys.Control | Keys.Delete));
            _menu.Opening += MenuOpening;
            _menu.Closing += MenuClosing;
        }
        protected static void MoveUpAction(object sender, EventArgs e) { GetInstance<TEVStageWrapper>().MoveUp(); }
        protected static void MoveDownAction(object sender, EventArgs e) { GetInstance<TEVStageWrapper>().MoveDown(); }
        
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[0].Enabled = _menu.Items[1].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            TEVStageWrapper w = GetInstance<TEVStageWrapper>();
            _menu.Items[0].Enabled = w.PrevNode != null;
            _menu.Items[1].Enabled = w.NextNode != null;
        }

        public void MoveUp()
        {
            if (PrevNode == null)
                return;

            if (_resource.MoveUp())
            {
                int index = Index - 1;
                TreeNode parent = Parent;
                TreeView.BeginUpdate();
                Remove();
                parent.Nodes.Insert(index, this);
                TreeView.SelectedNode = this;
                TreeView.EndUpdate();

                foreach (ResourceNode n in _resource.Parent.Children)
                    n.Name = "Stage" + n.Index;
            }
        }

        public void MoveDown()
        {
            if (NextNode == null)
                return;

            if (_resource.MoveDown())
            {
                int index = Index + 1;
                TreeNode parent = Parent;
                TreeView.BeginUpdate();
                Remove();
                parent.Nodes.Insert(index, this);
                TreeView.SelectedNode = this;
                TreeView.EndUpdate();

                foreach (ResourceNode n in _resource.Parent.Children)
                    n.Name = "Stage" + n.Index;
            }
        }
        #endregion

        public TEVStageWrapper() { ContextMenuStrip = _menu; }
    }
}
