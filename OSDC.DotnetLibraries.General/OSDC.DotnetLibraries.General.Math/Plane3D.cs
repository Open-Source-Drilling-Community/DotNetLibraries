using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Plane3D
    {
        public double? A { get; set; }
        public double? B { get; set; }
        public double? C { get; set; }
        public double? D { get; set; }
        /// <summary>
        /// Default constructor
        /// </summary>
        public Plane3D()
        {

        }
        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="point"></param>
        /// <param name="normal"></param>
        public Plane3D(Point3D point, Vector3D normal)
        {
            if (point != null && normal != null)
            {
                A = normal.X;
                B = normal.Y;
                C = normal.Z;
                D = -(A * point.X + B * point.Y + C * point.Z);
            }
        }
        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public Plane3D(Point3D p1, Point3D p2, Point3D p3)
        {
            if (p1 != null && p2 != null && p3 != null)
            {
                Vector3D v1 = new Vector3D(p1, p2);
                Vector3D v2 = new Vector3D(p1, p3);
                Vector3D normal = v1.CrossProduct(v2);
                A = normal.X;
                B = normal.Y;
                C = normal.Z;
                D = -(A * p1.X + B * p1.Y + C * p1.Z);
            }
        }

        /// <summary>
        /// true if point is on the opposite side of the plane as defined by its normal.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsInside(Point3D point)
        {
            if (point == null)
            {
                return false;
            }
            else
            {
                double? d = A * point.X + B * point.Y + C * point.Z + D;
                return d <= 0;
            }
        }
    }
}
