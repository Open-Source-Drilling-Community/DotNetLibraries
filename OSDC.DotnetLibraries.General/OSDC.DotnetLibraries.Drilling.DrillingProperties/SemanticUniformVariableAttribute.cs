
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticUniformVariableAttribute : SemanticTwoVariablesAttribute
    {
        public string? MinValueVariable
        {
            get
            {
                return ValueVariable;
            }
        }
        public string? MaxValueVariable
        {
            get
            {
                return SecondValueVariable;
            }
        }

        public SemanticUniformVariableAttribute(string? minValue, string? maxValue) :base(minValue, maxValue)
        {
        }
    }
}
