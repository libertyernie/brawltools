using System;
using BrawlLib.SSBBTypes;
using BrawlLib.Wii.Audio;
using System.Audio;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using System.Linq;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RSTMNode : RSARFileNode, IAudioSource
    {
        internal RSTMHeader* Header { get { return (RSTMHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.RSTM; } }

        int _encoding;
        int _channels;
        bool _looped;
        int _sampleRate;
        int _loopStart;
        int _numSamples;
        int _dataOffset;
        int _numBlocks;
        int _blockSize;
        int _bps;

        [Category("File Node"), Browsable(false)]
        public override int FileNodeIndex { get { return _fileIndex; } }
        [Category("Data"), Browsable(false)]
        public override string AudioLength { get { return _audioSource.Length.ToString("X"); } }
        [Category("Data"), Browsable(false)]
        public override string DataLength { get { return WorkingUncompressed.Length.ToString("X"); } }
        [Browsable(false)]
        public override string[] GroupRefs { get { return _groups.Select(x => x.TreePath).ToArray(); } }
        [Browsable(false)]
        public override string[] EntryRefs { get { return _references.ToArray(); } }

        [Category("Audio Stream")]
        public WaveEncoding Encoding { get { return (WaveEncoding)_encoding; } }
        [Category("Audio Stream")]
        public int Channels { get { return _channels; } }
        [Category("Audio Stream")]
        public bool IsLooped { get { return _looped; } }
        [Category("Audio Stream")]
        public int SampleRate { get { return _sampleRate; } }
        [Category("Audio Stream")]
        public int LoopStartSample { get { return _loopStart; } }
        [Category("Audio Stream")]
        public int NumSamples { get { return _numSamples; } }
        //[Category("Audio Stream")]
        //public int DataOffset { get { return _dataOffset; } }
        [Category("Audio Stream")]
        public int NumBlocks { get { return _numBlocks; } }
        [Category("Audio Stream")]
        public int BlockSize { get { return _blockSize; } }
        [Category("Audio Stream")]
        public int BitsPerSample { get { return _bps; } }

        public IAudioStream[] CreateStreams()
        {
            if (Header != null)
                return ADPCMStream.GetStreams(Header, _audioSource.Address);
            return new IAudioStream[] { null };
        }

        public override bool OnInitialize()
        {
            if ((_name == null) && (_origPath != null))
                _name = Path.GetFileNameWithoutExtension(_origPath);

            base.OnInitialize();

            StrmDataInfo* part1 = Header->HEADData->Part1;

            _encoding = part1->_format._encoding;
            _channels = part1->_format._channels;
            _looped = part1->_format._looped != 0;
            _sampleRate = part1->_sampleRate;
            _loopStart = part1->_loopStartSample;
            _numSamples = part1->_numSamples;
            _dataOffset = part1->_dataOffset;
            _numBlocks = part1->_numBlocks;
            _blockSize = part1->_blockSize;
            _bps = part1->_bitsPerSample;

            int offset = (int)(Header->DATAData->Data - Header);
            if (offset < WorkingUncompressed.Length)
            {
                _audioSource = new DataSource(Header->DATAData->Data, WorkingUncompressed.Length - offset);
                SetSizeInternal(offset);
            }

            return false;
        }

        public override unsafe void Export(string outPath)
        {
            if (outPath.EndsWith(".wav"))
                WAV.ToFile(CreateStreams()[0], outPath);
            else
            {
                if (_audioSource != DataSource.Empty)
                {
                    int size = WorkingUncompressed.Length + _audioSource.Length;
                    using (FileStream stream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        stream.SetLength(size);
                        using (FileMap map = FileMap.FromStreamInternal(stream, FileMapProtect.ReadWrite, 0, size))
                        {
                            VoidPtr addr = map.Address;

                            //Write header
                            Memory.Move(addr, WorkingUncompressed.Address, (uint)WorkingUncompressed.Length);

                            //Set the offset to the audio samples (_dataLocation)
                            RSTMHeader* hdr = (RSTMHeader*)addr;
                            hdr->_header._length = WorkingUncompressed.Length + _audioSource.Length;
                            hdr->DATAData->Set(_audioSource.Length);

                            addr += WorkingUncompressed.Length;

                            //Append audio samples to the end
                            Memory.Move(addr, _audioSource.Address, (uint)_audioSource.Length);
                        }
                    }
                }
                else
                    base.Export(outPath);
            }
        }

        public override unsafe void Replace(string fileName)
        {
            IAudioStream stream = null;

            if (fileName.EndsWith(".wav"))
                stream = WAV.FromFile(fileName);
            else
                base.Replace(fileName);

            if (stream != null)
                try { ReplaceRaw(RSTMConverter.Encode(stream, null)); }
                finally { stream.Dispose(); }

            int offset = (int)(Header->DATAData->Data - Header);
            if (offset < WorkingUncompressed.Length)
            {
                _audioSource = new DataSource(Header->DATAData->Data, WorkingUncompressed.Length - offset);
                SetSizeInternal(offset);
            }
        }

        internal static ResourceNode TryParse(DataSource source) { return ((RSTMHeader*)source.Address)->_header._tag == RSTMHeader.Tag ? new RSTMNode() : null; }
    }
}
