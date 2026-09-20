using System;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// One section of a complex path: whichever of its quantities the caller chooses to impose, and the
    /// curve it is to be drawn with. Everything left undefined is what the path works out.
    ///
    /// Whatever the curve, a section leaving a known station has three degrees of freedom, so three
    /// quantities settle it. They need not all be given on the same section: a complex path only asks for
    /// three times as many quantities as it has sections, spread as the caller likes provided every
    /// section carries at least one and no section is over-determined before the ones that would absorb
    /// the surplus.
    ///
    /// The quantities that count, at most one from the first line:
    ///   the length of the section, or the measured depth at its end
    ///   the inclination at its end
    ///   the azimuth at its end
    ///   the north, east and vertical coordinates of its end
    ///   the two parameters of its curve, which are the curvature and the toolface angle for a circular
    ///   arc and for a constant curvature and toolface curve, and the build up rate and the turn rate for
    ///   a constant build and turn curve
    /// </summary>
    [Serializable]
    public class ComplexPathSection
    {
        /// <summary>
        /// The curve this section is drawn with.
        /// </summary>
        public SectionCurveType CurveType { get; set; } = SectionCurveType.CircularArc;

        /// <summary>
        /// The station at the end of the section. Any of its position, inclination, azimuth and measured
        /// depth may be imposed, and the rest are worked out. This is also where the answer is written.
        /// </summary>
        public TrajectoryPoint3D End { get; set; } = new TrajectoryPoint3D();

        /// <summary>
        /// The along hole length of the section.
        /// </summary>
        public double? Length { get; set; } = null;

        /// <summary>
        /// The curvature, for a circular arc or a constant curvature and toolface curve.
        /// </summary>
        public double? Curvature { get; set; } = null;

        /// <summary>
        /// The toolface angle, for a circular arc or a constant curvature and toolface curve. On a circular
        /// arc it is quoted at the start of the section; on a constant curvature and toolface curve it is
        /// held all the way along.
        /// </summary>
        public double? Toolface { get; set; } = null;

        /// <summary>
        /// The build up rate, for a constant build and turn curve.
        /// </summary>
        public double? BUR { get; set; } = null;

        /// <summary>
        /// The turn rate, for a constant build and turn curve.
        /// </summary>
        public double? TurnRate { get; set; } = null;

        /// <summary>
        /// The section as it came out, once the path has been worked out.
        /// </summary>
        public ArcSection Solved { get; internal set; } = null;

        public ComplexPathSection()
        {
        }

        public ComplexPathSection(SectionCurveType curveType)
        {
            CurveType = curveType;
        }

        /// <summary>
        /// The two parameters of the curve this section is drawn with, in the order the forward
        /// construction takes them.
        /// </summary>
        internal double? FirstCurveParameter
        {
            get => CurveType == SectionCurveType.ConstantBuildAndTurn ? BUR : Curvature;
            set
            {
                if (CurveType == SectionCurveType.ConstantBuildAndTurn)
                {
                    BUR = value;
                }
                else
                {
                    Curvature = value;
                }
            }
        }

        internal double? SecondCurveParameter
        {
            get => CurveType == SectionCurveType.ConstantBuildAndTurn ? TurnRate : Toolface;
            set
            {
                if (CurveType == SectionCurveType.ConstantBuildAndTurn)
                {
                    TurnRate = value;
                }
                else
                {
                    Toolface = value;
                }
            }
        }

        /// <summary>
        /// True when a parameter belonging to another kind of curve has been set on this section, which is
        /// a mistake rather than something to be quietly ignored.
        /// </summary>
        internal bool HasParameterOfAnotherCurve
        {
            get
            {
                if (CurveType == SectionCurveType.ConstantBuildAndTurn)
                {
                    return Numeric.IsDefined(Curvature) || Numeric.IsDefined(Toolface);
                }
                return Numeric.IsDefined(BUR) || Numeric.IsDefined(TurnRate);
            }
        }

        /// <summary>
        /// How many quantities this section imposes. The length and the measured depth at the end say the
        /// same thing once the station before is known, so they count once between them.
        /// </summary>
        internal int ParameterCount
        {
            get
            {
                int count = 0;
                if (Numeric.IsDefined(Length) || (End != null && Numeric.IsDefined(End.Abscissa)))
                {
                    count++;
                }
                if (End != null && Numeric.IsDefined(End.Inclination))
                {
                    count++;
                }
                if (End != null && Numeric.IsDefined(End.Azimuth))
                {
                    count++;
                }
                if (End != null && Numeric.IsDefined(End.X))
                {
                    count++;
                }
                if (End != null && Numeric.IsDefined(End.Y))
                {
                    count++;
                }
                if (End != null && Numeric.IsDefined(End.Z))
                {
                    count++;
                }
                if (Numeric.IsDefined(FirstCurveParameter))
                {
                    count++;
                }
                if (Numeric.IsDefined(SecondCurveParameter))
                {
                    count++;
                }
                return count;
            }
        }
    }
}
