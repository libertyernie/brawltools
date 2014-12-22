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
        public static GLPanel Current { get { if (_currentPanel == null) TKContext.CurrentContext.Capture(true); return _currentPanel; } }
        private static GLPanel _currentPanel = null;

        public GLCamera Camera { get { return _camera; } }
        public TKContext Context { get { return _ctx; } }

        public float _fovY = 45.0f, _nearZ = 1.0f, _farZ = 200000.0f, _aspect;
        public Matrix _projectionMatrix;
        public Matrix _projectionInverse;

        public bool ProjectionChanged { get { return _projectionChanged; } set { _projectionChanged = value; } }

        protected int _updateCounter;
        protected bool _projectionChanged = true;
        protected TKContext _ctx;
        public GLCamera _camera;

        public bool IsOrthographic { get { return _orthographic; } set { _orthographic = value; _projectionChanged = true; Invalidate(); } }
        protected bool _orthographic = false;

        public enum BGImageType { Stretch, Center, ResizeWithBars }
        
        public GLPanel()
        {
            _camera = new GLCamera();

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

        protected delegate void NoArgsDelegate();
        public new void Capture()
        {
            if (InvokeRequired)
            {
                Invoke(new NoArgsDelegate(Capture));
                return;
            }

            if (_ctx != null)
                _ctx.Capture();
        }
        public void Release() { if (_ctx != null) _ctx.Release(); }

        protected override void OnLoad(EventArgs e)
        {
            _ctx = new TKContext(this);

            Vector3 v = (Vector3)BackColor;
            GL.ClearColor(v._x, v._y, v._z, 0.0f);
            GL.ClearDepth(1.0);

            Capture();
            
            OnInit(_ctx);

            _ctx.ContextChanged += OnContextChanged;
            _ctx.ResetOccured += OnReset;

            base.OnLoad(e);
        }

        protected virtual void OnReset(object sender, EventArgs e)
        {
            OnInit(_ctx);
            UpdateProjection();
        }

        protected virtual void OnContextChanged(bool isNowCurrent)
        {
            //Don't update anything if this context has just been released
            if (isNowCurrent)
            {
                OnResize(EventArgs.Empty);
                UpdateProjection();
            }

            _currentPanel = isNowCurrent ? this : null;
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
                    
                    //Render 3D scene
                    OnRender(e);

                    AfterRender();

                    GL.Finish();
                    _ctx.Swap();
                    
                    ErrorCode code = GL.GetError();
                    if (code != ErrorCode.NoError)
                        this.Reset(); //Stops the red X of death in its tracks
                }
                finally { Monitor.Exit(_ctx); }
            }
        }

        /// <summary>
        /// This is for rendering things in the background.
        /// This is called before projection and camera matrices are applied.
        /// </summary>
        protected virtual void BeforeRender() { }
        /// <summary>
        /// This is for rendering things in the foreground.
        /// </summary>
        protected virtual void AfterRender() { }

        internal virtual void OnInit(TKContext ctx) { }

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
        
        public void SetProjectionType(bool ortho)
        {
            if (ortho == _orthographic)
                return;

            Vector3 point = _camera.GetPoint();
            Vector3 rot = _camera._rotation;
            _camera.Reset();
            _camera.Translate(point);
            _camera.Rotate(rot);

            if (_orthographic = ortho)
            {
                //Set near z and far z for orthographic
                _nearZ = -10000;
                _farZ = 10000;

                float z = _camera._z;

                float scale = z == 0 ? 1.0f : 1.0f / z;
                _camera.Scale(scale, scale, 1.0f);
            }
            else
            {
                //Set near z and far z for perspective
                _nearZ = 1;
                _farZ = 200000;
            }
            
            _projectionChanged = true;
            Invalidate();
        }
        protected virtual void CalculateProjection()
        {
            if (_orthographic)
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

            if (_ctx != null)
                _ctx.Update();

            _aspect = (float)Width / Height;

            Invalidate();
        }

        public virtual void UpdateProjection()
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

        internal protected virtual void OnRender(PaintEventArgs e)
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
}
