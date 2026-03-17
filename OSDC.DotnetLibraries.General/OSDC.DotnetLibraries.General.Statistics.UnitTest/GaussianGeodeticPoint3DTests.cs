using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Statistics.UnitTest
{
    public class GaussianGeodeticPoint3DTests
    {
        [SetUp]
        public void Setup()
        {
        }

        private static void AssertGeodeticPointEqual(GeodeticPoint3D? p, double? lat, double? lon, double? tvd, double tol = 1e-10)
        {
            Assert.That(p, Is.Not.Null);
            Assert.That(p!.LatitudeWGS84, Is.Not.Null);
            Assert.That(p.LongitudeWGS84, Is.Not.Null);
            Assert.That(p.TvdWGS84, Is.Not.Null);
            Assert.That(System.Math.Abs(p.LatitudeWGS84!.Value - lat!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(p.LongitudeWGS84!.Value - lon!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(p.TvdWGS84!.Value - tvd!.Value), Is.LessThanOrEqualTo(tol));
        }

        private static void AssertPointEqual(Point3D? p, double? x, double? y, double? z, double tol = 1e-8)
        {
            Assert.That(p, Is.Not.Null);
            Assert.That(p!.X, Is.Not.Null);
            Assert.That(p.Y, Is.Not.Null);
            Assert.That(p.Z, Is.Not.Null);
            Assert.That(System.Math.Abs(p.X!.Value - x!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(p.Y!.Value - y!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(p.Z!.Value - z!.Value), Is.LessThanOrEqualTo(tol));
        }

        private static void AssertMatrixEqual(Matrix3x3? m,
                                              double? m00, double? m01, double? m02,
                                              double? m10, double? m11, double? m12,
                                              double? m20, double? m21, double? m22,
                                              double tol = 1e-10)
        {
            Assert.That(m, Is.Not.Null);
            Assert.That(System.Math.Abs(m![0, 0]!.Value - m00!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(m[0, 1]!.Value - m01!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(m[0, 2]!.Value - m02!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(m[1, 0]!.Value - m10!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(m[1, 1]!.Value - m11!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(m[1, 2]!.Value - m12!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(m[2, 0]!.Value - m20!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(m[2, 1]!.Value - m21!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(m[2, 2]!.Value - m22!.Value), Is.LessThanOrEqualTo(tol));
        }

        [Test]
        public void Test_DefaultConstructor()
        {
            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D();

            Assert.That(p.GeodeticMean, Is.Null);
            Assert.That(p.CovarianceNED, Is.Null);
            Assert.That(p.ReferencePoint, Is.Null);
            Assert.That(p.LatitudeWGS84, Is.Null);
            Assert.That(p.LongitudeWGS84, Is.Null);
            Assert.That(p.TvdWGS84, Is.Null);
            Assert.IsFalse(p.IsValidMean());
            Assert.IsFalse(p.IsValidCovariance());
            Assert.IsFalse(p.IsValid());
        }

        [Test]
        public void Test_Constructor_MeanAndCovariance_DefaultLinearizationAtMean()
        {
            GeodeticPoint3D mean = new GeodeticPoint3D
            {
                LatitudeWGS84 = 60.0,
                LongitudeWGS84 = 2.0,
                TvdWGS84 = 1500.0
            };
            Matrix3x3 cov = new Matrix3x3(4.0, 0.1, 0.2,
                                          0.1, 5.0, 0.3,
                                          0.2, 0.3, 6.0);

            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(mean, cov);

            AssertGeodeticPointEqual(p.GeodeticMean, 60.0, 2.0, 1500.0);
            AssertGeodeticPointEqual(p.ReferencePoint, 60.0, 2.0, 1500.0);
            AssertMatrixEqual(p.CovarianceNED,
                              4.0, 0.1, 0.2,
                              0.1, 5.0, 0.3,
                              0.2, 0.3, 6.0);
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void Test_Constructor_MeanCovarianceAndLinearization()
        {
            GeodeticPoint3D mean = new GeodeticPoint3D
            {
                LatitudeWGS84 = 61.0,
                LongitudeWGS84 = 3.0,
                TvdWGS84 = 2500.0
            };
            GeodeticPoint3D linearization = new GeodeticPoint3D
            {
                LatitudeWGS84 = 60.5,
                LongitudeWGS84 = 2.5,
                TvdWGS84 = 2000.0
            };
            Matrix3x3 cov = new Matrix3x3(1.0, 0.0, 0.0,
                                          0.0, 2.0, 0.0,
                                          0.0, 0.0, 3.0);

            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(mean, cov, linearization);

            AssertGeodeticPointEqual(p.GeodeticMean, 61.0, 3.0, 2500.0);
            AssertGeodeticPointEqual(p.ReferencePoint, 60.5, 2.5, 2000.0);
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void Test_Constructor_DoubleCoordinatesAndMatrix()
        {
            Matrix3x3 cov = new Matrix3x3(1.0, 0.0, 0.0,
                                          0.0, 2.0, 0.0,
                                          0.0, 0.0, 3.0);

            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(10.0, 20.0, 30.0, cov);

            Assert.That(p.LatitudeWGS84, Is.EqualTo(10.0));
            Assert.That(p.LongitudeWGS84, Is.EqualTo(20.0));
            Assert.That(p.TvdWGS84, Is.EqualTo(30.0));
            AssertGeodeticPointEqual(p.ReferencePoint, 10.0, 20.0, 30.0);
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void Test_Constructor_FullCoordinateAndCovarianceArguments()
        {
            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(
                1.0, 2.0, 3.0,
                10.0, 0.5, 0.6,
                0.5, 20.0, 0.7,
                0.6, 0.7, 30.0);

            AssertGeodeticPointEqual(p.GeodeticMean, 1.0, 2.0, 3.0);
            AssertMatrixEqual(p.CovarianceNED,
                              10.0, 0.5, 0.6,
                              0.5, 20.0, 0.7,
                              0.6, 0.7, 30.0);
            AssertGeodeticPointEqual(p.ReferencePoint, 1.0, 2.0, 3.0);
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void Test_CopyConstructor_DeepCopy()
        {
            GaussianGeodeticPoint3D p1 = new GaussianGeodeticPoint3D(
                1.0, 2.0, 3.0,
                10.0, 0.1, 0.2,
                0.1, 20.0, 0.3,
                0.2, 0.3, 30.0);
            p1.SetReferencePoint(new GeodeticPoint3D
            {
                LatitudeWGS84 = 4.0,
                LongitudeWGS84 = 5.0,
                TvdWGS84 = 6.0
            });

            GaussianGeodeticPoint3D p2 = new GaussianGeodeticPoint3D(p1);

            Assert.That(p2, Is.Not.SameAs(p1));
            Assert.That(p2.GeodeticMean, Is.Not.SameAs(p1.GeodeticMean));
            Assert.That(p2.CovarianceNED, Is.Not.SameAs(p1.CovarianceNED));
            Assert.That(p2.ReferencePoint, Is.Not.SameAs(p1.ReferencePoint));

            AssertGeodeticPointEqual(p2.GeodeticMean, 1.0, 2.0, 3.0);
            AssertGeodeticPointEqual(p2.ReferencePoint, 4.0, 5.0, 6.0);
            AssertMatrixEqual(p2.CovarianceNED,
                              10.0, 0.1, 0.2,
                              0.1, 20.0, 0.3,
                              0.2, 0.3, 30.0);

            p1.LatitudeWGS84 = 100.0;
            p1.CovarianceNED![0, 0] = 999.0;
            p1.ReferencePoint!.LatitudeWGS84 = 200.0;

            Assert.That(p2.LatitudeWGS84, Is.EqualTo(1.0));
            Assert.That(p2.CovarianceNED![0, 0], Is.EqualTo(10.0));
            Assert.That(p2.ReferencePoint!.LatitudeWGS84, Is.EqualTo(4.0));
        }

        [Test]
        public void Test_Clone_DeepCopy()
        {
            GaussianGeodeticPoint3D p1 = new GaussianGeodeticPoint3D(
                1.0, 2.0, 3.0,
                4.0, 0.0, 0.0,
                0.0, 5.0, 0.0,
                0.0, 0.0, 6.0);
            p1.SetReferencePoint(new GeodeticPoint3D
            {
                LatitudeWGS84 = 7.0,
                LongitudeWGS84 = 8.0,
                TvdWGS84 = 9.0
            });

            GaussianGeodeticPoint3D p2 = (GaussianGeodeticPoint3D)p1.Clone();

            Assert.That(p2, Is.Not.SameAs(p1));
            Assert.That(p2.GeodeticMean, Is.Not.SameAs(p1.GeodeticMean));
            Assert.That(p2.CovarianceNED, Is.Not.SameAs(p1.CovarianceNED));
            Assert.That(p2.ReferencePoint, Is.Not.SameAs(p1.ReferencePoint));

            AssertGeodeticPointEqual(p2.GeodeticMean, 1.0, 2.0, 3.0);
            AssertGeodeticPointEqual(p2.ReferencePoint, 7.0, 8.0, 9.0);
            AssertMatrixEqual(p2.CovarianceNED,
                              4.0, 0.0, 0.0,
                              0.0, 5.0, 0.0,
                              0.0, 0.0, 6.0);
        }

        [Test]
        public void Test_SetZero()
        {
            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);

            p.SetZero();

            AssertGeodeticPointEqual(p.GeodeticMean, 0.0, 0.0, 0.0);
            AssertGeodeticPointEqual(p.ReferencePoint, 0.0, 0.0, 0.0);
            AssertMatrixEqual(p.CovarianceNED,
                              0.0, 0.0, 0.0,
                              0.0, 0.0, 0.0,
                              0.0, 0.0, 0.0);
            Assert.IsTrue(p.IsZero());
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void Test_IsValidMean_And_IsValid()
        {
            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D();

            Assert.IsFalse(p.IsValidMean());
            Assert.IsFalse(p.IsValid());

            p.SetMean(1.0, 2.0, 3.0);
            Assert.IsTrue(p.IsValidMean());
            Assert.IsFalse(p.IsValid());

            p.SetCovariance(new Matrix3x3(1.0, 0.0, 0.0,
                                          0.0, 1.0, 0.0,
                                          0.0, 0.0, 1.0));
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void Test_HasIndependentCoordinates_True()
        {
            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(
                1.0, 2.0, 3.0,
                4.0, 0.0, 0.0,
                0.0, 5.0, 0.0,
                0.0, 0.0, 6.0);

            Assert.IsTrue(p.HasIndependentCoordinates());
        }

        [Test]
        public void Test_HasIndependentCoordinates_False()
        {
            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(
                1.0, 2.0, 3.0,
                4.0, 0.1, 0.0,
                0.1, 5.0, 0.0,
                0.0, 0.0, 6.0);

            Assert.IsFalse(p.HasIndependentCoordinates());
        }

        [Test]
        public void Test_SetMean_SetCovariance_SetReferencePoint()
        {
            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D();

            p.SetMean(10.0, 20.0, 30.0);
            p.SetCovariance(new Matrix3x3(1.0, 2.0, 3.0,
                                          4.0, 5.0, 6.0,
                                          7.0, 8.0, 9.0));
            p.SetReferencePoint(new GeodeticPoint3D
            {
                LatitudeWGS84 = 11.0,
                LongitudeWGS84 = 21.0,
                TvdWGS84 = 31.0
            });

            AssertGeodeticPointEqual(p.GeodeticMean, 10.0, 20.0, 30.0);
            AssertGeodeticPointEqual(p.ReferencePoint, 11.0, 21.0, 31.0);
            AssertMatrixEqual(p.CovarianceNED,
                              1.0, 2.0, 3.0,
                              4.0, 5.0, 6.0,
                              7.0, 8.0, 9.0);
        }

        [Test]
        public void Test_GetMeanPoint_And_GetReferencePointOrMean()
        {
            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(
                7.0, 8.0, 9.0,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);

            GeodeticPoint3D? mean = p.GetMeanPoint();
            GeodeticPoint3D? ref1 = p.GetReferencePointOrMean();

            AssertGeodeticPointEqual(mean, 7.0, 8.0, 9.0);
            AssertGeodeticPointEqual(ref1, 7.0, 8.0, 9.0);

            p.ReferencePoint = null;
            GeodeticPoint3D? ref2 = p.GetReferencePointOrMean();
            AssertGeodeticPointEqual(ref2, 7.0, 8.0, 9.0);
        }

        [Test]
        public void Test_EQ_OnMeanOnly()
        {
            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(
                1.0, 2.0, 3.0,
                10.0, 1.0, 2.0,
                1.0, 20.0, 3.0,
                2.0, 3.0, 30.0);

            GeodeticPoint3D q = new GeodeticPoint3D
            {
                LatitudeWGS84 = 1.0,
                LongitudeWGS84 = 2.0,
                TvdWGS84 = 3.0
            };

            Assert.IsTrue(p.EQ(q));
            Assert.IsTrue(p.EQ(q, 1e-12));
        }

        [Test]
        public void Test_ToLocalNED_SameReference_GivesZero()
        {
            GeodeticPoint3D reference = new GeodeticPoint3D
            {
                LatitudeWGS84 = 60.0,
                LongitudeWGS84 = 2.0,
                TvdWGS84 = 1500.0
            };

            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(reference,
                new Matrix3x3(1.0, 0.0, 0.0,
                              0.0, 1.0, 0.0,
                              0.0, 0.0, 1.0));

            Point3D? local = p.ToLocalNED(reference);

            AssertPointEqual(local, 0.0, 0.0, 0.0, 1e-6);
        }

        [Test]
        public void Test_GetCovarianceNED_SameReference_Unchanged()
        {
            GeodeticPoint3D reference = new GeodeticPoint3D
            {
                LatitudeWGS84 = 60.0,
                LongitudeWGS84 = 2.0,
                TvdWGS84 = 1500.0
            };

            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(reference,
                new Matrix3x3(4.0, 0.1, 0.2,
                              0.1, 5.0, 0.3,
                              0.2, 0.3, 6.0));

            Matrix3x3? cov = p.GetCovarianceNED(reference);

            AssertMatrixEqual(cov,
                              4.0, 0.1, 0.2,
                              0.1, 5.0, 0.3,
                              0.2, 0.3, 6.0);
        }

        [Test]
        public void Test_ToGaussianLocalNED_SameReference()
        {
            GeodeticPoint3D reference = new GeodeticPoint3D
            {
                LatitudeWGS84 = 60.0,
                LongitudeWGS84 = 2.0,
                TvdWGS84 = 1500.0
            };

            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(reference,
                new Matrix3x3(4.0, 0.1, 0.2,
                              0.1, 5.0, 0.3,
                              0.2, 0.3, 6.0));

            GaussianPoint3D? local = p.ToGaussianLocalNED(reference);

            Assert.That(local, Is.Not.Null);
            AssertPointEqual(local!.GetMeanPoint(), 0.0, 0.0, 0.0, 1e-6);
            AssertMatrixEqual(local.Covariance,
                              4.0, 0.1, 0.2,
                              0.1, 5.0, 0.3,
                              0.2, 0.3, 6.0);
        }

        [Test]
        public void Test_LocalNEDToGeodetic_And_Back_RoundTrip()
        {
            GeodeticPoint3D reference = new GeodeticPoint3D
            {
                LatitudeWGS84 = 60.0,
                LongitudeWGS84 = 2.0,
                TvdWGS84 = 1000.0
            };
            Point3D local = new Point3D(100.0, 50.0, 25.0);

            GeodeticPoint3D? geodetic = GaussianGeodeticPoint3D.LocalNEDToGeodetic(local, reference);
            Assert.That(geodetic, Is.Not.Null);

            GaussianGeodeticPoint3D gaussian = new GaussianGeodeticPoint3D(geodetic!,
                new Matrix3x3(1.0, 0.0, 0.0,
                              0.0, 1.0, 0.0,
                              0.0, 0.0, 1.0));

            Point3D? localRecovered = gaussian.ToLocalNED(reference);

            AssertPointEqual(localRecovered, 100.0, 50.0, 25.0, 1e-3);
        }

        [Test]
        public void Test_FromGaussianLocalNED()
        {
            GeodeticPoint3D reference = new GeodeticPoint3D
            {
                LatitudeWGS84 = 60.0,
                LongitudeWGS84 = 2.0,
                TvdWGS84 = 1000.0
            };
            GaussianPoint3D local = new GaussianPoint3D(
                100.0, 50.0, 25.0,
                4.0, 0.1, 0.2,
                0.1, 5.0, 0.3,
                0.2, 0.3, 6.0);

            GaussianGeodeticPoint3D? geo = GaussianGeodeticPoint3D.FromGaussianLocalNED(local, reference);

            Assert.That(geo, Is.Not.Null);
            AssertGeodeticPointEqual(geo!.ReferencePoint, 60.0, 2.0, 1000.0);
            AssertMatrixEqual(geo.CovarianceNED,
                              4.0, 0.1, 0.2,
                              0.1, 5.0, 0.3,
                              0.2, 0.3, 6.0);

            GaussianPoint3D? localRecovered = geo.ToGaussianLocalNED(reference);
            Assert.That(localRecovered, Is.Not.Null);
            AssertPointEqual(localRecovered!.GetMeanPoint(), 100.0, 50.0, 25.0, 1e-3);
            AssertMatrixEqual(localRecovered.Covariance,
                              4.0, 0.1, 0.2,
                              0.1, 5.0, 0.3,
                              0.2, 0.3, 6.0);
        }

        [Test]
        public void Test_GetCovarianceNED_RotationBetweenFrames_PreservesTrace()
        {
            GeodeticPoint3D sourceReference = new GeodeticPoint3D
            {
                LatitudeWGS84 = 60.0,
                LongitudeWGS84 = 2.0,
                TvdWGS84 = 1000.0
            };
            GeodeticPoint3D targetReference = new GeodeticPoint3D
            {
                LatitudeWGS84 = 60.001,
                LongitudeWGS84 = 2.001,
                TvdWGS84 = 1000.0
            };

            GaussianGeodeticPoint3D p = new GaussianGeodeticPoint3D(
                sourceReference,
                new Matrix3x3(4.0, 0.1, 0.2,
                              0.1, 5.0, 0.3,
                              0.2, 0.3, 6.0),
                sourceReference);

            Matrix3x3? rotated = p.GetCovarianceNED(targetReference);

            Assert.That(rotated, Is.Not.Null);

            double traceOriginal = p.CovarianceNED![0, 0]!.Value + p.CovarianceNED[1, 1]!.Value + p.CovarianceNED[2, 2]!.Value;
            double traceRotated = rotated![0, 0]!.Value + rotated[1, 1]!.Value + rotated[2, 2]!.Value;

            Assert.That(traceRotated, Is.EqualTo(traceOriginal).Within(1e-8));
        }
    }
}
