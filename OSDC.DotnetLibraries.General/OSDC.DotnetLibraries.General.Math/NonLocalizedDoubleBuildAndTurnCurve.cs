using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// A pair of constant build and turn curves whose curvatures agree where they meet.
    ///
    /// The curvature of such a curve is not constant along it: with a build up rate b and a turn rate t
    /// the curvature at inclination i is sqrt(b*b + t*t*sin(i)*sin(i)), so it changes as the inclination
    /// does. There is therefore no single curvature to share, as there is for a pair of circular arcs or
    /// a pair of curves of constant curvature and toolface. What is shared instead is the curvature at
    /// the junction, which is the one place both curves are defined on the same station, and holding the
    /// two equal there is what keeps the dogleg severity from stepping as the curve passes through it.
    /// </summary>
    [Serializable]
    public class NonLocalizedDoubleBuildAndTurnCurve : NonLocalizedCurve
    {
        /// <summary>
        /// the curvature of the first curve where the two meet
        /// </summary>
        public double? JunctionCurvature { get; set; }
        /// <summary>
        /// the curvature of the first curve at the junction divided by that of the second one there.
        /// One means the dogleg severity passes through the junction without a step, which is what the
        /// construction aims at unless it is asked for something else.
        /// </summary>
        public double? CurvatureRatio { get; set; }
        /// <summary>
        /// build up rate of the first curve, i.e., inclination gradient
        /// </summary>
        public double? UpstreamBUR { get; set; }
        /// <summary>
        /// turn rate of the first curve, i.e., azimuth gradient
        /// </summary>
        public double? UpstreamTR { get; set; }
        /// <summary>
        /// the length of the first curve
        /// </summary>
        public double? UpstreamLength { get; set; }
        /// <summary>
        /// build up rate of the second curve, i.e., inclination gradient
        /// </summary>
        public double? DownstreamBUR { get; set; }
        /// <summary>
        /// turn rate of the second curve, i.e., azimuth gradient
        /// </summary>
        public double? DownstreamTR { get; set; }
        /// <summary>
        /// the length of the second curve
        /// </summary>
        public double? DownstreamLength { get; set; }
    }
}
