using System;
using System.Audio;
using System.ComponentModel;

namespace System.Windows.Forms
{
    public class AudioPlaybackPanel : UserControl
    {
        #region Designer

        private CustomTrackBar trackBar1;
        private Button btnPlay;
        private Button btnRewind;
        private Label lblProgress;
        private Timer tmrUpdate;
        private System.ComponentModel.IContainer components;
        private ComboBox lstStreams;
        private CheckBox chkLoop;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.trackBar1 = new System.Windows.Forms.CustomTrackBar();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnRewind = new System.Windows.Forms.Button();
            this.chkLoop = new System.Windows.Forms.CheckBox();
            this.lblProgress = new System.Windows.Forms.Label();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.lstStreams = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // trackBar1
            // 
            this.trackBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBar1.Location = new System.Drawing.Point(0, 4);
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(536, 45);
            this.trackBar1.TabIndex = 0;
            this.trackBar1.TickFrequency = 2;
            this.trackBar1.UserSeek += new System.EventHandler(this.trackBar1_UserSeek);
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // btnPlay
            // 
            this.btnPlay.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnPlay.Location = new System.Drawing.Point(231, 69);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(75, 20);
            this.btnPlay.TabIndex = 1;
            this.btnPlay.Text = "Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnRewind
            // 
            this.btnRewind.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnRewind.Location = new System.Drawing.Point(201, 69);
            this.btnRewind.Name = "btnRewind";
            this.btnRewind.Size = new System.Drawing.Size(24, 20);
            this.btnRewind.TabIndex = 2;
            this.btnRewind.Text = "|<";
            this.btnRewind.UseVisualStyleBackColor = true;
            this.btnRewind.Click += new System.EventHandler(this.btnRewind_Click);
            // 
            // chkLoop
            // 
            this.chkLoop.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.chkLoop.Location = new System.Drawing.Point(133, 69);
            this.chkLoop.Name = "chkLoop";
            this.chkLoop.Size = new System.Drawing.Size(62, 20);
            this.chkLoop.TabIndex = 3;
            this.chkLoop.Text = "Loop";
            this.chkLoop.UseVisualStyleBackColor = true;
            this.chkLoop.CheckedChanged += new System.EventHandler(this.chkLoop_CheckedChanged);
            // 
            // lblProgress
            // 
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgress.Location = new System.Drawing.Point(0, 31);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(536, 23);
            this.lblProgress.TabIndex = 4;
            this.lblProgress.Text = "0/0";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Interval = 10;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // lstStreams
            // 
            this.lstStreams.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lstStreams.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lstStreams.FormattingEnabled = true;
            this.lstStreams.Location = new System.Drawing.Point(313, 68);
            this.lstStreams.Name = "lstStreams";
            this.lstStreams.Size = new System.Drawing.Size(73, 21);
            this.lstStreams.TabIndex = 5;
            this.lstStreams.SelectedIndexChanged += new System.EventHandler(this.lstStreams_SelectedIndexChanged);
            // 
            // AudioPlaybackPanel
            // 
            this.Controls.Add(this.lstStreams);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.chkLoop);
            this.Controls.Add(this.btnRewind);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.trackBar1);
            this.Name = "AudioPlaybackPanel";
            this.Size = new System.Drawing.Size(536, 111);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private bool _updating = false;
        private bool _loop = false;
        private bool _isPlaying = false;
        //private bool _isScrolling = false;

        private DateTime _sampleTime;
        private IAudioStream[] _targetStreams;

        private IAudioStream _targetStream 
        {
            get 
            {
                if (_targetIndex < 0)
                    return null;

                return _targetStreams[_targetIndex]; 
            } 
        }

        public IAudioStream[] TargetStreams
        {
            get { return _targetStreams; }
            set
            {
                if (value == _targetStreams)
                    return;

                lstStreams.Items.Clear(); 
                if (_targetStreams != null)
                    for (int x = 0; x < _targetStreams.Length; x++)
                    {
                        _targetStreams[x].Dispose();
                        _targetStreams[x] = null;
                    }

                if ((_targetStreams = value) != null)
                {
                    _buffers = new AudioBuffer[_targetStreams.Length];
                    if (lstStreams.Visible = _targetStreams.Length > 1)
                    {
                        int i = 1;
                        foreach (IAudioStream s in _targetStreams)
                            lstStreams.Items.Add("Stream" + i++);
                    }
                    else
                        lstStreams.Items.Add("");
                }
                else
                {
                    lstStreams.Visible = false;
                    lstStreams.Items.Add("");
                }
                _updating = true;
                lstStreams.SelectedIndex = _targetIndex = 0;
                _updating = false;
            }
        }

        private IAudioSource _targetSource;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IAudioSource TargetSource
        {
            get { return _targetSource; }
            set { TargetChanged(value); }
        }

        private AudioProvider _provider;

        private AudioBuffer[] _buffers;
        private AudioBuffer _buffer 
        {
            get
            {
                if (_targetIndex < 0)
                    return null;

                return _buffers[_targetIndex]; 
            }
        }

        public AudioPlaybackPanel() { InitializeComponent(); }

        protected override void Dispose(bool disposing)
        {
            Close();
            if (_provider != null)
            {
                _provider.Dispose();
                _provider = null;
            }
            base.Dispose(disposing);
        }

        private void Close()
        {
            //Stop playback
            Stop();

            //Dispose of buffer
            if (_buffers != null)
                for (int i = 0; i < _buffers.Length; i++)
                    if (_buffers[i] != null)
                    {
                        _buffers[i].Dispose();
                        _buffers[i] = null;
                    }

            if (_targetStreams != null)
            {
                for (int i = 0; i < _targetStreams.Length; i++)
                    if (_targetStreams[i] != null)
                    {
                        _targetStreams[i].Dispose();
                        _targetStreams[i] = null;
                    }
                _targetStreams = null;
            }

            _targetSource = null;

            //Reset fields
            chkLoop.Checked = false;
        }

        private void TargetChanged(IAudioSource newTarget)
        {
            if (_targetSource == newTarget)
                return;

            Close();

            if ((_targetSource = newTarget) == null)
                return;
            if ((TargetStreams = _targetSource.CreateStreams()) == null)
                return;
            if (_targetStream == null)
                return;

            //Create provider
            if (_provider == null)
            {
                _provider = AudioProvider.Create(null);
                _provider.Attach(this);
            }

            chkLoop.Checked = false;
            chkLoop.Enabled = _targetStream.IsLooping;

            //Create buffer for stream
            for (int i = 0; i < _buffers.Length; i++)
                _buffers[i] = _provider.CreateBuffer(_targetStreams[i]);

            if (_targetStream.Frequency > 0)
                _sampleTime = new DateTime((long)_targetStream.Samples * 10000000 / _targetStream.Frequency);

            trackBar1.Value = 0;
            trackBar1.TickStyle = TickStyle.None;
            trackBar1.Maximum = _targetStream.Samples;
            trackBar1.TickFrequency = _targetStream.Samples / 8;
            trackBar1.TickStyle = TickStyle.BottomRight;

            if (_targetStream.Frequency > 0)
                UpdateTimeDisplay();

            Enabled = _targetStream.Samples > 0;
        }

        private void UpdateTimeDisplay()
        {
            if (_targetStream == null) return;
            DateTime t = new DateTime((long)trackBar1.Value * 10000000 / _targetStream.Frequency);
            lblProgress.Text = String.Format("{0:mm:ss.ff} / {1:mm:ss.ff}", t, _sampleTime);
        }

        private void Seek(int sample)
        {
            trackBar1.Value = sample;

            //Only seek the buffer when playing.
            if (_isPlaying)
            {
                Stop();
                _buffer.Seek(sample);
                Play();
            }
        }

        private void Play()
        {
            if ((_isPlaying) || (_buffer == null))
                return;

            _isPlaying = true;

            //Start from beginning if at end
            if (trackBar1.Value == _targetStream.Samples)
                trackBar1.Value = 0;

            //Seek buffer to current sample
            _buffer.Seek(trackBar1.Value);

            //Fill initial buffer
            tmrUpdate_Tick(null, null);
            //Start timer
            tmrUpdate.Start();

            //Begin playback
            _buffer.Play();

            btnPlay.Text = "Stop";
        }
        private void Stop()
        {
            if (!_isPlaying)
                return;

            _isPlaying = false;

            //Stop timer
            tmrUpdate.Stop();

            //Stop device
            if (_buffer != null)
                _buffer.Stop();

            btnPlay.Text = "Play";
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if ((_isPlaying) && (_buffer != null))
            {
                _buffer.Fill();

                trackBar1.Value = _buffer.ReadSample;

                if (!_loop)
                {
                    if (_buffer.ReadSample >= _targetStream.Samples)
                        Stop();
                }
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (_isPlaying)
                Stop();
            else
                Play();
        }

        private void chkLoop_CheckedChanged(object sender, EventArgs e)
        {
            _loop = chkLoop.Checked;
            if (_buffer != null)
                _buffer.Loop = _loop;
        }

        private void btnRewind_Click(object sender, EventArgs e) { Seek(0); }
        private void trackBar1_ValueChanged(object sender, EventArgs e) { UpdateTimeDisplay(); }
        private void trackBar1_UserSeek(object sender, EventArgs e) { Seek(trackBar1.Value); }

        int _targetIndex = 0;
        private void lstStreams_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            bool temp = _isPlaying;

            if (temp)
                Stop();

            _targetIndex = lstStreams.SelectedIndex;

            if (_buffer != null)
                _buffer.Loop = _loop;

            if (temp)
                Play();
        }
    }
}
