using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Windows.Forms
{
    public partial class ModelEditControl : ModelEditorBase
    {
        protected override void ModelPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_collisionEditorMode)
                base.ModelPanel_MouseDown(sender, e);
            else if (e.Button == MouseButtons.Left)
            {
                bool create = Control.ModifierKeys == Keys.Alt;
                bool add = Control.ModifierKeys == Keys.Shift;
                bool subtract = Control.ModifierKeys == Keys.Control;
                bool move = Control.ModifierKeys == (Keys.Control | Keys.Shift);

                float depth = ModelPanel.GetDepth(e.X, e.Y);
                Vector3 target = ModelPanel.CurrentViewport.UnProject(e.X, e.Y, depth);
                Vector2 point;

                if (!move && (depth < 1.0f))
                {
                    point = (Vector2)target;

                    //Hit-detect points first
                    foreach (CollisionObject obj in _targetCollisionNode._objects)
                        if (obj._render)
                            foreach (CollisionLink p in obj._points)
                                if (p.Value.Contained(point, point, PointSelectRadius))
                                {
                                    if (create)
                                    {
                                        //Connect all selected links to point
                                        foreach (CollisionLink l in _selectedLinks)
                                            l.Connect(p);

                                        //Select point
                                        ClearCollisionSelection();
                                        p._highlight = true;
                                        _selectedLinks.Add(p);
                                        SelectionModified();

                                        ModelPanel.Invalidate();
                                        return;
                                    }

                                    if (subtract)
                                    {
                                        p._highlight = false;
                                        _selectedLinks.Remove(p);
                                        ModelPanel.Invalidate();
                                        SelectionModified();
                                    }
                                    else if (!_selectedLinks.Contains(p))
                                    {
                                        if (!add)
                                            ClearCollisionSelection();

                                        _selectedLinks.Add(p);
                                        p._highlight = true;
                                        ModelPanel.Invalidate();
                                        SelectionModified();
                                    }

                                    if ((!add) && (!subtract))
                                        BeginHover(target);
                                    //Single Link Selected
                                    return;
                                }

                    float dist;
                    float bestDist = float.MaxValue;
                    CollisionPlane bestMatch = null;

                    //Hit-detect planes finding best match
                    foreach (CollisionObject obj in _targetCollisionNode._objects)
                        if (obj._render)
                            foreach (CollisionPlane p in obj._planes)
                                if (point.Contained(p.PointLeft, p.PointRight, PointSelectRadius))
                                {
                                    dist = point.TrueDistance(p.PointLeft) + point.TrueDistance(p.PointRight) - p.PointLeft.TrueDistance(p.PointRight);
                                    if (dist < bestDist)
                                    { bestDist = dist; bestMatch = p; }
                                }

                    if (bestMatch != null)
                    {
                        if (create)
                        {
                            ClearCollisionSelection();

                            _selectedLinks.Add(bestMatch.Split(point));
                            _selectedLinks[0]._highlight = true;
                            SelectionModified();
                            ModelPanel.Invalidate();

                            _creating = true;
                            BeginHover(target);

                            return;
                        }

                        if (subtract)
                        {
                            _selectedLinks.Remove(bestMatch._linkLeft);
                            _selectedLinks.Remove(bestMatch._linkRight);
                            bestMatch._linkLeft._highlight = bestMatch._linkRight._highlight = false;
                            ModelPanel.Invalidate();

                            SelectionModified();
                            return;
                        }

                        //Select both points
                        if (!_selectedLinks.Contains(bestMatch._linkLeft) || !_selectedLinks.Contains(bestMatch._linkRight))
                        {
                            if (!add)
                                ClearCollisionSelection();

                            _selectedLinks.Add(bestMatch._linkLeft);
                            _selectedLinks.Add(bestMatch._linkRight);
                            bestMatch._linkLeft._highlight = bestMatch._linkRight._highlight = true;
                            ModelPanel.Invalidate();

                            SelectionModified();
                        }

                        if (!add)
                            BeginHover(target);
                        //Single Platform Selected;
                        return;
                    }
                }

                //Nothing found :(

                //Trace ray to Z axis
                target = Vector3.IntersectZ(target, ModelPanel.CurrentViewport.UnProject(e.X, e.Y, 0.0f), 0.0f);
                point = (Vector2)target;

                if (create)
                {
                    if (_selectedLinks.Count == 0)
                    {
                        if (_selectedCollisionObject == null)
                            return;

                        _creating = true;

                        //Create two points and hover
                        CollisionLink point1 = new CollisionLink(_selectedCollisionObject, point).Branch(point);

                        _selectedLinks.Add(point1);
                        point1._highlight = true;

                        SelectionModified();
                        BeginHover(target);
                        ModelPanel.Invalidate();
                        return;
                    }
                    else if (_selectedLinks.Count == 1)
                    {
                        //Create new plane extending to point
                        CollisionLink link = _selectedLinks[0];
                        _selectedLinks[0] = link.Branch((Vector2)target);
                        _selectedLinks[0]._highlight = true;
                        link._highlight = false;
                        SelectionModified();
                        ModelPanel.Invalidate();

                        //Hover new point so it can be moved
                        BeginHover(target);
                        return;
                    }
                    else
                    {
                        //Find two closest points and insert between
                        CollisionPlane bestMatch = null;
                        if (_selectedPlanes.Count == 1)
                            bestMatch = _selectedPlanes[0];
                        else
                        {
                            float dist;
                            float bestDist = float.MaxValue;

                            foreach (CollisionPlane p in _selectedPlanes)
                            {
                                dist = point.TrueDistance(p.PointLeft) + point.TrueDistance(p.PointRight) - p.PointLeft.TrueDistance(p.PointRight);
                                if (dist < bestDist)
                                { bestDist = dist; bestMatch = p; }
                            }
                        }

                        ClearCollisionSelection();

                        _selectedLinks.Add(bestMatch.Split(point));
                        _selectedLinks[0]._highlight = true;
                        SelectionModified();
                        ModelPanel.Invalidate();

                        _creating = true;
                        BeginHover(target);

                        return;
                    }
                }

                if (move)
                {
                    if (_selectedLinks.Count > 0)
                        BeginHover(target);
                    return;
                }

                if (!add && !subtract)
                    ClearCollisionSelection();

                BeginSelection(target, subtract);
            }
        }
        protected override void ModelPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_collisionEditorMode)
            {
                if (e.Button == Forms.MouseButtons.Left && !(_scaling || _translating || _rotating))
                {
                    weightEditor.TargetVertices = _selectedVertices;
                    vertexEditor.TargetVertices = _selectedVertices;
                }

                base.ModelPanel_MouseUp(sender, e);
            }
            else if (e.Button == MouseButtons.Left)
            {
                _hasMoved = false;
                FinishSelection();
                FinishHover();
                UpdateCollisionButtons();
            }
        }
        protected override void ModelPanel_MouseMove(object sender, MouseEventArgs e)
        {
            base.ModelPanel_MouseMove(sender, e);

            if (_translating && VertexLoc == null && SelectedBone != null && SnapBonesToCollisions)
                SnapYIfClose();

            if (_selecting) //Selection Box
            {
                Vector3 ray1 = ModelPanel.CurrentViewport.UnProject(new Vector3(e.X, e.Y, 0.0f));
                Vector3 ray2 = ModelPanel.CurrentViewport.UnProject(new Vector3(e.X, e.Y, 1.0f));

                _selectEnd = Vector3.IntersectZ(ray1, ray2, 0.0f);
                _selectEnd._z += SelectWidth;

                //Update selection
                UpdateCollisionSelection(false);

                ModelPanel.Invalidate();
            }

            UpdateHover(e.X, e.Y);
        }
    }
}
