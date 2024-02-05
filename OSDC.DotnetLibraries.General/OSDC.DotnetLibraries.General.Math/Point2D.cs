using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Point2D : IPoint2D
    {
        public virtual double? X { get; set; } = null;
        public virtual double? Y { get; set; } = null;
        /// <summary>
        /// default constructor
        /// </summary>
        public Point2D() : base()
        {
        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Point2D(Point2D src): base()
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
        public Point2D(double x, double y) : base()
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Point2D(double? x, double? y) : base()
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// cloning
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return new Point2D(this);
        }
        /// <summary>
        /// set this point to be undefined
        /// </summary>
        public virtual void SetUndefined()
        {
            X = Numeric.UNDEF_DOUBLE;
            Y = Numeric.UNDEF_DOUBLE;
        }
        /// <summary>
        /// this point is undefined if at least one its coordinate is undefined
        /// </summary>
        /// <returns></returns>
        public virtual bool IsUndefined()
        {
            return Numeric.IsUndefined(X) || Numeric.IsUndefined(Y);
        }
        /// <summary>
        /// set the coordinates to zero
        /// </summary>
        public virtual void SetZero()
        {
            X = 0;
            Y = 0;
        }
        /// <summary>
        /// this point is zero if both components are zero
        /// </summary>
        /// <returns></returns>
        public virtual bool IsZero()
        {
            return Numeric.EQ(X, 0) && Numeric.EQ(Y, 0);
        }
        /// <summary>
        /// Test equality with a numeric accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool EQ(IPoint2D cmp)
        {
            if (cmp == null)
            {
                return false;
            }
            return Numeric.EQ(X, cmp.X) && Numeric.EQ(Y, cmp.Y);
        }
        /// <summary>
        /// Test equality with a given accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool EQ(IPoint2D cmp, double precision)
        {
            if (cmp == null)
            {
                return false;
            }
            return Numeric.EQ(X, cmp.X, precision) && Numeric.EQ(Y, cmp.Y, precision);
        }
        /// <summary>
        /// Set the coordinates based on a reference
        /// </summary>
        /// <param name="point"></param>
        public void Set(IPoint2D point)
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
        public void Set(Point2D point)
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
        /// return the euclidian distance to pt
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public double? Distance(IPoint2D pt)
        {
            if (pt != null)
            {
                return Numeric.SqrtEqual((X - pt.X) * (X - pt.X) + (Y - pt.Y) * (Y - pt.Y));
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }
        /// <summary>
        /// force that this instance is at the given coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public void MoveTo(double x, double y)
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// move to the same coordinates as pt if it is not null
        /// </summary>
        /// <param name="pt"></param>
        public void MoveTo(IPoint2D pt)
        {
            if (pt != null)
            {
                X = pt.X;
                Y = pt.Y;
            }
        }
        /// <summary>
        /// translate the coordinates by the given values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Translate(double x, double y)
        {
            X += x;
            Y += y;
        }
        /// <summary>
        /// translate by the given vector if it is not null
        /// </summary>
        /// <param name="vec"></param>
        public void Translate(IVector2D vec)
        {
            if (vec != null)
            {
                X += vec.X;
                Y += vec.Y;
            }
        }
        /// <summary>
        /// calculate the cross product between the vector P1P2 and P1P3 where P1 is this.
        /// </summary>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public double? CrossProduct(IPoint2D p2, IPoint2D p3)
        {
            if (p2 != null && p3 != null)
            {
                double? ux = p2.X - X;
                double? uy = p2.Y - Y;
                double? vx = p3.X - X;
                double? vy = p3.Y - Y;
                return ux * vy - uy * vx;
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// check if this is colinear with p2 and p3
        /// </summary>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public bool AreColinear(IPoint2D p2, IPoint2D p3)
        {
            if (p2 != null && p3 != null)
            {
                return Numeric.EQ(CrossProduct(p2, p3), 0.0);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// predicate that is true when this is inside the circle defined by p1, p2, p3
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public bool IsInsideCircle(IPoint2D p1, IPoint2D p2, IPoint2D p3)
        {
            if (p1 != null && p2 != null && p3 != null)
            {
                double? a = p1.X * p1.X + p1.Y * p1.Y;
                double? b = p2.X * p2.X + p2.Y * p2.Y;
                double? c = p3.X * p3.X + p3.Y * p3.Y;
                double? cp = p1.CrossProduct(p2, p3);
                if (!Numeric.EQ(cp, 0))
                {
                    double? x = (a * (p2.Y - p3.Y) + b * (p3.Y - p1.Y) + c * (p1.Y - p2.Y)) / (2.0 * cp);
                    double? y = (a * (p2.X - p3.X) + b * (p3.X - p1.X) + c * (p1.X - p2.X)) / (2.0 * cp);
                    double? d1 = (p1.X - x) * (p1.X - x) + (p1.Y - y) * (p1.Y - y);
                    double? d2 = (X - x) * (X - x) + (Y - y) * (Y - y);
                    return Numeric.LE(d2, d1);
                }
                else
                {
                    // the circle is degenerated (the three points p1, p2, p3 are aligned)
                    // the point is inside the circle if it is colinear with at least two of the 3 points 
                    return Numeric.EQ(CrossProduct(p1, p2), 0.0);
                }
            }
            else
            {
                return false;
            }
        }
    }
}
