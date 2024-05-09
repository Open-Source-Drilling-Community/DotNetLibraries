using MathNet.Numerics.RootFinding;
using OSDC.DotnetLibraries.General.Common;
using System;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Matrix3x3 : IMatrix
    {
        private double?[,] M = null;

        public int RowCount => 3;

        public int ColumnCount => 3;

        public double? this[int row, int col]
        {
            get
            {
                return M[row, col];
            }
            set
            {
                M[row, col] = value;
            }
        }

        public Matrix3x3() : base()
        {
            M = new double?[3, 3];
        }

        public Matrix3x3(Matrix3x3 m)
        {
            if (m != null && m[0, 0] != null && m[0, 1] != null && m[0, 2] != null && m[1, 0] != null && m[1, 1] != null && m[1, 2] != null && m[2, 0] != null && m[2, 1] != null && m[2, 2] != null)
            {
                M = new double?[3, 3];
                M[0, 0] = m[0, 0];
                M[0, 1] = m[0, 1];
                M[0, 2] = m[0, 2];
                M[1, 0] = m[1, 0];
                M[1, 1] = m[1, 1];
                M[1, 2] = m[1, 2];
                M[2, 0] = m[2, 0];
                M[2, 1] = m[2, 1];
                M[2, 2] = m[2, 2];
            }
        }

        public Matrix3x3(double? m00, double? m01, double? m02, double? m10, double? m11, double? m12, double? m20, double? m21, double? m22)
        {
            if (m00 != null && m01 != null && m02 != null && m10 != null && m11 != null && m12 != null && m20 != null && m21 != null && m22 != null)
            {
                M = new double?[3, 3];
                M[0, 0] = m00;
                M[0, 1] = m01;
                M[0, 2] = m02;
                M[1, 0] = m10;
                M[1, 1] = m11;
                M[1, 2] = m12;
                M[2, 0] = m20;
                M[2, 1] = m21;
                M[2, 2] = m22;
            }
        }

        public void GetColumn(int col, ref IVector v)
        {
            throw new NotImplementedException();
        }

        public void GetRow(int row, ref IVector v)
        {
            throw new NotImplementedException();
        }

        public void GetSubMatrix(int startCol, int startRow, ref IMatrix m)
        {
            throw new NotImplementedException();
        }

        public void SetColumn(int col, IVector v)
        {
            throw new NotImplementedException();
        }

        public void SetRow(int row, IVector v)
        {
            throw new NotImplementedException();
        }

        public void SetSubMatrix(int startCol, int startRow, IMatrix m)
        {
            throw new NotImplementedException();
        }

        #region IEquivalent<IMatrix> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(IMatrix other)
        {
            if (other != null)
            {
                if (M == null && other.RowCount == 0 && other.ColumnCount == 0)
                {
                    return true;
                }
                else if (M != null && M.GetLength(0) == other.ColumnCount && M.GetLength(1) == other.RowCount)
                {
                    int noCols = M.GetLength(0);
                    int noRows = M.GetLength(1);
                    for (int i = 0; i < noCols; i++)
                    {
                        for (int j = 0; j < noRows; j++)
                        {
                            if (!Numeric.EQ(M[i, j], other[i, j]))
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(IMatrix other, double precision)
        {
            if (other != null)
            {
                if (M == null && other.RowCount == 0 && other.ColumnCount == 0)
                {
                    return true;
                }
                else if (M != null && M.GetLength(0) == other.ColumnCount && M.GetLength(1) == other.RowCount)
                {
                    int noCols = M.GetLength(0);
                    int noRows = M.GetLength(1);
                    for (int i = 0; i < noCols; i++)
                    {
                        for (int j = 0; j < noRows; j++)
                        {
                            if (!Numeric.EQ(M[i, j], other[i, j], precision))
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        public void Time(double a)
        {
            throw new NotImplementedException();
        }

        public IVector Time(IVector a)
        {
            throw new NotImplementedException();
        }

        public void Time(IVector a, ref IVector b)
        {
            throw new NotImplementedException();
        }

        public IMatrix Time(IMatrix a)
        {
            throw new NotImplementedException();
        }

        public void Time(IMatrix a, ref IMatrix b)
        {
            throw new NotImplementedException();
        }


        public IMatrix Add(IMatrix a)
        {
            if (a == null || a.RowCount != 3 || a.ColumnCount != 3 || a[0, 0] == null || a[0, 1] == null || a[0, 2] == null || a[1, 0] == null || a[1, 1] == null || a[1, 2] == null || a[2, 0] == null || a[2, 1] == null || a[2, 2] == null)
            {
                return null;
            }
            return new Matrix3x3(M[0, 0] + a[0, 0],
                                 M[0, 1] + a[0, 1],
                                 M[0, 2] + a[0, 2],
                                 M[1, 0] + a[1, 0],
                                 M[1, 1] + a[1, 1],
                                 M[1, 2] + a[1, 2],
                                 M[2, 0] + a[2, 0],
                                 M[2, 1] + a[2, 1],
                                 M[2, 2] + a[2, 2]);
        }

        public void AddAssign(IMatrix a)
        {
            if (a != null && a.RowCount == 3 && a.ColumnCount == 3 && a[0, 0] != null && a[0, 1] != null && a[0, 2] != null && a[1, 0] != null && a[1, 1] != null && a[1, 2] != null && a[2, 0] != null && a[2, 1] != null && a[2, 2] != null)
            {
                M[0, 0] += a[0, 0];
                M[0, 1] += a[0, 1];
                M[0, 2] += a[0, 2];
                M[1, 0] += a[1, 0];
                M[1, 1] += a[1, 1];
                M[1, 2] += a[1, 2];
                M[2, 0] += a[2, 0];
                M[2, 1] += a[2, 1];
                M[2, 2] += a[2, 2];
            }
        }

        public IMatrix Negate()
        {
            return new Matrix3x3(-M[0, 0],
                                 -M[0, 1],
                                 -M[0, 2],
                                 -M[1, 0],
                                 -M[1, 1],
                                 -M[1, 2],
                                 -M[2, 0],
                                 -M[2, 1],
                                 -M[2, 2]);
        }

        public void NegateAssign()
        {
            M[0, 0] = -M[0, 0];
            M[0, 1] = -M[0, 1];
            M[0, 2] = -M[0, 2];
            M[1, 0] = -M[1, 0];
            M[1, 1] = -M[1, 1];
            M[1, 2] = -M[1, 2];
            M[2, 0] = -M[2, 0];
            M[2, 1] = -M[2, 1];
            M[2, 2] = -M[2, 2];
        }

        public IMatrix Substract(IMatrix a)
        {
            if (a == null || a.RowCount != 3 || a.ColumnCount != 3 || a[0, 0] == null || a[0, 1] == null || a[0, 2] == null || a[1, 0] == null || a[1, 1] == null || a[1, 2] == null || a[2, 0] == null || a[2, 1] == null || a[2, 2] == null)
            {
                return null;
            }
            return new Matrix3x3(M[0, 0] - a[0, 0],
                                 M[0, 1] - a[0, 1],
                                 M[0, 2] - a[0, 2],
                                 M[1, 0] - a[1, 0],
                                 M[1, 1] - a[1, 1],
                                 M[1, 2] - a[1, 2],
                                 M[2, 0] - a[2, 0],
                                 M[2, 1] - a[2, 1],
                                 M[2, 2] - a[2, 2]);
        }

        public void SubstractAssign(IMatrix a)
        {
            if (a != null && a.RowCount == 3 && a.ColumnCount == 3 && a[0, 0] != null && a[0, 1] != null && a[0, 2] != null && a[1, 0] != null && a[1, 1] != null && a[1, 2] != null && a[2, 0] != null && a[2, 1] != null && a[2, 2] != null)
            {
                M[0, 0] -= a[0, 0];
                M[0, 1] -= a[0, 1];
                M[0, 2] -= a[0, 2];
                M[1, 0] -= a[1, 0];
                M[1, 1] -= a[1, 1];
                M[1, 2] -= a[1, 2];
                M[2, 0] -= a[2, 0];
                M[2, 1] -= a[2, 1];
                M[2, 2] -= a[2, 2];
            }
        }

        public IMatrix Multiply(double s)
        {
            return new Matrix3x3(M[0, 0] * s,
                                 M[0, 1] * s,
                                 M[0, 2] * s,
                                 M[1, 0] * s,
                                 M[1, 1] * s,
                                 M[1, 2] * s,
                                 M[2, 0] * s,
                                 M[2, 1] * s,
                                 M[2, 2] * s);
        }

        public void MultiplyAssign(double s)
        {
            M[0, 0] *= s;
            M[0, 1] *= s;
            M[0, 2] *= s;
            M[1, 0] *= s;
            M[1, 1] *= s;
            M[1, 2] *= s;
            M[2, 0] *= s;
            M[2, 1] *= s;
            M[2, 2] *= s;
        }

        public IVector Multiply(IVector v)
        {
            if (v == null || v[0] == null || v[1] == null || v[2] == null)
            {
                return null;
            }
            return new Vector3D(M[0, 0] * v[0] + M[0, 1] * v[1] + M[0, 2] * v[2],
                                M[1, 0] * v[0] + M[1, 1] * v[1] + M[1, 2] * v[2],
                                M[2, 0] * v[0] + M[2, 1] * v[1] + M[2, 2] * v[2]
                );
        }

        public IMatrix Multiply(IMatrix m)
        {
            if (m == null || m.RowCount != 3 || m.ColumnCount != 3 || m[0, 0] == null || m[0, 1] == null || m[0, 2] == null || m[1, 0] == null || m[1, 1] == null || m[1, 2] == null || m[2, 0] == null || m[2, 1] == null || m[2, 2] == null)
            {
                return null;
            }
            return new Matrix3x3(M[0, 0] * m[0, 0] + M[0, 1] * m[1, 0] + M[0, 2] * m[2, 0],
                                 M[0, 0] * m[0, 1] + M[0, 1] * m[1, 1] + M[0, 2] * m[2, 1],
                                 M[0, 0] * m[0, 2] + M[0, 1] * m[1, 2] + M[0, 2] * m[2, 2],
                                 M[1, 0] * m[0, 0] + M[1, 1] * m[1, 0] + M[1, 2] * m[2, 0],
                                 M[1, 0] * m[0, 1] + M[1, 1] * m[1, 1] + M[1, 2] * m[2, 1],
                                 M[1, 0] * m[0, 2] + M[1, 1] * m[1, 2] + M[1, 2] * m[2, 2],
                                 M[2, 0] * m[0, 0] + M[2, 1] * m[1, 0] + M[2, 2] * m[2, 0],
                                 M[2, 0] * m[0, 1] + M[2, 1] * m[1, 1] + M[2, 2] * m[2, 1],
                                 M[2, 0] * m[0, 2] + M[2, 1] * m[1, 2] + M[2, 2] * m[2, 2]
                );
        }

        public void MultiplyAssign(IMatrix m)
        {
            if (m != null && m.RowCount == 3 && m.ColumnCount == 3 && m[0, 0] != null && m[0, 1] != null && m[0, 2] != null && m[1, 0] != null && m[1, 1] != null && m[1, 2] != null && m[2, 0] != null && m[2, 1] != null && m[2, 2] != null &&
                M != null && M[0, 0] != null && M[0, 1] != null && M[0, 2] != null && M[1, 0] != null && M[1, 1] != null && M[1, 2] != null && M[2, 0] != null && M[2, 1] != null && M[2, 2] != null)
            {
                double? m00 = M[0, 0];
                double? m01 = M[0, 1];
                double? m02 = M[0, 2];
                double? m10 = M[1, 0];
                double? m11 = M[1, 1];
                double? m12 = M[1, 2];
                double? m20 = M[2, 0];
                double? m21 = M[2, 1];
                double? m22 = M[2, 2];
                M[0, 0] = m00 * m[0, 0] + m01 * m[1, 0] + m02 * m[2, 0];
                M[0, 1] = m00 * m[0, 1] + m01 * m[1, 1] + m02 * m[2, 1];
                M[0, 2] = m00 * m[0, 2] + m01 * m[1, 2] + m02 * m[2, 2];
                M[1, 0] = m10 * m[0, 0] + m11 * m[1, 0] + m12 * m[2, 0];
                M[1, 1] = m10 * m[0, 1] + m11 * m[1, 1] + m12 * m[2, 1];
                M[1, 2] = m10 * m[0, 2] + m11 * m[1, 2] + m12 * m[2, 2];
                M[2, 0] = m20 * m[0, 0] + m21 * m[1, 0] + m22 * m[2, 0];
                M[2, 1] = m20 * m[0, 1] + m21 * m[1, 1] + m22 * m[2, 1];
                M[2, 2] = m20 * m[0, 2] + m21 * m[1, 2] + m22 * m[2, 2];
            }
        }

        /// <summary>
        /// Transpose this matrix
        /// </summary>
        public void Transpose()
        {
            double? tmp;

            tmp = M[1, 0];
            M[1, 0] = M[0, 1];
            M[0, 1] = tmp;

            tmp = M[2, 0];
            M[2, 0] = M[0, 2];
            M[0, 2] = tmp;

            tmp = M[2, 1];
            M[2, 1] = M[1, 2];
            M[1, 2] = tmp;
        }

        /// <summary>
        /// Transpose this matrix and store the result in the given matrix
        /// </summary>
        /// <param name="result"></param>
        public void Transpose(ref IMatrix result)
        {
            if (M != null && M[0, 0] != null && M[0, 1] != null && M[0, 2] != null && M[1, 0] != null && M[1, 1] != null && M[1, 2] != null && M[2, 0] != null && M[2, 1] != null && M[2, 2] != null)
            {
                result[0, 0] = M[0, 0];
                result[1, 1] = M[1, 1];
                result[2, 2] = M[2, 2];

                double? tmp1 = M[1, 0]; // using temp variables allows result to be this
                double? tmp2 = M[0, 1];
                result[0, 1] = tmp1;
                result[1, 0] = tmp2;

                tmp1 = M[2, 0];
                tmp2 = M[0, 2];
                result[0, 2] = tmp1;
                result[2, 0] = tmp2;

                tmp1 = M[2, 1];
                tmp2 = M[1, 2];
                result[1, 2] = tmp1;
                result[2, 1] = tmp2;
            }
        }

        /// <summary>
        /// Returns the determinant of this matrix
        /// </summary>
        /// <returns></returns>
        public double? Determinant()
        {
            if (M == null || M[0, 0] == null || M[0, 1] == null || M[0, 2] == null || M[1, 0] == null || M[1, 1] == null || M[1, 2] == null || M[2, 0] == null || M[2, 1] == null || M[2, 2] == null)
            {
                return null;
            }
            return M[0, 0] * (M[1, 1] * M[2, 2] - M[1, 2] * M[2, 1]) +
                   M[0, 1] * (M[1, 2] * M[2, 0] - M[1, 0] * M[2, 2]) +
                   M[0, 2] * (M[1, 0] * M[2, 1] - M[1, 1] * M[2, 0]);
        }

        /// <summary>
        /// Sets this matrix to a counter clockwise rotation about the X axis
        /// </summary>
        /// <param name="angle">the rotation angle in radians</param>
        public void RotXAssign(double angle)
        {
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);

            M[0, 0] = 1.0;
            M[0, 1] = 0.0;
            M[0, 2] = 0.0;
            M[1, 0] = 0.0;
            M[1, 1] = cos;
            M[1, 2] = -sin;
            M[2, 0] = 0.0;
            M[2, 1] = sin;
            M[2, 2] = cos;
        }

        /// <summary>
        /// Sets this matrix to a counter clockwise rotation about the Y axis
        /// </summary>
        /// <param name="angle">the rotation angle in radians</param>
        public void RotYAssign(double angle)
        {
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);

            M[0, 0] = cos;
            M[0, 1] = 0.0;
            M[0, 2] = sin;
            M[1, 0] = 0.0;
            M[1, 1] = 1.0;
            M[1, 2] = 0.0;
            M[2, 0] = -sin;
            M[2, 1] = 0.0;
            M[2, 2] = cos;
        }

        /// <summary>
        /// Sets this matrix to a counter clockwise rotation about the Z axis
        /// </summary>
        /// <param name="angle">the rotation angle in radians</param>
        public void RotZAssign(double angle)
        {
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);

            M[0, 0] = cos;
            M[0, 1] = -sin;
            M[0, 2] = 0.0;
            M[1, 0] = sin;
            M[1, 1] = cos;
            M[1, 2] = 0.0;
            M[2, 0] = 0.0;
            M[2, 1] = 0.0;
            M[2, 2] = 1.0;
        }

        /// <summary>
        /// Sets this matrix to a counter clockwise rotation about the given axis.
        /// </summary>
        /// <param name="angle">the rotation angle in radians</param>
        /// <param name="x">the x coordinate of the direction vector</param>
        /// <param name="y">the y coordinate of the direction vector</param>
        /// <param name="z">the z coordinate of the direction vector</param>
        public void RotAssign(double angle, double? x, double? y, double? z)
        {
            if (x != null && y != null && z != null)
            {
                double norm = System.Math.Sqrt((double)(x * x + y * y + z * z));

                if (norm < Numeric.DOUBLE_ACCURACY)
                {
                    M[0, 0] = 1.0;
                    M[0, 1] = 0.0;
                    M[0, 2] = 0.0;
                    M[1, 0] = 0.0;
                    M[1, 1] = 1.0;
                    M[1, 2] = 0.0;
                    M[2, 0] = 0.0;
                    M[2, 1] = 0.0;
                    M[2, 2] = 1.0;
                }
                else
                {
                    norm = 1.0 / norm;

                    x *= norm;
                    y *= norm;
                    z *= norm;

                    double sin = System.Math.Sin(angle);
                    double cos = System.Math.Cos(angle);
                    double t = 1.0 - cos;

                    double? xz = x * z;
                    double? xy = x * y;
                    double? yz = y * z;

                    M[0, 0] = t * x * x + cos;
                    M[0, 1] = t * xy - sin * z;
                    M[0, 2] = t * xz + sin * y;
                    M[1, 0] = t * xy + sin * z;
                    M[1, 1] = t * y * y + cos;
                    M[1, 2] = t * yz - sin * x;
                    M[2, 0] = t * xz - sin * y;
                    M[2, 1] = t * yz + sin * x;
                    M[2, 2] = t * z * z + cos;
                }
            }
        }

        /// <summary>
        /// Sets this matrix to a counter clockwise rotation about the given axis.
        /// </summary>
        /// <param name="angle">the rotation angle in radians</param>
        /// <param name="v">the rotation axis</param>
        public void RotAssign(double angle, Vector3D v)
        {
            if (v != null && v.X != null && v.Y != null && v.Z != null)
            {
                RotAssign(angle, (double)v.X, (double)v.Y, (double)v.Z);
            }
        }

        /// <summary>
        /// Inverts this matrix
        /// </summary>
        public bool InvertAssign()
        {
            if (M != null && M[0, 0] != null && M[0, 1] != null && M[0, 2] != null && M[1, 0] != null && M[1, 1] != null && M[1, 2] != null && M[2, 0] != null && M[2, 1] != null && M[2, 2] != null)
            {
                double[] tmp = new double[9];
                int[] row_perm = new int[3];

                // Copy source matrix to tmp
                tmp[0] = (double)M[0, 0];
                tmp[1] = (double)M[0, 1];
                tmp[2] = (double)M[0, 2];

                tmp[3] = (double)M[1, 0];
                tmp[4] = (double)M[1, 1];
                tmp[5] = (double)M[1, 2];

                tmp[6] = (double)M[2, 0];
                tmp[7] = (double)M[2, 1];
                tmp[8] = (double)M[2, 2];

                try
                {
                    // Calculate LU decomposition: Is the matrix singular?
                    if (!LUDecomposition(ref tmp, ref row_perm))
                    {
                        throw new Exception("Non invertible matrix exception");
                    }

                    // Perform back substitution on the identity matrix
                    double[] result = new double[] { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };

                    LUBacksubstitution(tmp, row_perm, ref result);

                    M[0, 0] = result[0];
                    M[0, 1] = result[1];
                    M[0, 2] = result[2];

                    M[1, 0] = result[3];
                    M[1, 1] = result[4];
                    M[1, 2] = result[5];

                    M[2, 0] = result[6];
                    M[2, 1] = result[7];
                    M[2, 2] = result[8];

                    return true;
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Matrix to invert is not invertible");
                }
            }
            else
            {
                System.Console.WriteLine("Matrix to invert is null or contains null elements");
            }
            return false;
        }

        public static IMatrix Invert(IMatrix m)
        {
            if (m != null && m.RowCount == 3 && m.ColumnCount == 3 && m[0, 0] != null && m[0, 1] != null && m[0, 2] != null && m[1, 0] != null && m[1, 1] != null && m[1, 2] != null && m[2, 0] != null && m[2, 1] != null && m[2, 2] != null)
            {
                double[] tmp = new double[9];
                int[] row_perm = new int[3];

                // Copy source matrix to tmp
                tmp[0] = (double)m[0, 0];
                tmp[1] = (double)m[0, 1];
                tmp[2] = (double)m[0, 2];

                tmp[3] = (double)m[1, 0];
                tmp[4] = (double)m[1, 1];
                tmp[5] = (double)m[1, 2];

                tmp[6] = (double)m[2, 0];
                tmp[7] = (double)m[2, 1];
                tmp[8] = (double)m[2, 2];

                try
                {
                    // Calculate LU decomposition: Is the matrix singular?
                    if (!LUDecomposition(ref tmp, ref row_perm))
                    {
                        throw new Exception("Non invertible matrix exception");
                    }

                    // Perform back substitution on the identity matrix
                    double[] result = new double[] { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };

                    LUBacksubstitution(tmp, row_perm, ref result);

                    Matrix3x3 M = new Matrix3x3();
                    M[0, 0] = (double)result[0];
                    M[0, 1] = (double)result[1];
                    M[0, 2] = (double)result[2];
                    M[1, 0] = (double)result[3];
                    M[1, 1] = (double)result[4];
                    M[1, 2] = (double)result[5];
                    M[2, 0] = (double)result[6];
                    M[2, 1] = (double)result[7];
                    M[2, 2] = (double)result[8];
                    return M;
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Matrix to invert is not invertible");
                }
            }
            else
            {
                System.Console.WriteLine("Matrix to invert is null or contains null elements");
            }
            return null;
        }

        private static bool LUDecomposition(ref double[] matrix, ref int[] row_perm)
        {
            double[] row_scale = new double[3];

            // Determine implicit scaling information by looping over rows
            {
                int i, j;
                int ptr, rs;
                double big, temp;

                ptr = 0;
                rs = 0;

                // For each row ...
                i = 3;
                while (i-- != 0)
                {
                    big = 0.0;

                    // For each column, find the largest element in the row
                    j = 3;
                    while (j-- != 0)
                    {
                        temp = matrix[ptr++];
                        temp = System.Math.Abs(temp);
                        if (temp > big)
                        {
                            big = temp;
                        }
                    }

                    // Is the matrix singular?
                    if (big == 0.0)
                    {
                        return false;
                    }
                    row_scale[rs++] = 1.0 / big;
                }
            }

            {
                int j;
                int mtx;

                mtx = 0;

                // For all columns, execute Crout's method
                for (j = 0; j < 3; j++)
                {
                    int i, imax, k;
                    int target, p1, p2;
                    double sum, big, temp;

                    // Determine elements of upper diagonal matrix U
                    for (i = 0; i < j; i++)
                    {
                        target = mtx + (3 * i) + j;
                        sum = matrix[target];
                        k = i;
                        p1 = mtx + (3 * i);
                        p2 = mtx + j;
                        while (k-- != 0)
                        {
                            sum -= matrix[p1] * matrix[p2];
                            p1++;
                            p2 += 3;
                        }
                        matrix[target] = sum;
                    }

                    // Search for largest pivot element and calculate
                    // intermediate elements of lower diagonal matrix L.
                    big = 0.0;
                    imax = -1;
                    for (i = j; i < 3; i++)
                    {
                        target = mtx + (3 * i) + j;
                        sum = matrix[target];
                        k = j;
                        p1 = mtx + (3 * i);
                        p2 = mtx + j;
                        while (k-- != 0)
                        {
                            sum -= matrix[p1] * matrix[p2];
                            p1++;
                            p2 += 3;
                        }
                        matrix[target] = sum;

                        // Is this the best pivot so far?
                        if ((temp = row_scale[i] * System.Math.Abs(sum)) >= big)
                        {
                            big = temp;
                            imax = i;
                        }
                    }

                    if (imax < 0)
                    {
                        throw new Exception("Non invertible matrix exception");
                    }

                    // Is a row exchange necessary?
                    if (j != imax)
                    {
                        // Yes: exchange rows
                        k = 3;
                        p1 = mtx + (3 * imax);
                        p2 = mtx + (3 * j);
                        while (k-- != 0)
                        {
                            temp = matrix[p1];
                            matrix[p1++] = matrix[p2];
                            matrix[p2++] = temp;
                        }

                        // Record change in scale factor
                        row_scale[imax] = row_scale[j];
                    }

                    // Record row permutation
                    row_perm[j] = imax;

                    // Is the matrix singular
                    if (matrix[(mtx + (3 * j) + j)] == 0.0)
                    {
                        return false;
                    }

                    // Divide elements of lower diagonal matrix L by pivot
                    if (j != (3 - 1))
                    {
                        temp = 1.0 / (matrix[(mtx + (3 * j) + j)]);
                        target = mtx + (3 * (j + 1)) + j;
                        i = 2 - j;
                        while (i-- != 0)
                        {
                            matrix[target] *= temp;
                            target += 3;
                        }
                    }
                }
            }

            return true;
        }

        private static void LUBacksubstitution(double[] matrix1, int[] row_perm, ref double[] matrix2)
        {
            int i, ii, ip, j, k;
            int rp;
            int cv, rv;

            // rp = row_perm;
            rp = 0;

            // For each column vector of matrix2 ...
            for (k = 0; k < 3; k++)
            {
                // cv = &(matrix2[0][k]);
                cv = k;
                ii = -1;

                // Forward substitution
                for (i = 0; i < 3; i++)
                {
                    double sum;

                    ip = row_perm[rp + i];
                    sum = matrix2[cv + 3 * ip];
                    matrix2[cv + 3 * ip] = matrix2[cv + 3 * i];
                    if (ii >= 0)
                    {
                        // rv = &(matrix1[i][0]);
                        rv = i * 3;
                        for (j = ii; j <= i - 1; j++)
                        {
                            sum -= matrix1[rv + j] * matrix2[cv + 3 * j];
                        }
                    }
                    else if (sum != 0.0)
                    {
                        ii = i;
                    }
                    matrix2[cv + 3 * i] = sum;
                }

                // Backsubstitution
                // rv = &(matrix1[3][0]);
                rv = 2 * 3;
                matrix2[cv + 3 * 2] /= matrix1[rv + 2];

                rv -= 3;
                matrix2[cv + 3 * 1] = (matrix2[cv + 3 * 1] -
                        matrix1[rv + 2] * matrix2[cv + 3 * 2]) / matrix1[rv + 1];

                rv -= 3;
                matrix2[cv + 4 * 0] = (matrix2[cv + 3 * 0] -
                        matrix1[rv + 1] * matrix2[cv + 3 * 1] -
                        matrix1[rv + 2] * matrix2[cv + 3 * 2]) / matrix1[rv + 0];

            }
        }
    }
}
