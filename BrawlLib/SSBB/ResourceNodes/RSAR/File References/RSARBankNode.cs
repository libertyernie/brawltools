using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RSARBankNode : RSAREntryNode
    {
        internal INFOBankEntry* Header { get { return (INFOBankEntry*)WorkingUncompressed.Address; } }
        
        [Browsable(false)]
        internal override int StringId { get { return Header == null ? -1 : (int)Header->_stringId; } }

        internal RBNKNode _rbnk;

        [Browsable(false)]
        public RBNKNode BankNode
        {
            get { return _rbnk; }
            set
            {
                if (_rbnk != value)
                    _rbnk = value;
            }
        }
        [Category("INFO Bank"), Browsable(true), TypeConverter(typeof(DropDownListRSARFiles))]
        public string BankFile
        {
            get { return _rbnk == null ? null : _rbnk._name; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    BankNode = null;
                else
                {
                    RBNKNode node = null; int t = 0;
                    foreach (ResourceNode r in RSARNode.Files)
                    {
                        if (r.Name == value && r is RBNKNode) { node = r as RBNKNode; break; }
                        t++;
                    }
                    if (node != null)
                    {
                        BankNode = node;
                        _fileId = t;
                        SignalPropertyChange();
                    }
                }
            }
        }

        public override ResourceType ResourceType { get { return ResourceType.RSARBank; } }

        public int _fileId;

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _fileId = Header->_fileId;

            if (_fileId >= 0 && _fileId < RSARNode.Files.Count)
                _rbnk = RSARNode.Files[_fileId] as RBNKNode;
            if (_rbnk != null)
                _rbnk.AddBankRef(this);

            return false;
        }

        public override int OnCalculateSize(bool force)
        {
            return INFOBankEntry.Size;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            INFOBankEntry* header = (INFOBankEntry*)address;
            header->_stringId = _rebuildStringId;
            header->_fileId = _fileId;
            header->_padding = 0;
        }
    }
}
