
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticSensorVariableAttribute : SemanticThreeVariablesAttribute
    {
        public string? MeanVariable
        {
            get
            {
                return ValueVariable;
            }
        }
        public string? PrecisionVariable
        {
            get
            {
                return SecondValueVariable;
            }
        }
        public string? AccuracyVariable
        {
            get
            {
                return ThirdValueVariable;
            }
        }

        public SemanticSensorVariableAttribute(string meanVariable, string precisionVariable, string accuracyVariable) :base(meanVariable, precisionVariable, accuracyVariable)
        {
        }
    }
}
