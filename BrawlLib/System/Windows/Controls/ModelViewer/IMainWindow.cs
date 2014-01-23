using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace System.Windows.Forms
{
    public interface IMainWindow
    {
        void numFPS_ValueChanged(object sender, EventArgs e);
        void numFrameIndex_ValueChanged(object sender, EventArgs e);
        void numTotalFrames_ValueChanged(object sender, EventArgs e);
        void SelectedPolygonChanged(object sender, EventArgs e);
        void chkLoop_CheckedChanged(object sender, EventArgs e);
        void btnPlay_Click(object sender, EventArgs e);
        void UpdateVis0(object sender, EventArgs e);

        void CheckDimensions();
        void ReadVIS0();
        void SetCurrentControl();
        void UpdateModel();
        void UpdatePropDisplay();
        void SetFrame(int index);
        void BoneChange(MDL0BoneNode bone);
        void GetFiles(AnimType focusType);
        void AnimChanged(AnimType type);

        AnimationNode GetAnimation(AnimType type);

        ModelPlaybackPanel PlaybackPanel { get; }
        KeyframePanel KeyframePanel { get; }
        BonesPanel BonesPanel { get; }
        ModelPanel ModelPanel { get; }
        CHR0Editor CHR0Editor { get; }
        SRT0Editor SRT0Editor { get; }
        SHP0Editor SHP0Editor { get; }
        PAT0Editor PAT0Editor { get; }
        VIS0Editor VIS0Editor { get; }
        SCN0Editor SCN0Editor { get; }
        CLR0Editor CLR0Editor { get; }
        Panel AnimCtrlPnl { get; }
        Panel AnimEditors { get; }
        InterpolationEditor InterpolationEditor { get; }
        ModelViewerForm ModelViewerForm { get; }

        CHR0Node SelectedCHR0 { get; set; }
        SRT0Node SelectedSRT0 { get; set; }
        SHP0Node SelectedSHP0 { get; set; }
        PAT0Node SelectedPAT0 { get; set; }
        VIS0Node SelectedVIS0 { get; set; }
        SCN0Node SelectedSCN0 { get; set; }
        CLR0Node SelectedCLR0 { get; set; }
        MDL0BoneNode SelectedBone { get; set; }

        int CurrentFrame { get; set; }
        int MaxFrame { get; set; }
        bool Playing { get; set; }
        bool Loop { get; set; }
        bool Focused { get; }
        Drawing.Point Location { get; set; }
        Drawing.Size Size { get; set; }

        bool Updating { get; set; }
        bool EnableTransformEdit { get; set; }
        bool SyncVIS0 { get; set; }
        bool RenderLightDisplay { get; set; }
        uint AllowedUndos { get; set; }
        bool LinearInterpolation { get; set; }

        MDL0Node TargetModel { get; set; }
        VIS0EntryNode TargetVisEntry { get; set; }
        MDL0MaterialRefNode TargetTexRef { get; set; }
        ResourceNode ExternalAnimationsNode { get; }
        AnimType TargetAnimType { get; set; }
    }

    public static class StaticMainWindow
    {
        public static Color _floorHue = Color.FromArgb(255, 128, 128, 191);
    }

    public enum AnimType : int
    {
        None = -1,
        CHR = 0,
        SRT = 1,
        SHP = 2,
        PAT = 3,
        VIS = 4,
        CLR = 5,
        SCN = 6,
    }

    public class TransparentPanel : Panel
    {
        public TransparentPanel() { SetStyle(ControlStyles.UserPaint, true); }

        bool _transparent = true;

        protected override CreateParams CreateParams
        {
            get
            {
                if (_transparent)
                {
                    CreateParams createParams = base.CreateParams;
                    createParams.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                    return createParams;
                }
                else return base.CreateParams;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)0x84)
                m.Result = (IntPtr)(-1);
            else
                base.WndProc(ref m);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (_transparent)
            {
                // Do not paint background.
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }
    }
}