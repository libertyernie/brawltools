using System;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using System.ComponentModel;
using BrawlLib;
using BrawlLib.Imaging;
using BrawlLib.Wii.Models;
using BrawlLib.SSBB;
using BrawlLib.Modeling;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.VBN)]
    class VBNWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static VBNWrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("&Preview", null, PreviewAction, Keys.Control | Keys.P));
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
        protected static void PreviewAction(object sender, EventArgs e) { GetInstance<VBNWrapper>().Preview(); }

        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[3].Enabled = _menu.Items[4].Enabled = _menu.Items[6].Enabled = _menu.Items[7].Enabled = _menu.Items[10].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            VBNWrapper w = GetInstance<VBNWrapper>();
            _menu.Items[3].Enabled = _menu.Items[10].Enabled = w.Parent != null;
            _menu.Items[4].Enabled = ((w._resource.IsDirty) || (w._resource.IsBranch));
            _menu.Items[6].Enabled = w.PrevNode != null;
            _menu.Items[7].Enabled = w.NextNode != null;
        }
        #endregion

        public override string ExportFilter { get { return FileFilters.VBN; } }
        public override string ImportFilter { get { return FileFilters.VBN; } }

        public VBNWrapper() { ContextMenuStrip = _menu; }

        public void Preview()
        {
            new ModelForm().Show(_owner, (IModel)_resource);
        }
    }

    [NodeWrapper(ResourceType.VBNBone)]
    class VBNBoneWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static VBNBoneWrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("&Export", null, ExportAction, Keys.Control | Keys.E));
            _menu.Items.Add(new ToolStripMenuItem("&Replace", null, ReplaceAction, Keys.Control | Keys.R));
            _menu.Items.Add(new ToolStripMenuItem("Re&name", null, RenameAction, Keys.Control | Keys.N));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUpAction, Keys.Control | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Move D&own", null, MoveDownAction, Keys.Control | Keys.Down));
            _menu.Items.Add(new ToolStripMenuItem("Add To &Parent", null, AddToParentAction, Keys.Control | Keys.Shift | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Add To Next &Up", null, AddUpAction, Keys.Control | Keys.Alt | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Add To Next D&own", null, AddDownAction, Keys.Control | Keys.Alt | Keys.Down));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Add New Child", null, CreateAction, Keys.Control | Keys.Alt | Keys.N));
            _menu.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteAction, Keys.Control | Keys.Delete));
            _menu.Opening += MenuOpening;
            _menu.Closing += MenuClosing;
        }
        protected static void AddToParentAction(object sender, EventArgs e) { GetInstance<VBNBoneWrapper>().AddToParent(); }
        protected static void AddUpAction(object sender, EventArgs e) { GetInstance<VBNBoneWrapper>().AddUp(); }
        protected static void AddDownAction(object sender, EventArgs e) { GetInstance<VBNBoneWrapper>().AddDown(); }
        protected static void CreateAction(object sender, EventArgs e) { GetInstance<VBNBoneWrapper>().CreateNode(); }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[4].Enabled = _menu.Items[5].Enabled = _menu.Items[6].Enabled = _menu.Items[7].Enabled = _menu.Items[8].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            VBNBoneWrapper w = GetInstance<VBNBoneWrapper>();
            _menu.Items[4].Enabled = w.PrevNode != null;
            _menu.Items[5].Enabled = w.NextNode != null;
            _menu.Items[6].Enabled = w.Parent != null && w._resource.Parent is VBNBoneNode;
            _menu.Items[7].Enabled = w.PrevNode != null;
            _menu.Items[8].Enabled = w.NextNode != null;
        }

        public unsafe void AddUp()
        {
            //try
            //{
            if (PrevNode == null)
                return;

            if (_resource.AddUp())
            {
                TreeNode prev = PrevNode;
                TreeView.BeginUpdate();
                Remove();
                prev.Nodes.Add(this);
                _resource.Parent = _resource.Parent.Children[_resource.Index - 1];
                _resource.OnMoved();
                TreeView.EndUpdate();
                EnsureVisible();
                //TreeView.SelectedNode = this;
            }
            else
                return;
            //}
            //catch { return; }
        }

        public void AddDown()
        {
            //try
            //{
            if (NextNode == null)
                return;

            if (_resource.AddDown())
            {
                TreeNode next = NextNode;
                TreeView.BeginUpdate();
                Remove();
                next.Nodes.Add(this);
                _resource.Parent = _resource.Parent.Children[_resource.Index + 1];
                _resource.OnMoved();
                TreeView.EndUpdate();
                EnsureVisible();
                //TreeView.SelectedNode = this;
            }
            else
                return;
            //}
            //catch { return; }
        }

        public void AddToParent()
        {
            //try
            //{
            if (Parent == null)
                return;

            if (_resource.ToParent())
            {
                TreeNode parent = Parent;
                TreeView.BeginUpdate();
                Remove();
                parent.Parent.Nodes.Add(this);
                _resource.Parent = _resource.Parent.Parent;
                _resource.OnMoved();
                TreeView.EndUpdate();
                EnsureVisible();
                //TreeView.SelectedNode = this;
            }
            //}
            //catch { return; }
        }
        private void CreateNode()
        {
            TreeView.BeginUpdate();

            int id = 1;
            string name = "NewBone0";
            IModel model = ((VBNBoneNode)_resource).IModel;
            Top:
            foreach (VBNBoneNode b in model.BoneCache)
            {
                if (b.Name == name)
                {
                    name = "NewBone" + id++;
                    goto Top;
                }
            }
            VBNBoneNode bone = new VBNBoneNode(FrameState.Neutral, model.BoneCache.Length) { Name = name };

            bone._bindMatrix =
            bone._invBindMatrix =
            bone._frameMatrix =
            bone._invFrameMatrix =
            Matrix.Identity;

            _resource.AddChild(bone, true);

            TreeView.EndUpdate();

            Nodes[Nodes.Count - 1].EnsureVisible();
            //TreeView.SelectedNode = Nodes[Nodes.Count - 1];
        }
        #endregion

        public VBNBoneWrapper() { ContextMenuStrip = _menu; }
    }
}
