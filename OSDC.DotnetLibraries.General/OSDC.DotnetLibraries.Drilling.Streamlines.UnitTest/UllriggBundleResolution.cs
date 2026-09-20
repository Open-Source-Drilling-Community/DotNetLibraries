using System.Globalization;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// Why the Ullrigg corridor comes back as one bundle when the view plainly shows paths passing on
    /// either side of an uncertainty volume.
    /// <para>
    /// The bundling rule itself cannot be at fault: a pair is united only when it is never found
    /// separated, so had any cross-section split the fan, no pair across the split could have been
    /// united and the answer would have been two bundles or more. So the question is whether any
    /// cross-section splits it, which is a question about resolution. A group is cut where a gap exceeds
    /// <see cref="StreamlineBundlingOptions.ContrastRatio"/> times the <em>local spacing</em>, and with
    /// 34 paths spread over the whole fan that spacing is itself tens of metres. This sweeps the two
    /// things that would change it: how many paths are launched, and how large a gap has to be.
    /// </para>
    /// </summary>
    [TestFixture]
    public class UllriggBundleResolution
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

        private static List<Streamline> Generate(ObstacleField field, double[] u1, int launches)
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
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~WhyOneBundle"</c>.
        /// </summary>
        [Test]
        [Explicit("three full generations on the real data")]
        public void WhyOneBundle()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);
            TestContext.Progress.WriteLine(
                "launches | contrast | rule | bundles | sizes | adjacent pairs | ever separated | refused links");
            foreach (int launches in new[] { 36, 144, 400 })
            {
                List<Streamline> arrived = Generate(field, u1, launches);
                foreach (double contrast in new[] { 3.0, 2.0, 1.5 })
                {
                    foreach (BundlePartitionRule rule in new[] { BundlePartitionRule.Connected,
                                                                 BundlePartitionRule.Separated })
                    {
                        StreamlineBundlingResult bundling = new StreamlineBundler(
                            new StreamlineBundlingOptions
                            {
                                ContrastRatio = contrast,
                                PartitionRule = rule
                            })
                            .Bundle(arrived);
                        List<int> sizes = bundling.Bundles.Select(b => b.Count).ToList();
                        string shown = sizes.Count <= 8
                            ? string.Join("/", sizes)
                            : string.Join("/", sizes.Take(8)) + "/… (" + sizes.Count + " in all)";
                        TestContext.Progress.WriteLine(
                            $"  {arrived.Count,4} | {contrast,4:0.0} | {rule,-11} |"
                            + $" {bundling.Bundles.Count,5} | {shown}"
                            + $" | {bundling.AdjacentPairCount} | {bundling.SeparatedPairCount}"
                            + $" | {bundling.RefusedLinkCount}");
                    }
                }
            }
        }
    }
}
