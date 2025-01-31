
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticGaussianVariableAttribute : SemanticTwoVariablesAttribute
    {
        public string? MeanVariable
        {
            get
            {
                return ValueVariable;
            }
        }
        public string? StandardDeviationVariable
        {
            get
            {
                return SecondValueVariable;
            }
        }
        public double? DefaultStandardDeviation { get; } = null;

        public SemanticGaussianVariableAttribute(string? mean, string? standardDeviation) : base(mean, standardDeviation)
        {
        }

        public SemanticGaussianVariableAttribute(string? mean, string? standardDeviation, double defaultStandardDeviation) : base(mean, standardDeviation)
        {
            DefaultStandardDeviation = defaultStandardDeviation;
        }
    }
}
