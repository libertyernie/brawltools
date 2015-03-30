using BrawlLib.SSBBTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ikarus.MovesetFile
{
    public unsafe class MiscSoundData : ListOffset
    {
        public List<EntryListOffset<IndexValue>> _entries;
        protected override void OnParse(VoidPtr address)
        {
            base.OnParse(address);
            _entries = new List<EntryListOffset<IndexValue>>();
            for (int i = 0; i < Count; i++)
                _entries.Add(Parse<EntryListOffset<IndexValue>>(DataOffset + i * 8, 4));
        }

        protected override int OnGetLookupCount()
        {
            int count = (_entries.Count > 0 ? 1 : 0);
            foreach (EntryListOffset<IndexValue> r in _entries)
                count += (r._entries.Count > 0 ? 1 : 0);
            return count;
        }

        protected override int OnGetSize()
        {
            int size = 8;
            foreach (EntryListOffset<IndexValue> r in _entries)
                size += 8 + r._entries.Count * 4;
            return size;
        }

        protected override void OnWrite(VoidPtr address)
        {
            int sndOff = 0, mainOff = 0;
            foreach (EntryListOffset<IndexValue> r in _entries)
            {
                mainOff += 8;
                sndOff += r._entries.Count * 4;
            }

            //indices
            //sound list offsets
            //header

            bint* indices = (bint*)address;
            sListOffset* sndLists = (sListOffset*)(address + sndOff);
            sListOffset* header = (sListOffset*)((VoidPtr)sndLists + mainOff);

            RebuildAddress = header;

            if (_entries.Count > 0)
            {
                header->_startOffset = Offset(sndLists);
                _lookupOffsets.Add(&header->_startOffset);
            }

            header->_listCount = _entries.Count;

            foreach (EntryListOffset<IndexValue> r in _entries)
            {
                if (r._entries.Count > 0)
                {
                    sndLists->_startOffset = Offset(indices);
                    _lookupOffsets.Add(&sndLists->_startOffset);
                }

                (sndLists++)->_listCount = r._entries.Count;
                foreach (IndexValue b in r._entries)
                {
                    b.RebuildAddress = indices;
                    *indices++ = (int)b;
                }
            }
        }
    }
}
