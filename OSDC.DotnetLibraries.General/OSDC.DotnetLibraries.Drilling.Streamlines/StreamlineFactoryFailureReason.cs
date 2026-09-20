using System;

namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// Why a bundle could not be turned into a factory.
    ///
    /// A factory replaces a bundle by a median curve, a transform per cross-section and a density, and
    /// every one of those needs a bundle wide enough and populated enough to be measured. The reasons
    /// here are all about the bundle handed in being too thin a basis for that, never about the fit
    /// itself having been attempted and failed.
    /// </summary>
    [Serializable]
    public enum StreamlineFactoryFailureReason
    {
        /// <summary>
        /// Nothing went wrong.
        /// </summary>
        None,

        /// <summary>
        /// The bundle holds fewer streamlines than
        /// <see cref="StreamlineBundleFactoryOptions.MinimumStreamlineCount"/>. Three streamlines are the
        /// algebraic minimum for the affine transform of a cross-section, and a density needs a good deal
        /// more, so a tiny bundle is kept as it is rather than replaced by a surrogate of it.
        /// </summary>
        TooFewStreamlines,

        /// <summary>
        /// The streamlines of the bundle carry too few positions between them to lay out a median curve.
        /// </summary>
        TooFewPositions,

        /// <summary>
        /// Every streamline of the bundle is at the same place, or the bundle has no length: there is no
        /// curve to be median of.
        /// </summary>
        DegenerateGeometry,

        /// <summary>
        /// Not enough streamlines reach far enough along the median curve for the cross-sections to be
        /// populated. Happens when the members of the bundle barely overlap.
        /// </summary>
        InsufficientOverlap,

        /// <summary>
        /// The settings handed in are not usable.
        /// </summary>
        /// <summary>
        /// The median path runs inside a forbidden zone somewhere along its length.
        /// <para>
        /// The median is the path that would be planned, so this is not a corridor that is merely tight:
        /// it is a plan that goes through something a well may not enter, and no tolerance around it makes
        /// it valid. The streamlines it was built from are all clear — they were traced through open cells
        /// — but their middle is not, which is a statement about the bundle and not about them.
        /// </para>
        /// </summary>
        MedianInsideForbiddenZone,

        InvalidOptions
    }
}
