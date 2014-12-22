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
using Ikarus.ModelViewer;

namespace Ikarus.UI
{
    public partial class MainControl : ModelEditorBase
    {
        //Updates specified angle by applying an offset.
        //Allows pnlAnim to handle the changes so keyframes are updated.
        private unsafe void ApplyAngle(int index, float offset)
        {
            NumericInputBox box = chr0Editor._transBoxes[index + 3];
            box.Value = (float)Math.Round(box.Value + offset, 3);
            chr0Editor.BoxChanged(box, null);
        }
        //Updates translation with offset.
        private unsafe void ApplyTranslation(int index, float offset)
        {
            NumericInputBox box = chr0Editor._transBoxes[index + 6];
            box.Value = (float)Math.Round(box.Value + offset, 3);
            chr0Editor.BoxChanged(box, null);
        }
        //Updates scale with offset.
        private unsafe void ApplyScale(int index, float offset)
        {
            NumericInputBox box = chr0Editor._transBoxes[index];
            float value = (float)Math.Round(box.Value * offset, 3);
            if (value == 0) return;
            box.Value = value;
            chr0Editor.BoxChanged(box, null);
        }
        private unsafe void ApplyScale2(int index, float offset)
        {
            NumericInputBox box = chr0Editor._transBoxes[index];

            if (box._value == 0)
                return;

            float scale = (box.Value + offset) / box.Value;

            float value = (float)Math.Round(box.Value * scale, 3);
            if (value == 0) return;
            box.Value = value;
            chr0Editor.BoxChanged(box, null);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NW4RAnimType TargetAnimType
        {
            get { return (NW4RAnimType)fileType.SelectedIndex; }
            set { fileType.SelectedIndex = (int)value; }
        }

        private Control _currentControl = null;
        public int prevHeight = 0, prevWidth = 0;
        public void SetCurrentControl()
        {
            Control newControl = null;
            switch (TargetAnimType)
            {
                case NW4RAnimType.CHR:
                    newControl = chr0Editor;
                    break;
                case NW4RAnimType.SRT:
                    newControl = srt0Editor;
                    syncTexObjToolStripMenuItem.Checked = true;
                    break;
                case NW4RAnimType.SHP:
                    newControl = shp0Editor;
                    break;
                case NW4RAnimType.PAT:
                    newControl = pat0Editor;
                    syncTexObjToolStripMenuItem.Checked = true;
                    break;
                case NW4RAnimType.VIS:
                    newControl = vis0Editor;
                    break;
                case NW4RAnimType.CLR:
                    newControl = clr0Editor;
                    break;
            }
            if (_currentControl != newControl)
            {
                if (_currentControl != null)
                    _currentControl.Visible = false;
                _currentControl = newControl;
                if (!(_currentControl is SRT0Editor) && !(_currentControl is PAT0Editor))
                    syncTexObjToolStripMenuItem.Checked = false;
                if (_currentControl != null)
                {
                    _currentControl.Visible = true;
                    if (_currentControl is CHR0Editor)
                    {
                        animEditors.Height = 78;
                        panel3.Width = 582;
                        //rightPanel.pnlKeyframes.SetEditType(0);
                    }
                    else if (_currentControl is SRT0Editor)
                    {
                        animEditors.Height = 78;
                        panel3.Width = 483;
                        //rightPanel.pnlKeyframes.SetEditType(0);
                    }
                    else if (_currentControl is SHP0Editor)
                    {
                        animEditors.Height = 106;
                        panel3.Width = 533;
                        //rightPanel.pnlKeyframes.SetEditType(0);
                    }
                    else if (_currentControl is PAT0Editor)
                    {
                        animEditors.Height = 78;
                        panel3.Width = 402;
                    }
                    else if (_currentControl is VIS0Editor)
                    {
                        animEditors.Height = 62;
                        panel3.Width = 210;
                        //rightPanel.pnlKeyframes.SetEditType(1);
                    }
                    else if (_currentControl is CLR0Editor)
                    {
                        animEditors.Height = 62;
                        panel3.Width = 168;
                        //rightPanel.pnlKeyframes.SetEditType(2);
                    }
                    else
                        animEditors.Height = panel3.Width = 0;
                }
                else animEditors.Height = panel3.Width = 0;
                return;
            }
            CheckDimensions();
            UpdatePropDisplay();
        }
        public void UpdatePropDisplay()
        {
            if (animEditors.Height == 0 || animEditors.Visible == false)
                return;

            switch (TargetAnimType)
            {
                case NW4RAnimType.CHR: chr0Editor.UpdatePropDisplay(); break;
                case NW4RAnimType.SRT: srt0Editor.UpdatePropDisplay(); break;
                case NW4RAnimType.SHP: shp0Editor.UpdatePropDisplay(); break;
                //case AnimType.VIS: vis0Editor.UpdatePropDisplay(); break;
                case NW4RAnimType.PAT: pat0Editor.UpdatePropDisplay(); break;
                //case AnimType.SCN: scn0Editor.UpdatePropDisplay(); break;
                case NW4RAnimType.CLR: clr0Editor.UpdatePropDisplay(); break;
            }

            if (TargetAnimType == NW4RAnimType.VIS)
            {
                //if (scriptPanel.pnlKeyframes.visEditor.TargetNode != null && !((VIS0EntryNode)scriptPanel.pnlKeyframes.visEditor.TargetNode).Constant)
                //{
                //    scriptPanel.pnlKeyframes.visEditor._updating = true;
                //    scriptPanel.pnlKeyframes.visEditor.listBox1.SelectedIndices.Clear();
                //    scriptPanel.pnlKeyframes.visEditor.listBox1.SelectedIndex = CurrentFrame - 1;
                //    scriptPanel.pnlKeyframes.visEditor._updating = false;
                //}
            }
        }

        public bool _editingAll { get { return (!(comboCharacters.SelectedItem is MDL0Node) && comboCharacters.SelectedItem != null && comboCharacters.SelectedItem.ToString() == "All"); } }
        public void UpdateModel()
        {
            if (_updating)
                return;

            if (!_editingAll)
            {
                if (TargetModel != null)
                    UpdateModel(TargetModel);
            }
            else
                foreach (MDL0Node n in _targetModels)
                    UpdateModel(n);

            if (RunTime._articles != null)
                foreach (ArticleInfo a in RunTime._articles)
                    if (a != null && a.Running)
                        a.UpdateModel();

            if (!_playing) 
                UpdatePropDisplay();

            modelPanel.Invalidate();
        }
        private void UpdateModel(MDL0Node model)
        {
            int frame = CurrentFrame;
            if (_chr0 != null && !(TargetAnimType != NW4RAnimType.CHR && !playCHR0ToolStripMenuItem.Checked))
                model.ApplyCHR(_chr0, frame);
            else
                model.ApplyCHR(null, 0);
            if (_srt0 != null && !(TargetAnimType != NW4RAnimType.SRT && !playSRT0ToolStripMenuItem.Checked))
                model.ApplySRT(_srt0, frame);
            else
                model.ApplySRT(null, 0);
            if (_shp0 != null && !(TargetAnimType != NW4RAnimType.SHP && !playSHP0ToolStripMenuItem.Checked))
                model.ApplySHP(_shp0, frame);
            else
                model.ApplySHP(null, 0);
            if (_pat0 != null && !(TargetAnimType != NW4RAnimType.PAT && !playPAT0ToolStripMenuItem.Checked))
                model.ApplyPAT(_pat0, frame);
            else
                model.ApplyPAT(null, 0);
            if (_vis0 != null && !(TargetAnimType != NW4RAnimType.VIS && !playVIS0ToolStripMenuItem.Checked))
                if (model == TargetModel)
                    ReadVIS0();
                else
                    model.ApplyVIS(_vis0, frame);
            if (_clr0 != null && !(TargetAnimType != NW4RAnimType.CLR && !playCLR0ToolStripMenuItem.Checked))
                model.ApplyCLR(_clr0, frame);
            else
                model.ApplyCLR(null, 0);
        }
        
        public void AnimChanged(NW4RAnimType type)
        {
            //Update animation editors
            if (type != NW4RAnimType.SRT) modelListsPanel1.UpdateSRT0Selection(null);
            if (type != NW4RAnimType.PAT) modelListsPanel1.UpdatePAT0Selection(null);

            switch (type)
            {
                case NW4RAnimType.CHR:
                    break;
                case NW4RAnimType.SRT:
                    modelListsPanel1.UpdateSRT0Selection(SelectedSRT0);
                    break;
                case NW4RAnimType.SHP:
                    shp0Editor.UpdateSHP0Indices();
                    break;
                case NW4RAnimType.PAT:
                    pat0Editor.UpdateBoxes();
                    modelListsPanel1.UpdatePAT0Selection(SelectedPAT0);
                    break;
                case NW4RAnimType.VIS: 
                    vis0Editor.UpdateAnimation();
                    break;
                case NW4RAnimType.CLR: 
                    clr0Editor.UpdateAnimation();
                    break;
            }

            AnimationNode anim = GetAnimation(type);
            if (anim == null)
            {
                pnlPlayback.numFrameIndex.Maximum = MaxFrame = 0;
                pnlPlayback.numTotalFrames.Minimum = 0;
                _updating = true;
                pnlPlayback.numTotalFrames.Value = 0;
                _updating = false;
                pnlPlayback.btnPlay.Enabled =
                pnlPlayback.numTotalFrames.Enabled =
                pnlPlayback.numFrameIndex.Enabled = false;
                pnlPlayback.btnLast.Enabled = false;
                pnlPlayback.btnFirst.Enabled = false;
                pnlPlayback.Enabled = false;
                RunTime.SetFrame(-1);
            }
            else
            {
                int oldMax = MaxFrame;

                MaxFrame = anim.FrameCount;
                if (Array.IndexOf(Interpolated, anim.GetType()) >= 0)
                    MaxFrame += (anim.Loop ? 1 : 0);

                _updating = true;
                pnlPlayback.btnPlay.Enabled =
                pnlPlayback.numFrameIndex.Enabled =
                pnlPlayback.numTotalFrames.Enabled = true;
                pnlPlayback.Enabled = true;
                pnlPlayback.numTotalFrames.Value = MaxFrame;
                if (syncLoopToAnimationToolStripMenuItem.Checked)
                    pnlPlayback.chkLoop.Checked = GetAnimation(type).Loop;
                _updating = false;

                if (MaxFrame < oldMax)
                {
                    RunTime.SetFrame(0);
                    pnlPlayback.numFrameIndex.Maximum = MaxFrame;
                }
                else
                {
                    pnlPlayback.numFrameIndex.Maximum = MaxFrame;
                    RunTime.SetFrame(0);
                }
            }
        }

        public void numFrameIndex_ValueChanged(object sender, EventArgs e)
        {
            int val = (int)pnlPlayback.numFrameIndex.Value;
            if (val != CurrentFrame)
                SetFrame(val);
        }

        internal void UpdatePlaybackPanel()
        {
            pnlPlayback.btnNextFrame.Enabled = CurrentFrame < MaxFrame;
            pnlPlayback.btnPrevFrame.Enabled = CurrentFrame > 0;
            pnlPlayback.btnLast.Enabled = CurrentFrame != MaxFrame;
            pnlPlayback.btnFirst.Enabled = CurrentFrame > 1;
            if (CurrentFrame <= pnlPlayback.numFrameIndex.Maximum)
                pnlPlayback.numFrameIndex.Value = CurrentFrame;
        }

        public bool _playing = false;
        public void SetFrame(int index)
        {
            RunTime.SetFrame(index - 1);
        }

        internal void ApplyFrame()
        {
            UpdateModel();
            UpdatePropDisplay();
            UpdatePlaybackPanel();
        }
    }
}
