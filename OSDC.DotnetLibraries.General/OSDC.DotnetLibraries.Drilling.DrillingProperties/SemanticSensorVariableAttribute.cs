
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticSensorVariableAttribute : Attribute
    {
        public string? MeanVariable { get; set; } = null;
        public string? PrecisionVariable { get; } = null;
        public string? AccuracyVariable { get; } = null;

        public SemanticSensorVariableAttribute(string meanVariable, string precisionVariable, string accuracyVariable)
        {
            MeanVariable = meanVariable;
            PrecisionVariable = precisionVariable;
            AccuracyVariable = accuracyVariable;
        }
    }
}
