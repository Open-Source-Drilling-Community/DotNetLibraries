using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines
{
    /// <summary>
    /// A lightweight surrogate for a bundle of streamlines, able to produce as many streamlines of that
    /// bundle as are wanted without holding any of the originals.
    /// <para>
    /// It is made of a median curve, a series of cross-sections perpendicular to that curve at regular
    /// intervals, a transform per cross-section saying how the bundle is laid out there, and a single
    /// probability density over a normalized cross-section. A streamline is produced by drawing one polar
    /// position from the density and carrying it through every cross-section: the draw is therefore two
    /// numbers, and the whole of the streamline follows from them.
    /// </para>
    /// <para>
    /// The production is deterministic in those two numbers, so the streamlines it makes are smoother
    /// than the ones it was built from. What was left out is measured rather than discarded: see
    /// <see cref="ResidualDeviation"/> and <see cref="ResidualCorrelationLength"/>.
    /// </para>
    /// </summary>
    public class StreamlineBundleFactory
    {
        /// <summary>
        /// the cross-sections, in order along the median curve, at a regular
        /// <see cref="StationSpacing"/> apart
        /// </summary>
        public List<CrossSectionStation> Stations { get; internal set; } = new List<CrossSectionStation>();

        /// <summary>
        /// where the streamlines are in a normalized cross-section
        /// </summary>
        public PolarDensity? Density { get; internal set; } = null;

        /// <summary>
        /// The normalized radius the outline stands at, which is one by construction: a draw of that
        /// radius lands on the boundary of the cross-section and anything less lands inside it.
        /// </summary>
        public double NormalizedRadius { get; internal set; } = 0;

        /// <summary>
        /// the distance between two consecutive cross-sections
        /// </summary>
        public double StationSpacing { get; internal set; } = 0;

        /// <summary>
        /// the length of the median curve
        /// </summary>
        public double Length { get; internal set; } = 0;

        /// <summary>
        /// how many streamlines the factory was built from
        /// </summary>
        public int SourceStreamlineCount { get; internal set; } = 0;

        // ---- how faithful the surrogate is ------------------------------------------------------

        /// <summary>
        /// the average angle, in radians, between the tangent of the median curve and the tangents of the
        /// streamlines it stands for. How well the median curve took on the shape of the bundle.
        /// </summary>
        public double TangentDeviation { get; internal set; } = 0;

        /// <summary>
        /// the same angle, averaged over the streamlines of the bundle taken against one another rather
        /// than against the median. The median curve is doing as well as it can be asked to when
        /// <see cref="TangentDeviation"/> is no larger than this.
        /// </summary>
        public double BundleTangentSpread { get; internal set; } = 0;

        /// <summary>
        /// how far, on average, the streamlines of the bundle sit from where the factory would put them.
        /// The part of the bundle the surrogate does not reproduce.
        /// </summary>
        public double ResidualDeviation { get; internal set; } = 0;

        /// <summary>
        /// <see cref="ResidualDeviation"/> as a fraction of the radius of the bundle
        /// </summary>
        public double ResidualFraction { get; internal set; } = 0;

        /// <summary>
        /// the distance along the median curve over which what the surrogate leaves out stays correlated.
        /// A streamline drawn with that roughness put back would need it applied as a process of this
        /// correlation length.
        /// </summary>
        public double ResidualCorrelationLength { get; internal set; } = 0;

        /// <summary>
        /// how far the median curve had to be moved away from the geometric median of the bundle in order
        /// to be smooth, as a fraction of the radius of the bundle
        /// </summary>
        public double MedianDisplacementFraction { get; internal set; } = 0;

        /// <summary>
        /// the largest product of the outer reach of a cross-section and the curvature of the median
        /// curve there.
        /// <para>
        /// The cross-sections describe the bundle faithfully only while this stays below one. Past it the
        /// planes of neighbouring cross-sections cut into one another on the inside of the bend, the
        /// mapping from a polar position to a point in space stops being one to one, and the streamlines
        /// produced can cross one another even though the transform of every cross-section is injective.
        /// </para>
        /// </summary>
        public double MaximumTubeRatio { get; internal set; } = 0;

        /// <summary>
        /// whether <see cref="MaximumTubeRatio"/> stays below one everywhere
        /// </summary>
        public bool IsTubeValid
        {
            get
            {
                return MaximumTubeRatio < 1.0;
            }
        }

        /// <summary>
        /// the outline of every cross-section, in station order. Held here as well as on the stations,
        /// because the builder fits them as one block.
        /// </summary>
        internal CrossSectionPolygon?[]? Outlines { get; set; } = null;

        /// <summary>
        /// how far the bundle has turned about the median at every cross-section, rad
        /// </summary>
        internal double[]? Twists { get; set; } = null;

        /// <summary>
        /// the residual of every cross-section
        /// </summary>
        internal double[]? StationResidual { get; set; } = null;

        /// <summary>
        /// the radius of the bundle at every cross-section
        /// </summary>
        internal double[]? StationRadius { get; set; } = null;

        /// <summary>
        /// The stretch every streamline of the bundle began with in common, held apart from the fit and
        /// put back on the front of everything the factory produces.
        /// <para>
        /// A shared head carries no variability, so it tells the fit nothing and breaks it: the crossings
        /// there are one point, the cross-section has no extent, and what comes back is a transform of
        /// zero that quietly ignores the draw. It is not an oddity of one caller either. A well leaves
        /// its slot down a conduit, and a sidetrack leaves along its parent, so the paths of a bundle
        /// genuinely do begin identically and the factory has to say so rather than fail on it.
        /// </para>
        /// </summary>
        public List<Point3D> SharedHead { get; internal set; } = new List<Point3D>();

        /// <summary>
        /// How many of the streamlines the factory was built from are not represented by its tolerance
        /// region: they spend more of their length outside it than the builder allows.
        /// <para>
        /// A tolerance region is convex, so a bundle whose paths pass either side of an obstruction has no
        /// region that holds them all without holding the obstruction too. Members left outside are
        /// therefore a statement that the bundle was more than one corridor, and are what
        /// <see cref="StreamlineFactorySetBuilder"/> splits on.
        /// </para>
        /// </summary>
        public int OutsideMemberCount { get; internal set; } = 0;

        /// <summary>
        /// The least room a well has around the planned path anywhere along this corridor, m: the
        /// smallest inradius of its tolerance regions.
        /// <para>
        /// The least and not the average, because a corridor thirty metres wide that is throttled to two
        /// somewhere is undrillable where it is throttled. The ends are left out of it: the paths start
        /// together at the slot and converge on the target, so both are pinched by construction and
        /// neither says anything about the room in between.
        /// </para>
        /// </summary>
        public double LeastRoom { get; internal set; } = 0;

        /// <summary>
        /// The sharpest curvature of the median, rad/m, read over a thirty metre station. The median is
        /// the path that would be planned, so this is what it would cost to drill the plan.
        /// <para>
        /// A curvature and not a dogleg severity: the turn measured over the station is divided by the
        /// station's length, so the number is a property of the curve rather than of the interval it was
        /// sampled on. A caller wanting degrees per thirty metres converts at its own boundary.
        /// </para>
        /// </summary>
        public double MedianCurvature { get; internal set; } = 0;

        /// <summary>
        /// The same curvature read over a hundred and twenty metre station instead, rad/m.
        /// <para>
        /// Worth having beside <see cref="MedianCurvature"/> rather than instead of it. A real curvature
        /// reads the same however long the window it is measured over, while a corner is a fixed angle
        /// and so reads as a curvature falling with one over the window. The two together say which of
        /// the two a number is.
        /// </para>
        /// </summary>
        public double MedianCurvatureLong { get; internal set; } = 0;

        /// <summary>
        /// the median curve, as the ordered positions of the cross-sections
        /// </summary>
        /// <returns></returns>
        public Streamline GetMedianCurve()
        {
            List<Point3D> positions = new List<Point3D>(Stations.Count);
            foreach (CrossSectionStation station in Stations)
            {
                if (station.Position != null)
                {
                    positions.Add(new Point3D(station.Position));
                }
            }
            return new Streamline(positions) { Name = "median" };
        }

        /// <summary>
        /// produces the streamline that passes through the given normalized polar position. The same
        /// position always gives the same streamline.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        public Streamline Generate(double r, double theta)
        {
            List<Point3D> positions = new List<Point3D>(Stations.Count + SharedHead.Count);
            // the head is common to every member, so it is common to every realization: a produced
            // streamline that began at the end of it would start where the bundle spread out rather than
            // where the well starts
            positions.AddRange(SharedHead);
            foreach (CrossSectionStation station in Stations)
            {
                Point3D? position = station.GetPosition(r, theta);
                if (position != null)
                {
                    positions.Add(position);
                }
            }
            return new Streamline(positions);
        }

        /// <summary>
        /// draws one streamline. Pass a seeded generator to make the draw repeatable; with null the
        /// shared generator of the library is used, which cannot be seeded.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public Streamline? Draw(Random? random = null)
        {
            if (Density == null || !Density.Draw(random, out double r, out double theta))
            {
                return null;
            }
            return Generate(r, theta);
        }

        /// <summary>
        /// draws the given number of streamlines
        /// </summary>
        /// <param name="count"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public List<Streamline> Draw(int count, Random? random = null)
        {
            List<Streamline> drawn = new List<Streamline>(System.Math.Max(0, count));
            for (int i = 0; i < count; i++)
            {
                Streamline? streamline = Draw(random);
                if (streamline == null)
                {
                    break;
                }
                drawn.Add(streamline);
            }
            return drawn;
        }

        /// <summary>
        /// how much storage the factory occupies, in bytes, counting only the numbers that describe it
        /// </summary>
        public long GetFootprint()
        {
            long stations = (long)Stations.Count * (1 + 3 + 3 + 3 + 3 + 4 + 2) * sizeof(double);
            long density = Density == null ? 0 : (long)Density.CellCount * sizeof(double);
            return stations + density;
        }
    }
}
