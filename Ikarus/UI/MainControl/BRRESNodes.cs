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
using System.Windows.Forms;

namespace Ikarus.UI
{
    public partial class MainControl : UserControl, IMainWindow
    {
        public void GetFiles(NW4RAnimType focusType)
        {
            _updating = true;
            if (focusType == NW4RAnimType.None)
            {
                focusType = TargetAnimType;
                for (int i = 0; i < 6; i++)
                    if ((int)focusType != i)
                        SetSelectedBRRESFile((NW4RAnimType)i, null);
            }
            else
            {
                for (int i = 0; i < 6; i++)
                    if ((int)focusType != i)
                        RetrieveAnimation(focusType, (NW4RAnimType)i);
            }
            _updating = false;
        }
        public bool RetrieveAnimation(NW4RAnimType focusType, NW4RAnimType type)
        {
            AnimationNode f = GetAnimation(focusType);
            if (f == null)
            {
                SetSelectedBRRESFile(type, null);
                return false;
            }
            SetSelectedBRRESFile(type, RetrieveAnimation(f.Name, type));
            return GetAnimation(type) != null;
        }
        public AnimationNode RetrieveAnimation(string name, NW4RAnimType type)
        {
            if (listPanel._animations.ContainsKey(name) && listPanel._animations[name].ContainsKey(type))
                return listPanel._animations[name][type];
            return null;
        }

        public void CreateVIS0()
        {
            BRESNode group = null;
            BRESEntryNode n = null;
            if ((n = GetAnimation(TargetAnimType) as BRESEntryNode) != null &&
                (group = n.Parent.Parent as BRESNode) != null)
            {
                _vis0 = group.CreateResource<VIS0Node>(SelectedCHR0.Name);
                foreach (string s in listPanel.VIS0Indices.Keys)
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
            if ((n = GetAnimation(TargetAnimType) as BRESEntryNode) == null || CurrentFrame == 0)
                return;

            Start:
            if (_vis0 != null)
            {
                int index = listPanel._polyIndex;
                if (index == -1)
                    return;

                MDL0BoneNode bone = ((MDL0ObjectNode)modelListsPanel1.lstObjects.Items[index])._bone;

                VIS0EntryNode node = null;
                if ((node = (VIS0EntryNode)_vis0.FindChild(bone.Name, true)) == null && bone.BoneIndex != 0 && bone.Name != "EyeYellowM")
                {
                    node = _vis0.CreateEntry();
                    node.Name = bone.Name;
                    node.MakeConstant(true);
                }

                //Item is in the process of being un/checked; it's not un/checked at the given moment.
                //Use opposite of current check state.
                bool ANIMval = !modelListsPanel1.lstObjects.GetItemChecked(index);

                bool nowAnimated = false, alreadyConstant = false;
            Top:
                if (node != null)
                    if (node._entryCount != 0) //Node is animated
                    {
                        bool VIS0val = node.GetEntry(CurrentFrame - 1);

                        if (VIS0val != ANIMval)
                            node.SetEntry(CurrentFrame - 1, ANIMval);
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
            if (CurrentFrame == 0 || modelListsPanel1.lstObjects.Items.Count == 0)
                return;

            listPanel._vis0Updating = true;
            if (_vis0 != null)
                foreach (string n in listPanel.VIS0Indices.Keys)
                {
                    VIS0EntryNode node = null;
                    List<int> indices = listPanel.VIS0Indices[n];
                    for (int i = 0; i < indices.Count; i++)
                        if ((node = (VIS0EntryNode)_vis0.FindChild(((MDL0ObjectNode)modelListsPanel1.lstObjects.Items[indices[i]])._bone.Name, true)) != null)
                            if (node._entryCount != 0 && CurrentFrame > 0)
                                modelListsPanel1.lstObjects.SetItemChecked(indices[i], node.GetEntry(CurrentFrame - 1));
                            else
                                modelListsPanel1.lstObjects.SetItemChecked(indices[i], node._flags.HasFlag(VIS0Flags.Enabled));
                }
            listPanel._vis0Updating = false;
        }
    }
}
