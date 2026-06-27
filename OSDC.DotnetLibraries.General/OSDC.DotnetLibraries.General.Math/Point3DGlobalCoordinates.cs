using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// Describes a 3D point with Riemannian coordinates and geodetic coordinates on the WGS84 spheroid.
    /// </summary>
    [Serializable]
    public class Point3DGlobalCoordinates : Point3D, IPoint3DGlobalCoordinates, IEquivalent<IPoint3D>
    {
        private double? latitude_;
        private double? longitude_;
        private double? tvd_;

        /// <summary>
        /// Arc length from the equator to the latitude of the point.
        /// </summary>
        public override double? X
        {
            get => base.X;
            set
            {
                base.X = value;
                if (value != null && base.Y != null)
                {
                    SetLatitudeLongitude(value.Value, base.Y.Value);
                }
            }
        }

        /// <summary>
        /// Arc length from the Greenwich meridian to the longitude of the point along the latitude parallel.
        /// </summary>
        public override double? Y
        {
            get => base.Y;
            set
            {
                base.Y = value;
                if (base.X != null && value != null)
                {
                    SetLatitudeLongitude(base.X.Value, value.Value);
                }
            }
        }

        /// <summary>
        /// True vertical depth.
        /// </summary>
        public override double? Z
        {
            get => base.Z;
            set
            {
                base.Z = value;
                tvd_ = value;
            }
        }

        /// <summary>
        /// Synonym of X.
        /// </summary>
        public double? RiemannianNorth
        {
            get => X;
            set => X = value;
        }

        /// <summary>
        /// Synonym of Y.
        /// </summary>
        public double? RiemannianEast
        {
            get => Y;
            set => Y = value;
        }

        /// <summary>
        /// Latitude of the point on the WGS84 spheroid, in radians.
        /// </summary>
        public double? Latitude
        {
            get => latitude_;
            set
            {
                latitude_ = value;
                if (value != null && longitude_ != null)
                {
                    SetRiemannianNorthEast(value.Value, longitude_.Value);
                }
            }
        }

        /// <summary>
        /// Longitude of the point on the WGS84 spheroid, in radians.
        /// </summary>
        public double? Longitude
        {
            get => longitude_;
            set
            {
                longitude_ = value;
                if (latitude_ != null && value != null)
                {
                    SetRiemannianNorthEast(latitude_.Value, value.Value);
                }
            }
        }

        /// <summary>
        /// True vertical depth.
        /// </summary>
        public double? TVD
        {
            get => tvd_;
            set
            {
                tvd_ = value;
                base.Z = value;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Point3DGlobalCoordinates()
        {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Point3DGlobalCoordinates(IPoint3DGlobalCoordinates pt)
        {
            if (pt != null)
            {
                base.X = pt.X;
                base.Y = pt.Y;
                base.Z = pt.Z;
                latitude_ = pt.Latitude;
                longitude_ = pt.Longitude;
                tvd_ = pt.TVD;
            }
        }

        /// <summary>
        /// Copy constructor from a 3D point.
        /// </summary>
        public Point3DGlobalCoordinates(IPoint3D pt)
            : base(pt)
        {
            tvd_ = Z;
        }

        /// <summary>
        /// Constructor with Riemannian north/east and TVD initialization.
        /// </summary>
        public Point3DGlobalCoordinates(double x, double y, double z)
        {
            base.X = x;
            base.Y = y;
            Z = z;
            SetLatitudeLongitude(x, y);
        }

        /// <summary>
        /// Constructor with nullable Riemannian north/east and TVD initialization.
        /// </summary>
        public Point3DGlobalCoordinates(double? x, double? y, double? z)
        {
            base.X = x;
            base.Y = y;
            Z = z;
            if (x != null && y != null)
            {
                SetLatitudeLongitude(x.Value, y.Value);
            }
        }

        /// <summary>
        /// Constructor with initialization from an array.
        /// Expected order: [RiemannianNorth, RiemannianEast, TVD].
        /// </summary>
        public Point3DGlobalCoordinates(double[] dat)
        {
            if (dat != null && dat.Length >= 3)
            {
                base.X = dat[0];
                base.Y = dat[1];
                Z = dat[2];
                SetLatitudeLongitude(dat[0], dat[1]);
            }
        }

        /// <summary>
        /// Creates a deep copy.
        /// </summary>
        public override object Clone()
        {
            return new Point3DGlobalCoordinates(this);
        }

        /// <summary>
        /// Sets the coordinates as undefined.
        /// </summary>
        public override void SetUndefined()
        {
            base.SetUndefined();
            latitude_ = Numeric.UNDEF_DOUBLE;
            longitude_ = Numeric.UNDEF_DOUBLE;
            tvd_ = Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// Indicates whether at least one coordinate is undefined.
        /// </summary>
        public override bool IsUndefined()
        {
            return base.IsUndefined()
                || Numeric.IsUndefined(latitude_)
                || Numeric.IsUndefined(longitude_)
                || Numeric.IsUndefined(tvd_);
        }

        /// <summary>
        /// Sets all coordinates to zero.
        /// </summary>
        public override void SetZero()
        {
            base.SetZero();
            latitude_ = 0;
            longitude_ = 0;
            tvd_ = 0;
        }

        /// <summary>
        /// Indicates whether all coordinates are zero.
        /// </summary>
        public override bool IsZero()
        {
            return base.IsZero()
                && Numeric.EQ(latitude_, 0)
                && Numeric.EQ(longitude_, 0)
                && Numeric.EQ(tvd_, 0);
        }

        /// <summary>
        /// Sets Latitude and Longitude from Riemannian north/east coordinates.
        /// </summary>
        public void SetLatitudeLongitude(double riemannianNorth, double riemannianEast)
        {
            double latitude = LatitudeFromMeridianDistance(riemannianNorth);
            latitude_ = latitude;

            double parallelRadius = ParallelRadius(latitude);
            longitude_ = Numeric.EQ(parallelRadius, 0) ? null : riemannianEast / parallelRadius;
        }

        /// <summary>
        /// Sets Riemannian north/east coordinates from Latitude and Longitude.
        /// </summary>
        public void SetRiemannianNorthEast(double latitude, double longitude)
        {
            latitude_ = latitude;
            longitude_ = longitude;
            base.X = MeridianDistance(latitude);
            base.Y = ParallelRadius(latitude) * longitude;
        }

        /// <summary>
        /// Calculates the latitude corresponding to a meridian arc length from the equator.
        /// </summary>
        public static double LatitudeFromMeridianDistance(double meridianDistance)
        {
            double latitude = meridianDistance / Constants.EarthSemiMajorAxisWGS84;

            for (int i = 0; i < 12; i++)
            {
                double residual = MeridianDistance(latitude) - meridianDistance;
                double derivative = MeridianRadiusOfCurvature(latitude);
                double delta = residual / derivative;
                latitude -= delta;

                if (System.Math.Abs(delta) < 1e-14)
                {
                    break;
                }
            }

            return latitude;
        }

        /// <summary>
        /// Calculates meridian arc length from the equator to the latitude.
        /// </summary>
        public static double MeridianDistance(double latitude)
        {
            if (Numeric.EQ(latitude, 0))
            {
                return 0;
            }

            int intervals = 200;
            if (intervals % 2 != 0)
            {
                intervals++;
            }

            double step = latitude / intervals;
            double sum = 0;

            for (int i = 0; i <= intervals; i++)
            {
                double phi = i * step;
                double weight = i == 0 || i == intervals ? 1 : (i % 2 == 0 ? 2 : 4);
                sum += weight * MeridianRadiusOfCurvature(phi);
            }

            return sum * step / 3.0;
        }

        /// <summary>
        /// Calculates the meridian radius of curvature for the WGS84 spheroid.
        /// </summary>
        public static double MeridianRadiusOfCurvature(double latitude)
        {
            double a = Constants.EarthSemiMajorAxisWGS84;
            double e2 = FirstEccentricitySquared();
            double sinLatitude = System.Math.Sin(latitude);
            return a * (1.0 - e2) / System.Math.Pow(1.0 - e2 * sinLatitude * sinLatitude, 1.5);
        }

        /// <summary>
        /// Calculates the parallel radius used for Riemannian east.
        /// </summary>
        public static double ParallelRadius(double latitude)
        {
            double a = Constants.EarthSemiMajorAxisWGS84;
            double e2 = FirstEccentricitySquared();
            double sinLatitude = System.Math.Sin(latitude);
            double cosLatitude = System.Math.Cos(latitude);
            return a * cosLatitude / System.Math.Sqrt(1.0 - e2 * sinLatitude * sinLatitude);
        }

        private static double FirstEccentricitySquared()
        {
            double flattening = 1.0 / Constants.EarthInverseFlateningWGS84;
            return 2.0 * flattening - flattening * flattening;
        }
    }
}
