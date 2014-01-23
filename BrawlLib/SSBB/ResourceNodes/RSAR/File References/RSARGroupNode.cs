using System;
using BrawlLib.SSBBTypes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RSARGroupNode : RSAREntryNode
    {
        internal INFOGroupHeader* Header { get { return (INFOGroupHeader*)WorkingUncompressed.Address; } }
        [Browsable(false)]
        internal override int StringId { get { return Header == null ? -1 : (int)Header->_stringId; } }

        public override ResourceType ResourceType { get { return ResourceType.RSARGroup; } }

        internal List<RSARFileNode> _files = new List<RSARFileNode>();

        private int _entryNo;
        private int _extFilePathRef;

        [Category("Group")]
        public int EntryNumber { get { return _entryNo; } }
        [Category("Group")]
        public int ExtFilePathRef { get { return _extFilePathRef; } }

        public List<RSARFileNode> Files { get { return _files; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _entryNo = Header->_entryNum;
            _extFilePathRef = Header->_extFilePathRef;

            //Get file references
            RSARNode rsar = RSARNode;
            VoidPtr offset = &rsar.Header->INFOBlock->_collection;
            RuintList* list = Header->GetCollection(offset);
            int count = list->_numEntries;
            for (int i = 0; i < count; i++)
            {
                INFOGroupEntry* entry = (INFOGroupEntry*)list->Get(offset, i);
                int id = entry->_fileId;
                _files.Add(rsar.Files[id] as RSARFileNode);
                rsar.Files[id]._groups.Add(this);
            }

            SetSizeInternal(INFOGroupHeader.Size + 4 + _files.Count * (8 + INFOGroupEntry.Size));

            return false;
        }

        public override int OnCalculateSize(bool force)
        {
            return INFOGroupHeader.Size + 4 + _files.Count * (8 + INFOGroupEntry.Size);
        }

        internal INFOGroupHeader* _rebuildAddr;
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            INFOGroupHeader* header = (INFOGroupHeader*)address;
            _rebuildAddr = header;
            RuintList* list = (RuintList*)(address + INFOGroupHeader.Size);
            INFOGroupEntry* entries = (INFOGroupEntry*)((VoidPtr)list + 4 + _files.Count * 8);
            
            header->_entryNum = -1;
            header->_stringId = _rebuildStringId;
            //header->_extFilePathRef = 0;
            //header->_extFilePathRef._dataType = 0;
            //header->_headerLength = 0;
            //header->_headerOffset = 0;
            //header->_waveDataLength = 0;
            //header->_waveDataOffset = 0;
            header->_listOffset = (uint)((VoidPtr)list - _rebuildBase);

            list->_numEntries = _files.Count;
            int i = 0;
            foreach (RSARFileNode file in _files)
            {
                list->Entries[i] = (uint)((VoidPtr)(&entries[i]) - _rebuildBase);
                entries[i++]._fileId = file._fileIndex;
                //entries[i]._dataLength = 0;
                //entries[i]._dataOffset = 0;
                //entries[i]._headerLength = 0;
                //entries[i]._headerOffset = 0;
            }
        }
    }
}
