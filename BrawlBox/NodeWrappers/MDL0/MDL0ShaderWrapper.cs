using System;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using BrawlLib;
using System.ComponentModel;
using BrawlLib.IO;
using BrawlLib.SSBBTypes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.MDL0Shader)]
    class MDL0ShaderWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static MDL0ShaderWrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("&Export", null, ExportAction, Keys.Control | Keys.E));
            _menu.Items.Add(new ToolStripMenuItem("&Replace", null, ReplaceAction, Keys.Control | Keys.R));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUpAction, Keys.Control | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Move D&own", null, MoveDownAction, Keys.Control | Keys.Down));
            _menu.Items.Add(new ToolStripSeparator());
            //_menu.Items.Add(new ToolStripMenuItem("Add New Structure", null, CreateAction, Keys.Control | Keys.Alt | Keys.N));
            _menu.Items.Add(new ToolStripMenuItem("Add New Stage", null, CreateAction, Keys.Control | Keys.Alt | Keys.N));
            _menu.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteAction, Keys.Control | Keys.Delete));
            _menu.Opening += MenuOpening;
            _menu.Closing += MenuClosing;
        }
        protected static void MoveUpAction(object sender, EventArgs e) { GetInstance<MDL0ShaderWrapper>().MoveUp(); }
        protected static void MoveDownAction(object sender, EventArgs e) { GetInstance<MDL0ShaderWrapper>().MoveDown(); }
        //protected static void CreateAction(object sender, EventArgs e) { GetInstance<MDL0ShaderWrapper>().CreateStruct(); }
        protected static void CreateAction(object sender, EventArgs e) { GetInstance<MDL0ShaderWrapper>().CreateStage(); }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[6].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            MDL0ShaderWrapper w = GetInstance<MDL0ShaderWrapper>();
            _menu.Items[6].Enabled = w._resource.Children.Count < 16; //16 stages max!
        }
        //private void CreateStruct()
        //{
        //    if (_resource.Children.Count < 8)
        //    {
        //        MDL0ShaderStructNode shadstr = new MDL0ShaderStructNode();
        //        _resource.AddChild(shadstr, true);
        //        shadstr.Default();

        //        Nodes[Nodes.Count - 1].EnsureVisible();
        //        //TreeView.SelectedNode = Nodes[Nodes.Count - 1];
        //    }
        //}

        private void CreateStage()
        {
            if (_resource.Children.Count < 16)
            {
                TEVStageNode stage = new TEVStageNode();
                _resource.AddChild(stage, true);

                Nodes[Nodes.Count - 1].EnsureVisible();
                //TreeView.SelectedNode = Nodes[Nodes.Count - 1];
            }
        }

        public new void MoveUp()
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
                    n.Name = "Shader" + n.Index;
            }
        }

        public new void MoveDown()
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
                    n.Name = "Shader" + n.Index;
            }
        }
        #endregion

        public MDL0ShaderWrapper() { ContextMenuStrip = _menu; }
    }
}
