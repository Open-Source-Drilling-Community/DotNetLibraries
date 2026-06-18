using System;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Statistics;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// A double arc section utilizing the same curvature for both arcs
    /// </summary>
    public class DoubleArcs : ArcSection
    {
        /// <summary>
        /// The intermediate point in between the two circular arcs
        /// </summary>
        public CurvilinearPoint3D Intermediate { get; set; } = new CurvilinearPoint3D();
        /// <summary>
        /// A double arc curve
        /// </summary>
        public NonLocalizedDoubleArcs DoubleArcCurve { get; set; } = new NonLocalizedDoubleArcs();
        /// <summary>
        /// the generic accessor to the curve
        /// </summary>
        public override NonLocalizedCurve Curve
        {
            get => DoubleArcCurve;
            set
            {
                if (value is NonLocalizedDoubleArcs)
                {
                    DoubleArcCurve = (NonLocalizedDoubleArcs)value;
                }
            }
        }

        /// <summary>
        /// Calculate
        /// </summary>
        /// <returns></returns>
        public override bool Calculate()
        {
            return CalculateXYZ();
        }

        public bool CalculateXYZ()
        {
            CircularArcSection tmpXYZ = new CircularArcSection(Start, End);
            if (Start != null && tmpXYZ.End != null && tmpXYZ.End.Inclination != null && Numeric.EQ(tmpXYZ.End.Inclination, 0))
            {
                tmpXYZ.End.Inclination = 0.005 * Numeric.PI / 180.0;
                tmpXYZ.End.Azimuth = tmpXYZ.Start.GetAz(tmpXYZ.End);
            }
            if (tmpXYZ.CalculateXYZ() && 
                Numeric.EQ(End.Inclination, tmpXYZ.End.Inclination, 2e-4) &&
                Numeric.EQ(End.Azimuth, tmpXYZ.End.Azimuth, 2e-4))
            {
                // only one arc is necessary to reach the point and respect the final Incl and Az
                DoubleArcCurve.Curvature = tmpXYZ.Circle.Curvature;
                DoubleArcCurve.UpstreamReferenceToolface = tmpXYZ.Circle.ReferenceToolface;
                DoubleArcCurve.DownstreamReferenceToolface = tmpXYZ.Circle.ReferenceToolface;
                Intermediate.Set(End);
                return true;
            }
            else
            {
                // general case
                CubicSection tmpCubic = new CubicSection(Start, End);
                if (tmpCubic.CalculateXYZ())
                {
                    double[] trials = new double[] { 0.5, 0.25, 0.75 };
                    section1_.Start = new CurvilinearPoint3D();
                    section1_.End = new CurvilinearPoint3D();
                    section2_.Start = section1_.End;
                    section2_.End = new CurvilinearPoint3D();
                    section1_.Start.Set(Start);
                    section2_.End.Set(End);
                    for (int i = 0; i < trials.Length; i++)
                    { 
                        CurvilinearPoint3D inter = tmpCubic.InterpolateAtMD((double)(tmpCubic.Start.Abscissa + trials[i] * (tmpCubic.End.Abscissa - tmpCubic.Start.Abscissa)));
                        double[] xs = new double[] { 0, 1, 2};
                        double[] ys = new double[] { (double)End.Inclination, (double)End.Azimuth, 0 };
                        double[] sigs = new double[] { 1.0, 1.0, 1e-1 };
                        double[] a = new double[] { (double)inter.X, (double)inter.Y, (double)inter.Z };
                        bool[] ia = new bool[] { true, true, true };
                        DataModelling.NonLinearFitting(xs, ys, sigs, a, ia, EvaluateDoubleArc, out double chisq);
                        if (chisq < 1.0)
                        {
                            Intermediate.Set(a[0], a[1], a[2]);
                            CircularArcSection section1 = new CircularArcSection(Start, Intermediate);
                            section1.CalculateXYZ();
                            CircularArcSection section2 = new CircularArcSection(section1.End, End);
                            section2.CalculateXYZ();
                            Intermediate.Set(section1.End);
                            DoubleArcCurve.Curvature = section1.Circle.Curvature;
                            DoubleArcCurve.UpstreamReferenceToolface = section1.Circle.ReferenceToolface;
                            DoubleArcCurve.DownstreamReferenceToolface = section2.Circle.ReferenceToolface;
                            return true;
                        }  
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }
        private static CircularArcSection section1_ = new CircularArcSection();
        private static CircularArcSection section2_ = new CircularArcSection();
        private static double[] lastParams = new double[3];
        private static double[] lastResults = new double[3];
        private double EvaluateDoubleArc(double x, double[] @params)
        {
            if (lastParams == null || !Numeric.EQ(@params[0], lastParams[0]) || !Numeric.EQ(@params[1], lastParams[1]) || !Numeric.EQ(@params[2], lastParams[2]))
            {
                section1_.End.Set(@params[0], @params[1], @params[2]);
                section1_.CalculateXYZ();
                section2_.CalculateXYZ();
                lastParams[0] = @params[0];
                lastParams[1] = @params[1];
                lastParams[2] = @params[2];
                lastResults[0] = (double)section2_.End.Inclination;
                lastResults[1] = (double)section2_.End.Azimuth;
                lastResults[2] = (double)(section2_.Circle.Curvature - section1_.Circle.Curvature);
            }
            return lastResults[(int)x];
        }

        /// <summary>
        /// Interpolate at MD and return the interpolation
        /// </summary>
        /// <param name="md"></param>
        /// <returns></returns>
        public override CurvilinearPoint3D InterpolateAtMD(double md)
        {
            CurvilinearPoint3D result = new CurvilinearPoint3D();
            result.SetUndefined();
            InterpolateAtMD(md, result);
            return result;
        }

        /// <summary>
        /// Set the result of the interpolation at a given MD in the point p
        /// </summary>
        /// <param name="md"></param>
        /// <param name="p"></param>
        public void InterpolateAtMD(double md, CurvilinearPoint3D p)
        {
            if (p != null)
            {
                if (Start == null || Start.IsUndefined() || End == null || End.IsUndefined())
                {
                    p.SetUndefined();
                }
                else if (DoubleArcCurve == null || DoubleArcCurve.Curvature == null)
                {
                    CircularArcSection arc = new CircularArcSection(Start, End);
                    arc.Calculate();
                    arc.InterpolateAtMD(md, p);
                } else
                {
                    if (End.EQ(Intermediate))
                    {
                        CircularArcSection arc = new CircularArcSection(Start, End);
                        arc.Calculate();
                        arc.InterpolateAtMD(md, p);
                    }
                    else
                    {
                        double? s1 = Start.Abscissa;
                        double? s2 = End.Abscissa;
                        if (Numeric.EQ(s1, s2, 0.001))
                        {
                            p.Set(Start);
                        }
                        else
                        {
                            if (Numeric.IsUndefined(Start.Inclination) || Numeric.IsUndefined(Start.Azimuth))
                            {
                                Vector3D v = new Vector3D(Start, Intermediate);
                                if (Numeric.IsUndefined(Start.Inclination))
                                {
                                    Start.Inclination = v.GetIncl();
                                }
                                if (Numeric.IsUndefined(Start.Azimuth))
                                {
                                    Start.Azimuth = v.GetAz();
                                }
                            }
                            p.Abscissa = md;
                            CircularArcSection tmp = new CircularArcSection();
                            tmp.End.Set(p);
                            tmp.Circle.Curvature = DoubleArcCurve.Curvature;
                            if (Numeric.LE(md, Intermediate.Abscissa))
                            {
                                tmp.Start.Set(Start);
                                tmp.Circle.ReferenceToolface = DoubleArcCurve.UpstreamReferenceToolface;
                            }
                            else
                            {
                                tmp.Start.Set(Intermediate);
                                tmp.Circle.ReferenceToolface = DoubleArcCurve.DownstreamReferenceToolface;
                            }
                            if (tmp.CalculateSDT())
                            {
                                p.Set(tmp.End);
                            }
                            else
                            {
                                p.SetUndefined();
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// predicate to test if either the first or the second arc has zero length
        /// </summary>
        /// <returns></returns>
        public bool HasZeroLengthArc()
        {
            return (Start != null && Start.EQ(Intermediate)) || (End != null && End.EQ(Intermediate));
        }
        /// <summary>
        /// predicate to test if either the first or the second arc has zero length at the given accuracy
        /// </summary>
        /// <param name="acc"></param>
        /// <returns></returns>
        public bool HasZeroLengthArc(double acc)
        {
            return (Start != null && Start.EQ(Intermediate, acc)) || (End != null && End.EQ(Intermediate, acc));
        }

        /// <summary>
        ///  return the center of the first arc
        /// </summary>
        /// <returns></returns>
        public Point3D GetCenter1()
        {
            Point3D center = new Point3D();
            center.SetUndefined();
            GetCenter1(center);
            return center;
        }

        /// <summary>
        /// fill in the passed argument the coordinates of the center of the first arc
        /// </summary>
        /// <param name="center"></param>
        public void GetCenter1(IPoint3D center)
        {
            if (center != null)
            {
                if (DoubleArcCurve != null && 
                    Start != null &&
                    !Start.IsUndefined() &&
                    Start.Inclination != null &&
                    Start.Azimuth != null &&
                    Intermediate != null &&
                    DoubleArcCurve.Curvature != null &&
                    !Numeric.EQ(DoubleArcCurve.Curvature, 0))
                {
                    Vector3D t1 = Vector3D.CreateSpheric(1.0, (double)Start.Inclination, (double)Start.Azimuth);
                    Vector3D t2 = new Vector3D(Start, Intermediate);
                    Vector3D n = t1.CrossProduct(t2);
                    Vector3D tc = n.CrossProduct(t1);
                    Line3D l = new Line3D(Start, tc, true);
                    l.GetInterpolation(1.0 / (double)DoubleArcCurve.Curvature, center);
                }
                else
                {
                    center.SetUndefined();
                }
            }
        }

        /// <summary>
        /// return the center of the second arc
        /// </summary>
        /// <returns></returns>
        public Point3D GetCenter2()
        {
            Point3D center = new Point3D();
            center.SetUndefined();
            GetCenter2(center);
            return center;
        }

        /// <summary>
        /// fill in the passed argument the coordinates of the center of the second arc
        /// </summary>
        /// <param name="center"></param>
        public void GetCenter2(IPoint3D center)
        {
            if (center != null)
            {
                if (DoubleArcCurve != null &&
                    End != null &&
                    !End.IsUndefined() &&
                    End.Inclination != null &&
                    End.Azimuth != null &&
                    Intermediate != null &&
                    DoubleArcCurve.Curvature != null &&
                    !Numeric.EQ(DoubleArcCurve.Curvature, 0))
                {
                    Vector3D t1 = Vector3D.CreateSpheric(1.0, (double)Start.Inclination, (double)Start.Azimuth);
                    Vector3D t2 = new Vector3D(Intermediate, End);
                    Vector3D n = t1.CrossProduct(t2);
                    Vector3D tc = n.CrossProduct(t1);
                    Line3D l = new Line3D(Intermediate, tc, true);
                    l.GetInterpolation(1.0 / (double)DoubleArcCurve.Curvature, center);
                }
                else
                {
                    center.SetUndefined();
                }
            }
        }


    }
}
