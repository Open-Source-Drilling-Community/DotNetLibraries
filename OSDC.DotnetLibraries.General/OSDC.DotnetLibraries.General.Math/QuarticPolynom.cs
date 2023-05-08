using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public struct QuarticPolynom : IPolynom, IEquatable<IPolynom>, IEquatable<QuarticPolynom>, IUndefinable, ICloneable, ICopyable<IPolynom>, ICopyable<QuarticPolynom>, IZero
    {
        /// <summary>
        /// A x^4 + B x^3 + C x^2  + D x + E
        /// </summary>
        public double A { get; set; } = 0;
        /// <summary>
        /// A x^4 + B x^3 + C x^2  + D x + E
        /// </summary>
        public double B { get; set; } = 0;
        /// <summary>
        /// A x^4 + B x^3 + C x^2  + D x + E
        /// </summary>
        public double C { get; set; } = 0;
        /// <summary>
        /// A x^4 + B x^3 + C x^2  + D x + E
        /// </summary>
        public double D { get; set; } = 0;
        /// <summary>
        /// A x^4 + B x^3 + C x^2  + D x + E
        /// </summary>
        public double E { get; set; } = 0;


        /// <summary>
        /// Polynomial of the form
        /// 
        ///  A x^4 + B x^3 + C x^2  + D x + E
        /// 
        /// The array is stored as values[0] = A, values[1] = B, etc...
        /// </summary>
        /// <param name="values"></param>
        public QuarticPolynom(double[] values)
        {
            if (values != null && values.Length >= 5)
            {
                A = values[4];
                B = values[3];
                C = values[2];
                D = values[1];
                E = values[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        public QuarticPolynom(double a, double b, double c, double d, double e)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        public QuarticPolynom(QuarticPolynom src)
        {
            A = src.A;
            B = src.B;
            C = src.C;
            D = src.D;
            E = src.E;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        public QuarticPolynom(IPolynom src)
        {
            if (src != null && src.Degree == 4)
            {
                A = src[4];
                B = src[3];
                C = src[2];
                D = src[1];
                E = src[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return A.ToString() + "*x^4+" + B.ToString() + "*x^3+" + C.ToString() + "*x^2+" + D.ToString() + "*x+" + E.ToString();
        }

        #region IPolynom<T> Members

        /// <summary>
        /// 
        /// </summary>
        public int Degree
        {
            get
            {
                return 4;
            }
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
                        return E;
                    case 1:
                        return D;
                    case 2:
                        return C;
                    case 3:
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
                        E = value;
                        break;
                    case 1:
                        D = value;
                        break;
                    case 2:
                        C = value;
                        break;
                    case 3:
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
        /// <param name="a"></param>
        public void Set(double[] a)
        {
            if (a != null && a.Length >= 5)
            {
                A = a[4];
                B = a[3];
                C = a[2];
                D = a[1];
                E = a[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void Set(IPolynom a)
        {
            if (a != null && a.Degree == 4)
            {
                A = a[4];
                B = a[3];
                C = a[2];
                D = a[1];
                E = a[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double Eval(double x)
        {
            double result = E;
            double xn = x;
            result += xn * D;
            xn *= x;
            result += xn * C;
            xn *= x;
            result += xn * B;
            xn *= x;
            result += xn * A;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double Derive(double x)
        {
            double xn = 1.0;
            double sum = 0;
            sum += D * xn;
            xn *= x;
            sum += 2.0 * C * xn;
            xn *= x;
            sum += 3.0 * B * xn;
            xn *= x;
            sum += 4.0 * A * xn;
            return sum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double DeriveSecond(double x)
        {
            double xn = 1.0;
            double sum = 0;
            sum += 2.0 * C;
            xn *= x;
            sum += 6.0 * B * xn;
            xn*= x;
            sum += 12.0 * A * xn;
            return sum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public double Integrate(double a, double b)
        {
            double sum = 0;
            double an = a;
            double bn = b;
            sum += E * (bn - an);
            an *= a;
            bn *= b;
            sum += (D / 2.0) * (bn - an);
            an *= a;
            bn *= b;
            sum += (C / 3.0) * (bn - an);
            an *= a;
            bn *= b;
            sum += (B / 4.0) * (bn - an);
            an*= a;
            bn *= b;
            sum += (A / 5.0) * (bn - an);
            return sum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Derivate(ref IPolynom p)
        {
            if (p != null && p.Degree >= 3)
            {
                p[0] = D;
                p[1] = 2.0 * C;
                p[2] = 3.0 * B;
                p[3] = 4.0 * A;
                for (int i = 4; i < p.Degree; i++)
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
            if (p != null && p.Degree >= 5)
            {
                p[0] = 0;
                p[1] = E;
                p[2] = D / 2;
                p[3] = C / 3;
                p[4] = B / 4;
                p[5] = A / 5;
                for (int i = 6; i < p.Degree; i++)
                {
                    p[i] = 0;
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
            D = Numeric.UNDEF_DOUBLE;
            E = Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUndefined()
        {
            return Numeric.IsUndefined(A) || Numeric.IsUndefined(B)
                || Numeric.IsUndefined(C) || Numeric.IsUndefined(D)
                || Numeric.IsUndefined(E) || Numeric.IsInfinity(A) || Numeric.IsInfinity(B)
                || Numeric.IsInfinity(C) || Numeric.IsInfinity(D)
                || Numeric.IsInfinity(E);
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new QuarticPolynom(this);
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
            D = 0;
            E = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return Numeric.EQ(A, 0) && Numeric.EQ(B, 0) && Numeric.EQ(C, 0) && Numeric.EQ(D, 0) && Numeric.EQ(E, 0);
        }

        #endregion

        #region ICopyable<IPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dest"></param>
        public void Copy(ref IPolynom dest)
        {
            if (dest != null && dest.Degree == 4)
            {
                dest[0] = E;
                dest[1] = D;
                dest[2] = C;
                dest[3] = B;
                dest[4] = A;
            }
        }

        #endregion

        #region IEquatable<IPolynom> Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(IPolynom other)
        {
            if (other == null || other.Degree != 4)
            {
                return false;
            }
            else
            {
                return Numeric.EQ(A, other[4]) && 
                       Numeric.EQ(B, other[3]) && 
                       Numeric.EQ(C, other[2]) && 
                       Numeric.EQ(D, other[1]) && 
                       Numeric.EQ(E, other[0]);
            }
        }

        #endregion

        #region ICopyable<QuarticPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref QuarticPolynom item)
        {
            item.A = A;
            item.B = B;
            item.C = C;
            item.D = D;
            item.E = E;
        }

        #endregion

        #region IEquatable<QuarticPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(QuarticPolynom other)
        {
            return Numeric.EQ(A, other.A) &&
                   Numeric.EQ(B, other.B) &&
                   Numeric.EQ(C, other.C) &&
                   Numeric.EQ(D, other.D) &&
                   Numeric.EQ(E, other.E);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double FindRoot(double min, double max)
        {
            Complex r1 = Complex.Zero;
            Complex r2 = Complex.Zero;
            Complex r3 = Complex.Zero;
            Complex r4 = Complex.Zero;
            int count = FindRoots(ref r1, ref r2, ref r3, ref r4);
            if (count > 0 && Numeric.EQ(r1.Imaginary, 0))
            {
                double rr = r1.Real;
                if (Numeric.IsBetween(rr, min, max))
                {
                    return rr;
                }
            }
            if (count > 1 && Numeric.EQ(r2.Imaginary, 0))
            {
                double rr = r2.Real;
                if (Numeric.IsBetween(rr, min, max))
                {
                    return rr;
                }
            }
            if (count > 2 && Numeric.EQ(r3.Imaginary, 0))
            {
                double rr = r3.Real;
                if (Numeric.IsBetween(rr, min, max))
                {
                    return rr;
                }
            }
            if (count > 3 && Numeric.EQ(r4.Imaginary, 0))
            {
                double rr = r4.Real;
                if (Numeric.IsBetween(rr, min, max))
                {
                    return rr;
                }
            }
            return Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// solving using Ferrari's method
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <param name="r3"></param>
        /// <param name="r4"></param>
        /// <returns></returns>
        public int FindRoots(ref Complex r1, ref Complex r2, ref Complex r3, ref Complex r4)
        {
            if (IsUndefined())
            {
                r1.SetUndefined();
                r2.SetUndefined();
                r3.SetUndefined();
                r4.SetUndefined();
                return -1;
            }
            else
            {
                if (!Numeric.EQ(A, 0))
                {
                    // Normalize the coefficients
                    double b = B / A;
                    double c = C / A;
                    double d = D / A;
                    double e = E / A;

                    // Calculate the coefficients of the resolvent cubic equation
                    double b2 = b * b;
                    double b3 = b2 * b;
                    double b4 = b3 * b;
                    double alpha = -3.0 * b2 / 8.0 + c;
                    double beta = b3 / 8.0 - b * c / 2.0 + d;
                    double gamma = -3.0 * b4 / 256.0 + b2 * c / 16.0 - b * d / 4.0 + e;

                    if (Numeric.EQ(beta, 0))
                    {
                        Complex a0 = new Complex(alpha * alpha - 4.0 * gamma, 0);
                        Complex a1 = Complex.Sqrt(a0);
                        Complex a2 = Complex.Sqrt((-alpha + a1) / 2.0);
                        Complex a3 = Complex.Sqrt((-alpha - a1) / 2.0);
                        r1 = -b / 4.0 + a2;
                        r2 = -b / 4.0 + a3;
                        r3 = -b / 4.0 - a2;
                        r4 = -b / 4.0 - a3;
                        return 4;
                    }
                    else
                    {
                        double alpha2 = alpha * alpha;
                        double alpha3 = alpha2 * alpha;
                        Complex p = new Complex(-alpha2/12.0 -gamma, 0);
                        Complex q = new Complex(-alpha3 / 108.0 + alpha * gamma / 3.0 - beta * beta / 8.0, 0);
                        Complex r = -q / 2.0 + Complex.Sqrt(q * q / 4.0 + p * p * p / 27.0);
                        Complex u = Complex.Exp((1.0/3.0)*Complex.Log(r));
                        Complex y = new Complex((-5.0 / 6.0) * alpha, 0);
                        if (u== 0)
                        {
                            y = y - Complex.Exp((1.0/3.0)*Complex.Log(q));
                        }
                        else
                        {
                            y = y + u - p / (3.0 * u);
                        }
                        Complex w = Complex.Sqrt(alpha + 2.0 * y);
                        Complex w1 = -(3.0 * alpha + 2.0 * y + 2.0 * beta / w);
                        Complex w2 = -(3.0 * alpha + 2.0 * y - 2.0 * beta / w);
                        r1 = -b / 4.0 + 0.5 * (w + Complex.Sqrt(w1));
                        r2 = -b / 4.0 + 0.5 * (w - Complex.Sqrt(w1));
                        r3 = -b / 4.0 + 0.5 * (-w + Complex.Sqrt(w2));
                        r4 = -b / 4.0 + 0.5 * (-w - Complex.Sqrt(w2));
                        return 4;
                    }
                }
                else
                {
                    CubicPolynom cubic = new CubicPolynom(B, C, D, E);
                    r4.SetUndefined();
                    return cubic.FindRoots(ref r1, ref r2, ref r3);
                }
            }
        }

    }
}
