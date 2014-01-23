using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Imaging;
using BrawlLib.Wii.Models;
using System.Collections.Generic;
using System.Linq;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class MDL0ColorNode : MDL0EntryNode
    {
        internal MDL0ColorData* Header { get { return (MDL0ColorData*)WorkingUncompressed.Address; } }
        public MDL0ObjectNode[] Objects { get { return _objects.ToArray(); } }
        internal List<MDL0ObjectNode> _objects = new List<MDL0ObjectNode>();

        [Category("Color Data")]
        public int ID { get { return Header->_index; } }
        [Category("Color Data")]
        public bool IsRGBA { get { return Header->_isRGBA != 0; } }
        [Category("Color Data")]
        public WiiColorComponentType Format { get { return (WiiColorComponentType)(int)Header->_format; } }
        [Category("Color Data")]
        public byte EntryStride { get { return Header->_entryStride; } }
        [Category("Color Data")]
        public int NumEntries { get { return Header->_numEntries; } }

        private List<RGBAPixel> _colors;
        public RGBAPixel[] Colors
        {
            get { return _colors == null && Header != null ? (_colors = ColorCodec.ExtractColors(Header).Select(n => (RGBAPixel)n).ToList()).ToArray() : _colors.ToArray(); }
            set { _colors = value.ToList<RGBAPixel>(); SignalPropertyChange(); }
        }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            if ((_name == null) && (Header->_stringOffset != 0))
                _name = Header->ResourceString;

            return false;
        }

        public ColorCodec _enc;
        public override int OnCalculateSize(bool force)
        {
            if (Model._isImport || _changed)
            {
                _enc = new ColorCodec(Colors);
                return _enc._dataLen.Align(0x20) + 0x20;
            }
            else return base.OnCalculateSize(force);
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            if (Model._isImport || _changed)
            {
                //Write header
                MDL0ColorData* header = (MDL0ColorData*)address;
                header->_dataLen = length;
                header->_dataOffset = 0x20;
                header->_index = _entryIndex;
                header->_isRGBA = _enc._hasAlpha ? 1 : 0;
                header->_format = (int)_enc._outType;
                header->_entryStride = (byte)_enc._dstStride;
                header->_pad = 0;
                header->_numEntries = (ushort)Colors.Length;

                //Write data
                _enc.Write((byte*)header + 0x20);
                _enc.Dispose();
                _enc = null;
            }
            else
                base.OnRebuild(address, length, force);
        }

        protected internal override void PostProcess(VoidPtr mdlAddress, VoidPtr dataAddress, StringTable stringTable)
        {
            MDL0ColorData* header = (MDL0ColorData*)dataAddress;
            header->_mdl0Offset = (int)mdlAddress - (int)dataAddress;
            header->_stringOffset = (int)stringTable[Name] + 4 - (int)dataAddress;
            header->_index = Index;
        }
    }
}
