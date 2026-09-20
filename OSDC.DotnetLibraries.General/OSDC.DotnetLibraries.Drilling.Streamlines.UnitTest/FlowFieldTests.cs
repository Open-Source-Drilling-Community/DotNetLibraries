using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    [TestFixture]
    public class FlowFieldTests
    {
        /// <summary>
        /// A grid of uniform cells over a box, with nothing in it. Setting the finest and the coarsest
        /// cell to the same size leaves the octree no room to refine, so the answer can be compared with
        /// the analytic one without discretisation getting in the way.
        /// </summary>
        private static StreamlineGrid UniformBox(double length, double width, double cell)
        {
            List<Point3D> vertices = new List<Point3D>
            {
                new Point3D(length, 0, 0),
                new Point3D(length, width, 0),
                new Point3D(length, width, width),
                new Point3D(length, 0, width)
            };
            return StreamlineGrid.Build(new ObstacleField(),
                new[] { new StreamlineSource(new Point3D(0, 0, 0), new Vector3D(1, 0, 0)) },
                new TargetPolygon(vertices, cell),
                new StreamlineGridOptions
                {
                    FinestCellSize = cell,
                    CoarsestCellSize = cell,
                    MinimumMargin = 0,
                    MarginFraction = 0,
                    SourceRefinementRadius = 0
                });
        }

        /// <summary>
        /// Injects over the whole face at the low end of the given axis and extracts over the high end,
        /// in proportion to the area each cell presents to that face.
        /// <para>
        /// Selecting the cells by their low face rather than by their centre matters as soon as the grid
        /// is graded: cells of different sizes on the same face have different centres, so matching on the
        /// centre picks only whichever size happens to reach furthest and injects into a patch instead of
        /// across the face. Weighting by area is what makes the influx uniform per unit area, which is
        /// what makes the flow one dimensional and the potential exactly linear.
        /// </para>
        /// </summary>
        private static FlowProblem SlabToSlab(StreamlineGrid grid, int axis, double totalRate,
                                              out List<int> inlet, out List<int> outlet)
        {
            // the true extent of the cells, not the box that was asked for: the octree lays whole base
            // cells over the lattice, so it reaches past the requested box by up to one of them
            double low = double.MaxValue;
            double high = double.MinValue;
            for (int leaf = 0; leaf < grid.Tree.LeafCount; leaf++)
            {
                OctreeCell cell = grid.Tree.GetCell(leaf);
                double lower = Centre(cell, axis) - 0.5 * cell.Size;
                if (lower < low) { low = lower; }
                if (lower + cell.Size > high) { high = lower + cell.Size; }
            }
            inlet = new List<int>();
            outlet = new List<int>();
            double inletArea = 0;
            double outletArea = 0;
            for (int leaf = 0; leaf < grid.Tree.LeafCount; leaf++)
            {
                OctreeCell cell = grid.Tree.GetCell(leaf);
                double lower = Centre(cell, axis) - 0.5 * cell.Size;
                double upper = lower + cell.Size;
                if (lower <= low + 1e-9) { inlet.Add(leaf); inletArea += cell.Size * cell.Size; }
                if (upper >= high - 1e-9) { outlet.Add(leaf); outletArea += cell.Size * cell.Size; }
            }
            FlowProblem problem = new FlowProblem(grid);
            foreach (int leaf in inlet)
            {
                double area = grid.Tree.GetCell(leaf).Size;
                problem.AddRate(leaf, totalRate * area * area / inletArea);
            }
            foreach (int leaf in outlet)
            {
                double area = grid.Tree.GetCell(leaf).Size;
                problem.AddRate(leaf, -totalRate * area * area / outletArea);
            }
            return problem;
        }

        private static double Centre(OctreeCell cell, int axis)
        {
            return axis == 0 ? cell.CentreNorth : axis == 1 ? cell.CentreEast : cell.CentreVertical;
        }

        // ---- the analytic case ----------------------------------------------------------------------

        [Test]
        public void UniformFlowAcrossABoxIsLinearInThePotentialAndUniformInTheFlux()
        {
            const double cell = 2.0;
            const double totalRate = 12.0;
            StreamlineGrid grid = UniformBox(48.0, 24.0, cell);
            FlowProblem problem = SlabToSlab(grid, 0, totalRate, out List<int> inlet, out List<int> _);
            Assert.That(problem.GetRateImbalance(), Is.EqualTo(0.0).Within(1e-12));

            FlowField field = FlowField.Solve(problem);
            Assert.That(field.Converged, Is.True, $"residual {field.Residual:E2} after {field.IterationCount}");

            // the potential of a one dimensional flow falls linearly, so fitting a straight line through
            // it should leave nothing behind
            double sumX = 0, sumP = 0, sumXX = 0, sumXP = 0;
            int count = 0;
            for (int leaf = 0; leaf < grid.Tree.LeafCount; leaf++)
            {
                if (!field.IsActive(leaf)) { continue; }
                double x = grid.Tree.GetCell(leaf).CentreNorth;
                double p = field.GetPotential(leaf);
                sumX += x; sumP += p; sumXX += x * x; sumXP += x * p; count++;
            }
            double slope = (count * sumXP - sumX * sumP) / (count * sumXX - sumX * sumX);
            double intercept = (sumP - slope * sumX) / count;
            double worst = 0;
            for (int leaf = 0; leaf < grid.Tree.LeafCount; leaf++)
            {
                if (!field.IsActive(leaf)) { continue; }
                double x = grid.Tree.GetCell(leaf).CentreNorth;
                double error = System.Math.Abs(field.GetPotential(leaf) - (slope * x + intercept));
                if (error > worst) { worst = error; }
            }
            double drop = System.Math.Abs(slope) * 48.0;
            Assert.That(worst, Is.LessThan(1e-8 * drop),
                        $"the potential is not linear: {worst:E3} against a drop of {drop:E3}");

            // and every interior column carries the same share of the flow
            int columns = inlet.Count;
            double expected = totalRate / columns;
            Assert.That(columns, Is.GreaterThan(50), "the inlet should span the whole face");
            for (int leaf = 0; leaf < grid.Tree.LeafCount; leaf++)
            {
                if (!field.IsActive(leaf)) { continue; }
                OctreeCell c = grid.Tree.GetCell(leaf);
                if (c.CentreNorth < 10.0 || c.CentreNorth > 38.0) { continue; }
                Assert.That(field.GetFaceFlux(leaf, 0, 1), Is.EqualTo(expected).Within(1e-9 * totalRate));
                Assert.That(field.GetFaceFlux(leaf, 1, 1), Is.EqualTo(0.0).Within(1e-9 * totalRate));
                Assert.That(field.GetFaceFlux(leaf, 2, 1), Is.EqualTo(0.0).Within(1e-9 * totalRate));
            }
        }

        [Test]
        public void EveryCellConservesWhatPassesThroughIt()
        {
            StreamlineGrid grid = UniformBox(48.0, 24.0, 2.0);
            FlowProblem problem = SlabToSlab(grid, 0, 7.0, out List<int> _, out List<int> _2);
            FlowField field = FlowField.Solve(problem);
            // the whole of tracing rests on this: a field that is not divergence free lets streamlines
            // end in the middle of nowhere or cross one another
            Assert.That(field.WorstCellImbalance, Is.LessThan(1e-9),
                        $"worst cell imbalance {field.WorstCellImbalance:E3}");
        }

        [Test]
        public void TheAnswerDoesNotDependOnWhichAxisTheFlowRunsAlong()
        {
            double[] drops = new double[3];
            for (int axis = 0; axis < 3; axis++)
            {
                StreamlineGrid grid = UniformBox(48.0, 24.0, 3.0);
                FlowProblem problem = SlabToSlab(grid, axis, 5.0, out List<int> _, out List<int> _2);
                FlowField field = FlowField.Solve(problem);
                Assert.That(field.Converged, Is.True);
                double low = double.MaxValue, high = double.MinValue;
                for (int leaf = 0; leaf < grid.Tree.LeafCount; leaf++)
                {
                    if (!field.IsActive(leaf)) { continue; }
                    double p = field.GetPotential(leaf);
                    if (p < low) { low = p; }
                    if (p > high) { high = p; }
                }
                drops[axis] = high - low;
            }
            // north runs the long way and the other two the short way, so those two must agree exactly
            Assert.That(drops[1], Is.EqualTo(drops[2]).Within(1e-9 * drops[1]));
            Assert.That(drops[0], Is.GreaterThan(drops[1]), "the long way must cost more");
        }

        // ---- refinement must not change the answer ---------------------------------------------------

        /// <summary>
        /// the worst departure from a straight line in the potential, as a fraction of the total drop
        /// </summary>
        private static double MeasureNonLinearity(double finest, double coarsest, out int levels)
        {
            List<Point3D> vertices = new List<Point3D>
            {
                new Point3D(48.0, 0, 0), new Point3D(48.0, 24.0, 0),
                new Point3D(48.0, 24.0, 24.0), new Point3D(48.0, 0, 24.0)
            };
            StreamlineGrid grid = StreamlineGrid.Build(new ObstacleField(),
                new[] { new StreamlineSource(new Point3D(0, 0, 0), new Vector3D(1, 0, 0)) },
                new TargetPolygon(vertices, finest),
                new StreamlineGridOptions
                {
                    FinestCellSize = finest,
                    CoarsestCellSize = coarsest,
                    MinimumMargin = 0,
                    MarginFraction = 0,
                    SourceRefinementRadius = 8.0 * coarsest / finest
                });
            levels = grid.Tree.GetDepthHistogram().Count(h => h > 0);

            FlowProblem problem = SlabToSlab(grid, 0, 9.0, out List<int> _, out List<int> _2);
            FlowField field = FlowField.Solve(problem);
            Assert.That(field.Converged, Is.True, $"residual {field.Residual:E2}");
            Assert.That(field.WorstCellImbalance, Is.LessThan(1e-8),
                        "the field must stay conservative whatever the grading does to accuracy");

            double sumX = 0, sumP = 0, sumXX = 0, sumXP = 0;
            int count = 0;
            for (int leaf = 0; leaf < grid.Tree.LeafCount; leaf++)
            {
                if (!field.IsActive(leaf)) { continue; }
                double x = grid.Tree.GetCell(leaf).CentreNorth;
                double p = field.GetPotential(leaf);
                sumX += x; sumP += p; sumXX += x * x; sumXP += x * p; count++;
            }
            double slope = (count * sumXP - sumX * sumP) / (count * sumXX - sumX * sumX);
            double intercept = (sumP - slope * sumX) / count;
            double worst = 0;
            for (int leaf = 0; leaf < grid.Tree.LeafCount; leaf++)
            {
                if (!field.IsActive(leaf)) { continue; }
                double x = grid.Tree.GetCell(leaf).CentreNorth;
                double error = System.Math.Abs(field.GetPotential(leaf) - (slope * x + intercept));
                if (error > worst) { worst = error; }
            }
            return worst / (System.Math.Abs(slope) * 48.0);
        }

        [Test]
        public void AGradedGridBendsAUniformFlowOnlyByADiscretisationError()
        {
            // A two point flux is exact for a linear potential between two cells of the same size,
            // because the line joining their centres crosses the face squarely. At a coarse to fine
            // interface it does not: the coarse centre is offset sideways from three of the four fine
            // centres, so a gradient along the flow shows up as a difference across a face that is
            // supposed to be tangential to it, and a spurious flux crosses there. Those spurious fluxes
            // cancel around the coarse cell, which is why the field stays conservative and the tracing
            // stays sound, but they do perturb the potential nearby.
            //
            // What decides whether that is a discretisation error or an inconsistency is whether it goes
            // away under refinement. It does, and at about first order, so it is the former.
            double coarse = MeasureNonLinearity(1.0, 4.0, out int coarseLevels);
            double fine = MeasureNonLinearity(0.5, 2.0, out int fineLevels);
            Assert.That(coarseLevels, Is.GreaterThan(1), "the fixture did not produce a graded grid");
            Assert.That(fineLevels, Is.GreaterThan(1));
            TestContext.Progress.WriteLine(
                $"non-linearity from grading: {100 * coarse:F3} % of the drop at 1 m, "
                + $"{100 * fine:F3} % at 0.5 m, ratio {coarse / fine:F2}");
            // measured: 1.08 % of the drop with cells from 1 to 4 m, 0.62 % with cells from 0.5 to 2 m,
            // so a little under first order. Worth knowing when reading a streamline that passes close to
            // a change of level; the remedy, if it ever matters, is to interpolate the coarse side to the
            // centre of the sub-face, which is what turns a two point flux into a multi point one.
            Assert.That(coarse, Is.LessThan(0.015));
            Assert.That(fine, Is.LessThan(0.7 * coarse), "and halving the cells must reduce it");
        }

        // ---- the conduit ------------------------------------------------------------------------------

        [Test]
        public void AConduitCarriesTheWholeRateAlongItself()
        {
            const double cell = 2.0;
            StreamlineGrid grid = UniformBox(48.0, 24.0, cell);
            FlowProblem problem = SlabToSlab(grid, 0, 6.0, out List<int> _, out List<int> _2);

            // a chain of cells running east, part way up the box
            List<int> chain = new List<int>();
            for (double east = 1.0; east < 13.0; east += cell)
            {
                int leaf = grid.Tree.FindLeaf(23.0, east, 11.0);
                if (leaf >= 0 && (chain.Count == 0 || chain[chain.Count - 1] != leaf))
                {
                    chain.Add(leaf);
                }
            }
            Assert.That(chain.Count, Is.GreaterThan(3));
            problem.AddConduit(chain);
            // and inject at the head of it, taking the same amount back out at the far slab
            problem.AddRate(chain[0], 4.0);
            problem.AddRate(grid.Tree.FindLeaf(47.0, 23.0, 23.0), -4.0);

            FlowField field = FlowField.Solve(problem);
            Assert.That(field.Converged, Is.True, $"residual {field.Residual:E2}");

            // every cell of the pipe but the last must pass the whole four units along the chain and
            // nothing at all through its other faces
            for (int k = 0; k < chain.Count - 1; k++)
            {
                double along = field.GetFaceFlux(chain[k], 1, 1);
                Assert.That(along, Is.EqualTo(4.0).Within(1e-6),
                            $"cell {k} of the conduit passes {along:F4} rather than four");
                Assert.That(field.GetFaceFlux(chain[k], 0, 1), Is.EqualTo(0.0).Within(1e-9));
                Assert.That(field.GetFaceFlux(chain[k], 0, -1), Is.EqualTo(0.0).Within(1e-9));
                Assert.That(field.GetFaceFlux(chain[k], 2, 1), Is.EqualTo(0.0).Within(1e-9));
                Assert.That(field.GetFaceFlux(chain[k], 2, -1), Is.EqualTo(0.0).Within(1e-9));
            }
        }

        [Test]
        public void BlockedCellsTakeNoFlow()
        {
            ObstacleField field = new ObstacleField();
            List<WellboreUncertaintyStation> stations = new List<WellboreUncertaintyStation>();
            for (double md = 0; md <= 60.0 + 1e-9; md += 10.0)
            {
                stations.Add(new WellboreUncertaintyStation(md, 90.0 * System.Math.PI / 180.0,
                                                            90.0 * System.Math.PI / 180.0, 4.0, 4.0, 0));
            }
            field.Add(new WellboreUncertainty(stations, 24.0, 0.0, 12.0, 1.0));

            List<Point3D> vertices = new List<Point3D>
            {
                new Point3D(48.0, 0, 0), new Point3D(48.0, 24.0, 0),
                new Point3D(48.0, 24.0, 24.0), new Point3D(48.0, 0, 24.0)
            };
            StreamlineGrid grid = StreamlineGrid.Build(field,
                new[] { new StreamlineSource(new Point3D(0, 0, 0), new Vector3D(1, 0, 0)) },
                new TargetPolygon(vertices, 2.0),
                new StreamlineGridOptions
                {
                    FinestCellSize = 2.0, CoarsestCellSize = 2.0,
                    MinimumMargin = 0, MarginFraction = 0, SourceRefinementRadius = 0
                });
            Assert.That(grid.BlockedCount, Is.GreaterThan(0));

            FlowProblem problem = SlabToSlab(grid, 0, 5.0, out List<int> _, out List<int> _2);
            FlowField flow = FlowField.Solve(problem);
            Assert.That(flow.Converged, Is.True);
            for (int leaf = 0; leaf < grid.Tree.LeafCount; leaf++)
            {
                if (grid.States[leaf] != CellState.Blocked) { continue; }
                Assert.That(flow.IsActive(leaf), Is.False, "a blocked cell must carry no unknown");
                for (int axis = 0; axis < 3; axis++)
                {
                    for (int step = -1; step <= 1; step += 2)
                    {
                        Assert.That(flow.GetFaceFlux(leaf, axis, step), Is.EqualTo(0.0),
                                    "and take no flow");
                    }
                }
            }
            Assert.That(flow.WorstCellImbalance, Is.LessThan(1e-8));
        }
    }
}
