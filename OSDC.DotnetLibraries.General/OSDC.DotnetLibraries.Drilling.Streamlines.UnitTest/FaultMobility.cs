using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// A medium in which a fault is hard to travel <em>along</em> and no harder than the rock to cross
    /// square on.
    /// <para>
    /// The rule being expressed is that a fault is not penetrable except perpendicular to it. Stated as
    /// a permeability that is the uniaxial tensor the other way up from a
    /// <see cref="DirectionGuide"/>: where that makes one direction easy and leaves the rest alone,
    /// this leaves the fault's own normal alone and makes everything in the plane of the fault hard. A
    /// path crossing square on meets no resistance at all; one crossing at a graze has to travel a long
    /// way in the plane and pays for every metre of it.
    /// </para>
    /// <para>
    /// The effect is confined to a band either side of the surface and fades smoothly to nothing at its
    /// edge, so the medium has no edge of its own. Per axis the value is
    /// <c>1 - (1 - 1/contrast) w (1 - n_a^2)</c>, with <c>n</c> the unit normal of the nearest piece of
    /// fault and <c>w</c> the weight of the band. Along the normal that is one; across it, at the
    /// surface, it is one over the contrast.
    /// </para>
    /// <para>
    /// What it cannot do is impose the rule. The solver takes only the face normal component of a
    /// tensor, so an oblique fault loses its cross terms, and a mobility is a preference rather than a
    /// wall in any case: flow still crosses where the potential pushes hard enough. Whether it earns
    /// its place is a question for the crossings measured afterwards, not for this file.
    /// </para>
    /// </summary>
    public class FaultMobilityField : IFaceMobility
    {
        private readonly double[] triangles_;         // nine per triangle
        private readonly double[] normals_;           // three per triangle, unit
        private readonly Dictionary<(int, int, int), List<int>> buckets_
            = new Dictionary<(int, int, int), List<int>>();
        private readonly double bucketSize_;
        private readonly double width_;
        private readonly double drop_;

        /// <summary>
        /// how far either side of a fault the medium is affected, m
        /// </summary>
        public double Width
        {
            get { return width_; }
        }

        /// <summary>
        /// builds the medium from the faults that matter to a case
        /// </summary>
        /// <param name="faults"></param>
        /// <param name="width">how far either side of a surface the band reaches, m</param>
        /// <param name="contrast">
        /// how much harder it is to travel in the plane of a fault than across it, at the surface. One
        /// leaves the medium alone.
        /// </param>
        public FaultMobilityField(IReadOnlyList<FaultSurface> faults, double width, double contrast)
        {
            width_ = width > 0 ? width : 1.0;
            drop_ = contrast > 1.0 ? 1.0 - 1.0 / contrast : 0.0;
            bucketSize_ = width_;

            List<double> corners = new List<double>();
            List<double> normals = new List<double>();
            foreach (FaultSurface fault in faults)
            {
                double[] own = fault.GetTriangles(FaultCrossings.PillarSamples);
                for (int t = 0; t + 8 < own.Length; t += 9)
                {
                    double[] edgeOne = { own[t + 3] - own[t], own[t + 4] - own[t + 1],
                                         own[t + 5] - own[t + 2] };
                    double[] edgeTwo = { own[t + 6] - own[t], own[t + 7] - own[t + 1],
                                         own[t + 8] - own[t + 2] };
                    double[] normal = { edgeOne[1] * edgeTwo[2] - edgeOne[2] * edgeTwo[1],
                                        edgeOne[2] * edgeTwo[0] - edgeOne[0] * edgeTwo[2],
                                        edgeOne[0] * edgeTwo[1] - edgeOne[1] * edgeTwo[0] };
                    double length = System.Math.Sqrt(normal[0] * normal[0] + normal[1] * normal[1]
                                                     + normal[2] * normal[2]);
                    if (!(length > 0))
                    {
                        // a degenerate sliver carries no direction, so it says nothing about the rock
                        continue;
                    }
                    int index = normals.Count / 3;
                    for (int k = 0; k < 9; k++) { corners.Add(own[t + k]); }
                    normals.Add(normal[0] / length);
                    normals.Add(normal[1] / length);
                    normals.Add(normal[2] / length);
                    Bucket(index, own, t);
                }
            }
            triangles_ = corners.ToArray();
            normals_ = normals.ToArray();
        }

        /// <summary>
        /// how many triangles of fault the medium is built on
        /// </summary>
        public int TriangleCount
        {
            get { return normals_.Length / 3; }
        }

        private void Bucket(int index, double[] source, int at)
        {
            double[] low = { double.MaxValue, double.MaxValue, double.MaxValue };
            double[] high = { double.MinValue, double.MinValue, double.MinValue };
            for (int corner = 0; corner < 3; corner++)
            {
                for (int a = 0; a < 3; a++)
                {
                    double value = source[at + 3 * corner + a];
                    if (value < low[a]) { low[a] = value; }
                    if (value > high[a]) { high[a] = value; }
                }
            }
            // the band reaches a width beyond the triangle itself, so the buckets have to as well
            for (int i = Key(low[0] - width_); i <= Key(high[0] + width_); i++)
            {
                for (int j = Key(low[1] - width_); j <= Key(high[1] + width_); j++)
                {
                    for (int k = Key(low[2] - width_); k <= Key(high[2] + width_); k++)
                    {
                        if (!buckets_.TryGetValue((i, j, k), out List<int>? held))
                        {
                            held = new List<int>();
                            buckets_[(i, j, k)] = held;
                        }
                        held.Add(index);
                    }
                }
            }
        }

        private int Key(double value)
        {
            return (int)System.Math.Floor(value / bucketSize_);
        }

        /// <summary>
        /// the three face values at a position, indexed by axis
        /// </summary>
        public void GetValues(double north, double east, double vertical, double[] values)
        {
            values[0] = 1.0;
            values[1] = 1.0;
            values[2] = 1.0;
            if (!(drop_ > 0) || normals_.Length == 0)
            {
                return;
            }
            if (!buckets_.TryGetValue((Key(north), Key(east), Key(vertical)),
                                      out List<int>? nearby))
            {
                return;
            }
            double nearest = double.MaxValue;
            int chosen = -1;
            foreach (int index in nearby)
            {
                double distance = SquaredDistance(index, north, east, vertical);
                if (distance < nearest)
                {
                    nearest = distance;
                    chosen = index;
                }
            }
            if (chosen < 0) { return; }
            double reach = System.Math.Sqrt(nearest);
            if (reach >= width_) { return; }
            // one at the surface, nothing at the edge of the band, and smooth at both
            double share = reach / width_;
            double weight = (1.0 - share * share);
            weight *= weight;
            double factor = drop_ * weight;
            for (int a = 0; a < 3; a++)
            {
                double component = normals_[3 * chosen + a];
                values[a] = 1.0 - factor * (1.0 - component * component);
                if (values[a] < 1.0e-6) { values[a] = 1.0e-6; }
            }
        }

        /// <summary>
        /// how far a position is from one triangle, squared, by the closest point on it
        /// </summary>
        private double SquaredDistance(int index, double north, double east, double vertical)
        {
            int at = 9 * index;
            double[] a = { triangles_[at], triangles_[at + 1], triangles_[at + 2] };
            double[] ab = { triangles_[at + 3] - a[0], triangles_[at + 4] - a[1],
                            triangles_[at + 5] - a[2] };
            double[] ac = { triangles_[at + 6] - a[0], triangles_[at + 7] - a[1],
                            triangles_[at + 8] - a[2] };
            double[] ap = { north - a[0], east - a[1], vertical - a[2] };

            double d1 = Dot(ab, ap), d2 = Dot(ac, ap);
            if (d1 <= 0 && d2 <= 0) { return Dot(ap, ap); }

            double[] bp = { ap[0] - ab[0], ap[1] - ab[1], ap[2] - ab[2] };
            double d3 = Dot(ab, bp), d4 = Dot(ac, bp);
            if (d3 >= 0 && d4 <= d3) { return Dot(bp, bp); }

            double vc = d1 * d4 - d3 * d2;
            if (vc <= 0 && d1 >= 0 && d3 <= 0)
            {
                double share = d1 / (d1 - d3);
                return Squared(ap[0] - share * ab[0]) + Squared(ap[1] - share * ab[1])
                       + Squared(ap[2] - share * ab[2]);
            }

            double[] cp = { ap[0] - ac[0], ap[1] - ac[1], ap[2] - ac[2] };
            double d5 = Dot(ab, cp), d6 = Dot(ac, cp);
            if (d6 >= 0 && d5 <= d6) { return Dot(cp, cp); }

            double vb = d5 * d2 - d1 * d6;
            if (vb <= 0 && d2 >= 0 && d6 <= 0)
            {
                double share = d2 / (d2 - d6);
                return Squared(ap[0] - share * ac[0]) + Squared(ap[1] - share * ac[1])
                       + Squared(ap[2] - share * ac[2]);
            }

            double va = d3 * d6 - d5 * d4;
            if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
            {
                double share = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return Squared(bp[0] + share * (cp[0] - bp[0]))
                       + Squared(bp[1] + share * (cp[1] - bp[1]))
                       + Squared(bp[2] + share * (cp[2] - bp[2]));
            }

            double denominator = 1.0 / (va + vb + vc);
            double u = vb * denominator;
            double v = vc * denominator;
            return Squared(ap[0] - u * ab[0] - v * ac[0]) + Squared(ap[1] - u * ab[1] - v * ac[1])
                   + Squared(ap[2] - u * ab[2] - v * ac[2]);
        }

        private static double Dot(double[] first, double[] second)
        {
            return first[0] * second[0] + first[1] * second[1] + first[2] * second[2];
        }

        private static double Squared(double value)
        {
            return value * value;
        }
    }
}
