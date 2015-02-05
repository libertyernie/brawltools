using BrawlLib.SSBBTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class FMDLVertexNode : FMDLEntryNode
    {
        internal FVTX* Header { get { return (FVTX*)WorkingUncompressed.Address; } }

        public uint Elements { get { return Header->_elementCount; } }

        public override bool OnInitialize()
        {
            _name = "Vertex" + Index;

            return Header->_attributeCount > 0;
        }

        public override void OnPopulate()
        {
            FVTXAttribute* attrib = Header->AttributeArray;
            FVTXBuffer* buffers = Header->BufferArray;

            for (int x = 0; x < Header->_attributeCount; x++, attrib++)
                new FMDLBufferAttribNode().Initialize(this, attrib, FVTXAttribute.Size);
        }
    }

    public unsafe class FMDLBufferAttribNode : FMDLEntryNode
    {
        internal FVTXAttribute* Header { get { return (FVTXAttribute*)WorkingUncompressed.Address; } }

        public byte BufferIndex { get { return Header->_bufferIndex; } }
        public uint BufferOffset { get { return Header->_bufferOffset; } }
        public byte Type { get { return Header->_type; } }
        public FVTXAttributeFormat Format { get { return Header->Format; } }
        
        public override bool OnInitialize()
        {
            _name = Header->Name;
            //FVTXBuffer* buffer = &buffers[Header->_bufferIndex];
            //int stride = buffer->_stride;
            //VoidPtr pData = buffer->BufferAddress;

            return false;
        }
    }
}
