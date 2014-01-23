using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using Ikarus;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class Unknown22 : MovesetEntry
    {
        int _unk1, _unk2, _actionOffset;
        Script _script;

        [Category("Unknown Offset 22")]
        public int Unknown1 { get { return _unk1; } set { _unk1 = value; SignalPropertyChange(); } }
        [Category("Unknown Offset 22")]
        public int Unknown2 { get { return _unk2; } set { _unk2 = value; SignalPropertyChange(); } }
        [Category("Unknown Offset 22")]
        public int ActionOffset { get { return _actionOffset; } }

        public override void Parse(VoidPtr address)
        {
            sDataUnknown22* hdr = (sDataUnknown22*)address;
            _unk1 = hdr->_unk1;
            _unk2 = hdr->_unk2;
            _actionOffset = hdr->_actionOffset;

            if (_actionOffset > 0)
                _script = Parse<Script>(_actionOffset);
        }

        protected override int OnGetSize()
        {
            _lookupCount = 0;
            int size = 12;
            if (_script != null)
            {
                //size += _script.GetSize();
                if (_script.Count > 0)
                    _lookupCount = _script._lookupCount + 1;
            }
            return size;
        }

        protected override void OnWrite(VoidPtr address)
        {
            _rebuildAddr = address;

            sDataUnknown22* data = (sDataUnknown22*)address;
            data->_unk1 = _unk1;
            data->_unk2 = _unk2;

            if (_script != null && _script.Count > 0)
            {
                data->_actionOffset = Offset(_script._rebuildAddr);
                _lookupOffsets.Add(&data->_actionOffset);
            }
        }
    }
}