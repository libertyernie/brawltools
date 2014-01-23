using System;
using System.Runtime.InteropServices;
using BrawlLib.Wii.Animations;

namespace BrawlLib.Modeling
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FrameState
    {
        public override string ToString()
        {
            return String.Format("{0}{1}{2}", _scale.ToString(), _rotate.ToString(), _translate.ToString());
        }

        public static readonly FrameState Neutral = new FrameState(new Vector3(1.0f), new Vector3(), new Vector3());

        public Vector3 _scale;
        public Vector3 _rotate;
        public Vector3 _translate;

        public unsafe float this[int index]
        {
            get
            {
                fixed (FrameState* f = &this)
                    return ((float*)f)[index];
            }
            set
            {
                fixed (FrameState* f = &this)
                    ((float*)f)[index] = value;
            }
        }

        public Vector3 Translate
        {
            get { return _translate; }
            set { _translate = value; CalcTransforms(); }
        }
        public Vector3 Rotate
        {
            get { return _rotate; }
            set { _rotate = value; CalcTransforms(); }
        }
        public Vector3 Scale
        {
            get { return _scale; }
            set { _scale = value; CalcTransforms(); }
        }

        public Matrix _transform, _iTransform;

        public FrameState(AnimationFrame frame)
        {
            _scale = frame.Scale;
            _rotate = frame.Rotation;
            _translate = frame.Translation;

            CalcTransforms();
        }
        public FrameState(Vector3 scale, Vector3 rotation, Vector3 translation)
        {
            _scale = scale;
            _rotate = rotation;
            _translate = translation;

            CalcTransforms();
        }
        public void CalcTransforms()
        {
            _transform = Matrix.TransformMatrix(_scale, _rotate, _translate);
            _iTransform = Matrix.ReverseTransformMatrix(_scale, _rotate, _translate);
        }

        public static explicit operator AnimationFrame(FrameState state) { return new AnimationFrame(state._scale, state._rotate, state._translate); }
    }
}
