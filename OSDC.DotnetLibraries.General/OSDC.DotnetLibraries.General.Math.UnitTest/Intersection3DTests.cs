using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Math.UnitTest
{
    public class Intersection3DTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Line3D line2 = new Line3D();
            line2.ReferencePoint = new Point3D(0, 0, 0);
            line2.Direction = new Vector3D(0, 1, 0);
            Point3D intersection = line1.GetIntersection(line2);
            Assert.IsNotNull(intersection);
            Assert.AreEqual(intersection.X, 0);
            Assert.AreEqual(intersection.Y, 0);
            Assert.AreEqual(intersection.Z, 0);
        }
        [Test]
        public void Test2()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Ray3D ray2 = new Ray3D();
            ray2.ReferencePoint = new Point3D(0, 0, 0);
            ray2.Direction = new Vector3D(0, 1, 0);
            Point3D intersection = line1.GetIntersection(ray2);
            Assert.IsNotNull(intersection);
            Assert.AreEqual(intersection.X, 0);
            Assert.AreEqual(intersection.Y, 0);
            Assert.AreEqual(intersection.Z, 0);
        }
        [Test]
        public void Test3()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(0, 0, 0);
            segment2.Direction = new Vector3D(0, 1, 0);
            Point3D intersection = line1.GetIntersection(segment2);
            Assert.IsNotNull(intersection);
            Assert.AreEqual(intersection.X, 0);
            Assert.AreEqual(intersection.Y, 0);
            Assert.AreEqual(intersection.Z, 0);
        }

        [Test]
        public void Test4()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Line3D line2 = new Line3D();
            line2.ReferencePoint = new Point3D(0, 1, 0);
            line2.Direction = new Vector3D(0, 1, 0);
            Point3D intersection = line1.GetIntersection(line2);
            Assert.IsNotNull(intersection);
            Assert.AreEqual(intersection.X, 0);
            Assert.AreEqual(intersection.Y, 0);
            Assert.AreEqual(intersection.Z, 0);
        }
        [Test]
        public void Test5()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Ray3D ray2 = new Ray3D();
            ray2.ReferencePoint = new Point3D(0, 1, 0);
            ray2.Direction = new Vector3D(0, 1, 0);
            Point3D intersection = line1.GetIntersection(ray2);
            Assert.IsNull(intersection);
        }
        [Test]
        public void Test6()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(0, 1, 0);
            segment2.Direction = new Vector3D(0, 1, 0);
            Point3D intersection = line1.GetIntersection(segment2);
            Assert.IsNull(intersection);
        }
        [Test]
        public void Test7()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Line3D line2 = new Line3D();
            line2.ReferencePoint = new Point3D(0, 1, 0);
            line2.Direction = new Vector3D(0, 0, 1);
            Point3D intersection = line1.GetIntersection(line2);
            Assert.IsNull(intersection);
        }
        [Test]
        public void Test8()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Ray3D ray2 = new Ray3D();
            ray2.ReferencePoint = new Point3D(0, 1, 0);
            ray2.Direction = new Vector3D(0, 0, 1);
            Point3D intersection = line1.GetIntersection(ray2);
            Assert.IsNull(intersection);
        }
        [Test]
        public void Test9()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(0, 1, 0);
            segment2.Direction = new Vector3D(0, 0, 1);
            Point3D intersection = line1.GetIntersection(segment2);
            Assert.IsNull(intersection);
        }

        [Test]
        public void Test10()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Line3D line2 = new Line3D();
            line2.ReferencePoint = new Point3D(0, 0, 0);
            line2.Direction = new Vector3D(0, 1, 0);
            double? distance = line1.GetDistance(line2);
            Assert.IsNotNull(distance);
            Assert.AreEqual(0, distance.Value);
        }
        [Test]
        public void Test11()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Ray3D ray2 = new Ray3D();
            ray2.ReferencePoint = new Point3D(0, 0, 0);
            ray2.Direction = new Vector3D(0, 1, 0);
            double? distance = line1.GetDistance(ray2);
            Assert.IsNotNull(distance);
            Assert.AreEqual(0, distance.Value);
        }
        [Test]
        public void Test12()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(0, 0, 0);
            segment2.Direction = new Vector3D(0, 1, 0);
            double? distance = line1.GetDistance(segment2);
            Assert.IsNotNull(distance);
            Assert.AreEqual(0, distance.Value);
        }
        [Test]
        public void Test13()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Line3D line2 = new Line3D();
            line2.ReferencePoint = new Point3D(0, 1, 0);
            line2.Direction = new Vector3D(0, 0, 1);
            double? distance = line1.GetDistance(line2);
            Assert.IsNotNull(distance);
            Assert.AreEqual(1.0, distance);
        }
        [Test]
        public void Test14()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Ray3D ray2 = new Ray3D();
            ray2.ReferencePoint = new Point3D(0, 1, 0);
            ray2.Direction = new Vector3D(0, 0, 1);
            double? distance = line1.GetDistance(ray2);
            Assert.IsNotNull(distance);
            Assert.AreEqual(1.0, distance);
        }
        [Test]
        public void Test15()
        {
            Line3D line1 = new Line3D();
            line1.ReferencePoint = new Point3D(0, 0, 0);
            line1.Direction = new Vector3D(1, 0, 0);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(0, 1, 0);
            segment2.Direction = new Vector3D(0, 0, 1);
            double? distance = line1.GetDistance(segment2);
            Assert.IsNotNull(distance);
            Assert.AreEqual(1.0, distance);
        }
        [Test]
        public void Test16()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(1, 0, 0);
            segment2.End = new Point3D(1, 0, 10);
            double? param1, param2, toolface;
            bool isGravity;
            double? distance = segment1.GetDistance(segment2, out param1, out param2, out toolface, out isGravity);
            Assert.IsNotNull(distance);
            Assert.AreEqual(1.0, distance);
            Assert.AreEqual(0, param1);
            Assert.AreEqual(0, param2);
            Assert.AreEqual(0, toolface);
            Assert.IsFalse(isGravity);
        }
        [Test]
        public void Test17()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(1, 0, 20);
            segment2.End = new Point3D(1, 0, 30);
            double? distance = segment1.GetDistance(segment2);
            Assert.IsNotNull(distance);
            Assert.AreEqual(System.Math.Sqrt((double)((segment2.ReferencePoint.X-segment1.End.X)* (segment2.ReferencePoint.X - segment1.End.X)+ 
                                                      (segment2.ReferencePoint.Y - segment1.End.Y) * (segment2.ReferencePoint.Y - segment1.End.Y)+ 
                                                      (segment2.ReferencePoint.Z - segment1.End.Z) * (segment2.ReferencePoint.Z - segment1.End.Z))), 
                            distance, 1e-6);
        }
        [Test]
        public void Test18()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(1, 0, 5);
            segment2.End = new Point3D(1, 0, 10);
            double? distance = segment1.GetDistance(segment2);
            Assert.IsNotNull(distance);
            Assert.AreEqual(1.0, distance);
        }
        [Test]
        public void Test19()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(1, 0, -5);
            segment2.End = new Point3D(1, 0, 5);
            double? distance = segment1.GetDistance(segment2);
            Assert.IsNotNull(distance);
            Assert.AreEqual(1.0, distance);
        }
        [Test]
        public void Test20()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(1, 0, -5);
            segment2.End = new Point3D(1, 0, 15);
            double? distance = segment1.GetDistance(segment2);
            Assert.IsNotNull(distance);
            Assert.AreEqual(1.0, distance);
        }
        [Test]
        public void Test21()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(1, 0, 3);
            segment2.End = new Point3D(1, 0, 7);
            double? distance = segment1.GetDistance(segment2);
            Assert.IsNotNull(distance);
            Assert.AreEqual(1.0, distance);
        }
        [Test]
        public void Test22()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 3);
            segment1.End = new Point3D(0, 0, 7);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(1, 0, 0);
            segment2.End = new Point3D(1, 0, 10);
            double? distance = segment1.GetDistance(segment2);
            Assert.IsNotNull(distance);
            Assert.AreEqual(1.0, distance);
        }
        [Test]
        public void Test23()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(1, -5, 5);
            segment2.End = new Point3D(1, 5, 5);
            double? param1, param2, toolface;
            bool isGravity;
            double? distance = segment1.GetDistance(segment2, out param1, out param2, out toolface, out isGravity); 
            Assert.IsNotNull(distance);
            Assert.AreEqual(1.0, distance);
            Assert.AreEqual(0.5, param1);
            Assert.AreEqual(0.5, param2);
        }
        [Test]
        public void Test24()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(1, 1, 5);
            segment2.End = new Point3D(1, 11, 5);
            double? param1, param2, toolface;
            bool isGravity;
            double? distance = segment1.GetDistance(segment2, out param1, out param2, out toolface, out isGravity);
            Assert.IsNotNull(distance);
            Assert.AreEqual(System.Math.Sqrt(2), distance);
            Assert.AreEqual(0.5, param1);
            Assert.AreEqual(0, param2);
        }
        [Test]
        public void Test25()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(1, -11, 5);
            segment2.End = new Point3D(1, -1, 5);
            double? param1, param2, toolface;
            bool isGravity;
            double? distance = segment1.GetDistance(segment2, out param1, out param2, out toolface, out isGravity);
            Assert.IsNotNull(distance);
            Assert.AreEqual(System.Math.Sqrt(2), distance);
            Assert.AreEqual(0.5, param1);
            Assert.AreEqual(1, param2);
        }
        [Test]
        public void Test26()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(1, -5, -1);
            segment2.End = new Point3D(1, 5, -1);
            double? param1, param2, toolface;
            bool isGravity;
            double? distance = segment1.GetDistance(segment2, out param1, out param2, out toolface, out isGravity);
            Assert.IsNotNull(distance);
            Assert.AreEqual(System.Math.Sqrt(2), distance);
            Assert.AreEqual(0, param1);
            Assert.AreEqual(0.5, param2);
        }
        [Test]
        public void Test27()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(1, -5, 11);
            segment2.End = new Point3D(1, 5, 11);
            double? param1, param2, toolface;
            bool isGravity;
            double? distance = segment1.GetDistance(segment2, out param1, out param2, out toolface, out isGravity);
            Assert.IsNotNull(distance);
            Assert.AreEqual(System.Math.Sqrt(2), distance);
            Assert.AreEqual(1, param1);
            Assert.AreEqual(0.5, param2);
        }
        [Test]
        public void Test28()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(0, -5, 5);
            segment2.End = new Point3D(0, 5, 5);
            double? param1, param2, toolface;
            bool isGravity;
            double? distance = segment1.GetDistance(segment2, out param1, out param2, out toolface, out isGravity);
            Assert.IsNotNull(distance);
            Assert.AreEqual(0, distance);
            Assert.AreEqual(0.5, param1);
            Assert.AreEqual(0.5, param2);
        }
        [Test]
        public void Test29()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(0, -5, 10);
            segment2.End = new Point3D(0, 5, 10);
            double? param1, param2, toolface;
            bool isGravity;
            double? distance = segment1.GetDistance(segment2, out param1, out param2, out toolface, out isGravity);
            Assert.IsNotNull(distance);
            Assert.AreEqual(0, distance);
            Assert.AreEqual(1.0, param1);
            Assert.AreEqual(0.5, param2);
        }
        [Test]
        public void Test30()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(0, -5, 0);
            segment2.End = new Point3D(0, 5, 0);
            double? param1, param2, toolface;
            bool isGravity;
            double? distance = segment1.GetDistance(segment2, out param1, out param2, out toolface, out isGravity);
            Assert.IsNotNull(distance);
            Assert.AreEqual(0, distance);
            Assert.AreEqual(0.0, param1);
            Assert.AreEqual(0.5, param2);
        }
        [Test]
        public void Test31()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(0, -5, -1);
            segment2.End = new Point3D(0, 5, -1);
            double? param1, param2, toolface;
            bool isGravity;
            double? distance = segment1.GetDistance(segment2, out param1, out param2, out toolface, out isGravity);
            Assert.IsNotNull(distance);
            Assert.AreEqual(1, distance);
            Assert.AreEqual(0.0, param1);
            Assert.AreEqual(0.5, param2);
        }
        [Test]
        public void Test32()
        {
            Segment3D segment1 = new Segment3D();
            segment1.ReferencePoint = new Point3D(0, 0, 0);
            segment1.End = new Point3D(0, 0, 10);
            Segment3D segment2 = new Segment3D();
            segment2.ReferencePoint = new Point3D(0, -5, 11);
            segment2.End = new Point3D(0, 5, 11);
            double? param1, param2, toolface;
            bool isGravity;
            double? distance = segment1.GetDistance(segment2, out param1, out param2, out toolface, out isGravity);
            Assert.IsNotNull(distance);
            Assert.AreEqual(1, distance);
            Assert.AreEqual(1.0, param1);
            Assert.AreEqual(0.5, param2);
        }
    }
}
