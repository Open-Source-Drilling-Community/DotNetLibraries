using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// a non localized constant curvature and toolface curve
    /// </summary>
    [Serializable]
    public class NonLocalizedConstantCurvatureAndToolfaceCurve : NonLocalizedCurve
    {
        /// <summary>
        /// Curvature. Undefined until it is set, as on the circular arc and the build and turn curves, so
        /// that a curvature which was never given is not read as a curvature of nothing.
        /// </summary>
        public double? Curvature { get; set; }
        /// <summary>
        /// the radius of curvature is the inverse of the curvature
        /// </summary>
        public double? RadiusOfCurvature { get => Curvature == 0 ? null : 1.0 / Curvature; set => Curvature = 1.0 / value; }
        /// <summary>
        /// toolface, held all the way along the curve. Undefined until it is set.
        /// </summary>
        public double? Toolface { get; set; }
    }
}
