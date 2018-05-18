using System;
using System.Windows.Forms;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;
using System.ComponentModel;

namespace BrawlBox
{
    //Contains generic members inherited by all sub-classed nodes
    class GenericWrapper : BaseWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static GenericWrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("&Export", null, ExportAction, Keys.Control | Keys.E));
            _menu.Items.Add(new ToolStripMenuItem("&Replace", null, ReplaceAction, Keys.Control | Keys.R));
            _menu.Items.Add(new ToolStripMenuItem("Res&tore", null, RestoreAction, Keys.Control | Keys.T));
            //_menu.Items.Add(new ToolStripMenuItem("&Duplicate", null, DuplicateAction, Keys.Control | Keys.D));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUpAction, Keys.Control | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Move D&own", null, MoveDownAction, Keys.Control | Keys.Down));
            _menu.Items.Add(new ToolStripMenuItem("Re&name", null, RenameAction, Keys.Control | Keys.N));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteAction, Keys.Control | Keys.Delete));
            _menu.Opening += MenuOpening;
            _menu.Closing += MenuClosing;
        }
        protected static void DuplicateAction(object sender, EventArgs e) { GetInstance<GenericWrapper>().Duplicate(); }

        protected static void MoveUpAction(object sender, EventArgs e) { GetInstance<GenericWrapper>().MoveUp(); }
        protected static void MoveDownAction(object sender, EventArgs e) { GetInstance<GenericWrapper>().MoveDown(); }
        protected static void ExportAction(object sender, EventArgs e) { GetInstance<GenericWrapper>().Export(); }
        protected static void ReplaceAction(object sender, EventArgs e) { GetInstance<GenericWrapper>().Replace(); }
        protected static void RestoreAction(object sender, EventArgs e) { GetInstance<GenericWrapper>().Restore(); }
        protected static void DeleteAction(object sender, EventArgs e) { GetInstance<GenericWrapper>().Delete(); }
        protected static void RenameAction(object sender, EventArgs e) { GetInstance<GenericWrapper>().Rename(); }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[1].Enabled = _menu.Items[2].Enabled = _menu.Items[4].Enabled = _menu.Items[5].Enabled = _menu.Items[8].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            GenericWrapper w = GetInstance<GenericWrapper>();
            _menu.Items[1].Enabled = _menu.Items[8].Enabled = w.Parent != null;
            _menu.Items[2].Enabled = ((w._resource.IsDirty) || (w._resource.IsBranch));
            _menu.Items[4].Enabled = w.PrevNode != null;
            _menu.Items[5].Enabled = w.NextNode != null;
        }

        #endregion

        public GenericWrapper(IWin32Window owner) { _owner = owner;  ContextMenuStrip = _menu; }
        public GenericWrapper() { _owner = null; ContextMenuStrip = _menu; }

        public virtual ResourceNode Duplicate()
        {
            if (_resource._parent == null)
            {
                return null;
            }
            if(_resource._parent.GetType() == typeof(BRESGroupNode))
            {
                if (_resource._parent._parent == null)
                {
                    return null;
                }
                _resource._parent._parent.Rebuild();
                ResourceNode newNode = NodeFactory.FromAddress(null, _resource.WorkingUncompressed.Address, _resource.WorkingUncompressed.Length);
                int newIndex = _resource.Index + 1;
                if(!_resource.AllowDuplicateNames)
                {
                    int i = 2;
                    string newName = _resource.Name + " (" + i + ")";
                    while (_resource._parent.FindChildrenByName(newName).Length != 0)
                    {
                        ++i;
                        newIndex = _resource._parent.FindChildrenByName(newName)[0].Index + 1;
                        newName = _resource.Name + " (" + i + ")";
                    }
                    newNode.Name = newName;
                }
                else
                {
                    newNode.Name = _resource.Name;
                }
                _resource._parent.InsertChild(newNode, true, newIndex);
                newNode.Populate();
                return newNode;
            }
            else
            {
                _resource.Rebuild();
                ResourceNode newNode = NodeFactory.FromAddress(null, _resource.WorkingUncompressed.Address, _resource.WorkingUncompressed.Length);
                int newIndex = _resource.Index + 1;
                if (!_resource.AllowDuplicateNames)
                {
                    int i = 2;
                    string newName = _resource.Name + " (" + i + ")";
                    while (_resource._parent.FindChildrenByName(newName).Length != 0)
                    {
                        ++i;
                        newIndex = _resource._parent.FindChildrenByName(newName)[0].Index + 1;
                        newName = _resource.Name + " (" + i + ")";
                    }
                    newNode.Name = newName;
                }
                else
                {
                    newNode.Name = _resource.Name;
                }
                _resource._parent.InsertChild(newNode, true, newIndex);
                newNode.Populate();
                return newNode;
            }
        }

        public void MoveUp() { MoveUp(true); }
        public virtual void MoveUp(bool select)
        {
            if (PrevVisibleNode == null)
                return;

            if (_resource.MoveUp())
            {
                int index = Index - 1;
                TreeNode parent = Parent;
                TreeView.BeginUpdate();
                Remove();
                parent.Nodes.Insert(index, this);
                _resource.OnMoved();
                if (select)
                    TreeView.SelectedNode = this;
                TreeView.EndUpdate();
            }
        }

        public void MoveDown() { MoveDown(true); }
        public virtual void MoveDown(bool select)
        {
            if (NextVisibleNode == null)
                return;

            if (_resource.MoveDown())
            {
                int index = Index + 1;
                TreeNode parent = Parent;
                TreeView.BeginUpdate();
                Remove();
                parent.Nodes.Insert(index, this);
                _resource.OnMoved();
                if (select)
                    TreeView.SelectedNode = this;
                TreeView.EndUpdate();
            }
        }

        public virtual string ExportFilter { get { return BrawlLib.FileFilters.Raw; } }
        public virtual string ImportFilter { get { return ExportFilter; } }
        public virtual string ReplaceFilter { get { return ImportFilter; } }

        public static int CategorizeFilter(string path, string filter)
        {
            string ext = "*" + Path.GetExtension(path);

            string[] split = filter.Split('|');
            for (int i = 3; i < split.Length; i += 2)
                foreach (string s in split[i].Split(';'))
                    if (s.Equals(ext, StringComparison.OrdinalIgnoreCase))
                        return (i + 1) / 2;
            return 1;
        }

        public virtual string Export()
        {
            string outPath;
            int index = Program.SaveFile(ExportFilter, Text, out outPath);
            if (index != 0)
            {
                if (Parent == null)
                    _resource.Merge(Control.ModifierKeys == (Keys.Control | Keys.Shift));
                OnExport(outPath, index);
            }
            return outPath;
        }
        public virtual void OnExport(string outPath, int filterIndex) { _resource.Export(outPath); }

        public virtual void Replace()
        {
            if (Parent == null)
                return;

            string inPath;
            int index = Program.OpenFile(ReplaceFilter, out inPath);
            if (index != 0)
            {
                OnReplace(inPath, index);
                this.Link(_resource);
            }
        }

        public virtual void OnReplace(string inStream, int filterIndex) { _resource.Replace(inStream); }

        public void Restore()
        {
            _resource.Restore();
        }

        public void Delete()
        {
            if (Parent == null)
                return;

            _resource.Dispose();
            _resource.Remove();
        }

        public void Rename()
        {
            using (RenameDialog dlg = new RenameDialog()) { dlg.ShowDialog(MainForm.Instance, _resource); }
        }
    }
}
