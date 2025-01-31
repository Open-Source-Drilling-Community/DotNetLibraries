
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticBernoulliVariableAttribute : SemanticOneVariableAttribute
    {
        public SemanticBernoulliVariableAttribute(string variable) : base(variable)
        {
        }
    }
}
