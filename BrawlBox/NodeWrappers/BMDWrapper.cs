using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using System.ComponentModel;
using BrawlLib;
using BrawlLib.SSBBTypes;
using BrawlLib.Imaging;
using BrawlLib.Wii.Models;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.BMD)]
    class BMDWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static BMDWrapper()
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

        protected static void PreviewAction(object sender, EventArgs e) { GetInstance<BMDWrapper>().Preview(); }

        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[3].Enabled = _menu.Items[4].Enabled = _menu.Items[6].Enabled = _menu.Items[7].Enabled = _menu.Items[10].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            BMDWrapper w = GetInstance<BMDWrapper>();
            _menu.Items[3].Enabled = _menu.Items[10].Enabled = w.Parent != null;
            _menu.Items[4].Enabled = ((w._resource.IsDirty) || (w._resource.IsBranch));
            _menu.Items[6].Enabled = w.PrevNode != null;
            _menu.Items[7].Enabled = w.NextNode != null;
        }
        #endregion

        public override string ExportFilter { get { return FileFilters.Raw; } }
        public override string ImportFilter { get { return FileFilters.Raw; } }

        public BMDWrapper() { ContextMenuStrip = _menu; }

        public void Preview()
        {
            using (ModelForm form = new ModelForm())
                form.ShowDialog(_owner, (BMDNode)_resource);
        }
    }
}
