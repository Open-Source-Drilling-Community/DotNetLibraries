using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
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
    public class SurveyPoint : TrajectoryPoint3D
    {
        public static readonly double InterpolationDeltaAbscissa = 0.01;
        public static double CompleteCTCSDT1Step = 0.1;
        public static int CompleteCTCSDT2Count = 1000;
        /// <summary>
        /// Below this magnitude the constant curvature and toolface helpers use their Maclaurin series
        /// instead of the closed form. Chosen so that the two branches agree to well under one ulp.
        /// </summary>
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

            return DoglegAngle(Inclination.Value, Azimuth.Value, next.Inclination.Value, next.Azimuth.Value) / length;
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
        /// gets or sets a possible annotation for the survey point
        /// </summary>
        public string? Annotation { get; set; } = null;
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
                Annotation = src.Annotation;
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
        public static bool InterpolateAtAbscissa<A>(List<A> surveyList, double MD, SurveyPoint interpolatedPoint, TrajectoryCalculationType calculationMethod) where A : SurveyPoint
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
        /// <param name="referenceDepth"></param>
        /// <param name="abscissaList"></param>
        /// <returns></returns>
        public static List<SurveyPoint>? Interpolate<A>(List<A> surveyList, double? mdStep, double? referenceDepth = null, List<(double, string)>? abscissaList = null) where A : SurveyPoint
        {
            return Interpolate(surveyList, mdStep, referenceDepth, TrajectoryCalculationType.MinimumCurvatureMethod, null, abscissaList);
        }
        /// <summary>
        /// Return an interpolated SurveyList. The interpolation step is passed in argument. In addition
        /// interpolations are made at the abscissas given in a list. Additional points can be inserted so that
        /// the distance between the midpoint of the chord and the midpoint of the interpolated arc remains below
        /// a prescribed threshold. The interpolation uses the selected calculation method.
        /// </summary>
        /// <param name="mdStep"></param>
        /// <param name="referenceDepth"></param>
        /// <param name="calculationMethod"></param>
        /// <param name="maxChordMidArcDistance"></param>
        /// <param name="abscissaList"></param>
        /// <returns></returns>
        public static List<SurveyPoint>? Interpolate<A>(List<A> surveyList, double? mdStep, double? referenceDepth, TrajectoryCalculationType calculationMethod, double? maxChordMidArcDistance, List<(double, string)>? abscissaList = null) where A : SurveyPoint
        {
            if (surveyList is { Count: > 1 } &&
                surveyList[0].MD is { } md0 &&
                surveyList.Last<SurveyPoint>().MD is { } mdf)
            {
                double step = mdStep ?? double.NaN;
                bool useStepInterpolation = Numeric.IsDefined(step) &&
                    Numeric.GT(step, 0);
                double referenceDepthValue = referenceDepth ?? md0;

                List<(double Abscissa, string? Annotation)> targetAbscissas = [];
                if (abscissaList != null)
                {
                    foreach ((double s, string annotation) in abscissaList)
                    {
                        if (Numeric.GE(s, md0) && Numeric.LE(s, mdf))
                        {
                            targetAbscissas.Add((s, annotation));
                        }
                    }
                }

                if (useStepInterpolation)
                {
                    double nStart = System.Math.Ceiling((md0 - referenceDepthValue) / step);
                    double nEnd = System.Math.Floor((mdf - referenceDepthValue) / step);
                    for (double n = nStart; Numeric.LE(n, nEnd); n += 1.0)
                    {
                        double s = referenceDepthValue + n * step;
                        if (Numeric.GE(s, md0) && Numeric.LE(s, mdf))
                        {
                            targetAbscissas.Add((s, null));
                        }
                    }
                }

                if (targetAbscissas.Count == 0)
                {
                    return null;
                }

                targetAbscissas.Sort();

                List<(double Abscissa, string? Annotation)> uniqueAbscissas = [];
                foreach ((double s, string? annotation) in targetAbscissas)
                {
                    if (uniqueAbscissas.Count == 0 || !Numeric.EQ(uniqueAbscissas.Last().Abscissa, s))
                    {
                        uniqueAbscissas.Add((s, annotation));
                    }
                    else if (!string.IsNullOrEmpty(annotation))
                    {
                        uniqueAbscissas[^1] = (uniqueAbscissas[^1].Abscissa, annotation);
                    }
                }

                SurveyPoint? CreatePointAtAbscissa(double abscissa)
                {
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
                foreach ((double s, string? annotation) in uniqueAbscissas)
                {
                    SurveyPoint? point = CreatePointAtAbscissa(s);
                    if (point == null)
                    {
                        return null;
                    }
                    if (!string.IsNullOrEmpty(annotation))
                    {
                        point.Annotation = annotation;
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
                // The constant curvature and toolface construction, and the constant build and turn one,
                // both produce the curve defining parameters in closed form, so recomputing them by
                // finite differences here would only replace exact values with an approximation of
                // themselves.
                if (calculationMethod != TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod &&
                    calculationMethod != TrajectoryCalculationType.ConstantBuildAndTurnMethod)
                {
                    // The circular arc construction already reports the curvature, which is constant
                    // along the arc, and the toolface angle at the start of the arc. The finite
                    // difference pass is still wanted for the local build up and turn rates, which do
                    // vary along a circular arc, so it runs and those two are put back afterwards.
                    double? exactCurvature = next.Curvature;
                    double? exactToolface = next.Toolface;
                    CalculateCurvaturesToolfaceVerticalSection(next, calculationMethod);
                    if (exactCurvature != null)
                    {
                        next.Curvature = exactCurvature;
                    }
                    if (exactToolface != null)
                    {
                        next.Toolface = exactToolface;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        protected void CalculateCurvaturesToolfaceVerticalSection(SurveyPoint next, TrajectoryCalculationType calculationMethod = TrajectoryCalculationType.MinimumCurvatureMethod)
        {
            if (Abscissa != null &&
                Inclination != null &&
                next.Abscissa != null &&
                next.Inclination != null)
            {
                double dm = next.Abscissa.Value - Abscissa.Value;
                if (!Numeric.EQ(dm, 0.0) &&
                    (Numeric.EQ(Inclination.Value, 0.0) ||
                     Numeric.EQ(Inclination.Value, Numeric.PI) ||
                     Numeric.EQ(next.Inclination.Value, 0.0) ||
                     Numeric.EQ(next.Inclination.Value, Numeric.PI)))
                {
                    next.BUR = (next.Inclination.Value - Inclination.Value) / dm;
                    next.TUR = 0.0;
                    next.Curvature = System.Math.Abs(next.BUR.Value);
                    next.Toolface = Numeric.GE(next.BUR.Value, 0.0) ? 0.0 : Numeric.PI;

                    if (VerticalSection is not null && X is not null && Y is not null && next.X is not null && next.Y is not null)
                    {
                        next.VerticalSection = VerticalSection + Math.Sqrt((X.Value - next.X.Value) * (X.Value - next.X.Value) + (Y.Value - next.Y.Value) * (Y.Value - next.Y.Value));
                    }
                    return;
                }
            }

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














        public bool CompleteCDTSIA(CurvilinearPoint3D next, out List<SurveyPoint> solutions)
        {
            return CompleteCDTSIA(next, 0, true, out solutions);
        }


        public bool CompleteCDTSIA(CurvilinearPoint3D next, int branch, bool unwrapAzimuth, out List<SurveyPoint> solutions)
        {
            bool ok = CompleteCDTSIAInternal(next, branch, unwrapAzimuth, out List<TrajectoryPoint3D> found, true);
            solutions = AsSurveyPoints(found);
            return ok;
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
            bool constantCurvatureAndToolface = calculationMethod == TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod;
            if (constantCurvatureAndToolface)
            {
                // Cleared so that a value left over from an earlier use cannot be mistaken for one
                // this call produced.
                next.Curvature = null;
                next.Toolface = null;
                next.BUR = null;
                next.TUR = null;
            }
            if (CompleteFromXYZ((CurvilinearPoint3D)next, calculationMethod))
            {
                // The direct target point solve reports the curve defining parameters in closed form,
                // so only fall back on finite differences when it did not reach a solution and the
                // general search below it was used instead.
                if (!constantCurvatureAndToolface || next.Curvature == null || next.Toolface == null)
                {
                    CalculateCurvaturesToolfaceVerticalSection(next, calculationMethod);
                }
                return true;
            }
            else
            {
                return false;
            }
        }










        public bool CompleteCDTXYZ(CurvilinearPoint3D next, out List<SurveyPoint> solutions)
        {
            bool ok = CompleteCDTXYZInternal(next, out List<TrajectoryPoint3D> found, true);
            solutions = AsSurveyPoints(found);
            return ok;
        }

        /// <summary>
        /// The curve constructions live on the base class and hand back trajectory points. These
        /// overloads keep the published signatures of this class, so the stations are carried over.
        /// </summary>
        private static List<SurveyPoint> AsSurveyPoints(List<TrajectoryPoint3D> points)
        {
            List<SurveyPoint> converted = new List<SurveyPoint>();
            if (points == null)
            {
                return converted;
            }
            foreach (TrajectoryPoint3D point in points)
            {
                SurveyPoint survey = new SurveyPoint();
                survey.Set(point);
                survey.SetCurveParameters(point);
                converted.Add(survey);
            }
            return converted;
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
            // A constant curvature and toolface curve is undefined at exactly vertical, in either
            // direction. Everything between is ordinary: an inclination of PI/2 is a perfectly regular
            // point on the curve and must not fall back on a single circular arc, which is a different
            // curve entirely.
            if (Numeric.EQ(Inclination, 0) || Numeric.EQ(Inclination, Numeric.PI))
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
                        result.Curvature = DoglegAngle(Inclination.Value, Azimuth.Value, next.Inclination.Value, next.Azimuth.Value) / segmentLength;
                    }
                }
                if (VerticalSection is not null && X is not null && Y is not null && result.X is not null && result.Y is not null)
                {
                    result.VerticalSection = VerticalSection + Math.Sqrt((X.Value - result.X.Value) * (X.Value - result.X.Value) + (Y.Value - result.Y.Value) * (Y.Value - result.Y.Value));
                }

                double? curvature = result.Curvature;
                double? toolface = result.Toolface;
                double? verticalSection = result.VerticalSection;
                CalculateCurvaturesToolfaceVerticalSection(result, calculationMethod);
                result.Curvature = curvature;
                if (toolface != null)
                {
                    result.Toolface = toolface;
                }
                result.VerticalSection = verticalSection;
            }
            else if (calculationMethod == TrajectoryCalculationType.ConstantBuildAndTurnMethod)
            {
                // The constant build and turn interpolation already reports the rates of the segment,
                // which are its defining constants and so exact. Only fill in what it could not supply,
                // which happens when the far station is not itself a survey point.
                if (result.BUR == null && GetBTSegmentRates(next, out double segmentBur, out double segmentTur))
                {
                    result.BUR = segmentBur;
                    result.TUR = segmentTur;
                    double sinInclination = result.Inclination != null ? System.Math.Sin(result.Inclination.Value) : 0.0;
                    result.Curvature = Hypot(segmentBur, segmentTur * sinInclination);
                    result.Toolface = System.Math.Atan2(segmentTur * sinInclination, segmentBur);
                }
                if (result.VerticalSection == null &&
                    VerticalSection is not null && X is not null && Y is not null &&
                    result.X is not null && result.Y is not null)
                {
                    result.VerticalSection = VerticalSection + Math.Sqrt((X.Value - result.X.Value) * (X.Value - result.X.Value) + (Y.Value - result.Y.Value) * (Y.Value - result.Y.Value));
                }
            }
            else if (calculationMethod == TrajectoryCalculationType.ConstantCurvatureAndToolfaceMethod)
            {
                // The constant curvature and toolface interpolation already reports the parameters that
                // hold at the interpolated point, which on the straight continuation past a vertical
                // crossing are not those of the segment as a whole. Only fill in what it could not
                // supply, which happens when the far station is not itself a survey point.
                if (result.Curvature == null &&
                    GetCDTSegmentParameters(next, out double segmentCurvature, out double segmentToolface))
                {
                    result.Curvature = segmentCurvature;
                    result.Toolface = segmentToolface;
                    result.BUR = segmentCurvature * System.Math.Cos(segmentToolface);
                    double sinInclination = result.Inclination != null ? System.Math.Sin(result.Inclination.Value) : 0.0;
                    result.TUR = Numeric.EQ(sinInclination, 0.0)
                        ? null
                        : segmentCurvature * System.Math.Sin(segmentToolface) / sinInclination;
                }
                if (result.VerticalSection == null &&
                    VerticalSection is not null && X is not null && Y is not null &&
                    result.X is not null && result.Y is not null)
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
