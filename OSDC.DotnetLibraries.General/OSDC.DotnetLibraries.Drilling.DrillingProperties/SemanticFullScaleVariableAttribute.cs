
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticFullScaleVariableAttribute : SemanticThreeVariablesAttribute
    {
        public string? MeanVariable
        {
            get
            {
                return ValueVariable;
            }
        }
        public string? FullScaleVariable
        {
            get
            {
                return SecondValueVariable;
            }
        }
        public string? ProportionErrorVariable
        {
            get
            {
                return ThirdValueVariable;
            }
        }

        public SemanticFullScaleVariableAttribute(string meanVariable, string fullScaleVariable, string proportionErrorVariable) : base(meanVariable, fullScaleVariable, proportionErrorVariable)
        {
        }
    }
}
