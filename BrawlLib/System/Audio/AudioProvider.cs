using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Win32NT:
                    if (IntPtr.Size <= 4) return new wAudioProvider(device);
                    break;
            }
            
            if (device == null)
            {
                try
                {
                    return new alAudioProvider();
                }
                catch (TypeInitializationException) { }
            }

            return null;
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
