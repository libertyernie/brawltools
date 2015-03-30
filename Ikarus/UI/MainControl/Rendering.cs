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
using Ikarus;
using Ikarus.MovesetFile;
using Ikarus.ModelViewer;

namespace Ikarus.UI
{
    public partial class MainControl : ModelEditorBase
    {
        public unsafe override void modelPanel1_PreRender(ModelPanelViewport sender)
        {
            base.modelPanel1_PreRender(sender);
            Attributes.PreRender();
        }

        public unsafe override void modelPanel1_PostRender(ModelPanelViewport sender)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);

            Attributes.PostRender();

            //Render hurtboxes
            if (chkHurtboxes.Checked)
                for (int i = 0; i < listPanel.lstHurtboxes.Items.Count; i++)
                    if (listPanel.lstHurtboxes.GetItemChecked(i))
                        ((MiscHurtBox)listPanel.lstHurtboxes.Items[i]).Render(SelectedHurtbox != null && SelectedHurtbox.Index == i, Scriptor._hurtBoxType);

            //Render hitboxes
            if (chkHitboxes.Checked && Manager.Moveset != null)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GLDisplayList c = TKContext.GetRingList();
                GLDisplayList s = TKContext.GetSphereList();

                foreach (HitBox e in RunTime._hitBoxes)
                    e.Render(modelPanel.Camera.GetPoint());
            }

            base.modelPanel1_PostRender(sender);
        }
    }
}
