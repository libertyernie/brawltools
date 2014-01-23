using System;
using BrawlLib.IO;
using BrawlLib.SSBBTypes;

namespace System.Audio
{
    public unsafe class PCMStream : IAudioStream
    {
        private FileMap _sourceMap;

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

        internal PCMStream(FileMap map)
        {
            _sourceMap = map;

            RIFFHeader* header = (RIFFHeader*)_sourceMap.Address;
            _bps = header->_fmtChunk._bitsPerSample;
            _numChannels = header->_fmtChunk._channels;
            _frequency = (int)header->_fmtChunk._samplesSec;
            _numSamples = (int)(header->_dataChunk._chunkSize / header->_fmtChunk._blockAlign);

            _source = (short*)(_sourceMap.Address + header->GetSize());
            _samplePos = 0;
        }

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
            if (_sourceMap != null)
            {
                _sourceMap.Dispose();
                _sourceMap = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
