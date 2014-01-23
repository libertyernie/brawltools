using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using Ikarus;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class AttributeList : MovesetEntry
    {
        [Browsable(false)]
        public UnsafeBuffer AttributeBuffer { get { if (_buffer != null) return _buffer; else return _buffer = new UnsafeBuffer(0x2E4); } }
        private UnsafeBuffer _buffer;

        public override void Parse(VoidPtr address)
        {
            _buffer = new UnsafeBuffer(0x2E4);
            Memory.Move(_buffer.Address, address, 0x2E4);
        }
        protected override int OnGetSize()
        {
            _lookupCount = 0;
            return 0x2E4;
        }
        protected override void OnWrite(VoidPtr address)
        {
            _rebuildAddr = address;
            Memory.Move(address, _buffer.Address, 0x2E4);
        }

        public void SetFloat(int index, float value)
        {
            if (((bfloat*)AttributeBuffer.Address)[index] != value)
            {
                ((bfloat*)AttributeBuffer.Address)[index] = value;
                SignalPropertyChange();
            }
        }
        public float GetFloat(int index)
        {
            return ((bfloat*)AttributeBuffer.Address)[index];
        }
        public void SetInt(int index, int value)
        {
            if (((bint*)AttributeBuffer.Address)[index] != value)
            {
                ((bint*)AttributeBuffer.Address)[index] = value;
                SignalPropertyChange();
            }
        }
        public int GetInt(int index)
        {
            return ((bint*)AttributeBuffer.Address)[index];
        }
    }
}
