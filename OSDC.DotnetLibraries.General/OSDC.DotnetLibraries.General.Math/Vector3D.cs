using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Vector3D : Vector2D, IVector3D, IEquatable<Vector3D>, IDotProductable<Vector3D>, IAddable<Vector3D>, IScalarProduct<Vector3D>
    {
        /// <summary>
        /// z accessor
        /// </summary>
        public double? Z { get; set; } = null;
        /// <summary>
        /// the dimension is 3
        /// </summary>
        public override int Dim => 3;
        /// <summary>
        /// access to components through indexing
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public override double? this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    default:
                        return Z;
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    default:
                        Z = value;
                        break;
                }
            }
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public Vector3D(): base()
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Vector3D(Vector3D src) : base(src)
        { 
            if (src != null)
            {
                Z = src.Z;
            }
        }
        /// <summary>
        /// constructor with initialization from Point3D instance
        /// </summary>
        /// <param name="pt"></param>
        public Vector3D(Point3D pt)
        {
            if (pt != null)
            {
                X = pt.X;
                Y = pt.Y;
                Z = pt.Z;
            }
        }
        /// <summary>
        /// constructor as a bipoint
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        public Vector3D(Point3D pt1, Point3D pt2) : base()
        {
            if (pt1 != null && pt2 != null)
            {
                X = pt2.X - pt1.X;
                Y = pt2.Y - pt1.Y;
                Z = pt2.Z - pt1.Z;
            }
        }
        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vector3D(double x, double y, double z): base(x, y)
        {
            Z = z;
        }
        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vector3D(double? x, double? y, double? z) : base(x, y)
        {
            Z = z;
        }

        /// <summary>
        /// fill in the passed array with the components of the vector 3D
        /// </summary>
        /// <param name="a"></param>
        public override void CopyTo(double?[] a)
        {
            if (a != null && a.Length >= 3 && X != null && Y != null && Z != null)
            {
                a[0] = (double)X;
                a[1] = (double)Y;
                a[2] = (double)Z;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="v"></param>
        public override void CopyTo(int start, IVector v)
        {
            if (v != null && v.Dim >= Dim - start)
            {
                for (int i = start; i < Dim; i++)
                {
                    v[i - start] = this[i];
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public override void CopyFrom(double[] a)
        {
            if (a != null && a.Length >= Dim)
            {
                X = a[0];
                Y = a[1];
                Z = a[2];
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="v"></param>
        public override void CopyFrom(int start, IVector v)
        {
            if (v != null)
            {
                for (int i = 0; i < System.Math.Min(v.Dim, Dim - start); i++)
                {
                    this[i + start] = v[i];
                }
            }
        }
        /// <summary>
        /// cloning
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new Vector3D(this);
        }

        /// <summary>
        /// equal at numeric accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(Vector3D? cmp)
        {
            if (cmp == null)
            {
                return false;
            }
            return base.EQ(cmp) && Numeric.EQ(Z, cmp.Z);
        }
        /// <summary>
        /// equal at given accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(Vector3D? cmp, double precision)
        {
            if (cmp == null)
            {
                return false;
            }
            return base.EQ(cmp, precision) && Numeric.EQ(Z, cmp.Z, precision);
        }
        /// <summary>
        /// equal at numeric accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool EQ(IVector3D cmp)
        {
            if (cmp == null)
            {
                return false;
            }
            return base.EQ(cmp) && Numeric.EQ(Z, cmp.Z);
        }
        /// <summary>
        /// equal at given accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool EQ(IVector3D cmp, double precision)
        {
            if (cmp == null)
            {
                return false;
            }
            return base.EQ(cmp, precision) && Numeric.EQ(Z, cmp.Z, precision);
        }

        /// <summary>
        /// force the two components to be undefined
        /// </summary>
        public override void SetUndefined()
        {
            base.SetUndefined();
            Z = null;
        }
        /// <summary>
        /// predicate to determine if a vector has at least one component that is undefined
        /// </summary>
        /// <returns></returns>
        public override bool IsUndefined()
        {
            return base.IsUndefined() || Numeric.IsUndefined(Z);
        }
        /// <summary>
        /// Set the components of a 3D vector to zero
        /// </summary>
        public override void SetZero()
        {
            base.SetZero();
            Z = 0;
        }
        /// <summary>
        /// predicate to check that all components are equal to zero
        /// </summary>
        /// <returns></returns>
        public override bool IsZero()
        {
            return base.IsZero() && Numeric.EQ(Z, 0);
        }

        /// <summary>
        /// the Euclidian norm of the 3D vector
        /// </summary>
        /// <returns></returns>
        public override double? GetLength()
        {
            if (X == null || Y == null || Z == null)
            {
                return null;
            }
            double x = (double)X;
            double y = (double)Y;
            double z = (double)Z;
            return System.Math.Sqrt(x * x + y * y + z * z);
        }
        /// <summary>
        /// the square of the Euclidian norm of the 3D vector
        /// </summary>
        /// <returns></returns>
        public override double? GetLength2()
        {
            return X * X + Y * Y + Z * Z;
        }
        /// <summary>
        /// set the length of the vector but does not change the direction
        /// </summary>
        /// <param name="a"></param>
        public override void SetLength(double a)
        {
            if (X != null && Y != null && Z != null)
            {
                double x = (double)X;
                double y = (double)Y;
                double z = (double)Z;
                double l = System.Math.Sqrt(x * x + y * y + z * z);
                if (!Numeric.EQ(l, 0))
                {
                    double factor = a / l;
                    X *= factor;
                    Y *= factor;
                    Z *= factor;
                }
            }
        }
        /// <summary>
        /// check if the vector is unity at the given numerical precision
        /// </summary>
        /// <param name="precision"></param>
        /// <returns></returns>
        public new bool IsUnity()
        {
            return Numeric.EQ(GetLength(), 1.0);
        }
        /// <summary>
        /// normalize the vector
        /// </summary>
        public new void SetUnity()
        {
            SetLength(1.0);
        }

        /// <summary>
        /// return the inclination of this vector
        /// </summary>
        /// <returns></returns>
        public double? GetIncl()
        {
            if (X == null || Y == null || Z == null)
            {
                return null;
            }
            double x = (double)X;
            double y = (double)Y;
            double z = (double)Z;
            double length = System.Math.Sqrt(x * x + y * y + z * z);
            if (Numeric.EQ(length, 0.0))
            {
                return 0.0;
            }
            else
            {
                return System.Math.Acos(z / length);
            }
        }
        /// <summary>
        /// return the azimuth of this vector
        /// </summary>
        /// <returns></returns>
        public double? GetAz()
        {
            if (X == null || Y == null || Z == null)
            {
                return null;
            }
            double x = (double)X;
            double y = (double)Y;
            if (Numeric.EQ(x, 0.0) && Numeric.EQ(y, 0.0))
            {
                return null;
            }
            return System.Math.Atan2(y, x);
        }
        /// <summary>
        /// return the angle compared to the downward vertical if not horizontal, or the azimuth otherwise
        /// </summary>
        /// <param name="isGravity"></param>
        /// <returns></returns>
        public double? GetToolface(out bool isGravity)
        {
            isGravity = false;
            if (X != null || Y != null || Z != null)
            {
                double x = (double)X;
                double y = (double)Y;
                double z = (double)Z;
                Vector3D vertical = new Vector3D(0, 0, -1.0);
                double norm = System.Math.Sqrt(x * x + y * y + z * z);
                if (Numeric.EQ(norm, 0))
                {
                    return null;
                }
                else
                {
                    double? cos = vertical.Dot(this) / norm;
                    if (Numeric.EQ(cos, 0))
                    {
                        // perpendicular to vertical, then we use azimuth
                        return GetAz();
                    }
                    else
                    {
                        double? sin = vertical.CrossProduct(this).GetLength() / norm;
                        if (cos != null && sin != null)
                        {
                            isGravity = true;
                            return System.Math.Atan2((double)sin, (double)cos);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Set the composants based on a reference vector
        /// </summary>
        /// <param name="vector"></param>
        public void Set(IVector3D vector)
        {
            if (vector != null)
            {
                base.Set(vector);
                Z = vector.Z;
            }
        }
        /// <summary>
        /// Set the composants based on a reference vector
        /// </summary>
        /// <param name="vector"></param>
        public void Set(Vector3D vector)
        {
            if (vector != null)
            {
                base.Set(vector);
                Z = vector.Z;
            }
        }
        /// <summary>
        /// Set the vector composants
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set(double x, double y, double z)
        {
                base.Set(x, y);
                Z = z;
        }
        /// <summary>
        /// Set the vector composants
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set(double? x, double? y, double? z)
        {
            base.Set(x, y);
            Z = z;
        }

        /// <summary>
        /// the dot product of two 3D vectors
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double? Dot(Vector3D v)
        {
            if (v == null)
            {
                return null;
            }
            else
            {
                return X * v.X + Y * v.Y + Z * v.Z;
            }
        }
        /// <summary>
        /// return the sum of this vector with another vector
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector3D Add(Vector3D a)
        {
            if (a == null)
            {
                return null;
            }
            return new Vector3D() { X = X + a.X, Y = Y + a.Y, Z = Z + a.Z };
        }
        /// <summary>
        /// increment this vector with the values from another vector
        /// </summary>
        /// <param name="a"></param>
        public void AddAssign(Vector3D a)
        {
            if (a != null)
            {
                X += a.X;
                Y += a.Y;
                Z += a.Z;
            }
        }
        /// <summary>
        /// return the difference between this vector and another vector
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector3D Substract(Vector3D a)
        {
            if (a == null)
            {
                return null;
            }
            return new Vector3D() { X = X - a.X, Y = Y - a.Y, Z = Z - a.Z };
        }
        /// <summary>
        /// decrement this vector by the value of another vector
        /// </summary>
        /// <param name="a"></param>
        public void SubstractAssign(Vector3D a)
        {
            if (a != null)
            {
                X -= a.X;
                Y -= a.Y;
                Z -= a.Z;
            }
        }
        /// <summary>
        /// return the opposite vector to this
        /// </summary>
        public new Vector3D Negate()
        {
            return new Vector3D() { X = -X, Y = -Y, Z = -Z };
        }
        /// <summary>
        /// negate this vector components
        /// </summary>
        public override void NegateAssign()
        {
            X = -X;
            Y = -Y;
            Z = -Z;
        }
        /// <summary>
        /// return a 3D vector that contains the scalar product of this vector by a scalar value
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public new Vector3D Time(double a)
        {
            return new Vector3D() { X = X * a, Y = Y * a, Z = Z * a };
        }
        /// <summary>
        /// assign to this vector the scalar product by a scalar value
        /// </summary>
        /// <param name="x"></param>
        public override void TimeAssign(double x)
        {
            X *= x;
            Y *= x;
            Z *= x;
        }

        /// <summary>
        /// create a Vector3D using spherical components
        /// </summary>
        /// <param name="length"></param>
        /// <param name="inclination"></param>
        /// <param name="azimuth"></param>
        /// <returns></returns>
        public static Vector3D CreateSpheric(double? length, double? inclination, double? azimuth)
        {
            if (length == null || inclination == null || azimuth == null)
            {
                return null;
            }
            else
            {
                double l = (double)length;
                double incl = (double)inclination;
                double az = (double)azimuth;
                double ca = System.Math.Cos(az);
                double sa = System.Math.Sin(az);
                double ci = System.Math.Cos(incl);
                double si = System.Math.Sin(incl);
                return new Vector3D() { X = l * ca * si, Y = l * sa * si, Z = l * ci };
            }
        }
        /// <summary>
        /// in 2D the cross product is an outer operator and it returns a Vector 3D
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector3D CrossProduct(Vector3D a)
        {
            if (a == null)
            {
                return null;
            }
            return new Vector3D() { X = Y * a.Z - Z * a.Y, Y = Z * a.X - X * a.Z, Z = X * a.Y - Y * a.X };
        }
        /// <summary>
        /// in 2D the cross product is an outer operator and it returns a Vector 3D
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public void CrossProductAssign(Vector3D a)
        {
            if (a != null)
            {
                double? x = Y * a.Z - Z * a.Y;
                double? y = Z * a.X - X * a.Z;
                double? z = X * a.Y - Y * a.X;
                X = x;
                Y = y;
                Z = z;
            }
        }
        /// <summary>
        /// predicate for parallelism
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool IsParallel(Vector3D v, double precision)
        {
            if (v == null)
            {
                return false;
            }
            return Numeric.EQ(CrossProduct(v).GetLength(), 0.0, precision);
        }
        /// <summary>
        /// predicate for parallelism
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool IsParallel(Vector3D v)
        {
            if (v == null)
            {
                return false;
            }
            return Numeric.EQ(CrossProduct(v).GetLength(), 0.0);
        }
        /// <summary>
        /// predicate that tests if two 3D vectors are colinear
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool IsColinear(Vector3D v)
        {
            if (v == null || X == null || Y == null || Z == null || v.X == null || v.Y == null || v.Z == null)
            {
                return false;
            }
            double a = Numeric.UNDEF_DOUBLE;
            if (Numeric.IsUndefined(a) && !Numeric.EQ(v.X, 0) && !Numeric.EQ(X, 0))
            {
                a = (double)X / (double)v.X;
            }
            if (Numeric.IsUndefined(a) && !Numeric.EQ(v.Y, 0) && !Numeric.EQ(Y, 0))
            {
                a = (double)Y / (double)v.Y;
            }
            if (Numeric.IsUndefined(a) && !Numeric.EQ(v.Z, 0) && !Numeric.EQ(Z, 0))
            {
                a = (double)Z / (double)v.Z;
            }
            if (Numeric.IsUndefined(a))
            {
                return false;
            }
            else
            {
                return Numeric.EQ(X, a * v.X) && Numeric.EQ(Y, a * v.Y) && Numeric.EQ(Z, a * v.Z);
            }
        }
        /// <summary>
        /// return the solid angle between this vector and another vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double? GetSolidAngle(Vector3D v)
        {
            if (v == null)
            {
                return null;
            }
            double? l1 = GetLength();
            double? l2 = v.GetLength();
            if (Numeric.EQ(l1, 0.0) || Numeric.EQ(l2, 0.0))
            {
                return 0.0;
            }
            else
            {
                return Numeric.AcosEqual(Dot(v) / (l1 * l2));
            }
        }
    }
}
