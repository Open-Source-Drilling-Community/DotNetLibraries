using System.Globalization;
using System.Text;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// Where on the target the streamlines actually land.
    /// <para>
    /// Every sink cell draws the same rate, so the flux arriving is uniform over the target by
    /// construction. If the arrivals are not, either the launches do not carry equal shares of the source
    /// flux, or they do not sample all of it. This measures which.
    /// </para>
    /// </summary>
    [TestFixture]
    public class UllriggArrivalSpread
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private const double Ground = -91.2;
        private const double TargetVertical = 900.0;
        private const double TargetHalf = 80.0;

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
        /// the best configuration so far, with the launch disc and the number of launches left open
        /// </summary>
        private static StreamlineGenerationResult RunBest(ObstacleField field, double[] u1,
                                                          double inset, int launches,
                                                          double conduit = 40.0)
        {
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(u1[0] - 400.0 - TargetHalf, u1[1] + 400.0 - TargetHalf, TargetVertical),
                new Point3D(u1[0] - 400.0 + TargetHalf, u1[1] + 400.0 - TargetHalf, TargetVertical),
                new Point3D(u1[0] - 400.0 + TargetHalf, u1[1] + 400.0 + TargetHalf, TargetVertical),
                new Point3D(u1[0] - 400.0 - TargetHalf, u1[1] + 400.0 + TargetHalf, TargetVertical)
            };
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = conduit
            };
            return StreamlineGenerator.GenerateChannelled(
                field, new[] { slot }, new TargetPolygon(corners, 20.0),
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = 1.0,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = Ground,
                        FloorVertical = TargetVertical + 40.0
                    },
                    StreamlineCount = launches,
                    ChannelContrast = 100.0,
                    ChannelNarrowWidth = 25.0,
                    ChannelWideWidth = 150.0,
                    OutletGuideContrast = 20.0,
                    OutletGuideLength = 120.0,
                    OutletGuideWidth = 30.0,
                    LaunchInset = inset,
                    Relaxer = new SpineRelaxerOptions { HoldLength = conduit }
                }, 1);
        }

        /// <summary>
        /// What fraction of the target the arrivals cover, and where the patch sits.
        /// <para>
        /// Coverage is counted in sink cells rather than in area because a sink cell is the unit of equal
        /// rate: with every cell drawing the same share, the fraction of cells reached is the fraction of
        /// the total flux the launches sampled.
        /// </para>
        /// </summary>
        private static string DescribeArrivals(StreamlineGenerationResult result, double[] u1)
        {
            return DescribeArrivals(result, u1, out _, out _, out _);
        }

        /// <summary>
        /// the same, also handing back the three numbers a table wants
        /// </summary>
        private static string DescribeArrivals(StreamlineGenerationResult result, double[] u1,
                                               out int arrivedCount, out double columnFraction,
                                               out double boxFraction)
        {
            StreamlineGrid grid = result.Grid!;
            double centreNorth = u1[0] - 400.0;
            double centreEast = u1[1] + 400.0;

            // the columns of the target prism, one entry per distinct cell footprint
            HashSet<long> servedColumns = new HashSet<long>();
            foreach (int sink in grid.SinkLeaves)
            {
                OctreeCell cell = grid.Tree.GetCell(sink);
                servedColumns.Add(Column(cell));
            }

            HashSet<int> hitCells = new HashSet<int>();
            HashSet<long> hitColumns = new HashSet<long>();
            List<double[]> arrivals = new List<double[]>();
            for (int s = 0; s < result.Streamlines.Count; s++)
            {
                if (s < result.StreamlineOutcomes.Count
                    && result.StreamlineOutcomes[s] != TraceOutcome.ReachedSink)
                {
                    continue;
                }
                List<Point3D> positions = result.Streamlines[s].Positions!;
                Point3D last = positions[positions.Count - 1];
                arrivals.Add(new[] { last.X!.Value - centreNorth, last.Y!.Value - centreEast });
                int leaf = grid.Tree.FindLeaf(last.X!.Value, last.Y!.Value, last.Z!.Value);
                if (leaf >= 0)
                {
                    hitCells.Add(leaf);
                    hitColumns.Add(Column(grid.Tree.GetCell(leaf)));
                }
            }

            int[] quadrant = new int[4];
            double lowNorth = double.MaxValue, highNorth = double.MinValue;
            double lowEast = double.MaxValue, highEast = double.MinValue;
            foreach (double[] at in arrivals)
            {
                quadrant[(at[0] >= 0 ? 0 : 2) + (at[1] >= 0 ? 0 : 1)]++;
                lowNorth = System.Math.Min(lowNorth, at[0]);
                highNorth = System.Math.Max(highNorth, at[0]);
                lowEast = System.Math.Min(lowEast, at[1]);
                highEast = System.Math.Max(highEast, at[1]);
            }

            arrivedCount = arrivals.Count;
            columnFraction = (double)hitColumns.Count / System.Math.Max(1, servedColumns.Count);
            boxFraction = arrivals.Count > 0
                ? (highNorth - lowNorth) * (highEast - lowEast) / (4 * TargetHalf * TargetHalf)
                : 0.0;

            StringBuilder said = new StringBuilder();
            said.Append(arrivals.Count).Append(" arrived, ");
            said.Append(hitColumns.Count).Append('/').Append(servedColumns.Count)
                .Append(" target columns reached (")
                .Append((100.0 * hitColumns.Count / System.Math.Max(1, servedColumns.Count))
                        .ToString("0.0", Invariant)).Append("%), ");
            said.Append(hitCells.Count).Append('/').Append(grid.SinkLeaves.Count).Append(" sink cells, ");
            if (arrivals.Count > 0)
            {
                said.Append("box ").Append((highNorth - lowNorth).ToString("0.0", Invariant))
                    .Append(" x ").Append((highEast - lowEast).ToString("0.0", Invariant))
                    .Append(" m of ").Append((2 * TargetHalf).ToString("0", Invariant)).Append(" x ")
                    .Append((2 * TargetHalf).ToString("0", Invariant))
                    .Append(" (").Append((100.0 * (highNorth - lowNorth) * (highEast - lowEast)
                                          / (4 * TargetHalf * TargetHalf)).ToString("0.0", Invariant))
                    .Append("% of area), ");
                said.Append("north ").Append(lowNorth.ToString("0.0", Invariant)).Append(" to ")
                    .Append(highNorth.ToString("0.0", Invariant)).Append(", east ")
                    .Append(lowEast.ToString("0.0", Invariant)).Append(" to ")
                    .Append(highEast.ToString("0.0", Invariant)).Append(", ");
                said.Append("quadrants ").Append(quadrant[0]).Append('/').Append(quadrant[1])
                    .Append('/').Append(quadrant[2]).Append('/').Append(quadrant[3]);
            }
            return said.ToString();
        }

        /// <summary>
        /// a key for the footprint of a cell, so that cells stacked through the thickness of the target
        /// count once
        /// </summary>
        private static long Column(in OctreeCell cell)
        {
            return (long)System.Math.Round(cell.CentreNorth * 4.0) * 100000000L
                   + (long)System.Math.Round(cell.CentreEast * 4.0);
        }

        /// <summary>
        /// the value at the given quantile of a list, which is sorted in place
        /// </summary>
        private static double Quantile(List<double> values, double fraction)
        {
            if (values.Count == 0) { return double.NaN; }
            values.Sort();
            int index = (int)System.Math.Round(fraction * (values.Count - 1));
            return values[System.Math.Max(0, System.Math.Min(values.Count - 1, index))];
        }

        /// <summary>
        /// What the corridor costs to drill, over two station lengths.
        /// <para>
        /// Both are normalised to thirty metres. Real curvature reads the same however long the window; a
        /// corner is a fixed angle and so falls as one over the window, which is how the two columns tell
        /// them apart.
        /// </para>
        /// </summary>
        private static string DescribeCurvature(StreamlineGenerationResult result, double station)
        {
            List<double> worst = new List<double>();
            for (int s = 0; s < result.Streamlines.Count; s++)
            {
                if (s < result.StreamlineOutcomes.Count
                    && result.StreamlineOutcomes[s] != TraceOutcome.ReachedSink)
                {
                    continue;
                }
                double turn = StreamlineCurvature.GetWorst(result.Streamlines[s], station);
                worst.Add(turn * 180.0 / System.Math.PI * StreamlineCurvature.StandardStation / station);
            }
            if (worst.Count == 0) { return "none arrived"; }
            return Quantile(worst, 0).ToString("0.0", Invariant) + " / "
                   + Quantile(worst, 0.5).ToString("0.0", Invariant) + " / "
                   + Quantile(worst, 0.9).ToString("0.0", Invariant);
        }

        /// <summary>
        /// how close the paths come to an uncertainty volume, sampled at the stations a survey would
        /// report: the least over each path, then the worst and the median of those
        /// </summary>
        private static string DescribeClearance(StreamlineGenerationResult result, ObstacleField field)
        {
            List<double> least = new List<double>();
            for (int s = 0; s < result.Streamlines.Count; s++)
            {
                if (s < result.StreamlineOutcomes.Count
                    && result.StreamlineOutcomes[s] != TraceOutcome.ReachedSink)
                {
                    continue;
                }
                double here = double.MaxValue;
                foreach (Point3D at in StreamlineCurvature.Resample(result.Streamlines[s]))
                {
                    ObstacleProximity near = field.GetProximity(at.X!.Value, at.Y!.Value, at.Z!.Value,
                                                                60.0);
                    if (near.HasObstacle)
                    {
                        here = System.Math.Min(here, near.Clearance);
                    }
                }
                if (here < double.MaxValue) { least.Add(here); }
            }
            if (least.Count == 0) { return "no volume within 60 m"; }
            return Quantile(least, 0).ToString("0.0", Invariant) + " / "
                   + Quantile(least, 0.5).ToString("0.0", Invariant);
        }

        /// <summary>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~TheArrivalSpread"</c>.
        /// </summary>
        [Test]
        [Explicit("several full generations on the real data")]
        public void TheArrivalSpread()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);

            TestContext.Progress.WriteLine("launch disc: the fraction of the outlet face sampled");
            foreach (double inset in new[] { 0.25, 0.15, 0.05, 0.0 })
            {
                double fraction = System.Math.PI * (0.5 - inset) * (0.5 - inset);
                StreamlineGenerationResult got = RunBest(field, u1, inset, 36);
                TestContext.Progress.WriteLine(
                    $"  inset {inset:0.00} samples {100.0 * fraction:0.0}% of the face: "
                    + DescribeArrivals(got, u1));
            }

            TestContext.Progress.WriteLine("launch count at inset 0.25: does more filling widen the patch?");
            foreach (int launches in new[] { 36, 144 })
            {
                StreamlineGenerationResult got = RunBest(field, u1, 0.25, launches);
                TestContext.Progress.WriteLine($"  {launches} launches: " + DescribeArrivals(got, u1));
            }
        }

        /// <summary>
        /// The whole trade the launch disc makes, across its range: what the inset costs in target
        /// coverage and what it buys in curvature and clearance.
        /// <para>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~TheInsetSweep"</c>.
        /// </para>
        /// </summary>
        [Test]
        [Explicit("ten full generations on the real data")]
        public void TheInsetSweep()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);
            string output = Path.Combine(Path.GetTempPath(), "ullrigg-inset-sweep.tsv");
            using StreamWriter writer = new StreamWriter(output);
            writer.WriteLine("inset\tfaceSampled\tarrived\tcolumns\tboxArea\tgentle30\tmedian30\thigh30"
                             + "\tgentle120\tmedian120\thigh120\tworstClearance\tmedianClearance");

            TestContext.Progress.WriteLine(
                "inset | face % | arrived | columns | box % | dogleg deg/30 m at 30 m station"
                + " | at 120 m station | clearance m");
            TestContext.Progress.WriteLine(
                "      |        |         |         |       | gentle/median/p90"
                + "              | gentle/median/p90 | worst/median");
            foreach (double inset in new[] { 0.00, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30, 0.35, 0.40, 0.45 })
            {
                double fraction = System.Math.PI * (0.5 - inset) * (0.5 - inset);
                StreamlineGenerationResult got = RunBest(field, u1, inset, 36);
                string arrivals = DescribeArrivals(got, u1, out int arrived, out double columns,
                                                   out double box);
                string near = DescribeCurvature(got, 30.0);
                string far = DescribeCurvature(got, 120.0);
                string clear = DescribeClearance(got, field);
                TestContext.Progress.WriteLine(
                    $"  {inset:0.00} | {100.0 * fraction,5:0.0}  | " + arrivals
                    + " | " + near + " | " + far + " | " + clear);
                writer.WriteLine($"{inset.ToString("0.00", Invariant)}\t"
                                 + fraction.ToString("0.0000", Invariant) + "\t"
                                 + arrivals.Replace(", ", "\t") + "\t"
                                 + near.Replace(" / ", "\t") + "\t" + far.Replace(" / ", "\t") + "\t"
                                 + clear.Replace(" / ", "\t"));
            }
            TestContext.Progress.WriteLine("wrote " + output);
        }
    }
}
