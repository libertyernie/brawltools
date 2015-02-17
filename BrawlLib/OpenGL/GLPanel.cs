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

    public delegate void ViewportAction(GLViewport p);
    public abstract unsafe class GLPanel : UserControl, IEnumerable<GLViewport>
    {
        public static GLPanel Current
        {
            get
            {
                if (_currentPanel == null && TKContext.CurrentContext != null)
                    TKContext.CurrentContext.Capture(true);
                return _currentPanel;
            }
        }
        private static GLPanel _currentPanel = null;

        public TKContext Context { get { return _ctx; } }
        protected TKContext _ctx;

        public event ViewportAction OnCurrentViewportChanged;

        protected int _updateCounter;
        protected GLViewport _currentViewport;
        protected GLViewport _highlightedViewport;
        protected List<GLViewport> _viewports = new List<GLViewport>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GLViewport HighlightedViewport { get { return _highlightedViewport; } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GLViewport CurrentViewport
        {
            get
            {
                if (_currentViewport == null)
                {
                    if (_viewports.Count == 0)
                        CreateDefaultViewport();

                    _currentViewport = _viewports[0];
                }

                return _currentViewport;
            }
            set
            {
                _currentViewport = value;

                if (OnCurrentViewportChanged != null)
                    OnCurrentViewportChanged(_currentViewport);

                if (!_viewports.Contains(_currentViewport) && _currentViewport != null)
                    AddViewport(_currentViewport);
            }
        }

        public GLPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque |
                ControlStyles.ResizeRedraw,
                true);

            _viewports = new List<GLViewport>();
        }

        public void ClearViewports()
        {
            foreach (GLViewport v in _viewports)
            {
                v._owner = null;
                v.OnInvalidate -= Invalidate;
            }
            _viewports.Clear();
        }
        public void AddViewport(GLViewport v)
        {
            _viewports.Add(v);
            v._owner = this;
            v.Resize();
            v.OnInvalidate += Invalidate;
        }
        public void RemoveViewport(GLViewport v)
        {
            if (_viewports.Contains(v))
            {
                v._owner = null;
                v.OnInvalidate -= Invalidate;
                _viewports.Remove(v);
            }
        }
        public void RemoveViewport(int index)
        {
            if (index < 0 || index >= _viewports.Count)
                return;

            GLViewport v = _viewports[index];
            v._owner = null;
            v.OnInvalidate -= Invalidate;
            _viewports.RemoveAt(index);
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

        public new void Capture()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(Capture));
                return;
            }

            if (_ctx == null)
                _ctx = new TKContext(this);

            _ctx.Capture();
        }
        public void Release() { if (_ctx != null) _ctx.Release(); }

        protected override void OnLoad(EventArgs e)
        {
            _ctx = new TKContext(this);

            Capture();

            Vector3 v = (Vector3)BackColor;
            GL.ClearColor(v._x, v._y, v._z, 0.0f);
            GL.ClearDepth(1.0);
            
            OnInit(_ctx);

            _ctx.ContextChanged += OnContextChanged;
            _ctx.ResetOccured += OnReset;

            base.OnLoad(e);
        }

        protected virtual void OnReset(object sender, EventArgs e) { OnInit(_ctx); }
        protected virtual void OnContextChanged(bool isNowCurrent)
        {
            //Don't update anything if this context has just been released
            if (isNowCurrent)
                OnResize(EventArgs.Empty);

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

        public GLViewport GetViewport(int x, int y)
        {
            if (_viewports.Count == 1)
                return _viewports[0];

            x = x.Clamp(0, Width);
            y = Height - y.Clamp(0, Height);

            foreach (GLViewport w in _viewports)
                if (x >= w.Region.X &&
                    x <= w.Region.X + w.Region.Width &&
                    y >= w.Region.Y &&
                    y <= w.Region.Y + w.Region.Height)
                    return w;

            if (_viewports.Count == 0)
                CreateDefaultViewport();

            return _viewports[0];
        }

        public virtual void CreateDefaultViewport()
        {
            AddViewport(GLViewport.DefaultPerspective);
        }

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

                    OnRender(e);

                    //Display newly rendered back buffer
                    _ctx.Swap();
                    
                    //Check for errors
                    ErrorCode code = GL.GetError();
                    if (code != ErrorCode.NoError)
                        this.Reset(); //Stops the red X of death in its tracks
                }
                finally { Monitor.Exit(_ctx); }
            }
        }

        internal virtual void OnInit(TKContext ctx) { }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            _highlightedViewport = GetViewport(e.X, e.Y);

            base.OnMouseMove(e);
        }

        public Vector3 TraceZ(Vector3 point, float z)
        {
            GLCamera camera = CurrentViewport.Camera;
            if (camera == null)
                return new Vector3();

            double a = point._z - z;
            //Perform trig functions, using camera for angles

            //Get angle, truncating to MOD 180
            //double angleX = _camera._rotation._y - ((int)(_camera._rotation._y / 180.0) * 180);

            double angleX = Math.IEEERemainder(-camera._rotation._y, 180.0);
            if (angleX < -90.0f)
                angleX = -180.0f - angleX;
            if (angleX > 90.0f)
                angleX = 180.0f - angleX;

            double angleY = Math.IEEERemainder(camera._rotation._x, 180.0);
            if (angleY < -90.0f)
                angleY = -180.0f - angleY;
            if (angleY > 90.0f)
                angleY = 180.0f - angleY;

            float lenX = (float)(Math.Tan(angleX * Math.PI / 180.0) * a);
            float lenY = (float)(Math.Tan(angleY * Math.PI / 180.0) * a);

            return new Vector3(point._x + lenX, point._y + lenY, z);
        }
        
        protected override void OnResize(EventArgs e)
        {
            if (_ctx != null)
                _ctx.Update();

            foreach (GLViewport v in _viewports)
                v.Resize();

            Invalidate();
        }

        protected virtual void OnRender(PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (GLViewport v in _viewports)
                OnRenderViewport(e, v);
        }

        protected virtual void OnRenderViewport(PaintEventArgs e, GLViewport v) { }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GLPanel
            // 
            this.Name = "GLPanel";
            this.ResumeLayout(false);
        }

        public IEnumerator<GLViewport> GetEnumerator() { return _viewports.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
    }
}
