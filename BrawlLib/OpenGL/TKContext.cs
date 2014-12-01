using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrawlLib.OpenGL;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK.Graphics;

namespace BrawlLib.OpenGL
{
    public delegate T GLCreateHandler<T>(TKContext ctx);

    public unsafe class TKContext : IDisposable
    {
        IGraphicsContext _context;
        private IWindowInfo _winInfo = null;
        public IWindowInfo WindowInfo { get { return _winInfo; } }

        //These provide a way to manage which context is in use to avoid errors
        public static List<TKContext> BoundContexts { get { return _boundContexts == null ? _boundContexts = new List<TKContext>() : _boundContexts; } }
        public static List<TKContext> _boundContexts;
        public static TKContext CurrentContext = null;

        internal Dictionary<string, object> _states = new Dictionary<string, object>();
        public static T FindOrCreate<T>(string name, GLCreateHandler<T> handler)
        {
            if (CurrentContext == null)
                return default(T);

            if (CurrentContext._states.ContainsKey(name))
                return (T)CurrentContext._states[name];
            T obj = handler(CurrentContext);
            CurrentContext._states[name] = obj;
            return obj;
        }

        public void Unbind()
        {
            try
            {
                foreach (object o in _states.Values)
                {
                    if (o is GLDisplayList)
                        (o as GLDisplayList).Delete();
                    else if (o is GLTexture)
                        (o as GLTexture).Delete();
                }
            }
            catch { }
            _states.Clear();
        }

        public bool bSupportsGLSLBinding, bSupportsGLSLUBO, bSupportsGLSLATTRBind, bSupportsGLSLCache;
        public bool _shadersEnabled = true;
        public int _version = 0;

        private Control _window;
        public TKContext(Control window)
        {
            _window = window;
            _winInfo = Utilities.CreateWindowsWindowInfo(_window.Handle);
            _context = new GraphicsContext(GraphicsMode.Default, WindowInfo);
            _context.MakeCurrent(WindowInfo);
            (_context as IGraphicsContextInternal).LoadAll();
            
            // Check for GLSL support
            string version = GL.GetString(StringName.Version);
            _version = int.Parse(version[0].ToString());
            //if (_version < 2)
                _shadersEnabled = false;

            if (_shadersEnabled)
            {
                //Now check extensions
                string extensions = GL.GetString(StringName.Extensions);
                if (extensions.Contains("GL_ARB_shading_language_420pack"))
                    bSupportsGLSLBinding = true;
                if (extensions.Contains("GL_ARB_uniform_buffer_object"))
                    bSupportsGLSLUBO = true;
                if ((bSupportsGLSLBinding || bSupportsGLSLUBO) && extensions.Contains("GL_ARB_explicit_attrib_location"))
                    bSupportsGLSLATTRBind = true;
                if (extensions.Contains("GL_ARB_get_program_binary"))
                    bSupportsGLSLCache = true;
            }
            BoundContexts.Add(this);
        }

        public delegate void ContextChangedEventHandler(bool isCurrent);
        public event ContextChangedEventHandler ContextChanged;
        public event EventHandler ResetOccured;
        
        public void Dispose()
        {
            Release();
            if (_context != null)
                _context.Dispose();
            if (BoundContexts.Contains(this))
                BoundContexts.Remove(this);
        }

        public void CheckErrors()
        { 
            ErrorCode code = GL.GetError();
            if (code == ErrorCode.NoError)
                return;

            //throw new Exception(code.ToString());
            Reset();
        }

        public void Capture() { Capture(false); }
        public void Capture(bool force)
        {
            try
            {
                //Only proceed if this window is not already current
                if (force || CurrentContext != this)
                {
                    //Release the current context if it exists
                    if (CurrentContext != null)
                        CurrentContext.Release();

                    //Make this context the current one
                    CurrentContext = this;
                    if (!_context.IsCurrent)
                        _context.MakeCurrent(WindowInfo);

                    if (ContextChanged != null)
                        ContextChanged(true);
                }
            }
            catch //(Exception x)
            {
                //MessageBox.Show(x.ToString()); 
                Reset();
            }
        }
        public void Swap() 
        {
            try
            {
                if (CurrentContext == this &&
                    _context != null &&
                    _context.IsCurrent &&
                    !_context.IsDisposed)
                    _context.SwapBuffers();
            }
            catch //(Exception x)
            {
                //MessageBox.Show(x.ToString());
                Reset();
            }
        }

        public bool Resetting { get { return _resetting; } }
        private bool _resetting = false;

        public void Reset()
        {
            if (_resetting) //Prevent a possible infinite loop
                return;

            _resetting = true;
            _window.Reset();
            Dispose();
            _winInfo = Utilities.CreateWindowsWindowInfo(_window.Handle);
            _context = new GraphicsContext(GraphicsMode.Default, WindowInfo);
            Capture(true);
            Update();
            (_context as IGraphicsContextInternal).LoadAll();

            // Check for GLSL support
            //_version = int.Parse(GL.GetString(StringName.Version)[0].ToString());
            //if (_version < 2)
            _shadersEnabled = false;

            if (_shadersEnabled)
            {
                //Now check extensions
                string extensions = GL.GetString(StringName.Extensions);
                if (extensions.Contains("GL_ARB_shading_language_420pack"))
                    bSupportsGLSLBinding = true;
                if (extensions.Contains("GL_ARB_uniform_buffer_object"))
                    bSupportsGLSLUBO = true;
                if ((bSupportsGLSLBinding || bSupportsGLSLUBO) && extensions.Contains("GL_ARB_explicit_attrib_location"))
                    bSupportsGLSLATTRBind = true;
                if (extensions.Contains("GL_ARB_get_program_binary"))
                    bSupportsGLSLCache = true;
            }
            _resetting = false;
            if (ResetOccured != null)
                ResetOccured(this, EventArgs.Empty);
        }
        public void Release()
        {
            if (CurrentContext == this && !_context.IsDisposed && _context.IsCurrent)
            {
                CurrentContext = null;
                _context.MakeCurrent(null);

                if (ContextChanged != null)
                    ContextChanged(false);
            }
        }
        public void Update()
        {
            if (CurrentContext == this)
                _context.Update(WindowInfo); 
        }

        public static unsafe void DrawBox(Vector3 p1, Vector3 p2)
        {
            GL.Begin(PrimitiveType.QuadStrip);

            GL.Vertex3(p1._x, p1._y, p1._z);
            GL.Vertex3(p1._x, p2._y, p1._z);
            GL.Vertex3(p2._x, p1._y, p1._z);
            GL.Vertex3(p2._x, p2._y, p1._z);
            GL.Vertex3(p2._x, p1._y, p2._z);
            GL.Vertex3(p2._x, p2._y, p2._z);
            GL.Vertex3(p1._x, p1._y, p2._z);
            GL.Vertex3(p1._x, p2._y, p2._z);
            GL.Vertex3(p1._x, p1._y, p1._z);
            GL.Vertex3(p1._x, p2._y, p1._z);

            GL.End();

            GL.Begin(PrimitiveType.Quads);

            GL.Vertex3(p1._x, p2._y, p1._z);
            GL.Vertex3(p1._x, p2._y, p2._z);
            GL.Vertex3(p2._x, p2._y, p2._z);
            GL.Vertex3(p2._x, p2._y, p1._z);

            GL.Vertex3(p1._x, p1._y, p1._z);
            GL.Vertex3(p1._x, p1._y, p2._z);
            GL.Vertex3(p2._x, p1._y, p2._z);
            GL.Vertex3(p2._x, p1._y, p1._z);

            GL.End();
        }

        public unsafe static void DrawInvertedBox(Vector3 p1, Vector3 p2)
        {
            GL.Begin(PrimitiveType.QuadStrip);

            GL.Vertex3(p1._x, p1._y, p1._z);
            GL.Vertex3(p1._x, p2._y, p1._z);
            GL.Vertex3(p1._x, p1._y, p2._z);
            GL.Vertex3(p1._x, p2._y, p2._z);
            GL.Vertex3(p2._x, p1._y, p2._z);
            GL.Vertex3(p2._x, p2._y, p2._z);
            GL.Vertex3(p2._x, p1._y, p1._z);
            GL.Vertex3(p2._x, p2._y, p1._z);
            GL.Vertex3(p1._x, p1._y, p1._z);
            GL.Vertex3(p1._x, p2._y, p1._z);

            GL.End();

            GL.Begin(PrimitiveType.Quads);

            GL.Vertex3(p2._x, p2._y, p1._z);
            GL.Vertex3(p2._x, p2._y, p2._z);
            GL.Vertex3(p1._x, p2._y, p2._z);
            GL.Vertex3(p1._x, p2._y, p1._z);

            GL.Vertex3(p1._x, p1._y, p1._z);
            GL.Vertex3(p1._x, p1._y, p2._z);
            GL.Vertex3(p2._x, p1._y, p2._z);
            GL.Vertex3(p2._x, p1._y, p1._z);

            GL.End();
        }

        public static void DrawCube(Vector3 p, float radius)
        {
            Vector3 p1 = new Vector3(p._x + radius, p._y + radius, p._z + radius);
            Vector3 p2 = new Vector3(p._x - radius, p._y - radius, p._z - radius);
            DrawBox(p2, p1);
        }

        public static void DrawInvertedCube(Vector3 p, float radius)
        {
            Vector3 p1 = new Vector3(p._x + radius, p._y + radius, p._z + radius);
            Vector3 p2 = new Vector3(p._x - radius, p._y - radius, p._z - radius);
            DrawInvertedBox(p2, p1);
        }

        public static void DrawRing(float radius)
        {
            GL.PushMatrix();
            GL.Scale(radius, radius, radius);
            GetRingList().Call();
            GL.PopMatrix();
        }

        public GLDisplayList GetLine() { return FindOrCreate<GLDisplayList>("Line", CreateLine); }
        private static GLDisplayList CreateLine(TKContext ctx)
        {
            GLDisplayList list = new GLDisplayList();
            GL.Begin(PrimitiveType.Lines);

            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(2.0f, 0.0f, 0.0f);

            GL.End();

            list.End();
            return list;
        }

        public static GLDisplayList GetRingList() { return FindOrCreate<GLDisplayList>("Ring", CreateRing); }
        private static GLDisplayList CreateRing(TKContext ctx)
        {
            GLDisplayList list = new GLDisplayList();
            list.Begin();

            GL.Begin(PrimitiveType.LineLoop);

            float angle = 0.0f;
            for (int i = 0; i < 360; i++, angle = i * Maths._deg2radf)
                GL.Vertex2(Math.Cos(angle), Math.Sin(angle));

            GL.End();

            list.End();
            return list;
        }

        public static GLDisplayList GetSquareList() { return FindOrCreate<GLDisplayList>("Square", CreateSquare); }
        private static GLDisplayList CreateSquare(TKContext ctx)
        {
            GLDisplayList list = new GLDisplayList();
            list.Begin();

            GL.Begin(PrimitiveType.LineLoop);

            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 1.0f, 1.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);

            GL.End();

            list.End();
            return list;
        }

        public static GLDisplayList GetAxisList() { return FindOrCreate<GLDisplayList>("Axes", CreateAxes); }
        private static GLDisplayList CreateAxes(TKContext ctx)
        {
            GLDisplayList list = new GLDisplayList();
            list.Begin();

            GL.Begin(PrimitiveType.Lines);

            GL.Color4(1.0f, 0.0f, 0.0f, 1.0f);

            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(2.0f, 0.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 0.0f);
            GL.Vertex3(1.0f, 1.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 1.0f);

            GL.Color4(0.0f, 1.0f, 0.0f, 1.0f);

            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 2.0f, 0.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f);
            GL.Vertex3(1.0f, 1.0f, 0.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f);
            GL.Vertex3(0.0f, 1.0f, 1.0f);

            GL.Color4(0.0f, 0.0f, 1.0f, 1.0f);

            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 2.0f);
            GL.Vertex3(0.0f, 0.0f, 1.0f);
            GL.Vertex3(1.0f, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 1.0f, 1.0f);

            GL.End();

            list.End();
            return list;
        }
        public static GLDisplayList GetCubeList() { return FindOrCreate<GLDisplayList>("Cube", CreateCube); }
        private static GLDisplayList CreateCube(TKContext ctx)
        {
            GLDisplayList list = new GLDisplayList();
            list.Begin();

            GL.Begin(PrimitiveType.QuadStrip);

            Vector3 p1 = new Vector3(0);
            Vector3 p2 = new Vector3(0.99f);

            GL.Vertex3(p1._x, p1._y, p1._z);
            GL.Vertex3(p1._x, p2._y, p1._z);
            GL.Vertex3(p2._x, p1._y, p1._z);
            GL.Vertex3(p2._x, p2._y, p1._z);
            GL.Vertex3(p2._x, p1._y, p2._z);
            GL.Vertex3(p2._x, p2._y, p2._z);
            GL.Vertex3(p1._x, p1._y, p2._z);
            GL.Vertex3(p1._x, p2._y, p2._z);
            GL.Vertex3(p1._x, p1._y, p1._z);
            GL.Vertex3(p1._x, p2._y, p1._z);

            GL.End();

            GL.Begin(PrimitiveType.Quads);

            GL.Vertex3(p1._x, p2._y, p1._z);
            GL.Vertex3(p1._x, p2._y, p2._z);
            GL.Vertex3(p2._x, p2._y, p2._z);
            GL.Vertex3(p2._x, p2._y, p1._z);

            GL.Vertex3(p1._x, p1._y, p1._z);
            GL.Vertex3(p1._x, p1._y, p2._z);
            GL.Vertex3(p2._x, p1._y, p2._z);
            GL.Vertex3(p2._x, p1._y, p1._z);

            GL.End();

            list.End();
            return list;
        }

        public static GLDisplayList GetCircleList() { return FindOrCreate<GLDisplayList>("Circle", CreateCircle); }
        private static GLDisplayList CreateCircle(TKContext ctx)
        {
            GLDisplayList list = new GLDisplayList();
            list.Begin();

            GL.Begin(PrimitiveType.TriangleFan);

            GL.Vertex3(0.0f, 0.0f, 0.0f);

            float angle = 0.0f;
            for (int i = 0; i < 361; i++, angle = i * Maths._deg2radf)
                GL.Vertex2(Math.Cos(angle), Math.Sin(angle));

            GL.End();

            list.End();
            return list;
        }

        public static void DrawSphere(float radius)
        {
            GL.PushMatrix();
            GL.Scale(radius, radius, radius);
            GetSphereList().Call();
            GL.PopMatrix();
        }
        public static GLDisplayList GetSphereList() { return FindOrCreate<GLDisplayList>("Sphere", CreateSphere); }
        public static GLDisplayList CreateSphere(TKContext ctx)
        {
            //IntPtr quad = Glu.NewQuadric();
            //Glu.QuadricDrawStyle(quad, QuadricDrawStyle.Fill);
            //Glu.QuadricOrientation(quad, QuadricOrientation.Outside);

            GLDisplayList dl = new GLDisplayList();

            dl.Begin();

            DrawSphere(new Vector3(), 1.0f, 40);
            //Glu.Sphere(quad, 1.0f, 40, 40);
            dl.End();

            //Glu.DeleteQuadric(quad);

            return dl;
        }
        public static void DrawSphere(Vector3 center, float radius, uint precision)
        {
            if (radius < 0.0f)
                radius = -radius;

            if (radius == 0.0f)
                throw new DivideByZeroException("DrawSphere: Radius cannot be 0f.");
            
            if (precision == 0)
                throw new DivideByZeroException("DrawSphere: Precision of 8 or greater is required.");

            float halfPI = (float)(Math.PI * 0.5);
            float oneThroughPrecision = 1.0f / precision;
            float twoPIThroughPrecision = (float)(Math.PI * 2.0 * oneThroughPrecision);

            float theta1, theta2, theta3;
            Vector3 norm, pos;

            for (uint j = 0; j < precision / 2; j++)
            {
                theta1 = (j * twoPIThroughPrecision) - halfPI;
                theta2 = ((j + 1) * twoPIThroughPrecision) - halfPI;

                GL.Begin(PrimitiveType.TriangleStrip);
                for (uint i = 0; i <= precision; i++)
                {
                    theta3 = i * twoPIThroughPrecision;

                    norm._x = (float)(Math.Cos(theta2) * Math.Cos(theta3));
                    norm._y = (float)Math.Sin(theta2);
                    norm._z = (float)(Math.Cos(theta2) * Math.Sin(theta3));
                    pos._x = center._x + radius * norm._x;
                    pos._y = center._y + radius * norm._y;
                    pos._z = center._z + radius * norm._z;

                    GL.Normal3(norm._x, norm._y, norm._z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * (j + 1) * oneThroughPrecision);
                    GL.Vertex3(pos._x, pos._y, pos._z);

                    norm._x = (float)(Math.Cos(theta1) * Math.Cos(theta3));
                    norm._y = (float)Math.Sin(theta1);
                    norm._z = (float)(Math.Cos(theta1) * Math.Sin(theta3));
                    pos._x = center._x + radius * norm._x;
                    pos._y = center._y + radius * norm._y;
                    pos._z = center._z + radius * norm._z;

                    GL.Normal3(norm._x, norm._y, norm._z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * j * oneThroughPrecision);
                    GL.Vertex3(pos._x, pos._y, pos._z);
                }
                GL.End();
            }
        }
    }
}