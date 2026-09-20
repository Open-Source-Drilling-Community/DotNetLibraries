using System;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// Why a complex path could not be worked out.
    /// </summary>
    [Serializable]
    public enum ComplexPathFailureReason
    {
        /// <summary>
        /// Nothing went wrong.
        /// </summary>
        None,

        /// <summary>
        /// The station the path sets off from is not completely defined. A complex path is worked out from
        /// a station whose position, attitude and measured depth are all known.
        /// </summary>
        UndefinedStart,

        /// <summary>
        /// A section imposes nothing at all. A section with nothing imposed leaves where it ends up to the
        /// sections around it, and the place it hands over to the next one can then slide along the path
        /// without changing anything, which leaves the answer not a single path but a family of them.
        /// </summary>
        SectionWithoutParameters,

        /// <summary>
        /// A parameter belonging to another kind of curve has been imposed: a build up rate or a turn rate
        /// on a section drawn as a circular arc or as a constant curvature and toolface curve, or a
        /// curvature or a toolface angle on one drawn as a constant build and turn curve.
        /// </summary>
        ParameterOfAnotherCurve,

        /// <summary>
        /// The number of quantities imposed is not three times the number of sections. Each section leaving
        /// a known station has three degrees of freedom, so that is how many quantities settle the path.
        /// </summary>
        WrongNumberOfParameters,

        /// <summary>
        /// A section, or a run of sections from the start, imposes more than the three degrees of freedom
        /// each of them has.
        ///
        /// A surplus on one section is answered by the sections before it, since where they end is where it
        /// begins; a shortfall is answered by the sections after it. So a shortfall may be carried forward
        /// but a surplus may not: by the time it is met there is nothing left to give. Counting from the
        /// start, no run of sections may impose more than three times its length.
        /// </summary>
        OverDeterminedSections,

        /// <summary>
        /// The sections could not be worked out from the quantities imposed. Either no path of these curves
        /// satisfies them, or the search did not find it.
        /// </summary>
        NotSolved,

        /// <summary>
        /// A path was worked out but does not honour one of the quantities imposed on it. Nothing should
        /// produce this; it is checked for so that a path is never handed back which does not do what it
        /// was asked to.
        /// </summary>
        ParameterNotHonoured
    }
}
