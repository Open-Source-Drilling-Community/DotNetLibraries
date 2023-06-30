using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Line3D : ICloneable
    {
        public Point3D ReferencePoint { get; set; } = new Point3D();
        public virtual Vector3D Direction { get; set; } = new Vector3D();

        public virtual bool IsNormalized { get; set; } = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Line3D()
        {

        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="line"></param>
        public Line3D(Line3D line)
        {
            if (line != null)
            {
                if (line.ReferencePoint != null)
                {
                    ReferencePoint.Set(line.ReferencePoint);
                    Direction.Set(line.Direction);
                    IsNormalized = line.IsNormalized;
                }
            }
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="direction"></param>
        /// <param name="isNormalized"></param>
        public Line3D(Point3D reference, Vector3D direction, bool isNormalized)
        {
            if (reference != null)
            {
                ReferencePoint.Set(reference);
            }
            if (direction != null)
            {
                Direction.Set(direction);
            }
            IsNormalized = isNormalized;
        }
        /// <summary>
        /// cloning
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return new Line3D(this);
        }
        /// <summary>
        /// ensure that the direction is normalized
        /// </summary>
        public void Normalize()
        {
            Direction.SetUnity();
            IsNormalized = true;
        }

        public bool IsColinear(Line3D line)
        {
            Point3D p = new Point3D(ReferencePoint);
            GetInterpolation(1, p);
            return Direction.IsColinear(line.Direction) && line.ReferencePoint.AreColinear(ReferencePoint, p);
        }

        public bool IsParallel(Line3D line)
        {
            return Direction.IsParallel(line.Direction);
        }

        /// <summary>
        /// calculate a point on the line: point = ReferencePoint + t*Direction;
        /// </summary>
        /// <param name="t"></param>
        /// <param name="point"></param>
        public void GetInterpolation(double t, IPoint3D point)
        {
            if (point != null)
            {
                if (!IsNormalized)
                {
                    Normalize();
                    IsNormalized = true;
                }
                point.Set(ReferencePoint.X + t * Direction.X, ReferencePoint.Y + t * Direction.Y, ReferencePoint.Z + t * Direction.Z);
            }
        }
        /// <summary>
        /// return, if it exists, the intersection between two lines
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public virtual Point3D GetIntersection(Line3D line)
        {
            double? parameterReference, parameterComparison;
            return GetIntersection(line, false, false, false, false, out parameterReference, out parameterComparison);
        }
        /// <summary>
        /// return, if it exists, the intersection between a line and a ray
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public virtual Point3D GetIntersection(Ray3D ray)
        {
            double? parameterReference, parameterComparison;
            return GetIntersection(ray, false, false, true, false, out parameterReference, out parameterComparison);
        }
        /// <summary>
        /// return, if it exists, the intersection between a line and a segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public virtual Point3D GetIntersection(Segment3D segment)
        {
            double? parameterReference, parameterComparison;
            Line3D line = new Line3D(segment);
            return GetIntersection(line, false, false, true, true, out parameterReference, out parameterComparison);
        }
        /// <summary>
        /// return, if it exists, the intersection between two lines
        /// </summary>
        /// <param name="line"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <returns></returns>
        public virtual Point3D GetIntersection(Line3D line, out double? parameterReference, out double? parameterComparison)
        {
            return GetIntersection(line, false, false, false, false, out parameterReference, out parameterComparison);
        }
        /// <summary>
        /// return, if it exists, the intersection between a line and a ray
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <returns></returns>
        public virtual Point3D GetIntersection(Ray3D ray, out double? parameterReference, out double? parameterComparison)
        {
            return GetIntersection(ray, false, false, true, false, out parameterReference, out parameterComparison);
        }
        /// <summary>
        /// return, if it exists, the intersection between a line and a segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <returns></returns>
        public virtual Point3D GetIntersection(Segment3D segment, out double? parameterReference, out double? parameterComparison)
        {
            return GetIntersection(segment, false, false, true, true, out parameterReference, out parameterComparison);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="lambdaPositive"></param>
        /// <param name="lambdaLessThanOne"></param>
        /// <param name="muPositive"></param>
        /// <param name="muLessThanOne"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <returns></returns>
        protected Point3D GetIntersection(Line3D line, bool lambdaPositive, bool lambdaLessThanOne, bool muPositive, bool muLessThanOne, out double? parameterReference, out double? parameterComparison)
        {
            parameterReference = null;
            parameterComparison = null;
            if (ReferencePoint != null && 
                Direction != null && 
                line != null && 
                line.ReferencePoint != null && 
                line.Direction != null)
            {
                if (line.Direction.X != null &&
                    line.Direction.Y != null &&
                    line.Direction.Z != null &&
                    line.ReferencePoint.X != null &&
                    line.ReferencePoint.Y != null &&
                    line.ReferencePoint.Z != null &&
                    ReferencePoint.X != null &&
                    ReferencePoint.Y != null &&
                    ReferencePoint.Z != null &&
                    Direction.X != null &&
                    Direction.Y != null &&
                    Direction.Z != null)
                {
                    double x0a = (double)ReferencePoint.X;
                    double x0b = (double)line.ReferencePoint.X;
                    double y0a = (double)ReferencePoint.Y;
                    double y0b = (double)line.ReferencePoint.Y;
                    double z0a = (double)ReferencePoint.Z;
                    double z0b = (double)line.ReferencePoint.Z;
                    double vxa = (double)Direction.X;
                    double vxb = (double)line.Direction.X;
                    double vya = (double)Direction.Y;
                    double vyb = (double)line.Direction.Y;
                    double vza = (double)Direction.Z;
                    double vzb = (double)line.Direction.Z;
                    double normA = System.Math.Sqrt(vxa * vxa + vya * vya + vza * vza);
                    if (Numeric.EQ(normA, 0))
                    {
                        return null;
                    }
                    double normB = System.Math.Sqrt(vxb * vxb + vyb * vyb + vzb * vzb);
                    if (Numeric.EQ(normB, 0))
                    {
                        return null;
                    }
                    vxa /= normA;
                    vya /= normA;
                    vza /= normA;
                    vxb /= normB;
                    vyb /= normB;
                    vzb /= normB;
                    // use the projection on the plane P(x,y) to calculate the intersection
                    double det = vya * vxb - vxa * vyb;
                    if (!Numeric.EQ(det, 0))
                    {
                        double lambda = (-(x0b - x0a) * vyb + (y0b - y0a) * vxb) / det;
                        double mu = ((y0b - y0a) * vxa - (x0b - x0a) * vya) / det;
                        // check if the last equation (in z) is true
                        if (Numeric.EQ(lambda * vza - mu * vzb + z0a - z0b, 0))
                        {
                            // then there is an intersection
                            if ((!muPositive || Numeric.GE(mu, 0)) && (!muLessThanOne || Numeric.LE(mu, normB)) && (!lambdaPositive || Numeric.GE(lambda, 0.0)) && (!lambdaLessThanOne || Numeric.LE(lambda, normA)))
                            {
                                parameterReference = lambda / normA;
                                parameterComparison = mu / normB;
                                return new Point3D(x0b + mu * vxb, y0b + mu * vyb, z0b + mu * vzb);
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
                        // use the projection on the plane P(x, z) to calculate the intersection
                        det = vza * vxb - vxa * vzb;
                        if (!Numeric.EQ(det, 0))
                        {
                            double lambda = (-(x0b - x0a) * vzb + (z0b - z0a) * vxb) / det;
                            double mu = ((z0b - z0a) * vxa - (x0b - x0a) * vza) / det;
                            // check if the last equation (in y) is true
                            if (Numeric.EQ(lambda * vya - mu * vyb + y0a - y0b, 0))
                            {
                                // then there is an intersection
                                if ((!muPositive || Numeric.GE(mu, 0)) && (!muLessThanOne || Numeric.LE(mu, normB)) && (!lambdaPositive || Numeric.GE(lambda, 0.0)) && (!lambdaLessThanOne || Numeric.LE(lambda, normA)))
                                {
                                    parameterReference = lambda / normA;
                                    parameterComparison = mu / normB;
                                    return new Point3D(x0b + mu * vxb, y0b + mu * vyb, z0b + mu * vzb);
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
                            // use the projection on the plane P(y,z) to calculate the intersection
                            det = vza * vyb - vya * vzb;
                            if (!Numeric.EQ(det, 0))
                            {
                                double lambda = (-(y0b - y0a) * vzb + (z0b - z0a) * vyb) / det;
                                double mu = ((z0b - z0a) * vya - (y0b - y0a) * vza) / det;
                                // check if the last equation (in x) is true
                                if (Numeric.EQ(lambda * vxa - mu * vxb + x0a - x0b, 0))
                                {
                                    // then there is an intersection
                                    if ((!muPositive || Numeric.GE(mu, 0)) && (!muLessThanOne || Numeric.LE(mu, normB)) && (!lambdaPositive || Numeric.GE(lambda, 0.0)) && (!lambdaLessThanOne || Numeric.LE(lambda, normA)))
                                    {
                                        parameterReference = lambda / normA;
                                        parameterComparison = mu / normB;
                                        return new Point3D(x0b + mu * vxb, y0b + mu * vyb, z0b + mu * vzb);
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
        /// <summary>
        /// return the distance between a line and a point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public virtual double? GetDistance(Point3D point)
        {
            double? parameter, toolface;
            bool isGravity;
            return GetDistance(point, false, false, out parameter, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between a line and a point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="parameter"></param>
        /// <param name="toolface"></param>
        /// <param name="isGravity"></param>
        /// <returns></returns>
        public virtual double? GetDistance(Point3D point, out double? parameter, out double? toolface, out bool isGravity)
        {
            return GetDistance(point, false, false, out parameter, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between a point and a line
        /// </summary>
        /// <param name="point"></param>
        /// <param name="parameter"></param>
        /// <param name="toolface"></param>
        /// <param name="isGravity"></param>
        /// <returns></returns>
        protected virtual double? GetDistance(Point3D point, bool lambdaPositive, bool lambdaLessThanOne, out double? parameter, out double? toolface, out bool isGravity)
        {
            parameter = null;
            toolface = null;
            isGravity = false;
            if (point != null &&
                point.X != null &&
                point.Y != null &&
                point.Z != null &&
                ReferencePoint.X != null &&
                ReferencePoint.Y != null &&
                ReferencePoint.Z != null &&
                Direction.X != null &&
                Direction.Y != null &&
                Direction.Z != null)
            {
                double x0 = (double)point.X;
                double y0 = (double)point.Y;
                double z0 = (double)point.Z;
                double x0a = (double)ReferencePoint.X;
                double y0a = (double)ReferencePoint.Y;
                double z0a = (double)ReferencePoint.Z;
                double vxa = (double)Direction.X;
                double vya = (double)Direction.Y;
                double vza = (double)Direction.Z;
                double normA = System.Math.Sqrt(vxa * vxa + vya * vya + vza * vza);
                if (Numeric.EQ(normA, 0))
                {
                    return null;
                }
                vxa /= normA;
                vya /= normA;
                vza /= normA;
                double lambda = (vxa * (x0 - x0a) + vya * (y0 - y0a) + vza * (z0 - z0a));
                if (lambdaPositive && Numeric.LT(lambda, 0))
                {
                    lambda = 0;
                }
                if (lambdaLessThanOne && Numeric.GT(lambda, normA))
                {
                    lambda = normA;
                }
                double xb = x0 + lambda * vxa;
                double yb = y0 + lambda * vya;
                double zb = z0 + lambda * vza;
                parameter = lambda * normA;
                Vector3D vec = new Vector3D(x0 - xb, y0 - yb, z0 - zb);
                toolface = vec.GetToolface(out isGravity);
                return System.Math.Sqrt((x0 - xb) * (x0 - xb) + (y0 - yb) * (y0 - yb) + (z0 - zb) * (z0 - zb));
            }
            else
            {
                return null;
            }
        }
         /// <summary>
        /// return the distance between two lines
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public virtual double? GetDistance(Line3D line)
        {
            double? toolface, parameter1, parameter2;
            bool isGravity;
            return GetDistance(line, false, false, false, false, out parameter1, out parameter2, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between two lines
        /// </summary>
        /// <param name="line"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <param name="toolface"></param>
        /// <param name="isGravity"></param>
        /// <returns></returns>
        public virtual double? GetDistance(Line3D line, out double? parameterReference, out double? parameterComparison, out double? toolface, out bool isGravity)
        {
            return GetDistance(line, false, false, false, false, out parameterReference, out parameterComparison, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between a line and a ray
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public virtual double? GetDistance(Ray3D ray)
        {
            double? parameterReference, parameterComparison, toolface;
            bool isGravity;
            return GetDistance(ray, false, false, true, false, out  parameterReference, out parameterComparison, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between a line and a ray
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <param name="toolface"></param>
        /// <param name="isGravity"></param>
        /// <returns></returns>
        public virtual double? GetDistance(Ray3D ray, out double? parameterReference, out double? parameterComparison, out double? toolface, out bool isGravity)
        {
            return GetDistance(ray, false, false, true, false, out parameterReference, out parameterComparison, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between a line and a segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public virtual double? GetDistance(Segment3D segment)
        {
            double? parameterReference, parameterComparison, toolface;
            bool isGravity;
            return GetDistance(segment, false, false, true, true, out parameterReference, out parameterComparison, out toolface, out isGravity);
        }
        /// <summary>
        /// return the distance between a line and a segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public virtual double? GetDistance(Segment3D segment, out double? parameterReference, out double? parameterComparison, out double? toolface, out bool isGravity)
        {
            return GetDistance(segment, false, false, true, true, out parameterReference, out parameterComparison, out toolface, out isGravity);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="lambdaPositive"></param>
        /// <param name="lambdaLessThanOne"></param>
        /// <param name="muPositive"></param>
        /// <param name="muLessThanOne"></param>
        /// <param name="parameterReference"></param>
        /// <param name="parameterComparison"></param>
        /// <param name="toolface"></param>
        /// <param name="isGravity"></param>
        /// <returns></returns>
        protected double? GetDistance(Line3D line, bool lambdaPositive, bool lambdaLessThanOne, bool muPositive, bool muLessThanOne, out double? parameterReference, out double? parameterComparison, out double? toolface, out bool isGravity)
        {
            parameterReference = null;
            parameterComparison = null;
            toolface = null;
            isGravity = false;
            if (line != null &&
                line.ReferencePoint != null &&
                line.Direction != null &&
                line.ReferencePoint.X != null &&
                line.ReferencePoint.Y != null &&
                line.ReferencePoint.Z != null &&
                line.Direction.X != null &&
                line.Direction.Y != null &&
                line.Direction.Z != null &&
                ReferencePoint.X != null &&
                ReferencePoint.Y != null &&
                ReferencePoint.Z != null &&
                Direction.X != null &&
                Direction.Y != null &&
                Direction.Z != null)
            {
                double x0a = (double)ReferencePoint.X;
                double y0a = (double)ReferencePoint.Y;
                double z0a = (double)ReferencePoint.Z;
                double vxa = (double)Direction.X;
                double vya = (double)Direction.Y;
                double vza = (double)Direction.Z;
                double x0b = (double)line.ReferencePoint.X;
                double y0b = (double)line.ReferencePoint.Y;
                double z0b = (double)line.ReferencePoint.Z;
                double vxb = (double)line.Direction.X;
                double vyb = (double)line.Direction.Y;
                double vzb = (double)line.Direction.Z;
                double normA = System.Math.Sqrt(vxa * vxa + vya * vya + vza * vza);
                if (Numeric.EQ(normA, 0))
                {
                    return null;
                }
                double normB = System.Math.Sqrt(vxb * vxb + vyb * vyb + vzb * vzb);
                if (Numeric.EQ(normB, 0))
                {
                    return null;
                }
                vxa /= normA;
                vya /= normA;
                vza /= normA;
                vxb /= normB;
                vyb /= normB;
                vzb /= normB;
                double a1 = -1;
                double b1 = vxa * vxb + vya * vyb + vza * vzb;
                double c1 = -vxa * (x0b - x0a) - vya * (y0b - y0a) - vza * (z0b - z0a);
                double a2 = -(vxa * vxb + vya * vyb + vza * vzb);
                double b2 = 1;
                double c2 = -vxb * (x0b - x0a) - vyb * (y0b - y0a) - vzb * (z0b - z0a);
                double det = a1 * b2 - a2 * b1;
                if (!Numeric.EQ(det, 0))
                {
                    double lambda = (b2 * c1 - b1 * c2) / det;
                    if (lambdaPositive && Numeric.LT(lambda, 0))
                    {
                        lambda = 0;
                    }
                    if (lambdaLessThanOne && Numeric.GT(lambda, normA))
                    {
                        lambda = normA;
                    }
                    double mu = (a1 * c2 - a2 * c1) / det;
                    if (muPositive && Numeric.LT(mu, 0))
                    {
                        mu = 0;
                    }
                    if (muLessThanOne && Numeric.GT(mu, normB))
                    {
                        mu = normB;
                    }
                    double xa = x0a + lambda * vxa;
                    double ya = y0a + lambda * vya;
                    double za = z0a + lambda * vza;
                    double xb = x0b + mu * vxb;
                    double yb = y0b + mu * vyb;
                    double zb = z0b + mu * vzb;
                    parameterReference = lambda / normA;
                    parameterComparison = mu / normB;
                    Vector3D vec = new Vector3D(xb - xa, yb - ya, zb - za);
                    toolface = vec.GetToolface(out isGravity);
                    return System.Math.Sqrt((xb - xa) * (xb - xa) + (yb - ya) * (yb - ya) + (zb - za) * (zb - za));
                }
                else
                {
                    if (Direction.IsColinear(line.Direction))
                    {
                        double denom = vxa * vxb + vya * vxb + vza * vzb;
                        if (!Numeric.EQ(denom, 0))
                        {
                            if (lambdaPositive && lambdaLessThanOne && muPositive && muLessThanOne)
                            {
                                // segment and segment
                                double mu0 = (vxa * (x0a - x0b) + vya * (y0a - y0b) + vza * (z0a - z0b)) / denom;
                                double x1a = x0a + vxa * normA;
                                double y1a = y0a + vya * normA;
                                double z1a = z0a + vza * normA;
                                double mu1 = (vxa * (x1a - x0b) + vya * (y1a - y0b) + vza * (z1a - z0b)) / denom;
                                double lambda0 = (vxb * (x0b - x0a) + vyb * (y0b - y0a) + vzb * (z0b - z0a)) / denom;
                                double x1b = x0b + vxb * normB;
                                double y1b = y0b + vyb * normB;
                                double z1b = z0b + vzb * normB;
                                double lambda1 = (vxb * (x1b - x0a) + vyb * (y1b - y0a) + vzb * (z1b - z0a)) / denom;
                                if ((Numeric.GE(mu0, 0) && Numeric.LE(mu0, normB)) || (Numeric.GE(mu1, 0) && Numeric.LE(mu1, normB)))
                                {
                                    double mu, xa, ya, za;
                                    if (Numeric.GE(mu0, 0) && Numeric.LE(mu0, normB))
                                    {
                                        mu = mu0;
                                        xa = x0a;
                                        ya = y0a;
                                        za = z0a;
                                    }
                                    else
                                    {
                                        mu = mu1;
                                        xa = x1a;
                                        ya = y1a;
                                        za = z1a;   
                                    }
                                    double xi = x0b + mu * vxb;
                                    double yi = y0b + mu * vyb;
                                    double zi = z0b + mu * vzb;
                                    double d = System.Math.Sqrt((xi - xa) * (xi - xa) + (yi - ya) * (yi - ya) + (zi - za) * (zi - za));
                                    parameterReference = 0;
                                    parameterComparison = mu / normB;
                                    Vector3D vec = new Vector3D(xi - xa, yi - ya, zi - za);
                                    toolface = vec.GetToolface(out isGravity);
                                    return d;
                                }
                                else if ((Numeric.GE(lambda0, 0) && Numeric.LE(lambda0, normA)) || (Numeric.GE(lambda1, 0) && Numeric.LE(lambda1, normA)))
                                {
                                    double lambda, xb, yb, zb;
                                    if (Numeric.GE(lambda0, 0) && Numeric.LE(lambda0, normA))
                                    {
                                        lambda = lambda0;
                                        xb = x0b;
                                        yb = y0b;
                                        zb = z0b;
                                    }
                                    else
                                    {
                                        lambda = lambda1;
                                        xb = x1b;
                                        yb = y1b;
                                        zb = z1b;
                                    }
                                    double xi = x0a + lambda * vxa;
                                    double yi = y0a + lambda * vya;
                                    double zi = z0a + lambda * vza;
                                    double d = System.Math.Sqrt((xi - xb) * (xi - xb) + (yi - yb) * (yi - yb) + (zi - zb) * (zi - zb));
                                    parameterReference = lambda / normA;
                                    parameterComparison = 0;
                                    Vector3D vec = new Vector3D(xb - xi, yb - yi, zb - zi);
                                    toolface = vec.GetToolface(out isGravity);
                                    return d;
                                }
                                else
                                {
                                    double d1 = System.Math.Sqrt((x0b - x0a) * (x0b - x0a) + (y0b - y0a) * (y0b - y0a) + (z0b - z0a) * (z0b - z0a));
                                    double d2 = System.Math.Sqrt((x0b - x1a) * (x0b - x1a) + (y0b - y1a) * (y0b - y1a) + (z0b - z1a) * (z0b - z1a));
                                    double d3 = System.Math.Sqrt((x1b - x0a) * (x1b - x0a) + (y1b - y0a) * (y1b - y0a) + (z1b - z0a) * (z1b - z0a));
                                    double d4 = System.Math.Sqrt((x1b - x1a) * (x1b - x1a) + (y1b - y1a) * (y1b - y1a) + (z1b - z1a) * (z1b - z1a));
                                    double min = System.Math.Min(System.Math.Min(d1, d2), System.Math.Min(d3, d4));
                                    if (Numeric.EQ(d1, min))
                                    {
                                        parameterReference = 0;
                                        parameterComparison = 0;
                                        Vector3D vec = new Vector3D(x0b - x0a, y0b - y0a, z0b - z0a);
                                        toolface = vec.GetToolface(out isGravity);
                                        return d1;
                                    }
                                    else if (Numeric.EQ(d2, min))
                                    {
                                        parameterReference = 1;
                                        parameterComparison = 0;
                                        Vector3D vec = new Vector3D(x0b - x1a, y0b - y1a, z0b - z1a);
                                        toolface = vec.GetToolface(out isGravity);
                                        return d2;
                                    }
                                    else if (Numeric.EQ(d3, min))
                                    {
                                        parameterReference = 0;
                                        parameterComparison = 1;
                                        Vector3D vec = new Vector3D(x1b - x0a, y1b - y0a, z1b - z0a);
                                        toolface = vec.GetToolface(out isGravity);
                                        return d3;
                                    }
                                    else
                                    {
                                        parameterReference = 1;
                                        parameterComparison = 1;
                                        Vector3D vec = new Vector3D(x1b - x1a, y1b - y1a, z1b - z1a);
                                        toolface = vec.GetToolface(out isGravity);
                                        return d4;
                                    }
                                }

                            }
                            else if (lambdaPositive && lambdaLessThanOne && muPositive && !muLessThanOne)
                            {
                                // segment and ray
                                double mu0 = (vxa * (x0a - x0b) + vya * (y0a - y0b) + vza * (z0a - z0b)) / denom;
                                double x1a = x0a + vxa * normA;
                                double y1a = y0a + vya * normA;
                                double z1a = z0a + vza * normA;
                                double mu1 = (vxa * (x1a - x0b) + vya * (y1a - y0b) + vza * (z1a - z0b)) / denom;
                                if (Numeric.GE(mu0, 0) || Numeric.GE(mu1, 0))
                                {
                                    double mu = (Numeric.GE(mu0, 0)) ? mu0 : mu1;
                                    double xi = x0b + mu * vxb;
                                    double yi = y0b + mu * vyb;
                                    double zi = z0b + mu * vzb;
                                    double d = System.Math.Sqrt((xi - x0a) * (xi - x0a) + (yi - y0a) * (yi - y0a) + (zi - z0a) * (zi - z0a));
                                    parameterReference = 0;
                                    parameterComparison = mu / normB;
                                    Vector3D vec = new Vector3D(xi - x0a, yi - y0a, zi - z0a);
                                    toolface = vec.GetToolface(out isGravity);
                                    return d;
                                }
                                else
                                {
                                    double d1 = System.Math.Sqrt((x0b - x0a) * (x0b - x0a) + (y0b - y0a) * (y0b - y0a) + (z0b - z0a) * (z0b - z0a));
                                    double d2 = System.Math.Sqrt((x0b - x1a) * (x0b - x1a) + (y0b - y1a) * (y0b - y1a) + (z0b - z1a) * (z0b - z1a));
                                    if (d1 < d2)
                                    {
                                        parameterReference = 0;
                                        parameterComparison = 0;
                                        Vector3D vec = new Vector3D(x0b - x0a, y0b - y0a, z0b - z0a);
                                        toolface = vec.GetToolface(out isGravity);
                                        return d1;
                                    }
                                    else
                                    {
                                        parameterReference = 1;
                                        parameterComparison = 0;
                                        Vector3D vec = new Vector3D(x0b - x1a, y0b - y1a, z0b - z1a);
                                        toolface = vec.GetToolface(out isGravity);
                                        return d2;
                                    }
                                }
                            }
                            else if (lambdaPositive && lambdaLessThanOne && !muPositive && !muLessThanOne)
                            {
                                // segment and line
                                double mu0 = (vxa * (x0a - x0b) + vya * (y0a - y0b) + vza * (z0a - z0b)) / denom;
                                double xi = x0b + mu0 * vxb;
                                double yi = y0b + mu0 * vyb;
                                double zi = z0b + mu0 * vzb;
                                double d = System.Math.Sqrt((xi - x0a) * (xi - x0a) + (yi - y0a) * (yi - y0a) + (zi - z0a) * (zi - z0a));
                                parameterReference = 0;
                                parameterComparison = mu0 / normB;
                                Vector3D vec = new Vector3D(xi - x0a, yi - y0a, zi - z0a);
                                toolface = vec.GetToolface(out isGravity);
                                return d;
                            }
                            else if (lambdaPositive && !lambdaLessThanOne && muPositive && muLessThanOne)
                            {
                                // ray and segment
                                double lambda0 = (vxb * (x0b - x0a) + vyb * (y0b - y0a) + vzb * (z0b - z0a)) / denom;
                                double x1b = x0b + vxb * normB;
                                double y1b = y0b + vyb * normB;
                                double z1b = z0b + vzb * normB;
                                double lambda1 = (vxb * (x1b - x0a) + vyb * (y1b - y0a) + vzb * (z1b - z0a)) / denom;
                                if (Numeric.GE(lambda0, 0) || Numeric.GE(lambda1, 0))
                                {
                                    double lambda = (Numeric.GE(lambda0, 0)) ? lambda0 : lambda1;
                                    double xi = x0a + lambda * vxa;
                                    double yi = y0a + lambda * vya;
                                    double zi = z0a + lambda * vza;
                                    double d = System.Math.Sqrt((xi - x0b) * (xi - x0b) + (yi - y0b) * (yi - y0b) + (zi - z0b) * (zi - z0b));
                                    parameterReference = lambda / normA;
                                    parameterComparison = 0;
                                    Vector3D vec = new Vector3D(x0b - xi, y0b - yi, z0b - zi);
                                    toolface = vec.GetToolface(out isGravity);
                                    return d;
                                }
                                else
                                {
                                    double d1 = System.Math.Sqrt((x0b - x0a) * (x0b - x0a) + (y0b - y0a) * (y0b - y0a) + (z0b - z0a) * (z0b - z0a));
                                    double d2 = System.Math.Sqrt((x1b - x0a) * (x1b - x0a) + (y1b - y0a) * (y1b - y0a) + (z1b - z0a) * (z1b - z0a));
                                    if (d1 < d2)
                                    {
                                        parameterReference = 0;
                                        parameterComparison = 0;
                                        Vector3D vec = new Vector3D(x0b - x0a, y0b - y0a, z0b - z0a);
                                        toolface = vec.GetToolface(out isGravity);
                                        return d1;
                                    }
                                    else
                                    {
                                        parameterReference = 0;
                                        parameterComparison = 1;
                                        Vector3D vec = new Vector3D(x1b - x0a, y1b - y0a, z1b - z0a);
                                        toolface = vec.GetToolface(out isGravity);
                                        return d2;
                                    }
                                }
                            }
                            else if (lambdaPositive && !lambdaLessThanOne && muPositive && !muLessThanOne)
                            {
                                // ray and ray
                                double mu0 = (vxa * (x0a - x0b) + vya * (y0a - y0b) + vza * (z0a - z0b)) / denom;
                                if (Numeric.GE(mu0, 0))
                                {
                                    double xi = x0b + mu0 * vxb;
                                    double yi = y0b + mu0 * vyb;
                                    double zi = z0b + mu0 * vzb;
                                    double d = System.Math.Sqrt((xi - x0a) * (xi - x0a) + (yi - y0a) * (yi - y0a) + (zi - z0a) * (zi - z0a));
                                    parameterReference = 0;
                                    parameterComparison = mu0 / normB;
                                    Vector3D vec = new Vector3D(xi - x0a, yi - y0a, zi - z0a);
                                    toolface = vec.GetToolface(out isGravity);
                                    return d;
                                }
                                else
                                {
                                    double lambda0 = (vxb * (x0b - x0a) + vyb * (y0b - y0a) + vzb * (z0b - z0a)) / denom;
                                    if (Numeric.GE(lambda0, 0))
                                    {
                                        double xi = x0a + lambda0 * vxa;
                                        double yi = y0a + lambda0 * vya;
                                        double zi = z0a + lambda0 * vza;
                                        double d = System.Math.Sqrt((xi - x0b) * (xi - x0b) + (yi - y0b) * (yi - y0b) + (zi - z0b) * (zi - z0b));
                                        parameterReference = lambda0 / normA;
                                        parameterComparison = 0;
                                        Vector3D vec = new Vector3D(x0b - xi, y0b - yi, z0b - zi);
                                        toolface = vec.GetToolface(out isGravity);
                                        return d;
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            }
                            else if (lambdaPositive && !lambdaLessThanOne && !muPositive && !muLessThanOne)
                            {
                                // ray and line
                                double mu0 = (vxa * (x0a - x0b) + vya * (y0a - y0b) + vza * (z0a - z0b)) / denom;
                                double xi = x0b + mu0 * vxb;
                                double yi = y0b + mu0 * vyb;
                                double zi = z0b + mu0 * vzb;
                                double d = System.Math.Sqrt((xi - x0a) * (xi - x0a) + (yi - y0a) * (yi - y0a) + (zi - z0a) * (zi - z0a));
                                parameterReference = 0;
                                parameterComparison = mu0 / normB;
                                Vector3D vec = new Vector3D(xi - x0a, yi - y0a, zi - z0a);
                                toolface = vec.GetToolface(out isGravity);
                                return d;
                            }
                            else if (!lambdaPositive && !lambdaLessThanOne && muPositive && muLessThanOne)
                            {
                                // line and segment
                                double lambda0 = (vxb * (x0b - x0a) + vyb * (y0b - y0a) + vzb * (z0b - z0a)) / denom;
                                double xi = x0a + lambda0 * vxa;
                                double yi = y0a + lambda0 * vya;
                                double zi = z0a + lambda0 * vza;
                                double d = System.Math.Sqrt((xi - x0b) * (xi - x0b) + (yi - y0b) * (yi - y0b) + (zi - z0b) * (zi - z0b));
                                parameterReference = lambda0 / normA;
                                parameterComparison = 0;
                                Vector3D vec = new Vector3D(x0b - xi, y0b - yi, z0b - zi);
                                toolface = vec.GetToolface(out isGravity);
                                return d;
                            }
                            else if (!lambdaPositive && !lambdaLessThanOne && muPositive && !muLessThanOne)
                            {
                                // line and ray
                                double lambda0 = (vxb * (x0b - x0a) + vyb * (y0b - y0a) + vzb * (z0b - z0a)) / denom;
                                double xi = x0a + lambda0 * vxa;
                                double yi = y0a + lambda0 * vya;
                                double zi = z0a + lambda0 * vza;
                                double d = System.Math.Sqrt((xi - x0b) * (xi - x0b) + (yi - y0b) * (yi - y0b) + (zi - z0b) * (zi - z0b));
                                parameterReference = lambda0 / normA;
                                parameterComparison = 0;
                                Vector3D vec = new Vector3D(x0b - xi, y0b - yi, z0b - zi);
                                toolface = vec.GetToolface(out isGravity);
                                return d;
                            }
                            else
                            {
                                // line and line
                                double mu0 = (vxa * (x0a - x0b) + vya * (y0a - y0b) + vza * (z0a - z0b)) / denom;
                                double xi = x0b + mu0 * vxb;
                                double yi = y0b + mu0 * vyb;
                                double zi = z0b + mu0 * vzb;
                                double d = System.Math.Sqrt((xi - x0a) * (xi - x0a) + (yi - y0a) * (yi - y0a) + (zi - z0a) * (zi - z0a));
                                parameterReference = 0;
                                parameterComparison = mu0 / normB;
                                Vector3D vec = new Vector3D(xi - x0a, yi - y0a, zi - z0a);
                                toolface = vec.GetToolface(out isGravity);
                                return d;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        // check if the two lines intersect
                        Point3D intersection = GetIntersection(line, out parameterReference, out parameterComparison);
                        if (intersection != null)
                        {
                            return 0;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            else
            {
                return null;
            }
        }

    }
}
