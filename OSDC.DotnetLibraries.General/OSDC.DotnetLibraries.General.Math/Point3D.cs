using OSDC.DotnetLibraries.General.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Point3D : Point2D, IPoint3D
    {
        public double? Z { get; set; } = null;
        /// <summary>
        /// default constructor
        /// </summary>
        public Point3D(): base()
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
        /// equality at numeric accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(IPoint3D cmp)
        {
            if (cmp == null)
            {
                return false;
            }
            return base.Equals(cmp) && Numeric.EQ(Z, cmp.Z);
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
            if (X== null || Y== null)
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
            if (X== null || Y == null || Z == null)
            {
                return null;
            }
            double x = (double)X;
            double y = (double)Y;
            double z = (double)Z;
            return System.Math.Sqrt(x*x + y*y + z*z);
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
            return System.Math.Sqrt(dx*dx + dy*dy + dz*dz);
        }
    }
}
