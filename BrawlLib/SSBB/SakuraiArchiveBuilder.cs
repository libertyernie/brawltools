using BrawlLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBBTypes
{
    public class SakuraiArchiveBuilder
    {
        public SakuraiArchiveBuilder(SakuraiArchiveNode node)
        {
            _rootNode = node;
        }

        public SakuraiArchiveNode RootNode { get { return _rootNode; } }
        public bool IsCalculatingSize { get { return _calculatingSize; } }
        public bool IsRebuilding { get { return _rebuilding; } }

        int _size;

        SakuraiArchiveNode _rootNode;
        bool _rebuilding, _calculatingSize;
        CompactStringTable _referenceStringTable;

        public List<SakuraiEntryNode> _postProcessNodes;
        public VoidPtr _baseAddress;
        public LookupManager _lookupManager;
        public int _lookupCount = 0, _lookupLen = 0;

        public int _sectionCount, _referenceCount;

        public int GetSize()
        {
            _calculatingSize = true;

            //Reset variables
            _lookupCount = _lookupLen = 0;
            _lookupManager = new LookupManager();
            _postProcessNodes = new List<SakuraiEntryNode>();
            _referenceStringTable = new CompactStringTable();

            //Add header size
            _size = SakuraiArchiveHeader.Size;

            //Calculate the size of each section and add names to string table
            foreach (TableEntryNode section in _rootNode.SectionList)
            {
                SakuraiEntryNode entry = section;

                //Sections *usually* have only one reference in data or dataCommon
                //An example of an exception to this is the 'AnimCmd' section
                //If a reference exists, calculate the size of that reference instead
                if (section.References.Count > 0)
                {
                    entry = section.References[0];

                    //Table entries don't use lookup table?
                    //TODO: double check this
                    //lookupCount++;
                }

                //Add the size of the entry's data, and the entry itself
                _size += entry.GetSize() + 8;

                //Add the lookup count calculated in GetSize()
                _lookupCount += entry._lookupCount;

                //Add the section's name to the string table
                _referenceStringTable.Add(entry.Name);
            }

            //Calculate reference table size and add names to string table
            foreach (TableEntryNode reference in _rootNode.ReferenceList)
            {
                if (reference.References.Count > 0)
                {
                    //Add entry name to string table
                    _referenceStringTable.Add(reference.Name);

                    //Add entry size (reference don't have any actual 'data' size)
                    _size += 8;
                }

                //Table entries don't use lookup table?
                //TODO: double check this
                //lookupCount += e.References.Count - 1;
            }

            _calculatingSize = false;

            //Add the lookup size and reference table size
            _size += (_lookupLen = _lookupCount * 4) + _referenceStringTable.TotalSize;

            return _size;
        }

        public unsafe void Write(SakuraiArchiveNode node, VoidPtr address, int length)
        {
            _baseAddress = address;

            SakuraiArchiveHeader* hdr = (SakuraiArchiveHeader*)address;
            hdr->_externalSubRoutineCount = _sectionCount;

            foreach (TableEntryNode e in node.SectionList)
            {

            }
        }
    }
}
