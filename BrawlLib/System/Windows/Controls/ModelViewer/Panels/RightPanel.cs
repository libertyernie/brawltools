using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public partial class RightPanel : UserControl
    {
        public RightPanel()
        {
            InitializeComponent();
            editor.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlBones.Visible = editor.SelectedIndex == 0;
            pnlKeyframes.Visible = editor.SelectedIndex == 1;
            pnlOpenedFiles.Visible = editor.SelectedIndex == 2;
        }

        public void Reset() { pnlBones.Reset(); }
    }
}
