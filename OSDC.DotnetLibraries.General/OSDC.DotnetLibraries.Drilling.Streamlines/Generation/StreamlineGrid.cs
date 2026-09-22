using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// what a cell is, as far as the flow is concerned
    /// </summary>
    public enum CellState : byte
    {
        /// <summary>
        /// the flow can pass through it
        /// </summary>
        Open = 0,

        /// <summary>
        /// it touches the uncertainty volume of an existing well, so nothing may pass
        /// </summary>
        Blocked = 1
    }

    /// <summary>
    /// whether a planned well can be generated at all
    /// </summary>
    public enum StreamlineGridStatus
    {
        /// <summary>
        /// source and target are open and joined by open cells
        /// </summary>
        Connected,

        /// <summary>
        /// a source sits inside an obstacle. For a tie-in this usually means the parent was not muted far
        /// enough either side of the window.
        /// </summary>
        SourceBlocked,

        /// <summary>
        /// no open cell of the target could be found, either because the region is inside an obstacle or
        /// because it is thinner than the cells there
        /// </summary>
        TargetBlocked,

        /// <summary>
        /// both ends are open but no path of open cells joins them
        /// </summary>
        NotConnected
    }

    /// <summary>
    /// The octree a planned well is generated on, with every cell marked open or blocked, and the
    /// question of whether the target can be reached at all answered before any flow is computed.
    /// <para>
    /// The refinement follows the obstacles rather than a fixed schedule: a cell is divided until it is a
    /// small enough fraction of the passage it sits in. That makes the cells finest exactly where two
    /// wells run close and coarsest where there is nothing, which for a cluster means the top of the hole
    /// rather than the bottom, since near surface the wells are metres apart while the uncertainty is
    /// still small.
    /// </para>
    /// <para>
    /// A cell counts as blocked as soon as it touches an uncertainty volume, not only when its centre is
    /// inside one. The classification is therefore one sided: a path through open cells certainly clears
    /// the true volumes, and what resolution buys is not safety but the avoidance of a false refusal.
    /// </para>
    /// </summary>
    public class StreamlineGrid
    {
        /// <summary>
        /// the cells
        /// </summary>
        public LinearOctree Tree { get; private set; }

        /// <summary>
        /// what each leaf is, indexed as the tree indexes its leaves
        /// </summary>
        public CellState[] States { get; private set; }

        /// <summary>
        /// the leaf each source starts in, in the order the sources were given, or -1
        /// </summary>
        public int[] SourceLeaves { get; private set; } = Array.Empty<int>();

        /// <summary>
        /// the open leaves that lie in the target region
        /// </summary>
        public List<int> SinkLeaves { get; private set; } = new List<int>();

        /// <summary>
        /// whether the planned well can be generated
        /// </summary>
        public StreamlineGridStatus Status { get; private set; } = StreamlineGridStatus.NotConnected;

        /// <summary>
        /// the lowest corner of the box the octree covers
        /// </summary>
        public double[] DomainMinimum { get; private set; } = new double[3];

        /// <summary>
        /// the highest corner of the box the octree covers
        /// </summary>
        public double[] DomainMaximum { get; private set; } = new double[3];

        /// <summary>
        /// how many leaves are blocked
        /// </summary>
        public int BlockedCount { get; private set; }

        /// <summary>
        /// how many open leaves the source can reach
        /// </summary>
        public int ReachableCount { get; private set; }

        private StreamlineGrid(LinearOctree tree, CellState[] states)
        {
            Tree = tree;
            States = states;
        }

        /// <summary>
        /// Builds the octree for one set of sources and one target.
        /// <para>
        /// Muting is applied to the parent wells named by the sources, which changes those
        /// <see cref="WellboreUncertainty"/> objects. Sources that need different muting cannot share a
        /// grid and have to be built, and later solved, separately.
        /// </para>
        /// </summary>
        /// <param name="obstacles"></param>
        /// <param name="sources"></param>
        /// <param name="target"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static StreamlineGrid Build(ObstacleField obstacles, IReadOnlyList<StreamlineSource> sources,
                                           TargetPolygon target, StreamlineGridOptions? options = null)
        {
            if (obstacles == null)
            {
                throw new ArgumentNullException(nameof(obstacles));
            }
            if (sources == null || sources.Count == 0)
            {
                throw new ArgumentException("at least one source is needed", nameof(sources));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            options ??= new StreamlineGridOptions();
            if (!options.IsValid(out string? reason))
            {
                throw new ArgumentException(reason, nameof(options));
            }
            foreach (StreamlineSource source in sources)
            {
                if (!source.IsValid())
                {
                    throw new ArgumentException("a source has no position or no direction", nameof(sources));
                }
            }

            ApplyMuting(obstacles, sources);
            obstacles.BuildIndex();

            double[] minimum = { double.MaxValue, double.MaxValue, double.MaxValue };
            double[] maximum = { double.MinValue, double.MinValue, double.MinValue };
            foreach (StreamlineSource source in sources)
            {
                Extend(minimum, maximum, source.Position!.X!.Value, source.Position.Y!.Value,
                       source.Position.Z!.Value);
            }
            double[] centre = { 0, 0, 0 };
            for (int a = 0; a < 3; a++)
            {
                centre[a] = 0.5 * (minimum[a] + maximum[a]);
            }
            for (int a = 0; a < 3; a++)
            {
                if (target.BoundingBoxMinimum[a] < minimum[a]) { minimum[a] = target.BoundingBoxMinimum[a]; }
                if (target.BoundingBoxMaximum[a] > maximum[a]) { maximum[a] = target.BoundingBoxMaximum[a]; }
            }
            Point3D targetCentre = target.GetCentre();
            double span = System.Math.Sqrt(Squared(targetCentre.X!.Value - centre[0])
                                           + Squared(targetCentre.Y!.Value - centre[1])
                                           + Squared(targetCentre.Z!.Value - centre[2]));
            double margin = System.Math.Max(options.MinimumMargin, options.MarginFraction * span);
            for (int a = 0; a < 3; a++)
            {
                minimum[a] -= margin;
                maximum[a] += margin;
            }

            OctreeFrame frame = OctreeFrame.Covering(minimum, maximum, options.FinestCellSize,
                                                     out int deepestDepth);
            int baseDepth = deepestDepth;
            while (baseDepth > 0 && frame.GetCellSize(baseDepth) < options.CoarsestCellSize)
            {
                baseDepth--;
            }

            double sourceRadius = options.SourceRefinementRadius * options.FinestCellSize;
            double[] landingMinimum = new double[3];
            double[] landingMaximum = new double[3];
            target.GetLandingBox(landingMinimum, landingMaximum);
            LinearOctree tree = LinearOctree.Build(frame, minimum, maximum, baseDepth, deepestDepth,
                (in OctreeCell cell) => ShouldRefine(in cell, obstacles, sources, target, options,
                                                     sourceRadius, landingMinimum, landingMaximum));

            // Whether a cell is blocked depends on that cell and on the obstacles, which are indexed
            // above and not written to here, so the leaves can be classified in any order and in
            // parallel. It is worth doing: there is one obstacle query per leaf and the better part of
            // a million leaves.
            CellState[] states = new CellState[tree.LeafCount];
            System.Threading.Tasks.Parallel.For(0, tree.LeafCount, leaf =>
            {
                OctreeCell cell = tree.GetCell(leaf);
                // wholly above the ceiling or wholly below the floor, so no part of a planned well
                // may be there
                if ((options.CeilingVertical != null
                     && cell.MinimumVertical + cell.Size <= options.CeilingVertical.Value)
                    || (options.FloorVertical != null
                        && cell.MinimumVertical >= options.FloorVertical.Value))
                {
                    states[leaf] = CellState.Blocked;
                    return;
                }
                if (obstacles.IsBlocked(in cell))
                {
                    states[leaf] = CellState.Blocked;
                }
            });
            // counted afterwards rather than under a lock, which is both cheaper and free of any
            // question about the order things were added in
            int blocked = 0;
            for (int leaf = 0; leaf < states.Length; leaf++)
            {
                if (states[leaf] == CellState.Blocked) { blocked++; }
            }

            StreamlineGrid grid = new StreamlineGrid(tree, states)
            {
                DomainMinimum = minimum,
                DomainMaximum = maximum,
                BlockedCount = blocked
            };
            grid.Locate(sources, target);
            grid.CheckConnectivity();
            return grid;
        }

        /// <summary>
        /// stops a parent well from being an obstacle over a short interval around the window a sidetrack
        /// leaves it by
        /// </summary>
        private static void ApplyMuting(ObstacleField obstacles, IReadOnlyList<StreamlineSource> sources)
        {
            foreach (StreamlineSource source in sources)
            {
                if (source.ParentWell < 0 || source.ParentWell >= obstacles.Wells.Count)
                {
                    continue;
                }
                WellboreUncertainty parent = obstacles.Wells[source.ParentWell];
                double from = source.ParentMeasuredDepth - source.ParentMutedLength;
                double to = source.ParentMeasuredDepth + source.ParentMutedLength;
                parent.MutedFrom = parent.MutedFrom == null ? from : System.Math.Min(parent.MutedFrom.Value, from);
                parent.MutedTo = parent.MutedTo == null ? to : System.Math.Max(parent.MutedTo.Value, to);
            }
        }

        private static bool ShouldRefine(in OctreeCell cell, ObstacleField obstacles,
                                         IReadOnlyList<StreamlineSource> sources, TargetPolygon target,
                                         StreamlineGridOptions options, double sourceRadius,
                                         double[] landingMinimum, double[] landingMaximum)
        {
            if (cell.Size <= options.FinestCellSize * 1.0001)
            {
                return false;
            }
            // The target has to be resolved or no cell centre will ever land inside a thin prism, and
            // the same size has to carry along the landing sections, so that a conduit does not meet a
            // change of level partway and pinch itself down to one of the four sub-faces there.
            if (cell.Size > target.Thickness && cell.Intersects(landingMinimum, landingMaximum))
            {
                return true;
            }
            // and so does the immediate neighbourhood of every source
            foreach (StreamlineSource source in sources)
            {
                double distance = System.Math.Sqrt(
                    Squared(cell.CentreNorth - source.Position!.X!.Value)
                    + Squared(cell.CentreEast - source.Position.Y!.Value)
                    + Squared(cell.CentreVertical - source.Position.Z!.Value));
                if (distance < sourceRadius + cell.CircumscribedRadius)
                {
                    return true;
                }
            }

            // A cell of this size divides only if the passage it sits in is narrower than the size
            // divided by the fraction allowed, so obstacles further off than that cannot change the
            // answer and are not worth looking for. Tying the search to the cell rather than to the
            // coarsest cell in the grid is what keeps the cost of the fine levels down, and they are
            // where nearly all the cells are.
            double searchLimit = cell.Size / options.ClearanceCellFraction;
            ObstacleProximity proximity = obstacles.GetProximity(cell.CentreNorth, cell.CentreEast,
                                                                 cell.CentreVertical, searchLimit);
            if (!proximity.HasObstacle)
            {
                return false;
            }
            double reach = cell.CircumscribedRadius;
            if (proximity.NearestExcess < -reach)
            {
                // solidly inside an obstacle: nothing passes through it, so its inside needs no detail
                return false;
            }
            double required = proximity.Clearance * options.ClearanceCellFraction;
            if (System.Math.Abs(proximity.NearestExcess) <= reach && proximity.NearestSemiMinorAxis > 0)
            {
                double surface = proximity.NearestSemiMinorAxis * options.SurfaceCellFraction;
                if (surface < required)
                {
                    required = surface;
                }
            }
            if (required < options.FinestCellSize) { required = options.FinestCellSize; }
            if (required > options.CoarsestCellSize) { required = options.CoarsestCellSize; }
            return cell.Size > required * 1.0001;
        }

        private void Locate(IReadOnlyList<StreamlineSource> sources, TargetPolygon target)
        {
            SourceLeaves = new int[sources.Count];
            for (int s = 0; s < sources.Count; s++)
            {
                Point3D position = sources[s].Position!;
                SourceLeaves[s] = Tree.FindLeaf(position.X!.Value, position.Y!.Value, position.Z!.Value);
            }
            SinkLeaves = new List<int>();
            for (int leaf = 0; leaf < Tree.LeafCount; leaf++)
            {
                if (States[leaf] != CellState.Open)
                {
                    continue;
                }
                OctreeCell cell = Tree.GetCell(leaf);
                if (target.Contains(cell.CentreNorth, cell.CentreEast, cell.CentreVertical))
                {
                    SinkLeaves.Add(leaf);
                }
            }
        }

        /// <summary>
        /// Whether open cells join the sources to the target, by flooding out from the sources.
        /// <para>
        /// This is cheap next to a flow solve and it separates two answers that would otherwise look the
        /// same: a geometry with no way through, and a solver that has gone wrong.
        /// </para>
        /// </summary>
        private void CheckConnectivity()
        {
            Assess(null);
        }

        /// <summary>
        /// Re-answers whether the target can be reached, once the conduits and the shut faces of a
        /// problem are taken into account.
        /// <para>
        /// The first pass, run while the grid is built, knows only which cells are open. A perpendicular
        /// arrival, or a single admitted face, shuts faces between cells that are both open, which that
        /// pass cannot see, so a grid it called connected may still have no way through. This is what
        /// turns that into an honest refusal rather than a solve that returns nothing.
        /// </para>
        /// </summary>
        /// <param name="problem"></param>
        public void Reassess(FlowProblem problem)
        {
            if (problem == null)
            {
                throw new ArgumentNullException(nameof(problem));
            }
            Assess(problem);
        }

        private void Assess(FlowProblem? problem)
        {
            bool anySource = false;
            foreach (int leaf in SourceLeaves)
            {
                if (leaf >= 0 && States[leaf] == CellState.Open)
                {
                    anySource = true;
                }
            }
            if (!anySource)
            {
                Status = StreamlineGridStatus.SourceBlocked;
                return;
            }
            if (SinkLeaves.Count == 0)
            {
                Status = StreamlineGridStatus.TargetBlocked;
                return;
            }

            // a cell of a conduit reaches only the two cells that continue its chain
            Dictionary<int, (int Previous, int Next)> chain = new Dictionary<int, (int, int)>();
            if (problem != null)
            {
                foreach (FlowConduit conduit in problem.Conduits)
                {
                    int[] cells = conduit.Cells;
                    for (int k = conduit.OpenAtHead ? 1 : 0; k < cells.Length - 1; k++)
                    {
                        chain[cells[k]] = (k > 0 ? cells[k - 1] : -1, cells[k + 1]);
                    }
                }
            }

            bool[] seen = new bool[Tree.LeafCount];
            Queue<int> pending = new Queue<int>();
            foreach (int leaf in SourceLeaves)
            {
                if (leaf >= 0 && States[leaf] == CellState.Open && !seen[leaf])
                {
                    seen[leaf] = true;
                    pending.Enqueue(leaf);
                }
            }
            int reached = 0;
            List<int> neighbours = new List<int>();
            while (pending.Count > 0)
            {
                int leaf = pending.Dequeue();
                reached++;
                bool restricted = chain.TryGetValue(leaf, out (int Previous, int Next) link);
                for (int axis = 0; axis < 3; axis++)
                {
                    for (int step = -1; step <= 1; step += 2)
                    {
                        if (problem != null && problem.IsFaceClosed(leaf, axis, step))
                        {
                            continue;
                        }
                        neighbours.Clear();
                        Tree.GetFaceNeighbours(leaf, axis, step, neighbours);
                        foreach (int other in neighbours)
                        {
                            if (seen[other] || States[other] != CellState.Open)
                            {
                                continue;
                            }
                            if (problem != null && problem.IsFaceClosed(other, axis, -step))
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
                            seen[other] = true;
                            pending.Enqueue(other);
                        }
                    }
                }
            }
            ReachableCount = reached;
            foreach (int leaf in SinkLeaves)
            {
                if (seen[leaf])
                {
                    Status = StreamlineGridStatus.Connected;
                    return;
                }
            }
            Status = StreamlineGridStatus.NotConnected;
        }

        private static void Extend(double[] minimum, double[] maximum, double north, double east,
                                   double vertical)
        {
            double[] values = { north, east, vertical };
            for (int a = 0; a < 3; a++)
            {
                if (values[a] < minimum[a]) { minimum[a] = values[a]; }
                if (values[a] > maximum[a]) { maximum[a] = values[a]; }
            }
        }

        private static double Squared(double value)
        {
            return value * value;
        }

        /// <summary>
        /// a one line account of the grid, for a log or an assertion message
        /// </summary>
        public string Describe()
        {
            int[] histogram = Tree.GetDepthHistogram();
            List<string> levels = new List<string>();
            for (int depth = 0; depth < histogram.Length; depth++)
            {
                if (histogram[depth] > 0)
                {
                    levels.Add($"{Tree.Frame.GetCellSize(depth):G3}m:{histogram[depth]}");
                }
            }
            return $"{Tree.LeafCount} leaves [{string.Join(" ", levels)}], {BlockedCount} blocked, "
                   + $"{SinkLeaves.Count} target cells, {ReachableCount} reachable, {Status}";
        }
    }
}
