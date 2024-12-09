
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticGaussianVariableAttribute : Attribute
    {
        public string? MeanVariable { get; } = null;
        public string? StandardDeviationVariable { get; } = null;
        public double? DefaultStandardDeviation { get; } = null;

        public SemanticGaussianVariableAttribute(string? mean, string? standardDeviation)
        {
            MeanVariable = mean;
            StandardDeviationVariable = standardDeviation;
        }

        public SemanticGaussianVariableAttribute(string? mean, string? standardDeviation, double defaultStandardDeviation)
        {
            MeanVariable = mean;
            StandardDeviationVariable = standardDeviation;
            DefaultStandardDeviation = defaultStandardDeviation;
        }
    }
}
