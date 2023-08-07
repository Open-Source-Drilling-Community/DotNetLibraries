using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Triangle3D
    {
        public Point3D Vertex1 { get; set; }
        public Point3D Vertex2 { get; set; }
        public Point3D Vertex3 { get; set; }
        /// <summary>
        /// default constructor
        /// </summary>
        public Triangle3D()
        {

        }
        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public Triangle3D(Point3D p1, Point3D p2, Point3D p3)
        {
            Vertex1 = p1;
            Vertex2 = p2;
            Vertex3 = p3;
        }
    }
}
