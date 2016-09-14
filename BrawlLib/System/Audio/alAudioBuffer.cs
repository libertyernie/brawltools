using OpenTK.Audio.OpenAL;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Audio
{
    unsafe class alAudioBuffer : AudioBuffer
    {
        alAudioProvider _parent;
        int source;
        List<int> buffers;

        internal override int PlayCursor
        {
            get { int v; AL.GetSource((uint)source, ALGetSourcei.SampleOffset, out v); return v; }
            set { AL.Source((uint)source, ALSourcei.SampleOffset, value); }
        }
        public override int Volume
        {
            get { return 0; }
            set { }
        }
        public override int Pan
        {
            get { return 0; }
            set { }
        }
        
        internal alAudioBuffer(alAudioProvider parent, WaveFormatEx fmt)
        {
            _parent = parent;

            buffers = new List<int>();

            int size = DefaultBufferSpan * (int)fmt.nSamplesPerSec * fmt.nChannels * fmt.wBitsPerSample / 8;
            if (size == 0)
                return;

            _format = fmt.wFormatTag;
            _frequency = (int)fmt.nSamplesPerSec;
            _channels = fmt.nChannels;
            _bitsPerSample = fmt.wBitsPerSample;
            _dataLength = size;
            _blockAlign = _bitsPerSample * _channels / 8;
            _sampleLength = _dataLength / _blockAlign;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override BufferData Lock(int offset, int length)
        {
            BufferData data = new BufferData();

            offset = offset.Align(_blockAlign);
            length = length.Align(_blockAlign);
            
            data._dataOffset = offset;
            data._dataLength = length;
            data._sampleOffset = offset / _blockAlign;
            data._sampleLength = length / _blockAlign;

            if (length != 0)
            {
                IntPtr audioData = Marshal.AllocHGlobal(length);

                data._part1Address = audioData;
                data._part1Length = length;
                data._part1Samples = length / _blockAlign;

                data._part2Address = IntPtr.Zero;
                data._part2Length = 0;
                data._part2Samples = 0;
            }
            
            return data;
        }
        public override void Unlock(BufferData data)
        {
            int buffer = AL.GenBuffer();
            AL.BufferData(buffer, GetSoundFormat(_channels, _bitsPerSample), data.Part1Address, data.Part1Length, _frequency);
            if (source != 0)
            {
                AL.SourceQueueBuffer(source, buffer);
            }
            else
            {
                lock (buffers)
                {
                    buffers.Add(buffer);
                }
            }
            Marshal.FreeHGlobal(data.Part1Address);
        }

        public static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
        
        public override void Play()
        {
            source = AL.GenSource();
            lock (buffers)
            {
                foreach (int buffer in buffers)
                    AL.SourceQueueBuffer(source, buffer);
                buffers.Clear();
            }
            AL.SourcePlay(source);
        }
        public override void Stop() 
        {
            AL.SourceStop(source);
            AL.DeleteSource(source);
            source = 0;
        }
    }
}
