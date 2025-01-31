
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticDiracVariableAttribute : SemanticOneVariableAttribute
    {
        public SemanticDiracVariableAttribute(string? value) : base(value)
        {
        }
    }
}
