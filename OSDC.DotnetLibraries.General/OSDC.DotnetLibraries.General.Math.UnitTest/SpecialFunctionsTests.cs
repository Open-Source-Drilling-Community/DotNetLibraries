using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Math.UnitTest
{
    public class SpecialFunctionsTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            for (double m = 0; m <= 1.0; m += 0.01)
            {
                double x = SpecialFunctions.EllipseE(0, m);
                Assert.AreEqual(0, x, 1e-6);
            }
        }

        [Test]
        public void Test2()
        {
            for (double a = 0; a <= System.Math.PI/2.0; a += 0.01)
            {
                double x = SpecialFunctions.EllipseE(a, 0);
                Assert.AreEqual(a, x, 1e-6);
            }
        }

        [Test]
        public void Test3()
        {
            double x1 = SpecialFunctions.EllipseE(1.5, 1);
            Assert.AreEqual(0.997495, x1, 1e-6);
            double x2 = SpecialFunctions.EllipseE(1.5, 0.5);
            Assert.AreEqual(1.40613, x2, 1e-5);
            double x3 = SpecialFunctions.EllipseE(-1.5, 1);
            Assert.AreEqual(-0.997495, x3, 1e-6);
            double x4 = SpecialFunctions.EllipseE(-1.5, 0.5);
            Assert.AreEqual(-1.40613, x4, 1e-5);
        }

        [Test]
        public void Test4()
        {
            for (double a = 0; a <= System.Math.PI / 2.0; a += 0.01)
            {
                double x = SpecialFunctions.EllipseE(a, 0);
                double a1 = SpecialFunctions.InverseEllipseE(x, 0);
                Assert.AreEqual(a, a1, 1e-6);
            }
        }

        [Test]
        public void Test5()
        {
            for (double a = 0; a <= System.Math.PI / 2.0; a += 0.01)
            {
                double x = SpecialFunctions.EllipseE(a, 0.5);
                double a1 = SpecialFunctions.InverseEllipseE(x, 0.5);
                Assert.AreEqual(a, a1, 1e-6);
            }
        }

        [Test]
        public void Test6()
        {
            double m = 0.99999;
            for (double a = 0; a <= System.Math.PI / 2.0; a += 0.01)
            {
                double x = SpecialFunctions.EllipseE(a, m);
                double a1 = SpecialFunctions.InverseEllipseE(x, m);
                Assert.AreEqual(a, a1, 1e-6);
            }
        }
    }
}
