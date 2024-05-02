
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SemanticFullScaleVariableAttribute : Attribute
    {
        public string? MeanVariable { get; } = null;
        public string? FullScaleVariable { get; } = null;
        public string? ProportionErrorVariable { get; } = null;

        public SemanticFullScaleVariableAttribute(string meanVariable, string fullScaleVariable, string proportionErrorVariable)
        {
            MeanVariable = meanVariable;
            FullScaleVariable = fullScaleVariable;
            ProportionErrorVariable = proportionErrorVariable;
        }
    }
}
