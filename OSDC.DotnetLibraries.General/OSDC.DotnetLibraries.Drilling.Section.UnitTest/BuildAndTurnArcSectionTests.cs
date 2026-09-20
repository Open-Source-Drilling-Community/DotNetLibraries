using OSDC.DotnetLibraries.General.Math;
using static OSDC.DotnetLibraries.Drilling.Section.UnitTest.SectionTestHelper;

namespace OSDC.DotnetLibraries.Drilling.Section.UnitTest
{
    public class BuildAndTurnArcSectionTests
    {
        private static BuildAndTurnArcSection Section(double inclination, double azimuth)
        {
            return new BuildAndTurnArcSection
            {
                Start = StartPoint(inclination, azimuth),
                End = new TrajectoryPoint3D()
            };
        }

        private static (BuildAndTurnArcSection section, double bur, double tur, double length)?
            Reference(Random random)
        {
            double inclination = 0.05 + (System.Math.PI - 0.10) * random.NextDouble();
            double azimuth = 2.0 * System.Math.PI * random.NextDouble();
            double bur = -0.008 + 0.016 * random.NextDouble();
            double tur = -0.010 + 0.020 * random.NextDouble();
            double length = 10.0 + 500.0 * random.NextDouble();
            double inclinationEnd = inclination + bur * length;
            if (inclinationEnd <= 0.02 || inclinationEnd >= System.Math.PI - 0.02)
            {
                return null;
            }
            if (System.Math.Abs(bur) < 1e-6 || System.Math.Abs(tur) < 1e-6)
            {
                return null;
            }
            // The azimuth is carried wrapped, so a curve turning more than half a turn is no longer told
            // apart from a shorter one finishing on the same azimuth. Keeping the draw inside half a turn
            // leaves every combination below with a single answer, which is what lets these compare the
            // end point directly.
            if (System.Math.Abs(tur * length) > System.Math.PI - 0.1)
            {
                return null;
            }

            BuildAndTurnArcSection reference = Section(inclination, azimuth);
            reference.End.Abscissa = 1000.0 + length;
            reference.BuildAndTurn.BUR = bur;
            reference.BuildAndTurn.TR = tur;
            if (!reference.CalculateBTS() || !Produced(reference.End))
            {
                return null;
            }
            return (reference, bur, tur, length);
        }

        /// <summary>
        /// Given the two rates and an end inclination the curve really does reach, a curve has to come
        /// back and it has to reach that inclination.
        ///
        /// This is a regression test on two counts. CalculateBTI worked out the length and then handed
        /// over without ever setting the end measured depth, so the construction it delegated to had
        /// nothing to work from and declined every single time, taking CalculateBTZ down with it. It
        /// also carried a fallback adding a whole turn to the inclination when the requested change ran
        /// against the build up rate. The inclination of a build and turn curve varies linearly and
        /// does not wrap, so no curve reaches such a request and the fallback produced a curve driven
        /// through the vertical and back round instead.
        /// </summary>
        [Test]
        public void CalculateBTIReachesTheRequestedInclination()
        {
            Random random = new Random(Seed);
            int checkedCases = 0;
            double worst = 0.0;
            for (int i = 0; i < SweepCount; i++)
            {
                var drawn = Reference(random);
                if (drawn == null)
                {
                    continue;
                }
                var reference = drawn.Value;

                BuildAndTurnArcSection section = Section((double)reference.section.Start.Inclination,
                                                         (double)reference.section.Start.Azimuth);
                section.BuildAndTurn.BUR = reference.bur;
                section.BuildAndTurn.TR = reference.tur;
                section.End.Inclination = reference.section.End.Inclination;

                Assert.IsTrue(section.CalculateBTI(), "declined at case " + i);
                Assert.IsTrue(Produced(section.End), "no end point at case " + i);
                worst = System.Math.Max(worst, Difference(section.End.Inclination, reference.section.End.Inclination));
                checkedCases++;
            }
            Assert.Greater(checkedCases, SweepCount / 2);
            Assert.LessOrEqual(worst, Tolerance, "worst inclination residual");
        }

        /// <summary>
        /// An inclination the curve cannot reach, because getting there would mean building the wrong
        /// way, has to be declined rather than answered with a curve carried through the vertical.
        /// </summary>
        [Test]
        public void CalculateBTIDeclinesAnInclinationAgainstTheBuildUpRate()
        {
            BuildAndTurnArcSection section = Section(1.0, 0.5);
            section.BuildAndTurn.BUR = 0.002;
            section.BuildAndTurn.TR = 0.001;
            section.End.Inclination = 0.7;   // below the start, while the curve is building

            Assert.IsFalse(section.CalculateBTI());
        }

        /// <summary>
        /// Given the two rates and an end depth the curve really does reach, the curve that comes back
        /// has to reach it. The earlier code inverted the depth through a single branch of the arc
        /// sine, so an end inclination past the horizontal could never be produced.
        /// </summary>
        [Test]
        public void CalculateBTZReachesTheRequestedDepth()
        {
            Random random = new Random(Seed);
            int checkedCases = 0;
            double worst = 0.0;
            for (int i = 0; i < SweepCount; i++)
            {
                var drawn = Reference(random);
                if (drawn == null)
                {
                    continue;
                }
                var reference = drawn.Value;

                BuildAndTurnArcSection section = Section((double)reference.section.Start.Inclination,
                                                         (double)reference.section.Start.Azimuth);
                section.BuildAndTurn.BUR = reference.bur;
                section.BuildAndTurn.TR = reference.tur;
                section.End.Z = reference.section.End.Z;

                Assert.IsTrue(section.CalculateBTZ(), "declined at case " + i);
                Assert.IsTrue(Produced(section.End), "no end point at case " + i);
                worst = System.Math.Max(worst, Difference(section.End.Z, reference.section.End.Z));
                checkedCases++;
            }
            Assert.Greater(checkedCases, SweepCount / 2);
            Assert.LessOrEqual(worst, Tolerance, "worst depth residual");
        }

        /// <summary>
        /// A build and turn curve that starts above the horizontal and builds through it crosses a given
        /// depth twice. Whichever of the two the solver picks, the depth it lands on has to be the one
        /// asked for, and the end inclination past the horizontal has to be reachable at all.
        /// </summary>
        [Test]
        public void CalculateBTZReachesADepthPastTheHorizontal()
        {
            double inclinationStart = 1.3;    // just under the horizontal
            double bur = 0.003;               // building through it
            double tur = 0.001;
            double length = 200.0;

            BuildAndTurnArcSection reference = Section(inclinationStart, 0.4);
            reference.End.Abscissa = 1000.0 + length;
            reference.BuildAndTurn.BUR = bur;
            reference.BuildAndTurn.TR = tur;
            Assert.IsTrue(reference.CalculateBTS());
            Assert.Greater((double)reference.End.Inclination, System.Math.PI / 2.0,
                           "the reference curve should finish past the horizontal");

            BuildAndTurnArcSection section = Section(inclinationStart, 0.4);
            section.BuildAndTurn.BUR = bur;
            section.BuildAndTurn.TR = tur;
            section.End.Z = reference.End.Z;

            Assert.IsTrue(section.CalculateBTZ());
            Assert.IsTrue(Produced(section.End));
            Assert.AreEqual((double)reference.End.Z, (double)section.End.Z, Tolerance);
        }

        /// <summary>
        /// The well posed combinations reproduce the curve exactly.
        /// </summary>
        [Test]
        public void WellPosedCombinationsReproduceTheCurve()
        {
            Random random = new Random(Seed);
            int checkedCases = 0;
            double worst = 0.0;
            for (int i = 0; i < SweepCount; i++)
            {
                var drawn = Reference(random);
                if (drawn == null)
                {
                    continue;
                }
                var reference = drawn.Value;
                TrajectoryPoint3D expected = reference.section.End;
                double inclinationStart = (double)reference.section.Start.Inclination;
                double azimuthStart = (double)reference.section.Start.Azimuth;
                int caseIndex = i;

                void Check(string name, Action<BuildAndTurnArcSection> setup, Func<BuildAndTurnArcSection, bool> call)
                {
                    BuildAndTurnArcSection section = Section(inclinationStart, azimuthStart);
                    setup(section);
                    Assert.IsTrue(call(section), name + " declined at case " + caseIndex);
                    Assert.IsTrue(Produced(section.End), name + " gave no end point at case " + caseIndex);
                    double miss = System.Math.Sqrt(
                        System.Math.Pow((double)section.End.X - (double)expected.X, 2) +
                        System.Math.Pow((double)section.End.Y - (double)expected.Y, 2) +
                        System.Math.Pow((double)section.End.Z - (double)expected.Z, 2));
                    worst = System.Math.Max(worst, miss);
                }

                Check("BTS", s =>
                {
                    s.BuildAndTurn.BUR = reference.bur;
                    s.BuildAndTurn.TR = reference.tur;
                    s.End.Abscissa = expected.Abscissa;
                }, s => s.CalculateBTS());
                Check("LBT", s =>
                {
                    s.BuildAndTurn.BUR = reference.bur;
                    s.BuildAndTurn.TR = reference.tur;
                    s.BuildAndTurn.Length = reference.length;
                }, s => s.CalculateLBT());
                Check("SIA", s =>
                {
                    s.End.Abscissa = expected.Abscissa;
                    s.End.Inclination = expected.Inclination;
                    s.End.Azimuth = expected.Azimuth;
                }, s => s.CalculateSIA());
                Check("XYZ", s =>
                {
                    s.End.X = expected.X;
                    s.End.Y = expected.Y;
                    s.End.Z = expected.Z;
                }, s => s.CalculateXYZ());
                checkedCases++;
            }
            Assert.Greater(checkedCases, SweepCount / 2);
            Assert.LessOrEqual(worst, 1e-6, "worst position miss in metres");
        }

        /// <summary>
        /// The combinations that fix a rate, a length and an inclination leave the turn rate free, so
        /// they do not determine a curve. They are expected to decline rather than to invent one.
        ///
        /// Fixing the build up rate together with an end inclination already fixes the length, which
        /// makes the third quantity either redundant or contradictory, and in neither case does
        /// anything pin down how much the curve turns.
        /// </summary>
        [Test]
        public void UnderDeterminedCombinationsDecline()
        {
            Random random = new Random(Seed);
            int checkedCases = 0;
            for (int i = 0; i < 200; i++)
            {
                var drawn = Reference(random);
                if (drawn == null)
                {
                    continue;
                }
                var reference = drawn.Value;
                TrajectoryPoint3D expected = reference.section.End;
                double inclinationStart = (double)reference.section.Start.Inclination;
                double azimuthStart = (double)reference.section.Start.Azimuth;

                BuildAndTurnArcSection bsi = Section(inclinationStart, azimuthStart);
                bsi.BuildAndTurn.BUR = reference.bur;
                bsi.End.Abscissa = expected.Abscissa;
                bsi.End.Inclination = expected.Inclination;
                Assert.IsFalse(bsi.CalculateBSI(), "BSI at case " + i);

                BuildAndTurnArcSection bli = Section(inclinationStart, azimuthStart);
                bli.BuildAndTurn.BUR = reference.bur;
                bli.BuildAndTurn.Length = reference.length;
                bli.End.Inclination = expected.Inclination;
                Assert.IsFalse(bli.CalculateBLI(), "BLI at case " + i);

                BuildAndTurnArcSection biz = Section(inclinationStart, azimuthStart);
                biz.BuildAndTurn.BUR = reference.bur;
                biz.End.Inclination = expected.Inclination;
                biz.End.Z = expected.Z;
                Assert.IsFalse(biz.CalculateBIZ(), "BIZ at case " + i);

                checkedCases++;
            }
            Assert.Greater(checkedCases, 50);
        }
    }
}
