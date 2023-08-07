using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public struct ConstantPolynom : IPolynom, IEquivalent<IPolynom>, IEquivalent<ConstantPolynom>, IUndefinable, ICloneable, ICopyable<IPolynom>, ICopyable<ConstantPolynom>, IZero
    {
        /// <summary>
        /// A
        /// </summary>
        public double A { get; set; }

        /// <summary>
        /// A
        /// </summary>
        /// <param name="a"></param>
        public ConstantPolynom(double a)
        {
            A = a;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public ConstantPolynom(double[] a)
        {
            if (a != null && a.Length >= 1)
            {
                A = a[0];
            }
            else
            {
                A = Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public ConstantPolynom(IPolynom p)
        {
            if (p != null && p.Degree >= 0)
            {
                A = p[0];
            }
            else
            {
                A = Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public ConstantPolynom(ConstantPolynom p)
        {
            A = p.A;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return A.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Set(ConstantPolynom p)
        {
            A = p.A;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Set(double a)
        {
            A = a;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void Set(double[] a)
        {
            if (a != null && a.Length >= 1)
            {
                A = a[1];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Set(IPolynom p)
        {
            if (p != null && p.Degree >= 1)
            {
                A = p[1];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public int FindRoots(ref double root)
        {
            if (Numeric.EQ(A, 0))
            {
                root = 0;
                return 1;
            }
            else
            {
                root = Numeric.UNDEF_DOUBLE;
                return 0;
            }
        }

        #region IPolynom Members
        /// <summary>
        /// 
        /// </summary>
        public int Degree
        {
            get { return 0; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return A;
                }
                else
                {
                    return Numeric.UNDEF_DOUBLE;
                }
            }
            set
            {
                if (index == 0)
                {
                    A = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double Eval(double x)
        {
            return A;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double FindRoot(double min, double max)
        {
            double root = Numeric.UNDEF_DOUBLE;
            int result = FindRoots(ref root);
            if (result == 0)
            {
                return Numeric.UNDEF_DOUBLE;
            }
            else 
            {
                return min;
            }
        }

        #endregion

        #region IUndefinable Members
        /// <summary>
        /// 
        /// </summary>
        public void SetUndefined()
        {
            A = Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUndefined()
        {
            return Numeric.IsUndefined(A);
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new ConstantPolynom(this);
        }

        #endregion

        #region ICopyable<IPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref IPolynom item)
        {
            if (item != null && item.Degree == 0)
            {
                item[0] = A;
            }
        }

        #endregion

        #region IEquivalent<IPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(IPolynom other)
        {
            if (other != null && other.Degree == 0)
            {
                return Numeric.EQ(A, other[0]);
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
        public bool EQ(IPolynom other, double precision)
        {
            if (other != null && other.Degree == 0)
            {
                return Numeric.EQ(A, other[0], precision);
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region ICopyable<ConstantPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref ConstantPolynom item)
        {
            item.A = A;
        }

        #endregion

        #region IEquivalent<ConstantPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(ConstantPolynom other)
        {
            return Numeric.EQ(A, other.A);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(ConstantPolynom other, double precision)
        {
            return Numeric.EQ(A, other.A, precision);
        }
        #endregion

        #region IZero Members
        /// <summary>
        /// 
        /// </summary>
        public void SetZero()
        {
            A = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return Numeric.EQ(A, 0);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double Derive(double x)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double DeriveSecond(double x)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public double Integrate(double a, double b)
        {
            return A * (b - a);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Derivate(ref IPolynom p)
        {
            if (p != null && p.Degree >= 0)
            {
                for (int i = 0; i < p.Degree; i++)
                {
                    p[i] = 0;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Primitive(ref IPolynom p)
        {
            if (p != null && p.Degree >= 1)
            {
                p[0] = 0;
                p[1] = A;
                for (int i = 2; i < p.Degree; i++)
                {
                    p[i] = 0;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Derivate(ref double p)
        {
            p = 0;
        }

    }
}
