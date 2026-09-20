using System.Diagnostics;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// The generation grid on the thirteen Ullrigg trajectories.
    /// <para>
    /// The survey files carry only measured depth, inclination and azimuth, so the ellipses come from a
    /// nominal model and the wellheads from a synthetic three metre slot grid. Neither is the real thing,
    /// and the point of these is the size and the behaviour of the grid rather than any statement about
    /// Ullrigg itself.
    /// </para>
    /// </summary>
    [TestFixture]
    public class UllriggGenerationTests
    {
        private static string GetSurveyDirectory()
        {
            string directory = TestContext.CurrentContext.TestDirectory;
            string candidate = Path.Combine(directory, "UllriggSurveys");
            Assert.That(Directory.Exists(candidate), Is.True, $"no survey data at {candidate}");
            return candidate;
        }

        /// <summary>
        /// the thirteen wells, with wellheads on a three metre grid and a relative uncertainty model
        /// </summary>
        private static ObstacleField LoadUllrigg(double growth = 0.001, double boreholeRadius = 0.25)
        {
            NominalUncertaintyModel model = new NominalUncertaintyModel
            {
                BoreholeRadius = boreholeRadius,
                Growth = growth,
                Anisotropy = 1.5
            };
            ObstacleField field = new ObstacleField();
            string[] files = Directory.GetFiles(GetSurveyDirectory(), "*.txt");
            Array.Sort(files, StringComparer.Ordinal);
            for (int index = 0; index < files.Length; index++)
            {
                List<WellboreUncertaintyStation> stations =
                    SurveyFileReader.ReadWithNominalUncertainty(files[index], model);
                if (stations.Count < 2)
                {
                    continue;
                }
                field.Add(new WellboreUncertainty(stations, 3.0 * (index % 4), 3.0 * (index / 4), 0, 2.0)
                {
                    Name = Path.GetFileNameWithoutExtension(files[index])
                });
            }
            field.BuildIndex();
            return field;
        }

        private static TargetPolygon Target(double north, double east, double vertical, double half)
        {
            List<Point3D> vertices = new List<Point3D>
            {
                new Point3D(north - half, east - half, vertical),
                new Point3D(north + half, east - half, vertical),
                new Point3D(north + half, east + half, vertical),
                new Point3D(north - half, east + half, vertical)
            };
            return new TargetPolygon(vertices, 20.0);
        }

        [Test]
        public void TheSurveysAreReadAndTheVolumesAreWellFormed()
        {
            ObstacleField field = LoadUllrigg();
            Assert.That(field.Wells.Count, Is.EqualTo(13));
            foreach (WellboreUncertainty well in field.Wells)
            {
                Assert.That(well.SampleCount, Is.GreaterThan(10), well.Name);
                Assert.That(well.MaximumSemiAxis, Is.GreaterThan(0), well.Name);
                // the swept ellipse is a solid only while the section stays inside the radius of curvature
                Assert.That(well.MaximumSweepRatio, Is.LessThan(1.0),
                            $"{well.Name} folds over itself, ratio {well.MaximumSweepRatio:F3}");
            }
            Assert.That(field.BoundingBoxMaximum[2] - field.BoundingBoxMinimum[2], Is.GreaterThan(1000.0),
                        "the deepest well reaches beyond a kilometre");
        }

        [Test]
        public void ANewSlotInTheClusterReachesATargetBelow()
        {
            ObstacleField field = LoadUllrigg();
            // a new slot in the middle of the existing ones, starting vertically
            StreamlineSource slot = new StreamlineSource(new Point3D(4.5, 1.5, 0.0), new Vector3D(0, 0, 1));
            StreamlineGrid grid = StreamlineGrid.Build(field, new[] { slot },
                                                       Target(-200.0, 300.0, 900.0, 60.0),
                                                       new StreamlineGridOptions
                                                       {
                                                           FinestCellSize = 0.5,
                                                           CoarsestCellSize = 25.0
                                                       });
            Assert.That(grid.Status, Is.EqualTo(StreamlineGridStatus.Connected), grid.Describe());
            Assert.That(grid.BlockedCount, Is.GreaterThan(0), "the existing wells must block something");
            TestContext.Progress.WriteLine("new slot: " + grid.Describe());
        }

        [Test]
        public void ASidetrackOffAnExistingWellCanStart()
        {
            ObstacleField field = LoadUllrigg();
            int parent = -1;
            for (int w = 0; w < field.Wells.Count; w++)
            {
                if (field.Wells[w].Name != null && field.Wells[w].Name!.StartsWith("U2", StringComparison.Ordinal))
                {
                    parent = w;
                }
            }
            Assert.That(parent, Is.GreaterThanOrEqualTo(0), "U2 is the deepest well and the one to leave from");

            // the tie-in is a position on the parent, which is inside its own uncertainty volume
            WellboreUncertainty well = field.Wells[parent];
            int sample = well.SampleCount / 2;
            well.GetSample(sample, out double n, out double e, out double v);
            StreamlineSource tieIn = new StreamlineSource(new Point3D(n, e, v), new Vector3D(0, 1, 1))
            {
                ParentWell = parent,
                ParentMeasuredDepth = well.GetMeasuredDepth(sample),
                ParentMutedLength = 30.0
            };
            StreamlineGrid grid = StreamlineGrid.Build(field, new[] { tieIn },
                                                       Target(n - 150.0, e + 250.0, v + 300.0, 60.0),
                                                       new StreamlineGridOptions
                                                       {
                                                           FinestCellSize = 0.5,
                                                           CoarsestCellSize = 25.0
                                                       });
            Assert.That(grid.Status, Is.Not.EqualTo(StreamlineGridStatus.SourceBlocked),
                        "muting the parent should have opened the window: " + grid.Describe());
            Assert.That(grid.Status, Is.EqualTo(StreamlineGridStatus.Connected), grid.Describe());
            TestContext.Progress.WriteLine("sidetrack: " + grid.Describe());
        }

        [Test]
        public void TheGridStaysASizeAWorkstationCanHold()
        {
            ObstacleField field = LoadUllrigg();
            StreamlineSource slot = new StreamlineSource(new Point3D(4.5, 1.5, 0.0), new Vector3D(0, 0, 1));
            Stopwatch watch = Stopwatch.StartNew();
            StreamlineGrid grid = StreamlineGrid.Build(field, new[] { slot },
                                                       Target(-200.0, 300.0, 900.0, 60.0),
                                                       new StreamlineGridOptions
                                                       {
                                                           FinestCellSize = 0.5,
                                                           CoarsestCellSize = 25.0
                                                       });
            watch.Stop();
            TestContext.Progress.WriteLine($"built in {watch.ElapsedMilliseconds} ms: {grid.Describe()}");
            Assert.That(grid.Tree.LeafCount, Is.LessThan(5_000_000),
                        "the octree should stay in the low millions on a site this size");
        }

        /// <summary>
        /// Not part of the ordinary run: how the size of the uncertainty volumes decides whether the
        /// cluster is passable at all. Invoke with
        /// <c>dotnet test --filter "FullyQualifiedName~TheUncertaintyModelDecidesPassability"</c>.
        /// </summary>
        [Test]
        [Explicit("measures the effect of the uncertainty model")]
        public void TheUncertaintyModelDecidesPassability()
        {
            foreach (double growth in new[] { 0.0005, 0.001, 0.002, 0.005 })
            {
                ObstacleField field = LoadUllrigg(growth);
                StreamlineSource slot = new StreamlineSource(new Point3D(4.5, 1.5, 0.0), new Vector3D(0, 0, 1));
                Stopwatch watch = Stopwatch.StartNew();
                StreamlineGrid grid = StreamlineGrid.Build(field, new[] { slot },
                                                           Target(-200.0, 300.0, 900.0, 60.0),
                                                           new StreamlineGridOptions
                                                           {
                                                               FinestCellSize = 0.5,
                                                               CoarsestCellSize = 25.0
                                                           });
                watch.Stop();
                TestContext.Progress.WriteLine(
                    $"growth {growth * 100:F2} % of MD, {watch.ElapsedMilliseconds,6} ms: {grid.Describe()}");
            }
        }
    }
}
