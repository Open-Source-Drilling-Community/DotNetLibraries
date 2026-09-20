using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// why a traced streamline stopped
    /// </summary>
    public enum TraceOutcome
    {
        /// <summary>
        /// it reached a cell where flow is taken out of the domain
        /// </summary>
        ReachedSink,

        /// <summary>
        /// it ran into a place where the flow stands still, which happens on the upstream face of an
        /// obstacle and on the dividing surfaces that separate what goes one way round from what goes the
        /// other
        /// </summary>
        Stagnated,

        /// <summary>
        /// it left the grid
        /// </summary>
        LeftTheDomain,

        /// <summary>
        /// it crossed more cells than it was allowed to
        /// </summary>
        TooManyCells,

        /// <summary>
        /// it never started, the position given being outside the grid or in a closed cell
        /// </summary>
        BadStart
    }

    /// <summary>
    /// the settings of the tracer
    /// </summary>
    public class StreamlineTracerOptions
    {
        /// <summary>
        /// the largest number of cells one streamline may cross
        /// </summary>
        public int MaximumCellCrossings { get; set; } = 500000;

        /// <summary>
        /// how many positions to record inside each cell. One records only where the streamline enters,
        /// which is enough while the cells are small against the curvature of the path.
        /// </summary>
        public int PositionsPerCell { get; set; } = 1;
    }

    /// <summary>
    /// A streamline traced through a <see cref="FlowField"/> by Pollock's method.
    /// <para>
    /// Inside a cell each component of the velocity is taken to vary linearly between the two opposing
    /// faces, which makes the exit face, the exit point and the time of flight closed form. There is no
    /// step length to choose and no integration error to accumulate: the path inside a cell is the exact
    /// path of that velocity field.
    /// </para>
    /// <para>
    /// Two properties follow from the field being conservative with one flux per face, and both are
    /// relied on downstream. A streamline cannot leak through a face into a closed cell, because the flux
    /// there is exactly zero and the exit time is infinite. And two streamlines cannot cross, because they
    /// are integral curves of one velocity field, which is what <see cref="StreamlineBundler"/> assumes of
    /// its input.
    /// </para>
    /// <para>
    /// Crossing a change of level needs no special case in the integration: the tracer leaves a cell at a
    /// point on a face and asks the octree which leaf holds the position just beyond it, so a coarse cell
    /// handing over to one of four fine ones is the same operation as any other.
    /// </para>
    /// </summary>
    public class StreamlineTracer
    {
        /// <summary>
        /// the settings used
        /// </summary>
        public StreamlineTracerOptions Options { get; set; } = new StreamlineTracerOptions();

        /// <summary>
        /// why the last trace stopped
        /// </summary>
        public TraceOutcome Outcome { get; private set; } = TraceOutcome.BadStart;

        /// <summary>
        /// how many cells the last trace crossed
        /// </summary>
        public int CellsCrossed { get; private set; }

        /// <summary>
        /// the time of flight of the last trace, for a medium of unit storage. Multiply by porosity times
        /// saturation to get the real one: that scalar is the only place it enters, and it does not touch
        /// the geometry at all.
        /// </summary>
        public double TimeOfFlight { get; private set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public StreamlineTracer()
        {
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="options"></param>
        public StreamlineTracer(StreamlineTracerOptions options)
        {
            Options = options ?? new StreamlineTracerOptions();
        }

        /// <summary>
        /// Traces one streamline downstream from the given position.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="problem">its rates say where a streamline is allowed to end</param>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <returns>the positions traced, which is empty when the start was unusable</returns>
        public Streamline Trace(FlowField field, FlowProblem problem,
                                double north, double east, double vertical)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            if (problem == null)
            {
                throw new ArgumentNullException(nameof(problem));
            }
            List<Point3D> positions = new List<Point3D>();
            Outcome = TraceOutcome.BadStart;
            CellsCrossed = 0;
            TimeOfFlight = 0;

            LinearOctree tree = field.Grid.Tree;
            int leaf = tree.FindLeaf(north, east, vertical);
            if (leaf < 0 || !field.IsActive(leaf))
            {
                return new Streamline(positions);
            }

            double[] at = { north, east, vertical };
            double[] low = new double[3];
            double[] velocityLow = new double[3];
            double[] gradient = new double[3];
            int perCell = System.Math.Max(1, Options.PositionsPerCell);

            while (true)
            {
                positions.Add(new Point3D(at[0], at[1], at[2]));
                if (problem.Rates.TryGetValue(leaf, out double rate) && rate < 0)
                {
                    Outcome = TraceOutcome.ReachedSink;
                    break;
                }
                if (CellsCrossed >= Options.MaximumCellCrossings)
                {
                    Outcome = TraceOutcome.TooManyCells;
                    break;
                }

                OctreeCell cell = tree.GetCell(leaf);
                double size = cell.Size;
                double area = size * size;
                low[0] = cell.MinimumNorth;
                low[1] = cell.MinimumEast;
                low[2] = cell.MinimumVertical;
                for (int axis = 0; axis < 3; axis++)
                {
                    // the flux is stored positive outward, so on the low face an outward flow is a
                    // velocity in the negative direction
                    velocityLow[axis] = -field.GetFaceFlux(leaf, axis, -1) / area;
                    double velocityHigh = field.GetFaceFlux(leaf, axis, 1) / area;
                    gradient[axis] = (velocityHigh - velocityLow[axis]) / size;
                }

                int exitAxis = -1;
                int exitStep = 0;
                double exitTime = double.PositiveInfinity;
                for (int axis = 0; axis < 3; axis++)
                {
                    double here = velocityLow[axis] + gradient[axis] * (at[axis] - low[axis]);
                    if (here == 0)
                    {
                        continue;
                    }
                    int step = here > 0 ? 1 : -1;
                    double target = step > 0 ? velocityLow[axis] + gradient[axis] * size : velocityLow[axis];
                    double time;
                    if (System.Math.Abs(gradient[axis]) * size <= 1e-12 * System.Math.Abs(here))
                    {
                        // the component does not vary across the cell, so the travel is uniform
                        double distance = step > 0 ? low[axis] + size - at[axis] : low[axis] - at[axis];
                        time = distance / here;
                    }
                    else
                    {
                        if (target == 0 || target / here <= 0)
                        {
                            // the component dies away before the face is reached: that face is never met
                            continue;
                        }
                        time = System.Math.Log(target / here) / gradient[axis];
                    }
                    if (time > 0 && time < exitTime)
                    {
                        exitTime = time;
                        exitAxis = axis;
                        exitStep = step;
                    }
                }

                if (exitAxis < 0 || double.IsInfinity(exitTime) || double.IsNaN(exitTime))
                {
                    Outcome = TraceOutcome.Stagnated;
                    break;
                }

                // where the streamline has got to after that time, in each direction independently
                for (int sample = 1; sample <= perCell; sample++)
                {
                    double time = exitTime * sample / perCell;
                    double[] moved = new double[3];
                    for (int axis = 0; axis < 3; axis++)
                    {
                        moved[axis] = Advance(at[axis], low[axis], size, velocityLow[axis],
                                              gradient[axis], time);
                    }
                    if (sample < perCell)
                    {
                        positions.Add(new Point3D(moved[0], moved[1], moved[2]));
                    }
                    else
                    {
                        Array.Copy(moved, at, 3);
                    }
                }
                // land exactly on the face rather than a rounding away from it
                at[exitAxis] = exitStep > 0 ? low[exitAxis] + size : low[exitAxis];
                TimeOfFlight += exitTime;
                CellsCrossed++;

                // Stepping a little past the face says which leaf is on the other side, and that is the
                // same operation whether that leaf is the same size, twice as big or half as big. The step
                // is only a way of asking the question: the streamline itself stays exactly on the face,
                // or every recorded position would sit a fraction past the one it crossed.
                double nudge = 1.0e-4 * size;
                double[] beyond = { at[0], at[1], at[2] };
                beyond[exitAxis] += exitStep * nudge;
                int next = tree.FindLeaf(beyond[0], beyond[1], beyond[2]);
                if (next < 0)
                {
                    Outcome = TraceOutcome.LeftTheDomain;
                    break;
                }
                if (!field.IsActive(next))
                {
                    // cannot happen while the field is conservative, the flux into a closed cell being
                    // exactly zero, but a streamline that ended up inside one would be a silent breach of
                    // the clearance the whole method exists to guarantee
                    Outcome = TraceOutcome.Stagnated;
                    break;
                }
                leaf = next;
            }
            return new Streamline(positions);
        }

        /// <summary>
        /// where one component has got to after the given time, under a velocity varying linearly across
        /// the cell
        /// </summary>
        private static double Advance(double position, double low, double size, double velocityLow,
                                      double gradient, double time)
        {
            double here = velocityLow + gradient * (position - low);
            if (System.Math.Abs(gradient) * size <= 1e-12 * System.Math.Max(System.Math.Abs(here), 1e-300))
            {
                return position + here * time;
            }
            double moved = low + (here * System.Math.Exp(gradient * time) - velocityLow) / gradient;
            return moved;
        }
    }
}
