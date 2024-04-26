
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SemanticBernoulliVariableAttribute : Attribute
    {
        public string? Variable { get; } = null;

        public SemanticBernoulliVariableAttribute(string variable)
        {
            Variable = variable;
        }
    }
}
