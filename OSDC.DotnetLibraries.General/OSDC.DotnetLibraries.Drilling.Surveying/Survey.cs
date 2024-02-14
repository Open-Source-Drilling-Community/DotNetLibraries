﻿using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System.Text.Json.Serialization;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    public class Survey : CurvilinearPoint3D
    {
        private double? latitude_ = null;
        private double? longitude_ = null;

        /// <summary>
        /// synonym of Abscsissa
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [DepthReference(CommonProperty.DepthReferenceType.WGS84)]
        public double? MD {  get => base.Abscissa; set => base.Abscissa = value; }

        /// <summary>
        /// synonym of Z
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [DepthReference(CommonProperty.DepthReferenceType.WGS84)]
        public double? TVD { get => base.Z; set => base.Z = value; }

        /// <summary>
        /// redefinition to add drilling properties
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingPlaneAngle)]
        [AzimuthReference(CommonProperty.AzimuthReferenceType.TrueNorth)]
        public override double? Azimuth { get => base.Azimuth; set => base.Azimuth = value; }

        /// <summary>
        /// redefinition to add drilling properties
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingPlaneAngle)]
        public override double? Inclination { get => base.Inclination; set => base.Inclination = value; }

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
        /// Synonym of X. However, it is called Riemannian because the x-coordinate is defined in a Riemannian space
        /// of curvature corresponding to the Earth spheroid. The RiemannianNorth is the arc length from the equator
        /// to the latitude of the point.
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Position)]
        [PositionReference(CommonProperty.PositionReferenceType.WGS84)]
        public double? RiemannianNorth
        {
            get => X; set => X = value;
        }
        /// <summary>
        /// Synonym of Y. However, it is called Riemannian because the y-coordinate is defined in a Riemannian space
        /// of curvature corresponding to the Earth spheroid. The RiemannianEast is the arc length from the Greenwich meridian
        /// to the longitude of the point following the parallel at that latitude.
        /// </summary>
        public double? RiemannianEast
        {
            get => Y; set => Y = value;
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
        /// The local curvature at this Survey calculated using the minimum curvature method
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingCurvature)]
        public double? Curvature { get; set; } = null;
        /// <summary>
        /// The local toolface at this Survey calculated using the equation from Sawaryn and Thorogood (2005) 
        /// https://doi.org/10.2118/84246-PA
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingPlaneAngle)]
        public double? Toolface { get; set; } = null;
        /// <summary>
        /// The local build up rate at this Survey calculated using a finite difference method.
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingCurvature)]
        public double? BUR { get; set; } = null;
        /// <summary>
        /// The local turn rate at this Survey calculated using a finite difference method.
        /// </summary>
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingCurvature)]
        public double? TUR { get; set; } = null;

         /// <summary>
        /// 
        /// </summary>
        /// <param name="riemannianNorth"></param>
        /// <param name="riemannianEast"></param>
        public void SetLatitudeLongitude(double riemannianNorth, double riemannianEast)
        {
            double f = 1.0 / Constants.EarthInverseFlateningWGS84;
            double b = Constants.EarthSemiMajorAxisWGS84 * (1.0 - f);
            double b2 = b * b;
            double a2 = Constants.EarthSemiMajorAxisWGS84 * Constants.EarthSemiMajorAxisWGS84;
            double m = 1.0 - b2 / a2;
            double latitude = SpecialFunctions.InverseEllipseE(riemannianNorth/ Constants.EarthSemiMajorAxisWGS84, m);
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
                Longitude = riemannianEast / R;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void SetRiemannianNorthEast(double latitude, double longitude)
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
        /// return a SphericalPoint3D referred to a global coordinate system centered at the center of the Eart.
        /// </summary>
        /// <returns></returns>
        public SphericalPoint3D? GetSphericalPoint()
        {
            if (Latitude != null && Longitude != null && Z != null)
            {
                double a = Constants.EarthSemiMajorAxisWGS84;
                double f = 1.0 / Constants.EarthInverseFlateningWGS84;
                double b = a * (1.0 - f);
                double e2 = (a * a - b * b) / (a * a);
                double r = a / System.Math.Sqrt(1 - e2 * System.Math.Sin(Latitude.Value));
                r -= Z.Value;
                return new SphericalPoint3D() { Latitude = Latitude, Longitude = Longitude, R = r };
            }
            else
            {
                return null;
            }
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
        public bool Calculate(CurvilinearPoint3D next)
        {
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null)
            {
                return false;
            }
            if (next.Abscissa != null && next.Inclination != null && next.Azimuth != null)
            {
                return CompleteSIA(next);
            }
            else if (next.X != null && next.Y != null && next.Z != null)
            {
                return CompleteXYZ(next);
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
        public bool CompleteSIA(CurvilinearPoint3D next)
        {
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null || next.Abscissa == null || next.Inclination == null || next.Azimuth == null)
            {
                return false;
            }
            double x1 = X.Value;
            double y1 = Y.Value;
            double z1 = Z.Value;
            double i1 = Inclination.Value;
            double a1 = Azimuth.Value;
            double s1 = Abscissa.Value;
            double s2 = next.Abscissa.Value;
            double i2 = next.Inclination.Value;
            double a2 = next.Azimuth.Value;
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
            double si12 = System.Math.Sin((i2 - i1) / 2.0);
            double sa12 = System.Math.Sin((a2 - a1) / 2.0);
            double dl = 2.0 * System.Math.Asin(System.Math.Sqrt(si12 * si12 + si1 * si2 * sa12 * sa12));
            double rf;
            if (Numeric.EQ(dl, 0, 0.02))
            {
                double dl2 = dl * dl;
                rf = 1 + (dl2 / 12.0) * (1.0 + (dl2 / 10.0) * (1 + (dl2 / 168.0) * (1.0 + 31 * dl2 / 18.0)));
            }
            else
            {
                rf = (2.0 / dl) * System.Math.Tan(dl / 2.0);
            }
            next.X = x1 + 0.5 * dm * rf * (si1 * ca1 + si2 * ca2);
            next.Y = y1 + 0.5 * dm * rf * (si1 * sa1 + si2 * sa2);
            next.Z = z1 + 0.5 * dm * rf * (ci1 + ci2);
            return true;
        }
        /// <summary>
        /// This method calculates next using the minimum curvature method but also calculates the curvature, toolface, build-up rate and turn-rate
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteSIA(Survey next)
        {
            if (next == null || X == null || Y == null || Z == null || Inclination == null || Azimuth == null || Abscissa == null || next.Abscissa == null || next.Inclination == null || next.Azimuth == null)
            {
                return false;
            }
            double x1 = X.Value;
            double y1 = Y.Value;
            double z1 = Z.Value;
            double i1 = Inclination.Value;
            double a1 = Azimuth.Value;
            double s1 = Abscissa.Value;
            double s2 = next.Abscissa.Value;
            double i2 = next.Inclination.Value;
            double a2 = next.Azimuth.Value;
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
            double si12 = System.Math.Sin((i2 - i1)/2.0);
            double sa12 = System.Math.Sin((a2 - a1)/2.0);
            double dl = 2.0 * System.Math.Asin(System.Math.Sqrt(si12 * si12 + si1 * si2 * sa12 * sa12));
            double rf;
            if (Numeric.EQ(dl, 0, 0.02))
            {
                double dl2 = dl * dl;
                rf = 1 + (dl2 / 12.0) * (1.0 + (dl2 / 10.0) * (1 + (dl2 / 168.0) * (1.0 + 31 * dl2 / 18.0)));
            }
            else
            {
                rf = (2.0 / dl) * System.Math.Tan(dl / 2.0);
            }
            next.X = x1 + 0.5 * dm * rf * (si1 * ca1 + si2 * ca2);
            next.Y = y1 + 0.5 * dm * rf * (si1 * sa1 + si2 * sa2);
            next.Z = z1 + 0.5 * dm * rf * (ci1 + ci2);
            next.Curvature = dl / dm;
            CurvilinearPoint3D prev = new CurvilinearPoint3D();
            double ds = 0.1;
            if (InterpolateAtAbscissa(next, next.Abscissa.Value-ds, prev)) 
            {
                if (prev.Inclination != null && prev.Azimuth != null) 
                {
                    si1 = System.Math.Sin(prev.Inclination.Value);
                    ci1 = System.Math.Cos(prev.Inclination.Value);
                    si2 = System.Math.Sin(next.Inclination.Value);
                    ci2 = System.Math.Cos(next.Inclination.Value);
                    sa12 = System.Math.Sin(next.Azimuth.Value-prev.Azimuth.Value);
                    double ca12 = System.Math.Cos(next.Azimuth.Value - prev.Azimuth.Value);
                    double denom = si2 * ci1 * ca12 - si1 * ci2;
                    next.Toolface = System.Math.Atan2(denom, si2 * sa12);
                    
                    next.BUR = (next.Inclination - prev.Inclination) / ds;
                    if (next.Azimuth != null &&
                        prev.Azimuth != null &&
                        Numeric.LE(System.Math.Abs(next.Azimuth.Value - prev.Azimuth.Value), Numeric.PI))
                    {
                        next.TUR = (next.Azimuth - prev.Azimuth) / ds;
                    }
                    else
                    {
                        if (Numeric.GE(next.Azimuth - prev.Azimuth, 0))
                        {
                            next.TUR = (next.Azimuth - prev.Azimuth - 2.0 * Numeric.PI) / ds;
                        }
                        else
                        {
                            next.TUR = (next.Azimuth - prev.Azimuth + 2.0 * Numeric.PI) / ds;
                        }
                    }
                }
            }
            return true;
        }

        public bool CompleteXYZ(CurvilinearPoint3D next)
        {
            if (X != null &&
                Y != null &&
                Z != null &&
                next.X != null &&
                next.Y != null &&
                next.Z != null &&
                Inclination != null &&
                Azimuth != null)
            {
                double ci = System.Math.Cos(Inclination.Value);
                double si = System.Math.Sin(Inclination.Value);
                double ca = System.Math.Cos(Azimuth.Value);
                double sa = System.Math.Sin(Azimuth.Value);
                Vector3D v1 = Vector3D.CreateSpheric(1.0, Inclination, Azimuth);
                Vector3D v2 = new Vector3D(this, next);
                if (Numeric.EQ(Distance(next), 0.0) || v1.IsParallel(v2, 1e-4))
                {
                    next.Inclination = Inclination;
                    next.Azimuth = Azimuth;
                    next.Abscissa = Abscissa + Distance(next);
                    return true;
                }
                else
                {
                    double dls = 0;
                    double a;
                    Point3D p1 = TransCoord2PtsTg(next, next);
                    if (p1.Z != null && p1.Y != null && !Numeric.EQ(p1.Y.Value * p1.Y.Value + p1.Z.Value * p1.Z.Value, 0))
                    {
                        dls = 2.0 * p1.Y.Value / (p1.Y.Value * p1.Y.Value + p1.Z.Value * p1.Z.Value);
                        a = 2.0 * System.Math.Atan2(System.Math.Abs(p1.Y.Value), System.Math.Abs(p1.Z.Value));
                        if (Numeric.LE(p1.Z, 0))
                        {
                            a = 2.0 * Numeric.PI - (a - Numeric.PI * System.Math.Floor(a / Numeric.PI));
                        }
                        else
                        {
                            a -= Numeric.PI * System.Math.Floor(a / Numeric.PI);
                        }

                        Point3D p3 = new Point3D
                        {
                            X = 0.0,
                            Y = System.Math.Sin(a),
                            Z = System.Math.Cos(a)
                        };
                        Point3D? pt2 = TransCoord2PtsTgReversed(next, p3);
                        if (pt2 != null && pt2.X != null && pt2.Y != null && pt2.Z != null)
                        {
                            double c1 = pt2.Z.Value - Z.Value;
                            double b1 = pt2.Y.Value - Y.Value;
                            double a1 = pt2.X.Value - X.Value;
                            double? l;
                            if (Numeric.EQ(dls, 0))
                            {
                                l = Distance(next);
                            }
                            else
                            {
                                l = a / dls;
                            }
                            next.Abscissa = Abscissa + l;
                            next.Inclination = Numeric.AcosEqual(c1);
                            if (Numeric.EQ(a1, 0))
                            {
                                if (Numeric.GE(b1, 0))
                                {
                                    next.Azimuth = Numeric.PI / 2.0;
                                }
                                else
                                {
                                    next.Azimuth = 3.0 * Numeric.PI / 2.0;
                                }
                            }
                            else
                            {
                                next.Azimuth = System.Math.Atan(b1 / a1);
                            }
                            if (Numeric.LT(a1, 0))
                            {
                                next.Azimuth = next.Azimuth + Numeric.PI;
                            }
                            if (next.Azimuth < 0)
                            {
                                next.Azimuth = next.Azimuth + 2.0 * Numeric.PI;
                            }
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool CompleteXYZ(Survey next)
        {
            if (X != null &&
                Y != null &&
                Z != null &&
                next.X != null &&
                next.Y != null &&
                next.Z != null &&
                Inclination != null &&
                Azimuth != null)
            {
                double ci = System.Math.Cos(Inclination.Value);
                double si = System.Math.Sin(Inclination.Value);
                double ca = System.Math.Cos(Azimuth.Value);
                double sa = System.Math.Sin(Azimuth.Value);
                Vector3D v1 = Vector3D.CreateSpheric(1.0, Inclination, Azimuth);
                Vector3D v2 = new Vector3D(this, next);
                if (Numeric.EQ(Distance(next), 0.0) || v1.IsParallel(v2, 1e-4))
                {
                    next.Curvature = 0.0;
                    next.BUR = 0.0;
                    next.TUR = 0.0;
                    next.Toolface = 0.0;
                    next.Inclination = Inclination;
                    next.Azimuth = Azimuth;
                    next.Abscissa = Abscissa + Distance(next);
                    return true;
                }
                else
                {
                    double dls = 0;
                    double a;
                    Point3D p1 = TransCoord2PtsTg(next, next);
                    if (p1.Z != null && p1.Y != null && !Numeric.EQ(p1.Y.Value * p1.Y.Value + p1.Z.Value * p1.Z.Value, 0))
                    {
                        dls = 2.0 * p1.Y.Value / (p1.Y.Value * p1.Y.Value + p1.Z.Value * p1.Z.Value);
                        a = 2.0 * System.Math.Atan2(System.Math.Abs(p1.Y.Value), System.Math.Abs(p1.Z.Value));
                        if (Numeric.LE(p1.Z, 0))
                        {
                            a = 2.0 * Numeric.PI - (a - Numeric.PI * System.Math.Floor(a / Numeric.PI));
                        }
                        else
                        {
                            a -= Numeric.PI * System.Math.Floor(a / Numeric.PI);
                        }

                        Point3D p3 = new Point3D
                        {
                            X = 0.0,
                            Y = System.Math.Sin(a),
                            Z = System.Math.Cos(a)
                        };
                        Point3D? pt2 = TransCoord2PtsTgReversed(next, p3);
                        if (pt2 != null && pt2.X != null && pt2.Y != null && pt2.Z != null)
                        {
                            double c1 = pt2.Z.Value - Z.Value;
                            double b1 = pt2.Y.Value - Y.Value;
                            double a1 = pt2.X.Value - X.Value;
                            double? l;
                            if (Numeric.EQ(dls, 0))
                            {
                                l = Distance(next);
                            }
                            else
                            {
                                l = a / dls;
                            }
                            next.Abscissa = Abscissa + l;
                            next.Inclination = Numeric.AcosEqual(c1);
                            if (Numeric.EQ(a1, 0))
                            {
                                if (Numeric.GE(b1, 0))
                                {
                                    next.Azimuth = Numeric.PI / 2.0;
                                }
                                else
                                {
                                    next.Azimuth = 3.0 * Numeric.PI / 2.0;
                                }
                            }
                            else
                            {
                                next.Azimuth = System.Math.Atan(b1 / a1);
                            }
                            if (Numeric.LT(a1, 0))
                            {
                                next.Azimuth = next.Azimuth + Numeric.PI;
                            }
                            if (next.Azimuth < 0)
                            {
                                next.Azimuth = next.Azimuth + 2.0 * Numeric.PI;
                            }
                            next.Curvature = dls;
                            CurvilinearPoint3D prev = new CurvilinearPoint3D();
                            double ds = 0.1;
                            if (InterpolateAtAbscissa(next, next.Abscissa.Value - ds, prev))
                            {
                                if (prev.Inclination != null && prev.Azimuth != null)
                                {
                                    double si1 = System.Math.Sin(prev.Inclination.Value);
                                    double ci1 = System.Math.Cos(prev.Inclination.Value);
                                    double si2 = System.Math.Sin(next.Inclination.Value);
                                    double ci2 = System.Math.Cos(next.Inclination.Value);
                                    double sa12 = System.Math.Sin(next.Azimuth.Value - prev.Azimuth.Value);
                                    double ca12 = System.Math.Cos(next.Azimuth.Value - prev.Azimuth.Value);
                                    double denom = si2 * ci1 * ca12 - si1 * ci2;
                                    next.Toolface = System.Math.Atan2(denom, si2 * sa12);

                                    next.BUR = (next.Inclination - prev.Inclination) / ds;
                                    if (next.Azimuth != null &&
                                        prev.Azimuth != null &&
                                        Numeric.LE(System.Math.Abs(next.Azimuth.Value - prev.Azimuth.Value), Numeric.PI))
                                    {
                                        next.TUR = (next.Azimuth - prev.Azimuth) / ds;
                                    }
                                    else
                                    {
                                        if (Numeric.GE(next.Azimuth - prev.Azimuth, 0))
                                        {
                                            next.TUR = (next.Azimuth - prev.Azimuth - 2.0 * Numeric.PI) / ds;
                                        }
                                        else
                                        {
                                            next.TUR = (next.Azimuth - prev.Azimuth + 2.0 * Numeric.PI) / ds;
                                        }
                                    }
                                }
                            }
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Interpolate the result in between this survey and the next at a given curvilinear abscissa
        /// </summary>
        /// <param name="next"></param>
        /// <param name="s"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool InterpolateAtAbscissa(ICurvilinear3D next, double s, ICurvilinear3D result)
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
                result.Azimuth = Azimuth;
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
            double si1 = System.Math.Sin(i1);
            double si2 = System.Math.Sin(i2);
            double ci12 = System.Math.Cos(i2 - i1);
            double ca12 = System.Math.Cos(a2 - a1);
            double DL = System.Math.Acos(ci12 - (1 - ca12) * si2 * si1);
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
                double ci1 = System.Math.Cos(i1);
                double ca1 = System.Math.Cos(a1);
                double sa1 = System.Math.Sin(a1);
                double ci2 = System.Math.Cos(i2);
                double numerator = si2 * System.Math.Sin(a2 - a1);
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
                        result.Y = y1 + System.Math.Sin(az) * (1 - ci) / dls;
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
                        double xt = deltaXt * (ctf * ca1 * ci1 - sa1 * stf) + deltaZt * si1 * ca1;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="s"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool InterpolateAtAbscissa(ICurvilinear3D next, double s, Survey result)
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
                result.Azimuth = Azimuth;
                result.Curvature = Curvature;
                result.Toolface = Toolface;
                result.BUR = BUR;
                result.TUR = TUR;
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
            double si1 = System.Math.Sin(i1);
            double si2 = System.Math.Sin(i2);
            double ci12 = System.Math.Cos(i2 - i1);
            double ca12 = System.Math.Cos(a2 - a1);
            double DL = System.Math.Acos(ci12 - (1 - ca12) * si2 * si1);
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
                    result.Curvature = Curvature;
                    result.Toolface = Toolface;
                    result.BUR = BUR;
                    result.TUR = TUR;
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
                double ci1 = System.Math.Cos(i1);
                double ca1 = System.Math.Cos(a1);
                double sa1 = System.Math.Sin(a1);
                double ci2 = System.Math.Cos(i2);
                double numerator = si2 * System.Math.Sin(a2 - a1);
                double denominator = si2 * ci1 * System.Math.Cos(a2 - a1) - si1 * ci2;
                double tf = System.Math.Atan2(numerator, denominator);
                double dls = DL / DM;
                double dm = s - s1;
                result.Curvature = dls;
                if (Numeric.EQ(dls, 0))
                {
                    result.Inclination = i1;
                    result.Azimuth = a1;
                    result.X = x1 + dm * ca1 * si1;
                    result.Y = y1 + dm * sa1 * si1;
                    result.Z = z1 + dm * ci1;
                    result.Toolface = Toolface;
                    result.BUR = 0;
                    result.TUR = 0;
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
                        result.Y = y1 + System.Math.Sin(az) * (1 - ci) / dls;
                        result.Z = z1 + System.Math.Sin(incl) / dls;
                        result.Toolface = az;
                        result.BUR = dls;
                        result.TUR = 0;
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
                        double xt = deltaXt * (ctf * ca1 * ci1 - sa1 * stf) + deltaZt * si1 * ca1;
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
                        CurvilinearPoint3D prev = new CurvilinearPoint3D();
                        double ds = 0.1;
                        if (InterpolateAtAbscissa(next, s - ds, prev))
                        {
                            if (prev.Inclination != null && prev.Azimuth != null)
                            {
                                si1 = System.Math.Sin(prev.Inclination.Value);
                                ci1 = System.Math.Cos(prev.Inclination.Value);
                                si2 = System.Math.Sin(result.Inclination.Value);
                                ci2 = System.Math.Cos(result.Inclination.Value);
                                double sa12 = System.Math.Sin(result.Azimuth.Value - prev.Azimuth.Value);
                                ca12 = System.Math.Cos(result.Azimuth.Value - prev.Azimuth.Value);
                                double denom = si2 * ci1 * ca12 - si1 * ci2;
                                result.Toolface = System.Math.Atan2(denom, si2 * sa12);

                                result.BUR = (result.Inclination - prev.Inclination) / ds;
                                if (result.Azimuth != null &&
                                    prev.Azimuth != null &&
                                    Numeric.LE(System.Math.Abs(result.Azimuth.Value - prev.Azimuth.Value), Numeric.PI))
                                {
                                    result.TUR = (result.Azimuth - prev.Azimuth) / ds;
                                }
                                else
                                {
                                    if (Numeric.GE(result.Azimuth - prev.Azimuth, 0))
                                    {
                                        result.TUR = (result.Azimuth - prev.Azimuth - 2.0 * Numeric.PI) / ds;
                                    }
                                    else
                                    {
                                        result.TUR = (result.Azimuth - prev.Azimuth + 2.0 * Numeric.PI) / ds;
                                    }
                                }
                            }
                        }
                        return true;
                    }
                }
            }
        }
        /// <summary>
        /// Apply the minimum curvature method to a list of surveys.
        /// The first survey must be complete.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        internal static bool Calculate<A>(List<A> list) where A : Survey
        {
            if (list != null &&
                list.Count > 0 &&
                list[0].X != null &&
                list[0].Y != null &&
                list[0].Z != null &&
                list[0].Abscissa != null &&
                list[0].Inclination != null &&
                list[0].Azimuth != null)
            {
                A sv1 = list[0];
                bool ok = true;
                for (int i = 1; i < list.Count; i++)
                {
                    var sv2 = list[i];
                    if (sv2 != null && sv2.Abscissa != null && sv2.Inclination != null && sv2.Azimuth != null)
                    {
                        ok = sv1.CompleteSIA(sv2);
                        sv1 = sv2;
                        if (!ok) break;
                    }
                }
                return ok;
            }
            else
            {
                return false;
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
                SetRiemannianNorthEast((double)value, (double)longitude_);
            }
        }
        private void UpdateLongitude(double? value)
        {
            if (value != null && latitude_ != null)
            {
                SetRiemannianNorthEast((double)latitude_, (double)value);
            }
        }

        private Point3D? TransCoord2PtsTgReversed(Point3D pf, Point3D p1)
        {
            if (pf != null && p1 != null && Azimuth != null && Inclination != null && pf.Y != null && Y != null && pf.Z != null && Z != null && pf.X != null && X != null)
            {
                double ca = System.Math.Cos(Azimuth.Value);
                double sa = System.Math.Sin(Azimuth.Value);
                double ci = System.Math.Cos(Inclination.Value);
                double si = System.Math.Sin(Inclination.Value);
                double xk = si * ca;
                double yk = si * sa;
                double zk = ci;
                double xi1 = (double)(ci * (pf.Y - Y) - si * sa * (pf.Z - Z));
                double yi1 = (double)(si * ca * (pf.Z - Z) - ci * (pf.X - X));
                double zi1 = (double)(si * sa * (pf.X - X) - si * ca * (pf.Y - Y));
                double sqrti = System.Math.Sqrt(xi1 * xi1 + yi1 * yi1 + zi1 * zi1);
                if (!Numeric.EQ(sqrti, 0))
                {
                    double xi = xi1 / sqrti;
                    double yi = yi1 / sqrti;
                    double zi = zi1 / sqrti;
                    double xj1 = yi1 * ci - zi1 * si * sa;
                    double yj1 = zi1 * si * ca - xi1 * ci;
                    double zj1 = xi1 * si * sa - yi1 * si * ca;
                    double sqrtj = -System.Math.Sqrt(xj1 * xj1 + yj1 * yj1 + zj1 * zj1);
                    if (!Numeric.EQ(sqrtj, 0))
                    {
                        double xj = xj1 / sqrtj;
                        double yj = yj1 / sqrtj;
                        double zj = zj1 / sqrtj;
                        Point3D p2 = new()
                        {
                            X = xi * p1.X + xj * p1.Y + xk * p1.Z + X,
                            Y = yi * p1.X + yj * p1.Y + yk * p1.Z + Y,
                            Z = zi * p1.X + zj * p1.Y + zk * p1.Z + Z
                        };
                        return p2;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }
    }
}
