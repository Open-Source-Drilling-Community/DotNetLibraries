using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class CatalogueTests
{
    [Test]
    public void ResultSeparatesPotentialFromVectorAndPreservesDepthPositionMeaning()
    {
        var catalogue = Catalogue.Default;
        Assert.That(catalogue.Get(Concepts.GravityResult).Relations.Select(r => r.Target),
            Is.EquivalentTo(new[] { Concepts.GravityVector, Concepts.TotalPotential }));
        Assert.That(catalogue.Get(Concepts.GravityVector).Relations.Select(r => r.Target), Does.Not.Contain(Concepts.TotalPotential));
        Assert.That(catalogue.Get(Concepts.Sample).Relations.Select(r => r.Target), Does.Contain(Concepts.GravityResult));
        Assert.That(catalogue.IsA(Concepts.Position, Concepts.GenericGeodeticPosition), Is.True);
        Assert.That(catalogue.Get(Concepts.Position).Relations.Select(r => r.Target), Does.Contain(Concepts.EllipsoidalDepth));
        Assert.That(catalogue.Get(Concepts.GenericGeodeticPosition).Relations.Select(r => r.Target), Does.Not.Contain(Concepts.EllipsoidalDepth));
        Assert.That(catalogue.Get(Concepts.Request).Constraints, Is.Empty);
        Assert.That(catalogue.Get(Concepts.Response).Constraints, Is.Empty);
        Assert.That(catalogue.Document.Concepts.Count(c => c.Status == CurationStatus.Reviewed), Is.EqualTo(647),
            "EarthGravity approved on 2026-09-26; digest, EarthMagneticField and EarthVerticalDatum on 2026-09-27; EarthGeodesy and EarthCartographicProjection on 2026-09-28.");
    }

    [Test]
    public void CuratedMagneticVocabularyPreservesScientificDistinctions()
    {
        var catalogue = Catalogue.Default;
        Assert.That(catalogue.Document.Concepts, Has.Count.EqualTo(650));
        Assert.That(catalogue.Document.Concepts.Count(c => c.Status == CurationStatus.Reviewed), Is.EqualTo(647));
        Assert.That(catalogue.IsA(Concepts.MagneticDip, Concepts.Dip), Is.True);
        Assert.That(catalogue.Quantity(Concepts.MagneticDip)!.Id, Is.EqualTo(PlaneAngleDrillingQuantity.Instance.ID));
        Assert.That(catalogue.Quantity(Concepts.MagneticDeclination)!.Id, Is.EqualTo(PlaneAngleDrillingQuantity.Instance.ID));
        Assert.That(catalogue.Quantity(Concepts.EarthMagneticFluxDensity)!.Id, Is.EqualTo(EarthMagneticFluxDensityQuantity.Instance.ID));
        Assert.That(catalogue.Quantity(Concepts.Instant), Is.Null, "An instant is not an elapsed duration.");
        Assert.That(catalogue.IsA(Concepts.ModelInfo, Concepts.ScientificModelProvenance), Is.True);
        Assert.That(catalogue.IsA(Concepts.GeomagneticModelProvenance, Concepts.ScientificModelProvenance), Is.True);
        Assert.That(catalogue.Get(Concepts.GeodeticEvaluationPoint).Relations.Select(r => r.Target),
            Is.EquivalentTo(new[] { Concepts.GenericGeodeticPosition, Concepts.Instant }));
        Assert.That(catalogue.IsA(Concepts.GeodeticEvaluationPoint, Concepts.GenericGeodeticPosition), Is.False);
        Assert.That(catalogue.Get(Concepts.LowerBound).Kind, Is.EqualTo(SemanticKind.Role));
        Assert.That(catalogue.Get(Concepts.Utc).Kind, Is.EqualTo(SemanticKind.Reference));
        Assert.That(catalogue.Find("inclination"), Is.Empty);
    }

    [Test]
    public void CuratedVerticalDatumVocabularyKeepsDepthSeparationAngleAndErrorDistinct()
    {
        var catalogue = Catalogue.Default;
        Assert.That(catalogue.Document.Concepts.Take(156).Count(c => c.Status == CurationStatus.Proposed), Is.Zero);
        Assert.That(catalogue.Get(Concepts.GeoidUndulation).Status, Is.EqualTo(CurationStatus.Reviewed));
        Assert.That(catalogue.IsA(Concepts.GeoidReferencedDepth, Concepts.Depth), Is.True);
        Assert.That(catalogue.IsA(Concepts.GeoidReferencedDepth, Concepts.EllipsoidalDepth), Is.False);
        Assert.That(catalogue.Quantity(Concepts.GeoidReferencedDepth)!.Id, Is.EqualTo(DepthDrillingQuantity.Instance.ID));
        Assert.That(catalogue.Quantity(Concepts.GeoidUndulation)!.Id, Is.EqualTo(LengthStandardQuantity.Instance.ID));
        Assert.That(LengthStandardQuantity.Instance.MeaningfulPrecisionInSI, Is.EqualTo(0.001));
        Assert.That(catalogue.SiUnit(Concepts.GeoidUndulation), Is.EqualTo("m"));
        Assert.That(catalogue.Quantity(Concepts.GeoidApproximationError)!.Id, Is.EqualTo(LengthQuantity.Instance.ID));
        Assert.That(catalogue.Quantity(Concepts.AngularGridSpacing)!.Id, Is.EqualTo(PlaneAngleGeodesicQuantity.Instance.ID));
        Assert.That(catalogue.SiUnit(Concepts.AngularGridSpacing), Is.EqualTo("rad"));
        Assert.That(catalogue.IsA(Concepts.GeoidModelProvenance, Concepts.ScientificModelProvenance), Is.True);
        Assert.That(catalogue.Get(Concepts.GeoidDepthPosition).Relations.Select(r => r.Target), Does.Contain(Concepts.GeoidReferencedDepth));
        Assert.That(catalogue.Get(Concepts.GeoidDepthPosition).Relations.Select(r => r.Target), Does.Not.Contain(Concepts.EllipsoidalDepth));
        Assert.That(catalogue.Get(Concepts.DatasetTimestamp).Kind, Is.EqualTo(SemanticKind.Role));
        Assert.That(catalogue.Get(Concepts.Egm84Geoid).Kind, Is.EqualTo(SemanticKind.Reference));
    }

    [Test]
    public void EarthGravityQuantitiesComeFromAuthoritativeLibraries()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Catalogue.Default.Quantity(Concepts.Latitude)!.Id, Is.EqualTo(PlaneAngleGeodesicQuantity.Instance.ID));
            Assert.That(Catalogue.Default.Quantity(Concepts.EllipsoidalDepth)!.Id, Is.EqualTo(DepthDrillingQuantity.Instance.ID));
            Assert.That(Catalogue.Default.Quantity(Concepts.GravityAcceleration)!.Id, Is.EqualTo(AccelerationDrillingQuantity.Instance.ID));
            Assert.That(Catalogue.Default.Quantity(Concepts.TotalPotential)!.Id, Is.EqualTo(EarthGravityPotentialQuantity.Instance.ID));
        });
    }

    [Test]
    public void HierarchyInheritsRequirementsWithoutTreatingQuantityAsParent()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Catalogue.Default.IsA(Concepts.Latitude, Concepts.Coordinate), Is.True);
            Assert.That(Catalogue.Default.IsA(Concepts.Latitude, Concepts.Longitude), Is.False);
            Assert.That(Catalogue.Default.RequiredContext(Concepts.Latitude), Does.Contain("reference"));
            Assert.That(Catalogue.Default.Constraints(Concepts.EllipsoidalDepth), Does.Contain("Positions are not additive extents."));
        });
    }

    private static SemanticDefinition Definition(string id, params string[] parents) => new()
    { Id = "urn:test:" + id, Label = id, Definition = id, Parents = parents.Select(p => "urn:test:" + p).ToArray() };
    private static Catalogue Make(params SemanticDefinition[] concepts) => new(new CatalogueDocument
    { Id = "urn:test:catalogue", Version = "0.1.0", Concepts = concepts });

    [Test]
    public void CyclesMissingParentsAndIncompatibleQuantitiesFail()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidDataException>(() => Make(Definition("a", "b"), Definition("b", "a")));
            Assert.Throws<InvalidDataException>(() => Make(Definition("a", "missing")));
            Assert.Throws<InvalidDataException>(() => Make(Definition("a") with { QuantityName = "not-a-quantity" }));
            Assert.Throws<InvalidDataException>(() => Make(Definition("a") with { QuantityName = "DepthDrilling" },
                Definition("b", "a") with { QuantityName = "AccelerationDrilling" }));
            Assert.Throws<InvalidDataException>(() => Make(Definition("a") with { Kind = SemanticKind.Role }, Definition("b", "a")));
        });
    }

    [Test]
    public void AliasesRemainAmbiguousAndCallerCannotMutateValidatedGraph()
    {
        string[] aliases = ["shared"];
        var catalogue = Make(Definition("a") with { Aliases = aliases }, Definition("b") with { Aliases = ["shared"] });
        aliases[0] = "changed";
        Assert.That(catalogue.Find("shared"), Has.Count.EqualTo(2));
    }

    [Test]
    public void PortableRoundTripPreservesDefinitionsAndEveryConstantResolves()
    {
        var restored = Catalogue.Load(Catalogue.Default.ToJson());
        Assert.That(restored.Document.Concepts, Has.Count.EqualTo(Catalogue.Default.Document.Concepts.Count));
        foreach (var field in typeof(Concepts).GetFields())
            Assert.DoesNotThrow(() => restored.Get((string)field.GetRawConstantValue()!));
    }

    private sealed class ValidBinding
    {
        [Semantic(Concepts.GravityAcceleration, Role = Concepts.North, Reference = Concepts.Ned)]
        public double Value { get; set; }
    }
    private sealed class DigestBindings
    {
        [Semantic(Concepts.Sha256FileDigest, Role = Concepts.CoefficientFile)]
        public string Coefficients { get; set; } = string.Empty;
        [Semantic(Concepts.Sha256FileDigest, Role = Concepts.ModelMetadataFile)]
        public string Metadata { get; set; } = string.Empty;
    }

    [Test]
    public void DigestsSeparateAlgorithmFromFilePurposeAndPreservePublishedIdentity()
    {
        var catalogue = Catalogue.Default;
        Assert.That(catalogue.IsA(Concepts.Sha256FileDigest, Concepts.FileContentDigest), Is.True);
        Assert.That(catalogue.IsA(Concepts.CoefficientHash, Concepts.Sha256FileDigest), Is.True);
        Assert.That(catalogue.Get(Concepts.CoefficientHash).Definition,
            Is.EqualTo("Hexadecimal SHA-256 digest of the installed coefficient file."));
        Assert.That(catalogue.Get(Concepts.CoefficientHash).Status, Is.EqualTo(CurationStatus.Reviewed));
        var coefficients = SemanticMetadata.For(typeof(DigestBindings).GetProperty("Coefficients")!)!;
        var metadata = SemanticMetadata.For(typeof(DigestBindings).GetProperty("Metadata")!)!;
        Assert.That(coefficients["concept"]!.GetValue<string>(), Is.EqualTo(metadata["concept"]!.GetValue<string>()));
        Assert.That(coefficients["role"]!.GetValue<string>(), Is.EqualTo(Concepts.CoefficientFile));
        Assert.That(metadata["role"]!.GetValue<string>(), Is.EqualTo(Concepts.ModelMetadataFile));
        Assert.That(coefficients.ContainsKey("physicalQuantity"), Is.False);
        Assert.That(coefficients.ContainsKey("siUnit"), Is.False);
        Assert.That(catalogue.RequiredContext(Concepts.Sha256FileDigest), Does.Contain("source file and its purpose"));
        Assert.That(catalogue.Quantity(Concepts.CoefficientHash), Is.Null);
    }

    private sealed class InvalidBinding
    {
        [Semantic(Concepts.Latitude, Role = Concepts.Longitude)]
        public double Value { get; set; }
    }

    [Test]
    public void BindingsCarryQuantityIdentityAndRejectWrongKind()
    {
        var metadata = SemanticMetadata.For(typeof(ValidBinding).GetProperty("Value")!)!;
        Assert.That(metadata["physicalQuantity"]!["id"]!.GetValue<string>(), Is.EqualTo(AccelerationDrillingQuantity.Instance.ID.ToString()));
        Assert.That(metadata["role"]!.GetValue<string>(), Is.EqualTo(Concepts.North));
        Assert.Throws<InvalidDataException>(() => SemanticMetadata.For(typeof(InvalidBinding).GetProperty("Value")!));
    }
}
