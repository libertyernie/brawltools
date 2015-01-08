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
        public void SetCurrentControl()
        {
            Control newControl = null;
            syncTexObjToolStripMenuItem.Checked = (TargetAnimType == NW4RAnimType.SRT || TargetAnimType == NW4RAnimType.PAT);
            switch (TargetAnimType)
            {
                case NW4RAnimType.CHR: newControl = chr0Editor; break;
                case NW4RAnimType.SHP: newControl = shp0Editor; break;
                case NW4RAnimType.VIS: newControl = vis0Editor; break;
                case NW4RAnimType.CLR: newControl = clr0Editor; break;
                case NW4RAnimType.SRT: newControl = srt0Editor; break;
                case NW4RAnimType.PAT: newControl = pat0Editor; break;
            }
            if (_currentControl != newControl)
            {
                if (_currentControl != null)
                    _currentControl.Visible = false;
                _currentControl = newControl;

                if (!(_currentControl is SRT0Editor) && !(_currentControl is PAT0Editor))
                    syncTexObjToolStripMenuItem.Checked = false;

                if (_currentControl != null)
                    _currentControl.Visible = true;
            }
            CheckDimensions();
            UpdatePropDisplay();
        }

        public override void UpdateModel()
        {
            if (_updating)
                return;

            if (EditingAll)
                foreach (IModel n in _targetModels)
                    UpdateModel(n);
            else if (TargetModel != null)
                UpdateModel(TargetModel);

            if (RunTime._articles != null)
                foreach (ArticleInfo a in RunTime._articles)
                    if (a != null && a.Running)
                        a.UpdateModel();

            if (!_playing) 
                UpdatePropDisplay();

            modelPanel.Invalidate();
        }
        
        //public void AnimChanged(NW4RAnimType type)
        //{
        //    //Update animation editors
        //    if (type != NW4RAnimType.SRT) modelListsPanel1.UpdateSRT0Selection(null);
        //    if (type != NW4RAnimType.PAT) modelListsPanel1.UpdatePAT0Selection(null);

        //    switch (type)
        //    {
        //        case NW4RAnimType.CHR:
        //            break;
        //        case NW4RAnimType.SRT:
        //            modelListsPanel1.UpdateSRT0Selection(SelectedSRT0);
        //            break;
        //        case NW4RAnimType.SHP:
        //            shp0Editor.UpdateSHP0Indices();
        //            break;
        //        case NW4RAnimType.PAT:
        //            pat0Editor.UpdateBoxes();
        //            modelListsPanel1.UpdatePAT0Selection(SelectedPAT0);
        //            break;
        //        case NW4RAnimType.VIS: 
        //            vis0Editor.UpdateAnimation();
        //            break;
        //        case NW4RAnimType.CLR: 
        //            clr0Editor.UpdateAnimation();
        //            break;
        //    }

        //    NW4RAnimationNode anim = GetAnimation(type);
        //    if (anim == null)
        //    {
        //        pnlPlayback.numFrameIndex.Maximum = MaxFrame = 0;
        //        pnlPlayback.numTotalFrames.Minimum = 0;
        //        _updating = true;
        //        pnlPlayback.numTotalFrames.Value = 0;
        //        _updating = false;
        //        pnlPlayback.btnPlay.Enabled =
        //        pnlPlayback.numTotalFrames.Enabled =
        //        pnlPlayback.numFrameIndex.Enabled = false;
        //        pnlPlayback.btnLast.Enabled = false;
        //        pnlPlayback.btnFirst.Enabled = false;
        //        pnlPlayback.Enabled = false;
        //        RunTime.SetFrame(-1);
        //    }
        //    else
        //    {
        //        int oldMax = MaxFrame;

        //        MaxFrame = anim.FrameCount;
        //        if (Array.IndexOf(Interpolated, anim.GetType()) >= 0)
        //            MaxFrame += (anim.Loop ? 1 : 0);

        //        _updating = true;
        //        pnlPlayback.btnPlay.Enabled =
        //        pnlPlayback.numFrameIndex.Enabled =
        //        pnlPlayback.numTotalFrames.Enabled = true;
        //        pnlPlayback.Enabled = true;
        //        pnlPlayback.numTotalFrames.Value = MaxFrame;
        //        if (syncLoopToAnimationToolStripMenuItem.Checked)
        //            pnlPlayback.chkLoop.Checked = GetAnimation(type).Loop;
        //        _updating = false;

        //        if (MaxFrame < oldMax)
        //        {
        //            RunTime.SetFrame(0);
        //            pnlPlayback.numFrameIndex.Maximum = MaxFrame;
        //        }
        //        else
        //        {
        //            pnlPlayback.numFrameIndex.Maximum = MaxFrame;
        //            RunTime.SetFrame(0);
        //        }
        //    }
        //}

        //public void numFrameIndex_ValueChanged(object sender, EventArgs e)
        //{
        //    int val = (int)pnlPlayback.numFrameIndex.Value;
        //    if (val != CurrentFrame)
        //        SetFrame(val);
        //}

        internal void UpdatePlaybackPanel()
        {
            int frame = CurrentFrame + 1;

            pnlPlayback.btnNextFrame.Enabled = frame < MaxFrame;
            pnlPlayback.btnPrevFrame.Enabled = frame > 0;
            pnlPlayback.btnLast.Enabled = frame != MaxFrame;
            pnlPlayback.btnFirst.Enabled = frame > 1;
            if (frame <= pnlPlayback.numFrameIndex.Maximum)
                pnlPlayback.numFrameIndex.Value = frame;
        }

        public override void SetFrame(int index)
        {
            RunTime.SetFrame(index - 1);
        }
    }
}
