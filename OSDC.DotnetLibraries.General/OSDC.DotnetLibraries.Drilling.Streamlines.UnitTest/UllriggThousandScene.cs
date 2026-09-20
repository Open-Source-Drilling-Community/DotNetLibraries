using System.Diagnostics;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// A thousand launches at the default contrast: what it costs and what it finds.
    /// <para>
    /// The generation is timed three times rather than once. A single sample per point is what made the
    /// first cost measurement report the same work as taking 1552 ms and 181 ms, and made it look as
    /// though the launch count cost nothing at all.
    /// </para>
    /// </summary>
    [TestFixture]
    public class UllriggThousandScene
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private const double Ground = -91.2;
        private const double TargetVertical = 900.0;
        private const double TargetHalf = 80.0;
        private const int Launches = 1000;

        // a thousand paths at the stride used for 36 would be a scene file of several megabytes, and
        // nothing is read off a corridor at that resolution that is not read off it at this one
        private const int PositionsPerPath = 120;

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
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~AThousandLaunches"</c>.
        /// </summary>
        [Test]
        [Explicit("three full generations of a thousand paths, writes a scene file for display")]
        public void AThousandLaunches()
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

            TestContext.Progress.WriteLine(
                $"{Environment.ProcessorCount} logical processors, {Launches} launches, contrast 3.0, "
                + "Separated");

            List<double> generating = new List<double>();
            StreamlineGenerationResult result = new StreamlineGenerationResult();
            for (int repeat = 0; repeat < 3; repeat++)
            {
                StreamlineSource slot = new StreamlineSource(
                    new Point3D(slotAt[0], slotAt[1], slotAt[2]), new Vector3D(0, 0, 1))
                {
                    ConduitLength = 40.0
                };
                Stopwatch clock = Stopwatch.StartNew();
                result = StreamlineGenerator.GenerateChannelled(
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
                clock.Stop();
                generating.Add(clock.Elapsed.TotalSeconds);
            }
            generating.Sort();
            TestContext.Progress.WriteLine("generation: " + result.Describe());

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

            StreamlineBundlingOptions settings = new StreamlineBundlingOptions
            {
                ContrastRatio = 3.0,
                PartitionRule = BundlePartitionRule.Separated
            };
            for (int warm = 0; warm < 2; warm++)
            {
                new StreamlineBundler(settings).Bundle(bundleInput);
            }
            List<double> bundling = new List<double>();
            StreamlineBundlingResult bundled = new StreamlineBundlingResult();
            for (int repeat = 0; repeat < 7; repeat++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Stopwatch clock = Stopwatch.StartNew();
                bundled = new StreamlineBundler(settings).Bundle(bundleInput);
                clock.Stop();
                bundling.Add(clock.Elapsed.TotalMilliseconds);
            }
            bundling.Sort();

            long positions = 0;
            foreach (Streamline line in bundleInput) { positions += line.Positions!.Count; }
            TestContext.Progress.WriteLine(
                $"  generation: median {generating[1]:0.00} s ({generating[0]:0.00} to"
                + $" {generating[2]:0.00}), {candidates.Count} arrived of {Launches},"
                + $" {positions} positions");
            TestContext.Progress.WriteLine(
                $"  bundling:   median {bundling[3]:0.0} ms, quartiles {bundling[1]:0.0} and"
                + $" {bundling[5]:0.0}, range {bundling[0]:0.0} to {bundling[6]:0.0},"
                + $" {bundled.CrossingCount} crossings");
            TestContext.Progress.WriteLine(
                $"  bundles:    {bundled.Bundles.Count} ["
                + string.Join("/", bundled.Bundles.Select(b => b.Count).Take(12))
                + (bundled.Bundles.Count > 12 ? "/…" : "") + "], "
                + $"{bundled.AdjacentPairCount} adjacent pairs, {bundled.SeparatedPairCount} ever apart, "
                + $"{bundled.RefusedLinkCount} links refused");
            foreach (StreamlineBundle bundle in bundled.Bundles)
            {
                double gentlest = double.MaxValue;
                foreach (int at in bundle.StreamlineIndices)
                {
                    double worst = StreamlineCurvature.GetWorst(bundleInput[at]) * 180.0 / System.Math.PI;
                    if (worst > 0 && worst < gentlest) { gentlest = worst; }
                }
                if (bundle.Count < 3) { continue; }
                TestContext.Progress.WriteLine(
                    $"    bundle {bundle.Index}: {bundle.Count,4} paths, gentlest {gentlest:0.0} deg/30 m");
            }

            int[] bundleOf = new int[result.Streamlines.Count];
            for (int s = 0; s < bundleOf.Length; s++) { bundleOf[s] = -1; }
            for (int k = 0; k < candidates.Count; k++)
            {
                bundleOf[candidates[k]] = bundled.BundleIndexOfStreamline[k];
            }
            WriteScene(field, result, bundled, bundleOf, origin, slotAt, targetAt, corners);
        }

        private static void WriteScene(ObstacleField field, StreamlineGenerationResult result,
                                       StreamlineBundlingResult bundled, int[] bundleOf,
                                       double[] origin, double[] slotAt, double[] targetAt,
                                       List<Point3D> corners)
        {
            StringBuilder json = new StringBuilder();
            json.Append("{\"origin\":[").Append(Num(origin[0])).Append(',').Append(Num(origin[1]))
                .Append(",0],");
            json.Append("\"ground\":").Append(Num(Ground)).Append(',');
            json.Append("\"slot\":").Append(Point(slotAt, origin)).Append(',');
            AppendWells(json, field, origin);
            json.Append(",\"cases\":[{\"name\":\"1000 launches\"");
            json.Append(",\"conduit\":40,\"landing\":0,\"shaped\":true,\"outlet\":20,\"ceiling\":true");
            json.Append(",\"bundleCount\":").Append(bundled.Bundles.Count);
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
                int stride = System.Math.Max(1, positions.Count / PositionsPerPath);
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
            json.Append("],\"routes\":[]}");

            string output = Path.Combine(Path.GetTempPath(), "ullrigg-thousand-scene.json");
            File.WriteAllText(output, json.ToString());
            TestContext.Progress.WriteLine($"wrote {new FileInfo(output).Length / 1024} kB to {output}");
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
