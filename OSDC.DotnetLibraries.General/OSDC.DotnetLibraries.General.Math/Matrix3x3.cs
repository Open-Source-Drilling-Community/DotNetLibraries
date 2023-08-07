using OSDC.DotnetLibraries.General.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Matrix3x3 : IMatrix
    {
        private double?[,] M = null;

        public double? this[int col, int row] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int ColumnCount => throw new NotImplementedException();

        public int RowCount => throw new NotImplementedException();

        public IMatrix Add(IMatrix a)
        {
            throw new NotImplementedException();
        }

        public void AddAssign(IMatrix a)
        {
            throw new NotImplementedException();
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

        public IMatrix Negate()
        {
            throw new NotImplementedException();
        }

        public void NegateAssign()
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

        public IMatrix Substract(IMatrix a)
        {
            throw new NotImplementedException();
        }

        public void SubstractAssign(IMatrix a)
        {
            throw new NotImplementedException();
        }

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

        public void Transpose(ref IMatrix result)
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
    }
}
