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

namespace System.Windows.Forms
{
    public delegate void GLRenderEventHandler(object sender, TKContext ctx);

    public class ModelPanelSettings
    {
        public float _rotFactor = 0.1f;
        public float _transFactor = 0.05f;
        public float _zoomFactor = 2.5f;
        public int _zoomInit = 5;
        public int _yInit = 100;
        public float _viewDistance = 5.0f;
        public float _spotCutoff = 180.0f;
        public float _spotExponent = 100.0f;

        public Vector3 _defaultTranslate;
        public Vector2 _defaultRotate;
        
        public List<ResourceNode> _resourceList = new List<ResourceNode>();
        public List<IRenderedObject> _renderList = new List<IRenderedObject>();

        const float v = 100.0f / 255.0f;
        public Vector4 _position = new Vector4(100.0f, 45.0f, 45.0f, 1.0f);
        public Vector4 _ambient = new Vector4(v, v, v, 1.0f);
        public Vector4 _diffuse = new Vector4(v, v, v, 1.0f);
        public Vector4 _specular = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        public Vector4 _emission = new Vector4(v, v, v, 1.0f);
    }

    public unsafe class ModelPanel : GLPanel
    {
        public ModelPanelSettings _settings = new ModelPanelSettings();

        public bool _grabbing = false;
        public bool _scrolling = false;
        private int _lastX, _lastY;

        public float RotationScale { get { return _settings._rotFactor; } set { _settings._rotFactor = value; } }
        public float TranslationScale { get { return _settings._transFactor; } set { _settings._transFactor = value; } }
        public float ZoomScale { get { return _settings._zoomFactor; } set { _settings._zoomFactor = value; } }
        public int InitialZoomFactor { get { return _settings._zoomInit; } set { _settings._zoomInit = value; } }
        public int InitialYFactor { get { return _settings._yInit; } set { _settings._yInit = value; } }

        [TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 DefaultTranslate { get { return _settings._defaultTranslate; } set { _settings._defaultTranslate = value; } }
        [TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 DefaultRotate { get { return _settings._defaultRotate; } set { _settings._defaultRotate = value; } }

        public event GLRenderEventHandler PreRender, PostRender;

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
            _camera = new GLCamera();

            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ModelPanel_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ModelPanel_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ModelPanel_MouseUp);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _camera.Reset();
            _camera.Translate(_settings._defaultTranslate._x, _settings._defaultTranslate._y, _settings._defaultTranslate._z);
            _camera.Rotate(_settings._defaultRotate._x, _settings._defaultRotate._y);
        }

        public new bool Enabled { get { return _enabled; } set { _enabled = value; base.Enabled = value; } }
        private bool _enabled = true;

        public void ResetCamera()
        {
            _camera.Reset();
            _camera.Translate(DefaultTranslate._x, DefaultTranslate._y, DefaultTranslate._z);
            _camera.Rotate(DefaultRotate._x, DefaultRotate._y);
            Invalidate();
        }

        public void SetCamWithBox(Vector3 min, Vector3 max)
        {
            Vector3 average = new Vector3(
                (max._x + min._x) / 2.0f,
                (max._y + min._y) / 2.0f,
                (max._z + min._z) / 2.0f);

            float y = max._y - average._y;
            float x = max._x - average._x;
            float ratio = x / y;
            float tan = (float)Math.Tan((_fovY / 2.0f) * Maths._deg2radf);
            float distY = y / tan;
            float distX = distY * ratio;

            _camera.Reset();
            _camera.Translate(average._x, average._y, Maths.Max(distX, distY, max._z) + 3.0f);
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

            target.Attach(_ctx);

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
            foreach (IRenderedObject o in _settings._renderList)
                o.Refesh();
        }

        private float _multiplier = 1.0f;
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!Enabled)
                return;

            _scrolling = true;
            float z = (float)e.Delta / 120;
            if (Control.ModifierKeys == Keys.Shift)
                z *= 32;

            Zoom(-z * _settings._zoomFactor * _multiplier);

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

            if (e.Button == MouseButtons.Right)
                _grabbing = true;

            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _grabbing = false;
                Invalidate();
            }

            base.OnMouseUp(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!Enabled)
                return;

            int xDiff = e.X - _lastX;
            int yDiff = _lastY - e.Y;
            _lastX = e.X;
            _lastY = e.Y;

            Keys mod = Control.ModifierKeys;
            bool ctrl = (mod & Keys.Control) != 0;
            bool shift = (mod & Keys.Shift) != 0;
            bool alt = (mod & Keys.Alt) != 0;

            if (shift)
            {
                xDiff *= 16;
                yDiff *= 16;
            }

            if (_ctx != null)
            lock (_ctx)
                if (_grabbing)
                    if (ctrl)
                        if (alt)
                            Rotate(0, 0, -yDiff * RotationScale);
                        else
                            Rotate(yDiff * RotationScale, -xDiff * RotationScale);
                    else
                        Translate(-xDiff * TranslationScale, -yDiff * TranslationScale, 0.0f);

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
            if (_ortho)
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

            x *= _ortho ? 20.0f : 1.0f;
            y *= _ortho ? 20.0f : 1.0f;
            _camera.Translate(x, y, z);
            _scrolling = false;
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
            
            Vector4 PositionLight = new Vector4(r * (float)Math.Cos(azimuth) * (float)Math.Sin(elevation), r * (float)Math.Cos(elevation), r * (float)Math.Sin(azimuth) * (float)Math.Sin(elevation), 1);
            Vector4 SpotDirectionLight = new Vector4(-(float)Math.Cos(azimuth) * (float)Math.Sin(elevation), -(float)Math.Cos(elevation), -(float)Math.Sin(azimuth) * (float)Math.Sin(elevation), 1);
            
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

        public bool _enableSmoothing = false;
        internal unsafe override void OnInit(TKContext ctx)
        {
            Vector3 v = (Vector3)BackColor;
            GL.ClearColor(v._x, v._y, v._z, 0.0f);
            GL.ClearDepth(1.0f);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.ShadeModel(ShadingModel.Smooth);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Fastest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Fastest);
            GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Fastest);

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
        public bool _showCamCoords = false;
        protected internal override void OnRender(TKContext ctx, PaintEventArgs e)
        {
            if (_showCamCoords)
            {
                Vector3 v = _camera.GetPoint().Round(3);
                Vector3 r = _camera._rotation.Round(3);
                ScreenText[String.Format("Position\nX: {0}\nY: {1}\nZ: {2}\n\nRotation\nX: {3}\nY: {4}\nZ: {5}", v._x, v._y, v._z, r._x, r._y, r._z)] = new Vector3(5.0f, 5.0f, 0.5f);
            }

            if (_ctx._needsUpdate)
            {
                OnInit(ctx);
                OnResized();
                _ctx._needsUpdate = false;
            }

            if (_bgImage == null)
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            
            RecalcLight();

            if (PreRender != null)
                PreRender(this, ctx);

            foreach (IRenderedObject o in _settings._renderList)
                o.Render(ctx, this);

            if (PostRender != null)
                PostRender(this, ctx);
        }

        private void ModelPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !_forceNoSelection)
            {
                _selecting = true;
                _selStart = e.Location;
                _selEnd = e.Location;
            }
        }

        private void ModelPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _selEnd = e.Location;
                _selecting = false;
                Invalidate();
            }
        }

        private void ModelPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_selecting)
                _selEnd = e.Location;
        }
        public Bitmap GrabScreenshot(bool withTransparency)
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
    }
}
