using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// What came of building the factories of a whole bundling together.
    /// </summary>
    public class StreamlineFactorySet
    {
        /// <summary>
        /// the factories, one per bundle that could produce one
        /// </summary>
        public List<StreamlineBundleFactory> Factories { get; internal set; }
            = new List<StreamlineBundleFactory>();

        /// <summary>
        /// the streamlines of each factory, by their index in the source
        /// </summary>
        public List<List<int>> Members { get; internal set; } = new List<List<int>>();

        /// <summary>
        /// why each bundle that produced no factory did not
        /// </summary>
        public List<(int Members, StreamlineFactoryFailureReason Reason)> Refused { get; internal set; }
            = new List<(int, StreamlineFactoryFailureReason)>();

        /// <summary>
        /// how many times a bundle had to be split because its members did not fit in one convex
        /// tolerance region
        /// </summary>
        public int SplitCount { get; internal set; } = 0;

        /// <summary>
        /// How many rounds the build took. A round rebuilds every pending bundle and lays every region
        /// again, so this multiplies both of the costs below.
        /// </summary>
        public int RoundCount { get; internal set; } = 0;

        /// <summary>
        /// how many factory fits were run, counting a bundle refitted in a later round again
        /// </summary>
        public int FitCount { get; internal set; } = 0;

        /// <summary>
        /// how many fits were answered from the ones already done rather than run again
        /// </summary>
        public int ReusedFitCount { get; internal set; } = 0;

        /// <summary>
        /// how long those fits took in total, ms
        /// </summary>
        public double FitMilliseconds { get; internal set; } = 0;

        /// <summary>
        /// how long laying the tolerance regions took in total, ms
        /// </summary>
        public double ToleranceMilliseconds { get; internal set; } = 0;

        /// <summary>
        /// the marches and march steps of every region pass added together
        /// </summary>
        public long MarchCount { get; internal set; } = 0;

        /// <summary>
        /// how many steps those marches took
        /// </summary>
        public long MarchStepCount { get; internal set; } = 0;

        /// <summary>
        /// The routes: the corridors grouped by which of them nothing forbidden separates. This is what a
        /// caller asks for realizations from, with the limits it is willing to accept.
        /// </summary>
        /// <returns></returns>
        public List<StreamlineMetaFactory> GetRoutes()
        {
            Dictionary<int, StreamlineMetaFactory> byRoute = new Dictionary<int, StreamlineMetaFactory>();
            for (int f = 0; f < Factories.Count; f++)
            {
                int route = Tolerance != null && f < Tolerance.Group.Length ? Tolerance.Group[f] : 0;
                if (!byRoute.TryGetValue(route, out StreamlineMetaFactory? meta))
                {
                    meta = new StreamlineMetaFactory(route);
                    byRoute[route] = meta;
                }
                meta.Add(Factories[f]);
            }
            List<StreamlineMetaFactory> routes = new List<StreamlineMetaFactory>(byRoute.Values);
            routes.Sort((a, b) => a.Route.CompareTo(b.Route));
            return routes;
        }

        /// <summary>
        /// which corridors touch, and which groups of touching corridors they fall into. A group is a
        /// route that is genuinely apart from the others; the corridors inside one are the same route
        /// described more finely.
        /// </summary>
        public ToleranceRegionResult? Tolerance { get; internal set; } = null;
    }

    /// <summary>
    /// Builds the factories of a whole bundling at once.
    /// <para>
    /// One at a time is not enough, because two of the three things a tolerance region has to satisfy are
    /// relations between regions rather than properties of one: a region built alone can avoid the
    /// forbidden zones, but it cannot know that it is claiming ground another corridor already claims.
    /// </para>
    /// <para>
    /// The regions are convex, which is what makes them honest as a tolerance — a driller controls a
    /// deviation, not a route through a notch — and it is also what obliges a bundle to be split when its
    /// members will not fit in one. A bundle whose paths pass either side of an obstruction has no convex
    /// region containing them that does not also contain the obstruction, so it is two corridors and is
    /// reported as two.
    /// </para>
    /// </summary>
    public class StreamlineFactorySetBuilder
    {
        /// <summary>
        /// how the individual factories are fitted
        /// </summary>
        public StreamlineBundleFactoryOptions Factory { get; set; }
            = new StreamlineBundleFactoryOptions();

        /// <summary>
        /// how the tolerance regions are laid
        /// </summary>
        public ToleranceRegionOptions Tolerance { get; set; } = new ToleranceRegionOptions();

        /// <summary>
        /// The share of its cross-sections a streamline may fall outside its own tolerance region before
        /// it is taken out of that bundle. A member outside the region is not represented by it, and if
        /// enough of them are outside the bundle was not one corridor.
        /// </summary>
        public double OutsideFractionLimit { get; set; } = 0.25;

        /// <summary>
        /// how many times a bundle may be split before the remainder is accepted as it stands
        /// </summary>
        public int MaximumSplitRounds { get; set; } = 3;

        /// <summary>
        /// Builds a factory for every bundle, with the tolerance regions laid across all of them at once
        /// and bundles split where one convex region will not hold them.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bundles">the members of each bundle, by their index in the source</param>
        /// <param name="zones">what a well may not enter, or null for open ground</param>
        /// <returns></returns>
        public StreamlineFactorySet Build(IStreamlineSource source,
                                          IReadOnlyList<IReadOnlyList<int>> bundles,
                                          IForbiddenZoneField? zones)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (bundles == null)
            {
                throw new ArgumentNullException(nameof(bundles));
            }

            StreamlineFactorySet set = new StreamlineFactorySet();
            // What a bundle fits to depends on nothing but the source and its own members, so a bundle
            // carried unchanged into a later round fits to exactly what it fitted to before. A split
            // changes one bundle and leaves the rest alone, yet every round used to refit all of them;
            // on Ullrigg's flat case that was 67 fits where 24 bundles were ever distinct.
            // The key is the member list in the order it is held, not as a set: the provisional median
            // is a sum over the members, and a sum in a different order is not bit for bit the same.
            Dictionary<string, StreamlineBundleFactory> alreadyFitted
                = new Dictionary<string, StreamlineBundleFactory>(StringComparer.Ordinal);
            Dictionary<string, StreamlineFactoryFailureReason> alreadyRefused
                = new Dictionary<string, StreamlineFactoryFailureReason>(StringComparer.Ordinal);
            List<List<int>> pending = new List<List<int>>();
            foreach (IReadOnlyList<int> bundle in bundles)
            {
                pending.Add(new List<int>(bundle));
            }

            for (int round = 0; round <= MaximumSplitRounds; round++)
            {
                set.Factories.Clear();
                set.Members.Clear();
                set.Refused.Clear();
                StreamlineBundleFactoryBuilder builder = new StreamlineBundleFactoryBuilder(Factory);
                List<List<int>> kept = new List<List<int>>();
                set.RoundCount = round + 1;
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                foreach (List<int> members in pending)
                {
                    string signature = GetSignature(members);
                    if (alreadyRefused.TryGetValue(signature,
                                                   out StreamlineFactoryFailureReason refusedBefore))
                    {
                        set.Refused.Add((members.Count, refusedBefore));
                        continue;
                    }
                    if (!alreadyFitted.TryGetValue(signature,
                                                   out StreamlineBundleFactory? factory))
                    {
                        watch.Restart();
                        factory = builder.Build(source, members,
                                                out StreamlineFactoryFailureReason reason);
                        set.FitMilliseconds += watch.Elapsed.TotalMilliseconds;
                        set.FitCount++;
                        if (factory == null)
                        {
                            alreadyRefused[signature] = reason;
                            set.Refused.Add((members.Count, reason));
                            continue;
                        }
                        alreadyFitted[signature] = factory;
                    }
                    else
                    {
                        set.ReusedFitCount++;
                    }
                    set.Factories.Add(factory);
                    kept.Add(members);
                }
                // A median inside a forbidden zone is a statement about the bundle, not about its
                // members. Every streamline in it is clear — they were traced through open cells — so if
                // their middle is not, they must pass either side of the zone, which is two corridors and
                // not one. So the bundle is split there rather than refused, and only a bundle too small
                // to split is given up on. Splitting also has to happen before any region is laid: a
                // median inside a zone has no room to march from, its region collapses to a point, and
                // every member then reads as unrepresented, which is the degenerate case the
                // outside-fraction rule below cannot act on.
                List<List<int>> straddling = new List<List<int>>();
                if (zones != null)
                {
                    List<StreamlineBundleFactory> clear = new List<StreamlineBundleFactory>();
                    List<List<int>> clearMembers = new List<List<int>>();
                    for (int f = 0; f < set.Factories.Count; f++)
                    {
                        int offending = GetStationInsideZone(set.Factories[f], zones);
                        if (offending < 0)
                        {
                            clear.Add(set.Factories[f]);
                            clearMembers.Add(kept[f]);
                            continue;
                        }
                        if (round < MaximumSplitRounds
                            && SplitAcross(source, kept[f], set.Factories[f], offending,
                                           out List<int> first, out List<int> second)
                            && first.Count >= Factory.MinimumStreamlineCount
                            && second.Count >= Factory.MinimumStreamlineCount)
                        {
                            straddling.Add(first);
                            straddling.Add(second);
                            set.SplitCount++;
                            continue;
                        }
                        set.Refused.Add((kept[f].Count,
                                         StreamlineFactoryFailureReason.MedianInsideForbiddenZone));
                    }
                    set.Factories = clear;
                    kept = clearMembers;
                }
                set.Members = kept;
                if (straddling.Count > 0)
                {
                    // the bundles that straddle are taken round again along with everything else, so that
                    // the regions are laid only once all the medians are ones that could be planned
                    List<List<int>> again = new List<List<int>>(straddling);
                    again.AddRange(kept);
                    pending = again;
                    continue;
                }
                if (set.Factories.Count == 0)
                {
                    return set;
                }

                // every region laid with every median present, so that no two of them claim one place
                System.Diagnostics.Stopwatch laying = System.Diagnostics.Stopwatch.StartNew();
                set.Tolerance = ToleranceRegionBuilder.Build(set.Factories, zones, Tolerance);
                set.ToleranceMilliseconds += laying.Elapsed.TotalMilliseconds;
                set.MarchCount += set.Tolerance.MarchCount;
                set.MarchStepCount += set.Tolerance.MarchStepCount;

                // and then the members re-expressed in the regions that were actually laid, rather than
                // in the provisional outlines each factory fitted on its own
                List<List<int>> next = new List<List<int>>();
                bool split = false;
                for (int f = 0; f < set.Factories.Count; f++)
                {
                    List<int> outside = Refit(source, kept[f], set.Factories[f]);
                    if (round < MaximumSplitRounds && outside.Count > 0
                        && kept[f].Count - outside.Count >= Factory.MinimumStreamlineCount
                        && outside.Count >= Factory.MinimumStreamlineCount)
                    {
                        List<int> stays = new List<int>();
                        foreach (int member in kept[f])
                        {
                            if (!outside.Contains(member)) { stays.Add(member); }
                        }
                        next.Add(stays);
                        next.Add(outside);
                        set.SplitCount++;
                        split = true;
                    }
                    else
                    {
                        next.Add(kept[f]);
                    }
                }
                if (!split)
                {
                    return set;
                }
                pending = next;
            }
            return set;
        }

        /// <summary>
        /// The cross-section where the median is furthest inside a forbidden zone, or -1 when it stays
        /// clear all the way.
        /// </summary>
        /// <summary>
        /// What names a bundle for the purpose of reusing its fit: its members, in order.
        /// </summary>
        private static string GetSignature(IReadOnlyList<int> members)
        {
            System.Text.StringBuilder text = new System.Text.StringBuilder(8 * members.Count);
            foreach (int member in members)
            {
                text.Append(member).Append(',');
            }
            return text.ToString();
        }

        private int GetStationInsideZone(StreamlineBundleFactory factory, IForbiddenZoneField zones)
        {
            int worst = -1;
            double deepest = 0;
            for (int k = 0; k < factory.Stations.Count; k++)
            {
                Point3D? at = factory.Stations[k].Position;
                if (at == null)
                {
                    continue;
                }
                double clearance = zones.GetClearance(at.X!.Value, at.Y!.Value, at.Z!.Value,
                                                      Tolerance.MaximumTolerance);
                if (clearance <= 0 && -clearance >= deepest)
                {
                    deepest = -clearance;
                    worst = k;
                }
            }
            return worst;
        }

        /// <summary>
        /// Divides the members in two by which side of the obstruction they pass, using where they cross
        /// the offending cross-section.
        /// <para>
        /// Two means over the crossings, started from the two that are furthest apart, which on a bundle
        /// that straddles something is the one split there is: the crossings lie in two groups with the
        /// zone between them, and the pair furthest apart has one member in each.
        /// </para>
        /// </summary>
        private bool SplitAcross(IStreamlineSource source, IReadOnlyList<int> members,
                                 StreamlineBundleFactory factory, int station,
                                 out List<int> first, out List<int> second)
        {
            first = new List<int>();
            second = new List<int>();
            if (station < 0 || station >= factory.Stations.Count)
            {
                return false;
            }
            CrossSectionStation at = factory.Stations[station];
            List<int> present = new List<int>();
            List<double> u = new List<double>();
            List<double> v = new List<double>();
            foreach (int member in members)
            {
                List<Point3D> positions = new List<Point3D>();
                foreach ((double X, double Y, double Z) step in source.GetPositions(member))
                {
                    positions.Add(new Point3D(step.X, step.Y, step.Z));
                }
                if (GetCrossing(positions, at, out double here, out double there))
                {
                    present.Add(member);
                    u.Add(here);
                    v.Add(there);
                }
                else
                {
                    first.Add(member);
                }
            }
            if (present.Count < 2)
            {
                return false;
            }

            int one = 0, other = 0;
            double furthest = -1;
            for (int a = 0; a < present.Count; a++)
            {
                for (int b = a + 1; b < present.Count; b++)
                {
                    double du = u[a] - u[b], dv = v[a] - v[b];
                    double apart = du * du + dv * dv;
                    if (apart > furthest) { furthest = apart; one = a; other = b; }
                }
            }
            if (!(furthest > 0))
            {
                return false;
            }
            double firstU = u[one], firstV = v[one], secondU = u[other], secondV = v[other];
            int[] side = new int[present.Count];
            for (int round = 0; round < 8; round++)
            {
                double sumFirstU = 0, sumFirstV = 0, sumSecondU = 0, sumSecondV = 0;
                int countFirst = 0, countSecond = 0;
                for (int i = 0; i < present.Count; i++)
                {
                    double toFirst = (u[i] - firstU) * (u[i] - firstU) + (v[i] - firstV) * (v[i] - firstV);
                    double toSecond = (u[i] - secondU) * (u[i] - secondU)
                                      + (v[i] - secondV) * (v[i] - secondV);
                    side[i] = toFirst <= toSecond ? 0 : 1;
                    if (side[i] == 0) { sumFirstU += u[i]; sumFirstV += v[i]; countFirst++; }
                    else { sumSecondU += u[i]; sumSecondV += v[i]; countSecond++; }
                }
                if (countFirst == 0 || countSecond == 0)
                {
                    return false;
                }
                firstU = sumFirstU / countFirst;
                firstV = sumFirstV / countFirst;
                secondU = sumSecondU / countSecond;
                secondV = sumSecondV / countSecond;
            }
            for (int i = 0; i < present.Count; i++)
            {
                if (side[i] == 0) { first.Add(present[i]); } else { second.Add(present[i]); }
            }
            return true;
        }

        /// <summary>
        /// Re-expresses the members of one factory in the tolerance region that was laid on it, fits the
        /// density from them, and reports which of them the region does not represent.
        /// </summary>
        private List<int> Refit(IStreamlineSource source, IReadOnlyList<int> members,
                                StreamlineBundleFactory factory)
        {
            int stationCount = factory.Stations.Count;
            int memberCount = members.Count;
            double[] angle = new double[memberCount];
            double[] radius = new double[memberCount];
            List<int> outside = new List<int>();

            double[] crossU = new double[memberCount * stationCount];
            double[] crossV = new double[memberCount * stationCount];
            bool[] present = new bool[memberCount * stationCount];
            List<double> fractions = new List<double>();

            for (int m = 0; m < memberCount; m++)
            {
                List<Point3D> positions = new List<Point3D>();
                foreach ((double X, double Y, double Z) at in source.GetPositions(members[m]))
                {
                    positions.Add(new Point3D(at.X, at.Y, at.Z));
                }
                double sine = 0, cosine = 0;
                fractions.Clear();
                int beyond = 0, seen = 0;
                for (int k = 0; k < stationCount; k++)
                {
                    CrossSectionStation station = factory.Stations[k];
                    if (station.Polygon == null) { continue; }
                    if (!GetCrossing(positions, station, out double u, out double v)) { continue; }
                    int at = m * stationCount + k;
                    crossU[at] = u;
                    crossV[at] = v;
                    present[at] = true;
                    seen++;
                    double reach = System.Math.Sqrt(u * u + v * v);
                    double direction = System.Math.Atan2(v, u);
                    double boundary = station.Polygon.GetBoundaryRadius(direction);
                    sine += System.Math.Sin(direction);
                    cosine += System.Math.Cos(direction);
                    if (boundary > 0)
                    {
                        double share = reach / boundary;
                        fractions.Add(System.Math.Min(1.0, share));
                        if (share > 1.0) { beyond++; }
                    }
                }
                if (sine != 0 || cosine != 0) { angle[m] = System.Math.Atan2(sine, cosine); }
                if (fractions.Count > 0)
                {
                    fractions.Sort();
                    radius[m] = fractions[fractions.Count / 2];
                }
                if (seen > 0 && (double)beyond / seen > OutsideFractionLimit)
                {
                    outside.Add(members[m]);
                }
            }

            int cells = (int)System.Math.Round(memberCount / Factory.DensityCellPopulation);
            if (cells < 1) { cells = 1; }
            if (cells > Factory.MaximumDensityCellCount) { cells = Factory.MaximumDensityCellCount; }
            int rings = (int)System.Math.Round(System.Math.Sqrt(cells / 3.0));
            if (rings < 1) { rings = 1; }
            PolarDensity density = new PolarDensity(rings, 1.0);

            double sum = 0;
            long count = 0;
            double[] stationSum = new double[stationCount];
            int[] stationCounts = new int[stationCount];
            for (int m = 0; m < memberCount; m++)
            {
                for (int k = 0; k < stationCount; k++)
                {
                    int at = m * stationCount + k;
                    if (!present[at]) { continue; }
                    CrossSectionStation station = factory.Stations[k];
                    double reach = System.Math.Sqrt(crossU[at] * crossU[at] + crossV[at] * crossV[at]);
                    double direction = System.Math.Atan2(crossV[at], crossU[at]);
                    double boundary = station.Polygon!.GetBoundaryRadius(direction);
                    if (boundary > 0)
                    {
                        density.Add(System.Math.Min(1.0, reach / boundary), direction);
                    }
                    double predicted = radius[m] * station.Polygon.GetBoundaryRadius(angle[m]);
                    double eu = crossU[at] - predicted * System.Math.Cos(angle[m]);
                    double ev = crossV[at] - predicted * System.Math.Sin(angle[m]);
                    double squared = eu * eu + ev * ev;
                    sum += squared;
                    count++;
                    stationSum[k] += squared;
                    stationCounts[k]++;
                }
            }
            density.Prepare();
            factory.Density = density;
            factory.NormalizedRadius = 1.0;
            factory.ResidualDeviation = count > 0 ? System.Math.Sqrt(sum / count) : 0;

            double bundleRadius = 0;
            int radiusCount = 0;
            for (int k = 0; k < stationCount; k++)
            {
                bundleRadius += factory.Stations[k].Radius;
                radiusCount++;
                factory.Stations[k].ResidualDeviation = stationCounts[k] > 0
                    ? System.Math.Sqrt(stationSum[k] / stationCounts[k]) : 0;
            }
            bundleRadius = radiusCount > 0 ? bundleRadius / radiusCount : 0;
            factory.ResidualFraction = bundleRadius > 0 ? factory.ResidualDeviation / bundleRadius : 0;
            factory.OutsideMemberCount = outside.Count;
            return outside;
        }

        /// <summary>
        /// where a streamline crosses the plane of a station, keeping the crossing nearest the median
        /// when it wanders back through
        /// </summary>
        private static bool GetCrossing(List<Point3D> positions, CrossSectionStation station,
                                        out double u, out double v)
        {
            u = 0;
            v = 0;
            bool found = false;
            double nearest = double.MaxValue;
            double px = station.Position!.X!.Value;
            double py = station.Position!.Y!.Value;
            double pz = station.Position!.Z!.Value;
            double tx = station.Tangent!.X!.Value;
            double ty = station.Tangent!.Y!.Value;
            double tz = station.Tangent!.Z!.Value;
            double previous = 0;
            for (int i = 0; i < positions.Count; i++)
            {
                double dx = positions[i].X!.Value - px;
                double dy = positions[i].Y!.Value - py;
                double dz = positions[i].Z!.Value - pz;
                double along = dx * tx + dy * ty + dz * tz;
                if (i > 0 && ((previous <= 0 && along >= 0) || (previous >= 0 && along <= 0))
                    && previous != along)
                {
                    double fraction = -previous / (along - previous);
                    double x = positions[i - 1].X!.Value
                               + fraction * (positions[i].X!.Value - positions[i - 1].X!.Value);
                    double y = positions[i - 1].Y!.Value
                               + fraction * (positions[i].Y!.Value - positions[i - 1].Y!.Value);
                    double z = positions[i - 1].Z!.Value
                               + fraction * (positions[i].Z!.Value - positions[i - 1].Z!.Value);
                    double ex = x - px, ey = y - py, ez = z - pz;
                    double here = ex * station.FirstNormal!.X!.Value + ey * station.FirstNormal!.Y!.Value
                                  + ez * station.FirstNormal!.Z!.Value;
                    double there = ex * station.SecondNormal!.X!.Value
                                   + ey * station.SecondNormal!.Y!.Value
                                   + ez * station.SecondNormal!.Z!.Value;
                    double offset = here * here + there * there;
                    if (offset < nearest)
                    {
                        nearest = offset;
                        u = here;
                        v = there;
                        found = true;
                    }
                }
                previous = along;
            }
            return found;
        }
    }
}
