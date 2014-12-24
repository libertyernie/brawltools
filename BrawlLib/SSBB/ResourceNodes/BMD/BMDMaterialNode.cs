using BrawlLib.SSBBTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class BMDMaterialNode : BMDEntryNode
    {
        internal BMDMaterialEntry3* Header { get { return (BMDMaterialEntry3*)WorkingUncompressed.Address; } }

        public override bool OnInitialize()
        {
            int index = Index;

            BMDMaterialsHeader* mainHdr = (BMDMaterialsHeader*)WorkingUncompressed.Address;

            BMDMaterialEntry3* entry = (BMDMaterialEntry3*)(mainHdr->GetAddress(MatOffsets.MaterialEntries) + index * BMDMaterialEntry3.Size);
            BMDAlphaCompare* alphaFunc = (BMDAlphaCompare*)(mainHdr->GetAddress(MatOffsets.AlphaCompare) + index * BMDAlphaCompare.Size);
            BMDBlendInfo* blend = (BMDBlendInfo*)(mainHdr->GetAddress(MatOffsets.Blend) + index * BMDBlendInfo.Size);
            BMDColorChanInfo* colorChanInfo = (BMDColorChanInfo*)(mainHdr->GetAddress(MatOffsets.ColorChanInfo) + index * BMDColorChanInfo.Size);
            BMDTexGenInfo* texGen = (BMDTexGenInfo*)(mainHdr->GetAddress(MatOffsets.TexGenNum) + index * BMDTexGenInfo.Size);
            BMDTexMtxInfo* texMtx = (BMDTexMtxInfo*)(mainHdr->GetAddress(MatOffsets.TexMatrixInfo) + index * BMDTexMtxInfo.Size);
            //BMDBlendInfo* blend = (BMDBlendInfo*)(mainHdr->GetAddress(MatOffsets.Blend) + index * BMDBlendInfo.Size);
            //BMDColorChanInfo* colorChanInfo = (BMDColorChanInfo*)(mainHdr->GetAddress(MatOffsets.ColorChanInfo) + index * BMDColorChanInfo.Size);


            return false;
        }

        public override void OnPopulate()
        {
            //MDL0TextureRef* first = Header->First;
            //for (int i = 0; i < Header->_numTextures; i++)
            //    new MDL0MaterialRefNode().Initialize(this, first++, MDL0TextureRef.Size);
        }
    }
}
