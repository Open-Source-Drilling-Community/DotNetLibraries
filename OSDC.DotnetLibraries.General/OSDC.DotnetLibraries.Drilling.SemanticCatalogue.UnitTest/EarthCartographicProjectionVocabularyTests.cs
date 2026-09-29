using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class EarthCartographicProjectionVocabularyTests
{
    [Test]
    public void CuratedProjectionVocabularyPreservesRepresentationAndOperationBoundaries()
    {
        var c = Catalogue.Default;
        Assert.That(c.Document.Version, Is.EqualTo("0.6.0"));
        Assert.That(c.Document.Concepts, Has.Count.EqualTo(156));
        Assert.That(c.Document.Concepts.All(x => x.Status == CurationStatus.Reviewed), Is.True);
        Assert.That(c.IsA(Concepts.GeographicPosition2D, Concepts.GenericGeodeticPosition), Is.False);
        Assert.That(c.IsA(Concepts.GeographicPosition2D, Concepts.Position), Is.False);
        Assert.That(c.Get(Concepts.GeographicPosition2D).Relations.Select(x => x.Target),
            Is.EquivalentTo(new[] { Concepts.Latitude, Concepts.Longitude }));
        Assert.That(c.IsA(Concepts.CartographicProjection, Concepts.CoordinateOperation), Is.True);
        Assert.That(c.IsA(Concepts.CartographicProjection, Concepts.CoordinateConversion), Is.True);
        Assert.That(c.IsA(Concepts.CartographicProjection, Concepts.GeodeticTransformation), Is.False);
        Assert.That(c.IsA(Concepts.ProjectedCoordinateReferenceSystem, Concepts.CoordinateReferenceSystem), Is.True);
        Assert.That(c.IsA(Concepts.ProjectedCoordinateSystem, Concepts.CoordinateReferenceSystem), Is.False);
        Assert.That(c.IsA(Concepts.FirstStandardParallel, Concepts.StandardParallel), Is.True);
        Assert.That(c.IsA(Concepts.PseudoStandardParallel, Concepts.StandardParallel), Is.False);
    }

    [Test]
    public void EngineeringQuantitiesResolveThroughPublishedUnitConversion()
    {
        var c = Catalogue.Default;
        var scale = c.Quantity(Concepts.ProjectionScaleFactor)!;
        Assert.That(scale.Id, Is.EqualTo(ProjectionScaleFactorQuantity.Instance.ID));
        Assert.That(scale.Id, Is.Not.EqualTo(HelmertScaleDifferenceQuantity.Instance.ID));
        Assert.That(scale.Name, Is.EqualTo("ProjectionScaleFactor"));
        Assert.That(c.SiUnit(Concepts.ProjectionScaleFactor), Is.EqualTo("1"));
        Assert.That(ProjectionScaleFactorQuantity.Instance.MeaningfulPrecisionInSI, Is.EqualTo(1e-9));
        Assert.That(ProjectionScaleFactorQuantity.Instance.FromSIString(0.9996, "dimensionless"), Is.EqualTo("0.999600000"));
        foreach (var id in new[] { Concepts.Easting, Concepts.Northing })
        {
            Assert.That(c.IsA(id, Concepts.Coordinate), Is.True);
            Assert.That(c.Quantity(id)!.Id, Is.EqualTo(PositionDrillingQuantity.Instance.ID));
            Assert.That(c.SiUnit(id), Is.EqualTo("m"));
        }
        Assert.That(c.Quantity(Concepts.ProjectionLinearParameter)!.Id, Is.EqualTo(LengthSmallQuantity.Instance.ID));
        Assert.That(c.Quantity(Concepts.GridConvergence)!.Id, Is.EqualTo(PlaneAngleGeodesicQuantity.Instance.ID));
        Assert.That(c.Quantity(Concepts.ProjectionAngularParameter)!.Id, Is.EqualTo(PlaneAngleGeodesicQuantity.Instance.ID));
        Assert.That(c.Quantity(Concepts.CartographicProjectionParameter), Is.Null);
        Assert.That(c.Quantity(Concepts.LinearUnitConversionFactor), Is.Null);
        Assert.That(c.Quantity(Concepts.CoordinateAxisDirection), Is.Null);
    }

    private sealed class ProjectionBindings
    {
        [Semantic(Concepts.ProjectionScaleFactor, Role = Concepts.NaturalOrigin)]
        public double Scale { get; set; }

        [Semantic(Concepts.GridConvergence, Reference = Concepts.GridConvergenceTrueToGridClockwise)]
        public double? Convergence { get; set; }
    }

    [Test]
    public void ProviderMetadataPublishesReviewedQuantityRolesAndConventions()
    {
        var scale = SemanticMetadata.For(typeof(ProjectionBindings).GetProperty("Scale")!)!;
        Assert.That(scale["catalogueVersion"]!.GetValue<string>(), Is.EqualTo("0.6.0"));
        Assert.That(scale["curationStatus"]!.GetValue<string>(), Is.EqualTo("Reviewed"));
        Assert.That(scale["physicalQuantity"]!["name"]!.GetValue<string>(), Is.EqualTo("ProjectionScaleFactor"));
        Assert.That(scale["role"]!.GetValue<string>(), Is.EqualTo(Concepts.NaturalOrigin));
        var convergence = SemanticMetadata.For(typeof(ProjectionBindings).GetProperty("Convergence")!)!;
        Assert.That(convergence["reference"]!.GetValue<string>(), Is.EqualTo(Concepts.GridConvergenceTrueToGridClockwise));
        Assert.That(Catalogue.Default.RequiredContext(Concepts.ProjectionScaleFactor), Does.Contain("parameter identity"));
        Assert.That(Catalogue.Default.RequiredContext(Concepts.Easting), Does.Contain("coordinate origin"));
    }
}
