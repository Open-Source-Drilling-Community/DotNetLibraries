using System.Text.Json;
using NUnit.Framework;
using OSDC.DotnetLibraries.General.DataManagement;

namespace OSDC.DotnetLibraries.General.ResourceClassification.UnitTest;

public class ClassificationTests
{
    private static readonly Guid CategoryId = Guid.Parse("080d1c26-876d-446f-8ab3-0e151e42ce36");
    private static readonly Guid OptionId = Guid.Parse("e48af852-3b03-4daa-bb52-2baf92d9c521");
    private static readonly Dictionary<Guid, HashSet<Guid>> Catalogue = new() { [CategoryId] = [OptionId] };

    [TestCase(null, null, null)]
    [TestCase("", "option", "category_id_required")]
    [TestCase(null, "option", "category_id_required")]
    [TestCase("other", "option", "category_not_found")]
    [TestCase("category", null, "option_id_required")]
    [TestCase("category", "", "option_id_required")]
    [TestCase("category", "other", "option_not_in_category")]
    [TestCase("category", "option", null)]
    public void ReferencesPreserveDraftAndFailureSemantics(string? category, string? option, string? code)
    {
        Guid? Resolve(string? value) => value switch
        {
            null => null, "" => Guid.Empty, "category" => CategoryId, "option" => OptionId,
            _ => Guid.Parse("5c73f3c0-c144-4254-8d09-01a21a8cdb37")
        };
        ClassificationIssue? issue = ClassificationValidation.ValidateCategoryReference(
            Resolve(category), Resolve(option), Catalogue, "Assignments[3]", "MembershipCategoryID", "MembershipOptionID");
        Assert.That(issue?.Code, Is.EqualTo(code));
        if (issue != null) Assert.That(issue.Property, Does.StartWith("Assignments[3].Membership"));
    }

    [Test]
    public void OptionalIdentityReferenceDistinguishesUnlinkedFromEmptyOrUnknown()
    {
        HashSet<Guid> ids = [CategoryId, Guid.Empty];
        Assert.Multiple(() =>
        {
            Assert.That(ClassificationValidation.IsValidOptionalReference(null, ids), Is.True);
            Assert.That(ClassificationValidation.IsValidOptionalReference(CategoryId, ids), Is.True);
            Assert.That(ClassificationValidation.IsValidOptionalReference(Guid.Empty, ids), Is.False);
            Assert.That(ClassificationValidation.IsValidOptionalReference(OptionId, ids), Is.False);
        });
    }

    [Test]
    public void TypedCategoryInterfacePreservesExtensionsAndRoundTripsJson()
    {
        ExtendedOption extended = new() { ID = OptionId, Name = "Oil", Code = "oil" };
        FeatureCategory<ExtendedOption> category = new() { MetaInfo = new(CategoryId), Options = [extended] };
        IFeatureCategory contract = category;
        contract.Options = [extended, new FeatureOption { ID = CategoryId, Name = "Gas" }];
        Assert.That(category.Options![0], Is.SameAs(extended));
        Assert.That(category.Options[1].Name, Is.EqualTo("Gas"));
        var copy = JsonSerializer.Deserialize<FeatureCategory<ExtendedOption>>(JsonSerializer.Serialize(category))!;
        Assert.That(copy.Options![0].Code, Is.EqualTo("oil"));
        Assert.That(copy.MetaInfo!.ID, Is.EqualTo(CategoryId));
        // List adapters are snapshots, just as in the original service models.
        contract.Options!.Clear();
        Assert.That(category.Options, Has.Count.EqualTo(2));
        contract.Options = null;
        Assert.That(category.Options, Is.Null);
    }

    [Test]
    public void MembershipAdapterAndNullVersusEmptyOptionsRemainDistinct()
    {
        MembershipCategory category = new();
        IMembershipCategory contract = category;
        Assert.That(contract.Options, Is.Null);
        contract.Options = [];
        Assert.That(JsonSerializer.Serialize(category), Does.Contain("\"Options\":[]"));
        contract.Options = [new MembershipOption { ID = OptionId, Name = "License" }];
        var copy = JsonSerializer.Deserialize<MembershipCategory>(JsonSerializer.Serialize(category))!;
        Assert.That(copy.Options!.Single().ID, Is.EqualTo(OptionId));
    }

    [Test]
    public void ValidityPeriodsPermitOpenBoundsAndRejectReversedOrUndatedBounds()
    {
        DateTimeOffset now = DateTimeOffset.Parse("2026-09-29T12:00:00Z");
        Assert.Multiple(() =>
        {
            Assert.That(ClassificationValidation.ValidateValidityPeriod(true, null, null, "A"), Is.Null);
            Assert.That(ClassificationValidation.ValidateValidityPeriod(true, null, now, "A"), Is.Null);
            Assert.That(ClassificationValidation.ValidateValidityPeriod(true, now, null, "A"), Is.Null);
            Assert.That(ClassificationValidation.ValidateValidityPeriod(true, now, now, "A"), Is.Null);
            Assert.That(ClassificationValidation.ValidateValidityPeriod(true, now, now.AddDays(-1), "A")?.Code, Is.EqualTo("invalid_validity_period"));
            Assert.That(ClassificationValidation.ValidateValidityPeriod(false, now, null, "A")?.Code, Is.EqualTo("validity_period_not_allowed"));
        });
    }

    [Test]
    public void ExclusivityUsesInclusiveInstantsWithTimeZoneOffsetsAndOpenBounds()
    {
        DateTimeOffset boundary = DateTimeOffset.Parse("2026-09-29T12:00:00Z");
        var touching = new (DateTimeOffset?, DateTimeOffset?)[] { (null, boundary), (boundary.ToOffset(TimeSpan.FromHours(2)), null) };
        Assert.That(ClassificationValidation.FindExclusiveConflicts(true, touching), Is.EqualTo(new[] { (0, 1) }));
        var separate = new (DateTimeOffset?, DateTimeOffset?)[] { (null, boundary), (boundary.AddTicks(1), null) };
        Assert.That(ClassificationValidation.FindExclusiveConflicts(true, separate), Is.Empty);
        Assert.That(ClassificationValidation.FindExclusiveConflicts(false, separate), Has.Count.EqualTo(1));
    }

    [Test]
    public void DuplicateAndEmptyIdsHavePositionAwareErrors()
    {
        var errors = ClassificationValidation.ValidateUniqueIds([CategoryId, Guid.Empty, CategoryId], "Options");
        Assert.That(errors.Select(e => (e.Property, e.Code)), Is.EqualTo(new[]
        {
            ("Options[1].ID", "id_required"), ("Options[2].ID", "duplicate_id")
        }));
    }

    public class ExtendedOption : FeatureOption
    {
        public string? Code { get; set; }
    }
}
