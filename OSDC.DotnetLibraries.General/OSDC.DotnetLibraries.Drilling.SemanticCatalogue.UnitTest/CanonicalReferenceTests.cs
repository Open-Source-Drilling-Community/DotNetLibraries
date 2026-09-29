using System.Text.Json.Nodes;
using NUnit.Framework;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class CanonicalReferenceTests
{
    private const string Profile = Catalogue.OsdcCanonicalDrilling;

    [Test]
    public void AlongHoleIsCurveCoordinateIndependentOfAcquisitionAndLegacyIdsStillResolve()
    {
        var c = Catalogue.Default;
        Assert.That(c.IsA(Concepts.TieInAlongHoleDepth, Concepts.CurvilinearAbscissa), Is.True);
        Assert.That(c.IsA(Concepts.AlongHoleDepth, Concepts.EllipsoidalDepth), Is.False);
        Assert.That(c.Get(Concepts.AlongHoleDepth).Definition, Does.Contain("planned, calculated, interpolated or measured"));
        Assert.That(c.Find("AlongHoleDepth").Select(x => x.Id), Does.Contain(Concepts.AlongHoleDepth));
        Assert.That(c.Get(Concepts.MeasuredDepth).SupersededBy, Is.EqualTo(Concepts.AlongHoleDepth));
        Assert.That(c.Get(Concepts.TieInMeasuredDepth).SupersededBy, Is.EqualTo(Concepts.TieInAlongHoleDepth));
        Assert.That(c.Quantity(Concepts.TieInAlongHoleDepth)!.Name, Is.EqualTo("DepthDrilling"));
    }

    [TestCase(Concepts.TieInAlongHoleDepth, Concepts.Wgs84AlongHoleOrigin)]
    [TestCase(Concepts.DrillFloorDepth, Concepts.Wgs84)]
    [TestCase(Concepts.RiemannianNorth, Concepts.Wgs84RiemannianCoordinates)]
    [TestCase(Concepts.RiemannianEast, Concepts.Wgs84RiemannianCoordinates)]
    [TestCase(Concepts.WellboreAzimuth, Concepts.TrueNorthClockwise)]
    [TestCase(Concepts.WellboreInclination, Concepts.Wgs84DownwardNormal)]
    [TestCase(Concepts.AbsolutePressure, Concepts.Vacuum)]
    public void CanonicalReferencesAreInheritedAndMaterialized(string concept, string reference)
    {
        var metadata = SemanticMetadata.Create(concept, referenceProfile: Profile);
        Assert.That(metadata["reference"]!.GetValue<string>(), Is.EqualTo(reference));
        Assert.That(metadata["referenceScope"]!.GetValue<string>(), Is.EqualTo("canonical-storage-and-api"));
        Assert.That(metadata["presentationReferencesAllowed"]!.GetValue<bool>(), Is.True);
        Assert.That(metadata["referenceProfileVersion"]!.GetValue<string>(), Is.EqualTo("1.0.0"));
        Assert.That(metadata["referenceDefinition"]!.GetValue<string>(), Is.Not.Empty);
    }

    [Test]
    public void ContradictoryReferencesAndUnknownProfilesFail()
    {
        Assert.Throws<InvalidDataException>(() => SemanticMetadata.Create(Concepts.TieInAlongHoleDepth,
            reference: Concepts.Wgs84, referenceProfile: Profile));
        Assert.Throws<InvalidDataException>(() => SemanticMetadata.Create(Concepts.WellboreInclination,
            reference: Concepts.Ned, referenceProfile: Profile));
        Assert.Throws<InvalidDataException>(() => SemanticMetadata.Create(Concepts.AbsolutePressure,
            reference: Concepts.Wgs84, referenceProfile: Profile));
        Assert.Throws<KeyNotFoundException>(() => SemanticMetadata.Create(Concepts.AbsolutePressure,
            referenceProfile: "urn:missing"));
    }

    [Test]
    public void UncertaintiesMagneticDipAndConversionCoordinatesDoNotAcquireUnrelatedReferences()
    {
        foreach (string concept in new[] { Concepts.LinearStandardUncertainty, Concepts.AngularStandardUncertainty,
            Concepts.MagneticDip, Concepts.GeoidReferencedDepth, Concepts.GridConvergence })
            Assert.That(SemanticMetadata.Create(concept, referenceProfile: Profile)["reference"], Is.Null);
        Assert.That(SemanticMetadata.Create(Concepts.Latitude)["reference"], Is.Null,
            "Generic geodesy conversion coordinates must retain their explicit source/target datum context.");
        Assert.That(SemanticMetadata.Create(Concepts.GeoidReferencedDepth, reference: Concepts.Egm84Geoid)["reference"]!
            .GetValue<string>(), Is.EqualTo(Concepts.Egm84Geoid));
    }

    private sealed class Model
    {
        [Semantic(Concepts.TieInAlongHoleDepth, ReferenceProfile = Profile)]
        public double TieInPointAlongHoleDepth { get; set; }
    }

    [Test]
    public void ModelAndProviderFactoriesProduceEquivalentContractsWithoutAddingPayloadFields()
    {
        var attribute = SemanticMetadata.For(typeof(Model).GetProperty(nameof(Model.TieInPointAlongHoleDepth))!)!;
        var registry = SemanticMetadata.Create(Concepts.TieInAlongHoleDepth, referenceProfile: Profile);
        Assert.That(JsonNode.DeepEquals(attribute, registry), Is.True);
        var schema = JsonNode.Parse("{\"properties\":{\"TieInPointAlongHoleDepth\":{\"type\":\"number\"}}}")!.AsObject();
        SemanticMetadata.AnnotateObject(schema, typeof(Model));
        Assert.That(schema["properties"]!.AsObject(), Has.Count.EqualTo(1));
        Assert.That(schema["properties"]!["TieInPointAlongHoleDepth"]!["type"]!.GetValue<string>(), Is.EqualTo("number"));
        Assert.That(JsonNode.DeepEquals(schema["properties"]!["TieInPointAlongHoleDepth"]![SemanticMetadata.ExtensionName], registry), Is.True);
        Assert.That(attribute["referenceRequiredContext"]!.ToJsonString(), Does.Contain("extension model"));
    }

    [Test]
    public void ConflictingInheritedReferencesAreRejectedAtLoadAndProfilesAreImmutableSnapshots()
    {
        var original = Catalogue.Default.Document;
        var profile = original.ReferenceProfiles.Single();
        var conflict = new CanonicalReferenceBinding(Concepts.TieInAlongHoleDepth, Concepts.Wgs84);
        Assert.Throws<InvalidDataException>(() => new Catalogue(original with
        { ReferenceProfiles = [profile with { Bindings = profile.Bindings.Append(conflict).ToArray() }] }));
        var bindings = profile.Bindings.ToArray();
        var catalogue = new Catalogue(original with { ReferenceProfiles = [profile with { Bindings = bindings }] });
        bindings[0] = conflict;
        Assert.That(catalogue.CanonicalReference(Concepts.EllipsoidalDepth), Is.EqualTo(Concepts.Wgs84));
        Assert.That(Catalogue.Load(catalogue.ToJson()).CanonicalReference(Concepts.TieInAlongHoleDepth),
            Is.EqualTo(Concepts.Wgs84AlongHoleOrigin));
        Assert.Throws<InvalidDataException>(() => new Catalogue(original with { SchemaVersion = 1 }));
    }

    [Test]
    public void ReferenceDefinitionRequiresPathIntersectionNotVerticalOffset()
    {
        var reference = Catalogue.Default.Get(Concepts.Wgs84AlongHoleOrigin);
        Assert.That(reference.Definition, Does.Contain("intersection"));
        Assert.That(reference.Definition, Does.Contain("not obtained by adding or subtracting a vertical elevation offset"));
        Assert.That(reference.RequiredContext, Does.Contain("selected intersection when multiple intersections exist"));
    }
}
