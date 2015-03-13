using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrawlLib.OpenGL;
using OpenTK.Graphics.OpenGL;
using BrawlLib.Imaging;
using BrawlLib.Wii.Animations;
using BrawlLib.Modeling;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBBTypes;

namespace System.Windows.Forms
{
    public unsafe partial class TexCoordRenderer : GLPanel
    {
        public event EventHandler UVIndexChanged, ObjIndexChanged;

        public TexCoordRenderer()
        {
            InitializeComponent();
            CurrentViewport.ViewType = ViewportProjection.Front;
        }

        #region Variables

        internal bool _updating = false;

        private List<ResourceNode> _attached;
        private List<UVRenderInfo> _renderInfo;
        private List<int> _uvSetIndices, _objIndices;
        public int _uvIndex = -1, _objIndex = -1;

        private MDL0MaterialRefNode _targetMatRef = null;

        Vector2 _averageTexTranslation = new Vector2();

        //topLeft and bottomRight are used to match up the UV overlay to the texture underneath
        //These all must be recalculated when the window is resized
        public Vector2 _topLeft, _bottomRight;
        public float[] _texCoord = new float[8];
        public Vector2 _correct = new Vector2(1.0f);
        public float _width, _height, _halfW, _halfH;
        public int _xScale, _yScale;

        #endregion

        #region Properties

        public int UVWidth { get { return GLTex == null ? 0 : _xScale * GLTex.Width; } }
        public int UVHeight { get { return GLTex == null ? 0 : _yScale * GLTex.Height; } }

        public MDL0TextureNode Tex0 { get { return _targetMatRef == null ? null : _targetMatRef.TextureNode; } }
        public GLTexture GLTex { get { return Tex0 == null ? null : Tex0.Texture; } }
        
        public BindingList<string> UVSetNames { get { return _uvSetNames; } }
        private BindingList<string> _uvSetNames = new BindingList<string>();
        public BindingList<string> ObjectNames { get { return _objNames; } }
        private BindingList<string> _objNames = new BindingList<string>();

        public MDL0MaterialRefNode TargetNode
        {
            get { return _targetMatRef; }
            set { SetTarget(value); }
        }

        #endregion

        #region Functions

        private void SetTarget(MDL0MaterialRefNode texture)
        {
            if (_targetMatRef != texture && texture != null)
                CurrentViewport.Camera.Reset();

            if (_attached == null)
                _attached = new List<ResourceNode>();

            if (Tex0 != null)
                Tex0.Unbind();

            _attached.Clear();

            if (texture == null)
                return;

            _targetMatRef = texture;
            _attached.Add(_targetMatRef);

            if (Tex0 != null)
                Tex0.Prepare(_targetMatRef, -1);

            //Dispose of all old UV buffers
            if (_renderInfo != null)
                foreach (UVRenderInfo info in _renderInfo)
                    if (info._renderBuffer != null)
                        info._renderBuffer.Dispose();

            //Recreate lists
            _objIndices = new List<int>();
            _uvSetIndices = new List<int>();
            _renderInfo = new List<UVRenderInfo>();
            _objNames.Clear();
            _uvSetNames.Clear();

            int coordID = _targetMatRef.TextureCoordId;
            if (coordID < 0)
            {
                _objNames.Add("None");
                _uvSetNames.Add("None");
                CalcScaleTrans(coordID);
                ResizeData(Width, Height);
                Invalidate();
                return;
            }

            if (_targetMatRef.Material.Objects.Length > 1)
            {
                _objNames.Add("All");
                coordID = -1;
            }

            foreach (MDL0ObjectNode obj in _targetMatRef.Material.Objects)
            {
                _objNames.Add(obj.Name);
                UVRenderInfo info = new UVRenderInfo(obj._manager);
                _renderInfo.Add(info);
            }

            if (_objNames.Count == 0)
            {
                _objNames.Add("None");
                _uvSetNames.Add("None");
                CalcScaleTrans(coordID);
                ResizeData(Width, Height);
                Invalidate();
                return;
            }

            CalcScaleTrans(coordID);
            ResizeData(Width, Height);

            SetObjectIndex(-1);
            SetUVIndex(coordID);
        }

        public void SetUVIndex(int index)
        {
            _uvIndex = _uvSetNames.Count == 1 ? 0 : index >= 0 ? index.Clamp(0, _uvSetNames.Count - 2) : -1;
            if (UVIndexChanged != null)
                UVIndexChanged(this, EventArgs.Empty);
            UpdateDisplay();
        }

        public void SetObjectIndex(int index)
        {
            _uvSetIndices.Clear();
            if ((_objIndex = _objNames.Count == 1 ? 0 : index >= 0 ? index.Clamp(0, _objNames.Count - 2) : -1) >= 0)
            {
                foreach (MDL0UVNode uv in _targetMatRef.Material.Objects[_objIndex]._uvSet)
                    if (uv != null)
                        _uvSetIndices.Add(uv.Index);
            }
            else
            {
                foreach (MDL0ObjectNode obj in _targetMatRef.Material.Objects)
                    foreach (MDL0UVNode uv in obj._uvSet)
                        if (uv != null)
                            _uvSetIndices.Add(uv.Index);
            }

            if (_targetMatRef != null)
            {
                MDL0Node model = _targetMatRef.Model;
                string name = null;
                if (_uvSetNames.Count > 0 && _uvIndex >= 0 && _uvIndex < _uvSetNames.Count)
                    name = _uvSetNames[_uvIndex];

                _uvSetNames.Clear();
                _uvSetNames.Add(_uvSetIndices.Count == 1 ? model._uvList[_uvSetIndices[0]].Name : "All");
                if (model != null && model._uvList != null && _uvSetIndices.Count != 1)
                    foreach (int i in _uvSetIndices)
                        _uvSetNames.Add(model._uvList[i].Name);

                SetUVIndex(String.IsNullOrEmpty(name) ? _uvSetNames.IndexOf(name) : -1);
            }

            if (ObjIndexChanged != null)
                ObjIndexChanged(this, EventArgs.Empty);
        }

        public void UpdateDisplay()
        {
            bool singleObj = _objIndex >= 0;
            bool singleUV = _uvIndex >= 0;
            int r = 0;
            for (int i = 0; i < _renderInfo.Count; i++)
            {
                UVRenderInfo info = _renderInfo[i];
                info._dirty = true;
                if (singleObj)
                {
                    if (i != _objIndex)
                    {
                        info._isEnabled = false;
                        info._enabled = new bool[8];
                    }
                    else
                    {
                        info._isEnabled = true;
                        info._enabled = new bool[8];
                        for (int x = 0; x < 8; x++)
                            if (info._manager._faceData[x + 4] != null)
                                info._enabled[x] = !singleUV || _uvIndex == x;
                    }
                }
                else
                {
                    info._isEnabled = true;
                    info._enabled = new bool[8];
                    for (int x = 0; x < 8; x++)
                        if (info._manager._faceData[x + 4] != null)
                            info._enabled[x] = !singleUV || _uvIndex == r++;
                }
            }
            Invalidate();
        }

        #endregion

        #region MATH

        public void CalcScaleTrans(int coordID)
        {
            _averageTexTranslation = new Vector2();
            _xScale = 1;
            _yScale = 1;
            if (_renderInfo.Count == 0)
                return;

            //Find the nearest whole texture repetition to display all texture coordinates

            Vector2 min = new Vector2(float.MaxValue);
            Vector2 max = new Vector2(float.MinValue);
            foreach (UVRenderInfo info in _renderInfo)
            {
                if (coordID < 0)
                {
                    foreach (Vector2 v in info._minVals)
                        min.Min(v);
                    foreach (Vector2 v in info._maxVals)
                        max.Max(v);
                }
                else if (info._manager._faceData[coordID + 4] != null)
                {
                    min.Min(info._minVals[coordID]);
                    max.Max(info._maxVals[coordID]);
                }
            }

            //Get the ranges for each dimension
            Vector2 xMult = new Vector2(
                (decimal)min._x % 1.0m == 0.0m || min._x >= 0 ? (int)min._x : (int)min._x - 1,
                (decimal)max._x % 1.0m == 0.0m || max._x < 0 ? (int)max._x : (int)max._x + 1);

            Vector2 yMult = new Vector2(
                (decimal)min._y % 1.0m == 0.0m || min._y >= 0 ? (int)min._y : (int)min._y - 1,
                (decimal)max._y % 1.0m == 0.0m || max._y < 0 ? (int)max._y : (int)max._y + 1);

            //Get the number of texture repetitions to display
            _xScale = (int)(xMult._y - xMult._x);
            _yScale = (int)(yMult._y - yMult._x);

            //Change from [0, 1] to [-0.5, 0.5] to fit window range
            xMult -= 0.5f;
            yMult -= 0.5f;

            //Get the average translation
            _averageTexTranslation = new Vector2((xMult._y + xMult._x) / 2.0f, (yMult._y + yMult._x) / 2.0f);
        }

        public void ResizeData(float w, float h)
        {
            _width = w;
            _height = h;
            _halfW = w / 2.0f;
            _halfH = h / 2.0f;

            float
                texWidth = UVWidth,
                texHeight = UVHeight,
                tAspect = (float)UVWidth / (float)UVHeight,
                wAspect = w / h;

            //These are used to compensate for padding added on an axis
            _correct = new Vector2(1.0f);

            if (tAspect > wAspect)
            {
                //Texture is wider, use horizontal fit
                //X touches the edges of the window, Y has top and bottom padding

                //X
                _texCoord[0] = _texCoord[6] = 0.0f;
                _texCoord[2] = _texCoord[4] = 1.0f;

                //Y
                _texCoord[1] = _texCoord[3] = (_correct._y = tAspect / wAspect) / 2.0f + 0.5f;
                _texCoord[5] = _texCoord[7] = 1.0f - _texCoord[1];

                _bottomRight = new Vector2(_halfW, ((h - (w / texWidth * texHeight)) / h / 2.0f - 0.5f) * h);
                _topLeft = new Vector2(-_halfW, -_bottomRight._y);
            }
            else
            {
                //Window is wider, use vertical fit
                //Y touches the edges of the window, X has left and right padding

                //Y
                _texCoord[1] = _texCoord[3] = 1.0f;
                _texCoord[5] = _texCoord[7] = 0.0f;

                //X
                _texCoord[2] = _texCoord[4] = (_correct._x = wAspect / tAspect) / 2.0f + 0.5f;
                _texCoord[0] = _texCoord[6] = 1.0f - _texCoord[2];

                _bottomRight = new Vector2(1.0f - ((w - (h / texHeight * texWidth)) / w / 2.0f - 0.5f) * w, -_halfH);
                _topLeft = new Vector2(-_bottomRight._x, _halfH);
            }
        }

        void AlignTexture(GLCamera cam)
        {
            //The scale origin is the top left of the texture on the window (not of the window itself),
            //so we need to translate the center of the texture to that origin, 
            //scale the texture, then translate it back to where it was.
            OpenTK.Vector3 origin = new OpenTK.Vector3(-_topLeft._x / _width * _correct._x, _topLeft._y / _height * _correct._y, 0.0f);

            //First, apply window corrections to the texture

            //Translate to the average point before scaling the texture
            GL.Translate((OpenTK.Vector3)(Vector3)_averageTexTranslation);

            //Now scale the texture by the calculated values
            GL.Translate(origin);
            GL.Scale(_xScale, _yScale, 1.0f);
            GL.Translate(-origin);
            
            //Second, apply the texcoord bind transform
            TextureFrameState state = _targetMatRef._bindState;
            GL.MultMatrix((float*)&state._transform);

            //Lastly, apply the camera transform

            //Translate the texture coordinates to match where the user dragged the camera
            //Divide by width and height to convert window units (0 to w, 0 to h) to texcoord units (0 to 1)
            //Then multiply by the correction value if the window is bigger than the texture on an axis
            Vector3 point = cam.GetPoint();
            GL.Translate(point._x / _width * _correct._x, -point._y / _height * _correct._y, 0);

            //Scale the texture by the camera scale
            GL.Translate(origin);
            GL.Scale((OpenTK.Vector3)cam._scale);
            GL.Translate(-origin);
        }

        void AlignUVs()
        {
            GL.Translate(_averageTexTranslation._x / _correct._x * _width, _averageTexTranslation._y / _correct._y * _height, 0.0f);
            GL.Translate(_topLeft._x, _topLeft._y, 0.0f);
            GL.Scale((_bottomRight._x - _topLeft._x) / _xScale, (_bottomRight._y - _topLeft._y) / _yScale, 1.0f);
        }

        #endregion

        #region Rendering

        unsafe internal override void OnInit(TKContext ctx)
        {
            //Set caps
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            _attached = new List<ResourceNode>();
            ctx._states["_Node_Refs"] = _attached;
        }

        void RenderBackground()
        {
            GL.PushAttrib(AttribMask.TextureBit);

            GLTexture bgTex = TKContext.FindOrCreate<GLTexture>("TexBG", GLTexturePanel.CreateBG);
            bgTex.Bind();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            float
                s = (float)_width / (float)bgTex.Width,
                t = (float)_height / (float)bgTex.Height;

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex2(-_halfW, -_halfH);
            GL.TexCoord2(s, 0.0f);
            GL.Vertex2(_halfW, -_halfH);
            GL.TexCoord2(s, t);
            GL.Vertex2(_halfW, _halfH);
            GL.TexCoord2(0.0f, t);
            GL.Vertex2(-_halfW, _halfH);

            GL.End();

            GL.PopAttrib();
        }

        void RenderTexture(GLCamera cam)
        {
            if (Tex0 == null)
                return;
            Tex0.Prepare(_targetMatRef, -1);
            GLTexture texture = GLTex;
            if (texture == null || texture._texId <= 0)
                return;

            int filter = 0;
            switch (_targetMatRef.MagFilter)
            {
                case MatTextureMagFilter.Nearest:
                    filter = (int)TextureMagFilter.Nearest; break;
                case MatTextureMagFilter.Linear:
                    filter = (int)TextureMagFilter.Linear; break;
            }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, filter);
            switch (_targetMatRef.MinFilter)
            {
                case MatTextureMinFilter.Nearest:
                    filter = (int)TextureMinFilter.Nearest; break;
                case MatTextureMinFilter.Linear:
                    filter = (int)TextureMinFilter.Linear; break;
                case MatTextureMinFilter.Nearest_Mipmap_Nearest:
                    filter = (int)TextureMinFilter.NearestMipmapNearest; break;
                case MatTextureMinFilter.Nearest_Mipmap_Linear:
                    filter = (int)TextureMinFilter.NearestMipmapLinear; break;
                case MatTextureMinFilter.Linear_Mipmap_Nearest:
                    filter = (int)TextureMinFilter.LinearMipmapNearest; break;
                case MatTextureMinFilter.Linear_Mipmap_Linear:
                    filter = (int)TextureMinFilter.LinearMipmapLinear; break;
            }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, filter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, _targetMatRef.LODBias);

            switch ((int)_targetMatRef.UWrapMode)
            {
                case 0: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge); break;
                case 1: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); break;
                case 2: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat); break;
            }

            switch ((int)_targetMatRef.VWrapMode)
            {
                case 0: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge); break;
                case 1: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat); break;
                case 2: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat); break;
            }

            AlignTexture(cam);

            //Bind the material ref's texture
            GL.BindTexture(TextureTarget.Texture2D, texture._texId);

            //Draw a quad across the screen and render the texture with the calculated texcoords
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(_texCoord[0], _texCoord[1]);
            GL.Vertex2(-_halfW, -_halfH);
            GL.TexCoord2(_texCoord[2], _texCoord[3]);
            GL.Vertex2(_halfW, -_halfH);
            GL.TexCoord2(_texCoord[4], _texCoord[5]);
            GL.Vertex2(_halfW, _halfH);
            GL.TexCoord2(_texCoord[6], _texCoord[7]);
            GL.Vertex2(-_halfW, _halfH);

            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }

        public void Render(GLCamera cam, bool renderUVs, bool renderBG, bool renderTexture)
        {
            cam.LoadProjection();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Color4(Color.White);
            GL.Enable(EnableCap.Texture2D);
            //GL.Enable(EnableCap.LineSmooth);

            //Reset matrices
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Texture);
            GL.LoadIdentity();

            if (renderBG)
                RenderBackground();

            if (renderTexture)
                RenderTexture(cam);

            if (!renderUVs)
                return;

            //Now load the camera transform and draw the UV overlay over the texture
            cam.LoadModelView();

            //Color the lines limegreen, a bright color that probably won't be in a texture
            GL.Color4(Color.LimeGreen);

            AlignUVs();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.LineWidth(1.0f);

            //Render texture coordinates as vertex points
            foreach (UVRenderInfo info in _renderInfo)
                info.PrepareStream();
        }

        protected override void OnRender(PaintEventArgs e)
        {
            Rectangle r = CurrentViewport.Region;

            GL.Viewport(r);
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(r.X, r.Y, r.Width, r.Height);

            Render(CurrentViewport.Camera, true, true, true);

            GL.Disable(EnableCap.ScissorTest);
        }

        #endregion

        #region Mouse

        bool _grabbing = false;
        int _lastX = 0, _lastY = 0;
        float _transFactor = 0.05f;
        float _zoomFactor = 2.5f;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!Enabled)
                return;

            Zoom(-(float)e.Delta / 120.0f * _zoomFactor, e.X, e.Y);

            base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_ctx != null && _grabbing)
                lock (_ctx)
                {
                    int xDiff = e.X - _lastX;
                    int yDiff = e.Y - _lastY;

                    Translate(
                        -xDiff * _transFactor * 20.0f,
                        yDiff * _transFactor * 20.0f,
                        0.0f);
                }

            _lastX = e.X;
            _lastY = e.Y;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                _grabbing = true;

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && _grabbing)
            {
                _grabbing = false;
                Invalidate();
            }
        }

        private void Translate(float x, float y, float z)
        {
            CurrentViewport.Camera.Translate(x, y, z);
            Invalidate();
        }

        private void Zoom(float amt, float originX, float originY)
        {
            float scale = (amt >= 0 ? amt / 2.0f : 2.0f / -amt);
            Scale(scale, scale, scale);
        }

        private void Scale(float x, float y, float z)
        {
            CurrentViewport.Camera.Scale(x, y, z);
            Invalidate();
        }

        protected override bool ProcessKeyMessage(ref Message m)
        {
            if (!Enabled)
                return false;

            if (m.Msg == 0x100)
            {
                Keys mod = Control.ModifierKeys;
                bool ctrl = (mod & Keys.Control) != 0;
                bool shift = (mod & Keys.Shift) != 0;
                bool alt = (mod & Keys.Alt) != 0;
                switch ((Keys)m.WParam)
                {
                    case Keys.NumPad8:
                    case Keys.Up:
                        {
                            Translate(0.0f, -_transFactor * 8, 0.0f);
                            return true;
                        }
                    case Keys.NumPad2:
                    case Keys.Down:
                        {
                            Translate(0.0f, _transFactor * 8, 0.0f);
                            return true;
                        }
                    case Keys.NumPad6:
                    case Keys.Right:
                        {
                            Translate(_transFactor * 8, 0.0f, 0.0f);
                            return true;
                        }
                    case Keys.NumPad4:
                    case Keys.Left:
                        {
                            Translate(-_transFactor * 8, 0.0f, 0.0f);
                            return true;
                        }
                    case Keys.Add:
                    case Keys.Oemplus:
                        {
                            Zoom(-_zoomFactor, 0, 0);
                            return true;
                        }
                    case Keys.Subtract:
                    case Keys.OemMinus:
                        {
                            Zoom(_zoomFactor, 0, 0);
                            return true;
                        }
                }
            }

            return base.ProcessKeyMessage(ref m);
        }

        #endregion

        protected override void OnResize(EventArgs e)
        {
            ResizeData(Width, Height);
            base.OnResize(e);
        }

        private class UVRenderInfo
        {
            public UVRenderInfo(PrimitiveManager manager)
            {
                _manager = manager;
                for (int i = 0; i < 8; i++)
                    _enabled[i] = true;
                _isEnabled = true;
                CalcMinMax();
            }

            public bool _isEnabled;
            public int _stride;
            public PrimitiveManager _manager;
            public UnsafeBuffer _renderBuffer = null;
            public bool[] _enabled = new bool[8];
            public bool _dirty;

            public Vector2[] _minVals, _maxVals;
            public void CalcMinMax()
            {
                Vector2 min = new Vector2(float.MinValue);
                Vector2 max = new Vector2(float.MaxValue);
                _minVals = new Vector2[] { max, max, max, max, max, max, max, max };
                _maxVals = new Vector2[] { min, min, min, min, min, min, min, min };
                for (int i = 4; i < _manager._faceData.Length; i++)
                    if (_manager._faceData[i] != null && _enabled[i - 4])
                    {
                        Vector2* pSrc = (Vector2*)_manager._faceData[i].Address;
                        for (int x = 0; x < _manager._pointCount; x++, pSrc++)
                        {
                            _minVals[i - 4].Min(*pSrc);
                            _maxVals[i - 4].Max(*pSrc);
                        }
                    }
            }

            public void CalcStride()
            {
                _stride = 0;
                if (_isEnabled)
                    for (int i = 0; i < 8; i++)
                        if (_manager._faceData[i + 4] != null && _enabled[i])
                            _stride += 8;
            }

            public unsafe void PrepareStream()
            {
                if (!_isEnabled)
                    return;

                CalcStride();
                int bufferSize = _manager._pointCount * _stride;

                if (_renderBuffer != null && _renderBuffer.Length != bufferSize)
                {
                    _renderBuffer.Dispose();
                    _renderBuffer = null;
                }

                //Nothing to render if no buffer
                if (bufferSize <= 0)
                    return;

                if (_renderBuffer == null)
                {
                    _renderBuffer = new UnsafeBuffer(bufferSize);
                    _dirty = true;
                }

                if (_dirty)
                {
                    _dirty = false;
                    for (int i = 0; i < 8; i++)
                        UpdateStream(i);
                }

                GL.EnableClientState(ArrayCap.VertexArray);

                Vector2* pData = (Vector2*)_renderBuffer.Address;
                for (int i = 4; i < _manager._faceData.Length; i++)
                {
                    if (_manager._faceData[i] != null && _enabled[i - 4])
                    {
                        GL.VertexPointer(2, VertexPointerType.Float, _stride, (IntPtr)pData);
                        pData++;
                    }
                }

                uint[] indices = _manager._triangles._indices;
                GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, indices);

                GL.DisableClientState(ArrayCap.VertexArray);
            }

            private unsafe void UpdateStream(int index)
            {
                index += 4;
                if (_manager._faceData[index] == null || !_enabled[index - 4])
                    return;

                //Set starting address
                byte* pDst = (byte*)_renderBuffer.Address;
                for (int i = 4; i < index; i++)
                    if (_manager._faceData[i] != null && _enabled[i - 4])
                        pDst += 8;

                //Copy UVs
                Vector2* pSrc = (Vector2*)_manager._faceData[index].Address;
                for (int i = 0; i < _manager._pointCount; i++, pDst += _stride)
                    *(Vector2*)pDst = *pSrc++;
            }
        }
    }
}