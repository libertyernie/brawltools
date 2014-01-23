using System;
using System.Collections.Generic;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBBTypes;
using System.Linq;

namespace BrawlLib.Wii.Audio
{
    public unsafe class RSARConverter
    {
        public int _headerLen, _fileLen, _infoLen, _symbLen;
        internal int CalculateSize(RSAREntryList entries, RSARNode node)
        {
            //Header
            _headerLen = 0x40;

            //SYMB, INFO, FILE Headers
            _symbLen = 0x20;
            _infoLen = 0x8;
            _fileLen = 0x20;

            #region SYMB

            //String offsets
            _symbLen += entries._strings.Count * 4;

            //Strings are packed tightly with no trailing pad
            _symbLen += entries._stringLength;

            //Mask entries
            _symbLen += 32; //Headers
            _symbLen += (entries._strings.Count * 2 - 4) * 20; //Entries

            //Align
            _symbLen = _symbLen.Align(0x20);

            #endregion

            #region Info

            //Info ruint collection and ruint list counts
            _infoLen += 0x30;

            int sounds = 4, playerInfo = 4, banks = 4, groups = 4, files = 4;

            //ruint sizes
            sounds += entries._sounds.Count * 8;
            playerInfo += entries._playerInfo.Count * 8;
            groups += entries._groups.Count * 8;
            banks += entries._banks.Count * 8 + 8;
            files += entries._files.Count * 8;

            //Evaluate entries with child offsets
            foreach (RSAREntryState s in entries._sounds)
                sounds += s._node.CalculateSize(true);
            foreach (RSAREntryState s in entries._playerInfo)
                playerInfo += s._node.CalculateSize(true);
            foreach (RSAREntryState s in entries._banks)
                banks += s._node.CalculateSize(true);
            foreach (RSAREntryState s in entries._groups)
                groups += s._node.CalculateSize(true);
            groups += INFOGroupHeader.Size + 4; //Null group at the end
            groups += node._nullGroup.Files.Count * 32;
            foreach (RSARFileNode s in entries._files)
            {
                files += INFOFileHeader.Size + 4;
                if (!(s is RSARExtFileNode))
                    files += s._groups.Count * (8 + INFOFileEntry.Size);
                else
                    files += (s._extPath.Length + 1).Align(4);
            }

            //Footer and Align
            _infoLen = ((_infoLen += (sounds + banks + playerInfo + files + groups)) + 0x10).Align(0x20);

            #endregion

            #region File

            foreach (RSAREntryState r in entries._groups)
            {
                RSARGroupNode g = r._node as RSARGroupNode;
                foreach (RSARFileNode f in g.Files)
                    _fileLen += f.CalculateSize(true);
            }
            foreach (RSARFileNode f in node._nullGroup.Files)
                _fileLen += f.CalculateSize(true);

            //Align
            _fileLen = _fileLen.Align(0x20);

            #endregion

            return _headerLen + _symbLen + _infoLen + _fileLen;
        }

        internal int EncodeSYMBBlock(SYMBHeader* header, RSAREntryList entries, RSARNode node)
        {
            int len = 0;
            int count = entries._strings.Count;
            VoidPtr baseAddr = (VoidPtr)header + 8, dataAddr;
            bint* strEntry = (bint*)(baseAddr + 0x18);
            PString pStr = (byte*)strEntry + (count << 2);

            //Strings
            header->_stringOffset = 0x14;
            strEntry[-1] = entries._strings.Count;
            foreach (string s in entries._strings)
            {
                *strEntry++ = (int)(pStr - baseAddr);
                pStr.Write(s, 0, s.Length + 1);
                pStr += s.Length + 1;
            }

            dataAddr = pStr;

            //Sounds
            header->_maskOffset1 = (int)(dataAddr - baseAddr);
            dataAddr += EncodeMaskGroup(header, (SYMBMaskHeader*)dataAddr, entries._sounds, node, 0);

            //Player Info
            header->_maskOffset2 = (int)(dataAddr - baseAddr);
            dataAddr += EncodeMaskGroup(header, (SYMBMaskHeader*)dataAddr, entries._playerInfo, node, 1);

            //Groups
            header->_maskOffset3 = (int)(dataAddr - baseAddr);
            dataAddr += EncodeMaskGroup(header, (SYMBMaskHeader*)dataAddr, entries._groups, node, 2);

            //Banks
            header->_maskOffset4 = (int)(dataAddr - baseAddr);
            dataAddr += EncodeMaskGroup(header, (SYMBMaskHeader*)dataAddr, entries._banks, node, 3);

            int temp = (int)dataAddr - (int)header;
            len = temp.Align(0x20);

            //Fill padding
            byte* p = (byte*)dataAddr;
            for (int i = temp; i < len; i++)
                *p++ = 0;

            //Set header
            header->_header._tag = SYMBHeader.Tag;
            header->_header._length = len;

            return len;
        }
        internal int EncodeINFOBlock(INFOHeader* header, RSAREntryList entries, RSARNode node)
        {
            int len = 0;

            VoidPtr baseAddr = header->_collection.Address;
            ruint* values = (ruint*)baseAddr;
            VoidPtr dataAddr = baseAddr + 0x30;
            RuintList* entryList;
            int index = 0;

            //Set up sound ruint list
            values[0] = (uint)dataAddr - (uint)baseAddr;
            entryList = (RuintList*)dataAddr;
            entryList->_numEntries = entries._sounds.Count;
            dataAddr += entries._sounds.Count * 8 + 4;

            //Write sound entries
            foreach (RSAREntryState r in entries._sounds)
            {
                r._node._rebuildBase = baseAddr;
                entryList->Entries[index++] = (uint)dataAddr - (uint)baseAddr;
                r._node.Rebuild(dataAddr, r._node._calcSize, true);
                dataAddr += r._node._calcSize;
            }
            index = 0;
            //Set up bank ruint list
            values[1] = (uint)dataAddr - (uint)baseAddr;
            entryList = (RuintList*)dataAddr;
            entryList->_numEntries = entries._banks.Count;
            dataAddr += entries._banks.Count * 8 + 4;

            //Write bank entries
            foreach (RSAREntryState r in entries._banks)
            {
                r._node._rebuildBase = baseAddr;
                entryList->Entries[index++] = (uint)dataAddr - (uint)baseAddr;
                r._node.Rebuild(dataAddr, r._node._calcSize, true);
                dataAddr += r._node._calcSize;
            }
            index = 0;
            //Set up playerInfo ruint list
            values[2] = (uint)dataAddr - (uint)baseAddr;
            entryList = (RuintList*)dataAddr;
            entryList->_numEntries = entries._playerInfo.Count;
            dataAddr += entries._playerInfo.Count * 8 + 4;

            //Write playerInfo entries
            foreach (RSAREntryState r in entries._playerInfo)
            {
                r._node._rebuildBase = baseAddr;
                entryList->Entries[index++] = (uint)dataAddr - (uint)baseAddr;
                r._node.Rebuild(dataAddr, r._node._calcSize, true);
                dataAddr += r._node._calcSize;
            }
            index = 0;
            //Set up file ruint list
            values[3] = (uint)dataAddr - (uint)baseAddr;
            entryList = (RuintList*)dataAddr;
            entryList->_numEntries = entries._files.Count;
            dataAddr += entries._files.Count * 8 + 4;

            //Write file entries
            foreach (RSARFileNode file in entries._files)
            {
                entryList->Entries[index++] = (uint)dataAddr - (uint)baseAddr;
                INFOFileHeader* fileHdr = (INFOFileHeader*)dataAddr;
                dataAddr += INFOFileHeader.Size;
                RuintList* list = (RuintList*)dataAddr;
                fileHdr->_entryNumber = -1;
                if (file is RSARExtFileNode)
                {
                    //Make an attempt to get current file size
                    uint s = 0;
                    if (file.ExternalFileInfo.Exists)
                        s = (uint)file.ExternalFileInfo.Length;
                    if (file._extFileSize != s && s != 0) file._extFileSize = s;
                    //Shouldn't matter if 0
                    fileHdr->_headerLen = file._extFileSize;

                    fileHdr->_dataLen = 0;
                    fileHdr->_stringOffset = (uint)((VoidPtr)list - (VoidPtr)baseAddr);

                    sbyte* dPtr = (sbyte*)list;
                    file._extPath.Write(ref dPtr);
                    dataAddr += ((int)((VoidPtr)dPtr - (VoidPtr)dataAddr)).Align(4);

                    fileHdr->_listOffset = (uint)((VoidPtr)dataAddr - (VoidPtr)baseAddr);
                    dataAddr += 4; //Empty list
                }
                else
                {
                    fileHdr->_headerLen = (uint)file._headerLen;
                    fileHdr->_dataLen = (uint)file._audioLen;
                    //fileHdr->_stringOffset = 0;
                    fileHdr->_listOffset = (uint)((VoidPtr)list - (VoidPtr)baseAddr);
                    list->_numEntries = file._groups.Count;
                    INFOFileEntry* fileEntries = (INFOFileEntry*)((VoidPtr)list + 4 + file._groups.Count * 8);
                    int z = 0;
                    List<int> used = new List<int>();
                    foreach (RSARGroupNode g in file._groups)
                    {
                        list->Entries[z] = (uint)((VoidPtr)(&fileEntries[z]) - baseAddr);
                        fileEntries[z]._groupId = g._rebuildIndex;
                        int[] all = g._files.FindAllOccurences(file);
                        bool done = false;
                        foreach (int i in all)
                            if (!used.Contains(i))
                            {
                                fileEntries[z]._index = i;
                                used.Add(i);
                                done = true;
                                break;
                            }
                        if (!done)
                            fileEntries[z]._index = g._files.IndexOf(file);
                        z++;
                    }
                    dataAddr = (VoidPtr)fileEntries + file._groups.Count * INFOFileEntry.Size;
                }
            }
            index = 0;
            //Set up group ruint list
            values[4] = (uint)dataAddr - (uint)baseAddr;
            entryList = (RuintList*)dataAddr;
            entryList->_numEntries = entries._groups.Count + 1;
            dataAddr += (entries._groups.Count + 1) * 8 + 4;

            //Write group entries
            foreach (RSAREntryState r in entries._groups)
            {
                r._node._rebuildBase = baseAddr;
                entryList->Entries[index++] = (uint)dataAddr - (uint)baseAddr;
                r._node.Rebuild(dataAddr, r._node._calcSize, true);
                dataAddr += r._node._calcSize;
            }
            //Null group at the end
            entryList->Entries[entries._groups.Count] = (uint)dataAddr - (uint)baseAddr;
            INFOGroupHeader* grp = (INFOGroupHeader*)dataAddr;
            node._nullGroup._rebuildAddr = grp;
            node._nullGroup._rebuildBase = baseAddr;
            *(bint*)(dataAddr + INFOGroupHeader.Size) = 0;
            grp->_entryNum = -1;
            grp->_stringId = -1;
            //grp->_extFilePathRef = 0;
            //grp->_extFilePathRef._dataType = 0;
            grp->_headerLength = 0;
            grp->_waveDataLength = 0;
            grp->_headerOffset = grp->_waveDataOffset = _headerLen + _symbLen + _infoLen + _fileLen;
            grp->_listOffset = (uint)((VoidPtr)(dataAddr + INFOGroupHeader.Size) - baseAddr);
            dataAddr += INFOGroupHeader.Size;
            RuintList* l = (RuintList*)dataAddr;
            INFOGroupEntry* e = (INFOGroupEntry*)((VoidPtr)l + 4 + node._nullGroup.Files.Count * 8);
            l->_numEntries = node._nullGroup.Files.Count;
            int y = 0;
            foreach (RSARFileNode file in node._nullGroup.Files)
            {
                l->Entries[y] = (uint)((VoidPtr)(&e[y]) - baseAddr);
                e[y++]._fileId = file._fileIndex;
                //entries[i]._dataLength = 0;
                //entries[i]._dataOffset = 0;
                //entries[i]._headerLength = 0;
                //entries[i]._headerOffset = 0;
            }
            dataAddr = (VoidPtr)e + node._nullGroup.Files.Count * 0x18;

            //Write footer
            values[5] = (uint)dataAddr - (uint)baseAddr;
            *(INFOFooter*)dataAddr = node.ftr;

            //Set header
            header->_header._tag = INFOHeader.Tag;
            header->_header._length = len = ((int)((dataAddr + INFOFooter.Size) - (baseAddr - 8))).Align(0x20);

            return len;
        }
        internal int EncodeFILEBlock(FILEHeader* header, RSAREntryList entries, RSARNode node)
        {
            int len = 0;
            VoidPtr baseAddr = (VoidPtr)header + 0x20;
            VoidPtr addr = baseAddr;

            //Build files - order by groups
            for (int x = 0; x <= entries._groups.Count; x++)
            {
                RSARGroupNode g = x == entries._groups.Count ? node._nullGroup : ((RSAREntryState)entries._groups[x])._node as RSARGroupNode;
                int headerLen = 0, audioLen = 0;
                int i = 0;
                INFOGroupEntry* e = (INFOGroupEntry*)((VoidPtr)g._rebuildAddr + INFOGroupHeader.Size + 4 + g._files.Count * 8);
                g._rebuildAddr->_headerOffset = (int)(addr - node._rebuildBase);
                foreach (RSARFileNode f in g.Files)
                {
                    e[i]._headerLength = f._headerLen;
                    e[i++]._headerOffset = headerLen;

                    headerLen += f._headerLen;
                }
                i = 0;
                VoidPtr wave = addr + headerLen;
                g._rebuildAddr->_waveDataOffset = (int)(wave - node._rebuildBase);
                foreach (RSARFileNode f in g.Files)
                {
                    f._rebuildAudioAddr = wave + audioLen;

                    f.Rebuild(addr, f._headerLen, true);
                    addr += f._headerLen;

                    e[i]._dataLength = f._audioLen;
                    e[i++]._dataOffset = f._audioLen == 0 ? 0 : audioLen;

                    audioLen += f._audioLen;
                }

                addr += audioLen;
                g._rebuildAddr->_headerLength = headerLen;
                g._rebuildAddr->_waveDataLength = audioLen;
            }

            len = ((int)addr - (int)(VoidPtr)header).Align(0x20);

            //Set header
            header->_header._tag = FILEHeader.Tag;
            header->_header._length = len;

            return len;
        }

        //Failed attempts at generating symb ids commented out below, enjoy

        //static void GenIds(SYMBHeader* symb, SYMBMaskHeader* header, int index, ushort allowedBit)
        //{
        //    SYMBMaskEntry* first = &header->Entries[index];
        //    string mainName = symb->GetStringEntry(first->_stringId);

        //    for (int i = 1; i < header->_numEntries; i += 2)
        //    {
        //        SYMBMaskEntry* secName = &header->Entries[i];
        //        SYMBMaskEntry* secBit = &header->Entries[i + 1];

        //        if (i == index || secBit->_bit != allowedBit)
        //            continue;

        //        string compName = symb->GetStringEntry(secName->_stringId);

        //        int bitIndex = mainName.CompareBits(compName);
        //        if (bitIndex >= 0)
        //        {
        //            //Set the bit index
        //            secBit->_bit = (ushort)bitIndex;

        //            int bit = bitIndex % 8;
        //            int byteIndex = (bitIndex - bit) / 8;

        //            bool leftFound = false, rightFound = false;

        //            mainName = compName;

        //            //Keeping looking down the list for the left and right entries
        //            for (int x = i + 2; x < header->_numEntries; x += 2)
        //            {
        //                SYMBMaskEntry* lrName = &header->Entries[x];
        //                SYMBMaskEntry* lrBit = &header->Entries[x + 1];
        //                compName = symb->GetStringEntry(lrName->_stringId);

        //                if (x == index || lrBit->_bit != allowedBit)
        //                    continue;

        //                bool forceLeft = false;

        //                if (byteIndex >= Math.Min(mainName.Length, compName.Length))
        //                    forceLeft = true;

        //                if (forceLeft || mainName.AtBit(bitIndex) != compName.AtBit(bitIndex))
        //                {
        //                    if (leftFound)
        //                        continue;

        //                    leftFound = true;

        //                    secBit->_leftId = x + 1;
        //                    GenIds(symb, header, x, lrBit->_bit);
        //                }
        //                else
        //                {
        //                    if (rightFound)
        //                        continue;

        //                    rightFound = true;

        //                    secBit->_rightId = x + 1;
        //                    GenIds(symb, header, x, lrBit->_bit);
        //                }
        //            }

        //            if (!leftFound) //No strings matched
        //                secBit->_leftId = i;
        //            else if (!rightFound) //All strings matched
        //                secBit->_rightId = i;

        //            break;
        //        }
        //    }
        //}
        
        //String Table to convert to a tree
        //string[] stringTable = new string[] { "SDPLAYER_BGM", "SDPLAYER_SE", "SDPLAYER_VOICE", "SDPLAYER_SYSTEM", "SDPLAYER_LOOP", "SDPLAYER_VOICE2", "SDPLAYER_SE_NOEFFECT" };

        //public class TEntry
        //{
        //    public string _string;
        //    public string _binary;
        //    public int _bit = -1;
        //    public int _leftID = -1;
        //    public int _rightID = -1;
        //    public int _stringID;
        //    public int _id;

        //    public TEntry(RSAREntryState s)
        //    {
        //        _binary = (_string = s._node._fullPath).ToBinaryArray();
        //        _stringID = s._stringId;
        //        _id = s._index;
        //    }
        //}

        //Making the output table to save me buttloads of time >.>
        //public void convertToBinary(string):
        //    ''' Converts a text string to a string representation of itself in binary. '''
        //    return ''.join(['%08d'%int(bin(ord(i))[2:]) for i in string])

        //Populating an empty table for my own convenience.
        //I assume the stringlist is ordered by ID, and is the first and only segment in the symb.
        //Cause I'm a lazy bastard.

        //Normal String, String (in binary), bit, LeftID, RightID, StringID, ID

        //outputTable = [{'string':stringTable[x], 'binary':convertToBinary(stringTable[x]), 
        //                'bit':-1, 'LeftID':-1, 'RightID':-1, 'StringID':x, 'ID':x} for x in xrange(len(stringTable))]

        //private static void WriteData(List<TEntry> Left, List<TEntry> Right, int bit)
        //{
        //    //Writes all the necessary information to the node

        //    TEntry currentNode = Right[0];

        //    //Write Position
        //    currentNode._bit = bit;

        //    //Write the left branch (matching) This is a bit fugly, because of how I handle the output table.
        //    //I basically determine the 'line' based off the ID.
        //    if (Left.Count > 1)
        //        currentNode._leftID = Left[0]._id * 2;
        //    else
        //    {
        //        currentNode._leftID = Left[0]._id * 2 - 1;
        //        //Edge case for the first node, who has no -1 line.
        //        if (currentNode._leftID < 0)
        //            currentNode._leftID = 0;
        //    }

        //    if (Right.Count > 1)
        //        currentNode._rightID = Right[1]._id * 2;
        //    else
        //    {
        //        currentNode._rightID = Right[0]._id * 2 - 1;
        //        //Edge case for the first node, who has no -1 line.
        //        if (currentNode._rightID < 0)
        //            currentNode._rightID = 0;
        //    }
        //}

        //private static void SplitTable(List<TEntry> outputTable, out List<TEntry> Left, out List<TEntry> Right)
        //{
        //    //Splits the table in two and writes the data

        //    TEntry originalNode = outputTable[0];
        //    List<TEntry> Temp;

        //    //Iterate bit by bit
        //    int bit = 0, entry = 0;
        //    for (bit = 0; bit < originalNode._binary.Length; bit++)
        //    {
        //        char compareBit = originalNode._binary[bit];

        //        //Go over the table
        //        for (entry = 0; entry < outputTable.Count; entry++)
        //        {
        //            //Fucking Ugly edge case - initial string is too long to compare
        //            if (bit >= outputTable[entry]._binary.Length)
        //            {
        //                //Python stuff to change the 'start' of the table to the current position
        //                Temp = outputTable.ShiftFirst(entry);

        //                //Split the ordered table there into Left (matching) and right (non-matching)
        //                Left = new List<TEntry> { outputTable[entry] };

        //                Right = new List<TEntry>();
        //                foreach (TEntry t in Temp)
        //                    if (t._id != outputTable[entry]._id)
        //                        Right.Add(t);

        //                WriteData(Left, Right, bit);

        //                //It splits!
        //                return;
        //            }

        //            //This is the normal case - applies 90% of the time, I find.
        //            if (outputTable[entry]._binary[bit] != compareBit)
        //            {
        //                //Python stuff to change the 'start' of the table to the current position
        //                Temp = outputTable.ShiftFirst(entry);

        //                //Split the ordered table there into Left (matching) and right (non-matching)
        //                Left = new List<TEntry>();
        //                foreach (TEntry t in Temp)
        //                    if (t._binary[bit] == '0')
        //                        Left.Add(t);

        //                Right = new List<TEntry>();
        //                foreach (TEntry t in Temp)
        //                    if (t._binary[bit] == '1')
        //                        Right.Add(t);

        //                WriteData(Left, Right, bit);

        //                //It splits!
        //                return;
        //            }
        //        }
        //    }

        //    //If it got this far, then it ran out of bits.
        //    //So now we gotta

        //    //Split the ordered table there into Left (matching) and right (non-matching)
        //    Left = new List<TEntry> { originalNode };
        //    Right = new List<TEntry>();
        //    foreach (TEntry t in outputTable)
        //        if (t._id != originalNode._id)
        //            Right.Add(t);

        //    WriteData(Left, Right, bit);

        //    //It splits!
        //    return;
        //}

        //public static void CalcMatch(List<TEntry> outputTable)
        //{
        //    //Split the Table by node
        //    List<TEntry> Left, Right;
        //    SplitTable(outputTable, out Left, out Right);

        //    //foreach (TEntry t in Left)
        //    //    Console.WriteLine(String.Format("Left: {0} {1} {2} {3} ", t._string, t._bit, t._leftID, t._rightID));

        //    //Console.WriteLine("-------------------------------------------------");
            
        //    //foreach (TEntry t in Right)
        //    //    Console.WriteLine(String.Format("Right: {0} {1} {2} {3} ", t._string, t._bit, t._leftID, t._rightID));

        //    if (Left.Count > 1)
        //        CalcMatch(Left);
        //    if (Right.Count > 1)
        //        CalcMatch(Right);
        //}

        //public class PatriciaTree
        //{
        //    public string[] strings;
        //    public string[] bin_strings;
        //    public int out_node_idx = 0;

        //    public PatriciaTree(string[] str)
        //    {
        //        strings = str;
        //        bin_strings = strings.Select(x => x.ToBinaryArray()).ToArray();
        //    }

        //    public int[] partition_tree(List<int> idx_list, int position)
        //    {
        //        List<int> left = new List<int>(), right = new List<int>();
        //        for (int i = 0; i < idx_list.Count; i++ )
        //        {
        //            string bstr = bin_strings[i];
        //            if (position >= bstr.Length || position < 0)
        //                left.Add(i);
        //            else
        //            {
        //                char ch = bstr[position];
        //                if (ch == '1')
        //                    right.Add(i);
        //                else
        //                    left.Add(i);
        //            }
        //        }
        //        int total = left.Count + right.Count;
        //        if (total == 1)
        //        {
        //            //This is a leaf, woo

        //            int pos = -1, t;

        //            if (left.Count > 0)
        //                t = left[0];
        //            else
        //                t = right[0];

        //            return new int[] { pos, t };
        //        }
        //        else if (left.Count == 0 || right.Count == 0)
        //            //This is not a branch, let's try the next bit
        //            if (left.Count > 0)
        //                return partition_tree(left, position + 1);
        //            else
        //                return partition_tree(right, position + 1);
        //        else
        //        {
        //            int[] l = partition_tree(left, position + 1);
        //            int[] r = partition_tree(right, position + 1);
        //            return new int[] { position }.Append(l).Append(r);
        //        }
        //    }
        //    public int tree_size(int[] node)
        //    {
        //        //Node is the return from partition_tree()
        //        if (node[0] == -1)
        //            return 1;
        //        else
        //            return 1 + tree_size(new int[] { node[1] }) + tree_size(new int[] { node[2] });
        //    }
        //    public void dump_tree(int[] tree)
        //    {
        //        out_node_idx = 0;
        //        _dump_node(tree);
        //    }
        //    public void _dump_node(int[] node)
        //    {
        //        //Node is the return from partition_tree()
        //        int bit_id = node[0];
        //        if (bit_id == -1)
        //        {
        //            //We're packing a leaf
        //            Console.WriteLine(String.Format("{0} leaf  : {1}", out_node_idx, strings[node[1]]));
        //            out_node_idx += 1;
        //        }
        //        else
        //        {
        //            //We're packing a branch
        //            int left_size = tree_size(new int[] { node[1] });
        //            int left_idx = out_node_idx + 1;
        //            int right_idx = left_idx + left_size;
        //            Console.WriteLine(String.Format("{0} branch: bit={1} left={2} right={3}", out_node_idx, bit_id, left_idx, right_idx));
        //            out_node_idx += 1;
        //            dump_tree(new int[] { node[1] });
        //            dump_tree(new int[] { node[2] });
        //        }
        //    }
        //}

        //tree = PatriciaTree(('BANK_HOMEBUTTON','BANK_SYSTEM_SE','BANK_BGM','BANK_SOFTWARE_KEYBOARD_SE'))
        //print(tree.partition_tree())
        //tree.dump_tree(tree.partition_tree())

        private static int EncodeMaskGroup(SYMBHeader* symb, SYMBMaskHeader* header, List<RSAREntryState> group, RSARNode n, int grp)
        {
            SYMBMaskEntry* entry = header->Entries;
            //List<TEntry> outputTable = new List<TEntry>();

            int i = 0;
            foreach (RSAREntryState s in group)
            {
                entry[i++] = new SYMBMaskEntry(1, -1, -1, -1, s._stringId, s._index);
                if (s._index != 0)
                {
                    //entry[i++] = new SYMBMaskEntry(0, 0, 0, 0, -1, -1);
                    entry[i] = n._symbCache[grp][i++];
                }
                //outputTable.Add(new TEntry(s));
            }

            header->_numEntries = group.Count * 2 - 1;
            header->_rootId = n._rootIds[grp];

            //GenIds(symb, header, 0, 0);
            //CalcMatch(outputTable);

            //foreach (TEntry t in outputTable)
            //{
            //    *entry++ = new SYMBMaskEntry(1, -1, -1, -1, t._stringID, t._id);
            //    if (t._id != 0)
            //        *entry++ = new SYMBMaskEntry(0, (short)t._bit, t._leftID, t._rightID, -1, -1);
            //}

            //PatriciaTree t = new PatriciaTree(group.Select(x => x._node._fullPath).ToArray());
            //int[] p = t.partition_tree(new List<int>(t.strings.Length), 0);
            //t.dump_tree(p);

            int len = 8 + i * SYMBMaskEntry.Size;

            //int rootId = 0;
            //int lowestBit = int.MaxValue;
            //entry = header->Entries;
            //for (int i = 2; i < header->_numEntries; i += 2)
            //{
            //    if (entry[i]._bit < lowestBit)
            //    {
            //        lowestBit = entry[i]._bit;
            //        rootId = i;
            //    }
            //}
            //header->_rootId = rootId;

            return len;
        }
    }

    public class RSAREntryState
    {
        public RSAREntryNode _node;
        public int _index;
        public int _stringId;

        public static int Compare(RSAREntryState n1, RSAREntryState n2)
        {
            return n2._node.InfoIndex < n1._node.InfoIndex ? 1 : n2._node.InfoIndex > n1._node.InfoIndex ? -1 : 0;
            //return n2._stringId < n1._stringId ? 1 : n2._stringId > n1._stringId ? -1 : 0;
        }
    }

    public class RSARStringEntryState
    {
        public int _type;
        public int _index;
        public string _name;
    }

    public class RSAREntryList
    {
        public int _stringLength = 0;
        public List<string> _strings = new List<string>();
        public List<RSARStringEntryState> _tempStrings = new List<RSARStringEntryState>();
        public List<RSAREntryState> _sounds = new List<RSAREntryState>();
        public List<RSAREntryState> _playerInfo = new List<RSAREntryState>();
        public List<RSAREntryState> _groups = new List<RSAREntryState>();
        public List<RSAREntryState> _banks = new List<RSAREntryState>();
        public List<RSARFileNode> _files;

        public void AddEntry(string path, RSAREntryNode node)
        {
            RSAREntryState state = new RSAREntryState();
            RSARStringEntryState str = new RSARStringEntryState();

            state._node = node;
            if (node._name != "<null>")
                str._name = path;
            else 
                str._name = null;

            if (String.IsNullOrEmpty(str._name))
                state._stringId = -1;

            int type = -1;
            List<RSAREntryState> group;
            if (node is RSARSoundNode)
            {
                group = _sounds;
                type = 3;
            }
            else if (node is RSARGroupNode)
            {
                group = _groups;
                type = 1;
            }
            else if (node is RSARPlayerInfoNode)
            {
                group = _playerInfo;
                type = 0;
            }
            else
            {
                group = _banks;
                type = 2;
            }

            str._type = type;
            str._index = node.InfoIndex;

            _tempStrings.Add(str);

            state._index = group.Count;
            group.Add(state);

            state._node._rebuildIndex = state._index;
        }

        public void Clear()
        {
            _sounds.Clear();
            _playerInfo.Clear();
            _groups.Clear();
            _banks.Clear();
            _stringLength = 0;
            _strings = new List<string>();
            _tempStrings = new List<RSARStringEntryState>();
        }

        internal void SortStrings()
        {
            _stringLength = 0;
            _strings = new List<string>();
            int type = 0;
            int index = 0;
        Top:
            foreach (RSARStringEntryState s in _tempStrings)
            {
                if (s._type == type && s._index == index)
                {
                    if (s._name != null && !s._name.Contains("<null>"))
                        _strings.Add(s._name);
                    index++;
                    goto Top;
                }
            }
            if (++type < 4)
            {
                index = 0;
                goto Top;
            }

            foreach (string s in _strings)
                _stringLength += s.Length + 1;

            foreach (RSAREntryState s in _sounds)
                s._node._rebuildStringId = s._stringId = _strings.IndexOf(s._node._fullPath);
            foreach (RSAREntryState s in _playerInfo)
                s._node._rebuildStringId = s._stringId = _strings.IndexOf(s._node._fullPath);
            foreach (RSAREntryState s in _groups)
                s._node._rebuildStringId = s._stringId = _strings.IndexOf(s._node._fullPath);
            foreach (RSAREntryState s in _banks)
                s._node._rebuildStringId = s._stringId = _strings.IndexOf(s._node._fullPath);

            _sounds.Sort(RSAREntryState.Compare);
            _playerInfo.Sort(RSAREntryState.Compare);
            _groups.Sort(RSAREntryState.Compare);
            _banks.Sort(RSAREntryState.Compare);
        }
    }
}
