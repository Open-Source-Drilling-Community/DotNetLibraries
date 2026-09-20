using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// A corridor of medium that is simply easier to move through, following a curve, narrow at its two
    /// ends and wide in between.
    /// <para>
    /// This is the scalar counterpart of <see cref="PathGuideField"/> and it behaves quite differently.
    /// A guide makes one direction cheap, so it collimates: inside it the flow has to run along the axis
    /// whether that suits it or not, and where the guide stops the flow refracts. A channel makes the
    /// whole neighbourhood cheap and leaves it isotropic, so the flow goes there because it is the way of
    /// least resistance and is then free to take whatever line it likes within it.
    /// </para>
    /// <para>
    /// Three things follow from being scalar. A heterogeneous scalar permeability is exact under a two
    /// point flux, the face value being the harmonic mean of the two sides, so none of the tensor's
    /// inconsistency arises. A channel cannot be driven into an obstacle the way a conduit can, because
    /// it only raises the permeability of cells that are open anyway. And since the streamlines are
    /// launched carrying equal shares of the flux, concentrating the flux into a channel concentrates the
    /// streamlines into it — which is the point.
    /// </para>
    /// <para>
    /// The width narrows at both ends because that is where the path is pinned, and widens in between
    /// because that is where there is a choice worth leaving open.
    /// </para>
    /// </summary>
    public class ChannelMobilityField : IFaceMobility
    {
        private readonly double[][] point_;       // 3 per sample, per spine
        private readonly double[][] along_;       // arc length at each sample, per spine
        private readonly double[] total_;
        private readonly double narrow_;
        private readonly double wide_;
        private readonly double excess_;

        /// <summary>
        /// how many curves the channel follows
        /// </summary>
        public int Count
        {
            get
            {
                return point_.Length;
            }
        }

        /// <summary>
        /// how long the longest of the curves is, m
        /// </summary>
        public double Length
        {
            get
            {
                double longest = 0;
                foreach (double one in total_)
                {
                    if (one > longest) { longest = one; }
                }
                return longest;
            }
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="spine">the curve the channel follows, at least two samples</param>
        /// <param name="narrowWidth">how far the channel reaches at either end, m</param>
        /// <param name="wideWidth">how far it reaches at its middle, m</param>
        /// <param name="contrast">how much easier the medium is on the curve than far from it</param>
        public ChannelMobilityField(IReadOnlyList<Point3D> spine, double narrowWidth, double wideWidth,
                                    double contrast)
            : this(new[] { spine }, narrowWidth, wideWidth, contrast)
        {
        }

        /// <summary>
        /// Constructor over several curves, whose channels are joined by taking whichever is easiest at
        /// each point rather than by adding them. Adding would make the crossings and the places where
        /// two curves run side by side easier than the curves themselves, which is the opposite of what
        /// a set of alternatives means: each one should be as good as it would have been alone.
        /// </summary>
        /// <param name="spines">the curves to follow, each of at least two samples</param>
        /// <param name="narrowWidth"></param>
        /// <param name="wideWidth"></param>
        /// <param name="contrast"></param>
        public ChannelMobilityField(IReadOnlyList<IReadOnlyList<Point3D>> spines, double narrowWidth,
                                    double wideWidth, double contrast)
        {
            if (spines == null || spines.Count == 0)
            {
                throw new ArgumentException("a channel needs at least one curve", nameof(spines));
            }
            if (!(narrowWidth > 0) || !(wideWidth > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(narrowWidth));
            }
            List<double[]> points = new List<double[]>();
            List<double[]> alongs = new List<double[]>();
            List<double> totals = new List<double>();
            foreach (IReadOnlyList<Point3D> spine in spines)
            {
                if (spine == null || spine.Count < 2)
                {
                    continue;
                }
                double[] flat = new double[3 * spine.Count];
                double[] along = new double[spine.Count];
                for (int i = 0; i < spine.Count; i++)
                {
                    flat[3 * i] = spine[i].X!.Value;
                    flat[3 * i + 1] = spine[i].Y!.Value;
                    flat[3 * i + 2] = spine[i].Z!.Value;
                    if (i > 0)
                    {
                        along[i] = along[i - 1]
                                   + System.Math.Sqrt(Squared(flat[3 * i] - flat[3 * i - 3])
                                                      + Squared(flat[3 * i + 1] - flat[3 * i - 2])
                                                      + Squared(flat[3 * i + 2] - flat[3 * i - 1]));
                    }
                }
                if (!(along[spine.Count - 1] > 0))
                {
                    continue;
                }
                points.Add(flat);
                alongs.Add(along);
                totals.Add(along[spine.Count - 1]);
            }
            if (points.Count == 0)
            {
                throw new ArgumentException("none of the curves has any length", nameof(spines));
            }
            narrow_ = narrowWidth;
            wide_ = wideWidth;
            excess_ = contrast - 1.0;
            point_ = points.ToArray();
            along_ = alongs.ToArray();
            total_ = totals.ToArray();
        }

        /// <summary>
        /// how wide the channel is at a fraction of the way along it
        /// </summary>
        /// <param name="fraction">zero at the source end, one at the target end</param>
        /// <returns></returns>
        public double GetWidth(double fraction)
        {
            double u = fraction < 0 ? 0 : fraction > 1 ? 1 : fraction;
            // a parabola that is the narrow width at both ends and the wide one in the middle
            return narrow_ + (wide_ - narrow_) * 4.0 * u * (1.0 - u);
        }

        /// <summary>
        /// the three face values at a position, all the same because the channel is isotropic
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <param name="values"></param>
        public void GetValues(double north, double east, double vertical, double[] values)
        {
            if (values == null || values.Length < 3)
            {
                throw new ArgumentException("three values are needed", nameof(values));
            }
            values[0] = 1.0;
            values[1] = 1.0;
            values[2] = 1.0;
            if (!(excess_ > 0))
            {
                return;
            }

            double gain = 0;
            for (int spine = 0; spine < point_.Length; spine++)
            {
                double[] flat = point_[spine];
                double[] along = along_[spine];
                int count = along.Length;
                double best = double.MaxValue;
                double at = 0;
                for (int i = 0; i + 1 < count; i++)
                {
                    int index = 3 * i;
                    double ax = flat[index], ay = flat[index + 1], az = flat[index + 2];
                    double ux = flat[index + 3] - ax;
                    double uy = flat[index + 4] - ay;
                    double uz = flat[index + 5] - az;
                    double square = ux * ux + uy * uy + uz * uz;
                    double t = 0;
                    if (square > 0)
                    {
                        t = ((north - ax) * ux + (east - ay) * uy + (vertical - az) * uz) / square;
                        t = t < 0 ? 0 : t > 1 ? 1 : t;
                    }
                    double cx = ax + t * ux - north;
                    double cy = ay + t * uy - east;
                    double cz = az + t * uz - vertical;
                    double distance = cx * cx + cy * cy + cz * cz;
                    if (distance < best)
                    {
                        best = distance;
                        at = along[i] + t * (along[i + 1] - along[i]);
                    }
                }
                double width = GetWidth(at / total_[spine]);
                double here = excess_ * System.Math.Exp(-best / (width * width));
                if (here > gain) { gain = here; }
            }
            values[0] += gain;
            values[1] += gain;
            values[2] += gain;
        }

        private static double Squared(double value)
        {
            return value * value;
        }
    }
}
