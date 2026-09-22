using System.Globalization;
using System.Text;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// The whole chain written out for display: generation, bundling, and one factory per bundle, with
    /// each factory shown as what it actually is — a median path, a series of outlines perpendicular to
    /// it, and streamlines drawn from its density.
    /// </summary>
    [TestFixture]
    public class UllriggFactoryScene
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private const double Ground = -91.2;
        private const double TargetVertical = 900.0;
        private const double TargetHalf = 80.0;
        private const int Launches = 1000;
        private const int DrawsPerBundle = 12;
        private const int OutlineStride = 16;

        /// <summary>
        /// The cluster as one case sees it: the volumes that constrain that case, and separately every
        /// well that was read, whether it constrains anything or not.
        /// <para>
        /// The two are not the same list, because a sidetrack needs its parent's geometry — where the
        /// window is and which way the hole points there — even when that parent has been taken out
        /// of the constraints.
        /// </para>
        /// </summary>
        private sealed class Cluster
        {
            public ObstacleField Field { get; } = new ObstacleField();

            public Dictionary<string, WellboreUncertainty> Wells { get; }
                = new Dictionary<string, WellboreUncertainty>(StringComparer.Ordinal);

            public List<string> Order { get; } = new List<string>();

            public double[] Wellhead { get; set; } = new double[3];
        }

        /// <summary>
        /// reads the cluster, optionally leaving one named well out of the constraints while still
        /// reading its geometry
        /// </summary>
        private static Cluster LoadUllrigg(string? without = null)
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
            Cluster cluster = new Cluster();
            foreach (string file in Directory.GetFiles(directory, "*.txt").OrderBy(f => f))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (!heads.TryGetValue(name, out double[]? head)) { continue; }
                List<WellboreUncertaintyStation> stations = SurveyFileReader.ReadWithUncertainty(file);
                if (stations.Count < 2) { continue; }
                WellboreUncertainty well = new WellboreUncertainty(stations, head[0], head[1], head[2],
                                                                   2.0) { Name = name };
                cluster.Wells[name] = well;
                cluster.Order.Add(name);
                if (!string.Equals(name, without, StringComparison.Ordinal))
                {
                    cluster.Field.Add(well);
                }
            }
            cluster.Field.BuildIndex();
            cluster.Wellhead = heads["U1"];
            return cluster;
        }

        /// <summary>
        /// the sample of a well nearest a measured depth, a tie-in being named by depth while the volume
        /// is held at a fixed step
        /// </summary>
        private static int GetNearestSample(WellboreUncertainty well, double measuredDepth)
        {
            int best = 0;
            double least = double.MaxValue;
            for (int s = 0; s < well.SampleCount; s++)
            {
                double gap = System.Math.Abs(well.GetMeasuredDepth(s) - measuredDepth);
                if (gap < least)
                {
                    least = gap;
                    best = s;
                }
            }
            return best;
        }

        /// <summary>
        /// how far a position lies from the centreline of a well, m. What a sidetrack achieves against
        /// its parent is an outcome of the generation and is read off afterwards, which is what this is
        /// for; nothing here imposes it.
        /// </summary>
        private static double GetDistanceToCentreline(WellboreUncertainty well,
                                                      double north, double east, double vertical)
        {
            double least = double.MaxValue;
            for (int s = 0; s < well.SampleCount; s++)
            {
                well.GetSample(s, out double n, out double e, out double v);
                double dn = north - n;
                double de = east - e;
                double dv = vertical - v;
                double gap = dn * dn + de * de + dv * dv;
                if (gap < least) { least = gap; }
            }
            return System.Math.Sqrt(least);
        }

        /// <summary>
        /// One case of the family: where the planned well starts and how the target is entered.
        /// </summary>
        private sealed class SceneCase
        {
            /// <summary>
            /// what the case is called in the view
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The entrance inclination of the target, degrees from vertical. Nought is Ullrigg's own
            /// target, lying flat and entered from straight above; ninety would be a target standing on
            /// end, entered horizontally.
            /// </summary>
            public double InclinationDegrees { get; set; }

            /// <summary>
            /// the well this case sidetracks, or null for a new slot beside U1
            /// </summary>
            public string? ParentWell { get; set; } = null;

            /// <summary>
            /// where along the parent the window is, in the parent's own measured depth
            /// </summary>
            public double KickOffMeasuredDepth { get; set; }

            /// <summary>
            /// Whether the parent's own uncertainty volume is taken out of the constraints altogether,
            /// rather than merely muted over the window.
            /// <para>
            /// Dropping it says the old hole is not a thing to be avoided — it is being left behind,
            /// plugged or reused. Keeping it is the other question, and asks the sidetrack to stand clear
            /// of a bore it is leaving from, which is far harder near the window and is why the two are a
            /// switch and not an assumption.
            /// </para>
            /// </summary>
            public bool DropParentUncertainty { get; set; }
        }

        /// <summary>
        /// the cases, run in order into one scene
        /// </summary>
        private static readonly SceneCase[] Cases =
        {
            new SceneCase { Name = "0\u00b0 entrance", InclinationDegrees = 0.0 },
            new SceneCase { Name = "85\u00b0 entrance", InclinationDegrees = 85.0 },
            // The window is at 275 m and not at the 200 m first asked for. U3's centreline runs inside
            // U8's ninety-nine per cent volume from 186 to 229 m, worst 0.8 m in at 205, so no window in
            // that band is usable while U8 is a constraint. Nor is a merely positive clearance enough: a
            // cell is blocked as soon as a volume comes within its own circumscribed radius, so a finest
            // cell of a metre needs about 1.7 m of room at the point, and 250 m, with 1.1 m, misses it by
            // three centimetres. At 275 m there is 2.4 m and the start cell is open.
            new SceneCase { Name = "U3 sidetrack", InclinationDegrees = 0.0, ParentWell = "U3",
                            KickOffMeasuredDepth = 275.0, DropParentUncertainty = true },
            // the same window as the case above, so the pair differ only in how the target is entered;
            // and it shares its target with the 85 degree slot case, so either pair can be superimposed
            new SceneCase { Name = "U3 sidetrack, 85\u00b0", InclinationDegrees = 85.0,
                            ParentWell = "U3", KickOffMeasuredDepth = 275.0,
                            DropParentUncertainty = true }
        };

        /// <summary>
        /// Runs the whole chain once per case and writes them as one scene, so that the cases can be put
        /// side by side rather than compared from memory.
        /// <para>
        /// A case varies only two things: where the well starts, and how the target is entered. The
        /// conduit, channel, outlet guide, launch disc and flow settings are the same in all of them, so
        /// that what differs between cases is what the flow has to do rather than how it was asked.
        /// </para>
        /// <para>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~WriteFactoryScenes"</c>.
        /// </para>
        /// </summary>
        [Test]
        [Explicit("one full generation, bundling and factory set per case; writes a scene file")]
        public void WriteFactoryScenes()
        {
            // every well, whatever any one case makes of it, so that the view can draw the whole
            // cluster once and grey out what a case does not treat as a constraint
            Cluster whole = LoadUllrigg();
            double[] origin = whole.Wellhead;
            double[] slotAt = { origin[0] + 1.2, origin[1] + 1.2, origin[2] };

            StringBuilder json = new StringBuilder();
            json.Append("{\"origin\":[").Append(Num(origin[0])).Append(',').Append(Num(origin[1]))
                .Append(",0],");
            json.Append("\"ground\":").Append(Num(Ground)).Append(',');
            json.Append("\"slot\":").Append(Point(slotAt, origin));
            AppendWells(json, whole, origin);
            json.Append(",\"cases\":[");
            for (int c = 0; c < Cases.Length; c++)
            {
                if (c > 0) { json.Append(','); }
                AppendCase(json, Cases[c]);
            }
            json.Append("]}");

            string output = Path.Combine(Path.GetTempPath(), "ullrigg-factory-scene.json");
            File.WriteAllText(output, json.ToString());
            TestContext.Progress.WriteLine($"wrote {new FileInfo(output).Length / 1024} kB to {output}");
        }

        /// <summary>
        /// Everything about the cases that can be known without solving anything: where each starts,
        /// which way it leaves, how much room it has there, and whether the target can be reached at all.
        /// <para>
        /// This exists because the scene takes minutes per case and this takes seconds. A start inside
        /// somebody else's volume, or a domain that does not connect, is settled here.
        /// </para>
        /// <para>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~ReportTheCaseStarts"</c>.
        /// </para>
        /// </summary>
        [Test]
        [Explicit("one grid per case, no flow solved")]
        public void ReportTheCaseStarts()
        {
            List<string> unusable = new List<string>();
            foreach (SceneCase one in Cases)
            {
                Cluster cluster = LoadUllrigg(one.DropParentUncertainty ? one.ParentWell : null);
                List<Point3D> corners = BuildTarget(cluster.Wellhead, one.InclinationDegrees,
                                                    out double floorVertical);
                StreamlineSource start = BuildStart(cluster, one);
                start.GetUnitDirection(out double dn, out double de, out double dv);
                double departure = System.Math.Atan2(System.Math.Sqrt(dn * dn + de * de), dv)
                                   * 180.0 / System.Math.PI;
                double heading = System.Math.Atan2(de, dn) * 180.0 / System.Math.PI;
                if (heading < 0) { heading += 360.0; }
                ObstacleProximity room = cluster.Field.GetProximity(start.Position!.X!.Value,
                                                                    start.Position.Y!.Value,
                                                                    start.Position.Z!.Value, 200.0);
                TestContext.Progress.WriteLine(
                    $"=== {one.Name} ===");
                TestContext.Progress.WriteLine(
                    $"  start ({start.Position.X!.Value - cluster.Wellhead[0]:0.0},"
                    + $" {start.Position.Y!.Value - cluster.Wellhead[1]:0.0},"
                    + $" {start.Position.Z!.Value:0.0}), leaving {departure:0.0} deg"
                    + $" at {heading:0.0} deg azimuth, {cluster.Field.Wells.Count} constraints");
                TestContext.Progress.WriteLine(
                    $"  nearest {GetWellName(cluster, room.NearestWell)} {room.NearestExcess:0.00} m,"
                    + $" then {GetWellName(cluster, room.SecondWell)} {room.SecondExcess:0.00} m");
                if (one.ParentWell != null)
                {
                    ReportTheWindowAlongTheParent(cluster, one);
                }

                TargetPolygon target = new TargetPolygon(corners, 20.0)
                {
                    Incidence = TargetIncidence.Through,
                    ThroughLength = 60.0
                };
                StreamlineGrid grid = StreamlineGrid.Build(cluster.Field, new[] { start }, target,
                                                           new StreamlineGridOptions
                                                           {
                                                               FinestCellSize = 1.0,
                                                               CoarsestCellSize = 25.0,
                                                               CeilingVertical = Ground,
                                                               FloorVertical = floorVertical
                                                           });
                TestContext.Progress.WriteLine("  " + grid.Describe());
                // A point being clear is not the same as its cell being open: a cell is blocked as soon
                // as a volume comes within its own circumscribed radius, so a finest cell of one metre
                // needs about 1.7 m of room at the point to be sure of holding an open cell.
                int leaf = grid.SourceLeaves.Length > 0 ? grid.SourceLeaves[0] : -1;
                if (leaf >= 0)
                {
                    OctreeCell cell = grid.Tree.GetCell(leaf);
                    double atCentre = cluster.Field.GetNearestExcess(cell.CentreNorth, cell.CentreEast,
                                                                     cell.CentreVertical, 200.0);
                    TestContext.Progress.WriteLine(
                        $"  start cell {cell.Size:0.##} m, {grid.States[leaf]},"
                        + $" room at its centre {atCentre:0.00} m against a radius of"
                        + $" {cell.CircumscribedRadius:0.00} m");
                }
                ReportTheConduitChain(grid, start);
                if (grid.Status != StreamlineGridStatus.Connected)
                {
                    unusable.Add($"{one.Name}: {grid.Status}");
                }
            }
            Assert.That(unusable, Is.Empty, string.Join("; ", unusable));
        }

        /// <summary>
        /// Why a factory's fitted median can begin a long way from where its head ends.
        /// <para>
        /// Two corridors of the flat case do this: their stations start hundreds of metres below the end
        /// of the shared head, and the view then draws a straight line across the gap, which is not a
        /// path and has no cross-sections. Every member of a bundle shares its first positions exactly,
        /// so the fit should begin at the one after them; this reports what each bundle actually holds
        /// so the answer comes from the data rather than from reading the builder.
        /// </para>
        /// <para>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~ReportTheFittedBodies"</c>.
        /// </para>
        /// </summary>
        [Test]
        [Explicit("one full generation of the flat case")]
        public void ReportTheFittedBodies()
        {
            SceneCase one = Cases[0];
            Cluster cluster = LoadUllrigg(one.DropParentUncertainty ? one.ParentWell : null);
            List<Point3D> corners = BuildTarget(cluster.Wellhead, one.InclinationDegrees,
                                                out double floorVertical);
            StreamlineSource start = BuildStart(cluster, one);
            TargetPolygon target = new TargetPolygon(corners, 20.0)
            {
                Incidence = TargetIncidence.Through,
                ThroughLength = 60.0
            };
            StreamlineGenerationResult got = StreamlineGenerator.GenerateChannelled(
                cluster.Field, new[] { start }, target, BuildOptions(floorVertical), 1);
            TestContext.Progress.WriteLine("generation: " + got.Describe());

            List<Streamline> arrived = new List<Streamline>();
            for (int k = 0; k < got.Streamlines.Count; k++)
            {
                if (k < got.StreamlineOutcomes.Count
                    && got.StreamlineOutcomes[k] != TraceOutcome.ReachedSink)
                {
                    continue;
                }
                arrived.Add(got.Streamlines[k]);
            }
            StreamlineBundlingResult bundling = new StreamlineBundler().Bundle(arrived);
            StreamlineListSource source = new StreamlineListSource(arrived);
            List<IReadOnlyList<int>> asIndices = new List<IReadOnlyList<int>>();
            foreach (StreamlineBundle bundle in bundling.Bundles)
            {
                asIndices.Add(bundle.StreamlineIndices);
            }
            StreamlineFactorySetBuilder setBuilder = new StreamlineFactorySetBuilder();
            StreamlineFactorySet set = setBuilder.Build(source, asIndices, cluster.Field);
            int represented = 0;
            foreach (IReadOnlyList<int> held in set.Members) { represented += held.Count; }
            TestContext.Progress.WriteLine(
                $"{bundling.Bundles.Count} bundles from {arrived.Count}, {set.Factories.Count}"
                + $" corridors, {set.SplitCount} splits, {represented} members in a factory");
            foreach ((int Count, StreamlineFactoryFailureReason Reason) refused in set.Refused)
            {
                TestContext.Progress.WriteLine(
                    $"  refused: {refused.Count} members, {refused.Reason}");
            }

            for (int f = 0; f < set.Factories.Count; f++)
            {
                StreamlineBundleFactory factory = set.Factories[f];
                IReadOnlyList<int> members = set.Members[f];

                // the longest prefix every member of this corridor holds in common, worked out here so
                // it does not depend on the builder's own answer
                int common = int.MaxValue;
                int shortest = int.MaxValue;
                int longest = 0;
                foreach (int member in members)
                {
                    List<Point3D> positions = arrived[member].Positions!;
                    if (positions.Count < shortest) { shortest = positions.Count; }
                    if (positions.Count > longest) { longest = positions.Count; }
                }
                List<Point3D> first = arrived[members[0]].Positions!;
                foreach (int member in members)
                {
                    List<Point3D> positions = arrived[member].Positions!;
                    int matched = 0;
                    while (matched < positions.Count && matched < first.Count
                           && System.Math.Abs(positions[matched].X!.Value - first[matched].X!.Value) < 1e-6
                           && System.Math.Abs(positions[matched].Y!.Value - first[matched].Y!.Value) < 1e-6
                           && System.Math.Abs(positions[matched].Z!.Value - first[matched].Z!.Value) < 1e-6)
                    {
                        matched++;
                    }
                    if (matched < common) { common = matched; }
                }

                Point3D? firstStation = null;
                int withPosition = 0;
                foreach (CrossSectionStation station in factory.Stations)
                {
                    if (station.Position == null) { continue; }
                    withPosition++;
                    firstStation ??= station.Position;
                }
                Point3D headEnd = factory.SharedHead.Count > 0
                                  ? factory.SharedHead[factory.SharedHead.Count - 1]
                                  : new Point3D(0, 0, 0);
                double gap = firstStation == null ? double.NaN
                             : System.Math.Sqrt(
                                 Squared(firstStation.X!.Value - headEnd.X!.Value)
                                 + Squared(firstStation.Y!.Value - headEnd.Y!.Value)
                                 + Squared(firstStation.Z!.Value - headEnd.Z!.Value));
                TestContext.Progress.WriteLine(
                    $"  corridor {f,2}: {members.Count,4} members, positions {shortest}-{longest},"
                    + $" common prefix {common}, head {factory.SharedHead.Count},"
                    + $" stations {factory.Stations.Count} ({withPosition} placed),"
                    + $" length {factory.Length:0.0} m"
                    + $", head ends at v {headEnd.Z!.Value:0.0}"
                    + (firstStation == null ? ", no station placed"
                       : $", first station at v {firstStation.Z!.Value:0.0}, gap {gap:0.0} m")
                    + (gap > 25 ? "   <== GAP" : ""));

                // for the offenders, what the members themselves look like at the join
                if (gap > 25)
                {
                    foreach (int member in members)
                    {
                        List<Point3D> positions = arrived[member].Positions!;
                        int at = System.Math.Min(common, positions.Count - 1);
                        TestContext.Progress.WriteLine(
                            $"      member {member,4}: {positions.Count,5} positions,"
                            + $" [0] v {positions[0].Z!.Value:0.0},"
                            + $" [{common}] v {positions[at].Z!.Value:0.0},"
                            + $" [last] v {positions[positions.Count - 1].Z!.Value:0.0}");
                    }
                    ReportLeaveOneOut(source, members, setBuilder.Factory, arrived);
                    Dump(arrived, members, f);
                }
            }
        }

        private static double Squared(double value)
        {
            return value * value;
        }

        /// <summary>
        /// Rebuilds a corridor's factory with each of its members left out in turn.
        /// <para>
        /// The span a factory keeps is the longest run of cross-sections reaching the largest population
        /// any cross-section reaches, so one member that misses one plane breaks the run there and the
        /// shorter side is discarded. If that is what happened, dropping the member responsible restores
        /// the whole span, and dropping any other changes nothing. Nothing else in the builder behaves
        /// that way, which is what makes this decisive rather than suggestive.
        /// </para>
        /// </summary>
        private static void ReportLeaveOneOut(StreamlineListSource source, IReadOnlyList<int> members,
                                              StreamlineBundleFactoryOptions options,
                                              List<Streamline> arrived)
        {
            StreamlineBundleFactoryBuilder builder = new StreamlineBundleFactoryBuilder(options);

            static (double Top, double Length, int Stations) Describe(StreamlineBundleFactory? factory)
            {
                if (factory == null) { return (double.NaN, double.NaN, 0); }
                foreach (CrossSectionStation station in factory.Stations)
                {
                    if (station.Position != null)
                    {
                        return (station.Position.Z!.Value, factory.Length, factory.Stations.Count);
                    }
                }
                return (double.NaN, factory.Length, factory.Stations.Count);
            }

            StreamlineBundleFactory? whole = builder.Build(source, new List<int>(members),
                                                           out StreamlineFactoryFailureReason _);
            (double Top, double Length, int Stations) baseline = Describe(whole);
            TestContext.Progress.WriteLine(
                $"      leave-one-out baseline: first station v {baseline.Top:0.0},"
                + $" length {baseline.Length:0.0} m, {baseline.Stations} stations");

            foreach (int dropped in members)
            {
                List<int> kept = new List<int>();
                foreach (int member in members)
                {
                    if (member != dropped) { kept.Add(member); }
                }
                StreamlineBundleFactory? without = builder.Build(source, kept,
                                                                 out StreamlineFactoryFailureReason why);
                (double Top, double Length, int Stations) got = Describe(without);
                bool restored = !double.IsNaN(got.Top) && got.Top < baseline.Top - 100.0;
                TestContext.Progress.WriteLine(
                    $"      without {dropped,4}: "
                    + (without == null ? $"refused, {why}"
                       : $"first station v {got.Top:0.0}, length {got.Length:0.0} m")
                    + (restored ? "   <== RESTORED" : ""));
            }
        }

        /// <summary>
        /// writes a corridor's members out, so the factory build can be worked on again without solving
        /// the flow another time
        /// </summary>
        private static void Dump(List<Streamline> arrived, IReadOnlyList<int> members, int corridor)
        {
            string path = Path.Combine(Path.GetTempPath(), $"ullrigg-gap-corridor-{corridor}.tsv");
            using StreamWriter writer = new StreamWriter(path);
            writer.WriteLine("member\tindex\tnorth\teast\tvertical");
            foreach (int member in members)
            {
                List<Point3D> positions = arrived[member].Positions!;
                for (int k = 0; k < positions.Count; k++)
                {
                    writer.WriteLine($"{member}\t{k}\t{positions[k].X!.Value.ToString("0.####", Invariant)}"
                                     + $"\t{positions[k].Y!.Value.ToString("0.####", Invariant)}"
                                     + $"\t{positions[k].Z!.Value.ToString("0.####", Invariant)}");
                }
            }
            TestContext.Progress.WriteLine($"      members written to {path}");
        }

        /// <summary>
        /// Whether the chain of cells the conduit is laid along is face connected the whole way.
        /// <para>
        /// It matters because of what a conduit is: every cell of the chain is closed on all its faces
        /// but the two that continue it. Both the solve and the connectivity check therefore let a chain
        /// cell reach only its neighbour in the chain, and they reach it across a <em>face</em>. A chain
        /// that steps diagonally once \u2014 two indices changing between consecutive cells \u2014 has no
        /// shared face there, so the conduit is sealed and the target becomes unreachable, whatever the
        /// open cells say.
        /// </para>
        /// <para>
        /// A vertical slot never meets this: the walk advances along one axis and every step is a face.
        /// A tie-in leaves along its parent's attitude, which is nobody's axis.
        /// </para>
        /// </summary>
        private static void ReportTheConduitChain(StreamlineGrid grid, StreamlineSource source)
        {
            source.GetUnitDirection(out double dn, out double de, out double dv);
            double n = source.Position!.X!.Value;
            double e = source.Position.Y!.Value;
            double v = source.Position.Z!.Value;
            double length = source.ConduitLength ?? 40.0;
            double[] direction = { dn, de, dv };
            double[] at = { n, e, v };
            List<int> chain = new List<int>();
            double travelled = 0;
            while (travelled <= length)
            {
                int leaf = grid.Tree.FindLeaf(at[0], at[1], at[2]);
                if (leaf < 0 || grid.States[leaf] != CellState.Open) { break; }
                if (chain.Count == 0 || chain[chain.Count - 1] != leaf) { chain.Add(leaf); }
                OctreeCell box = grid.Tree.GetCell(leaf);
                double[] low = { box.MinimumNorth, box.MinimumEast, box.MinimumVertical };
                double nearest = double.MaxValue;
                for (int a = 0; a < 3; a++)
                {
                    if (System.Math.Abs(direction[a]) < 1.0e-12) { continue; }
                    double face = direction[a] > 0 ? low[a] + box.Size : low[a];
                    double reach = (face - at[a]) / direction[a];
                    if (reach < nearest) { nearest = reach; }
                }
                if (nearest == double.MaxValue) { break; }
                double stride = System.Math.Max(0, nearest) + 1.0e-6 * box.Size;
                for (int a = 0; a < 3; a++) { at[a] += direction[a] * stride; }
                travelled += stride;
            }
            int diagonal = 0;
            int firstDiagonal = -1;
            List<int> neighbours = new List<int>();
            for (int k = 0; k + 1 < chain.Count; k++)
            {
                bool shares = false;
                for (int axis = 0; axis < 3 && !shares; axis++)
                {
                    for (int sense = -1; sense <= 1 && !shares; sense += 2)
                    {
                        neighbours.Clear();
                        grid.Tree.GetFaceNeighbours(chain[k], axis, sense, neighbours);
                        if (neighbours.Contains(chain[k + 1])) { shares = true; }
                    }
                }
                if (!shares)
                {
                    diagonal++;
                    if (firstDiagonal < 0) { firstDiagonal = k; }
                }
            }
            TestContext.Progress.WriteLine(
                $"  conduit chain {chain.Count} cells over {length:0} m,"
                + $" {diagonal} of {System.Math.Max(1, chain.Count - 1)} steps with no shared face"
                + (firstDiagonal >= 0 ? $", first at step {firstDiagonal}" : ""));
        }

        /// <summary>
        /// How much room a window has at each depth along the parent, so that a kick-off depth is chosen
        /// from what the cluster allows rather than from a round number.
        /// <para>
        /// A tie-in is a point on the parent's centreline, and near a pad the ninety-nine per cent
        /// volumes of the neighbours overlap one another, so whether a window is usable at all is a
        /// question about the neighbours and not about the parent.
        /// </para>
        /// </summary>
        private static void ReportTheWindowAlongTheParent(Cluster cluster, SceneCase one)
        {
            WellboreUncertainty parent = cluster.Wells[one.ParentWell!];
            StringBuilder line = new StringBuilder("  window room along " + one.ParentWell + ":");
            for (double md = 100.0; md <= 400.0; md += 10.0)
            {
                int sample = GetNearestSample(parent, md);
                parent.GetSample(sample, out double n, out double e, out double v);
                ObstacleProximity room = cluster.Field.GetProximity(n, e, v, 200.0);
                line.Append($" {md:0}:{room.NearestExcess:0.0}({GetWellName(cluster, room.NearestWell)})");
            }
            TestContext.Progress.WriteLine(line.ToString());
        }

        private static string GetWellName(Cluster cluster, int well)
        {
            return well >= 0 && well < cluster.Field.Wells.Count
                   ? cluster.Field.Wells[well].Name ?? "?" : "none";
        }

        /// <summary>
        /// The target for one case: a square standing at the given entrance inclination.
        /// <para>
        /// A well entering a plane square travels along that plane's normal, so the inclination asked for
        /// is the inclination of the normal, and the target leans over as it grows. Its azimuth is the way
        /// the well is already heading, from the slot out to the target, so the plane stands across the
        /// route rather than edge-on to it.
        /// </para>
        /// </summary>
        private static List<Point3D> BuildTarget(double[] u1, double inclinationDegrees,
                                                 out double floorVertical)
        {
            double[] targetAt = { u1[0] - 400.0, u1[1] + 400.0, TargetVertical };
            double inclination = inclinationDegrees * System.Math.PI / 180.0;
            double azimuth = System.Math.Atan2(targetAt[1] - u1[1], targetAt[0] - u1[0]);
            double[] normal =
            {
                System.Math.Sin(inclination) * System.Math.Cos(azimuth),
                System.Math.Sin(inclination) * System.Math.Sin(azimuth),
                System.Math.Cos(inclination)
            };
            // one in-plane axis kept horizontal, the other completing the frame and so running up and
            // down the dip
            double[] across = { -System.Math.Sin(azimuth), System.Math.Cos(azimuth), 0 };
            double[] down =
            {
                normal[1] * across[2] - normal[2] * across[1],
                normal[2] * across[0] - normal[0] * across[2],
                normal[0] * across[1] - normal[1] * across[0]
            };
            List<Point3D> corners = new List<Point3D>();
            foreach ((double a, double b) in new[] { (-1.0, -1.0), (1.0, -1.0), (1.0, 1.0), (-1.0, 1.0) })
            {
                corners.Add(new Point3D(
                    targetAt[0] + TargetHalf * (a * across[0] + b * down[0]),
                    targetAt[1] + TargetHalf * (a * across[1] + b * down[1]),
                    targetAt[2] + TargetHalf * (a * across[2] + b * down[2])));
            }
            // a leaning target reaches below its centre, so the floor goes under the whole of it rather
            // than under a flat target's single depth
            floorVertical = TargetVertical + TargetHalf * System.Math.Abs(down[2]) + 40.0;
            return corners;
        }

        /// <summary>
        /// Where the planned well starts and which way it leaves.
        /// <para>
        /// A slot sits beside U1's wellhead and leaves vertical, held over the length of a conductor. A
        /// tie-in sits on its parent at the given measured depth and leaves along the parent's own
        /// direction there, there being no way to turn at a point.
        /// </para>
        /// </summary>
        private static StreamlineSource BuildStart(Cluster cluster, SceneCase one)
        {
            if (one.ParentWell == null)
            {
                double[] at = { cluster.Wellhead[0] + 1.2, cluster.Wellhead[1] + 1.2,
                                cluster.Wellhead[2] };
                return new StreamlineSource(new Point3D(at[0], at[1], at[2]), new Vector3D(0, 0, 1))
                {
                    ConduitLength = 40.0
                };
            }
            WellboreUncertainty parent = cluster.Wells[one.ParentWell];
            int sample = GetNearestSample(parent, one.KickOffMeasuredDepth);
            parent.GetSample(sample, out double n, out double e, out double v);
            parent.GetTangent(sample, out double tn, out double te, out double tv);
            StreamlineSource tieIn = new StreamlineSource(new Point3D(n, e, v), new Vector3D(tn, te, tv))
            {
                Name = one.ParentWell + " sidetrack",
                ConduitLength = 40.0
            };
            // A parent can only be muted where it is still an obstacle. With its volume out of the
            // constraints there is nothing to mute, and naming it here would index a well the field does
            // not hold.
            for (int w = 0; w < cluster.Field.Wells.Count; w++)
            {
                if (string.Equals(cluster.Field.Wells[w].Name, one.ParentWell, StringComparison.Ordinal))
                {
                    tieIn.ParentWell = w;
                    tieIn.ParentMeasuredDepth = parent.GetMeasuredDepth(sample);
                    tieIn.ParentMutedLength = 30.0;
                }
            }
            return tieIn;
        }

        private void AppendCase(StringBuilder json, SceneCase one)
        {
            // one field per case, so that what a case treats as a constraint, and any muting it applies,
            // cannot reach the next one
            Cluster cluster = LoadUllrigg(one.DropParentUncertainty ? one.ParentWell : null);
            double[] origin = cluster.Wellhead;
            List<Point3D> corners = BuildTarget(origin, one.InclinationDegrees, out double floorVertical);
            StreamlineSource start = BuildStart(cluster, one);
            start.GetUnitDirection(out double dn, out double de, out double dv);
            double departure = System.Math.Atan2(System.Math.Sqrt(dn * dn + de * de), dv)
                               * 180.0 / System.Math.PI;
            double heading = System.Math.Atan2(de, dn) * 180.0 / System.Math.PI;
            if (heading < 0) { heading += 360.0; }
            TestContext.Progress.WriteLine(
                $"=== {one.Name}: entrance inclination {one.InclinationDegrees:0} deg,"
                + $" floor at {floorVertical:0},"
                + $" {cluster.Field.Wells.Count} of {cluster.Order.Count} wells a constraint ===");
            TestContext.Progress.WriteLine(
                $"start at vertical {start.Position!.Z!.Value:0.0} m, leaving at {departure:0.0} deg"
                + $" inclination and {heading:0.0} deg azimuth");

            json.Append("{\"name\":\"").Append(one.Name).Append('"');
            json.Append(",\"inclination\":").Append(one.InclinationDegrees.ToString("0", Invariant));
            json.Append(",\"start\":").Append(Point(new[] { start.Position!.X!.Value,
                                                            start.Position.Y!.Value,
                                                            start.Position.Z!.Value }, origin));
            json.Append(",\"departure\":").Append(departure.ToString("0.#", Invariant));
            json.Append(",\"heading\":").Append(heading.ToString("0.#", Invariant));
            json.Append(",\"parent\":")
                .Append(one.ParentWell == null ? "null" : "\"" + one.ParentWell + "\"");
            // the wells this case does not count as constraints, so that the view can say so rather than
            // leave a corridor looking as though it ran through a volume
            json.Append(",\"excluded\":[");
            if (one.ParentWell != null && one.DropParentUncertainty)
            {
                json.Append('"').Append(one.ParentWell).Append('"');
            }
            json.Append(']');
            WriteScene(json, cluster, one, start, corners, floorVertical);
            json.Append('}');
        }

        private void WriteScene(StringBuilder json, Cluster cluster, SceneCase one,
                                StreamlineSource start, List<Point3D> corners, double floorVertical)
        {
            ObstacleField field = cluster.Field;
            double[] origin = cluster.Wellhead;
            // The rate is drawn sixty metres beyond the target, reachable only along a tube running out
            // from it, so the flow is already heading the way the target asks before it meets any wall.
            // A landing length belongs to the other method and is left alone here: there is nothing to
            // hold on the approach side when the direction is set by where the flow is going.
            TargetPolygon target = new TargetPolygon(corners, 20.0)
            {
                Incidence = TargetIncidence.Through,
                ThroughLength = 60.0
            };
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            StreamlineGenerationResult got = StreamlineGenerator.GenerateChannelled(
                field, new[] { start }, target, BuildOptions(floorVertical), 1);
            double generateSeconds = watch.Elapsed.TotalSeconds;
            TestContext.Progress.WriteLine("generation: " + got.Describe());

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
            // what the paths actually do where they land, which is the whole point of the case: the
            // target only asks for an inclination, it does not impose one
            target.GetArrivalUnit(out double an, out double ae, out double av);
            List<double> arrivals = new List<double>();
            foreach (Streamline line in arrived)
            {
                List<Point3D> positions = line.Positions!;
                for (int i = positions.Count - 1; i > 0; i--)
                {
                    double dn = positions[i].X!.Value - positions[i - 1].X!.Value;
                    double de = positions[i].Y!.Value - positions[i - 1].Y!.Value;
                    double dv = positions[i].Z!.Value - positions[i - 1].Z!.Value;
                    double length = System.Math.Sqrt(dn * dn + de * de + dv * dv);
                    if (!(length > 1e-9)) { continue; }
                    arrivals.Add(System.Math.Atan2(System.Math.Sqrt(dn * dn + de * de), dv)
                                 * 180.0 / System.Math.PI);
                    break;
                }
            }
            arrivals.Sort();
            if (arrivals.Count > 0)
            {
                TestContext.Progress.WriteLine(
                    $"arrival inclination asked {System.Math.Atan2(System.Math.Sqrt(an * an + ae * ae), av) * 180.0 / System.Math.PI:0.0}"
                    + $" deg, got {arrivals[0]:0.0} to {arrivals[arrivals.Count - 1]:0.0},"
                    + $" median {arrivals[arrivals.Count / 2]:0.0}");
            }

            // A sidetrack's separation from its parent is an outcome, not a constraint: here the
            // parent is not even an obstacle, so this says what the flow did of its own accord.
            WellboreUncertainty? parent = one.ParentWell != null
                                          && cluster.Wells.TryGetValue(one.ParentWell,
                                                                       out WellboreUncertainty? found)
                                          ? found : null;

            watch.Restart();
            StreamlineBundlingResult bundling = new StreamlineBundler().Bundle(arrived);
            double bundleSeconds = watch.Elapsed.TotalSeconds;
            StreamlineListSource source = new StreamlineListSource(arrived);
            List<IReadOnlyList<int>> asIndices = new List<IReadOnlyList<int>>();
            foreach (StreamlineBundle bundle in bundling.Bundles)
            {
                asIndices.Add(bundle.StreamlineIndices);
            }
            watch.Restart();
            StreamlineFactorySet set = new StreamlineFactorySetBuilder().Build(source, asIndices, field);
            double factorySeconds = watch.Elapsed.TotalSeconds;
            TestContext.Progress.WriteLine(
                $"{bundling.Bundles.Count} bundles from {arrived.Count}, {set.Factories.Count} corridors,"
                + $" {set.SplitCount} splits, {set.Tolerance?.GroupCount ?? 0} groups genuinely apart");

            // Where the time went. The two passes are kept apart because both build an octree and both
            // solve a field, so a single "grid" or "solve" figure would hide that each is paid twice.
            double total = generateSeconds + bundleSeconds + factorySeconds;
            StringBuilder phases = new StringBuilder("timing, s: ");
            foreach (string phase in new[] { "scout grid", "scout medium", "scout solve", "scout trace",
                                             "relax", "grid", "medium", "solve", "trace" })
            {
                if (!got.Timings.TryGetValue(phase, out double milliseconds)) { continue; }
                phases.Append($"{phase} {milliseconds / 1000.0:0.0}  ");
            }
            TestContext.Progress.WriteLine(phases.ToString());
            TestContext.Progress.WriteLine(
                $"factory set: {set.RoundCount} rounds, {set.FitCount} fits"
                + $" ({set.ReusedFitCount} reused) in"
                + $" {set.FitMilliseconds / 1000.0:0.0} s, regions laid in"
                + $" {set.ToleranceMilliseconds / 1000.0:0.0} s,"
                + $" {set.MarchCount} marches, {set.MarchStepCount} march steps"
                + (set.MarchStepCount > 0
                   ? $", {1000.0 * set.ToleranceMilliseconds / set.MarchStepCount:0.0} us a step" : ""));
            TestContext.Progress.WriteLine(
                $"timing, s: generate {generateSeconds:0.0} (scout"
                + $" {(got.Timings.TryGetValue("scout total", out double scoutTotal) ? scoutTotal / 1000.0 : 0):0.0}),"
                + $" bundle {bundleSeconds:0.0}, factory set {factorySeconds:0.0},"
                + $" case total {total:0.0} ({total / 60.0:0.0} min)");

            json.Append(",\"target\":[");
            for (int c = 0; c < corners.Count; c++)
            {
                if (c > 0) { json.Append(','); }
                json.Append(Point(new[] { corners[c].X!.Value, corners[c].Y!.Value, corners[c].Z!.Value },
                                  origin));
            }
            json.Append(']');
            json.Append(",\"bundles\":[");
            Random random = new Random(20260920);
            bool firstBundle = true;
            for (int b = 0; b < set.Factories.Count; b++)
            {
                StreamlineBundleFactory factory = set.Factories[b];
                int memberCount = set.Members[b].Count;

                // the room a well has around the planned path, read where the corridor actually is: the
                // head is still one point and the tail converges on the target, so both ends are pinched
                // by construction and say nothing about what can be drilled in between
                int firstStation = factory.Stations.Count / 10;
                int lastStation = factory.Stations.Count - 1 - factory.Stations.Count / 10;
                double leastInradius = double.MaxValue;
                for (int at = firstStation; at <= lastStation && at < factory.Stations.Count; at++)
                {
                    CrossSectionPolygon? polygon = factory.Stations[at].Polygon;
                    if (polygon != null && polygon.Inradius < leastInradius)
                    {
                        leastInradius = polygon.Inradius;
                    }
                }
                if (leastInradius == double.MaxValue) { leastInradius = 0; }
                // how often the median itself is inside a forbidden zone. Where it is, the march has
                // nowhere to go from its first step, the region collapses to the median point, and that
                // point is inside a volume: the inradius then correctly reads zero, but it is worth
                // saying so rather than letting it surface as a region that trespasses.
                int medianInside = 0;
                double medianWorst = 0;
                foreach (CrossSectionStation station in factory.Stations)
                {
                    if (station.Position == null) { continue; }
                    double excess = field.GetNearestExcess(station.Position.X!.Value,
                                                           station.Position.Y!.Value,
                                                           station.Position.Z!.Value, 60.0);
                    if (excess < 0)
                    {
                        medianInside++;
                        if (-excess > medianWorst) { medianWorst = -excess; }
                    }
                }

                // how close the median stays to the parent it left. Nothing asks for a separation, so
                // this reports one rather than checks it; the pinched head is left out because the first
                // stations are the window itself.
                double leastFromParent = double.NaN;
                if (parent != null)
                {
                    leastFromParent = double.MaxValue;
                    for (int at = factory.Stations.Count / 10; at < factory.Stations.Count; at++)
                    {
                        Point3D? position = factory.Stations[at].Position;
                        if (position == null) { continue; }
                        double gap = GetDistanceToCentreline(parent, position.X!.Value,
                                                             position.Y!.Value, position.Z!.Value);
                        if (gap < leastFromParent) { leastFromParent = gap; }
                    }
                }

                // The factory holds the median's curvature in rad/m, as everything in the library does.
                // A view is read by people who think in degrees per thirty metres, so the conversion
                // happens here, at the boundary, and nowhere inside.
                const double toDegreesPerStation = 180.0 / System.Math.PI
                                                   * StreamlineCurvature.StandardStation;
                double dogleg = factory.MedianCurvature * toDegreesPerStation;
                double doglegLong = factory.MedianCurvatureLong * toDegreesPerStation;
                // and the median on its own, without the shared head in front of it, to say whether a
                // corner sits at the join between the two
                Streamline bare = factory.GetMedianCurve();
                double doglegBare = StreamlineCurvature.GetWorst(bare) * 180.0 / System.Math.PI;
                TestContext.Progress.WriteLine(
                    $"  corridor {b}: {memberCount,4} members, least inradius {leastInradius,6:0.00} m,"
                    + $" residual {100.0 * factory.ResidualFraction:0.0}%,"
                    + $" {factory.OutsideMemberCount} unrepresented,"
                    + $" median inside a volume at {medianInside} of {factory.Stations.Count} stations"
                    + (medianInside > 0 ? $" (up to {medianWorst:0.00} m in)" : "")
                    + $", group {(set.Tolerance != null && b < set.Tolerance.Group.Length ? set.Tolerance.Group[b] : 0)}"
                    + $", median turns {dogleg:0.0} deg/30 m ({doglegLong:0.0} over 120 m,"
                    + $" {doglegBare:0.0} without the head)"
                    + (double.IsNaN(leastFromParent) ? ""
                       : $", closest to {one.ParentWell} {leastFromParent:0.0} m"));

                if (!firstBundle) { json.Append(','); }
                firstBundle = false;
                json.Append("{\"index\":").Append(b);
                json.Append(",\"group\":")
                    .Append(set.Tolerance != null && b < set.Tolerance.Group.Length
                            ? set.Tolerance.Group[b] : 0);
                json.Append(",\"members\":").Append(memberCount);
                json.Append(",\"leastInradius\":")
                    .Append(leastInradius.ToString("0.###", Invariant));
                json.Append(",\"residual\":")
                    .Append((100.0 * factory.ResidualFraction).ToString("0.#", Invariant));
                json.Append(",\"tubeRatio\":")
                    .Append(factory.MaximumTubeRatio.ToString("0.###", Invariant));
                json.Append(",\"length\":").Append(Num(factory.Length));
                json.Append(",\"head\":").Append(factory.SharedHead.Count);

                json.Append(",\"dogleg\":").Append(dogleg.ToString("0.##", Invariant));
                json.Append(",\"doglegLong\":").Append(doglegLong.ToString("0.##", Invariant));
                json.Append(",\"doglegBare\":").Append(doglegBare.ToString("0.##", Invariant));
                if (!double.IsNaN(leastFromParent))
                {
                    json.Append(",\"fromParent\":")
                        .Append(leastFromParent.ToString("0.##", Invariant));
                }

                // the median path, with the shared head in front of it so it starts at the slot
                json.Append(",\"median\":[");
                List<Point3D> spine = new List<Point3D>(factory.SharedHead);
                spine.AddRange(factory.GetMedianCurve().Positions!);
                int stride = System.Math.Max(1, spine.Count / 160);
                bool firstPoint = true;
                for (int i = 0; i < spine.Count; i += stride)
                {
                    if (!firstPoint) { json.Append(','); }
                    firstPoint = false;
                    json.Append(Point(new[] { spine[i].X!.Value, spine[i].Y!.Value, spine[i].Z!.Value },
                                      origin));
                }
                json.Append(']');

                // the outlines, as closed rings in the plane of their own cross-section
                json.Append(",\"outlines\":[");
                bool firstRing = true;
                for (int at = 0; at < factory.Stations.Count; at += OutlineStride)
                {
                    CrossSectionStation station = factory.Stations[at];
                    if (station.Polygon == null) { continue; }
                    if (!firstRing) { json.Append(','); }
                    firstRing = false;
                    json.Append("{\"inradius\":")
                        .Append(station.Polygon.Inradius.ToString("0.###", Invariant));
                    json.Append(",\"ring\":[");
                    int sectors = station.Polygon.DirectionCount;
                    for (int s = 0; s < sectors; s++)
                    {
                        if (s > 0) { json.Append(','); }
                        double angle = 2.0 * System.Math.PI * (s + 0.5) / sectors;
                        Point3D? at3D = station.GetPosition(1.0, angle);
                        if (at3D == null) { json.Append("[0,0,0]"); continue; }
                        json.Append(Point(new[] { at3D.X!.Value, at3D.Y!.Value, at3D.Z!.Value }, origin));
                    }
                    json.Append("]}");
                }
                json.Append(']');

                // and what the factory is for: streamlines drawn from it, holding none of the originals
                json.Append(",\"draws\":[");
                for (int d = 0; d < DrawsPerBundle; d++)
                {
                    Streamline? drawn = factory.Draw(random);
                    if (drawn == null || drawn.Count < 2) { continue; }
                    if (d > 0) { json.Append(','); }
                    List<Point3D> positions = drawn.Positions!;
                    int drawStride = System.Math.Max(1, positions.Count / 140);
                    json.Append('[');
                    bool firstDrawn = true;
                    for (int i = 0; i < positions.Count; i += drawStride)
                    {
                        if (!firstDrawn) { json.Append(','); }
                        firstDrawn = false;
                        json.Append(Point(new[] { positions[i].X!.Value, positions[i].Y!.Value,
                                                  positions[i].Z!.Value }, origin));
                    }
                    json.Append(']');
                }
                json.Append("]}");
            }
            json.Append(']');
        }

        /// <summary>
        /// the generation settings every case shares, so that a diagnostic and the scene cannot drift
        /// apart on what was actually run
        /// </summary>
        private static StreamlineGeneratorOptions BuildOptions(double floorVertical)
        {
            return new StreamlineGeneratorOptions
            {
                Grid = new StreamlineGridOptions
                {
                    FinestCellSize = 1.0,
                    CoarsestCellSize = 25.0,
                    CeilingVertical = Ground,
                    FloorVertical = floorVertical
                },
                StreamlineCount = Launches,
                ChannelContrast = 100.0,
                ChannelNarrowWidth = 25.0,
                ChannelWideWidth = 150.0,
                OutletGuideContrast = 20.0,
                OutletGuideLength = 120.0,
                OutletGuideWidth = 30.0,
                LaunchInset = 0.25,
                // the directive path now also arrives straight along the target's own direction for
                // its last stretch, so the cheap corridor points the right way where it matters
                Relaxer = new SpineRelaxerOptions { HoldLength = 40.0 }
            };
        }

        private static void AppendWells(StringBuilder json, Cluster cluster, double[] origin)
        {
            const int aroundCount = 14;
            json.Append(",\"wells\":[");
            for (int w = 0; w < cluster.Order.Count; w++)
            {
                WellboreUncertainty well = cluster.Wells[cluster.Order[w]];
                int stride = System.Math.Max(1, well.SampleCount / 80);
                if (w > 0) { json.Append(','); }
                json.Append("{\"name\":\"").Append(well.Name).Append("\",\"rings\":[");
                bool firstRing = true;
                for (int sample = 0; sample < well.SampleCount; sample += stride)
                {
                    if (!firstRing) { json.Append(','); }
                    firstRing = false;
                    json.Append('[');
                    for (int a = 0; a < aroundCount; a++)
                    {
                        if (a > 0) { json.Append(','); }
                        well.GetSectionPoint(sample, 2.0 * System.Math.PI * a / aroundCount,
                                             out double n, out double e, out double v);
                        json.Append(Point(new[] { n, e, v }, origin));
                    }
                    json.Append(']');
                }
                json.Append("]}");
            }
            json.Append(']');
        }

        private static string Point(double[] position, double[] origin)
        {
            return "[" + Num(position[0] - origin[0]) + "," + Num(position[1] - origin[1]) + ","
                   + Num(position[2]) + "]";
        }

        private static string Num(double value)
        {
            return value.ToString("0.###", Invariant);
        }
    }
}
