using System;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// Why a trajectory through a list of targets could not be worked out.
    ///
    /// The distinction that matters most is between a construction that found nothing and one that found
    /// something which then turned out not to reach the target. The first says the targets ask for a
    /// curve of that kind which does not exist, and is answered by moving the target or by drawing that
    /// leg with a different curve. The second would say the construction itself is at fault, and should
    /// not happen.
    /// </summary>
    [Serializable]
    public enum TargetAxisFailureReason
    {
        /// <summary>
        /// Nothing went wrong.
        /// </summary>
        None,

        /// <summary>
        /// The station the trajectory sets off from does not say where it is.
        /// </summary>
        UndefinedStart,

        /// <summary>
        /// A target does not say where it is.
        /// </summary>
        UndefinedTarget,

        /// <summary>
        /// The start carries no direction and none could be worked out, which happens when the first
        /// target sits exactly where the trajectory starts.
        /// </summary>
        UndefinedStartDirection,

        /// <summary>
        /// No curve of the kind asked for reaches this target from the station before it. A single curve
        /// cannot reach a target lying behind it, and a pair of curves of constant curvature and toolface
        /// cannot reach every attitude either, since such a curve stops turning at the vertical. Moving
        /// the target, or drawing this leg with another kind of curve, is what answers this.
        /// </summary>
        CurveDeclined,

        /// <summary>
        /// A curve was worked out but does not arrive at the target closely enough.
        ///
        /// At the accuracy the path asks for by default this should not happen, and it is checked for so
        /// that a trajectory is never handed back which does not go where it was asked to. It becomes a
        /// real answer once a caller asks for more accuracy than the constructions promise: where two
        /// stations are so nearly joined by one curve that the junction of a pair is not pinned down, the
        /// pair constructions fall back on that single curve and meet the direction only to within about a
        /// ten thousandth of a radian.
        /// </summary>
        TargetNotReached,

        /// <summary>
        /// The sections do not join up, or their measured depths do not run forward along them. As above,
        /// nothing should produce this.
        /// </summary>
        Discontinuous
    }
}
