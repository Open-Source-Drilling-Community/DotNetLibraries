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

        /// <summary>
        /// Converts 'this' point to local NED coordinates relative to <paramref name="reference"/>.
        /// </summary>
        public virtual Point3D? GeodeticToLocalNED(GeodeticPoint3D reference)
        {
            return GeodeticTransforms.GeodeticToLocalNED(this, reference);
        }

        /// <summary>
        /// Converts local NED coordinates to geodetic coordinates using <paramref name="reference"/>.
        /// </summary>
        public static GeodeticPoint3D? LocalNEDToGeodetic(Point3D localNed, GeodeticPoint3D reference)
        {
            return GeodeticTransforms.LocalNEDToGeodetic(localNed, reference);
        }

        /// <summary>
        /// Deterministic WGS84 / local NED conversion utilities.
        /// TVD is treated as positive downward, therefore ellipsoidal height is h = -TVD.
        /// </summary>
        public static class GeodeticTransforms
        {
            // WGS84 spheroid parameters
            private const double SemiMajorAxis = 6378137.0;
            private const double Flattening = 1.0 / 298.257223563;
            private const double FirstEccentricitySquared = Flattening * (2.0 - Flattening);
            private const double SemiMinorAxis = SemiMajorAxis * (1.0 - Flattening);
            private const double SecondEccentricitySquared = (SemiMajorAxis * SemiMajorAxis - SemiMinorAxis * SemiMinorAxis) / (SemiMinorAxis * SemiMinorAxis);

            public static Point3D? GeodeticToLocalNED(GeodeticPoint3D? point, GeodeticPoint3D? reference)
            {
                if (!IsValid(point) || !IsValid(reference))
                {
                    return null;
                }

                (double x, double y, double z) = GeodeticToGeocentric(point!);
                (double x0, double y0, double z0) = GeodeticToGeocentric(reference!);

                double dx = x - x0;
                double dy = y - y0;
                double dz = z - z0;

                double[,] c = CreateGeocentricToNedRotation(reference!);

                double north = c[0, 0] * dx + c[0, 1] * dy + c[0, 2] * dz;
                double east = c[1, 0] * dx + c[1, 1] * dy + c[1, 2] * dz;
                double down = c[2, 0] * dx + c[2, 1] * dy + c[2, 2] * dz;

                return new Point3D(north, east, down);
            }

            public static GeodeticPoint3D? LocalNEDToGeodetic(Point3D? localNed, GeodeticPoint3D? reference)
            {
                if (localNed?.X == null || localNed.Y == null || localNed.Z == null || !IsValid(reference))
                {
                    return null;
                }

                (double x0, double y0, double z0) = GeodeticToGeocentric(reference!);

                double[,] c = CreateGeocentricToNedRotation(reference!);
                double[,] ct =
                {
                { c[0, 0], c[1, 0], c[2, 0] },
                { c[0, 1], c[1, 1], c[2, 1] },
                { c[0, 2], c[1, 2], c[2, 2] }
            };

                double dx = ct[0, 0] * localNed.X.Value + ct[0, 1] * localNed.Y.Value + ct[0, 2] * localNed.Z.Value;
                double dy = ct[1, 0] * localNed.X.Value + ct[1, 1] * localNed.Y.Value + ct[1, 2] * localNed.Z.Value;
                double dz = ct[2, 0] * localNed.X.Value + ct[2, 1] * localNed.Y.Value + ct[2, 2] * localNed.Z.Value;

                return GeocentricToGeodetic(x0 + dx, y0 + dy, z0 + dz);
            }

            public static double[,] CreateGeocentricToNedRotation(GeodeticPoint3D reference)
            {
                double lat = reference.LatitudeWGS84!.Value;
                double lon = reference.LongitudeWGS84!.Value;

                double sinLat = System.Math.Sin(lat);
                double cosLat = System.Math.Cos(lat);
                double sinLon = System.Math.Sin(lon);
                double cosLon = System.Math.Cos(lon);

                return new[,]
                {
                { -sinLat * cosLon, -sinLat * sinLon,  cosLat },
                { -sinLon,            cosLon,           0.0    },
                { -cosLat * cosLon, -cosLat * sinLon, -sinLat }
            };
            }

            private static (double x, double y, double z) GeodeticToGeocentric(GeodeticPoint3D point)
            {
                double lat = point.LatitudeWGS84!.Value;
                double lon = point.LongitudeWGS84!.Value;
                double h = -point.TvdWGS84!.Value;

                double sinLat = System.Math.Sin(lat);
                double cosLat = System.Math.Cos(lat);
                double sinLon = System.Math.Sin(lon);
                double cosLon = System.Math.Cos(lon);

                double n = SemiMajorAxis / System.Math.Sqrt(1.0 - FirstEccentricitySquared * sinLat * sinLat);

                double x = (n + h) * cosLat * cosLon;
                double y = (n + h) * cosLat * sinLon;
                double z = (n * (1.0 - FirstEccentricitySquared) + h) * sinLat;
                return (x, y, z);
            }

            private static GeodeticPoint3D GeocentricToGeodetic(double x, double y, double z)
            {
                double p = System.Math.Sqrt(x * x + y * y);
                double theta = System.Math.Atan2(z * SemiMajorAxis, p * SemiMinorAxis);
                double sinTheta = System.Math.Sin(theta);
                double cosTheta = System.Math.Cos(theta);

                double lon = System.Math.Atan2(y, x);
                double lat = System.Math.Atan2(
                    z + SecondEccentricitySquared * SemiMinorAxis * sinTheta * sinTheta * sinTheta,
                    p - FirstEccentricitySquared * SemiMajorAxis * cosTheta * cosTheta * cosTheta);

                double sinLat = System.Math.Sin(lat);
                double n = SemiMajorAxis / System.Math.Sqrt(1.0 - FirstEccentricitySquared * sinLat * sinLat);
                double h = p / System.Math.Cos(lat) - n;

                return new GeodeticPoint3D
                {
                    LatitudeWGS84 = lat,
                    LongitudeWGS84 = lon,
                    TvdWGS84 = -h
                };
            }

            private static bool IsValid(GeodeticPoint3D? point)
            {
                return point != null &&
                       point.LatitudeWGS84 != null &&
                       point.LongitudeWGS84 != null &&
                       point.TvdWGS84 != null;
            }
        }
    }
}
