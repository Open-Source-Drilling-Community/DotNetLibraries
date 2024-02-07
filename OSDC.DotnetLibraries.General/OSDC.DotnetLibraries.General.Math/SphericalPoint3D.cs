using OSDC.DotnetLibraries.General.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// This class defines a point in 3D space either in cartesian coordinates or in spherical coordinates
    /// </summary>
    public class SphericalPoint3D : Point3D
    {
        protected double? r_ = null;
        protected double? latitude_ = null;
        protected double? longitude_ = null;

        /// <summary>
        /// ensure that the spherical coordinates are updated
        /// </summary>
        public override double? X
        {
            get => base.X;
            set
            {
                base.X = value;
                ConvertToSpherical();
            }
        }
        /// <summary>
        /// ensure that the spherical coordinates are updated
        /// </summary>
        public override double? Y
        {
            get => base.Y;
            set
            {
                base.Y = value;
                ConvertToSpherical();
            }
        }
        /// <summary>
        /// ensure that the spherical coordinates are updated
        /// </summary>
        public override double? Z
        {
            get => base.Z;
            set
            {
                base.Z = value;
                ConvertToSpherical();
            }
        }
        /// <summary>
        /// ensure that the cartisian coordinates are updated
        /// </summary>
        public virtual double? R
        {
            get
            {
                return r_;               
            }
            set
            {
                r_ = value;
                ConvertToCartesian();
            }
        }
        /// <summary>
        /// ensure that the cartesian coordinates are updated
        /// </summary>
        public virtual double? Latitude
        {
            get { return latitude_; }
            set
            {
                latitude_ = value;
                ConvertToCartesian();
            }
        }
        /// <summary>
        /// ensure that the cartesian coordinates are updated
        /// </summary>
        public virtual double? Longitude
        {
            get { return longitude_; }
            set
            {
                longitude_ = value;
                ConvertToCartesian();
            }
        }


        /// <summary>
        /// set the 3 cartesian coordinates at the same time
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetCartesian(double x, double y, double z)
        {
            base.X = x;
            base.Y = y;
            base.Z = z;
            ConvertToSpherical();
        }
        /// <summary>
        /// set the 3 spherical coordinates at the same time
        /// </summary>
        /// <param name="r"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        public void SetSpherical(double r, double lat, double lon)
        {
            r_ = r;
            latitude_ = lat;
            longitude_ = lon;
            ConvertToCartesian();
        }

        private void ConvertToCartesian()
        {
            if (r_ != null && latitude_ != null && longitude_ != null)
            {
                double r = r_.Value;
                double lat = latitude_.Value;
                double lon = longitude_.Value;
                double cosLat = System.Math.Cos(lat);
                base.X = r * cosLat * System.Math.Cos(lon);
                base.Y = r * cosLat * System.Math.Sin(lon);
                base.Z = r * System.Math.Sin(lat);
            }
        }

        private void ConvertToSpherical()
        {
            if (X != null && Y != null && Z != null)
            {
                double x = X.Value;
                double y = Y.Value;
                double z = Z.Value;
                r_ = System.Math.Sqrt(x * x + y * y + z * z);
                if (!Numeric.EQ(r_, 0))
                {
                    latitude_ = System.Math.Asin(z / r_.Value);
                    longitude_ = System.Math.Atan2(y, x);
                }
            }
        }
    }
}
