using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Matrix3x3
{
    public float m00;
    public float m01;
    public float m02;
    public float m10;
    public float m11;
    public float m12;
    public float m20;
    public float m21;
    public float m22;

    public float determinant
    {
        get
        {
            return m00 * (m11 * m22 - m12 * m21) - m01 * (m10 * m22 - m12 * m20) + m02 * (m10 * m21 - m11 * m20);
        }
    }

    public Matrix3x3 inverse
    {
        get
        {
            float det = determinant;
            if (det == 0)
                return zero;
            else
            {
                float invdet = 1 / det;
                return new Matrix3x3
                {
                    m00 = (m11 * m22 - m21 * m12) * invdet,
                    m01 = (m02 * m21 - m01 * m22) * invdet,
                    m02 = (m01 * m12 - m02 * m11) * invdet,
                    m10 = (m12 * m20 - m10 * m22) * invdet,
                    m11 = (m00 * m22 - m02 * m20) * invdet,
                    m12 = (m10 * m02 - m00 * m12) * invdet,
                    m20 = (m10 * m21 - m20 * m11) * invdet,
                    m21 = (m20 * m01 - m00 * m21) * invdet,
                    m22 = (m00 * m11 - m10 * m01) * invdet
                };
            }
        }
    }

    public Matrix4x4 toAffine4x4
    {
        get
        {
            return new Matrix4x4
            {
                m00 = m00,
                m01 = m01,
                m02 = m02,
                m03 = 0,
                m10 = m10,
                m11 = m11,
                m12 = m12,
                m13 = 0,
                m20 = m20,
                m21 = m21,
                m22 = m22,
                m23 = 0,
                m30 = 0,
                m31 = 0,
                m32 = 0,
                m33 = 1
            };
        }
    }

    public Vector3 GetRow(int index)
    {
        switch (index)
        {
            case 0:
                return new Vector3(m00, m01, m02);
            case 1:
                return new Vector3(m10, m11, m12);
            case 2:
                return new Vector3(m20, m21, m22);
            default:
                return Vector3.zero;
        }
    }

    public Vector3 GetColumn(int index)
    {
        switch (index)
        {
            case 0:
                return new Vector3(m00, m10, m20);
            case 1:
                return new Vector3(m01, m11, m21);
            case 2:
                return new Vector3(m02, m12, m22);
            default:
                return Vector3.zero;
        }
    }

    public void SetRow(int index, Vector3 row)
    {
        switch (index)
        {
            case 0:
                m00 = row.x;
                m01 = row.y;
                m02 = row.z;
                return;
            case 1:
                m10 = row.x;
                m11 = row.y;
                m12 = row.z;
                return;
            case 2:
                m20 = row.x;
                m21 = row.y;
                m22 = row.z;
                return;
            default:
                return;
        }
    }

    public void SetColumn(int index, Vector3 column)
    {
        switch (index)
        {
            case 0:
                m00 = column.x;
                m10 = column.y;
                m20 = column.z;
                return;
            case 1:
                m01 = column.x;
                m11 = column.y;
                m21 = column.z;
                return;
            case 2:
                m02 = column.x;
                m12 = column.y;
                m22 = column.z;
                return;
            default:
                return;
        }
    }

    public Vector3 Transform(Vector3 vector)
    {
        return new Vector3
        {
            x = m00 * vector.x + m01 * vector.y + m02 * vector.z,
            y = m10 * vector.x + m11 * vector.y + m12 * vector.z,
            z = m20 * vector.x + m21 * vector.y + m22 * vector.z
        };
    }

    public static Matrix3x3 zero
    {
        get
        {
            Matrix3x3 zero = new Matrix3x3
            {
                m00 = 0,
                m01 = 0,
                m02 = 0,
                m10 = 0,
                m11 = 0,
                m12 = 0,
                m20 = 0,
                m21 = 0,
                m22 = 0
            };
            return zero;
        }
    }

    public static Matrix3x3 identity
    {
        get
        {
            Matrix3x3 identity = new Matrix3x3
            {
                m00 = 1,
                m01 = 0,
                m02 = 0,
                m10 = 0,
                m11 = 1,
                m12 = 0,
                m20 = 0,
                m21 = 0,
                m22 = 1
            };
            return identity;
        }
    }

    public static Matrix3x3 OuterProduct(Vector3 a, Vector3 b)
    {
        return new Matrix3x3
        {
            m00 = a.x * b.x,
            m01 = a.x * b.y,
            m02 = a.x * b.z,
            m10 = a.y * b.x,
            m11 = a.y * b.y,
            m12 = a.y * b.z,
            m20 = a.z * b.x,
            m21 = a.z * b.y,
            m22 = a.z * b.z
        };
    }

    public static Matrix3x3 Rotate(Quaternion q)
    {
        return Convert4x4To3x3(Matrix4x4.Rotate(q));
    }

    public static Matrix3x3 Convert4x4To3x3(Matrix4x4 mat)
    {
        return new Matrix3x3
        {
            m00 = mat.m00,
            m01 = mat.m01,
            m02 = mat.m02,
            m10 = mat.m10,
            m11 = mat.m11,
            m12 = mat.m12,
            m20 = mat.m20,
            m21 = mat.m21,
            m22 = mat.m22
        };
    }

    public static Matrix3x3 operator +(Matrix3x3 a, Matrix3x3 b)
    {
        return new Matrix3x3
        {
            m00 = a.m00 + b.m00,
            m01 = a.m01 + b.m01,
            m02 = a.m02 + b.m02,
            m10 = a.m10 + b.m10,
            m11 = a.m11 + b.m11,
            m12 = a.m12 + b.m12,
            m20 = a.m20 + b.m20,
            m21 = a.m21 + b.m21,
            m22 = a.m22 + b.m22,
        };
    }

    public static Matrix3x3 operator -(Matrix3x3 a, Matrix3x3 b)
    {
        return new Matrix3x3
        {
            m00 = a.m00 - b.m00,
            m01 = a.m01 - b.m01,
            m02 = a.m02 - b.m02,
            m10 = a.m10 - b.m10,
            m11 = a.m11 - b.m11,
            m12 = a.m12 - b.m12,
            m20 = a.m20 - b.m20,
            m21 = a.m21 - b.m21,
            m22 = a.m22 - b.m22,
        };
    }

    public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b)
    {
        return new Matrix3x3
        {
            m00 = (a.m00 * b.m00) + (a.m01 * b.m10) + (a.m02 * b.m20),
            m01 = (a.m00 * b.m01) + (a.m01 * b.m11) + (a.m02 * b.m21),
            m02 = (a.m00 * b.m02) + (a.m01 * b.m12) + (a.m02 * b.m22),
            m10 = (a.m10 * b.m00) + (a.m11 * b.m10) + (a.m12 * b.m20),
            m11 = (a.m10 * b.m01) + (a.m11 * b.m11) + (a.m12 * b.m21),
            m12 = (a.m10 * b.m02) + (a.m11 * b.m12) + (a.m12 * b.m22),
            m20 = (a.m20 * b.m00) + (a.m21 * b.m10) + (a.m22 * b.m20),
            m21 = (a.m20 * b.m01) + (a.m21 * b.m11) + (a.m22 * b.m21),
            m22 = (a.m20 * b.m02) + (a.m21 * b.m12) + (a.m22 * b.m22),
        };
    }

    public static Matrix3x3 operator *(Matrix3x3 a, float scalar)
    {
        return new Matrix3x3
        {
            m00 = a.m00 * scalar,
            m01 = a.m01 * scalar,
            m02 = a.m02 * scalar,
            m10 = a.m10 * scalar,
            m11 = a.m11 * scalar,
            m12 = a.m12 * scalar,
            m20 = a.m20 * scalar,
            m21 = a.m21 * scalar,
            m22 = a.m22 * scalar,
        };
    }

    public static Matrix3x3 operator *(float scalar, Matrix3x3 a)
    {
        return new Matrix3x3
        {
            m00 = a.m00 * scalar,
            m01 = a.m01 * scalar,
            m02 = a.m02 * scalar,
            m10 = a.m10 * scalar,
            m11 = a.m11 * scalar,
            m12 = a.m12 * scalar,
            m20 = a.m20 * scalar,
            m21 = a.m21 * scalar,
            m22 = a.m22 * scalar
        };
    }

    public static Matrix3x3 operator /(Matrix3x3 a, float scalar)
    {
        return new Matrix3x3
        {
            m00 = a.m00 / scalar,
            m01 = a.m01 / scalar,
            m02 = a.m02 / scalar,
            m10 = a.m10 / scalar,
            m11 = a.m11 / scalar,
            m12 = a.m12 / scalar,
            m20 = a.m20 / scalar,
            m21 = a.m21 / scalar,
            m22 = a.m22 / scalar,
        };
    }

    public static bool operator ==(Matrix3x3 a, Matrix3x3 b)
    {
        if (a.m00 == b.m00 && a.m01 == b.m01 && a.m02 == b.m02 &&
            a.m10 == b.m10 && a.m11 == b.m11 && a.m12 == b.m12 &&
            a.m20 == b.m20 && a.m21 == b.m21 && a.m22 == b.m22)
            return true;
        else
            return false;
    }

    public static bool operator !=(Matrix3x3 a, Matrix3x3 b)
    {
        if (a.m00 != b.m00 || a.m01 != b.m01 || a.m02 != b.m02 ||
            a.m10 != b.m10 || a.m11 != b.m11 || a.m12 != b.m12 ||
            a.m20 != b.m20 || a.m21 != b.m21 || a.m22 != b.m22)
            return true;
        else
            return false;
    }
}
