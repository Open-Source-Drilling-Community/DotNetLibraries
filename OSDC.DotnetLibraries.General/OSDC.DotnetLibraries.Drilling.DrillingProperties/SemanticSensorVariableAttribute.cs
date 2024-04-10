
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
    public class SemanticSensorVariableAttribute : Attribute
    {
        public string? MeanVariable { get; set; } = null;
        public string? PrecisionVariable { get; } = null;
        public string? AccuracyVariable { get; } = null;
        public double? DefaultPrecision {  get; } = null;
        public double? DefaultAccuracy { get; } = null;

        public SemanticSensorVariableAttribute(string meanVariable, string precisionVariable, string accuracyVariable)
        {
            MeanVariable = meanVariable;
            PrecisionVariable = precisionVariable;
            AccuracyVariable = accuracyVariable;
        }

        public SemanticSensorVariableAttribute(string meanVariable, string precisionVariable, double defaultPrecision, string accuracyVariable)
        {
            MeanVariable = meanVariable;
            PrecisionVariable = precisionVariable;
            AccuracyVariable = accuracyVariable;
            DefaultPrecision = defaultPrecision;
        }

        public SemanticSensorVariableAttribute(string meanVariable, string precisionVariable, string accuracyVariable, double defaultAccuracy)
        {
            MeanVariable = meanVariable;
            PrecisionVariable = precisionVariable;
            AccuracyVariable = accuracyVariable;
            DefaultAccuracy = defaultAccuracy;
        }

        public SemanticSensorVariableAttribute(string meanVariable, string precisionVariable, double defaultPrecision, string accuracyVariable, double defaultAccuracy)
        {
            MeanVariable = meanVariable;
            PrecisionVariable = precisionVariable;
            AccuracyVariable = accuracyVariable;
            DefaultPrecision = defaultPrecision;
            DefaultAccuracy = defaultAccuracy;
        }
    }
}
