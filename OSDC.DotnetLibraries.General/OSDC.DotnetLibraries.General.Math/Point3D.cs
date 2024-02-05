using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Point3D : Point2D, IPoint3D, IEquatable<IPoint3D>
    {
        public virtual double? Z { get; set; } = null;
        /// <summary>
        /// default constructor
        /// </summary>
        public Point3D() : base()
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="cmp"></param>
        public Point3D(Point3D cmp) : base(cmp)
        {
            if (cmp != null)
            {
                Z = cmp.Z;
            }
        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="pt"></param>
        public Point3D(IPoint3D pt)
        {
            if (pt != null)
            {
                X = pt.X;
                Y = pt.Y;
                Z = pt.Z;
            }
        }
        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Point3D(double? x, double? y, double? z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /// <summary>
        /// constructor with initialization from an array
        /// </summary>
        /// <param name="dat"></param>
        public Point3D(double[] dat)
        {
            if (dat != null && dat.Length >= 3)
            {
                X = dat[0];
                Y = dat[1];
                Z = dat[2];
            }
        }
        /// <summary>
        /// constructor with initialization from a IVector3D
        /// </summary>
        /// <param name="vec"></param>
        public Point3D(IVector3D vec)
        {
            if (vec != null)
            {
                X = vec.X;
                Y = vec.Y;
                Z = vec.Z;
            }
        }
        /// <summary>
        /// constructor with initialization from an IVector
        /// </summary>
        /// <param name="vec"></param>
        public Point3D(IVector vec)
        {
            if (vec != null && vec.Dim >= 3)
            {
                X = vec[0];
                Y = vec[1];
                Z = vec[2];
            }
        }
        /// <summary>
        /// cloning
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new Point3D(this);
        }
        /// <summary>
        /// 
        /// </summary>
        public override void SetUndefined()
        {
            base.SetUndefined();
            Z = Numeric.UNDEF_DOUBLE;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsUndefined()
        {
            return base.IsUndefined() || Numeric.IsUndefined(Z);
        }
        /// <summary>
        /// set the coordinates to zero
        /// </summary>
        public override void SetZero()
        {
            base.SetZero();
            Z = 0;
        }
        /// <summary>
        /// this point is zero if both components are zero
        /// </summary>
        /// <returns></returns>
        public override bool IsZero()
        {
            return base.IsZero() && Numeric.EQ(Z, 0);
        }
        /// <summary>
        /// equality at numeric accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>

        public bool Equals(IPoint3D? cmp)
        {
            if (cmp == null)
            {
                return false;
            }
            return base.Equals(cmp) && Numeric.EQ(Z, cmp.Z);
        }
        /// <summary>
        /// equality at given accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(IPoint3D? cmp, double precision)
        {
            if (cmp == null)
            {
                return false;
            }
            return base.EQ(cmp, precision) && Numeric.EQ(Z, cmp.Z, precision);
        }
        public bool EQ(IPoint3D cmp)
        {
            if (cmp == null)
            {
                return false;
            }
            return base.Equals(cmp) && Numeric.EQ(Z, cmp.Z);
        }
        /// <summary>
        /// equality at given accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool EQ(IPoint3D cmp, double precision)
        {
            if (cmp == null)
            {
                return false;
            }
            return base.EQ(cmp, precision) && Numeric.EQ(Z, cmp.Z, precision);
        }

        /// <summary>
        /// return the polar radius of the point
        /// </summary>
        /// <returns></returns>
        public double? GetPlaneRadius()
        {
            if (X == null || Y == null)
            {
                return null;
            }
            double x = (double)X;
            double y = (double)Y;
            return System.Math.Sqrt(y * y + x * x);
        }
        /// <summary>
        /// return the polar direction of the point
        /// </summary>
        /// <returns></returns>
        public double? GetAz()
        {
            if (X == null || Y == null)
            {
                return null;
            }
            double x = (double)X;
            double y = (double)Y;
            if (Numeric.EQ(x, 0) && Numeric.EQ(y, 0))
            {
                return null;
            }
            return System.Math.Atan2(y, x);
        }
        /// <summary>
        /// Set the coordinates based on a reference point
        /// </summary>
        /// <param name="point"></param>
        public void Set(Point3D point)
        {
            if (point != null)
            {
                base.Set(point);
                Z = point.Z;
            }
        }
        /// <summary>
        /// Set the coordinates based on a reference point
        /// </summary>
        /// <param name="point"></param>
        public void Set(IPoint3D point)
        {
            if (point != null)
            {
                base.Set(point);
                Z = point.Z;
            }
        }

        /// <summary>
        /// Set the coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /// <summary>
        /// Set the coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set(double? x, double? y, double? z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// return the euclidian distance with another point
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double? GetDistance(IPoint2D other)
        {
            if (other == null || X == null || Y == null || other.X == null || other.Y == null)
            {
                return null;
            }
            double x1 = (double)X;
            double y1 = (double)Y;
            double x2 = (double)other.X;
            double y2 = (double)other.Y;
            double dx = x2 - x1;
            double dy = y2 - y1;
            return System.Math.Sqrt(dx * dx + dy * dy);
        }
        /// <summary>
        /// return the spherical radius
        /// </summary>
        /// <returns></returns>
        public double? GetRadius()
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
        /// return the inclination (0 meaning vertical, pi/2 meaning horizontal)
        /// </summary>
        /// <returns></returns>
        public double? GetInclination()
        {
            if (X == null || Y == null || Z == null)
            {
                return null;
            }
            double? length = GetRadius();
            if (length == null)
            {
                return null;
            }
            double l = (double)length;
            double z = (double)Z;
            return System.Math.Acos(z / l);
        }
        /// <summary>
        /// return the euclidian distance to another point
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double? GetDistance(IPoint3D other)
        {
            if (other == null || X == null || Y == null || Z == null || other.X == null || other.Y == null || other.Z == null)
            {
                return null;
            }
            double x1 = (double)X;
            double y1 = (double)Y;
            double z1 = (double)Z;
            double x2 = (double)other.X;
            double y2 = (double)other.Y;
            double z2 = (double)other.Z;
            double dx = x2 - x1;
            double dy = y2 - y1;
            double dz = z2 - z1;
            return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        /// <summary>
        /// return the horizontal distance between two points
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double GetHorizontalDistance(Point3D p)
        {
            if (p == null || X == null || Y == null || p.X == null || p.Y == null)
            {
                return 0;
            }
            else
            {
                return System.Math.Sqrt((double)((p.X - X) * (p.X - X) + (p.Y - Y) * (p.Y - Y)));
            }
        }
        /// <summary>
        /// return the inclination between two points
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double GetIncl(IPoint3D p)
        {
            if (p == null || p.X == null || X == null)
            {
                return 0.0;
            }
            else
            {
                double length = (double)GetDistance(p);
                return Numeric.AcosEqual((double)(p.Z - Z) / length);
            }
        }

        /// <summary>
        /// return the azimuth between two points
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double GetAz(Point3D p)
        {
            if (p == null || p.X == null || X == null)
            {
                return 0.0;
            }
            else
            {
                double length = GetHorizontalDistance(p);
                if (Numeric.EQ(length, 0))
                {
                    return 0.0;
                }
                else
                {
                    if (X == null)
                    {
                        return 0;
                    }
                    else
                    {
                        if ((Y - p.Y) >= 0.0)
                        {
                            return Numeric.AcosEqual((double)(X - p.X) / length);
                        }
                        else
                        {
                            return 0.5 * Numeric.PI - Numeric.AcosEqual((double)(X - p.X) / length);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// force that this instance is at the given coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public void MoveTo(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /// <summary>
        /// move to the same coordinates as pt if it is not null
        /// </summary>
        /// <param name="pt"></param>
        public void MoveTo(IPoint3D pt)
        {
            if (pt != null)
            {
                X = pt.X;
                Y = pt.Y;
                Z = pt.Z;
            }
        }
        /// <summary>
        /// Apply a rotation of radius r and angle a and a z translation
        /// </summary>
        /// <param name="r"></param>
        /// <param name="a"></param>
        /// <param name="dz"></param>
        public void MoveToCylindric(double r, double a, double dz)
        {
            Z += dz;
            X += r * System.Math.Cos(a);
            Y += r + System.Math.Sin(a);
        }
        /// <summary>
        /// Apply a rotation of radius r and angle a 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="a"></param>
        /// <param name="dz"></param>
        public void MoveToCylindric(double r, double a)
        {
            X += r * System.Math.Cos(a);
            Y += r + System.Math.Sin(a);
        }
        /// <summary>
        /// translate the coordinates by the given values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Translate(double x, double y, double z)
        {
            X += x;
            Y += y;
            Z += z;
        }
        /// <summary>
        /// translate by the given vector if it is not null
        /// </summary>
        /// <param name="vec"></param>
        public void Translate(IVector3D vec)
        {
            if (vec != null)
            {
                X += vec.X;
                Y += vec.Y;
                Z += vec.Z;
            }
        }
        /// <summary>
        ///  return the middle point between this point and p
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Point3D GetMiddle(IPoint3D p)
        {
            if (p != null)
            {
                Point3D middle = new Point3D((X + p.X) / 2.0, (Y + p.Y) / 2.0, (Z + p.Z) / 2.0);
                return middle;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// return a point at a relative distance between this point and point p2
        /// </summary>
        /// <param name="p2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public Point3D GetPoint(IPoint3D p2, double distance)
        {
            if (p2 != null)
            {
                return new Point3D(X + (p2.X - X) * distance, Y + (p2.Y - Y) * distance, Z + (p2.Z - Z) * distance);
            }
            else
            {
                if (Numeric.EQ(distance, 0))
                {
                    return new Point3D(this);
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// return this point considered to be defined in the coordinate system defined by pt and incl and az into the global coordinate system
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="incl"></param>
        /// <param name="az"></param>
        /// <returns></returns>
        public Point3D ToGlobal(IPoint3D pt, double incl, double az)
        {
            if (pt != null)
            {
                double ca = System.Math.Cos(az);
                double sa = System.Math.Sin(az);
                double ci = System.Math.Cos(incl);
                double si = System.Math.Sin(incl);
                double? x = ci * pt.X + si * pt.Z;
                double? y = pt.Y;
                double? z = -si * pt.X + ci * pt.Z;
                return new Point3D(X + ca * x - sa * y, Y + sa * x + ca * y, Z + z);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// return this point in the local coordinate system defined by pt and incl and az
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="incl"></param>
        /// <param name="az"></param>
        /// <returns></returns>
        public Point3D ToLocal(IPoint3D pt, double incl, double az)
        {
            if (pt != null)
            {
                double ca = System.Math.Cos(az);
                double sa = System.Math.Sin(az);
                double ci = System.Math.Cos(incl);
                double si = System.Math.Sin(incl);
                double? x = pt.X - X;
                double? y = pt.Y - Y;
                double? z = pt.Z - Z;
                double? XX = ca * x + sa * y;
                double? YY = -sa * x + ca * y;
                return new Point3D(ci * XX - si * z, YY, si * XX + ci * z);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Calculate the cross product between the vector P1P2 and P1P3 where P1 is this.
        /// </summary>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public Vector3D CrossProductVector(Point3D p2, Point3D p3)
        {
            if (p2 != null && p3 != null)
            {
                Vector3D p1p2 = new Vector3D(this, p2);
                Vector3D p1p3 = new Vector3D(this, p3);
                return p1p2.CrossProduct(p1p3);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Check if this is colinear with p2 and p3
        /// </summary>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>        
        public bool AreColinear(IPoint3D p2, IPoint3D p3)
        {
            if (p2 != null && p3 != null)
            {
                Vector3D cross = CrossProductVector((Point3D)p2, (Point3D)p3);
                return Numeric.EQ(cross.GetLength(), 0.0);
            }
            else
            {
                return false;
            }
        }
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="incl"></param>
        /// <param name="az"></param>
        /// <returns></returns>
        public static Point3D CreateSpheric(double l, double incl, double az)
        {
            double ca = System.Math.Cos(az);
            double sa = System.Math.Sin(az);
            double ci = System.Math.Cos(incl);
            double si = System.Math.Sin(az);
            Point3D p = new Point3D();
            p.X = l * ca * si;
            p.Y = l * sa * si;
            p.Z = l * ci;
            return p;
        }
        /// <summary>
        /// Check if the point is in the triangle
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public bool IsInsideTriangle(Triangle3D triangle)
        {
            if (SameSide(this, triangle.Vertex1, triangle.Vertex2, triangle.Vertex3) && SameSide(this, triangle.Vertex2, triangle.Vertex1, triangle.Vertex3) && SameSide(this, triangle.Vertex3, triangle.Vertex1, triangle.Vertex2))
                return true;
            else
                return false;


        }
        /// <summary>
        /// Check if p1 and p2 are on the same side of the line from a to b (assumes that all points are coplanar)
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool SameSide(Point3D p1, Point3D p2, Point3D a, Point3D b)
        {
            Vector3D ab = new Vector3D(a, b);
            Vector3D ap1 = new Vector3D(a, p1);
            Vector3D ap2 = new Vector3D(a, p2);
            Vector3D abap1 = ab.CrossProduct(ap1);
            Vector3D abap2 = ab.CrossProduct(ap2);
            if (abap1.Dot(abap2) >= 0)
                return true;
            else
                return false;
        }
    }
}
