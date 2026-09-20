using System;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

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
        public TrajectoryPoint3D Intermediate { get; set; } = new TrajectoryPoint3D();
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

        /// <summary>
        /// Work out the two arcs of equal curvature that lead from the start station to the end station,
        /// both of them fully defined in position and attitude.
        ///
        /// The construction rests on one property of a circular arc: its chord makes the same angle with
        /// the tangent at each end, so the chord is parallel to the sum of the two unit tangents. Writing
        /// the junction tangent as m, the two chords are therefore along t0 + m and m + t1, and an arc of
        /// radius r turning through an angle a has a chord of length 2*r*sin(a/2). The displacement from
        /// the start to the end station is the sum of the two chords, so
        ///
        ///     p1 - p0 = r * ( tan(a/2)*(t0 + m) + tan(b/2)*(m + t1) )
        ///
        /// with a the angle between t0 and m and b the angle between m and t1. Everything inside the
        /// bracket depends on the junction tangent alone, and the radius appears only as a scale on it.
        /// So the junction tangent has to make that bracket parallel to the chord of the whole section,
        /// which is two equations in the two degrees of freedom of a unit vector, and the radius then
        /// follows by dividing the two lengths. Two unknowns and a radius in closed form, in place of
        /// the three unknown coordinates of the junction point that were fitted before.
        ///
        /// The bracket is constant as the junction tangent sweeps the plane of t0 and t1, taking the
        /// value tan(psi)*(t0 + t1) there with psi half the angle between the tangents. A double arc of
        /// equal curvature is therefore planar only when the chord happens to bisect the two tangents,
        /// and in every other case the junction tangent has to leave that plane. This also means the
        /// problem is stationary across the plane, so the solution is looked for over the whole sphere
        /// of directions rather than from a single starting guess, and there may be none at all or
        /// several. When there are several the gentlest is kept.
        /// </summary>
        public bool CalculateXYZ()
        {
            if (Start == null || End == null ||
                !Numeric.IsDefined(Start.X) || !Numeric.IsDefined(Start.Y) || !Numeric.IsDefined(Start.Z) ||
                !Numeric.IsDefined(Start.Inclination) || !Numeric.IsDefined(Start.Azimuth) ||
                !Numeric.IsDefined(End.X) || !Numeric.IsDefined(End.Y) || !Numeric.IsDefined(End.Z) ||
                !Numeric.IsDefined(End.Inclination) || !Numeric.IsDefined(End.Azimuth))
            {
                return false;
            }

            // A single arc reaching the end station with the right attitude is the answer whenever there
            // is one: the two arcs are then the two halves of it and carry the same toolface angle.
            CircularArcSection single = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
            single.Start.Set(Start);
            single.End.Set(End);
            double singleArcMiss = double.PositiveInfinity;
            if (single.CalculateXYZ() &&
                Numeric.IsDefined(single.Circle.Curvature) &&
                Numeric.IsDefined(single.End.Inclination) && Numeric.IsDefined(single.End.Azimuth))
            {
                singleArcMiss = AttitudeMiss(single.End, End);
            }
            if (singleArcMiss <= AttitudeAccuracy)
            {
                return ReportSingleArc(single);
            }

            GetTangent(Start, out double t0x, out double t0y, out double t0z);
            GetTangent(End, out double t1x, out double t1y, out double t1z);
            double dx = (double)(End.X - Start.X);
            double dy = (double)(End.Y - Start.Y);
            double dz = (double)(End.Z - Start.Z);
            double chord = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (Numeric.EQ(chord, 0.0))
            {
                // The two stations coincide, so no arc of finite curvature joins them.
                return false;
            }
            dx /= chord;
            dy /= chord;
            dz /= chord;

            // Sweep the sphere of junction tangents coarsely, then settle on each direction that lines the
            // bracket up with the chord. The sweep is what makes this find a solution wherever there is
            // one, instead of depending on where it was started from.
            bool found = false;
            double bestCurvature = double.PositiveInfinity;
            double bestMx = 0.0;
            double bestMy = 0.0;
            double bestMz = 0.0;

            // The sweep is run again on a grid staggered by half a cell if the first one comes back with
            // nothing, which picks up the occasional solution whose basin fell between the samples. It
            // costs nothing in the ordinary case, where the first sweep already has an answer.
            foreach (double offset in SweepOffsets)
            {
                if (found)
                {
                    break;
                }

            // Three rows of the sweep are kept at a time so that a sample can be compared with all of its
            // neighbours, the azimuth wrapping round. Settling is done only from the samples that are
            // better than everything around them, which is one per basin, rather than from every sample
            // that happens to point the right way.
            double[] previous = new double[SweepAzimuthSteps];
            double[] current = new double[SweepAzimuthSteps];
            double[] next = new double[SweepAzimuthSteps];
            // Beyond the first and the last row there is nothing, so those rows compare against a floor
            // and can be the best of their neighbourhood in their own right. Leaving them out would lose
            // any solution lying near the poles of the frame the sweep happens to be written in.
            for (int j = 0; j < SweepAzimuthSteps; j++)
            {
                previous[j] = double.NegativeInfinity;
            }
            FillSweepRow(t0x, t0y, t0z, t1x, t1y, t1z, dx, dy, dz, 0, offset, current);

            for (int i = 0; i < SweepPolarSteps; i++)
            {
                if (i + 1 < SweepPolarSteps)
                {
                    FillSweepRow(t0x, t0y, t0z, t1x, t1y, t1z, dx, dy, dz, i + 1, offset, next);
                }
                else
                {
                    for (int j = 0; j < SweepAzimuthSteps; j++)
                    {
                        next[j] = double.NegativeInfinity;
                    }
                }
                for (int j = 0; j < SweepAzimuthSteps; j++)
                {
                    double alignment = current[j];
                    if (alignment < SweepAlignmentFloor)
                    {
                        continue;
                    }
                    int before = (j + SweepAzimuthSteps - 1) % SweepAzimuthSteps;
                    int after = (j + 1) % SweepAzimuthSteps;
                    if (alignment < current[before] || alignment < current[after] ||
                        alignment < previous[j] || alignment < next[j])
                    {
                        continue;
                    }

                    SweepDirection(i, j, offset, out double mx, out double my, out double mz);
                    if (!Settle(t0x, t0y, t0z, t1x, t1y, t1z, dx, dy, dz, ref mx, ref my, ref mz))
                    {
                        continue;
                    }
                    if (!Bracket(t0x, t0y, t0z, t1x, t1y, t1z, mx, my, mz,
                                 out double sx, out double sy, out double sz))
                    {
                        continue;
                    }
                    double bracket = System.Math.Sqrt(sx * sx + sy * sy + sz * sz);
                    if (!Numeric.GT(bracket, 0.0))
                    {
                        continue;
                    }
                    // The bracket has to point along the chord and not against it.
                    if ((sx * dx + sy * dy + sz * dz) / bracket < 1.0 - SettledAlignmentTolerance)
                    {
                        continue;
                    }
                    double curvature = bracket / chord;
                    if (!Numeric.GT(curvature, 0.0) || curvature >= bestCurvature)
                    {
                        continue;
                    }
                    found = true;
                    bestCurvature = curvature;
                    bestMx = mx;
                    bestMy = my;
                    bestMz = mz;
                }
                double[] rotate = previous;
                previous = current;
                current = next;
                next = rotate;
                }
            }

            if (!found)
            {
                // Nothing came out of the sweep. That happens where the section is so nearly a single
                // arc that the junction is not pinned down at all: every point along it is as good a
                // junction as any other, and there is no isolated direction for the sweep to settle on.
                // Recovering such an arc from its two stations is itself ill conditioned, which is why
                // it did not pass the strict test above, so it is taken here against a tolerance that
                // still sits far below any surveying accuracy. This is a last resort and never displaces
                // a genuine two arc solution, which is looked for first.
                if (singleArcMiss <= AttitudeFallbackAccuracy)
                {
                    return ReportSingleArc(single);
                }
                return false;
            }

            // The junction point sits at the end of the first chord, which is the first term of the
            // bracket scaled by the radius.
            double cosStart = t0x * bestMx + t0y * bestMy + t0z * bestMz;
            double halfTangent = HalfAngleTangent(cosStart);
            double radius = 1.0 / bestCurvature;
            TrajectoryPoint3D junction = new TrajectoryPoint3D();
            junction.Set((double)Start.X + radius * halfTangent * (t0x + bestMx),
                         (double)Start.Y + radius * halfTangent * (t0y + bestMy),
                         (double)Start.Z + radius * halfTangent * (t0z + bestMz));

            // Build the two arcs through that point. Each of them is an ordinary single arc problem, and
            // solving them rather than assembling the parameters by hand keeps the toolface angles and
            // the along hole distances consistent with every other section.
            CircularArcSection first = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
            first.Start.Set(Start);
            first.End.Set(junction);
            if (!first.CalculateXYZ() || !Numeric.IsDefined(first.Circle.Curvature))
            {
                return false;
            }
            CircularArcSection second = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
            second.Start.Set(first.End);
            second.End.Set(End);
            if (!second.CalculateXYZ() || !Numeric.IsDefined(second.Circle.Curvature))
            {
                return false;
            }

            // Nothing is reported until it has been checked. The earlier version accepted whatever the
            // fit came back with, against a tolerance of about a radian, so most of what it returned did
            // not reach the end attitude and did not have the same curvature on both arcs.
            if (AttitudeMiss(second.End, End) > AttitudeAccuracy)
            {
                return false;
            }
            double curvatureFirst = (double)first.Circle.Curvature;
            double curvatureSecond = (double)second.Circle.Curvature;
            double scale = System.Math.Max(System.Math.Abs(curvatureFirst), System.Math.Abs(curvatureSecond));
            if (Numeric.GT(scale, 0.0) &&
                System.Math.Abs(curvatureFirst - curvatureSecond) / scale > CurvatureAccuracy)
            {
                return false;
            }

            Intermediate.Set(first.End);
            End.Set(second.End);
            DoubleArcCurve.Curvature = 0.5 * (curvatureFirst + curvatureSecond);
            DoubleArcCurve.UpstreamReferenceToolface = first.Circle.ReferenceToolface;
            DoubleArcCurve.DownstreamReferenceToolface = second.Circle.ReferenceToolface;
            return true;
        }

        /// <summary>
        /// Report a section that one arc already covers. The two arcs are then the two halves of it, so
        /// they share the toolface angle and the junction sits at the end.
        /// </summary>
        private bool ReportSingleArc(CircularArcSection single)
        {
            DoubleArcCurve.Curvature = single.Circle.Curvature;
            DoubleArcCurve.UpstreamReferenceToolface = single.Circle.ReferenceToolface;
            DoubleArcCurve.DownstreamReferenceToolface = single.Circle.ReferenceToolface;
            End.Set(single.End);
            Intermediate.Set(End);
            return true;
        }

        /// <summary>
        /// The junction tangent at one sample of the sweep.
        /// </summary>
        private static void SweepDirection(int polarIndex, int azimuthIndex, double offset,
                                           out double x, out double y, out double z)
        {
            double polar = Numeric.PI * (polarIndex + 0.5 + offset) / SweepPolarSteps;
            double azimuth = 2.0 * Numeric.PI * (azimuthIndex + offset) / SweepAzimuthSteps;
            double sinPolar = System.Math.Sin(polar);
            x = sinPolar * System.Math.Cos(azimuth);
            y = sinPolar * System.Math.Sin(azimuth);
            z = System.Math.Cos(polar);
        }

        /// <summary>
        /// One row of the sweep, holding how well the bracket lines up with the chord at each sample.
        /// </summary>
        private static void FillSweepRow(double t0x, double t0y, double t0z,
                                         double t1x, double t1y, double t1z,
                                         double dx, double dy, double dz,
                                         int polarIndex, double offset, double[] row)
        {
            for (int j = 0; j < row.Length; j++)
            {
                SweepDirection(polarIndex, j, offset, out double mx, out double my, out double mz);
                if (!Alignment(t0x, t0y, t0z, t1x, t1y, t1z, mx, my, mz, dx, dy, dz, out double alignment))
                {
                    alignment = double.NegativeInfinity;
                }
                row[j] = alignment;
            }
        }

        /// <summary>
        /// How many polar and azimuthal samples the sweep of junction tangents takes. The bracket varies
        /// smoothly over the sphere and carries at most a handful of solutions, so this is about finding
        /// each basin rather than resolving anything, and the settling below does the accurate work.
        /// </summary>
        private const int SweepPolarSteps = 32;
        private static readonly double[] SweepOffsets = new double[] { 0.0, 0.5 };
        private const int SweepAzimuthSteps = 64;

        /// <summary>
        /// How well a sample has to line up with the chord before it is worth settling from.
        /// </summary>
        private const double SweepAlignmentFloor = 0.0;

        /// <summary>
        /// How well a settled direction has to line up before it counts as a solution.
        /// </summary>
        private const double SettledAlignmentTolerance = 1.0e-12;

        /// <summary>
        /// How closely the end attitude has to be reached, in radians.
        /// </summary>
        private const double AttitudeAccuracy = 1.0e-7;

        /// <summary>
        /// How closely the end attitude has to be reached when a single arc is all that is left to fall
        /// back on, in radians. About two ten thousandths of a degree, which is orders below what a
        /// survey resolves, and only ever used where the sweep found no two arc solution at all.
        /// </summary>
        private const double AttitudeFallbackAccuracy = 1.0e-4;

        /// <summary>
        /// How closely the two curvatures have to agree, relative to the larger of them. This is what
        /// makes the section a double arc of one curvature rather than two unrelated arcs.
        /// </summary>
        private const double CurvatureAccuracy = 1.0e-6;

        /// <summary>
        /// The unit tangent of a station, north, east and downward.
        /// </summary>
        private static void GetTangent(TrajectoryPoint3D point, out double x, out double y, out double z)
        {
            double sinInclination = System.Math.Sin((double)point.Inclination);
            x = sinInclination * System.Math.Cos((double)point.Azimuth);
            y = sinInclination * System.Math.Sin((double)point.Azimuth);
            z = System.Math.Cos((double)point.Inclination);
        }

        /// <summary>
        /// tan(a/2) written from cos(a) without going through the angle, which keeps it accurate for the
        /// small turns where most of the length of a section is spent.
        /// </summary>
        private static double HalfAngleTangent(double cosine)
        {
            if (cosine <= -1.0)
            {
                return double.PositiveInfinity;
            }
            if (cosine >= 1.0)
            {
                return 0.0;
            }
            return System.Math.Sqrt((1.0 - cosine) / (1.0 + cosine));
        }

        /// <summary>
        /// The bracket tan(a/2)*(t0 + m) + tan(b/2)*(m + t1), whose direction the junction tangent has to
        /// bring into line with the chord and whose length divided by the chord is the curvature.
        /// </summary>
        private static bool Bracket(double t0x, double t0y, double t0z,
                                    double t1x, double t1y, double t1z,
                                    double mx, double my, double mz,
                                    out double sx, out double sy, out double sz)
        {
            sx = 0.0;
            sy = 0.0;
            sz = 0.0;
            double cosStart = t0x * mx + t0y * my + t0z * mz;
            double cosEnd = mx * t1x + my * t1y + mz * t1z;
            double tangentStart = HalfAngleTangent(cosStart);
            double tangentEnd = HalfAngleTangent(cosEnd);
            if (double.IsInfinity(tangentStart) || double.IsInfinity(tangentEnd))
            {
                // The junction tangent points straight back along one of the two, which is a reversal
                // rather than an arc.
                return false;
            }
            sx = tangentStart * (t0x + mx) + tangentEnd * (mx + t1x);
            sy = tangentStart * (t0y + my) + tangentEnd * (my + t1y);
            sz = tangentStart * (t0z + mz) + tangentEnd * (mz + t1z);
            return true;
        }

        /// <summary>
        /// The cosine of the angle between the bracket and the chord, which is one exactly at a solution.
        /// </summary>
        private static bool Alignment(double t0x, double t0y, double t0z,
                                      double t1x, double t1y, double t1z,
                                      double mx, double my, double mz,
                                      double dx, double dy, double dz,
                                      out double alignment)
        {
            alignment = double.NegativeInfinity;
            if (!Bracket(t0x, t0y, t0z, t1x, t1y, t1z, mx, my, mz, out double sx, out double sy, out double sz))
            {
                return false;
            }
            double length = System.Math.Sqrt(sx * sx + sy * sy + sz * sz);
            if (!Numeric.GT(length, 0.0))
            {
                return false;
            }
            alignment = (sx * dx + sy * dy + sz * dz) / length;
            return true;
        }

        /// <summary>
        /// Settle a junction tangent onto a direction that lines the bracket up with the chord exactly.
        ///
        /// The two unknowns are the two degrees of freedom of a unit vector, taken here as a step in the
        /// plane across the current direction, and the two residuals are the components of the bracket
        /// across the chord. Newton on that pair converges in a few steps from anywhere in a basin; the
        /// step is cut back when it fails to improve, which is what carries it through the flat ground
        /// near the plane of the two tangents.
        /// </summary>
        private static bool Settle(double t0x, double t0y, double t0z,
                                   double t1x, double t1y, double t1z,
                                   double dx, double dy, double dz,
                                   ref double mx, ref double my, ref double mz)
        {
            // A frame across the chord, in which the two residuals are read.
            Orthogonal(dx, dy, dz, out double e1x, out double e1y, out double e1z,
                                   out double e2x, out double e2y, out double e2z);

            bool Residual(double x, double y, double z, out double r1, out double r2)
            {
                r1 = 0.0;
                r2 = 0.0;
                if (!Bracket(t0x, t0y, t0z, t1x, t1y, t1z, x, y, z, out double sx, out double sy, out double sz))
                {
                    return false;
                }
                double length = System.Math.Sqrt(sx * sx + sy * sy + sz * sz);
                if (!Numeric.GT(length, 0.0))
                {
                    return false;
                }
                r1 = (sx * e1x + sy * e1y + sz * e1z) / length;
                r2 = (sx * e2x + sy * e2y + sz * e2z) / length;
                return true;
            }

            if (!Residual(mx, my, mz, out double f1, out double f2))
            {
                return false;
            }
            double norm = f1 * f1 + f2 * f2;

            for (int iteration = 0; iteration < SettleIterations; iteration++)
            {
                if (norm <= SettleTolerance * SettleTolerance)
                {
                    return true;
                }
                // A frame across the current direction, in which the step is taken.
                Orthogonal(mx, my, mz, out double u1x, out double u1y, out double u1z,
                                       out double u2x, out double u2y, out double u2z);

                // The two by two Jacobian, by a central difference. The residuals are trigonometric in
                // the step, so a difference is as good as an analytic derivative here and far shorter.
                if (!Derivative(Residual, mx, my, mz, u1x, u1y, u1z, out double j11, out double j21) ||
                    !Derivative(Residual, mx, my, mz, u2x, u2y, u2z, out double j12, out double j22))
                {
                    return false;
                }
                double determinant = j11 * j22 - j12 * j21;
                if (Numeric.EQ(determinant, 0.0))
                {
                    // Flat ground, which is what the plane of the two tangents looks like. Nothing is
                    // learned here, so this direction is abandoned and the sweep carries on elsewhere.
                    return false;
                }
                double step1 = -(j22 * f1 - j12 * f2) / determinant;
                double step2 = -(-j21 * f1 + j11 * f2) / determinant;

                bool improved = false;
                double damping = 1.0;
                for (int cut = 0; cut < SettleCutbacks; cut++)
                {
                    double tx = mx + damping * (step1 * u1x + step2 * u2x);
                    double ty = my + damping * (step1 * u1y + step2 * u2y);
                    double tz = mz + damping * (step1 * u1z + step2 * u2z);
                    double length = System.Math.Sqrt(tx * tx + ty * ty + tz * tz);
                    if (Numeric.GT(length, 0.0))
                    {
                        tx /= length;
                        ty /= length;
                        tz /= length;
                        if (Residual(tx, ty, tz, out double g1, out double g2))
                        {
                            double trial = g1 * g1 + g2 * g2;
                            if (trial < norm)
                            {
                                mx = tx;
                                my = ty;
                                mz = tz;
                                f1 = g1;
                                f2 = g2;
                                norm = trial;
                                improved = true;
                                break;
                            }
                        }
                    }
                    damping *= 0.5;
                }
                if (!improved)
                {
                    break;
                }
            }
            return norm <= SettleTolerance * SettleTolerance;
        }

        private const int SettleIterations = 40;
        private const int SettleCutbacks = 30;
        /// <summary>
        /// How small the two residuals have to get before the settling is taken to have arrived. The
        /// Jacobian is only read off by a difference, but that costs rate of convergence rather than
        /// final accuracy, so Newton still arrives near the rounding of a double. Stopping earlier was
        /// measured to lose solutions, not save time. It is a criterion for stopping and not a claim
        /// about the answer: what is reported is checked against the end station before it is returned.
        /// </summary>
        private const double SettleTolerance = 1.0e-13;
        private const double SettleStep = 1.0e-6;

        private delegate bool ResidualFunction(double x, double y, double z, out double r1, out double r2);

        /// <summary>
        /// The derivative of the two residuals along one direction of the step frame, centrally.
        /// </summary>
        private static bool Derivative(ResidualFunction residual,
                                       double mx, double my, double mz,
                                       double ux, double uy, double uz,
                                       out double d1, out double d2)
        {
            d1 = 0.0;
            d2 = 0.0;
            double forwardLength = System.Math.Sqrt((mx + SettleStep * ux) * (mx + SettleStep * ux) +
                                                    (my + SettleStep * uy) * (my + SettleStep * uy) +
                                                    (mz + SettleStep * uz) * (mz + SettleStep * uz));
            double backwardLength = System.Math.Sqrt((mx - SettleStep * ux) * (mx - SettleStep * ux) +
                                                     (my - SettleStep * uy) * (my - SettleStep * uy) +
                                                     (mz - SettleStep * uz) * (mz - SettleStep * uz));
            if (!Numeric.GT(forwardLength, 0.0) || !Numeric.GT(backwardLength, 0.0))
            {
                return false;
            }
            if (!residual((mx + SettleStep * ux) / forwardLength,
                          (my + SettleStep * uy) / forwardLength,
                          (mz + SettleStep * uz) / forwardLength, out double a1, out double a2) ||
                !residual((mx - SettleStep * ux) / backwardLength,
                          (my - SettleStep * uy) / backwardLength,
                          (mz - SettleStep * uz) / backwardLength, out double b1, out double b2))
            {
                return false;
            }
            d1 = (a1 - b1) / (2.0 * SettleStep);
            d2 = (a2 - b2) / (2.0 * SettleStep);
            return true;
        }

        /// <summary>
        /// Two unit vectors completing the given one into a right handed frame.
        /// </summary>
        private static void Orthogonal(double x, double y, double z,
                                       out double e1x, out double e1y, out double e1z,
                                       out double e2x, out double e2y, out double e2z)
        {
            // Cross with whichever axis the direction leans on least, so the result never comes out short.
            double ax = 0.0;
            double ay = 0.0;
            double az = 0.0;
            double absX = System.Math.Abs(x);
            double absY = System.Math.Abs(y);
            double absZ = System.Math.Abs(z);
            if (absX <= absY && absX <= absZ)
            {
                ax = 1.0;
            }
            else if (absY <= absZ)
            {
                ay = 1.0;
            }
            else
            {
                az = 1.0;
            }
            e1x = y * az - z * ay;
            e1y = z * ax - x * az;
            e1z = x * ay - y * ax;
            double length = System.Math.Sqrt(e1x * e1x + e1y * e1y + e1z * e1z);
            e1x /= length;
            e1y /= length;
            e1z /= length;
            e2x = y * e1z - z * e1y;
            e2y = z * e1x - x * e1z;
            e2z = x * e1y - y * e1x;
        }

        /// <summary>
        /// How far apart two attitudes are, in radians, measured as the angle between the two tangents so
        /// that the azimuth counts for nothing at the vertical and wraps correctly everywhere else.
        /// </summary>
        private static double AttitudeMiss(TrajectoryPoint3D got, TrajectoryPoint3D wanted)
        {
            if (!Numeric.IsDefined(got.Inclination) || !Numeric.IsDefined(got.Azimuth) ||
                !Numeric.IsDefined(wanted.Inclination) || !Numeric.IsDefined(wanted.Azimuth))
            {
                return double.PositiveInfinity;
            }
            GetTangent(got, out double ax, out double ay, out double az);
            GetTangent(wanted, out double bx, out double by, out double bz);
            double crossX = ay * bz - az * by;
            double crossY = az * bx - ax * bz;
            double crossZ = ax * by - ay * bx;
            double sine = System.Math.Sqrt(crossX * crossX + crossY * crossY + crossZ * crossZ);
            double cosine = ax * bx + ay * by + az * bz;
            return System.Math.Atan2(sine, cosine);
        }

        /// <summary>
        /// Interpolate at MD and return the interpolation
        /// </summary>
        /// <param name="md"></param>
        /// <returns></returns>
        public override CurvilinearPoint3D InterpolateAtMD(double md)
        {
            CurvilinearPoint3D result = new TrajectoryPoint3D();
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
