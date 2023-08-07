using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Matrix : IMatrix, IEquivalent<Matrix>
    {
        private double?[,] data_ = null;

        public Matrix()
        {
           
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="noCols"></param>
        /// <param name="noRows"></param>
        public Matrix(int noCols, int noRows)
        {
            data_ = new double?[noCols, noRows];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noCols"></param>
        /// <param name="noRows"></param>
        /// <param name="def"></param>
        public Matrix(int noCols, int noRows, double def)
        {
            data_ = new double?[noCols, noRows];
            for (int i = 0; i < noCols; i++)
            {
                for (int j = 0; j < noRows; j++)
                {
                    data_[i, j] = def;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noCols"></param>
        /// <param name="noRows"></param>
        /// <param name="values"></param>
        public Matrix(int noCols, int noRows, double[] values)
        {
            data_ = new double?[noCols, noRows];
            if (values.Length >= noCols * noRows)
            {
                int tmp = 0;
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        data_[i, j] = values[tmp++]; ;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        public Matrix(IMatrix m)
        {
            if (m != null)
            {
                int noCols = m.ColumnCount;
                int noRows = m.RowCount;
                data_ = new double?[noCols, noRows];
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        data_[i, j] = m[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        public Matrix(Matrix m)
        {
            if (m != null)
            {
                int noCols = m.ColumnCount;
                int noRows = m.RowCount;
                data_ = new double?[noCols, noRows];
                Array.Copy(m.data_, data_, noCols * noRows);
            }
        }

        #region IMatrix Members
        /// <summary>
        /// 
        /// </summary>
        public int ColumnCount
        {
            get
            {
                if (data_ == null)
                {
                    return 0;
                }
                else
                {
                    return data_.GetLength(0);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int RowCount
        {
            get
            {
                if (data_ == null)
                {
                    return 0;
                }
                else
                {
                    return data_.GetLength(1);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public double? this[int col, int row]
        {
            get
            {
                if (data_ == null)
                {
                    return Numeric.UNDEF_DOUBLE;
                }
                else
                {
                    return data_[col, row];
                }
            }
            set
            {
                if (data_ != null)
                {
                    data_[col, row] = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <param name="v"></param>
        public void GetColumn(int col, ref IVector v)
        {
            if (data_ != null && v != null && v.Dim >= data_.GetLength(1))
            {
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noRows; i++)
                {
                    v[i] = data_[col, i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="v"></param>
        public void GetRow(int row, ref IVector v)
        {
            if (data_ != null && v != null && v.Dim >= data_.GetLength(0))
            {
                int noCols = data_.GetLength(0);
                for (int i = 0; i < noCols; i++)
                {
                    v[i] = data_[i, row];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <param name="v"></param>
        public void SetColumn(int col, IVector v)
        {
            if (data_ != null && v != null && v.Dim >= data_.GetLength(1))
            {
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noRows; i++)
                {
                    data_[col, i] = v[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="v"></param>
        public void SetRow(int row, IVector v)
        {
            if (data_ != null && v != null && v.Dim >= data_.GetLength(0))
            {
                int noCols = data_.GetLength(0);
                for (int i = 0; i < noCols; i++)
                {
                    data_[i, row] = v[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startCol"></param>
        /// <param name="startRow"></param>
        /// <param name="m"></param>
        public void GetSubMatrix(int startCol, int startRow, ref IMatrix m)
        {
            if (m != null && data_ != null)
            {
                int colSpan = System.Math.Min(data_.GetLength(0) - startCol, m.ColumnCount);
                int rowSpan = System.Math.Min(data_.GetLength(1) - startRow, m.RowCount);
                for (int i = 0; i < colSpan; i++)
                {
                    for (int j = 0; j < rowSpan; j++)
                    {
                        m[i, j] = data_[startCol + i, startRow + j];
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startCol"></param>
        /// <param name="startRow"></param>
        /// <param name="m"></param>
        public void SetSubMatrix(int startCol, int startRow, IMatrix m)
        {
            if (m != null && data_ != null)
            {
                int colSpan = System.Math.Min(data_.GetLength(0) - startCol, m.ColumnCount);
                int rowSpan = System.Math.Min(data_.GetLength(1) - startRow, m.RowCount);
                for (int i = 0; i < colSpan; i++)
                {
                    for (int j = 0; j < rowSpan; j++)
                    {
                        data_[startCol + i, startRow + j] = m[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public void Transpose(ref IMatrix result)
        {
            if (result != null && data_ != null)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                if (result.ColumnCount >= noRows && result.RowCount >= noCols)
                {
                    for (int i = 0; i < noCols; i++)
                    {
                        for (int j = 0; j < noRows; j++)
                        {
                            result[j, i] = data_[i, j];
                        }
                    }
                }
            }
        }

        #endregion

        #region IAddable<IMatrix> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public IMatrix Add(IMatrix a)
        {
            if (a == null || a.RowCount != RowCount || a.ColumnCount != ColumnCount)
            {
                return null;
            }
            else
            {
                Matrix b = new Matrix(ColumnCount, RowCount);
                if (data_ != null)
                {
                    int noCols = data_.GetLength(0);
                    int noRows = data_.GetLength(1);
                    for (int i = 0; i < noCols; i++)
                    {
                        for (int j = 0; j < noRows; j++)
                        {
                            b.data_[i, j] = data_[i, j] + a[i, j];
                        }
                    }
                }
                return b;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IMatrix Negate()
        {
            Matrix b = new Matrix(ColumnCount, RowCount);
            if (data_ != null)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        b.data_[i, j] = -data_[i, j];
                    }
                }
            }
            return b;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public IMatrix Substract(IMatrix a)
        {
            if (a == null || a.RowCount != RowCount || a.ColumnCount != ColumnCount)
            {
                return null;
            }
            else
            {
                Matrix b = new Matrix(ColumnCount, RowCount);
                if (data_ != null)
                {
                    int noCols = data_.GetLength(0);
                    int noRows = data_.GetLength(1);
                    for (int i = 0; i < noCols; i++)
                    {
                        for (int j = 0; j < noRows; j++)
                        {
                            b.data_[i, j] = data_[i, j] - a[i, j];
                        }
                    }
                }
                return b;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Add(IMatrix a, ref IMatrix b)
        {
            if (a != null && b != null && a.RowCount == RowCount && a.ColumnCount == ColumnCount && b.RowCount == RowCount && b.ColumnCount == ColumnCount && data_ != null)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        b[i, j] = data_[i, j] + a[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void Negate(ref IMatrix a)
        {
            if (a != null && a.RowCount == RowCount && a.ColumnCount == ColumnCount && data_ != null)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        a[i, j] = -data_[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Substract(IMatrix a, ref IMatrix b)
        {
            if (a != null && b != null && a.RowCount == RowCount && a.ColumnCount == ColumnCount && b.RowCount == RowCount && b.ColumnCount == ColumnCount && data_ != null)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        b[i, j] = data_[i, j] - a[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void AddAssign(IMatrix a)
        {
            if (a != null && data_ != null && data_.GetLength(0) == a.ColumnCount && data_.GetLength(1) == a.RowCount)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        data_[i, j] = data_[i, j] + a[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void NegateAssign()
        {
            if (data_ != null)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        data_[i, j] = -data_[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void SubstractAssign(IMatrix a)
        {
            if (a != null && data_ != null && data_.GetLength(0) == a.ColumnCount && data_.GetLength(1) == a.RowCount)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        data_[i, j] = data_[i, j] - a[i, j];
                    }
                }
            }
        }

        #endregion

        #region IProductable<IVector> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Time(IVector a, ref IVector b)
        {
            if (a != null && b != null && a.Dim == ColumnCount && b.Dim == RowCount)
            {
                int noCols = ColumnCount;
                int noRows = RowCount;
                for (int j = 0; j < noRows; j++)
                {
                    double? value = 0;
                    for (int i = 0; i < noCols; i++)
                    {
                        value = value + data_[i, j] * a[i];
                    }
                    b[j] = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public IVector Time(IVector a)
        {
            if (a != null && a.Dim == ColumnCount)
            {
                Vector b = new Vector(RowCount);
                int noCols = ColumnCount;
                int noRows = RowCount;
                for (int j = 0; j < noRows; j++)
                {
                    double? value = 0;
                    for (int i = 0; i < noCols; i++)
                    {
                        value = value + data_[i, j] * a[i];
                    }
                    b[j] = value;
                }
                return b;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region IProductable<IMatrix> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Time(IMatrix a, ref IMatrix b)
        {
            if (a != null && b != null && ColumnCount == a.RowCount && b.RowCount == RowCount && b.ColumnCount == a.ColumnCount)
            {
                int noRows = RowCount;
                int noCols = a.ColumnCount;
                int count = ColumnCount;
                for (int j = 0; j < noRows; j++)
                {
                    for (int i = 0; i < noCols; i++)
                    {
                        double? value = 0;
                        for (int k = 0; k < count; k++)
                        {
                            value = value + data_[k, j] * a[i, k];
                        }
                        b[i, j] = value;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public IMatrix Time(IMatrix a)
        {
            if (a != null && ColumnCount == a.RowCount)
            {
                Matrix b = new Matrix(a.ColumnCount, RowCount);
                int noRows = RowCount;
                int noCols = a.ColumnCount;
                int count = ColumnCount;
                for (int j = 0; j < noRows; j++)
                {
                    for (int i = 0; i < noCols; i++)
                    {
                        double? value = 0;
                        for (int k = 0; k < count; k++)
                        {
                            value = value + data_[k, j] * a[i, k];
                        }
                        b[i, j] = value;
                    }
                }
                return b;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region IScalarProduct<T> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public void Time(double x)
        {
            if (data_ != null)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        data_[i, j] = x * data_[i, j];
                    }
                }
            }
        }

        #endregion

        #region IZero Members
        /// <summary>
        /// 
        /// </summary>
        public void SetZero()
        {
            if (data_ != null)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        data_[i, j] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            if (data_ == null)
            {
                return true;
            }
            else
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        if (!Numeric.EQ(data_[i, j], 0))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        #endregion

        #region IUndefinable Members
        /// <summary>
        /// 
        /// </summary>
        public void SetUndefined()
        {
            if (data_ != null)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        data_[i, j] = Numeric.UNDEF_DOUBLE;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUndefined()
        {
            if (data_ != null)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        if (Numeric.IsUndefined(data_[i, j]))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region ICopyable<IMatrix> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref IMatrix item)
        {
            if (item != null && data_ != null && data_.GetLength(0) == item.ColumnCount && data_.GetLength(1) == item.RowCount)
            {
                int noCols = data_.GetLength(0);
                int noRows = data_.GetLength(1);
                for (int i = 0; i < noCols; i++)
                {
                    for (int j = 0; j < noRows; j++)
                    {
                        item[i, j] = data_[i, j];
                    }
                }
            }
        }

        #endregion

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
                if (data_ == null && other.RowCount == 0 && other.ColumnCount == 0)
                {
                    return true;
                }
                else if (data_ != null && data_.GetLength(0) == other.ColumnCount && data_.GetLength(1) == other.RowCount)
                {
                    int noCols = data_.GetLength(0);
                    int noRows = data_.GetLength(1);
                    for (int i = 0; i < noCols; i++)
                    {
                        for (int j = 0; j < noRows; j++)
                        {
                            if (!Numeric.EQ(data_[i, j], other[i, j]))
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
                if (data_ == null && other.RowCount == 0 && other.ColumnCount == 0)
                {
                    return true;
                }
                else if (data_ != null && data_.GetLength(0) == other.ColumnCount && data_.GetLength(1) == other.RowCount)
                {
                    int noCols = data_.GetLength(0);
                    int noRows = data_.GetLength(1);
                    for (int i = 0; i < noCols; i++)
                    {
                        for (int j = 0; j < noRows; j++)
                        {
                            if (!Numeric.EQ(data_[i, j], other[i, j], precision))
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

        #region ICopyable<Matrix<T,Calculator>> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref Matrix item)
        {
            if (item != null && data_ != null && item.data_.GetLength(0) == data_.GetLength(0) && item.data_.GetLength(1) == data_.GetLength(1))
            {
                Array.Copy(data_, item.data_, data_.Length);
            }
        }

        #endregion

        #region IEquivalent<Matrix<T,Calculator>> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(Matrix other)
        {
            if (other != null)
            {
                if (data_ == null && other.data_ == null)
                {
                    return true;
                }
                else if (data_ != null && other.data_ != null &&
                    data_.GetLength(0) == other.data_.GetLength(0) &&
                    data_.GetLength(1) == other.data_.GetLength(1))
                {
                    int noCols = data_.GetLength(0);
                    int noRows = data_.GetLength(1);
                    for (int i = 0; i < noCols; i++)
                    {
                        for (int j = 0; j < noRows; j++)
                        {
                            if (!Numeric.EQ(data_[i, j], other.data_[i, j]))
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
        public bool EQ(Matrix other, double precision)
        {
            if (other != null)
            {
                if (data_ == null && other.data_ == null)
                {
                    return true;
                }
                else if (data_ != null && other.data_ != null &&
                    data_.GetLength(0) == other.data_.GetLength(0) &&
                    data_.GetLength(1) == other.data_.GetLength(1))
                {
                    int noCols = data_.GetLength(0);
                    int noRows = data_.GetLength(1);
                    for (int i = 0; i < noCols; i++)
                    {
                        for (int j = 0; j < noRows; j++)
                        {
                            if (!Numeric.EQ(data_[i, j], other.data_[i, j], precision))
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
