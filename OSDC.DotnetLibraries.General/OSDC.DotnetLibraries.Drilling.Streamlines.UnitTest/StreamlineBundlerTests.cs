using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    [TestFixture]
    public class StreamlineBundlerTests
    {
        private static StreamlineBundlingResult Run(IReadOnlyList<Streamline> streamlines,
                                                    StreamlineBundlingOptions? options = null)
        {
            StreamlineBundler bundler = new StreamlineBundler(options ?? new StreamlineBundlingOptions());
            return bundler.Bundle(streamlines);
        }

        // ---- what the bundles should be on known configurations ---------------------------------

        [Test]
        public void UniformRaftIsASingleBundle()
        {
            List<Streamline> streamlines = StreamlineFixtures.UniformRaft(8, 8, 20.0);
            StreamlineBundlingResult result = Run(streamlines);
            Assert.That(result.Bundles.Count, Is.EqualTo(1), StreamlineFixtures.Describe(result));
            Assert.That(result.Bundles[0].Count, Is.EqualTo(64));
        }

        [Test]
        public void ARaftPartedByAnObstructionGivesTwoBundles()
        {
            List<Streamline> streamlines = StreamlineFixtures.RaftSplitByObstruction(8, 8, 20.0);
            StreamlineBundlingResult result = Run(streamlines);
            Assert.That(result.Bundles.Count, Is.EqualTo(2), StreamlineFixtures.Describe(result));
            // the fixture adds the streamlines column by column, so the left half is 0..31
            List<List<int>> canonical = StreamlineFixtures.Canonical(result);
            Assert.That(canonical[0], Is.EqualTo(Enumerable.Range(0, 32).ToList()));
            Assert.That(canonical[1], Is.EqualTo(Enumerable.Range(32, 32).ToList()));
        }

        [Test]
        public void ADensityContrastWithoutAnObstructionDoesNotPartAnything()
        {
            // the left half of the raft is four times denser than the right half, as happens where a
            // grid is locally refined around a tight cluster of sources. Nothing is obstructed, so
            // nothing should be separated.
            List<Streamline> streamlines = StreamlineFixtures.RaftWithDensityContrast();
            StreamlineBundlingResult result = Run(streamlines);
            Assert.That(result.Bundles.Count, Is.EqualTo(1), StreamlineFixtures.Describe(result));
            Assert.That(result.Bundles[0].Count, Is.EqualTo(streamlines.Count));
        }

        [Test]
        public void AClusterFanningOutGivesOneBundlePerTarget()
        {
            List<Streamline> streamlines = StreamlineFixtures.ClusterFanningOut(4, 9);
            StreamlineBundlingResult result = Run(streamlines);
            Assert.That(result.Bundles.Count, Is.EqualTo(4), StreamlineFixtures.Describe(result));
            foreach (StreamlineBundle bundle in result.Bundles)
            {
                Assert.That(bundle.Count, Is.EqualTo(9));
            }
        }

        [Test]
        public void AStreamlineTravellingAloneIsABundleOfOne()
        {
            List<Streamline> streamlines = StreamlineFixtures.UniformRaft(4, 4, 20.0);
            streamlines.Add(StreamlineFixtures.FromLateralProfile(z => (5000.0, 0.0)));
            StreamlineBundlingResult result = Run(streamlines);
            Assert.That(result.Bundles.Count, Is.EqualTo(2), StreamlineFixtures.Describe(result));
            StreamlineBundle? alone = result.GetBundleOf(16);
            Assert.That(alone, Is.Not.Null);
            Assert.That(alone!.IsSingleton, Is.True);
            Assert.That(result.AreBundledTogether(0, 16), Is.False);
            Assert.That(result.AreBundledTogether(0, 15), Is.True);
        }

        [Test]
        public void ARaftPartingTwiceGivesThreeBundles()
        {
            // the raft parts in two at 1000 m, and the right half parts again at 1500 m
            List<Streamline> streamlines = new List<Streamline>();
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    double x = i * 20.0;
                    double y = j * 20.0;
                    double first = i < 6 ? -60.0 : 60.0;
                    double second = i < 6 ? 0.0 : (i < 9 ? -60.0 : 60.0);
                    streamlines.Add(StreamlineFixtures.FromLateralProfile(z =>
                    {
                        double a = z >= -800 ? 0 : (z <= -1200 ? 1 : (-800 - z) / 400.0);
                        double b = z >= -1300 ? 0 : (z <= -1700 ? 1 : (-1300 - z) / 400.0);
                        return (x + first * a + second * b, y);
                    }));
                }
            }
            StreamlineBundlingResult result = Run(streamlines);
            Assert.That(result.Bundles.Count, Is.EqualTo(3), StreamlineFixtures.Describe(result));
            List<List<int>> canonical = StreamlineFixtures.Canonical(result);
            Assert.That(canonical[0], Is.EqualTo(Enumerable.Range(0, 24).ToList()));
            Assert.That(canonical[1], Is.EqualTo(Enumerable.Range(24, 12).ToList()));
            Assert.That(canonical[2], Is.EqualTo(Enumerable.Range(36, 12).ToList()));
        }

        // ---- the invariances that show no absolute scale leaked into the algorithm ---------------

        [Test]
        public void TheBundlingIsInvariantUnderAUniformScaling()
        {
            List<Streamline> streamlines = StreamlineFixtures.RaftSplitByObstruction(8, 8, 20.0);
            List<List<int>> reference = StreamlineFixtures.Canonical(Run(streamlines));
            foreach (double factor in new double[] { 1e-3, 1e3 })
            {
                List<List<int>> scaled =
                    StreamlineFixtures.Canonical(Run(StreamlineFixtures.Scale(streamlines, factor)));
                Assert.That(scaled, Is.EqualTo(reference), $"scaling by {factor} changed the bundling");
            }
        }

        [Test]
        public void TheBundlingIsInvariantUnderARefinementOfTheSampling()
        {
            // same geometry, twice as many vertices per streamline: the bundler must read the flow and
            // not the way the streamlines happen to have been sampled
            List<Streamline> streamlines = StreamlineFixtures.RaftSplitByObstruction(8, 8, 20.0);
            List<List<int>> reference = StreamlineFixtures.Canonical(Run(streamlines));
            List<Streamline> densified = StreamlineFixtures.Densify(StreamlineFixtures.Densify(streamlines));
            Assert.That(StreamlineFixtures.Canonical(Run(densified)), Is.EqualTo(reference));
        }

        [Test]
        public void TheBundlingIsInvariantUnderAPermutationOfTheInput()
        {
            List<Streamline> streamlines = StreamlineFixtures.RaftSplitByObstruction(8, 8, 20.0);
            StreamlineBundlingResult reference = Run(streamlines);

            Random random = new Random(12345);
            int[] permutation = Enumerable.Range(0, streamlines.Count).ToArray();
            for (int i = permutation.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (permutation[i], permutation[j]) = (permutation[j], permutation[i]);
            }
            List<Streamline> shuffled = permutation.Select(p => streamlines[p]).ToList();
            StreamlineBundlingResult permuted = Run(shuffled);

            Assert.That(permuted.Bundles.Count, Is.EqualTo(reference.Bundles.Count));
            for (int i = 0; i < streamlines.Count; i++)
            {
                for (int j = i + 1; j < streamlines.Count; j++)
                {
                    bool before = reference.AreBundledTogether(permutation[i], permutation[j]);
                    bool after = permuted.AreBundledTogether(i, j);
                    Assert.That(after, Is.EqualTo(before),
                                $"streamlines {permutation[i]} and {permutation[j]} changed company");
                }
            }
        }

        [Test]
        public void ParallelAndSequentialRunsAgree()
        {
            List<Streamline> streamlines = StreamlineFixtures.RaftSplitByObstruction(8, 8, 20.0);
            List<List<int>> parallel = StreamlineFixtures.Canonical(
                Run(streamlines, new StreamlineBundlingOptions { UseParallelism = true }));
            List<List<int>> sequential = StreamlineFixtures.Canonical(
                Run(streamlines, new StreamlineBundlingOptions { UseParallelism = false }));
            Assert.That(sequential, Is.EqualTo(parallel));
        }

        [Test]
        public void RepeatedRunsGiveTheSameBundles()
        {
            List<Streamline> streamlines = StreamlineFixtures.ClusterFanningOut(3, 12);
            List<List<int>> first = StreamlineFixtures.Canonical(Run(streamlines));
            for (int attempt = 0; attempt < 5; attempt++)
            {
                Assert.That(StreamlineFixtures.Canonical(Run(streamlines)), Is.EqualTo(first));
            }
        }

        // ---- degenerate inputs -------------------------------------------------------------------

        [Test]
        public void AnEmptyInputGivesNoBundle()
        {
            StreamlineBundlingResult result = Run(new List<Streamline>());
            Assert.That(result.Bundles.Count, Is.EqualTo(0));
            Assert.That(result.StreamlineCount, Is.EqualTo(0));
        }

        [Test]
        public void ASingleStreamlineIsABundleOfOne()
        {
            List<Streamline> streamlines = new List<Streamline>
            {
                StreamlineFixtures.FromLateralProfile(z => (0.0, 0.0))
            };
            StreamlineBundlingResult result = Run(streamlines);
            Assert.That(result.Bundles.Count, Is.EqualTo(1));
            Assert.That(result.Bundles[0].IsSingleton, Is.True);
        }

        [Test]
        public void StreamlinesWithoutPositionsAreReportedAndBundledAlone()
        {
            List<Streamline> streamlines = StreamlineFixtures.UniformRaft(4, 4, 20.0);
            streamlines.Add(new Streamline());
            streamlines.Add(new Streamline(new List<Point3D> { new Point3D(0.0, 0.0, 0.0) }));
            StreamlineBundlingResult result = Run(streamlines);
            Assert.That(result.StreamlinesWithoutCrossings, Does.Contain(16));
            Assert.That(result.StreamlinesWithoutCrossings, Does.Contain(17));
            Assert.That(result.GetBundleOf(16)!.IsSingleton, Is.True);
            Assert.That(result.GetBundleOf(17)!.IsSingleton, Is.True);
        }

        [Test]
        public void CoincidentStreamlinesStayTogether()
        {
            List<Streamline> streamlines = new List<Streamline>();
            for (int i = 0; i < 10; i++)
            {
                streamlines.Add(StreamlineFixtures.FromLateralProfile(z => (0.0, 0.0)));
            }
            StreamlineBundlingResult result = Run(streamlines);
            Assert.That(result.Bundles.Count, Is.EqualTo(1), StreamlineFixtures.Describe(result));
        }

        [Test]
        public void InvalidOptionsAreRejected()
        {
            StreamlineBundler bundler = new StreamlineBundler(
                new StreamlineBundlingOptions { ContrastRatio = 0.5 });
            Assert.Throws<ArgumentException>(() => bundler.Bundle(StreamlineFixtures.UniformRaft(2, 2, 20.0)));
        }
    }
}
