using UnityEngine;

using System;

[System.Diagnostics.DebuggerStepThrough]
public class Matrix3x3
{
    public static class InertiaTensors
    {
        public static Matrix3x3 Cuboid(float mass, Vector3 size)
        {
            return new Matrix3x3(
                (size.y * size.y) + (size.z * size.z), 0, 0,
                0, (size.x * size.x) + (size.z * size.z), 0,
                0, 0, (size.x * size.x) + (size.y * size.y)
            ) * (mass / 12);
        }

        public static Matrix3x3 Sphere(float mass, float radius, bool isHollow = false)
        {
            Matrix3x3 tensor = Identity * (2.0f / (isHollow ? 3 : 5) * mass * (radius * radius));
            return tensor;
        }
    }

    public static Matrix3x3 Zero => new Matrix3x3(zero);
    public static Matrix3x3 Identity => new Matrix3x3(identity);

    private static readonly float[] zero = new float[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private static readonly float[] identity = new float[9] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };

    public float[] data = new float[9];

    public Matrix3x3()
    {
        Array.Copy(identity, data, 9);
    }

    public Matrix3x3(params float[] list)
    {
        Array.Copy(list, data, 9);
    }
    public Matrix3x3(Matrix3x3 m)
    {
        Array.Copy(m.data, data, 9);
    }

    public Matrix3x3(Vector3 x, Vector3 y, Vector3 z)
    {
        data = new float[]
        {
            x.x, y.x, z.x,
            x.y, y.y, z.y,
            x.z, y.z, z.z
        };
    }

    public static Matrix3x3 operator+(Matrix3x3 m1, Matrix3x3 m2)
    {
        Matrix3x3 result = new Matrix3x3(m1);

        for (int i = 0; i < 9; i++)
            result.data[i] += m2[i];

        return result;
    }
    
	public static Matrix3x3 operator*(Matrix3x3 m, float s)
	{
		Matrix3x3 result = new Matrix3x3(m);

		for (int i = 0; i < 9; i++)
			result.data[i] *= s;

		return result;
	}
	public static Vector3 operator*(Matrix3x3 m, Vector3 v)
	{
		return new Vector3(
			v.x * m[0] + v.y * m[1] + v.z * m[2],
			v.x * m[3] + v.y * m[4] + v.z * m[5],
			v.x * m[6] + v.y * m[7] + v.z * m[8]
		);
	}

	public static Matrix3x3 operator*(Matrix3x3 m1, Matrix3x3 m2)
	{
		return new Matrix3x3(
			m1[0]*m2[0] + m1[1]*m2[3] + m1[2]*m2[6],
			m1[0]*m2[1] + m1[1]*m2[4] + m1[2]*m2[7],
			m1[0]*m2[2] + m1[1]*m2[5] + m1[2]*m2[8],

			m1[3]*m2[0] + m1[4]*m2[3] + m1[5]*m2[6],
			m1[3]*m2[1] + m1[4]*m2[4] + m1[5]*m2[7],
			m1[3]*m2[2] + m1[4]*m2[5] + m1[5]*m2[8],

			m1[6]*m2[0] + m1[7]*m2[3] + m1[8]*m2[6],
			m1[6]*m2[1] + m1[7]*m2[4] + m1[8]*m2[7],
			m1[6]*m2[2] + m1[7]*m2[5] + m1[8]*m2[8]
		);
    }

    public static bool operator ==(Matrix3x3 left, Matrix3x3 right) => left.Equals(right);
    public static bool operator !=(Matrix3x3 left, Matrix3x3 right) => !(left == right);

    public static Matrix3x3 Invert(Matrix3x3 m)
	{
		float t1 = m[0] * m[4];
		float t2 = m[0] * m[5];
		float t3 = m[1] * m[3];
		float t4 = m[2] * m[3];
		float t5 = m[1] * m[6];
		float t6 = m[2] * m[6];

		float det = t1*m[8] - t2*m[7] - t3*m[8] + t4*m[7] + t5*m[5] - t6*m[4];

		if (det == 0.0f) return null;
		float invd = 1.0f / det;

		Matrix3x3 result = new Matrix3x3(
			 (m[4] * m[8] - m[5] * m[7]),
			-(m[1] * m[8] - m[2] * m[7]),
			 (m[1] * m[5] - m[2] * m[4]),
			-(m[3] * m[8] - m[5] * m[6]),
			  m[0] * m[8] - t6,
			-(t2          - t4),
			 (m[3] * m[7] - m[4] * m[6]),
			-(m[0] * m[7] - t5),
			 (t1		  - t3)
		);

		return result * invd;
	}

	public static Matrix3x3 Transpose(Matrix3x3 m)
	{
		return new Matrix3x3(
			m[0], m[3], m[6],
			m[1], m[4], m[7],
			m[2], m[5], m[8]
		);
	}

	public float this[int i]
	{
		get => data[i];
		set { data[i] = value; }
	}

	public Vector3 Transform(Vector3 v) => this * v;
	public Matrix3x3 GetInverse() => Invert(this);
	public Matrix3x3 GetTranspose() => Transpose(this);

	public void Invert() => data = Invert(this).data;
	public void Transpose() => data = Transpose(this).data;

	public void SetOrientation(Quaternion q)
	{
		data[0] = 1 - (2 * q.z * q.z + 2 * q.w * q.w);
		data[1] = 2 * q.y * q.z + 2 * q.w * q.x;
		data[2] = 2 * q.y * q.w - 2 * q.z * q.x;
		data[3] = 2 * q.y * q.z - 2 * q.w * q.x;
		data[4] = 1 - (2 * q.y * q.y + 2 * q.w * q.w);
		data[5] = 2 * q.z * q.w + 2 * q.y * q.x;
		data[6] = 2 * q.y * q.w + 2 * q.z * q.x;
		data[7] = 2 * q.z * q.w - 2 * q.y * q.x;
		data[8] = 1 - (2 * q.y * q.y + 2 * q.z * q.z);
	}

    public void SetSkewSymmetric(Vector3 v)
    {
        data[0] = data[4] = data[8] = 0;
        data[1] = -v.z;
        data[2] = v.y;
        data[3] = v.z;
        data[5] = -v.x;
        data[6] = -v.y;
        data[7] = v.x;
    }

    public override bool Equals(object obj) => (obj is Matrix3x3) ? data.Equals(((Matrix3x3)obj).data) : false;
    public override int GetHashCode() => data.GetHashCode();
}
