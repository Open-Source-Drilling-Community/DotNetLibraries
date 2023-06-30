using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Point2D : IPoint2D
    {
        public double? X { get; set; } = null;
        public double? Y { get; set; } = null;
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
        /// Test equality with a numeric accuracy
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(IPoint2D cmp)
        {
            if (cmp == null)
            {
                return false;
            }
            return Numeric.EQ(X, cmp.X) && Numeric.EQ(Y, cmp.Y);
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

    }
}
