namespace SmashBox
{
    partial class AudioPlaybackControl
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
            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
            }
            if (_ds != null)
            {
                _ds.Dispose();
                _ds = null;
            }
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
            this.components = new System.ComponentModel.Container();
            this.btnPlay = new System.Windows.Forms.Button();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.lblCTime = new System.Windows.Forms.Label();
            this.chkLoop = new System.Windows.Forms.CheckBox();
            this.btnRewind = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnPlay
            // 
            this.btnPlay.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnPlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnPlay.Location = new System.Drawing.Point(126, 51);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(40, 30);
            this.btnPlay.TabIndex = 0;
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // trackBar1
            // 
            this.trackBar1.Dock = System.Windows.Forms.DockStyle.Top;
            this.trackBar1.LargeChange = 0;
            this.trackBar1.Location = new System.Drawing.Point(0, 0);
            this.trackBar1.Maximum = 1213;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(246, 45);
            this.trackBar1.SmallChange = 0;
            this.trackBar1.TabIndex = 2;
            this.trackBar1.TickFrequency = 500;
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            this.trackBar1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.trackBar1_MouseDown);
            this.trackBar1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackBar1_MouseUp);
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 10;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // lblCTime
            // 
            this.lblCTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCTime.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCTime.Location = new System.Drawing.Point(3, 30);
            this.lblCTime.Name = "lblCTime";
            this.lblCTime.Size = new System.Drawing.Size(240, 23);
            this.lblCTime.TabIndex = 4;
            this.lblCTime.Text = "00:00 / 00:00";
            this.lblCTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // chkLoop
            // 
            this.chkLoop.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.chkLoop.AutoSize = true;
            this.chkLoop.Location = new System.Drawing.Point(193, 34);
            this.chkLoop.Name = "chkLoop";
            this.chkLoop.Size = new System.Drawing.Size(50, 17);
            this.chkLoop.TabIndex = 5;
            this.chkLoop.Text = "Loop";
            this.chkLoop.UseVisualStyleBackColor = true;
            this.chkLoop.CheckedChanged += new System.EventHandler(this.chkLoop_CheckedChanged);
            // 
            // btnRewind
            // 
            this.btnRewind.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnRewind.BackgroundImage = global::SmashBox.Properties.Resources.Rewind;
            this.btnRewind.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRewind.Location = new System.Drawing.Point(80, 51);
            this.btnRewind.Name = "btnRewind";
            this.btnRewind.Size = new System.Drawing.Size(40, 30);
            this.btnRewind.TabIndex = 3;
            this.btnRewind.UseVisualStyleBackColor = true;
            this.btnRewind.Click += new System.EventHandler(this.btnRewind_Click);
            // 
            // AudioPlaybackControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.chkLoop);
            this.Controls.Add(this.btnRewind);
            this.Controls.Add(this.lblCTime);
            this.Controls.Add(this.trackBar1);
            this.Name = "AudioPlaybackControl";
            this.Size = new System.Drawing.Size(246, 95);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.AudioPlaybackControl_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Button btnRewind;
        private System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.Label lblCTime;
        private System.Windows.Forms.CheckBox chkLoop;
    }
}
