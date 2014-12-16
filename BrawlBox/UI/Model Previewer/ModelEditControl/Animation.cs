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
        //Updates specified angle by applying an offset.
        //Allows pnlAnim to handle the changes so keyframes are updated.
        private unsafe void ApplyAngle(int index, float offset)
        {
            NumericInputBox box = chr0Editor._transBoxes[index + 3];
            float newVal = (float)Math.Round(box.Value + offset, 3);
            if (box.Value != newVal)
            {
                box.Value = newVal;
                chr0Editor.BoxChanged(box, null);
            }
        }
        private unsafe void ApplyTranslation(int index, float offset)
        {
            NumericInputBox box = chr0Editor._transBoxes[index + 6];
            float newVal = (float)Math.Round(box.Value + offset, 3);
            if (box.Value != newVal)
            {
                box.Value = newVal;
                chr0Editor.BoxChanged(box, null);
            }
        }
        private unsafe void ApplyScale(int index, float scale)
        {
            NumericInputBox box = chr0Editor._transBoxes[index];
            float newVal = (float)Math.Round(box.Value * scale, 3);
            if (box.Value != newVal && newVal != 0.0f)
            {
                box.Value = newVal;
                chr0Editor.BoxChanged(box, null);
            }
        }

        public AnimType TargetAnimType
        {
            get { return (AnimType)leftPanel.fileType.SelectedIndex; }
            set { leftPanel.fileType.SelectedIndex = (int)value; }
        }

        private Control _currentControl = null;
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
        public void SetCurrentControl()
        {
            Control newControl = null;
            syncTexObjToolStripMenuItem.Checked = (TargetAnimType == AnimType.SRT || TargetAnimType == AnimType.PAT);
            switch (TargetAnimType)
            {
                case AnimType.CHR: newControl = chr0Editor; break;
                case AnimType.SHP: newControl = shp0Editor; break;
                case AnimType.VIS: newControl = vis0Editor; break;
                case AnimType.SCN: newControl = scn0Editor; break;
                case AnimType.CLR: newControl = clr0Editor; break;
                case AnimType.SRT: newControl = srt0Editor; break;
                case AnimType.PAT: newControl = pat0Editor; break;
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
                        animCtrlPnl.Width = 582;
                    }
                    else if (_currentControl is SRT0Editor)
                    {
                        animEditors.Height = 78;
                        animCtrlPnl.Width = 483;
                    }
                    else if (_currentControl is SHP0Editor)
                    {
                        animEditors.Height = 106;
                        animCtrlPnl.Width = 533;
                    }
                    else if (_currentControl is PAT0Editor)
                    {
                        animEditors.Height = 78;
                        animCtrlPnl.Width = 402;
                    }
                    else if (_currentControl is VIS0Editor)
                    {
                        animEditors.Height = 62;
                        animCtrlPnl.Width = 210;
                    }
                    else if (_currentControl is CLR0Editor)
                    {
                        animEditors.Height = 62;
                        animCtrlPnl.Width = 168;
                    }
                    else if (_currentControl is SCN0Editor)
                        scn0Editor.GetDimensions();
                    else
                        animEditors.Height = animCtrlPnl.Width = 0;
                }
                else animEditors.Height = animCtrlPnl.Width = 0;
                return;
            }
            CheckDimensions();
            UpdatePropDisplay();
        }
        public void UpdatePropDisplay()
        {
            chr0Editor.UpdatePropDisplay();

            //if (animEditors.Height == 0 || animEditors.Visible == false)
            //    return;

            switch (TargetAnimType)
            {
                //case AnimType.CHR: chr0Editor.UpdatePropDisplay(); break;
                case AnimType.SRT: srt0Editor.UpdatePropDisplay(); break;
                case AnimType.VIS: 
                    if (rightPanel.pnlKeyframes.visEditor.TargetNode != null && !((VIS0EntryNode)rightPanel.pnlKeyframes.visEditor.TargetNode).Constant)
                    {
                        rightPanel.pnlKeyframes.visEditor._updating = true;
                        rightPanel.pnlKeyframes.visEditor.listBox1.SelectedIndices.Clear();
                        rightPanel.pnlKeyframes.visEditor.listBox1.SelectedIndex = CurrentFrame - 1;
                        rightPanel.pnlKeyframes.visEditor._updating = false;
                    } 
                    break;
                case AnimType.SHP: shp0Editor.UpdatePropDisplay(); break;
                case AnimType.PAT: pat0Editor.UpdatePropDisplay(); break;
                case AnimType.SCN: scn0Editor.UpdatePropDisplay(); break;
                case AnimType.CLR: clr0Editor.UpdatePropDisplay(); break;
            }
            UpdateToolButtons();
        }

        public bool EditingAll { get { return (!(models.SelectedItem is MDL0Node) && models.SelectedItem != null && models.SelectedItem.ToString() == "All"); } }
        public void UpdateModel()
        {
            if (_updating || models == null)
                return;

            if (EditingAll)
                foreach (IModel n in _targetModels)
                    UpdateModel(n);
            else if (TargetModel != null)
                UpdateModel(TargetModel);

            if (!_playing)
                UpdatePropDisplay();

            ModelPanel.Invalidate();
        }
        private void UpdateModel(IModel model)
        {
            if (_chr0 != null && playCHR0ToolStripMenuItem.Checked)
                model.ApplyCHR(_chr0, _animFrame);
            else
                model.ApplyCHR(null, 0);
            if (_srt0 != null && playSRT0ToolStripMenuItem.Checked)
                model.ApplySRT(_srt0, _animFrame);
            else
                model.ApplySRT(null, 0);
            if (_shp0 != null && playSHP0ToolStripMenuItem.Checked)
                model.ApplySHP(_shp0, _animFrame);
            else
                model.ApplySHP(null, 0);
            if (_pat0 != null && playPAT0ToolStripMenuItem.Checked)
                model.ApplyPAT(_pat0, _animFrame);
            else
                model.ApplyPAT(null, 0);
            if (_vis0 != null && playVIS0ToolStripMenuItem.Checked)
                if (model == TargetModel)
                    ReadVIS0();
                else
                    model.ApplyVIS(_vis0, _animFrame);
            if (_scn0 != null)// && playSCN0ToolStripMenuItem.Checked)
                model.SetSCN0Frame(_animFrame);
            else
                model.SetSCN0Frame(0);
            if (_clr0 != null && playCLR0ToolStripMenuItem.Checked)
                model.ApplyCLR(_clr0, _animFrame);
            else
                model.ApplyCLR(null, 0);
        }
        public void UpdateKeyframePanel() { UpdateKeyframePanel(TargetAnimType); }
        public void UpdateKeyframePanel(AnimType type)
        {
            rightPanel.pnlKeyframes.TargetSequence = null;
            btnRightToggle.Enabled = true;
            switch (type)
            {
                case AnimType.CHR:
                    if (_chr0 != null && SelectedBone != null)
                        rightPanel.pnlKeyframes.TargetSequence = _chr0.FindChild(SelectedBone.Name, false);
                    break;
                case AnimType.SRT:
                    if (_srt0 != null && TargetTexRef != null)
                        rightPanel.pnlKeyframes.TargetSequence = srt0Editor.TexEntry;
                    break;
                case AnimType.SHP:
                    if (_shp0 != null)
                        rightPanel.pnlKeyframes.TargetSequence = shp0Editor.VertexSetDest;
                    break;
            }
        }
        public void UpdateEditor() { UpdateEditor(TargetAnimType); }
        public void UpdateEditor(AnimType type)
        {
            if (type != AnimType.SRT) leftPanel.UpdateSRT0Selection(null);
            if (type != AnimType.PAT) leftPanel.UpdatePAT0Selection(null);
            if (type != AnimType.SCN)
                foreach (IModel m in _targetModels)
                    m.SetSCN0(null);

            switch (type)
            {
                case AnimType.CHR:
                    break;
                case AnimType.SRT:
                    leftPanel.UpdateSRT0Selection(SelectedSRT0);
                    break;
                case AnimType.SHP:
                    shp0Editor.UpdateSHP0Indices();
                    break;
                case AnimType.PAT:
                    pat0Editor.UpdateBoxes();
                    leftPanel.UpdatePAT0Selection(SelectedPAT0);
                    break;
                case AnimType.VIS:
                    vis0Editor.UpdateAnimation();
                    break;
                case AnimType.SCN:
                    //foreach (MDL0Node m in _targetModels)
                    //    m.SetSCN0(_scn0);
                    scn0Editor.tabControl1_Selected(null, new TabControlEventArgs(null, scn0Editor.tabIndex, TabControlAction.Selected));
                    break;
                case AnimType.CLR:
                    clr0Editor.UpdateAnimation();
                    break;
            }
        }

        public void AnimChanged(AnimType type)
        {
            if (type == TargetAnimType)
            {
                UpdateEditor();
                UpdateKeyframePanel();
            }

            AnimationNode node = GetAnimation(type);
            if (node == null)
            {
                pnlPlayback.numFrameIndex.Maximum = _maxFrame = 0;
                pnlPlayback.numTotalFrames.Minimum = 0;

                _updating = true;
                pnlPlayback.numTotalFrames.Value = 0;
                selectedAnimationToolStripMenuItem.Enabled = false;
                _updating = false;

                pnlPlayback.btnPlay.Enabled =
                pnlPlayback.numTotalFrames.Enabled =
                pnlPlayback.numFrameIndex.Enabled = false;
                pnlPlayback.btnLast.Enabled = false;
                pnlPlayback.btnFirst.Enabled = false;
                pnlPlayback.Enabled = false;
                EnableTransformEdit = true;
                SetFrame(0);
            }
            else
            {
                _maxFrame = node.FrameCount;

                _updating = true;
                pnlPlayback.btnPlay.Enabled =
                pnlPlayback.numFrameIndex.Enabled =
                pnlPlayback.numTotalFrames.Enabled = true;
                pnlPlayback.Enabled = true;
                pnlPlayback.numTotalFrames.Value = _maxFrame;
                selectedAnimationToolStripMenuItem.Enabled = true;
                _updating = false;

                pnlPlayback.numFrameIndex.Maximum = _maxFrame;
                SetFrame(1);

                EnableTransformEdit = !_playing;

                UpdateToolButtons();
            }
        }

        public void UpdateToolButtons()
        {
            AnimationNode node = TargetAnimation;

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

        private static readonly Type[] Mergeable = new Type[] { typeof(CHR0Node) };
        private static readonly Type[] Appendable = new Type[] { typeof(CHR0Node), typeof(SRT0Node), typeof(SHP0Node), typeof(VIS0Node), typeof(PAT0Node) };
        private static readonly Type[] Resizable = new Type[] { typeof(CHR0Node), typeof(SRT0Node), typeof(SHP0Node), typeof(VIS0Node), typeof(PAT0Node) };
        private static readonly Type[] Interpolated = new Type[] { typeof(CHR0Node), typeof(SRT0Node), typeof(SHP0Node) };

        /// <summary>
        /// Call this after the frame is set
        /// </summary>
        private void HandleFirstPersonCamera()
        {
            if (stPersonToolStripMenuItem.Checked && _scn0 != null && scn0Editor._camera != null)
            {
                SCN0CameraNode c = scn0Editor._camera;
                CameraAnimationFrame f = c.GetAnimFrame(CurrentFrame - 1);
                Vector3 r = f.GetRotate(CurrentFrame, c.Type);
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

        public bool _playing = false;
        public void SetFrame(int index)
        {
            if (index > _maxFrame || index < 0)
                return;
            
            index = TargetModel == null ? 0 : index;

            CurrentFrame = index;

            HandleFirstPersonCamera();

            pnlPlayback.btnNextFrame.Enabled = _animFrame < _maxFrame;
            pnlPlayback.btnPrevFrame.Enabled = _animFrame > 0;

            pnlPlayback.btnLast.Enabled = _animFrame != _maxFrame;
            pnlPlayback.btnFirst.Enabled = _animFrame > 1;

            if (_animFrame <= pnlPlayback.numFrameIndex.Maximum)
                pnlPlayback.numFrameIndex.Value = _animFrame;

            if (!_playing)
            {
                if (InterpolationEditor != null && InterpolationEditor.Visible)
                    InterpolationEditor.Frame = CurrentFrame;
                KeyframePanel.numFrame_ValueChanged();
            }
        }

        private bool _bonesWereOff = false;

        CoolTimer _timer;
        void _timer_RenderFrame(object sender, FrameEventArgs e)
        {
            if (TargetAnimation == null)
                return;

            if (_animFrame >= _maxFrame)
                if (!_loop)
                    StopAnim();
                else
                    SetFrame(1);
            else
                SetFrame(_animFrame + 1);

            if (_capture)
                images.Add(ModelPanel.GrabScreenshot(false));
        }

        public void PlayAnim()
        {
            if (TargetAnimation == null || _maxFrame == 1)
                return;

            _playing = true;

            if (disableBonesWhenPlayingToolStripMenuItem.Checked)
            {
                if (RenderBones == false)
                    _bonesWereOff = true;
                RenderBones = false;
            }

            EnableTransformEdit = false;

            if (_animFrame >= _maxFrame) //Reset anim
                SetFrame(1);

            if (_animFrame < _maxFrame)
            {
                pnlPlayback.btnPlay.Text = "Stop";
                _timer.Run(0, (double)pnlPlayback.numFPS.Value);
            }
            else
            {
                if (disableBonesWhenPlayingToolStripMenuItem.Checked)
                    RenderBones = true;
                _playing = false;
            }
        }
        public void StopAnim()
        {
            if (!_playing && !_timer.IsRunning)
                return;

            _timer.Stop();

            _playing = false;

            if (disableBonesWhenPlayingToolStripMenuItem.Checked)
            {
                if (!_bonesWereOff)
                    RenderBones = true;

                _bonesWereOff = false;
            }

            pnlPlayback.btnPlay.Text = "Play";
            EnableTransformEdit = true;

            if (_capture)
            {
                RenderToGIF(images.ToArray());
                images.Clear();
                _capture = false;
            }
        }
    }
}
