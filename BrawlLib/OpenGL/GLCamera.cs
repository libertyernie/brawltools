using System;

namespace BrawlLib.OpenGL
{
    public unsafe class GLCamera
    {
        public Matrix _matrix;
        public Matrix _matrixInverse;

        public Vector3 _rotation;
        public Vector3 _scale;

        internal float _z;

        public GLCamera() { Reset(); }

        public Vector3 GetPoint() { return _matrixInverse.Multiply(new Vector3()); }
        
        public void Scale(float x, float y, float z) { Scale(new Vector3(x, y, z)); }
        public void Scale(Vector3 v)
        {
            _scale *= v;

            Apply();
        }
        public void Translate(Vector3 v) { Translate(v._x, v._y, v._z); }
        public void Translate(float x, float y, float z)
        {
            _matrix = Matrix.TranslationMatrix(-x, -y, -z) * _matrix;
            _matrixInverse.Translate(x, y, z);
            _z += z;
        }
        public void Rotate(float x, float y, float z) { Rotate(new Vector3(x, y, z)); }
        public void Rotate(Vector3 v)
        {
            _rotation += v;

            Apply();
        }

        private void Apply()
        {
            //Grab vertex from matrix
            Vector3 point = GetPoint();

            //Reset matrices
            _matrix = Matrix.ReverseTransformMatrix(_scale, _rotation, point);
            _matrixInverse = Matrix.TransformMatrix(_scale, _rotation, point);
        }

        public void Rotate(float x, float y) { Rotate(x, y, 0); }
        public void Pivot(float radius, float x, float y)
        {
            Translate(0, 0, -radius);
            Rotate(x, y);
            Translate(0, 0, radius);
        }

        public void Reset()
        {
            _matrix = _matrixInverse = Matrix.Identity;
            _rotation = new Vector3();
            _scale = new Vector3(1.5f);
            _z = 0.0f;
        }
    }
}
