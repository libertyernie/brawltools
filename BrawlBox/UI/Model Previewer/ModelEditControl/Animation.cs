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
    public partial class ModelEditControl : ModelEditorBase
    {
        public int prevHeight = 0, prevWidth = 0;
        public void ToggleWeightEditor()
        {
            if (weightEditor.Visible)
            {
                animEditors.Height = prevHeight;
                animCtrlPnl.Width = prevWidth;
                weightEditor.Visible = false;
                _currentControl.Visible = true;
            }
            else
            {
                if (vertexEditor.Visible)
                    ToggleVertexEditor();

                prevHeight = animEditors.Height;
                prevWidth = animCtrlPnl.Width;
                animCtrlPnl.Width = 320;
                animEditors.Height = 78;
                weightEditor.Visible = true;
                _currentControl.Visible = false;
            }

            CheckDimensions();
        }
        public void ToggleVertexEditor()
        {
            if (vertexEditor.Visible)
            {
                animEditors.Height = prevHeight;
                animCtrlPnl.Width = prevWidth;
                vertexEditor.Visible = false;
                _currentControl.Visible = true;
            }
            else
            {
                if (weightEditor.Visible)
                    ToggleWeightEditor();

                prevHeight = animEditors.Height;
                prevWidth = animCtrlPnl.Width;
                animCtrlPnl.Width = 230;
                animEditors.Height = 82;
                vertexEditor.Visible = true;
                _currentControl.Visible = false;
            }

            CheckDimensions();
        }
        
        /// <summary>
        /// Call this after the frame is set.
        /// </summary>
        private void HandleFirstPersonCamera()
        {
            if (firstPersonCameraToolStripMenuItem.Checked && _scn0 != null && scn0Editor._camera != null)
            {
                SCN0CameraNode c = scn0Editor._camera;
                CameraAnimationFrame f = c.GetAnimFrame(CurrentFrame - 1);
                Vector3 r = f.GetRotate(c.Type);
                Vector3 t = f.Pos;
                
                ModelPanelViewport v = ModelPanel.CurrentViewport;

                ViewportProjection proj = (ViewportProjection)(int)c.ProjectionType;
                if (v.ViewType != proj)
                    v.SetProjectionType(proj);

                GLCamera cam = v.Camera;
                cam.Reset();
                cam.Translate(t._x, t._y, t._z);
                cam.Rotate(r._x, r._y, r._z);

                bool retainAspect = true;
                float aspect = retainAspect ? cam.Aspect : f.Aspect;
                cam.SetProjectionParams(aspect, f.FovY, f.FarZ, f.NearZ);
            }
        }

        public override void OnAnimationChanged()
        {
            NW4RAnimationNode node = TargetAnimation;

            selectedAnimationToolStripMenuItem.Enabled = node != null;

            portToolStripMenuItem.Enabled = node is CHR0Node;
            mergeToolStripMenuItem.Enabled = node != null && Array.IndexOf(Mergeable, node.GetType()) >= 0;
            resizeToolStripMenuItem.Enabled = node != null && Array.IndexOf(Resizable, node.GetType()) >= 0;
            appendToolStripMenuItem.Enabled = node != null && Array.IndexOf(Appendable, node.GetType()) >= 0;

            int i = -1;
            bool hasKeys = node != null && !(node is SCN0Node) && (i = Array.IndexOf(Interpolated, node.GetType())) >= 0;
            string s = 
                i == 0 ? (SelectedBone != null ? SelectedBone.Name : "entry") :
                i == 1 ? (TargetTexRef != null ? TargetTexRef.Name : "entry") :
                i == 2 ? (shp0Editor.VertexSetDest != null ? shp0Editor.VertexSetDest.Name : "entry") :
            "entry";

            averageboneStartendTangentsToolStripMenuItem.Enabled = hasKeys && s != "entry";
            averageboneStartendTangentsToolStripMenuItem.Text = String.Format("Average {0} start/end keyframes", s);

            averageAllStartEndTangentsToolStripMenuItem.Enabled = node != null && Array.IndexOf(Interpolated, node.GetType()) >= 0;
            //syncStartendTangentsToolStripMenuItem.Enabled = node != null && Array.IndexOf(Interpolated, node.GetType()) >= 0;
        }
    }
}
