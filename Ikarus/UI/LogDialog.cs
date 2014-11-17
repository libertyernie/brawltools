using Ikarus.ModelViewer;
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

namespace System.Windows.Forms
{
    public partial class LogDialog : Form
    {
        public LogDialog()
        {
            InitializeComponent();
            listBox1.DataSource = RunTime._log;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RunTime.ClearLog();
        }
    }
}
