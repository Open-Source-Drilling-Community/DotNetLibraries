namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// Which corridors a caller is willing to have realizations drawn from.
    /// <para>
    /// The two limits are the two things that decide whether a corridor is worth drilling, and they are
    /// independent: all the room in the world is no use if the centre line cannot be drilled, and a
    /// gentle centre line is no use in a slot narrower than a driller can hold.
    /// </para>
    /// </summary>
    public class RealizationFilter
    {
        /// <summary>
        /// How much room a well must have around the planned path, m: the smallest disc about the median
        /// that fits inside the tolerance region, taken over the corridor's length. Compared against
        /// <see cref="StreamlineBundleFactory.LeastRoom"/>.
        /// </summary>
        public double MinimumRoom { get; set; } = 0;

        /// <summary>
        /// The sharpest curvature the planned path may have, rad/m. Compared against
        /// <see cref="StreamlineBundleFactory.MedianCurvature"/>.
        /// <para>
        /// A curvature rather than a dogleg severity, and in SI like everything else here: a caller
        /// thinking in degrees per thirty metres converts at its own boundary.
        /// </para>
        /// </summary>
        public double MaximumCurvature { get; set; } = double.PositiveInfinity;

        /// <summary>
        /// whether the given corridor passes both limits
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public bool Accepts(StreamlineBundleFactory factory)
        {
            if (factory == null)
            {
                return false;
            }
            return factory.LeastRoom >= MinimumRoom
                   && (double.IsPositiveInfinity(MaximumCurvature)
                       || (factory.MedianCurvature > 0 && factory.MedianCurvature <= MaximumCurvature));
        }
    }

    /// <summary>
    /// One route: the corridors that nothing forbidden separates, taken together.
    /// <para>
    /// It is a grouping and not a shape. Its corridors are contiguous, so a well may deviate from one
    /// into the next without crossing anything it may not, but they are not interchangeable and they are
    /// not merged: a streamline never changes bundle, so a realization is drawn from <em>one</em>
    /// corridor and stays in it. Asking a route for realizations therefore means choosing a corridor and
    /// drawing from that, not drawing from some average of them.
    /// </para>
    /// </summary>
    public class StreamlineMetaFactory
    {
        private readonly List<StreamlineBundleFactory> corridors_ = new List<StreamlineBundleFactory>();

        /// <summary>
        /// which route this is, as the group index the tolerance regions reported
        /// </summary>
        public int Route { get; internal set; } = 0;

        /// <summary>
        /// the corridors of this route, in the order the set holds them
        /// </summary>
        public IReadOnlyList<StreamlineBundleFactory> Corridors
        {
            get
            {
                return corridors_;
            }
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="route"></param>
        public StreamlineMetaFactory(int route)
        {
            Route = route;
        }

        /// <summary>
        /// adds a corridor to the route
        /// </summary>
        /// <param name="factory"></param>
        public void Add(StreamlineBundleFactory factory)
        {
            if (factory != null)
            {
                corridors_.Add(factory);
            }
        }

        /// <summary>
        /// the corridors that pass the filter, or all of them when none is given
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<StreamlineBundleFactory> GetQualifying(RealizationFilter? filter)
        {
            List<StreamlineBundleFactory> kept = new List<StreamlineBundleFactory>();
            foreach (StreamlineBundleFactory corridor in corridors_)
            {
                if (filter == null || filter.Accepts(corridor))
                {
                    kept.Add(corridor);
                }
            }
            return kept;
        }

        /// <summary>
        /// the room and the hardest turn of the best corridor this route has that passes the filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="room">the most room any qualifying corridor offers, m</param>
        /// <param name="curvature">the gentlest qualifying median, rad/m</param>
        /// <returns>how many corridors qualify</returns>
        public int Describe(RealizationFilter? filter, out double room, out double curvature)
        {
            room = 0;
            curvature = double.PositiveInfinity;
            List<StreamlineBundleFactory> kept = GetQualifying(filter);
            foreach (StreamlineBundleFactory corridor in kept)
            {
                if (corridor.LeastRoom > room) { room = corridor.LeastRoom; }
                if (corridor.MedianCurvature > 0 && corridor.MedianCurvature < curvature)
                {
                    curvature = corridor.MedianCurvature;
                }
            }
            if (kept.Count == 0) { curvature = 0; }
            return kept.Count;
        }

        /// <summary>
        /// Draws realizations from the corridors of this route that pass the filter.
        /// <para>
        /// A corridor is chosen in proportion to how many streamlines it was built from, which under
        /// equal-flux launching is its share of the flow, and the realization is then drawn from that one
        /// corridor alone. Every realization therefore lies inside one corridor's tolerance region at
        /// every cross-section, and so clear of every forbidden zone, and no realization wanders from one
        /// corridor into another the way no original streamline ever did.
        /// </para>
        /// </summary>
        /// <param name="count">how many to draw</param>
        /// <param name="filter">which corridors may be drawn from, or null for all of them</param>
        /// <param name="random">a seeded generator to make the draw repeatable, or null</param>
        /// <returns>the realizations, which may be fewer than asked for when no corridor qualifies</returns>
        public List<Streamline> GetRealizations(int count, RealizationFilter? filter = null,
                                                Random? random = null)
        {
            List<Streamline> drawn = new List<Streamline>(System.Math.Max(0, count));
            List<StreamlineBundleFactory> kept = GetQualifying(filter);
            if (kept.Count == 0 || count <= 0)
            {
                return drawn;
            }

            double total = 0;
            double[] share = new double[kept.Count];
            for (int c = 0; c < kept.Count; c++)
            {
                share[c] = System.Math.Max(1, kept[c].SourceStreamlineCount);
                total += share[c];
            }
            for (int i = 0; i < count; i++)
            {
                double at = (random != null ? random.NextDouble() : Random.Shared.NextDouble()) * total;
                int chosen = kept.Count - 1;
                double running = 0;
                for (int c = 0; c < kept.Count; c++)
                {
                    running += share[c];
                    if (at <= running)
                    {
                        chosen = c;
                        break;
                    }
                }
                Streamline? one = kept[chosen].Draw(random);
                if (one != null && one.Count > 1)
                {
                    drawn.Add(one);
                }
            }
            return drawn;
        }
    }
}
