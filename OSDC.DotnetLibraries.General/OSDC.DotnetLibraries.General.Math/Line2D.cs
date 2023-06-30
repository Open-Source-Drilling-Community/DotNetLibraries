using OSDC.DotnetLibraries.General.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Line2D : ICloneable
    {
        public Point2D ReferencePoint { get; set; } = null;
        public virtual Vector2D Direction { get; set; } = null;

        public virtual bool IsNormalized { get; set; } = false;
        /// <summary>
        /// Default constructor
        /// </summary>
        public Line2D()
        {

        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="line"></param>
        public Line2D(Line2D line)
        {
            if (line != null)
            {
                if (line.ReferencePoint != null)
                {
                    ReferencePoint = new Point2D(line.ReferencePoint);
                }
                if (line.Direction != null)
                {
                    Direction = new Vector2D(line.Direction);
                }
                IsNormalized = line.IsNormalized;
            }
        }
        /// <summary>
        /// initialization constructor
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="direction"></param>
        /// <param name="isNormalized"></param>
        public Line2D(Point2D reference, Vector2D direction, bool isNormalized)
        {
            if (reference != null)
            {
                ReferencePoint = reference;
            }
            if (direction != null)
            {
                Direction = direction;
            }
            IsNormalized = isNormalized;
        }

        /// <summary>
        /// cloning
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return new Line2D(this);
        }

        public virtual Point2D GetIntersection(Segment2D segment)
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
                        if (Numeric.GE(mu, 0) && Numeric.LE(mu, 1.0))
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
