using OSDC.DotnetLibraries.General.Math;
using System.Globalization;

namespace OSDC.DotnetLibraries.Drilling.Surveying.UnitTest
{
    /// <summary>
    /// The reduced order trajectory method run over the Ullrigg test wells.
    ///
    /// These are short, shallow, heavily surveyed holes, which is the opposite end of the range from the
    /// North Sea wells the golden vectors were generated on: a few tens to a couple of hundred stations
    /// over a few hundred metres, stations as close as a metre apart in places, and a great deal of
    /// near vertical hole where the azimuth means very little. They carry no reference answer, so nothing
    /// here compares against one. What is checked is what the method promises regardless of the well:
    /// that every section holds the angular tolerance it was cut at, that the refinement never returns
    /// something worse than what it started from, that the deviation a caller measures is the deviation
    /// the reduction reports, that no azimuth carrying curve is used across the vertical, and that the
    /// same survey always gives the same answer.
    ///
    /// The numbers each run achieves are written to the test output rather than asserted, since there is
    /// nothing to assert them against; they are there to be read.
    /// </summary>
    public class ReducedOrderTrajectoryUllriggTests
    {
        /// <summary>
        /// The tolerances asked for, in degrees.
        /// </summary>
        private static readonly double[] TolerancesInDegrees = { 0.5, 1.0, 2.0, 3.0 };

        /// <summary>
        /// One Ullrigg survey, as read from its file.
        /// </summary>
        private class UllriggSurvey
        {
            internal string Name = string.Empty;
            internal double[] MeasuredDepths = System.Array.Empty<double>();
            internal double[] Inclinations = System.Array.Empty<double>();
            internal double[] Azimuths = System.Array.Empty<double>();
            internal double[] Tangents = System.Array.Empty<double>();
            internal double[] Positions = System.Array.Empty<double>();
        }

        /// <summary>
        /// Reads the surveys shipped beside the tests.
        ///
        /// The files are three tab separated columns, measured depth in metres and inclination and
        /// azimuth in degrees, with no header. Most were written on a Norwegian machine and use a comma
        /// for the decimal point; one of them uses a full stop, so both are accepted. The azimuths are
        /// written inconsistently as well, some of them negative and some past a full turn, which is
        /// exactly what the unwrapping in section 2 of the specification is there to absorb.
        /// </summary>
        private static List<UllriggSurvey> LoadSurveys()
        {
            string folder = Path.Combine(TestContext.CurrentContext.TestDirectory, "UllriggSurveys");
            Assert.That(Directory.Exists(folder), Is.True, $"Survey folder not found at {folder}.");
            List<UllriggSurvey> surveys = new();
            foreach (string path in Directory.GetFiles(folder, "*.txt").OrderBy(f => f, System.StringComparer.Ordinal))
            {
                List<double> measuredDepths = new();
                List<double> inclinations = new();
                List<double> azimuths = new();
                foreach (string line in File.ReadAllLines(path))
                {
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0)
                    {
                        continue;
                    }
                    string[] fields = trimmed.Split(new[] { '\t', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (fields.Length < 3)
                    {
                        continue;
                    }
                    if (!TryParse(fields[0], out double measuredDepth)
                        || !TryParse(fields[1], out double inclination)
                        || !TryParse(fields[2], out double azimuth))
                    {
                        continue;
                    }
                    measuredDepths.Add(measuredDepth);
                    inclinations.Add(inclination * System.Math.PI / 180.0);
                    azimuths.Add(azimuth * System.Math.PI / 180.0);
                }

                UllriggSurvey survey = new()
                {
                    Name = Path.GetFileNameWithoutExtension(path).Replace("-MD-Incl-Az", string.Empty),
                    MeasuredDepths = measuredDepths.ToArray(),
                    Inclinations = inclinations.ToArray(),
                    Azimuths = azimuths.ToArray()
                };
                Assert.That(survey.MeasuredDepths.Length, Is.GreaterThan(1),
                    $"{survey.Name} carries no usable stations.");

                // The azimuths have to be made continuous before the tangents are taken, exactly as the
                // reduction does internally, or the checks below would be measuring a different survey.
                for (int i = 1; i < survey.Azimuths.Length; i++)
                {
                    while (survey.Azimuths[i] - survey.Azimuths[i - 1] > System.Math.PI)
                    {
                        survey.Azimuths[i] -= 2.0 * System.Math.PI;
                    }
                    while (survey.Azimuths[i] - survey.Azimuths[i - 1] < -System.Math.PI)
                    {
                        survey.Azimuths[i] += 2.0 * System.Math.PI;
                    }
                }
                survey.Tangents = new double[3 * survey.MeasuredDepths.Length];
                ReducedOrderTrajectory.TangentsFromAngles(survey.Inclinations, survey.Azimuths, survey.Tangents);
                survey.Positions = new double[3 * survey.MeasuredDepths.Length];
                ReducedOrderTrajectory.MinimumCurvature(survey.MeasuredDepths, survey.Tangents, survey.Positions);
                surveys.Add(survey);
            }
            Assert.That(surveys, Is.Not.Empty, "No Ullrigg surveys were read.");
            return surveys;
        }

        private static bool TryParse(string field, out double value)
        {
            return double.TryParse(field.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Every Ullrigg survey reduced at every tolerance, with the promises of the method checked on
        /// each and the numbers achieved written out.
        /// </summary>
        [Test]
        public void Test82UllriggSurveysReduceAtEveryTolerance()
        {
            List<UllriggSurvey> surveys = LoadSurveys();
            List<string> failures = new();
            List<string> rows = new();
            rows.Add(string.Format(CultureInfo.InvariantCulture,
                "{0,-14}{1,7}{2,9}{3,7}{4,7}{5,9}{6,9}{7,9}{8,9}",
                "well", "stns", "MD (m)", "tol", "sects", "max (m)", "rms (m)", "dIncl", "dAzi"));

            foreach (UllriggSurvey survey in surveys)
            {
                int stationCount = survey.MeasuredDepths.Length;
                double totalDepth = survey.MeasuredDepths[stationCount - 1];
                foreach (double degrees in TolerancesInDegrees)
                {
                    double tolerance = degrees * System.Math.PI / 180.0;
                    string what = $"{survey.Name} at {degrees} deg";

                    ReducedOrderTrajectory reduced = ReducedOrderTrajectory.Reduce(
                        survey.MeasuredDepths, survey.Inclinations, survey.Azimuths, tolerance);
                    if (!reduced.IsValid)
                    {
                        failures.Add($"{what}: {reduced.FailureDescription}");
                        continue;
                    }

                    // A reduction is not allowed to be longer than the survey it replaces.
                    if (reduced.SectionCount > stationCount - 1)
                    {
                        failures.Add($"{what}: {reduced.SectionCount} sections for {stationCount} stations");
                    }
                    if (reduced.StationCount != stationCount)
                    {
                        failures.Add($"{what}: {reduced.StationCount} stations kept out of {stationCount}");
                    }

                    // The sections have to join up and to cover the whole survey.
                    if (reduced.SectionCount > 0)
                    {
                        AssertClose(reduced.Sections[0].StartMeasuredDepth, survey.MeasuredDepths[0],
                            $"{what}: the first section does not start at the first station", failures);
                        AssertClose(reduced.Sections[reduced.SectionCount - 1].EndMeasuredDepth, totalDepth,
                            $"{what}: the last section does not end at the last station", failures);
                        for (int q = 1; q < reduced.SectionCount; q++)
                        {
                            AssertClose(reduced.Sections[q].StartMeasuredDepth,
                                reduced.Sections[q - 1].EndMeasuredDepth,
                                $"{what}: section {q} does not start where section {q - 1} ends", failures);
                        }
                    }

                    // Every section has to hold the angular tolerance it was cut at. This is what the
                    // segmentation promises and it is the one thing a caller can rely on absolutely; the
                    // distance which comes out of it is a consequence rather than a promise.
                    ReducedOrderTrajectory.Segment(survey.MeasuredDepths, survey.Tangents,
                        survey.Inclinations, survey.Azimuths, tolerance,
                        ReducedOrderTrajectory.DefaultMinimumSineInclination, false,
                        out int[] breaks, out ReducedOrderCurveType[] types);
                    for (int q = 0; q < types.Length; q++)
                    {
                        double residual = ReducedOrderTrajectory.FitSection(types[q],
                            survey.MeasuredDepths, survey.Tangents, survey.Inclinations, survey.Azimuths,
                            breaks[q], breaks[q + 1], out _, out _);
                        // A section spanning only two stations is the guaranteed fallback and is exact,
                        // so it cannot be over tolerance either.
                        if (residual > tolerance + 1.0e-12)
                        {
                            failures.Add($"{what}: section {q} of {types[q]} over stations "
                                + $"[{breaks[q]},{breaks[q + 1]}] leaves {residual:E3} rad, over the "
                                + $"{tolerance:E3} rad asked for");
                        }
                    }

                    // No azimuth carrying curve may span a station too close to the vertical for the
                    // azimuth to mean anything. Specification section 3.5.
                    for (int q = 0; q < types.Length; q++)
                    {
                        if (types[q] != ReducedOrderCurveType.ConstantCurvatureAndToolface
                            && types[q] != ReducedOrderCurveType.ConstantBuildAndTurn)
                        {
                            continue;
                        }
                        for (int n = breaks[q]; n <= breaks[q + 1]; n++)
                        {
                            if (System.Math.Sin(survey.Inclinations[n])
                                < ReducedOrderTrajectory.DefaultMinimumSineInclination)
                            {
                                failures.Add($"{what}: section {q} is {types[q]} but station {n} is "
                                    + $"{survey.Inclinations[n] * 180.0 / System.Math.PI:F2} deg from vertical");
                                break;
                            }
                        }
                    }

                    // The refinement may not return something worse than the chained initialisation.
                    ReducedOrderTrajectory chained = ReducedOrderTrajectory.Reduce(
                        survey.MeasuredDepths, survey.Inclinations, survey.Azimuths, tolerance,
                        new ReductionOptions() { Refine = false });
                    if (reduced.MaxDeviation > chained.MaxDeviation + 1.0e-9)
                    {
                        failures.Add($"{what}: refined to {reduced.MaxDeviation:F4} m, worse than the "
                            + $"{chained.MaxDeviation:F4} m it started from");
                    }

                    // The deviation reported has to be the one a caller measures through the public
                    // position query.
                    double worst = 0.0;
                    for (int i = 0; i < stationCount; i++)
                    {
                        Point3D? position = reduced.PositionAt(survey.MeasuredDepths[i]);
                        if (position == null)
                        {
                            failures.Add($"{what}: no position at station {i}");
                            break;
                        }
                        double north = (position.X ?? 0.0) - survey.Positions[3 * i];
                        double east = (position.Y ?? 0.0) - survey.Positions[3 * i + 1];
                        double vertical = (position.Z ?? 0.0) - survey.Positions[3 * i + 2];
                        worst = System.Math.Max(worst,
                            System.Math.Sqrt(north * north + east * east + vertical * vertical));
                    }
                    if (System.Math.Abs(worst - reduced.MaxDeviation) > 1.0e-9)
                    {
                        failures.Add($"{what}: reports {reduced.MaxDeviation:F6} m but a caller measures "
                            + $"{worst:F6} m");
                    }

                    rows.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0,-14}{1,7}{2,9:F0}{3,6:F1}{4,8}{5,9:F3}{6,9:F3}{7,8:F2}{8,8:F2}",
                        survey.Name, stationCount, totalDepth, degrees, reduced.SectionCount,
                        reduced.MaxDeviation, reduced.RmsDeviation,
                        reduced.MaxInclinationDeviation * 180.0 / System.Math.PI,
                        reduced.MaxAzimuthDeviationInHole * 180.0 / System.Math.PI));
                }
            }

            TestContext.Out.WriteLine(string.Join(System.Environment.NewLine, rows));
            TestContext.Out.WriteLine("dIncl and dAzi are the largest inclination and in hole azimuth "
                + "deviations, in degrees.");
            Assert.That(failures, Is.Empty, string.Join(System.Environment.NewLine, failures));
        }

        /// <summary>
        /// A coarser tolerance must not ask for more sections than a finer one. This is not something the
        /// method proves — the feasibility test it applies is not monotone, as
        /// <see cref="ReducedOrderTrajectoryVectorTests.Test67RefittedFeasibilityIsNotMonotone"/> shows —
        /// but it should hold on real wells, and a well where it failed would be worth looking at.
        /// </summary>
        [Test]
        public void Test83CoarserTolerancesNeedNoMoreSections()
        {
            List<UllriggSurvey> surveys = LoadSurveys();
            List<string> failures = new();

            foreach (UllriggSurvey survey in surveys)
            {
                int previous = int.MaxValue;
                foreach (double degrees in TolerancesInDegrees)
                {
                    ReducedOrderTrajectory.Segment(survey.MeasuredDepths, survey.Tangents,
                        survey.Inclinations, survey.Azimuths, degrees * System.Math.PI / 180.0,
                        ReducedOrderTrajectory.DefaultMinimumSineInclination, false,
                        out _, out ReducedOrderCurveType[] types);
                    if (types.Length > previous)
                    {
                        failures.Add($"{survey.Name}: {degrees} deg needs {types.Length} sections, more "
                            + $"than the {previous} a finer tolerance needed");
                    }
                    previous = types.Length;
                }
            }
            Assert.That(failures, Is.Empty, string.Join(System.Environment.NewLine, failures));
        }

        /// <summary>
        /// The same survey gives the same reduction every time, including through the random restarts of
        /// the refinement.
        /// </summary>
        [Test]
        public void Test84UllriggReductionsAreReproducible()
        {
            List<UllriggSurvey> surveys = LoadSurveys();
            double tolerance = 1.0 * System.Math.PI / 180.0;

            foreach (UllriggSurvey survey in surveys)
            {
                ReducedOrderTrajectory first = ReducedOrderTrajectory.Reduce(
                    survey.MeasuredDepths, survey.Inclinations, survey.Azimuths, tolerance);
                ReducedOrderTrajectory again = ReducedOrderTrajectory.Reduce(
                    survey.MeasuredDepths, survey.Inclinations, survey.Azimuths, tolerance);

                Assert.That(again.SectionCount, Is.EqualTo(first.SectionCount), survey.Name);
                Assert.That(again.MaxDeviation, Is.EqualTo(first.MaxDeviation), survey.Name);
                for (int q = 0; q < first.SectionCount; q++)
                {
                    Assert.That(again.Sections[q].FirstParameter,
                        Is.EqualTo(first.Sections[q].FirstParameter), $"{survey.Name} section {q}");
                    Assert.That(again.Sections[q].SecondParameter,
                        Is.EqualTo(first.Sections[q].SecondParameter), $"{survey.Name} section {q}");
                }
            }
        }

        /// <summary>
        /// Reduction to a stated distance over the Ullrigg wells, which is how a caller would normally
        /// ask. Specification section 10.
        /// </summary>
        [Test]
        public void Test85UllriggSurveysReduceToAStatedDistance()
        {
            List<UllriggSurvey> surveys = LoadSurveys();
            List<string> rows = new();
            List<string> failures = new();

            foreach (UllriggSurvey survey in surveys)
            {
                foreach (double target in new[] { 1.0, 0.25 })
                {
                    ReducedOrderTrajectory reduced = ReducedOrderTrajectory.ReduceToDeviation(
                        survey.MeasuredDepths, survey.Inclinations, survey.Azimuths, target);
                    if (!reduced.IsValid)
                    {
                        failures.Add($"{survey.Name} within {target} m: {reduced.FailureDescription}");
                        continue;
                    }
                    rows.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0,-14} within {1,5:F2} m: {2,3} sections at {3,4:F2} deg, achieved {4,7:F4} m{5}",
                        survey.Name, target, reduced.SectionCount,
                        reduced.AngularTolerance * 180.0 / System.Math.PI, reduced.MaxDeviation,
                        reduced.MaxDeviation <= target ? string.Empty : "   (not met by any rung)"));
                }
            }
            TestContext.Out.WriteLine(string.Join(System.Environment.NewLine, rows));
            Assert.That(failures, Is.Empty, string.Join(System.Environment.NewLine, failures));
        }

        private static void AssertClose(double actual, double expected, string what, List<string> failures)
        {
            if (System.Math.Abs(actual - expected) > 1.0e-9)
            {
                failures.Add($"{what}: expected {expected:R}, got {actual:R}");
            }
        }
    }
}
