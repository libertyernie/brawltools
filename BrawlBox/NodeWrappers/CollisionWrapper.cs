using System;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using System.ComponentModel;
using BrawlLib;

namespace BrawlBox
{
    [NodeWrapper(ResourceType.CollisionDef)]
    class CollisionWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static CollisionWrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("&Preview / Edit", null, EditAction, Keys.Control | Keys.P));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Export", null, ExportAction, Keys.Control | Keys.E));
            _menu.Items.Add(new ToolStripMenuItem("&Replace", null, ReplaceAction, Keys.Control | Keys.R));
            _menu.Items.Add(new ToolStripMenuItem("Res&tore", null, RestoreAction, Keys.Control | Keys.T));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUpAction, Keys.Control | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Move D&own", null, MoveDownAction, Keys.Control | Keys.Down));
            _menu.Items.Add(new ToolStripMenuItem("Re&name", null, RenameAction, Keys.Control | Keys.N));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Mirror Unbound Collisions", null,
                new ToolStripMenuItem("X-Axis", null, FlipXAction),
                new ToolStripMenuItem("Y-Axis", null, FlipYAction)
                ));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteAction, Keys.Control | Keys.Shift | Keys.Delete));
            _menu.Opening += MenuOpening;
            _menu.Closing += MenuClosing;
        }
        // StageBox collision flipping
        private static void FlipXAction(object sender, EventArgs e) { GetInstance<CollisionWrapper>().FlipX(); }
        private static void FlipYAction(object sender, EventArgs e) { GetInstance<CollisionWrapper>().FlipY(); }

        protected static void EditAction(object sender, EventArgs e) { GetInstance<CollisionWrapper>().Preview(); }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[3].Enabled = _menu.Items[4].Enabled = _menu.Items[6].Enabled = _menu.Items[7].Enabled = _menu.Items[10].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            CollisionWrapper w = GetInstance<CollisionWrapper>();
            _menu.Items[3].Enabled = _menu.Items[10].Enabled = w.Parent != null;
            _menu.Items[4].Enabled = ((w._resource.IsDirty) || (w._resource.IsBranch));
            _menu.Items[6].Enabled = w.PrevNode != null;
            _menu.Items[7].Enabled = w.NextNode != null;
        }
        #endregion

        public override string ExportFilter { get { return FileFilters.CollisionDef; } }

        public CollisionWrapper() { ContextMenuStrip = _menu; }


        public void FlipX()
        {
            CollisionNode coll = ((CollisionNode)_resource);
            //int i = 0;
            //int j = 0;
            foreach(CollisionObject cObj in coll._objects)
            {
                //++i;
                //Console.WriteLine("COLLISION OBJECT: " + i);
                if(cObj.LinkedBone == null)
                {
                    //j = 0;
                    //Console.WriteLine("   Not linked to a model");
                    cObj.resetFlip();
                    foreach (CollisionPlane p in cObj._planes)
                    {
                        //++j;
                        //Console.WriteLine("      Initiating Plane: " + j);
                        p.flipAcrossPlane('X');
                    }
                }
            }
            coll.SignalPropertyChange();
        }

        public void FlipY()
        {
            CollisionNode coll = ((CollisionNode)_resource);
            //int i = 0;
            //int j = 0;
            foreach (CollisionObject cObj in coll._objects)
            {
                //++i;
                //Console.WriteLine("COLLISION OBJECT: " + i);
                if (cObj.LinkedBone == null)
                {
                    //j = 0;
                    //Console.WriteLine("   Not linked to a model");
                    cObj.resetFlip();
                    foreach (CollisionPlane p in cObj._planes)
                    {
                        //++j;
                        //Console.WriteLine("      Initiating Plane: " + j);
                        p.flipAcrossPlane('Y');
                    }
                }
            }
            coll.SignalPropertyChange();
        }

        private void Preview()
        {
            using (CollisionForm frm = new CollisionForm())
                frm.ShowDialog(null, _resource as CollisionNode);
        }
    }
}
