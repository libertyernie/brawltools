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
    public partial class MainControl : ModelEditorBase
    {
        protected override NW4RAnimationNode FindCorrespondingAnimation(NW4RAnimationNode focusFile, NW4RAnimType targetType)
        {
            string name = focusFile.Name;
            if (listPanel._animations.ContainsKey(name) && 
                listPanel._animations[name].ContainsKey(targetType))
                return listPanel._animations[name][targetType];
            return null;
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
                        if ((node = (VIS0EntryNode)_vis0.FindChild(((MDL0ObjectNode)modelListsPanel1.lstObjects.Items[indices[i]])._visBoneNode.Name, true)) != null)
                            if (node._entryCount != 0 && CurrentFrame > 0)
                                modelListsPanel1.lstObjects.SetItemChecked(indices[i], node.GetEntry(CurrentFrame - 1));
                            else
                                modelListsPanel1.lstObjects.SetItemChecked(indices[i], node._flags.HasFlag(VIS0Flags.Enabled));
                }
            listPanel._vis0Updating = false;
        }
    }
}
