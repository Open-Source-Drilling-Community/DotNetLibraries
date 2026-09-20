using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// Builds a <see cref="StreamlineBundleFactory"/> from a bundle of streamlines.
    /// <para>
    /// The median curve and the cross-sections define each other: the cross-sections are perpendicular to
    /// the median curve, and the median curve passes through the middle of the crossings found in them.
    /// The fit is therefore iterative. A provisional median is obtained by averaging the streamlines at
    /// equal fractions of their length; cross-sections are laid out along it; the streamlines are cut by
    /// them; the geometric median of each set of crossings gives the next median curve; and so on.
    /// </para>
    /// <para>
    /// The median curve is then smoothed, because the geometric median wanders sideways wherever the
    /// cross-section is lopsided, and that wandering shows up as tangent the bundle does not have.
    /// Smoothing buys shape at the price of position, so it is taken only as far as
    /// <see cref="StreamlineBundleFactoryOptions.MedianDisplacementBudget"/> allows the curve to move away
    /// from the geometric median.
    /// </para>
    /// <para>
    /// The transforms are fitted together with a normalized coordinate per streamline, by alternating
    /// least squares. Fitting every cross-section against one shared normalized frame, rather than
    /// against the cross-section before it, means no error accumulates along the curve.
    /// </para>
    /// </summary>
    public class StreamlineBundleFactoryBuilder
    {
        private const int ProvisionalSampleCount = 256;
        private const int WeiszfeldIterationCount = 48;

        /// <summary>
        /// How far the smoothing of the median curve is allowed to reach. The sideways jitter it is there
        /// to remove is independent from one cross-section to the next, so a few passes carry it away;
        /// the displacement budget alone would not stop the smoothing, being an average over the whole
        /// curve, from diffusing one bad end a long way into the middle.
        /// </summary>
        private const int MaximumSmoothingPassCount = 32;

        /// <summary>
        /// the settings used by the builder
        /// </summary>
        public StreamlineBundleFactoryOptions Options { get; set; } = new StreamlineBundleFactoryOptions();

        /// <summary>
        /// default constructor
        /// </summary>
        public StreamlineBundleFactoryBuilder()
        {
        }

        /// <summary>
        /// constructor with initialization
        /// </summary>
        /// <param name="options"></param>
        public StreamlineBundleFactoryBuilder(StreamlineBundleFactoryOptions options)
        {
            Options = options ?? new StreamlineBundleFactoryOptions();
        }

        /// <summary>
        /// builds a factory from the given streamlines, taken as one bundle
        /// </summary>
        /// <param name="streamlines"></param>
        /// <param name="reason"></param>
        /// <returns>null when the bundle is not a usable basis for a factory</returns>
        public StreamlineBundleFactory? Build(IReadOnlyList<Streamline> streamlines,
                                              out StreamlineFactoryFailureReason reason)
        {
            if (streamlines == null)
            {
                throw new ArgumentNullException(nameof(streamlines));
            }
            int[] members = new int[streamlines.Count];
            for (int i = 0; i < members.Length; i++)
            {
                members[i] = i;
            }
            return Build(new StreamlineListSource(streamlines), members, out reason);
        }

        /// <summary>
        /// builds a factory for one bundle of a bundling result
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bundle"></param>
        /// <param name="reason"></param>
        /// <returns>null when the bundle is not a usable basis for a factory</returns>
        public StreamlineBundleFactory? Build(IStreamlineSource source, StreamlineBundle bundle,
                                              out StreamlineFactoryFailureReason reason)
        {
            if (bundle == null)
            {
                throw new ArgumentNullException(nameof(bundle));
            }
            return Build(source, bundle.StreamlineIndices, out reason);
        }

        /// <summary>
        /// builds a factory for every bundle of a bundling result. An entry is null where the bundle was
        /// too small or too thin to be replaced by a surrogate, and the matching entry of
        /// <paramref name="reasons"/> says why.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="result"></param>
        /// <param name="reasons"></param>
        /// <returns></returns>
        public List<StreamlineBundleFactory?> BuildAll(IStreamlineSource source,
                                                       StreamlineBundlingResult result,
                                                       out List<StreamlineFactoryFailureReason> reasons)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            List<StreamlineBundleFactory?> factories = new List<StreamlineBundleFactory?>(result.Bundles.Count);
            reasons = new List<StreamlineFactoryFailureReason>(result.Bundles.Count);
            foreach (StreamlineBundle bundle in result.Bundles)
            {
                factories.Add(Build(source, bundle, out StreamlineFactoryFailureReason reason));
                reasons.Add(reason);
            }
            return factories;
        }

        /// <summary>
        /// builds a factory from the streamlines of the source at the given indices, taken as one bundle
        /// </summary>
        /// <param name="source"></param>
        /// <param name="members"></param>
        /// <param name="reason"></param>
        /// <returns>null when the bundle is not a usable basis for a factory</returns>
        public StreamlineBundleFactory? Build(IStreamlineSource source, IReadOnlyList<int> members,
                                              out StreamlineFactoryFailureReason reason)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (members == null)
            {
                throw new ArgumentNullException(nameof(members));
            }
            if (!Options.IsValid(out string? _))
            {
                reason = StreamlineFactoryFailureReason.InvalidOptions;
                return null;
            }
            int memberCount = members.Count;
            if (memberCount < Options.MinimumStreamlineCount)
            {
                reason = StreamlineFactoryFailureReason.TooFewStreamlines;
                return null;
            }

            // ---- whatever the members begin with in common is held back from the fit ----------------
            List<Point3D> head = GetSharedHead(source, members);
            if (head.Count > 1)
            {
                // the last position of the head stays as the first of the fitted part, so the two join
                // without a gap when the head is put back on a realization
                source = new SkippingSource(source, head.Count - 1);
            }

            // ---- a provisional median, by averaging the streamlines at equal fractions of length -----
            double[]? provisional = BuildProvisionalMedian(source, members, out int usableMembers);
            if (provisional == null || usableMembers < Options.MinimumStreamlineCount)
            {
                reason = usableMembers < Options.MinimumStreamlineCount
                    ? StreamlineFactoryFailureReason.TooFewPositions
                    : StreamlineFactoryFailureReason.DegenerateGeometry;
                return null;
            }
            double provisionalLength = GetPolylineLength(provisional, ProvisionalSampleCount);
            if (!(provisionalLength > 0))
            {
                reason = StreamlineFactoryFailureReason.DegenerateGeometry;
                return null;
            }

            double spacing = Options.StationSpacing ?? provisionalLength / Options.StationCount;
            int stationCount = (int)(provisionalLength / spacing) + 1;
            if (stationCount < 4)
            {
                stationCount = 4;
                spacing = provisionalLength / (stationCount - 1);
            }

            double[] median = new double[3 * stationCount];
            ResampleByArcLength(provisional, ProvisionalSampleCount, median, stationCount);

            // ---- alternate between laying out cross-sections and recentring the median ---------------
            double[] tangent = new double[3 * stationCount];
            double[] firstNormal = new double[3 * stationCount];
            double[] secondNormal = new double[3 * stationCount];
            float[] crossU = new float[(long)memberCount * stationCount <= int.MaxValue
                                       ? memberCount * stationCount : 0];
            float[] crossV = new float[crossU.Length];
            if (crossU.Length == 0)
            {
                reason = StreamlineFactoryFailureReason.TooFewPositions;
                return null;
            }
            double[] stationDirection = new double[3 * stationCount];
            int[] stationPopulation = new int[stationCount];
            double medianDisplacement = 0;
            double bundleRadius = 0;

            for (int iteration = 0; iteration < Options.RefinementIterationCount; iteration++)
            {
                ComputeTangents(median, stationCount, tangent);
                ComputeRotationMinimisingFrame(median, tangent, stationCount, firstNormal, secondNormal);
                CollectCrossings(source, members, median, tangent, firstNormal, secondNormal, stationCount,
                                 crossU, crossV, stationPopulation, stationDirection);

                bundleRadius = GetBundleRadius(crossU, crossV, memberCount, stationCount);
                if (!(bundleRadius > 0))
                {
                    reason = StreamlineFactoryFailureReason.DegenerateGeometry;
                    return null;
                }

                double[] centred = new double[3 * stationCount];
                double[] offsetU = new double[stationCount];
                double[] offsetV = new double[stationCount];
                for (int k = 0; k < stationCount; k++)
                {
                    GeometricMedian(crossU, crossV, memberCount, stationCount, k, bundleRadius,
                                    out offsetU[k], out offsetV[k]);
                }
                // A cross-section no streamline reached says nothing about where the middle of the bundle
                // is. Left at the position it happened to have, it becomes a fixed point the smoothing
                // then drags the whole tail towards, and on the next pass the planes there are tilted
                // enough to leave the bundle altogether, which tilts them further still. Carry the
                // nearest measured offset into it instead.
                CarryOffsetsIntoEmptyStations(stationPopulation, stationCount, offsetU, offsetV);
                for (int k = 0; k < stationCount; k++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        centred[3 * k + c] = median[3 * k + c]
                                             + offsetU[k] * firstNormal[3 * k + c]
                                             + offsetV[k] * secondNormal[3 * k + c];
                    }
                }

                medianDisplacement = SmoothWithinBudget(centred, stationCount,
                                                        Options.MedianDisplacementBudget * bundleRadius);

                // keep only the stretch the bundle actually populates, so that a cross-section whose plane
                // pokes out of the bundle is dropped rather than carried into the next pass
                int keepFrom = 0;
                int keepTo = stationCount - 1;
                int wanted = GetRequiredPopulation(stationPopulation, stationCount, memberCount);
                if (FindPopulatedRun(stationPopulation, stationCount, wanted, out int runFrom, out int runTo))
                {
                    keepFrom = runFrom;
                    keepTo = runTo;
                }
                if (keepFrom > 0 || keepTo < stationCount - 1)
                {
                    int kept = keepTo - keepFrom + 1;
                    double[] trimmed = new double[3 * kept];
                    Array.Copy(centred, 3 * keepFrom, trimmed, 0, 3 * kept);
                    ResampleByArcLength(trimmed, kept, median, stationCount);
                }
                else
                {
                    ResampleByArcLength(centred, stationCount, median, stationCount);
                }
            }

            // ---- a final pass, against the median the factory will actually carry --------------------
            ComputeTangents(median, stationCount, tangent);
            ComputeRotationMinimisingFrame(median, tangent, stationCount, firstNormal, secondNormal);
            CollectCrossings(source, members, median, tangent, firstNormal, secondNormal, stationCount,
                             crossU, crossV, stationPopulation, stationDirection);
            bundleRadius = GetBundleRadius(crossU, crossV, memberCount, stationCount);

            // ---- keep the longest run of cross-sections the bundle actually populates ----------------
            int required = GetRequiredPopulation(stationPopulation, stationCount, memberCount);
            if (!FindPopulatedRun(stationPopulation, stationCount, required, out int firstStation, out int lastStation))
            {
                reason = StreamlineFactoryFailureReason.InsufficientOverlap;
                return null;
            }

            StreamlineBundleFactory factory = new StreamlineBundleFactory
            {
                SourceStreamlineCount = memberCount,
                StationSpacing = GetSpacing(median, firstStation, lastStation),
                MedianDisplacementFraction = bundleRadius > 0 ? medianDisplacement / bundleRadius : 0
            };
            FitOutlinesAndDensity(factory, crossU, crossV, memberCount, stationCount,
                                  firstStation, lastStation, bundleRadius);
            factory.SharedHead = head.Count > 1 ? head.GetRange(0, head.Count - 1) : new List<Point3D>();
            PopulateStations(factory, median, tangent, firstNormal, secondNormal,
                             stationPopulation, stationDirection, firstStation, lastStation);

            // what it would cost to drill the median, which is the path that would be planned
            List<Point3D> planned = new List<Point3D>(factory.SharedHead);
            planned.AddRange(factory.GetMedianCurve().Positions!);
            Streamline plannedPath = new Streamline(planned);
            // GetWorst gives the turn in radians over the station it is asked for, so dividing by that
            // station's length turns it into a curvature in rad/m
            factory.MedianCurvature = StreamlineCurvature.GetWorst(plannedPath,
                                          StreamlineCurvature.StandardStation)
                                      / StreamlineCurvature.StandardStation;
            factory.MedianCurvatureLong = StreamlineCurvature.GetWorst(plannedPath, 120.0) / 120.0;

            reason = StreamlineFactoryFailureReason.None;
            return factory;
        }

        // =========================================================================================
        /// <summary>
        /// The positions every member begins with in common.
        /// <para>
        /// A conduit, or the parent a sidetrack leaves along, is prepended to each member as literally
        /// the same positions, so the comparison is for coincidence and not for nearness: anything
        /// looser would swallow the start of a bundle that genuinely converges, which is a real feature
        /// of the bundle and has to reach the fit.
        /// </para>
        /// </summary>
        private static List<Point3D> GetSharedHead(IStreamlineSource source, IReadOnlyList<int> members)
        {
            List<Point3D> head = new List<Point3D>();
            if (members.Count < 2)
            {
                return head;
            }
            const double coincident = 1.0e-6;
            foreach ((double X, double Y, double Z) at in source.GetPositions(members[0]))
            {
                head.Add(new Point3D(at.X, at.Y, at.Z));
            }
            if (head.Count < 2)
            {
                head.Clear();
                return head;
            }
            // one short of the whole of the shortest member, so something is always left for the fit
            int shared = head.Count - 1;
            for (int m = 1; m < members.Count && shared > 0; m++)
            {
                int matched = 0;
                foreach ((double X, double Y, double Z) at in source.GetPositions(members[m]))
                {
                    if (matched >= shared
                        || System.Math.Abs(head[matched].X!.Value - at.X) > coincident
                        || System.Math.Abs(head[matched].Y!.Value - at.Y) > coincident
                        || System.Math.Abs(head[matched].Z!.Value - at.Z) > coincident)
                    {
                        break;
                    }
                    matched++;
                }
                if (matched < shared) { shared = matched; }
            }
            head.RemoveRange(shared, head.Count - shared);
            return head;
        }

        /// <summary>
        /// The same streamlines with the first few positions of each skipped. A wrapper rather than a
        /// copy, because the builder reads the source more than once and a bundle of a hundred thousand
        /// streamlines is not something to materialize in order to drop a conduit off the front.
        /// </summary>
        private sealed class SkippingSource : IStreamlineSource
        {
            private readonly IStreamlineSource inner_;
            private readonly int skip_;

            public SkippingSource(IStreamlineSource inner, int skip)
            {
                inner_ = inner;
                skip_ = System.Math.Max(0, skip);
            }

            public int Count
            {
                get
                {
                    return inner_.Count;
                }
            }

            public Guid? GetID(int index)
            {
                return inner_.GetID(index);
            }

            public string? GetName(int index)
            {
                return inner_.GetName(index);
            }

            public IEnumerable<(double X, double Y, double Z)> GetPositions(int index)
            {
                int passed = 0;
                foreach ((double X, double Y, double Z) at in inner_.GetPositions(index))
                {
                    if (passed++ < skip_)
                    {
                        continue;
                    }
                    yield return at;
                }
            }
        }

        // the provisional median
        // =========================================================================================

        private double[]? BuildProvisionalMedian(IStreamlineSource source, IReadOnlyList<int> members,
                                                 out int usableMembers)
        {
            double[] accumulator = new double[3 * ProvisionalSampleCount];
            double[] sample = new double[3 * ProvisionalSampleCount];
            double[] buffer = Array.Empty<double>();
            usableMembers = 0;
            foreach (int member in members)
            {
                int count = ReadMember(source, member, ref buffer);
                if (count < 2)
                {
                    continue;
                }
                if (!ResampleByArcLength(buffer, count, sample, ProvisionalSampleCount))
                {
                    continue;
                }
                for (int i = 0; i < accumulator.Length; i++)
                {
                    accumulator[i] += sample[i];
                }
                usableMembers++;
            }
            if (usableMembers == 0)
            {
                return null;
            }
            for (int i = 0; i < accumulator.Length; i++)
            {
                accumulator[i] /= usableMembers;
            }
            return accumulator;
        }

        private static int ReadMember(IStreamlineSource source, int member, ref double[] buffer)
        {
            int count = 0;
            foreach ((double x, double y, double z) in source.GetPositions(member))
            {
                if (double.IsNaN(x) || double.IsNaN(y) || double.IsNaN(z))
                {
                    continue;
                }
                if (3 * (count + 1) > buffer.Length)
                {
                    Array.Resize(ref buffer, System.Math.Max(3 * 1024, 2 * buffer.Length));
                }
                buffer[3 * count] = x;
                buffer[3 * count + 1] = y;
                buffer[3 * count + 2] = z;
                count++;
            }
            return count;
        }

        // =========================================================================================
        // polyline helpers
        // =========================================================================================

        private static double GetPolylineLength(double[] points, int count)
        {
            double length = 0;
            for (int i = 1; i < count; i++)
            {
                length += Distance(points, i - 1, points, i);
            }
            return length;
        }

        private static double Distance(double[] first, int firstIndex, double[] second, int secondIndex)
        {
            double dx = second[3 * secondIndex] - first[3 * firstIndex];
            double dy = second[3 * secondIndex + 1] - first[3 * firstIndex + 1];
            double dz = second[3 * secondIndex + 2] - first[3 * firstIndex + 2];
            return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// resamples a polyline at equal intervals of its own arc length
        /// </summary>
        private static bool ResampleByArcLength(double[] points, int count, double[] output, int samples)
        {
            if (count < 2 || samples < 2)
            {
                return false;
            }
            double[] cumulative = new double[count];
            for (int i = 1; i < count; i++)
            {
                cumulative[i] = cumulative[i - 1] + Distance(points, i - 1, points, i);
            }
            double total = cumulative[count - 1];
            if (!(total > 0))
            {
                return false;
            }
            int segment = 0;
            for (int s = 0; s < samples; s++)
            {
                double target = total * s / (samples - 1);
                while (segment < count - 2 && cumulative[segment + 1] < target)
                {
                    segment++;
                }
                double span = cumulative[segment + 1] - cumulative[segment];
                double t = span > 0 ? (target - cumulative[segment]) / span : 0;
                for (int c = 0; c < 3; c++)
                {
                    output[3 * s + c] = points[3 * segment + c]
                                        + t * (points[3 * (segment + 1) + c] - points[3 * segment + c]);
                }
            }
            return true;
        }

        private static double GetSpacing(double[] median, int firstStation, int lastStation)
        {
            int count = lastStation - firstStation;
            if (count < 1)
            {
                return 0;
            }
            double length = 0;
            for (int k = firstStation + 1; k <= lastStation; k++)
            {
                length += Distance(median, k - 1, median, k);
            }
            return length / count;
        }

        private static void ComputeTangents(double[] median, int count, double[] tangent)
        {
            for (int k = 0; k < count; k++)
            {
                int before = System.Math.Max(0, k - 1);
                int after = System.Math.Min(count - 1, k + 1);
                double tx = median[3 * after] - median[3 * before];
                double ty = median[3 * after + 1] - median[3 * before + 1];
                double tz = median[3 * after + 2] - median[3 * before + 2];
                double norm = System.Math.Sqrt(tx * tx + ty * ty + tz * tz);
                if (!(norm > 0))
                {
                    tangent[3 * k] = 0;
                    tangent[3 * k + 1] = 0;
                    tangent[3 * k + 2] = 1;
                    continue;
                }
                tangent[3 * k] = tx / norm;
                tangent[3 * k + 1] = ty / norm;
                tangent[3 * k + 2] = tz / norm;
            }
        }

        /// <summary>
        /// Carries a frame along the median curve by the double reflection method, so that it turns only
        /// as much as the curve obliges it to. Unlike the Frenet frame it stays defined where the curve is
        /// straight and it does not flip through an inflection, which matters because these curves have
        /// long straight stretches.
        /// </summary>
        private static void ComputeRotationMinimisingFrame(double[] median, double[] tangent, int count,
                                                           double[] firstNormal, double[] secondNormal)
        {
            // any unit vector perpendicular to the first tangent will do, and the smallest component of
            // the tangent gives the most stable one
            double tx = tangent[0], ty = tangent[1], tz = tangent[2];
            double ax = System.Math.Abs(tx), ay = System.Math.Abs(ty), az = System.Math.Abs(tz);
            double ux, uy, uz;
            if (ax <= ay && ax <= az) { ux = 0; uy = -tz; uz = ty; }
            else if (ay <= az) { ux = -tz; uy = 0; uz = tx; }
            else { ux = -ty; uy = tx; uz = 0; }
            Normalise(ref ux, ref uy, ref uz);
            firstNormal[0] = ux; firstNormal[1] = uy; firstNormal[2] = uz;

            for (int k = 1; k < count; k++)
            {
                double px = median[3 * k] - median[3 * (k - 1)];
                double py = median[3 * k + 1] - median[3 * (k - 1) + 1];
                double pz = median[3 * k + 2] - median[3 * (k - 1) + 2];
                double c1 = px * px + py * py + pz * pz;
                double lu = ux, lv = uy, lw = uz;
                double ltx = tangent[3 * (k - 1)], lty = tangent[3 * (k - 1) + 1], ltz = tangent[3 * (k - 1) + 2];
                if (c1 > 0)
                {
                    double factor = 2.0 * (px * ux + py * uy + pz * uz) / c1;
                    lu = ux - factor * px; lv = uy - factor * py; lw = uz - factor * pz;
                    factor = 2.0 * (px * ltx + py * lty + pz * ltz) / c1;
                    ltx -= factor * px; lty -= factor * py; ltz -= factor * pz;
                }
                double qx = tangent[3 * k] - ltx;
                double qy = tangent[3 * k + 1] - lty;
                double qz = tangent[3 * k + 2] - ltz;
                double c2 = qx * qx + qy * qy + qz * qz;
                if (c2 > 0)
                {
                    double factor = 2.0 * (qx * lu + qy * lv + qz * lw) / c2;
                    lu -= factor * qx; lv -= factor * qy; lw -= factor * qz;
                }
                // re-orthogonalise against the tangent, so that rounding does not drift
                double along = lu * tangent[3 * k] + lv * tangent[3 * k + 1] + lw * tangent[3 * k + 2];
                lu -= along * tangent[3 * k];
                lv -= along * tangent[3 * k + 1];
                lw -= along * tangent[3 * k + 2];
                Normalise(ref lu, ref lv, ref lw);
                ux = lu; uy = lv; uz = lw;
                firstNormal[3 * k] = ux; firstNormal[3 * k + 1] = uy; firstNormal[3 * k + 2] = uz;
            }

            for (int k = 0; k < count; k++)
            {
                secondNormal[3 * k] = tangent[3 * k + 1] * firstNormal[3 * k + 2]
                                      - tangent[3 * k + 2] * firstNormal[3 * k + 1];
                secondNormal[3 * k + 1] = tangent[3 * k + 2] * firstNormal[3 * k]
                                          - tangent[3 * k] * firstNormal[3 * k + 2];
                secondNormal[3 * k + 2] = tangent[3 * k] * firstNormal[3 * k + 1]
                                          - tangent[3 * k + 1] * firstNormal[3 * k];
            }
        }

        private static void Normalise(ref double x, ref double y, ref double z)
        {
            double norm = System.Math.Sqrt(x * x + y * y + z * z);
            if (norm > 0)
            {
                x /= norm; y /= norm; z /= norm;
            }
            else
            {
                x = 1; y = 0; z = 0;
            }
        }

        // =========================================================================================
        // cutting the streamlines by the cross-sections
        // =========================================================================================

        private void CollectCrossings(IStreamlineSource source, IReadOnlyList<int> members,
                                      double[] median, double[] tangent,
                                      double[] firstNormal, double[] secondNormal, int stationCount,
                                      float[] crossU, float[] crossV,
                                      int[] population, double[] direction)
        {
            Array.Fill(crossU, float.NaN);
            Array.Fill(crossV, float.NaN);
            Array.Clear(population, 0, stationCount);
            Array.Clear(direction, 0, 3 * stationCount);

            double[] buffer = Array.Empty<double>();
            double[] best = new double[stationCount];
            for (int m = 0; m < members.Count; m++)
            {
                int count = ReadMember(source, members[m], ref buffer);
                if (count < 2)
                {
                    continue;
                }
                Array.Fill(best, double.MaxValue);
                int guess = 0;
                int previousGuess = 0;
                for (int j = 1; j < count; j++)
                {
                    if (j == 1)
                    {
                        previousGuess = NearestStation(median, stationCount, buffer, 0, 0);
                    }
                    guess = NearestStation(median, stationCount, buffer, j, previousGuess);
                    int from = System.Math.Max(0, System.Math.Min(previousGuess, guess) - 1);
                    int to = System.Math.Min(stationCount - 1, System.Math.Max(previousGuess, guess) + 1);
                    double dx = buffer[3 * j] - buffer[3 * (j - 1)];
                    double dy = buffer[3 * j + 1] - buffer[3 * (j - 1) + 1];
                    double dz = buffer[3 * j + 2] - buffer[3 * (j - 1) + 2];
                    double segment = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    for (int k = from; k <= to; k++)
                    {
                        double f0 = (buffer[3 * (j - 1)] - median[3 * k]) * tangent[3 * k]
                                    + (buffer[3 * (j - 1) + 1] - median[3 * k + 1]) * tangent[3 * k + 1]
                                    + (buffer[3 * (j - 1) + 2] - median[3 * k + 2]) * tangent[3 * k + 2];
                        double f1 = (buffer[3 * j] - median[3 * k]) * tangent[3 * k]
                                    + (buffer[3 * j + 1] - median[3 * k + 1]) * tangent[3 * k + 1]
                                    + (buffer[3 * j + 2] - median[3 * k + 2]) * tangent[3 * k + 2];
                        if ((f0 > 0 && f1 > 0) || (f0 < 0 && f1 < 0))
                        {
                            // a segment ending exactly on the plane still crosses it, which matters at the
                            // very first and very last station
                            continue;
                        }
                        double span = f0 - f1;
                        double t = span != 0 ? f0 / span : 0;
                        if (t < 0) { t = 0; } else if (t > 1) { t = 1; }
                        double ox = buffer[3 * (j - 1)] + t * dx - median[3 * k];
                        double oy = buffer[3 * (j - 1) + 1] + t * dy - median[3 * k + 1];
                        double oz = buffer[3 * (j - 1) + 2] + t * dz - median[3 * k + 2];
                        double u = ox * firstNormal[3 * k] + oy * firstNormal[3 * k + 1] + oz * firstNormal[3 * k + 2];
                        double v = ox * secondNormal[3 * k] + oy * secondNormal[3 * k + 1] + oz * secondNormal[3 * k + 2];
                        double offset = u * u + v * v;
                        // a streamline that wanders back through a plane is taken where it is nearest the
                        // median curve
                        if (offset >= best[k])
                        {
                            continue;
                        }
                        if (best[k] == double.MaxValue)
                        {
                            population[k]++;
                        }
                        best[k] = offset;
                        int at = m * stationCount + k;
                        crossU[at] = (float)u;
                        crossV[at] = (float)v;
                        if (segment > 0)
                        {
                            // the direction the streamline is going here, turned to agree with the median
                            // so that streamlines traversed the other way round do not cancel out
                            double along = dx * tangent[3 * k] + dy * tangent[3 * k + 1] + dz * tangent[3 * k + 2];
                            double sign = along < 0 ? -1.0 / segment : 1.0 / segment;
                            direction[3 * k] += sign * dx;
                            direction[3 * k + 1] += sign * dy;
                            direction[3 * k + 2] += sign * dz;
                        }
                    }
                    previousGuess = guess;
                }
            }
        }

        private static int NearestStation(double[] median, int stationCount, double[] points, int index, int from)
        {
            int best = System.Math.Max(0, System.Math.Min(stationCount - 1, from));
            double bestDistance = SquaredDistance(median, best, points, index);
            int k = best;
            while (k + 1 < stationCount)
            {
                double candidate = SquaredDistance(median, k + 1, points, index);
                if (candidate >= bestDistance)
                {
                    break;
                }
                k++;
                bestDistance = candidate;
                best = k;
            }
            k = best;
            while (k > 0)
            {
                double candidate = SquaredDistance(median, k - 1, points, index);
                if (candidate >= bestDistance)
                {
                    break;
                }
                k--;
                bestDistance = candidate;
                best = k;
            }
            return best;
        }

        private static double SquaredDistance(double[] first, int firstIndex, double[] second, int secondIndex)
        {
            double dx = second[3 * secondIndex] - first[3 * firstIndex];
            double dy = second[3 * secondIndex + 1] - first[3 * firstIndex + 1];
            double dz = second[3 * secondIndex + 2] - first[3 * firstIndex + 2];
            return dx * dx + dy * dy + dz * dz;
        }

        private static double GetBundleRadius(float[] crossU, float[] crossV, int memberCount, int stationCount)
        {
            double sum = 0;
            long count = 0;
            for (int m = 0; m < memberCount; m++)
            {
                int at = m * stationCount;
                for (int k = 0; k < stationCount; k++)
                {
                    float u = crossU[at + k];
                    if (float.IsNaN(u))
                    {
                        continue;
                    }
                    float v = crossV[at + k];
                    sum += (double)u * u + (double)v * v;
                    count++;
                }
            }
            return count > 0 ? System.Math.Sqrt(sum / count) : 0;
        }

        /// <summary>
        /// the point minimising the sum of the distances to the crossings, by Weiszfeld's iteration. The
        /// median rather than the mean, so that one stray streamline does not pull the curve off centre.
        /// </summary>
        private static void GeometricMedian(float[] crossU, float[] crossV, int memberCount, int stationCount,
                                            int station, double scale, out double u, out double v)
        {
            double sumU = 0, sumV = 0;
            int count = 0;
            for (int m = 0; m < memberCount; m++)
            {
                float cu = crossU[m * stationCount + station];
                if (float.IsNaN(cu))
                {
                    continue;
                }
                sumU += cu;
                sumV += crossV[m * stationCount + station];
                count++;
            }
            if (count == 0)
            {
                u = 0; v = 0;
                return;
            }
            u = sumU / count;
            v = sumV / count;
            double floor = 1e-9 * (scale > 0 ? scale : 1.0);
            for (int iteration = 0; iteration < WeiszfeldIterationCount; iteration++)
            {
                double weightSum = 0;
                double nextU = 0, nextV = 0;
                for (int m = 0; m < memberCount; m++)
                {
                    float cu = crossU[m * stationCount + station];
                    if (float.IsNaN(cu))
                    {
                        continue;
                    }
                    float cv = crossV[m * stationCount + station];
                    double du = cu - u;
                    double dv = cv - v;
                    double distance = System.Math.Sqrt(du * du + dv * dv);
                    double weight = 1.0 / (distance > floor ? distance : floor);
                    weightSum += weight;
                    nextU += weight * cu;
                    nextV += weight * cv;
                }
                if (!(weightSum > 0))
                {
                    return;
                }
                nextU /= weightSum;
                nextV /= weightSum;
                double move = System.Math.Sqrt((nextU - u) * (nextU - u) + (nextV - v) * (nextV - v));
                u = nextU;
                v = nextV;
                if (move <= floor)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Smooths the curve as much as it will take without moving further than the budget away from
        /// where it started. Returns how far it did move.
        /// </summary>
        private static double SmoothWithinBudget(double[] curve, int count, double budget)
        {
            if (count < 3 || !(budget > 0))
            {
                return 0;
            }
            double[] original = (double[])curve.Clone();
            double[] candidate = (double[])curve.Clone();
            double[] work = new double[curve.Length];
            double[] accepted = (double[])curve.Clone();
            double acceptedDisplacement = 0;
            for (int pass = 0; pass < MaximumSmoothingPassCount; pass++)
            {
                // one binomial pass, the ends held where they are
                Array.Copy(candidate, work, curve.Length);
                for (int k = 1; k < count - 1; k++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        candidate[3 * k + c] = 0.25 * work[3 * (k - 1) + c]
                                               + 0.5 * work[3 * k + c]
                                               + 0.25 * work[3 * (k + 1) + c];
                    }
                }
                double displacement = RootMeanSquareDistance(candidate, original, count);
                if (displacement > budget)
                {
                    break;
                }
                Array.Copy(candidate, accepted, curve.Length);
                acceptedDisplacement = displacement;
            }
            Array.Copy(accepted, curve, curve.Length);
            return acceptedDisplacement;
        }

        private static double RootMeanSquareDistance(double[] first, double[] second, int count)
        {
            double sum = 0;
            for (int k = 0; k < count; k++)
            {
                sum += SquaredDistance(first, k, second, k);
            }
            return System.Math.Sqrt(sum / count);
        }

        /// <summary>
        /// gives every cross-section no streamline reached the offset of the nearest one that some did
        /// </summary>
        private static void CarryOffsetsIntoEmptyStations(int[] population, int count,
                                                          double[] offsetU, double[] offsetV)
        {
            int previous = -1;
            for (int k = 0; k < count; k++)
            {
                if (population[k] > 0)
                {
                    previous = k;
                }
                else if (previous >= 0)
                {
                    offsetU[k] = offsetU[previous];
                    offsetV[k] = offsetV[previous];
                }
            }
            int next = -1;
            for (int k = count - 1; k >= 0; k--)
            {
                if (population[k] > 0)
                {
                    next = k;
                }
                else if (next >= 0 && (previous < 0 || k < FirstPopulated(population, count)))
                {
                    offsetU[k] = offsetU[next];
                    offsetV[k] = offsetV[next];
                }
            }
        }

        private static int FirstPopulated(int[] population, int count)
        {
            for (int k = 0; k < count; k++)
            {
                if (population[k] > 0)
                {
                    return k;
                }
            }
            return count;
        }

        /// <summary>
        /// How populated a cross-section has to be to be trusted.
        /// <para>
        /// The plateau, meaning the largest population any cross-section reaches, is the number of
        /// streamlines that are present wherever the bundle is whole. A cross-section short of it is one
        /// whose plane has begun to leave the bundle through its end, and the crossings it does collect
        /// are all on one side, which drags the median curve sideways and tilts the plane further still.
        /// Those are trimmed. The setting acts as a floor underneath: if even the plateau falls short of
        /// it, the streamlines overlap too little to be described as one bundle at all.
        /// </para>
        /// </summary>
        private int GetRequiredPopulation(int[] population, int stationCount, int memberCount)
        {
            int plateau = 0;
            for (int k = 0; k < stationCount; k++)
            {
                if (population[k] > plateau)
                {
                    plateau = population[k];
                }
            }
            int floor = (int)System.Math.Ceiling(Options.MinimumStationPopulation * memberCount);
            if (floor < 3)
            {
                floor = 3;
            }
            return System.Math.Max(floor, plateau);
        }

        private static bool FindPopulatedRun(int[] population, int count, int required,
                                             out int first, out int last)
        {
            first = 0;
            last = -1;
            int runStart = -1;
            int bestLength = 0;
            for (int k = 0; k <= count; k++)
            {
                bool populated = k < count && population[k] >= required;
                if (populated)
                {
                    if (runStart < 0)
                    {
                        runStart = k;
                    }
                }
                else if (runStart >= 0)
                {
                    int length = k - runStart;
                    if (length > bestLength)
                    {
                        bestLength = length;
                        first = runStart;
                        last = k - 1;
                    }
                    runStart = -1;
                }
            }
            return bestLength >= 4;
        }

        // =========================================================================================
        // the outlines, the twists and the density
        // =========================================================================================

        /// <summary>
        /// Fits the cross-section outlines, the twist of each, and the one density the factory draws
        /// from.
        /// <para>
        /// The two halves depend on each other: an outline is built from where the members cross, and a
        /// member's normalized position is where it sits inside the outlines. So the fit alternates, as
        /// the affine version did. The station half is no longer a least squares problem, though — an
        /// outline is read straight off the crossings, because its job is to contain them rather than to
        /// pass through their middle.
        /// </para>
        /// </summary>
        private void FitOutlinesAndDensity(StreamlineBundleFactory factory,
                                           float[] crossU, float[] crossV,
                                           int memberCount, int stationCount,
                                           int firstStation, int lastStation, double bundleRadius)
        {
            int kept = lastStation - firstStation + 1;
            CrossSectionPolygon?[] outline = new CrossSectionPolygon?[kept];
            double[] twist = new double[kept];
            double[] memberAngle = new double[memberCount];
            double[] memberRadius = new double[memberCount];

            int reference = GetReferenceStation(crossU, crossV, memberCount, stationCount,
                                                firstStation, lastStation);
            for (int m = 0; m < memberCount; m++)
            {
                float u = crossU[m * stationCount + reference];
                if (float.IsNaN(u))
                {
                    memberAngle[m] = 0;
                    memberRadius[m] = 0;
                    continue;
                }
                double v = crossV[m * stationCount + reference];
                memberAngle[m] = System.Math.Atan2(v, u);
                memberRadius[m] = 1.0;
            }

            int sectorCount = GetSectorCount(memberCount);
            for (int iteration = 0; iteration < System.Math.Max(1, Options.TransformIterationCount);
                 iteration++)
            {
                for (int k = firstStation; k <= lastStation; k++)
                {
                    int slot = k - firstStation;
                    twist[slot] = GetTwist(crossU, crossV, memberAngle, memberCount, stationCount, k);
                    outline[slot] = BuildOutline(crossU, crossV, memberCount, stationCount, k,
                                                 twist[slot], sectorCount);
                }
                FitMembers(crossU, crossV, outline, twist, memberAngle, memberRadius,
                           memberCount, stationCount, firstStation, lastStation);
            }

            factory.Outlines = outline;
            factory.Twists = twist;
            // one, by construction: the outline is what a radius of one means, so the disc the density
            // lives on is the unit disc whatever the bundle measures in metres
            factory.NormalizedRadius = 1.0;

            MeasureResiduals(factory, crossU, crossV, outline, twist, memberAngle, memberRadius,
                             memberCount, stationCount, firstStation, lastStation, bundleRadius);
            BuildDensity(factory, crossU, crossV, outline, twist, memberCount, stationCount,
                         firstStation, lastStation);
        }

        /// <summary>
        /// The cross-section the normalized angles are measured from: the most populated one that has
        /// some extent.
        /// <para>
        /// Population alone is not enough. A cross-section where every member passes through the same
        /// point is as populated as any other and carries no shape at all, and the first position after a
        /// shared head is exactly such a cross-section.
        /// </para>
        /// </summary>
        private static int GetReferenceStation(float[] crossU, float[] crossV, int memberCount,
                                               int stationCount, int firstStation, int lastStation)
        {
            int reference = firstStation;
            int bestPopulation = -1;
            double bestSpread = -1;
            for (int k = firstStation; k <= lastStation; k++)
            {
                int population = 0;
                double meanU = 0, meanV = 0;
                for (int m = 0; m < memberCount; m++)
                {
                    float u = crossU[m * stationCount + k];
                    if (float.IsNaN(u)) { continue; }
                    meanU += u;
                    meanV += crossV[m * stationCount + k];
                    population++;
                }
                if (population < 2) { continue; }
                meanU /= population;
                meanV /= population;
                double spread = 0;
                for (int m = 0; m < memberCount; m++)
                {
                    float u = crossU[m * stationCount + k];
                    if (float.IsNaN(u)) { continue; }
                    double du = u - meanU;
                    double dv = crossV[m * stationCount + k] - meanV;
                    spread += du * du + dv * dv;
                }
                spread = System.Math.Sqrt(spread / population);
                if (!(spread > 0)) { continue; }
                if (population > bestPopulation
                    || (population == bestPopulation && spread > bestSpread))
                {
                    bestPopulation = population;
                    bestSpread = spread;
                    reference = k;
                }
            }
            return reference;
        }

        /// <summary>
        /// How many angular sectors an outline is divided into, from how many members there are to
        /// describe it. Chosen the way the density chooses its grid: the resolution follows the data
        /// rather than being asked for.
        /// </summary>
        private int GetSectorCount(int memberCount)
        {
            int sectors = (int)System.Math.Round(memberCount / Options.SectorPopulation);
            if (sectors < Options.MinimumSectorCount) { sectors = Options.MinimumSectorCount; }
            if (sectors > Options.MaximumSectorCount) { sectors = Options.MaximumSectorCount; }
            return sectors;
        }

        /// <summary>
        /// How far the bundle has turned about the median at this cross-section, as the circular mean of
        /// the turn of each member away from its own normalized angle.
        /// </summary>
        private static double GetTwist(float[] crossU, float[] crossV, double[] memberAngle,
                                       int memberCount, int stationCount, int station)
        {
            double sine = 0, cosine = 0;
            for (int m = 0; m < memberCount; m++)
            {
                float u = crossU[m * stationCount + station];
                if (float.IsNaN(u)) { continue; }
                double v = crossV[m * stationCount + station];
                if (!(u * u + v * v > 0)) { continue; }
                double turned = System.Math.Atan2(v, u) - memberAngle[m];
                sine += System.Math.Sin(turned);
                cosine += System.Math.Cos(turned);
            }
            return sine == 0 && cosine == 0 ? 0 : System.Math.Atan2(sine, cosine);
        }

        /// <summary>
        /// The outline of one cross-section when the factory is built on its own: the convex hull of the
        /// crossings, held as its support distance in each direction.
        /// <para>
        /// The support distance in a direction is the furthest any crossing reaches <em>along</em> that
        /// direction, which is not the same as the furthest crossing lying <em>in</em> a sector around it.
        /// The difference is the whole of the correctness here: the region bounded by the support
        /// distances is convex and contains every crossing, both by the definition of a support function,
        /// whereas a boundary drawn through the furthest crossing per sector is neither.
        /// </para>
        /// <para>
        /// This is still only an outline of where the streamlines went. It knows nothing of what a well
        /// may not enter or of what the neighbouring corridors have claimed, and
        /// <see cref="StreamlineFactorySetBuilder"/> replaces it with a tolerance region that does.
        /// </para>
        /// </summary>
        private CrossSectionPolygon? BuildOutline(float[] crossU, float[] crossV, int memberCount,
                                                  int stationCount, int station, double twist,
                                                  int directionCount)
        {
            CrossSectionPolygon polygon = new CrossSectionPolygon(directionCount);
            double[] support = new double[directionCount];
            for (int s = 0; s < directionCount; s++)
            {
                support[s] = double.NegativeInfinity;
            }
            int present = 0;
            for (int m = 0; m < memberCount; m++)
            {
                float u = crossU[m * stationCount + station];
                if (float.IsNaN(u)) { continue; }
                double v = crossV[m * stationCount + station];
                present++;
                // the crossing in the untwisted frame, which is the frame the outline is held in
                double reach = System.Math.Sqrt((double)u * u + v * v);
                double direction = System.Math.Atan2(v, u) - twist;
                double au = reach * System.Math.Cos(direction);
                double av = reach * System.Math.Sin(direction);
                for (int s = 0; s < directionCount; s++)
                {
                    double angle = polygon.GetDirection(s);
                    double along = au * System.Math.Cos(angle) + av * System.Math.Sin(angle);
                    if (along > support[s]) { support[s] = along; }
                }
            }
            if (present == 0)
            {
                return null;
            }

            double margin = 1.0 + System.Math.Max(0, Options.OutlineMargin);
            for (int s = 0; s < directionCount; s++)
            {
                double at = support[s];
                if (double.IsNegativeInfinity(at) || at < 0) { at = 0; }
                polygon.SetSupportDistance(s, at * margin);
            }
            polygon.Measure();
            return polygon;
        }

        /// <summary>
        /// Where each member sits in the normalized disc: one angle and one radius, held constant along
        /// its whole path, which is what makes a draw a coherent streamline rather than a series of
        /// unrelated points.
        /// </summary>
        private static void FitMembers(float[] crossU, float[] crossV, CrossSectionPolygon?[] outline,
                                       double[] twist, double[] memberAngle, double[] memberRadius,
                                       int memberCount, int stationCount,
                                       int firstStation, int lastStation)
        {
            List<double> fractions = new List<double>();
            for (int m = 0; m < memberCount; m++)
            {
                double sine = 0, cosine = 0;
                fractions.Clear();
                for (int k = firstStation; k <= lastStation; k++)
                {
                    float u = crossU[m * stationCount + k];
                    if (float.IsNaN(u)) { continue; }
                    CrossSectionPolygon? polygon = outline[k - firstStation];
                    if (polygon == null) { continue; }
                    double v = crossV[m * stationCount + k];
                    double radius = System.Math.Sqrt((double)u * u + v * v);
                    double untwisted = System.Math.Atan2(v, u) - twist[k - firstStation];
                    sine += System.Math.Sin(untwisted);
                    cosine += System.Math.Cos(untwisted);
                    double boundary = polygon.GetBoundaryRadius(untwisted);
                    if (boundary > 0) { fractions.Add(radius / boundary); }
                }
                if (sine != 0 || cosine != 0)
                {
                    memberAngle[m] = System.Math.Atan2(sine, cosine);
                }
                if (fractions.Count > 0)
                {
                    // the median rather than the mean: a member that wanders once should not have its
                    // whole normalized radius pulled by that one cross-section
                    fractions.Sort();
                    memberRadius[m] = fractions[fractions.Count / 2];
                }
            }
        }

        /// <summary>
        /// what the surrogate leaves out: how far each member sits from where the outlines and the
        /// twists put it
        /// </summary>
        private static void MeasureResiduals(StreamlineBundleFactory factory,
                                             float[] crossU, float[] crossV,
                                             CrossSectionPolygon?[] outline, double[] twist,
                                             double[] memberAngle, double[] memberRadius,
                                             int memberCount, int stationCount,
                                             int firstStation, int lastStation, double bundleRadius)
        {
            double sum = 0;
            long count = 0;
            double lagged = 0;
            double lagBase = 0;
            int kept = lastStation - firstStation + 1;
            double[] stationSum = new double[kept];
            int[] stationCounts = new int[kept];
            for (int m = 0; m < memberCount; m++)
            {
                double previousU = 0, previousV = 0;
                bool hasPrevious = false;
                for (int k = firstStation; k <= lastStation; k++)
                {
                    float u = crossU[m * stationCount + k];
                    if (float.IsNaN(u))
                    {
                        hasPrevious = false;
                        continue;
                    }
                    int slot = k - firstStation;
                    CrossSectionPolygon? polygon = outline[slot];
                    if (polygon == null)
                    {
                        hasPrevious = false;
                        continue;
                    }
                    // the outline is held untwisted, so it is asked at the member's own angle and the
                    // twist is applied only when the point is placed
                    double predicted = memberRadius[m] * polygon.GetBoundaryRadius(memberAngle[m]);
                    double turned = memberAngle[m] + twist[slot];
                    double predictedU = predicted * System.Math.Cos(turned);
                    double predictedV = predicted * System.Math.Sin(turned);
                    double eu = u - predictedU;
                    double ev = crossV[m * stationCount + k] - predictedV;
                    double squared = eu * eu + ev * ev;
                    sum += squared;
                    count++;
                    stationSum[slot] += squared;
                    stationCounts[slot]++;
                    if (hasPrevious)
                    {
                        lagged += eu * previousU + ev * previousV;
                        lagBase += squared;
                    }
                    previousU = eu;
                    previousV = ev;
                    hasPrevious = true;
                }
            }
            factory.ResidualDeviation = count > 0 ? System.Math.Sqrt(sum / count) : 0;
            factory.ResidualFraction = bundleRadius > 0 ? factory.ResidualDeviation / bundleRadius : 0;
            double correlation = lagBase > 0 ? lagged / lagBase : 0;
            if (correlation > 0 && correlation < 1)
            {
                factory.ResidualCorrelationLength = -factory.StationSpacing / System.Math.Log(correlation);
            }
            else
            {
                factory.ResidualCorrelationLength = correlation >= 1 ? double.PositiveInfinity : 0;
            }
            factory.StationResidual = new double[kept];
            factory.StationRadius = new double[kept];
            for (int slot = 0; slot < kept; slot++)
            {
                factory.StationResidual[slot] = stationCounts[slot] > 0
                    ? System.Math.Sqrt(stationSum[slot] / stationCounts[slot]) : 0;
            }
            for (int k = firstStation; k <= lastStation; k++)
            {
                double radiusSum = 0;
                int radiusCount = 0;
                for (int m = 0; m < memberCount; m++)
                {
                    float u = crossU[m * stationCount + k];
                    if (float.IsNaN(u))
                    {
                        continue;
                    }
                    float v = crossV[m * stationCount + k];
                    radiusSum += (double)u * u + (double)v * v;
                    radiusCount++;
                }
                factory.StationRadius[k - firstStation] = radiusCount > 0
                    ? System.Math.Sqrt(radiusSum / radiusCount) : 0;
            }
        }

        /// <summary>
        /// the one density the factory draws from, over the unit disc
        /// </summary>
        private void BuildDensity(StreamlineBundleFactory factory,
                                  float[] crossU, float[] crossV,
                                  CrossSectionPolygon?[] outline, double[] twist,
                                  int memberCount, int stationCount,
                                  int firstStation, int lastStation)
        {
            int cells = (int)System.Math.Round(memberCount / Options.DensityCellPopulation);
            if (cells < 1) { cells = 1; }
            if (cells > Options.MaximumDensityCellCount) { cells = Options.MaximumDensityCellCount; }
            int rings = (int)System.Math.Round(System.Math.Sqrt(cells / 3.0));
            if (rings < 1) { rings = 1; }
            PolarDensity density = new PolarDensity(rings, 1.0);

            // Every cross-section contributes where each streamline actually was, not where the outline
            // says it should have been. The scatter that leaves is the residual, and it smooths the
            // histogram by exactly the amount the model is uncertain by. The number of cells was fixed
            // from the number of streamlines, not from the number of samples, because the cross-sections
            // of one streamline are near copies of one another and not independent.
            for (int k = firstStation; k <= lastStation; k++)
            {
                int slot = k - firstStation;
                CrossSectionPolygon? polygon = outline[slot];
                if (polygon == null)
                {
                    continue;
                }
                for (int m = 0; m < memberCount; m++)
                {
                    float u = crossU[m * stationCount + k];
                    if (float.IsNaN(u))
                    {
                        continue;
                    }
                    double v = crossV[m * stationCount + k];
                    double radius = System.Math.Sqrt((double)u * u + v * v);
                    double untwisted = System.Math.Atan2(v, u) - twist[slot];
                    double boundary = polygon.GetBoundaryRadius(untwisted);
                    if (!(boundary > 0))
                    {
                        continue;
                    }
                    density.Add(System.Math.Min(1.0, radius / boundary), untwisted);
                }
            }
            density.Prepare();
            factory.Density = density;
        }

        private static void PopulateStations(StreamlineBundleFactory factory,
                                             double[] median, double[] tangent,
                                             double[] firstNormal, double[] secondNormal,
                                             int[] population, double[] direction,
                                             int firstStation, int lastStation)
        {
            double abscissa = 0;
            double biasSum = 0;
            double spreadSum = 0;
            int deviationCount = 0;
            double maximumTubeRatio = 0;
            double spacing = factory.StationSpacing;
            for (int k = firstStation; k <= lastStation; k++)
            {
                int slot = k - firstStation;
                if (k > firstStation)
                {
                    abscissa += Distance(median, k - 1, median, k);
                }
                CrossSectionStation station = new CrossSectionStation
                {
                    Abscissa = abscissa,
                    Position = new Point3D(median[3 * k], median[3 * k + 1], median[3 * k + 2]),
                    Tangent = new Vector3D(tangent[3 * k], tangent[3 * k + 1], tangent[3 * k + 2]),
                    FirstNormal = new Vector3D(firstNormal[3 * k], firstNormal[3 * k + 1], firstNormal[3 * k + 2]),
                    SecondNormal = new Vector3D(secondNormal[3 * k], secondNormal[3 * k + 1], secondNormal[3 * k + 2]),
                    StreamlineCount = population[k],
                    ResidualDeviation = factory.StationResidual != null ? factory.StationResidual[slot] : 0,
                    Radius = factory.StationRadius != null ? factory.StationRadius[slot] : 0
                };
                station.Polygon = factory.Outlines != null ? factory.Outlines[slot] : null;
                station.Twist = factory.Twists != null ? factory.Twists[slot] : 0;

                // the discrete curvature of the median curve, from the turn between consecutive stations
                if (k > firstStation && k < lastStation && spacing > 0)
                {
                    double cx = median[3 * (k + 1)] - 2 * median[3 * k] + median[3 * (k - 1)];
                    double cy = median[3 * (k + 1) + 1] - 2 * median[3 * k + 1] + median[3 * (k - 1) + 1];
                    double cz = median[3 * (k + 1) + 2] - 2 * median[3 * k + 2] + median[3 * (k - 1) + 2];
                    station.Curvature = System.Math.Sqrt(cx * cx + cy * cy + cz * cz) / (spacing * spacing);
                }
                // the outer reach of the outline is what the cross-section planes have to clear,
                // so it is what the curvature is judged against
                double reach = station.Polygon != null ? station.Polygon.OuterRadius : 0;
                double ratio = reach * station.Curvature;
                if (ratio > maximumTubeRatio)
                {
                    maximumTubeRatio = ratio;
                }
                if (population[k] > 0)
                {
                    // the resultant of the member directions: its direction is where the bundle is really
                    // going, and its length says how much the members disagree among themselves
                    double sx = direction[3 * k], sy = direction[3 * k + 1], sz = direction[3 * k + 2];
                    double resultant = System.Math.Sqrt(sx * sx + sy * sy + sz * sz);
                    if (resultant > 0)
                    {
                        double alignment = (sx * tangent[3 * k] + sy * tangent[3 * k + 1] + sz * tangent[3 * k + 2])
                                           / resultant;
                        if (alignment > 1) { alignment = 1; } else if (alignment < -1) { alignment = -1; }
                        biasSum += System.Math.Acos(alignment);
                        double concentration = resultant / population[k];
                        if (concentration > 1) { concentration = 1; }
                        spreadSum += System.Math.Sqrt(2.0 * (1.0 - concentration));
                        deviationCount++;
                    }
                }
                factory.Stations.Add(station);
            }
            factory.Length = abscissa;
            factory.TangentDeviation = deviationCount > 0 ? biasSum / deviationCount : 0;
            factory.BundleTangentSpread = deviationCount > 0 ? spreadSum / deviationCount : 0;
            factory.MaximumTubeRatio = maximumTubeRatio;
        }

        // =========================================================================================
        // two by two linear algebra
        // =========================================================================================

        private static void Invert(double a00, double a01, double a10, double a11,
                                   out double i00, out double i01, out double i10, out double i11)
        {
            double determinant = a00 * a11 - a01 * a10;
            if (determinant == 0)
            {
                i00 = 1; i01 = 0; i10 = 0; i11 = 1;
                return;
            }
            i00 = a11 / determinant;
            i01 = -a01 / determinant;
            i10 = -a10 / determinant;
            i11 = a00 / determinant;
        }

        /// <summary>
        /// the inverse square root of a symmetric positive definite two by two matrix
        /// </summary>
        private static void InverseSquareRoot(double a, double b, double c,
                                              out double w00, out double w01, out double w11)
        {
            double trace = a + c;
            double determinant = a * c - b * b;
            double discriminant = System.Math.Sqrt(System.Math.Max(0, trace * trace / 4.0 - determinant));
            double first = trace / 2.0 + discriminant;
            double second = trace / 2.0 - discriminant;
            double floor = 1e-300 + 1e-12 * System.Math.Abs(first);
            if (first < floor) { first = floor; }
            if (second < floor) { second = floor; }
            double ex, ey;
            if (b != 0)
            {
                ex = b;
                ey = first - a;
                double norm = System.Math.Sqrt(ex * ex + ey * ey);
                if (norm > 0) { ex /= norm; ey /= norm; } else { ex = 1; ey = 0; }
            }
            else if (a >= c)
            {
                ex = 1; ey = 0;
            }
            else
            {
                ex = 0; ey = 1;
            }
            double fx = -ey, fy = ex;
            double firstScale = 1.0 / System.Math.Sqrt(first);
            double secondScale = 1.0 / System.Math.Sqrt(second);
            w00 = firstScale * ex * ex + secondScale * fx * fx;
            w01 = firstScale * ex * ey + secondScale * fx * fy;
            w11 = firstScale * ey * ey + secondScale * fy * fy;
        }

    }
}
