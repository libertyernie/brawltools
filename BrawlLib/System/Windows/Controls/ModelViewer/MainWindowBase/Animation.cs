using BrawlLib.Modeling;
using BrawlLib.OpenGL;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBBTypes;
using Gif.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace System.Windows.Forms
{
    public partial class ModelEditorBase : UserControl
    {
        #region Animation
        public AnimationNode TargetAnimation
        {
            get { return GetAnimation(TargetAnimType); }
            set { SetAnimation(TargetAnimType, value); }
        }

        public AnimationNode GetAnimation(NW4RAnimType type)
        {
            switch (type)
            {
                case NW4RAnimType.CHR: return SelectedCHR0;
                case NW4RAnimType.SRT: return SelectedSRT0;
                case NW4RAnimType.SHP: return SelectedSHP0;
                case NW4RAnimType.PAT: return SelectedPAT0;
                case NW4RAnimType.VIS: return SelectedVIS0;
                case NW4RAnimType.SCN: return SelectedSCN0;
                case NW4RAnimType.CLR: return SelectedCLR0;
                default: return null;
            }
        }
        public void SetAnimation(NW4RAnimType type, AnimationNode value)
        {
            switch (type)
            {
                case NW4RAnimType.CHR: SelectedCHR0 = value as CHR0Node; break;
                case NW4RAnimType.SRT: SelectedSRT0 = value as SRT0Node; break;
                case NW4RAnimType.SHP: SelectedSHP0 = value as SHP0Node; break;
                case NW4RAnimType.PAT: SelectedPAT0 = value as PAT0Node; break;
                case NW4RAnimType.VIS: SelectedVIS0 = value as VIS0Node; break;
                case NW4RAnimType.SCN: SelectedSCN0 = value as SCN0Node; break;
                case NW4RAnimType.CLR: SelectedCLR0 = value as CLR0Node; break;
            }
        }
        public void SetAnimation(AnimationNode value)
        {
            if (value is CHR0Node)
                SelectedCHR0 = value as CHR0Node;
            else if (value is SRT0Node)
                SelectedSRT0 = value as SRT0Node;
            else if (value is SHP0Node)
                SelectedSHP0 = value as SHP0Node;
            else if (value is PAT0Node)
                SelectedPAT0 = value as PAT0Node;
            else if (value is VIS0Node)
                SelectedVIS0 = value as VIS0Node;
            else if (value is SCN0Node)
                SelectedSCN0 = value as SCN0Node;
            else if (value is CLR0Node)
                SelectedCLR0 = value as CLR0Node;
        }
        /// <param name="index">0 = X, 1 = Y, 2 = Z</param>
        /// <param name="offset">The amount to add to the current rotation displayed in the CHR0 editor box.</param>
        protected unsafe void ApplyAngle(int index, float offset)
        {
            NumericInputBox box = chr0Editor._transBoxes[index.Clamp(0, 2) + 3];
            float newVal = (float)Math.Round(box.Value + offset, 3);
            if (box.Value != newVal)
            {
                box.Value = newVal;
                chr0Editor.BoxChanged(box, null);
            }
        }
        /// <param name="index">0 = X, 1 = Y, 2 = Z</param>
        /// <param name="offset">The amount to add to the current translation displayed in the CHR0 editor box.</param>
        protected unsafe void ApplyTranslation(int index, float offset)
        {
            NumericInputBox box = chr0Editor._transBoxes[index.Clamp(0, 2) + 6];
            float newVal = (float)Math.Round(box.Value + offset, 3);
            if (box.Value != newVal)
            {
                box.Value = newVal;
                chr0Editor.BoxChanged(box, null);
            }
        }
        /// <param name="index">0 = X, 1 = Y, 2 = Z</param>
        /// <param name="offset">The multiplier for the current scale displayed in the CHR0 editor box.</param>
        protected unsafe void ApplyScale(int index, float scale)
        {
            NumericInputBox box = chr0Editor._transBoxes[index.Clamp(0, 2)];
            float newVal = (float)Math.Round(box.Value * scale, 3);
            if (box.Value != newVal && newVal != 0.0f)
            {
                box.Value = newVal;
                chr0Editor.BoxChanged(box, null);
            }
        }

        public virtual void SetFrame(int index)
        {
            if (index > _maxFrame || index < 0)
                return;

            index = TargetModel == null ? 0 : index;

            CurrentFrame = index;

            pnlPlayback.btnNextFrame.Enabled = _animFrame < _maxFrame;
            pnlPlayback.btnPrevFrame.Enabled = _animFrame > 0;

            pnlPlayback.btnLast.Enabled = _animFrame != _maxFrame;
            pnlPlayback.btnFirst.Enabled = _animFrame > 1;

            if (_animFrame <= (float)pnlPlayback.numFrameIndex.Maximum)
                pnlPlayback.numFrameIndex.Value = (decimal)_animFrame;

            if (!_playing)
            {
                if (InterpolationEditor != null && InterpolationEditor.Visible)
                    InterpolationEditor.Frame = CurrentFrame;
                KeyframePanel.numFrame_ValueChanged();
            }
        }

        protected virtual void _timer_RenderFrame(object sender, FrameEventArgs e)
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
                images.Add(ModelPanel.GetScreenshot(false));
        }

        public virtual void PlayAnim()
        {
            if (TargetAnimation == null || _maxFrame == 1)
                return;

            _playing = true;

            if (DisableBonesWhenPlaying)
            {
                if (RenderBones == false)
                    _bonesWereOff = true;
                RenderBones = false;
            }

            EnableTransformEdit = false;

            if (_animFrame >= _maxFrame)
                SetFrame(1);

            pnlPlayback.btnPlay.Text = "Stop";
            _timer.Run(0, (double)pnlPlayback.numFPS.Value);
        }
        public virtual void StopAnim()
        {
            if (!_playing && !_timer.IsRunning)
                return;

            _timer.Stop();

            _playing = false;

            if (DisableBonesWhenPlaying)
            {
                if (!_bonesWereOff)
                    RenderBones = true;

                _bonesWereOff = false;
            }

            pnlPlayback.btnPlay.Text = "Play";
            EnableTransformEdit = true;

            if (_capture)
            {
                RenderToGIF(images, ScreenCaptureFolder);

                images.Clear();
                _capture = false;

                if (InterpolationEditor != null)
                    InterpolationEditor.Enabled = true;

                ModelPanel.Enabled = true;
                Enabled = true;
            }
        }

        public virtual void UpdatePropDisplay()
        {
            switch (TargetAnimType)
            {
                case NW4RAnimType.CHR: chr0Editor.UpdatePropDisplay(); break;
                case NW4RAnimType.SRT: srt0Editor.UpdatePropDisplay(); break;
                case NW4RAnimType.SHP: shp0Editor.UpdatePropDisplay(); break;
                case NW4RAnimType.PAT: pat0Editor.UpdatePropDisplay(); break;
                case NW4RAnimType.SCN: scn0Editor.UpdatePropDisplay(); break;
                case NW4RAnimType.CLR: clr0Editor.UpdatePropDisplay(); break;
                case NW4RAnimType.VIS:
                    if (KeyframePanel.visEditor.TargetNode != null && !((VIS0EntryNode)KeyframePanel.visEditor.TargetNode).Constant)
                    {
                        KeyframePanel.visEditor._updating = true;
                        KeyframePanel.visEditor.listBox1.SelectedIndices.Clear();
                        KeyframePanel.visEditor.listBox1.SelectedIndex = CurrentFrame - 1;
                        KeyframePanel.visEditor._updating = false;
                    }
                    break;
            }
            OnAnimationChanged();
        }

        /// <summary>
        /// Applies animations to all models and invalidates the viewport.
        /// Also updates animation controls if not playing.
        /// </summary>
        public void UpdateModel()
        {
            if (_updating)
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
        public virtual void UpdateModel(IModel model)
        {
            //TODO: support for applying more than one animation per type

            if (_chr0 != null && PlayCHR0)
                model.ApplyCHR(_chr0, _animFrame);
            else
                model.ApplyCHR(null, 0);
            if (_srt0 != null && PlaySRT0)
                model.ApplySRT(_srt0, _animFrame);
            else
                model.ApplySRT(null, 0);
            if (_shp0 != null && PlaySHP0)
                model.ApplySHP(_shp0, _animFrame);
            else
                model.ApplySHP(null, 0);
            if (_pat0 != null && PlayPAT0)
                model.ApplyPAT(_pat0, _animFrame);
            else
                model.ApplyPAT(null, 0);
            if (_vis0 != null && PlayVIS0)
                if (model == TargetModel)
                    ApplyVIS0ToInterface();
                else
                    model.ApplyVIS(_vis0, _animFrame);
            if (_scn0 != null && PlaySCN0)
                model.SetSCN0Frame(_animFrame);
            else
                model.SetSCN0Frame(0);
            if (_clr0 != null && PlayCLR0)
                model.ApplyCLR(_clr0, _animFrame);
            else
                model.ApplyCLR(null, 0);
        }

        public void UpdateKeyframePanel() { UpdateKeyframePanel(TargetAnimType); }
        public virtual void UpdateKeyframePanel(NW4RAnimType type)
        {
            KeyframePanel.TargetSequence = null;
            //btnRightToggle.Enabled = true;
            switch (type)
            {
                case NW4RAnimType.CHR:
                    if (_chr0 != null && SelectedBone != null)
                        KeyframePanel.TargetSequence = _chr0.FindChild(SelectedBone.Name, false);
                    break;
                case NW4RAnimType.SRT:
                    if (_srt0 != null && TargetTexRef != null)
                        KeyframePanel.TargetSequence = srt0Editor.TexEntry;
                    break;
                case NW4RAnimType.SHP:
                    if (_shp0 != null)
                        KeyframePanel.TargetSequence = shp0Editor.VertexSetDest;
                    break;
            }
        }

        protected virtual void UpdateSRT0FocusControls(SRT0Node node) { }
        protected virtual void UpdatePAT0FocusControls(PAT0Node node) { }
        
        public void UpdateEditor() { UpdateEditor(TargetAnimType); }
        public virtual void UpdateEditor(NW4RAnimType type)
        {
            if (type != NW4RAnimType.SRT) UpdateSRT0FocusControls(null);
            if (type != NW4RAnimType.PAT) UpdatePAT0FocusControls(null);
            if (type != NW4RAnimType.SCN)
                foreach (IModel m in _targetModels)
                    m.SetSCN0(null);

            switch (type)
            {
                case NW4RAnimType.CHR:
                    break;
                case NW4RAnimType.SRT:
                    UpdateSRT0FocusControls(SelectedSRT0);
                    break;
                case NW4RAnimType.SHP:
                    shp0Editor.UpdateSHP0Indices();
                    break;
                case NW4RAnimType.PAT:
                    pat0Editor.UpdateBoxes();
                    UpdatePAT0FocusControls(SelectedPAT0);
                    break;
                case NW4RAnimType.VIS:
                    vis0Editor.UpdateAnimation();
                    break;
                case NW4RAnimType.SCN:
                    //foreach (MDL0Node m in _targetModels)
                    //    m.SetSCN0(_scn0);
                    scn0Editor.tabControl1_Selected(null, new TabControlEventArgs(null, scn0Editor.tabIndex, TabControlAction.Selected));
                    break;
                case NW4RAnimType.CLR:
                    clr0Editor.UpdateAnimation();
                    break;
            }
        }

        public void AnimChanged(NW4RAnimType type)
        {
            if (type == TargetAnimType)
            {
                UpdateEditor();
                UpdateKeyframePanel();
            }

            AnimationNode node = GetAnimation(type);
            if (node == null)
            {
                pnlPlayback.numFrameIndex.Maximum = (decimal)(_maxFrame = 0);
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
                pnlPlayback.numTotalFrames.Value = (decimal)_maxFrame;
                _updating = false;

                pnlPlayback.numFrameIndex.Maximum = (decimal)_maxFrame;
                SetFrame(1);

                EnableTransformEdit = !_playing;
            }

            OnAnimationChanged();
        }

        public virtual void OnAnimationChanged() { }

        protected static readonly Type[] Mergeable = new Type[] { typeof(CHR0Node) };
        protected static readonly Type[] Appendable = new Type[] { typeof(CHR0Node), typeof(SRT0Node), typeof(SHP0Node), typeof(VIS0Node), typeof(PAT0Node) };
        protected static readonly Type[] Resizable = new Type[] { typeof(CHR0Node), typeof(SRT0Node), typeof(SHP0Node), typeof(VIS0Node), typeof(PAT0Node) };
        protected static readonly Type[] Interpolated = new Type[] { typeof(CHR0Node), typeof(SRT0Node), typeof(SHP0Node), typeof(SCN0Node) };

        #endregion

        #region BRRES

        public void GetFiles(NW4RAnimType focusType)
        {
            if (focusType == NW4RAnimType.None)
            {
                focusType = TargetAnimType;
                if (focusType != NW4RAnimType.CHR) _chr0 = null;
                if (focusType != NW4RAnimType.SRT) _srt0 = null;
                if (focusType != NW4RAnimType.SHP) _shp0 = null;
                if (focusType != NW4RAnimType.PAT) _pat0 = null;
                if (focusType != NW4RAnimType.VIS) _vis0 = null;
                if (focusType != NW4RAnimType.SCN) _scn0 = null;
                if (focusType != NW4RAnimType.CLR) _clr0 = null;
            }
            else
            {
                if (focusType != NW4RAnimType.CHR) GetCHR0(focusType);
                if (focusType != NW4RAnimType.SRT) GetSRT0(focusType);
                if (focusType != NW4RAnimType.SHP) GetSHP0(focusType);
                if (focusType != NW4RAnimType.PAT) GetPAT0(focusType);
                if (focusType != NW4RAnimType.VIS) GetVIS0(focusType);
                if (focusType != NW4RAnimType.SCN) GetSCN0(focusType);
                if (focusType != NW4RAnimType.CLR) GetCLR0(focusType);
            }
        }

        public virtual bool GetSCN0(NW4RAnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _scn0 = null;
                return false;
            }
            if (TargetModel != null && (_scn0 = (SCN0Node)((ResourceNode)TargetModel).RootNode.FindChildByType(focusFile.Name, true, ResourceType.SCN0)) != null)
                return true;

            foreach (ResourceNode r in _animationSearchNodes)
                if (r != null && (_scn0 = (SCN0Node)r.FindChildByType(focusFile.Name, true, ResourceType.SCN0)) != null)
                    return true;
            return false;
        }
        public virtual bool GetCLR0(NW4RAnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _clr0 = null;
                return false;
            }
            if (TargetModel != null && (_clr0 = (CLR0Node)((ResourceNode)TargetModel).RootNode.FindChildByType(focusFile.Name, true, ResourceType.CLR0)) != null)
                return true;

            foreach (ResourceNode _externalAnimationsNode in _animationSearchNodes)
                if (_externalAnimationsNode != null && (_clr0 = (CLR0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.CLR0)) != null)
                    return true;
            return false;
        }
        public virtual bool GetVIS0(NW4RAnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _vis0 = null;
                return false;
            }
            if (TargetModel != null && (_vis0 = (VIS0Node)((ResourceNode)TargetModel).RootNode.FindChildByType(focusFile.Name, true, ResourceType.VIS0)) != null)
                return true;

            foreach (ResourceNode _externalAnimationsNode in _animationSearchNodes)
                if (_externalAnimationsNode != null && (_vis0 = (VIS0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.VIS0)) != null)
                    return true;
            return false;
        }
        public virtual bool GetPAT0(NW4RAnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _pat0 = null;
                return false;
            }
            if (TargetModel != null && (_pat0 = (PAT0Node)((ResourceNode)TargetModel).RootNode.FindChildByType(focusFile.Name, true, ResourceType.PAT0)) != null)
                return true;

            foreach (ResourceNode _externalAnimationsNode in _animationSearchNodes)
                if (_externalAnimationsNode != null && (_pat0 = (PAT0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.PAT0)) != null)
                    return true;
            return false;
        }
        public virtual bool GetSRT0(NW4RAnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _srt0 = null;
                return false;
            }
            if (TargetModel != null && (_srt0 = (SRT0Node)((ResourceNode)TargetModel).RootNode.FindChildByType(focusFile.Name, true, ResourceType.SRT0)) != null)
                return true;

            foreach (ResourceNode _externalAnimationsNode in _animationSearchNodes)
                if (_externalAnimationsNode != null && (_srt0 = (SRT0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.SRT0)) != null)
                    return true;
            return false;
        }
        public virtual bool GetSHP0(NW4RAnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _shp0 = null;
                return false;
            }
            if (TargetModel != null && (_shp0 = (SHP0Node)((ResourceNode)TargetModel).RootNode.FindChildByType(focusFile.Name, true, ResourceType.SHP0)) != null)
                return true;

            foreach (ResourceNode _externalAnimationsNode in _animationSearchNodes)
                if (_externalAnimationsNode != null && (_shp0 = (SHP0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.SHP0)) != null)
                    return true;
            return false;
        }
        public virtual bool GetCHR0(NW4RAnimType focusType)
        {
            BRESEntryNode focusFile = GetAnimation(focusType);
            if (focusFile == null)
            {
                _chr0 = null;
                return false;
            }
            if (TargetModel != null && (_chr0 = (CHR0Node)((ResourceNode)TargetModel).RootNode.FindChildByType(focusFile.Name, true, ResourceType.CHR0)) != null)
                return true;

            foreach (ResourceNode _externalAnimationsNode in _animationSearchNodes)
                if (_externalAnimationsNode != null && (_chr0 = (CHR0Node)_externalAnimationsNode.FindChildByType(focusFile.Name, true, ResourceType.CHR0)) != null)
                    return true;
            return false;
        }

        public void CreateVIS0()
        {
            if (!(_targetModel is MDL0Node))
                return;

            BRESNode group = null;
            BRESEntryNode n = null;
            if ((n = TargetAnimation as BRESEntryNode) != null &&
                (group = n.Parent.Parent as BRESNode) != null)
            {
                _vis0 = group.CreateResource<VIS0Node>(SelectedCHR0.Name);
                foreach (string s in VIS0Indices.Keys)
                {
                    VIS0EntryNode node = null;
                    if ((node = (VIS0EntryNode)_vis0.FindChild(s, true)) == null && ((MDL0BoneNode)((ResourceNode)_targetModel).FindChildByType(s, true, ResourceType.MDL0Bone)).BoneIndex != 0 && s != "EyeYellowM")
                    {
                        node = _vis0.CreateEntry();
                        node.Name = s;
                        node.MakeConstant(true);
                    }
                }
            }
        }

        public void UpdateVis0(ItemCheckEventArgs e)
        {
            BRESEntryNode n;
            if ((n = TargetAnimation as BRESEntryNode) == null ||
                _animFrame == 0 ||
                TargetModel == null)
                return;

        Start:
            if (_vis0 != null)
            {
                int index = e.Index;
                if (index == -1)
                    return;

                MDL0BoneNode bone = ((MDL0ObjectNode)TargetModel.Objects[index])._visBoneNode;

                VIS0EntryNode node = null;
                if ((node = (VIS0EntryNode)_vis0.FindChild(bone.Name, true)) == null && bone.BoneIndex != 0 && bone.Name != "EyeYellowM")
                {
                    node = _vis0.CreateEntry();
                    node.Name = bone.Name;
                    node.MakeConstant(true);
                }

                bool ANIMval = e.NewValue == CheckState.Checked;

                bool nowAnimated = false, alreadyConstant = false;
            Top:
                if (node != null)
                    if (node._entryCount != 0) //Node is animated
                    {
                        bool VIS0val = node.GetEntry((int)_animFrame - 1);

                        if (VIS0val != ANIMval)
                            node.SetEntry((int)_animFrame - 1, ANIMval);
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

                if (node != null && ((VIS0EntryNode)KeyframePanel.visEditor.TargetNode).Name == node.Name)
                    vis0Editor.UpdateEntry();
            }
            else
            {
                CreateVIS0();
                if (_vis0 != null)
                    goto Start;
            }
        }

        public bool VIS0Updating { get { return _vis0Updating; } set { _vis0Updating = value; } }
        private bool _vis0Updating = false;

        public virtual void ApplyVIS0ToInterface()
        {
            TargetModel.ApplyVIS(_vis0, _animFrame);
        }

        #endregion

        #region Playback Panel
        public void pnlPlayback_Resize(object sender, EventArgs e)
        {
            if (pnlPlayback.Width <= pnlPlayback.MinimumSize.Width)
            {
                pnlPlayback.Dock = DockStyle.Left;
                pnlPlayback.Width = pnlPlayback.MinimumSize.Width;
            }
            else
                pnlPlayback.Dock = DockStyle.Fill;
        }

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

            int max = (int)pnlPlayback.numTotalFrames.Value;
            _maxFrame = max;
            pnlPlayback.numFrameIndex.Maximum = max;

            if (Interpolated.Contains(TargetAnimation.GetType()) && TargetAnimation.Loop)
                max--;

            TargetAnimation.FrameCount = max;
        }
        public void btnPrevFrame_Click(object sender, EventArgs e) { pnlPlayback.numFrameIndex.Value--; }
        public void btnNextFrame_Click(object sender, EventArgs e) { pnlPlayback.numFrameIndex.Value++; }
        public void btnPlay_Click(object sender, EventArgs e)
        {
            if (_timer.IsRunning)
                StopAnim();
            else
                PlayAnim();
        }
        #endregion

        public void _interpolationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _interpolationForm = null;
            InterpolationFormOpen = false;
        }

        public virtual void UpdateAnimationPanelDimensions() { }

        protected void VISEntryChanged(object sender, EventArgs e)
        {
            UpdateModel();
        }

        protected void VISIndexChanged(object sender, EventArgs e)
        {
            int i = KeyframePanel.visEditor.listBox1.SelectedIndex;
            if (i >= 0 && i <= MaxFrame && i != CurrentFrame - 1)
                SetFrame(i + 1);
        }
    }
}