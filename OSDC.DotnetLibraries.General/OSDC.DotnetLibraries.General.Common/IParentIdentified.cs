
namespace OSDC.DotnetLibraries.General.Common
{
    public interface IParentIdentified
    {
        /// <summary>
        /// the ID of a parent
        /// </summary>
        Guid ParentID { get; set; }
    }
}
