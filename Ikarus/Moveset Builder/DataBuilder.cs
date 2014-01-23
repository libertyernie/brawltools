using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Animations;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.OpenGL;
using Ikarus;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe partial class NewMovesetBuilder
    {
        //---Notes--- (these may or may not actually matter - follow just to be safe)
        // - Subroutines are written right before the first script are called by
        // - Children are written before their parent

        //---General order of data---
        //Header
        //Attributes
        //SSE Attributes
        //Common Action Flags
        //Unknown 7
        //Subaction Other Data - includes articles (at end)
        //Subaction GFX Data - includes articles (at end)
        //Misc Bone Section
        //Subaction SFX Data - includes articles (at end)
        //Sound Lists Final Children Entries
        //Subaction Main Data - includes articles (at end)
        //Misc Section 1
        //Hurtboxes
        //Misc Unk Section 9 Final Children Entries
        //Ledgegrabs
        //Crawl
        //Tether
        //Multijump
        //Glide
        //Actions Data Part1/Part2 alternating
        //Subroutines
        //Article Actions
        //Action Flags
        //Action Entry Offsets
        //Action Exit Offsets
        //Action Pre
        //Subaction Flags
        //Subaction Offsets Main/GFX/SFX/Other
        //Model Visibility
        //Misc Item Bones
        //Hand Bones
        //Unknown Section 9
        //Unk24
        //Unknown Section 12
        //Unk22
        //Extra Params
        //Static Articles
        //Entry Article
        //Attributed Articles
        //Misc Section 2
        //Action Interrupts
        //Bone Floats 1
        //Bone Floats 2
        //Bone Floats 3
        //Bone References Main
        //Bone References Misc
        //Misc Sound Lists
        //Misc Section Header
        //Sections Data (includes data header)
        //Lookup Offsets
        //Sections Offsets
        //References Offsets
        //Sections/References String Table

        public List<MovesetEntry> _postProcessNodes;
        public VoidPtr _baseAddress;
        public LookupManager _lookupOffsets;
        public int _lookupCount = 0, _lookupLen = 0;
        public int _refCount = 0;
        CompactStringTable _referenceStringTable;

        //Offset - Size
        public Dictionary<int, int> _lookupEntries;

        MovesetFile _movesetNode;
        VoidPtr _dataAddress;
        
        public int CalcSize(MovesetFile node)
        {
            _movesetNode = node;

            int size = 0x20;

            _postProcessNodes = new List<MovesetEntry>();
            _lookupOffsets = new LookupManager();
            _lookupCount = 0;
            _lookupLen = 0;
            _referenceStringTable = new CompactStringTable();

            //Calculate the size of each section and add names to string table
            //foreach (MovesetEntry e in node._sections._sectionList)
            //{
            //    MovesetEntry entry = e;
            //    if (entry is ReferenceEntry)
            //    {
            //        ReferenceEntry ext = entry as ReferenceEntry;
            //        if (ext._references.Count > 0)
            //            entry = ext._references[0]; //Sections always have only one reference
            //    }
                
            //    int s = 0;
            //    if (!(entry is RawData))
            //        s = GetSectionSize(entry);
            //    else
            //        if (entry.Children.Count > 0)
            //            foreach (MovesetEntry n in entry.Children)
            //                s += GetSectionSize(n);
            //        else
            //            s = GetSectionSize(entry);

            //    size += s + 8; //Size of entry + data offset size

            //    _lookupCount += entry._lookupCount;
            //    _referenceStringTable.Add(entry.Name);
            //}

            ////Calculate reference table size and add names to string table
            //if (node._references != null)
            //    foreach (ReferenceEntry e in node._references.Children)
            //    {
            //        if (e._references.Count > 0)
            //        {
            //            _referenceStringTable.Add(e.Name);
            //            size += 8;
            //        }
            //        //references don't use lookup table
            //        //lookupCount += e._refs.Count - 1;
            //    }

            return size + (_lookupLen = _lookupCount * 4) + _referenceStringTable.TotalSize;
        }

        public int GetSectionSize(MovesetEntry node)
        {
            //if (node != null)
            //{
            //    node._lookupOffsets = new List<VoidPtr>();

            //    if ((node.Parent is DataSection
            //        || node.Parent is Miscellaneous
            //        || node.Parent is DataCommonSection
            //        || node.Parent is AnimParamSection
            //        //|| node.Parent is MoveDefSubParamNode
            //        ) && !node.isExtra)
            //        lookupCount++;

            //    int size = node.CalculateSize(true);
            //    lookupCount += node._lookupCount;

            //    return size;
            //}
            //else 
                return 0;
        }

        //Children are written in order but before their parent! 
        public void Write(MovesetFile node, VoidPtr address, int length)
        {
            //_movesetNode = node;

            //DataSection data = node._data;
            //DataCommonSection dataCommon = node._dataCommon;
            //MoveDefSectionNode sections = node._sections;
            //MoveDefReferenceNode references = node._references;

            //_baseAddress = address + 0x20;

            //FDefHeader* header = (FDefHeader*)address;
            //header->_fileSize = length;
            //header->_externalSubRoutineCount = _refCount;
            //header->_dataTableEntryCount = sections._sectionList.Count;
            //header->_lookupEntryCount = _lookupCount;
            //header->_pad1 = header->_pad2 = header->_pad3 = 0;

            //VoidPtr dataAddress = _baseAddress;

            //int lookupOffset = 0, sectionsOffset = 0;
            //foreach (MovesetEntry e in sections._sectionList)
            //{
            //    lookupOffset += (e._calcSize == 0 ? e._childLength + e._entryLength : e._calcSize);
            //    sectionsOffset += e._childLength;
            //}

            //VoidPtr lookupAddress = dataAddress + lookupOffset;
            //VoidPtr sectionsAddr = dataAddress + sectionsOffset;
            //VoidPtr dataHeaderAddr = dataAddress;

            //foreach (MovesetEntry e in sections._sectionList)
            //{
            //    e._lookupOffsets.Clear();
            //    if (e.Name == "data" || e.Name == "dataCommon")
            //    {
            //        dataHeaderAddr = sectionsAddr; //Don't rebuild yet
            //        sectionsAddr += e._entryLength;
            //    }
            //    else //Rebuild other sections first
            //    {
            //        if (e is ReferenceEntry)
            //        {
            //            ReferenceEntry ext = e as ReferenceEntry;
            //            if (ext._references.Count > 0)
            //            {
            //                MovesetEntry entry = ext._references[0];

            //                if (!(entry is RawData))
            //                    entry.Rebuild(sectionsAddr, entry._calcSize, true);
            //                else
            //                    if (entry.Children.Count > 0)
            //                    {
            //                        entry._rebuildAddr = sectionsAddr;
            //                        int off = 0;
            //                        foreach (MovesetEntry n in entry.Children)
            //                        {
            //                            n.Rebuild(sectionsAddr + off, n._calcSize, true);
            //                            off += n._calcSize;
            //                            entry._lookupOffsets.AddRange(n._lookupOffsets);
            //                        }
            //                    }
            //                    else
            //                        entry.Rebuild(sectionsAddr, entry._calcSize, true);

            //                e._rebuildAddr = entry._rebuildAddr;
            //                e._lookupOffsets = entry._lookupOffsets;
            //            }
            //            else
            //                e.Rebuild(sectionsAddr, e._calcSize, true);
            //        }
            //        else
            //            e.Rebuild(sectionsAddr, e._calcSize, true);
            //        if (e._lookupCount != e._lookupOffsets.Count && !((e as ReferenceEntry)._references[0] is ActionScript))
            //            Console.WriteLine();
            //        _lookupOffsets.AddRange(e._lookupOffsets.ToArray());
            //        sectionsAddr += e._calcSize;
            //    }
            //}

            //if (data != null)
            //{
            //    data.dataHeaderAddr = dataHeaderAddr;
            //    data.Rebuild(address + 0x20, data._childLength, true);
            //}
            //else if (dataCommon != null)
            //{
            //    dataCommon.dataHeaderAddr = dataHeaderAddr;
            //    dataCommon.Rebuild(address + 0x20, dataCommon._childLength, true);
            //}

            ////Go through each reference and set offsets
            //foreach (ReferenceEntry e in references.Children)
            //{
            //    for (int i = 0; i < e._references.Count; i++)
            //    {
            //        bint* addr = (bint*)e._references[i]._rebuildAddr;
            //        if (i == e._references.Count - 1)
            //            *addr = -1;
            //        else
            //        {
            //            *addr = (int)e._references[i + 1]._rebuildAddr - (int)_baseAddress;

            //            //references don't use lookup table
            //            //_lookupOffsets.Add(addr);
            //        }
            //    }
            //}

            ////Order lookup offsets
            //_lookupOffsets.Sort();

            //if (_lookupCount != _lookupOffsets.Count)
            //    Console.WriteLine(_lookupCount - _lookupOffsets.Count);

            ////Set header values for lookup entries
            //header->_lookupOffset = (int)lookupAddress - (int)_baseAddress;
            //header->_lookupEntryCount = _lookupOffsets.Count;

            //if (data != null && data.warioSwing4StringOffset > 0 && data.warioParams6 != null)
            //    ((WarioExtraParams6*)data.warioParams6._rebuildAddr)->_offset = data.warioSwing4StringOffset;

            //int val = -1;
            //if (data != null && data.zssFirstOffset > 0)
            //    val = data.zssFirstOffset;

            //bint* values = (bint*)lookupAddress;
            //foreach (VoidPtr addr in _lookupOffsets)
            //{
            //    if (val == addr && data != null && data.zssParams8 != null)
            //    {
            //        *(bint*)data.zssParams8._rebuildAddr = 29;
            //        *((bint*)data.zssParams8._rebuildAddr + 1) = (int)values - (int)_baseAddress;
            //    }

            //    *values++ = (int)(addr - _baseAddress);
            //}

            //dataAddress = (VoidPtr)values;
            //VoidPtr refTableAddr = dataAddress + sections._sectionList.Count * 8 + _refCount * 8;
            //_referenceStringTable.WriteTable(refTableAddr);

            //foreach (MovesetEntry e in sections._sectionList)
            //{
            //    *values++ = (int)e._rebuildAddr - (int)_baseAddress;
            //    *values++ = (int)_referenceStringTable[e.Name] - (int)refTableAddr;
            //}

            //foreach (ReferenceEntry e in references.Children)
            //    if (e._references.Count > 0)
            //    {
            //        *values++ = (int)e._references[0]._rebuildAddr - (int)_baseAddress;
            //        *values++ = (int)_referenceStringTable[e.Name] - (int)refTableAddr;
            //    }

            ////Some nodes handle rebuilding their own children, 
            ////so if one of those children has changed, the node will stay dirty and may rebuild over itself.
            ////Manually set IsDirty to false to avoid that.
            //node.IsDirty = false;

            ////Set the node's base address to the address that we just rebuilt to
            //node.BaseAddress = _baseAddress;
        }

        int _headerLen, _dataLen;
        int
            lookupCount = 0,
            part1Len = 0,
            part2Len = 0,
            part3Len = 0,
            part4Len = 0,
            part5Len = 0,
            part6Len = 0,
            part7Len = 0;

        public int CalcDataSize(DataSection node)
        {
            //MovesetFile RootNode = node.Parent.Parent as MovesetFile;

            //FDefSubActionStringTable subActionTable = new FDefSubActionStringTable();

            //#region Part 1

            //part1Len += GetSize(node._attributes);
            //part1Len += GetSize(node._sseAttributes);
            //part1Len += GetSize(node._commonActionFlags);
            //part1Len += GetSize(node._unk7);

            //foreach (SubActionGroup g in RootNode._subActions.Children)
            //{
            //    if (g.Name != "<null>")
            //    {
            //        foreach (ActionScript a in g.Children)
            //        {
            //            if (a.Children.Count > 0 || a._actionRefs.Count > 0 || a._build)
            //            {
            //                part1Len += GetSize(a);
            //                lookupCount++;
            //            }
            //        }
            //    }
            //}

            //part1Len += CalcSizeArticleActions(node, true, 0);
            //part1Len += CalcSizeArticleActions(node, true, 1);
            //part1Len += CalcSizeArticleActions(node, true, 2);
            //part1Len += GetSize(node._misc.unkBoneSection);

            //if (node._misc.soundData != null)
            //    foreach (MoveDefSoundDataNode r in node._misc.soundData.Children)
            //        part1Len += r.Children.Count * 4;

            //foreach (ActionGroup a in RootNode._actions.Children)
            //{
            //    if (a.Children[0].Children.Count > 0 || (a.Children[0] as ActionScript)._actionRefs.Count > 0 || (a.Children[0] as ActionScript)._build) //Entry
            //    {
            //        part1Len += GetSize(a.Children[0] as ActionScript);
            //        lookupCount++;
            //    }
            //    if (a.Children[1].Children.Count > 0 || (a.Children[1] as ActionScript)._actionRefs.Count > 0 || (a.Children[1] as ActionScript)._build) //Exit
            //    {
            //        part1Len += GetSize(a.Children[1] as ActionScript);
            //        lookupCount++;
            //    }
            //}

            //#endregion
            //#region Part 2

            //part2Len += GetSize(node._misc.unkSection1);
            //part2Len += GetSize(node._misc.hurtBoxes);

            ////if (node.misc.collisionData != null)
            ////    foreach (MoveDefOffsetNode offset in node.misc.collisionData.Children)
            ////        if (offset.Children.Count > 0 && !(offset.Children[0] as MoveDefEntryNode).External)
            ////            part2Len += offset.Children[0].Children.Count * 4;

            //part2Len += GetSize(node._misc.ledgeGrabs);
            //part2Len += GetSize(node._misc.tether);
            //part2Len += GetSize(node._misc.crawl);
            //part2Len += GetSize(node._misc.multiJump);
            //part2Len += GetSize(node._misc.glide);

            //for (int i = 0; i < RootNode._subRoutines.Count; i++)
            //    if ((RootNode._subRoutines[i] as ActionScript)._actionRefs.Count > 0)
            //        part2Len += GetSize(RootNode._subRoutines[i] as ActionScript);

            //if (node._unk22 != null)
            //    if (node._unk22.Children.Count > 0)
            //        part2Len += GetSize(node._unk22.Children[0] as ActionScript);

            //if (node._override1 != null)
            //    foreach (MoveDefActionOverrideEntryNode e in node._override1.Children)
            //        part2Len += GetSize(e.Children[0] as ActionScript);

            //if (node._override2 != null)
            //    foreach (MoveDefActionOverrideEntryNode e in node._override2.Children)
            //        part2Len += GetSize(e.Children[0] as ActionScript);

            //part2Len += CalcSizeArticleActions(node, false, 0);
            //part2Len += GetSize(node._actionFlags);

            //#endregion
            //#region Part 3

            ////Actions part 1 and 2 offsets
            //lookupCount += 2; //offset to the lists
            //part3Len += RootNode._actions.Children.Count * 8;

            //part3Len += GetSize(node._actionPre);

            //#endregion
            //#region Part 4

            ////Subaction flags
            //lookupCount++; //offset to the list
            //foreach (SubActionGroup g in RootNode._subActions.Children)
            //{
            //    if (g.Name != "<null>")
            //    {
            //        lookupCount++;
            //        subActionTable.Add(g.Name);
            //    }
            //    part4Len += 8;
            //}

            ////Subaction string table
            //part4Len += subActionTable.TotalSize;

            //#endregion
            //#region Part 5

            ////Subaction offsets already written
            //lookupCount += 4; //offset to the lists

            ////Add length of offsets to each subaction event list
            ////Multiply count by 16 (4 types (main, gfx, etc) * 1 uint offset size (4 bytes))
            //part5Len += RootNode._subActions.Children.Count * 16;
            //part5Len += GetSize(node.mdlVisibility);
            //part5Len += GetSize(node._misc.unkSection3);
            //part5Len += GetSize(node.boneRef2);

            //if (node.nanaSubActions != null)
            //{
            //    foreach (SubActionGroup g in node.nanaSubActions.Children)
            //        if (g.Name != "<null>")
            //        {
            //            foreach (ActionScript a in g.Children)
            //                if (a.Children.Count > 0 || a._actionRefs.Count > 0 || a._build)
            //                {
            //                    part5Len += GetSize(a);
            //                    lookupCount++;
            //                }
            //        }
            //    part5Len += node.nanaSubActions.Children.Count * 16;
            //}

            //part5Len += GetSize(node._misc.collisionData);
            //part5Len += GetSize(node.unk24);
            //part5Len += GetSize(node._misc.unk12);
            //part5Len += GetSize(node._unk22);

            //#endregion
            //#region Part 6

            //part6Len += GetSize(node.staticArticles);
            //part6Len += GetSize(node.entryArticle);

            //foreach (MovesetEntry e in node._articles.Values)
            //    part6Len += GetSize(e);

            //part6Len += GetSize(node._misc.unkSection2);

            //if (node.nanaSoundData != null)
            //{
            //    foreach (MoveDefSoundDataNode r in node.nanaSoundData.Children)
            //    {
            //        lookupCount += (r.Children.Count > 0 ? 1 : 0);
            //        part6Len += 8 + r.Children.Count * 4;
            //    }
            //}

            //part6Len += GetSize(node.actionInterrupts);
            //part6Len += GetSize(node.boneFloats1);
            //part6Len += GetSize(node.boneFloats2);
            //part6Len += GetSize(node.boneFloats3);
            //part6Len += GetSize(node.boneRef1);
            //part6Len += GetSize(node._misc.boneRefs);
            //part6Len += GetSize(node._misc.unkSection5);
            //part6Len += GetSize(node._misc.soundData);

            //#endregion
            //#region Part 7

            ////Misc Section
            //part7Len += 0x4C;
            //lookupCount++; //data offset to the misc section

            //#endregion

            ////There's gonna be an offset to every entry
            //lookupCount += node._extraEntries.Count;

            //node.subActionTable = subActionTable;
            //node._lookupCount = lookupCount;

            //return (node._childLength = 
            //(node.part1Len = part1Len) +
            //(node.part2Len = part2Len) +
            //(node.part3Len = part3Len) +
            //(node.part4Len = part4Len) +
            //(node.part5Len = part5Len) +
            //(node.part6Len = part6Len) +
            //(node.part7Len = part7Len));
            return 0;
        }

        public int GetSize(MovesetEntry node)
        {
            //if (node != null)
            //{
            //    int size = 0;
            //    if (!(node.External && !(node._externalEntry is MoveDefReferenceEntryNode)))
            //    {
            //        node._lookupOffsets = new List<VoidPtr>();

            //        if ((  node.Parent is DataSection 
            //            || node.Parent is MoveDefMiscNode 
            //            || node.Parent is DataCommonSection 
            //            || node.Parent is AnimParamSection
            //            //|| node.Parent is MoveDefSubParamNode
            //            ) && !node.isExtra)
            //            lookupCount++;

            //        size = node.CalculateSize(true);
            //        lookupCount += node._lookupCount;
            //    }
            //    return size;
            //}
            //else 
                return 0;
        }

        //Data order: Other, GFX, SFX, Main
        //Offset order: Main, GFX, SFX, Other
        //public void WriteSubactionData(bint* addr, int index, MoveDefActionListNode list)
        //{
        //    bint* offsets = addr + list.Children.Count * index;

        //    foreach (SubActionGroup grp in list.Children)
        //        if (grp.Name != "<null>")
        //        {
        //            ActionScript action = grp.Children[index] as ActionScript;
        //            if (((action.Children.Count > 0 || action._actionRefs.Count > 0 || action._build)))
        //            {
        //                offsets[grp.Index] = Rebuild(action);
        //                _lookupOffsets.Add(&offsets[grp.Index]);
        //            }
        //            else offsets[grp.Index] = 0;
        //        }
        //}

        //public void WriteActionData(bint* addr, MoveDefActionListNode list)
        //{
        //    bint* addr2 = addr + list.Children.Count;
        //    foreach (SubActionGroup grp in list.Children.Cast<MoveDefSubActionGroupNode>().OrderBy(x => ((ActionScript)x.Children[0])._offset))
        //    {
        //        ActionScript action1 = grp.Children[0] as ActionScript;
        //        ActionScript action2 = grp.Children[1] as ActionScript;
        //        if (((action1.Children.Count > 0 || action1._actionRefs.Count > 0 || action1._build)))
        //        {
        //            addr[grp.Index] = Rebuild(action1);
        //            _lookupOffsets.Add(&addr[grp.Index]);
        //        }
        //        else addr[grp.Index] = 0;
        //        if (((action2.Children.Count > 0 || action2._actionRefs.Count > 0 || action2._build)))
        //        {
        //            addr2[grp.Index] = Rebuild(action2);
        //            _lookupOffsets.Add(&addr2[grp.Index]);
        //        }
        //        else addr2[grp.Index] = 0;
        //    }
        //}

        internal unsafe void BuildData(DataSection node, DataHeader* header, VoidPtr address)
        {
            //MovesetFile RootNode = node.Parent.Parent as MovesetFile;

            //_dataAddress = address;
            //node._rebuildAddr = header;

            //VoidPtr extraOffsets = (VoidPtr)header + 124;
            //bint* action1Offsets = (bint*)(_dataAddress + node.part1Len + node.part2Len);
            //bint* action2Offsets = action1Offsets + RootNode._actions.Children.Count;
            //bint* mainOffsets = (bint*)(_dataAddress + (node.part1Len + node.part2Len + node.part3Len + node.part4Len));
            //bint* GFXOffsets = (bint*)((VoidPtr)mainOffsets + RootNode._subActions.Children.Count * 4);
            //bint* SFXOffsets =  (bint*)((VoidPtr)GFXOffsets + RootNode._subActions.Children.Count * 4);
            //bint* otherOffsets = (bint*)((VoidPtr)SFXOffsets + RootNode._subActions.Children.Count * 4);

            //FDefMiscSection* miscOffsetsAddr = (FDefMiscSection*)(_dataAddress + (node._childLength - 0x4C));

            //if (node != null)
            //{
            //    node._rebuildAddr = header;
            //    if (node._misc != null)
            //    {
            //        node._misc._rebuildAddr = miscOffsetsAddr;
            //        header->MiscSectionOffset = (int)miscOffsetsAddr - (int)_baseAddress;
            //    }
            //}

            //header->SubactionMainStart = (int)mainOffsets - (int)_baseAddress;
            //header->SubactionGFXStart = (int)GFXOffsets - (int)_baseAddress;
            //header->SubactionSFXStart = (int)SFXOffsets - (int)_baseAddress;
            //header->SubactionOtherStart = (int)otherOffsets - (int)_baseAddress;
            //header->EntryActionsStart = (int)action1Offsets - (int)_baseAddress;
            //header->ExitActionsStart = (int)action2Offsets - (int)_baseAddress;

            //#region Part 1

            ////Perform check to make sure we're on the right track
            //if ((int)_dataAddress - (int)_baseAddress != 0)
            //    Console.WriteLine("p1");

            //header->AttributeStart = Rebuild(node._attributes);
            //header->SSEAttributeStart = Rebuild(node._sseAttributes);
            //header->CommonActionFlagsStart = Rebuild(node._commonActionFlags);
            //header->Unknown7 = Rebuild(node._unk7);

            //WriteSubactionData(mainOffsets, 3, RootNode._subActions);
            //WriteSubactionData(mainOffsets, 1, RootNode._subActions);
            //WriteSubactionData(mainOffsets, 2, RootNode._subActions);
            //WriteSubactionData(mainOffsets, 0, RootNode._subActions);

            ////RebuildArticleActions(RootNode, node, ref _dataAddress, _rebuildBase, true, 1);

            ////if ((miscOffsetsAddr->UnkBoneSectionOffset = Rebuild(node._misc.unkBoneSection)) > 0)
            ////    miscOffsetsAddr->UnkBoneSectionCount = node._misc.unkBoneSection.Children.Count;

            ////RebuildArticleActions(RootNode, node, ref _dataAddress, _rebuildBase, true, 2);

            ////if (node._misc.soundData != null)
            ////    foreach (MoveDefSoundDataNode r in node._misc.soundData.Children)
            ////    {
            ////        foreach (MoveDefIndexNode b in r.Children)
            ////        {
            ////            b._entryOffset = _dataAddress;
            ////            *(bint*)_dataAddress = b.ItemIndex;
            ////            _dataAddress += 4;
            ////        }
            ////    }

            ////RebuildArticleActions(RootNode, node, ref _dataAddress, _rebuildBase, true, 0);

            //WriteActionData(action1Offsets, RootNode._actions);

            //#endregion
            //#region Part 2

            ////Perform check to make sure we're on the right track
            //if ((int)_dataAddress - (int)_baseAddress != node.part1Len)
            //    Console.WriteLine("p2");

            //miscOffsetsAddr->UnknownSection1Offset = Rebuild(node._misc.unkSection1);

            //if ((miscOffsetsAddr->HurtBoxOffset = Rebuild(node._misc.hurtBoxes)) > 0)
            //    miscOffsetsAddr->HurtBoxCount = node._misc.hurtBoxes.Children.Count;

            ////if (node.misc.collisionData != null)
            ////    foreach (MoveDefOffsetNode offset in node.misc.collisionData.Children)
            ////        if (offset.Children.Count > 0 && !(offset.Children[0] as MoveDefEntryNode).External)
            ////            foreach (MoveDefBoneIndexNode b in offset.Children[0].Children)
            ////            {
            ////                b._entryOffset = dataAddress;
            ////                *(bint*)dataAddress = b.boneIndex;
            ////                dataAddress += 4;
            ////            }

            //if ((miscOffsetsAddr->LedgegrabOffset = Rebuild(node._misc.ledgeGrabs)) > 0)
            //    miscOffsetsAddr->LedgegrabCount = node._misc.ledgeGrabs.Children.Count;

            //miscOffsetsAddr->TetherOffset = Rebuild(node._misc.tether);
            //miscOffsetsAddr->CrawlOffset = Rebuild(node._misc.crawl);
            //miscOffsetsAddr->MultiJumpOffset = Rebuild(node._misc.multiJump);
            //miscOffsetsAddr->GlideOffset = Rebuild(node._misc.glide);

            //for (int i = 0; i < RootNode._subRoutines.Count; i++)
            //    if ((RootNode._subRoutines[i] as ActionScript)._actionRefs.Count > 0)
            //        Rebuild(RootNode._subRoutines[i] as ActionScript);

            //if (node._unk22 != null)
            //    if (node._unk22.Children.Count > 0)
            //        Rebuild(node._unk22.Children[0] as ActionScript);

            //if (node._override1 != null)
            //    foreach (MoveDefActionOverrideEntryNode e in node._override1.Children)
            //        Rebuild(e.Children[0] as ActionScript);

            //if (node._override2 != null)
            //    foreach (MoveDefActionOverrideEntryNode e in node._override2.Children)
            //        Rebuild(e.Children[0] as ActionScript);

            //RebuildArticleActions(RootNode, node, ref _dataAddress, _baseAddress, false, 0);

            //header->ActionFlagsStart = Rebuild(node._actionFlags);

            //#endregion
            //#region Part 3

            ////Perform check to make sure we're on the right track
            //if ((int)_dataAddress - (int)_baseAddress != node.part1Len + node.part2Len)
            //    Console.WriteLine("p3");

            ////Actions part 1 and 2 already written
            //_dataAddress += RootNode._actions.Children.Count * 8;

            //header->ActionPreStart = Rebuild(node._actionPre);

            //#endregion
            //#region Part 4

            ////Perform check to make sure we're on the right track
            //if ((int)_dataAddress - (int)_baseAddress != node.part1Len + node.part2Len + node.part3Len)
            //    Console.WriteLine("p4");

            //node.subActionTable.WriteTable(_dataAddress);
            //_dataAddress += node.subActionTable.TotalSize;
            //header->SubactionFlagsStart = (int)_dataAddress - (int)_baseAddress;

            //int index = 0;
            //FDefSubActionFlag* flags = (FDefSubActionFlag*)_dataAddress;
            //foreach (SubActionGroup g in RootNode._subActions.Children)
            //{
            //    *flags = new FDefSubActionFlag() { _InTranslationTime = g._inTransTime, _Flags = g._flags, _stringOffset = (g.Name == "<null>" ? 0 : (int)node.subActionTable[g.Name] - (int)_baseAddress) };
            //    if (flags->_stringOffset > 0)
            //    {
            //        if (index == 412)
            //            node.zssFirstOffset = (int)flags->_stringOffset.Address - (int)_baseAddress;
            //        if (index == 317)
            //            node.warioSwing4StringOffset = (int)flags->_stringOffset.Address - (int)_baseAddress;

            //        _lookupOffsets.Add(flags->_stringOffset.Address);
            //    }
                
            //    flags++;
            //    index++;
            //}

            //_dataAddress = flags;

            //#endregion
            //#region Part 5

            //if ((int)_dataAddress - (int)_baseAddress != node.part1Len + node.part2Len + node.part3Len + node.part4Len)
            //    Console.WriteLine("p5");

            ////Subaction offsets already written
            //_dataAddress += RootNode._subActions.Children.Count * 16;

            //header->ModelVisibilityStart = Rebuild(node.mdlVisibility);
            //miscOffsetsAddr->UnknownSection3Offset = Rebuild(node._misc.unkSection3);
            //header->BoneRef2 = Rebuild(node.boneRef2);

            //if (node.nanaSubActions != null)
            //{
            //    int dataOff = 0;
            //    foreach (SubActionGroup grp in node.nanaSubActions.Children)
            //        if (grp.Name != "<null>")
            //            foreach (ActionScript a in grp.Children)
            //                if (a.Children.Count > 0 || a._actionRefs.Count > 0)
            //                    dataOff += a._calcSize;

            //    bint* main2Offsets = (bint*)(_dataAddress + dataOff);
            //    bint* GFX2Offsets = (bint*)((VoidPtr)main2Offsets + node.nanaSubActions.Children.Count * 4);
            //    bint* SFX2Offsets = (bint*)((VoidPtr)GFX2Offsets + node.nanaSubActions.Children.Count * 4);
            //    bint* other2Offsets = (bint*)((VoidPtr)SFX2Offsets + node.nanaSubActions.Children.Count * 4);

            //    bint* offsets = (bint*)extraOffsets;

            //    offsets[0] = (int)main2Offsets - (int)_baseAddress;
            //    offsets[1] = (int)GFX2Offsets - (int)_baseAddress;
            //    offsets[2] = (int)SFX2Offsets - (int)_baseAddress;
            //    offsets[3] = (int)other2Offsets - (int)_baseAddress;

            //    foreach (SubActionGroup grp in node.nanaSubActions.Children)
            //        if ((grp.Name != "<null>" && (grp.Children[3].Children.Count > 0 || ((ActionScript)grp.Children[3])._actionRefs.Count > 0 || ((ActionScript)grp.Children[3])._build)))
            //        {
            //            other2Offsets[grp.Index] = Rebuild(grp.Children[3] as ActionScript);
            //            _lookupOffsets.Add(&other2Offsets[grp.Index]);
            //        }
            //        else other2Offsets[grp.Index] = 0;

            //    foreach (SubActionGroup grp in node.nanaSubActions.Children)
            //        if ((grp.Name != "<null>" && (grp.Children[1].Children.Count > 0 || ((ActionScript)grp.Children[1])._actionRefs.Count > 0 || ((ActionScript)grp.Children[1])._build)))
            //        {
            //            GFX2Offsets[grp.Index] = Rebuild(grp.Children[1] as ActionScript);
            //            _lookupOffsets.Add(&GFX2Offsets[grp.Index]);
            //        }
            //        else GFX2Offsets[grp.Index] = 0;

            //    foreach (SubActionGroup grp in node.nanaSubActions.Children)
            //        if ((grp.Name != "<null>" && (grp.Children[2].Children.Count > 0 || ((ActionScript)grp.Children[2])._actionRefs.Count > 0 || ((ActionScript)grp.Children[2])._build)))
            //        {
            //            SFX2Offsets[grp.Index] = Rebuild(grp.Children[2] as ActionScript);
            //            _lookupOffsets.Add(&SFX2Offsets[grp.Index]);
            //        }
            //        else SFX2Offsets[grp.Index] = 0;

            //    foreach (SubActionGroup grp in node.nanaSubActions.Children)
            //        if ((grp.Name != "<null>" && (grp.Children[0].Children.Count > 0 || ((ActionScript)grp.Children[0])._actionRefs.Count > 0 || ((ActionScript)grp.Children[0])._build)))
            //        {
            //            main2Offsets[grp.Index] = Rebuild(grp.Children[0] as ActionScript);
            //            _lookupOffsets.Add(&main2Offsets[grp.Index]);
            //        }
            //        else main2Offsets[grp.Index] = 0;

            //    _dataAddress = (VoidPtr)other2Offsets + node.nanaSubActions.Children.Count * 4;
            //}

            //miscOffsetsAddr->CollisionDataOffset = Rebuild(node._misc.collisionData);
            //header->Unknown24 = Rebuild(node.unk24);
            //miscOffsetsAddr->UnknownSection12Offset = Rebuild(node._misc.unk12);
            //header->Unknown22 = Rebuild(node._unk22);

            //#endregion
            //#region Part 6

            //if ((int)_dataAddress - (int)_baseAddress != node.part1Len + node.part2Len + node.part3Len + node.part4Len+ node.part5Len)
            //    Console.WriteLine("p6");

            //header->StaticArticlesStart = Rebuild(node.staticArticles);
            //header->EntryArticleStart = Rebuild(node.entryArticle);
            
            //foreach (MoveDefArticleNode e in node._articles.Values)
            //    Rebuild(e);

            //if ((miscOffsetsAddr->UnknownSection2Offset = Rebuild(node._misc.unkSection2)) > 0)
            //    miscOffsetsAddr->UnknownSection2Count = node._misc.unkSection2.Children.Count;

            //if (node.nanaSoundData != null)
            //{
            //    foreach (MoveDefSoundDataNode r in node.nanaSoundData.Children)
            //        foreach (MoveDefIndexNode b in r.Children)
            //        {
            //            b._rebuildAddr = _dataAddress;
            //            *(bint*)_dataAddress = b.ItemIndex;
            //            _dataAddress += 4;
            //        }

            //    FDefListOffset* sndLists = (FDefListOffset*)(_dataAddress);
            //    FDefListOffset* data = (FDefListOffset*)(extraOffsets + 40);

            //    node.nanaSoundData._rebuildAddr = data;

            //    if (node.nanaSoundData.Children.Count > 0)
            //    {
            //        data->_startOffset = (int)sndLists - (int)_baseAddress;
            //        _lookupOffsets.Add(data->_startOffset.Address);
            //    }

            //    data->_listCount = node.nanaSoundData.Children.Count;

            //    foreach (MoveDefSoundDataNode r in node.nanaSoundData.Children)
            //    {
            //        if (r.Children.Count > 0)
            //        {
            //            sndLists->_startOffset = (int)(r.Children[0] as MovesetEntry)._rebuildAddr - (int)_baseAddress;
            //            _lookupOffsets.Add(sndLists->_startOffset.Address);
            //        }
            //        (sndLists++)->_listCount = r.Children.Count;
            //    }
            //    _dataAddress = sndLists;
            //}

            //header->ActionInterrupts = Rebuild(node.actionInterrupts);
            //header->BoneFloats1 = Rebuild(node.boneFloats1);
            //header->BoneFloats2 = Rebuild(node.boneFloats2);
            //header->BoneFloats3 = Rebuild(node.boneFloats3);
            //header->BoneRef1 = Rebuild(node.boneRef1);
            
            //miscOffsetsAddr->BoneRef2Offset = Rebuild(node._misc.boneRefs);
            //miscOffsetsAddr->UnknownSection5Offset = Rebuild(node._misc.unkSection5);
            //miscOffsetsAddr->SoundDataOffset = Rebuild(node._misc.soundData);

            //#endregion
            //#region Part 7

            //if ((int)_dataAddress - (int)_baseAddress != node.part1Len + node.part2Len + node.part3Len + node.part4Len+ node.part5Len + node.part6Len)
            //    Console.WriteLine("p7");

            ////Misc section, already written
            //_dataAddress += 0x4C;

            //#endregion

            ////Params

            //ExtraDataOffsets.GetOffsets(RootNode._character).Write(node._extraEntries, _lookupOffsets, _baseAddress, extraOffsets);

            ////int l = 0, ind = 0;
            ////foreach (int i in node._extraOffsets)
            ////{
            ////    if (i > 1480 && i < RootNode.dataSize)
            ////    {
            ////        MoveDefEntryNode e = node._extraEntries[l];

            ////         extraOffsets[ind] = (int)e._entryOffset - (int)baseAddress;

            ////        _lookupOffsets.Add(&extraOffsets[ind]);

            ////        l++;
            ////    }
            ////    else
            ////        extraOffsets[ind] = i;
            ////    ind++;
            ////}

            //header->EntryActionOverrides = ((node._override1 != null && node._override1.External) ? (int)node._override1._rebuildAddr - (int)_baseAddress : 0);
            //header->ExitActionOverrides = ((node._override2 != null && node._override2.External) ? (int)node._override2._rebuildAddr - (int)_baseAddress : 0);

            //header->Unknown27 = node.Unknown27;
            //header->Unknown28 = node.Unknown28;
            //header->Flags1 = node.Flags1uint;
            //header->Flags2 = node.Flags2int;

            //bint* hdr = (bint*)header;
            //for (int i = 0; i < 27; i++)
            //    if (hdr[i] > 0)
            //        _lookupOffsets.Add(&hdr[i]);

            //hdr = (bint*)miscOffsetsAddr;
            //for (int i = 0; i < 19; i++)
            //    if (hdr[i] > 0 && !(i % 2 == 0 && i > 0 && i < 9))
            //        _lookupOffsets.Add(&hdr[i]);

            ////Go back and add offsets to nodes that need them
            //foreach (MovesetEntry entry in _postProcessNodes)
            //    entry.PostProcess(_lookupOffsets);
        }
        
        public int Rebuild(MovesetEntry node)
        {
            //if (node != null)
            //{
            //    if (!(node.External && !(node._externalEntry is MoveDefReferenceEntryNode)))
            //    {
            //        node.Rebuild(_dataAddress, node._calcSize, true);
            //        _dataAddress += node._calcSize;

            //        if (node._lookupOffsets.Count != node._lookupCount && !(node is ActionScript))
            //            Console.WriteLine(node.TreePath + (node._lookupCount - node._lookupOffsets.Count));

            //        _lookupOffsets.AddRange(node._lookupOffsets.ToArray());
            //    }
            //    return node.RebuildOffset;
            //}
            //else 
                return 0;
        }

        //public int CalcSizeArticleActions(DataSection node, bool subactions, int index)
        //{
        //    int size = 0;
        //    if (node.staticArticles != null && node.staticArticles.Children.Count > 0)
        //        foreach (MoveDefArticleNode d in node.staticArticles.Children)
        //            if (!subactions)
        //            {
        //                if (d._actions != null)
        //                    foreach (ActionScript a in d._actions.Children)
        //                        if (a.Children.Count > 0)
        //                            size += GetSize(a);
        //            }
        //            else
        //            {
        //                if (d._subActions != null)
        //                    foreach (SubActionGroup grp in d._subActions.Children)
        //                        if (grp.Children[index].Children.Count > 0 || (grp.Children[index] as ActionScript)._actionRefs.Count > 0 || (grp.Children[index] as ActionScript)._build)
        //                            size += GetSize((grp.Children[index] as ActionScript));
        //            }

        //    if (node.entryArticle != null)
        //        if (!subactions)
        //        {
        //            if (node.entryArticle._actions != null)
        //                foreach (ActionScript a in node.entryArticle._actions.Children)
        //                    if (a.Children.Count > 0)
        //                        size += GetSize(a);
        //        }
        //        else
        //        {
        //            if (node.entryArticle._subActions != null)
        //                foreach (SubActionGroup grp in node.entryArticle._subActions.Children)
        //                    if (grp.Children[index].Children.Count > 0 || (grp.Children[index] as ActionScript)._actionRefs.Count > 0 || (grp.Children[index] as ActionScript)._build)
        //                        size += GetSize((grp.Children[index] as ActionScript));
        //        }

        //    foreach (MoveDefArticleNode d in node._articles.Values)
        //        if (!subactions)
        //        {
        //            if (d._actions != null)
        //            {
        //                if (d.pikmin)
        //                {
        //                    foreach (ActionGroup grp in d._actions.Children)
        //                        foreach (ActionScript a in grp.Children)
        //                            if (a.Children.Count > 0)
        //                                size += GetSize(a);
        //                }
        //                else
        //                    foreach (ActionScript a in d._actions.Children)
        //                        if (a.Children.Count > 0)
        //                            size += GetSize(a);
        //            }
        //        }
        //        else
        //        {
        //            if (d._subActions != null)
        //            {
        //                MovesetEntry e = d._subActions;
        //                int populateCount = 1;
        //                bool children = false;
        //                if (d._subActions.Children[0] is MoveDefActionListNode)
        //                {
        //                    populateCount = d._subActions.Children.Count;
        //                    children = true;
        //                }
        //                for (int i = 0; i < populateCount; i++)
        //                {
        //                    if (children)
        //                        e = d._subActions.Children[i] as MovesetEntry;

        //                    foreach (SubActionGroup grp in e.Children)
        //                        if (grp.Children[index].Children.Count > 0 || (grp.Children[index] as ActionScript)._actionRefs.Count > 0 || (grp.Children[index] as ActionScript)._build)
        //                            size += GetSize((grp.Children[index] as ActionScript));
        //                }
        //            }
        //        }
            
        //    return size;
        //}

        //public void RebuildArticleActions(MovesetFile RootNode, DataSection node, ref VoidPtr dataAddress, VoidPtr baseAddress, bool subactions, int index)
        //{
        //    if (node.staticArticles != null && node.staticArticles.Children.Count > 0)
        //        foreach (MoveDefArticleNode d in node.staticArticles.Children)
        //            if (!subactions)
        //            {
        //                if (d._actions != null)
        //                    foreach (ActionScript a in d._actions.Children)
        //                        if (a.Children.Count > 0)
        //                            Rebuild(a);
        //            }
        //            else
        //            {
        //                if (d._subActions != null)
        //                    foreach (SubActionGroup grp in d._subActions.Children)
        //                        if (grp.Children[index].Children.Count > 0 || (grp.Children[index] as ActionScript)._actionRefs.Count > 0 || (grp.Children[index] as ActionScript)._build)
        //                            Rebuild(grp.Children[index] as ActionScript);
        //            }

        //    if (node.entryArticle != null)
        //        if (!subactions)
        //        {
        //            if (node.entryArticle._actions != null)
        //                foreach (ActionScript a in node.entryArticle._actions.Children)
        //                    if (a.Children.Count > 0)
        //                        Rebuild(a);
        //        }
        //        else
        //        {
        //            if (node.entryArticle._subActions != null)
        //                foreach (SubActionGroup grp in node.entryArticle._subActions.Children)
        //                    if (grp.Children[index].Children.Count > 0 || (grp.Children[index] as ActionScript)._actionRefs.Count > 0 || (grp.Children[index] as ActionScript)._build)
        //                        Rebuild(grp.Children[index] as ActionScript);
        //        }
            
        //    foreach (MoveDefArticleNode d in node._articles.Values)
        //        if (!subactions)
        //        {
        //            if (d._actions != null)
        //            {
        //                if (d.pikmin)
        //                {
        //                    foreach (ActionGroup grp in d._actions.Children)
        //                        foreach (ActionScript a in grp.Children)
        //                            if (a.Children.Count > 0)
        //                                Rebuild(a);
        //                }
        //                else
        //                    foreach (ActionScript a in d._actions.Children)
        //                        if (a.Children.Count > 0)
        //                            Rebuild(a);
        //            }
        //        }
        //        else
        //        {
        //            if (d._subActions != null)
        //            {
        //                MovesetEntry e = d._subActions;
        //                int populateCount = 1;
        //                bool children = false;
        //                if (d._subActions.Children[0] is MoveDefActionListNode)
        //                {
        //                    populateCount = d._subActions.Children.Count;
        //                    children = true;
        //                }
        //                for (int i = 0; i < populateCount; i++)
        //                {
        //                    if (children)
        //                        e = d._subActions.Children[i] as MovesetEntry;

        //                    foreach (SubActionGroup grp in e.Children)
        //                        if (grp.Children[index].Children.Count > 0 || (grp.Children[index] as ActionScript)._actionRefs.Count > 0 || (grp.Children[index] as ActionScript)._build)
        //                            Rebuild(grp.Children[index] as ActionScript);
        //                }
        //            }
        //        }
        //}
    }

    /// <summary>
    /// When rebuilding, add the addresses of all offset values to this collection
    /// </summary>
    public class LookupManager : IEnumerable<VoidPtr>
    {
        private List<VoidPtr> _values = new List<VoidPtr>();
        public int Count { get { return _values.Count; } }
        public VoidPtr this[int index]
        {
            get
            {
                if (index >= 0 && index < _values.Count)
                    return _values[index];
                return null;
            }
            set
            {
                if (index >= 0 && index < _values.Count)
                    _values[index] = value;
            }
        }

        public void Add(VoidPtr value)
        {
            if (!_values.Contains(value))
                _values.Add(value);
        }
        public void AddRange(VoidPtr[] vals)
        {
            foreach (VoidPtr value in vals)
                if (!_values.Contains(value))
                    _values.Add(value);
        }

        public IEnumerator<VoidPtr> GetEnumerator() { return _values.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

        internal void Sort()
        {
            _values.Sort();
        }
    }
}