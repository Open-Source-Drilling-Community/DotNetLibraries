using System.Globalization;
using System.Text;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// The whole chain written out for display: generation, bundling, and one factory per bundle, with
    /// each factory shown as what it actually is — a median path, a series of outlines perpendicular to
    /// it, and streamlines drawn from its density.
    /// </summary>
    [TestFixture]
    public class UllriggFactoryScene
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private const double Ground = -91.2;
        private const double TargetVertical = 900.0;
        private const double TargetHalf = 80.0;
        private const int Launches = 1000;
        private const int DrawsPerBundle = 12;
        private const int OutlineStride = 16;

        private static ObstacleField LoadUllrigg(out double[] wellhead)
        {
            string directory = Path.Combine(TestContext.CurrentContext.TestDirectory,
                                            "UllriggUncertainty");
            Dictionary<string, double[]> heads = new Dictionary<string, double[]>();
            foreach (string line in File.ReadLines(Path.Combine(directory, "wellheads.txt")))
            {
                string text = line.Trim();
                if (text.Length == 0 || text.StartsWith("#", StringComparison.Ordinal)) { continue; }
                string[] parts = text.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4) { continue; }
                heads[parts[0]] = new[] { double.Parse(parts[1], Invariant),
                                          double.Parse(parts[2], Invariant),
                                          double.Parse(parts[3], Invariant) };
            }
            ObstacleField field = new ObstacleField();
            foreach (string file in Directory.GetFiles(directory, "*.txt").OrderBy(f => f))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (!heads.TryGetValue(name, out double[]? head)) { continue; }
                List<WellboreUncertaintyStation> stations = SurveyFileReader.ReadWithUncertainty(file);
                if (stations.Count < 2) { continue; }
                field.Add(new WellboreUncertainty(stations, head[0], head[1], head[2], 2.0) { Name = name });
            }
            field.BuildIndex();
            wellhead = heads["U1"];
            return field;
        }

        /// <summary>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~WriteFactoryScene"</c>.
        /// </summary>
        [Test]
        [Explicit("a full generation, bundling and a factory per bundle; writes a scene file")]
        public void WriteFactoryScene()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);
            double[] origin = u1;
            double[] slotAt = { origin[0] + 1.2, origin[1] + 1.2, origin[2] };
            double[] targetAt = { origin[0] - 400.0, origin[1] + 400.0, TargetVertical };
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(targetAt[0] - TargetHalf, targetAt[1] - TargetHalf, targetAt[2]),
                new Point3D(targetAt[0] + TargetHalf, targetAt[1] - TargetHalf, targetAt[2]),
                new Point3D(targetAt[0] + TargetHalf, targetAt[1] + TargetHalf, targetAt[2]),
                new Point3D(targetAt[0] - TargetHalf, targetAt[1] + TargetHalf, targetAt[2])
            };
            StreamlineSource slot = new StreamlineSource(new Point3D(slotAt[0], slotAt[1], slotAt[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = 40.0
            };
            StreamlineGenerationResult got = StreamlineGenerator.GenerateChannelled(
                field, new[] { slot }, new TargetPolygon(corners, 20.0),
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = 1.0,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = Ground,
                        FloorVertical = targetAt[2] + 40.0
                    },
                    StreamlineCount = Launches,
                    ChannelContrast = 100.0,
                    ChannelNarrowWidth = 25.0,
                    ChannelWideWidth = 150.0,
                    OutletGuideContrast = 20.0,
                    OutletGuideLength = 120.0,
                    OutletGuideWidth = 30.0,
                    LaunchInset = 0.25,
                    Relaxer = new SpineRelaxerOptions { HoldLength = 40.0 }
                }, 1);
            TestContext.Progress.WriteLine("generation: " + got.Describe());

            List<Streamline> arrived = new List<Streamline>();
            for (int s = 0; s < got.Streamlines.Count; s++)
            {
                if (s < got.StreamlineOutcomes.Count
                    && got.StreamlineOutcomes[s] != TraceOutcome.ReachedSink)
                {
                    continue;
                }
                arrived.Add(got.Streamlines[s]);
            }
            StreamlineBundlingResult bundling = new StreamlineBundler().Bundle(arrived);
            StreamlineListSource source = new StreamlineListSource(arrived);
            List<IReadOnlyList<int>> asIndices = new List<IReadOnlyList<int>>();
            foreach (StreamlineBundle bundle in bundling.Bundles)
            {
                asIndices.Add(bundle.StreamlineIndices);
            }
            StreamlineFactorySet set = new StreamlineFactorySetBuilder().Build(source, asIndices, field);
            TestContext.Progress.WriteLine(
                $"{bundling.Bundles.Count} bundles from {arrived.Count}, {set.Factories.Count} corridors,"
                + $" {set.SplitCount} splits, {set.Tolerance?.GroupCount ?? 0} groups genuinely apart");

            StringBuilder json = new StringBuilder();
            json.Append("{\"origin\":[").Append(Num(origin[0])).Append(',').Append(Num(origin[1]))
                .Append(",0],");
            json.Append("\"ground\":").Append(Num(Ground)).Append(',');
            json.Append("\"slot\":").Append(Point(slotAt, origin)).Append(',');
            json.Append("\"target\":[");
            for (int c = 0; c < corners.Count; c++)
            {
                if (c > 0) { json.Append(','); }
                json.Append(Point(new[] { corners[c].X!.Value, corners[c].Y!.Value, corners[c].Z!.Value },
                                  origin));
            }
            json.Append(']');
            AppendWells(json, field, origin);

            json.Append(",\"bundles\":[");
            Random random = new Random(20260920);
            bool firstBundle = true;
            for (int b = 0; b < set.Factories.Count; b++)
            {
                StreamlineBundleFactory factory = set.Factories[b];
                int memberCount = set.Members[b].Count;

                // the room a well has around the planned path, read where the corridor actually is: the
                // head is still one point and the tail converges on the target, so both ends are pinched
                // by construction and say nothing about what can be drilled in between
                int firstStation = factory.Stations.Count / 10;
                int lastStation = factory.Stations.Count - 1 - factory.Stations.Count / 10;
                double leastInradius = double.MaxValue;
                for (int at = firstStation; at <= lastStation && at < factory.Stations.Count; at++)
                {
                    CrossSectionPolygon? polygon = factory.Stations[at].Polygon;
                    if (polygon != null && polygon.Inradius < leastInradius)
                    {
                        leastInradius = polygon.Inradius;
                    }
                }
                if (leastInradius == double.MaxValue) { leastInradius = 0; }
                // how often the median itself is inside a forbidden zone. Where it is, the march has
                // nowhere to go from its first step, the region collapses to the median point, and that
                // point is inside a volume: the inradius then correctly reads zero, but it is worth
                // saying so rather than letting it surface as a region that trespasses.
                int medianInside = 0;
                double medianWorst = 0;
                foreach (CrossSectionStation station in factory.Stations)
                {
                    if (station.Position == null) { continue; }
                    double excess = field.GetNearestExcess(station.Position.X!.Value,
                                                           station.Position.Y!.Value,
                                                           station.Position.Z!.Value, 60.0);
                    if (excess < 0)
                    {
                        medianInside++;
                        if (-excess > medianWorst) { medianWorst = -excess; }
                    }
                }

                // The factory holds the median's curvature in rad/m, as everything in the library does.
                // A view is read by people who think in degrees per thirty metres, so the conversion
                // happens here, at the boundary, and nowhere inside.
                const double toDegreesPerStation = 180.0 / System.Math.PI
                                                   * StreamlineCurvature.StandardStation;
                double dogleg = factory.MedianCurvature * toDegreesPerStation;
                double doglegLong = factory.MedianCurvatureLong * toDegreesPerStation;
                // and the median on its own, without the shared head in front of it, to say whether a
                // corner sits at the join between the two
                Streamline bare = factory.GetMedianCurve();
                double doglegBare = StreamlineCurvature.GetWorst(bare) * 180.0 / System.Math.PI;
                TestContext.Progress.WriteLine(
                    $"  corridor {b}: {memberCount,4} members, least inradius {leastInradius,6:0.00} m,"
                    + $" residual {100.0 * factory.ResidualFraction:0.0}%,"
                    + $" {factory.OutsideMemberCount} unrepresented,"
                    + $" median inside a volume at {medianInside} of {factory.Stations.Count} stations"
                    + (medianInside > 0 ? $" (up to {medianWorst:0.00} m in)" : "")
                    + $", group {(set.Tolerance != null && b < set.Tolerance.Group.Length ? set.Tolerance.Group[b] : 0)}"
                    + $", median turns {dogleg:0.0} deg/30 m ({doglegLong:0.0} over 120 m,"
                    + $" {doglegBare:0.0} without the head)");

                if (!firstBundle) { json.Append(','); }
                firstBundle = false;
                json.Append("{\"index\":").Append(b);
                json.Append(",\"group\":")
                    .Append(set.Tolerance != null && b < set.Tolerance.Group.Length
                            ? set.Tolerance.Group[b] : 0);
                json.Append(",\"members\":").Append(memberCount);
                json.Append(",\"leastInradius\":")
                    .Append(leastInradius.ToString("0.###", Invariant));
                json.Append(",\"residual\":")
                    .Append((100.0 * factory.ResidualFraction).ToString("0.#", Invariant));
                json.Append(",\"tubeRatio\":")
                    .Append(factory.MaximumTubeRatio.ToString("0.###", Invariant));
                json.Append(",\"length\":").Append(Num(factory.Length));
                json.Append(",\"head\":").Append(factory.SharedHead.Count);

                json.Append(",\"dogleg\":").Append(dogleg.ToString("0.##", Invariant));
                json.Append(",\"doglegLong\":").Append(doglegLong.ToString("0.##", Invariant));
                json.Append(",\"doglegBare\":").Append(doglegBare.ToString("0.##", Invariant));

                // the median path, with the shared head in front of it so it starts at the slot
                json.Append(",\"median\":[");
                List<Point3D> spine = new List<Point3D>(factory.SharedHead);
                spine.AddRange(factory.GetMedianCurve().Positions!);
                int stride = System.Math.Max(1, spine.Count / 160);
                bool firstPoint = true;
                for (int i = 0; i < spine.Count; i += stride)
                {
                    if (!firstPoint) { json.Append(','); }
                    firstPoint = false;
                    json.Append(Point(new[] { spine[i].X!.Value, spine[i].Y!.Value, spine[i].Z!.Value },
                                      origin));
                }
                json.Append(']');

                // the outlines, as closed rings in the plane of their own cross-section
                json.Append(",\"outlines\":[");
                bool firstRing = true;
                for (int at = 0; at < factory.Stations.Count; at += OutlineStride)
                {
                    CrossSectionStation station = factory.Stations[at];
                    if (station.Polygon == null) { continue; }
                    if (!firstRing) { json.Append(','); }
                    firstRing = false;
                    json.Append("{\"inradius\":")
                        .Append(station.Polygon.Inradius.ToString("0.###", Invariant));
                    json.Append(",\"ring\":[");
                    int sectors = station.Polygon.DirectionCount;
                    for (int s = 0; s < sectors; s++)
                    {
                        if (s > 0) { json.Append(','); }
                        double angle = 2.0 * System.Math.PI * (s + 0.5) / sectors;
                        Point3D? at3D = station.GetPosition(1.0, angle);
                        if (at3D == null) { json.Append("[0,0,0]"); continue; }
                        json.Append(Point(new[] { at3D.X!.Value, at3D.Y!.Value, at3D.Z!.Value }, origin));
                    }
                    json.Append("]}");
                }
                json.Append(']');

                // and what the factory is for: streamlines drawn from it, holding none of the originals
                json.Append(",\"draws\":[");
                for (int d = 0; d < DrawsPerBundle; d++)
                {
                    Streamline? drawn = factory.Draw(random);
                    if (drawn == null || drawn.Count < 2) { continue; }
                    if (d > 0) { json.Append(','); }
                    List<Point3D> positions = drawn.Positions!;
                    int drawStride = System.Math.Max(1, positions.Count / 140);
                    json.Append('[');
                    bool firstDrawn = true;
                    for (int i = 0; i < positions.Count; i += drawStride)
                    {
                        if (!firstDrawn) { json.Append(','); }
                        firstDrawn = false;
                        json.Append(Point(new[] { positions[i].X!.Value, positions[i].Y!.Value,
                                                  positions[i].Z!.Value }, origin));
                    }
                    json.Append(']');
                }
                json.Append("]}");
            }
            json.Append("]}");

            string output = Path.Combine(Path.GetTempPath(), "ullrigg-factory-scene.json");
            File.WriteAllText(output, json.ToString());
            TestContext.Progress.WriteLine($"wrote {new FileInfo(output).Length / 1024} kB to {output}");
        }

        private static void AppendWells(StringBuilder json, ObstacleField field, double[] origin)
        {
            const int aroundCount = 14;
            json.Append(",\"wells\":[");
            for (int w = 0; w < field.Wells.Count; w++)
            {
                WellboreUncertainty well = field.Wells[w];
                int stride = System.Math.Max(1, well.SampleCount / 80);
                if (w > 0) { json.Append(','); }
                json.Append("{\"name\":\"").Append(well.Name).Append("\",\"rings\":[");
                bool firstRing = true;
                for (int sample = 0; sample < well.SampleCount; sample += stride)
                {
                    if (!firstRing) { json.Append(','); }
                    firstRing = false;
                    json.Append('[');
                    for (int a = 0; a < aroundCount; a++)
                    {
                        if (a > 0) { json.Append(','); }
                        well.GetSectionPoint(sample, 2.0 * System.Math.PI * a / aroundCount,
                                             out double n, out double e, out double v);
                        json.Append(Point(new[] { n, e, v }, origin));
                    }
                    json.Append(']');
                }
                json.Append("]}");
            }
            json.Append(']');
        }

        private static string Point(double[] position, double[] origin)
        {
            return "[" + Num(position[0] - origin[0]) + "," + Num(position[1] - origin[1]) + ","
                   + Num(position[2]) + "]";
        }

        private static string Num(double value)
        {
            return value.ToString("0.###", Invariant);
        }
    }
}
