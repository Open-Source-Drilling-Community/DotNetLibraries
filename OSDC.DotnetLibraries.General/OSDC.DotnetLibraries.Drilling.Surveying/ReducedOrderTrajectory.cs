using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    /// <summary>
    /// A dense directional survey rewritten as a small number of analytic sections which stay within a
    /// stated distance of it.
    ///
    /// A survey run carries a station every twenty or thirty metres, which is far more detail than a plan
    /// or an anti collision scan needs, and none of it says what the hole was actually drilled as. This
    /// replaces the run by a handful of straight holds, circular arcs, constant curvature and toolface
    /// curves and constant build and turn curves, joined so that the direction never jumps at a junction,
    /// and reports how far the replacement strays from the original.
    ///
    /// Everything here is pure SI: angles in radians, lengths in metres, curvatures and angular rates in
    /// radians per metre. The frame is north, east and vertical with the vertical axis positive downward,
    /// which is the frame the rest of this library works in. Azimuth is measured clockwise from north and
    /// inclination from the downward vertical.
    ///
    /// This file follows the specification in docs/ReducedOrderTrajectory/SPEC.md; the section numbers
    /// quoted against each method are its section numbers, so that the mathematics stays traceable.
    /// </summary>
    public class ReducedOrderTrajectory
    {
        /// <summary>
        /// The inclination the two azimuth carrying families are clamped into, in radians. Both of them
        /// divide by the sine of the inclination, directly or through a logarithm, so the inclination has
        /// to be held strictly inside the open interval from zero to pi. Specification section 3.3.
        /// </summary>
        public const double InclinationFloor = 1.0e-7;

        /// <summary>
        /// The upper end of the same clamp, pi less <see cref="InclinationFloor"/>.
        /// </summary>
        public static readonly double InclinationCeiling = System.Math.PI - 1.0e-7;

        /// <summary>
        /// How close to vertical a station has to be before the two azimuth carrying families are refused
        /// the interval containing it, expressed as the sine of the inclination. The default is the sine
        /// of three degrees. A circular arc and a hold are well behaved at the vertical and cover that
        /// part of the hole. Specification section 3.5.
        /// </summary>
        public static readonly double DefaultMinimumSineInclination = System.Math.Sin(3.0 * System.Math.PI / 180.0);

        /// <summary>
        /// The tolerances a reduction to a stated deviation works through, rad, from the finest to the
        /// coarsest: a third, a half, four fifths, one and a third, two and three degrees. Specification
        /// section 1.
        ///
        /// The values are the ones the method was characterised with, so they are written out rather than
        /// worked out from the degrees they came from.
        /// </summary>
        public static readonly double[] DefaultToleranceLadder =
            { 0.005236, 0.008727, 0.013963, 0.022689, 0.034907, 0.052360 };

        // =====================================================================================
        // What a reduction is. Specification section 1.
        // =====================================================================================

        private readonly List<ReducedOrderSection> sections_ = new List<ReducedOrderSection>();
        private double[] grid_ = System.Array.Empty<double>();
        private double[] gridTangents_ = System.Array.Empty<double>();
        private double[] gridPositions_ = System.Array.Empty<double>();
        private int[] sectionLow_ = System.Array.Empty<int>();

        /// <summary>
        /// The analytic sections the survey was replaced by, in order along the hole.
        /// </summary>
        public IReadOnlyList<ReducedOrderSection> Sections => sections_;

        /// <summary>
        /// How many sections the survey was reduced to.
        /// </summary>
        public int SectionCount => sections_.Count;

        /// <summary>
        /// How many stations the survey carried, after stations repeating a measured depth were dropped.
        /// </summary>
        public int StationCount { get; private set; }

        /// <summary>
        /// The inclination the trajectory sets off at, rad. The refinement moves this, so it is not in
        /// general the inclination the first station reported.
        /// </summary>
        public double InitialInclination { get; private set; }

        /// <summary>
        /// The azimuth the trajectory sets off at, rad, and likewise refined.
        /// </summary>
        public double InitialAzimuth { get; private set; }

        /// <summary>
        /// Where the first station is. Positions reported by <see cref="PositionAt"/> are measured from
        /// here. It is the origin unless the caller said otherwise.
        /// </summary>
        public Point3D Origin { get; private set; } = new Point3D(0.0, 0.0, 0.0);

        /// <summary>
        /// The measured depth of the first station, m.
        /// </summary>
        public double StartMeasuredDepth { get; private set; }

        /// <summary>
        /// The measured depth of the last station, m.
        /// </summary>
        public double EndMeasuredDepth { get; private set; }

        /// <summary>
        /// The angular tolerance the reduction was worked out at, rad. In a reduction to a stated
        /// deviation this is the rung of the ladder that was settled on.
        /// </summary>
        public double AngularTolerance { get; private set; }

        /// <summary>
        /// The largest distance between a station of the original survey and the reduced trajectory at
        /// the same measured depth, m. This is the primary measure of how good a reduction is.
        /// Specification section 9.
        /// </summary>
        public double MaxDeviation { get; private set; }

        /// <summary>
        /// The root mean square of the same distance over all stations, m.
        /// </summary>
        public double RmsDeviation { get; private set; }

        /// <summary>
        /// The largest difference in inclination between a station and the reduced trajectory, rad.
        /// </summary>
        public double MaxInclinationDeviation { get; private set; }

        /// <summary>
        /// The largest difference in azimuth between a station and the reduced trajectory, rad, weighted
        /// by the sine of the inclination. Specification section 9.
        ///
        /// The weighting turns an angle about the vertical into an angle in the hole, which is the one
        /// that means something. Without it a near vertical station, where a hand's turn of the bit swings
        /// the azimuth right round, reports a large number describing nothing.
        /// </summary>
        public double MaxAzimuthDeviationInHole { get; private set; }

        /// <summary>
        /// Why the reduction could not be worked out, or <see cref="ReducedOrderFailureReason.None"/>.
        /// </summary>
        public ReducedOrderFailureReason FailureReason { get; private set; } = ReducedOrderFailureReason.None;

        /// <summary>
        /// Whether the reduction worked out.
        /// </summary>
        public bool IsValid => FailureReason == ReducedOrderFailureReason.None;

        /// <summary>
        /// What went wrong, in words.
        /// </summary>
        public string FailureDescription
        {
            get
            {
                switch (FailureReason)
                {
                    case ReducedOrderFailureReason.None:
                        return string.Empty;
                    case ReducedOrderFailureReason.NoStations:
                        return "The survey carries no stations.";
                    case ReducedOrderFailureReason.MismatchedStationCounts:
                        return "The measured depths, inclinations and azimuths do not carry the same number of stations.";
                    case ReducedOrderFailureReason.UndefinedStation:
                        return "A station carries a measured depth, an inclination or an azimuth which is not a number.";
                    case ReducedOrderFailureReason.InvalidTolerance:
                        return "The angular tolerance asked for is not a positive angle.";
                    default:
                        return "The survey could not be reduced.";
                }
            }
        }

        /// <summary>
        /// How many sections of one family the reduction used.
        /// </summary>
        public int CountOf(ReducedOrderCurveType curveType)
        {
            int count = 0;
            for (int q = 0; q < sections_.Count; q++)
            {
                if (sections_[q].CurveType == curveType)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// A reduction is built by <see cref="Reduce(double[], double[], double[], double, ReductionOptions, IPoint3D)"/>
        /// or by <see cref="ReduceToDeviation(double[], double[], double[], double, IReadOnlyList{double}, ReductionOptions, IPoint3D)"/>.
        /// </summary>
        private ReducedOrderTrajectory()
        {
        }

        // =====================================================================================
        // Asking the reduced trajectory where it goes.
        // =====================================================================================

        /// <summary>
        /// The unit tangent of the reduced trajectory at a measured depth, or null where that depth is
        /// outside it.
        /// </summary>
        /// <param name="measuredDepth">Measured depth, m.</param>
        public Vector3D? TangentAt(double measuredDepth)
        {
            if (!TangentAt(measuredDepth, out double north, out double east, out double vertical))
            {
                return null;
            }
            return new Vector3D(north, east, vertical);
        }

        /// <summary>
        /// The inclination and azimuth of the reduced trajectory at a measured depth.
        /// </summary>
        /// <param name="measuredDepth">Measured depth, m.</param>
        /// <param name="inclination">Inclination from the downward vertical, rad.</param>
        /// <param name="azimuth">Azimuth clockwise from north, rad, in the interval from zero to two pi.</param>
        /// <returns>False where the measured depth is outside the trajectory.</returns>
        public bool AttitudeAt(double measuredDepth, out double inclination, out double azimuth)
        {
            inclination = Numeric.UNDEF_DOUBLE;
            azimuth = Numeric.UNDEF_DOUBLE;
            if (!TangentAt(measuredDepth, out double north, out double east, out double vertical))
            {
                return false;
            }
            inclination = System.Math.Acos(Clamp(vertical, -1.0, 1.0));
            azimuth = System.Math.Atan2(east, north);
            if (azimuth < 0.0)
            {
                azimuth += 2.0 * System.Math.PI;
            }
            return true;
        }

        /// <summary>
        /// Where the reduced trajectory is at a measured depth, or null where that depth is outside it.
        ///
        /// Between two points of the grid the trajectory was measured on, the position is carried forward
        /// from the nearer one below by the minimum curvature method, which is how the whole grid was
        /// integrated, so this agrees with <see cref="MaxDeviation"/> rather than sitting beside it.
        /// </summary>
        /// <param name="measuredDepth">Measured depth, m.</param>
        public Point3D? PositionAt(double measuredDepth)
        {
            if (!IsValid || grid_.Length == 0)
            {
                return null;
            }
            if (measuredDepth < grid_[0] - 1.0e-9 || measuredDepth > grid_[grid_.Length - 1] + 1.0e-9)
            {
                return null;
            }
            double clamped = Clamp(measuredDepth, grid_[0], grid_[grid_.Length - 1]);
            int below = LowerBound(grid_, clamped);
            if (below >= grid_.Length || grid_[below] > clamped)
            {
                below--;
            }
            if (below < 0)
            {
                below = 0;
            }
            double north = gridPositions_[3 * below];
            double east = gridPositions_[3 * below + 1];
            double vertical = gridPositions_[3 * below + 2];
            double step = clamped - grid_[below];
            if (step > 0.0 && TangentAt(clamped, out double endNorth, out double endEast, out double endVertical))
            {
                MinimumCurvatureStep(step,
                    gridTangents_[3 * below], gridTangents_[3 * below + 1], gridTangents_[3 * below + 2],
                    endNorth, endEast, endVertical,
                    ref north, ref east, ref vertical);
            }
            return new Point3D(
                (Origin.X ?? 0.0) + north,
                (Origin.Y ?? 0.0) + east,
                (Origin.Z ?? 0.0) + vertical);
        }

        /// <summary>
        /// The unit tangent at a measured depth, as three components.
        /// </summary>
        private bool TangentAt(double measuredDepth, out double north, out double east, out double vertical)
        {
            north = Numeric.UNDEF_DOUBLE;
            east = Numeric.UNDEF_DOUBLE;
            vertical = Numeric.UNDEF_DOUBLE;
            if (!IsValid || sections_.Count == 0)
            {
                return false;
            }
            if (measuredDepth < StartMeasuredDepth - 1.0e-9 || measuredDepth > EndMeasuredDepth + 1.0e-9)
            {
                return false;
            }
            double clamped = Clamp(measuredDepth, StartMeasuredDepth, EndMeasuredDepth);

            int section = sections_.Count - 1;
            for (int q = 0; q < sections_.Count; q++)
            {
                if (clamped <= sections_[q].EndMeasuredDepth)
                {
                    section = q;
                    break;
                }
            }
            ReducedOrderSection one = sections_[section];
            int entry = 3 * sectionLow_[section];
            double[] distance = new double[1] { clamped - one.StartMeasuredDepth };
            double[] tangent = new double[3];
            SectionTangents(one.CurveType, one.FirstParameter, one.SecondParameter,
                gridTangents_[entry], gridTangents_[entry + 1], gridTangents_[entry + 2],
                distance, 0, 1, tangent, 0);
            double length = System.Math.Sqrt(
                tangent[0] * tangent[0] + tangent[1] * tangent[1] + tangent[2] * tangent[2]);
            if (length < 1.0e-12)
            {
                return false;
            }
            north = tangent[0] / length;
            east = tangent[1] / length;
            vertical = tangent[2] / length;
            return true;
        }

        /// <summary>
        /// One minimum curvature step between two attitudes a course length apart.
        /// </summary>
        private static void MinimumCurvatureStep(
            double courseLength,
            double startNorth, double startEast, double startVertical,
            double endNorth, double endEast, double endVertical,
            ref double north, ref double east, ref double vertical)
        {
            double dotProduct = Clamp(
                startNorth * endNorth + startEast * endEast + startVertical * endVertical, -1.0, 1.0);
            double dogleg = System.Math.Acos(dotProduct);
            double ratioFactor = dogleg > 1.0e-8 ? 2.0 / dogleg * System.Math.Tan(0.5 * dogleg) : 1.0;
            double half = 0.5 * courseLength * ratioFactor;
            north += half * (startNorth + endNorth);
            east += half * (startEast + endEast);
            vertical += half * (startVertical + endVertical);
        }

        // =====================================================================================
        // Reducing a survey. Specification sections 2 and 10.
        // =====================================================================================

        /// <summary>
        /// Replaces a dense directional survey by a small number of analytic sections which stay within a
        /// stated angular tolerance of it.
        ///
        /// The tolerance is on the direction, not on the position: it bounds how far the direction of a
        /// section may stray from the direction the survey reports at any station it covers. The distance
        /// that comes out of it is reported as <see cref="MaxDeviation"/>. Three degrees, the coarsest
        /// rung of <see cref="DefaultToleranceLadder"/>, is the sensible default for planning work and
        /// gives of the order of a dozen sections and a few metres on a normal well; a third of a degree
        /// gives of the order of eighty sections and a decimetre.
        ///
        /// Stations repeating a measured depth are dropped, and the azimuth is made continuous along the
        /// hole before anything else happens, so a survey crossing north needs no preparation by the
        /// caller. Specification section 2.
        /// </summary>
        /// <param name="measuredDepths">Measured depth at each station, m, increasing.</param>
        /// <param name="inclinations">Inclination from the downward vertical at each station, rad.</param>
        /// <param name="azimuths">Azimuth clockwise from north at each station, rad.</param>
        /// <param name="angularTolerance">The largest angular deviation a section may leave, rad.</param>
        /// <param name="options">The settings to run under, or null for the defaults.</param>
        /// <param name="origin">Where the first station is, or null for the origin.</param>
        public static ReducedOrderTrajectory Reduce(
            double[] measuredDepths, double[] inclinations, double[] azimuths,
            double angularTolerance, ReductionOptions? options = null, IPoint3D? origin = null)
        {
            ReducedOrderFailureReason reason = Validate(measuredDepths, inclinations, azimuths, angularTolerance);
            if (reason != ReducedOrderFailureReason.None)
            {
                return new ReducedOrderTrajectory() { FailureReason = reason };
            }
            return Build(measuredDepths, inclinations, azimuths, angularTolerance,
                options ?? new ReductionOptions(), origin);
        }

        /// <summary>
        /// The same, from a list of survey points. The position of the first point, where it has one, is
        /// taken as the origin, so that positions come back in the caller's own frame.
        /// </summary>
        /// <param name="survey">The stations, in order along the hole.</param>
        /// <param name="angularTolerance">The largest angular deviation a section may leave, rad.</param>
        /// <param name="options">The settings to run under, or null for the defaults.</param>
        public static ReducedOrderTrajectory Reduce(
            List<SurveyPoint> survey, double angularTolerance, ReductionOptions? options = null)
        {
            if (survey == null || survey.Count == 0)
            {
                return new ReducedOrderTrajectory() { FailureReason = ReducedOrderFailureReason.NoStations };
            }
            int count = survey.Count;
            double[] measuredDepths = new double[count];
            double[] inclinations = new double[count];
            double[] azimuths = new double[count];
            for (int i = 0; i < count; i++)
            {
                SurveyPoint station = survey[i];
                if (station?.MD == null || station.Inclination == null || station.Azimuth == null)
                {
                    return new ReducedOrderTrajectory() { FailureReason = ReducedOrderFailureReason.UndefinedStation };
                }
                measuredDepths[i] = station.MD.Value;
                inclinations[i] = station.Inclination.Value;
                azimuths[i] = station.Azimuth.Value;
            }
            SurveyPoint first = survey[0];
            IPoint3D? origin = first.X != null && first.Y != null && first.Z != null ? first : null;
            return Reduce(measuredDepths, inclinations, azimuths, angularTolerance, options, origin);
        }

        /// <summary>
        /// Reduces a survey as far as it can be reduced while staying within a stated distance of it.
        /// Specification section 10.
        ///
        /// The ladder is worked through from its coarsest rung towards its finest, and the first
        /// reduction meeting the distance asked for is the answer, so the answer is the fewest sections
        /// which will do. Where no rung meets it, the closest one is returned; the caller should check
        /// <see cref="MaxDeviation"/> rather than assume.
        ///
        /// Choosing per well rather than picking one tolerance for all of them matters. The refinement
        /// occasionally settles into a poor local minimum, and while that is rare it is not rare enough to
        /// ignore; it has always been seen at one rung with its neighbours unaffected, so trying several
        /// absorbs it.
        /// </summary>
        /// <param name="measuredDepths">Measured depth at each station, m, increasing.</param>
        /// <param name="inclinations">Inclination from the downward vertical at each station, rad.</param>
        /// <param name="azimuths">Azimuth clockwise from north at each station, rad.</param>
        /// <param name="maximumDeviation">The largest distance the reduction may stray, m.</param>
        /// <param name="ladder">The tolerances to try, rad, or null for <see cref="DefaultToleranceLadder"/>.</param>
        /// <param name="options">The settings to run under, or null for the defaults.</param>
        /// <param name="origin">Where the first station is, or null for the origin.</param>
        public static ReducedOrderTrajectory ReduceToDeviation(
            double[] measuredDepths, double[] inclinations, double[] azimuths,
            double maximumDeviation, IReadOnlyList<double>? ladder = null,
            ReductionOptions? options = null, IPoint3D? origin = null)
        {
            IReadOnlyList<double> rungs = ladder ?? DefaultToleranceLadder;
            if (rungs.Count == 0)
            {
                return new ReducedOrderTrajectory() { FailureReason = ReducedOrderFailureReason.InvalidTolerance };
            }

            // Coarsest first, so that the first one which will do is also the one with the fewest sections.
            List<double> ordered = new List<double>(rungs);
            ordered.Sort();
            ReducedOrderTrajectory? closest = null;
            for (int r = ordered.Count - 1; r >= 0; r--)
            {
                ReducedOrderTrajectory candidate = Reduce(measuredDepths, inclinations, azimuths,
                    ordered[r], options, origin);
                if (!candidate.IsValid)
                {
                    return candidate;
                }
                if (candidate.MaxDeviation <= maximumDeviation)
                {
                    return candidate;
                }
                if (closest == null || candidate.MaxDeviation < closest.MaxDeviation)
                {
                    closest = candidate;
                }
            }
            return closest!;
        }

        /// <summary>
        /// The reasons a survey cannot be reduced, checked before anything is computed.
        /// </summary>
        private static ReducedOrderFailureReason Validate(
            double[] measuredDepths, double[] inclinations, double[] azimuths, double angularTolerance)
        {
            if (measuredDepths == null || inclinations == null || azimuths == null)
            {
                return ReducedOrderFailureReason.NoStations;
            }
            if (measuredDepths.Length == 0)
            {
                return ReducedOrderFailureReason.NoStations;
            }
            if (inclinations.Length != measuredDepths.Length || azimuths.Length != measuredDepths.Length)
            {
                return ReducedOrderFailureReason.MismatchedStationCounts;
            }
            if (!Numeric.IsDefined(angularTolerance) || angularTolerance <= 0.0)
            {
                return ReducedOrderFailureReason.InvalidTolerance;
            }
            for (int i = 0; i < measuredDepths.Length; i++)
            {
                if (!Numeric.IsDefined(measuredDepths[i])
                    || !Numeric.IsDefined(inclinations[i])
                    || !Numeric.IsDefined(azimuths[i]))
                {
                    return ReducedOrderFailureReason.UndefinedStation;
                }
            }
            return ReducedOrderFailureReason.None;
        }

        /// <summary>
        /// The whole method, from a checked survey to a reduction. Specification sections 2, 6 to 9.
        /// </summary>
        private static ReducedOrderTrajectory Build(
            double[] rawMeasuredDepths, double[] rawInclinations, double[] rawAzimuths,
            double angularTolerance, ReductionOptions options, IPoint3D? origin)
        {
            ReducedOrderTrajectory result = new ReducedOrderTrajectory()
            {
                AngularTolerance = angularTolerance
            };
            if (origin?.X != null && origin.Y != null && origin.Z != null)
            {
                result.Origin = new Point3D(origin.X.Value, origin.Y.Value, origin.Z.Value);
            }

            // Specification section 2, steps 1 and 2. Stations repeating a measured depth are dropped, and
            // the azimuth is made continuous, before anything else uses either.
            int rawCount = rawMeasuredDepths.Length;
            List<int> kept = new List<int>(rawCount) { 0 };
            for (int i = 1; i < rawCount; i++)
            {
                if (rawMeasuredDepths[i] - rawMeasuredDepths[kept[kept.Count - 1]] > 1.0e-9)
                {
                    kept.Add(i);
                }
            }
            int stationCount = kept.Count;
            double[] measuredDepths = new double[stationCount];
            double[] inclinations = new double[stationCount];
            double[] azimuths = new double[stationCount];
            for (int i = 0; i < stationCount; i++)
            {
                measuredDepths[i] = rawMeasuredDepths[kept[i]];
                inclinations[i] = rawInclinations[kept[i]];
                azimuths[i] = rawAzimuths[kept[i]];
            }
            for (int i = 1; i < stationCount; i++)
            {
                while (azimuths[i] - azimuths[i - 1] > System.Math.PI)
                {
                    azimuths[i] -= 2.0 * System.Math.PI;
                }
                while (azimuths[i] - azimuths[i - 1] < -System.Math.PI)
                {
                    azimuths[i] += 2.0 * System.Math.PI;
                }
            }

            result.StationCount = stationCount;
            result.StartMeasuredDepth = measuredDepths[0];
            result.EndMeasuredDepth = measuredDepths[stationCount - 1];
            result.InitialInclination = inclinations[0];
            result.InitialAzimuth = azimuths[0];

            if (stationCount < 2)
            {
                // Nothing to reduce and nothing wrong. Specification section 2, step 5.
                return result;
            }

            double[] tangents = new double[3 * stationCount];
            TangentsFromAngles(inclinations, azimuths, tangents);
            double[] observedPositions = new double[3 * stationCount];
            MinimumCurvature(measuredDepths, tangents, observedPositions);

            Segment(measuredDepths, tangents, inclinations, azimuths,
                angularTolerance, options.MinimumSineInclination, options.ReproduceReferenceSegmentation,
                out int[] breaks, out ReducedOrderCurveType[] types);

            double initialInclination = inclinations[0];
            double initialAzimuth = azimuths[0];
            double[] parameters = ChainedInitialisation(measuredDepths, tangents, breaks, types,
                initialInclination, initialAzimuth);
            Refine(measuredDepths, tangents, observedPositions, breaks, types,
                ref initialInclination, ref initialAzimuth, parameters, options);
            result.InitialInclination = initialInclination;
            result.InitialAzimuth = initialAzimuth;

            // Specification section 9: the profile is measured on a grid carrying every station, every
            // section boundary and an even subdivision of each section.
            BuildMeasurementGrid(measuredDepths, breaks, options.MeasurementStep,
                out double[] grid, out int[] sectionLow, out int[] sectionHigh, out int[] stationIndex);
            double[] gridTangents = new double[3 * grid.Length];
            double[] gridDistances = new double[grid.Length];
            ForwardProfile(initialInclination, initialAzimuth, types, parameters,
                sectionLow, sectionHigh, grid, gridDistances, gridTangents);
            double[] gridPositions = new double[3 * grid.Length];
            MinimumCurvature(grid, gridTangents, gridPositions);

            for (int q = 0; q < types.Length; q++)
            {
                result.sections_.Add(new ReducedOrderSection(types[q],
                    measuredDepths[breaks[q]], measuredDepths[breaks[q + 1]],
                    parameters[2 * q], parameters[2 * q + 1]));
            }
            result.grid_ = grid;
            result.gridTangents_ = gridTangents;
            result.gridPositions_ = gridPositions;
            result.sectionLow_ = sectionLow;

            double worst = 0.0;
            double sumSquared = 0.0;
            double worstInclination = 0.0;
            double worstAzimuthInHole = 0.0;
            for (int i = 0; i < stationCount; i++)
            {
                int at = 3 * stationIndex[i];
                int observed = 3 * i;
                double north = gridPositions[at] - observedPositions[observed];
                double east = gridPositions[at + 1] - observedPositions[observed + 1];
                double vertical = gridPositions[at + 2] - observedPositions[observed + 2];
                double squared = north * north + east * east + vertical * vertical;
                sumSquared += squared;
                double distance = System.Math.Sqrt(squared);
                if (distance > worst)
                {
                    worst = distance;
                }

                double reducedInclination = System.Math.Acos(Clamp(gridTangents[at + 2], -1.0, 1.0));
                double inclinationError = System.Math.Abs(reducedInclination - inclinations[i]);
                if (inclinationError > worstInclination)
                {
                    worstInclination = inclinationError;
                }
                double reducedAzimuth = System.Math.Atan2(gridTangents[at + 1], gridTangents[at]);
                double azimuthInHole = System.Math.Abs(
                    CurvilinearPoint3D.WrapToPi(reducedAzimuth - azimuths[i])) * System.Math.Sin(inclinations[i]);
                if (azimuthInHole > worstAzimuthInHole)
                {
                    worstAzimuthInHole = azimuthInHole;
                }
            }
            result.MaxDeviation = worst;
            result.RmsDeviation = System.Math.Sqrt(sumSquared / stationCount);
            result.MaxInclinationDeviation = worstInclination;
            result.MaxAzimuthDeviationInHole = worstAzimuthInHole;
            return result;
        }

        // =====================================================================================
        // Geometric primitives. Specification sections 0, 2.1, 3 and 7.1.
        //
        // These work on flat arrays of three doubles per point, north then east then vertical, rather
        // than on the library's Vector3D. The refinement evaluates the whole profile once per Jacobian
        // column and would otherwise allocate a vector object per station per column, which dominates
        // everything else the method does. Where a single direction is passed or returned it is passed
        // as three doubles for the same reason. The convenience overloads carrying Vector3D and Point3D
        // are for callers, and delegate to the same code the hot paths use.
        // =====================================================================================

        /// <summary>
        /// The unit tangent of a well path at a station of the given attitude. Specification section 0:
        /// t = ( sin(inclination) cos(azimuth), sin(inclination) sin(azimuth), cos(inclination) ).
        /// </summary>
        /// <param name="inclination">Inclination from the downward vertical, rad.</param>
        /// <param name="azimuth">Azimuth clockwise from north, rad.</param>
        /// <param name="north">The north component of the unit tangent.</param>
        /// <param name="east">The east component.</param>
        /// <param name="vertical">The vertical component, positive downward.</param>
        public static void TangentFromAngles(double inclination, double azimuth, out double north, out double east, out double vertical)
        {
            double sine = System.Math.Sin(inclination);
            north = sine * System.Math.Cos(azimuth);
            east = sine * System.Math.Sin(azimuth);
            vertical = System.Math.Cos(inclination);
        }

        /// <summary>
        /// The unit tangent at a station of the given attitude, as a vector. Convenience over
        /// <see cref="TangentFromAngles(double, double, out double, out double, out double)"/>.
        /// </summary>
        public static Vector3D TangentFromAngles(double inclination, double azimuth)
        {
            TangentFromAngles(inclination, azimuth, out double north, out double east, out double vertical);
            return new Vector3D(north, east, vertical);
        }

        /// <summary>
        /// The unit tangents of a whole survey, written into <paramref name="tangents"/> as three doubles
        /// per station.
        /// </summary>
        /// <param name="inclinations">Inclination at each station, rad.</param>
        /// <param name="azimuths">Azimuth at each station, rad, unwrapped.</param>
        /// <param name="tangents">Receives three doubles per station: north, east, vertical.</param>
        public static void TangentsFromAngles(double[] inclinations, double[] azimuths, double[] tangents)
        {
            int count = inclinations.Length;
            for (int i = 0; i < count; i++)
            {
                TangentFromAngles(inclinations[i], azimuths[i], out double north, out double east, out double vertical);
                tangents[3 * i] = north;
                tangents[3 * i + 1] = east;
                tangents[3 * i + 2] = vertical;
            }
        }

        /// <summary>
        /// Positions along a path by the minimum curvature method, starting from the origin.
        /// Specification section 2.1.
        ///
        /// The ratio factor is written as two over the dogleg angle times the tangent of its half, with a
        /// plain cut off to one below a dogleg of 1e-8 rad rather than the series expansion used
        /// elsewhere in this library. The reference implementation and the golden vectors were built that
        /// way and the last digits depend on it; the two forms agree to well inside a nanometre over a
        /// survey interval, so nothing is lost by keeping them separate.
        /// </summary>
        /// <param name="measuredDepths">Measured depth at each point, m, strictly increasing.</param>
        /// <param name="tangents">Three doubles per point: north, east, vertical, of unit length.</param>
        /// <param name="positions">Receives three doubles per point. The first is left at the origin.</param>
        public static void MinimumCurvature(double[] measuredDepths, double[] tangents, double[] positions)
        {
            int count = measuredDepths.Length;
            if (count == 0)
            {
                return;
            }
            positions[0] = 0.0;
            positions[1] = 0.0;
            positions[2] = 0.0;
            for (int i = 0; i < count - 1; i++)
            {
                int a = 3 * i;
                int b = a + 3;
                double courseLength = measuredDepths[i + 1] - measuredDepths[i];
                double dotProduct = tangents[a] * tangents[b]
                                  + tangents[a + 1] * tangents[b + 1]
                                  + tangents[a + 2] * tangents[b + 2];
                if (dotProduct > 1.0)
                {
                    dotProduct = 1.0;
                }
                else if (dotProduct < -1.0)
                {
                    dotProduct = -1.0;
                }
                double dogleg = System.Math.Acos(dotProduct);
                double ratioFactor = dogleg > 1.0e-8
                    ? 2.0 / dogleg * System.Math.Tan(0.5 * dogleg)
                    : 1.0;
                double half = 0.5 * courseLength * ratioFactor;
                positions[b] = positions[a] + half * (tangents[a] + tangents[b]);
                positions[b + 1] = positions[a + 1] + half * (tangents[a + 1] + tangents[b + 1]);
                positions[b + 2] = positions[a + 2] + half * (tangents[a + 2] + tangents[b + 2]);
            }
        }

        /// <summary>
        /// The two directions completing a right handed frame on the plane perpendicular to a tangent.
        /// Specification section 3.0.
        ///
        /// The seed direction is the vertical unless the tangent is within about twenty five degrees of
        /// it, in which case it is north, so that the subtraction never cancels. The construction has to
        /// be reproduced exactly, because the two parameters of a circular arc are that arc's turn rate
        /// vector resolved onto this frame and therefore mean nothing without it.
        /// </summary>
        /// <param name="tangentNorth">North component of the unit tangent the frame is built on.</param>
        /// <param name="tangentEast">East component.</param>
        /// <param name="tangentVertical">Vertical component.</param>
        /// <param name="firstNorth">North component of the first perpendicular direction.</param>
        /// <param name="firstEast">East component of the first perpendicular direction.</param>
        /// <param name="firstVertical">Vertical component of the first perpendicular direction.</param>
        /// <param name="secondNorth">North component of the second perpendicular direction.</param>
        /// <param name="secondEast">East component of the second perpendicular direction.</param>
        /// <param name="secondVertical">Vertical component of the second perpendicular direction.</param>
        public static void PerpendicularBasis(
            double tangentNorth, double tangentEast, double tangentVertical,
            out double firstNorth, out double firstEast, out double firstVertical,
            out double secondNorth, out double secondEast, out double secondVertical)
        {
            double seedNorth, seedEast, seedVertical;
            if (System.Math.Abs(tangentVertical) < 0.9)
            {
                seedNorth = 0.0;
                seedEast = 0.0;
                seedVertical = 1.0;
            }
            else
            {
                seedNorth = 1.0;
                seedEast = 0.0;
                seedVertical = 0.0;
            }
            double along = seedNorth * tangentNorth + seedEast * tangentEast + seedVertical * tangentVertical;
            double acrossNorth = seedNorth - along * tangentNorth;
            double acrossEast = seedEast - along * tangentEast;
            double acrossVertical = seedVertical - along * tangentVertical;
            double length = System.Math.Sqrt(acrossNorth * acrossNorth + acrossEast * acrossEast + acrossVertical * acrossVertical);
            firstNorth = acrossNorth / length;
            firstEast = acrossEast / length;
            firstVertical = acrossVertical / length;
            secondNorth = tangentEast * firstVertical - tangentVertical * firstEast;
            secondEast = tangentVertical * firstNorth - tangentNorth * firstVertical;
            secondVertical = tangentNorth * firstEast - tangentEast * firstNorth;
        }

        /// <summary>
        /// The tangents of one section, at the given distances along it, in the frame set by the tangent
        /// the section inherits. Specification sections 3.1 to 3.4.
        ///
        /// Every family is written relative to the inherited tangent, which is what makes the direction
        /// continuous at every junction without anything having to enforce it.
        /// </summary>
        /// <param name="curveType">Which family the section is drawn with.</param>
        /// <param name="firstParameter">
        /// The first of the two parameters. For a circular arc, the component of the turn rate vector
        /// along the first perpendicular direction, rad/m. For either of the other two curves, the build
        /// up rate, rad/m. Ignored for a hold.
        /// </param>
        /// <param name="secondParameter">
        /// The second parameter. For a circular arc, the component of the turn rate vector along the
        /// second perpendicular direction, rad/m. For a constant curvature and toolface curve, the turn
        /// parameter, the curvature resolved across the high side, rad/m. For a constant build and turn
        /// curve, the turn rate, rad/m. Ignored for a hold.
        /// </param>
        /// <param name="inheritedNorth">North component of the tangent the section starts on.</param>
        /// <param name="inheritedEast">East component.</param>
        /// <param name="inheritedVertical">Vertical component.</param>
        /// <param name="alongHoleDistances">Distances from the start of the section, m.</param>
        /// <param name="firstDistance">Index of the first distance to evaluate.</param>
        /// <param name="distanceCount">How many distances to evaluate.</param>
        /// <param name="tangents">Receives three doubles per distance: north, east, vertical.</param>
        /// <param name="firstTangent">Index of the first tangent to write, counted in points.</param>
        public static void SectionTangents(
            ReducedOrderCurveType curveType, double firstParameter, double secondParameter,
            double inheritedNorth, double inheritedEast, double inheritedVertical,
            double[] alongHoleDistances, int firstDistance, int distanceCount,
            double[] tangents, int firstTangent)
        {
            if (curveType == ReducedOrderCurveType.Hold)
            {
                WriteInherited(inheritedNorth, inheritedEast, inheritedVertical, distanceCount, tangents, firstTangent);
                return;
            }

            if (curveType == ReducedOrderCurveType.CircularArc)
            {
                double curvature = System.Math.Sqrt(firstParameter * firstParameter + secondParameter * secondParameter);
                if (curvature < 1.0e-14)
                {
                    WriteInherited(inheritedNorth, inheritedEast, inheritedVertical, distanceCount, tangents, firstTangent);
                    return;
                }
                PerpendicularBasis(inheritedNorth, inheritedEast, inheritedVertical,
                    out double firstNorth, out double firstEast, out double firstVertical,
                    out double secondNorth, out double secondEast, out double secondVertical);
                // The unit binormal: the axis the tangent turns about, perpendicular to the tangent.
                double cosineShare = firstParameter / curvature;
                double sineShare = secondParameter / curvature;
                double binormalNorth = cosineShare * firstNorth + sineShare * secondNorth;
                double binormalEast = cosineShare * firstEast + sineShare * secondEast;
                double binormalVertical = cosineShare * firstVertical + sineShare * secondVertical;
                // The normal at the start of the arc, the binormal crossed onto the tangent.
                double normalNorth = binormalEast * inheritedVertical - binormalVertical * inheritedEast;
                double normalEast = binormalVertical * inheritedNorth - binormalNorth * inheritedVertical;
                double normalVertical = binormalNorth * inheritedEast - binormalEast * inheritedNorth;
                for (int i = 0; i < distanceCount; i++)
                {
                    double turned = curvature * alongHoleDistances[firstDistance + i];
                    double cosine = System.Math.Cos(turned);
                    double sine = System.Math.Sin(turned);
                    int at = 3 * (firstTangent + i);
                    tangents[at] = cosine * inheritedNorth + sine * normalNorth;
                    tangents[at + 1] = cosine * inheritedEast + sine * normalEast;
                    tangents[at + 2] = cosine * inheritedVertical + sine * normalVertical;
                }
                return;
            }

            double startInclination = System.Math.Acos(
                inheritedVertical > 1.0 ? 1.0 : (inheritedVertical < -1.0 ? -1.0 : inheritedVertical));
            double startAzimuth = System.Math.Atan2(inheritedEast, inheritedNorth);

            if (curveType == ReducedOrderCurveType.ConstantBuildAndTurn)
            {
                // Both angles run at their own steady rate. No clamping: this family carries no logarithm
                // and nothing divides by the sine of the inclination.
                for (int i = 0; i < distanceCount; i++)
                {
                    double distance = alongHoleDistances[firstDistance + i];
                    double inclination = startInclination + firstParameter * distance;
                    double azimuth = startAzimuth + secondParameter * distance;
                    double sine = System.Math.Sin(inclination);
                    int at = 3 * (firstTangent + i);
                    tangents[at] = sine * System.Math.Cos(azimuth);
                    tangents[at + 1] = sine * System.Math.Sin(azimuth);
                    tangents[at + 2] = System.Math.Cos(inclination);
                }
                return;
            }

            // Constant curvature and toolface. The inclination runs at the build up rate and the azimuth
            // follows from integrating the turn parameter over one over the sine of the inclination, whose
            // exact integral is the logarithm of the tangent of the half inclination. Below a build up the
            // ratio of the two is taken at its limit instead, where the logarithm difference cancels.
            double clampedStart = Clamp(startInclination, InclinationFloor, InclinationCeiling);
            double startLogTangent = System.Math.Log(System.Math.Tan(0.5 * clampedStart));
            double startSine = System.Math.Sin(clampedStart);
            for (int i = 0; i < distanceCount; i++)
            {
                double distance = alongHoleDistances[firstDistance + i];
                double rawInclination = startInclination + firstParameter * distance;
                double inclination = Clamp(rawInclination, InclinationFloor, InclinationCeiling);
                double azimuthChange;
                if (System.Math.Abs(firstParameter * distance) > 1.0e-7 && System.Math.Abs(firstParameter) > 1.0e-12)
                {
                    azimuthChange = secondParameter / firstParameter
                        * (System.Math.Log(System.Math.Tan(0.5 * inclination)) - startLogTangent);
                }
                else
                {
                    azimuthChange = secondParameter * distance / System.Math.Max(startSine, 1.0e-9);
                }
                double azimuth = startAzimuth + azimuthChange;
                double sine = System.Math.Sin(inclination);
                int at = 3 * (firstTangent + i);
                tangents[at] = sine * System.Math.Cos(azimuth);
                tangents[at + 1] = sine * System.Math.Sin(azimuth);
                tangents[at + 2] = System.Math.Cos(inclination);
            }
        }

        /// <summary>
        /// The rotation carrying the direction <paramref name="fromNorth"/>, <paramref name="fromEast"/>,
        /// <paramref name="fromVertical"/> onto the direction <paramref name="ontoNorth"/>,
        /// <paramref name="ontoEast"/>, <paramref name="ontoVertical"/>, by Rodrigues' formula.
        /// Specification section 7.1.
        ///
        /// Two parallel directions need no rotation and the identity is returned. Two antiparallel ones
        /// have no distinguished axis to turn about, and the identity is returned there as well rather
        /// than an arbitrary choice; the chained initialisation which uses this never meets that case,
        /// since it aligns a station tangent with an inherited tangent a section length away from it.
        /// </summary>
        /// <param name="fromNorth">North component of the direction to rotate.</param>
        /// <param name="fromEast">East component of the direction to rotate.</param>
        /// <param name="fromVertical">Vertical component of the direction to rotate.</param>
        /// <param name="ontoNorth">North component of the direction to rotate onto.</param>
        /// <param name="ontoEast">East component of the direction to rotate onto.</param>
        /// <param name="ontoVertical">Vertical component of the direction to rotate onto.</param>
        /// <param name="rotation">Receives nine doubles, the rotation matrix read row by row.</param>
        public static void RotationAligning(
            double fromNorth, double fromEast, double fromVertical,
            double ontoNorth, double ontoEast, double ontoVertical,
            double[] rotation)
        {
            double axisNorth = fromEast * ontoVertical - fromVertical * ontoEast;
            double axisEast = fromVertical * ontoNorth - fromNorth * ontoVertical;
            double axisVertical = fromNorth * ontoEast - fromEast * ontoNorth;
            double cosine = fromNorth * ontoNorth + fromEast * ontoEast + fromVertical * ontoVertical;
            double axisLength2 = axisNorth * axisNorth + axisEast * axisEast + axisVertical * axisVertical;

            rotation[0] = 1.0; rotation[1] = 0.0; rotation[2] = 0.0;
            rotation[3] = 0.0; rotation[4] = 1.0; rotation[5] = 0.0;
            rotation[6] = 0.0; rotation[7] = 0.0; rotation[8] = 1.0;
            if (axisLength2 < 1.0e-20)
            {
                return;
            }

            // The cross product matrix of the axis, and the rotation as the identity plus it plus its
            // square weighted by one less the cosine over the squared axis length.
            double k01 = -axisVertical, k02 = axisEast;
            double k10 = axisVertical, k12 = -axisNorth;
            double k20 = -axisEast, k21 = axisNorth;
            double weight = (1.0 - cosine) / axisLength2;

            // K squared, written out. The diagonal of K is zero, which is what leaves these nine terms.
            double s00 = k01 * k10 + k02 * k20;
            double s01 = k02 * k21;
            double s02 = k01 * k12;
            double s10 = k12 * k20;
            double s11 = k10 * k01 + k12 * k21;
            double s12 = k10 * k02;
            double s20 = k21 * k10;
            double s21 = k20 * k01;
            double s22 = k20 * k02 + k21 * k12;

            rotation[0] += weight * s00;
            rotation[1] += k01 + weight * s01;
            rotation[2] += k02 + weight * s02;
            rotation[3] += k10 + weight * s10;
            rotation[4] += weight * s11;
            rotation[5] += k12 + weight * s12;
            rotation[6] += k20 + weight * s20;
            rotation[7] += k21 + weight * s21;
            rotation[8] += weight * s22;
        }

        /// <summary>
        /// The rotation carrying one direction onto another, as a matrix. Convenience over
        /// <see cref="RotationAligning(double, double, double, double, double, double, double[])"/>.
        /// </summary>
        public static Matrix3x3 RotationAligning(Vector3D from, Vector3D onto)
        {
            double[] rotation = new double[9];
            RotationAligning(
                from.X ?? 0.0, from.Y ?? 0.0, from.Z ?? 0.0,
                onto.X ?? 0.0, onto.Y ?? 0.0, onto.Z ?? 0.0,
                rotation);
            return new Matrix3x3(
                rotation[0], rotation[1], rotation[2],
                rotation[3], rotation[4], rotation[5],
                rotation[6], rotation[7], rotation[8]);
        }

        /// <summary>
        /// Applies a rotation held as nine doubles, read row by row, to a direction.
        /// </summary>
        public static void ApplyRotation(
            double[] rotation, double north, double east, double vertical,
            out double rotatedNorth, out double rotatedEast, out double rotatedVertical)
        {
            rotatedNorth = rotation[0] * north + rotation[1] * east + rotation[2] * vertical;
            rotatedEast = rotation[3] * north + rotation[4] * east + rotation[5] * vertical;
            rotatedVertical = rotation[6] * north + rotation[7] * east + rotation[8] * vertical;
        }

        // =====================================================================================
        // Closed form section fits and the L infinity angular residual. Specification section 5.
        //
        // Each family has a parameter estimate over a station interval which needs no iteration at all.
        // The residual every fit reports is the largest angle between the curve it describes and the
        // observed tangents, which is the quantity the segmentation thresholds on.
        // =====================================================================================

        /// <summary>
        /// The closed form parameters of one section over the station interval from
        /// <paramref name="first"/> to <paramref name="last"/>, anchored on the observed tangent at
        /// <paramref name="first"/>, and the largest angle by which the resulting curve misses the
        /// observed tangents. Specification section 5.
        ///
        /// The angles are taken from <paramref name="inclinations"/> and <paramref name="azimuths"/>
        /// rather than from the tangents, because the two regression based families need the azimuth
        /// unwrapped along the hole and a tangent has forgotten which turn it is on.
        /// </summary>
        /// <param name="curveType">Which family to fit.</param>
        /// <param name="measuredDepths">Measured depth at each station, m.</param>
        /// <param name="tangents">Three doubles per station: north, east, vertical.</param>
        /// <param name="inclinations">Inclination at each station, rad.</param>
        /// <param name="azimuths">Azimuth at each station, rad, unwrapped along the hole.</param>
        /// <param name="first">Index of the station the section starts at.</param>
        /// <param name="last">Index of the station the section ends at.</param>
        /// <param name="firstParameter">The first of the two parameters, meaning as per the family.</param>
        /// <param name="secondParameter">The second parameter.</param>
        /// <returns>
        /// The largest angular deviation over the interval, rad, or 1e9 where the interval has no length
        /// or the curve degenerates.
        /// </returns>
        public static double FitSection(
            ReducedOrderCurveType curveType,
            double[] measuredDepths, double[] tangents, double[] inclinations, double[] azimuths,
            int first, int last,
            out double firstParameter, out double secondParameter)
        {
            return FitSection(curveType, measuredDepths, tangents, inclinations, azimuths, first, last,
                out firstParameter, out secondParameter, new FitWorkspace(last - first + 1));
        }

        /// <summary>
        /// The same fit, reusing scratch the caller owns. The segmentation runs this a few thousand times
        /// on a long well and would otherwise spend most of its time allocating.
        /// </summary>
        private static double FitSection(
            ReducedOrderCurveType curveType,
            double[] measuredDepths, double[] tangents, double[] inclinations, double[] azimuths,
            int first, int last,
            out double firstParameter, out double secondParameter,
            FitWorkspace workspace)
        {
            firstParameter = 0.0;
            secondParameter = 0.0;
            double sectionLength = measuredDepths[last] - measuredDepths[first];
            if (sectionLength <= 0.0)
            {
                return DegenerateResidual;
            }

            int anchor = 3 * first;
            double anchorNorth = tangents[anchor];
            double anchorEast = tangents[anchor + 1];
            double anchorVertical = tangents[anchor + 2];

            switch (curveType)
            {
                case ReducedOrderCurveType.Hold:
                    // Nothing to estimate. Specification section 5.1.
                    break;

                case ReducedOrderCurveType.CircularArc:
                    FitCircularArc(measuredDepths, tangents, first, last, sectionLength,
                        anchorNorth, anchorEast, anchorVertical,
                        out firstParameter, out secondParameter);
                    break;

                case ReducedOrderCurveType.ConstantBuildAndTurn:
                    FitConstantBuildAndTurn(measuredDepths, inclinations, azimuths, first, last,
                        out firstParameter, out secondParameter);
                    break;

                default:
                    FitConstantCurvatureAndToolface(measuredDepths, inclinations, azimuths, first, last,
                        out firstParameter, out secondParameter);
                    break;
            }

            return SectionResidual(curveType, firstParameter, secondParameter,
                measuredDepths, tangents, first, last, workspace);
        }

        /// <summary>
        /// The largest angle by which a section of the given family and parameters, anchored on the
        /// observed tangent at <paramref name="first"/>, misses the observed tangents over the interval.
        /// Specification section 5.5.
        ///
        /// This is separate from the fit because the parameters witnessing that a family fits an interval
        /// are also the parameters to judge any part of that interval by, and the two are not the same as
        /// re-fitting the part in its own right.
        /// </summary>
        /// <param name="curveType">Which family the section is drawn with.</param>
        /// <param name="firstParameter">The first of the two parameters.</param>
        /// <param name="secondParameter">The second parameter.</param>
        /// <param name="measuredDepths">Measured depth at each station, m.</param>
        /// <param name="tangents">Three doubles per station: north, east, vertical.</param>
        /// <param name="first">Index of the station the section is anchored at.</param>
        /// <param name="last">Index of the station the interval ends at.</param>
        /// <returns>The largest angular deviation, rad, or 1e9 where the curve degenerates.</returns>
        public static double SectionResidual(
            ReducedOrderCurveType curveType, double firstParameter, double secondParameter,
            double[] measuredDepths, double[] tangents, int first, int last)
        {
            return SectionResidual(curveType, firstParameter, secondParameter, measuredDepths, tangents,
                first, last, new FitWorkspace(last - first + 1));
        }

        /// <summary>
        /// The same residual, reusing scratch the caller owns.
        /// </summary>
        private static double SectionResidual(
            ReducedOrderCurveType curveType, double firstParameter, double secondParameter,
            double[] measuredDepths, double[] tangents, int first, int last,
            FitWorkspace workspace)
        {
            int pointCount = last - first + 1;
            int anchor = 3 * first;
            double[] distances = workspace.Distances;
            double[] predicted = workspace.Predicted;
            double startDepth = measuredDepths[first];
            for (int n = 0; n < pointCount; n++)
            {
                distances[n] = measuredDepths[first + n] - startDepth;
            }
            SectionTangents(curveType, firstParameter, secondParameter,
                tangents[anchor], tangents[anchor + 1], tangents[anchor + 2],
                distances, 0, pointCount, predicted, 0);

            double worst = 0.0;
            for (int n = 0; n < pointCount; n++)
            {
                int at = 3 * n;
                double length = System.Math.Sqrt(
                    predicted[at] * predicted[at]
                    + predicted[at + 1] * predicted[at + 1]
                    + predicted[at + 2] * predicted[at + 2]);
                if (length < 1.0e-12)
                {
                    return DegenerateResidual;
                }
                int observed = 3 * (first + n);
                double dotProduct = (predicted[at] * tangents[observed]
                    + predicted[at + 1] * tangents[observed + 1]
                    + predicted[at + 2] * tangents[observed + 2]) / length;
                if (dotProduct > 1.0)
                {
                    dotProduct = 1.0;
                }
                else if (dotProduct < -1.0)
                {
                    dotProduct = -1.0;
                }
                double angle = System.Math.Acos(dotProduct);
                if (angle > worst)
                {
                    worst = angle;
                }
            }
            return worst;
        }

        /// <summary>
        /// Specification section 5.2. The turn axis is the mean of the pairwise binormals weighted by the
        /// dogleg each pair turns through, and the curvature is the accumulated dogleg over the section
        /// length. The axis is then resolved onto the perpendicular frame of the anchoring tangent, which
        /// is what the two parameters of a circular arc are.
        ///
        /// Taking the axis rather than the centre of the osculating circle is the point of the whole
        /// parameterisation: the centre runs off to infinity as the curvature vanishes, and a well is
        /// mostly nearly straight.
        /// </summary>
        private static void FitCircularArc(
            double[] measuredDepths, double[] tangents, int first, int last, double sectionLength,
            double anchorNorth, double anchorEast, double anchorVertical,
            out double firstParameter, out double secondParameter)
        {
            firstParameter = 0.0;
            secondParameter = 0.0;
            double axisNorth = 0.0, axisEast = 0.0, axisVertical = 0.0;
            double totalDogleg = 0.0;
            for (int n = first; n < last; n++)
            {
                int a = 3 * n;
                int b = a + 3;
                double crossNorth = tangents[a + 1] * tangents[b + 2] - tangents[a + 2] * tangents[b + 1];
                double crossEast = tangents[a + 2] * tangents[b] - tangents[a] * tangents[b + 2];
                double crossVertical = tangents[a] * tangents[b + 1] - tangents[a + 1] * tangents[b];
                double dotProduct = tangents[a] * tangents[b]
                                  + tangents[a + 1] * tangents[b + 1]
                                  + tangents[a + 2] * tangents[b + 2];
                if (dotProduct > 1.0)
                {
                    dotProduct = 1.0;
                }
                else if (dotProduct < -1.0)
                {
                    dotProduct = -1.0;
                }
                double dogleg = System.Math.Acos(dotProduct);
                double crossLength = System.Math.Sqrt(
                    crossNorth * crossNorth + crossEast * crossEast + crossVertical * crossVertical);
                if (crossLength > 1.0e-12)
                {
                    axisNorth += crossNorth / crossLength * dogleg;
                    axisEast += crossEast / crossLength * dogleg;
                    axisVertical += crossVertical / crossLength * dogleg;
                }
                totalDogleg += dogleg;
            }
            double axisLength = System.Math.Sqrt(
                axisNorth * axisNorth + axisEast * axisEast + axisVertical * axisVertical);
            if (axisLength <= 1.0e-12)
            {
                // The pairwise turns cancel, so there is no single plane of turn. A straight section is
                // the honest answer, and the residual will say whether it is good enough.
                return;
            }
            PerpendicularBasis(anchorNorth, anchorEast, anchorVertical,
                out double e1North, out double e1East, out double e1Vertical,
                out double e2North, out double e2East, out double e2Vertical);
            double curvature = totalDogleg / sectionLength;
            double alongFirst = (axisNorth * e1North + axisEast * e1East + axisVertical * e1Vertical) / axisLength;
            double alongSecond = (axisNorth * e2North + axisEast * e2East + axisVertical * e2Vertical) / axisLength;
            double inPlane = System.Math.Sqrt(alongFirst * alongFirst + alongSecond * alongSecond);
            if (inPlane > 1.0e-12)
            {
                firstParameter = curvature * alongFirst / inPlane;
                secondParameter = curvature * alongSecond / inPlane;
            }
        }

        /// <summary>
        /// Specification section 5.3. Ordinary least squares slopes of the inclination and of the azimuth
        /// against measured depth, both taken relative to the first station of the interval so that the
        /// intercept is zero by construction and the section starts where it is anchored.
        /// </summary>
        private static void FitConstantBuildAndTurn(
            double[] measuredDepths, double[] inclinations, double[] azimuths, int first, int last,
            out double buildUpRate, out double turnRate)
        {
            buildUpRate = 0.0;
            turnRate = 0.0;
            int pointCount = last - first + 1;
            double sumDepth = 0.0, sumDepthSquared = 0.0;
            double sumInclination = 0.0, sumDepthInclination = 0.0;
            double sumAzimuth = 0.0, sumDepthAzimuth = 0.0;
            for (int n = first; n <= last; n++)
            {
                double depth = measuredDepths[n] - measuredDepths[first];
                double inclination = inclinations[n] - inclinations[first];
                double azimuth = azimuths[n] - azimuths[first];
                sumDepth += depth;
                sumDepthSquared += depth * depth;
                sumInclination += inclination;
                sumDepthInclination += depth * inclination;
                sumAzimuth += azimuth;
                sumDepthAzimuth += depth * azimuth;
            }
            double denominator = pointCount * sumDepthSquared - sumDepth * sumDepth;
            if (denominator > 1.0e-12)
            {
                buildUpRate = (pointCount * sumDepthInclination - sumDepth * sumInclination) / denominator;
                turnRate = (pointCount * sumDepthAzimuth - sumDepth * sumAzimuth) / denominator;
            }
        }

        /// <summary>
        /// Specification section 5.4. The build up rate comes from the same inclination regression as the
        /// constant build and turn fit. The turn parameter then comes from regressing the azimuth not
        /// against measured depth but against the logarithm of the tangent of the half inclination, which
        /// is the variable the azimuth is linear in on this curve, its slope being the tangent of the
        /// toolface angle.
        ///
        /// Where the inclination barely moves that variable barely moves either and its regression says
        /// nothing, so the fit falls back on the azimuth against measured depth and converts the turn rate
        /// it gets into a turn parameter by the sine of the inclination.
        /// </summary>
        private static void FitConstantCurvatureAndToolface(
            double[] measuredDepths, double[] inclinations, double[] azimuths, int first, int last,
            out double buildUpRate, out double turnParameter)
        {
            int pointCount = last - first + 1;
            double sumDepth = 0.0, sumDepthSquared = 0.0;
            double sumInclination = 0.0, sumDepthInclination = 0.0;
            for (int n = first; n <= last; n++)
            {
                double depth = measuredDepths[n] - measuredDepths[first];
                double inclination = inclinations[n] - inclinations[first];
                sumDepth += depth;
                sumDepthSquared += depth * depth;
                sumInclination += inclination;
                sumDepthInclination += depth * inclination;
            }
            double denominator = pointCount * sumDepthSquared - sumDepth * sumDepth;
            buildUpRate = denominator > 1.0e-12
                ? (pointCount * sumDepthInclination - sumDepth * sumInclination) / denominator
                : 0.0;

            double startLogTangent = System.Math.Log(System.Math.Tan(
                0.5 * Clamp(inclinations[first], InclinationFloor, InclinationCeiling)));
            double sumLog = 0.0, sumLogSquared = 0.0, sumLogAzimuth = 0.0, sumAzimuth = 0.0;
            for (int n = first; n <= last; n++)
            {
                double logTangent = System.Math.Log(System.Math.Tan(
                    0.5 * Clamp(inclinations[n], InclinationFloor, InclinationCeiling))) - startLogTangent;
                double azimuth = azimuths[n] - azimuths[first];
                sumLog += logTangent;
                sumLogSquared += logTangent * logTangent;
                sumLogAzimuth += logTangent * azimuth;
                sumAzimuth += azimuth;
            }
            double logDenominator = pointCount * sumLogSquared - sumLog * sumLog;
            if (System.Math.Abs(logDenominator) > 1.0e-14 && System.Math.Abs(buildUpRate) > 1.0e-12)
            {
                // The slope is the tangent of the toolface angle, so the turn parameter is the build up
                // rate times it.
                double toolfaceTangent = (pointCount * sumLogAzimuth - sumLog * sumAzimuth) / logDenominator;
                turnParameter = buildUpRate * toolfaceTangent;
                return;
            }

            double sumAzimuthFallback = 0.0, sumDepthAzimuthFallback = 0.0;
            for (int n = first; n <= last; n++)
            {
                double depth = measuredDepths[n] - measuredDepths[first];
                double azimuth = azimuths[n] - azimuths[first];
                sumAzimuthFallback += azimuth;
                sumDepthAzimuthFallback += depth * azimuth;
            }
            double turnRate = denominator > 1.0e-12
                ? (pointCount * sumDepthAzimuthFallback - sumDepth * sumAzimuthFallback) / denominator
                : 0.0;
            turnParameter = turnRate * System.Math.Sin(
                Clamp(inclinations[first], InclinationFloor, InclinationCeiling));
        }

        // =====================================================================================
        // Segmentation. Specification section 6.
        // =====================================================================================

        /// <summary>
        /// Cuts the survey into few sections whose closed form fits all stay within
        /// <paramref name="angularTolerance"/>. Specification section 6.
        ///
        /// A set of parameters which fits an interval also fits every part of that interval, so a longer
        /// feasible interval never costs anything, and the search takes the longest one it can find at
        /// each step. It doubles its reach until a fit fails and then bisects, which is why this costs a
        /// logarithm rather than a scan.
        ///
        /// Two things are worth knowing about how close that comes to the fewest sections there are.
        /// Feasibility is hereditary for a fixed set of parameters, but the test applied here re-fits each
        /// interval, and the closed form fits are least squares rather than minimax, so a shorter interval
        /// re-fitted can score worse than the longer one containing it. Feasibility is therefore not
        /// monotone in the interval end, and the doubling can stop short of an interval which would have
        /// held. Separately, where the doubling runs off the end of the survey the last station has to be
        /// tried explicitly, since the bisection stops as soon as its two ends are adjacent and can never
        /// reach it; the reference implementation does not, and
        /// <see cref="ReductionOptions.ReproduceReferenceSegmentation"/> reproduces that where it is
        /// needed. So this is a good search rather than a provably optimal one.
        ///
        /// The result is a function of the input alone: the families are tried in the fixed order hold,
        /// circular arc, constant curvature and toolface, constant build and turn, and a family only
        /// displaces the one before it by reaching strictly further, so ties go to the simpler curve.
        /// </summary>
        /// <param name="measuredDepths">Measured depth at each station, m, strictly increasing.</param>
        /// <param name="tangents">Three doubles per station: north, east, vertical.</param>
        /// <param name="inclinations">Inclination at each station, rad.</param>
        /// <param name="azimuths">Azimuth at each station, rad, unwrapped along the hole.</param>
        /// <param name="angularTolerance">The largest angular deviation a section may leave, rad.</param>
        /// <param name="minimumSineInclination">
        /// How close to vertical a station has to be before the two azimuth carrying families are refused
        /// any interval containing it. See <see cref="DefaultMinimumSineInclination"/>.
        /// </param>
        /// <param name="breaks">
        /// Receives the station indices the sections run between, starting at zero and ending at the last
        /// station, so there is one more of these than there are sections.
        /// </param>
        /// <param name="types">Receives the family each section is drawn with.</param>
        public static void Segment(
            double[] measuredDepths, double[] tangents, double[] inclinations, double[] azimuths,
            double angularTolerance, double minimumSineInclination,
            out int[] breaks, out ReducedOrderCurveType[] types)
        {
            Segment(measuredDepths, tangents, inclinations, azimuths, angularTolerance,
                minimumSineInclination, false, out breaks, out types);
        }

        /// <summary>
        /// The same segmentation, with the choice of whether to reproduce the defect described under
        /// <see cref="ReductionOptions.ReproduceReferenceSegmentation"/>.
        /// </summary>
        /// <param name="measuredDepths">Measured depth at each station, m, strictly increasing.</param>
        /// <param name="tangents">Three doubles per station: north, east, vertical.</param>
        /// <param name="inclinations">Inclination at each station, rad.</param>
        /// <param name="azimuths">Azimuth at each station, rad, unwrapped along the hole.</param>
        /// <param name="angularTolerance">The largest angular deviation a section may leave, rad.</param>
        /// <param name="minimumSineInclination">The vertical guard, as a sine of the inclination.</param>
        /// <param name="reproduceReferenceSegmentation">
        /// True to leave the last station of the survey untested where the doubling overshoots it, as the
        /// reference implementation does.
        /// </param>
        /// <param name="breaks">Receives the station indices the sections run between.</param>
        /// <param name="types">Receives the family each section is drawn with.</param>
        public static void Segment(
            double[] measuredDepths, double[] tangents, double[] inclinations, double[] azimuths,
            double angularTolerance, double minimumSineInclination, bool reproduceReferenceSegmentation,
            out int[] breaks, out ReducedOrderCurveType[] types)
        {
            int stationCount = measuredDepths.Length;
            List<int> foundBreaks = new List<int>(stationCount) { 0 };
            List<ReducedOrderCurveType> foundTypes = new List<ReducedOrderCurveType>(stationCount);
            if (stationCount < 2)
            {
                breaks = foundBreaks.ToArray();
                types = foundTypes.ToArray();
                return;
            }

            // Where the last station at or before each index lies too close to the vertical for the two
            // azimuth carrying families, so that asking whether an interval contains one is a comparison
            // rather than a scan. Specification section 3.5.
            int[] lastNearVertical = new int[stationCount];
            int seen = -1;
            for (int n = 0; n < stationCount; n++)
            {
                if (System.Math.Sin(inclinations[n]) < minimumSineInclination)
                {
                    seen = n;
                }
                lastNearVertical[n] = seen;
            }

            FitWorkspace workspace = new FitWorkspace(stationCount);
            int start = 0;
            while (start < stationCount - 1)
            {
                int bestEnd = -1;
                ReducedOrderCurveType bestType = ReducedOrderCurveType.CircularArc;
                for (int family = 0; family < 4; family++)
                {
                    ReducedOrderCurveType curveType = (ReducedOrderCurveType)family;
                    bool azimuthCarrying = curveType == ReducedOrderCurveType.ConstantCurvatureAndToolface
                        || curveType == ReducedOrderCurveType.ConstantBuildAndTurn;

                    // Double the reach until the fit no longer holds.
                    int step = 1;
                    int lastFeasible = -1;
                    int end = start + 1;
                    while (end <= stationCount - 1)
                    {
                        if (azimuthCarrying && lastNearVertical[end] >= start)
                        {
                            break;
                        }
                        if (FitSection(curveType, measuredDepths, tangents, inclinations, azimuths,
                                start, end, out _, out _, workspace) <= angularTolerance)
                        {
                            lastFeasible = end;
                            step *= 2;
                            end = start + step;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (lastFeasible < 0)
                    {
                        continue;
                    }

                    // Bisect between the last interval that held and the first that did not.
                    int low = lastFeasible;
                    int high = System.Math.Min(end, stationCount - 1);

                    // Where the doubling ran off the end of the survey rather than meeting an interval
                    // it could not fit, the upper end of the bracket is the last station, which has
                    // never been tried: the bisection below stops as soon as the two ends are adjacent,
                    // so it can never reach it. Leaving it untried cuts the section short and costs a
                    // whole extra section, which on the reference wells is one section in fourteen.
                    if (!reproduceReferenceSegmentation && end > stationCount - 1 && high > low)
                    {
                        bool reachable = !(azimuthCarrying && lastNearVertical[high] >= start);
                        if (reachable
                            && FitSection(curveType, measuredDepths, tangents, inclinations, azimuths,
                                start, high, out _, out _, workspace) <= angularTolerance)
                        {
                            low = high;
                        }
                    }

                    while (low + 1 < high)
                    {
                        int middle = (low + high) / 2;
                        bool allowed = !(azimuthCarrying && lastNearVertical[middle] >= start);
                        if (allowed
                            && FitSection(curveType, measuredDepths, tangents, inclinations, azimuths,
                                start, middle, out _, out _, workspace) <= angularTolerance)
                        {
                            low = middle;
                        }
                        else
                        {
                            high = middle;
                        }
                    }
                    if (low > bestEnd)
                    {
                        bestEnd = low;
                        bestType = curveType;
                    }
                }

                if (bestEnd < 0)
                {
                    // Any two attitudes are related by a rotation, so a circular arc always spans two
                    // stations exactly. This is what guarantees the loop makes progress.
                    bestEnd = start + 1;
                    bestType = ReducedOrderCurveType.CircularArc;
                }
                foundTypes.Add(bestType);
                foundBreaks.Add(bestEnd);
                start = bestEnd;
            }

            breaks = foundBreaks.ToArray();
            types = foundTypes.ToArray();
        }

        // =====================================================================================
        // Forward evaluation of a whole profile. Specification section 4.
        // =====================================================================================

        /// <summary>
        /// The tangents of a whole profile on a sorted measured depth grid, each section picking up where
        /// the one before it left off. Specification section 4.
        ///
        /// Two details decide whether this is right. The tangent handed on is evaluated at the
        /// <i>next section's starting depth</i>, not at the last grid point of the current section, which
        /// are different points whenever the grid does not have a point exactly at the junction. And the
        /// tangents are renormalised at the end, because the two angle based families return a vector
        /// whose length is one only up to rounding.
        /// </summary>
        /// <param name="initialInclination">Inclination of the first station, rad.</param>
        /// <param name="initialAzimuth">Azimuth of the first station, rad.</param>
        /// <param name="types">The family of each section.</param>
        /// <param name="parameters">Two doubles per section; the slots of a hold are ignored.</param>
        /// <param name="sectionLow">Index into the grid where each section starts.</param>
        /// <param name="sectionHigh">Index into the grid just past where each section ends.</param>
        /// <param name="grid">The measured depth grid, m, sorted.</param>
        /// <param name="distances">Scratch, at least as long as the grid.</param>
        /// <param name="tangents">Receives three doubles per grid point.</param>
        public static void ForwardProfile(
            double initialInclination, double initialAzimuth,
            ReducedOrderCurveType[] types, double[] parameters,
            int[] sectionLow, int[] sectionHigh, double[] grid,
            double[] distances, double[] tangents)
        {
            ForwardProfile(initialInclination, initialAzimuth, types, parameters,
                sectionLow, sectionHigh, grid, distances, tangents, new double[1], new double[3]);
        }

        /// <summary>
        /// The same forward evaluation, with the two tiny buffers the handover between sections needs
        /// supplied by the caller. The refinement evaluates this once per unknown per iteration and must
        /// not allocate while doing so.
        /// </summary>
        private static void ForwardProfile(
            double initialInclination, double initialAzimuth,
            ReducedOrderCurveType[] types, double[] parameters,
            int[] sectionLow, int[] sectionHigh, double[] grid,
            double[] distances, double[] tangents,
            double[] exitDistance, double[] exitTangent)
        {
            int sectionCount = types.Length;
            TangentFromAngles(initialInclination, initialAzimuth,
                out double inheritedNorth, out double inheritedEast, out double inheritedVertical);

            for (int q = 0; q < sectionCount; q++)
            {
                int low = sectionLow[q];
                int high = sectionHigh[q];
                double start = grid[low];
                for (int n = low; n < high; n++)
                {
                    distances[n] = grid[n] - start;
                }
                SectionTangents(types[q], parameters[2 * q], parameters[2 * q + 1],
                    inheritedNorth, inheritedEast, inheritedVertical,
                    distances, low, high - low, tangents, low);

                if (q < sectionCount - 1)
                {
                    ExitTangent(types[q], parameters[2 * q], parameters[2 * q + 1],
                        inheritedNorth, inheritedEast, inheritedVertical,
                        grid[sectionLow[q + 1]] - start, exitDistance, exitTangent,
                        out inheritedNorth, out inheritedEast, out inheritedVertical);
                }
            }

            int pointCount = grid.Length;
            for (int n = 0; n < pointCount; n++)
            {
                int at = 3 * n;
                double length = System.Math.Sqrt(
                    tangents[at] * tangents[at]
                    + tangents[at + 1] * tangents[at + 1]
                    + tangents[at + 2] * tangents[at + 2]);
                if (length < 1.0e-12)
                {
                    length = 1.0;
                }
                tangents[at] /= length;
                tangents[at + 1] /= length;
                tangents[at + 2] /= length;
            }
        }

        /// <summary>
        /// The unit tangent one section hands to the next, at the distance along it where the next one
        /// starts. Specification section 4.
        /// </summary>
        private static void ExitTangent(
            ReducedOrderCurveType curveType, double firstParameter, double secondParameter,
            double inheritedNorth, double inheritedEast, double inheritedVertical,
            double exitDistance, double[] one, double[] tangent,
            out double north, out double east, out double vertical)
        {
            one[0] = exitDistance;
            SectionTangents(curveType, firstParameter, secondParameter,
                inheritedNorth, inheritedEast, inheritedVertical, one, 0, 1, tangent, 0);
            double length = System.Math.Sqrt(
                tangent[0] * tangent[0] + tangent[1] * tangent[1] + tangent[2] * tangent[2]);
            if (length < 1.0e-12)
            {
                length = 1.0;
            }
            north = tangent[0] / length;
            east = tangent[1] / length;
            vertical = tangent[2] / length;
        }

        // =====================================================================================
        // Chained initialisation. Specification section 7.
        // =====================================================================================

        /// <summary>
        /// The starting parameters of every section, each fitted in the frame it actually inherits rather
        /// than against the observed tangent at its own first station. Specification section 7.
        ///
        /// The segmentation fits each interval against what the survey says at its start, but in the
        /// assembled profile a section starts on whatever direction the section before it ended at, and
        /// the two are not the same. Fitting against the wrong frame and then chaining the result lets the
        /// error compound: on a three kilometre well the reference measured three hundred and forty five
        /// metres of drift. So each section is re-fitted here on its own stations rotated into the frame
        /// it inherits, and the exit of that fit is what the next section inherits in turn.
        /// </summary>
        /// <param name="measuredDepths">Measured depth at each station, m.</param>
        /// <param name="tangents">Three doubles per station: north, east, vertical.</param>
        /// <param name="breaks">The station indices the sections run between.</param>
        /// <param name="types">The family of each section.</param>
        /// <param name="initialInclination">Inclination the profile sets off at, rad.</param>
        /// <param name="initialAzimuth">Azimuth the profile sets off at, rad.</param>
        /// <returns>Two parameters per section; the slots of a hold are left at zero.</returns>
        public static double[] ChainedInitialisation(
            double[] measuredDepths, double[] tangents, int[] breaks, ReducedOrderCurveType[] types,
            double initialInclination, double initialAzimuth)
        {
            int sectionCount = types.Length;
            double[] parameters = new double[2 * sectionCount];
            if (sectionCount == 0)
            {
                return parameters;
            }

            int longest = 0;
            for (int q = 0; q < sectionCount; q++)
            {
                int span = breaks[q + 1] - breaks[q] + 1;
                if (span > longest)
                {
                    longest = span;
                }
            }
            double[] rotatedTangents = new double[3 * longest];
            double[] rotatedInclinations = new double[longest];
            double[] rotatedAzimuths = new double[longest];
            double[] sectionDepths = new double[longest];
            double[] rotation = new double[9];
            double[] exitDistance = new double[1];
            double[] exitTangent = new double[3];
            FitWorkspace workspace = new FitWorkspace(longest);

            TangentFromAngles(initialInclination, initialAzimuth,
                out double inheritedNorth, out double inheritedEast, out double inheritedVertical);

            for (int q = 0; q < sectionCount; q++)
            {
                int first = breaks[q];
                int last = breaks[q + 1];
                int pointCount = last - first + 1;

                int anchor = 3 * first;
                RotationAligning(tangents[anchor], tangents[anchor + 1], tangents[anchor + 2],
                    inheritedNorth, inheritedEast, inheritedVertical, rotation);

                double previousAzimuth = 0.0;
                for (int n = 0; n < pointCount; n++)
                {
                    int observed = 3 * (first + n);
                    ApplyRotation(rotation, tangents[observed], tangents[observed + 1], tangents[observed + 2],
                        out double north, out double east, out double vertical);
                    double length = System.Math.Sqrt(north * north + east * east + vertical * vertical);
                    north /= length;
                    east /= length;
                    vertical /= length;
                    rotatedTangents[3 * n] = north;
                    rotatedTangents[3 * n + 1] = east;
                    rotatedTangents[3 * n + 2] = vertical;
                    rotatedInclinations[n] = System.Math.Acos(Clamp(vertical, -1.0, 1.0));
                    double azimuth = System.Math.Atan2(east, north);
                    if (n > 0)
                    {
                        // Unwrapped along the section, so that the two regression based fits see a
                        // continuous azimuth rather than one jumping a whole turn at north.
                        while (azimuth - previousAzimuth > System.Math.PI)
                        {
                            azimuth -= 2.0 * System.Math.PI;
                        }
                        while (azimuth - previousAzimuth < -System.Math.PI)
                        {
                            azimuth += 2.0 * System.Math.PI;
                        }
                    }
                    rotatedAzimuths[n] = azimuth;
                    previousAzimuth = azimuth;
                    sectionDepths[n] = measuredDepths[first + n];
                }

                FitSection(types[q], sectionDepths, rotatedTangents, rotatedInclinations, rotatedAzimuths,
                    0, pointCount - 1, out double firstParameter, out double secondParameter, workspace);
                parameters[2 * q] = firstParameter;
                parameters[2 * q + 1] = secondParameter;

                ExitTangent(types[q], firstParameter, secondParameter,
                    inheritedNorth, inheritedEast, inheritedVertical,
                    measuredDepths[last] - measuredDepths[first], exitDistance, exitTangent,
                    out inheritedNorth, out inheritedEast, out inheritedVertical);
            }
            return parameters;
        }

        // =====================================================================================
        // Global refinement. Specification section 8.
        // =====================================================================================

        /// <summary>
        /// Settles the whole profile at once, against the attitude of every station and then against the
        /// position of every station. Specification section 8.
        ///
        /// The chained initialisation gets the shape right but not the place: each section is fitted
        /// against its own stations without any regard for where that leaves the sections after it, and
        /// the small errors add up along the hole. This moves every section parameter, and the attitude
        /// the profile sets off at, together.
        ///
        /// It runs in two stages. The attitude residual comes first because it has no lever arm — an
        /// error in an early section does not grow downhole — so it is well conditioned and lands close
        /// to the answer cheaply. The position residual, which is what a caller actually cares about,
        /// then starts from there. Position solves are restarted from small perturbations of the best
        /// answer so far until two of them in a row fail to improve on it, because stopping at the first
        /// leaves about one well in twenty in a poor local minimum.
        ///
        /// The starting point is always kept as a candidate, so this can never return something worse
        /// than the chained initialisation it was given.
        /// </summary>
        /// <param name="measuredDepths">Measured depth at each station, m.</param>
        /// <param name="observedTangents">Three doubles per station, the survey's own tangents.</param>
        /// <param name="observedPositions">Three doubles per station, the survey's own positions.</param>
        /// <param name="breaks">The station indices the sections run between.</param>
        /// <param name="types">The family of each section.</param>
        /// <param name="initialInclination">
        /// On entry the inclination the chained initialisation set off at, on exit the refined one, rad.
        /// </param>
        /// <param name="initialAzimuth">Likewise for the azimuth, rad.</param>
        /// <param name="parameters">
        /// On entry the chained initialisation's parameters, on exit the refined ones. Two per section.
        /// </param>
        /// <param name="options">The settings to run under, or null for the defaults.</param>
        /// <returns>
        /// The largest distance, m, between a station of the survey and the refined profile at the same
        /// measured depth, measured on the station grid.
        /// </returns>
        public static double Refine(
            double[] measuredDepths, double[] observedTangents, double[] observedPositions,
            int[] breaks, ReducedOrderCurveType[] types,
            ref double initialInclination, ref double initialAzimuth, double[] parameters,
            ReductionOptions? options = null)
        {
            options ??= new ReductionOptions();
            int stationCount = measuredDepths.Length;
            int sectionCount = types.Length;

            // Only the sections carrying parameters are free. A hold's two dead slots would contribute
            // two columns of exact zeros to the Jacobian and make the normal equations singular.
            int activeCount = 0;
            for (int q = 0; q < sectionCount; q++)
            {
                if (types[q] != ReducedOrderCurveType.Hold)
                {
                    activeCount += 2;
                }
            }
            int[] active = new int[activeCount];
            int writtenActive = 0;
            for (int q = 0; q < sectionCount; q++)
            {
                if (types[q] != ReducedOrderCurveType.Hold)
                {
                    active[writtenActive++] = 2 * q;
                    active[writtenActive++] = 2 * q + 1;
                }
            }

            RefinementWorkspace work = new RefinementWorkspace(
                stationCount, sectionCount, activeCount + 2, breaks);

            double[] start = new double[activeCount + 2];
            start[0] = initialInclination;
            start[1] = initialAzimuth;
            for (int a = 0; a < activeCount; a++)
            {
                start[a + 2] = parameters[active[a]];
            }

            double startDeviation = MaximumStationDeviation(start, measuredDepths, observedPositions,
                types, parameters, active, work);
            double[] best = (double[])start.Clone();
            double bestDeviation = startDeviation;

            if (activeCount == 0 || !options.Refine)
            {
                return bestDeviation;
            }

            // Absolute finite difference steps. Specification section 8.1: a relative step collapses on a
            // section parameter, which sits around a thousandth and legitimately passes through zero.
            double[] step = new double[activeCount + 2];
            step[0] = 1.0e-6;
            step[1] = 1.0e-6;
            for (int a = 0; a < activeCount; a++)
            {
                step[a + 2] = 1.0e-9;
            }

            double[] current = (double[])start.Clone();
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();

            // Stage one: the attitude residual.
            Solve(current, step, false, measuredDepths, observedTangents, observedPositions,
                types, parameters, active, work, options.MaximumIterationsPerSolve);
            double deviation = MaximumStationDeviation(current, measuredDepths, observedPositions,
                types, parameters, active, work);
            if (deviation < bestDeviation)
            {
                System.Array.Copy(current, best, current.Length);
                bestDeviation = deviation;
            }

            // Stage two: the position residual, restarted until it stops improving.
            Random random = new Random(options.RandomSeed);
            double previousDeviation = double.PositiveInfinity;
            int stalled = 0;
            for (int restart = 0; restart < options.MaximumRestarts; restart++)
            {
                Solve(current, step, true, measuredDepths, observedTangents, observedPositions,
                    types, parameters, active, work, options.MaximumIterationsPerSolve);
                deviation = MaximumStationDeviation(current, measuredDepths, observedPositions,
                    types, parameters, active, work);
                if (deviation < bestDeviation)
                {
                    System.Array.Copy(current, best, current.Length);
                    bestDeviation = deviation;
                }

                // Two consecutive restarts which do not improve on the best so far end it. One is not
                // enough: a well has been seen to fall from sixteen metres to a third of a metre only on
                // the fourth restart.
                if (previousDeviation - bestDeviation <= 1.0e-3 * System.Math.Max(previousDeviation, 1.0e-9))
                {
                    stalled++;
                    if (stalled >= 2)
                    {
                        break;
                    }
                }
                else
                {
                    stalled = 0;
                }
                if (clock.Elapsed >= options.RefinementBudget)
                {
                    break;
                }
                previousDeviation = bestDeviation;
                for (int p = 0; p < current.Length; p++)
                {
                    current[p] = best[p] + NextStandardNormal(random) * step[p] * 3.0e3;
                }
            }

            initialInclination = best[0];
            initialAzimuth = best[1];
            for (int a = 0; a < activeCount; a++)
            {
                parameters[active[a]] = best[a + 2];
            }
            return bestDeviation;
        }

        /// <summary>
        /// One Levenberg Marquardt solve, in place on <paramref name="x"/>. Specification section 8.3.
        ///
        /// The column scaling is taken from the Jacobian rather than fixed, and is the largest column norm
        /// seen so far so that it cannot shrink and let a direction run away. Fixed scaling is not good
        /// enough here: how much the final position moves when an early section parameter moves exceeds
        /// the same for a late one by roughly the ratio of the hole remaining below them, which is orders
        /// of magnitude on a long well.
        ///
        /// The step comes from the normal equations by Cholesky. Where the damped normal matrix is not
        /// positive definite, which happens when the damping is small and the problem locally singular,
        /// the damping is raised and the factorisation tried again rather than falling back to a
        /// decomposition of the Jacobian itself; raising it enough always succeeds, because the matrix
        /// becomes diagonally dominant.
        /// </summary>
        private static void Solve(
            double[] x, double[] step, bool usePosition,
            double[] measuredDepths, double[] observedTangents, double[] observedPositions,
            ReducedOrderCurveType[] types, double[] parameters, int[] active,
            RefinementWorkspace work, int maximumIterations)
        {
            int unknownCount = x.Length;
            int residualCount = 3 * measuredDepths.Length;
            double[] residual = work.Residual;
            double[] trialResidual = work.TrialResidual;
            double[] jacobian = work.Jacobian;
            double[] scale = work.ColumnScale;
            double[] normalMatrix = work.NormalMatrix;
            double[] gradient = work.Gradient;
            double[] delta = work.Delta;
            double[] trial = work.Trial;

            System.Array.Clear(scale, 0, unknownCount);
            Residuals(x, usePosition, measuredDepths, observedTangents, observedPositions,
                types, parameters, active, work, residual);
            double squaredNorm = SquaredNorm(residual, residualCount);
            double damping = 1.0e-3;

            for (int iteration = 0; iteration < maximumIterations; iteration++)
            {
                // Forward difference Jacobian, one residual evaluation per unknown.
                for (int p = 0; p < unknownCount; p++)
                {
                    double saved = x[p];
                    x[p] = saved + step[p];
                    Residuals(x, usePosition, measuredDepths, observedTangents, observedPositions,
                        types, parameters, active, work, trialResidual);
                    x[p] = saved;
                    double inverseStep = 1.0 / step[p];
                    int column = p * residualCount;
                    for (int r = 0; r < residualCount; r++)
                    {
                        jacobian[column + r] = (trialResidual[r] - residual[r]) * inverseStep;
                    }
                }

                // Column norms, kept at the largest seen so far, and the normal equations.
                for (int p = 0; p < unknownCount; p++)
                {
                    int column = p * residualCount;
                    double sum = 0.0;
                    for (int r = 0; r < residualCount; r++)
                    {
                        sum += jacobian[column + r] * jacobian[column + r];
                    }
                    double norm = System.Math.Sqrt(sum);
                    if (norm > scale[p])
                    {
                        scale[p] = norm;
                    }
                    double accumulated = 0.0;
                    for (int r = 0; r < residualCount; r++)
                    {
                        accumulated += jacobian[column + r] * residual[r];
                    }
                    gradient[p] = accumulated;
                }
                for (int p = 0; p < unknownCount; p++)
                {
                    int columnP = p * residualCount;
                    for (int q = p; q < unknownCount; q++)
                    {
                        int columnQ = q * residualCount;
                        double sum = 0.0;
                        for (int r = 0; r < residualCount; r++)
                        {
                            sum += jacobian[columnP + r] * jacobian[columnQ + r];
                        }
                        work.CrossProduct[p * unknownCount + q] = sum;
                        work.CrossProduct[q * unknownCount + p] = sum;
                    }
                }

                bool improved = false;
                for (int attempt = 0; attempt < 40; attempt++)
                {
                    System.Array.Copy(work.CrossProduct, normalMatrix, unknownCount * unknownCount);
                    for (int p = 0; p < unknownCount; p++)
                    {
                        double floored = System.Math.Max(scale[p], 1.0e-12);
                        normalMatrix[p * unknownCount + p] += damping * floored * floored;
                    }
                    if (!SolveByCholesky(normalMatrix, gradient, delta, unknownCount))
                    {
                        damping *= 10.0;
                        if (damping > 1.0e20)
                        {
                            return;
                        }
                        continue;
                    }
                    double stepLength = 0.0;
                    double pointLength = 0.0;
                    for (int p = 0; p < unknownCount; p++)
                    {
                        trial[p] = x[p] - delta[p];
                        stepLength += delta[p] * delta[p];
                        pointLength += x[p] * x[p];
                    }
                    stepLength = System.Math.Sqrt(stepLength);
                    pointLength = System.Math.Sqrt(pointLength);

                    Residuals(trial, usePosition, measuredDepths, observedTangents, observedPositions,
                        types, parameters, active, work, trialResidual);
                    double trialSquaredNorm = SquaredNorm(trialResidual, residualCount);
                    if (trialSquaredNorm < squaredNorm)
                    {
                        System.Array.Copy(trial, x, unknownCount);
                        System.Array.Copy(trialResidual, residual, residualCount);
                        double reduction = (squaredNorm - trialSquaredNorm) / System.Math.Max(squaredNorm, 1.0e-300);
                        squaredNorm = trialSquaredNorm;
                        damping /= 3.0;
                        if (damping < 1.0e-14)
                        {
                            damping = 1.0e-14;
                        }
                        improved = true;
                        if (reduction < 1.0e-12 || stepLength <= 1.0e-14 * (pointLength + 1.0e-14))
                        {
                            return;
                        }
                        break;
                    }
                    damping *= 3.0;
                    if (damping > 1.0e20)
                    {
                        return;
                    }
                }
                if (!improved)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// The residual of the whole profile against the survey, either attitude or position.
        /// Specification section 8. Both are three numbers per station, so that a station out of place in
        /// one direction cannot be traded off against another.
        /// </summary>
        private static void Residuals(
            double[] x, bool usePosition,
            double[] measuredDepths, double[] observedTangents, double[] observedPositions,
            ReducedOrderCurveType[] types, double[] parameters, int[] active,
            RefinementWorkspace work, double[] residual)
        {
            double[] full = work.FullParameters;
            System.Array.Copy(parameters, full, parameters.Length);
            for (int a = 0; a < active.Length; a++)
            {
                full[active[a]] = x[a + 2];
            }
            ForwardProfile(x[0], x[1], types, full, work.SectionLow, work.SectionHigh,
                measuredDepths, work.Distances, work.ModelTangents, work.ExitDistance, work.ExitTangent);

            int count = 3 * measuredDepths.Length;
            if (!usePosition)
            {
                for (int r = 0; r < count; r++)
                {
                    residual[r] = work.ModelTangents[r] - observedTangents[r];
                }
                return;
            }
            MinimumCurvature(measuredDepths, work.ModelTangents, work.ModelPositions);
            for (int r = 0; r < count; r++)
            {
                residual[r] = work.ModelPositions[r] - observedPositions[r];
            }
        }

        /// <summary>
        /// The largest distance between a station and the profile the decision vector describes, m. This
        /// is what the restart loop judges a candidate by, and it is deliberately the worst station rather
        /// than the sum of squares the solve itself minimises.
        /// </summary>
        private static double MaximumStationDeviation(
            double[] x, double[] measuredDepths, double[] observedPositions,
            ReducedOrderCurveType[] types, double[] parameters, int[] active,
            RefinementWorkspace work)
        {
            Residuals(x, true, measuredDepths, System.Array.Empty<double>(), observedPositions,
                types, parameters, active, work, work.Residual);
            double worst = 0.0;
            int stationCount = measuredDepths.Length;
            for (int i = 0; i < stationCount; i++)
            {
                int at = 3 * i;
                double distance = System.Math.Sqrt(
                    work.Residual[at] * work.Residual[at]
                    + work.Residual[at + 1] * work.Residual[at + 1]
                    + work.Residual[at + 2] * work.Residual[at + 2]);
                if (distance > worst)
                {
                    worst = distance;
                }
            }
            return worst;
        }

        private static double SquaredNorm(double[] values, int count)
        {
            double sum = 0.0;
            for (int i = 0; i < count; i++)
            {
                sum += values[i] * values[i];
            }
            return sum;
        }

        /// <summary>
        /// Solves a symmetric positive definite system in place by Cholesky, returning false where the
        /// matrix turns out not to be positive definite.
        /// </summary>
        private static bool SolveByCholesky(double[] matrix, double[] rightHandSide, double[] solution, int order)
        {
            for (int i = 0; i < order; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    double sum = matrix[i * order + j];
                    for (int k = 0; k < j; k++)
                    {
                        sum -= matrix[i * order + k] * matrix[j * order + k];
                    }
                    if (i == j)
                    {
                        if (sum <= 0.0)
                        {
                            return false;
                        }
                        matrix[i * order + i] = System.Math.Sqrt(sum);
                    }
                    else
                    {
                        matrix[i * order + j] = sum / matrix[j * order + j];
                    }
                }
            }
            for (int i = 0; i < order; i++)
            {
                double sum = rightHandSide[i];
                for (int k = 0; k < i; k++)
                {
                    sum -= matrix[i * order + k] * solution[k];
                }
                solution[i] = sum / matrix[i * order + i];
            }
            for (int i = order - 1; i >= 0; i--)
            {
                double sum = solution[i];
                for (int k = i + 1; k < order; k++)
                {
                    sum -= matrix[k * order + i] * solution[k];
                }
                solution[i] = sum / matrix[i * order + i];
            }
            return true;
        }

        /// <summary>
        /// A standard normal deviate by the polar form of the Box Muller transform, so that the
        /// perturbations depend only on the seed and not on the framework's own normal generator.
        /// </summary>
        private static double NextStandardNormal(Random random)
        {
            double first, second, squared;
            do
            {
                first = 2.0 * random.NextDouble() - 1.0;
                second = 2.0 * random.NextDouble() - 1.0;
                squared = first * first + second * second;
            }
            while (squared >= 1.0 || squared <= 0.0);
            return first * System.Math.Sqrt(-2.0 * System.Math.Log(squared) / squared);
        }

        // =====================================================================================
        // The reporting grid. Specification section 9.
        // =====================================================================================

        /// <summary>
        /// The grid the reduced profile is measured on: every station, every section boundary, and each
        /// section subdivided evenly at about <paramref name="measurementStep"/>. Specification section 9.
        /// </summary>
        /// <param name="measuredDepths">Measured depth at each station, m.</param>
        /// <param name="breaks">The station indices the sections run between.</param>
        /// <param name="measurementStep">The step to subdivide each section at, m.</param>
        /// <param name="grid">Receives the sorted grid, with duplicates removed.</param>
        /// <param name="sectionLow">Receives the index into the grid where each section starts.</param>
        /// <param name="sectionHigh">Receives the index just past where each section ends.</param>
        /// <param name="stationIndex">Receives, for each station, its index in the grid.</param>
        public static void BuildMeasurementGrid(
            double[] measuredDepths, int[] breaks, double measurementStep,
            out double[] grid, out int[] sectionLow, out int[] sectionHigh, out int[] stationIndex)
        {
            int stationCount = measuredDepths.Length;
            int sectionCount = breaks.Length - 1;
            List<double> points = new List<double>(stationCount * 2);
            points.AddRange(measuredDepths);
            for (int q = 0; q < sectionCount; q++)
            {
                double from = measuredDepths[breaks[q]];
                double to = measuredDepths[breaks[q + 1]];
                int pieces = (int)System.Math.Ceiling((to - from) / measurementStep);
                if (pieces <= 1)
                {
                    continue;
                }
                for (int n = 0; n <= pieces; n++)
                {
                    // The ends are written as themselves rather than computed, so that they coincide
                    // exactly with the station depths already in the list and fall away as duplicates.
                    points.Add(n == 0 ? from : (n == pieces ? to : from + (to - from) * n / pieces));
                }
            }
            points.Sort();
            List<double> unique = new List<double>(points.Count) { points[0] };
            for (int n = 1; n < points.Count; n++)
            {
                if (points[n] != unique[unique.Count - 1])
                {
                    unique.Add(points[n]);
                }
            }
            grid = unique.ToArray();

            sectionLow = new int[sectionCount];
            sectionHigh = new int[sectionCount];
            for (int q = 0; q < sectionCount; q++)
            {
                sectionLow[q] = LowerBound(grid, measuredDepths[breaks[q]]);
            }
            for (int q = 0; q < sectionCount - 1; q++)
            {
                sectionHigh[q] = sectionLow[q + 1];
            }
            if (sectionCount > 0)
            {
                sectionHigh[sectionCount - 1] = grid.Length;
            }

            stationIndex = new int[stationCount];
            for (int i = 0; i < stationCount; i++)
            {
                stationIndex[i] = LowerBound(grid, measuredDepths[i]);
            }
        }

        /// <summary>
        /// The index of the first entry of a sorted array which is not less than the value sought.
        /// </summary>
        private static int LowerBound(double[] sorted, double value)
        {
            int low = 0;
            int high = sorted.Length;
            while (low < high)
            {
                int middle = (low + high) / 2;
                if (sorted[middle] < value)
                {
                    low = middle + 1;
                }
                else
                {
                    high = middle;
                }
            }
            return low;
        }

        // =====================================================================================
        // Small shared helpers.
        // =====================================================================================

        /// <summary>
        /// What a fit reports when the interval has no length or the curve it describes degenerates. It is
        /// far above any tolerance a caller could ask for, so such an interval is simply never feasible.
        /// </summary>
        private const double DegenerateResidual = 1.0e9;

        /// <summary>
        /// Scratch a closed form fit needs, owned by the caller so that the segmentation, which runs a
        /// few thousand fits on a long well, allocates once rather than once a fit.
        /// </summary>
        private sealed class FitWorkspace
        {
            internal readonly double[] Distances;
            internal readonly double[] Predicted;

            internal FitWorkspace(int stationCount)
            {
                Distances = new double[stationCount];
                Predicted = new double[3 * stationCount];
            }
        }

        /// <summary>
        /// Everything the refinement writes into, taken once when it starts. A solve evaluates the whole
        /// profile once per unknown per iteration, a few hundred thousand times over a reduction, so
        /// nothing inside that loop may allocate.
        /// </summary>
        private sealed class RefinementWorkspace
        {
            internal readonly int[] SectionLow;
            internal readonly int[] SectionHigh;
            internal readonly double[] Distances;
            internal readonly double[] ModelTangents;
            internal readonly double[] ModelPositions;
            internal readonly double[] FullParameters;
            internal readonly double[] Residual;
            internal readonly double[] TrialResidual;
            internal readonly double[] Jacobian;
            internal readonly double[] ColumnScale;
            internal readonly double[] CrossProduct;
            internal readonly double[] NormalMatrix;
            internal readonly double[] Gradient;
            internal readonly double[] Delta;
            internal readonly double[] Trial;
            internal readonly double[] ExitDistance;
            internal readonly double[] ExitTangent;

            internal RefinementWorkspace(int stationCount, int sectionCount, int unknownCount, int[] breaks)
            {
                SectionLow = new int[sectionCount];
                SectionHigh = new int[sectionCount];
                for (int q = 0; q < sectionCount; q++)
                {
                    SectionLow[q] = breaks[q];
                }
                for (int q = 0; q < sectionCount - 1; q++)
                {
                    SectionHigh[q] = SectionLow[q + 1];
                }
                if (sectionCount > 0)
                {
                    SectionHigh[sectionCount - 1] = stationCount;
                }

                Distances = new double[stationCount];
                ModelTangents = new double[3 * stationCount];
                ModelPositions = new double[3 * stationCount];
                FullParameters = new double[2 * sectionCount];
                Residual = new double[3 * stationCount];
                TrialResidual = new double[3 * stationCount];
                Jacobian = new double[3 * stationCount * unknownCount];
                ColumnScale = new double[unknownCount];
                CrossProduct = new double[unknownCount * unknownCount];
                NormalMatrix = new double[unknownCount * unknownCount];
                Gradient = new double[unknownCount];
                Delta = new double[unknownCount];
                Trial = new double[unknownCount];
                ExitDistance = new double[1];
                ExitTangent = new double[3];
            }
        }

        /// <summary>
        /// Holds a value inside the closed interval given.
        /// </summary>
        internal static double Clamp(double value, double lower, double upper)
        {
            if (value < lower)
            {
                return lower;
            }
            return value > upper ? upper : value;
        }

        /// <summary>
        /// Writes the inherited tangent unchanged at every point of a section, which is what a hold does
        /// and what a circular arc of no curvature comes to.
        /// </summary>
        private static void WriteInherited(double north, double east, double vertical, int count, double[] tangents, int firstTangent)
        {
            for (int i = 0; i < count; i++)
            {
                int at = 3 * (firstTangent + i);
                tangents[at] = north;
                tangents[at + 1] = east;
                tangents[at + 2] = vertical;
            }
        }
    }
}
