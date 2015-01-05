using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.OpenGL;
using System.ComponentModel;
using System.Drawing;
using BrawlLib.Modeling;
using BrawlLib.SSBB.ResourceNodes;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using BrawlLib.Imaging;
using System.IO;

namespace System.Windows.Forms
{
    public class ModelPanelSettings
    {
        public float _rotFactor = 0.4f;
        public float _transFactor = 0.05f;
        public float _zoomFactor = 2.5f;
        public float _viewDistance = 5.0f;
        public float _spotCutoff = 180.0f;
        public float _spotExponent = 100.0f;

        public Vector3 _defaultTranslate;
        public Vector3 _defaultRotate;
        
        public List<ResourceNode> _resourceList = new List<ResourceNode>();
        public List<IRenderedObject> _renderList = new List<IRenderedObject>();

        const float v = 100.0f / 255.0f;
        public Vector4 _position = new Vector4(100.0f, 45.0f, 45.0f, 1.0f);
        public Vector4 _ambient = new Vector4(v, v, v, 1.0f);
        public Vector4 _diffuse = new Vector4(v, v, v, 1.0f);
        public Vector4 _specular = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        public Vector4 _emission = new Vector4(v, v, v, 1.0f);

        public ModelRenderAttributes _renderAttrib = new ModelRenderAttributes();
        public bool _renderFloor;
    }

    public unsafe class ModelPanel : GLPanel
    {
        public List<ModelPanelSettings> _viewports = new List<ModelPanelSettings>();

        public ModelPanelSettings _settings = new ModelPanelSettings();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float RotationScale { get { return _settings._rotFactor; } set { _settings._rotFactor = value; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float TranslationScale { get { return _settings._transFactor; } set { _settings._transFactor = value; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ZoomScale { get { return _settings._zoomFactor; } set { _settings._zoomFactor = value; } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 DefaultTranslate { get { return _settings._defaultTranslate; } set { _settings._defaultTranslate = value; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector2StringConverter))]
        public Vector3 DefaultRotate { get { return _settings._defaultRotate; } set { _settings._defaultRotate = value; } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Selecting { get { return _selecting; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScreenTextHandler ScreenText { get { return _text; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Drawing.Point SelectionStart { get { return _selStart; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Drawing.Point SelectionEnd { get { return _selEnd; } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AllowSelection { get { return _allowSelection; } set { _allowSelection = value; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TextOverlaysEnabled { get { return _textEnabled; } set { _textEnabled = value; Invalidate(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BGImageType BackgroundImageType { get { return _bgType; } set { _bgType = value; Invalidate(); } }

        protected Drawing.Point _selStart, _selEnd;
        protected BGImageType _bgType = BGImageType.Stretch;
        protected GLTexture _bgImage = null;
        protected ScreenTextHandler _text;
        protected bool _bgColorChanged = false;
        protected bool _textEnabled = false;
        protected bool _allowSelection = false;
        protected bool _selecting = false;
        protected bool _updateImage = false;

        public bool _grabbing = false;
        public bool _scrolling = false;
        public bool _showCamCoords = false;
        public bool _enableSmoothing = false;

        private int _lastX, _lastY;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool Enabled { get { return _enabled; } set { _enabled = value; base.Enabled = value; } }
        private bool _enabled = true;
        private float _multiplier = 1.0f;

        public event GLRenderEventHandler PreRender, PostRender;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                if (base.BackColor != value)
                {
                    base.BackColor = Color.FromArgb(0, value.R, value.G, value.B);
                    _bgColorChanged = true;
                    Invalidate();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector4StringConverter))]
        public Vector4 Emission
        {
            get { return _settings._emission; }
            set
            {
                _settings._emission = value;
                Invalidate();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector4StringConverter))]
        public Vector4 Ambient
        {
            get { return _settings._ambient; }
            set
            {
                _settings._ambient = value;
                Invalidate();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector4StringConverter))]
        public Vector4 LightPosition
        {
            get { return _settings._position; }
            set 
            {
                _settings._position = value; 
                Invalidate(); 
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector4StringConverter))]
        public Vector4 Diffuse
        {
            get { return _settings._diffuse; }
            set
            {
                _settings._diffuse = value;
                Invalidate();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector4StringConverter))]
        public Vector4 Specular
        {
            get { return _settings._specular; }
            set
            {
                _settings._specular = value;
                Invalidate();
            }
        }

        public ModelPanel()
        {
            MDL0TextureNode._folderWatcher.Changed += _folderWatcher_Changed;
            MDL0TextureNode._folderWatcher.Created += _folderWatcher_Changed;
            MDL0TextureNode._folderWatcher.Deleted += _folderWatcher_Changed;
            MDL0TextureNode._folderWatcher.Renamed += _folderWatcher_Renamed;
            MDL0TextureNode._folderWatcher.Error += _folderWatcher_Error;
        }

        ~ModelPanel()
        {
            MDL0TextureNode._folderWatcher.Changed -= _folderWatcher_Changed;
            MDL0TextureNode._folderWatcher.Created -= _folderWatcher_Changed;
            MDL0TextureNode._folderWatcher.Deleted -= _folderWatcher_Changed;
            MDL0TextureNode._folderWatcher.Renamed -= _folderWatcher_Renamed;
            MDL0TextureNode._folderWatcher.Error -= _folderWatcher_Error;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Image BackgroundImage
        {
            get { return base.BackgroundImage; }
            set
            {
                if (base.BackgroundImage != null)
                    base.BackgroundImage.Dispose();

                base.BackgroundImage = value;

                _updateImage = true;

                Invalidate();
            }
        }

        public void RenderSelection()
        {
            if (_selecting)
            {
                GL.Enable(EnableCap.LineStipple);
                GL.LineStipple(1, 0x0F0F);
                GL.Color4(Color.Blue);
                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex2(_selStart.X, _selStart.Y);
                GL.Vertex2(_selEnd.X, _selStart.Y);
                GL.Vertex2(_selEnd.X, _selEnd.Y);
                GL.Vertex2(_selStart.X, _selEnd.Y);
                GL.End();
                GL.Disable(EnableCap.LineStipple);
            }
        }

        private void _folderWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            RefreshReferences();
        }
        void _folderWatcher_Error(object sender, ErrorEventArgs e)
        {
            RefreshReferences();
        }
        void _folderWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            RefreshReferences();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _text = new ScreenTextHandler(this);

            _camera.Reset();
            _camera.Translate(_settings._defaultTranslate._x, _settings._defaultTranslate._y, _settings._defaultTranslate._z);
            _camera.Rotate(_settings._defaultRotate._x, _settings._defaultRotate._y);
        }

        protected override void OnContextChanged(bool isNowCurrent)
        {
            base.OnContextChanged(isNowCurrent);

            MDL0TextureNode._folderWatcher.SynchronizingObject = isNowCurrent ? this : null;
        }

        public void ResetCamera()
        {
            _camera.Reset();
            _camera.Translate(DefaultTranslate._x, DefaultTranslate._y, DefaultTranslate._z);
            _camera.Rotate(DefaultRotate._x, DefaultRotate._y);
            Invalidate();
        }

        public void SetCamWithBox(Box value) { SetCamWithBox(value.Min, value.Max); }
        public void SetCamWithBox(Vector3 min, Vector3 max)
        {
            //Get the position of the midpoint of the bounding box plane closer to the camera
            Vector3 frontMidPt = new Vector3((max._x + min._x) / 2.0f, (max._y + min._y) / 2.0f, max._z);
            float tan = (float)Math.Tan(_fovY / 2.0f * Maths._deg2radf), distX = 0, distY = 0;

            //The tangent value would only be 0 if the FOV was 0,
            //meaning nothing would be visible anyway
            if (tan != 0)
            {
                //Calculate lengths
                Vector3 extents = max - min;
                Vector3 halfExtents = extents / 2.0f;
                float ratio = halfExtents._x / halfExtents._y;
                distY = halfExtents._y / tan; //The camera's distance from the model's midpoint in respect to Y
                distX = distY * ratio;
            }

            _camera.Reset();
            _camera.Translate(frontMidPt._x, frontMidPt._y, Maths.Max(distX, distY, max._z) + 2.0f);
            Invalidate();
        }

        public void ClearAll()
        {
            ClearTargets();
            ClearReferences();

            if (_ctx != null)
            {
                _ctx.Unbind();
                _ctx._states["_Node_Refs"] = _settings._resourceList;
            }
        }

        public void AddTarget(IRenderedObject target)
        {
            if (_settings._renderList.Contains(target))
                return;

            _settings._renderList.Add(target);

            if (target is ResourceNode)
                _settings._resourceList.Add(target as ResourceNode);

            target.Attach();

            Invalidate();
        }
        public void RemoveTarget(IRenderedObject target)
        {
            if (!_settings._renderList.Contains(target))
                return;

            target.Detach();

            if (target is ResourceNode)
                RemoveReference(target as ResourceNode);

            _settings._renderList.Remove(target);
        }
        public void ClearTargets()
        {
            for (int i = 0; i < _settings._renderList.Count; i++)
                _settings._renderList[i].Detach();
            _settings._renderList.Clear();
        }

        public void AddReference(ResourceNode node)
        {
            if (_settings._resourceList.Contains(node))
                return;

            _settings._resourceList.Add(node);
            RefreshReferences();
        }
        public void RemoveReference(ResourceNode node)
        {
            if (!_settings._resourceList.Contains(node))
                return;

            _settings._resourceList.Remove(node);
            RefreshReferences();
        }
        public void ClearReferences()
        {
            _settings._resourceList.Clear();
            RefreshReferences();
        }
        public void RefreshReferences()
        {
            if (InvokeRequired)
            {
                Invoke(new NoArgsDelegate(RefreshReferences));
                return;
            }

            foreach (IRenderedObject o in _settings._renderList)
                o.Refresh();

            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!Enabled)
                return;
            
            float z = (float)e.Delta / 120;
            if (Control.ModifierKeys == Keys.Shift)
                z *= 32;

            _scrolling = true;
            Zoom(-z * _settings._zoomFactor);
            _scrolling = false;

            if (Control.ModifierKeys == Keys.Alt)
                if (z < 0)
                    _multiplier /= 0.9f;
                else
                    _multiplier *= 0.9f;

            base.OnMouseWheel(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!Enabled)
                return;

            if (e.Button == MouseButtons.Left)
            {
                if (_allowSelection && !_selecting)
                {
                    _selecting = true;
                    _selStart = e.Location;
                    _selEnd = e.Location;
                    _shiftSelecting = ModifierKeys == Keys.ShiftKey || ModifierKeys == Keys.Shift;
                }
                else if (_selecting && _shiftSelecting)
                {
                    _selecting = false;
                    _selEnd = e.Location;
                    _shiftSelecting = false;
                }
            }

            if (e.Button == MouseButtons.Right)
                _grabbing = true;
            else
                if (e.Button == Forms.MouseButtons.Middle)
                    _scrolling = true;

            base.OnMouseDown(e);
        }

        bool _shiftSelecting;

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && 
                _selecting && 
                !(ModifierKeys == Keys.ShiftKey || ModifierKeys == Keys.Shift || _shiftSelecting))
            {
                _selEnd = e.Location;
                _selecting = false;
            }

            if (e.Button == MouseButtons.Right)
                _grabbing = false;

            if (e.Button == MouseButtons.Middle)
                _scrolling = false;

            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)
                Invalidate();

            base.OnMouseUp(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!Enabled)
                return;

            if (_selecting)
                _selEnd = e.Location;

            if (_ctx != null && (_grabbing || _scrolling))
                lock (_ctx)
                {
                    int xDiff = e.X - _lastX;
                    int yDiff = _lastY - e.Y;

                    Keys mod = Control.ModifierKeys;
                    bool ctrl = (mod & Keys.Control) != 0;
                    bool shift = (mod & Keys.Shift) != 0;
                    bool alt = (mod & Keys.Alt) != 0;

                    if (shift)
                    {
                        xDiff *= 16;
                        yDiff *= 16;
                    }

                    if (_scrolling)
                        Translate(0, 0, (float)yDiff * 0.01f);
                    else if (ctrl)
                        if (alt)
                            Rotate(0, 0, -yDiff * RotationScale);
                        else
                            Rotate(yDiff * RotationScale, -xDiff * RotationScale);
                    else
                        Translate(-xDiff * TranslationScale, -yDiff * TranslationScale, 0.0f);
                }

            _lastX = e.X;
            _lastY = e.Y;

            if (_selecting)
                Invalidate();

            base.OnMouseMove(e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            keyData &= (Keys)0xFFFF;
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    return true;

                default:
                    return base.IsInputKey(keyData);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if ((e.KeyData == Keys.ShiftKey || e.KeyData == Keys.Shift) && _shiftSelecting)
            {
                _selecting = false;
                _shiftSelecting = false;
                Invalidate();
            }
        }

        public delegate bool KeyMessageEventHandler(ref Message m);
        public KeyMessageEventHandler EventProcessKeyMessage;
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
                    case Keys.Shift:
                    case Keys.ShiftKey:
                        if (_selecting)
                            _shiftSelecting = true;
                        break;

                    case Keys.NumPad8:
                    case Keys.Up:
                        {
                            if (alt)
                                break;
                            if (ctrl)
                                Rotate(-RotationScale * (shift ? 32 : 4), 0.0f);
                            else
                                Translate(0.0f, TranslationScale * (shift ? 128 : 8), 0.0f);
                            return true;
                        }
                    case Keys.NumPad2:
                    case Keys.Down:
                        {
                            if (alt)
                                break;
                            if (ctrl)
                                Rotate(RotationScale * (shift ? 32 : 4), 0.0f);
                            else
                                Translate(0.0f, -TranslationScale * (shift ? 128 : 8), 0.0f);
                            return true;
                        }
                    case Keys.NumPad6:
                    case Keys.Right:
                        {
                            if (alt)
                                break;
                            if (ctrl)
                                Rotate(0.0f, RotationScale * (shift ? 32 : 4));
                            else
                                Translate(TranslationScale * (shift ? 128 : 8), 0.0f, 0.0f);
                            return true;
                        }
                    case Keys.NumPad4:
                    case Keys.Left:
                        {
                            if (alt)
                                break;
                            if (ctrl)
                                Rotate(0.0f, -RotationScale * (shift ? 32 : 4));
                            else
                                Translate(-TranslationScale * (shift ? 128 : 8), 0.0f, 0.0f);
                            return true;
                        }
                    case Keys.Add:
                    case Keys.Oemplus:
                        {
                            if (alt) break;
                            Zoom(-ZoomScale * (shift ? 32 : 2));
                            return true;
                        }
                    case Keys.Subtract:
                    case Keys.OemMinus:
                        {
                            if (alt) break;
                            Zoom(ZoomScale * (shift ? 32 : 2));
                            return true;
                        }
                }
            }

            if (EventProcessKeyMessage != null)
                EventProcessKeyMessage(ref m);

            return base.ProcessKeyMessage(ref m);
        }
        private void Zoom(float amt)
        {
            amt *= _multiplier;
            if (_orthographic)
            {
                float scale = (amt >= 0 ? amt / 2.0f : 2.0f / -amt);
                Scale(scale, scale, 1.0f);
            }
            else
                Translate(0.0f, 0.0f, amt);
        }
        private void Scale(float x, float y, float z)
        {
            x *= _multiplier;
            y *= _multiplier;
            z *= _multiplier;

            _camera.Scale(x, y, z);
            _scrolling = false;
            Invalidate();
        }
        private void Translate(float x, float y, float z)
        {
            x *= _multiplier;
            y *= _multiplier;
            z *= _multiplier;

            x *= _orthographic ? 20.0f : 1.0f;
            y *= _orthographic ? 20.0f : 1.0f;
            _camera.Translate(x, y, z);
            //_scrolling = false;
            Invalidate();
        }
        private void Rotate(float x, float y)
        {
            x *= _multiplier;
            y *= _multiplier;

            _camera.Pivot(_settings._viewDistance, x, y);
            Invalidate();
        }
        private void Rotate(float x, float y, float z)
        {
            x *= _multiplier;
            y *= _multiplier;
            z *= _multiplier;

            _camera.Rotate(x, y, z);
            Invalidate();
        }

        //Call this every time the scene is rendered
        //Otherwise the light will move with the camera
        //(Which makes sense, since the camera isn't moving and the scene is)
        public void RecalcLight()
        {
            GL.Light(LightName.Light0, LightParameter.SpotCutoff, _settings._spotCutoff);
            GL.Light(LightName.Light0, LightParameter.SpotExponent, _settings._spotExponent);

            float r = _settings._position._x;
            float azimuth = _settings._position._y * Maths._deg2radf;
            float elevation = 360.0f - (_settings._position._z * Maths._deg2radf);

            float
                cosElev = (float)Math.Cos(elevation),
                sinElev = (float)Math.Sin(elevation),
                cosAzi = (float)Math.Cos(azimuth),
                sinAzi = (float)Math.Sin(azimuth);

            Vector4 PositionLight = new Vector4(r * cosAzi * sinElev, r * cosElev, r * sinAzi * sinElev, 1);
            Vector4 SpotDirectionLight = new Vector4(-cosAzi * sinElev, -cosElev, -sinAzi * sinElev, 1);
            
            GL.Light(LightName.Light0, LightParameter.Position, (float*)&PositionLight);
            GL.Light(LightName.Light0, LightParameter.SpotDirection, (float*)&SpotDirectionLight);

            fixed (Vector4* pos = &_settings._ambient)
            {
                GL.Light(LightName.Light0, LightParameter.Ambient, (float*)pos);
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, (float*)pos);
            }
            fixed (Vector4* pos = &_settings._diffuse)
            {
                GL.Light(LightName.Light0, LightParameter.Diffuse, (float*)pos);
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, (float*)pos);
            }
            fixed (Vector4* pos = &_settings._specular)
            {
                GL.Light(LightName.Light0, LightParameter.Specular, (float*)pos);
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, (float*)pos);
            }
            fixed (Vector4* pos = &_settings._emission)
            {
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, (float*)pos);
            }

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

            if (_enableSmoothing)
            {
                GL.Enable(EnableCap.PointSmooth);
                GL.Enable(EnableCap.PolygonSmooth);
                GL.Enable(EnableCap.LineSmooth);
                GL.PointSize(1.0f);
                GL.LineWidth(1.0f);
            }
            else
            {
                GL.Disable(EnableCap.PointSmooth);
                GL.Disable(EnableCap.PolygonSmooth);
                GL.Disable(EnableCap.LineSmooth);
                GL.PointSize(3.0f);
                GL.LineWidth(2.0f);
            }
        }

        internal unsafe override void OnInit(TKContext ctx)
        {
            Vector3 v = (Vector3)BackColor;
            GL.ClearColor(v._x, v._y, v._z, 0.0f);
            GL.ClearDepth(1.0);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.ShadeModel(ShadingModel.Smooth);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Nicest);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);

            //GL.PointSize(3.0f);
            //GL.LineWidth(2.0f);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            //GL.Enable(EnableCap.Normalize);

            RecalcLight();

            //Set client states
            ctx._states["_Node_Refs"] = _settings._resourceList;
        }
        protected internal override void OnRender(PaintEventArgs e)
        {
            BeforeRender();

            if (_showCamCoords)
            {
                Vector3 v = _camera.GetPoint().Round(3);
                Vector3 r = _camera._rotation.Round(3);
                ScreenText[String.Format("Position\nX: {0}\nY: {1}\nZ: {2}\n\nRotation\nX: {3}\nY: {4}\nZ: {5}", v._x, v._y, v._z, r._x, r._y, r._z)] = new Vector3(5.0f, 5.0f, 0.5f);
            }

            if (_bgImage == null)
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            RecalcLight();

            if (PreRender != null)
                PreRender(this);

            foreach (IRenderedObject o in _settings._renderList)
                o.Render(_settings._renderAttrib);

            if (PostRender != null)
                PostRender(this);

            AfterRender();
        }

        protected void AfterRender()
        {
            //Render selection overlay and/or text overlays
            if ((_selecting && _allowSelection) || _text.Count != 0)
            {
                GL.Color4(Color.White);

                GL.PushAttrib(AttribMask.AllAttribBits);
                {
                    GL.Disable(EnableCap.DepthTest);
                    GL.Disable(EnableCap.Lighting);
                    GL.Disable(EnableCap.CullFace);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

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

                            if (_text.Count != 0 && _textEnabled)
                                _text.Draw();

                            if (_selecting && _allowSelection)
                                RenderSelection();
                        }
                        GL.PopMatrix();
                    }
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.PopMatrix();
                }
                GL.PopAttrib();

                //Clear text values
                //This will be filled until the next render
                _text.Clear();
            }
        }

        protected void BeforeRender()
        {
            //Render background image
            if (BackgroundImage != null)
                RenderBackground();
            else if (_updateImage && _bgImage != null)
            {
                _bgImage.Delete();
                _bgImage = null;
                _updateImage = false;
            }

            if (_bgColorChanged)
            {
                Vector3 v = (Vector3)BackColor;
                GL.ClearColor(v._x, v._y, v._z, 0.0f);
                _bgColorChanged = false;
            }
        }

        private void RenderBackground()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.CullFace);

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

                    if (_updateImage)
                    {
                        if (_bgImage != null)
                        {
                            _bgImage.Delete();
                            _bgImage = null;
                        }

                        GL.ClearColor(Color.Black);

                        Bitmap bmp = BackgroundImage as Bitmap;

                        _bgImage = new GLTexture(bmp);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);
                        _bgImage.Bind();

                        _updateImage = false;
                    }
                    else
                        GL.BindTexture(TextureTarget.Texture2D, _bgImage._texId);

                    float* points = stackalloc float[8];
                    float tAspect = (float)_bgImage.Width / _bgImage.Height;
                    float wAspect = (float)Width / Height;

                    switch (_bgType)
                    {
                        case BGImageType.Stretch:

                            points[0] = points[1] = points[3] = points[6] = 0.0f;
                            points[2] = points[4] = Width;
                            points[5] = points[7] = Height;

                            break;

                        case BGImageType.Center:

                            if (tAspect > wAspect)
                            {
                                points[1] = points[3] = 0.0f;
                                points[5] = points[7] = Height;

                                points[0] = points[6] = Width * ((Width - ((float)Height / _bgImage.Height * _bgImage.Width)) / Width / 2.0f);
                                points[2] = points[4] = Width - points[0];
                            }
                            else
                            {
                                points[0] = points[6] = 0.0f;
                                points[2] = points[4] = Width;

                                points[1] = points[3] = Height * (((Height - ((float)Width / _bgImage.Width * _bgImage.Height))) / Height / 2.0f);
                                points[5] = points[7] = Height - points[1];
                            }
                            break;

                        case BGImageType.ResizeWithBars:

                            if (tAspect > wAspect)
                            {
                                points[0] = points[6] = 0.0f;
                                points[2] = points[4] = Width;

                                points[1] = points[3] = Height * (((Height - ((float)Width / _bgImage.Width * _bgImage.Height))) / Height / 2.0f);
                                points[5] = points[7] = Height - points[1];
                            }
                            else
                            {
                                points[1] = points[3] = 0.0f;
                                points[5] = points[7] = Height;

                                points[0] = points[6] = Width * ((Width - ((float)Height / _bgImage.Height * _bgImage.Width)) / Width / 2.0f);
                                points[2] = points[4] = Width - points[0];
                            }

                            break;
                    }

                    GL.Begin(PrimitiveType.Quads);

                    GL.TexCoord2(0.0f, 0.0f);
                    GL.Vertex2(&points[0]);
                    GL.TexCoord2(1.0f, 0.0f);
                    GL.Vertex2(&points[2]);
                    GL.TexCoord2(1.0f, 1.0f);
                    GL.Vertex2(&points[4]);
                    GL.TexCoord2(0.0f, 1.0f);
                    GL.Vertex2(&points[6]);

                    GL.End();

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)MatTextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)MatTextureMagFilter.Nearest);

                    GL.Disable(EnableCap.Texture2D);
                    GL.Enable(EnableCap.DepthTest);
                    GL.Enable(EnableCap.Lighting);
                }
                GL.PopMatrix();
            }
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

        public Bitmap GetScreenshot(bool withTransparency)
        {
            Bitmap bmp = new Bitmap(ClientSize.Width, ClientSize.Height);
            BitmapData data;
            if (withTransparency)
            {
                data = bmp.LockBits(ClientRectangle, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.ReadPixels(0, 0, ClientSize.Width, ClientSize.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            }
            else
            {
                data = bmp.LockBits(this.ClientRectangle, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                GL.ReadPixels(0, 0, ClientSize.Width, ClientSize.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            }
            bmp.UnlockBits(data);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }

        public delegate void RenderStateEvent(ModelPanel panel, bool value);
        public event RenderStateEvent
            RenderFloorChanged,
            RenderBonesChanged,
            RenderModelBoxChanged,
            RenderObjectBoxChanged,
            RenderVisBoneBoxChanged,
            RenderOffscreenChanged,
            RenderVerticesChanged,
            RenderNormalsChanged,
            RenderPolygonsChanged,
            RenderWireframeChanged,
            UseBindStateBoxesChanged,
            ApplyBillboardBonesChanged;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderFloor
        {
            get { return _settings._renderFloor; }
            set
            {
                _settings._renderFloor = value;

                Invalidate();

                if (RenderFloorChanged != null)
                    RenderFloorChanged(this, value);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderBones
        {
            get { return _settings._renderAttrib._renderBones; }
            set
            {
                _settings._renderAttrib._renderBones = value;

                Invalidate();

                if (RenderBonesChanged != null)
                    RenderBonesChanged(this, value);
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderVertices
        {
            get { return _settings._renderAttrib._renderVertices; }
            set
            {
                _settings._renderAttrib._renderVertices = value;

                Invalidate();

                if (RenderVerticesChanged != null)
                    RenderVerticesChanged(this, value);
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderNormals
        {
            get { return _settings._renderAttrib._renderNormals; }
            set
            {
                _settings._renderAttrib._renderNormals = value;

                Invalidate();

                if (RenderNormalsChanged != null)
                    RenderNormalsChanged(this, value);
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderPolygons
        {
            get { return _settings._renderAttrib._renderPolygons; }
            set
            {
                _settings._renderAttrib._renderPolygons = value;

                Invalidate();

                if (RenderPolygonsChanged != null)
                    RenderPolygonsChanged(this, value);
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderWireframe
        {
            get { return _settings._renderAttrib._renderWireframe; }
            set
            {
                _settings._renderAttrib._renderWireframe = value;

                Invalidate();

                if (RenderWireframeChanged != null)
                    RenderWireframeChanged(this, value);
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderModelBox
        {
            get { return _settings._renderAttrib._renderModelBox; }
            set
            {
                _settings._renderAttrib._renderModelBox = value;

                Invalidate();

                if (RenderModelBoxChanged != null)
                    RenderModelBoxChanged(this, value);
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderObjectBox
        {
            get { return _settings._renderAttrib._renderObjectBoxes; }
            set
            {
                _settings._renderAttrib._renderObjectBoxes = value;

                Invalidate();

                if (RenderObjectBoxChanged != null)
                    RenderObjectBoxChanged(this, value);
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderVisBoneBox
        {
            get { return _settings._renderAttrib._renderBoneBoxes; }
            set
            {
                _settings._renderAttrib._renderBoneBoxes = value;

                Invalidate();

                if (RenderVisBoneBoxChanged != null)
                    RenderVisBoneBoxChanged(this, value);
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseBindStateBoxes
        {
            get { return _settings._renderAttrib._useBindStateBoxes; }
            set
            {
                _settings._renderAttrib._useBindStateBoxes = value;

                Invalidate();

                if (UseBindStateBoxesChanged != null)
                    UseBindStateBoxesChanged(this, value);
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DontRenderOffscreen
        {
            get { return _settings._renderAttrib._dontRenderOffscreen; }
            set
            {
                _settings._renderAttrib._dontRenderOffscreen = value;

                Invalidate();

                if (RenderOffscreenChanged != null)
                    RenderOffscreenChanged(this, value);
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ApplyBillboardBones
        {
            get { return _settings._renderAttrib._applyBillboardBones; }
            set
            {
                _settings._renderAttrib._applyBillboardBones = value;

                Invalidate();

                if (ApplyBillboardBonesChanged != null)
                    ApplyBillboardBonesChanged(this, value);
            }
        }

        public static List<IModel> CollectModels(ResourceNode node)
        {
            List<IModel> models = new List<IModel>();
            GetModelsRecursive(node, models);
            return models;
        }

        private static void GetModelsRecursive(ResourceNode node, List<IModel> models)
        {
            switch (node.ResourceType)
            {
                case ResourceType.ARC:
                case ResourceType.RARC:
                case ResourceType.RARCFolder:
                case ResourceType.MRG:
                case ResourceType.BRES:
                case ResourceType.BRESGroup:
                case ResourceType.U8:
                case ResourceType.U8Folder:
                    foreach (ResourceNode n in node.Children)
                        GetModelsRecursive(n, models);
                    break;
                case ResourceType.MDL0:
                case ResourceType.BMD:
                    models.Add((IModel)node);
                    break;
            }
        }
    }

    public class ScreenTextHandler
    {
        private class TextData
        {
            internal string _string;
            internal List<Vector3> _positions;

            internal TextData() { _positions = new List<Vector3>(); }
        }
        public static int _fontSize = 12;
        private static readonly Font _textFont = new Font("Arial", _fontSize);
        private GLPanel _panel;
        private Dictionary<string, TextData> _text = new Dictionary<string, TextData>();
        public int Count { get { return _text.Count; } }

        private Drawing.Size _size = new Drawing.Size();
        private Bitmap _bitmap = null;
        private int _texId = -1;

        public Vector3 this[string text]
        {
            set
            {
                if (!_text.ContainsKey(text))
                    _text.Add(text, new TextData() { _string = text });

                _text[text]._positions.Add(value);
            }
        }

        public ScreenTextHandler(GLPanel p)
        {
            _text = new Dictionary<string, TextData>();
            _panel = p;
        }

        public void Clear() { _text.Clear(); }

        public unsafe void Draw()
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcColor);

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Replace);

            if (_size != _panel.ClientSize)
            {
                _size = _panel.ClientSize;

                if (_bitmap != null)
                    _bitmap.Dispose();
                if (_texId != -1)
                {
                    GL.DeleteTexture(_texId);
                    _texId = -1;
                }

                //Create a texture over the whole model panel
                _bitmap = new Bitmap(_size.Width, _size.Height);

                _bitmap.MakeTransparent();

                _texId = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, _texId);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _bitmap.Width, _bitmap.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            }

            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                g.Clear(Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                List<Vector2> _used = new List<Vector2>();

                foreach (TextData d in _text.Values)
                    foreach (Vector3 v in d._positions)
                        if (v._x + d._string.Length * 10 > 0 && v._x < _panel.Width &&
                            v._y > -10.0f && v._y < _panel.Height &&
                            v._z > 0 && v._z < 1 && //near and far depth values
                            !_used.Contains(new Vector2(v._x, v._y)))
                        {
                            g.DrawString(d._string, ScreenTextHandler._textFont, Brushes.Black, new PointF(v._x, v._y));
                            _used.Add(new Vector2(v._x, v._y));
                        }
            }

            GL.BindTexture(TextureTarget.Texture2D, _texId);

            BitmapData data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
            ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, _bitmap.Width, _bitmap.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            _bitmap.UnlockBits(data);

            //BitmapData data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _size.Width, _size.Height, 0,
            //    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            //_bitmap.UnlockBits(data);

            //GL.Color4(Color.Transparent);

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex2(0.0f, 0.0f);

            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex2(_size.Width, 0.0f);

            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex2(_size.Width, _size.Height);

            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex2(0.0f, _size.Height);

            GL.End();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
        }
    }
}
