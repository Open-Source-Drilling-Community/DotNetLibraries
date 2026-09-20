using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// The direction a planned well leaves in and the direction it arrives by.
    /// <para>
    /// The geometry is deliberately bare: no obstacles at all, a slot at the origin leaving downward, and
    /// a square target standing in a plane of constant north so that its normal is horizontal. What is
    /// being measured is the constraint itself, and an empty medium is where a failure of it has nowhere
    /// to hide.
    /// </para>
    /// </summary>
    [TestFixture]
    public class TargetApproachTests
    {
        private const double TargetNorth = 120.0;
        private const double TargetVertical = 200.0;

        /// <summary>
        /// a square standing in the plane of constant north, so that the normal of the polygon is
        /// horizontal and pointing north
        /// </summary>
        private static TargetPolygon StandingTarget(double half = 40.0, double thickness = 40.0)
        {
            List<Point3D> vertices = new List<Point3D>
            {
                new Point3D(TargetNorth, -half, TargetVertical - half),
                new Point3D(TargetNorth, half, TargetVertical - half),
                new Point3D(TargetNorth, half, TargetVertical + half),
                new Point3D(TargetNorth, -half, TargetVertical + half)
            };
            return new TargetPolygon(vertices, thickness);
        }

        private static StreamlineSource Slot(double north = 0)
        {
            return new StreamlineSource(new Point3D(north, 0, 0), new Vector3D(0, 0, 1));
        }

        private static StreamlineGeneratorOptions Settings()
        {
            return new StreamlineGeneratorOptions
            {
                Grid = new StreamlineGridOptions { FinestCellSize = 2.0, CoarsestCellSize = 16.0 },
                StreamlineCount = 16,
                ConduitLength = 20.0
            };
        }

        /// <summary>
        /// the unit tangent of the last stretch a streamline travelled before it arrived
        /// </summary>
        private static void GetArrivalTangent(Streamline line, out double north, out double east,
                                              out double vertical)
        {
            List<Point3D> positions = line.Positions!;
            Point3D last = positions[positions.Count - 1];
            Point3D before = positions[positions.Count - 2];
            north = last.X!.Value - before.X!.Value;
            east = last.Y!.Value - before.Y!.Value;
            vertical = last.Z!.Value - before.Z!.Value;
            double length = System.Math.Sqrt(north * north + east * east + vertical * vertical);
            if (length > 0)
            {
                north /= length;
                east /= length;
                vertical /= length;
            }
        }

        [Test]
        public void TheNormalOfAStandingTargetIsHorizontal()
        {
            TargetPolygon target = StandingTarget();
            Vector3D normal = target.GetNormal();
            Assert.That(System.Math.Abs(normal.Z!.Value), Is.LessThan(1.0e-12),
                        "a square in a plane of constant north cannot have a vertical normal");
            Assert.That(System.Math.Abs(System.Math.Abs(normal.X!.Value) - 1.0), Is.LessThan(1.0e-12));

            // the approach direction only picks the sign, and it may be given loosely
            target.ApproachDirection = new Vector3D(1, 0.3, 0.2);
            target.GetArrivalUnit(out double n, out double e, out double v);
            Assert.That(n, Is.EqualTo(1.0).Within(1.0e-12));
            Assert.That(e, Is.EqualTo(0.0).Within(1.0e-12));
            Assert.That(v, Is.EqualTo(0.0).Within(1.0e-12));

            target.ApproachDirection = new Vector3D(-1, 0.3, 0.2);
            target.GetArrivalUnit(out n, out _, out _);
            Assert.That(n, Is.EqualTo(-1.0).Within(1.0e-12), "the other side of the same plane");
        }

        [Test]
        public void OneFaceNeedsAnApproachDirectionThatNamesIt()
        {
            TargetPolygon target = StandingTarget();
            target.Sides = TargetSides.One;
            Assert.That(target.IsValid(out string? reason), Is.False);
            Assert.That(reason, Does.Contain("ApproachDirection"));

            // a direction lying in the plane names no face
            target.ApproachDirection = new Vector3D(0, 1, 0);
            Assert.That(target.IsValid(out reason), Is.False);
            Assert.That(reason, Does.Contain("plane"));

            target.ApproachDirection = new Vector3D(1, 0, 0);
            Assert.That(target.IsValid(out reason), Is.True, reason);
        }

        [Test]
        public void FreeArrivalThroughBothFacesIsWhatItAlwaysWas()
        {
            StreamlineGenerationResult result = StreamlineGenerator.Generate(
                new ObstacleField(), new[] { Slot() }, StandingTarget(), Settings());
            TestContext.Progress.WriteLine("free, both: " + result.Describe());

            Assert.That(result.Status, Is.EqualTo(StreamlineGridStatus.Connected));
            Assert.That(result.ArrivedCount, Is.GreaterThan(0), result.Describe());
            Assert.That(result.ServedTargetCellCount, Is.EqualTo(result.Grid!.SinkLeaves.Count),
                        "nothing is shut off when the arrival is unconstrained");
        }

        [Test]
        public void APerpendicularArrivalIsExactlyOnTheNormal()
        {
            TargetPolygon target = StandingTarget();
            target.Incidence = TargetIncidence.Perpendicular;
            target.Sides = TargetSides.One;
            target.ApproachDirection = new Vector3D(1, 0, 0);
            target.LandingLength = 32.0;

            StreamlineGenerationResult result = StreamlineGenerator.Generate(
                new ObstacleField(), new[] { Slot() }, target, Settings());
            TestContext.Progress.WriteLine("perpendicular, one face: " + result.Describe());

            Assert.That(result.Status, Is.EqualTo(StreamlineGridStatus.Connected), result.Describe());
            Assert.That(result.ArrivedCount, Is.GreaterThan(0), result.Describe());

            // inside a tube every lateral flux is zero, so the lateral velocity is identically zero and
            // the tangent is on the axis to the last bit rather than merely close to it
            double worst = 0;
            int counted = 0;
            foreach (Streamline line in result.Streamlines)
            {
                if (line.Count < 2)
                {
                    continue;
                }
                GetArrivalTangent(line, out double n, out double e, out double v);
                if (n <= 0)
                {
                    continue;                 // did not end at the target, so says nothing about arrival
                }
                counted++;
                double off = System.Math.Sqrt(e * e + v * v);
                if (off > worst) { worst = off; }
            }
            Assert.That(counted, Is.GreaterThan(0));
            TestContext.Progress.WriteLine(
                $"{counted} arrivals, worst departure from the normal {worst:E2}");
            Assert.That(worst, Is.LessThan(1.0e-9),
                        "a tube of closed faces leaves the velocity nowhere to point but along it");
        }

        [Test]
        public void OnlyTheAdmittedFaceIsReachedWhenOneIsAsked()
        {
            // a slot on each side of the standing target, so that both sides are genuinely on offer
            StreamlineSource near = Slot(0);
            StreamlineSource far = Slot(2.0 * TargetNorth);
            TargetPolygon target = StandingTarget();
            target.Sides = TargetSides.One;
            target.ApproachDirection = new Vector3D(1, 0, 0);     // arriving travelling north

            StreamlineGenerationResult result = StreamlineGenerator.Generate(
                new ObstacleField(), new[] { near, far }, target, Settings());
            TestContext.Progress.WriteLine("free, one face: " + result.Describe());
            Assert.That(result.Status, Is.EqualTo(StreamlineGridStatus.Connected), result.Describe());
            Assert.That(result.ArrivedCount, Is.GreaterThan(0), result.Describe());

            foreach (Streamline line in result.Streamlines)
            {
                if (line.Count < 2)
                {
                    continue;
                }
                GetArrivalTangent(line, out double n, out double _, out double _);
                Point3D last = line.Positions![line.Count - 1];
                if (!target.Contains(last.X!.Value, last.Y!.Value, last.Z!.Value))
                {
                    continue;
                }
                Assert.That(n, Is.GreaterThan(-1.0e-12),
                            "a streamline arrived through the face that was shut");
            }
        }

        [Test]
        public void BothFacesAreReachedWhenBothAreAsked()
        {
            StreamlineSource near = Slot(0);
            StreamlineSource far = Slot(2.0 * TargetNorth);
            TargetPolygon target = StandingTarget();
            target.Incidence = TargetIncidence.Perpendicular;
            target.Sides = TargetSides.Both;
            target.LandingLength = 32.0;

            StreamlineGenerationResult result = StreamlineGenerator.Generate(
                new ObstacleField(), new[] { near, far }, target, Settings());
            TestContext.Progress.WriteLine("perpendicular, both faces: " + result.Describe());
            Assert.That(result.Status, Is.EqualTo(StreamlineGridStatus.Connected), result.Describe());

            int northward = 0;
            int southward = 0;
            double worst = 0;
            foreach (Streamline line in result.Streamlines)
            {
                if (line.Count < 2)
                {
                    continue;
                }
                GetArrivalTangent(line, out double n, out double e, out double v);
                Point3D last = line.Positions![line.Count - 1];
                if (!target.Contains(last.X!.Value, last.Y!.Value, last.Z!.Value))
                {
                    continue;
                }
                if (n > 0) { northward++; } else { southward++; }
                double off = System.Math.Sqrt(e * e + v * v);
                if (off > worst) { worst = off; }
            }
            TestContext.Progress.WriteLine(
                $"{northward} arriving northward, {southward} southward, worst off-axis {worst:E2}");
            Assert.That(northward, Is.GreaterThan(0), "nothing arrived through the near face");
            Assert.That(southward, Is.GreaterThan(0), "nothing arrived through the far face");
            Assert.That(worst, Is.LessThan(1.0e-9), "an arrival left the normal");
        }

        /// <summary>
        /// A conduit is where the well starts, so it has to start at the slot whatever curve it is shaped
        /// to. Shaping to a scouted streamline once moved it: a streamline begins where it was launched,
        /// which is the outlet of the scout's own conduit, and the rate is injected at the head of a
        /// conduit, so the well quietly left from a hundred metres down.
        /// </summary>
        [Test]
        public void AShapedConduitStillStartsAtTheSlot()
        {
            TargetPolygon target = StandingTarget();
            target.Sides = TargetSides.One;
            target.ApproachDirection = new Vector3D(1, 0, 0);
            StreamlineGeneratorOptions settings = Settings();
            settings.ShapeDepartureToSpine = true;

            foreach (bool twoPass in new[] { false, true })
            {
                StreamlineGenerationResult result = twoPass
                    ? StreamlineGenerator.GenerateShaped(new ObstacleField(), new[] { Slot() }, target,
                                                         settings)
                    : StreamlineGenerator.Generate(new ObstacleField(), new[] { Slot() }, target,
                                                   settings);
                string what = twoPass ? "two pass" : "one pass";
                Assert.That(result.Status, Is.EqualTo(StreamlineGridStatus.Connected), what);
                Assert.That(result.ConduitPaths.Count, Is.GreaterThan(0), what);

                Point3D head = result.ConduitPaths[0][0];
                int atHead = result.Grid!.Tree.FindLeaf(head.X!.Value, head.Y!.Value, head.Z!.Value);
                TestContext.Progress.WriteLine(
                    $"{what}: conduit starts at {head.X!.Value:F1}, {head.Y!.Value:F1}, "
                    + $"{head.Z!.Value:F1}, {result.ConduitPaths[0].Count} cells");
                Assert.That(atHead, Is.EqualTo(result.Grid.SourceLeaves[0]),
                            $"{what}: the conduit does not start in the slot's own cell");
            }
        }

        /// <summary>
        /// Every candidate is a well path, so every candidate begins at the slot. The tracer starts
        /// where it was launched, at the conduit's outlet, so the conduit has to be carried back onto
        /// the front of what it returns or the generation claims the well starts underground.
        /// </summary>
        [Test]
        public void EveryProducedPathBeginsAtTheSlot()
        {
            StreamlineSource slot = Slot();
            foreach (bool shaped in new[] { false, true })
            {
                StreamlineGeneratorOptions settings = Settings();
                settings.ShapeDepartureToSpine = shaped;
                StreamlineGenerationResult result = StreamlineGenerator.Generate(
                    new ObstacleField(), new[] { slot }, StandingTarget(), settings);
                Assert.That(result.Streamlines.Count, Is.GreaterThan(0));

                double worst = 0;
                foreach (Streamline line in result.Streamlines)
                {
                    Point3D first = line.Positions![0];
                    double gap = System.Math.Sqrt(
                        System.Math.Pow(first.X!.Value - slot.Position!.X!.Value, 2)
                        + System.Math.Pow(first.Y!.Value - slot.Position.Y!.Value, 2)
                        + System.Math.Pow(first.Z!.Value - slot.Position.Z!.Value, 2));
                    if (gap > worst) { worst = gap; }
                }
                TestContext.Progress.WriteLine(
                    $"{(shaped ? "shaped" : "straight")}: {result.Streamlines.Count} paths, "
                    + $"furthest start {worst:E2} m from the slot");
                Assert.That(worst, Is.LessThan(1.0e-9),
                            "a produced path does not begin at the slot");
            }
        }

        /// <summary>
        /// A conduit must give out along the way the well is going, not sideways. A chain of cells can
        /// only step along an axis, so a shaped conduit is a staircase, and if it happens to end on a
        /// lateral step then every streamline launched across that face leaves at a right angle to the
        /// path — the very thing the conduit exists to prevent.
        /// </summary>
        [Test]
        public void AShapedConduitGivesOutAlongItsOwnDirection()
        {
            TargetPolygon target = StandingTarget();
            target.Sides = TargetSides.One;
            target.ApproachDirection = new Vector3D(1, 0, 0);
            StreamlineGeneratorOptions settings = Settings();
            settings.ShapeDepartureToSpine = true;
            settings.ConduitLength = 60.0;

            StreamlineGenerationResult result = StreamlineGenerator.Generate(
                new ObstacleField(), new[] { Slot() }, target, settings);
            Assert.That(result.ConduitPaths.Count, Is.GreaterThan(0));
            List<Point3D> run = result.ConduitPaths[0];
            Assert.That(run.Count, Is.GreaterThan(2));

            // the last step of the chain, against the way the chain as a whole is heading
            double lastN = run[run.Count - 1].X!.Value - run[run.Count - 2].X!.Value;
            double lastE = run[run.Count - 1].Y!.Value - run[run.Count - 2].Y!.Value;
            double lastV = run[run.Count - 1].Z!.Value - run[run.Count - 2].Z!.Value;
            int at = System.Math.Max(0, run.Count - 6);
            double runN = run[run.Count - 1].X!.Value - run[at].X!.Value;
            double runE = run[run.Count - 1].Y!.Value - run[at].Y!.Value;
            double runV = run[run.Count - 1].Z!.Value - run[at].Z!.Value;
            double[] overall = { runN, runE, runV };
            int dominant = 0;
            for (int a = 1; a < 3; a++)
            {
                if (System.Math.Abs(overall[a]) > System.Math.Abs(overall[dominant])) { dominant = a; }
            }
            double[] step = { lastN, lastE, lastV };
            TestContext.Progress.WriteLine(
                $"conduit of {run.Count} cells, last step ({lastN:F1}, {lastE:F1}, {lastV:F1}), "
                + $"heading ({runN:F1}, {runE:F1}, {runV:F1})");
            Assert.That(step[dominant] * overall[dominant], Is.GreaterThan(0),
                        "the conduit gives out across the way it is going");
        }

        [Test]
        public void ACeilingKeepsEverythingBelowIt()
        {
            StreamlineGeneratorOptions settings = Settings();
            settings.Grid.CeilingVertical = 0.0;                  // the slot sits exactly on it

            StreamlineGenerationResult result = StreamlineGenerator.Generate(
                new ObstacleField(), new[] { Slot() }, StandingTarget(), settings);
            TestContext.Progress.WriteLine("with a ceiling: " + result.Describe());
            Assert.That(result.Status, Is.EqualTo(StreamlineGridStatus.Connected), result.Describe());
            Assert.That(result.ArrivedCount, Is.GreaterThan(0), result.Describe());

            // only cells lying wholly above the ceiling are shut, so one cell of slack is expected and
            // anything beyond that is the ceiling failing to hold
            double slack = result.Grid!.Tree.Frame.GetCellSize(result.Grid.Tree.DeepestDepth);
            double highest = double.MaxValue;
            foreach (Streamline line in result.Streamlines)
            {
                foreach (Point3D position in line.Positions!)
                {
                    if (position.Z!.Value < highest) { highest = position.Z.Value; }
                }
            }
            TestContext.Progress.WriteLine($"shallowest position {highest:F2} m, slack {slack:F2} m");
            Assert.That(highest, Is.GreaterThan(settings.Grid.CeilingVertical!.Value - slack),
                        "a streamline climbed through the ceiling");
        }

        [Test]
        public void AnArrivalFromBehindGoesRoundTheTargetRatherThanThroughIt()
        {
            // one face admitted, and it is the far one, with the only slot on the near side and the
            // domain too tight to get round the rim
            TargetPolygon target = StandingTarget();
            target.Incidence = TargetIncidence.Perpendicular;
            target.Sides = TargetSides.One;
            target.ApproachDirection = new Vector3D(-1, 0, 0);    // must arrive travelling south
            target.LandingLength = 32.0;

            StreamlineGeneratorOptions settings = Settings();
            StreamlineGenerationResult result = StreamlineGenerator.Generate(
                new ObstacleField(), new[] { Slot() }, target, settings);
            TestContext.Progress.WriteLine("arrival from behind: " + result.Describe());

            // it is reachable only by going round the target, which the box does allow, so the useful
            // assertion is that whatever is reported is consistent rather than that it fails
            if (result.Status == StreamlineGridStatus.Connected)
            {
                foreach (Streamline line in result.Streamlines)
                {
                    if (line.Count < 2) { continue; }
                    GetArrivalTangent(line, out double n, out double e, out double v);
                    Point3D last = line.Positions![line.Count - 1];
                    if (!target.Contains(last.X!.Value, last.Y!.Value, last.Z!.Value)) { continue; }
                    Assert.That(n, Is.LessThan(1.0e-9), "it arrived the wrong way round");
                    Assert.That(System.Math.Sqrt(e * e + v * v), Is.LessThan(1.0e-9));
                }
            }
            else
            {
                Assert.That(result.Streamlines.Count, Is.EqualTo(0),
                            "a refusal must not come with streamlines");
            }
        }
    }
}
