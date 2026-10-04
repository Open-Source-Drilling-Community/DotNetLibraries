using NUnit.Framework;
using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.General.Math.UnitTest;

public sealed class BuildAndTurnTargetSolutionTests
{
    private static TrajectoryPoint3D Start() => new()
    {
        X = 6534963.594500519,
        Y = 328719.4050929201,
        Z = 974.7544888700581,
        Abscissa = 1026.78,
        Inclination = 0.46355365128420706,
        Azimuth = 2.8525680288655395
    };

    [Test]
    public void Cartesian_target_returns_distinct_roots_ordered_by_length()
    {
        IReadOnlyList<BuildAndTurnTargetSolution> solutions = Start().SolveBTTargetSolutions(
            6534960.156788087, 328704.32592859754, 1703.6123210251205);

        Assert.That(solutions.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(solutions.Select(solution => solution.Length), Is.Ordered.Ascending);
        Assert.That(solutions.Select(solution => (solution.SweptInclination, solution.SweptAzimuth, solution.Length)).Distinct().Count(),
            Is.EqualTo(solutions.Count));
        Assert.That(solutions.Any(solution =>
            System.Math.Abs(solution.PeakCurvature - 0.007232299613759401) <= 1e-12), Is.True);
        Assert.That(solutions.Any(solution => solution.PeakCurvature < 0.004654211338651545), Is.True);
    }

    [Test]
    public void Curvature_constrained_completion_uses_shortest_compliant_root()
    {
        TrajectoryPoint3D start = Start();
        TrajectoryPoint3D target = new()
        {
            X = 6534960.156788087,
            Y = 328704.32592859754,
            Z = 1703.6123210251205
        };

        Assert.That(start.CompleteBTXYZ(target, 0.004654211338651545), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(target.Abscissa - start.Abscissa, Is.EqualTo(845.293).Within(0.01));
            Assert.That(target.Curvature, Is.LessThanOrEqualTo(0.004654211338651545 + 1e-12));
            Assert.That(target.X, Is.EqualTo(6534960.156788087).Within(1e-5));
            Assert.That(target.Y, Is.EqualTo(328704.32592859754).Within(1e-5));
            Assert.That(target.Z, Is.EqualTo(1703.6123210251205).Within(1e-5));
        });
    }

    [Test]
    public void Tolerance_aware_completion_matches_the_shortest_compliant_enumerated_root()
    {
        const double maximumCurvature = 0.004654211338651545;
        const double positionTolerance = 0.25;
        TrajectoryPoint3D start = Start();
        IReadOnlyList<BuildAndTurnTargetSolution> solutions = start.SolveBTTargetSolutions(
            6534960.156788087, 328704.32592859754, 1703.6123210251205, positionTolerance);
        BuildAndTurnTargetSolution expected = solutions.First(solution =>
            solution.PeakCurvature <= maximumCurvature + 1e-12);
        TrajectoryPoint3D target = new()
        {
            X = 6534960.156788087,
            Y = 328704.32592859754,
            Z = 1703.6123210251205
        };

        BuildAndTurnTargetCompletionStatus status = start.CompleteBTXYZ(
            target, maximumCurvature, positionTolerance);

        Assert.Multiple(() =>
        {
            Assert.That(status, Is.EqualTo(BuildAndTurnTargetCompletionStatus.Completed));
            Assert.That(target.Abscissa - start.Abscissa, Is.EqualTo(expected.Length).Within(1e-8));
            Assert.That(target.Curvature, Is.LessThanOrEqualTo(maximumCurvature + 1e-12));
            Assert.That(target.X, Is.EqualTo(6534960.156788087).Within(positionTolerance));
            Assert.That(target.Y, Is.EqualTo(328704.32592859754).Within(positionTolerance));
            Assert.That(target.Z, Is.EqualTo(1703.6123210251205).Within(positionTolerance));
        });
    }

    [Test]
    public void Curvature_rejection_retains_a_geometric_root_for_classification()
    {
        const double positionTolerance = 0.25;
        TrajectoryPoint3D start = Start();
        TrajectoryPoint3D target = new()
        {
            X = 6534960.156788087,
            Y = 328704.32592859754,
            Z = 1703.6123210251205
        };

        BuildAndTurnTargetCompletionStatus status = start.CompleteBTXYZ(
            target, 1e-8, positionTolerance);

        Assert.Multiple(() =>
        {
            Assert.That(status, Is.EqualTo(BuildAndTurnTargetCompletionStatus.ExceedsMaximumCurvature));
            Assert.That(target.Abscissa, Is.GreaterThan(start.Abscissa));
            Assert.That(target.Curvature, Is.GreaterThan(1e-8));
            Assert.That(target.X, Is.EqualTo(6534960.156788087).Within(positionTolerance));
            Assert.That(target.Y, Is.EqualTo(328704.32592859754).Within(positionTolerance));
            Assert.That(target.Z, Is.EqualTo(1703.6123210251205).Within(positionTolerance));
        });
    }
}
