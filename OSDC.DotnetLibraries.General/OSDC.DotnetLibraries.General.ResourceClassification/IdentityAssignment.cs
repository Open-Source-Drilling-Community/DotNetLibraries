using OSDC.DotnetLibraries.General.DataManagement;

namespace OSDC.DotnetLibraries.General.ResourceClassification;

/// <summary>The value of an identification scheme for one resource.</summary>
public class IdentityAssignment : IIdentityAssignment
{
    /// <summary>Stable UUID of this assignment, distinct from the definition UUID.</summary>
    public Guid ID { get; set; }

    /// <summary>Reference to the identity definition. Null represents an unlinked draft.</summary>
    public Guid? IdentityID { get; set; }

    /// <summary>Resource-specific value in the selected identification scheme.</summary>
    public string? Value { get; set; }
}
