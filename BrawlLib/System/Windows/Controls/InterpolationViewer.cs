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
        public InterpolationViewer()
        {
            InitializeComponent();
            CurrentViewport.ViewType = ViewportProjection.Front;
        }

        #region Variables

        public event EventHandler SelectedKeyframeChanged, FrameChanged, SignalChange;

        internal bool _updating = false;

        private KeyframeEntry _selKey = null; //The currently selected keyframe
        private KeyframeEntry _hiKey = null; //The keyframe the mouse is hovering over

        private Vector2? _slopePoint = null;
        private Vector2 _origPos;
        private KeyframeEntry _keyRoot = null; //The first keyframe in the array

        private const float _lineWidth = 1.5f; //The size of lines
        private const float _pointWidth = 5.0f; //The size of points

        private int _frame; //The current frame index
        private int _frameLimit = 0; //The overall number of frames

        private float _xScale; //Width/Frames ratio
        private float _yScale; //Height/Values ratio
        private float _prevX; //Previous mouse point X
        private float _prevY; //Previous mouse point Y
        private float _tanLen = 5.0f; //The length of tangent values
        private float _precision = 4.0f; //The step value for lines drawn to represent interpolation
        private float _minVal = float.MaxValue; //The minimum value in all keyframes
        private float _maxVal = float.MinValue; //The maximum value in all keyframes

        private bool _keyDraggingAllowed = false; //Determines if the user can drag keyframe values across frames or to change the value
        private bool _drawTans = true; //Determines if tangents should be rendered
        private bool _syncStartEnd; //If true, the first and last keyframe values and tangents will be synchronized
        private bool _allKeys = true; //Determines if all keyframes should be rendered, or only the selected one and its neighbors
        private bool _genTans = false; //True if tangents should be automatically generated when affected keyframes change
        private bool _lockIncs = false; //Determines if the scales can be changed on resize
        private bool _dragging = false; //True if the user is dragging a keyframe or tangent slope

        private static bool _alterSelTangent_Drag = true;
        private static bool _alterAdjTangents_KeyFrame_Drag = true;

        #endregion

        #region Properties

        public bool DisplayAllKeyframes
        {
            get { return _allKeys; }
            set
            {
                _allKeys = value;
                Invalidate();
            }
        }
        public float TangentLength
        {
            get { return _tanLen; }
            set
            {
                _tanLen = value;
                Invalidate();
            }
        }
        public bool GenerateTangents { get { return _genTans; } set { _genTans = value; } }
        public bool KeyDraggingAllowed { get { return _keyDraggingAllowed; } set { _keyDraggingAllowed = value; } }
        public bool DrawTangents
        {
            get { return _drawTans; }
            set
            {
                _drawTans = value;
                Invalidate();
            }
        }
        public bool SyncStartEnd { get { return _syncStartEnd; } set { _syncStartEnd = value; } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public KeyframeEntry HighlightedKeyframe { get { return _hiKey; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public float Precision { get { return _precision; } set { _precision = value; Invalidate(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public KeyframeEntry KeyRoot { get { return _keyRoot; } set { _keyRoot = value; Invalidate(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int FrameIndex
        {
            get { return _frame; }
            set
            {
                _frame = value.Clamp(0, _frameLimit);

                Invalidate();

                if (!_updating && FrameChanged != null)
                    FrameChanged(this, null);
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int FrameLimit { get { return _frameLimit; } set { _frameLimit = value; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
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
        public bool AlterSelectedTangent_OnDrag
        {
            get { return _alterSelTangent_Drag; }
            set
            {
                _alterSelTangent_Drag = value;
                if (!_updating)
                    Invalidate();
            }
        }
        public bool AlterAdjTangent_OnSelectedDrag
        {
            get { return _alterAdjTangents_KeyFrame_Drag; }
            set
            {
                _alterAdjTangents_KeyFrame_Drag = value;
                if (!_updating)
                    Invalidate();
            }
        }
        #endregion

        #region Functions
        private bool Has3PlusVals()
        {
            HashSet<float> v = new HashSet<float>();
            if (DisplayAllKeyframes)
            {
                for (KeyframeEntry entry = _keyRoot._next; (entry != _keyRoot); entry = entry._next)
                {
                    v.Add(entry._value);
                    if (v.Count > 3)
                        return true;
                }
            }
            else
            {
                v.Add(SelectedKeyframe._prev._value);
                v.Add(SelectedKeyframe._value);
                v.Add(SelectedKeyframe._next._value);
                if (v.Count > 3)
                    return true;
            }
            return false;
        }

        private int GetKeyframeMaxIndex()
        {
            if (!DisplayAllKeyframes)
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
            if (!DisplayAllKeyframes)
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

        /// <summary>
        /// Returns true if two has a value
        /// </summary>
        /// <param name="index">Float value of current frame. Not normalized [0,1]</param>
        /// <returns></returns>
        private bool GetFrameValue(float index, out float one, out float two)
        {
            KeyframeEntry entry, root = _keyRoot;

            one = 0;
            two = 0;

            if (_keyRoot == null)
                return false;

            //check if index past last frame
            //clamp to last frame if index past last frame
            if (index >= root._prev._index)
            {
                KeyframeEntry e = root._prev;
                if (e._prev._index == e._index)
                {
                    one = e._prev._value;
                    two = e._value;
                    return true;
                }
                else
                {
                    one = e._value;
                    return false;
                }
            }

            //clamp to first frame if index < 0 (root._next == first frame)
            if (index <= root._next._index)
            {
                KeyframeEntry e = root._next;
                if (e._next._index == e._index)
                {
                    one = e._value;
                    two = e._next._value;
                    return true;
                }
                else
                {
                    one = e._value;
                    return false;
                }
            }

            //get keyframe entry  before float index
            //if the input key frame index exists as a keyframe entry,
            //return it's value instead.
            for (entry = root._next; (entry != root) && (entry._index < index); entry = entry._next)
                if (entry._index == index)
                {
                    if (entry._next._index == entry._index)
                    {
                        one = entry._value;
                        two = entry._next._value;
                    }
                    else
                    {
                        one = entry._value;
                        return false;
                    }
                }

            //otherwise, return the interpolated value of the entry prior to the input index.
            one = entry._prev.Interpolate(index - (float)entry._prev._index, false);
            return false;
        }

        public void FindMaxMin()
        {
            if (!DisplayAllKeyframes && SelectedKeyframe == null)
                return;

            _minVal = float.MaxValue;
            _maxVal = float.MinValue;

            int start = GetKeyframeMinIndex();
            int end = GetKeyframeMaxIndex();

            for (float i = start; i <= end; i += (1 / _precision))
                if (i >= 0 && i < _frameLimit)
                {
                    float one, two;
                    bool has2nd = GetFrameValue(i, out one, out two);
                    float v = one;

                    if (v < _minVal)
                        _minVal = v;
                    if (v > _maxVal)
                        _maxVal = v;

                    if (has2nd)
                    {
                        v = two;
                        if (v < _minVal)
                            _minVal = v;
                        if (v > _maxVal)
                            _maxVal = v;
                    }
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
        #endregion

        #region Mouse, Resize

        float ToScreenX(float value)
        {
            return value * _xScale - Width / 2.0f;
        }
        float ToScreenY(float value)
        {
            return value * _yScale - Height / 2.0f;
        }
        float FromScreenX(float value)
        {
            return (value + (Width / 2.0f)) / _xScale;
        }
        float FromScreenY(float value)
        {
            return (value + (Height / 2.0f)) / _yScale;
        }

        bool TrySelect(KeyframeEntry e, int min, float mouseX, float mouseY)
        {
            if (e._index >= 0)
            {
                float keyFrame = (float)e._index;
                Vector3 point = CurrentViewport.Camera.Project(keyFrame - min, e._value - _minVal, 0.0f);

                point._x += Width / 2.0f;
                point._y += Height / 2.0f;

                if (Math.Abs(mouseX - point._x) <= _pointWidth)
                {
                    if (Math.Abs(mouseY - point._y) <= _pointWidth)
                    {
                //if (Math.Abs(mouseX - ToScreenX(keyFrame - min)) <= _pointWidth)
                //{
                //    if (Math.Abs(mouseY - ToScreenY(e._value - _minVal)) <= _pointWidth)
                //    {
                        _hiKey = e;
                        Cursor = Cursors.Hand;
                        Invalidate();
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_selecting)
            {
                _selEnd = new Vector2(e.X, e.Y);
                Invalidate();
            }

            if (_keyRoot == null)
                return;

            float mouseY = e.Y;
            float mouseX = e.X;

            if (_ctx != null && _grabbing)
                lock (_ctx)
                {
                    int xDiff = e.X - _lastX;
                    int yDiff = e.Y - _lastY;

                    Translate(
                        -xDiff * _transFactor,
                        yDiff * _transFactor,
                        0.0f);

                    _lastX = e.X;
                    _lastY = e.Y;

                    return;
                }

            _lastX = e.X;
            _lastY = e.Y;

            //Get nearest frame value
            int frameVal = (int)(FromScreenX(mouseX) + 0.5f);

            int min = GetKeyframeMinIndex();
            if (!_dragging)
            {
                if (_hiKey != null)
                {
                    _hiKey = null;
                    Invalidate();
                }

                Cursor = Cursors.Default;

                if (DisplayAllKeyframes)
                    for (KeyframeEntry entry = _keyRoot._next; (entry != _keyRoot); entry = entry._next)
                        if (TrySelect(entry, min, mouseX, mouseY)) 
                            return;

                if (/*_drawTans && */_selKey != null)
                {
                    if (!DisplayAllKeyframes)
                    {
                        if (TrySelect(SelectedKeyframe._prev, min, mouseX, mouseY)) return;
                        if (TrySelect(SelectedKeyframe, min, mouseX, mouseY)) return;
                        if (TrySelect(SelectedKeyframe._next, min, mouseX, mouseY)) return;
                    }

                    float i1 = -(_tanLen / 2);
                    float i2 = -i1;

                    int xVal2 = _selKey._index;
                    float yVal = _selKey._value;
                    float tan = _selKey._tangent;

                    float p = (float)Math.Sqrt(_precision / 4.0f);
                    Vector2 one = new Vector2(
                        ToScreenX(xVal2 + i1 * p - min),
                        ToScreenY(yVal - _minVal + tan * i1 * p));
                    Vector2 two = new Vector2(
                        ToScreenX(xVal2 + i2 * p - min),
                        ToScreenY(yVal - _minVal + tan * i2 * p));

                    _slopePoint = null;
                    if (Math.Abs(mouseX - one._x) <= _pointWidth && 
                        Math.Abs(mouseY - one._y) <= _pointWidth)
                    {
                        _slopePoint = new Vector2(mouseX, mouseY);
                        _origPos = new Vector2(ToScreenX(xVal2 - min), ToScreenY(yVal - _minVal));
                        _hiKey = _selKey;
                        Cursor = Cursors.Hand;
                        Invalidate();
                        return;
                    }

                    if (Math.Abs(mouseX - two._x) <= _pointWidth && 
                        Math.Abs(mouseY - two._y) <= _pointWidth)
                    {
                        _slopePoint = new Vector2(mouseX, mouseY);
                        _origPos = new Vector2(ToScreenX(xVal2 - min), ToScreenY(yVal - _minVal));
                        _hiKey = _selKey;
                        Cursor = Cursors.Hand;
                        Invalidate();
                        return;
                    }
                }
                if (DisplayAllKeyframes)
                    if (Math.Abs(mouseX - ToScreenX(_frame)) <= _pointWidth)
                        Cursor = Cursors.VSplit;
            }
            else if (_selKey != null && (_keyDraggingAllowed || _slopePoint != null))
            {
                if (_slopePoint != null)
                {
                    int xVal2 = _selKey._index;
                    float yVal = _selKey._value;

                    float xDiff = mouseX - ((Vector2)_slopePoint)._x;
                    float yDiff = mouseY - ((Vector2)_slopePoint)._y;

                    float x2 = ((Vector2)_slopePoint)._x + xDiff;
                    float y2 = ((Vector2)_slopePoint)._y + yDiff;

                    _slopePoint = new Vector2(x2 == _origPos._x ? ((Vector2)_slopePoint)._x : x2, y2);

                    Vector2 x = (Vector2)_slopePoint - _origPos;
                    _selKey._tangent = (float)Math.Round(FromScreenY(x._y) / FromScreenX(x._x), 5);

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
                    float yVal = FromScreenY(mouseY) + _minVal;
                    int xv = frameVal;
                    int xDiff = xv - (int)(_prevX + 0.5f);
                    float yDiff = (yVal - _prevY);

                    int newFrameIndex = _selKey._index + xDiff;
                    bool frameSet =
                        newFrameIndex >= 0 &&
                        newFrameIndex < FrameLimit &&
                        (newFrameIndex >= _selKey._prev._index || _selKey._prev._index < 0) &&
                        (newFrameIndex <= _selKey._next._index || _selKey._next._index < 0);

                    if (newFrameIndex == _selKey._prev._index)
                    {
                        if (_selKey._prev._prev._index == _selKey._prev._index)
                            frameSet = false;
                    }
                    else if (newFrameIndex == _selKey._next._index)
                    {
                        if (_selKey._next._next._index == _selKey._next._index)
                            frameSet = false;
                    }

                    _selKey._value = (float)Math.Round(_selKey._value + yDiff, 3);

                    if (frameSet)
                        FrameIndex = _selKey._index = newFrameIndex;

                    _prevX = xv;
                    _prevY = yVal;

                    if (_genTans)
                    {
                        if (_alterSelTangent_Drag)
                            _selKey.GenerateTangent();

                        if (_alterAdjTangents_KeyFrame_Drag)
                        {
                            _selKey._prev.GenerateTangent();
                            _selKey._next.GenerateTangent();
                        }
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
            else if (frameVal >= 0 && frameVal < _frameLimit && _selKey == null)
                FrameIndex = frameVal;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _grabbing = true;
                base.OnMouseDown(e);
                return;
            }

            bool keyChanged = _selKey != _hiKey;

            if (DisplayAllKeyframes)
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

            if (keyChanged)
            {
                Invalidate();
                if (SelectedKeyframeChanged != null)
                    SelectedKeyframeChanged(this, null);
            }

            if (_allowSelection && !_selecting)
            {
                _selecting = true;
                _selStart = new Vector2(e.X, e.Y);
                _selEnd = _selStart;
                _shiftSelecting = ModifierKeys == Keys.ShiftKey || ModifierKeys == Keys.Shift;
            }
            else if (_selecting && _shiftSelecting)
            {
                _selecting = false;
                _selEnd = new Vector2(e.X, e.Y);
                _shiftSelecting = false;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_selecting && !(ModifierKeys == Keys.ShiftKey || ModifierKeys == Keys.Shift || _shiftSelecting))
            {
                _selEnd = new Vector2(e.X, e.Y);
                _selecting = false;
                Invalidate();
                return;
            }

            if (e.Button == MouseButtons.Right && _grabbing)
            {
                _grabbing = false;
                Invalidate();
                base.OnMouseUp(e);
                return;
            }

            if (_slopePoint != null || (_dragging && _lockIncs))
                Invalidate();

            _dragging = false;
            _lockIncs = false;
            _slopePoint = null;
        }

        protected override void OnResize(EventArgs e)
        {
            if (_ctx != null)
                _ctx.Update();

            _precision = (float)Width / 100.0f;

            base.OnResize(e);
        }

        #endregion
        
        #region Rendering
        unsafe internal override void OnInit(TKContext ctx)
        {
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            GL.LineWidth(_lineWidth);
            GL.PointSize(_pointWidth);
        }

        private void DrawTangent(KeyframeEntry e, float xMin)
        {
            if (e._index < 0)
                return;

            int xVal = e._index;
            float yVal = e._value;
            float tan = e._tangent;

            float i1 = -(_tanLen / 2);
            float i2 = (_tanLen / 2);

            float p = (float)Math.Sqrt(_precision / 4.0f);
            Vector2 one = new Vector2(
                ToScreenX(xVal + i1 * p - xMin),
                ToScreenY(yVal - _minVal + tan * i1 * p));
            Vector2 two = new Vector2(
                ToScreenX(xVal + i2 * p - xMin),
                ToScreenY(yVal - _minVal + tan * i2 * p));

            if (e == _selKey)
            {
                GL.Color4(Color.Purple);
                GL.Begin(PrimitiveType.Points);

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

                GL.Begin(PrimitiveType.LineStrip);
                GL.Vertex2(-7.0f, 3.5f);
                GL.Vertex2(0.0f, 0.0f);
                GL.Vertex2(-7.0f, -3.5f);
                GL.End();

                GL.PopMatrix();

                GL.PushMatrix();
                GL.Translate(two._x, two._y, 0.0f);
                GL.Rotate(angle, 0, 0, 1);

                GL.Begin(PrimitiveType.LineStrip);
                GL.Vertex2(-7.0f, 3.5f);
                GL.Vertex2(0.0f, 0.0f);
                GL.Vertex2(-7.0f, -3.5f);
                GL.End();

                GL.PopMatrix();
            }

            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(one._x, one._y);
            GL.Vertex2(two._x, two._y);
            GL.End();
        }

        protected override void OnRender(PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(Color.LightGray);

            if (_keyRoot == null)
                return;

            Rectangle region = CurrentViewport.Region;

            GL.Viewport(region);
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(region.X, region.Y, region.Width, region.Height);

            CalcXY();

            CurrentViewport.Camera.LoadProjection();
            CurrentViewport.Camera.LoadModelView();

            float one, two;
            bool has2nd;

            int min = 0, max;
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
                else if (_selKey != null/* && !Linear*/)
                    DrawTangent(_selKey, 0);

                //Draw interpolation
                GL.Color4(Color.Red);
                GL.Begin(PrimitiveType.LineStrip);

                for (float i = 0; i < (float)_frameLimit; i += (1 / _precision))
                {
                    has2nd = GetFrameValue(i, out one, out two);
                    GL.Vertex2(ToScreenX(i), ToScreenY(one - _minVal));
                    if (has2nd)
                    {
                        GL.End();
                        GL.Begin(PrimitiveType.LineStrip);
                        GL.Vertex2(ToScreenX(i), ToScreenY(two - _minVal));
                    }
                }
                GL.End();

                //Draw points
                GL.Color4(Color.Black);
                GL.Begin(PrimitiveType.Points);
                for (KeyframeEntry entry = _keyRoot._next; (entry != _keyRoot); entry = entry._next)
                {
                    bool t = false;
                    if (t = (_hiKey == entry || _selKey == entry))
                    {
                        GL.PointSize(_pointWidth * 4.0f);
                        GL.Color4(Color.Orange);
                    }

                    has2nd = GetFrameValue(entry._index, out one, out two);
                    GL.Vertex2(ToScreenX(entry._index), ToScreenY(one - _minVal));
                    if (has2nd)
                        GL.Vertex2(ToScreenX(entry._index), ToScreenY(two - _minVal));

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
                GL.Begin(PrimitiveType.Lines);

                min = GetKeyframeMinIndex();
                max = GetKeyframeMaxIndex();

                //float yv = (GetFrameValue(SelectedKeyframe._index) - _minVal) * _yScale;
                //GL.Vertex2(0.0f, yv);
                //GL.Vertex2(Width, yv);

                GL.End();

                //Draw interpolation
                GL.Color4(Color.Red);
                //GL.Begin(PrimitiveType.LineStrip);
                //for (float i = 0; i <= (float)(max - min); i += (1 / _precision))
                //    GL.Vertex2(i * _xScale, (GetFrameValue(i + min) - _minVal) * _yScale);
                //GL.End();
                
                //Draw tangent
                DrawTangent(SelectedKeyframe, min);
                if (_drawTans)
                {
                    DrawTangent(SelectedKeyframe._prev, min);
                    DrawTangent(SelectedKeyframe._next, min);
                }

                //Draw points
                GL.Color4(Color.Black);
                GL.Begin(PrimitiveType.Points);

                TryDraw(SelectedKeyframe._prev, min);
                TryDraw(SelectedKeyframe, min);
                TryDraw(SelectedKeyframe._next, min);

                GL.End();
            }

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1.0f, 1.0f);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            if (_allKeys)
            {
                //Draw frame indicator
                GL.Color4(Color.Blue);
                if (_frame >= 0 && _frame < _frameLimit)
                {
                    GL.Begin(PrimitiveType.Lines);

                    float r = _frame * _xScale;
                    GL.Vertex2(r, 0.0f);
                    GL.Vertex2(r, Height);

                    GL.End();
                }
            }
            else if (SelectedKeyframe != null)
            {
                float frameX = (SelectedKeyframe._index - min) * _xScale;
                GL.Vertex2(frameX, 0.0f);
                GL.Vertex2(frameX, Height);
            }

            if (_selecting)
            {
                GL.Enable(EnableCap.LineStipple);
                GL.LineStipple(1, 0x0F0F);
                GL.Color4(Color.Gray);
                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex2(_selStart._x, _selStart._y);
                GL.Vertex2(_selEnd._x, _selStart._y);
                GL.Vertex2(_selEnd._x, _selEnd._y);
                GL.Vertex2(_selStart._x, _selEnd._y);
                GL.End();
                GL.Disable(EnableCap.LineStipple);
            }

            GL.Disable(EnableCap.ScissorTest);
        }
        void TryDraw(KeyframeEntry e, float min)
        {
            if (e._index >= 0)
            {
                float one, two;
                bool has2nd = GetFrameValue(e._index, out one, out two);
                GL.Vertex2(ToScreenX(e._index - min), ToScreenY(one - _minVal));
                if (has2nd)
                    GL.Vertex2(ToScreenX(e._index - min), ToScreenY(two - _minVal));
            }
        }
        #endregion

        bool _grabbing = false;
        int _lastX = 0, _lastY = 0;
        float _transFactor = 0.05f;
        float _zoomFactor = 2.5f;
        public bool _allowSelection = true;
        public bool _selecting = false;
        public bool _shiftSelecting;
        public Vector2 _selStart, _selEnd;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!Enabled)
                return;

            Zoom(-(float)e.Delta / 120.0f * _zoomFactor, e.X, e.Y);

            base.OnMouseWheel(e);
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
    }
}