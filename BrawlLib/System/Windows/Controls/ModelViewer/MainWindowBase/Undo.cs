using BrawlLib.Modeling;
using BrawlLib.SSBB.ResourceNodes;
using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Forms
{
    public partial class ModelEditorBase : UserControl
    {
        public bool AwaitingRedoSave { get { return _currentUndo != null; } }
        public bool CanUndo { get { return _saveIndex >= 0; } }
        public bool CanRedo { get { return _saveIndex < _undoSaves.Count; } }

        private SaveState RedoSave { get { return _saveIndex < _redoSaves.Count && _saveIndex >= 0 ? _redoSaves[_saveIndex] : null; } }
        private SaveState UndoSave { get { return _saveIndex < _undoSaves.Count && _saveIndex >= 0 ? _undoSaves[_saveIndex] : null; } }

        public uint _allowedUndos = 50;
        private List<SaveState> _undoSaves = new List<SaveState>();
        private List<SaveState> _redoSaves = new List<SaveState>();
        private int _saveIndex = 0;
        private bool _undoing = false;
        private SaveState _currentUndo;

        #region Public Undo functions

        public void Undo()
        {
            _translating = _scaling = _rotating = false;

            if (AwaitingRedoSave)
                CancelUndoStateChange();
            else if (CanUndo)
            {
                ModelPanel.BeginUpdate();

                if (!_undoing)
                    _saveIndex--;
                _undoing = true;

                if (_saveIndex < _undoSaves.Count && _saveIndex >= 0)
                    ApplySaveState(UndoSave);

                //Decrement index after applying save
                _saveIndex--;

                UpdateUndoButtons();

                ModelPanel.EndUpdate();
            }
        }
        public void Redo()
        {
            _translating = _scaling = _rotating = false;

            if (AwaitingRedoSave)
                CancelUndoStateChange();

            if (CanRedo)
            {
                ModelPanel.BeginUpdate();

                if (_undoing)
                    _saveIndex++;
                _undoing = false;

                if (_saveIndex < _redoSaves.Count && _saveIndex >= 0)
                    ApplySaveState(RedoSave);

                //Increment index after applying save
                _saveIndex++;

                UpdateUndoButtons();

                ModelPanel.EndUpdate();
            }
        }
        public void CancelUndoStateChange()
        {
            if (!AwaitingRedoSave)
            {
#if DEBUG
                throw new Exception("Nothing to cancel.");
#else
                return;
#endif
            }

            _currentUndo = null;
        }

        #endregion

        #region State management
        protected void AddState(SaveState state)
        {
            if (!AwaitingRedoSave)
                AddUndo(state);
            else
                AddRedo(state);
        }
        private void AddUndo(SaveState save)
        {
            if (AwaitingRedoSave)
            {
#if DEBUG
                throw new Exception("Waiting for redo save to be added");
#else
                return;
#endif
            }

            save._isUndo = true;
            _currentUndo = save;
        }
        private void AddRedo(SaveState save)
        {
            if (!AwaitingRedoSave)
            {
#if DEBUG
                throw new Exception("Waiting for undo save to be added");
#else
                return;
#endif
            }

            save._isUndo = false;

            //Remove changes made after the current state
            //BEFORE adding new saves for this state
            int i = _saveIndex + 1;
            if (_undoSaves.Count > i && _undoSaves.Count - i > 0)
            {
                _undoSaves.RemoveRange(i, _undoSaves.Count - i);
                _redoSaves.RemoveRange(i, _redoSaves.Count - i);
            }

            //Add the saves
            _redoSaves.Add(save);
            _undoSaves.Add(_currentUndo);

            //Remove the oldest saves if over the max undo count
            if (_undoSaves.Count > _allowedUndos)
            {
                _undoSaves.RemoveAt(0);
                _redoSaves.RemoveAt(0);
            }
            else //Otherwise, move the save index to the current saves
                _saveIndex++;

            _currentUndo = null;
            UpdateUndoButtons();
        }
        /// <summary>
        /// This will apply states added to the undo buffer.
        /// Override to apply custom undo states
        /// </summary>
        protected virtual void ApplySaveState(SaveState state)
        {
            if (state is BoneState)
                ApplyBoneState((BoneState)state);
            else if (state is VertexState)
                ApplyVertexState((VertexState)state);
        }
        #endregion

        public virtual void UpdateUndoButtons() { }

        public void ResetUndoBuffer()
        {
            _undoSaves.Clear();
            _redoSaves.Clear();
            _saveIndex = -1;
            _undoing = false;
            _currentUndo = null;
        }

        #region Undo types
        /// <summary>
        /// Call twice; before and after changes
        /// </summary>
        public void BoneChange(params IBoneNode[] bones)
        {
            AddState(new BoneState()
            {
                _bones = bones,
                _frameStates = bones.Select(x => x.FrameState).ToArray(),
                _animation = SelectedCHR0,
                _frameIndex = CurrentFrame,
                _updateBoneOnly = CHR0Editor.chkMoveBoneOnly.Checked,
                _updateBindState = CHR0Editor.chkUpdateBindPose.Checked,
            });
        }
        private void ApplyBoneState(BoneState state)
        {
            if (TargetModel != state._targetModel)
                TargetModel = state._targetModel;

            SelectedCHR0 = state._animation;
            CurrentFrame = state._frameIndex;
            CHR0Editor.chkUpdateBindPose.Checked = state._updateBindState;
            CHR0Editor.chkMoveBoneOnly.Checked = state._updateBoneOnly;
            for (int i = 0; i < state._bones.Length; i++)
            {
                SelectedBone = state._bones[i];
                CHR0Editor.ApplyState(state._frameStates[i]);
            }
        }
        /// <summary>
        /// Call twice; before and after changes
        /// </summary>
        public void VertexChange(List<Vertex3> vertices)
        {
            AddState(new VertexState()
            {
                _chr0 = _chr0,
                _animFrame = CurrentFrame,
                _vertices = vertices,
                _weightedPositions = vertices.Select(x => x.WeightedPosition).ToList(),
                _targetModel = TargetModel,
            });
        }
        private void ApplyVertexState(VertexState state)
        {
            IModel model = TargetModel;
            CHR0Node n = _chr0;
            int frame = CurrentFrame;

            if (TargetModel != state._targetModel)
                TargetModel = state._targetModel;

            SelectedCHR0 = state._chr0;
            CurrentFrame = state._animFrame;

            for (int i = 0; i < state._vertices.Count; i++)
                state._vertices[i].WeightedPosition = state._weightedPositions[i];

            SetSelectedVertices(state._vertices);

            _vertexLoc = null;

            if (TargetModel != model)
                TargetModel = model;

            SelectedCHR0 = n;
            CurrentFrame = frame;

            UpdateModel();
        }
        #endregion
    }
}
