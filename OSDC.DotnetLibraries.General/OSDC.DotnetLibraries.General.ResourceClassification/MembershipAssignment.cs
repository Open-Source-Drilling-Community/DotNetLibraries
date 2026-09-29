using OSDC.DotnetLibraries.General.DataManagement;

namespace OSDC.DotnetLibraries.General.ResourceClassification;

/// <summary>An assignment of a membership category option to one resource.</summary>
public class MembershipAssignment : IMembershipAssignment
{
    /// <summary>Stable UUID of the assignment.</summary>
    public Guid ID { get; set; }

    /// <summary>Reference to the owning category; null is allowed for an unlinked draft.</summary>
    public Guid? MembershipCategoryID { get; set; }

    /// <summary>Reference to an option within the selected category.</summary>
    public Guid? MembershipOptionID { get; set; }

    /// <summary>Inclusive start of validity; null means no lower bound.</summary>
    public DateTimeOffset? FromDate { get; set; }

    /// <summary>Inclusive end of validity; null means no upper bound.</summary>
    public DateTimeOffset? ToDate { get; set; }
}
