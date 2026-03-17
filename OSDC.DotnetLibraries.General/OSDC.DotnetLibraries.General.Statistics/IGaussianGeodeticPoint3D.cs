using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Statistics
{
    /// <summary>
    /// Interface for an uncertainty-aware geodetic point in WGS84 coordinates.
    /// The mean is represented in geodetic WGS84 coordinates while the covariance is represented
    /// in a local North-East-Down (NED) tangent frame used as the linearization support to preserve
    /// the linearity of uncertainty propagation.
    /// </summary>
    public interface IGaussianGeodeticPoint3D
    {
        /// <summary>
        /// Mean position in geodetic WGS84 coordinates.
        /// </summary>
        GeodeticPoint3D? GeodeticMean { get; set; }

        /// <summary>
        /// 3x3 covariance matrix expressed in the local NED frame attached to <see cref="ReferencePoint"/>.
        /// </summary>
        Matrix3x3? CovarianceNED { get; set; }

        /// <summary>
        /// Reference geodetic point defining the local NED tangent frame used for covariance linearization.
        /// If null, the mean point is used as the linearization point.
        /// </summary>
        GeodeticPoint3D? ReferencePoint { get; set; }

        /// <summary>
        /// WGS84 latitude of the mean point, in degrees.
        /// </summary>
        double? LatitudeWGS84 { get; set; }

        /// <summary>
        /// WGS84 longitude of the mean point, in degrees.
        /// </summary>
        double? LongitudeWGS84 { get; set; }

        /// <summary>
        /// True vertical depth of the mean point relative to the WGS84 ellipsoid. Positive downward.
        /// </summary>
        double? TvdWGS84 { get; set; }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        object Clone();

        /// <summary>
        /// Sets all mean coordinates and covariance entries to zero.
        /// </summary>
        void SetZero();

        /// <summary>
        /// Returns true if all mean coordinates and covariance entries are zero.
        /// </summary>
        bool IsZero();

        /// <summary>
        /// Returns true if the mean, covariance, and linearization point are all valid (non-null).
        /// </summary>
        bool IsValid();

        /// <summary>
        /// Returns true if the mean geodetic coordinates are all non-null.
        /// </summary>
        bool IsValidMean();

        /// <summary>
        /// Returns true if all nine entries of the covariance matrix are non-null.
        /// </summary>
        bool IsValidCovariance();

        /// <summary>
        /// Returns true if the linearization point (reference point or mean) is valid.
        /// </summary>
        bool IsValidLinearizationPoint();

        /// <summary>
        /// Returns true if all off-diagonal entries of the covariance matrix are zero,
        /// indicating that the NED coordinate uncertainties are statistically independent.
        /// </summary>
        bool HasIndependentCoordinates();

        /// <summary>
        /// Sets the mean position from a <see cref="GeodeticPoint3D"/> instance.
        /// </summary>
        void SetMean(GeodeticPoint3D point);

        /// <summary>
        /// Sets the mean position from individual WGS84 coordinate components.
        /// </summary>
        void SetMean(double latitudeWgs84, double longitudeWgs84, double tvdWgs84);

        /// <summary>
        /// Sets the NED covariance matrix.
        /// </summary>
        void SetCovariance(Matrix3x3 covarianceNed);

        /// <summary>
        /// Sets the reference geodetic point used as the NED frame origin for covariance linearization.
        /// </summary>
        void SetReferencePoint(GeodeticPoint3D point);

        /// <summary>
        /// Returns a copy of the mean position as a <see cref="GeodeticPoint3D"/>, or null if the mean is invalid.
        /// </summary>
        GeodeticPoint3D? GetMeanPoint();

        /// <summary>
        /// Returns the reference point if valid, otherwise falls back to the mean point.
        /// </summary>
        GeodeticPoint3D? GetReferencePointOrMean();

        /// <summary>
        /// Returns the mean position expressed in the local NED frame defined by <paramref name="reference"/>.
        /// </summary>
        Point3D? ToLocalNED(GeodeticPoint3D reference);

        /// <summary>
        /// Returns this covariance re-expressed in the NED frame attached to <paramref name="reference"/>.
        /// Only rotates the covariance between tangent frames; the mean is not changed.
        /// </summary>
        Matrix3x3? GetCovarianceNED(GeodeticPoint3D reference);

        /// <summary>
        /// Returns a <see cref="GaussianPoint3D"/> with both mean and covariance expressed in the
        /// local NED frame defined by <paramref name="reference"/>.
        /// </summary>
        GaussianPoint3D? ToGaussianLocalNED(GeodeticPoint3D reference);

        /// <summary>
        /// Deterministic helper: converts the mean point to local NED coordinates relative to <paramref name="reference"/>.
        /// </summary>
        Point3D? GeodeticToLocalNED(GeodeticPoint3D reference);

        /// <summary>
        /// Returns true if the mean position equals the given geodetic point within default numeric tolerance.
        /// </summary>
        bool EQ(GeodeticPoint3D cmp);

        /// <summary>
        /// Returns true if the mean position equals the given geodetic point within the specified <paramref name="precision"/>.
        /// </summary>
        bool EQ(GeodeticPoint3D cmp, double precision);

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
        public GeodeticPoint3D? Realize();

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
        public GeodeticPoint3D? RealizeIndependent();
    }
}