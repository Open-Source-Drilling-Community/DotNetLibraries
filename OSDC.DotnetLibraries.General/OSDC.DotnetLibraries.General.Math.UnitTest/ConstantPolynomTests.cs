using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Math.UnitTest
{
    public class ConstantPolynomTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            ConstantPolynom poly = new ConstantPolynom(1.0);
            double root = 0;
            int count = poly.FindRoots(ref root);
            Assert.AreEqual(0, count);
        }
        [Test]
        public void Test2()
        {
            ConstantPolynom poly = new ConstantPolynom(0.0);
            double root = 0;
            int count = poly.FindRoots(ref root);
            Assert.AreEqual(1, count);
        }
        [Test]
        public void Test3()
        {
            ConstantPolynom poly = new ConstantPolynom(1.0);
            IPolynom derivative = new ConstantPolynom();
            poly.Derivate(ref derivative);
            Assert.AreEqual(0.0, derivative[0]);
        }
        [Test]
        public void Test4()
        {
            ConstantPolynom poly = new ConstantPolynom(1.0);
            IPolynom derivative = new LinearPolynom();
            poly.Derivate(ref derivative);
            Assert.AreEqual(0.0, derivative[0]);
            Assert.AreEqual(0.0, derivative[1]);
        }
        [Test]
        public void Test5()
        {
            ConstantPolynom poly = new ConstantPolynom(1.0);
            IPolynom derivative = new QuadraticPolynom();
            poly.Derivate(ref derivative);
            Assert.AreEqual(0.0, derivative[0]);
            Assert.AreEqual(0.0, derivative[1]);
            Assert.AreEqual(0.0, derivative[2]);
        }
        [Test]
        public void Test6()
        {
            ConstantPolynom poly = new ConstantPolynom(1.0);
            IPolynom derivative = new CubicPolynom();
            poly.Derivate(ref derivative);
            Assert.AreEqual(0.0, derivative[0]);
            Assert.AreEqual(0.0, derivative[1]);
            Assert.AreEqual(0.0, derivative[2]);
            Assert.AreEqual(0.0, derivative[3]);
        }
        [Test]
        public void Test7()
        {
            ConstantPolynom poly = new ConstantPolynom(1.0);
            IPolynom derivative = new QuarticPolynom();
            poly.Derivate(ref derivative);
            Assert.AreEqual(0.0, derivative[0]);
            Assert.AreEqual(0.0, derivative[1]);
            Assert.AreEqual(0.0, derivative[2]);
            Assert.AreEqual(0.0, derivative[3]);
            Assert.AreEqual(0.0, derivative[4]);
        }
        [Test]
        public void Test8()
        {
            ConstantPolynom poly = new ConstantPolynom(1.0);
            IPolynom primitive = new LinearPolynom();
            poly.Primitive(ref primitive);
            Assert.AreEqual(0.0, primitive[0]);
            Assert.AreEqual(1.0, primitive[1]);
        }
        [Test]
        public void Test9()
        {
            ConstantPolynom poly = new ConstantPolynom(1.0);
            IPolynom primitive = new QuadraticPolynom();
            poly.Primitive(ref primitive);
            Assert.AreEqual(0.0, primitive[0]);
            Assert.AreEqual(1.0, primitive[1]);
            Assert.AreEqual(0.0, primitive[2]);
        }
        [Test]
        public void Test10()
        {
            ConstantPolynom poly = new ConstantPolynom(1.0);
            IPolynom primitive = new CubicPolynom();
            poly.Primitive(ref primitive);
            Assert.AreEqual(0.0, primitive[0]);
            Assert.AreEqual(1.0, primitive[1]);
            Assert.AreEqual(0.0, primitive[2]);
            Assert.AreEqual(0.0, primitive[3]);
        }
        [Test]
        public void Test11()
        {
            ConstantPolynom poly = new ConstantPolynom(1.0);
            IPolynom primitive = new QuarticPolynom();
            poly.Primitive(ref primitive);
            Assert.AreEqual(0.0, primitive[0]);
            Assert.AreEqual(1.0, primitive[1]);
            Assert.AreEqual(0.0, primitive[2]);
            Assert.AreEqual(0.0, primitive[3]);
            Assert.AreEqual(0.0, primitive[4]);
        }
        [Test]
        public void Test12()
        {
            ConstantPolynom poly = new ConstantPolynom(1.0);
            IPolynom primitive = new LinearPolynom();
            poly.Primitive(ref primitive);
            double a = 1.0;
            double b = 2.0;
            double integral1 = primitive.Eval(b)- primitive.Eval(a);
            double integral2 = poly.Integrate(a, b);
            Assert.AreEqual(integral1, integral2);
        }
        [Test]
        public void Test13()
        {
            ConstantPolynom cpoly = new ConstantPolynom(1.0);
            Polynom poly = new Polynom(cpoly);
            Assert.AreEqual(cpoly.Degree, poly.Degree);
            Assert.AreEqual(cpoly[0], poly[0]);
            double x = 0;
            double eval1 = cpoly.Eval(x);
            double eval2 = poly.Eval(x);
            Assert.AreEqual(eval1, eval2);
            x = 1.0;
            eval1 = cpoly.Eval(x);
            eval2 = poly.Eval(x);
            Assert.AreEqual(eval1, eval2);
        }

        [Test]
        public void Test14()
        {
            ConstantPolynom cpoly = new ConstantPolynom(1.0);
            Polynom poly = new Polynom(cpoly);
            IPolynom derivative1 = new ConstantPolynom();
            IPolynom derivative2 = new ConstantPolynom();
            cpoly.Derivate(ref derivative1);
            poly.Derivate(ref derivative2);
            Assert.AreEqual(derivative1[0], derivative2[0]);
        }
        [Test]
        public void Test15()
        {
            ConstantPolynom cpoly = new ConstantPolynom(1.0);
            Polynom poly = new Polynom(cpoly);
            IPolynom derivative1 = new Polynom(0);
            IPolynom derivative2 = new Polynom(0);
            cpoly.Derivate(ref derivative1);
            poly.Derivate(ref derivative2);
            Assert.AreEqual(derivative1[0], derivative2[0]);
        }
        [Test]
        public void Test16()
        {
            ConstantPolynom cpoly = new ConstantPolynom(1.0);
            Polynom poly = new Polynom(cpoly);
            double x = 0;
            double derivate1 = cpoly.Derive(x);
            double derivate2 = poly.Derive(x);
            Assert.AreEqual(derivate1, derivate2);
            x = 1.0;
            derivate1 = cpoly.Derive(x);
            derivate2 = poly.Derive(x);
            Assert.AreEqual(derivate1, derivate2);
        }
        [Test]
        public void Test17()
        {
            ConstantPolynom cpoly = new ConstantPolynom(1.0);
            Polynom poly = new Polynom(cpoly);
            double x = 0;
            double derivate1 = cpoly.DeriveSecond(x);
            double derivate2 = poly.DeriveSecond(x);
            Assert.AreEqual(derivate1, derivate2);
            x = 1.0;
            derivate1 = cpoly.DeriveSecond(x);
            derivate2 = poly.DeriveSecond(x);
            Assert.AreEqual(derivate1, derivate2);
        }

        [Test]
        public void Test18()
        {
            ConstantPolynom cpoly = new ConstantPolynom(1.0);
            Polynom poly = new Polynom(cpoly);
            IPolynom primitive1 = new LinearPolynom();
            IPolynom primitive2 = new LinearPolynom();
            cpoly.Primitive(ref primitive1);
            poly.Primitive(ref primitive2);
            Assert.AreEqual(primitive1[0], primitive2[0]);
            Assert.AreEqual(primitive1[1], primitive2[1]);
        }
        [Test]
        public void Test19()
        {
            ConstantPolynom cpoly = new ConstantPolynom(1.0);
            Polynom poly = new Polynom(cpoly);
            IPolynom primitive1 = new Polynom(1);
            IPolynom primitive2 = new Polynom(1);
            cpoly.Primitive(ref primitive1);
            poly.Primitive(ref primitive2);
            Assert.AreEqual(primitive1[0], primitive2[0]);
            Assert.AreEqual(primitive1[1], primitive2[1]);
        }
        [Test]
        public void Test20()
        {
            ConstantPolynom cpoly = new ConstantPolynom(1.0);
            Polynom poly = new Polynom(cpoly);
            double a = 1.0;
            double b = 2.0;
            double integral1 = cpoly.Integrate(a, b);
            double integral2 = poly.Integrate(a, b);
            Assert.AreEqual(integral1, integral2);
        }

    }
}
