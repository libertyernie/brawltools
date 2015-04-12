using BrawlLib.Modeling;
using System.Collections.Generic;
namespace System.Windows.Forms
{
    public partial class ModelEditorBase : UserControl
    {
        public uint _allowedUndos = 50;
        public List<SaveState> _undoSaves = new List<SaveState>();
        public List<SaveState> _redoSaves = new List<SaveState>();
        public int _saveIndex = 0;
        public bool _awaitingRedoSave = false;
        public bool _undoing = false;

        void AddUndo(SaveState save)
        {
            if (_awaitingRedoSave)
            {
                throw new Exception("Waiting for redo save to be added");
            }

            int i = _saveIndex + 1;
            if (_undoSaves.Count > i)
            {
                _undoSaves.RemoveRange(i, _undoSaves.Count - i);
                _redoSaves.RemoveRange(i, _redoSaves.Count - i);
            }

            save._isUndo = true;
            _undoSaves.Add(save);
            _awaitingRedoSave = true;
        }
        void AddRedo(SaveState save)
        {
            if (!_awaitingRedoSave)
            {
                throw new Exception("Waiting for undo save to be added");
            }

            save._isUndo = false;
            _redoSaves.Add(save);
            
            if (_undoSaves.Count > _allowedUndos)
            {
                _undoSaves.RemoveAt(0);
                _redoSaves.RemoveAt(0);
            }
            else
                _saveIndex++;

            _awaitingRedoSave = false;
            UpdateUndoButtons();
        }

        void AddState(SaveState state)
        {
            if (!_awaitingRedoSave)
                AddUndo(state);
            else
                AddRedo(state);
        }

        /// <summary>
        /// Call twice; before and after changes
        /// </summary>
        public void BoneChange(IBoneNode bone)
        {
            SaveState state = new BoneState()
            {
                _bone = bone,
                _frameState = bone.FrameState,
                _animation = SelectedCHR0,
                _frameIndex = CurrentFrame,
            };

            AddState(state);
        }
        /// <summary>
        /// Call twice; before and after changes
        /// </summary>
        public void VertexChange(List<Vertex3> vertices, Matrix transform)
        {
            SaveState state = new VertexState()
            {
                _vertices = vertices,
                _targetModel = TargetModel,
                _origin = VertexLoc().Value,
                _transform = transform,
            };

            AddState(state);
        }

        public bool CanUndo { get { return _saveIndex >= 0; } }
        public bool CanRedo { get { return _saveIndex < _undoSaves.Count; } }

        public void Undo()
        {
            if (CanUndo)
            {
                ModelPanel.BeginUpdate();

                if (!_undoing)
                    _saveIndex--;
                _undoing = true;

                if (_saveIndex < _undoSaves.Count && _saveIndex >= 0)
                    ApplyState();

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

                if (_saveIndex < _redoSaves.Count && _saveIndex >= 0)
                    ApplyState();

                //Increment index after applying save
                _saveIndex++;

                UpdateUndoButtons();

                ModelPanel.EndUpdate();
            }
        }

        void ApplyBoneState(BoneState state)
        {
            if (TargetModel != state._targetModel)
            {
                _resetCamera = false;
                TargetModel = state._targetModel;
            }
            SelectedCHR0 = state._animation;
            CurrentFrame = state._frameIndex;
            SelectedBone = state._bone;
            CHR0Editor.ApplyState(state);
        }

        void ApplyVertexState(VertexState state)
        {
            if (TargetModel != state._targetModel)
            {
                _resetCamera = false;
                TargetModel = state._targetModel;
            }

            Matrix transform;
            Vector3 center = new Vector3();

            if (state._isUndo)
            {
                transform = ((VertexState)RedoSave)._transform.Invert();
                center = state._origin;
            }
            else
            {
                transform = state._transform;
                center = ((VertexState)UndoSave)._origin;
            }

            foreach (Vertex3 vertex in _selectedVertices)
                vertex.WeightedPosition = Maths.TransformAboutPoint(vertex.WeightedPosition, center, transform);

            _vertexLoc = null;

            UpdateModel();
        }

        public SaveState RedoSave { get { return _saveIndex < _redoSaves.Count && _saveIndex >= 0 ? _redoSaves[_saveIndex] : null; } }
        public SaveState UndoSave { get { return _saveIndex < _undoSaves.Count && _saveIndex >= 0 ? _undoSaves[_saveIndex] : null; } }
        
        private void ApplyState()
        {
            SaveState current = _undoing ? UndoSave : RedoSave;

            if (current is BoneState)
                ApplyBoneState((BoneState)current);
            else if (current is VertexState)
                ApplyVertexState((VertexState)current);
        }

        public virtual void UpdateUndoButtons() { }
    }
}
