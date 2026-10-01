using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Section.UnitTest;

public class DoubleSectionByDrilledLengthSolverTests
{
    [Test]
    public void CircularArcsRecoverDepthAndAttitudeWithFixedLengths()
    {
        TrajectoryPoint3D start = Start();
        (CircularArcSection first, CircularArcSection second) = CircularReference(start, 80.0, 55.0, 0.006, 0.4, -0.7);

        Assert.That(DoubleSectionByDrilledLengthSolver.TryCalculateCircularArcs(start, 80.0, 55.0,
            second.End.Z!.Value, second.End.Inclination!.Value, second.End.Azimuth!.Value, out DoubleArcs solved), Is.True);
        AssertEndpoint(solved.End, second.End);
        Assert.That(solved.Intermediate.Abscissa, Is.EqualTo(80.0).Within(1e-7));
        Assert.That(solved.DoubleArcCurve.Curvature, Is.EqualTo(0.006).Within(1e-5));
    }

    [Test]
    public void ConstantToolfaceArcsRecoverDepthAndAttitudeWithFixedLengths()
    {
        TrajectoryPoint3D start = Start();
        (ConstantCurvatureAndToolfaceArcSection first, ConstantCurvatureAndToolfaceArcSection second) =
            ConstantToolfaceReference(start, 65.0, 75.0, 0.005, 0.8, -0.35);

        Assert.That(DoubleSectionByDrilledLengthSolver.TryCalculateConstantCurvatureAndToolfaceArcs(start, 65.0, 75.0,
            second.End.Z!.Value, second.End.Inclination!.Value, second.End.Azimuth!.Value,
            out DoubleConstantCurvatureAndToolfaceArcs solved), Is.True);
        AssertEndpoint(solved.End, second.End);
        Assert.That(solved.Intermediate.Abscissa, Is.EqualTo(65.0).Within(1e-7));
        Assert.That(solved.DoubleCTCCurve.Curvature, Is.EqualTo(0.005).Within(1e-5));
    }

    [Test]
    public void BuildAndTurnArcsReachTargetAndMatchJunctionCurvature()
    {
        TrajectoryPoint3D start = Start();
        double firstLength = 70.0;
        double secondLength = 60.0;
        double b1 = 0.003;
        double t1 = 0.002;
        BuildAndTurnArcSection first = BuildTurn(start, firstLength, b1, t1);
        double junction = first.End.Inclination!.Value;
        double b2 = -0.0015;
        double firstCurvature = Curvature(b1, t1, junction);
        double t2 = System.Math.Sqrt(System.Math.Max(0.0, firstCurvature * firstCurvature - b2 * b2)) /
                    System.Math.Sin(junction);
        BuildAndTurnArcSection second = BuildTurn(first.End, secondLength, b2, t2);

        Assert.That(DoubleSectionByDrilledLengthSolver.TryCalculateBuildAndTurnArcs(start, firstLength, secondLength,
            second.End.Z!.Value, second.End.Inclination!.Value, second.End.Azimuth!.Value, 0,
            out DoubleBuildAndTurnArcs solved), Is.True);
        AssertEndpoint(solved.End, second.End);
        double gotJunction = solved.Intermediate.Inclination!.Value;
        double upstream = Curvature(solved.DoubleBuildAndTurnCurve.UpstreamBUR!.Value,
            solved.DoubleBuildAndTurnCurve.UpstreamTR!.Value, gotJunction);
        double downstream = Curvature(solved.DoubleBuildAndTurnCurve.DownstreamBUR!.Value,
            solved.DoubleBuildAndTurnCurve.DownstreamTR!.Value, gotJunction);
        Assert.That(upstream, Is.EqualTo(downstream).Within(1e-8));
    }

    private static TrajectoryPoint3D Start() => new()
    {
        Abscissa = 0.0,
        X = 100.0,
        Y = -25.0,
        Z = 1400.0,
        Inclination = 1.1,
        Azimuth = 0.7
    };

    private static (CircularArcSection, CircularArcSection) CircularReference(TrajectoryPoint3D start,
        double firstLength, double secondLength, double curvature, double firstToolface, double secondToolface)
    {
        CircularArcSection first = new(start, new TrajectoryPoint3D());
        first.Circle.Length = firstLength; first.Circle.Curvature = curvature; first.Circle.ReferenceToolface = firstToolface;
        Assert.That(first.CalculateLDT(), Is.True);
        CircularArcSection second = new(first.End, new TrajectoryPoint3D());
        second.Circle.Length = secondLength; second.Circle.Curvature = curvature; second.Circle.ReferenceToolface = secondToolface;
        Assert.That(second.CalculateLDT(), Is.True);
        return (first, second);
    }

    private static (ConstantCurvatureAndToolfaceArcSection, ConstantCurvatureAndToolfaceArcSection)
        ConstantToolfaceReference(TrajectoryPoint3D start, double firstLength, double secondLength,
            double curvature, double firstToolface, double secondToolface)
    {
        ConstantCurvatureAndToolfaceArcSection first = new(start, new TrajectoryPoint3D());
        first.CTCCurve.Length = firstLength; first.CTCCurve.Curvature = curvature; first.CTCCurve.Toolface = firstToolface;
        Assert.That(first.CalculateLDT(), Is.True);
        ConstantCurvatureAndToolfaceArcSection second = new(first.End, new TrajectoryPoint3D());
        second.CTCCurve.Length = secondLength; second.CTCCurve.Curvature = curvature; second.CTCCurve.Toolface = secondToolface;
        Assert.That(second.CalculateLDT(), Is.True);
        return (first, second);
    }

    private static BuildAndTurnArcSection BuildTurn(TrajectoryPoint3D start, double length, double build, double turn)
    {
        BuildAndTurnArcSection section = new(start, new TrajectoryPoint3D());
        section.BuildAndTurn.Length = length; section.BuildAndTurn.BUR = build; section.BuildAndTurn.TR = turn;
        Assert.That(section.CalculateLBT(), Is.True);
        return section;
    }

    private static double Curvature(double build, double turn, double inclination) =>
        System.Math.Sqrt(build * build + turn * turn * System.Math.Sin(inclination) * System.Math.Sin(inclination));

    private static void AssertEndpoint(TrajectoryPoint3D actual, TrajectoryPoint3D expected)
    {
        Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(1e-5));
        Assert.That(actual.Inclination, Is.EqualTo(expected.Inclination).Within(1e-6));
        double delta = System.Math.Atan2(System.Math.Sin(actual.Azimuth!.Value - expected.Azimuth!.Value),
            System.Math.Cos(actual.Azimuth.Value - expected.Azimuth.Value));
        Assert.That(delta, Is.EqualTo(0.0).Within(1e-6));
    }
}
