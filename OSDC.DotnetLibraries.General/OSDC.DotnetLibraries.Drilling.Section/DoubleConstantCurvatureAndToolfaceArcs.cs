using System;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// A section made of two constant curvature and constant toolface curves sharing one curvature,
    /// joining two stations that are both fully defined in position and in attitude. It is the same
    /// problem the double arc section solves, with the circular arc replaced by the curve a bent
    /// housing actually drills when the toolface is held.
    /// </summary>
    [Serializable]
    public class DoubleConstantCurvatureAndToolfaceArcs : ArcSection
    {
        /// <summary>
        /// The station where the two curves meet.
        /// </summary>
        public TrajectoryPoint3D Intermediate { get; set; } = new TrajectoryPoint3D();

        /// <summary>
        /// The pair of curves.
        /// </summary>
        public NonLocalizedDoubleConstantCurvatureAndToolfaceCurve DoubleCTCCurve { get; set; }
            = new NonLocalizedDoubleConstantCurvatureAndToolfaceCurve();

        /// <summary>
        /// the generic accessor to the curve
        /// </summary>
        public override NonLocalizedCurve Curve
        {
            get => DoubleCTCCurve;
            set
            {
                if (value is NonLocalizedDoubleConstantCurvatureAndToolfaceCurve)
                {
                    DoubleCTCCurve = (NonLocalizedDoubleConstantCurvatureAndToolfaceCurve)value;
                }
            }
        }

        public DoubleConstantCurvatureAndToolfaceArcs()
        {
        }

        public DoubleConstantCurvatureAndToolfaceArcs(TrajectoryPoint3D start, TrajectoryPoint3D end)
        {
            Start = start;
            End = end;
        }

        public override bool Calculate()
        {
            return CalculateXYZ();
        }

        /// <summary>
        /// Work out the two curves of equal curvature that lead from the start station to the end
        /// station, both of them fully defined in position and attitude.
        ///
        /// Three properties of a constant curvature and toolface curve carry the whole construction.
        /// Writing w = curvature*length for the dimensionless length and f for the toolface angle,
        ///
        ///     the inclination is linear:       dTheta = w*cos(f)
        ///     the azimuth follows through G:   dAlpha = w*sin(f) / G
        ///     the displacement is a pure scale: d = F(f, w) / curvature
        ///
        /// where G is the effective sine of the segment, which depends on the two inclinations alone.
        /// Read backwards, the first two say that choosing the inclination at the junction and how much
        /// of the total turn the first curve takes fixes both toolface angles and both dimensionless
        /// lengths outright: the changes of inclination have to add up to the change between the two
        /// stations, and so do the changes of azimuth. The end attitude is then reached whatever those
        /// two choices are, which leaves only the displacement to see to.
        ///
        /// The third property says the curvature is no more than a scale on that displacement. So the
        /// two choices have to bring the displacement into line with the chord joining the two stations,
        /// which is two equations, and the curvature is then the ratio of the two lengths. Two unknowns
        /// and a curvature in closed form, for a problem that has five parameters in it.
        ///
        /// There may be no solution and there may be several. The unknowns are swept before settling, so
        /// that a solution is found wherever it lies rather than only near a guess, and the gentlest of
        /// whatever is found is kept.
        ///
        /// The azimuth is carried wrapped, so the turn between the two stations is only known up to whole
        /// turns. The branch selects which one is meant and defaults to the shortest; a branch is never
        /// taken silently.
        /// </summary>
        public bool CalculateXYZ(int branch = 0)
        {
            if (Start == null || End == null ||
                !Numeric.IsDefined(Start.X) || !Numeric.IsDefined(Start.Y) || !Numeric.IsDefined(Start.Z) ||
                !Numeric.IsDefined(Start.Inclination) || !Numeric.IsDefined(Start.Azimuth) ||
                !Numeric.IsDefined(End.X) || !Numeric.IsDefined(End.Y) || !Numeric.IsDefined(End.Z) ||
                !Numeric.IsDefined(End.Inclination) || !Numeric.IsDefined(End.Azimuth))
            {
                return false;
            }

            // One curve reaching the end station with the right attitude is the answer whenever there is
            // one: the two curves are then the two halves of it and hold the same toolface.
            ConstantCurvatureAndToolfaceArcSection single = new ConstantCurvatureAndToolfaceArcSection
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };
            single.Start.Set(Start);
            single.End.Set(End);
            double singleMiss = double.PositiveInfinity;
            if (single.CalculateXYZ() &&
                Numeric.IsDefined(single.CTCCurve.Curvature) && Numeric.IsDefined(single.CTCCurve.Toolface) &&
                Numeric.IsDefined(single.End.Inclination) && Numeric.IsDefined(single.End.Azimuth))
            {
                singleMiss = AttitudeMiss(single.End, End);
            }
            if (singleMiss <= AttitudeAccuracy)
            {
                return ReportSingleCurve(single);
            }

            double inclinationStart = (double)Start.Inclination;
            double inclinationEnd = (double)End.Inclination;
            if (Numeric.EQ(System.Math.Sin(inclinationStart), 0.0) ||
                Numeric.EQ(System.Math.Sin(inclinationEnd), 0.0))
            {
                // At the vertical the azimuth of a station carries no meaning, so there is nothing for
                // the turn of the curves to be measured against.
                return false;
            }
            double turn = TrajectoryPoint3D.WrapToPi((double)End.Azimuth - (double)Start.Azimuth)
                        + 2.0 * Numeric.PI * branch;

            double chordX = (double)(End.X - Start.X);
            double chordY = (double)(End.Y - Start.Y);
            double chordZ = (double)(End.Z - Start.Z);
            double chord = System.Math.Sqrt(chordX * chordX + chordY * chordY + chordZ * chordZ);
            if (Numeric.EQ(chord, 0.0))
            {
                // The two stations coincide, so no pair of curves of finite curvature joins them.
                return false;
            }
            chordX /= chord;
            chordY /= chord;
            chordZ /= chord;

            // Two working sections, reused across the sweep rather than allocated inside it. They are
            // local on purpose: holding them in static fields, as the double arc section once did, makes
            // two sections solved at the same time write over each other.
            ConstantCurvatureAndToolfaceArcSection first = new ConstantCurvatureAndToolfaceArcSection
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };
            ConstantCurvatureAndToolfaceArcSection second = new ConstantCurvatureAndToolfaceArcSection
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };

            // The turn of the first curve is looked for around half of the whole turn. A window of a
            // whole turn either side of that covers the splits that arise in practice by a wide margin.
            double turnCentre = 0.5 * turn;

            bool found = false;
            double bestCurvature = double.PositiveInfinity;
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
                             turnCentre, chordX, chordY, chordZ, 0, offset, current);

                for (int i = 0; i < SweepInclinationSteps; i++)
                {
                    if (i + 1 < SweepInclinationSteps)
                    {
                        FillSweepRow(first, second, inclinationStart, (double)Start.Azimuth, inclinationEnd, turn,
                                     turnCentre, chordX, chordY, chordZ, i + 1, offset, next);
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
                        // Unlike the sweep of a sphere, the turn does not wrap round, so the ends of a
                        // row compare against nothing on the outside.
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
                                    chordX, chordY, chordZ, ref junctionInclination, ref firstTurn))
                        {
                            continue;
                        }
                        if (!Shape(first, second, inclinationStart, (double)Start.Azimuth, inclinationEnd, turn,
                                   junctionInclination, firstTurn,
                                   out double shapeX, out double shapeY, out double shapeZ,
                                   out double toolfaceFirst, out double lengthFirst,
                                   out double toolfaceSecond, out double lengthSecond))
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
                        double curvature = shape / chord;
                        if (!Numeric.GT(curvature, 0.0) || curvature >= bestCurvature)
                        {
                            continue;
                        }
                        found = true;
                        bestCurvature = curvature;
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
                if (singleMiss <= AttitudeFallbackAccuracy)
                {
                    return ReportSingleCurve(single);
                }
                return false;
            }

            if (!Shape(first, second, inclinationStart, (double)Start.Azimuth, inclinationEnd, turn,
                       bestJunctionInclination, bestFirstTurn,
                       out _, out _, out _,
                       out double bestToolfaceFirst, out double bestLengthFirst,
                       out double bestToolfaceSecond, out double bestLengthSecond))
            {
                return false;
            }

            // Build the two curves at the curvature that was found and check where they actually land.
            // Nothing is reported until it has been checked against the station it had to reach.
            double radius = 1.0 / bestCurvature;
            first.Start.Set(Start);
            first.CTCCurve.Curvature = bestCurvature;
            first.CTCCurve.Toolface = bestToolfaceFirst;
            first.CTCCurve.Length = bestLengthFirst * radius;
            if (!first.CalculateLDT() || !Defined(first.End))
            {
                return false;
            }
            second.Start.Set(first.End);
            second.CTCCurve.Curvature = bestCurvature;
            second.CTCCurve.Toolface = bestToolfaceSecond;
            second.CTCCurve.Length = bestLengthSecond * radius;
            if (!second.CalculateLDT() || !Defined(second.End))
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
            DoubleCTCCurve.Curvature = bestCurvature;
            DoubleCTCCurve.UpstreamToolface = bestToolfaceFirst;
            DoubleCTCCurve.DownstreamToolface = bestToolfaceSecond;
            DoubleCTCCurve.UpstreamLength = first.CTCCurve.Length;
            DoubleCTCCurve.DownstreamLength = second.CTCCurve.Length;
            DoubleCTCCurve.Length = (double)first.CTCCurve.Length + (double)second.CTCCurve.Length;
            return true;
        }

        /// <summary>
        /// How many samples the sweep takes over the inclination at the junction and over the share of
        /// the turn the first curve takes.
        ///
        /// Each sample costs two curves to be integrated, and a curve of constant curvature and toolface
        /// has no closed form for its position, so the sweep is what this construction spends its time
        /// on: about seven milliseconds a section, against a twentieth of that for the double arc, whose
        /// samples are a few dozen operations each. Coarsening it is tempting and wrong. Measured over
        /// the same cases, a sweep a third this size ran six times faster but came back with a different
        /// pair in about three in a hundred, some of them several times stiffer, because the basin
        /// holding the gentlest pair had fallen between its samples. Which pair is reported would then
        /// depend on the size of the grid, which is no basis for a construction. Trimming the settling
        /// instead saves nothing: the sweep, not the settling, is where the time goes.
        /// </summary>
        private const int SweepInclinationSteps = 20;
        private const int SweepTurnSteps = 28;

        /// <summary>
        /// The sweep is run again on a grid staggered by half a cell when the first one finds nothing,
        /// which picks up the occasional solution whose basin fell between the samples.
        /// </summary>
        private static readonly double[] SweepOffsets = new double[] { 0.0, 0.5 };

        /// <summary>
        /// How far either side of half the whole turn the share taken by the first curve is looked for.
        /// Measured over pairs built forwards, the share sits within about half a turn of the middle, so
        /// a whole turn either side is a wide margin.
        /// </summary>
        private static readonly double SweepTurnHalfWidth = 2.0 * Numeric.PI;

        /// <summary>
        /// How well a sample has to line up with the chord before it is worth settling from. A solution
        /// lines up exactly, so a sample pointing the other way cannot be in its basin.
        /// </summary>
        private const double SweepAlignmentFloor = 0.0;

        /// <summary>
        /// How well a settled choice has to line up before it counts as a solution.
        /// </summary>
        private const double SettledAlignmentTolerance = 1.0e-12;

        /// <summary>
        /// How closely the end attitude has to be reached, in radians.
        /// </summary>
        private const double AttitudeAccuracy = 1.0e-7;

        /// <summary>
        /// How closely the end position has to be reached, relative to the length of the chord.
        /// </summary>
        private const double PositionAccuracy = 1.0e-9;

        /// <summary>
        /// How closely the end attitude has to be reached when a single curve is all that is left to
        /// fall back on, in radians. About two ten thousandths of a degree, orders below what a survey
        /// resolves, and only ever used where the sweep found no pair at all.
        /// </summary>
        private const double AttitudeFallbackAccuracy = 1.0e-4;

        private const int SettleIterations = 40;
        private const int SettleCutbacks = 30;
        private const double SettleTolerance = 1.0e-13;
        private const double SettleStep = 1.0e-7;

        /// <summary>
        /// Report a section that one curve already covers. The two curves are then the two halves of it,
        /// so they hold the same toolface and the junction sits at the end.
        /// </summary>
        private bool ReportSingleCurve(ConstantCurvatureAndToolfaceArcSection single)
        {
            DoubleCTCCurve.Curvature = single.CTCCurve.Curvature;
            DoubleCTCCurve.UpstreamToolface = single.CTCCurve.Toolface;
            DoubleCTCCurve.DownstreamToolface = single.CTCCurve.Toolface;
            DoubleCTCCurve.UpstreamLength = single.CTCCurve.Length;
            DoubleCTCCurve.DownstreamLength = 0.0;
            DoubleCTCCurve.Length = single.CTCCurve.Length;
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
        /// The shape of the pair at unit curvature, for a given inclination at the junction and a given
        /// share of the turn taken by the first curve.
        ///
        /// The inclinations have to add up to the change between the two stations and so do the turns,
        /// which is what fixes the two toolface angles and the two dimensionless lengths. The end
        /// attitude comes out right by construction, whatever the two choices are; what comes back here
        /// is where the pair ends up, which is what the choices still have to see to.
        /// </summary>
        private static bool Shape(ConstantCurvatureAndToolfaceArcSection first,
                                  ConstantCurvatureAndToolfaceArcSection second,
                                  double inclinationStart, double azimuthStart, double inclinationEnd,
                                  double turn, double junctionInclination, double firstTurn,
                                  out double x, out double y, out double z,
                                  out double toolfaceFirst, out double lengthFirst,
                                  out double toolfaceSecond, out double lengthSecond)
        {
            x = 0.0;
            y = 0.0;
            z = 0.0;
            toolfaceFirst = 0.0;
            lengthFirst = 0.0;
            toolfaceSecond = 0.0;
            lengthSecond = 0.0;

            if (junctionInclination <= InclinationMargin ||
                junctionInclination >= Numeric.PI - InclinationMargin)
            {
                // A pair passing through the vertical loses its azimuth there and stops turning, so the
                // junction is kept clear of it.
                return false;
            }

            double buildFirst = junctionInclination - inclinationStart;
            double buildSecond = inclinationEnd - junctionInclination;
            double effectiveSineFirst = TrajectoryPoint3D.EffectiveSineCDT(inclinationStart, junctionInclination);
            double effectiveSineSecond = TrajectoryPoint3D.EffectiveSineCDT(junctionInclination, inclinationEnd);
            if (!Numeric.IsDefined(effectiveSineFirst) || !Numeric.IsDefined(effectiveSineSecond))
            {
                return false;
            }
            double turnFirst = effectiveSineFirst * firstTurn;
            double turnSecond = effectiveSineSecond * (turn - firstTurn);

            lengthFirst = System.Math.Sqrt(buildFirst * buildFirst + turnFirst * turnFirst);
            lengthSecond = System.Math.Sqrt(buildSecond * buildSecond + turnSecond * turnSecond);
            if (!Numeric.GT(lengthFirst + lengthSecond, 0.0))
            {
                return false;
            }
            toolfaceFirst = System.Math.Atan2(turnFirst, buildFirst);
            toolfaceSecond = System.Math.Atan2(turnSecond, buildSecond);

            first.Start.Set(0.0, 0.0, 0.0);
            first.Start.Inclination = inclinationStart;
            first.Start.Azimuth = azimuthStart;
            first.Start.Abscissa = 0.0;
            first.CTCCurve.Curvature = 1.0;
            first.CTCCurve.Toolface = toolfaceFirst;
            first.CTCCurve.Length = lengthFirst;
            if (!first.CalculateLDT() || !Defined(first.End))
            {
                return false;
            }

            second.Start.Set(first.End);
            second.Start.Abscissa = 0.0;
            second.CTCCurve.Curvature = 1.0;
            second.CTCCurve.Toolface = toolfaceSecond;
            second.CTCCurve.Length = lengthSecond;
            if (!second.CalculateLDT() || !Defined(second.End))
            {
                return false;
            }

            x = (double)second.End.X;
            y = (double)second.End.Y;
            z = (double)second.End.Z;
            return true;
        }

        /// <summary>
        /// How close to the vertical the junction is allowed to come.
        /// </summary>
        private const double InclinationMargin = 1.0e-6;

        /// <summary>
        /// The cosine of the angle between the displacement of the pair and the chord, which is one
        /// exactly at a solution.
        /// </summary>
        private static bool Alignment(ConstantCurvatureAndToolfaceArcSection first,
                                      ConstantCurvatureAndToolfaceArcSection second,
                                      double inclinationStart, double azimuthStart, double inclinationEnd,
                                      double turn, double junctionInclination, double firstTurn,
                                      double chordX, double chordY, double chordZ,
                                      out double alignment)
        {
            alignment = double.NegativeInfinity;
            if (!Shape(first, second, inclinationStart, azimuthStart, inclinationEnd, turn,
                       junctionInclination, firstTurn,
                       out double x, out double y, out double z, out _, out _, out _, out _))
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
        /// One row of the sweep, holding how well the displacement lines up with the chord at each share
        /// of the turn, for one inclination at the junction.
        /// </summary>
        private static void FillSweepRow(ConstantCurvatureAndToolfaceArcSection first,
                                         ConstantCurvatureAndToolfaceArcSection second,
                                         double inclinationStart, double azimuthStart, double inclinationEnd,
                                         double turn, double turnCentre,
                                         double chordX, double chordY, double chordZ,
                                         int inclinationIndex, double offset, double[] row)
        {
            for (int j = 0; j < row.Length; j++)
            {
                SweepPoint(turnCentre, inclinationIndex, j, offset,
                           out double junctionInclination, out double firstTurn);
                if (!Alignment(first, second, inclinationStart, azimuthStart, inclinationEnd, turn,
                               junctionInclination, firstTurn, chordX, chordY, chordZ, out double alignment))
                {
                    alignment = double.NegativeInfinity;
                }
                row[j] = alignment;
            }
        }

        /// <summary>
        /// Settle the two unknowns onto a choice that brings the displacement into line with the chord
        /// exactly. The two residuals are the components of the displacement across the chord, and
        /// Newton on that pair converges in a few steps from anywhere in a basin. The step is cut back
        /// when it fails to improve, so a step that overshoots does not throw the settling away.
        /// </summary>
        private static bool Settle(ConstantCurvatureAndToolfaceArcSection first,
                                   ConstantCurvatureAndToolfaceArcSection second,
                                   double inclinationStart, double azimuthStart, double inclinationEnd,
                                   double turn, double chordX, double chordY, double chordZ,
                                   ref double junctionInclination, ref double firstTurn)
        {
            Orthogonal(chordX, chordY, chordZ, out double e1x, out double e1y, out double e1z,
                                               out double e2x, out double e2y, out double e2z);

            bool Residual(double inclination, double share, out double r1, out double r2)
            {
                r1 = 0.0;
                r2 = 0.0;
                if (!Shape(first, second, inclinationStart, azimuthStart, inclinationEnd, turn,
                           inclination, share, out double x, out double y, out double z,
                           out _, out _, out _, out _))
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
                // The two by two Jacobian, by a central difference. The residuals are smooth in both
                // unknowns, and a difference costs rate of convergence rather than final accuracy.
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
                DoubleCTCCurve == null || !Numeric.IsDefined(DoubleCTCCurve.Curvature) ||
                !Numeric.IsDefined(DoubleCTCCurve.UpstreamToolface) ||
                !Numeric.IsDefined(DoubleCTCCurve.DownstreamToolface) ||
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

            ConstantCurvatureAndToolfaceArcSection piece = new ConstantCurvatureAndToolfaceArcSection
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };
            if (Numeric.LE(md, Intermediate.Abscissa))
            {
                piece.Start.Set(Start);
                piece.CTCCurve.Toolface = (double)DoubleCTCCurve.UpstreamToolface;
            }
            else
            {
                piece.Start.Set(Intermediate);
                piece.CTCCurve.Toolface = (double)DoubleCTCCurve.DownstreamToolface;
            }
            piece.CTCCurve.Curvature = (double)DoubleCTCCurve.Curvature;
            piece.CTCCurve.Length = md - (double)piece.Start.Abscissa;
            if (Numeric.LT(piece.CTCCurve.Length, 0.0))
            {
                point.SetUndefined();
                return;
            }
            if (piece.CalculateLDT() && Defined(piece.End))
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
