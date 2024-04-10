
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
    public class SemanticFullScaleVariableAttribute : Attribute
    {
        public string? MeanVariable { get; } = null;
        public string? FullScaleVariable { get; } = null;
        public string? ProportionErrorVariable { get; } = null;
        public double? DefaultFullScale { get; } = null;
        public double? DefaultProportionError { get; } = null;

        public SemanticFullScaleVariableAttribute(string meanVariable, string fullScaleVariable, string proportionErrorVariable)
        {
            MeanVariable = meanVariable;
            FullScaleVariable = fullScaleVariable;
            ProportionErrorVariable = proportionErrorVariable;
        }

        public SemanticFullScaleVariableAttribute(string meanVariable, string fullScaleVariable, double defaultFullScale, string proportionErrorVariable)
        {
            MeanVariable = meanVariable;
            FullScaleVariable = fullScaleVariable;
            ProportionErrorVariable = proportionErrorVariable;
            DefaultFullScale = defaultFullScale;
        }

        public SemanticFullScaleVariableAttribute(string meanVariable, string fullScaleVariable, string proportionErrorVariable, double defaultProportionError)
        {
            MeanVariable = meanVariable;
            FullScaleVariable = fullScaleVariable;
            ProportionErrorVariable = proportionErrorVariable;
            DefaultProportionError = defaultProportionError;
        }

        public SemanticFullScaleVariableAttribute(string meanVariable, string fullScaleVariable, double defaultFullScale, string proportionErrorVariable, double defaultProportionError)
        {
            MeanVariable = meanVariable;
            FullScaleVariable = fullScaleVariable;
            ProportionErrorVariable = proportionErrorVariable;
            DefaultFullScale = defaultFullScale;
            DefaultProportionError = defaultProportionError;
        }
    }
}
