using OSDC.DotnetLibraries.General.Common;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Section;

/// <summary>
/// Solves two commanded sections when both section lengths and only the final vertical depth and
/// attitude are imposed. Circular-arc and constant-curvature/toolface pairs share one curvature.
/// Build-and-turn pairs have equal spatial curvature at their junction.
/// </summary>
public static class DoubleSectionByDrilledLengthSolver
{
    private const int MaximumIterations = 60;
    private const double ResidualTolerance = 1.0e-9;

    public static bool TryCalculateCircularArcs(
        TrajectoryPoint3D start,
        double upstreamLength,
        double downstreamLength,
        double targetVerticalDepth,
        double targetInclination,
        double targetAzimuth,
        out DoubleArcs result)
    {
        result = new DoubleArcs();
        if (!Valid(start, upstreamLength, downstreamLength, targetVerticalDepth, targetInclination, targetAzimuth))
            return false;

        double total = upstreamLength + downstreamLength;
        double dogleg = AttitudeMiss(start.Inclination!.Value, start.Azimuth!.Value,
            targetInclination, targetAzimuth);
        double[] curvatureSeeds =
        [
            System.Math.Max(0.01, 0.5 * dogleg),
            System.Math.Max(0.03, dogleg),
            System.Math.Max(0.08, 2.0 * dogleg),
            0.25, 0.6, 1.2, 2.4
        ];

        double[]? best = null;
        double bestScore = double.PositiveInfinity;
        foreach (double curvatureSeed in curvatureSeeds.Distinct())
        {
            for (int first = 0; first < 8; first++)
            {
                for (int second = 0; second < 8; second++)
                {
                    double[] guess =
                    [
                        curvatureSeed,
                        -System.Math.PI + first * System.Math.PI / 4.0,
                        -System.Math.PI + second * System.Math.PI / 4.0
                    ];
                    if (!TrySolve(guess, values => CircularResidual(start, upstreamLength, downstreamLength,
                            targetVerticalDepth, targetInclination, targetAzimuth, values), out double[] solved) ||
                        solved[0] <= 0.0)
                        continue;
                    if (solved[0] < bestScore)
                    {
                        bestScore = solved[0];
                        best = solved;
                    }
                }
            }
        }
        if (best == null || !ForwardCircular(start, upstreamLength, downstreamLength,
                best[0] / total, best[1], best[2], out CircularArcSection? upstream,
                out CircularArcSection? downstream))
            return false;

        result.Start.Set(start);
        result.Intermediate.Set(upstream!.End);
        result.End.Set(downstream!.End);
        result.DoubleArcCurve.Curvature = best[0] / total;
        result.DoubleArcCurve.UpstreamReferenceToolface = best[1];
        result.DoubleArcCurve.DownstreamReferenceToolface = best[2];
        result.DoubleArcCurve.Length = total;
        return Verify(result.End, targetVerticalDepth, targetInclination, targetAzimuth, total);
    }

    public static bool TryCalculateConstantCurvatureAndToolfaceArcs(
        TrajectoryPoint3D start,
        double upstreamLength,
        double downstreamLength,
        double targetVerticalDepth,
        double targetInclination,
        double targetAzimuth,
        out DoubleConstantCurvatureAndToolfaceArcs result)
    {
        result = new DoubleConstantCurvatureAndToolfaceArcs();
        if (!Valid(start, upstreamLength, downstreamLength, targetVerticalDepth, targetInclination, targetAzimuth))
            return false;

        double total = upstreamLength + downstreamLength;
        double dogleg = AttitudeMiss(start.Inclination!.Value, start.Azimuth!.Value,
            targetInclination, targetAzimuth);
        double[] curvatureSeeds =
        [
            System.Math.Max(0.01, 0.5 * dogleg),
            System.Math.Max(0.03, dogleg),
            System.Math.Max(0.08, 2.0 * dogleg),
            0.25, 0.6, 1.2, 2.4
        ];

        double[]? best = null;
        double bestScore = double.PositiveInfinity;
        foreach (double curvatureSeed in curvatureSeeds.Distinct())
        {
            for (int first = 0; first < 8; first++)
            {
                for (int second = 0; second < 8; second++)
                {
                    double[] guess =
                    [
                        curvatureSeed,
                        -System.Math.PI + first * System.Math.PI / 4.0,
                        -System.Math.PI + second * System.Math.PI / 4.0
                    ];
                    if (!TrySolve(guess, values => ConstantToolfaceResidual(start, upstreamLength, downstreamLength,
                            targetVerticalDepth, targetInclination, targetAzimuth, values), out double[] solved) ||
                        solved[0] <= 0.0)
                        continue;
                    if (solved[0] < bestScore)
                    {
                        bestScore = solved[0];
                        best = solved;
                    }
                }
            }
        }
        if (best == null || !ForwardConstantToolface(start, upstreamLength, downstreamLength,
                best[0] / total, best[1], best[2], out ConstantCurvatureAndToolfaceArcSection? upstream,
                out ConstantCurvatureAndToolfaceArcSection? downstream))
            return false;

        result.Start.Set(start);
        result.Intermediate.Set(upstream!.End);
        result.End.Set(downstream!.End);
        result.DoubleCTCCurve.Curvature = best[0] / total;
        result.DoubleCTCCurve.UpstreamToolface = best[1];
        result.DoubleCTCCurve.DownstreamToolface = best[2];
        result.DoubleCTCCurve.UpstreamLength = upstreamLength;
        result.DoubleCTCCurve.DownstreamLength = downstreamLength;
        result.DoubleCTCCurve.Length = total;
        return Verify(result.End, targetVerticalDepth, targetInclination, targetAzimuth, total);
    }

    public static bool TryCalculateBuildAndTurnArcs(
        TrajectoryPoint3D start,
        double upstreamLength,
        double downstreamLength,
        double targetVerticalDepth,
        double targetInclination,
        double targetAzimuth,
        int azimuthBranch,
        out DoubleBuildAndTurnArcs result)
    {
        result = new DoubleBuildAndTurnArcs();
        if (!Valid(start, upstreamLength, downstreamLength, targetVerticalDepth, targetInclination, targetAzimuth))
            return false;

        double total = upstreamLength + downstreamLength;
        double inclinationChange = targetInclination - start.Inclination!.Value;
        double azimuthChange = WrapToPi(targetAzimuth - start.Azimuth!.Value) + 2.0 * System.Math.PI * azimuthBranch;
        double baseBuild = inclinationChange / total;
        double baseTurn = azimuthChange / total;
        double[] offsets = [-2.0, -1.0, -0.4, 0.0, 0.4, 1.0, 2.0];
        double[]? best = null;
        double bestScore = double.PositiveInfinity;
        foreach (double buildOffset in offsets)
        {
            foreach (double turnOffset in offsets)
            {
                double[] guess = [baseBuild * total + buildOffset, baseTurn * total + turnOffset];
                if (!TrySolve(guess, values => BuildTurnResidual(start, upstreamLength, downstreamLength,
                        targetVerticalDepth, targetInclination, targetAzimuth, azimuthBranch, values),
                        out double[] solved))
                    continue;

                Rates(solved, total, upstreamLength, downstreamLength, inclinationChange, azimuthChange,
                    out double buildFirst, out double turnFirst, out double buildSecond, out double turnSecond);
                double junctionInclination = start.Inclination.Value + buildFirst * upstreamLength;
                double score = System.Math.Max(
                    PeakCurvature(buildFirst, turnFirst, start.Inclination.Value, junctionInclination),
                    PeakCurvature(buildSecond, turnSecond, junctionInclination, targetInclination));
                if (score < bestScore)
                {
                    bestScore = score;
                    best = solved;
                }
            }
        }
        if (best == null)
            return false;

        Rates(best, total, upstreamLength, downstreamLength, inclinationChange, azimuthChange,
            out double b1, out double t1, out double b2, out double t2);
        if (!ForwardBuildTurn(start, upstreamLength, downstreamLength, b1, t1, b2, t2,
                out BuildAndTurnArcSection? upstream, out BuildAndTurnArcSection? downstream))
            return false;

        double junction = upstream!.End.Inclination!.Value;
        result.Start.Set(start);
        result.Intermediate.Set(upstream.End);
        result.End.Set(downstream!.End);
        result.DoubleBuildAndTurnCurve.UpstreamBUR = b1;
        result.DoubleBuildAndTurnCurve.UpstreamTR = t1;
        result.DoubleBuildAndTurnCurve.UpstreamLength = upstreamLength;
        result.DoubleBuildAndTurnCurve.DownstreamBUR = b2;
        result.DoubleBuildAndTurnCurve.DownstreamTR = t2;
        result.DoubleBuildAndTurnCurve.DownstreamLength = downstreamLength;
        result.DoubleBuildAndTurnCurve.CurvatureRatio = 1.0;
        result.DoubleBuildAndTurnCurve.JunctionCurvature = CurvatureAt(b1, t1, junction);
        result.DoubleBuildAndTurnCurve.Length = total;
        return Verify(result.End, targetVerticalDepth, targetInclination, targetAzimuth, total);
    }

    private static double[]? CircularResidual(TrajectoryPoint3D start, double firstLength, double secondLength,
        double targetDepth, double targetInclination, double targetAzimuth, double[] values)
    {
        double total = firstLength + secondLength;
        if (values[0] <= 0.0 || !ForwardCircular(start, firstLength, secondLength, values[0] / total,
                values[1], values[2], out _, out CircularArcSection? second))
            return null;
        return EndpointResidual(second!.End, targetDepth, targetInclination, targetAzimuth, total);
    }

    private static double[]? ConstantToolfaceResidual(TrajectoryPoint3D start, double firstLength, double secondLength,
        double targetDepth, double targetInclination, double targetAzimuth, double[] values)
    {
        double total = firstLength + secondLength;
        if (values[0] <= 0.0 || !ForwardConstantToolface(start, firstLength, secondLength, values[0] / total,
                values[1], values[2], out _, out ConstantCurvatureAndToolfaceArcSection? second))
            return null;
        return EndpointResidual(second!.End, targetDepth, targetInclination, targetAzimuth, total);
    }

    private static double[]? BuildTurnResidual(TrajectoryPoint3D start, double firstLength, double secondLength,
        double targetDepth, double targetInclination, double targetAzimuth, int branch, double[] values)
    {
        double total = firstLength + secondLength;
        double inclinationChange = targetInclination - start.Inclination!.Value;
        double azimuthChange = WrapToPi(targetAzimuth - start.Azimuth!.Value) + 2.0 * System.Math.PI * branch;
        Rates(values, total, firstLength, secondLength, inclinationChange, azimuthChange,
            out double b1, out double t1, out double b2, out double t2);
        if (!ForwardBuildTurn(start, firstLength, secondLength, b1, t1, b2, t2,
                out BuildAndTurnArcSection? first, out BuildAndTurnArcSection? second))
            return null;
        double junction = first!.End.Inclination!.Value;
        return
        [
            (second!.End.Z!.Value - targetDepth) / total,
            (CurvatureAt(b1, t1, junction) - CurvatureAt(b2, t2, junction)) * total
        ];
    }

    private static void Rates(double[] values, double total, double firstLength, double secondLength,
        double inclinationChange, double azimuthChange,
        out double b1, out double t1, out double b2, out double t2)
    {
        b1 = values[0] / total;
        t1 = values[1] / total;
        b2 = (inclinationChange - b1 * firstLength) / secondLength;
        t2 = (azimuthChange - t1 * firstLength) / secondLength;
    }

    private static bool ForwardCircular(TrajectoryPoint3D start, double firstLength, double secondLength,
        double curvature, double firstToolface, double secondToolface,
        out CircularArcSection? first, out CircularArcSection? second)
    {
        first = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
        first.Start.Set(start);
        first.Circle.Length = firstLength;
        first.Circle.Curvature = curvature;
        first.Circle.ReferenceToolface = firstToolface;
        if (!first.CalculateLDT()) { second = null; return false; }
        second = new CircularArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
        second.Start.Set(first.End);
        second.Circle.Length = secondLength;
        second.Circle.Curvature = curvature;
        second.Circle.ReferenceToolface = secondToolface;
        return second.CalculateLDT();
    }

    private static bool ForwardConstantToolface(TrajectoryPoint3D start, double firstLength, double secondLength,
        double curvature, double firstToolface, double secondToolface,
        out ConstantCurvatureAndToolfaceArcSection? first,
        out ConstantCurvatureAndToolfaceArcSection? second)
    {
        first = new ConstantCurvatureAndToolfaceArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
        first.Start.Set(start);
        first.CTCCurve.Length = firstLength;
        first.CTCCurve.Curvature = curvature;
        first.CTCCurve.Toolface = firstToolface;
        if (!first.CalculateLDT()) { second = null; return false; }
        second = new ConstantCurvatureAndToolfaceArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
        second.Start.Set(first.End);
        second.CTCCurve.Length = secondLength;
        second.CTCCurve.Curvature = curvature;
        second.CTCCurve.Toolface = secondToolface;
        return second.CalculateLDT();
    }

    private static bool ForwardBuildTurn(TrajectoryPoint3D start, double firstLength, double secondLength,
        double b1, double t1, double b2, double t2,
        out BuildAndTurnArcSection? first, out BuildAndTurnArcSection? second)
    {
        first = new BuildAndTurnArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
        first.Start.Set(start);
        first.BuildAndTurn.Length = firstLength;
        first.BuildAndTurn.BUR = b1;
        first.BuildAndTurn.TR = t1;
        if (!first.CalculateLBT()) { second = null; return false; }
        second = new BuildAndTurnArcSection(new TrajectoryPoint3D(), new TrajectoryPoint3D());
        second.Start.Set(first.End);
        second.BuildAndTurn.Length = secondLength;
        second.BuildAndTurn.BUR = b2;
        second.BuildAndTurn.TR = t2;
        return second.CalculateLBT();
    }

    private static double[] EndpointResidual(TrajectoryPoint3D end, double targetDepth,
        double targetInclination, double targetAzimuth, double lengthScale)
    {
        Tangent(end.Inclination!.Value, end.Azimuth!.Value, out double x, out double y, out double z);
        Tangent(targetInclination, targetAzimuth, out double tx, out double ty, out double tz);
        Orthogonal(tx, ty, tz, out double e1x, out double e1y, out double e1z,
            out double e2x, out double e2y, out double e2z);
        return
        [
            (end.Z!.Value - targetDepth) / lengthScale,
            x * e1x + y * e1y + z * e1z,
            x * e2x + y * e2y + z * e2z
        ];
    }

    private static bool TrySolve(double[] initial, Func<double[], double[]?> residual, out double[] solution)
    {
        solution = (double[])initial.Clone();
        double[]? current = residual(solution);
        if (current == null) return false;
        double norm = NormSquared(current);
        for (int iteration = 0; iteration < MaximumIterations; iteration++)
        {
            if (norm <= ResidualTolerance * ResidualTolerance) return true;
            int count = solution.Length;
            double[,] jacobian = new double[count, count];
            for (int column = 0; column < count; column++)
            {
                double step = 1.0e-5 * (1.0 + System.Math.Abs(solution[column]));
                double[] plus = (double[])solution.Clone();
                double[] minus = (double[])solution.Clone();
                plus[column] += step;
                minus[column] -= step;
                double[]? high = residual(plus);
                double[]? low = residual(minus);
                if (high == null || low == null) return false;
                for (int row = 0; row < count; row++)
                    jacobian[row, column] = (high[row] - low[row]) / (2.0 * step);
            }
            if (!SolveLinear(jacobian, current.Select(value => -value).ToArray(), out double[] delta))
                return false;
            bool improved = false;
            double damping = 1.0;
            for (int cut = 0; cut < 14; cut++)
            {
                double[] trial = solution.Zip(delta, (value, change) => value + damping * change).ToArray();
                double[]? trialResidual = residual(trial);
                if (trialResidual != null && NormSquared(trialResidual) < norm)
                {
                    solution = trial;
                    current = trialResidual;
                    norm = NormSquared(current);
                    improved = true;
                    break;
                }
                damping *= 0.5;
            }
            if (!improved) return false;
        }
        return norm <= ResidualTolerance * ResidualTolerance;
    }

    private static bool SolveLinear(double[,] matrix, double[] rhs, out double[] solution)
    {
        int count = rhs.Length;
        double[,] augmented = new double[count, count + 1];
        for (int row = 0; row < count; row++)
        {
            for (int column = 0; column < count; column++) augmented[row, column] = matrix[row, column];
            augmented[row, count] = rhs[row];
        }
        for (int pivot = 0; pivot < count; pivot++)
        {
            int best = pivot;
            for (int row = pivot + 1; row < count; row++)
                if (System.Math.Abs(augmented[row, pivot]) > System.Math.Abs(augmented[best, pivot])) best = row;
            if (System.Math.Abs(augmented[best, pivot]) < 1.0e-12) { solution = []; return false; }
            if (best != pivot)
                for (int column = pivot; column <= count; column++)
                    (augmented[pivot, column], augmented[best, column]) =
                        (augmented[best, column], augmented[pivot, column]);
            double divisor = augmented[pivot, pivot];
            for (int column = pivot; column <= count; column++) augmented[pivot, column] /= divisor;
            for (int row = 0; row < count; row++)
            {
                if (row == pivot) continue;
                double factor = augmented[row, pivot];
                for (int column = pivot; column <= count; column++)
                    augmented[row, column] -= factor * augmented[pivot, column];
            }
        }
        solution = Enumerable.Range(0, count).Select(row => augmented[row, count]).ToArray();
        return solution.All(Numeric.IsDefined);
    }

    private static bool Valid(TrajectoryPoint3D start, double firstLength, double secondLength,
        double targetDepth, double targetInclination, double targetAzimuth) =>
        start != null && Numeric.IsDefined(start.Abscissa) && Numeric.IsDefined(start.X) &&
        Numeric.IsDefined(start.Y) && Numeric.IsDefined(start.Z) && Numeric.IsDefined(start.Inclination) &&
        Numeric.IsDefined(start.Azimuth) && Numeric.GT(firstLength, 0.0) && Numeric.GT(secondLength, 0.0) &&
        Numeric.IsDefined(targetDepth) && Numeric.IsDefined(targetInclination) &&
        targetInclination >= 0.0 && targetInclination <= Numeric.PI && Numeric.IsDefined(targetAzimuth);

    private static bool Verify(TrajectoryPoint3D end, double targetDepth, double targetInclination,
        double targetAzimuth, double lengthScale) =>
        System.Math.Abs(end.Z!.Value - targetDepth) <= 1.0e-6 * System.Math.Max(1.0, lengthScale) &&
        AttitudeMiss(end.Inclination!.Value, end.Azimuth!.Value, targetInclination, targetAzimuth) <= 1.0e-7;

    private static double CurvatureAt(double build, double turn, double inclination) =>
        System.Math.Sqrt(build * build + turn * turn * System.Math.Sin(inclination) * System.Math.Sin(inclination));

    private static double PeakCurvature(double build, double turn, double from, double to)
    {
        double sine = System.Math.Max(System.Math.Abs(System.Math.Sin(from)), System.Math.Abs(System.Math.Sin(to)));
        if ((from <= Numeric.PI / 2.0 && to >= Numeric.PI / 2.0) ||
            (to <= Numeric.PI / 2.0 && from >= Numeric.PI / 2.0)) sine = 1.0;
        return System.Math.Sqrt(build * build + turn * turn * sine * sine);
    }

    private static double NormSquared(double[] values) => values.Sum(value => value * value);

    private static double WrapToPi(double value)
    {
        while (value > Numeric.PI) value -= 2.0 * Numeric.PI;
        while (value <= -Numeric.PI) value += 2.0 * Numeric.PI;
        return value;
    }

    private static double AttitudeMiss(double inclination, double azimuth,
        double targetInclination, double targetAzimuth)
    {
        Tangent(inclination, azimuth, out double x, out double y, out double z);
        Tangent(targetInclination, targetAzimuth, out double tx, out double ty, out double tz);
        double cx = y * tz - z * ty;
        double cy = z * tx - x * tz;
        double cz = x * ty - y * tx;
        return System.Math.Atan2(System.Math.Sqrt(cx * cx + cy * cy + cz * cz), x * tx + y * ty + z * tz);
    }

    private static void Tangent(double inclination, double azimuth, out double x, out double y, out double z)
    {
        double sine = System.Math.Sin(inclination);
        x = sine * System.Math.Cos(azimuth);
        y = sine * System.Math.Sin(azimuth);
        z = System.Math.Cos(inclination);
    }

    private static void Orthogonal(double x, double y, double z,
        out double e1x, out double e1y, out double e1z,
        out double e2x, out double e2y, out double e2z)
    {
        double ax = System.Math.Abs(x) <= System.Math.Abs(y) && System.Math.Abs(x) <= System.Math.Abs(z) ? 1.0 : 0.0;
        double ay = ax == 0.0 && System.Math.Abs(y) <= System.Math.Abs(z) ? 1.0 : 0.0;
        double az = ax == 0.0 && ay == 0.0 ? 1.0 : 0.0;
        e1x = y * az - z * ay;
        e1y = z * ax - x * az;
        e1z = x * ay - y * ax;
        double length = System.Math.Sqrt(e1x * e1x + e1y * e1y + e1z * e1z);
        e1x /= length; e1y /= length; e1z /= length;
        e2x = y * e1z - z * e1y;
        e2y = z * e1x - x * e1z;
        e2z = x * e1y - y * e1x;
    }
}
