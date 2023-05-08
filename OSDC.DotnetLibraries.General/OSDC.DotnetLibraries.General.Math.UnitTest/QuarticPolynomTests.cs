using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math.UnitTest
{
    public class QuarticPolynomTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            QuarticPolynom poly = new QuarticPolynom(1.0, -10.0, 35.0, -50.0, 24.0);
            Complex root1 = Complex.Zero;
            Complex root2 = Complex.Zero;
            Complex root3 = Complex.Zero;
            Complex root4 = Complex.Zero;
            int count = poly.FindRoots(ref root1, ref root2, ref root3, ref root4);
            Assert.AreEqual(4, count);
            Assert.AreEqual(4.0, root1.Real);
            Assert.AreEqual(0.0, root1.Imaginary);
            Assert.AreEqual(3.0, root2.Real);
            Assert.AreEqual(0.0, root2.Imaginary);
            Assert.AreEqual(1.0, root3.Real);
            Assert.AreEqual(0.0, root3.Imaginary);
            Assert.AreEqual(2.0, root4.Real);
            Assert.AreEqual(0.0, root4.Imaginary);
            Assert.Pass();
        }
        [Test]
        public void Test2()
        {
            QuarticPolynom poly = new QuarticPolynom(1.0, -7.0, 5.0, 31.0, -30.0);
            Complex root1 = Complex.Zero;
            Complex root2 = Complex.Zero;
            Complex root3 = Complex.Zero;
            Complex root4 = Complex.Zero;
            int count = poly.FindRoots(ref root1, ref root2, ref root3, ref root4);
            Assert.AreEqual(4, count);
            Assert.AreEqual(0.0, root1.Imaginary);
            Assert.AreEqual(0.0, root2.Imaginary);
            Assert.AreEqual(0.0, root3.Imaginary);
            Assert.AreEqual(0.0, root4.Imaginary);
            double eq = poly.Eval(root2.Real);
            Assert.AreEqual(0.0, eq);
            eq = poly.Eval(root2.Real);
            Assert.AreEqual(0.0, eq);
            eq = poly.Eval(root3.Real);
            Assert.AreEqual(0.0, eq);
            eq = poly.Eval(root4.Real);
            Assert.AreEqual(0.0, eq);
            Assert.Pass();
        }
        [Test]
        public void Test6()
        {
            QuarticPolynom poly = new QuarticPolynom(1.0, -10.0, 35.0, -50.0, 24.0);
            IPolynom derivative = new CubicPolynom();
            poly.Derivate(ref derivative);
            Assert.AreEqual(-50.0, derivative[0]);
            Assert.AreEqual(70.0, derivative[1]);
            Assert.AreEqual(-30.0, derivative[2]);
            Assert.AreEqual(4.0, derivative[3]);
            Assert.Pass();
        }
        [Test]
        public void Test10()
        {
            QuarticPolynom poly = new QuarticPolynom(1.0, 1.0, 1.0, 1.0, 1.0);
            IPolynom primitive = new Polynom(5);
            poly.Primitive(ref primitive);
            Assert.AreEqual(0.0, primitive[0]);
            Assert.AreEqual(1.0, primitive[1]);
            Assert.AreEqual(1.0/2.0, primitive[2]);
            Assert.AreEqual(1.0 / 3.0, primitive[3]);
            Assert.AreEqual(1.0 / 4.0, primitive[4]);
            Assert.Pass();
        }
        [Test]
        public void Test12()
        {
            QuarticPolynom poly = new QuarticPolynom(1.0, 1.0, 1.0, 1.0, 1.0);
            IPolynom primitive = new Polynom(5);
            poly.Primitive(ref primitive);
            double a = 1.0;
            double b = 2.0;
            double integral1 = primitive.Eval(b) - primitive.Eval(a);
            double integral2 = poly.Integrate(a, b);
            Assert.AreEqual(integral1, integral2, 1e-6);
            Assert.Pass();
        }
        [Test]
        public void Test16()
        {
            QuarticPolynom cpoly = new QuarticPolynom(1.0, 1.0, 1.0, 1.0, 1.0);
            Polynom poly = new Polynom(cpoly);
            double x = 0;
            double derivate1 = cpoly.Derive(x);
            double derivate2 = poly.Derive(x);
            Assert.AreEqual(derivate1, derivate2);
            x = 1.0;
            derivate1 = cpoly.Derive(x);
            derivate2 = poly.Derive(x);
            Assert.AreEqual(derivate1, derivate2);
            Assert.Pass();
        }
        [Test]
        public void Test17()
        {
            QuarticPolynom cpoly = new QuarticPolynom(1.0, 1.0, 1.0, 1.0, 1.0);
            Polynom poly = new Polynom(cpoly);
            double x = 0;
            double derivate1 = cpoly.DeriveSecond(x);
            double derivate2 = poly.DeriveSecond(x);
            Assert.AreEqual(derivate1, derivate2);
            x = 1.0;
            derivate1 = cpoly.DeriveSecond(x);
            derivate2 = poly.DeriveSecond(x);
            Assert.AreEqual(derivate1, derivate2);
            Assert.Pass();
        }

        [Test]
        public void Test19()
        {
            QuarticPolynom cpoly = new QuarticPolynom(1.0, 1.0, 1.0, 1.0, 1.0);
            Polynom poly = new Polynom(cpoly);
            IPolynom primitive1 = new Polynom(4);
            IPolynom primitive2 = new Polynom(4);
            cpoly.Primitive(ref primitive1);
            poly.Primitive(ref primitive2);
            Assert.AreEqual(primitive1[0], primitive2[0]);
            Assert.AreEqual(primitive1[1], primitive2[1]);
            Assert.AreEqual(primitive1[2], primitive2[2]);
            Assert.AreEqual(primitive1[3], primitive2[3]);
            Assert.AreEqual(primitive1[4], primitive2[4]);
            Assert.Pass();
        }
        [Test]
        public void Test20()
        {
            QuarticPolynom cpoly = new QuarticPolynom(1.0, 1.0, 1.0, 1.0, 1.0);
            Polynom poly = new Polynom(cpoly);
            double a = 1.0;
            double b = 2.0;
            double integral1 = cpoly.Integrate(a, b);
            double integral2 = poly.Integrate(a, b);
            Assert.AreEqual(integral1, integral2, 1e-6);
            Assert.Pass();
        }
        [Test]
        public void Test24()
        {
            QuarticPolynom cpoly = new QuarticPolynom(1.0, -10.0, 35.0, -50.0, 24.0);
            Polynom poly = new Polynom(cpoly);
            List<Complex> roots = new List<Complex>();
            int count = poly.FindRoots(roots);
            Assert.AreEqual(count, 4);
            Assert.AreEqual(0.0, roots[0].Imaginary);
            Assert.AreEqual(0.0, roots[1].Imaginary);
            Assert.AreEqual(0.0, roots[2].Imaginary);
            Assert.AreEqual(0.0, roots[3].Imaginary);
            double eq = cpoly.Eval(roots[0].Real);
            Assert.IsTrue(Numeric.EQ(eq, 0.0));
            eq = cpoly.Eval(roots[1].Real);
            Assert.IsTrue(Numeric.EQ(eq, 0.0));
            eq = cpoly.Eval(roots[2].Real);
            Assert.IsTrue(Numeric.EQ(eq, 0.0));
            eq = cpoly.Eval(roots[3].Real);
            Assert.IsTrue(Numeric.EQ(eq, 0.0));
            Assert.Pass();
        }

    }
}
