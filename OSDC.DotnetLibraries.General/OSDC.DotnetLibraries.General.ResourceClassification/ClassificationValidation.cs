namespace OSDC.DotnetLibraries.General.ResourceClassification;

/// <summary>A validation failure that can be mapped into a service's REST/MCP error envelope.</summary>
/// <param name="Property">Caller-supplied property path.</param>
/// <param name="Code">Stable machine-readable failure code.</param>
/// <param name="Message">Human-readable explanation.</param>
public sealed record ClassificationIssue(string Property, string Code, string Message);

/// <summary>Pure validation rules. Catalogue loading, transactions and HTTP status codes belong to the service.</summary>
public static class ClassificationValidation
{
    /// <summary>
    /// Validates a category/option pair against the caller's catalogue snapshot.
    /// Both null references represent an unlinked draft; an empty UUID is never a valid reference.
    /// </summary>
    public static ClassificationIssue? ValidateCategoryReference(Guid? categoryId, Guid? optionId,
        IReadOnlyDictionary<Guid, HashSet<Guid>> optionsByCategory, string path,
        string categoryProperty, string optionProperty)
    {
        if (categoryId == null && optionId == null) return null;
        if (categoryId is not Guid category || category == Guid.Empty)
            return new($"{path}.{categoryProperty}", "category_id_required", "A category UUID is required when an option is selected.");
        if (!optionsByCategory.TryGetValue(category, out HashSet<Guid>? options))
            return new($"{path}.{categoryProperty}", "category_not_found", $"No local category has UUID {category}.");
        if (optionId is not Guid option || option == Guid.Empty)
            return new($"{path}.{optionProperty}", "option_id_required", "An option UUID is required when a category is selected.");
        return options.Contains(option) ? null : new($"{path}.{optionProperty}", "option_not_in_category",
            $"Option UUID {option} does not belong to category UUID {category}.");
    }

    /// <summary>Whether an optional reference is null or identifies a known, non-empty UUID.</summary>
    public static bool IsValidOptionalReference(Guid? id, IReadOnlySet<Guid> knownIds) =>
        id == null || id != Guid.Empty && knownIds.Contains(id.Value);

    /// <summary>
    /// Validates inclusive validity bounds without modifying the assignment.
    /// Categories without validity periods must have null bounds; dated categories allow open bounds.
    /// </summary>
    public static ClassificationIssue? ValidateValidityPeriod(bool hasValidityPeriod,
        DateTimeOffset? fromDate, DateTimeOffset? toDate, string path)
    {
        if (!hasValidityPeriod && (fromDate != null || toDate != null))
            return new(path, "validity_period_not_allowed", "This category does not allow validity bounds.");
        return fromDate > toDate
            ? new($"{path}.FromDate", "invalid_validity_period", "FromDate cannot be after ToDate.") : null;
    }

    /// <summary>
    /// Tests inclusive intervals. Null bounds mean infinity; touching endpoints overlap.
    /// Validate each interval before using it for exclusivity checks.
    /// </summary>
    public static bool IntervalsOverlap(DateTimeOffset? firstStart, DateTimeOffset? firstEnd,
        DateTimeOffset? secondStart, DateTimeOffset? secondEnd) =>
        (firstStart ?? DateTimeOffset.MinValue) <= (secondEnd ?? DateTimeOffset.MaxValue) &&
        (secondStart ?? DateTimeOffset.MinValue) <= (firstEnd ?? DateTimeOffset.MaxValue);

    /// <summary>
    /// Finds conflicting assignment indices for one exclusive category on one resource.
    /// For undated categories, any second assignment conflicts. Same-option duplicates also conflict.
    /// Call only after validating references and validity bounds; non-exclusive categories need no check.
    /// </summary>
    public static IReadOnlyList<(int First, int Second)> FindExclusiveConflicts(
        bool hasValidityPeriod, IReadOnlyList<(DateTimeOffset? FromDate, DateTimeOffset? ToDate)> periods)
    {
        List<(int, int)> conflicts = [];
        for (int first = 0; first < periods.Count; first++)
            for (int second = first + 1; second < periods.Count; second++)
                if (!hasValidityPeriod || IntervalsOverlap(periods[first].FromDate, periods[first].ToDate,
                    periods[second].FromDate, periods[second].ToDate))
                    conflicts.Add((first, second));
        return conflicts;
    }

    /// <summary>
    /// Validates stable identifiers in one collection (definitions, category options or assignments).
    /// Reports empty IDs and subsequent occurrences of duplicate IDs without rewriting them.
    /// </summary>
    public static IReadOnlyList<ClassificationIssue> ValidateUniqueIds(IEnumerable<Guid> ids, string path)
    {
        HashSet<Guid> seen = [];
        List<ClassificationIssue> issues = [];
        int index = 0;
        foreach (Guid id in ids)
        {
            string property = $"{path}[{index++}].ID";
            if (id == Guid.Empty) issues.Add(new(property, "id_required", "A non-empty UUID is required."));
            else if (!seen.Add(id)) issues.Add(new(property, "duplicate_id", $"UUID {id} occurs more than once."));
        }
        return issues;
    }
}
