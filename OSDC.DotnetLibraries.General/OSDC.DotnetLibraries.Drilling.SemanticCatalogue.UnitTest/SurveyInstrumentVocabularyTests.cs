using NUnit.Framework;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class SurveyInstrumentVocabularyTests
{
    [Test]
    public void SurveyInstrumentIncrementIsReviewedAndPreservesThePreviousBaseline()
    {
        var c = Catalogue.Default;
        Assert.That(c.Document.Version, Is.EqualTo("0.19.0"));
        Assert.That(c.Document.Concepts, Has.Count.EqualTo(650));
        Assert.That(c.Document.Concepts.Take(370), Has.Count.EqualTo(370));
        Assert.That(c.Document.Concepts.Skip(370).Take(51), Has.Count.EqualTo(51));
        Assert.That(c.Document.Concepts.Skip(370).Take(51).Count(x => x.Status == CurationStatus.Reviewed), Is.EqualTo(50));
        Assert.That(c.Document.Concepts.Skip(370).Take(51).Count(x => x.Status == CurationStatus.Deprecated), Is.EqualTo(1));
        Assert.That(c.Document.Concepts.Skip(370).Take(51).All(x => x.Evidence.Count > 0), Is.True);
    }

    [Test]
    public void SurveyInstrumentTaxonomySeparatesModelsTemplatesAndSnapshots()
    {
        var c = Catalogue.Default;
        Assert.That(c.IsA(Concepts.SurveyInstrument, Concepts.Resource), Is.True);
        Assert.That(c.IsA(Concepts.MwdSurveyInstrumentModel, Concepts.SurveyInstrumentModel), Is.True);
        Assert.That(c.IsA(Concepts.GyroSurveyInstrumentModel, Concepts.SurveyInstrumentModel), Is.True);
        Assert.That(c.IsA(Concepts.WolffDeWardtErrorModel, Concepts.PositionUncertaintyModel), Is.True);
        Assert.That(c.IsA(Concepts.IscwsaErrorModel, Concepts.PositionUncertaintyModel), Is.True);
        Assert.That(c.IsA(Concepts.SurveyErrorSourceTemplate, Concepts.SurveyErrorSource), Is.True);
        Assert.That(c.IsA(Concepts.EmbeddedSurveyErrorSourceSnapshot, Concepts.SurveyErrorSource), Is.True);
        Assert.That(c.Get(Concepts.SurveyInstrument).Relations.Select(x => x.Target),
            Is.EquivalentTo(new[] { Concepts.SurveyInstrumentModel, Concepts.SurveyErrorSource }));
    }

    [Test]
    public void IscwsaModesRetainTheirApprovedDistinctMeanings()
    {
        var c = Catalogue.Default;
        foreach (var mode in new[]
                 {
                     Concepts.RandomPropagationMode, Concepts.SystematicPropagationMode,
                     Concepts.GlobalPropagationMode, Concepts.WellByWellPropagationMode
                 })
            Assert.That(c.IsA(mode, Concepts.ErrorPropagationMode), Is.True, mode);

        Assert.That(c.Get(Concepts.GlobalPropagationMode).Definition, Does.Contain("all wells"));
        Assert.That(c.IsA(Concepts.StationaryGyroMode, Concepts.GyroOperatingMode), Is.True);
        Assert.That(c.IsA(Concepts.ContinuousGyroMode, Concepts.GyroOperatingMode), Is.True);
        Assert.That(c.Get(Concepts.GyroOperatingMode).Constraints.Any(x => x.Contains("either Stationary or Continuous")), Is.True);
        Assert.That(c.Get(Concepts.GyroSwitchingParameter).Constraints, Does.Contain("Permitted values are +1 and -1."));
        Assert.That(c.Get(Concepts.KOperatorImposed).Definition, Does.Contain("Opaque legacy"));
        Assert.That(c.Get(Concepts.ErrorSourceOrderingIndex).Constraints,
            Does.Contain("Do not use this index as the error-source identity."));
    }

    [TestCase(Concepts.EarthAngularVelocity, "AngularVelocityDrilling", "rad/s")]
    [TestCase(Concepts.SurveyInstrumentCantAngle, "PlaneAngleDrilling", "rad")]
    [TestCase(Concepts.SurveyToolRunningSpeed, "Velocity", "m/s")]
    [TestCase(Concepts.GyroReinitializationDistance, "LengthStandard", "m")]
    [TestCase(Concepts.GyroNoiseReductionFactor, "ProportionStandard", "1")]
    [TestCase(Concepts.GyroSwitchingParameter, "ProportionStandard", "1")]
    [TestCase(Concepts.RelativeAlongHoleDepthStandardUncertainty, "ProportionSmall", "1")]
    [TestCase(Concepts.InstrumentMisalignmentStandardUncertainty, "PlaneAngleDrilling", "rad")]
    [TestCase(Concepts.AlongHoleDepthStandardUncertainty, "DepthDrilling", "m")]
    [TestCase(Concepts.AccelerationStandardUncertainty, "AccelerationDrilling", "m/s^2")]
    [TestCase(Concepts.MagneticFluxDensityStandardUncertainty, "EarthMagneticFluxDensity", "T")]
    [TestCase(Concepts.ScaleFactorStandardUncertainty, "ProportionSmall", "1")]
    [TestCase(Concepts.AngularVelocityStandardUncertainty, "AngularVelocitySurveyInstrumentDrilling", "rad/s")]
    [TestCase(Concepts.AngularRandomWalkStandardUncertainty, "RandomWalkDrilling", "rad/√s")]
    [TestCase(Concepts.ReciprocalLengthStandardUncertainty, "ReciprocalLengthSurveyInstrumentDrilling", "1/m")]
    [TestCase(Concepts.AngleMagneticFluxDensityStandardUncertainty, "AngleMagneticFluxDensitySurveyInstrumentDrilling", "rad·T")]
    public void SurveyEngineeringConceptsResolveAuthoritativeQuantities(string concept, string quantity, string unit)
    {
        var c = Catalogue.Default;
        Assert.That(c.Quantity(concept)!.Name, Is.EqualTo(quantity));
        Assert.That(c.SiUnit(concept), Is.EqualTo(unit));
    }

    [Test]
    public void SurveyErrorMagnitudesAreOneSigmaAndOriginFree()
    {
        var c = Catalogue.Default;
        foreach (var concept in new[]
                 {
                     Concepts.RelativeAlongHoleDepthStandardUncertainty,
                     Concepts.InstrumentMisalignmentStandardUncertainty,
                     Concepts.TrueInclinationStandardUncertainty,
                     Concepts.SurveyReferenceStandardUncertainty,
                     Concepts.DrillStringMagneticInterferenceStandardUncertainty,
                     Concepts.GyroCompassStandardUncertainty,
                     Concepts.AlongHoleDepthStandardUncertainty
                 })
        {
            Assert.That(c.IsA(concept, Concepts.StandardUncertainty), Is.True, concept);
            Assert.That(c.CanonicalReference(concept), Is.Null, concept);
        }
        Assert.That(c.Get(Concepts.SurveyErrorMagnitude).Definition, Does.Contain("one-sigma"));
        Assert.That(c.Get(Concepts.AlongHoleDepthStandardUncertainty).Definition, Does.Contain("origin-free"));
        Assert.That(c.Quantity(Concepts.ErrorMagnitudeQuantityIdentifier), Is.Null);
    }

    [Test]
    public void CantAndInclinationRolesCarryExplicitContext()
    {
        var c = Catalogue.Default;
        var cant = SemanticMetadata.Create(Concepts.SurveyInstrumentCantAngle,
            reference: Concepts.OrthogonalBodyFrameCantConvention);
        Assert.That(cant["reference"]!.GetValue<string>(), Is.EqualTo(Concepts.OrthogonalBodyFrameCantConvention));
        Assert.That(c.Get(Concepts.OrthogonalBodyFrameCantConvention).Definition, Does.Contain("less than or equal to 90 degrees"));
        foreach (var role in new[]
                 {
                     Concepts.ErrorApplicabilityStart, Concepts.ErrorApplicabilityEnd,
                     Concepts.ErrorInitializationInclination
                 })
            Assert.That(c.Get(role).Kind, Is.EqualTo(SemanticKind.Role), role);
    }

    [Test]
    public void AmidIsAnAngularLegacyMeaningAndAmilIsItsFluxDensityReplacement()
    {
        var c = Catalogue.Default;
        var amid = c.Get(Concepts.AmidAxialMagneticInterferenceStandardUncertainty);
        Assert.That(amid.Status, Is.EqualTo(CurationStatus.Deprecated));
        Assert.That(amid.SupersededBy, Is.EqualTo(Concepts.AmilAxialMagneticInterferenceStandardUncertainty));
        Assert.That(c.Quantity(amid.Id)!.Name, Is.EqualTo("PlaneAngleDrilling"));
        Assert.That(c.SiUnit(amid.Id), Is.EqualTo("rad"));
        Assert.That(c.Quantity(Concepts.AmilAxialMagneticInterferenceStandardUncertainty)!.Name,
            Is.EqualTo("EarthMagneticFluxDensity"));
        Assert.That(c.SiUnit(Concepts.AmilAxialMagneticInterferenceStandardUncertainty), Is.EqualTo("T"));
    }
}
