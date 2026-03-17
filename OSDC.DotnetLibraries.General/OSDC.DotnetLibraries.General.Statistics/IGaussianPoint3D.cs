using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Statistics
{
    /// <summary>
    /// Interface for an uncertainty-aware point in Cartesian coordinates.
    /// Mean and covariance are represented in Cartesian coordinates, e.g. North-East-Down (NED)
    /// </summary>
    public interface IGaussianPoint3D : IEquivalent<IPoint3D>, ICloneable, IZeroeable
    {
        /// <summary>
        /// Mean position of the Gaussian point.
        /// </summary>
        public Vector3D? Mean { get; set; }

        /// <summary>
        /// 3x3 covariance matrix of the Gaussian point.
        /// </summary>
        public Matrix3x3? Covariance { get; set; }

        /// <summary>
        /// Simplified access to mean X.
        /// </summary>
        public double? X { get; set; }

        /// <summary>
        /// Simplified access to mean Y.
        /// </summary>
        public double? Y { get; set; }

        /// <summary>
        /// Simplified access to mean Z.
        /// </summary>
        public double? Z { get; set; }

        /// <summary>
        /// Gaussian distribution realization strategy in the general case where coordinates are dependent on eachother
        /// </summary>
        /// <returns></returns>
        public Point3D? Realize();

        /// <summary>
        /// Gaussian distribution realization strategy in the case where coordinates are indepent
        /// </summary>
        /// <returns></returns>
        public Point3D? RealizeIndependent();

        /// <summary>
        /// True if mean exists and all covariance elements are defined.
        /// </summary>
        /// <returns></returns>
        public bool IsValid();

        /// <summary>
        /// True if mean exists and its coordinates are defined.
        /// Covariance is not required.
        /// </summary>
        /// <returns></returns>
        public bool IsValidMean();

        /// <summary>
        /// Set from another Gaussian point.
        /// </summary>
        /// <param name="point"></param>
        public void Set(IGaussianPoint3D point);

        /// <summary>
        /// Set mean only.
        /// </summary>
        /// <param name="point"></param>
        public void SetMean(IPoint3D point);

        /// <summary>
        /// Set mean coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetMean(double x, double y, double z);

        /// <summary>
        /// Set mean coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetMean(double? x, double? y, double? z);

        /// <summary>
        /// Set covariance.
        /// </summary>
        /// <param name="covariance"></param>
        public void SetCovariance(Matrix3x3 covariance);

        /// <summary>
        /// Return the mean as a deterministic point.
        /// </summary>
        /// <returns></returns>
        public Point3D? GetMeanPoint();

        /// <summary>
        /// Return the plane radius computed from the mean.
        /// </summary>
        /// <returns></returns>
        public double? GetPlaneRadius();

        /// <summary>
        /// Return the azimuth computed from the mean.
        /// </summary>
        /// <returns></returns>
        public double? GetAz();

        /// <summary>
        /// Return the spherical radius computed from the mean.
        /// </summary>
        /// <returns></returns>
        public double? GetRadius();

        /// <summary>
        /// Return the inclination computed from the mean.
        /// </summary>
        /// <returns></returns>
        public double? GetInclination();

        /// <summary>
        /// Return the Euclidean distance between the means.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double? GetDistance(IPoint3D other);

        /// <summary>
        /// Return the Euclidean distance between the means.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double? GetDistance(IGaussianPoint3D other);

        /// <summary>
        /// Return the horizontal distance between the means.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double GetHorizontalDistance(IPoint3D p);

        /// <summary>
        /// Return the horizontal distance between the means.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double GetHorizontalDistance(IGaussianPoint3D p);

        /// <summary>
        /// Return the inclination between the means.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double GetIncl(IPoint3D p);

        /// <summary>
        /// Return the inclination between the means.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double GetIncl(IGaussianPoint3D p);

        /// <summary>
        /// Return the azimuth between the means.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double GetAz(IPoint3D p);

        /// <summary>
        /// Return the azimuth between the means.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double GetAz(IGaussianPoint3D p);

        /// <summary>
        /// Force the mean to the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void MoveTo(double x, double y, double z);

        /// <summary>
        /// Force the mean to the same coordinates as pt if it is not null.
        /// </summary>
        /// <param name="p"></param>
        public void MoveTo(IPoint3D p);

        /// <summary>
        /// Force the mean to the same coordinates as pt if it is not null.
        /// </summary>
        /// <param name="p"></param>
        public void MoveTo(IGaussianPoint3D p);

        /// <summary>
        /// Translate the mean coordinates by the given values.
        /// Covariance is unchanged.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Translate(double x, double y, double z);

        /// <summary>
        /// Translate the mean by the given vector.
        /// Covariance is unchanged.
        /// </summary>
        /// <param name="vec"></param>
        public void Translate(IVector3D vec);

        /// <summary>
        /// Return the middle point between the means.
        /// Covariance is not propagated here.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Point3D? GetMiddle(IPoint3D p);

        /// <summary>
        /// Return the middle point between the means.
        /// Covariance is not propagated here.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Point3D? GetMiddle(IGaussianPoint3D p);

        /// <summary>
        /// Return a point at a relative distance between this mean and point p.
        /// Covariance is not propagated here.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public Point3D? GetPoint(IPoint3D p, double distance);

        /// <summary>
        /// Return a point at a relative distance between this mean and point p.
        /// Covariance is not propagated here.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public Point3D? GetPoint(IGaussianPoint3D p, double distance);

        /// <summary>
        /// Return this mean considered to be defined in the coordinate system
        /// defined by p and incl and az into the global coordinate system.
        /// Covariance is not propagated here.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="incl"></param>
        /// <param name="az"></param>
        /// <returns></returns>
        public Point3D? ToGlobal(IPoint3D p, double incl, double az);

        /// <summary>
        /// Return this mean in the local coordinate system defined by p and incl and az.
        /// Covariance is not propagated here.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="incl"></param>
        /// <param name="az"></param>
        /// <returns></returns>
        public Point3D? ToLocal(IPoint3D p, double incl, double az);

        /// <summary>
        /// Calculate the cross product vector between the vector P0P1 and P0P2 where P0 is 'this' mean.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public Vector3D? CrossProductVector(IPoint3D p1, IPoint3D p2);

        /// <summary>
        /// Check if 'this' mean is colinear with p1 and p2.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool AreColinear(IPoint3D p1, IPoint3D p2);
    }
}