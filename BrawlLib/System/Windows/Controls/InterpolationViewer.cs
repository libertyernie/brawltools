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

namespace System.Windows.Forms
{
    public partial class InterpolationViewer : GLPanel
    {
        public InterpolationViewer() { InitializeComponent(); }

        public event EventHandler SelectedKeyframeChanged, FrameChanged, SignalChange;
        
        public KeyframeEntry _selKey = null, _hiKey = null;
        private bool _lockIncs = false;
        private bool _dragging = false;
        private int _frame;
        private int _frameLimit = 0;
        internal bool _updating = false;
        private KeyframeEntry _keyRoot = null;
        private bool _allKeys = true;
        private bool _genTans = false;
        private const float _lineWidth = 1.5f, _pointWidth = 5.0f;
        private Vector2? _slopePoint = null;
        private Vector2 _origPos;
        private bool _keyDraggingAllowed = false;
        private float _xScale; //Width/Frames ratio
        private float _yScale; //Height/Values ratio
        private float _prevX;
        private float _prevY;
        private float _tanLen = 5.0f;
        private float _precision = 4.0f;
        private bool _drawTans = true;
        private bool _linear = false;
        private float _minVal = float.MaxValue;
        private float _maxVal = float.MinValue;
        private bool _syncStartEnd;
        public bool SyncStartEnd { get { return _syncStartEnd; } set { _syncStartEnd = value; } }
        public float TangentLength { get { return _tanLen; } set { _tanLen = value; Invalidate(); } }
        public float Precision { get { return _precision; } set { _precision = value; Invalidate(); } }
        public bool DrawTangents { get { return _drawTans; } set { _drawTans = value; Invalidate(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public KeyframeEntry KeyRoot { get { return _keyRoot; } set { _keyRoot = value; UpdateDisplay(); } }
        public bool KeyDraggingAllowed { get { return _keyDraggingAllowed; } set { _keyDraggingAllowed = value; } }
        public bool GenerateTangents { get { return _genTans; } set { _genTans = value; } }
        public int FrameIndex
        {
            get { return _frame; }
            set
            {
                _frame = value.Clamp(0, _frameLimit);
                //if (_keyRoot != null)
                //    for (KeyframeEntry entry = _keyRoot._next; (entry != _keyRoot); entry = entry._next)
                //        if (entry._index == value)
                //        {
                //            _selKey = entry;
                //            break;
                //        }
                Invalidate();

                if (!_updating && FrameChanged != null)
                    FrameChanged(this, null);
            }
        }
        public int FrameLimit { get { return _frameLimit; } set { _frameLimit = value; } }
        private int DisplayedFrameLimit
        {
            get
            {
                if (_allKeys)
                    return _frameLimit - 1;
                else if (_selKey != null)
                    return GetKeyframeMaxIndex() - GetKeyframeMinIndex();
                return 0;
            } 
        }
        public bool AllKeyframes
        {
            get { return _allKeys; }
            set
            {
                _allKeys = value;
                UpdateDisplay();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public KeyframeEntry SelectedKeyframe
        {
            get { return _selKey; }
            set
            {
                _selKey = value;

                if (SelectedKeyframeChanged != null && !_updating)
                    SelectedKeyframeChanged(this, null);
            }
        }
        public bool Linear
        {
            get { return _linear; }
            set
            {
                _linear = value;
                if (!_updating)
                    UpdateDisplay();
            }
        }
        private bool Has3PlusVals()
        {
            float val1 = 0, val2 = 0;
            if (AllKeyframes)
            {
                bool v1Set = false, v2Set = false;
                for (KeyframeEntry entry = _keyRoot._next; (entry != _keyRoot); entry = entry._next)
                {
                    if (!v1Set)
                    {
                        val1 = entry._value;
                        v1Set = true;
                    }
                    else if (!v2Set)
                    {
                        if (entry._value != val1)
                        {
                            val2 = entry._value;
                            v2Set = true;
                        }
                    }
                    else if (entry._value != val1 && entry._value != val2)
                        return true;
                }
            }
            else
            {
                if (SelectedKeyframe._prev._index != -1)
                    val1 = SelectedKeyframe._prev._value;

                if (SelectedKeyframe._value != val1)
                    val2 = SelectedKeyframe._value;
                else
                    return false;

                if (SelectedKeyframe._next._index != -1 && SelectedKeyframe._next._value != val1 && SelectedKeyframe._next._value != val2)
                    return true;
            }
            return false;
        }

        private int GetKeyframeMaxIndex()
        {
            if (!AllKeyframes)
            {
                if (_selKey != null)
                    if (_selKey._next._index < 0)
                        return _selKey._index;
                    else
                        return _selKey._next._index;
            }
            else
                return _frameLimit - 1;

            return 0;
        }

        private int GetKeyframeMinIndex()
        {
            if (!AllKeyframes)
            {
                if (_selKey != null)
                    if (_selKey._prev._index < 0)
                        return _selKey._index;
                    else
                        return _selKey._prev._index;
            }
            else
                return 0;

            return 0;
        }

        public void UpdateDisplay()
        {
            OnResized();
            Invalidate();
        }

        unsafe internal override void OnInit(TKContext ctx)
        {
            //Set caps
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            GL.LineWidth(_lineWidth);
            GL.PointSize(_pointWidth);

            OnResized();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_keyRoot == null)
                return;

            float xVal = e.X / _xScale;
            float y = (Height - e.Y);
            int frameVal = (int)(xVal + 1.5f);
            int min = GetKeyframeMinIndex();
            if (!_dragging)
            {
                _hiKey = null;

                Cursor = Cursors.Default;

                if (AllKeyframes)
                    for (KeyframeEntry entry = _keyRoot._next; (entry != _keyRoot); entry = entry._next)
                    {
                        float frame = (float)entry._index;
                        if (Math.Abs(e.X - (frame * _xScale)) <= _pointWidth)
                        {
                            if (Math.Abs(y - (entry._value - _minVal) * _yScale) <= _pointWidth)
                            {
                                _hiKey = entry;
                                Cursor = Cursors.Hand;
                                return;
                            }
                        }
                    }

                if (/*_drawTans && */_selKey != null)
                {
                    if (!AllKeyframes)
                    {
                        if (SelectedKeyframe._prev._index != -1)
                        {
                            float frame = (float)SelectedKeyframe._prev._index;
                            if (Math.Abs(e.X - ((frame - min) * _xScale)) <= _pointWidth)
                            {
                                if (Math.Abs(y - ((SelectedKeyframe._prev._value - _minVal) * _yScale)) <= _pointWidth)
                                {
                                    _hiKey = SelectedKeyframe._prev;
                                    Cursor = Cursors.Hand;
                                    return;
                                }
                            }
                        }

                        float frame1 = (float)SelectedKeyframe._index;
                        if (Math.Abs(e.X - ((frame1 - min) * _xScale)) <= _pointWidth)
                        {
                            if (Math.Abs(y - ((SelectedKeyframe._value - _minVal) * _yScale)) <= _pointWidth)
                            {
                                _hiKey = SelectedKeyframe;
                                Cursor = Cursors.Hand;
                                return;
                            }
                        }

                        if (SelectedKeyframe._next._index != -1)
                        {
                            float frame = (float)SelectedKeyframe._next._index;
                            if (Math.Abs(e.X - ((frame - min) * _xScale)) <= _pointWidth)
                            {
                                if (Math.Abs(y - ((SelectedKeyframe._next._value - _minVal) * _yScale)) <= _pointWidth)
                                {
                                    _hiKey = SelectedKeyframe._next;
                                    Cursor = Cursors.Hand;
                                    return;
                                }
                            }
                        }
                    }

                    float i1 = -(_tanLen / 2);
                    float i2 = (_tanLen / 2);

                    int xVal2 = _selKey._index;
                    float yVal = _selKey._value;
                    float tan = _selKey._tangent;

                    float p = (float)Math.Sqrt(_precision / 4.0f);
                    Vector2 one = new Vector2((xVal2 + i1 * p - min) * _xScale, (yVal - _minVal + tan * i1 * p) * _yScale);
                    Vector2 two = new Vector2((xVal2 + i2 * p - min) * _xScale, (yVal - _minVal + tan * i2 * p) * _yScale);

                    _slopePoint = null;
                    if (Math.Abs(e.X - one._x) <= _pointWidth)
                    {
                        if (Math.Abs(y - one._y) <= _pointWidth)
                        {
                            Cursor = Cursors.Hand;
                            _slopePoint = new Vector2(e.X, y);
                            _origPos = new Vector2((float)(xVal2 - min) * _xScale, (yVal - _minVal) * _yScale);
                            _hiKey = _selKey;
                            return;
                        }
                    }

                    if (Math.Abs(e.X - two._x) <= _pointWidth)
                    {
                        if (Math.Abs(y - two._y) <= _pointWidth)
                        {
                            Cursor = Cursors.Hand;
                            _slopePoint = new Vector2(e.X, y);
                            _origPos = new Vector2((float)(xVal2 - min) * _xScale, (yVal - _minVal) * _yScale);
                            _hiKey = _selKey;
                            return;
                        }
                    }
                }
                if (AllKeyframes)
                    if (Math.Abs(e.X - (_frame * _xScale)) <= _pointWidth)
                        Cursor = Cursors.VSplit;
            }
            else if (_selKey != null && (_keyDraggingAllowed || _slopePoint != null))
            {
                if (_slopePoint != null)
                {
                    int xVal2 = _selKey._index;
                    float yVal = _selKey._value;

                    float xDiff = e.X - ((Vector2)_slopePoint)._x;
                    float yDiff = y - ((Vector2)_slopePoint)._y;

                    float x2 = ((Vector2)_slopePoint)._x + xDiff;
                    float y2 = ((Vector2)_slopePoint)._y + yDiff;

                    _slopePoint = new Vector2(x2 == _origPos._x ? ((Vector2)_slopePoint)._x : x2, y2);

                    Vector2 x = (Vector2)_slopePoint - _origPos;
                    _selKey._tangent = (float)Math.Round((x._y / _yScale) / (x._x / _xScale), 5);

                    if (_genTans)
                    {
                        _selKey._prev.GenerateTangent();
                        _selKey._next.GenerateTangent();
                    }

                    if (_syncStartEnd)
                    {
                        if (SelectedKeyframe._prev._index == -1 && SelectedKeyframe._prev._prev != SelectedKeyframe)
                        {
                            SelectedKeyframe._prev._prev._tangent = SelectedKeyframe._tangent;
                            SelectedKeyframe._prev._prev._value = SelectedKeyframe._value;
                        }

                        if (SelectedKeyframe._next._index == -1 && SelectedKeyframe._next._next != SelectedKeyframe)
                        {
                            SelectedKeyframe._next._next._tangent = SelectedKeyframe._tangent;
                            SelectedKeyframe._next._next._value = SelectedKeyframe._value;
                        }
                    }

                    Invalidate();

                    if (SelectedKeyframeChanged != null)
                        SelectedKeyframeChanged(this, null);

                    if (SignalChange != null)
                        SignalChange(this, null);
                }
                else if (_keyDraggingAllowed)
                {
                    float yVal = y / _yScale + _minVal;
                    int xv = frameVal - 1;
                    int xDiff = xv - (int)_prevX;
                    float yDiff = (yVal - _prevY);

                    if (_selKey._prev._index < _selKey._index + xDiff && _selKey._next._index > _selKey._index + xDiff && _selKey._next._index != -1 && _selKey._prev._index != -1)
                        _selKey._index += xDiff;

                    _selKey._value = (float)Math.Round(_selKey._value + yDiff, 3);
                    _prevX = xv;
                    _prevY = yVal;

                    if (_genTans)
                    {
                        _selKey.GenerateTangent();
                        _selKey._prev.GenerateTangent();
                        _selKey._next.GenerateTangent();
                    }

                    if (_syncStartEnd)
                    {
                        if (SelectedKeyframe._prev._index == -1 && SelectedKeyframe._prev._prev != SelectedKeyframe)
                        {
                            SelectedKeyframe._prev._prev._tangent = SelectedKeyframe._tangent;
                            SelectedKeyframe._prev._prev._value = SelectedKeyframe._value;
                        }
                        if (SelectedKeyframe._next._index == -1 && SelectedKeyframe._next._next != SelectedKeyframe)
                        {
                            SelectedKeyframe._next._next._tangent = SelectedKeyframe._tangent;
                            SelectedKeyframe._next._next._value = SelectedKeyframe._value;
                        }
                    }

                    Invalidate();

                    if (SelectedKeyframeChanged != null)
                        SelectedKeyframeChanged(this, null);

                    if (SignalChange != null)
                        SignalChange(this, null);
                }
            }
            else if (frameVal > 0 && _selKey == null)
                FrameIndex = frameVal;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            bool t = _selKey != _hiKey;

            if (AllKeyframes)
                _dragging = (_selKey = _hiKey) != null || Cursor == Cursors.VSplit || _slopePoint != null;
            else
            {
                if (_hiKey != null)
                    _selKey = _hiKey;
                _dragging = _selKey != null && (_slopePoint != null || Cursor == Cursors.Hand);
            }

            if (_selKey != null)
            {
                if (_slopePoint == null)
                {
                    int min = GetKeyframeMinIndex();
                    _prevX = _selKey._index - min;
                    _prevY = _selKey._value;
                }
                _frame = _selKey._index;

                if ((_dragging && !Has3PlusVals()) || _slopePoint != null)
                    _lockIncs = true;
            }

            if (t)
            {
                Invalidate();
                if (SelectedKeyframeChanged != null)
                    SelectedKeyframeChanged(this, null);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_slopePoint != null || (_dragging && _lockIncs))
                Invalidate();

            _dragging = false;
            _lockIncs = false;
            _slopePoint = null;
        }

        public float GetFrameValue(float index)
        {
            KeyframeEntry entry, root = _keyRoot;

            if (_keyRoot == null)
                return 0;

            if (index >= root._prev._index)
                return root._prev._value;
            if (index <= root._next._index)
                return root._next._value;

            for (entry = root._next;
                (entry != root) &&
                (entry._index < index); 
                entry = entry._next)
                if (entry._index == index)
                    return entry._value;
            
            return entry._prev.Interpolate(index - (float)entry._prev._index, _linear);
        }

        public void FindMaxMin()
        {
            if (!AllKeyframes && SelectedKeyframe == null)
                return;

            _minVal = float.MaxValue;
            _maxVal = float.MinValue;

            int start = GetKeyframeMinIndex();
            int end = GetKeyframeMaxIndex();

            for (float i = start; i <= end; i += (1 / _precision))
                if (i >= 0 && i < _frameLimit)
                {
                    float v = GetFrameValue(i);

                    if (v < _minVal)
                        _minVal = v;
                    if (v > _maxVal)
                        _maxVal = v;
                }
        }

        private void CalcXY()
        {
            if (_lockIncs)
                return;

            int i = DisplayedFrameLimit;
            if (i == 0)
                return;

            //Calculate X Scale
            _xScale = (float)Width / (float)i;

            FindMaxMin();

            //Calculate Y Scale
            _yScale = ((float)Height / (_maxVal - _minVal));
        }

        private void DrawTangent(KeyframeEntry e, float xMin)
        {
            int xVal = e._index;
            float yVal = e._value;
            float tan = e._tangent;

            float i1 = -(_tanLen / 2);
            float i2 = (_tanLen / 2);

            float p = (float)Math.Sqrt(_precision / 4.0f);
            Vector2 one = new Vector2((xVal + i1 * p - xMin) * _xScale, (yVal - _minVal + tan * i1 * p) * _yScale);
            Vector2 two = new Vector2((xVal + i2 * p - xMin) * _xScale, (yVal - _minVal + tan * i2 * p) * _yScale);

            if (e == _selKey)
            {
                GL.Color4(Color.Purple);
                GL.Begin(BeginMode.Points);

                GL.Vertex2(one._x, one._y);
                GL.Vertex2(two._x, two._y);

                GL.End();
            }
            else
            {
                GL.Color4(Color.Green);

                float angle = (float)Math.Atan((tan * _yScale) / _xScale) * Maths._rad2degf;

                GL.PushMatrix();
                GL.Translate(one._x, one._y, 0.0f);
                GL.Rotate(angle - 180.0f, 0, 0, 1);

                GL.Begin(BeginMode.LineStrip);
                GL.Vertex2(-7.0f, 3.5f);
                GL.Vertex2(0.0f, 0.0f);
                GL.Vertex2(-7.0f, -3.5f);
                GL.End();

                GL.PopMatrix();

                GL.PushMatrix();
                GL.Translate(two._x, two._y, 0.0f);
                GL.Rotate(angle, 0, 0, 1);

                GL.Begin(BeginMode.LineStrip);
                GL.Vertex2(-7.0f, 3.5f);
                GL.Vertex2(0.0f, 0.0f);
                GL.Vertex2(-7.0f, -3.5f);
                GL.End();

                GL.PopMatrix();
            }

            GL.Begin(BeginMode.LineStrip);
            GL.Vertex2(one._x, one._y);
            GL.Vertex2(two._x, two._y);
            GL.End();
        }

        protected internal unsafe override void OnRender(TKContext ctx, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(Color.White);

            if (_keyRoot == null)
                return;

            CalcXY();

            if (_allKeys)
            {
                //Draw lines
                //GL.Color4(Color.Black);
                //GL.Begin(BeginMode.Lines);
                //for (KeyframeEntry entry = _keyRoot._next; (entry != _keyRoot); entry = entry._next)
                //{
                //    float xv = entry._index * xinc;
                //    GL.Vertex2(xv, 0.0f);
                //    GL.Vertex2(xv, Height);

                //    float yv = (GetFrameValue(entry._index) - _minVal) * yinc;
                //    GL.Vertex2(0.0f, yv);
                //    GL.Vertex2(Width, yv);
                //}
                //GL.End();

                //Draw tangents
                if (_drawTans)
                    for (KeyframeEntry entry = _keyRoot._next; (entry != _keyRoot); entry = entry._next)
                        DrawTangent(entry, 0);
                else if (_selKey != null && !Linear)
                    DrawTangent(_selKey, 0);

                //Draw interpolation
                GL.Color4(Color.Red);
                GL.Begin(BeginMode.LineStrip);
                if (!_linear)
                    for (float i = 0; i < (float)_frameLimit; i += (1 / _precision))
                        GL.Vertex2(i * _xScale, (GetFrameValue(i) - _minVal) * _yScale);
                else
                    for (KeyframeEntry entry = _keyRoot._next; (entry != _keyRoot); entry = entry._next)
                        GL.Vertex2(entry._index * _xScale, (GetFrameValue(entry._index) - _minVal) * _yScale);
                GL.End();
                
                //Draw frame indicator
                GL.Color4(Color.Blue);
                if (_frame >= 0 && _frame < _frameLimit)
                {
                    GL.Begin(BeginMode.Lines);

                    float r = _frame * _xScale;
                    GL.Vertex2(r, 0.0f);
                    GL.Vertex2(r, Height);

                    GL.End();
                }

                //Draw points
                GL.Color4(Color.Black);
                GL.Begin(BeginMode.Points);
                for (KeyframeEntry entry = _keyRoot._next; (entry != _keyRoot); entry = entry._next)
                {
                    bool t = false;
                    if (t = (_hiKey == entry || _selKey == entry))
                    {
                        GL.PointSize(_pointWidth * 4.0f);
                        GL.Color4(Color.Orange);
                    }
                    GL.Vertex2(entry._index * _xScale, (GetFrameValue(entry._index) - _minVal) * _yScale);

                    if (t)
                    {
                        GL.PointSize(_pointWidth);
                        GL.Color4(Color.Black);
                    }
                }
                GL.End();
            }
            else if (SelectedKeyframe != null)
            {
                //Draw lines
                GL.Color4(Color.Black);
                GL.Begin(BeginMode.Lines);

                int min = GetKeyframeMinIndex();
                int max = GetKeyframeMaxIndex();

                float xv = (SelectedKeyframe._index - min) * _xScale;
                GL.Vertex2(xv, 0.0f);
                GL.Vertex2(xv, Height);

                float yv = (GetFrameValue(SelectedKeyframe._index) - _minVal) * _yScale;
                GL.Vertex2(0.0f, yv);
                GL.Vertex2(Width, yv);

                GL.End();

                //Draw interpolation
                GL.Color4(Color.Red);
                GL.Begin(BeginMode.LineStrip);
                for (float i = 0; i <= (float)(max - min); i += (1 / _precision))
                    GL.Vertex2(i * _xScale, (GetFrameValue(i + min) - _minVal) * _yScale);
                GL.End();
                
                //Draw tangent
                DrawTangent(SelectedKeyframe, min);
                if (_drawTans)
                {
                    if (SelectedKeyframe._prev._index != -1)
                        DrawTangent(SelectedKeyframe._prev, min);
                    if (SelectedKeyframe._next._index != -1)
                        DrawTangent(SelectedKeyframe._next, min);
                }

                //Draw points
                GL.Color4(Color.Black);
                GL.Begin(BeginMode.Points);

                if (SelectedKeyframe._prev._index != -1)
                    GL.Vertex2((SelectedKeyframe._prev._index - min) * _xScale, (GetFrameValue(SelectedKeyframe._prev._index) - _minVal) * _yScale);
                GL.Vertex2((SelectedKeyframe._index - min) * _xScale, (GetFrameValue(SelectedKeyframe._index) - _minVal) * _yScale);
                if (SelectedKeyframe._next._index != -1)
                    GL.Vertex2((SelectedKeyframe._next._index - min) * _xScale, (GetFrameValue(SelectedKeyframe._next._index) - _minVal) * _yScale);

                GL.End();
            }
        }

        public override void OnResized()
        {
            if (_ctx == null)
                return;

            Capture();

            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, 0, Height, -0.1f, 1.0f);

            _precision = ((float)Width / (float)384) * 4;
        }
    }
}