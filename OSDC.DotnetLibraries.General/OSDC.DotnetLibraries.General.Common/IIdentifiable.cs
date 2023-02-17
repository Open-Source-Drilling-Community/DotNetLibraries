
namespace OSDC.DotnetLibraries.General.Common
{
    /// <summary>
    /// an interface for things that can be identified
    /// </summary>
    public interface IIdentifiable
    {
        Guid ID { get; set; }
    }
}
