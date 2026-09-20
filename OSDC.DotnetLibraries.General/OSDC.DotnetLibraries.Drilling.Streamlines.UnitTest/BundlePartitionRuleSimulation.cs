using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// What changes if a link is refused whenever taking it would put a pair that was seen apart into one
    /// bundle, rather than allowing the connected component to bridge past the separation.
    /// <para>
    /// Run on the fixtures the bundler is already asserted against, so that the consequence for the
    /// delivered behaviour is measured rather than reasoned about.
    /// </para>
    /// </summary>
    [TestFixture]
    public class BundlePartitionRuleSimulation
    {
        private static StreamlineBundlingResult Run(IReadOnlyList<Streamline> streamlines,
                                                    BundlePartitionRule rule)
        {
            return new StreamlineBundler(new StreamlineBundlingOptions { PartitionRule = rule })
                .Bundle(streamlines);
        }

        private static string Sizes(StreamlineBundlingResult result)
        {
            List<int> sizes = result.Bundles.Select(b => b.Count).ToList();
            return sizes.Count <= 6
                ? string.Join("/", sizes)
                : string.Join("/", sizes.Take(6)) + "/… (" + sizes.Count + " in all)";
        }

        private static void Compare(string name, IReadOnlyList<Streamline> streamlines, int asserted)
        {
            StreamlineBundlingResult connected = Run(streamlines, BundlePartitionRule.Connected);
            StreamlineBundlingResult separated = Run(streamlines, BundlePartitionRule.Separated);
            string verdict = connected.Bundles.Count == separated.Bundles.Count ? "same" : "CHANGED";
            TestContext.Progress.WriteLine(
                $"  {name,-38} asserts {asserted} | connected {connected.Bundles.Count}"
                + $" [{Sizes(connected)}] | separated {separated.Bundles.Count}"
                + $" [{Sizes(separated)}] | refused {separated.RefusedLinkCount}"
                + $" | ever apart {connected.SeparatedPairCount} | {verdict}");
        }

        /// <summary>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~TheFixturesUnderBothRules"</c>.
        /// </summary>
        [Test]
        [Explicit("a simulation, not an assertion")]
        public void TheFixturesUnderBothRules()
        {
            TestContext.Progress.WriteLine("the fixtures StreamlineBundlerTests already asserts:");

            Compare("uniform raft", StreamlineFixtures.UniformRaft(8, 8, 20.0), 1);
            Compare("raft parted by an obstruction",
                    StreamlineFixtures.RaftSplitByObstruction(8, 8, 20.0), 2);
            Compare("density contrast, no obstruction",
                    StreamlineFixtures.RaftWithDensityContrast(), 1);
            Compare("cluster fanning out to four targets",
                    StreamlineFixtures.ClusterFanningOut(4, 9), 4);

            List<Streamline> withLoner = StreamlineFixtures.UniformRaft(4, 4, 20.0);
            withLoner.Add(StreamlineFixtures.FromLateralProfile(z => (5000.0, 0.0)));
            Compare("raft plus one travelling alone", withLoner, 2);

            List<Streamline> twice = new List<Streamline>();
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    double x = i * 20.0;
                    double y = j * 20.0;
                    double first = i < 6 ? -60.0 : 60.0;
                    double second = i < 6 ? 0.0 : (i < 9 ? -60.0 : 60.0);
                    twice.Add(StreamlineFixtures.FromLateralProfile(z =>
                    {
                        double a = z >= -800 ? 0 : (z <= -1200 ? 1 : (-800 - z) / 400.0);
                        double b = z >= -1300 ? 0 : (z <= -1700 ? 1 : (-1300 - z) / 400.0);
                        return (x + first * a + second * b, y);
                    }));
                }
            }
            Compare("raft parting twice", twice, 3);
        }
    }
}
