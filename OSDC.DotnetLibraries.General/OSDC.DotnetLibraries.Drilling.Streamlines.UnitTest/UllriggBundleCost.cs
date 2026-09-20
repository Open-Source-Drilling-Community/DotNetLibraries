using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// What the two candidate configurations cost, and where the cost sits.
    /// <para>
    /// The two knobs do not act on the same stage. The number of launches is paid twice in the
    /// generation, the scout pass tracing as many streamlines as the final one, and again in the
    /// bundler, which has that many more crossings to group. The contrast ratio costs nothing in the
    /// generation at all — it only changes which links the grouping makes. So the comparison has to be
    /// made per stage rather than on one end-to-end number.
    /// </para>
    /// <para>
    /// The generation is timed at three launch counts so the fixed part can be told from the part that
    /// scales: the flow solve is the same problem whatever is launched through it.
    /// </para>
    /// </summary>
    [TestFixture]
    public class UllriggBundleCost
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

        private static StreamlineGenerationResult Generate(ObstacleField field, double[] u1, int launches)
        {
            double[] targetAt = { u1[0] - 400.0, u1[1] + 400.0, TargetVertical };
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(targetAt[0] - TargetHalf, targetAt[1] - TargetHalf, targetAt[2]),
                new Point3D(targetAt[0] + TargetHalf, targetAt[1] - TargetHalf, targetAt[2]),
                new Point3D(targetAt[0] + TargetHalf, targetAt[1] + TargetHalf, targetAt[2]),
                new Point3D(targetAt[0] - TargetHalf, targetAt[1] + TargetHalf, targetAt[2])
            };
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = 40.0
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
                        FloorVertical = targetAt[2] + 40.0
                    },
                    StreamlineCount = launches,
                    ChannelContrast = 100.0,
                    ChannelNarrowWidth = 25.0,
                    ChannelWideWidth = 150.0,
                    OutletGuideContrast = 20.0,
                    OutletGuideLength = 120.0,
                    OutletGuideWidth = 30.0,
                    LaunchInset = 0.25,
                    Relaxer = new SpineRelaxerOptions { HoldLength = 40.0 }
                }, 1);
        }

        private static List<Streamline> Arrived(StreamlineGenerationResult got)
        {
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
            return arrived;
        }

        /// <summary>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~WhatTheConfigurationsCost"</c>.
        /// </summary>
        [Test]
        [Explicit("three full generations on the real data, timed")]
        public void WhatTheConfigurationsCost()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);
            TestContext.Progress.WriteLine(
                $"{Environment.ProcessorCount} logical processors, "
                + (Debugger.IsAttached ? "debugger attached" : "no debugger")
                + ", built " + (IsOptimised() ? "optimised" : "DEBUG — absolute times are not release times"));

            TestContext.Progress.WriteLine("generation, wall seconds, three runs each:");
            Dictionary<int, List<Streamline>> produced = new Dictionary<int, List<Streamline>>();
            foreach (int launches in new[] { 4, 144, 400 })
            {
                List<double> runs = new List<double>();
                List<Streamline> arrived = new List<Streamline>();
                for (int repeat = 0; repeat < 3; repeat++)
                {
                    Stopwatch clock = Stopwatch.StartNew();
                    StreamlineGenerationResult got = Generate(field, u1, launches);
                    clock.Stop();
                    runs.Add(clock.Elapsed.TotalSeconds);
                    arrived = Arrived(got);
                }
                long positions = 0;
                foreach (Streamline line in arrived) { positions += line.Positions!.Count; }
                produced[launches] = arrived;
                runs.Sort();
                TestContext.Progress.WriteLine(
                    $"  {launches,4} asked, {arrived.Count,4} arrived: median {runs[1],6:0.00} s"
                    + $" ({runs[0]:0.00} to {runs[2]:0.00}), {positions} positions in all");
            }

            // Two runs are thrown away before any are kept. The first call through a code path pays for
            // the jitting of it, and the first large allocation pays for growing the heap; neither is a
            // cost the caller meets twice, so counting them would overstate the work by an order of
            // magnitude. The earlier attempt at this did count them and reported the same 220530
            // crossings as costing 1552 ms at one contrast and 181 ms at another.
            TestContext.Progress.WriteLine(
                "bundling, wall milliseconds, median of seven after two warm-ups:");
            foreach (int launches in new[] { 144, 400 })
            {
                foreach (double contrast in new[] { 3.0, 2.0 })
                {
                    StreamlineBundlingOptions settings = new StreamlineBundlingOptions
                    {
                        ContrastRatio = contrast,
                        PartitionRule = BundlePartitionRule.Separated
                    };
                    for (int warm = 0; warm < 2; warm++)
                    {
                        new StreamlineBundler(settings).Bundle(produced[launches]);
                    }
                    List<double> runs = new List<double>();
                    StreamlineBundlingResult last = new StreamlineBundlingResult();
                    for (int repeat = 0; repeat < 7; repeat++)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        Stopwatch clock = Stopwatch.StartNew();
                        last = new StreamlineBundler(settings).Bundle(produced[launches]);
                        clock.Stop();
                        runs.Add(clock.Elapsed.TotalMilliseconds);
                    }
                    runs.Sort();
                    TestContext.Progress.WriteLine(
                        $"  {produced[launches].Count,4} paths, contrast {contrast:0.0}:"
                        + $" median {runs[3],7:0.0} ms, quartiles {runs[1]:0.0} and {runs[5]:0.0},"
                        + $" range {runs[0]:0.0} to {runs[6]:0.0} | {last.CrossingCount} crossings, "
                        + $"{last.Bundles.Count} bundles, {last.RefusedLinkCount} refused");
                }
            }
        }

        private static bool IsOptimised()
        {
            object[] found = typeof(StreamlineBundler).Assembly.GetCustomAttributes(
                typeof(System.Diagnostics.DebuggableAttribute), false);
            if (found.Length == 0) { return true; }
            System.Diagnostics.DebuggableAttribute debuggable
                = (System.Diagnostics.DebuggableAttribute)found[0];
            return !debuggable.IsJITOptimizerDisabled;
        }
    }
}
