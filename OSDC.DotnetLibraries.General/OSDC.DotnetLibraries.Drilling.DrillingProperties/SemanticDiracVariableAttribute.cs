
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticDiracVariableAttribute : Attribute
    {
        public string? ValueVariable { get; } = null;

        public SemanticDiracVariableAttribute(string? value)
        {
            ValueVariable = value;
        }
    }
}
