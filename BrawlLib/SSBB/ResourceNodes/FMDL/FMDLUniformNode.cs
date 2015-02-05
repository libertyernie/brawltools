using BrawlLib.SSBBTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class FMDLUniformNode : FMDLEntryNode
    {
        internal FMDLUniform* Header { get { return (FMDLUniform*)WorkingUncompressed.Address; } }

        public ushort _unk1; //1
        public ushort _unk2; //0
        public float _value;

        [Category("Shader Uniform")]
        public float Value { get { return _value; } set { _value = value; SignalPropertyChange(); } }
        [Category("Shader Uniform")]
        public ushort Unk1 { get { return _unk1; } set { _unk1 = value; SignalPropertyChange(); } }
        [Category("Shader Uniform")]
        public ushort Unk2 { get { return _unk2; } set { _unk2 = value; SignalPropertyChange(); } }

        public override bool OnInitialize()
        {
            _name = Header->VariableName;
            _unk1 = Header->_unk1;
            _unk2 = Header->_unk2;
            _value = Header->_value;

            return false;
        }
    }
}
