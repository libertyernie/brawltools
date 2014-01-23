using System;
using System.Runtime.InteropServices;

namespace System
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public unsafe struct Matrix43
    {
        public static readonly Matrix43 Identity = ScaleMatrix(1.0f, 1.0f, 1.0f);

        internal fixed float _data[12];

        public float this[int index]
        {
            get { fixed (float* p = _data) return p[index]; }
            set { fixed (float* p = _data) *p = value; }
        }

        public void Translate(float x, float y, float z)
        {
            Matrix43 m = TranslationMatrix(x, y, z);
            Multiply(&m);
        }
        internal void Rotate(float x, float y, float z)
        {
            Matrix43 m = RotationMatrix(x, y, z);
            this.Multiply(&m);
        }

        internal void Scale(float x, float y, float z)
        {
            Matrix43 m = ScaleMatrix(x, y, z);
            this.Multiply(&m);
        }

        public Matrix43 GetTranslation()
        {
            Matrix43 m = Identity;
            float* p = (float*)&m;
            fixed(float* s = _data)
            {
                p[3] = s[3];
                p[7] = s[7];
                p[11] = s[11];
            }
            return m;
        }

        public static Matrix43 ScaleMatrix(float x, float y, float z)
        {
            Matrix43 m = new Matrix43();
            float* p = (float*)&m;
            p[0] = x;
            p[5] = y;
            p[10] = z;
            return m;
        }
        public static Matrix43 TranslationMatrix(float x, float y, float z)
        {
            Matrix43 m = new Matrix43();
            float* p = (float*)&m;
            p[3] = x;
            p[7] = y;
            p[11] = z;
            p[0] = p[5] = p[10] = 1.0f;
            return m;
        }

        public static Matrix43 RotationMatrixX(float x)
        {
            Matrix43 m = new Matrix43();
            float* p = (float*)&m;

            float cosx = (float)Math.Cos(x / 180.0f * Math.PI);
            float sinx = (float)Math.Sin(x / 180.0f * Math.PI);

            p[0] = 1.0f;
            p[5] = cosx;
            p[6] = -sinx;
            p[9] = sinx;
            p[10] = cosx;

            return m;
        }
        public static Matrix43 RotationMatrixRX(float x)
        {
            Matrix43 m = new Matrix43();
            float* p = (float*)&m;

            float cosx = (float)Math.Cos(x / 180.0f * Math.PI);
            float sinx = (float)Math.Sin(x / 180.0f * Math.PI);

            p[0] = 1.0f;
            p[5] = cosx;
            p[6] = sinx;
            p[9] = -sinx;
            p[10] = cosx;

            return m;
        }

        public static Matrix43 RotationMatrixY(float y)
        {
            Matrix43 m = new Matrix43();
            float* p = (float*)&m;

            float cosy = (float)Math.Cos(y / 180.0f * Math.PI);
            float siny = (float)Math.Sin(y / 180.0f * Math.PI);

            p[5] = 1.0f;

            p[0] = cosy;
            p[2] = siny;
            p[8] = -siny;
            p[10] = cosy;

            return m;
        }

        public static Matrix43 RotationMatrixRY(float y)
        {
            Matrix43 m = new Matrix43();
            float* p = (float*)&m;

            float cosy = (float)Math.Cos(y / 180.0f * Math.PI);
            float siny = (float)Math.Sin(y / 180.0f * Math.PI);

            p[5] = 1.0f;

            p[0] = cosy;
            p[2] = -siny;
            p[8] = siny;
            p[10] = cosy;

            return m;
        }

        public void RotateX(float x)
        {
            float var1, var2;
            float cosx = (float)Math.Cos(x / 180.0f * Math.PI);
            float sinx = (float)Math.Sin(x / 180.0f * Math.PI);

            fixed (float* p = _data)
            {
                var1 = p[1]; var2 = p[2];
                p[1] = (var1 * cosx) + (var2 * sinx);
                p[2] = (var1 * -sinx) + (var2 * cosx);

                var1 = p[5]; var2 = p[6];
                p[5] = (var1 * cosx) + (var2 * sinx);
                p[6] = (var1 * -sinx) + (var2 * cosx);

                var1 = p[9]; var2 = p[10];
                p[9] = (var1 * cosx) + (var2 * sinx);
                p[10] = (var1 * -sinx) + (var2 * cosx);
            }
        }

        public void RotateY(float y)
        {
            float var1, var2;
            float cosy = (float)Math.Cos(y / 180.0f * Math.PI);
            float siny = (float)Math.Sin(y / 180.0f * Math.PI);

            fixed (float* p = _data)
            {
                var1 = p[0]; var2 = p[2];
                p[0] = (var1 * cosy) + (var2 * -siny);
                p[2] = (var1 * siny) + (var2 * cosy);

                var1 = p[4]; var2 = p[6];
                p[4] = (var1 * cosy) + (var2 * -siny);
                p[6] = (var1 * siny) + (var2 * cosy);

                var1 = p[8]; var2 = p[10];
                p[8] = (var1 * cosy) + (var2 * -siny);
                p[10] = (var1 * siny) + (var2 * cosy);
            }
        }

        public void RotateZ(float z)
        {
            float var1, var2;
            float cosz = (float)Math.Cos(z / 180.0f * Math.PI);
            float sinz = (float)Math.Sin(z / 180.0f * Math.PI);

            fixed (float* p = _data)
            {
                var1 = p[0]; var2 = p[1];
                p[0] = (var1 * cosz) + (var2 * sinz);
                p[1] = (var1 * -sinz) + (var2 * cosz);

                var1 = p[4]; var2 = p[5];
                p[4] = (var1 * cosz) + (var2 * sinz);
                p[5] = (var1 * -sinz) + (var2 * cosz);

                var1 = p[8]; var2 = p[9];
                p[8] = (var1 * cosz) + (var2 * sinz);
                p[9] = (var1 * -sinz) + (var2 * cosz);
            }
        }

        public static Matrix43 RotationMatrix(float x, float y, float z)
        {
            float cosx = (float)Math.Cos(x / 180.0f * Math.PI);
            float sinx = (float)Math.Sin(x / 180.0f * Math.PI);
            float cosy = (float)Math.Cos(y / 180.0f * Math.PI);
            float siny = (float)Math.Sin(y / 180.0f * Math.PI);
            float cosz = (float)Math.Cos(z / 180.0f * Math.PI);
            float sinz = (float)Math.Sin(z / 180.0f * Math.PI);

            Matrix43 m = Identity;
            float* p = (float*)&m;

            p[5] = cosx;
            p[6] = -sinx;
            p[9] = sinx;
            p[10] = cosx;

            Matrix43 m2 = Identity;
            float* p2 = (float*)&m2;
            p2[0] = cosy;
            p2[2] = siny;
            p2[8] = -siny;
            p2[10] = cosy;

            Matrix43 m3 = Identity;
            float* p3 = (float*)&m3;
            p3[0] = cosz;
            p3[1] = -sinz;
            p3[4] = sinz;
            p3[5] = cosz;

            m.Multiply(&m2);
            m.Multiply(&m3);

            //p[0] = cosy * cosz;
            //p[1] = cosy * sinz;
            //p[2] = -siny;
            //p[4] = (sinx * siny * cosz - cosx * sinz);
            //p[5] = (sinx * siny * sinz + cosx * cosz);
            //p[6] = sinx * cosy;
            //p[8] = (cosx * siny * cosz + sinx * sinz);
            //p[9] = (cosx * siny * sinz - sinx * cosz);
            //p[10] = cosx * cosy;

            return m;
        }

        public static Matrix43 TransformationMatrix(Vector3 scale, Vector3 rotation, Vector3 translation)
        {
            float cosx = (float)Math.Cos(rotation._x / 180.0 * Math.PI);
            float sinx = (float)Math.Sin(rotation._x / 180.0 * Math.PI);
            float cosy = (float)Math.Cos(rotation._y / 180.0 * Math.PI);
            float siny = (float)Math.Sin(rotation._y / 180.0 * Math.PI);
            float cosz = (float)Math.Cos(rotation._z / 180.0 * Math.PI);
            float sinz = (float)Math.Sin(rotation._z / 180.0 * Math.PI);

            Matrix43 m;
            float* p = (float*)&m;

            p[0] = scale._x * cosy * cosz;
            p[1] = scale._y * (sinx * siny * cosz - cosx * sinz);
            p[2] = scale._z * (cosx * siny * cosz + sinx * sinz);
            p[3] = translation._x;
            p[4] = scale._x * cosy * sinz;
            p[5] = scale._y * (sinx * siny * sinz + cosx * cosz);
            p[6] = scale._z * (cosx * siny * sinz - sinx * cosz);
            p[7] = translation._y;
            p[8] = -scale._x * siny;
            p[9] = scale._y * sinx * cosy;
            p[10] = scale._z * cosx * cosy;
            p[11] = translation._z;

            return m;
        }

        public static Matrix43 ReverseTransformMatrix(Vector3 scale, Vector3 rotation, Vector3 translation)
        {
            float cosx = (float)Math.Cos(rotation._x / 180.0 * Math.PI);
            float sinx = (float)Math.Sin(rotation._x / 180.0 * Math.PI);
            float cosy = (float)Math.Cos(rotation._y / 180.0 * Math.PI);
            float siny = (float)Math.Sin(rotation._y / 180.0 * Math.PI);
            float cosz = (float)Math.Cos(rotation._z / 180.0 * Math.PI);
            float sinz = (float)Math.Sin(rotation._z / 180.0 * Math.PI);

            scale._x = 1 / scale._x;
            scale._y = 1 / scale._y;
            scale._z = 1 / scale._z;
            translation._x = -translation._x;
            translation._y = -translation._y;
            translation._z = -translation._z;

            Matrix43 m;
            float* p = (float*)&m;

            p[0] = scale._x * cosy * cosz;
            p[1] = scale._x * cosy * sinz;
            p[2] = -scale._x * siny;
            p[3] = (translation._x * p[0]) + (translation._y * p[1]) + (translation._z * p[2]);
            p[4] = scale._y * (sinx * siny * cosz - cosx * sinz);
            p[5] = scale._y * (sinx * siny * sinz + cosx * cosz);
            p[6] = scale._y * sinx * cosy;
            p[7] = (translation._x * p[4]) + (translation._y * p[5]) + (translation._z * p[6]);
            p[8] = scale._z * (cosx * siny * cosz + sinx * sinz);
            p[9] = scale._z * (cosx * siny * sinz - sinx * cosz);
            p[10] = scale._z * cosx * cosy;
            p[11] = (translation._x * p[8]) + (translation._y * p[9]) + (translation._z * p[10]);

            return m;
        }


        public void Multiply(Matrix43* m)
        {
            Matrix43 m2 = this;

            float* s1 = (float*)&m2, s2 = (float*)m;

            fixed (float* p = _data)
            {
                int index = 0;
                float val;
                for (int b = 0; b < 12; b += 4)
                    for (int a = 0; a < 4; a++)
                    {
                        val = 0.0f;
                        for (int x = b, y = a; y < 12; y += 4)
                            val += s1[x++] * s2[y];
                        p[index++] = val;
                    }

                p[3] += s1[3];
                p[7] += s1[7];
                p[11] += s1[11];
            }
        }

        public Vector3 Multiply(Vector3 v)
        {
            Vector3 nv = new Vector3();
            fixed (float* p = _data)
            {
                nv._x = (p[0] * v._x) + (p[1] * v._y) + (p[2] * v._z) + p[3];
                nv._y = (p[4] * v._x) + (p[5] * v._y) + (p[6] * v._z) + p[7];
                nv._z = (p[8] * v._x) + (p[9] * v._y) + (p[10] * v._z) + p[11];
            }
            return nv;
        }
        public void Add(Matrix43* m)
        {
            float* s = (float*)m;
            fixed (float* d = _data)
                for (int i = 0; i < 12; i++)
                    d[i] += s[i];
        }
        public void Subtract(Matrix43* m)
        {
            float* s = (float*)m;
            fixed (float* d = _data)
                for (int i = 0; i < 12; i++)
                    d[i] -= s[i];
        }

        internal void Multiply(float v)
        {
            fixed (float* p = _data)
            {
                for (int i = 0; i < 12; i++)
                    p[i] *= v;
            }
        }

        public static Matrix43 operator *(Matrix43 m1, Matrix43 m2)
        {
            Matrix43 m;

            float* s1 = (float*)&m1, s2 = (float*)&m2, p = (float*)&m;

            int index = 0;
            float val;
            for (int b = 0; b < 12; b += 4)
                for (int a = 0; a < 4; a++)
                {
                    val = 0.0f;
                    for (int x = b, y = a; y < 12; y += 4)
                        val += s1[x++] * s2[y];
                    p[index++] = val;
                }

            p[3] += s1[3];
            p[7] += s1[7];
            p[11] += s1[11];

            return m;
        }

        public static bool operator ==(Matrix43 m1, Matrix43 m2)
        {
            float* p1 = (float*)&m1, p2 = (float*)&m2;
            for (int i = 0; i < 12; i++)
                if (*p1++ != *p2++)
                    return false;
            return true;
        }
        public static bool operator !=(Matrix43 m1, Matrix43 m2) { return !(m1 == m2); }

        public override bool Equals(object obj)
        {
            if (obj is Matrix43)
                return (Matrix43)obj == this;
            return base.Equals(obj);
        }

        public override string ToString()
        {
            fixed (float* p = _data)
                return String.Format("({0},{1},{2},{3})({4},{5},{6},{7})({8},{9},{10},{11})", p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], p[10], p[11]);
        }
    }
}
