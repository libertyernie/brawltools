using BrawlLib.Modeling;
using System.Collections.Generic;
namespace System.Windows.Forms
{
    public partial class ModelEditorBase : UserControl
    {
        private void AddUndo(SaveState save)
        {
            int i = _saveIndex + 1;
            if (_undoSaves.Count > i)
            {
                _undoSaves.RemoveRange(i, _undoSaves.Count - i);
                _redoSaves.RemoveRange(i, _redoSaves.Count - i);
            }

            _undoSaves.Add(save);
            _undoing = true;

            if (_undoSaves.Count > _allowedUndos)
            {
                _undoSaves.RemoveAt(0);
                _redoSaves.RemoveAt(0);
                _saveIndex--;
            }
        }
        private void AddRedo(SaveState save)
        {
            _redoSaves.Add(save);
            _saveIndex++;
            UpdateUndoButtons();
        }

        bool _before = true;

        /// <summary>
        /// Call twice; before and after changes
        /// </summary>
        public void BoneChange(IBoneNode bone)
        {
            SaveState state = new SaveState();
            state._bone = bone;
            state._frameState = bone.FrameState;
            state._animation = SelectedCHR0;
            state._frameIndex = CurrentFrame;

            if (_before)
                AddUndo(state);
            else
                AddRedo(state);

            _before = !_before;
        }
        /// <summary>
        /// Call twice; before and after changes
        /// </summary>
        public void VertexChange(List<Vertex3> vertices)
        {
            SaveState state = new SaveState();
            state._vertices = vertices;
            state._targetModel = TargetModel;
            state._translation = (Vector3)VertexLoc();

            if (_before)
                AddUndo(state);
            else
                AddRedo(state);

            _before = !_before;
        }

        public bool CanUndo { get { return _saveIndex > -1; } }
        public bool CanRedo { get { return _saveIndex < _undoSaves.Count; } }
        protected void btnUndo_Click(object sender, EventArgs e) { Undo(); }
        protected void btnRedo_Click(object sender, EventArgs e) { Redo(); }

        public void Undo()
        {
            if (CanUndo)
            {
                ModelPanel.BeginUpdate();

                if (!_undoing)
                    _saveIndex--;
                _undoing = true;

                Apply(_undoSaves[_saveIndex]);

                //Decrement index after applying save
                _saveIndex--;

                UpdateUndoButtons();

                ModelPanel.EndUpdate();
            }
        }
        public void Redo()
        {
            if (CanRedo)
            {
                ModelPanel.BeginUpdate();

                if (_undoing)
                    _saveIndex++;
                _undoing = false;

                Apply(_redoSaves[_saveIndex]);

                //Increment index after applying save
                _saveIndex++;

                UpdateUndoButtons();

                ModelPanel.EndUpdate();
            }
        }

        private void Apply(SaveState s)
        {
            if (s._frameState != null)
            {
                SelectedCHR0 = s._animation;
                CurrentFrame = s._frameIndex;
                SelectedBone = s._bone;
                chr0Editor.ApplyState(s);
            }
            else if (s._vertices != null)
            {
                if (TargetModel != s._targetModel)
                    TargetModel = s._targetModel;

                Vector3 diff = _redoSaves[_saveIndex]._translation - _undoSaves[_saveIndex]._translation;
                if (!_undoing) diff = -diff;
                foreach (Vertex3 v in s._vertices)
                {
                    v._weightedPosition -= diff;
                    v.Unweight();
                }
                UpdateModel();
            }
        }

        public virtual void UpdateUndoButtons() { }
    }
}
