using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmashBox.DX9;
using SmashBox.AudioProcessing;
using SmashBox.Properties;

namespace SmashBox
{
    public interface IAudioSource
    {
        int AudioStreamCount { get; }
        IAudioStream[] AudioStreams { get; }
    }
    public interface IAudioStream
    {
        WaveFormatTag Format { get; }
        int BitsPerSample { get; }
        int Samples { get; }
        int Channels { get; }
        int Frequency { get; }

        bool IsLooping { get; }
        int LoopStartSample { get; }
        int LoopEndSample { get; }

        int SamplePosition { get; set; }
        int ReadSamples(VoidPtr destAddr, int numSamples);

        void Wrap();
    }

    public unsafe partial class AudioPlaybackControl : UserControl
    {
        private const int StaticBufferSeconds = 2;

        private IAudioStream _targetObject;

        private DirectSound _ds;
        private DirectSoundBuffer _buffer;

        private WaveFormatEx _sampleFormat;
        private bool _isLooped, _loopEnabled;
        private uint _bufferSize;//, _writeOffset;
        private uint _playSampleOffset, _playBufferOffset;
        private uint _loopEndSample, _loopStartSample;
        private uint _numSamples;

        private bool _isPlaying;
        private DateTime _sampleTime;

        private uint _writeBufferOffset, _writeSampleOffset;

        private bool _isScrolling;

        public AudioPlaybackControl()
        {
            InitializeComponent();
            //_ds = new DirectSound();
            //_ds.SetCooperativeLevel(this, DSCooperativeLevel.Priority);
        }

        public IAudioStream TargetObject
        {
            get { return _targetObject; }
            set
            {
                //Stop playback
                Stop();

                //Dispose of old buffer
                if (_buffer != null)
                {
                    _buffer.Dispose();
                    _buffer = null;
                }

                if ((_targetObject = value) == null)
                    return;

                WaveFormatEx fmt = new WaveFormatEx(value.Format, value.Channels, value.Frequency, value.BitsPerSample);
                _sampleFormat = fmt;
                _sampleTime = new DateTime((long)value.Samples * 10000000 / value.Frequency);
                _numSamples = (uint)value.Samples;

                if (_isLooped = value.IsLooping)
                {
                    _loopEndSample = (uint)value.LoopEndSample;
                    _loopStartSample = (uint)value.LoopStartSample;
                }

                DSBufferCapsFlags flags = DSBufferCapsFlags.CtrlVolume | DSBufferCapsFlags.GetCurrentPosition2 | DSBufferCapsFlags.LocDefer | DSBufferCapsFlags.GlobalFocus;
                
                _bufferSize = StaticBufferSeconds * fmt.nAvgBytesPerSec;
                _buffer = _ds.CreateSoundBuffer(new DSBufferDesc(_bufferSize, flags, &fmt, Guid.Empty));

                chkLoop.Enabled = _isLooped = value.IsLooping;
                chkLoop.Checked = false;

                trackBar1.Minimum = 0;
                trackBar1.Value = 0;
                trackBar1.Maximum = (int)_numSamples;
                trackBar1.TickFrequency = (int)_numSamples / 8;
                UpdateTimeDisplay();

                _isPlaying = false;
                _writeBufferOffset = _writeSampleOffset = 0;
                _playBufferOffset = _playSampleOffset = 0;

                btnPlay.BackgroundImage = Resources.Pause;
            }
        }

        private void RefreshOffset()
        {
            uint samplePos = _buffer.PlayPosition / _sampleFormat.nBlockAlign;
            uint playPos = samplePos * _sampleFormat.nBlockAlign;

            uint sampleDiff = (((playPos < _playBufferOffset) ? (playPos + _bufferSize) : playPos) - _playBufferOffset) / _sampleFormat.nBlockAlign;
            uint playDiff = sampleDiff * _sampleFormat.nBlockAlign;

            if (playDiff == 0)
                return;

            _playSampleOffset += sampleDiff;
            _playBufferOffset = playPos;

            if (_playSampleOffset >= _numSamples)
            {
                if (_isLooped && _loopEnabled)
                    _playSampleOffset = _loopStartSample + ((_playSampleOffset - _loopStartSample) % (_loopEndSample - _loopStartSample));
                else
                {
                    _playSampleOffset = _numSamples;
                    Stop();
                }
            }

            if (!_isScrolling)
                trackBar1.Value = (int)_playSampleOffset;
        }
        private void UpdateTimeDisplay()
        {
            DateTime t = new DateTime((long)trackBar1.Value * 10000000 / _sampleFormat.nSamplesPerSec);
            lblCTime.Text = String.Format("{0:mm:ss.ff} / {1:mm:ss.ff}", t, _sampleTime);
        }
        private void Fill()
        {
            uint samplePos = _buffer.PlayPosition / _sampleFormat.nBlockAlign;
            uint playPos = samplePos * _sampleFormat.nBlockAlign;

            uint numSamples = (((playPos <= _writeBufferOffset) ? (playPos + _bufferSize) : playPos) - _writeBufferOffset) / _sampleFormat.nBlockAlign / 8;

            bool loop = _loopEnabled && (_writeSampleOffset <= _loopEndSample) && ((_writeSampleOffset + numSamples) > _loopEndSample);
            if (loop)
                numSamples = _loopEndSample - _writeSampleOffset;

            uint lockLen = numSamples * _sampleFormat.nBlockAlign;
            if (lockLen == 0)
                return;

            int samplesRead;
            DirectSoundBufferData data = _buffer.Lock(_writeBufferOffset, lockLen);
            if (data.IsSplit)
            {
                byte* buffer = stackalloc byte[(int)lockLen];
                samplesRead = _targetObject.ReadSamples(buffer, (int)numSamples);
                Util.MoveMemory(data.Address1, buffer, data.Length1);
                Util.MoveMemory(data.Address2, buffer + data.Length1, data.Length2);
            }
            else
            {
                Util.FillMemory(data.Address1, lockLen, 0);
                samplesRead = _targetObject.ReadSamples(data.Address1, (int)numSamples);
            }

            _writeBufferOffset += lockLen;
            if (_writeBufferOffset >= _bufferSize)
                _writeBufferOffset -= _bufferSize;

            _writeSampleOffset += (uint)samplesRead;

            _buffer.Unlock(data);

            if (loop)
            {
                _targetObject.Wrap();
                _writeSampleOffset = _loopStartSample;
            }
        }

        private void Seek(int position)
        {
            bool play = _isPlaying;

            if (play)
                Stop();
            _targetObject.SamplePosition = position;
            _playSampleOffset = _writeSampleOffset = (uint)position;
            _writeBufferOffset = _buffer.PlayPosition;
            if (!_isScrolling)
                trackBar1.Value = position;
            if (play)
                Play();
        }

        private void Play()
        {
            if (_isPlaying)
                return;

            if (_playSampleOffset == _numSamples)
                Seek(0);

            tmrRefresh_Tick(null, null);
            tmrRefresh.Start();

            _buffer.Play(0, DSBufferPlayFlags.Looping);

            _isPlaying = true;
            btnPlay.BackgroundImage = Resources.Play;
        }
        private void Stop()
        {
            tmrRefresh.Stop();
            if (_buffer != null)
                _buffer.Stop();

            _isPlaying = false;
            btnPlay.BackgroundImage = Resources.Pause;
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (_isPlaying)
                Stop();
            else
                Play();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            UpdateTimeDisplay();
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            RefreshOffset();
            Fill();
        }

        private void chkLoop_CheckedChanged(object sender, EventArgs e)
        {
            _loopEnabled = chkLoop.Checked;
        }

        private void trackBar1_MouseDown(object sender, MouseEventArgs e)
        {
            //Get position of mouse
            if (e.Button != MouseButtons.Left)
                return;

            int x = 12, w = trackBar1.Width - 15;
            int y = 4, h = 20;

            if ((e.X < x) || (e.X > w) || (e.Y < y) || (e.Y > h))
                return;

            float scale = ((float)e.X - x) / (w - x);
            int pos = (int)(_numSamples * scale);

            _isScrolling = true;

            if ((trackBar1.Value > (pos - 10)) && (trackBar1.Value < (pos + 10)))
                return;

            trackBar1.Value = pos;
            trackBar1.InvokeMethod("SendMessage", 0x201, 1, (e.Y & 0xFFFF) << 16 | (e.X & 0xFFFF));
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (_isScrolling)
                Seek(trackBar1.Value);
            _isScrolling = false;
        }

        private void btnRewind_Click(object sender, EventArgs e)
        {
            Seek(0);
        }

        private Pen _loopStartPen = new Pen(Brushes.Red, 2.0f);
        private Pen _loopEndPen = new Pen(Brushes.Yellow, 2.0f);
        private void AudioPlaybackControl_Paint(object sender, PaintEventArgs e)
        {
            if (_isLooped)
            {
                using (Graphics g = trackBar1.CreateGraphics())
                {
                    int x = 13, w = trackBar1.Width - 15;
                    int y = 20;

                    float scale = (float)_loopStartSample / _numSamples * w + x;
                    g.DrawLine(_loopStartPen, scale, y, scale, y + 20);

                    scale = (float)_loopEndSample / _numSamples * w + x;
                    g.DrawLine(_loopEndPen, scale, y, scale, y + 20);
                }
            }
        }
    }
}
