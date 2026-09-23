namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// what avoiding faults across one route came to
    /// </summary>
    public class FaultRouteOutcome
    {
        /// <summary>
        /// the faults the route now avoids, each with the share of the route's paths that giving it up
        /// cost on its own
        /// </summary>
        public List<(string Fault, double Share)> Avoided { get; } = new List<(string, double)>();

        /// <summary>
        /// the faults the route still goes through, each with the share of its paths that cross it:
        /// either every corridor crosses it, or avoiding it would have given up more than the limit
        /// </summary>
        public List<(string Fault, double Share)> Kept { get; } = new List<(string, double)>();

        /// <summary>
        /// the corridors given up, by their index in the list handed in
        /// </summary>
        public List<int> Excluded { get; } = new List<int>();

        /// <summary>
        /// the share of the route's qualifying paths given up, all faults together
        /// </summary>
        public double ShareGivenUp { get; internal set; } = 0;
    }

    /// <summary>
    /// Gives up the corridors of a route that go through a fault the rest of the route passes by.
    /// <para>
    /// The corridor trim cannot do this, and rightly: a corridor whose median goes through a fault
    /// cannot be pulled back from it, so for that corridor the fault is unavoidable. It is avoidable only
    /// for the route. Measured on the 85 degree slot case, two faults poke into the route from opposite
    /// sides, one from above and one from below, and the corridors that go through them are the three
    /// furthest out on each side; every other corridor passes between the two tips.
    /// </para>
    /// <para>
    /// What decides it is how much of the route a fault would cost, as a share of the paths, and the
    /// limit on that is its own number, not the corridor trim's overlap limit: one bounds how much of a
    /// cross-section a corridor gives up, the other how much of a route gives up whole corridors. The
    /// limit is spent on all the faults together, cheapest first. Without it the rule is wrong in the
    /// worst way: on the same case one fault is crossed squarely by every corridor but one, which
    /// carries 1.8 % of the paths, and "avoidable if any corridor avoids it" would give up the other
    /// 98 % to dodge a fault that is crossed well.
    /// </para>
    /// <para>
    /// A corridor counts as crossing a fault when its tube does after it has been trimmed: that covers
    /// the median crossings and the grazes the trim could not afford to give up, and nothing it did.
    /// </para>
    /// </summary>
    public static class FaultRouteAvoidance
    {
        /// <summary>
        /// the most of a route's qualifying paths that may be given up to avoid faults, all of them
        /// together
        /// </summary>
        public const double DefaultRouteShareLimit = 0.25;

        /// <summary>
        /// Chooses which corridors to give up. Each corridor is described by its weight, which is its
        /// share of the flow and the weight realizations are drawn with, and the faults its tube goes
        /// through. Only the corridors that already qualify on room and turn are to be handed in.
        /// </summary>
        public static FaultRouteOutcome Choose(IReadOnlyList<(double Weight, IReadOnlyCollection<string> Faults)> corridors,
                                               double shareLimit)
        {
            FaultRouteOutcome outcome = new FaultRouteOutcome();
            double total = 0;
            foreach ((double weight, IReadOnlyCollection<string> _) in corridors) { total += weight; }
            if (!(total > 0)) { return outcome; }

            Dictionary<string, List<int>> crossers = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            for (int c = 0; c < corridors.Count; c++)
            {
                foreach (string fault in corridors[c].Faults)
                {
                    if (!crossers.TryGetValue(fault, out List<int>? list))
                    {
                        list = new List<int>();
                        crossers[fault] = list;
                    }
                    if (!list.Contains(c)) { list.Add(c); }
                }
            }
            List<(string Fault, List<int> By, double Share)> faults = new List<(string, List<int>, double)>();
            foreach (KeyValuePair<string, List<int>> entry in crossers)
            {
                double share = 0;
                foreach (int c in entry.Value) { share += corridors[c].Weight; }
                faults.Add((entry.Key, entry.Value, share / total));
            }
            // cheapest first, ties by name so that the choice does not depend on dictionary order
            faults.Sort((a, b) => a.Share != b.Share ? a.Share.CompareTo(b.Share)
                                                     : string.CompareOrdinal(a.Fault, b.Fault));

            HashSet<int> excluded = new HashSet<int>();
            double given = 0;
            foreach ((string fault, List<int> by, double share) in faults)
            {
                double more = 0;
                foreach (int c in by)
                {
                    if (!excluded.Contains(c)) { more += corridors[c].Weight; }
                }
                // a fault every corridor goes through cannot be avoided at any price, and the limit
                // below one would catch it anyway; saying so is clearer than relying on that
                if (by.Count == corridors.Count || (given + more) / total > shareLimit)
                {
                    outcome.Kept.Add((fault, share));
                    continue;
                }
                foreach (int c in by) { excluded.Add(c); }
                given += more;
                outcome.Avoided.Add((fault, share));
            }
            outcome.Excluded.AddRange(excluded.OrderBy(c => c));
            outcome.ShareGivenUp = given / total;
            return outcome;
        }

        /// <summary>
        /// Applies the choice to one route under a filter: the corridors the filter accepts on room and
        /// turn are weighed, the ones given up are added to <see cref="RealizationFilter.Excluded"/>
        /// of the filter handed back, and the filter handed in is left as it was.
        /// </summary>
        public static RealizationFilter Apply(StreamlineMetaFactory route, RealizationFilter filter,
                                              IReadOnlyDictionary<StreamlineBundleFactory, IReadOnlyCollection<string>> tubeFaults,
                                              double shareLimit, out FaultRouteOutcome outcome)
        {
            List<StreamlineBundleFactory> qualifying = route.GetQualifying(filter);
            List<(double, IReadOnlyCollection<string>)> described = new List<(double, IReadOnlyCollection<string>)>();
            foreach (StreamlineBundleFactory corridor in qualifying)
            {
                described.Add((System.Math.Max(1, corridor.SourceStreamlineCount),
                               tubeFaults.TryGetValue(corridor, out IReadOnlyCollection<string>? names)
                               ? names : Array.Empty<string>()));
            }
            outcome = Choose(described, shareLimit);
            HashSet<StreamlineBundleFactory> excluded = filter.Excluded != null
                ? new HashSet<StreamlineBundleFactory>(filter.Excluded)
                : new HashSet<StreamlineBundleFactory>();
            foreach (int c in outcome.Excluded) { excluded.Add(qualifying[c]); }
            return new RealizationFilter
            {
                MinimumRoom = filter.MinimumRoom,
                MaximumCurvature = filter.MaximumCurvature,
                Excluded = excluded
            };
        }
    }
}
