using System;
using BrawlLib.SSBBTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Compression;
using System.Windows;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class SARCNode : SARCEntryNode
    {
        internal SARC* Header { get { return (SARC*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.SARC; } }
        public override Type[] AllowedChildTypes { get { return new Type[] { typeof(SARCEntryNode) }; } }

        uint _hashMult = 0x65;
        int _align = 0x2000;
        public List<string> _strings = new List<string>();

        public override bool OnInitialize()
        {
            _name = Path.GetFileNameWithoutExtension(_origPath);
            _hashMult = Header->Entries->_hashMultiplier;
            return Header->Entries->_entryCount > 0;
        }

        public override void OnPopulate()
        {
            VoidPtr dataStart = Header->Data;
            SFAT* entries = Header->Entries;
            SFNT* strings = entries->Strings;
            SFATEntry* entry = entries->Entries;
            for (int i = 0; i < entries->_entryCount; i++, entry++)
            {
                VoidPtr entryAddr = dataStart + entry->_dataOffset;
                DataSource source = new DataSource(entryAddr, (int)(entry->_endOffset - entry->_dataOffset));

                ResourceNode node = null;
                if ((entry->_dataOffset == entry->_endOffset) || ((node = NodeFactory.FromSource(this, source)) == null))
                    (node = new SARCEntryNode()).Initialize(this, source);

                if (node._name == null)
                    node._name = strings->GetString(entry->_stringOffset);
            }
        }
        
        public override int OnCalculateSize(bool force)
        {
            _strings.Clear();
            int size = 0;
            foreach (SARCEntryNode e in Children)
                size += e.CalculateSize(force).Align(_align) + (e.Name.Length + 1).Align(4);
            size += SARC.Size + SFAT.Size + Children.Count * SFATEntry.Size + SFNT.Size;
            return size.Align(_align);
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            SARC* sarc = (SARC*)address;
            sarc->_tag = SARC.Tag;
            sarc->_headerLength = SARC.Size;
            sarc->Endian = Endian.Big;
            sarc->_length = (uint)length;
            sarc->_dataOffset = (uint)_align;
            sarc->_unk1 = 1;

            SFAT* sfat = sarc->Entries;
            sfat->_tag = SFAT.Tag;
            sfat->_headerLength = SFAT.Size;
            sfat->_entryCount = (ushort)Children.Count;
            sfat->_hashMultiplier = _hashMult;

            SFATEntry* entry = sfat->Entries;

            SFNT* sfnt = (SFNT*)(entry + Children.Count);
            sfnt->_tag = SFNT.Tag;
            sfnt->_version = 8;
            sfnt->_pad = 0;

            sbyte* stringStart = sfnt->Data;
            sbyte* pStr = stringStart;
            int offset = _align;
            foreach (SARCEntryNode e in Children)
            {
                string s = e.Name;
                entry->_unknown = 1;
                s.Write(pStr);
                pStr += (s.Length + 1).Align(4);
                entry->_stringOffset = (int)pStr - (int)stringStart / 4;
                entry->CalcHash(s, _hashMult);
                entry->_dataOffset = (uint)offset;

                e.Rebuild(address + offset, e._calcSize, force);

                int end = offset + e._calcSize;
                entry->_endOffset = (uint)end;
                offset = end.Align(_align);
                entry++;
            }
        }

        public void ExtractToFolder(string outFolder)
        {
            if (!Directory.Exists(outFolder))
                Directory.CreateDirectory(outFolder);

            foreach (SARCEntryNode entry in Children)
                if (entry is SARCNode)
                    ((SARCNode)entry).ExtractToFolder(Path.Combine(outFolder, entry.Name));
                else if (entry is BFRESNode)
                    ((BFRESNode)entry).ExportToFolder(outFolder);
        }

        //public void ReplaceFromFolder(string inFolder)
        //{
        //    DirectoryInfo dir = new DirectoryInfo(inFolder);
        //    DirectoryInfo[] dirs;
        //    foreach (SARCEntryNode entry in Children)
        //    {
        //        if (entry is SARCNode)
        //        {
        //            dirs = dir.GetDirectories(entry.Name);
        //            if (dirs.Length > 0)
        //            {
        //                ((SARCNode)entry).ReplaceFromFolder(dirs[0].FullName);
        //                continue;
        //            }
        //        }
        //        else if (entry is BFRESNode)
        //        {
        //            ((BFRESNode)entry).ReplaceFromFolder(inFolder);
        //            continue;
        //        }
        //    }
        //}

        internal static ResourceNode TryParse(DataSource source) { return ((SARC*)source.Address)->_tag == SARC.Tag ? new SARCNode() : null; }
    }

    public unsafe class SARCEntryNode : ResourceNode
    {
        public override ResourceType ResourceType { get { return ResourceType.SARCEntry; }  }

        [Browsable(true), TypeConverter(typeof(DropDownListCompression))]
        public override string Compression
        {
            get { return base.Compression; }
            set { base.Compression = value; }
        }
    }
}
