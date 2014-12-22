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
                ModelPanel.BackgroundImageType = GLPanel.BGImageType.Stretch;
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
                ModelPanel.BackgroundImageType = GLPanel.BGImageType.Center;
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
                ModelPanel.BackgroundImageType = GLPanel.BGImageType.ResizeWithBars;
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
            ModelPanel.TextOverlaysEnabled = enableTextOverlaysToolStripMenuItem.Checked;
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
            if (_updating)
                return;

            ControlType = rotationToolStripMenuItem.Checked ?  TransformType.Rotation : TransformType.None;
        }

        private void translationToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            ControlType = rotationToolStripMenuItem.Checked ? TransformType.Translation : TransformType.None;
        }

        private void scaleToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            ControlType = rotationToolStripMenuItem.Checked ? TransformType.Scale : TransformType.None;
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
                GetFiles(NW4RAnimType.None);
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

        private void EnableLiveTextureFolder_CheckedChanged(object sender, EventArgs e)
        {
            MDL0TextureNode._folderWatcher.EnableRaisingEvents = EnableLiveTextureFolder.Checked;
            ModelPanel.RefreshReferences();
        }

        public void SelectedPolygonChanged()
        {
            //We can't return here if the selected polygon is set to null.
            //If the target model is changed or the selected object is cleared,
            //things relying on the selected object must be updated to reflect that.
            //if (leftPanel.SelectedPolygon == null) return;

            //This sets the selected object index internally in the model.
            //This determines the target object for focus editing vertices, normals, etc in the viewer
            //If the selected object is set to null, the poly index will be set to -1 by IndexOf.
            //This means vertices, normals etc will be drawn for all objects, if enabled.
            _targetModel.SelectedObjectIndex = _targetModel.Objects.IndexOf(leftPanel.SelectedObject);

            //If this setting is enabled, we need to show the user what textures only this object uses.
            //If the polygon is set to null, all of the model's texture references will be shown.
            if (syncTexObjToolStripMenuItem.Checked)
                leftPanel.UpdateTextures();

            //Update the VIS editor to show the entries for the selected object
            if (TargetAnimType == NW4RAnimType.VIS && 
                leftPanel.SelectedObject != null && 
                vis0Editor.listBox1.Items.Count != 0 && 
                leftPanel.SelectedObject is MDL0ObjectNode)
            {
                int x = 0;
                foreach (object i in vis0Editor.listBox1.Items)
                    if (i.ToString() == ((MDL0ObjectNode)leftPanel.SelectedObject).VisibilityBone)
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

        /*private void chkShaders_CheckedChanged(object sender, EventArgs e)
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
        }*/

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
    }
}
