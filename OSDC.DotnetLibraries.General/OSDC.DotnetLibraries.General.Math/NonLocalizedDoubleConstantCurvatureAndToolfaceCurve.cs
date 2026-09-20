using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// A pair of constant curvature and constant toolface curves sharing one curvature.
    ///
    /// The toolface of such a curve holds all the way along it, unlike the reference toolface of a
    /// circular arc which is quoted at the start and then turns with the high side, so the two angles
    /// here are the toolface of the upstream curve and the toolface of the downstream one.
    /// </summary>
    [Serializable]
    public class NonLocalizedDoubleConstantCurvatureAndToolfaceCurve : NonLocalizedCurve
    {
        /// <summary>
        /// the curvature shared by the two curves
        /// </summary>
        public double? Curvature { get; set; }
        /// <summary>
        /// the radius of curvature is the inverse of the curvature
        /// </summary>
        public double? RadiusOfCurvature { get => 1.0 / Curvature; set => Curvature = 1.0 / value; }
        /// <summary>
        /// the toolface held along the first curve
        /// </summary>
        public double? UpstreamToolface { get; set; }
        /// <summary>
        /// the toolface held along the second curve
        /// </summary>
        public double? DownstreamToolface { get; set; }
        /// <summary>
        /// the length of the first curve
        /// </summary>
        public double? UpstreamLength { get; set; }
        /// <summary>
        /// the length of the second curve
        /// </summary>
        public double? DownstreamLength { get; set; }
    }
}
