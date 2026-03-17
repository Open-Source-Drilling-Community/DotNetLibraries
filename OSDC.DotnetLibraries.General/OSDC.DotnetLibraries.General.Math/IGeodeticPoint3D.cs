using OSDC.DotnetLibraries.General.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// an interface for 3D geodetic points defined in the global reference system WGS84.
    /// The coordinates are stored as latitude, longitude, and TVD only.
    /// This type is intentionally purely geodetic and does not inherit from Point3D
    /// or expose Cartesian X/Y/Z coordinates.
    /// </summary>
    public interface IGeodeticPoint3D
    {
        double? LatitudeWGS84 { get; set; }

        double? LongitudeWGS84 { get; set; }

        double? TvdWGS84 { get; set; }

        /// <summary>
        /// Creates a deep copy.
        /// </summary>
        /// <returns>A cloned geodetic point.</returns>
        public object Clone();

        /// <summary>
        /// Sets the coordinates as undefined.
        /// </summary>
        public void SetUndefined();

        /// <summary>
        /// Indicates whether at least one coordinate is undefined.
        /// </summary>
        /// <returns>True if undefined; otherwise false.</returns>
        public bool IsUndefined();

        /// <summary>
        /// Sets all coordinates to zero.
        /// </summary>
        public void SetZero();

        /// <summary>
        /// Indicates whether all coordinates are zero.
        /// </summary>
        /// <returns>True if all coordinates are zero; otherwise false.</returns>
        public bool IsZero();
    }
}
