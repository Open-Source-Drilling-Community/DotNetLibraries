using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// Settings for turning a set of medians into the tolerance regions around them.
    /// </summary>
    public class ToleranceRegionOptions
    {
        /// <summary>
        /// How far a well may be allowed to deviate from its planned path at the very most, m, whatever
        /// the room around it. A corridor through open ground would otherwise have no bound at all, and a
        /// deviation of hundreds of metres is not something a directional driller would take: it is paid
        /// for twice over, in the hole drilled to get back and in the hole that is no longer where it was
        /// meant to be.
        /// </summary>
        public double MaximumTolerance { get; set; } = 50.0;

        /// <summary>
        /// how many directions the region is bounded in. The region is the intersection of one half-plane
        /// per direction, so this is the resolution with which it can follow the shape of the room
        /// available.
        /// </summary>
        public int DirectionCount { get; set; } = 36;

        /// <summary>
        /// how far to step when marching out along a direction, m
        /// </summary>
        public double MarchStep { get; set; } = 0.5;

        /// <summary>
        /// How much clear of a forbidden zone the boundary is kept, m. Zero puts the boundary on the
        /// surface of the zone, which is inside the tolerance of nothing.
        /// </summary>
        public double ZoneStandoff { get; set; } = 0.0;

        /// <summary>
        /// How much of a forbidden zone has to lie between two corridors before they count as separated,
        /// m.
        /// <para>
        /// Without it, a segment clipping the tip of one volume at a single cross-section would declare
        /// two corridors where there is one. A fork is a fork only if there is something to go round.
        /// </para>
        /// </summary>
        public double MinimumHoleLength { get; set; } = 10.0;

        /// <summary>
        /// Over how many cross-sections either side the support distances are taken at their smallest, so
        /// that a region does not jump in and out as the room changes from one station to the next. A
        /// running minimum, never a mean: shrinking a region is always safe and growing one never is.
        /// </summary>
        public int SmoothingHalfWidth { get; set; } = 2;

        /// <summary>
        /// What share of a corridor is left out of <see cref="StreamlineBundleFactory.LeastRoom"/> at each
        /// end. The paths start together at the slot and converge on the target, so both ends are pinched
        /// by construction and neither says anything about the room in between.
        /// </summary>
        public double EndExclusionFraction { get; set; } = 0.1;

        /// <summary>
        /// checks that the settings are usable
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool IsValid(out string? reason)
        {
            reason = null;
            if (!(MaximumTolerance > 0))
            {
                reason = "MaximumTolerance must be greater than zero.";
            }
            else if (DirectionCount < 3)
            {
                reason = "DirectionCount must be at least three.";
            }
            else if (!(MarchStep > 0))
            {
                reason = "MarchStep must be greater than zero.";
            }
            else if (ZoneStandoff < 0)
            {
                reason = "ZoneStandoff cannot be negative.";
            }
            return reason == null;
        }
    }

    /// <summary>
    /// What the tolerance regions came to, besides the regions themselves.
    /// </summary>
    public class ToleranceRegionResult
    {
        /// <summary>
        /// whether each pair of corridors touches: they come within reach of one another somewhere and
        /// nothing forbidden lies between them for any appreciable length
        /// </summary>
        public bool[,] Contiguous { get; internal set; } = new bool[0, 0];

        /// <summary>
        /// Which group of touching corridors each one belongs to.
        /// <para>
        /// Reported and not acted on. A group is the connected components of the touching relation, and
        /// the members of a group are not therefore interchangeable: a streamline never changes bundle,
        /// so corridors are not merged on the strength of being reachable through a third.
        /// </para>
        /// </summary>
        public int[] Group { get; internal set; } = Array.Empty<int>();

        /// <summary>
        /// how many such groups there are, which is the number of routes that are genuinely apart
        /// </summary>
        public int GroupCount { get; internal set; } = 0;
    }

    /// <summary>
    /// Builds the convex tolerance region of every cross-section of every factory, with all the factories
    /// present at once.
    /// <para>
    /// Together rather than one at a time, because two of the three things a region has to satisfy are
    /// relations between regions rather than properties of one. A region built alone can only promise that
    /// it avoids the forbidden zones; it cannot promise that it does not claim ground another corridor has
    /// already claimed. Here a point belongs to the corridor whose median is nearest, which partitions the
    /// space and makes the regions disjoint without any pair of them having to negotiate.
    /// </para>
    /// </summary>
    public static class ToleranceRegionBuilder
    {
        /// <summary>
        /// Lays a convex tolerance region on every cross-section of the given factories.
        /// </summary>
        /// <param name="factories">the factories, whose medians and frames are already built</param>
        /// <param name="zones">what a well may not enter, or null for open ground</param>
        /// <param name="options"></param>
        public static ToleranceRegionResult Build(IReadOnlyList<StreamlineBundleFactory> factories,
                                                  IForbiddenZoneField? zones,
                                                  ToleranceRegionOptions? options = null)
        {
            if (factories == null)
            {
                throw new ArgumentNullException(nameof(factories));
            }
            options ??= new ToleranceRegionOptions();
            if (!options.IsValid(out string? reason))
            {
                throw new ArgumentException(reason, nameof(options));
            }

            // The region is the intersection of one half-plane per direction, so between two adjacent
            // directions its boundary is a chord, which bulges past both by one over the cosine of the
            // half angle between them. At the default resolution that is 1/cos(5 degrees), four parts in
            // a thousand — less than the step the march moves in — so the bulge is not worth correcting.
            // It was corrected once, by pulling both neighbouring half-planes in whenever a midpoint
            // reached too far, and that was a bad idea twice over: the problem it was aimed at turned out
            // to be medians lying inside a zone rather than any bulge, and because each half-plane is the
            // neighbour of two midpoints the cuts compounded round the circle and shrank every region to
            // a couple of march steps.
            int directions = options.DirectionCount;
            double[] cosine = new double[directions];
            double[] sine = new double[directions];
            for (int s = 0; s < directions; s++)
            {
                double angle = 2.0 * System.Math.PI * s / directions;
                cosine[s] = System.Math.Cos(angle);
                sine[s] = System.Math.Sin(angle);
            }

            // Which corridors touch, worked out from the medians before any region is laid. The
            // partition between two corridors is only real where something forbidden lies between them:
            // elsewhere it is an artefact of having to divide the space, and clipping a tolerance at it
            // would report a well as having less room than the ground actually gives it.
            bool[,] contiguous = GetContiguity(factories, zones, options);
            MedianIndex index = new MedianIndex(factories, options.MaximumTolerance);
            for (int f = 0; f < factories.Count; f++)
            {
                StreamlineBundleFactory factory = factories[f];
                foreach (CrossSectionStation station in factory.Stations)
                {
                    if (station.Position == null || station.FirstNormal == null
                        || station.SecondNormal == null)
                    {
                        continue;
                    }
                    CrossSectionPolygon region = new CrossSectionPolygon(directions);
                    int blocked = 0;
                    for (int s = 0; s < directions; s++)
                    {
                        double reach = March(station, cosine[s], sine[s], index, f, contiguous,
                                             zones, options, out bool stoppedByZone);
                        region.SetSupportDistance(s, reach);
                        if (stoppedByZone) { blocked++; }
                    }
                    region.BlockedDirectionCount = blocked;
                    region.Measure();
                    station.Polygon = region;
                    // the region is built about the median in the frame of the cross-section, so there is
                    // no turn left for a twist to carry
                    station.Twist = 0;
                }
            }
            Smooth(factories, options);
            MeasureRoom(factories, options);

            int[] group = GetGroups(contiguous, factories.Count, out int groupCount);
            return new ToleranceRegionResult
            {
                Contiguous = contiguous,
                Group = group,
                GroupCount = groupCount
            };
        }

        /// <summary>
        /// the least room each corridor offers, over the stretch of it that means anything
        /// </summary>
        private static void MeasureRoom(IReadOnlyList<StreamlineBundleFactory> factories,
                                        ToleranceRegionOptions options)
        {
            double share = System.Math.Max(0, System.Math.Min(0.45, options.EndExclusionFraction));
            foreach (StreamlineBundleFactory factory in factories)
            {
                int count = factory.Stations.Count;
                int first = (int)(count * share);
                int last = count - 1 - first;
                double least = double.MaxValue;
                for (int k = first; k <= last && k < count; k++)
                {
                    CrossSectionPolygon? region = factory.Stations[k].Polygon;
                    if (region != null && region.Inradius < least)
                    {
                        least = region.Inradius;
                    }
                }
                factory.LeastRoom = least == double.MaxValue ? 0 : least;
            }
        }

        /// <summary>
        /// Which pairs of corridors touch: they come within reach of each other somewhere, and nowhere
        /// along the stretch where they do is there an appreciable length of forbidden ground between
        /// their medians.
        /// </summary>
        private static bool[,] GetContiguity(IReadOnlyList<StreamlineBundleFactory> factories,
                                             IForbiddenZoneField? zones,
                                             ToleranceRegionOptions options)
        {
            int count = factories.Count;
            bool[,] contiguous = new bool[count, count];
            double reach = 2.0 * options.MaximumTolerance;
            for (int a = 0; a < count; a++)
            {
                for (int b = a + 1; b < count; b++)
                {
                    bool touching = IsTouching(factories[a], factories[b], zones, options, reach);
                    contiguous[a, b] = touching;
                    contiguous[b, a] = touching;
                }
            }
            return contiguous;
        }

        private static bool IsTouching(StreamlineBundleFactory first, StreamlineBundleFactory second,
                                       IForbiddenZoneField? zones, ToleranceRegionOptions options,
                                       double reach)
        {
            bool metAnywhere = false;
            int run = 0;
            double spacing = first.StationSpacing > 0 ? first.StationSpacing : 1.0;
            foreach (CrossSectionStation station in first.Stations)
            {
                Point3D? from = station.Position;
                if (from == null) { continue; }
                Point3D? to = GetNearestOnMedian(second, from);
                if (to == null) { continue; }
                double dx = to.X!.Value - from.X!.Value;
                double dy = to.Y!.Value - from.Y!.Value;
                double dz = to.Z!.Value - from.Z!.Value;
                double apart = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
                if (apart > reach)
                {
                    run = 0;
                    continue;
                }
                metAnywhere = true;
                if (zones == null || !IsSegmentBlocked(from, dx, dy, dz, apart, zones, options))
                {
                    run = 0;
                    continue;
                }
                run++;
                if (run * spacing >= options.MinimumHoleLength)
                {
                    return false;
                }
            }
            return metAnywhere;
        }

        /// <summary>
        /// whether anything forbidden lies on the straight line between the two medians here
        /// </summary>
        private static bool IsSegmentBlocked(Point3D from, double dx, double dy, double dz, double apart,
                                             IForbiddenZoneField zones, ToleranceRegionOptions options)
        {
            if (!(apart > 0))
            {
                return false;
            }
            double step = System.Math.Max(options.MarchStep, apart / 64.0);
            for (double at = step; at < apart; at += step)
            {
                double share = at / apart;
                double x = from.X!.Value + share * dx;
                double y = from.Y!.Value + share * dy;
                double z = from.Z!.Value + share * dz;
                if (zones.GetClearance(x, y, z, options.MaximumTolerance) <= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static Point3D? GetNearestOnMedian(StreamlineBundleFactory factory, Point3D from)
        {
            Point3D? best = null;
            double nearest = double.MaxValue;
            foreach (CrossSectionStation station in factory.Stations)
            {
                Point3D? at = station.Position;
                if (at == null) { continue; }
                double dx = at.X!.Value - from.X!.Value;
                double dy = at.Y!.Value - from.Y!.Value;
                double dz = at.Z!.Value - from.Z!.Value;
                double distance = dx * dx + dy * dy + dz * dz;
                if (distance < nearest) { nearest = distance; best = at; }
            }
            return best;
        }

        /// <summary>
        /// the connected components of the touching relation
        /// </summary>
        private static int[] GetGroups(bool[,] contiguous, int count, out int groupCount)
        {
            int[] group = new int[count];
            for (int f = 0; f < count; f++) { group[f] = -1; }
            groupCount = 0;
            Queue<int> waiting = new Queue<int>();
            for (int f = 0; f < count; f++)
            {
                if (group[f] >= 0) { continue; }
                group[f] = groupCount;
                waiting.Enqueue(f);
                while (waiting.Count > 0)
                {
                    int at = waiting.Dequeue();
                    for (int other = 0; other < count; other++)
                    {
                        if (other == at || group[other] >= 0 || !contiguous[at, other]) { continue; }
                        group[other] = groupCount;
                        waiting.Enqueue(other);
                    }
                }
                groupCount++;
            }
            return group;
        }

        /// <summary>
        /// Takes each support distance at its smallest over the neighbouring cross-sections, so that a
        /// region does not jump as the room changes and a streamline drawn along its edge does not wander
        /// with it. Shrinking is always safe, which is why this is a running minimum and not an average.
        /// </summary>
        private static void Smooth(IReadOnlyList<StreamlineBundleFactory> factories,
                                   ToleranceRegionOptions options)
        {
            int half = System.Math.Max(0, options.SmoothingHalfWidth);
            if (half == 0)
            {
                return;
            }
            foreach (StreamlineBundleFactory factory in factories)
            {
                int count = factory.Stations.Count;
                List<double[]> taken = new List<double[]>(count);
                for (int k = 0; k < count; k++)
                {
                    CrossSectionPolygon? region = factory.Stations[k].Polygon;
                    if (region == null) { taken.Add(Array.Empty<double>()); continue; }
                    double[] here = new double[region.DirectionCount];
                    for (int s = 0; s < region.DirectionCount; s++)
                    {
                        double least = double.MaxValue;
                        for (int about = -half; about <= half; about++)
                        {
                            int at = k + about;
                            if (at < 0 || at >= count) { continue; }
                            CrossSectionPolygon? other = factory.Stations[at].Polygon;
                            if (other == null || other.DirectionCount != region.DirectionCount)
                            {
                                continue;
                            }
                            if (other.SupportDistance[s] < least) { least = other.SupportDistance[s]; }
                        }
                        here[s] = least == double.MaxValue ? region.SupportDistance[s] : least;
                    }
                    taken.Add(here);
                }
                for (int k = 0; k < count; k++)
                {
                    CrossSectionPolygon? region = factory.Stations[k].Polygon;
                    if (region == null || taken[k].Length == 0) { continue; }
                    for (int s = 0; s < taken[k].Length; s++)
                    {
                        region.SetSupportDistance(s, taken[k][s]);
                    }
                    region.Measure();
                }
            }
        }

        /// <summary>
        /// How far the region may reach in one direction: out to the first forbidden zone, to the point
        /// where another corridor's median is nearer than this one's, or to the cap, whichever comes first.
        /// </summary>
        private static double March(CrossSectionStation station, double cosine, double sine,
                                    MedianIndex index, int own, bool[,] contiguous,
                                    IForbiddenZoneField? zones, ToleranceRegionOptions options,
                                    out bool stoppedByZone)
        {
            stoppedByZone = false;
            double px = station.Position!.X!.Value;
            double py = station.Position!.Y!.Value;
            double pz = station.Position!.Z!.Value;
            double dx = cosine * station.FirstNormal!.X!.Value + sine * station.SecondNormal!.X!.Value;
            double dy = cosine * station.FirstNormal!.Y!.Value + sine * station.SecondNormal!.Y!.Value;
            double dz = cosine * station.FirstNormal!.Z!.Value + sine * station.SecondNormal!.Z!.Value;

            double reached = 0;
            for (double at = options.MarchStep; at <= options.MaximumTolerance; at += options.MarchStep)
            {
                double x = px + at * dx, y = py + at * dy, z = pz + at * dz;
                if (zones != null)
                {
                    double clearance = zones.GetClearance(x, y, z, options.MaximumTolerance);
                    if (clearance <= options.ZoneStandoff)
                    {
                        stoppedByZone = true;
                        break;
                    }
                }
                // a corridor that touches this one is not a boundary: there is nothing between them,
                // so a well may deviate across it. Only a corridor kept apart by forbidden ground stops
                // the march.
                if (index.IsNearerToAnother(x, y, z, own, at, contiguous))
                {
                    break;
                }
                reached = at;
            }
            return reached;
        }

        /// <summary>
        /// Every median point of every factory, in a uniform grid so that the nearest one to a position
        /// can be found without walking all of them.
        /// <para>
        /// It has to be an index and not a scan. A region is marched out in a couple of dozen directions
        /// at every cross-section of every factory, and asking "is another corridor nearer here" by
        /// comparing against some thousands of median points at each step of each march is four orders of
        /// magnitude more work than the obstacle queries it sits beside.
        /// </para>
        /// </summary>
        private sealed class MedianIndex
        {
            private readonly Dictionary<(int, int, int), List<int>> cells_
                = new Dictionary<(int, int, int), List<int>>();
            private readonly List<(double X, double Y, double Z, int Factory)> points_
                = new List<(double, double, double, int)>();
            private readonly double size_;

            public MedianIndex(IReadOnlyList<StreamlineBundleFactory> factories, double size)
            {
                size_ = size > 0 ? size : 1.0;
                for (int f = 0; f < factories.Count; f++)
                {
                    foreach (CrossSectionStation station in factories[f].Stations)
                    {
                        Point3D? at = station.Position;
                        if (at == null) { continue; }
                        int index = points_.Count;
                        points_.Add((at.X!.Value, at.Y!.Value, at.Z!.Value, f));
                        (int, int, int) key = GetKey(at.X!.Value, at.Y!.Value, at.Z!.Value);
                        if (!cells_.TryGetValue(key, out List<int>? bucket))
                        {
                            bucket = new List<int>();
                            cells_[key] = bucket;
                        }
                        bucket.Add(index);
                    }
                }
            }

            private (int, int, int) GetKey(double x, double y, double z)
            {
                return ((int)System.Math.Floor(x / size_), (int)System.Math.Floor(y / size_),
                        (int)System.Math.Floor(z / size_));
            }

            /// <summary>
            /// whether a factory other than the given one has a median point nearer to the position
            /// </summary>
            public bool IsNearerToAnother(double x, double y, double z, int own, double reach,
                                          bool[,] contiguous)
            {
                int span = (int)System.Math.Ceiling(reach / size_) + 1;
                (int ix, int iy, int iz) = GetKey(x, y, z);
                double mine = double.MaxValue;
                double others = double.MaxValue;
                for (int a = -span; a <= span; a++)
                {
                    for (int b = -span; b <= span; b++)
                    {
                        for (int c = -span; c <= span; c++)
                        {
                            if (!cells_.TryGetValue((ix + a, iy + b, iz + c), out List<int>? bucket))
                            {
                                continue;
                            }
                            foreach (int index in bucket)
                            {
                                (double px, double py, double pz, int factory) = points_[index];
                                double dx = x - px, dy = y - py, dz = z - pz;
                                double distance = dx * dx + dy * dy + dz * dz;
                                if (factory == own)
                                {
                                    if (distance < mine) { mine = distance; }
                                }
                                else if (!contiguous[own, factory] && distance < others)
                                {
                                    others = distance;
                                }
                            }
                        }
                    }
                }
                return others < mine;
            }
        }
    }
}
