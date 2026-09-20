using System.Globalization;
using System.Text;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// The Ullrigg scene written with one case per launch inset instead of one per stage, so that what
    /// the launch disc does to the arrivals can be looked at rather than read off a table. The file is
    /// the same shape the scene viewer already reads.
    /// </summary>
    [TestFixture]
    public class UllriggInsetScene
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private const double Ground = -91.2;
        private const double TargetVertical = 900.0;
        private const double TargetHalf = 80.0;

        [Test]
        [Explicit("four full generations, writes a scene file for display")]
        public void WriteInsetScene()
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

            double[] origin = heads["U1"];
            double[] slotAt = { origin[0] + 1.2, origin[1] + 1.2, origin[2] };

            StringBuilder json = new StringBuilder();
            json.Append("{\"origin\":[").Append(Num(origin[0])).Append(',').Append(Num(origin[1]))
                .Append(",0],");
            json.Append("\"ground\":").Append(Num(Ground)).Append(',');
            json.Append("\"slot\":").Append(Point(slotAt, origin)).Append(',');
            AppendWells(json, field, origin);
            json.Append(",\"cases\":[");
            double[] insets = { 0.00, 0.10, 0.25, 0.45 };
            for (int i = 0; i < insets.Length; i++)
            {
                if (i > 0) { json.Append(','); }
                AppendInsetCase(json, field, origin, slotAt, insets[i]);
            }
            json.Append("],\"routes\":[]}");

            string output = Path.Combine(Path.GetTempPath(), "ullrigg-inset-scene.json");
            File.WriteAllText(output, json.ToString());
            TestContext.Progress.WriteLine($"wrote {new FileInfo(output).Length / 1024} kB to {output}");
        }

        /// <summary>
        /// the best configuration so far, run at one launch inset and written out as a case
        /// </summary>
        private static void AppendInsetCase(StringBuilder json, ObstacleField field, double[] origin,
                                            double[] slotAt, double inset)
        {
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
                ChannelContrast = 100.0,
                ChannelNarrowWidth = 25.0,
                ChannelWideWidth = 150.0,
                OutletGuideContrast = 20.0,
                OutletGuideLength = 120.0,
                OutletGuideWidth = 30.0,
                LaunchInset = inset,
                Relaxer = new SpineRelaxerOptions { HoldLength = 40.0 }
            };
            StreamlineGenerationResult result = StreamlineGenerator.GenerateChannelled(
                field, new[] { slot }, target, settings);
            TestContext.Progress.WriteLine($"inset {inset:0.00}: " + result.Describe());

            json.Append("{\"name\":\"Inset ").Append(inset.ToString("0.00", Invariant)).Append('"');
            json.Append(",\"conduit\":40");
            json.Append(",\"landing\":0");
            json.Append(",\"shaped\":true");
            json.Append(",\"outlet\":20");
            json.Append(",\"ceiling\":true");
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
                // minus one marks a path that never reached the target
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
                // always keep the last position so the path reaches the target, but not twice over
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
