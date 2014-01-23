using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Ikarus.UI
{
    public partial class OpenedFilesDialog : Form
    {
        public OpenedFilesDialog()
        {
            InitializeComponent();
            listBox1.DataSource = Program.OpenedFilePaths;
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            Process.Start(Program.RootPath + listBox1.SelectedItem as string);
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            int index = listBox1.IndexFromPoint(e.X, e.Y);
            if (listBox1.SelectedIndex != index)
                listBox1.SelectedIndex = index;

            if (e.Button == MouseButtons.Right)
                if (listBox1.SelectedIndex >= 0)
                    listBox1.ContextMenuStrip = ctxFile;
                else
                    listBox1.ContextMenuStrip = null;
        }

        private void ctxFile_Opening(object sender, CancelEventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
                e.Cancel = true;
            else
                saveToolStripMenuItem.Enabled = Program.OpenedFiles[listBox1.SelectedIndex].IsDirty;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
            {
                label1.Text = "";
                return;
            }

            string s = Path.GetFileName(listBox1.SelectedItem.ToString());
            label1.Text = String.Format("{0} - Has {1}changed", s, Program.OpenedFiles[listBox1.SelectedIndex].IsDirty ? "" : "not ");
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                listBox1.SelectedIndex = -1;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResourceNode r = Program.OpenedFiles[listBox1.SelectedIndex];
            r.Merge();
            r.Export(r._origPath);
            r.IsDirty = false;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
