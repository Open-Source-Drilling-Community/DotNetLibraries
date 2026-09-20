using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    [TestFixture]
    public class StreamlineTracerTests
    {
        private static StreamlineGrid Box(double length, double width, double finest, double coarsest,
                                          ObstacleField? obstacles = null, double sourceRadius = 0)
        {
            List<Point3D> vertices = new List<Point3D>
            {
                new Point3D(length, 0, 0), new Point3D(length, width, 0),
                new Point3D(length, width, width), new Point3D(length, 0, width)
            };
            return StreamlineGrid.Build(obstacles ?? new ObstacleField(),
                new[] { new StreamlineSource(new Point3D(0, 0, 0), new Vector3D(1, 0, 0)) },
                new TargetPolygon(vertices, finest),
                new StreamlineGridOptions
                {
                    FinestCellSize = finest,
                    CoarsestCellSize = coarsest,
                    MinimumMargin = 0,
                    MarginFraction = 0,
                    SourceRefinementRadius = sourceRadius
                });
        }

        private static FlowProblem SlabToSlab(StreamlineGrid grid, int axis, double totalRate)
        {
            double low = double.MaxValue, high = double.MinValue;
            for (int leaf = 0; leaf < grid.Tree.LeafCount; leaf++)
            {
                OctreeCell cell = grid.Tree.GetCell(leaf);
                double lower = Centre(cell, axis) - 0.5 * cell.Size;
                if (lower < low) { low = lower; }
                if (lower + cell.Size > high) { high = lower + cell.Size; }
            }
            double inletArea = 0, outletArea = 0;
            List<int> inlet = new List<int>(), outlet = new List<int>();
            for (int leaf = 0; leaf < grid.Tree.LeafCount; leaf++)
            {
                OctreeCell cell = grid.Tree.GetCell(leaf);
                double lower = Centre(cell, axis) - 0.5 * cell.Size;
                if (lower <= low + 1e-9) { inlet.Add(leaf); inletArea += cell.Size * cell.Size; }
                if (lower + cell.Size >= high - 1e-9) { outlet.Add(leaf); outletArea += cell.Size * cell.Size; }
            }
            FlowProblem problem = new FlowProblem(grid);
            foreach (int leaf in inlet)
            {
                double s = grid.Tree.GetCell(leaf).Size;
                problem.AddRate(leaf, totalRate * s * s / inletArea);
            }
            foreach (int leaf in outlet)
            {
                double s = grid.Tree.GetCell(leaf).Size;
                problem.AddRate(leaf, -totalRate * s * s / outletArea);
            }
            return problem;
        }

        private static double Centre(OctreeCell cell, int axis)
        {
            return axis == 0 ? cell.CentreNorth : axis == 1 ? cell.CentreEast : cell.CentreVertical;
        }

        // ---- the analytic case ------------------------------------------------------------------------

        [Test]
        public void InAUniformFlowAStreamlineIsAStraightLine()
        {
            StreamlineGrid grid = Box(48.0, 24.0, 2.0, 2.0);
            FlowProblem problem = SlabToSlab(grid, 0, 8.0);
            FlowField field = FlowField.Solve(problem);
            Assert.That(field.Converged, Is.True);

            StreamlineTracer tracer = new StreamlineTracer();
            Streamline line = tracer.Trace(field, problem, 1.0, 11.0, 13.0);
            Assert.That(tracer.Outcome, Is.EqualTo(TraceOutcome.ReachedSink),
                        $"{tracer.Outcome} after {tracer.CellsCrossed} cells");
            Assert.That(line.Count, Is.GreaterThan(10));

            // a uniform flow along north leaves east and vertical untouched, and the path must run the
            // length of the box
            foreach (Point3D position in line.Positions!)
            {
                Assert.That(position.Y!.Value, Is.EqualTo(11.0).Within(1e-9));
                Assert.That(position.Z!.Value, Is.EqualTo(13.0).Within(1e-9));
            }
            Point3D last = line.Positions![line.Count - 1];
            Assert.That(last.X!.Value, Is.GreaterThan(44.0));
        }

        [Test]
        public void TheTimeOfFlightMatchesTheAnalyticOne()
        {
            // A uniform flow of total rate Q across a face of area A has a Darcy velocity Q/A, so a
            // particle crosses a distance L in A*L/Q.
            //
            // The trace has to start clear of the injection cell for that to be the whole story. Inside
            // it the velocity rises from zero at the closed outer boundary to its full value at the far
            // face, so a particle released at its centre sets off at half speed and takes ln(2) times two
            // cell-crossings rather than half of one. That is correct behaviour and not worth designing
            // away, but it is not the analytic case being checked here.
            const double rate = 8.0;
            const double cell = 2.0;
            StreamlineGrid grid = Box(48.0, 24.0, cell, cell);
            FlowProblem problem = SlabToSlab(grid, 0, rate);
            FlowField field = FlowField.Solve(problem);

            double lowNorth = double.MaxValue, highNorth = double.MinValue;
            double lowEast = double.MaxValue, highEast = double.MinValue;
            double lowVertical = double.MaxValue, highVertical = double.MinValue;
            for (int leaf = 0; leaf < grid.Tree.LeafCount; leaf++)
            {
                OctreeCell c = grid.Tree.GetCell(leaf);
                lowNorth = System.Math.Min(lowNorth, c.MinimumNorth);
                highNorth = System.Math.Max(highNorth, c.MinimumNorth + c.Size);
                lowEast = System.Math.Min(lowEast, c.MinimumEast);
                highEast = System.Math.Max(highEast, c.MinimumEast + c.Size);
                lowVertical = System.Math.Min(lowVertical, c.MinimumVertical);
                highVertical = System.Math.Max(highVertical, c.MinimumVertical + c.Size);
            }
            double crossSection = (highEast - lowEast) * (highVertical - lowVertical);

            // start in the middle of the second cell, and stop on entering the last, which is the sink
            double from = lowNorth + 1.5 * cell;
            double to = highNorth - cell;
            double expected = crossSection * (to - from) / rate;

            StreamlineTracer tracer = new StreamlineTracer();
            Streamline line = tracer.Trace(field, problem, from, 11.0, 13.0);
            Assert.That(tracer.Outcome, Is.EqualTo(TraceOutcome.ReachedSink));
            Assert.That(line.Positions![line.Count - 1].X!.Value, Is.EqualTo(to).Within(1e-6));
            // what is left is the solver's own tolerance working through into the fluxes, not the
            // tracer: measured four parts in a hundred million against a residual asked for at one in a
            // thousand million
            Assert.That(tracer.TimeOfFlight, Is.EqualTo(expected).Within(1e-6 * expected),
                        $"time of flight {tracer.TimeOfFlight:F4} against {expected:F4}");
        }

        [Test]
        public void AStreamlineCrossesAChangeOfLevelWithoutDrifting()
        {
            // the grid is refined around the source corner, so a streamline launched through it passes
            // from fine cells to coarse ones and back. In a uniform flow it must still run dead straight.
            StreamlineGrid grid = Box(48.0, 24.0, 1.0, 4.0, sourceRadius: 14.0);
            Assert.That(grid.Tree.GetDepthHistogram().Count(h => h > 0), Is.GreaterThan(1),
                        "the fixture did not produce a graded grid");
            FlowProblem problem = SlabToSlab(grid, 0, 8.0);
            FlowField field = FlowField.Solve(problem);
            Assert.That(field.Converged, Is.True);

            StreamlineTracer tracer = new StreamlineTracer();
            Streamline line = tracer.Trace(field, problem, 0.5, 6.5, 6.5);
            Assert.That(tracer.Outcome, Is.EqualTo(TraceOutcome.ReachedSink),
                        $"{tracer.Outcome} after {tracer.CellsCrossed} cells");

            double worst = 0;
            foreach (Point3D position in line.Positions!)
            {
                worst = System.Math.Max(worst, System.Math.Abs(position.Y!.Value - 6.5));
                worst = System.Math.Max(worst, System.Math.Abs(position.Z!.Value - 6.5));
            }
            // the interface error of a two point flux is about one per cent of the potential drop, so a
            // little sideways drift is expected; a quarter of the finest cell is a generous bound on it
            Assert.That(worst, Is.LessThan(0.25),
                        $"the streamline drifted {worst:F4} m crossing changes of level");
        }

        // ---- what makes the output usable ---------------------------------------------------------------

        [Test]
        public void StreamlinesDoNotCrossOneAnother()
        {
            StreamlineGrid grid = Box(48.0, 24.0, 1.0, 4.0, sourceRadius: 10.0);
            FlowProblem problem = SlabToSlab(grid, 0, 8.0);
            FlowField field = FlowField.Solve(problem);
            StreamlineTracer tracer = new StreamlineTracer();

            List<Streamline> lines = new List<Streamline>();
            for (double east = 4.0; east < 20.0; east += 2.0)
            {
                Streamline line = tracer.Trace(field, problem, 0.5, east, 12.0);
                if (line.Count > 5)
                {
                    lines.Add(line);
                }
            }
            Assert.That(lines.Count, Is.GreaterThan(5));
            // launched in order of increasing east, they must stay in that order for ever: the order of
            // two integral curves of one velocity field cannot change without them meeting
            for (int a = 1; a < lines.Count; a++)
            {
                for (double north = 2.0; north < 44.0; north += 2.0)
                {
                    double before = EastAt(lines[a - 1], north);
                    double after = EastAt(lines[a], north);
                    if (double.IsNaN(before) || double.IsNaN(after))
                    {
                        continue;
                    }
                    Assert.That(after, Is.GreaterThan(before),
                                $"streamlines {a - 1} and {a} swapped order at north {north}");
                }
            }
        }

        private static double EastAt(Streamline line, double north)
        {
            List<Point3D> p = line.Positions!;
            for (int i = 1; i < p.Count; i++)
            {
                double a = p[i - 1].X!.Value, b = p[i].X!.Value;
                if ((a <= north && north <= b) || (b <= north && north <= a))
                {
                    double t = System.Math.Abs(b - a) > 0 ? (north - a) / (b - a) : 0;
                    return p[i - 1].Y!.Value + t * (p[i].Y!.Value - p[i - 1].Y!.Value);
                }
            }
            return double.NaN;
        }

        [Test]
        public void AStreamlineNeverEntersAClosedCell()
        {
            // an obstacle across the middle of the box. The flow goes round it, and no traced position may
            // land inside it: that is the clearance the whole method exists to give.
            ObstacleField obstacles = new ObstacleField();
            List<WellboreUncertaintyStation> stations = new List<WellboreUncertaintyStation>();
            for (double md = 0; md <= 40.0 + 1e-9; md += 5.0)
            {
                stations.Add(new WellboreUncertaintyStation(md, 90.0 * System.Math.PI / 180.0,
                                                            90.0 * System.Math.PI / 180.0, 5.0, 5.0, 0));
            }
            obstacles.Add(new WellboreUncertainty(stations, 24.0, -8.0, 12.0, 1.0));

            StreamlineGrid grid = Box(48.0, 24.0, 1.0, 2.0, obstacles);
            Assert.That(grid.BlockedCount, Is.GreaterThan(0));
            FlowProblem problem = SlabToSlab(grid, 0, 8.0);
            FlowField field = FlowField.Solve(problem);
            Assert.That(field.Converged, Is.True);

            StreamlineTracer tracer = new StreamlineTracer();
            int traced = 0;
            for (double east = 2.0; east < 22.0; east += 1.0)
            {
                for (double vertical = 6.0; vertical < 20.0; vertical += 4.0)
                {
                    Streamline line = tracer.Trace(field, problem, 0.5, east, vertical);
                    if (line.Count < 3)
                    {
                        continue;
                    }
                    traced++;
                    foreach (Point3D position in line.Positions!)
                    {
                        int leaf = grid.Tree.FindLeaf(position.X!.Value, position.Y!.Value,
                                                      position.Z!.Value);
                        if (leaf < 0)
                        {
                            continue;
                        }
                        Assert.That(grid.States[leaf], Is.EqualTo(CellState.Open),
                                    $"a streamline entered a closed cell at "
                                    + $"({position.X!.Value:F2},{position.Y!.Value:F2},{position.Z!.Value:F2})");
                    }
                }
            }
            Assert.That(traced, Is.GreaterThan(30));
        }

        [Test]
        public void AStartOutsideTheGridIsRefusedRatherThanGuessed()
        {
            StreamlineGrid grid = Box(48.0, 24.0, 2.0, 2.0);
            FlowProblem problem = SlabToSlab(grid, 0, 8.0);
            FlowField field = FlowField.Solve(problem);
            StreamlineTracer tracer = new StreamlineTracer();
            Streamline line = tracer.Trace(field, problem, -500.0, 0, 0);
            Assert.That(line.Count, Is.EqualTo(0));
            Assert.That(tracer.Outcome, Is.EqualTo(TraceOutcome.BadStart));
        }
    }
}
