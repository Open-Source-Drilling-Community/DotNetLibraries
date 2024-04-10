
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
    public class SemanticDiracVariableAttribute : Attribute
    {
        public string? ValueVariable { get; } = null;

        public SemanticDiracVariableAttribute(string? value)
        {
            ValueVariable = value;
        }
    }
}
