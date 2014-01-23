using System;
using BrawlLib.OpenGL;
using System.ComponentModel;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;
using BrawlLib.Modeling;
using System.Drawing;
using BrawlLib.Wii.Animations;
using System.Collections.Generic;
using BrawlLib.SSBBTypes;
using BrawlLib.IO;
using BrawlLib;
using System.Drawing.Imaging;
using Gif.Components;
using OpenTK.Graphics.OpenGL;
using BrawlLib.Imaging;
using System.Windows.Forms;

namespace Ikarus.UI
{
    public partial class MainControl : UserControl, IMainWindow
    {
        #region Mouse Down

        public bool Ctrl { get { return (ModifierKeys & Keys.Control) == Keys.Control; } }
        public bool Alt { get { return (ModifierKeys & Keys.Alt) == Keys.Alt; } }
        public bool Shift { get { return (ModifierKeys & Keys.Shift) == Keys.Shift; } }
        public bool CtrlAlt { get { return Ctrl && Alt; } }
        public bool NotCtrlAlt { get { return !Ctrl && !Alt; } }

        private void modelPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (dontHighlightBonesAndVerticesToolStripMenuItem.Checked)
                {
                    HighlightStuff(e);
                    modelPanel.Cursor = Cursors.Default;
                }

                //Reset snap flags
                _snapX = _snapY = _snapZ = _snapCirc = false;

                MDL0BoneNode bone = SelectedBone;
                MDL0ObjectNode poly = modelListsPanel1.SelectedPolygon;

                //Re-target selected bone
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
                    if (GetOrbPoint(new Vector2(e.X, e.Y), out point))
                    {
                        _firstPointBone = _lastPointBone = bone._inverseFrameMatrix * (_lastPointWorld = _firstPointWorld = point);
                        if (_editType == TransformType.Rotation)
                        {
                            _rotating = true;
                            _oldAngles = bone._frameState._rotate;
                        }
                        else if (_editType == TransformType.Translation)
                        {
                            _translating = true;
                            _oldPosition = bone._frameState._translate;
                        }
                        else if (_editType == TransformType.Scale)
                        {
                            _scaling = true;
                            _oldScale = bone._frameState._scale;
                        }
                        modelPanel._forceNoSelection = true;
                        if (_rotating || _translating || _scaling)
                            BoneChange(SelectedBone);
                    }
                }

            GetBone:

                //Try selecting new bone
                if (bone == null)
                    SelectedBone = _hiBone;

                //Ensure a redraw so the snapping indicators are correct
                modelPanel.Invalidate();
            }
        }

        public bool IsInTriangle(Vector3 point, Vector3 triPt1, Vector3 triPt2, Vector3 triPt3)
        {
            Vector3 v0 = triPt2 - triPt1;
            Vector3 v1 = triPt3 - triPt1;
            Vector3 v2 = point - triPt1;

            float dot00 = v0.Dot(v0);
            float dot01 = v0.Dot(v1);
            float dot02 = v0.Dot(v2);
            float dot11 = v1.Dot(v1);
            float dot12 = v1.Dot(v2);

            //Get barycentric coordinates
            float d = (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) / d;
            float v = (dot00 * dot12 - dot01 * dot02) / d;

            return u >= 0 && v >= 0 && u + v < 1;
        }

        #endregion

        #region Mouse Up

        private void modelPanel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //if (_rotating) btnUndo.Enabled = true;

                if (_rotating || _translating || _scaling)
                    BoneChange(SelectedBone);

                _snapX = _snapY = _snapZ = _snapCirc = false;
                _rotating = _translating = _scaling = false;
                modelPanel._forceNoSelection = false;
                //if (modelPanel1._selecting)
                    modelPanel._selecting = false;
            }
        }

        #endregion

        #region Mouse Move

        private unsafe void modelPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_playing)
                return;

            //modelPanel.BeginUpdate();

            bool moving = _scaling || _rotating || _translating;

            MDL0BoneNode bone = SelectedBone;
            //Vertex3 vertex = TargetVertex;

            if (moving)
            {
                Vector3 point;
                if (bone != null)
                {
                    if (GetOrbPoint(new Vector2(e.X, e.Y), out point))
                    {
                        //Convert to local point
                        Vector3 lPoint = bone._inverseFrameMatrix * point;

                        //Check for change in selection.
                        if (_lastPointBone != lPoint)
                        {
                            if (_editType == TransformType.Rotation)
                            {
                                //Get matrix with new rotation applied
                                Matrix m = bone._frameState._transform * Matrix.AxisAngleMatrix(_lastPointBone, lPoint);

                                //Derive angles from matrices, get difference
                                Vector3 angles = m.GetAngles() - bone._frameState._transform.GetAngles();

                                //Truncate (allows winding)
                                if (angles._x > 180.0f) angles._x -= 360.0f;
                                if (angles._y > 180.0f) angles._y -= 360.0f;
                                if (angles._z > 180.0f) angles._z -= 360.0f;
                                if (angles._x < -180.0f) angles._x += 360.0f;
                                if (angles._y < -180.0f) angles._y += 360.0f;
                                if (angles._z < -180.0f) angles._z += 360.0f;

                                //Apply difference to axes that have changed (pnlAnim should handle this so keyframes are created)
                                if (angles._x != 0.0f) ApplyAngle(0, angles._x);
                                if (angles._y != 0.0f) ApplyAngle(1, angles._y);
                                if (angles._z != 0.0f) ApplyAngle(2, angles._z);
                            }
                            else if (_editType == TransformType.Translation)
                            {
                                //Get difference
                                if (!_snapX) point._x = _lastPointWorld._x;
                                if (!_snapY) point._y = _lastPointWorld._y;
                                if (!_snapZ) point._z = _lastPointWorld._z;

                                lPoint = bone._inverseFrameMatrix * point;

                                Vector3 trans = (bone._frameState._transform * lPoint - bone._frameState._transform * _lastPointBone);

                                if (trans._x != 0.0f) ApplyTranslation(0, trans._x);
                                if (trans._y != 0.0f) ApplyTranslation(1, trans._y);
                                if (trans._z != 0.0f) ApplyTranslation(2, trans._z);
                            }
                            else if (_editType == TransformType.Scale)
                            {
                                if (!_snapX) point._x = _lastPointWorld._x;
                                if (!_snapY) point._y = _lastPointWorld._y;
                                if (!_snapZ) point._z = _lastPointWorld._z;

                                lPoint = bone._inverseFrameMatrix * point;

                                if (_snapX && _snapY && _snapZ)
                                {
                                    //Get scale factor
                                    float scale = (lPoint / _firstPointBone)._y;

                                    if (scale != 0)
                                    {
                                        ApplyScale(0, scale);
                                        ApplyScale(1, scale);
                                        ApplyScale(2, scale);
                                    }
                                }
                                else
                                {
                                    //Get scale factor
                                    //Vector3 scale = (lPoint / _firstPointBone);

                                    Vector3 scale = (bone._frameState._transform * lPoint - bone._frameState._transform * _lastPointBone);

                                    //Vector3 scale2 = bone._frameState._transform *  (bone._inverseFrameMatrix * (point - _lastPointWorld));

                                    if (scale._x != 0.0f)
                                        ApplyScale2(0, scale._x);
                                    if (scale._y != 0.0f)
                                        ApplyScale2(1, scale._y);
                                    if (scale._z != 0.0f)
                                        ApplyScale2(2, scale._z);
                                }
                            }
                            _lastPointWorld = point;
                            _lastPointBone = bone._inverseFrameMatrix * point;
                        }
                    }
                }
            }

            //modelPanel.EndUpdate();

            if (!moving && (!dontHighlightBonesAndVerticesToolStripMenuItem.Checked || (dontHighlightBonesAndVerticesToolStripMenuItem.Checked && modelPanel._selecting)))
                HighlightStuff(e);

            //if (RenderBones || RenderVertices)
            //    modelPanel.Invalidate();
        }
        public MDL0BoneNode _hiBone = null;
        public Vertex3 _hiVertex = null;
        public void HighlightStuff(MouseEventArgs e)
        {
            modelPanel.Cursor = Cursors.Default;
            float depth = modelPanel.GetDepth(e.X, e.Y);

            _hiX = _hiY = _hiZ = _hiCirc = _hiSphere = false;
            
            MDL0BoneNode bone = SelectedBone;
            //Vertex3 vertex = TargetVertex;
            MDL0ObjectNode poly = modelListsPanel1.SelectedPolygon;

            if (bone != null)
            {
                //Get the location of the bone
                Vector3 center = BoneLoc;

                //Standard radius scaling snippet. This is used for orb scaling depending on camera distance.
                float radius = center.TrueDistance(modelPanel._camera.GetPoint()) / _orbRadius * 0.1f;

                if (_editType == TransformType.Rotation)
                {
                    //Get point projected onto our orb.
                    Vector3 point = modelPanel.ProjectCameraSphere(new Vector2(e.X, e.Y), center, radius, false);

                    //Get the distance of the mouse point from the bone
                    float distance = point.TrueDistance(center);

                    if (Math.Abs(distance - radius) < (radius * _selectOrbScale)) //Point lies within orb radius
                    {
                        _hiSphere = true;

                        //Determine axis snapping
                        Vector3 angles = (bone._inverseFrameMatrix * point).GetAngles() * Maths._rad2degf;
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
                        modelPanel.Cursor = Cursors.Hand;
                }
                else if (_editType == TransformType.Translation)
                {
                    Vector3 point = modelPanel.UnProject(e.X, e.Y, depth);
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

                            modelPanel.Cursor = Cursors.Hand;
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
                                modelPanel.Cursor = Cursors.Hand;
                        }
                    }
                    else
                        goto GetBone;
                }
                else if (_editType == TransformType.Scale)
                {
                    Vector3 point = modelPanel.UnProject(e.X, e.Y, depth);
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
                            if (IsInTriangle(diff, new Vector3(), new Vector3(halfDist, 0, 0), new Vector3(0, halfDist, 0)))
                                if (IsInTriangle(diff, new Vector3(), new Vector3(centerDist, 0, 0), new Vector3(0, centerDist, 0)))
                                    _hiX = _hiY = _hiZ = true;
                                else _hiX = _hiY = true;
                            else if (IsInTriangle(diff, new Vector3(), new Vector3(halfDist, 0, 0), new Vector3(0, 0, halfDist)))
                                if (IsInTriangle(diff, new Vector3(), new Vector3(centerDist, 0, 0), new Vector3(0, 0, centerDist)))
                                    _hiX = _hiY = _hiZ = true;
                                else _hiX = _hiZ = true;
                            else if (IsInTriangle(diff, new Vector3(), new Vector3(0, halfDist, 0), new Vector3(0, 0, halfDist)))
                                if (IsInTriangle(diff, new Vector3(), new Vector3(0, centerDist, 0), new Vector3(0, 0, centerDist)))
                                    _hiX = _hiY = _hiZ = true;
                                else _hiY = _hiZ = true;

                            if (!_hiX && !_hiY && !_hiZ)
                                goto GetBone;
                            else
                                modelPanel.Cursor = Cursors.Hand;
                        }
                        else
                            modelPanel.Cursor = Cursors.Hand;
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
                if (!(_scaling || _rotating || _translating) && (depth < 1.0f) && (_targetModel != null) && (_targetModel._boneList != null))
                {
                    MDL0BoneNode o = null;

                    Vector3 point = modelPanel.UnProject(e.X, e.Y, depth);

                    //Find orb near chosen point
                    if (_editingAll)
                    {
                        foreach (MDL0Node m in _targetModels)
                            foreach (MDL0BoneNode b in m._boneList)
                                if (CompareDistanceRecursive(b, point, ref o))
                                    break;
                    }
                    else
                        foreach (MDL0BoneNode b in _targetModel._boneList)
                            if (CompareDistanceRecursive(b, point, ref o))
                                break;

                    if (_hiBone != null && _hiBone != SelectedBone)
                        _hiBone._nodeColor = Color.Transparent;
                    
                    if ((_hiBone = o) != null)
                    {
                        _hiBone._nodeColor = Color.FromArgb(255, 128, 0);
                        modelPanel.Cursor = Cursors.Hand;
                    }
                }
                else if (_hiBone != null)
                {
                    if (_hiBone != SelectedBone)
                        _hiBone._nodeColor = Color.Transparent;
                    _hiBone = null;
                }
            }
            
            modelPanel.Invalidate();
        }
        private void SelectVerts(MDL0ObjectNode o)
        {
            foreach (Vertex3 v in o._manager._vertices)
            {
                //Project each vertex into screen coordinates.
                //Then check to see if the 2D coordinates lie within the selection box.
                //In Soviet Russia, vertices come to YOUUUUU

                Vector3 vec3 = v.WeightedPosition;
                Vector2 vec2 = (Vector2)modelPanel.Project(vec3);
                Point start = modelPanel._selStart, end = modelPanel._selEnd;
                Vector2 min = new Vector2(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
                Vector2 max = new Vector2(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y));
                if ((vec2 <= max) && (vec2 >= min))
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
            foreach (MDL0Node m in _targetModels)
                if (m._linker != null && m._linker.BoneCache != null)
                    foreach (MDL0BoneNode b in m._linker.BoneCache)
                        b._boneColor = b._nodeColor = Color.Transparent;
        }
        
        public List<Vertex3> _selectedVertices = new List<Vertex3>();
        public void ResetVertexColors()
        {
            //foreach (Vertex3 v in _selectedVertices)
            if (_targetModels != null)
            foreach (MDL0Node m in _targetModels)
                if (m._objList != null)
                    foreach (MDL0ObjectNode o in m._objList)
                        if (o._manager != null)
                            foreach (Vertex3 v in o._manager._vertices)
                            {
                                v._highlightColor = Color.Transparent;
                                v._selected = false;
                            }
        }
        #endregion
    }
}
