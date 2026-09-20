using OSDC.DotnetLibraries.Drilling.Streamlines;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// Synthetic streamline sets with a known bundling, used by the tests.
    /// <para>
    /// They all flow downwards, from z = 0 to z = -2000, and none of them crosses another one. The
    /// lateral position of a streamline is given as a function of z, so a fixture is described by what
    /// its cross-sections look like at every depth.
    /// </para>
    /// </summary>
    internal static class StreamlineFixtures
    {
        public const double TopDepth = 0.0;
        public const double BottomDepth = -2000.0;
        public const int DefaultSampleCount = 200;

        /// <summary>
        /// builds one streamline by sampling its lateral position at regular depths
        /// </summary>
        public static Streamline FromLateralProfile(Func<double, (double X, double Y)> lateral,
                                                    int samples = DefaultSampleCount,
                                                    double top = TopDepth, double bottom = BottomDepth)
        {
            List<Point3D> positions = new List<Point3D>(samples + 1);
            for (int i = 0; i <= samples; i++)
            {
                double z = top + (bottom - top) * i / samples;
                (double x, double y) = lateral(z);
                positions.Add(new Point3D(x, y, z));
            }
            return new Streamline(positions);
        }

        /// <summary>
        /// a regular nx by ny raft of parallel vertical streamlines. One bundle.
        /// </summary>
        public static List<Streamline> UniformRaft(int nx, int ny, double spacing,
                                                   double originX = 0, double originY = 0)
        {
            List<Streamline> streamlines = new List<Streamline>(nx * ny);
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    double x = originX + i * spacing;
                    double y = originY + j * spacing;
                    streamlines.Add(FromLateralProfile(z => (x, y)));
                }
            }
            return streamlines;
        }

        /// <summary>
        /// a regular raft that parts into two halves below an obstruction. The two halves keep their own
        /// internal spacing, so the only thing that distinguishes the parting from the ordinary spacing
        /// is that the gap between the halves becomes much wider than that spacing. Two bundles.
        /// </summary>
        public static List<Streamline> RaftSplitByObstruction(int nx, int ny, double spacing,
                                                              double halfOffset = 60.0,
                                                              double rampTop = -800.0,
                                                              double rampBottom = -1200.0)
        {
            List<Streamline> streamlines = new List<Streamline>(nx * ny);
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    double x = i * spacing;
                    double y = j * spacing;
                    double side = i < nx / 2 ? -halfOffset : halfOffset;
                    streamlines.Add(FromLateralProfile(z => (x + side * Ramp(z, rampTop, rampBottom), y)));
                }
            }
            return streamlines;
        }

        /// <summary>
        /// a raft whose left half is four times more densely populated than its right half, with no
        /// obstruction anywhere. Reproduces what a locally refined grid, or a tight cluster of sources,
        /// does to the density of the streamlines. One bundle.
        /// </summary>
        public static List<Streamline> RaftWithDensityContrast(int ny = 4,
                                                               double fineSpacing = 5.0,
                                                               double coarseSpacing = 20.0,
                                                               int fineColumns = 8,
                                                               int coarseColumns = 8)
        {
            List<Streamline> streamlines = new List<Streamline>();
            double x = 0;
            for (int i = 0; i < fineColumns; i++)
            {
                double columnX = x;
                for (int j = 0; j < ny; j++)
                {
                    double y = j * coarseSpacing;
                    streamlines.Add(FromLateralProfile(z => (columnX, y)));
                }
                x += fineSpacing;
            }
            x += coarseSpacing - fineSpacing;
            for (int i = 0; i < coarseColumns; i++)
            {
                double columnX = x;
                for (int j = 0; j < ny; j++)
                {
                    double y = j * coarseSpacing;
                    streamlines.Add(FromLateralProfile(z => (columnX, y)));
                }
                x += coarseSpacing;
            }
            return streamlines;
        }

        /// <summary>
        /// every streamline leaves the same position at the top and fans out towards one of
        /// <paramref name="targetCount"/> widely separated targets, in a tight group per target.
        /// As many bundles as targets.
        /// </summary>
        public static List<Streamline> ClusterFanningOut(int targetCount, int perTarget,
                                                         double targetRadius = 1500.0,
                                                         double groupRadius = 40.0)
        {
            List<Streamline> streamlines = new List<Streamline>(targetCount * perTarget);
            for (int t = 0; t < targetCount; t++)
            {
                double angle = 2.0 * System.Math.PI * t / targetCount;
                double tx = targetRadius * System.Math.Cos(angle);
                double ty = targetRadius * System.Math.Sin(angle);
                int side = (int)System.Math.Ceiling(System.Math.Sqrt(perTarget));
                for (int m = 0; m < perTarget; m++)
                {
                    double ox = (m % side - (side - 1) * 0.5) * groupRadius;
                    double oy = (m / side - (side - 1) * 0.5) * groupRadius;
                    double endX = tx + ox;
                    double endY = ty + oy;
                    streamlines.Add(FromLateralProfile(z =>
                    {
                        double f = Ramp(z, TopDepth, BottomDepth);
                        return (endX * f, endY * f);
                    }));
                }
            }
            return streamlines;
        }

        /// <summary>
        /// inserts the midpoint of every segment, which refines the sampling without changing the
        /// geometry of any streamline
        /// </summary>
        public static List<Streamline> Densify(IReadOnlyList<Streamline> streamlines)
        {
            List<Streamline> densified = new List<Streamline>(streamlines.Count);
            foreach (Streamline streamline in streamlines)
            {
                List<Point3D> source = streamline.Positions!;
                List<Point3D> positions = new List<Point3D>(2 * source.Count);
                for (int i = 0; i < source.Count; i++)
                {
                    if (i > 0)
                    {
                        positions.Add(new Point3D(0.5 * (source[i - 1].X!.Value + source[i].X!.Value),
                                                  0.5 * (source[i - 1].Y!.Value + source[i].Y!.Value),
                                                  0.5 * (source[i - 1].Z!.Value + source[i].Z!.Value)));
                    }
                    positions.Add(new Point3D(source[i]));
                }
                densified.Add(new Streamline(positions));
            }
            return densified;
        }

        /// <summary>
        /// multiplies every coordinate by the given factor
        /// </summary>
        public static List<Streamline> Scale(IReadOnlyList<Streamline> streamlines, double factor)
        {
            List<Streamline> scaled = new List<Streamline>(streamlines.Count);
            foreach (Streamline streamline in streamlines)
            {
                List<Point3D> positions = new List<Point3D>(streamline.Positions!.Count);
                foreach (Point3D pt in streamline.Positions!)
                {
                    positions.Add(new Point3D(pt.X!.Value * factor, pt.Y!.Value * factor, pt.Z!.Value * factor));
                }
                scaled.Add(new Streamline(positions));
            }
            return scaled;
        }

        /// <summary>
        /// 0 above <paramref name="top"/>, 1 below <paramref name="bottom"/>, linear in between
        /// </summary>
        private static double Ramp(double z, double top, double bottom)
        {
            if (z >= top)
            {
                return 0.0;
            }
            if (z <= bottom)
            {
                return 1.0;
            }
            return (top - z) / (top - bottom);
        }

        /// <summary>
        /// the bundling as a canonical set of sets of streamline indices, so that two results can be
        /// compared without depending on the order of the bundles
        /// </summary>
        public static List<List<int>> Canonical(StreamlineBundlingResult result)
        {
            List<List<int>> canonical = new List<List<int>>();
            foreach (StreamlineBundle bundle in result.Bundles)
            {
                List<int> members = new List<int>(bundle.StreamlineIndices);
                members.Sort();
                canonical.Add(members);
            }
            canonical.Sort((left, right) =>
            {
                int compare = left[0].CompareTo(right[0]);
                return compare != 0 ? compare : left.Count.CompareTo(right.Count);
            });
            return canonical;
        }

        /// <summary>
        /// formats a bundling for an assertion message
        /// </summary>
        public static string Describe(StreamlineBundlingResult result)
        {
            List<string> sizes = new List<string>();
            foreach (StreamlineBundle bundle in result.Bundles)
            {
                sizes.Add(bundle.Count.ToString());
            }
            return $"{result.Bundles.Count} bundles of sizes [{string.Join(", ", sizes)}]" +
                   $" from {result.CrossingCount} crossings over {result.EffectiveCrossSectionCount}" +
                   $" cross-sections, spacing {result.SweepPlaneSpacing:G4}," +
                   $" {result.AdjacentPairCount} adjacent pairs of which {result.SeparatedPairCount} parted";
        }
    }
}
