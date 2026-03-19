using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;
using static OSDC.DotnetLibraries.General.Math.GeodeticPoint3D;

namespace OSDC.DotnetLibraries.General.Statistics
{
    /// <summary>
    /// Uncertainty-aware version of <see cref="GeodeticPoint3D"/>.
    /// The mean is represented in geodetic WGS84 coordinates while the covariance is represented
    /// in a local North-East-Down (NED) tangent frame used as the linearization support to preserve
    /// the linearity of uncertainty propagation
    /// </summary>
    public class GaussianGeodeticPoint3D : IGaussianGeodeticPoint3D
    {
        /// <summary>
        /// Mean position in geodetic WGS84 coordinates.
        /// </summary>
        public virtual GeodeticPoint3D? GeodeticMean { get; set; } = null;
        /// <summary>
        /// 3x3 covariance matrix expressed in the local NED frame attached to <see cref="ReferencePoint"/>.
        /// </summary>
        public virtual Matrix3x3? CovarianceNED { get; set; } = null;

        /// <summary>
        /// Reference geodetic point defining:
        /// - the local NED tangent frame used for covariance linearization.
        /// - including both its origin, and its orientation (as per WGS84)
        /// If null, the mean point is used, but for the sake of comparing geodetic points to one another,
        /// it is highly preferable to set a fixed reference point which is not the mean of every geodetic point.
        /// It can be the wellhead or any point in the vicinity of the rig which allows for a valid linearized approximation
        /// of the covariance matrix.
        /// </summary>
        public virtual GeodeticPoint3D? ReferencePoint { get; set; } = null;

        public virtual double? LatitudeWGS84
        {
            get => GeodeticMean?.LatitudeWGS84;
            set
            {
                GeodeticMean ??= new GeodeticPoint3D();
                GeodeticMean.LatitudeWGS84 = value;
            }
        }

        public virtual double? LongitudeWGS84
        {
            get => GeodeticMean?.LongitudeWGS84;
            set
            {
                GeodeticMean ??= new GeodeticPoint3D();
                GeodeticMean.LongitudeWGS84 = value;
            }
        }

        /// <summary>
        /// True vertical depth relative to WGS84 ellipsoid. Positive downward.
        /// </summary>
        public virtual double? TvdWGS84
        {
            get => GeodeticMean?.TvdWGS84;
            set
            {
                GeodeticMean ??= new GeodeticPoint3D();
                GeodeticMean.TvdWGS84 = value;
            }
        }

        public GaussianGeodeticPoint3D()
        {
        }

        public GaussianGeodeticPoint3D(IGaussianGeodeticPoint3D other)
        {
            if (other != null)
            {
                GeodeticMean = DeepCopy(other.GeodeticMean);
                CovarianceNED = DeepCopy(other.CovarianceNED);
                ReferencePoint = DeepCopy(other.ReferencePoint);
            }
        }

        public GaussianGeodeticPoint3D(GeodeticPoint3D mean, Matrix3x3 covarianceNed)
        {
            GeodeticMean = DeepCopy(mean);
            CovarianceNED = DeepCopy(covarianceNed);
            ReferencePoint = DeepCopy(mean);
        }

        public GaussianGeodeticPoint3D(GeodeticPoint3D mean, Matrix3x3 covarianceNed, GeodeticPoint3D referencePoint)
        {
            GeodeticMean = DeepCopy(mean);
            CovarianceNED = DeepCopy(covarianceNed);
            ReferencePoint = DeepCopy(referencePoint);
        }

        public GaussianGeodeticPoint3D(double latitudeWgs84, double longitudeWgs84, double tvdWgs84, Matrix3x3 covarianceNed)
        {
            GeodeticMean = new GeodeticPoint3D
            {
                LatitudeWGS84 = latitudeWgs84,
                LongitudeWGS84 = longitudeWgs84,
                TvdWGS84 = tvdWgs84
            };
            CovarianceNED = DeepCopy(covarianceNed);
            ReferencePoint = DeepCopy(GeodeticMean);
        }

        public GaussianGeodeticPoint3D(double? latitudeWgs84, double? longitudeWgs84, double? tvdWgs84,
            double? cov00, double? cov01, double? cov02,
            double? cov10, double? cov11, double? cov12,
            double? cov20, double? cov21, double? cov22)
        {
            GeodeticMean = new GeodeticPoint3D
            {
                LatitudeWGS84 = latitudeWgs84,
                LongitudeWGS84 = longitudeWgs84,
                TvdWGS84 = tvdWgs84
            };
            CovarianceNED = new Matrix3x3(cov00, cov01, cov02, cov10, cov11, cov12, cov20, cov21, cov22);
            ReferencePoint = DeepCopy(GeodeticMean);
        }

        public virtual object Clone()
        {
            return new GaussianGeodeticPoint3D(this);
        }

        public virtual void SetZero()
        {
            GeodeticMean ??= new GeodeticPoint3D();
            GeodeticMean.LatitudeWGS84 = 0.0;
            GeodeticMean.LongitudeWGS84 = 0.0;
            GeodeticMean.TvdWGS84 = 0.0;
            CovarianceNED = new Matrix3x3(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            ReferencePoint ??= new GeodeticPoint3D();
            ReferencePoint.LatitudeWGS84 = 0.0;
            ReferencePoint.LongitudeWGS84 = 0.0;
            ReferencePoint.TvdWGS84 = 0.0;
        }

        public virtual bool IsZero()
        {
            return Numeric.EQ(LatitudeWGS84, 0.0) &&
                   Numeric.EQ(LongitudeWGS84, 0.0) &&
                   Numeric.EQ(TvdWGS84, 0.0) &&
                   IsZero(CovarianceNED);
        }

        public virtual bool IsValid()
        {
            return IsValidMean() && IsValidCovariance() && IsValidLinearizationPoint();
        }

        public virtual bool IsValidMean()
        {
            return GeodeticMean != null &&
                   GeodeticMean.LatitudeWGS84 != null &&
                   GeodeticMean.LongitudeWGS84 != null &&
                   GeodeticMean.TvdWGS84 != null;
        }

        public virtual bool IsValidCovariance()
        {
            return CovarianceNED != null &&
                   CovarianceNED[0, 0] != null &&
                   CovarianceNED[0, 1] != null &&
                   CovarianceNED[0, 2] != null &&
                   CovarianceNED[1, 0] != null &&
                   CovarianceNED[1, 1] != null &&
                   CovarianceNED[1, 2] != null &&
                   CovarianceNED[2, 0] != null &&
                   CovarianceNED[2, 1] != null &&
                   CovarianceNED[2, 2] != null;
        }

        public virtual bool IsValidLinearizationPoint()
        {
            GeodeticPoint3D? p = GetReferencePointOrMean();
            return p != null &&
                   p.LatitudeWGS84 != null &&
                   p.LongitudeWGS84 != null &&
                   p.TvdWGS84 != null;
        }

        public virtual bool HasIndependentCoordinates()
        {
            return IsValidCovariance() &&
                   CovarianceNED![0, 1] == 0.0 && CovarianceNED[1, 0] == 0.0 &&
                   CovarianceNED[0, 2] == 0.0 && CovarianceNED[2, 0] == 0.0 &&
                   CovarianceNED[1, 2] == 0.0 && CovarianceNED[2, 1] == 0.0;
        }

        public virtual void SetMean(GeodeticPoint3D point)
        {
            GeodeticMean = DeepCopy(point);
        }

        public virtual void SetMean(double latitudeWgs84, double longitudeWgs84, double tvdWgs84)
        {
            GeodeticMean ??= new GeodeticPoint3D();
            GeodeticMean.LatitudeWGS84 = latitudeWgs84;
            GeodeticMean.LongitudeWGS84 = longitudeWgs84;
            GeodeticMean.TvdWGS84 = tvdWgs84;
        }

        public virtual void SetCovariance(Matrix3x3 covarianceNed)
        {
            CovarianceNED = DeepCopy(covarianceNed);
        }

        public virtual void SetReferencePoint(GeodeticPoint3D point)
        {
            ReferencePoint = DeepCopy(point);
        }

        public virtual GeodeticPoint3D? GetMeanPoint()
        {
            if (!IsValidMean())
            {
                return null;
            }

            return new GeodeticPoint3D
            {
                LatitudeWGS84 = GeodeticMean!.LatitudeWGS84,
                LongitudeWGS84 = GeodeticMean.LongitudeWGS84,
                TvdWGS84 = GeodeticMean.TvdWGS84
            };
        }

        public virtual GeodeticPoint3D? GetReferencePointOrMean()
        {
            if (ReferencePoint?.LatitudeWGS84 != null &&
                ReferencePoint.LongitudeWGS84 != null &&
                ReferencePoint.TvdWGS84 != null)
            {
                return ReferencePoint;
            }

            return GetMeanPoint();
        }

        /// <summary>
        /// Returns the mean position in a local NED frame defined by <paramref name="reference"/>.
        /// </summary>
        public virtual Point3D? ToLocalNED(GeodeticPoint3D reference)
        {
            return GeodeticTransforms.GeodeticToLocalNED(GetMeanPoint(), reference);
        }

        /// <summary>
        /// Returns this covariance re-expressed in the NED frame attached to <paramref name="reference"/>.
        /// This only rotates the covariance between tangent frames; it does not change the mean.
        /// </summary>
        public virtual Matrix3x3? GetCovarianceNED(GeodeticPoint3D reference)
        {
            if (!IsValidCovariance())
            {
                return null;
            }

            GeodeticPoint3D? sourceReference = GetReferencePointOrMean();
            if (sourceReference == null || reference == null)
            {
                return null;
            }

            if (AreSameGeodeticPoint(sourceReference, reference))
            {
                return DeepCopy(CovarianceNED);
            }

            double[,] cSource = GeodeticTransforms.CreateGeocentricToNedRotation(sourceReference);
            double[,] cTarget = GeodeticTransforms.CreateGeocentricToNedRotation(reference);
            double[,] sigmaSource = ToArray(CovarianceNED!);

            double[,] sigmaEearthCentered = Multiply(Transpose(cSource), Multiply(sigmaSource, cSource));
            double[,] sigmaTarget = Multiply(cTarget, Multiply(sigmaEearthCentered, Transpose(cTarget)));

            return FromArray(sigmaTarget);
        }

        /// <summary>
        /// Returns a local Gaussian point in the NED frame defined by <paramref name="reference"/>.
        /// Mean and covariance are both expressed in that frame.
        /// </summary>
        public virtual GaussianPoint3D? ToGaussianLocalNED(GeodeticPoint3D reference)
        {
            Point3D? meanLocal = ToLocalNED(reference);
            Matrix3x3? covarianceLocal = GetCovarianceNED(reference);

            if (meanLocal == null || covarianceLocal == null)
            {
                return null;
            }

            return new GaussianPoint3D(meanLocal, covarianceLocal);
        }

        /// <summary>
        /// Creates an uncertainty-aware geodetic point from a local Gaussian NED point.
        /// The local frame is defined by <paramref name="reference"/>.
        /// </summary>
        public static GaussianGeodeticPoint3D? FromGaussianLocalNED(GaussianPoint3D localNed, GeodeticPoint3D reference)
        {
            if (localNed == null || !localNed.IsValid() || reference == null)
            {
                return null;
            }

            Point3D? localMean = localNed.GetMeanPoint();
            if (localMean == null)
            {
                return null;
            }

            GeodeticPoint3D? mean = GeodeticTransforms.LocalNEDToGeodetic(localMean, reference);
            if (mean == null)
            {
                return null;
            }

            return new GaussianGeodeticPoint3D(mean, new Matrix3x3(localNed.Covariance!), new GeodeticPoint3D(reference));
        }

        /// <summary>
        /// Deterministic helper: converts this mean point to local NED coordinates relative to <paramref name="reference"/>.
        /// </summary>
        public virtual Point3D? GeodeticToLocalNED(GeodeticPoint3D reference)
        {
            return GeodeticTransforms.GeodeticToLocalNED(GetMeanPoint(), reference);
        }

        /// <summary>
        /// Deterministic helper: converts local NED coordinates to geodetic coordinates using <paramref name="reference"/>.
        /// </summary>
        public static GeodeticPoint3D? LocalNEDToGeodetic(Point3D localNed, GeodeticPoint3D reference)
        {
            return GeodeticTransforms.LocalNEDToGeodetic(localNed, reference);
        }

        public virtual bool EQ(GeodeticPoint3D cmp)
        {
            if (cmp == null)
            {
                return false;
            }

            return Numeric.EQ(LatitudeWGS84, cmp.LatitudeWGS84) &&
                   Numeric.EQ(LongitudeWGS84, cmp.LongitudeWGS84) &&
                   Numeric.EQ(TvdWGS84, cmp.TvdWGS84);
        }

        public virtual bool EQ(GeodeticPoint3D cmp, double precision)
        {
            if (cmp == null)
            {
                return false;
            }

            return Numeric.EQ(LatitudeWGS84, cmp.LatitudeWGS84, precision) &&
                   Numeric.EQ(LongitudeWGS84, cmp.LongitudeWGS84, precision) &&
                   Numeric.EQ(TvdWGS84, cmp.TvdWGS84, precision);
        }

        /// <summary>
        /// Realizes a sample from this geodetic Gaussian distribution in the general case where
        /// the NED covariance coordinates are correlated.
        /// <para>
        /// Strategy (linearize → sample → back-project):
        /// <list type="number">
        ///   <item>Re-express the covariance in the NED frame of the effective reference point.</item>
        ///   <item>Cholesky-decompose that covariance: Σ = L·Lᵀ.</item>
        ///   <item>Draw z ~ N(0,I₃) and form the NED sample: ned_sample = ned_mean + L·z.</item>
        ///   <item>Back-project the NED sample to geodetic WGS84 via <see cref="GeodeticTransforms.LocalNEDToGeodetic"/>.</item>
        /// </list>
        /// </para>
        /// <remarks>
        /// The non-linearity of the geodetic ↔ NED mapping is deliberately confined to step 4.
        /// Steps 1–3 are entirely linear, which is consistent with the linearized-covariance
        /// contract of this class. The approximation is accurate as long as the uncertainty
        /// ellipsoid is small relative to the Earth's curvature — the same assumption that
        /// underlies the covariance representation itself.
        /// Both <see cref="Realize"/> and <see cref="RealizeIndependent"/> apply the same
        /// back-projection, so the distinction between them is identical to the Euclidean case:
        /// <see cref="Realize"/> handles correlated NED axes via Cholesky;
        /// <see cref="RealizeIndependent"/> uses a cheaper per-axis sampling when the NED
        /// covariance is diagonal.
        /// </remarks>
        /// </summary>
        /// <returns>
        /// A sampled <see cref="GeodeticPoint3D"/>, or <c>null</c> if the state is invalid or
        /// the Cholesky decomposition fails (e.g. covariance is not positive-definite).
        /// </returns>
        public GeodeticPoint3D? Realize()
        {
            if (!IsValid())
                return null;

            GeodeticPoint3D? referencePoint = GetReferencePointOrMean();
            if (referencePoint == null)
                return null;

            // Re-express the covariance in the NED frame of the reference point.
            Matrix3x3? covNed = GetCovarianceNED(referencePoint);
            if (covNed == null)
                return null;

            // NED coordinates of the mean relative to the reference point.
            Point3D? nedMean = GeodeticTransforms.GeodeticToLocalNED(GetMeanPoint(), referencePoint);
            if (nedMean?.X == null || nedMean.Y == null || nedMean.Z == null)
                return null;

            var mu = Vector<double>.Build.Dense(new[]
            {
                nedMean.X!.Value,
                nedMean.Y!.Value,
                nedMean.Z!.Value
            });

            var sigma = Matrix<double>.Build.DenseOfArray(new[,]
            {
                { covNed[0, 0]!.Value, covNed[0, 1]!.Value, covNed[0, 2]!.Value },
                { covNed[1, 0]!.Value, covNed[1, 1]!.Value, covNed[1, 2]!.Value },
                { covNed[2, 0]!.Value, covNed[2, 1]!.Value, covNed[2, 2]!.Value }
            });

            try
            {
                var chol = sigma.Cholesky();

                var z = Vector<double>.Build.Dense(new[]
                {
                    new Normal(0, 1).Sample(),
                    new Normal(0, 1).Sample(),
                    new Normal(0, 1).Sample()
                });

                var nedSample = mu + chol.Factor * z;
                var nedSamplePoint = new Point3D(nedSample[0], nedSample[1], nedSample[2]);

                return GeodeticTransforms.LocalNEDToGeodetic(nedSamplePoint, referencePoint);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Realizes a sample from this geodetic Gaussian distribution in the special case where
        /// the NED covariance is diagonal (North, East, Down uncertainties are independent).
        /// <para>
        /// When <see cref="HasIndependentCoordinates"/> is true the three NED axes are statistically
        /// independent and can each be sampled directly from their marginal 1-D Gaussian, avoiding
        /// the Cholesky factorization required by <see cref="Realize"/>.
        /// </para>
        /// <remarks>
        /// See <see cref="Realize"/> for a discussion of how the geodetic non-linearity is handled.
        /// The back-projection step is identical in both methods; only the NED sampling strategy differs.
        /// </remarks>
        /// </summary>
        /// <returns>
        /// A sampled <see cref="GeodeticPoint3D"/>, or <c>null</c> if the state is invalid,
        /// the NED covariance is not diagonal, or the back-projection fails.
        /// </returns>
        public GeodeticPoint3D? RealizeIndependent()
        {
            if (!IsValid() || !HasIndependentCoordinates())
                return null;

            GeodeticPoint3D? referencePoint = GetReferencePointOrMean();
            if (referencePoint == null)
                return null;

            // Re-express the covariance in the NED frame of the reference point.
            Matrix3x3? covNed = GetCovarianceNED(referencePoint);
            if (covNed == null)
                return null;

            // NED coordinates of the mean relative to the reference point.
            Point3D? nedMean = GeodeticTransforms.GeodeticToLocalNED(GetMeanPoint(), referencePoint);
            if (nedMean?.X == null || nedMean.Y == null || nedMean.Z == null)
                return null;

            var gNorth = new GaussianDistribution(nedMean.X, System.Math.Sqrt(covNed[0, 0]!.Value));
            var gEast = new GaussianDistribution(nedMean.Y, System.Math.Sqrt(covNed[1, 1]!.Value));
            var gDown = new GaussianDistribution(nedMean.Z, System.Math.Sqrt(covNed[2, 2]!.Value));

            var nedSamplePoint = new Point3D(gNorth.Realize(), gEast.Realize(), gDown.Realize());

            return GeodeticTransforms.LocalNEDToGeodetic(nedSamplePoint, referencePoint);
        }

        protected static GeodeticPoint3D? DeepCopy(GeodeticPoint3D? src)
        {
            if (src == null)
            {
                return null;
            }

            return new GeodeticPoint3D
            {
                LatitudeWGS84 = src.LatitudeWGS84,
                LongitudeWGS84 = src.LongitudeWGS84,
                TvdWGS84 = src.TvdWGS84
            };
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

        protected static bool AreSameGeodeticPoint(GeodeticPoint3D p1, GeodeticPoint3D p2)
        {
            return Numeric.EQ(p1.LatitudeWGS84, p2.LatitudeWGS84) &&
                   Numeric.EQ(p1.LongitudeWGS84, p2.LongitudeWGS84) &&
                   Numeric.EQ(p1.TvdWGS84, p2.TvdWGS84);
        }

        protected static double[,] ToArray(Matrix3x3 matrix)
        {
            return new[,]
            {
                { matrix[0, 0]!.Value, matrix[0, 1]!.Value, matrix[0, 2]!.Value },
                { matrix[1, 0]!.Value, matrix[1, 1]!.Value, matrix[1, 2]!.Value },
                { matrix[2, 0]!.Value, matrix[2, 1]!.Value, matrix[2, 2]!.Value }
            };
        }

        protected static Matrix3x3 FromArray(double[,] values)
        {
            return new Matrix3x3(
                values[0, 0], values[0, 1], values[0, 2],
                values[1, 0], values[1, 1], values[1, 2],
                values[2, 0], values[2, 1], values[2, 2]);
        }

        protected static double[,] Transpose(double[,] matrix)
        {
            return new[,]
            {
                { matrix[0, 0], matrix[1, 0], matrix[2, 0] },
                { matrix[0, 1], matrix[1, 1], matrix[2, 1] },
                { matrix[0, 2], matrix[1, 2], matrix[2, 2] }
            };
        }

        protected static double[,] Multiply(double[,] a, double[,] b)
        {
            return new[,]
            {
                {
                    a[0, 0] * b[0, 0] + a[0, 1] * b[1, 0] + a[0, 2] * b[2, 0],
                    a[0, 0] * b[0, 1] + a[0, 1] * b[1, 1] + a[0, 2] * b[2, 1],
                    a[0, 0] * b[0, 2] + a[0, 1] * b[1, 2] + a[0, 2] * b[2, 2]
                },
                {
                    a[1, 0] * b[0, 0] + a[1, 1] * b[1, 0] + a[1, 2] * b[2, 0],
                    a[1, 0] * b[0, 1] + a[1, 1] * b[1, 1] + a[1, 2] * b[2, 1],
                    a[1, 0] * b[0, 2] + a[1, 1] * b[1, 2] + a[1, 2] * b[2, 2]
                },
                {
                    a[2, 0] * b[0, 0] + a[2, 1] * b[1, 0] + a[2, 2] * b[2, 0],
                    a[2, 0] * b[0, 1] + a[2, 1] * b[1, 1] + a[2, 2] * b[2, 1],
                    a[2, 0] * b[0, 2] + a[2, 1] * b[1, 2] + a[2, 2] * b[2, 2]
                }
            };
        }
    }
}