using System;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    /// <summary>
    /// Why a survey could not be reduced to analytic sections.
    ///
    /// Every reason here is about the survey handed in rather than about the method: given two stations
    /// which say where they are and which way the hole points, a reduction always exists, because a
    /// circular arc always joins two attitudes. So there is no reason in this list for a reduction that
    /// was attempted and did not work out.
    /// </summary>
    [Serializable]
    public enum ReducedOrderFailureReason
    {
        /// <summary>
        /// Nothing went wrong.
        /// </summary>
        None,

        /// <summary>
        /// The survey carries no stations at all.
        /// </summary>
        NoStations,

        /// <summary>
        /// The measured depth, inclination and azimuth do not all carry the same number of stations.
        /// </summary>
        MismatchedStationCounts,

        /// <summary>
        /// A station carries a measured depth, an inclination or an azimuth which is not a number.
        /// </summary>
        UndefinedStation,

        /// <summary>
        /// The tolerance asked for is not a positive angle.
        /// </summary>
        InvalidTolerance
    }
}
