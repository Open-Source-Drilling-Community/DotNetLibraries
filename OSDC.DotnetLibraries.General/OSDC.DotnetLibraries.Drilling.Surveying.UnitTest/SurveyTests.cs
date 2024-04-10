using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using OSDC.DotnetLibraries.General.Common;
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
            //Survey sv1 = new Survey() { Latitude = 58.93438 * System.Math.PI / 180.0, Longitude = 5.70725 * System.Math.PI / 180.0, Z = 0 };
            Survey sv1 = new Survey() { Latitude = 58.93438 * System.Math.PI / 180.0, Longitude = 5 * System.Math.PI / 180.0, Z = 0 }; 
            Survey sv2 = new Survey() { Latitude = sv1.Latitude, Longitude = 0, Z = 0 };
            double? distEast = sv2.Riemannian2DDistance(sv1.Latitude, sv1.Longitude);
            Survey sv3 = new Survey() { Latitude = 0, Longitude = sv1.Longitude, Z = 0 };
            double? distNorth = sv3.Riemannian2DDistance(sv1.Latitude, sv1.Longitude);
            Assert.NotNull(distEast);
            Assert.NotNull(distNorth);
            //Assert.AreEqual(distNorth.Value, sv1.X, 1e-3);
            Assert.AreEqual(distEast.Value, sv1.Y, 1e-3);
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

        [Test]
        public void Test10()
        {
            double lat1 = 0;
            double lat2 = 0;
            double lon1 = 0;
            double lon2 = 0;
            Survey sv1 = new Survey() { Latitude = lat1, Longitude = lon1, TVD = 0 };
            double? dist = sv1.Riemannian2DDistance(lat2, lon2);
            Assert.NotNull(dist);
            Assert.AreEqual(0, dist, 1e-4);
        }
        [Test]
        public void Test11()
        {
            double lat1 = 0;
            double lat2 = 0;
            double lon1 = 0;
            double lon2 = 1.0 * Numeric.PI / 180.0;
            Survey sv1 = new Survey() { Latitude = lat1, Longitude = lon1, TVD = 0 };
            double? dist = sv1.Riemannian2DDistance(lat2, lon2);
            double R = Constants.EarthSemiMajorAxisWGS84;
            double arc = R * (lon2 - lon1);
            Assert.NotNull(dist);
            Assert.AreEqual(arc, dist, 1e-4);
        }
        [Test]
        public void Test12()
        {
            double lat1 = 75.0 * Numeric.PI / 180.0;
            double lat2 = lat1;
            double lon1 = 0;
            double lon2 = 1.0 * Numeric.PI / 180.0;
            Survey sv1 = new Survey() { Latitude = lat1, Longitude =lon1, TVD = 0 };
            Survey sv2 = new Survey() { Latitude = lat2, Longitude =lon2, TVD = 0 };
            double? dist = sv1.Riemannian2DDistance(lat2, lon2);
            double a = Constants.EarthSemiMajorAxisWGS84;
            double f = 1.0 / Constants.EarthInverseFlateningWGS84;
            double b = a - a * f;
            double sinLat = System.Math.Sin(lat1);
            double e2 = (a * a - b * b) / (a * a);
            double R = a / System.Math.Sqrt(1 - e2 * sinLat*sinLat);
            R = System.Math.Cos(lat1) * R;
            double arc = R * (lon2-lon1);
            Assert.NotNull(dist);
            Assert.NotNull(sv2.RiemannianEast);
            Assert.NotNull(sv1.RiemannianEast);
            Assert.AreEqual(sv2.RiemannianEast.Value-sv1.RiemannianEast.Value, dist, 0.75);
        }
        [Test]
        public void Test13()
        {
            double lat1 = 0.0 * Numeric.PI / 180.0;
            double lat2 = 1.0 * Numeric.PI / 180.0;
            double lon1 = 0;
            double lon2 = 0;
            Survey sv1 = new Survey() { Latitude = lat1, Longitude = lon1, TVD = 0 };
            Survey sv2 = new Survey() { Latitude = lat2, Longitude = lon2, TVD = 0 };
            double? dist = sv1.Riemannian2DDistance(lat2, lon2);
            Assert.NotNull(dist);
            Assert.NotNull(sv2.RiemannianNorth);
            Assert.NotNull(sv1.RiemannianNorth);
            Assert.AreEqual(sv2.RiemannianNorth.Value- sv1.RiemannianNorth.Value, dist, 0.75);
        }

    }
}