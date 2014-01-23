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

        public List<Relocation> _relocations = new List<Relocation>();
        public void SetTarget(List<Relocation> relocations)
        {
            foreach (Relocation r in _relocations)
                r.Color = Color.Transparent;

            _relocations = relocations;
            if (_relocations.Count > 0)
                _baseOffset = (uint)_relocations[0]._index * 4;

            foreach (Relocation r in _relocations)
                r.Color = Color.FromArgb(255, 255, 200, 200);

            Display();
        }

        public PPCDisassembler() { InitializeComponent(); }

        public uint _baseOffset;

        public void Update(Relocation r)
        {
            if (r == null)
                return;
            int index = _relocations.IndexOf(r);
            if (index < 0)
                return;
            UpdateRow(index);
        }

        void UpdateRow(int i)
        {
            DataGridViewRow row = grdDisassembler.Rows[i];
            PPCOpCode opcode = _relocations[i].Code;

            row.Cells[0].Value = PPCFormat.Offset(_baseOffset + (uint)i * 4);
            row.Cells[1].Value = opcode.Name;
            row.Cells[2].Value = opcode.GetFormattedOperands();

            row.DefaultCellStyle.BackColor = _relocations[i]._section.GetStatusColorFromIndex(_relocations[i]._index);
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
                _editor.Position = _relocations[grdDisassembler.SelectedRows[0].Index]._index * 4;
                _updating = false;
            }
        }
    }
}
