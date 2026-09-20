namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// A stand-in ellipse of uncertainty, for surveys that carry only measured depth, inclination and
    /// azimuth.
    /// <para>
    /// It is not an error model and makes no claim to be one. It exists so that the generation can be
    /// exercised on ordinary survey files, and so that the sensitivity of the answer to the size of the
    /// volumes can be measured, which matters more than it might seem: the difference between an
    /// uncertainty taken well by well and one taken between two wells sharing a survey reference decides
    /// whether the shallow section of a cluster is passable at all.
    /// </para>
    /// </summary>
    public class NominalUncertaintyModel
    {
        /// <summary>
        /// the radius at the wellhead, m. Near surface it is the conductor and the casing that dominate,
        /// not the position uncertainty.
        /// </summary>
        public double BoreholeRadius { get; set; } = 0.25;

        /// <summary>
        /// how fast the semi-minor axis grows with measured depth, m per m
        /// </summary>
        public double Growth { get; set; } = 0.001;

        /// <summary>
        /// the ratio of the semi-major to the semi-minor axis
        /// </summary>
        public double Anisotropy { get; set; } = 1.5;

        /// <summary>
        /// the angle of the semi-major axis from the high side, rad
        /// </summary>
        public double SemiMajorAngle { get; set; } = 0.5 * System.Math.PI;

        /// <summary>
        /// the semi-minor axis at the given measured depth, m
        /// </summary>
        public double GetSemiMinorAxis(double measuredDepth)
        {
            double grown = Growth * System.Math.Abs(measuredDepth);
            return grown > BoreholeRadius ? grown : BoreholeRadius;
        }

        /// <summary>
        /// the semi-major axis at the given measured depth, m
        /// </summary>
        public double GetSemiMajorAxis(double measuredDepth)
        {
            return GetSemiMinorAxis(measuredDepth) * (Anisotropy > 1.0 ? Anisotropy : 1.0);
        }
    }
}
