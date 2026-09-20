using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Section.UnitTest
{
    /// <summary>
    /// Shared scaffolding for the section tests.
    ///
    /// A combination method is given some of the quantities describing a curve and has to work out the
    /// rest. Several of those combinations do not have a unique answer: a curve crossing a given true
    /// vertical depth crosses it on the way up as well as on the way down, and an azimuth is carried
    /// wrapped so the turn it implies is only known up to whole turns. Checking such a method by
    /// comparing its end point against the curve that generated the inputs therefore reports a valid
    /// alternative answer as a large error.
    ///
    /// So the tests below check what is actually required instead: that the curve which comes back
    /// honours the quantities it was given. That is the property the caller depends on, and it is well
    /// defined whichever of the admissible curves the method chooses to return.
    /// </summary>
    public static class SectionTestHelper
    {
        /// <summary>
        /// Angles agree to about a nanoradian and lengths to about a nanometre. Every combination is
        /// solved in closed form, so the residuals sit near the rounding of a double and this leaves a
        /// wide margin above them.
        /// </summary>
        public const double Tolerance = 1e-9;

        /// <summary>
        /// The number of random curves each sweep exercises. The seed is fixed so that a failure can be
        /// reproduced exactly.
        /// </summary>
        public const int SweepCount = 4000;

        public const int Seed = 20260914;

        public static TrajectoryPoint3D StartPoint(double inclination, double azimuth)
        {
            return new TrajectoryPoint3D
            {
                X = 0.0,
                Y = 0.0,
                Z = 500.0,
                Inclination = inclination,
                Azimuth = azimuth,
                Abscissa = 1000.0
            };
        }

        /// <summary>
        /// The difference between two azimuths taken the short way round.
        /// </summary>
        public static double WrappedDifference(double? a, double? b)
        {
            double difference = (double)a - (double)b;
            return difference - 2.0 * System.Math.PI * System.Math.Round(difference / (2.0 * System.Math.PI));
        }

        public static double Difference(double? a, double? b)
        {
            return System.Math.Abs((double)a - (double)b);
        }

        /// <summary>
        /// True when the section produced a usable end point rather than declining or returning nothing.
        /// </summary>
        public static bool Produced(TrajectoryPoint3D end)
        {
            return end != null &&
                   end.X != null && !double.IsNaN((double)end.X) &&
                   end.Y != null && !double.IsNaN((double)end.Y) &&
                   end.Z != null && !double.IsNaN((double)end.Z);
        }
    }
}
