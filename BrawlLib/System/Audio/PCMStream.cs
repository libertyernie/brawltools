using BrawlLib.SSBBTypes;
using System.Runtime.InteropServices;

namespace System.Audio
{
    public unsafe class PCMStream : IAudioStream
    {
        private IntPtr _allocatedHGlobal = IntPtr.Zero;
#if RSTMLIB
#else
        private BrawlLib.IO.FileMap _sourceMap;
#endif

        private short* _source;

        private int _bps;
        private int _numSamples;
        private int _numChannels;
        private int _frequency;
        private int _samplePos;

        private bool _looped;
        private int _loopStart;
        private int _loopEnd;

        public WaveFormatTag Format { get { return WaveFormatTag.WAVE_FORMAT_PCM; } }
        public int BitsPerSample { get { return _bps; } }
        public int Samples { get { return _numSamples; } }
        public int Channels { get { return _numChannels; } }
        public int Frequency { get { return _frequency; } }

        public bool IsLooping { get { return _looped; } set { _looped = value; } }
        public int LoopStartSample { get { return _loopStart; } set { _loopStart = value; } }
        public int LoopEndSample { get { return _loopEnd; } set { _loopEnd = value; } }

        public int SamplePosition
        {
            get { return _samplePos; }
            set { _samplePos = Math.Max(Math.Min(value, _numSamples), 0); }
        }

        public PCMStream(byte[] wavData)
        {
            _allocatedHGlobal = Marshal.AllocHGlobal(wavData.Length);
            Marshal.Copy(wavData, 0, _allocatedHGlobal, wavData.Length);

            RIFFHeader* header = (RIFFHeader*)_allocatedHGlobal;
            _bps = header->_fmtChunk._bitsPerSample;
            _numChannels = header->_fmtChunk._channels;
            _frequency = (int)header->_fmtChunk._samplesSec;
            _numSamples = (int)(header->_dataChunk._chunkSize / header->_fmtChunk._blockAlign);

            _source = (short*)((byte*)_allocatedHGlobal + header->GetSize());
            _samplePos = 0;

            _looped = false;
            _loopStart = 0;
            _loopEnd = _numSamples;

            smplLoop[] loops = header->_smplLoops;
            if (loops.Length > 0)
            {
                _looped = true;
                _loopStart = (int)loops[0]._dwStart;
                _loopEnd = (int)loops[0]._dwEnd;
            }
        }

#if RSTMLIB
#else
        internal PCMStream(short* source, int samples, int sampleRate, int channels, int bps)
        {
            _sourceMap = null;

            _bps = bps; //16
            _numChannels = channels;
            _frequency = sampleRate;
            _numSamples = samples;

            _source = source;
            _samplePos = 0;
        }

        internal PCMStream(WaveInfo* pWAVE, VoidPtr dataAddr)
        {
            _frequency = pWAVE->_sampleRate;
            _numSamples = pWAVE->NumSamples;
            _numChannels = pWAVE->_format._channels;

            _bps = pWAVE->_format._encoding == 0 ? 8 : 16;

            if (_numSamples <= 0) return;

            _loopStart = (int)pWAVE->LoopSample;
            _loopEnd = _numSamples;

            _source = (short*)dataAddr;
            _samplePos = 0;
        }
#endif

        internal static PCMStream[] GetStreams(RSTMHeader* pRSTM, VoidPtr dataAddr) {
            HEADHeader* pHeader = pRSTM->HEADData;
            StrmDataInfo* part1 = pHeader->Part1;
            int c = part1->_format._channels;
            PCMStream[] streams = new PCMStream[c.RoundUpToEven() / 2];

            for (int i = 0; i < streams.Length; i++) {
                int x = (i + 1) * 2 <= c ? 2 : 1;
                streams[i] = new PCMStream(pRSTM, x, i * 2, dataAddr);
            }

            return streams;
        }

        internal PCMStream(RSTMHeader* header, int channels, int startChannel, void* audioSource) {
            StrmDataInfo* info = header->HEADData->Part1;
            if (channels > 2) throw new NotImplementedException("Cannot load PCM16 audio with more than 2 channels");
            if (info->_format._channels < channels) throw new Exception("Not enough channels");
            
            int size = info->_numSamples * channels * sizeof(short);
            _allocatedHGlobal = Marshal.AllocHGlobal(size);

            byte* fromL = (byte*)audioSource;
            byte* fromR = fromL;
            byte* to = (byte*)_allocatedHGlobal;

            bool stereo = channels == 2;

            for (int block = 0; block < info->_numBlocks; block++)
            {
                int bs = (block == info->_numBlocks - 1)
                    ? info->_lastBlockSize
                    : info->_blockSize;
                int bt = (block == info->_numBlocks - 1)
                    ? info->_lastBlockTotal
                    : info->_blockSize;

                if (block == 0)
                {
                    fromL += (bt * startChannel);
                    fromR += (bt * startChannel);
                }
                else
                {
                    fromL += bt * (info->_format._channels - channels);
                    fromR += bt * (info->_format._channels - channels);
                }

                if (stereo)
                    fromR += bt;
                for (int i=0; i<bs; i+=2)
                {
                    to[0] = fromL[1];
                    to[1] = fromL[0];
                    fromL += 2;
                    to += 2;
                    if (stereo)
                    {
                        to[0] = fromR[1];
                        to[1] = fromR[0];
                        fromR += 2;
                        to += 2;
                    }
                }
                for (int i = bs; i < bt; i++)
                {
                    fromL++;
                    fromR++;
                }
                if (stereo)
                    fromL += bt;
            }

            _bps = 16;
            _numChannels = channels;
            _frequency = info->_sampleRate;
            _numSamples = info->_numSamples;

            _source = (short*)_allocatedHGlobal;
            _samplePos = 0;

            _looped = info->_format._looped != 0;
            _loopStart = info->_loopStartSample;
            _loopEnd = _numSamples;
        }

        public int ReadSamples(VoidPtr destAddr, int numSamples)
        {
            short* sPtr = _source + (_samplePos * _numChannels);
            short* dPtr = (short*)destAddr;

            int max = Math.Min(numSamples, _numSamples - _samplePos);

            for (int i = 0; i < max; i++)
                for (int x = 0; x < _numChannels; x++)
                    *dPtr++ = *sPtr++;

            _samplePos += max;

            return max;
        }

        public void Wrap() 
        {
            SamplePosition = _loopStart;
        }

        public void Dispose()
        {
#if RSTMLIB
#else
            if (_sourceMap != null)
            {
                _sourceMap.Dispose();
                _sourceMap = null;
            }
#endif
            if (_allocatedHGlobal != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_allocatedHGlobal);
                _allocatedHGlobal = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }
    }
}
