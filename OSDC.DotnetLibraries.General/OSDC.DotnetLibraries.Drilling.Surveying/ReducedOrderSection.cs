namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    /// <summary>
    /// One analytic curve of a reduced order trajectory, running between two measured depths.
    ///
    /// A section is parameterised relative to the direction it inherits from the section before it, which
    /// is what makes the direction continuous at every junction without anything having to enforce it. It
    /// therefore does not carry a starting attitude of its own: the attitude the whole trajectory sets
    /// off at, together with the sections in order, settles every direction along it.
    ///
    /// Everything is SI: measured depths in metres, rates and curvatures in radians per metre, angles in
    /// radians.
    /// </summary>
    public class ReducedOrderSection
    {
        /// <summary>
        /// Which of the four curves this section is drawn with.
        /// </summary>
        public ReducedOrderCurveType CurveType { get; }

        /// <summary>
        /// The measured depth the section starts at, m.
        /// </summary>
        public double StartMeasuredDepth { get; }

        /// <summary>
        /// The measured depth the section ends at, m.
        /// </summary>
        public double EndMeasuredDepth { get; }

        /// <summary>
        /// How much hole the section covers, m.
        /// </summary>
        public double Length => EndMeasuredDepth - StartMeasuredDepth;

        /// <summary>
        /// The first of the two curve defining parameters, rad/m.
        ///
        /// For a hold it is zero and means nothing. For a circular arc it is the component of the turn
        /// rate vector along the first direction of the perpendicular frame of the inherited tangent. For
        /// a constant curvature and toolface curve, and for a constant build and turn curve, it is the
        /// build up rate.
        /// </summary>
        public double FirstParameter { get; }

        /// <summary>
        /// The second curve defining parameter, rad/m.
        ///
        /// For a hold it is zero and means nothing. For a circular arc it is the component of the turn
        /// rate vector along the second direction of the perpendicular frame. For a constant curvature
        /// and toolface curve it is the turn parameter, the curvature resolved across the high side. For
        /// a constant build and turn curve it is the turn rate, the rate of change of azimuth.
        /// </summary>
        public double SecondParameter { get; }

        /// <summary>
        /// The curvature of the section, rad/m, where it has one.
        ///
        /// A hold has no curvature and reports zero. A circular arc and a constant curvature and toolface
        /// curve each hold a curvature along their whole length. A constant build and turn curve does
        /// not: its curvature is the square root of the sum of the squared build up rate and the squared
        /// turn rate times the squared sine of the inclination, and the inclination changes along it, so
        /// nothing is reported.
        /// </summary>
        public double? Curvature { get; }

        /// <summary>
        /// The toolface angle of the section, rad, measured from the high side and positive to the right.
        ///
        /// Only a constant curvature and toolface curve holds one, so only that family reports it. A
        /// circular arc has a toolface angle which turns along it, and the two parameters of an arc are
        /// resolved onto a frame which is not the high side, so the angle between them is not a toolface.
        /// </summary>
        public double? ToolfaceAngle { get; }

        /// <summary>
        /// The build up rate of the section, rad/m, where it holds one: the two angle based families.
        /// </summary>
        public double? BuildUpRate { get; }

        /// <summary>
        /// The turn rate of the section, rad/m, where it holds one: only a constant build and turn curve.
        /// </summary>
        public double? TurnRate { get; }

        /// <summary>
        /// Builds a section. The derived readings are worked out here, once, from the family and the two
        /// parameters.
        /// </summary>
        public ReducedOrderSection(ReducedOrderCurveType curveType,
            double startMeasuredDepth, double endMeasuredDepth,
            double firstParameter, double secondParameter)
        {
            CurveType = curveType;
            StartMeasuredDepth = startMeasuredDepth;
            EndMeasuredDepth = endMeasuredDepth;
            FirstParameter = firstParameter;
            SecondParameter = secondParameter;

            switch (curveType)
            {
                case ReducedOrderCurveType.Hold:
                    Curvature = 0.0;
                    break;
                case ReducedOrderCurveType.CircularArc:
                    Curvature = System.Math.Sqrt(
                        firstParameter * firstParameter + secondParameter * secondParameter);
                    break;
                case ReducedOrderCurveType.ConstantCurvatureAndToolface:
                    Curvature = System.Math.Sqrt(
                        firstParameter * firstParameter + secondParameter * secondParameter);
                    ToolfaceAngle = System.Math.Atan2(secondParameter, firstParameter);
                    BuildUpRate = firstParameter;
                    break;
                default:
                    BuildUpRate = firstParameter;
                    TurnRate = secondParameter;
                    break;
            }
        }

        /// <summary>
        /// A short description of the section, for diagnostics.
        /// </summary>
        public override string ToString()
        {
            return $"{CurveType} {StartMeasuredDepth:F1} to {EndMeasuredDepth:F1} m "
                + $"({FirstParameter:E3}, {SecondParameter:E3}) rad/m";
        }
    }
}
