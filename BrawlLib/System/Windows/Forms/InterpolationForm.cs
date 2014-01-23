using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Wii.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public partial class InterpolationForm : Form
    {
        public InterpolationEditor _interpolationEditor;
        public InterpolationForm(IMainWindow mainWindow)
        {
            InitializeComponent();
            _interpolationEditor = new InterpolationEditor(mainWindow);
            TopMost = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Controls.Add(_interpolationEditor);
            _interpolationEditor.Dock = DockStyle.Fill;
        }
    }
}
