using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public partial class ModelViewerForm : Form
    {
        public IMainWindow _mainWindow;
        public ModelViewerForm(IMainWindow mainWindow)
        {
            InitializeComponent();
            TopMost = true;
        }
    }
}
