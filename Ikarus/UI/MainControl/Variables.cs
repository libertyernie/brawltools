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
using Ikarus;
using Ikarus.MovesetFile;

namespace Ikarus.UI
{
    public partial class MainControl : ModelEditorBase
    {
        private DelegateOpenFile m_DelegateOpenFile;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override TransformType ControlType
        {
            get { return _controlType; }
            set
            {
                if (_controlType == value)
                    return;
                
                _controlType = value;

                _updating = true;
                rotationToolStripMenuItem.Checked = _controlType == TransformType.Rotation;
                translationToolStripMenuItem.Checked = _controlType == TransformType.Rotation;
                scaleToolStripMenuItem.Checked = _controlType == TransformType.Rotation;
                _updating = false;
            }
        }

        TransformType _controlType;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ResourceNode ExternalAnimationsNode { get { return Manager.Animations; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel AnimCtrlPnl { get { return panel3; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel AnimEditors { get { return animEditors; } }

        public bool
            _renderHurtboxes,
            _renderHitboxes;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override KeyframePanel KeyframePanel { get { return null; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override BonesPanel BonesPanel { get { return modelListsPanel1.bonesPanel1; } }

        public MiscHurtBox _selectedHurtbox;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MiscHurtBox SelectedHurtbox
        {
            get { return _selectedHurtbox; }
            set 
            {
                if ((_selectedHurtbox = value) != null)
                {
                    EnableHurtboxEditor();
                    hurtboxEditor.TargetHurtBox = value;
                }
                else
                {
                    DisableHurtboxEditor();
                    hurtboxEditor.TargetHurtBox = null;
                }
            }
        }
    }
}
