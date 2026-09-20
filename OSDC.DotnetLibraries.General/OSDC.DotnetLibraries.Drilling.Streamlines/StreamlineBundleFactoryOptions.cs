namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// The settings of the <see cref="StreamlineBundleFactoryBuilder"/>.
    /// </summary>
    public class StreamlineBundleFactoryOptions
    {
        /// <summary>
        /// the distance along the median curve between two consecutive cross-sections. When null, the
        /// length of the median curve divided by <see cref="StationCount"/> is used.
        /// </summary>
        public double? StationSpacing { get; set; } = null;

        /// <summary>
        /// the number of cross-sections laid out along the median curve when
        /// <see cref="StationSpacing"/> is null
        /// </summary>
        public int StationCount { get; set; } = 256;

        /// <summary>
        /// the smallest number of streamlines a bundle must hold to be worth replacing by a factory.
        /// <para>
        /// Three is the algebraic minimum for the affine transform of a cross-section, six equations for
        /// six unknowns, which leaves nothing over to measure a residual with, and a density estimated
        /// from a handful of points says more about those points than about the bundle. Below this count
        /// the bundle is better kept as it is.
        /// </para>
        /// </summary>
        public int MinimumStreamlineCount { get; set; } = 8;

        /// <summary>
        /// how many times the median curve is recomputed. Each pass lays out cross-sections
        /// perpendicular to the current median, intersects the streamlines with them, and takes the
        /// geometric median of the crossings as the next median.
        /// </summary>
        public int RefinementIterationCount { get; set; } = 3;

        /// <summary>
        /// the largest fraction of the radius of the bundle by which the median curve is allowed to move
        /// away from the geometric median of the crossings in order to be smooth.
        /// <para>
        /// The median curve has to satisfy two things at once which are not in general both satisfiable:
        /// it should lie in the middle of the bundle, and it should have the shape, meaning the tangent,
        /// of the streamlines around it. Smoothing buys tangent at the expense of position. This setting
        /// says how much position may be paid: the smoothing is taken as far as it will go without the
        /// curve moving further from the geometric median than this. Lower it to favour position further,
        /// raise it to favour shape.
        /// </para>
        /// </summary>
        public double MedianDisplacementBudget { get; set; } = 0.1;

        /// <summary>
        /// the number of alternating least squares passes used to fit the cross-section transforms
        /// together with the normalized coordinate of every streamline
        /// </summary>
        public int TransformIterationCount { get; set; } = 8;

        /// <summary>
        /// <summary>
        /// How many streamlines an angular sector of a cross-section outline should hold on average. The
        /// resolution of an outline follows the data this way rather than being asked for, as the density
        /// grid already does.
        /// </summary>
        public double SectorPopulation { get; set; } = 3.0;

        /// <summary>
        /// the fewest angular sectors an outline is divided into
        /// </summary>
        public int MinimumSectorCount { get; set; } = 6;

        /// <summary>
        /// the most angular sectors an outline is divided into
        /// </summary>
        public int MaximumSectorCount { get; set; } = 64;

        /// <summary>
        /// How far an outline is inflated beyond the furthest crossing it was built from, as a fraction.
        /// The requirement an outline meets is that it contain the crossings, not that it touch them, and
        /// a boundary drawn exactly through the outermost of them would leave half of them on it.
        /// </summary>
        public double OutlineMargin { get; set; } = 0.05;

        /// the average number of streamlines wanted per cell of the polar density. Sets how finely the
        /// cross-section is divided: a bundle of a few dozen streamlines gets a coarse grid, a bundle of
        /// thousands a fine one.
        /// </summary>
        public double DensityCellPopulation { get; set; } = 4.0;

        /// <summary>
        /// the largest number of cells of the polar density
        /// </summary>
        public int MaximumDensityCellCount { get; set; } = 4096;

        /// <summary>
        /// the smallest fraction of the streamlines of the bundle that has to reach a cross-section for a
        /// factory to be worth building at all.
        /// <para>
        /// Which cross-sections are kept is not decided by this but by the plateau: a cross-section is
        /// kept when as many streamlines reach it as reach the best populated one. A cross-section short
        /// of that is one whose plane has started to leave the bundle through its end, where the crossings
        /// it still collects all lie to one side. This setting is the floor underneath: when even the
        /// plateau falls short of it, the streamlines overlap too little to be one bundle.
        /// </para>
        /// </summary>
        public double MinimumStationPopulation { get; set; } = 0.5;

        /// <summary>
        /// checks that the settings are usable
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool IsValid(out string? reason)
        {
            reason = null;
            if (StationSpacing != null && !(StationSpacing.Value > 0))
            {
                reason = "StationSpacing must be strictly positive when it is given.";
            }
            else if (StationCount < 4)
            {
                reason = "StationCount must be at least 4.";
            }
            else if (MinimumStreamlineCount < 3)
            {
                reason = "MinimumStreamlineCount must be at least 3, the algebraic minimum for the transform.";
            }
            else if (RefinementIterationCount < 1)
            {
                reason = "RefinementIterationCount must be at least 1.";
            }
            else if (!(MedianDisplacementBudget >= 0))
            {
                reason = "MedianDisplacementBudget cannot be negative.";
            }
            else if (TransformIterationCount < 1)
            {
                reason = "TransformIterationCount must be at least 1.";
            }
            else if (!(DensityCellPopulation >= 1))
            {
                reason = "DensityCellPopulation must be at least 1.";
            }
            else if (MaximumDensityCellCount < 1)
            {
                reason = "MaximumDensityCellCount must be at least 1.";
            }
            else if (!(MinimumStationPopulation > 0) || MinimumStationPopulation > 1)
            {
                reason = "MinimumStationPopulation must be in (0, 1].";
            }
            return reason == null;
        }
    }
}
