using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Vector2D : IVector2D
    {
        /// <summary>
        /// X accessor
        /// </summary>
        public double? X { get; set; } = null;
        /// <summary>
        /// Y accessor
        /// </summary>
        public double? Y { get; set; } = null;
        /// <summary>
        /// default constructor
        /// </summary>
        public Vector2D(): base()
        {

        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Vector2D(Vector2D src) : base()
        {
            if (src != null)
            {
                X = src.X;
                Y = src.Y;
            }
        }
        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Vector2D(double? x, double? y)
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// Set the coordinates based on a reference
        /// </summary>
        /// <param name="point"></param>
        public void Set(Vector2D point)
        {
            if (point != null)
            {
                X = point.X;
                Y = point.Y;
            }
        }
        /// <summary>
        /// Set the coordinates based on a reference
        /// </summary>
        /// <param name="point"></param>
        public void Set(IVector2D point)
        {
            if (point != null)
            {
                X = point.X;
                Y = point.Y;
            }
        }
        /// <summary>
        /// Set the coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Set(double x, double y)
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// Set the coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Set(double? x, double? y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Equal at numeric accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(IVector2D cmp)
        {
            if (cmp == null)
            {
                return false;
            }
            return Numeric.EQ(X, cmp.X) && Numeric.EQ(Y, cmp.Y);
        }
        /// <summary>
        /// force the two components to be undefined
        /// </summary>
        public virtual void SetUndefined()
        {
            X = null;
            Y = null;
        }
        /// <summary>
        /// predicate to determine if a 2D vector has at least one component that is undefined
        /// </summary>
        /// <returns></returns>
        public virtual bool IsUndefined()
        {
            return Numeric.IsUndefined(X) || Numeric.IsUndefined(Y);
        }

        /// <summary>
        /// Set the components of a 2D vector to zero
        /// </summary>
        public virtual void SetZero()
        {
            X = 0;
            Y = 0;
        }
        /// <summary>
        /// predicate to check that both components are equal to zero
        /// </summary>
        /// <returns></returns>
        public virtual bool IsZero()
        {
            return Numeric.EQ(X, 0) && Numeric.EQ(Y, 0);
        }
        /// <summary>
        /// the Euclidian norm of the 2D vector
        /// </summary>
        /// <returns></returns>
        public virtual double? GetLength()
        {
            return Numeric.SqrtEqual(X * X + Y * Y);
        }
        /// <summary>
        /// set the length but keep the same direction
        /// </summary>
        /// <param name="a"></param>
        public virtual void SetLength(double a)
        {
            if (X != null && Y != null)
            {
                double x = (double)X;
                double y = (double)Y;
                double length = System.Math.Sqrt(x * x + y * y);
                if (!Numeric.EQ(length, 0))
                {
                    double factor = a / (double)length;
                    X *= factor;
                    Y *= factor;
                }
            }
        }
        /// <summary>
        /// check if the vector is unity at the given numerical precision
        /// </summary>
        /// <param name="precision"></param>
        /// <returns></returns>
        public bool IsUnity()
        {
            return Numeric.EQ(GetLength(), 1.0);
        }
        /// <summary>
        /// normalize the vector
        /// </summary>
        public void SetUnity()
        {
            SetLength(1.0);
        }
        /// <summary>
        /// the dot product of two 2D vectors
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double? Dot(Vector2D v)
        {
            if (v == null)
            {
                return null;
            }
            else
            {
                return X * v.X + Y * v.Y;
            }
        }
        /// <summary>
        /// return the sum of this vector with another vector
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector2D Add(Vector2D a)
        {
            if (a == null)
            {
                return null;
            }
            return new Vector2D() { X = X + a.X, Y = Y + a.Y };
        }
        /// <summary>
        /// increment this vector with the values from another vector
        /// </summary>
        /// <param name="a"></param>
        public void AddAssign(Vector2D a)
        {
            if (a != null)
            {
                X += a.X;
                Y += a.Y;
            }
        }
        /// <summary>
        /// return the difference between this vector and another vector
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector2D Substract(Vector2D a)
        {
            if (a == null)
            {
                return null;
            }
            return new Vector2D() { X = X - a.X, Y = Y - a.Y };

        }
        /// <summary>
        /// decrement this vector by the value of another vector
        /// </summary>
        /// <param name="a"></param>
        public void SubstractAssign(Vector2D a)
        {
            if (a != null)
            {
                X -= a.X;
                Y -= a.Y;
            }
        }
        /// <summary>
        /// return the opposite vector to this
        /// </summary>
        public virtual Vector2D Negate()
        {
            return new Vector2D() { X = -X, Y = -Y };
        }
        /// <summary>
        /// negate this vector components
        /// </summary>
        public virtual void NegateAssign()
        {
            X = -X;
            Y = -Y;
        }
        /// <summary>
        /// return a 2D vector that contains the scalar product of this vector by a scalar value
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector2D Time(double a)
        {
            return new Vector2D() { X = X * a, Y = Y * a };
        }
        /// <summary>
        /// assign to this vector the scalar product by a scalar value
        /// </summary>
        /// <param name="a"></param>
        public virtual void TimeAssign(double a)
        {
            X *= a;
            Y *= a;
        }

        /// <summary>
        /// in 2D the cross product is an outer operator and it returns a Vector 3D
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector3D CrossProduct(Vector2D a)
        {
            if (a != null)
            {
                Vector3D v = new Vector3D();
                v.X = 0;
                v.Y = 0;
                v.Z = X * a.Y - Y * a.X;
                return v;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// predicate that tests if two 2D vectors are colinear
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool IsColinear(Vector2D v)
        {
            if (v == null || v.X == null || v.Y == null || X == null || Y == null)
            {
                return false;
            }
            else
            {
                double a = Numeric.UNDEF_DOUBLE;
                if (Numeric.IsUndefined(a) && !Numeric.EQ(v.X, 0) && !Numeric.EQ(X, 0))
                {
                    a = (double)X / (double)v.X;
                }
                if (Numeric.IsUndefined(a) && !Numeric.EQ(v.Y, 0) && !Numeric.EQ(Y, 0))
                {
                    a = (double)Y / (double)v.Y;
                }
                if (Numeric.IsUndefined(a))
                {
                    return false;
                }
                else
                {
                    return Numeric.EQ(X, a * v.X) && Numeric.EQ(Y, a * v.Y);
                }
            }
        }

    }
}
