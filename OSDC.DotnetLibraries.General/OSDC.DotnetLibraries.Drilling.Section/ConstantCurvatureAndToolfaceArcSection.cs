using System;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// a section based on a constant curvature and toolface curve in between the start and end of the section
    /// </summary>
    [Serializable]
    public class ConstantCurvatureAndToolfaceArcSection : ArcSection
    {
        public static int IntegrationCount = 100;
        public NonLocalizedConstantCurvatureAndToolfaceCurve CTCCurve { get; set; } = new NonLocalizedConstantCurvatureAndToolfaceCurve();
        /// <summary>
        /// 
        /// </summary>
        public override NonLocalizedCurve Curve
        {
            get => CTCCurve;
            set
            {
                if (value is NonLocalizedConstantCurvatureAndToolfaceCurve)
                {
                    CTCCurve = (NonLocalizedConstantCurvatureAndToolfaceCurve)value;
                }
            }
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public ConstantCurvatureAndToolfaceArcSection()
        {
        }
        public override bool Calculate()
        {
            if (Numeric.IsDefined(CTCCurve.Curvature) &&
                         Numeric.IsDefined(CTCCurve.Toolface) &&
                         Numeric.IsDefined(CTCCurve.Length))
            {
                return CalculateLDT();
            }
            else if (Numeric.IsDefined(CTCCurve.Curvature) &&
                     Numeric.IsDefined(CTCCurve.Toolface) &&
                     Numeric.IsDefined(End.Abscissa))
            {
                return CalculateSDT();
            }
            else
            {
                return false;
            }
        }

        public override CurvilinearPoint3D InterpolateAtMD(double md)
        {
            CurvilinearPoint3D point = new CurvilinearPoint3D();
            point.SetUndefined();
            bool success = InterpolateAtMD(md, point);
            if (success)
            {
                return point;
            }
            else
            {
                return null;
            }
        }
        public bool InterpolateAtMD(double md, CurvilinearPoint3D point)
        {
            if (point != null)
            {
                CurvilinearPoint3D point1 = Start;
                CurvilinearPoint3D point2 = End;
                if (Numeric.EQ(point1.Abscissa, md, Numeric.DEPTH_ACCURACY))
                {
                    point.Set(point1);
                    return true;
                }
                else if (Numeric.EQ(point2.Abscissa, md, Numeric.DEPTH_ACCURACY))
                {
                    point.Set(point2);
                    return true;
                }
                else if (!Numeric.IsBetween(md, (double)point1.Abscissa, (double)point2.Abscissa))
                {
                    return false;
                }
                ConstantCurvatureAndToolfaceArcSection workingSection = new ConstantCurvatureAndToolfaceArcSection();
                workingSection.Start.Set(point1);
                workingSection.End = point;
                point.Abscissa = md;
                workingSection.CTCCurve.Curvature = CTCCurve.Curvature;
                workingSection.CTCCurve.Toolface = CTCCurve.Toolface;
                workingSection.CalculateSDT();
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CalculateSDT()
        {
            if (Numeric.IsDefined(CTCCurve.Curvature) &&
                Numeric.IsDefined(CTCCurve.Toolface) &&
                Numeric.IsDefined(End.Abscissa))
            {
                CTCCurve.Length = End.Abscissa - Start.Abscissa;
                return CalculateLDT();
            }
            else
            {
                return false;
            }
        }

        private bool CalculateLDT()
        {
            if (Numeric.IsDefined(CTCCurve.Curvature) &&
                Numeric.IsDefined(CTCCurve.Toolface) &&
                Numeric.IsDefined(CTCCurve.Length))
            {
                double l = (double)(CTCCurve.Length);
                End.Abscissa = Start.Abscissa + l;
                double kappa = (double)CTCCurve.Curvature;
                double TF = (double)CTCCurve.Toolface;
                double incl0 = (double)Start.Inclination;
                double az0 = (double)Start.Azimuth;
                if (Numeric.EQ(CTCCurve.Curvature, 0))
                {
                    double si = System.Math.Sin(incl0);
                    End.Inclination = Start.Inclination;
                    End.Azimuth = Start.Azimuth;
                    End.X = Start.X + l * si * System.Math.Cos(az0);
                    End.Y = Start.Y + l * si * System.Math.Sin(az0);
                    End.Z = Start.Z + l * System.Math.Cos(incl0);
                    return true;
                }
                else if (Numeric.EQ(Start.Inclination, 0))
                {
                    double incl1 = (l * kappa) % Numeric.PI;
                    double az1 = TF;
                    int halfCircleSign = 1;
                    if (Numeric.GT(l * kappa, Numeric.PI))
                    {
                        az1 += Numeric.PI;
                        incl1 = Numeric.PI - incl1;
                        halfCircleSign = -1;
                    }
                    End.Inclination = incl1;
                    End.Azimuth = az1;
                    double ci = System.Math.Cos(incl1);
                    End.X = Start.X + halfCircleSign * System.Math.Cos(az1) * (1.0 - ci) / kappa;
                    End.Y = Start.Y + halfCircleSign * System.Math.Sin(az1) * (1.0 - ci) / kappa;
                    End.Z = Start.Z + halfCircleSign * System.Math.Sin(incl1) / kappa;
                    return true;
                }
                else if (Numeric.EQ(TF % Math.PI, 0) || Numeric.EQ(TF, -Math.PI))
                {
                    // build up rate is zero
                    double tau = kappa / Math.Sin(incl0);
                    if (Numeric.GT(TF % 2.0*Math.PI, Math.PI) || Numeric.LT(TF % 2.0*Math.PI, 0))
                    {
                        tau *= -1.0;
                    }

                    End.Inclination = incl0;
                    End.Azimuth = (az0 + tau * tau) % 2.0 * Math.PI;
                    End.Z = Start.Z + l * Math.Cos(incl0);
                    double si = Math.Sin(incl0);
                    End.X = Start.X + (si / tau) * (Math.Sin(tau * l + az0) - Math.Sin(az0));
                    End.Y = Start.Y = (si / tau) * (Math.Cos(az0) - Math.Cos(tau * l + az0));
                    return true;
                }
                else
                {
                    double beta = CTCCurve.Curvature * Math.Cos(TF);
                    double incl1 = incl0 + beta * l;
                    End.Inclination = incl1;
                    End.Z = Start.Z + (Math.Sin(incl1) - Math.Sin(incl0)) / beta;
                    double A = Math.Sqrt(kappa * kappa - beta * beta) / beta;
                    double C = az0 - A * Math.Log(Math.Abs(Math.Tan(incl0 / 2.0)));
                    // calculate integrals
                    double intX = SimpsonIntegration(x => fX(x, beta, incl0, A, C), 0, l, IntegrationCount);
                    double intY = SimpsonIntegration(x => fY(x, beta, incl0, A, C), 0, l, IntegrationCount);
                    End.X = Start.X + intX;
                    End.Y = Start.Y + intY;
                    End.Azimuth = A * Math.Log(Math.Abs(Math.Tan(0.5 * (beta * l * incl0)))) + az0 - A * Math.Log(Math.Abs(Math.Tan(0.5 * incl0)));
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private double SimpsonIntegration(Func<double, double> f, double a, double b, int n)
        {
            if (n % 2 != 0)
            {
                throw new ArgumentException("The number of intervals must be even.");
            }

            double h = (b - a) / n;
            double sum = f(a) + f(b);

            for (int i = 1; i < n; i++)
            {
                double x = a + i * h;
                sum += f(x) * (i % 2 == 0 ? 2 : 4);
            }

            return h / 3 * sum;
        }

        private double fX(double s, double beta, double incl0, double A, double C)
        {
            return Math.Sin(beta * s + incl0) * Math.Cos(A * Math.Log(Math.Abs(Math.Tan(0.5 * (beta * s + incl0)))) + C);
        }
        private double fY(double s, double beta, double incl0, double A, double C)
        {
            return Math.Sin(beta * s + incl0) * Math.Sin(A * Math.Log(Math.Abs(Math.Tan(0.5 * (beta * s + incl0)))) + C);
        }
    }
}
