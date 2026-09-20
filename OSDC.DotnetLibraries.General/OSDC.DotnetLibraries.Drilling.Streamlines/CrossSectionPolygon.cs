namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// How far a well may deviate from the median in one cross-section: a convex region about the median,
    /// held as its support distance in each of a number of directions.
    /// <para>
    /// It is a <em>tolerance</em> and not an envelope of where the streamlines went, and the difference is
    /// the whole point. An envelope has to follow the paths, and paths go round things, so an envelope is
    /// free to arch over an obstruction its members avoided and to reach into ground a neighbouring bundle
    /// owns. A tolerance answers a different question — how far off the planned line a directional driller
    /// may be and still be safe — and that question only has a useful answer if the region is convex, free
    /// of every forbidden zone, and disjoint from its neighbours.
    /// </para>
    /// <para>
    /// Convex because a region with a forbidden notch in it cannot be held to in practice: a driller
    /// controls a deviation, not a route through a keyhole. The region is the intersection of the
    /// half-planes <c>x.u(s) &lt;= SupportDistance[s]</c>, which is convex by construction, and is contained
    /// in whatever star-shaped free region the support distances were measured from — so if each distance
    /// was measured by marching out to the first obstruction, nothing inside the region can be inside an
    /// obstruction.
    /// </para>
    /// <para>
    /// The <see cref="Inradius"/> is then simply the smallest support distance, which is the largest
    /// deviation that is safe in every direction at once: the number a drilling engineer's ability to hold
    /// a line is compared against.
    /// </para>
    /// </summary>
    public class CrossSectionPolygon
    {
        private readonly double[] support_;

        /// <summary>
        /// the support distance in each direction: how far the bounding half-plane of that direction sits
        /// from the median, m
        /// </summary>
        public IReadOnlyList<double> SupportDistance
        {
            get
            {
                return support_;
            }
        }

        /// <summary>
        /// how many directions the turn is divided into
        /// </summary>
        public int DirectionCount
        {
            get
            {
                return support_.Length;
            }
        }

        /// <summary>
        /// how many directions were stopped by a forbidden zone rather than by a neighbouring corridor or
        /// by the tolerance cap. Where this is zero the region is as large as it was allowed to be.
        /// </summary>
        public int BlockedDirectionCount { get; internal set; } = 0;

        /// <summary>
        /// The largest disc about the median that fits inside the region, m: the deviation that is safe
        /// whichever way it happens to go.
        /// </summary>
        public double Inradius { get; internal set; } = 0;

        /// <summary>
        /// the greatest support distance, m
        /// </summary>
        public double OuterRadius { get; internal set; } = 0;

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="directionCount"></param>
        public CrossSectionPolygon(int directionCount)
        {
            if (directionCount < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(directionCount));
            }
            support_ = new double[directionCount];
        }

        /// <summary>
        /// the direction the given index stands for, rad
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double GetDirection(int index)
        {
            return 2.0 * System.Math.PI * index / support_.Length;
        }

        /// <summary>
        /// sets the support distance of one direction
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetSupportDistance(int index, double value)
        {
            support_[index] = value;
        }

        /// <summary>
        /// How far the boundary of the region lies in the given direction, m.
        /// <para>
        /// For a convex region held by its support distances this is not the support distance of the
        /// nearest direction: it is the nearest bounding half-plane along the ray, which is the smallest
        /// of <c>SupportDistance[s] / cos(theta - direction(s))</c> over the half-planes the ray actually
        /// meets.
        /// </para>
        /// </summary>
        /// <param name="theta"></param>
        /// <returns></returns>
        public double GetBoundaryRadius(double theta)
        {
            double reach = double.MaxValue;
            for (int s = 0; s < support_.Length; s++)
            {
                double towards = System.Math.Cos(theta - GetDirection(s));
                if (towards <= 1.0e-9)
                {
                    continue;
                }
                double at = support_[s] / towards;
                if (at < reach)
                {
                    reach = at;
                }
            }
            return reach == double.MaxValue ? 0 : reach;
        }

        /// <summary>
        /// whether the given in-plane position lies inside the region
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool Contains(double u, double v)
        {
            for (int s = 0; s < support_.Length; s++)
            {
                double direction = GetDirection(s);
                if (u * System.Math.Cos(direction) + v * System.Math.Sin(direction) > support_[s])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// The corners of the region, in order. Consecutive bounding half-planes are intersected, and a
        /// corner that lies outside a third half-plane is dropped, which is what removes the half-planes
        /// that do not touch the region at all.
        /// </summary>
        /// <returns></returns>
        public List<(double U, double V)> GetVertices()
        {
            int count = support_.Length;
            List<(double U, double V)> vertices = new List<(double U, double V)>(count);
            for (int s = 0; s < count; s++)
            {
                int next = (s + 1) % count;
                double firstAngle = GetDirection(s);
                double secondAngle = GetDirection(next);
                double a0 = System.Math.Cos(firstAngle), b0 = System.Math.Sin(firstAngle);
                double a1 = System.Math.Cos(secondAngle), b1 = System.Math.Sin(secondAngle);
                double determinant = a0 * b1 - a1 * b0;
                if (System.Math.Abs(determinant) < 1.0e-12)
                {
                    continue;
                }
                double u = (support_[s] * b1 - support_[next] * b0) / determinant;
                double v = (a0 * support_[next] - a1 * support_[s]) / determinant;
                bool kept = true;
                for (int other = 0; other < count && kept; other++)
                {
                    if (other == s || other == next) { continue; }
                    double direction = GetDirection(other);
                    kept = u * System.Math.Cos(direction) + v * System.Math.Sin(direction)
                           <= support_[other] + 1.0e-9;
                }
                if (kept)
                {
                    vertices.Add((u, v));
                }
            }
            return vertices;
        }

        /// <summary>
        /// works out the inradius and the outer radius from the support distances
        /// </summary>
        public void Measure()
        {
            double inner = double.MaxValue;
            for (int s = 0; s < support_.Length; s++)
            {
                if (support_[s] < inner) { inner = support_[s]; }
            }
            // the distance from the median to the nearest bounding half-plane, which for a convex region
            // held this way is exactly the largest disc that fits
            Inradius = inner == double.MaxValue || inner < 0 ? 0 : inner;

            double outer = 0;
            foreach ((double U, double V) corner in GetVertices())
            {
                double reach = System.Math.Sqrt(corner.U * corner.U + corner.V * corner.V);
                if (reach > outer) { outer = reach; }
            }
            OuterRadius = outer;
        }
    }
}
