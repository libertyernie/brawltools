using BrawlLib;
using Ikarus.MovesetFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ikarus.MovesetBuilder
{
    partial class NewMovesetBuilder
    {
        public NewMovesetBuilder(MovesetNode node)
        {
            _movesetNode = node;
        }

        public MovesetNode MovesetNode { get { return _movesetNode; } }
        public bool CalculatingSize { get { return _calculatingSize; } }
        public bool Rebuilding { get { return _rebuilding; } }

        private bool _rebuilding, _calculatingSize;
        MovesetNode _movesetNode;

        public List<MovesetEntryNode> _postProcessNodes;
        public VoidPtr _baseAddress;
        public LookupManager _lookupOffsets;
        public int _lookupCount = 0, _lookupLen = 0;
        public int _refCount = 0;
        CompactStringTable _referenceStringTable;

        //Offset - Size
        public Dictionary<int, int> _lookupEntries;


        VoidPtr _dataAddress;

        public int GetSize()
        {
            _calculatingSize = true;

            int size = MovesetHeader.Size;

            _postProcessNodes = new List<MovesetEntryNode>();
            _lookupOffsets = new LookupManager();
            _lookupCount = 0;
            _lookupLen = 0;
            _referenceStringTable = new CompactStringTable();

            //Calculate the size of each section and add names to string table
            foreach (MovesetEntryNode section in _movesetNode.SectionList)
            {
                MovesetEntryNode entry = section;
                if (entry is ExternalEntryNode)
                {
                    ExternalEntryNode ext = entry as ExternalEntryNode;
                    if (ext.References.Count > 0)
                        entry = ext.References[0]; //Sections always have only one reference
                }

                //int s = 0;
                //if (!(entry is RawDataNode))
                //    s = GetSectionSize(entry);
                //else
                //    if (entry.Children.Count > 0)
                //        foreach (MovesetEntryNode n in entry.Children)
                //            s += GetSectionSize(n);
                //    else
                //        s = GetSectionSize(entry);

                //size += s + 8; //Size of entry + data offset size

                _lookupCount += entry._lookupCount;
                _referenceStringTable.Add(entry.Name);
            }

            //Calculate reference table size and add names to string table
            foreach (ExternalEntryNode entry in _movesetNode.ReferenceList)
            {
                //if (entry._references.Count > 0)
                //{
                //    _referenceStringTable.Add(entry.Name);
                //    size += 8;
                //}

                //references don't use lookup table
                //lookupCount += e._refs.Count - 1;
            }

            _calculatingSize = false;

            return size + (_lookupLen = _lookupCount * 4) + _referenceStringTable.TotalSize;
        }
    }
}
