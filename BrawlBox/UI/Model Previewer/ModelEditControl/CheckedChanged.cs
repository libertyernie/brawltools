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
        #region Projection
        private void orthographicToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            _updating = true;
            if (orthographicToolStripMenuItem.Checked)
            {
                ModelPanel.SetProjectionType(true);
                perspectiveToolStripMenuItem.Checked = false;
            }
            else
            {
                ModelPanel.SetProjectionType(false);
                perspectiveToolStripMenuItem.Checked = true;
            }
            _updating = false;
        }
        private void perspectiveToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            _updating = true;
            if (perspectiveToolStripMenuItem.Checked)
            {
                ModelPanel.SetProjectionType(false);
                orthographicToolStripMenuItem.Checked = false;
            }
            else
            {
                ModelPanel.SetProjectionType(true);
                orthographicToolStripMenuItem.Checked = true;
            }
            _updating = false;
        }
        #endregion

        #region BG Image Display

        private void stretchToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating) return;
            if (stretchToolStripMenuItem1.Checked)
            {
                _updating = true;
                centerToolStripMenuItem1.Checked = resizeToolStripMenuItem1.Checked = false;
                ModelPanel._bgType = GLPanel.BackgroundType.Stretch;
                _updating = false;
                ModelPanel.Invalidate();
            }
        }

        private void centerToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating) return;
            if (centerToolStripMenuItem1.Checked)
            {
                _updating = true;
                stretchToolStripMenuItem1.Checked = resizeToolStripMenuItem1.Checked = false;
                ModelPanel._bgType = GLPanel.BackgroundType.Center;
                _updating = false;
                ModelPanel.Invalidate();
            }
        }

        private void resizeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating) return;
            if (resizeToolStripMenuItem1.Checked)
            {
                _updating = true;
                centerToolStripMenuItem1.Checked = stretchToolStripMenuItem1.Checked = false;
                ModelPanel._bgType = GLPanel.BackgroundType.ResizeWithBars;
                _updating = false;
                ModelPanel.Invalidate();
            }
        }

        #endregion

        #region Settings
        private void showCameraCoordinatesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ModelPanel._showCamCoords = showCameraCoordinatesToolStripMenuItem.Checked;
        }
        private void enableTextOverlaysToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ModelPanel._textEnabled = enableTextOverlaysToolStripMenuItem.Checked;
        }
        private void enablePointAndLineSmoothingToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ModelPanel._enableSmoothing = enablePointAndLineSmoothingToolStripMenuItem.Checked;
        }
        private void stPersonToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ModelPanel.Invalidate();
        }
        #endregion

        #region Bone Control

        private void rotationToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating) return;
            if (rotationToolStripMenuItem.Checked)
            {
                _updating = true;
                scaleToolStripMenuItem.Checked = translationToolStripMenuItem.Checked = false;
                _editType = TransformType.Rotation;
                _snapCirc = _snapX = _snapY = _snapZ = false;
                _updating = false;
                ModelPanel.Invalidate();
            }
            else if (translationToolStripMenuItem.Checked == rotationToolStripMenuItem.Checked == scaleToolStripMenuItem.Checked)
                _editType = TransformType.None;
        }

        private void translationToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating) return;
            if (translationToolStripMenuItem.Checked)
            {
                _updating = true;
                rotationToolStripMenuItem.Checked = scaleToolStripMenuItem.Checked = false;
                _editType = TransformType.Translation;
                _snapCirc = _snapX = _snapY = _snapZ = false;
                _updating = false;
                ModelPanel.Invalidate();
            }
            else if (translationToolStripMenuItem.Checked == rotationToolStripMenuItem.Checked == scaleToolStripMenuItem.Checked)
                _editType = TransformType.None;
        }

        private void scaleToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating) return;
            if (scaleToolStripMenuItem.Checked)
            {
                _updating = true;
                rotationToolStripMenuItem.Checked = translationToolStripMenuItem.Checked = false;
                _editType = TransformType.Scale;
                _snapCirc = _snapX = _snapY = _snapZ = false;
                _updating = false;
                ModelPanel.Invalidate();
            }
            else if (translationToolStripMenuItem.Checked == rotationToolStripMenuItem.Checked == scaleToolStripMenuItem.Checked)
                _editType = TransformType.None;
        }

        #endregion

        #region List Synchronization
        private void syncObjectsListToVIS0ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            leftPanel.chkSyncVis.Checked = syncObjectsListToVIS0ToolStripMenuItem.Checked;
        }
        private void syncAnimationsTogetherToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (syncAnimationsTogetherToolStripMenuItem.Checked)
                GetFiles(TargetAnimType);
            else
                GetFiles(AnimType.None);
        }
        #endregion

        #region Playback Panel
        public void numFrameIndex_ValueChanged(object sender, EventArgs e)
        {
            int val = (int)pnlPlayback.numFrameIndex.Value;
            if (val != _animFrame)
            {
                int difference = val - _animFrame;
                if (TargetAnimation != null)
                    SetFrame(_animFrame += difference);
            }
        }
        public void numFPS_ValueChanged(object sender, EventArgs e)
        {
            _timer.TargetRenderFrequency = (double)pnlPlayback.numFPS.Value;
        }
        public void chkLoop_CheckedChanged(object sender, EventArgs e) 
        {
            _loop = pnlPlayback.chkLoop.Checked;
            //if (TargetAnimation != null)
            //    TargetAnimation.Loop = _loop;
        }
        public void numTotalFrames_ValueChanged(object sender, EventArgs e)
        {
            if ((TargetAnimation == null) || (_updating))
                return;

            pnlPlayback.numFrameIndex.Maximum = TargetAnimation.FrameCount = _maxFrame = (int)pnlPlayback.numTotalFrames.Value;
        }
        #endregion

        #region Panel Toggles

        private void showLeft_CheckedChanged(object sender, EventArgs e)
        {
            leftPanel.Visible = spltLeft.Visible = showLeft.Checked;
            btnLeftToggle.Text = showLeft.Checked == false ? ">" : "<";
        }
        private void showRight_CheckedChanged(object sender, EventArgs e)
        {
            rightPanel.Visible = spltRight.Visible = showRight.Checked;
            btnRightToggle.Text = showRight.Checked == false ? "<" : ">";
        }
        private void showBottom_CheckedChanged(object sender, EventArgs e) 
        {
            animEditors.Visible = !animEditors.Visible;
            CheckDimensions();
        }
        private void showTop_CheckedChanged(object sender, EventArgs e) { controlPanel.Visible = showTop.Checked; }
        public void DetermineRight()
        {
            if (rightPanel.Visible)
                btnRightToggle.Text = ">";
            else
                btnRightToggle.Text = "<";
        }

        #endregion

        public void SelectedPolygonChanged(object sender, EventArgs e)
        {
            _targetModel._polyIndex = _targetModel._objList.IndexOf(leftPanel.SelectedPolygon);

            if (syncTexObjToolStripMenuItem.Checked)
                leftPanel.UpdateTextures();

            if (TargetAnimType == AnimType.VIS)
                if (leftPanel.TargetObject != null && vis0Editor.listBox1.Items.Count != 0)
                {
                    int x = 0;
                    foreach (object i in vis0Editor.listBox1.Items)
                        if (i.ToString() == leftPanel.TargetObject.VisibilityBone)
                        {
                            vis0Editor.listBox1.SelectedIndex = x;
                            break;
                        }
                        else
                            x++;
                    if (x == vis0Editor.listBox1.Items.Count)
                        vis0Editor.listBox1.SelectedIndex = -1;
                }

            ModelPanel.Invalidate();
        }

        private void chkShaders_CheckedChanged(object sender, EventArgs e)
        {
            if (ModelPanel._ctx != null)
            {
                if (ModelPanel._ctx._version < 2 && chkShaders.Checked)
                {
                    MessageBox.Show("You need at least OpenGL 2.0 to view shaders.", "GLSL not supported",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    chkShaders.Checked = false;
                    return;
                }
                else
                {
                    if (ModelPanel._ctx._shadersEnabled && !chkShaders.Checked) { GL.UseProgram(0); GL.ActiveTexture(TextureUnit.Texture0); }
                    ModelPanel._ctx._shadersEnabled = chkShaders.Checked;
                }
            }
            ModelPanel.Invalidate();
        }

        #region Coordinates

        private void SparentLocalToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            _updating = true;
            SworldToolStripMenuItem.Checked = !SparentLocalToolStripMenuItem.Checked;
            _updating = false;

            ModelPanel.Invalidate();
        }

        private void SworldToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            _updating = true;
            SparentLocalToolStripMenuItem.Checked = !SworldToolStripMenuItem.Checked;
            _updating = false;

            ModelPanel.Invalidate();
        }

        private void RparentLocalToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            _updating = true;
            RworldToolStripMenuItem.Checked = !RparentLocalToolStripMenuItem.Checked;
            _updating = false;

            ModelPanel.Invalidate();
        }

        private void RworldToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            _updating = true;
            RparentLocalToolStripMenuItem.Checked = !RworldToolStripMenuItem.Checked;
            _updating = false;

            ModelPanel.Invalidate();
        }

        private void TparentLocalToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            _updating = true;
            TworldToolStripMenuItem.Checked = !TparentLocalToolStripMenuItem.Checked;
            _updating = false;

            ModelPanel.Invalidate();
        }

        private void TworldToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            _updating = true;
            TparentLocalToolStripMenuItem.Checked = !TworldToolStripMenuItem.Checked;
            _updating = false;

            ModelPanel.Invalidate();
        }

        #endregion

        private void chkExternalAnims_CheckedChanged(object sender, EventArgs e)
        {
            leftPanel.UpdateAnimations();
        }

        private void chkBRRESAnims_CheckedChanged(object sender, EventArgs e)
        {
            leftPanel.UpdateAnimations();
        }

        private void chkNonBRRESAnims_CheckedChanged(object sender, EventArgs e)
        {
            leftPanel.UpdateAnimations();
        }

        private void cHR0ToolStripMenuItem1_CheckedChanged(object sender, EventArgs e)
        {
            CHR0EntryNode._generateTangents = chkGenTansCHR.Checked;
        }

        private void sRT0ToolStripMenuItem1_CheckedChanged(object sender, EventArgs e)
        {
            SRT0TextureNode._generateTangents = chkGenTansSRT.Checked;
        }

        private void sHP0ToolStripMenuItem1_CheckedChanged(object sender, EventArgs e)
        {
            SHP0VertexSetNode._generateTangents = chkGenTansSHP.Checked;
        }

        private void sCN0LightToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            SCN0LightNode._generateTangents = chkGenTansLight.Checked;
        }

        private void sCN0FogToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            SCN0FogNode._generateTangents = chkGenTansFog.Checked;
        }

        private void sCN0CameraToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            SCN0CameraNode._generateTangents = chkGenTansCamera.Checked;
        }

        private void sHP0ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            SHP0VertexSetNode._linear = chkLinearSHP.Checked;
            UpdateModel();
        }

        private void sRT0ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            SRT0TextureNode._linear = chkLinearSRT.Checked;
            UpdateModel();
        }

        private void cHR0ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            CHR0EntryNode._linear = chkGenTansCHR.Checked;
            UpdateModel();
        }

        private void sCN0LightsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            SCN0LightNode._linear = chkLinearLight.Checked;
            ModelPanel.Invalidate();
        }

        private void sCN0FogToolStripMenuItem1_CheckedChanged(object sender, EventArgs e)
        {
            SCN0FogNode._linear = chkLinearFog.Checked;
            ModelPanel.Invalidate();
        }

        private void sCN0CameraToolStripMenuItem1_CheckedChanged(object sender, EventArgs e)
        {
            SCN0CameraNode._linear = chkLinearCamera.Checked;
            ModelPanel.Invalidate();
        }
    }
}
