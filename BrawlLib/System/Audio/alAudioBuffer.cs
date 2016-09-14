using OpenTK.Audio.OpenAL;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Audio
{
    unsafe class alAudioBuffer : AudioBuffer
    {
        alAudioProvider _parent;
        int source;
        List<int> buffersToQueue;
        int addToCursor;

        internal override int PlayCursor
        {
            get { int v; AL.GetSource((uint)source, ALGetSourcei.SampleOffset, out v); return v + addToCursor; }
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

            buffersToQueue = new List<int>();

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
                lock (buffersToQueue)
                {
                    buffersToQueue.Add(buffer);
                }
            }
            Marshal.FreeHGlobal(data.Part1Address);

            int dequeuedBuffers;
            AL.GetSource(source, ALGetSourcei.BuffersProcessed, out dequeuedBuffers);
            if (dequeuedBuffers > 0) {
                int[] bufferids = new int[dequeuedBuffers];
                AL.SourceUnqueueBuffers(source, dequeuedBuffers, bufferids);
                foreach (int id in bufferids) {
                    int length = 0;
                    AL.GetBuffer(id, ALGetBufferi.Size, out length);
                    addToCursor += length;
                    AL.DeleteBuffer(id);
                }
            }
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
            Console.WriteLine($"Source {source} created");
            lock (buffersToQueue)
            {
                foreach (int buffer in buffersToQueue)
                    AL.SourceQueueBuffer(source, buffer);
                buffersToQueue.Clear();
            }
            AL.SourcePlay(source);
        }
        public override void Stop() 
        {
            AL.SourceStop(source);

            int dequeuedBuffers;
            AL.GetSource(source, ALGetSourcei.BuffersProcessed, out dequeuedBuffers);
            if (dequeuedBuffers > 0) {
                int[] bufferids = new int[dequeuedBuffers];
                AL.SourceUnqueueBuffers(source, dequeuedBuffers, bufferids);
                foreach (int id in bufferids) {
                    AL.DeleteBuffer(id);
                }
            }

            addToCursor = 0;

            AL.DeleteSource(source);
            Console.WriteLine($"Source {source} deleted");
            source = 0;
        }
    }
}
