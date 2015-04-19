namespace System.Windows.Forms
{
    partial class MaterialEditor
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tbPixels = new System.Windows.Forms.TabPage();
            this.tbShader = new System.Windows.Forms.TabPage();
            this.tbSCN0 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tbPixels);
            this.tabControl1.Controls.Add(this.tbShader);
            this.tabControl1.Controls.Add(this.tbSCN0);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(752, 335);
            this.tabControl1.TabIndex = 0;
            // 
            // tbPixels
            // 
            this.tbPixels.Location = new System.Drawing.Point(4, 22);
            this.tbPixels.Name = "tbPixels";
            this.tbPixels.Padding = new System.Windows.Forms.Padding(3);
            this.tbPixels.Size = new System.Drawing.Size(744, 309);
            this.tbPixels.TabIndex = 0;
            this.tbPixels.Text = "Pixel Operations";
            // 
            // tbShader
            // 
            this.tbShader.Location = new System.Drawing.Point(4, 22);
            this.tbShader.Name = "tbShader";
            this.tbShader.Padding = new System.Windows.Forms.Padding(3);
            this.tbShader.Size = new System.Drawing.Size(744, 309);
            this.tbShader.TabIndex = 1;
            this.tbShader.Text = "Shader Properties";
            // 
            // tbSCN0
            // 
            this.tbSCN0.Location = new System.Drawing.Point(4, 22);
            this.tbSCN0.Name = "tbSCN0";
            this.tbSCN0.Size = new System.Drawing.Size(744, 309);
            this.tbSCN0.TabIndex = 2;
            this.tbSCN0.Text = "Linked SCN0 Entries";
            // 
            // MaterialEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "MaterialEditor";
            this.Size = new System.Drawing.Size(752, 335);
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TabControl tabControl1;
        private TabPage tbPixels;
        private TabPage tbShader;
        private TabPage tbSCN0;
    }
}
