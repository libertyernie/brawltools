namespace BrawlBox
{
    partial class AboutForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.btnClose = new System.Windows.Forms.Button();
			this.lblName = new System.Windows.Forms.Label();
			this.txtDescription = new System.Windows.Forms.TextBox();
			this.lblCopyright = new System.Windows.Forms.Label();
			this.lblBrawlLib = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnClose
			// 
			this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.btnClose.Location = new System.Drawing.Point(173, 235);
			this.btnClose.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(100, 28);
			this.btnClose.TabIndex = 0;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// lblName
			// 
			this.lblName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblName.Font = new System.Drawing.Font("Georgia", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblName.Location = new System.Drawing.Point(16, 11);
			this.lblName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(415, 28);
			this.lblName.TabIndex = 1;
			this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// txtDescription
			// 
			this.txtDescription.Font = new System.Drawing.Font("Corbel", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtDescription.Location = new System.Drawing.Point(16, 43);
			this.txtDescription.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtDescription.Multiline = true;
			this.txtDescription.Name = "txtDescription";
			this.txtDescription.ReadOnly = true;
			this.txtDescription.Size = new System.Drawing.Size(413, 110);
			this.txtDescription.TabIndex = 3;
			this.txtDescription.TabStop = false;
			this.txtDescription.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// lblCopyright
			// 
			this.lblCopyright.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblCopyright.Font = new System.Drawing.Font("Corbel", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCopyright.Location = new System.Drawing.Point(16, 158);
			this.lblCopyright.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblCopyright.Name = "lblCopyright";
			this.lblCopyright.Size = new System.Drawing.Size(415, 50);
			this.lblCopyright.TabIndex = 4;
			this.lblCopyright.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblBrawlLib
			// 
			this.lblBrawlLib.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblBrawlLib.Font = new System.Drawing.Font("Corbel", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblBrawlLib.Location = new System.Drawing.Point(16, 208);
			this.lblBrawlLib.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblBrawlLib.Name = "lblBrawlLib";
			this.lblBrawlLib.Size = new System.Drawing.Size(415, 25);
			this.lblBrawlLib.TabIndex = 5;
			this.lblBrawlLib.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// AboutForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(447, 278);
			this.Controls.Add(this.lblBrawlLib);
			this.Controls.Add(this.lblCopyright);
			this.Controls.Add(this.txtDescription);
			this.Controls.Add(this.lblName);
			this.Controls.Add(this.btnClose);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblCopyright;
		private System.Windows.Forms.Label lblBrawlLib;
    }
}