using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// A route already taken, kept clear of so that the next relaxation has to find a different one.
    /// <para>
    /// Nudging a seed and relaxing it does not by itself give alternatives: an elastic band holds a
    /// distinct way round only where the obstacles form a barrier it cannot slide through, and a cluster
    /// of wells fanning out with depth is a picket fence rather than a wall. Measured on Ullrigg,
    /// twenty five seeds pushed as far as twelve hundred metres to every side all threaded back between
    /// the wells and settled onto one route. Making the previous route itself an obstacle is what stops
    /// that, and it is the only thing that does.
    /// </para>
    /// </summary>
    public class KeepOutTube
    {
        /// <summary>
        /// the route to stay away from
        /// </summary>
        public IReadOnlyList<Point3D>? Path { get; set; } = null;

        /// <summary>
        /// how far away to stay, m
        /// </summary>
        public double Radius { get; set; } = 150.0;

        /// <summary>
        /// the distance from a position to the route, m, or a large number when there is no route
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <returns></returns>
        public double GetDistance(double north, double east, double vertical)
        {
            if (Path == null || Path.Count < 2)
            {
                return double.MaxValue;
            }
            double best = double.MaxValue;
            for (int i = 0; i + 1 < Path.Count; i++)
            {
                double ax = Path[i].X!.Value, ay = Path[i].Y!.Value, az = Path[i].Z!.Value;
                double ux = Path[i + 1].X!.Value - ax;
                double uy = Path[i + 1].Y!.Value - ay;
                double uz = Path[i + 1].Z!.Value - az;
                double square = ux * ux + uy * uy + uz * uz;
                double t = 0;
                if (square > 0)
                {
                    t = ((north - ax) * ux + (east - ay) * uy + (vertical - az) * uz) / square;
                    t = t < 0 ? 0 : t > 1 ? 1 : t;
                }
                double cx = ax + t * ux - north, cy = ay + t * uy - east, cz = az + t * uz - vertical;
                double gap = cx * cx + cy * cy + cz * cz;
                if (gap < best) { best = gap; }
            }
            return System.Math.Sqrt(best);
        }
    }

    /// <summary>
    /// the settings of a relaxation
    /// </summary>
    public class SpineRelaxerOptions
    {
        /// <summary>
        /// routes already found, which this relaxation has to keep away from
        /// </summary>
        public List<KeepOutTube> KeepOut { get; set; } = new List<KeepOutTube>();

        /// <summary>
        /// how many samples the spine is carried at
        /// </summary>
        public int SampleCount { get; set; } = 129;

        /// <summary>
        /// how many sweeps to take
        /// </summary>
        public int IterationCount { get; set; } = 400;

        /// <summary>
        /// how far outside an uncertainty volume the spine is pushed, m. The existing well's volume is
        /// what the field holds; this is the room a planned well needs beside it on top of that.
        /// </summary>
        public double Clearance { get; set; } = 5.0;

        /// <summary>
        /// how hard each sweep pulls a sample toward the mean of its two neighbours, between zero and
        /// one half
        /// </summary>
        public double Smoothing { get; set; } = 0.3;

        /// <summary>
        /// what share of the shortfall in clearance is taken out in one sweep
        /// </summary>
        public double Push { get; set; } = 0.4;

        /// <summary>
        /// How far to look for an obstacle, m. Only wanted a little beyond the clearance, and the cost
        /// of a query grows with the cube of it, so a radius chosen for safety rather than for need is
        /// what makes a relaxation take a minute instead of a second.
        /// </summary>
        public double SearchRadius { get; set; } = 60.0;

        /// <summary>
        /// The furthest one sweep may move a sample, as a fraction of the spacing between samples.
        /// <para>
        /// Without it the repulsion wins every argument with the smoothing: where the clearance asked for
        /// cannot be had, the push never stops, and the curve settles into whatever shape balances a
        /// shove of several metres a sweep against a smoothing that moves it by centimetres. Measured
        /// before this: a seed turning at 4.4 deg per 30 m came out at 15.1.
        /// </para>
        /// </summary>
        public double StepLimit { get; set; } = 0.25;

        /// <summary>
        /// How many times the curve is doubled on the way up to its full sample count.
        /// <para>
        /// Smoothing spreads over the samples like heat, so the number of sweeps it takes to pull out a
        /// bump grows with the square of how many samples wide the bump is. A displacement halfway along
        /// a curve of a hundred and twenty nine samples would need tens of thousands. On a curve of
        /// seventeen it is a handful of samples wide and goes in hundreds, and the result carried up to
        /// the next level is already most of the way there. Without this the relaxation simply hands the
        /// seed back: nine seeds nudged to different sides stayed two to four hundred metres apart
        /// instead of settling onto the few ways round that actually exist.
        /// </para>
        /// </summary>
        public int LevelCount { get; set; } = 4;

        /// <summary>
        /// how often the samples are spread evenly again, in sweeps. Smoothing bunches them toward the
        /// inside of a bend, and an uneven spacing makes the smoothing term mean something different at
        /// every sample.
        /// </summary>
        public int RespaceEvery { get; set; } = 25;

        /// <summary>
        /// How far the curve is held straight on its departure direction before it may turn, m.
        /// <para>
        /// The kick-off point. Pinning only the first sample fixes the tangent at the slot and lets the
        /// curve leave it at once, which on Ullrigg put the directive path 20 m sideways by the depth the
        /// conduit was still holding vertical — two incompatible instructions for the same hole. What
        /// the length should be is a formation question and not a numerical one: hard rock from surface
        /// needs only the bottom hole assembly to be in the hole, thirty or forty metres, while an
        /// unconsolidated top section may need a hundred or two.
        /// </para>
        /// </summary>
        public double HoldLength { get; set; } = 0;

        /// <summary>
        /// the same, at the target end: how far the curve arrives straight along the arrival direction
        /// </summary>
        public double HoldLengthAtTarget { get; set; } = 0;

        /// <summary>
        /// How far beyond a hold the curve is eased back to being free, m.
        /// <para>
        /// Without it a hold is a step: the samples inside it are put exactly on the ray and the first
        /// one outside is left wherever the relaxation had it, which is a corner in the constraint and
        /// so a corner in the curve. Over the blend the projection is mixed with the free position,
        /// weighted from one down to nothing.
        /// </para>
        /// </summary>
        public double HoldBlend { get; set; } = 60.0;

        /// <summary>
        /// the shallowest the spine may go, m, positive downward, or null for no limit
        /// </summary>
        public double? CeilingVertical { get; set; } = null;

        /// <summary>
        /// the deepest it may go, or null for no limit
        /// </summary>
        public double? FloorVertical { get; set; } = null;
    }

    /// <summary>
    /// what a relaxation achieved
    /// </summary>
    public class SpineRelaxerResult
    {
        /// <summary>
        /// the relaxed curve
        /// </summary>
        public List<Point3D> Spine { get; internal set; } = new List<Point3D>();

        /// <summary>
        /// how many samples still sit inside an uncertainty volume
        /// </summary>
        public int InsideCount { get; internal set; }

        /// <summary>
        /// the least clearance anywhere along it, m, negative where it is inside a volume
        /// </summary>
        public double LeastClearance { get; internal set; } = double.NaN;

        /// <summary>
        /// the sharpest turn along it, rad per 30 m
        /// </summary>
        public double WorstDogleg { get; internal set; } = double.NaN;

        /// <summary>
        /// how long it is, m
        /// </summary>
        public double Length { get; internal set; }

        /// <summary>
        /// a one line account, for a log or an assertion message
        /// </summary>
        public string Describe()
        {
            return $"{Length:F0} m, {InsideCount} samples inside, least clearance "
                   + $"{LeastClearance:F1} m, worst dogleg "
                   + $"{WorstDogleg * 180.0 / System.Math.PI:F2} deg/30m";
        }
    }

    /// <summary>
    /// Pushes a curve out of the uncertainty volumes it crosses while keeping it smooth and keeping both
    /// of its ends where they were.
    /// <para>
    /// A curve drawn straight between a slot and a target knows nothing about the wells in the way, and
    /// on Ullrigg it spends a tenth of its length inside one of them. Pushing it out with a repulsion
    /// alone would trade that for a dent with a corner at each end of it, which is no better: the whole
    /// point of having a spine is that it turns gently. So each sweep does both — it pulls every sample
    /// toward the mean of its neighbours, which is what limits the curvature, and pushes the ones that
    /// are too near a volume away from it. The two ends and the two end tangents are held throughout,
    /// because those are the things the well plan is not free to choose.
    /// </para>
    /// <para>
    /// This is an elastic band, and it inherits the usual weakness: it finds a way round the obstacles
    /// in whatever direction it was already leaning, and it cannot discover that going round the other
    /// side would have been better. That is what makes it worth relaxing several seeds rather than one,
    /// and worth taking those seeds from paths the flow has already traced, since each of those has
    /// already committed to a way round.
    /// </para>
    /// </summary>
    public static class SpineRelaxer
    {
        /// <summary>
        /// Relaxes a seed curve away from the obstacles.
        /// </summary>
        /// <param name="seed">the curve to start from, at least three samples</param>
        /// <param name="obstacles"></param>
        /// <param name="departure">the direction the curve must leave its first sample in</param>
        /// <param name="arrival">the direction it must reach its last sample in</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static SpineRelaxerResult Relax(IReadOnlyList<Point3D> seed, ObstacleField obstacles,
                                               Vector3D departure, Vector3D arrival,
                                               SpineRelaxerOptions? options = null)
        {
            if (seed == null || seed.Count < 3)
            {
                throw new ArgumentException("a seed needs at least three samples", nameof(seed));
            }
            if (obstacles == null)
            {
                throw new ArgumentNullException(nameof(obstacles));
            }
            options ??= new SpineRelaxerOptions();

            Unit(departure, out double pn, out double pe, out double pv);
            Unit(arrival, out double qn, out double qe, out double qv);

            // coarse to fine: a bump is few samples wide on a short curve and goes quickly there
            int levels = System.Math.Max(1, options.LevelCount);
            int coarsest = System.Math.Max(5, options.SampleCount >> (levels - 1));
            double[] at = Respace(seed, coarsest);
            for (int level = 0; level < levels; level++)
            {
                if (level > 0)
                {
                    int wanted = level == levels - 1 ? options.SampleCount
                                                     : System.Math.Min(options.SampleCount,
                                                                       (at.Length / 3) * 2 - 1);
                    at = Respace(at, wanted);
                }
                Sweep(at, obstacles, options, pn, pe, pv, qn, qe, qv);
            }
            int count = at.Length / 3;

            SpineRelaxerResult result = new SpineRelaxerResult();
            double least = double.MaxValue;
            for (int i = 0; i < count; i++)
            {
                int k = 3 * i;
                result.Spine.Add(new Point3D(at[k], at[k + 1], at[k + 2]));
                double excess = obstacles.GetNearestExcess(at[k], at[k + 1], at[k + 2],
                                                           options.SearchRadius);
                if (excess < 0) { result.InsideCount++; }
                if (excess < least) { least = excess; }
                if (i > 0)
                {
                    result.Length += System.Math.Sqrt(Squared(at[k] - at[k - 3])
                                                      + Squared(at[k + 1] - at[k - 2])
                                                      + Squared(at[k + 2] - at[k - 1]));
                }
            }
            result.LeastClearance = least;
            result.WorstDogleg = StreamlineCurvature.GetWorst(new Streamline(result.Spine));
            return result;
        }

        /// <summary>
        /// one run of sweeps at whatever sample count the curve is currently carried at
        /// </summary>
        private static void Sweep(double[] at, ObstacleField obstacles, SpineRelaxerOptions options,
                                  double pn, double pe, double pv,
                                  double qn, double qe, double qv)
        {
            int count = at.Length / 3;
            double[] next = new double[at.Length];
            double spacing = Length(at) / System.Math.Max(1, count - 1);
            for (int sweep = 0; sweep < options.IterationCount; sweep++)
            {
                Array.Copy(at, next, at.Length);
                for (int i = 1; i + 1 < count; i++)
                {
                    int k = 3 * i;
                    // toward the mean of the two neighbours, which is what keeps the turn gentle
                    for (int a = 0; a < 3; a++)
                    {
                        double mean = 0.5 * (at[k - 3 + a] + at[k + 3 + a]);
                        next[k + a] += options.Smoothing * (mean - at[k + a]);
                    }
                    // and away from anything too close
                    double excess = obstacles.GetNearestExcess(at[k], at[k + 1], at[k + 2],
                                                               options.SearchRadius);
                    double cap = options.StepLimit * spacing;
                    if (excess < options.Clearance)
                    {
                        GetOutwardDirection(obstacles, at[k], at[k + 1], at[k + 2], options.SearchRadius,
                                            out double gn, out double ge, out double gv);
                        double want = options.Push * (options.Clearance - excess);
                        if (want > cap) { want = cap; }
                        next[k] += want * gn;
                        next[k + 1] += want * ge;
                        next[k + 2] += want * gv;
                    }
                    // and away from any route already taken, which is what keeps this one different
                    foreach (KeepOutTube tube in options.KeepOut)
                    {
                        double gap = tube.GetDistance(at[k], at[k + 1], at[k + 2]);
                        if (gap >= tube.Radius)
                        {
                            continue;
                        }
                        GetAwayFromTube(tube, at[k], at[k + 1], at[k + 2],
                                        out double tn, out double te, out double tv);
                        double want = options.Push * (tube.Radius - gap);
                        if (want > cap) { want = cap; }
                        next[k] += want * tn;
                        next[k + 1] += want * te;
                        next[k + 2] += want * tv;
                    }
                    if (options.CeilingVertical != null && next[k + 2] < options.CeilingVertical.Value)
                    {
                        next[k + 2] = options.CeilingVertical.Value;
                    }
                    if (options.FloorVertical != null && next[k + 2] > options.FloorVertical.Value)
                    {
                        next[k + 2] = options.FloorVertical.Value;
                    }
                }
                Array.Copy(next, at, at.Length);

                // the ends and the two end tangents are not the plan's to choose, and neither is the
                // stretch of hole before the kick-off point
                HoldStraight(at, count, options.HoldLength, options.HoldBlend, true, pn, pe, pv);
                HoldStraight(at, count, options.HoldLengthAtTarget, options.HoldBlend, false,
                             -qn, -qe, -qv);

                if (options.RespaceEvery > 0 && (sweep + 1) % options.RespaceEvery == 0)
                {
                    double[] spread = Respace(at, count);
                    Array.Copy(spread, at, at.Length);
                    HoldStraight(at, count, options.HoldLength, options.HoldBlend, true, pn, pe, pv);
                    HoldStraight(at, count, options.HoldLengthAtTarget, options.HoldBlend, false,
                                 -qn, -qe, -qv);
                }
            }
        }

        /// <summary>
        /// Which way is out, by differencing the clearance.
        /// <para>
        /// The clearance is the only thing the obstacle field offers, so its gradient has to be taken by
        /// hand. Where the differences give nothing — deep inside a volume where every neighbour is just
        /// as far in, or out of range of everything — there is no direction to push and none is given.
        /// </para>
        /// </summary>
        private static void GetOutwardDirection(ObstacleField obstacles, double north, double east,
                                                double vertical, double radius,
                                                out double dn, out double de, out double dv)
        {
            const double step = 1.0;
            dn = obstacles.GetNearestExcess(north + step, east, vertical, radius)
                 - obstacles.GetNearestExcess(north - step, east, vertical, radius);
            de = obstacles.GetNearestExcess(north, east + step, vertical, radius)
                 - obstacles.GetNearestExcess(north, east - step, vertical, radius);
            dv = obstacles.GetNearestExcess(north, east, vertical + step, radius)
                 - obstacles.GetNearestExcess(north, east, vertical - step, radius);
            double length = System.Math.Sqrt(dn * dn + de * de + dv * dv);
            if (length > 1.0e-9)
            {
                dn /= length;
                de /= length;
                dv /= length;
            }
            else
            {
                dn = 0;
                de = 0;
                dv = 0;
            }
        }

        /// <summary>
        /// which way is away from a route already taken
        /// </summary>
        private static void GetAwayFromTube(KeepOutTube tube, double north, double east, double vertical,
                                            out double dn, out double de, out double dv)
        {
            const double step = 1.0;
            dn = tube.GetDistance(north + step, east, vertical)
                 - tube.GetDistance(north - step, east, vertical);
            de = tube.GetDistance(north, east + step, vertical)
                 - tube.GetDistance(north, east - step, vertical);
            dv = tube.GetDistance(north, east, vertical + step)
                 - tube.GetDistance(north, east, vertical - step);
            double length = System.Math.Sqrt(dn * dn + de * de + dv * dv);
            if (length > 1.0e-9)
            {
                dn /= length;
                de /= length;
                dv /= length;
            }
            else
            {
                dn = 0;
                de = 0;
                dv = 0;
            }
        }

        /// <summary>
        /// Puts every sample within a given length of one end back onto the ray that end leaves along,
        /// keeping how far along it each one sits. With a length of zero this is just the one neighbour,
        /// which fixes the tangent and nothing more.
        /// </summary>
        private static void HoldStraight(double[] at, int count, double length, double blend,
                                         bool fromStart, double dn, double de, double dv)
        {
            int end = fromStart ? 0 : count - 1;
            int step = fromStart ? 1 : -1;
            int e = 3 * end;
            if (blend < 0)
            {
                blend = 0;
            }
            for (int k = 1; k < count; k++)
            {
                int i = end + step * k;
                int n = 3 * i;
                double reach = System.Math.Sqrt(Squared(at[n] - at[e]) + Squared(at[n + 1] - at[e + 1])
                                                + Squared(at[n + 2] - at[e + 2]));
                // the first neighbour is always put on the ray, since that is what fixes the tangent
                double weight = 1.0;
                if (k > 1)
                {
                    if (reach > length + blend)
                    {
                        break;
                    }
                    if (reach > length && blend > 0)
                    {
                        // eased out rather than dropped, so the constraint has no step in it
                        double u = (reach - length) / blend;
                        weight = 1.0 - u * u * (3.0 - 2.0 * u);
                    }
                }
                at[n] += weight * (at[e] + reach * dn - at[n]);
                at[n + 1] += weight * (at[e + 1] + reach * de - at[n + 1]);
                at[n + 2] += weight * (at[e + 2] + reach * dv - at[n + 2]);
            }
        }

        /// <summary>
        /// the same curve carried at evenly spaced samples
        /// </summary>
        /// <summary>
        /// how long a flat run of samples is, m
        /// </summary>
        private static double Length(double[] flat)
        {
            double total = 0;
            for (int i = 3; i < flat.Length; i += 3)
            {
                total += System.Math.Sqrt(Squared(flat[i] - flat[i - 3]) + Squared(flat[i + 1] - flat[i - 2])
                                          + Squared(flat[i + 2] - flat[i - 1]));
            }
            return total;
        }

        private static double[] Respace(IReadOnlyList<Point3D> path, int count)
        {
            double[] flat = new double[3 * path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                flat[3 * i] = path[i].X!.Value;
                flat[3 * i + 1] = path[i].Y!.Value;
                flat[3 * i + 2] = path[i].Z!.Value;
            }
            return Respace(flat, count);
        }

        private static double[] Respace(double[] flat, int count)
        {
            int had = flat.Length / 3;
            double[] along = new double[had];
            for (int i = 1; i < had; i++)
            {
                along[i] = along[i - 1] + System.Math.Sqrt(Squared(flat[3 * i] - flat[3 * i - 3])
                                                           + Squared(flat[3 * i + 1] - flat[3 * i - 2])
                                                           + Squared(flat[3 * i + 2] - flat[3 * i - 1]));
            }
            double total = along[had - 1];
            double[] spread = new double[3 * count];
            if (!(total > 0))
            {
                Array.Copy(flat, spread, System.Math.Min(flat.Length, spread.Length));
                return spread;
            }
            int cursor = 0;
            for (int i = 0; i < count; i++)
            {
                double want = total * i / (count - 1);
                while (cursor + 2 < had && along[cursor + 1] < want)
                {
                    cursor++;
                }
                double run = along[cursor + 1] - along[cursor];
                double t = run > 0 ? (want - along[cursor]) / run : 0;
                for (int a = 0; a < 3; a++)
                {
                    spread[3 * i + a] = flat[3 * cursor + a]
                                        + t * (flat[3 * cursor + 3 + a] - flat[3 * cursor + a]);
                }
            }
            return spread;
        }

        private static void Unit(Vector3D direction, out double north, out double east,
                                 out double vertical)
        {
            north = direction?.X ?? 0;
            east = direction?.Y ?? 0;
            vertical = direction?.Z ?? 0;
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
}
