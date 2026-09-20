using System.Globalization;
using System.Text;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// Writes the Ullrigg case out as JSON so it can be looked at, unconstrained and constrained, so that
    /// the two can be put side by side.
    /// <para>
    /// Positions are shifted to a local origin at the cluster before being written. The Riemannian
    /// coordinates run to six and a half million metres, and anything drawing them in single precision
    /// would quantise a half metre cell into nothing.
    /// </para>
    /// </summary>
    [TestFixture]
    public class UllriggSceneExport
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private const double Ground = -91.2;

        [Test]
        [Explicit("writes a scene file for display")]
        public void WriteScene()
        {
            string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "UllriggUncertainty");
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
            AppendCase(json, field, origin, slotAt, 1);
            json.Append(',');
            AppendCase(json, field, origin, slotAt, 2);
            json.Append(',');
            AppendCase(json, field, origin, slotAt, 3);
            json.Append(',');
            AppendCase(json, field, origin, slotAt, 4);
            json.Append(']');
            AppendRoutes(json, field, origin, slotAt);
            json.Append('}');

            string output = Path.Combine(
                @"C:\Users\erca\AppData\Local\Temp\claude\C--OSDC-DotNetLibraries\c110bcbd-932b-4da8-94dd-5ef680692095\scratchpad",
                "ullrigg-scene.json");
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            File.WriteAllText(output, json.ToString());
            TestContext.Progress.WriteLine($"wrote {new FileInfo(output).Length / 1024} kB to {output}");
        }

        /// <summary>
        /// The distinct ways round, found by relaxing a curve clear of the wells and then forbidding it
        /// so the next relaxation has nowhere to collapse back to. No flow is solved for these.
        /// </summary>
        private static void AppendRoutes(StringBuilder json, ObstacleField field, double[] origin,
                                         double[] slotAt)
        {
            Point3D slot = new Point3D(slotAt[0], slotAt[1], slotAt[2]);
            Point3D target = new Point3D(origin[0] - 400.0, origin[1] + 400.0, 900.0);
            Vector3D down = new Vector3D(0, 0, 1);
            SpineRelaxerOptions settings = new SpineRelaxerOptions
            {
                CeilingVertical = Ground,
                FloorVertical = 940.0,
                HoldLength = 40.0
            };

            // Only the best one is drawn. The keep-out machinery will produce as many alternatives as
            // asked for, but on this geometry they cost three hundred metres of extra hole and two to
            // three times the curvature, so showing them alongside the one route worth drilling says
            // more about the search than about the problem.
            const int routeCount = 1;
            json.Append(",\"routes\":[");
            for (int round = 0; round < routeCount; round++)
            {
                double pushNorth = 0, pushEast = 0;
                foreach (KeepOutTube had in settings.KeepOut)
                {
                    Point3D middle = had.Path![had.Path.Count / 2];
                    pushNorth -= middle.X!.Value;
                    pushEast -= middle.Y!.Value;
                }
                if (settings.KeepOut.Count > 0)
                {
                    pushNorth = pushNorth / settings.KeepOut.Count
                                + (slot.X!.Value + target.X!.Value) / 2;
                    pushEast = pushEast / settings.KeepOut.Count
                               + (slot.Y!.Value + target.Y!.Value) / 2;
                    double length = System.Math.Sqrt(pushNorth * pushNorth + pushEast * pushEast);
                    if (length > 0)
                    {
                        pushNorth *= 400.0 / length;
                        pushEast *= 400.0 / length;
                    }
                }
                List<Point3D> seed = settings.KeepOut.Count == 0
                    ? ReferenceSpine.Sample(slot, down, target, down, 129)
                    : ReferenceSpine.Nudged(slot, down, target, down, pushNorth, pushEast, 0, 129);

                SpineRelaxerResult got = SpineRelaxer.Relax(seed, field, down, down, settings);
                TestContext.Progress.WriteLine($"route {round}: " + got.Describe());
                if (round > 0) { json.Append(','); }
                json.Append("{\"dogleg\":")
                    .Append((got.WorstDogleg * 180.0 / System.Math.PI).ToString("G4", Invariant));
                json.Append(",\"clearance\":").Append(got.LeastClearance.ToString("G4", Invariant));
                json.Append(",\"length\":").Append(Num(got.Length));
                json.Append(",\"points\":[");
                for (int i = 0; i < got.Spine.Count; i++)
                {
                    if (i > 0) { json.Append(','); }
                    json.Append(Point(new[] { got.Spine[i].X!.Value, got.Spine[i].Y!.Value,
                                              got.Spine[i].Z!.Value }, origin));
                }
                json.Append("]}");
                settings.KeepOut.Add(new KeepOutTube { Path = got.Spine, Radius = 150.0 });
            }
            json.Append(']');
        }

        /// <summary>
        /// One generation written out whole. Stage 0 is the bare problem, stage 1 adds the ground
        /// overhead and a floor under the target, stage 2 adds a walled landing on the target normal.
        /// </summary>
        private static void AppendCase(StringBuilder json, ObstacleField field, double[] origin,
                                       double[] slotAt, int stage)
        {
            double[] targetAt = { origin[0] - 400.0, origin[1] + 400.0, 900.0 };
            const double half = 80.0;
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(targetAt[0] - half, targetAt[1] - half, targetAt[2]),
                new Point3D(targetAt[0] + half, targetAt[1] - half, targetAt[2]),
                new Point3D(targetAt[0] + half, targetAt[1] + half, targetAt[2]),
                new Point3D(targetAt[0] - half, targetAt[1] + half, targetAt[2])
            };
            TargetPolygon target = new TargetPolygon(corners, 20.0);
            StreamlineSource slot = new StreamlineSource(new Point3D(slotAt[0], slotAt[1], slotAt[2]),
                                                         new Vector3D(0, 0, 1));
            StreamlineGridOptions grid = new StreamlineGridOptions
            {
                FinestCellSize = 1.0,
                CoarsestCellSize = 25.0
            };
            if (stage >= 1)
            {
                slot.ConduitLength = 100.0;
                grid.CeilingVertical = Ground;
                grid.FloorVertical = targetAt[2] + 40.0;
            }
            if (stage >= 2)
            {
                target.Incidence = TargetIncidence.Perpendicular;
                target.Sides = TargetSides.One;
                target.ApproachDirection = new Vector3D(0, 0, 1);
                target.LandingLength = 300.0;
            }
            if (stage >= 3)
            {
                // no walls at the target at all: the medium is made easier along a spine that has been
                // pushed clear of the wells, and the flow is left to follow it or not
                slot.ConduitLength = 200.0;
                target.Incidence = TargetIncidence.Free;
                target.Sides = TargetSides.Both;
            }
            if (stage >= 4)
            {
                // Ullrigg is hard from surface, so the well only has to stay vertical until the bottom
                // hole assembly is in the hole
                slot.ConduitLength = 40.0;
            }

            StreamlineGeneratorOptions settings = new StreamlineGeneratorOptions
            {
                Grid = grid,
                StreamlineCount = 36,
                ConduitLength = 30.0,
                ChannelContrast = stage >= 3 ? 100.0 : 1.0,
                ChannelNarrowWidth = 25.0,
                ChannelWideWidth = 150.0,
                // measured optimum: twenty is enough to stop the fan at the mouth, and two hundred
                // overshoots, the guide's own edge becoming the corner it was meant to remove
                OutletGuideContrast = stage >= 3 ? 20.0 : 1.0,
                OutletGuideLength = 120.0,
                OutletGuideWidth = 30.0,
                Relaxer = new SpineRelaxerOptions
                {
                    HoldLength = stage >= 3 ? slot.ConduitLength ?? 0.0 : 0.0
                }
            };
            StreamlineGenerationResult result = stage >= 3
                ? StreamlineGenerator.GenerateChannelled(field, new[] { slot }, target, settings)
                : StreamlineGenerator.Generate(field, new[] { slot }, target, settings);
            string[] names = { "As first built", "Bounded", "Landed", "Kick-off 200 m",
                               "Kick-off 40 m" };
            TestContext.Progress.WriteLine(names[stage] + ": " + result.Describe());

            json.Append("{\"name\":\"").Append(names[stage]).Append('"');
            json.Append(",\"conduit\":").Append(Num(slot.ConduitLength ?? 30.0));
            // the length only means anything when the incidence actually uses it, and stage three sets
            // the arrival back to free after having set a landing length
            json.Append(",\"landing\":").Append(
                target.Incidence != TargetIncidence.Free ? Num(target.LandingLength) : "0");
            json.Append(",\"shaped\":").Append(stage >= 3 ? "true" : "false");
            json.Append(",\"outlet\":").Append(stage >= 3 ? "20" : "0");
            json.Append(",\"ceiling\":").Append(stage >= 1 ? "true" : "false");
            // the ideal path honouring both end tangents, and what it would cost to drill it, so the
            // corridor can be read against something rather than against nothing
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
            // the numbers are written out rather than parsed back off the description, which carries the
            // decimal separator of whatever machine produced it
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
                // minus one marks a path that never reached the target, so nothing downstream counts
                // it among the candidates
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
                // always keep the last position so the path reaches the target, but not twice over:
                // a repeat would leave the final segment with no length and no direction to read off it
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
