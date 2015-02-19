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

namespace System.Windows.Forms
{
    public unsafe partial class TexCoordRenderer : GLPanel
    {
        public TexCoordRenderer()
        {
            InitializeComponent();
            _camera = new GLCamera();
        }

        public GLCamera _camera;
        public float _fovY = 45.0f, _nearZ = 1.0f, _farZ = 200000.0f, _aspect;
        public Matrix _projectionMatrix;
        public Matrix _projectionInverse;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ProjectionChanged { get { return _projectionChanged; } set { _projectionChanged = value; } }
        protected bool _projectionChanged = true;

        internal bool _updating = false;
        Vector2 _topLeft = new Vector2();
        Vector2 _bottomRight = new Vector2();
        List<ResourceNode> _attached;
        MDL0MaterialRefNode _targetMatRef = null;
        bool _remapPoints = false;
        float[] _points = new float[8];
        List<RenderInfo> _renderInfo;
        List<int> _uvSetIndices, _objIndices;
        public int _uvIndex = -1, _objIndex = -1;

        public BindingList<string> UVSetNames { get { return _uvSetNames; } }
        BindingList<string> _uvSetNames = new BindingList<string>();
        public BindingList<string> ObjectNames { get { return _objNames; } }
        BindingList<string> _objNames = new BindingList<string>();

        protected override void OnContextChanged(bool isNowCurrent)
        {
            if (isNowCurrent)
                UpdateProjection();

            base.OnContextChanged(isNowCurrent);
        }

        public MDL0MaterialRefNode TargetNode
        {
            get { return _targetMatRef; }
            set { SetTarget(value); }
        }

        private void SetTarget(MDL0MaterialRefNode texture)
        {
            _camera.Reset();

            if (_attached == null)
                _attached = new List<ResourceNode>();

            if (_targetMatRef != null)
                _targetMatRef.Unbind();
            _targetMatRef = texture;
            _attached.Clear();
            if (_targetMatRef == null)
                return;

            _attached.Add(_targetMatRef);
            _targetMatRef.Bind(-1);

            if (_renderInfo != null)
                foreach (RenderInfo info in _renderInfo)
                    if (info._renderBuffer != null)
                        info._renderBuffer.Dispose();

            _objIndices = new List<int>();
            _uvSetIndices = new List<int>();
            _renderInfo = new List<RenderInfo>();
            _objNames.Clear();
            _uvSetNames.Clear();

            int coordID = _targetMatRef.TextureCoordId;
            if (coordID < 0)
            {
                _objNames.Add("None");
                _uvSetNames.Add("None");
                Invalidate();
                return;
            }

            if (_targetMatRef.Material.Objects.Length > 1)
                _objNames.Add("All");

            foreach (MDL0ObjectNode obj in _targetMatRef.Material.Objects)
            {
                int x = 0;
                _objNames.Add(obj.Name);
                bool[] enabled = new bool[8];
                foreach (MDL0UVNode uv in obj._uvSet)
                {
                    if (uv != null)
                    {
                        int index = uv.Index;
                        if (!_uvSetIndices.Contains(index))
                            _uvSetIndices.Add(index);
                        enabled[x] = true;
                    }
                    x++;
                }
                _renderInfo.Add(new RenderInfo(this, obj._manager) { _enabled = enabled });
            }

            MDL0Node model = _targetMatRef.Model;
            _uvSetNames.Add(_uvSetIndices.Count == 1 ? model._uvList[_uvSetIndices[0]].Name : "All");
            if (model != null && model._uvList != null && _uvSetIndices.Count != 1)
                foreach (int i in _uvSetIndices)
                    _uvSetNames.Add(model._uvList[i].Name);

            //SetUVIndex(coordID);
            Invalidate();
        }

        public event EventHandler UVIndexChanged, ObjIndexChanged;

        public void SetUVIndex(int index)
        {
            if (index >= 0)
            {
                _uvIndex = index.Clamp(0, _uvSetNames.Count - 1);
                if (_objIndex >= 0)
                {
                    RenderInfo i = _renderInfo[_objIndex];
                    i._isEnabled = true;
                    i._enabled = new bool[8];
                    i._enabled[_uvIndex] = true;
                }
                else
                    foreach (RenderInfo i in _renderInfo)
                    {
                        i._isEnabled = true;
                        i._enabled = new bool[8];
                        i._enabled[_uvIndex] = true;
                    }
            }
            else
            {
                _uvIndex = -1;
                if (_objIndex >= 0)
                {
                    RenderInfo i = _renderInfo[_objIndex];
                    i._isEnabled = false;
                    i._enabled = new bool[8];
                    for (int x = 4; x < 12; x++)
                        if (i._manager._faceData[x] != null)
                        {
                            i._enabled[x - 4] = true;
                            i._isEnabled = true;
                        }
                }
                else
                    foreach (RenderInfo i in _renderInfo)
                    {
                        i._enabled = new bool[8];
                        i._isEnabled = false;
                        for (int x = 4; x < 12; x++)
                            if (i._manager._faceData[x] != null)
                            {
                                i._enabled[x - 4] = true;
                                i._isEnabled = true;
                            }
                    }
            }
            //if (UVIndexChanged != null)
            //    UVIndexChanged(this, EventArgs.Empty);
            Invalidate();
        }

        public void SetObjectIndex(int index)
        {
            if (index >= 0)
            {
                _objIndex = index.Clamp(0, _objNames.Count - 1);

                int u = 0;
                bool[] enabled = new bool[8];
                _uvSetIndices = new List<int>();
                MDL0ObjectNode obj = _targetMatRef.Material.Objects[index];
                foreach (MDL0UVNode o in obj._uvSet)
                {
                    if (o != null)
                    {
                        int r = o.Index;
                        if (!_uvSetIndices.Contains(r))
                            _uvSetIndices.Add(r);
                        enabled[u] = true;
                    }
                    u++;
                }

                int x = 0;
                foreach (RenderInfo i in _renderInfo)
                    if (i._isEnabled = (x++ == index))
                        i._enabled = enabled;
            }
            else
            {
                _objIndex = -1;
                foreach (MDL0ObjectNode obj in _targetMatRef.Material.Objects)
                    foreach (MDL0UVNode uv in obj._uvSet)
                        if (uv != null)
                        {
                            int c = uv.Index;
                            if (!_uvSetIndices.Contains(c))
                                _uvSetIndices.Add(c);
                        }

                foreach (RenderInfo i in _renderInfo)
                {
                    i._isEnabled = true;
                    i._enabled = new bool[8];
                    for (int x = 4; x < 12; x++)
                        if (i._manager._faceData[x] != null)
                            i._enabled[x - 4] = true;
                }
            }
            MDL0Node model = _targetMatRef.Model;
            string name = null;
            if (_uvSetNames.Count > 0 && _uvIndex >= 0 && _uvIndex < _uvSetNames.Count)
                name = _uvSetNames[_uvIndex];

            _uvSetNames.Clear();
            _uvSetNames.Add(_uvSetIndices.Count == 1 ? model._uvList[_uvSetIndices[0]].Name : "All");
            if (model != null && model._uvList != null && _uvSetIndices.Count != 1)
                foreach (int i in _uvSetIndices)
                    _uvSetNames.Add(model._uvList[i].Name);

            //if (ObjIndexChanged != null)
            //    ObjIndexChanged(this, EventArgs.Empty);

            SetUVIndex(name != null ? _uvSetNames.IndexOf(name) : -1);
        }

        unsafe internal override void OnInit(TKContext ctx)
        {
            //Set caps
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            _attached = new List<ResourceNode>();
            ctx._states["_Node_Refs"] = _attached;

            UpdateProjection();
        }

        protected void CalculateProjection()
        {
            _projectionMatrix = Matrix.OrthographicMatrix(-0.5f, 0.5f, -0.5f, 0.5f, -0.1f, 1.0f);
            _projectionInverse = Matrix.ReverseOrthographicMatrix(-0.5f, 0.5f, -0.5f, 0.5f, -0.1f, 1.0f);
        }

        protected void BeforeRender()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            {
                GL.LoadIdentity();
                Matrix p = Matrix.OrthographicMatrix(0, Width, 0, Height, -1, 1);
                GL.LoadMatrix((float*)&p);

                GL.MatrixMode(MatrixMode.Modelview);
                GL.PushMatrix();
                {
                    GL.LoadIdentity();

                    GL.Color4(Color.White);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

                    GL.Enable(EnableCap.Texture2D);

                    GLTexture bgTex = TKContext.FindOrCreate<GLTexture>("TexBG", GLTexturePanel.CreateBG);
                    bgTex.Bind();

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                    float* points = stackalloc float[8];
                    points[0] = points[1] = points[3] = points[6] = 0.0f;
                    points[2] = points[4] = Width;
                    points[5] = points[7] = Height;

                    float s = (float)Width / (float)bgTex.Width, t = (float)Height / (float)bgTex.Height;

                    GL.Begin(PrimitiveType.Quads);

                    GL.TexCoord2(0.0f, 0.0f);
                    GL.Vertex2(&points[0]);
                    GL.TexCoord2(s, 0.0f);
                    GL.Vertex2(&points[2]);
                    GL.TexCoord2(s, t);
                    GL.Vertex2(&points[4]);
                    GL.TexCoord2(0.0f, t);
                    GL.Vertex2(&points[6]);

                    GL.End();
                }
                GL.PopMatrix();
            }
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

        Vector2 _start, _scale;

        protected override void OnResize(EventArgs e)
        {
            _projectionChanged = true;
            if (_ctx != null)
                _ctx.Update();
            _aspect = (float)Width / Height;
            UpdateProjection();
            Invalidate();
        }

        public void UpdateProjection()
        {
            if (_ctx == null)
                return;
            CalculateProjection();
            Capture();
            GL.Viewport(ClientRectangle);
            GL.MatrixMode(MatrixMode.Projection);
            fixed (Matrix* p = &_projectionMatrix)
                GL.LoadMatrix((float*)p);
        }

        protected override void OnRender(PaintEventArgs e)
        {
            BeforeRender();

            //Set projection
            if (_projectionChanged)
            {
                UpdateProjection();
                _projectionChanged = false;
            }
            //Apply camera
            if (_camera != null)
                fixed (Matrix* p = &_camera._matrix)
                {
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadMatrix((float*)p);
                }

            GL.Color4(Color.White);

            if (_targetMatRef == null)
                return;
            _targetMatRef.Bind(-1);
            if (_targetMatRef._texture == null)
                return;
            GLTexture texture = _targetMatRef._texture.Texture;
            if (texture == null || texture._texId <= 0)
                return;

            float tAspect = (float)texture.Width / texture.Height;
            float wAspect = (float)Width / Height;

            if (tAspect > wAspect) //Texture is wider, use horizontal fit
            {
                //X values
                _points[0] = _points[6] = -0.5f;
                _points[2] = _points[4] = 0.5f;

                //Y values
                _points[1] = _points[3] = ((Height - ((float)Width / texture.Width * texture.Height))) / Height / 2.0f - 0.5f;
                _points[5] = _points[7] = -_points[1];
            }
            else
            {
                //Y values
                _points[1] = _points[3] = -0.5f;
                _points[5] = _points[7] = 0.5f;

                //X values
                _points[0] = _points[6] = (Width - ((float)Height / texture.Height * texture.Width)) / Width / 2.0f - 0.5f;
                _points[2] = _points[4] = -_points[0];
            }

            GL.BindTexture(TextureTarget.Texture2D, texture._texId);

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex2(_points[0], _points[1]);
            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex2(_points[2], _points[3]);
            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex2(_points[4], _points[5]);
            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex2(_points[6], _points[7]);

            GL.End();

            GL.Disable(EnableCap.Texture2D);
            GL.Color4(Color.LimeGreen);

            _topLeft = new Vector2(_points[0], _points[1]);
            _bottomRight = new Vector2(_points[4], _points[5]);

            _scale = new Vector2(_bottomRight._x - _topLeft._x, _bottomRight._y - _topLeft._y);
            _start = new Vector2(_topLeft._x, _topLeft._y);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.Translate(_start._x, _start._y, 0.0f);
            GL.Scale(_scale._x, _scale._y, 1.0f);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.LineWidth(1);

            foreach (RenderInfo info in _renderInfo)
                info.PrepareStream();
        }

        bool _grabbing = false;
        int _lastX = 0, _lastY = 0;
        float _multiplier = 1.0f;
        float _transFactor = 0.0025f;
        float _zoomFactor = 2.5f;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!Enabled)
                return;

            float z = (float)e.Delta / 120;
            Zoom(-z * _zoomFactor * _multiplier, e.X, e.Y);

            base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_ctx != null && _grabbing)
                lock (_ctx)
                {
                    int xDiff = e.X - _lastX;
                    int yDiff = e.Y - _lastY;

                    float w = 1, h = 1;

                    _transFactor = 0.005f;

                    if (_aspect > 1)
                        w = 1 / _aspect;
                    else if (_aspect < 1)
                        h = 1 / _aspect;

                    Translate(
                        -xDiff * _transFactor * w * _camera._scale._x,
                        -yDiff * _transFactor * h * _camera._scale._y,
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
            x *= _multiplier / _camera._scale._x;
            y *= _multiplier / _camera._scale._y;
            z *= _multiplier / _camera._scale._z;

            _camera.Translate(x, y, z);

            Invalidate();
        }

        private void Zoom(float amt, float originX, float originY)
        {
            //float w2 = Width / 2, h2 = Height / 2;

            //Translate((originX - w2) / w2, (originY - h2) / h2, 0);
            float scale = (amt >= 0 ? amt / 2.0f : 2.0f / -amt);
            Scale(scale, scale, 1.0f);
        }

        private void Scale(float x, float y, float z)
        {
            x *= _multiplier;
            y *= _multiplier;
            z *= _multiplier;

            _camera.Scale(x, y, z);
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

        private class RenderInfo
        {
            public RenderInfo(TexCoordRenderer renderer, PrimitiveManager manager)
            {
                _renderer = renderer;
                _manager = manager;
                for (int i = 0; i < 8; i++)
                    _enabled[i] = true;
                _isEnabled = true;
            }

            public bool _isEnabled;
            public int _stride;
            public PrimitiveManager _manager;
            public UnsafeBuffer _renderBuffer = null;
            public bool[] _dirty = new bool[8];
            public bool[] _enabled = new bool[8];
            TexCoordRenderer _renderer;

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

                if (bufferSize <= 0)
                    return;

                if (_renderBuffer == null)
                {
                    _renderBuffer = new UnsafeBuffer(bufferSize);
                    for (int i = 0; i < 8; i++)
                        _dirty[i] = true;
                }

                for (int i = 0; i < 8; i++)
                    if (_dirty[i])
                        UpdateStream(i);

                GL.EnableClientState(ArrayCap.VertexArray);

                byte* pData = (byte*)_renderBuffer.Address;
                for (int i = 4; i < _manager._faceData.Length; i++)
                    if (_manager._faceData[i] != null && _enabled[i - 4])
                    {
                        GL.VertexPointer(2, VertexPointerType.Float, _stride, (IntPtr)pData);
                        pData += 8;
                    }

                uint[] indices = _manager._triangles._indices;
                GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, indices);

                GL.DisableClientState(ArrayCap.VertexArray);
            }

            private unsafe void UpdateStream(int index)
            {
                _dirty[index] = false;

                if (_manager._faceData[index + 4] == null)
                    return;

                //Set starting address
                byte* pDst = (byte*)_renderBuffer.Address;
                for (int i = 0; i < index; i++)
                    if (_manager._faceData[i + 4] != null)
                        pDst += 8;

                Vector2* pSrc = (Vector2*)_manager._faceData[index + 4].Address;
                if (_renderer._remapPoints)
                    for (int i = 0; i < _manager._pointCount; i++, pDst += _stride)
                        *(Vector2*)pDst = (*pSrc++).RemapToRange(0.0f, 1.0f);
                else
                    for (int i = 0; i < _manager._pointCount; i++, pDst += _stride)
                        *(Vector2*)pDst = *pSrc++;
            }
        }
    }
}