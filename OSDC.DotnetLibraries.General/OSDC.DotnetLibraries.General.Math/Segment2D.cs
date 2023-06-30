using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Segment2D : Ray2D
    {
        public Point2D End { get; set; } = null;

        /// <summary>
        /// Direction is now calculated from ReferencePoint and End. If the Direction is provide, then the End is modified accordingly.
        /// </summary>
        public override Vector2D Direction
        {
            get
            {
                if (ReferencePoint != null && End != null)
                {
                    return new Vector2D(End.X - ReferencePoint.X, End.Y - ReferencePoint.Y);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (ReferencePoint != null && value != null)
                {
                    End.Set(ReferencePoint.X + value.X, ReferencePoint.Y + value.Y);
                }
            }
        }
        /// <summary>
        /// always false
        /// </summary>
        public override bool IsNormalized
        {
            get => false;
            set
            {
            }
        }
        /// <summary>
        /// default constructor
        /// </summary>
        public Segment2D() : base()
        {
        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Segment2D(Segment2D src) : base()
        {
            if (src != null)
            {
                if (src.ReferencePoint != null)
                {
                    ReferencePoint = new Point2D(src.ReferencePoint);
                }
                if (src.End != null)
                {
                    End = new Point2D(src.End);
                }
                if (src.Direction != null)
                {
                    Direction = new Vector2D(src.Direction);
                }
                IsNormalized = src.IsNormalized;
            }
        }
        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Segment2D(Point2D start, Point2D end) : base()
        {
            ReferencePoint = start;
            End = end;
        }
        /// <summary>
        /// Cloning
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new Segment2D(this);
        }

        public override Point2D GetIntersection(Segment2D segment)
        {
            if (ReferencePoint != null && End != null && segment != null && segment.ReferencePoint != null && segment.End != null)
            {
                Vector2D segmentDirection = segment.Direction;
                Vector2D direction = Direction;
                if (segmentDirection.X != null && segmentDirection.Y != null && segment.ReferencePoint.X != null && segment.ReferencePoint.Y != null &&
                    ReferencePoint.X != null && ReferencePoint.Y != null && direction.X != null && direction.Y != null)
                {
                    double x0a = (double)ReferencePoint.X;
                    double x0b = (double)segment.ReferencePoint.X;
                    double y0a = (double)ReferencePoint.Y;
                    double y0b = (double)segment.ReferencePoint.Y;
                    double vxa = (double)direction.X;
                    double vxb = (double)segmentDirection.X;
                    double vya = (double)direction.Y;
                    double vyb = (double)segmentDirection.Y;
                    double det = vya * vxb - vxa * vyb;
                    if (!Numeric.EQ(det, 0))
                    {
                        double lambda = (-(x0b - x0a) * vyb + (y0b - y0a) * vxb) / det;
                        double mu = ((y0b - y0a) * vxa - (x0b - x0a) * vya) / det;
                        if (Numeric.GE(mu, 0) && Numeric.LE(mu, 1.0) && Numeric.GE(lambda, 0) && Numeric.LE(lambda, 1.0))
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
