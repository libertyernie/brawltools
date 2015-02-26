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

        internal RelocationManager _manager;
        public int _sectionOffset;
        public List<PPCOpCode> _relocations = new List<PPCOpCode>();

        public void SetTarget(RELMethodNode node)
        {
            if (node.ResourceType == ResourceType.RELExternalMethod)
            {
                _relocations = null;
                _sectionOffset = 0;
                return;
            }

            _relocations = node._codes;
            _sectionOffset = 0;
            _manager = node._manager;

            Display();
        }

        public void SetTarget(List<PPCOpCode> relocations, int baseOffset, RelocationManager manager)
        {
            if (_relocations != null)
            {
                int startIndex = _sectionOffset / 4;
                for (int i = 0; i < _relocations.Count; i++)
                    _manager.ClearColor(startIndex + i);
            }

            _relocations = relocations;
            _sectionOffset = baseOffset;
            _manager = manager;

            if (_relocations != null)
            {
                Color c = Color.FromArgb(255, 255, 200, 200);
                int startIndex = _sectionOffset / 4;
                for (int i = 0; i < _relocations.Count; i++)
                    _manager.SetColor(startIndex + i, c);
            }

            Display();
        }

        public PPCDisassembler() { InitializeComponent(); }

        public void UpdateRow(int i)
        {
            if (_relocations == null)
                return;

            DataGridViewRow row = grdDisassembler.Rows[i];
            PPCOpCode opcode = _relocations[i];

            row.Cells[0].Value = PPCFormat.Offset(_sectionOffset + i * 4);
            row.Cells[1].Value = opcode.Name;
            row.Cells[2].Value = opcode.GetFormattedOperands();

            row.DefaultCellStyle.BackColor = _manager.GetStatusColorFromIndex(_sectionOffset / 4 + i);
        }

        void Display()
        {
            grdDisassembler.Rows.Clear();
            if (_relocations != null)
                for (int i = 0; i < _relocations.Count; i++)
                {
                    grdDisassembler.Rows.Add();
                    UpdateRow(i);
                }
        }

        private void grdDisassembler_DoubleClick(object sender, EventArgs e)
        {
            new PPCOpCodeEditor().ShowDialog(_relocations[grdDisassembler.SelectedRows[0].Index], null);
        }

        public bool _updating = false;
        private void grdDisassembler_SelectionChanged(object sender, EventArgs e)
        {

        }

        public SectionEditor _editor;

        private void grdDisassembler_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (_updating || _editor == null || grdDisassembler.SelectedRows.Count == 0)
                return;

            _updating = true;
            _editor.Position = _sectionOffset + grdDisassembler.SelectedRows[0].Index * 4;
            _updating = false;
        }
    }
}
