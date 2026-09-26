using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class CatalogueTests
{
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
