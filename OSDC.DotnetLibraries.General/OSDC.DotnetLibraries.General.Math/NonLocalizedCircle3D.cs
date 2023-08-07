using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// a non localized circle in 3D space
    /// </summary>
    [Serializable]
    public class NonLocalizedCircle3D : NonLocalizedCurve
    {
        /// <summary>
        /// curvature of the circle
        /// </summary>
        public double? Curvature { get; set; }
        /// <summary>
        /// the radius of curvature is the inverse of the curvature
        /// </summary>
        public double? RadiusOfCurvature { get => Curvature == 0 ? null : 1.0 / Curvature; set => Curvature = 1.0 / value; }
        /// <summary>
        /// the toolface at a reference position
        /// </summary>
        public double? ReferenceToolface { get; set; }
    }
}
