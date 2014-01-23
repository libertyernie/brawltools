using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using BrawlLib.Wii.Graphics;
using System.Runtime.InteropServices;
using BrawlLib.Modeling;
using System.Windows.Forms;
using BrawlLib.Imaging;

namespace BrawlLib.Wii.Models
{
    public unsafe delegate void ElementDecoder(ref byte* pIn, ref byte* pOut, float scale);
    public unsafe class ElementCodec
    {
        [Flags]
        public enum CodecType : int
        {
            S = 0,
            ST = 5,
            XY = 10,
            XYZ = 15
        }

        #region Decoders

        public static ElementDecoder[] Decoders = new ElementDecoder[] 
        {
            //Element_Input_Output
            Element_Byte_Float2, //S
            Element_SByte_Float2,
            Element_wUShort_Float2,
            Element_wShort_Float2,
            Element_wFloat_Float2,
            Element_Byte2_Float2, //ST
            Element_SByte2_Float2,
            Element_wUShort2_Float2,
            Element_wShort2_Float2,
            Element_wFloat2_Float2,
            Element_Byte2_Float3, //XY
            Element_SByte2_Float3,
            Element_wUShort2_Float3,
            Element_wShort2_Float3,
            Element_wFloat2_Float3,
            Element_Byte3_Float3, //XYZ
            Element_SByte3_Float3,
            Element_wUShort3_Float3,
            Element_wShort3_Float3,
            Element_wFloat3_Float3
        };

        public static void Element_Byte_Float2(ref byte* pIn, ref byte* pOut, float scale)
        {
            ((float*)pOut)[0] = (float)(*pIn++) * scale;
            ((float*)pOut)[1] = 0.0f;
            pOut += 8;
        }
        public static void Element_SByte_Float2(ref byte* pIn, ref byte* pOut, float scale)
        {
            ((float*)pOut)[0] = (float)(*(sbyte*)pIn++) * scale;
            ((float*)pOut)[1] = 0.0f;
            pOut += 8;
        }
        public static void Element_wUShort_Float2(ref byte* pIn, ref byte* pOut, float scale)
        {
            ((float*)pOut)[0] = (float)(ushort)((*pIn++ << 8) | *pIn++) * scale;
            ((float*)pOut)[1] = 0.0f;
            pOut += 8;
        }
        public static void Element_wShort_Float2(ref byte* pIn, ref byte* pOut, float scale)
        {
            ((float*)pOut)[0] = (float)(short)((*pIn++ << 8) | *pIn++) * scale;
            ((float*)pOut)[1] = 0.0f;
            pOut += 8;
        }
        public static void Element_wFloat_Float2(ref byte* pIn, ref byte* pOut, float scale)
        {
            float val;
            byte* p = (byte*)&val;
            p[3] = *pIn++;
            p[2] = *pIn++;
            p[1] = *pIn++;
            p[0] = *pIn++;

            ((float*)pOut)[0] = val * scale;
            ((float*)pOut)[1] = 0.0f;
            pOut += 8;
        }

        public static void Element_Byte2_Float2(ref byte* pIn, ref byte* pOut, float scale)
        {
            ((float*)pOut)[0] = (float)(*pIn++) * scale;
            ((float*)pOut)[1] = (float)(*pIn++) * scale;
            pOut += 8;
        }
        public static void Element_SByte2_Float2(ref byte* pIn, ref byte* pOut, float scale)
        {
            ((float*)pOut)[0] = (float)(*(sbyte*)pIn++) * scale;
            ((float*)pOut)[1] = (float)(*(sbyte*)pIn++) * scale;
            pOut += 8;
        }
        public static void Element_wUShort2_Float2(ref byte* pIn, ref byte* pOut, float scale)
        {
            ((float*)pOut)[0] = (float)(ushort)((*pIn++ << 8) | *pIn++) * scale;
            ((float*)pOut)[1] = (float)(ushort)((*pIn++ << 8) | *pIn++) * scale;
            pOut += 8;
        }
        public static void Element_wShort2_Float2(ref byte* pIn, ref byte* pOut, float scale)
        {
            ((float*)pOut)[0] = (float)(short)((*pIn++ << 8) | *pIn++) * scale;
            ((float*)pOut)[1] = (float)(short)((*pIn++ << 8) | *pIn++) * scale;
            pOut += 8;
        }
        public static void Element_wFloat2_Float2(ref byte* pIn, ref byte* pOut, float scale)
        {
            float val;
            byte* p = (byte*)&val;

            for (int i = 0; i < 2; i++)
            {
                p[3] = *pIn++;
                p[2] = *pIn++;
                p[1] = *pIn++;
                p[0] = *pIn++;
                ((float*)pOut)[i] = val * scale;
            }
            pOut += 8;
        }

        public static void Element_wShort2_Float3(ref byte* pIn, ref byte* pOut, float scale)
        {
            float* f = (float*)pOut;

            *f++ = (float)(short)((*pIn++ << 8) | *pIn++) * scale;
            *f++ = (float)(short)((*pIn++ << 8) | *pIn++) * scale;
            *f = 0.0f;

            pOut += 12;
        }

        public static void Element_wShort3_Float3(ref byte* pIn, ref byte* pOut, float scale)
        {
            short temp;
            byte* p = (byte*)&temp;
            for (int i = 0; i < 3; i++)
            {
                p[1] = *pIn++;
                p[0] = *pIn++;
                *(float*)pOut = (float)temp * scale;
                pOut += 4;
            }
        }
        public static void Element_wUShort2_Float3(ref byte* pIn, ref byte* pOut, float scale)
        {
            ushort temp;
            byte* p = (byte*)&temp;
            for (int i = 0; i < 3; i++)
            {
                if (i == 2)
                    *(float*)pOut = 0.0f;
                else
                {
                    p[1] = *pIn++;
                    p[0] = *pIn++;
                    *(float*)pOut = (float)temp * scale;
                }
                pOut += 4;
            }
        }
        public static void Element_wUShort3_Float3(ref byte* pIn, ref byte* pOut, float scale)
        {
            ushort temp;
            byte* p = (byte*)&temp;
            for (int i = 0; i < 3; i++)
            {
                p[1] = *pIn++;
                p[0] = *pIn++;
                *(float*)pOut = (float)temp * scale;
                pOut += 4;
            }
        }
        public static void Element_Byte2_Float3(ref byte* pIn, ref byte* pOut, float scale)
        {
            for (int i = 0; i < 3; i++)
            {
                *(float*)pOut = (i == 2) ? 0.0f : (float)(*pIn++) * scale;
                pOut += 4;
            }
        }
        public static void Element_Byte3_Float3(ref byte* pIn, ref byte* pOut, float scale)
        {
            for (int i = 0; i < 3; i++)
            {
                *(float*)pOut = (float)(*pIn++) * scale;
                pOut += 4;
            }
        }
        public static void Element_SByte2_Float3(ref byte* pIn, ref byte* pOut, float scale)
        {
            for (int i = 0; i < 3; i++)
            {
                *(float*)pOut = (i == 2) ? 0.0f : (float)(*(sbyte*)pIn++) * scale;
                pOut += 4;
            }
        }
        public static void Element_SByte3_Float3(ref byte* pIn, ref byte* pOut, float scale)
        {
            for (int i = 0; i < 3; i++)
            {
                *(float*)pOut = (float)(*(sbyte*)pIn++) * scale;
                pOut += 4;
            }
        }
        public static void Element_wFloat2_Float3(ref byte* pIn, ref byte* pOut, float scale)
        {
            float temp;
            byte* p = (byte*)&temp;
            for (int i = 0; i < 3; i++)
            {
                if (i == 2)
                    *(float*)pOut = 0.0f;
                else
                {
                    p[3] = *pIn++;
                    p[2] = *pIn++;
                    p[1] = *pIn++;
                    p[0] = *pIn++;
                    *(float*)pOut = temp;
                }
                pOut += 4;
            }
        }
        public static void Element_wFloat3_Float3(ref byte* pIn, ref byte* pOut, float scale)
        {
            float val;
            byte* p = (byte*)&val;
            for (int i = 0; i < 3; i++)
            {
                p[3] = *pIn++;
                p[2] = *pIn++;
                p[1] = *pIn++;
                p[0] = *pIn++;
                *(float*)pOut = val;
                pOut += 4;
            }
        }

        #endregion

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ElementDescriptor
    {
        public int Stride;
        public bool Weighted;
        public bool[] HasData;
        public byte[] Commands;
        public int[] Defs;
        private ushort[] Nodes;
        public int[] RemapTable;
        public int RemapSize;
        public List<List<Facepoint>> _points;

        public ElementDescriptor(MDL0Object* polygon)
        {
            byte* pData = (byte*)polygon->DefList;
            byte* pCom;
            ElementDef* pDef;

            CPElementSpec UVATGroups;
            int format; //0 for direct, 1 for byte, 2 for short

            //Create remap table for vertex weights
            RemapTable = new int[polygon->_numVertices];
            RemapSize = 0;
            Stride = 0;
            HasData = new bool[12];
            Nodes = new ushort[16];
            Commands = new byte[31];
            Defs = new int[12];

            _points = new List<List<Facepoint>>();

            //Read element descriptor from polygon display list
            MDL0PolygonDefs* Definitons = (MDL0PolygonDefs*)polygon->DefList;

            int fmtLo = (int)Definitons->VtxFmtLo;
            int fmtHi = (int)Definitons->VtxFmtHi;

            UVATGroups = new CPElementSpec(
                (uint)Definitons->UVATA,
                (uint)Definitons->UVATB,
                (uint)Definitons->UVATC);

            //Build extract script.
            //What we're doing is assigning extract commands for elements in the polygon, in true order.
            //This allows us to process the polygon blindly, assuming that the definition is accurate.
            //Theoretically, this should offer a significant speed bonus.
            fixed (int* pDefData = Defs)
            fixed (byte* pComData = Commands)
            {
                pCom = pComData;
                pDef = (ElementDef*)pDefData;

                //Pos/Norm weight
                if (Weighted = (fmtLo & 1) != 0)
                {
                    //Set the first command as the weight
                    *pCom++ = (byte)DecodeOp.PosWeight;
                    Stride++; //Increment stride by a byte (the length of the facepoints)
                }

                //Tex matrix
                for (int i = 0; i < 8; i++)
                    if (((fmtLo >> (i + 1)) & 1) != 0)
                    {
                        //Set the command for each texture matrix
                        *pCom++ = (byte)(DecodeOp.TexMtx0 + i);
                        Stride++; //Increment stride by a byte (the length of the facepoints)
                    }

                //Positions
                format = ((fmtLo >> 9) & 3) - 1;
                if (format >= 0)
                {
                    HasData[0] = true;

                    //Set the definitions input
                    pDef->Format = (byte)format;
                    //Set the type to Positions
                    pDef->Type = 0;
                    if (format == 0)
                    {
                        int f = (int)UVATGroups.PositionDef.DataFormat;

                        //Clamp format to even value and add length to stride
                        Stride += f.RoundDownToEven().Clamp(1, 4) * (!UVATGroups.PositionDef.IsSpecial ? 2 : 3);

                        pDef->Scale = (byte)UVATGroups.PositionDef.Scale;
                        pDef->Output = (byte)((!UVATGroups.PositionDef.IsSpecial ? (int)ElementCodec.CodecType.XY : (int)ElementCodec.CodecType.XYZ) + (byte)UVATGroups.PositionDef.DataFormat);
                        *pCom++ = (byte)DecodeOp.ElementDirect;
                    }
                    else
                    {
                        Stride += format; //Add to stride (the length of the facepoints)
                        pDef->Output = 12; //Set the output
                        *pCom++ = (byte)DecodeOp.ElementIndexed;
                    }
                    pDef++;
                }

                //Normals
                format = ((fmtLo >> 11) & 3) - 1;
                if (format >= 0)
                {
                    HasData[1] = true;

                    //Set the definitions input
                    pDef->Format = (byte)format;
                    //Set the type to Normals
                    pDef->Type = 1;
                    if (format == 0)
                    {
                        int f = (int)UVATGroups.NormalDef.DataFormat;
                        Stride += f.RoundDownToEven().Clamp(1, 4) * 3;

                        pDef->Scale = (byte)UVATGroups.NormalDef.Scale;
                        pDef->Output = (byte)(((int)ElementCodec.CodecType.XYZ) + (byte)UVATGroups.NormalDef.DataFormat);
                        *pCom++ = (byte)DecodeOp.ElementDirect;
                    }
                    else
                    {
                        Stride += format; //Add to stride (the length of the facepoints)
                        pDef->Output = 12; //Set the output
                        *pCom++ = (byte)DecodeOp.ElementIndexed;
                    }
                    pDef++;
                }

                //Colors
                for (int i = 0; i < 2; i++)
                {
                    format = ((fmtLo >> (i * 2 + 13)) & 3) - 1;
                    if (format >= 0) 
                    {
                        HasData[i + 2] = true;

                        //Set the definitions input
                        pDef->Format = (byte)format;
                        //Set the type to Colors
                        pDef->Type = (byte)(i + 2);
                        if (format == 0)
                        {
                            //pDef->Output = 
                            pDef->Scale = 0;
                            *pCom++ = (byte)DecodeOp.ElementDirect;
                        }
                        else
                        {
                            Stride += format; //Add to stride (the length of the facepoints)
                            pDef->Output = 4; //Set the output
                            *pCom++ = (byte)DecodeOp.ElementIndexed;
                        }
                        pDef++;
                    }
                }

                //UVs
                for (int i = 0; i < 8; i++)
                {
                    format = ((fmtHi >> (i * 2)) & 3) - 1;
                    if (format >= 0)
                    {
                        HasData[i + 4] = true;

                        //Set the definitions input
                        pDef->Format = (byte)format;
                        //Set the type to UVs
                        pDef->Type = (byte)(i + 4);
                        if (format == 0)
                        {
                            int f = (int)UVATGroups.GetUVDef(i).DataFormat;
                            Stride += f.RoundDownToEven().Clamp(1, 4);

                            pDef->Output = (byte)((!UVATGroups.GetUVDef(i).IsSpecial ? (int)ElementCodec.CodecType.S : (int)ElementCodec.CodecType.ST) + (byte)UVATGroups.GetUVDef(i).DataFormat);
                            pDef->Scale = (byte)UVATGroups.GetUVDef(i).Scale;
                            *pCom++ = (byte)DecodeOp.ElementDirect;
                        }
                        else
                        {
                            Stride += format; //Add to stride (the length of the facepoints)
                            pDef->Output = 8; //Set the output
                            *pCom++ = (byte)DecodeOp.ElementIndexed;
                        }
                        pDef++;
                    }
                }
                *pCom = 0;
            }
        }
        
        //Set node ID/Index using specified command block
        public void SetNode(ref byte* pIn, byte* start)
        {
            //Get node ID
            ushort node = *(bushort*)pIn;

            //Get cache index.
            //Wii memory assigns data using offsets of 4-byte values.
            //In this case, each matrix takes up 12 floats (4 bytes each)

            //Divide by 12, the number of float values per 4x3 matrix, to get the actual index
            int index = (*(bushort*)(pIn + 2) & 0xFFF) / 12;
            //Assign node ID to cache, using index
            fixed (ushort* n = Nodes)
                n[index] = node;

            //Increment pointer
            pIn += 4;
        }

        //Decode a single primitive using command list
        public void Run(ref byte* pIn, byte** pAssets, byte** pOut, int count, PrimitiveGroup group, ref ushort* indices, IMatrixNode[] nodeTable)
        {
            //pIn is the address in the primitives
            //pOut is address of the face data buffers
            //pAssets is the address of the raw asset buffers

            int weight = 0;

            int index = 0, outSize;
            DecodeOp o;
            ElementDef* pDef;
            byte* p;
            byte[] pTexMtx = new byte[8];

            byte* tIn, tOut;

            group._points.Add(new List<Facepoint>());

            //Iterate commands in list
            fixed (ushort* pNode = Nodes)
            fixed (int* pDefData = Defs)
            fixed (byte* pCmd = Commands)
            {
                for (int i = 0; i < count; i++)
                {
                    pDef = (ElementDef*)pDefData;
                    p = pCmd;

                    Facepoint f = new Facepoint();

                Continue:
                    o = (DecodeOp)(*p++);
                    switch (o)
                    {
                        //Process weight using cache
                        case DecodeOp.PosWeight:
                            weight = pNode[*pIn++ / 3];
                            goto Continue;

                        case DecodeOp.TexMtx0:
                        case DecodeOp.TexMtx1:
                        case DecodeOp.TexMtx2:
                        case DecodeOp.TexMtx3:
                        case DecodeOp.TexMtx4:
                        case DecodeOp.TexMtx5:
                        case DecodeOp.TexMtx6:
                        case DecodeOp.TexMtx7:
                            index = (int)o - (int)DecodeOp.TexMtx0;
                            //if (*pIn++ == 0x60) //Identity matrix...
                            //    Console.WriteLine();
                            pTexMtx[index] = (byte)(*pIn++ / 3);
                            goto Continue;

                        case DecodeOp.ElementDirect:
                            ElementCodec.Decoders[pDef->Output](ref pIn, ref pOut[pDef->Type], VQuant.DeQuantTable[pDef->Scale]);
                            goto Continue;

                        case DecodeOp.ElementIndexed:

                            //Get asset index
                            if (pDef->Format == 2)
                            {
                                index = *(bushort*)pIn; 
                                pIn += 2;
                            }
                            else
                                index = *pIn++;

                            switch (pDef->Type)
                            {
                                case 0:
                                    f._vertexIndex = index;
                                    break;
                                case 1:
                                    f._normalIndex = index;
                                    break;
                                case 2:
                                case 3:
                                    f._colorIndices[pDef->Type - 2] = index;
                                    break;
                                default:
                                    f._UVIndices[pDef->Type - 4] = index;
                                    break;
                            }

                            if (pDef->Type == 0) //Special processing for vertices
                            {
                                //Match weight and index with remap table
                                int mapEntry = (weight << 16) | index;

                                //Find matching index, starting at end of list
                                //Lower index until a match is found at that index or index is less than 0
                                index = RemapSize;
                                while ((--index >= 0) && (RemapTable[index] != mapEntry)) ;

                                //No match, create new entry
                                //Will be processed into vertices at the end!
                                if (index < 0)
                                {
                                    RemapTable[index = RemapSize++] = mapEntry;
                                    _points.Add(new List<Facepoint>());
                                }

                                //Write index
                                *indices++ = (ushort)index;

                                _points[index].Add(f);
                            }
                            else
                            {
                                //Copy data from buffer
                                outSize = pDef->Output;

                                //Input data from asset cache
                                tIn = pAssets[pDef->Type] + (index * outSize);
                                tOut = pOut[pDef->Type];

                                if (tIn != null && tOut != null)
                                {
                                    //Copy data to output
                                    while (outSize-- > 0)
                                        *tOut++ = *tIn++;

                                    //Increment element output pointer
                                    pOut[pDef->Type] = tOut;
                                }
                            }

                            pDef++;
                            goto Continue;

                        default: break; //End
                    }
                    
                    group._points[group._points.Count - 1].Add(f);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct ElementDef
        {
            public byte Format; //Input format
            public byte Output; //Output size/decoder
            public byte Type;
            public byte Scale;
        }

        public enum DecodeOp : int
        {
            End = 0,
            PosWeight,
            TexMtx0,
            TexMtx1,
            TexMtx2,
            TexMtx3,
            TexMtx4,
            TexMtx5,
            TexMtx6,
            TexMtx7,
            ElementDirect, 
            ElementIndexed
        }

        internal unsafe List<Vertex3> Finish(Vector3* pVert, IMatrixNode[] nodeTable)
        {
            //Create vertex list from remap table
            List<Vertex3> list = new List<Vertex3>(RemapSize);

            if (!Weighted)
            {
                //Add vertex to list using raw value.
                for (int i = 0; i < RemapSize; i++)
                {
                    Vertex3 v = new Vertex3(pVert[RemapTable[i]]) { _facepoints = _points[i] };
                    foreach (Facepoint f in v._facepoints) f._vertex = v;
                    list.Add(v);
                }
            }
            else if (nodeTable != null)
            {
                for (int i = 0; i < RemapSize; i++)
                {
                    int x = RemapTable[i];
                    //Create new vertex, assigning the value + influence from the remap table
                    Vertex3 v = new Vertex3(pVert[x & 0xFFFF], nodeTable[x >> 16]) { _facepoints = _points[i] };
                    foreach (Facepoint f in v._facepoints) f._vertex = v;
                    //Add vertex to list
                    list.Add(v);
                }
            }

            return list;
        }
    }
}
