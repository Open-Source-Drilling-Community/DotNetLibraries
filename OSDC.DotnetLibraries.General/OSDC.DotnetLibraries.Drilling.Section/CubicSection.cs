using System;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    public class CubicSection : ArcSection
    {
        /// <summary>
        /// Simpson's method has an error of the order of 1/N^4
        /// For a 10000m curve that gives an error of around 0.1m
        /// </summary>
        private static int TERMS_ = 18;

        private double[] a_ = null;
        private double[] b_ = null;
        private double[] c_ = null;
        private double[] d_ = null;

        /// <summary>
        /// does nothing
        /// </summary>
        public override NonLocalizedCurve Curve
        {
            get { return null; }
            set { }
        }
        /// <summary>
        /// default constructor
        /// </summary>
        public CubicSection() : base()
        {
            a_ = new double[3];
            b_ = new double[3];
            c_ = new double[3];
            d_ = new double[3];
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public CubicSection(CurvilinearPoint3D start, CurvilinearPoint3D end)
        {
            Start = start;
            End = end;
            a_ = new double[3];
            b_ = new double[3];
            c_ = new double[3];
            d_ = new double[3];
        }

        public override CurvilinearPoint3D InterpolateAtMD(double s)
        {
            if (Start == null || End == null || Start.IsUndefined() || End.IsUndefined())
            {
                return null;
            }
            else
            {
                if (Numeric.EQ(Start.Abscissa, End.Abscissa))
                {
                    return new CurvilinearPoint3D(Start);
                }
                else
                {
                    // find the parameter t corresponding to s
                    double si = (double)Start.Abscissa;
                    double t0 = (s - si) / (double)(End.Abscissa - si);
                    double s0 = si + GetLength(t0, TERMS_);
                    double delta = 0.0001;
                    int cc = 0;
                    while (System.Math.Abs(s0 - s) > 1e-2 && cc++ < 100)
                    {
                        double t1 = t0 + delta;
                        double s1 = si + GetLength(t1, TERMS_);
                        double slope = (s1 - s0) / delta;
                        if (!Numeric.EQ(slope, 0))
                        {
                            t0 -= (s0 - s) / slope;
                            s0 = si + GetLength(t0, TERMS_);
                        }
                        else
                        {
                            break;
                        }
                    }
                    double t = (s - (double)Start.Abscissa) / (double)(End.Abscissa - Start.Abscissa);
                    if (System.Math.Abs(s0 - s) <= 1e-2)
                    {
                        t = t0;
                    }
                    CurvilinearPoint3D p = new CurvilinearPoint3D();                
                    double t2 = t * t;
                    double t3 = t * t2;
                    p.Set(a_[0] * t3 + b_[0] * t2 + c_[0] * t + d_[0], a_[1] * t3 + b_[1] * t2 + c_[1] * t + d_[1], a_[2] * t3 + b_[2] * t2 + c_[2] * t + d_[2]);
                    p.Abscissa = Start.Abscissa + GetLength(t, TERMS_);
                    Vector3D v1 = GetDerivate(t);
                    p.Inclination = v1.GetIncl();
                    p.Azimuth = v1.GetAz();
                    return p;                 
                }
            }
        }
        /// <summary>
        /// only implemented for XYZ
        /// </summary>
        /// <returns></returns>
        public override bool Calculate()
        {
            if (Start != null && End != null && 
                !Start.IsUndefined() && 
                Numeric.IsDefined(End.X) &&
                Numeric.IsDefined(End.Y) &&
                Numeric.IsDefined(End.Z) &&
                Numeric.IsDefined(End.Inclination) && 
                Numeric.IsDefined(End.Azimuth))
            {
                return CalculateXYZ();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// calculate the cubic polynomial coefficients and the length of the curve
        /// </summary>
        /// <returns></returns>
        public bool CalculateXYZ()
        {
            if (Start != null && End != null &&
                !Start.IsUndefined() &&
                Numeric.IsDefined(End.X) &&
                Numeric.IsDefined(End.Y) &&
                Numeric.IsDefined(End.Z) &&
                Numeric.IsDefined(End.Inclination) &&
                Numeric.IsDefined(End.Azimuth))
            {
                d_[0] = (double)Start.X;
                d_[1] = (double)Start.Y;
                d_[2] = (double)Start.Z;
                double L = (double)Start.Distance(End);
                Vector3D v0 = Vector3D.CreateSpheric(2.0*L, Start.Inclination, Start.Azimuth);
                Vector3D v1 = Vector3D.CreateSpheric(2.0*L, End.Inclination, End.Azimuth);
                c_[0] = (double)v0.X;
                c_[1] = (double)v0.Y;
                c_[2] = (double)v0.Z;
                b_[0] = (double)(3.0 * (End.X - Start.X) - v1.X - 2.0 * v0.X);
                b_[1] = (double)(3.0 * (End.Y - Start.Y) - v1.Y - 2.0 * v0.Y);
                b_[2] = (double)(3.0 * (End.Z - Start.Z) - v1.Z - 2.0 * v0.Z);
                a_[0] = (double)(v1.X + v0.X - 2.0 * (End.X - Start.X));
                a_[1] = (double)(v1.Y + v0.Y - 2.0 * (End.Y - Start.Y));
                a_[2] = (double)(v1.Z + v0.Z - 2.0 * (End.Z - Start.Z));

                Vector3D t0 = GetDerivate(0);
                Vector3D t1 = GetDerivate(1);
                double l = GetLength(1.0, TERMS_);
                End.Abscissa = Start.Abscissa + l;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// first derivative
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private Vector3D GetDerivate(double t)
        {
            double t2 = t * t;
            return new Vector3D(3.0 * a_[0] * t2 + 2.0 * b_[0] * t + c_[0], 3.0 * a_[1] * t2 + 2.0 * b_[1] * t + c_[1], 3.0 * a_[2] * t2 + 2.0 * b_[2] * t + c_[2]);
        }
        /// <summary>
        /// Simpson integration of the length between 0 and t
        /// </summary>
        /// <param name="t"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private double GetLength(double t, int n)
        {
            double r = 0;
            double x = 0;
            double step = (2.0 * t) / n;
            Vector3D v1 = GetDerivate(0.0);
            r += (double)v1.GetLength();
            v1 = GetDerivate(t);
            r += (double)v1.GetLength();
            for (int i = 2; i <= n-2; i+=2)
            {
                x += step;
                v1 = GetDerivate(x);
                r += 2.0 * (double)v1.GetLength();
            }
            x = -step / 2.0;
            for (int i = 1; i <= n-1; i+=2)
            {
                x += step;
                v1 = GetDerivate(x);
                r += 4.0 * (double)v1.GetLength();
            }
            return (r * t) / (3.0 * n);
        }
    }
}
