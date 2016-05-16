using BrawlLib.Modeling;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Windows.Forms
{
    public partial class ModelEditControl : ModelEditorBase
    {
        public override void InitHotkeyList()
        {
            base.InitHotkeyList();

            List<HotKeyInfo> temp = new List<HotKeyInfo>()
            {
                new HotKeyInfo(Keys.A, true, false, false, HotkeySelectAllVertices),
                new HotKeyInfo(Keys.A, false, false, false, HotkeyToggleLeftPanel),
                new HotKeyInfo(Keys.D, false, false, false, HotkeyToggleRightPanel),
                new HotKeyInfo(Keys.W, false, false, false, HotkeyToggleTopPanel),
                new HotKeyInfo(Keys.S, false, false, false, HotkeyToggleBottomPanel),
                new HotKeyInfo(Keys.D, true, true, false, HotkeyToggleAllPanels),
                new HotKeyInfo(Keys.E, false, false, false, HotkeyScaleTool),
                new HotKeyInfo(Keys.R, false, false, false, HotkeyRotateTool),
                new HotKeyInfo(Keys.T, false, false, false, HotkeyTranslateTool),
                new HotKeyInfo(Keys.D0, false, false, false, HotkeyVertexEditor),
                new HotKeyInfo(Keys.D9, false, false, false, HotkeyWeightEditor),
                new HotKeyInfo(Keys.Delete, false, false, false, HotkeyDeleteCollision),
                new HotKeyInfo(Keys.OemOpenBrackets, false, false, false, HotkeyOpenBracket),
                new HotKeyInfo(Keys.OemCloseBrackets, false, false, false, HotkeyCloseBracket),
                new HotKeyInfo(Keys.Up, false, true, false, HotkeyNudgeLinkUp),
                new HotKeyInfo(Keys.Down, false, true, false, HotkeyNudgeLinkDown),
                new HotKeyInfo(Keys.Left, false, true, false, HotkeyNudgeLinkLeft),
                new HotKeyInfo(Keys.Right, false, true, false, HotkeyNudgeLinkRight),
            };
            _hotkeyList.AddRange(temp);
        }
        private bool HotkeyNudgeLinkUp()
        {
            if (!ModelPanel.Focused)
                return false;
            if (!_collisionEditorMode)
                return false;

            LinkMoveChange();
            float amount = Shift ? LargeIncrement : SmallIncrement;
            foreach (CollisionLink link in _selectedLinks)
                link._rawValue._y += amount;
            _collisionEditorControl.UpdatePropPanels();
            ModelPanel.Invalidate();
            TargetCollisionNode.SignalPropertyChange();
            LinkMoveChange();

            return true;
        }
        private bool HotkeyNudgeLinkDown()
        {
            if (!ModelPanel.Focused)
                return false;
            if (!_collisionEditorMode)
                return false;

            LinkMoveChange();
            float amount = Control.ModifierKeys == Keys.Shift ? LargeIncrement : SmallIncrement;
            foreach (CollisionLink link in _selectedLinks)
                link._rawValue._y -= amount;
            _collisionEditorControl.UpdatePropPanels();
            ModelPanel.Invalidate();
            TargetCollisionNode.SignalPropertyChange();
            LinkMoveChange();

            return true;
        }
        private bool HotkeyNudgeLinkLeft()
        {
            if (!ModelPanel.Focused)
                return false;
            if (!_collisionEditorMode)
                return false;

            LinkMoveChange();
            float amount = Shift ? LargeIncrement : SmallIncrement;
            foreach (CollisionLink link in _selectedLinks)
                link._rawValue._x -= amount;
            _collisionEditorControl.UpdatePropPanels();
            ModelPanel.Invalidate();
            TargetCollisionNode.SignalPropertyChange();
            LinkMoveChange();

            return true;
        }
        private bool HotkeyNudgeLinkRight()
        {
            if (!ModelPanel.Focused)
                return false;
            if (!_collisionEditorMode)
                return false;

            LinkMoveChange();
            float amount = Shift ? LargeIncrement : SmallIncrement;
            foreach (CollisionLink link in _selectedLinks)
                link._rawValue._x += amount;
            _collisionEditorControl.UpdatePropPanels();
            ModelPanel.Invalidate();
            TargetCollisionNode.SignalPropertyChange();
            LinkMoveChange();

            return true;
        }

        private bool HotkeyCloseBracket()
        {
            if (!ModelPanel.Focused)
                return false;
            if (!_collisionEditorMode)
                return false;

            CollisionLink link = null;
            bool two = false;

            if (_selectedPlanes.Count == 1)
            {
                link = _selectedPlanes[0]._linkRight;
                two = true;
            }
            else if (_selectedLinks.Count == 1)
                link = _selectedLinks[0];

            if (link != null)
                foreach (CollisionPlane p in link._members)
                    if (p._linkLeft == link)
                    {
                        ClearCollisionSelection();

                        _selectedLinks.Add(p._linkRight);
                        p._linkRight._highlight = true;
                        if (two)
                        {
                            _selectedLinks.Add(p._linkLeft);
                            p._linkLeft._highlight = true;
                        }
                        SelectionModified();

                        ModelPanel.Invalidate();
                        break;
                    }

            return false;
        }

        private bool HotkeyOpenBracket()
        {
            if (!ModelPanel.Focused)
                return false;
            if (!_collisionEditorMode)
                return false;

            CollisionLink link = null;
            bool two = false;

            if (_selectedPlanes.Count == 1)
            {
                link = _selectedPlanes[0]._linkLeft;
                two = true;
            }
            else if (_selectedLinks.Count == 1)
                link = _selectedLinks[0];

            if (link != null)
                foreach (CollisionPlane p in link._members)
                    if (p._linkRight == link)
                    {
                        ClearCollisionSelection();

                        _selectedLinks.Add(p._linkLeft);
                        p._linkLeft._highlight = true;
                        if (two)
                        {
                            _selectedLinks.Add(p._linkRight);
                            p._linkRight._highlight = true;
                        }

                        SelectionModified();
                        ModelPanel.Invalidate();
                        break;
                    }

            return false;
        }

        private bool HotkeyDeleteCollision()
        {
            if (!_collisionEditorMode)
                return false;

            if (_selectedPlanes.Count > 0)
                foreach (CollisionPlane plane in _selectedPlanes)
                    plane.Delete();
            else if (_selectedLinks.Count == 1)
                _selectedLinks[0].Pop();

            ClearCollisionSelection();
            SelectionModified();
            ModelPanel.Invalidate();

            return false;
        }

        protected override bool HotkeyCancelChange()
        {
            if (!AwaitingRedoSave)
                return false;

            if (_collisionEditorMode)
            {
                if (_hovering)
                    CancelHover();
                else if (_selecting)
                    CancelSelection();
                else
                {
                    ClearCollisionSelection();
                    ModelPanel.Invalidate();
                }
            }

            return base.HotkeyCancelChange();
        }

        #region Hotkeys
        private bool HotkeySelectAllVertices()
        {
            if (!ModelPanel.Focused)
                return false;

            ClearSelectedVertices();
            if (EditingAll)
            {
                if (_targetModels != null)
                    foreach (IModel mdl in _targetModels)
                        SelectAllVertices(mdl);
            }
            else if (TargetModel != null)
                SelectAllVertices(TargetModel);

            OnSelectedVerticesChanged();

            weightEditor.TargetVertices = _selectedVertices;
            vertexEditor.TargetVertices = _selectedVertices;

            ModelPanel.Invalidate();

            return true;
        }
        private bool HotkeyToggleLeftPanel()
        {
            if (ModelPanel.Focused)
            {
                btnLeftToggle_Click(this, EventArgs.Empty);
                return true;
            }
            return false;
        }
        private bool HotkeyToggleTopPanel()
        {
            if (ModelPanel.Focused)
            {
                btnTopToggle_Click(this, EventArgs.Empty);
                return true;
            }
            return false;
        }
        private bool HotkeyToggleRightPanel()
        {
            if (ModelPanel.Focused)
            {
                btnRightToggle_Click(this, EventArgs.Empty);
                return true;
            }
            return false;
        }
        private bool HotkeyToggleBottomPanel()
        {
            if (ModelPanel.Focused)
            {
                btnBottomToggle_Click(this, EventArgs.Empty);
                return true;
            }
            return false;
        }
        private bool HotkeyToggleAllPanels()
        {
            if (ModelPanel.Focused)
            {
                if (leftPanel.Visible || rightPanel.Visible || animEditors.Visible || controlPanel.Visible)
                    showBottom.Checked = showRight.Checked = showLeft.Checked = showTop.Checked = false;
                else
                    showBottom.Checked = showRight.Checked = showLeft.Checked = showTop.Checked = true;
                return true;
            }
            return false;
        }
        private bool HotkeyScaleTool()
        {
            if (ModelPanel.Focused)
            {
                ControlType = TransformType.Scale;
                return true;
            }
            return false;
        }
        private bool HotkeyRotateTool()
        {
            if (ModelPanel.Focused)
            {
                ControlType = TransformType.Rotation;
                return true;
            }
            return false;
        }
        private bool HotkeyTranslateTool()
        {
            if (ModelPanel.Focused)
            {
                ControlType = TransformType.Translation;
                return true;
            }
            return false;
        }

        private bool HotkeyWeightEditor()
        {
            if (ModelPanel.Focused)
            {
                ToggleWeightEditor();
                return true;
            }
            return false;
        }
        private bool HotkeyVertexEditor()
        {
            if (ModelPanel.Focused)
            {
                ToggleVertexEditor();
                return true;
            }
            return false;
        }
        #endregion
    }
}
