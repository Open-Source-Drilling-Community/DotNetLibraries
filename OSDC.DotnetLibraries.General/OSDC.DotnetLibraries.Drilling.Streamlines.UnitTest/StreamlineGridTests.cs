using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    [TestFixture]
    public class StreamlineGridTests
    {
        private const double Degree = System.Math.PI / 180.0;

        private static WellboreUncertainty VerticalWell(double north, double east, double depth,
                                                        double radius, string name)
        {
            List<WellboreUncertaintyStation> stations = new List<WellboreUncertaintyStation>();
            for (double md = 0; md <= depth + 1e-9; md += 50.0)
            {
                stations.Add(new WellboreUncertaintyStation(md, 0, 0, radius, radius, 0));
            }
            return new WellboreUncertainty(stations, north, east, 0, 2.0) { Name = name };
        }

        private static TargetPolygon SquareTarget(double north, double east, double vertical, double half,
                                                  double thickness = 10.0)
        {
            List<Point3D> vertices = new List<Point3D>
            {
                new Point3D(north - half, east - half, vertical),
                new Point3D(north + half, east - half, vertical),
                new Point3D(north + half, east + half, vertical),
                new Point3D(north - half, east + half, vertical)
            };
            return new TargetPolygon(vertices, thickness);
        }

        private static StreamlineSource DownwardSource(double north, double east, double vertical)
        {
            return new StreamlineSource(new Point3D(north, east, vertical), new Vector3D(0, 0, 1));
        }

        // ---- the target region ---------------------------------------------------------------------

        [Test]
        public void TheTargetPrismHoldsWhatItShould()
        {
            TargetPolygon target = SquareTarget(0, 0, 500.0, 50.0, thickness: 10.0);
            Assert.That(target.Contains(0, 0, 500.0), Is.True, "the middle");
            Assert.That(target.Contains(40.0, 40.0, 502.0), Is.True, "inside and within the thickness");
            Assert.That(target.Contains(60.0, 0, 500.0), Is.False, "outside the polygon");
            Assert.That(target.Contains(0, 0, 510.0), Is.False, "outside the thickness");
            Vector3D normal = target.GetNormal();
            Assert.That(System.Math.Abs(normal.Z!.Value), Is.EqualTo(1.0).Within(1e-9),
                        "a horizontal polygon has a vertical normal");
        }

        // ---- refinement ------------------------------------------------------------------------------

        [Test]
        public void TheCellsAreFinestWhereThePassageIsNarrowest()
        {
            // two vertical wells whose surfaces leave a twelve metre gap between them
            ObstacleField field = new ObstacleField();
            field.Add(VerticalWell(-16.0, 0, 600.0, 10.0, "left"));
            field.Add(VerticalWell(16.0, 0, 600.0, 10.0, "right"));

            StreamlineGridOptions options = new StreamlineGridOptions
            {
                FinestCellSize = 0.5,
                CoarsestCellSize = 32.0,
                MinimumMargin = 80.0,
                MarginFraction = 0.0
            };
            StreamlineGrid grid = StreamlineGrid.Build(field, new[] { DownwardSource(0, 0, 5.0) },
                                                       SquareTarget(0, 0, 550.0, 20.0), options);

            // in the middle of the gap the cells must be about a quarter of the twelve metre passage
            int inGap = grid.Tree.FindLeaf(0, 0, 300.0);
            Assert.That(inGap, Is.GreaterThanOrEqualTo(0));
            double gapSize = grid.Tree.GetCell(inGap).Size;
            Assert.That(gapSize, Is.LessThanOrEqualTo(4.0), grid.Describe());

            // far from everything they must stay coarse, but still inside the domain box
            int farAway = grid.Tree.FindLeaf(0, 80.0, 300.0);
            Assert.That(farAway, Is.GreaterThanOrEqualTo(0));
            Assert.That(grid.Tree.GetCell(farAway).Size, Is.GreaterThan(4.0 * gapSize), grid.Describe());
        }

        [Test]
        public void EveryPositionInsideAnUncertaintyVolumeIsInABlockedCell()
        {
            ObstacleField field = new ObstacleField();
            field.Add(VerticalWell(-16.0, 0, 600.0, 10.0, "left"));
            field.Add(VerticalWell(16.0, 0, 600.0, 10.0, "right"));
            StreamlineGrid grid = StreamlineGrid.Build(field, new[] { DownwardSource(0, 0, 5.0) },
                                                       SquareTarget(0, 0, 550.0, 20.0),
                                                       new StreamlineGridOptions { MarginFraction = 0.0 });

            Random random = new Random(4242);
            int tested = 0;
            for (int attempt = 0; attempt < 4000; attempt++)
            {
                // a position strictly inside one of the two tubes
                double centre = random.Next(2) == 0 ? -16.0 : 16.0;
                double angle = random.NextDouble() * 2.0 * System.Math.PI;
                double radius = 9.0 * System.Math.Sqrt(random.NextDouble());
                double n = centre + radius * System.Math.Cos(angle);
                double e = radius * System.Math.Sin(angle);
                double v = 20.0 + random.NextDouble() * 560.0;
                int leaf = grid.Tree.FindLeaf(n, e, v);
                if (leaf < 0)
                {
                    continue;
                }
                tested++;
                Assert.That(grid.States[leaf], Is.EqualTo(CellState.Blocked),
                            $"({n:F2},{e:F2},{v:F2}) is inside a volume but its cell is open");
            }
            Assert.That(tested, Is.GreaterThan(3000));
        }

        // ---- connectivity ----------------------------------------------------------------------------

        [Test]
        public void APassageBetweenTwoWellsIsFound()
        {
            ObstacleField field = new ObstacleField();
            field.Add(VerticalWell(-16.0, 0, 600.0, 10.0, "left"));
            field.Add(VerticalWell(16.0, 0, 600.0, 10.0, "right"));
            StreamlineGrid grid = StreamlineGrid.Build(field, new[] { DownwardSource(0, 0, 5.0) },
                                                       SquareTarget(0, 0, 550.0, 20.0),
                                                       new StreamlineGridOptions { MarginFraction = 0.0 });
            Assert.That(grid.Status, Is.EqualTo(StreamlineGridStatus.Connected), grid.Describe());
            Assert.That(grid.SinkLeaves.Count, Is.GreaterThan(0));
        }

        [Test]
        public void ASourceInsideAnUncertaintyVolumeIsReported()
        {
            ObstacleField field = new ObstacleField();
            field.Add(VerticalWell(0, 0, 600.0, 10.0, "parent"));
            // the source sits on the axis of the parent and nothing has been muted
            StreamlineGrid grid = StreamlineGrid.Build(field, new[] { DownwardSource(0, 0, 200.0) },
                                                       SquareTarget(120.0, 0, 550.0, 20.0),
                                                       new StreamlineGridOptions { MarginFraction = 0.0 });
            Assert.That(grid.Status, Is.EqualTo(StreamlineGridStatus.SourceBlocked), grid.Describe());
        }

        [Test]
        public void MutingTheParentLetsASidetrackStart()
        {
            ObstacleField field = new ObstacleField();
            field.Add(VerticalWell(0, 0, 600.0, 10.0, "parent"));
            StreamlineSource tieIn = new StreamlineSource(new Point3D(0, 0, 200.0), new Vector3D(1, 0, 1))
            {
                ParentWell = 0,
                ParentMeasuredDepth = 200.0,
                ParentMutedLength = 40.0
            };
            StreamlineGrid grid = StreamlineGrid.Build(field, new[] { tieIn },
                                                       SquareTarget(120.0, 0, 550.0, 20.0),
                                                       new StreamlineGridOptions { MarginFraction = 0.0 });
            Assert.That(grid.Status, Is.EqualTo(StreamlineGridStatus.Connected), grid.Describe());
        }

        [Test]
        public void AWallOfWellsIsReportedAsNoPath()
        {
            // a raft of overlapping horizontal wells right across the domain, between source and target
            ObstacleField field = new ObstacleField();
            for (double east = -140.0; east <= 140.0; east += 8.0)
            {
                List<WellboreUncertaintyStation> stations = new List<WellboreUncertaintyStation>();
                for (double md = 0; md <= 300.0; md += 50.0)
                {
                    stations.Add(new WellboreUncertaintyStation(md, 90.0 * Degree, 0, 6.0, 6.0, 0));
                }
                field.Add(new WellboreUncertainty(stations, -150.0, east, 200.0, 2.0));
            }
            StreamlineGridOptions options = new StreamlineGridOptions
            {
                FinestCellSize = 1.0,
                CoarsestCellSize = 16.0,
                MinimumMargin = 25.0,
                MarginFraction = 0.0
            };
            StreamlineGrid grid = StreamlineGrid.Build(field, new[] { DownwardSource(0, 0, 20.0) },
                                                       SquareTarget(0, 0, 380.0, 30.0), options);
            Assert.That(grid.Status, Is.EqualTo(StreamlineGridStatus.NotConnected), grid.Describe());
            Assert.That(grid.ReachableCount, Is.GreaterThan(0), "the source itself must be reachable");
        }

        [Test]
        public void WideningTheMarginDoesNotChangeWhetherAPassageExists()
        {
            ObstacleField field = new ObstacleField();
            field.Add(VerticalWell(-16.0, 0, 600.0, 10.0, "left"));
            field.Add(VerticalWell(16.0, 0, 600.0, 10.0, "right"));
            foreach (double margin in new[] { 60.0, 120.0, 240.0 })
            {
                StreamlineGrid grid = StreamlineGrid.Build(field, new[] { DownwardSource(0, 0, 5.0) },
                                                           SquareTarget(0, 0, 550.0, 20.0),
                                                           new StreamlineGridOptions
                                                           {
                                                               MinimumMargin = margin,
                                                               MarginFraction = 0.0
                                                           });
                Assert.That(grid.Status, Is.EqualTo(StreamlineGridStatus.Connected),
                            $"margin {margin}: {grid.Describe()}");
            }
        }
    }
}
