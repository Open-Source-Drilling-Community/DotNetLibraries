using OSDC.DotnetLibraries.General.DataManagement;

namespace OSDC.DotnetLibraries.General.ResourceClassification;

/// <summary>A selectable option belonging to a membership category.</summary>
public class MembershipOption : IMembershipOption
{
    /// <summary>Stable UUID of the option within its category.</summary>
    public Guid ID { get; set; }

    /// <summary>Display name of this option.</summary>
    public string? Name { get; set; }
}
