
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
    public class SemanticGeneralDistributionVariableAttribute : Attribute
    {
        public string? HistogramVariable { get; }

        public SemanticGeneralDistributionVariableAttribute(string? histogram)
        {
            HistogramVariable = histogram;
        }
    }
}
