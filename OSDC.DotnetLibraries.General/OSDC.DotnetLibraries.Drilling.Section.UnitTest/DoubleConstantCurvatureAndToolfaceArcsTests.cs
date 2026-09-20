using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using static OSDC.DotnetLibraries.Drilling.Section.UnitTest.SectionTestHelper;

namespace OSDC.DotnetLibraries.Drilling.Section.UnitTest
{
    /// <summary>
    /// The same problem the double arc section solves, with the circular arc replaced by the curve of
    /// constant curvature and constant toolface. The cases below are built by running two such curves
    /// forward, so a solution is known to exist, and what comes back is then rebuilt from the reported
    /// parameters and checked against the station it had to reach.
    ///
    /// These sweeps are smaller than the ones for the circular arcs. A curve of constant curvature and
    /// toolface has no closed form for its position, so every trial costs two numerical integrations and
    /// this construction is some hundred times dearer than the double arc.
    /// </summary>
    public class DoubleConstantCurvatureAndToolfaceArcsTests
    {
        private const int Cases = 400;

        private static void Tangent(TrajectoryPoint3D point, out double x, out double y, out double z)
        {
            double sine = System.Math.Sin((double)point.Inclination);
            x = sine * System.Math.Cos((double)point.Azimuth);
            y = sine * System.Math.Sin((double)point.Azimuth);
            z = System.Math.Cos((double)point.Inclination);
        }

        private static double AttitudeMiss(TrajectoryPoint3D got, TrajectoryPoint3D wanted)
        {
            Tangent(got, out double ax, out double ay, out double az);
            Tangent(wanted, out double bx, out double by, out double bz);
            double cx = ay * bz - az * by;
            double cy = az * bx - ax * bz;
            double cz = ax * by - ay * bx;
            return System.Math.Atan2(System.Math.Sqrt(cx * cx + cy * cy + cz * cz), ax * bx + ay * by + az * bz);
        }

        private static double Distance(TrajectoryPoint3D a, TrajectoryPoint3D b)
        {
            return System.Math.Sqrt(
                System.Math.Pow((double)a.X - (double)b.X, 2) +
                System.Math.Pow((double)a.Y - (double)b.Y, 2) +
                System.Math.Pow((double)a.Z - (double)b.Z, 2));
        }

        /// <summary>
        /// One constant curvature and toolface curve run forward from a station.
        /// </summary>
        private static ConstantCurvatureAndToolfaceArcSection Forward(TrajectoryPoint3D from,
                                                                      double curvature, double toolface, double length)
        {
            ConstantCurvatureAndToolfaceArcSection curve = new ConstantCurvatureAndToolfaceArcSection
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };
            curve.Start.Set(from);
            curve.CTCCurve.Curvature = curvature;
            curve.CTCCurve.Toolface = toolface;
            curve.CTCCurve.Length = length;
            if (!curve.CalculateLDT() || !Produced(curve.End) ||
                curve.End.Inclination == null || curve.End.Azimuth == null)
            {
                return null;
            }
            return curve;
        }

        /// <summary>
        /// A pair of curves of one curvature, giving two stations that such a pair certainly joins. The
        /// junction and the end are kept clear of the vertical, where a curve of this kind stops turning
        /// and its station loses its azimuth.
        /// </summary>
        private static (TrajectoryPoint3D start, TrajectoryPoint3D end, double curvature)? Reference(
            Random random, bool sameToolface)
        {
            double inclination = 0.2 + (System.Math.PI - 0.4) * random.NextDouble();
            double azimuth = 2.0 * System.Math.PI * random.NextDouble();
            double curvature = 3.0e-4 + 4.0e-3 * random.NextDouble();
            double toolfaceFirst = -System.Math.PI + 2.0 * System.Math.PI * random.NextDouble();
            double toolfaceSecond = sameToolface
                ? toolfaceFirst
                : -System.Math.PI + 2.0 * System.Math.PI * random.NextDouble();

            TrajectoryPoint3D start = StartPoint(inclination, azimuth);

            ConstantCurvatureAndToolfaceArcSection first =
                Forward(start, curvature, toolfaceFirst, 30.0 + 300.0 * random.NextDouble());
            if (first == null)
            {
                return null;
            }
            double junction = (double)first.End.Inclination;
            if (junction <= 0.05 || junction >= System.Math.PI - 0.05)
            {
                return null;
            }
            ConstantCurvatureAndToolfaceArcSection second =
                Forward(first.End, curvature, toolfaceSecond, 30.0 + 300.0 * random.NextDouble());
            if (second == null)
            {
                return null;
            }
            double finish = (double)second.End.Inclination;
            if (finish <= 0.05 || finish >= System.Math.PI - 0.05)
            {
                return null;
            }

            TrajectoryPoint3D end = new TrajectoryPoint3D();
            end.Set(second.End);
            return (start, end, curvature);
        }

        private static DoubleConstantCurvatureAndToolfaceArcs Solver(TrajectoryPoint3D start, TrajectoryPoint3D end)
        {
            DoubleConstantCurvatureAndToolfaceArcs section = new DoubleConstantCurvatureAndToolfaceArcs
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };
            section.Start.Set(start);
            section.End.Set(end);
            return section;
        }

        /// <summary>
        /// Rebuild the pair from the parameters that were reported and hand back where it really ends.
        /// </summary>
        private static TrajectoryPoint3D Rebuild(DoubleConstantCurvatureAndToolfaceArcs section,
                                                 TrajectoryPoint3D start, out TrajectoryPoint3D junction)
        {
            junction = null;
            ConstantCurvatureAndToolfaceArcSection first = Forward(start,
                (double)section.DoubleCTCCurve.Curvature,
                (double)section.DoubleCTCCurve.UpstreamToolface,
                (double)section.DoubleCTCCurve.UpstreamLength);
            if (first == null)
            {
                return null;
            }
            junction = first.End;
            ConstantCurvatureAndToolfaceArcSection second = Forward(first.End,
                (double)section.DoubleCTCCurve.Curvature,
                (double)section.DoubleCTCCurve.DownstreamToolface,
                (double)section.DoubleCTCCurve.DownstreamLength);
            return second?.End;
        }

        /// <summary>
        /// What comes back has to be two curves of one curvature that really do reach the end station,
        /// in position and in attitude, and the junction reported has to be where the first of them
        /// actually ends.
        /// </summary>
        [Test]
        public void SolvedSectionReachesTheEndStationWithOneCurvature()
        {
            Random random = new Random(Seed);
            int checkedCases = 0;
            int declinedCases = 0;
            double worstAttitude = 0.0;
            double worstPosition = 0.0;
            double worstJunction = 0.0;

            for (int i = 0; i < Cases; i++)
            {
                var drawn = Reference(random, i % 5 == 0);
                if (drawn == null)
                {
                    continue;
                }
                checkedCases++;

                DoubleConstantCurvatureAndToolfaceArcs section = Solver(drawn.Value.start, drawn.Value.end);
                if (!section.CalculateXYZ())
                {
                    declinedCases++;
                    continue;
                }
                Assert.IsTrue(Numeric.IsDefined(section.DoubleCTCCurve.Curvature), "no curvature at case " + i);
                Assert.IsTrue(Numeric.IsDefined(section.DoubleCTCCurve.UpstreamToolface), "no toolface at case " + i);
                Assert.IsTrue(Numeric.IsDefined(section.DoubleCTCCurve.DownstreamToolface), "no toolface at case " + i);
                Assert.IsTrue(Produced(section.Intermediate), "no junction at case " + i);
                Assert.Greater((double)section.DoubleCTCCurve.Curvature, 0.0, "curvature at case " + i);

                if (section.HasZeroLengthArc(1.0e-6))
                {
                    // One curve covered the section, so there is only the one to check.
                    ConstantCurvatureAndToolfaceArcSection single = Forward(drawn.Value.start,
                        (double)section.DoubleCTCCurve.Curvature,
                        (double)section.DoubleCTCCurve.UpstreamToolface,
                        (double)section.DoubleCTCCurve.UpstreamLength);
                    Assert.IsNotNull(single, "single curve at case " + i);
                    worstAttitude = System.Math.Max(worstAttitude, AttitudeMiss(single.End, drawn.Value.end));
                    worstPosition = System.Math.Max(worstPosition, Distance(single.End, drawn.Value.end));
                    continue;
                }

                TrajectoryPoint3D reached = Rebuild(section, drawn.Value.start, out TrajectoryPoint3D junction);
                Assert.IsNotNull(reached, "the reported pair could not be rebuilt at case " + i);
                worstAttitude = System.Math.Max(worstAttitude, AttitudeMiss(reached, drawn.Value.end));
                worstPosition = System.Math.Max(worstPosition, Distance(reached, drawn.Value.end));
                worstJunction = System.Math.Max(worstJunction, Distance(junction, section.Intermediate));
            }

            Assert.Greater(checkedCases, Cases / 2);
            Assert.Less(declinedCases, System.Math.Max(2, checkedCases / 100), "declined too often");
            Assert.LessOrEqual(worstAttitude, 1.0e-4, "worst end attitude in radians");
            Assert.LessOrEqual(worstPosition, 1.0e-6, "worst end position in metres");
            Assert.LessOrEqual(worstJunction, 1.0e-6, "worst junction in metres");
        }

        /// <summary>
        /// The curvature is a pure scale on the shape of such a pair, so the two toolface angles and the
        /// two dimensionless lengths are what the construction really settles. Scaling a case up in size
        /// therefore has to scale the curvature down by the same factor and leave the toolface angles
        /// where they were.
        /// </summary>
        [Test]
        public void TheCurvatureIsAPureScale()
        {
            Random random = new Random(Seed + 3);
            int checkedCases = 0;
            double worstCurvature = 0.0;
            double worstToolface = 0.0;

            for (int i = 0; i < 120; i++)
            {
                var drawn = Reference(random, false);
                if (drawn == null)
                {
                    continue;
                }
                DoubleConstantCurvatureAndToolfaceArcs section = Solver(drawn.Value.start, drawn.Value.end);
                if (!section.CalculateXYZ() || section.HasZeroLengthArc(1.0e-6))
                {
                    continue;
                }

                // The same attitudes with every displacement from the start doubled.
                const double Factor = 2.0;
                TrajectoryPoint3D stretched = new TrajectoryPoint3D();
                stretched.Set(drawn.Value.end);
                stretched.X = (double)drawn.Value.start.X + Factor * (double)(drawn.Value.end.X - drawn.Value.start.X);
                stretched.Y = (double)drawn.Value.start.Y + Factor * (double)(drawn.Value.end.Y - drawn.Value.start.Y);
                stretched.Z = (double)drawn.Value.start.Z + Factor * (double)(drawn.Value.end.Z - drawn.Value.start.Z);

                DoubleConstantCurvatureAndToolfaceArcs scaled = Solver(drawn.Value.start, stretched);
                if (!scaled.CalculateXYZ() || scaled.HasZeroLengthArc(1.0e-6))
                {
                    continue;
                }
                checkedCases++;

                double expected = (double)section.DoubleCTCCurve.Curvature / Factor;
                worstCurvature = System.Math.Max(worstCurvature,
                    System.Math.Abs((double)scaled.DoubleCTCCurve.Curvature - expected) / expected);
                worstToolface = System.Math.Max(worstToolface,
                    System.Math.Abs((double)scaled.DoubleCTCCurve.UpstreamToolface -
                                    (double)section.DoubleCTCCurve.UpstreamToolface));
                worstToolface = System.Math.Max(worstToolface,
                    System.Math.Abs((double)scaled.DoubleCTCCurve.DownstreamToolface -
                                    (double)section.DoubleCTCCurve.DownstreamToolface));
            }

            Assert.Greater(checkedCases, 30);
            Assert.LessOrEqual(worstCurvature, 1.0e-7, "the curvature should scale as one over the size");
            Assert.LessOrEqual(worstToolface, 1.0e-7, "the toolface angles should not move with the size");
        }

        /// <summary>
        /// A section that one curve already covers is reported as one curve: the junction sits at the end
        /// and the two toolface angles are the same. Unlike a pair of circular arcs, two curves of this
        /// kind sharing a toolface really are one curve, since the toolface is held all the way along.
        /// </summary>
        [Test]
        public void ASingleCurveIsReportedAsOne()
        {
            Random random = new Random(Seed + 5);
            int checkedCases = 0;
            double worstCurvature = 0.0;
            for (int i = 0; i < 120; i++)
            {
                double inclination = 0.3 + (System.Math.PI - 0.6) * random.NextDouble();
                double azimuth = 2.0 * System.Math.PI * random.NextDouble();
                double curvature = 5.0e-4 + 3.0e-3 * random.NextDouble();
                double toolface = -System.Math.PI + 2.0 * System.Math.PI * random.NextDouble();
                TrajectoryPoint3D start = StartPoint(inclination, azimuth);

                ConstantCurvatureAndToolfaceArcSection curve =
                    Forward(start, curvature, toolface, 40.0 + 300.0 * random.NextDouble());
                if (curve == null)
                {
                    continue;
                }
                double finish = (double)curve.End.Inclination;
                if (finish <= 0.1 || finish >= System.Math.PI - 0.1)
                {
                    continue;
                }

                DoubleConstantCurvatureAndToolfaceArcs section = Solver(start, curve.End);
                Assert.IsTrue(section.CalculateXYZ(), "case " + i + " should be solved");
                checkedCases++;
                Assert.IsTrue(section.HasZeroLengthArc(1.0e-6), "case " + i + " should need only one curve");
                Assert.AreEqual((double)section.DoubleCTCCurve.UpstreamToolface,
                                (double)section.DoubleCTCCurve.DownstreamToolface, Tolerance,
                                "the two toolface angles should agree at case " + i);
                worstCurvature = System.Math.Max(worstCurvature,
                    System.Math.Abs((double)section.DoubleCTCCurve.Curvature - curvature) / curvature);
            }
            Assert.Greater(checkedCases, 60);
            Assert.LessOrEqual(worstCurvature, 1.0e-6, "the curvature of the single curve");
        }

        /// <summary>
        /// Nothing is reported that does not reach the end station. Stations drawn at random are used
        /// here; a pair of these curves reaches far more of them than a pair of circular arcs does,
        /// because the toolface can be held anywhere, so most of these are solved rather than turned
        /// down. What matters is that every one that is solved stands up to being rebuilt.
        /// </summary>
        [Test]
        public void NothingIsReportedThatDoesNotReachTheEnd()
        {
            Random random = new Random(Seed + 9);
            int accepted = 0;
            double worstAttitude = 0.0;
            double worstPosition = 0.0;
            for (int i = 0; i < 300; i++)
            {
                TrajectoryPoint3D start = StartPoint(0.1 + (System.Math.PI - 0.2) * random.NextDouble(),
                                                     2.0 * System.Math.PI * random.NextDouble());
                TrajectoryPoint3D end = new TrajectoryPoint3D
                {
                    X = -500.0 + 1000.0 * random.NextDouble(),
                    Y = -500.0 + 1000.0 * random.NextDouble(),
                    Z = 500.0 + (-500.0 + 1000.0 * random.NextDouble()),
                    Inclination = 0.1 + (System.Math.PI - 0.2) * random.NextDouble(),
                    Azimuth = 2.0 * System.Math.PI * random.NextDouble()
                };

                DoubleConstantCurvatureAndToolfaceArcs section = Solver(start, end);
                if (!section.CalculateXYZ())
                {
                    continue;
                }
                accepted++;
                if (section.HasZeroLengthArc(1.0e-6))
                {
                    continue;
                }
                TrajectoryPoint3D reached = Rebuild(section, start, out _);
                Assert.IsNotNull(reached, "the reported pair could not be rebuilt at case " + i);
                worstAttitude = System.Math.Max(worstAttitude, AttitudeMiss(reached, end));
                worstPosition = System.Math.Max(worstPosition, Distance(reached, end));
            }
            Assert.Greater(accepted, 50, "the sweep should accept some of these");
            Assert.LessOrEqual(worstAttitude, 1.0e-4, "worst end attitude in radians");
            Assert.LessOrEqual(worstPosition, 1.0e-4, "worst end position in metres");
        }

        /// <summary>
        /// Interpolating along the solved section gives back its two ends, passes through the junction at
        /// the junction measured depth, and never jumps: a chord can never be longer than the along hole
        /// distance it spans.
        /// </summary>
        [Test]
        public void InterpolationFollowsTheSolvedSection()
        {
            Random random = new Random(Seed + 11);
            int checkedCases = 0;
            double worstEnds = 0.0;
            double worstJunction = 0.0;
            double worstJump = 0.0;

            for (int i = 0; i < 60; i++)
            {
                var drawn = Reference(random, false);
                if (drawn == null)
                {
                    continue;
                }
                DoubleConstantCurvatureAndToolfaceArcs section = Solver(drawn.Value.start, drawn.Value.end);
                if (!section.CalculateXYZ() || section.HasZeroLengthArc(1.0e-6))
                {
                    continue;
                }
                Assert.IsTrue(Numeric.IsDefined(section.Intermediate.Abscissa), "no junction depth at case " + i);
                Assert.IsTrue(Numeric.IsDefined(section.End.Abscissa), "no end depth at case " + i);
                checkedCases++;

                double from = (double)section.Start.Abscissa;
                double to = (double)section.End.Abscissa;

                TrajectoryPoint3D atStart = new TrajectoryPoint3D();
                section.InterpolateAtMD(from, atStart);
                TrajectoryPoint3D atEnd = new TrajectoryPoint3D();
                section.InterpolateAtMD(to, atEnd);
                Assert.IsTrue(Produced(atStart) && Produced(atEnd), "ends undefined at case " + i);
                worstEnds = System.Math.Max(worstEnds, Distance(atStart, section.Start));
                worstEnds = System.Math.Max(worstEnds, Distance(atEnd, drawn.Value.end));

                TrajectoryPoint3D atJunction = new TrajectoryPoint3D();
                section.InterpolateAtMD((double)section.Intermediate.Abscissa, atJunction);
                Assert.IsTrue(Produced(atJunction), "junction undefined at case " + i);
                worstJunction = System.Math.Max(worstJunction, Distance(atJunction, section.Intermediate));

                const int Samples = 30;
                TrajectoryPoint3D previous = atStart;
                for (int n = 1; n <= Samples; n++)
                {
                    TrajectoryPoint3D sample = new TrajectoryPoint3D();
                    section.InterpolateAtMD(from + (to - from) * n / Samples, sample);
                    Assert.IsTrue(Produced(sample), "sample " + n + " undefined at case " + i);
                    worstJump = System.Math.Max(worstJump, Distance(sample, previous) - (to - from) / Samples);
                    previous = sample;
                }
            }

            Assert.Greater(checkedCases, 20);
            Assert.LessOrEqual(worstEnds, 1.0e-6, "worst end point miss in metres");
            Assert.LessOrEqual(worstJunction, 1.0e-6, "worst junction miss in metres");
            Assert.LessOrEqual(worstJump, 1.0e-9, "worst jump between consecutive samples in metres");
        }

        /// <summary>
        /// The same section solved from several threads at once gives the same answer every time. The
        /// working curves are held as locals for that reason.
        /// </summary>
        [Test]
        public void SolvingIsIndependentBetweenInstances()
        {
            Random random = new Random(Seed + 13);
            List<(TrajectoryPoint3D start, TrajectoryPoint3D end)> cases =
                new List<(TrajectoryPoint3D, TrajectoryPoint3D)>();
            List<double> expected = new List<double>();
            int attempts = 0;
            while (cases.Count < 20 && attempts < 400)
            {
                attempts++;
                var drawn = Reference(random, false);
                if (drawn == null)
                {
                    continue;
                }
                DoubleConstantCurvatureAndToolfaceArcs section = Solver(drawn.Value.start, drawn.Value.end);
                if (!section.CalculateXYZ())
                {
                    continue;
                }
                cases.Add((drawn.Value.start, drawn.Value.end));
                expected.Add((double)section.DoubleCTCCurve.Curvature);
            }
            Assert.Greater(cases.Count, 10);

            int disagreements = 0;
            System.Threading.Tasks.Parallel.For(0, 200,
                new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = 8 },
                n =>
                {
                    int index = n % cases.Count;
                    DoubleConstantCurvatureAndToolfaceArcs section = Solver(cases[index].start, cases[index].end);
                    if (!section.CalculateXYZ() ||
                        System.Math.Abs((double)section.DoubleCTCCurve.Curvature - expected[index]) >
                            1.0e-12 * System.Math.Abs(expected[index]))
                    {
                        System.Threading.Interlocked.Increment(ref disagreements);
                    }
                });
            Assert.AreEqual(0, disagreements, "solving the same section on several threads disagreed");
        }
    }
}
