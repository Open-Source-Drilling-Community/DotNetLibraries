using OSDC.DotnetLibraries.General.DataManagement;

namespace OSDC.DotnetLibraries.General.ResourceClassification;

/// <summary>A membership category using the standard option model.</summary>
public class MembershipCategory : MembershipCategory<MembershipOption> { }

/// <summary>A membership category with a service-specific concrete option type for JSON serialization.</summary>
/// <typeparam name="TOption">The option type owned by the service contract.</typeparam>
public class MembershipCategory<TOption> : IMembershipCategory where TOption : class, IMembershipOption, new()
{
    /// <summary>Metadata and stable category UUID.</summary>
    public MetaInfo? MetaInfo { get; set; }

    /// <summary>Display name of the category.</summary>
    public string? Name { get; set; }

    /// <summary>Whether at most one assignment may be active at a given instant.</summary>
    public bool IsExclusive { get; set; }

    /// <summary>Whether assignments may have validity bounds. Null bounds are unbounded.</summary>
    public bool HasValidityPeriod { get; set; }

    /// <summary>Options owned by this category. Null and an empty list retain distinct JSON representations.</summary>
    public List<TOption>? Options { get; set; }

    // The interface exposes a list snapshot, matching the pre-existing service adapters.
    // Compatible options retain their concrete type and additional properties.
    List<IMembershipOption>? IMembershipCategory.Options
    {
        get => Options?.Cast<IMembershipOption>().ToList();
        set => Options = value?.Select(option => option is TOption typed
            ? typed : new TOption { ID = option.ID, Name = option.Name }).ToList();
    }

    /// <summary>Creation instant, assigned by the owning service.</summary>
    public DateTimeOffset? CreationDate { get; set; }

    /// <summary>Last modification instant, assigned by the owning service.</summary>
    public DateTimeOffset? LastModificationDate { get; set; }
}
