using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    ///  Polynom of the form
    /// 
    ///  A x^2 + B x + C
    /// </summary>
    public struct QuadraticPolynom : IPolynom, IEquivalent<IPolynom>, IEquivalent<QuadraticPolynom>, IUndefinable, ICloneable, ICopyable<IPolynom>, ICopyable<QuadraticPolynom>, IZero
    {
        /// <summary>
        /// Ax^2+Bx+C
        /// </summary>
        public double A { get; set; } = 0;
        /// <summary>
        /// Ax^2+Bx+C
        /// </summary>
        public double B { get; set; } = 0;
        /// <summary>
        /// Ax^2+Bx+C
        /// </summary>
        public double C { get; set; } = 0;

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public QuadraticPolynom(double a, double b, double c)
        {
            A = a;
            B = b;
            C = c;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="a"></param>
        public QuadraticPolynom(double[] a)
        {
            if (a != null && a.Length >= 3)
            {
                A = a[2];
                B = a[1];
                C = a[0];
            }
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="p"></param>
        public QuadraticPolynom(IPolynom p)
        {
            if (p != null && p.Degree == 2)
            {
                A = p[2];
                B = p[1];
                C = p[0];
            }
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="p"></param>
        public QuadraticPolynom(QuadraticPolynom p)
        {
            A = p.A;
            B = p.B;
            C = p.C;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return A.ToString() + "*x^2+" + B.ToString() + "*x+" + C.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        public int Degree
        {
            get { return 2; }
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
                switch (index)
                {
                    case 0:
                        return C;
                    case 1:
                        return B;
                    default:
                        return A;
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        C = value;
                        break;
                    case 1:
                        B = value;
                        break;
                    default:
                        A = value;
                        break;
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
            return A * x * x + B * x + C;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetMinimum()
        {
            if (Numeric.EQ(A, 0))
            {
                return Numeric.UNDEF_DOUBLE;
            }
            else
            {
                return -B / 2.0 * A;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Set(QuadraticPolynom p)
        {
            A = p.A;
            B = p.B;
            C = p.C;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public void Set(double a, double b, double c)
        {
            A = a;
            B = b;
            C = c;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void Set(double[] a)
        {
            if (a != null && a.Length >= 3)
            {
                A = a[2];
                B = a[1];
                C = a[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Set(IPolynom p)
        {
            if (p != null && p.Degree == 2)
            {
                A = p[2];
                B = p[1];
                C = p[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public int FindRoots(ref double r1, ref double r2)
        {

            if (Numeric.EQ(A, 0))
            {
                if (Numeric.EQ(B, 0))
                {
                    r1 = Numeric.UNDEF_DOUBLE;
                    r2 = Numeric.UNDEF_DOUBLE;
                    if (Numeric.EQ(C, 0))
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    r1 = -C / B;
                    r2 = Numeric.UNDEF_DOUBLE;
                    return 1;
                }
            }
            else
            {
                double delta = B * B - 4.0 * A * C;
                if (Numeric.EQ(delta, 0))
                {
                    r1 = -B / (2.0 * A);
                    r2 = r1;
                    return 2;
                }
                else if (Numeric.GT(delta, 0))
                {
                    double tmp1 = 2.0 * A;
                    double tmp2 = Numeric.SqrtEqual(delta);
                    double tmp3 = -B;
                    r1 = (tmp3 + tmp2) / tmp1;
                    r2 = (tmp3 - tmp2) / tmp1;
                    return 2;
                }
                else
                {
                    r1 = Numeric.UNDEF_DOUBLE;
                    r2 = Numeric.UNDEF_DOUBLE;
                    return 0;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public int FindRoots(ref Complex r1, ref Complex r2)
        {
            if (Numeric.EQ(A, 0))
            {
                if (Numeric.EQ(B, 0))
                {
                    r1.SetUndefined();
                    r2.SetUndefined();
                    if (Numeric.EQ(C, 0))
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    r1.Set(-C / B, 0);
                    r2.SetUndefined();
                    return 1;
                }
            }
            else
            {
                double delta = B * B - 4.0 * A * C;
                if (Numeric.EQ(delta, 0))
                {
                    r1.Set(-B / (2.0 * A), 0);
                    r2 = r1;
                    return 2;
                }
                else if (Numeric.GT(delta, 0))
                {
                    double tmp1 = 2.0 * A;
                    double tmp2 = Numeric.SqrtEqual(delta);
                    double tmp3 = -B;
                    r1.Set((tmp3 + tmp2) / tmp1, 0);
                    r2.Set((tmp3 - tmp2) / tmp1, 0);
                    return 2;
                }
                else
                {
                    double tmp1 = 2.0 * A;
                    double tmp2 = Numeric.SqrtEqual(-delta);
                    double tmp3 = -B / tmp1;
                    r1.Set(tmp3, tmp2 / tmp1);
                    r2.Set(tmp3, -tmp2 / tmp1);
                    return 2;
                }
            }
        }

        #region IPolynom  Members
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double FindRoot(double min, double max)
        {
            double root1 = Numeric.UNDEF_DOUBLE;
            double root2 = Numeric.UNDEF_DOUBLE;
            int result = FindRoots(ref root1, ref root2);
            if (result == 0)
            {
                return Numeric.UNDEF_DOUBLE;
            }
            else if (result == -1)
            {
                return min;
            }
            else if (result == 1)
            {
                if (Numeric.GE(root1, min) && Numeric.LE(root1, max))
                {
                    return root1;
                }
                else
                {
                    return Numeric.UNDEF_DOUBLE;
                }
            }
            else
            {
                if (Numeric.GE(root1, min) && Numeric.LE(root1, max))
                {
                    return root1;
                }
                else if (Numeric.GE(root2, min) && Numeric.LE(root2, max))
                {
                    return root2;
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
            C = Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUndefined()
        {
            return Numeric.IsUndefined(A) || Numeric.IsUndefined(B) || Numeric.IsUndefined(C);
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new QuadraticPolynom(this);
        }

        #endregion

        #region ICopyable<IPolynom > Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref IPolynom item)
        {
            if (item != null && item.Degree == 2)
            {
                item[0] = C;
                item[1] = B;
                item[2] = A;
            }
        }

        #endregion

        #region IEquivalent<IPolynom > Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(IPolynom other)
        {
            if (other != null && other.Degree == 2)
            {
                return Numeric.EQ(A, other[2]) && Numeric.EQ(B, other[1]) && Numeric.EQ(C, other[0]);
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
            if (other != null && other.Degree == 2)
            {
                return Numeric.EQ(A, other[2], precision) && Numeric.EQ(B, other[1], precision) && Numeric.EQ(C, other[0], precision);
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region ICopyable<QuadraticPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref QuadraticPolynom item)
        {
            item.A = A;
            item.B = B;
            item.C = C;
        }

        #endregion

        #region IEquivalent<QuadraticPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(QuadraticPolynom other)
        {
            return Numeric.EQ(A, other.A) && Numeric.EQ(B, other.B) && Numeric.EQ(C, other.C);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(QuadraticPolynom other, double precision)
        {
            return Numeric.EQ(A, other.A, precision) && Numeric.EQ(B, other.B, precision) && Numeric.EQ(C, other.C, precision);
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
            C = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return Numeric.EQ(A, 0) && Numeric.EQ(B, 0) && Numeric.EQ(C, 0);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public bool Fit(double x1, double y1, double x2, double y2, double x3, double y3, double epsilon)
        {
            double t1 = ((y2 - y1) * (x1 - x3)) + ((y3 - y1) * (x2 - x1));
            double t2 = ((x1 - x3) * ((x2 * x2) - (x1 * x1))) + ((x2 - x1) * ((x3 * x3) - (x1 * x1)));
            if (!Numeric.EQ(t2, 0, epsilon))
            {
                A = t1 / t2;
                t2 = x2 - x1;
                if (!Numeric.EQ(t2, 0, epsilon))
                {
                    B = ((y2 - y1) - (A * ((x2 * x2) - (x1 * x1)))) / t2;
                    C = ((y1 - (A * (x1 * x1))) - (B * x1));
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
        /// <param name="x"></param>
        /// <returns></returns>
        public double Derive(double x)
        {
            return 2.0 * A * x + B;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double DeriveSecond(double x)
        {
            return 2.0 * A;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public double Integrate(double a, double b)
        {
            double a2 = a * a;
            double b2 = b * b;
            double a3 = a2 * a;
            double b3 = b2 * b;
            return (A * (b3 - a3) / 3.0) + ((B * (b2 - a2) / 2.0) + (C * (b - a)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Derivate(ref IPolynom p)
        {
            if (p != null && p.Degree >= 1)
            {
                p[0] = B;
                p[1] = 2.0 * A;
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
        public void Primitive(ref IPolynom p)
        {
            if (p != null && p.Degree >= 3)
            {
                p[0] = 0;
                p[1] = C;
                p[2] = B / 2.0;
                p[3] = A / 3.0;
                for (int i = 4; i < p.Degree; i++)
                {
                    p[i] = 0;
                }
            }
        }

    }
}
