namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// One survey station of an existing well, with the ellipse of uncertainty in the plane perpendicular
    /// to the borehole there.
    /// <para>
    /// Everything is SI: metres and radians. The angle of the semi-major axis is measured from the high
    /// side of the borehole, turning toward the right hand side, which is the usual toolface sense. On a
    /// vertical borehole the high side is not defined and the angle stops meaning anything; the ellipse
    /// is very nearly circular there, so little is lost, and north is used as the reference instead.
    /// </para>
    /// </summary>
    public class WellboreUncertaintyStation
    {
        /// <summary>
        /// the measured depth of the station, m
        /// </summary>
        public double MeasuredDepth { get; set; } = 0;

        /// <summary>
        /// the inclination at the station, rad
        /// </summary>
        public double Inclination { get; set; } = 0;

        /// <summary>
        /// the azimuth at the station, rad
        /// </summary>
        public double Azimuth { get; set; } = 0;

        /// <summary>
        /// the semi-major axis of the ellipse of uncertainty, m
        /// </summary>
        public double SemiMajorAxis { get; set; } = 0;

        /// <summary>
        /// the semi-minor axis of the ellipse of uncertainty, m
        /// </summary>
        public double SemiMinorAxis { get; set; } = 0;

        /// <summary>
        /// the angle of the semi-major axis from the high side of the borehole, rad
        /// </summary>
        public double SemiMajorAngle { get; set; } = 0;

        /// <summary>
        /// default constructor
        /// </summary>
        public WellboreUncertaintyStation()
        {
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        public WellboreUncertaintyStation(double measuredDepth, double inclination, double azimuth,
                                          double semiMajorAxis, double semiMinorAxis, double semiMajorAngle)
        {
            MeasuredDepth = measuredDepth;
            Inclination = inclination;
            Azimuth = azimuth;
            SemiMajorAxis = semiMajorAxis;
            SemiMinorAxis = semiMinorAxis;
            SemiMajorAngle = semiMajorAngle;
        }

        /// <summary>
        /// whether the station is usable
        /// </summary>
        public bool IsValid()
        {
            return !double.IsNaN(MeasuredDepth) && !double.IsNaN(Inclination) && !double.IsNaN(Azimuth)
                   && SemiMajorAxis > 0 && SemiMinorAxis > 0
                   && SemiMajorAxis >= SemiMinorAxis - 1e-9;
        }
    }
}
