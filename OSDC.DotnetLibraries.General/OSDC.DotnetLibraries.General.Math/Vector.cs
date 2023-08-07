using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Vector : IVector, ICloneable, IUnity, IDotProductable<Vector>, IUndefinable, IZeroeable, IScalarProduct<Vector>
    {
        private double?[] values_;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dim"></param>
        public Vector(int dim)
        {
            if (dim > 0)
            {
                values_ = new double?[dim];
                SetUndefined();
            }
        }

        public Vector(int dim, double? def)
        {
            if (dim > 0)
            {
                values_ = new double?[dim];
                for (int i = 0; i < dim; i++)
                {
                    values_[i] = def;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        public Vector(double[] values)
        {
            if (values != null)
            {
                values_ = new double?[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    values_[i] = values[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        public Vector(IVector v)
        {
            if (v != null && v.Dim > 0)
            {
                values_ = new double?[v.Dim];
                for (int i = 0; i < v.Dim; i++)
                {
                    values_[i] = v[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Vector(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        public void Copy(ref IVector v)
        {
            if (v != null && v.Dim == Dim && Dim != 0)
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    v[i] = values_[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        public void CopyFrom(double[] values)
        {
            if (values != null && values.Length == Dim && Dim != 0)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values_[i] = values[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        public void CopyTo(double?[] values)
        {
            if (values != null && values.Length == Dim && Dim != 0)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = values_[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        public void Set(IVector v)
        {
            if (v != null && v.Dim == Dim && Dim != 0)
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    values_[i] = v[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool EQ(Vector v)
        {
            return EQ(v, Numeric.DOUBLE_ACCURACY);
        }

        public bool EQ(Vector v, double precision)
        {
            if (v == null || v.Dim != Dim)
            {
                return false;
            }
            else
            {
                if (Dim == 0)
                {
                    return true;
                }
                else
                {
                    for (int i = 0; i < values_.Length; i++)
                    {
                        if (!Numeric.EQ(values_[i], v[i], precision))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double? GetLength()
        {
            double? length = 0;
            if (values_ != null)
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    length = length + values_[i] * values_[i];
                }
            }
            return Numeric.SqrtEqual(length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double? GetLength2()
        {
            double? length = 0;
            if (values_ != null)
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    length = length + values_[i] * values_[i];
                }
            }
            return length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void SetLength(double a)
        {
            if (values_ != null)
            {
                double? length = GetLength();
                if (!Numeric.EQ(length, 0))
                {
                    double? factor = a / length;
                    for (int i = 0; i < values_.Length; i++)
                    {
                        values_[i] = values_[i] * factor;
                    }
                }
            }
        }

        #region IUnity Member
        /// <summary>
        /// 
        /// </summary>
        public void SetUnity()
        {
            if (values_ != null)
            {
                double? length = GetLength();
                if (!Numeric.EQ(length, 0))
                {
                    for (int i = 0; i < values_.Length; i++)
                    {
                        values_[i] = values_[i] / length;
                    }
                }
            }
        }
        public bool IsUnity()
        {
            return IsUnity(Numeric.DOUBLE_ACCURACY);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUnity(double precision)
        {
            return Numeric.EQ(GetLength2(), 1.0, precision);
        }

        #endregion

        #region IDotProduct<IVector<T>> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double? Dot(Vector v)
        {
            double? value = 0;
            if (v != null && v.Dim == Dim && Dim != 0)
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    value = value + values_[i] * v[i];
                }
            }
            return value;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public int Dim
        {
            get
            {
                if (values_ == null)
                {
                    return 0;
                }
                else
                {
                    return values_.Length;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double? this[int index]
        {
            get
            {
                if (values_ == null)
                {
                    return Numeric.UNDEF_DOUBLE;
                }
                else
                {
                    return values_[index];
                }
            }
            set
            {
                if (values_ != null)
                {
                    values_[index] = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="v"></param>
        public void CopyTo(int start, IVector v)
        {
            if (v != null && values_ != null)
            {
                int length = System.Math.Min(v.Dim, Dim - start);
                for (int i = 0; i < length; i++)
                {
                    v[i] = values_[start + i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="v"></param>
        public void CopyFrom(int start, IVector v)
        {
            if (v != null && values_ != null)
            {
                int length = System.Math.Min(v.Dim, Dim - start);
                for (int i = 0; i < length; i++)
                {
                    values_[start + i] = v[i];
                }
            }
        }

        #region IUndefinable Members
        /// <summary>
        /// 
        /// </summary>
        public void SetUndefined()
        {
            if (values_ != null)
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    values_[i] = Numeric.UNDEF_DOUBLE;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUndefined()
        {
            if (values_ == null)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    if (Numeric.IsUndefined(values_[i]))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion

         /// <summary>
        /// 
        /// </summary>
        public void SetZero()
        {
            if (values_ != null)
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    values_[i] = 0;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            if (values_ == null)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    if (!Numeric.EQ(values_[i], 0))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector Add(Vector v)
        {
            if (v != null && v.Dim == Dim && Dim != 0)
            {
                Vector r = new Vector(Dim);
                for (int i = 0; i < values_.Length; i++)
                {
                    r[i] = values_[i] + v[i];
                }
                return r;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        public void AddAssign(Vector v)
        {
            if (Dim == v.Dim && Dim != 0)
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    values_[i] = values_[i] + v[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public Vector Negate()
        {
            if (values_ != null)
            {
                Vector v = new Vector(Dim);
                for (int i = 0; i < values_.Length; i++)
                {
                    v[i] = -values_[i];
                }
                return v;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void NegateAssign()
        {
            if (values_ != null)
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    values_[i] = -values_[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector Substract(Vector v)
        {
            if (v != null && v.Dim == Dim && Dim != 0)
            {
                Vector r = new Vector(Dim);
                for (int i = 0; i < values_.Length; i++)
                {
                    r[i] = values_[i] - v[i];
                }
                return r;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        public void SubstractAssign(Vector v)
        {
            if (Dim == v.Dim && Dim != 0)
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    values_[i] = values_[i] - v[i];
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool IsParallel(IVector v)
        {
            if (v == null || v.Dim != Dim)
            {
                return false;
            }
            else
            {
                if (Dim == 0)
                {
                    return true;
                }
                else
                {
                    double? l1 = GetLength2();
                    double? l2 = v.GetLength2();
                    if (Numeric.EQ(l1, 0) || Numeric.EQ(l2, 0))
                    {
                        return Numeric.EQ(l1, l2);
                    }
                    else if (Numeric.EQ(l1, l2))
                    {
                        for (int i = 0; i < values_.Length; i++)
                        {
                            if (!Numeric.EQ(values_[i], v[i]))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    else
                    {
                        if (!Numeric.EQ(l1, 0))
                        {
                            double? ratio = Numeric.SqrtEqual(l2) / Numeric.SqrtEqual(l1);
                            for (int i = 0; i < values_.Length; i++)
                            {
                                if (!Numeric.EQ(values_[i] * ratio, v[i]))
                                {
                                    return false;
                                }
                            }
                            return true;
                        }
                        else
                        {
                            return Numeric.EQ(l2, 0);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// return a vector that corresponds the scalar product of this vector by a scalar value
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public Vector Time(double x)
        {
            if (values_ != null)
            {
                Vector v = new Vector(Dim);
                for (int i = 0; i < values_.Length; i++)
                {
                    v[i] = values_[i] * x;
                }
                return v;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Assign to this vector the scalar product of its components by a scalar value
        /// </summary>
        /// <param name="x"></param>
        public void TimeAssign(double x)
        {
            if (values_ != null)
            {
                for (int i = 0; i < values_.Length; i++)
                {
                    values_[i] = values_[i] * x;
                }
            }
        }
    }
}
