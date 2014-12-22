using BrawlLib.Modeling;
using BrawlLib.OpenGL;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
    public partial class ModelEditorBase : UserControl
    {
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderFloor
        {
            get { return ModelPanel.RenderFloor; }
            set { ModelPanel.RenderFloor = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderBones
        {
            get { return ModelPanel.RenderBones; }
            set { ModelPanel.RenderBones = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderVertices
        {
            get { return ModelPanel.RenderVertices; }
            set { ModelPanel.RenderVertices = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderNormals
        {
            get { return ModelPanel.RenderNormals; }
            set { ModelPanel.RenderNormals = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderPolygons
        {
            get { return ModelPanel.RenderPolygons; }
            set { ModelPanel.RenderPolygons = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderCollisions
        {
            get { return ModelPanel.RenderCollisions; }
            set { ModelPanel.RenderCollisions = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderWireframe
        {
            get { return ModelPanel.RenderWireframe; }
            set { ModelPanel.RenderWireframe = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderBox
        {
            get { return ModelPanel.RenderBox; }
            set { ModelPanel.RenderBox = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DontRenderOffscreen
        {
            get { return ModelPanel.DontRenderOffscreen; }
            set { ModelPanel.DontRenderOffscreen = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderLightDisplay
        {
            get { return _renderLightDisplay; }
            set { _renderLightDisplay = value; ModelPanel.Invalidate(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MaxFrame { get { return _maxFrame; } set { _maxFrame = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Updating { get { return _updating; } set { _updating = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Loop { get { return _loop; } set { _loop = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Playing { get { return _playing; } set { _playing = value; } }

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
        public ModelPlaybackPanel PlaybackPanel { get { return pnlPlayback; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModelPanel ModelPanel { get { return _viewerForm == null ? modelPanel : _viewerForm.modelPanel1; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModelViewerForm ModelViewerForm { get { return _viewerForm; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IModel TargetModel { get { return _targetModel; } set { ModelChanged(value); } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CHR0Node SelectedCHR0
        {
            get { return _chr0; }
            set
            {
                _chr0 = value;

                if (_updating)
                    return;

                AnimChanged(NW4RAnimType.CHR);
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

                AnimChanged(NW4RAnimType.SRT);
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

                AnimChanged(NW4RAnimType.SHP);
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

                AnimChanged(NW4RAnimType.PAT);
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

                AnimChanged(NW4RAnimType.VIS);
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

                AnimChanged(NW4RAnimType.SCN);
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

                AnimChanged(NW4RAnimType.CLR);
                UpdatePropDisplay();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ClearColor { get { return _clearColor; } set { _clearColor = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image BGImage
        {
            get { return ModelPanel.BackgroundImage; }
            set { BackgroundImageLoaded = (ModelPanel.BackgroundImage = value) != null; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AllowZoomExtents { get { return _selectedBone != null; } }
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
        public uint AllowedUndos { get { return _allowedUndos; } set { _allowedUndos = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public InterpolationEditor InterpolationEditor { get { return _interpolationEditor.Visible ? _interpolationEditor : _interpolationForm != null ? _interpolationForm._interpolationEditor : null; } }
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
                KeyframePanel.TargetSequence = _targetVisEntry as ResourceNode;
                KeyframePanel.chkConstant.Checked = _targetVisEntry._flags.HasFlag(VIS0Flags.Constant);
                KeyframePanel.chkEnabled.Checked = _targetVisEntry._flags.HasFlag(VIS0Flags.Enabled);
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual NW4RAnimType TargetAnimType
        {
            get { return _targetAnimType; }
            set
            {
                if (_targetAnimType == value)
                    return;

                _targetAnimType = value;
            }
        }

        //public Vector3 CamLoc() { return ModelPanel.Camera.GetPoint(); }
        public Vector3 CamLoc(ModelPanel panel) { return panel.Camera.GetPoint(); }

        //public float CamDistance(Vector3 v) { return CamDistance(v, ModelPanel._fovY); }
        //public float CamDistance(Vector3 v, float fovY) { return v.TrueDistance(CamLoc()) / _orbRadius * (fovY / 45.0f) * 0.1f; }
        public float CamDistance(Vector3 v, ModelPanel panel) { return v.TrueDistance(CamLoc(panel)) / _orbRadius * (panel._fovY / 45.0f) * 0.1f; }
        
        //public float OrbRadius() { return CamDistance(BoneLoc()); }
        public float OrbRadius(IBoneNode b, ModelPanel panel) { return CamDistance(BoneLoc(b), panel); }
        public float OrbRadius(Vector3 b, ModelPanel panel) { return CamDistance(b, panel); }

        //public float VertexOrbRadius() { return CamDistance((Vector3)VertexLoc()); }
        public float VertexOrbRadius(ModelPanel panel) { return CamDistance((Vector3)VertexLoc(), panel); }

        //public Matrix CamFacingMatrix() { return Matrix.TransformMatrix(new Vector3(OrbRadius()), BoneLoc().LookatAngles(CamLoc()) * Maths._rad2degf, BoneLoc()); }
        public Matrix CamFacingMatrix(IBoneNode b, ModelPanel panel) { return Matrix.TransformMatrix(new Vector3(OrbRadius(b, panel)), BoneLoc(b).LookatAngles(CamLoc(panel)) * Maths._rad2degf, BoneLoc(b)); }
        
        //public Vector3 BoneLoc() { return BoneLoc(SelectedBone); }
        public Vector3 BoneLoc(IBoneNode b) { return b == null ? new Vector3() : b.Matrix.GetPoint(); }
        public Vector3? VertexLoc()
        {
            if (_selectedVertices == null || _selectedVertices.Count == 0)
                return null;

            if (_vertexLoc != null)
                return _vertexLoc;

            Vector3 average = new Vector3();
            foreach (Vertex3 v in _selectedVertices)
                average += v.WeightedPosition;
            average /= _selectedVertices.Count;
            return _vertexLoc = average;
        }

        #region Overridable

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(true)]
        public virtual bool EditingAll
        {
            get;
            //{
            //    return (!(models.SelectedItem is IRenderedObject) && 
            //        models.SelectedItem != null && 
            //        models.SelectedItem.ToString() == "All");
            //}
            set;
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual KeyframePanel KeyframePanel { get { return null; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual BonesPanel BonesPanel { get { return null; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IBoneNode SelectedBone
        {
            get { return _selectedBone; }
            set
            {
                if (_selectedBone != null)
                    _selectedBone.BoneColor = _selectedBone.NodeColor = Color.Transparent;

                bool boneSelected = (_selectedBone = value) != null;
                if (boneSelected)
                {
                    _selectedBone.BoneColor = Color.FromArgb(0, 128, 255);
                    _selectedBone.NodeColor = Color.FromArgb(255, 128, 0);
                }

                //Check if the user selected a bone from another model.
                if (EditingAll && boneSelected && TargetModel != _selectedBone.IModel)
                {
                    _resetCamera = false;
                    TargetModel = _selectedBone.IModel;
                }

                if (BonesPanel != null)
                    BonesPanel.SetSelectedBone(_selectedBone);

                if (TargetAnimType == NW4RAnimType.CHR)
                    KeyframePanel.TargetSequence =
                        _chr0 != null && _selectedBone != null ?
                        _chr0.FindChild(_selectedBone.Name, false) :
                        null;

                OnSelectedBoneChanged();
                UpdatePropDisplay();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int CurrentFrame
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
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(false)]
        public virtual bool SyncVIS0 { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(false)]
        public virtual bool DisableBonesWhenPlaying { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(false)]
        public virtual bool DoNotHighlightOnMouseMove { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string ScreenCaptureFolder { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(".png")]
        public virtual string ScreenCaptureExtension { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(false)]
        public virtual bool InterpolationFormOpen { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(TransformType.Rotation)]
        public virtual TransformType ControlType { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(true)]
        public virtual bool PlayCHR0 { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(true)]
        public virtual bool PlaySRT0 { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(true)]
        public virtual bool PlayPAT0 { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(true)]
        public virtual bool PlayVIS0 { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(true)]
        public virtual bool PlayCLR0 { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(true)]
        public virtual bool PlaySCN0 { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(true)]
        public virtual bool PlaySHP0 { get; set; }
        
        #endregion
    }
}