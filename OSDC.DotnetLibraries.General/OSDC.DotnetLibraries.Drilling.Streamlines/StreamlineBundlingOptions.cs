namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// The settings of the <see cref="StreamlineBundler"/>.
    /// <para>
    /// None of the settings is a separation distance. The only quantity with a dimension is
    /// <see cref="SweepPlaneSpacing"/>, which is a sampling resolution and is derived from the data
    /// when it is left null. Whether two streamlines are separated is decided by
    /// <see cref="ContrastRatio"/>, which compares a gap with the local spacing of the streamlines
    /// themselves and is therefore dimensionless. A uniform scaling of all the input coordinates
    /// leaves the result unchanged.
    /// </para>
    /// </summary>
    /// <summary>
    /// How the pairs of streamlines that were never found apart are turned into a partition.
    /// <para>
    /// The two differ only when a pair that <em>was</em> found apart is joined by a chain of pairs that
    /// were not, which happens when the streamline bridging them is absent from the very cross-section
    /// that separated the pair — either because it does not reach that plane or because
    /// <see cref="StreamlineBundlingOptions.MinimumObliquity"/> discarded its crossing as too grazing.
    /// </para>
    /// </summary>
    public enum BundlePartitionRule
    {
        /// <summary>
        /// Connected components of the pairs that were never found apart. One bridging streamline is
        /// enough to join two sides that were seen apart, so a bundle may contain a separated pair.
        /// </summary>
        Connected,

        /// <summary>
        /// The same links, but a link is refused whenever taking it would put a separated pair in one
        /// bundle. No bundle then contains a pair that was ever seen apart, which is what the bundling
        /// rule states. Links are taken in a fixed order, so the result is deterministic; it is the
        /// coarsest partition that order reaches rather than the coarsest that exists, the latter being
        /// a graph colouring and not worth its cost here.
        /// </summary>
        Separated
    }

    public class StreamlineBundlingOptions
    {
        /// <summary>
        /// the distance between two consecutive sweep planes of a same family. When null, it is set to
        /// the largest extent of the data divided by <see cref="MaximumPlanesPerAxis"/>.
        /// </summary>
        public double? SweepPlaneSpacing { get; set; } = null;

        /// <summary>
        /// the number of sweep planes placed across the largest extent of the data when
        /// <see cref="SweepPlaneSpacing"/> is null
        /// </summary>
        public int MaximumPlanesPerAxis { get; set; } = 512;

        /// <summary>
        /// the number of consecutive sweep planes merged into a single cross-section.
        /// <para>
        /// This is what obliges a separation to persist before it is taken into account. A gap that is
        /// present on one plane only is filled in by the crossings of the neighbouring planes of the
        /// same slab and separates nothing. A gap that persists through the slab remains a gap. A value
        /// of 1 disables that filtering.
        /// </para>
        /// </summary>
        public int SlabThickness { get; set; } = 3;

        /// <summary>
        /// how much larger than the local spacing of the streamlines a gap has to be in order to count
        /// as a separation. Dimensionless.
        /// <para>
        /// Two crossings of a same cross-section are linked when their distance does not exceed this
        /// ratio times the larger of their two local scales. The local scale of a crossing is the median,
        /// over its nearest neighbours, of the distance from those neighbours to their own
        /// <see cref="NeighbourCount"/>-th nearest crossing. Because the comparison is with the local
        /// spacing and not with a fixed length, a locally refined grid, or a region where the streamlines
        /// happen to be denser, does not by itself produce a separation.
        /// </para>
        /// </summary>
        public double ContrastRatio { get; set; } = 3.0;

        /// <summary>
        /// the rank of the neighbour used to measure the local spacing of the streamlines in a
        /// cross-section
        /// </summary>
        public int NeighbourCount { get; set; } = 6;

        /// <summary>
        /// the smallest absolute cosine between the direction of a streamline and the normal of a sweep
        /// plane for a crossing of that plane to be recorded. Dimensionless.
        /// <para>
        /// Only the plane families that a streamline genuinely runs across are of any use. A family it
        /// runs nearly along gives slices that are not cross-sections of the flow at all: two streamlines
        /// that travel side by side but are offset along the flow reach such a plane at very different
        /// places, and would be taken for two streamlines that have parted. Since the largest component
        /// of a unit vector is always at least the inverse square root of three, a value below that
        /// leaves at least the dominant family of every segment in use, and the default sits just under
        /// it so that a direction sharing itself equally between the three axes keeps all three.
        /// </para>
        /// </summary>
        public double MinimumObliquity { get; set; } = 0.5;

        /// <summary>
        /// the number of cross-sections on which two neighbouring streamlines may be found in different
        /// groups while still being kept in the same bundle. Zero means that a single separation is
        /// enough.
        /// </summary>
        public int SeparationTolerance { get; set; } = 0;

        /// <summary>
        /// How the pairs that were never found separated are turned into a partition.
        /// </summary>
        public BundlePartitionRule PartitionRule { get; set; } = BundlePartitionRule.Separated;

        /// <summary>
        /// whether the cross-sections are processed in parallel. Only affects performance: the result is
        /// identical either way.
        /// </summary>
        public bool UseParallelism { get; set; } = true;

        /// <summary>
        /// the largest number of candidate neighbours examined when linking a crossing to the others of
        /// its cross-section. Guards against a quadratic cost when a sparse region is adjacent to a very
        /// dense one.
        /// </summary>
        public int MaximumLinkCandidates { get; set; } = 1024;

        /// <summary>
        /// checks that the settings are usable
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool IsValid(out string? reason)
        {
            reason = null;
            if (SweepPlaneSpacing != null && !(SweepPlaneSpacing.Value > 0))
            {
                reason = "SweepPlaneSpacing must be strictly positive when it is given.";
            }
            else if (MaximumPlanesPerAxis < 2)
            {
                reason = "MaximumPlanesPerAxis must be at least 2.";
            }
            else if (SlabThickness < 1)
            {
                reason = "SlabThickness must be at least 1.";
            }
            else if (!(ContrastRatio > 1.0))
            {
                reason = "ContrastRatio must be strictly greater than 1.";
            }
            else if (NeighbourCount < 1)
            {
                reason = "NeighbourCount must be at least 1.";
            }
            else if (MinimumObliquity < 0 || MinimumObliquity >= 1)
            {
                reason = "MinimumObliquity must be in [0, 1).";
            }
            else if (SeparationTolerance < 0)
            {
                reason = "SeparationTolerance cannot be negative.";
            }
            else if (MaximumLinkCandidates < 1)
            {
                reason = "MaximumLinkCandidates must be at least 1.";
            }
            return reason == null;
        }
    }
}
