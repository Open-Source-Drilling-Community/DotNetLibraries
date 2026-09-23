using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// what trimming a corridor back from the faults it grazes came to
    /// </summary>
    public class FaultTrimOutcome
    {
        /// <summary>
        /// the faults the corridor was pulled back from
        /// </summary>
        public List<string> Trimmed { get; } = new List<string>();

        /// <summary>
        /// the faults it still goes through, because pulling back from them would have cost more of the
        /// corridor than the limit allows
        /// </summary>
        public List<string> Kept { get; } = new List<string>();

        /// <summary>
        /// every fault that reaches inside the corridor at all, and what giving it up would cost on its
        /// own as a fraction of the cross-section that loses most
        /// </summary>
        public List<(string Fault, double Cost, bool Given)> Costed { get; }
            = new List<(string, double, bool)>();

        /// <summary>
        /// the faults the median itself goes through, which no amount of pulling the boundary in can
        /// avoid and which are therefore left to the crossing rule
        /// </summary>
        public List<string> Unavoidable { get; } = new List<string>();

        /// <summary>
        /// for each fault given up, what it overlapped before the pull-back and what it still overlaps
        /// after it
        /// </summary>
        public List<(string Fault, double Before, double After)> Residual { get; }
            = new List<(string, double, double)>();

        /// <summary>
        /// how much of the cross-section was given up, as a fraction, at the section that lost most
        /// </summary>
        public double AreaFractionLost { get; internal set; } = 0;

        /// <summary>
        /// how many member streamlines no longer fit the corridor and were dropped
        /// </summary>
        public int DroppedMembers { get; internal set; } = 0;

        /// <summary>
        /// how many are left
        /// </summary>
        public int KeptMembers { get; internal set; } = 0;

        /// <summary>
        /// whether anything was actually pulled back
        /// </summary>
        public bool Changed
        {
            get { return Trimmed.Count > 0; }
        }
    }

    /// <summary>
    /// Pulls a corridor back from the faults it only grazes, and leaves alone the ones it genuinely has
    /// to cross.
    /// <para>
    /// The distinction is what the whole thing turns on. A fault square across a corridor is a fault the
    /// well has to go through, and the honest answer is to say so and let the crossing rule judge it. A
    /// fault that clips the edge of the corridor is different: the corridor is only that wide because
    /// the streamlines happened to spread that far, and giving up the sliver beyond the fault costs
    /// almost nothing. So the test is how much of the cross-section would have to be given up, and the
    /// two cases are told apart by a limit on that.
    /// </para>
    /// <para>
    /// Pulling back is not a local act. A realization is one normalized position held the whole way
    /// along the tube — <see cref="CrossSectionStation.Apply"/> places it at <c>r</c> times the
    /// boundary at <c>theta</c>, at every cross-section — so a position ruled out where the fault
    /// is has to be ruled out everywhere, or the realization would simply reappear further along. The
    /// limit is therefore worked out per direction as the least any cross-section allows, and applied
    /// to all of them.
    /// </para>
    /// <para>
    /// And the density has to be rebuilt rather than merely thinned. Trimming rescales the tube, so a
    /// streamline that stays is at a different normalized radius afterwards than it was before; a
    /// density left alone would be reporting positions that no longer mean what they meant.
    /// </para>
    /// </summary>
    public static class FaultTrim
    {
        /// <summary>
        /// how much of a cross-section may be given up to avoid a fault before the fault is taken to be
        /// genuinely in the way instead
        /// </summary>
        public const double DefaultMaximumOverlap = 0.50;

        /// <summary>
        /// how far short of a fault the boundary is pulled back, m, so that it clears the fault rather
        /// than resting on it
        /// </summary>
        public const double Standoff = 0.5;

        /// <summary>
        /// Pulls the corridor back from every fault it grazes, drops the streamlines that no longer fit,
        /// and rebuilds the density over what is left.
        /// </summary>
        public static FaultTrimOutcome Apply(StreamlineBundleFactory factory,
                                             IReadOnlyList<FaultSurface> faults,
                                             IReadOnlyList<Streamline> members,
                                             double maximumOverlap)
        {
            FaultTrimOutcome outcome = new FaultTrimOutcome();
            outcome.KeptMembers = members?.Count ?? 0;
            int directions = 0;
            foreach (CrossSectionStation station in factory.Stations)
            {
                if (station.Polygon != null) { directions = station.Polygon.DirectionCount; break; }
            }
            if (directions == 0 || faults == null || faults.Count == 0)
            {
                return outcome;
            }

            double[] limit = new double[directions];
            for (int d = 0; d < directions; d++) { limit[d] = 1.0; }

            // Each fault costed on its own first, then taken in order of what it costs.
            // The limit has to hold for the corridor and not merely for one fault at a time: three
            // faults that each cost a fifth of the cross-section cost well over half of it together,
            // and a corridor trimmed that hard is no longer the corridor the flow found. So a fault is
            // only given up when giving it up leaves the whole still inside the limit, and the cheapest
            // are taken first so that what is spent buys as many faults as it can.
            // A fault the median itself goes through cannot be avoided by pulling the boundary in,
            // and trying to is what made a nonsense of this.
            // The limit kept for a direction is the least any cross-section allows, and what each
            // cross-section allows is whatever keeps the median's side of the fault. That is coherent
            // only while the median stays on one side. Where the median crosses, the sections before
            // the crossing ask for one side and the sections after ask for the other, and the least of
            // the two pulls the boundary in from both at once: the corridor collapses, and the cost
            // comes out at nine tenths of the section for a fault that merely passes through it.
            // Measured on Ullrigg: every corridor of the vertical sidetrack has exactly one such fault
            // and two or three it only grazes.
            // So the crossing is the test. A fault the median goes through is kept and left to the
            // obliquity rule, which is the right answer anyway: the corridor has to get past it, and
            // what matters is how squarely.
            List<Point3D> spine = new List<Point3D>(factory.SharedHead);
            spine.AddRange(factory.GetMedianCurve().Positions!);

            List<(FaultSurface Fault, double[] Limit, double Loss)> candidates
                = new List<(FaultSurface, double[], double)>();
            foreach (FaultSurface fault in faults)
            {
                if (FaultCrossings.Find(spine, new[] { fault }).Count > 0)
                {
                    outcome.Kept.Add(fault.Name ?? "?");
                    outcome.Unavoidable.Add(fault.Name ?? "?");
                    continue;
                }
                double[] own = new double[directions];
                for (int d = 0; d < directions; d++) { own[d] = 1.0; }
                double alone = GetLimits(factory, fault, own);
                if (!(alone > 0))
                {
                    // the fault never reaches inside this corridor
                    continue;
                }
                candidates.Add((fault, own, alone));
                outcome.Costed.Add((fault.Name ?? "?", alone, false));
            }
            candidates.Sort((a, b) => a.Loss.CompareTo(b.Loss));

            foreach ((FaultSurface fault, double[] own, double alone) in candidates)
            {
                if (alone >= maximumOverlap)
                {
                    outcome.Kept.Add(fault.Name ?? "?");
                    continue;
                }
                for (int d = 0; d < directions; d++)
                {
                    if (own[d] < limit[d]) { limit[d] = own[d]; }
                }
                outcome.Trimmed.Add(fault.Name ?? "?");
                for (int c = 0; c < outcome.Costed.Count; c++)
                {
                    if (string.Equals(outcome.Costed[c].Fault, fault.Name ?? "?",
                                      StringComparison.Ordinal))
                    {
                        outcome.Costed[c] = (outcome.Costed[c].Fault, outcome.Costed[c].Cost, true);
                    }
                }
            }
            if (outcome.Trimmed.Count == 0)
            {
                return outcome;
            }

            outcome.AreaFractionLost = Shrink(factory, limit, directions);

            // Did the pull-back actually remove what it claimed to? Asked of the trimmed corridor, so
            // a fault still reaching inside says the trim did not take rather than that it was never
            // tried.
            foreach ((FaultSurface fault, double[] own, double alone) in candidates)
            {
                if (!outcome.Trimmed.Contains(fault.Name ?? "?")) { continue; }
                double[] after = new double[directions];
                for (int d = 0; d < directions; d++) { after[d] = 1.0; }
                double left = GetLimits(factory, fault, after);
                outcome.Residual.Add((fault.Name ?? "?", alone, left));
            }
            Refill(factory, members, ref outcome);
            return outcome;
        }

        /// <summary>
        /// the most any one cross-section gives up under the given per-direction limits
        /// </summary>
        private static double GetWorstLoss(StreamlineBundleFactory factory, double[] limit)
        {
            double worst = 0;
            foreach (CrossSectionStation station in factory.Stations)
            {
                CrossSectionPolygon? region = station.Polygon;
                if (region == null || region.DirectionCount != limit.Length) { continue; }
                double loss = GetAreaLoss(region, limit);
                if (loss > worst) { worst = loss; }
            }
            return worst;
        }

        /// <summary>
        /// How far in every direction one fault forces the corridor, as a fraction of the boundary, and
        /// how much of a cross-section that costs at the section that loses most.
        /// </summary>
        private static double GetLimits(StreamlineBundleFactory factory, FaultSurface fault,
                                        double[] limit)
        {
            int directions = limit.Length;
            double[] triangles = fault.GetTriangles(FaultCrossings.PillarSamples);
            double worstLoss = 0;
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
                double[] here = new double[directions];
                for (int d = 0; d < directions; d++) { here[d] = 1.0; }
                bool met = false;
                for (int t = 0; t + 8 < triangles.Length; t += 9)
                {
                    if (!FaultCrossings.GetSectionCut(triangles, t, centre, along, first, second,
                                                      out double[] cutU, out double[] cutV))
                    {
                        continue;
                    }
                    met |= Bite(station.Polygon, cutU, cutV, here);
                }
                if (!met) { continue; }
                double loss = GetAreaLoss(station.Polygon, here);
                if (loss > worstLoss) { worstLoss = loss; }
                for (int d = 0; d < directions; d++)
                {
                    if (here[d] < limit[d]) { limit[d] = here[d]; }
                }
            }
            return worstLoss;
        }

        /// <summary>
        /// How far out each direction may still reach before it meets the segment the fault cuts across
        /// this cross-section. The part of the section beyond the segment is what is given up, and the
        /// median is on the near side of it whenever the loss is small enough to be worth giving up.
        /// </summary>
        private static bool Bite(CrossSectionPolygon region, double[] cutU, double[] cutV,
                                 double[] here)
        {
            bool met = false;
            double edgeU = cutU[1] - cutU[0];
            double edgeV = cutV[1] - cutV[0];
            for (int d = 0; d < region.DirectionCount; d++)
            {
                double direction = region.GetDirection(d);
                double rayU = System.Math.Cos(direction);
                double rayV = System.Math.Sin(direction);
                double denominator = rayU * edgeV - rayV * edgeU;
                if (System.Math.Abs(denominator) < 1.0e-12)
                {
                    continue;
                }
                double reach = (cutU[0] * edgeV - cutV[0] * edgeU) / denominator;
                double share = (cutU[0] * rayV - cutV[0] * rayU) / denominator;
                if (share < 0 || share > 1 || reach <= 0)
                {
                    continue;
                }
                double boundary = region.GetBoundaryRadius(direction);
                if (!(boundary > 0) || reach >= boundary)
                {
                    continue;
                }
                // stop short of the fault rather than on it, so the boundary clears it
                double allowed = (reach - Standoff) / boundary;
                if (allowed < 0) { allowed = 0; }
                if (allowed < here[d]) { here[d] = allowed; }
                met = true;
            }
            return met;
        }

        /// <summary>
        /// what fraction of a cross-section is given up by the given per-direction limits, the section
        /// being measured as the sectors its boundary sweeps
        /// </summary>
        private static double GetAreaLoss(CrossSectionPolygon region, double[] limit)
        {
            double whole = 0;
            double left = 0;
            for (int d = 0; d < region.DirectionCount; d++)
            {
                double boundary = region.GetBoundaryRadius(region.GetDirection(d));
                whole += boundary * boundary;
                left += boundary * limit[d] * boundary * limit[d];
            }
            return whole > 0 ? 1.0 - left / whole : 0;
        }

        /// <summary>
        /// Pulls every cross-section in by the limits, and says how much the worst of them lost. The
        /// support distance of a direction is capped rather than scaled, so what is promised is a
        /// boundary no further out than the limit allows whichever half-plane happens to bound it.
        /// </summary>
        private static double Shrink(StreamlineBundleFactory factory, double[] limit, int directions)
        {
            double worst = 0;
            foreach (CrossSectionStation station in factory.Stations)
            {
                CrossSectionPolygon? region = station.Polygon;
                if (region == null || region.DirectionCount != directions) { continue; }
                double[] wanted = new double[directions];
                for (int d = 0; d < directions; d++)
                {
                    wanted[d] = limit[d] * region.GetBoundaryRadius(region.GetDirection(d));
                }
                double loss = GetAreaLoss(region, limit);
                if (loss > worst) { worst = loss; }
                for (int d = 0; d < directions; d++)
                {
                    if (wanted[d] < region.SupportDistance[d])
                    {
                        region.SetSupportDistance(d, wanted[d]);
                    }
                }
                region.Measure();
            }
            return worst;
        }

        /// <summary>
        /// Drops the streamlines that no longer fit inside the corridor and rebuilds the density from
        /// those that do, in the same way the builder did: every cross-section of every surviving
        /// streamline, at the normalized position the trimmed boundary gives it.
        /// </summary>
        private static void Refill(StreamlineBundleFactory factory,
                                   IReadOnlyList<Streamline>? members,
                                   ref FaultTrimOutcome outcome)
        {
            if (members == null || factory.Density == null)
            {
                return;
            }
            List<(int Station, double Radius, double Angle)> samples
                = new List<(int, double, double)>();
            List<List<(int, double, double)>> perMember = new List<List<(int, double, double)>>();
            List<bool> fits = new List<bool>();
            foreach (Streamline member in members)
            {
                samples = Measure(factory, member);
                bool inside = true;
                foreach ((int station, double radius, double angle) in samples)
                {
                    CrossSectionPolygon? region = factory.Stations[station].Polygon;
                    if (region == null) { continue; }
                    double boundary = region.GetBoundaryRadius(angle);
                    if (boundary > 0 && radius > boundary * (1.0 + 1.0e-9))
                    {
                        inside = false;
                        break;
                    }
                }
                fits.Add(inside);
                perMember.Add(samples);
            }

            double[] counts = factory.Density.CellCounts;
            Array.Clear(counts, 0, counts.Length);
            int kept = 0;
            for (int m = 0; m < perMember.Count; m++)
            {
                if (!fits[m]) { continue; }
                kept++;
                foreach ((int station, double radius, double angle) in perMember[m])
                {
                    CrossSectionPolygon? region = factory.Stations[station].Polygon;
                    if (region == null) { continue; }
                    double boundary = region.GetBoundaryRadius(angle);
                    if (!(boundary > 0)) { continue; }
                    factory.Density.Add(System.Math.Min(1.0, radius / boundary), angle);
                }
            }
            factory.Density.Prepare();
            outcome.KeptMembers = kept;
            outcome.DroppedMembers = perMember.Count - kept;
        }

        /// <summary>
        /// where one streamline crosses each cross-section, as a radius and an untwisted angle in the
        /// frame of that cross-section
        /// </summary>
        private static List<(int Station, double Radius, double Angle)> Measure(
                                        StreamlineBundleFactory factory, Streamline member)
        {
            List<(int, double, double)> found = new List<(int, double, double)>();
            List<Point3D>? positions = member.Positions;
            if (positions == null || positions.Count < 2) { return found; }
            for (int k = 0; k < factory.Stations.Count; k++)
            {
                CrossSectionStation station = factory.Stations[k];
                if (station.Position == null || station.Tangent == null
                    || station.FirstNormal == null || station.SecondNormal == null)
                {
                    continue;
                }
                double[] centre = { station.Position.X!.Value, station.Position.Y!.Value,
                                    station.Position.Z!.Value };
                double[] along = { station.Tangent.X!.Value, station.Tangent.Y!.Value,
                                   station.Tangent.Z!.Value };
                double best = double.MaxValue;
                double bestU = 0, bestV = 0;
                double before = 0;
                for (int j = 0; j < positions.Count; j++)
                {
                    double here = (positions[j].X!.Value - centre[0]) * along[0]
                                  + (positions[j].Y!.Value - centre[1]) * along[1]
                                  + (positions[j].Z!.Value - centre[2]) * along[2];
                    if (j > 0 && ((before <= 0 && here >= 0) || (before >= 0 && here <= 0)))
                    {
                        double span = here - before;
                        double share = System.Math.Abs(span) > 1.0e-12 ? -before / span : 0;
                        double[] on = new double[3];
                        on[0] = positions[j - 1].X!.Value
                                + share * (positions[j].X!.Value - positions[j - 1].X!.Value)
                                - centre[0];
                        on[1] = positions[j - 1].Y!.Value
                                + share * (positions[j].Y!.Value - positions[j - 1].Y!.Value)
                                - centre[1];
                        on[2] = positions[j - 1].Z!.Value
                                + share * (positions[j].Z!.Value - positions[j - 1].Z!.Value)
                                - centre[2];
                        double u = on[0] * station.FirstNormal.X!.Value
                                   + on[1] * station.FirstNormal.Y!.Value
                                   + on[2] * station.FirstNormal.Z!.Value;
                        double v = on[0] * station.SecondNormal.X!.Value
                                   + on[1] * station.SecondNormal.Y!.Value
                                   + on[2] * station.SecondNormal.Z!.Value;
                        double offset = u * u + v * v;
                        if (offset < best) { best = offset; bestU = u; bestV = v; }
                    }
                    before = here;
                }
                if (best == double.MaxValue) { continue; }
                found.Add((k, System.Math.Sqrt(best),
                           System.Math.Atan2(bestV, bestU) - station.Twist));
            }
            return found;
        }
    }
}
