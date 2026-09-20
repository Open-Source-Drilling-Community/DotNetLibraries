using System.Globalization;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// Does a cross-section of a built factory actually contain the crossings it stands for?
    /// <para>
    /// The requirement is that the cross-section delineate the crossings of the original streamlines and
    /// englobe them, without having to pass through the outermost of them. The builder instead fits each
    /// cross-section by least squares, which is a fit through the middle of the crossings and not round
    /// the outside of them, so nothing makes containment hold. This measures how far from holding it is:
    /// a crossing is carried back through the inverse of its station's transform and its normalized
    /// radius compared with the disc the factory draws from.
    /// </para>
    /// </summary>
    [TestFixture]
    public class FactoryRequirementsCheck
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private const double Ground = -91.2;
        private const double TargetVertical = 900.0;
        private const double TargetHalf = 80.0;

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
        /// Where a streamline crosses the plane of a station, in the frame of that station.
        /// <para>
        /// A streamline that wanders can cross one plane more than once, and the builder keeps the
        /// crossing nearest the median. This has to keep the same one: taking the first instead reports a
        /// re-crossing far out in the domain as though the outline had failed to contain it, which is a
        /// property of the measurement and not of the outline.
        /// </para>
        /// </summary>
        private static bool GetCrossing(Streamline line, CrossSectionStation station,
                                        out double u, out double v)
        {
            u = 0;
            v = 0;
            bool found = false;
            double nearest = double.MaxValue;
            List<Point3D> positions = line.Positions!;
            double px = station.Position!.X!.Value;
            double py = station.Position!.Y!.Value;
            double pz = station.Position!.Z!.Value;
            double tx = station.Tangent!.X!.Value;
            double ty = station.Tangent!.Y!.Value;
            double tz = station.Tangent!.Z!.Value;
            double previous = 0;
            for (int i = 0; i < positions.Count; i++)
            {
                double dx = positions[i].X!.Value - px;
                double dy = positions[i].Y!.Value - py;
                double dz = positions[i].Z!.Value - pz;
                double along = dx * tx + dy * ty + dz * tz;
                if (i > 0 && ((previous <= 0 && along >= 0) || (previous >= 0 && along <= 0))
                    && previous != along)
                {
                    double fraction = -previous / (along - previous);
                    double x = positions[i - 1].X!.Value
                               + fraction * (positions[i].X!.Value - positions[i - 1].X!.Value);
                    double y = positions[i - 1].Y!.Value
                               + fraction * (positions[i].Y!.Value - positions[i - 1].Y!.Value);
                    double z = positions[i - 1].Z!.Value
                               + fraction * (positions[i].Z!.Value - positions[i - 1].Z!.Value);
                    double ex = x - px, ey = y - py, ez = z - pz;
                    double here = ex * station.FirstNormal!.X!.Value + ey * station.FirstNormal!.Y!.Value
                                  + ez * station.FirstNormal!.Z!.Value;
                    double there = ex * station.SecondNormal!.X!.Value
                                   + ey * station.SecondNormal!.Y!.Value
                                   + ez * station.SecondNormal!.Z!.Value;
                    double offset = here * here + there * there;
                    if (offset < nearest)
                    {
                        nearest = offset;
                        u = here;
                        v = there;
                        found = true;
                    }
                }
                previous = along;
            }
            return found;
        }

        /// <summary>
        /// What the factory actually holds, before anything is asked of it. A residual of the same size
        /// as the bundle radius and a tube ratio of zero are what a fit that failed looks like, not what a
        /// fit that succeeded looks like.
        /// <para>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~WhatTheFactoryHolds"</c>.
        /// </para>
        /// </summary>
        [Test]
        [Explicit("one full generation on the real data")]
        public void WhatTheFactoryHolds()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);
            StreamlineGenerationResult got = Run(field, u1, 1000);
            List<Streamline> arrived = Arrived(got);
            int conduitLength = got.ConduitPaths.Count > 0 ? got.ConduitPaths[0].Count : 0;
            TestContext.Progress.WriteLine(
                $"{arrived.Count} candidates, each carrying {conduitLength} shared conduit positions "
                + "at its head");

            StreamlineBundlingResult bundling = new StreamlineBundler().Bundle(arrived);
            foreach (StreamlineBundle bundle in bundling.Bundles)
            {
                if (bundle.Count < 10) { continue; }
                List<Streamline> whole = bundle.StreamlineIndices.Select(i => arrived[i]).ToList();
                // the same bundle with the shared head removed: every member of every bundle begins with
                // the identical conduit cells, so the crossings there are one point, not a spread
                List<Streamline> trimmed = new List<Streamline>();
                foreach (Streamline line in whole)
                {
                    List<Point3D> positions = line.Positions!;
                    if (positions.Count <= conduitLength + 2) { continue; }
                    trimmed.Add(new Streamline(positions.GetRange(conduitLength,
                                                                  positions.Count - conduitLength)));
                }
                Report($"bundle {bundle.Index} whole   ", whole);
                Report($"bundle {bundle.Index} trimmed ", trimmed);
                if (bundle.Index >= 3) { break; }
            }
        }

        private static void Report(string name, IReadOnlyList<Streamline> members)
        {
            StreamlineBundleFactory? factory = new StreamlineBundleFactoryBuilder()
                .Build(members, out StreamlineFactoryFailureReason why);
            if (factory == null)
            {
                TestContext.Progress.WriteLine($"  {name}: no factory ({why})");
                return;
            }
            TestContext.Progress.WriteLine(
                $"  {name}: {members.Count,4} members, {factory.Stations.Count} stations,"
                + $" length {factory.Length:0.0} m, spacing {factory.StationSpacing:0.00} m,"
                + $" normalized radius {factory.NormalizedRadius:0.000}");
            TestContext.Progress.WriteLine(
                $"      residual {100.0 * factory.ResidualFraction:0.0}% of radius,"
                + $" tube ratio {factory.MaximumTubeRatio:0.000},"
                + $" tangent deviation {factory.TangentDeviation:0.0000} rad against a bundle spread of"
                + $" {factory.BundleTangentSpread:0.0000}");
            foreach (int at in new[] { 0, factory.Stations.Count / 2, factory.Stations.Count - 1 })
            {
                if (at < 0 || at >= factory.Stations.Count) { continue; }
                CrossSectionStation station = factory.Stations[at];
                TestContext.Progress.WriteLine(
                    $"      station {at,3}: members {station.StreamlineCount,4},"
                    + $" radius {station.Radius:0.000} m,"
                    + $" inradius {(station.Polygon != null ? station.Polygon.Inradius : 0):0.000} m,"
                    + $" outer {(station.Polygon != null ? station.Polygon.OuterRadius : 0):0.000} m,"
                    + $" directions {(station.Polygon != null ? station.Polygon.DirectionCount : 0)}"
                    + $" ({(station.Polygon != null ? station.Polygon.BlockedDirectionCount : 0)} blocked),"
                    + $" twist {station.Twist:0.000} rad,"
                    + $" curvature {station.Curvature:0.00000}");
            }
        }

        /// <summary>
        /// Is the number of streamlines in a bundle a usable stand-in for how wide its corridor is?
        /// <para>
        /// Streamlines carry equal flux, and flux is area times velocity, so a wide slow corridor and a
        /// narrow fast one can carry the same number. The question matters because a bundle is worth
        /// keeping or discarding on whether it can be drilled, which is a question about width.
        /// </para>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~IsCountAProxyForWidth"</c>.
        /// </summary>
        [Test]
        [Explicit("two full generations on the real data")]
        public void IsCountAProxyForWidth()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);
            foreach (TargetArrivalWeighting weighting in new[] { TargetArrivalWeighting.Uniform,
                                                                 TargetArrivalWeighting.Gaussian })
            {
                StreamlineGenerationResult got = Run(field, u1, 1000, weighting);
                List<Streamline> arrived = Arrived(got);
                int conduitLength = got.ConduitPaths.Count > 0 ? got.ConduitPaths[0].Count : 0;
                StreamlineBundlingResult bundling = new StreamlineBundler().Bundle(arrived);
                TestContext.Progress.WriteLine(
                    $"{weighting}: {arrived.Count} arrived of 1000, {got.ServedTargetCellCount} cells"
                    + $" served, {bundling.Bundles.Count} bundles");
                TestContext.Progress.WriteLine(
                    "   members | radius over the middle 80% of the length, m | shared head | residual");
                foreach (StreamlineBundle bundle in bundling.Bundles)
                {
                    List<Streamline> members = bundle.StreamlineIndices
                        .Select(i => arrived[i]).ToList();
                    // the floor is dropped to three so the marginal bundles can be looked at rather than
                    // refused before they are measured
                    StreamlineBundleFactory? factory = new StreamlineBundleFactoryBuilder(
                        new StreamlineBundleFactoryOptions { MinimumStreamlineCount = 3 })
                        .Build(members, out StreamlineFactoryFailureReason why);
                    if (factory == null)
                    {
                        TestContext.Progress.WriteLine(
                            $"   {bundle.Count,7} | no factory ({why})");
                        continue;
                    }
                    // the pinch is what stops a well, not the average width, so the profile is read
                    // over the middle eighty per cent of the length: the head is where the paths are
                    // still together and the tail is where they converge on the target, and neither
                    // says anything about how much room there is in between
                    int first = factory.Stations.Count / 10;
                    int last = factory.Stations.Count - 1 - factory.Stations.Count / 10;
                    List<double> radii = new List<double>();
                    for (int k = first; k <= last && k < factory.Stations.Count; k++)
                    {
                        if (factory.Stations[k].Radius > 0) { radii.Add(factory.Stations[k].Radius); }
                    }
                    if (radii.Count == 0)
                    {
                        TestContext.Progress.WriteLine($"   {bundle.Count,7} | no radius");
                        continue;
                    }
                    radii.Sort();
                    TestContext.Progress.WriteLine(
                        $"   {bundle.Count,7} | pinch {radii[0],6:0.00} | tenth"
                        + $" {radii[radii.Count / 10],6:0.00} | median {radii[radii.Count / 2],6:0.00}"
                        + $" | widest {radii[radii.Count - 1],6:0.00}"
                        + $" | head {factory.SharedHead.Count,3}"
                        + $" | residual {100.0 * factory.ResidualFraction:0.0}%");
                }
            }
        }

        private static StreamlineGenerationResult Run(
            ObstacleField field, double[] u1, int launches,
            TargetArrivalWeighting weighting = TargetArrivalWeighting.Uniform)
        {
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
            TargetPolygon target = new TargetPolygon(corners, 20.0)
            {
                ArrivalWeighting = weighting
            };
            return StreamlineGenerator.GenerateChannelled(
                field, new[] { slot }, target,
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = 1.0,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = Ground,
                        FloorVertical = targetAt[2] + 40.0
                    },
                    StreamlineCount = launches,
                    ChannelContrast = 100.0,
                    ChannelNarrowWidth = 25.0,
                    ChannelWideWidth = 150.0,
                    OutletGuideContrast = 20.0,
                    OutletGuideLength = 120.0,
                    OutletGuideWidth = 30.0,
                    LaunchInset = 0.25,
                    Relaxer = new SpineRelaxerOptions { HoldLength = 40.0 }
                }, 1);
        }

        private static List<Streamline> Arrived(StreamlineGenerationResult got)
        {
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
            return arrived;
        }

        /// <summary>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~DoesTheCrossSectionContainTheCrossings"</c>.
        /// </summary>
        [Test]
        [Explicit("one full generation on the real data, then a factory per bundle")]
        public void DoesTheCrossSectionContainTheCrossings()
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
            int conduitLength = got.ConduitPaths.Count > 0 ? got.ConduitPaths[0].Count : 0;
            StreamlineBundlingResult bundling = new StreamlineBundler().Bundle(arrived);
            TestContext.Progress.WriteLine(
                $"{bundling.Bundles.Count} bundles from {arrived.Count} candidates; "
                + $"keeping those with at least ten members, conduit head of {conduitLength} removed");

            foreach (StreamlineBundle bundle in bundling.Bundles)
            {
                if (bundle.Count < 10) { continue; }
                // the shared conduit head has to go, or the fit is degenerate and there is nothing to
                // measure containment against
                List<Streamline> members = new List<Streamline>();
                foreach (int index in bundle.StreamlineIndices)
                {
                    List<Point3D> positions = arrived[index].Positions!;
                    if (positions.Count <= conduitLength + 2) { continue; }
                    members.Add(new Streamline(positions.GetRange(conduitLength,
                                                                  positions.Count - conduitLength)));
                }
                StreamlineBundleFactoryBuilder builder = new StreamlineBundleFactoryBuilder();
                StreamlineBundleFactory? factory = builder.Build(
                    members, out StreamlineFactoryFailureReason why);
                if (factory == null)
                {
                    TestContext.Progress.WriteLine(
                        $"  bundle {bundle.Index}: {bundle.Count} members, no factory ({why})");
                    continue;
                }

                // the members as the factory fitted them: it held the shared head back, and that head runs
                // vertically from the slot, so it crosses the early station planes a long way from the
                // median. Measuring containment against geometry the outline was never built from would
                // report the outline failing at something it was never shown.
                List<Streamline> fitted = new List<Streamline>();
                foreach (Streamline member in members)
                {
                    List<Point3D> positions = member.Positions!;
                    int drop = System.Math.Min(factory.SharedHead.Count, System.Math.Max(0, positions.Count - 2));
                    fitted.Add(new Streamline(positions.GetRange(drop, positions.Count - drop)));
                }

                long inside = 0;
                long outside = 0;
                double worstOvershoot = 0;
                double leastInradius = double.MaxValue;
                int blockedDirections = 0;
                int firstStation = factory.Stations.Count / 10;
                int lastStation = factory.Stations.Count - 1 - factory.Stations.Count / 10;
                for (int at = 0; at < factory.Stations.Count; at++)
                {
                    CrossSectionStation station = factory.Stations[at];
                    CrossSectionPolygon? polygon = station.Polygon;
                    if (polygon == null) { continue; }
                    blockedDirections += polygon.BlockedDirectionCount;
                    // the head is where the members are still together and the tail is where they
                    // converge on the target: both are pinched by construction and neither says anything
                    // about the room a well has in between
                    if (at >= firstStation && at <= lastStation && polygon.Inradius < leastInradius)
                    {
                        leastInradius = polygon.Inradius;
                    }
                    foreach (Streamline member in fitted)
                    {
                        if (!GetCrossing(member, station, out double u, out double v)) { continue; }
                        if (station.Contains(u, v))
                        {
                            inside++;
                            continue;
                        }
                        outside++;
                        double reach = System.Math.Sqrt(u * u + v * v);
                        double boundary = station.GetBoundaryRadius(u, v);
                        if (boundary > 0)
                        {
                            double overshoot = (reach - boundary) / boundary;
                            if (overshoot > worstOvershoot) { worstOvershoot = overshoot; }
                        }
                    }
                }
                TestContext.Progress.WriteLine(
                    $"  bundle {bundle.Index}: {bundle.Count,4} members, {factory.Stations.Count} stations"
                    + $" | crossings inside the outline {inside}, outside {outside}"
                    + $" ({100.0 * outside / System.Math.Max(1, inside + outside):0.000}%)"
                    + $", worst overshoot {100.0 * worstOvershoot:0.0}%"
                    + $" | least inradius {(leastInradius == double.MaxValue ? 0 : leastInradius):0.00} m"
                    + $" | {blockedDirections} blocked directions in all"
                    + $" | residual {100.0 * factory.ResidualFraction:0.0}% of the radius"
                    + $", tube ratio {factory.MaximumTubeRatio:0.00}");
            }
        }
    }
}
