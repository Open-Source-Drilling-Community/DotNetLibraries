using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// Pushing a spine clear of the Ullrigg wells without letting it kink.
    /// <para>
    /// No flow is solved here, so these run in a second rather than three minutes, which is what makes it
    /// worth sweeping the relaxation's own settings separately from everything downstream of it.
    /// </para>
    /// </summary>
    [TestFixture]
    public class SpineRelaxationTests
    {
        private const double Ground = -91.2;
        private const double TargetVertical = 900.0;

        private static ObstacleField LoadUllrigg(out double[] wellhead)
        {
            string directory = Path.Combine(TestContext.CurrentContext.TestDirectory,
                                            "UllriggUncertainty");
            Dictionary<string, double[]> heads = new Dictionary<string, double[]>();
            foreach (string line in File.ReadLines(Path.Combine(directory, "wellheads.txt")))
            {
                string text = line.Trim();
                if (text.Length == 0 || text.StartsWith("#", StringComparison.Ordinal)) { continue; }
                string[] parts = text.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4) { continue; }
                heads[parts[0]] = new[] { double.Parse(parts[1], CultureInfo.InvariantCulture),
                                          double.Parse(parts[2], CultureInfo.InvariantCulture),
                                          double.Parse(parts[3], CultureInfo.InvariantCulture) };
            }
            ObstacleField field = new ObstacleField();
            foreach (string file in Directory.GetFiles(directory, "*.txt").OrderBy(f => f))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (!heads.TryGetValue(name, out double[]? head)) { continue; }
                List<WellboreUncertaintyStation> stations = SurveyFileReader.ReadWithUncertainty(file);
                if (stations.Count < 2) { continue; }
                field.Add(new WellboreUncertainty(stations, head[0], head[1], head[2], 2.0) { Name = name });
            }
            field.BuildIndex();
            wellhead = heads["U1"];
            return field;
        }

        private static List<Point3D> Seed(double[] u1)
        {
            return ReferenceSpine.Sample(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                         new Vector3D(0, 0, 1),
                                         new Point3D(u1[0] - 400.0, u1[1] + 400.0, TargetVertical),
                                         new Vector3D(0, 0, 1), 257);
        }

        [Test]
        public void TheRelaxationPushesTheSpineOutOfTheVolumes()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);
            List<Point3D> seed = Seed(u1);

            int before = 0;
            foreach (Point3D point in seed)
            {
                if (field.GetNearestExcess(point.X!.Value, point.Y!.Value, point.Z!.Value, 400.0) < 0)
                {
                    before++;
                }
            }
            Assert.That(before, Is.GreaterThan(0), "the seed is supposed to cross a volume");

            SpineRelaxerResult relaxed = SpineRelaxer.Relax(seed, field, new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new SpineRelaxerOptions { CeilingVertical = Ground, FloorVertical = TargetVertical + 40.0 });
            TestContext.Progress.WriteLine($"{before} samples inside before, " + relaxed.Describe());

            Assert.That(relaxed.InsideCount, Is.Zero, "the spine still crosses a volume");
            Assert.That(relaxed.Spine.Count, Is.GreaterThan(2));
            // the ends and their tangents are the plan's fixed points
            Assert.That(relaxed.Spine[0].X!.Value, Is.EqualTo(u1[0] + 1.2).Within(1e-9));
            Assert.That(relaxed.Spine[relaxed.Spine.Count - 1].Z!.Value,
                        Is.EqualTo(TargetVertical).Within(1e-9));
            double dn = relaxed.Spine[1].X!.Value - relaxed.Spine[0].X!.Value;
            double de = relaxed.Spine[1].Y!.Value - relaxed.Spine[0].Y!.Value;
            Assert.That(System.Math.Sqrt(dn * dn + de * de), Is.LessThan(1e-9),
                        "the spine does not leave the slot vertically");
        }

        /// <summary>
        /// how far apart two curves are at their furthest, compared fraction by fraction along
        /// </summary>
        private static double Apart(IReadOnlyList<Point3D> a, IReadOnlyList<Point3D> b)
        {
            int count = System.Math.Min(a.Count, b.Count);
            double worst = 0;
            for (int i = 0; i < count; i++)
            {
                Point3D one = a[i * (a.Count - 1) / System.Math.Max(1, count - 1)];
                Point3D other = b[i * (b.Count - 1) / System.Math.Max(1, count - 1)];
                double gap = System.Math.Sqrt(
                    System.Math.Pow(one.X!.Value - other.X!.Value, 2)
                    + System.Math.Pow(one.Y!.Value - other.Y!.Value, 2)
                    + System.Math.Pow(one.Z!.Value - other.Z!.Value, 2));
                if (gap > worst) { worst = gap; }
            }
            return worst;
        }

        /// <summary>
        /// Finding the genuinely different ways round, by forbidding each one as it is found.
        /// <para>
        /// Two things had to be true before this could work. The relaxation had to converge, which it
        /// does only coarse to fine — before that it handed its seeds straight back and nine nudges gave
        /// nine "routes" that were nothing of the sort. And once it converges it collapses everything
        /// onto one route, because an elastic band slides between wells that fan out with depth: twenty
        /// five seeds pushed up to twelve hundred metres all came back to the same curve. So the
        /// alternatives cannot be discovered by proposing them. Each has to be found, then forbidden, so
        /// that the next relaxation has nowhere to collapse to.
        /// </para>
        /// </summary>
        [Test]
        public void ForbiddingEachRouteFindsTheNextOne()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);
            Point3D slot = new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]);
            Point3D target = new Point3D(u1[0] - 400.0, u1[1] + 400.0, TargetVertical);
            Vector3D down = new Vector3D(0, 0, 1);

            SpineRelaxerOptions settings = new SpineRelaxerOptions
            {
                CeilingVertical = Ground,
                FloorVertical = TargetVertical + 40.0,
                HoldLength = 40.0
            };
            List<SpineRelaxerResult> routes = new List<SpineRelaxerResult>();
            const double keepOut = 150.0;

            for (int round = 0; round < 4; round++)
            {
                // seeded away from everything already found, so the relaxation starts on the right side
                double pushNorth = 0, pushEast = 0;
                foreach (SpineRelaxerResult had in routes)
                {
                    Point3D middle = had.Spine[had.Spine.Count / 2];
                    pushNorth -= middle.X!.Value;
                    pushEast -= middle.Y!.Value;
                }
                if (routes.Count > 0)
                {
                    pushNorth = pushNorth / routes.Count + (slot.X!.Value + target.X!.Value) / 2;
                    pushEast = pushEast / routes.Count + (slot.Y!.Value + target.Y!.Value) / 2;
                    double length = System.Math.Sqrt(pushNorth * pushNorth + pushEast * pushEast);
                    if (length > 0)
                    {
                        pushNorth *= 400.0 / length;
                        pushEast *= 400.0 / length;
                    }
                }
                List<Point3D> seed = routes.Count == 0
                    ? ReferenceSpine.Sample(slot, down, target, down, 129)
                    : ReferenceSpine.Nudged(slot, down, target, down, pushNorth, pushEast, 0, 129);

                SpineRelaxerResult got = SpineRelaxer.Relax(seed, field, down, down, settings);
                double nearest = double.MaxValue;
                foreach (SpineRelaxerResult had in routes)
                {
                    double gap = Apart(got.Spine, had.Spine);
                    if (gap < nearest) { nearest = gap; }
                }
                TestContext.Progress.WriteLine(
                    $"  route {round}: {got.Describe()}"
                    + (routes.Count == 0 ? "" : $"   {nearest:F0} m from the nearest already found"));
                routes.Add(got);
                settings.KeepOut.Add(new KeepOutTube { Path = got.Spine, Radius = keepOut });
            }

            TestContext.Progress.WriteLine("");
            for (int j = 0; j < routes.Count; j++)
            {
                for (int i = j + 1; i < routes.Count; i++)
                {
                    TestContext.Progress.WriteLine(
                        $"    routes {j} and {i}: {Apart(routes[j].Spine, routes[i].Spine):F0} m apart");
                }
            }
            for (int j = 1; j < routes.Count; j++)
            {
                Assert.That(Apart(routes[j].Spine, routes[0].Spine), Is.GreaterThan(60.0),
                            $"route {j} is not distinct from the first");
            }
        }

        /// <summary>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~TheRelaxationSettingsSweep"</c>.
        /// </summary>
        [Test]
        [Explicit("sweeps the relaxation's own settings")]
        public void TheRelaxationSettingsSweep()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);
            List<Point3D> seed = Seed(u1);
            double seedDogleg = StreamlineCurvature.GetWorst(new Streamline(seed)) * 180 / System.Math.PI;
            TestContext.Progress.WriteLine($"seed: worst dogleg {seedDogleg:F2} deg/30m");
            TestContext.Progress.WriteLine(
                "  sweeps  smooth  push  want    ms | inside  least  dogleg  length");

            foreach (int sweeps in new[] { 400, 2000, 8000 })
            {
                foreach (double smoothing in new[] { 0.3, 0.45 })
                {
                    foreach (double clearance in new[] { 3.0, 10.0 })
                    {
                        Stopwatch watch = Stopwatch.StartNew();
                        SpineRelaxerResult relaxed = SpineRelaxer.Relax(seed, field,
                            new Vector3D(0, 0, 1), new Vector3D(0, 0, 1),
                            new SpineRelaxerOptions
                            {
                                IterationCount = sweeps,
                                Smoothing = smoothing,
                                Clearance = clearance,
                                CeilingVertical = Ground,
                                FloorVertical = TargetVertical + 40.0
                            });
                        watch.Stop();
                        TestContext.Progress.WriteLine(
                            $"  {sweeps,6} {smoothing,7:F2} {0.4,5:F2} {clearance,5:F0}"
                            + $" {watch.ElapsedMilliseconds,5} | {relaxed.InsideCount,6}"
                            + $" {relaxed.LeastClearance,6:F1} {relaxed.WorstDogleg * 180 / System.Math.PI,7:F2}"
                            + $" {relaxed.Length,7:F0}");
                    }
                }
            }
        }
    }
}
