using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class CurvilinearPoint3D : Point3D, ICurvilinear3D
    {
        /// <summary>
        /// the curvilinear abscissa
        /// </summary>
        public double? Abscissa { get; set; } = null;
        /// <summary>
        /// the inclination (0 is vertical)
        /// </summary>
        public double? Inclination { get; set; } = null;
        /// <summary>
        /// the azimuth (0 is in the x-direction)
        /// </summary>
        public double? Azimuth { get; set; } = null;
        /// <summary>
        /// Default constructor
        /// </summary>
        public CurvilinearPoint3D() : base()
        {
        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public CurvilinearPoint3D(CurvilinearPoint3D src) : base(src)
        {
            if (src != null)
            {
                Abscissa = src.Abscissa;
                Inclination = src.Inclination;
                Azimuth = src.Azimuth;
            }
        }
        /// <summary>
        /// equal at numeric accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool EQ(ICurvilinear3D cmp)
        {
            if (cmp == null) return false;
            return base.Equals(cmp) && Numeric.EQ(Abscissa, cmp.Abscissa) && Numeric.EQ(Inclination, cmp.Inclination) && Numeric.EQ(Azimuth, cmp.Azimuth);
        }
        /// <summary>
        /// equal at given accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool EQ(ICurvilinear3D cmp, double precision)
        {
            if (cmp == null) return false;
            return base.EQ(cmp, precision) && Numeric.EQ(Abscissa, cmp.Abscissa, precision) && Numeric.EQ(Inclination, cmp.Inclination, precision) && Numeric.EQ(Azimuth, cmp.Azimuth, precision);
        }
    }
}
