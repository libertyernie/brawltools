using BrawlLib.Modeling;
using BrawlLib.OpenGL;
using BrawlLib.SSBB.ResourceNodes;
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
        protected const float _orbRadius = 1.0f;
        protected const float _circRadius = 1.2f;
        protected const float _axisSnapRange = 7.0f;
        protected const float _selectRange = 0.03f; //Selection error range for orb and circ
        protected const float _axisSelectRange = 0.15f; //Selection error range for axes
        protected const float _selectOrbScale = _selectRange / _orbRadius;
        protected const float _circOrbScale = _circRadius / _orbRadius;

        public int _animFrame = 0, _maxFrame;
        public bool _updating, _loop;
        public CoolTimer _timer;

        public CHR0Node _chr0;
        public SRT0Node _srt0;
        public SHP0Node _shp0;
        public PAT0Node _pat0;
        public VIS0Node _vis0;
        public SCN0Node _scn0;
        public CLR0Node _clr0;

        public MDL0MaterialRefNode _targetTexRef = null;
        public VIS0EntryNode _targetVisEntry;
        public List<IModel> _targetModels = new List<IModel>();
        public IModel _targetModel;
        public IBoneNode _selectedBone = null;
        public List<Vertex3> _selectedVertices = new List<Vertex3>();
        public List<HotKeyInfo> _hotkeyList;
        public List<ResourceNode> _animationSearchNodes = new List<ResourceNode>();
        public List<Image> images = new List<Image>();

        //Bone Name - Attached Polygon Indices
        public Dictionary<string, List<int>> VIS0Indices
        {
            get
            {
                if (_targetModel is MDL0Node) 
                    return ((MDL0Node)_targetModel).VIS0Indices;
                return null;
            }
        }

        protected NW4RAnimType _targetAnimType;

        protected Vector3 _lastPointLocal, _lastPointWorld;
        protected Vector3 _oldAngles, _oldPosition, _oldScale;
        protected Matrix _newVertexTransform;
        protected Vector3? _vertexLoc = null;

        protected bool _rotating, _translating, _scaling;
        protected bool _snapX, _snapY, _snapZ, _snapCirc;
        protected bool _hiX, _hiY, _hiZ, _hiCirc, _hiSphere;
        public bool _resetCamera = true;
        protected bool _enableTransform = true;
        protected bool _playing = false;
        protected bool _bonesWereOff = false;
        protected bool _renderLightDisplay = false;
        public bool _capture = false;

        public static Color _floorHue = Color.FromArgb(255, 128, 128, 191);

        public static BindingList<NW4RAnimType> _editableAnimTypes = new BindingList<NW4RAnimType>()
        {
            NW4RAnimType.CHR,
            NW4RAnimType.SRT,
            NW4RAnimType.SHP,
            NW4RAnimType.PAT,
            NW4RAnimType.VIS,
            NW4RAnimType.CLR,
            NW4RAnimType.SCN,
        };

        public ModelViewerForm _viewerForm = null;
        public InterpolationEditor _interpolationEditor;
        public InterpolationForm _interpolationForm = null;
        public Control _currentControl = null;
        protected OpenFileDialog dlgOpen = new OpenFileDialog();

        #region Events

        public event GLRenderEventHandler EventPostRender, EventPreRender;
        public event MouseEventHandler EventMouseDown, EventMouseMove, EventMouseUp;
        public event EventHandler TargetModelChanged, ModelViewerChanged;

        #endregion

        #region Delegates

        protected delegate void DelegateOpenFile(String s);
        protected DelegateOpenFile _openFileDelegate;

        #endregion

        public static readonly Type[] AnimTypeList = new Type[]
        {
            typeof(CHR0Node),
            typeof(SRT0Node),
            typeof(SHP0Node),
            typeof(PAT0Node),
            typeof(VIS0Node),
            typeof(CLR0Node),
            typeof(SCN0Node),
        };
    }

    public enum NW4RAnimType : int
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

    public enum J3DAnimType : int
    {
        None = -1,
        BCK = 0,
    }

    public enum TransformType
    {
        None = -1,
        Translation = 0,
        Rotation = 1,
        Scale = 2
    }
}