using System;
using System.Drawing;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Imaging;
using System.ComponentModel;

namespace System.Windows.Forms
{
    public class CLRControl : UserControl
    {
        #region Designer

        private Label lblPrimary;
        private Label lblBase;
        private Label lblColor;
        private ContextMenuStrip ctxMenu;
        private IContainer components;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private Panel pnlPrimary;
        private Label lblCNoA;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem editToolStripMenuItem;
        private ListBox lstColors;
    
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblPrimary = new System.Windows.Forms.Label();
            this.lstColors = new System.Windows.Forms.ListBox();
            this.ctxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblBase = new System.Windows.Forms.Label();
            this.lblColor = new System.Windows.Forms.Label();
            this.pnlPrimary = new System.Windows.Forms.Panel();
            this.lblCNoA = new System.Windows.Forms.Label();
            this.ctxMenu.SuspendLayout();
            this.pnlPrimary.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPrimary
            // 
            this.lblPrimary.Location = new System.Drawing.Point(5, 2);
            this.lblPrimary.Name = "lblPrimary";
            this.lblPrimary.Size = new System.Drawing.Size(61, 20);
            this.lblPrimary.TabIndex = 0;
            this.lblPrimary.Text = "Base Color:";
            this.lblPrimary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lstColors
            // 
            this.lstColors.ContextMenuStrip = this.ctxMenu;
            this.lstColors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstColors.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstColors.FormattingEnabled = true;
            this.lstColors.IntegralHeight = false;
            this.lstColors.Location = new System.Drawing.Point(0, 24);
            this.lstColors.Name = "lstColors";
            this.lstColors.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstColors.Size = new System.Drawing.Size(334, 218);
            this.lstColors.TabIndex = 1;
            this.lstColors.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstColors_DrawItem);
            this.lstColors.DoubleClick += new System.EventHandler(this.lstColors_DoubleClick);
            this.lstColors.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstColors_KeyDown);
            this.lstColors.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstColors_MouseDown);
            // 
            // ctxMenu
            // 
            this.ctxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripMenuItem1,
            this.editToolStripMenuItem});
            this.ctxMenu.Name = "ctxMenu";
            this.ctxMenu.Size = new System.Drawing.Size(145, 76);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(141, 6);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.editToolStripMenuItem.Text = "Edit...";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // lblBase
            // 
            this.lblBase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBase.BackColor = System.Drawing.Color.Transparent;
            this.lblBase.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBase.Location = new System.Drawing.Point(72, 2);
            this.lblBase.Name = "lblBase";
            this.lblBase.Size = new System.Drawing.Size(149, 20);
            this.lblBase.TabIndex = 2;
            this.lblBase.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblBase.Click += new System.EventHandler(this.lblBase_Click);
            // 
            // lblColor
            // 
            this.lblColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblColor.Location = new System.Drawing.Point(231, 5);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(41, 14);
            this.lblColor.TabIndex = 3;
            this.lblColor.Click += new System.EventHandler(this.lblBase_Click);
            // 
            // pnlPrimary
            // 
            this.pnlPrimary.BackColor = System.Drawing.Color.White;
            this.pnlPrimary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPrimary.Controls.Add(this.lblColor);
            this.pnlPrimary.Controls.Add(this.lblCNoA);
            this.pnlPrimary.Controls.Add(this.lblPrimary);
            this.pnlPrimary.Controls.Add(this.lblBase);
            this.pnlPrimary.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPrimary.Location = new System.Drawing.Point(0, 0);
            this.pnlPrimary.Name = "pnlPrimary";
            this.pnlPrimary.Size = new System.Drawing.Size(334, 24);
            this.pnlPrimary.TabIndex = 4;
            // 
            // lblCNoA
            // 
            this.lblCNoA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCNoA.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblCNoA.Location = new System.Drawing.Point(271, 5);
            this.lblCNoA.Name = "lblCNoA";
            this.lblCNoA.Size = new System.Drawing.Size(41, 14);
            this.lblCNoA.TabIndex = 4;
            this.lblCNoA.Click += new System.EventHandler(this.lblBase_Click);
            // 
            // CLRControl
            // 
            this.Controls.Add(this.lstColors);
            this.Controls.Add(this.pnlPrimary);
            this.DoubleBuffered = true;
            this.Name = "CLRControl";
            this.Size = new System.Drawing.Size(334, 242);
            this.ctxMenu.ResumeLayout(false);
            this.pnlPrimary.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ARGBPixel _primaryColor;
        private ARGBPixel _copyColor;

        public int _colorId = 0;
        public int ColorID { get { return _colorId; } set { _colorId = value; SourceChanged(); } }

        private IColorSource _colorSource;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IColorSource ColorSource
        {
            get { return _colorSource; }
            set { _colorSource = value; SourceChanged(); }
        }

        private GoodColorDialog _dlgColor;
        private GradientDialog _dlgGradient;

        public CLRControl() 
        { 
            InitializeComponent();
            _dlgColor = new GoodColorDialog();
            _dlgGradient = new GradientDialog();
        }

        private void SourceChanged()
        {
            lstColors.BeginUpdate();
            lstColors.Items.Clear();

            if (_colorSource != null)
            {
                int count = _colorSource.ColorCount(_colorId);
                for (int i = 0; i < count; i++)
                    lstColors.Items.Add(_colorSource.GetColor(i, _colorId));

                if (pnlPrimary.Visible = _colorSource.HasPrimary(_colorId))
                {
                    _primaryColor = _colorSource.GetPrimaryColor(_colorId);
                    lblPrimary.Text = _colorSource.PrimaryColorName(_colorId);
                    UpdateBase();
                }
            }

            lstColors.EndUpdate();
        }

        private void UpdateBase()
        {
            lblBase.Text = _primaryColor.ToString();
            lblColor.BackColor = (Color)_primaryColor;
            lblCNoA.BackColor = Color.FromArgb(_primaryColor.R, _primaryColor.G, _primaryColor.B);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e) { lstColors_DoubleClick(sender, e); }
        private void lstColors_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                lstColors_DoubleClick(sender, e);
        }
        private void lstColors_DoubleClick(object sender, EventArgs e)
        {
            ListBox.SelectedIndexCollection indices = lstColors.SelectedIndices;
            if ((_colorSource == null) || (indices.Count <= 0))
                return;

            int count = indices.Count;
            if (count == 1)
            {
                int index = indices[0];
                _dlgColor.Color = (Color)(ARGBPixel)lstColors.Items[index];
                if (_dlgColor.ShowDialog(this) == DialogResult.OK)
                {
                    ARGBPixel p = (ARGBPixel)_dlgColor.Color;
                    lstColors.Items[index] = p;
                    _colorSource.SetColor(index, _colorId, p);
                }
            }
            else
            {
                //Sort indices
                int[] sorted = new int[count];
                indices.CopyTo(sorted, 0);
                Array.Sort(sorted);

                _dlgGradient.StartColor = (Color)(ARGBPixel)lstColors.Items[sorted[0]];
                _dlgGradient.EndColor = (Color)(ARGBPixel)lstColors.Items[sorted[count - 1]];
                if (_dlgGradient.ShowDialog(this) == DialogResult.OK)
                {
                    //Interpolate and apply to each in succession.
                    ARGBPixel start = (ARGBPixel)_dlgGradient.StartColor;
                    ARGBPixel end = (ARGBPixel)_dlgGradient.EndColor;
                    float stepA = (end.A - start.A) / (float)count;
                    float stepR = (end.R - start.R) / (float)count;
                    float stepG = (end.G - start.G) / (float)count;
                    float stepB = (end.B - start.B) / (float)count;
                    for (int i = 0; i < count; i++)
                    {
                        ARGBPixel p = new ARGBPixel(
                            (byte)(start.A + (i * stepA)),
                            (byte)(start.R + (i * stepR)),
                            (byte)(start.G + (i * stepG)),
                            (byte)(start.B + (i * stepB)));
                        lstColors.Items[sorted[i]] = p;
                        _colorSource.SetColor(sorted[i], _colorId, p);
                    }
                }
            }
        }

        private static Font _renderFont = new Font(FontFamily.GenericMonospace, 9.0f);
        private void lstColors_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle r = e.Bounds;
            int index = e.Index;

            g.FillRectangle(Brushes.White, r);

            if (index >= 0)
            {
                ARGBPixel p = (ARGBPixel)lstColors.Items[index];

                if ((e.State & DrawItemState.Selected) != 0)
                    g.FillRectangle(Brushes.LightBlue, r.X, r.Y, 230, r.Height);

                double n = Math.Floor(Math.Log10(_colorSource.ColorCount(_colorId)) + 1);
                g.DrawString(String.Format("[{0}]  -  {1}", index.ToString().PadLeft((int)n, ' '), p), _renderFont, Brushes.Black, 4.0f, e.Bounds.Y - 2);

                r.X += 250;
                r.Width = 40;

                using (Brush b = new SolidBrush((Color)p))
                    g.FillRectangle(b, r);
                g.DrawRectangle(Pens.Black, r);

                p.A = 255;
                r.X += 40;
                using (Brush b = new SolidBrush((Color)p))
                    g.FillRectangle(b, r);
                g.DrawRectangle(Pens.Black, r);
            }
        }

        private void lblBase_Click(object sender, EventArgs e)
        {
            if (_colorSource == null)
                return;

            _dlgColor.Color = (Color)_primaryColor;
            if (_dlgColor.ShowDialog(this) == DialogResult.OK)
            {
                _primaryColor = (ARGBPixel)_dlgColor.Color;
                _colorSource.SetPrimaryColor(_colorId, _primaryColor);
                UpdateBase();
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstColors.SelectedIndex >= 0)
                _copyColor = (ARGBPixel)lstColors.SelectedItem;
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = lstColors.SelectedIndex;
            if (index >= 0)
            {
                lstColors.Items[index] = _copyColor;
                _colorSource.SetColor(index, _colorId, _copyColor);
                //lstColors.Invalidate();
            }
        }

        private void lstColors_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = lstColors.IndexFromPoint(e.Location);
                lstColors.SelectedIndex = index;
            }
        }
    }
}
