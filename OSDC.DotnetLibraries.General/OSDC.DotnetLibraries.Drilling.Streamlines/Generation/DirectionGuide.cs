using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// How easily flow crosses a face, as a multiple of the background.
    /// <para>
    /// One value per axis rather than a tensor, because a face of the octree is axis aligned and the only
    /// thing a two point flux can use is the component of the permeability along the face normal.
    /// </para>
    /// </summary>
    public interface IFaceMobility
    {
        /// <summary>
        /// the three face values at a position, indexed by axis: 0 north, 1 east, 2 vertical
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <param name="values">filled with three values, each strictly positive</param>
        void GetValues(double north, double east, double vertical, double[] values);
    }

    /// <summary>
    /// A region in which flow finds one direction easier than the others, fading smoothly with distance.
    /// <para>
    /// This is the soft counterpart of a conduit. A conduit is a wall, so it imposes a direction exactly
    /// and imposes it everywhere inside and nowhere outside, which puts the whole turn into the one cell
    /// where the wall stops. A guide is a property of the medium instead: it is strongest at its anchor
    /// and fades over a length, so the flow is bent over that length rather than at a face. What is given
    /// up is exactness — a guide makes a direction cheap, not compulsory.
    /// </para>
    /// <para>
    /// The permeability it stands for is <c>I + (contrast - 1) w dd'</c>, an ordinary uniaxial tensor
    /// whose contrast is scaled by a weight that falls off as a Gaussian in the two extents. Because the
    /// weight is smooth everywhere and the tensor is the identity far away, nothing in the field has an
    /// edge.
    /// </para>
    /// </summary>
    public class DirectionGuide
    {
        /// <summary>
        /// where the guide is strongest, usually a slot or the centre of a target
        /// </summary>
        public Point3D? Anchor { get; set; } = null;

        /// <summary>
        /// the direction made easy. Need not be a unit vector, and its sign does not matter.
        /// </summary>
        public Vector3D? Direction { get; set; } = null;

        /// <summary>
        /// how far the guide reaches along its direction before it has faded, m
        /// </summary>
        public double AlongLength { get; set; } = 300.0;

        /// <summary>
        /// how far it reaches across its direction, m. Much smaller than
        /// <see cref="AlongLength"/> makes a column, the same makes a ball.
        /// </summary>
        public double AcrossLength { get; set; } = 300.0;

        /// <summary>
        /// how much easier the chosen direction is than the others at the anchor. One is isotropic.
        /// </summary>
        public double Contrast { get; set; } = 10.0;

        /// <summary>
        /// Whether the guide reaches only upstream of its anchor, that is against
        /// <see cref="Direction"/>.
        /// <para>
        /// A target that admits one face wants this: a guide that also reached past it would make it
        /// cheap to overshoot and come back, which is the opposite of an approach. A source wants the
        /// other half, and leaving this false gives both.
        /// </para>
        /// </summary>
        public bool UpstreamOnly { get; set; } = false;

        /// <summary>
        /// the other half: the guide reaches only downstream of its anchor, which is what a guide sitting
        /// at the mouth of a conduit wants, since there is nothing behind it to guide
        /// </summary>
        public bool DownstreamOnly { get; set; } = false;

        /// <summary>
        /// whether the guide can be used
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return Anchor != null && Anchor.X != null && Anchor.Y != null && Anchor.Z != null
                   && Direction != null && (Direction.GetLength() ?? 0) > 0
                   && AlongLength > 0 && AcrossLength > 0 && Contrast > 0;
        }
    }

    /// <summary>
    /// A medium made of a background of one, plus any number of <see cref="DirectionGuide"/> that add
    /// their own uniaxial contrast where they reach.
    /// <para>
    /// Guides add rather than replace, so two that overlap with different directions give a medium easy
    /// in both, which is what a slot and a target pulling on the same stretch of hole should do.
    /// </para>
    /// <para>
    /// A two point flux takes only the face normal component of a tensor, so where a guide points along
    /// an axis this is the tensor exactly, and where it does not the cross terms are dropped, the same
    /// gap a hanging node already has. For a guide that is worth less than it looks: the tensor here is a
    /// knob for shaping a corridor, not a measured rock property, so being faithful to it is not the
    /// point. What has to survive is conservation and the field being single valued, and both do — every
    /// face still carries one flux, and the matrix stays symmetric and positive definite because every
    /// value below is strictly positive.
    /// </para>
    /// </summary>
    public class DirectionGuideField : IFaceMobility
    {
        private readonly double[] axis_;          // three per guide, the unit direction
        private readonly double[] anchor_;        // three per guide
        private readonly double[] along_;
        private readonly double[] across_;
        private readonly double[] excess_;        // contrast minus one
        private readonly bool[] upstream_;
        private readonly bool[] downstream_;

        /// <summary>
        /// how many guides make up the field
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="guides">any that are not valid, or that have no contrast, are left out</param>
        public DirectionGuideField(IEnumerable<DirectionGuide> guides)
        {
            if (guides == null)
            {
                throw new ArgumentNullException(nameof(guides));
            }
            List<DirectionGuide> kept = new List<DirectionGuide>();
            foreach (DirectionGuide guide in guides)
            {
                if (guide != null && guide.IsValid() && guide.Contrast != 1.0)
                {
                    kept.Add(guide);
                }
            }
            Count = kept.Count;
            axis_ = new double[3 * Count];
            anchor_ = new double[3 * Count];
            along_ = new double[Count];
            across_ = new double[Count];
            excess_ = new double[Count];
            upstream_ = new bool[Count];
            downstream_ = new bool[Count];
            for (int g = 0; g < Count; g++)
            {
                DirectionGuide guide = kept[g];
                double dn = guide.Direction!.X!.Value;
                double de = guide.Direction.Y!.Value;
                double dv = guide.Direction.Z!.Value;
                double length = System.Math.Sqrt(dn * dn + de * de + dv * dv);
                axis_[3 * g] = dn / length;
                axis_[3 * g + 1] = de / length;
                axis_[3 * g + 2] = dv / length;
                anchor_[3 * g] = guide.Anchor!.X!.Value;
                anchor_[3 * g + 1] = guide.Anchor.Y!.Value;
                anchor_[3 * g + 2] = guide.Anchor.Z!.Value;
                along_[g] = guide.AlongLength;
                across_[g] = guide.AcrossLength;
                excess_[g] = guide.Contrast - 1.0;
                upstream_[g] = guide.UpstreamOnly;
                downstream_[g] = guide.DownstreamOnly;
            }
        }

        /// <summary>
        /// the three face values at a position
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
            for (int g = 0; g < Count; g++)
            {
                int at = 3 * g;
                double rn = north - anchor_[at];
                double re = east - anchor_[at + 1];
                double rv = vertical - anchor_[at + 2];
                double dn = axis_[at], de = axis_[at + 1], dv = axis_[at + 2];
                double s = rn * dn + re * de + rv * dv;
                if (upstream_[g] && s > 0)
                {
                    // past the anchor along the direction, which for a one sided target is the far side
                    continue;
                }
                if (downstream_[g] && s < 0)
                {
                    // behind the anchor, which at the mouth of a conduit is inside the conduit
                    continue;
                }
                double pn = rn - s * dn, pe = re - s * de, pv = rv - s * dv;
                double across = (pn * pn + pe * pe + pv * pv) / (across_[g] * across_[g]);
                double weight = System.Math.Exp(-(s * s / (along_[g] * along_[g]) + across));
                if (weight < 1.0e-6)
                {
                    continue;
                }
                double gain = excess_[g] * weight;
                values[0] += gain * dn * dn;
                values[1] += gain * de * de;
                values[2] += gain * dv * dv;
            }
            // a face that has become impossible to cross would make the matrix singular rather than
            // merely resistant, and nothing here is meant to block
            for (int a = 0; a < 3; a++)
            {
                if (values[a] < 1.0e-9)
                {
                    values[a] = 1.0e-9;
                }
            }
        }
    }
}
