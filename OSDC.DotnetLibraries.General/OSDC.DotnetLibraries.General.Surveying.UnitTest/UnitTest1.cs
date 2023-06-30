using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Surveying.UnitTest
{
    public class Tests
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
            ICurvilinear3D result = new Survey();
            sv1.InterpolateAtAbscissa(sv2, 0, ref result);
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
            ICurvilinear3D result = new Survey();
            sv1.InterpolateAtAbscissa(sv2, sv2.Abscissa.Value, ref result);
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
            ICurvilinear3D result = new Survey();
            sv1.InterpolateAtAbscissa(sv2, sv2.Abscissa.Value/2.0, ref result);
            Assert.AreEqual(sv2.Abscissa.Value/2.0, result.Abscissa.Value, acc);
            Assert.AreEqual(sv2.Inclination.Value, result.Inclination.Value, acc);
            Assert.AreEqual(sv2.Azimuth.Value, result.Azimuth.Value, acc);
            Assert.AreEqual(sv2.X.Value, result.X.Value, acc);
            Assert.AreEqual(sv2.Y.Value, result.Y.Value, acc);
            Assert.AreEqual(sv2.Z.Value/2.0, result.Z.Value, acc);
        }
    }
}