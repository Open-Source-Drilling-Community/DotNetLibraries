using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// A class for 3D Line segments from ReferencePointA to ReferencePointB
    /// </summary>
    public class LineSegment3D
    {
        public Point3D ReferencePointA { get; set; } = new Point3D();
        public Point3D ReferencePointB { get; set; } = new Point3D();

        /// <summary>
        /// Default constructor
        /// </summary>
        public LineSegment3D()
        {

        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="lineSegment"></param>
        public LineSegment3D(LineSegment3D lineSegment)
        {
            if (lineSegment != null)
            {
                if (lineSegment.ReferencePointA != null && lineSegment.ReferencePointB != null)
                {
                    ReferencePointA.Set(lineSegment.ReferencePointA);
                    ReferencePointB.Set(lineSegment.ReferencePointB);
                }
            }
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="referenceA"></param>
        /// <param name="referenceB"></param>
        public LineSegment3D(Point3D referenceA, Point3D referenceB)
        {
            if (referenceA != null)
            {
                ReferencePointA.Set(referenceA);
            }
            if (referenceB != null)
            {
                ReferencePointB.Set(referenceB);
            }
        }

        /// <summary>
        /// Calculate a point on the line: point = ReferencePointA + t*(ReferencePointB - ReferencePointA);
        /// </summary>
        /// <param name="t"></param>
        /// <param name="point"></param>
        public void GetInterpolation(double t, IPoint3D point)
        {
            if (point != null && t <= 1.0 && t >= 0.0)
            {
                point.Set(ReferencePointA.X + t * (ReferencePointB.X - ReferencePointA.X), ReferencePointA.Y + t * (ReferencePointB.Y - ReferencePointA.Y), ReferencePointA.Z + t * (ReferencePointB.Z - ReferencePointA.Z));
            }
        }

        /// <summary>
        /// Check if the point is on the line segment
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Includes(Point3D point)
        {
            if (point.IsUndefined() || ReferencePointA.IsUndefined() || ReferencePointB.IsUndefined())
            {
                return false;
            }
            if (point.AreColinear(ReferencePointA, ReferencePointB))
            {
                double? t = (point.X - ReferencePointA.X) / (ReferencePointB.X - ReferencePointA.X);
                if (t != null)
                {
                    if (Numeric.IsUndefined(t))
                    {
                        t = (point.Y - ReferencePointA.Y) / (ReferencePointB.Y - ReferencePointA.Y);
                        if (t != null)
                        {
                            if (Numeric.IsUndefined(t))
                            {
                                t = (point.Z - ReferencePointA.Z) / (ReferencePointB.Z - ReferencePointA.Z);
                                if (t != null)
                                {
                                    if (Numeric.IsUndefined(t))
                                    {
                                        if (point.EQ(ReferencePointA) && point.EQ(ReferencePointB))
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                    else if (t <= 1.0 && t >= 0.0)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if (t <= 1.0 && t >= 0.0)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (t <= 1.0 && t >= 0.0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool IntersectsTriangle(Triangle3D triangle)
        {
            if (triangle == null)
            {
                return false;
            }
            double? svA = SignedVolumeTetrahedron(ReferencePointA, triangle.Vertex1, triangle.Vertex2, triangle.Vertex3);
            double? svB = SignedVolumeTetrahedron(ReferencePointB, triangle.Vertex1, triangle.Vertex2, triangle.Vertex3);
            if (svA != null && svB != null)
            {
                if (System.Math.Sign((double)svA) == 0 && System.Math.Sign((double)svB) == 0)
                {
                    // Both the volumes are zero - Both points are in the plane of the triangle (coplanar) - Determine if they are inside or if the line crosses
                    bool referencePointsInside = ReferencePointA.IsInsideTriangle(triangle) || ReferencePointB.IsInsideTriangle(triangle);
                    if (referencePointsInside)
                    {
                        // One or both the reference points are within the triangle
                        return true;
                    }
                    else
                    {
                        // Check if the line crosses any of the line segments of the triangle
                        // First check if they are parallel (dot product of normal vectors)
                        Vector3D side1 = new Vector3D(triangle.Vertex1, triangle.Vertex2);
                        Vector3D side2 = new Vector3D(triangle.Vertex2, triangle.Vertex3);
                        Vector3D side3 = new Vector3D(triangle.Vertex3, triangle.Vertex1);
                        Vector3D line = new Vector3D(ReferencePointA, ReferencePointB);
                        if (side1.IsColinear(line) && ReferencePointA.AreColinear(triangle.Vertex1, triangle.Vertex2))
                        {
                            // Return true if line segments overlap
                            LineSegment3D segment1 = new LineSegment3D(triangle.Vertex1, triangle.Vertex2);
                            if (this.Includes(segment1.ReferencePointA) || this.Includes(segment1.ReferencePointB) || segment1.Includes(ReferencePointA) || segment1.Includes(ReferencePointB))
                            {
                                // At least one of the reference points of one segment is included in the other segment
                                return true;
                            }
                            else
                            {
                                // Line segments do not overlap
                            }
                        }
                        else if (side1.IsParallel(line))
                        {
                            // Line segments will not cross
                        }
                        else
                        {
                            // Check for intersection
                            bool sameSideRef = Point3D.SameSide(ReferencePointA, ReferencePointB, triangle.Vertex1, triangle.Vertex2);
                            bool sameSideVert = Point3D.SameSide(triangle.Vertex1, triangle.Vertex2, ReferencePointA, ReferencePointB);
                            if (!sameSideRef && !sameSideVert)
                            {
                                // The segments are intersecting since the reference points of each segment are on opposite sides of the line of the other segment
                                // Then we know that the line segment intersects the triangle
                                return true;
                            }
                            else
                            {
                                // The segments are not intersecting since the line from one of the segments is not crossing the other segment
                            }
                        }
                        if (side2.IsColinear(line) && ReferencePointA.AreColinear(triangle.Vertex2, triangle.Vertex3))
                        {
                            // Return true if line segments overlap
                            LineSegment3D segment2 = new LineSegment3D(triangle.Vertex2, triangle.Vertex3);
                            if (this.Includes(segment2.ReferencePointA) || this.Includes(segment2.ReferencePointB) || segment2.Includes(ReferencePointA) || segment2.Includes(ReferencePointB))
                            {
                                // At least one of the reference points of one segment is included in the other segment
                                return true;
                            }
                            else
                            {
                                // Line segments do not overlap
                            }
                        }
                        else if (side2.IsParallel(line))
                        {
                            // Line segments will not cross
                        }
                        else
                        {
                            // Check for intersection
                            bool sameSideRef = Point3D.SameSide(ReferencePointA, ReferencePointB, triangle.Vertex2, triangle.Vertex3);
                            bool sameSideVert = Point3D.SameSide(triangle.Vertex2, triangle.Vertex3, ReferencePointA, ReferencePointB);
                            if (!sameSideRef && !sameSideVert)
                            {
                                // The segments are intersecting since the reference points of each segment are on opposite sides of the line of the other segment
                                // Then we know that the line segment intersects the triangle
                                return true;
                            }
                            else
                            {
                                // The segments are not intersecting since the line from one of the segments is not crossing the other segment
                            }
                        }
                        if (side3.IsColinear(line) && ReferencePointA.AreColinear(triangle.Vertex3, triangle.Vertex1))
                        {
                            // Return true if line segments overlap
                            LineSegment3D segment3 = new LineSegment3D(triangle.Vertex3, triangle.Vertex1);
                            if (this.Includes(segment3.ReferencePointA) || this.Includes(segment3.ReferencePointB) || segment3.Includes(ReferencePointA) || segment3.Includes(ReferencePointB))
                            {
                                // At least one of the reference points of one segment is included in the other segment
                                return true;
                            }
                            else
                            {
                                // Line segments do not overlap
                            }
                        }
                        else if (side3.IsParallel(line))
                        {
                            // Line segments will not cross
                        }
                        else
                        {
                            // Check for intersection
                            bool sameSideRef = Point3D.SameSide(ReferencePointA, ReferencePointB, triangle.Vertex3, triangle.Vertex1);
                            bool sameSideVert = Point3D.SameSide(triangle.Vertex3, triangle.Vertex1, ReferencePointA, ReferencePointB);
                            if (!sameSideRef && !sameSideVert)
                            {
                                // The segments are intersecting since the reference points of each segment are on opposite sides of the line of the other segment
                                // Then we know that the line segment intersects the triangle
                                return true;
                            }
                            else
                            {
                                // The segments are not intersecting since the line from one of the segments is not crossing the other segment
                            }
                        }
                        return false;
                    }
                }
                else if (System.Math.Sign((double)svA) == 0)
                {
                    // ReferencePointA is in the plane of the triangle - Determine if it is inside
                    return ReferencePointA.IsInsideTriangle(triangle);
                }
                else if (System.Math.Sign((double)svB) == 0)
                {
                    // ReferencePointB is in the plane of the triangle - Determine if it is inside
                    return ReferencePointB.IsInsideTriangle(triangle);
                }
                else if (System.Math.Sign((double)svA) != System.Math.Sign((double)svB))
                {
                    // The reference points are on the opposite sides of the triangle's plane
                    // Check if the intersection is within the triangle
                    double? sv12 = SignedVolumeTetrahedron(ReferencePointA, ReferencePointB, triangle.Vertex1, triangle.Vertex2);
                    double? sv23 = SignedVolumeTetrahedron(ReferencePointA, ReferencePointB, triangle.Vertex2, triangle.Vertex3);
                    double? sv31 = SignedVolumeTetrahedron(ReferencePointA, ReferencePointB, triangle.Vertex3, triangle.Vertex1);
                    if (sv12 != null && sv23 != null && sv31 != null)
                    {
                        if (System.Math.Sign((double)sv12) == 0 && System.Math.Sign((double)sv23) == 0 && System.Math.Sign((double)sv31) == 0)
                        {
                            // All the volumes are zero - The line segment and the triangle are coplanar, but do they intersect?
                            // This should not happen since we check if we have coplanar results above (svA == svB == 0)
                        }
                        else if ((System.Math.Sign((double)sv12) != 0 && System.Math.Sign((double)sv23) == 0 || System.Math.Sign((double)sv31) == 0)
                            || (System.Math.Sign((double)sv23) != 0 && System.Math.Sign((double)sv31) == 0 || System.Math.Sign((double)sv12) == 0)
                            || (System.Math.Sign((double)sv31) != 0 && System.Math.Sign((double)sv12) == 0 || System.Math.Sign((double)sv23) == 0))
                        {
                            // Two of the volumes are zero - intersection at a vertex
                            return true;
                        }
                        else if ((System.Math.Sign((double)sv12) == 0 && System.Math.Sign((double)sv23) == System.Math.Sign((double)sv31))
                            || (System.Math.Sign((double)sv23) == 0 && System.Math.Sign((double)sv31) == System.Math.Sign((double)sv12))
                            || (System.Math.Sign((double)sv31) == 0 && System.Math.Sign((double)sv12) == System.Math.Sign((double)sv23)))
                        {
                            // One of the volumes are zero and the other two have the same sign - intersection at an edge
                            return true;
                        }
                        if (System.Math.Sign((double)sv12) == System.Math.Sign((double)sv23) && System.Math.Sign((double)sv12) == System.Math.Sign((double)sv31))
                        {
                            // All volumes have the same sign, which means that the intersection is within the triangle
                            return true;
                        }
                        else
                        {
                            // No intersection
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    // The reference points are on the same side of the triangle's plane
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        double? SignedVolumeTetrahedron(Point3D a, Point3D b, Point3D c, Point3D d)
        {
            Vector3D v1 = new Vector3D(a, b);
            Vector3D v2 = new Vector3D(a, c);
            Vector3D v3 = new Vector3D(a, d);
            Vector3D normal = v1.CrossProduct(v2);
            double? det = normal.Dot(v3);
            if (det != null)
                return det / 6.0;
            else
                return null;
        }
    }
}
