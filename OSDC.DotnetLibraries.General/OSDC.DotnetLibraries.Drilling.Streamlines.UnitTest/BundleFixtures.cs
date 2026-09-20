using OSDC.DotnetLibraries.Drilling.Streamlines;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// Bundles whose factory is known in advance, used by the factory tests.
    /// <para>
    /// All of them are tubes running from z = 0 down to z = -2000 around a known axis, filled by a
    /// Vogel spiral so that the cross-section is evenly covered without any randomness: streamline i of n
    /// sits at radius proportional to the square root of i over n and at i times the golden angle.
    /// </para>
    /// </summary>
    internal static class BundleFixtures
    {
        public const double TopDepth = 0.0;
        public const double BottomDepth = -2000.0;
        public const int DefaultSampleCount = 200;

        private static readonly double GoldenAngle = System.Math.PI * (3.0 - System.Math.Sqrt(5.0));

        /// <summary>
        /// where streamline i of n sits in the cross-section of the tube, in polar coordinates
        /// </summary>
        public static void GetSpiralPosition(int index, int count, double radius,
                                             out double r, out double theta)
        {
            r = radius * System.Math.Sqrt((index + 0.5) / count);
            theta = index * GoldenAngle;
        }

        /// <summary>
        /// builds a tube of streamlines. The axis, the radius of the cross-section and how the
        /// cross-section is turned are all functions of the fraction of the way down.
        /// </summary>
        public static List<Streamline> Tube(int count, int samples,
                                            Func<double, (double X, double Y, double Z)> axis,
                                            Func<double, double> radius,
                                            Func<double, double>? swirl = null,
                                            Func<int, int, (double R, double Theta)>? placement = null)
        {
            List<Streamline> streamlines = new List<Streamline>(count);
            for (int i = 0; i < count; i++)
            {
                double r0, theta0;
                if (placement != null)
                {
                    (r0, theta0) = placement(i, count);
                }
                else
                {
                    GetSpiralPosition(i, count, 1.0, out r0, out theta0);
                }
                List<Point3D> positions = new List<Point3D>(samples + 1);
                for (int s = 0; s <= samples; s++)
                {
                    double t = (double)s / samples;
                    (double ax, double ay, double az) = axis(t);
                    double scale = radius(t);
                    double turn = theta0 + (swirl == null ? 0.0 : swirl(t));
                    positions.Add(new Point3D(ax + scale * r0 * System.Math.Cos(turn),
                                              ay + scale * r0 * System.Math.Sin(turn),
                                              az));
                }
                streamlines.Add(new Streamline(positions));
            }
            return streamlines;
        }

        /// <summary>
        /// a straight vertical tube of constant radius. Its factory should be exact.
        /// </summary>
        public static List<Streamline> StraightTube(int count = 200, double radius = 40.0,
                                                    int samples = DefaultSampleCount)
        {
            return Tube(count, samples,
                        t => (0.0, 0.0, TopDepth + (BottomDepth - TopDepth) * t),
                        t => radius);
        }

        /// <summary>
        /// a straight vertical tube whose cross-section grows steadily. An affine transform per
        /// cross-section reproduces it exactly.
        /// </summary>
        public static List<Streamline> ExpandingTube(int count = 200, double nearRadius = 10.0,
                                                     double farRadius = 60.0,
                                                     int samples = DefaultSampleCount)
        {
            return Tube(count, samples,
                        t => (0.0, 0.0, TopDepth + (BottomDepth - TopDepth) * t),
                        t => nearRadius + (farRadius - nearRadius) * t);
        }

        /// <summary>
        /// a straight vertical tube whose cross-section turns as it descends. Reproduced exactly by an
        /// affine transform, but only if the frame carried along the median curve does not itself spin.
        /// </summary>
        public static List<Streamline> SwirlingTube(int count = 200, double radius = 40.0,
                                                    double turns = 1.5,
                                                    int samples = DefaultSampleCount)
        {
            return Tube(count, samples,
                        t => (0.0, 0.0, TopDepth + (BottomDepth - TopDepth) * t),
                        t => radius,
                        t => 2.0 * System.Math.PI * turns * t);
        }

        /// <summary>
        /// A tube whose axis follows a circular arc in the x z plane, of the given radius of curvature.
        /// <para>
        /// The offsets are taken in the plane perpendicular to the axis, not horizontally. Offsetting
        /// horizontally would look right while the axis is near vertical and stop being a tube at all once
        /// the axis has turned: at the bottom of a 2000 m arc of 1500 m radius the axis is 76 degrees off
        /// vertical, and a horizontal offset is then very nearly along it.
        /// </para>
        /// </summary>
        public static List<Streamline> CurvedTube(int count = 200, double radius = 40.0,
                                                  double curvatureRadius = 1500.0,
                                                  int samples = DefaultSampleCount)
        {
            double sweep = (TopDepth - BottomDepth) / curvatureRadius;
            List<Streamline> streamlines = new List<Streamline>(count);
            for (int i = 0; i < count; i++)
            {
                GetSpiralPosition(i, count, radius, out double r0, out double theta0);
                double offsetInPlane = r0 * System.Math.Cos(theta0);
                double offsetSideways = r0 * System.Math.Sin(theta0);
                List<Point3D> positions = new List<Point3D>(samples + 1);
                for (int s = 0; s <= samples; s++)
                {
                    double angle = sweep * s / samples;
                    // the axis, and the two directions perpendicular to it
                    double ax = curvatureRadius * (1.0 - System.Math.Cos(angle));
                    double az = -curvatureRadius * System.Math.Sin(angle);
                    double nx = System.Math.Cos(angle);
                    double nz = System.Math.Sin(angle);
                    positions.Add(new Point3D(ax + offsetInPlane * nx,
                                              offsetSideways,
                                              az + offsetInPlane * nz));
                }
                streamlines.Add(new Streamline(positions));
            }
            return streamlines;
        }

        /// <summary>
        /// a straight tube whose streamlines all sit in a ring, leaving the middle empty. Nothing about an
        /// affine transform can express that: only the density can.
        /// </summary>
        public static List<Streamline> AnnularTube(int count = 200, double innerRadius = 30.0,
                                                   double outerRadius = 40.0,
                                                   int samples = DefaultSampleCount)
        {
            return Tube(count, samples,
                        t => (0.0, 0.0, TopDepth + (BottomDepth - TopDepth) * t),
                        t => 1.0,
                        null,
                        (index, total) =>
                        {
                            double fraction = (index + 0.5) / total;
                            double r = System.Math.Sqrt(innerRadius * innerRadius
                                       + fraction * (outerRadius * outerRadius - innerRadius * innerRadius));
                            return (r, index * GoldenAngle);
                        });
        }

        /// <summary>
        /// the largest distance from any position of the streamline to the vertical axis
        /// </summary>
        public static double GetMaximumAxialDistance(Streamline streamline)
        {
            double largest = 0;
            foreach (Point3D pt in streamline.Positions!)
            {
                double r = System.Math.Sqrt(pt.X!.Value * pt.X.Value + pt.Y!.Value * pt.Y.Value);
                if (r > largest)
                {
                    largest = r;
                }
            }
            return largest;
        }

        /// <summary>
        /// the smallest distance from any position of the streamline to the vertical axis
        /// </summary>
        public static double GetMinimumAxialDistance(Streamline streamline)
        {
            double smallest = double.MaxValue;
            foreach (Point3D pt in streamline.Positions!)
            {
                double r = System.Math.Sqrt(pt.X!.Value * pt.X.Value + pt.Y!.Value * pt.Y.Value);
                if (r < smallest)
                {
                    smallest = r;
                }
            }
            return smallest;
        }

        /// <summary>
        /// how far the positions of <paramref name="candidate"/>, taken every
        /// <paramref name="stride"/>, are from the nearest position of <paramref name="reference"/>
        /// </summary>
        public static double GetLargestDeviation(Streamline candidate, Streamline reference, int stride = 16)
        {
            List<Point3D> from = candidate.Positions!;
            List<Point3D> to = reference.Positions!;
            double largest = 0;
            for (int i = 0; i < from.Count; i += stride)
            {
                double nearest = double.MaxValue;
                for (int j = 0; j < to.Count; j++)
                {
                    double dx = from[i].X!.Value - to[j].X!.Value;
                    double dy = from[i].Y!.Value - to[j].Y!.Value;
                    double dz = from[i].Z!.Value - to[j].Z!.Value;
                    double distance = dx * dx + dy * dy + dz * dz;
                    if (distance < nearest)
                    {
                        nearest = distance;
                    }
                }
                nearest = System.Math.Sqrt(nearest);
                if (nearest > largest)
                {
                    largest = nearest;
                }
            }
            return largest;
        }

        /// <summary>
        /// formats a factory for an assertion message
        /// </summary>
        public static string Describe(StreamlineBundleFactory factory)
        {
            return $"{factory.Stations.Count} cross-sections over {factory.Length:G4}," +
                   $" spacing {factory.StationSpacing:G4}," +
                   $" tangent deviation {factory.TangentDeviation:G3} rad against a bundle spread of" +
                   $" {factory.BundleTangentSpread:G3}," +
                   $" residual {factory.ResidualDeviation:G3} ({100.0 * factory.ResidualFraction:G3} %)," +
                   $" median moved {100.0 * factory.MedianDisplacementFraction:G3} %," +
                   $" tube ratio {factory.MaximumTubeRatio:G3}," +
                   $" {factory.Density?.CellCount ?? 0} density cells," +
                   $" {factory.GetFootprint()} bytes";
        }
    }
}
