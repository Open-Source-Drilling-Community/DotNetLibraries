using System.Text.Json;
using System.Text.Encodings.Web;
using System.Security.Cryptography;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.SemanticCatalogue.UnitTest;

public class WellWellBoreVocabularyTests
{
    [Test]
    public void ReviewedIncrementPreservesPublishedDefinitions()
    {
        var current = Catalogue.Default;
        Assert.That(current.Document.Version, Is.EqualTo("0.14.0"));
        Assert.That(current.Document.Concepts.Count, Is.EqualTo(577));
        Assert.That(current.Document.Concepts.All(c => c.Status is CurationStatus.Reviewed or CurationStatus.Deprecated), Is.True);
        var assembly = typeof(Catalogue).Assembly;
        using var stream = assembly.GetManifestResourceStream(
            assembly.GetManifestResourceNames().Single(x => x.EndsWith(".catalogue.json")))!;
        using var json = JsonDocument.Parse(stream);
        var original = json.RootElement.GetProperty("concepts").EnumerateArray().Take(208).ToArray();
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(original,
            new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        Assert.That(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
            Is.EqualTo("700472fc72f287cc5fc3c7156a98dfabb5eb257f0df717cbd14c3303a61baf1c"),
            "The 208 published 0.7.0 entries must remain unchanged.");
        Assert.That(current.Document.Concepts.Skip(208).All(c => c.Evidence.Count > 0), Is.True);
    }

    [Test]
    public void MeasuredDepthIsParentPathCoordinateWithoutVerticalDatumAssertion()
    {
        var c = Catalogue.Default;
        Assert.That(c.IsA(Concepts.TieInMeasuredDepth, Concepts.CurvilinearAbscissa), Is.True);
        Assert.That(c.IsA(Concepts.MeasuredDepth, Concepts.Depth), Is.True);
        Assert.That(c.IsA(Concepts.MeasuredDepth, Concepts.EllipsoidalDepth), Is.False);
        Assert.That(c.Quantity(Concepts.TieInMeasuredDepth)!.Name, Is.EqualTo("DepthDrilling"));
        Assert.That(c.SiUnit(Concepts.TieInMeasuredDepth), Is.EqualTo("m"));
        Assert.That(c.RequiredContext(Concepts.TieInMeasuredDepth), Does.Contain("parent wellbore"));
        Assert.That(c.RequiredContext(Concepts.TieInMeasuredDepth), Does.Contain("MD origin/reference"));
        Assert.That(c.Quantity(Concepts.LinearStandardUncertainty)!.Name, Is.EqualTo("LengthStandard"));
        Assert.That(c.Quantity(Concepts.DrillFloorDepth)!.Name, Is.EqualTo("DepthDrilling"));
    }

    [Test]
    public void JobsAndResourcesDoNotInheritTheirAssociatedObjects()
    {
        var c = Catalogue.Default;
        Assert.That(c.IsA(Concepts.SidetrackWellBore, Concepts.WellBore), Is.True);
        Assert.That(c.IsA(Concepts.WellBore, Concepts.Well), Is.False);
        Assert.That(c.IsA(Concepts.RigJob, Concepts.Rig), Is.False);
        Assert.That(c.IsA(Concepts.RigJob, Concepts.Resource), Is.True);
        Assert.That(c.Quantity(Concepts.DrillFloorDepthSource), Is.Null);
    }

    [Test]
    public void ExclusiveJobEndDoesNotReuseInclusiveAssignmentEnd()
    {
        var c = Catalogue.Default;
        Assert.That(c.Get(Concepts.RigJobStart).Kind, Is.EqualTo(SemanticKind.Role));
        Assert.That(c.Get(Concepts.RigJobEnd).Kind, Is.EqualTo(SemanticKind.Role));
        Assert.That(c.IsA(Concepts.RigJobEnd, Concepts.ValidityEnd), Is.False);
        Assert.That(c.IsA(Concepts.RigJobPeriod, Concepts.AssignmentValidityPeriod), Is.False);
        Assert.That(c.Get(Concepts.RigJobEnd).Definition, Does.Contain("Exclusive"));
        Assert.That(c.Get(Concepts.ValidityEnd).Definition, Does.Contain("Inclusive"));
    }
}
