using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using static OSDC.DotnetLibraries.Drilling.Section.UnitTest.SectionTestHelper;

namespace OSDC.DotnetLibraries.Drilling.Section.UnitTest
{
    /// <summary>
    /// A double arc section joins two fully defined stations with two circular arcs carrying the same
    /// curvature. The cases below are built by running two such arcs forward, so a solution is known to
    /// exist, and what comes back is then taken apart and checked against the station it had to reach.
    /// </summary>
    public class DoubleArcsTests
    {
        private static TrajectoryPoint3D Station(double inclination, double azimuth)
        {
            return StartPoint(inclination, azimuth);
        }

        private static void Tangent(TrajectoryPoint3D point, out double x, out double y, out double z)
        {
            double sine = System.Math.Sin((double)point.Inclination);
            x = sine * System.Math.Cos((double)point.Azimuth);
            y = sine * System.Math.Sin((double)point.Azimuth);
            z = System.Math.Cos((double)point.Inclination);
        }

        /// <summary>
        /// The angle between two attitudes, which counts the azimuth for nothing at the vertical and
        /// wraps correctly everywhere else.
        /// </summary>
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
        /// Two arcs of one curvature run forward, giving a start and an end station that a double arc
        /// section certainly joins. Each arc is kept to at most a half turn, which is as far as an arc
        /// can be told apart by the tangent at its end.
        /// </summary>
        private static (TrajectoryPoint3D start, TrajectoryPoint3D end)? Reference(Random random, bool sameToolface)
        {
            double inclination = 0.1 + (System.Math.PI - 0.2) * random.NextDouble();
            double azimuth = 2.0 * System.Math.PI * random.NextDouble();
            double curvature = 3.0e-4 + 5.0e-3 * random.NextDouble();
            double toolfaceFirst = -System.Math.PI + 2.0 * System.Math.PI * random.NextDouble();
            double toolfaceSecond = sameToolface
                ? toolfaceFirst
                : -System.Math.PI + 2.0 * System.Math.PI * random.NextDouble();
            // An arc is identified here by the tangent at its end, which cannot tell a turn of more than
            // half a circle from its shorter counterpart, so each arc is kept inside that.
            double longest = System.Math.PI / curvature;

            TrajectoryPoint3D start = Station(inclination, azimuth);

            CircularArcSection first = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
            first.Start.Set(start);
            first.Circle.Curvature = curvature;
            first.Circle.ReferenceToolface = toolfaceFirst;
            first.Circle.Length = 20.0 + System.Math.Min(300.0, longest - 20.0) * random.NextDouble();
            if (!first.CalculateLDT() || !Produced(first.End))
            {
                return null;
            }

            CircularArcSection second = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
            second.Start.Set(first.End);
            second.Circle.Curvature = curvature;
            second.Circle.ReferenceToolface = toolfaceSecond;
            second.Circle.Length = 20.0 + System.Math.Min(300.0, longest - 20.0) * random.NextDouble();
            if (!second.CalculateLDT() || !Produced(second.End))
            {
                return null;
            }
            if (second.End.Inclination == null || second.End.Azimuth == null)
            {
                return null;
            }

            TrajectoryPoint3D end = new TrajectoryPoint3D();
            end.Set(second.End);
            return (start, end);
        }

        private static DoubleArcs Solver(TrajectoryPoint3D start, TrajectoryPoint3D end)
        {
            DoubleArcs section = new DoubleArcs
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };
            section.Start.Set(start);
            section.End.Set(end);
            return section;
        }

        /// <summary>
        /// What comes back has to be two arcs that really do reach the end station with the attitude
        /// asked for, and that really do carry the same curvature. Both were checked afresh here rather
        /// than taken on trust, because the earlier solver fitted the junction point against a tolerance
        /// of about a radian and returned whatever the fit landed on: on this same sweep it claimed
        /// success almost every time while missing the end attitude by a couple of degrees typically and
        /// forty at worst, and while giving the two arcs quite different curvatures.
        /// </summary>
        [Test]
        public void SolvedSectionReachesTheEndStationWithOneCurvature()
        {
            Random random = new Random(Seed);
            int checkedCases = 0;
            int declinedCases = 0;
            double worstAttitude = 0.0;
            double worstCurvature = 0.0;
            double worstPosition = 0.0;

            for (int i = 0; i < SweepCount; i++)
            {
                var drawn = Reference(random, i % 5 == 0);
                if (drawn == null)
                {
                    continue;
                }
                checkedCases++;

                DoubleArcs section = Solver(drawn.Value.start, drawn.Value.end);
                if (!section.CalculateXYZ())
                {
                    declinedCases++;
                    continue;
                }
                Assert.IsTrue(Numeric.IsDefined(section.DoubleArcCurve.Curvature), "no curvature at case " + i);
                Assert.IsTrue(Produced(section.Intermediate), "no junction at case " + i);

                if (section.HasZeroLengthArc(1.0e-6))
                {
                    // A single arc covered the section, so there is only the one arc to check.
                    CircularArcSection single = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
                    single.Start.Set(drawn.Value.start);
                    single.End.Set(drawn.Value.end);
                    Assert.IsTrue(single.CalculateXYZ(), "single arc at case " + i);
                    worstAttitude = System.Math.Max(worstAttitude, AttitudeMiss(single.End, drawn.Value.end));
                    continue;
                }

                CircularArcSection first = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
                first.Start.Set(drawn.Value.start);
                first.End.Set(section.Intermediate);
                Assert.IsTrue(first.CalculateXYZ(), "first arc at case " + i);

                CircularArcSection second = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
                second.Start.Set(section.Intermediate);
                second.End.Set(drawn.Value.end);
                Assert.IsTrue(second.CalculateXYZ(), "second arc at case " + i);

                worstAttitude = System.Math.Max(worstAttitude, AttitudeMiss(second.End, drawn.Value.end));
                worstPosition = System.Math.Max(worstPosition, Distance(second.End, drawn.Value.end));

                double curvatureFirst = (double)first.Circle.Curvature;
                double curvatureSecond = (double)second.Circle.Curvature;
                double scale = System.Math.Max(System.Math.Abs(curvatureFirst), System.Math.Abs(curvatureSecond));
                if (scale > 0.0)
                {
                    worstCurvature = System.Math.Max(worstCurvature,
                        System.Math.Abs(curvatureFirst - curvatureSecond) / scale);
                    // and what was reported has to be that curvature, not something else
                    worstCurvature = System.Math.Max(worstCurvature,
                        System.Math.Abs((double)section.DoubleArcCurve.Curvature - curvatureFirst) / scale);
                }
            }

            Assert.Greater(checkedCases, SweepCount / 2);
            // Declining is allowed where the geometry is degenerate, but it has to stay rare.
            Assert.Less(declinedCases, checkedCases / 100, "declined too often");
            Assert.LessOrEqual(worstAttitude, 1.0e-4, "worst end attitude in radians");
            Assert.LessOrEqual(worstCurvature, 1.0e-6, "worst relative gap between the two curvatures");
            Assert.LessOrEqual(worstPosition, 1.0e-6, "worst end position in metres");
        }

        /// <summary>
        /// A section that one arc already covers is reported as one arc: the junction sits at the end and
        /// the two toolface angles are the same.
        ///
        /// The single arc has to be built as one arc. Two arcs sharing a reference toolface angle are not
        /// one arc between them, because the toolface is read against the high side, which turns as the
        /// curve does; that pair is a curve of constant curvature and toolface instead, and it is handled
        /// as the two arcs it is.
        /// </summary>
        [Test]
        public void ASingleArcIsReportedAsOne()
        {
            Random random = new Random(Seed + 5);
            int checkedCases = 0;
            for (int i = 0; i < 300; i++)
            {
                double inclination = 0.1 + (System.Math.PI - 0.2) * random.NextDouble();
                double azimuth = 2.0 * System.Math.PI * random.NextDouble();
                double curvature = 3.0e-4 + 5.0e-3 * random.NextDouble();
                TrajectoryPoint3D start = Station(inclination, azimuth);

                CircularArcSection arc = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
                arc.Start.Set(start);
                arc.Circle.Curvature = curvature;
                arc.Circle.ReferenceToolface = -System.Math.PI + 2.0 * System.Math.PI * random.NextDouble();
                // Kept inside a half turn, beyond which an arc is no longer told apart from the shorter
                // one joining the same two stations.
                arc.Circle.Length = 20.0 + (System.Math.PI / curvature - 20.0) * random.NextDouble();
                if (!arc.CalculateLDT() || !Produced(arc.End) || arc.End.Inclination == null)
                {
                    continue;
                }

                DoubleArcs section = Solver(start, arc.End);
                Assert.IsTrue(section.CalculateXYZ(), "case " + i + " should be solved");
                checkedCases++;
                Assert.IsTrue(section.HasZeroLengthArc(1.0e-6), "case " + i + " should need only one arc");
                Assert.AreEqual((double)section.DoubleArcCurve.UpstreamReferenceToolface,
                                (double)section.DoubleArcCurve.DownstreamReferenceToolface, Tolerance,
                                "the two toolface angles should agree at case " + i);
                Assert.AreEqual(curvature, (double)section.DoubleArcCurve.Curvature, 1.0e-9,
                                "the curvature should be the one the arc was built with at case " + i);
            }
            Assert.Greater(checkedCases, 200);
        }

        /// <summary>
        /// A section that no double arc of one curvature can make has to be turned down rather than
        /// answered with something that does not reach the end station. Stations drawn at random rarely
        /// admit one, so this is mostly a test that nothing is invented.
        /// </summary>
        [Test]
        public void NothingIsReportedThatDoesNotReachTheEnd()
        {
            Random random = new Random(Seed + 9);
            int accepted = 0;
            for (int i = 0; i < 2000; i++)
            {
                TrajectoryPoint3D start = Station(System.Math.PI * random.NextDouble(),
                                                  2.0 * System.Math.PI * random.NextDouble());
                TrajectoryPoint3D end = new TrajectoryPoint3D
                {
                    X = -500.0 + 1000.0 * random.NextDouble(),
                    Y = -500.0 + 1000.0 * random.NextDouble(),
                    Z = 500.0 + (-500.0 + 1000.0 * random.NextDouble()),
                    Inclination = System.Math.PI * random.NextDouble(),
                    Azimuth = 2.0 * System.Math.PI * random.NextDouble()
                };

                DoubleArcs section = Solver(start, end);
                if (!section.CalculateXYZ())
                {
                    continue;
                }
                accepted++;
                if (section.HasZeroLengthArc(1.0e-6))
                {
                    continue;
                }

                CircularArcSection first = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
                first.Start.Set(start);
                first.End.Set(section.Intermediate);
                CircularArcSection second = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
                second.Start.Set(section.Intermediate);
                second.End.Set(end);
                Assert.IsTrue(first.CalculateXYZ() && second.CalculateXYZ(), "arcs at case " + i);
                Assert.LessOrEqual(AttitudeMiss(second.End, end), 1.0e-4, "end attitude at case " + i);

                double curvatureFirst = (double)first.Circle.Curvature;
                double curvatureSecond = (double)second.Circle.Curvature;
                double scale = System.Math.Max(System.Math.Abs(curvatureFirst), System.Math.Abs(curvatureSecond));
                Assert.LessOrEqual(System.Math.Abs(curvatureFirst - curvatureSecond) / scale, 1.0e-6,
                                   "the two curvatures at case " + i);
            }
            Assert.Greater(accepted, 100, "the sweep should accept some of these");
        }

        /// <summary>
        /// Two arcs of one curvature sharing a reference toolface angle make a curve of constant
        /// curvature and constant toolface. It is still a double arc, and the solver has to come back
        /// with that same curvature and with the two toolface angles equal to the one it was built with.
        /// </summary>
        [Test]
        public void AConstantToolfacePairIsRecovered()
        {
            Random random = new Random(Seed + 7);
            int checkedCases = 0;
            double worstToolface = 0.0;
            for (int i = 0; i < 400; i++)
            {
                var drawn = Reference(random, true);
                if (drawn == null)
                {
                    continue;
                }
                DoubleArcs section = Solver(drawn.Value.start, drawn.Value.end);
                if (!section.CalculateXYZ() || section.HasZeroLengthArc(1.0e-6))
                {
                    continue;
                }
                checkedCases++;
                worstToolface = System.Math.Max(worstToolface,
                    System.Math.Abs((double)section.DoubleArcCurve.UpstreamReferenceToolface -
                                    (double)section.DoubleArcCurve.DownstreamReferenceToolface));
            }
            Assert.Greater(checkedCases, 100);
            Assert.LessOrEqual(worstToolface, 1.0e-6, "the two toolface angles should agree");
        }

        /// <summary>
        /// Interpolating along the solved section gives back its two ends, passes through the junction at
        /// the junction measured depth, and never jumps: consecutive samples cannot be further apart than
        /// the along hole distance between them.
        /// </summary>
        [Test]
        public void InterpolationFollowsTheSolvedSection()
        {
            Random random = new Random(Seed + 11);
            int checkedCases = 0;
            double worstEnds = 0.0;
            double worstJunction = 0.0;
            double worstJump = 0.0;

            for (int i = 0; i < 400; i++)
            {
                var drawn = Reference(random, false);
                if (drawn == null)
                {
                    continue;
                }
                DoubleArcs section = Solver(drawn.Value.start, drawn.Value.end);
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

                const int Samples = 40;
                TrajectoryPoint3D previous = atStart;
                for (int n = 1; n <= Samples; n++)
                {
                    TrajectoryPoint3D sample = new TrajectoryPoint3D();
                    section.InterpolateAtMD(from + (to - from) * n / Samples, sample);
                    Assert.IsTrue(Produced(sample), "sample " + n + " undefined at case " + i);
                    // A chord can never be longer than the arc it subtends, so any excess is a jump.
                    worstJump = System.Math.Max(worstJump,
                        Distance(sample, previous) - (to - from) / Samples);
                    previous = sample;
                }
            }

            Assert.Greater(checkedCases, 100);
            Assert.LessOrEqual(worstEnds, 1.0e-6, "worst end point miss in metres");
            Assert.LessOrEqual(worstJunction, 1.0e-6, "worst junction miss in metres");
            Assert.LessOrEqual(worstJump, 1.0e-9, "worst jump between consecutive samples in metres");
        }

        /// <summary>
        /// The same section solved from many threads at once has to give the same answer every time. The
        /// earlier solver kept its working arcs and a cache of its last evaluation in static fields, so
        /// two sections solved at the same time wrote over each other.
        /// </summary>
        [Test]
        public void SolvingIsIndependentBetweenInstances()
        {
            Random random = new Random(Seed + 13);
            List<(TrajectoryPoint3D start, TrajectoryPoint3D end)> cases = new List<(TrajectoryPoint3D, TrajectoryPoint3D)>();
            List<double> expected = new List<double>();
            while (cases.Count < 100)
            {
                var drawn = Reference(random, false);
                if (drawn == null)
                {
                    continue;
                }
                DoubleArcs section = Solver(drawn.Value.start, drawn.Value.end);
                if (!section.CalculateXYZ())
                {
                    continue;
                }
                cases.Add(drawn.Value);
                expected.Add((double)section.DoubleArcCurve.Curvature);
            }

            int disagreements = 0;
            System.Threading.Tasks.Parallel.For(0, 2000,
                new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = 8 },
                n =>
                {
                    int index = n % cases.Count;
                    DoubleArcs section = Solver(cases[index].start, cases[index].end);
                    bool solved = section.CalculateXYZ();
                    if (!solved ||
                        System.Math.Abs((double)section.DoubleArcCurve.Curvature - expected[index]) >
                            1.0e-12 * System.Math.Abs(expected[index]))
                    {
                        System.Threading.Interlocked.Increment(ref disagreements);
                    }
                });
            Assert.AreEqual(0, disagreements, "solving the same section on several threads disagreed");
        }
    }
}
