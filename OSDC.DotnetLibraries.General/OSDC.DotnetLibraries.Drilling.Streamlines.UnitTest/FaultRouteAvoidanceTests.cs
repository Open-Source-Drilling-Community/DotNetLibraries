using NUnit.Framework;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    [TestFixture]
    public class FaultRouteAvoidanceTests
    {
        private static (double, IReadOnlyCollection<string>) Corridor(double weight, params string[] faults)
        {
            return (weight, faults);
        }

        [Test]
        public void TheCorridorsThroughAFaultTipAreGivenUpAndAFaultEveryoneCrossesIsKept()
        {
            // the 85 degree slot case in miniature: every corridor crosses A squarely, the two furthest
            // out on one side go through the tip of B as well, and the rest pass it by
            List<(double, IReadOnlyCollection<string>)> route = new List<(double, IReadOnlyCollection<string>)>
            {
                Corridor(160, "A"), Corridor(110, "A"), Corridor(60, "A"),
                Corridor(37, "A", "B"), Corridor(20, "A", "B")
            };
            FaultRouteOutcome outcome = FaultRouteAvoidance.Choose(route, 0.25);
            Assert.That(outcome.Avoided.Select(f => f.Fault), Is.EqualTo(new[] { "B" }));
            Assert.That(outcome.Kept.Select(f => f.Fault), Is.EqualTo(new[] { "A" }));
            Assert.That(outcome.Excluded, Is.EqualTo(new[] { 3, 4 }));
            Assert.That(outcome.ShareGivenUp, Is.EqualTo(57.0 / 387.0).Within(1e-12));
        }

        [Test]
        public void AFaultOneSmallCorridorHappensToMissIsNotAvoidedAtTheCostOfTheRest()
        {
            // "avoidable if any corridor avoids it" would give up nearly everything here
            List<(double, IReadOnlyCollection<string>)> route = new List<(double, IReadOnlyCollection<string>)>
            {
                Corridor(160, "A"), Corridor(110, "A"), Corridor(11)
            };
            FaultRouteOutcome outcome = FaultRouteAvoidance.Choose(route, 0.25);
            Assert.That(outcome.Avoided, Is.Empty);
            Assert.That(outcome.Excluded, Is.Empty);
            Assert.That(outcome.Kept.Select(f => f.Fault), Is.EqualTo(new[] { "A" }));
        }

        [Test]
        public void TheLimitIsSpentOnAllTheFaultsTogetherCheapestFirst()
        {
            // each fault alone is within the limit, both together are not, so only the cheaper goes
            List<(double, IReadOnlyCollection<string>)> route = new List<(double, IReadOnlyCollection<string>)>
            {
                Corridor(70), Corridor(15, "B"), Corridor(20, "C")
            };
            FaultRouteOutcome outcome = FaultRouteAvoidance.Choose(route, 0.25);
            Assert.That(outcome.Avoided.Select(f => f.Fault), Is.EqualTo(new[] { "B" }));
            Assert.That(outcome.Kept.Select(f => f.Fault), Is.EqualTo(new[] { "C" }));
            Assert.That(outcome.Excluded, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void AFaultWhoseCorridorsAreAlreadyGivenUpCostsNothingMore()
        {
            List<(double, IReadOnlyCollection<string>)> route = new List<(double, IReadOnlyCollection<string>)>
            {
                Corridor(80), Corridor(10, "B", "C"), Corridor(10, "B")
            };
            FaultRouteOutcome outcome = FaultRouteAvoidance.Choose(route, 0.25);
            Assert.That(outcome.Avoided.Select(f => f.Fault), Is.EquivalentTo(new[] { "B", "C" }));
            Assert.That(outcome.ShareGivenUp, Is.EqualTo(0.2).Within(1e-12));
        }

        [Test]
        public void AnExcludedCorridorIsNeverDrawnFrom()
        {
            StreamlineBundleFactoryBuilder builder = new StreamlineBundleFactoryBuilder(new StreamlineBundleFactoryOptions());
            StreamlineBundleFactory? wide = builder.Build(BundleFixtures.StraightTube(radius: 40.0), out _);
            StreamlineBundleFactory? narrow = builder.Build(BundleFixtures.StraightTube(radius: 4.0), out _);
            Assert.That(wide, Is.Not.Null);
            Assert.That(narrow, Is.Not.Null);
            StreamlineMetaFactory route = new StreamlineMetaFactory(0);
            route.Add(wide!);
            route.Add(narrow!);

            RealizationFilter filter = new RealizationFilter
            {
                Excluded = new HashSet<StreamlineBundleFactory> { wide! }
            };
            Assert.That(route.GetQualifying(filter), Is.EqualTo(new[] { narrow }));
            List<Streamline> drawn = route.GetRealizations(200, filter, new Random(1));
            Assert.That(drawn.Count, Is.EqualTo(200));
            double furthest = 0;
            foreach (Streamline one in drawn)
            {
                foreach (Point3D at in one.Positions!)
                {
                    double off = System.Math.Sqrt(at.X!.Value * at.X.Value + at.Y!.Value * at.Y.Value);
                    if (off > furthest) { furthest = off; }
                }
            }
            // the narrow tube is four metres across its radius; the wide one would reach forty
            Assert.That(furthest, Is.LessThan(10.0));
        }
    }
}
