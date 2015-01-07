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
        private const float _orbRadius = 1.0f;
        private const float _circRadius = 1.2f;
        private const float _axisSnapRange = 7.0f;
        private const float _selectRange = 0.03f; //Selection error range for orb and circ
        private const float _axisSelectRange = 0.15f; //Selection error range for axes
        private const float _selectOrbScale = _selectRange / _orbRadius;
        private const float _circOrbScale = _circRadius / _orbRadius;

        public event EventHandler TargetModelChanged;

        private delegate void DelegateOpenFile(String s);
        private DelegateOpenFile m_DelegateOpenFile;

        public bool _updating;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentFrame { get { return RunTime.CurrentFrame + 1; } set { RunTime.CurrentFrame = value - 1; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MaxFrame { get { return RunTime.MaxFrame; } set { RunTime.MaxFrame = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Loop { get { return RunTime.Loop; } set { RunTime.Loop = value; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Updating { get { return _updating; } set { _updating = value; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CHR0Editor CHR0Editor { get { return chr0Editor; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SRT0Editor SRT0Editor { get { return srt0Editor; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SHP0Editor SHP0Editor { get { return shp0Editor; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VIS0Editor VIS0Editor { get { return vis0Editor; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PAT0Editor PAT0Editor { get { return pat0Editor; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SCN0Editor SCN0Editor { get { return null; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CLR0Editor CLR0Editor { get { return clr0Editor; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ResourceNode ExternalAnimationsNode { get { return Manager.Animations; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SyncVIS0 { get { return syncObjectsListToVIS0ToolStripMenuItem.Checked; } set { syncObjectsListToVIS0ToolStripMenuItem.Checked = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Playing { get { return RunTime._playing; } set { RunTime._playing = value; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel AnimCtrlPnl { get { return panel3; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel AnimEditors { get { return animEditors; } }

        public bool
            _renderHurtboxes,
            _renderHitboxes;

        public bool _resetCam = true;
        public bool _hide = false;
        private void ModelChanged(MDL0Node model)
        {
            if (model != null && !_targetModels.Contains(model))
                _targetModels.Add(model);

            if (_targetModel != null)
                _targetModel.IsTargetModel = false;

            if (_targetModel != null)
                modelPanel.RemoveTarget(_targetModel);

            if ((_targetModel = model) != null)
            {
                modelPanel.AddTarget(_targetModel);
                listPanel.VIS0Indices = _targetModel.VIS0Indices;
                _targetModel.IsTargetModel = true;
                ResetVertexColors();
                hurtboxEditor._mainControl_TargetModelChanged(null, null);
            }

            if (_resetCam)
            {
                modelPanel.ResetCamera();
                RunTime.SetFrame(0);
            }
            else
                _resetCam = true;

            modelListsPanel1.Reset();

            //if (TargetModelChanged != null)
            //    TargetModelChanged(this, null);

            //_updating = true;
            //if (_targetModel != null && !_editingAll)
            //    comboCharacters.SelectedItem = _targetModel;
            //_updating = false;

            if (_targetModel != null)
                RenderBones = _targetModel._renderBones;
        }

        MDL0BoneNode _selectedBone = null;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MDL0BoneNode SelectedBone 
        {
            get { return _selectedBone; } 
            set 
            {
                if (_selectedBone != null)
                    _selectedBone._boneColor = _selectedBone._nodeColor = Color.Transparent;
                
                if ((_selectedBone = value) != null)
                {
                    _selectedBone._boneColor = Color.FromArgb(0, 128, 255);
                    _selectedBone._nodeColor = Color.FromArgb(255, 128, 0);
                }

                if (comboCharacters.SelectedItem != null & !(comboCharacters.SelectedItem is MDL0Node) && comboCharacters.SelectedItem.ToString() == "All")
                    if (_selectedBone != null)
                        if (TargetModel != _selectedBone.Model)
                        {
                            //The user selected a bone from another model.
                            TargetModel = _selectedBone.Model;
                            _resetCam = false;
                        }

                modelListsPanel1.lstBones.SelectedItem = _selectedBone;
                chr0Editor.UpdatePropDisplay();

                //if (_chr0 != null && _selectedBone != null && TargetAnimType == AnimType.CHR)
                //    KeyframePanel.TargetSequence = _chr0.FindChild(_selectedBone.Name, false);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MDL0MaterialRefNode TargetTexRef { get { return _targetTexRef; } set { _targetTexRef = value; UpdatePropDisplay(); } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VIS0EntryNode TargetVisEntry 
        { 
            get { return _targetVisEntry; } 
            set 
            {
                _targetVisEntry = value; 
                UpdatePropDisplay();
                //pnlKeyframes.TargetSequence = _targetVisEntry as ResourceNode;
                //pnlKeyframes.chkConstant.Checked = _targetVisEntry._flags.HasFlag(VIS0Flags.Constant);
                //pnlKeyframes.chkEnabled.Checked = _targetVisEntry._flags.HasFlag(VIS0Flags.Enabled);
            } 
        }
        
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EnableTransformEdit
        {
            get { return _enableTransform; }
            set 
            {
                if (_enableTransform == value)
                    return;

                _enableTransform = value;
                chr0Editor.Enabled =
                srt0Editor.Enabled =
                shp0Editor.Enabled =
                vis0Editor.Enabled =
                pat0Editor.Enabled = 
                clr0Editor.Enabled = value; 
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderFloor 
        {
            get { return _renderFloor; } 
            set
            {
                _renderFloor = value;
                _updating = true;
                chkFloor.Checked = toggleFloor.Checked = _renderFloor;
                _updating = false;
                modelPanel.Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderBones
        {
            get { return _renderBones; }
            set
            {
                //if (_editingAll)
                //    foreach (MDL0Node m in _targetModels)
                //        m._renderBones = value;
                //else if (TargetModel != null)
                //    TargetModel._renderBones = value;
                //if (RunTime._articles != null)
                //foreach (ArticleInfo i in RunTime._articles)
                //    if (i.Running && i.ModelVisible)
                //        i._model._renderBones = value;

                //_renderBones = value;
                //_updating = true;
                //chkBones.Checked = toggleBones.Checked = _renderBones;
                //_updating = false;
                //modelPanel.Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderPolygons
        {
            get { return _renderPolygons; }
            set
            {
                //if (_editingAll)
                //    foreach (MDL0Node m in _targetModels)
                //        m._renderPolygons = value;
                //else if (TargetModel != null)
                //    TargetModel._renderPolygons = value;
                //if (RunTime._articles != null)
                //foreach (ArticleInfo i in RunTime._articles)
                //    if (i.Running && i.ModelVisible)
                //        i._model._renderPolygons = value;

                //_renderPolygons = value;
                //_updating = true;
                //chkPolygons.Checked = togglePolygons.Checked = _renderPolygons;
                //_updating = false;
                //modelPanel.Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderBox
        {
            get { return _renderBox; }
            set
            {
                //if (_editingAll && _targetModels != null)
                //    foreach (MDL0Node m in _targetModels)
                //        m._renderBox = value;
                //else if (TargetModel != null)
                //    TargetModel._renderBox = value;
                //if (RunTime._articles != null)
                //    foreach (ArticleInfo i in RunTime._articles)
                //        if (i.Running && i.ModelVisible)
                //            i._model._renderBox = value;

                //_renderBox = value;
                //_updating = true;
                //boundingBoxToolStripMenuItem.Checked = _renderBox;
                //_updating = false;
                //modelPanel.Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DontRenderOffscreen
        {
            get { return _dontRenderOffscreen; }
            set
            {
                //if (_editingAll && _targetModels != null)
                //    foreach (MDL0Node m in _targetModels)
                //        m._dontRenderOffscreen = value;
                //else if (TargetModel != null)
                //    TargetModel._dontRenderOffscreen = value;

                //_dontRenderOffscreen = value;
                //_updating = true;
                //chkDontRenderOffscreen.Checked = _dontRenderOffscreen;
                //_updating = false;
                //modelPanel.Invalidate();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderLightDisplay { get { return _renderLightDisplay; } set { _renderLightDisplay = value; modelPanel.Invalidate(); } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public uint AllowedUndos { get { return _allowedUndos; } set { _allowedUndos = value; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public InterpolationEditor InterpolationEditor { get { return null; } set { } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool InterpolationEditorVisible
        {
            get
            {
                //if (_interpolationEditor == null || _interpolationEditor.IsDisposed || !_interpolationEditor._open || !_interpolationEditor.Visible)
                    return false;
                //else
                //    return true;
            }
            set
            {
                //if (value)
                //{
                //    if (_interpolationEditor == null || _interpolationEditor.IsDisposed || !_interpolationEditor._open || !_interpolationEditor.Visible)
                //    {
                //        _interpolationEditor = new InterpolationEditor(this);
                //        _interpolationEditor.Visible = true;
                //        _interpolationEditor._open = true;
                //        _interpolationEditor.TopMost = true;
                //    }

                //}
                //else if (_interpolationEditor._open)
                //{
                //    _interpolationEditor.Visible = false;
                //    _interpolationEditor._open = false;
                //}
                //interpolationEditorToolStripMenuItem.Checked = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LinearInterpolation { get { return false; } set { } }

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
