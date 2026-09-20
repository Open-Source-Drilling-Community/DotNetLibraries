using System;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    /// <summary>
    /// Which analytic curve one section of a reduced order trajectory is drawn with.
    ///
    /// The four families are ordered from the simplest to the most general, and the reduction relies on
    /// that order: where two families reach equally far along the hole, the earlier one wins, so a
    /// straight hold is preferred to an arc and an arc to a curve carrying an azimuth rate. The numeric
    /// values are part of the interchange format and of the reference vectors, so they are fixed.
    ///
    /// Every section except a hold carries two parameters. What those two parameters mean depends on the
    /// family, and is given with each member below and in section 3 of the specification.
    /// </summary>
    [Serializable]
    public enum ReducedOrderCurveType
    {
        /// <summary>
        /// A straight hold: the tangent does not change along the section. No free parameters. The two
        /// parameter slots are carried as zero and are not decision variables in the refinement, because
        /// a slot that changes nothing contributes a null column to the Jacobian.
        /// </summary>
        Hold = 0,

        /// <summary>
        /// A circular arc: the curvature and the plane of the turn both hold along the section.
        ///
        /// The two parameters are the components of the turn rate vector in the basis perpendicular to
        /// the tangent the section inherits, so that the curvature is their magnitude and the plane of
        /// the turn their direction. They are carried this way rather than as a curvature and a toolface
        /// angle because the pair stays smooth through zero curvature, where the angle is unobservable.
        /// </summary>
        CircularArc = 1,

        /// <summary>
        /// A constant curvature and constant toolface curve: what a bent housing drills when the toolface
        /// is held and the curvature does not change. The inclination runs at a steady rate and the
        /// azimuth rate carries a factor one over the sine of the inclination.
        ///
        /// The two parameters are the build up rate and the turn parameter, the curvature resolved along
        /// and across the high side. As above, this pair rather than a curvature and a toolface angle.
        /// </summary>
        ConstantCurvatureAndToolface = 2,

        /// <summary>
        /// A constant build and turn curve: the inclination and the azimuth each run at their own steady
        /// rate, which is what a survey between two stations is usually read as. The two parameters are
        /// those two rates. This curve does not have a constant curvature.
        /// </summary>
        ConstantBuildAndTurn = 3
    }
}
