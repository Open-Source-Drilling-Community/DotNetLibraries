using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using MathNet.Numerics.Integration;
using System.Diagnostics.Metrics;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    public class SurveyPoint : CurvilinearPoint3D
    {
        private double? latitude_ = null;
        private double? longitude_ = null;
        public static readonly double InterpolationDeltaAbscissa = 0.01;
        public static double CompleteCTCSDT1Step = 0.1;
        public static int CompleteCTCSDT2Count = 1000;

        /// <summary>
        /// synonym of Abscsissa
        /// </summary>
        public double? MD { get => base.Abscissa; set => base.Abscissa = value; }
        /// <summary>
        /// redefinition to add drilling properties
        /// </summary>
        public override double? Azimuth { get => base.Azimuth; set => base.Azimuth = value; }
        /// <summary>
        /// redefinition to add drilling properties
        /// </summary>
        public override double? Inclination { get => base.Inclination; set => base.Inclination = value; }
        /// <summary>
        /// The length of the arc on the earth (modelled as a WGS84 spheroid) from the equator to the latitude of this point. 
        /// Positive in the north direction.
        /// </summary>
        public override double? X
        {
            get => base.X;
            set
            {
                base.X = value;
                UpdateX(value);
            }
        }
        /// <summary>
        /// The length of the arc on the earth (modelled as a WGS84 spheroid) from the Greenwich meridian to the longitude of this point.
        /// Positive in the east direction.
        /// </summary>
        public override double? Y
        {
            get => base.Y;
            set
            {
                base.Y = value;
                UpdateY(value);
            }
        }
        /// <summary>
        /// synonym of Z
        /// </summary>
        public double? TVD { get => base.Z; set => base.Z = value; }
        /// <summary>
        /// Synonym of X. However, it is called Riemannian because the x-coordinate is defined in a Riemannian space
        /// of curvature corresponding to the Earth spheroid. The RiemannianNorth is the arc length from the equator
        /// to the latitude of the point.
        /// </summary>
        public double? RiemannianNorth
        {
            get => X; set => X = value;
        }
        /// <summary>
        /// Synonym of Y. However, it is called Riemannian because the y-coordinate is defined in a Riemannian space
        /// of curvature corresponding to the Earth spheroid. The RiemannianEast is the arc length from the Greenwich meridian
        /// to the longitude of the point following the parallel at that latitude.
        /// </summary>
        public double? RiemannianEast
        {
            get => Y; set => Y = value;
        }
        /// <summary>
        /// Latitude of the point on the WGS84 spheroid
        /// </summary>
        public double? Latitude
        {
            get { return latitude_; }
            set
            {
                latitude_ = value;
                UpdateLatitude(value);
            }
        }
        /// <summary>
        /// Longitude of the point on the WGS84 spheroid
        /// </summary>
        public double? Longitude
        {
            get { return longitude_; }
            set
            {
                longitude_ = value;
                UpdateLongitude(value);
            }
        }
        /// <summary>
        /// The local curvature at this Survey calculated using the minimum curvature method
        /// </summary>
        public double? Curvature { get; set; } = null;
        /// <summary>
        /// The local toolface at this Survey calculated using the equation from Sawaryn and Thorogood (2005) 
        /// https://doi.org/10.2118/84246-PA
        /// </summary>
        public double? Toolface { get; set; } = null;
        /// <summary>
        /// The local build up rate at this Survey calculated using a finite difference method.
        /// </summary>
        public double? BUR { get; set; } = null;
        /// <summary>
        /// The local turn rate at this Survey calculated using a finite difference method.
        /// </summary>
        public double? TUR { get; set; } = null;
        /// <summary>
        /// Default constructor
        /// </summary>
        public SurveyPoint() : base()
        {
        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public SurveyPoint(SurveyPoint src) : base(src)
        {
            if (src != null)
            {
                Abscissa = src.Abscissa;
                Inclination = src.Inclination;
                Azimuth = src.Azimuth;
            }
        }
        /// <summary>
        /// Apply the minimum curvature method to a list of survey points.
        /// The first survey point must be complete.
        /// The method, using generics, applies to SurveyList and SurveyStationList as well
        /// </summary>
        /// <param name="surveyList"></param>
        /// <returns></returns>
        public static bool CompleteSurvey<A>(List<A> surveyList) where A : SurveyPoint
        {
            if (surveyList != null &&
                surveyList.Count > 0 &&
                surveyList[0].X != null &&
                surveyList[0].Y != null &&
                surveyList[0].Z != null &&
                surveyList[0].Abscissa != null &&
                surveyList[0].Inclination != null &&
                surveyList[0].Azimuth != null)
            {
                A sp1 = surveyList[0];
                bool ok = true;
                for (int i = 1; i < surveyList.Count; i++)
                {
                    var sp2 = surveyList[i];
                    if (sp2 != null && sp2.Abscissa != null && sp2.Inclination != null && sp2.Azimuth != null)
                    {
                        ok = sp1.CompleteFromSIA(sp2);
                        sp1 = sp2;
                        if (!ok) break;
                    }
                    else if (sp2 != null && sp2.Abscissa != null && sp2.Inclination != null && sp2.Azimuth != null)
                    {
                        ok = sp1.CompleteFromXYZ(sp2);
                        sp1 = sp2;
                        if (!ok) break;
                    }
                }
                return ok;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// interpolate a Survey at a given abscissa. The abscissa must be between the first and last Survey of the SurveyList.
        /// </summary>
        /// <param name="MD"></param>
        /// <param name="interpolatedPoint"></param>
        /// <returns></returns>
        public static bool InterpolateAtAbscissa<A>(List<A> surveyList, double MD, ICurvilinear3D interpolatedPoint) where A : SurveyPoint
        {
            if (interpolatedPoint == null ||
                Numeric.IsUndefined(MD) ||
                surveyList.Count < 2 ||
                Numeric.LT(MD, surveyList.First<SurveyPoint>().MD) ||
                Numeric.GT(MD, surveyList.Last<SurveyPoint>().MD))
            {
                return false;
            }
            else
            {
                for (int i = 1; i < surveyList.Count; i++)
                {
                    if (Numeric.GE(MD, surveyList[i - 1]?.MD) && Numeric.LE(MD, surveyList[i]?.MD))
                    {
                        return surveyList[i - 1].InterpolateAtAbscissa(surveyList[i], MD, interpolatedPoint);
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// Return an interpolated SurveyList. The interpolation step is passed in argument. In addition
        /// interpolations are made at the abscissas given in a list. The interpolation uses the minimum curvature method.
        /// </summary>
        /// <param name="mdStep"></param>
        /// <param name="abscissaList"></param>
        /// <returns></returns>
        public static List<SurveyPoint>? Interpolate<A>(List<A> surveyList, double mdStep, List<double>? abscissaList = null) where A : SurveyPoint
        {

            if (surveyList is { Count: > 1 } &&
                Numeric.IsDefined(mdStep) &&
                Numeric.GT(mdStep, 0) &&
                surveyList[0].MD is { } md0 &&
                surveyList.Last<SurveyPoint>().MD is { } mdf)
            {
                List<double> abscissaFilteredList = [];
                if (abscissaList != null)
                {
                    foreach (double s in abscissaList)
                    {
                        if (Numeric.GE(s, md0) && Numeric.LE(s, mdf))
                        {
                            abscissaFilteredList.Add(s);
                        }
                    }
                }
                abscissaFilteredList.Sort();
                List<SurveyPoint> resultList = [surveyList[0]];
                for (double s = md0 + mdStep; Numeric.LE(s, mdf); s += mdStep)
                {
                    double lastS = double.MinValue;
                    if (abscissaFilteredList.Count > 0)
                    {
                        // interpolate at all filtered abscissa within one mdStep
                        while (abscissaFilteredList.Count > 0 && Numeric.LE(abscissaFilteredList.First<double>(), s))
                        {
                            SurveyPoint interpolatedSurveyPoint = new();
                            if (InterpolateAtAbscissa(surveyList, abscissaFilteredList.First<double>(), interpolatedSurveyPoint))
                            {
                                resultList.Add(interpolatedSurveyPoint);
                                lastS = abscissaFilteredList.First<double>();
                            }
                            abscissaFilteredList.RemoveAt(0);
                        }
                    }
                    // and finalize by interpolating at the current abscissa s
                    if (!Numeric.EQ(s, lastS))
                    {
                        SurveyPoint sp = new();
                        if (InterpolateAtAbscissa(surveyList, s, sp))
                        {
                            resultList.Add(sp);
                        }
                    }
                }
                return resultList;
            }
            return null;
        }
        /// <summary>
        /// complete the next survey depending on its unknown values
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteNext(CurvilinearPoint3D next)
        {
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null)
            {
                return false;
            }
            if (next.Abscissa != null && next.Inclination != null && next.Azimuth != null)
            {
                return CompleteFromSIA(next);
            }
            else if (next.X != null && next.Y != null && next.Z != null)
            {
                return CompleteFromXYZ(next);
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Apply the minimum curvature method between this survey and the next
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteFromSIA(CurvilinearPoint3D next)
        {
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null || next.Abscissa == null || next.Inclination == null || next.Azimuth == null)
            {
                return false;
            }
            double x1 = X.Value;
            double y1 = Y.Value;
            double z1 = Z.Value;
            double i1 = Inclination.Value;
            double a1 = Azimuth.Value;
            double s1 = Abscissa.Value;
            double s2 = next.Abscissa.Value;
            double i2 = next.Inclination.Value;
            double a2 = next.Azimuth.Value;
            double dm = s2 - s1;
            if (Numeric.EQ(dm, 0))
            {
                next.X = x1;
                next.Y = y1;
                next.Z = z1;
                next.Inclination = i1;
                next.Azimuth = a1;
                next.Abscissa = s2;
                return true;
            }
            double ci1 = System.Math.Cos(i1);
            double si1 = System.Math.Sin(i1);
            double ca1 = System.Math.Cos(a1);
            double sa1 = System.Math.Sin(a1);
            double ci2 = System.Math.Cos(i2);
            double si2 = System.Math.Sin(i2);
            double ca2 = System.Math.Cos(a2);
            double sa2 = System.Math.Sin(a2);
            double si12 = System.Math.Sin((i2 - i1) / 2.0);
            double sa12 = System.Math.Sin((a2 - a1) / 2.0);
            double dl = 2.0 * System.Math.Asin(System.Math.Sqrt(si12 * si12 + si1 * si2 * sa12 * sa12));
            double rf;
            if (Numeric.EQ(dl, 0, 0.02))
            {
                double dl2 = dl * dl;
                rf = 1 + (dl2 / 12.0) * (1.0 + (dl2 / 10.0) * (1 + (dl2 / 168.0) * (1.0 + 31 * dl2 / 18.0)));
            }
            else
            {
                rf = (2.0 / dl) * System.Math.Tan(dl / 2.0);
            }
            next.X = x1 + 0.5 * dm * rf * (si1 * ca1 + si2 * ca2);
            next.Y = y1 + 0.5 * dm * rf * (si1 * sa1 + si2 * sa2);
            next.Z = z1 + 0.5 * dm * rf * (ci1 + ci2);
            return true;
        }
        /// <summary>
        /// This method calculates next using the minimum curvature method but also calculates the curvature, toolface, build-up rate and turn-rate
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteFromSIA(SurveyPoint next)
        {
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null || next.Abscissa == null || next.Inclination == null || next.Azimuth == null)
            {
                return false;
            }
            double x1 = X.Value;
            double y1 = Y.Value;
            double z1 = Z.Value;
            double i1 = Inclination.Value;
            double a1 = Azimuth.Value;
            double s1 = Abscissa.Value;
            double s2 = next.Abscissa.Value;
            double i2 = next.Inclination.Value;
            double a2 = next.Azimuth.Value;
            double dm = s2 - s1;
            if (Numeric.EQ(dm, 0))
            {
                next.X = x1;
                next.Y = y1;
                next.Z = z1;
                next.Inclination = i1;
                next.Azimuth = a1;
                next.Abscissa = s2;
                return true;
            }
            double ci1 = System.Math.Cos(i1);
            double si1 = System.Math.Sin(i1);
            double ca1 = System.Math.Cos(a1);
            double sa1 = System.Math.Sin(a1);
            double ci2 = System.Math.Cos(i2);
            double si2 = System.Math.Sin(i2);
            double ca2 = System.Math.Cos(a2);
            double sa2 = System.Math.Sin(a2);
            double si12 = System.Math.Sin((i2 - i1) / 2.0);
            double sa12 = System.Math.Sin((a2 - a1) / 2.0);
            double dl = 2.0 * System.Math.Asin(System.Math.Sqrt(si12 * si12 + si1 * si2 * sa12 * sa12));
            double rf;
            if (Numeric.EQ(dl, 0, 0.02))
            {
                double dl2 = dl * dl;
                rf = 1 + (dl2 / 12.0) * (1.0 + (dl2 / 10.0) * (1 + (dl2 / 168.0) * (1.0 + 31 * dl2 / 18.0)));
            }
            else
            {
                rf = (2.0 / dl) * System.Math.Tan(dl / 2.0);
            }
            next.X = x1 + 0.5 * dm * rf * (si1 * ca1 + si2 * ca2);
            next.Y = y1 + 0.5 * dm * rf * (si1 * sa1 + si2 * sa2);
            next.Z = z1 + 0.5 * dm * rf * (ci1 + ci2);
            next.Curvature = dl / dm;
            CurvilinearPoint3D prev = new CurvilinearPoint3D();
            double ds = InterpolationDeltaAbscissa;
            if (InterpolateAtAbscissa(next, next.Abscissa.Value - ds, prev))
            {
                if (prev.Inclination != null && prev.Azimuth != null)
                {
                    si1 = System.Math.Sin(prev.Inclination.Value);
                    ci1 = System.Math.Cos(prev.Inclination.Value);
                    si2 = System.Math.Sin(next.Inclination.Value);
                    ci2 = System.Math.Cos(next.Inclination.Value);
                    sa12 = System.Math.Sin(next.Azimuth.Value - prev.Azimuth.Value);
                    double ca12 = System.Math.Cos(next.Azimuth.Value - prev.Azimuth.Value);
                    double denom = si2 * ci1 * ca12 - si1 * ci2;
                    next.Toolface = System.Math.Atan2(si2 * sa12, denom);

                    next.BUR = (next.Inclination - prev.Inclination) / ds;
                    if (next.Azimuth != null &&
                        prev.Azimuth != null &&
                        Numeric.LE(System.Math.Abs(next.Azimuth.Value - prev.Azimuth.Value), Numeric.PI))
                    {
                        next.TUR = (next.Azimuth - prev.Azimuth) / ds;
                    }
                    else
                    {
                        if (Numeric.GE(next.Azimuth - prev.Azimuth, 0))
                        {
                            next.TUR = (next.Azimuth - prev.Azimuth - 2.0 * Numeric.PI) / ds;
                        }
                        else
                        {
                            next.TUR = (next.Azimuth - prev.Azimuth + 2.0 * Numeric.PI) / ds;
                        }
                    }
                }
            }
            return true;
        }
        public bool CompleteFromXYZ(CurvilinearPoint3D next)
        {
            if (X != null &&
                Y != null &&
                Z != null &&
                next.X != null &&
                next.Y != null &&
                next.Z != null &&
                Inclination != null &&
                Azimuth != null)
            {
                double ci = System.Math.Cos(Inclination.Value);
                double si = System.Math.Sin(Inclination.Value);
                double ca = System.Math.Cos(Azimuth.Value);
                double sa = System.Math.Sin(Azimuth.Value);
                Vector3D v1 = Vector3D.CreateSpheric(1.0, Inclination, Azimuth);
                Vector3D v2 = new Vector3D(this, next);
                if (Numeric.EQ(Distance(next), 0.0) || v1.IsParallel(v2, 1e-4))
                {
                    next.Inclination = Inclination;
                    next.Azimuth = Azimuth;
                    next.Abscissa = Abscissa + Distance(next);
                    return true;
                }
                else
                {
                    double dls = 0;
                    double a;
                    Point3D p1 = TransCoord2PtsTg(next, next);
                    if (p1.Z != null && p1.Y != null && !Numeric.EQ(p1.Y.Value * p1.Y.Value + p1.Z.Value * p1.Z.Value, 0))
                    {
                        dls = 2.0 * p1.Y.Value / (p1.Y.Value * p1.Y.Value + p1.Z.Value * p1.Z.Value);
                        a = 2.0 * System.Math.Atan2(System.Math.Abs(p1.Y.Value), System.Math.Abs(p1.Z.Value));
                        if (Numeric.LE(p1.Z, 0))
                        {
                            a = 2.0 * Numeric.PI - (a - Numeric.PI * System.Math.Floor(a / Numeric.PI));
                        }
                        else
                        {
                            a -= Numeric.PI * System.Math.Floor(a / Numeric.PI);
                        }

                        Point3D p3 = new Point3D
                        {
                            X = 0.0,
                            Y = System.Math.Sin(a),
                            Z = System.Math.Cos(a)
                        };
                        Point3D? pt2 = TransCoord2PtsTgReversed(next, p3);
                        if (pt2 != null && pt2.X != null && pt2.Y != null && pt2.Z != null)
                        {
                            double c1 = pt2.Z.Value - Z.Value;
                            double b1 = pt2.Y.Value - Y.Value;
                            double a1 = pt2.X.Value - X.Value;
                            double? l;
                            if (Numeric.EQ(dls, 0))
                            {
                                l = Distance(next);
                            }
                            else
                            {
                                l = a / dls;
                            }
                            next.Abscissa = Abscissa + l;
                            next.Inclination = Numeric.AcosEqual(c1);
                            if (Numeric.EQ(a1, 0))
                            {
                                if (Numeric.GE(b1, 0))
                                {
                                    next.Azimuth = Numeric.PI / 2.0;
                                }
                                else
                                {
                                    next.Azimuth = 3.0 * Numeric.PI / 2.0;
                                }
                            }
                            else
                            {
                                next.Azimuth = System.Math.Atan(b1 / a1);
                            }
                            if (Numeric.LT(a1, 0))
                            {
                                next.Azimuth = next.Azimuth + Numeric.PI;
                            }
                            if (next.Azimuth < 0)
                            {
                                next.Azimuth = next.Azimuth + 2.0 * Numeric.PI;
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
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteFromXYZ(SurveyPoint next)
        {
            if (X != null &&
                Y != null &&
                Z != null &&
                next.X != null &&
                next.Y != null &&
                next.Z != null &&
                Inclination != null &&
                Azimuth != null)
            {
                double ci = System.Math.Cos(Inclination.Value);
                double si = System.Math.Sin(Inclination.Value);
                double ca = System.Math.Cos(Azimuth.Value);
                double sa = System.Math.Sin(Azimuth.Value);
                Vector3D v1 = Vector3D.CreateSpheric(1.0, Inclination, Azimuth);
                Vector3D v2 = new Vector3D(this, next);
                if (Numeric.EQ(Distance(next), 0.0) || v1.IsParallel(v2, 1e-4))
                {
                    next.Curvature = 0.0;
                    next.BUR = 0.0;
                    next.TUR = 0.0;
                    next.Toolface = 0.0;
                    next.Inclination = Inclination;
                    next.Azimuth = Azimuth;
                    next.Abscissa = Abscissa + Distance(next);
                    return true;
                }
                else
                {
                    double dls = 0;
                    double a;
                    Point3D p1 = TransCoord2PtsTg(next, next);
                    if (p1.Z != null && p1.Y != null && !Numeric.EQ(p1.Y.Value * p1.Y.Value + p1.Z.Value * p1.Z.Value, 0))
                    {
                        dls = 2.0 * p1.Y.Value / (p1.Y.Value * p1.Y.Value + p1.Z.Value * p1.Z.Value);
                        a = 2.0 * System.Math.Atan2(System.Math.Abs(p1.Y.Value), System.Math.Abs(p1.Z.Value));
                        if (Numeric.LE(p1.Z, 0))
                        {
                            a = 2.0 * Numeric.PI - (a - Numeric.PI * System.Math.Floor(a / Numeric.PI));
                        }
                        else
                        {
                            a -= Numeric.PI * System.Math.Floor(a / Numeric.PI);
                        }

                        Point3D p3 = new Point3D
                        {
                            X = 0.0,
                            Y = System.Math.Sin(a),
                            Z = System.Math.Cos(a)
                        };
                        Point3D? pt2 = TransCoord2PtsTgReversed(next, p3);
                        if (pt2 != null && pt2.X != null && pt2.Y != null && pt2.Z != null)
                        {
                            double c1 = pt2.Z.Value - Z.Value;
                            double b1 = pt2.Y.Value - Y.Value;
                            double a1 = pt2.X.Value - X.Value;
                            double? l;
                            if (Numeric.EQ(dls, 0))
                            {
                                l = Distance(next);
                            }
                            else
                            {
                                l = a / dls;
                            }
                            next.Abscissa = Abscissa + l;
                            next.Inclination = Numeric.AcosEqual(c1);
                            if (Numeric.EQ(a1, 0))
                            {
                                if (Numeric.GE(b1, 0))
                                {
                                    next.Azimuth = Numeric.PI / 2.0;
                                }
                                else
                                {
                                    next.Azimuth = 3.0 * Numeric.PI / 2.0;
                                }
                            }
                            else
                            {
                                next.Azimuth = System.Math.Atan(b1 / a1);
                            }
                            if (Numeric.LT(a1, 0))
                            {
                                next.Azimuth = next.Azimuth + Numeric.PI;
                            }
                            if (next.Azimuth < 0)
                            {
                                next.Azimuth = next.Azimuth + 2.0 * Numeric.PI;
                            }
                            next.Curvature = dls;
                            CurvilinearPoint3D prev = new CurvilinearPoint3D();
                            double ds = InterpolationDeltaAbscissa;
                            if (InterpolateAtAbscissa(next, next.Abscissa.Value - ds, prev))
                            {
                                if (prev.Inclination != null && prev.Azimuth != null)
                                {
                                    double si1 = System.Math.Sin(prev.Inclination.Value);
                                    double ci1 = System.Math.Cos(prev.Inclination.Value);
                                    double si2 = System.Math.Sin(next.Inclination.Value);
                                    double ci2 = System.Math.Cos(next.Inclination.Value);
                                    double sa12 = System.Math.Sin(next.Azimuth.Value - prev.Azimuth.Value);
                                    double ca12 = System.Math.Cos(next.Azimuth.Value - prev.Azimuth.Value);
                                    double denom = si2 * ci1 * ca12 - si1 * ci2;
                                    next.Toolface = System.Math.Atan2(si2 * sa12, denom);

                                    next.BUR = (next.Inclination - prev.Inclination) / ds;
                                    if (next.Azimuth != null &&
                                        prev.Azimuth != null &&
                                        Numeric.LE(System.Math.Abs(next.Azimuth.Value - prev.Azimuth.Value), Numeric.PI))
                                    {
                                        next.TUR = (next.Azimuth - prev.Azimuth) / ds;
                                    }
                                    else
                                    {
                                        if (Numeric.GE(next.Azimuth - prev.Azimuth, 0))
                                        {
                                            next.TUR = (next.Azimuth - prev.Azimuth - 2.0 * Numeric.PI) / ds;
                                        }
                                        else
                                        {
                                            next.TUR = (next.Azimuth - prev.Azimuth + 2.0 * Numeric.PI) / ds;
                                        }
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
            }
            else
            {
                return false;
            }
        }
        public bool CompleteCASDT(CurvilinearPoint3D? next, double dls, double TF)
        {
            if ((next == null) ||
                IsUndefined() ||
                Inclination == null ||
                Azimuth == null ||
                Abscissa == null ||
                next.Abscissa == null ||
                Numeric.IsUndefined(next.Abscissa))
            {
                return false;
            }
            else if (Numeric.EQ(dls, 0.0))
            {
                CurvilinearPoint3D f = next;
                double dm = f.Abscissa.Value - Abscissa.Value;
                double si = Math.Sin(Inclination.Value);
                f.Inclination = Inclination;
                f.Azimuth = Azimuth;
                f.X = X + dm * si * Math.Cos(Azimuth.Value);
                f.Y = Y + dm * si * Math.Sin(Azimuth.Value);
                f.Z = Z + dm * Math.Cos(Inclination.Value);
                return true;
            }
            else if (Numeric.EQ(Inclination, 0.0))
            {
                CurvilinearPoint3D f = next;
                double dm = f.Abscissa.Value - Abscissa.Value;
                f.Inclination = dm * dls;
                f.Azimuth = TF;
                f.X = X + Math.Cos(f.Azimuth.Value) * (1 - Math.Cos(f.Inclination.Value)) / dls;
                f.Y = Y + Math.Sin(f.Azimuth.Value) * (1 - Math.Cos(f.Inclination.Value)) / dls;
                f.Z = Z + Math.Sin(f.Inclination.Value) / dls;
                return true;
            }
            else
            {
                CurvilinearPoint3D p1 = new();
                CurvilinearPoint3D p2 = new();
                p1.X = 0;
                p1.Y = 0;
                p1.Z = 0;
                p2.X = 0;
                p2.Y = 0;
                p2.Z = 0;
                CurvilinearPoint3D f = next;
                double dm = f.Abscissa.Value - Abscissa.Value;
                double teta = dm * dls;
                double st = Math.Sin(teta);
                double ct = Math.Cos(teta);
                p1.X = (1 - ct) / dls;
                p1.Y = 0.0;
                p1.Z = st / dls;
                Point3D r = TransCoord3RotsReversed(TF, p1);
                f.X = X + r.X;
                f.Y = Y + r.Y;
                f.Z = Z + r.Z;
                p1.X = st;
                p1.Y = 0.0;
                p1.Z = ct;
                r = TransCoord3RotsReversed(TF, p1);
                f.Inclination = Numeric.AcosEqual(r.Z);
                if (Numeric.EQ(r.Z, 1.0))
                {
                    f.Azimuth = Azimuth;
                }
                else
                {
                    if (r.X == null || r.Y == null || (Numeric.EQ(r.X, 0.0) && Numeric.EQ(r.Y, 0.0)))
                    {
                        f.Azimuth = null;
                    }
                    else
                    {
                        double teta2 = Numeric.AcosEqual(r.X.Value / Math.Sqrt(r.X.Value * r.X.Value + r.Y.Value * r.Y.Value));
                        if (r.Y >= 0.0)
                        {
                            f.Azimuth = teta2;
                        }
                        else
                        {
                            f.Azimuth = 2.0 * Math.PI - teta2;
                        }
                    }
                }
                return true;
            }
        }
        public bool CompleteCDTSDT1(SurveyPoint? next, double DLS, double TF, List<SurveyPoint>? inters = null)
        {
            if ((next == null) ||
                IsUndefined() ||
                Inclination == null ||
                Azimuth == null ||
                Abscissa == null ||
                next.Abscissa == null ||
                Numeric.IsUndefined(next.Abscissa))
            {
                return false;
            }
            if (Numeric.EQ(DLS, 0))
            {
                return CompleteCASDT(next, DLS, TF);
            }
            if (Numeric.EQ(Inclination, 0))
            {
                return CompleteCASDT(next, DLS, TF);
            }
            if (Numeric.EQ(Inclination, Math.PI / 2.0))
            {
                return CompleteCASDT(next, DLS, TF);
            }

            SurveyPoint sv2 = new SurveyPoint(this);
            SurveyPoint sv1 = new SurveyPoint();
            do
            {
                SurveyPoint tmp = sv1;
                sv1 = sv2;
                sv2 = tmp;
                sv2.Abscissa = Math.Min(next.Abscissa.Value, sv1.Abscissa.Value + CompleteCTCSDT1Step);
                bool ok = sv1.CompleteCASDT(sv2, DLS, TF);
                if (inters != null)
                {
                    object inter = sv2.MemberwiseClone();
                    if (inter is SurveyPoint)
                    {
                        inters.Add((SurveyPoint)inter);
                    }
                }
            } while (!Numeric.EQ(sv2.Abscissa, next.Abscissa));
            next.Inclination = sv2.Inclination;
            next.Azimuth = sv2.Azimuth;
            next.X = sv2.X;
            next.Y = sv2.Y;
            next.Z = sv2.Z;
            SurveyPoint prev = null;
            if (inters == null)
            {
                prev = sv1;
            }
            else
            {
                for (int i = inters.Count - 1; i >= 0; i--)
                {
                    if ((next.Abscissa.Value - inters[i].Abscissa.Value) >= InterpolationDeltaAbscissa)
                    {
                        prev = inters[i];
                    }
                }
            }
            double ds = sv2.Abscissa.Value - prev.Abscissa.Value;
            if (prev.Inclination != null && prev.Azimuth != null && sv2.Inclination != null && sv2.Azimuth != null && !Numeric.EQ(ds, 0))
            {
                double si1 = System.Math.Sin(prev.Inclination.Value);
                double ci1 = System.Math.Cos(prev.Inclination.Value);
                double si2 = System.Math.Sin(sv2.Inclination.Value);
                double ci2 = System.Math.Cos(sv2.Inclination.Value);
                double sa12 = System.Math.Sin(sv2.Azimuth.Value - prev.Azimuth.Value);
                double ca12 = System.Math.Cos(sv2.Azimuth.Value - prev.Azimuth.Value);
                double denom = si2 * ci1 * ca12 - si1 * ci2;
                next.Toolface = System.Math.Atan2(si2 * sa12, denom);

                next.BUR = (sv2.Inclination - prev.Inclination) / ds;
                if (sv2.Azimuth != null &&
                    prev.Azimuth != null &&
                    Numeric.LE(System.Math.Abs(sv2.Azimuth.Value - prev.Azimuth.Value), Numeric.PI))
                {
                    next.TUR = (sv2.Azimuth - prev.Azimuth) / ds;
                }
                else
                {
                    if (Numeric.GE(sv2.Azimuth - prev.Azimuth, 0))
                    {
                        next.TUR = (sv2.Azimuth - prev.Azimuth - 2.0 * Numeric.PI) / ds;
                    }
                    else
                    {
                        next.TUR = (sv2.Azimuth - prev.Azimuth + 2.0 * Numeric.PI) / ds;
                    }
                }
            }
            return true;
        }
        public bool CompleteCDTSDT2(SurveyPoint? next, double DLS, double TF)
        {
            if ((next == null) ||
                IsUndefined() ||
                Z == null ||
                Inclination == null ||
                Azimuth == null ||
                Abscissa == null ||
                next.Abscissa == null ||
                Numeric.IsUndefined(next.Abscissa))
            {
                return false;
            }
            if (Numeric.EQ(DLS, 0))
            {
                return CompleteCASDT(next, DLS, TF);
            }
            if (Numeric.EQ(Inclination, 0))
            {
                return CompleteCASDT(next, DLS, TF);
            }
            if (Numeric.EQ(Inclination, Math.PI / 2.0))
            {
                return CompleteCASDT(next, DLS, TF);
            }
            // case where the BUR is 0
            if (Numeric.EQ(TF, Math.PI / 2.0) || Numeric.EQ(TF, 3.0 * Math.PI / 2.0))
            {
                return CompleteCASDT(next, DLS, TF);
            }
            double beta = DLS * Math.Cos(TF);
            double l = next.Abscissa.Value - Abscissa.Value;
            double z = Z.Value + (1.0 / beta) * (Math.Sin(beta * l + Inclination.Value) - Math.Sin(Inclination.Value));
            next.Z = z;
            double A = Math.Sqrt(DLS * DLS - beta * beta) / beta;
            double C = Azimuth.Value - A * Math.Log(Math.Abs(Math.Tan(0.5 * Inclination.Value)));
            Func<double, double> fx = s => Math.Sin(beta * s + Inclination.Value) * Math.Cos(A * Math.Log(Math.Abs(Math.Tan(0.5 * (beta * s + Inclination.Value)))) + C);
            Func<double, double> fy = s => Math.Sin(beta * s + Inclination.Value) * Math.Sin(A * Math.Log(Math.Abs(Math.Tan(0.5 * (beta * s + Inclination.Value)))) + C);
            next.X = SimpsonRule.IntegrateComposite(fx, 0, next.Abscissa.Value - Abscissa.Value, CompleteCTCSDT2Count);
            next.Y = SimpsonRule.IntegrateComposite(fy, 0, next.Abscissa.Value - Abscissa.Value, CompleteCTCSDT2Count);
            next.Inclination = Inclination.Value + beta * l;
            next.Curvature = DLS;
            next.Toolface = TF;
            next.BUR = beta;
            next.TUR = Math.Sqrt(DLS * DLS - beta * beta) / Math.Sin(next.Inclination.Value);
            double r = Math.Sqrt(DLS * DLS - beta * beta) / beta;
            double alpha1 = Azimuth.Value + r * Math.Log(Math.Abs(Math.Tan(0.5 * next.Inclination.Value))) - r * Math.Log(Math.Tan(0.5 * Inclination.Value));
            double alpha2 = Azimuth.Value - r * Math.Log(Math.Abs(Math.Tan(0.5 * next.Inclination.Value))) + r * Math.Log(Math.Tan(0.5 * Inclination.Value));
            next.Azimuth = alpha1;

            return true;
        }
        /// <summary>
        /// Calculate the coordinates of the result survey station by interpolation between this survey station and the next survey station, for a given curvilinear abscissa
        /// </summary>
        /// <param name="next"></param>
        /// <param name="abscissa"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool InterpolateAtAbscissa(ICurvilinear3D next, double abscissa, ICurvilinear3D result)
        {
            if (next == null || result == null ||
                X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null ||
                next.X == null || next.Y == null || next.Z == null || next.Inclination == null || next.Azimuth == null || next.Abscissa == null)
            {
                return false;
            }
            if (!Numeric.IsBetween(abscissa, (double)Abscissa, (double)next.Abscissa))
            {
                return false;
            }
            result.Abscissa = abscissa;
            double x1 = (double)X;
            double y1 = (double)Y;
            double z1 = (double)Z;
            double i1 = (double)Inclination;
            double a1 = (double)Azimuth;
            double s1 = (double)Abscissa;
            double x2 = (double)next.X;
            double y2 = (double)next.Y;
            double z2 = (double)next.Z;
            double i2 = (double)next.Inclination;
            double a2 = (double)next.Azimuth;
            double s2 = (double)next.Abscissa;
            double si1 = System.Math.Sin(i1);
            double si2 = System.Math.Sin(i2);
            double ci12 = System.Math.Cos(i2 - i1);
            double ca12 = System.Math.Cos(a2 - a1);
            double DL = System.Math.Acos(ci12 - (1 - ca12) * si2 * si1);
            double DM = s2 - s1;
            if (Numeric.EQ(DM, 0))
            {
                if (Numeric.EQ(s1, abscissa))
                {
                    result.X = X;
                    result.Y = Y;
                    result.Z = Z;
                    result.Abscissa = Abscissa;
                    result.Inclination = Inclination;
                    result.Azimuth = Azimuth;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                double dx = x2 - x1;
                double dy = y2 - y1;
                double ci1 = System.Math.Cos(i1);
                double ca1 = System.Math.Cos(a1);
                double sa1 = System.Math.Sin(a1);
                double ci2 = System.Math.Cos(i2);
                double numerator = si2 * System.Math.Sin(a2 - a1);
                double denominator = si2 * ci1 * System.Math.Cos(a2 - a1) - si1 * ci2;
                double tf = System.Math.Atan2(numerator, denominator);
                double dls = DL / DM;
                double dm = abscissa - s1;
                if (Numeric.EQ(dls, 0))
                {
                    result.Inclination = i1;
                    result.Azimuth = a1;
                    result.X = x1 + dm * ca1 * si1;
                    result.Y = y1 + dm * sa1 * si1;
                    result.Z = z1 + dm * ci1;
                    return true;
                }
                else
                {
                    if (Numeric.EQ(i1, 0))
                    {
                        double incl = dm * dls;
                        result.Inclination = incl;
                        double az = System.Math.Atan2(dy, dx);
                        result.Azimuth = az;
                        double ci = System.Math.Cos(incl);
                        result.X = x1 + System.Math.Cos(az) * (1 - ci) / dls;
                        result.Y = y1 + System.Math.Sin(az) * (1 - ci) / dls;
                        result.Z = z1 + System.Math.Sin(incl) / dls;
                        return true;
                    }
                    else
                    {
                        if (Numeric.IsUndefined(tf))
                        {
                            return false;
                        }
                        double theta = dm * dls;
                        double deltaXp = (1 - System.Math.Cos(theta)) / dls;
                        double deltaZp = System.Math.Sin(theta) / dls;
                        double ctf = System.Math.Cos(tf);
                        double stf = System.Math.Sin(tf);
                        result.X = x1 + deltaXp * (ctf * ca1 * ci1 - sa1 * stf) + deltaZp * si1 * ca1;
                        result.Y = y1 + deltaXp * (stf * ca1 + ctf * sa1 * ci1) + deltaZp * si1 * sa1;
                        result.Z = z1 - deltaXp * ctf * si1 + deltaZp * ci1;
                        double deltaXt = System.Math.Sin(theta);
                        double deltaZt = System.Math.Cos(theta);
                        double xt = deltaXt * (ctf * ca1 * ci1 - sa1 * stf) + deltaZt * si1 * ca1;
                        double yt = deltaXt * (stf * ca1 + ctf * sa1 * ci1) + deltaZt * si1 * sa1;
                        double zt = -deltaXt * ctf * si1 + deltaZt * ci1;
                        result.Inclination = System.Math.Acos(zt);
                        if (Numeric.EQ(z1, 1))
                        {
                            result.Azimuth = a1;
                        }
                        else
                        {
                            double omega = System.Math.Acos(xt / System.Math.Sqrt(xt * xt + yt * yt));
                            if (Numeric.GE(yt, 0))
                            {
                                result.Azimuth = omega;
                            }
                            else
                            {
                                result.Azimuth = 2 * System.Math.PI - omega;
                            }
                        }
                        return true;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="s"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool InterpolateAtAbscissa(ICurvilinear3D next, double s, SurveyPoint result)
        {
            if (next == null || result == null ||
                X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null ||
                next.X == null || next.Y == null || next.Z == null || next.Inclination == null || next.Azimuth == null || next.Abscissa == null)
            {
                return false;
            }
            double x1, y1, z1, i1, a1, s1, x2, y2, z2, i2, a2, s2, si1, si2, ci12, ca12, DL, DM;
            if (!Numeric.IsBetween(s, (double)Abscissa, (double)next.Abscissa))
            {
                return false;
            }
            result.Abscissa = s;
            x1 = (double)X;
            y1 = (double)Y;
            z1 = (double)Z;
            i1 = (double)Inclination;
            a1 = (double)Azimuth;
            s1 = (double)Abscissa;
            x2 = (double)next.X;
            y2 = (double)next.Y;
            z2 = (double)next.Z;
            i2 = (double)next.Inclination;
            a2 = (double)next.Azimuth;
            s2 = (double)next.Abscissa;
            si1 = System.Math.Sin(i1);
            si2 = System.Math.Sin(i2);
            ci12 = System.Math.Cos(i2 - i1);
            ca12 = System.Math.Cos(a2 - a1);
            DL = System.Math.Acos(ci12 - (1 - ca12) * si2 * si1);
            DM = s2 - s1;
            if (Numeric.EQ(DM, 0))
            {
                if (Numeric.EQ(s1, s))
                {
                    result.X = X;
                    result.Y = Y;
                    result.Z = Z;
                    result.Abscissa = Abscissa;
                    result.Inclination = Inclination;
                    result.Azimuth = Azimuth;
                    result.Curvature = Curvature;
                    result.Toolface = Toolface;
                    result.BUR = BUR;
                    result.TUR = TUR;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                double dx = x2 - x1;
                double dy = y2 - y1;
                double ci1 = System.Math.Cos(i1);
                double ca1 = System.Math.Cos(a1);
                double sa1 = System.Math.Sin(a1);
                double ci2 = System.Math.Cos(i2);
                double numerator = si2 * System.Math.Sin(a2 - a1);
                double denominator = si2 * ci1 * System.Math.Cos(a2 - a1) - si1 * ci2;
                double tf = System.Math.Atan2(numerator, denominator);
                double dls = DL / DM;
                double dm = s - s1;
                result.Curvature = dls;
                if (Numeric.EQ(dls, 0))
                {
                    result.Inclination = i1;
                    result.Azimuth = a1;
                    result.X = x1 + dm * ca1 * si1;
                    result.Y = y1 + dm * sa1 * si1;
                    result.Z = z1 + dm * ci1;
                    result.Toolface = tf;
                    result.BUR = 0;
                    result.TUR = 0;
                    return true;
                }
                else
                {
                    if (Numeric.EQ(i1, 0))
                    {
                        double incl = dm * dls;
                        result.Inclination = incl;
                        double az = System.Math.Atan2(dy, dx);
                        result.Azimuth = az;
                        double ci = System.Math.Cos(incl);
                        result.X = x1 + System.Math.Cos(az) * (1 - ci) / dls;
                        result.Y = y1 + System.Math.Sin(az) * (1 - ci) / dls;
                        result.Z = z1 + System.Math.Sin(incl) / dls;
                        result.Toolface = az;
                        result.BUR = dls;
                        result.TUR = 0;
                        return true;
                    }
                    else
                    {
                        if (Numeric.IsUndefined(tf))
                        {
                            return false;
                        }
                        double theta = dm * dls;
                        double deltaXp = (1 - System.Math.Cos(theta)) / dls;
                        double deltaZp = System.Math.Sin(theta) / dls;
                        double ctf = System.Math.Cos(tf);
                        double stf = System.Math.Sin(tf);
                        result.X = x1 + deltaXp * (ctf * ca1 * ci1 - sa1 * stf) + deltaZp * si1 * ca1;
                        result.Y = y1 + deltaXp * (stf * ca1 + ctf * sa1 * ci1) + deltaZp * si1 * sa1;
                        result.Z = z1 - deltaXp * ctf * si1 + deltaZp * ci1;
                        double deltaXt = System.Math.Sin(theta);
                        double deltaZt = System.Math.Cos(theta);
                        double xt = deltaXt * (ctf * ca1 * ci1 - sa1 * stf) + deltaZt * si1 * ca1;
                        double yt = deltaXt * (stf * ca1 + ctf * sa1 * ci1) + deltaZt * si1 * sa1;
                        double zt = -deltaXt * ctf * si1 + deltaZt * ci1;
                        result.Inclination = System.Math.Acos(zt);
                        if (Numeric.EQ(z1, 1))
                        {
                            result.Azimuth = a1;
                        }
                        else
                        {
                            double omega = System.Math.Acos(xt / System.Math.Sqrt(xt * xt + yt * yt));
                            if (Numeric.GE(yt, 0))
                            {
                                result.Azimuth = omega;
                            }
                            else
                            {
                                result.Azimuth = 2 * System.Math.PI - omega;
                            }
                        }
                        CurvilinearPoint3D prev = new CurvilinearPoint3D();
                        double ds = Math.Min(DM / 2.0, InterpolationDeltaAbscissa);
                        if (s - ds < 0)
                        {
                            ds = -ds;
                        }
                        if (InterpolateAtAbscissa(next, s - ds, prev))
                        {
                            if (prev.Inclination != null && prev.Azimuth != null)
                            {
                                CurvilinearPoint3D sv2 = result;
                                CurvilinearPoint3D sv1 = prev;
                                if (ds < 0)
                                {
                                    sv2 = prev;
                                    sv1 = result;
                                    ds = -ds;
                                }
                                si1 = System.Math.Sin(sv1.Inclination.Value);
                                ci1 = System.Math.Cos(sv1.Inclination.Value);
                                si2 = System.Math.Sin(sv2.Inclination.Value);
                                ci2 = System.Math.Cos(sv2.Inclination.Value);
                                double sa12 = System.Math.Sin(sv2.Azimuth.Value - sv1.Azimuth.Value);
                                ca12 = System.Math.Cos(sv2.Azimuth.Value - sv1.Azimuth.Value);
                                double denom = si2 * ci1 * ca12 - si1 * ci2;
                                result.Toolface = System.Math.Atan2(si2 * sa12, denom);

                                result.BUR = (sv2.Inclination - sv1.Inclination) / ds;
                                if (sv2.Azimuth != null &&
                                    sv1.Azimuth != null &&
                                    Numeric.LE(System.Math.Abs(sv2.Azimuth.Value - sv1.Azimuth.Value), Numeric.PI))
                                {
                                    result.TUR = (sv2.Azimuth - sv1.Azimuth) / ds;
                                }
                                else
                                {
                                    if (Numeric.GE(sv2.Azimuth - sv1.Azimuth, 0))
                                    {
                                        result.TUR = (sv2.Azimuth - sv1.Azimuth - 2.0 * Numeric.PI) / ds;
                                    }
                                    else
                                    {
                                        result.TUR = (sv2.Azimuth - sv1.Azimuth + 2.0 * Numeric.PI) / ds;
                                    }
                                }
                            }
                        }
                        return true;
                    }
                }
            }
        }
        private void UpdateX(double? value)
        {
            if (value != null && Y != null)
            {
                SetLatitudeLongitude((double)value, (double)Y);
            }
        }
        private void UpdateY(double? value)
        {
            if (value != null && X != null)
            {
                SetLatitudeLongitude((double)X, (double)value);

            }
        }
        private void UpdateLatitude(double? value)
        {
            if (value != null && longitude_ != null)
            {
                SetRiemannianNorthEast((double)value, (double)longitude_);
            }
        }
        private void UpdateLongitude(double? value)
        {
            if (value != null && latitude_ != null)
            {
                SetRiemannianNorthEast((double)latitude_, (double)value);
            }
        }
        public double? Riemannian2DDistance(double? latitude2, double? longitude2)
        {
            if (TVD == null || Latitude == null || Longitude == null || latitude2 == null || longitude2 == null)
            {
                return null;
            }
            double z = TVD.Value;
            double lat1 = Latitude.Value;
            double lon1 = Longitude.Value;
            double lat2 = latitude2.Value;
            double lon2 = longitude2.Value;
            double a = Constants.EarthSemiMajorAxisWGS84;
            double b = a - a / Constants.EarthInverseFlateningWGS84;
            double a2 = a * a;
            double b2 = b * b;
            double e2 = (a2 - b2) / a2;
            double ap = a - z * System.Math.Sqrt(1 - e2 * System.Math.Sin(lat1));
            double bp = ap * System.Math.Sqrt(1 - e2);
            double ap2 = ap * ap;
            double bp2 = bp * bp;
            double fp = (ap - bp) / ap;
            double L = lon2 - lon1;
            double lambda = L;
            double U1 = System.Math.Atan((1 - fp) * System.Math.Tan(lat1));
            double U2 = System.Math.Atan((1 - fp) * System.Math.Tan(lat2));
            double cosU1 = System.Math.Cos(U1);
            double sinU1 = System.Math.Sin(U1);
            double cosU2 = System.Math.Cos(U2);
            double sinU2 = System.Math.Sin(U2);
            double cosAlpha2;
            double cosSigma;
            double sinLambda;
            double cos2sigmam;
            double sinSigma;
            double sigma;
            double lambdaPrev;
            int count = 0;
            do
            {
                lambdaPrev = lambda;
                double cosLambda = System.Math.Cos(lambdaPrev);
                sinLambda = System.Math.Sin(lambdaPrev);
                double term1 = cosU2 * sinLambda;
                double term2 = cosU1 * sinU2 - sinU1 * cosU2 * cosLambda;
                sinSigma = System.Math.Sqrt(term1 * term1 + term2 * term2);
                cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                sigma = System.Math.Atan2(sinSigma, cosSigma);
                if (Numeric.EQ(sinSigma, 0))
                {
                    return 0;
                }
                double sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
                double sinAlpha2 = sinAlpha * sinAlpha;
                cosAlpha2 = 1 - sinAlpha2;
                if (Numeric.EQ(cosAlpha2, 0))
                {
                    cos2sigmam = cosSigma;
                }
                else
                {
                    cos2sigmam = cosSigma - 2.0 * sinU1 * sinU2 / cosAlpha2;
                }
                double C = (fp / 16.0) * cosAlpha2 * (4.0 + fp * (4.0 - 3.0 * cosAlpha2));
                lambda = L + (1 - C) * fp * sinAlpha * (sigma + C * sinSigma * (cos2sigmam + C * cosSigma * (-1.0 + 2.0 * cos2sigmam * cos2sigmam)));
            } while (System.Math.Abs(lambda - lambdaPrev) > 1e-12 && count++ < 20);
            if (count >= 20)
            {

            }
            double u2 = cosAlpha2 * (ap2 - bp2) / bp2;
            double A = 1.0 + (u2 / 16384.0) * (4096.0 + u2 * (-768.0 + u2 * (320.0 - 175 * u2)));
            double B = (u2 / 1024) * (256.0 + u2 * (-128.0 + u2 * (74.0 - 47 * u2)));
            double deltaSigma = B * sinSigma * (cos2sigmam + (1.0 / 4.0) * B * (cosSigma * (-1 + 2.0 * cos2sigmam * cos2sigmam) - (B / 6.0) * cos2sigmam * (-3.0 + 4.0 * sinSigma * sinSigma) * (-3.0 + 4.0 * cos2sigmam * cos2sigmam)));
            return b * A * (sigma - deltaSigma);
        }
        /// <summary>
        /// Find the minimum MD-delta between two survey points
        /// The method, using generics, applies to SurveyList and SurveyStationList as well
        /// </summary>
        public static double? MinimumMDBetweenSurveyPoints<A>(List<A> list) where A : SurveyPoint
        {
            double? minDeltaMD = null;
            if (list != null && list.Count > 1)
            {
                for (int i = 0; i < list.Count - 1; i++)
                {
                    var deltaMD = list[i + 1].MD - list[i].MD;
                    if (Numeric.IsDefined(deltaMD) && (minDeltaMD == null || Numeric.LT(deltaMD, minDeltaMD)))
                    {
                        minDeltaMD = deltaMD;
                    }
                }
            }
            return minDeltaMD;
        }
        /// <summary>
        /// Find the maximum MD-delta between two survey's
        /// The method, using generics, applies to SurveyList and SurveyStationList as well
        /// </summary>
        public static double? MaximumMDBetweenSurveyPoints<A>(List<A> list) where A : SurveyPoint
        {
            double? maxDeltaMD = null;
            if (list != null && list.Count > 1)
            {
                for (int i = 0; i < list.Count - 1; i++)
                {
                    var deltaMD = list[i + 1].MD - list[i].MD;
                    if (Numeric.IsDefined(deltaMD) && (maxDeltaMD == null || Numeric.GT(deltaMD, maxDeltaMD)))
                    {
                        maxDeltaMD = deltaMD;
                    }
                }
            }
            return maxDeltaMD;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="riemannianNorth"></param>
        /// <param name="riemannianEast"></param>
        public void SetLatitudeLongitude(double riemannianNorth, double riemannianEast)
        {
            double f = 1.0 / Constants.EarthInverseFlateningWGS84;
            double a = Constants.EarthSemiMajorAxisWGS84;
            double b = a * (1.0 - f);
            double b2 = b * b;
            double a2 = a * a;
            double e2 = (a2 - b2) / a2;
            double e = System.Math.Sqrt(e2);
            double latitude = SpecialFunctions.InverseEllipseE(riemannianNorth / a, e2);
            Latitude = latitude;
            double sinLat = System.Math.Sin(latitude);
            double cosLat = System.Math.Cos(latitude);
            double R = a * cosLat / System.Math.Sqrt(1 - e2 * sinLat * sinLat);
            if (Numeric.EQ(R, 0))
            {
                Longitude = null;
            }
            else
            {
                Longitude = riemannianEast / R;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void SetRiemannianNorthEast(double latitude, double longitude)
        {
            double f = 1.0 / Constants.EarthInverseFlateningWGS84;
            double a = Constants.EarthSemiMajorAxisWGS84;
            double b = a * (1.0 - f);
            double a2 = a * a;
            double b2 = b * b;
            double e2 = (a2 - b2) / a2;
            double sinLat = System.Math.Sin(latitude);
            double cosLat = System.Math.Cos(latitude);
            double R = a2 * cosLat / System.Math.Sqrt(a2 * cosLat * cosLat + b2 * sinLat * sinLat);
            double R1 = a * cosLat / System.Math.Sqrt(1 - e2 * sinLat * sinLat);
            double phiPrime = System.Math.Atan((1 - f) * System.Math.Tan(latitude));
            double cosPhiPrime = System.Math.Cos(phiPrime);
            double sinPhiPrime = System.Math.Sin(phiPrime);
            double R2 = cosPhiPrime * System.Math.Sqrt((a2 * cosLat * a2 * cosLat + b2 * sinLat * b2 * sinLat) / (a2 * cosLat * cosLat + b2 * sinLat * sinLat));
            double R3 = cosPhiPrime * a * System.Math.Sqrt(1 - e2 * sinPhiPrime * sinPhiPrime);
            base.Y = R2 * longitude;
            double e = System.Math.Sqrt(e2);
            base.X = a * SpecialFunctions.EllipseE(latitude, e2);
        }
        /// <summary>
        /// return a SphericalPoint3D referred to a global coordinate system centered at the center of the Earth.
        /// </summary>
        /// <returns></returns>
        public SphericalPoint3D? GetSphericalPoint()
        {
            if (Latitude != null && Longitude != null && Z != null)
            {
                double a = Constants.EarthSemiMajorAxisWGS84;
                double f = 1.0 / Constants.EarthInverseFlateningWGS84;
                double b = a * (1.0 - f);
                double lat = Latitude.Value;
                double cosLat = System.Math.Cos(lat);
                double sinLat = System.Math.Sin(lat);
                double r = System.Math.Sqrt((a * a * a * a * cosLat * cosLat + b * b * b * b * sinLat * sinLat) / (a * a * cosLat * cosLat + b * b * sinLat * sinLat));
                r -= Z.Value;
                return new SphericalPoint3D() { Latitude = Latitude, Longitude = Longitude, R = r };
            }
            else
            {
                return null;
            }
        }
    }
}
