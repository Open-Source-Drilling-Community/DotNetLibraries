
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticTwoVariablesAttribute : SemanticOneVariableAttribute
    {
        public virtual string? SecondValueVariable { get; } = null;

        public SemanticTwoVariablesAttribute(string? value, string? secondValue) : base(value)
        {
            SecondValueVariable = secondValue;
        }
    }
}
