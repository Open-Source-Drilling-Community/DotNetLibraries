namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// Groups a set of non-crossing streamlines into bundles of streamlines that travel together.
    /// <para>
    /// The streamlines are cut by three families of parallel sweep planes, one per axis. The crossings
    /// of a slab of consecutive planes form a cross-section, in which the streamlines present are grouped
    /// by local proximity (see <see cref="CrossSectionGrouping"/>). Two streamlines found in different
    /// groups on any cross-section where both are present have parted company, and
    /// <see cref="BundlePartitionRule.Separated"/>, the default, never puts such a pair in one bundle: a
    /// streamline that parts from all the others, wherever along its path, ends up in a bundle of its
    /// own.
    /// </para>
    /// <para>
    /// The converse does not hold, and the difference is worth knowing. Two streamlines that never part
    /// are put together where that is possible, but a link between them is refused when taking it would
    /// drag a separated pair into the same bundle, so they may still end up apart. Links are considered
    /// in a fixed order, which makes the result reproducible; it is a partition free of separated pairs
    /// rather than the coarsest such partition, the latter being a graph colouring and not worth its
    /// cost. <see cref="StreamlineBundlingResult.RefusedLinkCount"/> reports how often a link was
    /// refused, and is zero exactly when the two rules cannot differ on the given streamlines.
    /// </para>
    /// <para>
    /// <see cref="BundlePartitionRule.Connected"/> is the earlier behaviour, kept for comparison: plain
    /// connected components, in which one streamline absent from the separating cross-section is enough
    /// to bridge two sides that were seen apart.
    /// </para>
    /// <para>
    /// Because the grouping compares a gap with the local spacing of the streamlines rather than with a
    /// distance given by the caller, no separation threshold has to be supplied. Multiplying all the
    /// input coordinates by a constant leaves the bundles unchanged.
    /// </para>
    /// <para>
    /// Merging is deliberately not represented: two streamlines that start apart and converge are found
    /// in different groups on the early cross-sections and are therefore kept in different bundles, even
    /// though they share the rest of their path. Describing a set of streamlines that travel together
    /// only over part of their length needs the interval form of the result, which this class does not
    /// produce.
    /// </para>
    /// </summary>
    public class StreamlineBundler
    {
        /// <summary>
        /// the settings used by <see cref="Bundle(IStreamlineSource)"/>
        /// </summary>
        public StreamlineBundlingOptions Options { get; set; } = new StreamlineBundlingOptions();

        /// <summary>
        /// default constructor
        /// </summary>
        public StreamlineBundler()
        {
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="options"></param>
        public StreamlineBundler(StreamlineBundlingOptions options)
        {
            Options = options ?? new StreamlineBundlingOptions();
        }

        /// <summary>
        /// groups the given streamlines into bundles
        /// </summary>
        /// <param name="streamlines"></param>
        /// <returns></returns>
        public StreamlineBundlingResult Bundle(IReadOnlyList<Streamline> streamlines)
        {
            if (streamlines == null)
            {
                throw new ArgumentNullException(nameof(streamlines));
            }
            return Bundle(new StreamlineListSource(streamlines));
        }

        /// <summary>
        /// groups the streamlines of the given source into bundles
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public StreamlineBundlingResult Bundle(IStreamlineSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!Options.IsValid(out string? reason))
            {
                throw new ArgumentException(reason, nameof(Options));
            }

            StreamlineBundlingResult result = new StreamlineBundlingResult();
            int streamlineCount = source.Count;
            result.StreamlineCount = streamlineCount;
            result.BundleIndexOfStreamline = new int[streamlineCount];
            if (streamlineCount == 0)
            {
                return result;
            }

            // ---- pass 1: the extent of the data, which sets the sweep resolution ----------------
            double[] minimum = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            double[] maximum = new double[3] { double.MinValue, double.MinValue, double.MinValue };
            for (int s = 0; s < streamlineCount; s++)
            {
                foreach ((double x, double y, double z) in source.GetPositions(s))
                {
                    if (x < minimum[0]) { minimum[0] = x; }
                    if (x > maximum[0]) { maximum[0] = x; }
                    if (y < minimum[1]) { minimum[1] = y; }
                    if (y > maximum[1]) { maximum[1] = y; }
                    if (z < minimum[2]) { minimum[2] = z; }
                    if (z > maximum[2]) { maximum[2] = z; }
                }
            }
            if (minimum[0] > maximum[0])
            {
                // no position at all
                AssignEverySingleton(source, result);
                return result;
            }
            result.BoundingBoxMinimum = minimum;
            result.BoundingBoxMaximum = maximum;

            double largestExtent = 0;
            for (int a = 0; a < 3; a++)
            {
                double extent = maximum[a] - minimum[a];
                if (extent > largestExtent)
                {
                    largestExtent = extent;
                }
            }
            if (!(largestExtent > 0))
            {
                // every position is the same point: nothing can be separated
                AssignSingleBundle(source, result);
                return result;
            }
            double spacing = Options.SweepPlaneSpacing ?? largestExtent / Options.MaximumPlanesPerAxis;
            result.SweepPlaneSpacing = spacing;

            int slabThickness = Options.SlabThickness;
            int[] planeCount = new int[3];
            int slabsPerFamily = 1;
            for (int a = 0; a < 3; a++)
            {
                planeCount[a] = (int)((maximum[a] - minimum[a]) / spacing) + 1;
                int slabs = (planeCount[a] - 1) / slabThickness + 1;
                if (slabs > slabsPerFamily)
                {
                    slabsPerFamily = slabs;
                }
            }
            result.SweepPlaneCounts = planeCount;
            int keyCount = 6 * slabsPerFamily;

            // ---- pass 2: how many crossings fall on each cross-section --------------------------
            int[] keyStart = new int[keyCount + 1];
            EnumerateCrossings(source, minimum, spacing, planeCount, slabThickness, slabsPerFamily,
                               (key, streamline, u, v) => keyStart[key + 1]++);
            for (int key = 0; key < keyCount; key++)
            {
                keyStart[key + 1] += keyStart[key];
            }
            int crossingCount = keyStart[keyCount];
            result.CrossingCount = crossingCount;
            if (crossingCount == 0)
            {
                AssignEverySingleton(source, result);
                return result;
            }

            // ---- pass 3: the crossings themselves, written straight into cross-section order -----
            float[] crossingU = new float[crossingCount];
            float[] crossingV = new float[crossingCount];
            int[] crossingStreamline = new int[crossingCount];
            bool[] hasCrossing = new bool[streamlineCount];
            int[] cursor = new int[keyCount];
            EnumerateCrossings(source, minimum, spacing, planeCount, slabThickness, slabsPerFamily,
                               (key, streamline, u, v) =>
                               {
                                   int at = keyStart[key] + cursor[key];
                                   cursor[key]++;
                                   crossingU[at] = u;
                                   crossingV[at] = v;
                                   crossingStreamline[at] = streamline;
                                   hasCrossing[streamline] = true;
                               });
            for (int s = 0; s < streamlineCount; s++)
            {
                if (!hasCrossing[s])
                {
                    result.StreamlinesWithoutCrossings.Add(s);
                }
            }

            int largestCrossSection = 0;
            int effectiveCrossSections = 0;
            for (int key = 0; key < keyCount; key++)
            {
                int size = keyStart[key + 1] - keyStart[key];
                if (size > largestCrossSection)
                {
                    largestCrossSection = size;
                }
                if (size >= 2)
                {
                    effectiveCrossSections++;
                }
            }
            result.EffectiveCrossSectionCount = effectiveCrossSections;

            // ---- group every cross-section, and collect the pairs found adjacent in one of them --
            int[] memberStreamline = new int[crossingCount];
            int[] memberLabel = new int[crossingCount];
            int[] memberCount = new int[keyCount];
            List<long> adjacentPairs = GroupCrossSections(
                keyCount, keyStart, crossingU, crossingV, crossingStreamline,
                memberStreamline, memberLabel, memberCount,
                streamlineCount, largestCrossSection);

            adjacentPairs.Sort();
            int pairCount = Deduplicate(adjacentPairs);
            result.AdjacentPairCount = pairCount;

            // ---- for every such pair, is it ever found separated? -------------------------------
            int[] separationCount = CountSeparations(
                adjacentPairs, pairCount, streamlineCount,
                keyCount, keyStart, memberStreamline, memberLabel, memberCount);

            // ---- a pair that never parts keeps the two streamlines in the same bundle -----------
            int tolerance = Options.SeparationTolerance;
            UnionFind bundles = new UnionFind(streamlineCount);
            long separated = 0;
            // under the Separated rule, which sets a link may not join
            Dictionary<int, HashSet<int>>? forbidden =
                Options.PartitionRule == BundlePartitionRule.Separated
                ? new Dictionary<int, HashSet<int>>() : null;
            if (forbidden != null)
            {
                for (int p = 0; p < pairCount; p++)
                {
                    if (separationCount[p] <= tolerance)
                    {
                        continue;
                    }
                    long apart = adjacentPairs[p];
                    Forbid(forbidden, (int)(apart >> 32), (int)(apart & 0xFFFFFFFFL));
                }
            }
            long refused = 0;
            for (int p = 0; p < pairCount; p++)
            {
                if (separationCount[p] > tolerance)
                {
                    separated++;
                    continue;
                }
                long packed = adjacentPairs[p];
                int first = (int)(packed >> 32);
                int second = (int)(packed & 0xFFFFFFFFL);
                if (forbidden != null)
                {
                    int rootFirst = bundles.Find(first);
                    int rootSecond = bundles.Find(second);
                    if (rootFirst == rootSecond)
                    {
                        continue;
                    }
                    if (forbidden.TryGetValue(rootFirst, out HashSet<int>? barred)
                        && barred.Contains(rootSecond))
                    {
                        // taking this link would put a pair that was seen apart in one bundle
                        refused++;
                        continue;
                    }
                    bundles.Union(first, second);
                    Absorb(forbidden, rootFirst, rootSecond, bundles.Find(first));
                    continue;
                }
                bundles.Union(first, second);
            }
            result.SeparatedPairCount = separated;
            result.RefusedLinkCount = refused;

            BuildBundles(source, bundles, result);
            return result;
        }

        /// <summary>
        /// Walks every streamline of the source and reports, for each crossing of a sweep plane, the
        /// cross-section it belongs to and where in that cross-section it falls.
        /// </summary>
        private void EnumerateCrossings(IStreamlineSource source, double[] origin, double spacing,
                                        int[] planeCount, int slabThickness, int slabsPerFamily,
                                        Action<int, int, float, float> visit)
        {
            int streamlineCount = source.Count;
            double minimumObliquity = Options.MinimumObliquity;
            Span<double> from = stackalloc double[3];
            Span<double> delta = stackalloc double[3];
            for (int s = 0; s < streamlineCount; s++)
            {
                bool started = false;
                double x0 = 0, y0 = 0, z0 = 0;
                foreach ((double x, double y, double z) in source.GetPositions(s))
                {
                    if (!started)
                    {
                        x0 = x; y0 = y; z0 = z;
                        started = true;
                        continue;
                    }
                    from[0] = x0; from[1] = y0; from[2] = z0;
                    delta[0] = x - x0; delta[1] = y - y0; delta[2] = z - z0;
                    x0 = x; y0 = y; z0 = z;
                    double length = System.Math.Sqrt(delta[0] * delta[0] + delta[1] * delta[1] + delta[2] * delta[2]);
                    if (!(length > 0))
                    {
                        continue;
                    }
                    for (int a = 0; a < 3; a++)
                    {
                        double da = delta[a];
                        if (System.Math.Abs(da) < minimumObliquity * length)
                        {
                            // a grazing crossing says nothing about where the streamline is going
                            continue;
                        }
                        double c0 = (from[a] - origin[a]) / spacing;
                        double c1 = (from[a] + da - origin[a]) / spacing;
                        int first, last, step, direction;
                        if (da > 0)
                        {
                            first = (int)System.Math.Floor(c0) + 1;
                            last = (int)System.Math.Floor(c1);
                            step = 1;
                            direction = 0;
                        }
                        else
                        {
                            first = (int)System.Math.Floor(c0);
                            last = (int)System.Math.Floor(c1) + 1;
                            step = -1;
                            direction = 1;
                        }
                        int block = a * 2 + direction;
                        int uAxis = (a + 1) % 3;
                        int vAxis = (a + 2) % 3;
                        for (int i = first; step > 0 ? i <= last : i >= last; i += step)
                        {
                            if (i < 0 || i >= planeCount[a])
                            {
                                continue;
                            }
                            double t = (origin[a] + i * spacing - from[a]) / da;
                            if (t < 0) { t = 0; } else if (t > 1) { t = 1; }
                            double pu = from[uAxis] + t * delta[uAxis] - origin[uAxis];
                            double pv = from[vAxis] + t * delta[vAxis] - origin[vAxis];
                            visit(block * slabsPerFamily + i / slabThickness, s, (float)pu, (float)pv);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Groups every cross-section and records, per cross-section, the group of every streamline
        /// present. Returns the pairs of streamlines that were found adjacent in at least one
        /// cross-section, packed as a long, with duplicates.
        /// </summary>
        private List<long> GroupCrossSections(int keyCount, int[] keyStart,
                                              float[] crossingU, float[] crossingV, int[] crossingStreamline,
                                              int[] memberStreamline, int[] memberLabel, int[] memberCount,
                                              int streamlineCount, int largestCrossSection)
        {
            List<long> merged = new List<long>();
            object gate = new object();

            SlabWorker CreateWorker()
            {
                return new SlabWorker(Options, streamlineCount, largestCrossSection);
            }

            void ProcessKey(int key, SlabWorker worker)
            {
                int start = keyStart[key];
                int count = keyStart[key + 1] - start;
                if (count <= 0)
                {
                    memberCount[key] = 0;
                    return;
                }
                worker.Grouping.Group(crossingU, crossingV, crossingStreamline, start, count,
                                      worker.Labels, worker.EdgeBuffer, 0, out int edgeCount);

                // one label per streamline; a streamline crossing this cross-section more than once
                // in different groups is left unlabelled and constrains nothing here
                worker.Stamp++;
                int members = 0;
                for (int t = 0; t < count; t++)
                {
                    int streamline = crossingStreamline[start + t];
                    int label = worker.Labels[t];
                    if (worker.StampOf[streamline] != worker.Stamp)
                    {
                        worker.StampOf[streamline] = worker.Stamp;
                        worker.Scratch[streamline] = start + members;  // where this streamline's label is kept
                        memberStreamline[start + members] = streamline;
                        memberLabel[start + members] = label;
                        members++;
                    }
                    else
                    {
                        int slot = worker.Scratch[streamline];
                        if (memberLabel[slot] != label)
                        {
                            memberLabel[slot] = -1;
                        }
                    }
                }
                memberCount[key] = members;

                for (int e = 0; e < edgeCount; e++)
                {
                    long packed = worker.EdgeBuffer[e];
                    int a = crossingStreamline[start + (int)(packed >> 32)];
                    int b = crossingStreamline[start + (int)(packed & 0xFFFFFFFFL)];
                    if (a == b)
                    {
                        continue;
                    }
                    int low = a < b ? a : b;
                    int high = a < b ? b : a;
                    worker.Pairs.Add(((long)low << 32) | (uint)high);
                }
            }

            if (Options.UseParallelism && keyCount > 1)
            {
                Parallel.For(0, keyCount, CreateWorker,
                             (key, state, worker) =>
                             {
                                 ProcessKey(key, worker);
                                 return worker;
                             },
                             worker =>
                             {
                                 lock (gate)
                                 {
                                     merged.AddRange(worker.Pairs);
                                 }
                             });
            }
            else
            {
                SlabWorker worker = CreateWorker();
                for (int key = 0; key < keyCount; key++)
                {
                    ProcessKey(key, worker);
                }
                merged.AddRange(worker.Pairs);
            }
            return merged;
        }

        /// <summary>
        /// records that these two sets may never be joined
        /// </summary>
        private static void Forbid(Dictionary<int, HashSet<int>> forbidden, int first, int second)
        {
            if (!forbidden.TryGetValue(first, out HashSet<int>? ofFirst))
            {
                ofFirst = new HashSet<int>();
                forbidden[first] = ofFirst;
            }
            ofFirst.Add(second);
            if (!forbidden.TryGetValue(second, out HashSet<int>? ofSecond))
            {
                ofSecond = new HashSet<int>();
                forbidden[second] = ofSecond;
            }
            ofSecond.Add(first);
        }

        /// <summary>
        /// Carries what two sets were forbidden to join onto the set they have just become, so that a
        /// separation recorded against either of them still bars the merged set.
        /// </summary>
        private static void Absorb(Dictionary<int, HashSet<int>> forbidden, int first, int second,
                                   int merged)
        {
            HashSet<int> into = new HashSet<int>();
            foreach (int from in new[] { first, second })
            {
                if (!forbidden.TryGetValue(from, out HashSet<int>? had))
                {
                    continue;
                }
                foreach (int other in had)
                {
                    if (other == first || other == second)
                    {
                        continue;
                    }
                    into.Add(other);
                    if (forbidden.TryGetValue(other, out HashSet<int>? back))
                    {
                        back.Remove(from);
                        back.Add(merged);
                    }
                }
                if (from != merged) { forbidden.Remove(from); }
            }
            if (into.Count > 0)
            {
                forbidden[merged] = into;
            }
            else
            {
                forbidden.Remove(merged);
            }
        }

        /// <summary>
        /// For every adjacent pair, counts the cross-sections on which the two streamlines are both
        /// present and in different groups.
        /// </summary>
        private int[] CountSeparations(List<long> pairs, int pairCount, int streamlineCount,
                                       int keyCount, int[] keyStart,
                                       int[] memberStreamline, int[] memberLabel, int[] memberCount)
        {
            int[] separation = new int[pairCount];
            if (pairCount == 0)
            {
                return separation;
            }

            // adjacency of the pair graph, in compressed row form, carrying the rank of the pair
            int[] degree = new int[streamlineCount + 1];
            for (int p = 0; p < pairCount; p++)
            {
                long packed = pairs[p];
                degree[(int)(packed >> 32) + 1]++;
                degree[(int)(packed & 0xFFFFFFFFL) + 1]++;
            }
            for (int s = 0; s < streamlineCount; s++)
            {
                degree[s + 1] += degree[s];
            }
            int[] neighbour = new int[2 * pairCount];
            int[] pairOf = new int[2 * pairCount];
            int[] fill = new int[streamlineCount];
            for (int p = 0; p < pairCount; p++)
            {
                long packed = pairs[p];
                int a = (int)(packed >> 32);
                int b = (int)(packed & 0xFFFFFFFFL);
                int at = degree[a] + fill[a];
                fill[a]++;
                neighbour[at] = b;
                pairOf[at] = p;
                at = degree[b] + fill[b];
                fill[b]++;
                neighbour[at] = a;
                pairOf[at] = p;
            }

            void ProcessKey(int key, SlabWorker worker)
            {
                int members = memberCount[key];
                if (members < 2)
                {
                    return;
                }
                int start = keyStart[key];
                worker.Stamp++;
                for (int j = 0; j < members; j++)
                {
                    int streamline = memberStreamline[start + j];
                    worker.StampOf[streamline] = worker.Stamp;
                    worker.Scratch[streamline] = memberLabel[start + j];  // here the label itself
                }
                for (int j = 0; j < members; j++)
                {
                    int a = memberStreamline[start + j];
                    int labelA = worker.Scratch[a];
                    if (labelA < 0)
                    {
                        continue;
                    }
                    int from = degree[a];
                    int to = degree[a + 1];
                    for (int e = from; e < to; e++)
                    {
                        int b = neighbour[e];
                        if (b < a || worker.StampOf[b] != worker.Stamp)
                        {
                            continue;
                        }
                        int labelB = worker.Scratch[b];
                        if (labelB < 0 || labelB == labelA)
                        {
                            continue;
                        }
                        Interlocked.Increment(ref separation[pairOf[e]]);
                    }
                }
            }

            if (Options.UseParallelism && keyCount > 1)
            {
                Parallel.For(0, keyCount,
                             () => new SlabWorker(Options, streamlineCount, 0),
                             (key, state, worker) =>
                             {
                                 ProcessKey(key, worker);
                                 return worker;
                             },
                             worker => { });
            }
            else
            {
                SlabWorker worker = new SlabWorker(Options, streamlineCount, 0);
                for (int key = 0; key < keyCount; key++)
                {
                    ProcessKey(key, worker);
                }
            }
            return separation;
        }

        /// <summary>
        /// turns the disjoint sets into the ordered list of bundles carried by the result
        /// </summary>
        private static void BuildBundles(IStreamlineSource source, UnionFind sets, StreamlineBundlingResult result)
        {
            int streamlineCount = source.Count;
            Dictionary<int, StreamlineBundle> byRoot = new Dictionary<int, StreamlineBundle>();
            List<StreamlineBundle> bundles = new List<StreamlineBundle>();
            for (int s = 0; s < streamlineCount; s++)
            {
                int root = sets.Find(s);
                if (!byRoot.TryGetValue(root, out StreamlineBundle? bundle))
                {
                    bundle = new StreamlineBundle();
                    byRoot[root] = bundle;
                    bundles.Add(bundle);
                }
                bundle.StreamlineIndices.Add(s);
                bundle.StreamlineIDs.Add(source.GetID(s));
            }
            bundles.Sort((left, right) =>
            {
                int compare = right.Count.CompareTo(left.Count);
                return compare != 0 ? compare : left.StreamlineIndices[0].CompareTo(right.StreamlineIndices[0]);
            });
            for (int b = 0; b < bundles.Count; b++)
            {
                bundles[b].Index = b;
                foreach (int s in bundles[b].StreamlineIndices)
                {
                    result.BundleIndexOfStreamline[s] = b;
                }
            }
            result.Bundles = bundles;
        }

        /// <summary>
        /// removes the duplicates of a sorted list in place, and returns the number of entries kept
        /// </summary>
        private static int Deduplicate(List<long> sorted)
        {
            if (sorted.Count == 0)
            {
                return 0;
            }
            int kept = 1;
            for (int i = 1; i < sorted.Count; i++)
            {
                if (sorted[i] != sorted[kept - 1])
                {
                    sorted[kept] = sorted[i];
                    kept++;
                }
            }
            return kept;
        }

        private static void AssignEverySingleton(IStreamlineSource source, StreamlineBundlingResult result)
        {
            UnionFind sets = new UnionFind(source.Count);
            for (int s = 0; s < source.Count; s++)
            {
                result.StreamlinesWithoutCrossings.Add(s);
            }
            BuildBundles(source, sets, result);
        }

        private static void AssignSingleBundle(IStreamlineSource source, StreamlineBundlingResult result)
        {
            UnionFind sets = new UnionFind(source.Count);
            for (int s = 1; s < source.Count; s++)
            {
                sets.Union(0, s);
            }
            BuildBundles(source, sets, result);
        }

        /// <summary>
        /// the scratch storage of one thread processing cross-sections
        /// </summary>
        private sealed class SlabWorker
        {
            public readonly CrossSectionGrouping Grouping;
            public readonly int[] Labels;
            public readonly long[] EdgeBuffer;
            public readonly int[] StampOf;

            /// <summary>
            /// indexed by streamline and valid only where StampOf matches Stamp. Holds a slot index
            /// while the cross-sections are being grouped, and a group label while the separations are
            /// being counted.
            /// </summary>
            public readonly int[] Scratch;
            public readonly List<long> Pairs = new List<long>();
            public int Stamp;

            public SlabWorker(StreamlineBundlingOptions options, int streamlineCount, int largestCrossSection)
            {
                Grouping = new CrossSectionGrouping(options.NeighbourCount, options.ContrastRatio,
                                                    options.MaximumLinkCandidates);
                Labels = new int[System.Math.Max(1, largestCrossSection)];
                EdgeBuffer = new long[System.Math.Max(1, largestCrossSection)];
                StampOf = new int[streamlineCount];
                Scratch = new int[streamlineCount];
                Stamp = 0;
            }
        }
    }
}
