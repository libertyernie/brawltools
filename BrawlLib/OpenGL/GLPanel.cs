using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using OpenTK.Platform;
using OpenTK.Graphics.OpenGL;
using BrawlLib.SSBB.ResourceNodes;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using System.ComponentModel;

namespace BrawlLib.OpenGL
{
    public static class ControlExtension
    {
        [ReflectionPermission(SecurityAction.Demand, MemberAccess = true)]
        public static void Reset(this Control c)
        {
            typeof(Control).InvokeMember("SetState", BindingFlags.NonPublic |
            BindingFlags.InvokeMethod | BindingFlags.Instance, null,
            c, new object[] { 0x400000, false });
        }
    }
    public abstract unsafe class GLPanel : UserControl
    {
        public TKContext _ctx;
        
        public bool _projectionChanged = true;
        private int _updateCounter;
        public GLCamera _camera;
        
        public GLPanel()
        {
            SetStyle(
                ControlStyles.UserPaint | 
                ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.Opaque | 
                ControlStyles.ResizeRedraw,
                true);
        }
        protected override void Dispose(bool disposing)
        {
            DisposeContext();
            base.Dispose(disposing);
        }
        private void DisposeContext()
        {
            if (_ctx != null)
            {
                _ctx.Unbind();
                _ctx.Dispose();
                _ctx = null;
            }
        }

        public void BeginUpdate() { _updateCounter++; }
        public void EndUpdate() { if ((_updateCounter = Math.Max(_updateCounter - 1, 0)) == 0) Invalidate(); }

        public new void Capture() { if (_ctx != null) _ctx.Capture(); }
        public void Release() { if (_ctx != null) _ctx.Release(); }

        protected override void OnLoad(EventArgs e)
        {
            _ctx = new TKContext(this);
            _text = new ScreenTextHandler(this);

            Vector3 v = (Vector3)BackColor;
            GL.ClearColor(v._x, v._y, v._z, 0.0f);
            GL.ClearDepth(1.0f);

            Capture();
            
            OnInit(_ctx);

            _ctx.ContextChanged += ContextChanged;

            base.OnLoad(e);
        }

        void ContextChanged(bool enabled)
        {
            //if (enabled)
            OnResize(null);
            OnResized();
        }

        protected override void DestroyHandle()
        {
            DisposeContext();
            base.DestroyHandle();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            DisposeContext();
            base.OnHandleDestroyed(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        public virtual float GetDepth(int x, int y)
        {
            float val = 0;
            GL.ReadPixels(x, Height - y, 1, 1, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, OpenTK.Graphics.OpenGL.PixelType.Float, ref val);
            return val;
        }

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

        bool _bgColorChanged = false;
        public bool _textEnabled = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScreenTextHandler ScreenText { get { return _text; } }
        private ScreenTextHandler _text;

        public Point _selStart, _selEnd;
        public bool _selecting = false;
        public GLTexture _bgImage = null;
        public bool _forceNoSelection = false;
        public bool _updateImage = false;
        public BackgroundType _bgType = BackgroundType.Stretch;
        public enum BackgroundType { Stretch, Center, ResizeWithBars }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_updateCounter > 0)
                return;

            if (_ctx == null)
                base.OnPaint(e);
            else if (Monitor.TryEnter(_ctx))
            {
                try
                {
                    //Direct OpenGL calls to this panel
                    Capture();

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

                    //Set projection
                    if (_projectionChanged)
                    {
                        OnResized();
                        _projectionChanged = false;
                    }

                    //Apply camera
                    if (_camera != null)
                        fixed (Matrix* p = &_camera._matrix)
                        {
                            GL.MatrixMode(MatrixMode.Modelview);
                            GL.LoadMatrix((float*)p);
                        }
                    
                    //Render 3D scene
                    OnRender(_ctx, e);

                    //Render selection overlay and/or text overlays
                    if ((_selecting && !_forceNoSelection) || _text.Count != 0)
                    {
                        GL.Color4(Color.White);

                        GL.PushAttrib(AttribMask.AllAttribBits);

                        GL.Disable(EnableCap.DepthTest);
                        GL.Disable(EnableCap.Lighting);
                        GL.Disable(EnableCap.CullFace);
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

                        GL.MatrixMode(MatrixMode.Projection);
                        GL.PushMatrix();
                        GL.LoadIdentity();
                        Matrix p = Matrix.OrthographicMatrix(0, Width, 0, Height, -1, 1);
                        GL.LoadMatrix((float*)&p);
                        
                        GL.MatrixMode(MatrixMode.Modelview);
                        GL.PushMatrix();
                        GL.LoadIdentity();

                        if (_text.Count != 0 && _textEnabled)
                            _text.Draw();

                        if (_selecting && !_forceNoSelection)
                            RenderSelection();

                        GL.PopAttrib();

                        GL.PopMatrix();
                        GL.MatrixMode(MatrixMode.Projection);
                        GL.PopMatrix();
                    }

                    GL.Finish();
                    _ctx.Swap();
                    
                    ErrorCode code = GL.GetError();
                    if (code != ErrorCode.NoError)
                        this.Reset(); //Stops the red X of death in its tracks
                }
                finally { Monitor.Exit(_ctx); }
            }

            //Clear text values
            //This will be filled until the next render
            _text.Clear();
        }

        public void RenderBackground()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.CullFace);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            Matrix p = Matrix.OrthographicMatrix(0, Width, 0, Height, -1, 1);
            GL.LoadMatrix((float*)&p);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
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
                case BackgroundType.Stretch:

                    points[0] = points[1] = points[3] = points[6] = 0.0f;
                    points[2] = points[4] = Width;
                    points[5] = points[7] = Height;

                    break;

                case BackgroundType.Center:

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

                case BackgroundType.ResizeWithBars:

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

            GL.Begin(BeginMode.Quads);

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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);

            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

        internal virtual void OnInit(TKContext ctx) { }

        public float _fovY = 45.0f, _nearZ = 1.0f, _farZ = 200000.0f, _aspect;

        internal Matrix _projectionMatrix;
        internal Matrix _projectionInverse;

        /// <summary>
        /// Projects a screen point to world coordinates.
        /// </summary>
        /// <returns>3D world point perpendicular to the camera with distance z</returns>
        public Vector3 UnProject(Vector3 point) { return UnProject(point._x, point._y, point._z); }
        /// <summary>
        /// Projects a screen point to world coordinates.
        /// </summary>
        /// <returns>3D world point perpendicular to the camera with distance z</returns>
        public Vector3 UnProject(float x, float y, float z)
        {
            if (_camera == null)
                return new Vector3();

            Vector4 v;
            v._x = 2 * x / Width - 1;
            v._y = 2 * (Height - y) / Height - 1;
            v._z = 2 * z - 1;
            v._w = 1.0f;

            return (Vector3)(_camera._matrixInverse * _projectionInverse * v);
        }

        /// <summary>
        /// Projects a world point to screen coordinates.
        /// </summary>
        /// <returns>2D coordinate on the screen with z as depth</returns>
        public Vector3 Project(float x, float y, float z) { return Project(new Vector3(x, y, z)); }
        /// <summary>
        /// Projects a world point to screen coordinates.
        /// </summary>
        /// <returns>2D coordinate on the screen with z as depth</returns>
        public Vector3 Project(Vector3 source)
        {
            if (_camera == null)
                return new Vector3();

            Vector4 v4 = (Vector4)source;
            Vector4 t1 = _camera._matrix * v4;
            Vector4 t2 = _projectionMatrix * t1;

            if ((t2._w = -t1._z) == 0)
                return new Vector3();

            t2._x /= t2._w;
            t2._y /= t2._w;
            t2._z /= t2._w;

            Vector3 v;

            v._x = (t2._x / 2.0f + 0.5f) * Width;
            v._y = Height - ((t2._y / 2.0f + 0.5f) * Height);
            v._z = (t2._z + 1.0f) / 2.0f;

            return v;
        }

        public Vector3 TraceZ(Vector3 point, float z)
        {
            if (_camera == null)
                return new Vector3();

            double a = point._z - z;
            //Perform trig functions, using camera for angles

            //Get angle, truncating to MOD 180
            //double angleX = _camera._rotation._y - ((int)(_camera._rotation._y / 180.0) * 180);

            double angleX = Math.IEEERemainder(-_camera._rotation._y, 180.0);
            if (angleX < -90.0f)
                angleX = -180.0f - angleX;
            if (angleX > 90.0f)
                angleX = 180.0f - angleX;

            double angleY = Math.IEEERemainder(_camera._rotation._x, 180.0);
            if (angleY < -90.0f)
                angleY = -180.0f - angleY;
            if (angleY > 90.0f)
                angleY = 180.0f - angleY;

            float lenX = (float)(Math.Tan(angleX * Math.PI / 180.0) * a);
            float lenY = (float)(Math.Tan(angleY * Math.PI / 180.0) * a);

            return new Vector3(point._x + lenX, point._y + lenY, z);
        }

        //Projects a ray at 'screenPoint' through sphere at 'center' with 'radius'.
        //If point does not intersect
        public Vector3 ProjectCameraSphere(Vector2 screenPoint, Vector3 center, float radius, bool clamp)
        {
            if (_camera == null)
                return new Vector3();

            Vector3 point;

            //Get ray points
            Vector4 v = new Vector4(2 * screenPoint._x / Width - 1, 2 * (Height - screenPoint._y) / Height - 1, -1.0f, 1.0f);
            Vector3 ray1 = (Vector3)(_camera._matrixInverse * _projectionInverse * v);
            v._z = 1.0f;
            Vector3 ray2 = (Vector3)(_camera._matrixInverse * _projectionInverse * v);

            if (!Maths.LineSphereIntersect(ray1, ray2, center, radius, out point))
            {
                //If no intersect is found, project the ray through the plane perpendicular to the camera.
                Maths.LinePlaneIntersect(ray1, ray2, center, _camera.GetPoint().Normalize(center), out point);

                //Clamp the point to edge of the sphere
                if (clamp)
                    point = Maths.PointAtLineDistance(center, point, radius);
            }

            return point;
        }
        internal bool _ortho = false;
        public void SetProjectionType(bool ortho)
        {
            _camera.Scale(1.0f / _camera._scale._x, 1.0f / _camera._scale._y, 1.0f / _camera._scale._z);
            if (_ortho = ortho)
            {
                _nearZ = -100;
                float z = _camera._z;
                _camera.Translate(0, 0, -_camera._z);
                float scale = z <= 0 ? -z / 2.0f : 1.0f / z * 2.0f;
                _camera.Scale(scale, scale, 1.0f);
            }
            else
            {
                _nearZ = 1;
                _camera.Translate(0, 0, _camera._z);
            }
            
            _projectionChanged = true;
            Invalidate();
        }
        protected void CalculateProjection()
        {
            if (_ortho)
            {
                _projectionMatrix = Matrix.OrthographicMatrix(Width, Height, _nearZ, _farZ);
                _projectionInverse = Matrix.ReverseOrthographicMatrix(Width, Height, _nearZ, _farZ);
            }
            else
            {
                _projectionMatrix = Matrix.PerspectiveMatrix(_fovY, _aspect, _nearZ, _farZ);
                _projectionInverse = Matrix.ReversePerspectiveMatrix(_fovY, _aspect, _nearZ, _farZ);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            _projectionChanged = true;

            if (BackgroundImage != null)
                GL.Viewport(0, 0, Width, Height);

            if (_ctx != null)
                _ctx.Update();

            Invalidate();
        }

        public virtual void OnResized()
        {
            if (_ctx == null)
                return;

            Capture();

            _aspect = (float)Width / Height;
            CalculateProjection();

            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);

            fixed (Matrix* p = &_projectionMatrix)
                GL.LoadMatrix((float*)p);
        }

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
                GL.Begin(BeginMode.LineLoop);
                GL.Vertex2(_selStart.X, _selStart.Y);
                GL.Vertex2(_selEnd.X, _selStart.Y);
                GL.Vertex2(_selEnd.X, _selEnd.Y);
                GL.Vertex2(_selStart.X, _selEnd.Y);
                GL.End();
                GL.Disable(EnableCap.LineStipple);
            }
        }

        internal protected virtual void OnRender(TKContext ctx, PaintEventArgs e)
        {
            GL.Clear(OpenTK.Graphics.OpenGL.ClearBufferMask.ColorBufferBit | OpenTK.Graphics.OpenGL.ClearBufferMask.DepthBufferBit);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GLPanel
            // 
            this.Name = "GLPanel";
            this.ResumeLayout(false);
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

        private Size _size = new Size();
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

            GL.Begin(BeginMode.Quads);

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
