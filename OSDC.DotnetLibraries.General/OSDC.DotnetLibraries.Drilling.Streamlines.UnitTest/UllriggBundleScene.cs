using System.Globalization;
using System.Text;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// The produced corridor put through <see cref="StreamlineBundler"/>.
    /// <para>
    /// The bundler is already used inside <see cref="StreamlineGenerator.GenerateChannelled"/>, but on the
    /// <em>scout</em> streamlines, to find the ways round that the channel is then built along. This asks a
    /// different question of it: once the corridor has been produced, how many distinct ways round does it
    /// actually contain? A bundle is a connectivity class, so two paths are in one bundle exactly when
    /// nothing ever comes between them.
    /// </para>
    /// </summary>
    [TestFixture]
    public class UllriggBundleScene
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private const double Ground = -91.2;
        private const double TargetVertical = 900.0;
        private const double TargetHalf = 80.0;
        private const double LaunchInset = 0.25;

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
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~TheBundlesOfTheCorridor"</c>.
        /// </summary>
        [Test]
        [Explicit("one full generation on the real data, writes a scene file for display")]
        public void TheBundlesOfTheCorridor()
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
            TargetPolygon target = new TargetPolygon(corners, 20.0);
            StreamlineSource slot = new StreamlineSource(new Point3D(slotAt[0], slotAt[1], slotAt[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = 40.0
            };
            StreamlineGenerationResult result = StreamlineGenerator.GenerateChannelled(
                field, new[] { slot }, target,
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = 1.0,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = Ground,
                        FloorVertical = targetAt[2] + 40.0
                    },
                    StreamlineCount = 36,
                    ChannelContrast = 100.0,
                    ChannelNarrowWidth = 25.0,
                    ChannelWideWidth = 150.0,
                    OutletGuideContrast = 20.0,
                    OutletGuideLength = 120.0,
                    OutletGuideWidth = 30.0,
                    LaunchInset = LaunchInset,
                    Relaxer = new SpineRelaxerOptions { HoldLength = 40.0 }
                }, 1);
            TestContext.Progress.WriteLine("generation: " + result.Describe());

            // Only the paths that reached the target are candidate corridors. A stagnated stub stops
            // wherever it stopped, so bundling it would report a way round that nothing travels.
            List<int> candidates = new List<int>();
            List<Streamline> bundleInput = new List<Streamline>();
            for (int s = 0; s < result.Streamlines.Count; s++)
            {
                if (s < result.StreamlineOutcomes.Count
                    && result.StreamlineOutcomes[s] != TraceOutcome.ReachedSink)
                {
                    continue;
                }
                candidates.Add(s);
                bundleInput.Add(result.Streamlines[s]);
            }

            // the same corridor under both partition rules, so the two can be put side by side
            List<StreamlineBundlingResult> runs = new List<StreamlineBundlingResult>();
            List<int[]> bundleOfRun = new List<int[]>();
            foreach (BundlePartitionRule rule in new[] { BundlePartitionRule.Connected,
                                                         BundlePartitionRule.Separated })
            {
                StreamlineBundlingResult one = new StreamlineBundler(
                    new StreamlineBundlingOptions { PartitionRule = rule }).Bundle(bundleInput);
                int[] of = new int[result.Streamlines.Count];
                for (int s = 0; s < of.Length; s++) { of[s] = -1; }
                for (int k = 0; k < candidates.Count; k++)
                {
                    of[candidates[k]] = one.BundleIndexOfStreamline[k];
                }
                runs.Add(one);
                bundleOfRun.Add(of);
            }
            StreamlineBundlingResult bundling = runs[0];
            int[] bundleOf = bundleOfRun[0];

            for (int r = 0; r < runs.Count; r++)
            {
                TestContext.Progress.WriteLine(
                    (r == 0 ? "Connected: " : "Separated: ") + runs[r].Bundles.Count
                    + " bundles [" + string.Join("/", runs[r].Bundles.Select(b => b.Count))
                    + "] from " + candidates.Count + " candidates, "
                    + runs[r].SeparatedPairCount + " pairs ever apart, "
                    + runs[r].RefusedLinkCount + " links refused");
            }
            TestContext.Progress.WriteLine(
                $"{bundling.Bundles.Count} bundles from {candidates.Count} candidates "
                + $"({result.Streamlines.Count - candidates.Count} did not reach the target)");
            TestContext.Progress.WriteLine(
                $"  sweep spacing {bundling.SweepPlaneSpacing:0.0} m, planes "
                + $"{bundling.SweepPlaneCounts[0]}/{bundling.SweepPlaneCounts[1]}/"
                + $"{bundling.SweepPlaneCounts[2]}, {bundling.EffectiveCrossSectionCount} effective "
                + $"cross-sections, {bundling.CrossingCount} crossings, "
                + $"{bundling.AdjacentPairCount} adjacent pairs");
            foreach (StreamlineBundle bundle in bundling.Bundles)
            {
                double gentlest = double.MaxValue;
                double lowNorth = double.MaxValue, highNorth = double.MinValue;
                double lowEast = double.MaxValue, highEast = double.MinValue;
                foreach (int at in bundle.StreamlineIndices)
                {
                    Streamline line = bundleInput[at];
                    double worst = StreamlineCurvature.GetWorst(line) * 180.0 / System.Math.PI;
                    if (worst > 0 && worst < gentlest) { gentlest = worst; }
                    Point3D last = line.Positions![line.Positions!.Count - 1];
                    double n = last.X!.Value - targetAt[0];
                    double e = last.Y!.Value - targetAt[1];
                    lowNorth = System.Math.Min(lowNorth, n);
                    highNorth = System.Math.Max(highNorth, n);
                    lowEast = System.Math.Min(lowEast, e);
                    highEast = System.Math.Max(highEast, e);
                }
                TestContext.Progress.WriteLine(
                    $"  bundle {bundle.Index}: {bundle.Count} paths, gentlest "
                    + $"{(gentlest < double.MaxValue ? gentlest : double.NaN):0.0} deg/30 m, arrives "
                    + $"north {lowNorth:0.0} to {highNorth:0.0}, east {lowEast:0.0} to {highEast:0.0}");
            }

            WriteScene(field, result, runs, bundleOfRun, origin, slotAt, targetAt, corners);
        }

        /// <summary>
        /// Whether one bundle is the geometry or the configuration.
        /// <para>
        /// The channelled run is funnelled along its own spines, so it cannot be asked how many ways
        /// round exist — it was told. The scout pass, which is the plain dipole family in a uniform
        /// medium, is the one that can answer.
        /// </para>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~WhereTheOneBundleComesFrom"</c>.
        /// </summary>
        [Test]
        [Explicit("two full generations on the real data")]
        public void WhereTheOneBundleComesFrom()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);
            double[] targetAt = { u1[0] - 400.0, u1[1] + 400.0, TargetVertical };
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(targetAt[0] - TargetHalf, targetAt[1] - TargetHalf, targetAt[2]),
                new Point3D(targetAt[0] + TargetHalf, targetAt[1] - TargetHalf, targetAt[2]),
                new Point3D(targetAt[0] + TargetHalf, targetAt[1] + TargetHalf, targetAt[2]),
                new Point3D(targetAt[0] - TargetHalf, targetAt[1] + TargetHalf, targetAt[2])
            };

            foreach (bool channelled in new[] { false, true })
            {
                StreamlineSource slot = new StreamlineSource(
                    new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]), new Vector3D(0, 0, 1))
                {
                    ConduitLength = 40.0
                };
                StreamlineGeneratorOptions settings = new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = 1.0,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = Ground,
                        FloorVertical = targetAt[2] + 40.0
                    },
                    StreamlineCount = 36,
                    ChannelContrast = channelled ? 100.0 : 1.0,
                    ChannelNarrowWidth = 25.0,
                    ChannelWideWidth = 150.0,
                    OutletGuideContrast = channelled ? 20.0 : 1.0,
                    OutletGuideLength = 120.0,
                    OutletGuideWidth = 30.0,
                    LaunchInset = LaunchInset,
                    Relaxer = new SpineRelaxerOptions { HoldLength = 40.0 }
                };
                StreamlineGenerationResult got = channelled
                    ? StreamlineGenerator.GenerateChannelled(field, new[] { slot },
                          new TargetPolygon(corners, 20.0), settings, 4)
                    : StreamlineGenerator.Generate(field, new[] { slot },
                          new TargetPolygon(corners, 20.0), settings);
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
                string sizes = string.Join("/", bundling.Bundles.Select(b => b.Count));
                TestContext.Progress.WriteLine(
                    (channelled ? "channelled, 4 spines asked for: " : "no channel, plain dipole: ")
                    + $"{bundling.Bundles.Count} bundles [{sizes}] from {arrived.Count} candidates, "
                    + $"{bundling.EffectiveCrossSectionCount} effective cross-sections, "
                    + $"{bundling.AdjacentPairCount} adjacent pairs, "
                    + $"{got.Spines.Count} spines used");
            }
        }

        private static void WriteScene(ObstacleField field, StreamlineGenerationResult result,
                                       List<StreamlineBundlingResult> runs, List<int[]> bundleOfRun,
                                       double[] origin, double[] slotAt, double[] targetAt,
                                       List<Point3D> corners)
        {
            StringBuilder json = new StringBuilder();
            json.Append("{\"origin\":[").Append(Num(origin[0])).Append(',').Append(Num(origin[1]))
                .Append(",0],");
            json.Append("\"ground\":").Append(Num(Ground)).Append(',');
            json.Append("\"slot\":").Append(Point(slotAt, origin)).Append(',');
            AppendWells(json, field, origin);
            json.Append(",\"cases\":[");
            for (int r = 0; r < runs.Count; r++)
            {
                if (r > 0) { json.Append(','); }
                AppendCase(json, result, runs[r], bundleOfRun[r],
                           r == 0 ? "Connected" : "Separated", origin, slotAt, targetAt, corners);
            }
            json.Append("],\"routes\":[]}");

            string output = Path.Combine(Path.GetTempPath(), "ullrigg-bundle-scene.json");
            File.WriteAllText(output, json.ToString());
            TestContext.Progress.WriteLine($"wrote {new FileInfo(output).Length / 1024} kB to {output}");
        }

        private static void AppendCase(StringBuilder json, StreamlineGenerationResult result,
                                       StreamlineBundlingResult bundling, int[] bundleOf, string name,
                                       double[] origin, double[] slotAt, double[] targetAt,
                                       List<Point3D> corners)
        {
            json.Append("{\"name\":\"").Append(name).Append('"');
            json.Append(",\"conduit\":40,\"landing\":0,\"shaped\":true,\"outlet\":20,\"ceiling\":true");
            json.Append(",\"bundleCount\":").Append(bundling.Bundles.Count);
            json.Append(",\"bundles\":[");
            for (int s = 0; s < bundleOf.Length; s++)
            {
                if (s > 0) { json.Append(','); }
                json.Append(bundleOf[s]);
            }
            json.Append(']');

            List<Point3D> spine = result.Spines.Count > 0 && result.Spines[0].Count > 2
                ? result.Spines[0]
                : ReferenceSpine.Sample(
                      new Point3D(slotAt[0], slotAt[1], slotAt[2]), new Vector3D(0, 0, 1),
                      new Point3D(targetAt[0], targetAt[1], targetAt[2]), new Vector3D(0, 0, 1));
            json.Append(",\"spineDogleg\":").Append(
                (StreamlineCurvature.GetWorst(new Streamline(spine)) * 180.0 / System.Math.PI)
                .ToString("G4", Invariant));
            json.Append(",\"spine\":[");
            int spineStride = System.Math.Max(1, spine.Count / 130);
            for (int i = 0; i < spine.Count; i += spineStride)
            {
                if (i > 0) { json.Append(','); }
                json.Append(Point(new[] { spine[i].X!.Value, spine[i].Y!.Value, spine[i].Z!.Value },
                                  origin));
            }
            json.Append(']');
            json.Append(",\"unknowns\":").Append(result.Field?.UnknownCount ?? 0);
            json.Append(",\"iterations\":").Append(result.Field?.IterationCount ?? 0);
            json.Append(",\"residual\":")
                .Append((result.Field?.Residual ?? 0).ToString("G4", Invariant));
            json.Append(",\"imbalance\":")
                .Append((result.Field?.WorstCellImbalance ?? 0).ToString("G4", Invariant));
            json.Append(",\"arrived\":").Append(result.ArrivedCount);
            json.Append(",\"served\":").Append(result.ServedTargetCellCount);
            json.Append(",\"target\":[");
            for (int c = 0; c < corners.Count; c++)
            {
                if (c > 0) { json.Append(','); }
                json.Append(Point(new[] { corners[c].X!.Value, corners[c].Y!.Value, corners[c].Z!.Value },
                                  origin));
            }
            json.Append("],\"conduits\":[");
            for (int c = 0; c < result.ConduitPaths.Count; c++)
            {
                if (c > 0) { json.Append(','); }
                json.Append('[');
                List<Point3D> run = result.ConduitPaths[c];
                for (int i = 0; i < run.Count; i++)
                {
                    if (i > 0) { json.Append(','); }
                    json.Append(Point(new[] { run[i].X!.Value, run[i].Y!.Value, run[i].Z!.Value },
                                      origin));
                }
                json.Append(']');
            }
            json.Append("],\"doglegs\":[");
            for (int t = 0; t < result.Streamlines.Count; t++)
            {
                if (t > 0) { json.Append(','); }
                bool arrived = t >= result.StreamlineOutcomes.Count
                               || result.StreamlineOutcomes[t] == TraceOutcome.ReachedSink;
                json.Append(arrived
                    ? (StreamlineCurvature.GetWorst(result.Streamlines[t])
                       * 180.0 / System.Math.PI).ToString("G4", Invariant)
                    : "-1");
            }
            json.Append("],\"streamlines\":[");
            for (int s = 0; s < result.Streamlines.Count; s++)
            {
                List<Point3D> positions = result.Streamlines[s].Positions!;
                int stride = System.Math.Max(1, positions.Count / 260);
                if (s > 0) { json.Append(','); }
                json.Append('[');
                bool firstPoint = true;
                for (int i = 0; i < positions.Count; i += stride)
                {
                    if (!firstPoint) { json.Append(','); }
                    firstPoint = false;
                    json.Append(Point(new[] { positions[i].X!.Value, positions[i].Y!.Value,
                                              positions[i].Z!.Value }, origin));
                }
                if ((positions.Count - 1) % stride != 0)
                {
                    Point3D last = positions[positions.Count - 1];
                    json.Append(',')
                        .Append(Point(new[] { last.X!.Value, last.Y!.Value, last.Z!.Value }, origin));
                }
                json.Append(']');
            }
            json.Append("],\"summary\":\"").Append(result.Describe().Replace("\"", "'")).Append("\"}");
        }

        /// <summary>
        /// the uncertainty volumes, as closed rings round each section
        /// </summary>
        private static void AppendWells(StringBuilder json, ObstacleField field, double[] origin)
        {
            const int aroundCount = 14;
            json.Append("\"wells\":[");
            for (int w = 0; w < field.Wells.Count; w++)
            {
                WellboreUncertainty well = field.Wells[w];
                int stride = System.Math.Max(1, well.SampleCount / 90);
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
