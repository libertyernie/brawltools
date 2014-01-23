using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct RSARHeader
    {
        public const int Size = 0x40;
        public const uint Tag = 0x52415352;

        public NW4RCommonHeader _header;

        public bint _symbOffset;
        public bint _symbLength;
        public bint _infoOffset;
        public bint _infoLength;
        public bint _fileOffset;
        public bint _fileLength;
        private int _pad1, _pad2, _pad3, _pad4, _pad5, _pad6;

        public void Set(int symbLen, int infoLen, int fileLen, byte vMinor)
        {
            int offset = 0x40;

            _header._tag = Tag;
            _header.Endian = Endian.Big;
            _header._version = (ushort)(0x100 + vMinor);
            _header._firstOffset = 0x40;
            _header._numEntries = 3;

            _symbOffset = offset;
            _symbLength = symbLen;
            _infoOffset = offset += symbLen;
            _infoLength = infoLen;
            _fileOffset = offset += infoLen;
            _fileLength = fileLen;

            _header._length = offset + fileLen;
        }

        private VoidPtr Address { get { fixed (RSARHeader* ptr = &this)return ptr; } }

        public SYMBHeader* SYMBBlock { get { return (SYMBHeader*)_header.Entries[0].Address; } }
        public INFOHeader* INFOBlock { get { return (INFOHeader*)_header.Entries[1].Address; } }
        public FILEHeader* FILEBlock { get { return (FILEHeader*)_header.Entries[2].Address; } }
    }

    #region SYMB

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct SYMBHeader
    {
        public const uint Tag = 0x424D5953;

        public SSBBEntryHeader _header;
        public bint _stringOffset;

        public bint _maskOffset1; //For sounds
        public bint _maskOffset2; //For types
        public bint _maskOffset3; //For groups
        public bint _maskOffset4; //For banks

        public SYMBHeader(int length)
        {
            _header._tag = Tag;
            _header._length = length;
            _stringOffset = 0x14;
            _maskOffset1 = _maskOffset2 = _maskOffset3 = _maskOffset4 = 0;
        }

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        //public VoidPtr StringData { get { return Address + 8 + _stringOffset; } }
        public SYMBMaskHeader* MaskData1 { get { return (SYMBMaskHeader*)(Address + 8 + _maskOffset1); } }
        public SYMBMaskHeader* MaskData2 { get { return (SYMBMaskHeader*)(Address + 8 + _maskOffset2); } }
        public SYMBMaskHeader* MaskData3 { get { return (SYMBMaskHeader*)(Address + 8 + _maskOffset3); } }
        public SYMBMaskHeader* MaskData4 { get { return (SYMBMaskHeader*)(Address + 8 + _maskOffset4); } }

        public uint StringCount { get { return StringOffsets[-1]; } }
        public buint* StringOffsets { get { return (buint*)(Address + 8 + _stringOffset + 4); } }

        //Gets names of file paths seperated by an underscore
        public string GetStringEntry(int index)
        {
            if (index < 0) 
                return "<null>";
            return new String(GetStringEntryAddr(index));
        }
        public sbyte* GetStringEntryAddr(int index)
        {
            return (sbyte*)(Address + 8 + StringOffsets[index]);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SYMBMaskHeader
    {
        public bint _rootId; //index of the first entry non leafed entry with the lowest bit value
        public bint _numEntries;

        private VoidPtr Address { get { fixed (SYMBMaskHeader* ptr = &this)return ptr; } }
        public SYMBMaskEntry* Entries { get { return (SYMBMaskEntry*)(Address + 8); } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SYMBMaskEntry //Like a ResourceEntry
    {
        public const int Size = 0x14;

        public bushort _flags;
        public bshort _bit; //ResourceEntry _id
        public bint _leftId; //ResourceEntry _leftIndex
        public bint _rightId; //ResourceEntry _rightIndex
        public bint _stringId;
        public bint _index;

        public SYMBMaskEntry(short bit, int left, int right) : this(0, bit, left, right, 0, 0) { }
        public SYMBMaskEntry(ushort flags, short bit, int left, int right, int id, int index)
        {
            _flags = flags; 
            _bit = bit;
            _leftId = left;
            _rightId = right; 
            _stringId = id; 
            _index = index; 
        }

        private SYMBMaskEntry* Address { get { fixed (SYMBMaskEntry* ptr = &this)return ptr; } }

        public SYMBMaskHeader* Parent
        {
            get
            {
                SYMBMaskEntry* entry = Address;
                while (entry->_flags != 1 && entry[-1]._flags != 1) entry--;
                return (SYMBMaskHeader*)((VoidPtr)(--entry) - 8);
            }
        }

    //    public static void Build(SYMBMaskHeader* group, int index)
    //    {
    //        //Get the first entry in the group, which is empty
    //        SYMBMaskEntry* list = group->Entries;
    //        //Get the entry that will be modified
    //        SYMBMaskEntry* entry = &list[index];
    //        //Get the first entry again
    //        SYMBMaskEntry* prev = &list[0];
    //        //Get the entry that the first entry's left index points to
    //        SYMBMaskEntry* current = &list[prev->_leftId];
    //        //The index of the current entry
    //        int currentIndex = prev->_leftId;

    //        bool isRight = false;

    //        //Get the length of the string
    //        int strLen = pString->_length;

    //        //Create a byte pointer to the struct's string data
    //        byte* pChar = (byte*)pString + 4, sChar;

    //        int eIndex = strLen - 1, eBits = pChar[eIndex].CompareBits(0), val;
    //        *entry = new ResourceEntry((eIndex << 3) | eBits, index, index, (int)dataAddress - (int)group, (int)pChar - (int)group);

    //        //Continue while the previous id is greater than the current. Loop backs will stop the processing.
    //        //Continue while the entry id is less than or equal the current id. Being higher than the current id means we've found a place to insert.
    //        while ((entry->_bit <= current->_bit) && (prev->_bit > current->_bit))
    //        {
    //            if (entry->_bit == current->_bit)
    //            {
    //                sChar = (byte*)group + current->_stringOffset;

    //                //Rebuild new id relative to current entry
    //                for (eIndex = strLen; (eIndex-- > 0) && (pChar[eIndex] == sChar[eIndex]); ) ;
    //                eBits = pChar[eIndex].CompareBits(sChar[eIndex]);

    //                entry->_bit = (ushort)((eIndex << 3) | eBits);

    //                if (((sChar[eIndex] >> eBits) & 1) != 0)
    //                {
    //                    entry->_leftId = (ushort)index;
    //                    entry->_rightId = currentIndex;
    //                }
    //                else
    //                {
    //                    entry->_leftId = currentIndex;
    //                    entry->_rightId = (ushort)index;
    //                }
    //            }

    //            //Is entry to the right or left of current?
    //            isRight = ((val = current->_bit >> 3) < strLen) && (((pChar[val] >> (current->_bit & 7)) & 1) != 0);

    //            prev = current;
    //            current = &list[currentIndex = (isRight) ? current->_rightId : current->_leftId];
    //        }

    //        sChar = (current->_stringOffset == 0) ? null : (byte*)group + current->_stringOffset;
    //        val = sChar == null ? 0 : (int)(*(bint*)(sChar - 4));

    //        if ((val == strLen) && (((sChar[eIndex] >> eBits) & 1) != 0))
    //            entry->_rightId = currentIndex;
    //        else
    //            entry->_leftId = currentIndex;

    //        if (isRight)
    //            prev->_rightId = (ushort)index;
    //        else
    //            prev->_leftId = (ushort)index;
    //    }
    }

    #endregion

    #region INFO

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct INFOHeader
    {
        public const uint Tag = 0x4F464E49;

        public SSBBEntryHeader _header;
        public RuintCollection _collection;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public RuintList* Sounds { get { return (RuintList*)_collection[0]; } } //INFOSoundEntry
        public RuintList* Banks { get { return (RuintList*)_collection[1]; } } //INFOBankEntry
        public RuintList* PlayerInfo { get { return (RuintList*)_collection[2]; } } //INFOPlayerInfoEntry
        public RuintList* Files { get { return (RuintList*)_collection[3]; } } //INFOFileEntry
        public RuintList* Groups { get { return (RuintList*)_collection[4]; } } //INFOGroupHeader
        public INFOFooter* Footer { get { return (INFOFooter*)_collection[5]; } }
        
        public INFOSoundEntry* GetSound(int i) { return (INFOSoundEntry*)(_collection.Address + (*Sounds)[i]); }
        public INFOBankEntry* GetBank(int i) { return (INFOBankEntry*)(_collection.Address + (*Banks)[i]); }
        public INFOPlayerInfoEntry* GetPlayerInfo(int i) { return (INFOPlayerInfoEntry*)(_collection.Address + (*PlayerInfo)[i]); }
        public INFOFileEntry* GetFile(int i) { return (INFOFileEntry*)(_collection.Address + (*Files)[i]); }
        public INFOGroupHeader* GetGroup(int i) { return (INFOGroupHeader*)(_collection.Address + (*Groups)[i]); }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct INFOFooter
    {
        public const int Size = 0x10;

        public bushort _seqSoundCount;
        public bushort _seqTrackCount;
        public bushort _strmSoundCount;
        public bushort _strmTrackCount;
        public bushort _strmChannelCount;
        public bushort _waveSoundCount;
        public bushort _waveTrackCount;
        public bushort _padding;
        public buint _reserved;
    }

    #region Sounds
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct INFOSoundEntry
    {
        public const int Size = 0x2C;

        public bint _stringId;
        public bint _fileId;
        public bint _playerId; // 0
        public ruint _param3dRefOffset;
        public byte _volume; //0x20
        public byte _playerPriority; //0x40
        public byte _soundType;
        public byte _remoteFilter; //0x00
        public ruint _soundInfoRef; //dataType: 0 = null, 1 = SeqSoundInfo, 2 = StrmSoundInfo, 3 = WaveSoundInfo
        public bint _userParam1;
        public bint _userParam2;
        public byte _panMode;
        public byte _panCurve;
        public byte _actorPlayerId;
        public byte _reserved;

        public Sound3DParam* GetParam3dRef(VoidPtr baseAddr) { return (Sound3DParam*)(baseAddr + _param3dRefOffset); }
        public VoidPtr GetSoundInfoRef(VoidPtr baseAddr) { return (baseAddr + _soundInfoRef); }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Sound3DParam
    {
        public const int Size = 0xC;

        public buint _flags;
        public byte _decayCurve;
        public byte _decayRatio;
        public byte _dopplerFactor;
        public byte _padding;
        public buint _reserved;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct SeqSoundInfo
    {
        public const int Size = 0x14;

        public buint _dataOffset;
        public bint _bankId;
        public buint _allocTrack;
        public byte _channelPriority;
        public byte _releasePriorityFix;
        public byte _pad1;
        public byte _pad2;
        public buint _reserved;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct StrmSoundInfo
    {
        public const int Size = 0xC;

        public buint _startPosition;
        public bushort _allocChannelCount; // Prior to version 0x0104, this was a bit flag
        public bushort _allocTrackFlag;
        public buint _reserved;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct WaveSoundInfo
    {
        public const int Size = 0x10;

        public bint _soundIndex;
        public buint _allocTrack;
        public byte _channelPriority;
        public byte _releasePriorityFix;
        public byte _pad1;
        public byte _pad2;
        public buint _reserved;
    }

    #endregion

    #region Banks
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct INFOBankEntry
    {
        public const int Size = 0xC;

        public bint _stringId;
        public bint _fileId;
        public bint _padding;
    }
    #endregion
    
    #region Player Info
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct INFOPlayerInfoEntry
    {
        public const int Size = 0x10;

        public bint _stringId;
        public byte _playableSoundCount;
        public byte _padding; //0
        public bushort _padding2; //0
        public buint _heapSize; //0
        public buint _reserved; //0
    }
    #endregion

    #region Files

    //Files can be a group of raw sounds, sequences, or external audio streams.
    //Audio streams (RSTM) can be loaded as BGMs using external files, referenced by the _stringOffset field.
    //Raw sounds (RWSD) contain sounds used in action scripts (usually mono).
    //Banks (RBNK) contain sounds that are played in sequence during gameplay.
    //Sequences (RSEQ) control the progression of banks.

    //Files can be referenced multiple times using loading groups. The _listOffset field contains a list of those references.
    //When a file is referenced by a group, it is copied to each group's header and data block.

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct INFOFileHeader
    {
        public const int Size = 0x1C;

        public buint _headerLen; //Includes padding. Set to file size if external file.
        public buint _dataLen; //Includes padding. Zero if external file.
        public bint _entryNumber; //-1
        public ruint _stringOffset; //External file path, only for BGMs. Path is relative to sound folder
        public ruint _listOffset; //List of groups this file belongs to. Empty if external file.

        public RuintList* GetList(VoidPtr baseAddr) { return (RuintList*)(baseAddr + _listOffset); }

        public string GetPath(VoidPtr baseAddr) { return (_stringOffset == 0) ? null : new String((sbyte*)(baseAddr + _stringOffset)); }
    }

    //Attached to a RuintList from INFOSetHeader
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct INFOFileEntry
    {
        public const int Size = 0x8;
        
        public bint _groupId;
        public bint _index;

        public int GroupId { get { return _groupId; } set { _groupId = value; } }
        public int Index { get { return _index; } set { _index = value; } }

        public override string ToString()
        {
            return String.Format("[{0}, {1}]", GroupId, Index);
        }
    }

    #endregion

    #region Groups

    //Groups are a collection of sound files.
    //Files can appear in multiple groups, but the data is actually copied to each group.
    //Groups are laid out in two blocks, first the header block, then the data block.
    //The header block holds all the headers belonging to each file, in sequential order.
    //The data block holds all the audio data belonging to each file, in sequential order.
    //Data referenced in the WAVE section is relative to the file's data, not the whole group block.
    //This means that the headers/data can simply be copied without changing anything.

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct INFOGroupHeader
    {
        public const int Size = 0x28;

        public bint _stringId; //string id
        public bint _entryNum; //always -1
        public ruint _extFilePathRef; //0
        public bint _headerOffset; //Absolute offset from RSAR file. //RWSD Location
        public bint _headerLength; //Total length of all headers in contained sets.
        public bint _waveDataOffset; //Absolute offset from RSAR file.
        public bint _waveDataLength; //Total length of all data in contained sets.
        public ruint _listOffset;

        public RuintList* GetCollection(VoidPtr offset) { return (RuintList*)(offset + _listOffset); }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct INFOGroupEntry
    {
        public const int Size = 0x18;

        public bint _fileId;
        public bint _headerOffset; //Offset to data, relative to headerOffset above
        public bint _headerLength; //File data length, excluding labels and audio
        public bint _dataOffset; //Offset to audio, relative to waveDataOffset above
        public bint _dataLength; //Audio length
        public bint _reserved;
        
        public override string ToString()
        {
            return String.Format("[{0:X}]", (uint)_fileId);
        }
    }

    #endregion

    #endregion

    #region FILE

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct FILEHeader //Holds all files directly after this header
    {
        public const uint Tag = 0x454C4946;
        public const int Size = 0x20;

        public SSBBEntryHeader _header;
        private fixed int _padding[6];
        
        public void Set(int length)
        {
            _header._tag = Tag;
            _header._length = length;
        }
    }
    #endregion
}
