using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Surveying.UnitTest
{
    public class SurveyTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            double acc = 1e-6;
            Survey sv1 = new Survey() { Abscissa = 0, Inclination = 0, Azimuth = 0, X=0, Y=0, Z=0 };
            Survey sv2 = new Survey() { Abscissa = 10, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0 };
            sv1.CompleteSIA(sv2);
            Survey result = new Survey();
            sv1.InterpolateAtAbscissa(sv2, 0, result);
            Assert.AreEqual(sv1.Abscissa.Value, result.Abscissa.Value, acc);
            Assert.AreEqual(sv1.Inclination.Value, result.Inclination.Value, acc);
            Assert.AreEqual(sv1.Azimuth.Value, result.Azimuth.Value, acc);
            Assert.AreEqual(sv1.X.Value, result.X.Value, acc);
            Assert.AreEqual(sv1.Y.Value, result.Y.Value, acc);
            Assert.AreEqual(sv1.Z.Value, result.Z.Value, acc);
        }
        [Test]
        public void Test2()
        {
            double acc = 1e-6;
            Survey sv1 = new Survey() { Abscissa = 0, Inclination = 0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
            Survey sv2 = new Survey() { Abscissa = 10, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0 };
            sv1.CompleteSIA(sv2);
            Survey result = new Survey();
            sv1.InterpolateAtAbscissa(sv2, sv2.Abscissa.Value, result);
            Assert.AreEqual(sv2.Abscissa.Value, result.Abscissa.Value, acc);
            Assert.AreEqual(sv2.Inclination.Value, result.Inclination.Value, acc);
            Assert.AreEqual(sv2.Azimuth.Value, result.Azimuth.Value, acc);
            Assert.AreEqual(sv2.X.Value, result.X.Value, acc);
            Assert.AreEqual(sv2.Y.Value, result.Y.Value, acc);
            Assert.AreEqual(sv2.Z.Value, result.Z.Value, acc);
        }
        [Test]
        public void Test3()
        {
            double acc = 1e-6;
            Survey sv1 = new Survey() { Abscissa = 0, Inclination = 0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
            Survey sv2 = new Survey() { Abscissa = 10, Inclination = 0, Azimuth = 0 };
            sv1.CompleteSIA(sv2);
            Survey result = new Survey();
            sv1.InterpolateAtAbscissa(sv2, sv2.Abscissa.Value/2.0, result);
            Assert.AreEqual(sv2.Abscissa.Value/2.0, result.Abscissa.Value, acc);
            Assert.AreEqual(sv2.Inclination.Value, result.Inclination.Value, acc);
            Assert.AreEqual(sv2.Azimuth.Value, result.Azimuth.Value, acc);
            Assert.AreEqual(sv2.X.Value, result.X.Value, acc);
            Assert.AreEqual(sv2.Y.Value, result.Y.Value, acc);
            Assert.AreEqual(sv2.Z.Value/2.0, result.Z.Value, acc);
        }

        [Test]
        public void Test4()
        {
            Survey sv1 = new Survey() { Latitude = 0, Longitude = 0, Z = 0 };
            Assert.AreEqual(0, sv1.X, 1e-3);
            Assert.AreEqual(0, sv1.Y, 1e-3);
        }

        [Test]
        public void Test5()
        {
            Survey sv1 = new Survey() { X = 0, Y = 0, Z = 0 };
            Assert.AreEqual(0, sv1.Latitude, 1e-5);
            Assert.AreEqual(0, sv1.Longitude, 1e-5);
        }

        [Test]
        public void Test6()
        {
            Survey sv1 = new Survey() { Latitude = 58.93438 * System.Math.PI / 180.0, Longitude = 5.70725 * System.Math.PI / 180.0, Z = 0 };
            Assert.AreEqual(6560503.2546896785, sv1.X, 1e-3);
            Assert.AreEqual(635328.16382991057, sv1.Y, 1e-3);
        }

        [Test]
        public void Test7()
        {
            Survey sv1 = new Survey() { X= 6560503.255, Y = 635328.164, Z = 0 };
            Assert.AreEqual(58.93438 * System.Math.PI / 180.0, sv1.Latitude, 1e-3);
            Assert.AreEqual(5.70725 * System.Math.PI / 180.0, sv1.Longitude, 1e-3);
        }


        [Test]
        public void Test8()
        {
            Survey sv = new Survey() { Z = 0, Latitude = 0, Longitude = 0 };
            SphericalPoint3D sphericalPoint3D = sv.GetSphericalPoint();
            Assert.NotNull(sphericalPoint3D);
            Assert.NotNull(sphericalPoint3D.X);
            Assert.NotNull(sphericalPoint3D.Y);
            Assert.NotNull(sphericalPoint3D.Z);
            Assert.AreEqual(General.Common.Constants.EarthSemiMajorAxisWGS84, sphericalPoint3D.X, 1e-6);
            Assert.AreEqual(0, sphericalPoint3D.Y, 1e-6);
            Assert.AreEqual(0, sphericalPoint3D.Z, 1e-6);
        }

        [Test]
        public void Test9()
        {
            Survey sv = new Survey() { Z = 0, Latitude = 0, Longitude = System.Math.PI/2.0 };
            SphericalPoint3D sphericalPoint3D = sv.GetSphericalPoint();
            Assert.NotNull(sphericalPoint3D);
            Assert.NotNull(sphericalPoint3D.X);
            Assert.NotNull(sphericalPoint3D.Y);
            Assert.NotNull(sphericalPoint3D.Z);
            Assert.AreEqual(0, sphericalPoint3D.X, 1e-6);
            Assert.AreEqual(General.Common.Constants.EarthSemiMajorAxisWGS84, sphericalPoint3D.Y, 1e-6);
            Assert.AreEqual(0, sphericalPoint3D.Z, 1e-6);
        }

    }
}