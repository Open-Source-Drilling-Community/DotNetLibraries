using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Statistics.UnitTest
{
    public class GaussianPoint3DTests
    {
        [SetUp]
        public void Setup()
        {
        }

        private static void AssertPointEqual(Point3D? p, double? x, double? y, double? z, double tol = 1e-10)
        {
            Assert.That(p, Is.Not.Null);
            Assert.That(p!.X, Is.Not.Null);
            Assert.That(p.Y, Is.Not.Null);
            Assert.That(p.Z, Is.Not.Null);
            Assert.That(System.Math.Abs(p.X!.Value - x!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(p.Y!.Value - y!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(p.Z!.Value - z!.Value), Is.LessThanOrEqualTo(tol));
        }

        private static void AssertVectorEqual(Vector3D? v, double? x, double? y, double? z, double tol = 1e-10)
        {
            Assert.That(v, Is.Not.Null);
            Assert.That(v!.X, Is.Not.Null);
            Assert.That(v.Y, Is.Not.Null);
            Assert.That(v.Z, Is.Not.Null);
            Assert.That(System.Math.Abs(v.X!.Value - x!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(v.Y!.Value - y!.Value), Is.LessThanOrEqualTo(tol));
            Assert.That(System.Math.Abs(v.Z!.Value - z!.Value), Is.LessThanOrEqualTo(tol));
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
            GaussianPoint3D p = new GaussianPoint3D();

            Assert.That(p.Mean, Is.Null);
            Assert.That(p.Covariance, Is.Null);
            Assert.That(p.X, Is.Null);
            Assert.That(p.Y, Is.Null);
            Assert.That(p.Z, Is.Null);
            Assert.IsFalse(p.IsValidMean());
            Assert.IsFalse(p.IsValid());
        }

        [Test]
        public void Test_Constructor_VectorAndMatrix()
        {
            Vector3D mean = new Vector3D(1.0, 2.0, 3.0);
            Matrix3x3 cov = new Matrix3x3(4.0, 0.1, 0.2,
                                          0.1, 5.0, 0.3,
                                          0.2, 0.3, 6.0);

            GaussianPoint3D p = new GaussianPoint3D(mean, cov);

            AssertVectorEqual(p.Mean, 1.0, 2.0, 3.0);
            AssertMatrixEqual(p.Covariance,
                              4.0, 0.1, 0.2,
                              0.1, 5.0, 0.3,
                              0.2, 0.3, 6.0);
            Assert.IsTrue(p.IsValidMean());
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void Test_Constructor_DoubleCoordinatesAndMatrix()
        {
            Matrix3x3 cov = new Matrix3x3(1.0, 0.0, 0.0,
                                          0.0, 2.0, 0.0,
                                          0.0, 0.0, 3.0);

            GaussianPoint3D p = new GaussianPoint3D(10.0, 20.0, 30.0, cov);

            Assert.That(p.X, Is.EqualTo(10.0));
            Assert.That(p.Y, Is.EqualTo(20.0));
            Assert.That(p.Z, Is.EqualTo(30.0));
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void Test_Constructor_FullCoordinateAndCovarianceArguments()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                10.0, 0.5, 0.6,
                0.5, 20.0, 0.7,
                0.6, 0.7, 30.0);

            AssertVectorEqual(p.Mean, 1.0, 2.0, 3.0);
            AssertMatrixEqual(p.Covariance,
                              10.0, 0.5, 0.6,
                              0.5, 20.0, 0.7,
                              0.6, 0.7, 30.0);
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void Test_Constructor_FromPoint3DAndMatrix()
        {
            Point3D mean = new Point3D(4.0, 5.0, 6.0);
            Matrix3x3 cov = new Matrix3x3(7.0, 0.0, 0.0,
                                          0.0, 8.0, 0.0,
                                          0.0, 0.0, 9.0);

            GaussianPoint3D p = new GaussianPoint3D(mean, cov);

            AssertVectorEqual(p.Mean, 4.0, 5.0, 6.0);
            AssertMatrixEqual(p.Covariance,
                              7.0, 0.0, 0.0,
                              0.0, 8.0, 0.0,
                              0.0, 0.0, 9.0);
        }

        [Test]
        public void Test_Constructor_FromArrayAndMatrix()
        {
            double[] dat = new[] { 7.0, 8.0, 9.0 };
            Matrix3x3 cov = new Matrix3x3(1.0, 0.0, 0.0,
                                          0.0, 1.0, 0.0,
                                          0.0, 0.0, 1.0);

            GaussianPoint3D p = new GaussianPoint3D(dat, cov);

            AssertVectorEqual(p.Mean, 7.0, 8.0, 9.0);
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void Test_CopyConstructor_DeepCopy()
        {
            GaussianPoint3D p1 = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                10.0, 0.1, 0.2,
                0.1, 20.0, 0.3,
                0.2, 0.3, 30.0);

            GaussianPoint3D p2 = new GaussianPoint3D(p1);

            Assert.That(p2, Is.Not.SameAs(p1));
            Assert.That(p2.Mean, Is.Not.SameAs(p1.Mean));
            Assert.That(p2.Covariance, Is.Not.SameAs(p1.Covariance));

            AssertVectorEqual(p2.Mean, 1.0, 2.0, 3.0);
            AssertMatrixEqual(p2.Covariance,
                              10.0, 0.1, 0.2,
                              0.1, 20.0, 0.3,
                              0.2, 0.3, 30.0);

            p1.X = 100.0;
            p1.Covariance![0, 0] = 999.0;

            Assert.That(p2.X, Is.EqualTo(1.0));
            Assert.That(p2.Covariance![0, 0], Is.EqualTo(10.0));
        }

        [Test]
        public void Test_Clone_DeepCopy()
        {
            GaussianPoint3D p1 = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                4.0, 0.0, 0.0,
                0.0, 5.0, 0.0,
                0.0, 0.0, 6.0);

            GaussianPoint3D p2 = (GaussianPoint3D)p1.Clone();

            Assert.That(p2, Is.Not.SameAs(p1));
            Assert.That(p2.Mean, Is.Not.SameAs(p1.Mean));
            Assert.That(p2.Covariance, Is.Not.SameAs(p1.Covariance));

            AssertVectorEqual(p2.Mean, 1.0, 2.0, 3.0);
            AssertMatrixEqual(p2.Covariance,
                              4.0, 0.0, 0.0,
                              0.0, 5.0, 0.0,
                              0.0, 0.0, 6.0);
        }

        [Test]
        public void Test_SetZero()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);

            p.SetZero();

            AssertVectorEqual(p.Mean, 0.0, 0.0, 0.0);
            AssertMatrixEqual(p.Covariance,
                              0.0, 0.0, 0.0,
                              0.0, 0.0, 0.0,
                              0.0, 0.0, 0.0);
            Assert.IsTrue(p.IsZero());
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void Test_IsValidMean_And_IsValid()
        {
            GaussianPoint3D p = new GaussianPoint3D();

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
            GaussianPoint3D p = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                4.0, 0.0, 0.0,
                0.0, 5.0, 0.0,
                0.0, 0.0, 6.0);

            Assert.IsTrue(p.HasIndependentCoordinates());
        }

        [Test]
        public void Test_HasIndependentCoordinates_False()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                4.0, 0.1, 0.0,
                0.1, 5.0, 0.0,
                0.0, 0.0, 6.0);

            Assert.IsFalse(p.HasIndependentCoordinates());
        }

        [Test]
        public void Test_SetMean_And_SetCovariance()
        {
            GaussianPoint3D p = new GaussianPoint3D();

            p.SetMean(10.0, 20.0, 30.0);
            p.SetCovariance(new Matrix3x3(1.0, 2.0, 3.0,
                                          4.0, 5.0, 6.0,
                                          7.0, 8.0, 9.0));

            AssertVectorEqual(p.Mean, 10.0, 20.0, 30.0);
            AssertMatrixEqual(p.Covariance,
                              1.0, 2.0, 3.0,
                              4.0, 5.0, 6.0,
                              7.0, 8.0, 9.0);
        }

        [Test]
        public void Test_Set_FromGaussianPoint()
        {
            GaussianPoint3D p1 = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                10.0, 0.0, 0.0,
                0.0, 20.0, 0.0,
                0.0, 0.0, 30.0);

            GaussianPoint3D p2 = new GaussianPoint3D();
            p2.Set(p1);

            AssertVectorEqual(p2.Mean, 1.0, 2.0, 3.0);
            AssertMatrixEqual(p2.Covariance,
                              10.0, 0.0, 0.0,
                              0.0, 20.0, 0.0,
                              0.0, 0.0, 30.0);

            Assert.That(p2.Mean, Is.Not.SameAs(p1.Mean));
            Assert.That(p2.Covariance, Is.Not.SameAs(p1.Covariance));
        }

        [Test]
        public void Test_GetMeanPoint()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                7.0, 8.0, 9.0,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);

            Point3D? mean = p.GetMeanPoint();

            AssertPointEqual(mean, 7.0, 8.0, 9.0);
        }

        [Test]
        public void Test_GetPlaneRadius()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                3.0, 4.0, 10.0,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);

            Assert.That(p.GetPlaneRadius(), Is.EqualTo(5.0).Within(1e-10));
        }

        [Test]
        public void Test_GetRadius()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                2.0, 3.0, 6.0,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);

            Assert.That(p.GetRadius(), Is.EqualTo(7.0).Within(1e-10));
        }

        [Test]
        public void Test_GetDistance_ToDeterministicPoint()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);

            Point3D q = new Point3D(4.0, 6.0, 3.0);

            Assert.That(p.GetDistance(q), Is.EqualTo(5.0).Within(1e-10));
        }

        [Test]
        public void Test_MoveTo_Translate()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);

            p.MoveTo(10.0, 20.0, 30.0);
            AssertVectorEqual(p.Mean, 10.0, 20.0, 30.0);

            p.Translate(1.0, -2.0, 3.0);
            AssertVectorEqual(p.Mean, 11.0, 18.0, 33.0);

            AssertMatrixEqual(p.Covariance,
                              1.0, 0.0, 0.0,
                              0.0, 1.0, 0.0,
                              0.0, 0.0, 1.0);
        }

        [Test]
        public void Test_GetMiddle()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                0.0, 0.0, 0.0,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);

            Point3D q = new Point3D(2.0, 4.0, 6.0);

            Point3D? m = p.GetMiddle(q);

            AssertPointEqual(m, 1.0, 2.0, 3.0);
        }

        [Test]
        public void Test_GetPoint()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                0.0, 0.0, 0.0,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);

            Point3D q = new Point3D(10.0, 20.0, 30.0);

            Point3D? r = p.GetPoint(q, 0.25);

            AssertPointEqual(r, 2.5, 5.0, 7.5);
        }

        [Test]
        public void Test_CrossProductVector_And_AreColinear()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                0.0, 0.0, 0.0,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);

            Point3D p2 = new Point3D(1.0, 0.0, 0.0);
            Point3D p3 = new Point3D(0.0, 1.0, 0.0);
            Point3D p4 = new Point3D(2.0, 0.0, 0.0);

            Vector3D? cross = p.CrossProductVector(p2, p3);

            Assert.That(cross, Is.Not.Null);
            Assert.That(cross!.GetLength(), Is.EqualTo(1.0).Within(1e-10));

            Assert.IsFalse(p.AreColinear(p2, p3));
            Assert.IsTrue(p.AreColinear(p2, p4));
        }

        [Test]
        public void Test_EQ_OnMeanOnly()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                10.0, 1.0, 2.0,
                1.0, 20.0, 3.0,
                2.0, 3.0, 30.0);

            Point3D q = new Point3D(1.0, 2.0, 3.0);

            Assert.IsTrue(p.EQ(q));
            Assert.IsTrue(p.EQ(q, 1e-12));
        }

        [Test]
        public void Test_Realize_Invalid_ReturnsNull()
        {
            GaussianPoint3D p = new GaussianPoint3D();

            Point3D? sample = p.Realize();

            Assert.That(sample, Is.Null);
        }

        [Test]
        public void Test_RealizeIndependent_Invalid_ReturnsNull()
        {
            GaussianPoint3D p = new GaussianPoint3D();

            Point3D? sample = p.RealizeIndependent();

            Assert.That(sample, Is.Null);
        }

        [Test]
        public void Test_RealizeIndependent_NonIndependent_ReturnsNull()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                1.0, 0.5, 0.0,
                0.5, 1.0, 0.0,
                0.0, 0.0, 1.0);

            Point3D? sample = p.RealizeIndependent();

            Assert.That(sample, Is.Null);
        }

        [Test]
        public void Test_Realize_GeneralCase_ReturnsSample()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                4.0, 0.2, 0.1,
                0.2, 5.0, 0.3,
                0.1, 0.3, 6.0);

            Point3D? sample = p.Realize();

            Assert.That(sample, Is.Not.Null);
            Assert.That(sample!.X, Is.Not.Null);
            Assert.That(sample.Y, Is.Not.Null);
            Assert.That(sample.Z, Is.Not.Null);
        }

        [Test]
        public void Test_RealizeIndependent_IndependentCase_ReturnsSample()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                1.0, 2.0, 3.0,
                4.0, 0.0, 0.0,
                0.0, 5.0, 0.0,
                0.0, 0.0, 6.0);

            Point3D? sample = p.RealizeIndependent();

            Assert.That(sample, Is.Not.Null);
            Assert.That(sample!.X, Is.Not.Null);
            Assert.That(sample.Y, Is.Not.Null);
            Assert.That(sample.Z, Is.Not.Null);
        }

        [Test]
        public void Test_Realize_SampleMean_CloseToTargetMean()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                10.0, -5.0, 3.0,
                4.0, 0.8, 0.1,
                0.8, 9.0, 0.3,
                0.1, 0.3, 16.0);

            const int n = 4000;
            double sx = 0.0;
            double sy = 0.0;
            double sz = 0.0;

            for (int i = 0; i < n; i++)
            {
                Point3D? s = p.Realize();
                Assert.That(s, Is.Not.Null);
                sx += s!.X!.Value;
                sy += s.Y!.Value;
                sz += s.Z!.Value;
            }

            double mx = sx / n;
            double my = sy / n;
            double mz = sz / n;

            Assert.That(mx, Is.EqualTo(10.0).Within(0.20));
            Assert.That(my, Is.EqualTo(-5.0).Within(0.25));
            Assert.That(mz, Is.EqualTo(3.0).Within(0.30));
        }

        [Test]
        public void Test_RealizeIndependent_SampleMean_CloseToTargetMean()
        {
            GaussianPoint3D p = new GaussianPoint3D(
                2.0, 4.0, 6.0,
                1.5, 0.0, 0.0,
                0.0, 2.5, 0.0,
                0.0, 0.0, 3.5);

            const int n = 4000;
            double sx = 0.0;
            double sy = 0.0;
            double sz = 0.0;

            for (int i = 0; i < n; i++)
            {
                Point3D? s = p.RealizeIndependent();
                Assert.That(s, Is.Not.Null);
                sx += s!.X!.Value;
                sy += s.Y!.Value;
                sz += s.Z!.Value;
            }

            double mx = sx / n;
            double my = sy / n;
            double mz = sz / n;

            Assert.That(mx, Is.EqualTo(2.0).Within(0.15));
            Assert.That(my, Is.EqualTo(4.0).Within(0.20));
            Assert.That(mz, Is.EqualTo(6.0).Within(0.25));
        }
    }
}