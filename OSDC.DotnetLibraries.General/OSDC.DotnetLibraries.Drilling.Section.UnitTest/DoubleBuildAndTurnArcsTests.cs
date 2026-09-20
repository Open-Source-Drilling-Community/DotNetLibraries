using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using static OSDC.DotnetLibraries.Drilling.Section.UnitTest.SectionTestHelper;

namespace OSDC.DotnetLibraries.Drilling.Section.UnitTest
{
    /// <summary>
    /// The same problem the double arc section solves, with the circular arc replaced by the constant
    /// build and turn curve.
    ///
    /// What is held in common is different here. A build and turn curve has no single curvature: at
    /// inclination i it is sqrt(b*b + t*t*sin(i)*sin(i)), which moves as the curve builds. The junction
    /// is the one place both curves are defined on the same station, so that is where their curvatures
    /// are made to agree, and the test below checks that they really do.
    /// </summary>
    public class DoubleBuildAndTurnArcsTests
    {
        private const int Cases = 1500;

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
        /// The curvature of a build and turn curve at a given inclination.
        /// </summary>
        private static double Curvature(double bur, double tur, double inclination)
        {
            double sine = System.Math.Sin(inclination);
            return System.Math.Sqrt(bur * bur + tur * tur * sine * sine);
        }

        private static BuildAndTurnArcSection Forward(TrajectoryPoint3D from, double bur, double tur, double length)
        {
            BuildAndTurnArcSection curve = new BuildAndTurnArcSection
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };
            curve.Start.Set(from);
            curve.BuildAndTurn.BUR = bur;
            curve.BuildAndTurn.TR = tur;
            curve.BuildAndTurn.Length = length;
            if (!curve.CalculateLBT() || !Produced(curve.End) ||
                curve.End.Inclination == null || curve.End.Azimuth == null)
            {
                return null;
            }
            return curve;
        }

        /// <summary>
        /// A pair of build and turn curves run forward, giving two stations that some such pair joins.
        /// The junction and the end are kept clear of the vertical, where a station loses its azimuth.
        /// </summary>
        private static (TrajectoryPoint3D start, TrajectoryPoint3D end)? Reference(Random random)
        {
            double inclination = 0.2 + (System.Math.PI - 0.4) * random.NextDouble();
            double azimuth = 2.0 * System.Math.PI * random.NextDouble();
            TrajectoryPoint3D start = StartPoint(inclination, azimuth);

            BuildAndTurnArcSection first = Forward(start,
                -0.005 + 0.010 * random.NextDouble(), -0.005 + 0.010 * random.NextDouble(),
                40.0 + 280.0 * random.NextDouble());
            if (first == null)
            {
                return null;
            }
            double junction = (double)first.End.Inclination;
            if (junction <= 0.1 || junction >= System.Math.PI - 0.1)
            {
                return null;
            }
            BuildAndTurnArcSection second = Forward(first.End,
                -0.005 + 0.010 * random.NextDouble(), -0.005 + 0.010 * random.NextDouble(),
                40.0 + 280.0 * random.NextDouble());
            if (second == null)
            {
                return null;
            }
            double finish = (double)second.End.Inclination;
            if (finish <= 0.1 || finish >= System.Math.PI - 0.1)
            {
                return null;
            }

            TrajectoryPoint3D end = new TrajectoryPoint3D();
            end.Set(second.End);
            return (start, end);
        }

        private static DoubleBuildAndTurnArcs Solver(TrajectoryPoint3D start, TrajectoryPoint3D end)
        {
            DoubleBuildAndTurnArcs section = new DoubleBuildAndTurnArcs
            {
                Start = new TrajectoryPoint3D(),
                End = new TrajectoryPoint3D()
            };
            section.Start.Set(start);
            section.End.Set(end);
            return section;
        }

        /// <summary>
        /// Rebuild the pair from the rates and lengths that were reported, and hand back where it really
        /// ends together with the station where the two curves meet.
        /// </summary>
        private static TrajectoryPoint3D Rebuild(DoubleBuildAndTurnArcs section, TrajectoryPoint3D start,
                                                 out TrajectoryPoint3D junction)
        {
            junction = null;
            NonLocalizedDoubleBuildAndTurnCurve curve = section.DoubleBuildAndTurnCurve;
            BuildAndTurnArcSection first = Forward(start, (double)curve.UpstreamBUR, (double)curve.UpstreamTR,
                                                  (double)curve.UpstreamLength);
            if (first == null)
            {
                return null;
            }
            junction = first.End;
            if ((double)curve.DownstreamLength <= 0.0)
            {
                return first.End;
            }
            BuildAndTurnArcSection second = Forward(first.End, (double)curve.DownstreamBUR,
                                                    (double)curve.DownstreamTR, (double)curve.DownstreamLength);
            return second?.End;
        }

        /// <summary>
        /// What comes back has to be two build and turn curves that really do reach the end station, in
        /// position and in attitude.
        /// </summary>
        [Test]
        public void SolvedSectionReachesTheEndStation()
        {
            Random random = new Random(Seed);
            int checkedCases = 0;
            int declinedCases = 0;
            double worstAttitude = 0.0;
            double worstPosition = 0.0;
            double worstJunction = 0.0;

            for (int i = 0; i < Cases; i++)
            {
                var drawn = Reference(random);
                if (drawn == null)
                {
                    continue;
                }
                checkedCases++;

                DoubleBuildAndTurnArcs section = Solver(drawn.Value.start, drawn.Value.end);
                if (!section.CalculateXYZ())
                {
                    declinedCases++;
                    continue;
                }
                NonLocalizedDoubleBuildAndTurnCurve curve = section.DoubleBuildAndTurnCurve;
                Assert.IsTrue(Numeric.IsDefined(curve.UpstreamBUR), "no upstream rate at case " + i);
                Assert.IsTrue(Numeric.IsDefined(curve.DownstreamBUR), "no downstream rate at case " + i);
                Assert.IsTrue(Numeric.IsDefined(curve.JunctionCurvature), "no junction curvature at case " + i);
                Assert.IsTrue(Produced(section.Intermediate), "no junction at case " + i);

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
        /// The quantity the section holds in common: the curvature of the first curve where it meets the
        /// second has to equal the curvature of the second one there, and that value has to be the one
        /// reported. This is what keeps the dogleg severity from stepping as the curve passes through the
        /// junction, and it is the whole reason the section is well posed at all.
        /// </summary>
        [Test]
        public void TheTwoCurvaturesAgreeAtTheJunction()
        {
            Random random = new Random(Seed + 1);
            int checkedCases = 0;
            double worstStep = 0.0;
            double worstReported = 0.0;

            for (int i = 0; i < Cases; i++)
            {
                var drawn = Reference(random);
                if (drawn == null)
                {
                    continue;
                }
                DoubleBuildAndTurnArcs section = Solver(drawn.Value.start, drawn.Value.end);
                if (!section.CalculateXYZ() || section.HasZeroLengthArc(1.0e-6))
                {
                    continue;
                }
                checkedCases++;

                NonLocalizedDoubleBuildAndTurnCurve curve = section.DoubleBuildAndTurnCurve;
                Rebuild(section, drawn.Value.start, out TrajectoryPoint3D junction);
                Assert.IsNotNull(junction, "no junction at case " + i);

                double atJunction = (double)junction.Inclination;
                double upstream = Curvature((double)curve.UpstreamBUR, (double)curve.UpstreamTR, atJunction);
                double downstream = Curvature((double)curve.DownstreamBUR, (double)curve.DownstreamTR, atJunction);
                double scale = System.Math.Max(upstream, downstream);
                Assert.Greater(scale, 0.0, "a curvature of nothing at case " + i);

                worstStep = System.Math.Max(worstStep, System.Math.Abs(upstream - downstream) / scale);
                worstReported = System.Math.Max(worstReported,
                    System.Math.Abs((double)curve.JunctionCurvature - upstream) / scale);
            }

            Assert.Greater(checkedCases, Cases / 4);
            Assert.LessOrEqual(worstStep, 1.0e-9, "the two curvatures should agree where the curves meet");
            Assert.LessOrEqual(worstReported, 1.0e-9, "the reported junction curvature should be that value");
        }

        /// <summary>
        /// Asking for a ratio other than one deliberately softens one curve against the other, and the
        /// ratio that comes back has to be the one that was asked for.
        /// </summary>
        [Test]
        public void TheCurvatureRatioIsHonoured()
        {
            Random random = new Random(Seed + 3);
            int checkedCases = 0;
            double worstRatio = 0.0;
            double worstPosition = 0.0;

            foreach (double ratio in new double[] { 0.5, 2.0 })
            {
                for (int i = 0; i < 300; i++)
                {
                    var drawn = Reference(random);
                    if (drawn == null)
                    {
                        continue;
                    }
                    DoubleBuildAndTurnArcs section = Solver(drawn.Value.start, drawn.Value.end);
                    if (!section.CalculateXYZ(0, ratio) || section.HasZeroLengthArc(1.0e-6))
                    {
                        continue;
                    }
                    checkedCases++;

                    NonLocalizedDoubleBuildAndTurnCurve curve = section.DoubleBuildAndTurnCurve;
                    TrajectoryPoint3D reached = Rebuild(section, drawn.Value.start, out TrajectoryPoint3D junction);
                    Assert.IsNotNull(reached, "the reported pair could not be rebuilt");
                    worstPosition = System.Math.Max(worstPosition, Distance(reached, drawn.Value.end));

                    double atJunction = (double)junction.Inclination;
                    double upstream = Curvature((double)curve.UpstreamBUR, (double)curve.UpstreamTR, atJunction);
                    double downstream = Curvature((double)curve.DownstreamBUR, (double)curve.DownstreamTR, atJunction);
                    worstRatio = System.Math.Max(worstRatio,
                        System.Math.Abs(upstream / downstream - ratio) / ratio);
                }
            }

            Assert.Greater(checkedCases, 100);
            Assert.LessOrEqual(worstRatio, 1.0e-9, "the ratio of the two curvatures at the junction");
            Assert.LessOrEqual(worstPosition, 1.0e-6, "worst end position in metres");
        }

        /// <summary>
        /// A section that one curve already covers is reported as one curve: the junction sits at the end
        /// and both curves carry the same rates.
        /// </summary>
        [Test]
        public void ASingleCurveIsReportedAsOne()
        {
            Random random = new Random(Seed + 5);
            int checkedCases = 0;
            double worstRates = 0.0;
            for (int i = 0; i < 300; i++)
            {
                double inclination = 0.3 + (System.Math.PI - 0.6) * random.NextDouble();
                double azimuth = 2.0 * System.Math.PI * random.NextDouble();
                double bur = -0.004 + 0.008 * random.NextDouble();
                double tur = -0.004 + 0.008 * random.NextDouble();
                if (System.Math.Abs(bur) < 1.0e-4 || System.Math.Abs(tur) < 1.0e-4)
                {
                    continue;
                }
                TrajectoryPoint3D start = StartPoint(inclination, azimuth);
                BuildAndTurnArcSection curve = Forward(start, bur, tur, 50.0 + 250.0 * random.NextDouble());
                if (curve == null)
                {
                    continue;
                }
                double finish = (double)curve.End.Inclination;
                if (finish <= 0.15 || finish >= System.Math.PI - 0.15)
                {
                    continue;
                }
                // Only the turns that stay inside half a circle are told apart from their shorter
                // counterpart by a wrapped azimuth.
                if (System.Math.Abs(tur * (double)curve.BuildAndTurn.Length) > System.Math.PI - 0.1)
                {
                    continue;
                }

                DoubleBuildAndTurnArcs section = Solver(start, curve.End);
                Assert.IsTrue(section.CalculateXYZ(), "case " + i + " should be solved");
                checkedCases++;
                Assert.IsTrue(section.HasZeroLengthArc(1.0e-6), "case " + i + " should need only one curve");
                worstRates = System.Math.Max(worstRates,
                    System.Math.Abs((double)section.DoubleBuildAndTurnCurve.UpstreamBUR - bur));
                worstRates = System.Math.Max(worstRates,
                    System.Math.Abs((double)section.DoubleBuildAndTurnCurve.UpstreamTR - tur));
            }
            Assert.Greater(checkedCases, 100);
            Assert.LessOrEqual(worstRates, 1.0e-9, "the rates of the single curve");
        }

        /// <summary>
        /// Nothing is reported that does not reach the end station. Stations drawn at random are used
        /// here; a pair of these curves reaches most of them, so what matters is that every one that is
        /// solved stands up to being rebuilt, and that the curvatures still agree at the junction.
        /// </summary>
        [Test]
        public void NothingIsReportedThatDoesNotReachTheEnd()
        {
            Random random = new Random(Seed + 9);
            int accepted = 0;
            double worstAttitude = 0.0;
            double worstPosition = 0.0;
            double worstStep = 0.0;
            for (int i = 0; i < 600; i++)
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

                DoubleBuildAndTurnArcs section = Solver(start, end);
                if (!section.CalculateXYZ())
                {
                    continue;
                }
                accepted++;
                if (section.HasZeroLengthArc(1.0e-6))
                {
                    continue;
                }
                TrajectoryPoint3D reached = Rebuild(section, start, out TrajectoryPoint3D junction);
                Assert.IsNotNull(reached, "the reported pair could not be rebuilt at case " + i);
                worstAttitude = System.Math.Max(worstAttitude, AttitudeMiss(reached, end));
                worstPosition = System.Math.Max(worstPosition, Distance(reached, end));

                NonLocalizedDoubleBuildAndTurnCurve curve = section.DoubleBuildAndTurnCurve;
                double atJunction = (double)junction.Inclination;
                double upstream = Curvature((double)curve.UpstreamBUR, (double)curve.UpstreamTR, atJunction);
                double downstream = Curvature((double)curve.DownstreamBUR, (double)curve.DownstreamTR, atJunction);
                worstStep = System.Math.Max(worstStep,
                    System.Math.Abs(upstream - downstream) / System.Math.Max(upstream, downstream));
            }
            Assert.Greater(accepted, 100, "the sweep should accept some of these");
            Assert.LessOrEqual(worstAttitude, 1.0e-4, "worst end attitude in radians");
            Assert.LessOrEqual(worstPosition, 1.0e-4, "worst end position in metres");
            Assert.LessOrEqual(worstStep, 1.0e-9, "the two curvatures should agree where the curves meet");
        }

        /// <summary>
        /// Interpolating along the solved section gives back its two ends, passes through the junction at
        /// the junction measured depth, and never jumps.
        /// </summary>
        [Test]
        public void InterpolationFollowsTheSolvedSection()
        {
            Random random = new Random(Seed + 11);
            int checkedCases = 0;
            double worstEnds = 0.0;
            double worstJunction = 0.0;
            double worstJump = 0.0;

            for (int i = 0; i < 200; i++)
            {
                var drawn = Reference(random);
                if (drawn == null)
                {
                    continue;
                }
                DoubleBuildAndTurnArcs section = Solver(drawn.Value.start, drawn.Value.end);
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

            Assert.Greater(checkedCases, 50);
            Assert.LessOrEqual(worstEnds, 1.0e-6, "worst end point miss in metres");
            Assert.LessOrEqual(worstJunction, 1.0e-6, "worst junction miss in metres");
            Assert.LessOrEqual(worstJump, 1.0e-9, "worst jump between consecutive samples in metres");
        }

        /// <summary>
        /// The same section solved from several threads at once gives the same answer every time.
        /// </summary>
        [Test]
        public void SolvingIsIndependentBetweenInstances()
        {
            Random random = new Random(Seed + 13);
            List<(TrajectoryPoint3D start, TrajectoryPoint3D end)> cases =
                new List<(TrajectoryPoint3D, TrajectoryPoint3D)>();
            List<double> expected = new List<double>();
            int attempts = 0;
            while (cases.Count < 60 && attempts < 600)
            {
                attempts++;
                var drawn = Reference(random);
                if (drawn == null)
                {
                    continue;
                }
                DoubleBuildAndTurnArcs section = Solver(drawn.Value.start, drawn.Value.end);
                if (!section.CalculateXYZ())
                {
                    continue;
                }
                cases.Add(drawn.Value);
                expected.Add((double)section.DoubleBuildAndTurnCurve.JunctionCurvature);
            }
            Assert.Greater(cases.Count, 30);

            int disagreements = 0;
            System.Threading.Tasks.Parallel.For(0, 600,
                new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = 8 },
                n =>
                {
                    int index = n % cases.Count;
                    DoubleBuildAndTurnArcs section = Solver(cases[index].start, cases[index].end);
                    if (!section.CalculateXYZ() ||
                        System.Math.Abs((double)section.DoubleBuildAndTurnCurve.JunctionCurvature - expected[index]) >
                            1.0e-12 * System.Math.Abs(expected[index]))
                    {
                        System.Threading.Interlocked.Increment(ref disagreements);
                    }
                });
            Assert.AreEqual(0, disagreements, "solving the same section on several threads disagreed");
        }
    }
}
