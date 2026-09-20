using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// How hard a generated corridor turns, measured the way a survey reports it.
    /// <para>
    /// A streamline has no limit on its curvature: it is an integral curve of a velocity field, and the
    /// field bends as sharply as the medium and the boundaries make it bend. A corridor that turns faster
    /// than the bit can build is therefore a perfectly good streamline and a useless well path, and
    /// nothing else in the chain notices. This is what lets the generation say so.
    /// </para>
    /// <para>
    /// The positions a <see cref="StreamlineTracer"/> records sit one per cell, so the spacing between
    /// them is the octree's cell size rather than anything to do with the path. Taking the angle between
    /// consecutive recorded segments would therefore measure the grid. The curve is resampled at a fixed
    /// station length first, which is also what makes the number comparable with a dogleg severity off a
    /// real survey.
    /// </para>
    /// </summary>
    public static class StreamlineCurvature
    {
        /// <summary>
        /// the station length a dogleg severity is quoted over, m
        /// </summary>
        public const double StandardStation = 30.0;

        /// <summary>
        /// Resamples a streamline at a fixed station length.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="stationLength"></param>
        /// <returns>the stations, starting at the first position</returns>
        public static List<Point3D> Resample(Streamline line, double stationLength = StandardStation)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }
            if (!(stationLength > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(stationLength));
            }
            List<Point3D> stations = new List<Point3D>();
            List<Point3D>? positions = line.Positions;
            if (positions == null || positions.Count < 2)
            {
                return stations;
            }
            stations.Add(positions[0]);
            double carried = 0;
            for (int i = 0; i + 1 < positions.Count; i++)
            {
                double an = positions[i].X!.Value, ae = positions[i].Y!.Value, av = positions[i].Z!.Value;
                double bn = positions[i + 1].X!.Value, be = positions[i + 1].Y!.Value,
                       bv = positions[i + 1].Z!.Value;
                double length = System.Math.Sqrt(Squared(bn - an) + Squared(be - ae) + Squared(bv - av));
                if (!(length > 0))
                {
                    continue;
                }
                double reached = stationLength - carried;
                while (reached <= length)
                {
                    double t = reached / length;
                    stations.Add(new Point3D(an + t * (bn - an), ae + t * (be - ae), av + t * (bv - av)));
                    reached += stationLength;
                }
                carried = (carried + length) % stationLength;
            }
            return stations;
        }

        /// <summary>
        /// The dogleg severity at every station of a streamline, in radians per station length.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="stationLength"></param>
        /// <returns>one value per interior station, so two fewer than the stations</returns>
        public static List<double> GetProfile(Streamline line, double stationLength = StandardStation)
        {
            List<Point3D> stations = Resample(line, stationLength);
            List<double> profile = new List<double>();
            for (int i = 1; i + 1 < stations.Count; i++)
            {
                double un = stations[i].X!.Value - stations[i - 1].X!.Value;
                double ue = stations[i].Y!.Value - stations[i - 1].Y!.Value;
                double uv = stations[i].Z!.Value - stations[i - 1].Z!.Value;
                double vn = stations[i + 1].X!.Value - stations[i].X!.Value;
                double ve = stations[i + 1].Y!.Value - stations[i].Y!.Value;
                double vv = stations[i + 1].Z!.Value - stations[i].Z!.Value;
                double lu = System.Math.Sqrt(un * un + ue * ue + uv * uv);
                double lv = System.Math.Sqrt(vn * vn + ve * ve + vv * vv);
                if (!(lu > 0) || !(lv > 0))
                {
                    continue;
                }
                double cosine = (un * vn + ue * ve + uv * vv) / (lu * lv);
                cosine = System.Math.Max(-1.0, System.Math.Min(1.0, cosine));
                profile.Add(System.Math.Acos(cosine));
            }
            return profile;
        }

        /// <summary>
        /// the sharpest turn anywhere along a streamline, in radians per station length
        /// </summary>
        /// <param name="line"></param>
        /// <param name="stationLength"></param>
        /// <returns></returns>
        public static double GetWorst(Streamline line, double stationLength = StandardStation)
        {
            double worst = 0;
            foreach (double angle in GetProfile(line, stationLength))
            {
                if (angle > worst)
                {
                    worst = angle;
                }
            }
            return worst;
        }

        /// <summary>
        /// The least turning the geometry can possibly ask for, in radians: leaving along one direction,
        /// reaching a point, and arriving along another.
        /// <para>
        /// A path that departs and arrives on given tangents has to turn at least as far as the two
        /// tangents stand from the straight line between the two ends, so a dogleg limit fixes a least
        /// length of hole, <c>turn / limit</c>, before anything at all is known about the medium. This is
        /// what says whether a landing section is long enough to be drilled rather than merely long
        /// enough to be imposed.
        /// </para>
        /// </summary>
        /// <param name="from"></param>
        /// <param name="departure">need not be a unit vector</param>
        /// <param name="to"></param>
        /// <param name="arrival">need not be a unit vector</param>
        /// <returns></returns>
        public static double GetLeastTurn(Point3D from, Vector3D departure, Point3D to, Vector3D arrival)
        {
            double cn = to.X!.Value - from.X!.Value;
            double ce = to.Y!.Value - from.Y!.Value;
            double cv = to.Z!.Value - from.Z!.Value;
            double chord = System.Math.Sqrt(cn * cn + ce * ce + cv * cv);
            if (!(chord > 0))
            {
                return 0;
            }
            return Between(departure, cn / chord, ce / chord, cv / chord)
                   + Between(arrival, cn / chord, ce / chord, cv / chord);
        }

        private static double Between(Vector3D direction, double cn, double ce, double cv)
        {
            double dn = direction.X ?? 0, de = direction.Y ?? 0, dv = direction.Z ?? 0;
            double length = System.Math.Sqrt(dn * dn + de * de + dv * dv);
            if (!(length > 0))
            {
                return 0;
            }
            double cosine = (dn * cn + de * ce + dv * cv) / length;
            return System.Math.Acos(System.Math.Max(-1.0, System.Math.Min(1.0, cosine)));
        }

        private static double Squared(double value)
        {
            return value * value;
        }
    }
}
