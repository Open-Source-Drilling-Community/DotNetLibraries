using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.UnitConversion.Conversion;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class EarthGeodesyVocabularyTests
{
    [Test]
    public void CuratedGeodesyIncrementKeepsPublishedIdentityMeaning()
    {
        var c = Catalogue.Default;
        Assert.That(c.Document.Version, Is.EqualTo("0.14.0"));
        Assert.That(c.Document.Concepts.Take(156).Count(x => x.Status == CurationStatus.Proposed), Is.Zero);
        Assert.That(c.Document.Concepts.Count(x => x.Status == CurationStatus.Reviewed), Is.EqualTo(574));
        Assert.That(c.Get(Concepts.ReferenceEllipsoid).Definition,
            Is.EqualTo("Identifier of the ellipsoid used for geographic coordinates."));
        Assert.That(c.IsA(Concepts.ReferenceEllipsoidDefinition, Concepts.ReferenceEllipsoid), Is.False);
        Assert.That(c.Get(Concepts.ReferenceEllipsoidDefinition).Relations.Select(x => x.Target),
            Is.EquivalentTo(new[] { Concepts.EllipsoidAxisLength, Concepts.InverseFlattening }));
        Assert.That(c.IsA(Concepts.DynamicGeodeticReferenceFrame, Concepts.GeodeticReferenceObject), Is.True);
        Assert.That(c.IsA(Concepts.StaticGeodeticReferenceFrame, Concepts.GeodeticReferenceFrame), Is.True);
        Assert.That(c.IsA(Concepts.GeodeticDatumEnsemble, Concepts.GeodeticReferenceObject), Is.True);
        Assert.That(c.IsA(Concepts.GeodeticDatumEnsemble, Concepts.GeodeticReferenceFrame), Is.False);
        Assert.That(c.IsA(Concepts.CoordinateReferenceSystem, Concepts.GeodeticReferenceObject), Is.False);
        Assert.That(c.IsA(Concepts.GeodeticTransformation, Concepts.CoordinateOperation), Is.True);
    }

    [Test]
    public void EpochsAreTimePositionsAndParametersHaveNoInventedUniversalUnit()
    {
        var c = Catalogue.Default;
        foreach (string id in new[] { Concepts.CoordinateEpoch, Concepts.FrameReferenceEpoch, Concepts.AnchorEpoch, Concepts.RealizationEpoch })
        {
            Assert.That(c.IsA(id, Concepts.Instant), Is.True);
            Assert.That(c.Quantity(id), Is.Null);
            Assert.That(c.SiUnit(id), Is.Null);
            Assert.That(c.RequiredContext(id), Does.Contain("time scale"));
            Assert.That(c.RequiredContext(id), Does.Contain("calendar and decimal-year convention"));
        }
        Assert.That(c.IsA(Concepts.CoordinateEpoch, Concepts.FrameReferenceEpoch), Is.False);
        Assert.That(c.Quantity(Concepts.CoordinateOperationParameter), Is.Null);
        Assert.That(c.SiUnit(Concepts.CoordinateOperationParameter), Is.Null);
        Assert.That(c.RequiredContext(Concepts.CoordinateOperationParameter), Does.Contain("declared parameter unit or file reference"));
        Assert.That(c.Quantity(Concepts.TransformationSelectionToken), Is.Null);
        Assert.That(c.IsA(Concepts.TransformationSelectionToken, Concepts.ModelSelectionToken), Is.False);
    }

    [Test]
    public void QuantitiesPreserveGeodeticPrecisionAndSeparateChangesFromCoordinates()
    {
        var c = Catalogue.Default;
        foreach (string id in new[] { Concepts.EllipsoidAxisLength, Concepts.DatumEnsembleAccuracy, Concepts.GeocentricTranslation, Concepts.EllipsoidalDepthChange })
        {
            Assert.That(c.Quantity(id)!.Id, Is.EqualTo(LengthStandardQuantity.Instance.ID));
            Assert.That(c.SiUnit(id), Is.EqualTo("m"));
        }
        foreach (string id in new[] { Concepts.HelmertRotation, Concepts.PrimeMeridianLongitude })
        {
            Assert.That(c.Quantity(id)!.Id, Is.EqualTo(PlaneAngleGeodesicQuantity.Instance.ID));
            Assert.That(c.SiUnit(id), Is.EqualTo("rad"));
        }
        foreach (string id in new[] { Concepts.InverseFlattening, Concepts.HelmertScaleDifference })
        {
            Assert.That(c.Quantity(id)!.Id, Is.EqualTo(id == Concepts.InverseFlattening
                ? InverseFlatteningQuantity.Instance.ID : HelmertScaleDifferenceQuantity.Instance.ID));
            Assert.That(c.Quantity(id)!.Id, Is.Not.EqualTo(DimensionLessStandardQuantity.Instance.ID));
            Assert.That(c.SiUnit(id), Is.EqualTo("1"));
        }
        Assert.That(c.Quantity(Concepts.CoordinateOperationAccuracy)!.Id, Is.EqualTo(LengthQuantity.Instance.ID));
        Assert.That(InverseFlatteningQuantity.Instance.MeaningfulPrecisionInSI, Is.EqualTo(1e-9));
        Assert.That(HelmertScaleDifferenceQuantity.Instance.MeaningfulPrecisionInSI, Is.EqualTo(1e-12));
        Assert.That(c.Quantity(Concepts.TransformationPathAccuracy)!.Id, Is.EqualTo(LengthQuantity.Instance.ID));
        Assert.That(c.IsA(Concepts.EllipsoidalDepthChange, Concepts.Depth), Is.False);
        Assert.That(c.IsA(Concepts.EllipsoidalDepthChange, Concepts.GeoidUndulation), Is.False);
        Assert.That(c.IsA(Concepts.MaximumAbsoluteValue, Concepts.MaximumAbsoluteError), Is.False);
        Assert.That(c.IsA(Concepts.WesternBoundary, Concepts.LowerBound), Is.False);
    }

    private sealed class GeodesyBindings
    {
        [Semantic(Concepts.HelmertRotation, Role = Concepts.CartesianX)]
        public double RotationX { get; set; }
        [Semantic(Concepts.CoordinateEpoch, Reference = Concepts.Utc)]
        public DateTimeOffset CoordinateEpochUtc { get; set; }
        [Semantic(Concepts.EllipsoidalDepthChange, Role = Concepts.MaximumAbsoluteValue)]
        public double MaximumAbsoluteDepthChange { get; set; }
    }

    [Test]
    public void ProviderAnnotationsExposeReviewedStatusAndInheritedMeaning()
    {
        var rotation = SemanticMetadata.For(typeof(GeodesyBindings).GetProperty("RotationX")!)!;
        Assert.That(rotation["curationStatus"]!.GetValue<string>(), Is.EqualTo("Reviewed"));
        Assert.That(rotation["role"]!.GetValue<string>(), Is.EqualTo(Concepts.CartesianX));
        Assert.That(rotation["physicalQuantity"]!["name"]!.GetValue<string>(), Is.EqualTo("PlaneAngleGeodesic"));
        var epoch = SemanticMetadata.For(typeof(GeodesyBindings).GetProperty("CoordinateEpochUtc")!)!;
        Assert.That(epoch["reference"]!.GetValue<string>(), Is.EqualTo(Concepts.Utc));
        Assert.That(epoch.ContainsKey("physicalQuantity"), Is.False);
        var change = SemanticMetadata.For(typeof(GeodesyBindings).GetProperty("MaximumAbsoluteDepthChange")!)!;
        Assert.That(change["physicalQuantity"]!["name"]!.GetValue<string>(), Is.EqualTo("LengthStandard"));
    }
}
