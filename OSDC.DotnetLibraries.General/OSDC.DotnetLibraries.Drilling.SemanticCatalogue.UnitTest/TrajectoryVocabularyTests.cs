using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class TrajectoryVocabularyTests
{
    [Test]
    public void TrajectoryIncrementIsReviewedAndPreservesThePreviousBaseline()
    {
        var c = Catalogue.Default;
        Assert.That(c.Document.Version, Is.EqualTo("0.23.0"));
        Assert.That(c.Document.Concepts, Has.Count.EqualTo(703));
        Assert.That(c.Document.Concepts.Take(421), Has.Count.EqualTo(421));
        Assert.That(c.Document.Concepts.Skip(421).Take(96), Has.Count.EqualTo(96));
        Assert.That(c.Document.Concepts.Skip(421).Take(96).All(x => x.Status == CurationStatus.Reviewed), Is.True);
        Assert.That(c.Document.Concepts.Skip(421).Take(96).All(x => x.Evidence.Count > 0), Is.True);
    }

    [Test]
    public void CurveCurvatureCharacteristicsAreReusableAcrossCalculationContexts()
    {
        var c = Catalogue.Default;
        Assert.Multiple(() =>
        {
            Assert.That(c.Get(Concepts.CurveCurvatureCharacteristics).Kind, Is.EqualTo(SemanticKind.Noun));
            Assert.That(c.Get(Concepts.MinimumValue).Kind, Is.EqualTo(SemanticKind.Role));
            Assert.That(c.Get(Concepts.JunctionValue).Kind, Is.EqualTo(SemanticKind.Role));
            Assert.That(c.Get(Concepts.MaximumValue).Kind, Is.EqualTo(SemanticKind.Role));
            Assert.That(c.Get(Concepts.CurveCurvatureCharacteristics).Relations.Any(relation =>
                relation.Kind == SemanticRelationKind.HasPart && relation.Target == Concepts.WellboreCurvature), Is.True);
            Assert.That(c.IsA(Concepts.LeadInContinuationSection, Concepts.SolvedTrajectorySection), Is.True);
            Assert.That(c.IsA(Concepts.UpstreamSteeringSection, Concepts.SolvedTrajectorySection), Is.True);
            Assert.That(c.IsA(Concepts.DownstreamSteeringSection, Concepts.SolvedTrajectorySection), Is.True);
        });
    }

    [Test]
    public void CoreResourcesAndCalculationFamiliesAreRepresented()
    {
        var c = Catalogue.Default;
        Assert.Multiple(() =>
        {
            Assert.That(c.IsA(Concepts.SurveyRun, Concepts.Resource), Is.True);
            Assert.That(c.IsA(Concepts.Trajectory, Concepts.Resource), Is.True);
            Assert.That(c.IsA(Concepts.ActualSurveyRun, Concepts.SurveyRun), Is.True);
            Assert.That(c.IsA(Concepts.PlannedTrajectory, Concepts.Trajectory), Is.True);
            Assert.That(c.IsA(Concepts.TrajectoryAggregationCase, Concepts.CalculationCase), Is.True);
            Assert.That(c.IsA(Concepts.TrajectoryRealizationCase, Concepts.CalculationCase), Is.True);
            Assert.That(c.IsA(Concepts.TrajectoryExtrapolationCase, Concepts.CalculationCase), Is.True);
            Assert.That(c.IsA(Concepts.TargetLandingCase, Concepts.CalculationCase), Is.True);
            Assert.That(c.IsA(Concepts.DirectionalControlEvaluationCase, Concepts.CalculationCase), Is.True);
            Assert.That(c.IsA(Concepts.MinimumDistanceCalculation, Concepts.CalculationCase), Is.True);
            Assert.That(c.IsA(Concepts.GlobalAntiCollisionCalculation, Concepts.CalculationCase), Is.True);
        });
    }

    [Test]
    public void AggregationInputsAndOrderedCollectionRolesAreExplicit()
    {
        var c = Catalogue.Default;
        Assert.Multiple(() =>
        {
            Assert.That(c.Quantity(Concepts.TrajectoryAggregationCentreCoordinateConstancyTolerance)!.Name,
                Is.EqualTo("LengthStandard"));
            Assert.That(c.SiUnit(Concepts.TrajectoryAggregationCentreCoordinateConstancyTolerance), Is.EqualTo("m"));
            Assert.That(c.Quantity(Concepts.TrajectoryAggregationCurvatureConstancyTolerance)!.Name,
                Is.EqualTo("CurvatureDrilling"));
            Assert.That(c.SiUnit(Concepts.TrajectoryAggregationCurvatureConstancyTolerance), Is.EqualTo("rad/m"));
            Assert.That(c.Get(Concepts.SequencePosition).Kind, Is.EqualTo(SemanticKind.Role));
            Assert.That(c.Get(Concepts.ExactCollectionCardinality).Kind, Is.EqualTo(SemanticKind.Role));
        });
    }

    [Test]
    public void AntiCollisionPlanningSemanticsAreExplicitAndProviderNeutral()
    {
        var c = Catalogue.Default;
        Assert.Multiple(() =>
        {
            Assert.That(c.Get(Concepts.ReferenceResource).Kind, Is.EqualTo(SemanticKind.Role));
            Assert.That(c.Get(Concepts.ComparisonResource).Kind, Is.EqualTo(SemanticKind.Role));
            Assert.That(c.IsA(Concepts.AntiCollisionPolicyMatchedState, Concepts.AntiCollisionPolicyEvaluationState), Is.True);
            Assert.That(c.IsA(Concepts.AntiCollisionPolicyNoMatchState, Concepts.AntiCollisionPolicyEvaluationState), Is.True);
            Assert.That(c.IsA(Concepts.AntiCollisionPolicyIndeterminateState, Concepts.AntiCollisionPolicyEvaluationState), Is.True);
            Assert.That(c.IsA(Concepts.AntiCollisionNormalClassification, Concepts.AntiCollisionSafetyClassification), Is.True);
            Assert.That(c.IsA(Concepts.AntiCollisionAlertClassification, Concepts.AntiCollisionSafetyClassification), Is.True);
            Assert.That(c.IsA(Concepts.AntiCollisionAlarmClassification, Concepts.AntiCollisionSafetyClassification), Is.True);
            Assert.That(c.IsA(Concepts.AntiCollisionIndeterminateClassification, Concepts.AntiCollisionSafetyClassification), Is.True);
            Assert.That(c.Quantity(Concepts.AntiCollisionAlertThreshold)!.Name, Is.EqualTo("ProportionStandard"));
            Assert.That(c.Quantity(Concepts.AntiCollisionAlarmThreshold)!.Name, Is.EqualTo("ProportionStandard"));
        });
    }

    [Test]
    public void CurveAndLandingTaxonomiesPreserveDomainDistinctions()
    {
        var c = Catalogue.Default;
        Assert.Multiple(() =>
        {
            Assert.That(c.IsA(Concepts.CircularArcSection, Concepts.AggregatedTrajectorySection), Is.True);
            Assert.That(c.IsA(Concepts.ConstantCurvatureToolfaceSection, Concepts.AggregatedTrajectorySection), Is.True);
            Assert.That(c.IsA(Concepts.ConstantBuildTurnSection, Concepts.AggregatedTrajectorySection), Is.True);
            Assert.That(c.IsA(Concepts.GeologicalTargetBoundary, Concepts.TargetBoundary), Is.True);
            Assert.That(c.IsA(Concepts.DrillerTargetBoundary, Concepts.TargetBoundary), Is.True);
            Assert.That(c.IsA(Concepts.ReachableTargetBoundary, Concepts.TargetBoundary), Is.True);
            Assert.That(c.IsA(Concepts.FreeLandingAttitude, Concepts.LandingAttitudeMode), Is.True);
            Assert.That(c.IsA(Concepts.PerpendicularLandingAttitude, Concepts.LandingAttitudeMode), Is.True);
            Assert.That(c.Get(Concepts.ReachableTargetBoundary).Constraints,
                Does.Contain("Multiple disjoint contours and interior holes are permitted when produced by the solution topology."));
        });
    }

    [TestCase(Concepts.TrueVerticalDepth, "DepthDrilling", "m")]
    [TestCase(Concepts.WellboreCurvature, "CurvatureDrilling", "rad/m")]
    [TestCase(Concepts.BuildRate, "CurvatureDrilling", "rad/m")]
    [TestCase(Concepts.TurnRate, "CurvatureDrilling", "rad/m")]
    [TestCase(Concepts.Toolface, "PlaneAngleDrilling", "rad")]
    [TestCase(Concepts.InterpolationInterval, "DepthDrilling", "m")]
    [TestCase(Concepts.MaximumChordArcDistance, "LengthStandard", "m")]
    [TestCase(Concepts.ConfidenceFactor, "ProportionStandard", "1")]
    [TestCase(Concepts.CalculationProgress, "ProportionStandard", "1")]
    [TestCase(Concepts.CurvatureResidual, "CurvatureDrilling", "rad/m")]
    [TestCase(Concepts.ToolfaceResidual, "PlaneAngleDrilling", "rad")]
    [TestCase(Concepts.CenterToCenterDistance, "PositionDrilling", "m")]
    [TestCase(Concepts.ClearanceDistance, "LengthStandard", "m")]
    [TestCase(Concepts.SeparationFactor, "ProportionStandard", "1")]
    public void TrajectoryQuantitiesResolveWithoutAddingPhysicalQuantities(string concept, string quantity, string unit)
    {
        var c = Catalogue.Default;
        Assert.That(c.Quantity(concept)!.Name, Is.EqualTo(quantity));
        Assert.That(c.SiUnit(concept), Is.EqualTo(unit));
    }

    [Test]
    public void ReferencesAndResidualsCarryTheRequiredSemantics()
    {
        var c = Catalogue.Default;
        Assert.Multiple(() =>
        {
            Assert.That(c.Get(Concepts.GeodeticVertical).Kind, Is.EqualTo(SemanticKind.Reference));
            Assert.That(c.Get(Concepts.TrueNorth).Kind, Is.EqualTo(SemanticKind.Reference));
            Assert.That(c.IsA(Concepts.GeodeticVertical, Concepts.SurveyInclinationReference), Is.True);
            Assert.That(c.IsA(Concepts.MagneticNorth, Concepts.SurveyAzimuthReference), Is.True);
            Assert.That(c.IsA(Concepts.CurvatureResidual, Concepts.DirectionalControlResidual), Is.True);
            Assert.That(c.IsA(Concepts.ToolfaceResidual, Concepts.DirectionalControlResidual), Is.True);
            Assert.That(c.Get(Concepts.ToolfaceResidual).Constraints.Single(), Does.Contain("circular angle"));
            Assert.That(c.Get(Concepts.ExternalReferenceValidation).Constraints.Single(),
                Does.Contain("unavailability is not evidence"));
        });
    }
}
