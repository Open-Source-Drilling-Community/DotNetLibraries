namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// An ordered chain of cells along which flow is confined.
    /// <para>
    /// Every cell of the chain but the last is closed on all its other faces. Whether the first one is
    /// closed on its outward side too is what separates the two ends of a well: at a source the head is
    /// where flow is injected and has to be sealed, while at a target the chain runs outward from the
    /// sink and its far end has to stay open for flow to enter.
    /// </para>
    /// </summary>
    public class FlowConduit
    {
        /// <summary>
        /// the leaves, in order
        /// </summary>
        public int[] Cells { get; }

        /// <summary>
        /// whether the first cell keeps the rest of its faces, rather than being sealed but for the one
        /// continuing the chain
        /// </summary>
        public bool OpenAtHead { get; }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="openAtHead"></param>
        public FlowConduit(int[] cells, bool openAtHead)
        {
            Cells = cells;
            OpenAtHead = openAtHead;
        }
    }

    /// <summary>
    /// What is to be solved on a <see cref="StreamlineGrid"/>: where flow is put in, where it is taken
    /// out, and which cells form a conduit that holds the direction a planned well leaves in.
    /// <para>
    /// Both ends are rate controlled rather than potential controlled. That is what makes the streamlines
    /// divide equally: a fixed rate per sink cell means each receives the same share whatever the medium
    /// between, so launching streamlines of equal flux lands equal numbers in each. With a single
    /// equipotential sink the arrival distribution would be whatever the geometry dictated and could not
    /// be prescribed.
    /// </para>
    /// <para>
    /// Rates therefore have to balance, and the resulting problem is pure Neumann: the potential is
    /// determined only up to a constant, which the solver handles rather than the caller.
    /// </para>
    /// </summary>
    public class FlowProblem
    {
        private readonly Dictionary<int, double> rates_ = new Dictionary<int, double>();
        private readonly List<FlowConduit> conduits_ = new List<FlowConduit>();
        private readonly HashSet<int> closedFaces_ = new HashSet<int>();

        /// <summary>
        /// the grid the problem lives on
        /// </summary>
        public StreamlineGrid Grid { get; }

        /// <summary>
        /// How easily flow crosses a face, as a multiple of the background, or null for a medium that is
        /// the same everywhere and in every direction.
        /// <para>
        /// This is the soft way of imposing a direction, next to the hard way a conduit gives. It costs
        /// exactness and buys a turn that is spread over a length instead of taken at a face.
        /// </para>
        /// </summary>
        public IFaceMobility? Mobility { get; set; } = null;

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="grid"></param>
        public FlowProblem(StreamlineGrid grid)
        {
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
        }

        /// <summary>
        /// the rate at every leaf that has one, positive into the domain
        /// </summary>
        public IReadOnlyDictionary<int, double> Rates
        {
            get
            {
                return rates_;
            }
        }

        /// <summary>
        /// the conduits, each an ordered chain of leaves
        /// </summary>
        public IReadOnlyList<FlowConduit> Conduits
        {
            get
            {
                return conduits_;
            }
        }

        /// <summary>
        /// the faces no flow may cross, each coded as six times the leaf plus twice the axis plus one for
        /// the high side
        /// </summary>
        public IReadOnlyCollection<int> ClosedFaces
        {
            get
            {
                return closedFaces_;
            }
        }

        /// <summary>
        /// adds a rate at a leaf, positive into the domain. Adding twice accumulates.
        /// </summary>
        /// <param name="leaf"></param>
        /// <param name="rate"></param>
        public void AddRate(int leaf, double rate)
        {
            if (leaf < 0 || leaf >= Grid.Tree.LeafCount)
            {
                throw new ArgumentOutOfRangeException(nameof(leaf));
            }
            rates_[leaf] = rates_.TryGetValue(leaf, out double already) ? already + rate : rate;
        }

        /// <summary>
        /// Adds a conduit: an ordered chain of leaves along which flow is confined.
        /// <para>
        /// Every cell of the chain but the last is closed on all its other faces, so whatever enters at
        /// the head has nowhere to go but along it. That is what makes the direction a planned well leaves
        /// in a hard constraint rather than a preference: a permeability contrast would only bias the
        /// flow, whereas a closed face leaves it no choice. The last cell of the chain is left open so the
        /// flow can rejoin the medium.
        /// </para>
        /// </summary>
        /// <param name="chain"></param>
        /// <param name="openAtHead">
        /// whether the first cell keeps its other faces. False for a source, where the head is where flow
        /// is injected; true for a target, where the chain runs outward from the sink and flow has to be
        /// able to enter at the far end.
        /// </param>
        public void AddConduit(IReadOnlyList<int> chain, bool openAtHead = false)
        {
            if (chain == null || chain.Count < 2)
            {
                throw new ArgumentException("a conduit needs at least two cells", nameof(chain));
            }
            conduits_.Add(new FlowConduit(chain.ToArray(), openAtHead));
        }

        /// <summary>
        /// Shuts one face of one cell, so that no flow crosses it.
        /// <para>
        /// This is how a target admits only one of its faces: the cells of the region keep the faces on
        /// the admitted side and lose the rest, which leaves the streamlines only one way in without
        /// saying anything about the angle they come in at.
        /// </para>
        /// </summary>
        /// <param name="leaf"></param>
        /// <param name="axis">0 north, 1 east, 2 vertical</param>
        /// <param name="step">-1 for the low face, 1 for the high one</param>
        public void CloseFace(int leaf, int axis, int step)
        {
            if (leaf < 0 || leaf >= Grid.Tree.LeafCount)
            {
                throw new ArgumentOutOfRangeException(nameof(leaf));
            }
            if (axis < 0 || axis > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(axis));
            }
            closedFaces_.Add(6 * leaf + 2 * axis + (step > 0 ? 1 : 0));
        }

        /// <summary>
        /// whether the given face has been shut
        /// </summary>
        /// <param name="leaf"></param>
        /// <param name="axis"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public bool IsFaceClosed(int leaf, int axis, int step)
        {
            return closedFaces_.Count > 0
                   && closedFaces_.Contains(6 * leaf + 2 * axis + (step > 0 ? 1 : 0));
        }

        /// <summary>
        /// how far the rates are from balancing. A pure Neumann problem has no solution unless this is
        /// zero, so the solver checks it.
        /// </summary>
        public double GetRateImbalance()
        {
            double total = 0;
            foreach (KeyValuePair<int, double> entry in rates_)
            {
                total += entry.Value;
            }
            return total;
        }
    }
}
