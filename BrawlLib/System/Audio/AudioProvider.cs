using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace System.Audio
{
    public abstract class AudioProvider : IDisposable
    {
        internal AudioDevice _device;
        public AudioDevice Device { get { return _device; } }

        internal List<AudioBuffer> _buffers = new List<AudioBuffer>();
        public List<AudioBuffer> Buffers { get { return _buffers; } }

        public static AudioProvider Create(AudioDevice device)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT: return new wAudioProvider(device);
                default: return null;
            }
        }

        ~AudioProvider() { Dispose(); }
        public virtual void Dispose()
        {
            foreach (AudioBuffer buffer in _buffers)
                buffer.Dispose();
            _buffers.Clear();
            GC.SuppressFinalize(this);
        }

        public abstract void Attach(Control owner);

        public abstract AudioBuffer CreateBuffer(IAudioStream target);
    }
}
