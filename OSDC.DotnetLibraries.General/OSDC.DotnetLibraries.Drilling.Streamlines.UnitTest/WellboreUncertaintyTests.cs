using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    [TestFixture]
    public class WellboreUncertaintyTests
    {
        private const double Degree = System.Math.PI / 180.0;

        /// <summary>
        /// a vertical well from the wellhead to the given depth, with a constant section
        /// </summary>
        private static WellboreUncertainty Vertical(double depth, double semiMajor, double semiMinor,
                                                    double angle = 0, double north = 0, double east = 0)
        {
            List<WellboreUncertaintyStation> stations = new List<WellboreUncertaintyStation>();
            for (double md = 0; md <= depth + 1e-9; md += 25.0)
            {
                stations.Add(new WellboreUncertaintyStation(md, 0, 0, semiMajor, semiMinor, angle));
            }
            return new WellboreUncertainty(stations, north, east, 0, 1.0);
        }

        /// <summary>
        /// a horizontal well running due north at the given depth
        /// </summary>
        private static WellboreUncertainty HorizontalNorth(double length, double depth,
                                                           double semiMajor, double semiMinor, double angle)
        {
            List<WellboreUncertaintyStation> stations = new List<WellboreUncertaintyStation>();
            for (double md = 0; md <= length + 1e-9; md += 25.0)
            {
                stations.Add(new WellboreUncertaintyStation(md, 90.0 * Degree, 0, semiMajor, semiMinor, angle));
            }
            return new WellboreUncertainty(stations, 0, 0, depth, 1.0);
        }

        // ---- the swept solid -----------------------------------------------------------------------

        [Test]
        public void ACircularSectionGivesTheDistanceToTheAxisLessTheRadius()
        {
            WellboreUncertainty well = Vertical(500.0, 8.0, 8.0);
            foreach (double offset in new[] { 0.0, 2.0, 8.0, 12.0, 40.0 })
            {
                double excess = well.GetRadialExcess(SampleNear(well, 250.0), offset, 0.0, 250.0);
                Assert.That(excess, Is.EqualTo(offset - 8.0).Within(1e-9),
                            $"at {offset} m from the axis");
            }
        }

        [Test]
        public void AnEllipticalSectionIsReachedAtItsOwnSemiAxes()
        {
            // the semi-major axis lies along the high side, which for a vertical hole falls back to north
            WellboreUncertainty well = Vertical(500.0, 20.0, 5.0, angle: 0);
            int sample = SampleNear(well, 250.0);
            Assert.That(well.GetRadialExcess(sample, 20.0, 0.0, 250.0), Is.EqualTo(0.0).Within(1e-9),
                        "on the semi-major axis");
            Assert.That(well.GetRadialExcess(sample, 0.0, 5.0, 250.0), Is.EqualTo(0.0).Within(1e-9),
                        "on the semi-minor axis");
            Assert.That(well.GetRadialExcess(sample, 10.0, 0.0, 250.0), Is.LessThan(0.0), "inside");
            Assert.That(well.GetRadialExcess(sample, 0.0, 10.0, 250.0), Is.GreaterThan(0.0), "outside");
        }

        [Test]
        public void TurningTheEllipseTurnsTheSolid()
        {
            WellboreUncertainty well = Vertical(500.0, 20.0, 5.0, angle: 90.0 * Degree);
            int sample = SampleNear(well, 250.0);
            // a quarter turn from the high side puts the long axis on the right hand side, which for a
            // vertical hole with north as the reference is east
            Assert.That(well.GetRadialExcess(sample, 0.0, 20.0, 250.0), Is.EqualTo(0.0).Within(1e-9));
            Assert.That(well.GetRadialExcess(sample, 5.0, 0.0, 250.0), Is.EqualTo(0.0).Within(1e-9));
        }

        [Test]
        public void OnAHorizontalHoleTheHighSideIsUp()
        {
            // due north, horizontal, long axis along the high side. The high side is vertically up, and
            // with the vertical positive downward that is the direction of decreasing vertical.
            WellboreUncertainty well = HorizontalNorth(400.0, 1000.0, 20.0, 5.0, angle: 0);
            int sample = SampleNear(well, 200.0);
            well.GetSample(sample, out double n, out double e, out double v);
            Assert.That(well.GetRadialExcess(sample, n, e, v - 20.0), Is.EqualTo(0.0).Within(1e-6),
                        "twenty metres above the axis should be on the surface");
            Assert.That(well.GetRadialExcess(sample, n, e + 5.0, v), Is.EqualTo(0.0).Within(1e-6),
                        "five metres to the east should be on the surface");
        }

        [Test]
        public void TheAngleIsReferredToTheHighSideAndFallsBackToNorthWhenNearlyVertical()
        {
            // The same changeover a toolface makes between its gravity and its magnetic form. Taking a
            // well heading east makes the two references a quarter turn apart, so which one is in use is
            // unambiguous: the high side of an east heading hole points east and a little up, while true
            // north projected into the same plane points north.
            const double azimuth = 90.0 * Degree;

            List<WellboreUncertaintyStation> steep = new List<WellboreUncertaintyStation>();
            List<WellboreUncertaintyStation> shallow = new List<WellboreUncertaintyStation>();
            for (double md = 0; md <= 400.0 + 1e-9; md += 25.0)
            {
                steep.Add(new WellboreUncertaintyStation(md, 20.0 * Degree, azimuth, 20.0, 4.0, 0));
                shallow.Add(new WellboreUncertaintyStation(md, 1.0 * Degree, azimuth, 20.0, 4.0, 0));
            }
            WellboreUncertainty inclined = new WellboreUncertainty(steep, 0, 0, 0, 1.0);
            WellboreUncertainty upright = new WellboreUncertainty(shallow, 0, 0, 0, 1.0);

            int atSteep = SampleNear(inclined, 200.0);
            inclined.GetSample(atSteep, out double sn, out double se, out double sv);
            // twenty degrees of inclination is above the changeover, so the long axis lies on the high
            // side, which is east and a little up
            double eastward = 20.0 * System.Math.Cos(20.0 * Degree);
            double upward = -20.0 * System.Math.Sin(20.0 * Degree);
            Assert.That(inclined.GetRadialExcess(atSteep, sn, se + eastward, sv + upward),
                        Is.EqualTo(0.0).Within(1e-6), "the high side should be the reference here");
            Assert.That(inclined.GetRadialExcess(atSteep, sn + 20.0, se, sv),
                        Is.GreaterThan(0.0), "north is not the long axis of an inclined hole");

            int atShallow = SampleNear(upright, 200.0);
            upright.GetSample(atShallow, out double un, out double ue, out double uv);
            // one degree is below the changeover, so the reference becomes true north instead
            Assert.That(upright.GetRadialExcess(atShallow, un + 20.0, ue, uv),
                        Is.EqualTo(0.0).Within(1e-3), "north should be the reference here");
            Assert.That(upright.GetRadialExcess(atShallow, un, ue + 20.0, uv),
                        Is.GreaterThan(0.0), "east is not the long axis once the reference is north");
        }

        [Test]
        public void ThePositionOnTheAxisIsTheDeepestInside()
        {
            WellboreUncertainty well = Vertical(500.0, 10.0, 4.0);
            int sample = SampleNear(well, 250.0);
            well.GetSample(sample, out double n, out double e, out double v);
            Assert.That(well.GetRadialExcess(sample, n, e, v), Is.EqualTo(-4.0).Within(1e-9));
        }

        [Test]
        public void TheSweepRatioIsReportedForACurvedWell()
        {
            List<WellboreUncertaintyStation> stations = new List<WellboreUncertaintyStation>();
            for (int s = 0; s <= 40; s++)
            {
                double md = 25.0 * s;
                // two degrees per thirty metres, so a radius of curvature near 860 m
                double inclination = System.Math.Min(90.0, md * 2.0 / 30.0) * Degree;
                stations.Add(new WellboreUncertaintyStation(md, inclination, 0, 20.0, 20.0, 0));
            }
            WellboreUncertainty well = new WellboreUncertainty(stations, 0, 0, 0, 2.0);
            double expected = 20.0 * (2.0 * Degree / 30.0);
            Assert.That(well.MaximumSweepRatio, Is.EqualTo(expected).Within(0.02 * expected));
            Assert.That(well.MaximumSweepRatio, Is.LessThan(1.0), "the sweep must not fold over itself");
        }

        [Test]
        public void AMutedIntervalIsNotAnObstacle()
        {
            WellboreUncertainty well = Vertical(500.0, 8.0, 8.0);
            well.MutedFrom = 200.0;
            well.MutedTo = 300.0;
            int inside = SampleNear(well, 250.0);
            Assert.That(well.IsMuted(inside), Is.True);
            Assert.That(double.IsPositiveInfinity(well.GetRadialExcess(inside, 0, 0, 250.0)), Is.True);
            int outside = SampleNear(well, 400.0);
            Assert.That(well.IsMuted(outside), Is.False);
            Assert.That(well.GetRadialExcess(outside, 0, 0, 400.0), Is.EqualTo(-8.0).Within(1e-9));
        }

        // ---- the field -----------------------------------------------------------------------------

        [Test]
        public void TheFieldFindsTheNearestObstacleSurface()
        {
            ObstacleField field = new ObstacleField();
            field.Add(Vertical(500.0, 5.0, 5.0, north: 0, east: 0));
            field.Add(Vertical(500.0, 5.0, 5.0, north: 40.0, east: 0));
            field.BuildIndex();

            // on the axis of the first well
            Assert.That(field.GetNearestExcess(0, 0, 250.0, 200.0), Is.EqualTo(-5.0).Within(1e-6));
            // exactly between the two: twenty metres from each axis, so fifteen outside each surface
            Assert.That(field.GetNearestExcess(20.0, 0, 250.0, 200.0), Is.EqualTo(15.0).Within(1e-6));
        }

        [Test]
        public void TheGapBetweenTwoWellsIsTheSumOfTheTwoExcesses()
        {
            ObstacleField field = new ObstacleField();
            field.Add(Vertical(500.0, 5.0, 5.0, north: 0, east: 0));
            field.Add(Vertical(500.0, 7.0, 7.0, north: 40.0, east: 0));
            field.BuildIndex();

            field.GetTwoNearest(20.0, 0, 250.0, 200.0, out double nearest, out int nearestWell,
                                out double second, out int secondWell);
            Assert.That(nearestWell, Is.Not.EqualTo(secondWell), "the two must be different wells");
            // the passage between the two surfaces is 40 - 5 - 7 = 28 m wide
            Assert.That(nearest + second, Is.EqualTo(28.0).Within(1e-6));
        }

        [Test]
        public void ACellIsBlockedAsSoonAsItTouchesAnObstacle()
        {
            ObstacleField field = new ObstacleField();
            field.Add(Vertical(500.0, 10.0, 10.0));
            field.BuildIndex();

            OctreeFrame frame = new OctreeFrame(-256, -256, -256, 1024.0);
            // a cell well outside
            OctreeKey far = frame.GetKey(60.0, 0.0, 250.0, 6)!.Value;
            Assert.That(field.IsBlocked(frame.GetCell(far)), Is.False);
            // a cell on the axis
            OctreeKey inside = frame.GetKey(0.0, 0.0, 250.0, 6)!.Value;
            Assert.That(field.IsBlocked(frame.GetCell(inside)), Is.True);

            // and the conservative rule: a cell whose centre is outside but which reaches the surface is
            // still blocked
            OctreeCell cell = frame.GetCell(inside);
            double reach = cell.CircumscribedRadius;
            double excess = field.GetNearestExcess(10.0 + 0.5 * reach, 0.0, 250.0, 100.0);
            Assert.That(excess, Is.GreaterThan(0.0), "that position is outside the solid");
            Assert.That(excess, Is.LessThan(reach), "but within reach of a cell of this size");
        }

        [Test]
        public void TheIndexedSearchAgreesWithAnExhaustiveOne()
        {
            // The bucket index only pays off if it is exact. It is easy to get the reach of the scan
            // subtly wrong, especially for a position outside the box holding the obstacles, and the
            // symptom is not a crash but a grid that is refined in slightly the wrong places.
            ObstacleField field = new ObstacleField();
            field.Add(Vertical(600.0, 6.0, 3.0, angle: 0.3, north: 0, east: 0));
            field.Add(Vertical(500.0, 12.0, 9.0, angle: 1.1, north: 55.0, east: 20.0));
            field.Add(HorizontalNorth(400.0, 300.0, 15.0, 7.0, angle: 0.7));
            field.BuildIndex();

            Random random = new Random(20260916);
            const double limit = 1.0e6;
            for (int attempt = 0; attempt < 3000; attempt++)
            {
                // deliberately including positions well outside the box holding the obstacles
                double n = -400.0 + random.NextDouble() * 900.0;
                double e = -400.0 + random.NextDouble() * 900.0;
                double v = -300.0 + random.NextDouble() * 1200.0;

                ObstacleProximity fast = field.GetProximity(n, e, v, limit);

                double bestFirst = double.PositiveInfinity;
                int bestFirstWell = -1;
                double[] perWell = new double[field.Wells.Count];
                for (int w = 0; w < field.Wells.Count; w++)
                {
                    WellboreUncertainty well = field.Wells[w];
                    double best = double.PositiveInfinity;
                    for (int sample = 0; sample < well.SampleCount; sample++)
                    {
                        double excess = well.GetRadialExcess(sample, n, e, v);
                        if (excess < best)
                        {
                            best = excess;
                        }
                    }
                    perWell[w] = best;
                    if (best < bestFirst)
                    {
                        bestFirst = best;
                        bestFirstWell = w;
                    }
                }
                double bestSecond = double.PositiveInfinity;
                for (int w = 0; w < perWell.Length; w++)
                {
                    if (w != bestFirstWell && perWell[w] < bestSecond)
                    {
                        bestSecond = perWell[w];
                    }
                }

                Assert.That(fast.NearestExcess, Is.EqualTo(bestFirst).Within(1e-9),
                            $"nearest at ({n:F1},{e:F1},{v:F1})");
                Assert.That(fast.SecondExcess, Is.EqualTo(bestSecond).Within(1e-9),
                            $"second at ({n:F1},{e:F1},{v:F1})");
            }
        }

        [Test]
        public void AnEmptyFieldBlocksNothing()
        {
            ObstacleField field = new ObstacleField();
            field.BuildIndex();
            Assert.That(double.IsPositiveInfinity(field.GetNearestExcess(0, 0, 0, 100.0)), Is.True);
        }

        /// <summary>
        /// the sample of the well nearest the given measured depth
        /// </summary>
        private static int SampleNear(WellboreUncertainty well, double measuredDepth)
        {
            int best = 0;
            double nearest = double.MaxValue;
            for (int s = 0; s < well.SampleCount; s++)
            {
                double distance = System.Math.Abs(well.GetMeasuredDepth(s) - measuredDepth);
                if (distance < nearest)
                {
                    nearest = distance;
                    best = s;
                }
            }
            return best;
        }
    }
}
