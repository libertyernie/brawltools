using System;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using BrawlLib.SSBB;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.OMO)]
    class OMOWrapper : GenericWrapper
    {
        #region Menu
        private static ContextMenuStrip _menu;
        static OMOWrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("&Load Skeleton", null, LoadSkeletonAction, Keys.Control | Keys.L));
            _menu.Items.Add(new ToolStripMenuItem("&Clear Skeleton", null, ClearSkeletonAction, Keys.Control | Keys.B));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Replace", null, ReplaceAction, Keys.Control | Keys.R));
            _menu.Items.Add(new ToolStripMenuItem("&Export", null, ExportAction, Keys.Control | Keys.E));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteAction, Keys.Control | Keys.D));
            _menu.Opening += MenuOpening;
            _menu.Closing += MenuClosing;
        }
        protected static void LoadSkeletonAction(object sender, EventArgs e) { GetInstance<OMOWrapper>().LoadSkeleton(); }
        protected static void ClearSkeletonAction(object sender, EventArgs e) { GetInstance<OMOWrapper>().ClearSkeleton(); }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[6].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            OMOWrapper w = GetInstance<OMOWrapper>();
            _menu.Items[6].Enabled = w.Parent != null;
        }
        #endregion

        public OMOWrapper() { ContextMenuStrip = _menu; }

        public override string ExportFilter { get { return FileFilters.Raw; } }

        public void LoadSkeleton()
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = SupportedFilesHandler.GetCompleteFilter("vbn");
            o.Title = "Please select a skeleton to use.";
            if (o.ShowDialog() == DialogResult.OK)
            {
                OMONode._skeleton = NodeFactory.FromFile(null, o.FileName) as VBNNode;
                OMONode._skeleton.Populate();
            }
        }
        public void ClearSkeleton()
        {
            OMONode._skeleton = null;
        }
    }
}
