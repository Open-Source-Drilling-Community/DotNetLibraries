using System.Globalization;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// Why a target standing at eighty five degrees builds its through conduits and then reports that
    /// nothing can reach a sink.
    /// <para>
    /// The connectivity flood lets a cell of a conduit be entered only from the two cells that continue
    /// its chain, and it finds neighbours by their shared faces. A chain whose consecutive cells are not
    /// face adjacent is therefore severed: nothing downstream of the break is reachable, however open the
    /// cells themselves are. This measures whether that is what has happened.
    /// </para>
    /// </summary>
    [TestFixture]
    public class ThroughConduitDiagnosis
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
        /// whether the two leaves share a face, which is the only way the flood can pass between them
        /// </summary>
        private static bool AreFaceAdjacent(StreamlineGrid grid, int first, int second)
        {
            List<int> neighbours = new List<int>();
            for (int axis = 0; axis < 3; axis++)
            {
                for (int step = -1; step <= 1; step += 2)
                {
                    neighbours.Clear();
                    grid.Tree.GetFaceNeighbours(first, axis, step, neighbours);
                    if (neighbours.Contains(second))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~WhyTheInclinedTargetWillNotConnect"</c>.
        /// </summary>
        [Test]
        [Explicit("builds the grid and the problem for the inclined case; no solve")]
        public void WhyTheInclinedTargetWillNotConnect()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);
            foreach (double inclinationDegrees in new[] { 0.0, 85.0 })
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
                double[] across = { -System.Math.Sin(azimuth), System.Math.Cos(azimuth), 0 };
                double[] down =
                {
                    normal[1] * across[2] - normal[2] * across[1],
                    normal[2] * across[0] - normal[0] * across[2],
                    normal[0] * across[1] - normal[1] * across[0]
                };
                List<Point3D> corners = new List<Point3D>();
                foreach ((double a, double b) in new[] { (-1.0, -1.0), (1.0, -1.0), (1.0, 1.0),
                                                         (-1.0, 1.0) })
                {
                    corners.Add(new Point3D(
                        targetAt[0] + TargetHalf * (a * across[0] + b * down[0]),
                        targetAt[1] + TargetHalf * (a * across[1] + b * down[1]),
                        targetAt[2] + TargetHalf * (a * across[2] + b * down[2])));
                }
                double floorVertical = TargetVertical + TargetHalf * System.Math.Abs(down[2]) + 40.0;

                TargetPolygon target = new TargetPolygon(corners, 20.0)
                {
                    Incidence = TargetIncidence.Through,
                    ThroughLength = 60.0
                };
                StreamlineSource slot = new StreamlineSource(
                    new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]), new Vector3D(0, 0, 1))
                {
                    ConduitLength = 40.0
                };
                StreamlineGenerationResult got = StreamlineGenerator.Generate(
                    field, new[] { slot }, target,
                    new StreamlineGeneratorOptions
                    {
                        Grid = new StreamlineGridOptions
                        {
                            FinestCellSize = 1.0,
                            CoarsestCellSize = 25.0,
                            CeilingVertical = Ground,
                            FloorVertical = floorVertical
                        },
                        StreamlineCount = 4
                    });

                TestContext.Progress.WriteLine(
                    $"=== {inclinationDegrees:0} deg: {got.Status}, {got.ServedTargetCellCount} served ===");
                StreamlineGrid? grid = got.Grid;
                FlowProblem? problem = got.Problem;
                if (grid == null || problem == null)
                {
                    TestContext.Progress.WriteLine("  no grid or problem to look at");
                    continue;
                }

                int chains = 0;
                int severed = 0;
                long links = 0;
                long broken = 0;
                int shortest = int.MaxValue;
                int longest = 0;
                foreach (FlowConduit conduit in problem.Conduits)
                {
                    int[] cells = conduit.Cells;
                    if (cells.Length < 2) { continue; }
                    chains++;
                    if (cells.Length < shortest) { shortest = cells.Length; }
                    if (cells.Length > longest) { longest = cells.Length; }
                    bool whole = true;
                    for (int k = 0; k + 1 < cells.Length; k++)
                    {
                        links++;
                        if (!AreFaceAdjacent(grid, cells[k], cells[k + 1]))
                        {
                            broken++;
                            whole = false;
                        }
                    }
                    if (!whole) { severed++; }
                }
                TestContext.Progress.WriteLine(
                    $"  {chains} chains, {shortest} to {longest} cells, {links} links of which"
                    + $" {broken} are not face adjacent, so {severed} chains are severed");
            }
        }
    }
}
