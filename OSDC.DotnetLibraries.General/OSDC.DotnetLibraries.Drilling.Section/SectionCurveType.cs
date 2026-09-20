using System;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// Which of the three curves a section is drawn with.
    /// </summary>
    [Serializable]
    public enum SectionCurveType
    {
        /// <summary>
        /// A circular arc: the curvature and the plane of the turn both hold along the section.
        /// </summary>
        CircularArc,
        /// <summary>
        /// A constant build and turn curve: the inclination and the azimuth each run at their own steady
        /// rate, which is what a survey between two stations is usually read as.
        /// </summary>
        ConstantBuildAndTurn,
        /// <summary>
        /// A constant curvature and constant toolface curve: what a bent housing drills when the toolface
        /// is held and the curvature does not change.
        /// </summary>
        ConstantCurvatureAndToolface
    }
}
