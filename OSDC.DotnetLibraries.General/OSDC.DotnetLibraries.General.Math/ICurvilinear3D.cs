using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface ICurvilinear3D : IPoint3D, IEquivalent<ICurvilinear3D>
    {
        /// <summary>
        /// the curvilinear abscissa or measured depth
        /// </summary>
        public double? Abscissa { get; set; }
        /// <summary>
        /// the inclination (0 when vertical, pi/2 when horizontal)
        /// </summary>
        public double? Inclination { get; set; }
        /// <summary>
        /// the azimuth (0 in the x-direction)
        /// </summary>
        public double? Azimuth { get; set; }
    }
}