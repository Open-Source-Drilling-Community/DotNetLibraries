using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// The volume an existing well occupies as far as a planned well is concerned: the ellipse of
    /// uncertainty of every survey station, swept along the trajectory.
    /// <para>
    /// The trajectory is reconstructed from the survey by the circular arc construction of
    /// <see cref="TrajectoryPoint3D.CompleteCASIA"/>, so that this shares the one implementation of that
    /// mathematics rather than keeping a copy of it, and then resampled at a fixed step. Queries are
    /// answered against the nearest sample, which is accurate while the step stays short against the
    /// radius of curvature.
    /// </para>
    /// <para>
    /// An interval of measured depth may be muted, which is what lets a sidetrack leave its parent: over
    /// that interval the parent is simply not an obstacle. Muting leaves a genuine gap in the solid, and
    /// the query accounts for the distance past the end of the remaining tube so that a position in the
    /// gap is correctly reported as clear rather than as being beside the nearest surviving station.
    /// </para>
    /// </summary>
    public class WellboreUncertainty
    {
        private readonly double[] north_;
        private readonly double[] east_;
        private readonly double[] vertical_;
        private readonly double[] measuredDepth_;
        private readonly double[] semiMajor_;
        private readonly double[] semiMinor_;
        private readonly double[] frame_;          // per sample: tangent, high side, right side
        private readonly double[] angleCosine_;
        private readonly double[] angleSine_;

        /// <summary>
        /// the name of the well this volume belongs to
        /// </summary>
        public string? Name { get; set; } = null;

        /// <summary>
        /// the distance between two consecutive samples of the trajectory, m
        /// </summary>
        public double SampleStep { get; }

        /// <summary>
        /// the largest semi-major axis anywhere along the well, m
        /// </summary>
        public double MaximumSemiAxis { get; }

        /// <summary>
        /// the lowest corner of the box holding the whole volume
        /// </summary>
        public double[] BoundingBoxMinimum { get; } = new double[3];

        /// <summary>
        /// the highest corner of the box holding the whole volume
        /// </summary>
        public double[] BoundingBoxMaximum { get; } = new double[3];

        /// <summary>
        /// the largest product of the semi-major axis and the curvature of the trajectory. The swept
        /// ellipse is a solid only while this stays below one; past it the perpendicular sections cut
        /// into one another on the inside of a bend and the sweep folds over itself.
        /// </summary>
        public double MaximumSweepRatio { get; }

        /// <summary>
        /// the start of the muted interval of measured depth, or null when nothing is muted
        /// </summary>
        public double? MutedFrom { get; set; } = null;

        /// <summary>
        /// the end of the muted interval of measured depth, or null when nothing is muted
        /// </summary>
        public double? MutedTo { get; set; } = null;

        /// <summary>
        /// builds the swept volume from the survey, starting the trajectory at the given wellhead
        /// </summary>
        /// <param name="stations">at least two valid stations, in order of measured depth</param>
        /// <param name="wellheadNorth"></param>
        /// <param name="wellheadEast"></param>
        /// <param name="wellheadVertical"></param>
        /// <param name="sampleStep"></param>
        public WellboreUncertainty(IReadOnlyList<WellboreUncertaintyStation> stations,
                                   double wellheadNorth = 0, double wellheadEast = 0,
                                   double wellheadVertical = 0, double sampleStep = 2.0)
        {
            if (stations == null)
            {
                throw new ArgumentNullException(nameof(stations));
            }
            if (stations.Count < 2)
            {
                throw new ArgumentException("at least two stations are needed", nameof(stations));
            }
            if (!(sampleStep > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sampleStep));
            }
            SampleStep = sampleStep;

            // the trajectory at the survey stations, by the shared circular arc construction
            int count = stations.Count;
            double[] stationNorth = new double[count];
            double[] stationEast = new double[count];
            double[] stationVertical = new double[count];
            double[] stationCurvature = new double[count];
            stationNorth[0] = wellheadNorth;
            stationEast[0] = wellheadEast;
            stationVertical[0] = wellheadVertical;
            TrajectoryPoint3D previous = new TrajectoryPoint3D(wellheadNorth, wellheadEast, wellheadVertical,
                                                               stations[0].MeasuredDepth,
                                                               stations[0].Inclination, stations[0].Azimuth);
            for (int s = 1; s < count; s++)
            {
                TrajectoryPoint3D next = new TrajectoryPoint3D
                {
                    Abscissa = stations[s].MeasuredDepth,
                    Inclination = stations[s].Inclination,
                    Azimuth = stations[s].Azimuth
                };
                if (!previous.CompleteCASIA(next))
                {
                    throw new ArgumentException($"station {s} could not be placed", nameof(stations));
                }
                stationNorth[s] = next.X!.Value;
                stationEast[s] = next.Y!.Value;
                stationVertical[s] = next.Z!.Value;
                stationCurvature[s] = next.Curvature ?? 0;
                previous = next;
            }

            // resample everything at a fixed step of measured depth
            double total = stations[count - 1].MeasuredDepth - stations[0].MeasuredDepth;
            int samples = System.Math.Max(2, (int)System.Math.Ceiling(total / sampleStep) + 1);
            north_ = new double[samples];
            east_ = new double[samples];
            vertical_ = new double[samples];
            measuredDepth_ = new double[samples];
            semiMajor_ = new double[samples];
            semiMinor_ = new double[samples];
            frame_ = new double[9 * samples];
            angleCosine_ = new double[samples];
            angleSine_ = new double[samples];

            double largestAxis = 0;
            double largestRatio = 0;
            for (int a = 0; a < 3; a++)
            {
                BoundingBoxMinimum[a] = double.MaxValue;
                BoundingBoxMaximum[a] = double.MinValue;
            }

            int segment = 0;
            for (int s = 0; s < samples; s++)
            {
                double md = stations[0].MeasuredDepth + total * s / (samples - 1);
                while (segment < count - 2 && stations[segment + 1].MeasuredDepth < md)
                {
                    segment++;
                }
                WellboreUncertaintyStation from = stations[segment];
                WellboreUncertaintyStation to = stations[segment + 1];
                double span = to.MeasuredDepth - from.MeasuredDepth;
                double t = span > 0 ? (md - from.MeasuredDepth) / span : 0;
                if (t < 0) { t = 0; } else if (t > 1) { t = 1; }

                // the position comes from the arc between the two bracketing stations, not from a straight
                // interpolation between them, so the sample sits on the trajectory and not on its chord
                TrajectoryPoint3D anchor = new TrajectoryPoint3D(stationNorth[segment], stationEast[segment],
                                                                 stationVertical[segment],
                                                                 from.MeasuredDepth, from.Inclination,
                                                                 from.Azimuth);
                double inclination = from.Inclination + t * (to.Inclination - from.Inclination);
                double azimuth = from.Azimuth + t * AngleDifference(to.Azimuth, from.Azimuth);
                TrajectoryPoint3D here = new TrajectoryPoint3D
                {
                    Abscissa = md,
                    Inclination = inclination,
                    Azimuth = azimuth
                };
                anchor.CompleteCASIA(here);

                north_[s] = here.X!.Value;
                east_[s] = here.Y!.Value;
                vertical_[s] = here.Z!.Value;
                measuredDepth_[s] = md;
                semiMajor_[s] = from.SemiMajorAxis + t * (to.SemiMajorAxis - from.SemiMajorAxis);
                semiMinor_[s] = from.SemiMinorAxis + t * (to.SemiMinorAxis - from.SemiMinorAxis);
                double angle = from.SemiMajorAngle + t * AngleDifference(to.SemiMajorAngle, from.SemiMajorAngle);
                angleCosine_[s] = System.Math.Cos(angle);
                angleSine_[s] = System.Math.Sin(angle);
                BuildFrame(inclination, azimuth, frame_, 9 * s);

                if (semiMajor_[s] > largestAxis)
                {
                    largestAxis = semiMajor_[s];
                }
                double curvature = stationCurvature[System.Math.Min(segment + 1, count - 1)];
                double ratio = semiMajor_[s] * curvature;
                if (ratio > largestRatio)
                {
                    largestRatio = ratio;
                }
                Extend(0, north_[s], semiMajor_[s]);
                Extend(1, east_[s], semiMajor_[s]);
                Extend(2, vertical_[s], semiMajor_[s]);
            }
            MaximumSemiAxis = largestAxis;
            MaximumSweepRatio = largestRatio;
        }

        private void Extend(int axis, double value, double margin)
        {
            if (value - margin < BoundingBoxMinimum[axis])
            {
                BoundingBoxMinimum[axis] = value - margin;
            }
            if (value + margin > BoundingBoxMaximum[axis])
            {
                BoundingBoxMaximum[axis] = value + margin;
            }
        }

        /// <summary>
        /// the difference of two angles, brought into the half turn either side of zero
        /// </summary>
        private static double AngleDifference(double to, double from)
        {
            double difference = to - from;
            while (difference > System.Math.PI) { difference -= 2.0 * System.Math.PI; }
            while (difference < -System.Math.PI) { difference += 2.0 * System.Math.PI; }
            return difference;
        }

        /// <summary>
        /// The smallest inclination at which the high side is used as the reference for the angle of the
        /// semi-major axis, rad.
        /// <para>
        /// The convention is the one toolfaces follow: the angle is measured from the high side while the
        /// hole is inclined enough for the high side to mean anything, and from true north below that.
        /// Three degrees is the usual place to change over.
        /// </para>
        /// </summary>
        public static double HighSideInclinationThreshold { get; set; } = 3.0 * System.Math.PI / 180.0;

        /// <summary>
        /// the tangent, the reference direction for the angle, and the direction a quarter turn from it
        /// </summary>
        private static void BuildFrame(double inclination, double azimuth, double[] frame, int at)
        {
            double si = System.Math.Sin(inclination);
            double ci = System.Math.Cos(inclination);
            double sa = System.Math.Sin(azimuth);
            double ca = System.Math.Cos(azimuth);
            // tangent, with the vertical positive downward
            frame[at] = si * ca;
            frame[at + 1] = si * sa;
            frame[at + 2] = ci;
            // The high side points against the vertical, and is the reference while the hole is inclined
            // enough for it to be defined. Below that the reference becomes true north projected into the
            // plane perpendicular to the borehole, which is the same changeover a toolface makes between
            // its gravity and its magnetic form.
            if (System.Math.Abs(si) > System.Math.Sin(HighSideInclinationThreshold))
            {
                frame[at + 3] = ci * ca;
                frame[at + 4] = ci * sa;
                frame[at + 5] = -si;
            }
            else
            {
                // north, made perpendicular to the tangent and brought back to unit length; taking north
                // itself would leave the frame skewed rather than orthonormal on a hole that is nearly,
                // but not exactly, vertical
                double along = frame[at];
                double nn = 1.0 - along * frame[at];
                double ne = -along * frame[at + 1];
                double nv = -along * frame[at + 2];
                double length = System.Math.Sqrt(nn * nn + ne * ne + nv * nv);
                if (length > 1e-12)
                {
                    frame[at + 3] = nn / length;
                    frame[at + 4] = ne / length;
                    frame[at + 5] = nv / length;
                }
                else
                {
                    frame[at + 3] = 0;
                    frame[at + 4] = 1;
                    frame[at + 5] = 0;
                }
            }
            // the right hand side completes the frame
            frame[at + 6] = frame[at + 1] * frame[at + 5] - frame[at + 2] * frame[at + 4];
            frame[at + 7] = frame[at + 2] * frame[at + 3] - frame[at] * frame[at + 5];
            frame[at + 8] = frame[at] * frame[at + 4] - frame[at + 1] * frame[at + 3];
        }

        /// <summary>
        /// the number of samples along the trajectory
        /// </summary>
        public int SampleCount
        {
            get
            {
                return north_.Length;
            }
        }

        /// <summary>
        /// the position of one sample
        /// </summary>
        public void GetSample(int sample, out double north, out double east, out double vertical)
        {
            north = north_[sample];
            east = east_[sample];
            vertical = vertical_[sample];
        }

        /// <summary>
        /// The direction of the trajectory at one sample, as a unit vector.
        /// <para>
        /// This is what a sidetrack leaves along: the departure direction of a tie-in is the parent's own
        /// direction at the window, there being no way to turn at a point.
        /// </para>
        /// </summary>
        public void GetTangent(int sample, out double north, out double east, out double vertical)
        {
            int at = 9 * sample;
            north = frame_[at];
            east = frame_[at + 1];
            vertical = frame_[at + 2];
        }

        /// <summary>
        /// the measured depth of one sample
        /// </summary>
        public double GetMeasuredDepth(int sample)
        {
            return measuredDepth_[sample];
        }

        /// <summary>
        /// the semi-minor axis of the section at one sample, m. The scale over which the shape of the
        /// solid varies, and therefore how finely its surface is worth resolving.
        /// </summary>
        public double GetSemiMinorAxis(int sample)
        {
            return semiMinor_[sample];
        }

        /// <summary>
        /// A point on the surface of the volume, on the section at the given sample.
        /// <para>
        /// The parameter runs round the ellipse rather than being a true angle, so equal steps in it do
        /// not give equal steps of arc, which is of no consequence for drawing a closed ring.
        /// </para>
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="parameter">nought to two pi</param>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        public void GetSectionPoint(int sample, double parameter,
                                    out double north, out double east, out double vertical)
        {
            double p = semiMajor_[sample] * System.Math.Cos(parameter);
            double q = semiMinor_[sample] * System.Math.Sin(parameter);
            // out of the axes of the ellipse and back into the frame of the borehole
            double cosine = angleCosine_[sample];
            double sine = angleSine_[sample];
            double u = p * cosine - q * sine;
            double v = p * sine + q * cosine;
            int at = 9 * sample;
            north = north_[sample] + u * frame_[at + 3] + v * frame_[at + 6];
            east = east_[sample] + u * frame_[at + 4] + v * frame_[at + 7];
            vertical = vertical_[sample] + u * frame_[at + 5] + v * frame_[at + 8];
        }

        /// <summary>
        /// whether the given sample has been muted and so is not an obstacle
        /// </summary>
        public bool IsMuted(int sample)
        {
            if (MutedFrom == null || MutedTo == null)
            {
                return false;
            }
            double md = measuredDepth_[sample];
            return md >= MutedFrom.Value && md <= MutedTo.Value;
        }

        /// <summary>
        /// How far the given position lies outside the surface of the volume, measured at the given
        /// sample. Negative inside, zero on the surface, positive outside.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="vertical"></param>
        /// <returns>positive infinity when the sample is muted</returns>
        public double GetRadialExcess(int sample, double north, double east, double vertical)
        {
            if (IsMuted(sample))
            {
                return double.PositiveInfinity;
            }
            double dn = north - north_[sample];
            double de = east - east_[sample];
            double dv = vertical - vertical_[sample];
            int at = 9 * sample;
            double along = dn * frame_[at] + de * frame_[at + 1] + dv * frame_[at + 2];
            double u = dn * frame_[at + 3] + de * frame_[at + 4] + dv * frame_[at + 5];
            double v = dn * frame_[at + 6] + de * frame_[at + 7] + dv * frame_[at + 8];

            // into the axes of the ellipse
            double cosine = angleCosine_[sample];
            double sine = angleSine_[sample];
            double p = u * cosine + v * sine;
            double q = -u * sine + v * cosine;

            double a = semiMajor_[sample];
            double b = semiMinor_[sample];
            double radius = System.Math.Sqrt(p * p + q * q);
            double excess;
            if (radius <= 0)
            {
                excess = -System.Math.Min(a, b);
            }
            else
            {
                // the ellipse radius in the direction of the position
                double scale = System.Math.Sqrt((p / a) * (p / a) + (q / b) * (q / b));
                excess = scale > 0 ? radius * (1.0 - 1.0 / scale) : -System.Math.Min(a, b);
            }

            // a position beyond the end of the tube, which is what a muted interval leaves behind, is
            // clear of it even when it lines up with the section
            double beyond = System.Math.Abs(along) - 0.5 * SampleStep;
            if (beyond <= 0)
            {
                return excess;
            }
            if (excess <= 0)
            {
                return beyond;
            }
            return System.Math.Sqrt(excess * excess + beyond * beyond);
        }
    }
}
