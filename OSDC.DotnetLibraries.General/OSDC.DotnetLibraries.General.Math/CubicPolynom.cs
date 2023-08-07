using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public struct CubicPolynom : IPolynom, IEquivalent<IPolynom>, IEquivalent<CubicPolynom>, IUndefinable, ICloneable, ICopyable<IPolynom>, ICopyable<CubicPolynom>, IZero
    {
        /// <summary>
        /// Ax^3+Bx^2+Cx+D
        /// </summary>
        public double A { get; set; } = 0;
        /// <summary>
        /// Ax^3+Bx^2+Cx+D
        /// </summary>
        public double B { get; set; } = 0;
        /// <summary>
        /// Ax^3+Bx^2+Cx+D
        /// </summary>
        public double C { get; set; } = 0;
        /// <summary>
        /// Ax^3+Bx^2+Cx+D
        /// </summary>
        public double D { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        public CubicPolynom(double a, double b, double c, double d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public CubicPolynom(double[] a)
        {
            if (a != null && a.Length >= 4)
            {
                A = a[3];
                B = a[2];
                C = a[1];
                D = a[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public CubicPolynom(IPolynom p)
        {
            if (p != null && p.Degree == 3)
            {
                A = p[3];
                B = p[2];
                C = p[1];
                D = p[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public CubicPolynom(QuadraticPolynom p)
        {
            A = p[3];
            B = p[2];
            C = p[1];
            D = p[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return A.ToString() + "*x^3+" + B.ToString() + "*x^2+" + C.ToString() + "*x+" + D.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Set(QuadraticPolynom p)
        {
            A = p[3];
            B = p[2];
            C = p[1];
            D = p[0];
        }

        #region ICubicPolynom Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        public void Set(double a, double b, double c, double d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Set(CubicPolynom p)
        {
            A = p.A;
            B = p.B;
            C = p.C;
            D = p.D;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <param name="r3"></param>
        /// <returns></returns>
        public int FindRoots(ref double r1, ref double r2, ref double r3)
        {
            if (Numeric.EQ(A, 0))
            {
                QuadraticPolynom p = new QuadraticPolynom(B, C, D);
                r3 = Numeric.UNDEF_DOUBLE;
                return p.FindRoots(ref r1, ref r2);
            }
            else
            {
                if (IsUndefined())
                {
                    r1 = r2 = r3 = Numeric.UNDEF_DOUBLE;
                    return -1;
                }
                // calculate the discreminant
                double b2 = B * B;
                double b3 = B * b2;
                double ac = A * C;
                double abc = ac * B;
                double a2 = A * A;
                double a2d = a2 * D;
                double P = b2 - 3.0 * ac;
                double P2 = P* P;
                double P3 = P * P2;
                double Q = 2.0 * b3 - 9.0 * abc + 27 * a2d;
                double Q2 = Q * Q;
                double delta = (4.0 * P - Q) / (27.0 * a2);

                double a = (B / A);
                double b = (C / A);
                double c = (D / A);

                if (Numeric.GT(delta, 0))
                {
                    a2 = a * a;
                    double a3 = a2 * a;
                    Q = (a2 - (b * 3)) / 9.0;
                    double R = ((a3 * 2) - (a * b * 9) + (c * 27)) / 54;
                    double R2 = R * R;
                    double Q3 = Q * Q * Q;

                    // 3 real roots
                    double teta = Numeric.AcosEqual(R / Numeric.SqrtEqual(Q3));
                    double tmp1 = -2.0 * Numeric.SqrtEqual(Q);
                    double tmp2 = a / 3;
                    r1 = (tmp1 * System.Math.Cos(teta / 3.0)) - tmp2;
                    r2 = (tmp1 * System.Math.Cos((teta + (Numeric.PI * 2)) / 3)) - tmp2;
                    r3 = (tmp1 * System.Math.Cos((teta - (Numeric.PI * 2)) / 3)) - tmp2;
                    return 3;
                }
                else if (Numeric.EQ(delta, 0))
                {
                    // multiple roots, all real
                    if (Numeric.EQ(b2, 3.0*ac))
                    {
                        r1 = -B / (3.0 * A);
                        r2 = r1;
                        r3 = r2;
                        return 3;
                    }
                    else
                    {
                        r1 = (4.0 * abc - 9.0 * a2d - b3) / (A * P);
                        r2 = (9.0 * A * D - B * C) / (2.0 * P);
                        r3 = r2;
                        return 3;
                    }
                }
                else
                {
                    a2 = a * a;
                    double a3 = a2 * a;
                    Q = (a2 - (b * 3)) / 9.0;
                    double R = ((a3 * 2) - (a * b * 9) + (c * 27)) / 54;
                    double R2 = R * R;
                    double Q3 = Q * Q * Q;

                    // 1 real root
                    double sign = System.Math.Sign(R);
                    if (Numeric.EQ(sign, 0))
                    {
                        sign = 1.0;
                    }
                    double A = -(sign * System.Math.Pow(Numeric.SqrtEqual((R2 - Q3)) + System.Math.Abs(R), 1.0 / 3.0));
                    double B = 0;
                    if (!Numeric.EQ(A, 0))
                    {
                        B = Q / A;
                    }
                    r1 = ((A + B) - (a / 3));
                    return 1;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <param name="r3"></param>
        /// <returns></returns>
        public int FindRoots(ref Complex r1, ref Complex r2, ref Complex r3)
        {
            if (Numeric.EQ(A, 0))
            {
                QuadraticPolynom p = new QuadraticPolynom(B, C, D);
                r3.SetUndefined();
                return p.FindRoots(ref r1, ref r2);
            }
            else
            {
                if (IsUndefined())
                {
                    r1.SetUndefined();
                    r2.SetUndefined();
                    r3.SetUndefined();
                    return -1;
                }
                // calculate the discreminant
                double b2 = B * B;
                double b3 = B * b2;
                double ac = A * C;
                double abc = ac * B;
                double a2 = A * A;
                double a2d = a2 * D;
                double P = b2 - 3.0 * ac;
                double P2 = P * P;
                double P3 = P * P2;
                double Q = 2.0 * b3 - 9.0 * abc + 27 * a2d;
                double Q2 = Q * Q;
                double delta = (4.0 * P - Q) / (27.0 * a2);

                double a = (B / A);
                double b = (C / A);
                double c = (D / A);

                if (Numeric.GT(delta, 0))
                {
                    // 3 real roots
                    a2 = a * a;
                    double a3 = a2 * a;
                    Q = (a2 - (b * 3)) / 9.0;
                    double R = ((a3 * 2) - (a * b * 9) + (c * 27)) / 54;
                    double R2 = R * R;
                    double Q3 = Q * Q * Q;

                    double teta = Numeric.AcosEqual((R / Numeric.SqrtEqual(Q3)));
                    double tmp1 = -((Numeric.SqrtEqual(Q) * 2));
                    double tmp2 = (a / 3);
                    r1.Set(((tmp1 * System.Math.Cos((teta / 3))) - tmp2), 0);
                    r2.Set(((tmp1 * System.Math.Cos(((teta + (Numeric.PI * 2)) / 3))) - tmp2), 0);
                    r3.Set(((tmp1 * System.Math.Cos(((teta - (Numeric.PI * 2)) / 3))) - tmp2), 0);
                    return 3;
                }
                else if (Numeric.EQ(delta, 0))
                {
                    // multiple roots, all real
                    if (Numeric.EQ(b2, 3.0 * ac))
                    {
                        r1.Set(-B / (3.0 * A), 0);
                        r2 = r1;
                        r3 = r2;
                        return 3;
                    }
                    else
                    {
                        r1.Set((4.0 * abc - 9.0 * a2d - b3) / (A * P), 0);
                        r2.Set((9.0 * A * D - B * C) / (2.0 * P), 0);
                        r3 = r2;
                        return 3;
                    }
                }
                else
                {
                    // 1 real root and 2 conjugate complex roots
                    a2 = a * a;
                    double a3 = a2 * a;
                    Q = (a2 - (b * 3)) / 9.0;
                    double R = ((a3 * 2) - (a * b * 9) + (c * 27)) / 54;
                    double R2 = R * R;
                    double Q3 = Q * Q * Q;

                    double sign = System.Math.Sign(R);
                    if (Numeric.EQ(sign, 0))
                    {
                        sign = 1;
                    }
                    double A = -((sign * System.Math.Pow((Numeric.SqrtEqual((R2 - Q3)) + System.Math.Abs(R)), 1.0 / 3.0)));
                    double B = 0;
                    if (!Numeric.EQ(A, 0))
                    {
                        B = (Q / A);
                    }
                    r1.Set(((A + B) - (a / 3)), 0);
                    r2.Set((((A + B) / -2) - (a / 3)), (Numeric.SqrtEqual(3.0) / 2) * (A - B));
                    r3.Set(r2.Real, -(r2.Imaginary));
                    return 3;
                }
            }
        }

        #endregion

        #region IPolynom Members
        /// <summary>
        /// 
        /// </summary>
        public int Degree
        {
            get { return 3; }
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
                        return D;
                    case 1:
                        return C;
                    case 2:
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
                        D = value;
                        break;
                    case 1:
                        C = value;
                        break;
                    case 2:
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
            if (a != null && a.Length >= 4)
            {
                A = a[3];
                B = a[2];
                C = a[1];
                D = a[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void Set(IPolynom a)
        {
            if (a != null && a.Degree == 3)
            {
                A = a[3];
                B = a[2];
                C = a[1];
                D = a[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double Eval(double x)
        {
            double v = D;
            v += (x * C);
            double xn = x * x;
            v += (xn * B);
            xn *= x;
            return (v + (xn * A));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double FindRoot(double min, double max)
        {
            double r1 = Numeric.UNDEF_DOUBLE;
            double r2 = Numeric.UNDEF_DOUBLE;
            double r3 = Numeric.UNDEF_DOUBLE;
            int results = FindRoots(ref r1, ref r2, ref r3);
            if (results < 0)
            {
                return min;
            }
            else if (results == 0)
            {
                return Numeric.UNDEF_DOUBLE;
            }
            else if (results == 1)
            {
                if (Numeric.GE(r1, min) && Numeric.LE(r1, max))
                {
                    return r1;
                }
                else
                {
                    return Numeric.UNDEF_DOUBLE;
                }
            }
            else if (results == 2)
            {
                if (Numeric.GE(r1, min) && Numeric.LE(r1, max))
                {
                    return r1;
                }
                else if (Numeric.GE(r2, min) && Numeric.LE(r2, max))
                {
                    return r2;
                }
                else
                {
                    return Numeric.UNDEF_DOUBLE;
                }
            }
            else
            {
                if (Numeric.GE(r1, min) && Numeric.LE(r1, max))
                {
                    return r1;
                }
                else if (Numeric.GE(r2, min) && Numeric.LE(r2, max))
                {
                    return r2;
                }
                else if (Numeric.GE(r3, min) && Numeric.LE(r3, max))
                {
                    return r3;
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
            D = Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUndefined()
        {
            return Numeric.IsUndefined(A) || Numeric.IsUndefined(B) || Numeric.IsUndefined(C) || Numeric.IsUndefined(D) || Numeric.IsInfinity(A) || Numeric.IsInfinity(B)
                || Numeric.IsInfinity(C) || Numeric.IsInfinity(D);
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new CubicPolynom(this);
        }

        #endregion

        #region ICopyable<IPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref IPolynom item)
        {
            if (item != null && item.Degree == 3)
            {
                item[0] = D;
                item[1] = C;
                item[2] = B;
                item[3] = A;
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
            if (other == null || other.Degree != 3)
            {
                return false;
            }
            else
            {
                return Numeric.EQ(A, other[3]) && Numeric.EQ(B, other[2]) && Numeric.EQ(C, other[1]) && Numeric.EQ(D, other[0]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(IPolynom other, double precision)
        {
            if (other == null || other.Degree != 3)
            {
                return false;
            }
            else
            {
                return Numeric.EQ(A, other[3], precision) && Numeric.EQ(B, other[2], precision) && Numeric.EQ(C, other[1], precision) && Numeric.EQ(D, other[0], precision);
            }
        }
        #endregion



        #region ICopyable<CubicPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref CubicPolynom item)
        {
            item[0] = D;
            item[1] = C;
            item[2] = B;
            item[3] = A;
        }

        #endregion

        #region IEquivalent<CubicPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(CubicPolynom other)
        {
            return Numeric.EQ(A, other.A) && Numeric.EQ(B, other.B) && Numeric.EQ(C, other.C) && Numeric.EQ(D, other.D);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(CubicPolynom other, double precision)
        {
            return Numeric.EQ(A, other.A, precision) && Numeric.EQ(B, other.B, precision) && Numeric.EQ(C, other.C, precision) && Numeric.EQ(D, other.D, precision);
        }

        #endregion

        #region IZero Members
        /// <summary>
        /// 
        /// </summary>
        public void SetZero()
        {
            A = 0;
            B = 0l;
            C = 0l;
            D = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return Numeric.EQ(A, 0) && Numeric.EQ(B, 0) && Numeric.EQ(C, 0) && Numeric.EQ(D, 0);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double Derive(double x)
        {
            double v = C;
            v += B * x * 2;
            return v + x * x * A * 3;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double DeriveSecond(double x)
        {
            return x * A * 6.0 + B * 2.0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public double Integrate(double a, double b)
        {
            double v = (b - a) * D;
            double an = a * a;
            double bn = b * b;
            v += (bn - an) * C / 2.0;
            an *= a;
            bn *= b;
            v += (bn - an) * B / 3.0;
            an *= a;
            bn *= b;
            return v + (bn - an) * A / 4.0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Derivate(ref IPolynom p)
        {
            if (p != null && p.Degree >= 2)
            {
                p[0] = C;
                p[1] = B * 2;
                p[2] = A * 3;
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
        public void Primitive(ref IPolynom p)
        {
            if (p != null && p.Degree >= 4)
            {
                p[0] = 0;
                p[1] = D;
                p[2] = C / 2;
                p[3] = B / 3;
                p[4] = A / 4;
                for (int i = 5; i < p.Degree; i++)
                {
                    p[i] = 0;
                }
            }
        }

    }
}
