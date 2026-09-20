using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// The generation grid on the real Ullrigg data.
    /// <para>
    /// The ellipses of uncertainty were computed by the trajectory service at a confidence of 0.99 and
    /// exported station by station, and the wellheads are the real slot positions of Ullrigg's cluster on
    /// the Ullandhaug field. Positions are Riemannian north and east in metres and the vertical is
    /// positive downward, so the wellheads sit at -91.2, ninety one metres above the ellipsoid.
    /// </para>
    /// <para>
    /// Unlike <see cref="UllriggGenerationTests"/>, which invents its ellipses and its slot grid and so
    /// stands on its own, these depend on the exported data being present.
    /// </para>
    /// </summary>
    [TestFixture]
    public class UllriggRealDataTests
    {
        private const double Confidence = 0.99;

        private static string GetDataDirectory()
        {
            string candidate = Path.Combine(TestContext.CurrentContext.TestDirectory, "UllriggUncertainty");
            Assert.That(Directory.Exists(candidate), Is.True, $"no exported uncertainty at {candidate}");
            return candidate;
        }

        /// <summary>
        /// the real wellhead of every well, by name
        /// </summary>
        private static Dictionary<string, double[]> LoadWellheads()
        {
            Dictionary<string, double[]> heads = new Dictionary<string, double[]>();
            foreach (string line in File.ReadLines(Path.Combine(GetDataDirectory(), "wellheads.txt")))
            {
                string text = line.Trim();
                if (text.Length == 0 || text.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }
                string[] parts = text.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4)
                {
                    continue;
                }
                heads[parts[0]] = new[]
                {
                    double.Parse(parts[1], CultureInfo.InvariantCulture),
                    double.Parse(parts[2], CultureInfo.InvariantCulture),
                    double.Parse(parts[3], CultureInfo.InvariantCulture)
                };
            }
            return heads;
        }

        private static ObstacleField LoadUllrigg()
        {
            Dictionary<string, double[]> heads = LoadWellheads();
            ObstacleField field = new ObstacleField();
            foreach (string file in Directory.GetFiles(GetDataDirectory(), "*.txt").OrderBy(f => f))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (!heads.TryGetValue(name, out double[]? head))
                {
                    continue;
                }
                List<WellboreUncertaintyStation> stations = SurveyFileReader.ReadWithUncertainty(file);
                if (stations.Count < 2)
                {
                    continue;
                }
                field.Add(new WellboreUncertainty(stations, head[0], head[1], head[2], 2.0) { Name = name });
            }
            field.BuildIndex();
            return field;
        }

        private static TargetPolygon Target(double north, double east, double vertical, double half)
        {
            List<Point3D> vertices = new List<Point3D>
            {
                new Point3D(north - half, east - half, vertical),
                new Point3D(north + half, east - half, vertical),
                new Point3D(north + half, east + half, vertical),
                new Point3D(north - half, east + half, vertical)
            };
            return new TargetPolygon(vertices, 20.0);
        }

        [Test]
        public void TheExportedUncertaintyLoadsAndIsWellFormed()
        {
            ObstacleField field = LoadUllrigg();
            Assert.That(field.Wells.Count, Is.EqualTo(13), "thirteen actual trajectories were exported");
            foreach (WellboreUncertainty well in field.Wells)
            {
                Assert.That(well.SampleCount, Is.GreaterThan(5), well.Name);
                Assert.That(well.MaximumSemiAxis, Is.GreaterThan(0), well.Name);
                Assert.That(well.MaximumSweepRatio, Is.LessThan(1.0),
                            $"{well.Name}: the swept ellipse folds over itself, "
                            + $"ratio {well.MaximumSweepRatio:F3}");
            }
            // the real ellipses are far larger than the nominal model guessed: several wells pass fifty
            // metres of semi-major axis at 0.99
            Assert.That(field.MaximumSemiAxis, Is.GreaterThan(40.0));
        }

        [Test]
        public void TheRealClusterIsTightAtTheTopAndOpenBelow()
        {
            ObstacleField field = LoadUllrigg();
            // a new slot between the existing ones, three metres north of U1's wellhead
            Dictionary<string, double[]> heads = LoadWellheads();
            double[] u1 = heads["U1"];
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1));
            StreamlineGrid grid = StreamlineGrid.Build(field, new[] { slot },
                                                       Target(u1[0] - 400.0, u1[1] + 400.0, 900.0, 80.0),
                                                       new StreamlineGridOptions
                                                       {
                                                           FinestCellSize = 0.5,
                                                           CoarsestCellSize = 25.0
                                                       });
            TestContext.Progress.WriteLine("new slot, real data: " + grid.Describe());
            Assert.That(grid.Status, Is.Not.EqualTo(StreamlineGridStatus.TargetBlocked), grid.Describe());
        }

        [Test]
        public void TheLateralIsARealTieInOnItsParent()
        {
            // U4Lateral starts at a measured depth of 455.8 m with its wellhead four hundred metres down,
            // so it is a genuine sidetrack rather than a well from surface, and its start lies inside U4
            ObstacleField field = LoadUllrigg();
            int parent = -1;
            int lateral = -1;
            for (int w = 0; w < field.Wells.Count; w++)
            {
                if (field.Wells[w].Name == "U4") { parent = w; }
                if (field.Wells[w].Name == "U4Lateral") { lateral = w; }
            }
            Assert.That(parent, Is.GreaterThanOrEqualTo(0));
            Assert.That(lateral, Is.GreaterThanOrEqualTo(0));

            field.Wells[lateral].GetSample(0, out double n, out double e, out double v);
            Assert.That(field.Wells[lateral].GetMeasuredDepth(0), Is.GreaterThan(400.0),
                        "the lateral does not start at surface");
            // without muting, that start is inside the parent
            double excess = field.GetNearestExcess(n, e, v, 500.0);
            Assert.That(excess, Is.LessThan(0.0),
                        $"the tie-in should be inside an existing volume, excess {excess:F2} m");
            TestContext.Progress.WriteLine(
                $"U4Lateral tie-in at MD {field.Wells[lateral].GetMeasuredDepth(0):F1}, "
                + $"{-excess:F2} m inside the nearest volume");
        }

        [Test]
        public void AWellPathIsGeneratedFromANewSlotToATarget()
        {
            ObstacleField field = LoadUllrigg();
            Dictionary<string, double[]> heads = LoadWellheads();
            double[] u1 = heads["U1"];
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1));
            Stopwatch watch = Stopwatch.StartNew();
            StreamlineGenerationResult result = StreamlineGenerator.Generate(field, new[] { slot },
                Target(u1[0] - 400.0, u1[1] + 400.0, 900.0, 80.0),
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions { FinestCellSize = 1.0, CoarsestCellSize = 25.0 },
                    StreamlineCount = 36,
                    ConduitLength = 30.0
                });
            watch.Stop();
            TestContext.Progress.WriteLine($"Ullrigg, real data, {watch.ElapsedMilliseconds} ms: "
                                           + result.Describe());

            Assert.That(result.Status, Is.EqualTo(StreamlineGridStatus.Connected));
            Assert.That(result.Field, Is.Not.Null);
            Assert.That(result.Field!.WorstCellImbalance, Is.LessThan(1e-6),
                        "the field must be conservative or the streamlines mean nothing");
            Assert.That(result.Streamlines.Count, Is.GreaterThan(0), result.Describe());

            // nothing produced may lie inside an uncertainty volume: that is the whole point
            foreach (Streamline line in result.Streamlines)
            {
                foreach (Point3D position in line.Positions!)
                {
                    int leaf = result.Grid!.Tree.FindLeaf(position.X!.Value, position.Y!.Value,
                                                          position.Z!.Value);
                    if (leaf < 0)
                    {
                        continue;
                    }
                    Assert.That(result.Grid.States[leaf], Is.EqualTo(CellState.Open),
                                "a produced path entered a closed cell");
                }
            }

            // and what comes out has to be usable by the rest of the library
            StreamlineBundlingResult bundling = new StreamlineBundler().Bundle(result.Streamlines);
            TestContext.Progress.WriteLine("bundled into " + StreamlineFixtures.Describe(bundling));
            Assert.That(bundling.Bundles.Count, Is.GreaterThan(0));
        }

        /// <summary>
        /// The same case with the constraints a well path actually has: the ground overhead, the slot
        /// holding vertical over a conductor's length, and an arrival that comes down onto the top of the
        /// target rather than from wherever the medium happens to send it.
        /// </summary>
        [Test]
        public void TheConstrainedCaseStaysUndergroundAndLandsOnTheNormal()
        {
            ObstacleField field = LoadUllrigg();
            Dictionary<string, double[]> heads = LoadWellheads();
            double[] u1 = heads["U1"];
            const double ground = -91.2;

            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = 100.0
            };
            TargetPolygon target = Target(u1[0] - 400.0, u1[1] + 400.0, 900.0, 80.0);
            target.Incidence = TargetIncidence.Perpendicular;
            target.Sides = TargetSides.One;
            target.ApproachDirection = new Vector3D(0, 0, 1);     // arriving on the way down
            target.LandingLength = 60.0;

            Stopwatch watch = Stopwatch.StartNew();
            StreamlineGenerationResult result = StreamlineGenerator.Generate(field, new[] { slot }, target,
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = 1.0,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = ground
                    },
                    StreamlineCount = 36
                });
            watch.Stop();
            TestContext.Progress.WriteLine($"Ullrigg, constrained, {watch.ElapsedMilliseconds} ms: "
                                           + result.Describe());

            Assert.That(result.Status, Is.EqualTo(StreamlineGridStatus.Connected), result.Describe());
            Assert.That(result.ArrivedCount, Is.GreaterThan(0), result.Describe());

            double slack = result.Grid!.Tree.Frame.GetCellSize(result.Grid.Tree.DeepestDepth);
            double shallowest = double.MaxValue;
            double worstOffAxis = 0;
            int landed = 0;
            foreach (Streamline line in result.Streamlines)
            {
                foreach (Point3D position in line.Positions!)
                {
                    if (position.Z!.Value < shallowest) { shallowest = position.Z.Value; }
                }
                if (line.Count < 2) { continue; }
                List<Point3D> positions = line.Positions!;
                Point3D last = positions[positions.Count - 1];
                Point3D before = positions[positions.Count - 2];
                double dn = last.X!.Value - before.X!.Value;
                double de = last.Y!.Value - before.Y!.Value;
                double dv = last.Z!.Value - before.Z!.Value;
                double length = System.Math.Sqrt(dn * dn + de * de + dv * dv);
                if (!(length > 0) || dv <= 0) { continue; }
                landed++;
                double off = System.Math.Sqrt(dn * dn + de * de) / length;
                if (off > worstOffAxis) { worstOffAxis = off; }
            }
            TestContext.Progress.WriteLine(
                $"shallowest {shallowest:F1} m against a ground at {ground:F1}, {landed} landed, "
                + $"worst departure from the vertical {worstOffAxis:E2}");

            Assert.That(shallowest, Is.GreaterThan(ground - slack),
                        "a path climbed above the ground, which the ceiling exists to prevent");
            Assert.That(landed, Is.GreaterThan(0));
            Assert.That(worstOffAxis, Is.LessThan(1.0e-9),
                        "the landing sections must be on the normal of the target");

            // and nothing produced may lie inside an uncertainty volume, constraints or no constraints
            foreach (Streamline line in result.Streamlines)
            {
                foreach (Point3D position in line.Positions!)
                {
                    int leaf = result.Grid!.Tree.FindLeaf(position.X!.Value, position.Y!.Value,
                                                          position.Z!.Value);
                    if (leaf < 0) { continue; }
                    Assert.That(result.Grid.States[leaf], Is.EqualTo(CellState.Open),
                                "a produced path entered a closed cell");
                }
            }
        }

        /// <summary>
        /// Not part of the ordinary run: what the real uncertainty does to the room available, depth by
        /// depth. Invoke with
        /// <c>dotnet test --filter "FullyQualifiedName~TheRoomAvailableByDepth"</c>.
        /// </summary>
        [Test]
        [Explicit("measures the clearance against the real ellipses")]
        public void TheRoomAvailableByDepth()
        {
            ObstacleField field = LoadUllrigg();
            TestContext.Progress.WriteLine(
                $"thirteen wells at confidence {Confidence}, largest semi-axis "
                + $"{field.MaximumSemiAxis:F1} m");
            TestContext.Progress.WriteLine("   TVD band     samples   min gap   median gap   with room");
            for (int band = -100; band < 1600; band += 200)
            {
                List<double> gaps = new List<double>();
                foreach (WellboreUncertainty well in field.Wells)
                {
                    for (int s = 0; s < well.SampleCount; s++)
                    {
                        well.GetSample(s, out double n, out double e, out double v);
                        if (v < band || v >= band + 200)
                        {
                            continue;
                        }
                        // the room a planned well would have beside this one, allowing for its own
                        // uncertainty on both sides
                        ObstacleProximity p = field.GetProximity(n, e, v, 400.0);
                        if (p.SecondWell < 0)
                        {
                            continue;
                        }
                        gaps.Add(p.SecondExcess);
                    }
                }
                if (gaps.Count == 0)
                {
                    continue;
                }
                gaps.Sort();
                double median = gaps[gaps.Count / 2];
                int withRoom = gaps.Count(g => g > 0);
                TestContext.Progress.WriteLine(
                    $"  {band,5} to {band + 200,-5} {gaps.Count,8} {gaps[0],10:F1} {median,12:F1}"
                    + $" {100.0 * withRoom / gaps.Count,10:F0} %");
            }
        }
    }
}
