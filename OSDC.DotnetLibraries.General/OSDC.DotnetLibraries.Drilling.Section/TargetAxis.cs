using System;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// A place the trajectory has to reach, and optionally the direction it has to be heading in when it
    /// gets there.
    ///
    /// A target given as a position alone is a point to pass through, and the direction the trajectory
    /// leaves it in is whatever the section arriving there ends up with. A target given with an
    /// inclination and an azimuth as well is an axis: the trajectory has to arrive at that position
    /// heading that way, which is one more pair of conditions and takes a pair of sections to satisfy.
    /// </summary>
    [Serializable]
    public class TargetAxis
    {
        /// <summary>
        /// The position to reach, and the attitude to reach it with when there is one.
        /// </summary>
        public TrajectoryPoint3D Station { get; set; } = new TrajectoryPoint3D();

        /// <summary>
        /// Which curve to draw the section or sections leading to this target with. When it is left
        /// undefined the path uses the type it carries itself, so a path of one kind throughout needs
        /// nothing set here.
        /// </summary>
        public SectionCurveType? CurveType { get; set; } = null;

        public TargetAxis()
        {
        }

        /// <summary>
        /// A target to pass through, with no direction imposed.
        /// </summary>
        public TargetAxis(double x, double y, double z)
        {
            Station.Set(x, y, z);
        }

        /// <summary>
        /// A target to reach heading a given way.
        /// </summary>
        public TargetAxis(double x, double y, double z, double inclination, double azimuth)
        {
            Station.Set(x, y, z);
            Station.Inclination = inclination;
            Station.Azimuth = azimuth;
        }

        /// <summary>
        /// True when a direction is imposed at this target as well as a position.
        /// </summary>
        public bool HasAxis
        {
            get
            {
                return Station != null &&
                       Numeric.IsDefined(Station.Inclination) && Numeric.IsDefined(Station.Azimuth);
            }
        }

        /// <summary>
        /// True when the target at least says where it is.
        /// </summary>
        public bool HasPosition
        {
            get
            {
                return Station != null &&
                       Numeric.IsDefined(Station.X) && Numeric.IsDefined(Station.Y) && Numeric.IsDefined(Station.Z);
            }
        }
    }
}
