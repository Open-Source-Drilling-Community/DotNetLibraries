using OSDC.DotnetLibraries.General.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Math.UnitTest
{
    public class Matrix3x3Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Evaluates whether two 3x3 matrices are equal
        /// </summary>
        [Test]
        public void Test_Equal()
        {
            Matrix3x3 m1 = new Matrix3x3(1, 0, 0,
                                        0, 1, 0,
                                        0, 0, 1);
            Matrix3x3 m2 = new Matrix3x3(1, 0, 0,
                                        0, 1, 0,
                                        0, 0, 1);
            Assert.IsTrue(m1.EQ(m2));
        }

        /// <summary>
        /// Evaluates whether two 3x3 matrices are equal to given precisions
        /// </summary>
        [Test]
        public void Test_EqualPrecision()
        {
            Matrix3x3 m1 = new Matrix3x3(1, 0, 0,
                                        0, 1, 0,
                                        0, 0, 1);
            Matrix3x3 m2 = new Matrix3x3(1.001, 0, 0,
                                        0, 1, 0,
                                        0, 0, 1);
            Assert.IsFalse(m1.EQ(m2));
            Assert.IsFalse(m1.EQ(m2, 0.0001));
            Assert.IsTrue(m1.EQ(m2, 0.001));
            Assert.IsTrue(m1.EQ(m2, 0.01));
        }

        /// <summary>
        /// Adds a 3x3 matrix to itself
        /// </summary>
        [Test]
        public void Test_Add()
        {
            Matrix3x3 m1 = new Matrix3x3(1, 2, 3,
                                        4, 5, 6,
                                        7, 8, 9);
            IMatrix m2 = m1.Add(m1);
            Assert.AreEqual(m2[0, 0], 2);
            Assert.AreEqual(m2[0, 1], 4);
            Assert.AreEqual(m2[0, 2], 6);
            Assert.AreEqual(m2[1, 0], 8);
            Assert.AreEqual(m2[1, 1], 10);
            Assert.AreEqual(m2[1, 2], 12);
            Assert.AreEqual(m2[2, 0], 14);
            Assert.AreEqual(m2[2, 1], 16);
            Assert.AreEqual(m2[2, 2], 18);
        }

        /// <summary>
        /// Adds a 3x3 matrix to itself and assigns the result to itself
        /// </summary>
        [Test]
        public void Test_Add_Assign()
        {
            Matrix3x3 m1 = new Matrix3x3(1, 2, 3,
                                        4, 5, 6,
                                        7, 8, 9);
            m1.AddAssign(m1);
            Assert.AreEqual(m1[0, 0], 2);
            Assert.AreEqual(m1[0, 1], 4);
            Assert.AreEqual(m1[0, 2], 6);
            Assert.AreEqual(m1[1, 0], 8);
            Assert.AreEqual(m1[1, 1], 10);
            Assert.AreEqual(m1[1, 2], 12);
            Assert.AreEqual(m1[2, 0], 14);
            Assert.AreEqual(m1[2, 1], 16);
            Assert.AreEqual(m1[2, 2], 18);
        }

        /// <summary>
        /// Negates a 3x3 matrix
        /// </summary>
        [Test]
        public void Test_Negate()
        {
            Matrix3x3 m1 = new Matrix3x3(1, 2, 3,
                                        4, 5, 6,
                                        7, 8, 9);
            IMatrix m2 = m1.Negate();
            Assert.AreEqual(m2[0, 0], -1);
            Assert.AreEqual(m2[0, 1], -2);
            Assert.AreEqual(m2[0, 2], -3);
            Assert.AreEqual(m2[1, 0], -4);
            Assert.AreEqual(m2[1, 1], -5);
            Assert.AreEqual(m2[1, 2], -6);
            Assert.AreEqual(m2[2, 0], -7);
            Assert.AreEqual(m2[2, 1], -8);
            Assert.AreEqual(m2[2, 2], -9);
        }

        /// <summary>
        /// Negates a 3x3 matrix and assigns the result to itself
        /// </summary>
        [Test]
        public void Test_Negate_Assign()
        {
            Matrix3x3 m1 = new Matrix3x3(1, 2, 3,
                                        4, 5, 6,
                                        7, 8, 9);
            m1.NegateAssign();
            Assert.AreEqual(m1[0, 0], -1);
            Assert.AreEqual(m1[0, 1], -2);
            Assert.AreEqual(m1[0, 2], -3);
            Assert.AreEqual(m1[1, 0], -4);
            Assert.AreEqual(m1[1, 1], -5);
            Assert.AreEqual(m1[1, 2], -6);
            Assert.AreEqual(m1[2, 0], -7);
            Assert.AreEqual(m1[2, 1], -8);
            Assert.AreEqual(m1[2, 2], -9);
        }

        /// <summary>
        /// Subtracts a 3x3 matrix to itself
        /// </summary>
        [Test]
        public void Test_Subtract()
        {
            Matrix3x3 m1 = new Matrix3x3(1, 2, 3,
                                        4, 5, 6,
                                        7, 8, 9);
            IMatrix m2 = m1.Substract(m1);
            Assert.AreEqual(m2[0, 0], 0);
            Assert.AreEqual(m2[0, 1], 0);
            Assert.AreEqual(m2[0, 2], 0);
            Assert.AreEqual(m2[1, 0], 0);
            Assert.AreEqual(m2[1, 1], 0);
            Assert.AreEqual(m2[1, 2], 0);
            Assert.AreEqual(m2[2, 0], 0);
            Assert.AreEqual(m2[2, 1], 0);
            Assert.AreEqual(m2[2, 2], 0);
        }

        /// <summary>
        /// Subtracts a 3x3 matrix to itself and assigns the result to itself
        /// </summary>
        [Test]
        public void Test_Subtract_Assign()
        {
            Matrix3x3 m1 = new Matrix3x3(1, 2, 3,
                                        4, 5, 6,
                                        7, 8, 9);
            m1.SubstractAssign(m1);
            Assert.AreEqual(m1[0, 0], 0);
            Assert.AreEqual(m1[0, 1], 0);
            Assert.AreEqual(m1[0, 2], 0);
            Assert.AreEqual(m1[1, 0], 0);
            Assert.AreEqual(m1[1, 1], 0);
            Assert.AreEqual(m1[1, 2], 0);
            Assert.AreEqual(m1[2, 0], 0);
            Assert.AreEqual(m1[2, 1], 0);
            Assert.AreEqual(m1[2, 2], 0);
        }

        /// <summary>
        /// Multiplies a 3x3 matrix by a scalar
        /// </summary>
        [Test]
        public void Test_MulScalar()
        {
            Matrix3x3 m1 = new Matrix3x3(1, 2, 3,
                                        4, 5, 6,
                                        7, 8, 9);
            IMatrix m2 = m1.Multiply(10);
            Assert.AreEqual((double)m2[0, 0], 10.0);
            Assert.AreEqual((double)m2[0, 1], 20.0);
            Assert.AreEqual((double)m2[0, 2], 30.0);
            Assert.AreEqual((double)m2[1, 0], 40.0);
            Assert.AreEqual((double)m2[1, 1], 50.0);
            Assert.AreEqual((double)m2[1, 2], 60.0);
            Assert.AreEqual((double)m2[2, 0], 70.0);
            Assert.AreEqual((double)m2[2, 1], 80.0);
            Assert.AreEqual((double)m2[2, 2], 90.0);
        }

        /// <summary>
        /// Multiplies a 3x3 matrix by a scalar and assigns the result to itself
        /// </summary>
        [Test]
        public void Test_MulScalar_Assign()
        {
            Matrix3x3 m = new Matrix3x3(1, 2, 3,
                                        4, 5, 6,
                                        7, 8, 9);
            m.MultiplyAssign(10);
            Assert.AreEqual((double)m[0, 0], 10.0);
            Assert.AreEqual((double)m[0, 1], 20.0);
            Assert.AreEqual((double)m[0, 2], 30.0);
            Assert.AreEqual((double)m[1, 0], 40.0);
            Assert.AreEqual((double)m[1, 1], 50.0);
            Assert.AreEqual((double)m[1, 2], 60.0);
            Assert.AreEqual((double)m[2, 0], 70.0);
            Assert.AreEqual((double)m[2, 1], 80.0);
            Assert.AreEqual((double)m[2, 2], 90.0);
        }

        /// <summary>
        /// Multiplies a 3x3 matrix by a 3D vector
        /// </summary>
        [Test]
        public void Test_MulVector()
        {
            Matrix3x3 m = new Matrix3x3(1, 2, 3,
                                        4, 5, 6,
                                        7, 8, 9);
            Vector3D v1 = new Vector3D(1, 2, 3);
            IVector v2 = m.Multiply(v1);
            Assert.AreEqual((double)v2[0], 14);
            Assert.AreEqual((double)v2[1], 32);
            Assert.AreEqual((double)v2[2], 50);
        }

        /// <summary>
        /// Multiplies a 3x3 matrix by another 3x3 matrix
        /// </summary>
        [Test]
        public void Test_MulMatrix()
        {
            Matrix3x3 m1 = new Matrix3x3(1, 4, -1,
                                        6, 1, 8,
                                        2, 5, 1);
            Matrix3x3 m2 = new Matrix3x3(2, 0, 1,
                                        7, 4, 2,
                                        8, 1, 3);
            Matrix3x3 m3 = new Matrix3x3(22, 15, 6,
                                        83, 12, 32,
                                        47, 21, 15);
            IMatrix m4 = m1.Multiply(m2);

            Assert.IsTrue(m4.EQ(m3));
        }

        /// <summary>
        /// Multiplies a 3x3 matrix by another 3x3 matrix and assigns the result to itself
        /// </summary>
        [Test]
        public void Test_MulMatrix_Assign()
        {
            Matrix3x3 m1 = new Matrix3x3(1, 2, 3,
                                        4, 5, 6,
                                        7, 8, 9);
            Matrix3x3 m2 = new Matrix3x3(2, 3, 4,
                                        5, 6, 7,
                                        8, 9, 1);
            Matrix3x3 m3 = new Matrix3x3(36, 42, 21,
                                        81, 96, 57,
                                        126, 150, 93);
            m1.MultiplyAssign(m2);

            Assert.IsTrue(m1.EQ(m3));
        }

        /// <summary>
        /// Transposes a 3x3 matrix
        /// </summary>
        [Test]
        public void Test_Transpose()
        {
            Matrix3x3 m = new Matrix3x3(1, 2, 3,
                                        4, 5, 6,
                                        7, 8, 9);
            m.Transpose();
            Assert.AreEqual((double)m[0, 0], 1);
            Assert.AreEqual((double)m[0, 1], 4);
            Assert.AreEqual((double)m[0, 2], 7);
            Assert.AreEqual((double)m[1, 0], 2);
            Assert.AreEqual((double)m[1, 1], 5);
            Assert.AreEqual((double)m[1, 2], 8);
            Assert.AreEqual((double)m[2, 0], 3);
            Assert.AreEqual((double)m[2, 1], 6);
            Assert.AreEqual((double)m[2, 2], 9);
        }

        /// <summary>
        /// Transposes a 3x3 matrix and stores the result in another matrix
        /// </summary>
        [Test]
        public void Test_Transpose_Store()
        {
            IMatrix m1 = new Matrix3x3(1, 2, 3,
                                        4, 5, 6,
                                        7, 8, 9);
            IMatrix m2 = new Matrix3x3();
            m1.Transpose(ref m2);
            Assert.AreEqual((double)m2[0, 0], 1);
            Assert.AreEqual((double)m2[0, 1], 4);
            Assert.AreEqual((double)m2[0, 2], 7);
            Assert.AreEqual((double)m2[1, 0], 2);
            Assert.AreEqual((double)m2[1, 1], 5);
            Assert.AreEqual((double)m2[1, 2], 8);
            Assert.AreEqual((double)m2[2, 0], 3);
            Assert.AreEqual((double)m2[2, 1], 6);
            Assert.AreEqual((double)m2[2, 2], 9);
        }

        /// <summary>
        /// Computes the determinant of a 3x3 matrix
        /// </summary>
        [Test]
        public void Test_Determinant()
        {
            Matrix3x3 m = new Matrix3x3(2, 4, -3,
                                      5, 7, 6,
                                     -8, 1, 9);
            double? det = m.Determinant();
            Assert.IsNotNull(det);
            Assert.AreEqual((double)det, -441);
        }

        /// <summary>
        /// Invert a 3x3 matrix
        /// </summary>
        [Test]
        public void Test_Invert()
        {
            IMatrix m1 = new Matrix3x3(7, -6, 3,
                                        4, -5, -4,
                                        2, 1, 8);
            IMatrix m2 = Matrix3x3.Invert(m1);
            Matrix3x3 invm = new Matrix3x3(-36, 51, 39,
                                           -40, 50, 40,
                                            14, -19, -11);
            invm = (Matrix3x3)invm.Multiply(1.0 / 30.0);
            Assert.IsTrue(m2.EQ(invm));

            IMatrix m3 = new Matrix3x3(0, 0, 1,
                              0, 0, 1,
                              1, 1, 4);
            Assert.IsTrue(Matrix3x3.Invert(m3) == null);
        }

        /// <summary>
        /// InvertAssign a 3x3 matrix
        /// </summary>
        [Test]
        public void Test_Invert_Assign()
        {
            Matrix3x3 m1 = new Matrix3x3(0, 0, 1,
                                        2, -1, 3,
                                        1, 1, 4);
            Assert.IsTrue(m1.InvertAssign());
            Matrix3x3 invm = new Matrix3x3(-7, 1, 1,
                                         -5, -1, 2,
                                         3, 0, 0);
            invm.MultiplyAssign(1.0 / 3.0);
            Assert.IsTrue(m1.EQ(invm));

            Matrix3x3 m2 = new Matrix3x3(0, 0, 1,
                              0, 0, 1,
                              1, 1, 4);
            Assert.IsFalse(m2.InvertAssign());
        }

        /// <summary>
        /// Verifies that elementary rotations are unitary
        /// </summary>
        [Test]
        public void Test_RotUnitary()
        {
            Matrix3x3 id = new Matrix3x3(1, 0, 0,
                                        0, 1, 0,
                                        0, 0, 1);
            Matrix3x3 m1 = new Matrix3x3();
            Matrix3x3 m2 = new Matrix3x3();

            m1.RotXAssign(System.Math.PI / 2);
            m2.RotXAssign(-System.Math.PI / 2);
            m1.MultiplyAssign(m2);
            Assert.IsTrue(m1.EQ(id));

            m1.RotYAssign(System.Math.PI / 2);
            m2.RotYAssign(-System.Math.PI / 2);
            m1.MultiplyAssign(m2);
            Assert.IsTrue(m1.EQ(id));

            m1.RotZAssign(System.Math.PI / 2);
            m2.RotZAssign(-System.Math.PI / 2);
            m1.MultiplyAssign(m2);
            Assert.IsTrue(m1.EQ(id));

            Vector3D v = new Vector3D(1, 2, 3);

            m1.RotAssign(System.Math.PI / 2, v[0], v[1], v[2]);
            m2.RotAssign(-System.Math.PI / 2, v[0], v[1], v[2]);
            m1.MultiplyAssign(m2);
            Assert.IsTrue(m1.EQ(id));

            m1.RotAssign(System.Math.PI / 2, v);
            m2.RotAssign(-System.Math.PI / 2, v);
            m1.MultiplyAssign(m2);
            Assert.IsTrue(m1.EQ(id));
        }

        /// <summary>
        /// Rotates points in the 3D space by various angles and around various axes
        /// </summary>
        [Test]
        public void Test_Rotation()
        {
            Matrix3x3 m = new Matrix3x3();

            IVector p1 = new Vector3D(1, 1, 0);
            Vector3D p2 = new Vector3D(1, 0, 1);
            m.RotXAssign(System.Math.PI / 2);
            Vector3D v = (Vector3D)m.Multiply(p1);
            Assert.IsTrue(v.EQ(p2));

            p1 = new Vector3D(1, 0, 1);
            p2 = new Vector3D(-1, 0, 1);
            m.RotYAssign(-System.Math.PI / 2);
            v = (Vector3D)m.Multiply(p1);
            Assert.IsTrue(v.EQ(p2));

            p1 = new Vector3D(-1, 0, 1);
            p2 = new Vector3D(0, 1, 1);
            m.RotZAssign(-System.Math.PI / 2);
            v = (Vector3D)m.Multiply(p1);
            Assert.IsTrue(v.EQ(p2));

            p1 = new Vector3D(0, 1, 0);
            p2 = new Vector3D(1, 0, 0);
            m.RotAssign(System.Math.PI, 1, 1, 0);
            v = (Vector3D)m.Multiply(p1);
            Assert.IsTrue(v.EQ(p2));

            double? sq2 = System.Math.Sqrt(2);
            p1 = new Vector3D(0, 1, 0);
            p2 = new Vector3D(1 / sq2, 0, 1 / sq2);
            Vector3D w = new Vector3D(1, sq2, 1);
            m.RotAssign(System.Math.PI, w);
            v = (Vector3D)m.Multiply(p1);
            Assert.IsTrue(v.EQ(p2));
        }
    }
}
