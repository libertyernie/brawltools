using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.PowerPcAssembly;
using System.Text;
using System.Windows.Forms;

namespace System.Windows.Controls
{
    public partial class PPCOpCodeEditor : Form
    {
        uint oldValue;
        Relocation _targetRelocation;
        SectionEditor _mainWindow;
        PPCOpCode _code;
        public PPCOpCodeEditor()
        {
            InitializeComponent();
        }

        public DialogResult ShowDialog(Relocation relocation, SectionEditor mainWindow)
        {
            _mainWindow = mainWindow;
            _targetRelocation = relocation;
            oldValue = _targetRelocation.RawValue;
            propertyGrid1.SelectedObject = _code = _targetRelocation.Code;
            label3.Text = "0x" + (_targetRelocation._index * 4).ToString("X");
            return base.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = Forms.DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _targetRelocation.RawValue = oldValue;
            _mainWindow.hexBox1.Invalidate();
            DialogResult = Forms.DialogResult.Cancel;
            Close();
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            _mainWindow.Position = _mainWindow.Position;
            _mainWindow.hexBox1.Invalidate();
            _targetRelocation.RawValue = _code;
        }
    }
}
