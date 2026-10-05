using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class FieldClusterVocabularyTests
{
    [Test]
    public void PublishedDefinitionsAreUnchangedAndFieldClusterVocabularyIsCurated()
    {
        var c = Catalogue.Default;
        Assert.That(c.Document.Version, Is.EqualTo("0.14.0"));
        Assert.That(c.Document.Concepts, Has.Count.EqualTo(577));
        Assert.That(c.Document.Concepts.Count(x => x.Status == CurationStatus.Reviewed), Is.EqualTo(574));
        Assert.That(c.Document.Concepts.Skip(156).Take(52).All(x => x.Status == CurationStatus.Reviewed), Is.True);
        Assert.That(c.Document.Concepts.Skip(156).Take(52).All(x => x.Evidence.Count > 0), Is.True);

        var assembly = typeof(Catalogue).Assembly;
        using var stream = assembly.GetManifestResourceStream(
            assembly.GetManifestResourceNames().Single(x => x.EndsWith(".catalogue.json")))!;
        using var json = JsonDocument.Parse(stream);
        var originalEntries = json.RootElement.GetProperty("concepts").EnumerateArray().Take(156).ToArray();
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(originalEntries,
            new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        Assert.That(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
            Is.EqualTo("1b3937bb5029de41c815e409f57fde2dd2cde04da237318ba14f3ea5db7f05af"),
            "The 156 published 0.6.0 definitions must not change when adding the curated vocabulary.");
    }

    [Test]
    public void ClassificationSpecializesMeaningWithoutConfusingIdentityWithAssignment()
    {
        var c = Catalogue.Default;
        Assert.Multiple(() =>
        {
            Assert.That(c.IsA(Concepts.FeatureCategory, Concepts.ClassificationCategory), Is.True);
            Assert.That(c.IsA(Concepts.MembershipCategory, Concepts.ClassificationCategory), Is.True);
            Assert.That(c.IsA(Concepts.FeatureAssignment, Concepts.ClassificationAssignment), Is.True);
            Assert.That(c.IsA(Concepts.MembershipAssignment, Concepts.ClassificationAssignment), Is.True);
            Assert.That(c.IsA(Concepts.MembershipAssignment, Concepts.FeatureAssignment), Is.False);
            Assert.That(c.IsA(Concepts.IdentityAssignment, Concepts.IdentityDefinition), Is.False);
            Assert.That(c.IsA(Concepts.WellSlot, Concepts.WellCluster), Is.False);
            Assert.That(c.IsA(Concepts.WellCluster, Concepts.Field), Is.False);
            Assert.That(c.Quantity(Concepts.ResourceIdentifier), Is.Null);
            Assert.That(c.Quantity(Concepts.IdentityValue), Is.Null);
            Assert.That(c.Quantity(Concepts.CategoryExclusivity), Is.Null);
            Assert.That(c.Get(Concepts.IdentityAssignment).Relations.Select(x => x.Target), Does.Contain(Concepts.IdentityValue));
        });
    }

    [Test]
    public void RiemannianCoordinatesAreNotProjectedCoordinatesOrDisplacements()
    {
        var c = Catalogue.Default;
        foreach (var id in new[] { Concepts.RiemannianNorth, Concepts.RiemannianEast })
        {
            Assert.That(c.IsA(id, Concepts.Coordinate), Is.True);
            Assert.That(c.IsA(id, Concepts.ProjectedCoordinate), Is.False);
            Assert.That(c.Quantity(id)!.Id, Is.EqualTo(PositionDrillingQuantity.Instance.ID));
            Assert.That(c.SiUnit(id), Is.EqualTo("m"));
            Assert.That(c.Constraints(id), Does.Contain("Positions are not additive extents."));
        }
        Assert.That(c.RequiredContext(Concepts.RiemannianEast), Does.Contain("latitude"));
        Assert.That(c.Get(Concepts.Wgs84RiemannianCoordinates).Kind, Is.EqualTo(SemanticKind.Reference));
        Assert.That(c.Quantity(Concepts.DelineationMargin)!.Id, Is.EqualTo(LengthStandardQuantity.Instance.ID));
        Assert.That(c.IsA(Concepts.DelineationMargin, Concepts.Coordinate), Is.False);
    }

    [Test]
    public void DepthsArePositionsWhileUncertaintyIsDispersion()
    {
        var c = Catalogue.Default;
        foreach (var id in new[] { Concepts.GroundMudLineDepth, Concepts.WaterSurfaceDepth })
        {
            Assert.That(c.IsA(id, Concepts.EllipsoidalDepth), Is.True);
            Assert.That(c.Quantity(id)!.Id, Is.EqualTo(DepthDrillingQuantity.Instance.ID));
            Assert.That(c.SiUnit(id), Is.EqualTo("m"));
        }
        Assert.That(c.Quantity(Concepts.LinearStandardUncertainty)!.Id, Is.EqualTo(LengthStandardQuantity.Instance.ID));
        Assert.That(c.Quantity(Concepts.AngularStandardUncertainty)!.Id, Is.EqualTo(PlaneAngleGeodesicQuantity.Instance.ID));
        Assert.That(c.IsA(Concepts.LinearStandardUncertainty, Concepts.Coordinate), Is.False);
        Assert.That(c.IsA(Concepts.AngularStandardUncertainty, Concepts.Latitude), Is.False);
        Assert.That(c.Quantity(Concepts.GaussianUncertainValue), Is.Null);
        Assert.That(c.Quantity(Concepts.StandardUncertainty), Is.Null);
        Assert.That(c.RequiredContext(Concepts.GaussianUncertainValue), Does.Contain("underlying measurand concept"));
    }

    [Test]
    public void RolesPreserveInclusiveValidityAndRecordVersusObservationTime()
    {
        var c = Catalogue.Default;
        Assert.That(c.IsA(Concepts.ValidityStart, Concepts.LowerBound), Is.True);
        Assert.That(c.IsA(Concepts.ValidityEnd, Concepts.UpperBound), Is.True);
        Assert.That(c.IsA(Concepts.TopDepthBoundary, Concepts.LowerBound), Is.True);
        Assert.That(c.IsA(Concepts.BottomDepthBoundary, Concepts.UpperBound), Is.True);
        Assert.That(c.Get(Concepts.CreationTime).Kind, Is.EqualTo(SemanticKind.Role));
        Assert.That(c.Get(Concepts.LastModificationTime).Kind, Is.EqualTo(SemanticKind.Role));
        Assert.That(c.Quantity(Concepts.AssignmentValidityPeriod), Is.Null);
        Assert.That(c.Constraints(Concepts.AssignmentValidityPeriod), Does.Contain("Two periods touching at an endpoint overlap."));
    }

    private class ReviewedBindings
    {
        [Semantic(Concepts.DelineationMargin)]
        public double Margin { get; set; }

        [Semantic(Concepts.GroundMudLineDepth, Role = Concepts.ExpectedValue, Reference = Concepts.Wgs84)]
        public double Mean { get; set; }

        [Semantic(Concepts.LinearStandardUncertainty)]
        public double Sigma { get; set; }

        [Semantic(Concepts.Instant, Role = Concepts.ValidityStart, Reference = Concepts.Utc)]
        public DateTimeOffset? FromDate { get; set; }
    }

    [Test]
    public void ProviderMetadataResolvesReviewedQuantitiesAndKeepsRolesSeparate()
    {
        var margin = SemanticMetadata.For(typeof(ReviewedBindings).GetProperty("Margin")!)!;
        Assert.That(margin["catalogueVersion"]!.GetValue<string>(), Is.EqualTo("0.14.0"));
        Assert.That(margin["curationStatus"]!.GetValue<string>(), Is.EqualTo("Reviewed"));
        Assert.That(margin["physicalQuantity"]!["name"]!.GetValue<string>(), Is.EqualTo("LengthStandard"));
        var mean = SemanticMetadata.For(typeof(ReviewedBindings).GetProperty("Mean")!)!;
        Assert.That(mean["role"]!.GetValue<string>(), Is.EqualTo(Concepts.ExpectedValue));
        Assert.That(mean["reference"]!.GetValue<string>(), Is.EqualTo(Concepts.Wgs84));
        var sigma = SemanticMetadata.For(typeof(ReviewedBindings).GetProperty("Sigma")!)!;
        Assert.That(sigma.ContainsKey("reference"), Is.False);
        var start = SemanticMetadata.For(typeof(ReviewedBindings).GetProperty("FromDate")!)!;
        Assert.That(start["role"]!.GetValue<string>(), Is.EqualTo(Concepts.ValidityStart));
        Assert.That(start.ContainsKey("physicalQuantity"), Is.False);
    }
}
