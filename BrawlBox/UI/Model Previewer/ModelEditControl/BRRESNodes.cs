using System;
using BrawlLib.OpenGL;
using System.ComponentModel;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;
using BrawlLib.Modeling;
using System.Drawing;
using BrawlLib.Wii.Animations;
using System.Collections.Generic;
using BrawlLib.SSBBTypes;
using BrawlLib.IO;
using BrawlLib;
using System.Drawing.Imaging;
using Gif.Components;
using OpenTK.Graphics.OpenGL;
using BrawlLib.Imaging;

namespace System.Windows.Forms
{
    public partial class ModelEditControl : UserControl, IMainWindow
    {
        public void GetFiles(AnimType focusType)
        {
            if (focusType == AnimType.None)
            {
                focusType = TargetAnimType;
                if (focusType != AnimType.CHR) _chr0 = null;
                if (focusType != AnimType.SRT) _srt0 = null;
                if (focusType != AnimType.SHP) _shp0 = null;
                if (focusType != AnimType.PAT) _pat0 = null;
                if (focusType != AnimType.VIS) _vis0 = null;
                if (focusType != AnimType.SCN) _scn0 = null;
                if (focusType != AnimType.CLR) _clr0 = null;
            }
            else
            {
                if (focusType != AnimType.CHR) GetCHR0(focusType);
                if (focusType != AnimType.SRT) GetSRT0(focusType);
                if (focusType != AnimType.SHP) GetSHP0(focusType);
                if (focusType != AnimType.PAT) GetPAT0(focusType);
                if (focusType != AnimType.VIS) GetVIS0(focusType);
                if (focusType != AnimType.SCN) GetSCN0(focusType);
                if (focusType != AnimType.CLR) GetCLR0(focusType);
            }
        }
        public bool GetSCN0(AnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _scn0 = null;
                return false;
            }
            if (TargetModel != null && (_scn0 = (SCN0Node)TargetModel.RootNode.FindChildByType(focusFile.Name, true, ResourceType.SCN0)) != null)
                return true;
            if (_externalAnimationsNode != null && (_scn0 = (SCN0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.SCN0)) != null)
                return true;
            return false;
        }
        public bool GetCLR0(AnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _clr0 = null;
                return false;
            }
            if (TargetModel != null && (_clr0 = (CLR0Node)TargetModel.RootNode.FindChildByType(focusFile.Name, true, ResourceType.CLR0)) != null)
                return true;
            if (_externalAnimationsNode != null && (_clr0 = (CLR0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.CLR0)) != null)
                return true;
            return false;
        }
        public bool GetVIS0(AnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _vis0 = null;
                return false;
            }
            if (TargetModel != null && (_vis0 = (VIS0Node)TargetModel.RootNode.FindChildByType(focusFile.Name, true, ResourceType.VIS0)) != null)
                return true;
            if (_externalAnimationsNode != null && (_vis0 = (VIS0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.VIS0)) != null)
                return true;
            return false;
        }
        public bool GetPAT0(AnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _pat0 = null;
                return false;
            }
            if (TargetModel != null && (_pat0 = (PAT0Node)TargetModel.RootNode.FindChildByType(focusFile.Name, true, ResourceType.PAT0)) != null)
                return true;
            if (_externalAnimationsNode != null && (_pat0 = (PAT0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.PAT0)) != null)
                return true;
            return false;
        }
        public bool GetSRT0(AnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _srt0 = null;
                return false;
            }
            if (TargetModel != null && (_srt0 = (SRT0Node)TargetModel.RootNode.FindChildByType(focusFile.Name, true, ResourceType.SRT0)) != null)
                return true;
            if (_externalAnimationsNode != null && (_srt0 = (SRT0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.SRT0)) != null)
                return true;
            return false;
        }
        public bool GetSHP0(AnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _shp0 = null;
                return false;
            }
            if (TargetModel != null && (_shp0 = (SHP0Node)TargetModel.RootNode.FindChildByType(focusFile.Name, true, ResourceType.SHP0)) != null)
                return true;
            if (_externalAnimationsNode != null && (_shp0 = (SHP0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.SHP0)) != null)
                return true;
            return false;
        }
        public bool GetCHR0(AnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _chr0 = null;
                return false;
            }
            if (TargetModel != null && (_chr0 = (CHR0Node)TargetModel.RootNode.FindChildByType(focusFile.Name, true, ResourceType.CHR0)) != null)
                return true;
            if (_externalAnimationsNode != null && (_chr0 = (CHR0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.CHR0)) != null)
                return true;
            return false;
        }

        public void CreateVIS0()
        {
            BRESNode group = null;
            BRESEntryNode n = null;
            if ((n = TargetAnimation as BRESEntryNode) != null &&
                (group = n.Parent.Parent as BRESNode) != null)
            {
                _vis0 = group.CreateResource<VIS0Node>(SelectedCHR0.Name);
                foreach (string s in leftPanel.VIS0Indices.Keys)
                {
                    VIS0EntryNode node = null;
                    if ((node = (VIS0EntryNode)_vis0.FindChild(s, true)) == null && ((MDL0BoneNode)_targetModel.FindChildByType(s, true, ResourceType.MDL0Bone)).BoneIndex != 0 && s != "EyeYellowM")
                    {
                        node = _vis0.CreateEntry();
                        node.Name = s;
                        node.MakeConstant(true);
                    }
                }
            }
        }

        public void UpdateVis0(object sender, EventArgs e)
        {
            BRESEntryNode n;
            if ((n = TargetAnimation as BRESEntryNode) == null || _animFrame == 0)
                return;

            Start:
            if (_vis0 != null)
            {
                int index = leftPanel._polyIndex;
                if (index == -1)
                    return;

                MDL0BoneNode bone = ((MDL0ObjectNode)leftPanel.lstObjects.Items[index])._bone;

                VIS0EntryNode node = null;
                if ((node = (VIS0EntryNode)_vis0.FindChild(bone.Name, true)) == null && bone.BoneIndex != 0 && bone.Name != "EyeYellowM")
                {
                    node = _vis0.CreateEntry();
                    node.Name = bone.Name;
                    node.MakeConstant(true);
                }

                //Item is in the process of being un/checked; it's not un/checked at the given moment.
                //Use opposite of current check state.
                bool ANIMval = !leftPanel.lstObjects.GetItemChecked(index);

                bool nowAnimated = false, alreadyConstant = false;
            Top:
                if (node != null)
                    if (node._entryCount != 0) //Node is animated
                    {
                        bool VIS0val = node.GetEntry(_animFrame - 1);

                        if (VIS0val != ANIMval)
                            node.SetEntry(_animFrame - 1, ANIMval);
                    }
                    else //Node is constant
                    {
                        alreadyConstant = true;

                        bool VIS0val = node._flags.HasFlag(VIS0Flags.Enabled);

                        if (VIS0val != ANIMval)
                        {
                            node.MakeAnimated();
                            nowAnimated = true;
                            goto Top;
                        }
                    }

                //Check if the entry can be made constant.
                //No point if the entry has just been made animated or if the node is already constant.
                if (node != null && !alreadyConstant && !nowAnimated)
                {
                    bool constant = true;
                    for (int i = 0; i < node._entryCount; i++)
                    {
                        if (i == 0)
                            continue;

                        if (node.GetEntry(i - 1) != node.GetEntry(i))
                        {
                            constant = false;
                            break;
                        }
                    }
                    if (constant) node.MakeConstant(node.GetEntry(0));
                }

                if (node != null && ((VIS0EntryNode)rightPanel.pnlKeyframes.visEditor.TargetNode).Name == node.Name)
                    vis0Editor.UpdateEntry();
            }
            else
            {
                CreateVIS0();
                if (_vis0 != null) 
                    goto Start;
            }
        }
        public void ReadVIS0()
        {
            if (_animFrame == 0 || leftPanel.lstObjects.Items.Count == 0)
                return;

            leftPanel._vis0Updating = true;
            if (_vis0 != null)
            {
                //if (TargetAnimation != null && _vis0.FrameCount != TargetAnimation.tFrameCount)
                //    UpdateVis0(null, null);

                foreach (string n in leftPanel.VIS0Indices.Keys)
                {
                    VIS0EntryNode node = null;
                    List<int> indices = leftPanel.VIS0Indices[n];
                    for (int i = 0; i < indices.Count; i++)
                    {
                        if ((node = (VIS0EntryNode)_vis0.FindChild(((MDL0ObjectNode)leftPanel.lstObjects.Items[indices[i]])._bone.Name, true)) != null)
                        {
                            if (node._entryCount != 0 && _animFrame > 0)
                                leftPanel.lstObjects.SetItemChecked(indices[i], node.GetEntry(_animFrame - 1));
                            else
                                leftPanel.lstObjects.SetItemChecked(indices[i], node._flags.HasFlag(VIS0Flags.Enabled));
                        }
                    }
                }
            }
            leftPanel._vis0Updating = false;
        }
    }
}
