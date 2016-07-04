using System;
using BrawlLib.SSBBTypes;
using System.IO;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class PACKNode : ResourceNode
    {
        internal PACKHeader* Header { get { return (PACKHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.PACK; } }

        public override void OnPopulate()
        {
            bint* dataOffsets = Header->DataOffsets;
            bint* stringOffsets = Header->StringOffsets;
            bint* lengths = Header->Lengths;

            ResourceNode entry = null;
            for (int i = 0; i < Header->_fileCount; ++i, dataOffsets++, stringOffsets++, lengths++)
            {
                if ((entry = NodeFactory.FromAddress(this, (VoidPtr)Header + *dataOffsets, *lengths)) == null)
                    (entry = new ARCEntryNode()).Initialize(this, (VoidPtr)Header + *dataOffsets, *lengths);
                entry._name = new String((sbyte*)Header + *stringOffsets);
            }
        }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            _name = Path.GetFileNameWithoutExtension(_origPath);
            return Header->_fileCount > 0;
        }

        public override int OnCalculateSize(bool force)
        {
            int size = 0x10 + (Children.Count * 12);
            int dataLen = 0;
            foreach (ResourceNode node in Children)
            {
                size += node.Name.Length + 1;
                dataLen += node.CalculateSize(force).Align(0x10);
            }
            return size.Align(0x10) + dataLen;
        }

        public override void OnRebuild(VoidPtr address, int size, bool force)
        {
            PACKHeader* header = (PACKHeader*)address;
            header->_pad1 = 0;
            header->_pad2 = 0;
            header->_tag = PACKHeader.Tag;
            header->_fileCount = (uint)Children.Count;

            bint* dataOffsets = header->DataOffsets;
            bint* stringOffsets = header->StringOffsets;
            bint* lengths = header->Lengths;

            sbyte* data = (sbyte*)address + 0x10 + Children.Count * 12;
            foreach (ResourceNode node in Children)
            {
                *stringOffsets++ = data - address;
                node._name.Write(ref data);
            }
            int len = data - address;
            len = len.Align(0x10) - len;
            while (len-- > 0)
                *data++ = 0;

            foreach (ResourceNode node in Children)
            {
                *lengths++ = node._calcSize;
                *dataOffsets++ = data - address;

                node.Rebuild(data, node._calcSize, force);
                data += node._calcSize.Align(0x10);
            }
        }

        internal static ResourceNode TryParse(DataSource source)
        {
            return *(BinTag*)source.Address == PACKHeader.Tag ? new PACKNode() : null;
        }

        public void ExtractToFolder(string outFolder)
        {
            if (!Directory.Exists(outFolder))
                Directory.CreateDirectory(outFolder);

            foreach (ResourceNode entry in Children)
                entry.Export(outFolder);
        }

        public void ReplaceFromFolder(string path)
        {
            
        }

        public void ImportFromFolder(string path)
        {
            
        }
    }
}
