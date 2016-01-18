using System;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using System.ComponentModel;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.RSARFolder)]
    class RSARFolderWrapper : GenericWrapper
    {
        #region Menu

        private static ContextMenuStrip _menu;
        static RSARFolderWrapper()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuItem("New", null,
                new ToolStripMenuItem("Folder", null, NewFolderAction),
                new ToolStripMenuItem("Sound", null,
                    new ToolStripMenuItem("From File", null, ImportSoundAction),
                    new ToolStripMenuItem("From Existing", null, CopySoundAction)),
                new ToolStripMenuItem("Bank", null, NewBankAction),
                new ToolStripMenuItem("PlayerInfo", null, NewTypeAction),
                new ToolStripMenuItem("Group", null, NewGroupAction)
                ));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUpAction, Keys.Control | Keys.Up));
            _menu.Items.Add(new ToolStripMenuItem("Move D&own", null, MoveDownAction, Keys.Control | Keys.Down));
            _menu.Items.Add(new ToolStripMenuItem("Re&name", null, RenameAction, Keys.Control | Keys.N));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteAction, Keys.Control | Keys.Delete));
            _menu.Opening += MenuOpening;
            _menu.Closing += MenuClosing;
        }
        protected static void NewFolderAction(object sender, EventArgs e) { GetInstance<RSARFolderWrapper>().NewFolder(); }
        protected static void ImportSoundAction(object sender, EventArgs e) { GetInstance<RSARFolderWrapper>().ImportSound(); }
        protected static void CopySoundAction(object sender, EventArgs e) { GetInstance<RSARFolderWrapper>().CopySound(); }
        protected static void NewBankAction(object sender, EventArgs e) { GetInstance<RSARFolderWrapper>().NewBank(); }
        protected static void NewTypeAction(object sender, EventArgs e) { GetInstance<RSARFolderWrapper>().NewType(); }
        protected static void NewGroupAction(object sender, EventArgs e) { GetInstance<RSARFolderWrapper>().NewGroup(); }
        private static void MenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _menu.Items[2].Enabled = _menu.Items[3].Enabled = true;
        }
        private static void MenuOpening(object sender, CancelEventArgs e)
        {
            GenericWrapper w = GetInstance<GenericWrapper>();
            _menu.Items[2].Enabled = w.PrevNode != null;
            _menu.Items[3].Enabled = w.NextNode != null;
        }

        #endregion

        public RSARFolderWrapper() 
        {
            ContextMenuStrip = _menu; 
        }

        public void NewFolder()
        {
            RSARFolderNode folder = new RSARFolderNode();
            folder.Name = _resource.FindName("NewFolder");
            _resource.AddChild(folder);
        }

        public void ImportSound()
        {

        }

        public void CopySound()
        {
            using (CloneSoundDialog dlg = new CloneSoundDialog())
                dlg.ShowDialog(null, ResourceNode as RSARFolderNode);
        }

        public void NewBank()
        {
            RSARBankNode folder = new RSARBankNode();
            folder.Name = _resource.FindName("NewBank");
            _resource.AddChild(folder);
        }

        public void NewType()
        {
            RSARPlayerInfoNode folder = new RSARPlayerInfoNode();
            folder.Name = _resource.FindName("NewPlayerInfo");
            _resource.AddChild(folder);
        }

        public void NewGroup()
        {
            RSARGroupNode folder = new RSARGroupNode();
            folder.Name = _resource.FindName("NewGroup");
            _resource.AddChild(folder);
        }
    }
}
