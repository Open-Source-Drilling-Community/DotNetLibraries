using OSDC.DotnetLibraries.General.Math;
using static OSDC.DotnetLibraries.Drilling.Section.UnitTest.SectionTestHelper;

namespace OSDC.DotnetLibraries.Drilling.Section.UnitTest
{
    public class ConstantCurvatureAndToolfaceArcSectionTests
    {
        private static ConstantCurvatureAndToolfaceArcSection Section(double inclination, double azimuth)
        {
            return new ConstantCurvatureAndToolfaceArcSection
            {
                Start = StartPoint(inclination, azimuth),
                End = new TrajectoryPoint3D()
            };
        }

        private static (ConstantCurvatureAndToolfaceArcSection section, double curvature, double toolface, double length)?
            Reference(Random random)
        {
            double inclination = 0.05 + (System.Math.PI - 0.10) * random.NextDouble();
            double azimuth = 2.0 * System.Math.PI * random.NextDouble();
            double curvature = 1e-5 + 6e-3 * random.NextDouble();
            double toolface = -System.Math.PI + 2.0 * System.Math.PI * random.NextDouble();
            double length = 10.0 + 400.0 * random.NextDouble();

            ConstantCurvatureAndToolfaceArcSection reference = Section(inclination, azimuth);
            reference.CTCCurve.Curvature = curvature;
            reference.CTCCurve.Toolface = toolface;
            reference.CTCCurve.Length = length;
            if (!reference.CalculateLDT() || !Produced(reference.End))
            {
                return null;
            }
            if (reference.End.Inclination == null || reference.End.Azimuth == null)
            {
                return null;
            }
            // A curve that builds all the way to the vertical stops turning there and carries straight
            // on, so its end station has no meaningful azimuth and its depth is reached over a whole
            // range of lengths. Going back from such a station is genuinely ill posed, and these tests
            // are about the combinations rather than about that degeneracy, so the draw is dropped.
            double sine = System.Math.Sin((double)reference.End.Inclination);
            if (System.Math.Abs(sine) < 1.0e-6)
            {
                return null;
            }
            return (reference, curvature, toolface, length);
        }

        /// <summary>
        /// Two stations and the distance between them determine a constant curvature and toolface curve
        /// outright, so the curve that comes back has to finish on the attitude it was given.
        /// </summary>
        [Test]
        public void CalculateSIARecoversTheEndAttitude()
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

                ConstantCurvatureAndToolfaceArcSection section =
                    Section((double)reference.section.Start.Inclination, (double)reference.section.Start.Azimuth);
                section.End.Abscissa = expected.Abscissa;
                section.End.Inclination = expected.Inclination;
                section.End.Azimuth = expected.Azimuth;

                Assert.IsTrue(section.CalculateSIA(), "declined at case " + i);
                Assert.IsTrue(Produced(section.End), "no end point at case " + i);
                worst = System.Math.Max(worst, Difference(section.End.Abscissa, expected.Abscissa));
                worst = System.Math.Max(worst, Difference(section.End.Inclination, expected.Inclination));
                worst = System.Math.Max(worst,
                    System.Math.Abs(WrappedDifference(section.End.Azimuth, expected.Azimuth)));
                checkedCases++;
            }
            Assert.Greater(checkedCases, SweepCount / 2);
            Assert.LessOrEqual(worst, Tolerance, "worst attitude residual");
        }

        /// <summary>
        /// The curvature and the end attitude give the toolface angle and the length in closed form: the
        /// change of inclination and the turn measured against the effective sine are the legs of the
        /// right angled triangle whose hypotenuse is the curvature times the length.
        /// </summary>
        [Test]
        public void CalculateDIARecoversTheEndAttitude()
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

                ConstantCurvatureAndToolfaceArcSection section =
                    Section((double)reference.section.Start.Inclination, (double)reference.section.Start.Azimuth);
                section.CTCCurve.Curvature = reference.curvature;
                section.End.Inclination = expected.Inclination;
                section.End.Azimuth = expected.Azimuth;

                Assert.IsTrue(section.CalculateDIA(), "declined at case " + i);
                Assert.IsTrue(Produced(section.End), "no end point at case " + i);
                worst = System.Math.Max(worst, Difference(section.End.Inclination, expected.Inclination));
                worst = System.Math.Max(worst,
                    System.Math.Abs(WrappedDifference(section.End.Azimuth, expected.Azimuth)));
                checkedCases++;
            }
            Assert.Greater(checkedCases, SweepCount / 2);
            Assert.LessOrEqual(worst, Tolerance, "worst attitude residual");
        }

        /// <summary>
        /// The curvature, the toolface angle and the end azimuth give the length in closed form through
        /// the substitution that makes the azimuth linear.
        ///
        /// The azimuth is carried wrapped, so the turn it implies is known only up to whole turns and
        /// the answer is not unique. Whichever branch is taken, the azimuth reached has to be the one
        /// asked for, up to whole turns. Where the branch that is asked for leads out of the range of
        /// inclination the method declines, which is why this does not require every case to succeed.
        /// </summary>
        [Test]
        public void CalculateDTAReachesTheRequestedAzimuth()
        {
            Random random = new Random(Seed);
            int checkedCases = 0;
            int solved = 0;
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
                checkedCases++;

                ConstantCurvatureAndToolfaceArcSection section =
                    Section((double)reference.section.Start.Inclination, (double)reference.section.Start.Azimuth);
                section.CTCCurve.Curvature = reference.curvature;
                section.CTCCurve.Toolface = reference.toolface;
                section.End.Azimuth = expected.Azimuth;

                if (!section.CalculateDTA() || !Produced(section.End))
                {
                    continue;
                }
                solved++;
                worst = System.Math.Max(worst,
                    System.Math.Abs(WrappedDifference(section.End.Azimuth, expected.Azimuth)));
            }
            Assert.Greater(checkedCases, SweepCount / 2);
            // The great majority are reached on the shortest branch.
            Assert.Greater(solved, (int)(0.9 * checkedCases));
            Assert.LessOrEqual(worst, Tolerance, "worst azimuth residual");
        }

        /// <summary>
        /// A branch is never selected silently. Where the shortest turn does not lead to a curve, asking
        /// for the branch that does has to produce one, and it has to reach the same azimuth.
        /// </summary>
        [Test]
        public void CalculateDTABranchesAreExplicit()
        {
            Random random = new Random(Seed);
            int recovered = 0;
            int declined = 0;
            for (int i = 0; i < SweepCount; i++)
            {
                var drawn = Reference(random);
                if (drawn == null)
                {
                    continue;
                }
                var reference = drawn.Value;
                TrajectoryPoint3D expected = reference.section.End;

                ConstantCurvatureAndToolfaceArcSection section =
                    Section((double)reference.section.Start.Inclination, (double)reference.section.Start.Azimuth);
                section.CTCCurve.Curvature = reference.curvature;
                section.CTCCurve.Toolface = reference.toolface;
                section.End.Azimuth = expected.Azimuth;
                if (section.CalculateDTA())
                {
                    continue;
                }
                declined++;

                for (int branch = -3; branch <= 3; branch++)
                {
                    if (branch == 0)
                    {
                        continue;
                    }
                    ConstantCurvatureAndToolfaceArcSection other =
                        Section((double)reference.section.Start.Inclination, (double)reference.section.Start.Azimuth);
                    other.CTCCurve.Curvature = reference.curvature;
                    other.CTCCurve.Toolface = reference.toolface;
                    other.End.Azimuth = expected.Azimuth;
                    if (other.CalculateDTA(branch) && Produced(other.End) &&
                        System.Math.Abs(WrappedDifference(other.End.Azimuth, expected.Azimuth)) <= Tolerance)
                    {
                        recovered++;
                        break;
                    }
                }
            }
            // Nearly every case the shortest branch turns down is reached on another one, which is what
            // makes those declines a matter of the branch rather than a defect.
            Assert.Greater(declined, 0, "the sweep should exercise the branch");
            Assert.Greater(recovered, (int)(0.9 * declined));
        }

        /// <summary>
        /// The inclination of a constant curvature and toolface curve is linear in the along hole
        /// distance at the rate curvature times the cosine of the toolface angle, so the end inclination
        /// gives the length at once.
        /// </summary>
        [Test]
        public void CalculateDTIReachesTheRequestedInclination()
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

                ConstantCurvatureAndToolfaceArcSection section =
                    Section((double)reference.section.Start.Inclination, (double)reference.section.Start.Azimuth);
                section.CTCCurve.Curvature = reference.curvature;
                section.CTCCurve.Toolface = reference.toolface;
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
        /// The vertical gained is the same function of the inclinations as for a constant build and turn
        /// curve, since the inclination is linear in both. Both roots are kept so that a curve reaching
        /// its depth past the horizontal is found.
        /// </summary>
        [Test]
        public void CalculateDTZReachesTheRequestedDepth()
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

                ConstantCurvatureAndToolfaceArcSection section =
                    Section((double)reference.section.Start.Inclination, (double)reference.section.Start.Azimuth);
                section.CTCCurve.Curvature = reference.curvature;
                section.CTCCurve.Toolface = reference.toolface;
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

                void Check(string name,
                           Action<ConstantCurvatureAndToolfaceArcSection> setup,
                           Func<ConstantCurvatureAndToolfaceArcSection, bool> call)
                {
                    ConstantCurvatureAndToolfaceArcSection section = Section(inclinationStart, azimuthStart);
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
                    s.CTCCurve.Curvature = reference.curvature;
                    s.CTCCurve.Toolface = reference.toolface;
                    s.End.Abscissa = expected.Abscissa;
                }, s => s.CalculateSDT());
                Check("LDT", s =>
                {
                    s.CTCCurve.Curvature = reference.curvature;
                    s.CTCCurve.Toolface = reference.toolface;
                    s.CTCCurve.Length = reference.length;
                }, s => s.CalculateLDT());
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
    }
}
