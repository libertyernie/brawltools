using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrawlLib.SSBB.ResourceNodes;
using System.PowerPcAssembly;
using System.Windows.Controls;

namespace System.Windows.Forms
{
    public partial class PPCDisassembler : UserControl
    {
        internal SectionEditor _editor;
        public int _sectionOffset;
        public List<PPCOpCode> _relocations = new List<PPCOpCode>();

        public void SetTarget(List<PPCOpCode> relocations, int baseOffset)
        {
            //foreach (Relocation r in _relocations)
            //    r.Color = Color.Transparent;

            _relocations = relocations;
            _sectionOffset = baseOffset;

            //foreach (Relocation r in _relocations)
            //    r.Color = Color.FromArgb(255, 255, 200, 200);

            Display();
        }

        public PPCDisassembler() { InitializeComponent(); }

        public void UpdateRow(int i)
        {
            DataGridViewRow row = grdDisassembler.Rows[i];
            PPCOpCode opcode = _relocations[i];

            row.Cells[0].Value = PPCFormat.Offset(_sectionOffset + i * 4);
            row.Cells[1].Value = opcode.Name;
            row.Cells[2].Value = opcode.GetFormattedOperands();

            row.DefaultCellStyle.BackColor = _editor._section._manager.GetStatusColorFromIndex(_sectionOffset / 4 + i);
        }

        void Display()
        {
            grdDisassembler.Rows.Clear();
            for (int i = 0; i < _relocations.Count; i++)
            {
                grdDisassembler.Rows.Add();
                UpdateRow(i);
            }
        }

        private void grdDisassembler_DoubleClick(object sender, EventArgs e)
        {
            new PPCOpCodeEditor().ShowDialog(_relocations[grdDisassembler.SelectedRows[0].Index], _editor);
        }

        public bool _updating = false;
        private void grdDisassembler_SelectionChanged(object sender, EventArgs e)
        {

        }

        private void grdDisassembler_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (_updating)
                return;

            if (grdDisassembler.SelectedRows.Count > 0)
            {
                _updating = true;
                _editor.Position = _sectionOffset + grdDisassembler.SelectedRows[0].Index * 4;
                _updating = false;
            }
        }
    }
}
