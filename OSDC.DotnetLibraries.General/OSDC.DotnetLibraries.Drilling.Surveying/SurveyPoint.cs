using MathNet.Numerics.Integration;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    public enum TrajectoryCalculationType
    {
        MinimumCurvatureMethod,
        ConstantBuildAndTurnMethod,
        ConstantCurvatureAndToolfaceMethod
    }
    public class SurveyPoint : CurvilinearPoint3D
    {
        private double? latitude_ = null;
        private double? longitude_ = null;
        public static readonly double InterpolationDeltaAbscissa = 0.01;
        public static double CompleteCTCSDT1Step = 0.1;
        public static int CompleteCTCSDT2Count = 1000;
        private static double GetAdaptiveCDTIncrementStep(double dls, double length)
        {
            if (!Numeric.IsDefined(dls) || !Numeric.IsDefined(length) || Numeric.LE(dls, 0.0) || Numeric.LE(length, 0.0))
            {
                return CompleteCTCSDT1Step;
            }
            // Limit the angular change per fallback substep and keep the historical fixed step as the minimum.
            double targetAngularStep = 0.01;
            double adaptive = targetAngularStep / dls;
            return System.Math.Min(length, System.Math.Max(CompleteCTCSDT1Step, System.Math.Min(10.0, adaptive)));
        }
        private static int GetAdaptiveCDTIntegrationCount(double dls, double length)
        {
            if (!Numeric.IsDefined(dls) || !Numeric.IsDefined(length) || Numeric.LE(dls, 0.0) || Numeric.LE(length, 0.0))
            {
                return CompleteCTCSDT2Count;
            }
            // Scale the Simpson subdivision count with the total angular variation over the segment.
            double totalAngle = System.Math.Abs(dls * length);
            int count = (int)System.Math.Ceiling(totalAngle / 0.01);
            count = System.Math.Max(32, count);
            count = System.Math.Min(CompleteCTCSDT2Count, count);
            if ((count & 1) == 1)
            {
                count++;
            }
            return count;
        }
        private double EstimateSegmentCurvature(SurveyPoint next, TrajectoryCalculationType calculationMethod)
        {
            if (next == null ||
                Abscissa == null ||
                next.Abscissa == null ||
                Inclination == null ||
                Azimuth == null ||
                next.Inclination == null ||
                next.Azimuth == null)
            {
                return 0.0;
            }

            double length = next.Abscissa.Value - Abscissa.Value;
            if (!Numeric.IsDefined(length) || Numeric.LE(length, 0.0))
            {
                return 0.0;
            }

            if (calculationMethod == TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod && next.Curvature != null)
            {
                return System.Math.Abs(next.Curvature.Value);
            }

            if (calculationMethod == TrajectoryCalculationType.ConstantBuildAndTurnMethod && next.BUR != null && next.TUR != null)
            {
                double sinInclination = System.Math.Sin(next.Inclination.Value);
                return System.Math.Sqrt(next.BUR.Value * next.BUR.Value + next.TUR.Value * next.TUR.Value * sinInclination * sinInclination);
            }

            double si1 = System.Math.Sin(Inclination.Value);
            double si2 = System.Math.Sin(next.Inclination.Value);
            double ci12 = System.Math.Cos(next.Inclination.Value - Inclination.Value);
            double ca12 = System.Math.Cos(next.Azimuth.Value - Azimuth.Value);
            double cosine = ci12 - (1.0 - ca12) * si2 * si1;
            cosine = System.Math.Max(-1.0, System.Math.Min(1.0, cosine));
            double dogleg = System.Math.Acos(cosine);
            return dogleg / length;
        }
        private double GetAdaptiveInterpolationDeltaAbscissa(SurveyPoint next, TrajectoryCalculationType calculationMethod)
        {
            if (next == null || Abscissa == null || next.Abscissa == null)
            {
                return InterpolationDeltaAbscissa;
            }

            double length = next.Abscissa.Value - Abscissa.Value;
            if (!Numeric.IsDefined(length) || Numeric.LE(length, 0.0))
            {
                return InterpolationDeltaAbscissa;
            }

            double maxStep = System.Math.Min(InterpolationDeltaAbscissa, 0.25 * length);
            if (Numeric.LE(maxStep, 0.0))
            {
                return InterpolationDeltaAbscissa;
            }

            double minStep = System.Math.Min(maxStep, System.Math.Max(1e-6, 1e-4 * length));
            double curvature = EstimateSegmentCurvature(next, calculationMethod);
            if (!Numeric.IsDefined(curvature) || Numeric.LE(curvature, 0.0))
            {
                return maxStep;
            }

            double targetAngularStep = 1e-3;
            double adaptive = targetAngularStep / curvature;
            return System.Math.Max(minStep, System.Math.Min(maxStep, adaptive));
        }

        /// <summary>
        /// synonym of Abscsissa
        /// </summary>
        public double? MD { get => base.Abscissa; set => base.Abscissa = value; }
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
        /// Gets or sets the vertical section value associated with the survey point.
        /// </summary>
        public double? VerticalSection { get; set; } = null;
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
                Curvature = src.Curvature;
                Toolface = src.Toolface;
                BUR = src.BUR;
                TUR = src.TUR;
                VerticalSection = src.VerticalSection;
            }
        }
        /// <summary>
        /// Apply the minimum curvature method to a list of survey points.
        /// The first survey point must be complete.
        /// The method, using generics, applies to SurveyList and SurveyStationList as well
        /// </summary>
        /// <param name="surveyList"></param>
        /// <returns></returns>
        public static bool CompleteSurvey<A>(List<A> surveyList, TrajectoryCalculationType calculationMethod = TrajectoryCalculationType.MinimumCurvatureMethod) where A : SurveyPoint
        {
            if (surveyList != null &&
                surveyList.Count > 0 &&
                surveyList[0].X != null &&
                surveyList[0].Y != null &&
                surveyList[0].Z != null &&
                (surveyList[0].Abscissa != null || surveyList[0].MD != null) &&
                surveyList[0].Inclination != null &&
                surveyList[0].Azimuth != null)
            {
                A sp1 = surveyList[0];
                bool ok = true;
                for (int i = 1; i < surveyList.Count; i++)
                {
                    var sp2 = surveyList[i];
                    // First evaluate X, Y, Z from S, I, A
                    if (sp2 != null && (sp2.Abscissa != null || sp2.MD != null) && sp2.Inclination != null && sp2.Azimuth != null)
                    {
                        ok = sp1.CompleteFromSIA(sp2, calculationMethod);
                        sp1 = sp2;
                        if (!ok) break;
                    }
                    // Then, evaluate S, I, A from X, Y, Z. IMPORTANT notice, if conditions are met, first prevail
                    else if (
                        sp2 != null &&
                        (sp2.X != null || sp2.RiemannianNorth != null) &&
                        (sp2.Y != null || sp2.RiemannianEast != null) &&
                        sp2.Z != null)
                    {
                        ok = sp1.CompleteFromXYZ(sp2, calculationMethod);
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
            return InterpolateAtAbscissa(surveyList, MD, interpolatedPoint, TrajectoryCalculationType.MinimumCurvatureMethod);
        }
        /// <summary>
        /// interpolate a Survey at a given abscissa. The abscissa must be between the first and last Survey of the SurveyList.
        /// The interpolation method is passed in argument.
        /// </summary>
        /// <param name="MD"></param>
        /// <param name="interpolatedPoint"></param>
        /// <param name="calculationMethod"></param>
        /// <returns></returns>
        public static bool InterpolateAtAbscissa<A>(List<A> surveyList, double MD, ICurvilinear3D interpolatedPoint, TrajectoryCalculationType calculationMethod) where A : SurveyPoint
        {
            if (interpolatedPoint == null ||
                Numeric.IsUndefined(MD) ||
                surveyList.Count < 2 ||
                Numeric.LT(MD, surveyList.First<A>().MD) ||
                Numeric.GT(MD, surveyList.Last<A>().MD))
            {
                return false;
            }
            else
            {
                for (int i = 1; i < surveyList.Count; i++)
                {
                    if (Numeric.GE(MD, surveyList[i - 1]?.MD) && Numeric.LE(MD, surveyList[i]?.MD))
                    {
                        return surveyList[i - 1].InterpolateAtAbscissa(surveyList[i], MD, interpolatedPoint, calculationMethod);
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
            return Interpolate(surveyList, mdStep, TrajectoryCalculationType.MinimumCurvatureMethod, null, abscissaList);
        }
        /// <summary>
        /// Return an interpolated SurveyList. The interpolation step is passed in argument. In addition
        /// interpolations are made at the abscissas given in a list. Additional points can be inserted so that
        /// the distance between the midpoint of the chord and the midpoint of the interpolated arc remains below
        /// a prescribed threshold. The interpolation uses the selected calculation method.
        /// </summary>
        /// <param name="mdStep"></param>
        /// <param name="calculationMethod"></param>
        /// <param name="maxChordMidArcDistance"></param>
        /// <param name="abscissaList"></param>
        /// <returns></returns>
        public static List<SurveyPoint>? Interpolate<A>(List<A> surveyList, double mdStep, TrajectoryCalculationType calculationMethod, double? maxChordMidArcDistance, List<double>? abscissaList = null) where A : SurveyPoint
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
                List<double> targetAbscissas = [md0];
                for (double s = md0 + mdStep; Numeric.LT(s, mdf); s += mdStep)
                {
                    targetAbscissas.Add(s);
                }
                targetAbscissas.Add(mdf);
                targetAbscissas.AddRange(abscissaFilteredList);
                targetAbscissas.Sort();

                List<double> uniqueAbscissas = [];
                foreach (double s in targetAbscissas)
                {
                    if (uniqueAbscissas.Count == 0 || !Numeric.EQ(uniqueAbscissas.Last(), s))
                    {
                        uniqueAbscissas.Add(s);
                    }
                }

                SurveyPoint? CreatePointAtAbscissa(double abscissa)
                {
                    if (Numeric.EQ(abscissa, md0))
                    {
                        return surveyList[0];
                    }
                    if (Numeric.EQ(abscissa, mdf))
                    {
                        return surveyList.Last();
                    }

                    SurveyPoint sp = new();
                    return InterpolateAtAbscissa(surveyList, abscissa, sp, calculationMethod) ? sp : null;
                }

                static double? MidChordSagitta(SurveyPoint start, SurveyPoint middle, SurveyPoint end)
                {
                    if (start.X is not { } x1 || start.Y is not { } y1 || start.Z is not { } z1 ||
                        middle.X is not { } xm || middle.Y is not { } ym || middle.Z is not { } zm ||
                        end.X is not { } x2 || end.Y is not { } y2 || end.Z is not { } z2)
                    {
                        return null;
                    }

                    double xc = 0.5 * (x1 + x2);
                    double yc = 0.5 * (y1 + y2);
                    double zc = 0.5 * (z1 + z2);
                    double dx = xm - xc;
                    double dy = ym - yc;
                    double dz = zm - zc;
                    return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
                }

                List<SurveyPoint> refinedPoints = [];

                void AppendRefinedSegment(SurveyPoint start, SurveyPoint end)
                {
                    if (maxChordMidArcDistance == null || !Numeric.GT(maxChordMidArcDistance, 0.0) ||
                        start.MD is not { } startMd || end.MD is not { } endMd)
                    {
                        refinedPoints.Add(end);
                        return;
                    }

                    double segmentLength = endMd - startMd;
                    if (!Numeric.GT(segmentLength, 1e-9))
                    {
                        refinedPoints.Add(end);
                        return;
                    }

                    double midMd = 0.5 * (startMd + endMd);
                    if (!Numeric.GT(midMd - startMd, 1e-9) || !Numeric.GT(endMd - midMd, 1e-9))
                    {
                        refinedPoints.Add(end);
                        return;
                    }

                    SurveyPoint middle = new();
                    if (!InterpolateAtAbscissa(surveyList, midMd, middle, calculationMethod))
                    {
                        refinedPoints.Add(end);
                        return;
                    }

                    double? sagitta = MidChordSagitta(start, middle, end);
                    if (sagitta != null && Numeric.GT(sagitta.Value, maxChordMidArcDistance.Value))
                    {
                        AppendRefinedSegment(start, middle);
                        AppendRefinedSegment(middle, end);
                    }
                    else
                    {
                        refinedPoints.Add(end);
                    }
                }

                List<SurveyPoint> basePoints = [];
                foreach (double s in uniqueAbscissas)
                {
                    SurveyPoint? point = CreatePointAtAbscissa(s);
                    if (point == null)
                    {
                        return null;
                    }
                    if (basePoints.Count == 0 || point.MD == null || basePoints.Last().MD == null || !Numeric.EQ(point.MD, basePoints.Last().MD))
                    {
                        basePoints.Add(point);
                    }
                }

                if (basePoints.Count == 0)
                {
                    return null;
                }

                refinedPoints.Add(basePoints[0]);
                for (int i = 1; i < basePoints.Count; i++)
                {
                    AppendRefinedSegment(basePoints[i - 1], basePoints[i]);
                }

                List<SurveyPoint> resultList = [];
                foreach (SurveyPoint point in refinedPoints)
                {
                    if (resultList.Count == 0 || point.MD == null || resultList.Last().MD == null || !Numeric.EQ(point.MD, resultList.Last().MD))
                    {
                        resultList.Add(point);
                    }
                }

                if (resultList.Count > 0)
                {
                    return resultList;
                }
                return null;
            }
            return null;
        }
        /// <summary>
        /// complete the next survey depending on its unknown values
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteNext(CurvilinearPoint3D next, TrajectoryCalculationType calculationMethod = TrajectoryCalculationType.MinimumCurvatureMethod)
        {
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null)
            {
                return false;
            }
            if (next.Abscissa != null && next.Inclination != null && next.Azimuth != null)
            {
                return CompleteFromSIA(next, calculationMethod);
            }
            else if (next.X != null && next.Y != null && next.Z != null)
            {
                return CompleteFromXYZ(next, calculationMethod);
            }
            else
            {
                return false;
            }
        }
        public bool CompleteFromSIA(CurvilinearPoint3D next, TrajectoryCalculationType calculationMethod = TrajectoryCalculationType.MinimumCurvatureMethod)
        {
            switch (calculationMethod)
            {
                case TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod:
                    return CompleteCDTSIA(next);
                case TrajectoryCalculationType.ConstantBuildAndTurnMethod:
                    return CompleteBTSIA(next);
                default:
                    return CompleteCASIA(next);
            }
        }
        public bool CompleteFromSIA(SurveyPoint next, TrajectoryCalculationType calculationMethod = TrajectoryCalculationType.MinimumCurvatureMethod)
        {
            if (CompleteFromSIA((CurvilinearPoint3D)next, calculationMethod))
            {
                CalculateCurvaturesToolfaceVerticalSection(next, calculationMethod);
                return true;
            }
            else
            {
                return false;
            }
        }
        protected void CalculateCurvaturesToolfaceVerticalSection(SurveyPoint next, TrajectoryCalculationType calculationMethod = TrajectoryCalculationType.MinimumCurvatureMethod)
        {
            CurvilinearPoint3D prev = new CurvilinearPoint3D();
            double ds = GetAdaptiveInterpolationDeltaAbscissa(next, calculationMethod);
            if (next.Abscissa is not null && next.Inclination is not null && next.Azimuth is not null && InterpolateAtAbscissa(next, next.Abscissa.Value - ds, prev, calculationMethod))
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
                    next.Curvature = (next.BUR != null && next.TUR != null) ? System.Math.Sqrt(next.BUR.Value * next.BUR.Value + next.TUR.Value * next.TUR.Value * si2 * si2) : (double?)null;
                }
            }
            if (VerticalSection is not null && X is not null && Y is not null && next.X is not null && next.Y is not null)
            {
                next.VerticalSection = VerticalSection + Math.Sqrt((X.Value - next.X.Value) * (X.Value - next.X.Value) + (Y.Value - next.Y.Value) * (Y.Value - next.Y.Value));
            }
        }
        /// <summary>
        /// Apply the minimum curvature method between this survey and the next
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteCASIA(CurvilinearPoint3D next)
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
        /// Apply the constant curvature and toolface method between this survey and the next
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteCDTSIA(CurvilinearPoint3D next)
        {
            return CompleteCDTSIAInternal(next, out _, false);
        }

        public bool CompleteCDTSIA(CurvilinearPoint3D next, out List<SurveyPoint> solutions)
        {
            return CompleteCDTSIAInternal(next, out solutions, true);
        }

        private bool CompleteCDTSIAInternal(CurvilinearPoint3D next, out List<SurveyPoint> solutions, bool collectSolutions)
        {
            solutions = new List<SurveyPoint>();
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null || next.Abscissa == null || next.Inclination == null || next.Azimuth == null)
            {
                return false;
            }

            double i1 = Inclination.Value;
            double a1 = Azimuth.Value;
            double s1 = Abscissa.Value;
            double i2 = next.Inclination.Value;
            double a2 = next.Azimuth.Value;
            double s2 = next.Abscissa.Value;
            double dm = s2 - s1;

            if (Numeric.EQ(dm, 0))
            {
                next.X = X;
                next.Y = Y;
                next.Z = Z;
                next.Inclination = i1;
                next.Azimuth = a1;
                next.Abscissa = s2;
                return true;
            }

            SurveyPoint? target = next as SurveyPoint;
            bool copyBack = target == null;
            target ??= new SurveyPoint()
            {
                Abscissa = next.Abscissa,
                Inclination = next.Inclination,
                Azimuth = next.Azimuth
            };
            static double NormalizeSignedAngle(double angle)
            {
                while (angle < -Numeric.PI)
                {
                    angle += 2.0 * Numeric.PI;
                }
                while (angle > Numeric.PI)
                {
                    angle -= 2.0 * Numeric.PI;
                }
                return angle;
            }
            static double NormalizeAzimuth(double angle)
            {
                double twoPi = 2.0 * Numeric.PI;
                angle %= twoPi;
                if (angle < 0.0)
                {
                    angle += twoPi;
                }
                return angle;
            }
            static void CanonicalizeInclinationAzimuth(ref double inclination, ref double azimuth)
            {
                while (inclination < 0.0)
                {
                    inclination = -inclination;
                    azimuth += Numeric.PI;
                }
                while (inclination > Numeric.PI)
                {
                    inclination = 2.0 * Numeric.PI - inclination;
                    azimuth += Numeric.PI;
                }
                azimuth = NormalizeAzimuth(azimuth);
            }
            static bool AreSameSolution(SurveyPoint left, SurveyPoint right)
            {
                const double positionTolerance = 1e-6;
                const double angleTolerance = 1e-8;
                if (left.X == null || left.Y == null || left.Z == null || left.Inclination == null || left.Azimuth == null ||
                    right.X == null || right.Y == null || right.Z == null || right.Inclination == null || right.Azimuth == null)
                {
                    return false;
                }
                return System.Math.Abs(left.X.Value - right.X.Value) <= positionTolerance &&
                    System.Math.Abs(left.Y.Value - right.Y.Value) <= positionTolerance &&
                    System.Math.Abs(left.Z.Value - right.Z.Value) <= positionTolerance &&
                    System.Math.Abs(left.Inclination.Value - right.Inclination.Value) <= angleTolerance &&
                    System.Math.Abs(NormalizeSignedAngle(left.Azimuth.Value - right.Azimuth.Value)) <= angleTolerance;
            }
            static (int minK, int maxK) GetSheetRange(double deltaAz, double logTerm, double bur)
            {
                const int fallbackRange = 4;
                const int maxRange = 64;
                const double maxSearchDls = 0.05;
                if (Numeric.EQ(logTerm, 0.0) || Numeric.EQ(bur, 0.0))
                {
                    return (-fallbackRange, fallbackRange);
                }
                double ratio = maxSearchDls / System.Math.Abs(bur);
                if (!Numeric.IsDefined(ratio) || ratio <= 1.0)
                {
                    return (-fallbackRange, fallbackRange);
                }
                double maxAbsA = System.Math.Sqrt(ratio * ratio - 1.0);
                double maxAbsDelta = System.Math.Abs(logTerm) * maxAbsA;
                if (!Numeric.IsDefined(maxAbsDelta))
                {
                    return (-fallbackRange, fallbackRange);
                }
                double twoPi = 2.0 * Numeric.PI;
                int minK = (int)System.Math.Ceiling((-maxAbsDelta - deltaAz) / twoPi);
                int maxK = (int)System.Math.Floor((maxAbsDelta - deltaAz) / twoPi);
                minK = System.Math.Max(minK, -maxRange);
                maxK = System.Math.Min(maxK, maxRange);
                minK = System.Math.Min(minK, -fallbackRange);
                maxK = System.Math.Max(maxK, fallbackRange);
                return (minK, maxK);
            }
            const double exactMatchTolerance = 1e-8;
            CanonicalizeInclinationAzimuth(ref i2, ref a2);
            double tan1 = System.Math.Tan(0.5 * i1);
            if (!Numeric.IsDefined(tan1) || Numeric.EQ(tan1, 0.0))
            {
                return false;
            }
            SurveyPoint? best = null;
            double bestScoreResidual = double.MaxValue;
            double bestRawAzimuthResidual = double.MaxValue;
            double bestDls = double.MaxValue;
            List<SurveyPoint>? exactSolutions = collectSolutions ? new List<SurveyPoint>() : null;
            List<(double rawInclination, double rawAzimuth)> targetVariants = new()
            {
                (i2, a2)
            };
            if (!Numeric.EQ(i2, 0.0))
            {
                targetVariants.Add((-i2, a2 - Numeric.PI));
            }
            if (!Numeric.EQ(i2, Numeric.PI))
            {
                targetVariants.Add((2.0 * Numeric.PI - i2, a2 - Numeric.PI));
            }

            foreach (var targetVariant in targetVariants)
            {
                double rawI2 = targetVariant.rawInclination;
                double rawA2 = targetVariant.rawAzimuth;
                double tan2 = System.Math.Tan(0.5 * rawI2);
                if (!Numeric.IsDefined(tan2) || Numeric.EQ(tan2, 0.0))
                {
                    continue;
                }
                double bur = (rawI2 - i1) / dm;
                double logTerm = System.Math.Log(System.Math.Abs(tan2 / tan1));
                double deltaAz = rawA2 - a1;
                var (minK, maxK) = GetSheetRange(deltaAz, logTerm, bur);
                for (int k = minK; k <= maxK; k++)
                {
                    double deltaCandidate = deltaAz + 2.0 * Numeric.PI * k;
                    double dls;
                    if (Numeric.EQ(logTerm, 0.0))
                    {
                        if (!Numeric.EQ(deltaCandidate, 0.0))
                        {
                            continue;
                        }
                        dls = System.Math.Abs(bur);
                    }
                    else
                    {
                        double A = deltaCandidate / logTerm;
                        dls = System.Math.Abs(bur) * System.Math.Sqrt(1.0 + A * A);
                    }

                    if (!Numeric.IsDefined(dls))
                    {
                        continue;
                    }

                    double ratioClamped = Numeric.EQ(dls, 0.0)
                        ? 1.0
                        : System.Math.Max(-1.0, System.Math.Min(1.0, bur / dls));
                    double tf0 = System.Math.Acos(ratioClamped);
                    double[] tfCandidates = Numeric.EQ(tf0, 0.0) || Numeric.EQ(tf0, Numeric.PI)
                        ? new double[] { tf0 }
                        : new double[] { tf0, 2.0 * Numeric.PI - tf0 };
                    foreach (double tfCandidate in tfCandidates)
                    {
                        SurveyPoint candidate = new SurveyPoint()
                        {
                            Abscissa = s2
                        };
                        if (!CompleteCDTSDT(candidate, dls, tfCandidate) || candidate.Inclination == null || candidate.Azimuth == null)
                        {
                            continue;
                        }
                        double candidateInclination = candidate.Inclination.Value;
                        double candidateAzimuth = candidate.Azimuth.Value;
                        CanonicalizeInclinationAzimuth(ref candidateInclination, ref candidateAzimuth);
                        double score = System.Math.Abs(candidateInclination - i2)
                            + System.Math.Abs(NormalizeSignedAngle(candidateAzimuth - a2));
                        candidate.Inclination = candidateInclination;
                        candidate.Azimuth = candidateAzimuth;
                        if (collectSolutions && score <= exactMatchTolerance && exactSolutions != null)
                        {
                            bool exists = false;
                            foreach (SurveyPoint existing in exactSolutions)
                            {
                                if (AreSameSolution(existing, candidate))
                                {
                                    exists = true;
                                    break;
                                }
                            }
                            if (!exists)
                            {
                                exactSolutions.Add(new SurveyPoint(candidate));
                            }
                        }
                        double rawAzimuthResidual = System.Math.Abs(candidateAzimuth - a2);
                        if (score < bestScoreResidual - 1e-10 ||
                            (System.Math.Abs(score - bestScoreResidual) <= 1e-10 &&
                             (System.Math.Abs(rawAzimuthResidual - bestRawAzimuthResidual) <= 1e-10
                                ? dls < bestDls
                                : rawAzimuthResidual < bestRawAzimuthResidual - 1e-10 ||
                                  (System.Math.Abs(rawAzimuthResidual - bestRawAzimuthResidual) <= 1e-8 && dls < bestDls))))
                        {
                            bestScoreResidual = score;
                            bestRawAzimuthResidual = rawAzimuthResidual;
                            bestDls = dls;
                            best = candidate;
                        }
                    }
                }
            }

            if (best == null)
            {
                return false;
            }
            target.X = best.X;
            target.Y = best.Y;
            target.Z = best.Z;
            target.Inclination = best.Inclination;
            target.Azimuth = best.Azimuth;
            target.Abscissa = best.Abscissa;
            target.Curvature = best.Curvature;
            target.Toolface = best.Toolface;
            target.BUR = best.BUR;
            target.TUR = best.TUR;
            target.VerticalSection = best.VerticalSection;
            if (collectSolutions && exactSolutions != null && exactSolutions.Count > 0)
            {
                solutions.AddRange(exactSolutions);
            }
            else if (collectSolutions)
            {
                solutions.Add(new SurveyPoint(best));
            }

            if (copyBack)
            {
                next.X = target.X;
                next.Y = target.Y;
                next.Z = target.Z;
                next.Inclination = target.Inclination;
                next.Azimuth = target.Azimuth;
                next.Abscissa = target.Abscissa;
            }
            return true;
        }

        /// <summary>
        /// Apply the constant build and turn method between this survey and the next
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteBTSIA(CurvilinearPoint3D next)
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

            double bur = (i2 - i1) / dm;
            double tur = (a2 - a1) / dm;
            double p = bur + tur;
            double s = bur - tur;
            double pdm = p * dm;
            double sdm = s * dm;

            if (next.Inclination < 0)
            {
                next.Inclination = -next.Inclination;
                next.Azimuth += Numeric.PI;
            }

            if (Numeric.EQ(bur, 0))
            {
                next.Z = z1 + System.Math.Cos(i1) * dm;
            }
            else
            {
                next.Z = z1 + (System.Math.Sin(next.Inclination.Value) - System.Math.Sin(i1)) / bur;
            }

            double E = System.Math.Sin(i1);
            double F = System.Math.Cos(i1);
            double G = System.Math.Sin(a1);
            double H = System.Math.Cos(a1);
            if (Numeric.EQ(System.Math.Abs(bur), System.Math.Abs(tur)))
            {
                if (Numeric.EQ(bur, 0))
                {
                    next.X = x1 + E * H * dm;
                    next.Y = y1 + E * G * dm;
                    return true;
                }
                else
                {
                    double epsilon = (bur * tur >= 0) ? 1 : -1;
                    double s2b = System.Math.Sin(2.0 * bur * dm);
                    double c2b = System.Math.Cos(2.0 * bur * dm);
                    double _2b = 2.0 * bur;
                    next.X = x1 + (0.5 * E * H * (dm + s2b / _2b) - 0.5 * E * G * epsilon * (1.0 - c2b) / _2b) + (0.5 * F * H * (1.0 - c2b) / _2b - 0.5 * F * G * epsilon * (dm - s2b / _2b));
                    next.Y = y1 + (0.5 * E * G * (dm + s2b / _2b) + 0.5 * E * H * epsilon * (1.0 - c2b) / _2b) + (0.5 * F * G * (1.0 - c2b) / _2b + 0.5 * F * H * epsilon * (dm - s2b / _2b));
                    return true;
                }
            }
            else
            {
                double cosp = System.Math.Cos(pdm) / p;
                double sinp = System.Math.Sin(pdm) / p;
                double coss = System.Math.Cos(sdm) / s;
                double sins = System.Math.Sin(sdm) / s;
                double _1p = 1.0 / p;
                double _1s = 1.0 / s;
                next.X = x1 + (0.5 * E * H * (sinp + sins) - 0.5 * E * G * ((coss - cosp) - (_1s - _1p))) - (0.5 * F * G * (sins - sinp) + 0.5 * F * H * (cosp + coss - (_1p + _1s)));
                next.Y = y1 + (0.5 * E * G * (sinp + sins) + 0.5 * E * H * ((coss - cosp) - (_1s - _1p))) + (0.5 * F * H * (sins - sinp) - 0.5 * F * G * (cosp + coss - (_1p + _1s)));
                return true;
            }
        }

        public bool CompleteFromXYZ(CurvilinearPoint3D next, TrajectoryCalculationType calculationMethod = TrajectoryCalculationType.MinimumCurvatureMethod)
        {
            switch (calculationMethod)
            {
                case TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod:
                    return CompleteCDTXYZ(next);
                case TrajectoryCalculationType.ConstantBuildAndTurnMethod:
                    return CompleteBTXYZ(next);
                default:
                    return CompleteCAXYZ(next);
            }
        }
        public bool CompleteFromXYZ(SurveyPoint next, TrajectoryCalculationType calculationMethod = TrajectoryCalculationType.MinimumCurvatureMethod)
        {
            if (CompleteFromXYZ((CurvilinearPoint3D)next, calculationMethod))
            {
                CalculateCurvaturesToolfaceVerticalSection(next, calculationMethod);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool CompleteCAXYZ(CurvilinearPoint3D next)
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
        public bool CompleteBTXYZ(CurvilinearPoint3D next)
        {
            if (next == null ||
                X == null ||
                Y == null ||
                Z == null ||
                next.X == null ||
                next.Y == null ||
                next.Z == null ||
                Inclination == null ||
                Azimuth == null ||
                Abscissa == null)
            {
                return false;
            }

            double targetX = next.X.Value;
            double targetY = next.Y.Value;
            double targetZ = next.Z.Value;
            double x1 = X.Value;
            double y1 = Y.Value;
            double z1 = Z.Value;
            double i1 = Inclination.Value;
            double a1 = Azimuth.Value;
            double s1 = Abscissa.Value;

            double chord =
                System.Math.Sqrt(
                    (targetX - x1) * (targetX - x1) +
                    (targetY - y1) * (targetY - y1) +
                    (targetZ - z1) * (targetZ - z1));

            if (Numeric.EQ(chord, 0.0))
            {
                next.Abscissa = s1;
                next.Inclination = i1;
                next.Azimuth = a1;
                return true;
            }

            static bool Solve3x3(double[,] matrix, double[] rhs, out double[] solution)
            {
                solution = new double[3];
                double[,] a = (double[,])matrix.Clone();
                double[] b = (double[])rhs.Clone();
                for (int i = 0; i < 3; i++)
                {
                    int pivot = i;
                    double maxAbs = System.Math.Abs(a[i, i]);
                    for (int j = i + 1; j < 3; j++)
                    {
                        double candidate = System.Math.Abs(a[j, i]);
                        if (candidate > maxAbs)
                        {
                            maxAbs = candidate;
                            pivot = j;
                        }
                    }
                    if (Numeric.EQ(maxAbs, 0.0))
                    {
                        return false;
                    }
                    if (pivot != i)
                    {
                        for (int k = i; k < 3; k++)
                        {
                            (a[i, k], a[pivot, k]) = (a[pivot, k], a[i, k]);
                        }
                        (b[i], b[pivot]) = (b[pivot], b[i]);
                    }
                    double diag = a[i, i];
                    for (int j = i + 1; j < 3; j++)
                    {
                        double factor = a[j, i] / diag;
                        for (int k = i; k < 3; k++)
                        {
                            a[j, k] -= factor * a[i, k];
                        }
                        b[j] -= factor * b[i];
                    }
                }
                for (int i = 2; i >= 0; i--)
                {
                    double sum = b[i];
                    for (int j = i + 1; j < 3; j++)
                    {
                        sum -= a[i, j] * solution[j];
                    }
                    if (Numeric.EQ(a[i, i], 0.0))
                    {
                        return false;
                    }
                    solution[i] = sum / a[i, i];
                }
                return true;
            }

            static double NormalizeAzimuth(double angle)
            {
                double twoPi = 2.0 * Numeric.PI;
                angle %= twoPi;
                if (angle < 0.0)
                {
                    angle += twoPi;
                }
                return angle;
            }

            static void Canonicalize(ref double abscissa, ref double inclination, ref double azimuth, double startAbscissa)
            {
                if (Numeric.LT(abscissa, startAbscissa))
                {
                    abscissa = startAbscissa;
                }
                while (inclination < 0.0)
                {
                    inclination = -inclination;
                    azimuth += Numeric.PI;
                }
                while (inclination > Numeric.PI)
                {
                    inclination = 2.0 * Numeric.PI - inclination;
                    azimuth += Numeric.PI;
                }
                azimuth = NormalizeAzimuth(azimuth);
            }

            static double AzimuthFromNorth(double dNorth, double dEast)
            {
                double horizontal = System.Math.Sqrt(dNorth * dNorth + dEast * dEast);
                if (Numeric.EQ(horizontal, 0.0))
                {
                    return 0.0;
                }
                return (dEast >= 0.0)
                    ? Numeric.AcosEqual(dNorth / horizontal)
                    : 2.0 * Numeric.PI - Numeric.AcosEqual(dNorth / horizontal);
            }

            double dNorth = targetX - x1;
            double dEast = targetY - y1;
            double horizontalDistance = System.Math.Sqrt(dNorth * dNorth + dEast * dEast);
            double dZ = targetZ - z1;
            double theta = NormalizeAzimuth(AzimuthFromNorth(dNorth, dEast) - a1);
            if (theta > Numeric.PI)
            {
                theta -= 2.0 * Numeric.PI;
            }

            bool Evaluate(double abscissaLocal, double inclinationLocal, double azimuthLocal, out CurvilinearPoint3D candidate, out double[] residual)
            {
                Canonicalize(ref abscissaLocal, ref inclinationLocal, ref azimuthLocal, s1);
                candidate = new CurvilinearPoint3D()
                {
                    Abscissa = abscissaLocal,
                    Inclination = inclinationLocal,
                    Azimuth = azimuthLocal
                };
                if (!CompleteBTSIA(candidate) || candidate.X == null || candidate.Y == null || candidate.Z == null)
                {
                    residual = [double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity];
                    return false;
                }
                residual =
                [
                    targetX - candidate.X.Value,
                    targetY - candidate.Y.Value,
                    targetZ - candidate.Z.Value
                ];
                return true;
            }

            static double ResidualNormSquared(double[] residual)
                => residual[0] * residual[0] + residual[1] * residual[1] + residual[2] * residual[2];

            bool TryDerivativeColumn(double[] state, int index, double[] baseResidual, out double[] column)
            {
                column = [0.0, 0.0, 0.0];
                double step = index switch
                {
                    0 => System.Math.Max(1e-6, 1e-6 * System.Math.Max(1.0, System.Math.Abs(state[0]))),
                    _ => System.Math.Max(1e-8, 1e-7 * System.Math.Max(1.0, System.Math.Abs(state[index])))
                };

                double[] plus = (double[])state.Clone();
                plus[index] += step;
                bool okPlus = Evaluate(plus[0], plus[1], plus[2], out _, out double[] residualPlus);

                double[] minus = (double[])state.Clone();
                minus[index] -= step;
                bool okMinus = Evaluate(minus[0], minus[1], minus[2], out _, out double[] residualMinus);

                if (okPlus && okMinus)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        column[i] = (residualPlus[i] - residualMinus[i]) / (2.0 * step);
                    }
                    return true;
                }
                if (okPlus)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        column[i] = (residualPlus[i] - baseResidual[i]) / step;
                    }
                    return true;
                }
                if (okMinus)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        column[i] = (baseResidual[i] - residualMinus[i]) / step;
                    }
                    return true;
                }
                return false;
            }

            bool TrySolveFromSeed(double seedAbscissa, double seedInclination, double seedAzimuth, out CurvilinearPoint3D solution)
            {
                solution = new CurvilinearPoint3D();
                double[] state = [seedAbscissa, seedInclination, seedAzimuth];
                Canonicalize(ref state[0], ref state[1], ref state[2], s1);

                if (!Evaluate(state[0], state[1], state[2], out CurvilinearPoint3D current, out double[] residual))
                {
                    return false;
                }

                double bestNorm = ResidualNormSquared(residual);
                double tolerance = System.Math.Max(1e-16, 1e-20 * System.Math.Max(1.0, chord * chord));
                if (bestNorm <= tolerance)
                {
                    solution = current;
                    return true;
                }

                for (int iteration = 0; iteration < 80; iteration++)
                {
                    double[][] jacobian = [[], [], []];
                    for (int columnIndex = 0; columnIndex < 3; columnIndex++)
                    {
                        if (!TryDerivativeColumn(state, columnIndex, residual, out jacobian[columnIndex]))
                        {
                            return false;
                        }
                    }

                    double[,] normal =
                    {
                        {
                            jacobian[0][0] * jacobian[0][0] + jacobian[0][1] * jacobian[0][1] + jacobian[0][2] * jacobian[0][2],
                            jacobian[0][0] * jacobian[1][0] + jacobian[0][1] * jacobian[1][1] + jacobian[0][2] * jacobian[1][2],
                            jacobian[0][0] * jacobian[2][0] + jacobian[0][1] * jacobian[2][1] + jacobian[0][2] * jacobian[2][2]
                        },
                        {
                            jacobian[1][0] * jacobian[0][0] + jacobian[1][1] * jacobian[0][1] + jacobian[1][2] * jacobian[0][2],
                            jacobian[1][0] * jacobian[1][0] + jacobian[1][1] * jacobian[1][1] + jacobian[1][2] * jacobian[1][2],
                            jacobian[1][0] * jacobian[2][0] + jacobian[1][1] * jacobian[2][1] + jacobian[1][2] * jacobian[2][2]
                        },
                        {
                            jacobian[2][0] * jacobian[0][0] + jacobian[2][1] * jacobian[0][1] + jacobian[2][2] * jacobian[0][2],
                            jacobian[2][0] * jacobian[1][0] + jacobian[2][1] * jacobian[1][1] + jacobian[2][2] * jacobian[1][2],
                            jacobian[2][0] * jacobian[2][0] + jacobian[2][1] * jacobian[2][1] + jacobian[2][2] * jacobian[2][2]
                        }
                    };
                    double[] gradient =
                    [
                        -(jacobian[0][0] * residual[0] + jacobian[0][1] * residual[1] + jacobian[0][2] * residual[2]),
                        -(jacobian[1][0] * residual[0] + jacobian[1][1] * residual[1] + jacobian[1][2] * residual[2]),
                        -(jacobian[2][0] * residual[0] + jacobian[2][1] * residual[1] + jacobian[2][2] * residual[2])
                    ];

                    double damping = 1e-8;
                    bool improved = false;
                    for (int dampingCount = 0; dampingCount < 12 && !improved; dampingCount++)
                    {
                        double[,] damped = (double[,])normal.Clone();
                        damped[0, 0] += damping * System.Math.Max(1.0, normal[0, 0]);
                        damped[1, 1] += damping * System.Math.Max(1.0, normal[1, 1]);
                        damped[2, 2] += damping * System.Math.Max(1.0, normal[2, 2]);

                        if (!Solve3x3(damped, gradient, out double[] step))
                        {
                            damping *= 10.0;
                            continue;
                        }

                        for (int lineSearch = 0; lineSearch < 12 && !improved; lineSearch++)
                        {
                            double factor = 1.0 / (1 << lineSearch);
                            double[] trial =
                            [
                                state[0] + factor * step[0],
                                state[1] + factor * step[1],
                                state[2] + factor * step[2]
                            ];
                            Canonicalize(ref trial[0], ref trial[1], ref trial[2], s1);
                            if (!Evaluate(trial[0], trial[1], trial[2], out CurvilinearPoint3D trialCandidate, out double[] trialResidual))
                            {
                                continue;
                            }

                            double trialNorm = ResidualNormSquared(trialResidual);
                            if (trialNorm < bestNorm)
                            {
                                state = trial;
                                current = trialCandidate;
                                residual = trialResidual;
                                bestNorm = trialNorm;
                                improved = true;
                            }
                        }

                        if (!improved)
                        {
                            damping *= 10.0;
                        }
                    }

                    if (bestNorm <= tolerance)
                    {
                        solution = current;
                        return true;
                    }

                    if (!improved)
                    {
                        break;
                    }
                }

                solution = current;
                return bestNorm <= System.Math.Max(1e-12, 1e-14 * System.Math.Max(1.0, chord * chord));
            }

            CurvilinearPoint3D caSeed = new() { X = targetX, Y = targetY, Z = targetZ };
            bool hasCaSeed = CompleteCAXYZ(caSeed) &&
                             caSeed.Abscissa != null &&
                             caSeed.Inclination != null &&
                             caSeed.Azimuth != null;

            double lineInclination = Numeric.AcosEqual((targetZ - z1) / chord);
            double lineAzimuth = AzimuthFromNorth(dNorth, dEast);
            double l = Numeric.EQ(theta, 0.0) ? horizontalDistance : horizontalDistance * theta / System.Math.Sin(theta);
            double elevation = Numeric.EQ(dZ, 0.0) ? Numeric.PI / 2.0 : (dZ > 0.0 ? System.Math.Atan(l / dZ) : Numeric.PI / 2.0 + System.Math.Atan(l / dZ));
            double phi = elevation - i1;
            double spatialDistance = System.Math.Sqrt(dZ * dZ + l * l);
            double initDm = Numeric.EQ(phi, 0.0) ? chord : spatialDistance * phi / System.Math.Sin(phi);
            double initInclination = i1 + 2.0 * phi;
            double initAzimuth = a1 + 2.0 * theta;

            List<(double abscissa, double inclination, double azimuth)> seeds = [];
            if (Numeric.GT(initDm, 0.0))
            {
                seeds.Add((s1 + initDm, initInclination, initAzimuth));
            }
            if (hasCaSeed)
            {
                seeds.Add((caSeed.Abscissa!.Value, caSeed.Inclination!.Value, caSeed.Azimuth!.Value));
            }
            seeds.Add((s1 + chord, lineInclination, lineAzimuth));
            seeds.Add((s1 + System.Math.Max(chord, hasCaSeed ? caSeed.Abscissa!.Value - s1 : chord), lineInclination, a1));
            seeds.Add((s1 + chord * 1.1, 0.5 * (i1 + lineInclination), lineAzimuth));
            if (hasCaSeed)
            {
                seeds.Add((caSeed.Abscissa!.Value, 0.5 * (caSeed.Inclination!.Value + lineInclination), caSeed.Azimuth!.Value));
            }

            CurvilinearPoint3D? bestCandidate = null;
            double bestResidualNorm = double.PositiveInfinity;
            foreach (var seed in seeds)
            {
                if (TrySolveFromSeed(seed.abscissa, seed.inclination, seed.azimuth, out CurvilinearPoint3D candidate) &&
                    candidate.X != null &&
                    candidate.Y != null &&
                    candidate.Z != null)
                {
                    double dx = targetX - candidate.X.Value;
                    double dy = targetY - candidate.Y.Value;
                    double dz = targetZ - candidate.Z.Value;
                    double residualNorm = dx * dx + dy * dy + dz * dz;
                    if (residualNorm < bestResidualNorm)
                    {
                        bestResidualNorm = residualNorm;
                        bestCandidate = candidate;
                    }
                }
            }

            if (bestCandidate != null &&
                bestCandidate.Abscissa != null &&
                bestCandidate.Inclination != null &&
                bestCandidate.Azimuth != null &&
                bestResidualNorm <= System.Math.Max(1e-12, 1e-14 * System.Math.Max(1.0, chord * chord)))
            {
                next.Abscissa = bestCandidate.Abscissa;
                next.Inclination = bestCandidate.Inclination;
                next.Azimuth = bestCandidate.Azimuth;
                next.X = targetX;
                next.Y = targetY;
                next.Z = targetZ;
                return true;
            }

            return false;
        }

        public bool CompleteCDTXYZ(CurvilinearPoint3D next)
            => CompleteCDTXYZInternal(next, out _, false);

        public bool CompleteCDTXYZ(CurvilinearPoint3D next, out double? curvature, out double? toolface)
        {
            bool result = CompleteCDTXYZInternal(next, out List<SurveyPoint> solutions, true);
            if (result && solutions.Count > 0 && solutions[0] != null)
            {
                curvature = solutions[0].Curvature;
                toolface = solutions[0].Toolface;
                next.Abscissa = solutions[0].Abscissa;
                next.Inclination = solutions[0].Inclination;
                next.Azimuth = solutions[0].Azimuth;
            }
            else
            {
                curvature = null;
                toolface = null;
            }
            return result;
        }

        public bool CompleteCDTXYZ(CurvilinearPoint3D next, out List<SurveyPoint> solutions)
            => CompleteCDTXYZInternal(next, out solutions, true);

        private bool CompleteCDTXYZInternal(CurvilinearPoint3D next, out List<SurveyPoint> solutions, bool collectSolutions)
        {
            List<SurveyPoint> solutionList = [];
            solutions = solutionList;
            if (next == null || X == null || Y == null || Z == null || next.X == null || next.Y == null || next.Z == null || Inclination == null || Azimuth == null || Abscissa == null)
            {
                return false;
            }

            double x1 = X.Value;
            double y1 = Y.Value;
            double z1 = Z.Value;
            double i1 = Inclination.Value;
            double a1 = Azimuth.Value;
            double s1 = Abscissa.Value;
            double targetX = next.X.Value;
            double targetY = next.Y.Value;
            double targetZ = next.Z.Value;
            double dx = targetX - x1;
            double dy = targetY - y1;
            double dz = targetZ - z1;
            double chord = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);

            static double WrapAngle(double angle)
            {
                double twoPi = 2.0 * Numeric.PI;
                angle %= twoPi;
                if (angle < 0.0)
                {
                    angle += twoPi;
                }
                return angle;
            }
            static double SignedAngleDistance(double angle1, double angle2)
            {
                double delta = angle1 - angle2;
                while (delta > Numeric.PI)
                {
                    delta -= 2.0 * Numeric.PI;
                }
                while (delta < -Numeric.PI)
                {
                    delta += 2.0 * Numeric.PI;
                }
                return System.Math.Abs(delta);
            }
            static double Norm2(double[] values)
            {
                double sum = 0.0;
                for (int i = 0; i < values.Length; i++)
                {
                    sum += values[i] * values[i];
                }
                return sum;
            }
            static bool Solve3x3(double[,] m, double[] rhs, out double[] x)
            {
                x = new double[3];
                double[,] a = (double[,])m.Clone();
                double[] b = (double[])rhs.Clone();
                for (int i = 0; i < 3; i++)
                {
                    int pivot = i;
                    double max = System.Math.Abs(a[i, i]);
                    for (int j = i + 1; j < 3; j++)
                    {
                        double candidate = System.Math.Abs(a[j, i]);
                        if (candidate > max)
                        {
                            max = candidate;
                            pivot = j;
                        }
                    }
                    if (max < 1e-14)
                    {
                        return false;
                    }
                    if (pivot != i)
                    {
                        for (int k = i; k < 3; k++)
                        {
                            (a[i, k], a[pivot, k]) = (a[pivot, k], a[i, k]);
                        }
                        (b[i], b[pivot]) = (b[pivot], b[i]);
                    }
                    double diag = a[i, i];
                    for (int j = i + 1; j < 3; j++)
                    {
                        double factor = a[j, i] / diag;
                        for (int k = i; k < 3; k++)
                        {
                            a[j, k] -= factor * a[i, k];
                        }
                        b[j] -= factor * b[i];
                    }
                }
                for (int i = 2; i >= 0; i--)
                {
                    double sum = b[i];
                    for (int j = i + 1; j < 3; j++)
                    {
                        sum -= a[i, j] * x[j];
                    }
                    if (System.Math.Abs(a[i, i]) < 1e-14)
                    {
                        return false;
                    }
                    x[i] = sum / a[i, i];
                }
                return true;
            }
            static bool EquivalentSolution(CurvilinearPoint3D left, CurvilinearPoint3D right)
            {
                if (left.Abscissa == null || left.Inclination == null || left.Azimuth == null ||
                    right.Abscissa == null || right.Inclination == null || right.Azimuth == null)
                {
                    return false;
                }
                return System.Math.Abs(left.Abscissa.Value - right.Abscissa.Value) <= 1e-6 &&
                       System.Math.Abs(left.Inclination.Value - right.Inclination.Value) <= 1e-6 &&
                       SignedAngleDistance(left.Azimuth.Value, right.Azimuth.Value) <= 1e-6;
            }
            static SurveyPoint ToSolutionPoint(SurveyPoint candidate)
                => new()
                {
                    Abscissa = candidate.Abscissa,
                    Inclination = candidate.Inclination,
                    Azimuth = candidate.Azimuth,
                    X = candidate.X,
                    Y = candidate.Y,
                    Z = candidate.Z,
                    Curvature = candidate.Curvature,
                    Toolface = candidate.Toolface
                };
            void TryAddSolution(SurveyPoint candidate)
            {
                if (!collectSolutions ||
                    candidate.Abscissa == null ||
                    candidate.Inclination == null ||
                    candidate.Azimuth == null)
                {
                    return;
                }
                SurveyPoint solution = ToSolutionPoint(candidate);
                foreach (CurvilinearPoint3D existing in solutionList)
                {
                    if (EquivalentSolution(existing, solution))
                    {
                        return;
                    }
                }
                solutionList.Add(solution);
            }

            if (Numeric.EQ(chord, 0.0))
            {
                next.Abscissa = s1;
                next.Inclination = i1;
                next.Azimuth = a1;
                double curvature = 0.0;
                double? toolface = null;
                if (collectSolutions)
                {
                    solutionList.Add(new SurveyPoint()
                    {
                        Abscissa = s1,
                        Inclination = i1,
                        Azimuth = a1,
                        X = targetX,
                        Y = targetY,
                        Z = targetZ,
                        Curvature = curvature,
                        Toolface = toolface
                    });
                }
                return true;
            }

            double tanX = System.Math.Sin(i1) * System.Math.Cos(a1);
            double tanY = System.Math.Sin(i1) * System.Math.Sin(a1);
            double tanZ = System.Math.Cos(i1);
            double forwardProjection = dx * tanX + dy * tanY + dz * tanZ;
            if (System.Math.Abs(chord - forwardProjection) <= 1e-8 * System.Math.Max(1.0, chord))
            {
                next.Abscissa = s1 + chord;
                next.Inclination = i1;
                next.Azimuth = a1;
                double curvature = 0.0;
                double? toolface = null;
                if (collectSolutions)
                {
                    solutionList.Add(new SurveyPoint()
                    {
                        Abscissa = s1 + chord,
                        Inclination = i1,
                        Azimuth = a1,
                        X = targetX,
                        Y = targetY,
                        Z = targetZ,
                        Curvature = curvature,
                        Toolface = toolface
                    });
                }
                return true;
            }

            double scale = System.Math.Max(chord, 1.0);
            CurvilinearPoint3D caSeed = new() { X = targetX, Y = targetY, Z = targetZ };
            bool hasCaSeed = CompleteCAXYZ(caSeed) &&
                caSeed.Abscissa != null &&
                caSeed.Inclination != null &&
                caSeed.Azimuth != null &&
                Numeric.GT(caSeed.Abscissa.Value - s1, 0.0);

            double caLength = s1 + chord;
            double caCurvature = System.Math.Max(1e-6, 1.0 / System.Math.Max(chord, 1.0));
            double caToolface = WrapAngle(a1);
            if (hasCaSeed)
            {
                SurveyPoint caSurvey = new()
                {
                    Abscissa = caSeed.Abscissa,
                    Inclination = caSeed.Inclination,
                    Azimuth = caSeed.Azimuth
                };
                CalculateCurvaturesToolfaceVerticalSection(caSurvey, TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod);
                if (caSurvey.Abscissa != null)
                {
                    caLength = caSurvey.Abscissa.Value;
                }
                if (caSurvey.Curvature != null && Numeric.GT(caSurvey.Curvature.Value, 0.0))
                {
                    caCurvature = caSurvey.Curvature.Value;
                }
                if (caSurvey.Toolface != null)
                {
                    caToolface = WrapAngle(caSurvey.Toolface.Value);
                }
            }

            const double wLength = 1e-5;
            const double wCurvature = 1e-4;
            const double solveTol = 1e-8;
            const double exactTol = 1e-12;
            const double acceptTol = 1e-4;
            const double lowDoglegThreshold = 0.35;

            double preferredSearchLength = hasCaSeed
                ? System.Math.Max(chord, (caLength - s1) + System.Math.Max(10.0, 0.1 * (caLength - s1)))
                : System.Math.Max(chord, 1.5 * chord);
            double expandedSearchLength = hasCaSeed
                ? System.Math.Max(chord, 2.0 * (caLength - s1))
                : System.Math.Max(chord, 4.0 * chord);
            double maxSearchLength = preferredSearchLength;

            bool Evaluate(double phi, double eta, double ell, bool regularize, out SurveyPoint candidate, out double[] residual, out double positionNorm2)
            {
                candidate = new SurveyPoint() { Abscissa = s1 + System.Math.Exp(ell) };
                double kappa = System.Math.Exp(eta);
                double wrappedPhi = WrapAngle(phi);
                double solvedLength = candidate.Abscissa.Value - s1;
                if (!Numeric.IsDefined(kappa) ||
                    !Numeric.IsDefined(candidate.Abscissa) ||
                    Numeric.LE(solvedLength, 0.0) ||
                    Numeric.LT(solvedLength, chord) ||
                    Numeric.GT(solvedLength, maxSearchLength))
                {
                    residual = regularize
                        ? [double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity]
                        : [double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity];
                    positionNorm2 = double.PositiveInfinity;
                    return false;
                }
                if (!CompleteCDTSDT(candidate, kappa, wrappedPhi) || candidate.X == null || candidate.Y == null || candidate.Z == null)
                {
                    residual = regularize
                        ? [double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity]
                        : [double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity];
                    positionNorm2 = double.PositiveInfinity;
                    return false;
                }
                double rx = (candidate.X.Value - targetX) / scale;
                double ry = (candidate.Y.Value - targetY) / scale;
                double rz = (candidate.Z.Value - targetZ) / scale;
                positionNorm2 = rx * rx + ry * ry + rz * rz;
                if (regularize)
                {
                    double rL = System.Math.Sqrt(wLength) * (solvedLength - (caLength - s1)) / scale;
                    double rK = System.Math.Sqrt(wCurvature) * (kappa - caCurvature) / System.Math.Max(caCurvature, 1e-6);
                    residual = [rx, ry, rz, rL, rK];
                }
                else
                {
                    residual = [rx, ry, rz];
                }
                return true;
            }

            bool TryDerivative(double[] state, int index, double[] baseResidual, bool regularize, out double[] column)
            {
                column = new double[baseResidual.Length];
                double step = index switch
                {
                    0 => 1e-5,
                    1 => System.Math.Max(1e-6, 1e-4 * System.Math.Max(1.0, System.Math.Abs(state[1]))),
                    _ => System.Math.Max(1e-4, 1e-4 * System.Math.Max(1.0, System.Math.Abs(state[2])))
                };

                double[] plus = (double[])state.Clone();
                plus[index] += step;
                bool okPlus = Evaluate(plus[0], plus[1], plus[2], regularize, out _, out double[] residualPlus, out _);

                double[] minus = (double[])state.Clone();
                minus[index] -= step;
                bool okMinus = Evaluate(minus[0], minus[1], minus[2], regularize, out _, out double[] residualMinus, out _);

                if (okPlus && okMinus)
                {
                    for (int i = 0; i < column.Length; i++)
                    {
                        column[i] = (residualPlus[i] - residualMinus[i]) / (2.0 * step);
                    }
                    return true;
                }
                if (okPlus)
                {
                    for (int i = 0; i < column.Length; i++)
                    {
                        column[i] = (residualPlus[i] - baseResidual[i]) / step;
                    }
                    return true;
                }
                if (okMinus)
                {
                    for (int i = 0; i < column.Length; i++)
                    {
                        column[i] = (baseResidual[i] - residualMinus[i]) / step;
                    }
                    return true;
                }
                return false;
            }

            bool TrySolve(double phiSeed, double kappaSeed, double lengthSeed, bool regularize, out SurveyPoint solution, out double bestPositionNorm2)
            {
                solution = new SurveyPoint();
                bestPositionNorm2 = double.PositiveInfinity;
                double[] state =
                [
                    WrapAngle(phiSeed),
                    System.Math.Log(System.Math.Max(kappaSeed, 1e-9)),
                    System.Math.Log(System.Math.Max(lengthSeed, chord))
                ];
                double[] initialState = (double[])state.Clone();
                if (!Evaluate(state[0], state[1], state[2], regularize, out SurveyPoint current, out double[] residual, out double positionNorm2))
                {
                    return false;
                }

                double bestObjective = Norm2(residual);
                double bestPositionNorm2Local = positionNorm2;
                SurveyPoint bestCandidate = current;
                void UpdateBest(double[] candidateState, SurveyPoint candidate, double[] candidateResidual, double candidatePositionNorm2)
                {
                    double candidateObjective = Norm2(candidateResidual);
                    if (candidateObjective < bestObjective)
                    {
                        state = (double[])candidateState.Clone();
                        current = candidate;
                        residual = candidateResidual;
                        bestObjective = candidateObjective;
                        bestPositionNorm2Local = candidatePositionNorm2;
                        bestCandidate = candidate;
                    }
                }
                for (int iter = 0; iter < 40 && bestPositionNorm2Local > solveTol * solveTol; iter++)
                {
                    int residualLength = regularize ? 5 : 3;
                    double[][] jacobian =
                    [
                        new double[residualLength],
                        new double[residualLength],
                        new double[residualLength]
                    ];
                    for (int k = 0; k < 3; k++)
                    {
                        if (!TryDerivative(state, k, residual, regularize, out jacobian[k]))
                        {
                            break;
                        }
                    }

                    if (jacobian[0].All(v => Numeric.EQ(v, 0.0)) &&
                        jacobian[1].All(v => Numeric.EQ(v, 0.0)) &&
                        jacobian[2].All(v => Numeric.EQ(v, 0.0)))
                    {
                        break;
                    }

                    double[,] normal = new double[3, 3];
                    double[] gradient = new double[3];
                    for (int r = 0; r < residual.Length; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            gradient[c] -= jacobian[c][r] * residual[r];
                            for (int c2 = 0; c2 < 3; c2++)
                            {
                                normal[c, c2] += jacobian[c][r] * jacobian[c2][r];
                            }
                        }
                    }

                    bool improved = false;
                    double damping = 1e-6;
                    for (int dc = 0; dc < 8 && !improved; dc++)
                    {
                        double[,] damped = (double[,])normal.Clone();
                        for (int d = 0; d < 3; d++)
                        {
                            damped[d, d] += damping * System.Math.Max(1.0, normal[d, d]);
                        }
                        if (!Solve3x3(damped, gradient, out double[] step))
                        {
                            damping *= 10.0;
                            continue;
                        }

                        for (int ls = 0; ls < 6 && !improved; ls++)
                        {
                            double factor = 1.0 / (1 << ls);
                            double[] trial =
                            [
                                WrapAngle(state[0] + factor * step[0]),
                                state[1] + factor * step[1],
                                state[2] + factor * step[2]
                            ];
                            if (!Evaluate(trial[0], trial[1], trial[2], regularize, out SurveyPoint candidate, out double[] trialResidual, out double trialPositionNorm2))
                            {
                                continue;
                            }
                            double trialObjective = Norm2(trialResidual);
                            if (trialObjective < bestObjective)
                            {
                                UpdateBest(trial, candidate, trialResidual, trialPositionNorm2);
                                improved = true;
                            }
                        }
                        if (!improved)
                        {
                            damping *= 10.0;
                        }
                    }
                    if (!improved)
                    {
                        break;
                    }
                }

                if (bestPositionNorm2Local > solveTol * solveTol)
                {
                    const int dimension = 3;
                    const int simplexSize = dimension + 1;
                    double[][] simplex = new double[simplexSize][];
                    SurveyPoint[] simplexCandidates = new SurveyPoint[simplexSize];
                    double[][] simplexResiduals = new double[simplexSize][];
                    double[] simplexObjectives = new double[simplexSize];
                    double[] simplexPositionNorm2 = new double[simplexSize];

                    bool TryInitializeVertex(int index, double[] candidateState)
                    {
                        if (!Evaluate(candidateState[0], candidateState[1], candidateState[2], regularize, out SurveyPoint candidate, out double[] candidateResidual, out double candidatePositionNorm2))
                        {
                            return false;
                        }
                        simplex[index] = candidateState;
                        simplexCandidates[index] = candidate;
                        simplexResiduals[index] = candidateResidual;
                        simplexObjectives[index] = Norm2(candidateResidual);
                        simplexPositionNorm2[index] = candidatePositionNorm2;
                        UpdateBest(candidateState, candidate, candidateResidual, candidatePositionNorm2);
                        return true;
                    }

                    double[][] simplexSeeds =
                    [
                        (double[])state.Clone(),
                        [WrapAngle(state[0] + 0.4), state[1], state[2]],
                        [state[0], state[1] + 0.5, state[2]],
                        [state[0], state[1], state[2] + 0.12]
                    ];

                    if (!TryInitializeVertex(0, simplexSeeds[0]) &&
                        !TryInitializeVertex(0, initialState))
                    {
                        solution = bestCandidate;
                        bestPositionNorm2 = bestPositionNorm2Local;
                        return Numeric.IsDefined(bestCandidate.Abscissa);
                    }

                    int initialized = 1;
                    for (int i = 1; i < simplexSize; i++)
                    {
                        if (TryInitializeVertex(initialized, simplexSeeds[i]))
                        {
                            initialized++;
                        }
                    }

                    while (initialized < simplexSize)
                    {
                        double offset = 0.15 * initialized;
                        double[] fallback =
                        [
                            WrapAngle(initialState[0] + offset),
                            initialState[1] + 0.2 * initialized,
                            initialState[2] + 0.05 * initialized
                        ];
                        if (!TryInitializeVertex(initialized, fallback))
                        {
                            break;
                        }
                        initialized++;
                    }

                    if (initialized == simplexSize)
                    {
                        const double alpha = 1.0;
                        const double gamma = 2.0;
                        const double rho = 0.5;
                        const double sigma = 0.5;

                        for (int iter = 0; iter < 80 && bestPositionNorm2Local > solveTol * solveTol; iter++)
                        {
                            int[] order = Enumerable.Range(0, simplexSize)
                                .OrderBy(idx => simplexObjectives[idx])
                                .ToArray();

                            double spread = simplexObjectives[order[^1]] - simplexObjectives[order[0]];
                            if (spread < 1e-14)
                            {
                                break;
                            }

                            int bestIdx = order[0];
                            int worstIdx = order[^1];
                            int secondWorstIdx = order[^2];

                            double[] centroid = new double[dimension];
                            for (int j = 0; j < dimension; j++)
                            {
                                double sum = 0.0;
                                for (int oi = 0; oi < simplexSize - 1; oi++)
                                {
                                    sum += simplex[order[oi]][j];
                                }
                                centroid[j] = sum / dimension;
                            }
                            centroid[0] = WrapAngle(centroid[0]);

                            bool TryAccept(int targetIdx, double[] candidateState)
                            {
                                candidateState[0] = WrapAngle(candidateState[0]);
                                if (!Evaluate(candidateState[0], candidateState[1], candidateState[2], regularize, out SurveyPoint candidate, out double[] candidateResidual, out double candidatePositionNorm2))
                                {
                                    return false;
                                }
                                simplex[targetIdx] = candidateState;
                                simplexCandidates[targetIdx] = candidate;
                                simplexResiduals[targetIdx] = candidateResidual;
                                simplexObjectives[targetIdx] = Norm2(candidateResidual);
                                simplexPositionNorm2[targetIdx] = candidatePositionNorm2;
                                UpdateBest(candidateState, candidate, candidateResidual, candidatePositionNorm2);
                                return true;
                            }

                            double[] reflected =
                            [
                                centroid[0] + alpha * (centroid[0] - simplex[worstIdx][0]),
                                centroid[1] + alpha * (centroid[1] - simplex[worstIdx][1]),
                                centroid[2] + alpha * (centroid[2] - simplex[worstIdx][2])
                            ];

                            if (TryAccept(worstIdx, reflected))
                            {
                                if (simplexObjectives[worstIdx] < simplexObjectives[bestIdx])
                                {
                                    double[] expandedState =
                                    [
                                        centroid[0] + gamma * (simplex[worstIdx][0] - centroid[0]),
                                        centroid[1] + gamma * (simplex[worstIdx][1] - centroid[1]),
                                        centroid[2] + gamma * (simplex[worstIdx][2] - centroid[2])
                                    ];
                                    if (TryAccept(worstIdx, expandedState) && simplexObjectives[worstIdx] > simplexObjectives[bestIdx])
                                    {
                                        TryAccept(worstIdx, reflected);
                                    }
                                }
                                else if (simplexObjectives[worstIdx] >= simplexObjectives[secondWorstIdx])
                                {
                                    double[] contractedState =
                                    [
                                        centroid[0] + rho * (simplex[worstIdx][0] - centroid[0]),
                                        centroid[1] + rho * (simplex[worstIdx][1] - centroid[1]),
                                        centroid[2] + rho * (simplex[worstIdx][2] - centroid[2])
                                    ];
                                    if (!TryAccept(worstIdx, contractedState))
                                    {
                                        for (int oi = 1; oi < simplexSize; oi++)
                                        {
                                            int idx = order[oi];
                                            double[] shrunk =
                                            [
                                                simplex[bestIdx][0] + sigma * (simplex[idx][0] - simplex[bestIdx][0]),
                                                simplex[bestIdx][1] + sigma * (simplex[idx][1] - simplex[bestIdx][1]),
                                                simplex[bestIdx][2] + sigma * (simplex[idx][2] - simplex[bestIdx][2])
                                            ];
                                            TryAccept(idx, shrunk);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                double[] contractedState =
                                [
                                    centroid[0] + rho * (simplex[worstIdx][0] - centroid[0]),
                                    centroid[1] + rho * (simplex[worstIdx][1] - centroid[1]),
                                    centroid[2] + rho * (simplex[worstIdx][2] - centroid[2])
                                ];
                                if (!TryAccept(worstIdx, contractedState))
                                {
                                    for (int oi = 1; oi < simplexSize; oi++)
                                    {
                                        int idx = order[oi];
                                        double[] shrunk =
                                        [
                                            simplex[bestIdx][0] + sigma * (simplex[idx][0] - simplex[bestIdx][0]),
                                            simplex[bestIdx][1] + sigma * (simplex[idx][1] - simplex[bestIdx][1]),
                                            simplex[bestIdx][2] + sigma * (simplex[idx][2] - simplex[bestIdx][2])
                                        ];
                                        TryAccept(idx, shrunk);
                                    }
                                }
                            }
                        }
                    }
                }

                bestPositionNorm2 = bestPositionNorm2Local;
                solution = bestCandidate;
                return true;
            }

            bool TryLowCurvatureFallback(out SurveyPoint bestCandidate, out double bestCandidateResidual)
            {
                bestCandidate = new SurveyPoint();
                bestCandidateResidual = double.PositiveInfinity;

                double caSegmentLength = System.Math.Max(chord, caLength - s1);
                double caDoglegEstimate = caCurvature * caSegmentLength;
                if (!hasCaSeed || caDoglegEstimate > lowDoglegThreshold)
                {
                    return false;
                }

                List<(double phi, double kappa, double length)> lowCurvatureSeeds =
                [
                    (caToolface, caCurvature, caSegmentLength),
                    (caToolface + Numeric.PI, caCurvature, caSegmentLength),
                    (caToolface, System.Math.Max(1e-7, 0.5 * caCurvature), caSegmentLength),
                    (caToolface + Numeric.PI, System.Math.Max(1e-7, 0.5 * caCurvature), caSegmentLength),
                    (caToolface, 1.5 * caCurvature, caSegmentLength),
                    (caToolface + Numeric.PI, 1.5 * caCurvature, caSegmentLength),
                    (caToolface - 0.15, caCurvature, caSegmentLength),
                    (caToolface + 0.15, caCurvature, caSegmentLength),
                    (caToolface + Numeric.PI - 0.15, caCurvature, caSegmentLength),
                    (caToolface + Numeric.PI + 0.15, caCurvature, caSegmentLength),
                    (caToolface, caCurvature, System.Math.Max(chord, 0.98 * caSegmentLength)),
                    (caToolface + Numeric.PI, caCurvature, System.Math.Max(chord, 0.98 * caSegmentLength)),
                    (caToolface, caCurvature, 1.02 * caSegmentLength),
                    (caToolface + Numeric.PI, caCurvature, 1.02 * caSegmentLength)
                ];

                double lineInclination = Numeric.AcosEqual(dz / chord);
                double lineAzimuth = WrapAngle(System.Math.Atan2(dy, dx));
                SurveyPoint lineSurvey = new()
                {
                    Abscissa = s1 + chord,
                    Inclination = lineInclination,
                    Azimuth = lineAzimuth
                };
                CalculateCurvaturesToolfaceVerticalSection(lineSurvey, TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod);
                if (lineSurvey.Toolface != null)
                {
                    double lineToolface = WrapAngle(lineSurvey.Toolface.Value);
                    double lineCurvature = (lineSurvey.Curvature != null && Numeric.GT(lineSurvey.Curvature.Value, 0.0))
                        ? lineSurvey.Curvature.Value
                        : caCurvature;
                    lowCurvatureSeeds.Add((lineToolface, lineCurvature, caSegmentLength));
                    lowCurvatureSeeds.Add((lineToolface + Numeric.PI, lineCurvature, caSegmentLength));
                    lowCurvatureSeeds.Add((lineToolface, System.Math.Max(1e-7, 0.5 * lineCurvature), caSegmentLength));
                    lowCurvatureSeeds.Add((lineToolface + Numeric.PI, System.Math.Max(1e-7, 0.5 * lineCurvature), caSegmentLength));
                    lowCurvatureSeeds.Add((lineToolface - 0.15, lineCurvature, caSegmentLength));
                    lowCurvatureSeeds.Add((lineToolface + 0.15, lineCurvature, caSegmentLength));
                    lowCurvatureSeeds.Add((lineToolface + Numeric.PI - 0.15, lineCurvature, caSegmentLength));
                    lowCurvatureSeeds.Add((lineToolface + Numeric.PI + 0.15, lineCurvature, caSegmentLength));
                }

                bool found = false;
                foreach (var seed in lowCurvatureSeeds)
                {
                    SurveyPoint candidate = new();
                    double candidateResidual = double.PositiveInfinity;
                    bool solved = false;

                    if (TrySolve(seed.phi, seed.kappa, seed.length, false, out SurveyPoint directCandidate, out double directResidual))
                    {
                        candidate = directCandidate;
                        candidateResidual = directResidual;
                        solved = true;
                    }

                    if (TrySolve(seed.phi, seed.kappa, seed.length, true, out SurveyPoint candidateRegularized, out double residualRegularized))
                    {
                        double candidateLength = candidateRegularized.Abscissa!.Value - s1;
                        double candidateCurvature = candidateRegularized.Curvature ?? seed.kappa;
                        SurveyPoint refinedCandidate = candidateRegularized;
                        double refinedResidual = residualRegularized;
                        if (TrySolve(candidateRegularized.Toolface ?? seed.phi, candidateCurvature, candidateLength, false, out SurveyPoint candidateUnregularized, out double residualUnregularized))
                        {
                            refinedCandidate = candidateUnregularized;
                            refinedResidual = residualUnregularized;
                        }
                        if (!solved || refinedResidual < candidateResidual)
                        {
                            candidate = refinedCandidate;
                            candidateResidual = refinedResidual;
                            solved = true;
                        }
                    }

                    if (!solved || candidate.Abscissa == null)
                    {
                        continue;
                    }

                    double candidateCurvatureForScore = candidate.Curvature ?? seed.kappa;

                    if (candidateResidual <= exactTol)
                    {
                        TryAddSolution(candidate);
                    }

                    if (!found ||
                        candidateResidual < bestCandidateResidual - 1e-12 ||
                        (System.Math.Abs(candidateResidual - bestCandidateResidual) <= 1e-12 &&
                         candidateCurvatureForScore < (bestCandidate.Curvature ?? double.PositiveInfinity) - 1e-12))
                    {
                        bestCandidate = candidate;
                        bestCandidateResidual = candidateResidual;
                        found = true;
                    }
                }

                return found;
            }

            if (TryLowCurvatureFallback(out SurveyPoint lowCurvatureCandidate, out double lowCurvatureResidual) &&
                lowCurvatureCandidate.Abscissa != null &&
                lowCurvatureCandidate.Inclination != null &&
                lowCurvatureCandidate.Azimuth != null &&
                lowCurvatureResidual <= acceptTol * acceptTol)
            {
                next.Abscissa = lowCurvatureCandidate.Abscissa;
                next.Inclination = lowCurvatureCandidate.Inclination;
                next.Azimuth = lowCurvatureCandidate.Azimuth;
                next.X = targetX;
                next.Y = targetY;
                next.Z = targetZ;
                return true;
            }

            List<(double phi, double kappa, double length)> seeds = [];
            void AddSeed(double phi, double kappa, double length)
            {
                if (!Numeric.GT(kappa, 0.0) || !Numeric.GT(length, 0.0))
                {
                    return;
                }
                seeds.Add((WrapAngle(phi), kappa, System.Math.Max(chord, length)));
            }
            void AddEndpointDerivedSeeds(double length, double inclination, double azimuth)
            {
                SurveyPoint endpoint = new()
                {
                    Abscissa = s1 + System.Math.Max(chord, length),
                    Inclination = inclination,
                    Azimuth = WrapAngle(azimuth)
                };
                CalculateCurvaturesToolfaceVerticalSection(endpoint, TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod);
                if (endpoint.Toolface != null)
                {
                    double endpointToolface = WrapAngle(endpoint.Toolface.Value);
                    double endpointCurvature = endpoint.Curvature != null && Numeric.GT(endpoint.Curvature.Value, 0.0)
                        ? endpoint.Curvature.Value
                        : caCurvature;
                    AddSeed(endpointToolface, endpointCurvature, length);
                    AddSeed(endpointToolface + Numeric.PI, endpointCurvature, length);
                    AddSeed(endpointToolface + Numeric.PI / 2.0, endpointCurvature, length);
                    AddSeed(endpointToolface - Numeric.PI / 2.0, endpointCurvature, length);
                    AddSeed(endpointToolface - 0.35, endpointCurvature, length);
                    AddSeed(endpointToolface + 0.35, endpointCurvature, length);
                    AddSeed(endpointToolface + Numeric.PI - 0.35, endpointCurvature, length);
                    AddSeed(endpointToolface + Numeric.PI + 0.35, endpointCurvature, length);
                    AddSeed(endpointToolface + Numeric.PI / 2.0 - 0.35, endpointCurvature, length);
                    AddSeed(endpointToolface + Numeric.PI / 2.0 + 0.35, endpointCurvature, length);
                    AddSeed(endpointToolface - Numeric.PI / 2.0 - 0.35, endpointCurvature, length);
                    AddSeed(endpointToolface - Numeric.PI / 2.0 + 0.35, endpointCurvature, length);
                }
            }
            void AddEndpointDerivedSeedNeighborhood(double length, double inclination, double azimuth)
            {
                foreach (double inclinationOffset in new[] { -0.08, -0.03, 0.0, 0.03, 0.08 })
                {
                    foreach (double azimuthOffset in new[] { -0.20, -0.08, 0.0, 0.08, 0.20 })
                    {
                        double perturbedInclination = inclination + inclinationOffset;
                        if (perturbedInclination <= 0.0 || perturbedInclination >= Numeric.PI)
                        {
                            continue;
                        }
                        AddEndpointDerivedSeeds(length, perturbedInclination, azimuth + azimuthOffset);
                    }
                }
            }
            void AddZeroCrossingSeeds(double length, double canonicalInclination, double canonicalAzimuth)
            {
                if (!Numeric.GT(length, chord) && !Numeric.EQ(length, chord))
                {
                    return;
                }
                if (!Numeric.LT(i1, 0.30) || !Numeric.GT(canonicalInclination, 0.20))
                {
                    return;
                }

                double rawInclination = -canonicalInclination;
                double rawAzimuth = WrapAngle(canonicalAzimuth - Numeric.PI);
                double beta = (rawInclination - i1) / length;
                if (!Numeric.LT(beta, 0.0))
                {
                    return;
                }

                foreach (double curvatureCandidate in new[]
                {
                    System.Math.Max(System.Math.Abs(beta) + 1e-6, 0.8 * caCurvature),
                    System.Math.Max(System.Math.Abs(beta) + 1e-6, caCurvature),
                    System.Math.Max(System.Math.Abs(beta) + 1e-6, 1.2 * caCurvature),
                    System.Math.Max(System.Math.Abs(beta) + 1e-6, 1.6 * caCurvature)
                })
                {
                    if (!Numeric.GE(curvatureCandidate, System.Math.Abs(beta)))
                    {
                        continue;
                    }
                    double ratio = System.Math.Max(-1.0, System.Math.Min(1.0, beta / curvatureCandidate));
                    double basePhi = System.Math.Acos(ratio);
                    foreach (double phi in new[]
                    {
                        basePhi,
                        2.0 * Numeric.PI - basePhi,
                        basePhi - 0.20,
                        basePhi + 0.20,
                        2.0 * Numeric.PI - basePhi - 0.20,
                        2.0 * Numeric.PI - basePhi + 0.20
                    })
                    {
                        AddSeed(phi, curvatureCandidate, length);
                    }
                }

                SurveyPoint rawEndpoint = new()
                {
                    Abscissa = s1 + length,
                    Inclination = rawInclination,
                    Azimuth = rawAzimuth
                };
                CalculateCurvaturesToolfaceVerticalSection(rawEndpoint, TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod);
                if (rawEndpoint.Toolface != null)
                {
                    double endpointCurvature = rawEndpoint.Curvature != null && Numeric.GT(rawEndpoint.Curvature.Value, 0.0)
                        ? System.Math.Max(System.Math.Abs(beta) + 1e-6, rawEndpoint.Curvature.Value)
                        : System.Math.Max(System.Math.Abs(beta) + 1e-6, caCurvature);
                    double endpointToolface = WrapAngle(rawEndpoint.Toolface.Value);
                    foreach (double phi in new[]
                    {
                        endpointToolface,
                        endpointToolface + Numeric.PI,
                        endpointToolface - 0.20,
                        endpointToolface + 0.20,
                        endpointToolface + Numeric.PI - 0.20,
                        endpointToolface + Numeric.PI + 0.20
                    })
                    {
                        AddSeed(phi, endpointCurvature, length);
                    }
                }
            }
            AddSeed(caToolface, caCurvature, caLength - s1);
            AddSeed(caToolface + Numeric.PI, caCurvature, caLength - s1);
            AddSeed(caToolface + Numeric.PI / 2.0, caCurvature, caLength - s1);
            AddSeed(caToolface - Numeric.PI / 2.0, caCurvature, caLength - s1);
            AddSeed(caToolface, 0.8 * caCurvature, System.Math.Max(chord, 1.1 * (caLength - s1)));
            AddSeed(caToolface + Numeric.PI, 0.8 * caCurvature, System.Math.Max(chord, 1.1 * (caLength - s1)));
            AddSeed(caToolface + Numeric.PI / 2.0, 0.8 * caCurvature, System.Math.Max(chord, 1.1 * (caLength - s1)));
            AddSeed(caToolface - Numeric.PI / 2.0, 0.8 * caCurvature, System.Math.Max(chord, 1.1 * (caLength - s1)));
            AddSeed(caToolface, 1.2 * caCurvature, System.Math.Max(chord, 0.95 * (caLength - s1)));
            AddSeed(caToolface + Numeric.PI, 1.2 * caCurvature, System.Math.Max(chord, 0.95 * (caLength - s1)));
            AddSeed(caToolface + Numeric.PI / 2.0, 1.2 * caCurvature, System.Math.Max(chord, 0.95 * (caLength - s1)));
            AddSeed(caToolface - Numeric.PI / 2.0, 1.2 * caCurvature, System.Math.Max(chord, 0.95 * (caLength - s1)));
            AddSeed(caToolface - 0.5, caCurvature, caLength - s1);
            AddSeed(caToolface + 0.5, caCurvature, caLength - s1);
            AddSeed(caToolface + Numeric.PI - 0.5, caCurvature, caLength - s1);
            AddSeed(caToolface + Numeric.PI + 0.5, caCurvature, caLength - s1);
            AddSeed(caToolface + Numeric.PI / 2.0 - 0.5, caCurvature, caLength - s1);
            AddSeed(caToolface + Numeric.PI / 2.0 + 0.5, caCurvature, caLength - s1);
            AddSeed(caToolface - Numeric.PI / 2.0 - 0.5, caCurvature, caLength - s1);
            AddSeed(caToolface - Numeric.PI / 2.0 + 0.5, caCurvature, caLength - s1);

            if (hasCaSeed && caSeed.Inclination != null && caSeed.Azimuth != null)
            {
                double seedLength = caSeed.Abscissa!.Value - s1;
                double seedInc = caSeed.Inclination.Value;
                double seedAzi = caSeed.Azimuth.Value;
                AddEndpointDerivedSeedNeighborhood(seedLength, seedInc, seedAzi);
                AddEndpointDerivedSeedNeighborhood(seedLength, Numeric.PI - seedInc, seedAzi + Numeric.PI);
                AddEndpointDerivedSeedNeighborhood(seedLength, seedInc, seedAzi + Numeric.PI);
                AddEndpointDerivedSeedNeighborhood(seedLength, Numeric.PI - seedInc, seedAzi);
                AddZeroCrossingSeeds(seedLength, seedInc, seedAzi);
                AddZeroCrossingSeeds(System.Math.Max(chord, 0.85 * seedLength), seedInc, seedAzi);
                AddZeroCrossingSeeds(1.15 * seedLength, seedInc, seedAzi);
            }

            List<(SurveyPoint candidate, double residual, double length, double curvature)> directCandidates = [];
            SurveyPoint? best = null;
            double bestResidual = double.PositiveInfinity;
            double bestLength = double.PositiveInfinity;
            double bestCurvature = double.PositiveInfinity;

            void TrackCandidate(SurveyPoint candidate, double candidateResidual, double candidateLength, double candidateCurvature)
            {
                if (candidate.Abscissa == null || candidate.Inclination == null || candidate.Azimuth == null)
                {
                    return;
                }

                CurvilinearPoint3D solution = ToSolutionPoint(candidate);
                for (int existingIndex = 0; existingIndex < directCandidates.Count; existingIndex++)
                {
                    if (!EquivalentSolution(ToSolutionPoint(directCandidates[existingIndex].candidate), solution))
                    {
                        continue;
                    }
                    bool keepNew =
                        candidateResidual < directCandidates[existingIndex].residual - 1e-12 ||
                        (System.Math.Abs(candidateResidual - directCandidates[existingIndex].residual) <= 1e-12 &&
                         (candidateLength < directCandidates[existingIndex].length - 1e-9 ||
                          (System.Math.Abs(candidateLength - directCandidates[existingIndex].length) <= 1e-9 &&
                           candidateCurvature < directCandidates[existingIndex].curvature - 1e-12)));
                    if (!keepNew)
                    {
                        return;
                    }
                    directCandidates.RemoveAt(existingIndex);
                    break;
                }

                directCandidates.Add((candidate, candidateResidual, candidateLength, candidateCurvature));
                directCandidates.Sort((left, right) =>
                {
                    int residualCompare = left.residual.CompareTo(right.residual);
                    if (residualCompare != 0)
                    {
                        return residualCompare;
                    }
                    int lengthCompare = left.length.CompareTo(right.length);
                    if (lengthCompare != 0)
                    {
                        return lengthCompare;
                    }
                    return left.curvature.CompareTo(right.curvature);
                });
                if (directCandidates.Count > 6)
                {
                    directCandidates.RemoveRange(6, directCandidates.Count - 6);
                }

                if (candidateResidual <= exactTol)
                {
                    TryAddSolution(candidate);
                }

                if (candidateResidual < bestResidual - 1e-12 ||
                    (System.Math.Abs(candidateResidual - bestResidual) <= 1e-12 &&
                     (candidateLength < bestLength - 1e-9 ||
                      (System.Math.Abs(candidateLength - bestLength) <= 1e-9 &&
                       candidateCurvature < bestCurvature - 1e-12))))
                {
                    best = candidate;
                    bestResidual = candidateResidual;
                    bestLength = candidateLength;
                    bestCurvature = candidateCurvature;
                }
            }

            bool expanded = false;
            bool progress = true;
            while (progress)
            {
                progress = false;
                foreach (var seed in seeds)
                {
                    if (TrySolve(seed.phi, seed.kappa, seed.length, true, out SurveyPoint candidateRegularized, out double residualRegularized))
                    {
                        double candidateLength = candidateRegularized.Abscissa!.Value - s1;
                        double candidateCurvature = candidateRegularized.Curvature ?? seed.kappa;
                        TrackCandidate(candidateRegularized, residualRegularized, candidateLength, candidateCurvature);
                        progress = true;

                        if (TrySolve(candidateRegularized.Toolface ?? seed.phi, candidateCurvature, candidateLength, false, out SurveyPoint candidateUnregularized, out double residualUnregularized))
                        {
                            TrackCandidate(candidateUnregularized, residualUnregularized, candidateUnregularized.Abscissa!.Value - s1, candidateUnregularized.Curvature ?? candidateCurvature);
                        }
                    }
                }

                if ((!progress || best == null || bestResidual > solveTol * solveTol) && !expanded)
                {
                    expanded = true;
                    progress = true;
                    maxSearchLength = expandedSearchLength;
                }
                else
                {
                    break;
                }
            }

            if (collectSolutions)
            {
                foreach (var directCandidate in directCandidates)
                {
                    TryAddSolution(directCandidate.candidate);
                }
                if (solutionList.Count == 0 && best != null)
                {
                    TryAddSolution(best);
                }
            }

            if (best == null || best.Abscissa == null || best.Inclination == null || best.Azimuth == null || bestResidual > acceptTol * acceptTol)
            {
                return false;
            }

            next.Abscissa = best.Abscissa;
            next.Inclination = best.Inclination;
            next.Azimuth = best.Azimuth;
            next.X = targetX;
            next.Y = targetY;
            next.Z = targetZ;
            return true;
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
            double step = GetAdaptiveCDTIncrementStep(DLS, next.Abscissa.Value - Abscissa.Value);
            do
            {
                SurveyPoint tmp = sv1;
                sv1 = sv2;
                sv2 = tmp;
                sv2.Abscissa = Math.Min(next.Abscissa.Value, sv1.Abscissa.Value + step);
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
            if (VerticalSection is not null && X is not null && Y is not null && next.X is not null && next.Y is not null)
            {
                next.VerticalSection = VerticalSection + Math.Sqrt((X.Value - next.X.Value) * (X.Value - next.X.Value) + (Y.Value - next.Y.Value) * (Y.Value - next.Y.Value));
            }
            return true;
        }
        public bool CompleteCDTSDT2(SurveyPoint? next, double DLS, double TF)
        {
            return CompleteCDTSDT(next, DLS, TF);
        }

        public bool CompleteCDTSDT(SurveyPoint? next, double DLS, double TF)
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
            static double NormalizeAzimuth(double angle)
            {
                double twoPi = 2.0 * Numeric.PI;
                angle %= twoPi;
                if (angle < 0.0)
                {
                    angle += twoPi;
                }
                return angle;
            }
            static void CanonicalizeInclinationAzimuth(SurveyPoint point)
            {
                if (point.Inclination == null || point.Azimuth == null)
                {
                    return;
                }
                double inclination = point.Inclination.Value;
                double azimuth = point.Azimuth.Value;
                while (inclination < 0.0)
                {
                    inclination = -inclination;
                    azimuth += Numeric.PI;
                }
                while (inclination > Numeric.PI)
                {
                    inclination = 2.0 * Numeric.PI - inclination;
                    azimuth += Numeric.PI;
                }
                point.Inclination = inclination;
                point.Azimuth = NormalizeAzimuth(azimuth);
            }
            if (Numeric.EQ(Inclination, 0))
            {
                return CompleteCASDT(next, DLS, TF);
            }
            if (Numeric.EQ(Inclination, Math.PI / 2.0))
            {
                return CompleteCASDT(next, DLS, TF);
            }
            // Exact BUR=0 remains the circular-arc special case.
            if (Numeric.EQ(TF, Math.PI / 2.0) || Numeric.EQ(TF, 3.0 * Math.PI / 2.0))
            {
                return CompleteCASDT(next, DLS, TF);
            }
            double beta = DLS * Math.Cos(TF);
            double l = next.Abscissa.Value - Abscissa.Value;
            double gamma = Math.Sqrt(Math.Max(0.0, DLS * DLS - beta * beta));
            int integrationCount = GetAdaptiveCDTIntegrationCount(DLS, l);
            // The closed-form CDT expressions become numerically singular when beta tends to 0:
            // they divide by beta and subtract nearly equal terms. In that regime keep the CDT
            // formulation, but evaluate it numerically with Simpson integration instead of
            // falling back to the incremental CA propagation.
            if (Math.Abs(beta) < 1e-8 || Math.Abs(beta * l) < 1e-6)
            {
                double sinI0 = Math.Sin(Inclination.Value);
                if (Math.Abs(sinI0) < 1e-8)
                {
                    return CompleteCASDT(next, DLS, TF);
                }
                Func<double, double> inclinationAt = s => Inclination.Value + beta * s;
                Func<double, double> azimuthAt = s => Azimuth.Value + gamma * s / sinI0;
                Func<double, double> fxNumerical = s => Math.Sin(inclinationAt(s)) * Math.Cos(azimuthAt(s));
                Func<double, double> fyNumerical = s => Math.Sin(inclinationAt(s)) * Math.Sin(azimuthAt(s));
                Func<double, double> fzNumerical = s => Math.Cos(inclinationAt(s));
                next.X = X.Value + SimpsonRule.IntegrateComposite(fxNumerical, 0, l, integrationCount);
                next.Y = Y.Value + SimpsonRule.IntegrateComposite(fyNumerical, 0, l, integrationCount);
                next.Z = Z.Value + SimpsonRule.IntegrateComposite(fzNumerical, 0, l, integrationCount);
                next.Inclination = Inclination.Value + beta * l;
                next.Azimuth = Azimuth.Value + gamma * l / sinI0;
                CanonicalizeInclinationAzimuth(next);
                next.Curvature = DLS;
                next.Toolface = TF;
                next.BUR = beta;
                next.TUR = gamma / Math.Sin(next.Inclination.Value);
                if (VerticalSection is not null && X is not null && Y is not null && next.X is not null && next.Y is not null)
                {
                    next.VerticalSection = VerticalSection + Math.Sqrt((X.Value - next.X.Value) * (X.Value - next.X.Value) + (Y.Value - next.Y.Value) * (Y.Value - next.Y.Value));
                }
                return true;
            }
            double z = Z.Value + (1.0 / beta) * (Math.Sin(beta * l + Inclination.Value) - Math.Sin(Inclination.Value));
            next.Z = z;
            double A = gamma / beta;
            double C = Azimuth.Value - A * Math.Log(Math.Abs(Math.Tan(0.5 * Inclination.Value)));
            Func<double, double> fx = s => Math.Sin(beta * s + Inclination.Value) * Math.Cos(A * Math.Log(Math.Abs(Math.Tan(0.5 * (beta * s + Inclination.Value)))) + C);
            Func<double, double> fy = s => Math.Sin(beta * s + Inclination.Value) * Math.Sin(A * Math.Log(Math.Abs(Math.Tan(0.5 * (beta * s + Inclination.Value)))) + C);
            next.X = SimpsonRule.IntegrateComposite(fx, 0, l, integrationCount);
            next.Y = SimpsonRule.IntegrateComposite(fy, 0, l, integrationCount);
            next.Inclination = Inclination.Value + beta * l;
            next.Curvature = DLS;
            next.Toolface = TF;
            next.BUR = beta;
            double rawEndInclination = next.Inclination.Value;
            next.TUR = gamma / Math.Sin(rawEndInclination);
            double r = gamma / beta;
            double alpha1 = Azimuth.Value + r * Math.Log(Math.Abs(Math.Tan(0.5 * rawEndInclination))) - r * Math.Log(Math.Tan(0.5 * Inclination.Value));
            double alpha2 = Azimuth.Value - r * Math.Log(Math.Abs(Math.Tan(0.5 * rawEndInclination))) + r * Math.Log(Math.Tan(0.5 * Inclination.Value));
            next.Azimuth = alpha1;
            CanonicalizeInclinationAzimuth(next);
            if (VerticalSection is not null && X is not null && Y is not null && next.X is not null && next.Y is not null)
            {
                next.VerticalSection = VerticalSection + Math.Sqrt((X.Value - next.X.Value) * (X.Value - next.X.Value) + (Y.Value - next.Y.Value) * (Y.Value - next.Y.Value));
            }

            return true;
        }
        public bool InterpolateAtAbscissa(ICurvilinear3D next, double abscissa, ICurvilinear3D result, TrajectoryCalculationType calculationMethod = TrajectoryCalculationType.MinimumCurvatureMethod)
        {
            switch (calculationMethod)
            {
                case TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod:
                    return InterpolateAtAbscissaCDT(next, abscissa, result);
                case TrajectoryCalculationType.ConstantBuildAndTurnMethod:
                    return InterpolateAtAbscissaBT(next, abscissa, result);
                default:
                    return InterpolateAtAbscissaCA(next, abscissa, result);
            }
        }
        public bool InterpolateAtAbscissa(ICurvilinear3D next, double s, SurveyPoint result, TrajectoryCalculationType calculationMethod = TrajectoryCalculationType.MinimumCurvatureMethod)
        {
            if (!InterpolateAtAbscissa(next, s, (ICurvilinear3D)result, calculationMethod))
            {
                return false;
            }
            if (Abscissa != null && Numeric.EQ(s, Abscissa.Value))
            {
                result.Curvature = Curvature;
                result.Toolface = Toolface;
                result.BUR = BUR;
                result.TUR = TUR;
                result.VerticalSection = VerticalSection;
            }
            else if (calculationMethod == TrajectoryCalculationType.MinimumCurvatureMethod &&
                     Abscissa != null &&
                     Inclination != null &&
                     Azimuth != null &&
                     next.Abscissa != null &&
                     next.Inclination != null &&
                     next.Azimuth != null)
            {
                if (next is SurveyPoint nextSurvey && nextSurvey.Curvature != null)
                {
                    result.Curvature = nextSurvey.Curvature;
                }
                else
                {
                    double segmentLength = next.Abscissa.Value - Abscissa.Value;
                    if (Numeric.EQ(segmentLength, 0.0))
                    {
                        result.Curvature = 0.0;
                    }
                    else
                    {
                        double si1 = System.Math.Sin(Inclination.Value);
                        double si2 = System.Math.Sin(next.Inclination.Value);
                        double ci12 = System.Math.Cos(next.Inclination.Value - Inclination.Value);
                        double ca12 = System.Math.Cos(next.Azimuth.Value - Azimuth.Value);
                        double cosine = ci12 - (1.0 - ca12) * si2 * si1;
                        cosine = System.Math.Max(-1.0, System.Math.Min(1.0, cosine));
                        double dogleg = System.Math.Acos(cosine);
                        result.Curvature = dogleg / segmentLength;
                    }
                }
                if (VerticalSection is not null && X is not null && Y is not null && result.X is not null && result.Y is not null)
                {
                    result.VerticalSection = VerticalSection + Math.Sqrt((X.Value - result.X.Value) * (X.Value - result.X.Value) + (Y.Value - result.Y.Value) * (Y.Value - result.Y.Value));
                }
            }
            else if (calculationMethod == TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod &&
                     next is SurveyPoint nextSurvey &&
                     nextSurvey.Curvature != null &&
                     nextSurvey.Toolface != null)
            {
                result.Curvature = nextSurvey.Curvature;
                result.Toolface = nextSurvey.Toolface;

                double beta = nextSurvey.Curvature.Value * System.Math.Cos(nextSurvey.Toolface.Value);
                double gamma = System.Math.Sqrt(System.Math.Max(0.0, nextSurvey.Curvature.Value * nextSurvey.Curvature.Value - beta * beta));
                result.BUR = beta;
                if (result.Inclination != null && !Numeric.EQ(System.Math.Sin(result.Inclination.Value), 0.0))
                {
                    result.TUR = gamma / System.Math.Sin(result.Inclination.Value);
                }
                else
                {
                    result.TUR = null;
                }
                if (VerticalSection is not null && X is not null && Y is not null && result.X is not null && result.Y is not null)
                {
                    result.VerticalSection = VerticalSection + Math.Sqrt((X.Value - result.X.Value) * (X.Value - result.X.Value) + (Y.Value - result.Y.Value) * (Y.Value - result.Y.Value));
                }
            }
            else
            {
                CalculateCurvaturesToolfaceVerticalSection(result, calculationMethod);
            }
            return true;
        }

        /// <summary>
        /// Calculate the coordinates of the result survey station by interpolation between this survey station and the next survey station, for a given curvilinear abscissa
        /// </summary>
        /// <param name="next"></param>
        /// <param name="abscissa"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool InterpolateAtAbscissaCA(ICurvilinear3D next, double abscissa, ICurvilinear3D result)
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
        public bool InterpolateAtAbscissaCDT(ICurvilinear3D next, double abscissa, ICurvilinear3D result)
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

            if (Numeric.EQ(next.Abscissa, Abscissa))
            {
                if (!Numeric.EQ(Abscissa, abscissa))
                {
                    return false;
                }
                result.X = X;
                result.Y = Y;
                result.Z = Z;
                result.Abscissa = Abscissa;
                result.Inclination = Inclination;
                result.Azimuth = Azimuth;
                return true;
            }

            if (next is not SurveyPoint nextSurvey || nextSurvey.Curvature == null || nextSurvey.Toolface == null)
            {
                return false;
            }

            SurveyPoint interpolated = new()
            {
                Abscissa = abscissa
            };
            if (!CompleteCDTSDT(interpolated, nextSurvey.Curvature.Value, nextSurvey.Toolface.Value))
            {
                return false;
            }

            result.X = interpolated.X;
            result.Y = interpolated.Y;
            result.Z = interpolated.Z;
            result.Abscissa = interpolated.Abscissa;
            result.Inclination = interpolated.Inclination;
            result.Azimuth = interpolated.Azimuth;
            return true;
        }
        public bool InterpolateAtAbscissaBT(ICurvilinear3D next, double abscissa, ICurvilinear3D result)
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

            double s1 = Abscissa.Value;
            double s2 = next.Abscissa.Value;
            result.Abscissa = abscissa;

            if (Numeric.EQ(s2, s1))
            {
                if (!Numeric.EQ(s1, abscissa))
                {
                    return false;
                }
                result.X = X;
                result.Y = Y;
                result.Z = Z;
                result.Abscissa = Abscissa;
                result.Inclination = Inclination;
                result.Azimuth = Azimuth;
                return true;
            }

            double bur = (next.Inclination.Value - Inclination.Value) / (s2 - s1);
            double tur = (next.Azimuth.Value - Azimuth.Value) / (s2 - s1);
            double dm = abscissa - s1;

            result.Inclination = Inclination.Value + bur * dm;
            result.Azimuth = Azimuth.Value + tur * dm;

            CurvilinearPoint3D interpolated = new()
            {
                Abscissa = result.Abscissa,
                Inclination = result.Inclination,
                Azimuth = result.Azimuth
            };
            if (!CompleteBTSIA(interpolated))
            {
                return false;
            }

            result.X = interpolated.X;
            result.Y = interpolated.Y;
            result.Z = interpolated.Z;
            result.Inclination = interpolated.Inclination;
            result.Azimuth = interpolated.Azimuth;
            return true;
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
            return Riemannian2DDistanceKarney(latitude2, longitude2);
        }
        public double? Riemannian2DDistanceVincenty(double? latitude2, double? longitude2)
        {
            if (Latitude == null || Longitude == null || latitude2 == null || longitude2 == null)
            {
                return null;
            }

            // Inputs are assumed to be in radians.
            double lat1 = Latitude.Value;
            double lon1 = Longitude.Value;
            double lat2 = latitude2.Value;
            double lon2 = longitude2.Value;

            double a = Constants.EarthSemiMajorAxisWGS84;
            double f = 1.0 / Constants.EarthInverseFlateningWGS84;
            double b = a * (1.0 - f);

            // Reduced latitudes
            double U1 = System.Math.Atan((1.0 - f) * System.Math.Tan(lat1));
            double U2 = System.Math.Atan((1.0 - f) * System.Math.Tan(lat2));

            double sinU1 = System.Math.Sin(U1);
            double cosU1 = System.Math.Cos(U1);
            double sinU2 = System.Math.Sin(U2);
            double cosU2 = System.Math.Cos(U2);

            // Normalize longitude difference to [-pi, pi]
            double L = lon2 - lon1;
            while (L > System.Math.PI) L -= 2.0 * System.Math.PI;
            while (L < -System.Math.PI) L += 2.0 * System.Math.PI;

            // Coincident points
            if (System.Math.Abs(lat1 - lat2) < 1e-15 && System.Math.Abs(L) < 1e-15)
            {
                return 0.0;
            }

            double lambda = L;
            double lambdaPrev;
            int iter = 0;
            const int maxIter = 100;

            double sinSigma = 0.0;
            double cosSigma = 0.0;
            double sigma = 0.0;
            double sinAlpha = 0.0;
            double cosSqAlpha = 0.0;
            double cos2SigmaM = 0.0;

            do
            {
                lambdaPrev = lambda;

                double sinLambda = System.Math.Sin(lambda);
                double cosLambda = System.Math.Cos(lambda);

                double t1 = cosU2 * sinLambda;
                double t2 = cosU1 * sinU2 - sinU1 * cosU2 * cosLambda;

                sinSigma = System.Math.Sqrt(t1 * t1 + t2 * t2);

                // Coincident points
                if (sinSigma == 0.0)
                {
                    return 0.0;
                }

                cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                sigma = System.Math.Atan2(sinSigma, cosSigma);

                sinAlpha = (cosU1 * cosU2 * sinLambda) / sinSigma;
                cosSqAlpha = 1.0 - sinAlpha * sinAlpha;

                // Equatorial line: cosSqAlpha == 0
                if (cosSqAlpha == 0.0)
                {
                    cos2SigmaM = 0.0;
                }
                else
                {
                    cos2SigmaM = cosSigma - (2.0 * sinU1 * sinU2) / cosSqAlpha;
                }

                double C = (f / 16.0) * cosSqAlpha * (4.0 + f * (4.0 - 3.0 * cosSqAlpha));

                lambda = L + (1.0 - C) * f * sinAlpha *
                         (sigma + C * sinSigma *
                         (cos2SigmaM + C * cosSigma *
                         (-1.0 + 2.0 * cos2SigmaM * cos2SigmaM)));

                iter++;
            }
            while (System.Math.Abs(lambda - lambdaPrev) > 1e-12 && iter < maxIter);

            if (iter >= maxIter)
            {
                // Vincenty may fail for nearly antipodal points.
                return null;
            }

            double uSq = cosSqAlpha * (a * a - b * b) / (b * b);

            double A = 1.0 + (uSq / 16384.0) *
                       (4096.0 + uSq * (-768.0 + uSq * (320.0 - 175.0 * uSq)));

            double B = (uSq / 1024.0) *
                       (256.0 + uSq * (-128.0 + uSq * (74.0 - 47.0 * uSq)));

            double deltaSigma =
                B * sinSigma *
                (cos2SigmaM +
                 0.25 * B *
                 (cosSigma * (-1.0 + 2.0 * cos2SigmaM * cos2SigmaM) -
                  (B / 6.0) * cos2SigmaM *
                  (-3.0 + 4.0 * sinSigma * sinSigma) *
                  (-3.0 + 4.0 * cos2SigmaM * cos2SigmaM)));

            double s = b * A * (sigma - deltaSigma);
            return s;
        }

        public double? Riemannian2DDistanceKarney(double? latitude2, double? longitude2)
        {
            if (Latitude == null || Longitude == null || latitude2 == null || longitude2 == null)
            {
                return null;
            }

            // This implementation assumes all angles are stored in radians.
            double lat1Rad = Latitude.Value;
            double lon1Rad = Longitude.Value;
            double lat2Rad = latitude2.Value;
            double lon2Rad = longitude2.Value;

            // Convert to degrees because GeographicLib uses degrees.
            double lat1Deg = lat1Rad * 180.0 / System.Math.PI;
            double lon1Deg = lon1Rad * 180.0 / System.Math.PI;
            double lat2Deg = lat2Rad * 180.0 / System.Math.PI;
            double lon2Deg = lon2Rad * 180.0 / System.Math.PI;

            double a = Constants.EarthSemiMajorAxisWGS84;
            double f = 1.0 / Constants.EarthInverseFlateningWGS84;

            // Karney geodesic on the WGS84 ellipsoid.
            var geod = new GeographicLib.Geodesic(a, f);

            // s12 is the geodesic distance in meters.
            geod.Inverse(lat1Deg, lon1Deg, lat2Deg, lon2Deg, out double s12);

            return s12;
        }

        public static double LatitudeFromMeridianDistance(double s)
        {
            double a = Constants.EarthSemiMajorAxisWGS84;
            double f = 1.0 / Constants.EarthInverseFlateningWGS84;
            double e2 = 2.0 * f - f * f;

            static double MeridianRadiusOfCurvature(double phi, double a, double e2)
            {
                double sinPhi = System.Math.Sin(phi);
                double denom = System.Math.Pow(1.0 - e2 * sinPhi * sinPhi, 1.5);
                return a * (1.0 - e2) / denom;
            }

            static double MeridianDistance(double phi, double a, double e2)
            {
                // Numerical quadrature could be used here, but for Newton
                // it is better to replace this with a meridian arc series.
                int n = 200;
                double h = phi / n;
                double sum = 0.0;

                for (int i = 0; i <= n; i++)
                {
                    double u = i * h;
                    double w = (i == 0 || i == n) ? 1.0 : (i % 2 == 0 ? 2.0 : 4.0);
                    sum += w * MeridianRadiusOfCurvature(u, a, e2);
                }

                return sum * h / 3.0;
            }

            double phi = s / a; // initial guess

            for (int i = 0; i < 10; i++)
            {
                double F = MeridianDistance(phi, a, e2) - s;
                double dF = MeridianRadiusOfCurvature(phi, a, e2);
                double delta = F / dF;
                phi -= delta;

                if (System.Math.Abs(delta) < 1e-14)
                {
                    break;
                }
            }

            return phi;
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
            double latitude = LatitudeFromMeridianDistance(riemannianNorth);
            latitude_ = latitude;
            double sinLat = System.Math.Sin(latitude);
            double cosLat = System.Math.Cos(latitude);
            double R = a * cosLat / System.Math.Sqrt(1 - e2 * sinLat * sinLat);
            if (Numeric.EQ(R, 0))
            {
                longitude_ = null;
            }
            else
            {
                longitude_ = riemannianEast / R;
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
            double R = a * cosLat / System.Math.Sqrt(1 - e2 * sinLat * sinLat);
            base.Y = R * longitude;
            SurveyPoint temp = new();
            temp.latitude_ = 0;
            temp.longitude_ = longitude;
            double? meridianDistance = temp.Riemannian2DDistance(latitude, longitude);
            base.X = meridianDistance == null ? null : System.Math.Sign(latitude) * meridianDistance.Value;
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
