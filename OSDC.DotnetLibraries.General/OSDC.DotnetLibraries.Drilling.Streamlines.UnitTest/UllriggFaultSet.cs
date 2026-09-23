using System.Globalization;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// A fault as the examples give it: a set of pillars, each a stick of points ordered from the top
    /// down, consecutive pillars running along strike so that the surface is the ruled surface between
    /// them.
    /// <para>
    /// This lives with the tests rather than in the library because nothing in the chain treats a fault
    /// as a constraint yet. It is here to choose which faults are worth looking at for a case and to
    /// hand them to the view.
    /// </para>
    /// </summary>
    public class FaultSurface
    {
        /// <summary>
        /// the name the file carries, which is also the file's own name
        /// </summary>
        public string? Name { get; set; } = null;

        /// <summary>
        /// the pillars, in the order the file gives them, each ordered from the top down
        /// </summary>
        public List<List<Point3D>> Pillars { get; } = new List<List<Point3D>>();

        /// <summary>
        /// the lowest corner of the box holding the whole fault
        /// </summary>
        public double[] BoundingBoxMinimum { get; }
            = { double.MaxValue, double.MaxValue, double.MaxValue };

        /// <summary>
        /// the highest corner of that box
        /// </summary>
        public double[] BoundingBoxMaximum { get; }
            = { double.MinValue, double.MinValue, double.MinValue };

        /// <summary>
        /// takes a point into the fault, keeping the bounding box up to date
        /// </summary>
        public void Add(int pillar, Point3D at)
        {
            while (Pillars.Count <= pillar)
            {
                Pillars.Add(new List<Point3D>());
            }
            Pillars[pillar].Add(at);
            double[] position = { at.X!.Value, at.Y!.Value, at.Z!.Value };
            for (int a = 0; a < 3; a++)
            {
                if (position[a] < BoundingBoxMinimum[a]) { BoundingBoxMinimum[a] = position[a]; }
                if (position[a] > BoundingBoxMaximum[a]) { BoundingBoxMaximum[a] = position[a]; }
            }
        }

        /// <summary>
        /// how many points the fault holds in all
        /// </summary>
        public int PointCount
        {
            get
            {
                int count = 0;
                foreach (List<Point3D> pillar in Pillars) { count += pillar.Count; }
                return count;
            }
        }

        /// <summary>
        /// Whether any of the fault lies in the given box.
        /// <para>
        /// The whole fault is rejected on its own bounding box first, and what survives is tested point
        /// by point and then along each pillar, so a stick that passes through the box without a point
        /// inside it still counts. What this does not catch is a fault that crosses the box only
        /// between two pillars, with neither of them entering it: the pillars of these examples are a
        /// median of 64 m apart and 105 m at the ninetieth percentile, so that needs a box narrower
        /// than the sticks are spaced.
        /// </para>
        /// </summary>
        public bool Intersects(double[] minimum, double[] maximum)
        {
            for (int a = 0; a < 3; a++)
            {
                if (BoundingBoxMaximum[a] < minimum[a] || BoundingBoxMinimum[a] > maximum[a])
                {
                    return false;
                }
            }
            foreach (List<Point3D> pillar in Pillars)
            {
                for (int k = 0; k < pillar.Count; k++)
                {
                    if (Contains(minimum, maximum, pillar[k]))
                    {
                        return true;
                    }
                    if (k > 0 && Crosses(minimum, maximum, pillar[k - 1], pillar[k]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool Contains(double[] minimum, double[] maximum, Point3D at)
        {
            double[] position = { at.X!.Value, at.Y!.Value, at.Z!.Value };
            for (int a = 0; a < 3; a++)
            {
                if (position[a] < minimum[a] || position[a] > maximum[a]) { return false; }
            }
            return true;
        }

        /// <summary>
        /// whether a segment meets the box, by the slab method: the segment is clipped against each
        /// pair of faces in turn and what is left is the stretch of it inside
        /// </summary>
        private static bool Crosses(double[] minimum, double[] maximum, Point3D from, Point3D to)
        {
            double[] start = { from.X!.Value, from.Y!.Value, from.Z!.Value };
            double[] direction = { to.X!.Value - start[0], to.Y!.Value - start[1],
                                   to.Z!.Value - start[2] };
            double entering = 0;
            double leaving = 1;
            for (int a = 0; a < 3; a++)
            {
                if (System.Math.Abs(direction[a]) < 1.0e-12)
                {
                    if (start[a] < minimum[a] || start[a] > maximum[a]) { return false; }
                    continue;
                }
                double first = (minimum[a] - start[a]) / direction[a];
                double second = (maximum[a] - start[a]) / direction[a];
                if (first > second) { (first, second) = (second, first); }
                if (first > entering) { entering = first; }
                if (second < leaving) { leaving = second; }
                if (entering > leaving) { return false; }
            }
            return true;
        }

        /// <summary>
        /// One pillar resampled to a fixed number of points by arc length, so that consecutive pillars
        /// can be joined into the quadrilaterals that stand for the surface. The pillars of a fault do
        /// not hold the same number of points, and without this there is nothing to join.
        /// </summary>
        public List<Point3D> GetResampledPillar(int pillar, int count)
        {
            List<Point3D> points = Pillars[pillar];
            List<Point3D> resampled = new List<Point3D>(count);
            if (points.Count == 0 || count < 2)
            {
                return resampled;
            }
            if (points.Count == 1)
            {
                for (int k = 0; k < count; k++) { resampled.Add(new Point3D(points[0])); }
                return resampled;
            }
            double[] along = new double[points.Count];
            for (int k = 1; k < points.Count; k++)
            {
                along[k] = along[k - 1] + Distance(points[k - 1], points[k]);
            }
            double total = along[points.Count - 1];
            if (!(total > 0))
            {
                for (int k = 0; k < count; k++) { resampled.Add(new Point3D(points[0])); }
                return resampled;
            }
            int segment = 0;
            for (int k = 0; k < count; k++)
            {
                double wanted = total * k / (count - 1);
                while (segment < points.Count - 2 && along[segment + 1] < wanted) { segment++; }
                double span = along[segment + 1] - along[segment];
                double share = span > 0 ? (wanted - along[segment]) / span : 0;
                resampled.Add(new Point3D(
                    points[segment].X!.Value
                        + share * (points[segment + 1].X!.Value - points[segment].X!.Value),
                    points[segment].Y!.Value
                        + share * (points[segment + 1].Y!.Value - points[segment].Y!.Value),
                    points[segment].Z!.Value
                        + share * (points[segment + 1].Z!.Value - points[segment].Z!.Value)));
            }
            return resampled;
        }

        private double[]? triangles_ = null;
        private int triangleSamples_ = 0;

        /// <summary>
        /// The fault as triangles: nine numbers each, three corners in order.
        /// <para>
        /// The surface is the ruled surface between consecutive pillars, so each pair of pillars is
        /// resampled to a common count and the quadrilaterals between them are cut in two. The result
        /// is held, because a path is tested against the same fault once per corridor.
        /// </para>
        /// </summary>
        public double[] GetTriangles(int samples)
        {
            if (triangles_ != null && triangleSamples_ == samples)
            {
                return triangles_;
            }
            List<double> built = new List<double>();
            List<List<Point3D>> grid = new List<List<Point3D>>();
            for (int p = 0; p < Pillars.Count; p++)
            {
                List<Point3D> even = GetResampledPillar(p, samples);
                if (even.Count == samples) { grid.Add(even); }
            }
            for (int k = 0; k + 1 < grid.Count; k++)
            {
                for (int j = 0; j + 1 < samples; j++)
                {
                    Add(built, grid[k][j], grid[k + 1][j], grid[k + 1][j + 1]);
                    Add(built, grid[k][j], grid[k + 1][j + 1], grid[k][j + 1]);
                }
            }
            triangles_ = built.ToArray();
            triangleSamples_ = samples;
            return triangles_;
        }

        private static void Add(List<double> into, Point3D a, Point3D b, Point3D c)
        {
            into.Add(a.X!.Value); into.Add(a.Y!.Value); into.Add(a.Z!.Value);
            into.Add(b.X!.Value); into.Add(b.Y!.Value); into.Add(b.Z!.Value);
            into.Add(c.X!.Value); into.Add(c.Y!.Value); into.Add(c.Z!.Value);
        }

        private static double Distance(Point3D from, Point3D to)
        {
            double dn = to.X!.Value - from.X!.Value;
            double de = to.Y!.Value - from.Y!.Value;
            double dv = to.Z!.Value - from.Z!.Value;
            return System.Math.Sqrt(dn * dn + de * de + dv * dv);
        }
    }

    /// <summary>
    /// One place where a path goes through a fault.
    /// </summary>
    public readonly struct FaultCrossing
    {
        /// <summary>
        /// the fault gone through
        /// </summary>
        public string Fault { get; }

        /// <summary>
        /// where, in north, east and vertical
        /// </summary>
        public Point3D At { get; }

        /// <summary>
        /// How far from square the crossing is, degrees: nought goes straight through the fault at
        /// right angles, ninety runs along inside it.
        /// <para>
        /// This is the angle between the path and the <em>normal</em> of the surface, not the surface
        /// itself, because what is being asked is how squarely the fault is taken. A well that crosses
        /// obliquely is a long way inside the damaged rock for every metre of fault it gets past.
        /// </para>
        /// </summary>
        public double Obliquity { get; }

        /// <summary>
        /// how far along the path it happened, m
        /// </summary>
        public double Along { get; }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        public FaultCrossing(string fault, Point3D at, double obliquity, double along)
        {
            Fault = fault;
            At = at;
            Obliquity = obliquity;
            Along = along;
        }
    }

    /// <summary>
    /// Where a path goes through the faults, and how squarely.
    /// <para>
    /// This measures and does not impose. Nothing in the flow knows a fault is there; the crossings are
    /// read off the path afterwards, the same way the separation a sidetrack achieves from its parent is
    /// read off rather than asked for. What it is for is a rule applied at the end: a plan may cross a
    /// fault, but not at a grazing angle.
    /// </para>
    /// </summary>
    public static class FaultCrossings
    {
        /// <summary>
        /// how many points each pillar is resampled to before the quadrilaterals between pillars are
        /// cut into triangles
        /// </summary>
        public const int PillarSamples = 6;

        /// <summary>
        /// How far a fault has to reach inside a corridor before it counts as going through it, m.
        /// <para>
        /// A corridor pulled back from a fault ends with its boundary on that fault, and a fault lying
        /// along a boundary is not one the well goes through. Without a tolerance every trim would
        /// report the fault it just avoided as still being crossed.
        /// </para>
        /// </summary>
        public const double CrossingTolerance = 0.25;

        /// <summary>
        /// every place the path goes through one of the faults
        /// </summary>
        public static List<FaultCrossing> Find(IReadOnlyList<Point3D> path,
                                               IReadOnlyList<FaultSurface> faults)
        {
            List<FaultCrossing> found = new List<FaultCrossing>();
            if (path == null || path.Count < 2 || faults == null)
            {
                return found;
            }
            double[] low = { double.MaxValue, double.MaxValue, double.MaxValue };
            double[] high = { double.MinValue, double.MinValue, double.MinValue };
            foreach (Point3D at in path)
            {
                double[] p = { at.X!.Value, at.Y!.Value, at.Z!.Value };
                for (int a = 0; a < 3; a++)
                {
                    if (p[a] < low[a]) { low[a] = p[a]; }
                    if (p[a] > high[a]) { high[a] = p[a]; }
                }
            }
            foreach (FaultSurface fault in faults)
            {
                bool apart = false;
                for (int a = 0; a < 3 && !apart; a++)
                {
                    if (fault.BoundingBoxMaximum[a] < low[a] || fault.BoundingBoxMinimum[a] > high[a])
                    {
                        apart = true;
                    }
                }
                if (apart) { continue; }
                double[] triangles = fault.GetTriangles(PillarSamples);
                double along = 0;
                for (int k = 0; k + 1 < path.Count; k++)
                {
                    double[] from = { path[k].X!.Value, path[k].Y!.Value, path[k].Z!.Value };
                    double[] to = { path[k + 1].X!.Value, path[k + 1].Y!.Value,
                                    path[k + 1].Z!.Value };
                    double[] step = { to[0] - from[0], to[1] - from[1], to[2] - from[2] };
                    double length = System.Math.Sqrt(step[0] * step[0] + step[1] * step[1]
                                                     + step[2] * step[2]);
                    if (length > 0)
                    {
                        for (int t = 0; t + 8 < triangles.Length; t += 9)
                        {
                            if (Hits(triangles, t, from, step, out double share,
                                     out double obliquity))
                            {
                                found.Add(new FaultCrossing(
                                    fault.Name ?? "?",
                                    new Point3D(from[0] + share * step[0],
                                                from[1] + share * step[1],
                                                from[2] + share * step[2]),
                                    obliquity, along + share * length));
                            }
                        }
                    }
                    along += length;
                }
            }
            return found;
        }

        /// <summary>
        /// Whether a segment goes through a triangle, by the Moller Trumbore construction, and if it
        /// does, how far along the segment and how far from square.
        /// </summary>
        private static bool Hits(double[] triangles, int at, double[] from, double[] step,
                                 out double share, out double obliquity)
        {
            share = 0;
            obliquity = 0;
            double[] edgeOne = { triangles[at + 3] - triangles[at],
                                 triangles[at + 4] - triangles[at + 1],
                                 triangles[at + 5] - triangles[at + 2] };
            double[] edgeTwo = { triangles[at + 6] - triangles[at],
                                 triangles[at + 7] - triangles[at + 1],
                                 triangles[at + 8] - triangles[at + 2] };
            double[] across = { step[1] * edgeTwo[2] - step[2] * edgeTwo[1],
                                step[2] * edgeTwo[0] - step[0] * edgeTwo[2],
                                step[0] * edgeTwo[1] - step[1] * edgeTwo[0] };
            double determinant = edgeOne[0] * across[0] + edgeOne[1] * across[1]
                                 + edgeOne[2] * across[2];
            if (System.Math.Abs(determinant) < 1.0e-12)
            {
                // the segment runs parallel to the plane of the triangle, so it does not go through it
                return false;
            }
            double inverse = 1.0 / determinant;
            double[] offset = { from[0] - triangles[at], from[1] - triangles[at + 1],
                                from[2] - triangles[at + 2] };
            double u = inverse * (offset[0] * across[0] + offset[1] * across[1]
                                  + offset[2] * across[2]);
            if (u < 0 || u > 1) { return false; }
            double[] second = { offset[1] * edgeOne[2] - offset[2] * edgeOne[1],
                                offset[2] * edgeOne[0] - offset[0] * edgeOne[2],
                                offset[0] * edgeOne[1] - offset[1] * edgeOne[0] };
            double v = inverse * (step[0] * second[0] + step[1] * second[1] + step[2] * second[2]);
            if (v < 0 || u + v > 1) { return false; }
            double reach = inverse * (edgeTwo[0] * second[0] + edgeTwo[1] * second[1]
                                      + edgeTwo[2] * second[2]);
            if (reach < 0 || reach > 1) { return false; }
            share = reach;

            double[] normal = { edgeOne[1] * edgeTwo[2] - edgeOne[2] * edgeTwo[1],
                                edgeOne[2] * edgeTwo[0] - edgeOne[0] * edgeTwo[2],
                                edgeOne[0] * edgeTwo[1] - edgeOne[1] * edgeTwo[0] };
            double normalLength = System.Math.Sqrt(normal[0] * normal[0] + normal[1] * normal[1]
                                                   + normal[2] * normal[2]);
            double stepLength = System.Math.Sqrt(step[0] * step[0] + step[1] * step[1]
                                                 + step[2] * step[2]);
            if (!(normalLength > 0) || !(stepLength > 0)) { return false; }
            double cosine = (step[0] * normal[0] + step[1] * normal[1] + step[2] * normal[2])
                            / (normalLength * stepLength);
            if (cosine < 0) { cosine = -cosine; }          // which way the normal points is arbitrary
            if (cosine > 1) { cosine = 1; }
            obliquity = System.Math.Acos(cosine) * 180.0 / System.Math.PI;
            return true;
        }

        /// <summary>
        /// Where the corridor itself, and not merely its median, goes through the faults.
        /// <para>
        /// A factory is a tube and not a line: the median is only the middle of it, and the tolerance
        /// region around it can be tens of metres across. A fault that misses the median by ten metres
        /// is still a fault a well drilled inside the corridor would go through, so what has to be
        /// tested is the region.
        /// </para>
        /// <para>
        /// Each cross-section is taken in turn. A fault triangle that straddles the plane of that
        /// cross-section meets it in a segment; the segment is put into the frame of the cross-section
        /// and clipped against the half-planes the region is held as, and whatever survives is a piece
        /// of fault inside the corridor. That is exact at each cross-section, so the only thing it can
        /// miss is a fault slipping between two of them, and they are a couple of metres apart.
        /// </para>
        /// <para>
        /// The angle is still taken against the tangent of the median, not against anything about the
        /// region: every path in a corridor runs the same way, so what differs between them is where
        /// they cross and not how squarely.
        /// </para>
        /// </summary>
        public static List<FaultCrossing> FindThroughTube(StreamlineBundleFactory factory,
                                                          IReadOnlyList<FaultSurface> faults)
        {
            List<FaultCrossing> found = new List<FaultCrossing>();
            if (factory == null || faults == null)
            {
                return found;
            }
            double[] low = { double.MaxValue, double.MaxValue, double.MaxValue };
            double[] high = { double.MinValue, double.MinValue, double.MinValue };
            foreach (CrossSectionStation station in factory.Stations)
            {
                if (station.Position == null || station.Polygon == null) { continue; }
                double reach = station.Polygon.OuterRadius;
                double[] at = { station.Position.X!.Value, station.Position.Y!.Value,
                                station.Position.Z!.Value };
                for (int a = 0; a < 3; a++)
                {
                    if (at[a] - reach < low[a]) { low[a] = at[a] - reach; }
                    if (at[a] + reach > high[a]) { high[a] = at[a] + reach; }
                }
            }
            if (low[0] > high[0]) { return found; }

            foreach (FaultSurface fault in faults)
            {
                bool apart = false;
                for (int a = 0; a < 3 && !apart; a++)
                {
                    if (fault.BoundingBoxMaximum[a] < low[a] || fault.BoundingBoxMinimum[a] > high[a])
                    {
                        apart = true;
                    }
                }
                if (apart) { continue; }
                double deepest = 0;
                double[] triangles = fault.GetTriangles(PillarSamples);
                foreach (CrossSectionStation station in factory.Stations)
                {
                    if (station.Position == null || station.Tangent == null
                        || station.FirstNormal == null || station.SecondNormal == null
                        || station.Polygon == null)
                    {
                        continue;
                    }
                    double[] centre = { station.Position.X!.Value, station.Position.Y!.Value,
                                        station.Position.Z!.Value };
                    double[] along = { station.Tangent.X!.Value, station.Tangent.Y!.Value,
                                       station.Tangent.Z!.Value };
                    double[] first = { station.FirstNormal.X!.Value, station.FirstNormal.Y!.Value,
                                       station.FirstNormal.Z!.Value };
                    double[] second = { station.SecondNormal.X!.Value, station.SecondNormal.Y!.Value,
                                        station.SecondNormal.Z!.Value };
                    for (int t = 0; t + 8 < triangles.Length; t += 9)
                    {
                        if (!MeetsSection(triangles, t, centre, along, first, second,
                                          station.Polygon, out double u, out double v,
                                          out double obliquity, out double depth))
                        {
                            continue;
                        }
                        if (depth > deepest) { deepest = depth; }
                        found.Add(new FaultCrossing(
                            fault.Name ?? "?",
                            new Point3D(centre[0] + u * first[0] + v * second[0],
                                        centre[1] + u * first[1] + v * second[1],
                                        centre[2] + u * first[2] + v * second[2]),
                            obliquity, station.Abscissa));
                    }
                }
                LastDepths[fault.Name ?? "?"] = deepest;
            }
            return found;
        }

        /// <summary>
        /// how far into the corridor the last call found each fault to reach, m of chord inside the
        /// cross-section. A fault the boundary has been trimmed onto reads near nought.
        /// </summary>
        public static Dictionary<string, double> LastDepths { get; }
            = new Dictionary<string, double>(StringComparer.Ordinal);

        /// <summary>
        /// Where a fault triangle cuts the plane of one cross-section: the two points of the segment,
        /// in the frame of that cross-section. False when the triangle does not reach the plane.
        /// </summary>
        public static bool GetSectionCut(double[] triangles, int at, double[] centre, double[] along,
                                         double[] first, double[] second,
                                         out double[] cutU, out double[] cutV)
        {
            cutU = new double[2];
            cutV = new double[2];
            double[] side = new double[3];
            for (int corner = 0; corner < 3; corner++)
            {
                side[corner] = (triangles[at + 3 * corner] - centre[0]) * along[0]
                               + (triangles[at + 3 * corner + 1] - centre[1]) * along[1]
                               + (triangles[at + 3 * corner + 2] - centre[2]) * along[2];
            }
            if ((side[0] > 0 && side[1] > 0 && side[2] > 0)
                || (side[0] < 0 && side[1] < 0 && side[2] < 0))
            {
                return false;
            }
            int met = 0;
            for (int corner = 0; corner < 3 && met < 2; corner++)
            {
                int next = (corner + 1) % 3;
                if ((side[corner] > 0 && side[next] > 0) || (side[corner] < 0 && side[next] < 0))
                {
                    continue;
                }
                double span = side[corner] - side[next];
                double share = System.Math.Abs(span) > 1.0e-12 ? side[corner] / span : 0;
                double[] on = new double[3];
                for (int a = 0; a < 3; a++)
                {
                    double one = triangles[at + 3 * corner + a];
                    double two = triangles[at + 3 * next + a];
                    on[a] = one + share * (two - one) - centre[a];
                }
                cutU[met] = on[0] * first[0] + on[1] * first[1] + on[2] * first[2];
                cutV[met] = on[0] * second[0] + on[1] * second[1] + on[2] * second[2];
                met++;
            }
            return met == 2;
        }

        /// <summary>
        /// Whether a fault triangle reaches inside the region of one cross-section, and if so where in
        /// it and how far from square.
        /// </summary>
        private static bool MeetsSection(double[] triangles, int at, double[] centre, double[] along,
                                         double[] first, double[] second,
                                         CrossSectionPolygon region,
                                         out double u, out double v, out double obliquity,
                                         out double depth)
        {
            u = 0; v = 0; obliquity = 0; depth = 0;
            if (!GetSectionCut(triangles, at, centre, along, first, second,
                               out double[] edgeU, out double[] edgeV))
            {
                return false;
            }
            if (!SegmentMeets(region, edgeU[0], edgeV[0], edgeU[1], edgeV[1], out u, out v,
                              out double inside))
            {
                return false;
            }
            // How far the fault actually reaches inside, radially. A fault the boundary has been
            // pulled back onto lies along that boundary, so the chord inside the region is as long as
            // the region is wide while the fault reaches no depth at all. Length cannot tell the two
            // apart; depth can.
            double lineU = edgeU[1] - edgeU[0];
            double lineV = edgeV[1] - edgeV[0];
            double deepest = 0;
            for (int d = 0; d < region.DirectionCount; d++)
            {
                double direction = region.GetDirection(d);
                double rayU = System.Math.Cos(direction);
                double rayV = System.Math.Sin(direction);
                double denominator = rayU * lineV - rayV * lineU;
                if (System.Math.Abs(denominator) < 1.0e-12) { continue; }
                double reach = (edgeU[0] * lineV - edgeV[0] * lineU) / denominator;
                double share = (edgeU[0] * rayV - edgeV[0] * rayU) / denominator;
                if (share < 0 || share > 1 || reach <= 0) { continue; }
                double boundary = region.GetBoundaryRadius(direction);
                if (boundary > reach && boundary - reach > deepest)
                {
                    deepest = boundary - reach;
                }
            }
            if (deepest <= CrossingTolerance)
            {
                return false;
            }
            depth = deepest;

            double[] edgeOne = { triangles[at + 3] - triangles[at],
                                 triangles[at + 4] - triangles[at + 1],
                                 triangles[at + 5] - triangles[at + 2] };
            double[] edgeTwo = { triangles[at + 6] - triangles[at],
                                 triangles[at + 7] - triangles[at + 1],
                                 triangles[at + 8] - triangles[at + 2] };
            double[] normal = { edgeOne[1] * edgeTwo[2] - edgeOne[2] * edgeTwo[1],
                                edgeOne[2] * edgeTwo[0] - edgeOne[0] * edgeTwo[2],
                                edgeOne[0] * edgeTwo[1] - edgeOne[1] * edgeTwo[0] };
            double length = System.Math.Sqrt(normal[0] * normal[0] + normal[1] * normal[1]
                                             + normal[2] * normal[2]);
            if (!(length > 0)) { return false; }
            double cosine = (along[0] * normal[0] + along[1] * normal[1] + along[2] * normal[2])
                            / length;
            if (cosine < 0) { cosine = -cosine; }
            if (cosine > 1) { cosine = 1; }
            obliquity = System.Math.Acos(cosine) * 180.0 / System.Math.PI;
            return true;
        }

        /// <summary>
        /// Whether a segment in the frame of a cross-section reaches inside its region, by clipping the
        /// segment against the half-planes the region is held as. The middle of whatever survives is
        /// handed back as where the fault is met.
        /// </summary>
        private static bool SegmentMeets(CrossSectionPolygon region, double fromU, double fromV,
                                         double toU, double toV, out double u, out double v)
        {
            return SegmentMeets(region, fromU, fromV, toU, toV, out u, out v, out double _);
        }

        /// <summary>
        /// as above, and also how long the piece of the segment inside the region is. A fault the
        /// boundary has been trimmed onto lies along the boundary, so it still meets the region but the
        /// piece inside is of no length: that is a touch and not a crossing.
        /// </summary>
        private static bool SegmentMeets(CrossSectionPolygon region, double fromU, double fromV,
                                         double toU, double toV, out double u, out double v,
                                         out double inside)
        {
            u = 0; v = 0; inside = 0;
            double entering = 0;
            double leaving = 1;
            double stepU = toU - fromU;
            double stepV = toV - fromV;
            for (int k = 0; k < region.DirectionCount; k++)
            {
                double direction = region.GetDirection(k);
                double cosine = System.Math.Cos(direction);
                double sine = System.Math.Sin(direction);
                double atStart = fromU * cosine + fromV * sine - region.SupportDistance[k];
                double rate = stepU * cosine + stepV * sine;
                if (System.Math.Abs(rate) < 1.0e-12)
                {
                    if (atStart > 0) { return false; }
                    continue;
                }
                double crosses = -atStart / rate;
                if (rate > 0)
                {
                    if (crosses < leaving) { leaving = crosses; }
                }
                else
                {
                    if (crosses > entering) { entering = crosses; }
                }
                if (entering > leaving) { return false; }
            }
            double middle = 0.5 * (entering + leaving);
            u = fromU + middle * stepU;
            v = fromV + middle * stepV;
            inside = (leaving - entering)
                     * System.Math.Sqrt(stepU * stepU + stepV * stepV);
            return true;
        }

        /// <summary>
        /// the most oblique of a set of crossings, degrees, or nought when there are none
        /// </summary>
        public static double GetWorstObliquity(IReadOnlyList<FaultCrossing> crossings)
        {
            double worst = 0;
            foreach (FaultCrossing crossing in crossings)
            {
                if (crossing.Obliquity > worst) { worst = crossing.Obliquity; }
            }
            return worst;
        }
    }

    /// <summary>
    /// Reads the fault examples after they have been moved under the Ullrigg cluster, and chooses the
    /// ones a case has any reason to care about.
    /// </summary>
    public static class FaultSet
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        /// <summary>
        /// reads every fault in a directory, one file each, in the tab separated form the generator
        /// wrote: pillar, north, east, vertical, with a commented header
        /// </summary>
        public static List<FaultSurface> ReadAll(string directory)
        {
            List<FaultSurface> faults = new List<FaultSurface>();
            if (!Directory.Exists(directory))
            {
                return faults;
            }
            foreach (string file in Directory.GetFiles(directory, "*.txt").OrderBy(f => f))
            {
                FaultSurface fault = new FaultSurface
                {
                    Name = Path.GetFileNameWithoutExtension(file)
                };
                Dictionary<int, int> byIndex = new Dictionary<int, int>();
                foreach (string line in File.ReadLines(file))
                {
                    string text = line.Trim();
                    if (text.Length == 0 || text.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    string[] parts = text.Split(new[] { '\t', ' ' },
                                                StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 4) { continue; }
                    int named = int.Parse(parts[0], Invariant);
                    if (!byIndex.TryGetValue(named, out int pillar))
                    {
                        pillar = byIndex.Count;
                        byIndex[named] = pillar;
                    }
                    fault.Add(pillar, new Point3D(double.Parse(parts[1], Invariant),
                                                  double.Parse(parts[2], Invariant),
                                                  double.Parse(parts[3], Invariant)));
                }
                if (fault.PointCount > 0) { faults.Add(fault); }
            }
            return faults;
        }

        /// <summary>
        /// The box a case spans: the one that holds its start and the middle of its target, opened out
        /// by the given amount on every side.
        /// </summary>
        public static void GetBox(Point3D from, Point3D to, double expansion,
                                  double[] minimum, double[] maximum)
        {
            double[] first = { from.X!.Value, from.Y!.Value, from.Z!.Value };
            double[] second = { to.X!.Value, to.Y!.Value, to.Z!.Value };
            for (int a = 0; a < 3; a++)
            {
                minimum[a] = System.Math.Min(first[a], second[a]) - expansion;
                maximum[a] = System.Math.Max(first[a], second[a]) + expansion;
            }
        }

        /// <summary>
        /// the faults with any part of them in the given box
        /// </summary>
        public static List<FaultSurface> Select(IReadOnlyList<FaultSurface> faults,
                                                double[] minimum, double[] maximum)
        {
            List<FaultSurface> chosen = new List<FaultSurface>();
            foreach (FaultSurface fault in faults)
            {
                if (fault.Intersects(minimum, maximum)) { chosen.Add(fault); }
            }
            return chosen;
        }
    }
}
