namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// The existing wells a planned well has to avoid, indexed so that the questions the octree asks
    /// millions of times can be answered quickly: how far outside every obstacle is this position, and
    /// how wide is the passage here.
    /// <para>
    /// The index is a uniform bucket grid over the trajectory samples of every well. A query expands
    /// outward over the buckets and stops as soon as no unexamined sample could beat what has been found,
    /// which it can decide because a sample at distance d contributes at best d minus the largest
    /// semi-axis in the field.
    /// </para>
    /// </summary>
    public class ObstacleField : IForbiddenZoneField
    {
        private readonly List<WellboreUncertainty> wells_ = new List<WellboreUncertainty>();
        private readonly double[] origin_ = new double[3];
        private double bucketSize_ = 1.0;
        private int countNorth_ = 1;
        private int countEast_ = 1;
        private int countVertical_ = 1;
        private int[] bucketStart_ = Array.Empty<int>();
        private int[] bucketWell_ = Array.Empty<int>();
        private int[] bucketSample_ = Array.Empty<int>();
        private bool indexed_;

        /// <summary>
        /// how many buckets the index is allowed, which at four bytes each bounds what it costs
        /// </summary>
        private const long MaximumBucketCount = 20_000_000L;

        /// <summary>
        /// the largest semi-major axis anywhere in the field
        /// </summary>
        public double MaximumSemiAxis { get; private set; }

        /// <summary>
        /// the lowest corner of the box holding every obstacle
        /// </summary>
        public double[] BoundingBoxMinimum { get; } = { double.MaxValue, double.MaxValue, double.MaxValue };

        /// <summary>
        /// the highest corner of the box holding every obstacle
        /// </summary>
        public double[] BoundingBoxMaximum { get; } = { double.MinValue, double.MinValue, double.MinValue };

        /// <summary>
        /// the wells held
        /// </summary>
        public IReadOnlyList<WellboreUncertainty> Wells
        {
            get
            {
                return wells_;
            }
        }

        /// <summary>
        /// adds one well. The index is rebuilt on the next query.
        /// </summary>
        /// <param name="well"></param>
        public void Add(WellboreUncertainty well)
        {
            if (well == null)
            {
                throw new ArgumentNullException(nameof(well));
            }
            wells_.Add(well);
            indexed_ = false;
        }

        /// <summary>
        /// builds the bucket index. Called on demand, but exposed so the cost can be paid deliberately.
        /// </summary>
        public void BuildIndex()
        {
            MaximumSemiAxis = 0;
            for (int a = 0; a < 3; a++)
            {
                BoundingBoxMinimum[a] = double.MaxValue;
                BoundingBoxMaximum[a] = double.MinValue;
            }
            long samples = 0;
            foreach (WellboreUncertainty well in wells_)
            {
                samples += well.SampleCount;
                if (well.MaximumSemiAxis > MaximumSemiAxis)
                {
                    MaximumSemiAxis = well.MaximumSemiAxis;
                }
                for (int a = 0; a < 3; a++)
                {
                    if (well.BoundingBoxMinimum[a] < BoundingBoxMinimum[a])
                    {
                        BoundingBoxMinimum[a] = well.BoundingBoxMinimum[a];
                    }
                    if (well.BoundingBoxMaximum[a] > BoundingBoxMaximum[a])
                    {
                        BoundingBoxMaximum[a] = well.BoundingBoxMaximum[a];
                    }
                }
            }
            if (samples == 0)
            {
                indexed_ = true;
                bucketStart_ = new int[1];
                return;
            }

            // The bucket has to be sized from how closely the samples are spaced along a well, not from
            // their average density over the box. The two differ enormously: the samples sit on curves,
            // so near a cluster a bucket sized for the average holds every sample of every well passing
            // through it, and a query for a half metre cell then walks thousands of them.
            double volume = 1.0;
            for (int a = 0; a < 3; a++)
            {
                origin_[a] = BoundingBoxMinimum[a];
                volume *= System.Math.Max(1.0, BoundingBoxMaximum[a] - BoundingBoxMinimum[a]);
            }
            double step = 0;
            foreach (WellboreUncertainty well in wells_)
            {
                if (well.SampleStep > step)
                {
                    step = well.SampleStep;
                }
            }
            bucketSize_ = 2.0 * step;
            double affordable = System.Math.Pow(volume / MaximumBucketCount, 1.0 / 3.0);
            if (bucketSize_ < affordable)
            {
                bucketSize_ = affordable;
            }
            if (!(bucketSize_ > 0))
            {
                bucketSize_ = 1.0;
            }
            countNorth_ = Extent(0);
            countEast_ = Extent(1);
            countVertical_ = Extent(2);
            long total = (long)countNorth_ * countEast_ * countVertical_;
            while (total > MaximumBucketCount)
            {
                bucketSize_ *= 1.5;
                countNorth_ = Extent(0);
                countEast_ = Extent(1);
                countVertical_ = Extent(2);
                total = (long)countNorth_ * countEast_ * countVertical_;
            }

            int buckets = (int)total;
            bucketStart_ = new int[buckets + 1];
            for (int w = 0; w < wells_.Count; w++)
            {
                WellboreUncertainty well = wells_[w];
                for (int s = 0; s < well.SampleCount; s++)
                {
                    well.GetSample(s, out double n, out double e, out double v);
                    bucketStart_[BucketOf(n, e, v) + 1]++;
                }
            }
            for (int b = 0; b < buckets; b++)
            {
                bucketStart_[b + 1] += bucketStart_[b];
            }
            bucketWell_ = new int[bucketStart_[buckets]];
            bucketSample_ = new int[bucketStart_[buckets]];
            int[] fill = new int[buckets];
            for (int w = 0; w < wells_.Count; w++)
            {
                WellboreUncertainty well = wells_[w];
                for (int s = 0; s < well.SampleCount; s++)
                {
                    well.GetSample(s, out double n, out double e, out double v);
                    int bucket = BucketOf(n, e, v);
                    int at = bucketStart_[bucket] + fill[bucket];
                    fill[bucket]++;
                    bucketWell_[at] = w;
                    bucketSample_[at] = s;
                }
            }
            indexed_ = true;
        }

        private int Extent(int axis)
        {
            double span = BoundingBoxMaximum[axis] - BoundingBoxMinimum[axis];
            return System.Math.Max(1, (int)(span / bucketSize_) + 1);
        }

        private int BucketOf(double north, double east, double vertical)
        {
            int i = Clamp((int)((north - origin_[0]) / bucketSize_), countNorth_);
            int j = Clamp((int)((east - origin_[1]) / bucketSize_), countEast_);
            int k = Clamp((int)((vertical - origin_[2]) / bucketSize_), countVertical_);
            return (i * countEast_ + j) * countVertical_ + k;
        }

        /// <summary>
        /// the bucket index a displacement from the origin falls in, rounding down rather than toward
        /// zero so that it stays right on the far side of the origin
        /// </summary>
        private long Floor(double displacement)
        {
            return (long)System.Math.Floor(displacement / bucketSize_);
        }

        private static int Clamp(int value, int extent)
        {
            return value < 0 ? 0 : value >= extent ? extent - 1 : value;
        }

        /// <summary>
        /// How far outside the two nearest distinct obstacles the given position lies. Negative means the
        /// position is inside that obstacle. The sum of the two is the width of the passage there, which
        /// is what the refinement of the octree is driven by.
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <param name="searchLimit">how far it is worth looking; beyond this the answer is reported as
        /// positive infinity, because a passage that wide needs no attention</param>
        /// <param name="nearest">the excess of the nearest obstacle</param>
        /// <param name="nearestWell">which well that is, or -1</param>
        /// <param name="second">the excess of the nearest obstacle of a different well</param>
        /// <param name="secondWell">which well that is, or -1</param>
        public void GetTwoNearest(double north, double east, double vertical, double searchLimit,
                                  out double nearest, out int nearestWell,
                                  out double second, out int secondWell)
        {
            ObstacleProximity proximity = GetProximity(north, east, vertical, searchLimit);
            nearest = proximity.NearestExcess;
            nearestWell = proximity.NearestWell;
            second = proximity.SecondExcess;
            secondWell = proximity.SecondWell;
        }

        /// <summary>
        /// How the obstacles look from the given position: the two nearest belonging to different wells,
        /// and the scale of the nearest one.
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <param name="searchLimit">how far it is worth looking; a passage wider than this needs no
        /// attention and is reported as unbounded</param>
        /// <returns></returns>
        public ObstacleProximity GetProximity(double north, double east, double vertical, double searchLimit)
        {
            if (!indexed_)
            {
                BuildIndex();
            }
            double nearest = double.PositiveInfinity;
            double second = double.PositiveInfinity;
            int nearestWell = -1;
            int secondWell = -1;
            double nearestSection = 0;
            if (bucketWell_.Length == 0)
            {
                return new ObstacleProximity(nearest, -1, 0, second, -1);
            }

            // Everything that could matter lies within the search limit of the surface of some
            // obstacle, and a surface within that limit belongs to an axis within the limit plus the
            // largest semi-axis in the field. So the box to scan is known up front and there is no need
            // to feel outward for it: expanding ring by ring re-walked the whole cube at every ring and
            // cost far more than the samples it found.
            double reach = searchLimit + MaximumSemiAxis;
            int spread = (int)(reach / bucketSize_) + 1;
            // The centre bucket must not be clamped before the range is taken from it. A position outside
            // the box holding the obstacles then has its range measured from the edge rather than from
            // where it actually is, and the range no longer reaches as far as it was meant to; with
            // coarse buckets that was hidden because the range overshot anyway.
            long ci = Floor(north - origin_[0]);
            long cj = Floor(east - origin_[1]);
            long ck = Floor(vertical - origin_[2]);
            int fromI = (int)System.Math.Max(0L, ci - spread);
            int toI = (int)System.Math.Min(countNorth_ - 1L, ci + spread);
            int fromJ = (int)System.Math.Max(0L, cj - spread);
            int toJ = (int)System.Math.Min(countEast_ - 1L, cj + spread);
            int fromK = (int)System.Math.Max(0L, ck - spread);
            int toK = (int)System.Math.Min(countVertical_ - 1L, ck + spread);
            if (fromI > toI || fromJ > toJ || fromK > toK)
            {
                return new ObstacleProximity(nearest, nearestWell, nearestSection, second, secondWell);
            }
            for (int i = fromI; i <= toI; i++)
            {
                int rowI = i * countEast_;
                for (int j = fromJ; j <= toJ; j++)
                {
                    int rowJ = (rowI + j) * countVertical_;
                    int first = bucketStart_[rowJ + fromK];
                    int last = bucketStart_[rowJ + toK + 1];
                    for (int at = first; at < last; at++)
                    {
                        int w = bucketWell_[at];
                        int sample = bucketSample_[at];
                        double excess = wells_[w].GetRadialExcess(sample, north, east, vertical);
                        bool wasNearest = nearestWell == w && excess < nearest;
                        bool becomesNearest = nearestWell != w && excess < nearest;
                        Offer(w, excess, ref nearest, ref nearestWell, ref second, ref secondWell);
                        if ((wasNearest || becomesNearest) && nearestWell == w)
                        {
                            nearestSection = wells_[w].GetSemiMinorAxis(sample);
                        }
                    }
                }
            }
            return new ObstacleProximity(nearest, nearestWell, nearestSection, second, secondWell);
        }

        /// <summary>
        /// keeps the two smallest excesses coming from two different wells
        /// </summary>
        private static void Offer(int well, double excess, ref double nearest, ref int nearestWell,
                                  ref double second, ref int secondWell)
        {
            if (well == nearestWell)
            {
                if (excess < nearest) { nearest = excess; }
            }
            else if (well == secondWell)
            {
                if (excess < second) { second = excess; }
            }
            else if (excess < nearest)
            {
                second = nearest;
                secondWell = nearestWell;
                nearest = excess;
                nearestWell = well;
            }
            else if (excess < second)
            {
                second = excess;
                secondWell = well;
            }
            if (nearest > second)
            {
                (nearest, second) = (second, nearest);
                (nearestWell, secondWell) = (secondWell, nearestWell);
            }
        }

        /// <summary>
        /// how far outside the nearest obstacle the given position lies, negative inside
        /// </summary>
        public double GetNearestExcess(double north, double east, double vertical, double searchLimit)
        {
            GetTwoNearest(north, east, vertical, searchLimit, out double nearest, out int _,
                          out double _, out int _);
            return nearest;
        }

        /// <summary>
        /// the same as <see cref="GetNearestExcess"/>, under the name a tolerance region asks for it by:
        /// what a well may not enter, whatever kind of thing is doing the forbidding
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <param name="searchLimit"></param>
        /// <returns></returns>
        public double GetClearance(double north, double east, double vertical, double searchLimit)
        {
            return GetNearestExcess(north, east, vertical, searchLimit);
        }

        /// <summary>
        /// Whether a cell is blocked, conservatively: a cell counts as blocked as soon as it touches an
        /// obstacle at all, not only when its centre is inside one, so that a path through cells that are
        /// not blocked is guaranteed to stay outside the true volumes.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public bool IsBlocked(in OctreeCell cell)
        {
            double radius = cell.CircumscribedRadius;
            return GetNearestExcess(cell.CentreNorth, cell.CentreEast, cell.CentreVertical, radius) < radius;
        }
    }
}
