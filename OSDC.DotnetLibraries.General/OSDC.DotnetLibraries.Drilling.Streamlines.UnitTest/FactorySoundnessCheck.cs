using System.Globalization;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// Three things a factory must not do, all of them visible in the 3D view before any test asked for
    /// them.
    /// <para>
    /// A factory stands for a corridor that is clear of every uncertainty volume, disjoint from the other
    /// corridors, and no more tortuous than the paths it was built from. An outline is an envelope, and
    /// an envelope can claim ground none of its members ever occupied: it can arch over an obstruction
    /// that the streamlines went round, and it can reach into the ground another bundle owns. And a
    /// boundary distance is an extreme of the crossings rather than an average of them, so it is a far
    /// noisier number station to station than a least squares fit would be, which a drawn streamline
    /// would inherit as wiggle.
    /// </para>
    /// </summary>
    [TestFixture]
    public class FactorySoundnessCheck
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private const double Ground = -91.2;
        private const double TargetVertical = 900.0;
        private const double TargetHalf = 80.0;
        private const int StationStride = 4;

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
                heads[parts[0]] = new[] { double.Parse(parts[1], Invariant),
                                          double.Parse(parts[2], Invariant),
                                          double.Parse(parts[3], Invariant) };
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

        /// <summary>
        /// whether a position falls inside the outline of the factory, judged at the cross-section whose
        /// plane it is nearest to
        /// </summary>
        private static bool IsInside(StreamlineBundleFactory factory, double n, double e, double v)
        {
            int best = -1;
            double nearest = double.MaxValue;
            for (int k = 0; k < factory.Stations.Count; k++)
            {
                Point3D? at = factory.Stations[k].Position;
                if (at == null) { continue; }
                double dn = n - at.X!.Value, de = e - at.Y!.Value, dv = v - at.Z!.Value;
                double distance = dn * dn + de * de + dv * dv;
                if (distance < nearest) { nearest = distance; best = k; }
            }
            if (best < 0) { return false; }
            CrossSectionStation station = factory.Stations[best];
            if (station.Polygon == null || station.Position == null) { return false; }
            double ox = n - station.Position.X!.Value;
            double oy = e - station.Position.Y!.Value;
            double oz = v - station.Position.Z!.Value;
            double u = ox * station.FirstNormal!.X!.Value + oy * station.FirstNormal!.Y!.Value
                       + oz * station.FirstNormal!.Z!.Value;
            double w = ox * station.SecondNormal!.X!.Value + oy * station.SecondNormal!.Y!.Value
                       + oz * station.SecondNormal!.Z!.Value;
            return station.Contains(u, w);
        }

        /// <summary>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~WhatTheOutlinesClaim"</c>.
        /// </summary>
        [Test]
        [Explicit("a full generation, then a factory per bundle, then sampling their interiors")]
        public void WhatTheOutlinesClaim()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);
            double[] targetAt = { u1[0] - 400.0, u1[1] + 400.0, TargetVertical };
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(targetAt[0] - TargetHalf, targetAt[1] - TargetHalf, targetAt[2]),
                new Point3D(targetAt[0] + TargetHalf, targetAt[1] - TargetHalf, targetAt[2]),
                new Point3D(targetAt[0] + TargetHalf, targetAt[1] + TargetHalf, targetAt[2]),
                new Point3D(targetAt[0] - TargetHalf, targetAt[1] + TargetHalf, targetAt[2])
            };
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = 40.0
            };
            StreamlineGenerationResult got = StreamlineGenerator.GenerateChannelled(
                field, new[] { slot }, new TargetPolygon(corners, 20.0),
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = 1.0,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = Ground,
                        FloorVertical = targetAt[2] + 40.0
                    },
                    StreamlineCount = 1000,
                    ChannelContrast = 100.0,
                    ChannelNarrowWidth = 25.0,
                    ChannelWideWidth = 150.0,
                    OutletGuideContrast = 20.0,
                    OutletGuideLength = 120.0,
                    OutletGuideWidth = 30.0,
                    LaunchInset = 0.25,
                    Relaxer = new SpineRelaxerOptions { HoldLength = 40.0 }
                }, 1);

            List<Streamline> arrived = new List<Streamline>();
            for (int s = 0; s < got.Streamlines.Count; s++)
            {
                if (s < got.StreamlineOutcomes.Count
                    && got.StreamlineOutcomes[s] != TraceOutcome.ReachedSink)
                {
                    continue;
                }
                arrived.Add(got.Streamlines[s]);
            }
            StreamlineBundlingResult bundling = new StreamlineBundler().Bundle(arrived);

            // built as a set, with the uncertainty volumes handed in: the regions are laid with every
            // median present, so they are disjoint, and marched out only as far as the volumes allow
            StreamlineListSource source = new StreamlineListSource(arrived);
            List<IReadOnlyList<int>> asIndices = new List<IReadOnlyList<int>>();
            foreach (StreamlineBundle bundle in bundling.Bundles)
            {
                asIndices.Add(bundle.StreamlineIndices);
            }
            StreamlineFactorySetBuilder setBuilder = new StreamlineFactorySetBuilder();
            StreamlineFactorySet set = setBuilder.Build(source, asIndices, field);
            TestContext.Progress.WriteLine(
                $"{bundling.Bundles.Count} bundles in, {set.Factories.Count} factories out,"
                + $" {set.SplitCount} splits, {set.Refused.Count} refused");

            List<StreamlineBundleFactory> factories = set.Factories;
            List<int> memberCounts = new List<int>();
            List<List<Streamline>> memberSets = new List<List<Streamline>>();
            foreach (List<int> members in set.Members)
            {
                memberCounts.Add(members.Count);
                memberSets.Add(members.Select(i => arrived[i]).ToList());
            }

            // ---- 1. does an outline claim ground an uncertainty volume occupies? -------------------
            TestContext.Progress.WriteLine(
                "outline against the volumes: interior samples inside a well, and how deep");
            List<List<Point3D>> interiors = new List<List<Point3D>>();
            for (int f = 0; f < factories.Count; f++)
            {
                StreamlineBundleFactory factory = factories[f];
                List<Point3D> interior = new List<Point3D>();
                long inside = 0;
                double deepest = 0;
                for (int k = 0; k < factory.Stations.Count; k += StationStride)
                {
                    CrossSectionStation station = factory.Stations[k];
                    if (station.Polygon == null) { continue; }
                    int sectors = System.Math.Min(24, station.Polygon.DirectionCount);
                    for (int a = 0; a < sectors; a++)
                    {
                        double angle = 2.0 * System.Math.PI * a / sectors;
                        for (int r = 1; r <= 4; r++)
                        {
                            Point3D? at = station.GetPosition(r / 4.0, angle);
                            if (at == null) { continue; }
                            interior.Add(at);
                            double excess = field.GetNearestExcess(at.X!.Value, at.Y!.Value,
                                                                   at.Z!.Value, 60.0);
                            if (excess < 0)
                            {
                                inside++;
                                if (-excess > deepest) { deepest = -excess; }
                            }
                        }
                    }
                }
                interiors.Add(interior);
                TestContext.Progress.WriteLine(
                    $"  {memberCounts[f],4} paths: {inside} of {interior.Count} samples inside a volume"
                    + $" ({100.0 * inside / System.Math.Max(1, interior.Count):0.00}%), deepest"
                    + $" {deepest:0.00} m");
            }

            // ---- 2. do two outlines claim the same ground? ------------------------------------------
            TestContext.Progress.WriteLine("outline against outline: samples of one inside another");
            for (int f = 0; f < factories.Count; f++)
            {
                int worstOther = -1;
                long worstCount = 0;
                long total = 0;
                for (int g = 0; g < factories.Count; g++)
                {
                    if (g == f) { continue; }
                    long shared = 0;
                    foreach (Point3D at in interiors[f])
                    {
                        if (IsInside(factories[g], at.X!.Value, at.Y!.Value, at.Z!.Value)) { shared++; }
                    }
                    total += shared;
                    if (shared > worstCount) { worstCount = shared; worstOther = g; }
                }
                TestContext.Progress.WriteLine(
                    $"  {memberCounts[f],4} paths: {total} of {interiors[f].Count} samples fall in some"
                    + $" other outline ({100.0 * total / System.Math.Max(1, interiors[f].Count):0.0}%),"
                    + $" worst against the {(worstOther >= 0 ? memberCounts[worstOther] : 0)}-path"
                    + $" bundle at {worstCount}");
            }

            // ---- 3. are the drawn streamlines rougher than the ones they stand for? -----------------
            TestContext.Progress.WriteLine(
                "tortuosity, worst dogleg over a 30 m station, deg/30 m: members vs draws");
            Random random = new Random(20260920);
            for (int f = 0; f < factories.Count; f++)
            {
                List<double> members = new List<double>();
                foreach (Streamline member in memberSets[f])
                {
                    double worst = StreamlineCurvature.GetWorst(member) * 180.0 / System.Math.PI;
                    if (worst > 0) { members.Add(worst); }
                }
                List<double> draws = new List<double>();
                for (int d = 0; d < 24; d++)
                {
                    Streamline? drawn = factories[f].Draw(random);
                    if (drawn == null || drawn.Count < 4) { continue; }
                    double worst = StreamlineCurvature.GetWorst(drawn) * 180.0 / System.Math.PI;
                    if (worst > 0) { draws.Add(worst); }
                }
                members.Sort();
                draws.Sort();
                string median = members.Count > 0
                    ? members[members.Count / 2].ToString("0.0", Invariant) : "-";
                string drawnMedian = draws.Count > 0
                    ? draws[draws.Count / 2].ToString("0.0", Invariant) : "-";
                TestContext.Progress.WriteLine(
                    $"  {memberCounts[f],4} paths: members {median}, draws {drawnMedian}");
            }
        }
    }
}
