using MathNet.Numerics.LinearAlgebra.Factorization;
using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Surveying.UnitTest
{
    public class CompleteTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            double acc = 1e-6;
            SurveyPoint sv1 = new SurveyPoint() { Abscissa = 0, Inclination = 0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Abscissa = 10, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0 };
            bool ok = sv1.CompleteFromSIA(sv2);
            Assert.IsTrue(ok);
            Assert.NotNull(sv2.Curvature);
            SurveyPoint sv4 = new SurveyPoint();
            ok = sv1.InterpolateAtAbscissa(sv2, sv1.Abscissa.Value + 2.0 * SurveyPoint.InterpolationDeltaAbscissa, sv4);
            Assert.IsTrue(ok);
            Assert.NotNull(sv4.Toolface);
            SurveyPoint sv3 = new SurveyPoint() { Abscissa = sv2.Abscissa };
            ok = sv1.CompleteCASDT(sv3, sv2.Curvature.Value, sv4.Toolface.Value);
            Assert.IsTrue(ok);
            Assert.NotNull(sv2.Inclination);
            Assert.NotNull(sv3.Inclination);
            Assert.AreEqual(sv2.Inclination.Value, sv3.Inclination.Value, acc);
            Assert.NotNull(sv2.Azimuth);
            Assert.NotNull(sv3.Azimuth);
            Assert.AreEqual(sv2.Azimuth.Value, sv3.Azimuth.Value, acc);
            Assert.NotNull(sv2.X);
            Assert.NotNull(sv3.X);
            Assert.AreEqual(sv2.X.Value, sv3.X.Value, acc);
            Assert.NotNull(sv2.Y);
            Assert.NotNull(sv3.Y);
            Assert.AreEqual(sv2.Y.Value, sv3.Y.Value, acc);
            Assert.NotNull(sv2.Z);
            Assert.NotNull(sv3.Z);
            Assert.AreEqual(sv2.Z.Value, sv3.Z.Value, acc);
        }

        [Test]
        public void Test2()
        {
            double acc = 1e-6;
            SurveyPoint sv1 = new SurveyPoint() { Abscissa = 0, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Abscissa = 10, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0 };
            bool ok = sv1.CompleteFromSIA(sv2);
            Assert.IsTrue(ok);
            Assert.NotNull(sv2.Curvature);
            SurveyPoint sv4 = new SurveyPoint();
            ok = sv1.InterpolateAtAbscissa(sv2, sv1.Abscissa.Value + 2.0 * SurveyPoint.InterpolationDeltaAbscissa, sv4);
            Assert.IsTrue(ok);
            Assert.NotNull(sv4.Toolface);
            SurveyPoint sv3 = new SurveyPoint() { Abscissa = sv2.Abscissa };
            ok = sv1.CompleteCASDT(sv3, sv2.Curvature.Value, sv4.Toolface.Value);
            Assert.IsTrue(ok);
            Assert.NotNull(sv2.Inclination);
            Assert.NotNull(sv3.Inclination);
            Assert.AreEqual(sv2.Inclination.Value, sv3.Inclination.Value, acc);
            Assert.NotNull(sv2.Azimuth);
            Assert.NotNull(sv3.Azimuth);
            Assert.AreEqual(sv2.Azimuth.Value, sv3.Azimuth.Value, acc);
            Assert.NotNull(sv2.X);
            Assert.NotNull(sv3.X);
            Assert.AreEqual(sv2.X.Value, sv3.X.Value, acc);
            Assert.NotNull(sv2.Y);
            Assert.NotNull(sv3.Y);
            Assert.AreEqual(sv2.Y.Value, sv3.Y.Value, acc);
            Assert.NotNull(sv2.Z);
            Assert.NotNull(sv3.Z);
            Assert.AreEqual(sv2.Z.Value, sv3.Z.Value, acc);
        }

        [Test]
        public void Test3()
        {
            double acc = 1e-6;
            SurveyPoint sv1 = new SurveyPoint() { Abscissa = 0, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Abscissa = 10, Inclination = 2.0 * System.Math.PI / 180.0, Azimuth = 0 };
            bool ok = sv1.CompleteFromSIA(sv2);
            Assert.IsTrue(ok);
            Assert.NotNull(sv2.Curvature);
            SurveyPoint sv4 = new SurveyPoint();
            ok = sv1.InterpolateAtAbscissa(sv2, sv1.Abscissa.Value + 2.0 * SurveyPoint.InterpolationDeltaAbscissa, sv4);
            Assert.IsTrue(ok);
            Assert.NotNull(sv4.Toolface);
            SurveyPoint sv3 = new SurveyPoint() { Abscissa = sv2.Abscissa };
            ok = sv1.CompleteCASDT(sv3, sv2.Curvature.Value, sv4.Toolface.Value);
            Assert.IsTrue(ok);
            Assert.NotNull(sv2.Inclination);
            Assert.NotNull(sv3.Inclination);
            Assert.AreEqual(sv2.Inclination.Value, sv3.Inclination.Value, acc);
            Assert.NotNull(sv2.Azimuth);
            Assert.NotNull(sv3.Azimuth);
            Assert.AreEqual(sv2.Azimuth.Value, sv3.Azimuth.Value, acc);
            Assert.NotNull(sv2.X);
            Assert.NotNull(sv3.X);
            Assert.AreEqual(sv2.X.Value, sv3.X.Value, acc);
            Assert.NotNull(sv2.Y);
            Assert.NotNull(sv3.Y);
            Assert.AreEqual(sv2.Y.Value, sv3.Y.Value, acc);
            Assert.NotNull(sv2.Z);
            Assert.NotNull(sv3.Z);
            Assert.AreEqual(sv2.Z.Value, sv3.Z.Value, acc);
        }
        [Test]
        public void Test4()
        {
            double acc1 = 1e-5;
            double acc2 = 1e-4;
            SurveyPoint sv1 = new SurveyPoint() { Abscissa = 0, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Abscissa = 10, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 1.0 * System.Math.PI / 180.0 };
            bool ok = sv1.CompleteFromSIA(sv2);
            Assert.IsTrue(ok);
            Assert.NotNull(sv2.Curvature);
            SurveyPoint sv4 = new SurveyPoint();
            ok = sv1.InterpolateAtAbscissa(sv2, sv1.Abscissa.Value + 2.0 * SurveyPoint.InterpolationDeltaAbscissa, sv4);
            Assert.IsTrue(ok);
            Assert.NotNull(sv4.Toolface);
            SurveyPoint sv3 = new SurveyPoint() { Abscissa = sv2.Abscissa };
            ok = sv1.CompleteCASDT(sv3, sv2.Curvature.Value, sv4.Toolface.Value);
            Assert.IsTrue(ok);
            Assert.NotNull(sv2.Inclination);
            Assert.NotNull(sv3.Inclination);
            Assert.AreEqual(sv2.Inclination.Value, sv3.Inclination.Value, acc1);
            Assert.NotNull(sv2.Azimuth);
            Assert.NotNull(sv3.Azimuth);
            Assert.AreEqual(sv2.Azimuth.Value, sv3.Azimuth.Value, acc1);
            Assert.NotNull(sv2.X);
            Assert.NotNull(sv3.X);
            Assert.AreEqual(sv2.X.Value, sv3.X.Value, acc2);
            Assert.NotNull(sv2.Y);
            Assert.NotNull(sv3.Y);
            Assert.AreEqual(sv2.Y.Value, sv3.Y.Value, acc2);
            Assert.NotNull(sv2.Z);
            Assert.NotNull(sv3.Z);
            Assert.AreEqual(sv2.Z.Value, sv3.Z.Value, acc2);
        }
        [Test]
        public void Test5()
        {
            double acc1 = 2e-5;
            double acc2 = 1e-4;
            SurveyPoint sv1 = new SurveyPoint() { Abscissa = 0, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Abscissa = 10, Inclination = 2.0 * System.Math.PI / 180.0, Azimuth = 1.0 * System.Math.PI / 180.0 };
            bool ok = sv1.CompleteFromSIA(sv2);
            Assert.IsTrue(ok);
            Assert.NotNull(sv2.Curvature);
            SurveyPoint sv4 = new SurveyPoint();
            ok = sv1.InterpolateAtAbscissa(sv2, sv1.Abscissa.Value + 2.0 * SurveyPoint.InterpolationDeltaAbscissa, sv4);
            Assert.IsTrue(ok);
            Assert.NotNull(sv4.Toolface);
            SurveyPoint sv3 = new SurveyPoint() { Abscissa = sv2.Abscissa };
            ok = sv1.CompleteCASDT(sv3, sv2.Curvature.Value, sv4.Toolface.Value);
            Assert.IsTrue(ok);
            Assert.NotNull(sv2.Inclination);
            Assert.NotNull(sv3.Inclination);
            Assert.AreEqual(sv2.Inclination.Value, sv3.Inclination.Value, acc1);
            Assert.NotNull(sv2.Azimuth);
            Assert.NotNull(sv3.Azimuth);
            Assert.AreEqual(sv2.Azimuth.Value, sv3.Azimuth.Value, acc1);
            Assert.NotNull(sv2.X);
            Assert.NotNull(sv3.X);
            Assert.AreEqual(sv2.X.Value, sv3.X.Value, acc2);
            Assert.NotNull(sv2.Y);
            Assert.NotNull(sv3.Y);
            Assert.AreEqual(sv2.Y.Value, sv3.Y.Value, acc2);
            Assert.NotNull(sv2.Z);
            Assert.NotNull(sv3.Z);
            Assert.AreEqual(sv2.Z.Value, sv3.Z.Value, acc2);
        }

        [Test]
        public void Test6()
        {
            double acc1 = 2e-5;
            double acc2 = 1e-4;
            SurveyPoint sv1 = new SurveyPoint() { Abscissa = 0, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Abscissa = 10, Inclination = 2.0 * System.Math.PI / 180.0, Azimuth = 1.0 * System.Math.PI / 180.0 };
            bool ok = sv1.CompleteFromSIA(sv2);
            Assert.IsTrue(ok);
            Assert.NotNull(sv2.Curvature);
            SurveyPoint sv4 = new SurveyPoint();
            ok = sv1.InterpolateAtAbscissa(sv2, sv1.Abscissa.Value + 2.0 * SurveyPoint.InterpolationDeltaAbscissa, sv4);
            Assert.IsTrue(ok);
            Assert.NotNull(sv4.Toolface);
            SurveyPoint sv3 = new SurveyPoint() { Abscissa = sv2.Abscissa };
            ok = sv1.CompleteCDTSDT1(sv3, sv2.Curvature.Value, sv4.Toolface.Value);
            Assert.IsTrue(ok);
            Assert.NotNull(sv3.Toolface);
            Assert.AreEqual(sv4.Toolface.Value, sv3.Toolface.Value, acc1);
            
        }

        [Test]
        public void Test7()
        {
            double acc1 = 2e-5;
            double acc2 = 1e-4;
            SurveyPoint sv1 = new SurveyPoint() { Abscissa = 0, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
            SurveyPoint sv2 = new SurveyPoint() { Abscissa = 100, Inclination = 20.0 * System.Math.PI / 180.0, Azimuth = 10.0 * System.Math.PI / 180.0 };
            bool ok = sv1.CompleteFromSIA(sv2);
            Assert.IsTrue(ok);
            Assert.NotNull(sv2.Curvature);
            SurveyPoint sv4 = new SurveyPoint();
            ok = sv1.InterpolateAtAbscissa(sv2, sv1.Abscissa.Value + 2.0 * SurveyPoint.InterpolationDeltaAbscissa, sv4);
            Assert.IsTrue(ok);
            Assert.NotNull(sv4.Toolface);
            SurveyPoint sv3 = new SurveyPoint() { Abscissa = sv2.Abscissa };
            ok = sv1.CompleteCDTSDT1(sv3, sv2.Curvature.Value, sv4.Toolface.Value);
            Assert.IsTrue(ok);
            Assert.NotNull(sv3.Toolface);
            Assert.AreEqual(sv4.Toolface.Value, sv3.Toolface.Value, acc1);

        }
    }
}
