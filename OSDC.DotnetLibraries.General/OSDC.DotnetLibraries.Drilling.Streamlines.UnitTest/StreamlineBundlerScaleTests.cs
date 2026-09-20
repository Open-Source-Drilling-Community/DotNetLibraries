using System.Diagnostics;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    [TestFixture]
    public class StreamlineBundlerScaleTests
    {
        private static void AssertQuadrantsAreTheBundles(SyntheticStreamlineSource source,
                                                         StreamlineBundlingResult result)
        {
            Assert.That(result.Bundles.Count, Is.EqualTo(4), StreamlineFixtures.Describe(result));
            Dictionary<int, int> quadrantOfBundle = new Dictionary<int, int>();
            for (int s = 0; s < source.Count; s++)
            {
                int bundle = result.BundleIndexOfStreamline[s];
                int quadrant = source.GetExpectedQuadrant(s);
                if (quadrantOfBundle.TryGetValue(bundle, out int expected))
                {
                    Assert.That(quadrant, Is.EqualTo(expected),
                                $"streamline {s} is in bundle {bundle} with streamlines of another quadrant");
                }
                else
                {
                    quadrantOfBundle[bundle] = quadrant;
                }
            }
        }

        [Test]
        public void AFewThousandStreamlinesAreBundledCorrectly()
        {
            SyntheticStreamlineSource source = new SyntheticStreamlineSource(2500, 200);
            StreamlineBundlingResult result = new StreamlineBundler().Bundle(source);
            AssertQuadrantsAreTheBundles(source, result);
        }

        /// <summary>
        /// Not part of the ordinary run: it is a measurement, not an assertion on behaviour. Invoke it
        /// with
        /// <c>dotnet test --filter "FullyQualifiedName~ScalesToTheIntendedProblemSize"</c>.
        /// </summary>
        [Test]
        [Explicit("measures throughput on large inputs")]
        public void ScalesToTheIntendedProblemSize()
        {
            foreach (int streamlineCount in new int[] { 10000, 50000, 100000 })
            {
                SyntheticStreamlineSource source = new SyntheticStreamlineSource(streamlineCount, 2000);
                Stopwatch watch = Stopwatch.StartNew();
                StreamlineBundlingResult result = new StreamlineBundler().Bundle(source);
                watch.Stop();
                long positions = (long)streamlineCount * 2001;
                TestContext.Progress.WriteLine(
                    $"{streamlineCount} streamlines, {positions} positions: {watch.ElapsedMilliseconds} ms, " +
                    StreamlineFixtures.Describe(result));
                AssertQuadrantsAreTheBundles(source, result);
            }
        }

        /// <summary>
        /// Also a measurement rather than an assertion: how long it takes to turn a bundle of realistic
        /// size into a factory, and how much smaller the factory is.
        /// </summary>
        [Test]
        [Explicit("measures the cost of building a factory for a large bundle")]
        public void AFactoryScalesToALargeBundle()
        {
            foreach (int streamlineCount in new int[] { 1000, 10000, 25000 })
            {
                SyntheticStreamlineSource source = new SyntheticStreamlineSource(streamlineCount, 2000);
                StreamlineBundlingResult bundling = new StreamlineBundler().Bundle(source);
                StreamlineBundle largest = bundling.Bundles[0];
                Stopwatch watch = Stopwatch.StartNew();
                StreamlineBundleFactory? factory =
                    new StreamlineBundleFactoryBuilder().Build(source, largest, out StreamlineFactoryFailureReason reason);
                watch.Stop();
                Assert.That(factory, Is.Not.Null, $"refused: {reason}");
                long original = (long)largest.Count * 2001 * 3 * sizeof(double);
                TestContext.Progress.WriteLine(
                    $"bundle of {largest.Count} streamlines, {(long)largest.Count * 2001} positions:" +
                    $" {watch.ElapsedMilliseconds} ms, {original / factory!.GetFootprint()}x smaller, " +
                    BundleFixtures.Describe(factory));
            }
        }
    }
}
