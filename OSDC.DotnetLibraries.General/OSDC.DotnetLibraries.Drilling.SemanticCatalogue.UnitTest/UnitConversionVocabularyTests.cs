using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class UnitConversionVocabularyTests
{
    [Test]
    public void UnitConversionIncrementIsReviewedAndPreservesThePreviousBaseline()
    {
        var c = Catalogue.Default;
        Assert.That(c.Document.Version, Is.EqualTo("0.15.0"));
        Assert.That(c.Document.Concepts, Has.Count.EqualTo(596));
        Assert.That(c.Document.Concepts.Take(517), Has.Count.EqualTo(517));
        Assert.That(c.Document.Concepts.Skip(517).Take(60), Has.Count.EqualTo(60));
        Assert.That(c.Document.Concepts.Skip(517).Take(60).All(x => x.Status == CurationStatus.Reviewed), Is.True);
        Assert.That(c.Document.Concepts.Skip(517).Take(60).All(x => x.Evidence.Count > 0), Is.True);
    }

    [Test]
    public void MetrologyAndConversionTaxonomiesPreserveContractDistinctions()
    {
        var c = Catalogue.Default;
        Assert.Multiple(() =>
        {
            Assert.That(c.IsA(Concepts.DefaultUnitChoice, Concepts.UnitChoice), Is.True);
            Assert.That(c.IsA(Concepts.SiUnitChoice, Concepts.UnitChoice), Is.True);
            Assert.That(c.IsA(Concepts.SiUnitSystem, Concepts.UnitSystem), Is.True);
            Assert.That(c.IsA(Concepts.MetricUnitSystem, Concepts.UnitSystem), Is.True);
            Assert.That(c.IsA(Concepts.ImperialUnitSystem, Concepts.UnitSystem), Is.True);
            Assert.That(c.IsA(Concepts.UnitedStatesCustomaryUnitSystem, Concepts.UnitSystem), Is.True);
            Assert.That(c.IsA(Concepts.DirectUnitConversion, Concepts.UnitConversion), Is.True);
            Assert.That(c.IsA(Concepts.UnitSystemConversion, Concepts.UnitConversion), Is.True);
            Assert.That(c.IsA(Concepts.UnitConversionSet, Concepts.Resource), Is.True);
            Assert.That(c.IsA(Concepts.UnitSystemConversionSet, Concepts.Resource), Is.True);
        });
    }

    [Test]
    public void GenericConversionValuesDoNotClaimOneFixedPhysicalQuantity()
    {
        var c = Catalogue.Default;
        Assert.Multiple(() =>
        {
            Assert.That(c.Quantity(Concepts.UnitConversionInputValue), Is.Null);
            Assert.That(c.Quantity(Concepts.UnitConversionNumericResult), Is.Null);
            Assert.That(c.Quantity(Concepts.UnitConversionFormattedResult), Is.Null);
            Assert.That(c.Quantity(Concepts.ConversionScaleFactor), Is.Null);
            Assert.That(c.Quantity(Concepts.ConversionBias), Is.Null);
            Assert.That(c.Get(Concepts.UnitConversionInputValue).RequiredContext,
                Does.Contain("physical quantity"));
            Assert.That(c.Get(Concepts.MeaningfulPrecisionInSi).Constraints.Single(),
                Does.Contain("does not round"));
        });
    }

    [Test]
    public void ServiceDerivedAndFormattingSemanticsAreExplicit()
    {
        var c = Catalogue.Default;
        Assert.Multiple(() =>
        {
            Assert.That(c.Get(Concepts.SiUnitSystem).Constraints.Single(), Does.Contain("derived by the service"));
            Assert.That(c.Get(Concepts.UnitConversionNumericResult).Constraints.Single(),
                Does.Contain("must not alter"));
            Assert.That(c.Get(Concepts.PhysicalQuantitySynonym).Constraints.Single(),
                Does.Contain("does not change"));
            Assert.That(c.Get(Concepts.SemanticSubject).Kind, Is.EqualTo(SemanticKind.Role));
            Assert.That(c.Get(Concepts.ConversionSourceUnit).Kind, Is.EqualTo(SemanticKind.Role));
        });
    }
}
