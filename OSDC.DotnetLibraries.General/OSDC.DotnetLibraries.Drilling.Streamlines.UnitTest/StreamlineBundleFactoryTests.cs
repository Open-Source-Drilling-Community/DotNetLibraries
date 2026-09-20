using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    [TestFixture]
    public class StreamlineBundleFactoryTests
    {
        private static StreamlineBundleFactory Build(IReadOnlyList<Streamline> streamlines,
                                                     StreamlineBundleFactoryOptions? options = null)
        {
            StreamlineBundleFactoryBuilder builder =
                new StreamlineBundleFactoryBuilder(options ?? new StreamlineBundleFactoryOptions());
            StreamlineBundleFactory? factory = builder.Build(streamlines, out StreamlineFactoryFailureReason reason);
            Assert.That(factory, Is.Not.Null, $"the factory was refused: {reason}");
            return factory!;
        }

        // ---- the median curve ---------------------------------------------------------------------

        [Test]
        public void TheMedianOfAStraightTubeIsItsAxis()
        {
            StreamlineBundleFactory factory = Build(BundleFixtures.StraightTube());
            Streamline median = factory.GetMedianCurve();
            Assert.That(BundleFixtures.GetMaximumAxialDistance(median), Is.LessThan(1.0),
                        BundleFixtures.Describe(factory));
            Assert.That(factory.TangentDeviation, Is.LessThan(1e-3), BundleFixtures.Describe(factory));
        }

        [Test]
        public void TheMedianOfACurvedTubeFollowsTheArc()
        {
            const double curvatureRadius = 1500.0;
            StreamlineBundleFactory factory = Build(BundleFixtures.CurvedTube(curvatureRadius: curvatureRadius));
            // every position of the median must sit on the circle of the axis, centred on (R, 0, 0)
            foreach (CrossSectionStation station in factory.Stations)
            {
                double dx = station.Position!.X!.Value - curvatureRadius;
                double dz = station.Position.Z!.Value;
                double distance = System.Math.Sqrt(dx * dx + dz * dz);
                Assert.That(distance, Is.EqualTo(curvatureRadius).Within(2.0), BundleFixtures.Describe(factory));
            }
        }

        [Test]
        public void TheMedianTakesOnTheShapeOfTheBundle()
        {
            // where the streamlines of a bundle disagree among themselves about which way they are going,
            // the median curve must not add to that disagreement
            foreach (List<Streamline> bundle in new[]
                     {
                         BundleFixtures.StraightTube(),
                         BundleFixtures.ExpandingTube(),
                         BundleFixtures.SwirlingTube(),
                         BundleFixtures.AnnularTube()
                     })
            {
                StreamlineBundleFactory factory = Build(bundle);
                Assert.That(factory.TangentDeviation,
                            Is.LessThanOrEqualTo(factory.BundleTangentSpread + 1e-9),
                            BundleFixtures.Describe(factory));
            }
        }

        [Test]
        public void OnACurvedBundleTheMedianIsAsTrueAsTheStreamlinesThemselvesAre()
        {
            // A curved bundle is the case where the streamlines agree with one another exactly, so there
            // is no spread to hide behind. What is left is that a streamline is a polyline: the direction
            // it reports where it crosses a cross-section is the direction of a chord, and over one chord
            // the true direction turns by the chord length times the curvature. The median cannot be
            // truer than the streamlines it is made of.
            const double curvatureRadius = 1500.0;
            const int samples = 200;
            double chordTurn = (2000.0 / samples) / curvatureRadius;
            StreamlineBundleFactory factory =
                Build(BundleFixtures.CurvedTube(curvatureRadius: curvatureRadius, samples: samples));
            Assert.That(factory.TangentDeviation, Is.LessThan(chordTurn), BundleFixtures.Describe(factory));
        }

        [Test]
        public void SamplingTheStreamlinesMoreFinelyMakesTheMedianTruer()
        {
            // and this is what says the deviation above is in the streamlines handed in rather than in the
            // way the median is built: refine them, and it goes away
            StreamlineBundleFactory coarse = Build(BundleFixtures.CurvedTube(samples: 100));
            StreamlineBundleFactory fine = Build(BundleFixtures.CurvedTube(samples: 800));
            Assert.That(fine.TangentDeviation, Is.LessThan(coarse.TangentDeviation / 3.0),
                        $"coarse: {BundleFixtures.Describe(coarse)}; fine: {BundleFixtures.Describe(fine)}");
            // And the surrogate fits closely throughout, so almost none of the deviation above is it
            // failing to fit. It is no longer exact to a ten thousandth, as it was while the
            // cross-sections carried an affine transform: an outline is piecewise linear between its
            // angular sectors, so even a perfectly smooth cross-section is cut by the chords. That error
            // goes as the square of the sector angle — with the sixty four sectors this fixture gets,
            // about a thousandth, which is what is seen. It is bounded by MaximumSectorCount and not by
            // anything about the bundle.
            Assert.That(coarse.ResidualFraction, Is.LessThan(2e-3));
            Assert.That(fine.ResidualFraction, Is.LessThan(2e-3));
        }

        [Test]
        public void TheMedianStaysInsideTheBundle()
        {
            // position was said to matter more than shape, so the smoothing must not be allowed to carry
            // the curve away from the geometric median of the crossings
            StreamlineBundleFactoryOptions options = new StreamlineBundleFactoryOptions();
            StreamlineBundleFactory factory = Build(BundleFixtures.CurvedTube(), options);
            Assert.That(factory.MedianDisplacementFraction,
                        Is.LessThanOrEqualTo(options.MedianDisplacementBudget + 1e-9),
                        BundleFixtures.Describe(factory));
        }

        // ---- the transforms -----------------------------------------------------------------------

        [Test]
        public void AnAffinelyDeformingTubeIsReproducedExactly()
        {
            // expansion and swirl are both affine maps of the cross-section, so the surrogate should have
            // essentially nothing left over
            foreach (List<Streamline> bundle in new[]
                     {
                         BundleFixtures.StraightTube(),
                         BundleFixtures.ExpandingTube(),
                         BundleFixtures.SwirlingTube()
                     })
            {
                StreamlineBundleFactory factory = Build(bundle);
                Assert.That(factory.ResidualFraction, Is.LessThan(0.02),
                            BundleFixtures.Describe(factory));
            }
        }

        [Test]
        public void ASwirlingTubeNeedsAFrameThatDoesNotSpin()
        {
            // a Frenet frame is undefined on this fixture, the axis being straight. The rotation
            // minimising frame is well defined and the swirl then shows up in the twist of the
            // cross-sections, where it belongs, rather than as residual.
            StreamlineBundleFactory factory = Build(BundleFixtures.SwirlingTube(turns: 2.0));
            Assert.That(factory.ResidualFraction, Is.LessThan(0.02), BundleFixtures.Describe(factory));
            // the cross-sections must actually be turning: compare the first and the last. The twist is
            // the direct successor of the rotation the affine transform used to carry, so this asks the
            // same question of the model that it always did.
            CrossSectionStation first = factory.Stations[0];
            CrossSectionStation last = factory.Stations[factory.Stations.Count - 1];
            Assert.That(System.Math.Abs(last.Twist - first.Twist), Is.GreaterThan(0.1),
                        "the swirl was not taken up by the twist of the cross-sections");
        }

        [Test]
        public void TheTubeRatioIsReportedAndStaysBelowOne()
        {
            StreamlineBundleFactory factory = Build(BundleFixtures.CurvedTube(radius: 40.0,
                                                                              curvatureRadius: 1500.0));
            Assert.That(factory.IsTubeValid, Is.True, BundleFixtures.Describe(factory));
            // a bundle of 40 m radius on a 1500 m arc: the ratio should be of that order, not zero
            Assert.That(factory.MaximumTubeRatio, Is.GreaterThan(0.005).And.LessThan(0.5),
                        BundleFixtures.Describe(factory));
        }

        // ---- production ---------------------------------------------------------------------------

        [Test]
        public void AProducedStreamlineStaysInsideTheBundle()
        {
            const double radius = 40.0;
            StreamlineBundleFactory factory = Build(BundleFixtures.StraightTube(radius: radius));
            Random random = new Random(20260915);
            for (int attempt = 0; attempt < 200; attempt++)
            {
                Streamline? drawn = factory.Draw(random);
                Assert.That(drawn, Is.Not.Null);
                Assert.That(BundleFixtures.GetMaximumAxialDistance(drawn!),
                            Is.LessThanOrEqualTo(radius * 1.05), BundleFixtures.Describe(factory));
            }
        }

        [Test]
        public void ProductionIsDeterministicInTheDrawnPosition()
        {
            StreamlineBundleFactory factory = Build(BundleFixtures.ExpandingTube());
            Streamline first = factory.Generate(0.6 * factory.NormalizedRadius, 1.2);
            Streamline second = factory.Generate(0.6 * factory.NormalizedRadius, 1.2);
            Assert.That(first.Count, Is.EqualTo(second.Count));
            for (int i = 0; i < first.Count; i++)
            {
                Assert.That(first.Positions![i].X!.Value, Is.EqualTo(second.Positions![i].X!.Value));
                Assert.That(first.Positions![i].Y!.Value, Is.EqualTo(second.Positions![i].Y!.Value));
                Assert.That(first.Positions![i].Z!.Value, Is.EqualTo(second.Positions![i].Z!.Value));
            }
        }

        [Test]
        public void ASeededDrawRepeats()
        {
            StreamlineBundleFactory factory = Build(BundleFixtures.StraightTube());
            List<Streamline> first = factory.Draw(20, new Random(7));
            List<Streamline> second = factory.Draw(20, new Random(7));
            Assert.That(first.Count, Is.EqualTo(second.Count));
            for (int s = 0; s < first.Count; s++)
            {
                Assert.That(BundleFixtures.GetLargestDeviation(first[s], second[s], 1), Is.EqualTo(0.0));
            }
        }

        [Test]
        public void TheProducedStreamlinesCoverTheOriginalOnes()
        {
            const double farRadius = 60.0;
            List<Streamline> bundle = BundleFixtures.ExpandingTube(count: 200, farRadius: farRadius);
            StreamlineBundleFactory factory = Build(bundle);
            List<Streamline> drawn = factory.Draw(400, new Random(4242));
            Assert.That(drawn.Count, Is.EqualTo(400));

            // A draw picks a cell of the density and then a position uniformly inside it, so a produced
            // streamline can only be placed to the size of a cell. That, not the number of draws, is what
            // limits how closely an individual streamline of the bundle can be matched, and it is set by
            // how many streamlines there were to estimate the density from.
            double cellSize = factory.NormalizedRadius
                              * System.Math.Sqrt(System.Math.PI / factory.Density!.CellCount);
            double tolerance = 1.5 * cellSize * farRadius / factory.NormalizedRadius;

            double worst = 0;
            double total = 0;
            int checkedCount = 0;
            for (int s = 0; s < bundle.Count; s += 13)
            {
                double nearest = double.MaxValue;
                foreach (Streamline candidate in drawn)
                {
                    double deviation = BundleFixtures.GetLargestDeviation(candidate, bundle[s], 64);
                    if (deviation < nearest)
                    {
                        nearest = deviation;
                    }
                }
                if (nearest > worst)
                {
                    worst = nearest;
                }
                total += nearest;
                checkedCount++;
            }
            Assert.That(worst, Is.LessThan(tolerance),
                        $"the worst reproduced streamline was {worst:F2} m out against a cell of" +
                        $" {tolerance:F2} m. {BundleFixtures.Describe(factory)}");
            // and typically much better than that
            Assert.That(total / checkedCount, Is.LessThan(0.5 * tolerance),
                        BundleFixtures.Describe(factory));
        }

        [Test]
        public void TheProducedStreamlinesStillFormOneBundle()
        {
            // end to end: what comes out of the factory must group back into the bundle it came from
            StreamlineBundleFactory factory = Build(BundleFixtures.ExpandingTube(count: 120));
            List<Streamline> drawn = factory.Draw(120, new Random(99));
            StreamlineBundlingResult result = new StreamlineBundler().Bundle(drawn);
            Assert.That(result.Bundles.Count, Is.EqualTo(1), StreamlineFixtures.Describe(result));
        }

        [Test]
        public void ADensityWithAHoleInItProducesStreamlinesThatAvoidTheMiddle()
        {
            // an annular bundle. No affine transform can express the hole, so if the produced streamlines
            // avoid the middle it is the density that is doing the work.
            const double innerRadius = 30.0;
            const double outerRadius = 40.0;
            StreamlineBundleFactory factory = Build(BundleFixtures.AnnularTube(count: 400,
                                                                               innerRadius: innerRadius,
                                                                               outerRadius: outerRadius));
            List<Streamline> drawn = factory.Draw(400, new Random(31337));
            int insideTheHole = 0;
            foreach (Streamline streamline in drawn)
            {
                if (BundleFixtures.GetMaximumAxialDistance(streamline) < 0.5 * innerRadius)
                {
                    insideTheHole++;
                }
            }
            Assert.That(insideTheHole, Is.EqualTo(0),
                        $"{insideTheHole} of {drawn.Count} were produced inside the hole." +
                        $" {BundleFixtures.Describe(factory)}");
        }

        // ---- bundles that do not get a factory ------------------------------------------------------

        [Test]
        public void ATinyBundleIsRefused()
        {
            List<Streamline> bundle = BundleFixtures.StraightTube(count: 5);
            StreamlineBundleFactoryBuilder builder = new StreamlineBundleFactoryBuilder();
            StreamlineBundleFactory? factory = builder.Build(bundle, out StreamlineFactoryFailureReason reason);
            Assert.That(factory, Is.Null);
            Assert.That(reason, Is.EqualTo(StreamlineFactoryFailureReason.TooFewStreamlines));
        }

        [Test]
        public void ABundleWithoutPositionsIsRefused()
        {
            List<Streamline> bundle = new List<Streamline>();
            for (int i = 0; i < 20; i++)
            {
                bundle.Add(new Streamline());
            }
            StreamlineBundleFactoryBuilder builder = new StreamlineBundleFactoryBuilder();
            StreamlineBundleFactory? factory = builder.Build(bundle, out StreamlineFactoryFailureReason reason);
            Assert.That(factory, Is.Null);
            Assert.That(reason, Is.EqualTo(StreamlineFactoryFailureReason.TooFewPositions));
        }

        [Test]
        public void CoincidentStreamlinesAreRefusedRatherThanFitted()
        {
            List<Streamline> bundle = new List<Streamline>();
            for (int i = 0; i < 20; i++)
            {
                List<Point3D> positions = new List<Point3D>();
                for (int s = 0; s <= 100; s++)
                {
                    positions.Add(new Point3D(0.0, 0.0, -20.0 * s));
                }
                bundle.Add(new Streamline(positions));
            }
            StreamlineBundleFactoryBuilder builder = new StreamlineBundleFactoryBuilder();
            StreamlineBundleFactory? factory = builder.Build(bundle, out StreamlineFactoryFailureReason reason);
            Assert.That(factory, Is.Null);
            Assert.That(reason, Is.EqualTo(StreamlineFactoryFailureReason.DegenerateGeometry));
        }

        // ---- the whole chain --------------------------------------------------------------------

        [Test]
        public void EveryBundleOfABundlingGetsItsFactoryOrAReason()
        {
            List<Streamline> streamlines = StreamlineFixtures.RaftSplitByObstruction(10, 10, 20.0);
            StreamlineListSource source = new StreamlineListSource(streamlines);
            StreamlineBundlingResult bundling = new StreamlineBundler().Bundle(source);
            Assert.That(bundling.Bundles.Count, Is.EqualTo(2));

            StreamlineBundleFactoryBuilder builder = new StreamlineBundleFactoryBuilder();
            List<StreamlineBundleFactory?> factories =
                builder.BuildAll(source, bundling, out List<StreamlineFactoryFailureReason> reasons);
            Assert.That(factories.Count, Is.EqualTo(2));
            for (int b = 0; b < factories.Count; b++)
            {
                Assert.That(factories[b], Is.Not.Null, $"bundle {b} was refused: {reasons[b]}");
                Assert.That(reasons[b], Is.EqualTo(StreamlineFactoryFailureReason.None));
                Assert.That(factories[b]!.SourceStreamlineCount, Is.EqualTo(50));
            }
        }

        [Test]
        public void AFactoryIsMuchSmallerThanTheBundleItReplaces()
        {
            List<Streamline> bundle = BundleFixtures.StraightTube(count: 500, samples: 2000);
            StreamlineBundleFactory factory = Build(bundle);
            long original = 500L * 2001 * 3 * sizeof(double);
            Assert.That(factory.GetFootprint(), Is.LessThan(original / 100),
                        BundleFixtures.Describe(factory));
        }
    }
}
