using OpenTK.Audio.OpenAL;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Audio
{
    unsafe class alAudioBuffer : AudioBuffer
    {
        // This class stores the source id and the length of discarded buffers.
        private class alSourceLock
        {
            public int currentSource;
            public int addToCursor;
        }

        alAudioProvider _parent;

        //Lock on this before using what's inside of it.
        alSourceLock sourceLock;

        // Buffers that need to be added to the stream once it starts playing.
        List<int> buffersToQueue;

        internal override int PlayCursor
        {
            get
            {
                lock (sourceLock)
                {
                    int v;
                    AL.GetSource(sourceLock.currentSource, ALGetSourcei.SampleOffset, out v);
                    return v + sourceLock.addToCursor;
                }
            }
            set
            {
                lock (sourceLock)
                {
                    AL.Source(sourceLock.currentSource, ALSourcei.SampleOffset, value - sourceLock.addToCursor  );
                }
            }
        }
        public override int Volume
        {
            get { return 0; }
            set { }
        }
        public override int Pan
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        
        internal alAudioBuffer(alAudioProvider parent, WaveFormatEx fmt)
        {
            _parent = parent;

            buffersToQueue = new List<int>();
            sourceLock = new alSourceLock();

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
                // Create a "fake" BufferData that does not point to an actual sound buffer.
                // We'll populate the real buffer when we unlock it.
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

            lock (sourceLock)
                {
                if (sourceLock.currentSource != 0)
                {
                    // Already playing - add buffer to source
                    AL.SourceQueueBuffer(sourceLock.currentSource, buffer);
                }
                else
                {
                    // This buffer will need to be added once the source is created
                    lock (buffersToQueue)
                    {
                        buffersToQueue.Add(buffer);
                    }
                }
            }

            // Free temporary memory buffer now that the data is in the real buffer
            Marshal.FreeHGlobal(data.Part1Address);

            Dequeue();
        }

        private void Dequeue()
        {
            lock (sourceLock)
            {
                int dequeuedBuffers;
                AL.GetSource(sourceLock.currentSource, ALGetSourcei.BuffersProcessed, out dequeuedBuffers);
                if (dequeuedBuffers > 0)
                {
                    int[] bufferids = new int[dequeuedBuffers];
                    AL.SourceUnqueueBuffers(sourceLock.currentSource, dequeuedBuffers, bufferids);
                    foreach (int id in bufferids)
                    {
                        int length = 0;
                        AL.GetBuffer(id, ALGetBufferi.Size, out length);
                        sourceLock.addToCursor += length;
                        AL.DeleteBuffer(id);
                    }
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
            lock (sourceLock)
            {
                if (sourceLock.currentSource != 0)
                    throw new Exception("Cannot start when already playing");

                sourceLock.currentSource = AL.GenSource();
                Console.WriteLine($"Source {sourceLock.currentSource} created");
                lock (buffersToQueue)
                {
                    foreach (int buffer in buffersToQueue)
                        AL.SourceQueueBuffer(sourceLock.currentSource, buffer);
                    buffersToQueue.Clear();
                }
                AL.SourcePlay(sourceLock.currentSource);
            }
        }
        public override void Stop() 
        {
            lock (sourceLock)
            {
                if (sourceLock.currentSource == 0)
                    return;

                AL.SourceStop(sourceLock.currentSource);

                Dequeue();

                sourceLock.addToCursor = 0;

                AL.DeleteSource(sourceLock.currentSource);
                Console.WriteLine($"Source {sourceLock.currentSource} deleted");
                sourceLock.currentSource = 0;
            }
        }
    }
}
