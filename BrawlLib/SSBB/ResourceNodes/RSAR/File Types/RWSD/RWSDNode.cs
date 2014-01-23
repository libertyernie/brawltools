using System;
using BrawlLib.SSBBTypes;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RWSDNode : RSARFileNode
    {
        internal RWSDHeader* Header { get { return (RWSDHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.RWSD; } }

        protected override void GetStrings(LabelBuilder builder)
        {
            foreach (RWSDDataNode node in Children[0].Children)
                builder.Add((uint)node.Index, node._name);
        }

        //Finds labels using LABL block between header and footer, also initializes array
        protected bool GetLabels(int count)
        {
            RWSDHeader* header = (RWSDHeader*)WorkingUncompressed.Address;
            int len = header->_header._length;
            LABLHeader* labl = (LABLHeader*)((int)header + len);

            if ((WorkingUncompressed.Length > len) && (labl->_tag == LABLHeader.Tag))
            {
                _labels = new LabelItem[count];
                count = labl->_numEntries;
                for (int i = 0; i < count; i++)
                {
                    LABLEntry* entry = labl->Get(i);
                    _labels[i] = new LabelItem() { String = entry->Name, Tag = entry->_id };
                }
                return true;
            }

            return false;
        }

        private void ParseBlocks()
        {
            VoidPtr dataAddr = Header;
            int len = Header->_header._length;
            int total = WorkingUncompressed.Length;

            SetSizeInternal(len);

            //Look for labl block
            LABLHeader* labl = (LABLHeader*)(dataAddr + len);
            if ((total > len) && (labl->_tag == LABLHeader.Tag))
            {
                int count = labl->_numEntries;
                _labels = new LabelItem[count];
                count = labl->_numEntries;
                for (int i = 0; i < count; i++)
                {
                    LABLEntry* entry = labl->Get(i);
                    _labels[i] = new LabelItem() { String = entry->Name, Tag = entry->_id };
                }
                len += labl->_size;
            }

            //Set data source
            if (total > len)
                _audioSource = new DataSource(dataAddr + len, total - len);
        }

        public override void GetName()
        {
            base.GetName();
            foreach (RWSDDataNode n in Children[0].Children)
            {
                string closestMatch = "";
                foreach (string s in n.References)
                {
                    if (closestMatch == "")
                        closestMatch = s;
                    else
                    {
                        int one = closestMatch.Length;
                        int two = s.Length;
                        int min = Math.Min(one, two);
                        for (int i = 0; i < min; i++)
                            if (Char.ToLower(s[i]) != Char.ToLower(closestMatch[i]) && i > 1)
                            {
                                closestMatch = closestMatch.Substring(0, i - 1);
                                break;
                            }
                    }
                }
                n._name = String.Format("{0}", closestMatch);
            }
        }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            ParseBlocks();

            return true;
        }

        public override void OnPopulate()
        {
            RWSDHeader* rwsd = Header;
            RWSD_DATAHeader* data = rwsd->Data;
            RuintList* list = &data->_list;
            int count = list->_numEntries;

            new RWSDDataGroupNode().Initialize(this, Header->Data, Header->_dataLength);
            if (Header->_waveOffset > 0 && VersionMinor < 3)
                new RWSDSoundGroupNode().Initialize(this, Header->Wave, Header->_waveLength);
            else if (VersionMinor >= 3)
                new RWARNode() { _name = "Audio" }.Initialize(this, _audioSource.Address, _audioSource.Length);

            //Get labels
            RSARNode parent;
            int count2 = Header->Data->_list._numEntries;
            if ((_labels == null) && ((parent = RSARNode) != null))
            {
                //Get them from RSAR
                SYMBHeader* symb2 = parent.Header->SYMBBlock;
                INFOHeader* info = parent.Header->INFOBlock;

                VoidPtr offset = &info->_collection;
                RuintList* soundList2 = info->Sounds;
                count2 = soundList2->_numEntries;

                _labels = new LabelItem[count2];

                INFOSoundEntry* entry;
                for (uint i = 0; i < count2; i++)
                    if ((entry = (INFOSoundEntry*)soundList2->Get(offset, (int)i))->_fileId == _fileIndex)
                    {
                        int x = ((WaveSoundInfo*)entry->GetSoundInfoRef(offset))->_soundIndex;
                        _labels[x] = new LabelItem() { Tag = i, String = symb2->GetStringEntry(entry->_stringId) };
                    }
            }

            for (int i = 0; i < count; i++)
            {
                RWSD_DATAEntry* entry = (RWSD_DATAEntry*)list->Get(list, i);
                RWSDDataNode node = new RWSDDataNode() { _name = _labels[i].String };
                node._offset = list;
                node.Initialize(Children[0], entry, 0);
            }
        }

        public override int OnCalculateSize(bool force)
        {
            _audioLen = 0;
            _headerLen = RWSDHeader.Size;
            if (VersionMinor >= 3)
            {
                _headerLen += Children[0].CalculateSize(true);
                _audioLen = Children[1].CalculateSize(true);
            }
            else
            {
                foreach (ResourceNode g in Children)
                    _headerLen += g.CalculateSize(true);
                foreach (WAVESoundNode s in Children[1].Children)
                    _audioLen += s._streamBuffer.Length;
            }

            return _headerLen + _audioLen;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            VoidPtr addr = address + 0x20;

            RWSDHeader* header = (RWSDHeader*)address;
            header->_header._length = _headerLen;
            header->_header._tag = RWSDHeader.Tag;
            header->_header._numEntries = (ushort)(VersionMinor >= 3 ? 1 : 2);
            header->_header._firstOffset = 0x20;
            header->_header.Endian = Endian.Big;
            header->_header._version = (ushort)(0x100 + VersionMinor);
            header->_dataOffset = 0x20;
            header->_dataLength = Children[0]._calcSize;
            header->_waveOffset = 0x20 + Children[0]._calcSize;
            header->_waveLength = Children[1]._calcSize;

            Children[0].Rebuild(addr, Children[0]._calcSize, true);
            addr += Children[0]._calcSize;

            SetSizeInternal(_headerLen);

            if (VersionMinor <= 2)
            {
                header->_waveOffset = 0x20 + Children[0]._calcSize;
                header->_waveLength = Children[1]._calcSize;

                VoidPtr audio = addr;
                if (RSARNode == null)
                    audio += Children[1]._calcSize;
                else 
                    audio = _rebuildAudioAddr;

                (Children[1] as RWSDSoundGroupNode)._audioAddr = audio;
                _audioSource = new DataSource(audio, _audioLen);

                Children[1].Rebuild(addr, Children[1]._calcSize, true);
                addr += Children[1]._calcSize;
            }
            else
            {
                header->_waveOffset = 0;
                header->_waveLength = 0;

                VoidPtr audio = addr;
                if (RSARNode != null)
                    audio = _rebuildAudioAddr;

                _audioSource = new DataSource(audio, _audioLen);
                Children[1].Rebuild(audio, Children[1]._calcSize, true);
            }
        }

        public override void Remove()
        {
            if (RSARNode != null)
                RSARNode.Files.Remove(this);
            base.Remove();
        }

        internal static ResourceNode TryParse(DataSource source) { return ((RWSDHeader*)source.Address)->_header._tag == RWSDHeader.Tag ? new RWSDNode() : null; }
    }
}
