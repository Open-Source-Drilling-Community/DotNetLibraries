using OSDC.DotnetLibraries.General.Math;
using static OSDC.DotnetLibraries.Drilling.Section.UnitTest.SectionTestHelper;

namespace OSDC.DotnetLibraries.Drilling.Section.UnitTest
{
    public class CircularArcSectionTests
    {
        private static CircularArcSection Section(double inclination, double azimuth)
        {
            return new CircularArcSection
            {
                Start = StartPoint(inclination, azimuth),
                End = new TrajectoryPoint3D()
            };
        }

        /// <summary>
        /// Builds a reference arc and hands back the curve and its end point, or null when the draw is
        /// not usable. The arc angle runs up to a whole turn on purpose: the inclination along an arc is
        /// not monotonic, and a formulation that quietly assumes a short arc only fails beyond a quarter
        /// turn.
        /// </summary>
        private static (CircularArcSection section, double curvature, double toolface, double length)?
            Reference(Random random, bool degenerateToolface)
        {
            double inclination = random.NextDouble() * System.Math.PI;
            double azimuth = 2.0 * System.Math.PI * random.NextDouble();
            double toolface = degenerateToolface
                ? new double[] { 0.0, System.Math.PI / 2.0, System.Math.PI, -System.Math.PI / 2.0, -System.Math.PI }[random.Next(5)]
                : -System.Math.PI + 2.0 * System.Math.PI * random.NextDouble();
            double curvature = 1e-5 + 8e-3 * random.NextDouble();
            double arcAngle = 0.001 + (2.0 * System.Math.PI - 0.002) * random.NextDouble();
            double length = arcAngle / curvature;

            CircularArcSection reference = Section(inclination, azimuth);
            reference.Circle.Curvature = curvature;
            reference.Circle.ReferenceToolface = toolface;
            reference.Circle.Length = length;
            if (!reference.CalculateLDT() || !Produced(reference.End))
            {
                return null;
            }
            return (reference, curvature, toolface, length);
        }

        /// <summary>
        /// Given the curvature, the toolface angle and an end inclination that the arc really does
        /// reach, the arc that comes back has to reach it.
        ///
        /// This is a regression test. The general branch of CalculateDTI solved for the half angle
        /// tangent and then took the arc tangent of the root, where the substitution t = tan(w/2) calls
        /// for twice that. Every arc outside the special cases came back with the wrong length, by as
        /// much as forty degrees of inclination.
        /// </summary>
        [Test]
        public void CalculateDTIReachesTheRequestedInclination()
        {
            Random random = new Random(Seed);
            int checkedCases = 0;
            double worst = 0.0;
            for (int i = 0; i < SweepCount; i++)
            {
                var drawn = Reference(random, i % 7 == 0);
                if (drawn == null)
                {
                    continue;
                }
                var reference = drawn.Value;

                CircularArcSection section = Section((double)reference.section.Start.Inclination,
                                                     (double)reference.section.Start.Azimuth);
                section.Circle.Curvature = reference.curvature;
                section.Circle.ReferenceToolface = reference.toolface;
                section.End.Inclination = reference.section.End.Inclination;

                Assert.IsTrue(section.CalculateDTI(), "declined at case " + i);
                Assert.IsTrue(Produced(section.End), "no end point at case " + i);
                worst = System.Math.Max(worst, Difference(section.End.Inclination, reference.section.End.Inclination));
                checkedCases++;
            }
            Assert.Greater(checkedCases, SweepCount / 2);
            Assert.LessOrEqual(worst, Tolerance, "worst inclination residual");
        }

        /// <summary>
        /// The same for the true vertical depth. The earlier formulation inverted the depth through a
        /// single branch of the arc sine and carried a separate case for a toolface in the build plane,
        /// so arcs reaching their depth on the far side of the turn were missed.
        /// </summary>
        [Test]
        public void CalculateDTZReachesTheRequestedDepth()
        {
            Random random = new Random(Seed);
            int checkedCases = 0;
            double worst = 0.0;
            for (int i = 0; i < SweepCount; i++)
            {
                var drawn = Reference(random, i % 7 == 0);
                if (drawn == null)
                {
                    continue;
                }
                var reference = drawn.Value;

                CircularArcSection section = Section((double)reference.section.Start.Inclination,
                                                     (double)reference.section.Start.Azimuth);
                section.Circle.Curvature = reference.curvature;
                section.Circle.ReferenceToolface = reference.toolface;
                section.End.Z = reference.section.End.Z;

                Assert.IsTrue(section.CalculateDTZ(), "declined at case " + i);
                Assert.IsTrue(Produced(section.End), "no end point at case " + i);
                worst = System.Math.Max(worst, Difference(section.End.Z, reference.section.End.Z));
                checkedCases++;
            }
            Assert.Greater(checkedCases, SweepCount / 2);
            Assert.LessOrEqual(worst, Tolerance, "worst depth residual");
        }

        /// <summary>
        /// A length is a length and an arc angle is an angle. The earlier CalculateDTI added a bare two
        /// pi to a length when the requested inclination lay on the other side of the start, which is a
        /// whole turn of arc only if the curvature happens to be one. A gentle arc is what exposes it,
        /// so this pins a small curvature explicitly.
        /// </summary>
        [Test]
        public void CalculateDTIReturnsALengthAndNotAnAngle()
        {
            double curvature = 0.002;
            double inclinationStart = 1.2;

            CircularArcSection section = Section(inclinationStart, 0.7);
            section.Circle.Curvature = curvature;
            section.Circle.ReferenceToolface = 0.0;
            // Dropping back below the start inclination, which a toolface in the build plane reaches
            // only by carrying the arc round past the vertical.
            section.End.Inclination = inclinationStart - 0.3;

            Assert.IsTrue(section.CalculateDTI());
            Assert.IsTrue(Produced(section.End));
            Assert.AreEqual(inclinationStart - 0.3, (double)section.End.Inclination, Tolerance);

            // Getting back below the start inclination means carrying the arc round past the vertical,
            // so the arc angle is most of a whole turn and the length at this curvature is thousands of
            // metres. The earlier code added a bare two pi to the length rather than to the arc angle,
            // which at a curvature of two thousandths came out at a few metres, and negative at that.
            Assert.Greater((double)section.Circle.Length, 1000.0);

            // Halving the curvature has to double the length, which a bare angle term would not do.
            CircularArcSection gentler = Section(inclinationStart, 0.7);
            gentler.Circle.Curvature = 0.5 * curvature;
            gentler.Circle.ReferenceToolface = 0.0;
            gentler.End.Inclination = inclinationStart - 0.3;
            Assert.IsTrue(gentler.CalculateDTI());
            Assert.AreEqual(2.0 * (double)section.Circle.Length, (double)gentler.Circle.Length, 1e-6);
        }

        /// <summary>
        /// The combinations that are well posed have to reproduce the curve exactly, since for those
        /// there is only the one answer.
        /// </summary>
        [Test]
        public void WellPosedCombinationsReproduceTheCurve()
        {
            Random random = new Random(Seed);
            int checkedCases = 0;
            double worst = 0.0;
            for (int i = 0; i < SweepCount; i++)
            {
                var drawn = Reference(random, i % 7 == 0);
                if (drawn == null)
                {
                    continue;
                }
                var reference = drawn.Value;
                TrajectoryPoint3D expected = reference.section.End;
                double inclinationStart = (double)reference.section.Start.Inclination;
                double azimuthStart = (double)reference.section.Start.Azimuth;
                int caseIndex = i;

                void Check(string name, Action<CircularArcSection> setup, Func<CircularArcSection, bool> call)
                {
                    CircularArcSection section = Section(inclinationStart, azimuthStart);
                    setup(section);
                    Assert.IsTrue(call(section), name + " declined at case " + caseIndex);
                    Assert.IsTrue(Produced(section.End), name + " gave no end point at case " + caseIndex);
                    double miss = System.Math.Sqrt(
                        System.Math.Pow((double)section.End.X - (double)expected.X, 2) +
                        System.Math.Pow((double)section.End.Y - (double)expected.Y, 2) +
                        System.Math.Pow((double)section.End.Z - (double)expected.Z, 2));
                    worst = System.Math.Max(worst, miss);
                }

                Check("SDT", s =>
                {
                    s.Circle.Curvature = reference.curvature;
                    s.Circle.ReferenceToolface = reference.toolface;
                    s.End.Abscissa = expected.Abscissa;
                }, s => s.CalculateSDT());
                Check("LDT", s =>
                {
                    s.Circle.Curvature = reference.curvature;
                    s.Circle.ReferenceToolface = reference.toolface;
                    s.Circle.Length = reference.length;
                }, s => s.CalculateLDT());
                // Reaching a target point is only well posed for the short arc. Past half a turn the arc
                // curls back on itself and heads for its start again, so a point reached by a long arc
                // is reached by a short one too and the construction rightly returns that one instead.
                if (reference.curvature * reference.length < System.Math.PI)
                {
                    Check("XYZ", s =>
                    {
                        s.End.X = expected.X;
                        s.End.Y = expected.Y;
                        s.End.Z = expected.Z;
                    }, s => s.CalculateXYZ());
                }
                checkedCases++;
            }
            Assert.Greater(checkedCases, SweepCount / 2);
            Assert.LessOrEqual(worst, 1e-6, "worst position miss in metres");
        }

        /// <summary>
        /// The attitude combinations recover the curve through the two stations, whose end attitude is
        /// then the one asked for.
        /// </summary>
        [Test]
        public void AttitudeCombinationsRecoverTheEndAttitude()
        {
            Random random = new Random(Seed + 1);
            int checkedCases = 0;
            double worst = 0.0;
            for (int i = 0; i < SweepCount; i++)
            {
                var drawn = Reference(random, false);
                if (drawn == null)
                {
                    continue;
                }
                var reference = drawn.Value;
                TrajectoryPoint3D expected = reference.section.End;
                if (expected.Inclination == null || expected.Azimuth == null)
                {
                    continue;
                }
                double inclinationStart = (double)reference.section.Start.Inclination;
                double azimuthStart = (double)reference.section.Start.Azimuth;
                int caseIndex = i;

                void Check(string name, Action<CircularArcSection> setup, Func<CircularArcSection, bool> call)
                {
                    CircularArcSection section = Section(inclinationStart, azimuthStart);
                    setup(section);
                    if (!call(section) || !Produced(section.End))
                    {
                        return;
                    }
                    worst = System.Math.Max(worst, Difference(section.End.Inclination, expected.Inclination));
                    worst = System.Math.Max(worst,
                        System.Math.Abs(WrappedDifference(section.End.Azimuth, expected.Azimuth)));
                }

                Check("SIA", s =>
                {
                    s.End.Abscissa = expected.Abscissa;
                    s.End.Inclination = expected.Inclination;
                    s.End.Azimuth = expected.Azimuth;
                }, s => s.CalculateSIA());
                Check("LIA", s =>
                {
                    s.Circle.Length = reference.length;
                    s.End.Inclination = expected.Inclination;
                    s.End.Azimuth = expected.Azimuth;
                }, s => s.CalculateLIA());
                Check("DIA", s =>
                {
                    s.Circle.Curvature = reference.curvature;
                    s.End.Inclination = expected.Inclination;
                    s.End.Azimuth = expected.Azimuth;
                }, s => s.CalculateDIA());
                checkedCases++;
            }
            Assert.Greater(checkedCases, SweepCount / 2);
            Assert.LessOrEqual(worst, Tolerance, "worst attitude residual in radians");
        }
    }
}
