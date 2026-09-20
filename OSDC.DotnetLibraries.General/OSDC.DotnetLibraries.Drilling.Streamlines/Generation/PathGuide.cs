using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// A smooth curve from one position and tangent to another, used as the spine a guide follows.
    /// <para>
    /// A Hermite with both tangent magnitudes set to the chord is very close to the double arc of equal
    /// curvature a well planner would draw, and it needs nothing outside this library. It does not have to
    /// be drillable in itself — it is a direction field, and what gets drilled is fitted to the corridor
    /// afterwards — but its own curvature is worth knowing, because a corridor cannot turn more gently
    /// than the spine it is laid along.
    /// </para>
    /// </summary>
    public static class ReferenceSpine
    {
        /// <summary>
        /// Samples a Hermite curve between two positions with two tangents.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="departure">need not be a unit vector; only its direction is used</param>
        /// <param name="to"></param>
        /// <param name="arrival">need not be a unit vector; only its direction is used</param>
        /// <param name="count">how many samples, at least two</param>
        /// <returns></returns>
        public static List<Point3D> Sample(Point3D from, Vector3D departure, Point3D to, Vector3D arrival,
                                           int count = 129)
        {
            if (from == null || to == null || departure == null || arrival == null)
            {
                throw new ArgumentNullException(nameof(from));
            }
            if (count < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            double an = from.X!.Value, ae = from.Y!.Value, av = from.Z!.Value;
            double bn = to.X!.Value, be = to.Y!.Value, bv = to.Z!.Value;
            double chord = System.Math.Sqrt(Squared(bn - an) + Squared(be - ae) + Squared(bv - av));
            Unit(departure, out double pn, out double pe, out double pv);
            Unit(arrival, out double qn, out double qe, out double qv);
            pn *= chord; pe *= chord; pv *= chord;
            qn *= chord; qe *= chord; qv *= chord;

            List<Point3D> samples = new List<Point3D>(count);
            for (int i = 0; i < count; i++)
            {
                double t = (double)i / (count - 1);
                double t2 = t * t;
                double t3 = t2 * t;
                double h00 = 2 * t3 - 3 * t2 + 1;
                double h10 = t3 - 2 * t2 + t;
                double h01 = -2 * t3 + 3 * t2;
                double h11 = t3 - t2;
                samples.Add(new Point3D(h00 * an + h10 * pn + h01 * bn + h11 * qn,
                                        h00 * ae + h10 * pe + h01 * be + h11 * qe,
                                        h00 * av + h10 * pv + h01 * bv + h11 * qv));
            }
            return samples;
        }

        /// <summary>
        /// The same curve pushed sideways in the middle, for use as a seed that commits to one way round
        /// the obstacles.
        /// <para>
        /// A relaxation is a local thing: it settles into the nearest clear curve to whatever it started
        /// from, and cannot discover that the other side of an obstacle would have been better. That is
        /// usually a weakness and here it is the mechanism — the seed picks which way round and the
        /// relaxation tidies it up. So the alternatives are found by nudging, not by searching.
        /// </para>
        /// <para>
        /// The bump is <c>sin squared</c> of the parameter, which is zero at both ends and has zero
        /// slope there, so the two positions and the two tangents all survive it and every alternative
        /// still leaves the slot and reaches the target the way it was told to.
        /// </para>
        /// </summary>
        /// <param name="from"></param>
        /// <param name="departure"></param>
        /// <param name="to"></param>
        /// <param name="arrival"></param>
        /// <param name="north">how far to push the middle, m</param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<Point3D> Nudged(Point3D from, Vector3D departure, Point3D to, Vector3D arrival,
                                           double north, double east, double vertical, int count = 129)
        {
            List<Point3D> plain = Sample(from, departure, to, arrival, count);
            List<Point3D> pushed = new List<Point3D>(plain.Count);
            for (int i = 0; i < plain.Count; i++)
            {
                double t = (double)i / (plain.Count - 1);
                double bump = System.Math.Sin(System.Math.PI * t);
                bump *= bump;
                pushed.Add(new Point3D(plain[i].X!.Value + bump * north,
                                       plain[i].Y!.Value + bump * east,
                                       plain[i].Z!.Value + bump * vertical));
            }
            return pushed;
        }

        /// <summary>
        /// A set of seeds pushed to each side of the straight curve, for relaxing into the distinct ways
        /// round whatever is in the way.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="departure"></param>
        /// <param name="to"></param>
        /// <param name="arrival"></param>
        /// <param name="displacement">how far to push, m</param>
        /// <param name="around">how many directions to try around the chord, at least two</param>
        /// <param name="count"></param>
        /// <returns>the unpushed curve first, then one per direction</returns>
        public static List<List<Point3D>> Fan(Point3D from, Vector3D departure, Point3D to,
                                              Vector3D arrival, double displacement, int around = 4,
                                              int count = 129)
        {
            List<List<Point3D>> seeds = new List<List<Point3D>>
            {
                Sample(from, departure, to, arrival, count)
            };
            double cn = to.X!.Value - from.X!.Value;
            double ce = to.Y!.Value - from.Y!.Value;
            double cv = to.Z!.Value - from.Z!.Value;
            double chord = System.Math.Sqrt(cn * cn + ce * ce + cv * cv);
            if (!(chord > 0) || !(displacement > 0) || around < 2)
            {
                return seeds;
            }
            cn /= chord; ce /= chord; cv /= chord;
            // any pair of directions across the chord will do, and the least of its components names one
            int smallest = System.Math.Abs(cn) <= System.Math.Abs(ce)
                           ? (System.Math.Abs(cn) <= System.Math.Abs(cv) ? 0 : 2)
                           : (System.Math.Abs(ce) <= System.Math.Abs(cv) ? 1 : 2);
            double hn = smallest == 0 ? 1 : 0, he = smallest == 1 ? 1 : 0, hv = smallest == 2 ? 1 : 0;
            double an = he * cv - hv * ce, ae = hv * cn - hn * cv, av = hn * ce - he * cn;
            double length = System.Math.Sqrt(an * an + ae * ae + av * av);
            an /= length; ae /= length; av /= length;
            double bn = ce * av - cv * ae, be = cv * an - cn * av, bv = cn * ae - ce * an;

            for (int k = 0; k < around; k++)
            {
                double angle = 2.0 * System.Math.PI * k / around;
                double cosine = System.Math.Cos(angle);
                double sine = System.Math.Sin(angle);
                seeds.Add(Nudged(from, departure, to, arrival,
                                 displacement * (cosine * an + sine * bn),
                                 displacement * (cosine * ae + sine * be),
                                 displacement * (cosine * av + sine * bv), count));
            }
            return seeds;
        }

        private static void Unit(Vector3D direction, out double north, out double east,
                                 out double vertical)
        {
            north = direction.X ?? 0;
            east = direction.Y ?? 0;
            vertical = direction.Z ?? 0;
            double length = System.Math.Sqrt(north * north + east * east + vertical * vertical);
            if (length > 0)
            {
                north /= length;
                east /= length;
                vertical /= length;
            }
        }

        private static double Squared(double value)
        {
            return value * value;
        }
    }

    /// <summary>
    /// A medium made easy along a curve rather than along a fixed direction.
    /// <para>
    /// This exists because a <see cref="DirectionGuide"/> collimates but does not turn. A guide of fixed
    /// direction makes one direction cheap inside a region and fades to isotropic outside it, so the flow
    /// runs straight while it is inside and then has to turn where the guide stops: the corner does not go
    /// away, it moves to the edge of the guide. Two such guides pointing the same way, which is what a
    /// vertical departure and a horizontal target give, make a staircase — measured on Ullrigg, a
    /// departure guide on its own took the median dogleg from 9.6 to 45.2 degrees per 30 m.
    /// </para>
    /// <para>
    /// The direction therefore has to rotate along the corridor, which means the guide has to follow a
    /// curve. What is cheap here is the tangent of a spine that already honours both end tangents, so the
    /// medium asks the flow to turn the way the spine turns instead of asking it to go straight and then
    /// turn all at once.
    /// </para>
    /// </summary>
    public class PathGuideField : IFaceMobility
    {
        private readonly double[] point_;         // 3 per sample
        private readonly double[] tangent_;       // 3 per segment, the unit tangent
        private readonly int count_;
        private readonly double across_;
        private readonly double excess_;

        /// <summary>
        /// how far from the spine the guide has faded, m
        /// </summary>
        public double Width
        {
            get
            {
                return across_;
            }
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="spine">the curve to follow, at least two samples</param>
        /// <param name="width">how far across the spine the guide reaches, m</param>
        /// <param name="contrast">how much easier the tangent is than across it, on the spine</param>
        public PathGuideField(IReadOnlyList<Point3D> spine, double width, double contrast)
        {
            if (spine == null || spine.Count < 2)
            {
                throw new ArgumentException("a spine needs at least two samples", nameof(spine));
            }
            if (!(width > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }
            count_ = spine.Count;
            across_ = width;
            excess_ = contrast - 1.0;
            point_ = new double[3 * count_];
            tangent_ = new double[3 * (count_ - 1)];
            for (int i = 0; i < count_; i++)
            {
                point_[3 * i] = spine[i].X!.Value;
                point_[3 * i + 1] = spine[i].Y!.Value;
                point_[3 * i + 2] = spine[i].Z!.Value;
            }
            for (int i = 0; i + 1 < count_; i++)
            {
                double dn = point_[3 * i + 3] - point_[3 * i];
                double de = point_[3 * i + 4] - point_[3 * i + 1];
                double dv = point_[3 * i + 5] - point_[3 * i + 2];
                double length = System.Math.Sqrt(dn * dn + de * de + dv * dv);
                if (!(length > 0))
                {
                    length = 1.0;
                }
                tangent_[3 * i] = dn / length;
                tangent_[3 * i + 1] = de / length;
                tangent_[3 * i + 2] = dv / length;
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
            if (!(excess_ > 0))
            {
                return;
            }

            // the nearest point of the spine, by projection onto each segment rather than onto each
            // sample, so that the tangent the flow is offered turns as smoothly as the spine does
            double best = double.MaxValue;
            int segment = 0;
            double where = 0;
            for (int i = 0; i + 1 < count_; i++)
            {
                int at = 3 * i;
                double ax = point_[at], ay = point_[at + 1], az = point_[at + 2];
                double bx = point_[at + 3], by = point_[at + 4], bz = point_[at + 5];
                double ux = bx - ax, uy = by - ay, uz = bz - az;
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
                    segment = i;
                    where = t;
                }
            }

            double weight = System.Math.Exp(-best / (across_ * across_));
            if (weight < 1.0e-6)
            {
                return;
            }
            // between two segments the tangent is blended, so the field has no step at a sample
            int first = segment;
            int second = where > 0.5 ? System.Math.Min(segment + 1, count_ - 2)
                                     : System.Math.Max(segment - 1, 0);
            double share = where > 0.5 ? where - 0.5 : 0.5 - where;
            double tn = (1 - share) * tangent_[3 * first] + share * tangent_[3 * second];
            double te = (1 - share) * tangent_[3 * first + 1] + share * tangent_[3 * second + 1];
            double tv = (1 - share) * tangent_[3 * first + 2] + share * tangent_[3 * second + 2];
            double length = System.Math.Sqrt(tn * tn + te * te + tv * tv);
            if (!(length > 0))
            {
                return;
            }
            tn /= length;
            te /= length;
            tv /= length;

            double gain = excess_ * weight;
            values[0] += gain * tn * tn;
            values[1] += gain * te * te;
            values[2] += gain * tv * tv;
        }
    }
}
