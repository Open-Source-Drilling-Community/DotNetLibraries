using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class SquaredMatrix : ISquaredMatrix, IEquivalent<SquaredMatrix>
    {
        private double?[,] data_ = null;


        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public SquaredMatrix()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dim"></param>
        public SquaredMatrix(int dim)
        {
            data_ = new double?[dim, dim];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dim"></param>
        /// <param name="def"></param>
        public SquaredMatrix(int dim, double def)
        {
            data_ = new double?[dim, dim];
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                { data_[i, j] = def; }
            }
        }

        /// <summary>
        /// The elements are stored column by column : 
        /// |  0    dim+1   ...   ...   |
        /// |  1    dim+2   ...   ...   |
        /// |  2    dim+3   ...   ...   | 
        /// | ...   ...     ...   ...   |
        /// | dim   2*dim   ... dim*dim |
        /// </summary>
        /// <param name="dim"></param>
        /// <param name="values"></param>
        public SquaredMatrix(int dim, double[] values)
        {
            data_ = new double?[dim, dim];
            if (values.Length >= dim * dim)
            {
                int tmp = 0;
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
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
        public SquaredMatrix(ISquaredMatrix m)
        {
            if (m != null)
            {
                int dim = m.Dim;
                data_ = new double?[dim, dim];
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
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
        public SquaredMatrix(SquaredMatrix m)
        {
            if (m != null)
            {
                int dim = m.Dim;
                data_ = new double?[dim, dim];
                Array.Copy(m.data_, data_, dim * dim);
            }
        }

        #endregion

        #region Set

        /// <summary>
        /// 
        /// </summary>
        /// <param name="def"></param>
        public void Set(double def)
        {
            if (data_ != null)
            {
                for (int i = 0; i < data_.GetLength(0); i++)
                {
                    for (int j = 0; j < data_.GetLength(0); j++)
                    { data_[i, j] = def; }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        public void Set(ISquaredMatrix m)
        {
            if (data_ != null && m != null && data_.GetLength(0) == m.Dim)
            {
                for (int i = 0; i < m.Dim; i++)
                {
                    for (int j = 0; j < m.Dim; j++)
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
        public void Set(IMatrix m)
        {
            if (data_ != null && m != null && data_.GetLength(0) == m.ColumnCount && data_.GetLength(0) == m.RowCount)
            {
                for (int i = 0; i < m.RowCount; i++)
                {
                    for (int j = 0; j < m.RowCount; j++)
                    {
                        data_[i, j] = m[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        public void Set(double[] values)
        {
            if (data_ != null && values != null && values.Length >= data_.GetLength(0) * data_.GetLength(0))
            {
                int tmp = 0;
                for (int i = 0; i < data_.GetLength(0); i++)
                {
                    for (int j = 0; j < data_.GetLength(0); j++)
                    {
                        data_[i, j] = values[tmp++]; ;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetId()
        {
            if (data_ != null)
            {
                for (int i = 0; i < data_.GetLength(0); i++)
                {
                    for (int j = 0; j < data_.GetLength(0); j++)
                    {
                        data_[i, j] = 0;
                    }
                    data_[i, i] = 1;
                }
            }
        }

        #endregion

        #region ISquaredMatrix Members
        /// <summary>
        /// 
        /// </summary>
        public int Dim
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
        /// <param name="v"></param>
        public void GetDiagonal(ref IVector v)
        {
            if (data_ != null && v.Dim >= data_.GetLength(0))
            {
                for (int i = 0; i < data_.GetLength(0); i++)
                {
                    v[i] = data_[i, i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        public void SetDiagonal(IVector v)
        {
            if (data_ != null && v.Dim >= data_.GetLength(0))
            {
                for (int i = 0; i < data_.GetLength(0); i++)
                {
                    data_[i, i] = v[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startCol"></param>
        /// <param name="startRow"></param>
        /// <param name="m"></param>
        public void GetSubMatrix(int startCol, int startRow, ref ISquaredMatrix m)
        {
            if (m != null && data_ != null)
            {
                int ColSpan = System.Math.Min(data_.GetLength(0) - startCol, m.Dim);
                int RowSpan = System.Math.Min(data_.GetLength(1) - startRow, m.Dim);
                for (int i = 0; i < ColSpan; i++)
                {
                    for (int j = 0; j < RowSpan; j++)
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
        public void SetSubMatrix(int startCol, int startRow, ISquaredMatrix m)
        {
            if (m != null && data_ != null)
            {
                int RowSpan = System.Math.Min(data_.GetLength(1) - startRow, m.Dim);
                int ColSpan = System.Math.Min(data_.GetLength(0) - startCol, m.Dim);
                for (int i = 0; i < ColSpan; i++)
                {
                    for (int j = 0; j < RowSpan; j++)
                    {
                        data_[startCol + i, startRow + j] = m[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// Returns the determinant of the matrix
        /// Uses gaussian elimination with partial pivoting
        /// </summary>
        /// <returns></returns>
        public double Determinant()
        {
            if (data_ == null) { return Numeric.UNDEF_DOUBLE; }
            else
            {
                bool nbex = false;
                int dim = data_.GetLength(0);
                double det = 1;
                double[,] copie = new double[dim, dim];
                Array.Copy(data_, copie, dim * dim);
                for (int i = 0; i < dim - 1; i++)
                {
                    double max = copie[i, i];
                    int index_max = i;
                    //research of the pivot
                    for (int j = i; j < dim; j++)
                    {
                        if (System.Math.Abs(copie[i, j]) > System.Math.Abs(max))
                        {
                            max = copie[i, j];
                            index_max = j;
                        }
                    }
                    //in that case det=0
                    if (Numeric.EQ(max, 0))
                    {
                        return 0;
                    }

                    //Exchange of two rows
                    if (index_max != i)
                    {
                        nbex = !nbex;
                        for (int j = i; j < dim; j++)
                        {
                            max = copie[j, i];
                            copie[j, i] = copie[j, index_max];
                            copie[j, index_max] = max;
                        }
                    }

                    //Divisions of the rows i+1 -> dim
                    for (int j = i + 1; j < dim; j++)
                    {
                        double q = copie[i, j] / copie[i, i];
                        for (int k = i; k < dim; k++)
                        {
                            copie[k, j] = copie[k, j] - q * copie[k, i];
                        }
                    }
                }
                // Det is now the product of the elements on the diagonal times (-1)^nbexchanges
                for (int i = 0; i < dim; i++)
                {
                    det = copie[i, i] * det;
                }
                if (!nbex)
                {
                    return det;
                }
                else
                {
                    return -det;
                }
            }
        }


        /// <summary>
        /// Returns the trace of the matrix
        /// </summary>
        /// <returns></returns>
        public double Trace()
        {
            if (data_ != null)
            {
                int dim = data_.GetLength(0);
                double trace = 0;
                for (int i = 0; i < dim; i++)
                {
                    trace = trace + (double)data_[i, i];
                }
                return trace;
            }
            else return Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// Returns the rank of the matrix
        /// under the form of an integer. Uses gaussian elimination
        /// </summary>
        /// <returns></returns>
        public int Rank()
        {
            if (data_ == null) { return 0; }
            else
            {
                int dim = data_.GetLength(0);
                int rank = 0;
                double[,] copie = new double[dim, dim];
                Array.Copy(data_, copie, dim * dim);
                for (int i = 0; i < dim; i++)
                {
                    double max = copie[i, i];
                    int index_max = i;
                    //research of the pivot
                    for (int j = i + 1; j < dim; j++)
                    {
                        if (System.Math.Abs(copie[i, j]) > System.Math.Abs(max))
                        {
                            max = copie[i, j];
                            index_max = j;
                        }
                    }

                    //if the pivot is zero, no division is needed
                    if (!Numeric.EQ(max, 0))
                    {
                        //Exchange of two rows
                        if (index_max != i)
                        {

                            for (int j = i; j < dim; j++)
                            {
                                max = copie[j, i];
                                copie[j, i] = copie[j, index_max];
                                copie[j, index_max] = max;
                            }
                        }

                        for (int j = 0; j < dim; j++)
                        {
                            if (j != i)
                            {
                                double q = copie[i, j] / copie[i, i];
                                for (int k = 0; k < dim; k++)
                                {
                                    copie[k, j] = copie[k, j] - q * copie[k, i];
                                }
                            }
                        }
                    }
                }
                //counting of the non-zero lines
                for (int j = 0; j < dim; j++)
                {
                    bool zero = true;
                    for (int i = 0; i < dim; i++)
                    {
                        if (!Numeric.EQ(copie[i, j], 0))
                        {
                            zero = false;
                        }
                    }
                    if (zero == true) { rank++; }
                }
                return dim - rank;
            }
        }


        /// <summary>
        /// returns the Frobenius norm of the matrix : 
        /// || A ||_F = (sum a_(i,j)^2)^(1/2)
        /// </summary>
        /// <returns></returns>

        public double FrobeniusNorm()
        {
            if (data_ != null)
            {
                int dim = data_.GetLength(0);
                double fnorm = 0;
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        fnorm = fnorm + (double)data_[i, j] * (double)data_[i, j];
                    }
                }
                return System.Math.Abs(System.Math.Sqrt(fnorm));
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }
        /// <summary>
        /// Return the infinity norm : sup_j sum_i |a_(i,j)|
        /// </summary>
        /// <returns></returns>
        public double InfinityNorm()
        {
            if (data_ == null)
            {
                return Numeric.UNDEF_DOUBLE;
            }
            else
            {
                int dim = data_.GetLength(0);
                double inorm = 0;
                double temp;
                for (int j = 0; j < dim; j++)
                {
                    temp = 0;
                    for (int i = 0; i < dim; i++)
                    {
                        temp += System.Math.Abs((double)data_[i, j]);
                    }
                    if (temp > inorm)
                    {
                        inorm = temp;
                    }
                }
                return inorm;
            }
        }


        /// <summary>
        /// Returns the one norm sup_i sum_j |a_(i,j)|
        /// </summary>
        /// <returns></returns>
        public double OneNorm()
        {
            if (data_ == null)
            {
                return Numeric.UNDEF_DOUBLE;
            }
            else
            {
                int dim = data_.GetLength(0);
                double onenorm = 0;
                double temp;
                for (int i = 0; i < dim; i++)
                {
                    temp = 0;
                    for (int j = 0; j < dim; j++)
                    {
                        temp += System.Math.Abs((double)data_[i, j]);
                    }
                    if (temp > onenorm)
                    {
                        onenorm = temp;
                    }
                }
                return onenorm;
            }
        }


        /// <summary>
        /// To be done
        /// </summary>
        /// <returns></returns>
        public double TwoNorm()
        {
            throw new Exception("The method or operation is not implemented.");
        }


        /// <summary>
        /// LU decomposition of a row-permutation of the matrix
        /// The array book and boolean parity keep track of the permutations
        ///    book[i] = j -> rows i and j have been exchanged
        ///    parity = false -> even number of exchanges
        /// The matrix is returned under the form
        ///      |u u u u u...u |
        ///      |l u u u u...u |
        ///      |l l u u u...u |
        ///      |l l l u u...u |
        ///      |............. |
        ///      |l l l ... l u |
        /// 
        /// where the matrix L is lower unit triangular : 
        ///       |1 0 ..... 0 |
        ///       |l 1 0 ... 0 | 
        ///   L:= |l l 1 0 . 0 |
        ///       |..........  |
        ///       |l l ......1 |
        /// 
        /// and the matrix U is upper triangular
        ///       |u.........u |
        ///       |0 u ......u |
        ///   U:= |0 0 u ....u | 
        ///       |........... |
        ///       |0.......0 u |
        /// One has 
        /// 
        /// A = L.U
        /// 
        /// Uses Crout's algorithm with partial pivoting
        /// </summary>
        public int LUdec(ref bool parity, ref int[] book)
        {
            if (data_ != null)
            {
                parity = false;
                for (int i = 0; i < Dim; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < j; k++)
                        {
                            sum = sum + (double)data_[k, j] * (double)data_[i, k];
                        }
                        data_[i, j] = data_[i, j] - sum;
                    }
                    for (int j = i; j < Dim; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < i; k++)
                        {
                            sum = sum + (double)data_[k, j] * (double)data_[i, k];
                        }
                        data_[i, j] = data_[i, j] - sum;
                    }
                    //research of the pivot element
                    double pivot = 0;
                    int row_pivot = i;
                    for (int j = i; j < Dim; j++)
                    {
                        if (System.Math.Abs((double)data_[i, j]) > System.Math.Abs(pivot))
                        {
                            pivot = (double)data_[i, j];
                            row_pivot = j;
                        }
                    }

                    if (Numeric.EQ(pivot, 0)) { return 0; }

                    //exchange of two rows
                    if (row_pivot != i)
                    {
                        for (int k = 0; k < Dim; k++)
                        {
                            double temp = (double)data_[k, i];
                            data_[k, i] = data_[k, row_pivot];
                            data_[k, row_pivot] = temp;
                        }
                        parity = !parity;
                        book[i] = row_pivot;
                    }
                    else { book[i] = i; }
                    //final divisions
                    for (int j = i + 1; j < Dim; j++)
                    {
                        data_[i, j] = data_[i, j] / pivot;
                    }
                }
                return 1;
            }
            else { return 0; }
        }

        /// <summary>
        /// Returns the determinant of a matrix 
        /// modified by the LU decomposition above
        /// (parity is related to the number of row exchanges)
        /// </summary>
        /// <param name="parity"></param>
        /// <returns></returns>
        public double DetLU(bool parity)
        {
            double det = 1;
            for (int k = 0; k < Dim; k++)
            {
                det = det * (double)data_[k, k];
            }
            if (parity)
            {
                return -det;
            }
            else
            {
                return det;
            }
        }

        /// <summary>
        /// Returns the determinant of the matrix, using LU decomposition
        /// </summary>
        /// <returns></returns>
        public double Det2()
        {
            if (data_ != null)
            {
                bool parity = false;
                int[] book = new int[data_.GetLength(0)];
                LUdec(ref parity, ref book);
                return DetLU(parity);
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// Solves the equation this.x=b
        /// where this and book are modified  by the LU decomposition above
        /// Stores the solution into b
        /// </summary>
        /// <param name="book"></param>
        /// <param name="b"></param>
        public void SubLu(int[] book, ref IVector b)
        {
            double sum;
            int index, v = -1;
            // fwb sub solves Ly=b
            for (int j = 0; j < Dim; j++)
            {
                index = book[j];
                sum = (double)b[index];
                b[index] = b[j];
                if (v >= 0)
                {
                    for (int i = v; i < j; i++)
                    {
                        sum = sum - (double)data_[i, j] * (double)b[i];
                    }
                }
                else
                {
                    if (!Numeric.EQ(sum, 0)) { v = j; }
                }
                b[j] = sum;
            }
            // backsub solves Ux=y
            for (int j = Dim - 1; j >= 0; j--)
            {
                sum = (double)b[j];
                for (int i = j + 1; i < Dim; i++)
                {
                    sum = sum - (double)data_[i, j] * (double)b[i];
                }
                b[j] = sum / data_[j, j];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public void Solve(ref IVector b)
        {
            if (data_ != null)
            {
                bool parity = false;
                int[] book = new int[data_.GetLength(0)];
                LUdec(ref parity, ref book);
                SubLu(book, ref b);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public void Solve_MinRowPreconditioning(ref IVector b)
        {
            for (int i = 0; i < Dim; i++)
            {
                IVector row = new Vector(Dim);
                GetRow(i, ref row);
                //find the minimum (in absolute value) non zero element in the row for preconditioning
                double min = 0;
                for (int j = 1; j < Dim; j++)
                {
                    if (!Numeric.EQ(row[j], 0) && !Numeric.EQ(min, 0))
                    {
                        if (Numeric.LT(System.Math.Abs((double)row[j]), System.Math.Abs(min)))
                        {
                            min = (double)row[j];
                        }
                    }
                    else if (!Numeric.EQ(row[j], 0) && Numeric.EQ(min, 0))
                    {
                        min = (double)row[j];
                    }
                }
                if (!Numeric.EQ(0, min))
                {
                    for (int j = 0; j < Dim; j++)
                    {
                        row[j] = row[j] / min;
                    }
                    SetRow(i, row);
                    b[i] = b[i] / min;
                }
            }
            //Solve
            Solve(ref b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public void Solve_MaxRowPreconditioning(ref IVector b)
        {
            for (int i = 0; i < Dim; i++)
            {
                IVector row = new Vector(Dim);
                GetRow(i, ref row);
                //find the maximum (in absolute value) non zero element in the row for preconditioning
                double min = 0;
                for (int j = 1; j < Dim; j++)
                {
                    if (!Numeric.EQ(row[j], 0) && !Numeric.EQ(min, 0))
                    {
                        if (Numeric.GT(System.Math.Abs((double)row[j]), System.Math.Abs(min)))
                        {
                            min = (double)row[j];
                        }
                    }
                    else if (!Numeric.EQ(row[j], 0) && Numeric.EQ(min, 0))
                    {
                        min = (double)row[j];
                    }
                }
                if (!Numeric.EQ(0, min))
                {
                    for (int j = 0; j < Dim; j++)
                    {
                        row[j] = row[j] / min;
                    }
                    SetRow(i, row);
                    b[i] = b[i] / min;
                }
            }
            //Solve
            Solve(ref b);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public void Solve_DiagonalPreconditioning(ref IVector b)
        {
            //preconditioning step. Multiply each row and the corresponding element in b by the inverse of the diagonal element in the matrix
            for (int i = 0; i < Dim; i++)
            {
                double diag = (double)data_[i, i];
                if (!Numeric.EQ(0, diag))
                {
                    IVector row = new Vector(Dim);
                    GetRow(i, ref row);
                    for (int j = 0; j < Dim; j++)
                    {
                        row[j] = row[j] / diag;
                    }
                    SetRow(i, row);
                    b[i] = b[i] / diag;
                }
            }
            //Solve
            Solve(ref b);
        }

        /// <summary>
        /// Turns this into its inverse IF this/book are returned/modified
        /// by the LU dec above
        /// </summary>
        /// <param name="book"></param>
        public void InvertLU(int[] book)
        {
            double[,] copie = new double[Dim, Dim];
            IVector col = new Vector(Dim);
            for (int i = 0; i < Dim; i++)
            {
                for (int j = 0; j < Dim; j++) { col[j] = 0; }
                col[i] = 1;
                SubLu(book, ref col);
                for (int j = 0; j < Dim; j++)
                { copie[i, j] = (double)col[j]; }
            }
            for (int i = 0; i < Dim; i++)
            {
                for (int j = 0; j < Dim; j++)
                { data_[i, j] = copie[i, j]; }
            }
        }

        /// <summary>
        /// Sets this to its inverse
        /// Uses LU decomposition
        /// </summary>
        public void Invert2()
        {
            if (data_ != null)
            {
                bool parity = false;
                int[] book = new int[data_.GetLength(0)];
                LUdec(ref parity, ref book);
                InvertLU(book);
            }
        }


        #endregion

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
                    return data_.GetLength(0);
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
            if (data_ != null && v != null && v.Dim >= data_.GetLength(0))
            {
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
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
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
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
            if (data_ != null && v != null && v.Dim >= data_.GetLength(0))
            {
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
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
                int ColSpan = System.Math.Min(data_.GetLength(0) - startCol, m.ColumnCount);
                int RowSpan = System.Math.Min(data_.GetLength(1) - startCol, m.RowCount);
                for (int i = 0; i < ColSpan; i++)
                {
                    for (int j = 0; j < RowSpan; j++)
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
        /// Transpose the matrix this into result
        /// </summary>
        /// <param name="result"></param>

        public void Transpose(ref IMatrix result)
        {
            if (result != null && data_ != null)
            {
                int dim = data_.GetLength(0);
                if (result.ColumnCount >= dim && result.RowCount >= dim)
                {
                    for (int i = 0; i < dim; i++)
                    {
                        for (int j = 0; j < dim; j++)
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
            if (a == null || a.RowCount != Dim || a.ColumnCount != Dim)
            {
                return null;
            }
            else
            {
                SquaredMatrix b = new SquaredMatrix(Dim);
                if (data_ != null)
                {
                    int dim = data_.GetLength(0);
                    for (int i = 0; i < dim; i++)
                    {
                        for (int j = 0; j < dim; j++)
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
            SquaredMatrix b = new SquaredMatrix(Dim);
            if (data_ != null)
            {
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
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
            if (a == null || a.RowCount != Dim || a.ColumnCount != Dim)
            {
                return null;
            }
            else
            {
                SquaredMatrix b = new SquaredMatrix(Dim);
                if (data_ != null)
                {
                    int dim = data_.GetLength(0);
                    for (int i = 0; i < dim; i++)
                    {
                        for (int j = 0; j < dim; j++)
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
            if (a != null && b != null && a.ColumnCount == Dim && a.RowCount == Dim && b.ColumnCount == Dim && b.RowCount == Dim && data_ != null)
            {
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
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
            if (a != null && a.ColumnCount == Dim && a.RowCount == Dim && data_ != null)
            {
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
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
            if (a != null && b != null && a.ColumnCount == Dim && a.RowCount == Dim && b.ColumnCount == Dim && b.RowCount == Dim && data_ != null)
            {
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        b[i, j] = data_[i, j] - a[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// Set this to (this + a)
        /// </summary>
        /// <param name="a"></param>
        public void AddAssign(IMatrix a)
        {
            if (data_ != null && a != null && a.RowCount == data_.GetLength(0) && a.ColumnCount == data_.GetLength(0))
            {
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    { data_[i, j] = data_[i, j] + a[i, j]; }
                }
            }
        }

        /// <summary>
        /// Set this to -this
        /// </summary>
        public void NegateAssign()
        {
            if (data_ != null)
            {
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    { data_[i, j] = -data_[i, j]; }
                }
            }
        }

        /// <summary>
        /// Set this to this - a
        /// </summary>
        /// <param name="a"></param>
        public void SubstractAssign(IMatrix a)
        {
            if (data_ != null && a != null && a.ColumnCount == data_.GetLength(0) && a.RowCount == data_.GetLength(0))
            {
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    { data_[i, j] = data_[i, j] - a[i, j]; }
                }
            }
        }

        #endregion

        #region IProductable<IVector> Members


        /// <summary>
        /// Set Vector b to this*a
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Time(IVector a, ref IVector b)
        {
            if (a != null && b != null && data_ != null && a.Dim == data_.GetLength(0) && b.Dim == data_.GetLength(0))
            {
                int dim = data_.GetLength(0);
                for (int j = 0; j < dim; j++)
                {
                    double value = 0;
                    for (int i = 0; i < dim; i++)
                    {
                        value = value + (double)data_[i, j] * (double)a[i];
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
            if (a != null && a.Dim == Dim)
            {
                int dim = Dim;
                Vector b = new Vector(dim);
                for (int j = 0; j < dim; j++)
                {
                    double value = 0;
                    for (int i = 0; i < dim; i++)
                    {
                        value = value + (double)data_[i, j] * (double)a[i];
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
        /// Set b  to this*a
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Time(IMatrix a, ref IMatrix b)
        {
            if (data_ != null && a != null && b != null && data_.GetLength(0) == a.RowCount && b.RowCount == data_.GetLength(1) && b.ColumnCount == a.ColumnCount)
            {
                int noCols = a.ColumnCount;
                int count = data_.GetLength(1);
                for (int j = 0; j < count; j++)
                {
                    for (int i = 0; i < noCols; i++)
                    {
                        double value = 0;
                        for (int k = 0; k < noCols; k++)
                        {
                            value = value + (double)data_[k, j] * (double)a[i, k];
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
        public IMatrix Time(IMatrix a)
        {
            if (a != null && Dim == a.RowCount)
            {
                int noCols = a.ColumnCount;
                int count = Dim;
                Matrix b = new Matrix(a.ColumnCount, Dim);
                for (int j = 0; j < count; j++)
                {
                    for (int i = 0; i < noCols; i++)
                    {
                        double value = 0;
                        for (int k = 0; k < noCols; k++)
                        {
                            value = value + (double)data_[k, j] * (double)a[i, k];
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
        /// set this to this*x for scalar x
        /// </summary>
        /// <param name="x"></param>
        public void Time(double x)
        {
            if (data_ != null)
            {
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        data_[i, j] = x * data_[i, j];
                    }
                }
            }
        }

        #endregion

        #region IZero Members

        /// <summary>
        /// set every element  to 0
        /// </summary>

        public void SetZero()
        {
            if (data_ != null)
            {
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
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
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
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
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
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
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
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
        /// Copies this into item
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref IMatrix item)
        {
            if (item != null && data_ != null && data_.GetLength(0) == item.ColumnCount && data_.GetLength(0) == item.RowCount)
            {
                int dim = data_.GetLength(0);
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        item[i, j] = data_[i, j];
                    }
                }
            }
        }

        #endregion

        #region IEquivalent<IMatrix> Members
        /// <summary>
        /// Tests if other==this
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
                else if (data_ != null && data_.GetLength(0) == other.ColumnCount && data_.GetLength(0) == other.RowCount)
                {
                    int dim = data_.GetLength(0);
                    for (int i = 0; i < dim; i++)
                    {
                        for (int j = 0; j < dim; j++)
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
        /// Tests if other==this
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
                else if (data_ != null && data_.GetLength(0) == other.ColumnCount && data_.GetLength(0) == other.RowCount)
                {
                    int dim = data_.GetLength(0);
                    for (int i = 0; i < dim; i++)
                    {
                        for (int j = 0; j < dim; j++)
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

        #region IProductable<ISquaredMatrix> Members
        /// <summary>
        /// Sets b to this*a
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Time(ISquaredMatrix a, ref ISquaredMatrix b)
        {
            if (a != null && b != null && Dim == a.Dim && Dim == b.Dim)
            {
                int dim = Dim;
                for (int j = 0; j < dim; j++)
                {
                    for (int i = 0; i < dim; i++)
                    {
                        double value = 0;
                        for (int k = 0; k < dim; k++)
                        {
                            value = value + (double)data_[k, j] * (double)a[i, k];
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
        public ISquaredMatrix Time(ISquaredMatrix a)
        {
            if (a != null && a.Dim == Dim)
            {
                int dim = Dim;
                SquaredMatrix b = new SquaredMatrix(dim);
                for (int j = 0; j < dim; j++)
                {
                    for (int i = 0; i < dim; i++)
                    {
                        double value = 0;
                        for (int k = 0; k < dim; k++)
                        {
                            value = value + (double)data_[k, j] * (double)a[i, k];
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void TimeAssign(ISquaredMatrix a)
        {
            if (a != null && a.Dim == Dim)
            {
                int dim = Dim;
                Vector b = new Vector(dim);
                for (int j = 0; j < dim; j++)
                {
                    for (int i = 0; i < dim; i++)
                    {
                        double value = 0;
                        for (int k = 0; k < dim; k++)
                        {
                            value = value + (double)data_[k, j] * (double)a[i, k];
                        }
                        b[j] = value;
                    }
                    for (int k = 0; k < dim; k++)
                    {
                        data_[k, j] = b[j];
                    }
                }
            }
        }

        /// <summary>
        /// Sets this to its inverse
        /// Uses GaussJordan elimination with partial pivoting
        /// in place (i.e. do not need extra memory space) 
        /// 
        /// </summary>
        public void InvertAssign()
        {
            if (data_ != null)
            {
                int dim = data_.GetLength(0);
                int[] exchanges = new int[dim];
                //begining of the algorithm
                for (int i = 0; i < dim; i++)
                {

                    //research of the pivot element
                    double max = (double)data_[i, i];
                    int index_max = i;
                    for (int j = i + 1; j < dim; j++)
                    {
                        if (System.Math.Abs((double)data_[i, j]) > System.Math.Abs(max))
                        {
                            max = (double)data_[i, j];
                            index_max = j;
                        }
                    }
                    exchanges[i] = index_max;
                    if (Numeric.EQ(max, 0)) { SetUndefined(); }

                    //Exchange (if necessary) of two lines
                    for (int j = 0; j < dim; j++)
                    {
                        max = (double)data_[j, i];
                        data_[j, i] = data_[j, index_max];
                        data_[j, index_max] = max;
                    }

                    //Normalisation of the current line
                    max = (double)data_[i, i];
                    data_[i, i] = 1;
                    for (int k = 0; k < dim; k++)
                    {
                        data_[k, i] = data_[k, i] / max;
                    }
                    //Linear combination for the dim-1 other lines
                    for (int j = 0; j < dim; j++)
                    {
                        if (j != i)
                        {
                            double temp = (double)data_[i, j];
                            data_[i, j] = 0;
                            for (int k = 0; k < dim; k++)
                            {
                                data_[k, j] = data_[k, j] - temp * data_[k, i];
                            }
                        }
                    }
                }

                //final permutations of columns
                for (int i = dim - 1; i >= 0; i--)
                {
                    if (exchanges[i] != i)
                    {
                        for (int j = 0; j < dim; j++)
                        {
                            double max = (double)data_[i, j];
                            data_[i, j] = data_[exchanges[i], j];
                            data_[exchanges[i], j] = max;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets this to its inverse
        /// Uses GaussJordan elimination with partial pivoting
        /// Can be improved (uses a 2*n,n temporary matrix)
        /// </summary>
        public void Invert_temp()
        {
            if (data_ != null)
            {
                int dim = data_.GetLength(0);
                // init. of the  [2n,n] matrix copie
                double[,] copie = new double[2 * dim, dim];
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    { copie[i, j] = (double)data_[i, j]; }
                }
                for (int i = dim; i < 2 * dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        if (i == j + dim)
                        {
                            copie[i, j] = 1;
                        }
                        else
                        {
                            copie[i, j] = 0;
                        }
                    }
                }
                // end of init.


                //begining of the algorithm
                for (int i = 0; i < dim; i++)
                {

                    //research of the pivot element
                    double max = copie[i, i];
                    int index_max = i;
                    for (int j = i + 1; j < dim; j++)
                    {
                        if (System.Math.Abs(copie[i, j]) > System.Math.Abs(max))
                        {
                            max = copie[i, j];
                            index_max = j;
                        }
                    }

                    if (Numeric.EQ(max, 0)) { SetUndefined(); }

                    //Exchange (if necessary) of two lines
                    for (int j = i; j < 2 * dim; j++)
                    {
                        max = copie[j, i];
                        copie[j, i] = copie[j, index_max];
                        copie[j, index_max] = max;
                    }

                    //Normalisation of the current line. copie[i,i]=1
                    max = copie[i, i];
                    for (int k = 0; k < dim * 2; k++)
                    {
                        copie[k, i] = copie[k, i] / max;
                    }

                    //Linear combination for the dim-1 other lines
                    // so that copie[j,i]=0
                    for (int j = 0; j < dim; j++)
                    {
                        if (j != i)
                        {
                            double q = copie[i, j] / copie[i, i];
                            for (int k = 0; k < 2 * dim; k++)
                            {
                                copie[k, j] = copie[k, j] - q * copie[k, i];
                            }
                        }
                    }
                }

                //The algorithm is finished
                //The right part of copie is copyed into this
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        data_[i, j] = copie[dim + i, j];
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ISquaredMatrix Invert()
        {
            SquaredMatrix a = new SquaredMatrix(Dim);
            if (Dim != 0)
            {
                Array.Copy(data_, a.data_, Dim * Dim);
                a.InvertAssign();
            }
            return a;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void Invert(ref ISquaredMatrix a)
        {
            if (Dim != 0 && a != null && a.Dim == Dim)
            {
                int dim = Dim;
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        a[i, j] = data_[i, j];
                    }
                }
                a.InvertAssign();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public ISquaredMatrix Divide(ISquaredMatrix a)
        {
            if (a != null && a.Dim == Dim)
            {
                return Time(a.Invert());
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Divide(ISquaredMatrix a, ref ISquaredMatrix b)
        {
            if (a != null && b != null && a.Dim == Dim && b.Dim == Dim)
            {
                Time(a.Invert(), ref b);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void DivideAssign(ISquaredMatrix a)
        {
            if (a != null && a.Dim == Dim)
            {
                TimeAssign(a.Invert());
            }
        }



        #endregion

        #region ICopyable<SquaredMatrix<T,Calculator>> Members


        /// <summary>
        /// Copies this into item
        /// </summary>
        /// <param name="item"></param>

        public void Copy(ref SquaredMatrix item)
        {
            if (item != null && data_ != null && item.data_.GetLength(0) == data_.GetLength(0))
            {
                Array.Copy(data_, item.data_, data_.Length);
            }
        }

        #endregion

        #region IEquivalent<SquaredMatrix<T,Calculator>> Members
        /// <summary>
        /// Tests if this equals other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(SquaredMatrix other)
        {
            if (other != null)
            {
                if (data_ == null && other.data_ == null)
                {
                    return true;
                }
                else if (data_ != null && other.data_ != null &&
                    data_.GetLength(0) == other.data_.GetLength(0))
                {
                    int dim = data_.GetLength(0);
                    for (int i = 0; i < dim; i++)
                    {
                        for (int j = 0; j < dim; j++)
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
        /// Tests if this equals other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(SquaredMatrix other, double precision)
        {
            if (other != null)
            {
                if (data_ == null && other.data_ == null)
                {
                    return true;
                }
                else if (data_ != null && other.data_ != null &&
                    data_.GetLength(0) == other.data_.GetLength(0))
                {
                    int dim = data_.GetLength(0);
                    for (int i = 0; i < dim; i++)
                    {
                        for (int j = 0; j < dim; j++)
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
