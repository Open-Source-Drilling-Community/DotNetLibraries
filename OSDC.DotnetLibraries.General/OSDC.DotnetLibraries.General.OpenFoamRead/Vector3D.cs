using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.OpenFoamRead
{
    public  class Vector3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public Vector3D() { }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Vector3D(Vector3D src)
        {
            if (src != null)
            {
                X = src.X;
                Y = src.Y;
                Z = src.Z;
            }
        }
        //initialization constructor
        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

    }
}
