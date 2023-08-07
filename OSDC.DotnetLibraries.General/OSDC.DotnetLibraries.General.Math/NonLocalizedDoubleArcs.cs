using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    public class NonLocalizedDoubleArcs : NonLocalizedCurve
    {
        /// <summary>
        /// common curvature used for the two arcs
        /// </summary>
        public double? Curvature { get; set; }
        /// <summary>
        /// the radius of curvature is the inverse of the curvature
        /// </summary>
        public double? RadiusOfCurvature { get => 1.0 / Curvature; set => Curvature = 1.0 / value; }
        /// <summary>
        /// the toolface at a reference position for the first arc
        /// </summary>
        public double? UpstreamReferenceToolface { get; set; }
        /// <summary>
        /// the toolface at the reference position for the second arc
        /// </summary>
        public double? DownstreamReferenceToolface { get; set; }

    }
}
