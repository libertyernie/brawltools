using BrawlLib.SSBB.ResourceNodes;
using System.ComponentModel;
using BrawlLib.SSBBTypes;
using BrawlLib.OpenGL;
using System.Collections.Generic;
using System.Drawing;
using BrawlLib.Modeling;
using OpenTK.Graphics.OpenGL;

namespace System.Windows.Forms
{
    public partial class ModelEditControl : ModelEditorBase
    {
        private const float SelectWidth = 7.0f;
        private const float PointSelectRadius = 1.5f;
        private const float SmallIncrement = 0.5f;
        private const float LargeIncrement = 3.0f;

        public List<CollisionNode> _collisions = new List<CollisionNode>();

        private CollisionNode _targetCollisionNode;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CollisionNode TargetCollisionNode
        {
            get { return _targetCollisionNode; }
            set { TargetChanged(value); }
        }

        public CollisionObject _selectedCollisionObject;
        private Matrix _snapMatrix;

        public bool _collisionEditorMode;

        private bool _hovering;
        public List<CollisionLink> _selectedLinks = new List<CollisionLink>();
        public List<CollisionPlane> _selectedPlanes = new List<CollisionPlane>();

        private bool _selecting, _selectInverse;
        private Vector3 _selectStart, _selectLast, _selectEnd;
        private bool _creating;

        public List<CollisionObject> CollisionObjects
        {
            get
            {
                if (TargetCollisionNode != null && TargetCollisionNode._objects != null)
                    return TargetCollisionNode._objects;
                return new List<CollisionObject>();
            }
        }
        
        private bool _hasMoved = false;

        private void TargetChanged(CollisionNode node)
        {
            ClearCollisionSelection();
            _snapMatrix = Matrix.Identity;
            _selectedCollisionObject = null;

            leftPanel.lstTextures.Items.Clear();

            if ((_targetCollisionNode = node) != null)
                foreach (CollisionObject obj in CollisionObjects)
                    leftPanel.lstTextures.Items.Add(obj, true);
        }

        private void SelectionModified()
        {
            _selectedPlanes.Clear();
            foreach (CollisionLink l in _selectedLinks)
                foreach (CollisionPlane p in l._members)
                    if (_selectedLinks.Contains(p._linkLeft) &&
                        _selectedLinks.Contains(p._linkRight) &&
                        !_selectedPlanes.Contains(p))
                        _selectedPlanes.Add(p);

            _collisionEditorControl.SelectionModified();
            _collisionEditorControl.UpdatePropPanels();
        }
        
        #region Object List
        private void snapToolStripMenuItem_Click(object sender, EventArgs e) { SnapObject(); }

        public void SnapObject()
        {
            if (_selectedCollisionObject == null)
                return;

            _updating = true;

            _snapMatrix = Matrix.Identity;

            List<CollisionObject> objects = CollisionObjects;
            for (int i = 0; i < objects.Count; i++)
                leftPanel.lstTextures.SetItemChecked(i, false);

            //Set snap matrix
            if (!String.IsNullOrEmpty(_selectedCollisionObject._modelName))
                foreach (IModel m in _targetModels)
                    if (((ResourceNode)m).Name == _selectedCollisionObject._modelName)
                    {
                        foreach (IBoneNode b in m.BoneCache)
                            if (b.Name == _selectedCollisionObject._boneName)
                            {
                                _snapMatrix = b.InverseMatrix;
                                break;
                            }
                        break;
                    }

            //Show objects with similar bones
            for (int i = objects.Count; i-- > 0;)
            {
                CollisionObject obj = objects[i] as CollisionObject;
                if (obj._modelName == _selectedCollisionObject._modelName &&
                    obj._boneName == _selectedCollisionObject._boneName)
                    leftPanel.lstTextures.SetItemChecked(i, true);
            }

            _updating = false;
            ModelPanel.Invalidate();
        }

        #endregion

        public void ClearCollisionSelection()
        {

            foreach (CollisionLink l in _selectedLinks)
                l._highlight = false;
            _selectedLinks.Clear();
            _selectedPlanes.Clear();
        }

        private void UpdateCollisionSelection(bool finish)
        {
            if (_targetCollisionNode == null)
                return;

            foreach (CollisionObject obj in _targetCollisionNode._objects)
                foreach (CollisionLink link in obj._points)
                {
                    link._highlight = false;
                    if (!obj._render)
                        continue;

                    Vector3 point = (Vector3)link.Value;

                    if (_selectInverse && point.Contained(_selectStart, _selectEnd, 0.0f))
                    {
                        if (finish)
                            _selectedLinks.Remove(link);
                        continue;
                    }

                    if (_selectedLinks.Contains(link))
                        link._highlight = true;
                    else if (!_selectInverse && point.Contained(_selectStart, _selectEnd, 0.0f))
                    {
                        link._highlight = true;
                        if (finish)
                            _selectedLinks.Add(link);
                    }
                }
        }
        public void UpdateCollisionButtons()
        {
            if (!_collisionEditorMode || _selecting || _hovering || _selectedLinks.Count == 0)
                btnMerge.Enabled = btnSplit.Enabled = btnSameX.Enabled = btnSameY.Enabled = false;
            else
            {
                btnMerge.Enabled = btnSameX.Enabled = btnSameY.Enabled = _selectedLinks.Count > 1;
                btnSplit.Enabled = true;
            }
        }

        private void _treeObjects_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is CollisionObject)
                (e.Node.Tag as CollisionObject)._render = e.Node.Checked;
            if (e.Node.Tag is CollisionPlane)
                (e.Node.Tag as CollisionPlane)._render = e.Node.Checked;

            ModelPanel.Invalidate();
        }
        private void BeginHover(Vector3 point)
        {
            if (_hovering)
                return;

            if (!_hasMoved) //Create undo for first move
            {
                LinkMoveChange();
                _hasMoved = true;
                TargetCollisionNode.SignalPropertyChange();
            }
            
            _selectStart = _selectLast = point;
            _hovering = true;
            UpdateCollisionButtons();
        }

        private void UpdateHover(int x, int y)
        {
            if (!_hovering)
                return;

            _selectEnd = Vector3.IntersectZ(ModelPanel.CurrentViewport.UnProject(x, y, 0.0f), ModelPanel.CurrentViewport.UnProject(x, y, 1.0f), _selectLast._z);
            
            //Apply difference in start/end
            Vector3 diff = _selectEnd - _selectLast;
            _selectLast = _selectEnd;

            //Move points
            foreach (CollisionLink p in _selectedLinks)
                p.Value += diff;
            
            ModelPanel.Invalidate();

            _collisionEditorControl.UpdatePropPanels();
        }
        private void CancelHover()
        {
            if (!_hovering)
                return;

            if (_hasMoved)
            {
                CancelUndoStateChange();
                _hasMoved = false;
            }

            _hovering = false;

            if (_creating)
            {
                _creating = false;
                //Delete points/plane
                _selectedLinks[0].Pop();
                ClearCollisionSelection();
                SelectionModified();
            }
            else
            {
                Vector3 diff = _selectStart - _selectLast;
                foreach (CollisionLink l in _selectedLinks)
                    l.Value += diff;
            }
            ModelPanel.Invalidate();
            _collisionEditorControl.UpdatePropPanels();
        }
        private void FinishHover()
        {
            _hovering = false;
            LinkMoveChange();
        }
        private void BeginSelection(Vector3 point, bool inverse)
        {
            if (_selecting)
                return;

            _selectStart = _selectEnd = point;

            _selectEnd._z += SelectWidth;
            _selectStart._z -= SelectWidth;

            _selecting = true;
            _selectInverse = inverse;

            UpdateCollisionButtons();
        }
        private void CancelSelection()
        {
            if (!_selecting)
                return;

            _selecting = false;
            _selectStart = _selectEnd = new Vector3(float.MaxValue);
            UpdateCollisionSelection(false);
            ModelPanel.Invalidate();
        }
        private void FinishSelection()
        {
            if (!_selecting)
                return;

            _selecting = false;
            UpdateCollisionSelection(true);
            ModelPanel.Invalidate();

            SelectionModified();

            //Selection Area Selected.
        }

        private void btnSplit_Click(object sender, EventArgs e)
        {
            LinkSplitChange();
            for (int i = _selectedLinks.Count; --i >= 0; )
                _selectedLinks[i].Split();
            ClearCollisionSelection();
            SelectionModified();
            ModelPanel.Invalidate();
            LinkSplitChange();
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            LinkMergeChange();
            for (int i = 0; i < _selectedLinks.Count - 1; )
            {
                CollisionLink link = _selectedLinks[i++];
                Vector2 pos = link.Value;
                int count = 1;
                for (int x = i; x < _selectedLinks.Count;)
                {
                    if (link.Merge(_selectedLinks[x]))
                    {
                        pos += _selectedLinks[x].Value;
                        count++;
                        _selectedLinks.RemoveAt(x);
                    }
                    else
                        x++;
                }
                link.Value = pos / count;
            }
            LinkMergeChange();
            ModelPanel.Invalidate();
        }
        private bool PointCollides(Vector3 point)
        {
            float f;
            return PointCollides(point, out f);
        }
        private bool PointCollides(Vector3 point, out float y_result)
        {
            y_result = float.MaxValue;
            Vector2 v2 = new Vector2(point._x, point._y);
            foreach (CollisionNode coll in _collisions)
            {
                foreach (CollisionObject obj in coll._objects)
                {
                    if (obj._render)
                    {
                        foreach (CollisionPlane plane in obj._planes)
                        {
                            if (plane._type == BrawlLib.SSBBTypes.CollisionPlaneType.Floor)
                            {
                                if (plane.PointLeft._x < v2._x && plane.PointRight._x > v2._x)
                                {
                                    float x = v2._x;
                                    float m = (plane.PointLeft._y - plane.PointRight._y)
                                        / (plane.PointLeft._x - plane.PointRight._x);
                                    float b = plane.PointRight._y - m * plane.PointRight._x;
                                    float y_target = m * x + b;
                                    //Console.WriteLine(y_target);
                                    if (Math.Abs(y_target - v2._y) <= Math.Abs(y_result - v2._y))
                                    {
                                        y_result = y_target;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return (Math.Abs(y_result - v2._y) <= 5);
        }
        private void SnapYIfClose()
        {
            float f;
            if (PointCollides(new Vector3(chr0Editor._transBoxes[6].Value, chr0Editor._transBoxes[7].Value, chr0Editor._transBoxes[8].Value), out f))
                ApplyTranslation(1, f - chr0Editor._transBoxes[7].Value);
        }
        private void btnSameX_Click(object sender, EventArgs e)
        {
            if (_selectedLinks.Count == 0)
                return;

            LinkMoveChange();

            float averageX = 0;
            int count = 0;
            foreach (CollisionLink link in _selectedLinks)
            {
                averageX += link.Value._y;
                ++count;
            }
            averageX /= count;

            foreach (CollisionLink link in _selectedLinks)
                link.Value = new Vector2(averageX, link.Value._y);

            ModelPanel.Invalidate();
            TargetCollisionNode.SignalPropertyChange();

            LinkMoveChange();
        }
        private void btnSameY_Click(object sender, EventArgs e)
        {
            if (_selectedLinks.Count == 0)
                return;

            LinkMoveChange();

            float averageY = 0;
            int count = 0;
            foreach (CollisionLink link in _selectedLinks)
            {
                averageY += link.Value._y;
                ++count;
            }
            averageY /= count;

            foreach (CollisionLink link in _selectedLinks)
                link.Value = new Vector2(link.Value._x, averageY);

            ModelPanel.Invalidate();
            TargetCollisionNode.SignalPropertyChange();

            LinkMoveChange();
        }
        private void modelTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is MDL0Node)
            {
                ((MDL0Node)e.Node.Tag).IsRendering = e.Node.Checked;
                if (!_updating)
                {
                    _updating = true;
                    foreach (TreeNode n in e.Node.Nodes)
                        n.Checked = e.Node.Checked;
                    _updating = false;
                }
            }
            else if (e.Node.Tag is MDL0BoneNode)
                ((MDL0BoneNode)e.Node.Tag)._render = e.Node.Checked;

            if (!_updating)
                ModelPanel.Invalidate();
        }

        private void modelTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            MDL0BoneNode bone = SelectedBone as MDL0BoneNode;
            if (bone != null)
            {
                bone._boneColor = Color.FromArgb(255, 0, 0);
                bone._nodeColor = Color.FromArgb(255, 128, 0);
                ModelPanel.Invalidate();
            }
        }

        private void modelTree_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            MDL0BoneNode bone = SelectedBone as MDL0BoneNode;
            if (bone != null)
            {
                bone._nodeColor = bone._boneColor = Color.Transparent;
                ModelPanel.Invalidate();
            }
        }
        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            if (SelectedBone == null)
                e.Cancel = true;
        }
        private void snapToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (SelectedBone != null)
            {
                _snapMatrix = SelectedBone.InverseMatrix;
                ModelPanel.Invalidate();
            }
        }
        private void btnResetSnap_Click(object sender, EventArgs e)
        {
            _snapMatrix = Matrix.Identity;
            ModelPanel.Invalidate();
        }
        //public void CreateUndo()
        //{
        //    CheckSaveIndex();
        //    if (undoSaves.Count > saveIndex)
        //    {
        //        undoSaves.RemoveRange(saveIndex, undoSaves.Count - saveIndex);
        //        redoSaves.Clear();
        //    }

        //    save = new CollisionState();
        //    save._collisionLinks = new List<CollisionLink>();
        //    save._linkVectors = new List<Vector2>();

        //    foreach (CollisionLink l in _selectedLinks)
        //    { save._collisionLinks.Add(l); save._linkVectors.Add(l.Value); }

        //    undoSaves.Add(save);
        //    btnUndo.Enabled = true;
        //    saveIndex++;
        //    save = null; 
        //}
        //private void CheckSaveIndex()
        //{
        //    if (saveIndex < 0)
        //    { saveIndex = 0; }

        //    if (undoSaves.Count > 25)
        //    { undoSaves.RemoveAt(0); saveIndex--; }
        //}
        //private void ClearUndoBuffer()
        //{
        //    saveIndex = 0;
        //    undoSaves.Clear();
        //    redoSaves.Clear();
        //    btnUndo.Enabled = btnRedo.Enabled = false;
        //}
        //private void Undo(object sender, EventArgs e)
        //{
        //    _selectedLinks.Clear();

        //    save = new CollisionState();

        //    if (undoSaves[saveIndex - 1]._linkVectors != null)     //XY Positions changed.
        //    {
        //        save._collisionLinks = new List<CollisionLink>();
        //        save._linkVectors = new List<Vector2>();

        //        for (int i = 0; i < undoSaves[saveIndex - 1]._collisionLinks.Count; i++)
        //        {
        //            _selectedLinks.Add(undoSaves[saveIndex - 1]._collisionLinks[i]);
        //            save._collisionLinks.Add(undoSaves[saveIndex - 1]._collisionLinks[i]);
        //            save._linkVectors.Add(undoSaves[saveIndex - 1]._collisionLinks[i].Value);
        //            _selectedLinks[i].Value = undoSaves[saveIndex - 1]._linkVectors[i];
        //        }
        //    }

        //    saveIndex--;
        //    CheckSaveIndex();

        //    if (saveIndex == 0)
        //    { btnUndo.Enabled = false; }
        //    btnRedo.Enabled = true;

        //    redoSaves.Add(save);
        //    save = null;
            
        //    ModelPanel.Invalidate();
        //    _collisionEditorControl.UpdatePropPanels();
        //}
        //private void Redo(object sender, EventArgs e)
        //{
        //    _selectedLinks.Clear();

        //    for (int i = 0; i < redoSaves[undoSaves.Count - saveIndex - 1]._collisionLinks.Count; i++)
        //    {
        //        _selectedLinks.Add(redoSaves[undoSaves.Count - saveIndex - 1]._collisionLinks[i]);
        //        _selectedLinks[i].Value = redoSaves[undoSaves.Count - saveIndex - 1]._linkVectors[i];
        //    }

        //    redoSaves.RemoveAt(undoSaves.Count - saveIndex - 1);
        //    saveIndex++;

        //    if (redoSaves.Count == 0)
        //    { btnRedo.Enabled = false; }
        //    btnUndo.Enabled = true;

        //    ModelPanel.Invalidate();
        //    _collisionEditorControl.UpdatePropPanels();
        //}

        private void btnTranslateAll_Click(object sender, EventArgs e) {
            if (_selectedLinks.Count == 0) {
                MessageBox.Show("You must select at least one collision link.");
                return;
            }
            using (TransformAttributesForm f = new TransformAttributesForm()) {
                f.TwoDimensional = true;
                if (f.ShowDialog() == DialogResult.OK) {
                    Matrix m = f.GetMatrix();
                    foreach (var link in _selectedLinks) {
                        link.Value = m * link.Value;
                    }
                    TargetCollisionNode.SignalPropertyChange();
                    ModelPanel.Invalidate();
                }
            }
        }
    }
}
