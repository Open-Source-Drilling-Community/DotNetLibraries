
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticGeneralDistributionVariableAttribute : Attribute
    {
        public string? HistogramVariable { get; }

        public SemanticGeneralDistributionVariableAttribute(string? histogram)
        {
            HistogramVariable = histogram;
        }
    }
}
