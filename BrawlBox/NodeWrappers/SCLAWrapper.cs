using System;
using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using System.ComponentModel;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.SCLA)]
    class SCLAWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static SCLAWrapper()
        {
            _menu = new ContextMenuStrip();

            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("Add New Entry", null, NewEntryAction, Keys.Control | Keys.J));
            _menu.Items.Add(new ToolStripMenuItem("Fill", null, FillAction, Keys.Control | Keys.F));
#if DEBUG
            _menu.Items.Add(new ToolStripMenuItem("Fill (Expanded)", null, FillExpandedAction, Keys.Control | Keys.Alt | Keys.F));
#endif
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
        protected static void NewEntryAction(object sender, EventArgs e) { GetInstance<SCLAWrapper>().NewEntry(); }
        protected static void FillAction(object sender, EventArgs e) { GetInstance<SCLAWrapper>().FillSCLA(32); }
        protected static void FillExpandedAction(object sender, EventArgs e) { GetInstance<SCLAWrapper>().FillSCLA(256); }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
#if DEBUG
            _menu.Items[8].Enabled = _menu.Items[9].Enabled = true;
#else
            _menu.Items[7].Enabled = _menu.Items[8].Enabled = true;
#endif
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            SCLAWrapper w = GetInstance<SCLAWrapper>();
#if DEBUG
            _menu.Items[8].Enabled = w.PrevNode != null;
            _menu.Items[9].Enabled = w.NextNode != null;
#else
            _menu.Items[7].Enabled = w.PrevNode != null;
            _menu.Items[8].Enabled = w.NextNode != null;
#endif
        }
        #endregion

        public override string ExportFilter { get { return FileFilters.SCLA; } }

        public void NewEntry()
        {
            SCLAEntryNode node = new SCLAEntryNode();
            _resource.AddChild(node);
        }

        public void FillSCLA(uint fillAmount)
        {
            ((SCLANode)_resource).fillSCLA(fillAmount);
        }

        public SCLAWrapper() { ContextMenuStrip = _menu; }
    }
}