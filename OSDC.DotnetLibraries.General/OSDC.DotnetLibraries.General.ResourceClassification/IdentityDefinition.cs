using OSDC.DotnetLibraries.General.DataManagement;

namespace OSDC.DotnetLibraries.General.ResourceClassification;

/// <summary>A named identification scheme. Its UUID identifies the definition, not the resource.</summary>
public class IdentityDefinition : IIdentity
{
    /// <summary>Metadata and stable UUID of this identity definition.</summary>
    public MetaInfo? MetaInfo { get; set; }

    /// <summary>Display name of the identification scheme.</summary>
    public string? Name { get; set; }

    /// <summary>Creation instant, assigned by the owning service.</summary>
    public DateTimeOffset? CreationDate { get; set; }

    /// <summary>Last modification instant, assigned by the owning service.</summary>
    public DateTimeOffset? LastModificationDate { get; set; }
}
