using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using static OSDC.DotnetLibraries.Drilling.Section.UnitTest.SectionTestHelper;

namespace OSDC.DotnetLibraries.Drilling.Section.UnitTest
{
    /// <summary>
    /// A path of sections worked out from a completely defined start and from three times as many
    /// quantities as there are sections, spread as the caller likes.
    ///
    /// The cases are built by running a path forward with known parameters and then handing back only some
    /// of its quantities. What is checked is that the path which comes back honours those quantities, not
    /// that it is the same path that generated them: the system being solved is square and nonlinear, so it
    /// may have more than one root and another root is just as correct an answer.
    /// </summary>
    public class ComplexPathTests
    {
        private static void ReadCurve(ArcSection section, out double length, out double first, out double second)
        {
            if (section is CircularArcSection arc)
            {
                length = (double)arc.Circle.Length;
                first = (double)arc.Circle.Curvature;
                second = (double)arc.Circle.ReferenceToolface;
                return;
            }
            if (section is BuildAndTurnArcSection turn)
            {
                length = (double)turn.BuildAndTurn.Length;
                first = (double)turn.BuildAndTurn.BUR;
                second = (double)turn.BuildAndTurn.TR;
                return;
            }
            ConstantCurvatureAndToolfaceArcSection toolface = (ConstantCurvatureAndToolfaceArcSection)section;
            length = (double)toolface.CTCCurve.Length;
            first = (double)toolface.CTCCurve.Curvature;
            second = (double)toolface.CTCCurve.Toolface;
        }

        private static ArcSection Forward(TrajectoryPoint3D from, SectionCurveType type,
                                          double length, double curvature, double toolface)
        {
            switch (type)
            {
                case SectionCurveType.CircularArc:
                    {
                        CircularArcSection arc = new CircularArcSection(from, new TrajectoryPoint3D());
                        arc.Circle.Curvature = curvature;
                        arc.Circle.ReferenceToolface = toolface;
                        arc.Circle.Length = length;
                        return arc.CalculateLDT() ? arc : null;
                    }
                case SectionCurveType.ConstantBuildAndTurn:
                    {
                        BuildAndTurnArcSection turn = new BuildAndTurnArcSection(from, new TrajectoryPoint3D());
                        turn.BuildAndTurn.BUR = curvature * System.Math.Cos(toolface);
                        turn.BuildAndTurn.TR = curvature * System.Math.Sin(toolface);
                        turn.BuildAndTurn.Length = length;
                        return turn.CalculateLBT() ? turn : null;
                    }
                default:
                    {
                        ConstantCurvatureAndToolfaceArcSection curve =
                            new ConstantCurvatureAndToolfaceArcSection(from, new TrajectoryPoint3D());
                        curve.CTCCurve.Curvature = curvature;
                        curve.CTCCurve.Toolface = toolface;
                        curve.CTCCurve.Length = length;
                        return curve.CalculateLDT() ? curve : null;
                    }
            }
        }

        /// <summary>
        /// A path run forward with known parameters, which is what the quantities are then taken from.
        /// </summary>
        private static (TrajectoryPoint3D start, List<ArcSection> path)? Reference(Random random,
                                                                                   SectionCurveType[] types)
        {
            TrajectoryPoint3D start = StartPoint(0.3 + 1.0 * random.NextDouble(),
                                                 2.0 * System.Math.PI * random.NextDouble());
            TrajectoryPoint3D current = new TrajectoryPoint3D();
            current.Set(start);
            List<ArcSection> path = new List<ArcSection>();
            foreach (SectionCurveType type in types)
            {
                ArcSection piece = Forward(current, type,
                                           150.0 + 200.0 * random.NextDouble(),
                                           3.0e-4 + 1.5e-3 * random.NextDouble(),
                                           -System.Math.PI + 2.0 * System.Math.PI * random.NextDouble());
                if (piece == null || !Produced(piece.End) || piece.End.Inclination == null)
                {
                    return null;
                }
                double reached = (double)piece.End.Inclination;
                if (reached <= 0.2 || reached >= System.Math.PI - 0.2)
                {
                    return null;
                }
                path.Add(piece);
                current = new TrajectoryPoint3D();
                current.Set(piece.End);
            }
            return (start, path);
        }

        /// <summary>
        /// Impose the first so many quantities of a section, in a fixed order, taken from the path that was
        /// run forward.
        /// </summary>
        private static void Impose(ComplexPathSection target, ArcSection source, int howMany)
        {
            ReadCurve(source, out double length, out double first, out double second);
            int given = 0;
            if (given++ < howMany) { target.End.X = source.End.X; }
            if (given++ < howMany) { target.End.Y = source.End.Y; }
            if (given++ < howMany) { target.End.Z = source.End.Z; }
            if (given++ < howMany) { target.End.Inclination = source.End.Inclination; }
            if (given++ < howMany) { target.End.Azimuth = source.End.Azimuth; }
            if (given++ < howMany) { target.Length = length; }
            if (given++ < howMany)
            {
                if (target.CurveType == SectionCurveType.ConstantBuildAndTurn) { target.BUR = first; }
                else { target.Curvature = first; }
            }
            if (given++ < howMany)
            {
                if (target.CurveType == SectionCurveType.ConstantBuildAndTurn) { target.TurnRate = second; }
                else { target.Toolface = second; }
            }
        }

        /// <summary>
        /// How far the path that came back is from the quantities that were imposed on it.
        /// </summary>
        private static void Check(ComplexPath path, List<ArcSection> reference, int[] howMany,
                                  ref double worstLength, ref double worstAngle)
        {
            for (int i = 0; i < reference.Count; i++)
            {
                ArcSection got = path.SolvedSections[i];
                ArcSection want = reference[i];
                ReadCurve(got, out double gotLength, out double gotFirst, out double gotSecond);
                ReadCurve(want, out double wantLength, out double wantFirst, out double wantSecond);
                int given = 0;
                if (given++ < howMany[i]) { worstLength = System.Math.Max(worstLength, System.Math.Abs((double)got.End.X - (double)want.End.X)); }
                if (given++ < howMany[i]) { worstLength = System.Math.Max(worstLength, System.Math.Abs((double)got.End.Y - (double)want.End.Y)); }
                if (given++ < howMany[i]) { worstLength = System.Math.Max(worstLength, System.Math.Abs((double)got.End.Z - (double)want.End.Z)); }
                if (given++ < howMany[i]) { worstAngle = System.Math.Max(worstAngle, System.Math.Abs((double)got.End.Inclination - (double)want.End.Inclination)); }
                if (given++ < howMany[i]) { worstAngle = System.Math.Max(worstAngle, System.Math.Abs(TrajectoryPoint3D.WrapToPi((double)got.End.Azimuth - (double)want.End.Azimuth))); }
                if (given++ < howMany[i]) { worstLength = System.Math.Max(worstLength, System.Math.Abs(gotLength - wantLength)); }
                if (given++ < howMany[i]) { worstAngle = System.Math.Max(worstAngle, System.Math.Abs(gotFirst - wantFirst) * 1000.0); }
                if (given++ < howMany[i]) { worstAngle = System.Math.Max(worstAngle, System.Math.Abs(TrajectoryPoint3D.WrapToPi(gotSecond - wantSecond))); }
            }
        }

        private static void Sweep(SectionCurveType[] types, int[] howMany, int cases,
                                  out int scenarios, out int solved,
                                  out double worstLength, out double worstAngle, out double worstJoin)
        {
            Random random = new Random(Seed);
            scenarios = 0;
            solved = 0;
            worstLength = 0.0;
            worstAngle = 0.0;
            worstJoin = 0.0;

            for (int k = 0; k < cases; k++)
            {
                var drawn = Reference(random, types);
                if (drawn == null)
                {
                    continue;
                }
                scenarios++;

                ComplexPath path = new ComplexPath { Start = new TrajectoryPoint3D() };
                path.Start.Set(drawn.Value.start);
                for (int i = 0; i < types.Length; i++)
                {
                    Impose(path.AddSection(types[i]), drawn.Value.path[i], howMany[i]);
                }
                if (!path.Calculate())
                {
                    continue;
                }
                solved++;
                Assert.AreEqual(types.Length, path.SolvedSections.Count, "one section out for each one in");
                Check(path, drawn.Value.path, howMany, ref worstLength, ref worstAngle);

                // The path has to join up: each section starts where the one before it ended.
                for (int i = 1; i < path.SolvedSections.Count; i++)
                {
                    TrajectoryPoint3D before = path.SolvedSections[i - 1].End;
                    TrajectoryPoint3D after = path.SolvedSections[i].Start;
                    double gap = System.Math.Sqrt(
                        System.Math.Pow((double)before.X - (double)after.X, 2) +
                        System.Math.Pow((double)before.Y - (double)after.Y, 2) +
                        System.Math.Pow((double)before.Z - (double)after.Z, 2));
                    worstJoin = System.Math.Max(worstJoin, gap);
                    worstJoin = System.Math.Max(worstJoin,
                        System.Math.Abs((double)before.Abscissa - (double)after.Abscissa));
                }
            }
        }

        private static readonly SectionCurveType Arc = SectionCurveType.CircularArc;
        private static readonly SectionCurveType Turn = SectionCurveType.ConstantBuildAndTurn;
        private static readonly SectionCurveType Toolface = SectionCurveType.ConstantCurvatureAndToolface;

        /// <summary>
        /// Every section carrying its own three quantities, which is what a section by section calculation
        /// already does. For a circular arc and a constant build and turn curve this goes through the closed
        /// form the section itself knows and is exact.
        /// </summary>
        [Test]
        public void EverySectionCarryingItsOwnThreeQuantities()
        {
            foreach (var types in new[] { new[] { Arc, Arc, Arc }, new[] { Turn, Turn, Turn } })
            {
                Sweep(types, new[] { 3, 3, 3 }, 150, out int scenarios, out int solved,
                      out double worstLength, out double worstAngle, out double worstJoin);
                Assert.Greater(scenarios, 50);
                Assert.AreEqual(scenarios, solved, "all of these should be solvable");
                Assert.LessOrEqual(worstLength, 1.0e-6, "imposed lengths and positions");
                Assert.LessOrEqual(worstAngle, 1.0e-6, "imposed angles");
                Assert.LessOrEqual(worstJoin, 1.0e-9, "the sections should join up");
            }
        }

        /// <summary>
        /// The same for a constant curvature and toolface curve, which is worked out by the general solve.
        /// </summary>
        [Test]
        public void EverySectionCarryingItsOwnThreeQuantitiesWithAConstantToolface()
        {
            Sweep(new[] { Toolface, Toolface, Toolface }, new[] { 3, 3, 3 }, 40,
                  out int scenarios, out int solved, out double worstLength, out double worstAngle,
                  out double worstJoin);
            Assert.Greater(scenarios, 15);
            Assert.AreEqual(scenarios, solved);
            Assert.LessOrEqual(worstLength, 1.0e-4, "imposed lengths and positions");
            Assert.LessOrEqual(worstAngle, 1.0e-6, "imposed angles");
            Assert.LessOrEqual(worstJoin, 1.0e-9, "the sections should join up");
        }

        /// <summary>
        /// Quantities spread unevenly, which is the whole point: a surplus on a later section reaches back
        /// and settles the ones before it. This is what a section by section calculation cannot do.
        /// </summary>
        [TestCase(1, 5)]
        [TestCase(2, 4)]
        [TestCase(4, 2)]
        [TestCase(5, 1)]
        public void QuantitiesSpreadUnevenlyOverTwoSections(int onFirst, int onSecond)
        {
            Sweep(new[] { Arc, Arc }, new[] { onFirst, onSecond }, 150,
                  out int scenarios, out int solved, out double worstLength, out double worstAngle,
                  out double worstJoin);
            Assert.Greater(scenarios, 50);
            if (onFirst > 3)
            {
                // A surplus on the first section cannot be answered by the one after it, so these are
                // turned down rather than solved.
                Assert.AreEqual(0, solved, "an over-determined first section should be refused");
                return;
            }
            Assert.AreEqual(scenarios, solved, "all of these should be solvable");
            Assert.LessOrEqual(worstLength, 1.0e-4, "imposed lengths and positions");
            Assert.LessOrEqual(worstAngle, 1.0e-6, "imposed angles");
            Assert.LessOrEqual(worstJoin, 1.0e-9, "the sections should join up");
        }

        /// <summary>
        /// The same over three sections, and with the three curves mixed along one path.
        /// </summary>
        [Test]
        public void QuantitiesSpreadUnevenlyOverThreeSections()
        {
            Sweep(new[] { Arc, Arc, Arc }, new[] { 1, 3, 5 }, 150, out int scenarios, out int solved,
                  out double worstLength, out double worstAngle, out double worstJoin);
            Assert.Greater(scenarios, 50);
            Assert.AreEqual(scenarios, solved);
            Assert.LessOrEqual(worstLength, 1.0e-4);
            Assert.LessOrEqual(worstAngle, 1.0e-6);
            Assert.LessOrEqual(worstJoin, 1.0e-9);

            Sweep(new[] { Arc, Arc, Arc }, new[] { 1, 1, 7 }, 150, out scenarios, out solved,
                  out worstLength, out worstAngle, out worstJoin);
            Assert.Greater(scenarios, 50);
            Assert.AreEqual(scenarios, solved);
            Assert.LessOrEqual(worstLength, 1.0e-4);
            Assert.LessOrEqual(worstAngle, 1.0e-6);
        }

        [Test]
        public void TheThreeCurvesMayBeMixedAlongOnePath()
        {
            Sweep(new[] { Arc, Turn }, new[] { 1, 5 }, 150, out int scenarios, out int solved,
                  out double worstLength, out double worstAngle, out double worstJoin);
            Assert.Greater(scenarios, 50);
            Assert.AreEqual(scenarios, solved);
            Assert.LessOrEqual(worstLength, 1.0e-4);
            Assert.LessOrEqual(worstAngle, 1.0e-6);

            Sweep(new[] { Arc, Toolface }, new[] { 2, 4 }, 30, out scenarios, out solved,
                  out worstLength, out worstAngle, out worstJoin);
            Assert.Greater(scenarios, 10);
            Assert.AreEqual(scenarios, solved);
            Assert.LessOrEqual(worstLength, 1.0e-4);
            Assert.LessOrEqual(worstAngle, 1.0e-6);
        }

        /// <summary>
        /// A path divides into the shortest runs of sections that settle on their own, and those are worked
        /// out one after another. A path of counts three and three is two runs of one; one and five is a
        /// single run of two.
        /// </summary>
        [Test]
        public void ThePathDividesIntoRunsThatSettleOnTheirOwn()
        {
            Sweep(new[] { Arc, Arc }, new[] { 3, 3 }, 60, out int scenarios, out int solved,
                  out double worstLength, out double worstAngle, out double worstJoin);
            Assert.Greater(scenarios, 20);
            Assert.AreEqual(scenarios, solved, "two runs of one section each");
            Assert.LessOrEqual(worstLength, 1.0e-6, "each run settled on its own should be exact");
        }

        // ------------------------------------------------------------------------------------------
        // What the rules refuse
        // ------------------------------------------------------------------------------------------

        private static ComplexPath Path()
        {
            ComplexPath path = new ComplexPath { Start = new TrajectoryPoint3D() };
            path.Start.Set(StartPoint(0.6, 1.0));
            return path;
        }

        /// <summary>
        /// A surplus reaches back to the sections before it and a shortfall forward to those after it, so a
        /// run counted from the start may fall short of three times its length but never exceed it.
        /// </summary>
        [Test]
        public void AnOverDeterminedRunIsRefused()
        {
            ComplexPath path = Path();
            ComplexPathSection first = path.AddSection(Arc);
            first.End.X = 100.0;
            first.End.Y = 50.0;
            first.End.Z = 700.0;
            first.Length = 300.0;                       // four on the first section
            ComplexPathSection second = path.AddSection(Arc);
            second.Length = 200.0;                      // one
            ComplexPathSection third = path.AddSection(Arc);
            third.End.X = 400.0;
            third.End.Y = 200.0;
            third.End.Z = 900.0;
            third.Length = 250.0;                       // four, so twelve less three is nine in total

            Assert.IsFalse(path.Calculate());
            Assert.AreEqual(ComplexPathFailureReason.OverDeterminedSections, path.FailureReason);
            Assert.AreEqual(0, path.FailedSectionIndex, "the first section is already over-determined");
            Assert.IsNotEmpty(path.FailureDescription);
        }

        [Test]
        public void ASectionImposingNothingIsRefused()
        {
            ComplexPath path = Path();
            path.AddSection(Arc);
            ComplexPathSection second = path.AddSection(Arc);
            second.End.X = 400.0;
            second.End.Y = 200.0;
            second.End.Z = 900.0;
            second.End.Inclination = 1.0;
            second.End.Azimuth = 1.2;
            second.Length = 300.0;

            Assert.IsFalse(path.Calculate());
            Assert.AreEqual(ComplexPathFailureReason.SectionWithoutParameters, path.FailureReason);
            Assert.AreEqual(0, path.FailedSectionIndex);
        }

        [Test]
        public void TheWrongNumberOfQuantitiesIsRefused()
        {
            ComplexPath path = Path();
            path.AddSection(Arc).Length = 300.0;
            path.AddSection(Arc).Length = 200.0;
            Assert.IsFalse(path.Calculate());
            Assert.AreEqual(ComplexPathFailureReason.WrongNumberOfParameters, path.FailureReason);
        }

        [Test]
        public void AParameterOfAnotherCurveIsRefused()
        {
            ComplexPath path = Path();
            ComplexPathSection section = path.AddSection(Arc);
            section.Length = 300.0;
            section.BUR = 0.001;
            section.TurnRate = 0.001;
            Assert.IsFalse(path.Calculate());
            Assert.AreEqual(ComplexPathFailureReason.ParameterOfAnotherCurve, path.FailureReason);
            Assert.AreEqual(0, path.FailedSectionIndex);
        }

        [Test]
        public void AStartThatIsNotCompletelyDefinedIsRefused()
        {
            ComplexPath path = new ComplexPath();
            path.Start.Set(0.0, 0.0, 500.0);            // no attitude
            path.AddSection(Arc).Length = 300.0;
            Assert.IsFalse(path.Calculate());
            Assert.AreEqual(ComplexPathFailureReason.UndefinedStart, path.FailureReason);
        }

        [Test]
        public void APathWithNoSectionsIsEmpty()
        {
            ComplexPath path = Path();
            Assert.IsTrue(path.Calculate());
            Assert.AreEqual(0, path.SolvedSections.Count);
            Assert.AreEqual(ComplexPathFailureReason.None, path.FailureReason);
        }
    }
}
