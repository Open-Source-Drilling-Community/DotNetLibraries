using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public struct LinearPolynom : IPolynom, IEquivalent<IPolynom>, IEquivalent<LinearPolynom>, IUndefinable, ICloneable, ICopyable<IPolynom>, ICopyable<LinearPolynom>, IZero
    {
        /// <summary>
        /// Ax+B
        /// </summary>
        public double A { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double B { get; set; }
        /// <summary>
        /// Ax+B
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public LinearPolynom(double a, double b)
        {
            A = a;
            B = b;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public LinearPolynom(double[] a)
        {
            if (a != null && a.Length >= 2)
            {
                A = a[1];
                B = a[0];
            }
            else
            {
                A = Numeric.UNDEF_DOUBLE;
                B = Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public LinearPolynom(IPolynom p)
        {
            if (p != null && p.Degree >= 1)
            {
                A = p[1];
                B = p[0];
            }
            else
            {
                A = Numeric.UNDEF_DOUBLE;
                B = Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public LinearPolynom(LinearPolynom p)
        {
            A = p.A;
            B = p.B;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return A.ToString() + "*x+" + B.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Set(LinearPolynom p)
        {
            A = p.A;
            B = p.B;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Set(double a, double b)
        {
            A = a;
            B = b;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void Set(double[] a)
        {
            if (a != null && a.Length >= 2)
            {
                A = a[1];
                B = a[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Set(IPolynom p)
        {
            if (p != null && p.Degree >= 2)
            {
                A = p[1];
                B = p[0];
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
                root = Numeric.UNDEF_DOUBLE;
                if (Numeric.EQ(B, 0))
                {
                    //infinite number of solutions
                    return -1;
                }
                else
                {
                    //no solutions
                    return 0;
                }
            }
            else
            {
                root = -B / A;
                return 1;
            }
        }

        #region IPolynom Members
        /// <summary>
        /// 
        /// </summary>
        public int Degree
        {
            get { return 1; }
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
                    return B;
                }
                else
                {
                    return A;
                }
            }
            set
            {
                if (index == 0)
                {
                    B = value;
                }
                else
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
            return A * x + B;
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
            else if (result == -1)
            {
                return min;
            }
            else
            {
                if (Numeric.GE(root, min) && Numeric.LE(root, max))
                {
                    return root;
                }
                else
                {
                    return Numeric.UNDEF_DOUBLE;
                }
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
            B = Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUndefined()
        {
            return Numeric.IsUndefined(A) || Numeric.IsUndefined(B);
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new LinearPolynom(this);
        }

        #endregion

        #region ICopyable<IPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref IPolynom item)
        {
            if (item != null && item.Degree == 1)
            {
                item[0] = B;
                item[1] = A;
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
            if (other != null && other.Degree == 1)
            {
                return Numeric.EQ(A, other[1]) && Numeric.EQ(B, other[0]);
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
            if (other != null && other.Degree == 1)
            {
                return Numeric.EQ(A, other[1], precision) && Numeric.EQ(B, other[0], precision);
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region ICopyable<LinearPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref LinearPolynom item)
        {
            item.A = A;
            item.B = B;
        }

        #endregion

        #region IEquivalent<LinearPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(LinearPolynom other)
        {
            return Numeric.EQ(A, other.A) && Numeric.EQ(B, other.B);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(LinearPolynom other, double precision)
        {
            return Numeric.EQ(A, other.A, precision) && Numeric.EQ(B, other.B, precision);
        }
        #endregion

        #region IZero Members
        /// <summary>
        /// 
        /// </summary>
        public void SetZero()
        {
            A = 0;
            B = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return Numeric.EQ(A, 0) && Numeric.EQ(B, 0);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double Derive(double x)
        {
            return A;
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
            return 0.5 * A * b * b + B * b - (0.5 * A * a * a + B * a);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Derivate(ref IPolynom p)
        {
            if (p != null && p.Degree >= 0)
            {
                p[0] = A;
                for (int i = 1; i < p.Degree; i++)
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
            if (p != null && p.Degree >= 2)
            {
                p[0] = 0;
                p[1] = B;
                p[2] = A / 2.0;
                for (int i = 3; i < p.Degree; i++)
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
            p = A;
        }

    }
}
