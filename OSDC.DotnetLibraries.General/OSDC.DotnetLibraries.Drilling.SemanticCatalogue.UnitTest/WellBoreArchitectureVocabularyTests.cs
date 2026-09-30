using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class WellBoreArchitectureVocabularyTests
{
    [Test]
    public void PublishedBaselineIsUnchangedAndNewDefinitionsAreReviewed()
    {
        var assembly = typeof(Catalogue).Assembly;
        using var stream = assembly.GetManifestResourceStream(
            assembly.GetManifestResourceNames().Single(x => x.EndsWith(".catalogue.json")))!;
        using var json = JsonDocument.Parse(stream);
        var original = json.RootElement.GetProperty("concepts").EnumerateArray().Take(232).ToArray();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(original,
            new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        Assert.That(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
            Is.EqualTo("54d11d4863e3afd27f065e6e7f212934d6cd14068e466fc19e1730b1073a5ae3"));
        Assert.That(Catalogue.Default.Document.Concepts.Skip(232).Take(60), Has.Count.EqualTo(60));
        Assert.That(Catalogue.Default.Document.Concepts.Skip(232).Take(60).All(x => x.Status == CurationStatus.Reviewed), Is.True);
    }

    [Test]
    public void GeometrySeparatesCoordinatesExtentsAndContainment()
    {
        var c = Catalogue.Default;
        Assert.That(c.IsA(Concepts.WellBoreArchitecture, Concepts.Resource), Is.True);
        Assert.That(c.IsA(Concepts.WellBoreArchitecture, Concepts.WellBore), Is.False);
        Assert.That(c.IsA(Concepts.HostComponentAbscissa, Concepts.CurvilinearAbscissa), Is.True);
        Assert.That(c.IsA(Concepts.HostComponentAbscissa, Concepts.AlongHoleDepth), Is.False);
        Assert.That(c.IsA(Concepts.PhysicalLengthExtent, Concepts.CurvilinearAbscissa), Is.False);
        Assert.That(c.Get(Concepts.OpenHoleSection).Relations,
            Does.Contain(new SemanticRelation(SemanticRelationKind.HasPart, Concepts.BoreholeSizeInterval)));
        Assert.That(c.IsA(Concepts.FluidLayerTopBoundary, Concepts.TopDepthBoundary), Is.True);
    }

    [Test]
    public void LocationBindingsResolveCorrectOriginsAndAllowPresentationReferences()
    {
        foreach (var role in new[] { Concepts.WellheadLocation, Concepts.CasingHangerLocation,
            Concepts.TubingHangerLocation, Concepts.FluidLayerTopBoundary })
            Assert.That(SemanticMetadata.Create(Concepts.EllipsoidalDepth, role: role,
                referenceProfile: Catalogue.OsdcCanonicalDrilling)["reference"]!.GetValue<string>(), Is.EqualTo(Concepts.Wgs84));
        foreach (var role in new[] { Concepts.CasingTopLocation, Concepts.TopOfCementLocation })
            Assert.That(SemanticMetadata.Create(Concepts.AlongHoleDepth, role: role,
                referenceProfile: Catalogue.OsdcCanonicalDrilling)["reference"]!.GetValue<string>(), Is.EqualTo(Concepts.Wgs84AlongHoleOrigin));
        var local = SemanticMetadata.Create(Concepts.HostComponentAbscissa, referenceProfile: Catalogue.OsdcCanonicalDrilling);
        Assert.That(local["reference"]!.GetValue<string>(), Is.EqualTo(Concepts.HostTopDownward));
        Assert.That(local["presentationReferencesAllowed"]!.GetValue<bool>(), Is.True);
        Assert.Throws<InvalidDataException>(() => SemanticMetadata.Create(Concepts.HostComponentAbscissa,
            reference: Concepts.Wgs84AlongHoleOrigin, referenceProfile: Catalogue.OsdcCanonicalDrilling));
    }

    [TestCase(Concepts.TensileStrength, "DrillStemMaterialStrengthDrilling", "Pa")]
    [TestCase(Concepts.TensileCapacity, "ForceDrilling", "N")]
    [TestCase(Concepts.TorsionalCapacity, "TorqueDrilling", "N·m")]
    [TestCase(Concepts.LinearMassDensity, "MassGradientPerLengthDrilling", "kg/m")]
    [TestCase(Concepts.MaterialDensity, "MassDensityDrilling", "kg/m³")]
    [TestCase(Concepts.PhysicalLengthExtent, "LengthStandard", "m")]
    [TestCase(Concepts.BurstPressureCapacity, "PressureDrilling", "Pa")]
    [TestCase(Concepts.CollapsePressureCapacity, "PressureDrilling", "Pa")]
    public void EngineeringQuantitiesHaveNoAbsoluteOrigin(string concept, string quantity, string unit)
    {
        Assert.That(Catalogue.Default.Quantity(concept)!.Name, Is.EqualTo(quantity));
        Assert.That(Catalogue.Default.SiUnit(concept), Is.EqualTo(unit));
        Assert.That(SemanticMetadata.Create(concept, referenceProfile: Catalogue.OsdcCanonicalDrilling)["reference"], Is.Null);
    }

    [Test]
    public void RepresentationsAndDispersionsDoNotAcquireMeasurandOrigins()
    {
        var c = Catalogue.Default;
        Assert.That(c.Quantity(Concepts.ScalarValueRepresentation), Is.Null);
        foreach (var definition in c.Document.Concepts.Skip(232).Take(60).Where(x => c.IsA(x.Id, Concepts.StandardUncertainty)))
        {
            Assert.That(c.Quantity(definition.Id), Is.Not.Null);
            Assert.That(SemanticMetadata.Create(definition.Id, referenceProfile: Catalogue.OsdcCanonicalDrilling)["reference"], Is.Null);
        }
        Assert.That(c.Get(Concepts.LinearStandardUncertainty).Definition, Does.Contain("position or depth"));
        Assert.That(c.Get(Concepts.DimensionalLengthStandardUncertainty).Definition, Does.Contain("extent or diameter"));
        Assert.That(c.IsA(Concepts.TensileCapacity, Concepts.TensileStrength), Is.False);
        Assert.That(c.IsA(Concepts.PressureDifference, Concepts.AbsolutePressure), Is.False);
    }
}
