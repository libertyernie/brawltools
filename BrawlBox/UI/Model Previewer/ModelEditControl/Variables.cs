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
        public event GLRenderEventHandler EventPostRender, EventPreRender;
        public event MouseEventHandler EventMouseDown, EventMouseMove, EventMouseUp;
        public event EventHandler TargetModelChanged, ModelViewerChanged;

        private delegate void DelegateOpenFile(String s);
        private DelegateOpenFile m_DelegateOpenFile;

        private const float _orbRadius = 1.0f;
        private const float _circRadius = 1.2f;
        private const float _axisSnapRange = 7.0f;
        private const float _selectRange = 0.03f; //Selection error range for orb and circ
        private const float _axisSelectRange = 0.15f; //Selection error range for axes
        private const float _selectOrbScale = _selectRange / _orbRadius;
        private const float _circOrbScale = _circRadius / _orbRadius;

        public int _animFrame = 0, _maxFrame;
        public bool _updating, _loop;

        public bool _resetCamera = true;

        public CHR0Node _chr0;
        public SRT0Node _srt0;
        public SHP0Node _shp0;
        public PAT0Node _pat0;
        public VIS0Node _vis0;
        public SCN0Node _scn0;
        public CLR0Node _clr0;

        public bool _rotating, _translating, _scaling;
        private Vector3 _lastPointBone, _firstPointBone, _lastPointWorld, _firstPointWorld;
        private Vector3 _oldAngles, _oldPosition, _oldScale;
        private bool _snapX, _snapY, _snapZ, _snapCirc;
        private bool _hiX, _hiY, _hiZ, _hiCirc, _hiSphere;

        public List<MDL0Node> _targetModels = new List<MDL0Node>();
        private MDL0Node _targetModel;
        private CollisionNode _targetCollision;

        public List<CollisionNode> _collisions = new List<CollisionNode>();

        public Color _clearColor;
        public MDL0MaterialRefNode _targetTexRef = null;
        public VIS0EntryNode _targetVisEntry;
        public bool _enableTransform = true;

        public bool _renderFloor, _renderBones = true, _renderBox, _dontRenderOffscreen = true, _renderVertices, _renderNormals, _renderPolygons = true, _renderCollisions = true, _renderWireframe, _collMatch;

        public static BindingList<AnimType> _editableAnimTypes = new BindingList<AnimType>()
        {
            AnimType.CHR,
            AnimType.SRT,
            AnimType.SHP,
            AnimType.PAT,
            AnimType.VIS,
            AnimType.CLR,
            AnimType.SCN,
        };

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MaxFrame { get { return _maxFrame; } set { _maxFrame = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Updating { get { return _updating; } set { _updating = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Loop { get { return _loop; } set { _loop = value; } }

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
        public SCN0Editor SCN0Editor { get { return scn0Editor; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CLR0Editor CLR0Editor { get { return clr0Editor; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ResourceNode ExternalAnimationsNode { get { return _externalAnimationsNode; } set { _externalAnimationsNode = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SyncVIS0 { get { return syncObjectsListToVIS0ToolStripMenuItem.Checked; } set { syncObjectsListToVIS0ToolStripMenuItem.Checked = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Playing { get { return _playing; } set { _playing = value; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel AnimCtrlPnl { get { return animCtrlPnl; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel AnimEditors { get { return animEditors; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModelPlaybackPanel PlaybackPanel { get { return pnlPlayback; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public KeyframePanel KeyframePanel { get { return rightPanel.pnlKeyframes; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BonesPanel BonesPanel { get { return rightPanel.pnlBones; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModelPanel ModelPanel { get { return _viewerForm == null ? modelPanel : _viewerForm.modelPanel1; } }
        
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModelViewerForm ModelViewerForm { get { return _viewerForm; } }
        ModelViewerForm _viewerForm = null;
        
        public AnimationNode TargetAnimation 
        {
            get { return GetAnimation(TargetAnimType); } 
            set { SetAnimation(TargetAnimType, value); } 
        }

        public AnimationNode GetAnimation(AnimType type)
        {
            switch (type)
            {
                case AnimType.CHR: return SelectedCHR0;
                case AnimType.SRT: return SelectedSRT0;
                case AnimType.SHP: return SelectedSHP0;
                case AnimType.PAT: return SelectedPAT0;
                case AnimType.VIS: return SelectedVIS0;
                case AnimType.SCN: return SelectedSCN0;
                case AnimType.CLR: return SelectedCLR0;
                default: return null;
            }
        }
        public void SetAnimation(AnimType type, AnimationNode value)
        {
            switch (type)
            {
                case AnimType.CHR: SelectedCHR0 = value as CHR0Node; break;
                case AnimType.SRT: SelectedSRT0 = value as SRT0Node; break;
                case AnimType.SHP: SelectedSHP0 = value as SHP0Node; break;
                case AnimType.PAT: SelectedPAT0 = value as PAT0Node; break;
                case AnimType.VIS: SelectedVIS0 = value as VIS0Node; break;
                case AnimType.SCN: SelectedSCN0 = value as SCN0Node; break;
                case AnimType.CLR: SelectedCLR0 = value as CLR0Node; break;
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MDL0Node TargetModel { get { return _targetModel; } set { ModelChanged(value); } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CollisionNode TargetCollision { // Should be set only when a collision is chosen in the dropdown.
            get {
                return _targetCollision;
            }
            set {
                _targetCollision = value;
                leftPanel.Reset();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CHR0Node SelectedCHR0
        { 
            get { return _chr0; } 
            set 
            {
                _chr0 = value;

                if (_updating)
                    return;

                AnimChanged(AnimType.CHR);
                UpdatePropDisplay();
            } 
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SRT0Node SelectedSRT0 
        {
            get { return _srt0; } 
            set 
            { 
                _srt0 = value;

                if (_updating)
                    return;

                AnimChanged(AnimType.SRT);
                UpdatePropDisplay();
            } 
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SHP0Node SelectedSHP0
        {
            get { return _shp0; }
            set
            {
                _shp0 = value;

                if (_updating)
                    return;

                AnimChanged(AnimType.SHP);
                UpdatePropDisplay();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PAT0Node SelectedPAT0
        {
            get { return _pat0; }
            set
            {
                _pat0 = value; 
                
                if (_updating)
                    return;

                AnimChanged(AnimType.PAT);
                UpdatePropDisplay();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VIS0Node SelectedVIS0
        {
            get { return _vis0; }
            set
            {
                _vis0 = value; 
                
                if (_updating)
                    return;

                AnimChanged(AnimType.VIS);
                UpdatePropDisplay();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SCN0Node SelectedSCN0
        {
            get { return _scn0; }
            set
            {
                _scn0 = value;

                if (_updating)
                    return;

                AnimChanged(AnimType.SCN);
                UpdatePropDisplay();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CLR0Node SelectedCLR0
        {
            get { return _clr0; }
            set
            {
                _clr0 = value;

                if (_updating)
                    return;

                AnimChanged(AnimType.CLR);
                UpdatePropDisplay();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ClearColor { get { return _clearColor; } set { _clearColor = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image BGImage { get { return ModelPanel.BackgroundImage; } set { ModelPanel.BackgroundImage = value; } }

        MDL0BoneNode _selectedBone = null;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MDL0BoneNode SelectedBone 
        {
            get { return _selectedBone; } 
            set 
            {
                if (_selectedBone != null)
                    _selectedBone._boneColor = _selectedBone._nodeColor = Color.Transparent;
                
                bool boneSelected = (_selectedBone = value) != null;
                if (boneSelected)
                {
                    _selectedBone._boneColor = Color.FromArgb(0, 128, 255);
                    _selectedBone._nodeColor = Color.FromArgb(255, 128, 0);
                }

                chkZoomExtents.Enabled = boneSelected;

                if (models.SelectedItem != null
                    && !(models.SelectedItem is MDL0Node)
                    && models.SelectedItem.ToString() == "All"
                    && boneSelected
                    && TargetModel != _selectedBone.Model)
                {
                    _resetCamera = false;

                    //The user selected a bone from another model.
                    TargetModel = _selectedBone.Model;
                }

                rightPanel.pnlBones.SetSelectedBone(_selectedBone);
                //weightEditor.BoneChanged();

                if (TargetModel != null)
                    TargetModel._selectedBone = _selectedBone;

                if (TargetAnimType == AnimType.CHR)
                    rightPanel.pnlKeyframes.TargetSequence =
                        _chr0 != null && _selectedBone != null ?
                        _chr0.FindChild(_selectedBone.Name, false) :
                        null;

                UpdatePropDisplay();
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
                rightPanel.pnlKeyframes.TargetSequence = _targetVisEntry as ResourceNode;
                rightPanel.pnlKeyframes.chkConstant.Checked = _targetVisEntry._flags.HasFlag(VIS0Flags.Constant);
                rightPanel.pnlKeyframes.chkEnabled.Checked = _targetVisEntry._flags.HasFlag(VIS0Flags.Enabled);
            } 
        }
        
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentFrame 
        { 
            get { return _animFrame; } 
            set 
            {
                _animFrame = value;
                UpdateModel(); 

                //The more frames there are in the animation, the more the viewer lags
                //if (InterpolationEditor != null)
                //    InterpolationEditor.Frame = CurrentFrame;
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
                scn0Editor.Enabled =
                clr0Editor.Enabled = 
                KeyframePanel.Enabled = value;
                if (InterpolationEditor != null && InterpolationEditor.Visible)
                    InterpolationEditor.Enabled = value;

                if (value)
                    UpdatePropDisplay();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderFloor 
        {
            get { return _renderFloor; } 
            set
            {
                chkFloor.Checked = toggleFloor.Checked = _renderFloor = value;
                ModelPanel.Invalidate();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderBones
        {
            get { return _renderBones; }
            set
            {
                chkBones.Checked = toggleBones.Checked = _renderBones = value;

                if (_editingAll)
                    foreach (MDL0Node m in _targetModels)
                        m._renderBones = _renderBones;
                else if (TargetModel != null)
                    TargetModel._renderBones = _renderBones;

                ModelPanel.Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderVertices
        {
            get { return _renderVertices; }
            set
            {
                chkVertices.Checked = toggleVertices.Checked = _renderVertices = value;

                if (_editingAll)
                    foreach (MDL0Node m in _targetModels)
                        m._renderVertices = _renderVertices;
                else 
                    if (TargetModel != null)
                        TargetModel._renderVertices = _renderVertices;

                ModelPanel.Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderNormals
        {
            get { return _renderNormals; }
            set
            {
                toggleNormals.Checked = _renderNormals = value;

                if (_editingAll)
                    foreach (MDL0Node m in _targetModels)
                        m._renderNormals = _renderNormals;
                else
                    if (TargetModel != null)
                        TargetModel._renderNormals = _renderNormals;

                ModelPanel.Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderPolygons
        {
            get { return _renderPolygons; }
            set
            {
                chkPolygons.Checked = togglePolygons.Checked = _renderPolygons = value;

                if (_editingAll)
                    foreach (MDL0Node m in _targetModels)
                        m._renderPolygons = _renderPolygons;
                else if (TargetModel != null)
                    TargetModel._renderPolygons = _renderPolygons;

                ModelPanel.Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderCollisions
        {
            get { return _renderCollisions; }
            set
            {
                chkCollisions.Checked = toggleCollisions.Checked = _renderCollisions = value;

                if (_editingAll)
                    foreach (CollisionNode m in _collisions)
                        foreach (CollisionObject o in m._objects)
                            o._render = _renderCollisions;
                else
                    if (TargetCollision != null)
                    {
                        foreach (CollisionObject o in TargetCollision._objects)
                            o._render = _renderCollisions;
                        for (int i = 0; i < leftPanel.lstObjects.Items.Count; i++)
                            leftPanel.lstObjects.SetItemChecked(i, _renderCollisions);
                    }

                ModelPanel.Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderWireframe
        {
            get { return _renderWireframe; }
            set
            {
                wireframeToolStripMenuItem.Checked = _renderWireframe = value;

                if (_editingAll)
                    foreach (MDL0Node m in _targetModels)
                        m._renderWireframe = _renderWireframe;
                else if (TargetModel != null)
                    TargetModel._renderWireframe = _renderWireframe;

                ModelPanel.Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderBox
        {
            get { return _renderBox; }
            set
            {
                boundingBoxToolStripMenuItem.Checked = _renderBox = value;

                if (_editingAll && _targetModels != null)
                    foreach (MDL0Node m in _targetModels)
                        m._renderBox = _renderBox;
                else if (TargetModel != null)
                    TargetModel._renderBox = _renderBox;

                ModelPanel.Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DontRenderOffscreen
        {
            get { return _dontRenderOffscreen; }
            set
            {
                chkDontRenderOffscreen.Checked = _dontRenderOffscreen = value;

                if (_editingAll && _targetModels != null)
                    foreach (MDL0Node m in _targetModels)
                        m._dontRenderOffscreen = _dontRenderOffscreen;
                else if (TargetModel != null)
                    TargetModel._dontRenderOffscreen = _dontRenderOffscreen;

                ModelPanel.Invalidate();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderLightDisplay { get { return _renderLightDisplay; } set { _renderLightDisplay = value; ModelPanel.Invalidate(); } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public uint AllowedUndos { get { return _allowedUndos; } set { _allowedUndos = value; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public InterpolationEditor InterpolationEditor { get { return _interpolationEditor.Visible ? _interpolationEditor : _interpolationForm != null ? _interpolationForm._interpolationEditor : null; } }
        private InterpolationEditor _interpolationEditor;
        public InterpolationForm _interpolationForm = null;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LinearInterpolation { get { return linearInterpolationToolStripMenuItem.Checked; } set { linearInterpolationToolStripMenuItem.Checked = value; } }
    }
}
