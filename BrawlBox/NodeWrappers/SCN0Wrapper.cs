using System;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib;
using System.Windows.Forms;
using System.ComponentModel;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.SCN0)]
    class SCN0Wrapper : GenericWrapper
    {
        #region Menu
        
        private static ContextMenuStrip _menu;
        static SCN0Wrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("&New LightSet", null, newLightSetAction, Keys.Control | Keys.L));
            _menu.Items.Add(new ToolStripMenuItem("&New Ambient Light", null, newAmbLightAction, Keys.Control | Keys.B));
            _menu.Items.Add(new ToolStripMenuItem("&New Light", null, newLightAction, Keys.Control | Keys.H));
            _menu.Items.Add(new ToolStripMenuItem("&New FogSet", null, newFogSetAction, Keys.Control | Keys.F));
            _menu.Items.Add(new ToolStripMenuItem("&New Camera", null, newCameraAction, Keys.Control | Keys.C));
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
        protected static void newLightSetAction(object sender, EventArgs e) { GetInstance<SCN0Wrapper>().newLightSet(); }
        protected static void newAmbLightAction(object sender, EventArgs e) { GetInstance<SCN0Wrapper>().newAmbLight(); }
        protected static void newLightAction(object sender, EventArgs e) { GetInstance<SCN0Wrapper>().newLight(); }
        protected static void newFogSetAction(object sender, EventArgs e) { GetInstance<SCN0Wrapper>().newFogSet(); }
        protected static void newCameraAction(object sender, EventArgs e) { GetInstance<SCN0Wrapper>().newCamera(); }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[7].Enabled = _menu.Items[8].Enabled = _menu.Items[10].Enabled = _menu.Items[11].Enabled = _menu.Items[14].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            SCN0Wrapper w = GetInstance<SCN0Wrapper>();
            _menu.Items[7].Enabled = _menu.Items[14].Enabled = w.Parent != null;
            _menu.Items[8].Enabled = ((w._resource.IsDirty) || (w._resource.IsBranch));
            _menu.Items[10].Enabled = w.PrevNode != null;
            _menu.Items[11].Enabled = w.NextNode != null;
        }

        #endregion

        public SCN0Wrapper() { ContextMenuStrip = _menu; }

        public override string ExportFilter { get { return FileFilters.SCN0; } }

        public void newLightSet()
        {
            SCN0EntryNode node = ((SCN0Node)_resource).CreateResource<SCN0LightSetNode>("NewLightSet");
            BaseWrapper res = this.FindResource(node, true);
            res.EnsureVisible();
            res.TreeView.SelectedNode = res;
        }
        public void newAmbLight()
        {
            SCN0EntryNode node = ((SCN0Node)_resource).CreateResource<SCN0AmbientLightNode>("NewAmbientLight");
            BaseWrapper res = this.FindResource(node, true);
            res.EnsureVisible();
            res.TreeView.SelectedNode = res;
        }
        public void newLight()
        {
            SCN0EntryNode node = ((SCN0Node)_resource).CreateResource<SCN0LightNode>("NewLight");
            BaseWrapper res = this.FindResource(node, true);
            res.EnsureVisible();
            res.TreeView.SelectedNode = res;
        }
        public void newFogSet()
        {
            SCN0EntryNode node = ((SCN0Node)_resource).CreateResource<SCN0FogNode>("NewFogSet");
            BaseWrapper res = this.FindResource(node, true);
            res.EnsureVisible();
            res.TreeView.SelectedNode = res;
        }
        public void newCamera()
        {
            SCN0EntryNode node = ((SCN0Node)_resource).CreateResource<SCN0CameraNode>("NewCamera");
            BaseWrapper res = this.FindResource(node, true);
            res.EnsureVisible();
            res.TreeView.SelectedNode = res;
        }
    }
}
