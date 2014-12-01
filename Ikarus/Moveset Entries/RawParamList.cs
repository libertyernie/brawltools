using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using Ikarus;

namespace Ikarus.MovesetFile
{
    public unsafe class RawParamList : MovesetEntryNode
    {
        public RawParamList() { }
        public RawParamList(int size) { _initSize = size; }

        public List<AttributeInfo> _info;
        public string _nameID;

        public new string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Manager.Params[_nameID]._newName = value;
                Manager._dictionaryChanged = true;
            }
        }

        [Browsable(false)]
        public UnsafeBuffer AttributeBuffer { get { return attributeBuffer; } }
        private UnsafeBuffer attributeBuffer;

        protected override void OnParse(VoidPtr address)
        {
            _nameID = _name;

            if (_initSize == 0)
                throw new Exception("Nothing to read");

            CharacterInfo cInfo = Manager.SelectedInfo;

            SectionParamInfo data = null;
            if (_name != null && cInfo._parameters.ContainsKey(_name))
            {
                data = cInfo._parameters[_name];
                _info = data._attributes;
                if (!String.IsNullOrEmpty(data._newName))
                    _name = data._newName;
            }
            else _info = new List<AttributeInfo>();

            if (_initSize > 0)
            {
                attributeBuffer = new UnsafeBuffer(_initSize);
                byte* pOut = (byte*)attributeBuffer.Address;
                byte* pIn = (byte*)address;

                for (int i = 0; i < _initSize; i++)
                {
                    if (i % 4 == 0)
                    {
                        if (data == null)
                        {
                            AttributeInfo info = new AttributeInfo();

                            //Guess if the value is a an integer or float
                            if (((((uint)*((buint*)pIn)) >> 24) & 0xFF) != 0 && *((bint*)pIn) != -1 && !float.IsNaN(((float)*((bfloat*)pIn))))
                                info._type = 0;
                            else
                                info._type = 1;

                            info._name = (info._type == 1 ? "*" : "") + "0x" + i.ToString("X");
                            info._description = "No Description Available.";

                            _info.Add(info);
                        }
                    }
                    *pOut++ = *pIn++;
                }
            }
        }

        protected override void OnWrite(VoidPtr address)
        {
            RebuildAddress = address;
            byte* pIn = (byte*)attributeBuffer.Address;
            byte* pOut = (byte*)address;
            for (int i = 0; i < attributeBuffer.Length; i++)
                *pOut++ = *pIn++;
        }

        protected override int OnGetSize()
        {
            _lookupCount = 0;
            _entryLength = attributeBuffer.Length;
            return _entryLength;
        }
    }
}
