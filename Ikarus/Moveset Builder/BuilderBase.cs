using BrawlLib.SSBBTypes;
using Ikarus.MovesetFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ikarus.MovesetBuilder
{
    public abstract class BuilderBase
    {
        int GetOffset(VoidPtr address) { return (int)address - (int)_baseAddress; }
        VoidPtr GetAddress(int offset) { return _baseAddress + offset; }
        public int CurrentOffset { get { return GetOffset(_currentAddress); } }

        //Split size calculations up into sections to make debugging easier
        public List<int> _lengths = new List<int>();

        public Action[] _getPartSize, _buildPart;
        public VoidPtr _baseAddress, _currentAddress;
        public int _partIndex, _lookup = 0;
        public MovesetNode _moveset;
        public int _size;

        //This calculates the size of child data only
        //The entry size is calculated in the node's OnGetSize() function
        public virtual int CalcSize()
        {
            //Add data sizes in order of appearance in file.
            //This is just so it's easier to keep track of things
            for (_partIndex = 0; _partIndex < _getPartSize.Length; _partIndex++)
            {
                //Add new starting length for this part
                _lengths.Add(0);

                //Run the function that will calculate this part's size
                //The size of the part will be added to the last added value in _lengths.
                _getPartSize[_partIndex]();
            }

            //Add up all part sizes.
            //Part sizes were stored in _lengths so they can be referred to later when writing the format
            foreach (int len in _lengths)
                _size += len;

            //Return total data size (this does not include the entry's header)
            return _size;
        }
        protected void AddSize(int amt)
        {
            _lengths[_lengths.Count - 1] += amt;
        }
        /// <summary>
        /// If the entry isn't null,
        /// adds the size of the entry to the current part length
        /// and adds the entry's lookup count to the main lookup count.
        /// </summary>
        protected void GetSize(SakuraiEntryNode entry)
        {
            if (entry != null)
            {
                _lengths[_lengths.Count - 1] += entry.GetSize();
                _lookup += entry.GetLookupCount();
            }
        }
        public virtual void Build(VoidPtr address)
        {
            _baseAddress = address;
            _currentAddress = address;

            int calcOffset = 0;
            for (int i = 0; i < _partIndex; i++)
            {
                //Write the part to the address.
                //This function will increment _currentAddress
                _buildPart[i]();

                //Add the previously calculated sizes for debug purposes
                calcOffset += _lengths[i];

                if (CurrentOffset != calcOffset)
                    throw new Exception(String.Format("Part {0} written incorrectly!", i));
            }
        }
        protected int Write(SakuraiEntryNode entry, int incAmt = 0)
        {
            if (entry != null)
            {
                //DEBUG
                if (entry._calcSize == 0 || entry._calcSize != entry.TotalSize)
                    throw new Exception("Entry size issues");

                int offset = entry.Write(_currentAddress);
                _currentAddress += incAmt > 0 ? incAmt : entry._calcSize;
            }
            return 0;
        }
    }
}
