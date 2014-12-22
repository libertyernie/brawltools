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
        public List<CollisionNode> _collisions = new List<CollisionNode>();
        private CollisionNode _targetCollision;
        public ResourceNode _externalAnimationsNode;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CollisionNode TargetCollision
        {
            get { return _targetCollision; }
            set { _targetCollision = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override BonesPanel BonesPanel { get { return rightPanel.pnlBones; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override KeyframePanel KeyframePanel { get { return rightPanel.pnlKeyframes; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override int CurrentFrame
        {
            get { return base.CurrentFrame; }
            set
            {
                base.CurrentFrame = value;
                HandleFirstPersonCamera();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool DisableBonesWhenPlaying
        {
            get { return disableBonesWhenPlayingToolStripMenuItem.Checked; }
            set { disableBonesWhenPlayingToolStripMenuItem.Checked = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool SyncVIS0
        {
            get { return syncObjectsListToVIS0ToolStripMenuItem.Checked; }
            set { syncObjectsListToVIS0ToolStripMenuItem.Checked = value; }
        }
        //[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public override Panel AnimCtrlPnl { get { return animCtrlPnl; } }
        //[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public override Panel AnimEditors { get { return animEditors; } }
        public override bool BackgroundImageLoaded
        {
            get { return loadImageToolStripMenuItem.Text == "Load Image"; }
            set { loadImageToolStripMenuItem.Text = value ? "Clear Image" : "Load Image"; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool DoNotHighlightOnMouseMove 
        {
            get { return dontHighlightBonesAndVerticesToolStripMenuItem.Checked; }
            set { dontHighlightBonesAndVerticesToolStripMenuItem.Checked = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string ScreenCaptureExtension { get { return _imgExt; } set { _imgExt = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string ScreenCaptureFolder
        {
            get { return ScreenCapBgLocText.Text; }
            set { ScreenCapBgLocText.Text = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ResourceNode ExternalAnimationsNode
        {
            get { return _externalAnimationsNode; }
            set { _externalAnimationsNode = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool InterpolationFormOpen
        {
            get { return interpolationEditorToolStripMenuItem.Checked; }
            set { interpolationEditorToolStripMenuItem.Checked = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override TransformType ControlType
        {
            get { return (TransformType)cboToolSelect.SelectedIndex; }
            set
            {
                if ((TransformType)cboToolSelect.SelectedIndex == value)
                    return;

                cboToolSelect.SelectedIndex = (int)value;
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override NW4RAnimType TargetAnimType
        {
            get { return _targetAnimType; }
            set
            {
                _targetAnimType = value;
                leftPanel.TargetAnimType = TargetAnimType;
                SetCurrentControl();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool PlayCHR0 { get { return playCHR0ToolStripMenuItem.Checked; } set { playCHR0ToolStripMenuItem.Checked = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool PlaySRT0 { get { return playSRT0ToolStripMenuItem.Checked; } set { playSRT0ToolStripMenuItem.Checked = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool PlayPAT0 { get { return playPAT0ToolStripMenuItem.Checked; } set { playPAT0ToolStripMenuItem.Checked = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool PlayVIS0 { get { return playVIS0ToolStripMenuItem.Checked; } set { playCHR0ToolStripMenuItem.Checked = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool PlayCLR0 { get { return playCLR0ToolStripMenuItem.Checked; } set { playCLR0ToolStripMenuItem.Checked = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(true)]
        public override bool PlaySCN0 { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool PlaySHP0 { get { return playSHP0ToolStripMenuItem.Checked; } set { playSHP0ToolStripMenuItem.Checked = value; } }
    }
}