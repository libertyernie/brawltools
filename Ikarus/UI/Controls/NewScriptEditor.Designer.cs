namespace System.Windows.Forms
{
    partial class NewScriptEditor
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
            this.pnlIntel = new System.Windows.Forms.Panel();
            this.intelBox = new System.Windows.Forms.ListBox();
            this.textBox = new UrielGuy.SyntaxHighlightingTextBox.SyntaxHighlightingTextBox();
            this.pnlIntel.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlIntel
            // 
            this.pnlIntel.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlIntel.Controls.Add(this.intelBox);
            this.pnlIntel.Location = new System.Drawing.Point(15, 17);
            this.pnlIntel.Name = "pnlIntel";
            this.pnlIntel.Padding = new System.Windows.Forms.Padding(1);
            this.pnlIntel.Size = new System.Drawing.Size(200, 100);
            this.pnlIntel.TabIndex = 4;
            this.pnlIntel.Visible = false;
            this.pnlIntel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.intelBox_MouseDown);
            // 
            // intelBox
            // 
            this.intelBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.intelBox.FormattingEnabled = true;
            this.intelBox.IntegralHeight = false;
            this.intelBox.Location = new System.Drawing.Point(1, 1);
            this.intelBox.Name = "intelBox";
            this.intelBox.Size = new System.Drawing.Size(198, 98);
            this.intelBox.Sorted = true;
            this.intelBox.TabIndex = 1;
            this.intelBox.DoubleClick += new System.EventHandler(this.intelBox_DoubleClick);
            this.intelBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.intelBox_MouseDown);
            // 
            // textBox
            // 
            this.textBox.AutoWordSelection = true;
            this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox.DetectUrls = false;
            this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox.Location = new System.Drawing.Point(0, 0);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(301, 197);
            this.textBox.TabIndex = 5;
            this.textBox.Text = "";
            this.textBox.WordWrap = false;
            this.textBox.SelectionChanged += new System.EventHandler(this.textBox_SelectionChanged);
            this.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown);
            // 
            // NewScriptEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlIntel);
            this.Controls.Add(this.textBox);
            this.Name = "NewScriptEditor";
            this.Size = new System.Drawing.Size(301, 197);
            this.pnlIntel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlIntel;
        private System.Windows.Forms.ListBox intelBox;
        private UrielGuy.SyntaxHighlightingTextBox.SyntaxHighlightingTextBox textBox;
    }
}
