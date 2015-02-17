using BrawlLib.Modeling;
using BrawlLib.OpenGL;
using BrawlLib.SSBB.ResourceNodes;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace System.Windows.Forms
{
    public partial class ModelEditorBase : UserControl
    {
        #region Mouse Down

        #region Targeting

        private void MouseDownTargetBone(MouseEventArgs e, ModelPanel panel)
        {
            //Re-target selected bone
            IBoneNode bone = SelectedBone;
            if (bone != null)
            {
                _snapX = _hiX;
                _snapY = _hiY;
                _snapZ = _hiZ;
                _snapCirc = _hiCirc;

                //Targeting functions are done in HighlightStuff
                if (!(_snapX || _snapY || _snapZ || _snapCirc || _hiSphere))
                {
                    //Orb selection missed. Assign bone and move to next step.
                    SelectedBone = bone = null;
                    goto GetBone;
                }

                //Bone re-targeted. Get frame values and local point aligned to snapping plane.
                Vector3 point;
                if (GetOrbPoint(new Vector2(e.X, e.Y), out point, panel.CurrentViewport))
                {
                    _lastPointLocal = bone.InverseMatrix * (_lastPointWorld = point);
                    if (ControlType == TransformType.Rotation)
                    {
                        _rotating = true;
                        _oldAngles = bone.FrameState._rotate;
                    }
                    else if (ControlType == TransformType.Translation)
                    {
                        _translating = true;
                        _oldPosition = bone.FrameState._translate;
                    }
                    else if (ControlType == TransformType.Scale)
                    {
                        _scaling = true;
                        _oldScale = bone.FrameState._scale;
                    }
                    panel.CurrentViewport.AllowSelection = false;
                    if (_rotating || _translating || _scaling)
                        BoneChange(SelectedBone);
                }
            }

        GetBone:

            //Try selecting new bone
            if (bone == null && panel.RenderBones)
                SelectedBone = _hiBone;
        }

        public void MouseDownTargetVertex(MouseEventArgs e, ModelPanel panel)
        {
            bool ok = false;

            //Re-target selected vertex
            if (VertexLoc() != null)
            {
                _snapX = _hiX;
                _snapY = _hiY;
                _snapZ = _hiZ;
                _snapCirc = _hiCirc;

                if (!(_snapX || _snapY || _snapZ || _snapCirc || _hiSphere))
                {
                    if (NotCtrlAlt)
                        ResetVertexColors();

                    ok = true;
                    goto GetVertex;
                }

                //Vertex re-targeted. Get translation and point (aligned to snapping plane).
                Vector3 point;
                if (GetVertexOrbPoint(new Vector2(e.X, e.Y), ((Vector3)VertexLoc()), out point, panel.CurrentViewport))
                {
                    panel.CurrentViewport.AllowSelection = false;
                    {
                        _translating = true;
                        _oldPosition = ((Vector3)VertexLoc());
                    }
                    _lastPointWorld = point;
                    _lastPointLocal = point * Matrix.TranslationMatrix(-VertexLoc().Value);
                    VertexChange(_selectedVertices);
                }
            }
            else
                ok = true;

        GetVertex:

            if (ok)
            {
                bool verticesChanged = false;
                if (_hiVertex != null)
                {
                    if (Ctrl)
                        if (!_selectedVertices.Contains(_hiVertex))
                        {
                            _selectedVertices.Add(_hiVertex);
                            _hiVertex._selected = true;
                            _hiVertex._highlightColor = Color.Orange;
                            verticesChanged = true;
                        }
                        else
                        {
                            _selectedVertices.Remove(_hiVertex);
                            _hiVertex._selected = false;
                            _hiVertex._highlightColor = Color.Transparent;
                            verticesChanged = true;
                        }
                    else if (Alt)
                        if (_selectedVertices.Contains(_hiVertex))
                        {
                            _selectedVertices.Remove(_hiVertex);
                            _hiVertex._selected = false;
                            _hiVertex._highlightColor = Color.Transparent;
                            verticesChanged = true;
                        }
                        else { }
                    else
                    {
                        ResetVertexColors();
                        if (_hiVertex != null)
                        {
                            _selectedVertices.Add(_hiVertex);
                            _hiVertex._selected = true;
                            _hiVertex._highlightColor = Color.Orange;
                            verticesChanged = true;
                        }
                    }
                }
                else if (NotCtrlAlt)
                    ResetVertexColors();

                if (verticesChanged)
                    OnSelectedVerticesChanged();
            }
        }

        #endregion

        protected virtual void modelPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            ModelPanel panel = sender as ModelPanel;
            ModelPanelViewport viewport = panel.CurrentViewport;

            if (panel._draggingViewports)
                return;

            if (e.Button == Forms.MouseButtons.Left)
            {
                if (DoNotHighlightOnMouseMove)
                {
                    HighlightStuff(e, panel);

                    //Reset the cursor (HighlightStuff sets the cursor)
                    panel.Cursor = Cursors.Default;
                }

                //Reset snap flags
                _snapX = _snapY = _snapZ = _snapCirc = false;

                MouseDownTargetBone(e, panel);
                MouseDownTargetVertex(e, panel);

                //Ensure a redraw so the snapping indicators are correct
                panel.Invalidate();
            }
        }

        #endregion

        #region Mouse Up

        protected virtual void modelPanel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                ModelPanel panel = sender as ModelPanel;

                panel.CurrentViewport.AllowSelection = true;

                if (_rotating || _translating || _scaling)
                    if (VertexLoc() == null)
                        BoneChange(SelectedBone);
                    else
                        VertexChange(_selectedVertices);

                _snapX = _snapY = _snapZ = _snapCirc = false;
                _rotating = _translating = _scaling = false;
            }
        }

        #endregion

        #region Mouse Move

        protected unsafe virtual void modelPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_playing)
                return;

            ModelPanel panel = sender as ModelPanel;
            ModelPanelViewport viewport = panel.CurrentViewport;

            bool moving = _scaling || _rotating || _translating;

            IBoneNode bone = SelectedBone;

            if (moving)
            {
                Vector3 point;
                if (bone != null)
                {
                    if (GetOrbPoint(new Vector2(e.X, e.Y), out point, viewport))
                    {
                        //Convert to local point
                        Vector3 lPoint = bone.InverseMatrix * point;

                        //Check for change in selection.
                        if (_lastPointLocal != lPoint)
                        {
                            switch (ControlType)
                            {
                                case TransformType.Scale:
                                    if (!_snapX) point._x = _lastPointWorld._x;
                                    if (!_snapY) point._y = _lastPointWorld._y;
                                    if (!_snapZ) point._z = _lastPointWorld._z;

                                    if (_snapX && _snapY && _snapZ)
                                    {
                                        //Get scale factor
                                        float scale = (point / _lastPointWorld)._y;

                                        if (scale != 0)
                                        {
                                            ApplyScale(0, scale);
                                            ApplyScale(1, scale);
                                            ApplyScale(2, scale);
                                        }
                                    }
                                    else
                                    {
                                        Vector3 point1 = bone.FrameState._transform * lPoint;
                                        Vector3 point2 = bone.FrameState._transform * _lastPointLocal;

                                        Vector3 scale = (point1 / point2);

                                        if (scale._x != 0.0f) ApplyScale(0, scale._x);
                                        if (scale._y != 0.0f) ApplyScale(1, scale._y);
                                        if (scale._z != 0.0f) ApplyScale(2, scale._z);
                                    }
                                    break;

                                case TransformType.Rotation:

                                    //Get matrix with new rotation applied
                                    Matrix m = bone.FrameState._transform * Matrix.AxisAngleMatrix(_lastPointLocal, lPoint);

                                    //Derive angles from matrices, get difference
                                    Vector3 angles = (m.GetAngles() - bone.FrameState._transform.GetAngles()).RemappedToRange(-180.0f, 180.0f);

                                    //Apply difference to axes that have changed (pnlAnim should handle this so keyframes are created)
                                    if (angles._x != 0.0f) ApplyAngle(0, angles._x);
                                    if (angles._y != 0.0f) ApplyAngle(1, angles._y);
                                    if (angles._z != 0.0f) ApplyAngle(2, angles._z);
                                    break;

                                case TransformType.Translation:

                                    if (!_snapX) point._x = _lastPointWorld._x;
                                    if (!_snapY) point._y = _lastPointWorld._y;
                                    if (!_snapZ) point._z = _lastPointWorld._z;

                                    lPoint = bone.InverseMatrix * point;

                                    Vector3 trans = (bone.FrameState._transform * lPoint - bone.FrameState._transform * _lastPointLocal);

                                    if (trans._x != 0.0f) ApplyTranslation(0, trans._x);
                                    if (trans._y != 0.0f) ApplyTranslation(1, trans._y);
                                    if (trans._z != 0.0f) ApplyTranslation(2, trans._z);
                                    break;
                            }

                            _lastPointWorld = point;
                            _lastPointLocal = bone.InverseMatrix * point;
                        }
                    }
                }
                else if (VertexLoc() != null && GetVertexOrbPoint(new Vector2(e.X, e.Y), ((Vector3)VertexLoc()), out point, viewport) && point != _lastPointWorld)
                {
                    Vector3 center = VertexLoc().Value;

                    //Convert to local point
                    Vector3 lPoint = Matrix.TranslationMatrix(-center) * point;

                    if (_lastPointLocal != lPoint)
                    {
                                Vector3 trans = point - _lastPointWorld;
                                foreach (Vertex3 vertex in _selectedVertices)
                                {
                                    Vector3 pos = vertex._weightedPosition;
                                    if (_snapX) pos._x += trans._x;
                                    if (_snapY) pos._y += trans._y;
                                    if (_snapZ) pos._z += trans._z;
                                    if (pos != vertex._weightedPosition)
                                        vertex.WeightedPosition = pos;
                                }
                    }
                    _vertexLoc = null;

                    UpdateModel();

                    _lastPointLocal = lPoint;
                    _lastPointWorld = point;
                }
            }

            if (!moving && (!DoNotHighlightOnMouseMove || (DoNotHighlightOnMouseMove && panel.CurrentViewport.Selecting)))
                HighlightStuff(e, panel);
        }
        #endregion

        #region Highlighting

        #region Targeting
        private void MouseMoveTargetBone(ModelPanel panel, MouseEventArgs e, float depth, ModelPanelViewport v)
        {
            IBoneNode bone = SelectedBone;
            if (bone != null)
            {
                //Get the location of the bone
                Vector3 center = BoneLoc(bone);

                //Standard radius scaling snippet. This is used for orb scaling depending on camera distance.
                float radius = CamDistance(center, v);

                if (ControlType == TransformType.Rotation)
                {
                    //Get point projected onto our orb.
                    Vector3 point = v.ProjectCameraSphere(new Vector2(e.X, e.Y), center, radius, false);

                    //Get the distance of the mouse point from the bone
                    float distance = point.TrueDistance(center);

                    if (Math.Abs(distance - radius) < (radius * _selectOrbScale)) //Point lies within orb radius
                    {
                        _hiSphere = true;

                        //Determine axis snapping
                        Vector3 angles = (bone.InverseMatrix * point).GetAngles() * Maths._rad2degf;
                        angles._x = (float)Math.Abs(angles._x);
                        angles._y = (float)Math.Abs(angles._y);
                        angles._z = (float)Math.Abs(angles._z);

                        if (Math.Abs(angles._y - 90.0f) <= _axisSnapRange)
                            _hiX = true;
                        else if (angles._x >= (180 - _axisSnapRange) || angles._x <= _axisSnapRange)
                            _hiY = true;
                        else if (angles._y >= (180 - _axisSnapRange) || angles._y <= _axisSnapRange)
                            _hiZ = true;
                    }
                    else if (Math.Abs(distance - (radius * _circOrbScale)) < (radius * _selectOrbScale)) //Point lies on circ line
                        _hiCirc = true;

                    if (_hiX || _hiY || _hiZ || _hiCirc)
                        panel.Cursor = Cursors.Hand;
                }
                else if (ControlType == TransformType.Translation)
                {
                    Vector3 point = v.UnProject(e.X, e.Y, depth);
                    Vector3 diff = (point - center) / radius;

                    float halfDist = _axisHalfLDist;
                    if (diff._x > -_axisSelectRange && diff._x < (_axisLDist + 0.01f) &&
                        diff._y > -_axisSelectRange && diff._y < (_axisLDist + 0.01f) &&
                        diff._z > -_axisSelectRange && diff._z < (_axisLDist + 0.01f))
                    {
                        //Point lies within axes
                        if (diff._x < halfDist && diff._y < halfDist && diff._z < halfDist)
                        {
                            //Point lies inside the double drag areas
                            if (diff._x > _axisSelectRange)
                                _hiX = true;
                            if (diff._y > _axisSelectRange)
                                _hiY = true;
                            if (diff._z > _axisSelectRange)
                                _hiZ = true;

                            panel.Cursor = Cursors.Hand;
                        }
                        else
                        {
                            //Check if point lies on a specific axis
                            float errorRange = _axisSelectRange;

                            if (diff._x > halfDist && Math.Abs(diff._y) < errorRange && Math.Abs(diff._z) < errorRange)
                                _hiX = true;
                            if (diff._y > halfDist && Math.Abs(diff._x) < errorRange && Math.Abs(diff._z) < errorRange)
                                _hiY = true;
                            if (diff._z > halfDist && Math.Abs(diff._x) < errorRange && Math.Abs(diff._y) < errorRange)
                                _hiZ = true;

                            if (!_hiX && !_hiY && !_hiZ)
                                goto GetBone;
                            else
                                panel.Cursor = Cursors.Hand;
                        }
                    }
                    else
                        goto GetBone;
                }
                else if (ControlType == TransformType.Scale)
                {
                    Vector3 point = v.UnProject(e.X, e.Y, depth);
                    Vector3 diff = (point - center) / radius;

                    if (diff._x > -_axisSelectRange && diff._x < (_axisLDist + 0.01f) &&
                        diff._y > -_axisSelectRange && diff._y < (_axisLDist + 0.01f) &&
                        diff._z > -_axisSelectRange && diff._z < (_axisLDist + 0.01f))
                    {
                        //Point lies within axes

                        //Check if point lies on a specific axis first
                        float errorRange = _axisSelectRange;

                        if (diff._x > errorRange && Math.Abs(diff._y) < errorRange && Math.Abs(diff._z) < errorRange)
                            _hiX = true;
                        if (diff._y > errorRange && Math.Abs(diff._x) < errorRange && Math.Abs(diff._z) < errorRange)
                            _hiY = true;
                        if (diff._z > errorRange && Math.Abs(diff._x) < errorRange && Math.Abs(diff._y) < errorRange)
                            _hiZ = true;

                        if (!_hiX && !_hiY && !_hiZ)
                        {
                            //Determine if the point is in the double or triple drag triangles
                            float halfDist = _scaleHalf2LDist;
                            float centerDist = _scaleHalf1LDist;
                            if (diff.IsInTriangle(new Vector3(), new Vector3(halfDist, 0, 0), new Vector3(0, halfDist, 0)))
                                if (diff.IsInTriangle(new Vector3(), new Vector3(centerDist, 0, 0), new Vector3(0, centerDist, 0)))
                                    _hiX = _hiY = _hiZ = true;
                                else _hiX = _hiY = true;
                            else if (diff.IsInTriangle(new Vector3(), new Vector3(halfDist, 0, 0), new Vector3(0, 0, halfDist)))
                                if (diff.IsInTriangle(new Vector3(), new Vector3(centerDist, 0, 0), new Vector3(0, 0, centerDist)))
                                    _hiX = _hiY = _hiZ = true;
                                else _hiX = _hiZ = true;
                            else if (diff.IsInTriangle(new Vector3(), new Vector3(0, halfDist, 0), new Vector3(0, 0, halfDist)))
                                if (diff.IsInTriangle(new Vector3(), new Vector3(0, centerDist, 0), new Vector3(0, 0, centerDist)))
                                    _hiX = _hiY = _hiZ = true;
                                else _hiY = _hiZ = true;

                            if (!_hiX && !_hiY && !_hiZ)
                                goto GetBone;
                            else
                                panel.Cursor = Cursors.Hand;
                        }
                        else
                            panel.Cursor = Cursors.Hand;
                    }
                    else
                        goto GetBone;
                }
            }

            //modelPanel1.Invalidate();

        GetBone:

            //Try selecting new bone
            //if (modelPanel._selecting)
            //{

            //}
            //else
            {
                if (!(_scaling || _rotating || _translating) && depth < 1.0f)
                {
                    IBoneNode o = null;

                    Vector3 point = v.UnProject(e.X, e.Y, depth);

                    //Find orb near chosen point
                    if (EditingAll)
                    {
                        foreach (IModel m in _targetModels)
                            foreach (IBoneNode b in m.RootBones)
                                if (CompareDistanceRecursive(b, point, ref o))
                                    break;
                    }
                    else if (_targetModel != null)
                        foreach (IBoneNode b in _targetModel.RootBones)
                            if (CompareDistanceRecursive(b, point, ref o))
                                break;

                    if (_hiBone != null && _hiBone != SelectedBone)
                        _hiBone.NodeColor = Color.Transparent;

                    if ((_hiBone = o) != null)
                    {
                        _hiBone.NodeColor = Color.FromArgb(255, 128, 0);
                        panel.Cursor = Cursors.Hand;
                    }
                }
                else if (_hiBone != null)
                {
                    if (_hiBone != SelectedBone)
                        _hiBone.NodeColor = Color.Transparent;
                    _hiBone = null;
                }
            }
        }
        private void MouseMoveTargetVertex(ModelPanel panel, MouseEventArgs e, float depth, ModelPanelViewport v)
        {
            if (VertexLoc() != null && v._renderAttrib._renderVertices)
            {
                float radius = VertexOrbRadius(v);
                Vector3 center = VertexLoc().Value;

                {
                    //Point lies within axes
                    if (diff._x < halfDist && diff._y < halfDist && diff._z < halfDist)
                    {
                        //Point lies inside the double drag areas
                        if (diff._x > _axisSelectRange)
                            _hiX = true;
                        if (diff._y > _axisSelectRange)
                            _hiY = true;
                        if (diff._z > _axisSelectRange)
                            _hiZ = true;

                        panel.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        //Check if point lies on a specific axis
                        float errorRange = _axisSelectRange;

                        if (diff._x > halfDist && Math.Abs(diff._y) < errorRange && Math.Abs(diff._z) < errorRange)
                            _hiX = true;
                        if (diff._y > halfDist && Math.Abs(diff._x) < errorRange && Math.Abs(diff._z) < errorRange)
                            _hiY = true;
                        if (diff._z > halfDist && Math.Abs(diff._x) < errorRange && Math.Abs(diff._y) < errorRange)
                            _hiZ = true;
                {
                    Vector3 point = v.UnProject(e.X, e.Y, depth);
                    Vector3 diff = (point - center) / radius;

                    float halfDist = _axisHalfLDist;
                    if (diff._x > -_axisSelectRange && diff._x < (_axisLDist + 0.01f) &&
                        diff._y > -_axisSelectRange && diff._y < (_axisLDist + 0.01f) &&
                        diff._z > -_axisSelectRange && diff._z < (_axisLDist + 0.01f))
                    {
                        //Point lies within axes
                        if (diff._x < halfDist && diff._y < halfDist && diff._z < halfDist)
                        {
                            //Point lies inside the double drag areas
                            if (diff._x > _axisSelectRange)
                                _hiX = true;
                            if (diff._y > _axisSelectRange)
                                _hiY = true;
                            if (diff._z > _axisSelectRange)
                                _hiZ = true;

                            panel.Cursor = Cursors.Hand;
                        }
                        else
                        {
                            //Check if point lies on a specific axis
                            float errorRange = _axisSelectRange;

                            if (diff._x > halfDist && Math.Abs(diff._y) < errorRange && Math.Abs(diff._z) < errorRange)
                                _hiX = true;
                            if (diff._y > halfDist && Math.Abs(diff._x) < errorRange && Math.Abs(diff._z) < errorRange)
                                _hiY = true;
                            if (diff._z > halfDist && Math.Abs(diff._x) < errorRange && Math.Abs(diff._y) < errorRange)
                                _hiZ = true;

                            if (!_hiX && !_hiY && !_hiZ)
                                goto GetVertex;
                            else
                                panel.Cursor = Cursors.Hand;
                        }
                    }
                }
            }

        GetVertex:

            //Try targeting a vertex
            if (RenderVertices)
            {
                if (panel.CurrentViewport.Selecting)
                {
                    if (NotCtrlAlt)
                        ResetVertexColors();

                    bool selected = false;
                    if (TargetModel != null)
                        if (TargetModel.SelectedObjectIndex < 0)
                        {
                            foreach (IObject o in TargetModel.Objects)
                                if (o.IsRendering)
                                {
                                    SelectVertices(o, panel);
                                    selected = true;
                                }
                        }
                        else
                        {
                            IObject w = TargetModel.Objects[TargetModel.SelectedObjectIndex];
                            if (w.IsRendering)
                            {
                                SelectVertices(w, panel);
                                selected = true;
                            }
                            else
                                foreach (IObject h in TargetModel.Objects)
                                    if (h.IsRendering)
                                    {
                                        SelectVertices(h, panel);
                                        selected = true;
                                    }
                        }
                    else if (_targetModels != null)
                    {
                        foreach (IModel m in _targetModels)
                        {
                            if (m.SelectedObjectIndex < 0)
                            {
                                foreach (IObject o in m.Objects)
                                    if (o.IsRendering)
                                    {
                                        SelectVertices(o, panel);
                                        selected = true;
                                    }
                            }
                            else
                            {
                                IObject w = m.Objects[m.SelectedObjectIndex];
                                if (w.IsRendering)
                                {
                                    SelectVertices(w, panel);
                                    selected = true;
                                }
                                else
                                    foreach (IObject h in m.Objects)
                                        if (h.IsRendering)
                                        {
                                            SelectVertices(h, panel);
                                            selected = true;
                                        }
                            }
                        }
                    }
                    if (selected)
                        OnSelectedVerticesChanged();
                }
                else
                {
                    if (depth < 1.0f && _targetModel != null)
                    {
                        Vector3 point = v.UnProject(e.X, e.Y, depth);
                        Vertex3 vertex = CompareVertexDistance(point);
                        if (_hiVertex != null && !_hiVertex._selected)
                        {
                            _hiVertex._highlightColor = Color.Transparent;
                            ModelPanel.CurrentViewport.AllowSelection = true;
                        }
                        if ((_hiVertex = vertex) != null)
                        {
                            _hiVertex._highlightColor = Color.Orange;
                            panel.Cursor = Cursors.Cross;
                            ModelPanel.CurrentViewport.AllowSelection = false;
                        }
                    }
                    else if (_hiVertex != null)
                    {
                        if (!_hiVertex._selected)
                        {
                            _hiVertex._highlightColor = Color.Transparent;
                            ModelPanel.CurrentViewport.AllowSelection = true;
                        }
                        _hiVertex = null;
                    }
                }
            }
        }
        #endregion

        public IBoneNode _hiBone = null;
        public Vertex3 _hiVertex = null;
        public void HighlightStuff(MouseEventArgs e, ModelPanel panel)
        {
            panel.Capture();

            _hiX = _hiY = _hiZ = _hiCirc = _hiSphere = false;

            float depth = panel.GetDepth(e.X, e.Y);
            ModelPanelViewport v = panel.HighlightedViewport;

#if DEBUG
            v.ScreenText["Depth: " + depth.ToString()] = new Vector3(5.0f, v.Height - 20.0f, 0.5f);
#endif
            MouseMoveTargetBone(panel, e, depth, v);
            MouseMoveTargetVertex(panel, e, depth, v);

            panel.Invalidate();
        }
        /// <summary>
        /// Does not call OnSelectedVerticesChanged()!
        /// </summary>
        private void SelectVertices(IObject o, ModelPanel panel)
        {
            foreach (Vertex3 v in o.Vertices)
            {
                //Project each vertex into screen coordinates.
                //Then check to see if the 2D coordinates lie within the selection box.
                //In Soviet Russia, vertices come to YOUUUUU

                Vector3 worldPos = v.WeightedPosition;
                Vector2 screenPos = (Vector2)panel.CurrentViewport.Camera.Project(worldPos);
                Drawing.Point start = panel.CurrentViewport.SelectionStart, end = panel.CurrentViewport.SelectionEnd;
                Vector2 min = new Vector2((float)Math.Min(start.X, end.X), (float)Math.Min(start.Y, end.Y));
                Vector2 max = new Vector2((float)Math.Max(start.X, end.X), (float)Math.Max(start.Y, end.Y));
                if (screenPos <= max && screenPos >= min)
                    if (Alt)
                    {
                        v._selected = false;
                        if (_selectedVertices.Contains(v))
                            _selectedVertices.Remove(v);
                        v._highlightColor = Color.Transparent;
                    }
                    else if (!v._selected)
                    {
                        v._selected = true;

                        if (!Ctrl || !_selectedVertices.Contains(v))
                            _selectedVertices.Add(v);
                        v._highlightColor = Color.Orange;
                    }
            }
        }
        public void ResetBoneColors()
        {
            if (_targetModels != null)
                foreach (IModel m in _targetModels)
                    foreach (IBoneNode b in m.BoneCache)
                        b.BoneColor = b.NodeColor = Color.Transparent;
        }

        public void ResetVertexColors()
        {
            if (_targetModels != null)
                foreach (IModel m in _targetModels)
                    foreach (IObject o in m.Objects)
                        if (o.Vertices != null)
                            foreach (Vertex3 v in o.Vertices)
                            {
                                v._highlightColor = Color.Transparent;
                                v._selected = false;
                            }
            _selectedVertices = new List<Vertex3>();
        }
        #endregion
    }
}