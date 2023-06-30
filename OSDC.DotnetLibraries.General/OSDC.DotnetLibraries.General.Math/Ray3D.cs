using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Ray3D : Line3D, ICloneable
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public Ray3D() : base()
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Ray3D(Line3D src) : base(src)
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Ray3D(Ray3D src) : base((Line3D)src)
        {

        }
        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="vec"></param>
        public Ray3D(Point3D pt, Vector3D vec, bool isNormalized) : base(pt, vec, isNormalized)
        {

        }
        /// <summary>
        /// cloning
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new Ray3D(this);
        }
        /// <summary>
        /// return, if it exists, the intersection between two rays
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public override Point3D GetIntersection(Ray3D ray)
        {
            double? parameterReference, parameterComparison;
            return GetIntersection(ray, true, false, true, false, out parameterReference, out parameterComparison);
        }

        /// <summary>
        /// return, if it exists, the intersection between two rays
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <returns></returns>
        public override Point3D GetIntersection(Ray3D ray, out double? parameterReference, out double? parameterComparison)
        {
            return GetIntersection(ray, true, false, true, false, out parameterReference, out parameterComparison);
        }

        /// <summary>
        /// return, if it exists, the intersection between a ray and a segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public override Point3D GetIntersection(Segment3D segment)
        {
            double? parameterReference, parameterComparison;
            return GetIntersection(segment, true, false, true, true, out parameterReference, out parameterComparison);
        }
        /// <summary>
        /// return, if it exists, the intersection between a ray and a segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <returns></returns>
        public override Point3D GetIntersection(Segment3D segment, out double? parameterReference, out double? parameterComparison)
        {
            return GetIntersection(segment, true, false, true, false, out parameterReference, out parameterComparison);
        }
        /// <summary>
        /// return the distance between a ray and a point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override double? GetDistance(Point3D point)
        {
            double? parameter, toolface;
            bool isGravity;
            return GetDistance(point, true, false, out parameter, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between a ray and a point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="parameter"></param>
        /// <param name="toolface"></param>
        /// <param name="isGravity"></param>
        /// <returns></returns>
        public override double? GetDistance(Point3D point, out double? parameter, out double? toolface, out bool isGravity)
        {
            return GetDistance(point, true, false, out parameter, out toolface, out isGravity);
        }

        /// <summary>
        /// return the distance between two rays
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public override double? GetDistance(Ray3D ray)
        {
            double? parameterReference, parameterComparison, toolface;
            bool isGravity;
            return GetDistance(ray, true, false, true, false, out parameterReference, out parameterComparison, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between two rays
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <param name="toolface"></param>
        /// <param name="isGravity"></param>
        /// <returns></returns>
        public override double? GetDistance(Ray3D ray, out double? parameterReference, out double? parameterComparison, out double? toolface, out bool isGravity)
        {
            return GetDistance(ray, true, false, true, false, out parameterReference, out parameterComparison, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between a ray and a segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public override double? GetDistance(Segment3D segment)
        {
            double? parameterReference, parameterComparison, toolface;
            bool isGravity;
            return GetDistance(segment, true, false, true, true, out parameterReference, out parameterComparison, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between a ray and a segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <param name="toolface"></param>
        /// <param name="isGravity"></param>
        /// <returns></returns>
        public override double? GetDistance(Segment3D segment, out double? parameterReference, out double? parameterComparison, out double? toolface, out bool isGravity)
        {
            return GetDistance(segment, true, false, true, true, out parameterReference, out parameterComparison, out toolface, out isGravity);
        }
    }
}
