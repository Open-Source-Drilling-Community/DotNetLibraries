using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public sealed class RadialProfileVocabularyTests
{
    [Test]
    public void RadialProfileDefinesOrderedBoundariesMaterialsAndEnvelopeProjection()
    {
        var catalogue = Catalogue.Default;

        Assert.Multiple(() =>
        {
            Assert.That(catalogue.IsA(Concepts.RadialProfileAtAbscissaEvaluation, Concepts.StatelessEvaluation), Is.True);
            Assert.That(catalogue.IsA(Concepts.OutermostKnownPhysicalEnvelopeDiameter, Concepts.RadialBoundaryDiameter), Is.True);
            Assert.That(catalogue.IsA(Concepts.InnermostKnownPhysicalEnvelopeDiameter, Concepts.RadialBoundaryDiameter), Is.True);
            Assert.That(catalogue.IsA(Concepts.DeepestCasingShoeEvaluation, Concepts.StatelessEvaluation), Is.True);
            Assert.That(catalogue.IsA(Concepts.CasingShoeAlongHoleDepth, Concepts.AlongHoleDepth), Is.True);
            Assert.That(catalogue.IsA(Concepts.BoreholeWallBoundary, Concepts.RadialBoundaryKind), Is.True);
            Assert.That(catalogue.IsA(Concepts.CementOuterBoundary, Concepts.RadialBoundaryKind), Is.True);
            Assert.That(catalogue.IsA(Concepts.CementInnerBoundary, Concepts.RadialBoundaryKind), Is.True);
            Assert.That(catalogue.IsA(Concepts.CasingOuterBoundary, Concepts.RadialBoundaryKind), Is.True);
            Assert.That(catalogue.IsA(Concepts.CasingInnerBoundary, Concepts.RadialBoundaryKind), Is.True);
            Assert.That(catalogue.IsA(Concepts.FormationMaterial, Concepts.RadialMaterialKind), Is.True);
            Assert.That(catalogue.IsA(Concepts.CementMaterial, Concepts.RadialMaterialKind), Is.True);
            Assert.That(catalogue.IsA(Concepts.CasingMaterial, Concepts.RadialMaterialKind), Is.True);
            Assert.That(catalogue.Quantity(Concepts.RadialBoundaryDiameter)?.SiUnitName, Is.EqualTo("metre"));
            Assert.That(catalogue.Quantity(Concepts.OutermostKnownPhysicalEnvelopeDiameter)?.SiUnitName, Is.EqualTo("metre"));
            Assert.That(catalogue.Quantity(Concepts.InnermostKnownPhysicalEnvelopeDiameter)?.SiUnitName, Is.EqualTo("metre"));
        });
    }

    [Test]
    public void VersionIs0200AndEveryPublicConstantResolves() =>
        Assert.Multiple(() =>
        {
            Assert.That(Catalogue.Default.Document.Version, Is.EqualTo("0.20.0"));
            foreach (var field in typeof(Concepts).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                if (field.IsLiteral && field.FieldType == typeof(string) && field.GetRawConstantValue() is string id)
                    Assert.That(() => Catalogue.Default.Get(id), Throws.Nothing, field.Name);
        });
}
