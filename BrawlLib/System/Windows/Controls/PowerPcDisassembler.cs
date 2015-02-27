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
        public int _baseOffset, _sectionOffset;
        public List<PPCOpCode> _codes = new List<PPCOpCode>();

        public void SetTarget(RELMethodNode node)
        {
            if (node.ResourceType == ResourceType.RELExternalMethod)
            {
                _codes = null;
                _sectionOffset = 0;
                _baseOffset = 0;
                _manager = null;
            }
            else
            {
                _codes = node._codes;
                _sectionOffset = 0;
                _baseOffset = (int)node._cmd._addend;
                _manager = node._manager;
            }

            Display();
        }

        public void SetTarget(List<PPCOpCode> relocations, int sectionOffset, RelocationManager manager)
        {
            if (_codes != null)
            {
                int startIndex = _sectionOffset / 4;
                for (int i = 0; i < _codes.Count; i++)
                    _manager.ClearColor(startIndex + i);
            }

            _codes = relocations;
            _sectionOffset = sectionOffset;
            _manager = manager;

            if (_codes != null)
            {
                Color c = Color.FromArgb(255, 155, 200, 200);
                int startIndex = _sectionOffset / 4;
                for (int i = 0; i < _codes.Count; i++)
                    _manager.SetColor(startIndex + i, c);
            }

            Display();
        }

        public PPCDisassembler() { InitializeComponent(); }

        public void UpdateRow(int i)
        {
            if (_codes == null)
                return;

            DataGridViewRow row = grdDisassembler.Rows[i];
            PPCOpCode opcode = _codes[i];

            row.Cells[0].Value = PPCFormat.Offset(_baseOffset + _sectionOffset + i * 4);
            row.Cells[1].Value = opcode.Name;
            row.Cells[2].Value = opcode.GetFormattedOperands();

            row.DefaultCellStyle.BackColor = _manager.GetStatusColorFromIndex(_sectionOffset / 4 + i);
        }

        void Display()
        {
            grdDisassembler.Rows.Clear();
            if (_codes != null)
                for (int i = 0; i < _codes.Count; i++)
                {
                    grdDisassembler.Rows.Add();
                    UpdateRow(i);
                }
        }

        private void grdDisassembler_DoubleClick(object sender, EventArgs e)
        {
            new PPCOpCodeEditor().ShowDialog(_codes[grdDisassembler.SelectedRows[0].Index], null);
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
