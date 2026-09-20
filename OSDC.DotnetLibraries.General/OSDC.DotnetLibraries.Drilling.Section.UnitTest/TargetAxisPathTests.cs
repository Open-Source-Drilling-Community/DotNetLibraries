using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using static OSDC.DotnetLibraries.Drilling.Section.UnitTest.SectionTestHelper;

namespace OSDC.DotnetLibraries.Drilling.Section.UnitTest
{
    /// <summary>
    /// A trajectory from a known station through a list of targets. A target that gives only a position
    /// is passed through and leaves the direction to the section that arrives there; a target that gives
    /// a direction as well has to be met heading that way, which takes a pair of sections.
    /// </summary>
    public class TargetAxisPathTests
    {
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
        /// Walk a well forward with circular arcs and take the stations it passes through as targets,
        /// keeping the direction on every other one. That gives a list of targets a trajectory certainly
        /// runs through, whichever curve it is later drawn with.
        /// </summary>
        private static (TrajectoryPoint3D start, List<TargetAxis> targets)? Scenario(Random random, int count)
        {
            double inclination = 0.25 + 1.2 * random.NextDouble();
            double azimuth = 2.0 * System.Math.PI * random.NextDouble();
            TrajectoryPoint3D start = StartPoint(inclination, azimuth);
            TrajectoryPoint3D current = new TrajectoryPoint3D();
            current.Set(start);

            List<TargetAxis> targets = new List<TargetAxis>();
            for (int n = 0; n < count; n++)
            {
                CircularArcSection arc = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
                arc.Start.Set(current);
                arc.Circle.Curvature = 2.0e-4 + 2.5e-3 * random.NextDouble();
                arc.Circle.ReferenceToolface = -System.Math.PI + 2.0 * System.Math.PI * random.NextDouble();
                arc.Circle.Length = 120.0 + 260.0 * random.NextDouble();
                if (!arc.CalculateLDT() || !Produced(arc.End) || arc.End.Inclination == null)
                {
                    return null;
                }
                double reached = (double)arc.End.Inclination;
                if (reached <= 0.2 || reached >= System.Math.PI - 0.2)
                {
                    return null;
                }
                TargetAxis target = new TargetAxis();
                target.Station.Set((double)arc.End.X, (double)arc.End.Y, (double)arc.End.Z);
                if (n % 2 == 0)
                {
                    target.Station.Inclination = arc.End.Inclination;
                    target.Station.Azimuth = arc.End.Azimuth;
                }
                targets.Add(target);
                current.Set(arc.End);
            }
            return (start, targets);
        }

        private static TargetAxisPath Path(TrajectoryPoint3D start, List<TargetAxis> targets, SectionCurveType type)
        {
            TargetAxisPath path = new TargetAxisPath { CurveType = type, Start = new TrajectoryPoint3D() };
            path.Start.Set(start);
            path.Targets.AddRange(targets);
            return path;
        }

        /// <summary>
        /// Every target has to be reached, in position and, where one is imposed, in direction too. The
        /// sections also have to join up: they share the stations where they meet, so the trajectory is
        /// continuous exactly rather than to within a tolerance.
        /// </summary>
        [TestCase(SectionCurveType.CircularArc)]
        [TestCase(SectionCurveType.ConstantBuildAndTurn)]
        [TestCase(SectionCurveType.ConstantCurvatureAndToolface)]
        public void EveryTargetIsReached(SectionCurveType type)
        {
            Random random = new Random(Seed);
            int scenarios = 0;
            int solved = 0;
            double worstPosition = 0.0;
            double worstAxis = 0.0;
            List<double> axisMisses = new List<double>();

            int wanted = type == SectionCurveType.ConstantCurvatureAndToolface ? 40 : 200;
            for (int k = 0; k < wanted; k++)
            {
                var drawn = Scenario(random, 4);
                if (drawn == null)
                {
                    continue;
                }
                scenarios++;
                TargetAxisPath path = Path(drawn.Value.start, drawn.Value.targets, type);
                if (!path.Calculate())
                {
                    // Turning a target down is allowed, but it has to say which one.
                    Assert.GreaterOrEqual(path.FailedTargetIndex, 0, "a failure should name the target");
                    Assert.Less(path.FailedTargetIndex, drawn.Value.targets.Count);
                    continue;
                }
                solved++;

                // The sections are in order, so the end of the section reaching target n is where it is.
                int at = 0;
                foreach (TargetAxis target in drawn.Value.targets)
                {
                    double best = double.PositiveInfinity;
                    TrajectoryPoint3D arrival = null;
                    foreach (ArcSection section in path.Sections)
                    {
                        double d = Distance(section.End, target.Station);
                        if (d < best)
                        {
                            best = d;
                            arrival = section.End;
                        }
                    }
                    worstPosition = System.Math.Max(worstPosition, best);
                    if (target.HasAxis && arrival != null)
                    {
                        double miss = AttitudeMiss(arrival, target.Station);
                        worstAxis = System.Math.Max(worstAxis, miss);
                        axisMisses.Add(miss);
                    }
                    at++;
                }

                // A section keeps its own stations, so the join is two objects carrying equal values.
                for (int i = 1; i < path.Sections.Count; i++)
                {
                    TrajectoryPoint3D before = path.Sections[i - 1].End;
                    TrajectoryPoint3D after = path.Sections[i].Start;
                    Assert.AreEqual(0.0, Distance(before, after), 1.0e-9, "the sections should join at case " + k);
                    Assert.AreEqual(0.0, AttitudeMiss(before, after), 1.0e-9, "the attitudes should join at case " + k);
                    Assert.AreEqual((double)before.Abscissa, (double)after.Abscissa, 1.0e-9,
                                    "the measured depths should join at case " + k);
                }
            }

            axisMisses.Sort();
            double typicalAxis = axisMisses.Count > 0 ? axisMisses[(int)(0.9 * axisMisses.Count)] : 0.0;

            Assert.Greater(scenarios, wanted / 2);
            Assert.Greater(solved, (int)(0.8 * scenarios), "most of these should be solvable");
            Assert.LessOrEqual(worstPosition, 1.0e-6, "worst target miss in metres");
            // The direction is held to what the constructions promise. Nearly every leg does far better
            // than this; the allowance is for the legs where two stations are so nearly joined by one
            // curve that a pair has no junction to settle on, and the pair construction falls back on that
            // single curve. Most of the legs here should still be at the accuracy of the arithmetic.
            Assert.LessOrEqual(worstAxis, 1.0e-4, "worst axis miss in radians");
            Assert.LessOrEqual(typicalAxis, 1.0e-9, "the axis is usually met to the accuracy of the arithmetic");
        }

        /// <summary>
        /// Where one curve already meets a target that imposes a direction, only that curve is reported.
        /// The earlier version of this calculation always emitted two sections for such a target, the
        /// second of them of no length, which is a curve with nothing in it for anything walking the
        /// trajectory to trip over.
        /// </summary>
        [Test]
        public void ASingleCurveIsNotReportedAsTwo()
        {
            Random random = new Random(Seed + 1);
            int checkedCases = 0;
            for (int i = 0; i < 100; i++)
            {
                double inclination = 0.3 + 1.0 * random.NextDouble();
                double azimuth = 2.0 * System.Math.PI * random.NextDouble();
                TrajectoryPoint3D start = StartPoint(inclination, azimuth);

                // One arc, so its end station is reachable from the start by that single arc.
                CircularArcSection arc = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
                arc.Start.Set(start);
                arc.Circle.Curvature = 5.0e-4 + 2.0e-3 * random.NextDouble();
                arc.Circle.ReferenceToolface = -System.Math.PI + 2.0 * System.Math.PI * random.NextDouble();
                arc.Circle.Length = 150.0 + 200.0 * random.NextDouble();
                if (!arc.CalculateLDT() || !Produced(arc.End) || arc.End.Inclination == null)
                {
                    continue;
                }
                double reached = (double)arc.End.Inclination;
                if (reached <= 0.2 || reached >= System.Math.PI - 0.2)
                {
                    continue;
                }

                TargetAxisPath path = new TargetAxisPath { Start = new TrajectoryPoint3D() };
                path.Start.Set(start);
                path.AddTarget((double)arc.End.X, (double)arc.End.Y, (double)arc.End.Z,
                               (double)arc.End.Inclination, (double)arc.End.Azimuth);
                Assert.IsTrue(path.Calculate(), "case " + i);
                checkedCases++;
                Assert.AreEqual(1, path.Sections.Count, "one arc should be reported as one section at case " + i);
            }
            Assert.Greater(checkedCases, 50);
        }

        /// <summary>
        /// A target that imposes a direction and is not reachable by one curve takes two sections.
        /// </summary>
        [Test]
        public void AnAxisTargetGenerallyTakesTwoSections()
        {
            TrajectoryPoint3D start = StartPoint(0.5, 0.3);
            TargetAxisPath path = new TargetAxisPath { Start = new TrajectoryPoint3D() };
            path.Start.Set(start);
            // Somewhere ahead, but asked to be met heading a long way round from the natural arc.
            path.AddTarget(200.0, 120.0, 900.0, 1.2, 2.6);
            Assert.IsTrue(path.Calculate());
            Assert.AreEqual(2, path.Sections.Count);
            Assert.AreEqual(1.2, (double)path.Sections[1].End.Inclination, 1.0e-6);
        }

        /// <summary>
        /// A path may draw each leg with a different curve.
        /// </summary>
        [Test]
        public void TheCurveMayBeChosenPerTarget()
        {
            Random random = new Random(Seed + 2);
            int checkedCases = 0;
            for (int i = 0; i < 60; i++)
            {
                var drawn = Scenario(random, 3);
                if (drawn == null)
                {
                    continue;
                }
                drawn.Value.targets[0].CurveType = SectionCurveType.CircularArc;
                drawn.Value.targets[1].CurveType = SectionCurveType.ConstantBuildAndTurn;
                drawn.Value.targets[2].CurveType = SectionCurveType.ConstantCurvatureAndToolface;

                TargetAxisPath path = Path(drawn.Value.start, drawn.Value.targets, SectionCurveType.CircularArc);
                if (!path.Calculate())
                {
                    continue;
                }
                checkedCases++;
                // The first target imposes a direction and the walk was made of arcs, so one arc meets it.
                Assert.IsInstanceOf<CircularArcSection>(path.Sections[0], "first leg at case " + i);
                Assert.IsTrue(path.Sections.Exists(s => s is BuildAndTurnArcSection), "no build and turn leg at case " + i);
                Assert.IsTrue(path.Sections.Exists(s => s is ConstantCurvatureAndToolfaceArcSection),
                              "no constant curvature and toolface leg at case " + i);
            }
            Assert.Greater(checkedCases, 20);
        }

        /// <summary>
        /// A target that cannot be reached names itself, and the legs worked out before it are kept so
        /// that a caller can see how far the trajectory got.
        /// </summary>
        [Test]
        public void AnUnreachableTargetIsNamed()
        {
            TrajectoryPoint3D start = StartPoint(0.6, 1.0);
            TargetAxisPath path = new TargetAxisPath { Start = new TrajectoryPoint3D() };
            path.Start.Set(start);
            path.AddTarget(100.0, 60.0, 700.0);
            // The second target sits back behind the first, which no single curve leaving it can reach:
            // getting there would mean turning through more than a half circle.
            path.AddTarget(0.0, 0.0, 500.0);

            Assert.IsFalse(path.Calculate());
            Assert.AreEqual(1, path.FailedTargetIndex, "the second target is the impossible one");
            Assert.AreEqual(1, path.Sections.Count, "the leg that did work should be kept");
            Assert.AreEqual(TargetAxisFailureReason.CurveDeclined, path.FailureReason,
                            "no curve reaches it, rather than one that misses");
            Assert.AreEqual(SectionCurveType.CircularArc, path.FailedCurveType);
            Assert.IsNotNull(path.FailedFrom, "the station the leg set off from should be reported");
            // What it set off from is where the first target is, so the question can be put again alone.
            Assert.AreEqual(100.0, (double)path.FailedFrom.X, 1.0e-6);
            Assert.AreEqual(60.0, (double)path.FailedFrom.Y, 1.0e-6);
            Assert.AreEqual(700.0, (double)path.FailedFrom.Z, 1.0e-6);
            Assert.IsNotEmpty(path.FailureDescription);
        }

        /// <summary>
        /// When the start carries no attitude one is chosen: the direction the first target asks to be met
        /// with if it asks for one, and otherwise straight at it.
        /// </summary>
        [Test]
        public void TheStartAttitudeIsWorkedOutWhenItIsMissing()
        {
            // First target imposes a direction, so the trajectory sets off that way.
            TargetAxisPath withAxis = new TargetAxisPath();
            withAxis.Start.Set(0.0, 0.0, 500.0);
            withAxis.AddTarget(300.0, 0.0, 800.0, 0.9, 0.0);
            Assert.IsTrue(withAxis.Calculate());
            Assert.AreEqual(0.9, (double)withAxis.Sections[0].Start.Inclination, 1.0e-9);
            Assert.AreEqual(0.0, (double)withAxis.Sections[0].Start.Azimuth, 1.0e-9);
            Assert.AreEqual(0.0, (double)withAxis.Sections[0].Start.Abscissa, 1.0e-9);

            // First target imposes only a position, so the trajectory heads straight at it. That target
            // lies due north and below, so the inclination is the angle off the vertical of the line to it.
            TargetAxisPath withPoint = new TargetAxisPath();
            withPoint.Start.Set(0.0, 0.0, 500.0);
            withPoint.AddTarget(300.0, 0.0, 800.0);
            Assert.IsTrue(withPoint.Calculate());
            Assert.AreEqual(System.Math.Atan2(300.0, 300.0), (double)withPoint.Sections[0].Start.Inclination, 1.0e-9);
            Assert.AreEqual(0.0, (double)withPoint.Sections[0].Start.Azimuth, 1.0e-9);
        }

        /// <summary>
        /// Interpolating along the whole trajectory follows it: the ends come back, and no two consecutive
        /// samples are further apart than the along hole distance between them. The sections snap a sample
        /// to one of their own ends when it falls within the depth accuracy of it, so a sample landing on a
        /// junction can sit that far from where its depth asks, and the allowance covers it.
        /// </summary>
        [Test]
        public void InterpolationFollowsTheTrajectory()
        {
            Random random = new Random(Seed + 3);
            int checkedCases = 0;
            double worstEnds = 0.0;
            double worstJump = 0.0;

            for (int i = 0; i < 60; i++)
            {
                var drawn = Scenario(random, 3);
                if (drawn == null)
                {
                    continue;
                }
                TargetAxisPath path = Path(drawn.Value.start, drawn.Value.targets, SectionCurveType.CircularArc);
                if (!path.Calculate())
                {
                    continue;
                }
                checkedCases++;

                double from = (double)path.Sections[0].Start.Abscissa;
                double to = (double)path.Sections[path.Sections.Count - 1].End.Abscissa;
                Assert.AreEqual(to - from, (double)path.Length, 1.0e-9, "the length of the trajectory");

                CurvilinearPoint3D atStart = path.InterpolateAtMD(from);
                CurvilinearPoint3D atEnd = path.InterpolateAtMD(to);
                Assert.IsNotNull(atStart);
                Assert.IsNotNull(atEnd);
                worstEnds = System.Math.Max(worstEnds, Distance((TrajectoryPoint3D)atStart, path.Sections[0].Start));
                worstEnds = System.Math.Max(worstEnds,
                    Distance((TrajectoryPoint3D)atEnd, path.Sections[path.Sections.Count - 1].End));

                const int Samples = 60;
                CurvilinearPoint3D previous = atStart;
                for (int n = 1; n <= Samples; n++)
                {
                    CurvilinearPoint3D sample = path.InterpolateAtMD(from + (to - from) * n / Samples);
                    Assert.IsNotNull(sample, "sample " + n + " at case " + i);
                    Assert.IsTrue(Numeric.IsDefined(sample.X), "sample " + n + " at case " + i);
                    worstJump = System.Math.Max(worstJump,
                        Distance((TrajectoryPoint3D)sample, (TrajectoryPoint3D)previous) - (to - from) / Samples);
                    previous = sample;
                }
            }

            Assert.Greater(checkedCases, 20);
            Assert.LessOrEqual(worstEnds, 1.0e-6, "worst end point miss in metres");
            Assert.LessOrEqual(worstJump, Numeric.DEPTH_ACCURACY, "worst jump between consecutive samples");
        }

        /// <summary>
        /// A target a pair of constant curvature and toolface curves cannot meet is turned down, and says
        /// so: the construction found nothing, rather than finding something that misses. Such a curve
        /// stops turning at the vertical, so there are attitudes it genuinely cannot arrive at, and the
        /// answer is to move the target or to draw that leg with another kind of curve. The same targets
        /// drawn with circular arcs are checked here to show that is what it comes down to.
        /// </summary>
        [Test]
        public void ACurveThatCannotBeDrawnSaysSo()
        {
            Random random = new Random(Seed + 7);
            int declined = 0;
            int rescuedByAnotherCurve = 0;
            for (int i = 0; i < 400 && declined < 5; i++)
            {
                var drawn = Scenario(random, 3);
                if (drawn == null)
                {
                    continue;
                }
                TargetAxisPath path = Path(drawn.Value.start, drawn.Value.targets,
                                           SectionCurveType.ConstantCurvatureAndToolface);
                if (path.Calculate())
                {
                    continue;
                }
                declined++;

                // Every failure has to name the target, the curve and the station it set off from.
                Assert.AreEqual(TargetAxisFailureReason.CurveDeclined, path.FailureReason,
                                "a construction that finds nothing is not the same as one that misses");
                Assert.GreaterOrEqual(path.FailedTargetIndex, 0);
                Assert.Less(path.FailedTargetIndex, drawn.Value.targets.Count);
                Assert.AreEqual(SectionCurveType.ConstantCurvatureAndToolface, path.FailedCurveType);
                Assert.IsNotNull(path.FailedFrom);
                Assert.IsTrue(Produced(path.FailedFrom), "the station it set off from should be usable");
                Assert.IsTrue(path.FailureDescription.Contains("constant curvature and toolface"),
                              "the description should name the curve: " + path.FailureDescription);

                // The trajectory got as far as the sections it kept.
                Assert.LessOrEqual(path.Sections.Count, 2 * path.FailedTargetIndex);

                TargetAxisPath asArcs = Path(drawn.Value.start, drawn.Value.targets, SectionCurveType.CircularArc);
                if (asArcs.Calculate())
                {
                    rescuedByAnotherCurve++;
                }
            }
            Assert.Greater(declined, 0, "the sweep should turn some of these down");
            Assert.Greater(rescuedByAnotherCurve, 0,
                           "at least some of what one curve cannot draw, another can");
        }

        /// <summary>
        /// Each way of failing reports its own reason.
        /// </summary>
        [Test]
        public void EachKindOfFailureIsTold()
        {
            TargetAxisPath noStart = new TargetAxisPath();
            noStart.AddTarget(100.0, 0.0, 600.0);
            Assert.IsFalse(noStart.Calculate());
            Assert.AreEqual(TargetAxisFailureReason.UndefinedStart, noStart.FailureReason);

            TargetAxisPath noTarget = new TargetAxisPath();
            noTarget.Start.Set(0.0, 0.0, 500.0);
            noTarget.Start.Inclination = 0.4;
            noTarget.Start.Azimuth = 1.1;
            noTarget.AddTarget(100.0, 0.0, 600.0);
            noTarget.Targets.Add(new TargetAxis());
            Assert.IsFalse(noTarget.Calculate());
            Assert.AreEqual(TargetAxisFailureReason.UndefinedTarget, noTarget.FailureReason);
            Assert.AreEqual(1, noTarget.FailedTargetIndex);

            // No attitude on the start and the first target sitting on it: nothing says which way to go.
            TargetAxisPath noDirection = new TargetAxisPath();
            noDirection.Start.Set(0.0, 0.0, 500.0);
            noDirection.AddTarget(0.0, 0.0, 500.0);
            Assert.IsFalse(noDirection.Calculate());
            Assert.AreEqual(TargetAxisFailureReason.UndefinedStartDirection, noDirection.FailureReason);

            // And nothing that succeeds reports a reason.
            TargetAxisPath fine = new TargetAxisPath();
            fine.Start.Set(0.0, 0.0, 500.0);
            fine.Start.Inclination = 0.4;
            fine.Start.Azimuth = 1.1;
            fine.AddTarget(100.0, 200.0, 800.0);
            Assert.IsTrue(fine.Calculate());
            Assert.AreEqual(TargetAxisFailureReason.None, fine.FailureReason);
            Assert.AreEqual(-1, fine.FailedTargetIndex);
            Assert.IsNull(fine.FailedCurveType);
            Assert.IsNull(fine.FailedFrom);
        }

        /// <summary>
        /// A path with no targets is a trajectory of nothing, not a failure.
        /// </summary>
        [Test]
        public void APathWithNoTargetsIsEmpty()
        {
            TargetAxisPath path = new TargetAxisPath();
            path.Start.Set(0.0, 0.0, 500.0);
            path.Start.Inclination = 0.4;
            path.Start.Azimuth = 1.1;
            Assert.IsTrue(path.Calculate());
            Assert.AreEqual(0, path.Sections.Count);
            Assert.IsNull(path.Length);
        }

        /// <summary>
        /// A target that does not even say where it is cannot be worked with.
        /// </summary>
        [Test]
        public void ATargetWithoutAPositionIsRefused()
        {
            TargetAxisPath path = new TargetAxisPath();
            path.Start.Set(0.0, 0.0, 500.0);
            path.Start.Inclination = 0.4;
            path.Start.Azimuth = 1.1;
            path.Targets.Add(new TargetAxis());
            Assert.IsFalse(path.Calculate());
        }
    }
}
