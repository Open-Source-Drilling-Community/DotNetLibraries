using System.Globalization;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.Streamlines.Generation;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.UnitTest
{
    /// <summary>
    /// What it takes for a constrained corridor to turn gently enough to be drilled.
    /// <para>
    /// Imposing a direction with closed faces is exact but abrupt: the constraint is a step, and a step in
    /// the allowed direction is a corner. These runs measure where the corner is, and whether a floor
    /// under the target and a longer landing section move it.
    /// </para>
    /// </summary>
    [TestFixture]
    public class UllriggLandingSweep
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

        /// <summary>
        /// how far the last stretch of a path doubles back on itself, m
        /// </summary>
        private static double GetClimbBeforeLanding(Streamline line, double over)
        {
            List<Point3D> positions = line.Positions!;
            double travelled = 0;
            double climb = 0;
            for (int i = positions.Count - 1; i > 0; i--)
            {
                double an = positions[i - 1].X!.Value, ae = positions[i - 1].Y!.Value,
                       av = positions[i - 1].Z!.Value;
                double bn = positions[i].X!.Value, be = positions[i].Y!.Value, bv = positions[i].Z!.Value;
                travelled += System.Math.Sqrt((bn - an) * (bn - an) + (be - ae) * (be - ae)
                                              + (bv - av) * (bv - av));
                if (bv < av) { climb += av - bv; }
                if (travelled > over) { break; }
            }
            return climb;
        }

        /// <summary>
        /// The worst dogleg of one path, whole and then with the last stretch left out, in degrees per
        /// 30 m. A path with too few stations to carry a turn at all is not a measurement and is left out.
        /// </summary>
        private static bool Measure(Streamline line, double ignoreLast,
                                    out double whole, out double body)
        {
            whole = 0;
            body = 0;
            List<double> profile = StreamlineCurvature.GetProfile(line);
            if (profile.Count < 4)
            {
                return false;
            }
            int keep = profile.Count - (int)(ignoreLast / StreamlineCurvature.StandardStation);
            for (int i = 0; i < profile.Count; i++)
            {
                double degrees = profile[i] * 180.0 / System.Math.PI;
                if (degrees > whole) { whole = degrees; }
                if (i < keep && degrees > body) { body = degrees; }
            }
            return true;
        }

        private static void Report(string label, StreamlineGenerationResult result)
        {
            List<double> whole = new List<double>();
            List<double> body = new List<double>();
            int doublingBack = 0;
            foreach (Streamline line in result.Streamlines)
            {
                if (!Measure(line, 150.0, out double w, out double b)) { continue; }
                whole.Add(w);
                body.Add(b);
                if (GetClimbBeforeLanding(line, 200.0) > 5.0) { doublingBack++; }
            }
            whole.Sort();
            body.Sort();
            TestContext.Progress.WriteLine($"  {label}");
            TestContext.Progress.WriteLine("      " + result.Describe());
            foreach ((int cells, double reach) in result.Conduits)
            {
                TestContext.Progress.WriteLine(
                    $"      conduit {cells} cells reaching {reach:F0} m");
            }
            TestContext.Progress.WriteLine(
                $"      {result.ArrivedCount,3} arrived, {whole.Count,2} measurable, "
                + $"doubling back {doublingBack,2}");
            TestContext.Progress.WriteLine(
                $"      whole path        gentlest {whole[0],6:F1}  median {whole[whole.Count / 2],6:F1}"
                + $"  sharpest {whole[whole.Count - 1],7:F1} deg/30m");
            TestContext.Progress.WriteLine(
                $"      last 150 m cut    gentlest {body[0],6:F1}  median {body[body.Count / 2],6:F1}"
                + $"  sharpest {body[body.Count - 1],7:F1} deg/30m");
            TestContext.Progress.WriteLine(
                $"      worst arrival off the normal {GetWorstTilt(result),7:F3} deg");
            if (result.ConduitPaths.Count > 0 && result.ConduitPaths[0].Count > 1)
            {
                List<double> attitudes = new List<double>();
                Point3D end = result.ConduitPaths[0][result.ConduitPaths[0].Count - 1];
                foreach (Streamline line in result.Streamlines)
                {
                    List<Point3D> positions = line.Positions!;
                    int from = -1;
                    for (int i = 0; i < positions.Count; i++)
                    {
                        if (Separation(positions[i], end) < 1.0) { from = i; break; }
                    }
                    if (from < 0) { continue; }
                    double travelled = 0;
                    int j = from;
                    while (j + 1 < positions.Count && travelled < 100.0)
                    {
                        travelled += Separation(positions[j], positions[j + 1]);
                        j++;
                    }
                    if (j == from) { continue; }
                    double dn = positions[j].X!.Value - positions[from].X!.Value;
                    double de = positions[j].Y!.Value - positions[from].Y!.Value;
                    double dv = positions[j].Z!.Value - positions[from].Z!.Value;
                    double length = System.Math.Sqrt(dn * dn + de * de + dv * dv);
                    if (length > 0)
                    {
                        attitudes.Add(System.Math.Acos(System.Math.Max(-1, System.Math.Min(1, dv / length)))
                                      * 180.0 / System.Math.PI);
                    }
                }
                attitudes.Sort();
                if (attitudes.Count > 0)
                {
                    TestContext.Progress.WriteLine(
                        $"      inclination 100 m past the outlet: median "
                        + $"{attitudes[attitudes.Count / 2],5:F1}, min {attitudes[0],5:F1}, "
                        + $"max {attitudes[attitudes.Count - 1],5:F1} deg");
                }
            }
            // how many of the alternatives are gentle enough to be worth anything, which is the number
            // that matters: a corridor is a set of candidates, not one path
            foreach (double limit in new[] { 4.0, 6.0, 10.0, 20.0 })
            {
                int within = whole.Count(w => w <= limit);
                TestContext.Progress.WriteLine(
                    $"      within {limit,4:F0} deg/30m   {within,3} of {whole.Count}");
            }
            TestContext.Progress.WriteLine(
                "      sorted  " + string.Join(" ", whole.Take(12).Select(w => $"{w:F1}")));
            ReportByStation(result);
        }

        /// <summary>
        /// The turn per station, normalised to degrees per 30 m, over a range of station lengths.
        /// <para>
        /// A path of real curvature turns the same amount per unit length however long a window it is
        /// measured over, so normalising and sweeping the window separates curvature from a corner: a
        /// corner is a fixed angle at a point and its normalised value falls as one over the window.
        /// </para>
        /// </summary>
        private static void ReportByStation(StreamlineGenerationResult result)
        {
            foreach (double station in new[] { 15.0, 30.0, 60.0, 120.0, 240.0 })
            {
                List<double> every = new List<double>();
                List<double> perPath = new List<double>();
                for (int i = 0; i < result.Streamlines.Count; i++)
                {
                    if (i < result.StreamlineOutcomes.Count
                        && result.StreamlineOutcomes[i] != TraceOutcome.ReachedSink)
                    {
                        continue;
                    }
                    List<double> profile = StreamlineCurvature.GetProfile(result.Streamlines[i], station);
                    if (profile.Count < 2) { continue; }
                    double worst = 0;
                    foreach (double turn in profile)
                    {
                        double scaled = turn * 180.0 / System.Math.PI * 30.0 / station;
                        every.Add(scaled);
                        if (scaled > worst) { worst = scaled; }
                    }
                    perPath.Add(worst);
                }
                if (every.Count == 0) { continue; }
                every.Sort();
                perPath.Sort();
                TestContext.Progress.WriteLine(
                    $"      {station,5:F0} m station: median {every[every.Count / 2],6:F2}"
                    + $"  90th {every[(int)(0.9 * every.Count)],6:F2}"
                    + $"  worst {every[every.Count - 1],7:F2}"
                    + $"  gentlest path {perPath[0],6:F2}");
            }
        }

        private static double Separation(Point3D a, Point3D b)
        {
            return System.Math.Sqrt(System.Math.Pow(a.X!.Value - b.X!.Value, 2)
                                    + System.Math.Pow(a.Y!.Value - b.Y!.Value, 2)
                                    + System.Math.Pow(a.Z!.Value - b.Z!.Value, 2));
        }

        /// <summary>
        /// the spine the guide follows, and what it would cost to drill it
        /// </summary>
        private static void ReportSpine(double[] u1, ObstacleField field)
        {
            List<Point3D> check = ReferenceSpine.Sample(
                new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]), new Vector3D(0, 0, 1),
                new Point3D(u1[0] - 400.0, u1[1] + 400.0, TargetVertical), new Vector3D(0, 0, 1), 513);
            double travelled = 0;
            double firstHit = -1;
            double worstInside = 0;
            int inside = 0;
            for (int i = 0; i < check.Count; i++)
            {
                if (i > 0)
                {
                    travelled += System.Math.Sqrt(
                        System.Math.Pow(check[i].X!.Value - check[i - 1].X!.Value, 2)
                        + System.Math.Pow(check[i].Y!.Value - check[i - 1].Y!.Value, 2)
                        + System.Math.Pow(check[i].Z!.Value - check[i - 1].Z!.Value, 2));
                }
                double excess = field.GetNearestExcess(check[i].X!.Value, check[i].Y!.Value,
                                                       check[i].Z!.Value, 500.0);
                if (excess < 0)
                {
                    inside++;
                    if (firstHit < 0) { firstHit = travelled; }
                    if (-excess > worstInside) { worstInside = -excess; }
                }
            }
            TestContext.Progress.WriteLine(
                firstHit < 0
                ? "  spine clears every uncertainty volume"
                : $"  spine enters an uncertainty volume at {firstHit:F0} m along, "
                  + $"{inside} of {check.Count} samples inside, up to {worstInside:F1} m deep");
        }

        private static void ReportSpineCurvature(double[] u1)
        {
            List<Point3D> spine = ReferenceSpine.Sample(
                new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]), new Vector3D(0, 0, 1),
                new Point3D(u1[0] - 400.0, u1[1] + 400.0, TargetVertical), new Vector3D(0, 0, 1));
            double worst = StreamlineCurvature.GetWorst(new Streamline(spine)) * 180.0 / System.Math.PI;
            double length = 0;
            for (int i = 1; i < spine.Count; i++)
            {
                length += System.Math.Sqrt(
                    System.Math.Pow(spine[i].X!.Value - spine[i - 1].X!.Value, 2)
                    + System.Math.Pow(spine[i].Y!.Value - spine[i - 1].Y!.Value, 2)
                    + System.Math.Pow(spine[i].Z!.Value - spine[i - 1].Z!.Value, 2));
            }
            TestContext.Progress.WriteLine(
                $"  spine: {length:F0} m long, worst dogleg {worst:F2} deg/30m");
            TestContext.Progress.WriteLine("");
        }

        /// <summary>
        /// one generation guided along a spine running the whole way
        /// </summary>
        /// <summary>
        /// one generation with both conduits shaped to the reference spine
        /// </summary>
        private static StreamlineGenerationResult RunShaped(ObstacleField field, double[] u1,
                                                            double conduit, double landing,
                                                            bool shapeLanding = true,
                                                            bool twoPass = false)
        {
            const double half = 80.0;
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 + half, TargetVertical),
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 + half, TargetVertical)
            };
            TargetPolygon target = new TargetPolygon(corners, 20.0)
            {
                Incidence = TargetIncidence.Perpendicular,
                Sides = TargetSides.One,
                ApproachDirection = new Vector3D(0, 0, 1),
                LandingLength = landing
            };
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = conduit
            };
            StreamlineGeneratorOptions settings = new StreamlineGeneratorOptions
            {
                Grid = new StreamlineGridOptions
                {
                    FinestCellSize = 1.0,
                    CoarsestCellSize = 25.0,
                    CeilingVertical = Ground,
                    FloorVertical = TargetVertical + 40.0
                },
                StreamlineCount = 36,
                ShapeDepartureToSpine = true,
                ShapeLandingToSpine = shapeLanding
            };
            return twoPass
                   ? StreamlineGenerator.GenerateShaped(field, new[] { slot }, target, settings)
                   : StreamlineGenerator.Generate(field, new[] { slot }, target, settings);
        }

        private static StreamlineGenerationResult RunSpine(ObstacleField field, double[] u1,
                                                           double contrast, double width)
        {
            const double half = 80.0;
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 + half, TargetVertical),
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 + half, TargetVertical)
            };
            TargetPolygon target = new TargetPolygon(corners, 20.0)
            {
                Sides = TargetSides.One,
                ApproachDirection = new Vector3D(0, 0, 1)
            };
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = 100.0
            };
            return StreamlineGenerator.Generate(field, new[] { slot }, target,
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = 1.0,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = Ground,
                        FloorVertical = TargetVertical + 40.0
                    },
                    StreamlineCount = 36,
                    SpineGuideContrast = contrast,
                    SpineGuideWidth = width
                });
        }

        private static StreamlineGenerationResult RunGuided(ObstacleField field, double[] u1,
                                                            double landing, double contrast,
                                                            double sourceContrast = -1)
        {
            if (sourceContrast < 0) { sourceContrast = contrast; }
            const double half = 80.0;
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 + half, TargetVertical),
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 + half, TargetVertical)
            };
            TargetPolygon target = new TargetPolygon(corners, 20.0)
            {
                Incidence = contrast > 1.0 ? TargetIncidence.Guided : TargetIncidence.Free,
                Sides = TargetSides.One,
                ApproachDirection = new Vector3D(0, 0, 1),
                LandingLength = landing,
                GuideContrast = System.Math.Max(contrast, 1.0001)
            };
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = 100.0
            };
            return StreamlineGenerator.Generate(field, new[] { slot }, target,
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = 1.0,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = Ground,
                        FloorVertical = TargetVertical + 40.0
                    },
                    StreamlineCount = 36,
                    SourceGuideContrast = sourceContrast,
                    SourceGuideLength = 300.0,
                    SourceGuideWidth = 150.0
                });
        }

        /// <summary>
        /// the worst departure from the target normal over all arrivals, degrees
        /// </summary>
        private static double GetWorstTilt(StreamlineGenerationResult result)
        {
            double worst = 0;
            foreach (Streamline line in result.Streamlines)
            {
                List<Point3D> positions = line.Positions!;
                for (int k = positions.Count - 1; k > 0; k--)
                {
                    double dn = positions[k].X!.Value - positions[k - 1].X!.Value;
                    double de = positions[k].Y!.Value - positions[k - 1].Y!.Value;
                    double dv = positions[k].Z!.Value - positions[k - 1].Z!.Value;
                    if (!(System.Math.Sqrt(dn * dn + de * de + dv * dv) > 1.0e-9)) { continue; }
                    if (dv > 0)
                    {
                        double tilt = System.Math.Atan2(System.Math.Sqrt(dn * dn + de * de), dv)
                                      * 180.0 / System.Math.PI;
                        if (tilt > worst) { worst = tilt; }
                    }
                    break;
                }
            }
            return worst;
        }

        /// <summary>
        /// The same problem with nothing in the way at all: the slot, the target, a conduit, and a
        /// channel of easier medium along the plain curve between the two ends. No relaxation is wanted
        /// because there is nothing to relax away from.
        /// <para>
        /// This is the baseline that says how much of the corridor's curvature is the method and how
        /// much is the cluster.
        /// </para>
        /// </summary>
        private static StreamlineGenerationResult RunClean(double[] u1, double channel, double outlet,
                                                           double half = 80.0, int count = 36,
                                                           double inset = 0.0)
        {
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 + half, TargetVertical),
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 + half, TargetVertical)
            };
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = 200.0
            };
            return StreamlineGenerator.Generate(new ObstacleField(), new[] { slot },
                new TargetPolygon(corners, 20.0),
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = 1.0,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = Ground,
                        FloorVertical = TargetVertical + 40.0
                    },
                    StreamlineCount = count,
                    ChannelContrast = channel,
                    ChannelNarrowWidth = 25.0,
                    ChannelWideWidth = 150.0,
                    OutletGuideContrast = outlet,
                    OutletGuideLength = 120.0,
                    OutletGuideWidth = 30.0,
                    LaunchInset = inset
                });
        }

        /// <summary>
        /// the bounded case with a free arrival, over a range of conduit lengths
        /// </summary>
        private static StreamlineGenerationResult RunConduit(ObstacleField field, double[] u1,
                                                             double conduit)
        {
            const double half = 80.0;
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 + half, TargetVertical),
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 + half, TargetVertical)
            };
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = conduit
            };
            return StreamlineGenerator.Generate(field, new[] { slot },
                new TargetPolygon(corners, 20.0),
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = 1.0,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = Ground,
                        FloorVertical = TargetVertical + 40.0
                    },
                    StreamlineCount = 36
                });
        }

        /// <summary>
        /// the bounded case with a scalar channel of raised permeability following the spine
        /// </summary>
        private static StreamlineGenerationResult RunChannel(ObstacleField field, double[] u1,
                                                             double contrast, double narrow,
                                                             double wide, bool scouted = false)
        {
            const double half = 80.0;
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 + half, TargetVertical),
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 + half, TargetVertical)
            };
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = 200.0
            };
            TargetPolygon target = new TargetPolygon(corners, 20.0);
            StreamlineGeneratorOptions settings = new StreamlineGeneratorOptions
            {
                Grid = new StreamlineGridOptions
                {
                    FinestCellSize = 1.0,
                    CoarsestCellSize = 25.0,
                    CeilingVertical = Ground,
                    FloorVertical = TargetVertical + 40.0
                },
                StreamlineCount = 36,
                ChannelContrast = contrast,
                ChannelNarrowWidth = narrow,
                ChannelWideWidth = wide
            };
            return scouted
                   ? StreamlineGenerator.GenerateShaped(field, new[] { slot }, target, settings)
                   : StreamlineGenerator.Generate(field, new[] { slot }, target, settings);
        }

        /// <summary>
        /// scout, bundle the ways round, relax one spine per bundle, channel on all of them
        /// </summary>
        private static StreamlineGenerationResult RunChannelled(ObstacleField field, double[] u1,
                                                                double contrast, double narrow,
                                                                double wide, int spines,
                                                                double conduit = 200.0,
                                                                double outletContrast = 1.0,
                                                                double finest = 1.0,
                                                                double inset = 0.25)
        {
            const double half = 80.0;
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 + half, TargetVertical),
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 + half, TargetVertical)
            };
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = conduit
            };
            StreamlineGenerationResult result = StreamlineGenerator.GenerateChannelled(
                field, new[] { slot }, new TargetPolygon(corners, 20.0),
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = finest,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = Ground,
                        FloorVertical = TargetVertical + 40.0
                    },
                    StreamlineCount = 36,
                    ChannelContrast = contrast,
                    ChannelNarrowWidth = narrow,
                    ChannelWideWidth = wide,
                    OutletGuideContrast = outletContrast,
                    OutletGuideLength = 120.0,
                    OutletGuideWidth = 30.0,
                    LaunchInset = inset,
                    Relaxer = new SpineRelaxerOptions { HoldLength = conduit }
                }, spines);
            for (int i = 0; i < result.SpineQuality.Count; i++)
            {
                TestContext.Progress.WriteLine($"      spine {i}: " + result.SpineQuality[i].Describe());
            }
            return result;
        }

        private static StreamlineGenerationResult Run(ObstacleField field, double[] u1,
                                                      double landing, bool floor, bool free = false)
        {
            const double half = 80.0;
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 - half, TargetVertical),
                new Point3D(u1[0] - 400.0 + half, u1[1] + 400.0 + half, TargetVertical),
                new Point3D(u1[0] - 400.0 - half, u1[1] + 400.0 + half, TargetVertical)
            };
            TargetPolygon target = new TargetPolygon(corners, 20.0);
            if (!free)
            {
                target.Incidence = TargetIncidence.Perpendicular;
                target.Sides = TargetSides.One;
                target.ApproachDirection = new Vector3D(0, 0, 1);
                target.LandingLength = landing;
            }
            StreamlineSource slot = new StreamlineSource(new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]),
                                                         new Vector3D(0, 0, 1))
            {
                ConduitLength = 100.0
            };
            return StreamlineGenerator.Generate(field, new[] { slot }, target,
                new StreamlineGeneratorOptions
                {
                    Grid = new StreamlineGridOptions
                    {
                        FinestCellSize = 1.0,
                        CoarsestCellSize = 25.0,
                        CeilingVertical = Ground,
                        FloorVertical = floor ? TargetVertical + 40.0 : null
                    },
                    StreamlineCount = 36
                });
        }

        /// <summary>
        /// Invoke with <c>dotnet test --filter "FullyQualifiedName~TheLandingLengthSweep"</c>.
        /// </summary>
        [Test]
        [Explicit("several full generations on the real data")]
        public void TheLandingLengthSweep()
        {
            ObstacleField field = LoadUllrigg(out double[] u1);

            // the least turning the geometry can ask for, before any medium is involved: leaving the slot
            // vertical and arriving on the target normal, both ends stand off the straight line between
            // them, and a dogleg limit turns that into a least length of hole
            double least = StreamlineCurvature.GetLeastTurn(
                new Point3D(u1[0] + 1.2, u1[1] + 1.2, u1[2]), new Vector3D(0, 0, 1),
                new Point3D(u1[0] - 400.0, u1[1] + 400.0, TargetVertical), new Vector3D(0, 0, 1));
            double degrees = least * 180.0 / System.Math.PI;
            TestContext.Progress.WriteLine(
                $"least possible turning {degrees:F1} deg, which at 3 deg/30 m needs "
                + $"{30.0 * degrees / 3.0:F0} m of hole, and at 2 deg/30 m needs "
                + $"{30.0 * degrees / 2.0:F0} m");
            TestContext.Progress.WriteLine("");

            ReportSpineCurvature(u1);
            ReportSpine(u1, field);
            foreach (double inset in new[] { 0.0, 0.25, 0.35 })
            {
                Report($"no wells, disc inset {inset:F2}", RunClean(u1, 100.0, 20.0, 80.0, 36, inset));
            }
            foreach (double inset in new[] { 0.0, 0.25, 0.35 })
            {
                Report($"with the wells, disc inset {inset:F2}",
                       RunChannelled(field, u1, 100.0, 25.0, 150.0, 1, 200.0, 20.0, 1.0, inset));
            }


            Assert.Pass();
        }
    }
}
