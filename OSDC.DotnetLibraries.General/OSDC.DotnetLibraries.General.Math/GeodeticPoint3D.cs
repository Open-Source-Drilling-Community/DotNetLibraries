using System;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// Describes a 3D geodetic point defined in the global reference system WGS84.
    /// The coordinates are stored as latitude, longitude, and TVD only.
    /// This type is intentionally purely geodetic and does not inherit from Point3D
    /// or expose Cartesian X/Y/Z coordinates.
    /// </summary>
    [Serializable]
    public class GeodeticPoint3D : IGeodeticPoint3D
    {
        /// <summary>
        /// Latitude in degrees in the WGS84 reference system.
        /// </summary>
        public double? LatitudeWGS84 { get; set; }

        /// <summary>
        /// Longitude in degrees in the WGS84 reference system.
        /// </summary>
        public double? LongitudeWGS84 { get; set; }

        /// <summary>
        /// True vertical depth associated with the WGS84 position.
        /// </summary>
        public double? TvdWGS84 { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GeodeticPoint3D()
        {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="pt">Source geodetic point.</param>
        public GeodeticPoint3D(IGeodeticPoint3D pt)
        {
            if (pt != null)
            {
                LatitudeWGS84 = pt.LatitudeWGS84;
                LongitudeWGS84 = pt.LongitudeWGS84;
                TvdWGS84 = pt.TvdWGS84;
            }
        }

        /// <summary>
        /// Constructor with initialization.
        /// </summary>
        /// <param name="latitudeWGS84">Latitude in degrees.</param>
        /// <param name="longitudeWGS84">Longitude in degrees.</param>
        /// <param name="tvdWGS84">True vertical depth.</param>
        public GeodeticPoint3D(double latitudeWGS84, double longitudeWGS84, double tvdWGS84)
        {
            LatitudeWGS84 = latitudeWGS84;
            LongitudeWGS84 = longitudeWGS84;
            TvdWGS84 = tvdWGS84;
        }

        /// <summary>
        /// Constructor with nullable initialization.
        /// </summary>
        /// <param name="latitudeWGS84">Latitude in degrees.</param>
        /// <param name="longitudeWGS84">Longitude in degrees.</param>
        /// <param name="tvdWGS84">True vertical depth.</param>
        public GeodeticPoint3D(double? latitudeWGS84, double? longitudeWGS84, double? tvdWGS84)
        {
            LatitudeWGS84 = latitudeWGS84;
            LongitudeWGS84 = longitudeWGS84;
            TvdWGS84 = tvdWGS84;
        }

        /// <summary>
        /// Constructor with initialization from an array.
        /// Expected order: [LatitudeWGS84, LongitudeWGS84, TvdWGS84].
        /// </summary>
        /// <param name="dat">Input array.</param>
        public GeodeticPoint3D(double[] dat)
        {
            if (dat != null && dat.Length >= 3)
            {
                LatitudeWGS84 = dat[0];
                LongitudeWGS84 = dat[1];
                TvdWGS84 = dat[2];
            }
        }

        /// <summary>
        /// Creates a deep copy.
        /// </summary>
        /// <returns>A cloned geodetic point.</returns>
        public virtual object Clone()
        {
            return new GeodeticPoint3D(this);
        }

        /// <summary>
        /// Sets the coordinates as undefined.
        /// </summary>
        public virtual void SetUndefined()
        {
            LatitudeWGS84 = Numeric.UNDEF_DOUBLE;
            LongitudeWGS84 = Numeric.UNDEF_DOUBLE;
            TvdWGS84 = Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// Indicates whether at least one coordinate is undefined.
        /// </summary>
        /// <returns>True if undefined; otherwise false.</returns>
        public virtual bool IsUndefined()
        {
            return Numeric.IsUndefined(LatitudeWGS84)
                || Numeric.IsUndefined(LongitudeWGS84)
                || Numeric.IsUndefined(TvdWGS84);
        }

        /// <summary>
        /// Sets all coordinates to zero.
        /// </summary>
        public virtual void SetZero()
        {
            LatitudeWGS84 = 0;
            LongitudeWGS84 = 0;
            TvdWGS84 = 0;
        }

        /// <summary>
        /// Indicates whether all coordinates are zero.
        /// </summary>
        /// <returns>True if all coordinates are zero; otherwise false.</returns>
        public virtual bool IsZero()
        {
            return Numeric.EQ(LatitudeWGS84, 0)
                && Numeric.EQ(LongitudeWGS84, 0)
                && Numeric.EQ(TvdWGS84, 0);
        }
    }
}
