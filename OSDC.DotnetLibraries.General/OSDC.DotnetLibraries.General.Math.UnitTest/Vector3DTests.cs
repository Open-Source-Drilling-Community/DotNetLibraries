using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Math.UnitTest
{
    public class Vector3DTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            double angle = 0;
            Vector3D vector = new Vector3D(System.Math.Cos(angle), System.Math.Sin(angle), 0);
            bool isGravity;
            double? toolface = vector.GetToolface(out isGravity);
            Assert.IsNotNull(toolface);
            Assert.IsFalse(isGravity);
            Assert.AreEqual(angle, (double)toolface, 1e-5);
        }

        [Test]
        public void Test2()
        {
            double angle = System.Math.PI/2.0;
            Vector3D vector = new Vector3D(System.Math.Cos(angle), System.Math.Sin(angle), 0);
            bool isGravity;
            double? toolface = vector.GetToolface(out isGravity);
            Assert.IsNotNull(toolface);
            Assert.IsFalse(isGravity);
            Assert.AreEqual(angle, (double)toolface, 1e-5);
        }
        [Test]
        public void Test3()
        {
            double angle = System.Math.PI;
            Vector3D vector = new Vector3D(System.Math.Cos(angle), System.Math.Sin(angle), 0);
            bool isGravity;
            double? toolface = vector.GetToolface(out isGravity);
            Assert.IsNotNull(toolface);
            Assert.IsFalse(isGravity);
            Assert.AreEqual(angle, (double)toolface, 1e-5);
        }
        [Test]
        public void Test4()
        {
            double angle = 3.0 * System.Math.PI / 2.0 ;
            Vector3D vector = new Vector3D(System.Math.Cos(angle), System.Math.Sin(angle), 0);
            bool isGravity;
            double? toolface = vector.GetToolface(out isGravity);
            Assert.IsNotNull(toolface);
            Assert.IsFalse(isGravity);
            Assert.AreEqual(-System.Math.PI / 2.0, (double)toolface, 1e-5);
        }
        [Test]
        public void Test5()
        {
            double angle = 0;
            Vector3D vector = new Vector3D(System.Math.Cos(angle), 0, System.Math.Sin(angle));
            bool isGravity;
            double? toolface = vector.GetToolface(out isGravity);
            Assert.IsNotNull(toolface);
            Assert.IsFalse(isGravity);
            Assert.AreEqual(angle, (double)toolface, 1e-5);
        }
        [Test]
        public void Test6()
        {
            double angle = System.Math.PI / 2.0;
            Vector3D vector = new Vector3D(System.Math.Cos(angle), 0, System.Math.Sin(angle));
            bool isGravity;
            double? toolface = vector.GetToolface(out isGravity);
            Assert.IsNotNull(toolface);
            Assert.IsTrue(isGravity);
            Assert.AreEqual(System.Math.PI, (double)toolface, 1e-5);
        }

        [Test]
        public void Test7()
        {
            double angle = System.Math.PI;
            Vector3D vector = new Vector3D(System.Math.Cos(angle), 0, System.Math.Sin(angle));
            bool isGravity;
            double? toolface = vector.GetToolface(out isGravity);
            Assert.IsNotNull(toolface);
            Assert.IsFalse(isGravity);
            Assert.AreEqual(angle, (double)toolface, 1e-5);
        }
        [Test]
        public void Test8()
        {
            double angle = 3.0 * System.Math.PI / 2.0;
            Vector3D vector = new Vector3D(System.Math.Cos(angle), 0, System.Math.Sin(angle));
            bool isGravity;
            double? toolface = vector.GetToolface(out isGravity);
            Assert.IsNotNull(toolface);
            Assert.IsTrue(isGravity);
            Assert.AreEqual(0, (double)toolface, 1e-5);
        }
        [Test]
        public void Test9()
        {
            double angle = 0;
            Vector3D vector = new Vector3D(0, System.Math.Cos(angle), System.Math.Sin(angle));
            bool isGravity;
            double? toolface = vector.GetToolface(out isGravity);
            Assert.IsNotNull(toolface);
            Assert.IsFalse(isGravity);
            Assert.AreEqual(System.Math.PI / 2.0, (double)toolface, 1e-5);
        }
        [Test]
        public void Test10()
        {
            double angle = System.Math.PI / 2.0;
            Vector3D vector = new Vector3D(0, System.Math.Cos(angle), System.Math.Sin(angle));
            bool isGravity;
            double? toolface = vector.GetToolface(out isGravity);
            Assert.IsNotNull(toolface);
            Assert.IsTrue(isGravity);
            Assert.AreEqual(System.Math.PI, (double)toolface, 1e-5);
        }

        [Test]
        public void Test11()
        {
            double angle = System.Math.PI;
            Vector3D vector = new Vector3D(0, System.Math.Cos(angle), System.Math.Sin(angle));
            bool isGravity;
            double? toolface = vector.GetToolface(out isGravity);
            Assert.IsNotNull(toolface);
            Assert.IsFalse(isGravity);
            Assert.AreEqual(-System.Math.PI/2.0, (double)toolface, 1e-5);
        }
        [Test]
        public void Test12()
        {
            double angle = 3.0 * System.Math.PI / 2.0;
            Vector3D vector = new Vector3D(0, System.Math.Cos(angle), System.Math.Sin(angle));
            bool isGravity;
            double? toolface = vector.GetToolface(out isGravity);
            Assert.IsNotNull(toolface);
            Assert.IsTrue(isGravity);
            Assert.AreEqual(0, (double)toolface, 1e-5);
        }
    }
}
