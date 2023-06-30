using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Segment3D : Ray3D
    {
        public Point3D End { get; set; } = null;

        /// <summary>
        /// Direction is now calculated from ReferencePoint and End. If the Direction is provide, then the End is modified accordingly.
        /// </summary>
        public override Vector3D Direction
        {
            get
            {
                if (ReferencePoint != null && End != null)
                {
                    return new Vector3D(End.X - ReferencePoint.X, End.Y - ReferencePoint.Y, End.Z - ReferencePoint.Z);
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
                    if (End == null)
                    {
                        End = new Point3D();
                    }
                    End.Set(ReferencePoint.X + value.X, ReferencePoint.Y + value.Y, ReferencePoint.Z + value.Z);
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
        public Segment3D() : base()
        {
        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Segment3D(Segment3D src) : base()
        {
            if (src != null)
            {
                if (src.ReferencePoint != null)
                {
                    ReferencePoint = new Point3D(src.ReferencePoint);
                }
                if (src.End != null)
                {
                    End = new Point3D(src.End);
                }
                if (src.Direction != null)
                {
                    Direction = new Vector3D(src.Direction);
                }
                IsNormalized = src.IsNormalized;
            }
        }
        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Segment3D(Point3D start, Point3D end) : base()
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
            return new Segment3D(this);
        }
        /// <summary>
        /// return, if it exists, the intersection between two segments
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public override Point3D GetIntersection(Segment3D segment)
        {
            double? parameterReference, parameterComparison;
            return GetIntersection(segment, true, true, true, true, out parameterReference, out parameterComparison);
        }
        /// <summary>
        /// return, if it exists, the intersection between two segments
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <returns></returns>
        public override Point3D GetIntersection(Segment3D segment, out double? parameterReference, out double? parameterComparison)
        {
            return GetIntersection(segment, true, true, true, true, out parameterReference, out parameterComparison);
        }
        /// <summary>
        /// return the distance between a segment and a point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override double? GetDistance(Point3D point)
        {
            double? parameter, toolface;
            bool isGravity;
            return GetDistance(point, true, true, out parameter, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between a segment and a point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="parameter"></param>
        /// <param name="toolface"></param>
        /// <param name="isGravity"></param>
        /// <returns></returns>
        public override double? GetDistance(Point3D point, out double? parameter, out double? toolface, out bool isGravity)
        {
            return GetDistance(point, true, true, out parameter, out toolface, out isGravity);
        }

        /// <summary>
        /// return the distance between a two segments
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public override double? GetDistance(Segment3D segment)
        {
            double? parameterReference, parameterComparison, toolface;
            bool isGravity;
            return GetDistance(segment, true, true, true, true, out parameterReference, out parameterComparison, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between a two segments
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <param name="toolface"></param>
        /// <param name="isGravity"></param>
        /// <returns></returns>
        public override double? GetDistance(Segment3D segment, out double? parameterReference, out double? parameterComparison, out double? toolface, out bool isGravity)
        {
            return GetDistance(segment, true, true, true, true, out parameterReference, out parameterComparison, out toolface, out isGravity);
        }

    }
}
