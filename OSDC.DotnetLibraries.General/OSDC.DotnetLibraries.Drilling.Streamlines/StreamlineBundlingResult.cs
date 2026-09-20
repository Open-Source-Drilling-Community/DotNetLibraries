namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// The outcome of a <see cref="StreamlineBundler"/> run, with the diagnostics needed to explain why
    /// the bundles came out the way they did.
    /// </summary>
    public class StreamlineBundlingResult
    {
        /// <summary>
        /// the bundles, ordered by decreasing number of streamlines and then by smallest streamline index
        /// </summary>
        public List<StreamlineBundle> Bundles { get; internal set; } = new List<StreamlineBundle>();

        /// <summary>
        /// for each streamline of the source, the index in <see cref="Bundles"/> of the bundle it
        /// belongs to
        /// </summary>
        public int[] BundleIndexOfStreamline { get; internal set; } = Array.Empty<int>();

        /// <summary>
        /// the number of streamlines that were submitted
        /// </summary>
        public int StreamlineCount { get; internal set; } = 0;

        /// <summary>
        /// the distance between two consecutive sweep planes that was actually used
        /// </summary>
        public double SweepPlaneSpacing { get; internal set; } = 0;

        /// <summary>
        /// the number of sweep planes that were used, per axis
        /// </summary>
        public int[] SweepPlaneCounts { get; internal set; } = new int[3];

        /// <summary>
        /// the number of cross-sections that carried at least two crossings and could therefore separate
        /// something
        /// </summary>
        public int EffectiveCrossSectionCount { get; internal set; } = 0;

        /// <summary>
        /// the total number of sweep plane crossings that were recorded
        /// </summary>
        public long CrossingCount { get; internal set; } = 0;

        /// <summary>
        /// how many links were refused because taking them would have put a pair that was seen apart in
        /// one bundle. Always zero under <see cref="BundlePartitionRule.Connected"/>.
        /// </summary>
        public long RefusedLinkCount { get; internal set; } = 0;

        /// <summary>
        /// the number of pairs of streamlines that were found adjacent in at least one cross-section
        /// </summary>
        public long AdjacentPairCount { get; internal set; } = 0;

        /// <summary>
        /// the number of those pairs that were found separated on more cross-sections than
        /// <see cref="StreamlineBundlingOptions.SeparationTolerance"/> allows, and that were therefore
        /// not kept
        /// </summary>
        public long SeparatedPairCount { get; internal set; } = 0;

        /// <summary>
        /// the indices of the streamlines that produced no usable crossing at all. They are bundles of
        /// one for want of evidence rather than because they were found to travel alone. Typically
        /// streamlines with fewer than two distinct positions, or shorter than one sweep plane spacing.
        /// </summary>
        public List<int> StreamlinesWithoutCrossings { get; internal set; } = new List<int>();

        /// <summary>
        /// the lower corner of the bounding box of the submitted positions
        /// </summary>
        public double[] BoundingBoxMinimum { get; internal set; } = new double[3];

        /// <summary>
        /// the upper corner of the bounding box of the submitted positions
        /// </summary>
        public double[] BoundingBoxMaximum { get; internal set; } = new double[3];

        /// <summary>
        /// the bundle the given streamline belongs to
        /// </summary>
        /// <param name="streamlineIndex"></param>
        /// <returns></returns>
        public StreamlineBundle? GetBundleOf(int streamlineIndex)
        {
            if (streamlineIndex < 0 || streamlineIndex >= BundleIndexOfStreamline.Length)
            {
                return null;
            }
            int bundle = BundleIndexOfStreamline[streamlineIndex];
            return bundle < 0 || bundle >= Bundles.Count ? null : Bundles[bundle];
        }

        /// <summary>
        /// whether the two given streamlines ended up in the same bundle
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public bool AreBundledTogether(int first, int second)
        {
            if (first < 0 || first >= BundleIndexOfStreamline.Length ||
                second < 0 || second >= BundleIndexOfStreamline.Length)
            {
                return false;
            }
            return BundleIndexOfStreamline[first] == BundleIndexOfStreamline[second];
        }
    }
}
