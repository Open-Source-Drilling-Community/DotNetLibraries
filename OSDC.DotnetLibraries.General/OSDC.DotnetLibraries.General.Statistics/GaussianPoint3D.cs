using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Statistics
{
    /// <summary>
    /// Uncertainty-aware version of <see cref="Point3D"/>.
    /// Mean and covariance are represented in Euclidean coordinates, e.g. North-East-Down (NED)
    /// </summary>
    public class GaussianPoint3D : IGaussianPoint3D
    {
        /// <summary>
        /// Mean position in Euclidean coordinates.
        /// </summary>
        public virtual Vector3D? Mean { get; set; } = null;
        /// <summary>
        /// 3x3 covariance matrix expressed in Euclidean coordinates.
        /// </summary>
        public virtual Matrix3x3? Covariance { get; set; } = null;

        /// <summary>
        /// simplified property to access directly mean X (e.g. N in NED)
        /// </summary>
        public virtual double? X
        {
            get
            {
                return Mean?.X;
            }
            set
            {
                Mean ??= new Vector3D();
                Mean.X = value;
            }
        }

        /// <summary>
        /// simplified property to access directly mean Y (e.g. E in NED)
        /// </summary>
        public virtual double? Y
        {
            get
            {
                return Mean?.Y;
            }
            set
            {
                Mean ??= new Vector3D();
                Mean.Y = value;
            }
        }

        /// <summary>
        /// simplified property to access directly mean Z (e.g. D in NED)
        /// </summary>
        public virtual double? Z
        {
            get
            {
                return Mean?.Z;
            }
            set
            {
                Mean ??= new Vector3D();
                Mean.Z = value;
            }
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public GaussianPoint3D()
        {
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="p"></param>
        public GaussianPoint3D(GaussianPoint3D p)
        {
            if (p != null)
            {
                Mean = DeepCopy(p.Mean);
                Covariance = DeepCopy(p.Covariance);
            }
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="pt"></param>
        public GaussianPoint3D(IGaussianPoint3D pt)
        {
            if (pt != null)
            {
                Mean = DeepCopy(pt.Mean);
                Covariance = DeepCopy(pt.Covariance);
            }
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="covariance"></param>
        public GaussianPoint3D(Vector3D mean, Matrix3x3 covariance)
        {
            Mean = DeepCopy(mean);
            Covariance = DeepCopy(covariance);
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="covariance"></param>
        public GaussianPoint3D(double x, double y, double z, Matrix3x3 covariance)
        {
            Mean = new Vector3D(x, y, z);
            Covariance = DeepCopy(covariance);
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="covariance"></param>
        public GaussianPoint3D(double? x, double? y, double? z, Matrix3x3 covariance)
        {
            Mean = new Vector3D(x, y, z);
            Covariance = DeepCopy(covariance);
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="cov00"></param>
        /// <param name="cov01"></param>
        /// <param name="cov02"></param>
        /// <param name="cov10"></param>
        /// <param name="cov11"></param>
        /// <param name="cov12"></param>
        /// <param name="cov20"></param>
        /// <param name="cov21"></param>
        /// <param name="cov22"></param>
        public GaussianPoint3D(double? x, double? y, double? z,
                               double? cov00, double? cov01, double? cov02,
                               double? cov10, double? cov11, double? cov12,
                               double? cov20, double? cov21, double? cov22)
        {
            Mean = new Vector3D(x, y, z);
            Covariance = new Matrix3x3(cov00, cov01, cov02, cov10, cov11, cov12, cov20, cov21, cov22);
        }

        /// <summary>
        /// constructor with initialization from a Point3D mean
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="covariance"></param>
        public GaussianPoint3D(IPoint3D pt, Matrix3x3 covariance)
        {
            if (pt != null)
            {
                Mean = new Vector3D(pt.X, pt.Y, pt.Z);
            }
            Covariance = DeepCopy(covariance);
        }

        /// <summary>
        /// constructor with initialization from an array
        /// </summary>
        /// <param name="dat"></param>
        /// <param name="covariance"></param>
        public GaussianPoint3D(double[] dat, Matrix3x3 covariance)
        {
            if (dat != null && dat.Length >= 3)
            {
                Mean = new Vector3D(dat[0], dat[1], dat[2]);
            }
            Covariance = DeepCopy(covariance);
        }

        /// <summary>
        /// cloning
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return new GaussianPoint3D(this);
        }

        /// <summary>
        /// set the mean and covariance to zero
        /// </summary>
        public virtual void SetZero()
        {
            Mean ??= new Vector3D();
            Mean.X = 0.0;
            Mean.Y = 0.0;
            Mean.Z = 0.0;
            Covariance = new Matrix3x3(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
        }

        /// <summary>
        /// this Gaussian point is zero if its mean is zero and its covariance is zero
        /// </summary>
        /// <returns></returns>
        public virtual bool IsZero()
        {
            return Numeric.EQ(X, 0.0) &&
                   Numeric.EQ(Y, 0.0) &&
                   Numeric.EQ(Z, 0.0) &&
                   IsZero(Covariance);
        }

        /// <summary>
        /// true if mean exists and all covariance elements are defined
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            return IsValidMean() && IsValidCovariance();
        }

        /// <summary>
        /// true if mean exists and its coordinates are defined
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValidMean()
        {
            return Mean != null &&
                   Mean.X != null &&
                   Mean.Y != null &&
                   Mean.Z != null;
        }

        /// <summary>
        /// true if covariance exists and all its coefficients are defined
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValidCovariance()
        {
            return Covariance != null &&
                   Covariance[0, 0] != null &&
                   Covariance[0, 1] != null &&
                   Covariance[0, 2] != null &&
                   Covariance[1, 0] != null &&
                   Covariance[1, 1] != null &&
                   Covariance[1, 2] != null &&
                   Covariance[2, 0] != null &&
                   Covariance[2, 1] != null &&
                   Covariance[2, 2] != null;
        }

        /// <summary>
        /// true if coordinates are independent, which is the case if the covariance matrix is diagonal
        /// </summary>
        /// <returns></returns>
        public virtual bool HasIndependentCoordinates()
        {
            return IsValid() &&
                Covariance![0, 1]! == 0.0 && Covariance![1, 0]! == 0.0 &&
                Covariance![1, 2]! == 0.0 && Covariance![2, 1]! == 0.0 &&
                Covariance![0, 2]! == 0.0 && Covariance![2, 0]! == 0.0;
        }

        /// <summary>
        /// set based on a reference point
        /// </summary>
        /// <param name="point"></param>
        public virtual void Set(GaussianPoint3D point)
        {
            if (point != null)
            {
                Mean = DeepCopy(point.Mean);
                Covariance = DeepCopy(point.Covariance);
            }
        }

        /// <summary>
        /// set based on a reference point
        /// </summary>
        /// <param name="point"></param>
        public virtual void Set(IGaussianPoint3D point)
        {
            if (point != null)
            {
                Mean = DeepCopy(point.Mean);
                Covariance = DeepCopy(point.Covariance);
            }
        }

        /// <summary>
        /// set the mean based on a deterministic point
        /// </summary>
        /// <param name="point"></param>
        public virtual void SetMean(Point3D point)
        {
            if (point != null)
            {
                Mean ??= new Vector3D();
                Mean.X = point.X;
                Mean.Y = point.Y;
                Mean.Z = point.Z;
            }
        }

        /// <summary>
        /// set the mean based on a deterministic point
        /// </summary>
        /// <param name="point"></param>
        public virtual void SetMean(IPoint3D point)
        {
            if (point != null)
            {
                Mean ??= new Vector3D();
                Mean.X = point.X;
                Mean.Y = point.Y;
                Mean.Z = point.Z;
            }
        }

        /// <summary>
        /// set the mean coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public virtual void SetMean(double x, double y, double z)
        {
            Mean ??= new Vector3D();
            Mean.X = x;
            Mean.Y = y;
            Mean.Z = z;
        }

        /// <summary>
        /// set the mean coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public virtual void SetMean(double? x, double? y, double? z)
        {
            Mean ??= new Vector3D();
            Mean.X = x;
            Mean.Y = y;
            Mean.Z = z;
        }

        /// <summary>
        /// set the covariance
        /// </summary>
        /// <param name="covariance"></param>
        public virtual void SetCovariance(Matrix3x3 covariance)
        {
            Covariance = DeepCopy(covariance);
        }

        /// <summary>
        /// Gaussian distribution realization strategy in the general case where coordinates are dependent on eachother
        /// 1- decomposes the covariance matrix into the product of a triangle matrix with its transpose L*Lt
        /// 2- draw 3 independent values following unit normal distribution
        /// 3- returns a sample as mu + L*z
        /// </summary>
        /// <returns></returns>
        public Point3D? Realize()
        {
            if (!IsValid())
                return null;

            var mu = Vector<double>.Build.Dense(new[]
            {
                Mean!.X!.Value,
                Mean!.Y!.Value,
                Mean!.Z!.Value
            });

            var sigma = Matrix<double>.Build.DenseOfArray(new[,]
            {
                { Covariance![0, 0]!.Value, Covariance[0, 1]!.Value, Covariance[0, 2]!.Value },
                { Covariance![1, 0]!.Value, Covariance[1, 1]!.Value, Covariance[1, 2]!.Value },
                { Covariance![2, 0]!.Value, Covariance[2, 1]!.Value, Covariance[2, 2]!.Value }
            });

            MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double>? chol;
            try
            {
                chol = sigma.Cholesky();

                var z = Vector<double>.Build.Dense(new[]
                {
                new Normal(0, 1).Sample(),
                new Normal(0, 1).Sample(),
                new Normal(0, 1).Sample()
                });

                var sample = mu + chol.Factor * z;

                return new Point3D(sample[0], sample[1], sample[2]);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gaussian distribution realization strategy in the case where coordinates are indepent
        /// </summary>
        /// <returns></returns>
        public Point3D? RealizeIndependent()
        {
            if (!IsValid() || !HasIndependentCoordinates())
                return null;

            var gx = new GaussianDistribution(Mean!.X, System.Math.Sqrt(Covariance![0, 0]!.Value));
            var gy = new GaussianDistribution(Mean!.Y, System.Math.Sqrt(Covariance![1, 1]!.Value));
            var gz = new GaussianDistribution(Mean!.Z, System.Math.Sqrt(Covariance![2, 2]!.Value));

            return new Point3D(
                gx.Realize(),
                gy.Realize(),
                gz.Realize());
        }




        /// <summary>
        /// return the mean as a deterministic point
        /// </summary>
        /// <returns></returns>
        public virtual Point3D? GetMeanPoint()
        {
            if (!IsValidMean())
            {
                return null;
            }
            return new Point3D(X, Y, Z);
        }

        /// <summary>
        /// return the polar radius of the mean point
        /// </summary>
        /// <returns></returns>
        public virtual double? GetPlaneRadius()
        {
            if (X == null || Y == null)
            {
                return null;
            }
            double x = (double)X;
            double y = (double)Y;
            return System.Math.Sqrt(y * y + x * x);
        }

        /// <summary>
        /// return the polar direction of the mean point
        /// </summary>
        /// <returns></returns>
        public virtual double? GetAz()
        {
            if (X == null || Y == null)
            {
                return null;
            }
            double x = (double)X;
            double y = (double)Y;
            if (Numeric.EQ(x, 0.0) && Numeric.EQ(y, 0.0))
            {
                return null;
            }
            return System.Math.Atan2(y, x);
        }

        /// <summary>
        /// return the spherical radius of the mean point
        /// </summary>
        /// <returns></returns>
        public virtual double? GetRadius()
        {
            if (X == null || Y == null || Z == null)
            {
                return null;
            }
            double x = (double)X;
            double y = (double)Y;
            double z = (double)Z;
            return System.Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// return the inclination of the mean point
        /// </summary>
        /// <returns></returns>
        public virtual double? GetInclination()
        {
            if (X == null || Y == null || Z == null)
            {
                return null;
            }
            double? length = GetRadius();
            if (length == null)
            {
                return null;
            }
            double l = (double)length;
            double z = (double)Z;
            return System.Math.Acos(z / l);
        }

        /// <summary>
        /// return the euclidian distance between the mean and another deterministic point
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual double? GetDistance(IPoint3D other)
        {
            if (other == null || X == null || Y == null || Z == null || other.X == null || other.Y == null || other.Z == null)
            {
                return null;
            }
            double x1 = (double)X;
            double y1 = (double)Y;
            double z1 = (double)Z;
            double x2 = (double)other.X;
            double y2 = (double)other.Y;
            double z2 = (double)other.Z;
            double dx = x2 - x1;
            double dy = y2 - y1;
            double dz = z2 - z1;
            return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// return the euclidian distance between the means
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual double? GetDistance(IGaussianPoint3D other)
        {
            if (other?.GetMeanPoint() is { } mp)
            {
                return GetDistance(mp);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// return the horizontal distance between the mean and a deterministic point
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual double GetHorizontalDistance(IPoint3D p)
        {
            if (p == null || X == null || Y == null || p.X == null || p.Y == null)
            {
                return 0.0;
            }
            return System.Math.Sqrt((double)((p.X - X) * (p.X - X) + (p.Y - Y) * (p.Y - Y)));
        }

        /// <summary>
        /// return the horizontal distance between the means
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual double GetHorizontalDistance(IGaussianPoint3D p)
        {
            if (p?.GetMeanPoint() is { } mp)
            {
                return GetHorizontalDistance(mp);
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// return the inclination between the mean and a deterministic point
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual double GetIncl(IPoint3D p)
        {
            if (IsValidMean() &&
                p?.X is { } && p?.Y is { } && p?.Z is { } pz)
            {
                double? distance = GetDistance(p);
                if (distance == null || Numeric.EQ(distance, 0.0))
                {
                    return 0.0;
                }
                return Numeric.AcosEqual((double)(pz - Z!) / distance.Value);
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// return the inclination between the means
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual double GetIncl(IGaussianPoint3D p)
        {
            if (p?.GetMeanPoint() is { } mp)
            {
                return GetIncl(mp);
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// return the azimuth between the mean and a deterministic point
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual double GetAz(IPoint3D p)
        {
            if (IsValidMean() && p?.X is { } px && p?.Y is { } py)
            {
                double length = GetHorizontalDistance(p);
                if (Numeric.EQ(length, 0.0))
                {
                    return 0.0;
                }
                else
                {
                    if ((Y! - py) >= 0.0)
                    {
                        return Numeric.AcosEqual((double)(X! - px) / length);
                    }
                    else
                    {
                        return 0.5 * Numeric.PI - Numeric.AcosEqual((double)(X! - px) / length);
                    }
                }
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// return the azimuth between the means
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual double GetAz(IGaussianPoint3D p)
        {
            if (p?.GetMeanPoint() is { } mp)
            {
                return GetAz(mp);
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// force that this mean is at the given coordinates
        /// covariance is unchanged
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public virtual void MoveTo(double x, double y, double z)
        {
            Mean ??= new Vector3D();
            Mean.X = x;
            Mean.Y = y;
            Mean.Z = z;
        }

        /// <summary>
        /// move mean to the same coordinates as pt if it is not null
        /// covariance is unchanged
        /// </summary>
        /// <param name="pt"></param>
        public virtual void MoveTo(IPoint3D pt)
        {
            if (pt != null)
            {
                Mean ??= new Vector3D();
                Mean.X = pt.X;
                Mean.Y = pt.Y;
                Mean.Z = pt.Z;
            }
        }

        /// <summary>
        /// move mean to the same coordinates as p if it is not null
        /// covariance is unchanged
        /// </summary>
        /// <param name="p"></param>
        public virtual void MoveTo(IGaussianPoint3D p)
        {
            if (p?.GetMeanPoint() is { } mp)
            {
                MoveTo(mp);
            }
        }

        /// <summary>
        /// translate the mean coordinates by the given values
        /// covariance is unchanged
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public virtual void Translate(double x, double y, double z)
        {
            Mean ??= new Vector3D();
            Mean.X += x;
            Mean.Y += y;
            Mean.Z += z;
        }

        /// <summary>
        /// translate by the given vector if it is not null
        /// covariance is unchanged
        /// </summary>
        /// <param name="vec"></param>
        public virtual void Translate(IVector3D vec)
        {
            if (vec != null)
            {
                Mean ??= new Vector3D();
                Mean.X += vec.X;
                Mean.Y += vec.Y;
                Mean.Z += vec.Z;
            }
        }

        /// <summary>
        /// return the middle point between this mean and p
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual Point3D? GetMiddle(IPoint3D p)
        {
            if (p != null)
            {
                return new Point3D((X + p.X) / 2.0, (Y + p.Y) / 2.0, (Z + p.Z) / 2.0);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// return the middle point between the means
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual Point3D? GetMiddle(IGaussianPoint3D p)
        {
            if (p?.GetMeanPoint() is { } mp)
            {
                return GetMiddle(mp);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// return a point at a relative distance between this mean and point p2
        /// </summary>
        /// <param name="p2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public virtual Point3D? GetPoint(IPoint3D p2, double distance)
        {
            if (p2 != null)
            {
                return new Point3D(X + (p2.X - X) * distance, Y + (p2.Y - Y) * distance, Z + (p2.Z - Z) * distance);
            }
            else
            {
                if (Numeric.EQ(distance, 0.0))
                {
                    return GetMeanPoint();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// return a point at a relative distance between 'this' mean and point p2
        /// </summary>
        /// <param name="p"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public virtual Point3D? GetPoint(IGaussianPoint3D p, double distance)
        {
            if (p?.GetMeanPoint() is { } mp)
            {
                return GetPoint(mp, distance);
            }
            else
            {
                if (Numeric.EQ(distance, 0.0))
                {
                    return GetMeanPoint();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// return this mean considered to be defined in the coordinate system defined by pt and incl and az into the global coordinate system
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="incl"></param>
        /// <param name="az"></param>
        /// <returns></returns>
        public virtual Point3D? ToGlobal(IPoint3D pt, double incl, double az)
        {
            if (pt != null)
            {
                double ca = System.Math.Cos(az);
                double sa = System.Math.Sin(az);
                double ci = System.Math.Cos(incl);
                double si = System.Math.Sin(incl);
                double? x = ci * pt.X + si * pt.Z;
                double? y = pt.Y;
                double? z = -si * pt.X + ci * pt.Z;
                return new Point3D(X + ca * x - sa * y, Y + sa * x + ca * y, Z + z);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// return this mean in the local coordinate system defined by pt and incl and az
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="incl"></param>
        /// <param name="az"></param>
        /// <returns></returns>
        public virtual Point3D? ToLocal(IPoint3D pt, double incl, double az)
        {
            if (pt != null)
            {
                double ca = System.Math.Cos(az);
                double sa = System.Math.Sin(az);
                double ci = System.Math.Cos(incl);
                double si = System.Math.Sin(incl);
                double? x = pt.X - X;
                double? y = pt.Y - Y;
                double? z = pt.Z - Z;
                double? XX = ca * x + sa * y;
                double? YY = -sa * x + ca * y;
                return new Point3D(ci * XX - si * z, YY, si * XX + ci * z);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// calculate the cross product vector between the vector P1P2 and P1P3 where P1 is this mean
        /// </summary>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public virtual Vector3D? CrossProductVector(IPoint3D p2, IPoint3D p3)
        {
            Point3D? p1 = GetMeanPoint();
            if (p1 != null && p2 != null && p3 != null)
            {
                Vector3D p1p2 = new Vector3D(p1, p2);
                Vector3D p1p3 = new Vector3D(p1, p3);
                return p1p2.CrossProduct(p1p3);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// check if this mean is colinear with p2 and p3
        /// </summary>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public virtual bool AreColinear(IPoint3D p2, IPoint3D p3)
        {
            if (p2 != null && p3 != null)
            {
                Vector3D? cross = CrossProductVector(p2, p3);
                return cross != null && Numeric.EQ(cross.GetLength(), 0.0);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// equality at numeric accuracy on the mean only
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public virtual bool EQ(IPoint3D cmp)
        {
            if (cmp == null)
            {
                return false;
            }
            return Numeric.EQ(X, cmp.X) &&
                   Numeric.EQ(Y, cmp.Y) &&
                   Numeric.EQ(Z, cmp.Z);
        }

        /// <summary>
        /// equality at given accuracy on the mean only
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public virtual bool EQ(IPoint3D cmp, double precision)
        {
            if (cmp == null)
            {
                return false;
            }
            return Numeric.EQ(X, cmp.X, precision) &&
                   Numeric.EQ(Y, cmp.Y, precision) &&
                   Numeric.EQ(Z, cmp.Z, precision);
        }

        protected static Vector3D? DeepCopy(Vector3D? src)
        {
            if (src == null)
            {
                return null;
            }
            return new Vector3D(src);
        }

        protected static Matrix3x3? DeepCopy(Matrix3x3? src)
        {
            if (src == null)
            {
                return null;
            }
            return new Matrix3x3(src);
        }

        protected static bool IsZero(Matrix3x3? covariance)
        {
            return covariance != null &&
                   Numeric.EQ(covariance[0, 0], 0.0) &&
                   Numeric.EQ(covariance[0, 1], 0.0) &&
                   Numeric.EQ(covariance[0, 2], 0.0) &&
                   Numeric.EQ(covariance[1, 0], 0.0) &&
                   Numeric.EQ(covariance[1, 1], 0.0) &&
                   Numeric.EQ(covariance[1, 2], 0.0) &&
                   Numeric.EQ(covariance[2, 0], 0.0) &&
                   Numeric.EQ(covariance[2, 1], 0.0) &&
                   Numeric.EQ(covariance[2, 2], 0.0);
        }
    }
}