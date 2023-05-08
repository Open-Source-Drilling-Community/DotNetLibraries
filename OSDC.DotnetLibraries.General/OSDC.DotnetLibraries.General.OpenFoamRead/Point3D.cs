using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.OpenFoamRead
{
    public class Point3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public Point3D() { }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Point3D(Point3D src)
        {
            if (src != null)
            {
                X = src.X;
                Y = src.Y;
                Z = src.Z;
            }
        }
        //initialization constructor
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
