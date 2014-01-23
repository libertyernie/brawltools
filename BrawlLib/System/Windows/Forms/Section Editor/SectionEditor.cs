using Be.Windows.Forms;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBBTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.PowerPcAssembly;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public unsafe partial class SectionEditor : Form
    {
        public ModuleSectionNode _section = null;

        public static List<SectionEditor> _openedSections = new List<SectionEditor>();

        public SectionEditor(ModuleSectionNode section)
        {
            InitializeComponent();

            ppcDisassembler1._editor = this;

            if ((_section = section) != null)
            {
                _section._linkedEditor = this;

                Relocation[] temp = new Relocation[_section._relocations.Count];
                _section._relocations.CopyTo(temp);
                _relocations = temp.ToList();
                _firstCommand = _section._firstCommand;
            }
            _openedSections.Add(this);

            Text = String.Format("Module Section Editor - {0}", _section.Name);

            hexBox1.SectionEditor = this;
            chkCodeSection.Checked = _section._isCodeSection;
            chkBSSSection.Checked = _section._isBSSSection;

            if (section.Root is RELNode)
            {
                RELNode r = (RELNode)section.Root;
                if (r._prologReloc != null && r._prologReloc._section == section)
                    _prologReloc = r._prologReloc;
                if (r._epilogReloc != null && r._epilogReloc._section == section)
                    _epilogReloc = r._epilogReloc;
                if (r._unresReloc != null && r._unresReloc._section == section)
                    _unresReloc = r._unresReloc;
                //if (r._nameReloc != null && r._nameReloc._section == section)
                //    _nameReloc = r._nameReloc;
            }

            panel5.Enabled = true;
        }

        //This editor serves as a temporary dynamic section.
        //When the editor is closed, the changes will then be applied to the section.
        public List<Relocation> _relocations;
        public RelCommand _firstCommand = null;

        protected override void OnClosed(EventArgs e)
        {
            Apply();

            _openedSections.Remove(this);
            if (_section != null)
                _section._linkedEditor = null;

            TargetRelocation = null;

 	        base.OnClosed(e);
        }

        void ByteProvider_LengthChanged(object sender, EventArgs e)
        {
            UpdateFileSizeStatus();
        }

        private void Init()
        {
            SetByteProvider();
            UpdateFileSizeStatus();

            //ppcDisassembler1.TargetNode = _section;
        }

        private void SetByteProvider()
        {
            if (hexBox1.ByteProvider != null)
                ((DynamicFileByteProvider)hexBox1.ByteProvider).Dispose();

            hexBox1.ByteProvider = new DynamicFileByteProvider(new UnmanagedMemoryStream((byte*)_section._dataBuffer.Address, _section._dataBuffer.Length, _section._dataBuffer.Length, FileAccess.ReadWrite)) { _supportsInsDel = false };
            hexBox1.ByteProvider.LengthChanged += ByteProvider_LengthChanged;
            hexBox1.InsertActiveChanged += hexBox1_InsertActiveChanged;
        }

        void hexBox1_InsertActiveChanged(object sender, EventArgs e)
        {
            insertValue.Text = hexBox1.InsertActive ? "Insert" : "Overwrite";
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Init();
        }

        void UpdateFileSizeStatus()
        {
            if (hexBox1.ByteProvider == null)
                fileSizeToolStripStatusLabel.Text = string.Empty;
            else
                fileSizeToolStripStatusLabel.Text = GetDisplayBytes(hexBox1.ByteProvider.Length);
        }

        string GetDisplayBytes(long size)
        {
            const long multi = 1024;
            long kb = multi;
            long mb = kb * multi;
            long gb = mb * multi;
            long tb = gb * multi;

            const string BYTES = "Bytes";
            const string KB = "KB";
            const string MB = "MB";
            const string GB = "GB";
            const string TB = "TB";

            string result;
            if (size < kb)
                result = string.Format("{0} {1}", size, BYTES);
            else if (size < mb)
                result = string.Format("{0} {1} ({2} Bytes)",
                    ConvertToOneDigit(size, kb), KB, ConvertBytesDisplay(size));
            else if (size < gb)
                result = string.Format("{0} {1} ({2} Bytes)",
                    ConvertToOneDigit(size, mb), MB, ConvertBytesDisplay(size));
            else if (size < tb)
                result = string.Format("{0} {1} ({2} Bytes)",
                    ConvertToOneDigit(size, gb), GB, ConvertBytesDisplay(size));
            else
                result = string.Format("{0} {1} ({2} Bytes)",
                    ConvertToOneDigit(size, tb), TB, ConvertBytesDisplay(size));

            return result;
        }

        string ConvertBytesDisplay(long size)
        {
            return size.ToString("###,###,###,###,###", CultureInfo.CurrentCulture);
        }

        string ConvertToOneDigit(long size, long quan)
        {
            double quotient = (double)size / (double)quan;
            string result = quotient.ToString("0.#", CultureInfo.CurrentCulture);
            return result;
        }

        private void hexBox1_Copied(object sender, EventArgs e)
        {
            EnableButtons();
        }

        private void hexBox1_CopiedHex(object sender, EventArgs e)
        {
            EnableButtons();
        }

        private void hexBox1_CurrentLineChanged(object sender, EventArgs e)
        {
            //PosChanged();
        }

        private void hexBox1_CurrentPositionInLineChanged(object sender, EventArgs e)
        {
            //PosChanged();
        }

        void PosChanged()
        {
            this.toolStripStatusLabel.Text = string.Format("Ln {0}    Col {1}",
                hexBox1.CurrentLine, hexBox1.CurrentPositionInLine);

            long offset = hexBox1.SelectionStart;
            long t = offset.RoundDown(4);

            _updating = true;

            TargetRelocation = (offset < hexBox1.ByteProvider.Length ? GetRelocationAtOffset((int)offset) : null);

            grpValue.Text = "Value @ 0x" + t.ToString("X");
            if (t + 3 < hexBox1.ByteProvider.Length)
            {
                grpValue.Enabled = !_section._isBSSSection;
                byte[] bytes = new byte[]
                {
                    //Read in little endian
                    hexBox1.ByteProvider.ReadByte(t + 3),
                    hexBox1.ByteProvider.ReadByte(t + 2),
                    hexBox1.ByteProvider.ReadByte(t + 1),
                    hexBox1.ByteProvider.ReadByte(t + 0),
                };

                //Reverse byte order to big endian
                txtByte1.Text = bytes[3].ToString("X2");
                txtByte2.Text = bytes[2].ToString("X2");
                txtByte3.Text = bytes[1].ToString("X2");
                txtByte4.Text = bytes[0].ToString("X2");

                //BitConverter converts from little endian
                float f = BitConverter.ToSingle(bytes, 0);
                float z;
                if (float.TryParse(txtFloat.Text, out z))
                {
                    if (z != f)
                        txtFloat.Text = f.ToString();
                }
                else
                    txtFloat.Text = f.ToString();

                int i = BitConverter.ToInt32(bytes, 0);
                int w;
                if (int.TryParse(txtInt.Text, out w))
                {
                    if (w != i)
                    {
                        txtInt.Text = i.ToString();
                        if (_section.HasCode && ppcDisassembler1.Visible)
                            ppcDisassembler1.Update(TargetRelocation);
                    }
                }
                else
                    txtInt.Text = i.ToString();

                string bin = ((Bin32)(uint)i).ToString();
                string[] bins = bin.Split(' ');

                txtBin1.Text = bins[0];
                txtBin2.Text = bins[1];
                txtBin3.Text = bins[2];
                txtBin4.Text = bins[3];
                txtBin5.Text = bins[4];
                txtBin6.Text = bins[5];
                txtBin7.Text = bins[6];
                txtBin8.Text = bins[7];
            }
            else
                grpValue.Enabled = false;

            OffsetToolStripStatusLabel.Text = String.Format("Offset: 0x{0}", offset.ToString("X"));

            if (_section.HasCode && ppcDisassembler1.Visible && TargetRelocation != null && !ppcDisassembler1._updating)
            {
                int i = ppcDisassembler1._relocations.IndexOf(TargetRelocation);
                if (i >= 0)
                {
                    ppcDisassembler1.grdDisassembler.ClearSelection();
                    ppcDisassembler1.grdDisassembler.Rows[i].Selected = true;
                    ppcDisassembler1.grdDisassembler.FirstDisplayedScrollingRowIndex = i;
                    //ppcDisassembler1.grdDisassembler.CurrentCell = ppcDisassembler1.grdDisassembler.Rows[i].Cells[0];
                }
            }

            _updating = false;
        }

        public long Position 
        {
            get { return hexBox1.SelectionStart; }
            set
            {
                if (hexBox1.SelectionStart == value)
                    PosChanged();
                else
                    hexBox1.SelectionStart = value;
            }
        }

        Relocation _targetRelocation;
        int _prev, _next;
        public Relocation TargetRelocation
        {
            get { return _targetRelocation; }
            set
            {
                if (_targetRelocation != null)
                    _targetRelocation._selected = false;

                if ((_targetRelocation = value) != null)
                {
                    _targetRelocation._selected = true;

                    lstLinked.DataSource = _targetRelocation.Linked;

                    if (_section.HasCode && ppcDisassembler1.Visible)
                    {
                        //Get the method that the cursor lies in and display it

                        Relocation r = value;
                        while (r.Previous != null && !(r.Previous.Code is PPCblr))
                            r = r.Previous;

                        int startIndex = r._index;

                        r = value;
                        while (!(r.Code is PPCblr) && r.Next != null)
                            r = r.Next;

                        int endIndex = r._index;

                        if (startIndex != _prev || endIndex != _next)
                        {
                            _prev = startIndex;
                            _next = endIndex;
                            List<Relocation> w = new List<Relocation>();
                            for (int i = startIndex; i <= endIndex; i++)
                                w.Add(_relocations[i]);

                            ppcDisassembler1.SetTarget(w);
                        }

                        bool u = _updating;
                        _updating = true;
                        chkConstructor.Checked = _targetRelocation._prolog;
                        chkDestructor.Checked = _targetRelocation._epilog;
                        chkUnresolved.Checked = _targetRelocation._unresolved;
                        _updating = u;
                    }
                }
                else
                    lstLinked.DataSource = null;

                CommandChanged();
            }
        }

        private void CommandChanged()
        {
            if (TargetRelocation == null)
            {
                propertyGrid1.SelectedObject = null;
                btnNewCmd.Enabled = false;
                btnDelCmd.Enabled = false;
                btnOpenTarget.Enabled = false;
                btnRemoveWord.Enabled = false;
            }
            else if ((propertyGrid1.SelectedObject = TargetRelocation.Command) != null)
            {
                btnNewCmd.Enabled = false;
                btnDelCmd.Enabled = true;
                btnOpenTarget.Enabled = TargetRelocation.Command.GetTargetRelocation() != null;
                btnRemoveWord.Enabled = true;
            }
            else
            {
                btnNewCmd.Enabled = true;
                btnDelCmd.Enabled = false;
                btnOpenTarget.Enabled = false;
                btnRemoveWord.Enabled = true;
            }

            hexBox1.Invalidate();
        }

        void EnableButtons()
        {
            copyToolStripMenuItem.Enabled = hexBox1.CanCopy();
            pasteOverwriteToolStripMenuItem.Enabled = hexBox1.CanPaste();
        }

        private void hexBox1_SelectionLengthChanged(object sender, EventArgs e)
        {
            EnableButtons();
        }

        private void hexBox1_SelectionStartChanged(object sender, EventArgs e)
        {
            EnableButtons();
            PosChanged();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hexBox1.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hexBox1.CopyHex();
        }

        private void pasteInsertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hexBox1.PasteHex(false);
        }

        private void pasteOverwriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hexBox1.PasteHex(true);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hexBox1.Delete();
        }

        internal FindOptions _findOptions = new FindOptions();
        FormFind _formFind = null;
        FormFind ShowFind()
        {
            if (_formFind == null || _formFind.IsDisposed)
            {
                _formFind = new FormFind(this);
                _formFind.Show(this);
            }
            else
                _formFind.Focus();
            
            return _formFind;
        }

        FormGoTo _formGoto = new FormGoTo();
        private void gotoToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            _formGoto.SetMaxByteIndex(hexBox1.ByteProvider.Length);
            _formGoto.SetDefaultValue(hexBox1.SelectionStart);
            if (_formGoto.ShowDialog() == DialogResult.OK)
            {
                hexBox1.SelectionStart = _formGoto.GetByteIndex();
                hexBox1.Focus();
            }
        }

        private void findToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowFind();
        }

        private void findNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Find(false);
        }

        private void findPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Find(true);
        }

        public void Find(bool backwards)
        {
            if (!_findOptions.IsValid)
                return;

            long res = backwards ? hexBox1.FindPrev(_findOptions) : hexBox1.FindNext(_findOptions);

            if (res == -1) // -1 = no match
                MessageBox.Show("Unable to find a match.", "Find", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (res == -2) // -2 = find was aborted
                return;
            else if (!hexBox1.Focused)
                hexBox1.Focus();

            Message m = new Message() { WParam = (IntPtr)(16 | 65536) };
            hexBox1._ki.PreProcessWmKeyUp(ref m);
        }

        private void chkBSSSection_CheckedChanged(object sender, EventArgs e)
        {
            bool isBSS = chkBSSSection.Checked;

            if (_updating)
            {
                menuStrip1.Enabled = !isBSS;
                grpValue.Enabled = !isBSS;
                hexBox1.ReadOnly = isBSS;
                return;
            }

            //There can only be one BSS section
            foreach (ModuleSectionNode s in ((ModuleNode)_section.Root).Sections)
            {
                if (s != _section)
                {
                    if (isBSS)
                    {
                        //Turn off any other BSS section
                        if (s._linkedEditor != null)
                        {
                            s._linkedEditor._updating = true;
                            s._linkedEditor.chkBSSSection.Checked = false;
                            s._linkedEditor._updating = false;
                        }
                        else
                            s._isBSSSection = false;
                    }
                    else
                    {
                        //Make sure there is another BSS section
                        bool found = false;
                        if (s._linkedEditor != null)
                        {
                            if (s._linkedEditor.chkBSSSection.Checked)
                            {
                                found = true;
                                break;
                            }

                        }
                        else
                            if (s._isBSSSection)
                            {
                                found = true;
                                break;
                            }
                        if (!found)
                            return;
                    }
                }
            }

            menuStrip1.Enabled = !isBSS;
            grpValue.Enabled = !isBSS;
            hexBox1.ReadOnly = isBSS;
        }

        private void chkCodeSection_CheckedChanged(object sender, EventArgs e)
        {
            pnlHexEditor.Dock = chkCodeSection.Checked ? DockStyle.Right : DockStyle.Fill;
            pnlFunctions.Visible = ppcDisassembler1.Visible = splitter2.Visible = chkCodeSection.Checked;
            txtFloat.Enabled = txtInt.Enabled = !chkCodeSection.Checked;

            if (chkCodeSection.Checked)
            {
                pnlHexEditor.Width = 500;
                Width = 972;
                displayStringsToolStripMenuItem.Checked = false;
            }
            else
            {
                Width = 840;
                displayStringsToolStripMenuItem.Checked = true;
            }

            if (ppcDisassembler1.Visible)
                foreach (Relocation r in ppcDisassembler1._relocations)
                    r.Color = Color.FromArgb(255, 255, 200, 200);
            else
                foreach (Relocation r in ppcDisassembler1._relocations)
                    r.Color = Color.Transparent;
            
            hexBox1.Invalidate();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog d = new SaveFileDialog())
            {
                d.Filter = "Raw Data (*.*)|*.*";
                d.FileName = _section.Name;
                d.Title = "Choose a place to export to.";
                if (d.ShowDialog() == Forms.DialogResult.OK)
                    _section.Export(d.FileName);
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog d = new OpenFileDialog())
            {
                d.Filter = "Raw Data (*.*)|*.*";
                if (d.ShowDialog() == Forms.DialogResult.OK)
                {
                    _section.Replace(d.FileName);
                    Init();
                }
            }
        }

        private void exportInitializedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog d = new SaveFileDialog())
            {
                d.Filter = "Raw Data (*.*)|*.*";
                d.FileName = _section.Name;
                if (d.ShowDialog() == Forms.DialogResult.OK)
                    _section.ExportInitialized(d.FileName);
            }
        }

        private void btnNewCmd_Click(object sender, EventArgs e)
        {
            if (TargetRelocation == null || TargetRelocation.Command != null)
                return;

            TargetRelocation.Command = new RelCommand((TargetRelocation._section.Root as ModuleNode).ID, TargetRelocation._section.Index, new RELLink());

            CommandChanged();
            hexBox1.Focus();
            _relocationsChanged = true;
        }

        private void btnDelCmd_Click(object sender, EventArgs e)
        {
            if (TargetRelocation == null || TargetRelocation.Command == null)
                return;

            TargetRelocation.Command = null;

            CommandChanged();
            hexBox1.Focus();
            _relocationsChanged = true;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            propertyGrid1.Refresh();
            hexBox1.Invalidate();
            _relocationsChanged = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //pnlHexEditor.Visible = radioButton1.Checked;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            //ppcDisassembler1.Visible = radioButton2.Checked;
        }

        private void txtSize_TextChanged(object sender, EventArgs e)
        {

        }

        public void OpenRelocation(Relocation target)
        {
            if (target == null)
                return;

            if (target._section != _section)
            {
                foreach (SectionEditor r in _openedSections)
                    if (r._section == target._section)
                    {
                        r.Position = target._index * 4;
                        r.Focus();
                        r.hexBox1.Focus();
                        return;
                    }

                SectionEditor x = new SectionEditor(target._section as ModuleSectionNode);
                x.Show();

                x.Position = target._index * 4;
                x.hexBox1.Focus();
            }
            else
            {
                Position = target._index * 4;
                hexBox1.Focus();
            }
        }

        private void btnOpenTarget_Click(object sender, EventArgs e)
        {
            if (TargetRelocation == null || TargetRelocation.Command == null)
                return;

            OpenRelocation(TargetRelocation.Command.GetTargetRelocation());
        }

        private void lstLinked_DoubleClick(object sender, EventArgs e)
        {
            if (lstLinked.SelectedItem != null)
                OpenRelocation(lstLinked.SelectedItem as Relocation);
        }

        bool _updating = false;
        private void txtFloat_TextChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            float f;
            if (float.TryParse(txtFloat.Text, out f))
            {
                long t = Position.RoundDown(4);
                byte* b = (byte*)&f;
                hexBox1.ByteProvider.WriteByte(t + 3, b[0]);
                hexBox1.ByteProvider.WriteByte(t + 2, b[1]);
                hexBox1.ByteProvider.WriteByte(t + 1, b[2]);
                hexBox1.ByteProvider.WriteByte(t + 0, b[3]);
                hexBox1.Invalidate();
                PosChanged();
            }
        }

        private void txtInt_TextChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            int i;
            if (int.TryParse(txtInt.Text, out i))
            {
                long t = Position.RoundDown(4);
                byte* b = (byte*)&i;
                hexBox1.ByteProvider.WriteByte(t + 3, b[0]);
                hexBox1.ByteProvider.WriteByte(t + 2, b[1]);
                hexBox1.ByteProvider.WriteByte(t + 1, b[2]);
                hexBox1.ByteProvider.WriteByte(t + 0, b[3]);
                hexBox1.Invalidate();
                PosChanged();
            }
        }

        private void txtBin1_TextChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            string text = (sender as TextBox).Text;
            if (text.Length != 4)
                return;

            foreach (char s in text)
                if (s != '0' && s != '1')
                    return;

            Bin32 b = Bin32.FromString(txtBin1.Text + " " + txtBin2.Text + " " + txtBin3.Text + " " + txtBin4.Text + " " + txtBin5.Text + " " + txtBin6.Text + " " + txtBin7.Text + " " + txtBin8.Text);
            long t = Position.RoundDown(4);

            byte
                b1 = (byte)((b >> 00) & 0xFF),
                b2 = (byte)((b >> 08) & 0xFF),
                b3 = (byte)((b >> 16) & 0xFF),
                b4 = (byte)((b >> 24) & 0xFF);

            txtByte1.Text = b1.ToString();
            txtByte2.Text = b2.ToString();
            txtByte3.Text = b3.ToString();
            txtByte4.Text = b4.ToString();

            hexBox1.ByteProvider.WriteByte(t + 3, b1);
            hexBox1.ByteProvider.WriteByte(t + 2, b2);
            hexBox1.ByteProvider.WriteByte(t + 1, b3);
            hexBox1.ByteProvider.WriteByte(t + 0, b4);

            hexBox1.Invalidate();
            PosChanged();
        }

        public bool _relocationsChanged = false;
        private void Apply()
        {
            if (hexBox1.ByteProvider == null)
                return;

            try
            {
                if (_section._isBSSSection != chkBSSSection.Checked || _section._isCodeSection != chkCodeSection.Checked)
                {
                    _section._isBSSSection = chkBSSSection.Checked;
                    _section._isCodeSection = chkCodeSection.Checked;
                    _section.SignalPropertyChange();
                }

                if (_section.Root is RELNode)
                {
                    RELNode r = _section.Root as RELNode;

                    if (r._prologReloc != _prologReloc && _prologReloc != null)
                    {
                        if (r._prologReloc != null)
                            r._prologReloc._prolog = false;

                        r._prologReloc = _prologReloc;
                        r.SignalPropertyChange();
                    }

                    if (r._epilogReloc != _epilogReloc && _epilogReloc != null)
                    {
                        if (r._epilogReloc != null)
                            r._epilogReloc._epilog = false;

                        r._epilogReloc = _epilogReloc;
                        r.SignalPropertyChange();
                    }

                    if (r._unresReloc != _unresReloc && _unresReloc != null)
                    {
                        if (r._unresReloc != null)
                            r._unresReloc._unresolved = false;

                        r._unresReloc = _unresReloc;
                        r.SignalPropertyChange();
                    }
                }

                DynamicFileByteProvider d = hexBox1.ByteProvider as DynamicFileByteProvider;
                if (!d.HasChanges())
                    return;

                UnsafeBuffer newBuffer = new UnsafeBuffer((int)d.Length);

                int amt = Math.Min(_section._dataBuffer.Length, newBuffer.Length);
                if (amt > 0)
                {
                    Memory.Move(newBuffer.Address, _section._dataBuffer.Address, (uint)amt);
                    if (newBuffer.Length - amt > 0)
                        Memory.Fill(newBuffer.Address + amt, (uint)(newBuffer.Length - amt), 0);
                }

                if (d._stream != null)
                    d._stream.Dispose();
                d._stream = new UnmanagedMemoryStream((byte*)newBuffer.Address, newBuffer.Length, newBuffer.Length, FileAccess.ReadWrite);

                d.ApplyChanges();

                _section._dataBuffer.Dispose();
                _section._dataBuffer = newBuffer;
                _section.SignalPropertyChange();

                if (_relocationsChanged)
                {
                    Relocation[] temp = new Relocation[_relocations.Count];
                    _relocations.CopyTo(temp);
                    List<Relocation> temp2 = temp.ToList();

                    _section._relocations = temp2;
                    _section._firstCommand = _firstCommand;

                    ResourceNode a = _section.Root;
                    if (a != null && a != _section.Root)
                        a.SignalPropertyChange();
                }

                hexBox1.Invalidate();
                hexBox1.Focus();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                EnableButtons();
            }
        }

        private void btnInsertWord_Click(object sender, EventArgs e)
        {
            long offset = Position.RoundDown(4);
            long index = offset / 4;

            DynamicFileByteProvider d = hexBox1.ByteProvider as DynamicFileByteProvider;
            d._supportsInsDel = true;
            d.InsertBytes(offset, new byte[] { 0, 0, 0, 0 });
            d._supportsInsDel = false;

            if (index == _relocations.Count)
                _relocations.Add(new Relocation(_section, (int)index));
            else
            {
                _relocations.Insert((int)index, new Relocation(_section, (int)index));
                foreach (ModuleDataNode s in ((ModuleNode)_section.Root).Sections)
                    foreach (Relocation r in s.Relocations)
                        FixRelocation(r, 1, offset);
            }

            for (int i = (int)index + 1; i < _relocations.Count; i++)
                _relocations[i]._index++;

            PosChanged();
        }

        private void FixRelocation(Relocation w, int amt, long offset)
        {
            foreach (Relocation l in w.Linked)
                if (l.Command != null)
                    if (l.Command.TargetSectionID == _section.Index && l.Command._addend >= offset)
                        l.Command._addend = (uint)((int)l.Command._addend + amt * 4);
        }

        private void btnRemoveWord_Click(object sender, EventArgs e)
        {
            long offset = Position.RoundDown(4);
            long index = offset / 4;

            DynamicFileByteProvider d = hexBox1.ByteProvider as DynamicFileByteProvider;
            d._supportsInsDel = true;
            d.DeleteBytes(offset, 4);
            d._supportsInsDel = false;

            _relocations.RemoveAt((int)index);
            foreach (ModuleDataNode s in ((ModuleNode)_section.Root).Sections)
                foreach (Relocation r in s.Relocations)
                    FixRelocation(r, -1, offset);

            for (int i = (int)index; i < _relocations.Count; i++)
                _relocations[i]._index--;

            PosChanged();
        }

        #region Command Functions

        public void ClearCommands()
        {
            RelCommand c = _firstCommand;
            while (c != null)
            {
                c._parentRelocation.Command = null;
                c = c._next;
            }
        }

        public void GetFirstCommand() { _firstCommand = GetCommandAfter(-1); }

        public RelCommand GetCommandAfter(int startIndex)
        {
            int i = GetIndexOfCommandAfter(startIndex);
            if (i >= 0 && i < _relocations.Count)
                return _relocations[i].Command;
            return null;
        }

        public RelCommand GetCommandBefore(int startIndex)
        {
            int i = GetIndexOfCommandBefore(startIndex);
            if (i >= 0 && i < _relocations.Count)
                return _relocations[i].Command;
            return null;
        }

        public int GetIndexOfCommandBefore(int startIndex)
        {
            if (startIndex < 0)
                return -1;

            if (startIndex > _relocations.Count)
                startIndex = _relocations.Count;

            for (int i = startIndex - 1; i >= 0; i--)
                if (i < _relocations.Count && _relocations[i].Command != null)
                    return i;

            return -1;
        }

        public int GetIndexOfCommandAfter(int startIndex)
        {
            if (startIndex >= _relocations.Count || startIndex < -1)
                return -1;

            for (int i = startIndex + 1; i < _relocations.Count - 1; i++)
                if (i < _relocations.Count && _relocations[i].Command != null)
                    return i;

            return -1;
        }

        public RelCommand GetCommandFromOffset(int offset) { return GetCommandFromIndex(offset.RoundDown(4) / 4); }
        public RelCommand GetCommandFromIndex(int index) { if (index < _relocations.Count && index >= 0) return _relocations[index].Command; return null; }

        public void SetCommandAtOffset(int offset, RelCommand cmd) { SetCommandAtIndex(offset.RoundDown(4) / 4, cmd); }
        public void SetCommandAtIndex(int index, RelCommand cmd)
        {
            if (index >= _relocations.Count || index < 0)
                return;

            if (_relocations[index].Command != null)
                _relocations[index].Command.Remove();

            _relocations[index].Command = cmd;

            RelCommand c = GetCommandBefore(index);
            if (c != null)
                cmd.InsertAfter(c);
            else
            {
                c = GetCommandAfter(index);
                if (c != null)
                    cmd.InsertBefore(c);
            }
            GetFirstCommand();
        }

        #endregion

        #region Relocation Functions

        public Relocation GetRelocationAtOffset(int offset) { return GetRelocationAtIndex(offset.RoundDown(4) / 4); }
        public Relocation GetRelocationAtIndex(int index) { return _relocations[index]; }

        public void SetRelocationAtOffset(int offset, Relocation value) { SetRelocationAtOffset(offset.RoundDown(4) / 4, value); }
        public void SetRelocationAtIndex(int index, Relocation value) { _relocations[index] = value; }
        
        public Color GetStatusColorFromOffset(int offset) { return GetStatusColorFromIndex(offset.RoundDown(4) / 4); }
        public Color GetStatusColorFromIndex(int index) { return GetStatusColor(_relocations[index]); }
        public Color GetStatusColor(Relocation c)
        {
            if (c.Code is PPCblr)
                return ModuleDataNode.clrBlr;
            if (c.Command == null)
                return ModuleDataNode.clrNotRelocated;
            return ModuleDataNode.clrRelocated;
        }

        #endregion

        private void highlightBlr_CheckedChanged(object sender, EventArgs e)
        {
            hexBox1.Invalidate();
        }

        private void displayInitialized_CheckedChanged(object sender, EventArgs e)
        {
            hexBox1.Invalidate();
        }

        private void displayStringsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            hexBox1.StringViewVisible = displayStringsToolStripMenuItem.Checked;
        }

        public Relocation _prologReloc = null, _epilogReloc = null, _unresReloc = null, _nameReloc = null;
        private void chkConstructor_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating || TargetRelocation == null)
                return;

            if (TargetRelocation == _prologReloc)
            {
                if (!(_prologReloc._prolog = chkConstructor.Checked))
                    _prologReloc = null;
            }
            else
            {
                if (chkConstructor.Checked)
                {
                    if (_prologReloc != null)
                        _prologReloc._prolog = false;
                    _prologReloc = TargetRelocation;
                    _prologReloc._prolog = true;
                }
            }
        }

        private void chkDestructor_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating || TargetRelocation == null)
                return;

            if (TargetRelocation == _epilogReloc)
            {
                if (!(_epilogReloc._epilog = chkDestructor.Checked))
                    _epilogReloc = null;
            }
            else
            {
                if (chkDestructor.Checked)
                {
                    if (_epilogReloc != null)
                        _epilogReloc._epilog = false;
                    _epilogReloc = TargetRelocation;
                    _epilogReloc._epilog = true;
                }
            }
        }

        private void chkUnresolved_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating || TargetRelocation == null)
                return;

            if (TargetRelocation == _unresReloc)
            {
                if (!(_unresReloc._unresolved = chkUnresolved.Checked))
                    _unresReloc = null;
            }
            else
            {
                if (chkUnresolved.Checked)
                {
                    if (_unresReloc != null)
                        _unresReloc._unresolved = false;
                    _unresReloc = TargetRelocation;
                    _unresReloc._unresolved = true;
                }
            }
        }
    }
}
