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

    public static Matrix3x3 Add(Matrix3x3 a, Matrix3x3 b)
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

    public static Matrix3x3 Multiply(Matrix3x3 a, Matrix3x3 b)
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

    public static Matrix3x3 Multiply(Matrix3x3 a, float b)
    {
        return new Matrix3x3
        {
            m00 = a.m00 * b,
            m01 = a.m01 * b,
            m02 = a.m02 * b,
            m10 = a.m10 * b,
            m11 = a.m11 * b,
            m12 = a.m12 * b,
            m20 = a.m20 * b,
            m21 = a.m21 * b,
            m22 = a.m22 * b,
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
}
