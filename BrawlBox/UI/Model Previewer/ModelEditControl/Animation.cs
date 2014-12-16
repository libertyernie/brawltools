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
                animCtrlPnl.Height = prevHeight;
                animCtrlPnl.Width = prevWidth;
                weightEditor.Visible = false;
                _currentControl.Visible = true;
            }
            else
            {
                if (vertexEditor.Visible)
                    ToggleVertexEditor();

                prevHeight = animCtrlPnl.Height;
                prevWidth = animCtrlPnl.Width;
                animCtrlPnl.Width = 320;
                animCtrlPnl.Height = 78;
                weightEditor.Visible = true;
                _currentControl.Visible = false;
            }

            CheckDimensions();
        }
        public void ToggleVertexEditor()
        {
            if (vertexEditor.Visible)
            {
                animCtrlPnl.Height = prevHeight;
                animCtrlPnl.Width = prevWidth;
                vertexEditor.Visible = false;
                _currentControl.Visible = true;
            }
            else
            {
                //if (weightEditor.Visible)
                //    ToggleWeightEditor();

                prevHeight = animCtrlPnl.Height;
                prevWidth = animCtrlPnl.Width;
                animCtrlPnl.Width = 118;
                animCtrlPnl.Height = 85;
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

                ModelPanel.Camera.Reset();
                ModelPanel.Camera.Translate(t._x, t._y, t._z);
                ModelPanel.Camera.Rotate(r._x, r._y, r._z);
                ModelPanel._aspect = f.Aspect;
                ModelPanel._farZ = f.FarZ;
                ModelPanel._nearZ = f.NearZ;
                ModelPanel._fovY = f.FovY;
                ModelPanel.UpdateProjection();
            }
        }

        public override void OnAnimationChanged()
        {
            AnimationNode node = TargetAnimation;

            selectedAnimationToolStripMenuItem.Enabled = node != null;

            portToolStripMenuItem.Enabled = node is CHR0Node;
            mergeToolStripMenuItem.Enabled = node != null && Array.IndexOf(Mergeable, node.GetType()) >= 0;
            resizeToolStripMenuItem.Enabled = node != null && Array.IndexOf(Resizable, node.GetType()) >= 0;
            appendToolStripMenuItem.Enabled = node != null && Array.IndexOf(Appendable, node.GetType()) >= 0;

            int i = -1;
            bool enabled = node != null && (i = Array.IndexOf(Interpolated, node.GetType())) >= 0 && (i == 0 ? SelectedBone != null : i == 1 ? TargetTexRef != null : shp0Editor.VertexSetDest != null);
            if (averageboneStartendTangentsToolStripMenuItem.Enabled = enabled)
                averageboneStartendTangentsToolStripMenuItem.Text = String.Format("Average {0} start/end keyframes", i == 0 ? SelectedBone.Name : i == 1 ? TargetTexRef.Name : shp0Editor.VertexSetDest.Name);
            else
                averageboneStartendTangentsToolStripMenuItem.Text = "Average entry start/end keyframes";

            averageAllStartEndTangentsToolStripMenuItem.Enabled = node != null && Array.IndexOf(Interpolated, node.GetType()) >= 0;
            syncStartendTangentsToolStripMenuItem.Enabled = node != null && Array.IndexOf(Interpolated, node.GetType()) >= 0;
        }
    }
}
