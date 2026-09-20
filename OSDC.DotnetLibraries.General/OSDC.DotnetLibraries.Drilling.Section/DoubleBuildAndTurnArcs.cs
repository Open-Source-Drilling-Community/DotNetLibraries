using System;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// A section made of two constant build and turn curves joining two stations that are both fully
    /// defined in position and in attitude, with the curvatures of the two curves made equal where they
    /// meet.
    ///
    /// This is the same problem the double arc and the double constant curvature and toolface sections
    /// solve, but the quantity held in common is not the same. A build and turn curve does not have one
    /// curvature: at inclination i it is sqrt(b*b + t*t*sin(i)*sin(i)), which varies as the curve builds.
    /// The junction is the one place where both curves are defined on the same station, so that is where
    /// they are made to agree, and the effect is that the dogleg severity passes through the junction
    /// without a step.
    /// </summary>
    [Serializable]
    public class DoubleBuildAndTurnArcs : ArcSection
    {
        /// <summary>
        /// The station where the two curves meet.
        /// </summary>
        public TrajectoryPoint3D Intermediate { get; set; } = new TrajectoryPoint3D();

        /// <summary>
        /// The pair of curves.
        /// </summary>
        public NonLocalizedDoubleBuildAndTurnCurve DoubleBuildAndTurnCurve { get; set; }
            = new NonLocalizedDoubleBuildAndTurnCurve();

        /// <summary>
        /// the generic accessor to the curve
        /// </summary>
        public override NonLocalizedCurve Curve
        {
            get => DoubleBuildAndTurnCurve;
            set
            {
                if (value is NonLocalizedDoubleBuildAndTurnCurve)
                {
                    DoubleBuildAndTurnCurve = (NonLocalizedDoubleBuildAndTurnCurve)value;
                }
            }
        }

        public DoubleBuildAndTurnArcs()
        {
        }

        public DoubleBuildAndTurnArcs(TrajectoryPoint3D start, TrajectoryPoint3D end)
        {
            Start = start;
            End = end;
        }

        public override bool Calculate()
        {
            return CalculateXYZ();
        }

        /// <summary>
        /// Work out the two build and turn curves that lead from the start station to the end station,
        /// with their curvatures equal where they meet.
        ///
        /// Along a build and turn curve the inclination and the azimuth both run linearly with the along
        /// hole distance, so the curve is settled by how far each of them moves and by how long it is.
        /// Writing that out, the displacement of such a curve is its length times a shape that depends
        /// only on the attitude it starts from and on how much the attitude moves:
        ///
        ///     d = L * F(theta0, alpha0, dTheta, dAlpha)
        ///
        /// The changes of inclination and of azimuth over the two curves have to add up to the change
        /// between the two stations, so choosing how each is split between them leaves only the two
        /// lengths, and those enter the displacement linearly:
        ///
        ///     chord = L1*F1 + L2*F2
        ///
        /// That is three equations in two lengths. It is short by one, which is exactly the one
        /// parameter family such a pair has, and is why a further condition has to be named before the
        /// section means anything.
        ///
        /// The condition used is that the two curvatures agree at the junction. A build and turn curve
        /// has no one curvature to share, so the junction is the only place the question can be put, and
        /// putting it there is what keeps the dogleg severity from stepping as the curve passes through.
        /// Written out it fixes the ratio of the two lengths,
        ///
        ///     L1/L2 = hypot(dTheta1, dAlpha1*sin(thetaM)) / ( ratio * hypot(dTheta2, dAlpha2*sin(thetaM)) )
        ///
        /// and with the ratio known the chord reads chord = L2*(ratio*F1 + F2). So the two splits have to
        /// bring that combination into line with the chord, which is two equations, and the length falls
        /// out by division. Two unknowns and a scale in closed form, as for the other two sections.
        ///
        /// The ratio is the curvature of the first curve at the junction over that of the second there.
        /// It defaults to one, which is the smooth join; asking for something else deliberately softens
        /// one curve against the other and costs nothing in the solving.
        ///
        /// The azimuth is carried wrapped, so the turn between the two stations is only known up to whole
        /// turns. The branch selects which one is meant and defaults to the shortest; a branch is never
        /// taken silently.
        /// </summary>
        public bool CalculateXYZ(int branch = 0, double curvatureRatio = 1.0)
        {
            if (Start == null || End == null ||
                !Numeric.IsDefined(Start.X) || !Numeric.IsDefined(Start.Y) || !Numeric.IsDefined(Start.Z) ||
                !Numeric.IsDefined(Start.Inclination) || !Numeric.IsDefined(Start.Azimuth) ||
                !Numeric.IsDefined(End.X) || !Numeric.IsDefined(End.Y) || !Numeric.IsDefined(End.Z) ||
                !Numeric.IsDefined(End.Inclination) || !Numeric.IsDefined(End.Azimuth))
            {
                return false;
            }
            if (!Numeric.IsDefined(curvatureRatio) || !Numeric.GT(curvatureRatio, 0.0))
            {
                return false;
            }

            // One curve reaching the end station with the right attitude is the answer whenever there is
            // one: the two curves are then the two halves of it and share both rates, so their curvatures
            // agree at the junction of their own accord.
            BuildAndTurnArcSection single = new BuildAndTurnArcSection
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };
            single.Start.Set(Start);
            single.End.Set(End);
            double singleMiss = double.PositiveInfinity;
            if (single.CalculateXYZ() &&
                Numeric.IsDefined(single.BuildAndTurn.BUR) && Numeric.IsDefined(single.BuildAndTurn.TR) &&
                Numeric.IsDefined(single.BuildAndTurn.Length) &&
                Numeric.IsDefined(single.End.Inclination) && Numeric.IsDefined(single.End.Azimuth))
            {
                singleMiss = AttitudeMiss(single.End, End);
            }
            if (singleMiss <= AttitudeAccuracy && Numeric.EQ(curvatureRatio, 1.0))
            {
                return ReportSingleCurve(single);
            }

            double inclinationStart = (double)Start.Inclination;
            double inclinationEnd = (double)End.Inclination;
            double turn = TrajectoryPoint3D.WrapToPi((double)End.Azimuth - (double)Start.Azimuth)
                        + 2.0 * Numeric.PI * branch;

            double chordX = (double)(End.X - Start.X);
            double chordY = (double)(End.Y - Start.Y);
            double chordZ = (double)(End.Z - Start.Z);
            double chord = System.Math.Sqrt(chordX * chordX + chordY * chordY + chordZ * chordZ);
            if (Numeric.EQ(chord, 0.0))
            {
                // The two stations coincide, so no pair of curves of finite length joins them.
                return false;
            }
            chordX /= chord;
            chordY /= chord;
            chordZ /= chord;

            // Two working sections, reused across the sweep rather than allocated inside it, and local
            // so that two sections solved at the same time cannot write over each other.
            BuildAndTurnArcSection first = new BuildAndTurnArcSection
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };
            BuildAndTurnArcSection second = new BuildAndTurnArcSection
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };

            double turnCentre = 0.5 * turn;

            bool found = false;
            double bestSeverity = double.PositiveInfinity;
            double bestJunctionInclination = 0.0;
            double bestFirstTurn = 0.0;

            double[] previous = new double[SweepTurnSteps];
            double[] current = new double[SweepTurnSteps];
            double[] next = new double[SweepTurnSteps];

            foreach (double offset in SweepOffsets)
            {
                if (found)
                {
                    break;
                }
                for (int j = 0; j < SweepTurnSteps; j++)
                {
                    previous[j] = double.NegativeInfinity;
                }
                FillSweepRow(first, second, inclinationStart, (double)Start.Azimuth, inclinationEnd, turn,
                             turnCentre, curvatureRatio, chordX, chordY, chordZ, 0, offset, current);

                for (int i = 0; i < SweepInclinationSteps; i++)
                {
                    if (i + 1 < SweepInclinationSteps)
                    {
                        FillSweepRow(first, second, inclinationStart, (double)Start.Azimuth, inclinationEnd, turn,
                                     turnCentre, curvatureRatio, chordX, chordY, chordZ, i + 1, offset, next);
                    }
                    else
                    {
                        for (int j = 0; j < SweepTurnSteps; j++)
                        {
                            next[j] = double.NegativeInfinity;
                        }
                    }

                    for (int j = 0; j < SweepTurnSteps; j++)
                    {
                        double alignment = current[j];
                        if (alignment < SweepAlignmentFloor)
                        {
                            continue;
                        }
                        // The turn does not wrap round, so the ends of a row have nothing on the outside.
                        double before = j > 0 ? current[j - 1] : double.NegativeInfinity;
                        double after = j + 1 < SweepTurnSteps ? current[j + 1] : double.NegativeInfinity;
                        if (alignment < before || alignment < after ||
                            alignment < previous[j] || alignment < next[j])
                        {
                            continue;
                        }

                        SweepPoint(turnCentre, i, j, offset,
                                   out double junctionInclination, out double firstTurn);
                        if (!Settle(first, second, inclinationStart, (double)Start.Azimuth, inclinationEnd, turn,
                                    curvatureRatio, chordX, chordY, chordZ,
                                    ref junctionInclination, ref firstTurn))
                        {
                            continue;
                        }
                        if (!Shape(first, second, inclinationStart, (double)Start.Azimuth, inclinationEnd, turn,
                                   curvatureRatio, junctionInclination, firstTurn,
                                   out double shapeX, out double shapeY, out double shapeZ,
                                   out double ratioOfLengths, out double buildFirst, out double turnFirst,
                                   out double buildSecond, out double turnSecond))
                        {
                            continue;
                        }
                        double shape = System.Math.Sqrt(shapeX * shapeX + shapeY * shapeY + shapeZ * shapeZ);
                        if (!Numeric.GT(shape, 0.0))
                        {
                            continue;
                        }
                        // The displacement has to point along the chord and not against it.
                        if ((shapeX * chordX + shapeY * chordY + shapeZ * chordZ) / shape
                            < 1.0 - SettledAlignmentTolerance)
                        {
                            continue;
                        }
                        double lengthSecond = chord / shape;
                        double lengthFirst = ratioOfLengths * lengthSecond;
                        if (!Numeric.GT(lengthFirst, 0.0) || !Numeric.GT(lengthSecond, 0.0))
                        {
                            continue;
                        }
                        // The gentlest pair is the one whose worst dogleg severity anywhere is least.
                        // For a pair sharing one curvature that is the curvature itself, so this is the
                        // same choice the other two sections make, written for a curvature that moves.
                        double severity = System.Math.Max(
                            PeakCurvature(buildFirst / lengthFirst, turnFirst / lengthFirst,
                                          inclinationStart, junctionInclination),
                            PeakCurvature(buildSecond / lengthSecond, turnSecond / lengthSecond,
                                          junctionInclination, inclinationEnd));
                        if (!(severity < bestSeverity))
                        {
                            continue;
                        }
                        found = true;
                        bestSeverity = severity;
                        bestJunctionInclination = junctionInclination;
                        bestFirstTurn = firstTurn;
                    }

                    double[] rotate = previous;
                    previous = current;
                    current = next;
                    next = rotate;
                }
            }

            if (!found)
            {
                // Nothing came out of the sweep. Where the section is so nearly a single curve that the
                // junction is not pinned down at all, every point along it serves equally well and there
                // is no isolated choice to settle on. Recovering such a curve from its two stations is
                // itself ill conditioned, which is why it did not pass the strict test above, so it is
                // taken here against a tolerance still far below any surveying accuracy. This is a last
                // resort and never displaces a genuine pair, which is looked for first.
                if (singleMiss <= AttitudeFallbackAccuracy && Numeric.EQ(curvatureRatio, 1.0))
                {
                    return ReportSingleCurve(single);
                }
                return false;
            }

            if (!Shape(first, second, inclinationStart, (double)Start.Azimuth, inclinationEnd, turn,
                       curvatureRatio, bestJunctionInclination, bestFirstTurn,
                       out double bestX, out double bestY, out double bestZ,
                       out double bestRatio, out double bestBuildFirst, out double bestTurnFirst,
                       out double bestBuildSecond, out double bestTurnSecond))
            {
                return false;
            }
            double bestShape = System.Math.Sqrt(bestX * bestX + bestY * bestY + bestZ * bestZ);
            if (!Numeric.GT(bestShape, 0.0))
            {
                return false;
            }
            double finalSecond = chord / bestShape;
            double finalFirst = bestRatio * finalSecond;

            // Build the two curves and check where they actually land. Nothing is reported until it has
            // been checked against the station it had to reach.
            first.Start.Set(Start);
            first.BuildAndTurn.BUR = bestBuildFirst / finalFirst;
            first.BuildAndTurn.TR = bestTurnFirst / finalFirst;
            first.BuildAndTurn.Length = finalFirst;
            if (!first.CalculateLBT() || !Defined(first.End))
            {
                return false;
            }
            second.Start.Set(first.End);
            second.BuildAndTurn.BUR = bestBuildSecond / finalSecond;
            second.BuildAndTurn.TR = bestTurnSecond / finalSecond;
            second.BuildAndTurn.Length = finalSecond;
            if (!second.CalculateLBT() || !Defined(second.End))
            {
                return false;
            }

            double positionMiss = System.Math.Sqrt(
                System.Math.Pow((double)second.End.X - (double)End.X, 2) +
                System.Math.Pow((double)second.End.Y - (double)End.Y, 2) +
                System.Math.Pow((double)second.End.Z - (double)End.Z, 2));
            if (positionMiss > PositionAccuracy * System.Math.Max(1.0, chord))
            {
                return false;
            }
            if (AttitudeMiss(second.End, End) > AttitudeAccuracy)
            {
                return false;
            }

            Intermediate.Set(first.End);
            End.Set(second.End);
            DoubleBuildAndTurnCurve.UpstreamBUR = first.BuildAndTurn.BUR;
            DoubleBuildAndTurnCurve.UpstreamTR = first.BuildAndTurn.TR;
            DoubleBuildAndTurnCurve.UpstreamLength = finalFirst;
            DoubleBuildAndTurnCurve.DownstreamBUR = second.BuildAndTurn.BUR;
            DoubleBuildAndTurnCurve.DownstreamTR = second.BuildAndTurn.TR;
            DoubleBuildAndTurnCurve.DownstreamLength = finalSecond;
            DoubleBuildAndTurnCurve.CurvatureRatio = curvatureRatio;
            DoubleBuildAndTurnCurve.JunctionCurvature =
                CurvatureAt((double)first.BuildAndTurn.BUR, (double)first.BuildAndTurn.TR, bestJunctionInclination);
            DoubleBuildAndTurnCurve.Length = finalFirst + finalSecond;
            return true;
        }

        /// <summary>
        /// How many samples the sweep takes over the inclination at the junction and over the share of
        /// the turn the first curve takes. A build and turn curve has its position in closed form, so a
        /// sample here is a few dozen operations rather than a pair of numerical integrations, and the
        /// sweep can afford to be as fine as the one over the sphere in the double arc section.
        /// </summary>
        private const int SweepInclinationSteps = 32;
        private const int SweepTurnSteps = 48;

        /// <summary>
        /// The sweep is run again on a grid staggered by half a cell when the first one finds nothing,
        /// which picks up the occasional solution whose basin fell between the samples.
        /// </summary>
        private static readonly double[] SweepOffsets = new double[] { 0.0, 0.5 };

        /// <summary>
        /// How far either side of half the whole turn the share taken by the first curve is looked for.
        /// </summary>
        private static readonly double SweepTurnHalfWidth = 2.0 * Numeric.PI;

        private const double SweepAlignmentFloor = 0.0;
        private const double SettledAlignmentTolerance = 1.0e-12;
        private const double AttitudeAccuracy = 1.0e-7;
        private const double PositionAccuracy = 1.0e-9;
        private const double AttitudeFallbackAccuracy = 1.0e-4;
        private const double InclinationMargin = 1.0e-9;

        private const int SettleIterations = 40;
        private const int SettleCutbacks = 30;
        private const double SettleTolerance = 1.0e-13;
        private const double SettleStep = 1.0e-7;

        /// <summary>
        /// The curvature of a build and turn curve at a given inclination.
        /// </summary>
        private static double CurvatureAt(double bur, double tur, double inclination)
        {
            double sine = System.Math.Sin(inclination);
            return System.Math.Sqrt(bur * bur + tur * tur * sine * sine);
        }

        /// <summary>
        /// The largest curvature a build and turn curve reaches between two inclinations. The inclination
        /// runs monotonically from one to the other, so the sine is largest at the horizontal when the
        /// curve passes through it and at one of the two ends otherwise.
        /// </summary>
        private static double PeakCurvature(double bur, double tur, double from, double to)
        {
            double low = System.Math.Min(from, to);
            double high = System.Math.Max(from, to);
            double sine;
            if (low <= 0.5 * Numeric.PI && high >= 0.5 * Numeric.PI)
            {
                sine = 1.0;
            }
            else
            {
                sine = System.Math.Max(System.Math.Abs(System.Math.Sin(low)),
                                       System.Math.Abs(System.Math.Sin(high)));
            }
            return System.Math.Sqrt(bur * bur + tur * tur * sine * sine);
        }

        /// <summary>
        /// Report a section that one curve already covers. The two curves are then the two halves of it,
        /// so they share both rates and the junction sits at the end.
        /// </summary>
        private bool ReportSingleCurve(BuildAndTurnArcSection single)
        {
            DoubleBuildAndTurnCurve.UpstreamBUR = single.BuildAndTurn.BUR;
            DoubleBuildAndTurnCurve.UpstreamTR = single.BuildAndTurn.TR;
            DoubleBuildAndTurnCurve.UpstreamLength = single.BuildAndTurn.Length;
            DoubleBuildAndTurnCurve.DownstreamBUR = single.BuildAndTurn.BUR;
            DoubleBuildAndTurnCurve.DownstreamTR = single.BuildAndTurn.TR;
            DoubleBuildAndTurnCurve.DownstreamLength = 0.0;
            DoubleBuildAndTurnCurve.CurvatureRatio = 1.0;
            DoubleBuildAndTurnCurve.JunctionCurvature = Numeric.IsDefined(single.End.Inclination)
                ? CurvatureAt((double)single.BuildAndTurn.BUR, (double)single.BuildAndTurn.TR,
                              (double)single.End.Inclination)
                : (double?)null;
            DoubleBuildAndTurnCurve.Length = single.BuildAndTurn.Length;
            End.Set(single.End);
            Intermediate.Set(End);
            return true;
        }

        private static bool Defined(TrajectoryPoint3D point)
        {
            return point != null &&
                   Numeric.IsDefined(point.X) && Numeric.IsDefined(point.Y) && Numeric.IsDefined(point.Z) &&
                   Numeric.IsDefined(point.Inclination) && Numeric.IsDefined(point.Azimuth);
        }

        /// <summary>
        /// The choice of the two unknowns at one sample of the sweep.
        /// </summary>
        private static void SweepPoint(double turnCentre, int inclinationIndex, int turnIndex, double offset,
                                       out double junctionInclination, out double firstTurn)
        {
            junctionInclination = Numeric.PI * (inclinationIndex + 0.5 + offset) / SweepInclinationSteps;
            firstTurn = turnCentre + SweepTurnHalfWidth *
                        (2.0 * (turnIndex + 0.5 + offset) / SweepTurnSteps - 1.0);
        }

        /// <summary>
        /// The combination ratio*F1 + F2 for a given inclination at the junction and a given share of the
        /// turn taken by the first curve, along with the ratio of the two lengths that holding the
        /// curvatures equal at the junction calls for.
        ///
        /// The changes of inclination and of azimuth are what settle the shape of each curve, and they
        /// have to add up to the change between the two stations. So these two choices fix everything
        /// except the overall size, and that is what the caller reads off by comparing the combination
        /// against the chord.
        /// </summary>
        private static bool Shape(BuildAndTurnArcSection first, BuildAndTurnArcSection second,
                                  double inclinationStart, double azimuthStart, double inclinationEnd,
                                  double turn, double curvatureRatio,
                                  double junctionInclination, double firstTurn,
                                  out double x, out double y, out double z,
                                  out double ratioOfLengths,
                                  out double buildFirst, out double turnFirst,
                                  out double buildSecond, out double turnSecond)
        {
            x = 0.0;
            y = 0.0;
            z = 0.0;
            ratioOfLengths = 0.0;
            buildFirst = junctionInclination - inclinationStart;
            turnFirst = firstTurn;
            buildSecond = inclinationEnd - junctionInclination;
            turnSecond = turn - firstTurn;

            if (junctionInclination <= InclinationMargin ||
                junctionInclination >= Numeric.PI - InclinationMargin)
            {
                // The azimuth of a station at the vertical carries no meaning, so the junction is kept
                // clear of it.
                return false;
            }

            // Holding the curvatures equal at the junction fixes the ratio of the two lengths. Both
            // curvatures are read at the junction inclination, which is what makes the sine common to
            // the two and leaves a ratio that does not depend on the sizes.
            double sine = System.Math.Sin(junctionInclination);
            double weightFirst = System.Math.Sqrt(buildFirst * buildFirst + turnFirst * turnFirst * sine * sine);
            double weightSecond = System.Math.Sqrt(buildSecond * buildSecond + turnSecond * turnSecond * sine * sine);
            if (!Numeric.GT(weightSecond, 0.0) || !Numeric.GT(weightFirst, 0.0))
            {
                // One of the two curves would neither build nor turn, so it is a tangent section and
                // carries no curvature to match against the other.
                return false;
            }
            ratioOfLengths = weightFirst / (curvatureRatio * weightSecond);

            // The shape of each curve at unit length: with a length of one the rates are the changes.
            first.Start.Set(0.0, 0.0, 0.0);
            first.Start.Inclination = inclinationStart;
            first.Start.Azimuth = azimuthStart;
            first.Start.Abscissa = 0.0;
            first.BuildAndTurn.BUR = buildFirst;
            first.BuildAndTurn.TR = turnFirst;
            first.BuildAndTurn.Length = 1.0;
            if (!first.CalculateLBT() || !Defined(first.End))
            {
                return false;
            }

            second.Start.Set(0.0, 0.0, 0.0);
            second.Start.Inclination = junctionInclination;
            second.Start.Azimuth = azimuthStart + firstTurn;
            second.Start.Abscissa = 0.0;
            second.BuildAndTurn.BUR = buildSecond;
            second.BuildAndTurn.TR = turnSecond;
            second.BuildAndTurn.Length = 1.0;
            if (!second.CalculateLBT() || !Defined(second.End))
            {
                return false;
            }

            x = ratioOfLengths * (double)first.End.X + (double)second.End.X;
            y = ratioOfLengths * (double)first.End.Y + (double)second.End.Y;
            z = ratioOfLengths * (double)first.End.Z + (double)second.End.Z;
            return true;
        }

        /// <summary>
        /// The cosine of the angle between the combination and the chord, which is one at a solution.
        /// </summary>
        private static bool Alignment(BuildAndTurnArcSection first, BuildAndTurnArcSection second,
                                      double inclinationStart, double azimuthStart, double inclinationEnd,
                                      double turn, double curvatureRatio,
                                      double junctionInclination, double firstTurn,
                                      double chordX, double chordY, double chordZ,
                                      out double alignment)
        {
            alignment = double.NegativeInfinity;
            if (!Shape(first, second, inclinationStart, azimuthStart, inclinationEnd, turn, curvatureRatio,
                       junctionInclination, firstTurn,
                       out double x, out double y, out double z,
                       out _, out _, out _, out _, out _))
            {
                return false;
            }
            double length = System.Math.Sqrt(x * x + y * y + z * z);
            if (!Numeric.GT(length, 0.0))
            {
                return false;
            }
            alignment = (x * chordX + y * chordY + z * chordZ) / length;
            return true;
        }

        /// <summary>
        /// One row of the sweep.
        /// </summary>
        private static void FillSweepRow(BuildAndTurnArcSection first, BuildAndTurnArcSection second,
                                         double inclinationStart, double azimuthStart, double inclinationEnd,
                                         double turn, double turnCentre, double curvatureRatio,
                                         double chordX, double chordY, double chordZ,
                                         int inclinationIndex, double offset, double[] row)
        {
            for (int j = 0; j < row.Length; j++)
            {
                SweepPoint(turnCentre, inclinationIndex, j, offset,
                           out double junctionInclination, out double firstTurn);
                if (!Alignment(first, second, inclinationStart, azimuthStart, inclinationEnd, turn,
                               curvatureRatio, junctionInclination, firstTurn,
                               chordX, chordY, chordZ, out double alignment))
                {
                    alignment = double.NegativeInfinity;
                }
                row[j] = alignment;
            }
        }

        /// <summary>
        /// Settle the two unknowns onto a choice that brings the combination into line with the chord
        /// exactly. The two residuals are its components across the chord, and Newton on that pair
        /// converges in a few steps from anywhere in a basin; the step is cut back when it fails to
        /// improve, so overshooting does not throw the settling away.
        /// </summary>
        private static bool Settle(BuildAndTurnArcSection first, BuildAndTurnArcSection second,
                                   double inclinationStart, double azimuthStart, double inclinationEnd,
                                   double turn, double curvatureRatio,
                                   double chordX, double chordY, double chordZ,
                                   ref double junctionInclination, ref double firstTurn)
        {
            Orthogonal(chordX, chordY, chordZ, out double e1x, out double e1y, out double e1z,
                                               out double e2x, out double e2y, out double e2z);

            bool Residual(double inclination, double share, out double r1, out double r2)
            {
                r1 = 0.0;
                r2 = 0.0;
                if (!Shape(first, second, inclinationStart, azimuthStart, inclinationEnd, turn, curvatureRatio,
                           inclination, share, out double x, out double y, out double z,
                           out _, out _, out _, out _, out _))
                {
                    return false;
                }
                double length = System.Math.Sqrt(x * x + y * y + z * z);
                if (!Numeric.GT(length, 0.0))
                {
                    return false;
                }
                r1 = (x * e1x + y * e1y + z * e1z) / length;
                r2 = (x * e2x + y * e2y + z * e2z) / length;
                return true;
            }

            if (!Residual(junctionInclination, firstTurn, out double f1, out double f2))
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
                if (!Residual(junctionInclination + SettleStep, firstTurn, out double ap1, out double ap2) ||
                    !Residual(junctionInclination - SettleStep, firstTurn, out double am1, out double am2) ||
                    !Residual(junctionInclination, firstTurn + SettleStep, out double bp1, out double bp2) ||
                    !Residual(junctionInclination, firstTurn - SettleStep, out double bm1, out double bm2))
                {
                    return false;
                }
                double j11 = (ap1 - am1) / (2.0 * SettleStep);
                double j21 = (ap2 - am2) / (2.0 * SettleStep);
                double j12 = (bp1 - bm1) / (2.0 * SettleStep);
                double j22 = (bp2 - bm2) / (2.0 * SettleStep);
                double determinant = j11 * j22 - j12 * j21;
                if (Numeric.EQ(determinant, 0.0))
                {
                    return false;
                }
                double stepInclination = -(j22 * f1 - j12 * f2) / determinant;
                double stepTurn = -(-j21 * f1 + j11 * f2) / determinant;

                bool improved = false;
                double damping = 1.0;
                for (int cut = 0; cut < SettleCutbacks; cut++)
                {
                    double trialInclination = junctionInclination + damping * stepInclination;
                    double trialTurn = firstTurn + damping * stepTurn;
                    if (trialInclination > InclinationMargin && trialInclination < Numeric.PI - InclinationMargin &&
                        Residual(trialInclination, trialTurn, out double g1, out double g2))
                    {
                        double trial = g1 * g1 + g2 * g2;
                        if (trial < norm)
                        {
                            junctionInclination = trialInclination;
                            firstTurn = trialTurn;
                            f1 = g1;
                            f2 = g2;
                            norm = trial;
                            improved = true;
                            break;
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

        /// <summary>
        /// Two unit vectors completing the given one into a right handed frame.
        /// </summary>
        private static void Orthogonal(double x, double y, double z,
                                       out double e1x, out double e1y, out double e1z,
                                       out double e2x, out double e2y, out double e2z)
        {
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
            return System.Math.Atan2(System.Math.Sqrt(crossX * crossX + crossY * crossY + crossZ * crossZ),
                                     ax * bx + ay * by + az * bz);
        }

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
        /// Interpolate at a given measured depth and return the interpolation.
        /// </summary>
        public override CurvilinearPoint3D InterpolateAtMD(double md)
        {
            CurvilinearPoint3D result = new TrajectoryPoint3D();
            result.SetUndefined();
            InterpolateAtMD(md, result);
            return result;
        }

        /// <summary>
        /// Set the result of the interpolation at a given measured depth in the point that is passed.
        /// </summary>
        public void InterpolateAtMD(double md, CurvilinearPoint3D point)
        {
            if (point == null)
            {
                return;
            }
            if (Start == null || Start.IsUndefined() || End == null || End.IsUndefined() ||
                DoubleBuildAndTurnCurve == null ||
                !Numeric.IsDefined(DoubleBuildAndTurnCurve.UpstreamBUR) ||
                !Numeric.IsDefined(DoubleBuildAndTurnCurve.UpstreamTR) ||
                !Numeric.IsDefined(DoubleBuildAndTurnCurve.DownstreamBUR) ||
                !Numeric.IsDefined(DoubleBuildAndTurnCurve.DownstreamTR) ||
                Intermediate == null || !Numeric.IsDefined(Intermediate.Abscissa))
            {
                point.SetUndefined();
                return;
            }
            if (Numeric.EQ(Start.Abscissa, End.Abscissa, 0.001))
            {
                point.Set(Start);
                return;
            }

            BuildAndTurnArcSection piece = new BuildAndTurnArcSection
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };
            if (Numeric.LE(md, Intermediate.Abscissa))
            {
                piece.Start.Set(Start);
                piece.BuildAndTurn.BUR = DoubleBuildAndTurnCurve.UpstreamBUR;
                piece.BuildAndTurn.TR = DoubleBuildAndTurnCurve.UpstreamTR;
            }
            else
            {
                piece.Start.Set(Intermediate);
                piece.BuildAndTurn.BUR = DoubleBuildAndTurnCurve.DownstreamBUR;
                piece.BuildAndTurn.TR = DoubleBuildAndTurnCurve.DownstreamTR;
            }
            piece.BuildAndTurn.Length = md - (double)piece.Start.Abscissa;
            if (Numeric.LT(piece.BuildAndTurn.Length, 0.0))
            {
                point.SetUndefined();
                return;
            }
            if (piece.CalculateLBT() && Defined(piece.End))
            {
                point.Set(piece.End);
            }
            else
            {
                point.SetUndefined();
            }
        }

        /// <summary>
        /// predicate to test if either the first or the second curve has zero length
        /// </summary>
        public bool HasZeroLengthArc()
        {
            return (Start != null && Start.EQ(Intermediate)) || (End != null && End.EQ(Intermediate));
        }

        /// <summary>
        /// predicate to test if either the first or the second curve has zero length at the given accuracy
        /// </summary>
        public bool HasZeroLengthArc(double acc)
        {
            return (Start != null && Start.EQ(Intermediate, acc)) || (End != null && End.EQ(Intermediate, acc));
        }
    }
}
