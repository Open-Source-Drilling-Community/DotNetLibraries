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
        /// Curvature
        /// </summary>
        public double Curvature { get; set; }
        /// <summary>
        /// the radius of curvature is the inverse of the curvature
        /// </summary>
        public double RadiusOfCurvature { get => 1.0 / Curvature; set => Curvature = 1.0 / value; }
        /// <summary>
        /// toolface
        /// </summary>
        public double Toolface { get; set; }
    }
}
