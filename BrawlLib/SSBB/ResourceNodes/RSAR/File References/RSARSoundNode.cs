using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.Audio;
using BrawlLib.Wii.Audio;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RSARSoundNode : RSAREntryNode, IAudioSource
    {
        internal INFOSoundEntry* Header { get { return (INFOSoundEntry*)WorkingUncompressed.Address; } }
        
        [Browsable(false)]
        internal override int StringId { get { return Header == null ? -1 : (int)Header->_stringId; } }

        public override ResourceType ResourceType { get { return ResourceType.RSARSound; } }

        Sound3DParam _sound3dParam;
        WaveSoundInfo _waveInfo = new WaveSoundInfo();
        StrmSoundInfo _strmInfo = new StrmSoundInfo();
        SeqSoundInfo _seqInfo = new SeqSoundInfo();

        public enum SndType
        {
            //Invalid = 0,

            SEQ = 1,
            STRM = 2,
            WAVE = 3
        }

        public int _fileId;
        public int _playerId;
        public byte _volume;
        public byte _playerPriority;
        public byte _soundType = 3;
        public byte _remoteFilter;
        public byte _panMode;
        public byte _panCurve;
        public byte _actorPlayerId;

        RSARFileNode _soundNode;

        //internal VoidPtr _dataAddr;

        [Browsable(false)]
        public RSARFileNode SoundNode
        {
            get { return _soundNode; }
            set
            {
                if (_soundNode != value)
                    _soundNode = value;
            }
        }
        [Category("a RSAR Sound"), Browsable(true), TypeConverter(typeof(DropDownListRSARFiles))]
        public string SoundFile
        {
            get { return _soundNode == null ? "<null>" : _soundNode._name; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    SoundNode = null;
                else
                {
                    Type e;
                    switch (SoundType)
                    {
                        //case SndType.Invalid: 
                        default: return;
                        case SndType.SEQ: e = typeof(RSEQNode); break;
                        case SndType.STRM: e = typeof(RSTMNode); break;
                        case SndType.WAVE: e = typeof(RWSDNode); break;
                    }
                    RSARFileNode node = null; int t = 0;
                    foreach (RSARFileNode r in RSARNode.Files)
                    {
                        if (r.Name == value) { node = r; break; }
                        t++;
                    }
                    if (node != null)
                    {
                        SoundNode = node;
                        _fileId = t;
                        SignalPropertyChange();
                    }
                }
            }
        }
        RSARBankNode _rbnk 
        {
            get
            {
                if (SoundType == SndType.SEQ && _seqInfo._bankId < RSARNode._infoCache[1].Count)
                    return RSARNode._infoCache[1][_seqInfo._bankId] as RSARBankNode;
                else return null;
            }
        }
        [Category("a RSAR Sound")]
        public int PlayerId { get { return _playerId; } set { _playerId = value; SignalPropertyChange(); } }
        [Category("a RSAR Sound")]
        public byte Volume { get { return _volume; } set { _volume = value; SignalPropertyChange(); } }
        [Category("a RSAR Sound")]
        public byte PlayerPriority { get { return _playerPriority; } set { _playerPriority = value; SignalPropertyChange(); } }
        [Category("a RSAR Sound")]
        public SndType SoundType { get { return (SndType)_soundType; } set { _soundType = ((byte)value).Clamp(1, 3); SignalPropertyChange(); } }
        [Category("a RSAR Sound")]
        public byte RemoteFilter { get { return _remoteFilter; } set { _remoteFilter = value; SignalPropertyChange(); } }
        [Category("a RSAR Sound")]
        public PanMode PanMode { get { return (PanMode)_panMode; } set { _panMode = (byte)value; SignalPropertyChange(); } }
        [Category("a RSAR Sound")]
        public PanCurve PanCurve { get { return (PanCurve)_panCurve; } set { _panCurve = (byte)value; SignalPropertyChange(); } }
        [Category("a RSAR Sound")]
        public byte ActorPlayerId { get { return _actorPlayerId; } set { _actorPlayerId = value; SignalPropertyChange(); } }
        [Category("a RSAR Sound")]
        public int UserParam1 { get { return _p1; } set { _p1 = value; SignalPropertyChange(); } }
        [Category("a RSAR Sound")]
        public int UserParam2 { get { return _p2; } set { _p2 = value; SignalPropertyChange(); } }

        public int _p1, _p2;

        [Flags]
        public enum Sound3DFlags
        {
            NotVolume = 1,
            NotPan = 2,
            NotSurroundPan = 4,
            NotPriority = 8,
            Filter = 16
        }

        [Category("b RSAR Sound 3D Param")]
        public Sound3DFlags Flags { get { return (Sound3DFlags)(uint)_sound3dParam._flags; } set { _sound3dParam._flags = (uint)value; SignalPropertyChange(); } }
        [Category("b RSAR Sound 3D Param")]
        public DecayCurve DecayCurve { get { return (DecayCurve)_sound3dParam._decayCurve; } set { _sound3dParam._decayCurve = (byte)value; SignalPropertyChange(); } }
        [Category("b RSAR Sound 3D Param")]
        public byte DecayRatio { get { return _sound3dParam._decayRatio; } set { _sound3dParam._decayRatio = value; SignalPropertyChange(); } }
        [Category("b RSAR Sound 3D Param")]
        public byte DopplerFactor { get { return _sound3dParam._dopplerFactor; } set { _sound3dParam._dopplerFactor = value; SignalPropertyChange(); } }

        RSEQLabelNode labl;

        [Category("c RSAR Seq Sound Info"), Browsable(true), TypeConverter(typeof(DropDownListRSARInfoSeqLabls))]
        public string SeqLabelEntry
        {
            get { return labl == null ? "<null>" : labl._name; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    _seqInfo._dataOffset = 0;
                else
                {
                    if (SoundNode is RSEQNode)
                        foreach (RSEQLabelNode r in SoundNode.Children)
                            if (r.Name == value)
                            {
                                _seqInfo._dataOffset = r.Id;
                                SignalPropertyChange(); 
                                break;
                            }
                }
            }
        }

        //[Category("c RSAR Seq Sound Info")]
        //public uint SeqEntryId { get { return _seqInfo._dataOffset; } set { _seqInfo._dataOffset = value; SignalPropertyChange(); } }
        //[Category("c RSAR Seq Sound Info")]
        //public uint BankId { get { return _seqInfo._bankId; } set { _seqInfo._bankId = value; SignalPropertyChange(); } }
        [Category("c RSAR Seq Sound Info"), Browsable(true), TypeConverter(typeof(DropDownListRSARFiles))]
        public string BankFile
        {
            get { return _rbnk == null ? "<null>" : _rbnk.TreePath; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    _seqInfo._bankId = -1;
                else
                {
                    RSARBankNode node = null; int t = 0;
                    foreach (ResourceNode r in RSARNode._infoCache[1])
                    {
                        if (r.Name == value && r is RSARBankNode) { node = r as RSARBankNode; break; }
                        t++;
                    }
                    if (node != null)
                    {
                        _seqInfo._bankId = t;
                        SignalPropertyChange();
                    }
                }
            }
        }
        [Category("c RSAR Seq Sound Info"), TypeConverter(typeof(Bin32StringConverter))]
        public Bin32 SeqAllocTrack { get { return new Bin32(_seqInfo._allocTrack); } set { _seqInfo._allocTrack = value._data; SignalPropertyChange(); } }
        [Category("c RSAR Seq Sound Info")]
        public byte SeqChannelPriority { get { return _seqInfo._channelPriority; } set { _seqInfo._channelPriority = value; SignalPropertyChange(); } }
        [Category("c RSAR Seq Sound Info")]
        public byte SeqReleasePriorityFix { get { return _seqInfo._releasePriorityFix; } set { _seqInfo._releasePriorityFix = value; SignalPropertyChange(); } }
        
        [Category("d RSAR Strm Sound Info")]
        public uint StartPosition { get { return _strmInfo._startPosition; } set { _strmInfo._startPosition = value; SignalPropertyChange(); } }
        [Category("d RSAR Strm Sound Info")]
        public ushort AllocChannelCount { get { return _strmInfo._allocChannelCount; } set { _strmInfo._allocChannelCount = value; SignalPropertyChange(); } }
        [Category("d RSAR Strm Sound Info")]
        public ushort AllocTrackFlag { get { return _strmInfo._allocTrackFlag; } set { _strmInfo._allocTrackFlag = value; SignalPropertyChange(); } }

        public RWSDDataNode _dataNode;

        [Category("e RSAR Wave Sound Info"), Browsable(true), TypeConverter(typeof(DropDownListRSARInfoSound))]
        public string SoundDataNode
        {
            get { return _dataNode == null ? "<null>" : _dataNode._name; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _waveInfo._soundIndex = -1;
                    _dataNode._refs.Remove(this);
                    _dataNode = null;
                }
                else
                {
                    if (SoundNode == null || SoundType != SndType.WAVE) return;

                    RWSDDataNode node = null; int t = 0;
                    if (value == null) goto skip;
                    foreach (RWSDDataNode r in SoundNode.Children[0].Children)
                    {
                        if (r.Name == value) { node = r; break; }
                        t++;
                    }
                skip:
                    if (node != null)
                    {
                        _waveInfo._soundIndex = t;
                        _dataNode._refs.Remove(this);
                        _dataNode = node;
                        _dataNode._refs.Add(this);
                    }
                    else
                    {
                        _waveInfo._soundIndex = -1;
                        _dataNode = null;
                    }
                    SignalPropertyChange();
                }
            }
        }
        //[Category("e RSAR Wave Sound Info")]
        //public int PackIndex { get { return _waveInfo._soundIndex; } set { _waveInfo._soundIndex = value; SignalPropertyChange(); } }
        [Category("e RSAR Wave Sound Info")]
        public uint AllocTrack { get { return _waveInfo._allocTrack; } set { _waveInfo._allocTrack = value; SignalPropertyChange(); } }
        [Category("e RSAR Wave Sound Info")]
        public byte ChannelPriority { get { return _waveInfo._channelPriority; } set { _waveInfo._channelPriority = value; SignalPropertyChange(); } }
        [Category("e RSAR Wave Sound Info")]
        public byte ReleasePriorityFix { get { return _waveInfo._releasePriorityFix; } set { _waveInfo._releasePriorityFix = value; SignalPropertyChange(); } }

        [Category("Audio Stream")]
        public WaveEncoding Encoding { get { return _dataNode == null ? WaveEncoding.ADPCM : _dataNode.Encoding; } }
        [Category("Audio Stream")]
        public int Channels { get { return _dataNode == null ? 0 : _dataNode.Channels; } }
        [Category("Audio Stream")]
        public bool IsLooped { get { return _dataNode == null ? false : _dataNode.IsLooped; } }
        [Category("Audio Stream")]
        public int SampleRate { get { return _dataNode == null ? 0 : _dataNode.SampleRate; } }
        [Category("Audio Stream")]
        public int LoopStartSample { get { return _dataNode == null ? 0 : _dataNode.LoopStartSample; } }
        [Category("Audio Stream")]
        public int NumSamples { get { return _dataNode == null ? 0 : _dataNode.NumSamples; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _fileId = Header->_fileId;
            _playerId = Header->_playerId;
            _volume = Header->_volume;
            _playerPriority = Header->_playerPriority;
            _soundType = Header->_soundType;
            _remoteFilter = Header->_remoteFilter;
            _panMode = Header->_panMode;
            _panCurve = Header->_panCurve;
            _actorPlayerId = Header->_actorPlayerId;
            _p1 = Header->_userParam1;
            _p2 = Header->_userParam2;

            INFOHeader* info = RSARNode.Header->INFOBlock;
            _sound3dParam = *Header->GetParam3dRef(&info->_collection);

            VoidPtr addr = Header->GetSoundInfoRef(&info->_collection);
            switch (Header->_soundInfoRef._dataType)
            {
                case 1:
                    _seqInfo = *(SeqSoundInfo*)addr;
                    break;
                case 2:
                    _strmInfo = *(StrmSoundInfo*)addr;
                    break;
                case 3:
                    _waveInfo = *(WaveSoundInfo*)addr;
                    break;
            }

            _soundNode = RSARNode.Files[_fileId];
            _soundNode.AddSoundRef(this);

            if (SoundNode is RSEQNode)
                foreach (RSEQLabelNode r in SoundNode.Children)
                    if (_seqInfo._dataOffset == r.Id) { labl = r; break; }

            if (SoundType == SndType.WAVE && _soundNode != null && !(_soundNode is RSARExtFileNode) && _soundNode.Children[0].Children.Count > _waveInfo._soundIndex && _waveInfo._soundIndex >= 0)
            {
                _dataNode = _soundNode.Children[0].Children[_waveInfo._soundIndex] as RWSDDataNode;
                if (_dataNode != null)
                    _dataNode._refs.Add(this);
            }

            return false;
        }
        public IAudioStream _stream;
        public IAudioStream[] CreateStreams()
        {
            _stream = null;
            if (_soundNode is RWSDNode)
            {
                RWSDDataNode d = _dataNode as RWSDDataNode;
                if (d != null && _soundNode.Children.Count > 1 && _soundNode.Children[1].Children.Count > d._part3._waveIndex && d._part3._waveIndex >= 0)
                    _stream = (_soundNode.Children[1].Children[d._part3._waveIndex] as RSARFileAudioNode).CreateStreams()[0];
            }
            return new IAudioStream[] { _stream };
        }

        public override int OnCalculateSize(bool force)
        {
            int size = INFOSoundEntry.Size + Sound3DParam.Size;
            switch (SoundType)
            {
                case RSARSoundNode.SndType.SEQ:
                    size += SeqSoundInfo.Size;
                    break;
                case RSARSoundNode.SndType.STRM:
                    size += StrmSoundInfo.Size;
                    break;
                case RSARSoundNode.SndType.WAVE:
                    size += WaveSoundInfo.Size;
                    break;
            }
            return size;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            INFOSoundEntry* header = (INFOSoundEntry*)address;
            VoidPtr addr = address + INFOSoundEntry.Size;
            header->_stringId = _rebuildStringId;
            header->_fileId = _fileId;
            header->_playerId = _playerId;
            header->_volume = _volume;
            header->_playerPriority = _playerPriority;
            header->_soundType = _soundType;
            header->_remoteFilter = _remoteFilter;
            header->_panMode = _panMode;
            header->_panCurve = _panCurve;
            header->_actorPlayerId = _actorPlayerId;
            header->_soundInfoRef = (uint)(addr - _rebuildBase);
            header->_userParam1 = _p1;
            header->_userParam2 = _p2;
            switch (SoundType)
            {
                case RSARSoundNode.SndType.SEQ:
                    *(SeqSoundInfo*)addr = _seqInfo;
                    header->_soundInfoRef._dataType = 1;
                    addr += SeqSoundInfo.Size;
                    break;
                case RSARSoundNode.SndType.STRM:
                    *(StrmSoundInfo*)addr = _strmInfo;
                    header->_soundInfoRef._dataType = 2;
                    addr += StrmSoundInfo.Size;
                    break;
                case RSARSoundNode.SndType.WAVE:
                    *(WaveSoundInfo*)addr = _waveInfo;
                    header->_soundInfoRef._dataType = 3;
                    addr += WaveSoundInfo.Size;
                    break;
            }
            header->_param3dRefOffset = (uint)(addr - _rebuildBase);
            *(Sound3DParam*)addr = _sound3dParam;
        }

        public override unsafe void Export(string outPath)
        {
            if (outPath.EndsWith(".wav"))
                WAV.ToFile(CreateStreams()[0], outPath);
            else
                base.Export(outPath);
        }
    }
}
