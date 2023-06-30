using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// a Ray is a semi line in the direction of the Direction vector
    /// </summary>
    public class Ray2D : Line2D
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public Ray2D() : base()
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Ray2D(Line2D src) : base(src)
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Ray2D(Ray2D src) : base((Line2D)src)
        {

        }
        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="vec"></param>
        public Ray2D(Point2D pt, Vector2D vec, bool isNormalized) : base(pt, vec, isNormalized)
        {

        }
        /// <summary>
        /// cloning
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new Ray2D(this);
        }

        public override Point2D GetIntersection(Segment2D segment)
        {
            if (ReferencePoint != null && Direction != null && segment != null && segment.ReferencePoint != null && segment.End != null)
            {
                Vector2D segmentDirection = segment.Direction;
                if (segmentDirection.X != null && segmentDirection.Y != null && segment.ReferencePoint.X != null && segment.ReferencePoint.Y != null &&
                    ReferencePoint.X != null && ReferencePoint.Y != null && Direction.X != null && Direction.Y != null)
                {
                    double x0a = (double)ReferencePoint.X;
                    double x0b = (double)segment.ReferencePoint.X;
                    double y0a = (double)ReferencePoint.Y;
                    double y0b = (double)segment.ReferencePoint.Y;
                    double vxa = (double)Direction.X;
                    double vxb = (double)segmentDirection.X;
                    double vya = (double)Direction.Y;
                    double vyb = (double)segmentDirection.Y;
                    double det = vya * vxb - vxa * vyb;
                    if (!Numeric.EQ(det, 0))
                    {
                        double lambda = (-(x0b - x0a) * vyb + (y0b - y0a) * vxb) / det;
                        double mu = ((y0b - y0a) * vxa - (x0b - x0a) * vya) / det;
                        if (Numeric.GE(mu, 0) && Numeric.LE(mu, 1.0) && Numeric.GE(lambda, 0))
                        {
                            return new Point2D(x0b + mu * vxb, y0b + mu * vyb);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
