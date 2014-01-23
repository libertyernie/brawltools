namespace System.Windows.Forms
{
    partial class PPCDisassembler
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.grdDisassembler = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.grdDisassembler)).BeginInit();
            this.SuspendLayout();
            // 
            // grdDisassembler
            // 
            this.grdDisassembler.AllowUserToAddRows = false;
            this.grdDisassembler.AllowUserToDeleteRows = false;
            this.grdDisassembler.AllowUserToResizeRows = false;
            this.grdDisassembler.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.grdDisassembler.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.grdDisassembler.BackgroundColor = System.Drawing.Color.White;
            this.grdDisassembler.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grdDisassembler.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.grdDisassembler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdDisassembler.ColumnHeadersVisible = false;
            this.grdDisassembler.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.NullValue = null;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grdDisassembler.DefaultCellStyle = dataGridViewCellStyle1;
            this.grdDisassembler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdDisassembler.Location = new System.Drawing.Point(0, 0);
            this.grdDisassembler.Name = "grdDisassembler";
            this.grdDisassembler.RowHeadersWidth = 12;
            this.grdDisassembler.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.grdDisassembler.RowTemplate.Height = 16;
            this.grdDisassembler.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grdDisassembler.Size = new System.Drawing.Size(312, 304);
            this.grdDisassembler.TabIndex = 1;
            this.grdDisassembler.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdDisassembler_RowEnter);
            this.grdDisassembler.SelectionChanged += new System.EventHandler(this.grdDisassembler_SelectionChanged);
            this.grdDisassembler.DoubleClick += new System.EventHandler(this.grdDisassembler_DoubleClick);
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Column1.HeaderText = "Column1";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 80;
            // 
            // Column2
            // 
            this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column2.FillWeight = 20F;
            this.Column2.HeaderText = "Column2";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column3.FillWeight = 40F;
            this.Column3.HeaderText = "Column3";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            // 
            // PPCDisassembler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grdDisassembler);
            this.Name = "PPCDisassembler";
            this.Size = new System.Drawing.Size(312, 304);
            ((System.ComponentModel.ISupportInitialize)(this.grdDisassembler)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.DataGridView grdDisassembler;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;

    }
}
