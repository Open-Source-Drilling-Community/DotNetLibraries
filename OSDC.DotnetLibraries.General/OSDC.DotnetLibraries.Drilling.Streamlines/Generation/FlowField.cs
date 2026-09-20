namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// The settings of the flow solve.
    /// </summary>
    public class FlowFieldOptions
    {
        /// <summary>
        /// the largest number of conjugate gradient iterations
        /// </summary>
        public int MaximumIterationCount { get; set; } = 20000;

        /// <summary>
        /// the relative residual at which the solve is taken to have converged
        /// </summary>
        public double Tolerance { get; set; } = 1.0e-9;
    }

    /// <summary>
    /// The potential and the face fluxes on a <see cref="StreamlineGrid"/>.
    /// <para>
    /// Darcy flow and current flow are the same steady problem, the divergence of a conductivity times a
    /// gradient being zero, so one solver serves both. The medium here is homogeneous and isotropic, which
    /// is what makes a two point flux exact: it is only for a tensor with off-diagonal terms that two
    /// point flux becomes inconsistent, and anisotropy is deliberately left out of this first increment.
    /// </para>
    /// <para>
    /// Flux is stored per face rather than as a velocity per cell, because that is what tracing needs. A
    /// single flux per face, shared by the two cells either side, is what makes the field exactly
    /// divergence free cell by cell, and that in turn is what stops traced streamlines from crossing or
    /// from leaking through a face.
    /// </para>
    /// </summary>
    public class FlowField
    {
        private readonly int[] unknownOf_;
        private readonly double[] potential_;
        private readonly double[] faceFlux_;

        /// <summary>
        /// the grid solved on
        /// </summary>
        public StreamlineGrid Grid { get; }

        /// <summary>
        /// how many cells carried an unknown
        /// </summary>
        public int UnknownCount { get; private set; }

        /// <summary>
        /// how many iterations the solve took
        /// </summary>
        public int IterationCount { get; private set; }

        /// <summary>
        /// the relative residual reached
        /// </summary>
        public double Residual { get; private set; }

        /// <summary>
        /// whether the residual came down to the tolerance asked for
        /// </summary>
        public bool Converged { get; private set; }

        /// <summary>
        /// the largest imbalance between what goes into a cell and what comes out of it, as a fraction of
        /// the total rate. A measure of whether the field is really divergence free, which is what tracing
        /// depends on.
        /// </summary>
        public double WorstCellImbalance { get; private set; }

        private FlowField(StreamlineGrid grid, int[] unknownOf, double[] potential, double[] faceFlux)
        {
            Grid = grid;
            unknownOf_ = unknownOf;
            potential_ = potential;
            faceFlux_ = faceFlux;
        }

        /// <summary>
        /// the potential at a leaf, or not a number where the cell carries no unknown
        /// </summary>
        /// <param name="leaf"></param>
        /// <returns></returns>
        public double GetPotential(int leaf)
        {
            return potential_[leaf];
        }

        /// <summary>
        /// whether the leaf took part in the solve
        /// </summary>
        public bool IsActive(int leaf)
        {
            return unknownOf_[leaf] >= 0;
        }

        /// <summary>
        /// The net flow through one face of a leaf, positive outward.
        /// </summary>
        /// <param name="leaf"></param>
        /// <param name="axis">0 north, 1 east, 2 vertical</param>
        /// <param name="step">-1 or 1</param>
        /// <returns></returns>
        public double GetFaceFlux(int leaf, int axis, int step)
        {
            return faceFlux_[6 * leaf + 2 * axis + (step > 0 ? 1 : 0)];
        }

        /// <summary>
        /// Solves the problem.
        /// </summary>
        /// <param name="problem"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static FlowField Solve(FlowProblem problem, FlowFieldOptions? options = null)
        {
            if (problem == null)
            {
                throw new ArgumentNullException(nameof(problem));
            }
            options ??= new FlowFieldOptions();
            StreamlineGrid grid = problem.Grid;
            LinearOctree tree = grid.Tree;
            int leaves = tree.LeafCount;

            // which cells carry an unknown
            int[] unknownOf = new int[leaves];
            int unknowns = 0;
            for (int leaf = 0; leaf < leaves; leaf++)
            {
                unknownOf[leaf] = grid.States[leaf] == CellState.Open ? unknowns++ : -1;
            }

            // a cell of a conduit is closed on every face but the two that continue the chain, so that
            // what is injected at its head has nowhere to go but along it
            // An open headed conduit simply leaves its first cell unrestricted, exactly as the last cell
            // of any conduit is left, so that flow can enter from the medium at that end.
            Dictionary<int, (int Previous, int Next)> chain = new Dictionary<int, (int, int)>();
            foreach (FlowConduit conduit in problem.Conduits)
            {
                int[] cells = conduit.Cells;
                for (int k = conduit.OpenAtHead ? 1 : 0; k < cells.Length - 1; k++)
                {
                    int previous = k > 0 ? cells[k - 1] : -1;
                    chain[cells[k]] = (previous, cells[k + 1]);
                }
            }

            BuildConnections(tree, grid, unknownOf, chain, problem, out int[] rowStart, out int[] column,
                             out double[] transmissibility, out int[] connectionLeaf,
                             out int[] connectionFace);

            double[] diagonal = new double[unknowns];
            for (int row = 0; row < unknowns; row++)
            {
                double sum = 0;
                for (int at = rowStart[row]; at < rowStart[row + 1]; at++)
                {
                    sum += transmissibility[at];
                }
                diagonal[row] = sum;
            }

            double[] rightHand = new double[unknowns];
            foreach (KeyValuePair<int, double> entry in problem.Rates)
            {
                int row = unknownOf[entry.Key];
                if (row >= 0)
                {
                    rightHand[row] += entry.Value;
                }
            }

            double[] solution = new double[unknowns];
            SolveConjugateGradient(rowStart, column, transmissibility, diagonal, rightHand, solution,
                                   options, out int iterations, out double residual, out bool converged);

            double[] potential = new double[leaves];
            for (int leaf = 0; leaf < leaves; leaf++)
            {
                potential[leaf] = unknownOf[leaf] >= 0 ? solution[unknownOf[leaf]] : double.NaN;
            }

            // the flux through every face, one value per face shared by the two cells either side
            double[] faceFlux = new double[6 * leaves];
            for (int row = 0; row < unknowns; row++)
            {
                for (int at = rowStart[row]; at < rowStart[row + 1]; at++)
                {
                    double flow = transmissibility[at] * (solution[row] - solution[column[at]]);
                    faceFlux[6 * connectionLeaf[at] + connectionFace[at]] += flow;
                }
            }

            FlowField field = new FlowField(grid, unknownOf, potential, faceFlux)
            {
                UnknownCount = unknowns,
                IterationCount = iterations,
                Residual = residual,
                Converged = converged
            };
            field.MeasureImbalance(problem);
            return field;
        }

        /// <summary>
        /// The transmissibility of every connection, in compressed row form over the unknowns.
        /// <para>
        /// For a homogeneous isotropic medium the transmissibility of a face is its area over the distance
        /// between the two cell centres, which is the two half conductances in series. Where a coarse cell
        /// meets four fine ones, the shared area is that of the fine face and the two half distances
        /// differ, so each of the four sub-faces carries its own transmissibility and the coarse cell sees
        /// four connections rather than one. That keeps the flux single valued on each sub-face, which is
        /// what conservation across the interface requires.
        /// </para>
        /// </summary>
        private static void BuildConnections(LinearOctree tree, StreamlineGrid grid, int[] unknownOf,
                                             Dictionary<int, (int Previous, int Next)> chain,
                                             FlowProblem problem,
                                             out int[] rowStart, out int[] column,
                                             out double[] transmissibility, out int[] connectionLeaf,
                                             out int[] connectionFace)
        {
            int leaves = tree.LeafCount;
            int unknowns = 0;
            for (int leaf = 0; leaf < leaves; leaf++)
            {
                if (unknownOf[leaf] >= 0) { unknowns++; }
            }

            List<int> otherLeaf = new List<int>();
            List<int> ownFace = new List<int>();
            List<int> otherFace = new List<int>();
            List<int> ownLeaf = new List<int>();
            List<double> value = new List<double>();
            List<int> neighbours = new List<int>();

            // the medium, one value per axis per cell, worked out once rather than once per face
            IFaceMobility? mobility = problem.Mobility;
            double[] ownMobility = { 1.0, 1.0, 1.0 };
            double[] farMobility = { 1.0, 1.0, 1.0 };

            for (int leaf = 0; leaf < leaves; leaf++)
            {
                if (unknownOf[leaf] < 0)
                {
                    continue;
                }
                bool restricted = chain.TryGetValue(leaf, out (int Previous, int Next) link);
                OctreeCell ownCell = tree.GetCell(leaf);
                double ownSize = ownCell.Size;
                if (mobility != null)
                {
                    mobility.GetValues(ownCell.CentreNorth, ownCell.CentreEast, ownCell.CentreVertical,
                                       ownMobility);
                }
                for (int axis = 0; axis < 3; axis++)
                {
                    for (int step = -1; step <= 1; step += 2)
                    {
                        if (problem.IsFaceClosed(leaf, axis, step))
                        {
                            continue;
                        }
                        neighbours.Clear();
                        tree.GetFaceNeighbours(leaf, axis, step, neighbours);
                        foreach (int other in neighbours)
                        {
                            if (other <= leaf || unknownOf[other] < 0)
                            {
                                continue;
                            }
                            // a face belongs to both cells, so either may shut it
                            if (problem.IsFaceClosed(other, axis, -step))
                            {
                                continue;
                            }
                            if (restricted && other != link.Previous && other != link.Next)
                            {
                                continue;
                            }
                            if (chain.TryGetValue(other, out (int Previous, int Next) far)
                                && leaf != far.Previous && leaf != far.Next)
                            {
                                continue;
                            }
                            OctreeCell farCell = tree.GetCell(other);
                            double otherSize = farCell.Size;
                            if (mobility != null)
                            {
                                mobility.GetValues(farCell.CentreNorth, farCell.CentreEast,
                                                   farCell.CentreVertical, farMobility);
                            }
                            double shared = System.Math.Min(ownSize, otherSize);
                            double area = shared * shared;
                            double half = ownMobility[axis] * area / (0.5 * ownSize);
                            double farHalf = farMobility[axis] * area / (0.5 * otherSize);
                            ownLeaf.Add(leaf);
                            otherLeaf.Add(other);
                            ownFace.Add(2 * axis + (step > 0 ? 1 : 0));
                            otherFace.Add(2 * axis + (step > 0 ? 0 : 1));
                            value.Add(1.0 / (1.0 / half + 1.0 / farHalf));
                        }
                    }
                }
            }

            // both directions of every connection, sorted into rows
            int count = value.Count;
            int[] degree = new int[unknowns + 1];
            for (int c = 0; c < count; c++)
            {
                degree[unknownOf[ownLeaf[c]] + 1]++;
                degree[unknownOf[otherLeaf[c]] + 1]++;
            }
            for (int row = 0; row < unknowns; row++)
            {
                degree[row + 1] += degree[row];
            }
            int[] columns = new int[2 * count];
            double[] weights = new double[2 * count];
            int[] leavesOf = new int[2 * count];
            int[] facesOf = new int[2 * count];
            int[] fill = new int[unknowns];
            for (int c = 0; c < count; c++)
            {
                Place(unknownOf[ownLeaf[c]], unknownOf[otherLeaf[c]], ownLeaf[c], ownFace[c], value[c]);
                Place(unknownOf[otherLeaf[c]], unknownOf[ownLeaf[c]], otherLeaf[c], otherFace[c], value[c]);
            }
            rowStart = degree;
            column = columns;
            transmissibility = weights;
            connectionLeaf = leavesOf;
            connectionFace = facesOf;

            void Place(int row, int other, int leaf, int face, double weight)
            {
                int at = degree[row] + fill[row];
                fill[row]++;
                columns[at] = other;
                weights[at] = weight;
                leavesOf[at] = leaf;
                facesOf[at] = face;
            }
        }

        /// <summary>
        /// Conjugate gradient with a Jacobi preconditioner.
        /// <para>
        /// Both ends of the problem being rate controlled makes it pure Neumann, so the operator is
        /// singular: adding a constant to the potential changes nothing. The right hand side is in the
        /// range of the operator because the rates balance, so the iteration is well defined, but rounding
        /// lets a constant creep in. Taking the mean out of the residual at every step keeps the iteration
        /// in the subspace where the problem is non-singular, which is cheaper and better conditioned than
        /// pinning a cell.
        /// </para>
        /// </summary>
        private static void SolveConjugateGradient(int[] rowStart, int[] column, double[] value,
                                                   double[] diagonal, double[] rightHand, double[] solution,
                                                   FlowFieldOptions options, out int iterations,
                                                   out double residual, out bool converged)
        {
            int n = solution.Length;
            iterations = 0;
            residual = 0;
            converged = n == 0;
            if (n == 0)
            {
                return;
            }
            double[] r = new double[n];
            double[] z = new double[n];
            double[] p = new double[n];
            double[] ap = new double[n];

            Array.Copy(rightHand, r, n);
            RemoveMean(r);
            double reference = Norm(r);
            if (!(reference > 0))
            {
                converged = true;
                return;
            }
            for (int i = 0; i < n; i++)
            {
                z[i] = diagonal[i] > 0 ? r[i] / diagonal[i] : r[i];
            }
            Array.Copy(z, p, n);
            double rz = Dot(r, z);

            for (int iteration = 1; iteration <= options.MaximumIterationCount; iteration++)
            {
                Multiply(rowStart, column, value, diagonal, p, ap);
                double denominator = Dot(p, ap);
                if (!(System.Math.Abs(denominator) > 0))
                {
                    break;
                }
                double alpha = rz / denominator;
                for (int i = 0; i < n; i++)
                {
                    solution[i] += alpha * p[i];
                    r[i] -= alpha * ap[i];
                }
                RemoveMean(r);
                double norm = Norm(r);
                iterations = iteration;
                residual = norm / reference;
                if (residual <= options.Tolerance)
                {
                    converged = true;
                    break;
                }
                for (int i = 0; i < n; i++)
                {
                    z[i] = diagonal[i] > 0 ? r[i] / diagonal[i] : r[i];
                }
                double next = Dot(r, z);
                double beta = next / rz;
                rz = next;
                for (int i = 0; i < n; i++)
                {
                    p[i] = z[i] + beta * p[i];
                }
            }
            RemoveMean(solution);
        }

        private static void Multiply(int[] rowStart, int[] column, double[] value, double[] diagonal,
                                     double[] x, double[] y)
        {
            for (int row = 0; row < y.Length; row++)
            {
                double sum = diagonal[row] * x[row];
                int to = rowStart[row + 1];
                for (int at = rowStart[row]; at < to; at++)
                {
                    sum -= value[at] * x[column[at]];
                }
                y[row] = sum;
            }
        }

        private static void RemoveMean(double[] x)
        {
            double sum = 0;
            for (int i = 0; i < x.Length; i++)
            {
                sum += x[i];
            }
            double mean = sum / x.Length;
            for (int i = 0; i < x.Length; i++)
            {
                x[i] -= mean;
            }
        }

        private static double Dot(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += a[i] * b[i];
            }
            return sum;
        }

        private static double Norm(double[] a)
        {
            return System.Math.Sqrt(Dot(a, a));
        }

        /// <summary>
        /// how far from divergence free the field came out, cell by cell
        /// </summary>
        private void MeasureImbalance(FlowProblem problem)
        {
            double total = 0;
            foreach (KeyValuePair<int, double> entry in problem.Rates)
            {
                total += System.Math.Abs(entry.Value);
            }
            if (!(total > 0))
            {
                total = 1.0;
            }
            double worst = 0;
            for (int leaf = 0; leaf < Grid.Tree.LeafCount; leaf++)
            {
                if (unknownOf_[leaf] < 0)
                {
                    continue;
                }
                double sum = 0;
                for (int face = 0; face < 6; face++)
                {
                    sum += faceFlux_[6 * leaf + face];
                }
                if (problem.Rates.TryGetValue(leaf, out double rate))
                {
                    sum -= rate;
                }
                double error = System.Math.Abs(sum);
                if (error > worst)
                {
                    worst = error;
                }
            }
            WorstCellImbalance = worst / total;
        }
    }
}
