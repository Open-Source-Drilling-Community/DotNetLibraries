using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math.UnitTest
{
    public class QuadraticPolynomTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, -3.0, 2.0);
            double root1 = 0;
            double root2 = 0;
            int count = poly.FindRoots(ref root1, ref root2);
            if (root1 > root2)
            {
                double tmp = root1;
                root1 = root2;
                root2 = tmp;
            }
            Assert.AreEqual(2, count);
            Assert.AreEqual(1.0, root1);
            Assert.AreEqual(2.0, root2);
            Assert.Pass();
        }
        [Test]
        public void Test2()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, -2.0, 1.0);
            double root1 = 0;
            double root2 = 0;
            int count = poly.FindRoots(ref root1, ref root2);
            Assert.AreEqual(2, count);
            Assert.AreEqual(1.0, root1);
            Assert.AreEqual(1.0, root2);
            Assert.Pass();
        }
        [Test]
        public void Test3()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, -3.0, 2.0);
            Complex root1 = new Complex();
            Complex root2 = new Complex();
            int count = poly.FindRoots(ref root1, ref root2);
            if (root1.Modulus > root2.Modulus)
            {
                Complex tmp = root1;
                root1 = root2;
                root2 = tmp;
            }
            Assert.AreEqual(2, count);
            Assert.AreEqual(1.0, root1.Real);
            Assert.AreEqual(0.0, root1.Imaginary);
            Assert.AreEqual(2.0, root2.Real);
            Assert.AreEqual(0.0, root2.Imaginary);
            Assert.Pass();
        }
        [Test]
        public void Test4()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, -2.0, 1.0);
            Complex root1 = new Complex();
            Complex root2 = new Complex();
            int count = poly.FindRoots(ref root1, ref root2);
            Assert.AreEqual(2, count);
            Assert.AreEqual(1.0, root1.Real);
            Assert.AreEqual(0.0, root1.Imaginary);
            Assert.AreEqual(1.0, root2.Real);
            Assert.AreEqual(0.0, root2.Imaginary);
            Assert.Pass();
        }
        [Test]
        public void Test5()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, -3.0, 2.0);
            IPolynom derivative = new LinearPolynom();
            poly.Derivate(ref derivative);
            Assert.AreEqual(-3.0, derivative[0]);
            Assert.AreEqual(2.0, derivative[1]);
            Assert.Pass();
        }
        [Test]
        public void Test6()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, -3.0, 2.0);
            IPolynom derivative = new QuadraticPolynom();
            poly.Derivate(ref derivative);
            Assert.AreEqual(-3.0, derivative[0]);
            Assert.AreEqual(2.0, derivative[1]);
            Assert.AreEqual(0.0, derivative[2]);
            Assert.Pass();
        }
        [Test]
        public void Test7()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, -3.0, 2.0);
            IPolynom derivative = new CubicPolynom();
            poly.Derivate(ref derivative);
            Assert.AreEqual(-3.0, derivative[0]);
            Assert.AreEqual(2.0, derivative[1]);
            Assert.AreEqual(0.0, derivative[2]);
            Assert.AreEqual(0.0, derivative[3]);
            Assert.Pass();
        }
        [Test]
        public void Test8()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, -3.0, 2.0);
            IPolynom derivative = new QuarticPolynom();
            poly.Derivate(ref derivative);
            Assert.AreEqual(-3.0, derivative[0]);
            Assert.AreEqual(2.0, derivative[1]);
            Assert.AreEqual(0.0, derivative[2]);
            Assert.AreEqual(0.0, derivative[3]);
            Assert.AreEqual(0.0, derivative[4]);
            Assert.Pass();
        }

        [Test]
        public void Test9()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, -3.0, 2.0);
            IPolynom primitive = new CubicPolynom();
            poly.Primitive(ref primitive);
            Assert.AreEqual(0.0, primitive[0]);
            Assert.AreEqual(2.0, primitive[1]);
            Assert.AreEqual(-1.5, primitive[2]);
            Assert.AreEqual(1.0/3.0, primitive[3]);
            Assert.Pass();
        }
        [Test]
        public void Test10()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, -3.0, 2.0);
            IPolynom primitive = new QuarticPolynom();
            poly.Primitive(ref primitive);
            Assert.AreEqual(0.0, primitive[0]);
            Assert.AreEqual(2.0, primitive[1]);
            Assert.AreEqual(-1.5, primitive[2]);
            Assert.AreEqual(1.0 / 3.0, primitive[3]);
            Assert.AreEqual(0.0, primitive[4]);
            Assert.Pass();
        }
        [Test]
        public void Test11()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, -3.0, 2.0);
            IPolynom primitive = new CubicPolynom();
            poly.Primitive(ref primitive);
            double a = 1.0;
            double b = 2.0;
            double integral1 = primitive.Eval(b)- primitive.Eval(a);
            double integral2 = poly.Integrate(a, b);
            Assert.AreEqual(integral1, integral2, 1e-6);
            Assert.Pass();
        }

        [Test]
        public void Test12()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, 0.0, -1.0);
            double root1 = 0;
            double root2 = 0;
            int count = poly.FindRoots(ref root1, ref root2);
            if (root1 > root2)
            {
                double tmp = root1;
                root1 = root2;
                root2 = tmp;
            }
            Assert.AreEqual(2, count);
            Assert.AreEqual(-1.0, root1);
            Assert.AreEqual(1.0, root2);
            Assert.Pass();
        }
        [Test]
        public void Test13()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, 0.0, 1.0);
            double root1 = 0;
            double root2 = 0;
            int count = poly.FindRoots(ref root1, ref root2);
            Assert.AreEqual(0, count);
            Assert.Pass();
        }
        [Test]
        public void Test14()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, 0.0, 1.0);
            Complex root1 = new Complex();
            Complex root2 = new Complex();
            int count = poly.FindRoots(ref root1, ref root2);
            Assert.AreEqual(2, count);
            Assert.AreEqual(0.0, root1.Real);
            Assert.AreEqual(1.0, root1.Imaginary);
            Assert.AreEqual(0.0, root2.Real);
            Assert.AreEqual(-1.0, root2.Imaginary);
            Assert.Pass();
        }
        [Test]
        public void Test15()
        {
            QuadraticPolynom poly = new QuadraticPolynom(1.0, 0.0, 0.0);
            double root1 = 0;
            double root2 = 0;
            int count = poly.FindRoots(ref root1, ref root2);
            if (root1 > root2)
            {
                double tmp = root1;
                root1 = root2;
                root2 = tmp;
            }
            Assert.AreEqual(2, count);
            Assert.AreEqual(0.0, root1);
            Assert.AreEqual(0.0, root2);
            Assert.Pass();
        }
        [Test]
        public void Test16()
        {
            QuadraticPolynom cpoly = new QuadraticPolynom(1.0, -3.0, 2.0);
            Polynom poly = new Polynom(cpoly);
            Assert.AreEqual(cpoly.Degree, poly.Degree);
            Assert.AreEqual(cpoly[0], poly[0]);
            Assert.AreEqual(cpoly[1], poly[1]);
            Assert.AreEqual(cpoly[2], poly[2]);
            double x = 0;
            double eval1 = cpoly.Eval(x);
            double eval2 = poly.Eval(x);
            Assert.AreEqual(eval1, eval2);
            x = 1.0;
            eval1 = cpoly.Eval(x);
            eval2 = poly.Eval(x);
            Assert.AreEqual(eval1, eval2);
            Assert.Pass();
        }

        [Test]
        public void Test17()
        {
            QuadraticPolynom cpoly = new QuadraticPolynom(1.0, -3.0, 2.0);
            Polynom poly = new Polynom(cpoly);
            IPolynom derivative1 = new LinearPolynom();
            IPolynom derivative2 = new LinearPolynom();
            cpoly.Derivate(ref derivative1);
            poly.Derivate(ref derivative2);
            Assert.AreEqual(derivative1[0], derivative2[0]);
            Assert.AreEqual(derivative1[1], derivative2[1]);
            Assert.Pass();
        }
        [Test]
        public void Test18()
        {
            QuadraticPolynom cpoly = new QuadraticPolynom(1.0, -3.0, 2.0);
            Polynom poly = new Polynom(cpoly);
            IPolynom derivative1 = new Polynom(1);
            IPolynom derivative2 = new Polynom(1);
            cpoly.Derivate(ref derivative1);
            poly.Derivate(ref derivative2);
            Assert.AreEqual(derivative1[0], derivative2[0]);
            Assert.AreEqual(derivative1[1], derivative2[1]);
            Assert.Pass();
        }
        [Test]
        public void Test19()
        {
            QuadraticPolynom cpoly = new QuadraticPolynom(1.0, -3.0, 2.0);
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
        public void Test20()
        {
            QuadraticPolynom cpoly = new QuadraticPolynom(1.0, -3.0, 2.0);
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
        public void Test21()
        {
            QuadraticPolynom cpoly = new QuadraticPolynom(1.0, -3.0, 2.0);
            Polynom poly = new Polynom(cpoly);
            IPolynom primitive1 = new CubicPolynom();
            IPolynom primitive2 = new CubicPolynom();
            cpoly.Primitive(ref primitive1);
            poly.Primitive(ref primitive2);
            Assert.AreEqual(primitive1[0], primitive2[0]);
            Assert.AreEqual(primitive1[1], primitive2[1]);
            Assert.AreEqual(primitive1[2], primitive2[2]);
            Assert.AreEqual(primitive1[3], primitive2[3]);
            Assert.Pass();
        }
        [Test]
        public void Test22()
        {
            QuadraticPolynom cpoly = new QuadraticPolynom(1.0, -3.0, 2.0);
            Polynom poly = new Polynom(cpoly);
            IPolynom primitive1 = new Polynom(3);
            IPolynom primitive2 = new Polynom(3);
            cpoly.Primitive(ref primitive1);
            poly.Primitive(ref primitive2);
            Assert.AreEqual(primitive1[0], primitive2[0]);
            Assert.AreEqual(primitive1[1], primitive2[1]);
            Assert.AreEqual(primitive1[2], primitive2[2]);
            Assert.AreEqual(primitive1[3], primitive2[3]);
            Assert.Pass();
        }
        [Test]
        public void Test23()
        {
            QuadraticPolynom cpoly = new QuadraticPolynom(1.0, -3.0, 2.0);
            Polynom poly = new Polynom(cpoly);
            double a = 1.0;
            double b = 2.0;
            double integral1 = cpoly.Integrate(a, b);
            double integral2 = poly.Integrate(a, b);
            Assert.AreEqual(integral1, integral2);
            Assert.Pass();
        }

        [Test]
        public void Test24()
        {
            QuadraticPolynom cpoly = new QuadraticPolynom(1.0, -3.0, 2.0);
            Polynom poly = new Polynom(cpoly);
            List<Complex> roots = new List<Complex>();
            int count = poly.FindRoots(roots);
            Assert.AreEqual(count, 2);
            Assert.AreEqual(0.0, roots[0].Imaginary);
            Assert.AreEqual(0.0, roots[1].Imaginary);
            double eq = cpoly.Eval(roots[0].Real);
            Assert.IsTrue(Numeric.EQ(eq, 0.0));
            eq = cpoly.Eval(roots[1].Real);
            Assert.IsTrue(Numeric.EQ(eq, 0.0));
            Assert.Pass();
        }

    }
}
