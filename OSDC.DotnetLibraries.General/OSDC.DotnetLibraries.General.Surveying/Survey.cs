using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace OSDC.DotnetLibraries.General.Surveying
{
    public class Survey : CurvilinearPoint3D
    {
        private double? latitude_ = null;
        private double? longitude_ = null;
        /// <summary>
        /// The length of the arc on the earth (modelled as a WGS84 spheroid) from the equator to the latitude of this point. 
        /// Positive in the north direction.
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Position)]
        [PositionReference(CommonProperty.PositionReferenceType.WGS84)]
        public override double? X
        {
            get => base.X;
            set
            {
                base.X = value;
                UpdateX(value);
            }
        }
        /// <summary>
        /// The length of the arc on the earth (modelled as a WGS84 spheroid) from the Greenwich meridian to the longitude of this point.
        /// Positive in the east direction.
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Position)]
        [PositionReference(CommonProperty.PositionReferenceType.WGS84)]
        public override double? Y
        {
            get => base.Y;
            set
            {
                base.Y = value;
                UpdateY(value);
            }
        }
        /// <summary>
        /// Latitude of the point on the WGS84 spheroid
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingPlaneAngle)]
        [PositionReference(CommonProperty.PositionReferenceType.WGS84)]
        public double? Latitude
        {
            get { return latitude_; }
            set
            {
                latitude_ = value;
                UpdateLatitude(value);
            }
        }
        /// <summary>
        /// Longitude of the point on the WGS84 spheroid
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingPlaneAngle)]
        [PositionReference(CommonProperty.PositionReferenceType.WGS84)]
        public double? Longitude
        {
            get { return longitude_; }
            set
            {
                longitude_ = value;
                UpdateLongitude(value);
            }
        }

         /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetLatitudeLongitude(double x, double y)
        {
            double f = 1.0 / Constants.EarthInverseFlateningWGS84;
            double b = Constants.EarthSemiMajorAxisWGS84 * (1.0 - f);
            double b2 = b * b;
            double a2 = Constants.EarthSemiMajorAxisWGS84 * Constants.EarthSemiMajorAxisWGS84;
            double m = 1.0 - b2 / a2;
            double latitude = SpecialFunctions.InverseEllipseE(x/ Constants.EarthSemiMajorAxisWGS84, m);
            Latitude = latitude;
            double sinLat = System.Math.Sin(latitude);
            double cosLat = System.Math.Cos(latitude);
            double R = Constants.EarthSemiMajorAxisWGS84 * System.Math.Sqrt(cosLat * cosLat + (b2 / a2) * sinLat * sinLat) / System.Math.Sqrt(1 - f * (2 - f) * sinLat * sinLat);
            if (Numeric.EQ(R, 0))
            {
                Longitude = null;
            }
            else
            {
                Longitude = y / R;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void SetXY(double latitude, double longitude)
        {
            double f = 1.0 / Constants.EarthInverseFlateningWGS84;
            double b = Constants.EarthSemiMajorAxisWGS84 * (1.0 - f);
            double b2 = b * b;
            double a2 = Constants.EarthSemiMajorAxisWGS84 * Constants.EarthSemiMajorAxisWGS84;
            double sinLat = System.Math.Sin(latitude);
            double cosLat = System.Math.Cos(latitude);
            double R = Constants.EarthSemiMajorAxisWGS84 * System.Math.Sqrt(cosLat * cosLat + (b2 / a2) * sinLat * sinLat) / System.Math.Sqrt(1 - f * (2 - f) * sinLat * sinLat);
            base.Y = R * longitude;
            double m = 1.0 - b2 / a2;
            base.X = Constants.EarthSemiMajorAxisWGS84 * SpecialFunctions.EllipseE(latitude, m);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Survey() : base()
        {
        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Survey(Survey src) : base(src)
        {
            if (src != null)
            {
                Abscissa = src.Abscissa;
                Inclination= src.Inclination;
                Azimuth= src.Azimuth;
            }
        }
        /// <summary>
        /// complete the next survey depending on its unknown values
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool Calculate(ICurvilinear3D next)
        {
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null)
            {
                return false;
            }
            if (next.Abscissa != null && next.Inclination != null && next.Azimuth != null)
            {
                return CompleteSIA(next);
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Apply the minimum curvature method between this survey and the next
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteSIA(ICurvilinear3D next)
        {
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null || next.Abscissa == null || next.Inclination == null || next.Azimuth == null)
            {
                return false;
            }
            double x1 = (double)X;
            double y1 = (double)Y;
            double z1 = (double)Z;
            double i1 = (double)Inclination;
            double a1 = (double)Azimuth;
            double s1 = (double)Abscissa;
            double s2 = (double)next.Abscissa;
            double i2 = (double)next.Inclination;
            double a2 = (double)next.Azimuth;
            double dm = s2 - s1;
            if (Numeric.EQ(dm, 0))
            {
                next.X = x1;
                next.Y = y1;
                next.Z = z1;
                next.Inclination = i1;
                next.Azimuth = a1;
                next.Abscissa = s2;
                return true;
            }
            double ci1 = System.Math.Cos(i1);
            double si1 = System.Math.Sin(i1);
            double ca1 = System.Math.Cos(a1);
            double sa1 = System.Math.Sin(a1);
            double ci2 = System.Math.Cos(i2);
            double si2 = System.Math.Sin(i2);
            double ca2 = System.Math.Cos(a2);
            double sa2 = System.Math.Sin(a2);
            double dl = System.Math.Acos(System.Math.Cos(i2 - i1) - (1 - System.Math.Cos(a2 - a1)) * si2 * si1);
            double dls = dl / dm;
            double rf = 1.0;
            if (!Numeric.EQ(dl, 0))
            {
                rf = (2.0 / dl) * System.Math.Tan(dl / 2.0);
            }
            next.X = x1 + 0.5 * dm * rf * (si1 * ca1 + si2 * ca2);
            next.Y = y1 + 0.5 * dm * rf * (si1 * sa1 + si2 * sa2);
            next.Z = z1 + 0.5 * dm * rf * (ci1 + ci2);
            return true;
        }
        /// <summary>
        /// Interpolate the result in between this survey and the next at a given curvilinear abscissa
        /// </summary>
        /// <param name="next"></param>
        /// <param name="s"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool InterpolateAtAbscissa(ICurvilinear3D next, double s, ref ICurvilinear3D result)
        {
            if (next == null || result == null ||
                X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null ||
                next.X == null || next.Y == null || next.Z == null || next.Inclination == null || next.Azimuth == null || next.Abscissa == null)
            {
                return false;
            }
            if (Numeric.EQ(s, Abscissa))
            {
                result.X = X;
                result.Y = Y;  
                result.Z = Z;
                result.Abscissa = Abscissa;
                result.Inclination = Inclination;
                result.Azimuth= Azimuth;
                return true;
            }
            if (!Numeric.IsBetween(s, (double)Abscissa, (double)next.Abscissa))
            {
                return false;
            }
            result.Abscissa = s;
            double x1 = (double)X;
            double y1 = (double)Y;
            double z1 = (double)Z;
            double i1 = (double)Inclination;
            double a1 = (double)Azimuth;
            double s1 = (double)Abscissa;
            double x2 = (double)next.X;
            double y2 = (double)next.Y;
            double z2 = (double)next.Z;
            double i2 = (double)next.Inclination;
            double a2 = (double)next.Azimuth;
            double s2 = (double)next.Abscissa;
            double sini1 = System.Math.Sin(i1);
            double sini2 = System.Math.Sin(i2);
            double DL = System.Math.Acos(System.Math.Cos(i2 - i1) - (1 - System.Math.Cos(a2 - a1)) * sini2 * sini1);
            double DM = s2 - s1;
            if (Numeric.EQ(DM, 0))
            {
                if (Numeric.EQ(s1, s))
                {
                    result.X = X;
                    result.Y = Y;
                    result.Z = Z;
                    result.Abscissa = Abscissa;
                    result.Inclination = Inclination;
                    result.Azimuth = Azimuth;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                double dx = x2 - x1;
                double dy = y2 - y1;
                double dz = z2 - z1;
                double ci1 = System.Math.Cos(i1);
                double si1 = System.Math.Sin(i1);
                double ca1 = System.Math.Cos(a1);
                double sa1 = System.Math.Sin(a1);
                double ci2 = System.Math.Cos(i2);
                double si2 = System.Math.Sin(i2);
                double ca2 = System.Math.Cos(a2);
                double sa2 = System.Math.Sin(a2);
                double x = dx * ci1 * ca1 + dy * sa1 * ci1 - dz * si1;
                double y = dy * ca1 - dz * sa1;
                double l = System.Math.Sqrt(x * x + y * y);
                double numerator = si2 * System.Math.Sin(a2 - a1) ;
                double denominator = si2 * ci1 * System.Math.Cos(a2 - a1) - si1 * ci2;
                double tf = System.Math.Atan2(numerator, denominator);
                double dls = DL / DM;
                double dm = s - s1;
                if (Numeric.EQ(dls, 0))
                {
                    result.Inclination = i1;
                    result.Azimuth = a1;
                    result.X = x1 + dm * ca1 * si1;
                    result.Y = y1 + dm * sa1 * si1;
                    result.Z = z1 + dm * ci1;
                    return true;
                }
                else
                {
                    if (Numeric.EQ(i1, 0))
                    {
                        double incl = dm * dls;
                        result.Inclination = incl;
                        double az = System.Math.Atan2(dy, dx);
                        result.Azimuth = az;
                        double ci = System.Math.Cos(incl);
                        result.X = x1 + System.Math.Cos(az) * (1 - ci) / dls;
                        result.Y = y1 + System.Math.Sin(az)*(1-ci) / dls;
                        result.Z = z1 + System.Math.Sin(incl) / dls;
                        return true;
                    }
                    else
                    {
                        if (Numeric.IsUndefined(tf))
                        {
                            return false;
                        }
                        double theta = dm * dls;
                        double deltaXp = (1 - System.Math.Cos(theta)) / dls;
                        double deltaZp = System.Math.Sin(theta) / dls;
                        double ctf = System.Math.Cos(tf);
                        double stf = System.Math.Sin(tf);
                        result.X = x1 + deltaXp * (ctf * ca1 * ci1 - sa1 * stf) + deltaZp * si1 * ca1;
                        result.Y = y1 + deltaXp * (stf * ca1 + ctf * sa1 * ci1) + deltaZp * si1 * sa1;
                        result.Z = z1 - deltaXp * ctf * si1 + deltaZp * ci1;
                        double deltaXt = System.Math.Sin(theta);
                        double deltaZt = System.Math.Cos(theta);
                        double xt = deltaXt * (ctf * ca1 * ci1 - sa1 * stf) * deltaZt * si1 * ca1;
                        double yt = deltaXt * (stf * ca1 + ctf * sa1 * ci1) + deltaZt * si1 * sa1;
                        double zt = -deltaXt * ctf * si1 + deltaZt * ci1;
                        result.Inclination = System.Math.Acos(zt);
                        if (Numeric.EQ(z1, 1))
                        {
                            result.Azimuth = a1;
                        }
                        else
                        {
                            double omega = System.Math.Acos(xt / System.Math.Sqrt(xt * xt + yt * yt));
                            if (Numeric.GE(yt, 0))
                            {
                                result.Azimuth = omega;
                            }
                            else
                            {
                                result.Azimuth = 2 * System.Math.PI - omega;
                            }
                        }
                        return true;
                    }
                }
            }
        }
        private void UpdateX(double? value)
        {
            if (value != null && Y != null)
            {
                SetLatitudeLongitude((double)value, (double)Y);
            }
        }

        private void UpdateY(double? value)
        {
            if (value != null && X != null)
            {
                SetLatitudeLongitude((double)X, (double)value);

            }
        }

        private void UpdateLatitude(double? value)
        {
            if (value != null && longitude_ != null)
            {
                SetXY((double)value, (double)longitude_);
            }
        }
        private void UpdateLongitude(double? value)
        {
            if (value != null && latitude_ != null)
            {
                SetXY((double)latitude_, (double)value);
            }
        }

    }
}
